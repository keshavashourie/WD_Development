/* Copyright 2019 Siemens */
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    /// <summary>
    /// 
    /// </summary>
    public class SS_ProcessTimer : ProcessTimer
    {
        #region Controls

        protected virtual CWC.RadioButtonList TimerTypesList
        {
            get { return Page.FindCamstarControl("TimerType") as CWC.RadioButtonList; }
        }
        protected virtual CWC.Duration MinWarningTime
        {
            get { return Page.FindCamstarControl("MinWarningTime") as CWC.Duration; }
        }
        protected virtual CWC.ColorPicker MinWarningTimeColor
        {
            get { return Page.FindCamstarControl("MinWarningTimeColor") as CWC.ColorPicker; }
        }
        protected virtual CWC.Duration MinTime
        {
            get { return Page.FindCamstarControl("MinTime") as CWC.Duration; }
        }
        protected virtual CWC.ColorPicker MinTimeColor
        {
            get { return Page.FindCamstarControl("MinTimeColor") as CWC.ColorPicker; }
        }
        protected virtual CWC.Duration MaxWarningTime
        {
            get { return Page.FindCamstarControl("MaxWarningTime") as CWC.Duration; }
        }
        protected virtual CWC.ColorPicker MaxWarningTimeColor
        {
            get { return Page.FindCamstarControl("MaxWarningTimeColor") as CWC.ColorPicker; }
        }
        protected virtual CWC.Duration MaxTime
        {
            get { return Page.FindCamstarControl("MaxTime") as CWC.Duration; }
        }
        protected virtual CWC.ColorPicker MaxTimeColor
        {
            get { return Page.FindCamstarControl("MaxTimeColor") as CWC.ColorPicker; }
        }
        protected virtual CWC.RadioButtonList ProcessTimerMinTimeDtl_TimerAction
        {
            get { return Page.FindCamstarControl("ProcessTimerMinTimeDtl_TimerAction") as CWC.RadioButtonList; }
        }
        protected virtual CheckBox IsProductionEventMin
        {
            get { return Page.FindCamstarControl("ProcessTimerMinTimeDtl_IsProductionEvent") as CheckBox; }
        }
        protected virtual NamedObject DefaultFailureModeMin
        {
            get { return Page.FindCamstarControl("ObjectChanges_DefaultFailureMode") as NamedObject; }
        }
        protected virtual NamedObject ClassificationMin
        {
            get { return Page.FindCamstarControl("ObjectChanges_Classification") as NamedObject; }
        }
        protected virtual NamedObject SubClassificationMin
        {
            get { return Page.FindCamstarControl("ObjectChanges_SubClassification") as NamedObject; }
        }
        protected virtual TextBox DefaultPEDescriptionMin
        {
            get { return Page.FindCamstarControl("ObjectChanges_DefaultPEDescription") as TextBox; }
        }
        protected virtual CheckBox IsReworkMin
        {
            get { return Page.FindCamstarControl("ProcessTimerMinTimeDtl_IsRework") as CheckBox; }
        }
        protected virtual WorkflowNavigator ToReworkWorkflowMin
        {
            get { return Page.FindCamstarControl("ProcessTimerMinTimeDtl_ToReworkWorkflow") as WorkflowNavigator; }
        }        
        protected virtual WorkflowNavigator MoveNonStdWorkflowMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_ToWorkflow") as WorkflowNavigator; }
        }
        protected virtual WorkflowNavigator MoveNonStdWorkflowMin
        {
            get { return Page.FindCamstarControl("ToWorkflow") as WorkflowNavigator; }
        }
        protected virtual NamedObject ReworkReasonMin
        {
            get { return Page.FindCamstarControl("ProcessTimerMinTimeDtl_ReworkReason") as NamedObject; }
        }
        protected virtual CheckBox IsMoveNonStdMin
        {
            get { return Page.FindCamstarControl("ProcessTimerMinTimeDtl_IsMoveNonStd") as CheckBox; }
        }
        protected virtual CheckBox IsYieldOffRejectMin
        {
            get { return Page.FindCamstarControl("ProcessTimerMinTimeDtl_ss_YieldOffRejects") as CheckBox; }
        }
        protected virtual CheckBox IsSplitBinsMin
        {
            get { return Page.FindCamstarControl("ProcessTimerMinTimeDtl_ss_SplitBins") as CheckBox; }
        }
        
        protected virtual WorkflowNavigator ToWorkflowMin
        {
            get { return Page.FindCamstarControl("ToWorkflow") as WorkflowNavigator; }
        }
        protected virtual CheckBox IsHoldMin
        {
            get { return Page.FindCamstarControl("ProcessTimerMinTimeDtl_IsHold") as CheckBox; }
        }
        protected virtual NamedObject HoldReasonMin
        {
            get { return Page.FindCamstarControl("ProcessTimerMinTimeDtl_HoldReason") as NamedObject; }
        }
        protected virtual CheckBox IsFutureHoldMin
        {
            get { return Page.FindCamstarControl("ProcessTimerMinTimeDtl_ss_IsFutureHold") as CheckBox; }
        }
        protected virtual NamedObject FailureFutureHoldSetupMin
        {
            get { return Page.FindCamstarControl("ProcessTimerMinTimeDtl_ss_FailureFutureHoldSetup") as NamedObject; }
        }
        protected virtual CheckBox IsBusinessRuleMin
        {
            get { return Page.FindCamstarControl("ProcessTimerMinTimeDtl_IsBusinessRule") as CheckBox; }
        }
        protected virtual NamedObject BusinessRuleMin
        {
            get { return Page.FindCamstarControl("ProcessTimerMinTimeDtl_BusinessRule") as NamedObject; }
        }

        protected virtual CWC.RadioButtonList ProcessTimerMaxTimeDtl_TimerAction
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_TimerAction") as CWC.RadioButtonList; }
        }
        protected virtual CheckBox IsProductionEventMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_IsProductionEvent") as CheckBox; }
        }
        protected virtual NamedObject DefaultFailureModeMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_DefaultFailureMode") as NamedObject; }
        }
        protected virtual NamedObject ClassificationMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_Classification") as NamedObject; }
        }
        protected virtual NamedObject SubClassificationMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_SubClassification") as NamedObject; }
        }
        protected virtual TextBox DefaultPEDescriptionMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_DefaultPEDescription") as TextBox; }
        }
        protected virtual CheckBox IsReworkMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_IsRework") as CheckBox; }
        }
        protected virtual WorkflowNavigator ToReworkWorkflowMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_ToReworkWorkflow") as WorkflowNavigator; }
        }
        protected virtual NamedObject ReworkReasonMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_ReworkReason") as NamedObject; }
        }
        protected virtual CheckBox IsMoveNonStdMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_IsMoveNonStd") as CheckBox; }
        }
        protected virtual CheckBox IsYieldOffRejectMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_ss_YieldOffRejects") as CheckBox; }
        }
        protected virtual CheckBox IsSplitBinsMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_ss_SplitBins") as CheckBox; }
        }
        protected virtual WorkflowNavigator ToWorkflowMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_ToWorkflow") as WorkflowNavigator; }
        }
        protected virtual CheckBox IsHoldMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_IsHold") as CheckBox; }
        }
        protected virtual NamedObject HoldReasonMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_HoldReason") as NamedObject; }
        }
        protected virtual CheckBox IsFutureHoldMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_ss_IsFutureHold") as CheckBox; }
        }
        protected virtual NamedObject FailureFutureHoldSetupMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_ss_FailureFutureHoldSetup") as NamedObject; }
        }
        protected virtual CheckBox IsBusinessRuleMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_IsBusinessRule") as CheckBox; }
        }
        protected virtual NamedObject BusinessRuleMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_BusinessRule") as NamedObject; }
        }
        protected virtual NamedObject MinTimeEsig
        {
            get { return Page.FindCamstarControl("ProcessTimerMinTimeDtl_ESigRequirement") as NamedObject; }
        }
        protected virtual NamedObject MaxTimeEsig
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_ESigRequirement") as NamedObject; }
        }

        protected virtual CheckBox IsConfirmationMin
        {
            get { return Page.FindCamstarControl("ProcessTimerMinTimeDtl_IsConfirmation") as CheckBox; }
        }

        protected virtual CheckBox IsConfirmationMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_IsConfirmation") as CheckBox; }
        }

        protected virtual JQDataGrid StartProcessTimerMapDtl
        {
            get { return Page.FindCamstarControl("ObjectChanges_StartProcessTimerMapDtl") as JQDataGrid; }
        }

        protected virtual JQDataGrid EndProcessTimerMapDtl
        {
            get { return Page.FindCamstarControl("ObjectChanges_EndProcessTimerMapDtl") as JQDataGrid; }
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (IsFutureHoldMax.CheckControl.Checked && !IsFutureHoldMax.IsChecked)
            {
                IsFutureHoldMax.Data = IsFutureHoldMax.IsChecked;
                IsFutureHoldMax_DataChanged(null, null);
            }

            if (IsFutureHoldMin.CheckControl.Checked && !IsFutureHoldMin.IsChecked)
            {
                IsFutureHoldMin.Data = IsFutureHoldMin.IsChecked;
                IsFutureHoldMin_DataChanged(null, null);
            }

            IsFutureHoldMax.DataChanged += IsFutureHoldMax_DataChanged;
            IsFutureHoldMin.DataChanged += IsFutureHoldMin_DataChanged;
        }

        protected override void IsMoveNonStdMax_DataChanged(object sender, EventArgs e)
        {
            base.IsMoveNonStdMax_DataChanged(sender, e);

            if (IsMoveNonStdMax.IsChecked)
            {
                ToWorkflowMax.Required = true;
            }
            else if (!IsMoveNonStdMax.IsChecked)
            {
                ToWorkflowMax.Required = false;
                ToWorkflowMax.ClearData();
            }
        }

        protected override void IsMoveNonStdMin_DataChanged(object sender, EventArgs e)
        {
            base.IsMoveNonStdMin_DataChanged(sender, e);

            if (IsMoveNonStdMin.IsChecked)
            {
                ToWorkflowMin.Required = true;
            }
            else if (!IsMoveNonStdMin.IsChecked)
            {
                ToWorkflowMin.Required = false;
                ToWorkflowMin.ClearData();
            }
        }

        protected void IsFutureHoldMax_DataChanged(object sender, EventArgs e)
        {
            if (IsFutureHoldMax.IsChecked)
            {
                FailureFutureHoldSetupMax.Required = true;
            }
            else if (!IsFutureHoldMax.IsChecked)
            {
                FailureFutureHoldSetupMax.Required = false;
                FailureFutureHoldSetupMax.ClearData();
            }
        }

        protected void IsFutureHoldMin_DataChanged(object sender, EventArgs e)
        {
            if (IsFutureHoldMin.IsChecked)
            {
                FailureFutureHoldSetupMin.Required = true;
            }
            else if (!IsFutureHoldMin.IsChecked)
            {
                FailureFutureHoldSetupMin.Required = false;
                FailureFutureHoldSetupMin.ClearData();
            }
        }


    }
}
