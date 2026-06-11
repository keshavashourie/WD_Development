// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Text.RegularExpressions;
using System.Globalization;

using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CWGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using PERS = Camstar.WebPortal.Personalization;


namespace Camstar.WebPortal.WebPortlets.MfgAuditTrailR2
{
    public class TransactionsPanelR2 : MatrixWebPart
    {
        #region Controls

        protected virtual CWC.DropDownList DetailsRequested
        {
            get { return FindCamstarControl("MfgAuditTrailInquiry_DetailsRequestedType") as CWC.DropDownList; }
        } // DetailsRequested

        protected virtual CWGC.JQDataGrid HistoryFilterGrid
        {
            get { return FindCamstarControl("MfgAuditTrailInquiry_HistoryFilterGrid") as CWGC.JQDataGrid; }
        }

        protected virtual CWGC.JQDataGrid MainLineGrid
        {
            get { return FindCamstarControl("MfgAuditTrailInquiry_MainLineGrid") as CWGC.JQDataGrid; }
        } // MainLineGrid

        protected virtual CWGC.JQDataGrid SummaryGrid
        {
            get
            {
                CWGC.JQDataGrid grid = null;
                if (DetailTabs != null && DetailTabs.SelectedItem != null && !string.IsNullOrEmpty(DetailTabs.SelectedItem.Name))
                    grid = Page.FindCamstarControl(string.Format(mkSummaryGridIdPattern, DetailTabs.SelectedItem.Name)) as CWGC.JQDataGrid;

                return grid;
            }
        } //SummaryGrid

        protected virtual CWGC.JQDataGrid DetailsGrid
        {
            get
            {
                CWGC.JQDataGrid grid = null;
                if (DetailTabs != null && DetailTabs.SelectedItem != null && !string.IsNullOrEmpty(DetailTabs.SelectedItem.Name))
                    grid = Page.FindCamstarControl(string.Format(mkDetailsGridIdPattern, DetailTabs.SelectedItem.Name)) as CWGC.JQDataGrid;

                return grid;
            }
        } // DetailsGrid

        protected virtual CWGC.JQDataGrid SubDetailsGrid
        {
            get
            {
                CWGC.JQDataGrid grid = null;
                if (DetailTabs != null && DetailTabs.SelectedItem != null && !string.IsNullOrEmpty(DetailTabs.SelectedItem.Name))
                    grid = Page.FindCamstarControl(string.Format(mkSubDetailsGridIdPattern, DetailTabs.SelectedItem.Name)) as CWGC.JQDataGrid;

                return grid;
            }
        } // SubDetailsGrid

        protected virtual JQTabContainer DetailTabs
        {
            get { return FindCamstarControl("MfgAuditTrailInquiry_DetailTabs") as JQTabContainer; }
        }

        protected virtual CWC.Button DocAttachment
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_DocAttachmentButton") as CWC.Button; }
        } // DocAttachment
        protected virtual CWC.Button ViewAttachmentButton
        {
            get { return Page.FindCamstarControl("ViewAttachmentButton") as CWC.Button; }
        }
        protected virtual CWC.Button SearchTxnBtn
        {
            get { return Page.FindCamstarControl("SearchTxnBtn") as CWC.Button; }
        }

        protected virtual CWC.Breadcrumb Breadcrumb
        {
            get { return Page.FindCamstarControl("BreadCrumb") as CWC.Breadcrumb; }
        }
        #endregion

        #region Protected properties

        protected virtual OM.Enumeration<OM.AvailableHistDetailsMaskEnum, int> AvailableHistoryDetails
        {
            set
            {
                ViewState.Add(mkAvailableHistoryDetails, value);
            }
            get
            {
                return ViewState[mkAvailableHistoryDetails] as OM.Enumeration<OM.AvailableHistDetailsMaskEnum, int>;
            }
        } // AvailableHistoryDetails

        protected OM.MfgAuditTrailInquiry ServiceData { get; set; }

        protected virtual string DetailsGridExpandedRowID
        {
            get
            {
                var val = Page.DataContract.GetValueByName("detailsGridExpandedRowID");
                return val == null ? "" : val.ToString();
            }
            set { Page.DataContract.SetValueByName("detailsGridExpandedRowID", value); }
        }

        private List<JQDataGrid> dataGrids;
        protected List<JQDataGrid> DataGrids
        {
            get { return dataGrids ?? (dataGrids = new List<JQDataGrid>()); }
        }

        private int CurrentDataGridIndex
        {
            get { return GetCurrentDataGridIndex(); }
        }

        protected int MaxBreadcrumbLevelForCurrentTab
        {
            get { return GetMaxBreadcrumbLevelForCurrentTab();  }
        }

        protected bool ShouldResetDetailTabs
        {
            get {
                bool? result = Page.PortalContext.LocalSession[ShouldResetDetailTabsKey] as bool?;
                if (result != null)
                { return result.Value; }
                else { return false; }
            }
            set {
                Page.PortalContext.LocalSession[ShouldResetDetailTabsKey] = value;
            }
        }
        private string CurrentDataGridIndexKey
        {
            get
            {
                return "MfgAudit.TransactionsPanelR2.CurrentDataGridIndex";
            }
        }// VisibleTabsIndexKey

        protected string VisibleTabsKey
        {
            get
            {
                return "MfgAuditInquiry_TransactionsPanelR2_VisibleTabsKey";
            }
        }

        protected string FilterGridRecordsetKey
        {
            get
            {
                return "AuditTrailInquiry_FilteredRecordsetOf_";
            }
        }
        protected string FilterGridFieldsKey
        {
            get
            {
                return "AuditTrailInquiry_GridFieldsOf_";
            }
        }
        protected string RegisteredAsScriptControlsKey
        {
            get
            {
                return "AuditTrailInquiry_GridsRegisteredAsScriptControls";
            }
        }
        protected string ShouldResetDetailTabsKey
        {
            get { return $"AuditTrailInquiry_ShouldResetDetailTabs_{Page.CallStackKey}"; }
        }
        #endregion

        #region Protected methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ServiceData = new OM.MfgAuditTrailInquiry();
            DetailTabs.SelectedIndexChanged += DetailTabs_SelectedIndexChanged;
            SearchTxnBtn.Click += SearchTxnBtn_Click;
            ViewAttachmentButton.Click += ViewAttachmentButton_Click;
            if(SummaryGrid != null )
            {
              (SummaryGrid.GridContext as BoundContext).SnapCompleted +=
               dataTbl =>
               {
                   var changed = false;
                   if (dataTbl.Columns.OfType<DataColumn>().FirstOrDefault(c => c.ColumnName == "Parent") is DataColumn parentCol)
                   {
                       foreach (var r in dataTbl.Rows.OfType<DataRow>())
                       {
                           var newStr = reformatDateTime(r[parentCol] as string);
                           if (newStr != null)
                           {
                               r[parentCol] = newStr;
                               changed = true;
                           }

                       }
                   }
                   if (changed)
                       dataTbl.AcceptChanges();
               };
            }
           


            if (ShouldResetDetailTabs)
            {
                ResetDetailTabs();
            }
            InitDataGrids();
            SetRowSelectedHandlersToDataGrids();

            if ((Page.Request.Form["__EVENTTARGET"] != null) && (Page.Request.Form["__EVENTTARGET"] == Breadcrumb.ClientID))
            {
                int eventArgument;
                if (Int32.TryParse(Page.Request.Form["__EVENTARGUMENT"], out eventArgument))
                {
                    ReturnToDataGridWithIndex(eventArgument);
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            InitFilterGrid();
            RefreshDataGridsVisibility();

            SetShouldClearBreadcrumbCommandsToClientSide();
            SetMaxBreadcrumbLevelToClientSide();
            SetTabNameToClientSide();

            ViewAttachmentButton.Hidden = true;

            string startupScript = "setTimeout(function(){ auditTrailBreadcrumbInit('" + Breadcrumb.ClientID + "', '" + this.GetType().AssemblyQualifiedName + "')}, 300);";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "auditTrailBreadcrumbInit", startupScript, true);

            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
        }

        protected virtual void ViewAttachmentButton_Click(object sender, EventArgs e)
        {
            var docName = Page.SessionVariables.GetValueByName("DocumentName").ToString();
            if (!string.IsNullOrEmpty(docName))
            {
                var docInfo = DownloadDocument(docName);
                if (docInfo != null && !string.IsNullOrEmpty(docInfo.FileName))
                {
                    var src = docInfo.IsRemote
                        ? string.Format("OpenDocumentUrl('{0}','{1}','');", JavascriptUtil.ConvertForJavascript(docInfo.URI), docInfo.AuthenticationType)
                        : string.Format("window.open('DownloadFile.aspx?viewdocfile={0}');", HttpUtility.UrlEncode(docInfo.FileName));
                    ScriptManager.RegisterStartupScript(this, Page.GetType(), "opendocument", src, true);
                    RenderToClient = true;
                }
            }
        }

        protected virtual DocumentRefInfo DownloadDocument(string name)
        {
            var documentRev = new OM.RevisionedObjectRef();
            var parts = name.Split(':');
            switch (parts.Length)
            {
                case 1:
                    documentRev.Name = name;
                    documentRev.RevisionOfRecord = true;
                    break;
                case 2:
                    documentRev.Name = parts[0];
                    documentRev.Revision = parts[1];
                    break;
            }

            OM.ResultStatus resultStatus;

            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var configFolder = CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;
            LabelCache labelCache = LabelCache.GetRuntimeCacheInstance();
            var label = labelCache.GetLabelByName("Lbl_SharedFolderDoesntExists");
            var message = string.Format(label != null ? label.Value : "Shared folder '{0}' does not exist.", configFolder);

            var docInfo = AttachmentExecutor.DownloadDocumentRef(documentRev, session.CurrentUserProfile, message,
                out resultStatus);
            if (!resultStatus.IsSuccess)
            {
                Page.DisplayMessage(resultStatus);
                docInfo = null;
            }
            return docInfo;
        }

        protected virtual ResponseData MainLineGrid_RowSelected(object sender, CWGC.JQGridEventArgs args)
        {
            if (MainLineGrid.Data != null)
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new Camstar.WCF.Services.MfgAuditTrailInquiryService(session.CurrentUserProfile);

                Page.GetSelectionData(ServiceData);

                var request = new Camstar.WCF.Services.MfgAuditTrailInquiry_Request();
                var result = new Camstar.WCF.Services.MfgAuditTrailInquiry_Result();
                var resultStatus = new OM.ResultStatus();

                request.Info = new OM.MfgAuditTrailInquiry_Info
                {
                    RequestValue = true,
                    AvailableHistoryDetails = new OM.Info(true),
                    Attachments = new OM.Info(true),
                    AttachmentsDetails = new OM.DocAttachmentsHistoryDetails_Info
                    {
                        FileExtension = new OM.Info(true),
                        FileName = new OM.Info(true),
                        Name = new OM.Info(true),
                        Version = new OM.Info(true)
                    },
                    AttachDocumentHistories = new OM.AttachDocumentHistory_Info
                    {
                        AttachedFileExtension = new OM.Info(true),
                        AttachedFileName = new OM.Info(true),
                        DocumentName = new OM.Info(true),
                        DocumentRevision = new OM.Info(true),
                    }
                };

                resultStatus = service.GetEnvironment(ServiceData, request, out result);

                if (resultStatus.IsSuccess && result.Value != null && result.Value.AvailableHistoryDetails != null)
                {
                    SetLayoutTabs(result.Value.AvailableHistoryDetails);
                    SetDocAttachment(result.Value);
                }
                ReloadDetailsData();
            }
            FilterGrid(GetCurrentDataGrid());
            SetDataGridIndexToNext();
            return null;
        }

        protected virtual void SetDocAttachment(OM.MfgAuditTrailInquiry serviceData)
        {
            if (DocAttachment != null)
            {
                if (IsAvailableHistoryDetail(OM.AvailableHistDetailsMaskEnum.DocAttachment) &&
                    serviceData.AttachmentsDetails != null && serviceData.AttachmentsDetails.Length > 0)
                {
                    var name = serviceData.AttachmentsDetails[0].Name.Value;
                    var attachmentsID = serviceData.Attachments.ID;

                    DocAttachment.Visible = true;

                    if (serviceData.AttachmentsDetails[0].Version != null)
                    {
                        var version = serviceData.AttachmentsDetails[0].Version.Value;
                        DocAttachment.OnClientClick = CreateDownloadFileScript(name, version, attachmentsID);
                    }
                    else
                    {
                        DocAttachment.OnClientClick = CreateDownloadFileScript(name, null, attachmentsID);
                    }

                }
                else
                    DocAttachment.Visible = false;
            }
            if (serviceData.AttachDocumentHistories != null && serviceData.AttachDocumentHistories.Length > 0)
            {
                var docName = serviceData.AttachDocumentHistories[0].DocumentName.Value + ":" + serviceData.AttachDocumentHistories[0].DocumentRevision.Value;
                Page.SessionVariables.SetValueByName("DocumentName", docName);
                ViewAttachmentButton.Visible = true;
            }
            else
            {
                ViewAttachmentButton.Visible = false;
            }
        } // void SetDocAttachment(MfgAuditTrailInquiry serviceData)

        protected virtual void SetLayoutTabs(OM.Enumeration<OM.AvailableHistDetailsMaskEnum, int> availableHistoryDetails)
        {
            AvailableHistoryDetails = availableHistoryDetails;
            var isFirst = true;
            var isTabsVisible = false;
            var tabs = DetailTabs.Tabs.OfType<JQTabPanel>();

            DetailTabs.Visible = true; //Workaround for the JQTabPanel Visible property logic
            foreach (var tab in tabs)
            {
                OM.AvailableHistDetailsMaskEnum tabEnum;
                if (Enum.TryParse<OM.AvailableHistDetailsMaskEnum>(tab.Name, true, out tabEnum))
                {
                    tab.Visible = IsAvailableHistoryDetail(tabEnum);
                    isTabsVisible = tab.Visible || isTabsVisible;

                    if (isFirst && isTabsVisible)
                    {
                        DetailTabs.SelectedIndex = tabs.ToList().IndexOf(tab);
                        isFirst = false;
                    }
                }
            }
            DetailTabs.Visible = isTabsVisible;
        } // SetLayoutTabs(Enumeration<AvailableHistDetailsMaskEnum, int> availableHistoryDetails)

        protected virtual string CreateDownloadFileScript(string name, string version, string attachmentID)
        {
            string script = string.Format("StartDownloadFile(\"{0}\", \"{1}\", \"{2}\")",
                   Camstar.WebPortal.Utilities.QueryStringUtil.SafeURL(name),
                   Camstar.WebPortal.Utilities.QueryStringUtil.SafeURL(version),
                   attachmentID);
            return script;
        } // string CreateDownloadFileScript(string name, string version, string attachmentID)

        protected virtual void ReloadDetailsData()
        {
            mkIsReloadDetailsData = true;
            DetailsRequested.ClearData();

            if (!string.IsNullOrEmpty(MainLineGrid.SelectedRowID))
            {
                var selectedTab = DetailTabs.SelectedItem;
                OM.DetailsRequestTypeEnum requestedType;

                if (selectedTab != null && Enum.TryParse<OM.DetailsRequestTypeEnum>(selectedTab.Name, true, out requestedType))
                    DetailsRequested.Data = requestedType;
            }
            else
            {
                SetLayoutTabs(new OM.Enumeration<OM.AvailableHistDetailsMaskEnum, int>(0));
                DetailsRequested.SettingData = false;
                DetailsRequested.OnDataChanged(EventArgs.Empty);
            }
        } // ReloadDetailsData()

        protected virtual bool IsAvailableHistoryDetail(OM.AvailableHistDetailsMaskEnum histDetails)
        {
            return AvailableHistoryDetails != null ? ((OM.AvailableHistDetailsMaskEnum)AvailableHistoryDetails.Value).HasFlag(histDetails) : false;
        } // IsAvailableHistoryDetail(AvailableHistDetailsMaskEnum histDetails)

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.OnClearValues += Page_OnClearValues;
            Page.OnGetSelectionData += Page_OnGetSelectionData;
        }
        private string reformatDateTime(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                var rx = new Regex(@"\d{1,2}\/\d{1,2}\/\d{4} \d{1,2}\:\d{2}:\d{2} [AP]M");
                // Convert DateTime to localized
                if (rx.Match(s) is Match m)
                {
                    // Parse data by USA format
                    var culture = CultureInfo.CreateSpecificCulture("en-US");
                    var d = DateTime.Parse(m.Value, culture);
                    var dl = d.ToShortDateString() + " " + d.ToLongTimeString();
                    var sr = rx.Replace(s, dl);
                    return sr;
                }
            }
            return null;
        }

        protected virtual void Page_OnClearValues(object sender, FormsFramework.ServiceDataEventArgs e)
        {
            ReloadDetailsData();
        } // void Page_OnClearValues(object sender, ServiceDataEventArgs e)

        protected virtual void Page_OnGetSelectionData(object sender, FormsFramework.ServiceDataEventArgs e)
        {
            int selectedMainlineRowID;

            if (e.Data is OM.MfgAuditTrailInquiry)
            {

                if (int.TryParse(MainLineGrid.SelectedRowID, out selectedMainlineRowID))
                {
                    int selectedDetailsRowID;
                    int selectedSummaryRowID;
                    string selectedMainlineId;

                    var inquiry = (OM.MfgAuditTrailInquiry)e.Data;
                    inquiry.SelectedHistoryMainlineIndex = selectedMainlineRowID;
                    if (MainLineGrid.SelectedItem != null)
                    {
                        DataRow row = MainLineGrid.SelectedItem as DataRow;

                        if (row != null && (row.Table.Columns.Contains("InstanceId")))
                        {
                            selectedMainlineId = row["InstanceId"].ToString();
                            if (!string.IsNullOrEmpty(selectedMainlineId))
                                inquiry.SelectedHistoryMainline = new OM.BaseObjectRef(selectedMainlineId);
                        }
                    }

                    if (int.TryParse(DetailsGrid.SelectedRowID, out selectedDetailsRowID))
                    {
                        inquiry.SelectedHistoryDetailsIndex = selectedDetailsRowID;
                    }
                    DetailsGridExpandedRowID = null;
                    if (SummaryGrid != null && int.TryParse(SummaryGrid.SelectedRowID, out selectedSummaryRowID))
                    {
                        inquiry.SelectedHistorySummaryIndex = selectedSummaryRowID;
                    }
                }
                else if (!mkIsReloadDetailsData)
                {
                    ReloadDetailsData();
                    mkIsReloadDetailsData = false;
                }
            }
        } // Page_OnGetSelectionData(object sender, ServiceDataEventArgs e)

        protected virtual void DetailTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DetailTabs.Visible)
            {
                mkIsReloadDetailsData = true;
                DetailsRequested.ClearData();

                if (!string.IsNullOrEmpty(MainLineGrid.SelectedRowID))
                {
                    var selectedTab = DetailTabs.SelectedItem;
                    OM.DetailsRequestTypeEnum requestedType;

                    if (selectedTab != null && Enum.TryParse<OM.DetailsRequestTypeEnum>(selectedTab.Name, true, out requestedType))
                        DetailsRequested.Data = (int)requestedType;

                    ShouldResetDetailTabs = false;
                    InitDataGrids();
                }
                else
                {
                    DetailsRequested.SettingData = false;
                    DetailsRequested.OnDataChanged(EventArgs.Empty);
                }
            }
        }

        protected virtual void SearchTxnBtn_Click(object sender, EventArgs e)
        {
            DetailTabs.Visible = false;
            MainLineGrid.Visible = true;
            ResetDataGridIndex();
        }

        #endregion

        #region Private methods

        private ResponseData DataGrid_RowSelected(object sender, CWGC.JQGridEventArgs args)
        {
            FilterGrid(GetCurrentDataGrid());
            SetDataGridIndexToNext();

            SelValGridContext dataContext = GetCurrentDataGrid().BoundContext as SelValGridContext;
            OM.RecordSet rs;
            OM.ResultStatus result = dataContext.GetSelectionValuesData(out rs, -1 );
            if (result.IsSuccess)
            {
                dataContext.SetSelectionValues(rs);
            }
            HideInactiveTabs();

            return null;
        }

        private void HideInactiveTabs()
        {
            SaveVisibleTabsIndexes(DetailTabs.Tabs);

            foreach (var tab in DetailTabs.Tabs)
            {
                (tab as JQTabPanel).Visible = false;
            }
            DetailTabs.SelectedItem.Visible = true;
        }

        private void SaveVisibleTabsIndexes(JQTabPanelCollection tabs)
        {
            List<int> indexes = new List<int>();
            int currentIndex = 0;

            foreach (JQTabPanel panel in tabs)
            {
                if (panel.Visible == true)
                {
                    indexes.Add(currentIndex);
                }
                currentIndex++;
            }
            Page.PortalContext.LocalSession[VisibleTabsKey] = indexes;
        }

        private void ShowVisibleTabs()
        {
            List<int> indexes = GetVisibleTabsIndexes();

            foreach (int index in indexes)
            {
                DetailTabs.Tabs[index].Visible = true;
            }
        }

        private List<int> GetVisibleTabsIndexes()
        {
            return Page.PortalContext.LocalSession[VisibleTabsKey] as List<int>;
        }

        private void SetRowSelectedHandlersToDataGrids()
        {
            AddGridRowSelectedHandler(MainLineGrid, MainLineGrid_RowSelected);

            for(int i=1; i < DataGrids.Count; i++)
            {
                if ((i < (DataGrids.Count - 1)) && (DataGrids[i + 1].Data != null))
                {
                    AddGridRowSelectedHandler(DataGrids[i], DataGrid_RowSelected);
                }
            }
        }

        private void InitDataGrids()
        {
            DataGrids.Clear();
            InitDataGrid(MainLineGrid);
            InitDataGrid(SummaryGrid);
            InitDataGrid(DetailsGrid);
            InitDataGrid(SubDetailsGrid);
        }

        private void FilterGrid(JQDataGrid grid)
        {
            LoadSelectedRowToFilterGridFrom(grid);
        }

        private void ReloadFilterGridData()
        {
            var filterGridContext = HistoryFilterGrid.BoundContext as SelValGridContext;
            string filteredGridId = GetGridIdByIndex(CurrentDataGridIndex - 1);

            if (!(String.IsNullOrEmpty(filteredGridId)))
            {
                OM.RecordSet rs = Page.PortalContext.LocalSession[FilterGridRecordsetKey + filteredGridId] as OM.RecordSet;
                filterGridContext.SetSelectionValues(rs);
                filterGridContext.Fields = Page.PortalContext.LocalSession[FilterGridFieldsKey + filteredGridId] as JQFieldCollection;
            }
        }

        private string GetGridIdByIndex(int index)
        {
            if ((index > -1) && (index < DataGrids.Count))
            {
                return DataGrids[index].ID;
            }
            else {
                return String.Empty;
            }
        }

        private OM.RecordSet GetRecordsetFromSelectedRow(CWGC.JQDataGrid srcGrid)
        {
            BoundContext srcContext = srcGrid.BoundContext;
            var srcData = srcContext.Data as DataTable;
            if (srcData.Rows.Count > 0)
            {
                int selectedRowIndex = Int32.Parse(srcContext.SelectedRowID);

                int visibleRows = srcGrid.Settings.VisibleRows ?? 0;
                int selectedRowIndexOnCurrentPage = visibleRows > 0 ? selectedRowIndex % visibleRows : 0;

                object[] selectedRow = (srcData.Rows[selectedRowIndexOnCurrentPage] as DataRow).ItemArray.Clone() as object[];

                return ConvertSelectedRowToRecordSet(selectedRow, srcData.Columns);
            }
            else
                return new OM.RecordSet();
        }

        private void LoadSelectedRowToFilterGridFrom(CWGC.JQDataGrid srcGrid)
        {
            string gridId = srcGrid.ID;
            Page.PortalContext.LocalSession[FilterGridRecordsetKey + gridId] = GetRecordsetFromSelectedRow(srcGrid);
            Page.PortalContext.LocalSession[FilterGridFieldsKey + gridId] = GetFieldsFrom(srcGrid);
        }

        private JQFieldCollection GetFieldsFrom(JQDataGrid grid)
        {
            JQFieldCollection fields = new JQFieldCollection();
            foreach (var field in grid.BoundContext.Fields)
            {
                JQField newField = new JQField();
                newField.Caption = field.Caption;
                newField.CellStyle = field.CellStyle;
                newField.ContentHidden = field.ContentHidden;
                newField.BindPath = field.BindPath;
                newField.DataType = field.DataType;
                newField.HeaderStyle = field.HeaderStyle;
                newField.LabelName = field.LabelName;
                newField.LabelText = field.LabelText;
                newField.ID = field.ID;
                newField.Visible = field.Visible;
                newField.Width = field.Width;
                fields.Add(newField);
            };
            return fields;
        }

        private string[] GetTableHeadersFromColumns(DataColumnCollection DCCollection)
        {
            if (DCCollection != null)
            {
                DataColumn[] DCArray = new DataColumn[DCCollection.Count];
                DCCollection.CopyTo(DCArray, 0);
                return DCArray.Select(c => c.ColumnName).ToArray();
            }
            return null;
        }

        private OM.RecordSet ConvertSelectedRowToRecordSet(object[] tableData, DataColumnCollection tableHeaders)
        {
            OM.RecordSet rs = new OM.RecordSet();

            OM.Header[] headers = tableHeaders.OfType<DataColumn>().Select(h => new OM.Header
            {
                Name = h.ColumnName,
                Label = new OM.Label(h.ColumnName),
                TypeCode = h.DataType == typeof(DateTime) ?TypeCode.DateTime : TypeCode.String
            }).ToArray();

            rs.Headers = headers;
            rs.Rows = new OM.Row[1];

            OM.Row row = new OM.Row();
            row.Values = tableData.Select(
                    o => o is DateTime ? ((DateTime)o).ToString(SortableDateTimePatternMillisec) : o.ToString()
            ).ToArray();

            rs.Rows[0] = row;
            return rs;
        }

        private JQDataGrid GetCurrentDataGrid()
        {
            return DataGrids[CurrentDataGridIndex];
        }

        private int GetCurrentDataGridIndex()
        {
            int? index = Page.PortalContext.LocalSession[CurrentDataGridIndexKey] as int?;

            if ((index != null) && (index.Value > 0 ))
            {
                return index.Value;
            }
            else { return 0; } //  Mainline grid is active
        }

        private void ResetDataGridIndex()
        {
            Page.PortalContext.LocalSession[CurrentDataGridIndexKey] = 0;
        }

        private void SetDataGridIndexToPrev()
        {
            Page.PortalContext.LocalSession[CurrentDataGridIndexKey] = (GetCurrentDataGridIndex() - 1);
        }

        private void SetDataGridIndexToNext()
        {
            Page.PortalContext.LocalSession[CurrentDataGridIndexKey] = (GetCurrentDataGridIndex() + 1);
        }

        private void SetDataGridIndexTo(int index)
        {
            Page.PortalContext.LocalSession[CurrentDataGridIndexKey] = index;
        }

        private int GetMaxBreadcrumbLevelForCurrentTab()
        {
            int level = 0;
            foreach (var grid in DataGrids)
            {
                if ((grid != null) && (grid.Data != null))
                {
                    level++;
                }
            }
            return level - 1; // without Mainline grid
        }

        private void SetShouldClearBreadcrumbCommandsToClientSide()
        {
            string script;

            if (CurrentDataGridIndex < 1 /* Mainline grid is current */)
            {
                script = $"var shouldClearBreadcrumbCommands_{Page.CallStackKey} = true;";
            }
            else
            {
                script = $"var shouldClearBreadcrumbCommands_{Page.CallStackKey} = false;";
            }
            ScriptManager.RegisterStartupScript(this, GetType(), "shouldClearBreadcrumbCommands", script, true);
        }

        private void SetTabNameToClientSide()
        {
            string script = $"var TransactionPanelTabName_{Page.CallStackKey} = '{DetailTabs.SelectedItem.Name}';";

            ScriptManager.RegisterStartupScript(this, GetType(), "TransactionPanelTabName", script, true);
        }

        private void SetMaxBreadcrumbLevelToClientSide()
        {
            string script = $"var MaxBreadcrumbLevel_{Page.CallStackKey} = {MaxBreadcrumbLevelForCurrentTab};";

            ScriptManager.RegisterStartupScript(this, GetType(), "MaxBreadcrumbLevel", script, true);
        }

        private void AddGridRowSelectedHandler(CWGC.JQDataGrid grid, JQGridEventHandler handler)
        {
            if ((grid != null) && (handler != null))
            {
                grid.RowSelected += handler;
            }
        }

        private void InitDataGrid(CWGC.JQDataGrid dataGrid)
        {
            if (dataGrid != null)
            {
                DataGrids.Add(dataGrid);
            }
        }

        private void InitFilterGrid()
        {
            HistoryFilterGrid.LabelPosition = PERS.LabelPositionType.Hidden;
            HistoryFilterGrid.LabelText = String.Empty;
            HistoryFilterGrid.CssClass = "FilterGrid";
            HistoryFilterGrid.BoundContext.VisibleRows = 1;
            HistoryFilterGrid.Settings.VisibleRows = 1;
            HistoryFilterGrid.Settings.ParentGrid = null;
            HistoryFilterGrid.Settings.Navigator = PERS.JQGridNavigatorMode.Disabled;
            HistoryFilterGrid.Settings.Pager.Mode = PERS.GridPagerModes.AlwaysHidden;
            HistoryFilterGrid.Settings.Pager.DisplayTotalRecords = false;
            HistoryFilterGrid.Settings.Pager.ShowButtons = false;

            ReloadFilterGridData();
            RefreshFilterGridVisibility();
        }

        private void RefreshFilterGridVisibility()
        {
            if ((HistoryFilterGrid.BoundContext.Data != null)&& (MainLineGrid.SelectedItem != null))
            {
                HistoryFilterGrid.Visible = true;
            }
            else
            {
                HistoryFilterGrid.Visible = false;
            }
            CamstarWebControl.SetRenderToClient(HistoryFilterGrid);
        }

        private void RefreshDataGridsVisibility()
        {
            if (CurrentDataGridIndex > 0)
            {
                MainLineGrid.Visible = false;
                DetailTabs.Visible = true;

                for(int i = 1; i < DataGrids.Count; i++)
                {
                    DataGrids[i].Hidden = true;
                }
                DataGrids[CurrentDataGridIndex].Visible = true;
                DataGrids[CurrentDataGridIndex].Hidden = false;
            }
            else {
                MainLineGrid.Visible = true;
                DetailTabs.Visible = false;
            }
            CamstarWebControl.SetRenderToClient(MainLineGrid);
            CamstarWebControl.SetRenderToClient(DetailTabs);
        }

        #region Actions on breadcrumb events

        private void ReturnToDataGridWithIndex(int index)
        {
            if ((index == 0) && (DetailTabs.SelectedIndex != index))
            {
                ShouldResetDetailTabs = true;
            }
            if (index == 1)
            {
                ShowVisibleTabs();
            }
            ResetGridRowSelectionForGridIndex(index);
            SetDataGridIndexTo(index);

            CamstarWebControl.SetRenderToClient(DataGrids[index]);
            CamstarWebControl.SetRenderToClient(HistoryFilterGrid);
        }
        private void ResetDetailTabs()
        {
            DetailTabs.ClearSelection();
            DetailTabs.SelectedIndex = 0;
            DetailTabs.Tabs[0].Selected = true;
        }

        private void ResetGridRowSelectionForGridIndex(int index)
        {
            for (int i = index; i < DataGrids.Count; i++)
            {
                ResetGridRowSelection(DataGrids[i]);
            }
        }

        private void ResetGridRowSelection(CWGC.JQDataGrid grid)
        {
            grid.BoundContext.SelectedItem = null;
            grid.BoundContext.SelectedRowID = null;
            grid.BoundContext.SelectedRowIDs = null;
            grid.BoundContext.SelectedRowIndex = null;
        }
        #endregion

        #endregion

        #region Fields

        private bool mkIsReloadDetailsData = false;

        #endregion

        #region Constants
        protected const string mkAvailableHistoryDetails = "AvailableHistoryDetailsViewState";
        protected const string mkDetailsGridIdPattern = "MfgAuditTrailInquiry_{0}Grid";
        protected const string mkSubDetailsGridIdPattern = "MfgAuditTrailInquiry_Sub{0}Grid";
        protected const string mkSummaryGridIdPattern = "MfgAuditTrailInquiry_{0}SummaryGrid";

        protected readonly string SortableDateTimePatternMillisec = System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.SortableDateTimePattern + "'.'fff";


        #endregion
    }

}

