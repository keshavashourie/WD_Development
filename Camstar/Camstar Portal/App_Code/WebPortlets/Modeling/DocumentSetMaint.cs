// Copyright Siemens 2025
using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.Utilities;


namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class DocumentSetMaint : MatrixWebPart
    {
        #region Protected methods

        protected override void OnLoad(EventArgs e)
        {
            DocumentEntriesGrid.RowSelected += new JQGridEventHandler(DocumentEntriesGrid_RowSelected);
            DocumentEntriesGrid.GridContext.RowUpdated += GridContext_RowUpdated;

            base.OnLoad(e);
        }

        protected override void OnPreRender(System.EventArgs e)
        {

            if (!DocumentEntriesGrid.IsRowSelected)
            {
                ViewDocument.Data = null;
                ViewDocument.Enabled = false;
            }

            base.OnPreRender(e);

            if (_needToRefreshDocument.HasValue && _needToRefreshDocument.Value)
            {
                _needToRefreshDocument = null;
                if (_documentSet != null)
                {
                    var docSet = _documentSet.Clone();
                    ViewDocument.Data = docSet;
                    _documentSet = null;
                }
                CamstarWebControl.SetRenderToClient(ViewDocument);
            }
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ViewDocument", "ViewDocument();", true);
        }

        public override void GetInputData(OM.Service service)
        {
            base.GetInputData(service);
        }

        public override ValidationStatus ValidateInputData(Service serviceData)
        {
            ValidationStatus status = base.ValidateInputData(serviceData);
            if (status.IsSuccess)
            {
                OM.DocumentSetMaint svc = (OM.DocumentSetMaint)serviceData;
                if (svc != null && svc.ObjectChanges != null && svc.ObjectChanges.DocumentEntries != null)
                {
                    foreach (DocumentEntryChanges item in svc.ObjectChanges.DocumentEntries)
                    {
                        if (item.ListItemAction != ListItemAction.Delete && (item.Name == null || string.IsNullOrEmpty(item.Name?.Value)))
                        {
                            LabelCache cache = FrameworkManagerUtil.GetLabelCache(Page.Session);
                            if (cache != null)
                            {
                                WCF.ObjectStack.Label docEntryName = new WCF.ObjectStack.Label();
                                docEntryName = cache.GetLabelByName("DocumentEntry_Name");
                                ValidationStatusItem statusItem = new RegularValidationStatusItem(docEntryName.Value);
                                status.Add(statusItem);

                            }
                        }
                    }
                }
            }
            return status;
        }
        protected virtual ResponseData DocumentEntriesGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            SetDocument((DocumentEntriesGrid.BoundContext as ItemDataContext).GetSelectedCell("Document") as string);
            return args.Response;
        }

        protected virtual ResponseData GridContext_RowUpdated(Object sender, JQGridEventArgs e)
        {
            var response = e.Response;

            var documentEntriesGridData = DocumentEntriesGrid.Data as DocumentEntryChanges[];
            if (documentEntriesGridData != null)
            {
                var index = Convert.ToInt32(e.State.RowID);
                if (!documentEntriesGridData[index].IsNullOrEmpty())
                {
                    DocumentEntriesGrid.GridContext.SelectRow(e.State.RowID, true);
                    SetDocument((DocumentEntriesGrid.BoundContext as ItemDataContext).GetSelectedCell("Document") as string);
                    return e.Response;
                }
            }

            return response;
        }

        protected virtual DocumentMaint_Result GetDocumentInformation(string name, string revision)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.DocumentMaintService(session.CurrentUserProfile);
            var serviceData = new OM.DocumentMaint();
            if (string.IsNullOrEmpty(revision))
            {
                serviceData.ObjectToChange = new RevisionedObjectRef(name);
            }
            else
            {
                serviceData.ObjectToChange = new RevisionedObjectRef(name, revision);
            }
            var request = new Camstar.WCF.Services.DocumentMaint_Request();
            var result = new Camstar.WCF.Services.DocumentMaint_Result();
            var resultStatus = new OM.ResultStatus();

            request.Info = new OM.DocumentMaint_Info()
            {
                RequestValue = true,
                ObjectChanges = new DocumentChanges_Info()
                {
                    BrowseMode = new OM.Info(true),
                    FileLocation = new OM.Info(true),
                    FileName = new OM.Info(true),
                    FileVersion = new OM.Info(true),
                    Identifier = new OM.Info(true),
                    Name = new OM.Info(true)

                }

            };

            resultStatus = service.Load(serviceData, request, out result);

            if (resultStatus.IsSuccess)
                return result;

            return null;

        }

        protected virtual void SetViewDocument(string docName, string docIdentifier, BrowseModeEnum browseMode, string docRevision)
        {

            var docSet = new DocumentSet
            {
                DocumentEntries = new DocumentEntry[]
                {
                    new DocumentEntry
                    {
                        DocumentIdentifier = docIdentifier,
                        Name = docName,
                        DisplayName = docName,
                        Document = new RevisionedObjectRef(docName,docRevision),
                        DocumentBrowseMode = (int)browseMode
                    }
                    }
            };

            ViewDocument.Data = docSet;
            _documentSet = docSet;
            _needToRefreshDocument = true;
            ViewDocument.Enabled = true;
        }


        protected virtual void SetDocument(string docName)
        {
            var docRevision = string.Empty;
            if (docName != null)
            {
                var revDelimiter = CamstarPortalSection.GetRevisionDelimiter();
                if (docName.Contains(revDelimiter))
                {
                    docRevision = docName.Substring(docName.LastIndexOf(revDelimiter) + 1);
                    docName = docName.Substring(0, docName.LastIndexOf(revDelimiter));
                }
                var docInfo = GetDocumentInformation(docName, docRevision);
                if (docInfo != null)
                {
                    if (docInfo.Value.ObjectChanges.Identifier != null)
                    {
                        var docIdentifier = docInfo.Value.ObjectChanges.Identifier.ToString();
                        var browseMode = (BrowseModeEnum)docInfo.Value.ObjectChanges.BrowseMode.Value;
                        SetViewDocument(docName, docIdentifier, browseMode, docRevision);
                    }
                    else
                    {
                        ViewDocument.Data = null;
                        ViewDocument.Enabled = false;
                    }
                }
            }

        }

        #endregion

        #region Controls

        protected virtual JQDataGrid DocumentEntriesGrid
        {
            get { return Page.FindCamstarControl("ObjectChanges_DocumentEntries") as JQDataGrid; }
        }

        protected virtual CWC.ViewDocumentsControl ViewDocument
        {
            get { return Page.FindCamstarControl("ViewDocument") as CWC.ViewDocumentsControl; }
        }

        #endregion

        #region Private Member Variables

        protected virtual DocumentSet _documentSet
        {
            get
            {
                if (Page.Session["docSet"] == null)
                    return null;
                return (DocumentSet)Page.Session["docSet"];
            }
            set
            {
                Page.Session["docSet"] = value;
            }
        }

        protected virtual bool? _needToRefreshDocument
        {
            get
            {
                if (Page.Session["needToRefreshDoc"] == null)
                    return null;
                return (bool)Page.Session["needToRefreshDoc"];
            }
            set
            {
                Page.Session["needToRefreshDoc"] = value;
            }
        }

        #endregion
    }
}
