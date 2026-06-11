/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using PERS = Camstar.WebPortal.Personalization;

/// <summary>
/// Summary description for SS_LotPacking
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_LotPacking : MatrixWebPart
    {
        #region Properties

        protected CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("LotPacking_ComputerName") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoEmployeeField { get { return Page.FindCamstarControl("LotPacking_Employee") as CWC.NamedObject; } }
        protected CWC.TextBox _txtSelectionIdField { get { return Page.FindCamstarControl("LotPacking_SelectionId") as CWC.TextBox; } }
        protected CWC.ContainerList _ContainerField { get { return Page.FindCamstarControl("LotPacking_Container") as CWC.ContainerList; } }
        protected CWC.NamedObject _ndoPackingTypeField { get { return Page.FindCamstarControl("LotPacking_PackingType") as CWC.NamedObject; } }
        protected CWC.TextBox _txtPackingSizeField { get { return Page.FindCamstarControl("LotPacking_PackingSize") as CWC.TextBox; } }
        protected CWC.TextBox _txtPartialBoxQtyField { get { return Page.FindCamstarControl("LotPacking_PartialBoxQty") as CWC.TextBox; } }
        protected CWC.TextBox _txtPackingQtyField { get { return Page.FindCamstarControl("LotPacking_PackingQty") as CWC.TextBox; } }
        protected CWC.TextBox _txtNumberOfBoxesField { get { return Page.FindCamstarControl("LotPacking_NumberOfBoxes") as CWC.TextBox; } }
        protected CWC.TextBox _txtPartialBoxNumberField { get { return Page.FindCamstarControl("LotPacking_PartialBoxNumber") as CWC.TextBox; } }
        protected CWC.TextBox _txtTotalLotQtyField { get { return Page.FindCamstarControl("LotPacking_TotalLotQty") as CWC.TextBox; } }
        protected CWC.Button _CalculateButton { get { return Page.FindCamstarControl("LotPacking_CalculateButton") as CWC.Button; } }
        protected CWC.Button _ResetButton { get { return Page.FindCamstarControl("LotPacking_ResetButton") as CWC.Button; } }
        protected CWC.Button _SubmitButton { get { return Page.FindCamstarControl("LotPacking_SubmitButton") as CWC.Button; } }
       
        protected JQDataGrid _gridDetails { get { return Page.FindCamstarControl("LotPacking_Details") as JQDataGrid; } }
        protected JQDataGrid _gridPackingInstructions { get { return Page.FindCamstarControl("LotPacking_PackingInstructions") as JQDataGrid; } }

        // framework control flag
        CWC.CheckBox _chkIsActive { get { return Page.FindCamstarControl("LotPacking_IsActive") as CWC.CheckBox; } }
        protected bool _bIsPopup { get { return Page.IsAJAXFloatingFrame; } }

        #endregion

        #region Page Events

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _txtSelectionIdField.DataChanged += new EventHandler(LotPacking_SelectionId_DataChanged);
                _ndoPackingTypeField.DataChanged += new EventHandler(LotPacking_PackingType_DataChanged);                
                _CalculateButton.Click += new EventHandler(LotPacking_CalculateButton_Click);
                _ResetButton.Click += new EventHandler(LotPacking_ResetButton_Click);           

                if (!Page.IsPostBack)
                {
                    _txtComputerNameField.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);

                    if (_bIsPopup)
                    {
                        _SubmitButton.Visible = false;
                        _SubmitButton.Enabled = false;
                        _ResetButton.Visible = false;
                        _ResetButton.Enabled = false;
                        LotPacking_SelectionId_DataChanged(null, null);
                    }
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
        }

        public void LotPacking_PackingInstructions_ClickCellButton()
        {
            // Here is action on click
            var parts = Page.EventArgument.Split(':');
            var rowIndex = parts[3];

            // check the row of buttob clicked is selected
            string strRowId = (_gridPackingInstructions.GridContext as ItemDataContext).SelectedRowIDs != null ? (_gridPackingInstructions.GridContext as ItemDataContext).SelectedRowIDs.FirstOrDefault(p => p.ToString() == rowIndex.ToString()) : null;
            if (!string.IsNullOrEmpty(strRowId))
            {
                List<CombineLotDetails> CombineLotDetails = new List<CombineLotDetails>((List<CombineLotDetails>)ViewState["CombineLotDetails"]);
                // Get first cell of row and display it 
                int Qty = Convert.ToInt32((_gridPackingInstructions.GridContext as ItemDataContext).GetCell(rowIndex, "Qty").ToString());
                int PrevQty = Qty;

                if (CombineLotDetails != null) 
                {
                    for (int DetailsRowIndex = 0; DetailsRowIndex < (_gridDetails.GridContext as ItemDataContext).GetTotalRows(); DetailsRowIndex++)
                    {
                        string DetailsRow_Container = (_gridDetails.GridContext as ItemDataContext).GetCell(DetailsRowIndex,"FromContainer").ToString();
                        string DetailsRow_RowId = (_gridDetails.GridContext as ItemDataContext).GetRowId(DetailsRowIndex);
                        CombineLotDetails CombineLotDetailsItem = CombineLotDetails.FirstOrDefault(p => p.FromContainer.Name == DetailsRow_Container);
                        if (CombineLotDetailsItem != null)
                        {
                            if ((int)CombineLotDetailsItem.StandbyQty >= Qty) { (_gridDetails.GridContext as ItemDataContext).SetCell(DetailsRow_RowId, "StandbyQty", Qty.ToString()); }
                            else { (_gridDetails.GridContext as ItemDataContext).SetCell(DetailsRow_RowId, "StandbyQty", (int)CombineLotDetailsItem.StandbyQty); }
                            Qty = Qty - ((int)CombineLotDetailsItem.StandbyQty >= Qty? Qty: Convert.ToInt32((_gridDetails.GridContext as ItemDataContext).GetCell(DetailsRowIndex, "StandbyQty").ToString()));

                            if ((int)CombineLotDetailsItem.QtyToProcess >= Qty) { (_gridDetails.GridContext as ItemDataContext).SetCell(DetailsRow_RowId, "QtyToProcess", Qty.ToString()); }
                            else { (_gridDetails.GridContext as ItemDataContext).SetCell(DetailsRow_RowId, "QtyToProcess", (int)CombineLotDetailsItem.QtyToProcess); }
                            Qty = Qty - ((int)CombineLotDetailsItem.QtyToProcess >= Qty ? Qty : Convert.ToInt32((_gridDetails.GridContext as ItemDataContext).GetCell(DetailsRowIndex, "QtyToProcess").ToString()));

                            if ((int)CombineLotDetailsItem.InProcessQty >= Qty) { (_gridDetails.GridContext as ItemDataContext).SetCell(DetailsRow_RowId, "InProcessQty", Qty.ToString()); }
                            else { (_gridDetails.GridContext as ItemDataContext).SetCell(DetailsRow_RowId, "InProcessQty", (int)CombineLotDetailsItem.InProcessQty); }
                            Qty = Qty - ((int)CombineLotDetailsItem.InProcessQty >= Qty ? Qty : Convert.ToInt32((_gridDetails.GridContext as ItemDataContext).GetCell(DetailsRowIndex, "InProcessQty").ToString()));

                            if ((int)CombineLotDetailsItem.ProcessedQty >= Qty) { (_gridDetails.GridContext as ItemDataContext).SetCell(DetailsRow_RowId, "ProcessedQty", Qty.ToString()); }
                            else { (_gridDetails.GridContext as ItemDataContext).SetCell(DetailsRow_RowId, "ProcessedQty", (int)CombineLotDetailsItem.ProcessedQty); }
                            Qty = Qty - ((int)CombineLotDetailsItem.ProcessedQty >= Qty ? Qty : Convert.ToInt32((_gridDetails.GridContext as ItemDataContext).GetCell(DetailsRowIndex, "ProcessedQty").ToString()));
                            _gridDetails.Action_SelectRow(DetailsRow_RowId, (PrevQty != Qty) ? "select" : "deselect");
                            PrevQty = Qty;
                        }
                    }
                    CamstarWebControl.SetRenderToClient(_gridDetails);
                }
                FetchData("PackingInstructionsClick");
            }
        }

        public void LotPacking_PackingType_DataChanged(object sender, EventArgs e)
        {
            FetchData("PackingTypeChanged");
        }

        public void LotPacking_SelectionId_DataChanged(object sender, EventArgs e)
        {            
            _ContainerField.TextEditControl.Text = _txtSelectionIdField.TextControl.Text;

            ClearFields();           
            FetchData("Load");
        }

        public void LotPacking_CalculateButton_Click(object sender, EventArgs e)
        {
            FetchData("CalculateButton");
        }

        public void LotPacking_ResetButton_Click(object sender, EventArgs e)
        {           
            ClearFields();
            FetchData("Load");
        }

        private void ClearFields()
        {
            _gridDetails.Data = null;
            _gridDetails.Action_SelectRow(null, "deselect");
            _gridPackingInstructions.Data = null;
            _gridPackingInstructions.Action_SelectRow(null, "deselect");

            _gridDetails.ClearData();
            _gridPackingInstructions.ClearData();
            ViewState["CombineLotDetails"] = null;

            _ndoPackingTypeField.ClearData();
            ClearPackingTypeDetails();
        }

        private void ClearPackingTypeDetails()
        {
            // clear the packing type details             
            _txtNumberOfBoxesField.ClearData();
            _txtPackingQtyField.ClearData();
            _txtPackingSizeField.ClearData();
            _txtPartialBoxNumberField.ClearData();
            _txtPartialBoxQtyField.ClearData();
            _txtTotalLotQtyField.ClearData();
        }

        public override void GetInputData(Service serviceData)
        {
            bool bExecute = false;
            if (_chkIsActive != null)
                bExecute = _chkIsActive.CheckControl.Checked || _bIsPopup;
            if (bExecute)
            {
                base.GetInputData(serviceData);

                if (serviceData is OM.LotPacking)
                {
                    OM.LotPacking svcData = serviceData as OM.LotPacking;

                    if (_gridDetails.Data != null && _gridDetails.GridContext.SelectedRowIDs != null)
                    {

                        // get the total number of lot selected
                        int intTotalDetails = _gridDetails.GridContext.SelectedRowIDs.Count;
                        // get the list of selected row id
                        string[] strRowIds = _gridDetails.GridContext.SelectedRowIDs.ToArray();
                        int iIndex = -1;

                        svcData.Details = new CombineLotDetails[intTotalDetails];

                        // collect the containers
                        foreach (string strRowId in strRowIds)
                        {
                            iIndex = iIndex + 1;
                            svcData.Details[iIndex] = new CombineLotDetails();
                            svcData.Details[iIndex].ListItemAction = OM.ListItemAction.Add;
                            svcData.Details[iIndex].FromContainer = new ContainerRef();
                            svcData.Details[iIndex].FromContainer.Name = _gridDetails.GridContext.GetCell(strRowId, "FromContainer").ToString();
                            svcData.Details[iIndex].StandbyQty = (Primitive<double>)_gridDetails.GridContext.GetCell(strRowId, "StandbyQty");
                            svcData.Details[iIndex].QtyToProcess = (Primitive<double>)_gridDetails.GridContext.GetCell(strRowId, "QtyToProcess");
                            svcData.Details[iIndex].InProcessQty = (Primitive<double>)_gridDetails.GridContext.GetCell(strRowId, "InProcessQty");
                            svcData.Details[iIndex].ProcessedQty = (Primitive<double>)_gridDetails.GridContext.GetCell(strRowId, "ProcessedQty");
                            svcData.Details[iIndex].TransferRejects = (Primitive<bool>)_gridDetails.GridContext.GetCell(strRowId, "TransferRejects");
                        }
                    }

                    if (_gridPackingInstructions.Data != null && _gridPackingInstructions.GridContext.SelectedRowIDs != null && _gridPackingInstructions.GridContext.SelectedRowIDs.Count > 0)
                    {
                        // get the list of selected row id
                        string[] strRowIds = _gridPackingInstructions.GridContext.SelectedRowIDs.ToArray();

                        svcData.PackingLotId = _gridPackingInstructions.GridContext.GetCell(strRowIds[0], "LotId").ToString();
                    }
                }
            }
        }

        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (serviceData is OM.LotPacking && status.IsSuccess)
            {
                ClearFields();
                FetchData("Load");
            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "PackingInstructionCellAction")
            {
                LotPacking_PackingInstructions_ClickCellButton();
            }
        }

        #endregion

        #region Methods

        public void FetchPackingTypeDetails()
        {
            try
            {
                bool bExecute = false;
                if (_chkIsActive != null)
                    bExecute = _chkIsActive.CheckControl.Checked || _bIsPopup;
                if (bExecute)
                {
                    if (!string.IsNullOrEmpty(_txtSelectionIdField.TextControl.Text) && _ndoPackingTypeField.Data != null)
                    {
                        // get the session and user profile
                        var fs = FrameworkManagerUtil.GetFrameworkSession();

                        // init the service, service data and service info objects
                        LotPackingService objSvc = new LotPackingService(fs.CurrentUserProfile);
                        LotPacking objSvcData = new LotPacking();
                        objSvcData.Container = new ContainerRef(_ContainerField.TextEditControl.Text);
                        objSvcData.PackingType = new NamedObjectRef(_ndoPackingTypeField.TextEditControl.Text);
                       
                        // prepare the service info
                        LotPacking_Info objSvcInfo = new LotPacking_Info();
                        objSvcInfo.PackingSize = FieldInfoUtil.RequestValue();
                        objSvcInfo.NumberOfBoxes = FieldInfoUtil.RequestValue();
                        objSvcInfo.PartialBoxQty = FieldInfoUtil.RequestValue();
                        objSvcInfo.PartialBoxNumber = FieldInfoUtil.RequestValue();
                        objSvcInfo.PackingQty = FieldInfoUtil.RequestValue();
                        objSvcInfo.TotalLotQty = FieldInfoUtil.RequestValue();

                        // init the result object
                        LotPacking_Result objResult = new LotPacking_Result();

                        // execute to request the value
                        ResultStatus resultStatus = objSvc.GetEnvironment(objSvcData, new LotPacking_Request { Info = objSvcInfo }, out objResult);
                        // clear the currentdetails and packing instructions grid

                        ClearPackingTypeDetails();

                        if (resultStatus.IsSuccess)
                        {
                            // display the data
                            if (objResult.Value.PackingSize != null) { _txtPackingSizeField.Data = objResult.Value.PackingSize.ToString(); }
                            if (objResult.Value.NumberOfBoxes != null) { _txtNumberOfBoxesField.Data = objResult.Value.NumberOfBoxes.ToString(); }
                            if (objResult.Value.PartialBoxQty != null) { _txtPartialBoxQtyField.Data = objResult.Value.PartialBoxQty.ToString(); }
                            if (objResult.Value.PartialBoxNumber != null) { _txtPartialBoxNumberField.Data = objResult.Value.PartialBoxNumber.ToString(); }
                            if (objResult.Value.PackingQty != null) { _txtPackingQtyField.Data = objResult.Value.PackingQty.ToString(); }
                            if (objResult.Value.TotalLotQty != null) { _txtTotalLotQtyField.Data = objResult.Value.TotalLotQty.ToString(); }                            
                        }
                        else
                            DisplayMessage(resultStatus);
                    }
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
        }

        public void FetchData(string EventName)
        {
            try
            {
                bool bExecute = false;
                if (_chkIsActive != null)
                    bExecute = _chkIsActive.CheckControl.Checked || _bIsPopup;
                if (bExecute)
                {
                    if (!string.IsNullOrEmpty(_txtSelectionIdField.TextControl.Text))
                    {
                        // get the session and user profile
                        var fs = FrameworkManagerUtil.GetFrameworkSession();

                        // init the service, service data and service info objects
                        LotPackingService objSvc = new LotPackingService(fs.CurrentUserProfile);
                        LotPacking objSvcData = new LotPacking();
                        objSvcData.Container = new ContainerRef(_ContainerField.TextEditControl.Text);
                        objSvcData.PackingType = new NamedObjectRef(_ndoPackingTypeField.TextEditControl.Text);

                        if (EventName != "Load")
                        {
                            // set selected row ID to an array
                            string[] strRowIds = (_gridDetails.GridContext.SelectedRowIDs != null) ? _gridDetails.GridContext.SelectedRowIDs.ToArray() : null;
                            if (strRowIds != null && strRowIds.Length > 0)
                            {
                                int iIndex = 0;
                                objSvcData.Details = new CombineLotDetails[strRowIds.Length];
                                foreach (string strRowId in strRowIds)
                                {
                                    // get the data from details grid
                                    CombineLotDetails objSelectedRow = (_gridDetails.GridContext as ItemDataContext).GetItem(strRowId) as CombineLotDetails;
                                    objSvcData.Details[iIndex] = new CombineLotDetails();
                                    objSvcData.Details[iIndex].FromContainer = new ContainerRef();
                                    objSvcData.Details[iIndex].FromContainer.Name = objSelectedRow.FromContainer.Name;
                                    objSvcData.Details[iIndex].StandbyQty = objSelectedRow.StandbyQty.Value;
                                    objSvcData.Details[iIndex].QtyToProcess = objSelectedRow.QtyToProcess.Value;
                                    objSvcData.Details[iIndex].InProcessQty = objSelectedRow.InProcessQty.Value;
                                    objSvcData.Details[iIndex].ProcessedQty = objSelectedRow.ProcessedQty.Value;
                                    iIndex = iIndex + 1;
                                }
                            }
                        }

                        // prepare the service info
                        LotPacking_Info objSvcInfo = new LotPacking_Info();
                        if (EventName == "Load")
                        {
                            objSvcInfo.Container = FieldInfoUtil.RequestValue();
                            objSvcInfo.PackingType = FieldInfoUtil.RequestValue();
                            objSvcInfo.CurrentDetails = new CombineLotDetails_Info();
                            objSvcInfo.CurrentDetails.FromContainer = FieldInfoUtil.RequestValue();
                            objSvcInfo.CurrentDetails.FromCarrier = FieldInfoUtil.RequestValue();
                            objSvcInfo.CurrentDetails.Spec = FieldInfoUtil.RequestValue();
                            objSvcInfo.CurrentDetails.MainQty = FieldInfoUtil.RequestValue();
                            objSvcInfo.CurrentDetails.StandbyQty = FieldInfoUtil.RequestValue();
                            objSvcInfo.CurrentDetails.QtyToProcess = FieldInfoUtil.RequestValue();
                            objSvcInfo.CurrentDetails.InProcessQty = FieldInfoUtil.RequestValue();
                            objSvcInfo.CurrentDetails.ProcessedQty = FieldInfoUtil.RequestValue();
                            objSvcInfo.CurrentDetails.TransferRejects = FieldInfoUtil.RequestValue();
                            objSvcInfo.PackingInstructions = new SchedulePacking_Info();
                            objSvcInfo.PackingInstructions.LotId = FieldInfoUtil.RequestValue();
                            objSvcInfo.PackingInstructions.Qty = FieldInfoUtil.RequestValue();
                        }
                        objSvcInfo.PackingSize = FieldInfoUtil.RequestValue();
                        objSvcInfo.NumberOfBoxes = FieldInfoUtil.RequestValue();
                        objSvcInfo.PartialBoxQty = FieldInfoUtil.RequestValue();
                        objSvcInfo.PartialBoxNumber = FieldInfoUtil.RequestValue();
                        objSvcInfo.PackingQty = FieldInfoUtil.RequestValue();
                        objSvcInfo.TotalLotQty = FieldInfoUtil.RequestValue();

                        // init the result object
                        LotPacking_Result objResult = new LotPacking_Result();

                        // execute to request the value
                        ResultStatus resultStatus = objSvc.GetEnvironment(objSvcData, new LotPacking_Request { Info = objSvcInfo }, out objResult);
                        // clear the currentdetails and packing instructions grid
                        

                        if (resultStatus.IsSuccess)
                        {
                            // display the data
                            if (objResult.Value.Container != null) { _ContainerField.Data = objResult.Value.Container; }

                            // display the details
                            if (objResult.Value.PackingType != null) 
                                { _ndoPackingTypeField.Data = objResult.Value.PackingType; }

                            if (objResult.Value.CurrentDetails != null)
                            {
                                bool boolCarriersExist = false;
                                List<CombineLotDetails> objDetails = new List<CombineLotDetails>();
                                List<CombineLotDetails> objViewStateDetails = new List<CombineLotDetails>();
                                foreach (CombineLotDetails objData in objResult.Value.CurrentDetails)
                                {
                                    CombineLotDetails objNewRow = new CombineLotDetails();
                                    if (objData.FromContainer != null) { objNewRow.FromContainer = new ContainerRef { Name = objData.FromContainer.Name }; }
                                    if (objData.FromCarrier != null && !string.IsNullOrEmpty(objData.FromCarrier.Name)) { boolCarriersExist = true; objNewRow.FromCarrier = new NamedObjectRef { Name = objData.FromCarrier.Name }; }
                                    objNewRow.Spec = new RevisionedObjectRef { Name = objData.Spec.Name };
                                    objNewRow.MainQty = objData.MainQty;
                                    objNewRow.StandbyQty = objData.StandbyQty;
                                    objNewRow.QtyToProcess = objData.QtyToProcess;
                                    objNewRow.InProcessQty = objData.InProcessQty;
                                    objNewRow.ProcessedQty = objData.ProcessedQty;
                                    objNewRow.TransferRejects = objData.TransferRejects;
                                    objDetails.Add(objNewRow);

                                    CombineLotDetails objViewStateNewRow = new CombineLotDetails();
                                    if (objData.FromContainer != null) { objViewStateNewRow.FromContainer = new ContainerRef { Name = objData.FromContainer.Name }; }
                                    if (objData.FromCarrier != null && !string.IsNullOrEmpty(objData.FromCarrier.Name)) { objViewStateNewRow.FromCarrier = new NamedObjectRef { Name = objData.FromCarrier.Name }; }
                                    objViewStateNewRow.Spec = new RevisionedObjectRef { Name = objData.Spec.Name };
                                    objViewStateNewRow.MainQty = objData.MainQty;
                                    objViewStateNewRow.StandbyQty = objData.StandbyQty;
                                    objViewStateNewRow.QtyToProcess = objData.QtyToProcess;
                                    objViewStateNewRow.InProcessQty = objData.InProcessQty;
                                    objViewStateNewRow.ProcessedQty = objData.ProcessedQty;
                                    objViewStateNewRow.TransferRejects = objData.TransferRejects;
                                    objViewStateDetails.Add(objViewStateNewRow);
                                }
                                if (objDetails.Count > 0)
                                {
                                    // prepare the ViewState["CombineLotDetails"] to keep the original value of the CurrentDetails
                                    ViewState["CombineLotDetails"] = new List<CombineLotDetails>(objViewStateDetails);

                                    //assign to the CurrentDetails to the grid
                                    _gridDetails.Data = objDetails.ToArray();
                                    _gridDetails.BoundContext.Fields["FromCarrier"].Visible = !(boolCarriersExist == false);

                                    //Interactive Sorting
                                    foreach (JQField Field in _gridDetails.BoundContext.Fields)
                                    {
                                        Field.Sortable = true;
                                    }

                                    if (objResult.Value.PackingInstructions == null || (objResult.Value.PackingInstructions != null && objResult.Value.PackingInstructions.Length == 0))
                                    {
                                        for (int RowIndex = 0; RowIndex < _gridDetails.GridContext.GetTotalRows(); RowIndex++)
                                        {
                                            _gridDetails.Action_SelectRow(RowIndex.ToString().PadLeft(6, '0'), "select");
                                        }
                                    }
                                }
                            }
                            
                            if (objResult.Value.PackingInstructions != null)
                            {
                                List<SchedulePacking> objPackingInstructions = new List<SchedulePacking>();
                                foreach (SchedulePacking objData in objResult.Value.PackingInstructions)
                                {
                                    SchedulePacking objNewRow = new SchedulePacking();
                                    objNewRow.LotId = objData.LotId;
                                    objNewRow.Qty = objData.Qty;
                                    objPackingInstructions.Add(objNewRow);
                                }
                                if (objPackingInstructions.Count > 0) 
                                { 
                                    _gridPackingInstructions.Data = objPackingInstructions.ToArray();
                                    
                                    // Interactive Sorting
                                    foreach (JQField Field in _gridPackingInstructions.BoundContext.Fields)
                                    {
                                        Field.Sortable = true;
                                    }
                                }
                            }                            

                            if (objResult.Value.PackingSize != null) { _txtPackingSizeField.Data = objResult.Value.PackingSize.ToString(); }
                            if (objResult.Value.NumberOfBoxes != null) { _txtNumberOfBoxesField.Data = objResult.Value.NumberOfBoxes.ToString(); }
                            if (objResult.Value.PartialBoxQty != null) { _txtPartialBoxQtyField.Data = objResult.Value.PartialBoxQty.ToString(); }
                            if (objResult.Value.PartialBoxNumber != null) { _txtPartialBoxNumberField.Data = objResult.Value.PartialBoxNumber.ToString(); }
                            if (objResult.Value.PackingQty != null) { _txtPackingQtyField.Data = objResult.Value.PackingQty.ToString(); }
                            if (objResult.Value.TotalLotQty != null) { _txtTotalLotQtyField.Data = objResult.Value.TotalLotQty.ToString(); }
                            CamstarWebControl.SetRenderToClient(_gridDetails);
                            CamstarWebControl.SetRenderToClient(_gridPackingInstructions);
                        }
                        else
                            DisplayMessage(resultStatus);
                    }
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
        }

        #endregion

    }
}



