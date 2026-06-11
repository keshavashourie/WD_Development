//Copyright Siemens 2022
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using Newtonsoft.Json;
using System.Web;
using Camstar.WCF.Services;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.Constants;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Camstar.WebPortal.Helpers;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    /// <summary>
    /// 
    /// </summary>
    public class isInventoryLocation : FhmMaint
    {
        #region Controls
        //TODO: following properties only needed for testing SWAC popup.
        protected virtual CWC.TextBox InventoryLocationName { get { return Page.FindCamstarControl("NameTxt") as CWC.TextBox; } }

        protected virtual CWC.NamedObject ParentResource { get { return Page.FindCamstarControl("ObjectChanges_isParentResource") as CWC.NamedObject; } }
        #endregion

        #region Protected Functions

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                if (!string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.NodeDefinition_Mod]))
                {
                    string name = Page.Request.QueryString[QueryStringConstants.NodeDefinition_Mod];
                    string parentTitle = string.Empty;
                    if (!string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.ParentTitle_Mod]))
                    {
                        parentTitle = Page.Request.QueryString[QueryStringConstants.ParentTitle_Mod];

                        ParentResource.Data = parentTitle;//.Split('-')[0];
                        ParentResource.Enabled = false;
                    }
                }
            }

        }

        protected override List<string> GetSwacComponentInitializationArgs(string webPartName, string buttonName)
        {
            var location = Page.DataContract.GetValueByName("SelectedInstanceRef") as NamedObjectRef;
            string accessPermission = SrcIntegrationHelper.GetUserAccessPermission();
            string bearerToken = SrcIntegrationHelper.BearerToken;

            List<string> args = new List<string>
            {
                location.ID,
                location.Name,// InventoryLocationName.Data as string,
                "EQUIPMENT",
                accessPermission,
                bearerToken
            };

            return args;
        }

        #endregion

        #region Public Functions
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);

            if (!status.IsSuccess || Page.EventArgument != "FloatingFrameSubmitParentPostBackArgument")
                return;

            var actionName = Page.PortalContext.DataContract.GetValueByName<string>("ActionName");
            // set ActionName in DataContract back to null
            Page.PortalContext.DataContract.SetValueByName("ActionName", null);

            var instanceId = Page.PortalContext.DataContract.GetValueByName<string>("InstanceId");

            if (actionName == "Copy")
            {
                var newInstanceName = Page.PortalContext.DataContract.GetValueByName<string>("SuggestedInstanceName");

                if (!string.IsNullOrEmpty(newInstanceName))
                {
                    // get new instanceId using query service
                    var newInstanceId = GetInventoryLocationIdByName(newInstanceName.ToString());

                    CopyResourceSettings(
                        instanceId,
                        newInstanceId,
                        "EQUIPMENT");
                }
            }
            else if (actionName == "Delete")
            {
                DeleteResourceSettings(instanceId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isInventoryLocationName"></param>
        /// <returns>Empty string if unable to get Inventory Location ID</returns>
        public string GetInventoryLocationIdByName(string isInventoryLocationName)
        {
            string inventoryLocationId = "";

            var recordSet = new RecordSet();
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var queryService = new QueryService(session.CurrentUserProfile);
            var qParameters = new QueryParameters() 
            {
                Parameters = new QueryParameter[]
                {
                    new QueryParameter {Name="isInventoryLocationName", Value=isInventoryLocationName },
                }
            };

            var resultStatus = queryService.Execute("isGetisInventoryLocationIdByName", qParameters, new QueryOptions(), out recordSet);
            if (resultStatus.IsSuccess && recordSet.Rows != null && recordSet.Rows.Length > 0)
            {
                inventoryLocationId = recordSet.Rows[0].Values[0].ToString();
            }

            return inventoryLocationId;
        }

        #endregion
    }

}