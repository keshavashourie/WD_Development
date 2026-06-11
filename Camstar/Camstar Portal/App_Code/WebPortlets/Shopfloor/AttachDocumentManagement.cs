// Copyright Siemens 2023  
using System;
using System.Web;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using PERS = Camstar.WebPortal.Personalization;
using System.IO;
using OS = Camstar.WCF.ObjectStack;
using System.Web.UI;
using Camstar.WebPortal.WCFUtilities;
using OM = Camstar.WCF.ObjectStack;

/// <summary>
/// Class used as code behind for Document Attach Management
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
	public class AttachDocumentManagement : MatrixWebPart
	{

		#region Properties

        protected virtual CWC.Button SearchBtn
		{
			get { return Page.FindCamstarControl("SearchBtn") as CWC.Button; }
		} // SearchBtn 

        protected virtual CWC.TextBox DocRevision
		{
			get { return Page.FindCamstarControl("AttachDocument_DocumentRevision") as CWC.TextBox; }
		} // DocRevision

        protected virtual JQDataGrid AttachedDocumentsGrid
		{
            get { return Page.FindCamstarControl("GetAttachedDocuments_AttachedDocuments") as JQDataGrid; }
		} // AttachedDocumentsGrid

        protected virtual CWC.NamedObject NDOInstanceField
		{
			get { return Page.FindCamstarControl("GetAttachedDocuments_NDO") as CWC.NamedObject; }
		} // NDOInstanceField

        protected virtual CWC.RevisionedObject RDOInstanceField
		{
			get { return Page.FindCamstarControl("GetAttachedDocuments_RDO") as CWC.RevisionedObject; }
		} // RDOInstanceField

        protected virtual CWC.ContainerList ContainerField
		{
			get { return Page.FindCamstarControl("GetAttachedDocuments_Container") as CWC.ContainerList; }
		} // ContainerField

        protected virtual CWC.CheckBox IsRDOField
		{
			get { return Page.FindCamstarControl("GetAttachedDocuments_IsRDO") as CWC.CheckBox; }
		} // IsRDOField

        protected virtual CWC.CheckBox IsContainerField
		{
			get { return Page.FindCamstarControl("GetAttachedDocuments_IsContainer") as CWC.CheckBox; }
		} // IsContainerField

        protected virtual CWC.CheckBox IsNDOField
		{
			get { return Page.FindCamstarControl("GetAttachedDocuments_IsNDO") as CWC.CheckBox; }
		} // IsNDOField

        protected virtual CWC.Button AttachBtn
		{
			get { return Page.FindCamstarControl("AttachButton") as CWC.Button; }
		} //AttachBtn

        protected virtual CWC.Button DetachBtn
		{
			get { return Page.FindCamstarControl("DetachButton") as CWC.Button; }
		} //DetachBtn

        protected virtual CWC.Button ClearBtn
		{
			get { return Page.FindCamstarControl("ClearBtn") as CWC.Button; }
		} //ClearBtn

        protected virtual CWC.RevisionedObject DocumentInstanceField
		{
			get { return Page.FindCamstarControl("SelectedDocument") as CWC.RevisionedObject; }
		} // DocumentInstanceField

        protected virtual CWC.RevisionedObject AttachedDocumentHistoryField
		{
			get { return Page.FindCamstarControl("AttachedDocumentHistory") as CWC.RevisionedObject; }
		} // AttachedDocumentHistoryField

        protected virtual CWC.TextBox StoredFileNameField
		{
			get { return Page.FindCamstarControl("DocAttachManagement_AttachedFileName") as CWC.TextBox; }
		} // StoredFileNameField

        protected virtual CWC.TextBox SelectedObjectTypeIdField
		{
			get { return Page.FindCamstarControl("SelectedObjectTypeId") as CWC.TextBox; }
		} // SelectedObjectTypeIdField

        protected virtual CWC.TextBox SelectedObjectTypeField
		{
			get { return Page.FindCamstarControl("SelectedObjectType") as CWC.TextBox; }
		} // SelectedObjectTypeField

        protected virtual CWC.TextBox SelectedInstanceField
		{
			get { return Page.FindCamstarControl("SelectedInstance") as CWC.TextBox; }
		} // SelectedInstanceField

        protected virtual CWC.ContainerList SelectedContainerInstanceField
		{
			get { return Page.FindCamstarControl("SelectedContainerInstnace") as CWC.ContainerList; }
		} // SelectedContainerInstanceField

        protected virtual CWC.DropDownList ObjectTypeIdField
		{
			get { return Page.FindCamstarControl("GetAttachedDocuments_ObjectTypeId") as CWC.DropDownList; }
		} // ObjectTypeIdField

        protected virtual CWC.DateChooser FromTimeStampIdField
		{
			get { return Page.FindCamstarControl("GetAttachedDocuments_FromTimeStamp") as CWC.DateChooser; }
		} // FromTimeStampIdField

        protected virtual CWC.DateChooser ToTimeStampField
		{
			get { return Page.FindCamstarControl("GetAttachedDocuments_ToTimestamp") as CWC.DateChooser; }
		} // ToTimeStampField

        protected virtual CWC.CheckBox IncludeAttachedField
		{
			get { return Page.FindCamstarControl("GetAttachedDocuments_IncludeAttached") as CWC.CheckBox; }
		} // IncludeAttachedField

        protected virtual CWC.CheckBox IncludeDetachedField
		{
			get { return Page.FindCamstarControl("GetAttachedDocuments_IncludeDetached") as CWC.CheckBox; }
		} // IncludeDetachedField

        protected virtual CWC.CheckBox ShowOnlyRORField
		{
			get { return Page.FindCamstarControl("GetAttachedDocuments_ShowOnlyROR") as CWC.CheckBox; }
		} // ShowOnlyRORField

        protected virtual CWC.DropDownList AttachmentTypeField
        {
            get { return Page.FindCamstarControl("GetAttachedDocuments_AttachmentType") as CWC.DropDownList; }
        } // AttachmentTypeField

        protected virtual CWC.DropDownList DocumentTypeField
        {
            get { return Page.FindCamstarControl("GetAttachedDocuments_DocumentType") as CWC.DropDownList; }
        } // DocumentTypeField

        protected virtual CWC.RevisionedObject DocumentNameField
		{
			get { return Page.FindCamstarControl("GetAttachedDocuments_DocumentName") as CWC.RevisionedObject; }
		} // DocumentNameField

        protected virtual CWC.NamedObject EmployeeFilterField
		{
			get { return Page.FindCamstarControl("GetAttachedDocuments_EmployeeFilter") as CWC.NamedObject; }
		} // EmployeeFilterField

        protected virtual CWC.DropDownList FileExtensionField
		{
			get { return Page.FindCamstarControl("GetAttachedDocuments_FileExtension") as CWC.DropDownList; }
		} // FileExtensionField

        protected virtual CWC.CheckBox IsDetachedField
		{
			get { return Page.FindCamstarControl("IsDetached") as CWC.CheckBox; }
		} // IsDetachedField

        protected virtual CWC.CheckBox AttachAsRORField
		{
			get { return Page.FindCamstarControl("AttachAsROR") as CWC.CheckBox; }
		} // AttachAsROR

        protected virtual CWC.CheckBox UseRORField
		{
			get { return Page.FindCamstarControl("UseROR") as CWC.CheckBox; }
		} // UseROR
    
  
		

		#endregion


		# region methods

		public override void RequestValues(Info serviceInfo, Service serviceData)
		{
			base.RequestValues(serviceInfo, serviceData);

			if (ContainerField.Data != null || NDOInstanceField.Data != null || RDOInstanceField.Data != null)
			{
				// Extract the corresponding information to save

				// for Container
				if (IsContainerField != null)
				{
					if (IsContainerField.CheckControl.Checked == true && ContainerField.Data != null)
					{
						((OS.GetAttachedDocuments)serviceData).InstanceName = ((ContainerRef)ContainerField.Data).Name;
						((OS.GetAttachedDocuments)serviceData).ContainerInstance = (ContainerRef)ContainerField.Data;
					}
				}

				// for NDO
				if (IsNDOField != null)
				{
					if (IsNDOField.CheckControl.Checked == true && NDOInstanceField.Data != null)
					{
						((OS.GetAttachedDocuments)serviceData).InstanceName = ((NamedObjectRef)NDOInstanceField.Data).Name;
					}
				}

				// for RDO
				if (IsRDOField != null)
				{
					if (IsRDOField.CheckControl.Checked == true && RDOInstanceField.Data != null) 
					{
						((OS.GetAttachedDocuments)serviceData).InstanceName = ((RevisionedObjectRef)RDOInstanceField.Data).Name;
						((OS.GetAttachedDocuments)serviceData).InstanceRevision = ((RevisionedObjectRef)RDOInstanceField.Data).Revision;
						((OS.GetAttachedDocuments)serviceData).InstanceIsROR = ((RevisionedObjectRef)RDOInstanceField.Data).RevisionOfRecord;
					}
				}
               
            
            }
		}

		
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			AttachBtn.Click += new EventHandler(AttachBtn_Click);
			DetachBtn.Click += new EventHandler(DetachBtn_Click);
			ClearBtn.Click += new EventHandler(ClearBtn_Click);
            ObjectTypeIdField.DataChanged += new EventHandler(ObjectTypeIdField_DataChanged);
			AttachedDocumentsGrid.RowSelected += new JQGridEventHandler(AttachedDocumentsGrid_RowSelected);			

            AttachBtn.Hidden = true;
            DetachBtn.Hidden = true;


            // Portal backup - Ensures Seaarch Btn Functionality onLoad
            if (ObjectTypeIdField.IsEmpty == true && DocumentTypeField.IsEmpty == true && EmployeeFilterField.IsEmpty == true && DocumentNameField.IsEmpty == true && FileExtensionField.IsEmpty == true && ToTimeStampField.IsEmpty == true && FromTimeStampIdField.IsEmpty == true)
            {
                SearchBtn.Enabled = false;
            }
            if (!ObjectTypeIdField.IsEmpty == true || !DocumentTypeField.IsEmpty == true || !EmployeeFilterField.IsEmpty == true || !DocumentNameField.IsEmpty == true || !FileExtensionField.IsEmpty == true || !ToTimeStampField.IsEmpty == true || !FromTimeStampIdField.IsEmpty == true)
            {
                SearchBtn.Enabled = true;
            }
            // clears Object instance field when ObjectType field is cleared or changed
             
                //ContainerField.ClearData();
                //NDOInstanceField.ClearData();
                //RDOInstanceField.ClearData(); 

             if (DocumentTypeField.IsEmpty == true)
             {
                 DocumentTypeField.ClearData();
             }
         
			// Executes Search click after a Doc Attach Popup has been called
			if (Page.Request.Form["__EVENTARGUMENT"] == "FloatingFrameSubmitParentPostBackArgument")
			{
				if (AnyFieldSelected())
                    ScriptManager.RegisterStartupScript(Page.Form, GetType(), "ExecuteSearch", " $('#ctl00_WebPartManager_DocSearchButtons_WP_SearchBtn').click();", true);
               SearchBtn.Enabled = true;
			}			
			
		}

        protected virtual void ClearBtn_Click(object sender, EventArgs e)
		{
			ClearAllFields();
		}

        protected virtual ResponseData AttachedDocumentsGrid_RowSelected(object sender, JQGridEventArgs args)
		{
			if (AttachedDocumentsGrid.SelectedRowIDs.Length > 0)
			{
				return new StatusData(true, "Row selected");
			}
			else
			{
				DetachBtn.Enabled = false;
				DocumentInstanceField.ClearData();
				return new StatusData(false, "Row not selected");
			}
		}
        protected virtual void ObjectTypeIdField_DataChanged(object sender, EventArgs e)
        {
            ContainerField.ClearData();
            NDOInstanceField.ClearData();
            RDOInstanceField.ClearData();
            SelectedInstanceField.ClearData();
            SelectedContainerInstanceField.ClearData();
            IsRDOField.ClearData();
            IsRDOField.CheckControl.Checked = false;
            IsNDOField.ClearData();
            IsNDOField.CheckControl.Checked = false;
            IsContainerField.ClearData();
            IsContainerField.CheckControl.Checked = false;
		}

        protected virtual void DetachBtn_Click(object sender, EventArgs e)
		{
			
            int objectType;
            objectType = Convert.ToInt32(SelectedObjectTypeField.TextControl.Text);

            if ((Boolean)UseRORField.IsChecked)
            {
                RevisionedObjectRef r = new RevisionedObjectRef();
                DocumentInstanceField.RevisionControl.Text = "";
                DocumentInstanceField.RevisionOfRecord = true;
            }

            Page.DataContract.SetValueByName("AttachedDocumentViewOnlyDM", DocumentInstanceField.Data);
            Page.DataContract.SetValueByName("AttachedDocumentDM", AttachedDocumentHistoryField.Data);
            Page.DataContract.SetValueByName("CalledExternallyDM", false);
            Page.DataContract.SetValueByName("ObjectTypeIdDM", Convert.ToInt32(SelectedObjectTypeIdField.TextControl.Text));
            Page.DataContract.SetValueByName("AttachAsRORDM",AttachAsRORField.IsChecked);
            Page.DataContract.SetValueByName("UseRORDM",UseRORField.IsChecked);
            Page.DataContract.SetValueByName("DispalyNameDM", SelectedInstanceField.Data);
				
				
            if (objectType== 0)
			{
                Page.DataContract.SetValueByName("ObjectTypeDM", 0);
                Page.DataContract.SetValueByName("InstanceNameDM", SelectedContainerInstanceField.TextEditControl.Text);
                Page.DataContract.SetValueByName("IsNDODM", true);
                Page.DataContract.SetValueByName("IsRDODM", false);
                Page.DataContract.SetValueByName("IsContainerDM", false);
								
			}
            else if (objectType == 1)
			{
                Page.DataContract.SetValueByName("ObjectTypeDM", 1);
               
                string s = SelectedInstanceField.Data.ToString();
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
            else if (objectType == 2)
			{
				
                Page.DataContract.SetValueByName("ObjectTypeDM", 2);
                Page.DataContract.SetValueByName("InstanceNameDM", SelectedContainerInstanceField.TextEditControl.Text);
                Page.DataContract.SetValueByName("ContainerInstanceDM",SelectedContainerInstanceField.Data);
                Page.DataContract.SetValueByName("IsContainerDM", true);
                Page.DataContract.SetValueByName("IsNDODM", false);
                Page.DataContract.SetValueByName("IsRDODM", false);
			}

		}


		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

            if (DocumentTypeField.TextEditControl.Text.Length < 1)
            {
                DocumentNameField.ClearData();
                DocumentNameField.Enabled = false;
            }
		}

        protected virtual void AttachBtn_Click(object sender, EventArgs e)
		{
			Page.DataContract.SetValueByName("CalledExternally", false);

            // call attach with grid's information
            if (AttachedDocumentsGrid.SelectedRowID != null)
            {
                Page.DataContract.SetValueByName("ObjectTypeId", SelectedObjectTypeIdField.Data);

                int objectType;
                objectType = Convert.ToInt32(SelectedObjectTypeField.TextControl.Text);


                if (objectType == 2)
                {
                    Page.DataContract.SetValueByName("IsContainer", true);
                    Page.DataContract.SetValueByName("ContainerName", SelectedContainerInstanceField.Data);
                }
                
                if (objectType == 1)
                {
                    Page.DataContract.SetValueByName("IsRDO", true);

                    RevisionedObjectRef RDORef = new RevisionedObjectRef();
                    string s = SelectedInstanceField.Data.ToString();
                    string[] parts = s.Split('(');
                    string RDOName = parts[0].Trim();
                    string s2 = parts[1];
                    string[] parts2 = s2.Split(')');
                    string RDOrev = parts2[0].Trim();
                    RDORef.Name = RDOName;
                    RDORef.Revision = RDOrev;

                    Page.DataContract.SetValueByName("RDOName", RDORef);
                }

                
                if (objectType == 0)
                {
                    Page.DataContract.SetValueByName("IsNDO", true);
                    Page.DataContract.SetValueByName("NDOName", SelectedInstanceField.Data);
                }
            }
            else
            {
                Page.DataContract.SetValueByName("ObjectTypeId", ObjectTypeIdField.Data);

                if (ObjectTypeIdField.TextEditControl.Text.Length > 0)
                {
                    if (IsContainerField.IsChecked)
                    {
                        Page.DataContract.SetValueByName("IsContainer", true);
                        Page.DataContract.SetValueByName("ContainerName", ContainerField.Data);
                    }

                    if (IsRDOField.IsChecked)
                    {
                        Page.DataContract.SetValueByName("IsRDO", true);
                        Page.DataContract.SetValueByName("RDOName", RDOInstanceField.Data);
                    }

                    if (IsNDOField.IsChecked)
                    {
                        Page.DataContract.SetValueByName("IsNDO", true);
                        Page.DataContract.SetValueByName("NDOName", NDOInstanceField.Data);
                    }
                }
            }

		}


        public override void WebPartCustomAction(object sender, PERS.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);

            switch (e.Action.Name)
            {
                case "ViewButton":
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
                    break;
                case "AttachButton":
                    {
                        ScriptManager.RegisterStartupScript(Page.Form, GetType(), "AttachButton", " $('#ctl00_WebPartManager_DocAttachManagementResults_AttachButton').click();", true);
                        break;
                    }
                case "DetachButton":
                    {
                        ScriptManager.RegisterStartupScript(Page.Form, GetType(), "DetachButton", " $('#ctl00_WebPartManager_DocAttachManagementResults_DetachButton').click();", true);
                        
                        break;
                    }
            }

        }
        
		# endregion



		#region functions

        protected virtual void ClearAllFields()
		{
			AttachedDocumentsGrid.ClearData();
			NDOInstanceField.ClearData();
			RDOInstanceField.ClearData();
			ContainerField.ClearData();
			IsRDOField.ClearData();		
			DocumentInstanceField.ClearData();
			StoredFileNameField.ClearData();
			SelectedObjectTypeIdField.ClearData();
			SelectedObjectTypeField.ClearData();
			SelectedInstanceField.ClearData();
			SelectedContainerInstanceField.ClearData();
			ObjectTypeIdField.ClearData();
			FromTimeStampIdField.ClearData();
			ToTimeStampField.ClearData();
			IncludeAttachedField.CheckControl.Checked = true;
			IncludeDetachedField.ClearData();
			ShowOnlyRORField.ClearData();
            DocumentTypeField.ClearData();
			DocumentNameField.ClearData();
			EmployeeFilterField.ClearData();
			FileExtensionField.ClearData();
			AttachedDocumentHistoryField.ClearData();
			AttachAsRORField.ClearData ();
			UseRORField.ClearData();
            SearchBtn.Enabled = false;
		}

        protected virtual bool AnyFieldSelected()
		{
			if (!ObjectTypeIdField.IsEmpty) return true;
			if (!FromTimeStampIdField.IsEmpty) return true;
			if (!ToTimeStampField.IsEmpty) return true;
            if (!DocumentTypeField.IsEmpty) return true;
			if (!DocumentNameField.IsEmpty) return true;
			if (!EmployeeFilterField.IsEmpty) return true;
			if (!FileExtensionField.IsEmpty) return true;
			return false;
		}

        protected virtual DocumentRefInfo DownloadDocument()
        {
            ResultStatus resultStatus;
            var documentRev = (RevisionedObjectRef)DocumentInstanceField.Data;

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
			return null;
		}

		#endregion


	}
}
