// Copyright Siemens 2023 
using System;
using System.Linq;
using System.Data;
using System.Web.UI;

using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

/// <summary>
/// Summary description for PartRequestMain
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Parts
{
    public class PartRequestMain : MatrixWebPart
    {
        private CWC.NamedObject resource { get { return Page.FindCamstarControl("PartTxn_Resource") as CWC.NamedObject; } }
        private CWC.NamedObject resourcefamily { get { return Page.FindCamstarControl("PartTxn_ResourceFamily") as CWC.NamedObject; } }
        private CWC.DropDownList requesttype { get { return Page.FindCamstarControl("PartTxn_RequestType") as CWC.DropDownList; } }
        private CWC.DropDownList requeststatus { get { return Page.FindCamstarControl("PartTxn_RequestStatus") as CWC.DropDownList; } }

        private CWC.TextBox selectedresource { get { return Page.FindCamstarControl("SelectedResource") as CWC.TextBox; } }
        private CWC.TextBox selectedresourcefamily { get { return Page.FindCamstarControl("SelectedResourceFamily") as CWC.TextBox; } }
        private CWC.TextBox selectedrequesttype { get { return Page.FindCamstarControl("SelectedRequestType") as CWC.TextBox; } }
        private CWC.TextBox selectedrequeststatus { get { return Page.FindCamstarControl("SelectedRequestStatus") as CWC.TextBox; } }
        private CWC.TextBox selectedrow { get { return Page.FindCamstarControl("SelectedRow") as CWC.TextBox; } }
        private CWC.TextBox selectedrowaftersubmit { get { return Page.FindCamstarControl("SelectedRowAfterSubmit") as CWC.TextBox; } }

        private JQDataGrid results { get { return Page.FindCamstarControl("ResultsPanel") as JQDataGrid; } }

        private CWC.Button HorizonSearchButton {get { return Page.FindCamstarControl("SearchButton") as CWC.Button; }}
        private CWC.Button HorizonClearAllButton { get { return Page.FindCamstarControl("resetButton") as CWC.Button; } }

        private JQTabContainer SearchTabContainer { get { return Page.FindCamstarControl("Search_TabContainer") as JQTabContainer; } }

        public PartRequestMain()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public void UpdateParams()
        {
            if (resource.Data == null)
                selectedresource.Data = "%";
            else
                selectedresource.Data = resource.Data;

            if (resourcefamily.Data == null)
                selectedresourcefamily.Data = "%";
            else
                selectedresourcefamily.Data = resourcefamily.Data;

            if (requesttype.Text == "")
                selectedrequesttype.Data = "%";
            else
                selectedrequesttype.Data = requesttype.Text;

            if (requeststatus.Text == "")
                selectedrequeststatus.Data = "%";
            else
                selectedrequeststatus.Data = requeststatus.Text;

            results.ClearData();
            ManagePanelButton();
        }

        public void ClearFieldsPartRequestMain()
        {
            if (resource.Data != null)
                resource.Data = null;

            if (resourcefamily.Data != null)
                resourcefamily.Data = null;

            if (requesttype.Data != null)
                requesttype.ClearData();

            if (requeststatus.Data != null)
                requeststatus.ClearData();

            selectedresource.Data = "%";
            selectedresourcefamily.Data = "%";
            selectedrequesttype.Data = "%";
            selectedrequeststatus.Data = "%";


            results.ClearData();
        }

        public void ManagePanelButton()
        {
            string varPartRequest = "PartRequestButton";
            string varPartRequestUpdate = "PartRequestUpdateButton";
            string varPartRequestIssue = "PartRequestIssueButton";
            string varPartRequestCancel = "PartRequestCancelButton";
            string varPartRequestAcknowledge = "PartRequestAcknowledgeButton";
            string varPartRequestAssign = "PartRequestAssignButton";
            string varPartRequestComplete = "PartRequestCompleteButton";
            string varPartRequestCancelAcknowledge = "PartRequestCancelAcknowledgeButton";

            var serviceType = Page.PrimaryServiceType;

            // set default condition
            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequest).FirstOrDefault().IsHidden = (serviceType != "PartRequest");
            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestUpdate).FirstOrDefault().IsHidden = true;
            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestIssue).FirstOrDefault().IsHidden = true;
            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestCancel).FirstOrDefault().IsHidden = true;
            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestAcknowledge).FirstOrDefault().IsHidden = true;
            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestAssign).FirstOrDefault().IsHidden = true;
            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestComplete).FirstOrDefault().IsHidden = true;
            this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestCancelAcknowledge).FirstOrDefault().IsHidden = true;

            if (results.GridContext.SelectedRowID != null)
            {
                try
                {
                    /// get selected row column value 
                    string ReqStatus = results.GridContext.GetSelectedCell("RequestStatus").ToString();

                    // visible criteria for panel button 
                    bool enRequestUpdate = (" REQUESTED,ACKNOWLEDGED,ASSIGNED".IndexOf(ReqStatus) > 0);
                    bool enRequestIssue = (" COMPLETED".IndexOf(ReqStatus) > 0);
                    bool enRequestCancel = (" REQUESTED,ACKNOWLEDGED,ASSIGNED,COMPLETED".IndexOf(ReqStatus) > 0);
                    bool enRequestAcknowledge = (" REQUESTED".IndexOf(ReqStatus) > 0);
                    bool enRequestAssign = (" ACKNOWLEDGED,ASSIGNED".IndexOf(ReqStatus) > 0);
                    bool enRequestComplete = (" ASSIGNED".IndexOf(ReqStatus) > 0);
                    bool enRequestCancelAcknowledge = (" CANCELLED".IndexOf(ReqStatus) > 0);

                    // set hidden == !(visible)
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequest).FirstOrDefault().IsHidden = (serviceType != "PartRequest");
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestUpdate).FirstOrDefault().IsHidden = !enRequestUpdate || serviceType != "PartRequest";
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestIssue).FirstOrDefault().IsHidden = !enRequestIssue || serviceType != "PartRequest";
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestCancel).FirstOrDefault().IsHidden = !enRequestCancel || serviceType != "PartRequest";
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestAcknowledge).FirstOrDefault().IsHidden = !enRequestAcknowledge || serviceType == "PartRequest";
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestAssign).FirstOrDefault().IsHidden = !enRequestAssign || serviceType == "PartRequest";
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestComplete).FirstOrDefault().IsHidden = !enRequestComplete || serviceType == "PartRequest";
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestCancelAcknowledge).FirstOrDefault().IsHidden = !enRequestCancelAcknowledge || serviceType == "PartRequest";

                }
                catch (Exception)
                {
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequest).FirstOrDefault().IsHidden = (serviceType != "PartRequest");
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestUpdate).FirstOrDefault().IsHidden = true;
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestIssue).FirstOrDefault().IsHidden = true;
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestCancel).FirstOrDefault().IsHidden = true;
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestAcknowledge).FirstOrDefault().IsHidden = true;
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestAssign).FirstOrDefault().IsHidden = true;
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestComplete).FirstOrDefault().IsHidden = true;
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varPartRequestCancelAcknowledge).FirstOrDefault().IsHidden = true;
                }
            }
        }

        public void ForceDataContractMembers()
        {
            if (results != null)
            {
                if (results.SelectedRowID != null)
                {
                    Page.PortalContext.DataContract.SetValueByName("SelectedResource", results.GridContext.GetSelectedCell("Name"));
                    Page.PortalContext.DataContract.SetValueByName("SelectedRequestOrder", results.GridContext.GetSelectedCell("RequestOrder"));
                    Page.PortalContext.DataContract.SetValueByName("SelectedRequestType", results.GridContext.GetSelectedCell("RequestType"));
                    Page.PortalContext.DataContract.SetValueByName("SelectedJobOrder", results.GridContext.GetSelectedCell("JobOrder"));
                }
            }
        }

        //public void ResultsGrid_PreRender(object sender, EventArgs e)
        //{
        //    ManagePanelButton();
        //}

        protected override void OnPreRender(EventArgs e)
        {
            if (string.Compare(this.PrimaryServiceType, "PartRequest", true) == 0)
            {
                Personalization.PageContent content = Page.Model.PublishedContent as PageContent;
                if (content != null)
                {
                    var helpUrl = @"onlinehelpoutput/psf_help/portalsfug_csh.htm#WIPTracking/Technician_Requests.htm";
                    content.HelpFileURL = helpUrl;
                }
            }
            base.OnPreRender(e);

            ScriptManager.RegisterStartupScript(this, this.GetType(), "SearchLayout_AddSlideoutToogler", string.Format("SearchLayout_AddSlideoutToogler('{0}');", results.ClientID), true);

            if (!Page.IsPostBack)
            {
                LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);

                System.Web.UI.WebControls.ListItemCollection items = requeststatus.DropDownControl.Items;
                items.Clear();
                items.Add(string.Empty);
                items.Add(labelCache.GetLabelByName("PartRequest_Requested").Value);
                items.Add(labelCache.GetLabelByName("PartRequest_Acknowledged").Value);
                items.Add(labelCache.GetLabelByName("PartRequest_Assigned").Value);
                items.Add(labelCache.GetLabelByName("PartRequest_Completed").Value);
                items.Add(labelCache.GetLabelByName("PartRequest_Cancelled").Value);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument")
            {
                if (results.SelectedRowID != null)
                {
                    selectedrowaftersubmit.Data = results.GridContext.SelectedRowID.ToString();
                    //results.ClearData();
                    //results.GridContext.LoadData();

                    UpdateParams();
                    int curPage = 1;
                    int rowsperpage = results.Settings.RowsPerPage ?? 20;
                    int addPage = Convert.ToInt32(selectedrowaftersubmit.Data.ToString()) / rowsperpage;
                    int newIndex = Convert.ToInt32(selectedrowaftersubmit.Data.ToString()) % rowsperpage;
                    curPage = curPage + addPage;
                    results.BoundContext.CurrentPage = curPage;
                    results.GridContext.LoadData();
                    int totalpage = (Convert.ToInt32(results.GridContext.GetTotalRows() / rowsperpage)) + 1;
                    int newResultIndex = Convert.ToInt32(results.GridContext.GetTotalRows() % rowsperpage);
                    string ReqOrder = "";
                    if (results.BoundContext.CurrentPage < totalpage)
                    {
                        if (selectedrowaftersubmit.Data != null)
                            ReqOrder = results.GridContext.GetCell(newIndex, "RequestOrder").ToString();
                    }
                    else
                    {
                        if (selectedrowaftersubmit.Data != null && newIndex < newResultIndex)
                            ReqOrder = results.GridContext.GetCell(newIndex, "RequestOrder").ToString();
                    }
                    if (ReqOrder == selectedrow.Data.ToString())
                    {
                        results.SelectedRowID = selectedrowaftersubmit.Data.ToString();
                        results.BoundContext.CurrentPage = curPage;
                    }
                } else
                    UpdateParams();
            }
            //results.PreRender += new EventHandler(ResultsGrid_PreRender);


            ForceDataContractMembers();

            ManagePanelButton();
        }
    }
}
