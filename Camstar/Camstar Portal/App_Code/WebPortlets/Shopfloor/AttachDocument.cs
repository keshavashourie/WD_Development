// Copyright Siemens 2025  
using System;
using System.Collections.Generic;
using System.Web;
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
using System.Linq;
using OS = Camstar.WCF.ObjectStack;
using System.Web.UI;
using Camstar.WebPortal.WebPortlets.XceleratorShare;
using System.Threading.Tasks;
using PERS = Camstar.WebPortal.Personalization;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class AttachDocument : MatrixWebPart
    {
        #region Properties
        protected string _saveUniqueFileName = string.Empty;

        protected virtual CWC.CheckBox ImageOnly { get { return Page.FindCamstarControl("AttachDocument_isIsImage") as CWC.CheckBox; } }

        protected virtual CWC.RevisionedObject ImageInstance { get { return Page.FindCamstarControl("AttachDocument_isAttachedImage") as CWC.RevisionedObject; } }

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

        protected virtual CWC.RadioButton RadioBtn_UploadDocument_XceleratorShare
        {
            get { return Page.FindCamstarControl("RadioBtn_UploadDocument_XceleratorShare") as CWC.RadioButton; }
        } // RadioBtn_UploadDocument_XceleratorShare

        protected virtual CWC.DropDownList AttachmentType
        {
            get { return Page.FindCamstarControl("AttachDocument_AttachmentType") as CWC.DropDownList; }
        } // AttachmentType

        protected virtual CWC.TextBox StoredFileNameField
        {
            get { return Page.FindCamstarControl("AttachDocument_AttachedFileName") as CWC.TextBox; }
        } // StoredFileNameField

        protected virtual CWC.NamedObject NDOInstanceField
        {
            get { return Page.FindCamstarControl("AttachDocument_NDO") as CWC.NamedObject; }
        } // NDOInstanceField

        protected virtual CWC.RevisionedObject RDOInstanceField
        {
            get { return Page.FindCamstarControl("AttachDocument_RDO") as CWC.RevisionedObject; }
        } // RDOInstanceField

        protected virtual CWC.ContainerList ContainerField
        {
            get { return Page.FindCamstarControl("AttachDocument_Container") as CWC.ContainerList; }
        } // ContainerField

        protected virtual CWC.CheckBox IsRDOField
        {
            get { return Page.FindCamstarControl("AttachDocument_IsRDO") as CWC.CheckBox; }
        } // IsRDOField

        protected virtual CWC.CheckBox IsContainerField
        {
            get { return Page.FindCamstarControl("AttachDocument_IsContainer") as CWC.CheckBox; }
        } // IsContainerField

        protected virtual CWC.CheckBox IsNDOField
        {
            get { return Page.FindCamstarControl("AttachDocument_IsNDO") as CWC.CheckBox; }
        } // IsNDOField

        protected virtual CWC.DropDownList ObjectTypeField
        {
            get { return Page.FindCamstarControl("AttachDocument_ObjectType") as CWC.DropDownList; }
        } // ObjectTypeField

        protected virtual CWC.DropDownList XceleratorShareProjects_DropDown
        {
            get { return Page.FindCamstarControl("XceleratorShareProjects_DropDown") as CWC.DropDownList; }
        } // XceleratorShareProjects_DropDown

        protected virtual CWC.RevisionedObject DocumentInstanceField
        {
            get { return Page.FindCamstarControl("AttachDocument_DocumentInstance") as CWC.RevisionedObject; }
        } // DocumentInstanceField

        protected virtual CWC.Button ViewDocumentButton
        {
            get { return Page.FindCamstarControl("ViewDocumentButton") as CWC.Button; }
        } //DocumentActionsButton

        protected virtual CWC.CheckBox CalledExternallyField
        {
            get { return Page.FindCamstarControl("AttachDocument_CalledExternally") as CWC.CheckBox; }
        } // CalledExternallyField

        protected virtual CWC.TextBox ServiceTypeNameField
        {
            get { return Page.FindCamstarControl("AttachDocument_ServiceTypeName") as CWC.TextBox; }
        } // ServiceTypeNameField

        protected virtual ContainerListGrid HiddenContainer
        {
            get { return Page.FindCamstarControl("HiddenSelectedContainer") as ContainerListGrid; }
        } // ServiceTypeNameField

        protected virtual CWC.CheckBox AttachAsRORField
        {
            get { return Page.FindCamstarControl("AttachDocument_AttachAsROR") as CWC.CheckBox; }
        } // AttachAsRORField

        protected virtual CWC.TextBox CommentsField
        {
            get { return Page.FindCamstarControl("AttachDocument_Comments") as CWC.TextBox; }
        } // CommentsField

        protected virtual CWC.CheckBox IsPackageField
        {
            get { return Page.FindCamstarControl("AttachDocument_IsPackage") as CWC.CheckBox; }
        } // IsPackageField

        protected virtual CWC.NamedObject PackageField
        {
            get { return Page.FindCamstarControl("CSICDOName_ChangePackage") as CWC.NamedObject; }
        } // PackageField

        protected virtual Personalization.UIAction SubmitAction
        {
            get { return Page.ActionDispatcher.PageActions().FirstOrDefault(x => x.Name.Equals("Submit")); }
        }

        protected virtual CWC.DropDownList AttachMode
        {
            get { return Page.FindCamstarControl("AttachDocument_AttachMode") as CWC.DropDownList; }
        } // AttachDocument_AttachModeEnum

        protected virtual CWC.TextBox URL
        {
            get { return Page.FindCamstarControl("AttachDocument_Identifier") as CWC.TextBox; }
        } // AttachDocument_Identifier

        string ErrorMessage = string.Empty;

        protected virtual CWC.Button SubmitButton { get { return Page.FindCamstarControl("Submit") as CWC.Button; } }
        protected virtual CWC.Button DelayAttachButton { get { return Page.FindCamstarControl("DelayAttachButton") as CWC.Button; } }
        protected virtual CWC.CheckBox DelayAttach { get { return Page.FindCamstarControl("DelayAttach") as CWC.CheckBox; } }
        protected virtual CWC.CheckBox DocsToAttach { get { return Page.FindCamstarControl("DocsToAttach") as CWC.CheckBox; } }

        #endregion

        #region Private Member Variables

        private DirectoryInfo _folder = null;
        private FileInfo _file = null;
        private string _XShareNoActiveProjects = string.Empty;
        private string _XShareProjectMissing = string.Empty;
        private string _XShareFileMissing = string.Empty;
        private string _XShareDocNameMissing = string.Empty;
        private string _XShareInstanceNameMissing = string.Empty;
        private static string _DefaultSubmitAction = string.Empty;
        private Attachment _XShareAttachment = null;
        #endregion

        #region methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            XceleratorShareProjects_DropDown.PickListPanelControl.PreProcessData += PickListPanelControl_PreProcessData;
            RadioBtn_NewDocNOReuse.DataChanged += new EventHandler(RadioBtn_NewDocNOReuse_DataChanged);
            RadioBtn_ExistingDoc.DataChanged += new EventHandler(RadioBtn_ExistingDoc_DataChanged);
            RadioBtn_UploadDocument_XceleratorShare.DataChanged += new EventHandler(RadioBtn_UploadDocument_XceleratorShare_DataChanged);
            RadioBtn_NewDocReuse.DataChanged += new EventHandler(RadioBtn_NewDocReuse_DataChanged);
            ViewDocumentButton.Click += new EventHandler(ViewDocumentButton_Click);
            DocumentInstanceField.DataChanged += new EventHandler(DocumentInstanceField_DataChanged);
            ImageInstance.DataChanged += new EventHandler(ImageInstance_DataChanged);
            AttachMode.DataChanged += new EventHandler(AttachMode_DataChanged);

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
            if (string.IsNullOrEmpty(_XShareInstanceNameMissing))
            {
                _XShareInstanceNameMissing = FrameworkManagerUtil.GetLabelValue("XShareInstanceNameMissing") ?? string.Empty;
            }
            if (RadioBtn_UploadDocument_XceleratorShare.Visible == true)
            {
                RadioBtn_UploadDocument_XceleratorShare.Visible = false;
            }

            if (IsPackageField.IsChecked)
            {
                _DefaultSubmitAction = SubmitAction.ServiceName;
                InitAttachToPackageData();
            }

            if (Page.Request.Form["__EVENTARGUMENT"] == null)
            {

                NDOInstanceField.Hidden = true;
                RDOInstanceField.Hidden = true;
                ContainerField.Hidden = true;
                _DefaultSubmitAction = SubmitAction.ServiceName;

                if (CalledExternallyField.IsChecked == true)
                {
                    ObjectTypeField.Hidden = true;
                    NDOInstanceField.Hidden = true;
                    RDOInstanceField.Hidden = true;
                    ContainerField.Hidden = true;
                    NDOInstanceField.Enabled = false;
                    RDOInstanceField.Enabled = false;
                    ContainerField.Enabled = false;
                }

                if (IsNDOField.IsChecked == true)
                {
                    NDOInstanceField.Hidden = false;
                }

                if (IsRDOField.IsChecked == true)
                {
                    RDOInstanceField.Hidden = false;
                }

                if (IsContainerField.IsChecked == true)
                {
                    ContainerField.Hidden = false;
                }

            }

            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            RadioBtn_UploadDocument_XceleratorShare.Visible = session.HasXceleratorShareAccess && CamstarPortalSection.Settings.DefaultSettings.TeamcenterShareEnabled;
        }

        protected virtual void ImageInstance_DataChanged(object sender, EventArgs e)
        {
            if (ImageInstance.TextEditControl.Text.Length > 0)
            {
                ViewDocumentButton.Enabled = true;
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

        private bool IsValidInstanceName()
        {
            return ObjectTypeField.Data == null || (ObjectTypeField.Data != null && (NDOInstanceField.Data != null || ContainerField.Data != null || RDOInstanceField.Data != null));
        }

        public override bool PreExecute(Info serviceInfo, Service serviceData)
        {
            if (RadioBtn_UploadDocument_XceleratorShare.RadioControl.Checked)
            {
                bool isValid = true;
                if (!IsValidInstanceName())
                {
                    Page.DisplayMessage(_XShareInstanceNameMissing, false);
                    isValid = false;
                }
                else if (!IsValidProject())
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
                        using (XceleratorShareDataManager xceleratorsharedatamanager = new XceleratorShareDataManager())
                        {
                            string collabspaceId = GetCollaborationSpaceId(xceleratorsharedatamanager);


                         _XShareAttachment = xceleratorsharedatamanager.UploadAttachment(AuthManager.AccessKey,
                         AuthManager.SecretAccessKey,
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
            }
            return base.PreExecute(serviceInfo, serviceData);
        }

        private void PickListPanelControl_PreProcessData(object sender, DataRequestEventArgs e)
        {
            var dp = (sender as CWC.PickLists.PickListPanel).DataProvider as CWC.PickLists.SelectionSourceBasedDataProvider;
            var recordSet = (dp.DataProvider as CWC.PickLists.StaticValuesDataProvider).RecordSet;
            recordSet.Clear();
            if (RadioBtn_UploadDocument_XceleratorShare != null && (bool)RadioBtn_UploadDocument_XceleratorShare.Data == true)
            {
                LoadProjectList(recordSet);
                dp.TotalCount = recordSet.Rows.Count;
                if (dp.TotalCount == 0)
                {
                    dp.ResultStatus.IsSuccess = false;
                    dp.ResultStatus.Message = _XShareNoActiveProjects;
                }
            }
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/ImageMaint.js");
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            DelayAttachButton.Visible = false;

            if (ImageOnly.IsChecked)
            {
                LabelCache labelCache = LabelCache.GetRuntimeCacheInstance();
                var label = labelCache.GetLabelByName("isInvalidImage");
                string acceptScript = $"jQuery(document).ready(function(){{ $('#ctl00_WebPartManager_AttachDocument_WP_DocumentPath_InputField').attr('accept','{Modeling.isImageMaint.GetAcceptString()}'); }})";
                string validateScript = $"isImageMaint.initializeDocAttach('{Modeling.isImageMaint.GetExtensionsString()}', '{label.Value}')";
                ScriptManager.RegisterStartupScript(Page.Form, Page.Form.GetType(), "attachImageStartupScript", $"{acceptScript}; {validateScript};", true);
            }

            if (HiddenContainer.TextEditControl.Text.Length > 0)
            {
                IsContainerField.IsChecked = true;
                CalledExternallyField.IsChecked = true;

                IsContainerField.CheckControl.Checked = true;
                CalledExternallyField.CheckControl.Checked = true;
            }

            SubmitAction.IsControlDisabled = !RadioBtn_UploadDocument_XceleratorShare.RadioControl.Checked && ObjectTypeField.IsEmpty && !CalledExternallyField.IsChecked;

            DelayAttachButton.Visible = false;

            if (DelayAttach.IsChecked && !IsContainerField.IsChecked)
            {
                SubmitButton.Visible = false;
                DelayAttachButton.Visible = true;
                ObjectTypeField.Visible = false;
            }
            else if (IsContainerField.IsChecked)
            {
                SubmitButton.Visible = true;
                DelayAttachButton.Visible = false;
                ObjectTypeField.Visible = true;
            }
            if (ErrorMessage != string.Empty)
            {
                Page.StatusBar.WriteError(ErrorMessage);
            }
        }

        protected virtual void DocumentInstanceField_DataChanged(object sender, EventArgs e)
        {
            if (DocumentInstanceField.TextEditControl.Text.Length > 0)
            {
                ViewDocumentButton.Enabled = true;
            }
        }

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
                RadioBtn_UploadDocument_XceleratorShare.RadioControl.Checked = false;
                AttachmentType.Data = (int)AttachmentTypeEnum.NewDocumentReuse;
                URL.Visible = false;
                AttachMode.Visible = true;
                UploadField.Visible = true;
                StoredFileNameField.ClearData();
                ViewDocumentButton.Enabled = false;
                ViewDocumentButton.Visible = false;
                DocRevision.Visible = true;
                ClearXceleratorShareProjectsDropDown();
                XceleratorShareProjects_DropDown.Visible = false;
                SetSubmitAction(_DefaultSubmitAction);
                if (ImageOnly.IsChecked)
                {
                    ImageInstance.ClearData();
                    ImageInstance.Visible = false;
                }
                if (AttachMode.Data != null && (int)AttachMode.Data == 2)
                    AttachMode_DataChanged(this, e);
                AttachMode.Data = 2;
            }
        }

        protected virtual void RadioBtn_ExistingDoc_DataChanged(object sender, EventArgs e)
        {
            if (RadioBtn_ExistingDoc.RadioControl.Checked)
            {
                RadioBtn_NewDocNOReuse.RadioControl.Checked = false;
                RadioBtn_NewDocReuse.RadioControl.Checked = false;
                RadioBtn_UploadDocument_XceleratorShare.RadioControl.Checked = false;
                AttachmentType.Data = (int)AttachmentTypeEnum.ExistingDocument;
                AttachAsRORField.ClearData();
                AttachAsRORField.Visible = false;
                DocRevision.ClearData();
                DocRevision.Visible = false;
                UploadField.Visible = false;
                AttachMode.Visible = false;
                AttachMode.ClearData();
                URL.Visible = false;
                URL.ClearData();
                DocName.ClearData();
                DocName.Visible = false;
                DocDescription.ClearData();
                DocDescription.Visible = false;
                DocumentInstanceField.Visible = true;
                StoredFileNameField.ClearData();
                ViewDocumentButton.Visible = true;
                ClearXceleratorShareProjectsDropDown();
                XceleratorShareProjects_DropDown.Visible = false;
                //  ViewDocumentButton.Enabled = true;
                SetSubmitAction(_DefaultSubmitAction);
                if (ImageOnly.IsChecked)
                {
                    ImageInstance.Visible = true;
                    DocumentInstanceField.Visible = false;
                }
            }
        }

        protected virtual void RadioBtn_NewDocNOReuse_DataChanged(object sender, EventArgs e)
        {
            if (RadioBtn_NewDocNOReuse.RadioControl.Checked)
            {
                RadioBtn_NewDocReuse.RadioControl.Checked = false;
                RadioBtn_ExistingDoc.RadioControl.Checked = false;
                RadioBtn_UploadDocument_XceleratorShare.RadioControl.Checked = false;
                AttachmentType.Data = (int)AttachmentTypeEnum.NewDocumentNOReuse;
                URL.Visible = false;
                AttachMode.Visible = true;
                UploadField.Visible = true;
                StoredFileNameField.ClearData();
                ViewDocumentButton.Enabled = false;
                ViewDocumentButton.Visible = false;
                DocRevision.ClearData();
                DocRevision.Visible = false;
                ClearXceleratorShareProjectsDropDown();
                XceleratorShareProjects_DropDown.Visible = false;
                SetSubmitAction(_DefaultSubmitAction);
                if (ImageOnly.IsChecked)
                {
                    ImageInstance.ClearData();
                    ImageInstance.Visible = false;
                }
                if (AttachMode.Data != null && (int)AttachMode.Data == 2)
                    AttachMode_DataChanged(this, e);
                AttachMode.Data = 2;
            }
        }

        protected virtual void RadioBtn_UploadDocument_XceleratorShare_DataChanged(object sender, EventArgs e)
        {
            if (RadioBtn_UploadDocument_XceleratorShare.RadioControl.Checked)
            {
                RadioBtn_NewDocNOReuse.RadioControl.Checked = false;
                RadioBtn_NewDocReuse.RadioControl.Checked = false;
                RadioBtn_ExistingDoc.RadioControl.Checked = false;
                AttachmentType.Data = (int)AttachmentTypeEnum.NewDocumentReuse;
                AttachAsRORField.ClearData();
                AttachAsRORField.Visible = false;
                DocRevision.ClearData();
                DocRevision.Visible = false;
                UploadField.Visible = true;
                DocName.ClearData();
                DocName.Visible = true;
                DocDescription.ClearData();
                DocDescription.Visible = true;
                DocumentInstanceField.Visible = false;
                StoredFileNameField.ClearData();
                ViewDocumentButton.Visible = false;
                ClearXceleratorShareProjectsDropDown();
                XceleratorShareProjects_DropDown.Visible = true;
                AttachMode.Visible = false;
                AttachMode.ClearData();
                URL.Visible = false;
                URL.ClearData();
                SetSubmitAction("XShareAttachDocument");
            }
        }

        protected virtual void AttachMode_DataChanged(object sender, EventArgs e)
        {
            if (AttachMode.Data != null)
            {
                string mode = AttachMode.Data.ToString();
                switch (mode)
                {   //Local File
                    case "2":
                        {
                            if (RadioBtn_NewDocReuse.RadioControl.Checked)
                            {
                                UploadField.Visible = true;
                                URL.Visible = false;
                                DocName.Visible = true;
                                DocDescription.Visible = true;
                                DocumentInstanceField.ClearData();
                                DocumentInstanceField.Visible = false;
                                AttachAsRORField.Visible = true;
                                DocRevision.Visible = true;

                            }

                            if (RadioBtn_NewDocNOReuse.RadioControl.Checked)
                            {
                                UploadField.Visible = true;
                                URL.Visible = false;
                                DocName.Visible = true;
                                DocDescription.Visible = true;
                                DocumentInstanceField.ClearData();
                                DocumentInstanceField.Visible = false;
                                AttachAsRORField.ClearData();
                                AttachAsRORField.Visible = false;
                                DocRevision.Visible = false;
                                DocRevision.ClearData();
                            }


                            break;
                        }
                    //URL
                    case "3":
                        {
                            if (RadioBtn_NewDocReuse.RadioControl.Checked)
                            {
                                UploadField.Visible = false;
                                URL.Visible = true;
                                URL.Focus();
                                DocName.Visible = true;
                                DocDescription.Visible = true;
                                DocumentInstanceField.ClearData();
                                DocumentInstanceField.Visible = false;
                                AttachAsRORField.Visible = true;
                                DocRevision.Visible = true;

                            }

                            if (RadioBtn_NewDocNOReuse.RadioControl.Checked)
                            {
                                UploadField.Visible = false;
                                URL.Visible = true;
                                URL.Focus();
                                DocName.Visible = true;
                                DocDescription.Visible = true;
                                DocumentInstanceField.ClearData();
                                DocumentInstanceField.Visible = false;
                                AttachAsRORField.ClearData();
                                AttachAsRORField.Visible = false;
                                DocRevision.Visible = false;
                                DocRevision.ClearData();
                            }


                            break;
                        }
                }
            }
        }

        private void ClearXceleratorShareProjectsDropDown()
        {
            XceleratorShareProjects_DropDown.ClearData();
            XceleratorShareProjects_DropDown.PickListPanelControl.ClearViewData();
        }

        private void LoadProjectList(System.Data.DataTable recordSet)
        {
            List<LCSProject> projects = new List<LCSProject>();
            string errorMessage = null;
            XceleratorShareDataManager xceleratorsharedatamanager = new XceleratorShareDataManager();

            try
            {
                string collabspaceId = GetCollaborationSpaceId(xceleratorsharedatamanager);
                projects = xceleratorsharedatamanager.GetProjects(AuthManager.AccessKey,
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

            if (Page.ProcessingContext.Status != ProcessingStatusType.SubmitTransaction)
            {
                return;
            }

            if (!UploadField.IsEmpty)
            {
                StoredFileNameField.Data = new FileInfo((string)UploadField.Data).Name;

                //string uploadDirectory = CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;
                string uniqueUploadDirectory = FileInput.GetUploadFolder();// !string.IsNullOrEmpty(FileInput.UniqueUploadFolder) ? FileInput.UniqueUploadFolder : uploadDirectory;
                if (Directory.Exists(uniqueUploadDirectory))
                {
                    _folder = new DirectoryInfo(uniqueUploadDirectory);
                }
                string fileName = (string)StoredFileNameField.Data;

                if (_folder != null)
                {
                    ((OS.AttachDocument)serviceData).FilePath = _folder.FullName; //File Location

                    _file = new FileInfo(_folder.FullName + "\\" + fileName);
                    string uniqueFilePath = String.Empty;

                    if (TryUploadFileToFileShare(_file.FullName, out uniqueFilePath))
                    {
                        _saveUniqueFileName = GetFileName(uniqueFilePath);
                        ((OS.AttachDocument)serviceData).AttachedFileName = _saveUniqueFileName;
                        ((OS.AttachDocument)serviceData).AttachedFileExtension = _file.Extension;
                    }
                }
                ((OS.AttachDocument)serviceData).Identifier = (string)UploadField.Data;
            }

            if (ContainerField.Data != null || NDOInstanceField.Data != null || RDOInstanceField.Data != null)
            {
                // Extract the corresponding information to save

                // for Container
                if (IsContainerField.CheckControl.Checked == true)
                {
                    ((OS.AttachDocument)serviceData).InstanceName = ((ContainerRef)ContainerField.Data).Name;
                    ((OS.AttachDocument)serviceData).ContainerInstance = (ContainerRef)ContainerField.Data;
                }

                // for NDO
                if (IsNDOField.CheckControl.Checked == true)
                {
                    ((OS.AttachDocument)serviceData).InstanceName = ((NamedObjectRef)NDOInstanceField.Data).Name;
                }

                // for RDO
                if (IsRDOField.CheckControl.Checked == true)
                {
                    ((OS.AttachDocument)serviceData).InstanceName = ((RevisionedObjectRef)RDOInstanceField.Data).Name;
                    ((OS.AttachDocument)serviceData).InstanceRevision = ((RevisionedObjectRef)RDOInstanceField.Data).Revision;
                    ((OS.AttachDocument)serviceData).InstanceIsROR = ((RevisionedObjectRef)RDOInstanceField.Data).RevisionOfRecord;
                }
            }
            if (RadioBtn_UploadDocument_XceleratorShare.RadioControl.Checked)
            {
                ((OS.XShareAttachDocument)serviceData).BrowseMode = BrowseModeEnum.HTTPFile;
                ((OS.XShareAttachDocument)serviceData).DocumentRevision = "1";
                ((OS.XShareAttachDocument)serviceData).Comments = CommentsField.Data?.ToString();
                ((OS.XShareAttachDocument)serviceData).AttachedFileExtension = _file?.Extension;
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
            else
            {
                //UploadField.ClearData();
                if (!string.IsNullOrEmpty(_saveUniqueFileName) && UploadField.Data != null)
                {
                    string uploadDirectory = FileInput.GetUploadFolder();// CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;
                    //if (!string.IsNullOrEmpty(FileInput.UniqueUploadFolder))
                    //    uploadDirectory = FileInput.UniqueUploadFolder;

                    if (Directory.Exists(uploadDirectory))
                    {
                        _folder = new DirectoryInfo(uploadDirectory);

                        if (_folder != null)
                        {
                            try
                            {
                                File.Move(_folder.FullName + "\\" + _saveUniqueFileName, _folder.FullName + "\\" + UploadField.Data.ToString());
                            }
                            catch //    If an error occurs, we must clear the field since it will need to be re-uploaded to the CamstarUploads folder
                            {
                                UploadField.ClearData();
                            }
                        }
                    }
                }
                _saveUniqueFileName = string.Empty;
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
                else
                {
                }
            }
        }

        #endregion

        #region functions

        protected virtual void ClearObjects()
        {
            RadioBtn_NewDocReuse.RadioControl.Checked = false;
            RadioBtn_ExistingDoc.RadioControl.Checked = false;
            RadioBtn_NewDocNOReuse.RadioControl.Checked = false;
            RadioBtn_UploadDocument_XceleratorShare.RadioControl.Checked = false;
            AttachAsRORField.Visible = false;
            DocRevision.Visible = false;
            UploadField.Visible = false;
            DocName.Visible = false;
            DocDescription.Visible = false;
            DocumentInstanceField.Visible = false;
            ViewDocumentButton.Visible = false;
            ContainerField.Hidden = true;
            RDOInstanceField.Hidden = true;
            NDOInstanceField.Hidden = true;
            XceleratorShareProjects_DropDown.Visible = false;
            AttachMode.Visible = false;
            URL.Visible = false;

            AttachmentType.ClearData();
            AttachAsRORField.ClearData();
            DocRevision.ClearData();
            DocName.ClearData();
            DocDescription.ClearData();
            DocumentInstanceField.ClearData();
            StoredFileNameField.ClearData();
            ObjectTypeField.ClearData();
            ObjectTypeField.ClearSelectionValues();
            ContainerField.ClearData();
            RDOInstanceField.ClearData();
            NDOInstanceField.ClearData();
            CommentsField.ClearData();
            UploadField.ClearData();
            URL.ClearData();
            AttachMode.ClearData();
            ClearXceleratorShareProjectsDropDown();

            ImageInstance.Visible = false;
            ImageInstance.ClearData();

            Page.DataContract.SetValueByName("DocumentNameDM", null);
            Page.DataContract.SetValueByName("DocumentRevisionDM", null);
            Page.DataContract.SetValueByName("DocumentCommentsDM", null);
            Page.DataContract.SetValueByName("DocumentDescriptionDM", null);
        }

        protected virtual void ClearDataValues()
        {
            RadioBtn_NewDocReuse.RadioControl.Checked = false;
            RadioBtn_ExistingDoc.RadioControl.Checked = false;
            RadioBtn_NewDocNOReuse.RadioControl.Checked = false;
            RadioBtn_UploadDocument_XceleratorShare.RadioControl.Checked = false;
            AttachAsRORField.Visible = false;
            DocRevision.Visible = false;
            UploadField.Visible = false;
            DocName.Visible = false;
            DocDescription.Visible = false;
            DocumentInstanceField.Visible = false;
            ViewDocumentButton.Visible = false;
            XceleratorShareProjects_DropDown.Visible = false;
            AttachMode.Visible = false;
            URL.Visible = false;

            AttachAsRORField.ClearData();
            DocRevision.ClearData();
            DocName.ClearData();
            DocDescription.ClearData();
            DocumentInstanceField.ClearData();
            StoredFileNameField.ClearData();
            CommentsField.ClearData();
            UploadField.ClearData();
            URL.ClearData();
            AttachMode.ClearData();
            ClearXceleratorShareProjectsDropDown();
            ImageInstance.Visible = false;
            ImageInstance.ClearData();

            if (IsPackageField.IsChecked)
            {
                InitAttachToPackageData();
            }
            Page.DataContract.SetValueByName("DocumentNameDM", null);
            Page.DataContract.SetValueByName("DocumentRevisionDM", null);
            Page.DataContract.SetValueByName("DocumentCommentsDM", null);
            Page.DataContract.SetValueByName("DocumentDescriptionDM", null);
        }

        private string GetLabelValue(string labelId)
        {
            try
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                OS.Label label = labelCache.GetLabelByName(labelId);
                return label.Value;
            }
            catch (Exception ex) //Catch errors
            {
                return ex.ToString();
            }
        }

        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);

            if (e.Action.Name == "ClearAction")
            {
                ClearDataValues();
            }
            else if (e.Action.Name == "DelayAttachButton")
            {
                //Page.CloseFloatingFrameOnSubmit(new ResultStatus());
                if (ValidatePageData())
                {
                    DocsToAttach.CheckControl.Checked = true;
                    Page.CloseFloatingFrameOnSubmit(new ResultStatus());
                }
            }

        }

        private bool ValidatePageData()
        {
            if (AttachmentType.Data == null)
            {
                ErrorMessage = GetLabelValue("isAttachDocument_TypeMustBeSelected");
            }
            else
            {
                bool isMissed = false;
                AttachmentTypeEnum attachType = Enumeration<AttachmentTypeEnum, int>.ConvertToEnum(Convert.ToInt32(AttachmentType.Data.ToString()), out isMissed);

                if (attachType == AttachmentTypeEnum.NewDocumentReuse || attachType == AttachmentTypeEnum.NewDocumentNOReuse)
                {
                    // must have file and doc name set
                    if (AttachMode.Data != null && (int)AttachMode.Data == 2 && (UploadField.Data == null || string.IsNullOrWhiteSpace(UploadField.Data.ToString())))
                    {
                        ErrorMessage = GetLabelValue("AttachDocument_FileMustBeSelected");
                    }
                    if (DocName.Data == null || string.IsNullOrWhiteSpace(DocName.Data.ToString()))
                    {
                        ErrorMessage = GetLabelValue("AttachDocument_NameRequired");
                    }
                }

                if (attachType == AttachmentTypeEnum.NewDocumentReuse)
                {
                    //must have revision set
                    if (DocRevision.Data == null || string.IsNullOrWhiteSpace(DocRevision.Data.ToString()))
                    {
                        ErrorMessage = GetLabelValue("AttachDocument_RevisionRequired");
                    }
                }

                if (attachType == AttachmentTypeEnum.ExistingDocument)
                {
                    if (DocumentInstanceField.Data == null)
                    {
                        ErrorMessage = GetLabelValue("AttachDocument_ExistingDocumentRequired");
                    }
                }
            }

            return string.IsNullOrEmpty(ErrorMessage);
        }

        protected virtual DocumentRefInfo DownloadDocument()
        {
            ResultStatus resultStatus;
            var documentRev = GetRevObjectRef();
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var configFolder = CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;
            var labelCache = LabelCache.GetRuntimeCacheInstance();
            var label = labelCache.GetLabelByName("Lbl_SharedFolderDoesntExists");
            var message = string.Format(label != null ? label.Value : "Shared folder '{0}' does not exist.", configFolder);

            if (ImageOnly.IsChecked)
            {
                var docInfo = Camstar.WebPortal.WebPortlets.Modeling.isImageAttachmentExecutor.DownloadDocumentRef(documentRev, session.CurrentUserProfile, message, out resultStatus);
                if (!resultStatus.IsSuccess)
                {
                    Page.DisplayMessage(resultStatus);
                    docInfo = null;
                }
                return docInfo;
            }
            else
            {
                var docInfo = AttachmentExecutor.DownloadDocumentRef(documentRev, session.CurrentUserProfile, message, out resultStatus);
                if (!resultStatus.IsSuccess)
                {
                    Page.DisplayMessage(resultStatus);
                    docInfo = null;
                }
                return docInfo;
            }
        }

        protected virtual RevisionedObjectRef GetRevObjectRef()
        {
            if (ImageOnly.IsChecked)
            {
                var nameTxt = ((RevisionedObjectRef)(ImageInstance.Data)).Name;
                var revTxt = ((RevisionedObjectRef)(ImageInstance.Data)).Revision;
                var isROR = ((RevisionedObjectRef)(ImageInstance.Data)).RevisionOfRecord;
                if (!string.IsNullOrEmpty(nameTxt))
                {
                    var name = nameTxt == null ? "" : nameTxt.ToString();
                    var rev = revTxt == null ? "" : revTxt.ToString();
                    bool useROR = isROR == null ? false : (bool)isROR;
                    return WSObjectRef.AssignRevisionedObject(name, rev, useROR, "isImage");
                }
                return null;
            }
            else
            {
                var nameTxt = ((RevisionedObjectRef)(DocumentInstanceField.Data)).Name;
                var revTxt = ((RevisionedObjectRef)(DocumentInstanceField.Data)).Revision;
                var isROR = ((RevisionedObjectRef)(DocumentInstanceField.Data)).RevisionOfRecord;
                if (!string.IsNullOrEmpty(nameTxt))
                {
                    var name = nameTxt == null ? "" : nameTxt.ToString();
                    var rev = revTxt == null ? "" : revTxt.ToString();
                    bool useROR = isROR == null ? false : (bool)isROR;
                    return WSObjectRef.AssignRevisionedObject(name, rev, useROR, "Document");
                }
            }
            return null;
        }

        protected virtual void InitAttachToPackageData()
        {
            CalledExternallyField.Data = true;
            ServiceTypeNameField.Data = "ChangePackageHeader";
            IsNDOField.Data = true;
            // Set InstanceName value
            NDOInstanceField.Data = PackageField.Data;
            // Hide AttachDocument_NDO control element
            NDOInstanceField.Visible = false;
            // Set default radio button value
            if (!RadioBtn_ExistingDoc.RadioControl.Checked)
            {
                RadioBtn_ExistingDoc.RadioControl.Checked = true;
                RadioBtn_ExistingDoc_DataChanged(RadioBtn_ExistingDoc, null);
            }
        }

        #endregion

        #region private Methods

        private bool TryUploadFileToFileShare(string originalFilePath, out string outPath)
        {
            if (!originalFilePath.StartsWith(CamstarPortalSection.Settings.DefaultSettings.UploadDirectory))
            {
                string fileShareFileName = GetUserProfile().Name + GetFileName(originalFilePath);// GetUniqueFileName(originalFilePath);
                fileShareFileName = GetUniqueFileName(originalFilePath); ;
                OS.UserProfile profile = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session).CurrentUserProfile;

                string errorMsg = string.Empty;

                if (!FileMgmtUtil.UploadFileToFileShare(originalFilePath, HttpUtility.UrlEncode(fileShareFileName), profile, GetUserDomain(), out outPath, out errorMsg))
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

        private void SetSubmitAction(string action)
        {
            SubmitAction.ServiceName = action;
        }

        private string GetCollaborationSpaceId(XceleratorShareDataManager xceleratorsharedatamanager)
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

            if (string.IsNullOrEmpty(session.CollabspaceId))
            {
                session.CollabspaceId = xceleratorsharedatamanager.GetCollaborationSpace(Page.Request.Headers.Get(XceleratorShareAccessManager.ACCESS_KEY), Page.Request.Headers.Get(XceleratorShareAccessManager.SECRET_ACCESS_KEY));
            }
            return session.CollabspaceId;
        }
        #endregion

    }
}
