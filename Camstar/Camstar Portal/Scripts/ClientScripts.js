// Copyright Siemens 2025 
var Camstar = Camstar || { WebPortal: Camstar.WebPortal || {} };


///ESigCaptureVP.xml
function AdjustESigCaptureGrid() {
    OpenESigContainerGrid();
    HighlighFilledEsigRequirement.call(this);
    var grid = $(this.GridID);
    var hoverRows = grid.getGridParam("hoverrows"); //detects wether row can be selected
    if (hoverRows) {
        //grid in individual mode
        DisableSelection.call(this);
        HighlightCurrentAndSelectNextRow.call(this);
    }
    else {
        //grid in batch mode
        HighlightAddedCaptures.call(this);
    }
    if (Camstars.Browser.IE) {
        var me = this;
        setTimeout(function () {
            $.jgrid.wrapCells(me);
        }, 0);
    }
}

function HighlighFilledEsigRequirement() {
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var highlightClass = "collected-signatures";
    $('tr.jqgroup', grid).addClass(highlightClass);
    $(rowData).each(function () {
        //remove group highlighting if child row is not valid
        if (this['IsValid'] === "False") {
            var row = $('#' + this['_id_column'], grid);
            while (row) {
                if (row.hasClass('jqgroup')) {
                    row.removeClass(highlightClass);
                    return;
                }
                row = row.prev();
            }
        }
    }
    );
}

function HighlightCurrentAndSelectNextRow() {
    var grid = $(this.GridID);
    var plusicon = grid[0].p.groupingView.plusicon;
    var selectedRowID = grid.getGridParam("selrow");
    //open the first group
    if (selectedRowID == null)
        return;
    var rowData = grid.getRowData(selectedRowID);
    var selectedRow = $('#' + selectedRowID, grid);
    if (rowData["IsValid"] === "True") {
        $('.highlighted-row').removeClass('highlighted-row');
        selectedRow.addClass('highlighted-row');
        selectedRow = selectedRow.nextAll('.jqgrow:first');
        //set the next row selected
        grid.jqGrid("setSelection", selectedRow.attr('id'), true);
    }
    //expand group if it's collapsed
    var group = selectedRow.prevAll('.jqgroup:first');
    $('.' + plusicon, $(group)).click();
}

function HighlightAddedCaptures() {
    var grid = $(this.GridID);
    $('.ui-state-highlight', grid).removeClass('ui-state-highlight');
    $('tr[aria-selected="true"]').addClass('highlighted-row');
}

function ViewESigContainers(eSigDetailID) {
    __page.checkedESigDetailID = eSigDetailID;
    setTimeout(__doPostBack('ESigClick', eSigDetailID), 0);
}

function OpenESigContainerGrid() {
    if (window['__page'] === undefined) return;
    if (__page.checkedESigDetailID === undefined) return;
    var eSigDetailID = __page.checkedESigDetailID;
    var pos = $('#' + eSigDetailID).offset();
    var containerGrid = $('.ui-jqgrid:first');
    var hideFunction = function () { containerGrid.hide(); };
    pos.top += 14;
    pos.left = 60;
    containerGrid
        .offset(pos)
        .show()
        .one('mouseleave', hideFunction);
    $(document).one('mousedown', hideFunction);
    __page.checkedESigDetailID = undefined;
}


function DisableSelection() {
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    $(rowData).each(function () {
        if (this["IsValid"] === "True") {
            //prevent selection of valid rows

            $('#' + this['_id_column']).off();

            $('#' + this['_id_column']).click(function (e) {
                var $t = $(e.target);
                if ($t.is('[chevron]')) {
                    $.jgrid.clickOnChevron(e);
                }
                e.stopPropagation();
            });
        }
    });
}

function QtyToCombineValidation(obj1) {
    var textbox = $('input[name="' + obj1.target.name + '"]');
    var oldValue = textbox.parents("td").first().prev().text();
    var grid = $('table[id$=ctl00_WebPartManager_CombineWP_EligibleContainersGrid]');
    var rowData = grid.getRowData();
    if (oldValue == "")
        for (var i = 0; i < rowData.length; i++) {
            var record = rowData[i];
            if (record._id_column == window.idrow) {
                oldValue = record.Qty;
                break;
            }
        }
    var newValue = textbox.val();
    if (Number.parseLocale(oldValue) < Number.parseLocale(newValue))
        setTimeout(function () { jAlert($(".ui-validation-error").text()); }, 100);
}

function StringFormat() {
    var s = arguments[0];
    for (var i = 0; i < arguments.length - 1; i++) {
        var reg = new RegExp("\\{" + i + "\\}", "gm");
        s = s.replace(reg, arguments[i + 1]);
    }

    return s;

}

function MultiSelectItemsChanged() {
    var textBox = $get(this.GridID.substring(1, this.GridID.lastIndexOf("_Panl")) + "_Edit");;
    if (textBox) {
        var displayText = textBox.getAttribute("displayValueText");
        if (!displayText)
            displayText = "Selected {0} of {1}";
        if (typeof (this._totalSelectedRows) != "undefined" && typeof (this._totalRows) != "undefined") {
            if (this._totalSelectedRows > 0)
                textBox.value = StringFormat(displayText, this._totalSelectedRows, this._totalRows);
            else
                textBox.value = "";
            $(textBox).change();
        }
        else
            textBox.value = "";
    }
}

function ContainerAttrMaint_DataType_Changed(obj1, obj2, obj3) {

    var isExpressionCheckBox = $(obj1.target).parents("tr").first().find('input[name=IsExpression]');
    isExpressionCheckBox.attr('disabled', 'true');

    var dataTypeValue = $get(obj1.target.id.substring(0, obj1.target.id.lastIndexOf("_Edit")) + "_Value").value;

    isExpressionCheckBox.attr('disabled', (obj1.target.value !== "String" && dataTypeValue != 4));
}

function ContainerAttrMaint_renderCompleted(isAtTheEndOfProcessing) {
    var label = $("#ctl00_WebPartManager_UserAttributeWP_WarningMessage");
    label.hide();
    var message = "";
    if (typeof label[0] !== "undefined")
        message = label[0].textContent;

    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var type = [];
    var li = $(".jstree-leaf", "#scrollablepanel").not(".jstree-loading");
    li.each(function () {
        type += this.textContent.trim() + ",";
    });
    for (var i = 0; i < rowData.length; i++) {
        var record = rowData[i];
        var currentRow = $('tr[id="' + record._id_column + '"]', grid);
        var cells = $('td', currentRow);
        //when the row is ediable, getRowData returns html instead of values
        if (currentRow.attr('editable') == '1')
            continue;
        var status = record['DataType'];
        var pos = type.indexOf(status);
        if (type.length != 0 && pos == -1) {
            cells[3].title = "";
            cells[3].textContent = "";
            this.showMessage(message);
        }

        if (status != 'String')
            currentRow.find('input').last().prop('checked', false);

        currentRow.find('input').last().prop('disabled', !(status == 'String'));
    }
}

function ContainerAttrMaint_editingStarted(isAtTheEndOfProcessing, rowid) {
    var currentRow = $('tr[id="' + rowid + '"]', $(this.GridID));
    if (currentRow.length) {
        var status = $('input[id$="DataType_InlineEditorControl_Edit"]', currentRow).val();
        if (status != 'String')
            currentRow.find('input').last().prop('checked', false);

        currentRow.find('input').last().prop('disabled', !(status == 'String'));
    }
}

function DownloadAttachment(attachmentGridId, attachId) {
    var grid = $("#" + attachmentGridId);
    var selectedRow = $("tr[aria-selected=true]", grid);
    if (selectedRow.length == 0)
        return;
    var cells = $('td', selectedRow);
    if (cells.length > 2) {
        var attachmentID = $("input[id*='Attachments_ParentID_Value']");
        if (attachmentID.length != 0)
            attachId = attachmentID.val();
        var title = $(cells[1]).attr('title');
        var version = $(cells[2]).attr('title');
        StartDownloadFile(title, version, attachId);
    }
}

function DisableViewDetailsSearchButton() {
    var viewSearchButton = $("input[id*='ViewSearchDetailsButton']");
    if (viewSearchButton.length == 1) {
        if (this._totalSelectedRows != 1)
            viewSearchButton.attr("disabled", true);
        else
            viewSearchButton.removeAttr("disabled");

    }

    var addReferenceButton = $("input[id*='AddReferenceButton']");
    if (addReferenceButton.length == 1) {
        if (this._totalSelectedRows == 0)
            addReferenceButton.attr("disabled", true);
        else
            addReferenceButton.removeAttr("disabled");

    }

}

function DisableViewDetailsXrefButton() {
    var xrefsGrid = $(this.GridID);
    var selectedRows = $("tr[aria-selected=true]", xrefsGrid);
    var viewXRefsButton = $("input[id*='ViewXRefsDetailsButton']");
    if (xrefsGrid.length == 1 && viewXRefsButton.length == 1) {
        if (selectedRows.length != 1)
            viewXRefsButton.attr("disabled", true);
        else
            viewXRefsButton.removeAttr("disabled");

    }
}

function CompIssue_renderCompleted() {
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var qtyRequired, qtyIssued;
    var issueStatusCell, qtyIssuedCell;
    for (var x = 0; x < rowData.length; x++) {
        if (rowData[x].QtyIssued != "" & rowData[x].QtyRequired != "" & rowData[x].QtyIssued != "---") {

            qtyIssued = Number.parseLocale(rowData[x].QtyIssued == "" ? 0 : rowData[x].QtyIssued);
            qtyRequired = Number.parseLocale(rowData[x].QtyRequired == "" ? 0 : rowData[x].QtyRequired);

            issueStatusCell = $(".issue-status", grid)[x];
            qtyIssuedCell = $(".qty-issued", grid)[x];
            $(qtyIssuedCell).css("background-position", "right");
            if (qtyIssued < qtyRequired) {
                $(issueStatusCell).addClass("under-satisfied");
                $(qtyIssuedCell).addClass("under-satisfied");
            }
            if (qtyIssued > qtyRequired) {
                $(issueStatusCell).addClass("over-satisfied");
                $(qtyIssuedCell).addClass("over-satisfied");
            }
            if (qtyIssued == qtyRequired) {
                $(issueStatusCell).addClass("satisfied");
                $(qtyIssuedCell).addClass("satisfied");
            }
        }
    }

    var issueControlField = $("#ctl00_WebPartManager_MaterialsRequirementWP_ComponentIssueUDA_IssueControl_Edit");
    issueControlField.attr("title", issueControlField.val());
}

function CompReplace_renderCompleted() {
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var qtyReplaced, qtyIssued;
    var issueStatusCell, qtyIssuedCell;
    var submitBtn = $("[id*='ReplaceAction']");
    submitBtn.attr("disabled", "disabled");
    var isCorrectRowsExists = false;
    var isIncorrectRowsExists = false;
    for (var x = 0; x < rowData.length; x++) {

        if (rowData[x].QtyIssued != "" & rowData[x].QtyRequired != "" & rowData[x].QtyIssued != "---") {
            qtyIssued = Number.parseLocale(rowData[x].QtyIssued == "" ? 0 : rowData[x].QtyIssued);
            qtyReplaced = Number.parseLocale(rowData[x].QtyReplaced == "" ? 0 : rowData[x].QtyReplaced);
            issueStatusCell = $(".issue-status", grid)[x];
            qtyIssuedCell = $(".qty-issued", grid)[x];
            $(qtyIssuedCell).css("background-position", "right");
            if (qtyIssued > qtyReplaced) {
                $(issueStatusCell).addClass("under-satisfied");
                $(qtyIssuedCell).addClass("under-satisfied");
            }
            if (qtyIssued < qtyReplaced) {
                $(issueStatusCell).addClass("over-satisfied");
                $(qtyIssuedCell).addClass("over-satisfied");
            }
            if (qtyIssued === qtyReplaced) {
                $(issueStatusCell).addClass("satisfied");
                $(qtyIssuedCell).addClass("satisfied");
                isCorrectRowsExists = true;
            } else if (qtyReplaced !== 0) {
                isIncorrectRowsExists = true;
            }
        }
    }

    if (isCorrectRowsExists & !isIncorrectRowsExists) {
        submitBtn.removeAttr("disabled");
    }
}

function MultiMoveNonStd_renderCompleted() {
    var grid = $(this.GridID);
    var rowData = grid.getRowData();

    var selectedRows = grid.getGridParam("selarrrow");
    if (selectedRows.length > 0) {
        var isFound = false;
        var initialRowData = grid.getRowData(selectedRows[0]);
        var initialRowElement = $('[id=' + selectedRows[0] + ']', grid);

        for (var i = 0; i < selectedRows.length; i++) {
            var rowData = grid.getRowData(selectedRows[i]);
            if (initialRowData.Resource != rowData.Resource) {
                isFound = true;
                break;
            }
        }

        var resourceElement = $find("ctl00_WebPartManager_MultiContainerMoveNonStdWP_MoveNonStd_Resource");
        if (resourceElement != null) {
            resourceElement.setValue({ key: initialRowData.Resource, text: initialRowData.Resource });
            resourceElement.set_Hidden(isFound);
        }
    }
    else {
        var resourceElement = $find("ctl00_WebPartManager_MultiContainerMoveNonStdWP_MoveNonStd_Resource");
        if (resourceElement != null) {
            resourceElement.clearValue();
            resourceElement.set_Hidden(false);
        }
    }
	var rowIds = grid.getDataIDs().filter((id) => !id.includes('empty'));;
	for (var i = 0; i < rowIds.length; i++) {
		var rowData = grid.getRowData(rowIds[i]);
		if(rowData.InRework == "False" || rowData.Status.includes("Passed"))
		{
			var resetReworkStatusElement = $('#ctl00_WebPartManager_MultiContainerMoveNonStdWP_MultiContainer_Containers_ResetReworkStatus_' + rowIds[i]+'_dcb');
			var resetReworkStatusTD = resetReworkStatusElement.closest('td');
			resetReworkStatusElement.attr('disabled', 'disabled');
			resetReworkStatusTD.attr('disabled', 'disabled');
			resetReworkStatusElement.click(function (e) {
				e.stopPropagation();
			});
		}
	}
}

// Click on the check box 
function AssignApproval_EditEntry_click(e) {
    //AssignApproval_SetReadOnly(null);
}

function AssignApproval_EditorOption_change(e) {
    //AssignApproval_SetReadOnly(null);
}

function AssignApproval_editingStarted(isEndOfOperation, rowid) {
    AssignApproval_SetReadOnly(rowid, this);

    var that = this;
    setTimeout(function () { $(that.GridID).find('#' + rowid).find(':input:enabled:visible:first').focus() }, 100);
}

function AssignApproval_SetReadOnly(editingRowID, grid) {
    var editOptionVal = null;
    var isEntryRequired = null;

    var dropDownEditOption = grid.GetInlineComponent("EditOption");
    if (dropDownEditOption) {
        editOptionVal = dropDownEditOption.getValue();
    }

    var checkboxEntryReq = $('#' + editingRowID + '_EntryRequired');
    if (checkboxEntryReq.length > 0) {
        isEntryRequired = checkboxEntryReq[0].checked;
    }

    var substituteRO = !isEntryRequired && editOptionVal == '3';
    var roleRO = (editOptionVal == '3' || editOptionVal == '2');
    var nameRO = (editOptionVal == '3');
    var levelRO = isEntryRequired && (editOptionVal == '3' || editOptionVal == '1' || editOptionVal == '2');

    var dropDownSubstituteOption = grid.GetInlineComponent("SubstituteOption");
    if (dropDownSubstituteOption) {
        var prm = { PropertyKey: eval(Camstar.Ajax.DirectUpdateParameterKeys.Enable), PropertyValue: substituteRO ? 'False' : 'True' };
        dropDownSubstituteOption.directUpdate(prm);
    }

    var ndoRole = grid.GetInlineComponent("ApproverRole");
    if (ndoRole) {
        var prm = { PropertyKey: eval(Camstar.Ajax.DirectUpdateParameterKeys.Enable), PropertyValue: roleRO ? 'False' : 'True' };
        ndoRole.directUpdate(prm);
    }

    var ndoName = grid.GetInlineComponent("Approver");
    if (ndoName) {
        var prm = { PropertyKey: eval(Camstar.Ajax.DirectUpdateParameterKeys.Enable), PropertyValue: nameRO ? 'False' : 'True' };
        ndoName.directUpdate(prm);
    }

    var textBoxLevel = grid.GetInlineComponent("SheetLevel");
    if (textBoxLevel) {
        var prm = { PropertyKey: eval(Camstar.Ajax.DirectUpdateParameterKeys.Enable), PropertyValue: levelRO ? 'False' : 'True' };
        textBoxLevel.directUpdate(prm);
    }

}

/* E-Proceedure Task List Summary */
function eProcTaskListSummary_renderComplete(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{
    //get/set row and header detail
    var theGrid = jQuery(this.GridID);
    var grpCount = -1;

    var rowHeader = "";
    var me = this;

    $(theGrid.getRowData()).each(function () {
        //formatGroupHeader
        if (this.TaskListName != undefined && this.TaskListName != rowHeader) {
            //reset rowHeader & update grpCount
            rowHeader = this.TaskListName;
            grpCount++;

            var trHeader = $(theGrid.find("tr.jqgroup")).filter(function () { return $(this).is('[id="' + me._gridID + 'ghead_0_' + grpCount.toString() + '"]'); }).first();

            trHeader.children('td:first').contents().filter(function () { return this.nodeType == 3 /*TextNode*/; }).remove();

            //strip out any html characters from the task list instruction field if present
            //div is a fake element
            var instructionText = $('<div />').html(this.TaskListInstruction).text();
            var instructionFormat = $("[id$=Lbl_EprocTaskListSummaryInstruction]").html();

            //strip out any html characters from the task list instruction field if present
            try { instructionText = $(instructionText).text() == "" ? instructionText : $(instructionText).text(); } catch (err) { }

            var newContent = $.jgrid.format(
                '<div class="jqgrid-custom-group-header wrapper " keep="">' +
                instructionFormat + '</div>',
                this.TaskListName, this.ExecutionModeText, this.TasksCompleted, this.TaskCount,
                this.RequiredTasksCompleted == '' ? 0 : this.RequiredTasksCompleted,
                this.RequiredTaskCount, '<span class="instructionTextClass" title =' + '"' + instructionText + '">' + instructionText + '</span>'
            );

            var $td = $(trHeader.children('td').first());
            $td.append($(newContent));

            var leftWidth = $(".ui-icon-circlesmall-plus,.ui-icon-circlesmall-minus", $td).outerWidth() +
                $(">div >b", $td).outerWidth() + 8;

            $(".instructionTextClass", $td).css("width", "calc( 100% - " + leftWidth + "px )").css("white-space", "normal");

            trHeader.children('td').each($.jgrid.cutOverflowText);
            if ($("[chevron]", $td).length) {
                $td.click($.jgrid.clickOnChevron);
            }
        }

        //update row formatting
        var trCurrent = $(theGrid).find(".jqgrow td[title='" + this._id_column + "']:first").parent("tr:first");
        var tdClass;
        var spanClass;
        var count;
        var status = this.TaskStatusText;
        var labels = [{ Name: 'Lbl_Passed' }, { Name: 'TaskStatusEnum_NotExecuted' }, { Name: 'TaskStatusEnum_Fail' }, { Name: 'TaskStatusEnum_Pass' }];
        __page.getLabels(labels, function (resp) {
            if ($.isArray(resp)) {
                var cssStatusValue;
                $.each(resp, function () {
                    var labelName = this.Name;
                    var labelText = this.Value;
                    switch (labelName) {
                        case 'TaskStatusEnum_Pass':
                            if (status == labelText) {
                                cssStatusValue = 'pass';
                                break;
                            };
                        case 'Lbl_Passed':
                            if (status == labelText) {
                                cssStatusValue = 'pass';
                                break;
                            };
                        case 'TaskStatusEnum_NotExecuted':
                            if (status == labelText) {
                                cssStatusValue = 'pending';
                                break;
                            };
                        case 'TaskStatusEnum_Fail':
                            if (status == labelText) {
                                cssStatusValue = 'fail';
                                break;
                            };
                        default:
                            break;
                    }
                });
                //update status
                spanClass = "ui-icon-status-" + cssStatusValue;
                $(trCurrent).find("td[aria-describedby$='_TaskStatusText']:first")
                    .addClass(spanClass)
                    .attr("title", status);

                var $cellWithInstruction = $(trCurrent).find("td[aria-describedby$='_TaskInstruction']").first();
                $cellWithInstruction.html($cellWithInstruction.text());
                $cellWithInstruction.css("white-space", "normal");
            }
        });
        var processed = this.ProcessedCount == "" ? "0" : this.ProcessedCount;
        //update count
        count = processed + " (Min " + this["MinIterations"] + " | Max " + (this["MaxIterations"] != "" ? this["MaxIterations"] : "0") + ")";
        tdClass = (Number(processed) >= Number(this["MinIterations"])) ? "ui-status-pass" : ""; //only highlight completed counts
        $(trCurrent).find("td[aria-describedby$='_Count']:first")
            .data("val", count)
            .html(count)
            .addClass(tdClass)
            .attr("title", count);
    });
}

//OBSOLETE:  SearchLayout_AddSlideoutToogler
function SearchPanelSlideOut_renderComplete(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{
    // Add funnel for the horizon theme
    if (this._isHorizonStyle) {

        var $tdFirst = $(this.PagerID + " table.navtable tr td:first-child");
        if ($tdFirst.length) {
            var $funnel = $("<td class='ui-pg-button ui-corner-all'><div class=ui-pg-div></div><span class='ui-icon funnel-icon'></span></td>");
            $funnel.prop("title", "Filter").prop("id", "funnel_" + this._gridID).prop("class", "funnel_1_1");
            $funnel.insertBefore($tdFirst);
            $funnel = $(this.PagerID + " table.navtable tr td:first-child");
            $funnel.click(function () {

                $(".close-button-desktop").click();

                return false;
            });
        }
    }
}

//OBSOLETE:  SearchLayout_AddSlideoutToogler
function SearchPanelSlideOut_AddToggleSearchAttr(isPostBack, isSearch, isTabSwitched, isUserVal) {
    if ($(".container-search-panel").find(".close-button-desktop").length == 0) {
        var $btn = $("<span class='close-button-desktop'></span>");
        $(".container-search-panel").find(".header-search-panel").append($btn);

        $btn.click(function () {
            $(".webpart.container-search-panel").toggle('slide', 500,
                function () {
                    var $funnel = $(".funnel_1_1");
                    $funnel.parent().find('span').toggleClass("funnel-icon").toggleClass("funnel-icon-close");
                    $(this).closest(".cell-m").toggleClass("collapsed", $(this).is(":hidden"));
                    top_resize();
                });
        });
    }
}

/* Container Search */
// update __page property for translated labels
function GetContainerSearchLabels() {
    setTimeout(function () {
        if (typeof (__page) !== 'undefined' && !__page._pageLabels.length) {
            var labels = [{ Name: 'Container_CurrentStatus' }, { Name: 'Lbl_History' }, { Name: 'QueryType_User' }];
            __page.getLabels(labels, function (response) {
                if ($.isArray(response)) {
                    $.each(response, function () {
                        var labelname = this.Name;
                        var labeltext = this.Value.replace(' ', '');
                        __page._pageLabels.push({ Name: labelname, Value: labeltext });
                    });
                }
                else {
                    alert(response.Error);
                }
            });
        }
    }, 500);
}

function SearchLayout_AddSlideoutToogler(gridID) {
    let $wpSearch = $(".common-search-panel");
    let showFunnel = function (isClick) {
        $(".global-funnel").remove();
        if ($wpSearch.css("display") != "none") {
            let $grid = $("#gbox_" + gridID);
            if ($grid.css("display") != "none") {
                let $tdFirst = $("#gview_" + gridID + " table.navtable tr td:first-child");
                let funnelId = "funnel_" + gridID;
                if ($tdFirst.length && $tdFirst[0].id !== funnelId) {
                    let $funnel = $("<td class='ui-pg-button ui-corner-all'><div class=ui-pg-div></div><span class='ui-icon funnel-icon'></span></td>");
                    $funnel.prop("title", "Filter").prop("id", funnelId);
                    $funnel.insertBefore($tdFirst);
                    $funnel.click(function () {
                        $(".close-button-desktop").click();
                    });
                }
            }
            else if ($(".global-funnel").length === 0 && isClick) {
                let $closeBtn = $("<div class='global-funnel'><span class='ui-icon funnel-icon'></span></div>")
                $closeBtn.insertBefore($wpSearch.closest(".form-container>div"));
                $closeBtn.click(function () {
                    $(".close-button-desktop").click();
                });
            }
        }
    };

    setTimeout(function () { showFunnel(false); }, 500);

    let $btn = $wpSearch.find(".close-button-desktop");

    if ($btn.length == 0) {
        $btn = $("<span class='close-button-desktop'></span>");
        $wpSearch.find(".header-search-panel").append($btn);

        $btn.click(function () {
            showFunnel(true);

            $wpSearch.toggle('slide', 500,
                function () {
                    let $pageContainer = $wpSearch.closest(".page-container");
                    if ($wpSearch.css("display") == "none") {
                        $pageContainer.addClass("search-panel-hidden");
                    }
                    else {
                        $(".global-funnel").remove();
                        $pageContainer.removeClass("search-panel-hidden");
                    }
                });
        });
    }
}

function setContainerSearchStartScrollHandler() {
    var $scrollArea = $('.common-search-panel-tabs #main-tab-pages');
    var $shadowArea = $('.common-search-panel .bottom-buttoms.last-cell').parent();

    $scrollArea.scroll(function () {
        addShadowTo($shadowArea);

        clearTimeout($(this).data('scrollTimerId'));

        $(this).data('scrollTimerId', setTimeout(function () {
            removeShadowFrom($shadowArea);
        }.bind(this), 500));
    });
}

function addShadowTo(shadowArea) {
    if (!(shadowArea.hasClass('shadowDuringScroll'))) {
        shadowArea.addClass('shadowDuringScroll');
    };
}
function removeShadowFrom(shadowArea) {
    if (shadowArea.hasClass('shadowDuringScroll')) {
        shadowArea.removeClass('shadowDuringScroll');
    };
}



function ContainerSearch_renderComplete(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{
    setContainerSearchStartScrollHandler();
    // default labels
    // only get page labels after render is complete
    var curStatusLbl = 'CurrentStatus';
    var historyLbl = 'History';
    if (typeof (__page) == 'undefined') {
        GetContainerSearchLabels();
    }
    else {
        var translatedCurStatus = __page._pageLabels.filter(function (l) {
            return l.Name === 'Container_CurrentStatus';
        });

        if (translatedCurStatus && translatedCurStatus.length) {
            curStatusLbl = translatedCurStatus[0].Value;
        }
        var translatedHistory = __page._pageLabels.filter(function (l) {
            return l.Name === 'Lbl_History'
        });
        if (translatedHistory && translatedHistory.length) {
            historyLbl = translatedHistory[0].Value;
        }
    }
    //update page messages based on new grid state
    var lblInstructions = $('#WebPart_QuickLinksWP span[id$="_ActionPanelInstructions"]');
    var divActions = $('#WebPart_ActionsControl');
    var divSingles = $('#WebPart_QuickLinksWP td.linkcell');
    var showActions = false, showSingles = false;

    if (this._totalSelectedRows == 0) {
        $(lblInstructions).html((lblInstructions).attr("defaultlabel"));
    }
    else {
        $(lblInstructions).html($($lblMultiActions).html().replace('{0}', this._totalSelectedRows));

        //check tracking arrays: status/content & check for $reloadActions flag
        if (typeof $reloadActions != 'undefined' && $reloadActions == true) {
            //update to current set in local memory - based off prior selection state of grid
            $arrCurrentActionRows = (typeof $arrCurrentRowSelections != 'undefined') ? $arrCurrentRowSelections.slice() : [];
            $reloadActions = false; //reset
        }

        var selarrrow = jQuery(this.GridID).getGridParam('selarrrow');
        if (typeof (selarrrow) !== 'undefined') {
            $arrCurrentRowSelections = selarrrow.slice(); //update to new/current row set

            if (typeof $arrCurrentActionRows != "undefined" && $arrCurrentActionRows.length == $arrCurrentRowSelections.length) {
                //sort arrays
                $arrCurrentActionRows = $arrCurrentActionRows.sort();
                $arrCurrentRowSelections = $arrCurrentRowSelections.sort();

                //show/hide based on comparisson of current selection with current action set
                showActions = (window.SerializeObject($arrCurrentActionRows) == window.SerializeObject($arrCurrentRowSelections));
                showSingles = (this._totalSelectedRows == 1 && showActions);
            }
        }
    }

    // Ignore hiding for command bar
    if (!document.body.attributes["sideMenu"]) {
        $(divActions).toggle(showActions);
    }
    $(divSingles).toggle(showSingles);
    $(lblInstructions).toggle(!showSingles);

    if (typeof $stateIsSearch == 'undefined')
        return;
    if ($stateIsSearch === false)
        return;

    //set default page messages
    var thisTab;

    $.each($tabHeaders, function (e) {
        if ($(this).hasClass('ui-state-active')) {
            switch (e) {
                case 0:
                    thisTab = !($($tabHeaders[1]).hasClass('ui-icon-combine')) ? curStatusLbl : 'CurrentAndHistory';
                    break;
                case 1:
                    thisTab = !($($tabHeaders[0]).hasClass('ui-icon-combine')) ? historyLbl : 'CurrentAndHistory';
                    break;
                case 2:
                default:
                    thisTab = $(this).first('a').text().replace(' ', '');
                    break;
            }

            return false;
        }
    });

    //set default grid title - NOTE - this may need to be moved to a postload function associated w/ the grid since grid title will not be available on initial load- jqgrid not built yet
    var regEx = '_' + thisTab + '$';
    $lblGridTitle = $('#WebPart_ContainerSearch_ResultsWP span.ui-jqgrid-title');

    //find corresponding title and set value
    $.each($lblGridTitles, function () {
        if ($(this).attr('id').toString().match(regEx) != null) {
            $($lblGridTitle).html(thisTab == 'UserQuery' ? $(this).html().replace('{0}', $($inputFilters_UserQuery).val()) : $(this).html());
            return false;
        }
    });
    //hide page message once search is performed
    $lblPageMessages.hide();
}

function ContainerSearch_AddToggleSearchAttr(isPostBack, isSearch, isTabSwitched, isUserVal) {
    // defaults
    var curStatusLbl = 'CurrentStatus';
    var historyLbl = 'History';
    // get page translated labels
    if (typeof (__page) !== 'undefined' && __page._pageLabels.length) {
        var translatedCurStatus = __page._pageLabels.filter(function (l) {
            return l.Name === 'Container_CurrentStatus';
        });
        if (translatedCurStatus && translatedCurStatus.length) {
            curStatusLbl = translatedCurStatus[0].Value;
        }
        var translatedHistory = __page._pageLabels.filter(function (l) {
            return l.Name === 'Lbl_History';
        });
        if (translatedHistory && translatedHistory.length); {
            historyLbl = translatedHistory[0].Value;
        }
    }

    //create global vars for Container Search page + add attributes and bind events
    $tabHeaders = $('div[id$="_ContainerSearch_TabContainer"] .ui-tabs-nav > li');

    //hidden items - maintained for content retrieval only
    $lblPageMessages = $('#WebPart_ContainerSearch_ResultsWP span[id*="_ContainerSearch_InfoMessage_"]').hide();
    $lblGridTitles = $('#WebPart_ContainerSearch_ResultsWP span[id*="_ContainerSearch_ResultsGridTitle_"]').hide();
    $lblNoActions = $('#WebPart_ContainerSearch_ResultsWP span[id$="_ActionPanel_NoActions"]').hide();
    $lblMultiActions = $('#WebPart_ContainerSearch_ResultsWP span[id$="_ActionPanel_MultiSelect"]').hide();
    $lblInstructions = $('#WebPart_QuickLinksWP span[id$="_ActionPanelInstructions"]');

    $stateIsSearch = isSearch === true;

    if (!isPostBack) {
        $inputFilters_CurrentStatus = $('#WebPart_CurrentStatusWP input[id$="_Edit"],#WebPart_CurrentStatusWP .cs-textbox > input[type="text"], #WebPart_CurrentStatusWP input[id*="ApplyLineAssignment"], #WebPart_CurrentStatusWP input[id*="InQualityControl"]').attr('togglesearch', curStatusLbl);
        $.merge($inputFilters_CurrentStatus, $('#WebPart_CurrentStatusWP input[id*="ApplyLineAssignment"], #WebPart_CurrentStatusWP input[id*="ApplyLineAssignment"]').attr('togglesearch', curStatusLbl));
        $inputFilters_History = $('#WebPart_HistoryWP input[id$="_Edit"], #WebPart_HistoryWP .cs-textbox > input[type="text"], #WebPart_HistoryWP .cs-date > input').attr('togglesearch', historyLbl);
        $.merge($inputFilters_History, $('#WebPart_HistoryWP input[id*="ApplyLineAssignment"]').attr('togglesearch', historyLbl));
    }

    $inputFilters_UserQuery = $('#WebPart_UserQueryWP input[id$="_ContainersTxn_UserQuery_Edit"]').attr('togglesearch', 'UserQuery');

    setTimeout(function () {
       
        $('#WebPart_CurrentStatusWP input[name*="ContainerName"]').on('keypress', function (e) { ContainerSearch_KeyPress($(this), e); });
        $inputFilters_CurrentStatus.off('change').on('change', function (e) { ContainerSearch_SearchButtonStatus($(this), e); });
        $inputFilters_History.off('change').on('change', function (e) { ContainerSearch_SearchButtonStatus($(this), e); });
        $inputFilters_UserQuery.off('change').on('change', function (e) { ContainerSearch_SearchButtonStatus($(this), e); });

        //wire update for clear all
        $('#WebPart_ContainerSearch_ActionsWP input[id$="_ContainerSearch_ClearAll"]').on('click', function () {
            $inputFilters_CurrentStatus.val('');
            $inputFilters_History.val('');
            $($tabHeaders).removeClass('ui-icon-combine');
        });
    }, 500);

    if (isPostBack === true && isTabSwitched === false) {
        //Check Action Panel Status 
        if (typeof $reloadActions === 'undefined' && $('#WebPart_ActionsControl div.action').length === 0 && $('#ctl00_WebPartManager_ContainerSearch_ResultsWP_ContainerSearch_ResultsGrid tr.ui-widget-content').attr('id') != '#empty#0') //no actions, display message
            $($lblInstructions).html($($lblNoActions).html());

        if (!isUserVal) {
            //Check Search button status
            var searchBtn = $find("ctl00_WebPartManager_ContainerSearch_ActionsWP_ContainerSearch_Search") ||
                $find("ctl00_WebPartManager_ContainerSearchWP_ContainerSearch_Search");
            if (searchBtn) {
                searchBtn.set_Disabled(!$($tabHeaders).hasClass('ui-icon-combine'));
            }
        }
    }

    //set default tab messages
    var thisTab;
    $.each($tabHeaders, function () {
        $(this).find('a').bind('click', function () { ContainerSearch_SetTabDetails($(this).text().replace(' ', '')); });

        if ($(this).hasClass('ui-state-active')) {
            thisTab = $(this).first('a').text().replace(' ', '');
            ContainerSearch_SetTabDetails(thisTab);
        }
    });
}

function ContainerSearchVPR2_AddToggleSearchAttr(isPostBack, isSearch, isTabSwitched, isUserVal) {
    // defaults
    var curStatusLbl = 'CurrentStatus';
    var historyLbl = 'History';
    var userQueryLbl = 'UserQuery';
    // get page translated labels
    if (typeof (__page) !== 'undefined' && __page._pageLabels.length) {
        var translatedCurStatus = __page._pageLabels.filter(function (l) {
            return l.Name === 'Container_CurrentStatus';
        });
        if (translatedCurStatus && translatedCurStatus.length) {
            curStatusLbl = translatedCurStatus[0].Value;
        }
        var translatedHistory = __page._pageLabels.filter(function (l) {
            return l.Name === 'Lbl_History';
        });
        if (translatedHistory && translatedHistory.length); {
            historyLbl = translatedHistory[0].Value;
        }

        var translatedUserQuery = __page._pageLabels.filter(function (l) {
            return l.Name === 'QueryType_User';
        });
        if (translatedUserQuery && translatedUserQuery.length); {
            userQueryLbl = translatedUserQuery[0].Value;
        }
    }

    //create global vars for Container Search page + add attributes and bind events
    $tabHeaders = $('div[id$="_ContainerSearch_TabContainer"] .ui-tabs-nav > li');

    //hidden items - maintained for content retrieval only
    $lblPageMessages = $('#WebPart_ContainerSearch_ResultsWP span[id*="_ContainerSearch_InfoMessage_"]').hide();
    $lblGridTitles = $('#WebPart_ContainerSearch_ResultsWP span[id*="_ContainerSearch_ResultsGridTitle_"]').hide();
    $lblNoActions = $('#WebPart_ContainerSearch_ResultsWP span[id$="_ActionPanel_NoActions"]').hide();
    $lblMultiActions = $('#WebPart_ContainerSearch_ResultsWP span[id$="_ActionPanel_MultiSelect"]').hide();
    $lblInstructions = $('#WebPart_QuickLinksWP span[id$="_ActionPanelInstructions"]');

    $stateIsSearch = isSearch === true;

    if (!isPostBack) {
        $inputFilters_CurrentStatus = $('#WebPart_CurrentStatusWP input[id$="_Edit"],#WebPart_CurrentStatusWP .cs-textbox > input[type="text"], #WebPart_CurrentStatusWP input[id*="ApplyLineAssignment"], #WebPart_CurrentStatusWP input[id*="InQualityControl"]').attr('togglesearch', curStatusLbl);
        $.merge($inputFilters_CurrentStatus, $('#WebPart_CurrentStatusWP input[id*="ApplyLineAssignment"],#WebPart_CurrentStatusWP input[id*="History_ApplyLineAssignment"],#WebPart_CurrentStatusWP input.cs-date, #WebPart_CurrentStatusWP input[id*="ApplyLineAssignment"]').attr('togglesearch', curStatusLbl));
        $inputFilters_History = $('#WebPart_CurrentStatusWP .hasDatepicker,#WebPart_HistoryWP input[id$="_Edit"], #WebPart_HistoryWP .cs-textbox > input[type="text"], #WebPart_HistoryWP .cs-date > input[type="text"]').attr('togglesearch', historyLbl);
        $.merge($inputFilters_History, $('#WebPart_HistoryWP input[id*="ApplyLineAssignment"]').attr('togglesearch', historyLbl));
    }

    $inputFilters_UserQuery = $('#WebPart_UserQueryWP input[id$="_ContainersTxn_UserQuery_Edit"]').attr('togglesearch', 'UserQuery');

    setTimeout(function () {
        $inputFilters_History = $('#WebPart_CurrentStatusWP .hasDatepicker,#WebPart_HistoryWP input[id$="_Edit"], #WebPart_HistoryWP .cs-textbox > input[type="text"], #WebPart_HistoryWP .cs-date > input[type="text"]');
        $('#WebPart_CurrentStatusWP input[name*="ContainerName"]').bind('keypress', function (e) { ContainerSearch_KeyPress($(this), e); });
        $inputFilters_CurrentStatus.off('change').on('change', function (e) { ContainerSearch_SearchButtonStatus($(this), e); });
        $inputFilters_History.off('change').on('change', function (e) { ContainerSearch_SearchButtonStatus($(this), e); });
        $inputFilters_UserQuery.off('change').on('change', function (e) { ContainerSearch_SearchButtonStatus($(this), e); });

        $inputFilters_History_new = $('#WebPart_CurrentStatusWP .hasDatepicker,#WebPart_HistoryWP input[id$="_Edit"], #WebPart_HistoryWP .cs-textbox > input[type="text"], #WebPart_HistoryWP .cs-date > input[type="text"]');
        $inputFilters_History_new.off('change').on('change', function (e) { ContainerSearch_SearchButtonStatus($(this), e); });
        //wire update for clear all
        $('#WebPart_ContainerSearchWP input[id$="_ContainerSearch_ClearAll"]').bind('click', function () {
            $inputFilters_CurrentStatus.val('');
            $inputFilters_History.val('');
            $inputFilters_History_new.val('');
            $($tabHeaders).removeClass('ui-icon-combine');
        });
    }, 500);

    if (isPostBack === true && isTabSwitched === false) {
        //Check Action Panel Status 
        if (typeof $reloadActions === 'undefined' && $('#WebPart_ActionsControl div.action').length === 0 && $('#ctl00_WebPartManager_ContainerSearch_ResultsWP_ContainerSearch_ResultsGrid tr.ui-widget-content').attr('id') != '#empty#0') //no actions, display message
            $($lblInstructions).html($($lblNoActions).html());

        if (!isUserVal) {
            //Check Search button status
            var searchBtn = $find("ctl00_WebPartManager_ContainerSearch_ActionsWP_ContainerSearch_Search") ||
                $find("ctl00_WebPartManager_ContainerSearchWP_ContainerSearch_Search");
            if (searchBtn) {
                searchBtn.set_Disabled(!$($tabHeaders).hasClass('ui-icon-combine'));
            }
        }
    }

    //set default tab messages
    var thisTab;
    $.each($tabHeaders, function () {
        $(this).find('a').bind('click', function () { ContainerSearch_SetTabDetails($(this).text().replace(' ', '')); });

        if ($(this).hasClass('ui-state-active')) {
            thisTab = $(this).first('a').text().replace(' ', '');
            ContainerSearch_SetTabDetails(thisTab);
        }
    });

    var search = $("#ctl00_WebPartManager_ContainerSearchWP_ContainerSearch_Search");

    if (isTabSwitched === true) {

        switch (thisTab) {
            case curStatusLbl:
                if (ContainerSearch_TabHasContent(curStatusLbl) === true) {
                    search.attr("disabled", false);
                }
                break;

            case userQueryLbl:
                if ($inputFilters_UserQuery.val() != '') {
                    search.attr("disabled", false);
                }
                break;
        }
    }
}
function doResetResultsGridScrollTop() {
    let resultsGrid = $('#gview_ctl00_WebPartManager_ContainerSearch_ResultsWP_ContainerSearch_ResultsGrid');
    let div = resultsGrid.find('.ui-jqgrid-bdiv');
    div.scrollTop(0);
}
function ContainerSearchVPR2_ResetResultsGridScrollTop() {
    setTimeout(doResetResultsGridScrollTop, 200);
}
function ContainerSearch_SetTabDetails(thisTab) {
    // defaults
    var curStatusRegEx = 'CurrentStatus';
    var historyRegEx = 'History';
    var userQueryRegEx = 'UserQuery';
    var curStatusLbl = 'CurrentStatus';
    var historyLbl = 'History';
    var userQueryLbl = 'UserQuery';

    if (typeof (__page) !== 'undefined' && __page._pageLabels.length) {
        var translatedCurStatus = __page._pageLabels.filter(function (l) {
            return l.Name === 'Container_CurrentStatus';
        });
        if (translatedCurStatus && translatedCurStatus.length) {
            curStatusLbl = translatedCurStatus[0].Value;
        }
        var translatedHistory = __page._pageLabels.filter(function (l) {
            return l.Name === 'Lbl_History';
        });
        if (translatedHistory && translatedHistory.length); {
            historyLbl = translatedHistory[0].Value;
        }
        var translatedUserQuery = __page._pageLabels.filter(function (l) {
            return l.Name === 'QueryType_User';
        });
        if (translatedUserQuery && translatedUserQuery.length) {
            userQueryLbl = translatedUserQuery[0].Value;
        }
    }
    if (thisTab == curStatusLbl) {
        $inputFilters_CurrentStatus = $('#WebPart_CurrentStatusWP input[id$="_Edit"],#WebPart_CurrentStatusWP .cs-textbox > input[type="text"]').attr('togglesearch', curStatusLbl);
        $.merge($inputFilters_CurrentStatus, $('#WebPart_CurrentStatusWP input[id*="ApplyLineAssignment"]').attr('togglesearch', curStatusLbl));
        $.merge($inputFilters_CurrentStatus, $('#WebPart_CurrentStatusWP input[id*="InQualityControl"]').attr('togglesearch', curStatusLbl));
    }
    if (thisTab == historyLbl) {
        $inputFilters_History = $('#WebPart_HistoryWP input[id$="_Edit"], #WebPart_HistoryWP .cs-textbox > input[type="text"], #WebPart_HistoryWP .cs-date > input').attr('togglesearch', historyLbl);
        $.merge($inputFilters_History, $('#WebPart_HistoryWP input[id*="ApplyLineAssignment"]').attr('togglesearch', historyLbl));
    }

    var isEnabled = ContainerSearch_TabHasContent(curStatusLbl);
    ContainerSearch_TabIcon(curStatusLbl, isEnabled);
    isEnabled = ContainerSearch_TabHasContent(historyLbl);
    ContainerSearch_TabIcon(historyLbl, isEnabled);
    var tabId = curStatusRegEx;
    switch (thisTab) {
        case curStatusLbl:
            tabId = curStatusRegEx;
        case historyLbl:
            tabId = historyRegEx;
            ContainerSearch_SearchButtonStatus(thisTab, null); //check for content in current tab

            if ($($tabHeaders[thisTab == curStatusLbl ? 1 : 0]).hasClass('ui-icon-combine')) //check for content in opposite tab
                thisTab = 'CurrentAndHistory';
            break;
        case userQueryLbl:
            tabId = userQueryRegEx;
            ContainerSearch_SearchButtonStatus($($inputFilters_UserQuery[0]), null);
            break;
    }

    var regEx = '_' + tabId + '$';
    $.each($lblPageMessages, function () {
        $(this).css('display', ($(this).attr('id').toString().match(regEx) != null ? '' : 'none'));
    });
}

function ContainerSearch_TabHasContent(thisTab) {
    // defaults
    var curStatusLbl = 'CurrentStatus';
    var historyLbl = 'History';
    if (typeof (__page) !== 'undefined' && __page._pageLabels.length) {
        var translatedCurStatus = __page._pageLabels.filter(function (l) {
            return l.Name === 'Container_CurrentStatus';
        });
        if (translatedCurStatus && translatedCurStatus.length) {
            curStatusLbl = translatedCurStatus[0].Value;
        }
        var translatedHistory = __page._pageLabels.filter(function (l) {
            return l.Name === 'Lbl_History';
        });
        if (translatedHistory && translatedHistory.length); {
            historyLbl = translatedHistory[0].Value;
        }
    }

    var hasContent = false;
    var $filters = null;

    switch (thisTab) {
        case curStatusLbl:
            $filters = $inputFilters_CurrentStatus;

            break;
        case historyLbl:
            $filters = $inputFilters_History;

            break;
    }

    //check if specific filters in set are empty
    $.each($filters, function () {
        if ($(this).is(':text') && ($(this).val() != '') || (($(this).is(':checkbox') && $(this).is(':checked')) && isLineAssignment())) {
            hasContent = true;
            return true;
        }
    });

    return hasContent;
}

function isLineAssignment() {
    var isLA = false;
    var header = getCEP_top().$find('ctl00_Header');
    if (!header)
        return isLA;

    if (header.get_operationValue() != '' || header.get_resourceValue() != '' || header.get_workcenterValue() != '' || header.get_workstationValue() != '' || header.get_specValue() != '')
        isLA = true;
    return isLA;
}

function ContainerSearch_TabIcon(thisTab, showIcon) {
    // defaults
    var curStatusLbl = 'CurrentStatus';
    var historyLbl = 'History';
    var userQueryLbl = 'UserQuery';

    if (typeof (__page) !== 'undefined' && __page._pageLabels.length) {
        var translatedCurStatus = __page._pageLabels.filter(function (l) {
            return l.Name === 'Container_CurrentStatus';
        });
        if (translatedCurStatus && translatedCurStatus.length) {
            curStatusLbl = translatedCurStatus[0].Value;
        }
        var translatedHistory = __page._pageLabels.filter(function (l) {
            return l.Name === 'Lbl_History';
        });
        if (translatedHistory && translatedHistory.length); {
            historyLbl = translatedHistory[0].Value;
        }
        var translatedUserQuery = __page._pageLabels.filter(function (l) {
            l.Name === 'QueryType_User';
        });
        if (translatedUserQuery && translatedUserQuery.length) {
            userQueryLbl = translatedUserQuery[0].Value;
        }
    }

    var tabID, style;

    switch (thisTab) {
        case curStatusLbl:
            tabID = 0;
            style = 'combine';
            break;
        case historyLbl:
            tabID = 1;
            style = 'combine';
            break;
        case userQueryLbl:
            tabID = 2;
            style = 'active';
            break;
        default:
            return;
    }

    if (showIcon)
        $($tabHeaders[tabID]).addClass('ui-icon-' + style);
    else
        $($tabHeaders[tabID]).removeClass('ui-icon-' + style);
}


function ContainerSearch_KeyPress(thisInput, e) {
    var code = (e.keyCode ? e.keyCode : e.which);
    if (code == 13) {
        ContainerSearch_SearchButtonStatus(thisInput, e);
        var searchBtn = $find("ctl00_WebPartManager_ContainerSearch_ActionsWP_ContainerSearch_Search") ||
            $find("ctl00_WebPartManager_ContainerSearchWP_ContainerSearch_Search");
        searchBtn.get_element().click();
    }
}

function ContainerSearch_SearchButtonStatus(thisInput, e) {
    // defaults
    var curStatusLbl = 'CurrentStatus';
    var historyLbl = 'History';
    var userQueryLbl = 'UserQuery';

    if (typeof (__page) !== 'undefined' && __page._pageLabels.length) {
        var translatedCurStatus = __page._pageLabels.filter(function (l) {
            return l.Name === 'Container_CurrentStatus';
        });
        if (translatedCurStatus && translatedCurStatus.length) {
            curStatusLbl = translatedCurStatus[0].Value;
        }
        var translatedHistory = __page._pageLabels.filter(function (l) {
            return l.Name === 'Lbl_History';
        });
        if (translatedHistory && translatedHistory.length); {
            historyLbl = translatedHistory[0].Value;
        }
        var translatedUserQuery = __page._pageLabels.filter(function (l) {
            return l.Name === 'QueryType_User';
        });
        if (translatedUserQuery && translatedUserQuery.length) {
            userQueryLbl = translatedUserQuery[0].Value;
        }
    }

    var thisTab,
        hasAttr = false,
        $thisInput;
    if (typeof thisInput == 'string') {
        thisTab = thisInput;
    }
    else {
        thisTab = $(thisInput).attr('togglesearch');
        hasAttr = true;
    }
    $thisInput = $(thisInput);
    var isEnabled = (hasAttr && (($thisInput.val() != '' && $thisInput.is(':text')) || ($thisInput.is(':checkbox') && $thisInput.is(':checked') && $thisInput.is('[id*="ApplyLineAssignment"]')) || ($thisInput.is(':checkbox') && $thisInput.is(':checked') && $thisInput.is('[id*="InQualityControl"]')) || (($thisInput.is(':checkbox') && $thisInput.is(':checked')) && isLineAssignment()))) || ((thisTab == curStatusLbl || thisTab == historyLbl) && ContainerSearch_TabHasContent(thisTab));
    if (e) {
        if (e.type == 'keypress' && e.which >= 48 && (($thisInput.val() != '' && $thisInput.is(':text')) || (($thisInput.is(':checkbox') && $thisInput.is(':checked')) && isLineAssignment())))
            isEnabled = true;
        else if (e.type == 'change' && (($(e.target).val() != '' && $(e.target).is(':text')) || ($(e.target).is(':checkbox') && $(e.target).is(':checked') && isLineAssignment())))
            isEnabled = true;
    }

    ContainerSearch_TabIcon(thisTab, isEnabled); //update icon

    if (!isEnabled)//check for entry in another field set
    {
        var curStatusLbl = typeof (defaultTabLabel) !== 'undefined' && defaultTabLabel === 'CurrentStatus' ? thisTab : 'CurrentStatus';
        var historyLbl = typeof (defaultTabLabel) !== 'undefined' && defaultTabLabel === 'History' ? thisTab : 'History';
        switch (thisTab) {
            case curStatusLbl:
                isEnabled = ContainerSearch_TabHasContent(historyLbl);
                break;
            case historyLbl:
                isEnabled = ContainerSearch_TabHasContent(curStatusLbl);
                break;
            case 'CurrentAndHistory':
                isEnabled = ContainerSearch_TabHasContent('CurrentStatus') || ContainerSearch_TabHasContent('History');
                break;
            case userQueryLbl:
                isEnabled = ($inputFilters_UserQuery.val() != '');
                break;
        }
    }
    var searchBtn = $find("ctl00_WebPartManager_ContainerSearch_ActionsWP_ContainerSearch_Search") ||
        $find("ctl00_WebPartManager_ContainerSearchWP_ContainerSearch_Search");
    if (searchBtn) {
        searchBtn.set_Disabled(!isEnabled);
    }
}

function ContainerSearch_SetDefaultDDLValue(displayID, controlID, displayVal, controlVal) {
    var ddlDisplay = $('#' + displayID);
    var ddlController = $('#' + controlID + '_Value');

    if ($(ddlDisplay).val() == '') { $(ddlDisplay).val(displayVal); $(ddlController).val(controlVal); }

    ContainerSearch_SearchButtonStatus($(ddlDisplay), null);
}

function ContainerSearchVPR2_SetDefaultDDLValue(displayID, controlID, displayVal, controlVal, controlReason) {
    var ddlDisplay = $('#' + displayID);
    var ddlController = $('#' + controlID + '_Value');
    var ddControlReason = $('#' + controlReason);


    if ($(ddlDisplay).val() == '' && $(ddControlReason).val() != '') { $(ddlDisplay).val(displayVal); $(ddlController).val(controlVal); }

    ContainerSearch_SearchButtonStatus($(ddlDisplay), null);
}

/* Maintenance Management */
function MaintenanceManagement_renderComplete(isAtTheEndOfProcessing, rowid, prm2) {
    var theGrid = jQuery(this.GridID);
    var jqRowData = theGrid.getRowData();

    var page = window.__page || parent.__page || getCEP_top().__page;

    var labelNames = [
        { Name: 'Fld_MaintMgmt_Pending' },
        { Name: 'Fld_MaintMgmt_Due' },
        { Name: 'Fld_MaintMgmt_PastDue' }
    ];

    page.getLabels(labelNames, function (response) {
        if ($.isArray(response)) {
            var labelMap = {};
            response.forEach(function (item) {
                labelMap[item.Name] = item.Value;
            });

            jqRowData.forEach(function (row) {
                try {
                    var rowClass;

                    switch (row['MaintenanceState']) {
                        case labelMap['Fld_MaintMgmt_Pending']:
                            rowClass = 'yellow';
                            break;
                        case labelMap['Fld_MaintMgmt_Due']:
                            rowClass = 'orange';
                            break;
                        case labelMap['Fld_MaintMgmt_PastDue']:
                            rowClass = 'red';
                            break;
                    }

                    if (rowClass) {
                        var jqRows = theGrid.find('.jqgrow');
                        var trCurrent = $(jqRows)
                            .find("td[title='" + row["MaintenanceStatus"] + "']:first")
                            .parent("tr:first");
                        $(trCurrent).addClass('ui-jqgrid-row-' + rowClass);
                    }
                } catch (ex) {
                    
                }
            });
        }
    });
}

//PEManage AffectedMaterials
function AMSearchResultsGrid_renderCompleted() {
    var containersOnlyOption = $("input[id*='SearchTypeContainersOnly']");
    var materialMatchOption = $("input[id*='SearchTypeMaterialMatch']");
    if (containersOnlyOption.length === 1 && materialMatchOption.length === 1) {
        var grid = $(this.GridID);
        (containersOnlyOption.is(":checked") || materialMatchOption.is(":checked")) ? grid.hideCol("subgrid") : grid.showCol("subgrid");
    }
}

/* Mfg Audit Trail */
function MfgAuditTrail_renderComplete(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{
    var gridId = this._gridID;
    var theGrid = jQuery(this.GridID);
    var jqRowData = theGrid.getRowData();
    var jqRows = theGrid.find('.jqgrow');
    $(jqRowData).each(function () {
        if (this['HasChild'] == '0') {
            $("td[class='ui-sgcollapsed sgcollapsed']", jqRows[parseInt(this['RowIndex'])])
                .removeClass('ui-sgcollapsed sgcollapsed')
                .children().remove();
        }

        if (this['HasChild'] != '' && this['HasChild'] != '0' && !jqRows[parseInt(this['RowIndex'])].id.startsWith('#empty#')) {
            AddDrillDownIndicator(this, jqRows)
        }

        var minTimeGMT = this['MinEndTimeGMT'];
        if (typeof minTimeGMT != 'undefined' && minTimeGMT) {
            var $tdCurrent = $(jqRows).find("td[aria-describedby$='MinEndTimeGMT'][title='" + minTimeGMT + "']:first");
            if ($tdCurrent.length) {
                var d = new Date(minTimeGMT);
                var resDate = d.format("dd.hh:mm:ss");
                $tdCurrent.attr('title', resDate);
                $tdCurrent.html(resDate);
            }
        }
        var maxTimeGMT = this['MaxEndTimeGMT'];
        if (typeof maxTimeGMT != 'undefined' && maxTimeGMT) {
            var $tdCurrent = $(jqRows).find("td[aria-describedby$='MaxEndTimeGMT'][title='" + maxTimeGMT + "']:first");
            if ($tdCurrent.length) {
                var d = new Date(maxTimeGMT);
                var resDate = d.format("dd.hh:mm:ss");
                $tdCurrent.attr('title', resDate);
                $tdCurrent.html(resDate);
            }
        }
    });

    MfgAuditTrailInquiry_MfgAuditTrailGrid_renderCompleted(this.GridID);
}

function MfgAuditTrailResetPage() {

    let a = $("#ctl00_WebPartManager_PanelFiltersSearchTabWP_PanelFiltersTabs > div.cs-nav-tabs > div > ul > li:nth-child(1) > a");

    $("#ctl00_WebPartManager_PanelFiltersSearchTabWP_ClearAllButton").on("click", function () { a.trigger("click"); });

    let hiddenReset = $("#ctl00_WebPartManager_PanelFiltersSearchTabWP_ClearAllButtonHidden");

    if (window["reset"]) {
        window["reset"] = false;
        hiddenReset.click();
    }

    a.on("click", function () {
        window["reset"] = true;
    });
}

function ResourceAuditTrail_renderComplete() {
    var gridId = this._gridID;
    var theGrid = jQuery(this.GridID);
    var jqRowData = theGrid.getRowData();
    var jqRows = theGrid.find('.jqgrow');

    $(jqRowData).each(function () {
        if (this['HasChild'] != '' && (this['HasChild'] != '0' || gridId.endsWith("MainLineGrid"))) {
            AddDrillDownIndicator(this, jqRows)
        }
    });

    var that = this;
    setTimeout(function (e) {
        AuditTrailSetGridHeight(that.GridID)
    }, 0);

    window.addEventListener("resize", debounce(function (e) {
        AuditTrailSetGridHeight(that.GridID);

    }));
}

function AttachmentsGrid_renderCompleted() {

    var pattern = new RegExp('[-+&#]', 'g');
    var titleAttachment = $("input[id*='AttachmentsGridEditing_WP_Title']");
    notDisplayWarningFromGrid = true;////Disabling validation in the grid, only server-side validation.
    titleAttachment.keypress(function (e) {
        var txt = String.fromCharCode(e.which);
        if (txt.match(pattern)) {
            return false;
        }
    });

    titleAttachment.bind('paste', function (e) {
        var $this = $(this);
        var oldValue = $this.val();
        setTimeout(function () {
            var newVal = $this.val();
            if (newVal.match(pattern)) {
                $this.val(oldValue);
            }
        }, 0);
    });

    var qualityObject = $("input[id*='UpdateEvent_QualityObject']");
    if (qualityObject.length == 2 && qualityObject.val() == "")
        __doPostBack(this.GridID, "UpdateQualityObjectInfo");

    var self = this;

    var gridId = this.GridID;
    var addGridId = gridId.replace('ctl00', 'add_ctl00');
    var addAttachmentButton = $(addGridId).first();

    if (addAttachmentButton && !self._bindForAttachmentButtonWasAdded) {
        self._bindForAttachmentButtonWasAdded = true;
        addAttachmentButton.bind('click', function (e) {
            self._sendRequest({ "RowID": null, }, "SelectAllRows", "_selectAll_completed");
        });
    }

    var viewAttachmentButton = $("[id$='ViewAttachmentButton']").first();
    if (viewAttachmentButton) {
        if (!viewAttachmentButton.attr('onclick')) {
            viewAttachmentButton.attr('onclick', "DownloadAttachment('" + self.GridID.substring(1) + "','');");
        }

        var openAttachmentButton = $('#ctl00_WebPartManager_AttachmentsGridEditing_WP_File_Editing_InputField').first();
        var titleInput = $('#ctl00_WebPartManager_AttachmentsGridEditing_WP_Title_Editing_ctl00').first();
        var versionInput = $('#ctl00_WebPartManager_AttachmentsGridEditing_WP_Version_Editing_ctl00').first();
        var fileNameInput = $('#fileuploadtextctl00_WebPartManager_AttachmentsGridEditing_WP_File_Editing').first();
        var fileNameHiddenInput = $('#ctl00_WebPartManager_AttachmentsGridEditing_WP_File_Editing_InputField_Hidden').first();

        if (self._totalSelectedRows == 1) {
            $(viewAttachmentButton).removeAttr("disabled");

            $(openAttachmentButton).attr("disabled", "disabled");
            $(titleInput).attr("disabled", "disabled");
            $(versionInput).attr("disabled", "disabled");

            var grid = $(this.GridID);
            var selectedRow = $("tr[aria-selected=true]", grid)[0];
            var value = selectedRow.cells[4].innerHTML;

            $(fileNameInput).attr('placeholder', value);
            $(fileNameInput).attr('value', value);
            $(fileNameHiddenInput).attr('value', value);
        } else {
            $(viewAttachmentButton).attr("disabled", "disabled");

            $(openAttachmentButton).removeAttr("disabled", "disabled");
            $(titleInput).removeAttr("disabled", "disabled");
            $(versionInput).removeAttr("disabled", "disabled");

            $(fileNameInput).removeAttr('placeholder');
            $(fileNameInput).removeAttr('value');

        }
    }
}

function SplitActionsToGroups(actionsId, groupName, groupCss, behaviour, needDelimiter, selectedActions) {
    var actionControl = $("#" + actionsId);
    if (groupName && actionControl.length > 0) {
        var actions = $('input[actionGroupName="' + groupName + '"]', actionControl);
        if (actions.length > 0) {
            var groupHtml = "<div class='" + groupCss + "'>";
            if (needDelimiter)
                groupHtml += "<div class='delimiter'></div>";
            for (var i = 0; i < actions.length; i++) {
                var action = $(actions[i]);
                var btnClass = action.attr('class');
                var btnDisabled = action.attr('disabled');
                var btnText = action.val();
                var btnId = action.attr('id');
                if (selectedActions.length > 0) {
                    if (behaviour == "1") // radiobutton.
                    {
                        var selAction = selectedActions[0];
                        if (btnId.indexOf(selAction) >= 0)
                            btnClass += " selected";
                    }
                }
                if (i == 0)
                    btnClass += " first";
                if (i == actions.length - 1)
                    btnClass += " last";

                var clickEvent = action[0].attributes['onclick'];
                if (clickEvent)
                    clickEvent = clickEvent.value;
                var clientClickHtml = "onclick=\"" + clickEvent + "\"";
                if (btnDisabled)
                    clientClickHtml = "onclick=\"return false;" + "\"" + " disabled='disabled'";

                var actionHtml = "<button class='" + btnClass + "'" + clientClickHtml + "title='" + btnText + "' id='" + btnId + "'>"
                    + "<span class='btnIcon'></span>"
                    + "<span class='btnText'>" + btnText + "</span>"
                    + "</button>";
                groupHtml += actionHtml;

                action.removeAttr('id');
                action.parent().hide();
            }
            actionControl.append(groupHtml);
        }
    }
}

function MDL_instanceList_rendered(isAtTheEndOfProcessing, rowid, prm2) {
    // Adjust the grid
    var theGrid = $(this.GridID);
    var grid = this;

    var mode = $.jgrid.getUserAttributes(grid, 'listViewMode', 0);
    var exp = $.jgrid.getUserAttributes(grid, 'listViewMode', 2);
    $('div#WebPart_MDL_Filter_WP').attr('modeling', mode);
    if (mode == 'list') {
        theGrid[0].control.wrapEverything = true;
        var $bdiv = theGrid.parent().parent();
        if (!exp)
            $bdiv.width(231).height("calc( 100vh - 193px )");

        // Fix column width
        var isRDO = (theGrid.find('tr.jqgroup').length > 0);
        var td = theGrid.find('tr.jqgfirstrow td:visible');
        if (grid.multiCheckBox) {
            if ($(td[1]).width() < 100)
                td = $(td[2]);
            else
                td = $(td[1]);
            if (isRDO)
                td.width(200);
            else
                td.width(202);
        }
        else
            td.width(211); // for modeling.

        var getData = function ($r, col) {
            return $('td[aria-describedby="' + grid._gridID + '_' + col + '"]', $r);
        };
        $('tr:not(.jqgfirstrow)', theGrid).each(
            function () {
                var $tr = $(this);
                if (!$tr.hasClass("jqgroup")) {
                    var revision = getData($tr, "Revision").text();
                    var name = getData($tr, "Name").text();
                    //Refactor to use actual label.
                    var rorLbl = " ROR" + getRevisionDelimiter() + " ";
                    if (revision) {
                        var instanceId = getData($tr, "InstanceId").text();
                        var revOfRcd = getData($tr, "RevOfRcd").text();
                        var isROR = instanceId === revOfRcd;
                        var $displayedTd = getData($tr, "Displayed");
                        if (!$displayedTd.hasClass("instance-name")) {
                            var displayName = name + getRevisionDelimiter() + revision;
                            $displayedTd.text(displayName);

                            // add data and title for textoverflow and tooltip
                            $displayedTd.data("val", displayName).addClass("instance-name").toggleClass("exp-imp", !!exp);
                        }
                        else {
                            // formatting is already done
                            return true;
                        }
                        if (isROR) {
                            $displayedTd.append("<span rev-is-ror class=ui-rdo-ror />");

                            // Fix for - CPR 268955:FF(v8)PR 267879 TAC :ROR name sometimes does not appear in the instance name
                            /* Added .trim() to name variable to fix CPR 412472:PR ? TAC 11053573 Same instance name with without space.
                             * If user added RDO instances with leading or trailing space then ROR suffix should visible.
                            */
                            var grp = $tr.siblings(".jqgroup").filter(function () { return this.innerText == name.trim() }).first();

                            var textNode = $("td", grp)
                                .contents()
                                .filter(function () {
                                    return this.nodeType == Node.TEXT_NODE;
                                });
                            if (typeof (textNode[0]) != "undefined")
                                textNode[0].textContent = name + rorLbl + revision;
                        }
                        else
                            $("span[rev-is-ror]", $displayedTd).remove();
                    }
                }
            });

        theGrid.find('tr.jqgroup').unbind('click').click(
            function (e) {
                if ($(e.target).is('span.ui-icon')) {
                    return true; // Open/close click 
                }
                else if ($(e.target).is('span[chevron]')) {
                    $.jgrid.clickOnChevron(e);
                    return false;
                }
                else {
                    var tr = $(this).next();
                    while (tr.length > 0) {
                        if (tr.find('span.ui-rdo-ror').length > 0) {
                            tr.click();
                            return false;
                        }
                        tr = tr.next();
                    }
                }
                return true;
            }
        );

        $('#gbox_' + this._gridID + ' tr[role=row]').click(function (e) {
            if (__page.isDirty()) {
                var me = this;
                jConfirmSetText(me, theGrid);
                e.stopPropagation();
            }
        });
        // Modify the label text
        var ctl = $('#ctl00_WebPartManager_MDL_Filter_WP_InstanceNameTxt');
        if (ctl.find('span.filter-refresh-btn').length == 0) {
            if (exp && exp == 'export') // if within Export pageflow.
            {
                var label = $('[id$=lblAvailableInstances]').text();
                $('<div class="webpart-actions">' +
                    '<div class="header"><span>' + label + '</span><div class="filter-buttons-div"></div></div>' +
                    '</div>'
                ).insertBefore(ctl.children('span').first());
                var btn = $("#ctl00_WebPartManager_MDL_Filter_WP_PopupFilterBtn");
                btn.appendTo($(".filter-buttons-div", ctl));
            }
            else {
                var label = $('[id$=lblInstances]').text();
                $('<div class="webpart-actions">' +
                    '<div class="header"><span>' + label + '</span>' +
                    '<span class="objectsCount">2 of 2</span>' +
                    '</div>' +
                    '</div>'
                ).insertBefore(ctl.children('span').first());
            }

            ctl.children('span').first().hide();

            $('<span class="filter-refresh-btn" />')
                .insertAfter($('input[type="text"]', ctl[0]))
                .click(
                    function () {
                        grid.SetFilter(ctl.find('input[type="text"]').val());
                        grid.Reload(true); // force reload
                    }
                );


            $('input[name="ctl00$WebPartManager$MDL_Filter_WP$InstanceNameTxt$ctl00"]')
                .keypress(
                    function (e) {
                        var keyCode = e.which;
                        if (keyCode == 13) {
                            $('span.filter-refresh-btn').click();
                            e.preventDefault();
                        }
                    });

        }
        var objCount = $('span.objectsCount', ctl);
        if (objCount.length) {
            parent.__page.getLabel('Grid_Of',
                function (response) {
                    if ($.isArray(response)) {
                        // Label
                        var label = response[0];
                        objCount.text(theGrid.getGridParam('reccount') + ' ' + label.Value + ' ' + theGrid.getGridParam('records'));
                    }
                    else {
                        // Error
                        objCount.text(theGrid.getGridParam('reccount') + ' of ' + theGrid.getGridParam('records'));
                    }
                });
        }

        // Disable rows for export/import
        var tdz = $('td[aria-describedby$="ItemAlreadySelected"]', theGrid);
        if (tdz.length) {
            tdz.each(
                function (ind, td1) {
                    var tr = $(td1).parent();
                    var slc = $(td1).text() == "True";
                    tr.attr('disabled', slc);
                    if (slc) {
                        tr.find(':checkbox').each(function () {
                            if (slc)
                                $(this).attr('disabled', 'disabled');
                            else
                                $(this).removeAttr('disabled');
                            this.checked = slc;
                        });
                    }
                }
            );
        }

    }
    else if (mode == 'grid') {
        // Replace status values with text
        $('td[aria-describedby="ctl00_WebPartManager_MDL_Filter_WP_InstanceGrid_Status"]', theGrid).each(
            function (ind, td1) {
                var t = $(td1);
                if (t.attr('title') == "1")
                    t.text('Active');
                else
                    t.text('Inactive');
            }
        );
    }

    setTimeout(function () {
        resizeModelingGrid(theGrid);
    }, 0);


    // scroll to selected row
    var selectedRow = theGrid.find('tr[aria-selected="true"]');
    if (selectedRow.length) {
        //selectedRow[0].scrollIntoView();
    }

    var $selectAllChk = $('#ctl00_WebPartManager_MDL_Filter_WP_SelectAllItemsChk_ctl00');
    if ($selectAllChk.length) {
        var mb = this.multiCheckBox;
        if (theGrid.data('isChecking') === 'checking') {
            // don't touch in case of checking progress
            theGrid.data('isChecking', '');
        } else {
            // Sync check status in other case
            $selectAllChk.prop('checked', mb.prop('checked'));
            $selectAllChk.trigger('change');
        }

        $selectAllChk.unbind('click').bind('click', function (e) {
            theGrid.data('isChecking', 'checking');
            mb.prop('checked', !this.checked);
            mb.click();
        });
    }
}

function FireInstanceConfirm(text, theGrid, me, warning) {
    jConfirm(text, null, function (r) {
        if (r == true) {
            theGrid.resetSelection();
            var $tr = $(me);
            //search for active revision tr, to send correct Id to the server
            if ($(me).hasClass("jqgroup")) {
                while ($tr.length > 0) {
                    if ($tr.find('span.ui-rdo-ror').length > 0) {
                        theGrid.setSelection($tr.attr("id"), true);
                        break;
                    }
                    $tr = $tr.next();
                }
            }
            else
                theGrid.setSelection(me.id, true);
            __page.resetDirty();
        }
    }, warning);

}

function jConfirmSetText(me, theGrid) {
    var labels = [{ Name: 'Lbl_UnsavedChangesForInstances' }, { Name: 'Lbl_Warning' }];
    __page.getLabels(labels, function (response) {
        if ($.isArray(response)) {
            var value;
            var warning;
            $.each(response, function () {
                var labelname = this.Name;
                var labeltext = this.Value;
                switch (labelname) {
                    case 'Lbl_UnsavedChangesForInstances':
                        value = labeltext;
                        break;
                    case 'Lbl_Warning':
                        warning = labeltext;
                        break;
                    default:
                        break;
                }
            });
            FireInstanceConfirm(value, theGrid, me, warning);
        }
        else {
            alert(response.Error);
        }
    });
}

function resizeModelingGrid(grid) {
    var oBody = window.document.body;
    if ($(oBody).hasClass('body-modeling')) {
        if (grid) {
            if (grid.closest(".export-import-left-panel").length)
                return;

            // Remove height:auto from the zone
            var z = grid.closest("div.zone");
            z.prop("style", null);

            // Set new grid height
            var $wp = $("#WebPart_MDL_Filter_WP", z);



            // Calculate height of the static elements
            var delta = $("#ctl00_WebPartManager_MDL_Filter_WP_InstanceNameTxt", $wp).closest("td").outerHeight(true);
            var $actions = $("#TemplateContentDiv > table > tbody > tr > td.mdl-view-modes");
            var actions_height = $actions.outerHeight(true);
            delta += (actions_height === 0) ? getActionsBarHeight($actions) : actions_height;
            var $pager = $(".ui-jqgrid-pager", $wp);
            delta += $pager.outerHeight(true);

            delta += 8; // extra padding

            var $gview = $("#gview_" + grid.prop("id") + " div.ui-jqgrid-bdiv");
            $gview.height("calc( 100vh - " + delta + "px)");
        }
    }
}

function getActionsBarHeight(selector) {
    try {
        return parseFloat($(selector).find("div#WebPart_ActionsControl")[0].style.height);
    } catch {
        return 0;
    }
}

function ChangeMngApplicationApproval_editingStarted(isAtTheEndOfProcessing, rowid) {
    var currentRow = $('tr[id="' + rowid + '"]', $(this.GridID));
    var daysAfterDue = $("input[name='ctl00$WebPartManager$PackageWP$ApprovalReminders_DaysAfterDue_InlineEditorControl$ctl01']")[0];
    var daysBeforeDye = $("input[name='ctl00$WebPartManager$PackageWP$ApprovalReminders_DaysBeforeDue_InlineEditorControl$ctl01']")[0];
    var dayBeforeCell = $(".daybefore", currentRow);
    var dayAfterCell = $(".dayafter", currentRow);

    dayBeforeCell.bind('change', function (event) {
        daysAfterDue.value = "";

    });
    dayAfterCell.bind('change', function (event) {
        daysBeforeDye.value = "";
    });
}

function ChangeMngApplicationCollaboration_editingStarted(isAtTheEndOfProcessing, rowid) {
    var currentRow = $('tr[id="' + rowid + '"]', $(this.GridID));
    var daysAfterDue = $("input[name='ctl00$WebPartManager$CollaborationWP$CollaborationReminders_DaysAfterDue_InlineEditorControl$ctl01']")[0];
    var daysBeforeDye = $("input[name='ctl00$WebPartManager$CollaborationWP$CollaborationReminders_DaysBeforeDue_InlineEditorControl$ctl01']")[0];
    var dayBeforeCell = $(".daybefore", currentRow);
    var dayAfterCell = $(".dayafter", currentRow);

    dayBeforeCell.bind('change', function (event) {
        daysAfterDue.value = "";

    });
    dayAfterCell.bind('change', function (event) {
        daysBeforeDye.value = "";
    });
}


// Detached tab container code with horizontal scrolling
function Organization_Category_change(e) {
    var categoryField = $find('ctl00_WebPartManager_CategoryMapEditor_Category');
    var selectedCategory = categoryField.get_value().value;
    var fieldsToUpdate = [$find('ctl00_WebPartManager_CategoryMapEditor_Role'), $find('ctl00_WebPartManager_CategoryMapEditor_Owner')];
    //if category='nonconformance'
    if (selectedCategory == '3') {
        fieldsToUpdate.forEach(function (el) { el.clearValue(); el.set_ReadOnly(true); });
    }
    else
        fieldsToUpdate.forEach(function (el) { el.set_ReadOnly(false); });
}
// export/import implementations

function RefreshInstances(p) {
    var instGridTable = $('table[id$="_MDL_Filter_WP_InstanceGrid"]');
    if (instGridTable.length) {
        var grid = $find(instGridTable.attr('id'));
        if (grid) {
            var cdoListCtl = $('span[id$="MDL_Objects_WP_CDOList"]');
            if (cdoListCtl.length) {
                var cdoComponent = $find(cdoListCtl.attr('id'));
                if (cdoComponent && cdoComponent.SelectedItem != null) {
                    var pb_args = "Click," + $(cdoComponent.SelectedItem).attr('maint') + ',' + $(cdoComponent.SelectedItem).attr('isRDO') +
                        ',' + $(cdoComponent.SelectedItem).attr('CDODefID') + ',' + encodeURIComponent($(cdoComponent.SelectedItem).text());
                    __page.postback(cdoComponent.get_name(), pb_args);
                }
            }
        }
    }
    return false;
}

function ExportImportCheck() {

    var spanExport = $('#ctl00_WebPartManager_ChoiceWP_ManualExportBtr');
    var spanImport = $('#ctl00_WebPartManager_ChoiceWP_ManualImportBtr');
    var spanAuto = $('#ctl00_WebPartManager_ChoiceWP_ManualAutotBtr');

    var radioExport = $('input[id*="ExportBtr"]');
    var radioImport = $('input[id*="ImportBtr"]');
    var radioAuto = $('input[id*="AutoBtr"]');
    if (radioExport.is(':checked')) {
        spanExport.parent().addClass('export-import-navigation-highlight');
        spanImport.parent().removeClass('export-import-navigation-highlight');
        spanAuto.parent().removeClass('export-import-navigation-highlight');
        PopulateManualExportLabels();
    }
    if (radioImport.is(':checked')) {
        spanExport.parent().removeClass('export-import-navigation-highlight');
        spanImport.parent().addClass('export-import-navigation-highlight');
        spanAuto.parent().removeClass('export-import-navigation-highlight');
        PopulateManualImportLabels();
    }
    if (radioAuto.is(':checked')) {
        spanExport.parent().removeClass('export-import-navigation-highlight');
        spanImport.parent().removeClass('export-import-navigation-highlight');
        spanAuto.parent().addClass('export-import-navigation-highlight');
    }
}

function ClearPopupMessage(parameters) {
    var statusBar = $("#WebPart_StatusBar");
    if (statusBar.length === 1 && statusBar.is(":visible"))
        statusBar.hide();

}

function ShowPopupMessage(id, isPostback) {
    var statusBar = $find(id);
    if (statusBar != null)
        statusBar.open(isPostback);
    else
        Sys.Application.add_load(function () { $find(id).open(isPostback); });
}

function PopulateManualExportLabels() {
    var element = $('.DT_Diagram_ManualExport');
    var labels = [{ Name: 'Lbl_ManualExportDataTransferFlowChart' }, { Name: 'Lbl_SourceSystemCurrent' }, { Name: 'Lbl_ApplyFilters' }, { Name: 'Lbl_ExportProcess' }, { Name: 'Lbl_ReviewTargetImpact' }, { Name: 'Lbl_ExportPackage' }];
    GetLabels(labels, element);
}
function PopulateManualImportLabels() {
    var element = $('.DT_Diagram_ManualImport');
    var labels = [{ Name: 'Lbl_ManualImportDataTransferFlowChart' }, { Name: 'Lbl_ExportPackage' }, { Name: 'Lbl_ImportProcess' }, { Name: 'Lbl_TargetSystemSelected' }, { Name: 'Lbl_ExecutionSummary' }];
    GetLabels(labels, element);
}
function CreateLabel(labelText, marginleft, margintop, width, isBold, isTitle) {
    var label;
    if (isBold) {
        label = $('<label/>', { 'text': labelText })
            .css({ 'position': 'absolute', 'margin-left': marginleft, 'margin-top': margintop, 'width': width, 'font-weight': 'bold' });
    }
    else {
        label = $('<label/>', { 'text': labelText })
            .css({ 'position': 'absolute', 'margin-left': marginleft, 'margin-top': margintop, 'width': width });
    }
    if (isTitle) {
        $(label).attr('title', labelText);
        var overflowStyle = { '-ms-text-overflow': 'ellipsis', 'text-overflow': 'ellipsis', 'white-space': 'nowrap', 'overflow': 'hidden' };
        $(label).css(overflowStyle);
    }
    return label;
}
function GetLabels(ary, elementToAppend) {
    __page.getLabels(ary,
        function (lbls) {
            if ($.isArray(lbls)) {
                $.each(lbls, function () {
                    var labelName = this.Name;
                    var labelText = this.Value;
                    var marginleft;
                    var margintop;
                    var width;
                    var isBold;
                    var isTitle;
                    switch (labelName) {
                        case 'Lbl_ManualExportDataTransferFlowChart':
                            marginleft = '0px';
                            margintop = '10px';
                            width = '250px';
                            isBold = true;
                            isTitle = true;
                            break;
                        case 'Lbl_SourceSystemCurrent':
                            marginleft = '45px';
                            margintop = '75px';
                            width = '200px';
                            isBold = true;
                            isTitle = false;
                            break;
                        case 'Lbl_ApplyFilters':
                            marginleft = '150px';
                            margintop = '150px';
                            width = '75px';
                            isBold = false;
                            isTitle = false;
                            break;
                        case 'Lbl_ExportProcess':
                            marginleft = '205px';
                            margintop = '100px';
                            width = '100px';
                            isBold = false;
                            isTitle = false;
                            break;
                        case 'Lbl_ReviewTargetImpact':
                            marginleft = '300px';
                            margintop = '150px';
                            width = '50px';
                            isBold = false;
                            isTitle = false;
                            break;
                        case 'Lbl_ExportPackage':
                            marginleft = '490px';
                            margintop = '125px';
                            width = '50px';
                            isBold = true;
                            isTitle = false;
                            break;
                        case 'Lbl_ManualImportDataTransferFlowChart':
                            marginleft = '0px';
                            margintop = '10px';
                            width = '250px';
                            isBold = true;
                            isTitle = true;
                            break;
                        case 'Lbl_ImportProcess':
                            marginleft = '440px';
                            margintop = '255px';
                            width = '100px';
                            isBold = false;
                            isTitle = false;
                            break;
                        case 'Lbl_TargetSystemSelected':
                            marginleft = '350px';
                            margintop = '400px';
                            width = '200px';
                            isBold = true;
                            isTitle = false;
                            break;
                        case 'Lbl_ExecutionSummary':
                            marginleft = '110px';
                            margintop = '380px';
                            width = '200px';
                            isBold = false;
                            isTitle = false;
                            break;
                        default:
                            break;
                    }
                    var label = CreateLabel(labelText, marginleft, margintop, width, isBold, isTitle);
                    $(elementToAppend).append(label);
                });
            }
            else {
                __page.getLabel('Lbl_ErrorIncorrectLabelName',
                    function (callback) {
                        if ($.isArray(callback)) {
                            var msg = callback[0];
                            alert(msg.Value);
                        }
                    });
            }
        });
}
/* WIP Messages */
function WIPMsg_MsgType_change(e) {
    var typeCtrl = $find('ctl00_WebPartManager_WIPMessageTypeWP_WIPMsgType');
    var type = typeCtrl.get_value().value;
    var operationToFind = $find('ctl00_WebPartManager_WIPMessageTypeWP_OperationToFind');
    operationToFind.set_Hidden(type != 3);
    var labelToFind = $find('ctl00_WebPartManager_WIPMessageTypeWP_LabelToFind');
    labelToFind.set_Hidden(type != 2);
}

function WIPMsgKeyGrid_NewKeyAdded_renderCompleted() {
    var grid = $(this.GridID);
    var selectedRowID = grid.getGridParam("selrow");

    if (window['msgGridCollection'])
        msgGridCollection = msgGridCollection.add($(this._element));
    else
        msgGridCollection = $(this._element);

    //if row is become selected, deselect the other rows
    $('.jqgrow', grid).click(function () {
        $('.ui-state-highlight', '.WIPMsgTreeCell').not(this).removeClass("ui-state-highlight").attr({ "aria-selected": "false", "tabindex": "-1" })
        var parent = $(this).parents(msgGridCollection);
        msgGridCollection.not(parent).each(function () {
            this.p.selrow = null;
        });
    });

    //select manully if the selected row is changed on the server side
    var stateControl = $('#TreeStateControl.cs-textbox > input[type="text"]');
    var stateVal = stateControl.val();
    var stateArr = stateVal.split('$');
    if (stateArr.length < 2)
        return;
    var newContrextID = stateArr[0];
    var rowToBeSelecte = stateArr[1];
    if (this.get_contextId() == newContrextID) {
        if (selectedRowID != rowToBeSelecte)
            $('#' + rowToBeSelecte, grid).click();
        stateControl.val('');
    }
}

function WIPMsgGrid_renderCompleted() {
    var grid = $('#gview_' + this._gridID);
    //first load
    if (window['__page'] === undefined)
        $('.ui-jqgrid-bdiv', grid).toggle();
    if ($('.ui-jqgrid-bdiv', grid).is(':visible'))
        $('.ui-jqgrid-titlebar', grid).addClass('expanded-titlebar');
    $('.ui-jqgrid-titlebar', grid).click(function () {
        $('.ui-jqgrid-bdiv', grid).toggle();
        $(this).toggleClass('expanded-titlebar');
    });
    return false;
}

function WIPAllKeyGrid_renderCompleted() {
    WIPMsgKeyGrid_NewKeyAdded_renderCompleted.call(this);
    WIPMsgGrid_renderCompleted.call(this);
}

function PackageSearch_renderCompleted() {
    var grid = $("div[id*='gbox_ctl00_WebPartManager_CM_ChangePkgSearchResult_WP']");
    var checkboxesGrid = $(".cbox", grid);
    var lblInstructions = $('#WebPart_QuickLinksWP span[id$="_ActionPanelInstructions"]');
    var rows = $(".jqgrow");

    rows.bind('click', function (e) {
        $("#WebPart_ActionsControl").hide();
    });

    checkboxesGrid.bind('change', function (event) {
        $("#WebPart_ActionsControl").hide();
    });

    if (this._totalSelectedRows == 0)
        lblInstructions.html((lblInstructions).attr("defaultlabel"));
    else {
        var that = this;
        __page.getLabel('Lbl_SelectedPackagesCount',
            function (response) {
                if ($.isArray(response)) {
                    lblInstructions[0].textContent = response[0].Value + that._totalSelectedRows;
                }
            });
    }
}

function DelegationSearchGrid_renderCompleted() {

    $("#WebPart_ActionsControl").css('display', 'block');
    var delegateTask = $("#WebPart_ActionsControl_UIComponent");
    delegateTask.css('display', 'none');
    var grid = $(this.GridID);
    var selectedRowID = grid.getGridParam("selrow");
    if (selectedRowID != null)
        delegateTask.css('display', 'block');
}

function AMSearchGrid_renderCompleted() {
    var scrapButton = $("#ctl00_WebPartManager_ActionsControl_QOScrap");
    var splitButton = $("#ctl00_WebPartManager_ActionsControl_QOSplit");
    var holdButton = $("#ctl00_WebPartManager_ActionsControl_QOHolds");
    var releaseButton = $("#ctl00_WebPartManager_ActionsControl_QOReleases");
    var moveButton = $("#ctl00_WebPartManager_ActionsControl_QOMoveNonStds");
    scrapButton.attr('disabled', 'disabled');
    splitButton.attr('disabled', 'disabled');
    holdButton.attr('disabled', 'disabled');
    releaseButton.attr('disabled', 'disabled');
    moveButton.attr('disabled', 'disabled');
    var grid = $(this.GridID);
    var selRow = $('tr.ui-state-highlight', grid);
    if (selRow.length == 1) {
        scrapButton.removeAttr('disabled');
        splitButton.removeAttr('disabled');
        holdButton.removeAttr('disabled');
        releaseButton.removeAttr('disabled');
        moveButton.removeAttr('disabled');

    } else if (selRow.length > 1) {

        holdButton.removeAttr('disabled');
        releaseButton.removeAttr('disabled');
        moveButton.removeAttr('disabled');
    }

}
function TogglePermissionsVisibility(parameters) {
    var permissions = {
        Name: [190, 200, 210],
        Perform: [100, 120, 130, 150, 160, 170],
        Create: [110, 180, 230],
        Read: [110, 180, 230],
        Update: [110, 180, 230],
        Delete: [110, 180, 230],
        Lock: [110, 230],
        Unlock: [110, 230],
        DispatchOverride: [120, 130],
        Invoke: [140],
        DownloadExport: [160],
        UploadImport: [160],
        StopProcess: [160],
        ResumeProcess: [160],
        RestartProcess: [160],
        Grant: [180],
        Revoke: [180],
        AllowLogin: [190],
        Browse: [200],
        AllowAccess: [210]
    };
    var grid = $(this.GridID);
    var permissionCode = parseInt($("input[name*='PermissionTypeCode']").val());
    if (isNaN(permissionCode))
        return;
    var columnModel = grid.getGridParam("colModel");
    //Skips first 6 columns, which are not checkbox
    for (var i = 6; i < columnModel.length; i++) {
        var columnName = columnModel[i].name;
        if ($.inArray(permissionCode, permissions[columnName]) !== -1) {
            grid.showCol(columnName);
            if ($.inArray(permissionCode, permissions["Name"]) !== -1) {
                grid.hideCol("SelVal");
            }
            continue;
        }
        else
            grid.hideCol(columnName);
    }
}

function BusinessRuleHandler_DataType_Changed(obj1, obj2, obj3) {
    var id = $(obj1.target).parents("table").first().attr("id");
    var grid = $('#' + id);
    var colModels = grid.getGridParam("colModel");
    if (typeof (colModels) == 'undefined')
        return;

    var dataTypeValue = $get(obj1.target.id.substring(0, obj1.target.id.lastIndexOf("_Edit")) + "_Value").value;

    var objectTypeNameInlineControlID = $(obj1.target).parents("tr").first().find('div[name=ObjectTypeName]').attr('id');
    var objectTypeNameControl = $find(objectTypeNameInlineControlID.substring(0, objectTypeNameInlineControlID.lastIndexOf("_Div")));
    if (objectTypeNameControl)
        objectTypeNameControl.setValue('');

    var objectDisplayValueInlineControlID = $(obj1.target).parents("tr").first().find('div[name=ObjectDisplayValue]').attr('id');
    var objectDisplayValueControl = $find(objectDisplayValueInlineControlID.substring(0, objectDisplayValueInlineControlID.lastIndexOf("_Div")));
    if (objectDisplayValueControl)
        objectDisplayValueControl.setValue('');

    var defaultValueInlineControlID = $(obj1.target).parents("tr").first().find('div[name=DefaultValue]').attr('id');
    var defaultValueControl = $find(defaultValueInlineControlID.substring(0, defaultValueInlineControlID.lastIndexOf("_Div")));
    if (defaultValueControl)
        defaultValueControl.setValue('');

    var objectDefaultValueInlineControlID = $(obj1.target).parents("tr").first().find('div[name=ObjectDefaultValue]').attr('id');
    var objectDefaultValueControl = $find(objectDefaultValueInlineControlID.substring(0, objectDefaultValueInlineControlID.lastIndexOf("_Div")));
    if (objectDefaultValueControl)
        objectDefaultValueControl.setValue('');

    if (dataTypeValue == 5) //Object data type
    {
        grid.showCol('ObjectDisplayValue');
        grid.hideCol('DefaultValue');

        $('#' + objectTypeNameInlineControlID).show();
    }
    else {
        grid.hideCol('ObjectDisplayValue');
        grid.showCol('DefaultValue');

        $('#' + objectTypeNameInlineControlID).hide();
    }
}

function BusinessRuleHandler_ObjectTypeName_Changed(obj1, obj2, obj3) {
    var id = $(obj1.target).parents("table").first().attr("id");
    var grid = $('#' + id);
    var colModels = grid.getGridParam("colModel");
    if (typeof (colModels) == 'undefined')
        return;

    var objectTypeInlineControlID = $(obj1.target).parents("tr").first().find('div[name=ObjectTypeName]').attr('id');
    var objectTypeControl = $find(objectTypeInlineControlID.substring(0, objectTypeInlineControlID.lastIndexOf("_Div")));
    var value = { key: objectTypeControl._value.value, text: objectTypeControl._value.value };

    var objectTypeTypeIdInlineControlID = $(obj1.target).parents("tr").first().find('div[name=ObjectTypeTypeId]').attr('id');
    var objectTypeTypeIdControl = $find(objectTypeTypeIdInlineControlID.substring(0, objectTypeTypeIdInlineControlID.lastIndexOf("_Div")));
    objectTypeTypeIdControl.setValue(value);
}

function BusinessRuleHandler_editingStarted(isEndOfOperation, rowid) {
    var grid = $('#' + this._gridID);
    var objectTypeNameInlineControlID = $('#' + rowid).find('div[name=ObjectTypeName]').attr('id');

    var dataTypeValue = "";
    var dataTypeInlineControlID = $('#' + rowid).find('div[name=DataType]').attr('id');
    var dataTypeControl = $find(dataTypeInlineControlID.substring(0, dataTypeInlineControlID.lastIndexOf("_Div")));
    if (dataTypeControl)
        dataTypeValue = dataTypeControl.getValue();

    var defaultValue = "";
    var defaultValueInlineControlID = $('#' + rowid).find('div[name=DefaultValue]').attr('id');

    if (dataTypeValue == 5) //Object data type
    {
        grid.showCol('ObjectDisplayValue');
        grid.hideCol('DefaultValue');

        $('#' + objectTypeNameInlineControlID).show();
    }
    else {
        grid.hideCol('ObjectDisplayValue');
        grid.showCol('DefaultValue');

        $('#' + objectTypeNameInlineControlID).hide();
    }
}

function BusinessRuleHandler_ObjectDisplayValue_Changed(obj1, obj2, obj3) {
    var defaultValueInlineControlID = $(obj1.target).parents("tr").first().find('div[name=DefaultValue]').attr('id');
    var defaultValueControl = $find(defaultValueInlineControlID.substring(0, defaultValueInlineControlID.lastIndexOf("_Div")));

    var objectDisplayValueInlineControlID = $(obj1.target).parents("tr").first().find('div[name=ObjectDisplayValue]').attr('id');
    var objectDisplayValueControl = $find(objectDisplayValueInlineControlID.substring(0, objectDisplayValueInlineControlID.lastIndexOf("_Div")));

    var objectDefaultValueInlineControlID = $(obj1.target).parents("tr").first().find('div[name=ObjectDefaultValue]').attr('id');
    var objectDefaultValueControl = $find(objectDefaultValueInlineControlID.substring(0, objectDefaultValueInlineControlID.lastIndexOf("_Div")));

    var value = { key: objectDisplayValueControl.get_value().value, text: objectDisplayValueControl.get_value().value };

    objectDefaultValueControl.setValue(objectDisplayValueControl.get_value().value);
    defaultValueControl.setValue(objectDisplayValueControl.get_editor().value);
}

function DispatchRule_renderCompleted() {
    var queryElement = $('span[id$="_ObjectChanges_QueryType"]');

    if (queryElement.length) {
        var queryTypeFld = $find(queryElement.attr('id'));
        if (queryTypeFld) {

            //Clear QueryTypeField selection if all rows in the DispatchDetailsField are deleted
            if (isDelRow) {
                if (this._totalRows == 0) {
                    queryTypeFld.clearValue();
                    isDelRow = false;
                }

            }

            // Set Add New Row availability according to the QueryTypeField state
            DispatchRule_DisableEnableAddRow();

            //Set QueryTypeField Enabled if there are non-empty rows in the DispatchDetailsField else to Disabled
            if (this._totalRows == 0) {
                queryTypeFld.set_Disabled(false);
            }
            else {
                queryTypeFld.set_Disabled(true);
            }

        }
    }

}

function DispatchRule_DisableEnableAddRow() {
    var queryElement = $('span[id$="_ObjectChanges_QueryType"]');
    var gridElement = $('span[id$="_ObjectChanges_DispatchDetails"]');

    if (queryElement.length && gridElement.length) {
        var queryTypeFld = $find(queryElement.attr('id'));
        var grid = $find(gridElement.attr('id'));

        if (queryTypeFld && grid) {
            // Set Add New Row availability according to the QueryTypeField state
            if (grid != null && queryTypeFld != null) {
                if (queryTypeFld.get_IsEmpty()) {
                    $('#add_' + grid._gridID).addClass('ui-state-disabled');
                    grid.set_editingMode("Disabled");
                }
                else {
                    $('#add_' + grid._gridID).removeClass('ui-state-disabled');
                    grid.set_editingMode("Inline");
                }
            }
        }
    }
}

var isDelRow = false;

function DispatchRule_rowDelete(isAtTheEndOfProcessing, rowid, prm2) {
    if (isAtTheEndOfProcessing)
        isDelRow = true;
    return true;
}

//UserQuery grid
// hide excel button
function UserQuery_renderComplete(isAtTheEndOfProcessing) {
    var theGrid = jQuery(this.GridID);
    var jqRowData = theGrid.getRowData();
    var excel = $("span.ui-icon-excel");
    excel.show();
    var found = false;
    for (var i = 0; i < jqRowData.length; i++) {
        if (jqRowData[i]._id_column.indexOf("#empty#") == -1)
            found = true
        break;

    }
    if (!found)
        excel.hide()
}


function Export_TargetImpact_renderCompleted(isAtTheEndOfProcessing) {
    var theGrid = jQuery(this.GridID);
    var jqRowData = theGrid.getRowData();

    var updateCount = 0;
    var newCount = 0;
    for (var i = 0; i < jqRowData.length; i++) {

    }

    var trHeader = $(theGrid.find("tr.jqgroup"));
}

function Export_References_IsRef_renderCompleted(isAtTheEndOfProcessing) {
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    for (var i = 0; i < rowData.length; i++) {
        var record = rowData[i];
        var currentRow;

        if (record._id_column != undefined)
            currentRow = $('tr[id="' + record._id_column + '"]', grid);
        else
            currentRow = $('tr.jqgrow', grid).eq(i);

        var td = $(currentRow).find("td[aria-describedby$='IsRefImage']:first");
        if (record['IsRef'] == "True")
            td.addClass('refImage');
        else
            td.removeClass('refImage');
    }
}

/*CollectSamplingData_VP*/

function PaintSampleTestGrid() {
    var grid = $(this.GridID);
    $("tr.jqgrow td:contains('In Process')", grid).addClass("inprocesscell");
    $("tr.jqgrow td:contains('Pass')", grid).addClass("passcell");
    $("tr.jqgrow td:contains('Fail')", grid).addClass("failcell");
}

function confirmDataChange() {
    return confirm('Sample Data has not been saved.  Press OK to continue');
}

function OpenModelingPageWitinTab(tabContainerId, pageName, title, query) {
    let callStackKey = "";
    var tabcont = $find(tabContainerId);
    if (tabcont)
        callStackKey = tabcont.OpenPage(pageName, query, title, null);
    return callStackKey;
}

function ApprovalTemplate_EditOption_DataChange() {
    var editOptionComponent = $find('ctl00_WebPartManager_ApproversPopUp_ApprovalEntries_EditOption');
    var subOptionComponent = $find('ctl00_WebPartManager_ApproversPopUp_ApprovalEntries_SubstituteOption');
    if (editOptionComponent && subOptionComponent) {
        var optval = editOptionComponent.getValue();
        subOptionComponent.set_ReadOnly(optval == '3');
        if (optval == '3') {
            var prm = { PropertyKey: eval(Camstar.Ajax.DirectUpdateParameterKeys.Data), PropertyValue: '1'/*None*/ };
            subOptionComponent.directUpdate(prm)
        }
    }

}
// CollectLotSamplingData_VP
function AddTooltip() {
    var requiredSamplesLabels = $('.samplestatus .margin-inquiry-required-samples');
    if (requiredSamplesLabels) {
        $.each(requiredSamplesLabels, function () {
            var requiredSamplesLabelText = $(this).text();
            $(this).attr('title', requiredSamplesLabelText);
        });
    }
}

//DocSetMaint Page for view document control
function ViewDocument() {
    var labels = [];
    labels.push({ Name: 'ViewDocumentButton' });
    if (typeof (__page) !== 'undefined') {
        __page.getLabels(labels, function (cb) {
            var ViewDocumentLabel = '';
            if ($.isArray(cb)) {
                $.each(cb, function () {
                    var ViewDocumentLabel = this.Value;
                    $("div.ui-docview > div.document > div").remove();
                    $("div.ui-docview > div.document").append("<div><span title='" + ViewDocumentLabel + "'>" + ViewDocumentLabel + "</span></div>");
                });
            }
            else {
                $("div.ui-docview > div.document > div").remove();
                var ViewDocumentLabel = "View Document";
                $("div.ui-docview > div.document").append("<div><span title='" + ViewDocumentLabel + "'>" + ViewDocumentLabel + "</span></div>");
            }
        });
    }
}

function ApprovalTemplate_EditOption_DataChange() {
    var editOptionComponent = $find('ctl00_WebPartManager_ApproversPopUp_ApprovalEntries_EditOption');
    var subOptionComponent = $find('ctl00_WebPartManager_ApproversPopUp_ApprovalEntries_SubstituteOption');
    if (editOptionComponent && subOptionComponent) {
        var optval = editOptionComponent.getValue();
        subOptionComponent.set_ReadOnly(optval == '3');
        if (optval == '3') {
            var prm = { PropertyKey: eval(Camstar.Ajax.DirectUpdateParameterKeys.Data), PropertyValue: '1'/*None*/ };
            subOptionComponent.directUpdate(prm)
        }
    }

}



//
// BusinessRuleSchedule_VP
//
// constructor
Camstar.WebPortal.BusinessRuleSchedule = function () {
    this.dayOfWeekFieldId = null;
    this.dayOfMonthFieldId = null;
    this.monthOfYearFieldId = null;
    this.recurringDatePatternFieldId = null;
    this.lastDayOfMonthId = null;
};
Camstar.WebPortal.BusinessRuleSchedule.prototype =
    {
        init: function (dayOfWeekField, dayOfMonthField, lastDayOfMonth, monthOfYearField, recurringDatePatternField) {
            this.dayOfWeekFieldId = dayOfWeekField;
            this.dayOfMonthFieldId = dayOfMonthField;
            this.lastDayOfMonthId = lastDayOfMonth;
            this.monthOfYearFieldId = monthOfYearField;
            this.recurringDatePatternFieldId = recurringDatePatternField;

            var me = this;

            $('#' + this.lastDayOfMonthId + ' input:checkbox').click(function (e) { me.clearDayOfMonth(e); });
            $('#' + this.dayOfMonthFieldId + ' input').click(function () { me.textboxOnClick(); });
        },

        clearDayOfMonth: function (e) {
            if (e.target.checked) {
                $('#' + this.dayOfMonthFieldId + ' input').val('');
            }
        },

        clearLastDayOfMonth: function () {
            var $dayOfMonthControl = $('#' + this.dayOfMonthFieldId + ' input');
            $('#' + this.lastDayOfMonthId + ' input').prop('checked', $dayOfMonthControl.val() == '');
            $('#' + this.lastDayOfMonthId + ' input').trigger('change');
        },

        textboxOnClick: function () {
            var me = this;
            if (Camstars.Browser.FireFox) {
                $('#' + this.dayOfMonthFieldId + ' input').focusout(function () {
                    me.clearLastDayOfMonth();
                });
            }
            else {
                $(event.srcElement).blur(function () {
                    me.clearLastDayOfMonth();
                });
            }
        }
    };

//
// InstanceList_WP and DT_Selection_VP
// Note: Changes here will influnce both page/webpart.
//
function InstanceList_RenderComplete() {
    var theGrid = $(this.GridID);
    var grid = this;

    var ctl = $('#ctl00_WebPartManager_InstanceList_WP_InstanceNameTxt');

    if (ctl.find('span.refresh-filter-btn').length == 0) {

        ctl.children('span').first().hide();

        $('<span class="refresh-filter-btn" />')
            .insertAfter($('input[type="text"]', ctl[0]))
            .click(
                function () {
                    var instFilt = $("#ctl00_WebPartManager_InstanceList_WP_InstanceNameTxt");
                    var searchText = $('input[type="text"]', instFilt).val();
                    grid.SetFilter(searchText);
                    grid.Reload(true); // force reload
                });
    }

    // Disable selected rows 
    var tdz = $('td[aria-describedby$="ItemAlreadySelected"]', theGrid);
    if (tdz.length) {
        tdz.each(
            function (ind, td1) {
                var tr = $(td1).parent();
                var slc = $(td1).text() == "True";
                tr.attr('disabled', slc);
                if (slc) {
                    tr.find(':checkbox').each(function () {
                        if (slc) {
                            tr.removeClass("ui-state-highlight");
                            $(this).attr('disabled', 'disabled');
                        }
                        else
                            $(this).removeAttr('disabled');
                        this.checked = slc;
                    });
                }
            });
    }

    theGrid.find('tr.jqgroup').each(
        function (ind, tr) {
            tr = $(tr);
            if (tr.attr('ROR')) {
            }
            else {
                var td = (tr.children('td').first());
                tr = tr.next();
                var revRor = null;

                while (tr.length > 0) {
                    if (tr.find('span.ui-rdo-ror').length > 0) {
                        revRor = tr;
                        break;
                    }
                    tr = tr.next();
                }

                if (revRor != null && revRor.length) {
                    revRor = revRor.find('td[aria-describedby$="Revision"]');
                    td.parent().attr('ROR', revRor.text());
                    td[0].childNodes[1].nodeValue = (td.text() + "  ROR" + getRevisionDelimiter() + " " + revRor.text());
                }
            }
        }
    );

    var $selectAllChk = $('#ctl00_WebPartManager_InstanceList_WP_SelectAllChk_ctl00');
    var mb = this.multiCheckBox;
    if (theGrid.data('isChecking') === 'checking') {
        // don't touch in case of checking progress
        theGrid.data('isChecking', '');
    }
    else {
        // Sync check status in other case
        $selectAllChk.removeAttr('checked').prop('checked', mb.prop('checked'));
        $selectAllChk.trigger('change');
    }

    $selectAllChk.unbind('click').bind('click', function (e) {
        theGrid.data('isChecking', 'checking');
        mb.prop('checked', !this.checked);
        mb.click();
    });

    if (window.onresize != null)
        window.onresize();
}
//DelegationSearch
function Delegation_Delete() {
    var delButton = $(".ui-icon-trash");
    var addButton = $(".ui-icon-add");

    if (!$(delButton).parent().parent().hasClass("ui-state-disabled"))
        delButton.click(function () {
            jConfirmDelegationSetText();
            return false;
        });

    addButton.click(function (e) {
        __doPostBack("AddButton", "");
    });
}

function FireDelegationConfirm(labeltext, lblwarnings) {
    jConfirm(labeltext, lblwarnings, function (r) {
        if (r == true) {
            __doPostBack("DelButton", "");
        }
    }, lblwarnings);
}
function jConfirmDelegationSetText() {
    var labels = [{ Name: 'Lbl_DeleteDelegation' }, { Name: 'Lbl_Warning' }];
    __page.getLabels(labels, function (response) {
        if ($.isArray(response)) {
            var value;
            var warningLbl;
            $.each(response, function () {
                var labelName = this.Name;
                var labelText = this.Value;
                switch (labelName) {
                    case 'Lbl_DeleteDelegation':
                        value = labelText;
                        break;
                    case 'Lbl_Warning':
                        warningLbl = labelText;
                        break;
                    default:
                        break;
                }
            });
            FireDelegationConfirm(value, warningLbl);
        }
        else {
            alert(response.Error);
        }
    });
}

function DataChangeControls() {

    var starrDate = $("#ctl00_WebPartManager_ByDatePopup_StartDate");
    var endDate = $("#ctl00_WebPartManager_ByDatePopup_EndDate");
    starrDate.change(function () {
        __doPostBack("StartDate", "");
    });
    endDate.change(function () {
        __doPostBack("EndDate", "");
    });
}

function DelegationSearch_AddToggleSearchAttr() {
    $("#WebPart_BlankWP3").css("position", "relative").css("left", "1037px");
    $("#ctl00_WebPartManager_TabContainer_WP_DelegateInquery_TabContainer_ctl00").css('border', 'none');//remove border
    $("#ctl00_WebPartManager_TabContainer_WP_DelegateInquery_TabContainer_ctl02").css('border', 'none');

    setTimeout(function () {
        $pkgInquiryTabHeaders = $('div[id$="ctl00_WebPartManager_TabContainer_WP_DelegateInquery_TabContainer"] .ui-tabs-nav > li');

        if ($('#WebPart_DelegateByTaskSearchWP input[id$="_Edit"]').length != 0) {
            $inputFilters_EmployeeAssignmentSearch = $('#WebPart_DelegateByTaskSearchWP input[id$="_Edit"]').attr('togglesearch', 0);
            $.merge($inputFilters_EmployeeAssignmentSearch, $('#WebPart_DelegateByTaskSearchWP input[type$="checkbox"]:not([id$="_DelegateInquiry_PackageOwner_Panl_Stat"])').attr('togglesearch', 0));
            $inputFilters_EmployeeAssignmentSearch.bind('change', function (e) { PackageInquiry_SetTabDetail($(this), e); });
        }
        if ($('#WebPart_DelegateByDateSearchWP input[id$="_Edit"], #WebPart_DelegateByDateSearchWP input[type$="checkbox"]').length != 0) {
            $inputFilters_GeneralSearch = $('#WebPart_DelegateByDateSearchWP input[id$="_Edit"], #WebPart_DelegateByDateSearchWP input[type$="checkbox"]').attr('togglesearch', 1);
            $inputFilters_GeneralSearch.bind('change', function (e) { PackageInquiry_SetTabDetail($(this), e); });
        }

        var isEnabled = PackageInquiry_TabHasContent($pkgInquiryTabHeaders[0]);
        PackageInquiry_TabIcon($pkgInquiryTabHeaders[0], isEnabled);
        isEnabled = PackageInquiry_TabHasContent($pkgInquiryTabHeaders[1]);
        PackageInquiry_TabIcon($pkgInquiryTabHeaders[1], isEnabled);

        if ($inputFilters_GeneralSearch.length == 0)
            PackageInquiry_TabIcon($pkgInquiryTabHeaders[1], true);

    }, 0);
}

//PackageInquiry

var $pkgInquiryTabHeaders = [];
var $inputFilters_EmployeeAssignmentSearch = [];
var $inputFilters_GeneralSearch = [];
var wasClearAll = false;

//function GetMultiSelectFilters(cntrl, cb) {        
//    if (cb) {
//        $.ajax({
//            url: $(this).attr('href'),
//            success: function () {
//                cb(cntrl);
//            },
//            error: function (xhr, status, error) {
//                alert(error);
//            }
//        });
//    }
//}

function PackageInquiry_AddToggleSearchAttr(isClearAll) {
    setTimeout(function () {
        $pkgInquiryTabHeaders = $('div[id$="ctl00_WebPartManager_TabContainer_WP_PackageInquery_TabContainer"] .ui-tabs-nav > li');
        if (isClearAll) {
            $inputFilters_EmployeeAssignmentSearch = [];
            $inputFilters_GeneralSearch = [];
            wasClearAll = true;
        }
        if ($('#WebPart_EmployeeAssignmentSearchWP input[id$="_Edit"]').length != 0) {
            $inputFilters_EmployeeAssignmentSearch = $('#WebPart_EmployeeAssignmentSearchWP input[id$="_Edit"]').attr('togglesearch', 0);
            $.merge($inputFilters_EmployeeAssignmentSearch, $('#WebPart_EmployeeAssignmentSearchWP input[type$="checkbox"]:not([id$="_PackageInquiry_PackageOwner_Panl_Stat"])').attr('togglesearch', 0));
            $inputFilters_EmployeeAssignmentSearch.bind('change', function (e) { PackageInquiry_SetTabDetail($(this), e); });
        }
        if ($('#WebPart_GeneralSearchWP input[id$="_Edit"], #WebPart_GeneralSearchWP .cs-textbox > input[type="text"]').length != 0) {
            $inputFilters_GeneralSearch = $('#WebPart_GeneralSearchWP input[id$="_Edit"], #WebPart_GeneralSearchWP .cs-textbox > input[type="text"]').attr('togglesearch', 1);
            $inputFilters_GeneralSearch.bind('change', function (e) { PackageInquiry_SetTabDetail($(this), e); });
        }

        var isEnabled = PackageInquiry_TabHasContent($pkgInquiryTabHeaders[0]);
        PackageInquiry_TabIcon($pkgInquiryTabHeaders[0], isEnabled);
        isEnabled = PackageInquiry_TabHasContent($pkgInquiryTabHeaders[1]);
        PackageInquiry_TabIcon($pkgInquiryTabHeaders[1], isEnabled);

        if ($inputFilters_GeneralSearch.length == 0 && !wasClearAll)
            PackageInquiry_TabIcon($pkgInquiryTabHeaders[1], true);

        //if ($('[id$="_PackageStatus"]').first().length) {
        //    var pkgStatus = $find($('[id$="_PackageStatus"]').first().attr('id'));
        //    InitializeFilter(pkgStatus);
        //}

    }, 0);

}

function PackageInquiry_SetTabDetail(thisInput, e) {
    var thisTab;
    thisTab = $(thisInput).attr('togglesearch');

    var tab = $pkgInquiryTabHeaders[thisTab];
    var isEnabled = PackageInquiry_TabHasContent(tab);
    PackageInquiry_TabIcon(tab, isEnabled);
}
function PackageInquiry_TabHasContent(thisTab) {
    var hasContent = false;
    var $filters = [];
    switch (thisTab) {
        case $pkgInquiryTabHeaders[0]:
            $filters = $inputFilters_EmployeeAssignmentSearch;
            break;
        case $pkgInquiryTabHeaders[1]:
            $filters = $inputFilters_GeneralSearch;
            break;
    }
    //check if specific filters in set are empty
    $.each($filters, function () {
        if ($(this).is(':text') && ($(this).val() != '') || ($(this).is(':checkbox') && $(this).is(':checked'))) {
            hasContent = true;
            return true;
        }
    });
    return hasContent;
}
function PackageInquiry_TabIcon(thisTab, showIcon) {
    if (showIcon) {
        $(thisTab).addClass('ui-icon-combine');
    }
    else {
        $(thisTab).removeClass('ui-icon-combine');
    }
}
function RefreshModelingSelectedDefaultPkg(packageName) {
    var modelingCDOListControl = $find($("[id$='ModelingMainVP_CDOList']").attr("id"));
    if (modelingCDOListControl)
        modelingCDOListControl.refreshDefaultChangePkgValue(packageName);
}

// Package Multiple Inquiry

function PackageSearchMultiple_AddToggleSearchAttr() {
    setTimeout(function () {
        $pkgInquiryTabHeaders.push(null);
        $pkgInquiryTabHeaders.push($('div[id$="ctl00_WebPartManager_TabContainer_WP_PackageInquery_TabContainer"] .ui-tabs-nav > li'));

        if ($('#WebPart_GeneralSearchWP input[id$="_Edit"], #WebPart_GeneralSearchWP .cs-textbox > input[type="text"]').length != 0) {
            $inputFilters_GeneralSearch = $('#WebPart_GeneralSearchWP input[id$="_Edit"], #WebPart_GeneralSearchWP .cs-textbox > input[type="text"]').attr('togglesearch', 1);
            $inputFilters_GeneralSearch.bind('change', function (e) { PackageInquiry_SetTabDetail($(this), e); });
            var isEnabled = PackageInquiry_TabHasContent($pkgInquiryTabHeaders[1]);
            PackageInquiry_TabIcon($pkgInquiryTabHeaders[1], isEnabled);
        }
    }, 0);

    //setTimeout(function () {
    //    if ($('[id$="_PackageStatus"]').first().length) {
    //        var pkgStatus = $find($('[id$="_PackageStatus"]').first().attr('id'));
    //        InitializeFilter(pkgStatus);
    //    }
    //}, 200);
}

// Activation Inquiry

function ActivationInquiry_AddToggleSearchAttr() {
    setTimeout(function () {
        $pkgInquiryTabHeaders = $('div[id$="ctl00_WebPartManager_TabContainer_WP_Control0"] .ui-tabs-nav > li');

        if ($('#WebPart_BlankWP input[id$="_Edit"], #WebPart_BlankWP .cs-textbox > input[type="text"], #WebPart_BlankWP .cs-date > input[type="text"]').length != 0) {
            $inputFilters_EmployeeAssignmentSearch = $('#WebPart_BlankWP input[id$="_Edit"],  #WebPart_BlankWP .cs-textbox > input[type="text"], #WebPart_BlankWP .cs-date > input').attr('togglesearch', 0);
            $inputFilters_EmployeeAssignmentSearch.bind('change', function (e) { PackageInquiry_SetTabDetail($(this), e); });
        }

        var isEnabled = PackageInquiry_TabHasContent($pkgInquiryTabHeaders[0]);
        PackageInquiry_TabIcon($pkgInquiryTabHeaders[0], isEnabled);

        //$.ajax({
        //    url: $(this).attr('href'),
        //    success: function () {
        //        if ($('[id$="_PackageStatus"]').first().length) {
        //            var pkgStatus = $find($('[id$="_PackageStatus"]').first().attr('id'))
        //            GetMultiSelectFilters(pkgStatus, InitializeFilter);
        //        }
        //    },
        //    error: function (xhr, status, error) {
        //        alert(error);
        //    }
        //});

        //$.ajax({
        //    url: $(this).attr('href'),
        //    success: function () {
        //        if ($('[id$="_ActivationState"]').first().length) {
        //            var activationState = $find($('[id$="_ActivationState"]').first().attr('id'))
        //            GetMultiSelectFilters(activationState, InitializeFilter);
        //        }
        //    },
        //    error: function (xhr, status, error) {
        //        alert(error);
        //    }
        //});    

    }, 0);
}

//function InitializeFilter(multiSelectFilter) {
//    $(multiSelectFilter).unbind('change');
//    if (typeof (__page) !== 'undefined') {
//        var lbls = [{ Name: 'Lbl_Name' }, { Name: 'Lbl_GridDisplayingToOfItems' }];
//        multiSelectFilter._setLabelValues(lbls, __page, multiSelectFilter);
//    }
//}

//Master Data Catalog

function MasterDataCatalogDtl_RenderCompleted() {
    var grid = $(this.GridID);
    var allchbx = $(':checkbox', grid);
    var isUserClick = true;
    if (allchbx.length) {
        allchbx.unbind('.mdcatalog');
        allchbx.bind('click.mdcatalog', function () {
            if (isUserClick) {
                var tr = $(this).closest("tr");
                var trchbx = tr.find(':checkbox');

                for (var i = 0; i < trchbx.length; i++)
                    if (trchbx[i].checked && trchbx[i] != this) {
                        isUserClick = false;
                        trchbx[i].click();
                    }
            }
            isUserClick = true;
        });
    }
}

function MasterDataCatalogDtl_editingStarted(isAtTheEndOfProcessing, rowid) {
    var currentRow = $('tr[id="' + rowid + '"]', $(this.GridID));
    var allchbx = $(':checkbox', currentRow);
    if (allchbx.length) {
        allchbx.click(
            function () {
                var chek = this.checked;
                var tr = $(this).closest("tr");
                var tdchbx = tr.find('td[checked=checked]');
                var inpchbx = tr.find(':checkbox');
                inpchbx.prop("checked", false);
                tdchbx.removeAttr("checked");
                this.checked = chek;
            });
    }
}

function setChangeMgtSaveState() {
    var modelingList = this.parent.$find($(this.parent.document.body).find("[id$='CDOList']").attr('id'));
    this.parent.$find(modelingList.get_changeMgtSaveId()).setState(true);
}

function click_SelectCDO() {
    var objList = $("#ctl00_WebPartManager_InstanceList_WP_ObjectsList_Panl");
    var key = window.cdoKey;
    var selectedCDO = $("li[key='" + key + "']", objList);
    selectedCDO.click();
}

function OwnerOnly(control, buttonControl) {
    $('#' + control).removeAttr("onclick");
    $('#' + buttonControl).click();
}

/* ActivationImpact_VP */
function PaintImpactGridRows() {
    var differenceGrid = $(this.GridID);
    var labels = [];
    labels.push({ Name: 'NewAction' });
    labels.push({ Name: 'UpdateButton' });
    labels.push({ Name: 'Lbl_Error' });
    labels.push({ Name: 'ActivationImpact_Incompatible' });
    if (typeof (__page) !== 'undefined') {
        __page.getLabels(labels, function (cb) {
            var newLabel = '';
            var updateLabel = '';
            var errorLabel = '';
            var incompatibleLabel = '';
            if ($.isArray(cb)) {
                $.each(cb, function () {
                    var labelname = this.Name;
                    var labelvalue = this.Value;
                    switch (labelname) {
                        case 'NewAction':
                            newLabel = labelvalue;
                            break;
                        case 'UpdateButton':
                            updateLabel = labelvalue;
                            break;
                        case 'Lbl_Error':
                            errorLabel = labelvalue;
                            break;
                        case 'ActivationImpact_Incompatible':
                            incompatibleLabel = labelvalue;
                        default:
                            break;
                    }
                });
                var rowData = differenceGrid.getRowData();
                for (var i = 0; i < rowData.length; i++) {
                    var record = rowData[i];
                    var currentRow;

                    if (record._id_column != undefined)
                        currentRow = $('tr[id="' + record._id_column + '"]', differenceGrid);
                    else
                        currentRow = $('tr.jqgrow', differenceGrid).eq(i);

                    var tdImpact = $(currentRow).find('td[aria-describedby$="_Impact"]');
                    var impactText = tdImpact.text();
                    switch (impactText) {
                        case newLabel:
                            tdImpact.addClass('change-mgmt-impact-new');
                            break;
                        case updateLabel:
                            tdImpact.addClass('change-mgmt-impact-update');
                            break;
                        case errorLabel:
                            tdImpact.addClass('change-mgmt-incompatible');
                            break;
                        case incompatibleLabel:
                            tdImpact.addClass('change-mgmt-incompatible');
                            break;
                        default:
                            break;
                    }
                }
            }
            else {
                alert(cb.Error);
            }
        });
    }
}

/*MDL_InstanceHeader_WP*/

function labelWrapper() {

    var instanceHeader = $('#ctl00_WebPartManager_MDL_InstanceHeader_NameTxt');
    var instanceHeaderChild = $('#ctl00_WebPartManager_MDL_InstanceHeader_NameTxt > :first-child');
    var instanceHeaderText = $('#ctl00_WebPartManager_MDL_InstanceHeader_NameTxt > :first-child').text();

    instanceHeader.addClass('labelSpan');
    instanceHeaderChild.addClass('labelTextWrapper')
        .attr('title', instanceHeaderText)
        .tooltip({
            track: true,
            content: function () { return '<div class="nvtooltip labelContent" required="true"><b>' + instanceHeaderText + '</b></div>'; },
            show: null
        });
}

function resetIsDirty() {
    var wp = $('[id$=MDL_InstanceHeader_ObjectChanges_FilterTags]').parents('.webpart').first();
    if (wp.length) {
        var webPart = window.$find(wp.attr('id') + '_UIComponent');
        if (typeof (webPart) !== 'undefinded' && webPart !== null) {
            webPart.set_dirtyFlagTrigger(false);
        }
    }
}
function filterTags_changed(e) {
    if (typeof (e) !== 'undefined' && e !== null) {
        var webPart = e.data.webPart;
        webPart.set_dirtyFlagTrigger(true);
    }
}

/*Process Timer*/
function onProcessTimerCheckBoxClick(source, ctrls) {
    $(ctrls).each(
        function () {
            var el = $find(this);
            el.clearValue();
            el.set_Disabled(!$(source).prop("checked"));
        }
    );
}

function onAllowTxnClick(source, ctrls) {
    var isDisabled = $(source).find("input[type=radio]:checked").val() == 1;
    $(ctrls).each(
        function () {
            var el = $('#' + this).find("input[type=checkbox]");
            if (el.prop("checked"))
                el.click();

            el.parent().prop("disabled", !isDisabled ? "" : "disabled");
            el.prop("disabled", !isDisabled ? "" : "disabled");
        }
    );
}

function activeTimerClick() {
    if (__page.isMobilePage()) {
        $(window).bind("orientationchange", activeTimerOrientationChange);
    }

    if (__page.isMobilePage() && screen.availWidth < 1024) {
        var cbid = $(".cs-command-sidebar").prop('id');
        if (cbid) {
            var sb = $find(cbid);
            if (sb) {
                sb.panelToggle("active-timers");
            }
        }
    }
    else {
        var iframe = $('.timers-flyout');
        if (iframe.length) {
            iframe.remove();
        }
        else {
            var div_list = [];
            var containerControl = $find("ctl00_WebPartManager_ContainerStatusWP_R2_ContainerStatus_ContainerName");
            var container;
            if (containerControl) {
                container = containerControl.get_Data();
            }
            var $timerControl = $("#ctl00_WebPartManager_ContainerStatusWP_R2_ActiveTimer");
            var dc = encodeURIComponent(JSON.stringify({ "SelectedContainerNameDM": container }));
            var url = "TimersListPopupDesktop_VP.aspx?IsFloatingFrame=2&IsChild=true&responsive=true&slideout=timers&DataContracts=" + dc;
            var $di = $(document.createElement('div')).addClass('ui-flyout timers-flyout');

            $di.append($(document.createElement('div')).addClass('pointer'));
            var $flyoutContainer = $(document.createElement('div')).addClass('ui-flyout-container ui-resizable');
            $di.append($flyoutContainer);
            $flyoutContainer.append(document.createElement('iframe'));
            $('iframe', $di)
                .prop("src", url)
                .prop("name", "TimersFlyout")
                .css('width', "100%").css("height", "100%");
            div_list.push($di);
            if (__page.isMobilePage()) {
                $timerControl.css('max-height', '25px');
            }
            $timerControl.append(div_list);
        }
    }
}

function activeTimerReload() {
    window.frameElement.contentWindow.location.reload();
}

function activeTimerClose() {
    var tf = $(window.parent.document.body).find(".timers-flyout");
    if (tf.length) {
        tf.remove();
    }
}

function activeTimerOrientationChange() {
    var iPadLandscapeWidth = 1024;
    if (screen.availWidth === iPadLandscapeWidth && window.orientation != 0) {
        var cbid = $(".cs-command-sidebar").prop('id');
        if (cbid) {
            var sb = $find(cbid);
            if (sb) {
                sb.panelToggle(false);
            }
        }
    }
    else {
        var iframe = $('.timers-flyout');
        if (iframe.length) {
            iframe.remove();
        }
    }

    $(window).unbind("orientationchange", activeTimerOrientationChange);
}

/* Manage Document Attachments */
function cutOverflowForGridHeader() {
    var $th = $('.ui-jqgrid-labels');
    $.each($th, function () {
        $.each(this.cells, function () {
            $.jgrid.cutOverflowText.call(this);
        });
    });
}


/*ProcessTimerSearch*/
function ProcessTimerSearch_Filter() {
    $inputFilters = $('#WebPart_ProcessTimerSearchWP input[id$="_Edit"], #WebPart_ProcessTimerSearchWP .cs-textbox > input[type="text"], #WebPart_ProcessTimerSearchWP .cs-date > input');


    setTimeout(ProcessTimerSetup, 500);
    ProcessTimerSetup();
}

function ProcessTimerSetup() {
    $('#WebPart_ProcessTimerSearchWP input[name*="Container"]').off('keypress').on('keypress', function (e) {
        ProcessTimerSearch_KeyPress($(this), e);
    });

    $inputFilters.off('change').on('change', function (e) {
        ProcessTimerSearch_SearchButtonStatus($(this), e);
    });

    //wire update for clear all
    $('#WebPart_ActionButtons_WP input[id$="_ClearAllButton"]').on('click', function () {
        $inputFilters.val('');
        var searchBtn = $find("ctl00_WebPartManager_ActionButtons_WP_SearchButton");
        searchBtn.set_Disabled(true);
    });

    ProcessTimerPicklistSetup();
}

function ProcessTimerPicklistSetup() {
    const selector = '#WebPart_ProcessTimerSearchWP .cs-picklist > input[type="text"]';
    const elements = document.querySelectorAll(selector);

    Array.from(elements).forEach(item => {
        item.addEventListener("change", ProcessTimerSearch_ChangeValue);
        item.addEventListener("focus", ProcessTimerSearch_ChangeValue);
        item.addEventListener("blur", ProcessTimerSearch_ChangeValue);
    });
}

function ProcessTimerSearch_ChangeValue(e) {
    ProcessTimerSearch_SearchButtonStatus($(this), e);
}

function ProcessTimerSearch_KeyPress(thisInput, e) {
    var code = (e.keyCode ? e.keyCode : e.which);
    if (code == 13) {
        ProcessTimerSearch_SearchButtonStatus(thisInput, e);
        var searchBtn = $find("ctl00_WebPartManager_ActionButtons_WP_SearchButton");
        setTimeout(function () { searchBtn.get_element().click(); }, 0);
    }
}

function ProcessTimerSearch_SearchButtonStatus(thisInput, e) {
    var isEnabled = false;
    var filter = $('#WebPart_ProcessTimerSearchWP input[id$="_Edit"], #WebPart_ProcessTimerSearchWP .cs-textbox > input[type="text"], #WebPart_ProcessTimerSearchWP .cs-date > input');
    if (e) {
        if (e.type == 'keypress' && e.which >= 48 && ($(thisInput).val() == ''))
            isEnabled = true;
        else if (e.type == 'change' && $(e.target).val() != '')
            isEnabled = true;
    }

    // Check if all the filter value is empty
    $.each(filter, function () {
        if ($(this).val() != '') {
            isEnabled = true;
        }
    });

    var searchBtn = $find("ctl00_WebPartManager_ActionButtons_WP_SearchButton");
    if (searchBtn) {
        searchBtn.set_Disabled(!isEnabled);
    }
}

function ProcessTimer_CellColor() {
    var me = this;
    RefreshProcessTimerInquiryGrid(me);
    var interval = setInterval(RefreshProcessTimerInquiryGrid.bind(null, me), me._durationUpdateInterval);
    for (var i = 1; i < interval; i++) {
        clearInterval(i - 1);
    }
}

function RefreshProcessTimerInquiryGrid(gridObject) {
    var me = gridObject;
    var grid = $(me.GridID);
    var rowData = grid.getRowData();
    for (var i = 0; i < rowData.length; i++) {
        var record = rowData[i];
        var currentRow;

        if (record._id_column != undefined)
            currentRow = $('tr[id="' + record._id_column + '"]', grid);
        else
            currentRow = $('tr.jqgrow', grid).eq(i);

        var curDate = new Date();
        var curDateUtc = new Date(curDate.getUTCFullYear(), curDate.getUTCMonth(), curDate.getUTCDate(), curDate.getUTCHours(), curDate.getUTCMinutes(), curDate.getUTCSeconds());

        var tdDuration = $(currentRow).find('td[jqgrid-duration-autoupdate]');
        var timeToMin = '';
        var timeToMax = '';
        var tdEndTimeGMT = $(currentRow).find('td[aria-describedby$="_EndTimeGMT"]');
        var endTimeGMT = tdEndTimeGMT.text();

        if (tdDuration && (!endTimeGMT || endTimeGMT.trim().length === 0)) {
            var durationTimer = new DurationTimer();
            durationTimer.interval = me._durationUpdateInterval;
            var timers = durationTimer.getTimerFromColumns(me, currentRow);
            if (timers.min) {
                timeToMin = durationTimer.formatDurationRel(timers.min['endTime']);
            }
            if (timers.max) {
                timeToMax = durationTimer.formatDurationRel(timers.max['endTime']);
            }
        }
        var tdMin = $(currentRow).find("td[aria-describedby$='_TimetoMin']");
        if (timeToMin) {
            tdMin.text(timeToMin);
            tdMin.attr('title', timeToMin);
        }
        var tdWarnMinTime = $(currentRow).find("td[aria-describedby$='_MinEndWarningTimeGMT']").text();
        var tdMinTimeColor = $(currentRow).find("td[aria-describedby$='_MinTimeColor']").text();
        var tdMinWarningTimeColor = $(currentRow).find("td[aria-describedby$='_MinWarningTimeColor']").text();

        // When the time to Min exceeds the MinTime color will be applied
        if (timeToMin != null && timeToMin.trim().length > 0 && timeToMin.indexOf('+') > -1) {
            tdMin.css("background-color", tdMinTimeColor);
        }
        // When the time to Min is < 5 mins color will be applied
        else if (timeToMin != null && timeToMin.trim().length > 0 && timeToMin.indexOf('+') === -1) {
            var minWarn = new Date(tdWarnMinTime);
            if (curDateUtc.getTime() > minWarn.getTime()) {
                tdMin.css("background-color", tdMinWarningTimeColor)
            }
        }

        var tdMax = $(currentRow).find("td[aria-describedby$='_TimetoMax']");
        if (timeToMax && (!endTimeGMT || endTimeGMT.trim().length === 0)) {
            tdMax.text(timeToMax);
            tdMax.attr('title', timeToMax);
        }

        var tdMaxTimeColor = $(currentRow).find("td[aria-describedby$='_MaxTimeColor']").text();
        var tdWarnMaxTime = $(currentRow).find("td[aria-describedby$='_MaxEndWarningTimeGMT']").text();
        var tdMaxWarningTimeColor = $(currentRow).find("td[aria-describedby$='_MaxWarningTimeColor']").text();
        // When the time to Max exceeds the MaxTime color will be applied
        if (timeToMax != null && timeToMax.trim().length > 0 && timeToMax.indexOf('+') > -1) {
            tdMax.css("background-color", tdMaxTimeColor);
        }
        // When the time to Max is < 5 mins yellow color will be applied
        else if (timeToMax != null && timeToMax.trim().length > 0 && timeToMax.indexOf('+') === -1) {
            var maxWarn = new Date(tdWarnMaxTime);
            if (curDateUtc.getTime() > maxWarn.getTime()) {
                tdMax.css("background-color", tdMaxWarningTimeColor);
            }
        }
    }
}

function SetConfirmationCheckBoxes(minEndTxnExecute, maxEndTxnExecute, minCheckboxId, maxCheckboxId) {
    //need to wait radiobuttonlist initialization
    setTimeout(function () {
        var endTxnExecutelabelMin = $(minEndTxnExecute).parent();
        var isConfirmationMinCheckbox = $(minCheckboxId);
        isConfirmationMinCheckbox.detach();
        isConfirmationMinCheckbox.css({ "display": "block", "padding": "0px 0px 5px 26px" });
        endTxnExecutelabelMin.after(isConfirmationMinCheckbox);

        var endTxnExecutelabelMax = $(maxEndTxnExecute).parent();
        var isConfirmationMaxCheckbox = $(maxCheckboxId);
        isConfirmationMaxCheckbox.detach();
        isConfirmationMaxCheckbox.css({ "display": "block", "padding": "0px 0px 5px 26px" });
        endTxnExecutelabelMax.after(isConfirmationMaxCheckbox);
    }, 200);
}

function MaximizeCanvasPopup() {
    pop.GetCallerPage().pop.maximize();
}

function AssignReviewContent_CollaboratorsGrid_RowSelected() {
    var rowId = this._last_selectedRowID;
    var rowData = this.getRowData(rowId);
    var specialInstraction = $("#ctl00_WebPartManager_CollaboratorStatusWP_SpecialInstructions_ctl00")[0];
    var isRowSelected = $('tr[id="' + rowId + '"]', this.get_element()).attr('aria-selected') == 'true';
    if (specialInstraction) {
        if (isRowSelected && rowData.SpecialInstructions) //action on the selected row
            specialInstraction.innerHTML = rowData.SpecialInstructions;
        if (!isRowSelected && rowId)             //action on the deselected row
            specialInstraction.innerHTML = "";
    }
}

function AssignChangePkgCompletePopup_RowSelect() {
    var rowId = this._last_selectedRowID;
    var warningLabel = $('#ctl00_WebPartManager_BlankWP_WarningMessageLabel');
    warningLabel.css("display", "none");
    var message = warningLabel.attr("defaultlabel") + ".";
    var completeButton = $('[name$="CompleteButton"]');
    var isRowSelected = $('tr[id="' + rowId + '"]', this.get_element()).attr('aria-selected') == 'true';
    completeButton.off("click");
    if (!isRowSelected && rowId) { //action on the deselected row
        completeButton.on("click", function () {
            __page.displayStatus(message, "Warning");
            return false;
        });
    }
}

// SplitQty
function DisableContainerNameColumn() {
    var check = $('[id$=_Split_AutoNumber]:first-child').find('input:first');	
    var isChecked = $(check).is(':checked');

    if (isChecked) {

        var inlineCol = $('td[aria-describedby$="_ToContainerName"]');
        var html;
        if (inlineCol) {
            $.each(inlineCol, function () {
                $(this).find('input:first').prop("disabled", true);
            });
        }
    }
}

// Slitting
function ToContainerGrid_editingStarted(isAtTheEndOfProcessing, rowid, prm2) {
	var currentRow = $('tr[id="' + rowid + '"]', $(this.GridID));	
	var inlineQtyCol = $(currentRow).find('td[aria-describedby$="_Qty"]');
	var inlineHiddenQtyCol = $(currentRow).find('#ctl00_WebPartManager_SlittingWP_ToContainerGrid_HiddenQty_InlineEditorControl_ctl01');
	var ContainerQty = $('#ctl00_WebPartManager_ContainerStatusWP_R2_ContainerStatus_Qty').text();
	inlineQtyCol.text(ContainerQty);
	inlineHiddenQtyCol.val(ContainerQty);
}

function removeHyperLink(row) {
    var isExistTD = $(row).find("td[aria-describedby$='IsModelingObjectExist']:first");
    var displayTD = $(row).find("td[aria-describedby$='DisplayedName']:first");
    var isImportedTD = $(row).find("td[aria-describedby$='IsImported']:first");

    if (isExistTD.text() == "0" || isImportedTD.text() === "1") {
        td = $(row).find("td[aria-describedby$='DisplayedNameHyperlink']:first");
        td.text(displayTD.text());
    }
}

function AddEditRemoveReadOnly() {
    var addButton = $('[id^=add_]');
    addButton.click(function () {
        ChildContainerGrid_AddEdit();
    });

    var editButton = $('[id^=edit_]');
    editButton.click(function () {
        ChildContainerGrid_AddEdit();
    });
}

function ChildContainerGrid_AddEdit() {
    var level_editing = $('#ctl00_WebPartManager_ChildContainerDetailsEditing_WP_Level_Editing').find('div:first').find('div:first');
    var levelDropdown = "<input name=\"ctl00$WebPartManager$ChildContainerDetailsEditing_WP$Level_Editing$Imbt\" id=\"ctl00_WebPartManager_ChildContainerDetailsEditing_WP_Level_Editing_Imbt\" title=\"Hide or show the list of instances\" src=\"/CamstarPortal/Themes/Camstar/Images/Icons/icon-picklist-down-16x16.png\" onclick=\"return false;\" style=\"cursor:;\" type=\"image\"></input>";

    var generateParentName = $('#ctl00_WebPartManager_TwoLevelStart_MainDetailsWP_Details_AutoNumber').find('input:first').is(':checked');
    var generateChildName = $('#ctl00_WebPartManager_TwoLevelStart_ChildDetailsWP_Details_AutoNumberChild').find('input:first').is(':checked');
    var firstRow = $('#ctl00_WebPartManager_TwoLevelStart_ChildDetailsWP_ChildDetails_ChildContainersGrid').find('tr:nth-child(2)');

    $('#ctl00_WebPartManager_ChildContainerDetailsEditing_WP_GenerateNames').attr('checked', generateParentName || generateChildName);

    var isGridEmpty = false;
    if ($(firstRow).prop('id').indexOf('empty') >= 0) {
        isGridEmpty = true;
    }

    if (isGridEmpty) {
        $('#ctl00_WebPartManager_ChildContainerDetailsEditing_WP_ContainerName_Editing').find('input:first').prop('disabled', false);
        level_editing = $('#ctl00_WebPartManager_ChildContainerDetailsEditing_WP_Level_Editing:first-child').find('div:first').find('div:first');

        if (!level_editing.find('input:nth-child(2)')) {
            level_editing.append(levelDropdown);
        }

    }
    else {
        if (generateParentName) {
            $('#ctl00_WebPartManager_ChildContainerDetailsEditing_WP_ContainerName_Editing').find('input:first').prop('disabled', true);

            if (!level_editing.find('input:nth-child(2)')) {
                level_editing.append(levelDropdown);
            }
        }

        if (generateChildName) {
            $('#ctl00_WebPartManager_ChildContainerDetailsEditing_WP_ContainerName_Editing').find('input:first').prop('disabled', true);
            level_editing.attr('readonly', 'readonly');

            var level_editingTextbox = level_editing.find('input:first');
            level_editingTextbox.attr('allowfreeformtextentry', false);
            level_editingTextbox.prop('disabled', true);

            level_editing.find('input:nth-child(2)').remove();
        }

        if (!generateParentName && !generateChildName) {
            $('#ctl00_WebPartManager_ChildContainerDetailsEditing_WP_ContainerName_Editing').find('input:first').prop('disabled', false);

            if (!level_editing.find('input:nth-child(2)')) {
                level_editing.append(levelDropdown);
            }
        }
    }
}

function DisallowedTransactionsGrid_render_complete() {
    var isInViewMode = $("input[id *= BriefDescription]")[0].hasAttribute("readonly");
    var gridrows = $('#' + this._gridID).find("tr.ui-widget-content.jqgrow.ui-row-ltr");
    if (isInViewMode) {
        gridrows.attr("editable", "-1");
    }
    else {
        gridrows.removeAttr("editable");
    }
}
function AssignReviewContentRenderComplited() {
    if (this._isHorizonStyle) {
        var grid = $find("gview_ctl00_WebPartManager_PackageSelections_WP_ObjectTypeGrid");
        $(".ui-jqgrid-bdiv", grid).css("height", "460px");
    }
}

//Record Production Event page R2
function FailureModesGrid_deleteElementChargeToWorkflowStep() {
    var parent = document.querySelector(".webpart-record-prod-event-toggle");
    if (parent) {
        var toWorkflowControl = document.querySelector("#ctl00_WebPartManager_RecordProdEventAddlFieldsWP_ChargeToWorkflowStep");
        var parentRow = $(toWorkflowControl).parent().parent();
        var indexOfControl = $(toWorkflowControl).parent().index();
        var element = document.querySelector("#ctl00_WebPartManager_RecordProdEventAddlFieldsWP_ChargeToWorkflowStep_Step");
        var emptyCell = null;
        if (parentRow.children().length > indexOfControl + 2) {
            emptyCell = parentRow.children()[indexOfControl + 1]
        }
        if (element && emptyCell) {
            element.parentNode.removeChild(element);
            emptyCell.appendChild(element);
        }
    }
}

//Move CargeToStep textbox from WorkflowNavigator control to bottom. Webpart R2
function MoveChargeToStep() {
    var element = ($("#" + $(".general-workflow")[0].id + "_Step"))[0];
    var cell = $(".moveChargeToStepClass")[0];
    if (element && cell) {
        element.parentNode.removeChild(element);
        cell.appendChild(element);
    }
}

function SetGridWidth(width) {
    if (this._isHorizonStyle) {
        var grid = $find(this._gridID);
        grid.SetWidth(width);
    }
}



//IntelligenceReportObjectControl
function IntelligenceReportObject() {
    $(".form-container").addClass("IntelligenceReportObjectControl");
}

function SaveSPCChartHTML(path) {
    var el = $(".spcChartPanel").clone();
    el.find(".highcharts-tooltip").css("visibility","hidden");
    var html = el.html();
    var blob = new Blob([html], { type: 'text/csv' });
    if (window.navigator.msSaveOrOpenBlob) {
        window.navigator.msSaveBlob(blob, path);
    }
    else {
        var elem = window.document.createElement('a');
        elem.href = window.URL.createObjectURL(blob);
        elem.download = path;
        document.body.appendChild(elem);
        elem.click();
        document.body.removeChild(elem);
    }

}
//AttachDocumentManagement_VP override
function ColorDetachedDocs() {
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    for (var i = 0; i < rowData.length; i++) {
        var record = rowData[i];
        var currentRow;

        if (record._id_column != undefined)
            currentRow = $('tr[id="' + record._id_column + '"]', grid);
        else
            currentRow = $('tr.jqgrow', grid).eq(i);

        var row = record['IsDetached'];
        if (row == 'True') {
            currentRow.addClass('failcell');
        }
    }
    cutOverflowForGridHeader();
}

function AddDrillDownIndicator(row, jqRows, grid) {
    var td = $("td:visible[role='gridcell']", jqRows[parseInt(row['RowIndex']) % jqRows.length]).first();
    if (!td.hasClass('drill-down-indicator-wrapper')) {
        td.addClass('drill-down-indicator-wrapper');
        td.append(document.createElement("div"));
        $("div", td).append(document.createElement("span"));
        $("div span", td).append(document.createElement("i"));
        $("div span i", td).addClass('drill-down-indicator-true-2');
        $("div span i", td).on("click", function (event) {
            grid._onSelectRow(td.closest('tr')[0].id, true, { target: td });
            return true;
        });
    }
}


function MfgAuditTrailInquiry_MfgAuditTrailGrid_renderCompleted(gridID) {
    var theGrid = jQuery(this.GridID);
    var jqGrid = this;
    var jqRowData = theGrid.getRowData();
    var jqRows = theGrid.find('.jqgrow');
    if (gridID.endsWith("MainLineGrid")) {
        $(jqRowData).each(function () {
            if (this['HasChild'] != '') {
                AddDrillDownIndicator(this, jqRows, jqGrid)
            }
        });
    }

    setTimeout(function (e) {
        AuditTrailSetGridHeight(gridID)
    }, 0);

    window.addEventListener("resize", debounce(function (e) {
        AuditTrailSetGridHeight(gridID)
    }));
}

function AuditTrailSetGridHeight(gridID) {
    var finalHeight;

    if ($("[id$=SearchTxnBtn]").is(":visible")) {
        var searchBtnOffset = $("[id$=SearchTxnBtn]").offset();
        var gridOffset = $(gridID).parents(".ui-jqgrid").find(".ui-jqgrid-bdiv").offset();
        finalHeight = searchBtnOffset.top - gridOffset.top;
    }
    else {
        var formContainerHeight = $(gridID).parents(".form-container").height();
        var gridHeader = $(gridID).parents(".ui-jqgrid").find(".ui-jqgrid-hdiv");
        finalHeight = formContainerHeight - gridHeader.offset().top - gridHeader.height() - 91;
    }

    $(gridID).setGridHeight(finalHeight);
}

//For MfgAuditTrailInquiry_ContainerStatus, MfgAuditTrailInquiry_QualityObjectStatus and ResourceAuditTrailInquiry_ResourceStatus
function MfgAuditTrailInquiry_ContainerStatus_renderCompleted(gridID) {
    setTimeout(function (e) {
        SetContainerStatusGridHeight(gridID);
    }, 0);

    window.addEventListener("resize", debounce(function (e) {
        SetContainerStatusGridHeight(gridID);
    }));
}

function SetContainerStatusGridHeight(gridID) {
    var searchBtnOffset = $("[id$=SearchTxnBtn]").offset();
    var gridOffset = $(gridID).parents(".ui-jqgrid").find(".ui-jqgrid-bdiv").offset();
    var statusGridPager = $(gridID).parents(".ui-jqgrid").find(".ui-pager-control").height();
    var finalHeight = searchBtnOffset.top - gridOffset.top - statusGridPager - 27;

    $(gridID).setGridHeight(finalHeight);
}

function debounce(func) {
    var timer;
    return function (event) {
        if (timer) clearTimeout(timer);
        timer = setTimeout(func, 0, event);
    };
}

addClickHandlerToGridRows = function ($grid, handler) {
    if (($grid !== 'undefined') && ($grid.length > 0) && (handler !== 'undefined')) {
        $grid.on('click', 'tr:not([empty-row])', handler);
    }
}
isNotEmpty = function (htmlString) {
    if (htmlString.replace('&nbsp;').length > 0) {
        return true;
    } else {
        return false;
    }
}

findTitleForBreadcrumbCommand = function (row) {
    var title = 'Unknown'; // in case that all fields will be empty

    if (typeof row != 'undefined') {
        var $visibleColumns = $(row).find('td:visible');
        $visibleColumns.each(function () {
            var html = $(this).html();
            if (isNotEmpty(html)) {
                if (html.length > 40) {
                    html = html.substring(0, 40) + "&#8230;";
                }
                title = html;
                return false;
            }
        });
        return title;
    }
}

getGridLevelForSelector = function (selector) {
    var gridsSelectors = getGridsSelectors();
    return _getIndexByValueFromArray(gridsSelectors, selector);
}

bindBreadcrumbToGrid = function (breadcrumb, gridSelector) {
    var $grid = $(gridSelector);
    var gridLevel = getGridLevelForSelector(gridSelector);

    var gridStateReset = function (commandArgs, context, gridLevel) {
        __page.postback(context._element.id, gridLevel);
    };
    var gridClickHandler = function (event) {
        if (!$(event.target).hasClass('drill-down-indicator-true-2'))
            return false;

        var selectedRow = event.currentTarget;
        var commandTitle = findTitleForBreadcrumbCommand(selectedRow);
        var cmd = new JavascriptCommand(commandTitle, gridStateReset, gridLevel);

        breadcrumb.addCommand(cmd);
    };
    addClickHandlerToGridRows($grid, gridClickHandler);
}


function clearBreadcrumbCommands(breadcrumb) {
    var shouldClear = "shouldClearBreadcrumbCommands_" + __page.get_CallStackKey();
    if ((typeof breadcrumb != 'undefined') && (typeof window[shouldClear] != 'undefined') && (window[shouldClear] == true)) {
        breadcrumb.clearStorage();
    }
}

function getGridsSelectors() {
    var tabName = "TransactionPanelTabName_" + __page.get_CallStackKey();
    return [{ key: "mainlineGrid", value: "table[id$='AuditTrailInquiry_MainLineGrid']" },
    { key: "summaryGrid", value: "table[id$='AuditTrailInquiry_" + window[tabName] + "SummaryGrid']" },
    { key: "detailsGrid", value: "table[id$='AuditTrailInquiry_" + window[tabName] + "Grid']" }];
}
function auditTrailBreadcrumbInit(breadcrumbId) {
    var breadcrumb = $find(breadcrumbId);
    clearBreadcrumbCommands(breadcrumb);
    limitBreadcrumbLevel(breadcrumb);
    bindBreadcrumbToGrids(breadcrumb, getGridsSelectors());

    updateAttachmentButtonState();
}

function bindBreadcrumbToGrids(breadcrumb, gridsSelectors) {

    const BreadcrumbLevels = { MainlineGrid: 1, SummaryGrid: 2, DetailsGrid: 3 };
    var maxLevel = "MaxBreadcrumbLevel_" + __page.get_CallStackKey();

    if (typeof breadcrumb == 'undefined') {
        console.log('There is no ability to bind Breadcrumb to grids. Breadcrumb is not defined!');
        return;
    }

    bindBreadcrumbToGrid(breadcrumb, _getValueByKeyFromArray(gridsSelectors, 'mainlineGrid'));

    if (typeof window[maxLevel] != 'undefined') {
        if (window[maxLevel] > BreadcrumbLevels.MainlineGrid) {
            bindBreadcrumbToGrid(breadcrumb, _getValueByKeyFromArray(gridsSelectors, 'summaryGrid'));
        }
        if (window[maxLevel] > BreadcrumbLevels.SummaryGrid) {
            bindBreadcrumbToGrid(breadcrumb, _getValueByKeyFromArray(gridsSelectors, 'detailsGrid'));
        }
    } else {
        console.log('MaxBreadcrumbLevel is not defined!');
    }
}

function numberingRuleEdit() {
    $("#ctl00_WebPartManager_DetailsGroupWP_ExcludedValuesGrid_ExcludeValue_InlineEditorControl_Edit").attr("maxlength", 1);

    $("#ctl00_WebPartManager_DetailsGroupWP_ExcludedValuesGrid_ExcludeValue_InlineEditorControl_Edit").focus(function () {
        this.value = this.value.toUpperCase();

    });

    $("#ctl00_WebPartManager_DetailsGroupWP_ExcludedValuesGrid_ExcludeValue_InlineEditorControl_Edit").focusout(function () {
        if (this.value.length == 0)
            $(this.closest("td")).attr("cell-not-entered", "");
        else
            $(this.closest("td")).removeAttr("cell-not-entered", "");
    });

    $("#ctl00_WebPartManager_DetailsGroupWP_ExcludedValuesGrid_ExcludeValue_InlineEditorControl_Imbt").focus(function () {
        $(this.closest("td")).removeAttr("cell-not-entered", "");
    });


    $("#ctl00_WebPartManager_DetailsGroupWP_ExcludedValuesGrid_ExcludeValue_InlineEditorControl_Edit").on("input", function () {
        $(this).val($(this).val().replace(/[^a-zA-Z]/g, ''));
    });

    $("#ctl00_WebPartManager_DetailsGroupWP_ObjectChanges_MaximumValueAlphaNumeric_ctl00").change(function () {
        this.value = this.value.toUpperCase();
    });

    $("#ctl00_WebPartManager_DetailsGroupWP_ObjectChanges_LastAssignedSequenceAlphaNumeric_ctl00").change(function () {
        this.value = this.value.toUpperCase();
    });
}

function _getValueByKeyFromArray(myArray, searchTerm) {
    for (var i = 0; i < myArray.length; i++) {
        if (myArray[i]['key'] === searchTerm) return myArray[i]['value'];
    }
    return 'undefined';
}

function _getIndexByValueFromArray(myArray, searchTerm) {
    for (var i = 0; i < myArray.length; i++) {
        if (myArray[i]['value'] === searchTerm) return i;
    }
    return 'undefined';
}

function isHorizontalScrollbarOn(selector) {
    var $scrollArea = $(selector);
    $scrollArea.scrollLeft(1);
    if ($scrollArea.scrollLeft() !== 0) {
        $scrollArea.scrollLeft(0);
        return true;
    }
    return false;
}

function limitBreadcrumbLevel(breadcrumb) {
    var maxLevel = "MaxBreadcrumbLevel_" + __page.get_CallStackKey();
    if ((typeof window[maxLevel] != 'undefined') && (typeof breadcrumb != 'undefined')) {
        var currLevel = breadcrumb._commands.length;
        var difference = currLevel - window[maxLevel];
        for (i = 0; i < difference; i++) {
            breadcrumb.removeCommand();
        }
    }
}

function showSecondaryFilters(show) {
    let div = $('#ctl00_WebPartManager_PanelFiltersSearchWP_CollapsibleSectionsAccordion');
    if (div.length) {
        let childs = div.children("[index=3]");
        if (childs.length) {
            if (show) {
                $(childs[0]).show();
                if ($(childs[0]).hasClass("opened"))
                    $(childs[1]).show();
            } else {
                $(childs[0]).hide();
                $(childs[1]).hide();
            }
        }
    }
}

// FilterPanel
function initFiltersPanel(searchTxnBtnId, breadcrumbId, showSecondary) {
    var searchTxnBtn = $find(searchTxnBtnId);
    var breadcrumb = $find(breadcrumbId);

    if (typeof breadcrumb != 'undefined') {
        addClickHandlerToJQObject(searchTxnBtn, breadcrumb.clearStorage);
    }
}

function addClickHandlerToJQObject(JQObject, handler) {
    if ((JQObject.length > 0) && (typeof handler != 'undefined')) {
        $(JQObject).click(handler);
    }
}
// end FilterPanel

function updateAttachmentButtonState() {
    var $viewAttachmentBtn = $("input[id$=AttachmentButton");
    if ($viewAttachmentBtn.length > 0) {
        var btnTooltip = $viewAttachmentBtn.attr('title');;
        var $tr = $("div[id$=_SummaryDetailsSummaryGrid").find("table.ui-pg-table.navtable tr");
        $tr.append("<td class='ui-pg-button ui-corner-all'><div class='ui-pg-div' onclick=attachmentButtonClick()><span class='ui-icon ui-icon-attachment' title='" + btnTooltip + "'></span></div></td>");
    }
}

function attachmentButtonClick() {
    $('input[id$=AttachmentButton').click();
}

//gview_ctl00_WebPartManager_BlankWP_isMaterialRequestAcknowledge_ServiceDetails
// adjusted grid to be responsive(adjust to screen size) in horizon
function isMaterialRequestAcknowledge_renderComplete() {
    if (this._isHorizonStyle) {
        var rowheight = document.getElementById('ctl00_WebPartManager_BlankWP_isMaterialRequestAcknowledge_isInventoryLocation').clientHeight + 10;
        var grid = $find("gview_ctl00_WebPartManager_BlankWP_isMaterialRequestAcknowledge_ServiceDetails");
        var finalheight = "calc(70vh - " + rowheight + "px)";
        $(".ui-jqgrid-bdiv", grid).css("height", finalheight);
    }
}
function isMaterialRequest_renderComplete() {
    if (this._isHorizonStyle) {
        var rowheight = document.getElementById('ctl00_WebPartManager_BlankWP_isMaterialRequest_isInventoryLocation').clientHeight + 10;
        var grid = $find("gview_ctl00_WebPartManager_BlankWP_isMaterialRequest_ServiceDetails");
        var finalheight = "calc(70vh - " + rowheight + "px)";
        $(".ui-jqgrid-bdiv", grid).css("height", finalheight);
    }
}

function resetDirtyAfterSaveAndTest() {
    if ((typeof shouldResetDirty != 'undefined') && (shouldResetDirty != false)) {
        __page.resetDirty();
    }
}

function ConfirmMacroFileParsinfForSPC(control) {
    $('#' + control).removeAttr("onclick");
    $("#" + control + " input:checkbox")[0].click();
}

function CancelMacroFileParsinfForSPC(inlineSPCControl, macrofileControl) {
    $('#' + inlineSPCControl).removeAttr("onclick");
    $("#" + macrofileControl + " input:text").val("");
    $("#" + inlineSPCControl + " input:checkbox")[0].click();
}
function CorrectFileNameInAttachDocTable()
{
    var notEmptyRows = $("table[id$='WP_AttachedDocuments']").find("tr:not([empty-row])");
    var cellsWithFileName = notEmptyRows.children("td[aria-describedby$='_FileName']");

    cellsWithFileName.each(function (index, element)
    {
        let fname = element.innerHTML;
		let fileName = GetOriginalFileName(fname);
		let extension = GetExtension(fname);
		if (!fileName)
			fileName = fname;
        let origFileName = fileName.endsWith(extension) ? fileName : fileName + extension;

        if (origFileName.length > 0)
        {
            element.innerHTML = origFileName;
            element.title = origFileName;
        }
    });
}

function GetOriginalFileName(fname)
{
    const underscore = "_";
    const emptyString = "";
    const minNumbChars = 3;

    if ((typeof fname == "undefined") || (fname.length < minNumbChars))
    {
        return emptyString;
    }

    let underscoreLastPos = fname.lastIndexOf(underscore);
    if (underscoreLastPos > 1)
    {
        let underscorePreLastPos = fname.lastIndexOf(underscore, underscoreLastPos - 1);
        if (underscorePreLastPos > 0)
        {
            return fname.substring(0, underscorePreLastPos);
        }
    }
    return emptyString;
}

function GetExtension(fname)
{
    if (typeof fname != "undefined")
    {
        let dotLastPos = fname.lastIndexOf(".");
        if (dotLastPos > -1)
        {
            return fname.substring(dotLastPos);
        }
    }
    return "";
}

function ShowHideSPCFilterPanel(wpSearch) {
    let $pageContainer = wpSearch.closest(".page-container");
    $pageContainer.addClass("search-panel-hidden");
    $(".spcChartViewerControl").prepend("<div class='custom-funnel'><span id='spc-funnel' class='ui-icon funnel-icon OpenClass'></span></div>");
    $(".OpenClass").on("click", function () {
        $(".custom-funnel").remove();
        $pageContainer.removeClass("search-panel-hidden");
        let $btn = wpSearch.find(".close-button-desktop");
        if ($btn.length == 0) {
            $btn = $("<span class='close-button-desktop'></span>");
            wpSearch.find(".header-search-panel").append($btn);
            $btn.click(function () {
                wpSearch.toggle('slide', 500,
                    function () {
                        ShowHideSPCFilterPanel(wpSearch);
                    });
            });
        }
        $(".spc-filter-panel").show('slide', {
            direction: 'left'
        }, 500);
    })
}

function SearchLayout_AddSlideoutTooglerSPCRealtimeMonitoring(controlID) {
    let $wpSearch = $(".common-search-panel");

    let $btn = $wpSearch.find(".close-button-desktop");

    if ($btn.length == 0 && $wpSearch[0].clientWidth > 0) {
        $btn = $("<span class='close-button-desktop'></span>");
        $wpSearch.find(".header-search-panel").append($btn);

        $btn.click(function () {
            $wpSearch.toggle('slide', 500,
                function () {
                    if ($wpSearch.css("display") == "none") {
                        ShowHideSPCFilterPanel($wpSearch);
                    }
                });
        });
    }
    else if ($btn.length == 0 && $wpSearch[0].clientWidth == 0) {
        ShowHideSPCFilterPanel($wpSearch);
    }
}

function InlineSPCRealTimeMonitoringScript(param) {
    var IntervalIDs = document.getElementsByName('ctl00$WebPartManager$SPCChartsViewerWP$IntervalId$ctl00');

    var currentIntervals = IntervalIDs[0].value;
    var currentIntervalArray = currentIntervals.split(";");
    for (x = 0; x < currentIntervalArray.length; x++) {
        window.clearInterval(currentIntervalArray[x]);
    }

    var dataUpdateInterval = setInterval(function () {
        $("#ctl00_WebPartManager_SPCChartsViewerWP_RefreshChart").click();
    }, getSliderIntervalValue() * 1000);

    IntervalIDs[0].value = dataUpdateInterval;

    $(function () {
        $("#refreshIntervalValue").val(getSliderIntervalValue() + " s");
        $("#ctl00_WebPartManager_SPCChartsViewerWP_refreshSlider").slider({
            range: "min",
            value: getSliderIntervalValue(),
            min: 10,
            max: 600,
            step: 10,
            animate: true,
            slide: function (event, ui) {
                $("#refreshIntervalValue").val(ui.value + " s");
                $("#ctl00_WebPartManager_SPCChartsViewerWP_IntervalValue_ctl00").val((ui.value));

                var currentIntervals = IntervalIDs[0].value;
                var currentIntervalArray = currentIntervals.split(";");
                for (x = 0; x < currentIntervalArray.length; x++) {
                    window.clearInterval(currentIntervalArray[x]);
                }

                clearInterval(dataUpdateInterval);
                var dataUpdateInterval = setInterval(function () {
                    $("#ctl00_WebPartManager_SPCChartsViewerWP_RefreshChart").click();
                }, ui.value * 1000);

                IntervalIDs[0].value = dataUpdateInterval;
            }
        });
    });
};

function getSliderIntervalValue() {
    return $("#ctl00_WebPartManager_SPCChartsViewerWP_IntervalValue_ctl00")[0].value;
}

/* Supervisor and Technician Jobs search results grid */
function Job_GridColumn_renderComplete(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{
    var theGrid = jQuery(this.GridID);
    var jqRowData = theGrid.getRowData();
    var GridId = this.GridID;

    $(jqRowData).each(function () {
        try {
            var rowClass;
            var symtompCode = this['SymptomCode'];
            var causeCode = this['CauseCode'];
            var repairCode = this['RepairCode'];
            var prefix = 'general-color-'; // "ui-jqgrid-row-";

            if (this['JobStatus'] != null) {

                if (this['JobStatus'] == "CREATED") {
                    rowClass = 'lightpink';
                }
                else if (this['JobStatus'] == "ASSIGNED") {
                    rowClass = 'yellow';
                }
                else if (this['JobStatus'] == "ACKNOWLEDGED") {
                    rowClass = 'orange';
                }
                else if (this['JobStatus'] == "ACTIVE") {
                    rowClass = 'lightblue';
                }
                else if (this['JobStatus'] == "INPROGRESS") {
                    rowClass = 'lightgreen';
                }


                var arrHeader = new Array(GridId + "_btnResource", GridId + "_btnHistory", GridId + "_btnSyptom", GridId + "_btnCause", GridId + "_btnRepair", "ends");
                var arrToolTip = new Array("Resource Parts - " + this["Name"], "Job History - " + this["JobOrder"], this["Name"] + " Symptom Code", this["Name"] + " Cause Code", this["Name"] + " Repair Code");
                var i = 0;
                if (rowClass) {
                    var jqRows = theGrid.find('.jqgrow');
                    var trCurrent = $(jqRows).find("td[aria-describedby$='_RN'][title='" + this["RN"] + "']:first").parent("tr:first");
                    $(trCurrent).find('td:eq(2)').addClass(prefix + rowClass);

                    $("td", trCurrent).each(
                        function () {
                            if (("#" + this.attributes["aria-describedby"].value) == arrHeader[i]) {
                                $(this).addClass('ui-jqgrid-column-image');
                                $(this).removeAttr("style");

                                if (arrHeader[i] == (GridId + "_btnSyptom") && symtompCode == "")
                                    $(this).html("");
                                else if (arrHeader[i] == (GridId + "_btnCause") && causeCode == "")
                                    $(this).html("");
                                else if (arrHeader[i] == (GridId + "_btnRepair") && repairCode == "")
                                    $(this).html("");
                                else
                                    $(this).attr("title", arrToolTip[i]);

                                i++;
                            };
                        }

                    );

                }
            }
        } catch (ex) {}
    });

    setTimeout(function (e) {
        AuditTrailSetGridHeight(theGrid);
    }, 0);

    window.addEventListener("resize", debounce(function (e) {
        AuditTrailSetGridHeight(theGrid);
    }));

}

/* Technician & Inventory Requests search result grid */
function Part_GridColumn_renderComplete(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{
    /*
    This function is called to display colours to the request status coloumn. It will help to determine which colour to put based on the value in request status column.        
    */
    var theGrid = jQuery(this.GridID);
    var jqRowData = theGrid.getRowData();
    var GridId = this.GridID;

    $(jqRowData).each(function () {
        try {
            var rowClass;
            if (this["RequestStatus"] == "REQUESTED")
                rowClass = 'orange';
            else if (this["RequestStatus"] == "ACKNOWLEDGED")
                rowClass = 'yellow';
            else if (this["RequestStatus"] == "ASSIGNED")
                rowClass = 'lightblue';
            else if (this["RequestStatus"] == "COMPLETED")
                rowClass = 'lightgreen';
            else if (this["RequestStatus"] == "CANCELLED")
                rowClass = 'lightpink';
            else
                rowClass = null;

            if (rowClass) {
                var jqRows = theGrid.find('.jqgrow');
                var trCurrent = $(jqRows).find("td[aria-describedby$='_RN'][title='" + this["RN"] + "']:first").parent("tr:first");
                $(trCurrent).find('td:eq(6)').addClass('general-color-' + rowClass);

                var arrHeader = new Array(GridId + "_ResourcePartsBtn", GridId + "_RequestHistoryBtn", GridId + "_JobHistoryBtn", "ends");
                var arrToolTip = new Array("Resource Parts - " + this["Name"], "Request History - " + this["RequestOrder"], "Job History - " + this["JobOrder"]);
                var jobOrder = this["JobOrder"];
                var i = 0;
                $("td", trCurrent).each(
                    function () {
                        if (("#" + this.attributes["aria-describedby"].value) == arrHeader[i]) {
                            if (arrHeader[i] == GridId + "_JobHistoryBtn" && jobOrder == "") {
                                ($(this).find("input")).hide();
                            }
                            else {
                                $(this).attr("title", arrToolTip[i]);
                            }
                            i++;
                        };
                    }
                );
            }

        } catch (ex) { }
    });

    setTimeout(function (e) {
        AuditTrailSetGridHeight(theGrid);
    }, 0);

    window.addEventListener("resize", debounce(function (e) {
        AuditTrailSetGridHeight(theGrid);
    }));
}

/* Part Maintenance search result grid */
function PartMaint_GridColumn_renderComplete(isAtTheEndOfProcessing, rowid, prm2)
{
    var theGrid = jQuery(this.GridID);
    var jqRowData = theGrid.getRowData();
    var GridId = this.GridID;

    $(jqRowData).each(function () {
        try {
            var jqRows = theGrid.find('.jqgrow');
            var trCurrent = $(jqRows).find("td[aria-describedby$='_RN'][title='" + this["RN"] + "']:first").parent("tr:first");
            var toolTip = "Part History - " + this["Name"];
            $("td", trCurrent).each(
                function () {
                    if (("#" + this.attributes["aria-describedby"].value) == GridId + "_PartHistoryBtn") {
                        $(this).attr("title", toolTip);
                    };
                }
            );
        } catch (ex) { }
    });

    setTimeout(function (e) {
        AuditTrailSetGridHeight(theGrid);
    }, 0);

    window.addEventListener("resize", debounce(function (e) {
        AuditTrailSetGridHeight(theGrid);
    }));
}

/* Maintenance Class Activation status column*/
function MaintClassesGrid_renderComplete(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{
    var theGrid = jQuery(this.GridID);
    var jqRowData = theGrid.getRowData();
	
	const threshold = 5000;
	
    for (var x = 0; x < jqRowData.length; x++) {
        try {
			if (jqRowData[x].Activated == "True"){
				if (jqRowData[x].ResourceCount != null && jqRowData[x].ActiveCount !=null)
				{
					const totalCount = parseInt(jqRowData[x].ResourceCount);
					const activeCount = parseInt(jqRowData[x].ActiveCount);
					const resourcesToActivate = Math.min(threshold, totalCount - activeCount);

					const gridRowId = jqRowData[x]._id_column;
					
					if(totalCount == activeCount) {
						theGrid.jqGrid('setCell', gridRowId, 'Status', '');
					} 
					else if(resourcesToActivate > 0) {
						theGrid.jqGrid('setCell', gridRowId, 'Status', resourcesToActivate + ' resource(s) to be activated');
					} 
				}
			}
        } catch (ex) { 
			console.error('Error processing row:', ex);
		}
    }
}

/* Maintenance Class Activation grid*/
function MaintClassesGrid_activateClick(isAtTheEndOfProcessing, rowid, prm2) {
    var theGrid = jQuery(this.GridID);
    var jqRowData = theGrid.getRowData();
	
	const threshold = 5000;
	
	// Get all the elements based on rowid
	var $tdCheckbox = $('tr#' + rowid + ' td[aria-describedby="ctl00_WebPartManager_MaintenanceClassActivationWP_MaintClassesGrid_Activated"]');
	var $tdStatus = $('tr#' + rowid + ' td[aria-describedby="ctl00_WebPartManager_MaintenanceClassActivationWP_MaintClassesGrid_Status"]');
	var totalCount = parseInt($('tr#' + rowid + ' td[aria-describedby="ctl00_WebPartManager_MaintenanceClassActivationWP_MaintClassesGrid_ResourceCount"]').text());
	var activeCount = parseInt($('tr#' + rowid + ' td[aria-describedby="ctl00_WebPartManager_MaintenanceClassActivationWP_MaintClassesGrid_ActiveCount"]').text());
	
	// Find the checkbox
    var $checkbox = $tdCheckbox.find('input[type="checkbox"]');
    
    // Check the current state of the checkbox
    var isChecked = $checkbox.is(':checked');
    
    if (isChecked) {
        // Activation logic
        const resourcesToActivate = Math.min(threshold, totalCount - activeCount);
        if (resourcesToActivate > 0) {
            $tdStatus.text(resourcesToActivate + " resource(s) to be activated");
        } else {
            $tdStatus.text(''); // Clear status if no resources left to be activated
        }
    } else {
        // Deactivation logic
        const resourcesToDeactivate = Math.min(threshold, activeCount);
        if (activeCount > 0) {
            $tdStatus.text(resourcesToDeactivate + " resource(s) to be deactivated");
        } else {
            $tdStatus.text(''); // Clear status if no active resources
        }
    }
}