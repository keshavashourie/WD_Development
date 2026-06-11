/*
***************************************************************************
Copyright 2023 Siemens
Client side script for the SEMI WIP Main Simple Page
***************************************************************************
*/

var scsWIPMainSimple = (function () {
    'use strict';

    var labels = {};
    //Dispatch Lot
    var isDispatchTileEmpty = false;
    var lastSelectedLotTile = "";
    //Activity Section Start
    var txnData = null;
    var isExpandCell = false;
    var requiredToolPlan = null;
    //Eqp Tile Start
    var totalEqpRecord = 0;
    var isEqpTileEmpty = false;
    var lastSelectedEqpTile = "";
    var observeRightPanelDiv = false;
    var displaySPC = false;
    var LastEqpAvai = 2;

    var IDS = {
        SELECTIONID: '#ctl00_WebPartManager_scsWIPMain_ControllerWP_WIPMain_LotItem_ContainerId_ctl00',
        PROCESSTYPE: '#ctl00_WebPartManager_ControllerWP_WIPMain_ProcessType_Edit',
        ACTIVITYPANELID: '#activityPanelContent',
        READYFORID: 'ctl00_WebPartManager_scsWIPMain_ControllerWP_btnDispFilter',
        AVAILEQPID: 'ctl00_WebPartManager_scsWIPMain_ControllerWP_btnEqpFilter',
        EXPANDCELLID: 'ctl00_WebPartManager_scsWIPMain_ControllerWP_btnExpand',
        HIDDENCONTAINER: 'ctl00_WebPartManager_scsDispatchListWP_HiddenSelectedContainer_ctl00',
        HIDDENSTATE: 'ctl00_WebPartManager_scsWIPMain_ControllerWP_HiddenSelectedState_ctl00',
        HIDDENAVAIL: 'ctl00_WebPartManager_scsWIPMain_ControllerWP_HiddenSelectedAvail_ctl00',
        HIDDENEQP: '#ctl00_WebPartManager_scsEqpStatusWP_HiddenSelectedEqp_ctl00',
        LOTFILTER: 'ctl00_WebPartManager_scsWIPMain_ControllerWP_txtLotId_ctl00',
        HiddenAvaiState: 'ctl00_WebPartManager_scsWIPMain_ControllerWP_HiddenAvaiState_ctl00',
        EQPFILTER: 'ctl00_WebPartManager_scsWIPMain_ControllerWP_txtEquipment_ctl00'
    };
    var SELECTORS = {
        MAIN_HEADER: '#ctl00_WebPartManager_ActivityWP_ActivityToggler .toggle-container .header:first',
        MAIN_HEADER_DIV: '#ctl00_WebPartManager_ActivityWP_ActivityToggler .toggle-container .header:first div'
    };


    var ACTIVITY_VP = {
        "0": "SS_LotBinsPopUpVP",
        "1": "SS_CarrierValidateVP",
        "4": "SS_InProcessSplitPopupVP",
        "5": "SS_LotPackingPopupVP",
        "6": "SS_WIPEquipmentSetupPopupVP",
        "7": "SS_WIPEqpMaterialsSetupPopupVP",
        "8": "SS_ModifyMaintenanceVP",
        "9": "SS_WIPEquipmentSetupPopupVP",
        "10": "SS_ItemRejectsPopUpVP",
        "11": "SS_LotRejectsPopUpVP",
        "13": "SS_SamplingWIPDataPopupVP",
        "14": "SS_WIPSortingPopupVP",
        "15": "SS_SetTestProgramVP",
        "16": "SS_WIPEquipmentSetupPopupVP",
        "17": "SS_WIPDataPopupVP"
    }

    var Controller = {
        selectionId: null,
        equipment: null,
        processType: null,
        employee: null,
        serviceType: null,
        wipFlag: null
    };
    var supportedActivity = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "13", "14", "15", "16", "17"];

    var TEMPLATE = {
        activityTemplate: '<div id="Activity__ACTIVITY_ID_" class="activity-toggle-container">\
                            <div id="Activity__ACTIVITY_ID__Header" class="activity-header" title="Click to expand/collapse the activity section">\
                                <img src="/CamstarPortal/Themes/Horizon/Images/Icons/icon-expand-16x16.svg" id="toggleImg" class="CollapsableSectionImage">\
                                <div class="header-text"><div class="header-title">_ACTIVITY_TITLE_ <img id="openNewTabImg__ACTIVITY_ID_" src="./assets/image/cmdOpen24.svg" title="Open in a new tab" style="width:24px;height:24px;margin-left:10px;margin-top:-6px;">\
                                </div><div class="header-detail">_ACTIVITY_STATUS_</div></div>\
                            </div>\
                            <div class="activity-content" style="display:none"></div>\
                            </div>',
        iconComplete: '<img src="./assets/image/indicatorCheckmarkGreen16.svg" style="width: 24px" title="Complete">',
        iconPending: '<img src="./assets/image/indicatorWarning16.svg" style="width: 24px" title="Pending">',
        iconOptional: '<img src="./assets/image/indicatorOption16.svg" style="width: 24px" title="Optional">',
        iconIncomplete: '<img src="./assets/image/indicatorError16.svg" style="width: 24px" title="Incomplete">'
    };

    //Resize Dispatch list height based on right working section dynamic height
    const wksObserver = new ResizeObserver(function (entries) {
        entries.forEach(function (entry) {
            // each entry is an instance of ResizeObserverEntry
            let vh = document.body.clientHeight - 150;
            if (entry.contentRect.height <= 0) {
                observeRightPanelDiv = false;
            } else {
                if (vh <= entry.contentRect.height) {
                    vh = entry.contentRect.height;
                }
                document.getElementById("ctl00_WebPartManager_scsDispatchListWP_DispatchListTile").style.height = vh + "px";
            }
        });
    })

    const myObserver = new ResizeObserver(function (entries) {

        //Remove not display element from obeserver
        let displayNoneEntries = entries.filter(function (entry) {
            return entry.contentRect.height === 0;
        });
        displayNoneEntries.forEach(function (elem) { myObserver.unobserve(elem.target); });

        if (displayNoneEntries.length > 0)
            return;

        //Adjust element height
        entries.forEach(function (entry) {

            let $content = $(entry.target).closest(".float-form-container");
            let $fixedDiv = $content.find("#nonscrollablepanel");
            let height = 10;
            if ($fixedDiv.length > 0)
                height += $fixedDiv[0].scrollHeight;

            let regex = /\w+\.aspx/;
            let vp = entry.target.baseURI.match(regex);

            if (vp.length > 0) {
                let activityID = getKeyByValue(ACTIVITY_VP, vp[0].slice(0, -5));
                if (activityID === "6") // Multiple activity share same VP
                    activityID = getEqpSetupActivityID(entry.target.baseURI);

                $("#Activity_" + activityID).find(".activity-content").css("height", (height + entry.contentRect.height) + "px");
            }
        });
    });
    //Activity Section End


    // interface
    var scsWIPMainSimpleInterface = {
        initialize: initialize,
        displayEmptyMessage: displayEmptyMessage,
        displayEmptyMessageWithLoad: displayEmptyMessageWithLoad,
        hideLotEmptyMessage: hideLotEmptyMessage,
        highlightLotTile: highlightLotTile,
        unhighlightLotTile: unhighlightLotTile,
        deletedLotFromGrid: deletedLotFromGrid,
        refreshLotInfo: refreshLotInfo,
        showSelectedLot: showSelectedLot,
        uncheckLotCheckbox: uncheckLotCheckbox,
        //Activity Section Start
        setTxnData: setTxnData,
        renderData: renderData,
        setRequiredActivities: setRequiredActivities,
        refreshRequiredActivityAjax: refreshRequiredActivityAjax,
        iframeOnload: iframeOnload,
        get_controlId: get_controlId,
        get_serverType: get_serverType,
        //Activity Section End
        clearAll: clearAll,
        //Eqp Section Start
        hideEqpEmptyMessage: hideEqpEmptyMessage,
        hideEqpWP: hideEqpWP,
        disableEquipmentTextFilter,
        renameAVAILEQPIDButton: renameAVAILEQPIDButton
        //Eqp Section End
    }
    return scsWIPMainSimpleInterface;

    function initialize(labelObj, reInit) {
        console.log("initialize");
        if (reInit) { // only run for page first load or reload
            console.log("initialize: reinit");
            labels = labelObj;

            //temporarily hide the list
            $("#ctl00_WebPartManager_scsDispatchListWP_DispatchListTile").hide();
            $("#ctl00_WebPartManager_scsEqpStatusWP_EquipmentTile").hide();
            $("#WebPart_ActivityWP_UIComponent").hide();
            //register the listener for the refresh activity request
            $(function () {
                var $tab = __page.$getActiveTabPanel();
                if ($tab && $tab.length > 0) {
                    let id = $tab[0].id;
                    var $t = $("#" + id, window.parent.document);
                    var $iframe = $("iframe", $t);
                    $iframe[0].addEventListener('refresh', function (e) {
                        displaySPC = e.detail;
                        refreshRequiredActivityAjax();
                    });
                }
            });

        }

        // Reset the Activity Tiles during initialize
        resetActivityDisplay();
        getControllerData();
        document.getElementById('ctl00_WebPartManager_scsWIPMain_ControllerWP_txtLotId_ctl00').placeholder = "Search by lot";
        document.getElementById('ctl00_WebPartManager_scsWIPMain_ControllerWP_txtEquipment_ctl00').placeholder = "Search by equipment";
        document.getElementById('ctl00_WebPartManager_scsWIPMain_ControllerWP_txtCarrier_ctl00').placeholder = "Search by carrier";

        //Generate Filter Icon drop down list
        generateFilterDropDown();

        hideEmtpyRowAndCell();
        resetActivitiesHeadder();
        buildTextFilterButton();
        buildEqpFilterButton();

        if (!observeRightPanelDiv) {
            observeRightPanelDiv = true;
            const rightSection = document.querySelector('.scswipmainsimple-right #ctl00_AJAXContentPlaceHolder_ctl00_DynamicZone_r1_c1_UIComponent');
            wksObserver.observe(rightSection);
        }
    }

    function displayEmptyMessage(webpartNo) {
        if (webpartNo == "1") { // Dispatch List Tile
            if ($("#WebPart_scsDispatchListWP").find(".lp-empty-state").length == 0) {
                var elementD = $('#WebPart_scsDispatchListWP').append("<div class='lp-empty-state' id='lp-emp-state' />");
                $(".lp-empty-state", elementD).append("<canvas id='lp-empty-state-img' />");
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
                $(".lp-empty-state", elementD).append("<div class='lp-empty-state-txt'>There is no data to display.</>");
                $("#ctl00_WebPartManager_scsDispatchListWP_DispatchListTile").hide();
                isDispatchTileEmpty = true;
            }
        }
        else if (webpartNo == "2") { // Equipment Tiles

            if ($("#WebPart_scsEqpStatusWP").find(".elp-empty-state").length == 0) {
                var elementE = $('#WebPart_scsEqpStatusWP').append("<div class='elp-empty-state' id='elp-emp-state' />");
                $(".elp-empty-state", elementE).append("<canvas id='elp-empty-state-img' />");
                var emptyStateImgEq = document.getElementById("elp-empty-state-img"),
                    ctxEq = emptyStateImgEq.getContext('2d'),
                    imageEq = new Image(192, 192);
                imageEq.src = 'Themes/Horizon/images/icons/typeComputer48.svg';
                imageEq.onload = function () {
                    ctxEq.drawImage(imageEq,
                        0,
                        0,
                        emptyStateImgEq.width,
                        emptyStateImgEq.height);
                }
                $(".elp-empty-state", elementE).append("<div class='lp-empty-state-txt'>There is no data to display.</>");
                $("#ctl00_WebPartManager_scsEqpStatusWP_EquipmentTile").hide();
            }
        }
    }

    function displayEmptyMessageWithLoad(resetData, webpartNo) {
        console.log("displayEmptyMessageWithLoad:" + webpartNo);
        displayEmptyMessage(webpartNo);
        if (webpartNo == "1") {
            refreshLotInfo(0);
        }
    }

    /* PART: Dispatch List Tiles */
    function hideLotEmptyMessage(resetData) {
        console.log("hideLotEmptyMessage");
        isDispatchTileEmpty = false;
        $("#ctl00_WebPartManager_scsDispatchListWP_DispatchListTile").hide();
        setTimeout(updateLotTiles, 200);

    }

    function updateLotTiles() {
        console.log("updateLotTiles");
        $("#ctl00_WebPartManager_scsDispatchListWP_DispatchListTile").show();
        var totalLotCount = 0;
        var TEMPLATE = {
            contentRow: "<span title='Qty: _TT1 Qty2: _TT2'>_QUANTITY_LABEL: _QTY  <img src='assets/image/indicatorContainsInnerMismatches16.svg' style='width:16px;' \
                title='_QTY_LABEL'/>  _QTY2  <img src='assets/image/indicatorPartiallyAssignedByDescendants16.svg' style='width:16px;' title='_QTY2_LABEL'/></span>"
        };

        $("#ctl00_WebPartManager_scsDispatchListWP_DispatchListTile .item").each(function () {
            var $tile = $(this);
            var tileId = $tile.attr('data-custom');
            $tile.attr("title", "Click to select");
            $tile.click(function () { clickLotTile(tileId); });

            var icon = document.createElement("span");
            icon.className = "icon-first";

            $(".icon", $tile).remove();

            if ($tile.find(".icon-first").length == 0)
                $(".textArea", $tile).before(icon);

            // handle tile checkbox behavior
            var chkBox = document.createElement("input");
            chkBox.type = "checkbox";
            chkBox.className = "tileCheckbox";
            chkBox.id = "cb-" + $tile.attr('data-custom');
            chkBox.addEventListener('click', function (e) {
                e.stopPropagation();
            });
            chkBox.addEventListener('change', function () {
                clickCheckBox(tileId, this.checked);
            });
            var $isSelected = $("span:nth-child(9):not([class])", $tile);
            if ($isSelected.text() == "true") {
                chkBox.checked = true;
                totalLotCount++;
            }
            $isSelected.remove();
            icon.appendChild(chkBox);

            var $qty = $("span:nth-child(2):not([class])", $tile);
            var $qty2 = $("span:nth-child(3):not([class])", $tile);
            if (($qty.is(":visible") && $qty.find("img").length == 0)) {
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
            $product.addClass("infoHide");
            var $step = $("span:nth-child(4):not([class])", $tile);
            $step.addClass("infoHide");
            $step.attr("title", $step.text());

            var $state = $("span:nth-child(5):not([class])", $tile);
            var $priority = $("span:nth-child(6):not([class])", $tile);
            var $status = $("span:nth-child(7):not([class])", $tile);

            if ($state.is(":visible")) {

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

            /* 
            // Temporary hide icon-last, use for activity status in future 
            var $icon = $('.icon-last', $tile);
            if ($icon.find(".cs-button-image").length == 0) {
                $icon.append("<input type='button' title='" + labels.SimpleWIPMain + "' class='cs-button-image' style='width: 28px;background-repeat:no-repeat;background-position:center;background-image:url(./assets/image/cmdWorkInProgress24.svg);border-color:#D4D4D4;margin-top: 15px !important;' value=''>");
                $(".cs-button-image", $icon).click(function () {
                    $(".close-button").click();
                    $('#ctl00_WebPartManager_scsDispatchListWP_HiddenSelectedContainer_ctl00').val($icon.parent().attr('data-custom'));
                    $('#ctl00_WebPartManager_ButtonsBar_WIPButton').click();
                });
            }
            */
        });

        //highlight preselected lot tile
        var preselectedLot = $(IDS.SELECTIONID)[0].value;
        if (preselectedLot != null && preselectedLot != "")
            highlightLotTile(preselectedLot);

        refreshLotInfo(totalLotCount);

    }

    // handler for clicking a tile
    function clickLotTile(id) {
        console.log("clickLotTile");
        var type = "select";
        if (!$("#cb-" + id)[0].checked)
            type = "scan" //Scan lot to see whether lot info is validate to proceed and load to lot grid
        triggerCallButton(id, type);
    }

    function clickCheckBox(id, isChecked) {
        console.log("clickCheckBox");
        var type = "scan";
        if (!isChecked)
            type = "delete";
        triggerCallButton(id, type)
    }

    function uncheckLotCheckbox(id) {
        $("#cb-" + id).prop("checked", false);
    }

    function triggerCallButton(id, type) {
        console.log("triggerCallButton:" + type + ", id:" + id);

        let tile = $('[data-custom="' + id + '"]');
        if (tile) {
            $('#' + IDS.HIDDENCONTAINER).val(id);

            if (type == "scan") {
                $("#ctl00_WebPartManager_scsDispatchListWP_btnDispLotScan").click();
            }
            else if (type == "select") {
                $("#ctl00_WebPartManager_scsDispatchListWP_btnDispLotSelect").click();

            }
            else if (type == "delete") {
                $("#ctl00_WebPartManager_scsDispatchListWP_btnDispLotDelete").click();
            }
        }
    }

    function deletedLotFromGrid(id, highlightId) {
        console.log("deleteLotFromGrid");
        unhighlightLotTile(id);
        highlightLotTile(highlightId);
    }

    function highlightLotTile(id) {
        if (id == null) return;
        console.log("highlightLotTile:" + id);
        // UnHighlight if previous highlight lot id is different
        if (id != lastSelectedLotTile)
            unhighlightLotTile(lastSelectedLotTile);

        // Highlight Lot
        let tile = $('[data-custom="' + id + '"]');
        if (tile) {
            tile.addClass("tile-selected");
            $("#cb-" + id).prop("checked", true);
            lastSelectedLotTile = id;

        }
    }

    function unhighlightLotTile(id) {
        if (id == null) return;
        console.log("UnhighlightLotTile:" + id);

        let tile = $('[data-custom="' + id + '"]');
        if (tile) {
            tile.removeClass("tile-selected");
        }
    }

    function refreshLotInfo(lotCount) {
        console.log("refreshLotInfo");
        refreshLotCount(lotCount);
        refreshReadyForButton(lotCount);
    }

    function refreshLotCount(number) {
        console.log("refreshLotCount");
        var totalLotHtml = document.getElementById("ctl00_WebPartManager_scsDispatchListWP_DispatchListTotal");
        if (number > 0)
            totalLotHtml.innerHTML = "(" + number.toString() + ")";
        else
            totalLotHtml.innerHTML = "";
    }

    function showSelectedLot(show) {
        console.log('showSelectedLot');
        var button = document.getElementById("ctl00_WebPartManager_scsDispatchListWP_btnDispLotSelectedFilter")
        if (show)
            button.classList.add("filtered");
        else
            button.classList.remove("filtered");
    }
    /* END PART: Dispatch List Tiles */

    /* PART: Activity Tiles Start*/
    function renderData() {
        console.log("renderData");
        if (txnData != null) {
            setServiceType(txnData.WIPMain_WIPFlagSelection.Value);
            getControllerData();
            showHideActivity();
            setMainToggleHeader(Controller.selectionId);
            setWIPTransactionIcon();
            if (Controller.selectionId)
                setRequiredActivities(txnData.WIPMain_RequiredActivitiesEx);
            else
                refreshRequiredActivityAjax();
            disableEquipmentTextFilter();
        }
    }

    function refreshRequiredActivityAjax() {
        if (Controller.selectionId) {
            var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), scsWIPMainSimple);
            transition.set_command("RefreshRequiredActivitiesList");
            var callParameters =
            {
                "Container": Controller.selectionId,
                "Equipment": Controller.equipment,
                "ProcessType": Controller.processType,
                "WIPFlag": Controller.wipFlag
            };
            var callParamsString = Sys.Serialization.JavaScriptSerializer.serialize(callParameters);
            transition.set_commandParameters(callParamsString);
            transition.set_clientCallback("setRequiredActivities");

            var communicator = new Camstar.Ajax.Communicator(transition, scsWIPMainSimple);
            communicator.syncCall();
            communicator.dispose();
        }
    }

    function setRequiredActivities(requiredActivities) {
        resetActivityDisplay();

        if (requiredActivities.Data && requiredActivities.Data.HTML) {
            let response = JSON.parse(requiredActivities.Data.HTML);
            requiredActivities = response.WIPMain_RequiredActivitiesEx;
            if (response.WIPMain_RequiredToolPlan)
                requiredToolPlan = response.WIPMain_RequiredToolPlan.Name
            else
                requiredToolPlan = null;
            collapseActivity();
        }

        if (!requiredActivities)
            return;

        //sort the activities from not executed to completed
        requiredActivities.sort(function (a, b) {
            return a.scsRequiredActivityEx_Status.Value - b.scsRequiredActivityEx_Status.Value;
        });

        $.each(requiredActivities, function (key, value) {

            let activityID = value.scsRequiredActivityEx_Activity.Value;

            if (!supportedActivity.includes(activityID))
                return;

            // Set activity ID, title and status
            let msg = value.scsRequiredActivityEx_Message.Value

            $(IDS.ACTIVITYPANELID).append(TEMPLATE.activityTemplate
                .replace(/_ACTIVITY_ID_/g, activityID)
                .replace("_ACTIVITY_TITLE_", msg)
                .replace("_ACTIVITY_STATUS_", getActivityStatus(value.scsRequiredActivityEx_Status.Value)));

            // Set activity border and bg color
            setActivityColor($("#Activity_" + activityID), value);

            var isMobile = $(getCEP_top().document.body).hasClass("mobile");

            // Add expand/collapse section and render iframe onclick
            $("#Activity_" + activityID + "_Header").click(function () {


                if (activityID === "2") {
                    $(IDS.BTN_CHECKSHEET).click();
                    $("#Activity_" + activityID + "_Header").attr("title", "Click to open in a new tab");
                }
                else if (activityID === "3") {
                    $(IDS.BTN_EPROC).click();
                    $("#Activity_" + activityID + "_Header").attr("title", "Click to open in a new tab");
                }
                else {
                    let content = this.nextElementSibling;

                    if (content.style.display === "block") {
                        content.style.display = "none";
                        $(this).find(".CollapsableSectionImage").attr("src", "/CamstarPortal/Themes/Horizon/Images/Icons/icon-expand-16x16.svg");
                        //show back all the elements
                        collapseActivity();

                    } else {
                        content.style.display = "block";
                        $(this).find(".CollapsableSectionImage").attr("src", "/CamstarPortal/Themes/Horizon/Images/Icons/icon-collapse-16x16.svg");
                        //temporary hide other elements except current activity
                        //need to press the ctrl key to execute this
                        // if mobile then use this way
                        if (event.ctrlKey === true || isMobile)
                            expandActivity(activityID);
                        renderActivity(value);

                    }
                }

            });

            //redirect not supported in mobile, hide the icon
            if (isMobile) {
                $("#openNewTabImg_" + activityID).hide();
            } else {
                //open activity in a new tab
                $("#openNewTabImg_" + activityID).click(function () {
                    if (activityID === "2") {
                        $(IDS.BTN_CHECKSHEET).click();
                    }
                    else if (activityID === "3") {
                        $(IDS.BTN_EPROC).click();
                    } else {
                        let query = getPageQueryString(activityID);
                        let dataContract = query.dataContract;
                        let pageName = query.pageTitle;
                        let vp = ACTIVITY_VP[activityID];
                        __page.openInNewTab(vp + ".aspx", "responsive=true&DataContracts=" + dataContract, pageName, null, null);
                    }
                    return false;
                });
            }
        });

        $(function () {
            //addSubmitButton();
            if (displaySPC) {
                $("#ctl00_WebPartManager_ButtonsBar_LastSPCDisplayAction").click();
                displaySPC = false;
            }
        });
    }

    function getServiceType(wipFlag, svcType) {
        let binSvcType = "";
        let rejectSvcType = "";

        if (wipFlag == 3) {
            rejectSvcType = "LotRejectsInProcess";
            binSvcType = "LotBinsInProcess";
        }
        else if (wipFlag == 4) {
            if (txnData.WIPMain_AllowRejectsDispose.Value == "true")
                rejectSvcType = "LotRejectsDispose";
            else
                rejectSvcType = "LotRejectsPostProcess";

            if (txnData.WIPMain_AllowBinsDispose.Value == "true")
                binSvcType = "LotBinsDispose";
            else
                binSvcType = "LotBinsPostProcess";
        }

        if (svcType === "Reject")
            return rejectSvcType;
        else if (svcType === "Bin")
            return binSvcType
        else
            return "";

    }

    function getActivityStatus(activityStatus) {
        if (activityStatus == 0) {
            return "Not Executed";
        }
        else if (activityStatus == 1) {
            return "Passed";
        }
        else if (activityStatus == 2) {
            return "Optional";
        }
        else if (activityStatus == 4) {
            return "Incomplete";
        }
        return "Status not found";
    }

    function setTxnData(_txnData) {
        console.log("setTxnData");
        txnData = _txnData;
    }

    function setActivityColor(oStatusDiv, data) {
        resetActivityColor(oStatusDiv);
        var StatusValue = data.scsRequiredActivityEx_Status.Value;
        switch (StatusValue) {
            case "0":
                $(oStatusDiv).addClass("pending");
                $(oStatusDiv).find(".activity-header").append(TEMPLATE.iconPending);
                break;
            case "1":
                $(oStatusDiv).addClass("complete");
                $(oStatusDiv).find(".activity-header").append(TEMPLATE.iconComplete);
                break;
            case "2":
                $(oStatusDiv).addClass("optional");
                $(oStatusDiv).find(".activity-header").append(TEMPLATE.iconOptional);
                break;
            case "4":
                $(oStatusDiv).addClass("error");
                $(oStatusDiv).find(".activity-header").append(TEMPLATE.iconIncomplete);
                break;
        }
    }

    function resetActivityColor(oStatusDiv) {
        $(oStatusDiv).removeClass("pending");
        $(oStatusDiv).removeClass("optional");
        $(oStatusDiv).removeClass("complete");
    }

    function collapseActivity() {
        $("#WebPart_ItemsWP_UIComponent").parent().closest('.row').css("display", "block");
        $("#WebPart_EquipmentWP_UIComponent").parent().closest('.row').css("display", "block");
        $("#WebPart_ControllerWP_UIComponent").parent().closest('.row').css("display", "block");
        $("#WebPart_ControllerWP_UIComponent").parent().closest('.row').css("display", "block");
        if (Controller.wipFlag == "1") {
            $("#WebPart_TrackInPanelWP_UIComponent").parent().closest('.row').css("display", "block");
            $("#WebPart_TrackOutPanelWP_UIComponent").parent().closest('.row').css("display", "none");
            $("#WebPart_MoveOutPanelWP_UIComponent").parent().closest('.row').css("display", "none");
        }
        else if (Controller.wipFlag == "2") {
            $("#WebPart_TrackOutPanelWP_UIComponent").parent().closest('.row').css("display", "block");
            $("#WebPart_TrackInPanelWP_UIComponent").parent().closest('.row').css("display", "none");
            $("#WebPart_MoveOutPanelWP_UIComponent").parent().closest('.row').css("display", "none");
        }
        else if (Controller.wipFlag == "4") {
            $("#WebPart_MoveOutPanelWP_UIComponent").parent().closest('.row').css("display", "block");
            $("#WebPart_TrackInPanelWP_UIComponent").parent().closest('.row').css("display", "none");
            $("#WebPart_TrackOutPanelWP_UIComponent").parent().closest('.row').css("display", "none");
        }
        $("#WebPart_CommentWP_UIComponent").parent().closest('.row').css("display", "block");
        $("#ctl00_WebPartManager_ContentWP_MainToggler_CollapsableSectionExDiv .toggle-control-container .row:last > div").css("display", "block");

        $(".activity-toggle-container", $("#activityPanelContent")).each(function () {
            this.style.display = "block";
        });
    }

    function renderActivity(data) {

        let activityID = data.scsRequiredActivityEx_Activity.Value;
        let $activityContent = $("#Activity_" + activityID).find(".activity-content");
        let iframe = $activityContent.find("iframe");

        if (iframe.length > 0) {
            // Add element back to be observed as it get disconnect when accordion is closed
            myObserver.observe($(iframe).contents().find(".float-form-container #DynamicContentDiv")[0]);
            return;
        }

        let dataContract = getPageQueryString(activityID).dataContract;
        let vp = ACTIVITY_VP[activityID];


        // Render iframe
        if (vp && dataContract) {

            $activityContent.append("<iframe id='FloatingFrame_frame" + activityID + "' width='100%' height='100%' scrolling='no' frameborder='0' marginheight='0px' marginwidth='0px' \
                                 onload='scsWIPMainSimple.iframeOnload(" + activityID + ")' src='/CamstarPortal/" + vp + ".aspx?CallStackKey=" + __page.get_CallStackKey() +
                "&IsFloatingFrame=2&responsive=true&DataContracts=" + dataContract + "' style='display: none;'></iframe>");

        }
        else {
            $activityContent.append("<div>Page Load Error</div>");
        }
    }

    function expandActivity(activityID) {

        $("#WebPart_ItemsWP_UIComponent").parent().closest('.row').css("display", "none");
        $("#WebPart_EquipmentWP_UIComponent").parent().closest('.row').css("display", "none");
        $("#WebPart_ControllerWP_UIComponent").parent().closest('.row').css("display", "none");
        $("#WebPart_ControllerWP_UIComponent").parent().closest('.row').css("display", "none");
        $("#WebPart_TrackInPanelWP_UIComponent").parent().closest('.row').css("display", "none");
        $("#WebPart_TrackOutPanelWP_UIComponent").parent().closest('.row').css("display", "none");
        $("#WebPart_MoveOutPanelWP_UIComponent").parent().closest('.row').css("display", "none");
        $("#WebPart_CommentWP_UIComponent").parent().closest('.row').css("display", "none");
        $("#ctl00_WebPartManager_ContentWP_MainToggler_CollapsableSectionExDiv .toggle-control-container .row:last > div").css("display", "none");
        $(".activity-toggle-container", $("#activityPanelContent")).each(function () {
            if (this.id != ("Activity_" + activityID)) {
                this.style.display = "none";
            }
        });
    }

    function getPageQueryString(activityID) {
        let query = {
            dataContract: null,
            pageTitle: null
        }
        // Assign data contract and VP based on activity
        switch (activityID) {

            case "0": // Binning
                query.dataContract = encodeURIComponent(JSON.stringify({
                    "LotBinsTxn_ContainerDM": Controller.selectionId,
                    "WIPLotBins_IsActive": "true",
                    "LotBinsTxn_ProcessTypeDM": Controller.processType,
                    "LotBinsTxn_EquipmentDM": Controller.equipment,
                    "LotBinsTxn_EmployeeDM": Controller.employee,
                    "LotBinsTxn_ServiceTypeDM": getServiceType(txnData.WIPMain_WIPFlagSelection.Value, "Bin"),
                    "LotBinsTxn_ActivitySectionDM": "true",
                    "WIPMainCallStackKey": __page.get_CallStackKey()
                }));
                query.pageTitle = "Lot Bins - " + Controller.selectionId;
                break;

            case "1": // Carrier Validate
                query.dataContract = encodeURIComponent(JSON.stringify({
                    "ValidateCarrier_Container_DM": Controller.selectionId,
                    "ValidateCarrier_WIPMainTxn_DM": getWIPState(Controller.wipFlag),
                    "ValidateCarrier_ActivitySectionDM": "true",
                    "WIPMainCallStackKey": __page.get_CallStackKey()
                }));
                query.pageTitle = "Carrier Validate - " + Controller.selectionId;
                break;

            case "4": // In Process Split
                query.dataContract = encodeURIComponent(JSON.stringify({
                    "InProcessSplit_SetSelectionIdDM": Controller.selectionId,
                    "InProcessSplit_SetEquipmentDM": Controller.equipment,
                    "InProcessSplit_SetProcessTypeDM": Controller.processType,
                    "InProcessSplit_ActivitySectionDM": "true",
                    "WIPMainCallStackKey": __page.get_CallStackKey()
                }));
                query.pageTitle = "In-Process Split - " + Controller.selectionId;
                break;


            case "5": // Lot Packing
                query.dataContract = encodeURIComponent(JSON.stringify({
                    "LotPacking_SelectionIdDM": Controller.selectionId,
                    "LotPacking_EmployeeDM": Controller.employee,
                    "LotPacking_ActivitySectionDM": "true",
                    "WIPMainCallStackKey": __page.get_CallStackKey()
                }));
                query.pageTitle = "Lot Packing - " + Controller.selectionId;
                break;

            case "7": // Material Setup
                query.dataContract = encodeURIComponent(JSON.stringify({
                    "WIPEqpMaterialsSetup_ContainerDM": Controller.selectionId,
                    "WIPEqpMaterialsSetup_IsActive": "true",
                    "WIPEqpMaterialsSetup_ProcessTypeDM": Controller.processType,
                    "WIPEqpMaterialsSetup_EquipmentDM": Controller.equipment,
                    "WIPEqpMaterialsSetup_ActivitySectionDM": "true",
                    "WIPMainCallStackKey": __page.get_CallStackKey()
                }));
                query.pageTitle = "Equipment Materials Setup - " + Controller.equipment;
                break;

            case "8": // Modify Maintenance
                query.dataContract = encodeURIComponent(JSON.stringify({
                    "ModifyMaintenance_ActivitySectionDM": "true",
                    "WIPMainCallStackKey": __page.get_CallStackKey()
                }));
                query.pageTitle = "Modify Maintenance";
                break;

            case "9": // Recipe
                query.dataContract = encodeURIComponent(JSON.stringify({
                    "EquipmentSetup_ToolPlanLotDM": Controller.selectionId,
                    "WIPEqpSetup_IsActive": "true",
                    "EquipmentSetup_ToolPlanProcessTypeDM": Controller.processType,
                    "EquipmentSetup_ResourceDM": Controller.equipment,
                    "EquipmentSetup_EmployeeDM": Controller.employee,
                    "WIPEquipmentSetup_ServiceTypeDM": "EquipmentSetup",
                    "WIPEqpSetup_ActivitySectionDM": "Recipe",
                    "WIPMainCallStackKey": __page.get_CallStackKey()
                }));
                query.pageTitle = "Equipment Setup - " + Controller.equipment;
                break;

            case "6": // Mask
                query.dataContract = encodeURIComponent(JSON.stringify({
                    "EquipmentSetup_ToolPlanLotDM": Controller.selectionId,
                    "WIPEqpSetup_IsActive": "true",
                    "EquipmentSetup_ToolPlanProcessTypeDM": Controller.processType,
                    "EquipmentSetup_ResourceDM": Controller.equipment,
                    "EquipmentSetup_EmployeeDM": Controller.employee,
                    "WIPEquipmentSetup_ServiceTypeDM": "EquipmentSetup",
                    "WIPEqpSetup_ActivitySectionDM": "Mask",
                    "WIPMainCallStackKey": __page.get_CallStackKey()
                }));
                query.pageTitle = "Equipment Setup - " + Controller.equipment;

                break;

            case "10": // Item Rejects
                query.dataContract = encodeURIComponent(JSON.stringify({
                    "WIPItemRejects_ContainerDM": Controller.selectionId,
                    "WIPItemRejects_IsPopupDM": "true",
                    "WIPItemRejects_EquipmentDM": Controller.equipment,
                    "WIPItemRejects_ProcessTypeDM": Controller.processType,
                    "WIPItemRejects_ActivitySectionDM": "true",
                    "WIPItemRejects_EmployeeDM": Controller.employee,
                    "WIPItemRejects_ServiceTypeDM": getServiceType(txnData.WIPMain_WIPFlagSelection.Value, "Reject"),
                    "WIPMainCallStackKey": __page.get_CallStackKey()
                }));
                query.pageTitle = "Items Reject - Lot " + Controller.selectionId;
                break;

            case "11": // Lot Rejects
                query.dataContract = encodeURIComponent(JSON.stringify({
                    "WIPLotRejects_ContainerDM": Controller.selectionId,
                    "WIPLotRejects_IsActiveDM": "true",
                    "WIPLotRejects_IsPopupDM": "true",
                    "WIPLotRejects_EquipmentDM": Controller.equipment,
                    "WIPLotRejects_ProcessTypeDM": Controller.processType,
                    "WIPLotRejects_ActivitySectionDM": "true",
                    "WIPLotRejects_EmployeeDM": Controller.employee,
                    "WIPLotRejects_ServiceTypeDM": getServiceType(txnData.WIPMain_WIPFlagSelection.Value, "Reject"),
                    "WIPMainCallStackKey": __page.get_CallStackKey()
                }));
                query.pageTitle = "Lot Reject - Lot " + Controller.selectionId;
                break;

            case "13": // Sampling
                query.dataContract = encodeURIComponent(JSON.stringify({
                    "SamplingWIPData_ContainerDM": Controller.selectionId,
                    "SamplingWIPData_IsActive": "true",
                    "SamplingWIPData_IsPopupDM": "true",
                    "SamplingWIPData_EquipmentDM": Controller.equipment,
                    "SamplingWIPData_ServiceNameDM": Controller.serviceType,
                    "SamplingWIPData_ProcessTypeDM": Controller.processType,
                    "SamplingWIPData_ActivitySectionDM": "true",
                    "SamplingWIPData_EmployeeDM": Controller.employee,
                    "WIPMainCallStackKey": __page.get_CallStackKey()
                }));
                query.pageTitle = "Sampling - Lot " + Controller.selectionId;
                break;

            case "14": // Sorting
                query.dataContract = encodeURIComponent(JSON.stringify({
                    "WIPSorting_SelectionIdDM": Controller.selectionId,
                    "WIPSorting_EmployeeDM": Controller.employee,
                    "WIPSorting_IsActive": "true",
                    "WIPSorting_ActivitySectionDM": "true",
                    "WIPMainCallStackKey": __page.get_CallStackKey()
                }));
                query.pageTitle = "Sorting - " + Controller.selectionId;
                break;

            case "15": // Test Program
                query.dataContract = encodeURIComponent(JSON.stringify({
                    "SetTestProgram_SelectionId_DM": Controller.selectionId,
                    "SetTestProgram_Employee_DM": Controller.employee,
                    "SetTestProgram_ActivitySectionDM": "true",
                    "WIPMainCallStackKey": __page.get_CallStackKey()
                }));
                query.pageTitle = "Test Program - " + Controller.selectionId;
                break;

            case "16": // Tool plan
                query.dataContract = encodeURIComponent(JSON.stringify({
                    "EquipmentSetup_ToolPlanLotDM": Controller.selectionId,
                    "WIPEqpSetup_IsActive": "true",
                    "EquipmentSetup_ToolPlanProcessTypeDM": Controller.processType,
                    "EquipmentSetup_ResourceDM": Controller.equipment,
                    "EquipmentSetup_EmployeeDM": Controller.employee,
                    "WIPEquipmentSetup_ServiceTypeDM": "EquipmentSetup",
                    "WIPEqpSetup_IsToolPlan": "true",
                    "WIPEqpSetup_ActivitySectionDM": "Tool",
                    "WIPMainCallStackKey": __page.get_CallStackKey(),
                    "WIPEqpSetup_RequiredToolPlan": requiredToolPlan,
                }));
                query.pageTitle = "Equipment Setup - " + Controller.equipment;
                break;

            case "17": // WIP Data
                query.dataContract = encodeURIComponent(JSON.stringify({
                    "WIPData_ContainerDM": Controller.selectionId,
                    "WIPData_IsActiveDM": "true",
                    "WIPData_IsPopupDM": "true",
                    "WIPData_EquipmentDM": Controller.equipment,
                    "WIPData_ServiceNameDM": Controller.serviceType,
                    "WIPData_ProcessTypeDM": Controller.processType,
                    "WIPData_ActivitySectionDM": "true",
                    "WIPData_EmployeeDM": Controller.employee,
                    "WIPMainCallStackKey": __page.get_CallStackKey()
                }));
                query.pageTitle = "WIP Data Collection - Lot " + Controller.selectionId;
                break;
        }
        return query;
    }

    function iframeOnload(activityID) {
        let frame = document.getElementById("FloatingFrame_frame" + activityID);
        frame.style.display = "block";

        let $activityContent = $("#Activity_" + activityID).find(".activity-content");
        let $frameContent = $(frame).contents();

        var frame_doc = frame.contentWindow ? frame.contentWindow.document : frame.contentDocument;
        setEmptyCell(frame_doc);

        // Remove loading span
        $activityContent.find('span:first').remove();

        // Add element to be observed
        myObserver.observe($frameContent.find(".float-form-container #DynamicContentDiv")[0]);
    }

    function setEmptyCell(elementId) {
        $('.webpart .matrix', $(elementId)).each(function (matrixNum, matrix) {
            if ($(matrix).is(":visible") == true) {
                $('tr, div.row', $(matrix)).each(function (rn, r) {
                    ($(r).children("td.cell, div.cell-m")).each(function (cn, c) {
                        var visibleChildren = $(c).children(":visible");
                        if (visibleChildren.length == 0) {
                            $(c).addClass("empty");
                        }
                    })
                        .last().addClass('last-cell');
                });
            }
        });
    }

    function addSubmitButton() {
        $("#ctl00_WebPartManager_ContentWP_MainToggler_CollapsableSectionExDiv .toggle-control-container .row:last div").remove();
        let $activityContent = $(".pending", $("#activityPanelContent"));
        let disabled = "";
        $("#ctl00_WebPartManager_ButtonsBar_Submit").removeAttr("disabled", "disabled");
        $("#SubmitAction").removeClass("cmdbar-aspNetDisabled");
        if ($activityContent && $activityContent.length > 0 || (Controller.wipFlag == 1 && getValue(IDS.EQUIPMENT) == "")) {
            disabled = "disabled='disabled'";
            $("#SubmitAction").addClass("cmdbar-aspNetDisabled");
            $("#ctl00_WebPartManager_ButtonsBar_Submit").attr("disabled", "disabled");
        }
        var div = document.createElement("div");
        div.style.width = "100%";
        div.style.padding = "0px 30px 10px 30px";
        var state = getWIPState(txnData.WIPMain_WIPFlagSelection.Value);
        div.innerHTML = "<input type='submit' onclick='$(\"#ctl00_WebPartManager_ButtonsBar_Submit\").click();' title='" + state + "' class='cs-button' " + disabled + " value='" + state + "' style='float: right;'/>";
        $("#ctl00_WebPartManager_ContentWP_MainToggler_CollapsableSectionExDiv .toggle-control-container .row:last").append(div);
    }

    function getWIPState(wipFlag) {
        if (wipFlag == 1) {
            return "Track In";
        } else if (wipFlag == 3) {
            return "Track Out";
        }
        else if (wipFlag == 5) {
            return "Move In";
        }
        else if (wipFlag == 4) {
            return "Move Out";
        }
        return "No State";
    }

    function setServiceType(wipFlag) {
        if (wipFlag == 1) {
            Controller.serviceType = "TrackInLot";
            Controller.wipFlag = "1";
        } else if (wipFlag == 3) {
            Controller.serviceType = "TrackOutLot";
            Controller.wipFlag = "2";
        }
        else if (wipFlag == 5) {
            Controller.wipFlag = "5";
        }
        else if (wipFlag == 4) {
            Controller.serviceType = "LotMoveOut";
            Controller.wipFlag = "4";
        }
    }

    function get_serverType() {
        return "Camstar.WebPortal.WebPortlets.Shopfloor.scsWIPMainSimple, App_Code";
    }

    function get_controlId() {
        return "scsWIPMainSimple";
    }

    function getKeyByValue(object, value) {
        return Object.keys(object).find(key => object[key] === value);
    }

    function getControllerData() {
        Controller.selectionId = getValue(IDS.SELECTIONID);
        Controller.equipment = getValue(IDS.HIDDENEQP);
        Controller.processType = getValue(IDS.PROCESSTYPE);
        Controller.employee = getValue(IDS.EMPLOYEE)
    }

    function getValue(id) {
        var inputField = $(id);
        return inputField.val();
    }

    function resetActivityDisplay() {
        $(IDS.ACTIVITYPANELID).empty();
        myObserver.disconnect();
    }

    function setValue(id, value) {
        var inputField = $(id);
        return inputField.val(value);
    }

    function showHideActivity() {
        var selectedlot = $("#ctl00_WebPartManager_scsWIPMain_ControllerWP_WIPMain_LotItem_ContainerId_ctl00").val();
        if (selectedlot)
            $("#WebPart_ActivityWP_UIComponent").show();
        else
            $("#WebPart_ActivityWP_UIComponent").hide();
    }

    function setWIPTransactionIcon() {
        $(".scsWIPMainSimple #WIPTransactionIcon").remove();
        var icons = ['indicatorMoveIn16', 'indicatorInputIntended16', 'indicatorOutputIntended16', 'indicatorMoveOut16'];
        var wipState = [5, 1, 3, 4];
        var div = '<div id="WIPTransactionIcon">';
        var WIPFlagValue = txnData.WIPMain_WIPFlagSelection.Value;

        for (var i = 0; i < icons.length; i++) {
            var active = txnData.WIPMain_WIPFlagSelection.Value == wipState[i] ? ' active' : '';
            if (active)
                div += '<div class="item' + active + '" title="' + getWIPState(wipState[i]) + '"><img src="./assets/image/' + icons[i] + '.svg" title="' + getWIPState(wipState[i]) + '"><div class="bar"></div></div>';
        }
        div += '</div>';
        $(SELECTORS.MAIN_HEADER_DIV).append(div);

    }

    function setMainToggleHeader(selectedLot) {
        $(SELECTORS.MAIN_HEADER_DIV).remove();
        var node = document.createElement("div");
        node.style.width = "100%";

        var state = document.createElement("span");
        state.innerHTML = getWIPState(txnData.WIPMain_WIPFlagSelection.Value) + "&nbsp;" + "Activities" + "&nbsp" + selectedLot;
        node.appendChild(state);

        $(SELECTORS.MAIN_HEADER).append(node);
    }

    function resetActivitiesHeadder() {
        $(SELECTORS.MAIN_HEADER_DIV).remove();
        $("#WebPart_ActivityWP_UIComponent").hide();
    }
    /* END PART: Activity Tiles End*/

    /* START PART: Filter DropDown*/
    function generateFilterDropDown() {
        var readyForDiv = document.getElementById(IDS.READYFORID);
        var availDiv = document.getElementById(IDS.AVAILEQPID);
        var expandCell = document.getElementById(IDS.EXPANDCELLID);

        //change input type to button
        readyForDiv.type = "button";
        availDiv.type = "button";
        expandCell.type = "button";

        // Create Ready For drop down
        var dropdowngrp = document.createElement('div');
        dropdowngrp.className = "dropdown dropdown__filter";
        dropdowngrp.appendChild(readyForDiv.cloneNode(true));
        readyForDiv.parentNode.replaceChild(dropdowngrp, readyForDiv);
        var dropdownRF = document.createElement('div');
        dropdownRF.className = "dropdown-content";
        dropdownRF.id = "readyForMenu";

        const readyForState = [5, 1, 3, 4];
        const readyForIcon = ["move-in", "track-in", "track-out", "move-out"];
        for (let i = 0; i < readyForState.length; i++) {
            var readyForItem = document.createElement('button');
            readyForItem.className = "dropdown-item " + readyForIcon[i];
            readyForItem.type = "button";
            readyForItem.innerHTML = getWIPState(readyForState[i]);
            dropdownRF.appendChild(readyForItem);

        }
        dropdowngrp.appendChild(dropdownRF.cloneNode(true));

        // Create Availability drop down
        dropdowngrp = document.createElement('div');
        dropdowngrp.className = "dropdown";
        dropdowngrp.appendChild(availDiv.cloneNode(true));
        availDiv.parentNode.replaceChild(dropdowngrp, availDiv);
        dropdownRF = document.createElement('div');
        dropdownRF.className = "dropdown-content";
        dropdownRF.id = "availMenu";

        const availLabel = ["Available", "Unavailable"];
        for (let i = 0; i < availLabel.length; i++) {
            var avaiItem = document.createElement('button');
            avaiItem.className = "dropdown-item " + availLabel[i].toLowerCase();
            avaiItem.type = "button";
            avaiItem.innerHTML = availLabel[i];
            dropdownRF.appendChild(avaiItem);

        }
        dropdowngrp.appendChild(dropdownRF.cloneNode(true));
        // Register listener for Ready for menu click
        const rFDiv = document.getElementById(IDS.READYFORID);
        rFDiv.addEventListener('click', function (e) {
            document.getElementById("availMenu").classList.remove("show");
            document.getElementById("readyForMenu").classList.toggle("show");
            e.preventDefault();
        });
        // Close dropdown menu when clicking outside
        window.onclick = function (event) {
            if (!event.target.matches('.dropdown__filter') && !event.target.matches('.wipmainsimple-icon')) {
                document.getElementById("availMenu").classList.remove("show");
                document.getElementById("readyForMenu").classList.remove("show");
            }
        }
        // Register listener for Ready for menu item click
        const breakdownButton = document.querySelectorAll('#readyForMenu .dropdown-item');
        breakdownButton.forEach(function (btn, i) {
            btn.addEventListener('click', function () {
                document.getElementById("readyForMenu").classList.remove("show");
                var filterState = document.getElementById(IDS.HIDDENSTATE).value;
                filterState = filterState == "" ? 99 : parseInt(filterState);
                if (filterState == i) {
                    $('#readyForMenu button:nth-child(' + (i + 1) + ')').removeClass("selected");
                    document.getElementById(IDS.HIDDENSTATE).setAttribute("value", "");
                } else {
                    if (filterState != 99) {
                        $('#readyForMenu button:nth-child(' + (filterState + 1) + ')').removeClass("selected");
                    }
                    $('#readyForMenu button:nth-child(' + (i + 1) + ')').addClass("selected");
                    document.getElementById(IDS.HIDDENSTATE).setAttribute("value", i);
                }
                $("#ctl00_WebPartManager_scsDispatchListWP_btnDispLotFilter").click();
            });
        });
        // Register listener for Availability menu click
        const aDiv = document.getElementById(IDS.AVAILEQPID);
        aDiv.addEventListener('click', function (e) {
            document.getElementById("readyForMenu").classList.remove("show");
            document.getElementById("availMenu").classList.toggle("show");
            e.preventDefault();
        });
        // Register listener for Availability menu item click
        const availButton = document.querySelectorAll('#availMenu .dropdown-item');
        availButton.forEach(function (btn, i) {
            btn.addEventListener('click', function () {
                document.getElementById("availMenu").classList.remove("show");
                var filterAvail = document.getElementById(IDS.HIDDENAVAIL).value;
                filterAvail = filterAvail == "" ? 99 : parseInt(filterAvail);
                if (filterAvail == LastEqpAvai) {
                    $('#availMenu button:nth-child(' + (i + 1) + ')').removeClass("selected");
                    document.getElementById(IDS.HIDDENAVAIL).setAttribute("value", "");
                } else {
                    if (filterAvail != 99) {
                        $('#availMenu button:nth-child(' + (filterAvail + 1) + ')').removeClass("selected");
                    }
                    $('#availMenu button:nth-child(' + (i + 1) + ')').addClass("selected");
                    if (i == 0) {
                        document.getElementById(IDS.HIDDENAVAIL).setAttribute("value", 1);
                    }
                    else {
                        document.getElementById(IDS.HIDDENAVAIL).setAttribute("value", 0);
                    }
                }
                LastEqpAvai = parseInt(filterAvail);
                $("#ctl00_WebPartManager_scsEqpStatusWP_btnEqpStatusFilter").click();
            });
        });

        //Register listener for Expand Cell button click
        if (isExpandCell) {
            $('#' + IDS.EXPANDCELLID).addClass('show');
        }
        $('#' + IDS.EXPANDCELLID).click(function () {
            console.log("EXPANDCELLID");
            isExpandCell = !isExpandCell;
            this.classList.toggle('show');
            const lotHideinfos = document.querySelectorAll('.scswipmainsimple .tileContainer .infoHide');
            lotHideinfos.forEach(function (info, i) {
                info.classList.toggle('show');
            });
        });
    }

    function refreshReadyForButton(lotCount) {
        console.log("refreshReadyForButton");
        if (lotCount > 0) {
            document.getElementById(IDS.READYFORID).disabled = true;
        } else {
            document.getElementById(IDS.READYFORID).disabled = false;
        }
        let filterState = document.getElementById(IDS.HIDDENSTATE);
        if (filterState != null && filterState.value) {
            $('#readyForMenu button:nth-child(' + (parseInt(filterState.value) + 1) + ')').addClass("selected");
        }
        renameReadyForButton();
    }

    function renameReadyForButton() {
        console.log("renameReadyForButton");
        const readyForState = [5, 1, 3, 4];
        const readyForIcon = ["move-in", "track-in", "track-out", "move-out"];
        const readyForBtn = document.getElementById(IDS.READYFORID);
        readyForBtn.classList.remove("move-in", "track-in", "track-out", "move-out");
        let filterState = document.getElementById(IDS.HIDDENSTATE);
        if (filterState != null && filterState.value) {
            readyForBtn.value = getWIPState(readyForState[filterState.value]);
            readyForBtn.classList.add(readyForIcon[filterState.value]);
        } else {
            readyForBtn.value = "Ready For";
        }
    }

    function renameAVAILEQPIDButton() {
        console.log("renameAVAILEQPIDButton");
        const Availability = [0, 1];
        const AvailabilityBtn = document.getElementById(IDS.AVAILEQPID);
        const AvailabilityIcon = ["Available", "Unavailable"];
        AvailabilityBtn.classList.remove("Available", "Unavailable");
        let filterState = document.getElementById(IDS.HiddenAvaiState);
        if (filterState != null && filterState.value) {
            var TempAvaiState = "";

            if (filterState.value == "Available")
                TempAvaiState = 0;
            else if (filterState.value == "Unavailable")
                TempAvaiState = 1;
            AvailabilityBtn.value = filterState.value;
            AvailabilityBtn.classList.add(AvailabilityIcon[TempAvaiState].toLowerCase());

        } else {
            AvailabilityBtn.value = "Availability";
        }
    }

    /* END PART: Filter DropDown*/

    /* PART: Text Filter */
    function buildTextFilterButton() {
        var lotInput = document.getElementById(IDS.LOTFILTER);


        // Create Lot Text Filter Inner Button
        var parentDiv = document.createElement('div');
        parentDiv.className = "simple-wipmain-buttonIn";
        parentDiv.appendChild(lotInput.cloneNode(true));
        lotInput.parentNode.replaceChild(parentDiv, lotInput);
        var innerBtn = document.createElement('input');
        innerBtn.id = "simple-wipmain-lotInnerBtn";
        innerBtn.title = "Clear Lot Filter";
        innerBtn.type = 'button';
        innerBtn.class = 'clear';
        parentDiv.appendChild(innerBtn.cloneNode(true));
        let clearBtn = document.getElementById("simple-wipmain-lotInnerBtn");
        lotInput = document.getElementById(IDS.LOTFILTER);

        addEventListenerForTextFilter(lotInput, clearBtn);

    }

    function addEventListenerForTextFilter(input, innerBtn) {
        if (input.value.length != 0)
            innerBtn.style.display = "inline-block";
        innerBtn.addEventListener('click', () => {
            if (input.value.length != 0) {
                input.value = "";
                input.dispatchEvent(new Event('change'));
            }
        });
        input.addEventListener("input", function () {
            if (input.value === "") {
                innerBtn.style.display = "none";
            } else {
                innerBtn.style.display = "inline-block";
            }
        });
    }

    function buildEqpFilterButton() {
        var eqpInput = document.getElementById(IDS.EQPFILTER);

        // Create Equipment Text Filter Inner Button
        var parentDiv = document.createElement('div');
        parentDiv.className = "simple-wipmain-buttonIn";
        parentDiv.appendChild(eqpInput.cloneNode(true));
        eqpInput.parentNode.replaceChild(parentDiv, eqpInput);
        var innerBtn = document.createElement('input');
        innerBtn.id = "simple-wipmain-EqpInnerBtn";
        innerBtn.title = "Clear Eqp Filter";
        innerBtn.type = 'button';
        innerBtn.class = 'clear';
        parentDiv.appendChild(innerBtn.cloneNode(true));
        let clearBtn = document.getElementById("simple-wipmain-EqpInnerBtn");
        eqpInput = document.getElementById(IDS.EQPFILTER);
        addEventListenerForTextFilter(eqpInput, clearBtn);
    }
    /* END PART: Text Filter*/

    /* PART: Eqp List Tiles */
    function hideEqpEmptyMessage(resetData) {
        console.log("hideEqpEmptyMessage");

        isEqpTileEmpty = false;
        $("#ctl00_WebPartManager_scsEqpStatusWP_EquipmentTile").hide()
        setTimeout(updateEqpTiles, 200);
    }

    function updateEqpTiles() {
        totalEqpRecord = 0;
        var TempEqp = "";
        $("#WebPart_scsEqpStatusWP_UIComponent").show();
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
            TempEqp = eqpName;
            $tile.click(function () { clickEqpTile(eqpName); });
            $tile.attr("title", eqpName);

            var $LotCount = $("span:nth-child(2):not([class])", $tile);
            var $EquipmentAvailability = $("span:nth-child(3):not([class])", $tile);
            var $isSelected = $("span:nth-child(4):not([class])", $tile);
            var $RequiredPM = $("span:nth-child(5):not([class])", $tile);

            if ($isSelected.text() == "true") {
                highlightEqpTile(eqpName);
            }

            $LotCount.hide();
            $EquipmentAvailability.hide();
            $isSelected.hide();
            $RequiredPM.hide();


            // Add Tile Image
            let icon = document.createElement("span");
            icon.className = "eqp-icon-first";

            $(".icon", $tile).remove();

            if ($tile.find(".eqp-icon-first").length == 0)
                $(".titleArea", $tile).before(icon);

            let iconStatusHTML = "";

            if ($tile.find(".eqp-icon-first").length > 0) {

                // Add Availibility and Unavailibility Icon
                if ($EquipmentAvailability.text() == "True") {
                    $(".title", $tile).before(TEMPLATE.imgEqpAvailable);
                    $tile.addClass('eqp-available');
                    // Add RequiredPM
                    if ($RequiredPM.text() == "True") {
                        $(".title", $tile).before(TEMPLATE.imgPMDue);
                    }
                }

                else {

                    $(".title", $tile).before(TEMPLATE.imgEqpUnavailable);
                    $tile.addClass('eqp-unavailable');
                    //Add RequiredPM Icon
                    if ($RequiredPM.text() == "True") {
                        $(".title", $tile).before(TEMPLATE.imgPMDue);
                    }
                }
            }

            // Add Lot Count Span
            if (parseInt($LotCount.text()) > 0)
                $(".eqp-icon-first", $tile).append(TEMPLATE.lotCountHTML.replace("_LOT_COUNT_VALUE_", $LotCount.text()).replace("_LOT_COUNT_ID", eqpName + '_EqpStatusWP_Tile_LotCount'))


            $tile.css('cursor', 'pointer');

            totalEqpRecord++;
            if (totalEqpRecord > 50)
                return false;
        });

        $('#ctl00_WebPartManager_scsEqpStatusWP_EquipmentTile .icon-last').hide();
    };

    function clickEqpTile(eqpName) {
        let tile = $('[data-custom="' + eqpName + '"]');
        if (tile) {
            if (tile.hasClass("eqp-unavailable")) {
                $(IDS.HIDDENEQP).val(eqpName + "_UNAVAILABLE");
            } else {
                $(IDS.HIDDENEQP).val(eqpName);
                highlightEqpTile(eqpName);
            }
            $("#ctl00_WebPartManager_scsEqpStatusWP_btnEqpSelect").click();
        }
    }

    function highlightEqpTile(id) {
        if (id == null) return;
        console.log("highlightEqpTile:" + id);
        // UnHighlight if previous highlight lot id is different
        if (id != lastSelectedEqpTile)
            unhighlightEqpTile(lastSelectedEqpTile);

        // Highlight Lot
        let tile = $('[data-custom="' + id + '"]');
        if (tile) {
            tile.addClass("tile-selected");
            lastSelectedEqpTile = id;

        }
    }

    function unhighlightEqpTile(id) {
        if (id == null) return;
        console.log("UnhighlightEqpTile:" + id);

        let tile = $('[data-custom="' + id + '"]');
        if (tile) {
            tile.removeClass("tile-selected");
        }
    }

    function hideEqpWP() {
        console.log("hideEqpWP");
        $("#WebPart_scsEqpStatusWP_UIComponent").hide();
    }

    function disableEquipmentTextFilter() {
        if (Controller.wipFlag != 1)
            document.getElementById(IDS.EQPFILTER).disabled = "true";
    }
    /* PART: Eqp List Tiles */

    function clearAll() {
        console.log("clearAll");
        //Dispatch Lot
        isDispatchTileEmpty = false;
        lastSelectedLotTile = "";
        $('#' + IDS.EXPANDCELLID).removeClass('show');
        document.getElementById(IDS.HIDDENSTATE).setAttribute("value", "");
        document.getElementById(IDS.HIDDENCONTAINER).setAttribute("value", "");
        //Activity Section Start
        txnData = null;
        isExpandCell = false;
        //Eqp Tile Start
        totalEqpRecord = 0;
        isEqpTileEmpty = false;
        lastSelectedEqpTile = "";
        document.getElementById(IDS.HIDDENAVAIL).setAttribute("value", "");
    }

    function hideEmtpyRowAndCell() {
        //hide empty rows and cells
        setEmptyCell("#WebPart_ProcessWP_UIComponent");

        $('.row', $("#ctl00_WebPartManager_ProcessWP_ProcessTypeToggler_CollapsableSectionExDiv")).each(function (rn, row) {
            $('.webpart', $(row)).each(function (wn, webpart) {
                if ($(webpart).is(":visible") == false) {
                    $(row).addClass("empty");
                } else {
                    $(row).removeClass("empty");
                }
            });

        });
    }
})();