/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Reflection;
using System.Reflection.Emit;
using System.ComponentModel;

using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using PERS = Camstar.WebPortal.Personalization;
using Camstar.WebPortal.Personalization;
using CamstarPortal.WebControls;
using Camstar.WCF.Services;
using System.Collections;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Constants;
using System.Text.RegularExpressions;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    /// Summary description for CarrierAssignSlotMapTxn
    /// </summary>
    public class CarrierAssignSlotMapTxn : scsShopfloorBase
    {
        CWC.TextBox _txtSelectionIdField { get { return Page.FindCamstarControl("SelectionId") as CWC.TextBox; } }
        CWC.NamedObject _ndoCarrierField { get { return Page.FindCamstarControl("Carrier") as CWC.NamedObject; } }
        JQDataGrid _gridLotInfoField { get { return Page.FindCamstarControl("LotInfoFieldGrid") as JQDataGrid; } }
        JQDataGrid _gridWafersFieldhdn { get { return Page.FindCamstarControl("WafersFieldGrid") as JQDataGrid; } }
        JQDataGrid _gridWafersField { get { return Page.FindCamstarControl("WafersFieldGrid2") as JQDataGrid; } }
        JQDataGrid _gridSlotMapsDetailsField { get { return Page.FindCamstarControl("SlotMapDetailsGrid") as JQDataGrid; } }
        CWC.ContainerList _ContainerField { get { return Page.FindCamstarControl("HiddenSelectedContainer") as CWC.ContainerList; } }
        CWC.DropDownList _ddlSlotNumberField { get { return Page.FindCamstarControl("SlotNumber") as CWC.DropDownList; } }
        SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }
        CWC.Button _addButton { get { return Page.FindCamstarControl("AddButton") as CWC.Button; } }
        CWC.Button _addAllButton { get { return Page.FindCamstarControl("AddAllButton") as CWC.Button; } }
        CWC.Button _removeButton { get { return Page.FindCamstarControl("RemoveButton") as CWC.Button; } }
        CWC.Button _removeAllButton { get { return Page.FindCamstarControl("RemoveAllButton") as CWC.Button; } }
        CWC.CheckBox _chkboxIsLotAssignedSlotMap { get { return Page.FindCamstarControl("IsLotAssignedSlotMap") as CWC.CheckBox; } }

        const string const_sSelectionId = "SELECTIONID";
        const string const_sCarrier = "CARRIER";

        public CarrierAssignSlotMapTxn()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_gridWafersFieldhdn != null)
                _gridWafersFieldhdn.Visible = false;

            _addButton.Click += new EventHandler(AddButton_Click);
            _addAllButton.Click += new EventHandler(AddAllButton_Click);
            _removeButton.Click += new EventHandler(RemoveButton_Click);
            _removeAllButton.Click += new EventHandler(RemoveAllButton_Click);
            if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                OnPopupClose();

            if (Page.EventTarget == "ctl00$WebPartManager$WafersButtonsPanelWP$WafersFieldGrid2" && !string.IsNullOrEmpty(_gridWafersField.GridContext.SelectedRowID))
                singleselectWafer();

            var lotInfoFieldGrid = (Page.FindCamstarControl("LotInfoFieldGrid")) as JQDataGrid;
            var wafersFieldGrid = (Page.FindCamstarControl("WafersFieldGrid")) as JQDataGrid;
            var slotMapDetailsGrid = (Page.FindCamstarControl("SlotMapDetailsGrid")) as JQDataGrid;

            if (IsResponsive)
            {
                if (lotInfoFieldGrid.Settings.Automation == null)
                    lotInfoFieldGrid.Settings.Automation = new GridAutomation();

                lotInfoFieldGrid.Settings.Automation.ShrinkColumnWidthToFit = false;

                if (wafersFieldGrid.Settings.Automation == null)
                    wafersFieldGrid.Settings.Automation = new GridAutomation();

                wafersFieldGrid.Settings.Automation.ShrinkColumnWidthToFit = false;

                if (slotMapDetailsGrid.Settings.Automation == null)
                    slotMapDetailsGrid.Settings.Automation = new GridAutomation();

                slotMapDetailsGrid.Settings.Automation.ShrinkColumnWidthToFit = false;
            }
        }

        private void singleselectWafer()
        {
            //_gridWafersField.GridContext.SelectedRowIDs[0] = _gridWafersField.GridContext.SelectedRowID;
            _gridWafersField.GridContext.SelectedRowIDs.RemoveRange(0, _gridWafersField.GridContext.SelectedRowIDs.Count);
            _gridWafersField.GridContext.SelectedRowIDs.Add(_gridWafersField.GridContext.SelectedRowID);
            CamstarWebControl.SetRenderToClient(_gridWafersField);
        }
        private void ResetFields()
        {
            _gridLotInfoField.ClearData();
            _gridWafersField.ClearData();
        }

        private void SetLotSelection(string sSelectionId)
        {
            JQDataGrid _gridLotInfoFieldx = Page.FindCamstarControl("LotInfoFieldGrid") as JQDataGrid;
            //_gridLotInfoField.ClearData();
            SEMI.AppCode.UIUtility.GetLotQuerySelection(this, this.PrimaryServiceType.ToString(), sSelectionId, false, ref _gridLotInfoFieldx, "LotInfoFieldGrid", false, new string[] { "__STYLE" });

        }

        public void AddButton_Click(object sender, EventArgs e)
        {
            try
            {

                if (_gridSlotMapsDetailsField.GridContext.GetTotalRows() != 0 && _gridWafersField.GridContext.GetTotalRows() != 0 && _chkboxIsLotAssignedSlotMap.CheckControl.Checked == false)
                {
                    if (_ddlSlotNumberField.Data != null)
                    {
                        if (!string.IsNullOrEmpty(_gridWafersField.GridContext.SelectedRowID))
                        {
                            string selectedRowId = _gridWafersField.GridContext.SelectedRowID;
                            LotWafers selectedRow = (_gridWafersField.GridContext as ItemDataContext).GetItem(selectedRowId) as LotWafers;
                            if (selectedRow != null)
                            {
                                for (int i = 0; i < _gridSlotMapsDetailsField.GridContext.GetTotalRows(); i++)
                                {
                                    string strSlotMapsDetailsRowID = i.ToString().PadLeft(6, '0');
                                    if (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapsDetailsRowID, "SlotNumber").ToString() == _ddlSlotNumberField.Data.ToString())
                                    {
                                        if (selectedRow.Grade != "Add")
                                        {
                                            if (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapsDetailsRowID, "Status").ToString() == "UP" && (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapsDetailsRowID, "WaferNumber") == null || _gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapsDetailsRowID, "WaferNumber").ToString() == ""))
                                            {
                                                _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapsDetailsRowID, "WaferNumber", selectedRow.WaferNumber.Value);
                                                _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapsDetailsRowID, "WaferScribeNumber", selectedRow.WaferScribeNumber.Value);
                                                _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapsDetailsRowID, "Lot", selectedRow.Container.Name);
                                                _gridWafersField.GridContext.SetCell(selectedRowId, "Grade", "Add");
                                                CamstarWebControl.SetRenderToClient(_gridWafersField);
                                                CamstarWebControl.SetRenderToClient(_gridSlotMapsDetailsField);
                                            }
                                            else
                                            {
                                                throw new Exception("The slot number is either occupied or the status is down.");
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("The wafer is already added before.");
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }

        public void AddAllButton_Click(object sender, EventArgs e)
        {
            try
            {
                List<String> previousAddedRow = new List<string>();
                bool rowAdded = false;

                if (_gridSlotMapsDetailsField.GridContext.GetTotalRows() != 0 && _gridWafersField.GridContext.GetTotalRows() != 0 && _chkboxIsLotAssignedSlotMap.CheckControl.Checked == false)
                {

                    for (int j = 0; j < _gridWafersField.GridContext.GetTotalRows(); j++)
                    {
                        string strGridWaferRowID = j.ToString().PadLeft(6, '0');
                        rowAdded = false;
                        for (int i = 0; i < _gridSlotMapsDetailsField.GridContext.GetTotalRows(); i++)
                        {
                            // string strSlotMapDetailsRowID = _gridSlotMapsDetailsField.GridContext.GetRowId(i);
                            string strSlotMapDetailsRowID = i.ToString().PadLeft(6, '0');
                            if (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapDetailsRowID, "Status").ToString() == "UP" && (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapDetailsRowID, "WaferNumber") == null || _gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapDetailsRowID, "WaferNumber").ToString() == ""))
                            {
                                if (_gridWafersField.GridContext.GetCell(strGridWaferRowID, "Grade").ToString() != "Add")
                                {
                                    if (!previousAddedRow.Contains(strSlotMapDetailsRowID))
                                    {
                                        Regex regex = new Regex(@"\(([^\}]+)\)");

                                        //string inputLot = _gridWafersField.GridContext.GetCell(strGridWaferRowID, "Container").ToString().Replace("(LOT)", "");
                                        string inputLot = _gridWafersField.GridContext.GetCell(strGridWaferRowID, "Container").ToString();
                                        inputLot = regex.Replace(inputLot, "");
                                        _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapDetailsRowID, "WaferNumber", _gridWafersField.GridContext.GetCell(strGridWaferRowID, "WaferNumber").ToString());
                                        _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapDetailsRowID, "WaferScribeNumber", _gridWafersField.GridContext.GetCell(strGridWaferRowID, "WaferScribeNumber").ToString());
                                        _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapDetailsRowID, "Lot", inputLot);
                                        _gridWafersField.GridContext.SetCell(strGridWaferRowID, "Grade", "Add");
                                        CamstarWebControl.SetRenderToClient(_gridWafersField);
                                        CamstarWebControl.SetRenderToClient(_gridSlotMapsDetailsField);
                                        previousAddedRow.Add(strSlotMapDetailsRowID);
                                        rowAdded = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    rowAdded = true;
                                }
                            }
                        }
                    }

                    if (!rowAdded)
                    {
                        throw new Exception("The slot map has not enough slot for all wafers, either none or partial wafers are assigned.");
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }

        public void RemoveButton_Click(object sender, EventArgs e)
        {
            if (_gridSlotMapsDetailsField.GridContext.GetTotalRows() != 0 && _gridWafersField.GridContext.GetTotalRows() != 0 && _chkboxIsLotAssignedSlotMap.CheckControl.Checked == false)
            {
                if (_ddlSlotNumberField.Data != null)
                {
                    for (int i = 0; i < _gridSlotMapsDetailsField.GridContext.GetTotalRows(); i++)
                    {
                        string strSlotMapsDetailsRowID = i.ToString().PadLeft(6, '0');
                        if (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapsDetailsRowID, "SlotNumber").ToString() == _ddlSlotNumberField.Data.ToString())
                        {
                            // string strSlotMapDetailsRowID = _gridSlotMapsDetailsField.GridContext.GetRowId(i);
                            for (int j = 0; j < _gridWafersField.GridContext.GetTotalRows(); j++)
                            {
                                string strGridWaferRowID = j.ToString().PadLeft(6, '0');
                                if (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapsDetailsRowID, "WaferScribeNumber") != null && (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapsDetailsRowID, "WaferScribeNumber").ToString() == _gridWafersField.GridContext.GetCell(strGridWaferRowID, "WaferScribeNumber").ToString()))
                                {

                                    _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapsDetailsRowID, "WaferNumber", "");
                                    _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapsDetailsRowID, "WaferScribeNumber", "");
                                    _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapsDetailsRowID, "Lot", "");
                                    _gridWafersField.GridContext.SetCell(strGridWaferRowID, "Grade", "");
                                    CamstarWebControl.SetRenderToClient(_gridWafersField);
                                    CamstarWebControl.SetRenderToClient(_gridSlotMapsDetailsField);
                                    break;
                                }
                            }
                        }
                    }

                }
            }
        }

        public void RemoveAllButton_Click(object sender, EventArgs e)
        {
            if (_gridSlotMapsDetailsField.GridContext.GetTotalRows() != 0 && _gridWafersField.GridContext.GetTotalRows() != 0 && _chkboxIsLotAssignedSlotMap.CheckControl.Checked == false)
            {

                for (int i = 0; i < _gridSlotMapsDetailsField.GridContext.GetTotalRows(); i++)
                {

                    string strSlotMapDetailsRowID = i.ToString().PadLeft(6, '0');
                    for (int j = 0; j < _gridWafersField.GridContext.GetTotalRows(); j++)
                    {
                        string strGridWaferRowID = j.ToString().PadLeft(6, '0');
                        if (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapDetailsRowID, "WaferScribeNumber") != null && (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapDetailsRowID, "WaferScribeNumber").ToString() == _gridWafersField.GridContext.GetCell(strGridWaferRowID, "WaferScribeNumber").ToString()))
                        {
                            _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapDetailsRowID, "WaferNumber", "");
                            _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapDetailsRowID, "WaferScribeNumber", "");
                            _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapDetailsRowID, "Lot", "");
                            _gridWafersField.GridContext.SetCell(strGridWaferRowID, "Grade", "");
                            CamstarWebControl.SetRenderToClient(_gridWafersField);
                            CamstarWebControl.SetRenderToClient(_gridSlotMapsDetailsField);
                            break;
                        }
                    }
                }

            }
        }

        private void FetchData(string sEventName)
        {
            try
            {
                Page.StatusBar.ClearMessage();

                // Get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                string sServiceType = this.PrimaryServiceType;

                // Run proper constructor and prepare 
                var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
                var svcConstructor = svcType.GetConstructor(new Type[] { typeof(Camstar.WCF.ObjectStack.UserProfile) });
                var svc = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

                // Service Data
                var data = CreateServiceData(sServiceType);
                var oServiceData = data as CarrierAssignSlotMap;

                CreateInsertionDetails_Info oInsertionDetailsInfo = new CreateInsertionDetails_Info();
                CreateInsertionDetails[] oInsertionDetailsData = null;

                // Service Info
                var info = CreateServiceInfo(sServiceType);
                var oServiceInfo = info as CarrierAssignSlotMap_Info;

                string sSelectionId = _txtSelectionIdField.Data != null ? _txtSelectionIdField.Data.ToString() : "";

                // Prepare the request 
                if (sEventName == const_sSelectionId)
                {
                    oServiceData.SelectionId = sSelectionId;
                    oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
                    oServiceInfo.AssignedCarrier = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotWafers = new LotWafers_Info();
                    oServiceInfo.LotWafers.WaferSequence = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotWafers.WaferNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotWafers.WaferScribeNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotWafers.Container = FieldInfoUtil.RequestValue();
                    oServiceInfo.ss_IsLotAssignedSlotMap = FieldInfoUtil.RequestValue();
                }
                else if (sEventName == const_sCarrier)
                {
                    oServiceData.Carrier = new NamedObjectRef();
                    oServiceData.Carrier = _ndoCarrierField.Data as NamedObjectRef;
                    oServiceInfo.SlotMapSelection = new SlotMapDetails_Info();
                    oServiceInfo.SlotMapSelection.SlotNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.SlotMapSelection.WaferNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.SlotMapSelection.WaferScribeNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.SlotMapSelection.Lot = FieldInfoUtil.RequestValue();
                    oServiceInfo.SlotMapSelection.Status = FieldInfoUtil.RequestValue();
                }


                // Prepare Request
                var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
                (oRequest as Request).Info = oServiceInfo;

                Result oResult;

                // Request the data
                ResultStatus oResultStatus = (svc as IShopFloorBase).ResolveSelectionId(oServiceData as DCObject, oRequest as Request, out oResult);

                if (oResultStatus.IsSuccess)
                {
                    // Set the result object
                    var oResponseData = oResult.Value as CarrierAssignSlotMap;

                    if (oResponseData.ss_IsLotAssignedSlotMap != null)
                        _chkboxIsLotAssignedSlotMap.CheckControl.Checked = oResponseData.ss_IsLotAssignedSlotMap.Value;

                    // Display information for resolve selection id
                    if (sEventName == const_sSelectionId)
                    {
                        // Set the value of the resolve Container Name and other information
                        _ContainerField.Data = oResponseData.SelectionContainer.Name.ToString();

                        // Fetch Lot Information and Display on datagrid
                        SetLotSelection(_ContainerField.Data.ToString());

                        if (oResponseData.LotWafers != null)
                        {
                            foreach (LotWafers wafersList in oResponseData.LotWafers)
                            {
                                string waferGrade = "";
                                //int iNewRowCount = _gridWafersField.BoundContext.GetTotalRows();
                                //(_gridWafersField.GridContext as ItemDataContext).MakeAutoRowId(iNewRowCount);
                                //string id = (_gridWafersField.GridContext as ItemDataContext).AddNewRow(iNewRowCount.ToString());
                                //object waferDetails = ((_gridWafersField.GridContext as ItemDataContext).Data as Array).GetValue(iNewRowCount);
                                //(waferDetails as LotWafers).WaferSequence = wafersList.WaferSequence;
                                //(waferDetails as LotWafers).WaferNumber = wafersList.WaferNumber;
                                //(waferDetails as LotWafers).WaferScribeNumber = wafersList.WaferScribeNumber;
                                //(waferDetails as LotWafers).Container = wafersList.Container;
                                if (oResponseData.AssignedCarrier != null)
                                {
                                    //(waferDetails as LotWafers).Grade = "Add";
                                    waferGrade = "Add";
                                }
                                WafersGrid_AddNewRow((int)wafersList.WaferSequence, (string)wafersList.WaferNumber, (string)wafersList.WaferScribeNumber, wafersList.Container, waferGrade);

                                //_gridWafersField.GridContext.AdjustCurrentPage(id);
                                //CamstarWebControl.SetRenderToClient(_gridWafersField);
                            }
                        }

                    }
                    else if (sEventName == const_sCarrier)
                    {
                        if (oResponseData.SlotMapSelection != null)
                        {
                            ArrayList slotNo = new ArrayList();

                            foreach (SlotMapDetails slotmapList in oResponseData.SlotMapSelection)
                            {
                                //int iNewRowCount = _gridSlotMapsDetailsField.BoundContext.GetTotalRows();
                                //(_gridSlotMapsDetailsField.GridContext as ItemDataContext).MakeAutoRowId(iNewRowCount);
                                //string id = (_gridSlotMapsDetailsField.GridContext as ItemDataContext).AddNewRow(iNewRowCount.ToString());
                                //object slotmapDetails = ((_gridSlotMapsDetailsField.GridContext as ItemDataContext).Data as Array).GetValue(iNewRowCount);
                                //(slotmapDetails as SlotMapDetails).SlotNumber = slotmapList.SlotNumber;
                                //(slotmapDetails as SlotMapDetails).WaferNumber = slotmapList.WaferNumber;
                                //(slotmapDetails as SlotMapDetails).WaferScribeNumber = slotmapList.WaferScribeNumber;
                                //(slotmapDetails as SlotMapDetails).Lot = slotmapList.Lot;
                                //(slotmapDetails as SlotMapDetails).Status = slotmapList.Status;

                                SlotMapGrid_AddNewRow((string)slotmapList.SlotNumber, (string)slotmapList.WaferNumber, (string)slotmapList.WaferScribeNumber, slotmapList.Lot, (string)slotmapList.Status);

                                slotNo.Add(Convert.ToString(slotmapList.SlotNumber));
                                //_gridSlotMapsDetailsField.GridContext.AdjustCurrentPage(id);
                                //CamstarWebControl.SetRenderToClient(_gridSlotMapsDetailsField);
                            }

                            AddData(slotNo);

                        }
                    }
                }
                else
                {
                    DisplayMessage(oResultStatus);
                    //ResetFields();
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }

        public void SelectionIdField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                string sContainer = _txtSelectionIdField.Data.ToString();
                string strSelectedGridId = (_gridLotInfoField.GridContext as BoundContext).GetRowIdByCellValue("Lot", sContainer);

                if (string.IsNullOrEmpty(strSelectedGridId))
                    FetchData(const_sSelectionId);

                _txtSelectionIdField.TextControl.Text = "";
                _txtSelectionIdField.Focus();

                if (_chkboxIsLotAssignedSlotMap.CheckControl.Checked == true)
                    throw new Exception("A virtual Slop Map was already assigned with Lot Assign Experiment. Carrier slot map assigmnet is not allowed.");
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.Message, false));
            }
        }

        public void LotInfoFieldGrid_RowSelected(object sender, EventArgs e)
        {
            try
            {
                _gridWafersField.ClearData();
                // Get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                string sServiceType = this.PrimaryServiceType;

                // Run proper constructor and prepare 
                var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
                var svcConstructor = svcType.GetConstructor(new Type[] { typeof(Camstar.WCF.ObjectStack.UserProfile) });
                var svc = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

                // Service Data
                var data = CreateServiceData(sServiceType);
                var oServiceData = data as CarrierAssignSlotMap;

                CreateInsertionDetails_Info oInsertionDetailsInfo = new CreateInsertionDetails_Info();

                // Service Info
                var info = CreateServiceInfo(sServiceType);
                var oServiceInfo = info as CarrierAssignSlotMap_Info;

                string sRowID = _gridLotInfoField.SelectedRowID;
                string sSelectionId = _gridLotInfoField.GridContext.GetCell(sRowID, "Lot").ToString();

                // Prepare the request 
                oServiceData.SelectionId = sSelectionId;
                oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
                oServiceInfo.AssignedCarrier = FieldInfoUtil.RequestValue();
                oServiceInfo.LotWafers = new LotWafers_Info();
                oServiceInfo.LotWafers.WaferSequence = FieldInfoUtil.RequestValue();
                oServiceInfo.LotWafers.WaferNumber = FieldInfoUtil.RequestValue();
                oServiceInfo.LotWafers.WaferScribeNumber = FieldInfoUtil.RequestValue();
                oServiceInfo.LotWafers.Container = FieldInfoUtil.RequestValue();

                // Prepare Request
                var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
                (oRequest as Request).Info = oServiceInfo;

                Result oResult;

                // Request the data
                ResultStatus oResultStatus = (svc as IShopFloorBase).ResolveSelectionId(oServiceData as DCObject, oRequest as Request, out oResult);

                if (oResultStatus.IsSuccess)
                {
                    // Set the result object
                    var oResponseData = oResult.Value as CarrierAssignSlotMap;

                    // Set the value of the resolve Container Name and other information
                    _ContainerField.Data = oResponseData.SelectionContainer.Name.ToString();

                    if (oResponseData.LotWafers != null)
                    {
                        foreach (LotWafers wafersList in oResponseData.LotWafers)
                        {
                            string waferGrade = "";
                            if (oResponseData.AssignedCarrier != null)
                            {
                                //(waferDetails as LotWafers).Grade = "Add";
                                waferGrade = "Add";
                            }

                            WafersGrid_AddNewRow((int)wafersList.WaferSequence, (string)wafersList.WaferNumber, (string)wafersList.WaferScribeNumber, wafersList.Container, waferGrade);

                        }
                    }
                }
            }

            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }

        public void LotInfoFieldGrid_RowDelete(object sender, EventArgs e)
        {
            // get the container that was removed
            string sContainer = _ContainerField.Data.ToString();

            if (sContainer != "")
            {
                _gridWafersField.ClearData();
                _ContainerField.ClearData();
            }
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            int totalRow = _gridLotInfoField.TotalRowCount;

            List<ContainerRef> Containers = new List<ContainerRef>();
            for (int i = 0; i < totalRow; i++)
            {
                string sSelectionId = _gridLotInfoField.GridContext.GetCell(i, "Lot").ToString();
                Containers.Add(new ContainerRef(sSelectionId));
            }
            (serviceData as CarrierAssignSlotMap).Containers = Containers.ToArray();


        }

        public void Carrier_DataChanged(object sender, EventArgs e)
        {
            if (!_ndoCarrierField.IsEmpty)
            {
                _gridSlotMapsDetailsField.ClearData();
                _ddlSlotNumberField.ClearData();
                _ddlSlotNumberField.PickListPanelControl.ClearSelectionValues();
                FetchData(const_sCarrier);
            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            if (action != null && action.Parameters == "Clear")
            {
                Page.ClearValues();
                ResetFields();
                _gridSlotMapsDetailsField.ClearData();
                _ddlSlotNumberField.ClearData();
                _ddlSlotNumberField.PickListPanelControl.ClearSelectionValues();
            }
        }

        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                Page.ShopfloorReset(null, null);
                ResetFields();
            }
        }

        public void AddData(ArrayList data)
        {
            string[] slotNo = data.ToArray(typeof(string)) as string[];
            RecordSet rsNamedObject = new RecordSet();
            OM.Header[] rsHeaders = new OM.Header[2];
            Row[] rsRows = new Row[slotNo.Length];

            rsHeaders[0] = new OM.Header();
            rsHeaders[0].TypeCode = TypeCode.String;
            rsHeaders[0].Name = "Name";

            rsHeaders[1] = new OM.Header();
            rsHeaders[1].TypeCode = TypeCode.String;
            rsHeaders[1].Name = "Value";

            rsNamedObject.Headers = rsHeaders;

            for (int x = 0; x < slotNo.Length; x++)
            {
                rsRows[x] = new Row();
                string[] strRowValues = new string[2];
                strRowValues[0] = slotNo[x];
                strRowValues[1] = slotNo[x];
                rsRows[x].Values = strRowValues;
            }

            rsNamedObject.Rows = rsRows;

            _ddlSlotNumberField.PickListPanelControl.SetSelectionValues(rsNamedObject);
        }

        public void WafersGrid_AddNewRow(int WaferSequence, string WaferNumber, string WaferScribeNumber, ContainerRef Container, string Grade)
        {
            try
            {
                JQDataGrid _gridDetails = _gridWafersField;
                LotWafers[] oNewDetail = new LotWafers[1];
                oNewDetail[0] = new LotWafers();
                oNewDetail[0].WaferSequence = WaferSequence;
                oNewDetail[0].WaferNumber = WaferNumber;
                oNewDetail[0].WaferScribeNumber = WaferScribeNumber;
                oNewDetail[0].Container = Container;
                oNewDetail[0].Grade = Grade;
                LotWafers[] oExisting = (_gridDetails.GridContext as BoundContext).Data as LotWafers[];
                if (oExisting != null)
                {
                    bool isUnique = true;
                    for (int i = 0; i < oExisting.Length; i++)
                    {
                        if (oExisting[i].WaferSequence.Equals(oNewDetail[0].WaferSequence))
                        {
                            isUnique = false;
                        }
                    }
                    if (isUnique)
                    {
                        LotWafers[] oMerged = new LotWafers[oExisting.Length + 1];
                        Array.Copy(oExisting, oMerged, oExisting.Length);
                        Array.Copy(oNewDetail, 0, oMerged, oExisting.Length, 1);
                        (_gridDetails.GridContext as BoundContext).Data = oMerged.ToArray();
                    }
                }
                else
                {
                    (_gridDetails.GridContext as BoundContext).Data = oNewDetail.ToArray();
                }
                _gridDetails.BoundContext.LoadData();
                CamstarWebControl.SetRenderToClient(_gridDetails);
            }
            catch (Exception ex)
            { }
        }

        public void SlotMapGrid_AddNewRow(string SlotNumber, string WaferNumber, string WaferScribeNumber, ContainerRef Lot, string Status)
        {
            try
            {
                JQDataGrid _gridDetails = _gridSlotMapsDetailsField;
                SlotMapDetails[] oNewDetail = new SlotMapDetails[1];
                oNewDetail[0] = new SlotMapDetails();
                oNewDetail[0].SlotNumber = SlotNumber;
                oNewDetail[0].WaferNumber = WaferNumber;
                oNewDetail[0].WaferScribeNumber = WaferScribeNumber;
                oNewDetail[0].Lot = Lot;
                oNewDetail[0].Status = Status;
                SlotMapDetails[] oExisting = (_gridDetails.GridContext as BoundContext).Data as SlotMapDetails[];
                if (oExisting != null)
                {
                    bool isUnique = true;
                    for (int i = 0; i < oExisting.Length; i++)
                    {
                        if (oExisting[i].SlotNumber.Equals(oNewDetail[0].SlotNumber))
                        {
                            isUnique = false;
                        }
                    }
                    if (isUnique)
                    {
                        SlotMapDetails[] oMerged = new SlotMapDetails[oExisting.Length + 1];
                        Array.Copy(oExisting, oMerged, oExisting.Length);
                        Array.Copy(oNewDetail, 0, oMerged, oExisting.Length, 1);
                        (_gridDetails.GridContext as BoundContext).Data = oMerged.ToArray();
                    }
                }
                else
                {
                    (_gridDetails.GridContext as BoundContext).Data = oNewDetail.ToArray();
                }
                _gridDetails.BoundContext.LoadData();
                CamstarWebControl.SetRenderToClient(_gridDetails);
            }
            catch (Exception ex)
            { }
        }

        //-----------------------------------------
        // On Popup closed event
        //-----------------------------------------
        public void OnPopupClose()
        {
            try
            {
                if (Page.DataContract.GetValueByName("SelectedLotsListDM") != null)
                {
                    string[] sContainers = Page.DataContract.GetValueByName("SelectedLotsListDM") as string[];
                    _envSelectedLots.SS_ContainersList = null;
                    Page.DataContract.SetValueByName("SelectedLotsListDM", null);
                    foreach (string container in sContainers)
                        _txtSelectionIdField.Data = container;
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }
    }
}