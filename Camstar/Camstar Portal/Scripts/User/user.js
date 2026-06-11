/*
***************************************************************************
Copyright 2025 Siemens
This file should include all user-defined CSS classes. 
Classes defined in this file will over-ride any classes of the
same name written in any of the Camstar supplied CSS files.
***************************************************************************
*/

/* Complete Maint row coloring */
function SS_CompleteMaint_GridColumn_renderComplete() //renderCompleted
{
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    for (var i = 0; i < rowData.length; i++) {
        var record = rowData[i];
        var currentRow;

        if (record._id_column != undefined)
            currentRow = $('tr[id="' + record._id_column + '"]', grid);
        else
            currentRow = $('tr.jqgrow', grid).eq(i);

        if (record['ChecklistId'] != "") {
            if (record['Employee'] == "")
                currentRow.addClass('ss-general-color-lightgreen');
            else
                currentRow.addClass('ss-general-color-orange');
        }
    }
}

/* LotModifyWafers, others? */
function SS_WaferDetailsGrid_rowAdd(isAtTheEndOfProcessing, rowid, prm2) {
    /*
    This function is called when the row add button (plus sign) is clicked (the row data has NOT been added to the grid yet?)
    */
    var index = parseInt(rowid);
    if (index >= 0) {
        var btnRowAdd = document.getElementById('ctl00_WebPartManager_SS_LotModifyWP_rowAddButton');  // was: ctl00_WebPartManager_SS_LotModifyWafersWP_rowAddButton
        btnRowAdd.click();
    }
    return false;
}

/* LotModifyWafers, others? */
function SS_WaferdetailsGrid_rowDelete(isAtTheEndOfProcessing, rowid, prm2) {
    /*
    This function is called when the row delete button (trash can) is clicked (the row data has NOT been deleted from the grid yet?)
    */
    var index = parseInt(rowid);
    if (index >= 0) {
        var btnRowDelete = document.getElementById('ctl00_WebPartManager_SS_LotModifyWP_rowDeleteButton');  // was: ctl00_WebPartManager_SS_LotModifyWafersWP_rowDeleteButton
        btnRowDelete.click();
    }
    return false;
}


/* LotCarriersSetup, EqpCarriersSetup, others? */
function SS_CarrierGrid_rowAdd(isAtTheEndOfProcessing, rowid, prm2) {
    /*
    This function is called when the row add button (plus sign) is clicked (the row data has NOT been added to the grid yet?)
    */
    var index = parseInt(rowid);
    if (index >= 0) { // carrier name to be added
        var btnRowAdd = document.getElementById('ctl00_WebPartManager_SS_CarriersSetupWP_rowAddButton');
        btnRowAdd.click();
    }
    return false;
}

/* LotCarriersSetup, EqpCarriersSetup, others? */
function SS_CarrierGrid_rowDelete(isAtTheEndOfProcessing, rowid, prm2) {
    /*
    This function is called when the row delete button (trash can) is clicked (the row data has NOT been deleted from the grid yet?)
    */
    var index = parseInt(rowid);
    if (index >= 0) {
        var btnRowDelete = document.getElementById('ctl00_WebPartManager_SS_CarriersSetupWP_rowDeleteButton');
        btnRowDelete.click();
    }
    return false;
}

/* LotShip, LotShipCancel, others? */
function SS_ContainerGrid_rowDelete(isAtTheEndOfProcessing, rowid, prm2) {
    /*
    This function is called when the row delete button (trashcan) is clicked, the row data has NOT been removed from the grid yet.
    If the grid has VisibleRows value set, the rowData.length value will never be 0, even if there is no actual data in the grid, hence the else condition
    The 'else' condition will check to see if there is a container value in the 2nd row, if present means that there will still be row data after the delete so DO NOT call the clear function.        
    */
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var btnClear = document.getElementById('ctl00_WebPartManager_ButtonsBar_ResetAction');

    if (rowData.length <= 0 || rowData[1].Lot == "") {
        btnClear.click();
        return false;
    } else {
        return true;
    }
}

/* WIP Materials Setup */
function SS_EqpMaterialsSetup_MaterialPart_SetFocusOnScan(e, fieldname) {
    var keynum;

    if (window.event)   // IE
        keynum = e.keyCode;
    else if (e.which) // Netscape/Firefox/Opera
        keynum = e.which;
    if (keynum == 13) {    // 13 is ENTER key
        var btn = document.getElementById('ctl00_WebPartManager_SS_WIPEqpMaterialsSetupWP_WIPEqpMaterialsSetup_AddMaterialPartButton');
        var MaterialPart = document.getElementsByName("ctl00$WebPartManager$SS_WIPEqpMaterialsSetupWP$WIPEqpMaterialsSetup_MaterialPart$Edit")

        if (MaterialPart[0].value == "")
            MaterialPart[0].focus();
        else if (fieldname == "MaterialPartName")
            btn.click();
        else if (fieldname == "MaterialPartRev")
            btn.click();

        return false;
    }
    return true;
}

function WIPEqpMaterialsSetup_Details_renderCompleted(isAtTheEndOfProcessing, rowid, prm2) {

    var theGrid = jQuery(this.GridID);

    var titleMaterial = document.getElementById('ctl00_WebPartManager_SS_WIPEqpMaterialsSetupWP_WIPEqpMaterialsSetup_MaterialPartButton').title;
    var titleProduct = document.getElementById('ctl00_WebPartManager_SS_WIPEqpMaterialsSetupWP_WIPEqpMaterialsSetup_ProductButton').title;

    //Assign tool tips to each row icon
    var btnsMaterialPart = $(theGrid).find('input[name="btnMaterialPart_cellAction_btnMaterialPart"]');
    var btnsProductAvailable = $(theGrid).find('input[name="btnProductAvailable_cellAction_btnProductAvailable"]');

    btnsMaterialPart.each(function () { $(this).attr('title', titleMaterial); });
    btnsProductAvailable.each(function () { $(this).attr('title', titleProduct); });
}

/* WIP Main */
function SS_WIPMain_LotDataGrid_rowDelete(isAtTheEndOfProcessing, rowid, prm2) {
    /*
        This function is called when the row delete button (trashcan) is clicked, the row data has NOT been removed from the grid yet.
        If the grid has VisibleRows value set, the rowData.length value will never be 0, even if there is no actual data in the grid, hence the else condition
        The 'else' condition will check to see if there is a container value in the 2nd row, if present means that there will still be row data after the delete so DO NOT call the clear function.        
    */
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var btnClear = document.getElementById('ctl00_WebPartManager_SS_WIPMain_HomeWP_Main_ClearButton');
    var btnDelete = document.getElementById('ctl00_WebPartManager_SS_WIPMain_ControllerWP_WIPMain_Controller_ContainerDelete');
    var ContainerToDelete = document.getElementsByName('ctl00$WebPartManager$WIPMain_LotItemWP_LotGridWP$WIPMain_LotItem_ContainerToDelete$ctl00');
    var FoundContainer;
    if (rowData.length <= 0) {
        btnClear.click();
        return false;
    }
    else {
        if (rowData[1].Container == "" && rowid == "000000") {
            btnClear.click();
            return false;
        }
        else {
            for (var i = 0; i < rowData.length; i++) {
                if (rowData[i]['_id_column'] == rowid) {
                    FoundContainer = rowData[i];
                }

                if (FoundContainer && FoundContainer.Container != "") {
                    ContainerToDelete[0].value = FoundContainer.Container;
                    btnDelete.click();
                    return true;
                    break;
                }
            }
        }
    }
}

/* WIPLot Bins Txn */
function SS_WIPLotBins_Summation() {
    var BinQty = document.getElementsByName('ctl00$WebPartManager$SS_WIPLotBinsWP$TotalBinQty$ctl00');
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var TotalBinQty = 0;
    var Qty;

    for (var x = 0; x < rowData.length; x++) {
        currentRow = $("tr", grid)[x + 1];

        Qty = Number(rowData[x].BinQty);
        if (Qty > 0) {
            TotalBinQty += Qty;
        }
    }
    BinQty[0].value = TotalBinQty;
}

/* WIPLot Wafer Bins Txn */
function SS_WIPLotWaferBins_Summation() {
    var BinQty = document.getElementsByName('ctl00$WebPartManager$SS_WIPLotBinsWP$CurrentWaferBinQty$ctl00');
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var TotalBinQty = 0;
    var Qty;

    for (var x = 0; x < rowData.length; x++) {
        currentRow = $("tr", grid)[x + 1];

        Qty = Number(rowData[x].BinQtyEx);
        if (Qty > 0) {
            TotalBinQty += Qty;
        }
    }
    BinQty[0].value = TotalBinQty;
}

/* WIPItemRejects Txn */
function SS_WIPItemRejects_Summation() {
    var ReworkableRejectQty = document.getElementsByName('ctl00$WebPartManager$SS_WIPItemRejectsWP$WIPItemRejects_TotalReworkableRejectQty$ctl00');
    var UnidentifiableRejectQty = document.getElementsByName('ctl00$WebPartManager$SS_WIPItemRejectsWP$WIPItemRejects_TotalUnidentifiableRejectQty$ctl00');
    var WaferRejectsQty = document.getElementsByName('ctl00$WebPartManager$SS_WIPItemRejectsWP$WIPItemRejects_TotalWaferRejectsQty$ctl00');
    var grid = $(this.GridID);
    var rowData = grid.getRowData();

    var TotalReworkableRejectQty = 0;
    var TotalUnidentifiableRejectQty = 0;
    var TotalWaferRejectsQty = 0;
    var Qty;

    for (var x = 0; x < rowData.length; x++) {
        currentRow = $("tr", grid)[x + 1];

        Qty = Number(rowData[x].ReworkableRejectQty);
        if (Qty > 0) {
            TotalReworkableRejectQty = TotalReworkableRejectQty + Qty;
        }

        Qty = Number(rowData[x].UnidentifiableRejectQty);
        if (Qty > 0) {
            TotalUnidentifiableRejectQty = TotalUnidentifiableRejectQty + Qty;
        }

        Qty = Number(rowData[x].WaferRejectsQty);
        if (Qty > 0) {
            TotalWaferRejectsQty = TotalWaferRejectsQty + Qty;
        }
    }


    if (typeof (ReworkableRejectQty[0]) != 'undefined' && ReworkableRejectQty[0] != null) {
        ReworkableRejectQty[0].value = TotalReworkableRejectQty;
    }

    if (typeof (UnidentifiableRejectQty[0]) != 'undefined' && UnidentifiableRejectQty[0] != null) {
        UnidentifiableRejectQty[0].value = TotalUnidentifiableRejectQty;
    }

    WaferRejectsQty[0].value = TotalWaferRejectsQty;
}

/* WIPLot Rejects Txn */
function SS_WIPLotRejects_Summation() {
    var RejectQty = document.getElementsByName('ctl00$WebPartManager$SS_WIPLotRejectsWP$WIPLotRejects_RejectQty$ctl00');
    var DefectQty = document.getElementsByName('ctl00$WebPartManager$SS_WIPLotRejectsWP$WIPLotRejects_DefectQty$ctl00');
    var ReworkableRejectQty = document.getElementsByName('ctl00$WebPartManager$SS_WIPLotRejectsWP$WIPLotRejects_ReworkableRejectQty$ctl00');
    var UnidentifiableRejectQty = document.getElementsByName('ctl00$WebPartManager$SS_WIPLotRejectsWP$WIPLotRejects_UnidentifiableRejectQty$ctl00');
    var ValidRejectQty = document.getElementsByName('ctl00$WebPartManager$SS_WIPLotRejectsWP$WIPLotRejects_ValidRejectQty$ctl00');
    var InvalidRejectQty = document.getElementsByName('ctl00$WebPartManager$SS_WIPLotRejectsWP$WIPLotRejects_InvalidRejectQty$ctl00');
    var BonusBackRejectQty = document.getElementsByName('ctl00$WebPartManager$SS_WIPLotRejectsWP$WIPLotRejects_BonusBackRejectQty$ctl00');
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var TotalRejectQty = 0;
    var TotalDefectQty = 0;
    var TotalReworkableRejectQty = 0;
    var TotalUnidentifiableRejectQty = 0;
    var TotalValidRejectQty = 0;
    var TotalInvalidRejectQty = 0;
    var TotalBonusBackRejectQty = 0;
    var Qty;

    for (var x = 0; x < rowData.length; x++) {
        currentRow = $("tr", grid)[x + 1];

        Qty = Number(rowData[x].RejectQty);
        if (Qty > 0) {
            TotalRejectQty = TotalRejectQty + Qty;
        }

        Qty = Number(rowData[x].DefectQty);
        if (Qty > 0) {
            TotalDefectQty = TotalDefectQty + Qty;
        }

        Qty = Number(rowData[x].ReworkableRejectQty);
        if (Qty > 0) {
            TotalReworkableRejectQty = TotalReworkableRejectQty + Qty;
        }

        Qty = Number(rowData[x].UnidentifiableRejectQty);
        if (Qty > 0) {
            TotalUnidentifiableRejectQty = TotalUnidentifiableRejectQty + Qty;
        }

        Qty = Number(rowData[x].ValidRejectQty);
        if (Qty > 0) {
            TotalValidRejectQty = TotalValidRejectQty + Qty;
        }

        Qty = Number(rowData[x].InvalidRejectQty);
        if (Qty > 0) {
            TotalInvalidRejectQty = TotalInvalidRejectQty + Qty;
        }

        Qty = Number(rowData[x].BonusBackRejectQty);
        if (Qty > 0) {
            TotalBonusBackRejectQty = TotalBonusBackRejectQty + Qty;
        }
    }
    RejectQty[0].value = TotalRejectQty;

    /* Check whether datagrid columns are visible or hidden */
    if (typeof (DefectQty[0]) != 'undefined' && DefectQty[0] != null) {
        DefectQty[0].value = TotalDefectQty;
    }

    if (typeof (ReworkableRejectQty[0]) != 'undefined' && ReworkableRejectQty[0] != null) {
        ReworkableRejectQty[0].value = TotalReworkableRejectQty;
    }

    if (typeof (UnidentifiableRejectQty[0]) != 'undefined' && UnidentifiableRejectQty[0] != null) {
        UnidentifiableRejectQty[0].value = TotalUnidentifiableRejectQty;
    }

    if (typeof (ValidRejectQty[0]) != 'undefined' && ValidRejectQty[0] != null) {
        ValidRejectQty[0].value = TotalValidRejectQty;
    }

    if (typeof (InvalidRejectQty[0]) != 'undefined' && InvalidRejectQty[0] != null) {
        InvalidRejectQty[0].value = TotalInvalidRejectQty;
    }

    if (typeof (BonusBackRejectQty[0]) != 'undefined' && BonusBackRejectQty[0] != null) {
        BonusBackRejectQty[0].value = TotalBonusBackRejectQty;
    }
}

/* Job Txn */
function SS_JobsChecklist_GridRow_renderComplete(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{
    var theGrid = jQuery(this.GridID);
    var jqRowData = theGrid.getRowData();

    $(jqRowData).each(function () {
        try {
            var rowClass;
            if (this['ChecklistCount'] != null && this['ChecklistCount'] > 0)
                rowClass = 'lightgreen';

            if (rowClass) {
                var jqRows = theGrid.find('.jqgrow');
                var trCurrent = $(jqRows).find("td[aria-describedby$='ctl00_WebPartManager_SS_JobGeneralWP02_Job_Checklist_ChecklistId'][title='" + this["ChecklistId"] + "']:first").parent("tr:first");
                $(trCurrent).addClass('ss-general-color-' + rowClass);
            }
        } catch (ex) { }
    });
}

/* Job Txn Main Panel column*/
function SS_Job_GridColumn_renderComplete(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
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
            var prefix = 'ss-general-color-'; // "ui-jqgrid-row-";

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
                var arrToolTip = new Array(this["Name"] + " Material Parts", this["Name"] + " Job History", this["Name"] + " Symptom Code", this["Name"] + " Cause Code", this["Name"] + " Repair Code");
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
        } catch (ex) { alert("halo"); }
    });
}

function SPCRecordsGrid_renderCompleted(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var currentRow;
    var SPCErrorMessage;
    var SPCSetupRowData;
    var ContainerRowData;
    var ResourceRowData;
    var SPCResultRowData;
    var SPCFailureActionRowData;
    var rowClass;

    var Container = document.getElementsByName('ctl00$WebPartManager$SearchFieldsWP$ObjectChanges_Container$Edit');
    var Resource = document.getElementsByName('ctl00$WebPartManager$SearchFieldsWP$ObjectChanges_Resource$Edit');
    var SPCResult = document.getElementsByName('ctl00$WebPartManager$SearchFieldsWP$ObjectChanges_SPCResult$Edit');
    var SPCFailureAction = document.getElementsByName('ctl00$WebPartManager$SearchFieldsWP$ObjectChanges_SPCFailureAction$Edit');

    for (var x = 0; x < rowData.length; x++) {
        currentRow = $("tr", grid)[x + 1];

        SPCErrorMessage = rowData[x].SPCErrorMessage;
        ContainerRowData = rowData[x].ContainerName;
        ResourceRowData = rowData[x].ResourceName;
        SPCResultRowData = rowData[x].SPCResult;
        SPCFailureActionRowData = rowData[x].SPCFailureAction;
        SPCSetupRowData = rowData[x].SPCSetupName;

        if (SPCSetupRowData != "") {
            if (SPCErrorMessage != "")
                $(currentRow).addClass("ss-general-color-lightpink");
            else {
                if ((Container[0].value != "" && Container[0].value != ContainerRowData) ||
                    (Resource[0].value != "" && Resource[0].value != ResourceRowData) ||
                    (SPCResult[0].value != "" && SPCResult[0].value != SPCResultRowData) ||
                    (SPCFailureAction[0].value != "" && SPCFailureAction[0].value != SPCFailureActionRowData))
                    $(currentRow).addClass("ss-general-color-yellow");
                else
                    $(currentRow).addClass("ss-general-color-lightgreen");
            }
        }
    }
}

function annotateSPC(var1, var2, var3) {
    document.querySelector("[name$=\"$DataPointName$ctl00\"]").value = var1;
    document.querySelector("[name$=\"$DataPointID$ctl00\"]").value = var2;
    if (document.querySelector("[name$=\"$IsAnnotationRequired$ctl00\"]"))
        document.querySelector("[name$=\"$IsAnnotationRequired$ctl00\"]").value = var3;

    var btnAnnotate = $("[id$=btnAnnotate]");

    if (btnAnnotate[0].name == "ctl00$WebPartManager$BlankWP1$btnAnnotate") {
        stopSPCRealTimeMonitoring();
    }

    btnAnnotate.click();
}

/* Parts Txn */
function ResultsPanel_renderCompleted(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{
    /*
    This function is called to display colours to the request status coloumn. It will help to determine which colour to put based on the value in request status column.        
    */
    var theGrid = jQuery(this.GridID);
    var jqRowData = theGrid.getRowData();

    for (var x = 0; x < jqRowData.length; x++) {
        currentRow = $("tr", theGrid)[x + 1];
        try {
            var rowClass;
            if (jqRowData[x].RequestStatus == "REQUESTED")
                rowClass = 'orange';
            else if (jqRowData[x].RequestStatus == "ACKNOWLEDGED")
                rowClass = 'yellow';
            else if (jqRowData[x].RequestStatus == "ASSIGNED")
                rowClass = 'lightblue';
            else if (jqRowData[x].RequestStatus == "COMPLETED")
                rowClass = 'lightgreen';
            else if (jqRowData[x].RequestStatus == "CANCELLED")
                rowClass = 'lightpink';
            else
                rowClass = null;

            if (rowClass)
                $(currentRow).find('td:eq(6)').addClass('ss-general-color-' + rowClass);
        } catch (ex) { }
    }
}

/* General Set Row Style */
function SetRowStyle_renderCompleted(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{
    /*
    This function is called to edit the grid row property based on the __STYLE column available in the grid.        
    */
    var theGrid = jQuery(this.GridID);
    var jqRowData = theGrid.getRowData();
    $(jqRowData).each(function () {
        try {
            var style = this['__STYLE'].split('=');
            var aStyle = style[0];
            var newColor = style[1];
            var jqRows = theGrid.find('.jqgrow');
            var trCurrent = $(jqRows).find("td[title='" + this["Lot"] + "']:first").parent("tr:first");
            if (aStyle.toUpperCase() == "FORECOLOR")
                $(trCurrent).find('td').css({ 'color': newColor });
            else if (aStyle.toUpperCase() == "BACKCOLOR")
                $(trCurrent).find('td').css({ 'background-color': newColor });
            else if (aStyle.toUpperCase() == "BOLD" && newColor == "YES")
                $(trCurrent).find('td').css({ 'font-weight': 'bold' });
        } catch (ex) { }
    });
}

/* WIP Data - WIP Data Value for Lot column*/
function SS_WIPData_ByLotDetails_renderCompleted(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{
    var theGrid = jQuery(this.GridID);
    var jqRowData = theGrid.getRowData();

    for (var x = 0; x < jqRowData.length; x++) {
        currentRow = $("tr", theGrid)[x + 1];
        try {
            var rowClass = null;
            if (jqRowData[x].ChangeLimitFailed != null && jqRowData[x].ChangeLimitFailed == "True") { rowClass = 'ss-wipdata-webpart-datavaluefailed'; }
            if (jqRowData[x].LowerLimitFailed != null && jqRowData[x].LowerLimitFailed == "True") { rowClass = 'ss-wipdata-webpart-datavaluefailed'; }
            if (jqRowData[x].UpperLimitFailed != null && jqRowData[x].UpperLimitFailed == "True") { rowClass = 'ss-wipdata-webpart-datavaluefailed'; }

            var requiredClass = null;
            if (jqRowData[x].IsRequired != null && jqRowData[x].IsRequired == "True") { requiredClass = 'ss-wipdata-webpart-isRequired'; }

            if (rowClass) {
                if ($(currentRow).find('td:eq(4)').hasClass("ui-jqgrid-column-image"))
                    $(currentRow).find('td:eq(3)').addClass(rowClass);
                else
                    $(currentRow).find('td:eq(4)').addClass(rowClass);
            }

            if (requiredClass)
                $(currentRow).find('td:eq(2)').addClass(requiredClass);

            if (jqRowData[x].scsWIPDataGroupingType != null) {
                if (jqRowData[x].scsWIPDataGroupingType == "1") { //Equipment
                    if (window.document.documentMode) { //check if IE
                        $("td", currentRow).eq(2).css("box-shadow", "#ff8c00 0px 0px 0px 2px inset");
                        $("td", currentRow).eq(2).css("outline", "#ff8c00");
                    } else {
                        $("td", currentRow).eq(2).css("outline-color", "#ff8c00");
                        $("td", currentRow).eq(2).css("outline-style", "solid");
                        $("td", currentRow).eq(2).css("outline-width", "2px");
                        $("td", currentRow).eq(2).css("outline-offset", "-3px");
                    }
                }
                else if (jqRowData[x].scsWIPDataGroupingType == "2") { //Batch Id
                    if (window.document.documentMode) { //check if IE
                        $("td", currentRow).eq(2).css("box-shadow", "#1e76ff 0px 0px 0px 2px inset");
                        $("td", currentRow).eq(2).css("outline", "#1e76ff");
                    } else {
                        $("td", currentRow).eq(2).css("outline-color", "#1e76ff");
                        $("td", currentRow).eq(2).css("outline-style", "solid");
                        $("td", currentRow).eq(2).css("outline-width", "2px");
                        $("td", currentRow).eq(2).css("outline-offset", "-3px");
                    }
                }
            }

            var td = $("td", currentRow);
            if (td.hasClass('ui-jqgrid-column-image')) {
                var toolTipTxt = 'WIP Data Valid Values';
                var elem = $('#ctl00_WebPartManager_SS_WIPDataWP_HiddenToolTipLabel');
                if (elem != null && elem.length) {
                    toolTipTxt = elem.text();
                }
                $("input", td).attr('title', toolTipTxt);
            }

        } catch (ex) { }
    }
}

/* WIP Data - WIP Data Value for wafer column*/
function SS_WIPData_ByWaferDetails_renderCompleted(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{
    var theGrid = $(this.GridID);
    var jqRowData = theGrid.getRowData();
    var currentRow;
    var columnCount = 3;
    if ($("#ctl00_WebPartManager_SS_WIPDataWP_WIPData_HideDataPointDetails_ctl00").attr("checked") === "checked") {
        columnCount = 1;
    }
    for (var x = 1; x <= jqRowData.length; x++) {
        currentRow = $("tr", theGrid)[x];

        $("td", currentRow).each(function () {
            var cellValue = this.title;
            var rowClass;
            if (cellValue.indexOf("ss_wipdata_datavaluefailed=true") > -1) {
                rowClass = "ss-wipdata-webpart-datavaluefailed";
                $("td", currentRow).eq(this.cellIndex - columnCount).addClass(rowClass);
            }
        })

        var td = $("td", currentRow);
        if (td.hasClass('ui-jqgrid-column-image')) {
            var toolTipTxt = 'WIP Data Valid Values';
            var elem = $('#ctl00_WebPartManager_SS_WIPDataWP_HiddenToolTipLabel');
            if (elem != null && elem.length) {
                toolTipTxt = elem.text();
            }
            $("input", td).attr('title', toolTipTxt);
        }
    }
}


function fixFlyoutDropDown(flyoutButtons) {
    Array.forEach(flyoutButtons,
        function (btnId) {
            var btn = $('[id$="' + btnId + 'Button' + '"].ui-flyout-closebutton');
            btn.unbind('click');
            btn.click(
                function (e) {
                    var container = $(this).parent();  //('ui-flyout-container');
                    container.find('iframe').prop('src', 'Blank.htm');
                    container.hide();
                    return false;
                });
        });
}


/* WIPLotFailures grid style */
function WIPLotFailures_renderCompleted(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{
    var theGrid = jQuery(this.GridID);
    var jqRowData = theGrid.getRowData();
    var newColor = 'RED';

    for (var x = 0; x < jqRowData.length; x++) {
        currentRow = $("tr", theGrid)[x + 1];
        try {
            if (jqRowData[x].YieldResult == "FAIL")
                $(currentRow).find('td').css({ 'color': newColor });
            else if (jqRowData[x].BL_YieldResult == "FAIL")
                $(currentRow).find('td').css({ 'color': newColor });
            else if (jqRowData[x].LL_YieldResult == "FAIL")
                $(currentRow).find('td').css({ 'color': newColor });
            else if (jqRowData[x].PL_YieldResult == "FAIL")
                $(currentRow).find('td').css({ 'color': newColor });
            else if (jqRowData[x].WL_YieldResult == "FAIL")
                $(currentRow).find('td').css({ 'color': newColor });

        } catch (ex) { }
    }
}

/* Lot Combine */
function SS_LotCombine_LotDataGrid_rowDelete(isAtTheEndOfProcessing, rowid, prm2) {
    /*
    This function is called when the row delete button (trashcan) is clicked, the row data has NOT been removed from the grid yet.
    If the grid has VisibleRows value set, the rowData.length value will never be 0, even if there is no actual data in the grid, hence the else condition
    The 'else' condition will check to see if there is a container value in the 2nd row, if present means that there will still be row data after the delete so DO NOT call the clear function.        
    */
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var btnClear = document.getElementById('ctl00_WebPartManager_SS_LotCombineMainWP_LotCombine_ClearButton');
    var btnDelete = document.getElementById('ctl00_WebPartManager_SS_LotCombineMainWP_LotCombine_LotDeleteButton');
    var ContainerToDelete = document.getElementsByName('ctl00$WebPartManager$SS_LotCombineMainWP$LotCombine_ContainerToDelete$ctl00');

    if (rowData.length <= 0) {
        btnClear.click();
        return false;
    }
    else {
        if (rowData[1].Lot == "") {
            btnClear.click();
            return false;
        }
        else {
            var index = parseInt(rowid);
            if (rowData[index].Container != "") {
                ContainerToDelete[0].value = rowData[index].Lot;
                btnDelete.click();
                return true;
            }
        }
    }
}


/* Lot Split */
function SS_LotSplit_DetailsDataGrid_rowDelete(isAtTheEndOfProcessing, rowid, prm2) {
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var ContainerToDelete = document.getElementsByName('ctl00$WebPartManager$LotSplitDetailsWP$LotSplit_ContainerToDelete$ctl00');
    var index = parseInt(rowid);
    if (rowData[index]._id_column != "") {
        ContainerToDelete[0].value = rowData[index]._id_column;
        return true;
    }
    else {
        ContainerToDelete[0].value = "";
    }

}


/* Lot Split */
function SS_LotSplit_DetailsDataGrid_renderCompleted(isAtTheEndOfProcessing, rowid, prm2) {
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var btnUpdate = document.getElementById('ctl00_WebPartManager_LotSplitDetailsWP_LotSplit_WaferGridUpdateButton');
    var ContainerToDelete = document.getElementsByName('ctl00$WebPartManager$LotSplitDetailsWP$LotSplit_ContainerToDelete$ctl00');
    var index = parseInt(rowid);
    var SelectedRowId = document.getElementById('ctl00_WebPartManager_LotSplitDetailsWP_txtRowId_ctl00');
    SelectedRowId.value = rowid;
    if (ContainerToDelete[0].value != "") {
        btnUpdate.click();
    }
    else {
        if (index >= 0 && rowid != "") {
            rowData[index].idx = rowid;
            btnUpdate.click();
        }
    }
    return true;
}

/* Lot Split By Items */
function SS_LotSplitByItems_WafersGrid_edittingStarted(isAtTheEndOfProcessing, rowid, prm2) {
    var btn = document.getElementById('ctl00_WebPartManager_LotSplitDetailsWP_btnCallSelect');
    var SelectedRowId = document.getElementById('ctl00_WebPartManager_LotSplitDetailsWP_txtRowId_ctl00');
    var strf = "jqg_ctl00_WebPartManager_LotSplitDetailsWP_LotSplit_Wafers_" + rowid;
    var chkBox = $('input[name=' + strf + ']');
    chkBox.attr("checkbox", true);

    if (rowid != "") {
        SelectedRowId.value = rowid;
        btn.click();
    }
}

/* Ad-Hoc WIP Data grid style */
function AdHocWIPDataDetails_renderCompleted(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{
    var theGrid = jQuery(this.GridID);
    var jqRowData = theGrid.getRowData();

    for (var x = 0; x < jqRowData.length; x++) {
        currentRow = $("tr", theGrid)[x + 1];
        try {
            if (jqRowData[x].IsRequired == "True")
                $(currentRow).find('td').css({ 'font-weight': 'bold' });

        } catch (ex) { }
    }
}

//----------------------------------------------------------------------------------------------------------------------------------

/* ResourceLayoutMaint Details Grid _renderCompleted */
function ResourceLayoutDetails_renderCompleted(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{
    var ElementID_Prefix = 'ctl00_WebPartManager_MDL_Specific_CollapsibleSectionsAccordion_ctl04_';
    var ElementName_Prefix = 'ctl00$WebPartManager$MDL_Specific$CollapsibleSectionsAccordion$ctl04$';
    var ElementName_Suffix = '$ctl00';

    var btnCanvasRender = document.getElementById(ElementID_Prefix + 'RenderCanvas');
    var CanvasAreaHeight = document.getElementsByName(ElementName_Prefix + 'ObjectChanges_LayoutHeight' + ElementName_Suffix);
    var CanvasAreaWidth = document.getElementsByName(ElementName_Prefix + 'ObjectChanges_LayoutWidth' + ElementName_Suffix);
    var CanvasState = document.getElementsByName(ElementName_Prefix + 'RenderCanvas_State' + ElementName_Suffix);

    if (CanvasAreaHeight[0].value != "" && CanvasAreaWidth[0].value != "") {
        if (CanvasState[0].value == "") {
            // clicking the btnCanvasRender does not refresh/redraw the panel anymore as of V6SU10
            // runaround method to draw the panel after getting the data.            
            // set a timer interval to click the btnCanvasRender
            var DrawInterval = setInterval(function () { ResourceLayoutMaint_DrawCanvas() }, 400);
            CanvasState[0].value = DrawInterval;
            //btnCanvasRender.click();
            return true;
        }
    }

    return true;
}

function ResourceLayoutMaint_DrawCanvas() {
    var ElementID_Prefix = 'ctl00_WebPartManager_MDL_Specific_CollapsibleSectionsAccordion_ctl04_';
    var ElementName_Prefix = 'ctl00$WebPartManager$MDL_Specific$CollapsibleSectionsAccordion$ctl04$';
    var ElementName_Suffix = '$ctl00';

    var btnCanvasRender = document.getElementById(ElementID_Prefix + 'RenderCanvas');
    var CanvasState = document.getElementsByName(ElementName_Prefix + 'RenderCanvas_State' + ElementName_Suffix);

    btnCanvasRender.click();
    window.clearInterval(CanvasState[0].value);

    return true;
}

//----------------------------------------------------------------------------------------------------------------------------------

/* ResourceLayoutMaint Script */
function ResourceLayoutMaintScript() {

    var ElementID_Prefix = 'ctl00_WebPartManager_MDL_Specific_CollapsibleSectionsAccordion_ctl04_';
    var ElementName_Prefix = 'ctl00$WebPartManager$MDL_Specific$CollapsibleSectionsAccordion$ctl04$';
    var ElementName_Suffix = '$ctl00';

    var TargetName = document.getElementsByName(ElementName_Prefix + 'TargetName$ctl00');
    var TargetX = document.getElementsByName(ElementName_Prefix + 'TargetX$ctl00');
    var TargetY = document.getElementsByName(ElementName_Prefix + 'TargetY$ctl00');
    var TargetAction = document.getElementsByName(ElementName_Prefix + 'Details_Action$ctl00');
    var btnUpdate = document.getElementById(ElementID_Prefix + 'UpdateDetail');
    var Equipment = document.getElementsByName(ElementName_Prefix + 'Details_Resource$Edit');
    var BackGroundFilename = document.getElementsByName(ElementName_Prefix + 'ObjectChanges_BackgroundFilename$Edit');
    var theGrid = jQuery('#' + ElementID_Prefix + 'ObjectChanges_Details');
    var spaceTag = "_0xSPACEx0_";

    // ctl00$WebPartManager$MDL_Specific$CollapsibleSectionsAccordion$ctl03$ObjectChanges_LayoutHeight$ctl00

    //------------------------------------------------------------------------------------------------------
    // set the data changed event to set the background of the canvas
    $("#" + ElementID_Prefix + "ObjectChanges_BackgroundFilename").on("propertychange change paste input", function () {
        $('#' + ElementID_Prefix + 'canvasArea').css('background-image', 'url(' + BackGroundFilename[0].value + ')');
        $('#' + ElementID_Prefix + 'canvasArea').css('background-repeat', 'no-repeat');
        $('#' + ElementID_Prefix + 'canvasArea').css('background-position', 'center center');

    });

    //------------------------------------------------------------------------------------------------------

    // deletion icon functionality
    $("#" + ElementID_Prefix + "deleteIcon").click(function () {
        if (TargetName[0].value != "") {
            // set the textboxes so that the server side function has data to update the server side grid data
            TargetAction[0].value = "DELETE";
            TargetX[0].value = 0;
            TargetY[0].value = 0;

            // search for the client side rowId
            var jqRowData = theGrid.getRowData();
            var gridRowId = "";
            var gridRowIdFound = false;
            var resourceName = TargetName[0].value;
            resourceName = resourceName.replace(ElementID_Prefix, "");
            resourceName = resourceName.replace(spaceTag, " ");
            for (var x = 0; x < jqRowData.length; x++) {
                if (jqRowData[x].Resource == resourceName) {
                    gridRowId = jqRowData[x]._id_column;
                    gridRowIdFound = true;
                }
            }

            // delete the client side row
            if (gridRowIdFound) {
                var su = theGrid.jqGrid('delRowData', gridRowId);
            }

            // remove the selected resource icon
            $('#' + TargetName[0].value).remove();

            // trigger the server side function to update the server side grid data.
            btnUpdate.click();

            $(' .deleteIconClass_Icon').css("opacity", "0.3"); /*gray out the icon*/
        }
    });

    //------------------------------------------------------------------------------------------------------
    // enable the icon to be draggable
    $(" .resourceIconClass").draggable
        ({
            helper: 'clone',
            cursor: 'hand'
        });

    //------------------------------------------------------------------------------------------------------
    // show/hide the full resource name on mouse over
    $("#" + ElementID_Prefix + "canvasArea  .drawnMaintIconClass").mouseover(function () {
        $(this).children(".drawnMaintIconDetailsClass").show();
    }).mouseout(function () {
        $(this).children(".drawnMaintIconDetailsClass").hide();
    });

    //------------------------------------------------------------------------------------------------------
    // show/hide the full resource name on mouse over
    $(" .deleteIconClass_Icon").mouseover(function () {
        $(this).css("border-width", "2px");
    }).mouseout(function () {
        $(this).css("border-width", "1px");
    });

    //------------------------------------------------------------------------------------------------------
    // enable the icons in the canvas area to be dragable
    $(" .drawnMaintIconClass").draggable
        ({
            revert: 'invalid',
            containment: " .canvasAreaClass",
            scroll: true,
            cursor: "crosshair",
            stop: function () {
                var pos = $(this).position();
                var canvasPos = $(' .canvasAreaClass').position();
                var iconX = pos.left; // -canvasPos.left;
                var iconY = pos.top; // -canvasPos.top;			        

                TargetName[0].value = $(this).attr('id');
                TargetX[0].value = iconX;
                TargetY[0].value = iconY;

                TargetAction[0].value = "UPDATE";
                btnUpdate.click();
            }
        });

    //------------------------------------------------------------------------------------------------------
    // enable the objects in the canvas area to be selectable
    $("#" + ElementID_Prefix + "canvasArea").selectable
        ({
            selected: function (event, ui) {
                $(' .deleteIconClass_Icon').css("opacity", "1"); /* full color the icon*/
                TargetName[0].value = $(ui.selected).attr('id');
            },

            unselected: function () {
                $(' .deleteIconClass_Icon').css("opacity", "0.3"); /*gray out the icon*/
                TargetName[0].value = "";
            }
        });

    //------------------------------------------------------------------------------------------------------
    // manually trigger the "select" of clicked elements
    $("#" + ElementID_Prefix + "canvasArea  .drawnMaintIconClass").click(function (e) {

        $("#" + ElementID_Prefix + "canvasArea > .drawnMaintIconClass").removeClass("ui-selected");
        if ($(this).hasClass("ui-selected")) {
            // remove selected class from element if already selected
            $(this).removeClass("ui-selected");
        }
        else {
            // add selecting class if not
            $(this).addClass("ui-selecting");
        }

        $("#" + ElementID_Prefix + "canvasArea").data("selectable")._mouseStop(null);
    });

    //------------------------------------------------------------------------------------------------------
    // enable the CanvasArea to be dropable
    $(" .canvasAreaClass").droppable
        ({
            drop: function (event, ui) {
                if (ui.draggable.attr('id') == ElementID_Prefix + 'resourceIcon') {
                    var IsNew = true;

                    if (Equipment[0].value == "") {
                        IsNew = false;
                        alert('Select a resource before dropping the icon to the layout');
                    }

                    if (IsNew) {
                        var jqRowData = theGrid.getRowData();
                        for (var x = 0; x < jqRowData.length; x++) {
                            if (jqRowData[x].Resource == Equipment[0].value) {
                                IsNew = false;
                                alert('Resource already exists in the layout');
                            }
                        }
                    }

                    if (IsNew) {
                        var newId = Equipment[0].value;
                        newId = newId.replace(" ", spaceTag);
                        var displayName = Equipment[0].value;
                        var fullName = Equipment[0].value;

                        if (displayName.length > 8) {
                            displayName = displayName.substring(0, 7) + "..";
                        }

                        $(ui.helper).clone(false).removeAttr('id').attr('id', newId).appendTo(' .canvasAreaClass');

                        // set the display name of the icon
                        $('#' + newId).find(".resourceIdClass").html(displayName);
                        // set the mouseover full name
                        $('#' + newId).find(".dMIDC_ResourceValue").html(fullName);
                        $('#' + newId).removeClass("resourceIconClass");
                        $('#' + newId).addClass("drawnMaintIconClass");

                        $('#' + newId).draggable
                            ({
                                containment: " .canvasAreaClass",
                                scroll: true,
                                cursor: "crosshair",
                                revert: 'invalid',
                                stop: function () {
                                    var pos = $(this).position();
                                    var canvasPos = $(' .canvasAreaClass').position();
                                    var iconX = pos.left; // -canvasPos.left;
                                    var iconY = pos.top; // -canvasPos.top;

                                    TargetName[0].value = $(this).attr('id');
                                    TargetX[0].value = iconX;
                                    TargetY[0].value = iconY;

                                    TargetAction[0].value = "UPDATE";
                                    btnUpdate.click();
                                }
                            });


                        //$('#' + newId).addClass("ui-selectee"); // need to manually add this class to make it selectable	                

                        // make the icon clickable for the selection function
                        $('#' + newId).click(function () {

                            $("#" + ElementID_Prefix + "canvasArea > .drawnMaintIconClass").removeClass("ui-selected");

                            if ($(this).hasClass("ui-selected")) {
                                // remove selected class from element if already selected
                                $(this).removeClass("ui-selected");
                            }
                            else {
                                // add selecting class if not
                                $(this).addClass("ui-selected");
                                $(' .deleteIconClass_Icon').css("opacity", "1"); /* full color the delete icon*/
                            }

                            TargetName[0].value = $(this).attr('id');
                        });

                        // enable the mouseover function
                        $('#' + newId).mouseover(function () {
                            $(this).children(".drawnMaintIconDetailsClass").show();
                        }).mouseout(function () {
                            $(this).children(".drawnMaintIconDetailsClass").hide();
                        });

                        var iconOffset = $('#' + newId).offset();
                        var canvasOffset = $("#" + ElementID_Prefix + "canvasArea").offset();
                        //$('#' + newId).css("position", "relative").offset(offset);			            

                        var pos = $('#' + newId).position();
                        var canvasPos = $("#" + ElementID_Prefix + "canvasArea").position();
                        var iconX = Math.ceil(pos.left - canvasPos.left);
                        var iconY = Math.ceil(pos.top - canvasPos.top);

                        /*
                        alert("IconOffset = " + Math.ceil(iconOffset.left) + " || " + Math.ceil(iconOffset.top) +
                        " <br> IconDropPosition = " + Math.ceil(pos.left) + " || " + Math.ceil(pos.top) +
                        " <br> CanvasPosition = " + Math.ceil(canvasPos.left) + " || " + Math.ceil(canvasPos.top) +
                        " <br> CanvasOffset = " + Math.ceil(canvasOffset.left) + " || " + Math.ceil(canvasOffset.top) +
                        " <br> CalcIconPos = " + iconX + " || " + iconY);
                        */

                        // temporarily hardcode the drop position
                        //iconX = 10;
                        // iconY = 10

                        $('#' + newId).css("position", "absolute");
                        $('#' + newId).css("top", iconY);
                        $('#' + newId).css("left", iconX);

                        // modify the details panel top and left position
                        $('#' + newId).find(".resourceIconDetailsClass").css("top", "40px");
                        $('#' + newId).find(".resourceIconDetailsClass").css("left", "40px");
                        // add the drawnDetailsClass to enable the mouseover effect
                        $('#' + newId).find(".resourceIconDetailsClass").addClass("drawnMaintIconDetailsClass");
                        // remove the templateDetailsClass
                        $('#' + newId).find(".resourceIconDetailsClass").removeClass("resourceIconDetailsClass");

                        TargetName[0].value = newId;
                        TargetX[0].value = iconX;
                        TargetY[0].value = iconY;

                        TargetAction[0].value = "ADD";

                        var actualId = newId.replace(spaceTag, " ");
                        var rowNewId = String("000000" + jqRowData.length).slice(-6);
                        var datarow = { Resource: actualId, XLocation: Math.ceil(iconX), YLocation: Math.ceil(iconY), _id_column: rowNewId };
                        var addOpStatus = theGrid.jqGrid('addRowData', rowNewId, datarow);
                        btnUpdate.click();
                    } // if (IsNew)
                } // if (ui.draggable.attr('id') == ElementID_Prefix + 'resourceIcon') 
            } // drop: function (event, ui)
        });          // $(" .canvasAreaClass").droppable
};

//----------------------------------------------------------------------------------------------------------------------------------

/* ResourceLayoutView AJAX script to auto refresh the page periodically */
function ResourceLayoutView_Refresh() {
    var ResourceLayout = document.getElementsByName('ctl00$WebPartManager$ResourceLayoutSelectionWP$ResourceLayout$Edit');
    $.ajax({
        type: "POST", //GET or POST or PUT or DELETE verb
        url: "SS_ResourceLayoutService.svc/ResourceLayout_GetResourceLayoutItems", // Location of the service
        data: '{"ResourceLayoutName": "' + ResourceLayout[0].value + '"}', //Data sent to server
        contentType: "application/json; charset=utf-8", // content type sent to server
        dataType: "json", //Expected data format from server
        processdata: true, //True or False
        success: function (msg) {//On Successfull service call
            ResourceLayoutView_ParseData(msg);
        },
        error: ResourceLayoutView_ServiceError// When Service call fails
    });
}

//----------------------------------------------------------------------------------------------------------------------------------

/* ResourceLayoutView supporting AJAX script */
function ResourceLayoutView_ParseData(result) {
    var resourceData = result.d;
    for (i = 0; i < resourceData.length; i++) {
        // find the div
        var oResource = resourceData[i];
        var spaceTag = "_0xSPACEx0_";
        oResource.ResourceName = oResource.ResourceName.replace(" ", spaceTag);
        var oStatusDiv = "#ctl00_WebPartManager_ResourceLayoutCanvasWP_" + oResource.ResourceName + " .drawnIconClass_Status";
        var oResourceNameDiv = "#ctl00_WebPartManager_ResourceLayoutCanvasWP_" + oResource.ResourceName + " .drawnIconClass_Resource";
        var oResourceIconImage = "#ctl00_WebPartManager_ResourceLayoutCanvasWP_" + oResource.ResourceName + " .drawnIconClass_Icon_Image";

        // set the status div color and font color of the resource name
        $(oResourceNameDiv).css("color", oResource.STYLE);
        if (oResource.IsAvailable == false) {
            $(oResourceIconImage).css("color", oResource.STYLE);
            if (!($(oResourceIconImage).hasClass("resourceLayoutIcon_DownState"))) {
                $(oResourceIconImage).addClass("resourceLayoutIcon_DownState");
            }
        }
        else {
            $(oResourceIconImage).removeClass("resourceLayoutIcon_DownState");
            $(oResourceIconImage).removeClass("resourceLayoutIcon_Down");
            $(oResourceIconImage).css("color", oResource.STYLE);
        }

        // update the details panel
        var oDetailsStatusSpan = "#ctl00_WebPartManager_ResourceLayoutCanvasWP_" + oResource.ResourceName + " .dIDC_StatusValue";
        var oDetailsReasonSpan = "#ctl00_WebPartManager_ResourceLayoutCanvasWP_" + oResource.ResourceName + " .dIDC_ReasonValue";
        var oDetailsLotCountSpan = "#ctl00_WebPartManager_ResourceLayoutCanvasWP_" + oResource.ResourceName + " .dIDC_LotCountValue";
        var oDetailsLotDiv = "#ctl00_WebPartManager_ResourceLayoutCanvasWP_" + oResource.ResourceName + " .dIDC_Lot";

        $(oDetailsStatusSpan).html(oResource.Status);
        $(oDetailsReasonSpan).html(oResource.Reason);
        $(oDetailsLotCountSpan).html(oResource.LotCount);
        $(oDetailsLotDiv).html(oResource.LotIDListHTML);

    }
}

//----------------------------------------------------------------------------------------------------------------------------------

/* ResourceLayoutView supporting AJAX script */
function ResourceLayoutView_ServiceError(xhr) {
    alert("AJAX Error. Please refresh page");

    if (xhr.responseText) {
        var err = xhr.responseText;
        if (err)
            error(err);
        else
            error({ Message: "Unknown server error." })
    }

    return;
}

//----------------------------------------------------------------------------------------------------------------------------------

/* ResourceLayoutView Script*/
function ResourceLayoutViewScript() {
    var IntervalIDs = document.getElementsByName('ctl00$WebPartManager$ResourceLayoutSelectionWP$IntervalIDs$ctl00');
    var AvailableResources = document.getElementsByName('ctl00$WebPartManager$ResourceLayoutSelectionWP$AvailableResources$Edit');

    //------------------------------------------------------------------------------------------------------
    // set the data changed event to highlight the equipment in the layout panel
    $("#ctl00_WebPartManager_ResourceLayoutSelectionWP_AvailableResources").on("propertychange change paste input", function () {
        //alert('test');
        $("#ctl00_WebPartManager_ResourceLayoutCanvasWP_canvasArea > .drawnIconClass").removeClass("ui-selected");

        var ControlId = AvailableResources[0].value;
        var fullControlId = "#ctl00_WebPartManager_ResourceLayoutCanvasWP_" + ControlId;
        $(fullControlId).addClass("ui-selected");
    });

    //------------------------------------------------------------------------------------------------------
    // enable the objects in the canvas area to be selectable
    $("#ctl00_WebPartManager_ResourceLayoutCanvasWP_canvasArea").selectable
        ({
            selected: function (event, ui) {
                var fullControlId = $(ui.selected).attr('id');
                var ControlId = fullControlId.replace("ctl00_WebPartManager_ResourceLayoutCanvasWP_", "");
                var ControlId2 = $(ui.selected).attr('resname');
                ControlId = ControlId.replace("imgIcon_", "");
                AvailableResources[0].value = ControlId2;
            },

            unselected: function () {
                AvailableResources[0].value = "";
            }
        });

    $("#ctl00_WebPartManager_ResourceLayoutCanvasWP_canvasArea > .drawnIconClass").mouseover(function () {
        $(this).children(".drawnIconDetailsClass").show();
    }).mouseout(function () {
        $(this).children(".drawnIconDetailsClass").hide();
    });

    //------------------------------------------------------------------------------------------------------
    // manually trigger the "select" of clicked elements
    $("#ctl00_WebPartManager_ResourceLayoutCanvasWP_canvasArea > .drawnIconClass").click(function (e) {
        if (e.metaKey == false) {
            // if command key is pressed don't deselect existing elements
            $("#ctl00_WebPartManager_ResourceLayoutCanvasWP_canvasArea > .drawnIconClass").removeClass("ui-selected");
            $(this).addClass("ui-selecting");
        }
        else {
            if ($(this).hasClass("ui-selected")) {
                // remove selected class from element if already selected
                $(this).removeClass("ui-selected");
            }
            else {
                // add selecting class if not
                $(this).addClass("ui-selecting");
            }
        }

        $("#ctl00_WebPartManager_ResourceLayoutCanvasWP_canvasArea").data("selectable")._mouseStop(null);
    });

    // clear the old intervals, IDs are stored in the IntervalIDs textbox
    var currentIntervals = IntervalIDs[0].value;
    var currentIntervalArray = currentIntervals.split(";");
    for (x = 0; x < currentIntervalArray.length; x++) {
        window.clearInterval(currentIntervalArray[x]);
    }

    var flashInterval = setInterval(function () { $(".resourceLayoutIcon_DownState").toggleClass("resourceLayoutIcon_Down"); }, 500);
    var dataUpdateInterval = setInterval(function () { ResourceLayoutView_Refresh() }, 5000);

    IntervalIDs[0].value = flashInterval + ";" + dataUpdateInterval;

    //-----refresh slider jqueryUI------------------------
    $(function () {
        $("#ctl00_WebPartManager_ResourceLayoutSelectionWP_refreshSlider").slider({
            range: "min",
            value: 5,
            min: 1,
            max: 120,
            slide: function (event, ui) {
                $("#refreshIntervalValue").val(ui.value + " s");

                // restart the intervals
                // clear the old intervals, IDs are stored in the IntervalIDs textbox
                var currentIntervals = IntervalIDs[0].value;
                var currentIntervalArray = currentIntervals.split(";");
                for (x = 0; x < currentIntervalArray.length; x++) {
                    window.clearInterval(currentIntervalArray[x]);
                }

                var flashInterval = setInterval(function () { $(".resourceLayoutIcon_DownState").toggleClass("resourceLayoutIcon_Down"); }, 500);
                var dataUpdateInterval = setInterval(function () { ResourceLayoutView_Refresh() }, (ui.value * 1000));

                IntervalIDs[0].value = flashInterval + ";" + dataUpdateInterval;
            }
        });

        $("#refreshIntervalValue").val($("#ctl00_WebPartManager_ResourceLayoutSelectionWP_refreshSlider").slider("value") + " s");
    });

};

//----------------------------------------------------------------------------------------------------------------------------------

/*Container field validation for Carrier Operations page*/
function CarrierOperations_ContainerFieldValidation(e) {

    // removed. Handled by camstar portal clienteventhandler    
    //var keynum;
    //if (window.event)   // IE
    //    keynum = e.keyCode;
    //else if (e.which) // Netscape/Firefox/Opera
    //    keynum = e.which;
    //if (keynum == 9 || keynum == 13 || e.type == "change") {    // 9 is TAB, 13 is ENTER

    var btn = document.getElementById('ctl00_WebPartManager_CarrierPosAssignWP_BtnContainer');
    var ctnReloadBtn = document.getElementById('ctl00_WebPartManager_CarrierPosAssignWP_BtnReloadContainer');
    var chgContSlotBtn = document.getElementById('ctl00_WebPartManager_CarrierPosAssignWP_BtnChgContSlot');
    var carrierName = document.getElementById('ctl00_WebPartManager_CarrierPosAssignWP_CarrierPositionAssign_Carrier_Edit').value;
    var container = document.getElementById('ctl00_WebPartManager_CarrierPosAssignWP_CarrierPositionAssign_Container_ctl00');
    var containerName = document.getElementById('ctl00_WebPartManager_CarrierPosAssignWP_CarrierPositionAssign_Container_ctl00').value;
    var carrierContainer = document.getElementById('ctl00_WebPartManager_CarrierPosAssignWP_CarrierPositionAssign_CarrierContainer_Edit').value;
    var slotPosition = document.getElementById('ctl00_WebPartManager_CarrierPosAssignWP_SlotPosition_Edit').value;
    var divToggelSwitch = document.getElementById("ctl00_WebPartManager_CarrierPosAssignWP_CarrierAssignPostion_Action_booleanSwitchWrapper").classList;
    var isAuto = false; // true= auto | false = manual
    if (divToggelSwitch.contains('switch-on')) {
        isAuto = true;
    }
    var loadbtn = document.getElementById('ctl00_WebPartManager_CarrierPosAssignWP_CarrierPositionAssign_Load').firstChild.checked;
    var unloadbtn = document.getElementById('ctl00_WebPartManager_CarrierPosAssignWP_CarrierPositionAssign_Unload').firstChild.checked;
    var isLoad = false; // true= Load | false = Unload
    if (loadbtn) {
        isLoad = true;
    }

    if (containerName != "") {

        //get grid details
        var ElementID_Prefix = 'ctl00_WebPartManager_CarrierPosAssignWP_';
        var grid = jQuery('#' + ElementID_Prefix + 'CarrierPositionAssign_Details');
        var rowData = grid.getRowData();
        var isDuplicateCont = false;
        var contPositionNo = "";
        var rowNo = 0;

        //validate occupied slot
        if (isLoad && !isAuto) // manual load
        {
            var isSlotOccupied = false;
            for (var x = 0; x < rowData.length; x++) {
                if (rowData[x].PositionNumber == slotPosition && rowData[x].Container != "") {
                    gridRowId = rowData[x]._id_column;
                    isSlotOccupied = true;
                    break;
                }
            }
            if (isSlotOccupied) {
                __page.displayStatus("Slot occupied. Please select an empty slot.", "Warning", "Warning");
                return false;
            }
        }

        for (var x = 0; x < rowData.length; x++) {
            if (rowData[x].Container.toUpperCase() == containerName.toUpperCase()) {
                gridRowId = rowData[x]._id_column;
                isDuplicateCont = true;
                contPositionNo = rowData[x].PositionNumber;
                rowNo = x;
                break;
            }
        }

        //check for secondary LOAD Duplicates of containers below the UNLOAD
        if (isDuplicateCont) {
            var rowData2 = grid.getRowData();
            var isDuplicateCont2 = false;
            var contPositionNo2 = "";
            var rowNo2 = 0;

            for (var y = 0; y < rowData2.length; y++) {
                if (rowData2[y].Container.toUpperCase() == containerName.toUpperCase() && rowData2[y].PositionNumber != contPositionNo && rowData2[y].SetupAction.toUpperCase() == "LOAD") {
                    gridRowId = rowData2[y]._id_column;
                    isDuplicateCont2 = true;
                    contPositionNo2 = rowData2[y].PositionNumber;
                    rowNo2 = y;
                    break;
                }
            }
        }


        //Validate UNLOAD
        if (carrierName != "") {
            if (!isLoad) {
                if (isDuplicateCont) {
                    if (rowData[rowNo].SetupAction.toUpperCase() == "UNLOAD") {
                        __page.displayStatus(containerName.toUpperCase() + " is pending UNLOAD on Position Number: " + contPositionNo + ".", "Warning", "Warning");
                        return false;
                    }
                    else if (rowData[rowNo].SetupAction.toUpperCase() == "LOAD") {
                        __page.displayStatus(containerName.toUpperCase() + " is pending LOAD on Position Number: " + contPositionNo + ".", "Warning", "Warning");
                        return false;
                    }
                    else {
                        btn.click();
                        return true;
                    }
                }
                else {
                    __page.displayStatus(containerName.toUpperCase() + " does not exist as an assigned container in the Carrier.", "Warning", "Warning");
                    return false;
                }
            }

            //Validate LOAD
            else {
                if (isDuplicateCont) {
                    //1.Check if container setup action is empty, that means its already loaded in the carrier
                    if (rowData[rowNo].SetupAction == "" || rowData[rowNo].SetupAction == null) {
                        var errMsg = containerName.toUpperCase() + " was assigned in another slot. Do you wish to UNLOAD container from Slot Number: " + contPositionNo + " ?";
                        jConfirm(errMsg, 'WARNING', function (r) {
                            if (r == true) {
                                ctnReloadBtn.click();
                                return true;
                            }
                        }, 'Warning');
                        return false;
                    }
                    else if (rowData[rowNo].SetupAction.toUpperCase() == "LOAD") {
                        if (!isAuto) {
                            chgContSlotBtn.click();
                            __page.displayStatus(containerName.toUpperCase() + " was previously pending LOAD on Position Number: " + contPositionNo + " and now has been transfered to selected Position Number: " + slotPosition + ".", "Warning", "Warning");
                            return true;
                        }
                        else {
                            __page.displayStatus(containerName.toUpperCase() + " is pending LOAD on Position Number: " + contPositionNo + ".", "Warning", "Warning");
                            return false;
                        }
                    }
                    else if (rowData[rowNo].SetupAction.toUpperCase() == "UNLOAD") {
                        if (isDuplicateCont2) {
                            if (isAuto) {
                                __page.displayStatus(containerName.toUpperCase() + " is pending LOAD on Position Number: " + contPositionNo2 + ".", "Warning", "Warning");
                                return false;
                            }
                            chgContSlotBtn.click();
                            __page.displayStatus(containerName.toUpperCase() + " was previously pending LOAD on Position Number: " + contPositionNo2 + " and now has been transfered to selected Position Number: " + slotPosition + ".", "Warning", "Warning");
                            return true;
                        }
                        else {
                            //__page.displayStatus(containerName.toUpperCase() + " is pending UNLOAD on Position Number: " + contPositionNo + ".", "Warning", "Warning");
                            //return false;
                            btn.click();
                        }
                    }
                    else {
                        //throw error, stop trx
                        __page.displayStatus(containerName.toUpperCase() + " already exist on Position Number: " + contPositionNo + ".", "Warning", "Warning");
                        return false;
                    }
                }
            }
        }
        else {
            __page.displayStatus("Carrier is empty.", "Warning", "Warning");
            return false;
        }

        // validate valid container with first container found in grid. 1.Active 2.isInWIP 3. In the same step/phase and same StartParentContainer as other container in current carrier 
        var containerToValidate = "";
        var firstContFound = "";
        for (var y = 0; y < rowData.length; y++) {
            if (rowData[y].Container != "" && rowData[y].Container != null) {
                firstContFound = rowData[y].Container;
                break;
            }
        }

        containerToValidate = firstContFound;

        //removed on task 202588, to allow empty carrier to take on container on any kind of step/phase
        //// validate using carrierContainer if it has value.
        //if (carrierContainer == "") {
        //    containerToValidate = firstContFound;
        //}
        //else
        //{
        //    containerToValidate = carrierContainer;
        //}

        var inputData = carrierName + ',' + containerName + ',' + isAuto + ',' + isLoad + ',' + containerToValidate;

        $.ajax({
            type: "POST", //GET or POST or PUT or DELETE verb
            url: "ss_carrieroperationservice.svc/ValidateContainers", // Location of the service
            data: '{"inputDataString": "' + inputData + '"}', //Data sent to server
            contentType: "application/json; charset=utf-8", // content type sent to server
            dataType: "json", //Expected data format from server
            processdata: true, //True or False
            success: function (msg) {//On Successfull service call

                (CarrierOperation_ServiceSuccess(msg.d)); //a function to validate the error msg to be displayed out
            },
            error: CarrierOperation_ServiceError// When Service call fails
        });
        return false;
    }
    return true;
}

/* CarrierOperations supporting AJAX script */
function CarrierOperation_ServiceSuccess(result) {
    //get grid hidden labels
    var ElementID_Prefix = 'ctl00_WebPartManager_CarrierPosAssignWP_';
    var grid = jQuery('#' + ElementID_Prefix + 'CarrierPositionAssign_HiddenLabel');
    var rowData = grid.getRowData();
    var btn = document.getElementById('ctl00_WebPartManager_CarrierPosAssignWP_BtnContainer');

    //valid containers
    if (result == null) {
        btn.click();
        return true;
    }
    else {
        if (result.indexOf("Invalid Container") > -1) {
            __page.displayStatus(rowData[0].Value, "Warning", "Warning");
            return false;
        }
        else if (result.indexOf("was assigned in another carrier") > -1) {
            __page.displayStatus(result, "Warning", "Warning");
            return false;
        }
        else if (result.indexOf("scsStatusValidation") > -1) {
            __page.displayStatus(rowData[1].Value, "Warning", "Warning");
            return false;
        }
        else if (result.indexOf("scsWorkflowStepValidation") > -1) {
            __page.displayStatus(rowData[2].Value, "Warning", "Warning");
            return false;
        }
        else if (result.indexOf("scsParentContainerValidation") > -1) {
            __page.displayStatus(rowData[3].Value, "Warning", "Warning");
            return false;
        }
        else if (result.indexOf("scsScheduleDataValidation") > -1) {
            __page.displayStatus(rowData[4].Value, "Warning", "Warning");
            return false;
        }
        else if (result.indexOf("scsWorkOrderValidation") > -1) {
            __page.displayStatus(rowData[5].Value, "Warning", "Warning");
            return false;
        }
        else if (result.indexOf("scsWIPTransactionValidation") > -1) {
            __page.displayStatus(rowData[6].Value, "Warning", "Warning");
            return false;
        }
        else {
            alert(result);
            return false;
        }
    }
}

/* CarrierOperations supporting AJAX script */
function CarrierOperation_ServiceError(xhr) {
    alert("AJAX Error. Please refresh page");

    if (xhr.responseText) {
        var err = xhr.responseText;
        if (err)
            error(err);
        else
            error({ Message: "Unknown server error." })
    }
    return;
}

/* CarrierOperations Unload All Confirmation Popup */
function CarrierOperations_UnloadAllConfirmation() {
    jConfirm("Do you want to Unload All?", 'WARNING', function (r) {
        if (r == true) {
            var btnUnloadAll = document.getElementById('ctl00_WebPartManager_ButtonsBar_HiddenUnloadAllAction');
            btnUnloadAll.click();
            return true;
        }
    }, 'Warning');

    return false;
}

/* CarrierOperations Transfer All Confirmation Popup */
function CarrierOperations_TransferAllConfirmation() {
    jConfirm("There is a required position(s) in the Transfer Carrier which is in a Down state, do you wish to continue to Transfer All?", 'WARNING', function (r) {
        if (r == true) {
            var btnTransferAll = document.getElementById('ctl00_WebPartManager_ButtonsBar_HiddenTransferAllAction');
            btnTransferAll.click();
            return true;
        }
    }, 'Warning');

    return false;
}

//----------------------------------------------------------------------------------------------------------------------------------

function ResourceSetStatusScript() {
    var IntervalID = document.getElementsByName('ctl00$WebPartManager$BlankWP$IntervalID$ctl00');

    window.clearInterval(IntervalID[0].value);
    var dataUpdateInterval = setInterval(function () { ResourceStatus_Refresh() }, 4000);
    IntervalID[0].value = dataUpdateInterval;
}

function ResourceStatus_Refresh() {
    var btnRefresh = document.getElementById("ctl00_WebPartManager_BlankWP_RefreshButton");
    var Resource = document.getElementsByName('ctl00$WebPartManager$BlankWP$Resource$Edit');
    var IntervalID = document.getElementsByName('ctl00$WebPartManager$BlankWP$IntervalID$ctl00');

    if (Resource[0].value != "") {
        window.clearInterval(IntervalID[0].value);
        var dataUpdateInterval = setInterval(function () { ResourceStatus_Refresh() }, 4000);

        IntervalID[0].value = dataUpdateInterval;
        btnRefresh.click();
    }
}

// material move reset
function MaterialMove_Reset(isAtTheEndOfProcessing, rowid, prm2) {
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var index = parseInt(rowid);
    var btnReset = document.getElementById('ctl00_WebPartManager_SS_MaterialMoveWP_Reset');

    if (rowData.length <= 0) {
        btnReset.click();
        return false;
    }
    else {
        if (index == 0 && rowData[1].Lot == "") {
            btnReset.click();
            return false;
        }
    }
    return true;
}

//inventory move reset
function InventoryMove_Reset(isAtTheEndOfProcessing, rowid, prm2) {
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var index = parseInt(rowid);
    var btnReset = document.getElementById('ctl00_WebPartManager_SS_LotMoveInventoryWP_Reset');

    if (rowData.length <= 0) {
        btnReset.click();
        return false;
    }
    else {
        if (index == 0 && rowData[1].Lot == "") {
            btnReset.click();
            return false;
        }
    }
    return true;
}

/* Lot Form */
function SS_LotForm_DetailsGrid_rowDelete(isAtTheEndOfProcessing, rowid, prm2) {
    /*
    This function is called when the row delete button (trashcan) is clicked, the row data has NOT been removed from the grid yet.
    If the grid has VisibleRows value set, the rowData.length value will never be 0, even if there is no actual data in the grid, hence the else condition
    The 'else' condition will check to see if there is a container value in the 2nd row, if present means that there will still be row data after the delete so DO NOT call the clear function.        
    */
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var btnReset = document.getElementById('ctl00_WebPartManager_ButtonsBar_ResetAction');
    var btnRefresh = document.getElementById('ctl00_WebPartManager_SS_LotFormWP_RefreshWafersAndMainLot');

    if (rowData.length <= 0) {
        btnReset.click();
        return false;
    }
    else {
        if (rowData[1].Lot == "") {
            btnReset.click();
            return false;
        }
        else {
            btnRefresh.click();
            return true;
        }

    }
}

function LotHold_SetContainerOnGridEdit(isAtTheEndOfProcessing, rowid, prm2) {
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var index = parseInt(rowid);
    var ContainerSelection = document.getElementsByName('ctl00$WebPartManager$SS_LotHoldWP$LotHold_ContainerSelection$Edit');

    ContainerSelection[0].value = rowData[index].Lot;
    return true;
}

//lot impound reset
function LotImpound_Reset(isAtTheEndOfProcessing, rowid, prm2) {
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var index = parseInt(rowid);
    var btnReset = document.getElementById('ctl00_WebPartManager_SS_LotImpoundWP_Reset');

    if (rowData.length <= 0) {
        btnReset.click();
        return false;
    }
    else {
        if (index == 0 && rowData[1].Lot == "") {
            btnReset.click();
            return false;
        }
    }
    return true;
}

function ExperimentPlanDetails_ActionDetails_editingStarted(isAtTheEndOfProcessing, rowid, prm2) {
    var index = parseInt(rowid);
    var SelectedRowIndex = document.getElementsByName('ctl00$WebPartManager$MDL_Specific$CollapsibleSectionsAccordion$ctl04$HiddenSelectedRowIDTextBox$ctl00');
    SelectedRowIndex[0].value = index;
    return true;
}

function ExperimentPlanDetails_ActionDetails_rowDelete(isAtTheEndOfProcessing, rowid, prm2) {
    var HiddenSelectedRowTxt = document.getElementsByName('ctl00$WebPartManager$MDL_Specific$CollapsibleSectionsAccordion$ctl04$HiddenSelectedRowIDTextBox$ctl00');
    HiddenSelectedRowTxt[0].value = null;
    return true;
}

function PartRequestAssign_SetSelectedRowOnGridEdit(isAtTheEndOfProcessing, rowid, prm2) {
    var index = parseInt(rowid);
    var SelectedRowIndex = document.getElementsByName('ctl00$WebPartManager$SS_PartRequestTxnWP$HiddenSelectedRowId$ctl00');
    SelectedRowIndex[0].value = index;
    return true;
}

function CustomSPCGridFilter_editingStarted(isAtTheEndOfProcessing, rowid, prm2) {
    var index = parseInt(rowid);
    var SelectedRowIndex = document.getElementsByName('ctl00$WebPartManager$CriteriaWP$CollapsibleSectionsAccordion$ctl05$HiddenSelectedRowIDTextBox$ctl00');
    SelectedRowIndex[0].value = index;
    return true;
}

/* Lot Send Ahead */
function SS_LotSendAhead_LotDataGrid_rowDelete(isAtTheEndOfProcessing, rowid, prm2) {
    /*
    This function is called when the row delete button (trashcan) is clicked, the row data has NOT been removed from the grid yet.
    If the grid has VisibleRows value set, the rowData.length value will never be 0, even if there is no actual data in the grid, hence the else condition
    The 'else' condition will check to see if there is a container value in the 2nd row, if present means that there will still be row data after the delete so DO NOT call the clear function.        
    */
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var btnClear = document.getElementById('ctl00_WebPartManager_SS_LotSendAheadMainWP_ss_LotSendAhead_ClearButton');
    var btnDelete = document.getElementById('ctl00_WebPartManager_SS_LotSendAheadMainWP_ss_LotSendAhead_LotDeleteButton');

    var ContainerToDelete = document.getElementsByName('ctl00$WebPartManager$SS_LotSendAheadMainWP$ss_LotSendAhead_ContainerToDelete$ctl00');

    if (rowData.length <= 0) {
        btnClear.click();
        return false;
    }
    else {
        if (rowData[1].Lot == "") {
            btnClear.click();
            return false;
        }
        else {
            var index = parseInt(rowid);
            if (rowData[index].Container != "") {
                ContainerToDelete[0].value = rowData[index].Lot;
                btnDelete.click();
                return true;
            }
        }
    }
}

/* MaintenanceScheduleInquiry Script*/
function MaintenanceScheduleInquiryScript() {

    $("#ctl00_WebPartManager_CanvasWP_canvasAreaFrame .ScheduleDue").mouseover(function () {
        $(this).find(".ScheduleDetails").show();
    }).mouseout(function () {
        $(this).find(".ScheduleDetails").hide();
    });
}

/* Equipment WIP Main */
function SS_EquipmentWIPMain_LotDataGrid_rowDelete(isAtTheEndOfProcessing, rowid, prm2) {
    /*
        This function is called when the row delete button (trashcan) is clicked, the row data has NOT been removed from the grid yet.
        If the grid has VisibleRows value set, the rowData.length value will never be 0, even if there is no actual data in the grid, hence the else condition
        The 'else' condition will check to see if there is a container value in the 2nd row, if present means that there will still be row data after the delete so DO NOT call the clear function.        
    */
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var btnReload = document.getElementById('ctl00_WebPartManager_DispatchListLotsWP_EqpWIPMain_Reload');
    var btnRemove = document.getElementById('ctl00_WebPartManager_DispatchListLotsWP_EqpWIPMain_RemoveSelection');
    var btnClear = document.getElementById('ctl00_WebPartManager_ActionsControl_Reset');
    var ContainerToDelete = document.getElementsByName('ctl00$WebPartManager$DispatchListLotsWP$EqpWIPMain_ContainerToRemove$ctl00');

    if (rowData.length <= 0) {
        btnClear.click();
        return false;
    }
    else {
        var index = parseInt(rowid);
        ContainerToDelete[0].value = rowData[index].Container;
        btnRemove.click();
        return true;
    }
}
/* Surveillance Management */
function SurveillanceManagement_renderComplete(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{
    var theGrid = jQuery(this.GridID);
    var jqRowData = theGrid.getRowData();

    $(jqRowData).each(function () {
        try {
            var rowClass;
            switch (this['ss_SurveillanceState']) {
                case 'Past Due':
                    rowClass = 'red';
                    break;
                case 'Due':
                    rowClass = 'orange';
                    break;
                case 'Pending':
                    rowClass = 'yellow';
                    break;
            }

            if (rowClass) {
                var jqRows = theGrid.find('.jqgrow');
                var trCurrent = $(jqRows).find("td[title='" + this["ss_SurveillanceStatus"] + "']:first").parent("tr:first");
                $(trCurrent).addClass('ui-jqgrid-row-' + rowClass);
            }
        } catch (ex) { }
    });
}


// lot modify bins function to hide/show the wafer
function LotModifyBins_renderComplete(isAtTheEndOfProcessing, rowid, prm2) {
    var theGrid = jQuery(this.GridID);
    var IsWaferProcessing = document.getElementsByName('ctl00$WebPartManager$LotModifyBinsWP$LotModifyBins_IsWaferProcessing$ctl00');

    if (IsWaferProcessing[0].checked) {
        theGrid.showCol('WaferScribeNumber');
        theGrid.showCol('ToWaferScribeNumber');
    }
    else {
        theGrid.hideCol('WaferScribeNumber');
        theGrid.hideCol('ToWaferScribeNumber');
    }

}

//label plan maint function to auto generate Level ID
function LabelPlanDetails_editingStarted(isAtTheEndOfProcessing, rowid, prm2) {
    var highestval = 0;
    var HighestLevelText = document.getElementsByName("ctl00$WebPartManager$MDL_Specific$CollapsibleSectionsAccordion$ctl04$HighestLevelID$ctl00");

    if (HighestLevelText[0].value == "") {
        var grid = $(this.GridID);
        var data = grid.getGridParam('reccount');

        var rowData = grid.getRowData();

        for (var i = 0; i < rowData.length; i++) {
            var record = rowData[i];

            if (parseInt(record['LevelID']) > highestval) {
                highestval = parseInt(record['LevelID']);
                HighestLevelText[0].value = highestval;
            }
        }
    }

    var LevelIDText = document.getElementsByName("ctl00$WebPartManager$MDL_Specific$CollapsibleSectionsAccordion$ctl04$ObjectChanges_ss_LabelPlanDetails_LevelID_InlineEditorControl$ctl01");

    if (LevelIDText[0].value == "") {
        if (HighestLevelText[0].value == "") {
            LevelIDText[0].value = 1;
        }
        else {
            LevelIDText[0].value = parseInt(HighestLevelText[0].value) + 1;
            HighestLevelText[0].value = parseInt(HighestLevelText[0].value) + 1;
        }
    }

    return true;
}

function LabelPlanDetails_renderComplete(isAtTheEndOfProcessing, rowid, prm2) {
    document.getElementById("ctl00_WebPartManager_MDL_Specific_CollapsibleSectionsAccordion_ctl04_HighestLevelID").style.display = 'none';
}

/* Lot Start Service Attributes grid style */
function LotStartServiceAttrsDetails_renderCompleted(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{
    var theGrid = jQuery(this.GridID);
    var jqRowData = theGrid.getRowData();

    for (var x = 0; x < jqRowData.length; x++) {
        currentRow = $("tr", theGrid)[x + 1];
        try {
            if (jqRowData[x].IsRequired == "True")
                $(currentRow).find('td').css({ 'font-weight': 'bold' });

        } catch (ex) { }
    }
}

function ss_UsageReqSetupDetails_editingStarted(isAtTheEndOfProcessing, rowid, prm2) {
    var SelectedRowIndex = document.getElementsByName('ctl00$WebPartManager$MDL_Specific$CollapsibleSectionsAccordion$ctl09$HiddenSelectedRowIDTextBox$ctl00');
    SelectedRowIndex[0].value = rowid;

    var detailsGrid = document.getElementById('ctl00_WebPartManager_MDL_Specific_CollapsibleSectionsAccordion_ctl09_ObjectChanges_ss_UsageReqSetupDetails');

    var tbody = detailsGrid.getElementsByTagName("tbody");
    var tr = document.getElementById(rowid);
    var td = tr.getElementsByTagName("td");
    var type = td[4].getElementsByTagName("input");
    var expression = td[5].getElementsByTagName("span");
    var query = td[6].getElementsByTagName("span");

    if (type[0].value == 'Expression') {
        expression[0].style.display = "";
        query[0].style.display = "none";
    }
    else if (type[0].value == 'Query') {
        expression[0].style.display = "none";
        query[0].style.display = "";
    }
    else if (type[0].value == '') {
        expression[0].style.display = "none";
        query[0].style.display = "none";
    }

    return true;
}

function EqpWIPMain_DispatchList_renderCompleted(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{
    var theGrid = jQuery(this.GridID);
    var jqRowData = theGrid.getRowData();

    for (var x = 0; x < jqRowData.length; x++) {
        currentRow = $("tr", theGrid)[x + 1];

        if (jqRowData[x].__STYLE != null && jqRowData[x].__STYLE != "") {
            var style = jqRowData[x].__STYLE.split('=');
            var aStyle = style[0];
            var newColor = style[1];

            if (aStyle.toUpperCase() == "FORECOLOR")
                $(currentRow).find('td').css({ 'color': newColor });
        }
    }
}


function WIPMain_TxnConfirmation() //renderCompleted
{
    jConfirm("Executing this transction will also execute timer disposition actions. Do you want to proceed?", 'WARNING', function (r) {
        if (r == true) {
            var btnSubmitTxn = document.getElementById('ctl00_WebPartManager_SS_WIPMain_HomeWP_Main_HiddenSubmit');
            btnSubmitTxn.click();
        }
    }, 'Warning');

    return true;
}


function EquipmentWIPMain_TxnConfirmation() //renderCompleted
{
    jConfirm("Executing this transction will also execute timer disposition actions. Do you want to proceed?", 'WARNING', function (r) {
        if (r == true) {
            var btnSubmitTxn = document.getElementById('ctl00_WebPartManager_DispatchListLotsWP_Main_HiddenSubmit');
            btnSubmitTxn.click();
        }
    }, 'Warning');

    return true;
}

function CheckSheet_TxnConfirmation() {
    jConfirm('Executing this transction will also execute timer disposition actions. Do you want to proceed?', 'WARNING', function (r) {
        if (r == true) {
            var btnSubmitTxn = document.getElementById('ctl00_WebPartManager_ActionsControl_ExecuteAction');
            __doPostBack('btnSubmitTxn', "TxnConfirm")
        }
    }, 'Warning');
    return true;
}

/* SPCRealTimeMonitoring Script*/
function SPCRealTimeMonitoringScript(param) {
    var IntervalIDs = document.getElementsByName('ctl00$WebPartManager$ChartAreaWP$IntervalIDs$ctl00');

    // clear the old intervals, IDs are stored in the IntervalIDs textbox
    var currentIntervals = IntervalIDs[0].value;
    var currentIntervalArray = currentIntervals.split(";");
    for (x = 0; x < currentIntervalArray.length; x++) {
        window.clearInterval(currentIntervalArray[x]);
    }

    var dataUpdateInterval = setInterval(function () {
        if ($("#ctl00_WebPartManager_ChartAreaWP_Status_ctl00")[0].value == "OFF")
            clearInterval(dataUpdateInterval);
        else if (document.getElementById("ctl00_WebPartManager_ChartAreaWP_StopResumeBtn").value == "On") {
            executeSPCService();
        }
    }, getIntervalValue() * 1000);

    IntervalIDs[0].value = dataUpdateInterval;

    //-----refresh slider jqueryUI------------------------
    $(function () {
        $("#ctl00_WebPartManager_ChartAreaWP_refreshSlider").slider({
            range: "min",
            value: getIntervalValue(),
            min: 10,
            max: 120,
            slide: function (event, ui) {
                $("#refreshIntervalValue").val(ui.value + " s");
                $("#ctl00_WebPartManager_ChartAreaWP_IntervalValue_ctl00").val(ui.value);

                // restart the intervals
                // clear the old intervals, IDs are stored in the IntervalIDs textbox
                var currentIntervals = IntervalIDs[0].value;
                var currentIntervalArray = currentIntervals.split(";");
                for (x = 0; x < currentIntervalArray.length; x++) {
                    window.clearInterval(currentIntervalArray[x]);
                }

                clearInterval(dataUpdateInterval);
                var dataUpdateInterval = setInterval(function () {
                    if ($("#ctl00_WebPartManager_ChartAreaWP_Status_ctl00")[0].value == "OFF")
                        clearInterval(dataUpdateInterval);
                    else if (document.getElementById("ctl00_WebPartManager_ChartAreaWP_StopResumeBtn").value == "On") {
                        executeSPCService();
                    }
                }, ui.value * 1000);

                IntervalIDs[0].value = dataUpdateInterval;
            }
        });
        $("#refreshIntervalValue").val(getIntervalValue() + " s");
    });

    $("#ctl00_WebPartManager_BlankWP1_SearchClear").click(function () {
        clearInterval(dataUpdateInterval);
    });

    //toggle between stop and resume button click action
    $("#ctl00_WebPartManager_ChartAreaWP_StopResumeBtn").click(function () {
        toggleStopResume();
    });

    /*$("#ctl00_WebPartManager_BlankWP1_btnAnnotate").click(function () {
        toggleStopResume();
    });*/

    //executeSPCService();
};

function toggleStopResume() {
    var sliderDiv = document.getElementById("ctl00_WebPartManager_ChartAreaWP_RefreshSliderPanel");

    if (document.getElementById("ctl00_WebPartManager_ChartAreaWP_StopResumeBtn").value == "On") {
        $("#ctl00_WebPartManager_ChartAreaWP_StopResumeBtn")[0].value = $("#ctl00_WebPartManager_ChartAreaWP_hiddenStopLabel_ctl00")[0].value;
        $("#ctl00_WebPartManager_ChartAreaWP_StopResumeBtn")[0].style.backgroundColor = "Red";
        $("#ctl00_WebPartManager_ChartAreaWP_Status_ctl00")[0].value = "OFF";
        sliderDiv.style.display = 'none';
    }
    else {
        $("#ctl00_WebPartManager_ChartAreaWP_StopResumeBtn")[0].value = $("#ctl00_WebPartManager_ChartAreaWP_hiddenResumeLabel_ctl00")[0].value;
        $("#ctl00_WebPartManager_ChartAreaWP_StopResumeBtn")[0].style.backgroundColor = "Green"
        $("#ctl00_WebPartManager_ChartAreaWP_Status_ctl00")[0].value = "ON";
        sliderDiv.style.display = 'block';
        executeSPCService();
    }
}

function getIntervalValue() {
    return $("#ctl00_WebPartManager_ChartAreaWP_IntervalValue_ctl00")[0].value;
}

function executeSPCService() {
    var $ap = __page.$getActiveTabPanel();
    var src = $ap.children().attr('src');

    if (src == undefined || (src && src.toLowerCase().indexOf('ss_spcrealtimemonitoringvp') == -1)) {
        return;
    }

    var SPCSetupName = $("#ctl00_WebPartManager_SPCRealTimeMonitoringSelectionWP_ObjectChanges_SPCSetup_Edit")[0].value;
    var employeeName = $("#ctl00_WebPartManager_ChartAreaWP_EmployeeName_ctl00")[0].value;
    var QueryParamString = $("#ctl00_WebPartManager_ChartAreaWP_txtQueryParams_ctl00")[0].value;
    var SPCResultFilename = $("#ctl00_WebPartManager_ChartAreaWP_SPCTxnDataList_SPCResultFilename_ctl00")[0].value;

    if (SPCResultFilename.toLowerCase().indexOf("spcchart") > -1 && __page.get_eventArgument() && __page.get_eventArgument().value == 'ExecuteSPCPostBackArgument') {
        __page.set_eventArgument('');
        return;
    }

    if (SPCResultFilename.toLowerCase().indexOf("spcchart") == -1) {
        SPCResultFilename = "";
    }

    var inputData = SPCSetupName + ',' + employeeName + ',' + QueryParamString + ',' + SPCResultFilename;
    $.ajax({
        type: "POST", //GET or POST or PUT or DELETE verb
        url: "ss_spcrealtimeservice.svc/ExecuteSPCService", // Location of the service
        data: '{"inputDataString": "' + inputData + '"}', //Data sent to server
        contentType: "application/json; charset=utf-8", // content type sent to server
        dataType: "json", //Expected data format from server
        processdata: true, //True or False
        success: function (msg) {//On Successfull service call
            if (msg.d[0].indexOf("Command syntax error") == -1) {
                if (msg.d[0].toLowerCase().indexOf("spcchart") > -1) {
                    renderInlineChart(msg);
                }
                else {
                    renderChart(msg);
                }
            }
            else {
                SPCRealTimeMonitoring_ServiceError(null, msg.d[0]);
            }
        },
        error: SPCRealTimeMonitoring_ServiceError// When Service call fails
    });

    function renderInlineChart(msg) {
        var $ap = __page.$getActiveTabPanel();
        var src = $ap.children().attr('src');
        if (src == undefined || (src && src.toLowerCase().indexOf('ss_spcrealtimemonitoringvp') == -1)) {
            return;
        }
        var newChartURL = msg.d[0];
        var oldChartURL = $("#ctl00_WebPartManager_ChartAreaWP_SPCTxnDataList_SPCResultFilenameOld_ctl00")[0];
        var currentChartURL = $("#ctl00_WebPartManager_ChartAreaWP_SPCTxnDataList_SPCResultFilename_ctl00")[0];

        oldChartURL.value = currentChartURL.value;
        currentChartURL.value = newChartURL;

        var iStartIndex = newChartURL.indexOf("://", 0); // get the http:// or https:// index
        var iMidIndex = newChartURL.indexOf("/", iStartIndex + 3);  // get next      
        var sFileURL = newChartURL;

        if ((iStartIndex >= 0) && (iMidIndex > iStartIndex))
            sFileURL = newChartURL.substring(iMidIndex);

        //fix for IE doesn't have origin
        if (!window.location.origin) {
            window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
        }

        if (window.location.origin != undefined)
            newChartURL = window.location.origin + sFileURL;
        if (window.location.href.indexOf("localhost") > -1) {
            newChartURL = window.location.protocol + "//localhost" + sFileURL;
        }

        $.ajax({
            url: newChartURL,
            success: function (data) {
                var div = document.getElementById("ctl00_WebPartManager_ChartAreaWP_ChartPanel");
                div.innerHTML = data.substring(data.indexOf("<table"));//replace current chart panel with new htmlfile
                var i = sFileURL.toLowerCase().indexOf("spcchart.htm");
                var src = "../.." + sFileURL.substring(0, i - 1) + "/GenerateCharts.js";
                $.getScript(src);
            },
            cache: false
        });
    }

    function renderChart(msg) {
        var newChartURL = msg.d[0];
        var oldChartURL = $("#ctl00_WebPartManager_ChartAreaWP_SPCTxnDataList_SPCResultFilenameOld_ctl00")[0];
        var oldImgURL = $("#ctl00_WebPartManager_ChartAreaWP_OldSPCImgFileName_ctl00")[0];
        var currentChartURL = $("#ctl00_WebPartManager_ChartAreaWP_SPCTxnDataList_SPCResultFilename_ctl00")[0];

        var div = document.getElementById("ctl00_WebPartManager_ChartAreaWP_ChartPanel");
        oldChartURL.value = currentChartURL.value;
        currentChartURL.value = newChartURL;
        oldImgURL.value = div.getElementsByTagName("img")[0].src;

        deleteOldChart(oldChartURL.value, oldImgURL.value);

        var iStartIndex = newChartURL.indexOf("://", 0); // get the http:// or https:// index
        var iMidIndex = newChartURL.indexOf("/", iStartIndex + 3);  // get next      
        var sFileURL = newChartURL;

        if ((iStartIndex >= 0) && (iMidIndex > iStartIndex))
            sFileURL = newChartURL.substring(iMidIndex);

        //fix for IE doesn't have origin
        if (!window.location.origin) {
            window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
        }

        if (window.location.origin != undefined)
            newChartURL = window.location.origin + sFileURL;
        //newChartURL = window.location.origin + "/SPC/" + GetFilename(newChartURL) + ".htm";
        if (window.location.href.indexOf("localhost") > -1) {
            newChartURL = window.location.protocol + "//localhost" + sFileURL;
            //newChartURL = "http://localhost/SPC/" + GetFilename(newChartURL) + ".htm";
        }

        $.ajax({
            url: newChartURL, success: function (data) {
                div.innerHTML = data; //replace current chart panel with new htmlfile
            }
        });
    }

    function GetFilename(url) {
        if (url) {
            var m = url.toString().match(/.*\/(.+?)\./);
            if (m && m.length > 1) {
                return m[1];
            }
        }
        return "";
    }

    function deleteOldChart(oldChartURL, oldImgURL) {
        var oldChartURL = $("#ctl00_WebPartManager_ChartAreaWP_SPCTxnDataList_SPCResultFilenameOld_ctl00")[0];
        var oldImgURL = $("#ctl00_WebPartManager_ChartAreaWP_OldSPCImgFileName_ctl00")[0];

        //delete old html and img file then delete SPC Txn Data
        if (oldChartURL.value != "" && oldImgURL.value != "") {
            var inputData = oldChartURL.value + ',' + oldImgURL.value;
            $.ajax({
                type: "POST", //GET or POST or PUT or DELETE verb
                url: "ss_spcrealtimeservice.svc/deleteOldFiles", // Location of the service
                data: '{"inputDataString": "' + inputData + '"}', //Data sent to server
                contentType: "application/json; charset=utf-8", // content type sent to server
                dataType: "json", //Expected data format from server
                processdata: true, //True or False
                success: function (msg) {//On Successfull service call
                    //renderChart(msg)
                },
                error: SPCRealTimeMonitoring_ServiceError// When Service call fails
            });
        }
    }
}

function SPCRealTimeMonitoring_ServiceError(xhr, spcError) {
    stopSPCRealTimeMonitoring();
    //toggleStopResume();

    if (spcError != undefined)
        alert(spcError);
    else {
        alert("AJAX Error. Please refresh page");

        if (xhr.responseText) {
            var err = xhr.responseText;
            if (err)
                //error(err);
                alert(err);
            else
                error({ Message: "Unknown server error." })
        }
    }

    return;
}

function stopSPCRealTimeMonitoring() {
    $("#ctl00_WebPartManager_ChartAreaWP_Status_ctl00")[0].value = "OFF";
    toggleStopResume();
}

function SPCRealTime_ParamsEdited_RenderComplete(isAtTheEndOfProcessing, rowid, prm2) {
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var QueryParamString = "";

    for (var i = 0; i < rowData.length; i++) {
        var record = rowData[i];
        var currentRow;

        if (record._id_column.indexOf("empty") == -1) {
            if (QueryParamString != "")
                QueryParamString = QueryParamString + ';';

            QueryParamString = QueryParamString + record.ParamName + ':' + record.ParamValue;
        }
        else
            break;
    }

    $("#ctl00_WebPartManager_ChartAreaWP_txtQueryParams_ctl00")[0].value = QueryParamString;

    if ($("#ctl00_WebPartManager_ChartAreaWP_Status_ctl00")[0] != null && $("#ctl00_WebPartManager_ChartAreaWP_Status_ctl00")[0].value == "ON") {
        executeSPCService();
    }
}

/* Wafer Map */
function RenderMapDataInCanvasRaw(mapDataList, binDefinitionList, mapType, binType, deviceSizeX, deviceSizeY, dimensionX, dimensionY, nullBin, fitToScreen) {
    // Create array from the map data list
    var dataset = JSON.parse(mapDataList);
    mapDataList = null;

    var primaryBin = "";

    // Create array from the bin definition list
    var binDefinitionSet = null;
    if (binDefinitionList != "") {
        binDefinitionSet = JSON.parse(binDefinitionList);
        binDefinitionList = null;
        primaryBin = binDefinitionSet[0].code;
    }
    //var uniqueBinCodes = [...new Set(dataset.map(item => item.value))];

    var lookup = {};
    var uniqueBinCodes = [];

    var primaryColor = "#87dd44";
    var color = d3.scale.linear()
        .domain([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19])
        .range(["#3366cc", "#dc3912", "#ff9900", "#990099", "#0099c6", "#dd4477", "#b82e2e", "#316395", "#994499", "#22aa99", "#aaaa11", "#6633cc", "#e67300", "#8b0707", "#651067", "#5574a6", "#3b3eac", "#ffeeff", "#cfbecf", "#ff8bfa"]);

    if (deviceSizeX == null || deviceSizeY == null || deviceSizeX <= 0 || deviceSizeY <= 0) {
        deviceSizeX = 1;
        deviceSizeY = 1;
    }

    var xScale = 1;
    var yScale = 1;
    var fullX = deviceSizeX * dimensionX;
    var fullY = deviceSizeY * dimensionY;
    var deviceRatio = 1;

    if (fitToScreen == 1) {
        if (fullX > fullY) {
            deviceRatio = 1000 / fullX;
        }
        else {
            deviceRatio = 500 / fullY;
        }
        xScale = deviceRatio;
        yScale = deviceRatio;
    }
    else {
        if (deviceSizeX > 5 || deviceSizeX < 1 || deviceSizeY > 5 || deviceSizeY < 1) {
            if (deviceSizeX > deviceSizeY) {
                deviceRatio = 50 / deviceSizeX;
            }
            else {
                deviceRatio = 50 / deviceSizeY;
            }
        }
        else {
            deviceRatio = 10;
        }
        xScale = deviceRatio;
        yScale = deviceRatio;
    }

    var fullWidth = dimensionX * deviceSizeX * xScale;
    var fullHeight = dimensionY * deviceSizeY * yScale;
    var margin = { top: 10, right: 10, bottom: 10, left: 10 };

    if (fullWidth > 8000 || fullHeight > 8000) {
        var deviceRatio = 1;
        if (fullX > fullY) {
            deviceRatio = 8000 / fullX;
        }
        else {
            deviceRatio = 8000 / fullY;
        }
        xScale = deviceRatio;
        yScale = deviceRatio;
        fullWidth = dimensionX * deviceSizeX * xScale;
        fullHeight = dimensionY * deviceSizeY * yScale;
    }

    var scaledDeviceSizeX = deviceSizeX * xScale;
    var scaledDeviceSizeY = deviceSizeY * yScale;

    function getSquare(canvas, evt) {
        var rect = canvas.node().getBoundingClientRect();
        return {
            x: (evt.clientX - rect.left) - (evt.clientX - rect.left) % scaledDeviceSizeX,
            y: fullHeight - scaledDeviceSizeY - ((evt.clientY - rect.top) - (evt.clientY - rect.top) % scaledDeviceSizeY)
        };
    }

    function fillSquare(context, x, y, sizeX, sizeY, sqColor) {
        context.beginPath();
        context.fillStyle = sqColor;
        context.fillRect(x, y, sizeX - 1, sizeY - 1);
    }

    function selectedSquare(context, x, y, sizeX, sizeY) {
        context.beginPath();
        context.lineWidth = "2";
        context.strokeStyle = "red";
        context.rect(x, y, sizeX, sizeY);
        context.stroke();
    }

    $('canvas').remove();
    var canvas = d3.select(".chart").append("canvas")
        .attr("id", "WaferMapCanvas")
        .attr("width", fullWidth + 5)
        .attr("height", fullHeight + 5);

    var context = canvas.node().getContext('2d');
    context.translate(0, fullHeight);
    context.scale(1, -1);

    context.clearRect(0, 0, canvas.width, canvas.height);

    var binTypeLength = 1;
    if (binType.toUpperCase() == "DECIMAL")
        binTypeLength = 3;
    else if (binType.toUpperCase() == "HEXADECIMAL")
        binTypeLength = 2;
    else if (binType.toUpperCase() == "INTEGER2")
        binTypeLength = 4;

    if (mapType == "" || mapType.toUpperCase() == "2DARRAY") {
        var y = dimensionY - 1;
        dataset.forEach(function (point) {
            for (var x = 0; x < dimensionX; x++) {
                var binCodeValue = point.value.substring(binTypeLength * x, (binTypeLength * x) + binTypeLength);
                if (binCodeValue != nullBin) {
                    if (!(binCodeValue in lookup)) {
                        lookup[binCodeValue] = 1;
                        uniqueBinCodes.push(binCodeValue);
                    }
                    if (primaryBin != "" && primaryBin == binCodeValue) {
                        fillSquare(context, x * scaledDeviceSizeX, y * scaledDeviceSizeY, scaledDeviceSizeX, scaledDeviceSizeY, primaryColor);
                    }
                    else {
                        fillSquare(context, x * scaledDeviceSizeX, y * scaledDeviceSizeY, scaledDeviceSizeX, scaledDeviceSizeY, color(uniqueBinCodes.indexOf(binCodeValue)));
                    }
                }
            }
            y--;
        });
    }
    else if (mapType.toUpperCase() == "ROW/COLUMN") {
        dataset.forEach(function (point) {
            var valIndex = 0;
            var binCodeValuesLength = point.value.length / binTypeLength;
            var binCodeXMax = binCodeValuesLength + point.x;
            for (var x = point.x; x < binCodeXMax; x++) {
                var binCodeValue = point.value.substring(binTypeLength * valIndex, (binTypeLength * valIndex) + binTypeLength);
                if (binCodeValue != nullBin) {
                    if (!(binCodeValue in lookup)) {
                        lookup[binCodeValue] = 1;
                        uniqueBinCodes.push(binCodeValue);
                    }
                    if (primaryBin != "" && primaryBin == binCodeValue) {
                        fillSquare(context, x * scaledDeviceSizeX, point.y * scaledDeviceSizeY, scaledDeviceSizeX, scaledDeviceSizeY, primaryColor);
                    }
                    else {
                        fillSquare(context, x * scaledDeviceSizeX, point.y * scaledDeviceSizeY, scaledDeviceSizeX, scaledDeviceSizeY, color(uniqueBinCodes.indexOf(binCodeValue)));
                    }
                }
                valIndex++;
            }
        });
    }
    else if (mapType.toUpperCase() == "ARRAY") {
        dataset.forEach(function (point) {
            var valIndex = 0;
            for (var y = dimensionY - 1; y >= 0; y--) {
                for (var x = 0; x < dimensionX; x++) {
                    var binCodeValue = point.value.substring(binTypeLength * valIndex, (binTypeLength * valIndex) + binTypeLength);
                    if (binCodeValue != nullBin) {
                        if (!(binCodeValue in lookup)) {
                            lookup[binCodeValue] = 1;
                            uniqueBinCodes.push(binCodeValue);
                        }
                        if (primaryBin != "" && primaryBin == binCodeValue) {
                            fillSquare(context, x * scaledDeviceSizeX, y * scaledDeviceSizeY, scaledDeviceSizeX, scaledDeviceSizeY, primaryColor);
                        }
                        else {
                            fillSquare(context, x * scaledDeviceSizeX, y * scaledDeviceSizeY, scaledDeviceSizeX, scaledDeviceSizeY, color(uniqueBinCodes.indexOf(binCodeValue)));
                        }
                    }
                    valIndex++;
                }
            }
        });
    }
    else if (mapType.toUpperCase() == "COORDINATE") {
        dataset.forEach(function (point) {
            var binCodeValue = point.value;
            if (binCodeValue != nullBin) {
                if (!(binCodeValue in lookup)) {
                    lookup[binCodeValue] = 1;
                    uniqueBinCodes.push(binCodeValue);
                }
                if (primaryBin != "" && primaryBin == binCodeValue) {
                    fillSquare(context, point.x * scaledDeviceSizeX, point.y * scaledDeviceSizeY, scaledDeviceSizeX, scaledDeviceSizeY, primaryColor);
                }
                else {
                    fillSquare(context, point.x * scaledDeviceSizeX, point.y * scaledDeviceSizeY, scaledDeviceSizeX, scaledDeviceSizeY, color(uniqueBinCodes.indexOf(binCodeValue)));
                }
            }
        });
    }

    //Build the Map Data Legends
    $('#WebPart_MapDataLegendsWP').empty();
    var legendsdiv = document.getElementById('WebPart_MapDataLegendsWP');
    for (var x = 0; x < uniqueBinCodes.length; x++) {
        if (uniqueBinCodes[x] != nullBin) {
            var boxContainer = document.createElement("DIV");
            var box = document.createElement("DIV");
            box.className = "legendsbox";
            var label = document.createElement("SPAN");
            label.className = "chartlegendstooltip";
            var labeltooltip = document.createElement("SPAN");
            labeltooltip.className = "chartlegendstooltiptext";

            if (binDefinitionSet == null) {
                label.innerHTML = uniqueBinCodes[x];
                box.style.backgroundColor = color(x);
            }
            else {
                var filteredBinDef = binDefinitionSet.filter(function (obj) {
                    return obj.code == uniqueBinCodes[x];
                });
                if (filteredBinDef[0] != null) {
                    if (filteredBinDef[0].description != null) {
                        var theDescription = filteredBinDef[0].description;
                        if (theDescription.length > 20) {
                            labeltooltip.innerHTML = theDescription;
                            label.innerHTML = uniqueBinCodes[x] + " - " + theDescription.substring(0, 17) + "...";
                            label.appendChild(labeltooltip);
                        }
                        else {
                            label.innerHTML = uniqueBinCodes[x] + " - " + filteredBinDef[0].description;
                        }
                    }
                    else {
                        label.innerHTML = uniqueBinCodes[x];
                    }

                    if (filteredBinDef[0].code == primaryBin) {
                        box.style.backgroundColor = primaryColor;
                    }
                    else {
                        box.style.backgroundColor = color(x);
                    }
                }
                else {
                    label.innerHTML = uniqueBinCodes[x];
                    box.style.backgroundColor = color(x);
                }
            }

            boxContainer.appendChild(box);
            boxContainer.appendChild(label);

            legendsdiv.appendChild(boxContainer);
        }
    }
    document.getElementById("ctl00_WebPartManager_MapDataDisplayWP_BinDefinitions_ctl00").value = uniqueBinCodes;

    //Add mouse click event
    canvas.node().addEventListener('click', function (evt) {
        var mousePos = getSquare(canvas, evt);
        var selectedX = Math.round(mousePos.x / scaledDeviceSizeX);
        var selectedY = Math.round(mousePos.y / scaledDeviceSizeY);
        var datasetSelectedIndex = dimensionY - selectedY - 1;
        var selectedValue = nullBin;
        var substringStart = 0;
        if (mapType == "" || mapType.toUpperCase() == "2DARRAY") {
            substringStart = binTypeLength * selectedX;
            selectedValue = dataset[datasetSelectedIndex].value.substring(substringStart, substringStart + binTypeLength);
        }
        else if (mapType.toUpperCase() == "ROW/COLUMN") {
            var result = dataset.filter(function (obj) {
                return obj.y == selectedY;
            });
            if (result[0].value != "") {
                var valIndex = selectedX - result[0].x;
                if (valIndex >= 0) {
                    substringStart = binTypeLength * valIndex;
                    selectedValue = result[0].value.substring(substringStart, substringStart + binTypeLength);
                }
            }
        }
        else if (mapType.toUpperCase() == "ARRAY") {
            substringStart = (dimensionX * binTypeLength * datasetSelectedIndex) + (binTypeLength * selectedX);
            selectedValue = dataset[0].value.substring(substringStart, substringStart + binTypeLength);
        }
        else if (mapType.toUpperCase() == "COORDINATE") {
            var result = dataset.filter(function (obj) {
                return obj.x == selectedX && obj.y == selectedY;
            });
            if (result[0].value != "") {
                selectedValue = result[0].value;
            }
        }
        if (selectedValue != nullBin) {
            document.getElementById("ctl00_WebPartManager_MapDataDisplayWP_scsMapDataDisplay_XDimension_ctl00").value = selectedX;
            document.getElementById("ctl00_WebPartManager_MapDataDisplayWP_scsMapDataDisplay_YDimension_ctl00").value = selectedY;
            document.getElementById("ctl00_WebPartManager_MapDataDisplayWP_scsMapDataDisplay_CurrentBinCode_ctl00").value = selectedValue;
        }
    }, false);

    //Remove script tags
    var docHeader = __page.get_documentHeader();
    $(docHeader).children("script").remove();
}

function RenderMapDataInFullViewCanvas() {
    var margin = { top: 20, right: 20, bottom: 20, left: 20 };
    var fullWidth = 980;
    var fullHeight = 480;

    if (parent.document.body.classList.contains('Horizon-theme')) {

        if (parent.document.body.classList.contains('mobile-device')) {

            fullWidth = screen.width - 50;
            fullHeight = screen.height - 203;

        } else {
            var fullviewwebpart = $("#WebPart_BlankWP0.webpart.fullviewchart");

            fullWidth = fullviewwebpart.outerWidth() != 0 ? fullviewwebpart.outerWidth() - 30 : 960; //960;
            fullHeight = fullviewwebpart.outerHeight() != 0 ? fullviewwebpart.outerHeight() - 30 : 520;//520;
        }

    }

    var canvas = d3.select(".fullviewchart").append("canvas")
        .attr("id", "FullViewWaferMapCanvas")
        .attr("width", fullWidth + 10)
        .attr("height", fullHeight + 10)
        .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

    var context = canvas.node().getContext('2d');

    var existingcanvas = parent.document.getElementById('WaferMapCanvas');
    var ecanvaswidth = existingcanvas.offsetWidth;
    var ecanvasheight = existingcanvas.offsetHeight;
    var canvasRatio = 1;

    if (ecanvaswidth / fullWidth > ecanvasheight / fullHeight) {
        deviceRatio = fullWidth / ecanvaswidth;
    }
    else {
        deviceRatio = fullHeight / ecanvasheight;
    }

    context.drawImage(existingcanvas, 0, 0, ecanvaswidth * deviceRatio, ecanvasheight * deviceRatio);
}

function ClearMapDataCanvas() {
    $('canvas').remove();
    $('#WebPart_MapDataLegendsWP').empty();
    //localStorage.clear();
    //sessionStorage.clear();

    //Remove script tags
    var docHeader = __page.get_documentHeader();
    $(docHeader).children("script").remove();
}

function MapDataDisplayMode(mode) {
    // Hide navigation menu
    if (mode > 0) {
        window.parent.$("#ctl00_Header").hide();
        window.parent.$("#ctl00_NavigationMenu_NavigationMenu").hide();
        window.parent.$("#ctl00_MasterFooter").width("100%");
        window.parent.$(".pageTablist").hide();
        window.parent.$(".forFirstlevelTabs").hide();
        window.parent.$(".form-container").width("1920px");
        window.parent.$("#SessionTimeoutType").val("alive");
        $(".body-main").width("1920px");
        $(".form-container").width("1920px");
        $(".header-container").width("1920px");
        $(".footer").width("1920px");
        $("#ctl00_WebPartManager_ReportContainerWP_KeepAliveButton").hide();

        var container = window.parent.$("#tabContainerControl");
        $("iframe", container).width("1920px");
    }
    else {
        $("#ctl00_Header").show();
        $("#ctl00_NavigationMenu_NavigationMenu").show();
        $("#ctl00_MasterFooter").width("100%");
        $(".pageTablist").show();
        $(".forFirstlevelTabs").show();
        $(".form-container").width("100%");
        $("#SessionTimeoutType").val("alive");

        // Adjust the inner iframe
        $(".body-main").width("100%");
        $(".form-container").width("100%");
        $("#ctl00_WebPartManager_ReportContainerWP_KeepAliveButton").show();
    }
    // find the iframe we are contained in and set width
    var container = window.parent.$("#tabContainerControl");
    $("iframe", container).width("100%");
}

$(">form", document.body).on("page.initialized", function () {

    if ($(document.body).is(".AJAXMaster-page.mobile-device") && !$(".pageTitleMobile", this).length) {
        var $tabCont = $(window.frameElement.parentElement.parentElement.parentElement);
        var txt = $(".cs-nav-tabs > .cs-nav-wrapper > ul > li:first-child .tab-caption-text", $tabCont).text();

        $(".zone-static", this).after("<div class=pageTitleMobile><span class=pageTitle /></div>");
        $(".pageTitleMobile > span.pageTitle", this).text(txt);
    }

});

/* Service Data Selection Popup */
function ServiceDataSelection_RenderComplete(isAtTheEndOfProcessing, rowid, prm2) {
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var btnClear = document.getElementById('ctl00_WebPartManager_BlankWP_DisplayError');
    var HiddenTxt = document.getElementsByName('ctl00$WebPartManager$BlankWP$HiddenNoResultsFound$ctl00');

    if (rowData.length <= 0 && HiddenTxt[0].value == "Clicked") {
        HiddenTxt[0].value = "";
        btnClear.click();
        return true;
    }
    else {
        if (rowData[0].Lot == "" && HiddenTxt[0].value == "Clicked") {
            HiddenTxt[0].value = "";
            btnClear.click();
            return true;
        }
    }
}

/* WIP Main Tile Required Activities */
function WIPMain_RequiredActivitiesEx_renderComplete(isAtTheEndOfProcessing, rowid, prm2) {
    var iMaxTileCount = 12;
    var grid = $(this.GridID);
    var rowData = grid.getRowData();

    var ActivityTileID_Prefix = 'ctl00_WebPartManager_scsWIPMainActivityWP_ActivityTile0';

    //alert('RequiredActivitiesEx_RenderCompleted');

    // regardless of data, set all tiles as hidden 1st
    for (x = 0; x < iMaxTileCount; x++) {
        $("#" + ActivityTileID_Prefix + x.toString()).hide();
    }

    if (rowData[0].Activity != "") // VisibleRows = true, no data
    {
        // set the upper limit to our maximum number of tiles 
        var iActivityCountLimit;
        if (rowData.length >= iMaxTileCount)
            iActivityCountLimit = iMaxTileCount;
        else
            iActivityCountLimit = rowData.length;

        for (var i = 0; i < iActivityCountLimit; i++) {
            if (rowData[i]['Activity'] != "") {
                var ActivityTileID = "#" + ActivityTileID_Prefix + i.toString();
                $(ActivityTileID).show();

                var oActivityMessage = ActivityTileID + " .WIPMain_Activity_Message"
                $(oActivityMessage).html(rowData[i]['Message']);

                var oActivityMessageDiv = ActivityTileID + "_StatusMessageDiv";
                $(oActivityMessageDiv).removeClass("WaferSamplingActivity");

                var oStatusMessage = ActivityTileID + " .WIPMain_Activity_StatusMessage"

                if (rowData[i]['Status'] == 'NoStatus')
                    $(oStatusMessage).html('');
                if (rowData[i]['Status'] == 'scsError')
                    $(oStatusMessage).html('Incomplete');
                else
                    $(oStatusMessage).html(rowData[i]['Status']);

                var oSubMessage = ActivityTileID + " .WIPMain_Activity_SubMessage"
                $(oSubMessage).html("");

                var oStatusDiv = ActivityTileID + "_StatusDiv";
                $(oStatusDiv).removeClass("WIPMain_Activity_StatusDiv_Optional");
                $(oStatusDiv).removeClass("WIPMain_Activity_StatusDiv_Pending");
                $(oStatusDiv).removeClass("WIPMain_Activity_StatusDiv_Complete");
                $(oStatusDiv).removeClass("WIPMain_Activity_StatusDiv_scsError");
                $(oStatusDiv).removeClass("WIPMain_Activity_StatusDiv_WafersSelected");
                $(oStatusDiv).removeClass("WIPMain_Activity_StatusDiv_WafersNotSelected");
                $(oStatusDiv).removeClass("WIPMain_Activity_StatusDiv_WafersNotEnforced");

                var StatusValue = rowData[i]['StatusValue'].toString();

                switch (StatusValue) {
                    case "0":
                        $(oStatusDiv).addClass("WIPMain_Activity_StatusDiv_Pending");
                        break;
                    case "1":
                        $(oStatusDiv).addClass("WIPMain_Activity_StatusDiv_Complete");
                        break;
                    case "2":
                        $(oStatusDiv).addClass("WIPMain_Activity_StatusDiv_Optional");
                        break;
                    case "4":
                        $(oStatusDiv).addClass("WIPMain_Activity_StatusDiv_scsError");
                        break;
                    case "5":
                        $(oStatusDiv).addClass("WIPMain_Activity_StatusDiv_WafersSelected");
                        break;
                    case "6":
                        $(oActivityMessageDiv).addClass("WaferSamplingActivity");
                        $(oStatusDiv).addClass("WIPMain_Activity_StatusDiv_WafersNotSelected");
                        break;
                    case "7":
                        $(oStatusDiv).addClass("WIPMain_Activity_StatusDiv_WafersNotEnforced");
                        break;
                }

                var vEnableClick = rowData[i]['ClickEnabled'];
                var ActivityValue = rowData[i]['ActivityValue'];

                if (vEnableClick == "Y")
                {
                    $(ActivityTileID).bind('click', { msg: ActivityValue }, function (event) { WIPMain_ActivityTile_Click(event.data.msg); });
                }
            }
            else
                break;
        }
    }

    return true;
}

function WIPMain_ActivityTile_Click(ActivityValue) {
    //Current Enum values
    //0. Binning
    //1. CarrierValidate
    //2. CheckSheet -- New Page
    //3. EProcedure -- New Page
    //4. InProcessSplit
    //5. LotPacking
    //6. Mask (Equipment Setup)
    //7. MaterialSetup
    //8. ModifyMaint
    //9. Recipe (Equipment Setup)
    //10. RejectItems
    //11. RejectLots
    //12. ReservedEquipment -- not yet done
    //13. Sampling
    //14. Sorting
    //15. TestProgram
    //16. ToolPlan (Equipment Setup)
    //17. WIPData    
    //20. WaferSampling
    var ActivityLink;
    var ActivityButtonID_Prefix = 'ctl00_WebPartManager_scsWIPMainActivityWP_btn';
    switch (ActivityValue) {
        case "0":
            ActivityLink = "Binning";
            break;
        case "1":
            ActivityLink = "CarrierValidate";
            break;
        case "2":
            ActivityLink = "CheckSheet";
            break;
        case "3":
            ActivityLink = "EProcedure";
            break;
        case "4":
            ActivityLink = "InProcessSplit";
            break;
        case "5":
            ActivityLink = "LotPacking";
            break;
        case "6":
            ActivityLink = "Mask";
            break;
        case "7":
            ActivityLink = "MaterialSetup";
            break;
        case "8":
            ActivityLink = "ModifyMaint";
            break;
        case "9":
            ActivityLink = "Recipe";
            break;
        case "10":
            ActivityLink = "RejectItems";
            break;
        case "11":
            ActivityLink = "RejectLots";
            break;
        case "12":
            ActivityLink = "ReservedEquipment";
            break;
        case "13":
            ActivityLink = "Sampling";
            break;
        case "14":
            ActivityLink = "Sorting";
            break;
        case "15":
            ActivityLink = "TestProgram";
            break;
        case "16":
            ActivityLink = "ToolPlan";
            break;
        case "17":
            ActivityLink = "WIPData";
            break;
        case "20":
            ActivityLink = "WaferSampling";
            break;
    }

    var btnActivity = document.getElementById(ActivityButtonID_Prefix + ActivityLink);
    btnActivity.click();
    return true;
}

/* WIP Main R2 */
function scsWIPMainR2_LotDataGrid_rowDelete(isAtTheEndOfProcessing, rowid, prm2) {
    /*
        This function is called when the row delete button (trashcan) is clicked, the row data has NOT been removed from the grid yet.
        If the grid has VisibleRows value set, the rowData.length value will never be 0, even if there is no actual data in the grid, hence the else condition
        The 'else' condition will check to see if there is a container value in the 2nd row, if present means that there will still be row data after the delete so DO NOT call the clear function.        
    */
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var btnClear = document.getElementsByClassName('action-icon-reset')[0];
    var btnDelete = document.getElementById('ctl00_WebPartManager_WIPMain_LotItemWP_LotGridWP_WIPMain_LotItem_DeleteBtn');
    var ContainerToDelete = document.getElementsByName('ctl00$WebPartManager$WIPMain_LotItemWP_LotGridWP$WIPMain_LotItem_ContainerToDelete$ctl00');
    var FoundContainer;
    if (rowData.length <= 0) {
        btnClear.click();
        return false;
    }
    else {
        if (rowData[1].Container == "" && rowid == "000000") {
            btnClear.click();
            return false;
        }
        else {
            for (var i = 0; i < rowData.length; i++) {
                if (rowData[i]['_id_column'] == rowid) {
                    FoundContainer = rowData[i];
                }

                if (FoundContainer && FoundContainer.Container != "") {
                    ContainerToDelete[0].value = FoundContainer.Container;
                    btnDelete.click();
                    return true;
                    break;
                }
            }
        }
    }
}

/* WIP Main Advanced for timer process confirmation*/
function WIPMainAdvanced_TxnConfirmation() //renderCompleted
{
    jConfirm("Executing this transction will also execute timer disposition actions. Do you want to proceed?", 'WARNING', function (r) {
        if (r == true) {
            var btnSubmitTxn = document.getElementById('ctl00_WebPartManager_scsWIPMain_ControllerWP_Main_HiddenSubmit');//document.getElementById('ctl00_WebPartManager_SS_WIPMain_HomeWP_Main_HiddenSubmit');
            btnSubmitTxn.click();
        }
    }, 'Warning');

    return true;
}

function WIPEqpSetupToolGrid_rowAdd(isAtTheEndOfProcessing, rowid, prm2) {
    var theGrid = $(this.GridID);
    var jqRowData = theGrid.getRowData();
    var currentRow;

    for (var x = 1; x <= jqRowData.length; x++) {
        currentRow = $("tr", theGrid)[x];
        var td = $("td", currentRow);
        if (td.hasClass('ui-jqgrid-column-image')) {
            var toolTipTxt = 'Selection Values';
            var elem = $('#ctl00_WebPartManager_SS_WIPEquipmentSetupWP_HiddenToolTipLabel');
            if (elem != null && elem.length) {
                toolTipTxt = elem.text();
            }
            $("input", td).attr('title', toolTipTxt);
        }
    }
    return false;
}

function scsWIPMain_ItemDataGrid_renderCompleted() {

    var theGrid = $(this.GridID);
    var rowData = theGrid.getRowData();

    var isEnforced = false;
    var isTrackIn = $('#ctl00_WebPartManager_scsWIPMain_ControllerWP2_Main_TrackInRadioButton_ctl00').is(":checked");
    var isWaferSampling = rowData.length > 0 && rowData[0].scsWaferSamplingMethod != "";

    if (isTrackIn && isWaferSampling) {
        for (var i = 0; i < rowData.length; i++) {
            var isSamplingEnforced = rowData[i].scsSamplingEnforced == "True";
            var currChkBox = $('#jqg_ctl00_WebPartManager_WIPMain_LotItemWP_ItemGridWP_scsWIPMain_ItemDataGrid_' + rowData[i]._id_column);
            var currChkBoxTD = currChkBox.closest('td');

            if (isSamplingEnforced) {
                isEnforced = true;
                currChkBox.attr('disabled', 'disabled');
                currChkBoxTD.attr('disabled', 'disabled');
                currChkBox.click(function (e) {
                    e.stopPropagation();
                });
            }
        }

        if (isEnforced) {
            $('#jqgh_ctl00_WebPartManager_WIPMain_LotItemWP_ItemGridWP_scsWIPMain_ItemDataGrid_cb').hide();
        }
    }
}

function scsEqpMain_ItemDataGrid_renderCompleted() {

    var theGrid = $(this.GridID);
    var rowData = theGrid.getRowData();

    var isEnforced = false;
    var isTrackIn = $('#ctl00_WebPartManager_SS_WIPMain_HomeWP_Main_TrackInRadioButton_ctl00').is(":checked");
    var isWaferSampling = rowData.length > 0 && rowData[0].scsWaferSamplingMethod != "";

    if (isTrackIn && isWaferSampling) {
        for (var i = 0; i < rowData.length; i++) {
            var isSamplingEnforced = rowData[i].scsSamplingEnforced == "True";
            var currChkBox = $('#jqg_ctl00_WebPartManager_BlankWP_WIPMain_ItemDataGrid_' + rowData[i]._id_column);
            var currChkBoxTD = currChkBox.closest('td');

            if (isSamplingEnforced) {
                isEnforced = true;
                currChkBox.attr('disabled', 'disabled');
                currChkBoxTD.attr('disabled', 'disabled');
                currChkBox.click(function (e) {
                    e.stopPropagation();
                });
            }
        }

        if (isEnforced) {
            $('#jqgh_ctl00_WebPartManager_BlankWP_WIPMain_ItemDataGrid_cb').hide();
        }
    }
}

/* codes for the new Online Traveler page (scsOnlineTravelerR2_VP) start here */
function OnlineTravelerR2_AddDrillDownIndicator(row, jqRows, grid) {
    var td = $("td:visible[role='gridcell']", jqRows[(parseInt(row['SEQ']) - 1) % jqRows.length]).first();
    if (!td.hasClass('drill-down-indicator-wrapper')) {
        td.addClass('drill-down-indicator-wrapper');
        td.append(document.createElement("div"));
        $("div", td).append(document.createElement("span"));
        $("div span", td).append(document.createElement("i"));
        $("div span i", td).addClass('drill-down-indicator-true-2');
        $("div span i", td).attr("id", "ctl00_WebPartManager_BlankWP_OnlineTraveler_TransactionDetailIcon");
        $("div span i", td).attr("title", "See transaction details");
        $("div span i", td).on("click", function (event) {
            grid._onSelectRow(td.closest('tr')[0].id, true, { target: td });
            $("#ctl00_WebPartManager_BlankWP_OnlineTraveler_SelectionId_ctl00").attr("disabled", "disabled");
            $("#online_traveler_reset_button").attr("disabled", "disabled");
            return true;
        });
    }
}

function OnlineTravelerR2_BreadcrumbInit(breadcrumbId) {
    var breadcrumb = $find(breadcrumbId);
    if (breadcrumb) {
        breadcrumb.clearStorage();
        const BreadcrumbLevels = { MainlineGrid: 1 };
        var maxLevel = "MaxBreadcrumbLevel_" + __page.get_CallStackKey();

        if (typeof breadcrumb == 'undefined') {
            console.log('There is no ability to bind Breadcrumb to grids. Breadcrumb is not defined!');
            return;
        }

        OnlineTravelerR2_BindBreadcrumbToGrid(breadcrumb);
    }
}


function OnlineTravelerR2_BindBreadcrumbToGrid(breadcrumb) {
    var $grid = $("table[id$='OnlineTraveler_Grid']");
    var gridLevel = 0;

    var gridStateReset = function (commandArgs, context, gridLevel) {
        $("#ctl00_WebPartManager_BlankWP_OnlineTraveler_SelectionId_ctl00").removeAttr("disabled");
        $("#online_traveler_reset_button").removeAttr("disabled");
        __page.postback(context._element.id, gridLevel);
    };
    var gridClickHandler = function (event) {
        if (!$(event.target).hasClass('drill-down-indicator-true-2'))
            return false;

        var selectedRow = event.currentTarget;
        var val = OnlineTravelerR2_FindTitleForBreadcrumbCommand(selectedRow);
        var commandTitle = "<input type='submit' id='ctl00_WebPartManager_BlankWP_OnlineTraveler_Return' title='Return to Online Traveler grid' class='cs-button-image' \
        style='width: 28px;background-repeat:no-repeat;background-position:center;background-image:url(./assets/image/cmdBack24.svg);\
        border-color:#D4D4D4;margin-top: 0px !important;margin-left: -4px !important;' value=''><span class='seq-title'>Sequence " + val + "</span>";
        var cmd = new JavascriptCommand(commandTitle, gridStateReset, gridLevel);
        breadcrumb.removeCommand(cmd);
        breadcrumb.addCommand(cmd);
    };
    addClickHandlerToGridRows($grid, gridClickHandler);
}

function OnlineTravelerR2_FindTitleForBreadcrumbCommand(row) {
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

function OnlineTravelerR2_AddSlideoutToogler(gridID) {
    let $wpSearch = $(".common-search-panel");
    let showFunnel = function (isClick) {
        $(".global-funnel").remove();
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
    };

    setTimeout(function () { showFunnel(false); }, 200);

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

    if (document.URL.indexOf("IsFloatingFrame=") != -1) {
        let $pageContainer = $wpSearch.closest(".page-container");
        $pageContainer.addClass("search-panel-hidden");
    }
}
/* codes for the new Online Traveler page (scsOnlineTravelerR2_VP) end here */

function SimpleWIPMain_onActivityPageSubmitted(callStackKey) {

    //find the Simple wip main iframe
    var $iframe = GetIFrameByCallStackKey(callStackKey);
    if ($iframe) {
        var displaySPC = false;
        if (__page.get_virtualPageName() === "SS_WIPDataPopupVP") {
            displaySPC = true;
        }
        const refresh = new CustomEvent('refresh', {
            detail: displaySPC
        });
        $iframe.dispatchEvent(refresh);
    }

    //close the activity page
    __page.closeTab('');

    //redirect to simple wip main
    var $tab = __page.$getPageTabControl().scrollableTabs('getTab', { callStackKey: callStackKey });
    if ($tab.length > 0)
        $tab[0].children[0].click();
}

function GetIFrameByCallStackKey(callStackKey) {
    var $result;
    var $tabs = $("div#tabContainerControl ul#tablist li[role='tab'] a[role='presentation", window.parent.document);
    $tabs.each(function () {
        var item = $(this);
        let id = item[0].hash;
        if (id === "#tabContainerControl_" + callStackKey) {
            var $t = $(id, window.parent.document);
            var $iframe = $("iframe", $t);
            $result = $iframe[0];
            return true;
        }

    });
    return $result;
}

function LandingPage_onPageReload() {
    var div = window.parent.document.getElementsByTagName('iframe')[0].contentWindow.document.getElementById('WebPart_scsWIPChartWP_UIComponent');

    if (div) {
        const refresh = new Event('refreshLandingPage');
        div.dispatchEvent(refresh);
    }
}

function getTabsByVPName(vpName) {
    var $result = [];
    var $tabs = $("div#tabContainerControl ul#tablist li[role='tab'] a[role='presentation", window.parent.document);
    $tabs.each(function () {
        var item = $(this);
        let id = item[0].hash;
        var $t = $(id, window.parent.document);
        var $iframe = $("iframe", $t);
        let regex = /\w+\.aspx/;
        let vp = $iframe[0].src.match(regex);
        if (vp == $.trim(vpName).replace('*', '') + ".aspx") {
            $result.push(item);
        }
    });
    return $result;
}

function LandingPage_onRefreshDispatchList() {
    var $tab = getTabsByVPName("scsLandingPage_VP");
    if ($tab) {
        $tab.forEach(function (element) {
            let id = element[0].hash;
            var $t = $(id, window.parent.document);
            var $iframe = $("iframe", $t);
            var div = $iframe[0].contentWindow.document.getElementById('WebPart_scsDispatchListWP_UIComponent');
            if (div) {
                const refresh = new Event('refresh');
                div.dispatchEvent(refresh);
            }
        });
    }
}

function LandingPage_onPageReload() {
    var div = window.parent.document.getElementsByTagName('iframe')[0].contentWindow.document.getElementById('WebPart_scsGeneralWP_UIComponent');

    if (div) {
        const refresh = new Event('refreshLandingPage');
        div.dispatchEvent(refresh);
    }
}

function Matrix_AddSlideoutToogler(gridID) {

    if (document.URL.indexOf("IsFloatingFrame=") != -1) {
        $(function () {
            if ($("#ctl00_WebPartManager_ButtonsBar_SaveAsNew").css("display") === "none") {
                var $product = $("#ctl00_WebPartManager_CriteriaWP_CollapsibleSectionsAccordion_ctl03_ObjectChanges_Product_Edit");
                if ($product.length == 0)
                    $product = $("#ctl00_WebPartManager_CriteriaWP_CollapsibleSectionsAccordion_ctl03_ObjectChanges_scsProduct_Edit");
                if ($product.length && $product[0].hasAttribute("disabled")) {
                    parent.pop.setTitle("View");
                    if (parent.pop.getTitle !== "View")
                        parent.pop.showFrame();
                    $("#WebPart_CriteriaWP_UIComponent input[type='submit']").attr("disabled", "disabled");
                    $("#WebPart_CriteriaWP_UIComponent input[type='text']").attr("disabled", "disabled");
                    $("#WebPart_CriteriaWP_UIComponent input[type='checkbox']").attr("disabled", "disabled");
                    $("#WebPart_CriteriaWP_UIComponent .cs-picklist").attr("disabled", "disabled");
                    //$("#ctl00_WebPartManager_CriteriaWP_CollapsibleSectionsAccordion_ctl02_arrow").click();
                    $("#edit_ctl00_WebPartManager_CriteriaWP_CollapsibleSectionsAccordion_ctl05_ObjectChanges_Details").unbind("click");

                }
                else {
                    parent.pop.setTitle("Add");
                    if (parent.pop.getTitle !== "Add")
                        parent.pop.showFrame();
                }
            }
            else {
                parent.pop.setTitle("Edit");
                if (parent.pop.getTitle !== "Edit")
                    parent.pop.showFrame();
            }
        });
    }

    if ($("#WebPart_SelectionControlsWP").hasClass("common-search-panel")) {
        $("#WebPart_SelectionControlsWP").css("width", "420px");
        //$("#WebPart_SelectionControlsWP").css("height", "800px");
        $("#WebPart_SelectionControlsWP").css("overflow-y", "auto");
        let grid = jQuery("#" + gridID);
        $("#WebPart_SelectionControlsWP").css("height", grid.height);
        let $wpSearch = $("#WebPart_SelectionControlsWP_UIComponent");
        let showFunnel = function (isClick) {
            $(".global-funnel").remove();
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
        };

        $(function () { showFunnel(false); });

        let showAudit = function (isClick) {
            let $grid = $("#gbox_" + gridID);
            if ($grid.css("display") != "none") {
                let sep1 = $("#gview_" + gridID + " table.navtable tr td:nth-last-child(2)");
                if (sep1.hasClass("ui-state-disabled")) {
                    sep1.remove();
                }
                let sep2 = $("#gview_" + gridID + " table.navtable tr td:nth-last-child(3)");
                if (sep2.hasClass("ui-state-disabled")) {
                    sep2.remove();
                }
                let $tdLast = $("#gview_" + gridID + " table.navtable tr td:nth-last-child(3)");
                let funnelId = "audit_" + gridID;
                if ($tdLast.length && $tdLast[0].id !== funnelId) {
                    let $funnel = $("<td class='ui-pg-button ui-corner-all' style='display:none'><div class=ui-pg-div></div><span class='ui-icon audit-icon'></span></td>");
                    $funnel.prop("title", "View Audit Trail").prop("id", funnelId);
                    $funnel.insertAfter($tdLast);
                    var theGrid = jQuery("#" + gridID);
                    $funnel.click(function () {
                        if (theGrid[0].control.get_selectedCount() > 0)
                            $("#ctl00_WebPartManager_SS_SetupB_ButtonPanelWP_Act_ViewAudit").click();
                        else
                            __page.displayStatus("Please select a row to view audit trail.", "Warning", "Warning");
                    });

                    let funnelId1 = "details_" + gridID;
                    let $funnel1 = $("<td class='ui-pg-button ui-corner-all' style='display:none'><div class=ui-pg-div></div><span class='ui-icon details-icon'></span></td>");
                    $funnel1.prop("title", "View Details").prop("id", funnelId1);
                    $funnel1.insertAfter($tdLast);
                    $funnel1.click(function () {
                        if (theGrid[0].control.get_selectedCount() > 0)
                            $("#ctl00_WebPartManager_SS_SetupB_SelectionWP_ButtonView").click();
                        else
                            __page.displayStatus("Please select a row to view details.", "Warning", "Warning");
                    });
                }
            }
        };

        $(function () { showAudit(false); });

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

        if (document.URL.indexOf("IsFloatingFrame=") != -1) {
            let $pageContainer = $wpSearch.closest(".page-container");
            $pageContainer.addClass("search-panel-hidden");
        }

        let header = $("#WebPart_SelectionControlsWP > .matrix > .row:first-child")
        if ($("div", header).hasClass("header-search-panel")) {
            header.css("margin", "0px");
            $("#WebPart_SelectionControlsWP").before(header);
        }

        try {
            let footer = $("#WebPart_SelectionControlsWP > .matrix > .row:last-child")
            if ($("div", footer).hasClass("bottom-buttoms")) {
                footer.css("margin", "0px");
                footer.css("width", "420px");
                footer.addClass("common-search-panel");
                $("#WebPart_SelectionControlsWP").after(footer);
            }
        } catch (ex) {

        }
        MatrixFilterPanelSetHeight();
    }
}

function MatrixFilterPanelSetHeight() {
    var finalHeight;

    var formContainerHeight = $("#WebPart_SelectionControlsWP").parents(".form-container").height();
    finalHeight = formContainerHeight - 140;
    $("#WebPart_SelectionControlsWP").css("height", finalHeight + "px");
}

function MatrixSelectionGrid_renderCompleted() {

    var theGrid = jQuery(this.GridID);

    Matrix_AddSlideoutToogler("ctl00_WebPartManager_SS_SetupB_SelectionWP_SelectionGrid");

    $("#add_ctl00_WebPartManager_SS_SetupB_SelectionWP_SelectionGrid").on("click", function () {
        $("#ctl00_WebPartManager_SS_SetupB_SelectionWP_ButtonAdd").click();

    });

    $("#edit_ctl00_WebPartManager_SS_SetupB_SelectionWP_SelectionGrid").unbind("click");
    $("#edit_ctl00_WebPartManager_SS_SetupB_SelectionWP_SelectionGrid").on("click", function () {
        if (theGrid[0].control.get_selectedCount() > 0)
            $("#ctl00_WebPartManager_SS_SetupB_SelectionWP_ButtonEdit").click();
        else
            __page.displayStatus("Please select a row to edit.", "Warning", "Warning");

    });

    $("#del_ctl00_WebPartManager_SS_SetupB_SelectionWP_SelectionGrid").on("click", function () {
        if (theGrid[0].control.get_selectedCount() > 0)
            this.click();
        else
            __page.displayStatus("Please select a row to delete.", "Warning", "Warning");

    });

    $("#refresh_ctl00_WebPartManager_SS_SetupB_SelectionWP_SelectionGrid").unbind("click");
    $("#refresh_ctl00_WebPartManager_SS_SetupB_SelectionWP_SelectionGrid").on("click", function () {
        $("#ctl00_WebPartManager_SS_SetupB_SelectionWP_Selection_PageRefresh").click();

    });

    setTimeout(function (e) {
        AuditTrailSetGridHeight(theGrid);
        MatrixFilterPanelSetHeight();
    }, 0);

    window.addEventListener("resize", debounce(function (e) {
        AuditTrailSetGridHeight(theGrid);
        MatrixFilterPanelSetHeight();
    }));

    var gridMatrixAddBtn = document.getElementById('add_ctl00_WebPartManager_SS_SetupB_SelectionWP_SelectionGrid');
    var gridMatrixEditBtn = document.getElementById('edit_ctl00_WebPartManager_SS_SetupB_SelectionWP_SelectionGrid');
    var gridMatrixDeleteBtn = document.getElementById('del_ctl00_WebPartManager_SS_SetupB_SelectionWP_SelectionGrid');
    var gridMatrixRefreshBtn = document.getElementById('refresh_ctl00_WebPartManager_SS_SetupB_SelectionWP_SelectionGrid');

    gridMatrixAddBtn.hidden = true;
    gridMatrixEditBtn.hidden = true;
    gridMatrixDeleteBtn.hidden = true;
    gridMatrixRefreshBtn.hidden = true;

}

function scsContainerSearchGrid_renderCompleted(isAtTheEndOfProcessing, rowid, prm2) //renderCompleted
{

    let theGrid = jQuery(this.GridID);
    let jqRowData = theGrid.getRowData();

    for (let x = 0; x < jqRowData.length; x++) {
        currentRow = $("tr", theGrid)[x + 1];
        try {
            let imgSrc = 'assets/image/indicatorStatusCompleted16.svg';
            let style = 'width:2rem;height:2rem;margin-right:15px;';
            let title = 'True';

            if (jqRowData[x].InProcess != null && jqRowData[x].InProcess == "False") {
                imgSrc = 'assets/image/indicatorStatusFailed16.svg';
                style = 'width:2rem;height:2rem;margin-right:15px;';
                title = 'False';
            }

            let td = $("td", currentRow);
            if (td.hasClass('ui-jqgrid-column-image')) {
                $("input", td).attr('src', imgSrc);
                $("input", td).attr('style', style);
                $("input", td).attr('title', title);
            }
        } catch (ex) { }
    }

    //execute core function
    ContainerSearch_renderComplete(isAtTheEndOfProcessing, rowid, prm2);
}


function scsMultiLotModifyAttribute_ItemDataGrid_renderCompleted() {

    let theGrid = $(this.GridID);
    let rowData = theGrid.getRowData();

    for (let i = 0; i < rowData.length; i++) {
        let currChkBox = $('#ctl00_WebPartManager_SS_MLA_WP_LotInfoFieldGrid_scsApplyToChildLots_' + rowData[i]._id_column + '_dcb');
        let currChkBoxTD = currChkBox.closest('td');
        let isParentContainer = rowData[i].IsParentLot;

        if (isParentContainer === 'False') {

            currChkBox.attr('disabled', 'disabled');
            currChkBoxTD.attr('disabled', 'disabled');
            currChkBox.click(function (e) {
                e.stopPropagation();
            });
        }
    }
}

/* Multi-lots modify attributes lot delete*/
function scsMultiLotModifyAttribute_ItemDataGrid_rowDelete(isAtTheEndOfProcessing, rowid, prm2) {
    /*
    This function is called when the row delete button (trash can) is clicked (the row data has NOT been deleted from the grid yet)
    */
    var index = parseInt(rowid);
    if (index >= 0) {
        var btnRowDelete = document.getElementById('ctl00_WebPartManager_SS_MLA_WP_MultiLotsModifyAttrs_Delete');
        btnRowDelete.click();
    }
    return false;
}

/* Multi-lots Modify Attributes Confirmation Popup */
function scsMultiLotModifyAttribute_LotDeleteConfirmation() {
    jConfirm("You have modified the lot. Proceeding to delete will lose its changes. Delete and lose changes?", 'WARNING', function (r) {
        if (r == true) {
            var btnConfirmDelete = document.getElementById('ctl00_WebPartManager_SS_MLA_WP_MultiLotsModifyAttrs_ConfirmDelete');
            btnConfirmDelete.click();
            return true;
        }
    }, 'Warning');

    return false;
}


/* WIP Main R2 */
function scsWIPMainSimple_LotDataGrid_rowDelete(isAtTheEndOfProcessing, rowid, prm2) {
    /*
        This function is called when the row delete button (trashcan) is clicked, the row data has NOT been removed from the grid yet.
        If the grid has VisibleRows value set, the rowData.length value will never be 0, even if there is no actual data in the grid, hence the else condition
        The 'else' condition will check to see if there is a container value in the 2nd row, if present means that there will still be row data after the delete so DO NOT call the clear function.        
    */
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var btnClear = document.getElementsByClassName('action-icon-reset')[0];
    var btnDelete = document.getElementById('ctl00_WebPartManager_WIPMain_LotItemWP_LotGridWP_WIPMain_LotItem_DeleteBtn');
    var ContainerToDelete = document.getElementsByName('ctl00$WebPartManager$scsWIPMain_ControllerWP$WIPMain_LotItem_ContainerToDelete$ctl00');
    var FoundContainer;
    if (rowData.length <= 0) {
        btnClear.click();
        return false;
    }
    else {
        if (rowData[1].Container == "" && rowid == "000000") {
            btnClear.click();
            return false;
        }
        else {
            for (var i = 0; i < rowData.length; i++) {
                if (rowData[i]['_id_column'] == rowid) {
                    FoundContainer = rowData[i];
                }

                if (FoundContainer && FoundContainer.Container != "") {
                    ContainerToDelete[0].value = FoundContainer.Container;
                    btnDelete.click();
                    return true;
                    break;
                }
            }
        }
    }
}

/* Display Inline SPC statistical metrics */
function renderChartStats(data, $control, chartType) {
    if (!data || !data.result || !data.result.processValues) return;

    var options = data.specifications;

    if (chartType === "histogramChart" && options.histogramType) {
        renderHistogramParameters(data, $control);
    } else if (chartType === "probabilityPlot") {
        return;
    } else if (chartType === "attrControlChart") {
        return;
    } else if (chartType === "cumulativeSumChart") {
        return;
    } else if (chartType === "cumulativeCountChart") {
        return;
    } else if (chartType === "cumulatedUSumChart") {
        return;
    } else if (chartType === "defectPareto") {
        return;
    } else if (chartType === "barChart") {
        return;
    } else {
        //single value and other control charts
        if (options.displayDescriptiveStats) {
            renderDescriptiveStats(data, $control);
        }
    }
}

function renderDescriptiveStats(data, $control) {
    if (!data || !data.result || !data.result.processValues) return;

    var pv = data.result.processValues;
    var decimalPlaces = data.specifications.decimalPlaces || 4;

    var formatNum = function (val) {
        if (val === null || val === undefined) return "";
        var num = parseFloat(val);
        if (isNaN(num)) return "";
        return num.toFixed(decimalPlaces);
    };

    var variance = pv.calculatedSb ? Math.pow(pv.calculatedSb, 2) : null;

    var params = [
        { label: "Mean", value: formatNum(pv.calculatedXbb) },
        { label: "Std Dev.", value: formatNum(pv.calculatedSb) },
        { label: "Variance", value: formatNum(variance) },
        { label: "Range", value: formatNum(pv.range) },
        { label: "Min", value: formatNum(pv.calculatedMin) },
        { label: "Max", value: formatNum(pv.calculatedMax) },
        { label: "Valid Cases", value: pv.countOfValidValues || "" }
    ];

    var ownerId = ($control && $control.id) || "";
    var $scopeParent = $control.parent();
    $scopeParent.find('.spcDescriptiveStats[data-owner="' + ownerId + '"]').remove();

    var $table = $('<table class="spcDescriptiveStats" style="width:100%; margin-top:20px; margin-left:20px; margin-bottom:20px; border-collapse:collapse;">');
    if (ownerId) $table.attr("data-owner", ownerId);
    var $tbody = $('<tbody>');

    var midPoint = Math.ceil(params.length / 2);
    for (var i = 0; i < midPoint; i++) {
        var $row = $('<tr>');
        var $leftCell = $('<td style="padding:5px 10px; border:1px solid #ddd; width:50%;">');
        $leftCell.append($('<strong>').text(params[i].label + ": "));
        $leftCell.append($('<span>').text(params[i].value));
        $row.append($leftCell);

        if (i + midPoint < params.length) {
            var $rightCell = $('<td style="padding:5px 10px; border:1px solid #ddd; width:50%;">');
            $rightCell.append($('<strong>').text(params[i + midPoint].label + ": "));
            $rightCell.append($('<span>').text(params[i + midPoint].value));
            $row.append($rightCell);
        } else {
            $row.append($('<td style="padding:5px 10px; border:1px solid #ddd; width:50%;">'));
        }
        $tbody.append($row);
    }

    $table.append($tbody);
    var chartWidth = $control.outerWidth();
    if (chartWidth && chartWidth > 0) $table.css("width", chartWidth + "px");
    $control.after($table);
}

function renderHistogramParameters(data, $control) {
    if (!data || !data.result || !data.result.processValues) return;

    var pv = data.result.processValues;
    var specs = data.specifications || {};
    var subgroups = data.subgroups || [];

    var decimalPlaces = data.specifications.decimalPlaces || 4;

    var sigma = (pv.processSigma && pv.processSigma !== 0.0) ? pv.processSigma : (pv.sigmaEstimated || 0);
    var mean = pv.calculatedXbb || 0;

    var plus3StdDev = mean + 3 * sigma;
    var minus3StdDev = mean - 3 * sigma;
    var centerSpec = specs.target || specs.currentNominalValue || null;

    var formatNum = function (val) {
        if (val === null || val === undefined || val === "") return "";
        var num = parseFloat(val);
        if (isNaN(num)) return "";
        return num.toFixed(decimalPlaces);
    };

    var formatPercent = function (val) {
        if (val === null || val === undefined || val === "") return "";
        var num = parseFloat(val);
        if (isNaN(num)) return "";
        return num.toFixed(2) + "%";
    };

    var params = [
        { label: "Mean", value: formatNum(mean) },
        { label: "St.Dev.", value: formatNum(pv.calculatedSb) },
        { label: "+3 Std Dev.", value: formatNum(plus3StdDev) },
        { label: "-3 Std Dev.", value: formatNum(minus3StdDev) },
        { label: "Cases", value: pv.countOfValidValues || "" },
        { label: "Center Spec", value: formatNum(centerSpec) },
        { label: "Sigma (σ)", value: formatNum(sigma) },
        { label: "Upper Spec", value: formatNum(specs.currentUpperToleranceLimitAbs) },
        { label: "Lower Spec", value: formatNum(specs.currentLowerToleranceLimitAbs) },
        { label: "Subgroups", value: subgroups.length || "" },
        { label: "Capability Index (Cp)", value: formatNum(pv.cp) },
        { label: "Upper Index (Cpu)", value: formatNum(pv.cpkU) },
        { label: "Lower Index (Cpl)", value: formatNum(pv.cpkL) },
        { label: "Cpk Index", value: formatNum(pv.cpk) },
        { label: "Product above spec", value: formatPercent(pv.probabilityOfValuesLargerThanUpperToleranceInPercent) },
        { label: "Product below spec", value: formatPercent(pv.probabilityOfValuesLessThanLowerToleranceInPercent) },
        { label: "Total beyond spec", value: formatPercent((pv.probabilityOfValuesLargerThanUpperToleranceInPercent || 0) + (pv.probabilityOfValuesLessThanLowerToleranceInPercent || 0)) }
    ];

    var ownerId = ($control && $control.id) || "";
    var $scopeParent = $control.parent();
    $scopeParent.find('.spcHistogramParameters[data-owner="' + ownerId + '"]').remove();

    var $table = $('<table class="spcHistogramParameters" style="width:100%; margin-top:20px; margin-left:20px; margin-bottom:20px; border-collapse:collapse;">');
    if (ownerId) $table.attr("data-owner", ownerId);
    var $tbody = $('<tbody>');

    var midPoint = Math.ceil(params.length / 2);
    for (var i = 0; i < midPoint; i++) {
        var $row = $('<tr>');
        var $leftCell = $('<td style="padding:5px 10px; border:1px solid #ddd; width:50%;">');
        $leftCell.append($('<strong>').text(params[i].label + ": "));
        $leftCell.append($('<span>').text(params[i].value));
        $row.append($leftCell);

        if (i + midPoint < params.length) {
            var $rightCell = $('<td style="padding:5px 10px; border:1px solid #ddd; width:50%;">');
            $rightCell.append($('<strong>').text(params[i + midPoint].label + ": "));
            $rightCell.append($('<span>').text(params[i + midPoint].value));
            $row.append($rightCell);
        } else {
            $row.append($('<td style="padding:5px 10px; border:1px solid #ddd; width:50%;">'));
        }
        $tbody.append($row);
    }

    $table.append($tbody);
    var chartWidth = $control.outerWidth();
    if (chartWidth && chartWidth > 0) $table.css("width", chartWidth + "px");
    $control.after($table);
}