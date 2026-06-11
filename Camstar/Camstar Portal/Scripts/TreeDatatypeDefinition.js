// The application will take various data available to it and compile a dataType object from it.Therefore the dataType is build dynamically based on other data.
// The dataType will then be bound to the appropriate node data objects.The purpose of creating a dataType dynamically is that it is easy to bind a data node 
// to its collection definition and create linkage between the two. Put another way a dataType not only identieis what a JSON object is, it acts as a bridge
// between a data node and it's collecion definition.
var treeDataTypeDefinition = (function () {
    'use strict';

    var _dataTypeDefinitions = [];

    // Public functions interface
    var dataTypeDefinitionDataInterface = {
        initialize: initialize,
        getDefinition: getDefinition,
        getDefinitionByCollectionKey: getDefinitionByCollectionKey,
    }

    // Initialize the class object.
    function initialize() {
        treeCollectionDefinition.iterateCollectionDefinitions(compileDataTypeDefinitions);
    }

    // The caller can get a data type definition object by passing in its name or an object with a name property.
    function getDefinition(dataType) {

        // Get data type name. This values can then be used to get the definition. There are three possible ways that this request can be made
        // 1) Just the name
        // 2) An uncompiled dataType - There are instances such as after a postback where the dataType object being passed in has not been
        //      compiled. This function will take the name property from that object and get the full dataType definition based on that.
        // 3) A compiled dataType object is passed in - This will function just like #2 above does. The end result is the object that gets
        //      passed in will be the same as the object that gets returned.
        //
        //  The goal is to provide consistent output regardless of the input.
        var dataTypeName = "";
		var dataTypeDef = null;
		if (dataType) {
			if (typeof dataType === "string") {
				dataTypeName = dataType;
			} else if (dataType.hasOwnProperty("name")) {
				dataTypeName = dataType.name;
			} else if (dataType.hasOwnProperty("Name")) {
				dataTypeName = dataType.Name;
			} else {
				dataTypeDef = _dataTypeDefinitions[dataTypeName];
			}
			if (dataTypeDef === null)
				dataTypeDef = _dataTypeDefinitions[dataTypeName];
		}
        if (!dataTypeDef)
            dataTypeDef = null;

        return dataTypeDef;
    }

    // Get a dataType by the name of its associated collection type
    function getDefinitionByCollectionKey(collectionKey) {
        var collectionDefinition = treeCollectionDefinition.getDefinition(collectionKey);
        var dataTypeToReturn = null;

        if (collectionDefinition) {
            dataTypeToReturn = getDefinition(collectionDefinition.dataType.name);
        }

        return dataTypeToReturn;
    }

    // Private. Create the dataType definition based off of the information in the collection definition data.
    function compileDataTypeDefinitions(parentCollectionDefinitionType, collectionDefinitionType) {
        // Build the dataType definitions
        var collectionDefinition = treeCollectionDefinition.getDefinition(collectionDefinitionType);
        var parentCollectionDefinition = parentCollectionDefinitionType.length > 0 ?
            treeCollectionDefinition.getDefinition(parentCollectionDefinitionType) : null;

        var parentDataTypeName = parentCollectionDefinition != null ? parentCollectionDefinition.dataType.name : "";
        var dataTypeName = collectionDefinition.dataType.name;
        var dataTypeDefinitionObject = getDefinition(dataTypeName);
        var parentDataTypeObject = parentCollectionDefinition != null ? getDefinition(parentDataTypeName) : null;

        if (!dataTypeDefinitionObject) {
            // The dataType does not exist yet. Create it here and add it to the array.
            // Derived Properties: Name, Title.
            _dataTypeDefinitions[dataTypeName] = collectionDefinition.dataType;

            // Now add additional properties
            dataTypeDefinitionObject = _dataTypeDefinitions[dataTypeName];
            dataTypeDefinitionObject.collectionType = collectionDefinitionType;
            dataTypeDefinitionObject.childCollectionTypes = collectionDefinition.childCollections;
            dataTypeDefinitionObject.childDataTypes = [];
            dataTypeDefinitionObject.parentDataTypes = [];
            dataTypeDefinitionObject.idFieldMapping = getMappedFieldName(collectionDefinition, "id", true);
            dataTypeDefinitionObject.nameFieldMapping = getMappedFieldName(collectionDefinition, "name", true)
            dataTypeDefinitionObject.parentIdForUpdateFieldMapping = getMappedFieldName(collectionDefinition, "parentIdForUpdate", false);
            dataTypeDefinitionObject.dataTypeForUpdateFieldMapping = getMappedFieldName(collectionDefinition, "dataTypeForUpdate", false);

            dataTypeDefinitionObject.getCollectionDefinition = function () {
                return treeCollectionDefinition.getDefinition(this.collectionType);
            }
        }

        if (parentDataTypeObject) {
            parentDataTypeObject.childDataTypes.push(dataTypeDefinitionObject)
            dataTypeDefinitionObject.parentDataTypes.push(parentDataTypeObject);
        }
    }

    // Create the field mapping name. The tree maps several field names such as id and name. See the tree configuration and the 
    // collection defnitions for more information. This function traverses a hiararchy of data to get a name. The name is created as
    // follows:
    //  1. Does the name exist on the collection definition? Use that value.
    //  2. Does the name exist in the global configuration? Use that value. 
    //          (NOTE: The collection definition and the config objects both need to use the same exact variable name for the field)
    //  3. Should the function use the field key named passed in as the name (useKeyAsDefault = true)
    //  4. Return blank. No field name defined.
    function getMappedFieldName(collectionDefinition, fieldNameKey, useKeyAsDefault) {
        var fieldMappingsList = collectionDefinition.dataType.fieldMappings;
        var fieldName = fieldMappingsList[fieldNameKey];

        if (!fieldName || fieldName.length == 0) {
            fieldMappingsList = treeConfiguration.getFieldMappings();
            fieldName = fieldMappingsList ? fieldMappingsList[fieldNameKey] : "";
            if (!fieldName || fieldName.length == 0) {
                fieldName = useKeyAsDefault ? fieldNameKey : "";
            }
        }

        return fieldName;
    }

    return dataTypeDefinitionDataInterface;

})();
