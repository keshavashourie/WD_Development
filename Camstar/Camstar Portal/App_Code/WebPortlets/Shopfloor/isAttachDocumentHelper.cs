// Copyright Siemens 2022 
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

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{

    /// <summary>
    /// Additional functionality for grids and data contracts on Attach Document
    /// </summary>
    public class isAttachDocumentHelper : MatrixWebPart
    {
        protected virtual JQDataGrid DocumentsGrid { get { return Page.FindCamstarControl("Defect_isAttachDocumentDetails") as JQDataGrid; } }
        protected virtual CWC.Button AttachBtn { get { return Page.FindCamstarControl("AttachButton") as CWC.Button; } }
        protected virtual CWC.CheckBox DelayAttach { get { return Page.FindCamstarControl("HiddenDelayAttach") as CWC.CheckBox; } }
        protected virtual CWC.CheckBox DocsToAttach { get { return Page.FindCamstarControl("DocsToAttach") as CWC.CheckBox; } }
        protected virtual CWC.DropDownList AttachmentType { get { return Page.FindCamstarControl("AttachmentType") as CWC.DropDownList; } }
        protected virtual CWC.TextBox DocumentPath { get { return Page.FindCamstarControl("HiddenDocumentPath") as CWC.TextBox; } }
        protected virtual CWC.TextBox DocumentName { get { return Page.FindCamstarControl("HiddenDocumentName") as CWC.TextBox; } }
        protected virtual CWC.TextBox DocumentRevision { get { return Page.FindCamstarControl("HiddenDocumentRevision") as CWC.TextBox; } }
        protected virtual CWC.TextBox DocumentDescription { get { return Page.FindCamstarControl("HiddenDocumentDescription") as CWC.TextBox; } }
        protected virtual CWC.TextBox DocumentComments { get { return Page.FindCamstarControl("HiddenDocumentComments") as CWC.TextBox; } }
        protected virtual CWC.CheckBox AttachAsROR { get { return Page.FindCamstarControl("AttachAsROR") as CWC.CheckBox; } }
        protected virtual CWC.CheckBox ExistingDocumentUseROR { get { return Page.FindCamstarControl("ExistingDocumentUseROR") as CWC.CheckBox; } }
        protected virtual CWC.RevisionedObject DocumentInstanceField { get { return Page.FindCamstarControl("ExistingDocument") as CWC.RevisionedObject; } }

        protected bool HaveDocumentToAttach
        {
            get
            {
                return (bool)HttpContext.Current.Session["HaveDocumentToAttach"];
            }
            set
            {
                HttpContext.Current.Session["HaveDocumentToAttach"] = value;
            }
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // log and repair defect popups currently only handle attaching documents
            // no detach or view
            if (AttachBtn != null)
            {
                AttachBtn.Click += new EventHandler(AttachBtn_Click);
            }
           
            if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument" && (DocsToAttach.IsChecked || HaveDocumentToAttach))
            {
                AddDocToGrid();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            DocsToAttach.CheckControl.Checked = false;
            HaveDocumentToAttach = false;
           
        }

        protected virtual void AttachBtn_Click(object sender, EventArgs e)
        {
            // not sure why need to manually set this as DataContract should pick it up directrly from the control....but it doesn't
            Page.DataContract.SetValueByName("DelayAttachDM", DelayAttach.IsChecked);

        }

        /// <summary>
        /// Adds a new document to the list of documents.
        /// </summary>
        private void AddDocToGrid()
        {
            int docCount = DocumentsGrid.TotalRowCount;
            List<AttachDocumentDetails> docs;
            if (docCount > 0)
                docs = new List<AttachDocumentDetails>(DocumentsGrid.Data as AttachDocumentDetails[]);
            else
                docs = new List<AttachDocumentDetails>();

            AttachDocumentDetails newDoc = new AttachDocumentDetails();
            bool isMissed = false;
            AttachmentTypeEnum attachType = Enumeration<AttachmentTypeEnum, int>.ConvertToEnum(Convert.ToInt32(AttachmentType.Data.ToString()), out isMissed);
            newDoc.isAttachmentType = attachType;
            newDoc.AttachmentType = Convert.ToString(attachType);
            if (!DocumentPath.IsEmpty)
            {
                string path = DocumentPath.Data.ToString();
                string fileName = new FileInfo(path).Name;
                string configFolder = CWC.FileInput.GetUploadFolder();// CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;
                DirectoryInfo folder = (Directory.Exists(configFolder)) ? new DirectoryInfo(configFolder) : null;
                if (folder != null)
                {
                    newDoc.isFilePath = folder.FullName;
                    FileInfo file = new FileInfo(folder.FullName + "\\" + fileName);
                    if (file != null)
                    {
                        newDoc.isAttachedFileName = file.Name;
                        newDoc.isAttachedFileExtension = file.Extension;
                    }
                }
                newDoc.isIdentifier = path;
            }

            if (attachType == AttachmentTypeEnum.ExistingDocument)
            {
                // get name/rev from selected existing doc RDO instance. 
                // validation in AttachDocument page will have verified instance not null.
                newDoc.AttachedDocument = DocumentInstanceField.Data as RevisionedObjectRef;
                newDoc.isDocumentName = newDoc.AttachedDocument.Name;
                newDoc.isDocumentRevision = newDoc.AttachedDocument.Revision;
            }
            else
            {
                // get name/rev from controls where user manually entered data
                // validation in AttachDocument page will have verified name not null, but rev may be optional
                newDoc.isDocumentName = DocumentName.Data.ToString();
                newDoc.isDocumentRevision = DocumentRevision.Data != null ? DocumentRevision.Data.ToString() : string.Empty;
            }
            newDoc.isDocumentDescription = DocumentDescription.Data != null ? DocumentDescription.Data.ToString() : string.Empty;
            newDoc.isDocumentComments = DocumentComments.Data != null ? DocumentComments.Data.ToString() : string.Empty;
            newDoc.AttachAsROR = AttachAsROR.IsChecked;
            newDoc.UseROR = ExistingDocumentUseROR.IsChecked;

            docs.Add(newDoc);
            DocumentsGrid.Data = docs.ToArray();

        }


    }


}
