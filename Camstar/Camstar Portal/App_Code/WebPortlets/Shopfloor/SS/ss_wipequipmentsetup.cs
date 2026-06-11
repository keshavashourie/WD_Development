/* Copyright 2023 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Windows;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using PERS = Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

/// <summary>
/// Summary description for SS_WIPEquipmentSetup
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_WIPEquipmentSetup : MatrixWebPart
    {
        #region Properties

        // ContainerLists
        CWC.ContainerList _ctlContainerField { get { return Page.FindCamstarControl("EquipmentSetup_ToolPlanLot") as CWC.ContainerList; } }
        // NamedObjects
        CWC.NamedObject _ndoProcessTypeField { get { return Page.FindCamstarControl("EquipmentSetup_ToolPlanProcessType") as CWC.NamedObject; } }
        CWC.NamedObject _ndoEquipmentField { get { return Page.FindCamstarControl("EquipmentSetup_Resource") as CWC.NamedObject; } }
        CWC.NamedObject _ndoEmployeeField { get { return Page.FindCamstarControl("EquipmentSetup_Employee") as CWC.NamedObject; } }
        CWC.NamedObject _ndoMaskField { get { return Page.FindCamstarControl("EquipmentSetup_Mask") as CWC.NamedObject; } }
        CWC.NamedObject _ndoLotStepNameField { get { return Page.FindCamstarControl("EquipmentSetup_ReferenceLotStepName") as CWC.NamedObject; } }
        CWC.NamedObject _ndoLotProcessTypeField { get { return Page.FindCamstarControl("EquipmentSetup_ReferenceLotProcessType") as CWC.NamedObject; } }
        CWC.NamedObject _ndoLotMaskField { get { return Page.FindCamstarControl("EquipmentSetup_ReferenceLotMask") as CWC.NamedObject; } }
        CWC.NamedObject _ndoDocumentSetField { get { return Page.FindCamstarControl("EquipmentSetup_DocumentSet") as CWC.NamedObject; } }
        CWC.NamedObject _ndoToolPlanField { get { return Page.FindCamstarControl("EquipmentSetup_ToolPlan") as CWC.NamedObject; } }
        CWC.NamedObject _ndoParentResourceField { get { return Page.FindCamstarControl("EquipmentSetup_ParentResource") as CWC.NamedObject; } }
        CWC.NamedObject _ndoMachineGroupField { get { return Page.FindCamstarControl("EquipmentSetup_MachineGroup") as CWC.NamedObject; } }
        CWC.NamedObject _ndoPackageGroupField { get { return Page.FindCamstarControl("EquipmentSetup_PackageGroup") as CWC.NamedObject; } }
        CWC.NamedObject _ndoPhysicalLocationField { get { return Page.FindCamstarControl("EquipmentSetup_PhysicalLocation") as CWC.NamedObject; } }
        CWC.NamedObject _ndoPhysicalPositionField { get { return Page.FindCamstarControl("EquipmentSetup_PhysicalPosition") as CWC.NamedObject; } }
        CWC.NamedObject _ndoMfgLineField { get { return Page.FindCamstarControl("EquipmentSetup_ss_MfgLine") as CWC.NamedObject; } }
        // RevisionedObjects
        CWC.RevisionedObject _rdoRecipeField { get { return Page.FindCamstarControl("EquipmentSetup_Recipe") as CWC.RevisionedObject; } }
        CWC.RevisionedObject _rdoProductField { get { return Page.FindCamstarControl("EquipmentSetup_Product") as CWC.RevisionedObject; } }
        // TextBoxs
        CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("EquipmentSetup_ComputerName") as CWC.TextBox; } }
        CWC.TextBox _txtCommentsField { get { return Page.FindCamstarControl("EquipmentSetup_Comments") as CWC.TextBox; } }
        CWC.TextBox _txtScanToolsField { get { return Page.FindCamstarControl("EquipmentSetup_ScanTools") as CWC.TextBox; } }
        CWC.TextBox _txtReferenceLotField { get { return Page.FindCamstarControl("EquipmentSetup_ReferenceLot") as CWC.TextBox; } }
        CWC.TextBox _txtToolPlanNameField { get { return Page.FindCamstarControl("EquipmentSetup_ToolPlanName") as CWC.TextBox; } }
        CWC.TextBox _txtToolPlanDesField { get { return Page.FindCamstarControl("EquipmentSetup_ToolPlanDescription") as CWC.TextBox; } }
        CWC.TextBox _txtMaxLotsField { get { return Page.FindCamstarControl("EquipmentSetup_MaxLots") as CWC.TextBox; } }
        CWC.TextBox _txtMaxUnitsField { get { return Page.FindCamstarControl("EquipmentSetup_MaxUnits") as CWC.TextBox; } }
        // JQDataGrids
        JQDataGrid _gridParamsDetailsFields { get { return Page.FindCamstarControl("EquipmentSetup_Params") as JQDataGrid; } }
        JQDataGrid _gridToolsDetailsFields { get { return Page.FindCamstarControl("EquipmentSetup_Tools") as JQDataGrid; } }
        JQDataGrid _gridToolPlanDetailsFields { get { return Page.FindCamstarControl("EquipmentSetup_ToolPlanDetails") as JQDataGrid; } }
        JQDataGrid scsEquipmentToolMaskDetailsFields { get { return Page.FindCamstarControl("scsEquipmentSetup_MaskTool") as JQDataGrid; } }
        //Buttons
        CWC.Button _btnDocumentSet { get { return Page.FindCamstarControl("EquipmentSetup_DocumentSetButton") as CWC.Button; } }
        CWC.Button _btnCopyReference { get { return Page.FindCamstarControl("EquipmentSetup_CopyReferenceButton") as CWC.Button; } }
        CWC.Button _btnReset { get { return Page.FindCamstarControl("EquipmentSetup_ResetButton") as CWC.Button; } }
        CWC.Button _btnSubmit { get { return Page.FindCamstarControl("EquipmentSetup_SubmitButton") as CWC.Button; } }
        // Dropdownlists
        CWC.DropDownList _ddlServiceTypeField { get { return Page.FindCamstarControl("WIPEquipmentSetup_ServiceType") as CWC.DropDownList; } }
        CWC.DropDownList _ddlMultiLotsFlag { get { return Page.FindCamstarControl("EquipmentSetup_MultiLotsFlag") as CWC.DropDownList; } }
        // Checkboxs
        CWC.CheckBox _chkIsActiveField { get { return Page.FindCamstarControl("WIPEquipmentSetup_IsActive") as CWC.CheckBox; } }
        CWC.TextBox _txtResourceChangeCount { get { return Page.FindCamstarControl("EquipmentSetup_ResourceChangeCount") as CWC.TextBox; } }
        //ToggleContainer
        ToggleContainer _tgcToolPlanFieldCtl { get { return Page.FindCamstarControl("ToolPlanFieldCtl") as ToggleContainer; } }
        ToggleContainer _tgcReferenceFieldCtl { get { return Page.FindCamstarControl("ReferenceFieldCtl") as ToggleContainer; } }

        protected MatrixWebPart wpToolPlanWP
        {
            get
            {
                MatrixWebPart wp = null;
                int iParentControlsCount = Parent.Controls.Count;
                for (int z = 0; z < iParentControlsCount; z++)
                {
                    if (Parent.Controls[z].ID == "ToolPlanWP")
                        wp = Parent.Controls[z] as MatrixWebPart;
                }
                return wp;
            }
        }

        protected bool _bIsPopup { get { return Page.IsAJAXFloatingFrame; } }
        protected bool _bIsExecute { get { return !_ndoEquipmentField.IsEmpty; } }

        #endregion
        Boolean isResourceChange = false;

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            _ndoEquipmentField.DataChanged += new EventHandler(ResourceField_DataChanged);
            _txtScanToolsField.TextChanged += new EventHandler(ToolTextControl_TextChanged);
            _ndoToolPlanField.DataChanged += new EventHandler(ToolPlanField_DataChanged);
            if (_btnReset != null)
                _btnReset.Click += new EventHandler(ResetButton_Click);

            if (_ndoEmployeeField.Visible == true)
                _ndoDocumentSetField.DataChanged += new EventHandler(DocumentSetField_DataChanged);
            else
                _ctlContainerField.DataChanged += new EventHandler(ProcessTypeLotField_DataChanged);
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument")
                {
                    var attrVal = Page.PortalContext.DataContract.GetValueByName<string>("WIPEquipmentSetup_ReturnedValueDM");
                    var rowid = Page.PortalContext.DataContract.GetValueByName<string>("WIPEquipmentSetup_SelectedRowIdDM");
                    if (!string.IsNullOrEmpty(rowid))
                    {
                        if (attrVal != null)
                        {
                            int iRowId = int.Parse(rowid);
                            var data = _gridToolsDetailsFields.Data as NamedObjectRef[];
                            data[iRowId] = new NamedObjectRef();
                            data[iRowId].Name = attrVal;
                        }
                    }

                    // clear the data contracts as the mess with the data loading of grid values
                    Page.PortalContext.DataContract.SetValueByName("WIPEquipmentSetup_ReturnedValueDM", null);
                    Page.PortalContext.DataContract.SetValueByName("WIPEquipmentSetup_SelectedRowIdDM", null);
                } // if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument")                

                // Set the grid to visible if the meets equipment as primary service
                string CurrentPrimaryServiceType = this.PrimaryServiceType;
                if (CurrentPrimaryServiceType.ToLower().IndexOf("equipment") != 0)
                {
                    if (CurrentPrimaryServiceType.ToLower().Contains("equipment"))
                        scsEquipmentToolMaskDetailsFields.Visible = true;
                    else
                        scsEquipmentToolMaskDetailsFields.Visible = false;
                }



                if (_ddlServiceTypeField.Data != null)
                    this.PrimaryServiceType = _ddlServiceTypeField.Data.ToString();
                else
                    _ddlServiceTypeField.Data = this.PrimaryServiceType;

                if (_bIsPopup)
                {
                    if (_btnSubmit != null && _btnReset != null)
                    {
                        _btnSubmit.Visible = false;
                        _btnSubmit.Enabled = false;
                        _btnReset.Visible = false;
                        _btnReset.Enabled = false;
                    }

                    var actions = Page.ActionDispatcher.ActionPanelActions();
                    if (actions != null)
                    {
                        foreach (var submitAction in actions.OfType<PERS.SubmitAction>())
                        {
                            submitAction.ServiceName = _ddlServiceTypeField.Data.ToString();
                        }
                    }

                    var IsToolPlan = Page.PortalContext.DataContract.GetValueByName<string>("WIPEqpSetup_IsToolPlan");

                    if (IsToolPlan != null && IsToolPlan.ToString() == "true")
                    {
                        _tgcToolPlanFieldCtl.DefaultState = CollapsableState.Expanded;
                    }
                }

                // Set service name of the button dynamically.
                if (_btnSubmit != null)
                {
                    PERS.SubmitAction bSubmit = new PERS.SubmitAction();
                    bSubmit.ServiceName = _ddlServiceTypeField.Data.ToString();
                    bSubmit.Location = PERS.ActionLocation.Button;
                    _btnSubmit.DefaultAction = bSubmit;
                }

                if (!Page.IsPostBack)
                {
                    _txtComputerNameField.Data = SEMI.AppCode.UIUtility.GetComputerName(this);

                    Personalization.UIAction[] actUIActions = Page.ActionDispatcher.PageActions();
                    foreach (Personalization.UIAction act in actUIActions)
                    {
                        if (act.Name.ToUpper() == "SUBMIT")
                            act.ServiceName = _ddlServiceTypeField.Data.ToString();

                        if (act.Name.ToUpper() == "RESET")
                            if (_bIsPopup)
                                act.IsHidden = true;

                        if (act.Name.ToUpper() == "CLOSE")
                            if (!_bIsPopup)
                                act.IsHidden = true;
                    }

                    //hide the label used for the tooltip in the grid
                    CWC.Label HiddenToolTipLabel = Page.FindCamstarControl("HiddenToolTipLabel") as CWC.Label;
                    if (HiddenToolTipLabel != null)
                        HiddenToolTipLabel.Style["display"] = "none";

                    SEMI.AppCode.UIUtility.MaximizePopUp(this);
                }
                if (Page.PrimaryServiceType.Equals("CarrierSetup"))
                {
                    _ndoMachineGroupField.Enabled = false;
                    _ndoMachineGroupField.FieldExpressions = "";
                    _ndoMachineGroupField.DataSubmissionMode = DataSubmissionModeType.Skip;

                    _txtMaxLotsField.Hidden = true;
                    _txtMaxLotsField.FieldExpressions = "";
                    _txtMaxLotsField.DataSubmissionMode = DataSubmissionModeType.Skip;

                    _txtMaxUnitsField.Hidden = true;
                    _txtMaxUnitsField.FieldExpressions = "";
                    _txtMaxUnitsField.DataSubmissionMode = DataSubmissionModeType.Skip;

                    wpToolPlanWP.Hidden = true;
                    _tgcToolPlanFieldCtl.Visible = false;
                    _txtScanToolsField.Visible = false;
                    _gridToolsDetailsFields.Visible = false;
                    _gridParamsDetailsFields.Visible = false;
                    _tgcReferenceFieldCtl.Visible = false;

                    _ddlMultiLotsFlag.Hidden = true;
                    _ddlMultiLotsFlag.FieldExpressions = "";
                    _ddlMultiLotsFlag.DataSubmissionMode = DataSubmissionModeType.Skip;

                    _rdoRecipeField.Hidden = true;
                    _rdoRecipeField.FieldExpressions = "";
                    _rdoRecipeField.DataSubmissionMode = DataSubmissionModeType.Skip;

                    _ndoToolPlanField.Hidden = true;
                    _ndoToolPlanField.FieldExpressions = "";
                    _ndoToolPlanField.DataSubmissionMode = DataSubmissionModeType.Skip;

                    _ndoMaskField.Hidden = true;
                    _ndoMaskField.FieldExpressions = "";
                    _ndoMaskField.DataSubmissionMode = DataSubmissionModeType.Skip;

                    _ndoParentResourceField.Enabled = false;
                    _rdoProductField.Hidden = true;
                    _rdoProductField.FieldExpressions = "";
                    _rdoProductField.DataSubmissionMode = DataSubmissionModeType.Skip;

                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected override void OnLoadPersonalization()
        {
            base.OnLoadPersonalization();
            if (!Page.IsPostBack)
            {
                //--------additional code required for the resource layout to run proper--------------
                if (Page.IsAJAXFloatingFrame)
                    if (Page.DataContract.DataMembers != null)
                        if (Page.DataContract.GetValueByName("WIPEquipmentSetup_ServiceTypeDM") != null)
                            Page.PrimaryServiceType = (Page.PortalContext.DataContract.GetValueByName("WIPEquipmentSetup_ServiceTypeDM").ToString());
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void ToolPlanField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (_bIsExecute)
                {
                    // get the session and user profile
                    var fs = FrameworkManagerUtil.GetFrameworkSession();

                    string ServiceTypeRequired = _ddlServiceTypeField.Data.ToString();
                    ResultStatus oServiceResult = new ResultStatus(null, false);
                    var oServiceData = WCFObject.CreateObject(ServiceTypeRequired) as ICreator;
                    var oServiceRequest = WCFObject.CreateObject(ServiceTypeRequired + "_Request") as ICreator;
                    var oServiceInfo = WCFObject.CreateObject(ServiceTypeRequired + "_Info") as ICreator;
                    Result oResponseData = null;
                    var oService = new WSDataCreator().CreateService(ServiceTypeRequired, fs.CurrentUserProfile);
                    bool bRequestToolPlan = false;

                    _gridToolPlanDetailsFields.ClearData();
                    CamstarWebControl.SetRenderToClient(_gridToolPlanDetailsFields);
                    _txtToolPlanDesField.TextControl.Text = "";
                    bRequestToolPlan = true;

                    oServiceData.SetValue("Resource", _ndoEquipmentField.Data as NamedObjectRef);

                    if (_ctlContainerField != null)
                    {
                        if (_ctlContainerField.Data != null)
                        {
                            oServiceData.SetValue("ToolPlanLot", _ctlContainerField.Data as ContainerRef);
                        }
                    }

                    oServiceData.SetValue("ToolPlan", _ndoToolPlanField.Data as NamedObjectRef);

                    // check allow tools when changing new tool plan
                    if (isResourceChange == false)
                        CheckAllowTools((NamedObjectRef)_ndoToolPlanField.Data);
                    if (bRequestToolPlan)
                    {
                        if (_ndoProcessTypeField.Data != null)
                        { oServiceData.SetValue("ToolPlanProcessType", _ndoProcessTypeField.Data as NamedObjectRef); }
                        else
                        {
                            _ndoProcessTypeField.TextEditControl.Text = "NORMAL";
                            oServiceData.SetValue("ToolPlanProcessType", _ndoProcessTypeField.Data as NamedObjectRef);
                        }

                        oServiceInfo.SetValue("ToolPlanName", new OM.Info(true));
                        oServiceInfo.SetValue("ToolPlanDescription", new OM.Info(true));
                        oServiceInfo.SetValue("ToolPlanDetails", new OM.EquipmentSetupToolPlanItem_Info());
                        oServiceInfo.SetValue("ToolPlanDetails.ItemName", new OM.Info(true));
                        oServiceInfo.SetValue("ToolPlanDetails.ItemComments", new OM.Info(true));
                        oServiceInfo.SetValue("ToolPlanDetails.Detail", new OM.Info(true));
                        oServiceInfo.SetValue("ToolPlanDetails.DisplayName", new OM.Info(true));
                        oServiceInfo.SetValue("ToolPlanDetails.ToolFamilyQty", new OM.Info(true));
                    }
                    oServiceRequest.SetValue("Info", oServiceInfo);

                    // Request the data
                    ResultStatus oResultStatus = oService.GetEnvironment(oServiceData as DCObject, oServiceRequest as Request, out oResponseData);
                    if (oResultStatus.IsSuccess)
                    {

                        if (bRequestToolPlan)
                        {
                            _txtToolPlanNameField.Data = (oResponseData as ICreator).GetValue("Value.ToolPlanName");
                            _txtToolPlanDesField.Data = (oResponseData as ICreator).GetValue("Value.ToolPlanDescription");

                            EquipmentSetupToolPlanItem[] objToolPlanDetails = ((oResponseData as ICreator).GetValue("Value.ToolPlanDetails") as EquipmentSetupToolPlanItem[]);

                            // bind the results to the grid for ToolPlan
                            if (objToolPlanDetails != null && _ndoToolPlanField.Data != null)
                            {
                                (_gridToolPlanDetailsFields.GridContext as BoundContext).Data = objToolPlanDetails.ToArray();
                                _gridToolPlanDetailsFields.BoundContext.LoadData();
                            }
                            else
                            {
                                _gridToolPlanDetailsFields.ClearData();
                                _txtToolPlanDesField.ClearData();
                            }
                            CamstarWebControl.SetRenderToClient(_gridToolPlanDetailsFields);
                        }
                    }
                    else
                        throw new Exception("The Tool Plan provided is not a valid Tool Plan");
                }

            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public bool CheckAllowTools(NamedObjectRef newToolPlanName)
        {
            try
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new Camstar.WCF.Services.EquipmentSetupService(session.CurrentUserProfile);
                var serviceData = new OM.EquipmentSetup();

                var request = new Camstar.WCF.Services.EquipmentSetup_Request();
                var result = new Camstar.WCF.Services.EquipmentSetup_Result();

                request.Info = new OM.EquipmentSetup_Info { ss_AllowTools = FieldInfoUtil.RequestValue() };

                serviceData.ToolPlan = newToolPlanName;

                int toolCount = _gridToolsDetailsFields.TotalRowCount;
                serviceData.Tools = new NamedObjectRef[toolCount];
                for (int i = 0; i < toolCount; i++)
                {
                    serviceData.Tools[i] = new NamedObjectRef();
                    serviceData.Tools[i].Name = _gridToolsDetailsFields.GridContext.GetCell(i, "Name").ToString();
                }

                ResultStatus resultStatus = service.ss_EquipmentSetup_GetAllowTools(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    NamedObjectRef[] objToolsDetails = ((result as ICreator).GetValue("Value.ss_AllowTools") as NamedObjectRef[]);

                    // clear Tools grid
                    _gridToolsDetailsFields.ClearData();

                    // bind the results to the Tools grid
                    if (objToolsDetails != null)
                    {
                        (_gridToolsDetailsFields.GridContext as BoundContext).Data = objToolsDetails.ToArray();
                        _gridToolsDetailsFields.BoundContext.LoadData();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void ResourceField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (_bIsExecute)
                {
                    isResourceChange = true;
                    // get the session and user profile
                    var fs = FrameworkManagerUtil.GetFrameworkSession();

                    string ServiceTypeRequired = _ddlServiceTypeField.Data.ToString();
                    ResultStatus oServiceResult = new ResultStatus(null, false);
                    var oServiceData = WCFObject.CreateObject(ServiceTypeRequired) as ICreator;
                    var oServiceRequest = WCFObject.CreateObject(ServiceTypeRequired + "_Request") as ICreator;
                    var oServiceInfo = WCFObject.CreateObject(ServiceTypeRequired + "_Info") as ICreator;
                    Result oResponseData = null;
                    var oService = new WSDataCreator().CreateService(ServiceTypeRequired, fs.CurrentUserProfile);
                    bool bRequestToolPlan = false;
                    oServiceData.SetValue("Resource", _ndoEquipmentField.Data as NamedObjectRef);

                    if (_ctlContainerField != null)
                    {
                        if (_ctlContainerField.Data != null)
                        {
                            oServiceData.SetValue("ToolPlanLot", _ctlContainerField.Data as ContainerRef);
                            bRequestToolPlan = true;
                        }
                    }
                    oServiceInfo.SetValue("Comments", new OM.Info(true));
                    if (!Page.PrimaryServiceType.Equals("CarrierSetup"))
                    {
                        oServiceInfo.SetValue("Mask", new OM.Info(true));
                        oServiceInfo.SetValue("Recipe", new OM.Info(true));
                        oServiceInfo.SetValue("ToolPlan", new OM.Info(true));

                        oServiceInfo.SetValue("ToolsInUse", new OM.Info(true));
                        oServiceInfo.SetValue("ParamsSelection", new OM.EquipmentParamsDetails_Info());
                        oServiceInfo.SetValue("ParamsSelection.ParamName", new OM.Info(true));
                        oServiceInfo.SetValue("ParamsSelection.ParamObject", new OM.Info(true));
                        oServiceInfo.SetValue("ParamsSelection.ParamValue", new OM.Info(true));
                        if (bRequestToolPlan)
                        {
                            if (_ndoProcessTypeField.Data != null)
                            { oServiceData.SetValue("ToolPlanProcessType", _ndoProcessTypeField.Data as NamedObjectRef); }

                            oServiceInfo.SetValue("ToolPlanName", new OM.Info(true));
                            oServiceInfo.SetValue("ToolPlanDescription", new OM.Info(true));
                            oServiceInfo.SetValue("ToolPlanDetails", new OM.EquipmentSetupToolPlanItem_Info());
                            oServiceInfo.SetValue("ToolPlanDetails.ItemName", new OM.Info(true));
                            oServiceInfo.SetValue("ToolPlanDetails.ItemComments", new OM.Info(true));
                            oServiceInfo.SetValue("ToolPlanDetails.Detail", new OM.Info(true));
                            oServiceInfo.SetValue("ToolPlanDetails.DisplayName", new OM.Info(true));
                            oServiceInfo.SetValue("ToolPlanDetails.ToolFamilyQty", new OM.Info(true));
                        }
                    }

                    oServiceInfo.SetValue("ResourceChangeCount", new OM.Info(true));


                    oServiceRequest.SetValue("Info", oServiceInfo);

                    // Request the data
                    ResultStatus oResultStatus = oService.GetEnvironment(oServiceData as DCObject, oServiceRequest as Request, out oResponseData);
                    if (oResultStatus.IsSuccess)
                    {
                        _txtCommentsField.Data = (oResponseData as ICreator).GetValue("Value.Comments");
                        EquipmentParamsDetails[] objParamsDetails = ((oResponseData as ICreator).GetValue("Value.ParamsSelection") as EquipmentParamsDetails[]);
                        NamedObjectRef[] objToolsDetails = ((oResponseData as ICreator).GetValue("Value.ToolsInUse") as NamedObjectRef[]);

                        // bind the results to the grid for Params
                        if (objParamsDetails != null)
                        {
                            (_gridParamsDetailsFields.GridContext as BoundContext).Data = objParamsDetails.ToArray();
                            _gridParamsDetailsFields.BoundContext.LoadData();
                        }
                        else
                        {
                            _gridParamsDetailsFields.ClearData();
                        }

                        // bind the results to the grid for Tools
                        if (objToolsDetails != null)
                        {
                            (_gridToolsDetailsFields.GridContext as BoundContext).Data = objToolsDetails.ToArray();
                            _gridToolsDetailsFields.BoundContext.LoadData();
                        }
                        else
                        {
                            _gridToolsDetailsFields.ClearData();
                        }

                        if (bRequestToolPlan)
                        {
                            _txtToolPlanNameField.Data = (oResponseData as ICreator).GetValue("Value.ToolPlanName");
                            _txtToolPlanDesField.Data = (oResponseData as ICreator).GetValue("Value.ToolPlanDescription");

                            EquipmentSetupToolPlanItem[] objToolPlanDetails = ((oResponseData as ICreator).GetValue("Value.ToolPlanDetails") as EquipmentSetupToolPlanItem[]);

                            // bind the results to the grid for ToolPlan
                            if (objToolPlanDetails != null)
                            {
                                (_gridToolPlanDetailsFields.GridContext as BoundContext).Data = objToolPlanDetails.ToArray();
                                _gridToolPlanDetailsFields.BoundContext.LoadData();
                            }
                            else
                            {
                                _gridToolPlanDetailsFields.ClearData();
                            }
                            CamstarWebControl.SetRenderToClient(_gridToolPlanDetailsFields);
                        }

                        _ndoMaskField.Data = (oResponseData as ICreator).GetValue("Value.Mask");
                        _rdoRecipeField.Data = (oResponseData as ICreator).GetValue("Value.Recipe");
                        _ndoToolPlanField.Data = (oResponseData as ICreator).GetValue("Value.ToolPlan");
                        _txtResourceChangeCount.Data = (oResponseData as ICreator).GetValue("Value.ResourceChangeCount");

                        CamstarWebControl.SetRenderToClient(_rdoRecipeField);
                        CamstarWebControl.SetRenderToClient(_gridParamsDetailsFields);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void ReferenceLotField_DataChanged(string fieldName)
        {
            try
            {

                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                string ServiceTypeRequired = _ddlServiceTypeField.Data.ToString();
                ResultStatus iServiceResult = new ResultStatus(null, false);
                var iServiceData = WCFObject.CreateObject(ServiceTypeRequired) as ICreator;
                var iServiceRequest = WCFObject.CreateObject(ServiceTypeRequired + "_Request") as ICreator;
                var iServiceInfo = WCFObject.CreateObject(ServiceTypeRequired + "_Info") as ICreator;
                Result iResponseData = null;
                var iService = new WSDataCreator().CreateService(ServiceTypeRequired, fs.CurrentUserProfile);

                _btnCopyReference.Enabled = false;
                bool passFlag = true;

                if (_txtReferenceLotField.Data != "" && _txtReferenceLotField.Data != null)
                {
                    iServiceData.SetValue("ReferenceLot", new ContainerRef(_txtReferenceLotField.TextControl.Text));
                    switch (fieldName)
                    {
                        case "ReferenceLot":
                            ClearValues("All");
                            iServiceInfo.SetValue("ReferenceLotStepName", new Info(true, true));
                            break;
                        case "ReferenceLotStepName":
                            ClearValues(fieldName);
                            if (_ndoLotStepNameField.Data != null)
                            {
                                _btnCopyReference.Enabled = true;
                                iServiceData.SetValue("ReferenceLotStepName", _ndoLotStepNameField.Data.ToString());
                                iServiceInfo.SetValue("ReferenceLotProcessType", new Info(true, true));
                            }
                            else
                                passFlag = false;
                            break;
                        case "ReferenceLotProcessType":
                            ClearValues(fieldName);
                            if (_ndoLotStepNameField.Data != null && _ndoLotProcessTypeField.Data != null)
                            {
                                _btnCopyReference.Enabled = true;
                                iServiceData.SetValue("ReferenceLotStepName", _ndoLotStepNameField.Data.ToString());
                                iServiceData.SetValue("ReferenceLotProcessType", _ndoLotProcessTypeField.Data as NamedObjectRef);
                                iServiceInfo.SetValue("ReferenceLotMask", new Info(true, true));
                            }
                            else
                                passFlag = false;
                            break;
                    }

                    if (passFlag == true)
                    {
                        iServiceRequest.SetValue("Info", iServiceInfo);
                        ResultStatus iResultStatus = iService.GetEnvironment(iServiceData as DCObject, iServiceRequest as Request, out iResponseData);
                        if (iResultStatus.IsSuccess)
                        {
                            RecordSet objRefName = new RecordSet();
                            switch (fieldName)
                            {
                                case "ReferenceLot":
                                    switch (ServiceTypeRequired)
                                    {
                                        case "EquipmentSetup":
                                            objRefName = (iResponseData as EquipmentSetup_Result).Environment.ReferenceLotStepName.SelectionValues;
                                            break;
                                        case "WaferEquipmentSetup":
                                            objRefName = (iResponseData as WaferEquipmentSetup_Result).Environment.ReferenceLotStepName.SelectionValues;
                                            break;
                                        case "BackGrindEquipmentSetup":
                                            objRefName = (iResponseData as BackGrindEquipmentSetup_Result).Environment.ReferenceLotStepName.SelectionValues;
                                            break;
                                        case "AssemblyEquipmentSetup":
                                            objRefName = (iResponseData as AssemblyEquipmentSetup_Result).Environment.ReferenceLotStepName.SelectionValues;
                                            break;
                                        case "TestEquipmentSetup":
                                            objRefName = (iResponseData as TestEquipmentSetup_Result).Environment.ReferenceLotStepName.SelectionValues;
                                            break;
                                        case "MaskSetup":
                                            objRefName = (iResponseData as MaskSetup_Result).Environment.ReferenceLotStepName.SelectionValues;
                                            break;
                                        case "ToolSetup":
                                            objRefName = (iResponseData as ToolSetup_Result).Environment.ReferenceLotStepName.SelectionValues;
                                            break;
                                        case "CarrierSetup":
                                            objRefName = (iResponseData as CarrierSetup_Result).Environment.ReferenceLotStepName.SelectionValues;
                                            break;
                                    }
                                    break;
                                case "ReferenceLotStepName":
                                    switch (ServiceTypeRequired)
                                    {
                                        case "EquipmentSetup":
                                            objRefName = (iResponseData as EquipmentSetup_Result).Environment.ReferenceLotProcessType.SelectionValues;
                                            break;
                                        case "WaferEquipmentSetup":
                                            objRefName = (iResponseData as WaferEquipmentSetup_Result).Environment.ReferenceLotProcessType.SelectionValues;
                                            break;
                                        case "BackGrindEquipmentSetup":
                                            objRefName = (iResponseData as BackGrindEquipmentSetup_Result).Environment.ReferenceLotProcessType.SelectionValues;
                                            break;
                                        case "AssemblyEquipmentSetup":
                                            objRefName = (iResponseData as AssemblyEquipmentSetup_Result).Environment.ReferenceLotProcessType.SelectionValues;
                                            break;
                                        case "TestEquipmentSetup":
                                            objRefName = (iResponseData as TestEquipmentSetup_Result).Environment.ReferenceLotProcessType.SelectionValues;
                                            break;
                                        case "MaskSetup":
                                            objRefName = (iResponseData as MaskSetup_Result).Environment.ReferenceLotProcessType.SelectionValues;
                                            break;
                                        case "ToolSetup":
                                            objRefName = (iResponseData as ToolSetup_Result).Environment.ReferenceLotProcessType.SelectionValues;
                                            break;
                                        case "CarrierSetup":
                                            objRefName = (iResponseData as CarrierSetup_Result).Environment.ReferenceLotProcessType.SelectionValues;
                                            break;
                                    }
                                    break;
                                case "ReferenceLotProcessType":
                                    switch (ServiceTypeRequired)
                                    {
                                        case "EquipmentSetup":
                                            objRefName = (iResponseData as EquipmentSetup_Result).Environment.ReferenceLotMask.SelectionValues;
                                            break;
                                        case "WaferEquipmentSetup":
                                            objRefName = (iResponseData as WaferEquipmentSetup_Result).Environment.ReferenceLotMask.SelectionValues;
                                            break;
                                        case "BackGrindEquipmentSetup":
                                            objRefName = (iResponseData as BackGrindEquipmentSetup_Result).Environment.ReferenceLotMask.SelectionValues;
                                            break;
                                        case "AssemblyEquipmentSetup":
                                            objRefName = (iResponseData as AssemblyEquipmentSetup_Result).Environment.ReferenceLotMask.SelectionValues;
                                            break;
                                        case "TestEquipmentSetup":
                                            objRefName = (iResponseData as TestEquipmentSetup_Result).Environment.ReferenceLotMask.SelectionValues;
                                            break;
                                        case "MaskSetup":
                                            objRefName = (iResponseData as MaskSetup_Result).Environment.ReferenceLotMask.SelectionValues;
                                            break;
                                        case "ToolSetup":
                                            objRefName = (iResponseData as ToolSetup_Result).Environment.ReferenceLotMask.SelectionValues;
                                            break;
                                        case "CarrierSetup":
                                            objRefName = (iResponseData as CarrierSetup_Result).Environment.ReferenceLotMask.SelectionValues;
                                            break;
                                    }
                                    break;
                            }
                            // porpulate dropdown for Reference Lot Step Name
                            if (objRefName != null)
                            {
                                NamedObjectRef[] objRefList = new NamedObjectRef[objRefName.Rows.Length];
                                for (int lengthRef = 0; lengthRef < objRefName.Rows.Length; lengthRef++)
                                {
                                    objRefList[lengthRef] = new NamedObjectRef();
                                    objRefList[lengthRef].Name = objRefName.Rows[lengthRef].Values[0].ToString();
                                }
                                switch (fieldName)
                                {
                                    case "ReferenceLot":
                                        var LotStep = _ndoLotStepNameField;
                                        SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref LotStep, objRefList);
                                        break;
                                    case "ReferenceLotStepName":
                                        var LotProcess = _ndoLotProcessTypeField;
                                        SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref LotProcess, objRefList);
                                        break;
                                    case "ReferenceLotProcessType":
                                        var LotMask = _ndoLotMaskField;
                                        SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref LotMask, objRefList);
                                        break;
                                }
                            }
                        }
                    }
                }
                else
                    ClearValues("All");

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void ReferenceLotMaskField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (_ndoLotMaskField.Data != null && _ndoLotMaskField.Data.ToString() != "")
                    _ndoMaskField.Data = _ndoLotMaskField.Data.ToString();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void ProcessTypeLotField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (_bIsExecute)
                {
                    var fs = FrameworkManagerUtil.GetFrameworkSession();

                    string ServiceTypeRequired = _ddlServiceTypeField.Data.ToString();
                    ResultStatus pServiceResult = new ResultStatus(null, false);
                    var pServiceData = WCFObject.CreateObject(ServiceTypeRequired) as ICreator;
                    var pServiceRequest = WCFObject.CreateObject(ServiceTypeRequired + "_Request") as ICreator;
                    var pServiceInfo = WCFObject.CreateObject(ServiceTypeRequired + "_Info") as ICreator;
                    Result pResponseData = null;
                    var pService = new WSDataCreator().CreateService(ServiceTypeRequired, fs.CurrentUserProfile);

                    pServiceData.SetValue("Resource", _ndoEquipmentField.Data as NamedObjectRef);
                    pServiceData.SetValue("ToolPlanLot", _ctlContainerField.Data as ContainerRef);
                    pServiceData.SetValue("ToolPlanProcessType", _ndoProcessTypeField.Data as NamedObjectRef);
                    pServiceInfo.SetValue("ToolPlanName", new OM.Info(true));
                    pServiceInfo.SetValue("ToolPlanDescription", new OM.Info(true));
                    pServiceInfo.SetValue("ToolPlanDetails", new OM.EquipmentSetupToolPlanItem_Info());
                    pServiceInfo.SetValue("ToolPlanDetails.ItemName", new OM.Info(true));
                    pServiceInfo.SetValue("ToolPlanDetails.ItemComments", new OM.Info(true));
                    pServiceInfo.SetValue("ToolPlanDetails.Detail", new OM.Info(true));
                    pServiceInfo.SetValue("ToolPlanDetails.DisplayName", new OM.Info(true));
                    pServiceInfo.SetValue("ToolPlanDetails.ToolFamilyQty", new OM.Info(true));

                    pServiceRequest.SetValue("Info", pServiceInfo);

                    // Request the data
                    ResultStatus pResultStatus = pService.GetEnvironment(pServiceData as DCObject, pServiceRequest as Request, out pResponseData);
                    if (pResultStatus.IsSuccess)
                    {
                        _txtToolPlanNameField.Data = (pResponseData as ICreator).GetValue("Value.ToolPlanName");
                        _txtToolPlanDesField.Data = (pResponseData as ICreator).GetValue("Value.ToolPlanDescription");

                        EquipmentSetupToolPlanItem[] objToolPlanDetails = ((pResponseData as ICreator).GetValue("Value.ToolPlanDetails") as EquipmentSetupToolPlanItem[]);

                        // bind the results to the grid for ToolPlan
                        if (objToolPlanDetails != null)
                        {
                            (_gridToolPlanDetailsFields.GridContext as BoundContext).Data = objToolPlanDetails.ToArray();
                            _gridToolPlanDetailsFields.BoundContext.LoadData();
                        }
                        CamstarWebControl.SetRenderToClient(_gridToolPlanDetailsFields);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void DocumentSetField_DataChanged(object sender, EventArgs e)
        {
            if (_ndoDocumentSetField.Data != null)
                _btnDocumentSet.Enabled = true;
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void ToolTextControl_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (_txtScanToolsField.Data != "" && _txtScanToolsField.Data != null)
                {
                    // set grid details to list
                    NamedObjectRef[] objTools = (_gridToolsDetailsFields.GridContext as BoundContext).Data as NamedObjectRef[];
                    List<NamedObjectRef> objTool = new List<NamedObjectRef>();

                    int selectedIndex = 0;
                    if (objTools != null)
                    {
                        objTool = objTools.OfType<NamedObjectRef>().ToList();

                        // exit if entry already exist
                        foreach (NamedObjectRef oTool in objTool)
                        {
                            if (oTool.Name == _txtScanToolsField.Data.ToString())
                            {
                                _txtScanToolsField.TextChanged -= ToolTextControl_TextChanged;
                                _txtScanToolsField.Data = "";
                                return;
                            }
                            selectedIndex += 1;
                        }
                    }

                    // add new tool to the Tool grid
                    NamedObjectRef objNewRow = new NamedObjectRef();
                    objNewRow.Name = _txtScanToolsField.Data.ToString();
                    objTool.Insert(selectedIndex, objNewRow);
                    (_gridToolsDetailsFields.GridContext as BoundContext).Data = objTool.ToArray();
                    _gridToolsDetailsFields.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(_gridToolsDetailsFields);

                    // set scan tools field to empty
                    _txtScanToolsField.TextChanged -= ToolTextControl_TextChanged;
                    _txtScanToolsField.Data = "";
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void CopyReferenceLotButton_Click(object sender, EventArgs e)
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                string ServiceTypeRequired = _ddlServiceTypeField.Data.ToString();
                ResultStatus bServiceResult = new ResultStatus(null, false);
                var bServiceData = WCFObject.CreateObject(ServiceTypeRequired) as ICreator;
                var bServiceRequest = WCFObject.CreateObject(ServiceTypeRequired + "_Request") as ICreator;
                var bServiceInfo = WCFObject.CreateObject(ServiceTypeRequired + "_Info") as ICreator;
                Result bResponseData = null;
                var bService = new WSDataCreator().CreateService(ServiceTypeRequired, fs.CurrentUserProfile);

                bServiceData.SetValue("Resource", _ndoEquipmentField.Data as NamedObjectRef);
                bServiceData.SetValue("ReferenceLot", new ContainerRef(_txtReferenceLotField.TextControl.Text));
                bServiceData.SetValue("ReferenceLotStepName", _ndoLotStepNameField.Data.ToString());
                bServiceData.SetValue("ReferenceLotProcessType", _ndoLotProcessTypeField.Data as NamedObjectRef);
                bServiceInfo.SetValue("ReferenceLotParams", new OM.EquipmentParamsDetails_Info());
                bServiceInfo.SetValue("ReferenceLotParams.ParamName", new OM.Info(true));
                bServiceInfo.SetValue("ReferenceLotParams.ParamObject", new OM.Info(true));
                bServiceInfo.SetValue("ReferenceLotParams.ParamValue", new OM.Info(true));

                bServiceRequest.SetValue("Info", bServiceInfo);

                // Request the data
                ResultStatus bResultStatus = bService.GetEnvironment(bServiceData as DCObject, bServiceRequest as Request, out bResponseData);

                if (bResultStatus.IsSuccess)
                {
                    EquipmentParamsDetails[] objCpyParamsDetails = ((bResponseData as ICreator).GetValue("Value.ReferenceLotParams") as EquipmentParamsDetails[]);

                    // bind the results to the grid for Params
                    if (objCpyParamsDetails != null)
                    {
                        (_gridParamsDetailsFields.GridContext as BoundContext).Data = objCpyParamsDetails.ToArray();
                        _gridParamsDetailsFields.BoundContext.LoadData();
                    }
                    CamstarWebControl.SetRenderToClient(_gridParamsDetailsFields);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void ResetButton_Click(object sender, EventArgs e)
        {
            _gridParamsDetailsFields.ClearData();
            _gridToolsDetailsFields.ClearData();
            _txtReferenceLotField.ClearData();
            if (_ndoEmployeeField.Visible != true)
            {
                ClearValues("All");
                ResourceField_DataChanged(null, null);
            }
            else
                ClearValues("Resource");
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override void GetInputData(OM.Service serviceData)
        {
            try
            {

                base.GetInputData(serviceData);

                if (_bIsExecute)
                {
                    int gridParamsCount = _gridParamsDetailsFields.BoundContext.GetTotalRows();
                    if (gridParamsCount > 0)
                    {
                        EquipmentParamsDetails[] oParams = new EquipmentParamsDetails[gridParamsCount];
                        for (int i = 0; i < gridParamsCount; i++)
                        {
                            oParams[i] = new EquipmentParamsDetails();
                            oParams[i].ListItemAction = ListItemAction.Add;
                            oParams[i].ParamName = new NamedObjectRef();
                            oParams[i].ParamName.Name = _gridParamsDetailsFields.GridContext.GetCell(i, "ParamName").ToString();
                            oParams[i].ParamObject = _gridParamsDetailsFields.GridContext.GetCell(i, "ParamObject").ToString();
                            oParams[i].ParamValue = _gridParamsDetailsFields.GridContext.GetCell(i, "ParamValue").ToString();
                        } //End for loop

                        if (serviceData is OM.EquipmentSetup)
                        {
                            (serviceData as OM.EquipmentSetup).EqpParams = oParams;
                        }
                        else if (serviceData is OM.WaferEquipmentSetup)
                        {
                            (serviceData as OM.WaferEquipmentSetup).EqpParams = oParams;
                        }
                        else if (serviceData is OM.BackGrindEquipmentSetup)
                        {
                            (serviceData as OM.BackGrindEquipmentSetup).EqpParams = oParams;
                        }
                        else if (serviceData is OM.AssemblyEquipmentSetup)
                        {
                            (serviceData as OM.AssemblyEquipmentSetup).EqpParams = oParams;
                        }
                        else if (serviceData is OM.TestEquipmentSetup)
                        {
                            (serviceData as OM.TestEquipmentSetup).EqpParams = oParams;
                        }
                    }

                    int gridToolsCount = _gridToolsDetailsFields.BoundContext.GetTotalRows();
                    if (gridToolsCount > 0)
                    {
                        NamedObjectRef[] oTools = new NamedObjectRef[gridToolsCount];
                        for (int i = 0; i < gridToolsCount; i++)
                        {
                            oTools[i] = new NamedObjectRef();
                            oTools[i].ListItemAction = ListItemAction.Add;
                            oTools[i].Name = _gridToolsDetailsFields.GridContext.GetCell(i, "Name").ToString();
                        } //End for loop

                        if (serviceData is OM.EquipmentSetup)
                        {
                            (serviceData as OM.EquipmentSetup).Tools = oTools;
                        }
                        else if (serviceData is OM.WaferEquipmentSetup)
                        {
                            (serviceData as OM.WaferEquipmentSetup).Tools = oTools;
                        }
                        else if (serviceData is OM.BackGrindEquipmentSetup)
                        {
                            (serviceData as OM.BackGrindEquipmentSetup).Tools = oTools;
                        }
                        else if (serviceData is OM.AssemblyEquipmentSetup)
                        {
                            (serviceData as OM.AssemblyEquipmentSetup).Tools = oTools;
                        }
                        else if (serviceData is OM.TestEquipmentSetup)
                        {
                            (serviceData as OM.TestEquipmentSetup).Tools = oTools;
                        }
                    }

                    NamedObjectRef oMask = new NamedObjectRef();
                    RevisionedObjectRef oRecipe = new RevisionedObjectRef();
                    NamedObjectRef oToolPlan = new NamedObjectRef();
                    NamedObjectRef oEquipment = new NamedObjectRef();
                    int iChangeCount = 0;
                    if (_txtResourceChangeCount.Data != null)
                    {
                        try { iChangeCount = int.Parse(_txtResourceChangeCount.Data.ToString()); }
                        catch { iChangeCount = 0; }
                    }

                    if (_ndoMaskField.Data != null)
                        oMask = _ndoMaskField.Data as NamedObjectRef;
                    else
                        oMask.Name = "";

                    if (_rdoRecipeField.Data != null)
                        oRecipe = _rdoRecipeField.Data as RevisionedObjectRef;
                    else
                    {
                        oRecipe.Name = "";
                        oRecipe.Revision = "";
                        oRecipe.RevisionOfRecord = false;
                    }

                    if (_ndoToolPlanField.Data != null)
                        oToolPlan = _ndoToolPlanField.Data as NamedObjectRef;
                    else
                        oToolPlan.Name = "";

                    if (_ndoEquipmentField.Data != null)
                        oEquipment = _ndoEquipmentField.Data as NamedObjectRef;

                    // manually collect all the other fields
                    if (serviceData is OM.EquipmentSetup)
                    {
                        if (!(serviceData is OM.CarrierSetup))
                        {
                            if (_ndoMachineGroupField != null)
                                if (_ndoMachineGroupField.Data != null)
                                    (serviceData as OM.EquipmentSetup).MachineGroup = _ndoMachineGroupField.Data as NamedObjectRef;
                                else
                                {
                                    (serviceData as OM.EquipmentSetup).MachineGroup = new NamedObjectRef();
                                    (serviceData as OM.EquipmentSetup).MachineGroup.Name = "";
                                }
                            (serviceData as OM.EquipmentSetup).Mask = oMask;
                            if (_txtMaxLotsField != null)
                            {
                                if (_txtMaxLotsField.Data != null)
                                    (serviceData as OM.EquipmentSetup).MaxLots = _txtMaxLotsField.Data as Primitive<int>;
                                else
                                {
                                    (serviceData as OM.EquipmentSetup).MaxLots = null;
                                }
                            }
                            if (_txtMaxUnitsField != null)
                            {
                                if (_txtMaxUnitsField.Data != null)
                                    (serviceData as OM.EquipmentSetup).MaxUnits = _txtMaxUnitsField.Data as Primitive<double>;
                                else
                                    (serviceData as OM.EquipmentSetup).MaxUnits = null;
                            }
                            if (_ddlMultiLotsFlag != null)
                            {
                                if (_ddlMultiLotsFlag.Data != null)
                                    (serviceData as OM.EquipmentSetup).MultiLotsFlag = _ddlMultiLotsFlag.Data as Primitive<int>;
                                else
                                    (serviceData as OM.EquipmentSetup).MultiLotsFlag = null;
                            }

                            (serviceData as OM.EquipmentSetup).Recipe = oRecipe;
                            (serviceData as OM.EquipmentSetup).ToolPlan = oToolPlan;
                        }
                        if (_ndoMfgLineField != null)
                            if (_ndoMfgLineField.Data != null)
                                (serviceData as OM.EquipmentSetup).ss_MfgLine = _ndoMfgLineField.Data as NamedObjectRef;
                            else
                            {
                                (serviceData as OM.EquipmentSetup).ss_MfgLine = new NamedObjectRef();
                                (serviceData as OM.EquipmentSetup).ss_MfgLine.Name = "";
                            }
                        (serviceData as OM.EquipmentSetup).Resource = oEquipment;
                        (serviceData as OM.EquipmentSetup).ResourceChangeCount = iChangeCount;
                        if (_ndoEmployeeField != null)
                        {
                            if (_ndoEmployeeField.Data != null)
                                (serviceData as OM.EquipmentSetup).Employee = _ndoEmployeeField.Data as NamedObjectRef;
                            else
                            {
                                (serviceData as OM.EquipmentSetup).Employee = new NamedObjectRef();
                                (serviceData as OM.EquipmentSetup).Employee.Name = "";
                            }
                        }
                        if (_ndoDocumentSetField != null)
                        {
                            if (_ndoDocumentSetField.Data != null)
                                (serviceData as OM.EquipmentSetup).DocumentSet = _ndoDocumentSetField.Data as NamedObjectRef;
                            else
                            {
                                (serviceData as OM.EquipmentSetup).DocumentSet = new NamedObjectRef();
                                (serviceData as OM.EquipmentSetup).DocumentSet.Name = "";
                            }
                        }
                        if (_ndoParentResourceField != null)
                        {
                            if (_ndoParentResourceField.Data != null)
                                (serviceData as OM.EquipmentSetup).ParentResource = _ndoParentResourceField.Data as NamedObjectRef;
                            else
                            {
                                (serviceData as OM.EquipmentSetup).ParentResource = new NamedObjectRef();
                                (serviceData as OM.EquipmentSetup).ParentResource.Name = "";
                            }
                        }

                        if (_ndoPackageGroupField != null)
                        {
                            if (_ndoPackageGroupField.Data != null)
                                (serviceData as OM.EquipmentSetup).PackageGroup = _ndoPackageGroupField.Data as NamedObjectRef;
                            else
                            {
                                (serviceData as OM.EquipmentSetup).PackageGroup = new NamedObjectRef();
                                (serviceData as OM.EquipmentSetup).PackageGroup.Name = "";
                            }
                        }


                        if (_ndoPhysicalLocationField != null)
                        {
                            if (_ndoPhysicalLocationField.Data != null)
                                (serviceData as OM.EquipmentSetup).PhysicalLocation = _ndoPhysicalLocationField.Data as NamedObjectRef;
                            else
                            {
                                (serviceData as OM.EquipmentSetup).PhysicalLocation = new NamedObjectRef();
                                (serviceData as OM.EquipmentSetup).PhysicalLocation.Name = "";
                            }
                        }
                        if (_ndoPhysicalPositionField != null)
                        {
                            if (_ndoPhysicalPositionField.Data != null)
                                (serviceData as OM.EquipmentSetup).PhysicalPosition = _ndoPhysicalPositionField.Data as NamedObjectRef;
                            else
                            {
                                (serviceData as OM.EquipmentSetup).PhysicalPosition = new NamedObjectRef();
                                (serviceData as OM.EquipmentSetup).PhysicalPosition.Name = "";
                            }
                        }
                        if (_rdoProductField != null)
                        {
                            if (_rdoProductField.Data != null)
                                (serviceData as OM.EquipmentSetup).Product = _rdoProductField.Data as RevisionedObjectRef;
                            else
                            {
                                (serviceData as OM.EquipmentSetup).Product = new RevisionedObjectRef();
                                (serviceData as OM.EquipmentSetup).Product.Name = "";
                                (serviceData as OM.EquipmentSetup).Product.Revision = "";
                                (serviceData as OM.EquipmentSetup).Product.RevisionOfRecord = false;
                            }
                        }
                    }

                    if (_txtReferenceLotField.Data != null)
                    {
                        ContainerRef oReferenceLot = new ContainerRef();
                        oReferenceLot.Name = _txtReferenceLotField.TextControl.Text;
                        if (serviceData is OM.EquipmentSetup)
                        {
                            (serviceData as OM.EquipmentSetup).ReferenceLot = oReferenceLot;
                        }
                        else if (serviceData is OM.WaferEquipmentSetup)
                        {
                            (serviceData as OM.WaferEquipmentSetup).ReferenceLot = oReferenceLot;
                        }
                        else if (serviceData is OM.TestEquipmentSetup)
                        {
                            (serviceData as OM.TestEquipmentSetup).ReferenceLot = oReferenceLot;
                        }
                        else if (serviceData is OM.AssemblyEquipmentSetup)
                        {
                            (serviceData as OM.AssemblyEquipmentSetup).ReferenceLot = oReferenceLot;
                        }
                        else if (serviceData is OM.BackGrindEquipmentSetup)
                        {
                            (serviceData as OM.BackGrindEquipmentSetup).ReferenceLot = oReferenceLot;
                        }
                    }
                    //ResourceField_DataChanged(null, null);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void ClearValues(string passFieldName)
        {
            if (passFieldName == "ReferenceLotStepName" || passFieldName == "All")
            {
                if (passFieldName == "All")
                {
                    _ndoLotStepNameField.ClearData();
                    _ndoLotStepNameField.ClearSelectionValues();
                }
                _ndoLotProcessTypeField.ClearData();
                _ndoLotProcessTypeField.ClearSelectionValues();
            }
            _ndoLotMaskField.ClearData();
            _ndoLotMaskField.ClearSelectionValues();

            if (passFieldName == "Resource")
                Page.ClearDataValues();
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (serviceData is OM.EquipmentSetup || serviceData is OM.WaferSortEquipmentSetup || serviceData is OM.AssemblyEquipmentSetup || serviceData is OM.WaferEquipmentSetup || serviceData is TestEquipmentSetup || serviceData is BackGrindEquipmentSetup)
                if (status.IsSuccess)
                {

                    if (_ndoEmployeeField.Visible == true)
                    {
                        ClearValues("Resource");
                        _txtComputerNameField.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
                    }
                    else
                    {
                        if (_bIsExecute)
                            _txtResourceChangeCount.Data = CurrentChangeCount();
                    }
                }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public string CurrentChangeCount()
        {
            string sChangeCount = _txtResourceChangeCount.Data.ToString();

            // get the session and user profile
            var fs = FrameworkManagerUtil.GetFrameworkSession();

            string ServiceTypeRequired = _ddlServiceTypeField.Data.ToString();
            ResultStatus oServiceResult = new ResultStatus(null, false);
            var oServiceData = WCFObject.CreateObject(ServiceTypeRequired) as ICreator;
            var oServiceRequest = WCFObject.CreateObject(ServiceTypeRequired + "_Request") as ICreator;
            var oServiceInfo = WCFObject.CreateObject(ServiceTypeRequired + "_Info") as ICreator;
            Result oResponseData = null;
            var oService = new WSDataCreator().CreateService(ServiceTypeRequired, fs.CurrentUserProfile);
            oServiceData.SetValue("Resource", _ndoEquipmentField.Data as NamedObjectRef);
            oServiceInfo.SetValue("ResourceChangeCount", new OM.Info(true));
            oServiceRequest.SetValue("Info", oServiceInfo);

            // Request the data
            ResultStatus oResultStatus = oService.GetEnvironment(oServiceData as DCObject, oServiceRequest as Request, out oResponseData);
            if (oResultStatus.IsSuccess)
            {
                sChangeCount = (oResponseData as ICreator).GetValue("Value.ResourceChangeCount").ToString();
            }

            return sChangeCount;
        }
    }
}



