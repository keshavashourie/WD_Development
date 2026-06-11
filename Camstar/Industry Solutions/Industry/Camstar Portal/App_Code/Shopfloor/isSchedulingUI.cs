/* © 2018 Siemens Product Lifecycle Management Software Inc. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
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

using SWC = System.Web.UI.WebControls;
using System.Collections;

/// <summary>
/// Summary description for SchedulingUI
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class isSchedulingUI : MatrixWebPart
    {
        protected virtual JQDataGrid ResultsGrid { get { return Page.FindCamstarControl("SearchResultsGrid") as JQDataGrid; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            (ResultsGrid.GridContext as SelValGridContext).SnapCompleted += IsSchedulingUI_SnapCompleted;
        }


        /// <summary>
        /// Massage the data before sending to client
        /// </summary>
        /// <param name="dataWindowTable"></param>
        private void IsSchedulingUI_SnapCompleted(DataTable dataWindowTable)
        {
            // javascript cannot reliably parse date strings.  Convert to ticks and set values to hidden columns
            foreach(DataRow mfgOrderRow in dataWindowTable.Rows)
            {
                string plannedStartDateTicks = "-1";
                if (!mfgOrderRow.IsNull("PlannedStartDate"))
                {
                    DateTime plannedStartDate = mfgOrderRow.Field<DateTime>("PlannedStartDate");
                    plannedStartDateTicks = new DateTimeOffset(plannedStartDate).ToUnixTimeMilliseconds().ToString();
                }
                mfgOrderRow.SetField<string>("PlannedStartDateTicks", plannedStartDateTicks);

                string actualStartDateTicks = "-1";
                if (!mfgOrderRow.IsNull("ActualStartDate"))
                {
                    DateTime actualStartDate = mfgOrderRow.Field<DateTime>("ActualStartDate");
                    actualStartDateTicks = new DateTimeOffset(actualStartDate).ToUnixTimeMilliseconds().ToString();
                }
                mfgOrderRow.SetField<string>("ActualStartDateTicks", actualStartDateTicks);

                string plannedCompletionDateTicks = "-1";
                if (!mfgOrderRow.IsNull("PlannedCompletionDate"))
                {
                    DateTime plannedCompletionDate = mfgOrderRow.Field<DateTime>("PlannedCompletionDate");
                    plannedCompletionDateTicks = new DateTimeOffset(plannedCompletionDate).ToUnixTimeMilliseconds().ToString();
                }
                mfgOrderRow.SetField<string>("PlannedCompletionDateTicks", plannedCompletionDateTicks);
            }
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/User/Industry Solutions/Scheduling.js");
        }


        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (IsResponsive)
            {
                // Not needed if implementing the 'Classic' page
                ScriptManager.RegisterStartupScript(Page.Form, Page.Form.GetType(), "SearchLayoutFunctions", string.Format("isScheduling.initialize('{0}', {1});", ResultsGrid.ClientID, Page.IsPostBack ? "false" : "true"), true);
            }
        }
    }
}