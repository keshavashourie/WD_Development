var MDL_InstanceList = (function () {
    'use strict';

    // Public functions interface
    var instanceListInterface = {
        hideInstanceList: hideInstanceList
    };

    // If the HideInstanceList querystring value set to true then on page load hide the Instance List
    $(document).ready(function () {
        var hideInstanceList = window.location.href.indexOf("HideInstanceList=true") != -1;

        if (hideInstanceList)
            $("#WebPart_MDL_Filter_WP").hide();

        setTimeout(focusName, 100);
    });

    function focusName() {
        $('#ctl00_WebPartManager_MDL_InstanceHeader_NameTxt_ctl00').focus();
    }

    // On postback we need to be able rehide the instance list if required from the server.
    function hideInstanceList() {
        $("#WebPart_MDL_Filter_WP").hide();
    }

    return instanceListInterface;
}) ();