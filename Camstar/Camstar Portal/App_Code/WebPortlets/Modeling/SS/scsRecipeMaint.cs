//© Copyright Siemens 2024
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    /// <summary>
    /// Inherits the Recipe class and will be executed only for SEMI environment
    /// </summary>
    public class scsRecipeMaint : Recipe
    {
        #region Controls

        private CWC.CheckBox SMSpecific { get { return Page.FindCamstarControl("SMSpecific") as CWC.CheckBox; } }
        private CWC.CheckBox ELSpecific { get { return Page.FindCamstarControl("ELSpecific") as CWC.CheckBox; } }

        private JQDataGrid _gridSubRecipes { get { return Page.FindCamstarControl("ObjectChanges_SubRecipes") as JQDataGrid; } }


        #endregion

        #region Protected Functions

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                BrowseMode.TextEditControl.Enabled = false;
            }
            DocumentActionsButton.Attributes.Add("actionType", "SubmitAction");

            DocumentActionsButton.Click += DocumentActionsButton_Click; 

            if(SMSpecific.CheckControl.Checked) 
            {
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
        }
        #endregion

        #region Public Functions

        public void DocumentActionsButton_Click(object sender, EventArgs e)
        {

            if (SMSpecific.CheckControl.Checked && ELSpecific.CheckControl.Checked)
            {
                base.DocumentActionsButton_Click(sender, e);
            } 
            else if (SMSpecific.CheckControl.Checked)
            {
                string fileName = DownloadDocument();
                if (!string.IsNullOrEmpty(fileName))
                {
                    ScriptManager.RegisterStartupScript(this, Page.GetType(), "opendocument", string.Format("window.open('DownloadFile.aspx?viewdocfile={0}');", fileName), true);
                }
                _attachMode = AttachmentMode.UploadLocalFile;
            }
            else
            {
                base.DocumentActionsButton_Click(sender, e);
            }

           

        }

        #endregion

        #region Private Functions
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
                request.Info = new RecipeMaint_Info();
                request.Info.ObjectChanges = new RecipeChanges_Info();
                request.Info.ObjectChanges.FileName = new Info(true);

                RecipeMaint_Result result;
                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var serv = Page is IForm ? (Page as IForm).Service.GetService<RecipeMaintService>() : new RecipeMaintService(session.CurrentUserProfile);
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
        #endregion

        #region Constants

        #endregion

        #region Private Member Variables

        private AttachmentMode _attachMode = AttachmentMode.NotSet;
        private FormMode _formMode;
        private DirectoryInfo _folder = null;
        private FileInfo _file = null;
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

        #endregion

    }

}

