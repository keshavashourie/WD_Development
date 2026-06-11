// Copyright Siemens 2023

var isIndustryDefect = (function () {
    'use strict';

//    var documents = new CR.DocSlideOut.DocCollection();  //for the document slideout - view document
    var LAYOUT_ROWS = {
        RESOURCE: 0,
        DEFECT_BUTTON: 1,
        DEFECT_GRID: 2
    };
    Object.freeze(LAYOUT_ROWS);

    var DEFECT_BUTTON_IDS = {
        NEW: 'defectButtonNew',
        REPAIR: 'defectButtonRepair',
        REPAIR_ADVISOR: 'defectButtonRepairAdvisor',
        REOPEN: 'defectButtonReopen',
        CHANGE_REASON: 'defectButtonChangeReason',
        NOTES: 'defectButtonNotes',
        DELETE: 'defectButtonDelete',
        EXPORT: 'defectButtonExport'
    };
    Object.freeze(DEFECT_BUTTON_IDS);

    var DEFECT_BUTTON_SELECTORS = {
        NEW: '#' + DEFECT_BUTTON_IDS.NEW,
        REPAIR: '#' + DEFECT_BUTTON_IDS.REPAIR,
        REPAIR_ADVISOR: '#' + DEFECT_BUTTON_IDS.REPAIR_ADVISOR,
        REOPEN: '#' + DEFECT_BUTTON_IDS.REOPEN,

        CHANGE_REASON: '#' + DEFECT_BUTTON_IDS.CHANGE_REASON,
        NOTES: '#' + DEFECT_BUTTON_IDS.NOTES,
        DELETE: '#' + DEFECT_BUTTON_IDS.DELETE,
        EXPORT: '#' + DEFECT_BUTTON_IDS.EXPORT
    };
    Object.freeze(DEFECT_BUTTON_SELECTORS);

    var CTRL_IDS = {
        POPUP_LOG_DEFECT: 'ctl00_WebPartManager_BlankWP_PopupLogDefect',
        POPUP_REPAIR_DEFECT: 'ctl00_WebPartManager_BlankWP_PopupRepairDefect',
        POPUP_VIEW_REPAIRS: 'ctl00_WebPartManager_BlankWP_ViewRepairInfo',
        REOPEN_DEFECTS: 'ctl00_WebPartManager_BlankWP_ReopenDefects',
        DELETE_DEFECTS: 'ctl00_WebPartManager_BlankWP_DeleteDefects',
        CHANGE_DEFECT_REASON: 'ctl00_WebPartManager_BlankWP_ChangeDefectReason',
        POPUP_SHOW_NOTES: 'ctl00_WebPartManager_BlankWP_PopupNotes',
        POPUP_REPAIR_ADVISOR: 'ctl00_WebPartManager_BlankWP_RepairAdvisor',
        DEFECT_GRID: 'ctl00_WebPartManager_BlankWP_isCurrentDefectGrid',
        SELECTED_DEFECTS_JSON: 'ctl00_WebPartManager_BlankWP_SelectedDefectsJson_ctl00',
        CONTAINER_NAME: 'ctl00_WebPartManager_ContainerStatusWP_R2_ContainerStatus_ContainerName_Edit',
        LABEL_DICTIONARY_JSON: 'ctl00_WebPartManager_BlankWP_LabelDictionaryJson_ctl00',
        RELOAD_DEFECT_GRID_BTN: 'ctl00_WebPartManager_BlankWP_ReloadDefectGrid',
        RESOURCENAME: 'ctl00_WebPartManager_BlankWP_ResourceName_ctl00',
        EXPORT_DEFECT: 'gview_ctl00_WebPartManager_BlankWP_isCurrentDefectGrid span.ui-icon.ui-icon-excel',
        ADVISOR_ACTIONS: 'ctl00_WebPartManager_BlankWP_RepairAdvisorSelectedActions_ctl00',
        REPAIR_FROM_ADVISOR: 'ctl00_WebPartManager_BlankWP_RepairFromAdvisor'
    };
    Object.freeze(CTRL_IDS);

    var CTRL_SELECTORS = {
        POPUP_LOG_DEFECT: '#' + CTRL_IDS.POPUP_LOG_DEFECT,
        POPUP_REPAIR_DEFECT: '#' + CTRL_IDS.POPUP_REPAIR_DEFECT,
        POPUP_VIEW_REPAIRS: '#' + CTRL_IDS.POPUP_VIEW_REPAIRS,
        REOPEN_DEFECTS: '#' + CTRL_IDS.REOPEN_DEFECTS,
        DELETE_DEFECTS: '#' + CTRL_IDS.DELETE_DEFECTS,
        CHANGE_DEFECT_REASON: '#' + CTRL_IDS.CHANGE_DEFECT_REASON,
        POPUP_SHOW_NOTES: '#' + CTRL_IDS.POPUP_SHOW_NOTES,
        POPUP_REPAIR_ADVISOR: '#' + CTRL_IDS.POPUP_REPAIR_ADVISOR,
        DEFECT_GRID: '#' + CTRL_IDS.DEFECT_GRID,
        SELECTED_DEFECTS_JSON: '#' + CTRL_IDS.SELECTED_DEFECTS_JSON,
        RELOAD_DEFECT_GRID_BTN: '#' + CTRL_IDS.RELOAD_DEFECT_GRID_BTN,
        CONTAINER_NAME: '#' + CTRL_IDS.CONTAINER_NAME,
        LABEL_DICTIONARY_JSON: '#' + CTRL_IDS.LABEL_DICTIONARY_JSON,
        RESOURCENAME: '#' + CTRL_IDS.RESOURCENAME,
        EXPORT_DEFECT: '#' + CTRL_IDS.EXPORT_DEFECT,
        ADVISOR_ACTIONS: '#' + CTRL_IDS.ADVISOR_ACTIONS,
        REPAIR_FROM_ADVISOR: '#' + CTRL_IDS.REPAIR_FROM_ADVISOR,
        RESOURCE_EDIT: '#ctl00_WebPartManager_BlankWP_DropDownResource_Edit',
        RESOURCE_PANEL: '#ctl00_WebPartManager_BlankWP_DropDownResource > div#ctl00_WebPartManager_BlankWP_DropDownResource_Panl',
        RESOURCE_PANEL_VIEWER: '#ctl00_WebPartManager_BlankWP_DropDownResource > #ctl00_WebPartManager_BlankWP_DropDownResource_Panl > div.viewer',
        RESOURCE_LIST_ITEMS: '#ctl00_WebPartManager_BlankWP_DropDownResource > #ctl00_WebPartManager_BlankWP_DropDownResource_Panl > div.viewer ul > li'
    };
    Object.freeze(CTRL_SELECTORS);

    var DEFECT_STATUSES = {
        OPEN: 'OPEN',
        REPAIRED: 'REPAIRED'

    };
    Object.freeze(DEFECT_STATUSES);

    var isDefectInterface = {
        initialize: initialize,
        newDefects: newDefects,
        repairDefects: repairDefects,
        repairAdvisor: repairAdvisor,
        reopenDefects: reopenDefects,
        changeDefectsReason: changeDefectsReason,
        defectsNotes: defectsNotes,
        deleteDefects: deleteDefects,
        exportGrid: exportGrid,
        update: update,
        //getDefectDocElements: getDefectDocElements,
        repairFromAdvisor: repairFromAdvisor,
        closeFloatingFrameOverride: closeFloatingFrameOverride,
        selectResource: selectResource,
        getSelectedDefectId: getSelectedDefectId
    };

    function getDefectButtonsMarkup() {
        return '<div id="divDefectCommandBar"></div>';
    }

    // Encapsulate the toolbar
    var toolbar = (function TOOLBAR() {
        // return this interface
        var toolbarInterface = {
            defectSelectionChanged: defectSelectionChanged,
            hideDefectGridButtons: hideDefectGridButtons
        };

        // Assume no defect initially selected
        $(function () {
            defectSelectionChanged([]);
        });

        function hideDefectGridButtons() {
            $(DEFECT_BUTTON_SELECTORS.NEW).hide();
            $(DEFECT_BUTTON_SELECTORS.REPAIR).hide();
            $(DEFECT_BUTTON_SELECTORS.REPAIR_ADVISOR).hide();
            $(DEFECT_BUTTON_SELECTORS.REOPEN).hide();
            $(DEFECT_BUTTON_SELECTORS.CHANGE_REASON).hide();
            $(DEFECT_BUTTON_SELECTORS.NOTES).hide();
            $(DEFECT_BUTTON_SELECTORS.DELETE).hide();

            $(CTRL_SELECTORS.POPUP_SHOW_NOTES).hide();
            $(CTRL_SELECTORS.POPUP_REPAIR_DEFECT).hide();
            $(CTRL_SELECTORS.POPUP_REPAIR_ADVISOR).hide();
            $(CTRL_SELECTORS.SELECTED_DEFECTS_JSON).hide();
            $(CTRL_SELECTORS.CHANGE_DEFECT_REASON).hide();
            $(CTRL_SELECTORS.RESOURCENAME).hide();
            $(CTRL_SELECTORS.ADVISOR_ACTIONS).hide();
            $(CTRL_SELECTORS.REPAIR_FROM_ADVISOR).hide();
        }

        //function setDocuments(defectDocuments) {
        //    /*var labelDictionary = JSON.parse($(CTRL_SELECTORS.LABEL_DICTIONARY_JSON).val());*/
        //    documents.clear();
        //    if (defectDocuments && defectDocuments.length !== 0) {
        //        documents.addSet(new CR.DocSlideOut.DocSet("isDefect", defectDocuments));
        //    }
        //    if (defectDocuments === null) {
        //        documents = new CR.DocSlideOut.DocCollection();
        //    }
        //}

        // Interface method - tells us when the defect selections have changed.
        function defectSelectionChanged(selectedDefects) {
            //var attachDocuments = [];
            var selectionCount = 0;
            if (selectedDefects && selectedDefects.length) {
                selectionCount = selectedDefects.length;
            }

            var openCount = 0;
            var repairedCount = 0;

            selectedDefects.forEach(function (selectedDefect) {
                if (selectedDefect.isStatus.toUpperCase() === DEFECT_STATUSES.OPEN) {
                    openCount++;
                } else if (selectedDefect.isStatus.toUpperCase() === DEFECT_STATUSES.REPAIRED) {
                    repairedCount++;
                }
                //call getDefectDocuments to retrieve documents for the selected defect
            //    if (selectedDefect) {
            //        let request = {
            //            CurrentDefectId: selectedDefect.isCurrentDefectsIDString
            //        };

            //        $.ajax({
            //            type: "POST",
            //            dataType: "json",
            //            url: './isDefectService.svc/web/getDefectDocuments',
            //            headers: {
            //                'Accept': 'application/json'
            //            },
            //            contentType: "application/json;charset=UTF-8",
            //            async: false,
            //            data: JSON.stringify(request),
            //            context: document.body,
            //            success: function (data) {
            //                attachDocuments.push(data);
            //            }
            //        })
            //    }
            });

            // Hide all the buttons then show only the appropriate ones
            hideDefectGridButtons();

            // Show action menu options based on the selected defect's status
            if (selectionCount > 0) {
                // Change Reason
                $(DEFECT_BUTTON_SELECTORS.CHANGE_REASON).show();

                if (selectionCount === 1) {
                    // Notes
                    $(DEFECT_BUTTON_SELECTORS.NOTES).show();

                    //defect documents - setDocuments
                    //if (attachDocuments.length > 0)
                    //    setDocuments(attachDocuments[0].isDefectDocumentList);

                    // Repair Advisor
                    if (openCount === 1) {
                        $(DEFECT_BUTTON_SELECTORS.REPAIR_ADVISOR).show();
                    }
                }

                if (selectionCount >= 1) {
                    // Delete
                    $(DEFECT_BUTTON_SELECTORS.DELETE).show();

                    if (openCount === selectionCount) {
                        // Repair and/or No Fault Found
                        $(DEFECT_BUTTON_SELECTORS.REPAIR).show();

                    } else if (repairedCount === selectionCount) {
                        // Reopen
                        $(DEFECT_BUTTON_SELECTORS.REOPEN).show();
                    }
                //    if (selectionCount > 1) {
                //        setDocuments(null);
                //    }
                }
            }
        //    if (selectionCount === 0) {
        //        setDocuments(null);
        //    }
        }

        return toolbarInterface;
    })();

    // Selections in the defect broswer have changed
    function defectSelectionChanged(selectedDefects) {
        toolbar.defectSelectionChanged(selectedDefects);
    }

    // encapsulate the defect grid
    var defectGrid = (function DEFECT_GRID(selectionChangedCallback) {
        var defectGridInterface = {
            setup: setup,
            getSelectedDefects: getSelectedDefects
        };

        // Object wrapper around the defect grid
        var $defectGrid;

        //setup();

        // Initialization for defect grid (grid events, etc...)
        function setup() {
            $defectGrid = $(CTRL_SELECTORS.DEFECT_GRID);
            var checkBoxIDTemplate = "jqg_ctl00_WebPartManager_BlankWP_isCurrentDefectGrid_";

            // Triggered when user clicks the "Select All" checkbox in the 
            $defectGrid.bind("jqGridSelectAll", function (event, selRowIds, selected) {
                // This event seems to send all selected row IDs regardless of whether the user is selecting or un-selecting
                updateCheckBoxes(selected ? selRowIds : []);
                defectsSelected(selected ? selRowIds : []);
            });

            // listen for select row event
            $defectGrid.bind("jqGridSelectRow", function (event, rowId, originalEvent) {
                var selRowIds = $defectGrid.jqGrid("getGridParam", "selarrrow");
                updateCheckBoxes(selRowIds);
                defectsSelected(selRowIds);
            });

            function updateCheckBoxes(selRowIds) {
                var defectDisplayCount = getDefects().length;
                for (var i = 0; i < defectDisplayCount; i++) {
                    if (selRowIds.indexOf(i.toString()) != -1)
                        $('#label' + checkBoxIDTemplate + i.toLocaleString('en-US', { minimumIntegerDigits: 6, useGrouping: false })).addClass('is-grid-checkbox-checked');
                    else
                        $('#label' + checkBoxIDTemplate + i.toLocaleString('en-US', { minimumIntegerDigits: 6, useGrouping: false })).removeClass('is-grid-checkbox-checked');
                }
            }

            // Get objects for the given selected defects and trigger the selection callback
            function defectsSelected(selRowIds) {
                if (!selectionChangedCallback) {
                    return;
                }

                // build list of selected defect objects
                var selectedDefects = new Array();
                var defect;
                for (var i = 0; i < selRowIds.length; i++) {
                    defect = $defectGrid.jqGrid("getRowData", selRowIds[i]);
                    //defect = $defectGrid.jqGrid("getLocalRow", selRowIds[i]);   // gets original data instead of <td> contents so numbers are not strings.
                    if (defect.isCurrentDefectsIDString)
                        selectedDefects.push(defect);
                }

                // notify the current defect (and thus toolbar) that the defect selections have changed
                selectionChangedCallback(selectedDefects);
            }
        }

        // return all defects in the grid
        function getDefects() {
            var defectRows = $defectGrid.jqGrid("getRowData");
            var defects = defectRows.filter(function (defect) {
                return defect.isCurrentDefectsId;
            });
            return defects;
        }

        // Return all selected defects
        function getSelectedDefects() {
            // get array of selected row IDs
            var selRowIds = $defectGrid.jqGrid("getGridParam", "selarrrow");

            var selectedDefects = new Array();

            selRowIds.forEach(function (selRowId) {
                var defect = $defectGrid.jqGrid("getRowData", selRowId);
                if (defect.isCurrentDefectsIDString)
                    selectedDefects.push(defect);
            });

            return selectedDefects;
        }

        return defectGridInterface;
    })(defectSelectionChanged);

    function initialize(labels) {

        if (typeof labels !== 'undefined')
            localStorage.setItem("IndustryDefectLabels", JSON.stringify(labels));
        else
            labels = JSON.parse(localStorage.getItem("IndustryDefectLabels"));
        if (window.CloseFloatingFrame && window.CloseFloatingFrame !== isIndustryDefect.closeFloatingFrameOverride) {
            window.CloseFloatingFrame = isIndustryDefect.closeFloatingFrameOverride;
        }

        var rows = $("div#WebPart_BlankWP > div.matrix > div.row");
        $("div", rows[LAYOUT_ROWS.DEFECT_BUTTON]).append(getDefectButtonsMarkup());

        var defectTD = $('#divDefectCommandBar');
        if (labels) {
            appendDefectBarButton(labels.NewDefect, DEFECT_BUTTON_IDS.NEW, 'isIndustryDefect.newDefects()', true);
            appendDefectBarButton(labels.RepairDefect, DEFECT_BUTTON_IDS.REPAIR, 'isIndustryDefect.repairDefects()', true);
            appendDefectBarButton(labels.RepairAdvisor, DEFECT_BUTTON_IDS.REPAIR_ADVISOR, 'isIndustryDefect.repairAdvisor()', true);

            appendDefectBarButton(labels.Reopen, DEFECT_BUTTON_IDS.REOPEN, 'isIndustryDefect.reopenDefects()', true);

            appendDefectBarButton(labels.ChangeReason, DEFECT_BUTTON_IDS.CHANGE_REASON, 'isIndustryDefect.changeDefectsReason()', true);
            appendDefectBarButton(labels.Notes, DEFECT_BUTTON_IDS.NOTES, 'isIndustryDefect.defectsNotes()', true);
            appendDefectBarButton(labels.DeleteDefect, DEFECT_BUTTON_IDS.DELETE, 'isIndustryDefect.deleteDefects()', true);
            appendDefectBarButton("Export", DEFECT_BUTTON_IDS.EXPORT, 'isIndustryDefect.exportGrid()', true);

            toolbar.hideDefectGridButtons();
        }

        function appendDefectBarButton(value, id, onClickFunction, enabled) {
            var disabledClass = enabled ? "" : " button-disabled"
            defectTD.append(
                "<div class='isDefectButton" + disabledClass + "' id='" + id + "' onclick=" + onClickFunction + ">" +
                "<span class='isButtonIcon' id='" + id + "Icon' title='" + value + "'></span>" +
                "</div>");
        }

        defectGrid.setup();
    }

    function update() {
        initialize();
    }

    function newDefects() {
        $(CTRL_SELECTORS.SELECTED_DEFECTS_JSON).val();

        // Logging a new defect - just open the popup
        $(CTRL_SELECTORS.POPUP_LOG_DEFECT).click();
    }

    function repairDefects() {
        // set selected defects to JSON field to pass to repair defect popup
        var selectedDefects = defectGrid.getSelectedDefects();
        $(CTRL_SELECTORS.SELECTED_DEFECTS_JSON).val(JSON.stringify(selectedDefects));
        $(CTRL_SELECTORS.POPUP_REPAIR_DEFECT).click();
    }

    function repairAdvisor() {
        // set selected defects to JSON field to pass to repair advisor popup
        var selectedDefects = defectGrid.getSelectedDefects();
        $(CTRL_SELECTORS.SELECTED_DEFECTS_JSON).val(JSON.stringify(selectedDefects));
        $(CTRL_SELECTORS.POPUP_REPAIR_ADVISOR).click();
    }

    // popup repair dlg after selection repair actions from advisor dlg.
    function repairFromAdvisor() {
        $(CTRL_SELECTORS.REPAIR_FROM_ADVISOR).click();
    }

    //function getDocIconPath(fileType) {
    //    var theme = "Camstar";
    //    var extension = ".png";
    //    if ($("body").hasClass("Horizon-theme")) {
    //        theme = "Horizon";
    //        extension = ".svg";
    //    }

    //    var path = "Themes/" + theme + "/images/icons/";
    //    var icon;
    //    switch (fileType.toLowerCase()) {
    //        case ".bmp":
    //            icon = "typeBMP48"; break;
    //        case ".rtf":
    //            icon = "typeRTF48"; break;
    //        case ".pdf":
    //            icon = "icon-pdf-32x32"; break;
    //        case ".docx":
    //        case ".doc":
    //            icon = "icon-word-32x32"; break;
    //        case ".xml":
    //            icon = "typeXml48"; break;
    //        case ".exe":
    //            icon = "typeExe48"; break;
    //        case ".exel":
    //            icon = "typeMsExcel48"; break;
    //        case ".gif":
    //            icon = "typeGif48"; break;
    //        case ".jpg":
    //        case ".jpeg":
    //            icon = "icon-jpg-32x32"; break;
    //        case ".txt":
    //            icon = "typeTxt48"; break;
    //        case ".zip":
    //            icon = "typeZipFile48"; break;
    //        case ".png":
    //            icon = "typePng48"; break;
    //        case ".pptx":
    //        case ".pptm":
    //        case ".ppt":
    //            icon = "typeMsPowerpoint48"; break;
    //        case ".html":
    //        case ".htm":
    //            icon = "HTML_32_32"; break;

    //        case ".jt":
    //            switch (theme.toLowerCase()) {
    //                case "camstar":
    //                    icon = "3D_32_32"; break;
    //                case "horizon":
    //                    icon = "Type3D48"; break;
    //            }
    //            break;
    //        case "url":
    //            icon = "HTML_32_32"; break;
    //        default:
    //            icon = "icon-unknown-32x32"; break;
    //    }
    //    path += icon + extension;

    //    return path;
    //}

    // if only one defect is selected, return its ID.  null otherwise
    function getSelectedDefectId() {
        var selectedDefects = defectGrid.getSelectedDefects();

        return (selectedDefects && selectedDefects.length === 1) ? selectedDefects[0].isCurrentDefectsIDString : null;
    }


    // return - array of jQuery objects - one for each document for the selected defect and one for the header
    //function getDefectDocElements() {
    //    if (!documents)
    //        return null;

    //    var defectDocElements = [];

    //    var docSets = documents["sets"];

    //    Array.forEach(docSets, function (docSet) {
    //        // Document Set name
    //        var $hdr = $("<div class='header-content-row cs-docset'></div>");
    //        $hdr.text(docSet.name);
    //        defectDocElements.push($hdr);

    //        // Doc entries
    //        Array.forEach(docSet.entries, function (de) {
    //            var $entry = $(
    //                "<div class='content-row doc-entry'>" +
    //                "<img class=icon></img>" +
    //                "<div class=info><div class=name></div><div class=desc></div></div>" +
    //                "</div>");
    //            $entry.attr("browse-mode", de.BrowseMode);
    //            $entry.prop("title", de.Description);
    //            $('.icon', $entry).prop("src", getDocIconPath(de.FileExtension));
    //            $('.name', $entry).text(de.Name);
    //            $('.desc', $entry).text(de.Description);
    //            $entry.click(function () {
    //                //  Open document in new tab
    //                CallServer(JSON.stringify({
    //                    fun: 'openDocument',
    //                    serverType: "Camstar.WebPortal.Helpers.isContainerStatusInquiry",
    //                    clientType: "ContainerStatusWebPart_Deco",
    //                    documentName: de.Name,
    //                    documentRev: de.Revision
    //                }), null);
    //            });
    //            defectDocElements.push($entry);
    //        });
    //    });
    //    return defectDocElements;
    //}

    function reopenDefects() {
        var selectedDefects = defectGrid.getSelectedDefects();
        isReopenDefects(defectActionComplete, null, selectedDefects);
    }

    function isReopenDefects(successCallback, failCallback, selectedDefects) {

        successCallback = successCallback;
        failCallback = failCallback;

        var request = new DefectActionRequest(selectedDefects);
        let getReOpenParams = {
            requestOpen: request,
            requestContainer: $(CTRL_SELECTORS.CONTAINER_NAME).val()
        };

        $.ajax({
            type: "POST",
            dataType: "json",
            url: './isDefectService.svc/web/setReOpenDefect',
            headers: {
                'Accept': 'application/json'
            },
            contentType: "application/json;charset=UTF-8",
            async: true,
            data: JSON.stringify(getReOpenParams),
            context: document.body
        })
            .success(successCallback)
            .fail(failCallback);
    }

    function changeDefectsReason() {
        var selectedDefects = defectGrid.getSelectedDefects();
        $(CTRL_SELECTORS.SELECTED_DEFECTS_JSON).val(JSON.stringify(selectedDefects));
        $(CTRL_SELECTORS.CHANGE_DEFECT_REASON).click();
    }

    $(document).ready(function () {

        var popupFrame = $("#ctl00_FloatingFrame")
        var maximizeButton = popupFrame.find("#ctl00_MaximizeLink");

        $(maximizeButton).click(function () {
            if ($(popupFrame.html()).find("span").text() == "Notes") {
                if (maximizeButton.hasClass("ui-floatingframe-max")) {
                    adjustNotePopupUI("#ctl00_FloatingFrame > div > div:eq(1)", 0);
                }
                else if (maximizeButton.hasClass("ui-floatingframe-normalize")) {
                    adjustNotePopupUI("#ctl00_FloatingFrame > div > div:eq(1)", 100);
                }
            }
        });

        $('#FloatingFrame_frame').on('load', function () {
            $('#FloatingFrame_frame').contents().find('body').on('click', 'a', function () {
                if (maximizeButton.hasClass("ui-floatingframe-normalize")) {
                    $("#FloatingFrame_frame").ready(function () {
                        adjustNotePopupUI("#ctl00_FloatingFrame > div > div:eq(1)", 100)
                    });
                }
            });
        });

    });


    function adjustNotePopupUI(widthSelector, width) {
        $("#FloatingFrame_frame").ready(function () {
            setTimeout(function () {
                var Frame = $("#FloatingFrame_frame");
                var WebPart_NotesWP = Frame.contents().find("#WebPart_NotesWP");
                var InspectNoteTextBox = Frame.contents().find("#ctl00_WebPartManager_InspectNoteWP_InspectNoteTextBox_ctl00");
                var RepairNoteTextBox = Frame.contents().find("#ctl00_WebPartManager_RepairNoteWP_RepairNoteTextBox_ctl00");

                $(WebPart_NotesWP).css('width', ($(widthSelector).width() * 0.9) + width);
                $(InspectNoteTextBox).css('width', ($(widthSelector).width() * 0.8) + width);
                $(RepairNoteTextBox).css('width', ($(widthSelector).width() * 0.8) + width);
            }, 10);
        });
    }

    function defectsNotes() {
        var selectedDefects = defectGrid.getSelectedDefects();
        $(CTRL_SELECTORS.SELECTED_DEFECTS_JSON).val(JSON.stringify(selectedDefects));
        $(CTRL_SELECTORS.POPUP_SHOW_NOTES).click();
    }

    function exportGrid() {
        setTimeout(function () {
            $(CTRL_SELECTORS.EXPORT_DEFECT).trigger('click');
            setTimeout(function () { positionExportFlyout(CTRL_SELECTORS.EXPORT_DEFECT); }, 0);
        }, 0);
    }

    // want the export flyout to appear just under the button that launched it
    function positionExportFlyout(buttonSelector) {
        var buttonOffset = $(buttonSelector).offset();
        var flyoutOffset = buttonOffset ? { top: buttonOffset.top + 50, left: buttonOffset.left + 4 } : { top: 500, left: 100 };
        $('.flyout.grid-flyout').offset(flyoutOffset);
    }

    // Delete selected defect(s)
    function deleteDefects() {
        var labelDictionary = JSON.parse($(CTRL_SELECTORS.LABEL_DICTIONARY_JSON).val());
        // Have user confirm they want to delete
        jConfirm(labelDictionary.AlertConfirmDelete, labelDictionary.UIInSiteConfirmDeleteDlgTitle, function (deleteConfirmed) {
            if (deleteConfirmed) {
                var selectedDefects = defectGrid.getSelectedDefects();
                isDeleteDefects(defectActionComplete, null, selectedDefects);
            }
        });
    }

    function isDeleteDefects(successCallback, failCallback, selectedDefects) {

        successCallback = successCallback;
        failCallback = failCallback;

        var request = new DefectActionRequest(selectedDefects);
        let getDeleteParams = {
            requestDelete: request,
            requestContainer: $(CTRL_SELECTORS.CONTAINER_NAME).val()
        };

        $.ajax({
            type: "POST",
            dataType: "json",
            url: './isDefectService.svc/web/setDeleteDefect',
            headers: {
                'Accept': 'application/json'
            },
            contentType: "application/json;charset=UTF-8",
            async: true,
            data: JSON.stringify(getDeleteParams),
            context: document.body
        })
        .success(successCallback)
        .fail(failCallback);
    }

    function DefectActionRequest(defects) {
        this.Defects = defects;
    }

    function defectActionComplete(data) {
        var dataResult = null;

        if (data.setDeleteDefectResult)
            dataResult = data.setDeleteDefectResult;
        if (data.setReOpenDefectResult)
            dataResult = data.setReOpenDefectResult;

        if (dataResult.Success) {
            __page.displayStatus(dataResult.Message, "Success");
            $(CTRL_SELECTORS.RELOAD_DEFECT_GRID_BTN).click();
        }
        else {
            __page.displayStatus(dataResult.Exception, "Error");
        }
    }

    // Override the Camstar version of this function
    function closeFloatingFrameOverride(notifyParent, keepParentTabOpen) {
        var iFrameVPName = pop.GetIFrameVirtualPage();

        pop.hide();
        if (notifyParent) {
            if (iFrameVPName === "isRepairAdvisor_VP") {
                setTimeout(function () { isIndustryDefect.repairFromAdvisor(); }, 0);
            } else if (iFrameVPName === "isRepairDefectPopUp_VP" || iFrameVPName === "isLogDefectPopUp_VP") {
                $(CTRL_SELECTORS.RELOAD_DEFECT_GRID_BTN).click();
            }
        }
    }

    // Invoked from codebehind.  Wait for the Resource dropdown to be populated and then select a Resource
    function selectResource(firstChoiceResourceName, secondChoiceResourceName) {
        let resourcePanelViewerObserver = new MutationObserver(function (mutations, obs) {

            if (mutations && mutations.length > 1) {
                // figure out if we are modifying the Resource dropdown panel and are we removing and adding nodes (updating the Resources)
                let resourcePanel = document.querySelector(CTRL_SELECTORS.RESOURCE_PANEL);
                let panelModified = mutations[0].target?.parentElement === resourcePanel && mutations[1].target?.parentElement === resourcePanel;
                let nodesRemoved = mutations[0].removedNodes?.length > 0;
                let nodesAdded = mutations[1].addedNodes?.length > 0;

                if (panelModified && nodesRemoved && nodesAdded) {
                    let ddResourceNames = $(CTRL_SELECTORS.RESOURCE_LIST_ITEMS).map(function () { return $(this).attr('title') }).get();

                    // select desired Resource (if it's available in the list)
                    if (ddResourceNames.includes(firstChoiceResourceName)) {
                        $(CTRL_SELECTORS.RESOURCE_EDIT).val(firstChoiceResourceName);
                    } else if (ddResourceNames.includes(secondChoiceResourceName)) {
                        $(CTRL_SELECTORS.RESOURCE_EDIT).val(secondChoiceResourceName);
                    }

                    // found what we're waiting for, so no need to keep observing
                    obs.disconnect();
                }
            }
        });

        // watch for the Resources in the dropdown to be updated
        resourcePanelViewerObserver.observe($(CTRL_SELECTORS.RESOURCE_PANEL_VIEWER)[0], {
            childList: true,
            subtree: true
        });
    }
    return isDefectInterface;
})();