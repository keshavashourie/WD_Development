// Copyright Siemens 2019  
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.Personalization;



/// <summary>
/// Summary description for Document
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class isImageAttachmentExecutor
    {
        public static OM.ResultStatus ViewAttachment(OM.DocAttachmentsTxn data, OM.UserProfile profile, out OM.DocAttachmentsTxn txnOutput)
        {
            txnOutput = null;
            DocAttachmentsTxn_Result txnResult = null;
            PopulateTxnDetails(ref data, OM.DocAttachmentActionEnum.Download);

            OM.ResultStatus result = ExecuteAttachmentTxn(data, OM.DocAttachmentActionEnum.Download, profile, out txnResult);

            if (result.IsSuccess)
            {
                if (txnResult != null)
                {
                    if (txnResult.Value != null && txnResult.Value is OM.DocAttachmentsTxn)
                    {
                        txnOutput = txnResult.Value;
                    }//if
                }//if
            }//if result
            return result;
        }//ViewAttachment

        private static OM.ResultStatus ExecuteAttachmentTxn(OM.DocAttachmentsTxn data, OM.DocAttachmentActionEnum action, OM.UserProfile profile)
        {
            DocAttachmentsTxn_Result txnResult = null;
            return ExecuteAttachmentTxn(data, action, profile, out txnResult);
        }//ExecuteAttachmentTxn

        private static OM.ResultStatus ExecuteAttachmentTxn(OM.DocAttachmentsTxn data, OM.DocAttachmentActionEnum action, OM.UserProfile profile, out DocAttachmentsTxn_Result txnResult)
        {
            txnResult = null;
            OM.ResultStatus executionResult = null;
            DocAttachmentsTxn_Request request = new DocAttachmentsTxn_Request();

            DocAttachmentsTxnService serv = new DocAttachmentsTxnService(profile);

            request.Info = CreateDocAttachmentInfo(action);

            executionResult = serv.ExecuteTransaction(data, request, out txnResult);
            return executionResult;
        }//ExecuteAttachmentTxn

        private static OM.DocAttachmentsTxn_Info CreateDocAttachmentInfo(OM.DocAttachmentActionEnum action)
        {
            OM.DocAttachmentsTxn_Info info = new OM.DocAttachmentsTxn_Info();
            if (action == Camstar.WCF.ObjectStack.DocAttachmentActionEnum.Download)
            {
                info.DocDetail = new OM.AttachedDocDetail_Info();
                info.DocDetail.File = new OM.Info();
                info.DocDetail.File.RequestValue = true;

                info.DocDetail.OriginalFileName = new OM.Info();
                info.DocDetail.OriginalFileName.RequestValue = true;
            }//if

            if (action != Camstar.WCF.ObjectStack.DocAttachmentActionEnum.Download)
            {
                info.Attachments = new Camstar.WCF.ObjectStack.DocAttachments_Info();
                info.Attachments.Documents = new Camstar.WCF.ObjectStack.AttachedDoc_Info();
                info.Attachments.Documents.RequestValue = true;
            }
            return info;
        }//CreateDocAttachmentInfo

        private static void PopulateTxnDetails(ref OM.DocAttachmentsTxn data, OM.DocAttachmentActionEnum docAction)
        {
            data.DocDetail.Action = docAction;

            if (docAction == OM.DocAttachmentActionEnum.Download)
            {
                string folder = new DirectoryInfo(FileMgmtUtil.UploadFolder).FullName;
                if (!folder.EndsWith("\\"))
                    folder += "\\";
                data.AttachmentsLocation = folder;
            }//if docAction
        }//PopulateTxnDetails

        public static DocumentRefInfo GetDocumentInfo(string docName, string docRev, OM.UserProfile profile, out OM.ResultStatus resultStatus)
        {
            var retVal = new DocumentRefInfo();
            var documentRef = new OM.RevisionedObjectRef(docName, docRev);
            if (string.IsNullOrEmpty(docRev))
                documentRef.RevisionOfRecord = true;
            retVal.DocumentRef = documentRef;
            resultStatus = new OM.ResultStatus();
            if (profile != null)
            {
                var data = new OM.isImageMaint { ObjectToChange = documentRef };
                var request = new isImageMaint_Request
                {
                    Info = new OM.isImageMaint_Info
                    {
                        ObjectChanges = new OM.isImageChanges_Info
                        {
                            Identifier = new OM.Info(true),
                            AuthenticationType = new OM.Info(true),
                            BrowseMode = new OM.Info(true)
                        }
                    }
                };

                isImageMaint_Result result;
                var docMaintService = new isImageMaintService(profile);
                resultStatus = docMaintService.Load(data, request, out result);
                if (resultStatus.IsSuccess)
                {
                    var authType = OM.AuthenticationTypeEnum.None;
                    if (result.Value.ObjectChanges.AuthenticationType != null)
                        authType = (OM.AuthenticationTypeEnum)result.Value.ObjectChanges.AuthenticationType;
                    retVal.URI = (string)result.Value.ObjectChanges.Identifier;
                    retVal.AuthenticationType = authType;
                    retVal.Credentials = GetDocCredentials(authType, profile);
                    if (result.Value.ObjectChanges.BrowseMode != null)
                        retVal.BrowseMode = (OM.BrowseModeEnum)result.Value.ObjectChanges.BrowseMode;
                }
            }

            return retVal;
        }

        public static DocumentRefInfo DownloadDocumentRef(OM.RevisionedObjectRef doc, OM.UserProfile profile, string sharedFolderDoesntExists, out OM.ResultStatus resultStatus)
        {
            var docMaintInfo = GetDocumentInfo(doc.Name, doc.Revision, profile, out resultStatus);

            if (resultStatus.IsSuccess)
            {
                if (!docMaintInfo.IsRemote)
                {
                    string configFolder = CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;
                    if (Directory.Exists(configFolder))
                    {
                        var serv = new isImageMaintService(profile);
                        var data = new OM.isImageMaint { ObjectToChange = doc };

                        var dirInfo = new DirectoryInfo(configFolder);
                        var data1 = new OM.isImageMaint
                        {
                            ObjectChanges = new OM.isImageChanges { FileLocation = dirInfo.FullName }
                        };
                        serv.BeginTransaction();
                        serv.Load(data);
                        serv.DownloadFile(data1);

                        var request = new isImageMaint_Request
                        {
                            Info = new OM.isImageMaint_Info
                            {
                                ObjectChanges = new OM.isImageChanges_Info
                                {
                                    FileName = new OM.Info(true),
                                    Identifier = new OM.Info(true),
                                    AuthenticationType = new OM.Info(true)
                                }
                            }
                        };

                        isImageMaint_Result result;
                        resultStatus = serv.CommitTransaction(request, out result);
                        if (resultStatus.IsSuccess && result != null)
                            docMaintInfo.FileName = result.Value.ObjectChanges.FileName != null
                                ? result.Value.ObjectChanges.FileName.Value
                                : null;
                    }
                    else
                    {
                        resultStatus = new OM.ResultStatus(sharedFolderDoesntExists, false);
                        if (string.IsNullOrEmpty(sharedFolderDoesntExists))
                            resultStatus.Message = "Shared Folder Doesnt Exists";
                    }
                }
            }

            return docMaintInfo;
        }

        public static string GetDocCredentials(OM.AuthenticationTypeEnum authenticationType, OM.UserProfile profile)
        {
            string credentials = string.Empty;
            if (authenticationType == OM.AuthenticationTypeEnum.Basic)
            {
                string userName = string.Empty;
                string userPwd = string.Empty;
                if (profile != null)
                {
                    userName = profile.Name;
                    if (HttpContext.Current != null)
                    {
                        var currentUserPassword = HttpContext.Current.Session[SessionConstants.UserPassword] as OM.EncryptedField;
                        userPwd = OM.EncryptedField.GetPlainValue(currentUserPassword);
                    }
                    var data = new OM.EmployeeMaint { ObjectToChange = new OM.NamedObjectRef(userName) };
                    var request = new EmployeeMaint_Request
                    {
                        Info = new OM.EmployeeMaint_Info
                        {
                            ObjectChanges = new OM.EmployeeChanges_Info
                            {
                                DocManagerUser = new OM.Info(true),
                                DocManagerPassword = new OM.Info(true)
                            }
                        }
                    };
                    var employeeMaintService = new EmployeeMaintService(profile);
                    EmployeeMaint_Result result;
                    var resultStatus = employeeMaintService.Load(data, request, out result);
                    if (resultStatus.IsSuccess)
                    {
                        if (result.Value.ObjectChanges.DocManagerUser != null)
                        {
                            if (!string.IsNullOrEmpty(result.Value.ObjectChanges.DocManagerUser.Value))
                                userName = result.Value.ObjectChanges.DocManagerUser.Value;

                            if (result.Value.ObjectChanges.DocManagerPassword != null)
                            {
                                if (!string.IsNullOrEmpty(result.Value.ObjectChanges.DocManagerPassword.Value))
                                {
                                    var encrypted =
                                        new OM.EncryptedField(result.Value.ObjectChanges.DocManagerPassword.Value, true);
                                    userPwd = OM.EncryptedField.GetPlainValue(encrypted);
                                }
                            }
                        }
                    }
                }
                var currentUserProfile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                string session = currentUserProfile.SessionID != null ? ":" + currentUserProfile.SessionID.Value : "";
                credentials = Convert.ToBase64String(Encoding.Default.GetBytes(string.Format("{0}:{1}{2}", userName, userPwd, session)));
            }
            return credentials;
        }
    }


    public class isImageMaint : MatrixWebPart
    {
        #region Public Methods
        public override void DisplayValues(Service serviceData)
        {
            base.DisplayValues(serviceData);

            var changes = (serviceData as DocumentMaint).ObjectChanges;
            var mode = DefineBrowseMode(changes);

            DefineFormMode();
            ShowFieldsForMode(mode);
            DisplayDataForMode(mode, (string)changes.Identifier);
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            DocumentChanges changes = (serviceData as DocumentMaint).ObjectChanges;
            var browseMode = DefineBrowseMode(changes);

            switch (browseMode)
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
                        src = string.Format("isImageMaint.openModelViewerPopup('{0}','{1}', '{2}');", JavascriptUtil.ConvertForJavascript(docInfo.DocumentRef.Name),
                                                                                 JavascriptUtil.ConvertForJavascript(docInfo.DocumentRef.Revision), "3D Model Popup");
                    }
                    else
                    {
                        src = docInfo.IsRemote
                            ? string.Format("OpenDocumentUrl('{0}','{1}','');", JavascriptUtil.ConvertForJavascript(docInfo.URI), docInfo.AuthenticationType)
                            : string.Format("window.open('DownloadFile.aspx?viewdocfile={0}');", HttpUtility.UrlEncode(docInfo.FileName));
                    }

                    ScriptManager.RegisterStartupScript(this, Page.GetType(), "opendocument", src, true);
                    CamstarWebControl.SetRenderToClient(StoredFileNameField);
                }
            }
        }
        #endregion

        #region Protected methods
        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/ImageMaint.js");
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
            BrowseFileField.DataChanged += BrowseFileField_DataChanged;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Not needed if implementing the 'Classic' page
            //LabelCache labelCache = LabelCache.GetRuntimeCacheInstance();
            //var label = labelCache.GetLabelByName("isInvalidImage");

            BrowseFileField.FileMask = GetAcceptString();
            string acceptScript = $"jQuery(document).ready(function(){{ $('#ctl00_WebPartManager_GeneralGroupWP_LocalFileInput_InputField').attr('accept','{GetAcceptString()}'); }})";
            string validateScript = $"isImageMaint.initializeModeling('{GetExtensionsString()}', '{GetInvalidImageMessage()}')";
            ScriptManager.RegisterStartupScript(Page.Form, Page.Form.GetType(), "imageMaintInitialize", $"{acceptScript}; {validateScript};", true);
        }

        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                FileInput.DeleteUniqueFolder();
            }
        }

        protected virtual void BrowseFileField_DataChanged(object sender, EventArgs e)
        {
            if (BrowseFileField.Data != null)
            {
                BrowseModeEnum mode = BrowseModeEnum.LocalFile;
                if (BrowseMode.Data != null)
                    mode = (BrowseModeEnum)BrowseMode.Data;
                if (mode != BrowseModeEnum.Url)
                {
                    string file = BrowseFileField.Data.ToString();
                    string extension = file.Substring(file.LastIndexOf('.') + 1);
                    if (!IsImageExtension(extension))
                    {
                        BrowseFileField.ClearData();
                        Page.StatusBar.WriteError(GetInvalidImageMessage());
                    }
                }
            }
        }
        #endregion

        #region Private Methods
        private string GetInvalidImageMessage()
        {
            LabelCache labelCache = LabelCache.GetRuntimeCacheInstance();
            var label = labelCache.GetLabelByName("isInvalidImage");
            return label.Value;
        }

        private BrowseModeEnum DefineBrowseMode(DocumentChanges changes)
        {
            if (BrowseMode.Data != null)
            {
                return (BrowseModeEnum)BrowseMode.Data;
            }

            var mode = BrowseModeEnum.LocalFile;

            var isFileNameExist = string.IsNullOrEmpty((string)changes.FileName);
            if (isFileNameExist || HasWebPath(changes))
            {
                mode = BrowseModeEnum.HTTPFile;
            }
            else
            {
                mode = BrowseModeEnum.LocalFile;
            }

            BrowseMode.Data = (int)mode;
            return mode;
        }

        private void GetLocalFileData(DocumentChanges changes)
        {
            var filePath = UploadFileField.Data as string;
            var prevIdentifier = StoredFileNameField.Data as string;
            StoredFileNameField.Data = GetFileName(filePath);

            var formMode = DefineFormMode();
            //TODO: what is intent of checking "formMode"? method always sets as Document.
            if (formMode == FormMode.Document && !UploadFileField.IsEmpty)
            {
                UploadToDB.CheckControl.Checked = true;
            }

            if (!UploadFileField.IsEmpty && UploadToDB.CheckControl.Checked)
            {
                var configFolder = FileInput.GetUploadFolder();// CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;
                if (Directory.Exists(configFolder))
                {
                    var storedFileName = (string)StoredFileNameField.Data;
                    var folder = new DirectoryInfo(configFolder);
                    changes.FileLocation = folder.FullName;
                    changes.FileName = GetFileName(folder.FullName + "\\" + storedFileName);
                }

                changes.UploadFile = true;
                changes.Identifier = filePath;
            }
            else
            {
                if (changes != null)
                {
                    var leaveFieldsUnchanged =
                        formMode == FormMode.ReportTemplate
                        && !UploadFileField.IsEmpty
                        && UploadToDB.CheckControl.Checked;

                    if (leaveFieldsUnchanged)
                    {
                        changes.UploadFile = false;
                        changes.Identifier = filePath;
                    }
                    else
                    {
                        changes.Identifier = prevIdentifier;
                    }

                    changes.FileLocation = null;
                    changes.UploadFile = null;
                }
            }

            BrowseFileField.ClearData();
            UrlField.ClearData();
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
                    if (!string.IsNullOrEmpty(identifier))
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

        private string GetFileExtension(string path)
        {
            var fileName = GetFileName(path, false);
            string extension = string.Empty;
            int idx = fileName.LastIndexOf(".");
            if (idx >= 0)
                extension = fileName.Substring(idx);
            return extension;
        }

        private string GetFileName(string path, bool showError = false)
        {
            try
            {
                var fileInfo = new FileInfo(path);
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

            var isValidServiceType = serviceType == typeof(isImageMaint);

            return isValidServiceType;
        }

        private bool HasWebPath(DocumentChanges changes)
        {
            var identifier = (string)changes.Identifier;
            var possibleUrlSchemes = new[] { "http://", "https://", "ftp://", " \\" };
            return possibleUrlSchemes.Any(s => (identifier).StartsWith(s, StringComparison.InvariantCultureIgnoreCase));
        }

        private FormMode DefineFormMode()
        {
            UploadToDB.Visible = false;
            //UploadFileField.Accept = "image/*";
            return FormMode.Document;
        }

        public static bool IsImageExtension(string ext)
        {
            return GetImageExtensions().Contains(ext.ToLower());
        }

        public static string[] GetImageExtensions()
        {
            string[] validExtensions = { "tif", "svg", "jpg", "jpeg", "bmp", "gif", "png", "tiff", "avif", "xmb", "ico", "pjp", "jfif", "webp", "pjpeg", "jt", "pdf" };
            return validExtensions;
        }

        public static string GetAcceptString()
        {
            return "." + String.Join(",.", GetImageExtensions()); ;
        }

        public static string GetExtensionsString()
        {
            return String.Join(",", GetImageExtensions());
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
            var docInfo = isImageAttachmentExecutor.DownloadDocumentRef(documentRev, session.CurrentUserProfile, message, out resultStatus);

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

                    return WSObjectRef.AssignRevisionedObject(name, rev, useROR, "isImage");
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
