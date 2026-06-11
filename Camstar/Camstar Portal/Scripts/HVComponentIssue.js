var csiHVComponentIssue = (function () {
    'use strict';

    var api = {
        initToggleContainers: initToggleContainers
    };

    return api;

    function toggleContainerClicked(toggle) {
        //console.log('toggleContainerClicked with arg [' + toggle + ']');
        var toggleExpandedId, dataLoadedId, buttonId;

        if (toggle === 'issue') {
            toggleExpandedId = 'ctl00_WebPartManager_HVIssuedComponentsWP_IssueToggleExpanded_ctl00';
            dataLoadedId = 'ctl00_WebPartManager_HVIssuedComponentsWP_IssueDataLoaded_ctl00';
            buttonId = 'ctl00_WebPartManager_HVIssuedComponentsWP_GetIssuedComponents';
        } 

        if (!toggleExpandedId) {
            return;
        }

        var $toggleExpanded = $('#' + toggleExpandedId);
        var $dataLoaded = $('#' + dataLoadedId);

        var toggleExpanding = $toggleExpanded.val() === 'no';
        var dataLoaded = $dataLoaded.val() === 'yes';
        $toggleExpanded.val(toggleExpanding ? 'yes' : 'no');

        console.log('toggleExpanding: ' + toggleExpanding + ', dataLoaded: ' + dataLoaded);
        if (toggleExpanding && !dataLoaded) {
            setTimeout(function () {
                console.log('clicking ' + toggle + ' button');
                $('#' + buttonId).click();
            }, 100);
            // server logic updates dataLoaded control value
        }

    }

    function initToggleContainers() {
        //console.log('initToggleContainers');

        //$('#ctl00_WebPartManager_IssueOptionsWP_HVIssuedComponentsToggle').hide();
        $('div#ctl00_WebPartManager_IssueOptionsWP_HVIssuedComponentsToggle .toggle-container .header').on('click', function (e) { toggleContainerClicked('issue'); });
    }

})();

