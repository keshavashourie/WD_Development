using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.Utilities;
using System;
using System.Collections.Generic;
using System.Web;
using cbArgs = Camstar.WebPortal.WebPortlets.CommandBarCallBackArgs;
using OM = Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.Helpers
{
    /// <summary>
    /// Handles server side functions to retrieve data for the sidebar slide out action on the Landing Page (via LandingPageCommandBar data contract)
    /// </summary>
    public class DashboardSlideOutHelper : CommandBarHelper
    {

        public DashboardSlideOutHelper()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public override PanelNameValueData GetPanelData(cbArgs callbackArgs)
        {
            PanelNameValueData panelData = base.GetPanelData(callbackArgs);
            if (panelData != null)
            {
                LoadLabels();

                if (callbackArgs.fun == "getContainerInfo")
                {
                    GetContainerInfo(panelData, callbackArgs.containerName, callbackArgs.filter);
                }
                else if (callbackArgs.fun == "getPMInfo")
                {
                    GetEquipmentPMInfo(panelData, callbackArgs.containerName);
                }
                else if (callbackArgs.fun == "getEqpInfo")
                {
                    GetEquipmentInfo(panelData, callbackArgs.containerName);
                }
            }
            return panelData;
        }

        #region Lot Details
        public virtual void GetContainerInfo(PanelNameValueData panelData, string containerName, string filter)
        {
            /*
            // get the session and user profile
            var fs = FrameworkManagerUtil.GetFrameworkSession();

            // init service objects
            GUIUtilityService oService = new GUIUtilityService(fs.CurrentUserProfile);
            OM.GUIUtility oServiceData = new OM.GUIUtility();
            OM.GUIUtility_Info oServiceInfo = new OM.GUIUtility_Info();
            GUIUtility_Result oServiceResult = new GUIUtility_Result();

            string sTypeNameID = "";

            oServiceData.QueryName = "_WIPMain_Lot";
            oServiceData.IgnoreLotVerification = true;
            oServiceData.LotId = containerName;

            // set the TypeNameID
            sTypeNameID = oServiceData.QueryName.ToString();

            // request data
            oServiceInfo.LotId = FieldInfoUtil.RequestSelectionValue();

            // init request
            GUIUtility_Request oServiceRequest = new GUIUtility_Request();
            oServiceRequest.Info = oServiceInfo;
            // execute!
            OM.ResultStatus oResultStatus = oService.GetEnvironment(oServiceData, oServiceRequest, out oServiceResult);

            if (oResultStatus.IsSuccess)
            {
                OM.RecordSet recordSet = oServiceResult.Environment.LotId.SelectionValues;
                var labelCache = LabelCache.GetRuntimeCacheInstance();
                int i = 0;
                foreach (OM.Header header in recordSet.Headers)
                {
                    ++i;
                    if (isHidden(header.Name)) continue;
                    string columnName = labelCache.GetLabelByName(header.Label.Name).Value;
                    if (!panelData.ContainsKey(columnName))
                        panelData.Add(columnName, recordSet.Rows[0].Values[i - 1]);
                }
            }
            else
            {
                panelData.Add("Error", oResultStatus.ExceptionData.ToString());
            }
            */
        }

        private bool isHidden(string headerName)
        {
            string[] sHiddenColumnNames = new string[] { "__STYLE", "__OutputCarrier", "__SelectionId", "ProcessTimerName", "ProcessTimerRevision", "StartTimeGMT", "MinEndWarningTimeGMT", "MinWarningTimeColor", "MinEndTimeGMT", "MinTimeColor", "MaxEndWarningTimeGMT", "MaxWarningTimeColor", "MaxEndTimeGMT", "MaxTimeColor" };
            foreach (string header in sHiddenColumnNames)
            {
                if (header.Equals(headerName))
                    return true;
            }
            return false;
        }
        #endregion

        #region PM Details

        protected class PMDetails
        {
            public string Resource;
            public string MaintenanceType;
            public string MaintenanceReq;
            public string MaintenanceState;
            public string NextDueDate;
            public string NextThruputQtyDue;
            public string NextUsageCountDue;
        }

        public virtual void GetEquipmentPMInfo(PanelNameValueData panelData, string resourceName)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new WCF.Services.GetMaintenanceStatusesService(session.CurrentUserProfile);
            var serviceData = new OM.GetMaintenanceStatuses();
            serviceData.Resource = new OM.NamedObjectRef(resourceName);

            var request = new WCF.Services.GetMaintenanceStatuses_Request();
            var result = new WCF.Services.GetMaintenanceStatuses_Result();
            var resultStatus = new OM.ResultStatus();

            request.Info = new OM.GetMaintenanceStatuses_Info
            {
                MaintenanceStatus = FieldInfoUtil.RequestSelectionValue()
            };

            resultStatus = service.GetEnvironment(serviceData, request, out result);
            if (resultStatus.IsSuccess)
            {
                OM.RecordSet recordSet = result.Environment.MaintenanceStatus.SelectionValues;

                if (recordSet == null)
                {
                    panelData.Add("Empty", "Empty");
                    return;
                }

                TileColumn column = new TileColumn()
                {
                    Name = "PM",
                    Visible = true
                };

                TilesContext ControlState = new TilesContext();

                ControlState.Columns = new List<TileColumnContext>();

                ControlState.Columns.Add(new TileColumnContext
                {
                    Name = column.Name,
                    Title = column.LabelText,
                    CssClass = column.CssClass,
                    ColumnStyle = column.ColumnStyle,
                    TileStyle = column.TileStyle,
                    DefaultImage = column.DefaultImage,
                    Visible = column.Visible
                });

                var ordList = new List<PMDetails>();
                foreach (OM.Row row in recordSet.Rows)
                {
                    var item = new PMDetails
                    {
                        Resource = row.Values[25],
                        MaintenanceState = row.Values[6],
                        MaintenanceType = row.Values[7],
                        MaintenanceReq = row.Values[15],
                        NextDueDate = row.Values[10],
                        NextThruputQtyDue = row.Values[16],
                        NextUsageCountDue = row.Values[31]
                    };
                    ordList.Add(item);
                }

                ControlState.Tiles = new List<TileContext>();

                foreach (PMDetails item in ordList)
                {
                    ControlState.Tiles.Add(CreatePMTile(item, "PM"));
                }

                panelData.Add("TileContext", ControlState);
            }
        }

        protected TileContext CreatePMTile(PMDetails ordListItem, string tileColName)
        {
            var tile = new TileContext
            {
                ColumnName = tileColName,
                Title = ordListItem.MaintenanceReq,
            };

            var strings = new List<string>();
            strings.Add($"{labelValues["MaintenanceState"]}: {ordListItem.MaintenanceState}");
            if (ordListItem.MaintenanceType == "Date Requirement" || ordListItem.MaintenanceType == "Recurring Date Requirement")
            {
                string date = Convert.ToDateTime(ordListItem.NextDueDate).ToString("MM/dd/yyyy hh:mm tt");
                strings.Add($"{labelValues["NextDueDate"]}: {date}");
            }
            if (ordListItem.MaintenanceType == "Thruput Requirement")
                strings.Add($"{labelValues["NextThruputQtyDue"]}: {ordListItem.NextThruputQtyDue}");
            if (ordListItem.MaintenanceType == "Usage Requirement")
                strings.Add($"{labelValues["NextUsageCountDue"]}: {ordListItem.NextUsageCountDue}");
            tile.Text = strings.ToArray();
            tile.CustomData = ordListItem.Resource;

            return tile;
        }
        #endregion

        #region Eqp Details

        public virtual void GetEquipmentInfo(PanelNameValueData panelData, string resourceName)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            panelData.Add("Empty", "Empty");
            return;

        }

        #endregion

        #region Tile Helper
        public class TilesContext
        {
            public List<TileContext> Tiles { get; set; }
            public List<TileColumnContext> Columns { get; set; }
            public string CustomData { get; set; }
            public bool UsePostback { get; set; }
            public string DefaultImage { get; set; }
        }

        public class TileColumnContext
        {
            public string Name { get; set; }
            public string Title { get; set; }
            public string CssClass { get; set; }
            public string ColumnStyle { get; set; }
            public string TileStyle { get; set; }
            public string DefaultImage { get; set; }
            public bool Visible { get; set; }
        }

        public class TileContext
        {
            public string TileStyle { get; set; }
            public string ColumnName { get; set; }
            public string Title { get; set; }
            public string[] Text { get; set; }
            public string CustomData { get; set; }
            public string Image { get; set; }
        }
        #endregion

        #region Label

        private Dictionary<string, string> labelNames;
        private Dictionary<string, string> labelValues;

        protected void SetLabelNames()
        {
            labelNames = new Dictionary<string, string>
            {
                { "Resource", "Lbl_Resource" },
                { "MaintenanceState", "GetMaintenanceStatusDetails_MaintenanceState" },
                { "MaintenanceType", "GetMaintenanceStatusDetails_MaintenanceType" },
                { "MaintenanceReq", "AssignedMaintReqMaint_MaintenanceReq" },
                { "NextDueDate", "MaintenanceStatus_NextDueDate" },
                { "NextThruputQtyDue", "GetMaintenanceStatusDetails_NextThruputQtyDue" },
                { "NextUsageCountDue", "GetMaintenanceStatusDetails_ss_NextUsageCountDue"},
                { "ProductDisplay", "CSICDOName_Product" },
                { "WorkflowStep", "CSICDOName_Step" },
                { "Quantity", "Web_Quantity" }
            };
        }

        protected void LoadLabels()
        {
            // create mapping of friendly label names to actual label names
            SetLabelNames();

            // load labels to the cache
            var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
            LabelList labelList = new LabelList();
            foreach (KeyValuePair<string, string> item in labelNames)
            {
                labelList.Add(new Label(item.Value));
            }
            labelCache.GetLabels(labelList);

            // create mapping of friendly names to lable values
            labelValues = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> item in labelNames)
            {
                var label = labelCache.GetLabelByName(item.Value);
                labelValues.Add(item.Key, label.Value);
            }
        }

        #endregion
    }
}