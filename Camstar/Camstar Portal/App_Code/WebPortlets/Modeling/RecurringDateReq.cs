// Copyright Siemens 2023  
using System;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebControls;
using System.Web.UI;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.Util;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.Personalization;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class RecurringDateReq : MatrixWebPart
    {

        protected virtual DropDownList DayOfWeekField
        {
            get { return Page.FindCamstarControl("ObjectChanges_DayOfWeek") as DropDownList; }
        }

        protected virtual TextBox DayOfMonthField
        {
            get { return Page.FindCamstarControl("ObjectChanges_DayOfMonth") as TextBox; }
        }

        protected virtual DropDownList MonthOfYearField
        {
            get { return Page.FindCamstarControl("ObjectChanges_MonthOfYear") as DropDownList; }
        }

        protected virtual DropDownList RecurringDatePatternField
        {
            get { return Page.FindCamstarControl("RecurringDatePatternField") as DropDownList; }
        }

        protected virtual DropDownList TimeZone
        {
            get { return Page.FindCamstarControl("TimeZone") as DropDownList; }
        }
        protected virtual TextBox SelectedTimeZoneHidden
        {
            get { return Page.FindCamstarControl("ObjectChanges_TimeZone") as TextBox; }
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

            if (SelectedTimeZoneHidden.Data == null)
            {
                var defaultTimeZone = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile.TimeZone;
                TimeZone.Data = defaultTimeZone;
                TimeZone.DataBind();
            }
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            // Clears data of invisible controls.
            string selectedPattern = RecurringDatePatternField.Text;
            if ((serviceData as RecurringDateReqMaint) != null && (serviceData as RecurringDateReqMaint).ObjectChanges != null)
            {
                var changes = (serviceData as RecurringDateReqMaint).ObjectChanges as RecurringDateReqChanges;
                if (string.IsNullOrEmpty(selectedPattern) || string.Equals(selectedPattern, "Daily"))
                {
                    changes.DayOfWeek = new Enumeration<DayOfWeekEnum, int>();
                    changes.DayOfMonth = new Primitive<int>();
                    changes.MonthOfYear = new Enumeration<MonthEnum, int>();
                }
                else if (string.Equals(selectedPattern, "Weekly"))
                {
                    changes.DayOfMonth = new Primitive<int>();
                    changes.MonthOfYear = new Enumeration<MonthEnum, int>();
                }
                else if (string.Equals(selectedPattern, "Monthly"))
                {
                    changes.DayOfWeek = new Enumeration<DayOfWeekEnum, int>();
                    changes.MonthOfYear = new Enumeration<MonthEnum, int>();
                }
                else if (string.Equals(selectedPattern, "Yearly"))
                {
                    changes.DayOfWeek = new Enumeration<DayOfWeekEnum, int>();
                }
                else if (string.Equals(selectedPattern, "Hourly"))
                {
                    changes.DayOfWeek = new Enumeration<DayOfWeekEnum, int>();
                }
            }
        }
    }
}
