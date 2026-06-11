// © 2017 Siemens Product Lifecycle Management Software Inc.
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using System.IO;
using Camstar.WebPortal.WCFUtilities;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    /// <summary>
    /// For Web Part isAttachImage_WP
    /// </summary>
    public class isAttachImageHelper : AttachDocumentHelper
    {

        protected virtual CWC.RevisionedObject isAttachedImage
        {
            get { return Page.FindCamstarControl("DetachDocument_isAttachedImage") as CWC.RevisionedObject; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }


        protected override void AttachBtn_Click(object sender, EventArgs e)
        {
            Page.DataContract.SetValueByName("IsContainer", false);
            Page.DataContract.SetValueByName("CalledExternally", true);
            Page.DataContract.SetValueByName("IsNDO", IsNDO.IsChecked);
            Page.DataContract.SetValueByName("IsRDO", IsRDO.IsChecked);
            Page.DataContract.SetValueByName("ServiceTypeName", Page.DataContract.GetValueByName("PrimaryServiceType"));
            Page.DataContract.SetValueByName("ImageOnlyDM", true);
            if (IsNDO.IsChecked)
            {
                MaintenanceBehaviorContext pageContext = (MaintenanceBehaviorContext)Page.PortalContext;
                if (pageContext != null)
                {
                    Page.DataContract.SetValueByName("ObjectTypeId", pageContext.CDOID);
                }
                Page.DataContract.SetValueByName("NDOName", Page.DataContract.GetValueByName("InstanceName"));
            }

            if (IsRDO.IsChecked)
            {
                MaintenanceBehaviorContext pageContext = (MaintenanceBehaviorContext)Page.PortalContext;
                if (pageContext != null)
                {
                    Page.DataContract.SetValueByName("ObjectTypeId", pageContext.CDOID);
                }
                Page.DataContract.SetValueByName("RDOName", Page.DataContract.GetValueByName("InstanceName"));
            }
        }

        protected override void DetachBtn_Click(object sender, EventArgs e)
        {
            if (AttachedDocumentsGrid.SelectedRowIDs != null && !string.IsNullOrEmpty(SelectedObjectTypeIdField.TextControl.Text))
            {
                Page.DataContract.SetValueByName("AttachedDocumentViewOnlyDM", DocumentInstanceField.Data);
                //Page.DataContract.SetValueByName("AttachedDocumentDM", DoumentInHistoryField.Data);
                if (DocumentInHistoryField.Data != null)
                    Page.DataContract.SetValueByName("AttachedImageDM", DocumentInHistoryField.Data);
                else
                    Page.DataContract.SetValueByName("AttachedImageDM", DocumentInstanceField.Data);
                Page.DataContract.SetValueByName("CalledExternallyDM", false);
                Page.DataContract.SetValueByName("ObjectTypeIdDM", Convert.ToInt32(SelectedObjectTypeIdField.TextControl.Text));
                Page.DataContract.SetValueByName("AttachAsRORDM", AttachAsRORField.IsChecked);
                Page.DataContract.SetValueByName("UseRORDM", UseRORField.IsChecked);
                Page.DataContract.SetValueByName("DispalyNameDM", (string)Page.DataContract.GetValueByName("InstanceName"));
                Page.DataContract.SetValueByName("ImageOnlyDM", true);

                if (IsNDO.IsChecked)
                {

                    Page.DataContract.SetValueByName("ObjectTypeDM", 0);
                    Page.DataContract.SetValueByName("InstanceNameDM", (string)Page.DataContract.GetValueByName("InstanceName"));
                    Page.DataContract.SetValueByName("IsNDODM", true);
                    Page.DataContract.SetValueByName("IsRDODM", false);
                    Page.DataContract.SetValueByName("IsContainerDM", false);
                }
                else if (IsRDO.IsChecked)
                {
                    // below parsing logic was failing on an RO modeling page due to "InstanceName" not containing revision.
                    // parsing seems to expect value like <name>(<rev>).
                    // would throw Index out of range exception setting s2.
                    // don't know use case where this might work or if it is obsolete logic.
                    // only try it if InstanceName and InstanceRevision are not set as DataContract.

                    string RDOName = (string)Page.DataContract.GetValueByName("InstanceName");
                    string RDOrev = (string)Page.DataContract.GetValueByName("InstanceRevision");

                    if (string.IsNullOrEmpty(RDOName) || string.IsNullOrEmpty(RDOrev))
                    {
                        string s = (string)Page.DataContract.GetValueByName("InstanceName");
                        string[] parts = s.Split('(');
                        RDOName = parts[0].Trim();
                        string s2 = parts[1];
                        string[] parts2 = s2.Split(')');
                        RDOrev = parts2[0].Trim();
                    }

                    Page.DataContract.SetValueByName("InstanceNameDM", RDOName);
                    Page.DataContract.SetValueByName("InstanceRevisionDM", RDOrev);
                    Page.DataContract.SetValueByName("IsRDODM", true);
                    Page.DataContract.SetValueByName("IsNDODM", false);
                    Page.DataContract.SetValueByName("IsContainerDM", false);
                }
            }
            
        }

        protected override ResponseData AttachedDocumentsGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            if (AttachedDocumentsGrid.SelectedRowIDs.Length > 0)
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
                r.CDOTypeName = "isImage";
                DocumentInstanceField.Data = r;
                return new StatusData(true, "Row selected");
            }
            else
            {
                DetachBtn.Enabled = false;
                ViewBtn.Enabled = false;
                DocumentInstanceField.ClearData();
                return new StatusData(false, "Row not selected");
            }
        }

        protected override DocumentRefInfo DownloadDocument()
        {
            ResultStatus resultStatus;
            var documentRev = (RevisionedObjectRef)DocumentInstanceField.Data;

            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var configFolder = CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;
            LabelCache labelCache = LabelCache.GetRuntimeCacheInstance();
            var label = labelCache.GetLabelByName("Lbl_SharedFolderDoesntExists");
            var message = string.Format(label != null ? label.Value : "Shared folder '{0}' does not exist.", configFolder);

            var docInfo = isImageAttachmentExecutor.DownloadDocumentRef(documentRev, session.CurrentUserProfile, message,
                out resultStatus);
            if (!resultStatus.IsSuccess)
            {
                Page.DisplayMessage(resultStatus);
                docInfo = null;
            }
            return docInfo;
        }

    }
}
