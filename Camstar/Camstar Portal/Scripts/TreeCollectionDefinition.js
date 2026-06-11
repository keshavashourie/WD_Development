// This class object handles the "Collection Definitions" for the Hierarchical Tree Control. It is intitialzed with a JSON document. See the 
// team wiki for more information. Each node of data in the tree belongs to a parent collection. In other word, collections collect data. For that reason the
// data inside a collection shares a lot attributes in common. So the first use of the this Collection Definition object is to have a way to store the 
// shared metadata for a grouping of data nodes. This data can then be used to control rendering and business logic.Another use of the data is that it defines
// relationshipts. The node data sent into the tree control is system agnostic. Out of the box the tree has no idea what properties in the node data are 
// data and which ones are collections of child nodes. Therefore the definition file that is class manages contains that information.
var treeCollectionDefinition = (function () {
    'use strict';

    var _collectionDefinitions = null;

    // Public functions interface
    var definitionDataInterface = {
        initialize: initialize,
        getNodeRootDefinition: getRootNodeCollectionDefinition,
        getDefinition: getDefinition,
        iterateCollectionDefinitions: iterateCollectionDefinitions
    }

    // Init the class with data
    function initialize(collectionDefinitions) {
        _collectionDefinitions = collectionDefinitions;
        iterateCollectionDefinitions(compileCollectionDefinition);
    }

    // Getting to the root node is a bit different then the others. This function will get the caller the root node's data
    function getRootNodeCollectionDefinition() {
        return _collectionDefinitions[treeConfiguration.rootCollectionName];
    }

    // Get a collection defintion object based on its key. Every entry in the definition file has a key that Javascript can access. That key is the 
    // collection name. See the team's wiki for more inforation.
    function getDefinition(nodeCollectionType) {
        return _collectionDefinitions[nodeCollectionType];
    }

    // A compilation will iterate over every definition object and add additional properties and functions to them. Remember
    // that the definition starts its life as a JSON document (probably as a string first). By the time it gets into this class instance
    // it will be a javascript object. A "compilation" is simply an enhancement of the existing data.
    function compileCollectionDefinition(parentCollectionDefinitionType, collectionDefinitionTypeKey) {
        var collectionDefinition = getDefinition(collectionDefinitionTypeKey);
 
        collectionDefinition.getCollectionName = function () { return collectionDefinitionTypeKey; }
    }

    // A generic iteration function. This function will iterate over each node of the Collections Definiton file and use callbacks set
    // by the caller to pass back information to be used is the caller's business logic.
    function iterateCollectionDefinitions(processCollectionHandler, parentCollectionType, currentCollectionDefinitionType) {

        if (!processCollectionHandler)
            throw "processCollectionHandler missing";

        if (!currentCollectionDefinitionType || currentCollectionDefinitionType.length === 0) {
            parentCollectionType = "";
            currentCollectionDefinitionType = treeConfiguration.rootCollectionName;
        }

        var nodeCollectionDefinition = getDefinition(currentCollectionDefinitionType);
        processCollectionHandler(parentCollectionType, currentCollectionDefinitionType);

        $.each(nodeCollectionDefinition.childCollections, function (indexOfDefinition, childCollectionDefinitionType) {
            if (childCollectionDefinitionType.length > 0)
                iterateCollectionDefinitions(processCollectionHandler, currentCollectionDefinitionType, childCollectionDefinitionType);
        });
    }

    return definitionDataInterface;

})();
