// Copyright Siemens 2023

// Handle the Factory Hierarchy Model (FHM) Import Preview Page.
var factoryHierarchyImport = (function () {
    'use strict';

    // localization labels
    var _localizedLabels = null;

    // object level variable
    var _treeControlId = "fhmImportTreeContainer";
    var SELECTED_AREA_NAME;

    // SELECTORS
    var SELECTORS = {
        TREE_DATA: '#ctl00_WebPartManager_ImportDetailWP_treeData_ctl00',
        TREE_DEFINITION: "#ctl00_WebPartManager_ImportDetailWP_treeDefinition_ctl00",
        TREE_CONTAINER: "#" + _treeControlId,
        WEB_PART: '#WebPart_ImportDetailWP',
        IMPORT_BUTTON: "#ctl00_WebPartManager_ButtonsBar_Import",
		ERRORS_CONTAINER: '#fhmImportErrors',       
        ERRORS_LIST: '#fhmImportErrorsList',
		IMPORT_ERROR_HEADER: "#fhmImportErrorsHeader",
    };

    // Public functions interface
    var factoryHierarchyModelInterface = {
        initialize: initialize
    };

    // The server code will call this during the page's initialization. This function will then do any setup of the page that is needed.
    function initialize(localizedLabels) {

        _localizedLabels = localizedLabels;
		
		$(SELECTORS.IMPORT_ERROR_HEADER).text(_localizedLabels["Errors"]);
   
        var treeConfig = {
            "rootCollectionName": "Areas",
            "defaultTitleTemplate": "",
            "fieldMappings": {
                "id": "ID",
                "name": "Name",
                "parentIdForUpdate": "",
                "dataTypeForUpdate": ""
            }
        };

        var dataToImport = getImportData();
        SELECTED_AREA_NAME = dataToImport.Areas[0].Name;

        treeControl.setEventHandlers(null, null, null, null, null, null, onTreeRendered, onCheckboxChecked);

        treeControl.createTree(dataToImport, getTreeDefinition(), treeConfig, _treeControlId);

        // Click handler for the execute import button
        $(SELECTORS.IMPORT_BUTTON).off('click').on('click', function () {
            var mssImport = getCheckedTreeData();
            if (mssImport) {
                mssImport.defaultSettings = JSON.stringify(dataToImport.DefaultSettings);
                importData(mssImport);
            };
        });

        setTimeout(function () { treeControl.expandAll() }, 0);
    }

    // callback from tree (Add error message to disabled checkbox)
    function onTreeRendered() {
        var $disabledcheckbox = $(SELECTORS.TREE_CONTAINER + ' span.tree-node-item-checkbox[disabled="disabled"]');
        var $treeNodeTitle = $disabledcheckbox.siblings('.tree-node-item-title');

        var objectExistMsg = document.createTextNode('(' + _localizedLabels["ObjectAlreadyExists"] + ')');
        var errMsgElem = document.createElement("span");
        errMsgElem.appendChild(objectExistMsg);
        errMsgElem.style.color = "red";
        errMsgElem.style.marginLeft = "10px";

        $treeNodeTitle.each(function () {
            this.appendChild(errMsgElem.cloneNode(true));
        });
    }

    function onCheckboxChecked(nodeData, nodeCollection) {
        var importData = getImportData();
        var currentTreeData = getTreeDataByName(importData, nodeData.Name);

        if (!currentTreeData)
            return;

        currentTreeData.isChecked = (nodeData.isChecked !== undefined && !nodeData.checkboxDisable ? nodeData.isChecked : true);
        if (nodeData.hasChildren()) {
            nodeCollection.childCollections.forEach(function (child) {
                if (currentTreeData[child]) {
                    currentTreeData[child].forEach(function (currentChild) {
                        var childNode = treeControl.nodeData.findNodeByName(currentChild.Name);
                        currentChild.isChecked = (childNode.isChecked !== undefined && !childNode.checkboxDisable ? childNode.isChecked : true);
                    })
                }
            });
        }

        if (nodeData.hasParentNode()) {
            var parent = nodeData.getParentNode();
            if (parent.isChecked !== undefined) {
                var parentTreeData = getTreeDataByName(importData, parent.Name);
                parentTreeData.isChecked = (!parent.checkboxDisable ? parent.isChecked : true);
            }
        }

        // Update tree data for retaining changes if postback
        $(SELECTORS.TREE_DATA).val(JSON.stringify(importData));
    }

    function getTreeDataByName(treeData, node) {
        if (!treeData.Areas)
            return null;

        var foundTreeData = null;

        $.each(treeData.Areas[0].Cells, function (index, cell) {
            if (foundTreeData !== null)
                return false;

            if (cell.Name === node) {
                foundTreeData = cell;
                return false;
            }

            var cellNode = treeControl.nodeData.findNodeByName(cell.Name);
            cellNode.dataType.getCollectionDefinition().childCollections.forEach(function (child) {
                if (cell[child]) {
                    $.each(cell[child], function (index, eqp) {
                        if (eqp.Name === node) {
                            foundTreeData = eqp;
                            return false;
                        }
                    });
                }
            });
        });

        return foundTreeData;
    }

    // Get checked Tree Data
    function getCheckedTreeData() {
        var $checkedCheckbox = $(SELECTORS.TREE_CONTAINER + ' span.tree-node-item-checkbox[checked="checked"]');
        var $treeNodeContainers = $checkedCheckbox.closest('.tree-node-container');

        var checkedID = [];
        $treeNodeContainers.each(function () {
            var id = this.getAttribute('data-node-id');
            checkedID.push(id);
        });

        // Return if no resource is checked
        if (checkedID.length === 0) {
            __page.displayStatus(_localizedLabels["NoImportContent"], "Error");
            return null;
        }

        var importTreeData = getImportData().Areas[0];
        var importCheckedData = {};

        var selectedNode = treeControl.nodeData.findNodeByName(SELECTED_AREA_NAME);
        var nodeDef = selectedNode.dataType.getCollectionDefinition();
        var paramName = nodeDef.customData.Import.paramName;

        importCheckedData[paramName] = {
            name: importTreeData.Name,
            importSRCSettings: importTreeData.hasSRCSettings,
            cells: []
        };

        importTreeData.Cells.forEach(function (cell) {
            if (checkedID.includes(cell.ID)) {
                // Add Cell
                const cellImportData = {
                    name: cell.Name,
                    equipments: [],
                    factoryLevelIndex: cell.FactoryLevelIndex
                };
                importCheckedData[paramName].cells.push(cellImportData)

                // Add Cell's Children (Equipment)
                var cellNode = treeControl.nodeData.findNodeByName(cell.Name);
                cellNode.dataType.getCollectionDefinition().childCollections.forEach(function (child) {
                    if (cell[child]) {
                        cell[child].forEach(function (equipment) {
                            if (checkedID.includes(equipment.ID)) {
                                const eqpImportData = {
                                    name: equipment.Name,
                                    machineType: equipment.MachineType,
                                    machineId: equipment.ID,
                                    factoryLevelIndex: equipment.FactoryLevelIndex,
                                    overrideSettings: JSON.stringify(equipment.OverrideSettings)
                                };
                                cellImportData.equipments.push(eqpImportData);
                            }
                        });
                    }
                });
            }
        });

        return importCheckedData;
    }

    function importData(mssImport) {
        var selectedNode = treeControl.nodeData.findNodeByName(SELECTED_AREA_NAME);
        var nodeDef = selectedNode.dataType.getCollectionDefinition();
        if (typeof __page !== 'undefined')
            __page.showModal();
        $(SELECTORS.IMPORT_BUTTON).prop('disabled', true);
        $.ajax({
            type: "POST",
            dataType: "json",
            url: './FactoryHierarchyService.svc/web/' + nodeDef.customData.Import.operationName,
            headers: {
                'Accept': 'application/json'
            },
            // Must set content-type this way to avoid jQuery bug with sending JSON containing "??"
            // https://forum.jquery.com/topic/special-characters-issue-find-random-strings-like-jquery20206329934545792639-1415046914457-in-data
            contentType: "application/json;charset=UTF-8",
            async: true,
            data: JSON.stringify(mssImport),
            context: document.body
        })
            .success(importSuccess)
            .fail(importFail);
    }

    function importSuccess(response) {
        if (typeof __page !== 'undefined')
            __page.hideModal();

        $(SELECTORS.IMPORT_BUTTON).prop('disabled', false);
        var selectedNode = treeControl.nodeData.findNodeByName(SELECTED_AREA_NAME);
        var nodeDef = selectedNode.dataType.getCollectionDefinition();
        var responseField = nodeDef.customData.Import.responseFieldName;
        var isSuccess = true;
        var importResponse = response[responseField];
        let $errorsList = $(SELECTORS.ERRORS_LIST);

        $errorsList.empty();
		
		$.each(response.isImportAreaResourcesResult, function (index, responseMsgItem) {
			if(responseMsgItem.Value.ExceptionData) {
					isSuccess = false;
					let message = responseMsgItem.Value.ExceptionData.Description;
					let $errorLi = $(`<li><span class='tree-error-item-name'>${message}</li>`);
					$errorsList.append($errorLi);
			} else if(responseMsgItem.Key == "SRCResult") {
				if(responseMsgItem.Value.IsSuccess) {
					var srcResponse = $.parseJSON(responseMsgItem.Value.Message);

					$.each(srcResponse, function (index, result) {
						
						if (!result.Success) {               
							isSuccess = false;
							let $errorLi = $(`<li><span class='tree-error-item-name'>${result.Resource}</span> - ${result.Message}</li>`);
							$errorsList.append($errorLi);						
						}
					});
				}
			}
		});
		
        if (isSuccess) {
            window.parent.factoryHierarchy.importComplete(_localizedLabels["IsCompletionImportSRC"]);
            window.parent.CloseFloatingFrame(false);
        } else {
            // a somewhat quick and dirty way to do this
            $(SELECTORS.TREE_CONTAINER)
                .css('display', 'inline-block')
                .css('width', '46%');

            $(SELECTORS.ERRORS_CONTAINER)
                .css('display', 'inline-block')
                .css('width', '46%');           
        }
    }

    function importFail(response) {
        if (typeof __page !== 'undefined')
            __page.hideModal();
        $(SELECTORS.IMPORT_BUTTON).prop('disabled', false);
        if (response.responseText) {
            const error = $.parseXML(response.responseText);
            __page.displayStatus($(error).find("Reason")[0].textContent, "Error");
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