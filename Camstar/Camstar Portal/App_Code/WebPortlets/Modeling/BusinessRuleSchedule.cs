// Copyright Siemens 2023  
using System;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.WCFUtilities;
using System.Data;
using System.Data.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;
using Camstar.Util;
using Camstar.WebPortal.PortalConfiguration;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class BusinessRuleSchedule : MatrixWebPart
    {
        protected virtual DropDownList DayOfWeekField
        {
            get { return Page.FindCamstarControl("ObjectChanges_DayOfWeek") as DropDownList; }
        }

        protected virtual TextBox DayOfMonthField
        {
            get { return Page.FindCamstarControl("ObjectChanges_DayOfMonth") as TextBox; }
        }

        protected virtual DropDownList RecurrencePatternField
        {
            get { return Page.FindCamstarControl("ObjectChanges_RecurrencePattern") as DropDownList; }
        }

        protected virtual CheckBox IsLastDayOfMonthField
        {
            get { return Page.FindCamstarControl("ObjectChanges_IsLastDayOfMonth") as CheckBox; }
        }

        protected virtual DropDownList MonthOfYearField
        {
            get { return Page.FindCamstarControl("ObjectChanges_MonthOfYear") as DropDownList; }
        }
        protected virtual DropDownList TimeZone
        {
            get { return Page.FindCamstarControl("TimeZone") as DropDownList; }            
        }
        protected virtual TextBox SelectedTimeZoneHidden
        {
            get { return Page.FindCamstarControl("ObjectChanges_TimeZone") as TextBox; }
        }
        protected virtual RadioButton SchedulingTypeBasic
        {
            get { return Page.FindCamstarControl("SchedulingType_Basic") as RadioButton; }
        }
        protected virtual RadioButton SchedulingTypeAdvanced
        {
            get { return Page.FindCamstarControl("SchedulingType_Advanced") as RadioButton; }
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
        protected virtual CheckBox IsViewField
        {
            get { return Page.FindCamstarControl("ObjectChanges_IsView") as CheckBox; }
        }
        protected virtual TextBox RecurrenceFrequency
        {
            get { return Page.FindCamstarControl("ObjectChanges_RecurrenceFrequency") as TextBox; }
        }

        public override void GetInputData(Service serviceData)
        {
            if ((bool)IsViewField.Data)
            {
                RecurrenceFrequency.Data = "";
                RecurrencePatternField.Data = "";
                RecurrenceFrequency.Required = false;
                RecurrencePatternField.Required = false;
                DayOfMonthField.Data = "";
                IsLastDayOfMonthField.Data = "";
                MonthOfYearField.Data = "";
                DayOfWeekField.Data = "";

                var data = (serviceData as ScheduledBusinessRuleMaint);
                if (data != null && data.ObjectChanges != null)
                {
                    data.ObjectChanges.RecurrencePattern = null;
                    data.ObjectChanges.RecurrenceFrequency = null;
                }
            }
            else
            {
              //clear all  Advanced Mode fields
                ScheduleMonthsField.Data = "";
                ScheduleHoursField.Data = "";
                ScheduleDaysOfWeekField.Data = "";
                ScheduleDaysOfMonthField.Data = "";

                var data = (serviceData as ScheduledBusinessRuleMaint);
                if (data != null && data.ObjectChanges != null)
                {
                    data.ObjectChanges.ScheduleDaysOfMonth = null;
                    data.ObjectChanges.ScheduleDaysOfWeek = null;
                    data.ObjectChanges.ScheduleHours = null;
                    data.ObjectChanges.ScheduleMonths = null;
                }

                if (RecurrencePatternField.Data == null || RecurrencePatternField.Data.GetType() != typeof(int))
                    return;
                var excludedPatterns = new[] { 0, 1, 2, 5, 6, 7 };
                if (excludedPatterns.Contains((int)RecurrencePatternField.Data))
                {
                    DayOfMonthField.Data = null;
                    IsLastDayOfMonthField.Data = false;
                    MonthOfYearField.Data = null;
                }
                if ((int)RecurrencePatternField.Data == 3)
                {
                    MonthOfYearField.Data = null;
                }
                base.GetInputData(serviceData);
                if ((serviceData as ScheduledBusinessRuleMaint) != null
                    && (serviceData as ScheduledBusinessRuleMaint).ObjectChanges != null
                    && (serviceData as ScheduledBusinessRuleMaint).ObjectChanges.RecurrencePattern != null)
                {
                    var changes = (serviceData as ScheduledBusinessRuleMaint).ObjectChanges;
                    var pattern = changes.RecurrencePattern.Value;

                    switch (pattern)
                    {

                        case 1: //daily                      
                        case 5: //hourly
                        case 6: //minutes
                        case 0: //once
                        case 7: //seconds
                            if (changes.DayOfMonth != null)
                                (changes.DayOfMonth as Primitive<int>).IsPrimitiveEmpty = true;
                            changes.IsLastDayOfMonth = false;

                            if (changes.MonthOfYear != null)
                                (changes.MonthOfYear as Primitive<int>).IsPrimitiveEmpty = true;

                            if (changes.DayOfWeek != null)
                                (changes.DayOfWeek as Primitive<int>).IsPrimitiveEmpty = true;
                            break;

                        case 2: //weekly
                            if (changes.DayOfMonth != null)
                                ((changes.DayOfMonth) as Primitive<int>).IsPrimitiveEmpty = true;
                            changes.IsLastDayOfMonth = false;

                            if (changes.MonthOfYear != null)
                                ((changes.MonthOfYear) as Primitive<int>).IsPrimitiveEmpty = true;
                            break;

                        case 3: //monthly
                            if (changes.MonthOfYear != null)
                                ((changes.MonthOfYear) as Primitive<int>).IsPrimitiveEmpty = true;
                            if (changes.DayOfWeek != null)
                                ((changes.DayOfWeek) as Primitive<int>).IsPrimitiveEmpty = true;
                            break;

                        case 4: //yearly
                            if (changes.DayOfWeek != null)
                                ((changes.DayOfWeek) as Primitive<int>).IsPrimitiveEmpty = true;
                            break;
                    }
                }
            }
        }

        public override FormsFramework.ValidationStatus ValidateInputData(Service serviceData)
        {
            ValidationStatus status = base.ValidateInputData(serviceData);
            //copy action
            if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument" && (bool)IsViewField.Data)
            {
                RecurrenceFrequency.Required = false;
                RecurrencePatternField.Required = false;
            }

            return status;
        }

        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            var listOfTimeZones = CamstarPortalSection.Settings.TimeZoneSettings.TimeZones
                     .Select(it =>
                     {
                         if (!string.IsNullOrEmpty(it.TimeZoneCode) && DateUtil.timeZoneMap.ContainsKey(it.TimeZoneCode))
                         {
                             return new CustomListValueMapItem()
                             {
                                 Value = it.TimeZoneCode,
                                 DisplayName = $"(UTC{DateUtil.ExtractTimeOffset(it.TimeZoneCode)}) {it.Name}"
                             };
                         }
                         else
                         {
                             return null; 
                         }
                     })
                     .Where(item => item != null) 
                     .ToArray();

            TimeZone.CustomListValues = listOfTimeZones;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if ((bool)IsViewField.Data)
            {
                RecurrenceFrequency.Required = false;
                RecurrencePatternField.Required = false;
            }
            else
            {
                RecurrenceFrequency.Required = true;
                RecurrencePatternField.Required = true;
            }

            if (SelectedTimeZoneHidden.Data == null)
            {
                var defaultTimeZone = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile.TimeZone;
                TimeZone.Data = defaultTimeZone;
                TimeZone.DataBind();
            }

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SchedulingTypeBasic.DataChanged += SchedulingTypeBasic_DataChanged;
            SchedulingTypeAdvanced.DataChanged += SchedulingTypeAdvanced_DataChanged;

            string startupScript = String.Format("_businessRuleSchedule = new Camstar.WebPortal.BusinessRuleSchedule(); _businessRuleSchedule.init('{0}','{1}','{2}','{3}', '{4}');",
                  DayOfWeekField.ClientID, DayOfMonthField.ClientID, IsLastDayOfMonthField.ClientID, MonthOfYearField.ClientID, RecurrencePatternField.ClientID);
            if (!Page.ClientScript.IsStartupScriptRegistered("BusinessRuleScheduleStartup"))
                ScriptManager.RegisterStartupScript(this, this.GetType(), "BusinessRuleScheduleStartup", startupScript, true);

            ScriptManager.RegisterClientScriptInclude(this, typeof(string), "BusinessRuleSchedule",
                                                      ResolveClientUrl("~/Scripts/BusinessRuleSchedule.js"));
            var startupScript1 =
                String.Format("getTypeSelectionElementsId('{0}','{1}');",
                              SchedulingTypeBasic.RadioControl.ClientID,
                              SchedulingTypeAdvanced.RadioControl.ClientID) +
                String.Format("getTableElementsId('{0}');",
                              IsViewField.CheckControl.ClientID) +
                String.Format("getScheduleElementsId('{0}','{1}','{2}','{3}','{4}','{5}', '{6}');",
                              StartingAtHourField.TextControl.ClientID,
                              HoursField.TextControl.ClientID,
                              SpecificDaysOfWeekField.CheckControl.ClientID,
                              SpecificDaysOfMonthField.CheckControl.ClientID,
                              SpecificMonthsField.CheckControl.ClientID,
                              StartingAtDayControl.TextControl.ClientID,
                              EveryDayControl.TextControl.ClientID) +
                String.Format("getHiddenScheduleElementsId('{0}','{1}','{2}','{3}');",
                              ScheduleHoursField.TextControl.ClientID,
                              ScheduleDaysOfWeekField.TextControl.ClientID,
                              ScheduleDaysOfMonthField.TextControl.ClientID,
                              ScheduleMonthsField.TextControl.ClientID) +
                String.Format("loadInitialData();");
            if (!Page.ClientScript.IsStartupScriptRegistered("Startup"))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "Startup", startupScript1, true);
            }

            SchedulingTypeBasic.RadioControl.Attributes["onclick"] = "setIsView(true);";
            SchedulingTypeAdvanced.RadioControl.Attributes["onclick"] = "setIsView(false);";
            HoursField.TextControl.Attributes["onchange"] = "hours_Changed();";
            StartingAtHourField.TextControl.Attributes["onchange"] = "hours_Changed();";
            SpecificDaysOfWeekField.CheckControl.Attributes["onclick"] = String.Format("setDisplayForDaysOfWeek({0},{1});", "this.checked", SpecificDaysOfWeekField.CheckControl.ClientID);
            SpecificDaysOfMonthField.CheckControl.Attributes["onclick"] = String.Format("setDisplayForDaysOfMonth({0},{1});", "this.checked", SpecificDaysOfMonthField.CheckControl.ClientID);
            SpecificMonthsField.CheckControl.Attributes["onclick"] = String.Format("setDisplayForMonths({0},{1});", "this.checked", SpecificMonthsField.CheckControl.ClientID);
            RenderToClient = true;
        }

        protected virtual void SchedulingTypeAdvanced_DataChanged(object sender, EventArgs e)
        {
            RecurrenceFrequency.Required = true;
            RecurrencePatternField.Required = true;
        }

        protected virtual void SchedulingTypeBasic_DataChanged(object sender, EventArgs e)
        {
            RecurrenceFrequency.Required = true;
            RecurrencePatternField.Required = true;
        }
    }
}
