//Copyright Siemens 2024
using System;
using System.Collections.Generic;
using System.Text;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using Newtonsoft.Json;
using Camstar.WebPortal.Constants;
using System.Web;
using Camstar.WCF.Services;
using Camstar.WebPortal.Personalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Data;
using System.Linq;
using Camstar.WebPortal.Helpers;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    /// <summary>
    /// 
    /// </summary>
    public class ResourceMaint : FhmMaint
    {
        #region Controls
        //TODO: following properties only needed for testing SWAC popup.
        protected virtual CWC.TextBox ResourceName { get { return Page.FindCamstarControl("NameTxt") as CWC.TextBox; } }
        protected virtual CWC.NamedObject Factory { get { return Page.FindCamstarControl("Factory") as CWC.NamedObject; } }
        protected virtual CWC.DropDownList FactoryLevel { get { return Page.FindCamstarControl("FactoryLevel") as CWC.DropDownList; } }
        protected virtual CWC.NamedObject ParentResource { get { return Page.FindCamstarControl("ParentResource") as CWC.NamedObject; } }
        protected virtual CWC.Button ShowSwacPopup { get { return Page.FindCamstarControl("ShowSwacPopup") as CWC.Button; } }     

        // Implemented to cater Synergy Project 
        protected virtual CWC.NamedObject ResourceType { get { return Page.FindCamstarControl("ResourceType") as CWC.NamedObject; } }
        protected virtual CWC.NamedObject PrintQueue { get { return Page.FindCamstarControl("PrintQueue") as CWC.NamedObject; } }
        protected virtual CWC.NamedObject MaintenanceClass { get { return Page.FindCamstarControl("MaintenanceClass") as CWC.NamedObject; } }
        protected virtual CWC.NamedObject ToolPlan { get { return Page.FindCamstarControl("ObjectChanges_ToolPlan") as CWC.NamedObject; } }
        protected virtual CWC.DropDownList ClearanceLevel { get { return Page.FindCamstarControl("ClearanceLevel") as CWC.DropDownList; } }
        protected virtual JQDataGrid ChildResources { get { return Page.FindCamstarControl("ChildResources") as JQDataGrid; } }


        private static string FactoryLevelArea = "1";
        private static string FactoryLevelCell = "2";
        private static string FactoryLevelEquipment = "3";

        private static string NodeDefinitionAreas = "Area";
        private static string NodeDefinitionCells = "Cell";
        private static string NodeDefinitionEquipments = "Equipment";

        private Dictionary<string, string> portalHelpFile = new Dictionary<string, string> {
            { "ToolMaint", @"onlinehelpoutput/portalmodeling_Help/PortalModeling_CSH.htm#ModelingObjects/Defining_Tools.htm"},
        };

        #endregion

        #region Protected Functions

        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            

            if (Page.PrimaryServiceType == "ResourceMaint")
            {
                ToolPlan.Enabled = true;
            }

            if (Page.PrimaryServiceType == "ResourceMaint" || Page.PrimaryServiceType == "ToolMaint" || Page.PrimaryServiceType == "FeederMaint" || Page.PrimaryServiceType == "FeederBankMaint")
            {
                ResourceType.Visible = true;
                PrintQueue.Visible = true;
                MaintenanceClass.Visible = true;
                ClearanceLevel.Visible = true;
                ParentResource.Visible = true;
                ChildResources.Visible = true;
            }


        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            

            FactoryLevel.DataBinding += FactoryLevel_DataBinding;
            

            if (!Page.IsPostBack)
            {
                if (!string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.NodeDefinition_Mod]))
                {
                    string name = Page.Request.QueryString[QueryStringConstants.NodeDefinition_Mod];
                    string parentTitle = string.Empty;
                    if (!string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.ParentTitle_Mod]))
                    {
                        parentTitle = Page.Request.QueryString[QueryStringConstants.ParentTitle_Mod];

                        if (name.Equals(NodeDefinitionAreas))
                        {
                            Factory.Data = parentTitle;
                            Factory.Enabled = false;
                        }
                        else
                        {
                            ParentResource.Data = parentTitle;//.Split('-')[0];
                            ParentResource.Enabled = false;
                        }
                    }
                }
            }
        }

        protected virtual void FactoryLevel_DataBinding(object sender, EventArgs e)
        {
            ShowSwacPopup.Visible = false;

            if (!string.IsNullOrEmpty(Page.Request.QueryString[QueryStringConstants.NodeDefinition_Mod]))
            {
                string name = Page.Request.QueryString[QueryStringConstants.NodeDefinition_Mod];
                if (name.Contains(NodeDefinitionAreas))
                {
                    FactoryLevel.Data = FactoryLevelArea;
                }
                else
                {
                    if (name.Contains(NodeDefinitionCells))
                    {
                        FactoryLevel.Data = FactoryLevelCell;
                    }
                    else if (name.Contains(NodeDefinitionEquipments))
                    {
                        FactoryLevel.Data = FactoryLevelEquipment;
                    }
                }
                FactoryLevel.Enabled = false;
            }

        }

        //TODO: update to get correct data when have actual SWAC component interface.
        protected override List<string> GetSwacComponentInitializationArgs(string webPartName, string buttonName)
        {
            var resource = Page.DataContract.GetValueByName("SelectedInstanceRef") as NamedObjectRef;
            string level;
            string accessPermission = SrcIntegrationHelper.GetUserAccessPermission();
            string bearerToken = SrcIntegrationHelper.BearerToken;
            int facLevel = FactoryLevel.Data != null ? (int)FactoryLevel.Data : 0;

            if (facLevel == 1)
                level = "AREA";
            else if (facLevel == 2)
                level = "LINE";
            else if (facLevel == 3)
                level = "EQUIPMENT";
            else
                level = "NOT_SET";

            List<string> args = new List<string>
            {
                resource.ID,
                resource.Name,
                level,
                accessPermission,
                bearerToken
            };

            return args;
        }

        protected void DisplaySwacButton()
        {
            string url = SwacWP.SwacComponentUrl;
            if (!string.IsNullOrEmpty(url) && ResourceName.Data != null && !string.IsNullOrWhiteSpace(ResourceName.Data.ToString()))
            {
                // Make visible the Swac buton when the FactoryLevel is set.
                //      FactoryLevel.Data = 0 or null means not set
                //      If Swac url not set, the container is not visible, so below will have no affect
                int facLevel = FactoryLevel.Data != null ? (int)FactoryLevel.Data : 0;
                if (facLevel == 0)
                    ShowSwacPopup.Visible = false;
                else
                    ShowSwacPopup.Visible = true;
            }
            else
                ShowSwacPopup.Visible = false;
        }

        protected string LoadSRCExternalPermissions(string extPermission)
        {
            Service serviceData = new Service();
            FrameworkSession fs = FrameworkManagerUtil.GetFrameworkSession(this.Page.Session);
            QueryService queryService = new QueryService(fs.CurrentUserProfile);

            string empname = Page.Session["UserName"].ToString();
            string application = "Workspace";
            string module = "SRC";

            QueryParameters queryParam = new QueryParameters();
            queryParam.Parameters = new QueryParameter[3];
            queryParam.Parameters[0] = new QueryParameter();
            queryParam.Parameters[0].Name = "EmployeeName";
            queryParam.Parameters[0].Value = empname;
            queryParam.Parameters[1] = new QueryParameter();
            queryParam.Parameters[1].Name = "Application";
            queryParam.Parameters[1].Value = application;
            queryParam.Parameters[2] = new QueryParameter();
            queryParam.Parameters[2].Name = "Module";
            queryParam.Parameters[2].Value = module;

            RecordSet record = new RecordSet();
            ResultStatus Result = queryService.Execute("PermissionInquiry_GetExternalPermissions", queryParam, new QueryOptions(), out record);

            if (Result.IsSuccess && record.TotalCount.ToString() != null)
            {
                DataTable resultDt = record.GetAsDataTable();

                if (resultDt != null && resultDt.Rows != null && resultDt.Rows.Count > 0)
                {
                    var exist = resultDt.AsEnumerable()
                                        .Where(x => x.Field<string>("ExternalPermissionName")
                                        .Equals(extPermission)).Count() > 0;

                    if (exist)
                    {
                        // We used to return the IPLSettingsReadOnly permission. But the name was changed
                        // to "IPL Configuration". We are not modifying SRC so we have to make this code
                        // work with the old IPLSettingsReadOnly permission setting.
                        // Read/Write access is actually the default for SRC. So if
                        // "IPL Configuration" is set as the external permission then return blank. This will 
                        // not pass any permission to SRC and therefore the user can edit data.
                        return string.Empty;
                    }
                }
            }

            // No match on the setting IPL Configuration role. Make it the user's access read only to make the system
            // work the way it used to.
            return "IPLSettingsReadOnly";
        }

        protected override void OnPreRender(System.EventArgs e)
        {
            string helpUrl = string.Empty;

            if (portalHelpFile.ContainsKey(PrimaryServiceType)) 
                helpUrl = portalHelpFile[PrimaryServiceType];

            if (!string.IsNullOrEmpty(helpUrl))
            {
                Personalization.PageContent content = Page.Model.PublishedContent as PageContent;
                if (content != null)
                {
                    content.HelpFileURL = helpUrl;
                }
            }
            base.OnPreRender(e);
        }
        #endregion

        #region Public Functions
        public override void DisplayValues(Service serviceData)
        {
            base.DisplayValues(serviceData);

            DisplaySwacButton();
        }

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
                var suggestedInstanceName = Page.PortalContext.DataContract.GetValueByName<string>("SuggestedInstanceName");

                // when the new instance name not null and factorylevel not null
                if (!string.IsNullOrEmpty(suggestedInstanceName) && !string.IsNullOrEmpty(FactoryLevel.Text))
                {
                    // get new instanceId using query service
                    var suggestedInstanceId = GetResourceIdByName(suggestedInstanceName.ToString());

                    CopyResourceSettings(
                        instanceId,
                        suggestedInstanceId,
                        FactoryLevel.Text == "Cell" ? "Line" : FactoryLevel.Text);  // replace with Line if is Cell FactoryLevel
                }
            }
            else if(actionName == "Delete")
            {
                DeleteResourceSettings(instanceId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns>Empty string if unable to get Resource ID</returns>
        public string GetResourceIdByName(string resourceName)
        {
            string resourceId = "";
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

            var queryService = new QueryService(session.CurrentUserProfile);
            var qParameters = new QueryParameters()
            {
                Parameters = new QueryParameter[]
                {
                    new QueryParameter {Name="ResourceName", Value=resourceName },
                }
            };

            var recordSet = new RecordSet();
            var resultStatus = queryService.Execute("GetResourceIdByName", qParameters, new QueryOptions(), out recordSet);
            if (resultStatus.IsSuccess && recordSet.Rows != null && recordSet.Rows.Length > 0)
            {
                resourceId = recordSet.Rows[0].Values[0].ToString();
            }
            
            return resourceId;
        }

        

        #endregion

    }

}
