// Copyright Siemens 2023

// Handle the Factory Hierarchy Model (FHM) Import Preview Page.
var srcImport = (function () {
    'use strict';

    // localization labels
    var _localizedLabels = null;

    // object level variable
    var _treeControlId = "fhmImportTreeContainer";

    // SELECTORS
    var SELECTORS = {
        TREE_DATA: '#ctl00_WebPartManager_ImportDetailWP_treeData_ctl00',
        TREE_DEFINITION: "#ctl00_WebPartManager_ImportDetailWP_treeDefinition_ctl00",
        TREE_CONTAINER: "#" + _treeControlId,
        ERRORS_CONTAINER: '#fhmImportErrors',       
        ERRORS_LIST: '#fhmImportErrorsList',
        WEB_PART: '#WebPart_ImportDetailWP',
        IMPORT_BUTTON: "#ctl00_WebPartManager_ButtonsBar_Import",
        IMPORT_ERROR_HEADER: "#fhmImportErrorsHeader",
        WAITING_INDICATOR: "#mod"
    };

    // Public functions interface
    var factoryHierarchyModelInterface = {
        initialize: initialize
    };

    function localizePage() {        
        $(SELECTORS.IMPORT_ERROR_HEADER).text(_localizedLabels["Errors"]);
    }

    //async function showProcessingIndicator(show) {
    //    setTimeout(() => {
    //        if (show) {
    //            $(SELECTORS.WAITING_INDICATOR).css("display", "block");
    //            $("#modImage").css("display", "block");
    //        }
    //        else
    //            $(SELECTORS.WAITING_INDICATOR).css("display", "none");
    //    }, 0);
    //}

    // The server code will call this during the page's initialization. This function will then do any setup of the page that is needed.
    function initialize(localizedLabels) {
        //showProcessingIndicator(true);

        _localizedLabels = localizedLabels;

        // translate SRC format into format the FHM tree can display
        let srcImportData = getImportData();

        var treeData =
        {
            Enterprises: [
                getFactoryItem(srcImportData.factoryHierarchy.name)
            ]
        };

        localizePage();
        addChildren(treeData.Enterprises[0], srcImportData.factoryHierarchy.children);

        //showProcessingIndicator(false);

        function addChildren(treeParent, srcChildren) {
            if (!srcChildren) {
                return;
            }

            srcChildren.forEach(function (srcChild) {
                let treeChild = null;
                if (srcChild.factoryLevel === 2) { //Factory
                    treeChild = pushChild(treeParent, 'Factories', srcChild.name);
                } else if (srcChild.factoryLevel === 4) { //Area
                    treeChild = pushChild(treeParent, 'Areas', srcChild.name);
                } else if (srcChild.factoryLevel === 8) { //Cell
                    treeChild = pushChild(treeParent, 'Cells', srcChild.name);
                } else if (srcChild.factoryLevel === 16) { //Equipment or InventoryLocation
                    let childArrayName = 'Equipment';
                    // TODO - move knowledge of "isInventoryLocation" to an IS-specific file?
                    if (srcChild.attributes && srcChild.attributes.cdo === 'isInventoryLocation') {
                        childArrayName = 'InventoryLocations';
                    } 

                    treeChild = pushChild(treeParent, childArrayName, srcChild.name);
                }

                addChildren(treeChild, srcChild.children);
            });
        }

        // create and add a child to the array with the given name
        // childArrayName: ex - "Factories"
        // childName: ex - "MyFactory"
        // return: newly created child object
        function pushChild(treeParent, childArrayName, childName) {
            // create child array if it isn't there
            if (!treeParent.hasOwnProperty(childArrayName)) {
                treeParent[childArrayName] = [];
            }

            // make child object and add to array
            let treeChild = getFactoryItem(childName);
            treeParent[childArrayName].push(treeChild);
            return treeChild;
        }

        function getFactoryItem(name) {
            return {
                Name: name,
                ID: name,
                isChecked: true,
                checkboxDisable: false
            };
        }
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

        treeControl.setEventHandlers(null, null, null, null, null, null, null, null);
        treeControl.createTree(treeData, getTreeDefinition(), treeConfig, _treeControlId);

        // Click handler for the execute import button
        $(SELECTORS.IMPORT_BUTTON).prop("onclick", null).off("click").on('click', function () {
            //showProcessingIndicator(true);

            // use check status of nodes in tree to remove any import data that isn't checked
            let treeNodes = treeControl.nodeData.getAllNodes();

            removeUncheckedChildren(srcImportData.factoryHierarchy, treeNodes.Enterprises[0]);

            // send remaining (checked) data to SRC for import
            importHierarchy(srcImportData);

            function removeUncheckedChildren(srcImportItem, treeNode) {
                if (!treeNode.hasChildren()) {
                    return;
                }

                // iterate each of the tree node's child collections
                treeNode.dataType.childCollectionTypes.forEach(function (childCollectionName) {
                    if (treeNode[childCollectionName]) {
                        treeNode[childCollectionName].forEach(function (childTreeNode) {
                            // find the matching child in the SRC data
                            let matchingSrcChild = srcImportItem.children.find(srcChild => srcChild.name === childTreeNode.ID);

                            if (matchingSrcChild) {
                                if (childTreeNode.isChecked) {
                                    // don't have to remove this node, but check its children
                                    removeUncheckedChildren(matchingSrcChild, childTreeNode);
                                } else {
                                    // remove item from SRC data (user does not want to import it)
                                    let matchingSrcChildIndex = srcImportItem.children.indexOf(matchingSrcChild);
                                    srcImportItem.children.splice(matchingSrcChildIndex, 1);
                                }
                            } else {
                                console.error(`Unable to find SRC import item matching tree node with ID: ${childTreeNode.ID}`);
                            }
                        });
                    }
                });
            }

            //showProcessingIndicator(false);
        });

        setTimeout(function () { treeControl.expandAll() }, 0);
    }

    //
    function importHierarchy(srcImportData) {

        $(SELECTORS.IMPORT_BUTTON).prop('disabled', true);
        $(SELECTORS.TREE_CONTAINER).attr('style', '');
        $(SELECTORS.ERRORS_CONTAINER).attr('style', '');

        $.ajax({
            type: "POST",
            dataType: "json",
            url: './FactoryHierarchyService.svc/web/SrcImport',
            headers: {
                'Accept': 'application/json'
            },
            // Must set content-type this way to avoid jQuery bug with sending JSON containing "??"
            // https://forum.jquery.com/topic/special-characters-issue-find-random-strings-like-jquery20206329934545792639-1415046914457-in-data
            contentType: "application/json;charset=UTF-8",
            async: true,
            data: JSON.stringify({ srcFactoryHierarchy: JSON.stringify(srcImportData) }),
            context: document.body
        })
        .success(importSuccess)
        .fail(importFail)
        .always(function () { $(SELECTORS.IMPORT_BUTTON).prop('disabled', false); });
    }

    function importSuccess(response) {
        let importResult = response.SrcImportResult;

        if (!importResult.Success) {
            // API call error vs and import error
            __page.displayStatus(importResult.Message, "Error");
            return;
        }

        let srcResult = JSON.parse(importResult.SrcResult);

        // perhaps items in the import were not successful
        if (!srcResult.isSuccess) {

            // a somewhat quick and dirty way to do this
            $(SELECTORS.TREE_CONTAINER)
                .css('display', 'inline-block')
                .css('width', '46%');

            $(SELECTORS.ERRORS_CONTAINER)
                .css('display', 'inline-block')
                .css('width', '46%');

            let $errorsList = $(SELECTORS.ERRORS_LIST);
            $errorsList.empty();

            srcResult.details.forEach(function (detail) {
                if (!detail.isSuccess) {
                    let $errorLi = $(`<li><span class='tree-error-item-name'>${detail.factoryItemName}</span> - ${detail.failureReason}</li>`);
                    $errorsList.append($errorLi);
                }
            });
        } else {
            window.parent.factoryHierarchy.importComplete(_localizedLabels["IsCompletionImportSRC"]);
            window.parent.CloseFloatingFrame(false);
        }
    }

    function importFail(response) {
        if (typeof __page !== 'undefined')
            __page.hideModal();

        if (response.responseText) {
            __page.displayStatus(response.responseText, "Error");
        }

        console.error(response);
    }

    // Info to be imported (was parsed out of the import file)
    function getImportData() {
        var importData = null;

        var importDataString = $(SELECTORS.TREE_DATA).val();
        if (importDataString.length > 0)
            importData = JSON.parse(importDataString);

        return importData;
    }

    // Get the JSON data the represents the FHM
    function getTreeDefinition() {
        var treeDefinition = null;

        var definition = $(SELECTORS.TREE_DEFINITION).val();
        if (definition.length > 0)
            treeDefinition = JSON.parse(definition);

        return treeDefinition;
    }

    return factoryHierarchyModelInterface;
})();