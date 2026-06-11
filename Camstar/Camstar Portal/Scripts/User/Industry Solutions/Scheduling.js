var isScheduling = (function () {
    'use strict';

    // handle to interval
    var refreshInterval = null;
    var lastTimer = null;

    $(function () {
        if (__page.get_isResponsive()) {
            $('#WebPart_ActionsControl_UIComponent').hide();
        }

        setRefreshInterval();

        $(window).on('beforeunload', function () {
            clearInterval(refreshInterval);
            refreshInterval = null;
        });
    });

    function doToggleClick() {
        $(".close-button-desktop").click();
    }
    function scheduleSearchAddSlideoutToogler(resultsGridID, click) {
        if (!click) {
            let $wpSearch = $(".common-search-panel");
            $wpSearch.show();
        }
        SearchLayout_AddSlideoutToogler(resultsGridID);
        
        if (click)
            setTimeout(function () { doToggleClick(); }, 0);
    }

    function doSearchClick() {
        $("#ctl00_WebPartManager_SearchWP_SearchButton").click();
    }

    // get value from slider and set refresh interval
    function setRefreshInterval() {
        var $refreshSecondsSlider = $('#RefreshIntervalRange');
        if (refreshInterval)
            clearInterval(refreshInterval);

        refreshInterval = setInterval(function () { doSearchClick(); }, $refreshSecondsSlider.val() * 1000);
        lastTimer = $refreshSecondsSlider.val();

        // Update the text box 
        $('#refreshSeconds').html($refreshSecondsSlider.val());
    }

    function initialize(gridID, click) {
        var $refreshSecondsSlider = $('#RefreshIntervalRange');
        if (lastTimer) {
            $('#refreshSeconds').html(lastTimer);
            $refreshSecondsSlider.val(lastTimer);
        }

        $refreshSecondsSlider.on('input change', setRefreshInterval);
        scheduleSearchAddSlideoutToogler(gridID, click);

        if (isTimerSlider) {
            isTimerSlider.initialize();
        }
    }

    var isSchedulingInterface = {
        initialize: initialize
    };

    return isSchedulingInterface;
})();

// Configured in Portal Studio - triggered after grid has been rendered
// Tried to encapsulate this, but seems Portal Studio can only be configured to call global functions
function mfgOrderGridRenderCompleted() {
    var $grid = $(this.GridID);
    var mfgOrders = $grid.getRowData();

    $(mfgOrders).each(function () {
        try {
            let mfgOrder = this;

            // set styling based on any missed start/complete dates
            let actualStartDateTicks = parseInt(mfgOrder.ActualStartDateTicks, 10);
            let plannedStartDateTicks = parseInt(mfgOrder.PlannedStartDateTicks, 10);
            let plannedCompletionDateTicks = parseInt(mfgOrder.PlannedCompletionDateTicks, 10);
            let mfgOrderQty = parseInt(mfgOrder.MfgOrderQty, 10);
            let processedQuantity = parseInt(mfgOrder.ProcessedQuantity, 10);

            let nowTicks = new Date().getTime();

            // find the corresponding row in the grid
            var $matchingTr = $grid.find('td[title="' + mfgOrder.InstanceID + '"]').closest('tr');

            // -1 indicates not set
            // we have a planned start date and the nothing has been started or we started after planned
            if (plannedStartDateTicks !== -1 && ((actualStartDateTicks === -1 && plannedStartDateTicks < nowTicks) || (actualStartDateTicks > plannedStartDateTicks))) {
                $matchingTr.find('td[aria-describedby$="_ActualStartDate"]').addClass('is-alert');
                //$matchingTr.find('td[aria-describedby$="_PlannedStartDate"]').addClass('is-alert');
            }

            // planned completion date is in the past and we have processed less than the required quantity
            if (plannedCompletionDateTicks !== -1 && plannedCompletionDateTicks < nowTicks && processedQuantity < mfgOrderQty) {
                $matchingTr.find('td[aria-describedby$="_PlannedCompletionDate"]').addClass('is-alert');
            }

        } catch (ex) {
            console.error(ex);
        }
    });
}
