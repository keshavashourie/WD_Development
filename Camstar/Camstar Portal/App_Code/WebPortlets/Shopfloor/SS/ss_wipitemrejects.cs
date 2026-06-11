/* Copyright 2025 Siemens */
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
using CamstarPortal.WebControls;
using Camstar.WCF.Services;
using System.Collections;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Constants;


namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    /// Summary description for WIP Item Rejects
    /// </summary>
    public class WIPItemRejects : MatrixWebPart
    {
        #region Properties 
        CWC.CheckBox _chkIsActiveField { get { return Page.FindCamstarControl("WIPItemRejects_IsActive") as CWC.CheckBox; } }
        CWC.CheckBox _chkIsPopupField { get { return Page.FindCamstarControl("WIPItemRejects_IsPopup") as CWC.CheckBox; } }
        CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("WIPItemRejects_ComputerName") as CWC.TextBox; } }
        CWC.TextBox _txtMaxRejectQtyField { get { return Page.FindCamstarControl("WIPItemRejects_MaxRejectQty") as CWC.TextBox; } }
        JQDataGrid _gridCurrentDetailsField { get { return Page.FindCamstarControl("WIPItemRejects_CurrentDetails") as JQDataGrid; } }
        
        // Location text box
        CWC.TextBox _txtXLocationField { get { return Page.FindCamstarControl("WIPItemRejects_WaferRejectsXLocation") as CWC.TextBox; } }
        CWC.TextBox _txtYLocationField { get { return Page.FindCamstarControl("WIPItemRejects_WaferRejectsYLocation") as CWC.TextBox; } }
        CWC.TextBox _txtZLocationField { get { return Page.FindCamstarControl("WIPItemRejects_WaferRejectsZLocation") as CWC.TextBox; } }

        CWC.TextBox _txtWaferRejectsQtyField { get { return Page.FindCamstarControl("WIPItemRejects_WaferRejectsQty") as CWC.TextBox; } }
        CWC.CheckBox _chkYieldOffWaferField { get { return Page.FindCamstarControl("WIPItemRejects_YieldOffWafer") as CWC.CheckBox; } }
        CWC.CheckBox _chkRejectWholeSubProductField { get { return Page.FindCamstarControl("WIPItemRejects_RejectWholeSubProduct") as CWC.CheckBox; } }

        CWC.ContainerList _ContainerField { get { return Page.FindCamstarControl("WIPItemRejects_Container") as CWC.ContainerList; } }
        CWC.NamedObject _ndoProcessTypeField { get { return Page.FindCamstarControl("WIPItemRejects_ProcessType") as CWC.NamedObject; } }
        CWC.NamedObject _ndoEquipmentField { get { return Page.FindCamstarControl("WIPItemRejects_Equipment") as CWC.NamedObject; } }
        CWC.DropDownList _ddlServiceTypeField { get { return Page.FindCamstarControl("WIPItemRejects_ServiceType") as CWC.DropDownList; } }
        CWC.NamedObject _ndoWaferScribeNumberField { get { return Page.FindCamstarControl("WIPItemRejects_WaferScribeNumber") as CWC.NamedObject; } }
        CWC.DropDownList _ddlWaferRejectsTypeField { get { return Page.FindCamstarControl("WIPItemRejects_WaferRejectsType") as CWC.DropDownList; } }
        CWC.NamedObject _ndoLossReasonField { get { return Page.FindCamstarControl("WIPItemRejects_LossReason") as CWC.NamedObject; } }
        CWC.NamedObject _ndoRejectCategoryField { get { return Page.FindCamstarControl("WIPItemRejects_RejectCategory") as CWC.NamedObject; } }
        CWC.RevisionedObject _rdoSubProductField { get { return Page.FindCamstarControl("WIPItemRejects_SubProduct") as CWC.RevisionedObject; } }
        CWC.Button _btnSubmit { get { return Page.FindCamstarControl("WIPItemRejects_SubmitButton") as CWC.Button; } }
        CWC.Button _btnReset { get { return Page.FindCamstarControl("ResetButton") as CWC.Button; } }

        // Summary textbox
        CWC.TextBox _txtTotalReworkableRejectQtyField { get { return Page.FindCamstarControl("WIPItemRejects_TotalReworkableRejectQty") as CWC.TextBox; } }
        CWC.TextBox _txtTotalUnidentifiableRejectQtyField { get { return Page.FindCamstarControl("WIPItemRejects_TotalUnidentifiableRejectQty") as CWC.TextBox; } }
        CWC.TextBox _txtTotalWaferRejectsQtyField { get { return Page.FindCamstarControl("WIPItemRejects_TotalWaferRejectsQty") as CWC.TextBox; } }
        CWC.TextBox _txtTxnTypeField { get { return Page.FindCamstarControl("WIPItemRejects_TxnType") as CWC.TextBox; } }
        
        //
        string sAllowReworkableRejectQty;
        string sAllowUnidentifiableRejectQty;

        protected CWC.TextBox _txtWIPItemRejects_Init { get { return Page.FindCamstarControl("WIPItemRejects_Init") as CWC.TextBox; } }
        protected bool _bIsPopup { get { return Page.IsAJAXFloatingFrame; } }
        // TESTING AREA //
        private string _waferName = null;
        private int _rowId = -1;
        #endregion

        #region Constants     
        const string const_sLocation = "LOCATION";
        const string const_sWhole = "WHOLE";
        const string const_sMoveOut = "Move Out";
        #endregion

        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            _ContainerField.DataChanged += Container_DataChanged;
            _ndoProcessTypeField.DataChanged += ProcessType_DataChanged;
            _ndoEquipmentField.DataChanged += Equipment_DataChanged;
            _txtWIPItemRejects_Init.DataChanged += _txtWIPItemRejects_Init_DataChanged;
            _ddlWaferRejectsTypeField.DataChanged += RejectsType_DataChanged; 
        } // OnPreLoad

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            // Set service name of the button dynamically.
            PERS.SubmitAction bSubmit = new PERS.SubmitAction();
            bSubmit.ServiceName = _ddlServiceTypeField.Data.ToString();
            bSubmit.Location = PERS.ActionLocation.Button;
            bSubmit.Permissions = new PERS.PermissionDefinition();
            bSubmit.Permissions.DisplayMode = PERS.PermissionDisplayMode.Disable;
            bSubmit.Permissions.PagePermission = true;
            bSubmit.Permissions.ServicePermission = false; 
            _btnSubmit.DefaultAction = bSubmit;

            // Get Computername
            _txtComputerNameField.Data = SEMI.AppCode.UIUtility.GetComputerName(this);

            // Make Process Type field visible when Container at Move Out State
            if ((_txtTxnTypeField?.Data?.ToString() ?? "") == const_sMoveOut)
                _ndoProcessTypeField.Enabled = true;

            if (!Page.IsPostBack)
            {
                // During Move Out, wip main process type default to NORMAL, clear it let system get the process type base on container
                if ((_txtTxnTypeField?.Data?.ToString() ?? "") == const_sMoveOut)
                    _ndoProcessTypeField.Data = null;

                // additional logic for CR019 EquipmentWIPMain (or where the web part is referenced into a standalone page)
                if (Page.IsAJAXFloatingFrame)
                {
                    // hide the submit and reset buttons
                    _btnSubmit.Visible = false;
                    _btnSubmit.Enabled = false;
                    _btnReset.Visible = false;
                    _btnReset.Enabled = false;

                    Personalization.UIAction[] actUIActions = Page.ActionDispatcher.PageActions();
                    foreach (Personalization.UIAction act in actUIActions)
                    {
                        if (act.Name.ToUpper() == "SUBMIT")
                        {
                            act.ServiceName = _ddlServiceTypeField.Data.ToString();
                            act.Permissions = new PERS.PermissionDefinition();
                            act.Permissions.DisplayMode = PERS.PermissionDisplayMode.Disable;
                            act.Permissions.PagePermission = true;
                            act.Permissions.ServicePermission = false;
                        }
                    }

                    FetchData();
                }
            }
        }

        void _txtWIPItemRejects_Init_DataChanged(object sender, EventArgs e)
        {
            if (!_ContainerField.IsEmpty && !_bIsPopup)
            {
                ResetFields();
                FetchData();
            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            if (action != null && action.Parameters == "WIPItemRejects_Reload")
            {
                ResetFields();
                FetchData();
            }
            else if (action != null && action.Parameters == "AddToGrid")
                AddSvcDetailsRowItem();
        }

        public void Container_DataChanged(object sender, EventArgs e)
        {
            if (!_ContainerField.IsEmpty && !_bIsPopup)
            {
                ResetFields();
                FetchData();
            }
        }

        public void ProcessType_DataChanged(object sender, EventArgs e)
        {
            if ((!_ndoProcessTypeField.IsEmpty) && (!_ContainerField.IsEmpty))
            {
                ResetFields();
                FetchData();
            }
        }

        public void Equipment_DataChanged(object sender, EventArgs e)
        {
            if ((!_ndoEquipmentField.IsEmpty) && (!_ContainerField.IsEmpty) && !_bIsPopup)
            {
                ResetFields();
                FetchData();
            }
        }

        public void RejectsType_DataChanged(object sender, EventArgs e)
        {
            if (!_ddlWaferRejectsTypeField.IsEmpty)
            {
                if (_ddlWaferRejectsTypeField.Data.ToString() == const_sLocation)
                    XYZLocation_Visible(true);
                else
                    XYZLocation_Visible(false);
            }
        }

        private void XYZLocation_Visible(bool bValue)
        {
            _txtXLocationField.Visible = bValue;
            _txtYLocationField.Visible = bValue;
            _txtZLocationField.Visible = bValue;

            CamstarWebControl.SetRenderToClient(_txtXLocationField);
            CamstarWebControl.SetRenderToClient(_txtXLocationField);
            CamstarWebControl.SetRenderToClient(_txtXLocationField);
        }

        private void FetchData()
        {
            try
            {
                bool bExecute = false;
                if (_chkIsActiveField != null)
                    bExecute = (_chkIsActiveField.CheckControl.Checked || _chkIsPopupField.CheckControl.Checked);

                if (bExecute)
                {
                    string sServiceType = _ddlServiceTypeField.Data.ToString();
                    this.PrimaryServiceType = sServiceType;
                    var fs = FrameworkManagerUtil.GetFrameworkSession();

                    // Prepare service
                    ResultStatus res = new ResultStatus(null, false);
                    var cdo = WCFObject.CreateObject(sServiceType) as ICreator;
                    var request = WCFObject.CreateObject(sServiceType + "_Request") as ICreator;
                    var reqInfo = WCFObject.CreateObject(sServiceType + "_Info") as ICreator;
                    Result oResponseData = null;
                    var service = new WSDataCreator().CreateService(sServiceType, fs.CurrentUserProfile);

                    // Set Data Input
                    cdo.SetValue("Container", _ContainerField.Data as ContainerRef);
                    cdo.SetValue("ProcessType", _ndoProcessTypeField.Data as NamedObjectRef);
                    if (!_ndoEquipmentField.IsEmpty)
                        cdo.SetValue("Equipment", _ndoEquipmentField.Data as NamedObjectRef);

                    // Request value
                    reqInfo.SetValue("MaxRejectQty", new OM.Info(true));
                    reqInfo.SetValue("IsWaferProcessing", new OM.Info(true));
                    reqInfo.SetValue("AllowDefectQty", new OM.Info(true));
                    reqInfo.SetValue("AllowReworkableRejectQty", new OM.Info(true));
                    reqInfo.SetValue("AllowUnidentifiableRejectQty", new OM.Info(true));
                    reqInfo.SetValue("ProcessTypeSelection", new OM.Info(true));
                    reqInfo.SetValue("EquipmentSelection", new OM.Info(true));
                    reqInfo.SetValue("IsMultiProducts", new OM.Info(true));

                    reqInfo.SetValue("WafersDetailsSelection", new OM.WIPLotTxnWafersDetails_Info());
                    reqInfo.SetValue("WafersDetailsSelection.WaferScribeNumber", new OM.Info(true));

                    reqInfo.SetValue("CurrentDetails", new OM.LotRejectsDetails_Info());
                    reqInfo.SetValue("CurrentDetails.WaferScribeNumber", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.WaferRejectsType", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.WaferRejectsQty", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.WaferRejectsXLocation", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.WaferRejectsYLocation", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.WaferRejectsZLocation", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.LossReason", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.RejectCategory", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.DefectQty", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.RejectQty", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.YieldOffWafer", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.ReworkableRejectQty", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.UnidentifiableRejectQty", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.ValidRejectQty", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.InvalidRejectQty", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.BonusBackRejectQty", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.RejectCause", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.RejectComment", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.SubProduct", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.RejectWholeSubProduct", new OM.Info(true));

                    // perf. enhancement to request for LossReason and SubProduct selectionvalues in a single request instead of the separate requests
                    reqInfo.SetValue("LossReason", new OM.Info(false, true));
                    reqInfo.SetValue("SubProduct", new OM.Info(false, true));

                    request.SetValue("Info", reqInfo);

                    // Execute Request 
                    ResultStatus oResultStatus = service.GetEnvironment(cdo as DCObject, request as Request, out oResponseData);
                    if (oResultStatus.IsSuccess)
                    {
                        // Sub-Product and Reject Whole Sub-Product fields visible if Lot’s Product has multi-product enable, else hidden
                        Boolean isMultiProductFlag = false;
                        if ((oResponseData as ICreator).GetValue("Value.IsMultiProducts") != null)
                           isMultiProductFlag =  Convert.ToBoolean((oResponseData as ICreator).GetValue("Value.IsMultiProducts").ToString() as string);

                        _chkRejectWholeSubProductField.Visible = isMultiProductFlag;
                        _chkRejectWholeSubProductField.Enabled = isMultiProductFlag;
                        _rdoSubProductField.Visible = isMultiProductFlag;
                        _rdoSubProductField.Enabled = isMultiProductFlag;                        

                        _txtMaxRejectQtyField.Data = ((oResponseData as ICreator).GetValue("Value.MaxRejectQty").ToString() as string);
                        LotRejectsDetails[] objRejectsDetails = ((oResponseData as ICreator).GetValue("Value.CurrentDetails") as LotRejectsDetails[]);
                        WIPLotTxnWafersDetails[] objWafersDetails = ((oResponseData as ICreator).GetValue("Value.WafersDetailsSelection") as WIPLotTxnWafersDetails[]);

                        // US494280: Set the Selection Values when the lot is in Move Out State
                        if ((_txtTxnTypeField?.Data?.ToString() ?? "") == const_sMoveOut)
                        {
                            CWC.NamedObject _ndoProcessTypeTemp = _ndoProcessTypeField;
                            SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoProcessTypeTemp, (oResponseData.Value as LotRejectsPostProcess)?.ProcessTypeSelection);
                        }

                        NamedObjectRef[] objProcessTypeSelection = ((oResponseData as ICreator).GetValue("Value.ProcessTypeSelection") as NamedObjectRef[]);

                        // Select the first record by default 
                        if (objProcessTypeSelection != null)
                        {
                            NamedObjectRef objProcessType = objProcessTypeSelection.FirstOrDefault(p => p.Name == (_ndoProcessTypeField?.Data?.ToString() ?? ""));
                            _ndoProcessTypeField.Data = objProcessType ?? objProcessTypeSelection[0];
                        }

                        sAllowReworkableRejectQty = ((oResponseData as ICreator).GetValue("Value.AllowReworkableRejectQty").ToString() as string);
                        sAllowUnidentifiableRejectQty = ((oResponseData as ICreator).GetValue("Value.AllowUnidentifiableRejectQty").ToString() as string);
                        ViewState["AllowReworkableRejectQty"] = sAllowReworkableRejectQty;
                        ViewState["AllowUnidentifiableRejectQty"] = sAllowUnidentifiableRejectQty;

                        // set the loss reason selection values                                                
                        if ((oResponseData as ICreator).GetValue("Environment.LossReason") != null)
                        {
                            OM.Environment envLossReason = (oResponseData as ICreator).GetValue("Environment.LossReason") as OM.Environment;
                            if (envLossReason.SelectionValues != null)
                            {
                                _ndoLossReasonField.SetSelectionValues(envLossReason.SelectionValues);
                                CamstarWebControl.SetRenderToClient(_ndoLossReasonField);
                            }
                        }

                        // set the subProduct selection values                                                                                               
                        if ((oResponseData as ICreator).GetValue("Environment.SubProduct") != null)
                        {
                            OM.Environment envSubProduct = (oResponseData as ICreator).GetValue("Environment.SubProduct") as OM.Environment;
                            if (envSubProduct.SelectionValues != null)
                            {
                                _rdoSubProductField.SetSelectionValues(envSubProduct.SelectionValues);
                                CamstarWebControl.SetRenderToClient(_rdoSubProductField);
                            }
                        }

                        // Clear datagrid
                        _gridCurrentDetailsField.ClearData();

                        // Hide columns based on criteria
                        JQFieldCollection objFieldCollection = _gridCurrentDetailsField.BoundContext.Fields;
                        foreach (JQField objField in objFieldCollection)
                        {
                            switch (objField.ID)
                            {
                                case "ReworkableRejectQty":
                                    objField.Visible = (sAllowReworkableRejectQty == "True");
                                    _txtTotalReworkableRejectQtyField.Visible = (sAllowReworkableRejectQty == "True");
                                    break;
                                case "UnidentifiableRejectQty":
                                    objField.Visible = (sAllowUnidentifiableRejectQty == "True");
                                    _txtTotalUnidentifiableRejectQtyField.Visible = (sAllowUnidentifiableRejectQty == "True");
                                    break;
                            }
                        }

                        if (objWafersDetails != null)
                        {
                            CWC.NamedObject _ndoWaferScribeNumberEx = Page.FindCamstarControl("WIPItemRejects_WaferScribeNumber") as CWC.NamedObject;
                            int iItems = objWafersDetails.Count();
                            NamedObjectRef[] refWafersDetails = new NamedObjectRef[iItems];
                            for (int i = 0; i < iItems; i++)
                            {
                                refWafersDetails[i] = new NamedObjectRef();
                                refWafersDetails[i].Name = objWafersDetails[i].WaferScribeNumber.ToString();
                            }

                            SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoWaferScribeNumberEx, refWafersDetails);
                            _ndoWaferScribeNumberEx.Data = refWafersDetails != null ? refWafersDetails[0].Name.ToString() : null;
                            CamstarWebControl.SetRenderToClient(_ndoWaferScribeNumberEx);

                            //Bind data manually to datagrid column "Item Id"
                            var _ndoWaferInlineEx = _gridCurrentDetailsField.FindControl("WaferScribeNumber_InlineEditorControl") as CWC.NamedObject;
                            SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoWaferInlineEx, refWafersDetails);
                        }

                        // Set the first record as the default value of Rejects Type
                        _ddlWaferRejectsTypeField.Data = const_sWhole;

                        // Bind response data to datagrid
                        if (objRejectsDetails != null)
                        {
                            (_gridCurrentDetailsField.GridContext as BoundContext).Data = objRejectsDetails.ToArray();
                            _gridCurrentDetailsField.BoundContext.LoadData();
                        }
                        CamstarWebControl.SetRenderToClient(_gridCurrentDetailsField);
                    }
                    else
                    {
                        DisplayMessage(oResultStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }

        public override void GetInputData(OM.Service serviceData)
        {
            try
            {
                sAllowUnidentifiableRejectQty = ViewState["AllowUnidentifiableRejectQty"] as string;
                sAllowReworkableRejectQty = ViewState["AllowReworkableRejectQty"] as string;                
                bool bExecute = false;
                if (_chkIsActiveField != null)
                    bExecute = (_chkIsActiveField.CheckControl.Checked || _chkIsPopupField.CheckControl.Checked);

                if (bExecute)
                {
                    base.GetInputData(serviceData);

                    if (serviceData is OM.LotRejectsInProcess)
                        (serviceData as OM.LotRejectsInProcess).Container = _ContainerField.Data as ContainerRef;
                    else if (serviceData is OM.LotRejectsDispose)
                        (serviceData as OM.LotRejectsDispose).Container = _ContainerField.Data as ContainerRef;
                    else if (serviceData is OM.LotRejectsPostProcess)
                        (serviceData as OM.LotRejectsPostProcess).Container = _ContainerField.Data as ContainerRef;

                    int gridCount = _gridCurrentDetailsField.BoundContext.GetTotalRows();

                    if (gridCount > 0)
                    {
                        LotRejectsDetails[] oRejectDetails = new LotRejectsDetails[gridCount];
                        for (int i = 0; i < gridCount; i++)
                        {
                            string sRowId = i.ToString().PadLeft(6, '0');

                            oRejectDetails[i] = new LotRejectsDetails();
                            if (_gridCurrentDetailsField.GridContext.GetCell(sRowId, "LossReason") != null)
                            {
                                oRejectDetails[i].LossReason = new NamedObjectRef();
                                oRejectDetails[i].LossReason.Name = _gridCurrentDetailsField.GridContext.GetCell(sRowId, "LossReason").ToString();
                            }

                            if (_gridCurrentDetailsField.GridContext.GetCell(sRowId, "RejectCategory") != null)
                            {
                                oRejectDetails[i].RejectCategory = new NamedObjectRef();
                                oRejectDetails[i].RejectCategory.Name = _gridCurrentDetailsField.GridContext.GetCell(sRowId, "RejectCategory").ToString();
                            }
                            if (sAllowReworkableRejectQty == "True")
                                oRejectDetails[i].ReworkableRejectQty = _gridCurrentDetailsField.GridContext.GetCell(sRowId, "ReworkableRejectQty") != null ? int.Parse(_gridCurrentDetailsField.GridContext.GetCell(sRowId, "ReworkableRejectQty").ToString()) : 0;
                            if (sAllowUnidentifiableRejectQty == "True")
                                oRejectDetails[i].UnidentifiableRejectQty = _gridCurrentDetailsField.GridContext.GetCell(sRowId, "UnidentifiableRejectQty") != null ? int.Parse(_gridCurrentDetailsField.GridContext.GetCell(sRowId, "UnidentifiableRejectQty").ToString()) : 0;

                            oRejectDetails[i].WaferScribeNumber = _gridCurrentDetailsField.GridContext.GetCell(sRowId, "WaferScribeNumber") != null ? _gridCurrentDetailsField.GridContext.GetCell(sRowId, "WaferScribeNumber").ToString() : "";
                            oRejectDetails[i].WaferRejectsQty = _gridCurrentDetailsField.GridContext.GetCell(sRowId, "WaferRejectsQty") != null ? int.Parse(_gridCurrentDetailsField.GridContext.GetCell(sRowId, "WaferRejectsQty").ToString()) : 0;
                            oRejectDetails[i].WaferRejectsType = _gridCurrentDetailsField.GridContext.GetCell(sRowId, "WaferRejectsType") != null ? _gridCurrentDetailsField.GridContext.GetCell(sRowId, "WaferRejectsType").ToString() : "";
                            oRejectDetails[i].YieldOffWafer = _gridCurrentDetailsField.GridContext.GetCell(sRowId, "YieldOffWafer").ToString() == "True" ? true : false;

                            if (oRejectDetails[i].WaferRejectsType != null)
                            {
                                if (oRejectDetails[i].WaferRejectsType == const_sLocation)
                                {
                                    oRejectDetails[i].WaferRejectsXLocation = _gridCurrentDetailsField.GridContext.GetCell(sRowId, "WaferRejectsXLocation") != null ? int.Parse(_gridCurrentDetailsField.GridContext.GetCell(sRowId, "WaferRejectsXLocation").ToString()) : 0;
                                    oRejectDetails[i].WaferRejectsYLocation = _gridCurrentDetailsField.GridContext.GetCell(sRowId, "WaferRejectsYLocation") != null ? int.Parse(_gridCurrentDetailsField.GridContext.GetCell(sRowId, "WaferRejectsYLocation").ToString()) : 0;
                                    oRejectDetails[i].WaferRejectsZLocation = _gridCurrentDetailsField.GridContext.GetCell(sRowId, "WaferRejectsZLocation") != null ? int.Parse(_gridCurrentDetailsField.GridContext.GetCell(sRowId, "WaferRejectsZLocation").ToString()) : 0;
                                }
                            }

                            oRejectDetails[i].RejectCause = _gridCurrentDetailsField.GridContext.GetCell(sRowId, "RejectCause") != null ? _gridCurrentDetailsField.GridContext.GetCell(sRowId, "RejectCause").ToString() : "";
                            oRejectDetails[i].RejectComment = _gridCurrentDetailsField.GridContext.GetCell(sRowId, "RejectComment") != null ? _gridCurrentDetailsField.GridContext.GetCell(sRowId, "RejectComment").ToString() : "";

                            // get the data for sub product
                            string[] SubProduct = _gridCurrentDetailsField.GridContext.GetCell(sRowId, "SubProduct") != null ? _gridCurrentDetailsField.GridContext.GetCell(sRowId, "SubProduct").ToString().Split(':') : new string[0];
                            if (SubProduct.Length > 0)
                            {
                                oRejectDetails[i].SubProduct = new RevisionedObjectRef();
                                oRejectDetails[i].SubProduct.Name = SubProduct[0];
                                if (SubProduct.Length > 1)
                                    oRejectDetails[i].SubProduct.Revision = SubProduct[1];
                                else
                                    oRejectDetails[i].SubProduct.RevisionOfRecord = true;
                                oRejectDetails[i].RejectWholeSubProduct = _gridCurrentDetailsField.GridContext.GetCell(sRowId, "RejectWholeSubProduct").ToString() == "True" ? true : false;
                            }
                        } //End for loop

                        if (serviceData is OM.LotRejectsInProcess)
                        {
                            (serviceData as OM.LotRejectsInProcess).Details = oRejectDetails;
                            (serviceData as OM.LotRejectsInProcess).Equipment = _ndoEquipmentField.Data as NamedObjectRef;
                        }
                        else if (serviceData is OM.LotRejectsDispose)
                        {
                            (serviceData as OM.LotRejectsDispose).Details = oRejectDetails;
                        }
                        else if (serviceData is OM.LotRejectsPostProcess)
                        {
                            (serviceData as OM.LotRejectsPostProcess).Details = oRejectDetails;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }

        private void ResetFields()
        {
            _txtMaxRejectQtyField.ClearData();
            _ndoWaferScribeNumberField.ClearData();
            _ndoWaferScribeNumberField.ClearSelectionValues();
            //_ddlWaferRejectsTypeField.ClearData();
            //_ddlWaferRejectsTypeField.ClearSelectionValues();
            _ddlWaferRejectsTypeField.Data = const_sWhole;
            _ndoLossReasonField.ClearData();
            _ndoLossReasonField.ClearSelectionValues();
            _ndoRejectCategoryField.ClearData();
            _ndoRejectCategoryField.ClearSelectionValues();
            //_txtWaferRejectsQtyField.ClearData();
            _txtWaferRejectsQtyField.Data = 0;
            _gridCurrentDetailsField.ClearData();
            _txtXLocationField.ClearData();
            _txtYLocationField.ClearData();
            _txtZLocationField.ClearData();
            _rdoSubProductField.ClearData();
            _chkYieldOffWaferField.ClearData();
			_chkRejectWholeSubProductField.ClearData();
        }
        
        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            bool bExecute = false;
            if (_chkIsActiveField != null)
                bExecute = _chkIsActiveField.CheckControl.Checked;

            if (bExecute)
            {
                if (status.IsSuccess)
                {
                    if (serviceData is OM.LotRejectsPostProcess || serviceData is OM.LotRejectsInProcess || serviceData is OM.LotRejectsDispose)
                        Container_DataChanged(null, null);
                }
            }
        }

        private void AddSvcDetailsRowItem()
        {
            try
            {
                sAllowReworkableRejectQty = ViewState["AllowReworkableRejectQty"] as string;
                sAllowUnidentifiableRejectQty = ViewState["AllowUnidentifiableRejectQty"] as string;
                if ((!_ndoWaferScribeNumberField.IsEmpty) && (!_ddlWaferRejectsTypeField.IsEmpty) && (!_ndoLossReasonField.IsEmpty))
                {                   
                    LotRejectsDetails[] oExistingList = (_gridCurrentDetailsField.GridContext as BoundContext).Data as LotRejectsDetails[];
                    List<LotRejectsDetails> oNewList = new List<LotRejectsDetails>();            

                    if (oExistingList == null)
                        oExistingList = new LotRejectsDetails[0];

                    foreach (LotRejectsDetails oRow in oExistingList)
                    {
                        LotRejectsDetails oCurrentRow = new LotRejectsDetails();
                        oCurrentRow.WaferScribeNumber = oRow.WaferScribeNumber;
                        oCurrentRow.WaferRejectsType = oRow.WaferRejectsType;
                        oCurrentRow.LossReason = oRow.LossReason;                        
                        oCurrentRow.ReworkableRejectQty = oRow.ReworkableRejectQty;                        
                        oCurrentRow.UnidentifiableRejectQty = oRow.UnidentifiableRejectQty;
                        oCurrentRow.WaferRejectsQty = oRow.WaferRejectsQty;
                        oCurrentRow.YieldOffWafer = oRow.YieldOffWafer;
                        oCurrentRow.RejectCategory = oRow.RejectCategory;
                        if (_rdoSubProductField.Visible)
                        {
                            oCurrentRow.SubProduct = oRow.SubProduct;
                            oCurrentRow.RejectWholeSubProduct = oRow.RejectWholeSubProduct;
                        }
                        if (_ddlWaferRejectsTypeField.Data.ToString() == const_sLocation)
                        {
                            oCurrentRow.WaferRejectsXLocation = oRow.WaferRejectsXLocation;
                            oCurrentRow.WaferRejectsYLocation = oRow.WaferRejectsYLocation;
                            oCurrentRow.WaferRejectsZLocation = oRow.WaferRejectsZLocation;
                        }
                        oNewList.Add(oCurrentRow);
                    }
                    LotRejectsDetails oNewRow = new LotRejectsDetails();
                    oNewRow.WaferScribeNumber = _ndoWaferScribeNumberField.Data != null ? _ndoWaferScribeNumberField.Data.ToString() : null;
                    oNewRow.WaferRejectsType = _ddlWaferRejectsTypeField.Data != null ? _ddlWaferRejectsTypeField.Data.ToString() : null;
                    oNewRow.LossReason = _ndoLossReasonField.Data != null ? _ndoLossReasonField.Data as NamedObjectRef : null;
                    if (sAllowReworkableRejectQty == "True")
                        oNewRow.ReworkableRejectQty = 0;
                    if (sAllowUnidentifiableRejectQty == "True")
                        oNewRow.UnidentifiableRejectQty = 0;
                    oNewRow.WaferRejectsQty = _txtWaferRejectsQtyField.Data != null ? int.Parse(_txtWaferRejectsQtyField.Data.ToString()) : 0;
                    oNewRow.YieldOffWafer = _chkYieldOffWaferField.IsChecked;
                    oNewRow.RejectCategory = _ndoRejectCategoryField.Data != null ? _ndoRejectCategoryField.Data as NamedObjectRef : null;
                    if (_rdoSubProductField.Visible)
                    {
                        oNewRow.SubProduct = _rdoSubProductField.Data != null ? _rdoSubProductField.Data as RevisionedObjectRef : null;
                        oNewRow.RejectWholeSubProduct = _chkRejectWholeSubProductField.IsChecked ? true : false;
                    }
                    if (_ddlWaferRejectsTypeField.Data.ToString() == const_sLocation)
                    {
                        oNewRow.WaferRejectsXLocation = _txtXLocationField.Data != null ? double.Parse(_txtXLocationField.Data.ToString()) : 0;
                        oNewRow.WaferRejectsYLocation = _txtYLocationField.Data != null ? double.Parse(_txtYLocationField.Data.ToString()) : 0;
                        oNewRow.WaferRejectsZLocation = _txtZLocationField.Data != null ? double.Parse(_txtZLocationField.Data.ToString()) : 0;
                    }
                    oNewList.Add(oNewRow);

                    (_gridCurrentDetailsField.GridContext as BoundContext).Data = oNewList.ToArray();
                    _gridCurrentDetailsField.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(_gridCurrentDetailsField);
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "() : " + Ex.Message, false));
            }
        }
    }
}



