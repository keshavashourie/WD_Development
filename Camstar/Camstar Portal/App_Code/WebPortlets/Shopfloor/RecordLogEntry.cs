// Copyright Siemens 2025
using System;

using OM = Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;

using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using System.Web;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.WCFUtilities;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class RecordLogEntry : MatrixWebPart
    {
        //protected virtual JQDataGrid psDetails { get { return Page.FindCamstarControl("Control") as JQDataGrid; } }
        protected virtual CWC.NamedObject OperatorLogField { get { return Page.FindCamstarControl("RecordLogEntry_OperatorLog") as CWC.NamedObject; } }
        protected virtual CWC.TextBox ObjectType { get { return Page.FindCamstarControl("RecordLogEntry_ObjectType") as CWC.TextBox; } }
        protected virtual CWC.NamedObject NamedDataObject { get { return Page.FindCamstarControl("RecordLogEntry_NamedDataObject") as CWC.NamedObject; } }
        protected virtual CWC.RevisionedObject RevisionedObject { get { return Page.FindCamstarControl("RecordLogEntry_RevisionedObject") as CWC.RevisionedObject; } }
        protected virtual CWC.CheckBox IsContainer { get { return Page.FindCamstarControl("RecordLogEntry_IsContainer") as CWC.CheckBox; } }
        protected virtual CWC.CheckBox IsQualityObject { get { return Page.FindCamstarControl("RecordLogEntry_IsQualityObject") as CWC.CheckBox; } }
        protected virtual CWC.CheckBox IsNDO { get { return Page.FindCamstarControl("RecordLogEntry_IsNDO") as CWC.CheckBox; } }
        protected virtual CWC.CheckBox IsRDO { get { return Page.FindCamstarControl("RecordLogEntry_IsRDO") as CWC.CheckBox; } }
        protected virtual CWC.TextBox LogEntry { get { return Page.FindCamstarControl("RecordLogEntry_LogEntry") as CWC.TextBox; } }
        protected virtual CWC.NamedObject QualityObject { get { return Page.FindCamstarControl("RecordLogEntry_QualityObject") as CWC.NamedObject; } }
        protected virtual ContainerListGrid Container { get { return Page.FindCamstarControl("RecordLogEntry_Container") as ContainerListGrid; } }
        protected virtual CWC.Button DocAttachButton { get { return Page.FindCamstarControl("AttachButton") as CWC.Button; } }
        protected virtual CWC.Button DocViewButton { get { return Page.FindCamstarControl("ViewButton") as CWC.Button; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            OperatorLogField.DataChanged += OperatorLog_DataChanged;
            if(!Page.IsPostBack)
            {
                //ObjectType.Visible = false;
                RevisionedObject.Visible = false;
                NamedDataObject.Visible = false;
                Container.Visible = false;
                QualityObject.Visible = false;
            }

            if (AttachBtn != null || DetachBtn != null)
            {
                AttachBtn.Click += new EventHandler(AttachBtn_Click);
                DetachBtn.Click += new EventHandler(DetachBtn_Click);
                ViewBtn.Click += new EventHandler(ViewBtn_Click);
                AttachedDocumentsGrid.RowSelected += new JQGridEventHandler(AttachedDocumentsGrid_RowSelected);
                if (AttachedDocumentsGrid.SelectedRowIDs != null)
                {
                    if (AttachedDocumentsGrid.SelectedRowIDs.Length > 0)
                    {
                        DetachBtn.Enabled = true;
                        ViewBtn.Enabled = true;
                    }
                }
                else
                {
                    DetachBtn.Enabled = false;
                    ViewBtn.Enabled = false;
                    DocumentInstanceField.ClearData();
                }
            }
        }        

        private void OperatorLog_DataChanged(object sender, EventArgs e)
        {
            LogEntry.ClearData();

            if (OperatorLogField.Data == null)
            {
                RevisionedObject.ClearData();
                NamedDataObject.ClearData();
                IsRDO.ClearData();
                IsNDO.ClearData();
                ObjectType.ClearData();
                QualityObject.ClearData();
                Container.ClearData();
                RevisionedObject.Visible = false;
                NamedDataObject.Visible = false;
                Container.Visible = false;
                QualityObject.Visible = false;

                return;
            }
            var service = new RecordLogEntryService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var data = new OM.RecordLogEntry();
            var info = new OM.RecordLogEntry_Info();
            var request = new RecordLogEntry_Request();
            OM.ResultStatus rs = new OM.ResultStatus();
            info.ObjectType = FieldInfoUtil.RequestValue();
            info.IsNDO = FieldInfoUtil.RequestValue();
            info.IsRDO = FieldInfoUtil.RequestValue();
            info.IsContainer = FieldInfoUtil.RequestValue();
            info.IsQualityObject = FieldInfoUtil.RequestValue();
            info.NamedDataObject = FieldInfoUtil.RequestValue();
            info.RevisionedObject = FieldInfoUtil.RequestValue();
            info.ObjectTypeName = FieldInfoUtil.RequestValue();
            info.QualityObject = FieldInfoUtil.RequestValue();
            info.Container = FieldInfoUtil.RequestValue();
            info.AttachDocumentDetails = new AttachDocumentDetails_Info() { RequestValue = true };

            request.Info = info;

            data.OperatorLog = (OM.NamedObjectRef)OperatorLogField.Data as OM.NamedObjectRef;

            RecordLogEntry_Result result = null;
            rs = service.GetEnvironment(data, request, out result);
            if (rs.IsSuccess)
            {
                //set field values
                NamedDataObject.Data = result.Value.NamedDataObject;
                RevisionedObject.Data = result.Value.RevisionedObject;
                ObjectType.Data = result.Value.ObjectTypeName;
                IsNDO.CheckControl.Checked = (Boolean)result.Value.IsNDO;
                IsRDO.CheckControl.Checked = (Boolean)result.Value.IsRDO;
                IsContainer.CheckControl.Checked = (Boolean)result.Value.IsContainer;
                IsQualityObject.CheckControl.Checked = (Boolean)result.Value.IsQualityObject;
                QualityObject.Data = result.Value.QualityObject;
                Container.Data = result.Value.Container;
                AttachedDocumentsGrid.Data = result.Value.AttachDocumentDetails;

                if (IsNDO.CheckControl.Checked)
                {
                    RevisionedObject.Visible = false;
                    NamedDataObject.Visible = true;
                    Container.Visible = false;
                    QualityObject.Visible = false;
                }
                else if (IsRDO.CheckControl.Checked)
                {
                    NamedDataObject.Visible = false;
                    RevisionedObject.Visible = true;
                    Container.Visible = false;
                    QualityObject.Visible = false;
                }
                else if(IsQualityObject.CheckControl.Checked)
                {
                    RevisionedObject.Visible = false;
                    NamedDataObject.Visible = false;
                    Container.Visible = false;
                    QualityObject.Visible = true;
                }
                else if (IsContainer.CheckControl.Checked)
                {
                    RevisionedObject.Visible = false;
                    NamedDataObject.Visible = false;
                    Container.Visible = true;
                    QualityObject.Visible = false;
                }
                ObjectType.Visible = true;
            }
            else
                DisplayMessage(rs);
        }        

        /// <summary>
        /// Doc attach functionality
        /// </summary>
        #region Controls

        protected virtual JQDataGrid AttachedDocumentsGrid { get { return Page.FindCamstarControl("AttachedDocuments") as JQDataGrid; } }

        protected virtual CWC.Button AttachBtn { get { return Page.FindCamstarControl("AttachButton") as CWC.Button; } }

        protected virtual CWC.Button DetachBtn { get { return Page.FindCamstarControl("DetachButton") as CWC.Button; } }

        protected virtual CWC.Button ViewBtn { get { return Page.FindCamstarControl("ViewButton") as CWC.Button; } }

        protected virtual CWC.TextBox NameField { get { return Page.FindCamstarControl("NameTxt") as CWC.TextBox; } }

        protected virtual CWC.RevisionedObject DocumentInstanceField { get { return Page.FindCamstarControl("AttachDocument_DocInstance") as CWC.RevisionedObject; } }

        protected virtual CWC.TextBox StoredFileNameField { get { return Page.FindCamstarControl("AttachDocument_AttachedFileName") as CWC.TextBox; } }

        protected virtual CWC.TextBox InstanceNameField { get { return Page.FindCamstarControl("NameTxt") as CWC.TextBox; } }
        #endregion

        protected virtual CWC.CheckBox AttachAsRORField { get { return Page.FindCamstarControl("AttachAsROR") as CWC.CheckBox; } } // AttachAsROR

        protected virtual CWC.CheckBox UseRORField { get { return Page.FindCamstarControl("UseROR") as CWC.CheckBox; } } // UseROR

        protected virtual CWC.TextBox SelectedObjectTypeIdField { get { return Page.FindCamstarControl("SelectedObjectTypeId") as CWC.TextBox; } } // SelectedObjectTypeIdField

        protected virtual CWC.RevisionedObject DoumentInHistoryField { get { return Page.FindCamstarControl("AttachDocument_DocInHistory") as CWC.RevisionedObject; } }//DoumentInHistoryField

        #region Protected Functions

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (InstanceNameField.Data == null)
                AttachBtn.Enabled = false;
            else
                AttachBtn.Enabled = true;
        }

        protected virtual ResponseData AttachedDocumentsGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            if (AttachedDocumentsGrid.SelectedRowIDs != null && AttachedDocumentsGrid.SelectedRowIDs.Length > 0)
            {
                DetachBtn.Enabled = true;
                ViewBtn.Enabled = true;
                AttachDocumentDetails attachedDocDetail = (AttachDocumentDetails)AttachedDocumentsGrid.GridContext.GetItem(AttachedDocumentsGrid.SelectedRowID);
                RevisionedObjectRef r = new RevisionedObjectRef();
                r.Name = (string)attachedDocDetail.DocumentName;
                if ((Boolean)attachedDocDetail.UseROR != true)
                {
                    r.Revision = (string)attachedDocDetail.DocumentRevision;
                }

                r.RevisionOfRecord = (Boolean)attachedDocDetail.UseROR;
                r.CDOTypeName = "Document";
                DocumentInstanceField.Data = r;
                ScriptManager.RegisterArrayDeclaration(this, "__startFocusElement", "'" + DocViewButton.ClientID + "'");
                return new StatusData(true, "Row selected");
            }
            else
            {
                DetachBtn.Enabled = false;
                ViewBtn.Enabled = false;
                DocumentInstanceField.ClearData();
                ScriptManager.RegisterArrayDeclaration(this, "__startFocusElement", "'" + DocAttachButton.ClientID + "'");
                return new StatusData(false, "Row not selected");
            }
        }

        protected virtual void ViewBtn_Click(object sender, EventArgs e)
        {
            var docInfo = DownloadDocument();
            if (docInfo != null && !string.IsNullOrEmpty(docInfo.FileName))
            {
                var src = docInfo.IsRemote
                    ? string.Format("OpenDocumentUrl('{0}','{1}','');", JavascriptUtil.ConvertForJavascript(docInfo.URI), docInfo.AuthenticationType)
                    : string.Format("window.open('DownloadFile.aspx?viewdocfile={0}');", docInfo.FileName);
                StoredFileNameField.Data = docInfo.FileName;
                ScriptManager.RegisterStartupScript(this, Page.GetType(), "opendocument", src, true);
                StoredFileNameField.ClearData();
            }
        }

        protected virtual void DetachBtn_Click(object sender, EventArgs e)
        {
            Page.DataContract.SetValueByName("AttachedDocumentViewOnlyDM", DocumentInstanceField.Data);
            Page.DataContract.SetValueByName("AttachedDocumentDM", DoumentInHistoryField.Data);
            Page.DataContract.SetValueByName("CalledExternallyDM", false);
            Page.DataContract.SetValueByName("ObjectTypeIdDM", 8997);//CDODefID for OperatorLog CDO
            Page.DataContract.SetValueByName("AttachAsRORDM", AttachAsRORField.IsChecked);
            Page.DataContract.SetValueByName("UseRORDM", UseRORField.IsChecked);
            Page.DataContract.SetValueByName("DispalyNameDM", (string)NameField.Data);//Page.DataContract.GetValueByName("InstanceName"));

            Page.DataContract.SetValueByName("ObjectTypeDM", 0);
            Page.DataContract.SetValueByName("InstanceNameDM", (string)NameField.Data);//Page.DataContract.GetValueByName("InstanceName"));
            Page.DataContract.SetValueByName("IsNDODM", true);
            Page.DataContract.SetValueByName("IsRDODM", false);
            Page.DataContract.SetValueByName("IsContainerDM", false);
            Page.SessionDataContract.SetValueByName("detach_" + Page.Session.SessionID, true);
        }

        protected virtual void AttachBtn_Click(object sender, EventArgs e)
        {
            Page.DataContract.SetValueByName("IsContainer", false);
            Page.DataContract.SetValueByName("CalledExternally", true);
            Page.DataContract.SetValueByName("IsNDO", true);
            Page.DataContract.SetValueByName("IsRDO", false);
            Page.DataContract.SetValueByName("ServiceTypeName", "OperatorLogMaint");//NDO service to use to associate the document
            Page.DataContract.SetValueByName("ObjectTypeId", 8997); //CDODefID for OperatorLog CDO
            Page.DataContract.SetValueByName("NDOName", NameField.Data);
            Page.SessionDataContract.SetValueByName("attach_" + Page.Session.SessionID, true);
        }

        public override void ChildPostExecute(ResultStatus status, Service serviceData)
        {
            base.ChildPostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                var attached = Page.SessionDataContract.GetValueByName("attach_" + Page.Session.SessionID);
                var detached = Page.SessionDataContract.GetValueByName("detach_" + Page.Session.SessionID);
                if (((attached != null) && ((bool)attached == true)) || ((detached != null) && ((bool)detached == true)))
                {
                    AttachedDocumentsGrid.Action_SelectRow(null, "deselect");
                    AttachedDocumentsGrid_RowSelected(null, null);
                    OperatorLog_DataChanged(null, new EventArgs());
                }
            }
        }

        #endregion

        #region Protected Functions
        protected virtual DocumentRefInfo DownloadDocument()
        {
            ResultStatus resultStatus;
            var documentRev = (RevisionedObjectRef)DocumentInstanceField.Data;

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
        #endregion        
    }
}
