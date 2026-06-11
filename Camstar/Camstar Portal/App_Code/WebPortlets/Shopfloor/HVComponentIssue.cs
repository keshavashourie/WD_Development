using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WebPortlets;
using Camstar.WCF.Services;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;


namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class HVComponentIssue : MatrixWebPart
    {
        protected CWC.NamedObject SelectedResource { get { return Page.FindCamstarControl("HVComponentIssue_Resource") as CWC.NamedObject; } }
        protected JQDataGrid SetupDetailsGrid { get { return Page.FindCamstarControl("HVComponentIssue_CurrentSetupDetail") as JQDataGrid; } }
        protected ContainerListGrid HiddenSelectedContainer { get { return Page.FindCamstarControl("HiddenSelectedContainer") as ContainerListGrid; } }

        protected JQDataGrid HVIssuedComponentsGrid { get { return Page.FindCamstarControl("HVIssuedComponentsGrid") as JQDataGrid; } }

        // For each toggle section, keep track of whether it has been expanded and data loaded (load data on demand and don't re-load)
        protected CWC.TextBox IssueToggleExpanded { get { return Page.FindCamstarControl("IssueToggleExpanded") as CWC.TextBox; } }
        protected CWC.TextBox IssueDataLoaded { get { return Page.FindCamstarControl("IssueDataLoaded") as CWC.TextBox; } }
        protected CWC.Button GetIssuedComponents { get { return Page.FindCamstarControl("GetIssuedComponents") as CWC.Button; } }

        public HVComponentIssue()
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                IssueToggleExpanded.Data = "no";
                IssueDataLoaded.Data = "no";
            }

            SelectedResource.DataChanged += HiddenSelectedResource_DataChanged;
            GetIssuedComponents.Click += GetIssuedComponents_Click;
            (HVIssuedComponentsGrid.GridContext as QueryContext).BeforeQueryExecution += HVIssuedComponentsGrid_BeforeQueryExecution;

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.RegisterStartupScript(this, GetType().GetType(), "initToggleContainers", "csiHVComponentIssue.initToggleContainers();", true);
        }

        public override void ClearValues(OM.Service serviceData)
        {
            base.ClearValues(serviceData);
            IssueToggleExpanded.Data = "no";
            IssueDataLoaded.Data = "no";
        }

        protected virtual void HiddenSelectedResource_DataChanged(object sender, EventArgs e)
        {
            LoadSetupDetailsGrid();
        }

        protected virtual void LoadSetupDetailsGrid()
        {
            try
            {
                if (SelectedResource.Data != null)
                {
                    var currentUserProfile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                    QueryService service = new QueryService(currentUserProfile);
                    OM.QueryOptions options = new OM.QueryOptions();
                    OM.RecordSet data = new OM.RecordSet();

                    OM.QueryParameters queryParamsObj = new OM.QueryParameters();
                    OM.QueryParameter[] queryParams = new OM.QueryParameter[1]
                    {
                    new OM.QueryParameter("ResourceName", SelectedResource.Data.ToString())
                    };
                    queryParamsObj.Parameters = queryParams;

                    string query = "GetCurrentHVResourceSetupDetails";
                    OM.ResultStatus resStatus = service.Execute(query, queryParamsObj, options, out data);
                    if (resStatus.IsSuccess)
                    {
                        DataTable dt = data.GetAsDataTable();
                        List<OM.HVResourceSetupDetail> details = new List<OM.HVResourceSetupDetail>();
                        for (int row = 0; row < dt.Rows.Count; row++)
                        {
                            var detail = new OM.HVResourceSetupDetail();
                            detail.CompId = dt.Rows[row].Field<string>("CompId");
                            detail.CompName = dt.Rows[row].Field<string>("CompName");
                            detail.Slot = Convert.ToInt32(dt.Rows[row].Field<string>("Slot"));
                            detail.SubSlot = Convert.ToInt32(dt.Rows[row].Field<string>("SubSlot"));
                            details.Add(detail);
                        }

                        SetupDetailsGrid.ClearData();
                        SetupDetailsGrid.Data = details.ToArray();
                    }

                }
            }
            catch (Exception ex)
            {
                Page.DisplayMessage(ex.InnerException != null ? ex.InnerException.Message : ex.Message, false);
            }
        }

        private void GetIssuedComponents_Click(object sender, EventArgs e)
        {
            HVIssuedComponentsGrid.ClearData();
            HVIssuedComponentsGrid.BoundContext.Reload(new ClientGridState());
        }

        private bool HVIssuedComponentsGrid_BeforeQueryExecution(QueryState queryState)
        {
            bool containerSet = HiddenSelectedContainer.Data != null;
            bool toggleContainerExpanded = IssueToggleExpanded.Data != null && IssueToggleExpanded.Data.ToString() == "yes";

            if (containerSet && toggleContainerExpanded)
            {
                IssueDataLoaded.Data = "yes";
                return true;
            }
            return false;
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/HVComponentIssue.js");
        }



    }
}