/*
***************************************************************************
Copyright 2022 Siemens
Client side script for the Dispatch List webpart on the UX Demo Landing Page
***************************************************************************
*/

var scsDispatchListWP = (function () {
    'use strict';

    var isDispatchTileEmpty = true;
    var labels = {};
    var lastSelectedTile = null;
    var visibleRecord = 0;
    var totalRecord = 0;
    const NO_OF_RECORD = 9;

    var filterIconParam = null;
    var filterTextParam = null;
    var lastSelectedIcon = null;
    var lastSelectedIconId = null;
    var lastFilterText = null;
    var clearReload = false;

    // interface
    var scsDispatchListWPInterface = {
        initialize: initialize,
        hideEmptyMessage: hideEmptyMessage,
        hideEmptyMessageWithLoad: hideEmptyMessageWithLoad,
        displayEmptyMessage: displayEmptyMessage,
        displayEmptyMessageWithLoad: displayEmptyMessageWithLoad,
        refreshDispatchListAjax: refreshDispatchListAjax,
        refreshDispatchListClientCallback: refreshDispatchListClientCallback,
        get_controlId: get_controlId,
        get_serverType: get_serverType,
        isLotExistInList: isLotExistInList,
        clearAll: clearAll
    }
    return scsDispatchListWPInterface;

    function initialize(labelObj, isAjax) {

        labels = labelObj;
        lastSelectedTile = null;

        $('#ctl00_WebPartManager_scsDispatchListWP_HiddenSelectedContainer_ctl00').val("");

        $(".close-button").attr("title", "Close");

        $("#WebPart_scsDispatchListWP").find(".page-num").remove();
        $("#WebPart_scsDispatchListWP").find("#WebPart_scsDispatchListWP_FilterLot").remove();
        $("#WebPart_scsDispatchListWP").find(".dispatchlist-filter").remove();

        //temporarily hide the list
        $("#ctl00_WebPartManager_scsDispatchListWP_DispatchListTile").hide();

        //register the listener for the refresh
        var div = document.getElementById('WebPart_scsDispatchListWP_UIComponent');

        div.addEventListener('refresh', function (e) {
            refreshDispatchListAjax();
        });

        visibleRecord = NO_OF_RECORD;
        if (totalRecord < NO_OF_RECORD) {
            visibleRecord = totalRecord;
        }

        //temporary
        visibleRecord = 0;

        var filterIconHTML = "";
        var filterTextHTML = "";
        var filterState = [0, 1, 2, 3];
        var filterClass = ["move-in", "track-in", "track-out", "move-out"]
        var filterValue = ["Move In", "Track In", "Track Out", "Move Out"]

        for (var i = 0; i < filterState.length; i++) {
            filterIconHTML += "<input type='button' id='WebPartManager_scsDispatchListWP_FilterBtn_" + filterClass[i] + "' value='" + filterState[i] + "' class='btnLotFilter " + filterClass[i] + "' title='Pending for " + filterValue[i] + "'></input>";
        }

        filterTextHTML = "<div class='dispatchlist-filter'><div class='cs-textbox'><input type='text' autocomplete='off' id='WebPart_scsDispatchListWP_FilterLot' placeholder='Lot Filter'/></div> \
            <input type='button' id='IconLotClear' class='txtfilter cs-clear' title='Clear'></input>";
        filterIconHTML = "<div id='filterIconLbl' class='cs-label'>Select to filter: </div>" + filterIconHTML + "</div>";

        $("#ctl00_WebPartManager_scsDispatchListWP_DispatchListTitle").after(filterTextHTML + filterIconHTML);
        $("#ctl00_WebPartManager_scsDispatchListWP_DispatchListTitle").after("<div class='cs-label page-num'>(" + visibleRecord + " of " + totalRecord + ")</div>");

        filterIconHTML = "";
        filterTextHTML = "";


        if (lastFilterText) {
            $(document).ready(function () { $("#WebPart_scsDispatchListWP_FilterLot").val(lastFilterText); })
            $(document).ready(function () { $("#WebPart_scsDispatchListWP_FilterLot").focus(); })
        }

        if (lastSelectedIcon && lastSelectedIconId) {
            $(document).ready(function () { $(lastSelectedIconId).toggleClass("btnLotFilter-selected"); })
        }

        // Lot Search filter click function
        // OnClick ENTER or TAB will submit the input
        var fliterTextBox = document.getElementById("WebPart_scsDispatchListWP_FilterLot");
        fliterTextBox.addEventListener('keydown', function (event) {
            if ((event.keyCode === 13 || event.keyCode === 9) && event.target.nodeName === 'INPUT') {
                var textID = $("#WebPart_scsDispatchListWP_FilterLot");
                filterTextParam = textID.val().toString();
                lastSelectedIcon ? lastSelectedIcon : null;
                lastSelectedIconId ? lastSelectedIconId : null;
                refreshDispatchListAjax();
            }
        });

        //Icon search click function.
        $(".btnLotFilter").click(function (event) {
            var iconId = this.id;
            iconId = "#" + iconId;
            filterIconParam = $(iconId).val().toString();
            lastFilterText ? lastFilterText : null
            refreshDispatchListAjax();
            if (lastSelectedIconId && !lastSelectedIcon) {
                lastSelectedIconId = null;
            }
            else
                lastSelectedIconId = iconId;

        });

        // Clear lot search text field
        $("#IconLotClear").click(function (event) {
            var filterValue = $("#WebPart_scsDispatchListWP_FilterLot").val('');
            filterTextParam = null;
            lastSelectedIcon ? lastSelectedIcon : null;
            lastSelectedIconId ? lastSelectedIconId : null;
            if (lastFilterText || lastSelectedIcon) {
                clearReload = true;
                refreshDispatchListAjax();
            }
            $("#WebPart_scsDispatchListWP_FilterLot").focus();
            filterValue.focus();
            event.preventDefault();
        });

        if (isDispatchTileEmpty) {
            displayEmptyMessage();
            return;
        }

        isDispatchTileEmpty = true;

        // update tiles in timeout to give client processing time to add them to page.       
        setTimeout(updateTiles, 200);

        function updateTiles() {

            $("#ctl00_WebPartManager_scsDispatchListWP_DispatchListTile").show();

            var TEMPLATE = {
                contentRow: "<span title='Qty: _TT1 Qty2: _TT2'>_QUANTITY_LABEL: _QTY  <img src='assets/image/indicatorContainsInnerMismatches16.svg' style='width:16px;' \
                title='_QTY_LABEL'/>  _QTY2  <img src='assets/image/indicatorPartiallyAssignedByDescendants16.svg' style='width:16px;' title='_QTY2_LABEL'/></span>"
            };

            $("#ctl00_WebPartManager_scsDispatchListWP_DispatchListTile .item").each(function () {
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
                var $qty2 = $("span:nth-child(3):not([class])", $tile);
                if (isAjax || ($qty.is(":visible") && $qty.find("img").length == 0)) {
                    $qty.after(
                        TEMPLATE.contentRow
                            .replace(/_QUANTITY_LABEL/, labels.Quantity)
                            .replace(/_QTY/, $qty.text())
                            .replace(/_QTY_LABEL/, labels.Qty)
                            .replace(/_QTY2/, $qty2.text())
                            .replace(/_QTY2_LABEL/, labels.Qty2)
                            .replace(/_TT1/, $qty.text())
                            .replace(/_TT2/, $qty2.text()));
                    $qty.remove();
                    $qty2.remove();
                }

                var $product = $("span:nth-child(3):not([class])", $tile);
                $product.attr("title", $product.text());
                var $step = $("span:nth-child(4):not([class])", $tile);
                $step.attr("title", $step.text());

                var $state = $("span:nth-child(5):not([class])", $tile);
                var $priority = $("span:nth-child(6):not([class])", $tile);
                var $status = $("span:nth-child(7):not([class])", $tile);

                if (isAjax || $state.is(":visible")) {

                    var $onHold = "";

                    if ($status.text() == "DARKRED") {
                        $tile.attr("style", "border-left: 4px solid red !important;");
                        $onHold = "<span class='on-hold' title='Lot is on hold.'></span>";
                    } else {
                        $tile.attr("style", "border-left: 0 !important;");
                    }
                    if ($state.text() == "Move In")
                        $state.after("<div class='titleArea'><span class='move-in' title='Pending for " + $state.text() + "'>\
                                     </span><span class='tag' title='Priority Code: " + $priority.text() + "'>" + $priority.text() + "</span>" + $onHold + "</div>");
                    else if ($state.text() == "Track In")
                        $state.after("<div class='titleArea'><span class='track-in' title='Pending for " + $state.text() + "'>\
                                     </span><span class='tag' title='Priority Code: " + $priority.text() + "'>" + $priority.text() + "</span>" + $onHold + "</div>");
                    else if ($state.text() == "Track Out")
                        $state.after("<div class='titleArea'><span class='track-out' title='Pending for " + $state.text() + "'>\
                                     </span><span class='tag' title='Priority Code: " + $priority.text() + "'>" + $priority.text() + "</span>" + $onHold + "</div>");
                    else if ($state.text() == "Move Out")
                        $state.after("<div class='titleArea'><span class='move-out' title='Pending for " + $state.text() + "'>\
                                     </span><span class='tag' title='Priority Code: " + $priority.text() + "'>" + $priority.text() + "</span>" + $onHold + "</div>");
                    $state.remove();
                    $priority.remove();
                    $status.remove();
                }

                var $icon = $('.icon-last', $tile);
                if ($icon.find(".cs-button-image").length == 0) {
                    $icon.append("<input type='button' title='" + labels.SimpleWIPMain + "' class='cs-button-image' style='width: 28px;background-repeat:no-repeat;background-position:center;background-image:url(./assets/image/cmdWorkInProgress24.svg);border-color:#D4D4D4;margin-top: 15px !important;' value=''>");
                    $(".cs-button-image", $icon).click(function () {
                        $(".close-button").click();
                        $('#ctl00_WebPartManager_scsDispatchListWP_HiddenSelectedContainer_ctl00').val($icon.parent().attr('data-custom'));
                        $('#ctl00_WebPartManager_ButtonsBar_WIPButton').click();
                    });
                }
            });

        };
    }

    function displayEmptyMessage() {
        totalRecord = 0;
        if ($("#WebPart_scsDispatchListWP").find(".lp-empty-state").length == 0) {
            var element = $('#WebPart_scsDispatchListWP').append("<div class='lp-empty-state' id='lp-emp-state' />");
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

    function displayEmptyMessageWithLoad(resetData) {
        if (resetData)
            clearAll();
        displayEmptyMessage();
    }

    function hideEmptyMessage(totalRecordArg) {
        totalRecord = totalRecordArg;
        isDispatchTileEmpty = false;
    }

    function hideEmptyMessageWithLoad(totalRecordArg, resetData) {
        if (resetData)
            clearAll();
        hideEmptyMessage(totalRecordArg);
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
            $('#ctl00_WebPartManager_scsDispatchListWP_HiddenSelectedContainer_ctl00').val(id);
            tile.addClass("tile-selected");
            lastSelectedTile = tile;
            visibleRecord = 1;
            $("#WebPart_scsDispatchListWP").find(".page-num").empty();
            $("#WebPart_scsDispatchListWP").find(".page-num").append("(" + visibleRecord + " of " + totalRecord + ")");
            $(".close-button").click();
        }
        $("#LotDetailsAction").removeClass("cmdbar-aspNetDisabled");
    }

    function isLotExistInList(lotName) {
        return $("#ctl00_WebPartManager_scsDispatchListWP_DispatchListTile").find("[data-custom='" + lotName + "']").length > 0;
    }

    function refreshDispatchListAjax() {
        var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), scsDispatchListWP);
        var textParam = filterTextParam;
        var iconParam = filterIconParam;
        var prevIcon = lastSelectedIcon;
        var prevText = lastFilterText;

        if ((prevText && prevText == textParam) && !clearReload) {
            if (iconParam && iconParam != prevIcon) {
                iconParam = filterIconParam ? filterIconParam : prevIcon ? prevIcon : null;
                textParam = prevText;
            }
        }

        if ((prevIcon && prevIcon == iconParam) && !clearReload) {
            if (textParam && textParam != prevText) {
                textParam = filterTextParam ? filterTextParam : prevText ? prevText : null;
            }
            else
                iconParam = null;
        }

        if (clearReload == true) {
            iconParam = filterIconParam ? filterIconParam : null;
            textParam = null;
            clearReload = false;
        }

        transition.set_command("RefreshDispatchList");
        var callParameters =
        {
            "Labels": Sys.Serialization.JavaScriptSerializer.serialize(labels),
            "TextSearch": textParam ? textParam : "",
            "State": iconParam ? iconParam : ""

        };
        var callParamsString = Sys.Serialization.JavaScriptSerializer.serialize(callParameters);
        transition.set_commandParameters(callParamsString);
        transition.set_clientCallback("refreshDispatchListClientCallback");
        var communicator = new Camstar.Ajax.Communicator(transition, scsDispatchListWP);
        communicator.syncCall();
        communicator.dispose();

        if (textParam) {
            lastFilterText = textParam;
        }
        else
            lastFilterText = null;

        if (iconParam) {
            lastSelectedIcon = iconParam;
        }
        else {
            lastSelectedIcon = null;
            lastSelectedIconId = null;
            filterIconParam = null;
        }
    }

    function refreshDispatchListClientCallback(data) {
        if (data.Data && data.Data.HTML) {
            let state = JSON.parse(data.Data.HTML);
            var tileContainer = jQuery("#ctl00_WebPartManager_scsDispatchListWP_DispatchListTile");
            tileContainer[0].control.setValue(state);
            hideEmptyMessage(state.CustomData);
            $("#WebPart_scsDispatchListWP").find(".lp-empty-state").remove();
            initialize(labels, true);
        }
        else {
            totalRecord = 0;
            isDispatchTileEmpty = true;
            initialize(labels);
        }
    }

    function get_serverType() {
        return "Camstar.WebPortal.WebPortlets.Shopfloor.scsDispatchListWP, App_Code";
    }

    function get_controlId() {
        return "scsDispatchListWP";
    }

    function clearAll() {
        lastSelectedTile = null;
        visibleRecord = 0;
        totalRecord = 0;

        filterIconParam = null;
        filterTextParam = null;
        lastSelectedIcon = null;
        lastSelectedIconId = null;
        lastFilterText = null;
        clearReload = false;
    }

})();