using Camstar.WebPortal.WebPortlets;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WebPortal.WebPortlets.XceleratorShare;
using System.Threading.Tasks;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;
using System.IO;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Personalization;
using OS = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;

/// <summary>
/// Summary description for AttachToXceleratorShare
/// </summary>
namespace Camstar.WebPortal.WebPortlets.XceleratorShare
{
    public class AttachToXceleratorShare : MatrixWebPart
    {

        #region Properties

        protected virtual CWC.FileInput UploadField
        {
            get { return Page.FindCamstarControl("DocumentPath") as CWC.FileInput; }
        } // UploadField 

        protected virtual CWC.TextBox DocName
        {
            get { return Page.FindCamstarControl("AttachDocument_DocumentName") as CWC.TextBox; }
        } // DocName

        protected virtual CWC.TextBox DocDescription
        {
            get { return Page.FindCamstarControl("AttachDocument_DocumentDescription") as CWC.TextBox; }
        } // DocDescription

        protected virtual CWC.DropDownList XceleratorShareProjects_DropDown
        {
            get { return Page.FindCamstarControl("XceleratorShareProjects_DropDown") as CWC.DropDownList; }
        } // XceleratorShareProjects_DropDown

        protected virtual Personalization.UIAction SubmitAction
        {
            get { return Page.ActionDispatcher.PageActions().FirstOrDefault(x => x.Name.Equals("Submit")); }
        } // AttachBtn

        #endregion

        #region Private Member Variables

        private DirectoryInfo _folder = null;
        private FileInfo _file = null;
        private string _XShareNoActiveProjects = string.Empty;
        private string _XShareProjectMissing = string.Empty;
        private string _XShareFileMissing = string.Empty;
        private string _XShareDocNameMissing = string.Empty;
        private Attachment _XShareAttachment = null;

        #endregion

        #region Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            XceleratorShareProjects_DropDown.PickListPanelControl.PreProcessData += PickListPanelControl_PreProcessData;

            // Retrieving labels for xcelerator share error messages
            if (string.IsNullOrEmpty(_XShareNoActiveProjects))
            {
                _XShareNoActiveProjects = FrameworkManagerUtil.GetLabelValue("XShareNoActiveProjects") ?? string.Empty;
            }
            if (string.IsNullOrEmpty(_XShareProjectMissing))
            {
                _XShareProjectMissing = FrameworkManagerUtil.GetLabelValue("XShareProjectMissing") ?? string.Empty;
            }
            if (string.IsNullOrEmpty(_XShareFileMissing))
            {
                _XShareFileMissing = FrameworkManagerUtil.GetLabelValue("XShareFileMissing") ?? string.Empty;
            }
            if (string.IsNullOrEmpty(_XShareDocNameMissing))
            {
                _XShareDocNameMissing = FrameworkManagerUtil.GetLabelValue("XShareDocNameMissing") ?? string.Empty;
            }

        }

        private bool IsValidProject()
        {
            return XceleratorShareProjects_DropDown.Data != null ? XceleratorShareProjects_DropDown.Data.ToString().Length > 0 : false;
        }

        private bool IsValidDocumentName()
        {
            return DocName.Data != null ? DocName.Data.ToString().Trim().Length > 0 : false;
        }

        private bool IsValidDocumentPath()
        {
            return UploadField.Data != null ? UploadField.Data.ToString().Trim().Length > 0 : false;
        }

        public override bool PreExecute(Info serviceInfo, Service serviceData)
        {


            bool isValid = true;
            if (!IsValidProject())
            {
                Page.DisplayMessage(_XShareProjectMissing, false);
                isValid = false;
            }
            else if (!IsValidDocumentPath())
            {
                Page.DisplayMessage(_XShareFileMissing, false);
                isValid = false;
            }
            else if (!IsValidDocumentName())
            {
                Page.DisplayMessage(_XShareDocNameMissing, false);
                isValid = false;
            }
            try
            {
                if (isValid)
                {
                    string StoredFileName = new FileInfo((string)UploadField.Data).Name;
                    using (XceleratorShareDataManager xceleratorsharedatamanager = new XceleratorShareDataManager())
                    {
                        string collabspaceId = GetCollaborationSpaceId(xceleratorsharedatamanager);

                        _XShareAttachment = xceleratorsharedatamanager.UploadAttachment(Page.Request.Headers.Get(XceleratorShareDataManager.ACCESS_KEY),
                        Page.Request.Headers.Get(XceleratorShareDataManager.SECRET_ACCESS_KEY),
                        collabspaceId,
                        "project",
                        XceleratorShareProjects_DropDown.Data.ToString(),
                        _folder.FullName + "\\" + ((OS.AttachDocument)serviceData).AttachedFileName,
                        DocName.Data.ToString(),
                        DocDescription.Data == null ? "" : DocDescription.Data.ToString(),
                        XceleratorShareProjects_DropDown.Text
                        );
                        ((OS.XShareAttachDocument)serviceData).XShareCollabspaceId = _XShareAttachment.CollabsapceId;
                        ((OS.XShareAttachDocument)serviceData).XShareContainerId = _XShareAttachment.ContainerId;
                        ((OS.XShareAttachDocument)serviceData).XShareContainerType = _XShareAttachment.ContainerType;
                        ((OS.XShareAttachDocument)serviceData).XShareURN = _XShareAttachment.Urn;
                        ((OS.XShareAttachDocument)serviceData).XShareParentFolder = _XShareAttachment.ParentFolder;
                        ((OS.XShareAttachDocument)serviceData).XShareContainerName = _XShareAttachment.ContainerName;
                        ((OS.XShareAttachDocument)serviceData).Identifier = _XShareAttachment.Url;
                    }
                }
            }
            catch (Exception exception)
            {
                isValid = false;
                Page.DisplayMessage(exception.InnerException != null ? exception.InnerException.Message : exception.Message, false);
            }

            if (!isValid)
            {
                UploadField.ClearData();
                return false;
            }

            return base.PreExecute(serviceInfo, serviceData);
        }

        private void PickListPanelControl_PreProcessData(object sender, DataRequestEventArgs e)
        {
            var dp = (sender as CWC.PickLists.PickListPanel).DataProvider as CWC.PickLists.SelectionSourceBasedDataProvider;
            var recordSet = (dp.DataProvider as CWC.PickLists.StaticValuesDataProvider).RecordSet;
            recordSet.Clear();
            LoadProjectList(recordSet);
            dp.TotalCount = recordSet.Rows.Count;
            if (dp.TotalCount == 0)
            {
                dp.ResultStatus.IsSuccess = false;
                dp.ResultStatus.Message = _XShareNoActiveProjects;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            SubmitAction.ServiceName = "XShareAttachDocument";
        }

        private void LoadProjectList(System.Data.DataTable recordSet)
        {
            List<LCSProject> projects = new List<LCSProject>();
            string errorMessage = null;
            XceleratorShareDataManager xceleratorsharedatamanager = new XceleratorShareDataManager();

            try
            {
                string collabspaceId = GetCollaborationSpaceId(xceleratorsharedatamanager);
                projects = xceleratorsharedatamanager.GetProjects(
                        AuthManager.AccessKey,
                        AuthManager.SecretAccessKey,
                        collabspaceId);

            }
            catch (Exception exception)
            {
                errorMessage = exception.InnerException != null ? exception.InnerException.Message : exception.Message;
            }

            if (errorMessage != null)
            {
                Page.DisplayMessage(errorMessage, false);
            }
            else
            {
                foreach (var project in projects)
                {

                    recordSet.LoadDataRow(new object[] { project.Name, project.Id }, true);
                }
            }
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            string StoredFileName;

            if (Page.ProcessingContext.Status != ProcessingStatusType.SubmitTransaction)
            {
                return;
            }

            if (!UploadField.IsEmpty)
            {
                StoredFileName = new FileInfo((string)UploadField.Data).Name;

                string uploadDirectory = CWC.FileInput.GetUploadFolder();// CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;

                if (Directory.Exists(uploadDirectory))
                {
                    _folder = new DirectoryInfo(uploadDirectory);
                }
                string fileName = (string)StoredFileName;

                if (_folder != null)
                {
                    ((OS.AttachDocument)serviceData).FilePath = _folder.FullName; //File Location

                    _file = new FileInfo(_folder.FullName + "\\" + fileName);
                    string uniqueFilePath = String.Empty;

                    if (TryUploadFileToFileShare(_file.FullName, out uniqueFilePath))
                    {
                        ((OS.AttachDocument)serviceData).AttachedFileName = GetFileName(uniqueFilePath);
                        ((OS.AttachDocument)serviceData).AttachedFileExtension = _file.Extension;
                    }
                }
                ((OS.AttachDocument)serviceData).Identifier = (string)UploadField.Data;

                ((OS.XShareAttachDocument)serviceData).BrowseMode = BrowseModeEnum.HTTPFile;
                ((OS.XShareAttachDocument)serviceData).DocumentRevision = "1";
                ((OS.XShareAttachDocument)serviceData).AttachedFileExtension = _file?.Extension;
                ((OS.XShareAttachDocument)serviceData).AttachmentType = AttachmentTypeEnum.NewDocumentReuse;
                if (DocName.Data != null)
                {
                    ((OS.XShareAttachDocument)serviceData).DocumentName = DocName.Data.ToString();
                }

                //Bug 228449 : No Audit Trail Record Shows When Uploading a Document to Manage G/E or Manage P/E via Xshare Fix
                ((OS.XShareAttachDocument)serviceData).InstanceName = Page.DataContract.GetValueByName("QualityObject").ToString();
                ((OS.XShareAttachDocument)serviceData).IsNDO = true;
                ((OS.XShareAttachDocument)serviceData).ObjectTypeName = ((Camstar.WCF.ObjectStack.DCObject)Page.DataContract.GetValueByName("QualityObject")).CDOTypeName;
            }
        }

        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);

            if (status.IsSuccess)
            {
                ClearObjects();
            }
            else
            {
                UploadField.ClearData();
                if (_XShareAttachment != null)
                {
                    XceleratorShareDataManager xceleratorShareDataManager = new XceleratorShareDataManager();
                    xceleratorShareDataManager.DeleteAttachment(
                          AuthManager.AccessKey,
                          AuthManager.SecretAccessKey,
                         _XShareAttachment.CollabsapceId,
                         _XShareAttachment.ContainerType,
                         _XShareAttachment.ContainerId,
                         _XShareAttachment.Domain,
                         _XShareAttachment.Urn
                        );
                }
            }
        }

        #endregion

        #region functions
        protected virtual void ClearObjects()
        {
            XceleratorShareProjects_DropDown.ClearData();
            XceleratorShareProjects_DropDown.PickListPanelControl.ClearViewData();
            UploadField.ClearData();
            DocName.ClearData();
            DocDescription.ClearData();
        }

        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);

            if (e.Action.Name == "ClearAction")
            {
                ClearObjects();
            }
        }

        #endregion

        #region Private Methods

        private bool TryUploadFileToFileShare(string originalFilePath, out string outPath)
        {
            if (!originalFilePath.StartsWith(CamstarPortalSection.Settings.DefaultSettings.UploadDirectory))
            {
                string fileShareFileName = GetUniqueFileName(originalFilePath);
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

        private string GetCollaborationSpaceId(XceleratorShareDataManager xceleratorsharedatamanager)
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

            if (string.IsNullOrEmpty(session.CollabspaceId))
            {
                session.CollabspaceId = xceleratorsharedatamanager.GetCollaborationSpace(AuthManager.AccessKey, AuthManager.SecretAccessKey);
            }
            return session.CollabspaceId;
        }
        #endregion
    }
}
