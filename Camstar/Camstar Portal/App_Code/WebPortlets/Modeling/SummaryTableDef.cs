// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.WCFUtilities;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class SummaryTableDef : MatrixWebPart
    {
        protected override void OnLoadPersonalization()
        {
            base.OnLoadPersonalization();

            var copyAction = Page.ActionDispatcher.GetActionByName("CopyBtn");
            if (copyAction != null)
                copyAction.ConditionHandler = "WebPartConditionActionHandler"; // need to hide Copy action.
        }

        public override void WebPartConditionActionHandler(object sender, ConditionActionEventArgs e)
        {
            base.WebPartConditionActionHandler(sender, e);
            e.IsHidden = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ScriptManager.RegisterClientScriptInclude(this, typeof(string), "SummaryTableDef",
                                                      ResolveClientUrl("~/Scripts/SummaryTableDef_VP.js"));

            var startupScript =
                String.Format("getTypeSelectionElementsId('{0}','{1}','{2}','{3}');",
                              ViewField.RadioControl.ClientID,
                              TableField.RadioControl.ClientID, ViewField.ClientID, TableField.ClientID) +
                String.Format("getTableElementsId('{0}','{1}','{2}', '{3}');",
                              IsViewField.CheckControl.ClientID,
                              AppendToTableField.ClientID,
                              IsManuallyExecutedField.ClientID,
                              TableNameTypeLabel.ClientID) +
                String.Format("getScheduleElementsId('{0}','{1}','{2}','{3}','{4}','{5}', '{6}', '{7}');",
                              IsEnabledField.ClientID,
                              HoursField.TextControl.ClientID,
                              StartingAtHourField.TextControl.ClientID,
                              SpecificDaysOfWeekField.CheckControl.ClientID,
                              SpecificDaysOfMonthField.CheckControl.ClientID,
                              SpecificMonthsField.CheckControl.ClientID,
                              StartingAtDayControl.TextControl.ClientID,
                              EveryDayControl.TextControl.ClientID) +
                String.Format("getDateElementsId('{0}','{1}');",
                              StartDateField.ClientID,
                              EndDateField.ClientID) +
                String.Format("getHiddenScheduleElementsId('{0}','{1}','{2}','{3}');",
                              ScheduleHoursField.TextControl.ClientID,
                              ScheduleDaysOfWeekField.TextControl.ClientID,
                              ScheduleDaysOfMonthField.TextControl.ClientID,
                              ScheduleMonthsField.TextControl.ClientID) +
                String.Format("loadInitialData();");

            if (!Page.ClientScript.IsStartupScriptRegistered("Startup"))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "Startup", startupScript, true);
            }

            ViewField.RadioControl.Attributes["onclick"] = "setIsView(true);";
            TableField.RadioControl.Attributes["onclick"] = "setIsView(false);";
            IsManuallyExecutedField.CheckControl.Attributes["onclick"] = "setIsManual(this.checked);";

            HoursField.TextControl.Attributes["onchange"] = "hours_Changed();";
            StartingAtHourField.TextControl.Attributes["onchange"] = "hours_Changed();";

            SpecificDaysOfWeekField.CheckControl.Attributes["onclick"] = "setDisplayForDaysOfWeek(this.checked);";
            SpecificDaysOfMonthField.CheckControl.Attributes["onclick"] = "setDisplayForDaysOfMonth(this.checked);";
            SpecificMonthsField.CheckControl.Attributes["onclick"] = "setDisplayForMonths(this.checked);";

            SaveTestQueryButton.Click += SaveTestQueryPopupLinkButton_Click;
            SaveExecuteManuallyButton.Click += SaveExecuteManuallyButton_Click;
            RenderToClient = true;
        }

        protected virtual void SaveTestQueryPopupLinkButton_Click(object sender, EventArgs e)
        {
            ResultStatus res = AddOrUpdate();
            if (res.IsSuccess)
            {
                OpenTestQueryPopup();
            }
            DisplayMessage(res);
        }

        protected virtual void OpenTestQueryPopup()
        {
            FloatPageOpenAction floatAction = new FloatPageOpenAction();
            floatAction.FrameLocation = new UIFloatingPageLocation();
            floatAction.PageName = "SummaryTableDefTestPopup_VP";
            floatAction.FrameLocation.Width = 680;
            floatAction.FrameLocation.Height = 480;
            Page.ActionDispatcher.ExecuteAction(floatAction);
        }

        protected virtual void SaveExecuteManuallyButton_Click(object sender, EventArgs e)
        {
            ResultStatus res = AddOrUpdate();
            DisplayMessage(res);
        }

        protected virtual ResultStatus AddOrUpdate()
        {
            //Perform value validation
            SummaryTableDefMaint inputForExecute = new SummaryTableDefMaint();
            Page.GetInputData(inputForExecute);

            ResultStatus resultStatus = (this.Page as CamstarForm).ValidateInputData(inputForExecute);
            if (!resultStatus.IsSuccess)
                return resultStatus;

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(Page.Session);
            SummaryTableDefMaintService service = Page.Service.GetService<SummaryTableDefMaintService>();
            service.BeginTransaction();
            var pc = Page.PortalContext as MaintenanceBehaviorContext;
            if (pc.State != MaintenanceBehaviorContext.MaintenanceState.Edit && !(Page.DataContract.GetValueByName("IsAddOrUpdate") == null ? false : (bool)Page.DataContract.GetValueByName("IsAddOrUpdate")))
            {
                service.New(); //add new cdo
                Page.DataContract.SetValueByName("IsAddOrUpdate", true);
            }
            else
            {
                SummaryTableDefMaint input = new SummaryTableDefMaint() { ObjectToChange = new NamedObjectRef() { Name = (Page.FindCamstarControl("NameTxt") as TextBox).Data.ToString() } };
                service.Load(input);
            }

            service.ExecuteTransaction(inputForExecute);
            resultStatus = service.CommitTransaction();

            if (!resultStatus.IsSuccess)
            {
                Page.DataContract.SetValueByName("IsAddOrUpdate", false);
            }
            else
            {
                if (pc.Current == null)
                    pc.Current = new NamedObjectRef()
                    {
                        Name = (Page.FindCamstarControl("NameTxt") as TextBox).Data.ToString()
                    };
                Page.LoadModelingValues(true);
            }

            return resultStatus;
        }

        #region Controls

        protected virtual Button SaveTestQueryButton
        {
            get { return Page.FindCamstarControl("SaveTestQueryPopupLinkButton") as Button; }
        }

        protected virtual Button SaveExecuteManuallyButton
        {
            get { return Page.FindCamstarControl("SaveAndExecuteManuallyButton") as Button; }
        }

        protected virtual RadioButton ViewField
        {
            get { return Page.FindCamstarControl("SummaryType_ViewSelection") as RadioButton; }
        }

        protected virtual RadioButton TableField
        {
            get { return Page.FindCamstarControl("SummaryType_TableSelection") as RadioButton; }
        }

        protected virtual CheckBox IsViewField
        {
            get { return Page.FindCamstarControl("ObjectChanges_IsView") as CheckBox; }
        }

        protected virtual CheckBox AppendToTableField
        {
            get { return Page.FindCamstarControl("ObjectChanges_AppendToTable") as CheckBox; }
        }

        protected virtual CheckBox IsManuallyExecutedField
        {
            get { return Page.FindCamstarControl("ObjectChanges_IsManuallyExecuted") as CheckBox; }
        }

        protected virtual CheckBox IsEnabledField
        {
            get { return Page.FindCamstarControl("ObjectChanges_IsEnabled") as CheckBox; }
        }

        protected virtual TextBox HoursField
        {
            get { return Page.FindCamstarControl("EveryHourControl") as TextBox; }
        }

        protected virtual TextBox StartingAtHourField
        {
            get { return Page.FindCamstarControl("StartingAtHourControl") as TextBox; }
        }

        protected virtual CheckBox SpecificDaysOfWeekField
        {
            get { return Page.FindCamstarControl("SpecificDaysOfWeek") as CheckBox; }
        }

        protected virtual TextBox ScheduleHoursField
        {
            get { return Page.FindCamstarControl("ObjectChanges_ScheduleHours") as TextBox; }
        }

        protected virtual TextBox ScheduleDaysOfWeekField
        {
            get { return Page.FindCamstarControl("ObjectChanges_ScheduleDaysOfWeek") as TextBox; }
        }

        protected virtual TextBox ScheduleDaysOfMonthField
        {
            get { return Page.FindCamstarControl("ObjectChanges_ScheduleDaysOfMonth") as TextBox; }
        }

        protected virtual TextBox ScheduleMonthsField
        {
            get { return Page.FindCamstarControl("ObjectChanges_ScheduleMonths") as TextBox; }
        }

        protected virtual CheckBox SpecificDaysOfMonthField
        {
            get { return Page.FindCamstarControl("SpecificDaysOfMonth") as CheckBox; }
        }

        protected virtual CheckBox SpecificMonthsField
        {
            get { return Page.FindCamstarControl("SpecificMonths") as CheckBox; }
        }

        protected virtual TextBox StartingAtDayControl
        {
            get { return Page.FindCamstarControl("StartingAtDayControl") as TextBox; }
        }

        protected virtual TextBox EveryDayControl
        {
            get { return Page.FindCamstarControl("EveryDayControl") as TextBox; }
        }

        protected virtual DateChooser StartDateField
        {
            get { return Page.FindCamstarControl("ObjectChanges_StartDate") as DateChooser; }
        }

        protected virtual DateChooser EndDateField
        {
            get { return Page.FindCamstarControl("ObjectChanges_EndDate") as DateChooser; }
        }

        protected virtual FormsFramework.WebControls.Label TableNameTypeLabel
        {
            get { return Page.FindCamstarControl("lblTableNameType") as FormsFramework.WebControls.Label; }
        }

        #endregion
    }
}
