// Copyright Siemens 2023

var isLogDefectPopup = (function () {
    'use strict';

    let isLogDefectPopupInterface = {
        initialize: initialize
    };

    let CTRL_SELECTORS = {
        ADD_BUTTON: '#ctl00_WebPartManager_BlankWP_AddMaterialListItems'
    };

    //
    function initialize() {
        pop.GetCallerPage().pop.maximize();

        let $matListItemGrid = $("#ctl00_WebPartManager_BlankWP_MaterialListItemsGrid");

        // Add button enable/disable
        $(CTRL_SELECTORS.ADD_BUTTON).prop('disabled', true);

        // Triggered when user clicks the "Select All" checkbox in the header
        $matListItemGrid.bind("jqGridSelectAll", function (event, selRowIds, selected) {
            // This event sends all selected row IDs regardless of whether the user is selecting or un-selecting

            // enabled state of Add button
            $(CTRL_SELECTORS.ADD_BUTTON).prop('disabled', !selected);
        });

        // listen for select row event
        $matListItemGrid.bind("jqGridSelectRow", function (event, rowId, originalEvent) {
            var selRowIds = $matListItemGrid.jqGrid("getGridParam", "selarrrow");
            // enabled state of Add button
            $(CTRL_SELECTORS.ADD_BUTTON).prop('disabled', !(selRowIds?.length));
        });
    }

    return isLogDefectPopupInterface;
})();