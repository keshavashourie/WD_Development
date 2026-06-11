// This class object manages data for the tree control. It handles all CRUD operations for the data.
// Additionally it "compiles" the data submitted by the program to add addtional data and functionality
// to it that can be then used for business logic.

// Documentation on the "compiled" data functions and properties can be found at: https://teams.microsoft.com/l/entity/com.microsoft.teamspace.tab.wiki/tab::b6c8f981-a3f2-432b-9c38-cb5023694410?context=%7B%22subEntityId%22%3A%22%7B%5C%22pageId%5C%22%3A319%2C%5C%22sectionId%5C%22%3A320%2C%5C%22origin%5C%22%3A2%7D%22%2C%22channelId%22%3A%2219%3Aaa347b3128bb4d558c3579dffe53d532%40thread.skype%22%7D&tenantId=6b5bd02b-92d2-40b2-9ffd-c9c94280c757

var treeDataProvider = (function () {
    'use strict';

    var _nodeData = null;

    // Public functions interface
    var dataProviderInterface = {
        initialize: initialize,
        getAllNodes: getAllNodes,
        getRootCollectionNodes: getRootCollectionNodes,
        findNodeByProperty: findNodeByProperty,
        findNodeById: findNodeById,
        findNodeByName: findNodeByName,
        createDefaultIterationOptionsObject: createDefaultIterationOptionsObject,
        iterateNodes: initiateNodeIteration,
        addNode: addNewNode,
        deleteNode: deleteNode,
        updateNode: updateNode,
        getEditPreview: createEditPreview
    };

    // Initialize the data
    function initialize(nodeDataJSON) {
        _nodeData = nodeDataJSON;
        compileData();
    }

    // Add a new node to the root node or to an existing parent node. If the parent parameter is null or undefined
    // then the new node will be assigned to the root collection.
    function addNewNode(newNodeData, parentNode, nodeDataType) {

        if (!parentNode) {
            var rootNodeCollectionType = treeConfiguration.rootCollectionName;

            compileNodeData(null, newNodeData, rootNodeCollectionType);
            getAllNodes()[rootNodeCollectionType].push(newNodeData);
        } else {
            var dataType = treeDataTypeDefinition.getDefinition(nodeDataType);
            var collectionType = dataType.collectionType;

            compileNodeData(parentNode, newNodeData, collectionType);

            if (!parentNode[collectionType])
                parentNode[collectionType] = [];

            parentNode[collectionType].push(newNodeData);
        }
    }

    // Change the collection a node belongs to
    function changeNodeParent(nodeToMove, updatedNodeData) {
        var editPreview = createEditPreview(nodeToMove, updatedNodeData);

        var newParentNode = editPreview.updatedParent;
        var newDataType = editPreview.updatedDataType;
        if (newDataType !== null) {
            var newCollectionType = editPreview.hasDataTypeChanged ? newDataType.getCollectionDefinition().getCollectionName() : nodeToMove.getCollectionType();

            var formerParentNode = nodeToMove.getParentNode();
            var formerNodeCollectionType = nodeToMove.getCollectionType();
            var formerCollectionNodes = formerParentNode[formerNodeCollectionType];
            var position = -1;

            for (var index in formerCollectionNodes) {
                var node = formerCollectionNodes[index];
                // Locate the node to move in the parent collection
                if (nodeToMove.ID === node.ID) {
                    position = index;
                    break;
                }
            }

            if (position >= 0) {
                compileNodeData(newParentNode, nodeToMove, newCollectionType);

                // Does parent node contain the key for the collection? If not, add it.
                if (!newParentNode[newCollectionType])
                    newParentNode[newCollectionType] = [];

                // add the node to the new parent under the correct collection.
                newParentNode[newCollectionType].push(nodeToMove);

                // Remove the node from the original parent node.
                formerParentNode[formerNodeCollectionType].splice([position], 1);

                // If the orginal parent no longer has elements in the collection the node was removed from,
                // delete the key.
                if (formerParentNode[formerNodeCollectionType].length === 0)
                    delete formerParentNode[formerNodeCollectionType];
            } else {
                throw ("Unable to find the node to delete in the nodes collection.");
            }
        }
    }

    if (typeof Object.assign !== 'function') {
        // Must be writable: true, enumerable: false, configurable: true
        Object.defineProperty(Object, "assign", {
            value: function assign(target, varArgs) { // .length of function is 2
                'use strict';
                if (target === null || target === undefined) {
                    throw new TypeError('Cannot convert undefined or null to object');
                }

                var to = Object(target);

                for (var index = 1; index < arguments.length; index++) {
                    var nextSource = arguments[index];

                    if (nextSource !== null && nextSource !== undefined) {
                        for (var nextKey in nextSource) {
                            // Avoid bugs when hasOwnProperty is shadowed
                            if (Object.prototype.hasOwnProperty.call(nextSource, nextKey)) {
                                to[nextKey] = nextSource[nextKey];
                            }
                        }
                    }
                }
                return to;
            },
            writable: true,
            configurable: true
        });
    }

    // Update a node. The updatedNodeData parameter has the new JSON data for the update.
    function updateNode(nodeToUpdate, updatedNodeData) {
        Object.assign(nodeToUpdate, updatedNodeData);
        var editPreview = createEditPreview(nodeToUpdate, updatedNodeData);

        if (editPreview.hasParentChanged) {
            changeNodeParent(nodeToUpdate, updatedNodeData);
        }
    }

    // The edit preview is used to drive business logic elsewhere. It calculates the impact of what updating a node's data
    // will before it occurs.There are multiple places in the tree code need to know this information so this function removes
    // duplication. A question a function may want to know is, did the parent of the node being edited change? If so, what is the
    // extent of the changes as the impact the structure of the tree node data. Based on the answers to those questions, the 
    // business logic of the tree can prepare for what kinds of changes are needed before the node's data is permanently
    // changed and the previous state of the data is permanenly lost.
    function createEditPreview(originalDataNode, updatedNodeData) {
        var dataTypeForUpdateFieldName = originalDataNode.dataType.dataTypeForUpdateFieldMapping;
        var dataTypeForUpdateName = dataTypeForUpdateFieldName.length > 0 ? updatedNodeData[dataTypeForUpdateFieldName] : "";
        var dataTypeForUpdate = dataTypeForUpdateName.length > 0 ? treeDataTypeDefinition.getDefinition(dataTypeForUpdateName) : null;
        var currentDataTypeName = originalDataNode.getDataTypeName();
        var hasDataTypeChanged = dataTypeForUpdate != null && dataTypeForUpdateName !== currentDataTypeName;
        //var hasDataTypeChanged = (dataTypeForUpdate != null && dataTypeForUpdateName !== currentDataTypeName) || (dataTypeForUpdate === null && currentDataTypeName !== null && currentDataTypeName.length > 0);
        var dataTypeObject = !hasDataTypeChanged ? originalDataNode.dataType : treeDataTypeDefinition.getDefinition(dataTypeForUpdateName);

        var parentIdForUpdateFieldName = dataTypeObject !== null ? dataTypeObject.parentIdForUpdateFieldMapping : "";
        var parentForUpdateId = parentIdForUpdateFieldName.length > 0 ? updatedNodeData[parentIdForUpdateFieldName] : "";
        var currentParentId = originalDataNode.hasParentNode() ? originalDataNode.getParentNode().getId() : "";
        var hasParentChanged = (parentForUpdateId.length > 0 && parentForUpdateId !== currentParentId) || (parentForUpdateId.length === 0 && currentParentId.length > 0);

        var origParentDataType = originalDataNode.getParentNode() ? originalDataNode.getParentNode().dataType : null;
        var updateParent = hasParentChanged ? findNodeById(parentForUpdateId, origParentDataType) : null;
        if (updateParent === null && hasParentChanged) {
            updateParent = findNodeById(parentForUpdateId, dataTypeObject);
            if (updateParent === null && hasDataTypeChanged && dataTypeObject !== null) {
                dataTypeObject = treeDataTypeDefinition.getDefinition(dataTypeObject.parentDataTypes[0].name)
                updateParent = findNodeById(parentForUpdateId, dataTypeObject);
            }
        }
        var properties = {
            "hasParentChanged": hasParentChanged,
            "hasDataTypeChanged": hasDataTypeChanged,
            "originalParent": originalDataNode.hasParentNode() ? originalDataNode.getParentNode() : null,
            "updatedParent": updateParent,
            "originalDataType": originalDataNode.dataType,
            "updatedDataType": hasDataTypeChanged ? dataTypeForUpdate : null
        };

        return properties;
    }

    // Remove a node from its collection
    function deleteNode(nodeToDelete) {
        var nodeCollectionType = nodeToDelete.getCollectionType();
        var nodeToDeleteParent = nodeToDelete.getParentNode();
        var collectionNodes = nodeToDeleteParent != null ? nodeToDelete.getParentNode()[nodeCollectionType] : getRootCollectionNodes();
        var position = -1;

        for (var index in collectionNodes) {
            var node = collectionNodes[index];

            if (nodeToDelete.getId() === node.getId()) {
                position = index;
                break;
            }
        }

        if (position >= 0) {
            if (nodeToDeleteParent) {
                nodeToDeleteParent[nodeCollectionType].splice([position], 1);

                if (nodeToDeleteParent[nodeCollectionType].length === 0)
                    delete nodeToDeleteParent[nodeCollectionType];
            } else {
                getAllNodes()[treeConfiguration.rootCollectionName].splice([position], 1);
            }
        } else {
            throw ("Unable to find the node to delete in the nodes collection.");
        }
    }

    // Get all data nodes starting with the root collection
    function getAllNodes() {
        return _nodeData;
    }

    // Get an array of nodes that are at the root of the tree
    function getRootCollectionNodes() {
        var allNodes = getAllNodes();
        return allNodes ? allNodes[treeConfiguration.rootCollectionName] : [];
    }

    // The default options are used by the iterator function. The caller would first call
    // this function to get a blank object. The developer would then fill in whatever properties
    // they need to configure.
    function createDefaultIterationOptionsObject() {
        var iterationOptions = {
            "acceptanceCriteriaHandler": null,
            "processNodeHandler": null,
            "collectionIterationCompleteHandler": null
        };

        return iterationOptions;
    }

    // Find a node by its id property value
    function findNodeById(id, dataType) {
        return findNodeByProperty(id, "id", dataType);
    }

    // Find a node by its name property value
    function findNodeByName(name, dataType) {
        return findNodeByProperty(name, "name", dataType);
    }

    // Find a node by any given property on the data as needed by the developer.
    function findNodeByProperty(value, propertyToMatchOn, dataType) {
        var iterationOptions = createDefaultIterationOptionsObject();
        var nodeDataTypeDef = treeDataTypeDefinition.getDefinition(dataType);
        var nodeCollectionType = nodeDataTypeDef ? nodeDataTypeDef.collectionType : "";

        var matchCriteria = {
            "valueToFind": value,
            "propertyToMatchOn": propertyToMatchOn,
            "nodeCollectionType": nodeCollectionType
        };

        iterationOptions.acceptanceCriteriaHandler = isSearchMatchHandler

        var node = initiateNodeIteration(iterationOptions, matchCriteria);
        return node;
    }

    // When the devloper searches for data, this function will be called by the iterator function to
    // do the procecessing of the data. It will do a comparision of data as configured by the user
    function isSearchMatchHandler(parentNode, nodeData, collectionType, valueFromCaller) {
        var isMatch = (valueFromCaller.nodeCollectionType.length === 0 || collectionType === valueFromCaller.nodeCollectionType) &&
            ((valueFromCaller.propertyToMatchOn === "id" && valueFromCaller.valueToFind === nodeData.getId()) ||
                (valueFromCaller.propertyToMatchOn === "name" && valueFromCaller.valueToFind === nodeData.getName()) ||
                valueFromCaller.valueToFind === nodeData[valueFromCaller.propertyToMatchOn]);

        return isMatch;
    }

    // Set up the node iterator so that we can do a compilation.
    function compileData() {
        var iterationOptions = createDefaultIterationOptionsObject();
        iterationOptions.processNodeHandler = compileNodeData;

        initiateNodeIteration(iterationOptions);
    }

    // initiate the node iteration processing
    function initiateNodeIteration(iterationOptions, valueFromCaller) {
        return iterateNodes(iterationOptions, valueFromCaller);
    }

    // A generic iteration. Loop through the node structure and get the data in collections.
    function iterateNodes(iterationOptions, valueFromCaller, parentNode, currentNodeCollection, currentNodeCollectionType) {
        if (!iterationOptions)
            throw "IterationOptions missing";

        var acceptanceCriteriaHandler = iterationOptions.acceptanceCriteriaHandler;
        var processNodeHandler = iterationOptions.processNodeHandler;
        var collectionIterationCompleteHandler = iterationOptions.collectionIterationCompleteHandler
        var nodeDataRet = null;


        if (!parentNode) {
            currentNodeCollection = getRootCollectionNodes();
            currentNodeCollectionType = treeConfiguration.rootCollectionName;
            parentNode = null;

            if (typeof valueFromCaller === "undefined")
                valueFromCaller = null;
        }

        $.each(currentNodeCollection, function (index, currentNodeData) {
            if (processNodeHandler !== null)
                processNodeHandler(parentNode, currentNodeData, currentNodeCollectionType, valueFromCaller);

            var returnCurrentNode = acceptanceCriteriaHandler === null ? false : acceptanceCriteriaHandler(parentNode, currentNodeData, currentNodeCollectionType, valueFromCaller);
            if (returnCurrentNode) {
                nodeDataRet = currentNodeData;
                return false;
            }

            var dataTypeCollectionChildTypes = currentNodeData.dataType.childCollectionTypes;

            // Now we want to get the children of the current data node. The name propery of the name/value pairs include id, name along with the arrays
            // of child elements. To distinguish between the name/value pairs the describe the object versus its children, we will use the child collection types defined in 
            // the dataType.
            $.each(dataTypeCollectionChildTypes, function (indexOfDefinition, childnodeCollectionType) {
                var childData = currentNodeData[childnodeCollectionType];
                if (childData) {
                    var ret = iterateNodes(iterationOptions, valueFromCaller, currentNodeData, childData, childnodeCollectionType);
                    if (ret != null) {
                        nodeDataRet = ret;
                        return false;
                    }
                }
            });

            if (nodeDataRet !== null)
                return false;
        });

        if (collectionIterationCompleteHandler)
            collectionIterationCompleteHandler(parentNode, currentNodeCollection, currentNodeCollectionType);

        return nodeDataRet;
    }

    // A compilation will add additional properties and functions to the JSON data 
    // object that was passed into the class object.

    // For more informaton see: https://teams.microsoft.com/l/entity/com.microsoft.teamspace.tab.wiki/tab::b6c8f981-a3f2-432b-9c38-cb5023694410?context=%7B%22subEntityId%22%3A%22%7B%5C%22pageId%5C%22%3A319%2C%5C%22sectionId%5C%22%3A320%2C%5C%22origin%5C%22%3A2%7D%22%2C%22channelId%22%3A%2219%3Aaa347b3128bb4d558c3579dffe53d532%40thread.skype%22%7D&tenantId=6b5bd02b-92d2-40b2-9ffd-c9c94280c757
    function compileNodeData(parentNode, nodeToCompileData, collectionType) {
        var nodeDefinition = treeCollectionDefinition.getDefinition(collectionType);
        var parentRef = parentNode ? parentNode : null;

        // Set the datatype of this node
        nodeToCompileData.dataType = treeDataTypeDefinition.getDefinition(nodeDefinition.dataType.name);

        //nodeToCompileData._compiled = {
        //    "parentNode": parentRef
        //};

        // Get the parent node object
        nodeToCompileData.getParentNode = function () {
            return parentRef;
        }

        // Does this node have a parent node?
        nodeToCompileData.hasParentNode = function () {
            return this.getParentNode() != null ? true : false;
        }

        //  Get the name of the datatype of this node
        nodeToCompileData.getDataTypeName = function () {
            return this.dataType.name;
        }

        // Get the name of the collection type this node belongs to.
        nodeToCompileData.getCollectionType = function () {
            return this.dataType.collectionType;
        }

        // Get the id field value of this node. This is used because we allow the developer to define what the property name of the 
        // id field is in the configuration.
        nodeToCompileData.getId = function () {
            return this[this.dataType.idFieldMapping];
        }

        // Get the name field value of this node. This is used because we allow the developer to define what the property name of the 
        // name field is in the configuration.
        nodeToCompileData.getName = function () {
            return this[this.dataType.nameFieldMapping];
        }

        // Does this node have child nodes?
        nodeToCompileData.hasChildren = function () {
            var hasChildren = false;
            var thisNode = this;

            $.each(thisNode.dataType.childCollectionTypes, function (indexOfDefinition, childNodeCollectionKey) {
                if (thisNode[childNodeCollectionKey]) {
                    hasChildren = true;
                    return false;
                }
            });

            return hasChildren;;
        }
    }

    return dataProviderInterface;

})();
