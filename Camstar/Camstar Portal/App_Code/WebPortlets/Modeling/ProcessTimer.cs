// Copyright Siemens 2023  
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
    /// Used on the Server Catalog Maint VP to encrypt the password
    /// </summary>
    public class ProcessTimer : MatrixWebPart
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
        protected virtual NamedObject ReworkReasonMin
        {
            get { return Page.FindCamstarControl("ProcessTimerMinTimeDtl_ReworkReason") as NamedObject; }
        }
        protected virtual CheckBox IsMoveNonStdMin
        {
            get { return Page.FindCamstarControl("ProcessTimerMinTimeDtl_IsMoveNonStd") as CheckBox; }
        }
        protected virtual NamedObject ResourceGroupMin
        {
            get { return Page.FindCamstarControl("ObjectChanges_ResourceGroup") as NamedObject; }
        }
        protected virtual NamedObject ResourceMin
        {
            get { return Page.FindCamstarControl("ObjectChanges_Resource") as NamedObject; }
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
        protected virtual NamedObject ReworkReasonMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_ReworkReason") as NamedObject; }
        }
        protected virtual CheckBox IsMoveNonStdMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_IsMoveNonStd") as CheckBox; }
        }
        protected virtual NamedObject ResourceGroupMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_ResourceGroup") as NamedObject; }
        }
        protected virtual NamedObject ResourceMax
        {
            get { return Page.FindCamstarControl("ProcessTimerMaxTimeDtl_Resource") as NamedObject; }
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

        #region Protected Functions
        #endregion

        #region Public Functions        

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            StartProcessTimerMapDtl.GridContext.GridReloading += GridContext_GridReloading;
            EndProcessTimerMapDtl.GridContext.GridReloading += GridContext_GridReloading;

            if (IsProductionEventMin.CheckControl.Checked && !IsProductionEventMin.IsChecked)
            {
                IsProductionEventMin.Data = IsProductionEventMin.IsChecked;
                IsProductionEventMin_DataChanged(null, null);
            }

            if (IsProductionEventMax.CheckControl.Checked && !IsProductionEventMax.IsChecked)
            {
                IsProductionEventMax.Data = IsProductionEventMax.IsChecked;
                IsProductionEventMax_DataChanged(null, null);
            }
            if (IsReworkMin.CheckControl.Checked && !IsReworkMin.IsChecked)
            {
                IsReworkMin.Data = IsReworkMin.IsChecked;
                IsReworkMin_DataChanged(null, null);
            }
            if (IsReworkMax.CheckControl.Checked && !IsReworkMax.IsChecked)
            {
                IsReworkMax.Data = IsReworkMax.IsChecked;
                IsReworkMax_DataChanged(null, null);
            }
            if (IsMoveNonStdMin.CheckControl.Checked && !IsMoveNonStdMin.IsChecked)
            {
                IsMoveNonStdMin.Data = IsMoveNonStdMin.IsChecked;
                IsMoveNonStdMin_DataChanged(null, null);
            }
            if (IsMoveNonStdMax.CheckControl.Checked && !IsMoveNonStdMax.IsChecked)
            {
                IsMoveNonStdMax.Data = IsMoveNonStdMax.IsChecked;
                IsMoveNonStdMax_DataChanged(null, null);
            }
            if (IsHoldMin.CheckControl.Checked && !IsHoldMin.IsChecked)
            {
                IsHoldMin.Data = IsHoldMin.IsChecked;
                IsHoldMin_DataChanged(null, null);
            }            
            if (IsHoldMax.CheckControl.Checked && !IsHoldMax.IsChecked)
            {
                IsHoldMax.Data = IsHoldMax.IsChecked;
                IsHoldMax_DataChanged(null, null);
            }
            if (IsBusinessRuleMin.CheckControl.Checked && !IsBusinessRuleMin.IsChecked)
            {
                IsBusinessRuleMin.Data = IsBusinessRuleMin.IsChecked;
                IsBusinessRuleMin_DataChanged(null, null);
            }
            if (IsBusinessRuleMax.CheckControl.Checked && !IsBusinessRuleMax.IsChecked)
            {
                IsBusinessRuleMax.Data = IsBusinessRuleMax.IsChecked;
                IsBusinessRuleMax_DataChanged(null, null);
            }

            //IsProductionEventMin.CheckControl.Attributes["onclick"] = string.Format("onProcessTimerCheckBoxClick(this,['{0}','{1}', '{2}', '{3}']);", DefaultFailureModeMin.ClientID, ClassificationMin.ClientID, SubClassificationMin.ClientID, DefaultPEDescriptionMin.ClientID);
            //IsReworkMin.CheckControl.Attributes["onclick"] = string.Format("onProcessTimerCheckBoxClick(this,['{0}']);", ReworkReasonMin.ClientID);
            //IsMoveNonStdMin.CheckControl.Attributes["onclick"] = string.Format("onProcessTimerCheckBoxClick(this,['{0}','{1}', '{2}']);", ResourceGroupMin.ClientID, ResourceMin.ClientID, ToWorkflowMin.ClientID);
            //IsHoldMin.CheckControl.Attributes["onclick"] = string.Format("onProcessTimerCheckBoxClick(this,['{0}']);", HoldReasonMin.ClientID);

            //IsProductionEventMax.CheckControl.Attributes["onclick"] = string.Format("onProcessTimerCheckBoxClick(this,['{0}','{1}', '{2}', '{3}']);", DefaultFailureModeMax.ClientID, ClassificationMax.ClientID, SubClassificationMax.ClientID, DefaultPEDescriptionMax.ClientID);
            //IsReworkMax.CheckControl.Attributes["onclick"] = string.Format("onProcessTimerCheckBoxClick(this,['{0}']);", ReworkReasonMax.ClientID);
            //IsMoveNonStdMax.CheckControl.Attributes["onclick"] = string.Format("onProcessTimerCheckBoxClick(this,['{0}','{1}', '{2}']);", ResourceGroupMax.ClientID, ResourceMax.ClientID, ToWorkflowMax.ClientID);
            //IsHoldMax.CheckControl.Attributes["onclick"] = string.Format("onProcessTimerCheckBoxClick(this,['{0}']);", HoldReasonMax.ClientID);

            TimerTypesList.SelectedIndexChanged += TimerTypesList_SelectedIndexChanged;
            ProcessTimerMinTimeDtl_TimerAction.SelectedIndexChanged += ProcessTimerMinTimeDtl_TimerAction_SelectedIndexChanged;
            ProcessTimerMaxTimeDtl_TimerAction.SelectedIndexChanged += ProcessTimerMaxTimeDtl_TimerAction_SelectedIndexChanged;

            IsProductionEventMin.DataChanged += IsProductionEventMin_DataChanged;
            IsProductionEventMax.DataChanged += IsProductionEventMax_DataChanged;            

            IsReworkMin.DataChanged += IsReworkMin_DataChanged;
            IsReworkMax.DataChanged += IsReworkMax_DataChanged;

            IsMoveNonStdMin.DataChanged += IsMoveNonStdMin_DataChanged;
            IsMoveNonStdMax.DataChanged += IsMoveNonStdMax_DataChanged;
            
            IsHoldMin.DataChanged += IsHoldMin_DataChanged;
            IsHoldMax.DataChanged += IsHoldMax_DataChanged;

            IsBusinessRuleMin.DataChanged += IsBusinessRuleMin_DataChanged;
            IsBusinessRuleMax.DataChanged += IsBusinessRuleMax_DataChanged;

            //ProcessTimerMinTimeDtl_TimerAction.AutoPostBack = false;
            //ProcessTimerMaxTimeDtl_TimerAction.AutoPostBack = false;

            //ProcessTimerMinTimeDtl_TimerAction.ListControl.Attributes["onchange"] = string.Format("onAllowTxnClick(this, ['{0}','{1}','{2}','{3}'])", IsProductionEventMin.ClientID, IsReworkMin.ClientID, IsMoveNonStdMin.ClientID, IsHoldMin.ClientID);
            //ProcessTimerMaxTimeDtl_TimerAction.ListControl.Attributes["onchange"] = string.Format("onAllowTxnClick(this, ['{0}','{1}','{2}','{3}'])", IsProductionEventMax.ClientID, IsReworkMax.ClientID, IsMoveNonStdMax.ClientID, IsHoldMax.ClientID);
            var scr = String.Format("SetConfirmationCheckBoxes({0},{1},{2},{3});",
                "'input[id=" + ProcessTimerMinTimeDtl_TimerAction.ClientID + "_ctl00_1]'",
                "'input[id=" + ProcessTimerMaxTimeDtl_TimerAction.ClientID + "_ctl00_1]'",
                "'#" + IsConfirmationMin.ClientID + "'",
                "'#" + IsConfirmationMax.ClientID + "'");
            ScriptManager.RegisterStartupScript(Page.Form, GetType(), "SetConfirmationControls", scr, true);
        }

        protected virtual ResponseData GridContext_GridReloading(object sender, JQGridEventArgs args)
        {
            var modelingContext = Page.PortalContext as MaintenanceBehaviorContext;

            bool isStartProcessTimerGrid = (sender == StartProcessTimerMapDtl.GridContext);
            bool isEndProcessTimerGrid = (sender == EndProcessTimerMapDtl.GridContext);

            if (modelingContext != null)
            {
                var currentModelingObject = modelingContext.Current as RevisionedObjectRef;                
                var serv = new ProcessTimerMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);                
                var data = new ProcessTimerMaint();                
                data.ObjectChanges = new ProcessTimerChanges();
                data.ObjectToChange = currentModelingObject;

                var request = new ProcessTimerMaint_Request();
                request.Info = new ProcessTimerMaint_Info();
                request.Info.ObjectChanges = new ProcessTimerChanges_Info();
                if (isStartProcessTimerGrid)
                {
                    request.Info.ObjectChanges.StartProcessTimerMapDtl = new ProcessTimerMapDtlChanges_Info();
                    request.Info.ObjectChanges.StartProcessTimerMapDtl.RequestValue = true;
                }
                else if (isEndProcessTimerGrid)
                {
                    request.Info.ObjectChanges.EndProcessTimerMapDtl = new ProcessTimerMapDtlChanges_Info();
                    request.Info.ObjectChanges.EndProcessTimerMapDtl.RequestValue = true;
                }

                var result = new ProcessTimerMaint_Result();                
                var res = serv.Load(data, request, out result);
                if (res.IsSuccess)
                {
                    if (result != null)
                    {
                        if (isStartProcessTimerGrid)
                            (StartProcessTimerMapDtl.GridContext as ItemDataContext).Data = result.Value.ObjectChanges.StartProcessTimerMapDtl;
                        else if (isEndProcessTimerGrid)
                            (EndProcessTimerMapDtl.GridContext as ItemDataContext).Data = result.Value.ObjectChanges.EndProcessTimerMapDtl;                      
                    }
                }
                else
                    DisplayMessage(res);
            }
            
            
            return null;
        }        

        public override void DisplayValues(Service serviceData)
        {
            base.DisplayValues(serviceData);
            TimerTypesList_SelectedIndexChanged(null, null);
            ProcessTimerMinTimeDtl_TimerAction_SelectedIndexChanged(null, null);
            ProcessTimerMaxTimeDtl_TimerAction_SelectedIndexChanged(null, null);
        }

        public virtual void TimerTypesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            MinWarningTime.Enabled = true;
            MinWarningTimeColor.Enabled = true;
            MinTime.Enabled = true;
            MinTimeColor.Enabled = true;
            MaxWarningTime.Enabled = true;
            MaxWarningTimeColor.Enabled = true;
            MaxTime.Enabled = true;
            MaxTimeColor.Enabled = true;

            MinTime.Required = true;
            MaxTime.Required = true;

            var type = (TimerTypeEnum)Enum.Parse(typeof(TimerTypeEnum), TimerTypesList.Data as string);
            if (type == TimerTypeEnum.Min)
            {
                MaxWarningTime.ClearData();
                MaxWarningTimeColor.ClearData();
                MaxTime.ClearData();
                MaxTimeColor.ClearData();
                MaxWarningTime.Enabled = MaxWarningTimeColor.Enabled = false;
                MaxTime.Enabled = MaxTimeColor.Enabled = false;
                MaxTime.Required = false;
            }
            else if (type == TimerTypeEnum.Max)
            {
                MinWarningTime.ClearData();
                MinWarningTimeColor.ClearData();
                MinTime.ClearData();
                MinTimeColor.ClearData();
                MinWarningTime.Enabled = MinWarningTimeColor.Enabled = false;
                MinTime.Enabled = MinTimeColor.Enabled = false;
                MinTime.Required = false;
            }
        }

        protected virtual void ProcessTimerMinTimeDtl_TimerAction_SelectedIndexChanged(object sender, EventArgs e)
        {
            var type = (TimerActionEnum)Enum.Parse(typeof(TimerActionEnum), ProcessTimerMinTimeDtl_TimerAction.Data as string);
            if(type == TimerActionEnum.EndTxnNotExecute)
            {
                IsProductionEventMin.Enabled = false;
                IsProductionEventMin.ClearData();

                IsReworkMin.Enabled = false;
                IsReworkMin.ClearData();

                IsMoveNonStdMin.Enabled = false;
                IsMoveNonStdMin.ClearData();

                ResourceGroupMin.Enabled = false;
                ResourceGroupMin.Data = null;

                ResourceMin.Enabled = false;
                ResourceMin.Data = null;

                ToWorkflowMin.Enabled = false;
                ToWorkflowMin.ClearData();

                IsHoldMin.Enabled = false;
                IsHoldMin.ClearData();

                IsBusinessRuleMin.Enabled = false;
                IsBusinessRuleMin.ClearData();
                
                DefaultFailureModeMin.Required = false;
                DefaultFailureModeMin.Data = null;

                ClassificationMin.Required = false;
                ClassificationMin.Data = null;

                SubClassificationMin.Required = false;
                SubClassificationMin.Data = null;

                DefaultPEDescriptionMin.Required = false;
                DefaultPEDescriptionMin.Data = null;

                ReworkReasonMin.Required = false;
                ReworkReasonMin.Data = null;

                HoldReasonMin.Required = false;
                HoldReasonMin.Data = null;

                BusinessRuleMin.Required = false;
                BusinessRuleMin.Data = null;

                MinTimeEsig.Enabled = false;
                MinTimeEsig.Data = null;
                MinTimeEsig.Required = false;

                IsConfirmationMin.Enabled = false;
                IsConfirmationMin.CheckControl.Checked = false;
            }
            else if (type == TimerActionEnum.EndTxnExecute)
            {
                IsProductionEventMin.Enabled = true;
                IsReworkMin.Enabled = true;
                IsMoveNonStdMin.Enabled = true;
                IsHoldMin.Enabled = true;
                IsBusinessRuleMin.Enabled = true;
           
                DefaultFailureModeMin.Required = !DefaultFailureModeMin.IsEmpty;
                ClassificationMin.Required = !ClassificationMin.IsEmpty;                
                SubClassificationMin.Required = !SubClassificationMin.IsEmpty;                
                DefaultPEDescriptionMin.Required = !DefaultPEDescriptionMin.IsEmpty;                
                ReworkReasonMin.Required = !ReworkReasonMin.IsEmpty;                
                HoldReasonMin.Required = !HoldReasonMin.IsEmpty;
                BusinessRuleMin.Required = !BusinessRuleMin.IsEmpty;
                MinTimeEsig.Enabled = false;
                MinTimeEsig.Data = null;
                MinTimeEsig.Required = false;
                IsConfirmationMin.Enabled = true;
            }
            else if (type == TimerActionEnum.EndTxnExecuteWithESig)
            { 
                MinTimeEsig.Enabled = true;
                MinTimeEsig.Required = true;

                IsProductionEventMin.Enabled = false;
                IsProductionEventMin.ClearData();

                IsReworkMin.Enabled = false;
                IsReworkMin.ClearData();

                IsMoveNonStdMin.Enabled = false;
                IsMoveNonStdMin.ClearData();

                ResourceGroupMin.Enabled = false;
                ResourceGroupMin.Data = null;

                ResourceMin.Enabled = false;
                ResourceMin.Data = null;

                ToWorkflowMin.Enabled = false;
                ToWorkflowMin.ClearData();

                IsHoldMin.Enabled = false;
                IsHoldMin.ClearData();

                IsBusinessRuleMin.Enabled = false;
                IsBusinessRuleMin.ClearData();

                DefaultFailureModeMin.Required = false;
                DefaultFailureModeMin.Data = null;

                ClassificationMin.Required = false;
                ClassificationMin.Data = null;

                SubClassificationMin.Required = false;
                SubClassificationMin.Data = null;

                DefaultPEDescriptionMin.Required = false;
                DefaultPEDescriptionMin.Data = null;

                ReworkReasonMin.Required = false;
                ReworkReasonMin.Data = null;

                HoldReasonMin.Required = false;
                HoldReasonMin.Data = null;

                BusinessRuleMin.Required = false;
                BusinessRuleMin.Data = null;

                IsConfirmationMin.Enabled = false;
                IsConfirmationMin.CheckControl.Checked = false;
            }
        }

        protected virtual void ProcessTimerMaxTimeDtl_TimerAction_SelectedIndexChanged(object sender, EventArgs e)
        {
            var type = (TimerActionEnum)Enum.Parse(typeof(TimerActionEnum), ProcessTimerMaxTimeDtl_TimerAction.Data as string);
            if (type == TimerActionEnum.EndTxnNotExecute)
            {
                IsProductionEventMax.Enabled = false;
                IsProductionEventMax.ClearData();

                IsReworkMax.Enabled = false;
                IsReworkMax.ClearData();

                IsMoveNonStdMax.Enabled = false;
                IsMoveNonStdMax.ClearData();

                ResourceGroupMax.Enabled = false;
                ResourceGroupMax.Data = null;

                ResourceMax.Enabled = false;
                ResourceMax.Data = null;

                ToWorkflowMax.Enabled = false;
                ToWorkflowMax.ClearData();

                IsHoldMax.Enabled = false;
                IsHoldMax.ClearData();

                IsBusinessRuleMax.Enabled = false;
                IsBusinessRuleMax.ClearData();
                
                DefaultFailureModeMax.Required = false;
                DefaultFailureModeMax.Data = null;

                ClassificationMax.Required = false;
                ClassificationMax.Data = null;

                SubClassificationMax.Required = false;
                SubClassificationMax.Data = null;

                DefaultPEDescriptionMax.Required = false;
                DefaultPEDescriptionMax.Data = null;

                ReworkReasonMax.Required = false;
                ReworkReasonMax.Data = null;

                HoldReasonMax.Required = false;
                HoldReasonMax.Data = null;

                BusinessRuleMax.Required = false;
                BusinessRuleMax.Data = null;

                MaxTimeEsig.Enabled = false;
                MaxTimeEsig.Data = null;
                MaxTimeEsig.Required = false;

                IsConfirmationMax.Enabled = false;
                IsConfirmationMax.CheckControl.Checked = false;

            }
            else if (type == TimerActionEnum.EndTxnExecute)
            {
                IsProductionEventMax.Enabled = true;
                IsReworkMax.Enabled = true;
                IsMoveNonStdMax.Enabled = true;
                IsHoldMax.Enabled = true;
                IsBusinessRuleMax.Enabled = true;

                DefaultFailureModeMax.Required = !DefaultFailureModeMax.IsEmpty;
                ClassificationMax.Required = !ClassificationMax.IsEmpty;
                SubClassificationMax.Required = !SubClassificationMax.IsEmpty;
                DefaultPEDescriptionMax.Required = !DefaultPEDescriptionMax.IsEmpty;
                ReworkReasonMax.Required = !ReworkReasonMax.IsEmpty;
                HoldReasonMax.Required = !HoldReasonMax.IsEmpty;
                BusinessRuleMax.Required = !BusinessRuleMax.IsEmpty;
                MaxTimeEsig.Enabled = false;
                MaxTimeEsig.Data = null;
                MaxTimeEsig.Required = false;
                IsConfirmationMax.Enabled = true;
            }
            else if (type == TimerActionEnum.EndTxnExecuteWithESig)
            { 
                MaxTimeEsig.Enabled = true;                
                MaxTimeEsig.Required = true;

                IsProductionEventMax.Enabled = false;
                IsProductionEventMax.ClearData();

                IsReworkMax.Enabled = false;
                IsReworkMax.ClearData();

                IsMoveNonStdMax.Enabled = false;
                IsMoveNonStdMax.ClearData();

                ResourceGroupMax.Enabled = false;
                ResourceGroupMax.Data = null;

                ResourceMax.Enabled = false;
                ResourceMax.Data = null;

                ToWorkflowMax.Enabled = false;
                ToWorkflowMax.ClearData();

                IsHoldMax.Enabled = false;
                IsHoldMax.ClearData();

                IsBusinessRuleMax.Enabled = false;
                IsBusinessRuleMax.ClearData();

                DefaultFailureModeMax.Required = false;
                DefaultFailureModeMax.Data = null;

                ClassificationMax.Required = false;
                ClassificationMax.Data = null;

                SubClassificationMax.Required = false;
                SubClassificationMax.Data = null;

                DefaultPEDescriptionMax.Required = false;
                DefaultPEDescriptionMax.Data = null;

                ReworkReasonMax.Required = false;
                ReworkReasonMax.Data = null;

                HoldReasonMax.Required = false;
                HoldReasonMax.Data = null;

                BusinessRuleMax.Required = false;
                BusinessRuleMax.Data = null;

                IsConfirmationMax.Enabled = false;
                IsConfirmationMax.CheckControl.Checked = false;
            }
        }

        protected virtual void IsProductionEventMin_DataChanged(object sender, EventArgs e)
        {
            if (IsProductionEventMin.IsChecked)
            {
                DefaultFailureModeMin.Required = true;
                DefaultFailureModeMin.Enabled = true;

                ClassificationMin.Required = true;
                ClassificationMin.Enabled = true;

                SubClassificationMin.Required = true;
                SubClassificationMin.Enabled = true;

                DefaultPEDescriptionMin.Required = true;
                DefaultPEDescriptionMin.Enabled = true;
            }
            else if (!IsProductionEventMin.IsChecked)
            {
                DefaultFailureModeMin.Required = false;
                DefaultFailureModeMin.Enabled = false;
                DefaultFailureModeMin.Data = null;

                ClassificationMin.Required = false;
                ClassificationMin.Enabled = false;
                ClassificationMin.Data = null;

                SubClassificationMin.Required = false;
                SubClassificationMin.Enabled = false;
                SubClassificationMin.Data = null;

                DefaultPEDescriptionMin.Required = false;
                DefaultPEDescriptionMin.Enabled = false;
                DefaultPEDescriptionMin.Data = null;
            }          
        }

        protected virtual void IsProductionEventMax_DataChanged(object sender, EventArgs e)
        {
            if (IsProductionEventMax.IsChecked)
            {
                DefaultFailureModeMax.Required = true;
                DefaultFailureModeMax.Enabled = true;

                ClassificationMax.Required = true;
                ClassificationMax.Enabled = true;

                SubClassificationMax.Required = true;
                SubClassificationMax.Enabled = true;

                DefaultPEDescriptionMax.Required = true;
                DefaultPEDescriptionMax.Enabled = true;                

            }
            else if (!IsProductionEventMax.IsChecked)
            {
                DefaultFailureModeMax.Required = false;
                DefaultFailureModeMax.Enabled = false;
                DefaultFailureModeMax.Data = null;

                ClassificationMax.Required = false;
                ClassificationMax.Enabled = false;
                ClassificationMax.Data = null;

                SubClassificationMax.Required = false;
                SubClassificationMax.Enabled = false;
                SubClassificationMax.Data = null;

                DefaultPEDescriptionMax.Required = false;
                DefaultPEDescriptionMax.Enabled = false;
                DefaultPEDescriptionMax.Data = null;               
            }
        }

        protected virtual void IsReworkMin_DataChanged(object sender, EventArgs e)
        {
            if (IsReworkMin.IsChecked)
            {
                ReworkReasonMin.Required = true;
                ReworkReasonMin.Enabled = true;
            }
            else if (!IsReworkMin.IsChecked)
            {
                ReworkReasonMin.Required = false;
                ReworkReasonMin.Enabled = false;
                ReworkReasonMin.Data = null;
            }
        }

        protected virtual void IsReworkMax_DataChanged(object sender, EventArgs e)
        {
            if (IsReworkMax.IsChecked)
            {
                ReworkReasonMax.Required = true;
                ReworkReasonMax.Enabled = true;
            }
            else if (!IsReworkMax.IsChecked)
            {
                ReworkReasonMax.Required = false;
                ReworkReasonMax.Enabled = false;
                ReworkReasonMax.Data = null;
            }
                
        }

        protected virtual void IsMoveNonStdMin_DataChanged(object sender, EventArgs e)
        {
            if (IsMoveNonStdMin.IsChecked)
            {
                ResourceGroupMin.Enabled = true;
                ResourceMin.Enabled = true;
                ToWorkflowMin.Enabled = true;
            }
            else if (!IsMoveNonStdMin.IsChecked)
            {
                ResourceGroupMin.Enabled = false;
                ResourceGroupMin.Data = null;

                ResourceMin.Enabled = false;
                ResourceMin.Data = null;

                ToWorkflowMin.Enabled = false;
                ToWorkflowMin.ClearData();
            }
        }

        protected virtual void IsMoveNonStdMax_DataChanged(object sender, EventArgs e)
        {
            if(IsMoveNonStdMax.IsChecked)
            {
                ResourceGroupMax.Enabled = true;
                ResourceMax.Enabled = true;
                ToWorkflowMax.Enabled = true;                
            }
            else if(!IsMoveNonStdMax.IsChecked)
            {
                ResourceGroupMax.Enabled = false;
                ResourceGroupMax.Data = null;

                ResourceMax.Enabled = false;
                ResourceMax.Data = null;

                ToWorkflowMax.Enabled = false;
                ToWorkflowMax.ClearData();
            }
        }

        protected virtual void IsHoldMin_DataChanged(object sender, EventArgs e)
        {
            if (IsHoldMin.IsChecked)
            {
                HoldReasonMin.Required = true;
                HoldReasonMin.Enabled = true;
            }                
            else if (!IsHoldMin.IsChecked)
            {
                HoldReasonMin.Required = false;
                HoldReasonMin.Enabled = false;
                HoldReasonMin.Data = null;
            }
                
        }

        protected virtual void IsHoldMax_DataChanged(object sender, EventArgs e)
        {
            if (IsHoldMax.IsChecked)
            {
                HoldReasonMax.Required = true;
                HoldReasonMax.Enabled = true;
            }                
            else if (!IsHoldMax.IsChecked)
            {
                HoldReasonMax.Required = false;
                HoldReasonMax.Enabled = false;
                HoldReasonMax.Data = null;
            }                
        }

        protected virtual void IsBusinessRuleMin_DataChanged(object sender, EventArgs e)
        {
            if (IsBusinessRuleMin.IsChecked)
            {
                BusinessRuleMin.Required = true;
                BusinessRuleMin.Enabled = true;
            }
            else if (!IsBusinessRuleMin.IsChecked)
            {
                BusinessRuleMin.Required = false;
                BusinessRuleMin.Enabled = false;
                BusinessRuleMin.Data = null;
            }
        }

        protected virtual void IsBusinessRuleMax_DataChanged(object sender, EventArgs e)
        {
            if (IsBusinessRuleMax.IsChecked)
            {
                BusinessRuleMax.Required = true;
                BusinessRuleMax.Enabled = true;
            }
            else if (!IsBusinessRuleMax.IsChecked)
            {
                BusinessRuleMax.Required = false;
                BusinessRuleMax.Enabled = false;
                BusinessRuleMax.Data = null;
            }
        }

        #endregion

        #region Private Functions

        #endregion

        #region Constants

        #endregion

        #region Private Member Variables

        #endregion

    }
}

