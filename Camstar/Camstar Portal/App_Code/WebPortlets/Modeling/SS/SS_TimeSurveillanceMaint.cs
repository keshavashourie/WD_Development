// Copyright 2019 Siemens
using System;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebControls;
using System.Web.UI;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class SS_TimeSurveillanceMaint : MatrixWebPart
    {

        protected virtual DropDownList DayOfWeekField
        {
            get { return Page.FindCamstarControl("ObjectChanges_ss_DayOfWeek") as DropDownList; }
        }

        protected virtual TextBox DayOfMonthField
        {
            get { return Page.FindCamstarControl("ObjectChanges_ss_DayOfMonth") as TextBox; }
        }

        protected virtual DropDownList MonthOfYearField
        {
            get { return Page.FindCamstarControl("ObjectChanges_ss_MonthOfYear") as DropDownList; }
        }

        protected virtual DropDownList RecurringDatePatternField
        {
            get { return Page.FindCamstarControl("ObjectChanges_ss_RecurringDatePattern") as DropDownList; }
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
            }
        }
    }
}


