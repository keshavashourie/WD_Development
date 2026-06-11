// This class object manages the configuration data for the Hierarchical Tree Control.
var treeConfiguration = (function () {
    'use strict';

    var _configuration = null;

    // Public functions interface
    var configurationDataInterface = {
        initialize: initialize,
        getFieldMappings: getFieldMappings,
        configurationData: _configuration,
        rootCollectionName: "",
        enableTreeAutoDisable: false,
        defaultTitleTemplate: ""
    }

    // Initialize the class. The configurationData paramater is the JSON object containing the config data.
    function initialize(configurationData) {
        _configuration = configurationData;

        configurationDataInterface.configurationData;
   
        // The name of the first node key, that forms the root of the tree
        configurationDataInterface.rootCollectionName = configurationData.rootCollectionName;

        // Not used at this time. Can disable the tree automatically when a record is being edited.
        configurationDataInterface.enableTreeAutoDisable = configurationData.enableTreeAutoDisable;

        // The default title format for creating tree node titles. If not set the the item's name is the default.
        configurationDataInterface.defaultTitleTemplate = configurationData.defaultTitleTemplate;
    }

    // GEt an array of all configured field mappings.
    function getFieldMappings() {
        return _configuration.fieldMappings;
    }

    return configurationDataInterface;

})();
