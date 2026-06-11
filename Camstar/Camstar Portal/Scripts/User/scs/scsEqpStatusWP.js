/*
***************************************************************************
Copyright 2022 Siemens
Client side script for the Equipment Status webpart on the UX Demo Landing Page
***************************************************************************
*/

var scsEqpStatusWP = (function () {
    'use strict';

    var isEqpTileEmpty = true;
    var eqpWPlabels = {};
    var eqpTileList = [];
    var lastSelectedEqpTile = null;
    var visibleRecord = 0;
    var totalRecord = 0;

    var filterIconParam = null;
    var filterTextParam = null;
    var lastSelectedIcon = null;
    var lastSelectedIconId = null;
    var lastFilterText = null;
    var clearReload = false;

    // interface
    var scsEqpStatusWPInterface = {
        initialize: initialize,
        eqpTileNotEmpty: eqpTileNotEmpty,
        eqpTileNotEmptyWithLoad: eqpTileNotEmptyWithLoad,
        displayEmptyMessage: displayEmptyMessage,
        displayEmptyMessageWithLoad: displayEmptyMessageWithLoad,
        refreshEquipmentStatusAjax: refreshEquipmentStatusAjax,
        refreshEquipmentStatusClientCallback: refreshEquipmentStatusClientCallback,
        get_controlId: get_controlId,
        get_serverType: get_serverType,
        isResourceExistInList: isResourceExistInList,
        clearAll: clearAll
    }

    return scsEqpStatusWPInterface;

    function initialize(labelObj) {

        eqpWPlabels = labelObj;
        lastSelectedEqpTile = null;
        $("#ctl00_WebPartManager_scsEqpStatusWP_HiddenSelectedEqp_ctl00")[0].value = null;
        $(".close-button").click();


        $("#WebPart_scsEqpStatusWP").find("#WebPart_scsEquipmentWP_FilterEqp").remove();
        $("#WebPart_scsEqpStatusWP").find(".eqipmentstatus-filter").remove();


        //temporarily hide the list
        $("#ctl00_WebPartManager_scsEqpStatusWP_EquipmentTile").hide();

        //register the listener for the refresh
        var div = document.getElementById('WebPart_scsEqpStatusWP_UIComponent');
        div.addEventListener('refresh', function (e) {
            refreshEquipmentStatusAjax();
        });


        /*Equipment Search Filter Start.*/
        //Binding the Filter TextBox and Icons.

        var filterIconHTML = "";
        var filterTextHTML = "";
        var filterState = [0, 1];
        var filterClass = ["unavailable", "available"]
        var filterValue = ["Unavailable", "Available"]

        for (var i = 0; i < filterState.length; i++) {
            filterIconHTML += "<input type='button' id='WebPartManager_scsEqpStatusWP_FilterBtn_" + filterClass[i] + "' value='" + filterState[i] + "' class='btnEqpFilter " + filterClass[i] + "' title='" + filterValue[i] + "'></input>";
        }

        filterTextHTML = "<div class='eqipmentstatus-filter'><div class='cs-textbox'><input type='text' autocomplete='off' id='WebPart_scsEquipmentWP_FilterEqp' placeholder='Equipment Filter'/></div> \
            <input type='button' id='IconCleareqp' class='txtfilter cs-clear' title='Clear'></input>";
        filterIconHTML = "<div id='filtericonlbleqp' class='cs-label'>Select to filter: </div>" + filterIconHTML + "</div>";

        $("#ctl00_WebPartManager_scsEqpStatusWP_LblShowingEntries").after(filterTextHTML + filterIconHTML);
        filterIconHTML = "";
        filterTextHTML = "";




        // Clear Equipment search text field

        if (lastFilterText) {
            $(document).ready(function () { $("#WebPart_scsEquipmentWP_FilterEqp").val(lastFilterText); })
            $(document).ready(function () { $("#WebPart_scsEquipmentWP_FilterEqp").focus(); })
        }

        if (lastSelectedIcon && lastSelectedIconId) {
            $(document).ready(function () { $(lastSelectedIconId).toggleClass("btnEqpFilter-selected"); })
        }

        // Equipment Search filter click function
        // OnClick ENTER or TAB will submit the input
        var fliterTextBox = document.getElementById("WebPart_scsEquipmentWP_FilterEqp");
        fliterTextBox.addEventListener('keydown', function (event) {
            if ((event.keyCode === 13 || event.keyCode === 9) && event.target.nodeName === 'INPUT') {
                var textID = $("#WebPart_scsEquipmentWP_FilterEqp");
                filterTextParam = textID.val().toString();
                lastSelectedIcon ? lastSelectedIcon : null;
                lastSelectedIconId ? lastSelectedIconId : null;
                refreshEquipmentStatusAjax();
            }
        });

        //Icon search click function.
        $(".btnEqpFilter").click(function () {
            var iconId = this.id;
            iconId = "#" + iconId;
            filterIconParam = $(iconId).val().toString();
            lastFilterText ? lastFilterText : null
            refreshEquipmentStatusAjax();
            //filterIconParam = null;
            if (lastSelectedIconId && !lastSelectedIcon) {
                lastSelectedIconId = null;
            }
            else {
                lastSelectedIconId = iconId;
            }
        });



        // Clear Equipment  search text field
        $("#IconCleareqp").click(function (event) {
            var filterValue = $("#WebPart_scsEquipmentWP_FilterEqp").val('');
            filterTextParam = null;
            lastSelectedIcon ? lastSelectedIcon : null;
            lastSelectedIconId ? lastSelectedIconId : null;
            if (lastFilterText || lastSelectedIcon) {
                clearReload = true;
                refreshEquipmentStatusAjax();
            }
            $("#WebPart_scsEquipmentWP_FilterEqp").focus();
            filterValue.focus();
            event.preventDefault();
        });

        if (isEqpTileEmpty) {
            displayEmptyMessage();
            return;
        }

        isEqpTileEmpty = true;
        /*Equipment Search Filter End.*/
        // update tiles in timeout to give client processing time to add them to page.
        setTimeout(updateTiles, 200);

        function updateTiles() {

            $("#ctl00_WebPartManager_scsEqpStatusWP_EquipmentTile").show();

            var TEMPLATE = {
                lotCountHTML: '<span class="eqp-lot-count" id="_LOT_COUNT_ID" title="Lot Count">_LOT_COUNT_VALUE_</span>',
                imgEqpUnavailable: '<img src="./assets/image/indicatorStatusStopped16.svg" class="eqp-status-icon" title="Unavailable">',
                imgEqpAvailable: '<img src="./assets/image/indicatorStatusReady16.svg" class="eqp-status-icon" title="Available">',
                imgPMDue: '<img src="./assets/image/indicatorStatusMaintenance16.svg" id="_LOT_PM_DUE" class="eqp-status-icon" title="PM Required">'
            };

            $("#ctl00_WebPartManager_scsEqpStatusWP_EquipmentTile .item").each(function () {
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

                    $(".textArea", $tile).append("<span>" + iconStatusHTML.replace("_LOT_PM_DUE", eqpName + '_EqpStatusWP_Tile_PM') + "</span>");

                    // Add Lot Count Span
                    if (eqpItem.LotCount > 0)
                        $(".titleArea", $tile).append(TEMPLATE.lotCountHTML.replace("_LOT_COUNT_VALUE_", eqpItem.LotCount).replace("_LOT_COUNT_ID", eqpName + '_EqpStatusWP_Tile_LotCount'))


                }

                $tile.css('cursor', 'pointer');

            });

            $('#ctl00_WebPartManager_scsEqpStatusWP_EquipmentTile .icon-last').hide();
        };

    }



    function displayEmptyMessage() {
        if ($("#WebPart_scsEqpStatusWP").find(".elp-empty-state").length == 0) {
            var element = $('#WebPart_scsEqpStatusWP').append("<div class='elp-empty-state' id='lp-emp-state' />");
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

    function displayEmptyMessageWithLoad(resetData) {
        if (resetData)
           clearAll();
        displayEmptyMessage();
    }

    function eqpTileNotEmpty(eqpList) {
        isEqpTileEmpty = false;
        eqpTileList = eqpList;
    }

    function eqpTileNotEmptyWithLoad(eqpList, total, resetData) {
        if (resetData)
            clearAll();
        totalRecord = parseInt(total);
        eqpTileNotEmpty(eqpList);
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
            visibleRecord = 1;
            loadEntries(visibleRecord);
            $(".close-button").click();
        }

        $("#ctl00_WebPartManager_scsEqpStatusWP_HiddenSelectedEqp_ctl00")[0].value = eqpName;
        $("#EqpDetailsAction").addClass("cmdbar-aspNetDisabled");
        $("#PMAction").addClass("cmdbar-aspNetDisabled");
        if (eqpName != "" && $("#" + eqpName + "_EqpStatusWP_Tile_LotCount").length) {
            $("#EqpDetailsAction").removeClass("cmdbar-aspNetDisabled");
        }
        if (eqpName != "" && $("#" + eqpName + "_EqpStatusWP_Tile_PM").length) {
            $("#PMAction").removeClass("cmdbar-aspNetDisabled");
        }

        isOEEEqpHistoryWP.refreshChart(eqpName);
    }

    function refreshEquipmentStatusAjax() {

        var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), scsEqpStatusWP);
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
        transition.set_command("RefreshEquipmentStatus");
        var callParameters =
        {
            "Labels": Sys.Serialization.JavaScriptSerializer.serialize(eqpWPlabels),
            "TextSearch": textParam ? textParam : "",
            "IsAvailable": iconParam ? iconParam : ""

        };
        var callParamsString = Sys.Serialization.JavaScriptSerializer.serialize(callParameters);
        transition.set_commandParameters(callParamsString);
        transition.set_clientCallback("refreshEquipmentStatusClientCallback");
        var communicator = new Camstar.Ajax.Communicator(transition, scsEqpStatusWP);
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

    function refreshEquipmentStatusClientCallback(data) {
        if (data.Data && data.Data.HTML) {
            let state = JSON.parse(data.Data.HTML);
            var tileContainer = jQuery("#ctl00_WebPartManager_scsEqpStatusWP_EquipmentTile");
            tileContainer[0].control.setValue(state);
            isEqpTileEmpty = false;
            eqpTileList = JSON.parse(state.CustomData.split('|')[1]);
            totalRecord = state.CustomData.split('|')[0];
            loadEntries(0);
            $("#WebPart_scsEqpStatusWP").find(".elp-empty-state").remove();
            initialize(eqpWPlabels);
        }
        else {
            isEqpTileEmpty = true;
            totalRecord = 0;
            loadEntries(0);
            initialize(eqpWPlabels);
        }
    }

    function get_serverType() {
        return "Camstar.WebPortal.WebPortlets.Shopfloor.scsEqpStatusWP, App_Code";
    }

    function get_controlId() {
        return "scsEqpStatusWP";
    }

    function clearAll() {
        lastSelectedEqpTile = null;
        visibleRecord = 0;
        totalRecord = 0;
        filterIconParam = null;
        filterTextParam = null;
        lastSelectedIcon = null;
        lastSelectedIconId = null;
        lastFilterText = null;
        clearReload = false;
    }

    function loadEntries(count) {
        $("#ctl00_WebPartManager_scsEqpStatusWP_LblShowingEntries")[0].innerHTML = "(" + count + " of " + totalRecord + ")";
    }


})();




