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
using Camstar.WebPortal.Personalization;
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
    /// Summary description for LotHoldDispositionTxn
    /// </summary>
    public class LotHoldDispositionTxn : scsShopfloorBase
    {
        #region Properties 
        // Hidden objects
        CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("ComputerName") as CWC.TextBox; } }
        CWC.TextBox _txtServiceTypeField { get { return Page.FindCamstarControl("ServiceType") as CWC.TextBox; } }
        CWC.NamedObject _ndoProcessTypeField { get { return Page.FindCamstarControl("LotHoldDisposition_ProcessType") as CWC.NamedObject; } }
        CWC.CheckBox _chkIsWaferProcessingField { get { return Page.FindCamstarControl("IsWaferProcessing") as CWC.CheckBox; } }
        CWC.TextBox _txtCurrentDispositionPlanField { get { return Page.FindCamstarControl("CurrentDispositionPlan") as CWC.TextBox; } }
        CWC.TextBox _txtScheduleDataObjectTypeField { get { return Page.FindCamstarControl("ScheduleDataObjectType") as CWC.TextBox; } }
        CWC.ContainerList _ContainerField { get { return Page.FindCamstarControl("HiddenSelectedContainer") as CWC.ContainerList; } }
        SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }

        CWC.TextBox _txtSelectionIdField { get { return Page.FindCamstarControl("SelectionId") as CWC.TextBox; } }
        JQDataGrid _gridLotInfoField { get { return Page.FindCamstarControl("LotInfoFieldGrid") as JQDataGrid; } }
        CWC.NamedObject _ndoReleaseReasonField { get { return Page.FindCamstarControl("ReleaseReason") as CWC.NamedObject; } }
        CWC.DropDownList _ddlDispositionPlanField { get { return Page.FindCamstarControl("DispositionPlan") as CWC.DropDownList; } }
        CWC.TextBox _txtCommentsField { get { return Page.FindCamstarControl("Comments") as CWC.TextBox; } }

        CWC.Button _btnFailures { get { return Page.FindCamstarControl("FailuresButton") as CWC.Button; } }
        CWC.Button _btnSupplementarySpec { get { return Page.FindCamstarControl("SupplementarySpecButton") as CWC.Button; } }
        CWC.Button _btnWIPData { get { return Page.FindCamstarControl("WIPDataButton") as CWC.Button; } }
        CWC.Button _btnItemRejects { get { return Page.FindCamstarControl("ItemRejectsButton") as CWC.Button; } }
        CWC.Button _btnLotRejects { get { return Page.FindCamstarControl("LotRejectsButton") as CWC.Button; } }
        CWC.Button _btnBins { get { return Page.FindCamstarControl("BinsButton") as CWC.Button; } }

        //Command bar action buttons
        CWC.Button _btnFailuresCmdBar { get { return Page.FindCamstarControl("WIPLotFailuresAction") as CWC.Button; } }
        CWC.Button _btnSupplementarySpecCmdBar { get { return Page.FindCamstarControl("LotScheduleModifyAction") as CWC.Button; } }
        CWC.Button _btnWIPDataCmdBar { get { return Page.FindCamstarControl("WIPDataPopupAction") as CWC.Button; } }
        CWC.Button _btnItemRejectsCmdBar { get { return Page.FindCamstarControl("ItemRejectsAction") as CWC.Button; } }
        CWC.Button _btnLotRejectsCmdBar { get { return Page.FindCamstarControl("LotRejectsAction") as CWC.Button; } }
        CWC.Button _btnBinsCmdBar { get { return Page.FindCamstarControl("LotBinsAction") as CWC.Button; } }

        // Rework Panel
        CWC.NamedObject _ndoReworkWP_ReworkReasonField { get { return Page.FindCamstarControl("ReworkWP_ReworkReason") as CWC.NamedObject; } }
        CWC.CheckBox _chkReworkWP_ReworkFullLotField { get { return Page.FindCamstarControl("ReworkWP_ReworkFullLot") as CWC.CheckBox; } }
        CWC.TextBox _txtReworkWP_ReworkQtyField { get { return Page.FindCamstarControl("ReworkWP_ReworkQty") as CWC.TextBox; } }
        CWC.CheckBox _chkReworkWP_YieldOffRejectsField { get { return Page.FindCamstarControl("ReworkWP_YieldOffRejects") as CWC.CheckBox; } }
        CWC.CheckBox _chkReworkWP_SplitBinsField { get { return Page.FindCamstarControl("ReworkWP_SplitBins") as CWC.CheckBox; } }
        CWC.DropDownList _ddlReworkWP_ReworkStepTypeField { get { return Page.FindCamstarControl("ReworkWP_ReworkStepType") as CWC.DropDownList; } }
        CWC.NamedSubentity _sndReworkWP_ReworkStepField { get { return Page.FindCamstarControl("ReworkWP_ReworkStep") as CWC.NamedSubentity; } }
        CWC.NamedSubentity _sndReworkWP_ReworkReEntryStepField { get { return Page.FindCamstarControl("ReworkWP_ReworkReEntryStep") as CWC.NamedSubentity; } }
        CWC.NamedSubentity _sndReworkWP_ReworkEndStepField { get { return Page.FindCamstarControl("ReworkWP_ReworkEndStep") as CWC.NamedSubentity; } }

        // Release Only Panel
        CWC.CheckBox _chkReleaseOnlyWP_ApplyToChildLotsField { get { return Page.FindCamstarControl("ReleaseOnlyWP_ApplyToChildLots") as CWC.CheckBox; } }
        CWC.CheckBox _chkReleaseOnlyWP_scsForceYieldCheckField { get { return Page.FindCamstarControl("ReleaseOnlyWP_scsForceYieldCheck") as CWC.CheckBox; } }
        // Yield Off Panel
        CWC.NamedObject _ndoYieldOffWP_YieldOffReasonField { get { return Page.FindCamstarControl("YieldOffWP_YieldOffReason") as CWC.NamedObject; } }

        // Proceed Panel
        CWC.CheckBox _chkProceedWP_YieldOffRejectsField { get { return Page.FindCamstarControl("ProceedWP_YieldOffRejects") as CWC.CheckBox; } }
        CWC.CheckBox _chkProceedWP_SplitBinsField { get { return Page.FindCamstarControl("ProceedWP_SplitBins") as CWC.CheckBox; } }
        CWC.CheckBox _chkProceedWP_ApplyToChildLotsField { get { return Page.FindCamstarControl("ProceedWP_ApplyToChildLots") as CWC.CheckBox; } }
        CWC.TextBox _txtProceedWP_ProceedNewLotIdField { get { return Page.FindCamstarControl("ProceedWP_ProceedNewLotId") as CWC.TextBox; } }
        CWC.NamedObject _ndoProceedWP_ProceedNewLotHoldReasonField { get { return Page.FindCamstarControl("ProceedWP_ProceedNewLotHoldReason") as CWC.NamedObject; } }

        // Split Rejects Panel
        CWC.TextBox _txtSplitRejectWP_SplitRejectLotIdField { get { return Page.FindCamstarControl("SplitRejectWP_SplitRejectLotId") as CWC.TextBox; } }
        CWC.TextBox _txtSplitRejectWP_SplitRejectGoodQtyField { get { return Page.FindCamstarControl("SplitRejectWP_SplitRejectGoodQty") as CWC.TextBox; } }
        CWC.CheckBox _chkSplitRejectWP_CreateNewScheduleField { get { return Page.FindCamstarControl("SplitRejectWP_CreateNewSchedule") as CWC.CheckBox; } }
        CWC.CheckBox _chkSplitRejectWP_AutoMoveOutField { get { return Page.FindCamstarControl("SplitRejectWP_AutoMoveOut") as CWC.CheckBox; } }
        CWC.CheckBox _chkSplitRejectWP_YieldOffRejectsField { get { return Page.FindCamstarControl("SplitRejectWP_YieldOffRejects") as CWC.CheckBox; } }
        CWC.CheckBox _chkSplitRejectWP_SplitBinsField { get { return Page.FindCamstarControl("SplitRejectWP_SplitBins") as CWC.CheckBox; } }

        // Rescreen Panel
        CWC.NamedObject _ndoRescreenWP_InsertionReasonField { get { return Page.FindCamstarControl("RescreenWP_InsertionReason") as CWC.NamedObject; } }
        CWC.CheckBox _chkRescreenWP_WaiveNextYieldCheckField { get { return Page.FindCamstarControl("RescreenWP_WaiveNextYieldCheck") as CWC.CheckBox; } }
        CWC.CheckBox _chkRescreenWP_UseCurrentInsertionDetailsField { get { return Page.FindCamstarControl("RescreenWP_UseCurrentInsertionDetails") as CWC.CheckBox; } }
        JQDataGrid _gridRescreenWP_InsertionDetailsField { get { return Page.FindCamstarControl("RescreenWP_InsertionDetails") as JQDataGrid; } }
        SEMI.AppCode.DataEnvelopControl _envRescreenWP_WIPDataValidValuesList { get { return Page.FindCamstarControl("RescreenWP_WIPDataValidValuesList") as SEMI.AppCode.DataEnvelopControl; } }
        CWC.TextBox _txtRescreenWP_WaferScribeNumberField { get { return Page.FindCamstarControl("RescreenWP_WaferScribeNumber") as CWC.TextBox; } }

        #endregion

        #region Constants
        const string const_sServiceType_LotHoldDisposition = "LotHoldDisposition";
        const string const_sServiceType_LotHoldDisposition1 = "LotHoldDisposition1";
        const string const_sServiceType_LotHoldDisposition2 = "LotHoldDisposition2";
        const string const_sSelectionId = "SELECTIONID";
        const string const_sDispositionPlan_ReleaseOnly = "RELEASEONLY";
        const string const_sDispositionPlan_Proceed = "PROCEED";
        const string const_sDispositionPlan_Rescreen = "RESCREEN";
        const string const_sDispositionPlan_Rework = "REWORK";
        const string const_sDispositionPlan_YieldOff = "YIELDOFF";
        const string const_sDispositionPlan_SplitReject = "SPLITREJECT";

        #endregion

        #region Functions

        public LotHoldDispositionTxn()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Page.CollectDataContract();
            string sServiceType = this.PrimaryServiceType;

            if (!Page.IsPostBack)
            {
                // Get Computer name
                _txtComputerNameField.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
                _txtServiceTypeField.Data = this.PrimaryServiceType;

                // Hide the classic buttons
                _btnFailures.Hidden = true;
                _btnSupplementarySpec.Hidden = true;
                _btnBins.Hidden = true;
                _btnItemRejects.Hidden = true;
                _btnLotRejects.Hidden = true;
                _btnWIPData.Hidden = true;

                // Hide the command bar buttons
                _btnFailuresCmdBar.Visible = false;
                _btnSupplementarySpecCmdBar.Visible = false;
                _btnBinsCmdBar.Visible = false;
                _btnItemRejectsCmdBar.Visible = false;
                _btnLotRejectsCmdBar.Visible = false;
                _btnWIPDataCmdBar.Visible = false;

                _ndoReleaseReasonField.ReadOnly = (sServiceType == const_sServiceType_LotHoldDisposition2);
                _ddlDispositionPlanField.ReadOnly = (sServiceType == const_sServiceType_LotHoldDisposition2);
                _txtCommentsField.ReadOnly = (sServiceType == const_sServiceType_LotHoldDisposition2);

                var lotInfoFieldGrid = (Page.FindCamstarControl("LotInfoFieldGrid")) as JQDataGrid;

                if (IsResponsive)
                {
                    if (lotInfoFieldGrid.Settings.Automation == null)
                        lotInfoFieldGrid.Settings.Automation = new GridAutomation();

                    lotInfoFieldGrid.Settings.Automation.ShrinkColumnWidthToFit = false;
                }

                SetPageActionServiceName();
            }
            else
            {
                CollectSelectedContainer();
                CollectSelectedValues();
            }
        }

        private void CollectSelectedContainer()
        {
            // Check if it is a pop up close, get the return result
            if (SEMI.AppCode.UIUtility.IsPopupClose(this))
            {
                if (_envSelectedLots != null)
                    // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                    if (Page.DataContract.GetValueByName("SelectedLotsListDM") != null)
                        _envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("SelectedLotsListDM") as string[];

                if (_envSelectedLots.SS_ContainersList != null)
                {
                    string[] sContainers;
                    sContainers = _envSelectedLots.SS_ContainersList;

                    // Set Selection Id textbox value
                    _txtSelectionIdField.Data = sContainers[0];

                    //nullify the containers list
                    _envSelectedLots.SS_ContainersList = null;

                    SelectionIdField_DataChanged(null, null);
                }
            }
            if (Page.EventTarget.Contains(_txtSelectionIdField.ID) && _txtSelectionIdField.IsChanged)
            {
                SelectionIdField_DataChanged(null, null);
            }
        }

        private void SetPageActionServiceName()
        {
            // Set service name of Page action
            var actSubmit = (Page as Camstar.WebPortal.FormsFramework.IActionContainer).ActionDispatcher.GetActionByName("SubmitAction");
            if (actSubmit != null)
                (actSubmit as Personalization.SubmitAction).ServiceName = this.PrimaryServiceType;
        }


        private void CollectSelectedValues()
        {
            if (SEMI.AppCode.UIUtility.IsPopupClose(this))
            {
                string sSelectedRowId = "";
                if (Page.DataContract.GetValueByName("RescreenWP_SelectedRowIdDM") != null)
                    sSelectedRowId = Page.DataContract.GetValueByName("RescreenWP_SelectedRowIdDM") as string;

                WIPLotTxnWafersDetails[] oWafers = null;
                //CreateInsertionDetailsQtys[] oQtys = null;
                string sValue = "";

                if (_chkIsWaferProcessingField.CheckControl.Checked)
                    oWafers = Page.DataContract.GetValueByName("RescreenWP_ReturnWafersDM") as WIPLotTxnWafersDetails[];
                else
                    sValue = Page.DataContract.GetValueByName("RescreenWP_ReturnValueDM") as string;

                if (!string.IsNullOrEmpty(sSelectedRowId))
                {
                    if (_chkIsWaferProcessingField.CheckControl.Checked)
                    {
                        if (oWafers != null)
                            _gridRescreenWP_InsertionDetailsField.GridContext.SetCell(sSelectedRowId, "WafersDetails", oWafers);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(sValue))
                            _gridRescreenWP_InsertionDetailsField.GridContext.SetCell(sSelectedRowId, "QtyToProcess", sValue);
                    }
                }
            } // End if
        }

        private void Populate_WIPDataValue(CreateInsertionDetails[] oInsertionDetailsData)
        {
            if (oInsertionDetailsData != null)
            {
                // Nullify the Valid Values list
                _envRescreenWP_WIPDataValidValuesList.SS_WIPDataValidValuesList = null;

                foreach (CreateInsertionDetails oInsertionDetail in oInsertionDetailsData)
                {
                    // Check whether lot is Wafer Processing or Qty Processing
                    if (_chkIsWaferProcessingField.CheckControl.Checked)
                    {
                        if (oInsertionDetail.WafersDetails != null)
                        {
                            List<string> sDefaultWafersList = new List<string>();

                            foreach (WIPLotTxnWafersDetails oWafer in oInsertionDetail.WafersDetails)
                            {
                                // For static list of wafers
                                sDefaultWafersList.Add(oWafer.WaferScribeNumber.Value.ToString());
                            }

                            _envRescreenWP_WIPDataValidValuesList.SS_WIPDataValidValuesList = sDefaultWafersList.ToArray();
                        }
                    }
                    else
                    {
                        if (oInsertionDetail.ValidQtys != null)
                        {
                            List<string> sValidQtysList = new List<string>();
                            bool bFirstValidQty = true;
                            foreach (CreateInsertionDetailsQtys oValidQty in oInsertionDetail.ValidQtys)
                            {
                                sValidQtysList.Add(oValidQty.QtyToProcess.Value.ToString());
                                if (bFirstValidQty)
                                {
                                    oInsertionDetail.QtyToProcess = oValidQty.QtyToProcess;
                                }
                                bFirstValidQty = false;
                            }

                            _envRescreenWP_WIPDataValidValuesList.SS_WIPDataValidValuesList = sValidQtysList.ToArray();
                        }
                    }// end If
                }//end for loop
            }
        }

        public void SelectionIdField_DataChanged(object sender, EventArgs e)
        {
            if (!_txtSelectionIdField.IsEmpty)
            {
                ResetFields();
                FetchData(const_sSelectionId);
                SetControls();
            }
        }

        public void DispositionPlanField_DataChanged(object sender, EventArgs e)
        {
            int iInsertionDetailsCount = _gridRescreenWP_InsertionDetailsField.BoundContext.GetTotalRows();
            string sDispositionPlan = _ddlDispositionPlanField.Data != null ? _ddlDispositionPlanField.Data.ToString() : "";

            if (sDispositionPlan == const_sDispositionPlan_Rescreen && iInsertionDetailsCount == 0)
                FetchData(const_sDispositionPlan_Rescreen);

            SetControls();
        }

        private void SetLotSelection(string sSelectionId)
        {
            JQDataGrid _gridLotInfoFieldx = Page.FindCamstarControl("LotInfoFieldGrid") as JQDataGrid;
            _gridLotInfoFieldx.ClearData();
            SEMI.AppCode.UIUtility.GetLotQuerySelection(this, this.PrimaryServiceType.ToString(), sSelectionId, true, ref _gridLotInfoFieldx, "LotInfoFieldGrid");
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            if (action != null && action.Parameters == "Clear")
            {
                Page.ClearValues();
                ResetFields();
                SetControls();
            }
        }

        private void FetchData(string sEventName)
        {
            try
            {
                // Get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                string sServiceType = this.PrimaryServiceType;

                // Run proper constructor and prepare 
                var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
                var svcConstructor = svcType.GetConstructor(new Type[] { typeof(Camstar.WCF.ObjectStack.UserProfile) });
                var svc = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

                // Service Data
                var data = CreateServiceData(sServiceType);
                var oServiceData = data as LotHoldDisposition;

                CreateInsertionDetails_Info oInsertionDetailsInfo = new CreateInsertionDetails_Info();
                CreateInsertionDetails[] oInsertionDetailsData = null;

                // Service Info
                var info = CreateServiceInfo(sServiceType);
                var oServiceInfo = info as LotHoldDisposition_Info;

                oServiceInfo.ProcessTypeSelection = FieldInfoUtil.RequestValue();

                string sSelectionId = _txtSelectionIdField.Data != null ? _txtSelectionIdField.Data.ToString() : "";
                // Prepare the request 
                if (sEventName == const_sSelectionId)
                {
                    oServiceData.SelectionId = sSelectionId;
                    oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
                    oServiceInfo.ScheduleDataObjectType = FieldInfoUtil.RequestValue();
                    oServiceInfo.DispositionPlanSelection = FieldInfoUtil.RequestValue();
                    oServiceInfo.DispositionPlan = FieldInfoUtil.RequestValue();
                    oServiceInfo.Comments = FieldInfoUtil.RequestValue();
                    oServiceInfo.IsWaferProcessing = FieldInfoUtil.RequestValue();
                    oServiceInfo.ReleaseReason = FieldInfoUtil.RequestValue();
                    oServiceInfo.AllowWIPData = FieldInfoUtil.RequestValue();
                    oServiceInfo.AllowRejectsRecording = FieldInfoUtil.RequestValue();
                    oServiceInfo.AllowBinsRecording = FieldInfoUtil.RequestValue();
                    oServiceInfo.AllowSupplementarySpec = FieldInfoUtil.RequestValue();

                    // Fetch some disposition data
                    oServiceInfo.ApplyToChildLots = FieldInfoUtil.RequestValue();
                    oServiceInfo.scsForceYieldCheck = FieldInfoUtil.RequestValue();
                    oServiceInfo.AutoMoveOut = FieldInfoUtil.RequestValue();
                    oServiceInfo.ProceedNewLotId = FieldInfoUtil.RequestValue();
                    oServiceInfo.ProceedNewLotHoldReason = FieldInfoUtil.RequestValue();
                    oServiceInfo.YieldOffRejects = FieldInfoUtil.RequestValue();
                    oServiceInfo.SplitBins = FieldInfoUtil.RequestValue();
                    oServiceInfo.ReworkReason = FieldInfoUtil.RequestValue();
                    oServiceInfo.ReworkQty = FieldInfoUtil.RequestValue();
                    oServiceInfo.ReworkFullLot = FieldInfoUtil.RequestValue();
                    oServiceInfo.ReworkStepType = FieldInfoUtil.RequestValue();
                    oServiceInfo.ReworkStep = FieldInfoUtil.RequestValue();
                    oServiceInfo.ReworkReEntryStep = FieldInfoUtil.RequestValue();
                    oServiceInfo.ReworkEndStep = FieldInfoUtil.RequestValue();
                    oServiceInfo.SplitRejectLotId = FieldInfoUtil.RequestValue();
                    oServiceInfo.SplitRejectGoodQty = FieldInfoUtil.RequestValue();
                    oServiceInfo.CreateNewSchedule = FieldInfoUtil.RequestValue();
                    oServiceInfo.YieldOffReason = FieldInfoUtil.RequestValue();
                }
                else if (sEventName == const_sDispositionPlan_Rescreen)
                {
                    oServiceData.Container = _ContainerField.Data as ContainerRef;
                    oServiceInfo.InsertionReason = FieldInfoUtil.RequestValue();
                    oServiceInfo.WaiveNextYieldCheck = FieldInfoUtil.RequestValue();
                    oInsertionDetailsInfo = new CreateInsertionDetails_Info();
                    oInsertionDetailsInfo.ProcessType = FieldInfoUtil.RequestValue();
                    oInsertionDetailsInfo.ProcessStatus = FieldInfoUtil.RequestValue();

                    if (_chkIsWaferProcessingField.CheckControl.Checked)
                    {
                        oInsertionDetailsInfo.WafersDetails = new WIPLotTxnWafersDetails_Info();
                        oInsertionDetailsInfo.WafersDetails.WaferScribeNumber = FieldInfoUtil.RequestValue();
                    }
                    else
                    {
                        oInsertionDetailsInfo.AllowQtyOverride = FieldInfoUtil.RequestValue();
                        oInsertionDetailsInfo.QtyToProcess = FieldInfoUtil.RequestValue();
                        oInsertionDetailsInfo.ValidQtys = new CreateInsertionDetailsQtys_Info();
                        oInsertionDetailsInfo.ValidQtys.QtyToProcess = FieldInfoUtil.RequestValue();
                        oInsertionDetailsInfo.MinQtyToProcess = FieldInfoUtil.RequestValue();
                        oInsertionDetailsInfo.MaxQtyToProcess = FieldInfoUtil.RequestValue();
                        oInsertionDetailsInfo.TestStatus = FieldInfoUtil.RequestValue();
                        oInsertionDetailsInfo.WIPTestStatus = FieldInfoUtil.RequestValue();
                        oInsertionDetailsInfo.TestPlan = FieldInfoUtil.RequestValue();
                        oInsertionDetailsInfo.TestSubPlan = FieldInfoUtil.RequestValue();
                        oInsertionDetailsInfo.TestSubPlanType = FieldInfoUtil.RequestValue();
                    }

                    if (!_txtCurrentDispositionPlanField.IsEmpty && _txtCurrentDispositionPlanField.Data.ToString() == const_sDispositionPlan_Rescreen)
                    {
                        oServiceInfo.CurrentInsertionDetails = oInsertionDetailsInfo;
                        _chkRescreenWP_UseCurrentInsertionDetailsField.CheckControl.Checked = true;
                    }
                    else
                    {
                        oServiceInfo.InsertionDetailsSelection = oInsertionDetailsInfo;
                        _chkRescreenWP_UseCurrentInsertionDetailsField.CheckControl.Checked = false;
                    }
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
                    var oResponseData = oResult.Value as LotHoldDisposition;

                    // Set Process Type
                    if (oResponseData.ProcessTypeSelection != null)
                    {
                        List<NamedObjectRef> oProcessTypeSelection = new List<NamedObjectRef>();
                        foreach (NamedObjectRef oProcessType in oResponseData.ProcessTypeSelection)
                        {
                            oProcessTypeSelection.Add(new NamedObjectRef() { Name = oProcessType.Name });
                        }
                        _ndoProcessTypeField.Data = oProcessTypeSelection != null ? oProcessTypeSelection[0] : null;
                    }


                    // Display information for resolve selection id
                    if (sEventName == const_sSelectionId)
                    {
                        // Set the value of the resolve Container Name and other information
                        _ContainerField.Data = oResponseData.SelectionContainer.Name.ToString();
                        _chkIsWaferProcessingField.CheckControl.Checked = _chkIsWaferProcessingField.IsChecked = (oResponseData.IsWaferProcessing == true);
                        if (oResponseData.ScheduleDataObjectType != null)
                        {
                            string serviceTypeRequired = "";
                            string ignored = "";
                            SEMI.AppCode.Services.SchedulingTxn.GetServiceTypes(oResponseData.ScheduleDataObjectType.ToString(), ref ignored, ref serviceTypeRequired, ref ignored, ref ignored, ref ignored, ref ignored, ref ignored);
                            _txtScheduleDataObjectTypeField.Data = serviceTypeRequired;
                        }
                        _txtCommentsField.Data = oResponseData.Comments;

                        if (oResponseData.ReleaseReason != null)
                            _ndoReleaseReasonField.Data = oResponseData.ReleaseReason;

                        if (oResponseData.DispositionPlan != null)
                        {
                            _txtCurrentDispositionPlanField.Data = oResponseData.DispositionPlan;
                            _ddlDispositionPlanField.Data = oResponseData.DispositionPlan.ToString();
                            _ddlDispositionPlanField.ReadOnly = (this.PrimaryServiceType == const_sServiceType_LotHoldDisposition2);

                            if (oResponseData.YieldOffReason != null)
                                _ndoYieldOffWP_YieldOffReasonField.Data = oResponseData.YieldOffReason.ToString();
                            _ndoYieldOffWP_YieldOffReasonField.ReadOnly = (this.PrimaryServiceType == const_sServiceType_LotHoldDisposition2);
                        }

                        // Set the Popup button hidden property
                        if (!IsHorizon())
                        {
                            _btnFailures.Hidden = false;
                            _btnWIPData.Hidden = (oResponseData.AllowWIPData == false);
                        }
                        else
                        {
                            _btnFailuresCmdBar.Visible = true;
                            _btnWIPDataCmdBar.Visible = !(oResponseData.AllowWIPData == false);
                        }

                        if (_chkIsWaferProcessingField.CheckControl.Checked && oResponseData.AllowRejectsRecording == true)
                        {
                            if (!IsHorizon())
                            {
                                _btnItemRejects.Hidden = false;
                                _btnLotRejects.Hidden = true;
                            }
                            else
                            {
                                _btnItemRejectsCmdBar.Visible = true;
                                _btnLotRejectsCmdBar.Visible = false;
                            }
                        }
                        else if (_chkIsWaferProcessingField.CheckControl.Checked == false && oResponseData.AllowRejectsRecording == true)
                        {
                            if (!IsHorizon())
                            {
                                _btnItemRejects.Hidden = true;
                                _btnLotRejects.Hidden = false;
                            }
                            else
                            {
                                _btnItemRejectsCmdBar.Visible = false;
                                _btnLotRejectsCmdBar.Visible = true;
                            }
                        }

                        if (!IsHorizon())
                        {
                            _btnBins.Hidden = (oResponseData.AllowBinsRecording == false);
                            _btnSupplementarySpec.Hidden = (oResponseData.AllowSupplementarySpec == false);
                        }
                        else
                        {
                            _btnBinsCmdBar.Visible = !(oResponseData.AllowBinsRecording == false);
                            _btnSupplementarySpecCmdBar.Visible = !(oResponseData.AllowSupplementarySpec == false);
                        }

                        // Fetch Lot Information and Display on datagrid
                        SetLotSelection(_ContainerField.Data.ToString());

                        // Disposition Plan
                        if (oResponseData.DispositionPlanSelection != null)
                        {
                            RecordSet rsDispositionPlan = ConvertStringToRecordset(oResponseData.DispositionPlanSelection);
                            _ddlDispositionPlanField.SetSelectionValues(rsDispositionPlan);
                        }

                        // Release Only Panel
                        _chkReleaseOnlyWP_ApplyToChildLotsField.CheckControl.Checked = (oResponseData.ApplyToChildLots == true);
                        _chkReleaseOnlyWP_ApplyToChildLotsField.Data = (oResponseData.ApplyToChildLots == true);
                        _chkReleaseOnlyWP_scsForceYieldCheckField.CheckControl.Checked = (oResponseData.scsForceYieldCheck == true);
                        _chkReleaseOnlyWP_scsForceYieldCheckField.IsChecked = (oResponseData.scsForceYieldCheck == true);
                        _chkReleaseOnlyWP_scsForceYieldCheckField.Data = (oResponseData.scsForceYieldCheck == true);

                        // Proceed Panel
                        _txtProceedWP_ProceedNewLotIdField.Data = oResponseData.ProceedNewLotId;
                        _chkProceedWP_ApplyToChildLotsField.CheckControl.Checked = (oResponseData.ApplyToChildLots == true);
                        _chkProceedWP_ApplyToChildLotsField.Data = (oResponseData.ApplyToChildLots == true);
                        _chkProceedWP_YieldOffRejectsField.CheckControl.Checked = (oResponseData.YieldOffRejects == true) & (oResponseData.AllowRejectsRecording == true);
                        _chkProceedWP_YieldOffRejectsField.Data = (oResponseData.YieldOffRejects == true) & (oResponseData.AllowRejectsRecording == true);
                        _chkProceedWP_SplitBinsField.CheckControl.Checked = (oResponseData.SplitBins == true) & (oResponseData.AllowBinsRecording == true);
                        _chkProceedWP_SplitBinsField.Data = (oResponseData.SplitBins == true) & (oResponseData.AllowBinsRecording == true);

                        if (oResponseData.ProceedNewLotHoldReason != null)
                        {
                            _ndoProceedWP_ProceedNewLotHoldReasonField.Data = oResponseData.ProceedNewLotHoldReason;
                        }

                        // Rework Panel
                        _ndoReworkWP_ReworkReasonField.Data = oResponseData.ReworkReason;
                        _chkReworkWP_YieldOffRejectsField.CheckControl.Checked = (oResponseData.YieldOffRejects == true) & (oResponseData.AllowRejectsRecording == true);
                        _chkReworkWP_SplitBinsField.CheckControl.Checked = (oResponseData.SplitBins == true) & (oResponseData.AllowBinsRecording == true);
                        _txtReworkWP_ReworkQtyField.Data = oResponseData.ReworkQty;
                        _chkReworkWP_ReworkFullLotField.CheckControl.Checked = (oResponseData.ReworkFullLot == true);
                        _ddlReworkWP_ReworkStepTypeField.Data = oResponseData.ReworkStepType.ToString();
                        _sndReworkWP_ReworkStepField.Data = oResponseData.ReworkStep;
                        _sndReworkWP_ReworkReEntryStepField.Data = oResponseData.ReworkReEntryStep;
                        _sndReworkWP_ReworkEndStepField.Data = oResponseData.ReworkEndStep;

                        // Split Reject Panel
                        _txtSplitRejectWP_SplitRejectLotIdField.Data = oResponseData.SplitRejectLotId;
                        _txtSplitRejectWP_SplitRejectGoodQtyField.Data = oResponseData.SplitRejectGoodQty;
                        _chkSplitRejectWP_CreateNewScheduleField.CheckControl.Checked = (oResponseData.CreateNewSchedule == true);
                        _chkSplitRejectWP_YieldOffRejectsField.CheckControl.Checked = (oResponseData.YieldOffRejects == true) & (oResponseData.AllowRejectsRecording == true);
                        _chkSplitRejectWP_SplitBinsField.CheckControl.Checked = (oResponseData.SplitBins == true) & (oResponseData.AllowBinsRecording == true);
                        _chkSplitRejectWP_AutoMoveOutField.CheckControl.Checked = (oResponseData.AutoMoveOut == true);

                        _chkSplitRejectWP_CreateNewScheduleField.Data = (oResponseData.CreateNewSchedule == true);
                        _chkSplitRejectWP_YieldOffRejectsField.Data = (oResponseData.YieldOffRejects == true) & (oResponseData.AllowRejectsRecording == true);
                        _chkSplitRejectWP_SplitBinsField.Data = (oResponseData.SplitBins == true) & (oResponseData.AllowBinsRecording == true);
                        _chkSplitRejectWP_AutoMoveOutField.Data = (oResponseData.AutoMoveOut == true);

                    }
                    else if (sEventName == const_sDispositionPlan_Rescreen)
                    {
                        // Display or hide grid columns if lot is Item or Qty Processing.
                        SetGridColumns_RescreenPanel();

                        _chkRescreenWP_WaiveNextYieldCheckField.CheckControl.Checked = (oResponseData.WaiveNextYieldCheck == true);
                        _chkRescreenWP_WaiveNextYieldCheckField.Data = (oResponseData.WaiveNextYieldCheck == true);

                        if (_chkRescreenWP_UseCurrentInsertionDetailsField.CheckControl.Checked)
                            oInsertionDetailsData = oResponseData.CurrentInsertionDetails;
                        else
                            oInsertionDetailsData = oResponseData.InsertionDetailsSelection;

                        if (oResponseData.InsertionReason != null)
                            _ndoRescreenWP_InsertionReasonField.Data = oResponseData.InsertionReason;

                        // Populate WIP Data Value
                        Populate_WIPDataValue(oInsertionDetailsData);

                        // Bind datagrid
                        _gridRescreenWP_InsertionDetailsField.BoundContext.Data = oInsertionDetailsData.ToArray();
                        _gridRescreenWP_InsertionDetailsField.BoundContext.LoadData();
                        CamstarWebControl.SetRenderToClient(_gridRescreenWP_InsertionDetailsField);

                        Page.CollectDataContract();
                    }
                }
                else
                {
                    DisplayMessage(oResultStatus);
                    ResetFields();
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }

        private void SetGridColumns_RescreenPanel()
        {
            string sColCaption = "";

            // Hide columns based on criteria
            JQFieldCollection objFieldCollection = _gridRescreenWP_InsertionDetailsField.BoundContext.Fields;
            foreach (JQField objField in objFieldCollection)
            {
                switch (objField.ID)
                {
                    case "QtyToProcess":
                        objField.Visible = (_chkIsWaferProcessingField.CheckControl.Checked == false);

                        // Pass the column name to popup page.
                        if (_chkIsWaferProcessingField.CheckControl.Checked == false)
                            sColCaption = objField.Caption;
                        break;
                    case "AllowQtyOverride":
                    case "btnQtyToProcess":
                    case "MinQtyToProcess":
                    case "MaxQtyToProcess":
                    case "TestStatus":
                    case "WIPTestStatus":
                    case "TestPlan":
                    case "TestSubPlan":
                    case "TestSubPlanType":
                        objField.Visible = (_chkIsWaferProcessingField.CheckControl.Checked == false);
                        break;
                    case "btnWafersDetails":
                        objField.Visible = (_chkIsWaferProcessingField.CheckControl.Checked == true);

                        // Pass the column name to popup page.
                        if (_chkIsWaferProcessingField.CheckControl.Checked)
                            sColCaption = _txtRescreenWP_WaferScribeNumberField.LabelControl.Text;
                        break;
                }
            }

            // Set the Datacontract member for Caption to be pass to DataSelectionPopup
            Page.DataContract.SetValueByName("RescreenWP_CaptionDM", sColCaption);
        }

        private void ResetFields()
        {
            // Main Panel
            _ndoProcessTypeField.ClearData();
            _gridLotInfoField.ClearData();
            _ndoReleaseReasonField.ClearData();
            _ddlDispositionPlanField.ClearData();
            _ddlDispositionPlanField.ClearSelectionValues();

            if (!IsHorizon())
            {
                _btnFailures.Hidden = true;
                _btnSupplementarySpec.Hidden = true;
                _btnWIPData.Hidden = true;
                _btnLotRejects.Hidden = true;
                _btnItemRejects.Hidden = true;
                _btnBins.Hidden = true;
            }
            else
            {
                _btnFailuresCmdBar.Visible = false;
                _btnSupplementarySpecCmdBar.Visible = false;
                _btnBinsCmdBar.Visible = false;
                _btnItemRejectsCmdBar.Visible = false;
                _btnLotRejectsCmdBar.Visible = false;
                _btnWIPDataCmdBar.Visible = false;
            }

            // Rescreen Panel
            _gridRescreenWP_InsertionDetailsField.ClearData();
        }

        private RecordSet ConvertStringToRecordset(Primitive<string>[] stringRef)
        {
            if (stringRef.Length > 0)
            {
                RecordSet rs = new RecordSet();
                OM.Header[] rsHeaders = new OM.Header[1];
                Row[] rsRow = new Row[stringRef.Length];

                rsHeaders[0] = new OM.Header();
                rsHeaders[0].TypeCode = TypeCode.String;
                rsHeaders[0].Name = "Name";

                rs.Headers = rsHeaders;

                for (int x = 0; x < stringRef.Length; x++)
                {
                    rsRow[x] = new Row();
                    string[] sValues = new string[1];
                    sValues[0] = stringRef[x].Value.ToString();
                    rsRow[x].Values = sValues;
                }
                rs.Rows = rsRow;

                // Output recordset
                return rs;
            }
            else
                return null;
        }

        private void SetControls()
        {
            try
            {
                string sDispositionPlan = _ddlDispositionPlanField.Data != null ? _ddlDispositionPlanField.Data.ToString() : "";

                // Find webpart control
                int iCtrlCount = Parent.Controls.Count;
                MatrixWebPart ReleaseOnlyWP = null;
                MatrixWebPart YieldOffWP = null;
                MatrixWebPart ReworkWP = null;
                MatrixWebPart ProceedWP = null;
                MatrixWebPart RescreenWP = null;
                MatrixWebPart SplitRejectWP = null;

                for (int i = 0; i < iCtrlCount; i++)
                {
                    if (Parent.Controls[i].ID == "ReleaseOnlyWP")
                        ReleaseOnlyWP = Parent.Controls[i] as MatrixWebPart;
                    else if (Parent.Controls[i].ID == "YieldOffWP")
                        YieldOffWP = Parent.Controls[i] as MatrixWebPart;
                    else if (Parent.Controls[i].ID == "ReworkWP")
                        ReworkWP = Parent.Controls[i] as MatrixWebPart;
                    else if (Parent.Controls[i].ID == "ProceedWP")
                        ProceedWP = Parent.Controls[i] as MatrixWebPart;
                    else if (Parent.Controls[i].ID == "RescreenWP")
                        RescreenWP = Parent.Controls[i] as MatrixWebPart;
                    else if (Parent.Controls[i].ID == "SplitRejectWP")
                        SplitRejectWP = Parent.Controls[i] as MatrixWebPart;
                }

                // Release Only Panel
                ReleaseOnlyWP.Enabled = (this.PrimaryServiceType != const_sServiceType_LotHoldDisposition2);
                ReleaseOnlyWP.Hidden = (sDispositionPlan != const_sDispositionPlan_ReleaseOnly);

                // Proceed Panel
                ProceedWP.Enabled = (this.PrimaryServiceType != const_sServiceType_LotHoldDisposition2);
                ProceedWP.Hidden = (sDispositionPlan != const_sDispositionPlan_Proceed);

                if (!IsHorizon())
                {
                    _chkProceedWP_YieldOffRejectsField.Hidden = (_btnLotRejects.Hidden && _btnItemRejects.Hidden);
                    _chkProceedWP_SplitBinsField.Hidden = _btnBins.Hidden;
                }
                else
                {
                    _chkProceedWP_YieldOffRejectsField.Hidden = (!_btnLotRejectsCmdBar.Visible && !_btnItemRejectsCmdBar.Visible);
                    _chkProceedWP_SplitBinsField.Hidden = !_btnBinsCmdBar.Visible;

                }

                // Rework Panel
                ReworkWP.Enabled = (this.PrimaryServiceType != const_sServiceType_LotHoldDisposition2);
                ReworkWP.Hidden = (sDispositionPlan != const_sDispositionPlan_Rework);
                _chkReworkWP_ReworkFullLotField.Hidden = _chkIsWaferProcessingField.CheckControl.Checked;
                _txtReworkWP_ReworkQtyField.Hidden = _chkIsWaferProcessingField.CheckControl.Checked;

                if (!IsHorizon())
                {
                    _chkReworkWP_YieldOffRejectsField.Hidden = (_btnLotRejects.Hidden && _btnItemRejects.Hidden);
                    _chkReworkWP_SplitBinsField.Hidden = _btnBins.Hidden;
                }
                else
                {
                    _chkReworkWP_YieldOffRejectsField.Hidden = (!_btnLotRejectsCmdBar.Visible && !_btnItemRejectsCmdBar.Visible);
                    _chkReworkWP_SplitBinsField.Hidden = !_btnBinsCmdBar.Visible;
                }

                // Split Reject Panel
                SplitRejectWP.Enabled = (this.PrimaryServiceType != const_sServiceType_LotHoldDisposition2);
                SplitRejectWP.Hidden = (sDispositionPlan != const_sDispositionPlan_SplitReject);

                if (!IsHorizon())
                {
                    _chkSplitRejectWP_YieldOffRejectsField.Hidden = (_btnLotRejects.Hidden && _btnItemRejects.Hidden);
                    _chkSplitRejectWP_SplitBinsField.Hidden = _btnBins.Hidden;
                }
                else
                {
                    _chkSplitRejectWP_YieldOffRejectsField.Hidden = (!_btnLotRejectsCmdBar.Visible && !_btnItemRejectsCmdBar.Visible);
                    _chkSplitRejectWP_SplitBinsField.Hidden = !_btnBinsCmdBar.Visible;
                }

                // Yield Off Panel
                YieldOffWP.Enabled = (this.PrimaryServiceType != const_sServiceType_LotHoldDisposition2);
                YieldOffWP.Hidden = (sDispositionPlan != const_sDispositionPlan_YieldOff);

                // Rescreen Panel
                RescreenWP.Enabled = (this.PrimaryServiceType != const_sServiceType_LotHoldDisposition2);
                RescreenWP.Hidden = (sDispositionPlan != const_sDispositionPlan_Rescreen);
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.TargetSite.Name + "(): " + Ex.Message);
            }
        }

        public override void GetInputData(Service serviceData)
        {
            try
            {
                base.GetInputData(serviceData);
                string sDispositionPlan = _ddlDispositionPlanField.Data != null ? _ddlDispositionPlanField.Data.ToString() : "";

                if (this.PrimaryServiceType != const_sServiceType_LotHoldDisposition2)
                {
                    (serviceData as OM.LotHoldDisposition).ReleaseReason = _ndoReleaseReasonField.Data as NamedObjectRef;
                    (serviceData as OM.LotHoldDisposition).DispositionPlan = sDispositionPlan;
                    (serviceData as OM.LotHoldDisposition).Comments = _txtCommentsField.Data != null ? _txtCommentsField.Data.ToString() : null;

                    switch (sDispositionPlan)
                    {
                        case const_sDispositionPlan_ReleaseOnly:
                            (serviceData as OM.LotHoldDisposition).ApplyToChildLots = _chkReleaseOnlyWP_ApplyToChildLotsField.CheckControl.Checked;
                            (serviceData as OM.LotHoldDisposition).scsForceYieldCheck = _chkReleaseOnlyWP_scsForceYieldCheckField.CheckControl.Checked;
                            break;
                        case const_sDispositionPlan_Proceed:
                            if (!_txtProceedWP_ProceedNewLotIdField.IsEmpty)
                                (serviceData as OM.LotHoldDisposition).ProceedNewLotId = _txtProceedWP_ProceedNewLotIdField.Data.ToString();
                            if (!_ndoProceedWP_ProceedNewLotHoldReasonField.IsEmpty)
                                (serviceData as OM.LotHoldDisposition).ProceedNewLotHoldReason = _ndoProceedWP_ProceedNewLotHoldReasonField.Data as NamedObjectRef;
                            (serviceData as OM.LotHoldDisposition).ApplyToChildLots = _chkProceedWP_ApplyToChildLotsField.CheckControl.Checked;
                            (serviceData as OM.LotHoldDisposition).YieldOffRejects = _chkProceedWP_YieldOffRejectsField.CheckControl.Checked;
                            (serviceData as OM.LotHoldDisposition).SplitBins = _chkProceedWP_SplitBinsField.CheckControl.Checked;
                            break;
                        case const_sDispositionPlan_Rework:
                            if (!_ndoReworkWP_ReworkReasonField.IsEmpty)
                                (serviceData as OM.LotHoldDisposition).ReworkReason = _ndoReworkWP_ReworkReasonField.Data as NamedObjectRef;
                            (serviceData as OM.LotHoldDisposition).YieldOffRejects = _chkReworkWP_YieldOffRejectsField.CheckControl.Checked;
                            (serviceData as OM.LotHoldDisposition).SplitBins = _chkReworkWP_SplitBinsField.CheckControl.Checked;
                            (serviceData as OM.LotHoldDisposition).ReworkQty = _txtReworkWP_ReworkQtyField.Data != null ? double.Parse(_txtReworkWP_ReworkQtyField.Data.ToString()) : 0;
                            (serviceData as OM.LotHoldDisposition).ReworkFullLot = _chkReworkWP_ReworkFullLotField.CheckControl.Checked;
                            (serviceData as OM.LotHoldDisposition).ReworkStepType = _ddlReworkWP_ReworkStepTypeField.Data != null ? _ddlReworkWP_ReworkStepTypeField.Data.ToString() : null;
                            (serviceData as OM.LotHoldDisposition).ReworkStep = _sndReworkWP_ReworkStepField.Data as NamedSubentityRef;
                            (serviceData as OM.LotHoldDisposition).ReworkReEntryStep = _sndReworkWP_ReworkReEntryStepField.Data as NamedSubentityRef;
                            (serviceData as OM.LotHoldDisposition).ReworkEndStep = _sndReworkWP_ReworkEndStepField.Data as NamedSubentityRef;
                            break;
                        case const_sDispositionPlan_SplitReject:
                            if (!_txtSplitRejectWP_SplitRejectLotIdField.IsEmpty)
                                (serviceData as OM.LotHoldDisposition).SplitRejectLotId = _txtSplitRejectWP_SplitRejectLotIdField.Data != null ? _txtSplitRejectWP_SplitRejectLotIdField.Data.ToString() : null;
                            if (!_txtSplitRejectWP_SplitRejectGoodQtyField.IsEmpty)
                                (serviceData as OM.LotHoldDisposition).SplitRejectGoodQty = _txtSplitRejectWP_SplitRejectGoodQtyField.Data != null ? double.Parse(_txtSplitRejectWP_SplitRejectGoodQtyField.Data.ToString()) : 0;
                            (serviceData as OM.LotHoldDisposition).CreateNewSchedule = _chkSplitRejectWP_CreateNewScheduleField.CheckControl.Checked;
                            (serviceData as OM.LotHoldDisposition).YieldOffRejects = _chkSplitRejectWP_YieldOffRejectsField.CheckControl.Checked;
                            (serviceData as OM.LotHoldDisposition).SplitBins = _chkSplitRejectWP_SplitBinsField.CheckControl.Checked;
                            (serviceData as OM.LotHoldDisposition).AutoMoveOut = _chkSplitRejectWP_AutoMoveOutField.CheckControl.Checked;
                            break;
                        case const_sDispositionPlan_YieldOff:
                            (serviceData as OM.LotHoldDisposition).YieldOffRejects = false;
                            if (!_ndoYieldOffWP_YieldOffReasonField.IsEmpty)
                                (serviceData as OM.LotHoldDisposition).YieldOffReason = _ndoYieldOffWP_YieldOffReasonField.Data as NamedObjectRef;
                            break;
                        case const_sDispositionPlan_Rescreen:
                            //if (!_ndoRescreenWP_InsertionReasonField.IsEmpty)

                            (serviceData as OM.LotHoldDisposition).InsertionReason = _ndoRescreenWP_InsertionReasonField.Data as NamedObjectRef;
                            (serviceData as OM.LotHoldDisposition).WaiveNextYieldCheck = _chkRescreenWP_WaiveNextYieldCheckField.CheckControl.Checked;

                            int iGridCount = _gridRescreenWP_InsertionDetailsField.GridContext.GetTotalRows();
                            (serviceData as OM.LotHoldDisposition).InsertionDetails = new CreateInsertionDetails[iGridCount];
                            CreateInsertionDetails[] oInputInsertionDetails = (_gridRescreenWP_InsertionDetailsField.BoundContext).Data as CreateInsertionDetails[];

                            for (int i = 0; i < iGridCount; i++)
                            {
                                (serviceData as OM.LotHoldDisposition).InsertionDetails[i] = new CreateInsertionDetails();
                                (serviceData as OM.LotHoldDisposition).InsertionDetails[i].ProcessType = oInputInsertionDetails[i].ProcessType;
                                (serviceData as OM.LotHoldDisposition).InsertionDetails[i].ProcessStatus = oInputInsertionDetails[i].ProcessStatus;

                                // Check whether wafer or qty processing
                                if (_chkIsWaferProcessingField.CheckControl.Checked)
                                {
                                    int iWafersDetailsCount = oInputInsertionDetails[i].WafersDetails.Count();
                                    (serviceData as OM.LotHoldDisposition).InsertionDetails[i].WafersDetails = new WIPLotTxnWafersDetails[iWafersDetailsCount];
                                    for (int j = 0; j < iWafersDetailsCount; j++)
                                    {
                                        (serviceData as OM.LotHoldDisposition).InsertionDetails[i].WafersDetails[j] = new WIPLotTxnWafersDetails();
                                        (serviceData as OM.LotHoldDisposition).InsertionDetails[i].WafersDetails[j].WaferScribeNumber = oInputInsertionDetails[i].WafersDetails[j].WaferScribeNumber;
                                    }
                                }
                                else
                                {
                                    (serviceData as OM.LotHoldDisposition).InsertionDetails[i].AllowQtyOverride = oInputInsertionDetails[i].AllowQtyOverride;
                                    (serviceData as OM.LotHoldDisposition).InsertionDetails[i].QtyToProcess = oInputInsertionDetails[i].QtyToProcess;
                                    (serviceData as OM.LotHoldDisposition).InsertionDetails[i].MinQtyToProcess = oInputInsertionDetails[i].MinQtyToProcess;
                                    (serviceData as OM.LotHoldDisposition).InsertionDetails[i].MaxQtyToProcess = oInputInsertionDetails[i].MaxQtyToProcess;
                                    (serviceData as OM.LotHoldDisposition).InsertionDetails[i].TestStatus = oInputInsertionDetails[i].TestStatus;
                                    (serviceData as OM.LotHoldDisposition).InsertionDetails[i].WIPTestStatus = oInputInsertionDetails[i].WIPTestStatus;
                                    (serviceData as OM.LotHoldDisposition).InsertionDetails[i].TestPlan = oInputInsertionDetails[i].TestPlan;
                                    (serviceData as OM.LotHoldDisposition).InsertionDetails[i].TestSubPlan = oInputInsertionDetails[i].TestSubPlan;
                                    (serviceData as OM.LotHoldDisposition).InsertionDetails[i].TestSubPlanType = oInputInsertionDetails[i].TestSubPlanType;

                                    int iValidQtysCount = oInputInsertionDetails[i].ValidQtys.Count();
                                    (serviceData as OM.LotHoldDisposition).InsertionDetails[i].ValidQtys = new CreateInsertionDetailsQtys[iValidQtysCount];
                                    for (int j = 0; j < iValidQtysCount; j++)
                                    {
                                        (serviceData as OM.LotHoldDisposition).InsertionDetails[i].ValidQtys[j] = new CreateInsertionDetailsQtys();
                                        (serviceData as OM.LotHoldDisposition).InsertionDetails[i].ValidQtys[j].QtyToProcess = oInputInsertionDetails[i].ValidQtys[j].QtyToProcess;
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }

        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                Page.ShopfloorReset(null, null);
                ResetFields();
                SetControls();
            }
        }
        #endregion

    }
}
