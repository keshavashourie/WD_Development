/* Copyright 2023 Siemens */
using System;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WCF.Services;
using Camstar.WebPortal.Personalization;
using System.Collections.Generic;
using DocumentFormat.OpenXml.ExtendedProperties;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class SS_ExperimentPlanMaint : MatrixWebPart
    {
        private CWC.RevisionedObject _rdoProcessSpecField { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpec") as CWC.RevisionedObject; } }

        private CWC.NamedObject _ndoOwnerGroupField { get { return Page.FindCamstarControl("ObjectChanges_OwnerGroup") as CWC.NamedObject; } }

        private CWC.NamedObject _ndoReviewAndApprovalGroupField { get { return Page.FindCamstarControl("ObjectChanges_ReviewAndApprovalGroup") as CWC.NamedObject; } }

        private CWC.DateChooser _dtcEffectiveFromDateField { get { return Page.FindCamstarControl("ObjectChanges_EffectiveFromDate") as CWC.DateChooser; } }

        private CWC.DateChooser _dtcEffectiveThruDateField { get { return Page.FindCamstarControl("ObjectChanges_EffectiveThruDate") as CWC.DateChooser; } }

        private JQDataGrid _gridDetailsField { get { return Page.FindCamstarControl("ObjectChanges_ExperimentPlanDetails") as JQDataGrid; } }

        private CWC.DropDownList _ddlExperimentPlanStatusField { get { return Page.FindCamstarControl("ObjectChanges_ExperimentPlanStatus") as CWC.DropDownList; } }

        private JQDataGrid _gridActionDetailsField { get { return Page.FindCamstarControl("ExperimentPlanDetails_ActionDetails") as JQDataGrid; } }

        private CWC.DropDownList _ddlActionTypeFieldEditor { get { return _gridActionDetailsField.FindControl("ExperimentPlanDetails_ActionDetails_ActionType_InlineEditorControl") as CWC.DropDownList; } }

        private CWC.DropDownList _ddlExpPlanActionFieldEditor { get { return _gridActionDetailsField.FindControl("ExperimentPlanDetails_ActionDetails_ExpPlanAction_InlineEditorControl") as CWC.DropDownList; } }

        private CWC.TextBox _txtHiddenSelectedRowID { get { return Page.FindCamstarControl("HiddenSelectedRowIDTextBox") as CWC.TextBox; } }

        private CWC.TextBox _txtStepPlanDescription { get { return Page.FindCamstarControl("ExperimentPlanDetails_StepPlanDescription") as CWC.TextBox; } }

        private CWC.DropDownList _ddlExperimentPlanDetailsStepNameField { get { return _gridActionDetailsField.FindControl("ExperimentPlanDetails_StepName") as CWC.DropDownList; } }

        private CWC.RevisionedObject _rdoExperimentPlanDetailsSpecField { get { return Page.FindCamstarControl("ExperimentPlanDetails_Spec") as CWC.RevisionedObject; } }

        private CWC.NamedObject _ndoExperimentPlanDetailsTransactionRestrictionRoleField { get { return Page.FindCamstarControl("ExperimentPlanDetails_TransactionRestrictionRole") as CWC.NamedObject; } }

        private CWC.NamedObject _ndoExperimentPlanDetailsSpecialInstructionsField { get { return Page.FindCamstarControl("ExperimentPlanDetails_SpecialInstructions") as CWC.NamedObject; } }

        private JQDataGrid _gridExperimentPlanDetailsParameterOverridesField { get { return Page.FindCamstarControl("ExperimentPlanDetails_ParameterOverrides") as JQDataGrid; } }

        private CWC.NamedObject _ndoExperimentPlanDetailsEquipmentField { get { return Page.FindCamstarControl("ExperimentPlanDetails_Equipment") as CWC.NamedObject; } }

        private CWC.NamedObject _ndoExperimentPlanDetailsEquipmentGroupField { get { return Page.FindCamstarControl("ExperimentPlanDetails_EquipmentGroup") as CWC.NamedObject; } }

        private CWC.NamedObject _ndoExperimentPlanDetailsAutomationPlanField { get { return Page.FindCamstarControl("ExperimentPlanDetails_AutomationPlan") as CWC.NamedObject; } }

        private JQDataGrid _gridExperimentPlanDetailsActionDetailsField { get { return Page.FindCamstarControl("ExperimentPlanDetails_ActionDetails") as JQDataGrid; } }

        private CWC.DropDownList _ddlExperimentPlanDetailsExpInstructionTypeField { get { return _gridActionDetailsField.FindControl("ExperimentPlanDetails_ss_ExpInstructionType") as CWC.DropDownList; } }

        private JQDataGrid _gridExperimentPlanDetailsSlotWaferInstructionField { get { return Page.FindCamstarControl("ExperimentPlanDetails_ss_SlotWaferInstruction") as JQDataGrid; } }

        CWC.WorkflowNavigator _DetailsWfNav { get { return Page.FindCamstarControl("ExperimentPlantDetails_Workflow") as CWC.WorkflowNavigator; } }
        CWC.WorkflowNavigator _DetailsSkipToWfNav { get { return Page.FindCamstarControl("ExperimentPlantDetails_SkipToWorkflow") as CWC.WorkflowNavigator; } }
        CWC.NamedSubentity _DetailsStep { get { return Page.FindCamstarControl("Details_ss_Step") as CWC.NamedSubentity; } }
        CWC.NamedSubentity _DetailsSkipToStep { get { return Page.FindCamstarControl("Details_ss_SkipToStep") as CWC.NamedSubentity; } }

        CWC.CheckBox _DetailsUpdateSkipToStackKey { get { return Page.FindCamstarControl("Details_ss_UpdateSkipToStackKey") as CWC.CheckBox; } }
        CWC.CheckBox _DetailsUpdateStackKey { get { return Page.FindCamstarControl("Details_ss_UpdateStackKey") as CWC.CheckBox; } }
        CWC.CheckBox _DetailsDisableCarrierValidation { get { return Page.FindCamstarControl("ExperimentPlanDetails_ss_DisableCarrierValidation") as CWC.CheckBox; } }

        protected MatrixWebPart wpDetailsWP
        {
            get
            {
                MatrixWebPart wp = null;
                int iParentControlsCount = Parent.Controls.Count;
                for (int z = 0; z < iParentControlsCount; z++)
                {
                    if (Parent.Controls[z].ID == "ExperimentPlantDetailsPopupWP")
                        wp = Parent.Controls[z] as MatrixWebPart;
                }
                return wp;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            if (((CWC.CheckBox)Page.FindCamstarControl("ObjectChanges_SysForceToUpper")).CheckControl.Checked == true)
                SEMI.AppCode.UIUtility.SetCapital((CWC.TextBox)Page.FindCamstarControl("NameTxt"), ((CWC.CheckBox)Page.FindCamstarControl("ObjectChanges_SysForceToUpper")).CheckControl.Checked);

            base.OnLoad(e);
            if (_ddlActionTypeFieldEditor != null)
            {
                _ddlActionTypeFieldEditor.AutoPostBack = true;
                _ddlActionTypeFieldEditor.ClearDataWhenDisabled = false;
                _ddlActionTypeFieldEditor.DataChanged += _ddlActionTypeFieldEditor_DataChanged;
                _ddlExpPlanActionFieldEditor.DisplayingData += new EventHandler<CWC.PickLists.DataRequestEventArgs>(_ddlExpPlanActionFieldEditor_DisplayingData);
            }
            var stackControl = Page.FindCamstarControl(_DetailsWfNav.ClientID + "_Stack") as FieldControl;
            stackControl.DataSubmissionMode = Personalization.DataSubmissionModeType.Skip;
            _DetailsWfNav.StepControl.DataSubmissionMode = Personalization.DataSubmissionModeType.Skip;
            _DetailsWfNav.StepControl.Hidden = true;

            var stackControl2 = Page.FindCamstarControl(_DetailsSkipToWfNav.ClientID + "_Stack") as FieldControl;
            stackControl2.DataSubmissionMode = Personalization.DataSubmissionModeType.Skip;
            _DetailsSkipToWfNav.StepControl.DataSubmissionMode = Personalization.DataSubmissionModeType.Skip;
            _DetailsSkipToWfNav.StepControl.Hidden = true;

            if (Page.EventTarget != null && Page.EventTarget.Contains("ObjectChanges_ExperimentPlanDetails"))
                _DetailsWfNav.ClearData();

            //---Fix Bug 348003:When editing Experiment Plan Details, the WorkflowStackKey update wrongly---Start
            //_DetailsWfNav.DataChanged += _DetailsWfNav_DataChanged;
            //---Fix Bug 348003:When editing Experiment Plan Details, the WorkflowStackKey update wrongly---End

            //_DetailsSkipToWfNav.DataChanged += _DetailsSkipToWfNav_DataChanged;
            _DetailsWfNav.StepControl.DataChanged += _DetailsWfNav_DataChanged;
            _DetailsSkipToWfNav.StepControl.DataChanged += _DetailsSkipToWfNav_DataChanged;

            _DetailsUpdateSkipToStackKey.Hidden = true;
            _DetailsUpdateStackKey.Hidden = true;

            if (_DetailsWfNav.Data == null)
            {
                _DetailsStep.Data = null;
                _DetailsUpdateStackKey.Data = true;
                _DetailsUpdateStackKey.CheckControl.Checked = true;
            }
            if (wpDetailsWP != null)
                wpDetailsWP.Hidden = true;
        }

        void _DetailsSkipToWfNav_DataChanged(object sender, EventArgs e)
        {
            var stackControl = Page.FindCamstarControl(_DetailsSkipToWfNav.ClientID + "_Stack") as FieldControl;
            NamedSubentityRef[] stack = (stackControl.Data) as NamedSubentityRef[];
            JQDataGrid grdStack = Page.FindCamstarControl("Details_ssSkipToWorkflowStack") as JQDataGrid;

            if (stack != null)
                grdStack.Data = stack;
            else
                grdStack.Data = null;

            if (_DetailsSkipToWfNav.StepControl.Data != null)
                _DetailsSkipToStep.Data = _DetailsSkipToWfNav.StepControl.Data;

            _DetailsUpdateSkipToStackKey.Data = true;
            _DetailsUpdateSkipToStackKey.CheckControl.Checked = true;



            stackControl.Data = null;
            CamstarWebControl.SetRenderToClient(grdStack);
            CamstarWebControl.SetRenderToClient(_DetailsSkipToStep);
        }


        void _DetailsWfNav_DataChanged(object sender, EventArgs e)
        {
            var stackControl = Page.FindCamstarControl(_DetailsWfNav.ClientID + "_Stack") as FieldControl;
            NamedSubentityRef[] stack = (stackControl.Data) as NamedSubentityRef[];
            JQDataGrid grdStack = Page.FindCamstarControl("Details_ssWorkflowStack") as JQDataGrid;

            //---Fix Bug 348003:When editing Experiment Plan Details, the WorkflowStackKey update wrongly---Start
            if (_DetailsWfNav.StepControl.Data != null){
                _DetailsStep.Data = _DetailsWfNav.StepControl.Data;
                _DetailsWfNav.StepControl.Data = null;
            }
            else if (_DetailsWfNav.StepControl.Data == null && _DetailsWfNav.Data == null)
               _DetailsStep.Data = null;
            //---Fix Bug 348003:When editing Experiment Plan Details, the WorkflowStackKey update wrongly---End

            if (stack != null)
                grdStack.Data = stack;
            else
                grdStack.Data = null;

            //---OBSOLETE LOGIC. Bug 348003:When editing Experiment Plan Details, the WorkflowStackKey update wrongly---
            // if (_DetailsWfNav.StepControl.Data != null)
            //    _DetailsStep.Data = _DetailsWfNav.StepControl.Data;
            // else if (_DetailsWfNav.StepControl.Data == null && _DetailsWfNav.Data == null)
            //    _DetailsStep.Data = null;

            _DetailsUpdateStackKey.Data = true;
            _DetailsUpdateStackKey.CheckControl.Checked = true;

            stackControl.Data = null;
            CamstarWebControl.SetRenderToClient(grdStack);
            CamstarWebControl.SetRenderToClient(_DetailsStep);
        }

        void _ddlActionTypeFieldEditor_DataChanged(object sender, EventArgs e)
        {
            if (_ddlActionTypeFieldEditor.Data != null)
            {
                _gridActionDetailsField.Focus();
                ExperimentActionChanges[] newActionDetails = new ExperimentActionChanges[_gridActionDetailsField.TotalRowCount];
                newActionDetails = _gridActionDetailsField.Data as ExperimentActionChanges[];
                if (_txtHiddenSelectedRowID.Data != null)
                {
                    int selectedRowId = Convert.ToInt32(_txtHiddenSelectedRowID.Data);
                    if (newActionDetails[selectedRowId] != null && (newActionDetails[selectedRowId].ActionType != _ddlActionTypeFieldEditor.Data.ToString()))
                    {
                        newActionDetails[selectedRowId].ActionType = _ddlActionTypeFieldEditor.Data.ToString();
                        newActionDetails[selectedRowId].ExpPlanAction = null;
                    }
                    _ddlActionTypeFieldEditor.ClearData();
                    CamstarWebControl.SetRenderToClient(_ddlExpPlanActionFieldEditor);
                    CamstarWebControl.SetRenderToClient(_gridActionDetailsField);
                }
            }
        }

        void _ddlExpPlanActionFieldEditor_DisplayingData(object sender, CWC.PickLists.DataRequestEventArgs e)
        {
            if (e.TotalRecords == 0)
            {
                ExperimentActionChanges[] newActionDetails = new ExperimentActionChanges[_gridActionDetailsField.TotalRowCount];
                newActionDetails = _gridActionDetailsField.Data as ExperimentActionChanges[];

                int selectedRowId = Convert.ToInt32(_txtHiddenSelectedRowID.Data);
                if (newActionDetails[selectedRowId] != null)
                {
                    var fs = FrameworkManagerUtil.GetFrameworkSession();
                    Result objResult = new Result();

                    // init the service, service data and service info objects
                    UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                    ExperimentPlanMaintService Svc = new ExperimentPlanMaintService(profile);
                    ExperimentPlanMaint SvcData = new ExperimentPlanMaint();
                    ExperimentPlanChanges_Info objChangesInfo = new ExperimentPlanChanges_Info() { ExperimentPlanDetails = new ExperimentPlanDetailsChanges_Info() { ActionDetails = new ExperimentActionChanges_Info() } };
                    ExperimentPlanMaint_Info SvcInfo = new ExperimentPlanMaint_Info();
                    ExperimentPlanMaint_Request ReqData = new ExperimentPlanMaint_Request();
                    ExperimentPlanMaint_Result ResData = new ExperimentPlanMaint_Result();

                    SvcData.ObjectChanges = new ExperimentPlanChanges();
                    SvcData.ObjectChanges.ExperimentPlanDetails = new ExperimentPlanDetailsChanges[1] { new ExperimentPlanDetailsChanges() };
                    SvcData.ObjectChanges.ExperimentPlanDetails[0].ActionDetails = new ExperimentActionChanges[1] { new ExperimentActionChanges() };
                    string sActionType = "";
                    if (newActionDetails[selectedRowId].ActionType != null)
                        sActionType = newActionDetails[selectedRowId].ActionType.ToString(); // by default, use the existing ActionType already set in the grid if available

                    if (_ddlActionTypeFieldEditor != null)
                        if (_ddlActionTypeFieldEditor.Data != null)
                            sActionType = _ddlActionTypeFieldEditor.Data.ToString(); // the actual ActionType should be dependent on the Editor control
                    SvcData.ObjectChanges.ExperimentPlanDetails[0].ActionDetails[0].ActionType = sActionType;
                    objChangesInfo.ExperimentPlanDetails.ActionDetails.ExpPlanAction = FieldInfoUtil.RequestSelectionValue();
                    SvcInfo.ObjectChanges = objChangesInfo;
                    ReqData.Info = SvcInfo;

                    //Execute Request
                    ResultStatus Results = Svc.GetEnvironment(SvcData, ReqData, out ResData);
                    if (Results.IsSuccess && ResData.Environment.ObjectChanges.ExperimentPlanDetails.ActionDetails.ExpPlanAction.SelectionValues != null)
                    {
                        if (ResData.Environment.ObjectChanges.ExperimentPlanDetails.ActionDetails.ExpPlanAction.SelectionValues.Rows != null)
                        {
                            if (ResData.Environment.ObjectChanges.ExperimentPlanDetails.ActionDetails.ExpPlanAction.SelectionValues.Rows.Length > 0)
                            {
                                _ddlExpPlanActionFieldEditor.SetSelectionValues(ResData.Environment.ObjectChanges.ExperimentPlanDetails.ActionDetails.ExpPlanAction.SelectionValues);
                            }
                        }
                        else
                            _ddlActionTypeFieldEditor.ClearSelectionValues();
                    }
                }

            }

        }



        private void CheckIfPlanApproved()
        {
            if (_ddlExperimentPlanStatusField.Data != null)
            {
                if (_ddlExperimentPlanStatusField.OriginalData != null && _ddlExperimentPlanStatusField.OriginalData.ToString().Equals("Approved"))
                {

                    _rdoProcessSpecField.ReadOnly = true;
                    _ndoOwnerGroupField.Enabled = false;
                    _ndoReviewAndApprovalGroupField.Enabled = false;
                    _dtcEffectiveFromDateField.ReadOnly = true;
                    _dtcEffectiveThruDateField.ReadOnly = true;
                    _gridDetailsField.ReadOnly = true;
                    _gridDetailsField.Settings.NavigatorActions = new Personalization.JQNavigatorAction[] {new Personalization.JQNavigatorAction(){
                                          Action= Personalization.JQGridNavActionType.Edit,
                                          Enable=true,
                                          Visible=true}};

                    //popup's fields
                    _txtStepPlanDescription.ReadOnly = true;
                    _ddlExperimentPlanDetailsStepNameField.ReadOnly = true;
                    _rdoExperimentPlanDetailsSpecField.ReadOnly = true;
                    _ndoExperimentPlanDetailsTransactionRestrictionRoleField.Enabled = false;
                    _ndoExperimentPlanDetailsSpecialInstructionsField.Enabled = false;
                    _DetailsDisableCarrierValidation.Enabled = false;
                    _DetailsSkipToWfNav.Enabled = false;
                    _DetailsWfNav.Enabled = false;
                    _ddlExperimentPlanDetailsExpInstructionTypeField.ReadOnly = true;
                    _gridExperimentPlanDetailsParameterOverridesField.ReadOnly = true;
                    _gridExperimentPlanDetailsParameterOverridesField.Enabled = false;
                    (_gridExperimentPlanDetailsParameterOverridesField.Settings as GridDataSettingsItemList).EditorSettings.EditingMode = JQEditingModes.Disabled;
                    _gridExperimentPlanDetailsParameterOverridesField.Settings.NavigatorActions = new Personalization.JQNavigatorAction[] {new Personalization.JQNavigatorAction(){
                                          Action= Personalization.JQGridNavActionType.Excel,
                                          Enable=false,
                                          Visible=false},
                                   new Personalization.JQNavigatorAction(){
                                          Action= Personalization.JQGridNavActionType.Delete,
                                          Enable=false,
                                          Visible=false}};
                    _gridExperimentPlanDetailsParameterOverridesField.ApplyFieldPersonalization();
                    _ndoExperimentPlanDetailsEquipmentField.Enabled = false;
                    _ndoExperimentPlanDetailsEquipmentGroupField.Enabled = false;
                    _ndoExperimentPlanDetailsAutomationPlanField.Enabled = false;
                    _gridExperimentPlanDetailsActionDetailsField.ReadOnly = true;
                    _gridExperimentPlanDetailsActionDetailsField.Enabled = false;
                    (_gridExperimentPlanDetailsActionDetailsField.Settings as GridDataSettingsItemList).EditorSettings.EditingMode = JQEditingModes.Disabled;
                    _gridExperimentPlanDetailsActionDetailsField.Settings.NavigatorActions = new Personalization.JQNavigatorAction[] {new Personalization.JQNavigatorAction(){
                                          Action= Personalization.JQGridNavActionType.Excel,
                                          Enable=false,
                                          Visible=false},
                                   new Personalization.JQNavigatorAction(){
                                          Action= Personalization.JQGridNavActionType.Delete,
                                          Enable=false,
                                          Visible=false}};
                    _gridExperimentPlanDetailsActionDetailsField.ApplyFieldPersonalization();
                    _gridExperimentPlanDetailsSlotWaferInstructionField.ReadOnly = true;
                    _gridExperimentPlanDetailsSlotWaferInstructionField.Enabled = false;
                    (_gridExperimentPlanDetailsSlotWaferInstructionField.Settings as GridDataSettingsItemList).EditorSettings.EditingMode = JQEditingModes.Disabled;
                    _gridExperimentPlanDetailsSlotWaferInstructionField.Settings.NavigatorActions = new Personalization.JQNavigatorAction[] {new Personalization.JQNavigatorAction(){
                                          Action= Personalization.JQGridNavActionType.Excel,
                                          Enable=false,
                                          Visible=false},
                                   new Personalization.JQNavigatorAction(){
                                          Action= Personalization.JQGridNavActionType.Delete,
                                          Enable=false,
                                          Visible=false}};
                    _gridExperimentPlanDetailsSlotWaferInstructionField.ApplyFieldPersonalization();
                }
                else
                {
                    _rdoProcessSpecField.ReadOnly = false;
                    _ndoOwnerGroupField.Enabled = true;
                    _ndoReviewAndApprovalGroupField.Enabled = true;
                    _dtcEffectiveFromDateField.ReadOnly = false;
                    _dtcEffectiveThruDateField.ReadOnly = false;
                    _gridDetailsField.ReadOnly = false;
                    _dtcEffectiveFromDateField.Enabled = true;
                    _dtcEffectiveThruDateField.Enabled = true;
                    _gridDetailsField.Settings.NavigatorActions = new Personalization.JQNavigatorAction[]
                    {
                       new Personalization.JQNavigatorAction(){
                           Action= Personalization.JQGridNavActionType.Add,
                           Enable= true,
                           Visible=true},
                       new Personalization.JQNavigatorAction(){
                           Action= Personalization.JQGridNavActionType.Edit,
                           Enable=true,
                           Visible=true},
                       new Personalization.JQNavigatorAction(){
                           Action= Personalization.JQGridNavActionType.Delete,
                           Enable=true,
                           Visible=true,}
                    };

                    //popup's fields
                    _txtStepPlanDescription.ReadOnly = false;
                    _ddlExperimentPlanDetailsStepNameField.ReadOnly = false;
                    _rdoExperimentPlanDetailsSpecField.ReadOnly = false;
                    _ndoExperimentPlanDetailsTransactionRestrictionRoleField.Enabled = true;
                    _ndoExperimentPlanDetailsSpecialInstructionsField.Enabled = true;
                    _gridExperimentPlanDetailsParameterOverridesField.ReadOnly = false;
                    _DetailsDisableCarrierValidation.Enabled = true;
                    _DetailsSkipToWfNav.Enabled = true;
                    _DetailsWfNav.Enabled = true;
                    _ddlExperimentPlanDetailsExpInstructionTypeField.ReadOnly = false;
                    _gridExperimentPlanDetailsParameterOverridesField.Enabled = true;
                    (_gridExperimentPlanDetailsParameterOverridesField.Settings as GridDataSettingsItemList).EditorSettings.EditingMode = JQEditingModes.Inline;
                    _gridExperimentPlanDetailsParameterOverridesField.Settings.NavigatorActions = new Personalization.JQNavigatorAction[] {new Personalization.JQNavigatorAction(){
                           Action= Personalization.JQGridNavActionType.Excel,
                           Enable=true,
                           Visible=true},
                           new Personalization.JQNavigatorAction(){
                           Action= Personalization.JQGridNavActionType.Delete,
                           Enable=true,
                           Visible=true},
                        new Personalization.JQNavigatorAction(){
                           Action= Personalization.JQGridNavActionType.Add,
                           Enable= true,
                           Visible=true},
                       new Personalization.JQNavigatorAction(){
                           Action= Personalization.JQGridNavActionType.Edit,
                           Enable=true,
                           Visible=true}};
                    _gridExperimentPlanDetailsParameterOverridesField.ApplyFieldPersonalization();
                    _ndoExperimentPlanDetailsEquipmentField.Enabled = true;
                    _ndoExperimentPlanDetailsEquipmentGroupField.Enabled = true;
                    _ndoExperimentPlanDetailsAutomationPlanField.Enabled = true;
                    _gridExperimentPlanDetailsActionDetailsField.ReadOnly = false;
                    _gridExperimentPlanDetailsActionDetailsField.Enabled = true;
                    (_gridExperimentPlanDetailsActionDetailsField.Settings as GridDataSettingsItemList).EditorSettings.EditingMode = JQEditingModes.Inline;
                    _gridExperimentPlanDetailsActionDetailsField.Settings.NavigatorActions = new Personalization.JQNavigatorAction[] {new Personalization.JQNavigatorAction(){
                           Action= Personalization.JQGridNavActionType.Excel,
                           Enable=true,
                           Visible=true},
                           new Personalization.JQNavigatorAction(){
                           Action= Personalization.JQGridNavActionType.Delete,
                           Enable=true,
                           Visible=true},
                        new Personalization.JQNavigatorAction(){
                           Action= Personalization.JQGridNavActionType.Add,
                           Enable= true,
                           Visible=true},
                       new Personalization.JQNavigatorAction(){
                           Action= Personalization.JQGridNavActionType.Edit,
                           Enable=true,
                           Visible=true} };
                    _gridExperimentPlanDetailsActionDetailsField.ApplyFieldPersonalization();

                    _gridExperimentPlanDetailsSlotWaferInstructionField.ReadOnly = false;
                    _gridExperimentPlanDetailsSlotWaferInstructionField.Enabled = true;
                    (_gridExperimentPlanDetailsSlotWaferInstructionField.Settings as GridDataSettingsItemList).EditorSettings.EditingMode = JQEditingModes.Inline;
                    _gridExperimentPlanDetailsSlotWaferInstructionField.Settings.NavigatorActions = new Personalization.JQNavigatorAction[] {new Personalization.JQNavigatorAction(){
                           Action= Personalization.JQGridNavActionType.Excel,
                           Enable=true,
                           Visible=true},
                           new Personalization.JQNavigatorAction(){
                           Action= Personalization.JQGridNavActionType.Delete,
                           Enable=true,
                           Visible=true},
                        new Personalization.JQNavigatorAction(){
                           Action= Personalization.JQGridNavActionType.Add,
                           Enable= true,
                           Visible=true},
                       new Personalization.JQNavigatorAction(){
                           Action= Personalization.JQGridNavActionType.Edit,
                           Enable=true,
                           Visible=true} };
                    _gridExperimentPlanDetailsSlotWaferInstructionField.ApplyFieldPersonalization();
                }
            }
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            ExperimentPlanChanges serviceDataChanges = (serviceData as ExperimentPlanMaint).ObjectChanges;
            if (serviceDataChanges.ExperimentPlanDetails != null)
            {
                JQDataGrid _gridDetails = Page.FindCamstarControl("ObjectChanges_ExperimentPlanDetails") as JQDataGrid;
                ExperimentPlanDetailsChanges[] oDetails = _gridDetails.BoundContext.Data as ExperimentPlanDetailsChanges[];
                int DetailsCount = serviceDataChanges.ExperimentPlanDetails.Length;
                int oDetailsCount = oDetails.Length;
                var matching = new List<Tuple<int, int>>();
                var delmatching = new List<int>();

                for (int y = 0; y < oDetailsCount; y++)
                {
                    for (int i = 0; i < DetailsCount; i++)
                    {
                        if (serviceDataChanges.ExperimentPlanDetails[i].ListItemAction == ListItemAction.Change)
                        {
                            if ((oDetails[y].ss_Step == serviceDataChanges.ExperimentPlanDetails[i].ss_Step && serviceDataChanges.ExperimentPlanDetails[i].ss_Workflow == null 
                                || oDetails[y].ss_Workflow == serviceDataChanges.ExperimentPlanDetails[i].ss_Workflow && serviceDataChanges.ExperimentPlanDetails[i].ss_Step == null
                                || oDetails[y].ss_Workflow == serviceDataChanges.ExperimentPlanDetails[i].ss_Workflow && oDetails[y].ss_Step == serviceDataChanges.ExperimentPlanDetails[i].ss_Step)
                                ||
                                (oDetails[y].ss_SkipToStep == serviceDataChanges.ExperimentPlanDetails[i].ss_SkipToStep && serviceDataChanges.ExperimentPlanDetails[i].ss_SkipToWorkflow == null
                                || oDetails[y].ss_SkipToWorkflow == serviceDataChanges.ExperimentPlanDetails[i].ss_SkipToWorkflow && serviceDataChanges.ExperimentPlanDetails[i].ss_SkipToStep == null
                                || oDetails[y].ss_SkipToWorkflow == serviceDataChanges.ExperimentPlanDetails[i].ss_SkipToWorkflow && oDetails[y].ss_SkipToStep == serviceDataChanges.ExperimentPlanDetails[i].ss_SkipToStep
                                || serviceDataChanges.ExperimentPlanDetails[i].ss_SkipToWorkflow == null && serviceDataChanges.ExperimentPlanDetails[i].ss_SkipToStep == null)
                                )
                            {
                                matching.Add((new Tuple<int, int>(y, i)));
                            }

                        }

                        if (y == 0)
                        {
                            if (serviceDataChanges.ExperimentPlanDetails[i].ListItemAction == ListItemAction.Delete || serviceDataChanges.ExperimentPlanDetails[i].ListItemAction == ListItemAction.Add)
                            {
                                delmatching.Add(i);
                            }
                        }

                    }

                }


                foreach (var x in matching)
                {
                    serviceDataChanges.ExperimentPlanDetails[x.Item2].ss_Workflow = new RevisionedObjectRef();
                    serviceDataChanges.ExperimentPlanDetails[x.Item2].ss_Workflow = oDetails[x.Item1].ss_Workflow;

                    serviceDataChanges.ExperimentPlanDetails[x.Item2].ss_SkipToWorkflow = new RevisionedObjectRef();
                    serviceDataChanges.ExperimentPlanDetails[x.Item2].ss_SkipToWorkflow = oDetails[x.Item1].ss_SkipToWorkflow;

                    if (serviceDataChanges.ExperimentPlanDetails[x.Item2].ss_Step != null)
                        if (serviceDataChanges.ExperimentPlanDetails[x.Item2].ss_Workflow != null)
                        {
                            NamedSubentityRef WIPStep = new NamedSubentityRef() { Name = (serviceDataChanges.ExperimentPlanDetails[x.Item2].ss_Step as NamedSubentityRef).Name, Parent = serviceDataChanges.ExperimentPlanDetails[x.Item2].ss_Workflow as RevisionedObjectRef };
                            serviceDataChanges.ExperimentPlanDetails[x.Item2].ss_Step = WIPStep;
                        }

                    if (serviceDataChanges.ExperimentPlanDetails[x.Item2].ss_SkipToStep != null)
                        if (serviceDataChanges.ExperimentPlanDetails[x.Item2].ss_SkipToWorkflow != null)
                        {
                            NamedSubentityRef SkipToWIPStep = new NamedSubentityRef() { Name = (serviceDataChanges.ExperimentPlanDetails[x.Item2].ss_SkipToStep as NamedSubentityRef).Name, Parent = serviceDataChanges.ExperimentPlanDetails[x.Item2].ss_SkipToWorkflow as RevisionedObjectRef };
                            serviceDataChanges.ExperimentPlanDetails[x.Item2].ss_SkipToStep = SkipToWIPStep;
                        }
                    if (serviceDataChanges.ExperimentPlanDetails[x.Item2].ActionDetails != null)
                    {
                        int ActionCount = serviceDataChanges.ExperimentPlanDetails[x.Item2].ActionDetails.Length;
                        for (int j = 0; j < ActionCount; j++)
                        {
                            if (serviceDataChanges.ExperimentPlanDetails[x.Item2].ActionDetails[j].ActionType != null && serviceDataChanges.ExperimentPlanDetails[x.Item2].ActionDetails[j].ExpPlanAction != null)
                            {
                                if (serviceDataChanges.ExperimentPlanDetails[x.Item2].ActionDetails[j].ListItemAction != ListItemAction.Delete)
                                {
                                    serviceDataChanges.ExperimentPlanDetails[x.Item2].ActionDetails[j].ExpPlanAction.CDOTypeName = serviceDataChanges.ExperimentPlanDetails[x.Item2].ActionDetails[j].ActionType.ToString();
                                }
                            }
                        }
                    }
                }

                foreach (var y in delmatching)
                {
                    if (serviceDataChanges.ExperimentPlanDetails[y].ListItemAction == ListItemAction.Change)
                    {
                        serviceDataChanges.ExperimentPlanDetails[y].ss_Workflow = new RevisionedObjectRef();
                        serviceDataChanges.ExperimentPlanDetails[y].ss_Workflow = oDetails[int.Parse(serviceDataChanges.ExperimentPlanDetails[y].ListItemIndex.ToString())].ss_Workflow;

                        serviceDataChanges.ExperimentPlanDetails[y].ss_SkipToWorkflow = new RevisionedObjectRef();
                        serviceDataChanges.ExperimentPlanDetails[y].ss_SkipToWorkflow = oDetails[int.Parse(serviceDataChanges.ExperimentPlanDetails[y].ListItemIndex.ToString())].ss_SkipToWorkflow;
                    }

                    if (serviceDataChanges.ExperimentPlanDetails[y].ss_Step != null)
                        if (serviceDataChanges.ExperimentPlanDetails[y].ss_Workflow != null)
                        {
                            NamedSubentityRef WIPStep = new NamedSubentityRef() { Name = (serviceDataChanges.ExperimentPlanDetails[y].ss_Step as NamedSubentityRef).Name, Parent = serviceDataChanges.ExperimentPlanDetails[y].ss_Workflow as RevisionedObjectRef };
                            serviceDataChanges.ExperimentPlanDetails[y].ss_Step = WIPStep;
                        }

                    if (serviceDataChanges.ExperimentPlanDetails[y].ss_SkipToStep != null)
                        if (serviceDataChanges.ExperimentPlanDetails[y].ss_SkipToWorkflow != null)
                        {
                            NamedSubentityRef SkipToWIPStep = new NamedSubentityRef() { Name = (serviceDataChanges.ExperimentPlanDetails[y].ss_SkipToStep as NamedSubentityRef).Name, Parent = serviceDataChanges.ExperimentPlanDetails[y].ss_SkipToWorkflow as RevisionedObjectRef };
                            serviceDataChanges.ExperimentPlanDetails[y].ss_SkipToStep = SkipToWIPStep;
                        }
                    if (serviceDataChanges.ExperimentPlanDetails[y].ActionDetails != null)
                    {
                        int ActionCount = serviceDataChanges.ExperimentPlanDetails[y].ActionDetails.Length;
                        for (int j = 0; j < ActionCount; j++)
                        {
                            if (serviceDataChanges.ExperimentPlanDetails[y].ActionDetails[j].ActionType != null && serviceDataChanges.ExperimentPlanDetails[y].ActionDetails[j].ExpPlanAction != null)
                            {
                                if (serviceDataChanges.ExperimentPlanDetails[y].ActionDetails[j].ListItemAction != ListItemAction.Delete)
                                {
                                    serviceDataChanges.ExperimentPlanDetails[y].ActionDetails[j].ExpPlanAction.CDOTypeName = serviceDataChanges.ExperimentPlanDetails[y].ActionDetails[j].ActionType.ToString();
                                }

                            }
                        }
                    }
                }
            }
        }



        protected override void OnPreRender(EventArgs e)
        {
            CheckIfPlanApproved();

            base.OnPreRender(e);

        }
    }
}




