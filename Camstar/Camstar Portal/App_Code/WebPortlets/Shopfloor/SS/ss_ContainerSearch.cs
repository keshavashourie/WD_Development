// Copyright 2019 Siemens
using System;
using System.Web;
using System.Web.UI;
using System.Collections;
using Camstar.WebPortal.WebPortlets;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebGridControls;

/// <summary>
/// Summary description for ContainerSearch
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class ss_ContainerSearch : MatrixWebPart
    {
        public ss_ContainerSearch()
        {
        }

        protected virtual CWC.Button SearchButton { get { return Page.FindCamstarControl("ContainerSearch_Search") as CWC.Button; } }
        protected bool isSearch = false;
        protected virtual CWC.TextBox Current_Qty { get { return Page.FindCamstarControl("ContainerSearchFilters_Qty") as CWC.TextBox; } }
        protected virtual CWC.DropDownList Current_QtyOperator { get { return Page.FindCamstarControl("ContainerSearchFilters_QtyOperator") as CWC.DropDownList; } }
        protected virtual CWC.DropDownList Current_InRework { get { return Page.FindCamstarControl("ContainerSearchFilters_InRework") as CWC.DropDownList; } }
        protected virtual CWC.NamedObject Current_ReworkReason { get { return Page.FindCamstarControl("ContainerSearchFilters_ReworkReason") as CWC.NamedObject; } }
        protected virtual CWC.DropDownList Current_OnHold { get { return Page.FindCamstarControl("ContainerSearchFilters_OnHold") as CWC.DropDownList; } }
        protected virtual CWC.NamedObject Current_OnHoldReason { get { return Page.FindCamstarControl("ContainerSearchFilters_HoldReason") as CWC.NamedObject; } }
        protected virtual CWC.NamedObject Query_UserQuery { get { return Page.FindCamstarControl("ContainersTxn_UserQuery") as CWC.NamedObject; } }

        protected virtual SectionDropDown ContainerDocuments { get { return Page.FindCamstarControl("ContainerDocuments_Section") as SectionDropDown; } }
        protected virtual SectionDropDown ContainerStatus { get { return Page.FindCamstarControl("ContainerStatus_Section") as SectionDropDown; } }
        protected virtual CWC.Button ContainerMfgAuditTrail { get { return Page.FindCamstarControl("ContainerMfgAuditTrail") as CWC.Button; } }
        protected virtual CWC.Label ActionPanelInstructions { get { return Page.FindCamstarControl("ActionPanelInstructions") as CWC.Label; } }

        protected virtual CWC.ContainerList ContainerStatus_ContainerName { get { return Page.FindCamstarControl("ContainerStatus_ContainerName") as CWC.ContainerList; } }
        protected virtual JQDataGrid resultsGrid { get { return Page.FindCamstarControl("ContainerSearch_ResultsGrid") as JQDataGrid; } }
        protected virtual JQTabContainer filterTabContainer { get { return Page.FindCamstarControl("ContainerSearch_TabContainer") as JQTabContainer; } }
        protected virtual CWC.TextBox ContainerSearchFilters_ContainerName { get { return Page.FindCamstarControl("ContainerSearchFilters_ContainerName") as CWC.TextBox; } }

        
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.OnRequestControlSelectionValues += Page_OnRequestControlSelectionValues;
            _isTabChnaged = false;
        }

        protected virtual void filterTabContainer_SelectedIndexChanged(object sender, EventArgs e)
        {
            _isTabChnaged = true;
        }

        protected virtual void Page_OnRequestControlSelectionValues(object obj, FormsFramework.SelectionControlProcessingEventArgs e)
        {
            if (!Page.IsPostBack || ((System.Web.UI.Control)(e.Control)).ID == null || !((System.Web.UI.Control)(e.Control)).ID.Contains("ContainerSearch_ResultsGrid"))
                return;//only process if search

            //set Search flag so grid title will update
            isSearch = true;

            //ensure user query is skipped if UQ tab is not selected
            if (filterTabContainer != null && resultsGrid != null)
            {
                if (filterTabContainer.SelectedIndex == 0 || filterTabContainer.SelectedIndex == 1)
                {
                    if (e.Control == resultsGrid && e.Data is OM.ContainersTxn)
                    {
                        // clear the 3rd tab filters
                        var data = e.Data as OM.ContainersTxn;
                        data.UserQuery = null;
                        data.UserQueryParameters = null;
                    }
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
                return;

            if (ContainerSearchFilters_ContainerName.IsChanged)
                SearchButton.Click += new EventHandler(SearchButton_Click);

            //ensure single row selection data is maintained prior to page events
            if (resultsGrid.GridContext.SelectedRowIDs != null && resultsGrid.GridContext.SelectedRowIDs.Count == 1)
            {
                ContainerStatus_ContainerName.Data = resultsGrid.GridContext.SelectedRowIDs[0];
                ContainerDocuments.Enabled = !(Page.FindCamstarControl("ContainerStatus_ViewDocuments") as CWC.ViewDocumentsControl).IsEmpty;
                var qlinksWP = Page.FindIForm("QuickLinksWP") as WebPartBase;
                if (qlinksWP != null)
                {
                    qlinksWP.RenderToClient = true;
                }
            }
            
            //add selectedRowIDs to session
            if (resultsGrid.GridContext.SelectedRowIDs != null)
            {
                string[] selectedContainers = resultsGrid.GridContext.SelectedRowIDs.ToArray();
                Page.Session.Add("selectedContainers", selectedContainers);
            }

            filterTabContainer.SelectedIndexChanged += new EventHandler(filterTabContainer_SelectedIndexChanged);
        }

        protected virtual void SearchButton_Click(object sender, EventArgs e)
        {
            resultsGrid.ClearData();
            resultsGrid.Action_Reload("");
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //Wire up handlers with additional actions
            Current_Qty.TextControl.Attributes.Add("onchange", "ContainerSearch_SetDefaultDDLValue('" + Current_QtyOperator.TextEditControl.ClientID + "', '" + Current_QtyOperator.ClientID + "', '=', '10')");
            Current_ReworkReason.TextEditControl.Attributes.Add("onchange", "ContainerSearch_SetDefaultDDLValue('" + Current_InRework.TextEditControl.ClientID + "', '" + Current_InRework.ClientID + "', 'True', '1')");
            Current_OnHoldReason.TextEditControl.Attributes.Add("onchange", "ContainerSearch_SetDefaultDDLValue('" + Current_OnHold.TextEditControl.ClientID + "', '" + Current_OnHold.ClientID + "', 'True', '1')");

            //force script rendering even if WebPart is hidden
            ScriptManager.RegisterStartupScript(Page.Form, Page.Form.GetType(), "ContainerSearchFunctions",
                string.Format("ContainerSearch_AddToggleSearchAttr({0},{1},{2},{3});",
                    Page.IsPostBack.ToString().ToLower(), isSearch.ToString().ToLower(), _isTabChnaged.ToString().ToLower(), (Query_UserQuery.Data != null).ToString().ToLower()), true);

            //set info based on grid state
            if (resultsGrid.GridContext.SelectedRowIDs != null && resultsGrid.GridContext.SelectedRowIDs.Count > 0)
            {
                ContainerStatus.Visible = ContainerDocuments.Visible = ContainerMfgAuditTrail.Visible = resultsGrid.GridContext.SelectedRowIDs.Count == 1;

                LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
                if (resultsGrid.GridContext.SelectedRowIDs.Count > 1)
                {
                    ActionPanelInstructions.LabelText = string.Format(labelCache.GetLabelByName("ContainerSearchDetail_ActionInfo_Multi").Value, "<b>" + resultsGrid.GridContext.SelectedRowIDs.Count + "</b>");
                    ActionPanelInstructions.Style.Clear();
                }
                else
                    ActionPanelInstructions.Style.Add("display", "none");
            }
        }

        private bool _isTabChnaged = false;
    }
}


