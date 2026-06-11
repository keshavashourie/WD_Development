// Copyright Siemens 2023  
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CWGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using DocumentMaintService = Camstar.WCF.Services.DocumentMaintService;
using DocumentMaint_Request = Camstar.WCF.Services.DocumentMaint_Request;
using DocumentMaint_Result = Camstar.WCF.Services.DocumentMaint_Result;
using OM = Camstar.WCF.ObjectStack;


namespace Camstar.WebPortal.WebPortlets.ResourceAuditTrail
{
    public class TransactionsPanel : MatrixWebPart
    {
        #region Controls

        protected virtual CWC.DropDownList DetailsRequested
        {
            get { return FindCamstarControl("ResourceAuditTrailInquiry_DetailsRequestedType") as CWC.DropDownList; }
        } // DetailsRequested

        protected virtual CWGC.JQDataGrid MainLineGrid
        {
            get { return FindCamstarControl("ResourceAuditTrailInquiry_MainLineGrid") as CWGC.JQDataGrid; }
        } // MainLineGrid

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

        protected virtual JQTabContainer DetailTabs
        {
            get { return FindCamstarControl("ResourceAuditTrailInquiry_DetailTabs") as JQTabContainer; }
        }

        protected virtual CWC.Button DocAttachment
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_DocAttachmentButton") as CWC.Button; }
        } // DocAttachment
        protected virtual CWC.Button ViewAttachmentButton
        {
            get { return Page.FindCamstarControl("ViewAttachmentButton") as CWC.Button; }
        }
        protected virtual CWC.Button SearchTxnBtn
        {
            get { return Page.FindCamstarControl("SearchTxnBtn") as CWC.Button; }
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

        #endregion

        #region Protected methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DetailTabs.SelectedIndexChanged += DetailTabs_SelectedIndexChanged;
            MainLineGrid.RowSelected += MainLineGrid_RowSelected;
            SearchTxnBtn.Click += SearchTxnBtn_Click;
            DetailsGrid.GridContext.RowExpanding += GridContext_RowExpanding;
            ViewAttachmentButton.Click+=ViewAttachmentButton_Click;
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
            var docInfo = AttachmentExecutor.DownloadDocumentRef(documentRev, session.CurrentUserProfile, message, out resultStatus);
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
                var service = new Camstar.WCF.Services.ResourceAuditTrailInquiryService(session.CurrentUserProfile);

                var serviceData = new OM.ResourceAuditTrailInquiry();

                Page.GetSelectionData(serviceData);

                var request = new Camstar.WCF.Services.ResourceAuditTrailInquiry_Request();
                var result = new Camstar.WCF.Services.ResourceAuditTrailInquiry_Result();
                var resultStatus = new OM.ResultStatus();

                request.Info = new OM.ResourceAuditTrailInquiry_Info
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

                resultStatus = service.GetEnvironment(serviceData, request, out result);

                if (resultStatus.IsSuccess && result.Value != null && result.Value.AvailableHistoryDetails != null)
                {
                    SetLayoutTabs(result.Value.AvailableHistoryDetails);
                    SetDocAttachment(result.Value);
                }

                ReloadDetailsData();
                if (result.Value != null && result.Value.AvailableHistoryDetails != null  && result.Value.AvailableHistoryDetails.ToString() == OM.AvailableHistDetailsMaskEnum.DataCollection.ToString())
                {
                    DetailTabs.SelectedIndex = DetailTabs.SelectedItem.TabIndex;
                    ReloadTabs();
                }
            }

            return null;
        } // MainLineGrid_RowSelected(object sender, JQGridEventArgs args)

        protected virtual void SetDocAttachment(OM.ResourceAuditTrailInquiry serviceData)
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
                var docName = serviceData.AttachDocumentHistories[0].DocumentName.Value;
                Page.SessionVariables.SetValueByName("DocumentName", docName);
                ViewAttachmentButton.Visible = true;
            }
            else
            {
                ViewAttachmentButton.Visible = false;
            }
        } // void SetDocAttachment(ResourceAuditTrailInquiry serviceData)

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
            SummaryGrid.ClearData();

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

        protected virtual ResponseData GridContext_RowExpanding(object sender, CWGC.JQGridEventArgs args)
        {
            detailsGridExpandedRowID = (sender as GridContext).ResolveID(args.State.RowID);
            return null;
        } // OnInit(EventArgs e)

        protected virtual void Page_OnClearValues(object sender, FormsFramework.ServiceDataEventArgs e)
        {
            ReloadDetailsData();
        } // void Page_OnClearValues(object sender, ServiceDataEventArgs e)

        protected virtual void Page_OnGetSelectionData(object sender, FormsFramework.ServiceDataEventArgs e)
        {
            int selectedMainlineRowID;
            if (e.Data is OM.ResourceAuditTrailInquiry)
            {
                if (int.TryParse(MainLineGrid.SelectedRowID, out selectedMainlineRowID))
                {
                    int selectedDetailsRowID;
                    int selectedSummaryRowID;
                    string selectedMainlineId;

                    var inquiry = (OM.ResourceAuditTrailInquiry)e.Data;

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

                    if (int.TryParse(detailsGridExpandedRowID, out selectedDetailsRowID))
                    {
                        inquiry.SelectedHistoryDetailsIndex = selectedDetailsRowID;
                    }
                    detailsGridExpandedRowID = null;
                    if (SummaryGrid != null && int.TryParse(SummaryGrid.GridContext.ExpandedRowID, out selectedSummaryRowID))
                    {
                        inquiry.SelectedHistorySummaryIndex = selectedSummaryRowID;
                        SummaryGrid.GridContext.ExpandedRowID = null;
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
            ReloadTabs();
        } // DetailTabs_SelectedIndexChanged(object sender, EventArgs e)

        protected void ReloadTabs()
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
        }

        #endregion

        #region Private methods

        #endregion

        #region Fields

        private bool mkIsReloadDetailsData = false;
        protected virtual string detailsGridExpandedRowID
        {
            get
            {
                var val = Page.DataContract.GetValueByName("detailsGridExpandedRowID");
                return val == null ? "" : val.ToString();
            }
            set { Page.DataContract.SetValueByName("detailsGridExpandedRowID", value);}
        }

        #endregion

        #region Constants

        protected const string mkAvailableHistoryDetails = "AvailableHistoryDetailsViewState";
        protected const string mkDetailsGridIdPattern = "ResourceAuditTrailInquiry_{0}Grid";
        protected const string mkSummaryGridIdPattern = "ResourceAuditTrailInquiry_{0}SummaryGrid";

        #endregion
    }
}
