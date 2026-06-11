// Copyright Siemens 2023  
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using System;
using System.Linq;
using System.Web.UI;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;

/// <summary>
/// Summary description for ContainerSearch
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class ContainerSearchVP_R2 : MatrixWebPart
    {
        public ContainerSearchVP_R2()
        {
        }
        protected virtual CWC.Button SearchButton { get { return Page.FindCamstarControl("ContainerSearch_Search") as CWC.Button; } }
        protected virtual CWC.Button ResetButton { get { return Page.FindCamstarControl("ContainerSearch_ClearAll") as CWC.Button; } }

        protected bool isSearch = false;
        protected bool isLineAssignment = false;
        protected virtual CWC.TextBox Current_Qty { get { return Page.FindCamstarControl("ContainerSearchFilters_Qty") as CWC.TextBox; } }
        protected virtual CWC.DropDownList Current_QtyOperator { get { return Page.FindCamstarControl("ContainerSearchFilters_QtyOperator") as CWC.DropDownList; } }
        protected virtual CWC.DropDownList Current_InRework { get { return Page.FindCamstarControl("ContainerSearchFilters_InRework") as CWC.DropDownList; } }
        protected virtual CWC.NamedObject Current_ReworkReason { get { return Page.FindCamstarControl("ContainerSearchFilters_ReworkReason") as CWC.NamedObject; } }
        protected virtual CWC.DropDownList Current_OnHold { get { return Page.FindCamstarControl("ContainerSearchFilters_OnHold") as CWC.DropDownList; } }
        protected virtual CWC.NamedObject Current_OnHoldReason { get { return Page.FindCamstarControl("ContainerSearchFilters_HoldReason") as CWC.NamedObject; } }
        protected virtual CWC.NamedObject Query_UserQuery { get { return Page.FindCamstarControl("ContainersTxn_UserQuery") as CWC.NamedObject; } }
        protected virtual CWC.CheckBox Current_ApplyLineAssigment { get { return Page.FindCamstarControl("CurrentStatus_ApplyLineAssignment") as CWC.CheckBox; } }
        protected virtual CWC.CheckBox InQualityControl { get { return Page.FindCamstarControl("InQualityControl") as CWC.CheckBox; } }
        protected virtual CWC.CheckBox History_ApplyLineAssigment { get { return Page.FindCamstarControl("History_ApplyLineAssignment") as CWC.CheckBox; } }

        protected virtual SectionDropDown ContainerDocuments { get { return Page.FindCamstarControl("ContainerDocuments_Section") as SectionDropDown; } }
        protected virtual SectionDropDown ContainerStatus { get { return Page.FindCamstarControl("ContainerStatus_Section") as SectionDropDown; } }
        protected virtual CWC.Button ContainerMfgAuditTrail { get { return Page.FindCamstarControl("ContainerMfgAuditTrail") as CWC.Button; } }
        protected virtual CWC.Label ActionPanelInstructions { get { return Page.FindCamstarControl("ActionPanelInstructions") as CWC.Label; } }

        protected virtual CWC.ContainerList ContainerStatus_ContainerName { get { return Page.FindCamstarControl("ContainerStatus_ContainerName") as CWC.ContainerList; } }
        protected virtual JQDataGrid ResultsGrid { get { return Page.FindCamstarControl("ContainerSearch_ResultsGrid") as JQDataGrid; } }
        protected virtual JQTabContainer FilterTabContainer { get { return Page.FindCamstarControl("ContainerSearch_TabContainer") as JQTabContainer; } }
        protected virtual CWC.TextBox ContainerSearchFilters_ContainerName { get { return Page.FindCamstarControl("ContainerSearchFilters_ContainerName") as CWC.TextBox; } }

        protected virtual CWC.NamedObject LAOperation { get { return Page.FindCamstarControl("ContainerSearchFilters_LAOperation") as CWC.NamedObject; } }
        protected virtual CWC.NamedObject LAResource { get { return Page.FindCamstarControl("ContainerSearchFilters_LAResource") as CWC.NamedObject; } }
        protected virtual CWC.NamedObject LAWorkstation { get { return Page.FindCamstarControl("ContainerSearchFilters_LAWorkstation") as CWC.NamedObject; } }
        protected virtual CWC.NamedObject LAWorkCenter { get { return Page.FindCamstarControl("ContainerSearchFilters_LAWorkCenter") as CWC.NamedObject; } }
        protected virtual WebPartBase ContainerSearchWP { get { return Page.FindIForm("ContainerSearch_ResultsWP") as WebPartBase; } }


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.OnRequestControlSelectionValues += Page_OnRequestControlSelectionValues;
            Page.OnClearValues += Page_OnClearValues;
            Page.OnDisplayFormSelectionValues += Page_OnDisplayFormSelectionValues;        
            _isTabChnaged = false;
        }

        private void Page_OnDisplayFormSelectionValues(object sender, FormsFramework.FormProcessingEventArgs e)
        {
            if (e.Environment != null && e.Environment.IsEmpty)
            {
                (ResultsGrid.GridContext as DataGridContext).ClearData();
                ResultsGrid.SetSelectionValues(e.Environment.SelectionValues);
            }
        }

        private void Page_OnClearValues(object sender, FormsFramework.ServiceDataEventArgs e)
        {
            (ResultsGrid.GridContext as DataGridContext).ClearData();

            resetSelectedIndexForUserQueryTabContainer();
        }

        private void resetSelectedIndexForUserQueryTabContainer()
        {
            var userQuery_TabContainer = Query_UserQuery.NamingContainer.FindControl("UserQuery_TabContainer") as JQTabContainer;
            userQuery_TabContainer.SelectedIndex = 0;
        }

        protected virtual void filterTabContainer_SelectedIndexChanged(object sender, EventArgs e)
        {
            _isTabChnaged = true;
        }

        protected virtual void Page_OnRequestControlSelectionValues(object obj, FormsFramework.SelectionControlProcessingEventArgs e)
        {
            if (!Page.IsPostBack || ((Control)e.Control).ID == null || !((Control)e.Control).ID.Contains("ContainerSearch_ResultsGrid"))
                return;//only process if search

            (ResultsGrid.GridContext as DataGridContext).ClearData();

            //set Search flag so grid title will update
            isSearch = true;

            //ensure user query is skipped if UQ tab is not selected
            if (FilterTabContainer != null && ResultsGrid != null)
            {
                if (FilterTabContainer.SelectedIndex == 0 )
                {
                    var data = e.Data as OM.ContainersTxn;
                    UIComponentDataContract contract = Page.SessionDataContract;
                    if (e.Control == ResultsGrid && data is OM.ContainersTxn)
                    {
                        // clear the 3rd tab filters
                        data.UserQuery = null;
                        data.UserQueryParameters = null;
                    }
                    if (Current_ApplyLineAssigment != null && Current_ApplyLineAssigment.IsChecked)
                    {
                        data.ContainerSearchFilters.ApplyLineAssignmentCurrentStatus = true;
                        isLineAssignment = true;
                    }
                    else if (History_ApplyLineAssigment != null && History_ApplyLineAssigment.IsChecked)
                    {
                        data.ContainerSearchFilters.ApplyLineAssignmentHistory = true;
                        isLineAssignment = true;
                    }
                    if (isLineAssignment)
                    {
                        LAOperation.Data = data.ContainerSearchFilters.LAOperation = contract.GetValueByName<OM.NamedObjectRef>(DataMemberConstants.Operation);
                        LAResource.Data = data.ContainerSearchFilters.LAResource = contract.GetValueByName<OM.NamedObjectRef>(DataMemberConstants.Resource);
                        LAWorkCenter.Data = data.ContainerSearchFilters.LAWorkCenter = contract.GetValueByName<OM.NamedObjectRef>(DataMemberConstants.WorkCenter);
                        LAWorkstation.Data = data.ContainerSearchFilters.LAWorkstation = contract.GetValueByName<OM.NamedObjectRef>(DataMemberConstants.WorkStation);
                    }
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
                return;

            ResultsGrid.RowSelected += ResultsGrid_RowSelected;
            FilterTabContainer.SelectedIndexChanged += new EventHandler(filterTabContainer_SelectedIndexChanged);
            ResetButton.Click += (object sender, EventArgs ea) => { LoadActionsStatus(); };

            Page.LoadComplete += Page_LoadComplete;
        }

        private void Page_LoadComplete(object sender, EventArgs e)
        {
            if(Page.EventArgument != "OnRowSelected")
                LoadActionsStatus();
        }

        private ResponseData ResultsGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            LoadActionsStatus();
            return null;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //Wire up handlers with additional actions
            Current_Qty.TextControl.Attributes.Add("onchange", "ContainerSearch_SetDefaultDDLValue('" + Current_QtyOperator.TextEditControl.ClientID + "', '" + Current_QtyOperator.ClientID + "', '=', '10')");
            Current_ReworkReason.TextEditControl.Attributes.Add("onchange", "ContainerSearchVPR2_SetDefaultDDLValue('" + Current_InRework.TextEditControl.ClientID + "', '" + Current_InRework.ClientID + "', 'Yes', '1','" + Current_ReworkReason.TextEditControl.ClientID + "')");
            Current_OnHoldReason.TextEditControl.Attributes.Add("onchange", "ContainerSearchVPR2_SetDefaultDDLValue('" + Current_OnHold.TextEditControl.ClientID + "', '" + Current_OnHold.ClientID + "', 'Yes', '1','" + Current_OnHoldReason.TextEditControl.ClientID + "')");

            //force script rendering even if WebPart is hidden
            if (!Page.EventArgument.ToLower().Contains("onrowselected"))
                ScriptManager.RegisterStartupScript(this, this.GetType(), "resetResultsGridScrollTop", $"ContainerSearchVPR2_ResetResultsGridScrollTop();", true);

            ScriptManager.RegisterStartupScript(Page.Form, Page.Form.GetType(), "ContainerSearchFunctions",
                string.Format("ContainerSearchVPR2_AddToggleSearchAttr({0},{1},{2},{3});",
                    Page.IsPostBack.ToString().ToLower(), isSearch.ToString().ToLower(), _isTabChnaged.ToString().ToLower(), (Query_UserQuery.Data != null).ToString().ToLower()), true);

            ScriptManager.RegisterStartupScript(Page.Form, Page.Form.GetType(), "SearchLayoutFunctions", string.Format("SearchLayout_AddSlideoutToogler('{0}');", ResultsGrid.ClientID), true);
            
            //set info based on grid state
            if (ResultsGrid.GridContext.SelectedRowIDs != null && ResultsGrid.GridContext.SelectedRowIDs.Count == 1)
                ContainerStatus_ContainerName.Data = ResultsGrid.GridContext.SelectedRowIDs.First();
            else
                ContainerStatus_ContainerName.ClearData();

            // Refresh QuickLinksWP
            (Page.FindIForm("QuickLinksWP") as WebPartBase).RenderToClient = true;

        }

        public virtual void LoadActionsStatus()
        {
            Page.ActionDispatcher.LoadActionsStatus();
            (Page.FindIForm("ActionsControl") as ActionsControl).RenderToClient = true;
        }
        private bool _isTabChnaged = false;
    }
}
