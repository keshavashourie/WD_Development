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
    /// Summary description for WIPLotRejects
    /// </summary>
    public class WIPLotRejects : MatrixWebPart
    {
        #region Properties 
        CWC.CheckBox _chkIsActiveField { get { return Page.FindCamstarControl("WIPLotRejects_IsActive") as CWC.CheckBox; } }
        CWC.CheckBox _chkIsPopupField { get { return Page.FindCamstarControl("WIPLotRejects_IsPopup") as CWC.CheckBox; } }
        CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("WIPLotRejects_ComputerName") as CWC.TextBox; } }
        CWC.TextBox _txtMaxRejectQtyField { get { return Page.FindCamstarControl("WIPLotRejects_MaxRejectQty") as CWC.TextBox; } }
        JQDataGrid _gridCurrentDetailsField { get { return Page.FindCamstarControl("WIPLotRejects_CurrentDetails") as JQDataGrid; } }
        
        // Summary textbox
        CWC.TextBox _txtRejectQtyField { get { return Page.FindCamstarControl("WIPLotRejects_RejectQty") as CWC.TextBox; } }
        CWC.TextBox _txtDefectQtyField { get { return Page.FindCamstarControl("WIPLotRejects_DefectQty") as CWC.TextBox; } }
        CWC.TextBox _txtReworkableRejectQtyField { get { return Page.FindCamstarControl("WIPLotRejects_ReworkableRejectQty") as CWC.TextBox; } }
        CWC.TextBox _txtUnidentifiableRejectQtyField { get { return Page.FindCamstarControl("WIPLotRejects_UnidentifiableRejectQty") as CWC.TextBox; } }
        CWC.TextBox _txtValidRejectQtyField { get { return Page.FindCamstarControl("WIPLotRejects_ValidRejectQty") as CWC.TextBox; } }
        CWC.TextBox _txtInvalidRejectQtyField { get { return Page.FindCamstarControl("WIPLotRejects_InvalidRejectQty") as CWC.TextBox; } }
        CWC.TextBox _txtBonusBackRejectQtyField { get { return Page.FindCamstarControl("WIPLotRejects_BonusBackRejectQty") as CWC.TextBox; } }
        
        CWC.ContainerList _ContainerField { get { return Page.FindCamstarControl("WIPLotRejects_Container") as CWC.ContainerList; } }
        CWC.NamedObject _ndoProcessTypeField { get { return Page.FindCamstarControl("WIPLotRejects_ProcessType") as CWC.NamedObject; } }
        CWC.NamedObject _ndoEquipmentField { get { return Page.FindCamstarControl("WIPLotRejects_Equipment") as CWC.NamedObject; } }
        CWC.DropDownList _ddlServiceTypeField { get { return Page.FindCamstarControl("WIPLotRejects_ServiceType") as CWC.DropDownList; } }
        CWC.Button _btnSubmit { get { return Page.FindCamstarControl("WIPLotRejects_SubmitButton") as CWC.Button; } }
        CWC.Button _btnReset { get { return Page.FindCamstarControl("WIPLotRejects_ResetButton") as CWC.Button; } }
        
        protected CWC.TextBox _txtWIPLotRejects_Init { get { return Page.FindCamstarControl("WIPLotRejects_Init") as CWC.TextBox; } }
        protected bool _bIsPopup { get { return Page.IsAJAXFloatingFrame; } }
        CWC.TextBox _txtTxnTypeField { get { return Page.FindCamstarControl("WIPLotRejects_TxnType") as CWC.TextBox; } }
        #endregion

        #region Constants
        const string const_sLotRejectsInProcess = "LotRejectsInProcess";
        const string const_sLotRejectsDispose = "LotRejectsDispose";
        const string const_sMoveOut = "Move Out";

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _ContainerField.DataChanged += Container_DataChanged;
            _ndoProcessTypeField.DataChanged += ProcessType_DataChanged;
            _ndoEquipmentField.DataChanged += Equipment_DataChanged;
            _txtWIPLotRejects_Init.DataChanged += _txtWIPLotRejects_Init_Init_DataChanged;

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

                // additional logic for CR019 EquipmentWIPMain (or where the web part is referenced into a popup page)
                if (_bIsPopup)
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

        void _txtWIPLotRejects_Init_Init_DataChanged(object sender, EventArgs e)
        {
            // additional logic for CR019 to disable the event if this page is called as a popup
            if (!_ContainerField.IsEmpty && !_bIsPopup)
                FetchData();
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            if (action != null && action.Parameters == "WIPLotRejects_Reload" && !_ContainerField.IsEmpty)
                FetchData();
        }

        public void Container_DataChanged(object sender, EventArgs e)
        {
            // additional logic for CR019 to disable the event if this page is called as a popup
            if (!_ContainerField.IsEmpty && !_bIsPopup)
                FetchData();
        }

        public void ProcessType_DataChanged(object sender, EventArgs e)
        {
            // additional logic for CR019 to disable the event if this page is called as a popup
            if ((!_ndoProcessTypeField.IsEmpty) && (!_ContainerField.IsEmpty))
                FetchData();
        }

        public void Equipment_DataChanged(object sender, EventArgs e)
        {
            // additional logic for CR019 to disable the event if this page is called as a popup
            if ((!_ndoEquipmentField.IsEmpty) && (!_ContainerField.IsEmpty) && (!_bIsPopup))
                FetchData();
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

                    reqInfo.SetValue("CurrentDetails", new OM.LotRejectsDetails_Info());
                    reqInfo.SetValue("CurrentDetails.LossReason", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.RejectCategory", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.DefectQty", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.RejectQty", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.ReworkableRejectQty", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.UnidentifiableRejectQty", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.ValidRejectQty", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.InvalidRejectQty", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.BonusBackRejectQty", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.RejectCause", new OM.Info(true));
                    reqInfo.SetValue("CurrentDetails.RejectComment", new OM.Info(true));

                    request.SetValue("Info", reqInfo);

                    // Execute Request 
                    ResultStatus oResultStatus = service.GetEnvironment(cdo as DCObject, request as Request, out oResponseData);
                    if (oResultStatus.IsSuccess)
                    {
                        _txtMaxRejectQtyField.Data = ((oResponseData as ICreator).GetValue("Value.MaxRejectQty").ToString() as string);
                        LotRejectsDetails[] objRejectsDetails = ((oResponseData as ICreator).GetValue("Value.CurrentDetails") as LotRejectsDetails[]);

                        string sIsWaferProcessing = ((oResponseData as ICreator).GetValue("Value.IsWaferProcessing").ToString() as string);
                        string sAllowDefectQty = ((oResponseData as ICreator).GetValue("Value.AllowDefectQty").ToString() as string);
                        string sAllowReworkableRejectQty = ((oResponseData as ICreator).GetValue("Value.AllowReworkableRejectQty").ToString() as string);
                        string sAllowUnidentifiableRejectQty = ((oResponseData as ICreator).GetValue("Value.AllowUnidentifiableRejectQty").ToString() as string);

                        // Clear datagrid
                        _gridCurrentDetailsField.ClearData();

                        // Hide columns based on criteria
                        JQFieldCollection objFieldCollection = _gridCurrentDetailsField.BoundContext.Fields;
                        foreach (JQField objField in objFieldCollection)
                        {
                            switch (objField.ID)
                            {
                                case "RejectQty":
                                    objField.Visible = (sIsWaferProcessing == "False");
                                    break;
                                case "DefectQty":
                                    objField.Visible = (sAllowDefectQty == "True");
                                    break;
                                case "ReworkableRejectQty":
                                    objField.Visible = (sAllowReworkableRejectQty == "True");
                                    break;
                                case "UnidentifiableRejectQty":
                                    objField.Visible = (sAllowUnidentifiableRejectQty == "True");
                                    break;
                                case "ValidRejectQty":
                                case "InvalidRejectQty":
                                case "BonusBackRejectQty":
                                    objField.Visible = (sServiceType == const_sLotRejectsDispose);
                                    break;
                            }
                        }

                        // Hide or display Summation text boxes
                        _txtRejectQtyField.Visible = (sIsWaferProcessing == "False");
                        _txtDefectQtyField.Visible = (sAllowDefectQty == "True");
                        _txtReworkableRejectQtyField.Visible = (sAllowReworkableRejectQty == "True");
                        _txtUnidentifiableRejectQtyField.Visible = (sAllowUnidentifiableRejectQty == "True");
                        _txtValidRejectQtyField.Visible = (sServiceType == const_sLotRejectsDispose);
                        _txtInvalidRejectQtyField.Visible = (sServiceType == const_sLotRejectsDispose);
                        _txtBonusBackRejectQtyField.Visible = (sServiceType == const_sLotRejectsDispose);

                        // Bind response data to datagrid
                        if (objRejectsDetails != null)
                        {
                            (_gridCurrentDetailsField.GridContext as BoundContext).Data = objRejectsDetails.ToArray();
                            _gridCurrentDetailsField.BoundContext.LoadData();
                        }
                        CamstarWebControl.SetRenderToClient(_gridCurrentDetailsField);

                        // US494280: Set the ProcessType to the first of the SelVal when the Process Type pass in is not in SelVal
                        if ((_txtTxnTypeField?.Data?.ToString() ?? "") == const_sMoveOut)
                        {
                            CWC.NamedObject _ndoProcessTypeTemp = _ndoProcessTypeField;
                            SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoProcessTypeTemp, (oResponseData.Value as LotRejectsPostProcess).ProcessTypeSelection);
                        }

                        // Select the first record by default 
                        NamedObjectRef[] objProcessTypeSelection = ((oResponseData as ICreator).GetValue("Value.ProcessTypeSelection") as NamedObjectRef[]);

                        if (objProcessTypeSelection != null)
                        {
                            NamedObjectRef objProcessType = objProcessTypeSelection?.FirstOrDefault(p => p.Name == (_ndoProcessTypeField?.Data?.ToString() ?? ""));
                            _ndoProcessTypeField.Data = objProcessType ?? objProcessTypeSelection[0];
                        }

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
                bool bExecute = false;
                if (_chkIsActiveField != null)
                    bExecute = (_chkIsActiveField.CheckControl.Checked || _chkIsPopupField.CheckControl.Checked);

                if (bExecute)
                {
                    base.GetInputData(serviceData);

                    if (serviceData is OM.LotRejectsInProcess)
                    {
                        (serviceData as OM.LotRejectsInProcess).Container = _ContainerField.Data as ContainerRef;
                        (serviceData as OM.LotRejectsInProcess).Equipment = _ndoEquipmentField.Data as NamedObjectRef;
                    }
                    else if (serviceData is OM.LotRejectsDispose)
                    {
                        (serviceData as OM.LotRejectsDispose).Container = _ContainerField.Data as ContainerRef;
                    }
                    else if (serviceData is OM.LotRejectsPostProcess)
                    {
                        (serviceData as OM.LotRejectsPostProcess).Container = _ContainerField.Data as ContainerRef;
                    }

                    int gridCount = _gridCurrentDetailsField.BoundContext.GetTotalRows();
                    if (gridCount > 0)
                    {
                        LotRejectsDetails[] oDetails = _gridCurrentDetailsField.Data as LotRejectsDetails[];
                        // nullify __index, __id and __parent 
                        foreach (LotRejectsDetails oItem in oDetails)
                        {
                            oItem.ListItemIndex = null;
                            oItem.Self = null;
                            oItem.LossReason.ID = null;
                        }

                        if (serviceData is OM.LotRejectsInProcess)                                                                        
                            (serviceData as OM.LotRejectsInProcess).Details = oDetails;                         
                        else if (serviceData is OM.LotRejectsDispose)                        
                            (serviceData as OM.LotRejectsDispose).Details = oDetails;                                                    
                        else if (serviceData is OM.LotRejectsPostProcess)                        
                            (serviceData as OM.LotRejectsPostProcess).Details = oDetails;                                                
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "(): " + ex.Message, false));
            }
        }

        private void ResetFields()
        {
            _gridCurrentDetailsField.ClearData();
            _txtMaxRejectQtyField.ClearData();
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

        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            _ContainerField.DataChanged += Container_DataChanged;
            _ndoProcessTypeField.DataChanged += ProcessType_DataChanged;
            _ndoEquipmentField.DataChanged += Equipment_DataChanged;
            _txtWIPLotRejects_Init.DataChanged += _txtWIPLotRejects_Init_Init_DataChanged;
        }
    }
}



