// Copyright Siemens 2023  
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.PortalFramework;
using CamstarPortal.WebControls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Camstar.WebPortal.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using PERS = Camstar.WebPortal.Personalization;
using DT = Camstar.WebPortal.WebPortlets.DataTransfer;
using Siemens.OPCR.Diagnostics;

namespace Camstar.WebPortal.WebPortlets.DataTransfer
{
    public class InstanceListWP : MatrixWebPart
    {

        #region Controls

        protected virtual CWC.TextBox InstanceNameTxt { get { return Page.FindCamstarControl("InstanceNameTxt") as CWC.TextBox; } }
        protected virtual JQDataGrid ExportInstanceGrid { get { return Page.FindCamstarControl("DT_ExportInstanceGrid") as JQDataGrid; } }
        protected virtual CWC.Button SelectBtn { get { return Page.FindCamstarControl("SelectBtn") as CWC.Button; } }
        protected virtual JQDataGrid ItemsSelectedGrid { get { return Page.FindCamstarControl("ItemsSelectedGrid") as JQDataGrid; } }
        protected virtual CWC.DropDownList ObjectList { get { return Page.FindCamstarControl("ObjectList") as CWC.DropDownList; } }
        protected virtual CWC.Button IncludeRefsBtn { get { return Page.FindCamstarControl("IncludeRefsBtn") as CWC.Button; } }
        protected virtual CWC.RadioButton SelectAll_Btr { get { return Page.FindCamstarControl("SelectAll_Btr") as CWC.RadioButton; } }
        protected virtual CWC.CheckBox SelectAllItemsChk { get { return Page.FindCamstarControl("SelectAllItemsChk") as CWC.CheckBox; } }
        protected virtual CWC.TextBox ExcludedRefsTxt { get { return Page.FindCamstarControl("ExcludedRefsTxt") as CWC.TextBox; } }
        protected virtual CWC.FileInput ImportFile { get { return Page.FindCamstarControl("ImportFileInp") as CWC.FileInput; } }

        protected virtual JQDataGrid ObjectTypeGrid { get; set; }

        #region WebParts
        protected virtual WebPartBase FilterWP { get { return Page.FindIForm("MDL_Filter_WP") as WebPartBase; } }
        protected virtual WebPartBase InfoPanelWP { get { return Page.FindIForm("DT_InfoPanel_WP") as WebPartBase; } }
        protected virtual WebPartBase SelectAllInstructionWP { get { return Page.FindIForm("SelectAllInstructionWP") as WebPartBase; } }
        #endregion

        #endregion

        #region Protected Functions

        private LogClient _log = null;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.Navigate += Page_Navigate;
        }

        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            if (Transfer.Type == TransferType.Import)
                PreLoadImport();
            if (Transfer.Type == TransferType.Export)
                PreLoadExport();

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ObjectTypeGrid = Page.FindCamstarControl("ObjectTypeGrid") as JQDataGrid;

            // Get label for circular reference messages
            if (string.IsNullOrEmpty(_circRefError))
            {
                _circRefError = FrameworkManagerUtil.GetLabelValue("CircularRefErrorLbl") ?? string.Empty;
            }

            if (Transfer.Type != TransferType.Import)
            {
                if (SelectBtn != null)
                {
                    // Select instance page
                    SelectBtn.Click += new EventHandler(SelectBtn_Click);
                }

                if (IncludeRefsBtn != null)
                {
                    IncludeRefsBtn.Click += IncludeRefsBtn_Click;
                }

                if (ObjectList != null)
                {
                    ObjectList.DataChanged += ObjectList_DataChanged;
                }

                if (ExportInstanceGrid != null)
                    SetupInstanceGrid();

                if (ExcludedRefsTxt != null)
                {
                    if (_excludedObjects == null)
                        _excludedObjects = GetExcludedObjects();

                    var s = "";
                    Array.ForEach(_excludedObjects, o => s += (o + "\n"));
                    ExcludedRefsTxt.Data = s;
                }

                // Get session data when popup page is closed
                if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument")
                {
                    if (Page.SessionVariables.GetValueByName("InstanceFilters") != null)
                        InstanceFilters = Page.SessionVariables.GetValueByName("InstanceFilters") as ExportInstanceFilters;
                    ReloadInstanceGrid();
                }
            }

            if (SelectedInstances != null)
            {
                ObjectTypeGrid.Data = SelectedInstances;
            }
            ObjectTypeGrid.GridContext.DataChanged += GridContext_DataChanged;
        }

        ResponseData GridContext_DataChanged(object sender, JQGridEventArgs args)
        {
            var childCont = sender as GridContext;

            if (childCont != null && childCont.ParentGridContext != null)
            {
                var cont = childCont.ParentGridContext as ItemDataContext;
                var selectedInstances = cont.Data as ObjectTypeItem[];
                if (selectedInstances != null)
                {
                    var oldCount = selectedInstances.Length;
                    // Cleanup empty Object Type
                    var items =
                        from d in selectedInstances
                        where d.Instances != null && d.Instances.Length > 0
                        select d;

                    var objectTypeItems = items as ObjectTypeItem[] ?? items.ToArray();
                    if (objectTypeItems.Count() < oldCount)
                    {
                        cont.Data = objectTypeItems;
                        SelectedInstances = objectTypeItems;
                        if (cont.ExpandedRowIDs != null)
                        {
                            cont.ExpandedRowIDs.Clear();
                            cont.ExpandedRowID = null;
                        }
                    }
                }
            }

            return null;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            var isImport = (Transfer.Type == TransferType.Import);
            if (SelectAll_Btr != null && FilterWP != null)
            {
                FilterWP.Hidden = isImport;
                ObjectList.Hidden = isImport;
                SelectAllInstructionWP.Hidden = isImport;
                ImportFile.Visible = isImport;
                ExportInstanceGrid.Visible = !isImport;
                var boundContext = ItemsSelectedGrid.BoundContext;
                boundContext.Fields["LastEditedBy"].Visible = !isImport;
                boundContext.Fields["LastEditedTime"].Visible = !isImport;
                boundContext.Fields["Description"].Width = isImport ? 519 : 223;
            }

            if (!isImport)
            {
                var pc = Page.PortalContext as MaintenanceBehaviorContext;
                if (pc.ReloadInstanceList && ExportInstanceGrid != null)
                {
                    ExportInstanceGrid.ClearData();
                    ExportInstanceGrid.Action_Reload("");
                    pc.ReloadInstanceList = false;
                    RenderToClient = true;
                }
            }
        }
        #endregion

        #region Public Functions

        public virtual void SelectAllInstances(bool doSelect)
        {
            ExportInstanceGrid.Action_SelectRow(null, doSelect ? "select" : "deselect");
        }

        public override void GetSelectionData(OM.Service serviceData)
        {
            base.GetSelectionData(serviceData);
            if (serviceData is OM.CDOInquiry)
            {
                var pc = (Page.PortalContext as MaintenanceBehaviorContext);
                if (pc != null)
                {
                    var data = serviceData as OM.CDOInquiry;
                    data.CDODefId = new OM.Enumeration<OM.MaintainableObjectEnum, string>(pc.CDOID);
                    data.IsRDO = pc.IsRDO;

                    if (InstanceFilters != null)
                    {
                        var filter = InstanceFilters;
                        data.EmployeeFilter = filter.EmployeeFilter;
                        data.StatusFilter = filter.StatusFilter;
                        data.RORFilter = filter.RORFilter;
                        data.PrefixExcludeFilter = filter.PrefixExcludeFilter;
                        data.PrefixIncludeFilter = filter.PrefixIncludeFilter;
                        data.SufixExcludeFilter = filter.SufixExcludeFilter;
                        data.SufixIncludeFilter = filter.SufixIncludeFilter;
                        data.BeginDateFilter = filter.BeginDateFilter;
                        data.EndDateFilter = filter.EndDateFilter;
                    }
                }
            }
        }

        #endregion

        #region Private Functions

        protected virtual void Page_Navigate(object sender, NavigationEventArgs e)
        {
            if (e.EventType == NavigationEventTypes.Next || e.EventType == NavigationEventTypes.Back)
            {
                SelectedInstances = ObjectTypeGrid.Data as ObjectTypeItem[];
            }
        }

        protected virtual void MDL_InstanceList_SnapCompleted(DataTable dataWindowTable)
        {
            if (dataWindowTable.Columns.Contains("Revision"))
            {
                // Modify RDO grid
                foreach (var r in dataWindowTable.Rows.OfType<DataRow>())
                {
                    r.BeginEdit();
                    var isROR = string.Compare(r["InstanceId"] as string, r["RevOfRcd"] as string, true) == 0;
                    r["Displayed"] = HttpContext.Current.Server.HtmlEncode(r["Name"] as string) + CamstarPortalSection.GetRevisionDelimiter() +
                                HttpContext.Current.Server.HtmlEncode(r["Revision"] as string) + (isROR ? "<span class=\"ui-rdo-ror\" />" : "<span class=\"ui-rdo-non-ror\" />");
                }
                dataWindowTable.AcceptChanges();
            }

            // for all rows - exclude selected items
            if (ObjectTypeGrid != null && ObjectTypeGrid.Data != null && Page.PortalContext is MaintenanceBehaviorContext && ObjectList.Data != null)
            {
                string cdoName, cdoDisplayName;
                int cdoId;
                GetCurrentCDOInfo(out cdoId, out cdoName, out cdoDisplayName);
                var objSelected = (ObjectTypeGrid.Data as ObjectTypeItem[]).FirstOrDefault(x => x.Name == cdoName);
                if (objSelected != null && objSelected.Instances != null)
                {
                    // Modify grid rows 
                    foreach (var r in dataWindowTable.Rows.OfType<DataRow>())
                    {
                        r.BeginEdit();
                        r["ItemAlreadySelected"] =
                            objSelected.Instances.Any(s => s.CDOTypeName == cdoName && s.InstanceID == r.Field<string>("InstanceId"));
                    }
                    dataWindowTable.AcceptChanges();
                }
            }
        }

        protected virtual void ObjectList_DataChanged(object sender, EventArgs e)
        {
            InstanceNameTxt.ClearData();
            ReloadInstanceGrid();
        }

        protected virtual void ReloadInstanceGrid()
        {
            SetupInstanceGrid();

            var pc = (Page.PortalContext as MaintenanceBehaviorContext);
            pc.ReloadInstanceList = true;
            pc.CDOID = ObjectList.Data as string;

            if (Transfer.Type == TransferType.Import)
            {
                pc.CDODisplayName = ObjectList.CustomListValues[0].DisplayName;
            }

            // reset select all instaces checkbox
            if (SelectAllItemsChk != null)
                SelectAllItemsChk.Data = false;
        }

        protected virtual void IncludeRefsBtn_Click(object sender, EventArgs e)
        {
            if (ObjectTypeGrid != null && ObjectTypeGrid.Data != null)
            {
                var allSelected =
                    from o in ObjectTypeGrid.Data as ObjectTypeItem[]
                    where o.Instances != null
                    from it in o.Instances
                    select it;

                var index = 1;
                var itemGroups = new List<ObjectTypeItem>(SelectedInstances);
                try
                {
                    var refs = GetReferences(allSelected.ToArray(), false);
                    var transfer = new DT.DataTransfer
                    (
                        new DT.DataTransferInfo(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile.Name),
                        new DT.DataTransferRepository(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile),
                        new Dictionary<string, string>()
                    );
                    refs = transfer.OrderSelectedInstances(refs.ToArray(), true);
                    if (_circularRefInstances != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        if (_circularRefInstances.Count > 0)
                        {
                            var grpedInstances = _circularRefInstances
                                                    .GroupBy(x => x.Value)
                                                    .ToDictionary(y => y.Key, y => y.Select(x => x.Key).ToArray());

                            foreach (var pair in grpedInstances)
                            {
                                var i = 0;
                                foreach (var instance in pair.Value)
                                {
                                    if (i > 0)
                                    {
                                        sb.Append(",");
                                    }
                                    else
                                    {
                                        sb.Append(pair.Key + ":");
                                    }
                                    sb.Append(instance);
                                    i++;
                                }
                                sb.Append(";");
                            }
                            _circRefError += sb.ToString();
                            Page.DisplayWarning(_circRefError);
                        }
                    }

                    if (refs != null)
                    {
                        var orderedRefs = refs.OrderByDescending(r => r.Order);
                        foreach (var rf in orderedRefs)
                        {
                            rf.Order = index++;

                            var itemGroup = itemGroups.FirstOrDefault(g => g.Name == rf.CDOTypeName);
                            if (itemGroup == null)
                            {
                                itemGroup = new ObjectTypeItem()
                                {
                                    Name = rf.CDOTypeName,
                                    DisplayName = rf.CDOTypeValue,
                                    CDOID = int.Parse(rf.CDOTypeID),
                                    Instances = new SelectedInstanceItem[] { rf }
                                };
                                itemGroups.Add(itemGroup);
                            }
                            else
                            {
                                var instances = itemGroup.Instances.ToList();
                                var instance = instances.FirstOrDefault(i => i.Name == rf.Name && i.Revision == rf.Revision);
                                if (instance != null)
                                    instance.Order = Math.Max(instance.Order, rf.Order);
                                else
                                    instances.Add(rf);

                                itemGroup.Instances = instances.ToArray();
                            }
                        }
                    }

                    SelectedInstances = itemGroups.OrderBy(c => c.DisplayName).ToArray();
                    ObjectTypeGrid.Data = SelectedInstances;
                    InfoPanelWP.RenderToClient = true;
                    CheckSelectionLimit();
                }
                catch (Exception ex)
                {
                    DisplayMessage(new OM.ResultStatus(ex.Message, false));
                }
            }
        }

        protected virtual void GetCurrentCDOInfo(out int CDOId, out string CDOName, out string CDODisplayName)
        {
            string cdoIdStr;
            var cont = Page.PortalContext as MaintenanceBehaviorContext;
            if (cont != null)
            {
                cdoIdStr = cont.CDOID;
            }
            else
            {
                cdoIdStr = ObjectList.Data as string;
            }

            CDOId = int.Parse(cdoIdStr);

            if (Transfer.Type == TransferType.Export)
            {
                var exportCdo = Page.PortalContext.LocalSession["ExportObjectsList"] as Dictionary<string, CDOData>;
                if (exportCdo != null)
                {
                    var d = exportCdo.FirstOrDefault(s => s.Value.CDODefID == cdoIdStr);
                    CDOName = d.Value.CDOName;
                    CDODisplayName = d.Value.CDODisplayName;
                    return;
                }
            }
            CDOName = null;
            CDODisplayName = null;
        }

        protected virtual void SelectBtn_Click(object sender, EventArgs e)
        {
            if (ObjectTypeGrid != null)
            {
                var instanceGrid = ExportInstanceGrid;
                int cdoDefId;
                string cdoName;
                string cdoDisplayName;


                var data = SelectedInstances ?? new ObjectTypeItem[] { };
                var selectedItems = instanceGrid.GridContext.GetSelectedItems(false);

                if (selectedItems != null)
                {
                    GetCurrentCDOInfo(out cdoDefId, out cdoName, out cdoDisplayName);
                    var objTypeItem = data.FirstOrDefault(s => s.Name == cdoName);
                    if (objTypeItem == null)
                    {
                        objTypeItem = new ObjectTypeItem()
                        {
                            Name = cdoName,
                            CDOID = cdoDefId,
                            DisplayName = cdoDisplayName,
                            Instances = new SelectedInstanceItem[] { }
                        };
                        data = data.Concat(new[] { objTypeItem }).OrderBy(o => o.DisplayName).ToArray();
                        objTypeItem = data.First(s => s.Name == cdoName);
                    }

                    var SelectedRowIdList = Page.CurrentCallStack.Context.LocalSession.Keys.OfType<string>().Where(n => n.Contains("((ROW))")).ToArray();

                    Dictionary<string, ItemDataContext> SelectedItemContextCopy = new Dictionary<string, ItemDataContext>();
                    if (SelectedRowIdList != null)
                    {
                        foreach (var s in SelectedRowIdList)
                        {
                            SelectedItemContextCopy.Add(s, (Page.CurrentCallStack.Context.LocalSession[s] as ItemDataContext));

                            Page.CurrentCallStack.Context.LocalSession.Remove(s);
                        }
                    }

                    //sort subgrid contexts after sorting data by DisplayName
                    for (int j = 0; j < data.Length; j++)
                    {
                        var item = data[j];

                        for (int i = 0; i < SelectedRowIdList.Count(); i++)
                        {
                            var subgridData = SelectedItemContextCopy[SelectedRowIdList[i]].Data as SelectedInstanceItem[];
                            if (subgridData != null && (subgridData.Length != 0 && item.CDOID == int.Parse(subgridData[0].CDOTypeID)))
                            {
                                var oldRowId = SelectedItemContextCopy[SelectedRowIdList[i]].ParentRowID;
                                var newRowId = SelectedRowIdList[i].Replace(oldRowId, j.ToString("000000"));
                                Page.CurrentCallStack.Context.LocalSession[newRowId] = SelectedItemContextCopy[SelectedRowIdList[i]];
                            }
                        }
                    }

                    if (objTypeItem != null && objTypeItem.Instances == null)
                        objTypeItem.Instances = new SelectedInstanceItem[0];

                    var instances =
                        (from si in selectedItems
                         select new SelectedInstanceItem(si as DataRow) { CDOTypeID = cdoDefId.ToString(), CDOTypeName = cdoName }).ToArray();

                    // Clear selecting in the instance list
                    instanceGrid.GridContext.GetSelectedItems(true);

                    var comparer = new InstanceComparer<SelectedInstanceItem>((i1, i2) => i1.InstanceID == i2.InstanceID, i => i.InstanceID.GetHashCode());
                    objTypeItem.Instances = instances.Union(objTypeItem.Instances, comparer).OrderBy(it => it.Name).ToArray();

                    ObjectTypeGrid.Data = data;

                    FilterWP.RenderToClient = true;
                    SelectedInstances = data;

                    if (InfoPanelWP != null)
                        InfoPanelWP.RenderToClient = true;
                }
            }
            CheckSelectionLimit();
        }

        protected virtual void CheckSelectionLimit()
        {
            if (SelectedInstances != null)
            {
                var selectedInstanceCount = SelectedInstances.Sum(i => i.Instances.Count());
                if (LimitNotification.NotificationRequired(selectedInstanceCount))
                {
                    var notification = String.Format("alert('{0}','Warning!', null);", LimitNotification.Notification);
                    System.Web.UI.ScriptManager.RegisterStartupScript(this, Page.GetType(), "Warning", notification, true);

                    Page.DisplayWarning(LimitNotification.Notification); //TODO: Page warning needs to be fixed
                    Page.RenderToClient = true;
                }
            }
        }

        protected virtual void SetupInstanceGrid()
        {
            var pc = (Page.PortalContext as MaintenanceBehaviorContext);

            var exportObjectList = Page.PortalContext.LocalSession["ExportObjectsList"] as Dictionary<string, CDOData>;
            var selectObj = exportObjectList.Values.FirstOrDefault(o => o.CDODefID == (ObjectList.Data as string));
            if (selectObj.IsRDO != null)
                pc.IsRDO = (bool)selectObj.IsRDO;

            var boundContext = ExportInstanceGrid.BoundContext;

            if (Transfer.Type == TransferType.Import)
            {
                boundContext.Attributes["listViewMode"] = ("list " + (pc.IsRDO ? "rdo" : "ndo") + " import");
            }
            if (Transfer.Type == TransferType.Export)
            {
                (boundContext as SelValGridContext).RequestFieldExpression = "CDOInquiry.CDOFilteredInstances";
                boundContext.Attributes["listViewMode"] = ("list " + (pc.IsRDO ? "rdo" : "ndo") + " export");
            }
            boundContext.SnapCompleted += new SnapCompletedHandler(MDL_InstanceList_SnapCompleted);
            boundContext.Fields["Name"].Visible = !pc.IsRDO;
            boundContext.Fields["Displayed"].Visible = pc.IsRDO;
            boundContext.Width = 230;
            boundContext.Attributes.Add("keepwrapper", "true");

            var st = boundContext.Settings;
            st.Layout.ZebraRows = false;
            st.Layout.ShowSelectedRows = false;

            if (st.Automation != null)
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
                    GroupFields = new PERS.GroupField[] { new PERS.GroupField { DataField = "Name" } },
                };
                st.Layout.ShowAllExpand = false;
            }
            else
            {
                st.Grouping = null;
                st.Layout.ShowAllExpand = false;
            }
        }

        protected virtual void PreLoadImport()
        {
            if (Page.PortalContext != null)
            {
                var importFileName = this.ImportFile.Data.ToString();
                var filePath = CWC.FileInput.UploadFilePath;
                if (importFileName.Length > 0 && filePath.Length > 0)
                {
                    try
                    {
                        var importFile = new ImportFile(filePath);
                        var importData = importFile.GetImportData();
                        if (importData != null)
                        {
                            var ol = ModelingObjectList.GetImportObjects(importData);
                            var otList =
                                from inst in importData.ImportContents
                                group inst by inst.ObjectTypeName.Value
                                    into g
                                orderby g.Key
                                select
                            new ObjectTypeItem()
                            {
                                Name = g.Key,
                                DisplayName = ol.FirstOrDefault(z => z.Value.CDOName == g.Key).Value.CDODisplayName ?? g.Key,
                                Instances =
                                    (from n in importData.ImportContents
                                     where n.ObjectTypeName.Value == g.Key
                                     select new SelectedInstanceItem
                                     {
                                         CDOTypeID = n.ObjectType.ToString(),
                                         CDOTypeName = (string)n.ObjectTypeName,
                                         Name = (string)n.ObjectName,
                                         InstanceID = (string)n.ObjectInstanceId,
                                         Revision = (string)n.Revision,
                                         IsROR = (bool)n.IsROR,
                                         Order = n.ListItemIndex ?? 0,
                                         DisplayedName = string.IsNullOrEmpty((string)n.Revision) ? (string)n.ObjectName : $"{n.ObjectName}{CamstarPortalSection.GetRevisionDelimiter()} {n.Revision}"
                                     }
                                    ).OrderBy(f => f.Order).ToArray()
                            };
                            SelectedInstances = otList.OrderBy(s => s.DisplayName).ToArray();
                        }

                        Page.PortalContext.LocalSession["ImportSetName"] = importFile.SetName;
                    }
                    catch (Exception e)
                    {
                        DisplayMessage(new OM.ResultStatus(e.Message, false));
                    }

                    RefreshInfoPanelWP();
                    RefreshSelectedInstanceTotals();
                }
            }
        }

        protected virtual void RefreshInfoPanelWP()
        {
            if (InfoPanelWP != null)
                InfoPanelWP.RenderToClient = true;
        }

        protected virtual void RefreshSelectedInstanceTotals()
        {
            var countTotalObject = 0;
            var countTotalInstances = 0;
            if (SelectedInstances != null)
            {
                countTotalInstances = SelectedInstances.Where(item => item.Instances != null).Sum(item => item.Instances.Length);
                countTotalObject = SelectedInstances.Count();
            }
            Page.PageflowControls.Add("TotalObjects", countTotalObject);
            Page.PageflowControls.Add("TotalInstances", countTotalInstances);
        }

        protected virtual void PreLoadExport()
        {
            if (ObjectList != null)
            {
                Dictionary<string, CDOData> exportObject;
                if (Page.PortalContext != null && Page.PortalContext.LocalSession["ExportObjectsList"] != null)
                {
                    exportObject = Page.PortalContext.LocalSession["ExportObjectsList"] as Dictionary<string, CDOData>;
                }
                else
                {
                    exportObject = ModelingObjectList.GetExportObjects(false);
                    Page.PortalContext.LocalSession["ExportObjectsList"] = exportObject;
                }
                _excludedObjects = GetExcludedObjects();
                ObjectList.CustomListValues =
                    (from o in exportObject
                     where !_excludedObjects.Contains(o.Value.CDODisplayName)
                     orderby o.Value.CDODisplayName
                     select
                         new PERS.CustomListValueMapItem()
                         {
                             DisplayName = o.Value.CDODisplayName,
                             Value = o.Value.CDODefID
                         }).ToArray();
            }
        }

        protected virtual string[] GetExcludedObjects()
        {
            Dictionary<string, CDOData> exportObject;
            var excludedObjectIDs = new string[0];
            var excludedObjects = new string[0];
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = Page.Service.GetService<FactoryMaintService>();

            var serviceData = new OM.FactoryMaint();

            if (session.SessionValues != null && !String.IsNullOrEmpty(session.SessionValues.Factory))
            {
                serviceData.ObjectToChange = new OM.NamedObjectRef(session.SessionValues.Factory);
                var request = new FactoryMaint_Request();
                var result = new FactoryMaint_Result();
                var resultStatus = new OM.ResultStatus();

                request.Info = new OM.FactoryMaint_Info
                {
                    RequestValue = true,
                    ObjectChanges = new OM.FactoryChanges_Info
                    {
                        Name = new OM.Info(true),
                        ModelingObjsToExclude = new OM.ModelingObjsToExcludeChanges_Info
                        {
                            ModelingCDOTypeId = new OM.Info(true),
                            DisplayName = new OM.Info(true)
                        }
                    }
                };

                resultStatus = service.Load(serviceData, request, out result);
                if (resultStatus.IsSuccess && result.Value.ObjectChanges.ModelingObjsToExclude != null)
                {
                    if (Page.PortalContext != null && Page.PortalContext.LocalSession["ExportObjectsList"] != null)
                    {
                        excludedObjectIDs = result.Value.ObjectChanges.ModelingObjsToExclude
                            .Where(objExclude => objExclude.ModelingCDOTypeId != null)
                            .Select(objExclude => objExclude.ModelingCDOTypeId.Value.ToString())
                            .ToArray();
                        exportObject = Page.PortalContext.LocalSession["ExportObjectsList"] as Dictionary<string, CDOData>;
                        excludedObjects = exportObject
                            .Where(objExclude => excludedObjectIDs.Contains(objExclude.Value.CDODefID))
                            .Select(objExclude => objExclude.Value.CDODisplayName.ToString())
                            .ToArray();
                    }
                    else
                    {
                        // ModelingObjsToExcludeChanges.DiplayName use not preferred ...
                        excludedObjects = result.Value.ObjectChanges.ModelingObjsToExclude
                            .Where(objExclude => objExclude.ModelingCDOTypeId != null)
                            .Select(objExclude => objExclude.DisplayName.ToString())
                            .ToArray();

                    }
                }
            }

            return excludedObjects;

        }

        protected virtual IEnumerable<SelectedInstanceItem> GetReferences(SelectedInstanceItem[] selectedItems, bool recursion)
        {
            if (selectedItems != null && selectedItems.Length > 0)
            {
                _refs = new List<SelectedInstanceItem>(selectedItems);

                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var cdoServ = new CDOInquiryService(session.CurrentUserProfile);
                var items = selectedItems;

                while (items.Length > 0)
                {
                    //loop thru items and get references
                    foreach (var item in items)
                    {
                        var reference = _refs.FirstOrDefault(x => x.Name == item.Name);
                        var order = reference != null ? reference.Order + 1 : 1;
                        var cdo = new OM.CDOInquiry()
                        {
                            SelectedInstances = new OM.BaseObjectRef[] { new OM.BaseObjectRef(item.InstanceID) },
                            Recursive = recursion
                        };
                        var req = new CDOInquiry_Request
                        {
                            Info = new OM.CDOInquiry_Info
                            {
                                ObjectReferencesList = new OM.ObjectReferencesInfo_Info { RequestValue = true }
                            }
                        };

                        CDOInquiry_Result res;
                        var status = cdoServ.GetReferences(cdo, req, out res);
                        if (status.IsSuccess && res.Value != null && res.Value.ObjectReferencesList != null)
                        {
                            Array.ForEach(res.Value.ObjectReferencesList, obj =>
                            {
                                if (obj.ObjectFields != null)
                                    Array.ForEach(obj.ObjectFields,
                                        field => AddFieldItemToReferences(field, order, item.InstanceID));
                            });
                        }
                        item.ShouldOpen = false;
                    }
                    //set the items list to items that still need to be explored
                    items = _refs.Where(r => r.ShouldOpen).ToArray();
                }
                return _refs;
            }

            return null;
        }

        protected virtual void AddFieldItemToReferences(OM.ObjectField r, int order, string parent)
        {
            if (r is OM.SubentityField)
            {
                var subentity = r as OM.SubentityField;
                if (subentity.Instances != null)
                {
                    Array.ForEach(subentity.Instances, item =>
                    {
                        AddSubentityInstance(item, order, parent);
                    });
                }

            }
            else if (r is OM.ReferenceField)
            {
                var reference = r as OM.ReferenceField;
                if (reference.References != null)
                    Array.ForEach(reference.References, rf =>
                    {
                        //  Fix for CPR 336275 - IR # 10688948 - export fails for Resource that has Resource Setup data
                        if (rf.ObjectName != null && rf.ObjectInstanceId != null)
                        {
                            if (rf.ObjectFields != null)
                                Array.ForEach(rf.ObjectFields, item => AddFieldItemToReferences(item, order, (string)item.FieldName));
                            AddToReferences(new SelectedInstanceItem(rf) { ShouldOpen = true, Order = order, ReferenceParent = parent });
                        }
                    });
            }
        }

        protected virtual void AddToReferences(SelectedInstanceItem it, int depthCount = 0)
        {
            if (_excludedObjects == null || !_excludedObjects.Contains(it.CDOTypeValue))
            {
                var existingItem = _refs.FirstOrDefault(x => x.InstanceID == it.InstanceID);
                if (existingItem != null)
                {
                    existingItem.ReferenceParent = it.ReferenceParent;
                    RecursivelyIncrementOrder(existingItem, it.Order++, depthCount);
                }
                else
                    _refs.Insert(0, it);
            }
        }

        private void LogMessage(LogSeverity level, string method, string message)
        {
            if (_log == null)
            {
                _log = new LogClient("Camstar");
                _log.ClassName = "InstanceListWP";
            }
            _log.LogMessage(level, method, message);
        }
        protected virtual void RecursivelyIncrementOrder(SelectedInstanceItem item, int order, int depthCount)
        {
            depthCount++;
            var circRefInst = _refs.FirstOrDefault(x => x.InstanceID == item.ReferenceParent);
            if (depthCount > _maxDepthCount)
            {
                if (_circularRefInstances == null)
                {
                    _circularRefInstances = new Dictionary<string, string>();
                }
                if (circRefInst != null)
                {
                    if (!_circularRefInstances.ContainsKey(circRefInst.Name))
                    {
                        if (!_circularRefInstances.ContainsKey(item.Name))
                        {
                            LogMessage(LogSeverity.Warning, "RecursivelyIncrementOrder", $"Adding item: {item.Name}, CDOType: {item.CDOTypeName} for CircRefInst: {circRefInst.Name}");
                            _circularRefInstances.Add(item.Name, item.CDOTypeName);
                        }
                        else
                            LogMessage(LogSeverity.Warning, "RecursivelyIncrementOrder", $"Item: {item.Name}, CDOType: {item.CDOTypeName} already in Circular Reference list for CircRefInst: {circRefInst.Name}");
                    }
                }
            }
            item.Order = order;
            var childItems = _refs.Where(x => x.ReferenceParent == item.InstanceID);
            foreach (var child in childItems)
            {
                if (_circularRefInstances != null)
                {
                    if (circRefInst != null)
                    {
                        if (_circularRefInstances.ContainsKey(circRefInst.Name))
                        {
                            LogMessage(LogSeverity.Warning, "RecursivelyIncrementOrder", $"Item: {child.Name}, CDOType: {item.CDOTypeName} found for CircularRef CDOType: {_circularRefInstances[circRefInst.Name]} for {circRefInst.Name}");
                            break;
                        }
                    }
                }
                RecursivelyIncrementOrder(child, order + 1, depthCount);
            }
        }

        protected virtual void AddSubentityInstance(OM.SubentityInstance sr, int order, string parent)
        {
            if (sr.ObjectFields != null)
            {
                Array.ForEach(sr.ObjectFields, item =>
                {
                    AddFieldItemToReferences(item, order, parent);
                });
            }
        }

        #endregion

        #region Constants

        private const int _maxDepthCount = 5;

        #endregion

        #region Protected Member Variables

        protected virtual DataTransfer Transfer
        {
            get { return (DataTransfer)Page.PortalContext.LocalSession["Transfer"]; }
        }

        protected virtual ObjectTypeItem[] SelectedInstances
        {
            get
            {
                return Page.PortalContext.DataContract.GetValueByName<ObjectTypeItem[]>("DT_SelectedInstances");
            }
            set
            {
                Page.PortalContext.DataContract.SetValueByName("DT_SelectedInstances", value);
            }
        }

        protected virtual ExportInstanceFilters InstanceFilters
        {
            get { return Page.PortalContext.DataContract.GetValueByName<ExportInstanceFilters>("InstanceFilters"); }
            set { Page.PortalContext.DataContract.SetValueByName("InstanceFilters", value); }
        }

        protected virtual SelectionLimitNotification LimitNotification
        {
            get
            {
                var limitNotification = Page.PortalContext.DataContract.GetValueByName<SelectionLimitNotification>("LimitNotification");
                if (limitNotification == null)
                {
                    limitNotification = new SelectionLimitNotification();
                    Page.PortalContext.DataContract.SetValueByName("LimitNotification", limitNotification);
                }
                return limitNotification;
            }
            set { Page.PortalContext.DataContract.SetValueByName("LimitNotification", value); }
        }

        private string _circRefError;
        private List<SelectedInstanceItem> _refs;
        private Dictionary<string, string> _circularRefInstances;
        public string[] _excludedObjects;
        #endregion
    }

    public class SelectionLimitNotification
    {
        private bool notified = false;
        private int threshold;

        public string Notification;

        public SelectionLimitNotification()
        {
            //TODO: set dynamically with PS setting
            threshold = 1000; //CamstarPortalSection.Settings.DefaultSettings.SelectionLimitThreshold Ex:SuccessPopupFadeOutTime
            Notification = string.Format(@"The export has been assigned over {0} instances.\n Please be aware that system performance may be impacted.", threshold.ToString());
        }

        public virtual bool NotificationRequired(int selectionCount)
        {
            if (!notified && selectionCount > threshold)
            {
                notified = true;
            }
            return notified;
        }
    }

    public class InstanceComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> getEquals;
        private readonly Func<T, int> getHashCode;

        public InstanceComparer(Func<T, T, bool> equals, Func<T, int> hashCode)
        {
            getEquals = equals;
            getHashCode = hashCode;
        }

        public virtual bool Equals(T x, T y)
        {
            return getEquals(x, y);
        }

        public virtual int GetHashCode(T obj)
        {
            return getHashCode(obj);
        }
    }


}
