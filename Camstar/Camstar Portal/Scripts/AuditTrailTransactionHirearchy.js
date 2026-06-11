// Copyright Siemens 2023

// Client side code to handle the Factory Hierarchy Model (FHM) Page.
var factoryHierarchy = (function () {
    'use strict';

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
        COMMAND_BAR: '.tree-control-command-bar',
        EXPAND_TREE_BUTTON: "#expandButton",
        COLLAPSE_TREE_BUTTON: "#collapseButton",
        WEB_PART: '#WebPart_FHMTreeWP'
    };

    var MODELING_OPERATIONS = {
        EDIT: 0,
        ADD_CHILD: 1,
        ADD_SIBLING: 2,
        COPY: 3
    };

    // Public functions interface
    var auditTrailConfiguratorTreeView = {
        initialize: initialize
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
    });

   
  

    function closeAddNodeDropdown() {
        $(SELECTORS.COMMAND_ADD_NEW_LIST).hide();
        $(SELECTORS.COMMAND_ADD_NEW_BUTTON).removeClass("command-button-selected");

        $('html').off('click', closeAddNodeDropdown);

    }

    // The server code will call this during the page's initialization. This function will then do any setup of the page that is needed.
    function initialize(translationsDocument) {

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

        $(SELECTORS.SEARCH_CLEAR_BUTTON).hide();
        $(SELECTORS.COLLAPSE_TREE_BUTTON).hide();
        treeControl.setEventHandlers(onItemSelected, null, null, null, null, null, onTreeRendered);

        treeControl.createTree(getTreeData(), getTreeDefinition(), treeConfig, _treeControlId);

        setTranslations(translationsDocument);
        bindEventHandlers();
    }

    function onTreeRendered() {
        $("[title=HistoryDetails]").parent().children('.tree-caret')[0].click()
    }

    function expandTree() {
        $(SELECTORS.EXPAND_TREE_BUTTON).hide();
        $(SELECTORS.COLLAPSE_TREE_BUTTON).show().focus();

        treeControl.expandAll();
    }

    function collapseTree() {
        $(SELECTORS.COLLAPSE_TREE_BUTTON).hide();
        $(SELECTORS.EXPAND_TREE_BUTTON).show().focus();

        treeControl.collapseAll();
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
    function onItemSelected(selectedNodeData) {
        __doPostBack("TransactionSelected", selectedNodeData.getName());
    }


    // modeling page has been opened, hook up listeners
    function modelingOpened(callStackKey, customData, operation) {
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
            
        }

        $('body')[0].addEventListener('maintDeleteSuccess', maintDeleteSuccess);

        // tell tree that the node is gone
        function maintDeleteSuccess(ev) {
            treeControl.deleteSelectedTreeNode();
            // delay closing tabs so the Delete success message can pop up
            setTimeout(closeAllTabs, 0);
        }

        $('body')[0].addEventListener('maintCopySuccess', maintCopySuccess);

        // load new node info and update tree
        function maintCopySuccess(ev) {
            operation = MODELING_OPERATIONS.ADD_SIBLING;
           
        }
        
        
    }

    // Set any localization text that needs to be set in the HTML
    function setTranslations(searchTreeCriteria) {
        var labelValue = searchTreeCriteria["Search"];
        $(SELECTORS.SEARCH_TREE_CRITERIA_INPUT).attr("placeholder", labelValue);

        labelValue = searchTreeCriteria["AddButton"];
        $(SELECTORS.COMMAND_ADD_NEW_BUTTON).attr("title", labelValue);

        labelValue = searchTreeCriteria["ExpandButton"];
        $(SELECTORS.EXPAND_TREE_BUTTON).attr("title", labelValue);

        labelValue = searchTreeCriteria["CollapseButton"];
        $(SELECTORS.COLLAPSE_TREE_BUTTON).attr("title", labelValue);

        labelValue = searchTreeCriteria["SearchButton"];
        $(SELECTORS.SEARCH_EXECUTION_BUTTON).attr("title", labelValue);
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

        $(SELECTORS.SEARCH_TREE_CRITERIA_INPUT).keydown(searchKeyDown);
        $(SELECTORS.EXPAND_TREE_BUTTON).click(expandTree);
        $(SELECTORS.COLLAPSE_TREE_BUTTON).click(collapseTree);
        //  addButtonClicked function does not exist
        //$(SELECTORS.COMMAND_ADD_NEW_BUTTON).click(addButtonClicked);

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

    return auditTrailConfiguratorTreeView;
})();
