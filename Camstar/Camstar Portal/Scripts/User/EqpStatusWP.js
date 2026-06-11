/*
***************************************************************************
Copyright 2022 Siemens
Client side script for the Equipment Status webpart on the UX Demo Landing Page
***************************************************************************
*/

var EqpStatusWP = (function () {
    'use strict';

    var isEqpTileEmpty = true;
    var eqpWPlabels = {};
    var eqpTileList = [];
    var lastSelectedEqpTile = null;

    // interface
    var EqpStatusWPInterface = {
        initialize: initialize,
        eqpTileNotEmpty: eqpTileNotEmpty,
        displayEmptyMessage: displayEmptyMessage,
        refreshEquipmentStatusAjax: refreshEquipmentStatusAjax,
        refreshEquipmentStatusClientCallback: refreshEquipmentStatusClientCallback,
        get_controlId: get_controlId,
        get_serverType: get_serverType,
        isResourceExistInList: isResourceExistInList
    }
    return EqpStatusWPInterface;

    function initialize(labelObj) {

        eqpWPlabels = labelObj;
        lastSelectedEqpTile = null;
        $("#ctl00_WebPartManager_EqpStatusWP_HiddenSelectedEqp_ctl00")[0].value = null;
        $(".close-button").click();
        $("#ctl00_WebPartManager_EqpStatusWP_EquipmentTile").hide();
        if (isEqpTileEmpty) {
            displayEmptyMessage();
            return;
        }


        // update tiles in timeout to give client processing time to add them to page.
        setTimeout(updateTiles, 200);

        function updateTiles() {

            $("#ctl00_WebPartManager_EqpStatusWP_EquipmentTile").show();

            var TEMPLATE = {
                lotCountHTML: '<span class="eqp-lot-count" title="Lot Count">_LOT_COUNT_VALUE_</span>',
                imgEqpUnavailable: '<img src="./assets/image/indicatorStatusStopped16.svg" class="eqp-status-icon" title="Unavailable">',
                imgEqpAvailable: '<img src="./assets/image/indicatorStatusReady16.svg" class="eqp-status-icon" title="Available">',
                imgPMDue: '<img src="./assets/image/indicatorStatusMaintenance16.svg" class="eqp-status-icon" title="PM Required">'
            };

            $("#ctl00_WebPartManager_EqpStatusWP_EquipmentTile .item").each(function () {
                let $tile = $(this);
                let eqpName = $tile.attr('data-custom');
                $tile.click(function () { clickTile(eqpName); });

                // Add Tile Image
                let icon = document.createElement("span");
                icon.className = "eqp-icon-first";

                $(".icon", $tile).remove();

                if ($tile.find(".eqp-icon-first").length == 0)
                    $(".textArea", $tile).before(icon);

                // Add Tooltip
                let $description = $("span:nth-child(2):not([class])", $tile);
                $description.attr("title", $description.text());

                let $status = $("span:nth-child(3):not([class])", $tile);
                $status.attr("title", $status.text());

                let eqpItem = eqpTileList.find(eqp => eqp.EquipmentName === eqpName);
                if (eqpItem) {

                    let iconStatusHTML = "";

                    // Add Equipment unavailable class
                    if (eqpItem.EquipmentAvailability) {
                        $tile.addClass('eqp-available');
                        iconStatusHTML += TEMPLATE.imgEqpAvailable;
                    }
                    else {
                        $tile.addClass('eqp-unavailable');
                        iconStatusHTML += TEMPLATE.imgEqpUnavailable;
                    }

                    // PM Required
                    if (eqpItem.RequiredPM)
                        iconStatusHTML += TEMPLATE.imgPMDue;

                    $(".textArea", $tile).append("<span>" + iconStatusHTML + "</span>");

                    // Add Lot Count Span
                    if (eqpItem.LotCount > 0)
                        $(".titleArea", $tile).append(TEMPLATE.lotCountHTML.replace("_LOT_COUNT_VALUE_", eqpItem.LotCount));
                }

                $tile.css('cursor', 'pointer');

            });

            $('#ctl00_WebPartManager_EqpStatusWP_EquipmentTile .icon-last').hide();
        };
    }

    function displayEmptyMessage() {
        if ($("#WebPart_EqpStatusWP").find(".elp-empty-state").length == 0) {
            var element = $('#WebPart_EqpStatusWP').append("<div class='elp-empty-state' id='lp-emp-state' />");
            $(".elp-empty-state", element).append("<canvas id='elp-empty-state-img' />");
            var emptyStateImg = document.getElementById("elp-empty-state-img"),
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
            $(".elp-empty-state", element).append("<div class='lp-empty-state-txt'>There is no data to display.</>");

            isEqpTileEmpty = true;
            eqpTileList = [];
        }
    }

    function eqpTileNotEmpty(eqpList) {
        isEqpTileEmpty = false;
        eqpTileList = eqpList;
    }

    function isResourceExistInList(resource) {
        return eqpTileList.some(function (e) { return e.EquipmentName === resource })
    }

    function clickTile(eqpName) {

        let tile = $('[data-custom="' + eqpName + '"]');
        let prevEqp = lastSelectedEqpTile ? lastSelectedEqpTile.attr("data-custom") : null;
        if (prevEqp && prevEqp === eqpName)
            return;

        if (lastSelectedEqpTile && lastSelectedEqpTile != tile)
            lastSelectedEqpTile.removeClass("tile-selected");
        if (tile) {
            tile.addClass("tile-selected");
            lastSelectedEqpTile = tile;
            $(".close-button").click();
        }

        $("#ctl00_WebPartManager_EqpStatusWP_HiddenSelectedEqp_ctl00")[0].value = eqpName;
        isOEEEqpHistoryWP.refreshChart(eqpName);
    }

    function refreshEquipmentStatusAjax() {
        var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), EqpStatusWP);
        transition.set_command("RefreshEquipmentStatus");
        var callParamsString = Sys.Serialization.JavaScriptSerializer.serialize(eqpWPlabels);
        transition.set_commandParameters(callParamsString);
        transition.set_clientCallback("refreshEquipmentStatusClientCallback");
        var communicator = new Camstar.Ajax.Communicator(transition, EqpStatusWP);
        communicator.syncCall();
        communicator.dispose();
    }

    function refreshEquipmentStatusClientCallback(data) {
        if (data.Data && data.Data.HTML) {
            let state = JSON.parse(data.Data.HTML);
            var tileContainer = jQuery("#ctl00_WebPartManager_EqpStatusWP_EquipmentTile");
            tileContainer[0].control.setValue(state);
            isEqpTileEmpty = false;
            eqpTileList = JSON.parse(state.CustomData);
            $("#ctl00_WebPartManager_EqpStatusWP_LblShowingEntries")[0].innerHTML = "(" + state.Tiles.length + " of " + eqpTileList + ")";
            $("#WebPart_EqpStatusWP").find(".elp-empty-state").remove();
            initialize(eqpWPlabels);
        } else {
            isEqpTileEmpty = true;
            $("#ctl00_WebPartManager_EqpStatusWP_LblShowingEntries")[0].innerHTML = "";
            initialize(eqpWPlabels);
        }
    }

    function get_serverType() {
        return "Camstar.WebPortal.WebPortlets.Shopfloor.EqpStatusWP, App_Code";
    }

    function get_controlId() {
        return "EqpStatusWP";
    }
})();