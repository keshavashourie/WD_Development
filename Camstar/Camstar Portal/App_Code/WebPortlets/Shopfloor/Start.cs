// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using MDL = Camstar.WebPortal.WebPortlets.Modeling;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Personalization;
using CamstarPortal.WebControls;
using Camstar.WCF.Services;
using System.Collections;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WCF.ObjectStack;
using System.IO;
using OS = Camstar.WCF.ObjectStack;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;
using Camstar.WebPortal.FormsFramework.HtmlControls;


namespace Camstar.WebPortal.WebPortlets.Shopfloor
{

    /// <summary>
    /// This Web Part is code-behind for the AttachDocument Web Part located on the Start Txn/VP.
    /// </summary>

    public class StartAttachDocument : MatrixWebPart
    {
        #region Controls

        protected virtual CWC.FileInput UploadField
        {
            get { return Page.FindCamstarControl("DocumentPath") as CWC.FileInput; }
        } // UploadField 

        protected virtual CWC.TextBox DocRevision
        {
            get { return Page.FindCamstarControl("AttachDocument_DocumentRevision") as CWC.TextBox; }
        } // DocRevision

        protected virtual CWC.TextBox DocName
        {
            get { return Page.FindCamstarControl("AttachDocument_DocumentName") as CWC.TextBox; }
        } // DocName

        protected virtual CWC.TextBox DocDescription
        {
            get { return Page.FindCamstarControl("AttachDocument_DocumentDescription") as CWC.TextBox; }
        } // DocDescription

        protected virtual CWC.RadioButton RadioBtn_NewDocNOReuse
        {
            get { return Page.FindCamstarControl("RadioBtn_AttachDocument_NewDocumentNOReuse") as CWC.RadioButton; }
        } // RadioBtn_NewDocNOReuse

        protected virtual CWC.RadioButton RadioBtn_NewDocReuse
        {
            get { return Page.FindCamstarControl("RadioBtn_AttachDocument_NewDocumentReuse") as CWC.RadioButton; }
        } // RadioBtn_NewDocReuse

        protected virtual CWC.RadioButton RadioBtn_ExistingDoc
        {
            get { return Page.FindCamstarControl("RadioBtn_AttachDocument_Existing") as CWC.RadioButton; }
        } // RadioBtn_ExistingDoc

        protected virtual CWC.DropDownList AttachmentType
        {
            get { return Page.FindCamstarControl("AttachDocument_AttachmentType") as CWC.DropDownList; }
        } // AttachmentType

        protected virtual CWC.TextBox StoredFileNameField
        {
            get { return Page.FindCamstarControl("AttachDocument_AttachedFileName") as CWC.TextBox; }
        } // StoredFileNameField

        protected virtual CWC.CheckBox IsContainerField
        {
            get { return Page.FindCamstarControl("AttachDocument_IsContainer") as CWC.CheckBox; }
        } // IsContainerField


        protected virtual CWC.RevisionedObject DocumentInstanceField
        {
            get { return Page.FindCamstarControl("AttachDocument_DocumentInstance") as CWC.RevisionedObject; }
        } // DocumentInstanceField

        protected virtual CWC.Button ViewDocumentButton
        {
            get { return Page.FindCamstarControl("ViewDocumentButton") as CWC.Button; }
        } //DocumentActionsButton

        protected virtual CWC.CheckBox AttachAsRORField
        {
            get { return Page.FindCamstarControl("AttachDocument_AttachAsROR") as CWC.CheckBox; }
        } // AttachAsRORField

        protected virtual CWC.TextBox ContainerName
        {
            get { return Page.FindCamstarControl("Details_ContainerName") as CWC.TextBox; }
        }

        protected virtual CWC.NamedObject NumberingRule
        {
            get { return Page.FindCamstarControl("Details_AutoNumberRule") as CWC.NamedObject; }
        }

        protected virtual CWC.CheckBox AutoNumber
        {
            get { return Page.FindCamstarControl("Details_GenerateName") as CWC.CheckBox; }
        }

        #endregion

        #region Protected Functions

        /// <summary>
        /// This function calls the newly created objects for use OnLoad
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // For responsive mode check box is moved to the ContainerName cell 
            if (IsResponsive)
            {
                var AutoNumberCtl = AutoNumber ?? Page.FindCamstarControl("Details_AutoNumber") as CWC.CheckBox ;
                var anCell = AutoNumberCtl.Parent;
                var i = anCell.Controls.IndexOf(AutoNumberCtl);
                if (i != -1)
                {
                    var ctl = anCell.Controls[i];
                    anCell.Controls.RemoveAt(i);
                    ContainerName.Parent.Controls.Add(ctl);
                }
            }
            RadioBtn_NewDocNOReuse.DataChanged += new EventHandler(RadioBtn_NewDocNOReuse_DataChanged);
            RadioBtn_ExistingDoc.DataChanged += new EventHandler(RadioBtn_ExistingDoc_DataChanged);
            RadioBtn_NewDocReuse.DataChanged += new EventHandler(RadioBtn_NewDocReuse_DataChanged);
            ViewDocumentButton.Click += new EventHandler(ViewDocumentButton_Click);

        }

        public override bool PreExecute(Info serviceInfo, Service serviceData)
        {
            bool result = base.PreExecute(serviceInfo, serviceData);

            if (NumberingRule != null && NumberingRule.Data == null && AutoNumber != null && AutoNumber.IsChecked)
            {
                result = false;
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                Page.DisplayWarning(labelCache.GetLabelByName("Start_NoNumberingRuleDefined").Value);
            }

            return result;
        }

        #endregion

        #region Public Functions

        protected virtual void ViewDocumentButton_Click(object sender, EventArgs e)
        {
            var docInfo = DownloadDocument();
            if (docInfo != null && !string.IsNullOrEmpty(docInfo.FileName))
            {
                var src = docInfo.IsRemote
                    ? string.Format("OpenDocumentUrl('{0}','{1}','');", JavascriptUtil.ConvertForJavascript(docInfo.URI), docInfo.AuthenticationType)
                    : string.Format("window.open('DownloadFile.aspx?viewdocfile={0}&origfilename={1}');", HttpUtility.UrlEncode(docInfo.FileName), HttpUtility.UrlEncode(docInfo.URI));
                StoredFileNameField.Data = docInfo.FileName;
                ScriptManager.RegisterStartupScript(this, Page.GetType(), "opendocument", src, true);
                StoredFileNameField.ClearData();
            }
        }


        protected virtual void RadioBtn_NewDocReuse_DataChanged(object sender, EventArgs e)
        {
            if (RadioBtn_NewDocReuse.RadioControl.Checked)
            {
                RadioBtn_NewDocNOReuse.RadioControl.Checked = false;
                RadioBtn_ExistingDoc.RadioControl.Checked = false;
                AttachmentType.ClearData();
                AttachmentType.Data = (int) AttachmentTypeEnum.NewDocumentReuse;
                AttachAsRORField.Visible = true;
                DocRevision.Visible = true;
                UploadField.Visible = true;
                DocName.Visible = true;
                DocDescription.Visible = true;
                DocumentInstanceField.ClearData();
                DocumentInstanceField.Visible = false;
                StoredFileNameField.ClearData();
                ViewDocumentButton.Enabled = false;
                ViewDocumentButton.Visible = false;

            }

        }


        protected virtual void RadioBtn_ExistingDoc_DataChanged(object sender, EventArgs e)
        {
            if (RadioBtn_ExistingDoc.RadioControl.Checked)
            {
                RadioBtn_NewDocNOReuse.RadioControl.Checked = false;
                RadioBtn_NewDocReuse.RadioControl.Checked = false;
                AttachmentType.ClearData();
                AttachmentType.Data = (int) AttachmentTypeEnum.ExistingDocument;
                AttachAsRORField.ClearData();
                AttachAsRORField.Visible = false;
                DocRevision.ClearData();
                DocRevision.Visible = false;
                UploadField.Visible = false;
                DocName.ClearData();
                DocName.Visible = false;
                DocDescription.ClearData();
                DocDescription.Visible = false;
                DocumentInstanceField.ClearData();

                StoredFileNameField.ClearData();

            }
        }


        protected virtual void RadioBtn_NewDocNOReuse_DataChanged(object sender, EventArgs e)
        {
            if (RadioBtn_NewDocNOReuse.RadioControl.Checked)
            {

                RadioBtn_NewDocReuse.RadioControl.Checked = false;
                RadioBtn_ExistingDoc.RadioControl.Checked = false;
                AttachmentType.ClearData();
                AttachmentType.Data = (int) AttachmentTypeEnum.NewDocumentNOReuse;
                AttachAsRORField.ClearData();
                AttachAsRORField.Visible = false;
                DocRevision.ClearData();
                DocRevision.Visible = false;
                UploadField.Visible = true;
                DocName.Visible = true;
                DocDescription.Visible = true;
                DocumentInstanceField.ClearData();
                DocumentInstanceField.Visible = false;
                StoredFileNameField.ClearData();
                ViewDocumentButton.Enabled = false;
                ViewDocumentButton.Visible = false;

            }
        }


        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            if (Page.ProcessingContext.Status != ProcessingStatusType.SubmitTransaction)
            {
                return;
            }

            if (!UploadField.IsEmpty)
            {
                StoredFileNameField.Data = new FileInfo((string) UploadField.Data).Name;

                string uploadDirectory = FileInput.GetUploadFolder();// CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;

                if (Directory.Exists(uploadDirectory))
                {
                    _folder = new DirectoryInfo(uploadDirectory);
                }

                string fileName = (string) StoredFileNameField.Data;
                //string actualSavedFile = HttpContext.Current.Session["PostedUniqueFileName"] as string;
                //if (!string.IsNullOrEmpty(actualSavedFile))
                //    fileName = actualSavedFile;

                if (_folder != null)
                {
                    ((OS.Start) serviceData).Details.FilePath = _folder.FullName; //File Location

                    _file = new FileInfo(_folder.FullName + "\\" + fileName);
                    string uniqueFilePath = String.Empty;

                    if (TryUploadFileToFileShare(_file.FullName, out uniqueFilePath))
                    {
                        ((OS.Start)serviceData).Details.AttachedFileName = GetFileName(uniqueFilePath);
                        ((OS.Start)serviceData).Details.AttachedFileExtension = _file.Extension;
                    }
                }
                ((OS.Start) serviceData).Details.Identifier = (string) UploadField.Data;
            }
        }

        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);

            if (status.IsSuccess)
            {
                ClearObjects();
                FileInput.DeleteUniqueFolder();
            }
        }

        /// Resets the page value to default 
        /// </summary> 
        /// <param name="sender"></param> 
        /// <param name="e"></param> 
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action.Parameters == "Reset")
            {
                base.ClearValues();

                RadioBtn_NewDocNOReuse.RadioControl.Checked = false;
                RadioBtn_NewDocReuse.RadioControl.Checked = false;
                RadioBtn_ExistingDoc.RadioControl.Checked = false;
                DocName.ClearData();
                DocRevision.ClearData();
                StoredFileNameField.ClearData();
                DocumentInstanceField.ClearData();
                ViewDocumentButton.Enabled = false;
                ViewDocumentButton.Visible = false;
                UploadField.ClearData();

                StoredFileNameField.ClearData();
                ClearObjects();
                Page.ShopfloorReset(sender, e as CustomActionEventArgs);

            }

        }



        #endregion

        #region Private Functions

        protected virtual void ClearObjects()
        {
            RadioBtn_NewDocReuse.RadioControl.Checked = false;
            RadioBtn_ExistingDoc.RadioControl.Checked = false;
            RadioBtn_NewDocNOReuse.RadioControl.Checked = false;
            AttachAsRORField.Visible = false;
            AttachAsRORField.ClearData();
            AttachmentType.ClearData();
            DocRevision.ClearData();
            DocRevision.Visible = false;
            UploadField.Visible = false;
            DocName.ClearData();
            DocName.Visible = false;
            DocDescription.ClearData();
            DocDescription.Visible = false;
            DocumentInstanceField.ClearData();
            DocumentInstanceField.Visible = false;
            StoredFileNameField.ClearData();
        }


        protected virtual DocumentRefInfo DownloadDocument()
        {
            ResultStatus resultStatus;
            var documentRev = GetRevObjectRef();

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
        
        protected virtual RevisionedObjectRef GetRevObjectRef()
        {
            var nameTxt = ((RevisionedObjectRef) (DocumentInstanceField.Data)).Name;
            var revTxt = ((RevisionedObjectRef) (DocumentInstanceField.Data)).Revision;
            var isROR = ((RevisionedObjectRef) (DocumentInstanceField.Data)).RevisionOfRecord;
            if (!string.IsNullOrEmpty(nameTxt))
            {
                var name = nameTxt == null ? "" : nameTxt.ToString();
                var rev = revTxt == null ? "" : revTxt.ToString();
                bool useROR = isROR == null ? false : (bool) isROR;
                return WSObjectRef.AssignRevisionedObject(name, rev, useROR, "Document");
            }
            return null;
        }

        #endregion

        #region private Methods

        private bool TryUploadFileToFileShare(string originalFilePath, out string outPath)
        {
            if (!originalFilePath.StartsWith(CamstarPortalSection.Settings.DefaultSettings.UploadDirectory))
            {
                string fileShareFileName = GetUserProfile().Name + GetFileName(originalFilePath);// GetUniqueFileName(originalFilePath);

                //string fileShareFileName = GetUniqueFileName(originalFilePath);
                OS.UserProfile profile = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session).CurrentUserProfile;

                string errorMsg = string.Empty;

                if (!FileMgmtUtil.UploadFileToFileShare(originalFilePath, fileShareFileName, profile, GetUserDomain(), out outPath, out errorMsg))
                {
                    Page.StatusBar.WriteError(errorMsg);
                    return false;
                }

                if (!FileMgmtUtil.DeleteFileFromFileShare(GetFileName(originalFilePath), out errorMsg))
                {
                    Page.StatusBar.WriteError(errorMsg);
                }
            }
            else
                outPath = originalFilePath;
            return true;
        }

        private string GetUniqueFileName(string originalFilePath)
        {
            return FileMgmtUtil.GenerateUniqueFileName(originalFilePath, GetUserProfile().Name);
        }

        private OS.UserProfile GetUserProfile()
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

        private string GetFileName(string path, bool showError = false)
        {
            try
            {
                var fileInfo = new System.IO.FileInfo(path);
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
        #endregion

        #region Constants

        #endregion

        #region Private Member Variables


        private DirectoryInfo _folder = null;
        private FileInfo _file = null;

        #endregion

    }

}

