/* Copyright 2023 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using SEMI.AppCode;
using Camstar.WebPortal.FormsFramework;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebGridControls;

/// <summary>
/// Summary description for SS_ProcessSpecMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class SS_ProcessSpecMaint : MatrixWebPart
    {

        #region Controls
        CWC.DateChooser EffectiveTimestampField { get { return Page.FindCamstarControl("ObjectChanges_EffectiveTimestamp") as CWC.DateChooser; } }
        CWC.TextBox DurationField { get { return Page.FindCamstarControl("ObjectChanges_Duration") as CWC.TextBox; } }
        CWC.CheckBox IsEngineeringField { get { return Page.FindCamstarControl("ObjectChanges_IsEngineering") as CWC.CheckBox; } }
        CWC.CheckBox UseOnlyIfValidField { get { return Page.FindCamstarControl("ObjectChanges_UseOnlyIfValid") as CWC.CheckBox; } }
        CWC.CheckBox SysForceToUpperField { get { return Page.FindCamstarControl("ObjectChanges_SysForceToUpper") as CWC.CheckBox; } }
        CWC.TextBox NameField { get { return Page.FindCamstarControl("NameTxt") as CWC.TextBox; } }

        CWC.WorkflowNavigator _DetailsWfNav { get { return Page.FindCamstarControl("Details_WorkflowNav") as CWC.WorkflowNavigator; } }
        CWC.WorkflowNavigator _ActionsWfNav { get { return Page.FindCamstarControl("Actions_scsWorkflowNav") as CWC.WorkflowNavigator; } }
        CWC.WorkflowNavigator _DetailsSkipToWfNav { get { return Page.FindCamstarControl("Details_SkipToWorkflowNav") as CWC.WorkflowNavigator; } }
        CWC.WorkflowNavigator _DetailsRelatedWfNav { get { return Page.FindCamstarControl("Details_RelatedWorkflowNav") as CWC.WorkflowNavigator; } }
        CWC.WorkflowNavigator _DetailsAggregateWfNav { get { return Page.FindCamstarControl("Details_AggregateWorkflowNav") as CWC.WorkflowNavigator; } }
        CWC.NamedSubentity _DetailsStep { get { return Page.FindCamstarControl("Details_ss_Step") as CWC.NamedSubentity; } }
        CWC.NamedSubentity _ActionsStep { get { return Page.FindCamstarControl("Actions_scsStep") as CWC.NamedSubentity; } }
        CWC.NamedSubentity _DetailsSkipToStep { get { return Page.FindCamstarControl("Details_ss_SkipToStep") as CWC.NamedSubentity; } }
        CWC.NamedSubentity _DetailsRelatedStep { get { return Page.FindCamstarControl("Details_ssRelatedStep") as CWC.NamedSubentity; } }
        CWC.NamedSubentity _DetailsAggregateStep { get { return Page.FindCamstarControl("Details_ssAggregateStep") as CWC.NamedSubentity; } }

        CWC.CheckBox _DetailsUpdateSkipToStackKey { get { return Page.FindCamstarControl("Details_ss_UpdateSkipToStackKey") as CWC.CheckBox; } }
        CWC.CheckBox _DetailsUpdateStackKey { get { return Page.FindCamstarControl("Details_ss_UpdateStackKey") as CWC.CheckBox; } }
        CWC.CheckBox _ActionsUpdateStackKey { get { return Page.FindCamstarControl("Actions_scsUpdateStackKey") as CWC.CheckBox; } }
        CWC.CheckBox _DetailsUpdateRelatedStackKey { get { return Page.FindCamstarControl("Details_ss_UpdateRelatedStackKey") as CWC.CheckBox; } }
        CWC.CheckBox _DetailsUpdateAggregateStackKey { get { return Page.FindCamstarControl("Details_ss_UpdateAggregateStackKey") as CWC.CheckBox; } }

        CWC.RadioButton _UnitsPerHour { get { return Page.FindCamstarControl("UnitsPerHour") as CWC.RadioButton; } }
        CWC.RadioButton _HoursPerUnit { get { return Page.FindCamstarControl("HoursPerUnit") as CWC.RadioButton; } }
        CWC.Duration _Detail_HoursPerUnit { get { return Page.FindCamstarControl("Detail_HoursPerUnit") as CWC.Duration; } }
        CWC.Duration _Detail_SetupTime0 { get { return Page.FindCamstarControl("Detail_SetupTime0") as CWC.Duration; } }
        CWC.TextBox _Detail_UnitsPerHour { get { return Page.FindCamstarControl("Detail_UnitsPerHour") as CWC.TextBox; } }
        CWC.TitleControl _SchedulingSection0 { get { return Page.FindCamstarControl("SchedulingSection0") as CWC.TitleControl; } }
        CWC.TitleControl _RunRateSection { get { return Page.FindCamstarControl("RunRateSection") as CWC.TitleControl; } }


        protected MatrixWebPart wpDetailsWP
        {
            get
            {
                MatrixWebPart wp = null;
                int iParentControlsCount = Parent.Controls.Count;
                for (int z = 0; z < iParentControlsCount; z++)
                {
                    if (Parent.Controls[z].ID == "ProcessSpecDetailsLPWP")
                        wp = Parent.Controls[z] as MatrixWebPart;
                }
                return wp;
            }
        }

        #endregion

        #region PageEvents

        //-----------------------------------------------------------
        //
        //-----------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UIUtility.SetCapital(NameField, SysForceToUpperField.CheckControl.Checked);
            if (PrimaryServiceType.IndexOf("SSMaint") > 0)
            {
                EffectiveTimestampField.ReadOnly = false;
                EffectiveTimestampField.DataSubmissionMode = Personalization.DataSubmissionModeType.NotSet;
                DurationField.ReadOnly = false;
                DurationField.DataSubmissionMode = Personalization.DataSubmissionModeType.NotSet;
                IsEngineeringField.Visible = true;
                IsEngineeringField.ReadOnly = false;
                IsEngineeringField.DataSubmissionMode = Personalization.DataSubmissionModeType.NotSet;
                UseOnlyIfValidField.Visible = true;
                UseOnlyIfValidField.ReadOnly = false;
                UseOnlyIfValidField.DataSubmissionMode = Personalization.DataSubmissionModeType.NotSet;
            }

            _UnitsPerHour.Visible = false;
            _HoursPerUnit.Visible = false;
            _Detail_HoursPerUnit.Visible = false;
            _Detail_SetupTime0.Visible = false;
            _Detail_UnitsPerHour.Visible = false;
            _SchedulingSection0.Visible = false;
            _RunRateSection.Visible = false;

            var stackControl = Page.FindCamstarControl(_DetailsWfNav.ClientID + "_Stack") as FieldControl;
            stackControl.DataSubmissionMode = Personalization.DataSubmissionModeType.Skip;
            _DetailsWfNav.StepControl.DataSubmissionMode = Personalization.DataSubmissionModeType.Skip;
            _DetailsWfNav.StepControl.Hidden = true;

            var stackControl2 = Page.FindCamstarControl(_DetailsSkipToWfNav.ClientID + "_Stack") as FieldControl;
            stackControl2.DataSubmissionMode = Personalization.DataSubmissionModeType.Skip;
            _DetailsSkipToWfNav.StepControl.DataSubmissionMode = Personalization.DataSubmissionModeType.Skip;
            _DetailsSkipToWfNav.StepControl.Hidden = true;

            var stackControl3 = Page.FindCamstarControl(_DetailsRelatedWfNav.ClientID + "_Stack") as FieldControl;
            stackControl3.DataSubmissionMode = Personalization.DataSubmissionModeType.Skip;
            _DetailsRelatedWfNav.StepControl.DataSubmissionMode = Personalization.DataSubmissionModeType.Skip;
            _DetailsRelatedWfNav.StepControl.Hidden = true;

            var stackControl4 = Page.FindCamstarControl(_DetailsAggregateWfNav.ClientID + "_Stack") as FieldControl;
            stackControl4.DataSubmissionMode = Personalization.DataSubmissionModeType.Skip;
            _DetailsAggregateWfNav.StepControl.DataSubmissionMode = Personalization.DataSubmissionModeType.Skip;
            _DetailsAggregateWfNav.StepControl.Hidden = true;

            var stackControl5 = Page.FindCamstarControl(_ActionsWfNav.ClientID + "_Stack") as FieldControl;
            stackControl.DataSubmissionMode = Personalization.DataSubmissionModeType.Skip;
            _ActionsWfNav.StepControl.DataSubmissionMode = Personalization.DataSubmissionModeType.Skip;
            _ActionsWfNav.StepControl.Hidden = true;

            _DetailsWfNav.DataChanged += _DetailsWfNav_DataChanged;
            //_DetailsSkipToWfNav.DataChanged += _DetailsSkipToWfNav_DataChanged;
            _DetailsWfNav.StepControl.DataChanged += _DetailsWfNav_DataChanged;
            _ActionsWfNav.DataChanged += _ActionsWfNav_DataChanged;
            //_DetailsSkipToWfNav.DataChanged += _DetailsSkipToWfNav_DataChanged;
            _ActionsWfNav.StepControl.DataChanged += _ActionsWfNav_DataChanged;
            _DetailsSkipToWfNav.StepControl.DataChanged += _DetailsSkipToWfNav_DataChanged;
            _DetailsRelatedWfNav.StepControl.DataChanged += _DetailsRelatedWfNavStepControl_DataChanged;
            _DetailsAggregateWfNav.StepControl.DataChanged += _DetailsAggregateWfNavStepControl_DataChanged;

            _DetailsUpdateSkipToStackKey.Hidden = true;
            _DetailsUpdateStackKey.Hidden = true;
            _ActionsUpdateStackKey.Hidden = true;
            _DetailsUpdateRelatedStackKey.Hidden = true;
            _DetailsUpdateAggregateStackKey.Hidden = true;

            if (_DetailsRelatedWfNav.Data == null)
            {
                _DetailsRelatedStep.Data = null;
                _DetailsUpdateRelatedStackKey.Data = true;
                _DetailsUpdateRelatedStackKey.CheckControl.Checked = true;
            }

            if (_DetailsSkipToWfNav.Data == null)
            {
                _DetailsSkipToStep.Data = null;
                _DetailsUpdateSkipToStackKey.Data = true;
                _DetailsUpdateSkipToStackKey.CheckControl.Checked = true;
            }

            if (_DetailsWfNav.Data == null)
            {
                _DetailsStep.Data = null;
                _DetailsUpdateStackKey.Data = true;
                _DetailsUpdateStackKey.CheckControl.Checked = true;
            }

            if (_ActionsWfNav.Data == null)
            {
                _ActionsStep.Data = null;
                _ActionsUpdateStackKey.Data = true;
                _ActionsUpdateStackKey.CheckControl.Checked = true;
            }

            if (_DetailsAggregateWfNav.Data == null)
            {
                _DetailsAggregateStep.Data = null;
                _DetailsUpdateAggregateStackKey.Data = true;
                _DetailsUpdateAggregateStackKey.CheckControl.Checked = true;
            }

            if (wpDetailsWP != null)
                wpDetailsWP.Hidden = true;
        }

        //-----------------------------------------------------------
        //
        //-----------------------------------------------------------
        void _DetailsRelatedWfNavStepControl_DataChanged(object sender, EventArgs e)
        {
            var stackControl = Page.FindCamstarControl(_DetailsRelatedWfNav.ClientID + "_Stack") as FieldControl;
            NamedSubentityRef[] stack = (stackControl.Data) as NamedSubentityRef[];
            JQDataGrid grdStack = Page.FindCamstarControl("Details_ssRelatedWorkflowStack") as JQDataGrid;

            if (stack != null)
                grdStack.Data = stack;
            else
                grdStack.Data = null;

            if (_DetailsRelatedWfNav.StepControl.Data != null)
                _DetailsRelatedStep.Data = _DetailsRelatedWfNav.StepControl.Data;

            _DetailsUpdateRelatedStackKey.Data = true;
            _DetailsUpdateRelatedStackKey.CheckControl.Checked = true;

            stackControl.Data = null;
            CamstarWebControl.SetRenderToClient(grdStack);
            CamstarWebControl.SetRenderToClient(_DetailsRelatedStep);
        }

        //-----------------------------------------------------------
        //
        //-----------------------------------------------------------
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
            else
                _DetailsSkipToStep.Data = null;

            _DetailsUpdateSkipToStackKey.Data = true;
            _DetailsUpdateSkipToStackKey.CheckControl.Checked = true;

            stackControl.Data = null;
            CamstarWebControl.SetRenderToClient(grdStack);
            CamstarWebControl.SetRenderToClient(_DetailsSkipToStep);
        }

        //-----------------------------------------------------------
        //
        //-----------------------------------------------------------
        void _DetailsWfNav_DataChanged(object sender, EventArgs e)
        {
            var stackControl = Page.FindCamstarControl(_DetailsWfNav.ClientID + "_Stack") as FieldControl;
            NamedSubentityRef[] stack = (stackControl.Data) as NamedSubentityRef[];
            JQDataGrid grdStack = Page.FindCamstarControl("Details_ssWorkflowStack") as JQDataGrid;

            if (stack != null)
                grdStack.Data = stack;
            else
                grdStack.Data = null;

            if (_DetailsWfNav.StepControl.Data != null)
                _DetailsStep.Data = _DetailsWfNav.StepControl.Data;
            else
                if (_DetailsWfNav.StepControl.Data == null && _DetailsWfNav.Data == null) _DetailsStep.Data = null;

            _DetailsUpdateStackKey.Data = true;
            _DetailsUpdateStackKey.CheckControl.Checked = true;

            stackControl.Data = null;
            CamstarWebControl.SetRenderToClient(grdStack);
            CamstarWebControl.SetRenderToClient(_DetailsStep);
        }

        //-----------------------------------------------------------
        //
        //-----------------------------------------------------------
        void _ActionsWfNav_DataChanged(object sender, EventArgs e)
        {
            var stackControl = Page.FindCamstarControl(_ActionsWfNav.ClientID + "_Stack") as FieldControl;
            NamedSubentityRef[] stack = (stackControl.Data) as NamedSubentityRef[];
            JQDataGrid grdStack = Page.FindCamstarControl("Actions_scsWorkflowStack") as JQDataGrid;

            if (stack != null)
                grdStack.Data = stack;
            else
                grdStack.Data = null;

            if (_ActionsWfNav.StepControl.Data != null)
                _ActionsStep.Data = _ActionsWfNav.StepControl.Data;
            else
                if (_ActionsWfNav.StepControl.Data == null && _ActionsWfNav.Data == null) _ActionsStep.Data = null;

            _ActionsUpdateStackKey.Data = true;
            _ActionsUpdateStackKey.CheckControl.Checked = true;

            stackControl.Data = null;
            CamstarWebControl.SetRenderToClient(grdStack);
            CamstarWebControl.SetRenderToClient(_ActionsStep);
        }

        //-----------------------------------------------------------
        //
        //-----------------------------------------------------------
        void _DetailsAggregateWfNavStepControl_DataChanged(object sender, EventArgs e)
        {
            var stackControl = Page.FindCamstarControl(_DetailsAggregateWfNav.ClientID + "_Stack") as FieldControl;
            NamedSubentityRef[] stack = (stackControl.Data) as NamedSubentityRef[];
            JQDataGrid grdStack = Page.FindCamstarControl("Details_ssAggregateWorkflowStack") as JQDataGrid;

            if (stack != null)
                grdStack.Data = stack;
            else
                grdStack.Data = null;

            if (_DetailsAggregateWfNav.StepControl.Data != null)
                _DetailsAggregateStep.Data = _DetailsAggregateWfNav.StepControl.Data;
            else
                _DetailsAggregateStep.Data = null;

            _DetailsUpdateAggregateStackKey.Data = true;
            _DetailsUpdateAggregateStackKey.CheckControl.Checked = true;

            stackControl.Data = null;
            CamstarWebControl.SetRenderToClient(grdStack);
            CamstarWebControl.SetRenderToClient(_DetailsAggregateStep);
        }

        //-----------------------------------------------------------
        //
        //-----------------------------------------------------------
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            int gridDetailsCount = 0;
            if ((serviceData != null) && ((serviceData as ProcessSpecMaint).ObjectChanges != null) && ((serviceData as ProcessSpecMaint).ObjectChanges.Details != null))
            {
                JQDataGrid _gridDetails = Page.FindCamstarControl("ObjectChanges_Details") as JQDataGrid;
                ProcessSpecDetailsChanges[] oDetails = _gridDetails.BoundContext.Data as ProcessSpecDetailsChanges[];

                foreach (ProcessSpecDetailsChanges oDetailsChanges in (serviceData as ProcessSpecMaint).ObjectChanges.Details)
                {
                    if (oDetailsChanges.ListItemAction == ListItemAction.Change)
                    {
                        oDetailsChanges.ss_Workflow = new RevisionedObjectRef("");
                        if (oDetails[gridDetailsCount].ss_Workflow != null)
                            oDetailsChanges.ss_Workflow = oDetails[gridDetailsCount].ss_Workflow;

                        oDetailsChanges.ss_SkipToWorkflow = new RevisionedObjectRef("");
                        if (oDetails[gridDetailsCount].ss_SkipToWorkflow != null)
                            oDetailsChanges.ss_SkipToWorkflow = oDetails[gridDetailsCount].ss_SkipToWorkflow;

                        oDetailsChanges.ss_RelatedWorkflow = new RevisionedObjectRef("");
                        if (oDetails[gridDetailsCount].ss_RelatedWorkflow != null)
                            oDetailsChanges.ss_RelatedWorkflow = oDetails[gridDetailsCount].ss_RelatedWorkflow;

                        oDetailsChanges.ss_AggregateWorkflow = new RevisionedObjectRef("");
                        if (oDetails[gridDetailsCount].ss_AggregateWorkflow != null)
                            oDetailsChanges.ss_AggregateWorkflow = oDetails[gridDetailsCount].ss_AggregateWorkflow;
                        gridDetailsCount++;
                    }

                    if (oDetailsChanges.ss_Step != null)
                        if (oDetailsChanges.ss_Workflow != null)
                        {
                            NamedSubentityRef WIPStep = new NamedSubentityRef() { Name = (oDetailsChanges.ss_Step as NamedSubentityRef).Name, Parent = oDetailsChanges.ss_Workflow as RevisionedObjectRef };
                            oDetailsChanges.ss_Step = WIPStep;
                        }

                    if (oDetailsChanges.ss_SkipToStep != null)
                        if (oDetailsChanges.ss_SkipToWorkflow != null)
                        {
                            NamedSubentityRef SkipToWIPStep = new NamedSubentityRef() { Name = (oDetailsChanges.ss_SkipToStep as NamedSubentityRef).Name, Parent = oDetailsChanges.ss_SkipToWorkflow as RevisionedObjectRef };
                            oDetailsChanges.ss_SkipToStep = SkipToWIPStep;
                        }

                    if (oDetailsChanges.ss_RelatedStep != null)
                        if (oDetailsChanges.ss_RelatedWorkflow != null)
                        {
                            NamedSubentityRef RelatedWIPStep = new NamedSubentityRef() { Name = (oDetailsChanges.ss_RelatedStep as NamedSubentityRef).Name, Parent = oDetailsChanges.ss_RelatedWorkflow as RevisionedObjectRef };
                            oDetailsChanges.ss_RelatedStep = RelatedWIPStep;
                        }

                    if (oDetailsChanges.ss_AggregateStep != null)
                        if (oDetailsChanges.ss_AggregateWorkflow != null)
                        {
                            NamedSubentityRef AggregateWIPStep = new NamedSubentityRef() { Name = (oDetailsChanges.ss_AggregateStep as NamedSubentityRef).Name, Parent = oDetailsChanges.ss_AggregateWorkflow as RevisionedObjectRef };
                            oDetailsChanges.ss_AggregateStep = AggregateWIPStep;
                        }

                    if (oDetailsChanges.Actions != null)
                    {
                        JQDataGrid _gridActions = Page.FindCamstarControl("Details_Actions") as JQDataGrid;
                        ProcessSpecDetailsActionChang[] oActions = _gridActions.BoundContext.Data as ProcessSpecDetailsActionChang[];
                        int countDetailsAction = 0;
                        foreach (ProcessSpecDetailsActionChang oDetailsActionsChanges in oDetailsChanges.Actions)
                        {
                            if (oDetailsActionsChanges.ListItemAction == ListItemAction.Change)
                            {
                                oDetailsActionsChanges.scsWorkflow = new RevisionedObjectRef("");
                                if (oActions[countDetailsAction].scsWorkflow != null)
                                    oDetailsActionsChanges.scsWorkflow = oActions[countDetailsAction].scsWorkflow;
                                countDetailsAction++;
                            }
                            if (oDetailsActionsChanges.scsStep != null)
                                if (oDetailsActionsChanges.scsWorkflow != null)
                                {
                                    NamedSubentityRef WIPStep = new NamedSubentityRef() { Name = (oDetailsActionsChanges.scsStep as NamedSubentityRef).Name, Parent = oDetailsActionsChanges.scsWorkflow as RevisionedObjectRef };
                                    oDetailsActionsChanges.scsStep = WIPStep;
                                }
                        }
                    }
                }
            }
        }
        #endregion
    }
}