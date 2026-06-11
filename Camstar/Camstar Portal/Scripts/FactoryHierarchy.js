// Copyright Siemens 2024

// Client side code to handle the Factory Hierarchy Model (FHM) Page.
var factoryHierarchy = (function () {
    'use strict';

    const _cachedMessageTextKey = "fhmMessageTextKey"
    const _cachedMessageTypeKey = "fhmMessageTypeKey"

    var operation = 0;

    // localization labels
    var _localizedLabels = null;

    // object level variable
    var _treeControlId = "fhmTreeContainer";

    // function to call after any tab has been removed
    var _tabRemovedCallback = null;

    var IDS = {
        TAB_CONTAINER: 'ctl00_WebPartManager_ModelingTabWP_TabContainer'
    };

    // SELECTORS
    var SELECTORS = {
        TREE_DATA: '#ctl00_WebPartManager_FHMTreeWP_treeData_ctl00',
        TREE_DEFINITION: "#ctl00_WebPartManager_FHMTreeWP_treeDefinition_ctl00",
        TREE_CONTAINER: "#" + _treeControlId,
        SEARCH_CLEAR_BUTTON: "#searchClearButton",
        SEARCH_TREE_CRITERIA_INPUT: "#searchTreeCriteria",
        SEARCH_EXECUTION_BUTTON: "#searchExecuteButton",
        TAB_CONTAINER: '#' + IDS.TAB_CONTAINER,
        TABS: '#' + IDS.TAB_CONTAINER + ' ul#tablist > li.ui-tab',
        TAB_CLOSE_BUTTONS: '#' + IDS.TAB_CONTAINER + ' ul#tablist > li.ui-tab a.ui-tabs-close',
        COMMAND_ADD_NEW_BUTTON: "#addTreeNodeButton",
        COMMAND_ADD_NEW_LIST: "#commandAddNewNodeList",
        COMMAND_IMPORT_LIST: "#commandImportList",
        COMMAND_BAR: '.tree-control-command-bar',
        EXPAND_TREE_BUTTON: "#expandButton",
        COLLAPSE_TREE_BUTTON: "#collapseButton",
        LINK_CHILDREN_TREE_BUTTON: "#linkChildrenButton",
        WEB_PART: '#WebPart_FHMTreeWP',
        IMPORT_BUTTON: "#importButton",
        VALOR_EXPORT_FILE: "#valorExportFileInput",
        SRC_EXPORT_FILE: '#srcExportFileInput',
        IMPORT_TREE_DATA: '#ctl00_WebPartManager_FHMTreeWP_importTreeData_ctl00',
        IMPORT_FILENAME: '#ctl00_WebPartManager_FHMTreeWP_importFileName_ctl00',
        IMPORT_PREVIEW_POPUP_BUTTON: '#ctl00_WebPartManager_FHMTreeWP_btnImportPopup',
        IMPORT_TYPE: '#ctl00_WebPartManager_FHMTreeWP_importType_ctl00'
    };

    var MODELING_OPERATIONS = {
        EDIT: 0,
        ADD_CHILD: 1,
        ADD_SIBLING: 2,
        COPY: 3,
        LINK_CHILDREN: 4
    };

    // Public functions interface
    var factoryHierarchyModelInterface = {
        initialize: initialize,
        setPageLoadedMessage: setPageLoadedMessage,
        importComplete: importComplete
    };

    $(function () {
        // Height of content - excluding padding
        let wpHeight = $(SELECTORS.WEB_PART).height();
        // Height including padding, border, margin
        let cmdBarHeight = $(SELECTORS.COMMAND_BAR).outerHeight(true);
        // make tree and its container fill the remaining web part height
        let treeHeight = wpHeight - cmdBarHeight;
        $(SELECTORS.TREE_CONTAINER).outerHeight(treeHeight, true);
        treeControl.setHeight(treeHeight);

        showPageLoadedMessage();
    });

    // show user list of sources they may import from - Valor or SRC
    function importButtonClicked() {
        let $importList = $(SELECTORS.COMMAND_IMPORT_LIST);

        if ($importList.is(':hidden')) {
            // rebuild list of import options
            $importList.html("");
            $(SELECTORS.IMPORT_BUTTON).addClass("command-button-selected");

            // SRC always available
            let srcLabelText = 'SRC';  // TODO - localize
            let $srcOption = $(`<div class='command-bar-dropdown-item' >${srcLabelText}</div>`);
            $srcOption.click(function () {
                $importList.hide();
                $(SELECTORS.SRC_EXPORT_FILE).click();
            });
            $importList.append($srcOption);

            // Valor available when an Area is selected
            var selNodeData = treeControl.getSelectedNodeData();
            if (selNodeData && selNodeData.dataType.name === 'Area') {
                let valorLabelText = 'Valor';  // TODO - localize
                let $valorOption = $(`<div class='command-bar-dropdown-item' >${valorLabelText}</div>`);
                $valorOption.click(function () {
                    $importList.hide();
                    $(SELECTORS.VALOR_EXPORT_FILE).click();
                });
                $importList.append($valorOption);
            }

            $('html').one('click', function () { closeDropdown(SELECTORS.COMMAND_IMPORT_LIST, SELECTORS.IMPORT_BUTTON); });

            $importList.show();

            CR.Event.stopEvent(event);
        } else {
            closeDropdown(SELECTORS.COMMAND_IMPORT_LIST, SELECTORS.IMPORT_BUTTON);
        }
    }

    // "Add" button clicked - show user list of options (types of objects to add)
    function addButtonClicked() {
        var $treeAddNewNodeList = $(SELECTORS.COMMAND_ADD_NEW_LIST);

        if ($treeAddNewNodeList.is(":hidden")) {
            // rebuild list of new node options
            $treeAddNewNodeList.html("");
            $(SELECTORS.COMMAND_ADD_NEW_BUTTON).addClass("command-button-selected");

            // ask tree what kind of collections (types) we can create
            var newNodeCollectionDefs = treeControl.getNewNodeCollectionDefs();
            $.each(newNodeCollectionDefs, function (index, collectionDef) {
                var title = collectionDef.dataType.title;
                let collectionTypeName = collectionDef.getCollectionName();

                // create elem and attach click handler
                var $newOption = $("<div class='command-bar-dropdown-item' data-collection-type='" + collectionTypeName + "'>" + title + "</div>");
                $newOption.click(function () {
                    var collectionDefinition = treeCollectionDefinition.getDefinition(collectionTypeName);
                    $treeAddNewNodeList.hide();
                    addNewNode(collectionDefinition);
                });
                $treeAddNewNodeList.append($newOption);
            });

            $('html').one('click', function () { closeDropdown(SELECTORS.COMMAND_ADD_NEW_LIST, SELECTORS.COMMAND_ADD_NEW_BUTTON); });

            $treeAddNewNodeList.show();

            CR.Event.stopEvent(event);
        } else {
            closeDropdown(SELECTORS.COMMAND_ADD_NEW_LIST, SELECTORS.COMMAND_ADD_NEW_BUTTON);
        }
    }

    // listSelector -
    function closeDropdown(listSelector, buttonSelector) {
        $(listSelector).hide();
        $(buttonSelector).removeClass("command-button-selected");

        $('html').off('click', closeDropdown);
    }

    // The server code will call this during the page's initialization. This function will then do any setup of the page that is needed.
    function initialize(srcApiUrl, translationsDocument) {

        var treeConfig = {
            "rootCollectionName": "Enterprises",
            "defaultTitleTemplate": "",
            "fieldMappings": {
                "id": "ID",
                "name": "Name",
                "parentIdForUpdate": "",
                "dataTypeForUpdate": ""
            }
        };

        if (!srcApiUrl)
            showCommandButton(SELECTORS.IMPORT_BUTTON, false);

        showCommandButton(SELECTORS.LINK_CHILDREN_TREE_BUTTON, false);
        showCommandButton(SELECTORS.SEARCH_CLEAR_BUTTON, false);
        showCommandButton(SELECTORS.COLLAPSE_TREE_BUTTON, false);

        treeControl.setEventHandlers(onItemSelected, onItemSelecting);
        treeControl.createTree(getTreeData(), getTreeDefinition(), treeConfig, _treeControlId);

        setTranslations(translationsDocument);
        bindEventHandlers();
        
    }

    // The Valor and SRC imports popups reloads the page after a successfull import. Because of
    // the reload, any status message will not be shown. A workaround is needed. The approach
    // here is to store the import window success message in the browser's session storage.
    // Then after the page reloads this function will display a messageusing the 
    // session value. After that delete it.
    function showPageLoadedMessage() {
        var timeOut = 0;
        var messageType = "Success";
        var message = localStorage.getItem(_cachedMessageTextKey);

        if (message) {
            setTimeout(function () {
                if (_cachedMessageTypeKey)
                    messageType = localStorage.getItem(_cachedMessageTypeKey);

                __page.displayStatus(message, "Success");

                clearPageLoadedMessage();
            }, timeOut);
        }

    }

    // Set a message to display after the page loads. This can be used
    // to set a message before reloading the page. After the page reloads
    // the message set here will be displayed
    function setPageLoadedMessage(message, type) {
        if (!type)
            type = "Success"

        localStorage.setItem(_cachedMessageTextKey, message);
        localStorage.setItem(_cachedMessageTypeKey, type);        
    }

    // After a FHM import operation is complete, handle the logic needed to for this page.
    // The post import process is to display a loading icon, set a success message to be 
    // displayed after the page is reloaded, and then reload the page.
    function importComplete(successMessage) {
        // Display the loading icon after the import is complete. There can be a delay before
        // the window starts the reload process. Therefore use the load icon to communicate to
        // the user to wait. We do not need to hide the processing indicator because
        // it will disappear with the page reload.
        displayProcessingIndicator(true);

        // set a message to be dispayed after the reload.
        setPageLoadedMessage(successMessage);

        // Now reload the page.
        window.location.reload();
    }

    function clearPageLoadedMessage() {
        localStorage.removeItem(_cachedMessageTextKey);
        localStorage.removeItem(_cachedMessageTypeKey);
    }

    function enableCommandButton(buttonSelector, isEnabled) {
        if (isEnabled)
            $(buttonSelector).removeClass("command-button-disabled");
        else
            $(buttonSelector).addClass("command-button-disabled");
    }

    function showCommandButton(buttonSelector, isVisible) {
        let $button = $(buttonSelector);

        if (isVisible)
            $button.show();
        else
            $button.hide();

        return $button;
    }

    function expandTree() {
        showCommandButton(SELECTORS.EXPAND_TREE_BUTTON, false);
        showCommandButton(SELECTORS.COLLAPSE_TREE_BUTTON, true).focus();

        treeControl.expandAll();

        CR.Event.stopEvent(event);
    }

    function collapseTree() {
        showCommandButton(SELECTORS.COLLAPSE_TREE_BUTTON, false);
        showCommandButton(SELECTORS.EXPAND_TREE_BUTTON, true).focus();

        treeControl.collapseAll();

        CR.Event.stopEvent(event);
    }

    function enableTree(isEnabled) {
        treeControl.enableTree(isEnabled);

        if (isEnabled)
            $(".tree-control-command-bar").removeClass("command-bar-disabled");
        else
            $(".tree-control-command-bar").addClass("command-bar-disabled");
    }

    function anyTabsOpen() {
        return $(SELECTORS.TABS).length > 0;
    }

    function closeAllTabs() {
        $(SELECTORS.TAB_CLOSE_BUTTONS).click();
    }
    function closeOnDelete() {
        let stat = $('.webpart-status', $('.cs-tab-pages').find('iframe')[0].document);
        if (stat.length) {
            let msg = $find('WebPart_StatusBar_UIComponent');
            if (msg) {
                let text = $('#SpanStatusMessage', stat[0]).text().replace('SUCCESS!', '').trim();                   
                msg.write(text, 'Success');
            }
        }
        closeAllTabs();
    }

    // Close any open tabs and then call the given function
    function closeTabsAndProceed(callbackAfterTabsClosed) {
        if (anyTabsOpen()) {
            // do this when tab closes
            _tabRemovedCallback = function () {
                _tabRemovedCallback = null;
                callbackAfterTabsClosed();
            }

            // click tabs close
            closeAllTabs();
        } else {
            callbackAfterTabsClosed();
        }
    }

    // callback from tree
    function onItemSelecting(selectCallback) {
        closeTabsAndProceed(selectCallback);
    }

    // callback from tree
    function onItemSelected(selectedNodeData, selectedCollectionDefinition) {
        var selNodeData = treeControl.getSelectedNodeData();
        var customData = selectedCollectionDefinition.customData;
        var linkPopupConfig = customData.LinkItemPopupConfiguration;

        showCommandButton(SELECTORS.LINK_CHILDREN_TREE_BUTTON, linkPopupConfig != null);

        setTimeout(() => {
            var queryString = getModelingQueryString(selectedCollectionDefinition, selectedNodeData);
            var callStackKey = OpenModelingPageWitinTab(IDS.TAB_CONTAINER, customData.ModelingPage, customData.CDOTitle, queryString);

            modelingOpened(callStackKey, selectedCollectionDefinition.customData, selNodeData.dataType);
        }, 0);
    }

    // Open modeling page to add new node
    // return - call stack key
    function addNewNode(newNodeCollectionDef) {
        closeTabsAndProceed(function () {
            // assume we are creating a sibling or child of the SELECTED node
            var selNodeData = treeControl.getSelectedNodeData();
            var newNodeCustomData = newNodeCollectionDef.customData;
            var parentNodeName = selNodeData && selNodeData.hasParentNode() ? selNodeData.getParentNode().getName() : "";
            var newNodeTypeName = newNodeCollectionDef.dataType.name;

            //showCommandButton(SELECTORS.IMPORT_BUTTON, false);

            let sameTypeAsSelected = !selNodeData || newNodeTypeName === selNodeData.dataType.name;

            // creating a new node of the same type as selected?
            var parentNodeName = '';
            if (sameTypeAsSelected) {
                // sibling - so parent is same as selected node
                parentNodeName = (selNodeData && selNodeData.hasParentNode()) ? selNodeData.getParentNode().getName() : "";
            } else {
                // child - so parent is the selected node
                parentNodeName = selNodeData.getName();
            }

            var paramsForNew = {
                IsNew: 'true',
                ParentTitle: parentNodeName,
                NodeDefinition: newNodeTypeName
            };
            var queryString = getModelingQueryString(newNodeCollectionDef, null, paramsForNew);

            var callStackKey = OpenModelingPageWitinTab(IDS.TAB_CONTAINER, newNodeCustomData.ModelingPage, newNodeCustomData.CDOTitle, queryString);
            operation = sameTypeAsSelected ? MODELING_OPERATIONS.ADD_SIBLING : MODELING_OPERATIONS.ADD_CHILD;

            modelingOpened(callStackKey, newNodeCollectionDef.customData, newNodeCollectionDef.dataType);
        });
    }

    // modeling page has been opened, hook up listeners
    function modelingOpened(callStackKey, customData, dataType) {
        // tab closed event
        $('.ui-page-tab').on('tabRemoved.fht', function (ev, detail) {
            if (detail && detail.callStackKey && detail.callStackKey === callStackKey) {
                // remove event handlers
                $('.ui-page-tab').off('tabRemoved.fht');
                $('body')[0].removeEventListener('maintSubmitSuccess', maintSubmitSuccess);
                $('body')[0].removeEventListener('maintDeleteSuccess', maintDeleteSuccess);
                $('body')[0].removeEventListener('maintCopySuccess', maintCopySuccess);

                if (_tabRemovedCallback) {
                    _tabRemovedCallback();
                }
            }
        });

        // User successfully submitted creation or update
        $('body')[0].addEventListener('maintSubmitSuccess', maintSubmitSuccess);

        // get updated data and tell tree
        function maintSubmitSuccess(ev) {
            updateCdo(ev.detail.callStackKey, ev.detail.id);
        }

        $('body')[0].addEventListener('maintDeleteSuccess', maintDeleteSuccess);

        // tell tree that the node is gone
        function maintDeleteSuccess(ev) {
            treeControl.deleteSelectedTreeNode();
            // delay closing tabs so the Delete success message can pop up
            setTimeout(closeOnDelete, 5);
        }

        $('body')[0].addEventListener('maintCopySuccess', maintCopySuccess);

        // load new node info and update tree
        function maintCopySuccess(ev) {
            operation = MODELING_OPERATIONS.ADD_SIBLING;
            updateCdo(ev.detail.callStackKey, ev.detail.id);
        }

        function updateCdo(callStackKeyUpdated, cdoInstanceId) {
            // callStackKeyUpdated - callstack key of the tab where the update happened
            if (callStackKeyUpdated !== callStackKey)
                return;

            getCdoDataForTree(customData, null, cdoInstanceId, dataType);
        }

    }

    function getCdoDataForTree(serviceConfig, childOperation, cdoInstanceId, dataType, sync) {
        let svcParams = {};
        let currentOperation = childOperation ?? operation;
        svcParams[serviceConfig.GetOperation.paramName] = cdoInstanceId;

        $.ajax({
            type: "POST",
            dataType: "json",
            url: './FactoryHierarchyService.svc/web/' + serviceConfig.GetOperation.operationName,
            headers: {
                'Accept': 'application/json'
            },
            contentType: "application/json;charset=UTF-8",
            async: sync ? false : true,
            data: JSON.stringify(svcParams),
            context: document.body
        })
            .success(getCdoDataSuccess)
            .fail(ajaxCallFailure);

        // Retrieve a record from a CDO and add or update a node in the tree with that data
        function getCdoDataSuccess(response) {
            let savedObj = JSON.parse(response[serviceConfig.GetOperation.responseFieldName]);

            if (currentOperation === MODELING_OPERATIONS.EDIT) {
                treeControl.updateSelectedTreeNode(savedObj);
            } else if (currentOperation === MODELING_OPERATIONS.ADD_CHILD) {
                treeControl.addNode(savedObj, false, dataType, true);
                // after saving the new node, we are now editing it
                currentOperation = MODELING_OPERATIONS.EDIT;
            } else if (currentOperation === MODELING_OPERATIONS.ADD_SIBLING || currentOperation === MODELING_OPERATIONS.COPY) {
                treeControl.addNode(savedObj, true, dataType, true);
                currentOperation = MODELING_OPERATIONS.EDIT;
            } else if (currentOperation === MODELING_OPERATIONS.LINK_CHILDREN) {
                // Link an existing record to the FDM.
                treeControl.addNode(savedObj, false, dataType, false);
            } else {
                console.warn('CDO saved, but unknown operation: ' + currentOperation);
            }

            if (childOperation != null) {
                childOperation = currentOperation;
            }
            else {
                operation = currentOperation;
            }
        }
    }

    // Set any localization text that needs to be set in the HTML
    function setTranslations(localizedLabelsCollection) {
        _localizedLabels = localizedLabelsCollection;

        var labelValue = localizedLabelsCollection["Search"];
        $(SELECTORS.SEARCH_TREE_CRITERIA_INPUT).attr("placeholder", labelValue);
        $(SELECTORS.SEARCH_EXECUTION_BUTTON).attr("title", labelValue);

        labelValue = localizedLabelsCollection["AddButton"];
        $(SELECTORS.COMMAND_ADD_NEW_BUTTON).attr("title", labelValue);

        labelValue = localizedLabelsCollection["ExpandButton"];
        $(SELECTORS.EXPAND_TREE_BUTTON).attr("title", labelValue);

        labelValue = localizedLabelsCollection["CollapseButton"];
        $(SELECTORS.COLLAPSE_TREE_BUTTON).attr("title", labelValue);

        labelValue = localizedLabelsCollection["ImportButton"];
        $(SELECTORS.IMPORT_BUTTON).attr("title", labelValue);

        labelValue = localizedLabelsCollection["FhmLinkChildren"];
        $(SELECTORS.LINK_CHILDREN_TREE_BUTTON).attr("title", labelValue);
    }

    // Bind handlers for controls
    function bindEventHandlers() {
        // Click event for handle the clear search button.
        $(SELECTORS.SEARCH_CLEAR_BUTTON).click(function () {
            treeControl.search("");
            $(SELECTORS.SEARCH_TREE_CRITERIA_INPUT).val("");
            $(this).hide();
        });

        // Click handler for the execute search button
        $(SELECTORS.SEARCH_EXECUTION_BUTTON).click(function () {
            var textToSearchFor = $(SELECTORS.SEARCH_TREE_CRITERIA_INPUT).val();
            treeControl.search(textToSearchFor);
        });

        // Click handler for the execute import button
        $(SELECTORS.IMPORT_BUTTON).click(importButtonClicked);

        $(SELECTORS.SEARCH_TREE_CRITERIA_INPUT).keydown(searchKeyDown);
        $(SELECTORS.EXPAND_TREE_BUTTON).click(expandTree);
        $(SELECTORS.COLLAPSE_TREE_BUTTON).click(collapseTree);
        $(SELECTORS.LINK_CHILDREN_TREE_BUTTON).click(showLinkPopup);
        $(SELECTORS.COMMAND_ADD_NEW_BUTTON).click(addButtonClicked);
        $(SELECTORS.VALOR_EXPORT_FILE).change(valorImport);
        $(SELECTORS.SRC_EXPORT_FILE).change(srcImport);

        function searchKeyDown(e) {
            // search button visibility
            if ($(SELECTORS.SEARCH_TREE_CRITERIA_INPUT).val() !== "")
                $(SELECTORS.SEARCH_CLEAR_BUTTON).show();
            else
                $(SELECTORS.SEARCH_CLEAR_BUTTON).hide();

            var key = e.which;

            if (key === Camstars.KeyCodes.Enter || key === Camstars.KeyCodes.Tab) {
                // Enter key pressed. Execute the search.
                $(SELECTORS.SEARCH_EXECUTION_BUTTON).click();
                CR.Event.stopEvent(e);//e.stopPropagation();
                return false;
            } else if (key === Camstars.KeyCodes.Backspace && $(SELECTORS.SEARCH_TREE_CRITERIA_INPUT).val().length === 0) {
                $(SELECTORS.SEARCH_EXECUTION_BUTTON).click();
                CR.Event.stopEvent(e);//e.stopPropagation();
                return false;
            }
        }
    }

    //
    function srcImport(exportFileInput) {
        const srcExportFile = exportFileInput.target.files[0];
        if (!srcExportFile)
            return;

        // read contents (factory definition) of file
        const exportFileReader = new FileReader();
        exportFileReader.readAsText(srcExportFile);

        exportFileReader.onload = function () {

            exportFileInput.target.value = "";

            // TODO - any error checking here?
            // set data to be sent to preview popup through DataContractLink
            $(SELECTORS.IMPORT_TREE_DATA).val(exportFileReader.result);
            $(SELECTORS.IMPORT_FILENAME).val(srcExportFile.name);
            $(SELECTORS.IMPORT_TYPE).val('SRC');

            // open preview popup
            $(SELECTORS.IMPORT_PREVIEW_POPUP_BUTTON).click();
        };

        exportFileReader.onerror = function () {
            exportFileInput.target.value = "";
            __page.displayStatus(exportFileReader.error, "Error");
        };
    }

    // Import factory structure and resource settings from xml file that was exported from Valor MSS 2
    function valorImport(exportFileInput) {

        const factoryExportFile = exportFileInput.target.files[0];
        if (!factoryExportFile)
            return;

        const reader = new FileReader();
        reader.readAsText(factoryExportFile);

        reader.onload = function () {

            exportFileInput.target.value = "";
            // fill this with info to import and send to preview popup
            let importContent = {
                Areas: [],
                DefaultSettings: extractDefaultSettingsFromXml(reader.result)
            };

            try {
                importContent.Areas.push(extractAreaFromXml(reader.result));
            } catch (e) {
                console.error(e);
                __page.displayStatus(e, "Error");
            }

            if (importContent.Areas.length > 0 && importContent.Areas[0].Cells.length > 0) {
                // Initialize data to be sent to preview popup through DataContractLink
                $(SELECTORS.IMPORT_TREE_DATA).val(JSON.stringify(importContent));
                $(SELECTORS.IMPORT_FILENAME).val(factoryExportFile.name);
                $(SELECTORS.IMPORT_TYPE).val('Valor');

                $(SELECTORS.IMPORT_PREVIEW_POPUP_BUTTON).click();
            }
            else {
                __page.displayStatus(_localizedLabels["NoImportContent"], "Error");
            }
        };

        reader.onerror = function () {
            exportFileInput.target.value = "";
            __page.displayStatus(reader.error, "Error");
        };
    }

    // parse the given XML (exported from Valor MSS) and return an object of default settings for the enterprise
    function extractDefaultSettingsFromXml(factoryXml) {
        let xmlDoc = $.parseXML(factoryXml);
        const $factoryXml = $(xmlDoc);

        let defaultSettings = {};

        // default settings are in a special <machine> element with id="0"
        $factoryXml.find('factory machine[id="0"] setting').each(function (setting) {
            defaultSettings[this.attributes.TitleInDB.value] = this.attributes.Value.value;
        //    defaultSettings.push({
        //        Title: setting.Title,
        //        TitleInDB: setting.TitleInDB,
        //        Value: setting.Value,
        //        Type: setting.Type
        //    });
        });

        return defaultSettings;
    }

    // return: Area object a FH tree can display
    function extractAreaFromXml(factoryXml) {

        // Exit if selected node is not Area level
        const selNodeData = treeControl.getSelectedNodeData();
        if (!selNodeData || selNodeData.dataType.name !== "Area")
            return;

        let xmlDoc = $.parseXML(factoryXml);
        const $xml = $(xmlDoc);

        // Add ID (Id is required for Tree Control rendering)
        let selectedArea = {
            ID: 'area',
            Name: selNodeData.Name,
            hasSRCSettings: $xml.find("setting").length > 0,
            Cells: []
        };

        // Add all <line> elements as Cells of the selected Area
        $xml.find("line").each(function () {

            selectedArea.Cells.push({
                ID: "line" + this.attributes.id.value,
                Name: this.attributes.name.value,
                FactoryLevelIndex: (this.attributes.machineOrdinal ? this.attributes.machineOrdinal.value : null)
            });

            // get child <machine> elements and create array of equivalent Opcenter Equipment objects
            const eqps = [];
            $(this).find("machine").each(function () {
                let name = this.attributes.name ? this.attributes.name.value : "";
                if (!name && $(this).find("[Title='nickname']").length > 0)
                    name = $(this).find("[Title='nickname']")[0].attributes.Value.value;
                if (name) {
                    let eqpCount = eqps.push({
                        ID: this.attributes.id.value,
                        Name: (this.attributes.name ? this.attributes.name.value : $(this).find("[Title='nickname']")[0].attributes.Value.value),
                        MachineType: (this.attributes.McType ? this.attributes.McType.value : ""),
                        FactoryLevelIndex: (this.attributes.machineOrdinal ? this.attributes.machineOrdinal.value : null)
                    });

                    // create settings object on equipment
                    eqps[eqpCount - 1].OverrideSettings = {};

                    // <machine>\<setting> elements override the defaults
                    $(this).find('setting').each(function () {
                        eqps[eqpCount - 1].OverrideSettings[this.attributes.TitleInDB.value] = this.attributes.Value.value;
                    });
                } else
                    console.warn("Node has no Name or Nickname: " + this.outerHTML);
            });

            // add Equipment as children of this Cell
            if (eqps.length > 0)
                selectedArea.Cells[selectedArea.Cells.length - 1].Equipment = eqps;
        });

        // extract default settings from special <machine> element with 

        return selectedArea;
    }

    // Get the JSON data the represents the FHM
    function getTreeData() {
        var treeDataValue = null;

        var data = $(SELECTORS.TREE_DATA).val();
        if (data.length > 0)
            treeDataValue = JSON.parse(data);

        return treeDataValue;
    }

    // Get the JSON data the represents the FHM
    function getTreeDefinition() {
        var treeDefinition = null;

        var definition = $(SELECTORS.TREE_DEFINITION).val();
        if (definition.length > 0)
            treeDefinition = JSON.parse(definition);

        return treeDefinition;
    }

    // nodeData - if set, indicates editing an existing object
    // moreParams - object with name/value pairs appended onto the query string
    function getModelingQueryString(collectionDefinition, nodeData, moreParams) {
        var curStackKey = __page.get_CallStackKey();
        var nodeDef = collectionDefinition.customData;
        var instanceId = nodeData ? nodeData.getId() : null;

        let queryString = "HideInstanceList=true"
            + "&ResetCallStack=true"
            + "&id=" + nodeDef.CDODefID
            + "&isRDO=false"
            + "&maint=" + nodeDef.ServiceName
            + "&wip=false"
            + "&preventNew=true"
            + "&name=" + nodeDef.CDOTitle
            + "&CDOName=" + nodeDef.CDOName
            + "&maintTypeId=" + nodeDef.MaintTypeID
            + (instanceId ? "&InstanceID=" + instanceId : "")
            + (curStackKey ? "&pStackId=" + curStackKey : "");

        // append additional props
        if (moreParams) {
            for (let prop in moreParams) {
                queryString = queryString + '&' + prop + '=' + moreParams[prop];
            }
        }

        return queryString;
    }

    // The link object popup may not have any tabs or multiple tabs. If the popup does not have tabs then
    // this function returns true.
    function isTablessObjectLinkPopup() {
        var popupConfig = treeControl.getSelectedNodeCustomData().LinkItemPopupConfiguration;
        return popupConfig.tabDefinition.length === 0;
    }

    // Load a popup with resources, factories, etc that are not yet linked to the FHM. Users may select
    // these objects and they will be added as children of the currently selected object.
    function showLinkPopup() {
        setTimeout(() => {
            var popupConfig = treeControl.getSelectedNodeCustomData().LinkItemPopupConfiguration;

            displayProcessingIndicator(true);

            $.ajax({
                type: "POST",
                dataType: "json",
                url: './FactoryHierarchyService.svc/web/' + popupConfig.getOperationName,
                headers: {
                    'Accept': 'application/json'
                },
                contentType: "application/json;charset=UTF-8",
                async: true

            })
                .success(loadItemsToLinkSuccess)
                .fail(ajaxCallFailure);

            // Data has been retrieved.Now display the picklist popup.
            function loadItemsToLinkSuccess(response) {
                let config = isTablessObjectLinkPopup() ? createLinkPopupSingle(popupConfig) : createlinkPopupMultiple(popupConfig);

                displayProcessingIndicator(false);

                if (!isResponseStatusError(response)) {
                    // Open popup
                    if (isTablessObjectLinkPopup()) {
                        CR.PickGrid.openPopup(response.linkableResults, config, linkObjectsToFhmTabless);
                    } else {
                        CR.PickGrid.openPopup(response.linkableResults, config, linkObjectsToFhmMultipleTabs);
                    }
                }
            }
        }, 0);
    }

    function createlinkPopupMultiple(popupConfig) {
        var nameColHeaderTitle = _localizedLabels["Name"];
        var descColHeaderTitle = _localizedLabels["Description"];

        let config = {
            title: popupConfig.popupTitle,
            multiSelect: true,
            objectTypes: []
        };

        $.each(popupConfig.tabDefinition, function (index, popupTabConfig) {
            var item = {
                displayText: popupTabConfig.title,
                idPropName: 'InstanceID',
                filterPropName: 'Name',
                objsToPickPropName: popupTabConfig.collectionKey,
                colDefs:
                    [
                        {
                            headerText: nameColHeaderTitle,
                            propName: 'Name'
                        },
                        {
                            headerText: descColHeaderTitle,
                            propName: 'Description'
                        }
                    ]
            }

            config.objectTypes.push(item);
        });

        return config;
    }

    function createLinkPopupSingle(popupConfig) {
        var nameColHeaderTitle = _localizedLabels["Name"];
        var descColHeaderTitle = _localizedLabels["Description"];

        return {
            multiSelect: true,
            title: popupConfig.popupTitle,
            objectType:
            {
                idPropName: 'InstanceID',
                filterPropName: 'Name',
                colDefs:
                    [
                        {
                            headerText: nameColHeaderTitle,
                            propName: 'Name'
                        },
                        {
                            headerText: descColHeaderTitle,
                            propName: 'Description'
                        }
                    ]
            }
        };
    }

    // Does the response object (Camstar.WCF.ObjectStack.ResultStatus) from the server contain an error?
    // If so return true and optionally show an error message
    function isResponseStatusError(response, showMessage) {
        let isError = false;

        if (response != null && Object.keys(response).length > 0) {
            let statusKey = Object.keys(response)[0];

            isError = !response[statusKey].IsSuccess
            if (isError && (typeof showMessage === 'undefined' || showMessage === true)) {
                showErrorMessage(response[statusKey].Message);
            }
        }
    }

    // Link existing items such as resources and factories to the FHM. The data comes from a popup that 
    // that will load potential items to be added as children to the currently selected node.
    function linkObjectsToFhmTabless(selectedObject) {
        var customData = treeControl.getSelectedNodeCustomData();
        var popupConfig = customData.LinkItemPopupConfiguration;
        var parentType = treeControl.getSelectedNodeData().dataType.name;
        var parentName = treeControl.getSelectedNodeData().getName();
        var itemNames = selectedObject.map(po => { return po.Name; });
        displayProcessingIndicator(true);

        $.ajax({
            type: "POST",
            dataType: "json",
            url: './FactoryHierarchyService.svc/web/' + popupConfig.addOperationName,
            headers: {
                'Accept': 'application/json'
            },
            contentType: "application/json;charset=UTF-8",
            async: true,
            data: JSON.stringify({ "parentName": parentName, "parentType": parentType, "childItems": itemNames })
        })
            .success(addTreeItemsFromlink)
            .fail(ajaxCallFailure);

        // Once the database has been updated, update the tree.
        function addTreeItemsFromlink(response) {
            // Make sure the response status object does not specify that an error occured. If it doesn't
            // then process the data
            if (!isResponseStatusError(response)) {
                $.each(selectedObject, function (index, selectedItem) {
                    let childOperation = MODELING_OPERATIONS.LINK_CHILDREN;
                    let childCollectionType = treeControl.getSelectedNodeData().dataType.childCollectionTypes[0];
                    let serviceConfig = treeCollectionDefinition.getDefinition(childCollectionType).customData;
                    var dataType = treeCollectionDefinition.getDefinition(childCollectionType).dataType.name;

                    //  Do syncrhonous here since if we are doing a large number (> 50) async will result in errors
                    getCdoDataForTree(serviceConfig, childOperation, selectedItem.InstanceID, dataType, true);
                });
            }
            displayProcessingIndicator(false);
        }
    }

    function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    function displayProcessingIndicator(isShown) {
        if ($("#mod").length > 0 && $("#modImage").length > 0) {
            if (isShown) {
                $get("mod").style.display = "block";
                $get("modImage").style.display = "block";
            }
            else {
                $get("mod").style.display = "none";
                $get("modImage").style.display = "none";
            }
        }
    }

    // Link existing objects such as resources and factories to the FHM. The data comes from a popup that 
    // that will load potential items to be added as children to the currently selected node.
    function linkObjectsToFhmMultipleTabs(selectedObject) {
        var customData = treeControl.getSelectedNodeCustomData();
        var popupConfig = customData.LinkItemPopupConfiguration;
        var parentType = treeControl.getSelectedNodeData().dataType.name;
        var parentName = treeControl.getSelectedNodeData().getName();
        var popupConfig = treeControl.getSelectedNodeCustomData().LinkItemPopupConfiguration;
        var popupTabs = popupConfig.tabDefinition;
        var childItems = [];

        // Create an array of children to be added. The structure is a key/value pair so that
        // it maps to a dictionary on the server
        $.each(popupTabs, function (index, tab) {
            var propName = tab.collectionKey;
            var arrayOfNames = selectedObject[propName];
            var itemsList = arrayOfNames.map(po => { return po.Name; });

            childItems.push({ "Key": propName, "Value": itemsList });
        });

        $.ajax({
            type: "POST",
            dataType: "json",
            url: './FactoryHierarchyService.svc/web/' + popupConfig.addOperationName,
            headers: {
                'Accept': 'application/json'
            },
            contentType: "application/json;charset=UTF-8",
            async: true,
            data: JSON.stringify({ "parentName": parentName, "parentType": parentType, "childItems": childItems })
        })
            .success(addTreeItemsFromlink)
            .fail(ajaxCallFailure);

        // Once the database has been updated, update the tree.
        function addTreeItemsFromlink(response) {
            // Make sure the response status object does not specify that an error occured. If it doesn't
            // then process the data
            if (!isResponseStatusError(response)) {
                $.each(popupTabs, function (index, tab) {
                    var propName = tab.collectionKey;
                    var selectedItems = selectedObject[propName];
                    var collectionCustomData = treeCollectionDefinition.getDefinition(propName).customData;
                    var dataType = treeCollectionDefinition.getDefinition(propName).dataType.name;

                    $.each(selectedItems, function (index, selectedItem) {
                        let childOperation = MODELING_OPERATIONS.LINK_CHILDREN;

                        getCdoDataForTree(collectionCustomData, childOperation, selectedItem.InstanceID, dataType);
                    });

                });

            }
        }
    }

    function showErrorMessage(message) {
        displayProcessingIndicator(false);
        __page.displayStatus(message, "Error");
    }

    function ajaxCallFailure(response) {
        let labelValue = _localizedLabels["StatusMessage_ServerError"];
        showErrorMessage(labelValue);
    }

    return factoryHierarchyModelInterface;
})();