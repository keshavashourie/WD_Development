// Copyright Siemens 2025  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Camstar.WebPortal.PortalConfiguration;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CamstarPortal.WebControls.Accordion;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using PERS = Camstar.WebPortal.Personalization;
using CamstarPortal.WebControls;
using System.Collections;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using System.Web.UI;
using System.Text.RegularExpressions;
using WcfUtil = Camstar.WebPortal.WCFUtilities;
using OS = Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class MDL_InstanceList : MatrixWebPart
    {

        #region Controls

        protected virtual CWC.TextBox InstanceNameTxt { get { return Page.FindCamstarControl("InstanceNameTxt") as CWC.TextBox; } }
        protected virtual CWC.TextBox DescriptionTxt { get { return Page.FindCamstarControl("DescriptionFilter") as CWC.TextBox; } }
        protected virtual CWC.NamedObject LastEditNDO { get { return Page.FindCamstarControl("LastEditNDO") as CWC.NamedObject; } }
        protected virtual CWC.CheckBox ShowActiveChk { get { return Page.FindCamstarControl("ShowActiveChk") as CWC.CheckBox; } }
        protected virtual CWC.CheckBox ShowRORChk { get { return Page.FindCamstarControl("ShowRORChk") as CWC.CheckBox; } }
        protected virtual JQDataGrid InstanceGrid { get { return Page.FindCamstarControl("InstanceGrid") as JQDataGrid; } }
        protected virtual CWC.Button ClearAllBtn { get { return Page.FindCamstarControl("ClearAllBtn") as CWC.Button; } }

        protected virtual CWC.TextBox AssociatedPackagesTxt { get { return Page.FindCamstarControl("AssociatedPackagesTxt") as CWC.TextBox; } }
        protected virtual CWC.CheckBox InstanceLockedChk { get { return Page.FindCamstarControl("InstanceLockedChk") as CWC.CheckBox; } }

        protected virtual Accordion CollapsibleSectionsAccordion
        {
            get
            {
                return Page.FindIForm("CollapsibleSectionsAccordion") as Accordion;
            }
        }
        #endregion

        #region WebParts
        protected virtual WebPartBase FilterWebPart { get { return Page.FindIForm("MDL_Filter_WP") as WebPartBase; } }

        #endregion


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (HttpContext.Current.CurrentHandler != null && !(HttpContext.Current.CurrentHandler is AjaxEntry))
            {
                CDOData? data = null;
                if (!Page.IsPostBack)
                {
                    // page was loaded from Studio directly and there is no PrimaryServiceType specified.
                    if (string.IsNullOrEmpty(Page.PrimaryServiceType) && string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.Maint_Mod]))
                    {
                        if (CamstarPortalSection.Settings.CDOFormsSettings != null && CamstarPortalSection.Settings.CDOFormsSettings.CDOForms != null)
                        {
                            var pageName = PERS.PageMapping.ExtractPageName(Page.Request.Path);
                            var formInfo = CamstarPortalSection.Settings.CDOFormsSettings.CDOForms.FirstOrDefault(cdoForm => string.Equals(cdoForm.PageName, pageName, StringComparison.OrdinalIgnoreCase));
                            if (formInfo != null && !string.IsNullOrEmpty(formInfo.Service))
                                Page.PrimaryServiceType = formInfo.Service;
                        }
                    }
                    if (!string.IsNullOrEmpty(Page.PrimaryServiceType))
                    {
                        var cache = new MaintCDOCache();
                        data = cache.GetMaintCDOData(Page.PrimaryServiceType);
                    }
                }

                var pc = (Page.PortalContext as MaintenanceBehaviorContext);

                if (!string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.Id_Mod]))
                    pc.CDOID = Page.Request.QueryString[QueryStringConstants.Id_Mod];
                else if (data.HasValue)
                    pc.CDOID = data.Value.CDODefID;

                if (!string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.IsRDO_Mod]))
                    pc.IsRDO = bool.Parse(Page.Request.QueryString[QueryStringConstants.IsRDO_Mod]);
                else if (data.HasValue)
                    pc.IsRDO = data.Value.IsRDO.HasValue && data.Value.IsRDO.Value;

                if (!string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.Name_Mod]))
                    pc.CDODisplayName = Page.Request.QueryString[QueryStringConstants.Name_Mod];
                else if (data.HasValue)
                    pc.CDODisplayName = data.Value.CDODisplayName;

                if (!string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.CDOName_Mod]))
                    pc.CDOTypeName = Page.Request.QueryString[QueryStringConstants.CDOName_Mod];
                else if (data.HasValue)
                    pc.CDOTypeName = data.Value.CDOName;

                if (!string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.Maint_Mod]))
                    pc.MaintService = Page.Request.QueryString[QueryStringConstants.Maint_Mod];
                else if (data.HasValue)
                    pc.MaintService = data.Value.MaintService;

                if (!string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.MaintTypeId_Mod]))
                    pc.MaintenanceTypeID = Page.Request.QueryString[QueryStringConstants.MaintTypeId_Mod];
                else if (data.HasValue)
                    pc.MaintenanceTypeID = data.Value.MaintTypeID;

                if (!string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.WIP_Mod]))
                    pc.WIPAvailable = bool.Parse(Page.Request.QueryString[QueryStringConstants.WIP_Mod]);
                else if (data.HasValue)
                    pc.WIPAvailable = data.Value.IsWIPSupported;

                if (!string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.PStackId_Mod]))
                    pc.ParentStackId = Page.Request.QueryString[QueryStringConstants.PStackId_Mod];

                if (!string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.IsNew_Mod]))
                {
                    if (bool.Parse(Page.Request.QueryString[QueryStringConstants.IsNew_Mod]))
                    {
                        pc.State = MaintenanceBehaviorContext.MaintenanceState.New;
                    }
                }

                if (Page.EventArgument.Contains("UIAction"))
                {
                    if (Page.EventArgument.Contains("GridView")) pc.ViewMode = InstanceListMode.Grid.ToString();
                    else if (Page.EventArgument.Contains("List")) pc.ViewMode = InstanceListMode.List.ToString();
                    else if (Page.EventArgument.Contains("Bulk")) pc.ViewMode = InstanceListMode.Edit.ToString();
                }

                if (pc.ViewMode == null || (pc.ViewMode == "Grid" && Page.EventArgument == "OnRowSelected"))
                    pc.ViewMode = InstanceListMode.List.ToString();

                if (!Page.IsPostBack)
                {
                    if (!string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.InstanceName_Mod]))
                    {
                        var instName = Page.Request.QueryString[QueryStringConstants.InstanceName_Mod];
                        if (!pc.IsRDO)
                        {
                            pc.Current = new OM.NamedObjectRef(instName);
                        }
                        else
                        {
                            var revision = Page.Request.QueryString[QueryStringConstants.InstanceRev_Mod];
                            pc.Current = new OM.RevisionedObjectRef(instName, revision);
                        }
                        pc.State = MaintenanceBehaviorContext.MaintenanceState.Edit;
                    }
                    else if (!string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.InstanceId_Mod]))
                    {
                        pc.Current = new OM.BaseObjectRef(Page.Request.QueryString[QueryStringConstants.InstanceId_Mod]);
                    }
                }
            }
        }
        public override void RequestSelectionValues(OM.Info serviceInfo, OM.Service serviceData)
        {
            (InstanceGrid.GridContext as SelValGridContext).RequestSpecificTypeOnly = true;
            base.RequestSelectionValues(serviceInfo, serviceData);
        }
        public override void LoadPersonalization()
        {
            base.LoadPersonalization();
            var pc = (Page.PortalContext as MaintenanceBehaviorContext);
            if (pc != null && !string.IsNullOrEmpty(pc.MaintService))
                Page.PrimaryServiceType = pc.MaintService;
        }

        public virtual ResponseData InstanceGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            return DoRowSelected(sender, args, false);
        }
        protected ResponseData DoRowSelected(object sender, JQGridEventArgs args, bool clearRow=false)
        { 
            var pc = Page.PortalContext as MaintenanceBehaviorContext;
            var selectedItems = InstanceGrid.GridContext.GetSelectedItems(false);
            if (selectedItems != null && selectedItems.Count() > 0)
            {
                var row = clearRow ? null : selectedItems.First() as DataRow;
                OM.RecordSet recordSet;
                var rowsCount = InstanceGrid.TotalRowCount;
                var totalPagesCount = (InstanceGrid.TotalRowCount / InstanceGrid.GridContext.RowsPerPage) + (InstanceGrid.TotalRowCount % InstanceGrid.GridContext.RowsPerPage == 0 ? 0 : 1);
                int valueIndex = -1;
                var selValContext = InstanceGrid.GridContext as SelValGridContext;

                var value = pc.Current != null ? pc.Current.ID : string.Empty;
                var pcCurValue = pc.Current != null ? pc.Current.ID : string.Empty;
                bool isRdoNotResolved = false;
                if (string.IsNullOrEmpty(value))
                {
                    var ndo = pc.Current as OM.NamedObjectRef;
                    if (ndo != null && !string.IsNullOrEmpty(ndo.Name))
                        value = ndo.Name;
                    else
                    {
                        var rdo = pc.Current as OM.RevisionedObjectRef;
                        if (rdo != null && !string.IsNullOrEmpty(rdo.Name))
                        {
                            value = rdo.Name;
                            isRdoNotResolved = true;
                        }
                    }
                }
                //search for the instance, in case it is located on some other page, to go there
                for (int i = 1; i <= totalPagesCount && row == null; i++)
                {
                    InstanceGrid.GridContext.CurrentPage = i;
                    selValContext.GetSelectionValuesData(out recordSet, -1);
                    string instanceIdName = "InstanceId";
                    string headerName = instanceIdName;
                    if (string.IsNullOrEmpty(pcCurValue))
                        headerName = "Name";
                    valueIndex = Array.IndexOf(recordSet.Headers, recordSet.Headers.FirstOrDefault(h => h.Name.Equals(headerName)));
                    OM.Row selectedRow = null;
                    if (isRdoNotResolved)
                    {
                        var rdo = (OM.RevisionedObjectRef)pc.Current;
                        if (!string.IsNullOrEmpty(rdo.Revision))
                        {
                            var revisionIndex = Array.IndexOf(recordSet.Headers, recordSet.Headers.FirstOrDefault(h => string.Compare(h.Name, "Revision", true) == 0));
                            if (revisionIndex > -1)
                                selectedRow = recordSet.Rows.FirstOrDefault(r => r.Values[valueIndex].Equals(value) && r.Values[revisionIndex].Equals(rdo.Revision));
                        }
                        else // revision of record.
                        {
                            var rorId = Array.IndexOf(recordSet.Headers, recordSet.Headers.FirstOrDefault(h => string.Compare(h.Name, "RevOfRcd", true) == 0));
                            var instanceId = Array.IndexOf(recordSet.Headers, recordSet.Headers.FirstOrDefault(h => string.Compare(h.Name, instanceIdName, true) == 0));
                            if (rorId > -1 && instanceId > -1)
                                selectedRow = recordSet.Rows.FirstOrDefault(r => r.Values[valueIndex].Equals(value) && r.Values[rorId].Equals(r.Values[instanceId]));
                        }
                    }
                    else
                        selectedRow = recordSet.Rows.FirstOrDefault(r => r.Values[valueIndex].Equals(value));

                    if (selectedRow == null)
                        continue;
                    int realPage = i;
                    if (recordSet.Rows.Count() != InstanceGrid.GridContext.RowsPerPage)
                    {
                        for (int j = 0; j < recordSet.Rows.Count(); j++)
                        {
                            if (recordSet.Rows[j] == selectedRow)
                            {
                                realPage += (j / InstanceGrid.GridContext.RowsPerPage);
                                break;
                            }                            
                        }
                    }
                    recordSet.TotalCount = rowsCount;
                    InstanceGrid.SetSelectionValues(recordSet);
                    InstanceGrid.GridContext.CurrentPage = realPage;
                    if (realPage != i)
                        InstanceGrid.GridContext.LoadData();
                    int index = Array.IndexOf(recordSet.Headers, recordSet.Headers.FirstOrDefault(h => string.Compare(h.Name, instanceIdName, true) == 0));
                    if (index >= 0 && index < selectedRow.Values.Length)
                        InstanceGrid.BoundContext.SelectRow(selectedRow.Values[index], true);
                    selectedItems = InstanceGrid.GridContext.GetSelectedItems(false);
                    if (selectedItems != null && selectedItems.Count() > 0)
                        row = selectedItems.First() as DataRow;
                }

                var prevItem = pc.Current;

                if (row != null)
                    SetupPage(pc, row);
                else
                    DoSetupPage();

                if (CollapsibleSectionsAccordion != null && (prevItem ?? new OM.BaseObjectRef()).ID != pc.Current.ID)
                    CollapsibleSectionsAccordion.RestoreExpandedSections();
            }


            if (_mode == InstanceListMode.Grid)
                pc.ViewMode = InstanceListMode.List.ToString();

            Page.SetDefaultFocus();

            return null;
        }

        protected virtual InstanceListMode _mode
        {
            get { return (InstanceListMode)Enum.Parse(typeof(InstanceListMode), (Page.PortalContext as MaintenanceBehaviorContext).ViewMode); }
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/MDL_InstanceList.js");
        }

        protected override void OnLoad(EventArgs e)
        {
            FilterWebPart.PrimaryServiceType = (Page.PortalContext as MaintenanceBehaviorContext).MaintService;

            if (Page.IsPostBack)
                SessionVariables.SetValueByName("IsCopy", null);

            if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument")
            {
                var popupCmd = Page.PortalContext.DataContract.GetValueByName<string>("InstancePopupCommand");
                if (Page.SessionVariables.GetValueByName("IsCopy") != null)
                {
                    popupCmd = "copy";
                    Page.SessionVariables.SetValueByName("IsCopy", null);
                }
                    
                Page.PortalContext.DataContract.SetValueByName("InstancePopupCommand", "");
                if (!string.IsNullOrEmpty(popupCmd))
                {
                    if (popupCmd == "copy")
                    {
                        var name = Page.PortalContext.DataContract.GetValueByName<string>("SuggestedInstanceName");
                        var rev = Page.PortalContext.DataContract.GetValueByName<string>("SuggestedInstanceRevision");
                        Page.CopyCDO(name, rev, true);
                    }
                    else if (popupCmd == "delete")
                    {
                        Page.DeleteCDO(true);
                    }

                    else if (popupCmd == "addtopkg")
                    {          
                        Page.OnMaintSubmitButtonClicked(true);
                    }

                    (Page.PortalContext as MaintenanceBehaviorContext).ReloadInstanceList = true;
                }
            }

            HandleHideInstanceList();

            base.OnLoad(e);
            SetupControls();
            //SetupGrid();
        }

        /// <summary>
        /// The factory hierarchical model page needs to hide the Instance List. This is complicated by a few things:
        /// 1) Trying to do it with CSS classes causes side effects when loading the tabs for some reason.
        /// 2) This Webpart posts back and is contained in an iframe.
        /// 
        /// The solution is to add a new querystring parameter called HideInstanceList. If that is found and set to true
        /// we will execute some javacript. The javascript will hide the instance list when the page loads from a JQuery
        /// document ready() function because registering a startup script when the page first loads causes display issues.
        /// We can then handle the hide from the code behind when the page does its partial postback.
        /// </summary>
        protected void HandleHideInstanceList()
        {
            if (!Page.IsPostBack)
            {
                string hideInstanceList = !string.IsNullOrEmpty(Page.Request.QueryString["HideInstanceList"]) ?
                    Page.Request.QueryString["HideInstanceList"] : "false";

                // When the user clicks on one of the modeling page action buttons, the original
                // querystring is not sent back. Therefore, set the hideInstanceList value
                // in an attribute on the control for consumption on postback.
                FilterWebPart.Attributes.Add("HideInstanceList", hideInstanceList);
            }
            else
            {
                // We posted back. Get the HideInstanceList from the FilterWebPart's attributes.
                string hideInstanceList = FilterWebPart.Attributes["HideInstanceList"];

                if (Convert.ToBoolean(hideInstanceList) == true)
                {
                    // Tell the client to hide the Instance List.
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "hideInstanceList",
                                                      "MDL_InstanceList.hideInstanceList()", true);
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            var pc = Page.PortalContext as MaintenanceBehaviorContext;

            var currentPage = InstanceGrid.GridContext.CurrentPage;
            var selValContext = InstanceGrid.GridContext as SelValGridContext;
            OM.RecordSet recordSet = null;
            bool haveRecordSet = false;
            if (selValContext != null)
            {
                haveRecordSet = Page.EventArgument.Contains("NewRev") || Page.EventArgument.Contains("CopyRev") || Page.EventArgument.Contains("OnRowSelected:WebPart_MDL_Filter");
                if (!haveRecordSet)
                {
                    CheckFilter(pc, InstanceGrid.Data != null);
                    selValContext.GetSelectionValuesData(out recordSet, -1);
                    if (recordSet != null && !Page.IsPostBack)
                    {
                        ReloadInstanceList(pc);
                    }
                    haveRecordSet = recordSet != null && recordSet.Rows != null;
                }
            }
            SetupGrid(haveRecordSet);

            string currentId = null;
            if (pc != null && pc.Current != null)
                currentId = pc.Current.ID;

            if (pc != null)
            {
                if (pc.ReloadInstanceList)
                {
                    ReloadInstanceList(pc);
                    if (recordSet == null && Page.IsPostBack)
                        recordSet = InstanceGrid.Data as OM.RecordSet;
                }

                if (pc.State == MaintenanceBehaviorContext.MaintenanceState.New || pc.State == MaintenanceBehaviorContext.MaintenanceState.NewRev || pc.State == MaintenanceBehaviorContext.MaintenanceState.None)
                {
                    if (InstanceGrid.BoundContext.SelectedRowIDs != null)
                    {
                        InstanceGrid.BoundContext.SelectedRowIDs.Clear();
                        InstanceGrid.BoundContext.SelectedRowID = null;
                        RenderToClient = true;
                    }
                }
                else if (pc.State == MaintenanceBehaviorContext.MaintenanceState.Edit && pc.Current != null && !InstanceGrid.IsRowSelected)
                {
                    _clearStatusMessage = false; // keeps status message after new instance was created.
                    if (pc.IsSuccessMessage)
                        NavigateToItemPage(recordSet, currentPage, currentId);
                    RenderToClient = true;
                }

            }

            string instanceID = Page.Request.QueryString["InstanceID"];
            if (!string.IsNullOrEmpty(instanceID))
            {
                if (string.Compare(instanceID, "0000000000000000") == 0)
                {
                    bool preventNewVS = ViewState[QueryStringConstants.PreventNew] != null && !string.IsNullOrEmpty(ViewState[QueryStringConstants.PreventNew].ToString()) && Util.StringUtil.ToBool(ViewState[QueryStringConstants.PreventNew].ToString());
                    ViewState[QueryStringConstants.PreventNew] = "true";
                    (Page.PortalContext as MaintenanceBehaviorContext).PreventNew = true;
                    Page.NewActionClicked(this, null);
                }
                else
                {
                    InstanceGrid.GridContext.SelectedRowIDs = new List<string>() { instanceID };
                    DoRowSelected(null, null, false);
                }
            }

        }

        private void ReloadInstanceList(MaintenanceBehaviorContext ctx)
        {
            if (ctx != null)
            {
                InstanceGrid.ClearData();
                InstanceGrid.Action_Reload("");
                ctx.ReloadInstanceList = false;
                RenderToClient = true;
            }
        }

        private void NavigateToItemPage(OM.RecordSet recordSet, int currentPage, string currentId)
        {
            if (recordSet == null)
            {
                DoSetupPage();
                return;
            }
            MaintenanceBehaviorContext pc = Page.PortalContext as MaintenanceBehaviorContext;
            recordSet.TotalCount = InstanceGrid.TotalRowCount;
            InstanceGrid.SetSelectionValues(recordSet);
            InstanceGrid.GridContext.CurrentPage = currentPage;
            InstanceGrid.BoundContext.SelectRow(currentId, true);
            var selectedItems = InstanceGrid.GridContext.GetSelectedItems(false);
            var row = selectedItems.First() as DataRow;

            //  If the number of rows in the RecordSet does not match the RowsPerPage - must see if the selected item is on a "future" page
            if (recordSet.Rows == null || recordSet.Rows.Count() != InstanceGrid.GridContext.RowsPerPage)
            {
                string value = pc.Current != null ? pc.Current.ID : string.Empty;
                bool isRdoNotResolved = false;
                if (string.IsNullOrEmpty(value))
                {
                    var ndo = pc.Current as OM.NamedObjectRef;
                    if (ndo != null && !string.IsNullOrEmpty(ndo.Name))
                        value = ndo.Name;
                    else
                    {
                        var rdo = pc.Current as OM.RevisionedObjectRef;
                        if (rdo != null && !string.IsNullOrEmpty(rdo.Name))
                        {
                            value = rdo.Name;
                            isRdoNotResolved = true;
                        }
                    }
                }
                OM.Row selectedRow = null;
                string instanceIdName = "InstanceId";
                string headerName = instanceIdName;
                int valueIndex = Array.IndexOf(recordSet.Headers, recordSet.Headers.FirstOrDefault(h => h.Name.Equals(headerName)));
                if (isRdoNotResolved)
                {
                    var rdo = (OM.RevisionedObjectRef)pc.Current;
                    if (!string.IsNullOrEmpty(rdo.Revision))
                    {
                        var revisionIndex = Array.IndexOf(recordSet.Headers, recordSet.Headers.FirstOrDefault(h => string.Compare(h.Name, "Revision", true) == 0));
                        if (revisionIndex > -1)
                            selectedRow = recordSet.Rows.FirstOrDefault(r => r.Values[valueIndex].Equals(value) && r.Values[revisionIndex].Equals(rdo.Revision));
                    }
                    else // revision of record.
                    {
                        var rorId = Array.IndexOf(recordSet.Headers, recordSet.Headers.FirstOrDefault(h => string.Compare(h.Name, "RevOfRcd", true) == 0));
                        var instanceId = Array.IndexOf(recordSet.Headers, recordSet.Headers.FirstOrDefault(h => string.Compare(h.Name, instanceIdName, true) == 0));
                        if (rorId > -1 && instanceId > -1)
                            selectedRow = recordSet.Rows.FirstOrDefault(r => r.Values[valueIndex].Equals(value) && r.Values[rorId].Equals(r.Values[instanceId]));
                    }
                }
                else if (recordSet.Rows != null)
                    selectedRow = recordSet.Rows.FirstOrDefault(r => r.Values[valueIndex].Equals(value));

                if (selectedRow != null)
                {
                    int realPage = currentPage;
                    for (int j = 0; j < recordSet.Rows.Count(); j++)
                    {
                        if (recordSet.Rows[j] == selectedRow)
                        {
                            realPage += (j / InstanceGrid.GridContext.RowsPerPage);
                            break;
                        }
                    }
                    //  If realPage is not current page, clear the DataRow
                    if (currentPage != realPage)
                        row = null;
                }
            }
            if (row != null)
            {
                SetupPage(pc, row);
            }
            else if (!CamstarPortalSection.Settings.DefaultSettings.DoNotNavigateToModelingInstancePage)
            {
                //It's not in our current recordset. This means the item was likely renamed so that it is on a different
                //page. In that case, reset and begin looking from the start.
                DoRowSelected(null, null, true);
            }
            else 
            {
                DoSetupPage();
            }
        }
        void DoSetupPage()
        {
            MaintenanceBehaviorContext pc = Page.PortalContext as MaintenanceBehaviorContext;
            if (pc.Current != null)
            {
                //  Get data about the current instance that was just modified and use it for the SetupPage call
                CWC.TextBox txtName = Page.FindCamstarControl("NameTxt") as CWC.TextBox;
                CWC.TextBox txtRev = Page.FindCamstarControl("RevisionTxt") as CWC.TextBox;
                CWC.CheckBox chkROR = Page.FindControl("IsRORChk") as CWC.CheckBox;
                var instanceHeaderWp = Page.Manager.WebParts["MDL_InstanceHeader"];
                if (instanceHeaderWp != null)
                {
                    if (txtName == null)
                        txtName = instanceHeaderWp.FindControl("NameTxt") as CWC.TextBox;
                    if (txtRev == null)
                        txtRev = instanceHeaderWp.FindControl("RevisionTxt") as CWC.TextBox;
                    if (chkROR == null)
                        chkROR = instanceHeaderWp.FindControl("IsRORChk") as CWC.CheckBox;
                }
                string name = txtName != null ? txtName.Data as string : string.Empty;
                string rev = txtRev != null ? txtRev.Data as string : string.Empty;
                bool isROR = chkROR != null ? (bool)chkROR.Data : false;
                SetupPage(pc, null, pc.Current.ID, name, rev, isROR);
            }
        }
        void SetupPage(MaintenanceBehaviorContext pc, DataRow row, string id = null, string name = null, string rev = null, bool isROR = false)
        {
            if (row == null && (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name)))
                return;
            else if (row != null)
            {
                name = row[0] as string;
                rev = row[1] as string;
            }
            //The item is in our current recordset, so set up the Page
            if (pc.IsRDO)
            {
                if (row != null)
                {
                    id = row[4] as string;
                    isROR = string.Compare(row[2] as string, row[4] as string) == 0;
                }
                pc.Current = new OM.RevisionedObjectRef(name, rev);
                (pc.Current as OM.RevisionedObjectRef).ID = id;
                pc.IsROR = isROR;
                (pc.Current as OM.RevisionedObjectRef).RevisionOfRecord = pc.IsROR;
            }
            else
            {
                if (row != null)
                    id = row[2] as string;
                pc.Current = new OM.NamedObjectRef(name);
                (pc.Current as OM.NamedObjectRef).ID = id;
            }
            if (!string.IsNullOrEmpty(pc.CDOTypeName))
                pc.Current.CDOTypeName = pc.CDOTypeName;

            Page.PortalContext.DataContract.SetValueByName("SelectedInstanceRef", pc.Current.Clone());

            Page.LoadModelingValues(_clearStatusMessage);

            if (Page.MPCMEnabled)
            {
                int mainTypeID = int.Parse(pc.MaintenanceTypeID);
                Page.UpdateChangeMgtSaveButtonState(mainTypeID, pc.Current.ID);
            }

            // Setup data contracts to use them in copy function
            Page.PortalContext.DataContract.SetValueByName("InstanceName", name);
            var descr = Page.FindCamstarControl("DescriptionField") as CWC.TextBox;
            var notes = Page.FindCamstarControl("NotesField") as CWC.TextBox;

            Page.PortalContext.DataContract.SetValueByName("InstanceDescription", descr != null ? descr.Data : string.Empty);
            Page.PortalContext.DataContract.SetValueByName("InstanceNotes", notes != null ? notes.Data : string.Empty);
            Page.PortalContext.DataContract.SetValueByName("SuggestedInstanceName", "Copy of " + name);

            Page.PortalContext.DataContract.SetValueByName("InstanceCSS", pc.IsRDO ? "rdo" : "ndo");
            Page.PortalContext.DataContract.SetValueByName("InstancePopupCommand", "");

            Page.PortalContext.DataContract.SetValueByName("CDOTypeName", pc.CDOTypeName);
            Page.PortalContext.DataContract.SetValueByName("InstanceId", pc.Current.ID);
            Page.PortalContext.DataContract.SetValueByName("CDODisplayName", pc.CDODisplayName);

            Page.PortalContext.DataContract.SetValueByName("whereCame", "Modeling");
            Page.PortalContext.DataContract.SetValueByName("IsChangeMgtSettingsRequired", false);

            if (pc.IsRDO)
            {
                Page.PortalContext.DataContract.SetValueByName("InstanceRevision", rev);
                Page.PortalContext.DataContract.SetValueByName("InstanceIsROR", pc.IsROR);
                Page.PortalContext.DataContract.SetValueByName("SuggestedInstanceRevision", "Copy of " + rev);
                Page.PortalContext.DataContract.SetValueByName("InstanceNameDisable", true);
            }
        }

        protected virtual void SetupControls()
        {
            bool isGrid = _mode == InstanceListMode.Grid;

            DescriptionTxt.Visible = isGrid;
            LastEditNDO.Visible = isGrid;
            ShowActiveChk.Visible = isGrid;
            ShowRORChk.Visible = isGrid;
            ClearAllBtn.Visible = isGrid;

            InstanceGrid.Visible = (_mode != InstanceListMode.Undefined);
            InstanceNameTxt.Visible = (_mode != InstanceListMode.Undefined);

            Width = isGrid ? 950 : 230;

            if (_mode == InstanceListMode.List)
                InstanceNameTxt.LabelText = "Instances";

            if ((_mode == InstanceListMode.Undefined || _mode == InstanceListMode.List) && Page.FindIForm("ActionsControl") != null)
                (Page.FindIForm("ActionsControl") as ActionsControl).SelectedActions.Add("ListViewBtn");
        }

        protected virtual void SetupGrid(bool haveRecordset)
        {
            var pc = (Page.PortalContext as MaintenanceBehaviorContext);

            InstanceGrid.BoundContext.Fields.ForEach(f => { if (f.Width > 1) f.Visible = (_mode == InstanceListMode.Grid); });
            InstanceGrid.BoundContext.IdentityField.Visible = false;

            if (_mode == InstanceListMode.Grid)
            {
                InstanceGrid.BoundContext.Fields["Name"].Visible = true;
                InstanceGrid.BoundContext.Fields["Displayed"].Visible = pc.IsRDO;

                InstanceGrid.BoundContext.Width = 950;
                InstanceGrid.LabelPosition = PERS.LabelPositionType.Top;
                InstanceGrid.BoundContext.Attributes["listViewMode"] = ("list " + (pc.IsRDO ? "rdo" : "ndo"));

                var st = InstanceGrid.BoundContext.Settings as PERS.GridDataSettingsSelVal;
                st.Layout.ZebraRows = true;
                st.Layout.ShowSelectedRows = true;

                st.Automation.ShrinkColumnWidthToFit = false;

                if (st.Pager == null)
                    st.Pager = new PERS.JQGridPagerSettings();

                st.Pager.Mode = PERS.GridPagerModes.AlwaysVisible;
                st.Pager.DisplayTotalRecords = true;
                st.Pager.Position = PERS.HorizontalAlignment.Left;
                st.Pager.RecordTextFormat = null;

                st.NavigatorActions = new PERS.JQNavigatorAction[] { new PERS.JQNavigatorAction { Action = PERS.JQGridNavActionType.Refresh, Visible = true } };
                if (pc.IsRDO)
                {
                    st.Grouping = new PERS.GroupingView()
                    {
                        GroupFields = new PERS.GroupField[] { new PERS.GroupField { DataField = "Name" } }
                    };
                }
                else
                {
                    st.Grouping = null;
                }
            }
            else if (_mode == InstanceListMode.List)
            {
                if (pc.IsRDO)
                {
                    InstanceGrid.BoundContext.Fields["Name"].Visible = false;
                    InstanceGrid.BoundContext.Fields["Displayed"].Visible = true;
                }
                else
                {
                    InstanceGrid.BoundContext.Fields["Name"].Visible = true;
                }

                InstanceGrid.BoundContext.Width = 230;
                InstanceGrid.LabelPosition = PERS.LabelPositionType.Hidden;
                InstanceGrid.BoundContext.Attributes["listViewMode"] = "list " + (pc.IsRDO ? "rdo" : "ndo");
                InstanceGrid.BoundContext.Attributes["keepwrapper"] = "true";

                var st = InstanceGrid.BoundContext.Settings as PERS.GridDataSettingsSelVal;
                st.Layout.ZebraRows = false;
                st.Layout.ShowSelectedRows = false;

                st.Automation.ShrinkColumnWidthToFit = true;

                if (st.Pager == null)
                    st.Pager = new PERS.JQGridPagerSettings();

                st.Pager.Mode = PERS.GridPagerModes.AlwaysVisible;
                st.Pager.DisplayTotalRecords = false;
                st.Pager.Position = PERS.HorizontalAlignment.Middle;
                st.Pager.RecordTextFormat = "";

                st.NavigatorActions = new PERS.JQNavigatorAction[] { new PERS.JQNavigatorAction { Action = PERS.JQGridNavActionType.Refresh, Visible = false } };

                if (pc.IsRDO)
                {
                    st.Grouping = new PERS.GroupingView()
                    {
                        GroupFields = new PERS.GroupField[] { new PERS.GroupField { DataField = "Name" } }
                    };
                }

                CheckFilter(pc, haveRecordset);
            }
        }

        protected void CheckFilter(MaintenanceBehaviorContext pc, bool haveRecordset)
        {
            // Keep state of filter of Instance Grid on postback
            CWC.TextBox name = Page.FindCamstarControl("NameTxt") as CWC.TextBox;
            string newName = name != null ? name.Data as string : string.Empty;
            var filter = InstanceNameTxt.Data != null ? InstanceNameTxt.Data.ToString() : string.Empty;
            string curName = pc.DataContract.GetValueByName("InstanceName") as string;
            if (!string.IsNullOrEmpty(newName))
                curName = newName;
            //  IR 10774891
            if (!string.IsNullOrEmpty(filter) && !string.IsNullOrEmpty(curName))
            {
                bool match = IsSqlLikeMatch(curName, filter) || IsSqlLikeMatch(filter, curName);
                if (!match || !haveRecordset)// !string.IsNullOrEmpty(curName) && !curName.ToLower().StartsWith(filter))
                {
                    filter = string.Empty;
                    InstanceNameTxt.Data = filter;
                }
            }
            //Workaround for IR 9150304 
            if (!string.IsNullOrEmpty(filter))
            {
                filter = Microsoft.JScript.GlobalObject.escape(filter);
            }
            //End Workaround for IR 9150304
            InstanceGrid.GridContext.Filter = filter;
            InstanceGrid.GridContext.FilterAllowed = true;
        }

        private bool IsSqlLikeMatch(string input, string pattern)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(pattern))
                return false;
            else
            {
                string lowPattern = pattern.ToLower();
                string lowInput = input.ToLower();
                bool matches = new Regex(@"\A" + new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\").Replace(lowPattern, ch => @"\" + ch).Replace('_', '.').Replace("%", ".*") + @"\z", RegexOptions.Singleline).IsMatch(lowInput);
                return lowInput.StartsWith(lowPattern) || matches;
            }
        }
        private bool _clearStatusMessage = true;
    }

    public enum InstanceListMode { Undefined, Grid, List, Edit };
}
