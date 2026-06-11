/*
***************************************************************************
Copyright 2022 Siemens
Client side script for the Dispatch List webpart on the UX Demo Landing Page
***************************************************************************
*/

var DispatchListWP = (function () {
    'use strict';

    var isDispatchTileEmpty = true;
    var labels = {};
    var lastSelectedTile = null;
    var state = null;
    var totalRecord = null;
    const NO_OF_RECORD = 9;

    // interface
    var DispatchListWPInterface = {
        initialize: initialize,
        hideEmptyMessage: hideEmptyMessage,
        displayEmptyMessage: displayEmptyMessage,
        refreshDispatchListAjax: refreshDispatchListAjax,
        refreshDispatchListClientCallback: refreshDispatchListClientCallback,
        get_controlId: get_controlId,
        get_serverType: get_serverType,
        isLotExistInList: isLotExistInList
    }
    return DispatchListWPInterface;

    function initialize(labelObj, isAjax) {

        labels = labelObj;
        lastSelectedTile = null;

        $('#ctl00_WebPartManager_DispatchListWP_HiddenSelectedContainer_ctl00').val("");

        $(".close-button").attr("title", "Close");

        $("#WebPart_DispatchListWP").find(".page-num").remove();

        //temporarily hide the list
        $("#ctl00_WebPartManager_DispatchListWP_DispatchListTile").hide();

        //register the listener for the refresh
        var div = document.getElementById('WebPart_DispatchListWP_UIComponent');
        div.addEventListener('refresh', function (e) {
            refreshDispatchListAjax();
        });

        if (isDispatchTileEmpty) {
            displayEmptyMessage();
            return;
        }

        if (totalRecord) {
            var visibleRecord = NO_OF_RECORD;
            if (totalRecord < NO_OF_RECORD) {
                visibleRecord = totalRecord;
            }
            $("#ctl00_WebPartManager_DispatchListWP_DispatchListTitle").after("<span class='cs-label page-num'>(" + visibleRecord + " of " + totalRecord + ")</span>");
        }

        isDispatchTileEmpty = true;

        // update tiles in timeout to give client processing time to add them to page.       
        setTimeout(updateTiles, 200);

        function updateTiles() {

            $("#ctl00_WebPartManager_DispatchListWP_DispatchListTile").show();

            var TEMPLATE = {
                contentRow: "<span title='Qty: _TT1 Qty2: _TT2'>_QUANTITY_LABEL: _QTY  <img src='assets/image/indicatorContainsInnerMismatches16.svg' style='width:16px;' \
                title='_QTY_LABEL'/></span>"
            };

            $("#ctl00_WebPartManager_DispatchListWP_DispatchListTile .item").each(function () {
                var $tile = $(this);
                if ($tile.attr('data-ind') > NO_OF_RECORD - 1) {
                    $tile.hide();
                    return;
                }

                var tileId = $tile.attr('data-custom');
                $tile.attr("title", "Click to select");
                $tile.click(function () { clickTile(tileId); });

                var icon = document.createElement("span");
                icon.className = "icon-first";

                $(".icon", $tile).remove();

                if ($tile.find(".icon-first").length == 0)
                    $(".textArea", $tile).before(icon);

                var $qty = $("span:nth-child(2):not([class])", $tile);
                if (isAjax || ($qty.is(":visible") && $qty.find("img").length == 0)) {
                    $qty.after(
                        TEMPLATE.contentRow
                            .replace(/_QUANTITY_LABEL/, labels.Quantity)
                            .replace(/_QTY/, $qty.text())
                            .replace(/_QTY_LABEL/, labels.Qty)
                            .replace(/_TT1/, $qty.text()));
                    $qty.remove();
                }

                var $product = $("span:nth-child(3):not([class])", $tile);
                $product.attr("title", $product.text());
                var $step = $("span:nth-child(4):not([class])", $tile);
                $step.attr("title", $step.text());

                var $state = $("span:nth-child(5):not([class])", $tile);
                var $status = $("span:nth-child(6):not([class])", $tile);
                var $isOnHold = $("span:nth-child(7):not([class])", $tile);
                if (isAjax || $state.is(":visible")) {

                    var $onHold = "";

                    if ($isOnHold.text() == "true") {
                        $tile.attr("style", "border-left: 4px solid red !important;");
                        $onHold = "<span class='on-hold' title='Lot is on hold.'></span>";
                    } else {
                        $tile.attr("style", "border-left: 0 !important;");
                    }
                    if ($state.text() == "In Queue")
                        $state.after("<div class='titleArea'><span class='move-in' title='" + $state.text() + "'>\
                                     </span><span class='tag' title='Status: " + $status.text() + "'>" + $status.text() + "</span>" + $onHold + "</div>");                   
                    else if ($state.text() == "In Process")
                        $state.after("<div class='titleArea'><span class='move-out' title='" + $state.text() + "'>\
                                     </span><span class='tag' title='Status: " + $status.text() + "'>" + $status.text() + "</span>" + $onHold + "</div>");
                    $state.remove();
                    $status.remove();
                    $isOnHold.remove();
                }

                var $icon = $('.icon-last', $tile);
                if ($icon.find(".cs-button-image").length == 0) {
                    $icon.append("<input type='submit' title='Operation View' class='cs-button-image' style='width: 28px;background-repeat:no-repeat;background-position:center;background-image:url(./assets/image/cmdWorkInProgress24.svg);border-color:#D4D4D4;margin-top: 15px !important;' value=''>");
                    $(".cs-button-image", $icon).click(function () {
                        $(".close-button").click();
                        $('#ctl00_WebPartManager_DispatchListWP_HiddenSelectedContainer_ctl00').val($icon.parent().attr('data-custom'));
                        $('#ctl00_WebPartManager_ButtonsBar_WIPButton').click();
                    });
                }
            });

        };
    }

    function displayEmptyMessage() {
        totalRecord = null;
        if ($("#WebPart_DispatchListWP").find(".lp-empty-state").length == 0) {
            var element = $('#WebPart_DispatchListWP').append("<div class='lp-empty-state' id='lp-emp-state' />");
            $(".lp-empty-state", element).append("<canvas id='lp-empty-state-img' />");
            var emptyStateImg = document.getElementById("lp-empty-state-img"),
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
            $(".lp-empty-state", element).append("<div class='lp-empty-state-txt'>There is no data to display.</>");
            isDispatchTileEmpty = true;
        }
    }

    function hideEmptyMessage(totalRecordArg) {
        totalRecord = totalRecordArg;
        isDispatchTileEmpty = false;
    }

    // handler for clicking a tile
    function clickTile(id) {
        let tile = $('[data-custom="' + id + '"]');
        let prevTileId = lastSelectedTile ? lastSelectedTile.attr("data-custom") : null;
        if (prevTileId && prevTileId === id)
            return;

        if (lastSelectedTile && lastSelectedTile != tile)
            lastSelectedTile.removeClass("tile-selected");
        if (tile) {
            $('#ctl00_WebPartManager_DispatchListWP_HiddenSelectedContainer_ctl00').val(id);
            tile.addClass("tile-selected");
            lastSelectedTile = tile;
            $(".close-button").click();
        }
    }

    function isLotExistInList(lotName) {
        return $("#ctl00_WebPartManager_DispatchListWP_DispatchListTile").find("[data-custom='" + lotName + "']").length > 0;
    }

    function refreshDispatchListAjax() {
        var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), DispatchListWP);
        transition.set_command("RefreshDispatchList");
        var callParamsString = Sys.Serialization.JavaScriptSerializer.serialize(labels);
        transition.set_commandParameters(callParamsString);
        transition.set_clientCallback("refreshDispatchListClientCallback");
        var communicator = new Camstar.Ajax.Communicator(transition, DispatchListWP);
        communicator.syncCall();
        communicator.dispose();
    }

    function refreshDispatchListClientCallback(data) {
        if (data.Data && data.Data.HTML) {
            let state = JSON.parse(data.Data.HTML);
            var tileContainer = jQuery("#ctl00_WebPartManager_DispatchListWP_DispatchListTile");
            tileContainer[0].control.setValue(state);
            hideEmptyMessage(state.CustomData);
            $("#WebPart_DispatchListWP").find(".lp-empty-state").remove();
            initialize(labels, true);
        } else {
            isDispatchTileEmpty = true;
            initialize(labels);
        }
    }

    function get_serverType() {
        return "Camstar.WebPortal.WebPortlets.Shopfloor.DispatchListWP, App_Code";
    }

    function get_controlId() {
        return "DispatchListWP";
    }

})();