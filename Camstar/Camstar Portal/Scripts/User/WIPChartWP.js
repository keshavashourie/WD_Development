/*
***************************************************************************
Copyright 2022 Siemens
Client side script for the WIP Chart webpart on the UX Demo Landing Page
***************************************************************************
*/

var WIPChartWP = (function () {
    'use strict';

    var isWIPChartEmpty = true;
    var isRejectChartEmpty = true;

    // interface
    var WIPChartWPInterface = {
        initialize: initialize,
        wipChartNotEmpty: wipChartNotEmpty,
        rejectChartNotEmpty: rejectChartNotEmpty,
        wipEmptyMessage: wipEmptyMessage,
        rejectEmptyMessage: rejectEmptyMessage,
        refreshRejectChart: refreshRejectChart,
        refreshWIPChart: refreshWIPChart,
        refreshBarChartAjax: refreshBarChartAjax,
        refreshRejectChartClientCallback: refreshRejectChartClientCallback,
        refreshWIPChartClientCallback: refreshWIPChartClientCallback,
        get_controlId: get_controlId,
        get_serverType: get_serverType
    }
    return WIPChartWPInterface;

    function initialize() {
        if (isWIPChartEmpty) {
            wipEmptyMessage();
        }
        if (isRejectChartEmpty) {
            rejectEmptyMessage();
        }


        //register the listener for the refresh wipChart
        var div = document.getElementById('WebPart_WIPChartWP_UIComponent');
        div.addEventListener('refreshLandingPage', function (e) {
            $("#ctl00_WebPartManager_ButtonsBar_RefreshButton").click();
        });


        let $landingPage = getTabsByVPName("LandingPage_VP");
        $landingPage[0].on("click", function () {
            removeRejectChart();
            removeWIPChart();

            updateChart("ctl00_WebPartManager_WIPChartWP_RejectChart", null);
            updateChart("ctl00_WebPartManager_WIPChartWP_WipChart", null);
        });
    }

    function wipChartNotEmpty() {
        isWIPChartEmpty = false;
    }

    function rejectChartNotEmpty() {
        isRejectChartEmpty = false;
    }

    function wipEmptyMessage() {
        if ($("#ctl00_WebPartManager_WIPChartWP_WipChart").find(".wlp-empty-state").length == 0) {
            var element = $('#ctl00_WebPartManager_WIPChartWP_WipChart').append("<div class='wlp-empty-state' id='lp-emp-state' />");
            $(".wlp-empty-state", element).append("<canvas id='wlp-empty-state-img' />");
            var emptyStateImg = document.getElementById("wlp-empty-state-img"),
                ctx = emptyStateImg.getContext('2d'),
                image = new Image(192, 192);
            image.src = 'Themes/Horizon/images/icons/typeComputer48.svg';
            image.onload = function () {
                ctx.drawImage(image,
                    0,
                    0,
                    emptyStateImg.width,
                    emptyStateImg.height);
            }
            $(".wlp-empty-state", element).append("<div class='lp-empty-state-txt'>There is no data to display.</>");
            isWIPChartEmpty = true;
        }
        $("#ctl00_WebPartManager_WIPChartWP_WipChart_ChartContainer").hide();
    }

    function rejectEmptyMessage() {
        if ($("#ctl00_WebPartManager_WIPChartWP_RejectChart").find(".rlp-empty-state").length == 0) {
            var element = $('#ctl00_WebPartManager_WIPChartWP_RejectChart').append("<div class='rlp-empty-state' id='lp-emp-state' />");
            $(".rlp-empty-state", element).append("<canvas id='rlp-empty-state-img' />");
            var emptyStateImg = document.getElementById("rlp-empty-state-img"),
                ctx = emptyStateImg.getContext('2d'),
                image = new Image(192, 192);
            image.src = 'Themes/Horizon/images/icons/typeComputer48.svg';
            image.onload = function () {
                ctx.drawImage(image,
                    0,
                    0,
                    emptyStateImg.width,
                    emptyStateImg.height);
            }
            $(".rlp-empty-state", element).append("<div class='lp-empty-state-txt'>There is no data to display.</>");
            isRejectChartEmpty = true;
        }
        $("#ctl00_WebPartManager_WIPChartWP_RejectChart_ChartContainer").hide();
    }

    function refreshRejectChart() {
        refreshBarChartAjax("Reject", "refreshRejectChartClientCallback");
    }

    function refreshWIPChart() {
        refreshBarChartAjax("WIP", "refreshWIPChartClientCallback");
    }

    function refreshBarChartAjax(type, clientCallback) {
        var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), WIPChartWP);
        transition.set_command("RefreshBarChart");
        var callParamsString = Sys.Serialization.JavaScriptSerializer.serialize({
            "Type": type
        });
        transition.set_commandParameters(callParamsString);
        transition.set_clientCallback(clientCallback);
        var communicator = new Camstar.Ajax.Communicator(transition, WIPChartWP);
        communicator.syncCall();
        communicator.dispose();
    }

    function refreshRejectChartClientCallback(data) {
        if (data.Data && data.Data.HTML) {
            rejectChartNotEmpty();
            removeRejectChart();
            
            updateChart("ctl00_WebPartManager_WIPChartWP_RejectChart", data.Data.HTML);
        } else {
            rejectEmptyMessage();
        }
    }

    function refreshWIPChartClientCallback(data) {
        if (data.Data && data.Data.HTML) {
            wipChartNotEmpty();
            removeWIPChart();
            updateChart("ctl00_WebPartManager_WIPChartWP_WipChart", data.Data.HTML);
        } else {        
            wipEmptyMessage();
        }
    }

    function updateChart(chatId, data) {
        var chart = jQuery("#" + chatId);
        if (data != null) {
            chart[0].control.set_DataSet(data);
        }
        chart[0].control.RefreshChart('Multibar');
    }

    function removeRejectChart() {
        $("#ctl00_WebPartManager_WIPChartWP_RejectChart_ChartContainer svg:first").remove();
    }

    function removeWIPChart() {
        $("#ctl00_WebPartManager_WIPChartWP_WipChart_ChartContainer svg:first").remove();
    }

    function get_serverType() {
        return "Camstar.WebPortal.WebPortlets.Shopfloor.WIPChartWP, App_Code";
    }

    function get_controlId() {
        return "WIPChartWP";
    }
})();