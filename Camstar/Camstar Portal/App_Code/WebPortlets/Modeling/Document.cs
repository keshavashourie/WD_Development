// Copyright Siemens 2024
using System;
using IO = System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Camstar.WCF.ObjectStack;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.FormsFramework.HtmlControls;
using Camstar.WebPortal.FormsFramework;


/// <summary>
/// Summary description for Document
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class Document : MatrixWebPart
    {
        #region Public Methods

        public override void DisplayValues(Service serviceData)
        {
            base.DisplayValues(serviceData);

            var changes = (serviceData as DocumentMaint).ObjectChanges;

            SetBrowseMode(changes);
            DefineFormMode();

            var browseModeData = BrowseMode.Data != null ?(BrowseModeEnum)BrowseMode.Data : GetBrowseMode(changes);
            ShowFieldsForMode(browseModeData);
            DisplayDataForMode(browseModeData, (string)changes.Identifier);
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            if (!IsValidServiceType(serviceData)) return;
            
            DocumentChanges changes = (serviceData as DocumentMaint).ObjectChanges;
            SetBrowseMode(changes);

            switch ((BrowseModeEnum)BrowseMode.Data)
            {
                case BrowseModeEnum.LocalFile:
                    if (changes == null)
                    {
                        var wcfObjHelper = new WCFObject(serviceData);
                        var type = wcfObjHelper.GetFieldType("ObjectChanges");
                        (serviceData as DocumentMaint).ObjectChanges = wcfObjHelper.CreateEmptyValue(type) as DocumentChanges;
                        changes = (serviceData as DocumentMaint).ObjectChanges;
                    }
                    GetLocalFileData(changes);
                    break;
                     
                case BrowseModeEnum.HTTPFile:
                    if (changes != null)
                    {
                        GetHTTPData(changes);
                    }
                    break;

                case BrowseModeEnum.Url:
                    if (changes != null)
                    {
                        GetUrlData(changes);
                    }
                    break;
            }
        }

        public virtual void DocumentActionsButton_Click(object sender, EventArgs e)
        {
            if (StoredFileNameField.IsEmpty)
            {
                string scr = string.Format("OpenDocumentUrl('{0}','{1}','{2}', '{3}');",
                    JavascriptUtil.ConvertForJavascript((string)BrowseFileField.Data),
                    AuthenticationType.Text,
                    CantFindDocumentLabel.ClientID,
                    EmptyDocumentURLLabel.ClientID);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenDocument", scr, true);
                RenderToClient = true;
            }
            else
            {
                var docInfo = DownloadDocument();
                if (docInfo != null && !string.IsNullOrEmpty(docInfo.FileName))
                {
                    var src = "";

                    if (System.IO.Path.GetExtension(docInfo.FileName).ToLower() == ".jt")
                    {
                        src = string.Format("documentMaint.openModelViewerPopup('{0}','{1}', '{2}');", JavascriptUtil.ConvertForJavascript(docInfo.DocumentRef.Name),
                                             JavascriptUtil.ConvertForJavascript(docInfo.DocumentRef.Revision), "3D Model Popup");
                    }
                    else
                    {
                        src = docInfo.IsRemote
                           ? string.Format("OpenDocumentUrl('{0}','{1}','');", JavascriptUtil.ConvertForJavascript(docInfo.URI), docInfo.AuthenticationType)
                           : string.Format("window.open('DownloadFile.aspx?viewdocfile={0}&origfilename={1}');", HttpUtility.UrlEncode(docInfo.FileName), HttpUtility.UrlEncode(docInfo.URI));
                    }

                    ScriptManager.RegisterStartupScript(this, Page.GetType(), "opendocument", src, true);
                    CamstarWebControl.SetRenderToClient(StoredFileNameField);
                }
            }
        }

        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                try
                {
                    if ((BrowseModeEnum)BrowseMode.Data == BrowseModeEnum.LocalFile)
                    {
                        var storedFileNameFieldValue = (string)StoredFileNameField.Data;
                        var uploadDirectory = CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;

                        if (IO.Directory.Exists(uploadDirectory))
                        {
                            var uploadDirectoryInfo = new IO.DirectoryInfo(uploadDirectory);
                            string originalFilePath = IO.Path.Combine(uploadDirectoryInfo.FullName, storedFileNameFieldValue);
                            string errorMsg = string.Empty;
                            if (!FileMgmtUtil.DeleteFileFromFileShare(GetFileName(originalFilePath), out errorMsg))
                            {
                                Page.StatusBar.WriteError(errorMsg);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Page.StatusBar.WriteError(ex.Message);
                }
            }
        }

        protected virtual void BrowseMode_DataChanged(object sender, EventArgs e)
        {
            if (BrowseMode.Data != null)
            {
                bool isLocalFile = ((BrowseModeEnum)BrowseMode.Data == BrowseModeEnum.LocalFile);
                bool isHttpFile = ((BrowseModeEnum)BrowseMode.Data == BrowseModeEnum.HTTPFile);
                bool isUrl = ((BrowseModeEnum)BrowseMode.Data == BrowseModeEnum.Url);
                //set visible the 
                AuthenticationType.Visible = isHttpFile;
                BrowseFileField.Visible = isHttpFile;
                UploadFileField.Visible = isLocalFile;
                UrlField.Visible = isUrl;
                StoredFileNameField.Visible = isLocalFile || isHttpFile;
                FileVersionField.Visible = isLocalFile || isHttpFile;
                FileVersionField.ReadOnly = isHttpFile;
            }
        }
        #endregion

        #region Protected methods
        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/DocumentMaint.js");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                BrowseMode.TextEditControl.Enabled = false;
            }
            DocumentActionsButton.Attributes.Add("actionType", "SubmitAction");
            DocumentActionsButton.Click += DocumentActionsButton_Click;

            BrowseMode.DataChanged += BrowseMode_DataChanged;
        }

        
        #endregion

        #region Private Methods

        private void SetBrowseMode(DocumentChanges changes)
        {
            if (BrowseMode.IsEmpty)
            {
                BrowseMode.Data = GetBrowseMode(changes);
            }
            else
            {
                return;
            }

        }

        private BrowseModeEnum GetBrowseMode(DocumentChanges changes)
        {
            var fileNameIsEmpty = string.IsNullOrEmpty((string)changes.FileName);

            if (fileNameIsEmpty || IdentifierContainsNetUrlScheme(changes))
            {
                return BrowseModeEnum.HTTPFile;
            }
            else
            {
                return BrowseModeEnum.LocalFile;
            }
        }

        private void GetLocalFileData(DocumentChanges changes)
        {

            var filePath = UploadFileField.Data as string;
            var prevIdentifier = StoredFileNameField.Data as string;
            StoredFileNameField.Data = GetFileName(filePath);

            var formMode = DefineFormMode();

            SetUploadToDB(formMode);

            if (ShouldUploadToDB())
            {
                SetAttachedFileProperties(changes, filePath);
            }
            else
            {
                if (changes != null)
                {
                    if (ShouldLeaveFieldsUnchanged(formMode))
                    {
                        changes.UploadFile = false;
                        changes.Identifier = filePath;
                    }
                    else
                    {
                        changes.Identifier = prevIdentifier;
                    }
                    NullifyAttachedFileProperties(changes);
                }
            }

            BrowseFileField.ClearData();
            UrlField.ClearData();
        }

        private void SetAttachedFileProperties(DocumentChanges changes, string filePath)
        {
            if (changes == null) return;

            var storedFileNameFieldValue = (string)StoredFileNameField.Data;
            var uploadDirectory = FileInput.GetUploadFolder();// CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;

            if (IO.Directory.Exists(uploadDirectory))
            {
                var uploadDirectoryInfo = new IO.DirectoryInfo(uploadDirectory);
                string originalFilePath = IO.Path.Combine(uploadDirectoryInfo.FullName, storedFileNameFieldValue);
                string uniqueFilePath = String.Empty;

                if (TryUploadFileToFileShare(originalFilePath, out uniqueFilePath))
                {
                    changes.FileLocation = uploadDirectoryInfo.FullName;
                    changes.FileName = GetFileName(uniqueFilePath);
                }
            }
            changes.UploadFile = true;
            changes.Identifier = filePath;
        }

        private bool TryUploadFileToFileShare(string originalFilePath, out string outPath)
        {
            if (!originalFilePath.StartsWith(CamstarPortalSection.Settings.DefaultSettings.UploadDirectory))
            {
                string fileShareFileName = GetUniqueFileName(originalFilePath);
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session).CurrentUserProfile;

                string errorMsg = string.Empty;

                if (!FileMgmtUtil.UploadFileToFileShare(originalFilePath, HttpUtility.UrlEncode(fileShareFileName), profile, GetUserDomain(), out outPath, out errorMsg))
                {
                    Page.StatusBar.WriteError(errorMsg);
                    return false;
                }
                /*
                if (!FileMgmtUtil.DeleteFileFromFileShare(GetFileName(originalFilePath), out errorMsg))
                {
                    Page.StatusBar.WriteError(errorMsg);
                }
                */
            }
            else
                outPath = originalFilePath;
            return true;
        }

        private bool ShouldLeaveFieldsUnchanged(FormMode formMode)
        {
            return (formMode == FormMode.ReportTemplate
                        && !UploadFileField.IsEmpty
                        && UploadToDB.CheckControl.Checked);
        }

        private bool ShouldUploadToDB()
        {
            return (!UploadFileField.IsEmpty && UploadToDB.CheckControl.Checked);
        }

        private void SetUploadToDB(FormMode formMode)
        {
            if (formMode == FormMode.Document && !UploadFileField.IsEmpty)
            {
                UploadToDB.CheckControl.Checked = true;
            }
        }

        private void NullifyAttachedFileProperties(DocumentChanges changes)
        {
            changes.FileLocation = null;
            changes.UploadFile = null;
        }

        private string GetUniqueFileName(string originalFilePath)
        {
            return FileMgmtUtil.GenerateUniqueFileName(originalFilePath, GetUserProfile().Name);
        }

        private OM.UserProfile GetUserProfile()
        {
            return FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session).CurrentUserProfile;
        }

        private string GetUserDomain()
        {
            string domainName = string.Empty;
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

            if (!string.IsNullOrEmpty(session.SessionValues.UserDomain))
            {
                domainName = session.SessionValues.UserDomain;
            }
            return domainName;
        }  

        private void GetHTTPData(DocumentChanges changes)
        {
            changes.Identifier = BrowseFileField.Data == null ? string.Empty : (string)BrowseFileField.Data;
            changes.FileLocation = string.Empty;
            changes.FileName = string.Empty;
            changes.FileVersion = (string)FileVersionField.Data;

            StoredFileNameField.ClearData();
            UrlField.ClearData();
        }

        private void GetUrlData(DocumentChanges changes)
        {
            changes.Identifier = UrlField.Data == null ? string.Empty : (string)UrlField.Data;
            changes.FileLocation = string.Empty;
            changes.FileName = string.Empty;
            changes.FileVersion = (string)FileVersionField.Data;

            StoredFileNameField.ClearData();
            BrowseFileField.ClearData();
        }

        private void ShowFieldsForMode(BrowseModeEnum mode)
        {
            HideAllFields();

            switch (mode)
            {
                default:
                case BrowseModeEnum.LocalFile:
                    UploadFileField.Visible = true;
                    FileVersionField.ReadOnly = false;

                    StoredFileNameField.Visible = true;
                    FileVersionField.Visible = true;
                    DocumentActionsButton.Visible = true;
                    NoteLabel.Visible = true;
                    break;

                case BrowseModeEnum.HTTPFile:
                    BrowseFileField.Visible = true;
                    AuthenticationType.Visible = true;

                    StoredFileNameField.Visible = true;
                    FileVersionField.Visible = true;
                    DocumentActionsButton.Visible = true;
                    NoteLabel.Visible = true;
                    break;

                case BrowseModeEnum.Url:
                    UrlField.Visible = true;
                    break;
            }
        }

        private void HideAllFields()
        {
            UploadFileField.Visible = false;
            FileVersionField.ReadOnly = true;

            BrowseFileField.Visible = false;
            AuthenticationType.Visible = false;

            UrlField.Visible = false;

            StoredFileNameField.Visible = false;
            FileVersionField.Visible = false;
            DocumentActionsButton.Visible = false;
            NoteLabel.Visible = false;
        }

        private void DisplayDataForMode(BrowseModeEnum mode, string identifier)
        {
            BrowseFileField.ClearData();
            UploadFileField.ClearData();
            UrlField.ClearData();

            switch (mode)
            {
                default:
                case BrowseModeEnum.LocalFile:
                    if(!string.IsNullOrEmpty(identifier))
                        StoredFileNameField.Data = GetFileName(identifier);
                    UploadFileField.Data = identifier;
                    BrowseFileField.ClearData();
                    break;

                case BrowseModeEnum.HTTPFile:
                    StoredFileNameField.ClearData();
                    BrowseFileField.Text = identifier;
                    break;

                case BrowseModeEnum.Url:
                    StoredFileNameField.ClearData();
                    UrlField.Data = identifier;
                    break;
            }

            DocumentActionsButton.Enabled = !(BrowseFileField.IsEmpty && StoredFileNameField.IsEmpty);
        }

        private string GetFileName(string path, bool showError = false)
        {
            try
            {
                var fileInfo = new IO.FileInfo(path);
                return fileInfo.Name;
            }
            catch (Exception ex)
            {
                if (showError)
                {
                    Page.StatusBar.WriteError(ex.Message);
                }
                return null;
            }
        }

        private bool IsValidServiceType(Service service)
        {
            var serviceType = service.GetType();

            var isValidServiceType =
                serviceType == typeof(DocumentMaint)
                || serviceType == typeof(RecipeMaint)
                || serviceType == typeof(ReportTemplateMaint);

            return isValidServiceType;
        }

        private bool IdentifierContainsNetUrlScheme(DocumentChanges changes)
        {
            var identifier = (string)changes.Identifier;
            var possibleUrlSchemes = new[] { "http://", "https://", "ftp://", " \\" };
            return possibleUrlSchemes.Any(s => (identifier).StartsWith(s, StringComparison.InvariantCultureIgnoreCase));
        }

        private FormMode DefineFormMode()
        {
            if (PrimaryServiceType.Equals("DocumentMaint"))
            {
                BrowseFileField.FileMask = "*.*";
                UploadToDB.Visible = false;

                return FormMode.Document;
            }
            else
            {
                BrowseFileField.FileMask = "*.rpt";
                UploadToDB.Visible = true;

                UploadFileField.Data = 0;
                BrowseMode.Visible = false;

                return FormMode.ReportTemplate;
            }
        }

        private DocumentRefInfo DownloadDocument()
        {
            LabelCache labelCache = LabelCache.GetRuntimeCacheInstance();
            var label = labelCache.GetLabelByName("Lbl_SharedFolderDoesntExists");
            var configFolder = CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;
            var message = label != null
                ? label.Value
                : string.Format("Shared folder '{0}' does not exist.", configFolder);

            var documentRev = GetRevObjectRef();
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

            ResultStatus resultStatus;
            var docInfo = AttachmentExecutor.DownloadDocumentRef(documentRev, session.CurrentUserProfile, message, out resultStatus);

            if (!resultStatus.IsSuccess)
            {
                Page.DisplayMessage(resultStatus);
                docInfo = null;
            }

            return docInfo;
        }

        private RevisionedObjectRef GetRevObjectRef()
        {
            var instanceHeaderWp = Page.Manager.WebParts["MDL_InstanceHeader"];

            if (instanceHeaderWp != null)
            {
                var nameTxt = instanceHeaderWp.FindControl("NameTxt") as CWC.TextBox;
                var revTxt = instanceHeaderWp.FindControl("RevisionTxt") as CWC.TextBox;
                var isROR = instanceHeaderWp.FindControl("IsRORChk") as CWC.CheckBox;

                if (nameTxt != null && revTxt != null && isROR != null)
                {
                    var name = nameTxt.Data == null ? "" : nameTxt.Data.ToString();
                    var rev = revTxt.Data == null ? "" : revTxt.Data.ToString();
                    bool useROR = isROR.Data == null ? false : (bool)isROR.Data;

                    return WSObjectRef.AssignRevisionedObject(name, rev, useROR, "Document");
                }
            }

            return null;
        }

        #endregion

        #region Controls

        protected virtual CWC.TextBox DocumentName
        {
            get { return Page.FindCamstarControl("ObjectChanges_ECO") as CWC.TextBox; }
        }

        protected virtual CWC.DropDownList BrowseMode
        {
            get { return Page.FindCamstarControl("BrowseMode") as CWC.DropDownList; }
        }

        protected virtual CWC.FileInput UploadFileField
        {
            get { return Page.FindCamstarControl("LocalFileInput") as CWC.FileInput; }
        }

        protected virtual CWC.FileBrowse BrowseFileField
        {
            get { return Page.FindCamstarControl("HTTPFileInput") as CWC.FileBrowse; }
        }

        protected virtual CWC.TextBox UrlField
        {
            get { return Page.FindCamstarControl("UrlInput") as CWC.TextBox; }
        }

        protected virtual CWC.Label NoteLabel
        {
            get { return Page.FindCamstarControl("Control") as CWC.Label; }
        }

        protected virtual CWC.DropDownList AuthenticationType
        {
            get { return Page.FindCamstarControl("AuthenticationType") as CWC.DropDownList; }
        }

        protected virtual CWC.Button DocumentActionsButton
        {
            get { return Page.FindCamstarControl("DocumentActionButton") as CWC.Button; }
        }

        protected virtual CWC.CheckBox UploadToDB
        {
            get { return Page.FindCamstarControl("ObjectChanges_UploadFile") as CWC.CheckBox; }
        }

        protected virtual CWC.TextBox StoredFileNameField
        {
            get { return Page.FindCamstarControl("ObjectChanges_FileName") as CWC.TextBox; }
        }

        protected virtual CWC.TextBox FileVersionField
        {
            get { return Page.FindCamstarControl("ObjectChanges_FileVersion") as CWC.TextBox; }
        }

        protected virtual CWC.Label CantFindDocumentLabel
        {
            get { return Page.FindCamstarControl("CantFindDocumentMsg") as CWC.Label; }
        }

        protected virtual CWC.Label EmptyDocumentURLLabel
        {
            get { return Page.FindCamstarControl("EmptyURLMsg") as CWC.Label; }
        }

        #endregion

        private enum FormMode
        {
            Document,
            ReportTemplate
        }
    }
}
