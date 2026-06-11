/* Copyright 2023 Siemens */
using System;
using System.IO;
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
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.HtmlControls;
using Camstar.WebPortal.FormsFramework;


/// <summary>
/// Summary description for RecipeMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class SS_RecipeMaint : MatrixWebPart
    {
        JQDataGrid _gridSubRecipes { get { return Page.FindCamstarControl("ObjectChanges_SubRecipes") as JQDataGrid; } }

        #region Public Methods

        public override void GetInputData(Service serviceData)
        {
            
            _attachMode = AttachmentMode.UploadLocalFile;
            try
            {
                string strfilePath = FileInput.UploadFilePath;
                if (UploadField.Data != null && !string.IsNullOrEmpty((string)UploadField.Data))
                    StoredFileNameField.Data = new FileInfo((string)UploadField.Data).Name;
            }
            catch (Exception ex) { }
            

            base.GetInputData(serviceData);
            if (serviceData.GetType() == typeof(DocumentMaint) ||
                serviceData.GetType() == typeof(RecipeMaint) ||
                serviceData.GetType() == typeof(ReportTemplateMaint))
            {

                RecipeChanges changes = (serviceData as RecipeMaint).ObjectChanges;
                if (_attachMode == AttachmentMode.UploadLocalFile)
                {
                    if (!UploadField.IsEmpty && UploadToDB.CheckControl.Checked)
                    {
                        string configFolder = CWC.FileInput.GetUploadFolder();
                        if (Directory.Exists(configFolder))
                            _folder = new DirectoryInfo(configFolder);

                        string fileName = (string)StoredFileNameField.Data;
                        if (_folder != null)
                        {
                            changes.FileLocation = _folder.FullName;
                            _file = new FileInfo(_folder.FullName + "\\" + fileName);
                            if (_file != null)
                                changes.FileName = _file.Name;
                        }

                        changes.UploadFile = true;
                        changes.Identifier = (string)UploadField.Data;
                    }
                    else
                    {
                        // leave fields unchanged
                        if (_formMode == FormMode.ReportTemplate && !UploadField.IsEmpty && UploadToDB.CheckControl.Checked)
                        {
                            changes.Identifier = (string)UploadField.Data;
                            changes.UploadFile = false;
                        }
                        else
                        {
                            changes.Identifier = null;
                        }
                        changes.FileLocation = null;
                        changes.UploadFile = null;
                    }
                    //BrowseFileField.ClearData();
                }
                else
                {
                    //changes.Identifier = (string)BrowseFileField.Data;
                    changes.FileLocation = string.Empty;
                    changes.FileName = string.Empty;
                    changes.FileVersion = (string)FileVersionField.Data;
                    //UploadField.ClearData();
                    StoredFileNameField.ClearData();

                }
            }


        }


        public void DocumentActionsButton_Click(object sender, EventArgs e)
        {

            //string scriptstring = "alert('Welcome');";
            //ScriptManager.RegisterStartupScript((sender as Control), this.GetType(), "alert", scriptstring, true); 

            //ScriptManager.RegisterClientScriptInclude(this, typeof(string), "Document", ResolveClientUrl("~/Scripts/Document_VP.js"));

            var fileName = DownloadDocument();
            if (!string.IsNullOrEmpty(fileName))
            {
                ScriptManager.RegisterStartupScript(this, Page.GetType(), "opendocument", string.Format("window.open('DownloadFile.aspx?viewdocfile={0}');", fileName), true);
            }
            _attachMode = AttachmentMode.UploadLocalFile;
            
        }

        public override void ClearValues(Service serviceData)
        {
            base.ClearValues();

            StoredFileNameField.ClearData();
            FileVersionField.ClearData();
            _attachMode = AttachmentMode.NotSet;
           // UploadFileTypeField.ClearData();

        }

        #endregion

        #region Protected methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
                
            DocumentActionsButton.Attributes.Add("actionType", "SubmitAction");
            DocumentActionsButton.Click += DocumentActionsButton_Click;

             if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument" && !string.IsNullOrEmpty(Page.PortalContext.DataContract.GetValueByName<string>("Recipe_ReturnedValueDM")))
                {

                    var RecipeVal = Page.PortalContext.DataContract.GetValueByName<string>("Recipe_ReturnedValueDM");                    
                    var RecipeRev = Page.PortalContext.DataContract.GetValueByName<string>("Recipe_ReturnedRevisonDM");                    
                    var rowid = Page.PortalContext.DataContract.GetValueByName<string>("Recipe_SelectedRowIdDM");

                    if (!string.IsNullOrEmpty(rowid))
                    {                        
                        int iRowId = int.Parse(rowid);
                        var data = _gridSubRecipes.Data as SubRecipesChanges[];

                        if (data[iRowId].SubRecipe == null)
                        {
                            data[iRowId].SubRecipe = new RevisionedObjectRef();
                        }
                            data[iRowId].SubRecipe.Name = RecipeVal;
                            data[iRowId].SubRecipe.Revision = RecipeRev;
                            String SubRecipes = RecipeVal + ":" + RecipeRev;
                            String SubRecipe = Convert.ToString(data[iRowId].SubRecipe);
                            SubRecipe = SubRecipes;
                        
                    }

                    // clear the data contracts as the mess with the data loading of grid values
                    Page.PortalContext.DataContract.SetValueByName("Recipe_ReturnedValueDM", null);
                    Page.PortalContext.DataContract.SetValueByName("Recipe_ReturnedRevisonDM", null);
                    Page.PortalContext.DataContract.SetValueByName("Recipe_SelectedRowIdDM", null);
                }
            
            
        }

        protected override void OnPreRender(System.EventArgs e)
        {
            
            if (_attachMode == AttachmentMode.UploadLocalFile || _formMode == FormMode.ReportTemplate)
                _attachMode = AttachmentMode.UploadLocalFile;              

            if (StoredFileNameField.IsEmpty)
            {
                DocumentActionsButton.Enabled = false;
            }
            else
            {
                DocumentActionsButton.Enabled = true;
            }

            base.OnPreRender(e);
        }

        #endregion

        #region Private Methods

        private string GetLabelValue(string labelName)
        {
            string lblValue = "";

            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
            Camstar.WCF.ObjectStack.Label label = labelCache.GetLabelByName(labelName);
            if (label.IsEmpty)
                lblValue = label.DefaultValue;
            else
                lblValue = label.Value;

            return lblValue;
        }

        private string DownloadDocument()
        {
            string retVal = null;
            string configFolder = CamstarPortalSection.Settings.DefaultSettings.UploadDirectory;
            if (Directory.Exists(configFolder))
            {
                var dirInfo = new DirectoryInfo(configFolder);
                var documentRev = GetRevObjectRef();

                var data = new RecipeMaint { ObjectToChange = documentRev };
                var data1 = new RecipeMaint { ObjectChanges = new RecipeChanges { FileLocation = dirInfo.FullName } };
                var request = new RecipeMaint_Request();
                request.Info = new RecipeMaint_Info ();
                request.Info.ObjectChanges = new RecipeChanges_Info ();
                request.Info.ObjectChanges.FileName = new Info(true);

                RecipeMaint_Result result;
                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var serv = Page is IForm ? (Page as IForm).Service.GetService<RecipeMaintService>() : new RecipeMaintService (session.CurrentUserProfile);
                serv.BeginTransaction();
                serv.Load(data);
                serv.DownloadFile(data1);
                var resultStatus = serv.CommitTransaction(request, out result);

                if (resultStatus.IsSuccess)
                {
                    if (result != null)
                    {
                        string fileName = result.Value.ObjectChanges.FileName != null ? result.Value.ObjectChanges.FileName.Value : null;
                        if (!string.IsNullOrEmpty(fileName))
                            retVal = fileName;
                    }
                }
                else
                    (Page as IForm).Page.DisplayMessage(resultStatus);
            }
            else
                (Page as IForm).Page.DisplayMessage(new ResultStatus("Shared folder '" + configFolder + "' does not exist.", false));
            return retVal;
        }

        private RevisionedObjectRef GetRevObjectRef()
        {
            var instanceHeaderWp = Page.Manager.WebParts["MDL_InstanceHeader"];
            if (instanceHeaderWp != null)
            {
                var nameTxt = instanceHeaderWp.FindControl("NameTxt") as CWC.TextBox;
                var revTxt = instanceHeaderWp.FindControl("RevisionTxt") as CWC.TextBox;
                var isROR = instanceHeaderWp.FindControl("IsRORChk") as CWC.CheckBox;
                if (nameTxt != null && revTxt != null)
                {
                    var name = nameTxt.Data == null ? "" : nameTxt.Data.ToString();
                    var rev = revTxt.Data == null ? "" : revTxt.Data.ToString();
                    bool useROR = isROR.Data == null ? false : (bool)isROR.Data;
                    return WSObjectRef.AssignRevisionedObject(name, rev, useROR, "Recipe");
                }
            }
            return null;
        }

        #endregion

        #region Controls

        protected CWC.Button DocumentActionsButton
        {
            get { return Page.FindCamstarControl("DocumentActionButton") as CWC.Button; }
        }

        protected CWC.FileInput UploadField
        {
            get { return Page.FindCamstarControl("FileInput") as CWC.FileInput; }
        }

        protected CWC.CheckBox UploadToDB
        {
            get { return Page.FindCamstarControl("ObjectChanges_UploadFile") as CWC.CheckBox; }
        }

        protected CWC.TextBox StoredFileNameField
        {
            get { return Page.FindCamstarControl("ObjectChanges_FileName") as CWC.TextBox; }
        }

        protected CWC.TextBox FileVersionField
        {
            get { return Page.FindCamstarControl("ObjectChanges_FileVersion") as CWC.TextBox; }
        }


        #endregion

        #region Properties

        private string _identifier
        {
            get
            {
                return ViewState["Identifier"] as string;
            }
            set
            {
                ViewState["Identifier"] = value;
            }
        }

        #endregion

        #region Private Member Variables

        private AttachmentMode _attachMode = AttachmentMode.NotSet;
        private FormMode _formMode;
        private DirectoryInfo _folder = null;
        private FileInfo _file = null;

        #endregion

        private enum AttachmentMode
        {
            NotSet,
            UploadLocalFile,
            UploadRemoteFile,
            CancelSubmit
        }

        private enum FormMode
        {
            Document,
            ReportTemplate
        }



    }
}



