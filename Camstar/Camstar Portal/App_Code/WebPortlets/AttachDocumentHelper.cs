// Copyright Siemens 2023  
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

namespace Camstar.WebPortal.WebPortlets
{

    /// <summary>
    /// Additional functionality for grids and data contracts on Attach Document
    /// </summary>
    public class AttachDocumentHelper : MatrixWebPart
    {
		#region Controls
		

        protected virtual JQDataGrid AttachedDocumentsGrid
		{
			get { return Page.FindCamstarControl("AttachedDocuments") as JQDataGrid; }
		}

        protected virtual CWC.Button AttachBtn
		{
			get { return Page.FindCamstarControl("AttachButton") as CWC.Button; }
		}

        protected virtual CWC.Button DetachBtn
		{
			get { return Page.FindCamstarControl("DetachButton") as CWC.Button; }
		}

        protected virtual CWC.Button ViewBtn
		{
			get { return Page.FindCamstarControl("ViewButton") as CWC.Button; }
		}

        protected virtual CWC.TextBox NameField
		{
			get { return Page.FindCamstarControl("NameTxt") as CWC.TextBox; }
		}

        protected virtual CWC.CheckBox IsNDO
		{
			get { return Page.FindCamstarControl("IsNDO") as CWC.CheckBox; }
		}

        protected virtual CWC.CheckBox IsRDO
		{
			get { return Page.FindCamstarControl("IsRDO") as CWC.CheckBox; }
		}

        protected virtual CWC.RevisionedObject DocumentInstanceField
		{
			get { return Page.FindCamstarControl("AttachDocument_DocInstance") as CWC.RevisionedObject; }
		}

        protected virtual CWC.TextBox StoredFileNameField
		{
			get { return Page.FindCamstarControl("AttachDocument_AttachedFileName") as CWC.TextBox; }
		}

        protected virtual CWC.TextBox InstanceNameField
		{
			get { return Page.FindCamstarControl("NameTxt") as CWC.TextBox; }
		}
		#endregion

        protected virtual CWC.CheckBox AttachAsRORField
        {
            get { return Page.FindCamstarControl("AttachAsROR") as CWC.CheckBox; }
        } // AttachAsROR

        protected virtual CWC.CheckBox UseRORField
        {
            get { return Page.FindCamstarControl("UseROR") as CWC.CheckBox; }
        } // UseROR

        protected virtual CWC.TextBox SelectedObjectTypeIdField
        {
            get { return Page.FindCamstarControl("SelectedObjectTypeId") as CWC.TextBox; }
        } // SelectedObjectTypeIdField

        protected virtual CWC.RevisionedObject DocumentInHistoryField
        {
            get { return Page.FindCamstarControl("AttachDocument_DocInHistory") as CWC.RevisionedObject; }
        }//DoumentInHistoryField

		#region Protected Functions

		/// <summary>
		/// Handle button state based on grid selected rows
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
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
				r.CDOTypeName = "Document";
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

        protected virtual void ViewBtn_Click(object sender, EventArgs e)
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

        protected virtual void DetachBtn_Click(object sender, EventArgs e)
		{
            Page.DataContract.SetValueByName("AttachedDocumentViewOnlyDM", DocumentInstanceField.Data);
            Page.DataContract.SetValueByName("AttachedDocumentDM", DocumentInHistoryField.Data);
            Page.DataContract.SetValueByName("CalledExternallyDM", false);
            Page.DataContract.SetValueByName("ObjectTypeIdDM", Convert.ToInt32(SelectedObjectTypeIdField.TextControl.Text));
            Page.DataContract.SetValueByName("AttachAsRORDM",AttachAsRORField.IsChecked);
            Page.DataContract.SetValueByName("UseRORDM",UseRORField.IsChecked);
            Page.DataContract.SetValueByName("DispalyNameDM", (string)Page.DataContract.GetValueByName("InstanceName"));
            
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
 
                string s = (string)Page.DataContract.GetValueByName("InstanceName");
                string[] parts = s.Split('(');
                string RDOName = parts[0].Trim();
                string s2 = parts[1];
                string[] parts2 = s2.Split(')');
                string RDOrev = parts2[0].Trim();
  
                Page.DataContract.SetValueByName("InstanceNameDM", RDOName);
                Page.DataContract.SetValueByName("InstanceRevisionDM", RDOrev);
                Page.DataContract.SetValueByName("IsRDODM", true);
                Page.DataContract.SetValueByName("IsNDODM", false);
                Page.DataContract.SetValueByName("IsContainerDM", false);
            }

		}

        protected virtual void AttachBtn_Click(object sender, EventArgs e)
		{
			Page.DataContract.SetValueByName("IsContainer", false);
			Page.DataContract.SetValueByName("CalledExternally", true);
			Page.DataContract.SetValueByName("IsNDO", IsNDO.IsChecked);
			Page.DataContract.SetValueByName("IsRDO", IsRDO.IsChecked);						
			Page.DataContract.SetValueByName("ServiceTypeName", Page.DataContract.GetValueByName("PrimaryServiceType"));
			if (IsNDO.IsChecked)
			{
			    MaintenanceBehaviorContext pageContext = (MaintenanceBehaviorContext) Page.PortalContext;
			    if (pageContext != null)
			    {
			        Page.DataContract.SetValueByName("ObjectTypeId",  pageContext.CDOID);    
			    }
				Page.DataContract.SetValueByName("NDOName", Page.DataContract.GetValueByName("InstanceName"));
			}

			if (IsRDO.IsChecked)
			{				
				Page.DataContract.SetValueByName("RDOName", Page.DataContract.GetValueByName("InstanceName"));
			}
		}

		#endregion

		#region Public Functions

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

        #region Constants

        #endregion

	    #region Private Member Variables

        #endregion
    }
}
