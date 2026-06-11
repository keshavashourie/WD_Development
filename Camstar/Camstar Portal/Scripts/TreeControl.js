// Copyright Siemens 2023

// Create a tree control. The control is built using a JSON data file with a specific structure that is matched with a
// JSON document that acts as a template to describe how the data should be rendered.

// This class object handles the UX portion of the tree as well as communicating with subsystems.
var treeControl = (function () {
    'use strict';

    // object level variables
    var _outputTreeToElementId = "";
    var _currentlySelectedNodeData = null;
    var _lastSearchCriteria = "";

    const SELECTORS = {
        ROOT: '#treeNode_root'
    };

    const DATA_ATTRIBUTE_NAMES = {
        "NODEID": "data-node-id",
        "COLLECTION": "data-collection-definition",
        "ACTION": "data-action"
    }

    const TEMPLATES = {
        checkbox: "<span id='_CHECKBOXID_-span' class='cs-checkbox tree-node-item-checkbox' _CHECKED_ _DISABLED_>\
                   <input type='checkbox' id='_CHECKBOXID_' name='_CHECKBOXID_' _CHECKED_ _DISABLED_/>\
                   <label for='_CHECKBOXID_'></label></span>"
    }

    // All event handers for the client.
    var _eventHandlers = {
        itemSelecting: null,
        itemSelectedEventHandler: null,
        itemButtonClickedEventHandler: null,
        treeLoadedEventHandler: null,
        newButtonClickedHandler: null,
        editButtonClickedHandler: null,
        copyButtonClickedHandler: null,
        deleteButtonClickedHandler: null,
        checkedboxCheckedHandler: null
    };

    // Action definitions are used for CRUD operations and tree buttons. Define an actions and it's relevent properties
    // for consumption. The permissionsRequired signifies the requires user permissions for create, read, update, or delete
    // records. The some combination of C, R, U, or D is used here
    var actionDefinitions = {
        read: {
            title: "Read",
            action: "Read",
            buttonCssClass: "",
            labelId: "",
            permissionsRequired: "R"
        },
        new: {
            title: "New",
            action: "New",
            buttonCssClass: "tree-item-button-new",
            labelId: "",
            permissionsRequired: "C"
        },
        edit: {
            title: "Edit",
            action: "Edit",
            buttonCssClass: "tree-item-button-edit",
            labelId: "",
            permissionsRequired: "RU"
        },
        copy: {
            title: "Copy",
            action: "Copy",
            buttonCssClass: "tree-item-button-copy",
            labelId: "",
            permissionsRequired: "RC"
        },
        delete: {
            title: "Delete",
            action: "Delete",
            buttonCssClass: "tree-item-button-delete",
            labelId: "",
            permissionsRequired: "RC"
        }
    };

    // Public functions interface
    var treeControlInterface = {
        createTree: createTree,
        collapseAll: collapseAll,
        expandAll: expandAll,
        search: search,
        enableTree: enableTree,
        setEventHandlers: setEventHandlers,
        getSelectedNodeData: getSelectedNodeData,
        getSelectedNodeCustomData: getSelectedNodeCustomData,
        addNode: addNode,
        addChildToRoot: addChildToRootNode,
        addChildToSelectedTreeNode: addChildNodeToSelected,
        addSiblingNodeToSelected: addSiblingNodeToSelected,
        hasSelectedTreeNode: hasSelectedNode,
        deleteSelectedTreeNode: deleteSelectedTreeNode,
        updateSelectedTreeNode: updateSelectedTreeNode,
        getNewNodeCollectionDefs: getNewNodeCollectionDefs,
        reload: reload,
        isTreeEnabled: isTreeEnabled,
        eventHandlers: _eventHandlers,
        setHeight: setHeight,
        nodeData: {
            getAllNodes: treeDataProvider.getAllNodes(),
            findNodeByProperty: treeDataProvider.findNodeByProperty,
            findNodeById: treeDataProvider.findNodeById,
            findNodeByName: treeDataProvider.findNodeByName
        }
    };

    function reload(nodeDataJson) {

        if (nodeDataJson)
            treeDataProvider.initialize(nodeDataJson);

        renderTree();
    }

    // Get list of collection definitions (types) that may be created given the currently
    // selected node (ie sibling or child)
    function getNewNodeCollectionDefs() {
        var newNodeCollectionDefs = [];

        try {
            if (hasSelectedNode()) {
                var nodeData = getSelectedNodeData();
                var collectionDefinition = nodeData.dataType.getCollectionDefinition();
                var nodeDataType = nodeData.dataType;

                // sibling (same type as selected)
                if (userHasActionPermission(actionDefinitions.new, collectionDefinition)) {
                    newNodeCollectionDefs.push(collectionDefinition);
                }

                // child types
                $.each(nodeDataType.childDataTypes, function (index, childDataType) {
                    if (userHasActionPermission(actionDefinitions.new, childDataType.getCollectionDefinition())) {
                        newNodeCollectionDefs.push(childDataType.getCollectionDefinition());
                    }
                });
            } else {
                var collectionDefinition = treeCollectionDefinition.getNodeRootDefinition();
                if (userHasActionPermission(actionDefinitions.new, collectionDefinition)) {
                    newNodeCollectionDefs.push(collectionDefinition);
                }
            }

        } catch (msg) {
            console.error("An error occured getting the available new node options: %s", msg);
            throw msg;
        }

        return newNodeCollectionDefs;
    }

    function isTreeEnabled() {
        var treeControl = getTreeControlContainerElement();
        return $(treeControl).hasClass("tree-disabled") === false;
    }

    // Set max height the tree may take
    function setHeight(heightPx) {
        $(SELECTORS.ROOT).outerHeight(heightPx, true);
    }

    // Programatically enable the tree control
    function enableTree(isEnabled) {
        var treeControl = getTreeControlContainerElement();

        if (isEnabled)
            $(treeControl).removeClass("tree-disabled");
        else
            $(treeControl).addClass("tree-disabled");
    }

    // Set the event handlers that the tree will be using to communicate with the caller.
    function setEventHandlers(onItemSelected, onItemSelecting, onItemButtonClicked, onEditButtonClicked, onCopyButtonClicked, onDeleteButtonClicked, onTreeRendered, onCheckboxChecked) {
        _eventHandlers = {
            itemSelectedEventHandler: onItemSelected,
            itemSelecting: onItemSelecting,
            itemButtonClickedEventHandler: onItemButtonClicked,
            treeRenderedEventHandler: onTreeRendered,
            editButtonClickedHandler: onEditButtonClicked,
            copyButtonClickedHandler: onCopyButtonClicked,
            deleteButtonClickedHandler: onDeleteButtonClicked,
            checkedboxCheckedHandler: onCheckboxChecked
        };
    }

    function getSelectedNodeCustomData() {
        return getSelectedNodeData().dataType.getCollectionDefinition().customData;
    }

    // Get the data node object for the selected tree node
    function getSelectedNodeData() {
        return _currentlySelectedNodeData;
    }

    // Has the user selected a tree node?
    function hasSelectedNode() {
        return getSelectedNodeData() !== null;
    }

    // siblingOfSelected - if false, assumes new node is child of selected
    function addNode(newNode, siblingOfSelected, dataType, isSelectNewChildNode) {
        if (hasSelectedNode()) {
            if (siblingOfSelected) {
                addSiblingNodeToSelected(newNode, isSelectNewChildNode);
            } else {
                dataType = typeof dataType === undefined ? null : dataType;
                addChildNodeToSelected(newNode, dataType, isSelectNewChildNode);
            }
        } else {
            addChildToRootNode(newNode);
        }
    }

    // Add a tree node to the tree's root
    function addChildToRootNode(dataToAdd, isSelectNewChildNode) {
        try {
            treeDataProvider.addNode(dataToAdd);
            addTreeNode(null, dataToAdd, dataToAdd.getCollectionType(), isSelectNewChildNode);
            sortTreeRootCollection();
        } catch (msg) {
            console.error("An error occured adding a child node to the selected tree node: %s", msg);
            throw msg;
        } finally {
            handleModifyTreeNodeError();
        }

        return true;
    }

    // Add a child node to the selected tree node
    function addChildNodeToSelected(dataToAdd, dataType, isSelectNewChildNode) {
        var hasNode = false;

        if (hasSelectedNode()) {
            hasNode = true;

            try {
                var selectedNode = getSelectedNodeData();
                if (!dataType) {
                    dataType = selectedNode.dataType.childDataTypes[0];
                }

                addChildNode(dataToAdd, selectedNode, dataType, isSelectNewChildNode);
            } catch (msg) {
                console.error("An error occured adding a child node to the selected tree node: %s", msg);
                throw msg;
            } finally {
                handleModifyTreeNodeError();
            }
        }

        return hasNode;
    }

    // Add a sibling node to the selected tree node
    function addSiblingNodeToSelected(dataToAdd, isSelectNewChildNode) {
        var hasNode = false;

        if (hasSelectedNode()) {
            hasNode = true;

            try {
                var selectedNode = getSelectedNodeData();
                var selectedNodeParent = selectedNode.getParentNode();

                if (!selectedNodeParent) {
                    addChildToRootNode(dataToAdd);
                } else {
                    // Node that has a parent node
                    var dataType = selectedNode.dataType;
                    addChildNode(dataToAdd, selectedNodeParent, dataType, isSelectNewChildNode);
                }
            } catch (msg) {
                console.error("An error occured adding a sibling node to the selected tree node: %s", msg);
                throw msg;
            } finally {
                handleModifyTreeNodeError();
            }
        }

        return hasNode;
    }

    // Add a child node to a parent tree node
    function addChildNode(dataToAdd, parentNode, dataType, isSelectNewChildNode) {

        treeDataProvider.addNode(dataToAdd, parentNode, dataType);
        addTreeNode(parentNode, dataToAdd, dataToAdd.getCollectionType(), isSelectNewChildNode);

        completeTreeNodeAddOrDelete(dataToAdd);
    }

    function handleModifyTreeNodeError() {
        enableTree(true);
    }


    // A tree container is the div that holds all the elements that work together to display a tree node. It 
    // also contains data attributes about the tree node that can be used to retreive its underlying data 
    // if needed.
    function getTreeContainerItemByClass($treeElementForContainer, classToFind) {
        var $container = getTreeNodeContainer($treeElementForContainer);
        var $childElement = $container.children(".tree-node-item-container").children("." + classToFind);

        return $childElement;
    }

    // Not use at this time as we are no using buttons located at the node level for our tree. This could be used for 
    // later projects.
    function getTreeContainerActionButton($treeElementForContainer, actionDefinition) {
        var $container = getTreeNodeContainer($treeElementForContainer);
        var $button = $container.children(".tree-node-item-container").find("." + actionDefinition.buttonCssClass);

        return $button;
    }

    // Sort all of the tree nodes at the root of the tree
    function sortTreeRootCollection() {
        return;
        var $container = $(SELECTORS.ROOT);
        var treeNodecontainers = $container.children('.tree-node-container').toArray().sort(treeNodeSortComparer)

        for (var i = 0; i < treeNodecontainers.length; i++) {
            var $treeNodecontainer = $(treeNodecontainers[i]).detach();
            $treeNodecontainer.appendTo($container);
        }
    }

    // Sort the tree node children of the passed in collection.
    function sortTreeCollection($collectionToSort) {
        return;
        var $container = getTreeNodeContainer($collectionToSort);
        var $treeCollapsableSection = $container.children(".tree-nested");
        var treeNodecontainers = $treeCollapsableSection.children('.tree-node-container').toArray().sort(treeNodeSortComparer)

        for (var i = 0; i < treeNodecontainers.length; i++) {
            var $treeNodecontainer = $(treeNodecontainers[i]).detach();
            $treeNodecontainer.appendTo($treeCollapsableSection);
        }
    }

    // Used for the sort functions.
    function treeNodeSortComparer(a, b) {
        var aTitle = $(a).find(".tree-node-item-title").html();
        var bTitle = $(b).find(".tree-node-item-title").html();
        var result = aTitle.localeCompare(bTitle);

        return result;
    }

    // Update the tree selected tree node. The data itself will be updated directly by the user.
    // This function takes care of the display rendering.
    function updateSelectedTreeNode(updateNodeData) {
        var hasNode = false;

        if (hasSelectedNode() && updateNodeData) {
            hasNode = true;

            try {
                var nodeToUpdate = getSelectedNodeData();
                var editPreview = treeDataProvider.getEditPreview(nodeToUpdate, updateNodeData);

                if (!editPreview.hasParentChanged) {
                    treeDataProvider.updateNode(nodeToUpdate, updateNodeData);

                } else {
                    changeSelectedTreeNodeParent(updateNodeData);
                }

                var newTitle = createTreeNodeTitle(nodeToUpdate);
                var $titleNode = getTreeContainerItemByClass(getTreeNodeContainerFromData(nodeToUpdate), "tree-node-item-title");
                $titleNode.text(newTitle).attr("title", newTitle);

                var parent = nodeToUpdate.getParentNode();
                if (parent) {
                    var $treeNode = getTreeNodeContainerFromData(parent);
                    sortTreeCollection($treeNode);
                }

                updateSearchHighlightForNode($titleNode);
            } catch (msg) {
                console.error("An error occurred updating the selected tree node: %s", msg);
                throw msg;
            } finally {
                handleModifyTreeNodeError();
            }
        }

        return hasNode;
    }

    function deleteSelectedTreeNode() {
        var hasNode = false;

        if (hasSelectedNode()) {
            hasNode = true;
            var nodeToDelete = getSelectedNodeData();
            treeDataProvider.deleteNode(nodeToDelete);

            // Now remove the element from its current parent
            deleteTreeNode(nodeToDelete, false);
            nodeToDelete = null;

        }

        return hasNode;
    }

    // Pass in a data node and get its tree node container element back.
    function getTreeNodeContainerFromData(dataNode) {
        if (dataNode !== null) {
            var treeElementId = createTreeNodeContainerElementId(dataNode.getId(), dataNode.getCollectionType());
            var $treeElement = $("#" + treeElementId);

            return $treeElement;
        } else
            return null;
    }

    // Remove a tree node. This is UI only. The underlying data is not modified by this function call.
    function deleteTreeNode(nodeToDelete, isMove) {
        // Now delete the element from its current parent
        var $treeElementToDelete = getTreeNodeContainerFromData(nodeToDelete);
        var $previousTreeNodeCollapseContainer = $treeElementToDelete.parent();
        var $treeNodeToDeleteElement = isMove ? $treeElementToDelete.detach() : $treeElementToDelete.remove();

        // Update the tree node based on whether there are remaining children or not
        if ($previousTreeNodeCollapseContainer.children().length === 0) {
            $previousTreeNodeCollapseContainer.remove();
        }

        if ($previousTreeNodeCollapseContainer.children().length === 0) {
            $previousTreeNodeCollapseContainer.remove();
        }

        completeTreeNodeAddOrDelete(nodeToDelete);

        return $treeNodeToDeleteElement;
    }

    // After a node has been edited or deleted, do any shared cleanup operations
    function completeTreeNodeAddOrDelete(nodeDataFromUpdate) {
        if (!nodeDataFromUpdate.hasParentNode()) {
            return;
        }

        var parentDataNode = nodeDataFromUpdate.getParentNode();
        var $treeNodeParent = getTreeNodeContainerFromData(parentDataNode);
        var $treeNodeCurrent = getTreeNodeContainerFromData(nodeDataFromUpdate);

        sortTreeCollection($treeNodeParent);
        //refreshDeleteButton(parentDataNode);
        updateTreeContainerCaretDisplay($treeNodeParent);
        expandParentTreeNodes($treeNodeCurrent);
    }

    // Change the parent of a tree node. This function only allows for movement within the same parent collection type
    function changeSelectedTreeNodeParent(updatedNodeData) {
        var hasNode = false;

        if (hasSelectedNode()) {
            hasNode = true;

            try {
                // First update the node data
                var nodeToMove = getSelectedNodeData();
                var editPreview = treeDataProvider.getEditPreview(nodeToMove, updatedNodeData);
                var newParentNode = editPreview.updatedParent;
                var prevNodeParent = editPreview.originalParent;
                var $prevTreeNodeParent = getTreeNodeContainerFromData(prevNodeParent);
                var $treeNodeToMoveElement = deleteTreeNode(nodeToMove, true);

                if (newParentNode !== null) {
                    treeDataProvider.updateNode(nodeToMove, updatedNodeData);

                    // Now attach the element to its new parent
                    var $attachToTreeNodeParentElement = getTreeNodeContainerFromData(newParentNode);
                    var $newTreeElementToMoveCollapseContainer = $attachToTreeNodeParentElement.children(".tree-nested");

                    // No collapsable container div. Add it first
                    let needExpand = false;
                    if ($newTreeElementToMoveCollapseContainer.length === 0) {
                        var childrenContainerElement = createTreeNodeCollapsableContainerElement(newParentNode);
                        $attachToTreeNodeParentElement.get()[0].appendChild(childrenContainerElement);
                        $newTreeElementToMoveCollapseContainer = $attachToTreeNodeParentElement.children().last();
                        needExpand = true;
                        $attachToTreeNodeParentElement.get()[0].children[0].children[0].className = "tree-caret tree-caret-collapsed";
                    }

                    $treeNodeToMoveElement.appendTo($newTreeElementToMoveCollapseContainer);
                    if (needExpand)
                        $attachToTreeNodeParentElement.get()[0].children[0].children[0].click();

                    if (editPreview.hasDataTypeChanged) {
                        //Update data properties on node
                        $treeNodeToMoveElement.attr(DATA_ATTRIBUTE_NAMES.COLLECTION, nodeToMove.getCollectionType());
                        $treeNodeToMoveElement.attr("id", createTreeNodeContainerElementId(nodeToMove.getId(), nodeToMove.getCollectionType()));

                        var collectionDefinition = nodeToMove.dataType.getCollectionDefinition();
                        if (collectionDefinition.iconCSSClass && collectionDefinition.iconCSSClass.length > 0) {
                            var nodeIconCSS = collectionDefinition.iconCSSClass;
                            var itemImage = getTreeContainerItemByClass($treeNodeToMoveElement, "tree-item-icon");
                            itemImage.attr("class", "tree-item-icon " + nodeIconCSS);
                            itemImage.attr("title", collectionDefinition.iconTitle);
                        }
                    }

                    completeTreeNodeAddOrDelete(nodeToMove);
                }
                updateTreeContainerCaretDisplay($prevTreeNodeParent);

            } catch (msg) {
                console.error("An error occured changing the selected nodes' parent tree node: %s", msg);
                throw msg;
            } finally {
                handleModifyTreeNodeError();
            }
        }

        return hasNode;
    }

    // After tree nodes have been added or deleted, this function should be used to see if the parent element needs to have
    // caret or not. This based on whether or not the tree node has children or not after the change has been made.
    function updateTreeContainerCaretDisplay(nodeContainerToUpdate) {
        var nodeContainer = getTreeNodeContainer(nodeContainerToUpdate);
        const caretSelector = ".tree-caret";
        let nodeHasChildren = nodeContainer.find("> div.tree-child-node").length > 0;
        let nodeMarkedAsLeaf = nodeContainer.find("> div > div.tree-last-node-caret").length > 0;

        if (!nodeHasChildren && !nodeMarkedAsLeaf) {
            nodeContainer.find(caretSelector).removeClass().addClass("tree-last-node-caret");
        } else if (nodeHasChildren && nodeMarkedAsLeaf) {
            // remove class that identifies the node as a leaf and add classes that will show the caret
            nodeContainer.find("> div > div.tree-last-node-caret").removeClass().addClass("tree-caret tree-caret-collapsed");
        }
    }

    // For simplicity's sake the tree control provides access to some of the functions used to access the underlying data obects. WE 
    // provide them thought the interface object. But if the data access class for the tree has not been added to the DOM yet, javascript
    // will through an exception when initially creating the tree control. So instead we defer the assignment until later and take
    // care of it here.
    function setPublicDataAccessFunctionReferences() {
        var add = treeControlInterface.nodeData;

        add.getAllNodes = treeDataProvider.getAllNodes;
        add.findNodeByProperty = treeDataProvider.findNodeByProperty;
        add.findNodeById = treeDataProvider.findNodeById;
        add.findNodeByName = treeDataProvider.findNodeByName;
    }

    // Public function - Generate a tree and render it to the client. This function takes JSON data document 
    // and builds a tree from it. There is also a definition document that describes display
    // attributes to apply to the tree data document. 
    //
    // Further information about both of these
    // JSON documents can be found at the bottom of this javascript file.
    function createTree(nodeDataJson, collectionDefinitionsJson, configurationJson, outputTreeToElementId) {
        if (!outputTreeToElementId || outputTreeToElementId.length == 0)
            throw ("No output html element specified")

        setPublicDataAccessFunctionReferences();

        treeConfiguration.initialize(configurationJson);
        treeCollectionDefinition.initialize(collectionDefinitionsJson);
        treeDataTypeDefinition.initialize();
        treeDataProvider.initialize(nodeDataJson);

        $("#" + outputTreeToElementId).addClass("tree-control");
        _outputTreeToElementId = outputTreeToElementId;

        renderTree();
    }

    // When the title on a node changes, and the node as part of search results, check to see if it should still
    // be highlighted as matching result or not.
    function updateSearchHighlightForNode(titleElement) {
        if (_lastSearchCriteria.length > 0) {
            if (titleElement.hasClass("search-match-indicator")) {
                var nodeTextValue = $(titleElement).html();

                var startPosition = nodeTextValue.toLowerCase().indexOf(_lastSearchCriteria.toLowerCase());
                if (startPosition >= 0) {
                    searchResultsProcessItem(titleElement, _lastSearchCriteria);
                } else {
                    titleElement.removeClass("search-match-indicator");
                }
            }
        }
    }

    var clickFoundNodeTimeoutId = null;

    // Search the tree for the first match on the text the user entered. At this time we only match on the first
    // result, so if there are other matches they will not be handled
    function search(textToSearchFor) {
        if (clickFoundNodeTimeoutId) {
            clearTimeout(clickFoundNodeTimeoutId);
        }

        _lastSearchCriteria = textToSearchFor;
        clearSearchHighlights();

        if (textToSearchFor.length > 0 && isTreeEnabled()) {
            // Find the text that matches our criteria. The JQuery ":contains()" function is case sensative so the code below
            // was used instead to get the matches.
            var matches = $(".tree-node-item-title").filter(function () {
                var reg = new RegExp(textToSearchFor, "i");
                return reg.test($(this).text());
            });

            if (matches.length > 0) {
                matches.each(function (index, match) {
                    searchResultsProcessItem(match, textToSearchFor);
                });
                // make sure first match is visible
                matches.first('.tree-node-item-container')[0].scrollIntoView(false);

                //// Exactly one match?  Click it.
                //if (matches.length === 1) {
                //    // wait a bit to make sure they don't change the search
                //    clickFoundNodeTimeoutId = setTimeout(function () { nodeClicked(matches[0]); }, 1000);
                //}
            }
        }
    }

    // Process the search results for a single matching item if one was found.
    function searchResultsProcessItem(matchingElement, textToSearchFor) {
        var nodeTextValue = $(matchingElement).html();
        var startPosition = nodeTextValue.toLowerCase().indexOf(textToSearchFor.toLowerCase());
        var endPosition = startPosition + textToSearchFor.length;

        var preText = startPosition > 0 ? nodeTextValue.substring(0, startPosition) : "";
        var textToHighlight = nodeTextValue.substring(startPosition, endPosition);
        var postText = nodeTextValue.substring(endPosition);
        var highlightHtml = preText + "<span>" + textToHighlight + "</span>" + postText;

        $(matchingElement).addClass("search-match-indicator");
        $(matchingElement).html(highlightHtml);
        expandParentTreeNodes(matchingElement);
    }

    // Reset content to value contained in the title
    function clearSearchHighlights() {
        $(".search-match-indicator")
            .html(function () {
                return $(this).attr('title');
            })
            .removeClass("search-match-indicator");
    }

    // Deselect the selected node. Take care of any UI or data changes to reflext the new state of the tree.
    function deselectSelectedNode() {
        if (hasSelectedNode()) {
            $(".selected-tree-node").removeClass("selected-tree-node");
            removeFullWidthDisplay();
            _currentlySelectedNodeData = null;
        }
    }

    function removeFullWidthDisplay() {
        $(".fullWidthHighlight").css("margin-left", "");
        $(".fullWidthHighlight").removeClass("fullWidthHighlight");
    }

    // Expand the whole tree from the root
    function expandAll() {
        $(".tree-caret-collapsed").click();
    }

    // Collapse the tree down to the root
    function collapseAll() {
        $(".tree-caret-expanded").click();
        //  not sure why this was being done
        //deselectSelectedNode();   
    }

    // Return tree of the tree element is in the expanded state.
    function isTreeNodeExpanded(treeElement) {
        var $container = getTreeNodeContainer(treeElement);
        var expandedCaretElement = getTreeContainerItemByClass($container, "tree-caret-expanded");
        return expandedCaretElement.length !== 0;
    }

    // Starting from the tree element that is passed in, expand all of its parent nodes right up to the 
    // root.
    function expandParentTreeNodes(treeElement) {
        var forInstanceID = getTreeNodeContainer(treeElement).attr(DATA_ATTRIBUTE_NAMES.NODEID);

        // Get all of the parent nodes up the to the start of the tree
        var parents = $(treeElement).parentsUntil("#treeNode_root");

        parents.each(function (index, parentElem) {
            if ($(parentElem).hasClass("tree-node-container")) {
                var parentElemInstanceID = $(parentElem).attr(DATA_ATTRIBUTE_NAMES.NODEID);
                var caretElement = getTreeContainerItemByClass($(parentElem), "tree-caret-collapsed");

                if (parentElemInstanceID !== forInstanceID && caretElement.length > 0) {
                    // Expand this node
                    caretElement.click();
                }
            }
        });

    }

    // This will be used for having a delete button on a tree node. We are not using that functionality at this
    // time and this code needs more work.
    //function refreshDeleteButton(nodeData) {
    //    var $treeNode = getTreeNodeContainerFromData(nodeData);
    //    var $deleteButton = getTreeContainerActionButton($treeNode, actionDefinitions.delete);

    //    if ($deleteButton.length > 0) {
    //        if (nodeData.hasChildren())
    //            $deleteButton.removeClass(_buttonDisabledCssClass);
    //        else
    //            $deleteButton.addClass(_buttonDisabledCssClass);
    //    }
    //}

    // Get the JSON document for the tree's data
    function getTreeData() {
        return treeDataProvider.getAllNodes();
    }

    // Get the tree definition by its key.
    function getTreecollectionDefinition(collectionType) {
        return treeCollectionDefinition.getDefinition(collectionType);
    }

    // Get a reference to the control on the page that the tree will be rendered into.
    function getTreeControlContainerElement() {
        var control = document.getElementById(_outputTreeToElementId);
        if (control == null)
            throw "html element to output the tree control to is invalid";

        return control;
    }

    // The function drives  the rendering of the tree based on the supplied data.
    function renderTree() {
        getTreeControlContainerElement().innerHTML = "";

        // Perform the rendering with a recursive function.
        setTimeout(function () {
            var treeData = getTreeData();

            if (treeData !== null)
                createTreeNodes();
        }, 0);
    }

    // An event handler to be fired when a collection of data for the tree has been iterated over. The handler will 
    // sort the collection.
    function nodeDataCollectionIterationCompleted(parentNode, currentNodeCollection, currentNodeCollectionType) {
        if (parentNode) {
            var $treeNode = getTreeNodeContainerFromData(parentNode);
            sortTreeCollection($treeNode);
        } else {
            sortTreeRootCollection();
        }

    }

    // Intitialize the data provider's iterator function. This configuration will be used to draw the actual tree.
    function createTreeNodes() {
        var isLoaded = true;

        try {
            var iterationOptions = treeDataProvider.createDefaultIterationOptionsObject();
            iterationOptions.processNodeHandler = addTreeNode;
            iterationOptions.collectionIterationCompleteHandler = nodeDataCollectionIterationCompleted;

            treeDataProvider.iterateNodes(iterationOptions);
        } catch (msg) {
            isLoaded = false;
            console.error("Unable to load tree: " + msg);
        }

        if (_eventHandlers.treeRenderedEventHandler) {
            setTimeout(function () {
                _eventHandlers.treeRenderedEventHandler(isLoaded);
            }, 0);
        }
    }

    function createStringFromTemplate(nodeData, defaultTemplateFormat, templateFormatForCollection) {
        // Start of using functions. Work has started but needs testing. We don't need it right now so 
        // it is deferred.
        //const getParentIdFunction = "{getParentId()}";
        //const getParentNameFunction = "{getParentName()}";

        var stringValue = defaultTemplateFormat ? defaultTemplateFormat : "";

        // Is there an alternate format? If so configure it with the default format and
        // let the remainder of the function process it.
        if (templateFormatForCollection && templateFormatForCollection.length > 0) {
            stringValue = templateFormatForCollection.replace("{DefaultFormat}", stringValue);
        }

        // Do any {field} replacements that need to be done
        $.each(nodeData, function (fieldKey, fieldValue) {
            if (typeof fieldValue === 'string' && fieldValue.length > 0) {
                stringValue = stringValue.replace("{" + fieldKey + "}", fieldValue);
            }
        });

        // Needs testing before uncommenting this code
        //if (stringValue.indexOf(getParentIdFunction) && nodeData.hasParentNode())
        //    stringValue = stringValue.replace(getParentIdFunction, nodeData.getParentNode().getId());

        //if (stringValue.indexOf(getParentNameFunction) && nodeData.hasParentNode())
        //    stringValue = stringValue.replace(getParentNameFunction, nodeData.getParentNode().getName());

        // Do any cleanup work that needs to be done
        stringValue = templateStringPostProcessing(stringValue);

        return stringValue;
    }

    // A format (default or custom) has been found, it will use the data from the current element to do a replace and create the 
    // title.
    //
    // Parameters:
    //      nodeData - The name/value pairs the contain the data to be processed.
    function createTreeNodeTitle(nodeData) {
        var cfgDefaultTemplate = treeConfiguration.defaultTitleTemplate;
        var treeNodeCollectionType = nodeData.getCollectionType();
        var collectionDefinition = getTreecollectionDefinition(treeNodeCollectionType);
        var defaultFormat = cfgDefaultTemplate && cfgDefaultTemplate.length > 0 ? cfgDefaultTemplate : nodeData.getName();

        var titleValue = createStringFromTemplate(nodeData, defaultFormat, collectionDefinition.titleFormat);
        return titleValue;
    }

    // Cleanup of the title creation from the template. Once cleanup is that we have text and field replacement grouped with [[ and ]]
    // brackets. If the {field} replacement was successfull then only the [[ and ]] will be removed. But if their was not a match then
    // also remove both the brackets and all of the text between them.
    function templateStringPostProcessing(titleValue) {

        while (titleValue.indexOf("{") > 0) {
            // Remove failed substitutions that have grouped text
            var startDeletionAt = titleValue.indexOf("{");
            var endDeletionAt = titleValue.indexOf("}");
            var groupingAt = titleValue.indexOf("[[");

            if (groupingAt >= 0 && groupingAt < startDeletionAt) {
                startDeletionAt = groupingAt;
                endDeletionAt = titleValue.indexOf("]]") + 2;
            }

            var titleValueStart = titleValue.substring(0, startDeletionAt);
            var titleValueEnd = titleValue.substring(endDeletionAt, titleValue.length);

            titleValue = titleValueStart + titleValueEnd;
        }

        titleValue = titleValue.replace(/\[\[/g, "");
        titleValue = titleValue.replace(/\]\]/g, "");
        titleValue = titleValue.replace(/  +/g, ' ');
        titleValue = titleValue.trim();

        return titleValue;
    }

    // A tree node container is a div that holds all of the UI elements, data properties that up a node in the tree display.
    // This function creates an ID for one of them for dom selection or to create a new element's ID in a consistent manner.
    function createTreeNodeContainerElementId(id, collectionDefinitionKey) {
        return createTreeElementID("treeNodeContainer", id, collectionDefinitionKey)
    }

    // A collapsable tree element is a div that holds a tree node's children. The collapsable section essentially hides
    // the child tree nodes until the user clicks on a caret. Then the collaspsable container will display its nodes.
    // This function creates an ID for one of them for dom selection or to create a new element's ID in a consistent manner.
    function createTreeNodeCollapsableContainerElementID(id, collectionDefinitionKey) {
        return createTreeElementID("treeNodeCollapsable", id, collectionDefinitionKey)
    }

    // Generic formatter for creating tree element IDs'
    function createTreeElementID(prefix, id, collectionDefinitionKey) {
        return prefix + "_" + id + "_" + collectionDefinitionKey;
    }

    // Create the tree node and add it to the tree on the page
    function addTreeNode(parentNode, currentNodeData, currentNodeCollectionType, isSelectNewChildNode) {
        const treeNodeRootElementId = "treeNode_root";

        var parentElementId = parentNode !== null ? createTreeNodeCollapsableContainerElementID(parentNode.getId(), parentNode.getCollectionType()) : "";

        // Get the definition data 
        var collectionDefinition = getTreecollectionDefinition(currentNodeCollectionType);

        // Are these the root tree elements?
        var isRootElement = parentNode === null;

        // Parent is either the root or a node of data. Get the ID.
        var parentElementId = isRootElement ? treeNodeRootElementId : parentElementId;

        // Create a title to display for the new tree node. 
        var nodeTitle = createTreeNodeTitle(currentNodeData);
        var hasChildData = currentNodeData.hasChildren();

        // Do we have any of the tree rendered yet?
        if (isRootElement && $("#" + treeNodeRootElementId).length === 0) {
            // The first UL statement has not been created to contain the data. So let's
            // create that first.
            var treeRootElment = document.createElement("div");
            treeRootElment.setAttribute("id", treeNodeRootElementId);
            treeRootElment.setAttribute("class", "tree-nested tree-node-expanded tree-root-node");
            getTreeControlContainerElement().appendChild(treeRootElment);
        }

        // Get the parent element reference
        var parentElement = document.getElementById(parentElementId);
        if (!parentElement) {
            // When adding a new node after the tree has been fully rendered, the collapsabe section will be missing for an element
            // with no child nodes. Add it back in here.
            var containerNodeID = createTreeNodeContainerElementId(parentNode.getId(), parentNode.getCollectionType());
            var collapsable = createTreeNodeCollapsableContainerElement(parentNode);
            var container = document.getElementById(containerNodeID);
            container.appendChild(collapsable);

            parentElement = document.getElementById(parentElementId);
        }

        var nodeID = createTreeNodeContainerElementId(currentNodeData.getId(), currentNodeCollectionType);

        // Create the tree item and add it to the the tree on the page.
        var nodeItemListHeader = document.createElement("div");
        nodeItemListHeader.setAttribute("id", nodeID);
        nodeItemListHeader.setAttribute(DATA_ATTRIBUTE_NAMES.NODEID, currentNodeData.ID);
        nodeItemListHeader.setAttribute(DATA_ATTRIBUTE_NAMES.COLLECTION, currentNodeCollectionType);
        nodeItemListHeader.setAttribute("class", "tree-node-container");

        var treeNodeContainer = document.createElement("div");
        treeNodeContainer.setAttribute("class", "tree-node-item-container");
        nodeItemListHeader.appendChild(treeNodeContainer);
        parentElement.appendChild(nodeItemListHeader)

        var caretItem = document.createElement("div");
        if (hasChildData) {
            caretItem.setAttribute("class", "tree-caret tree-caret-collapsed");
        } else {
            caretItem.setAttribute("class", "tree-last-node-caret");
        }

        caretItem.addEventListener('click', function () { caretClicked(this); });

        treeNodeContainer.appendChild(caretItem);

        // Create Tree node item checkbox
        if (currentNodeData.isChecked !== undefined) {
            let checkBoxId = 'checkbox_' + currentNodeData.ID.replaceAll(' ', '-')   // can't have spaces in an HTML ID

            var nodeCheckBox = TEMPLATES.checkbox
                .replace(new RegExp('_CHECKBOXID_', 'g'), checkBoxId)
                .replace(new RegExp('_CHECKED_', 'g'), (currentNodeData.isChecked ? 'checked="checked"' : ''))
                .replace(new RegExp('_DISABLED_', 'g'), (currentNodeData.checkboxDisable ? 'disabled="disabled"' : ''));

            treeNodeContainer.insertAdjacentHTML("beforeend", nodeCheckBox);
            var checkbox = $('#' + checkBoxId);
            checkbox.click(checkboxChecked);
        }

        // Create tree node item icon
        if (collectionDefinition.iconCSSClass && collectionDefinition.iconCSSClass.length > 0) {
            var nodeIconCSS = collectionDefinition.iconCSSClass;
            var itemImage = document.createElement("div");

            itemImage.setAttribute("class", "tree-item-icon " + nodeIconCSS);
            itemImage.setAttribute("title", collectionDefinition.iconTitle);
            treeNodeContainer.appendChild(itemImage);
        }

        var itemName = document.createTextNode(nodeTitle);
        var nodeItemName = document.createElement("div");
        nodeItemName.appendChild(itemName);
        nodeItemName.setAttribute("class", "tree-node-item-title");
        nodeItemName.setAttribute("title", nodeTitle);
        treeNodeContainer.appendChild(nodeItemName);

        // Remove for now. Add this back later with configuration. Display buttons on a tree node
        //treeNodeContainer.appendChild(createTreeNodeButtons(currentNodeData));

        $(nodeItemName).on('click', function () { nodeClicked(this); });

        if (isSelectNewChildNode === undefined || isSelectNewChildNode) {
            expandParentTreeNodes(nodeItemName);
            selectNode($(nodeItemName)[0], false);
        }

        // Are there any child elements that will be added? If so, create a DIV element to contain them. The 
        // child DIV elements will be attached to this new DIV in a later call to this function. 
        if (hasChildData) {
            var childrenContainerElement = createTreeNodeCollapsableContainerElement(currentNodeData);
            nodeItemListHeader.appendChild(childrenContainerElement);
        }
    }

    // User has clicked on a tree node
    function nodeClicked(clickedDomElem) {
        if (_eventHandlers.itemSelecting) {
            // tell handler that we want to select a new node, give callback for when it is ok
            // to go ahead and select the node
            _eventHandlers.itemSelecting(function () { selectNode(clickedDomElem); });
        } else {
            selectNode(clickedDomElem)
        }
    }

    // A collapsable tree element is a div that holds a tree node's children. The collapsable section essentially hides
    // the child tree nodes until the user clicks on a caret. Then the collaspsable container will display its nodes.
    // This function create the DOM object for a collapsable container element and returns it to the caller.
    function createTreeNodeCollapsableContainerElement(nodeData) {
        var currentNodeCollectionType = nodeData.getCollectionType()
        var childrenContainerElement = document.createElement("div");
        var childElementID = createTreeNodeCollapsableContainerElementID(nodeData.ID, currentNodeCollectionType);
        childrenContainerElement.setAttribute("id", childElementID);
        childrenContainerElement.setAttribute("class", "tree-nested tree-node-collapsed tree-child-node");

        return childrenContainerElement;
    }

    // A tree node action button was clicked. Handle the click event logic.
    // NOTE: Not in use at this time
    function treeNodeButtonClicked(button, actionType) {
        if ($(button).hasClass(_buttonDisabledCssClass) === false) {
            var selectedNodeData = getSelectedNodeData();
            var selectedCollectionDefinition = getTreecollectionDefinition(selectedNodeData.getCollectionType());

            setTimeout(function () {
                if (_eventHandlers.itemButtonClickedEventHandler) {
                    _eventHandlers.itemButtonClickedEventHandler(selectedNodeData, selectedCollectionDefinition, actionType);
                }

                if (actionType === actionDefinitions.copy.action && _eventHandlers.copyButtonClickedHandler) {
                    _eventHandlers.copyButtonClickedHandler(selectedNodeData, selectedCollectionDefinition);
                }

                if (actionType === actionDefinitions.edit.action && _eventHandlers.editButtonClickedHandler) {
                    _eventHandlers.editButtonClickedHandler(selectedNodeData, selectedCollectionDefinition);
                }

                if (actionType === actionDefinitions.delete.action && _eventHandlers.deleteButtonClickedHandler) {
                    _eventHandlers.deleteButtonClickedHandler(selectedNodeData, selectedCollectionDefinition);
                }

            }, 0);
        }

    }

    // Does the user have the permisisons to perform an action on a tree node. The Collection Definition file will send in the user's access permissions
    // for that page. Those permissions that are specified in the colleciton definition will then be compared to the required permissions on the 
    // action as defined int the actionDefinition object.
    function userHasActionPermission(actionDefinition, treecollectionDefinition) {
        var hasAccess = false;

        if ("Permissions" in treecollectionDefinition && "permissionsRequired" in actionDefinition) {
            var permissionsAssignedToUser = treecollectionDefinition.permissions.length > 0 ? treecollectionDefinition.permissions.toLowerCase() : "all";
            var permissionsAssignedToAction = actionDefinition.permissionsRequired.toLowerCase();

            if (permissionsAssignedToUser !== "none") {
                if (permissionsAssignedToUser === "all") {
                    hasAccess = true;
                } else {
                    var permissionsMatchCount = 0;
                    var numberOfPermissionsRequired = permissionsAssignedToAction.length;

                    for (var i = 0; i < permissionsAssignedToAction.length; i++) {
                        var permission = (permissionsAssignedToAction.charAt(i));
                        if (permissionsAssignedToUser.indexOf(permission) >= 0)
                            permissionsMatchCount++;
                    }

                    hasAccess = permissionsMatchCount === numberOfPermissionsRequired;
                }
            }
        } else {
            hasAccess = true;
        }

        return hasAccess;
    }

    // Create all of the action buttons for a tree node. Use the tree node's underlying data object to create the 
    // buttons. This function then returns the full DOM object to add to the tree node container.
    function createTreeNodeButtons(nodeData) {

        var treecollectionDefinitionType = nodeData.getCollectionType();
        var treecollectionDefinition = getTreecollectionDefinition(treecollectionDefinitionType);
        var nodeCommandButtons = document.createElement("div");
        nodeCommandButtons.setAttribute("class", "tree-node-item-buttons-container");

        if (userHasActionPermission(actionDefinitions.edit, treecollectionDefinition))
            nodeCommandButtons.appendChild(createNodeActionButton(actionDefinitions.edit));

        if (userHasActionPermission(actionDefinitions.copy, treecollectionDefinition))
            nodeCommandButtons.appendChild(createNodeActionButton(actionDefinitions.copy));

        var disableDelete = nodeData.hasChildren() === false;
        if (userHasActionPermission(actionDefinitions.delete, treecollectionDefinition))
            nodeCommandButtons.appendChild(createNodeActionButton(actionDefinitions.delete, disableDelete));


        return nodeCommandButtons;
    }

    // Create a button for tree node actions. This is actions such as Add, Delete, Edit, etc. Return the
    // new DOM object to the caller.
    function createNodeActionButton(buttonType, disableButton) {
        var buttonCssClass = buttonType.buttonCssClass;

        if (disableButton)
            buttonCssClass += " " + _buttonDisabledCssClass;

        var button = document.createElement("div");
        button.setAttribute("class", buttonCssClass);
        button.setAttribute("title", buttonType.title);
        button.setAttribute(DATA_ATTRIBUTE_NAMES.ACTION, buttonType.action);
        button.addEventListener('click', function () { treeNodeButtonClicked(this, buttonType.action); });

        return button;
    }

    // Fires when a user clicks on a caret icon
    function caretClicked(caretElement) {
        // If leaf node, nothing to do
        if ($(caretElement).hasClass('tree-last-node-caret')) {
            return;
        }

        var nestedTreeElement = $(caretElement).parent().parent().children(".tree-nested");
        nestedTreeElement.toggleClass("tree-node-expanded");
        nestedTreeElement.toggleClass("tree-node-collapsed");
        $(caretElement).toggleClass("tree-caret-expanded");
        $(caretElement).toggleClass("tree-caret-collapsed");
    }

    // Fires when a user checked on checkbox
    function checkboxChecked() {
        const $treeNodeContainer = getTreeNodeContainer(this);
        const checked = !($(this).closest('span').attr('checked'));

        // Checked all parent nodes of current node if not import will fail due to dependency (Only apply to checked)
        if (checked) {
            const parents = $treeNodeContainer.parentsUntil("#treeNode_root");
            parents.each(function (index, parentElem) {
                const $parentElem = $(parentElem);
                if ($parentElem.hasClass("tree-node-container")) {
                    var checkbox = $parentElem.children('.tree-node-item-container').find('.tree-node-item-checkbox > input[type="checkbox"]');
                    if (checkbox.length > 0)
                        setCheckBoxChecked(checkbox[0], checked);
                }
            });
        }

        // Checked/unchecked all descendent node of current node
        const treeNodeContainerCheckbox = $treeNodeContainer.find('input[type="checkbox"]');
        treeNodeContainerCheckbox.each(function () {
            setCheckBoxChecked(this, checked);
        });

        if (_eventHandlers.checkedboxCheckedHandler) {
            var nodeInfo = getNodeInfo($treeNodeContainer);
            setTimeout(function () { _eventHandlers.checkedboxCheckedHandler(nodeInfo.nodeData, nodeInfo.collectionDefinition) }, 0);
        }

    }

    // Set the checkbox value (check or uncheck)
    function setCheckBoxChecked(checkboxInput, checked) {

        if (checkboxInput.hasAttribute("disabled"))
            return;

        var checkboxSpanId = "#" + checkboxInput.id + "-span";

        // Classic requires the nested <input> to also be marked checked
        var $checkElems = $(checkboxSpanId + ", " + checkboxSpanId + " > input");

        // Update node data checked value
        var $treeNodeContainer = $(checkboxInput).closest(".tree-node-container");
        var nodeInfo = getNodeInfo($treeNodeContainer);
        if (nodeInfo.nodeData.isChecked !== undefined)
            nodeInfo.nodeData.isChecked = checked;

        // fixed View Mode radio button option, replace attr with prop
        if (checked) {
            $checkElems.prop('checked', true);
            $checkElems.attr('checked', 'checked');
        } else {
            $checkElems.prop('checked', false);
            $checkElems.removeAttr('checked');
        }
    }

    //  Toggle the highlight on the selected node and load the modeling page
    function selectNode(treeNode, triggerSelectEvent) {
        triggerSelectEvent = (typeof triggerSelectEvent === 'undefined') ? true : triggerSelectEvent;       // default to true

        var $treeNodeContainer = getTreeNodeContainer(treeNode);
        var nodeInfo = getNodeInfo($treeNodeContainer);

        deselectSelectedNode();
        $treeNodeContainer.addClass("selected-tree-node");
        setTreeNodeToFullWidth(treeNode);
        _currentlySelectedNodeData = nodeInfo.nodeData;

        if (triggerSelectEvent && _eventHandlers.itemSelectedEventHandler) {
            setTimeout(function () {
                _eventHandlers.itemSelectedEventHandler(nodeInfo.nodeData, nodeInfo.collectionDefinition);
            }, 0);
        }
    }

    // Hack to extend a highlight bar all the way to the left hand side of the control as per UX standards.
    // The tree was created before we aware that the bar had to extend the full with of the outer div. The 
    // tree would need a complete rewrite for this. See this hacks gets us around having to completely
    // restrucuture the tree.
    function setTreeNodeToFullWidth(treeNode) {
        var $treeNodeContainer = getTreeNodeContainer(treeNode);
        var widthDifferenceForFullHighlight = $("#" + _outputTreeToElementId).offset().left - $treeNodeContainer.children(".tree-node-item-container").offset().left;
        var widthDifferenceForFullHighlightPositive = Math.abs(widthDifferenceForFullHighlight);
        $treeNodeContainer.children(".tree-node-item-container")
            .css("margin-left", widthDifferenceForFullHighlight.toString() + "px")
            .addClass("fullWidthHighlight");

        $treeNodeContainer.children(".tree-node-item-container").children().first()
            .css("margin-left", widthDifferenceForFullHighlightPositive.toString() + "px")
            .addClass("fullWidthHighlight");
    }

    // for the given node container element, return object with props for the collection def
    // and the node data
    function getNodeInfo($nodeContainer) {
        var collectionDefinitionKey = $nodeContainer.attr(DATA_ATTRIBUTE_NAMES.COLLECTION);
        var collectionDefinition = getTreecollectionDefinition(collectionDefinitionKey);

        var instanceID = $nodeContainer.attr(DATA_ATTRIBUTE_NAMES.NODEID);
        var dataType = collectionDefinition.dataType;

        return {
            collectionDefinition: collectionDefinition,
            nodeData: treeDataProvider.findNodeById(instanceID, dataType)
        }
    }

    // A tree node container is a div that holds all of the UI elements, data properties that up a node in the tree display.
    // The function can receive any aspect of a tree node element and it will get it's container element and return it to
    // the caller.
    function getTreeNodeContainer(selectedElement) {
        var $nodeContainerElement = null;

        if ($(selectedElement).hasClass("tree-node-container")) {
            $nodeContainerElement = $(selectedElement);
        } else if ($(selectedElement).parent().hasClass("tree-node-container")) {
            $nodeContainerElement = $(selectedElement).parent();
        } else {
            var $ancesterBeforeNodeContainer = $(selectedElement).parentsUntil(".tree-node-container");
            $nodeContainerElement = $ancesterBeforeNodeContainer.last().parent();
        }

        return $nodeContainerElement;
    }

    return treeControlInterface;

})();
