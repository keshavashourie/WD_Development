/*
***************************************************************************
Copyright 2022 Siemens
Client side script for the SEMI Landing Page
***************************************************************************
*/

var scsLandingPage = (function () {
    'use strict';

    // interface
    var scsLandingPageInterface = {
        initialize: initialize
    }
    return scsLandingPageInterface;

    function initialize() {
        //register the listener for the refresh landing page
        var div = document.getElementById('WebPart_scsGeneralWP_UIComponent');
        div.addEventListener('refreshLandingPage', function (e) {
            $("#ctl00_WebPartManager_ButtonsBar_RefreshButton").click();
        });
    }
})();