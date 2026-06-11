//Copyright Siemens 2023
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using Newtonsoft.Json;
using Camstar.WebPortal.Constants;
using Camstar.WCF.Services;
using System.Web;
using System.Threading.Tasks;
using System.Net.Http;
using Camstar.WebPortal.Personalization;
using System.Data;
using System.Linq;
using Camstar.WebPortal.Helpers;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    /// <summary>
    /// 
    /// </summary>
    public class FactoryMaint : FhmMaint
    {
        #region Controls

        protected virtual CWC.CheckBox ShouldDisplayGeneralMessage
        {
            get { return Page.FindCamstarControl("ObjectChanges_DisplayGeneralMessage") as CWC.CheckBox; }
        }

        protected virtual CWC.TextEditor GeneralMessage
        {
            get { return Page.FindCamstarControl("GeneralMessageTextEditor") as CWC.TextEditor; }
        }

        //TODO: following properties only needed for testing SWAC popup.
        protected virtual CWC.TextBox FactoryName { get { return Page.FindCamstarControl("NameTxt") as CWC.TextBox; } }
        protected virtual CWC.NamedObject Enterprise { get { return Page.FindCamstarControl("Entrprise") as CWC.NamedObject; } } //incorrect spelling is the actual control name.

        #endregion

        #region Protected Functions
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                //rizal:check if there is enterprise from FHM
                if (!string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.ParentTitle_Mod]))
                {
                    string parentTitle = Page.Request.QueryString[QueryStringConstants.ParentTitle_Mod];
                    Enterprise.Data = parentTitle;
                    Enterprise.Enabled = false;
                }
            }
        }

        //TODO: update to get correct data when have actual SWAC component interface.
        protected override List<string> GetSwacComponentInitializationArgs(string webPartName, string buttonName)
        {
            var factory = Page.DataContract.GetValueByName("SelectedInstanceRef") as NamedObjectRef;
            string accessPermission = SrcIntegrationHelper.GetUserAccessPermission();
            string bearerToken = SrcIntegrationHelper.BearerToken;

            List<string> args = new List<string>
            {
                factory.ID,
                factory.Name,
                "SITE",
                accessPermission,
                bearerToken
            };
            return args;
        }

        #endregion

        #region Public Functions

        public override void GetInputData(Service serviceData)
        {
            if (ShouldDisplayGeneralMessage.CheckControl.Checked)
            {
                try
                {
                    char[] buffer = GeneralMessage.TextControl.Text.ToCharArray();
                    StringBuilder sb = new StringBuilder();

                    bool inTag = false;
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        if (buffer[i].Equals('<'))
                            inTag = true;

                        if (!inTag)
                            sb.Append(buffer[i]);

                        if (buffer[i].Equals('>'))
                            inTag = false;

                    }

                    string s = sb.ToString();

                    if (sb.ToString().Equals(""))
                        GeneralMessage.Data = null;
                }
                catch (Exception) { }
            }

            base.GetInputData(serviceData);
        }

        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);

            var actionName = Page.PortalContext.DataContract.GetValueByName<string>("ActionName");

            // execution only in copy action, prevent delete action excute calling API
            if (status.IsSuccess && Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument")
            {
                // set ActionName in DataContract back to null
                Page.PortalContext.DataContract.SetValueByName("ActionName", null);

                var instanceId = Page.PortalContext.DataContract.GetValueByName<string>("InstanceId");

                if (actionName == "Copy")
                {
                    var newInstanceName = Page.PortalContext.DataContract.GetValueByName<string>("SuggestedInstanceName");

                    if (!string.IsNullOrEmpty(newInstanceName))
                    {
                        // get new instanceId using query service
                        var suggestedInstanceId = GetFactoryIdByName(newInstanceName.ToString());

                        // in factory, factorylevel will set to site
                        CopyResourceSettings(instanceId, suggestedInstanceId, "SITE");
                    }
                }
                else if (actionName == "Delete")
                {
                    DeleteResourceSettings(instanceId);
                }
            }
        }

        /// <summary>
        /// TODO - remove
        /// </summary>
        public void ShowAPIConnectionError()
        {
            var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
            var notification = String.Format("alert('{0}','Warning!', null);", labelCache.GetLabelByName("SRCAPI_ConnectionFailed").Value);
            System.Web.UI.ScriptManager.RegisterStartupScript(this, Page.GetType(), "Warning", notification, true);
            Page.DisplayWarning(notification);
        }

        /// <summary>
        /// Use query service to get ID of the Factory with the given name
        /// </summary>
        /// <param name="factoryName"></param>
        /// <returns>ID of the given Factory</returns>
        protected string GetFactoryIdByName(string factoryName)
        {
            string factoryId = "";

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var queryService = new QueryService(session.CurrentUserProfile);
            var qParameters = new QueryParameters() 
            {
                Parameters = new QueryParameter[]
                {
                    new QueryParameter {Name="FactoryName", Value=factoryName },
                }
            };

            var recordSet = new RecordSet();
            var resultStatus = queryService.Execute("GetFactoryIdByName", qParameters, new QueryOptions(), out recordSet);
            if (resultStatus.IsSuccess && recordSet.Rows != null && recordSet.Rows.Length > 0)
            {
                factoryId = recordSet.Rows[0].Values[0].ToString();
            }

            return factoryId;
        }
    
        #endregion
    }

}
