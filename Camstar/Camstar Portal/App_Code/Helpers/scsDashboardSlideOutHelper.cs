//© 2022 Siemens Product Lifecycle Management Software Inc.
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
    /// Handles server side functions to retrieve data for the sidebar slide out action on the Landing Page (via scsLandingPageCommandBar data contract)
    /// </summary>
    public class scsDashboardSlideOutHelper : CommandBarHelper
    {

        public scsDashboardSlideOutHelper()
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
            var service = new WCF.Services.EquipmentMaterialsSetupService(session.CurrentUserProfile);
            var request = new WCF.Services.EquipmentMaterialsSetup_Request();
            var result = new WCF.Services.EquipmentMaterialsSetup_Result();
            var resultStatus = new OM.ResultStatus();

            var serviceData = new OM.EquipmentMaterialsSetup
            {
                Resource = new OM.NamedObjectRef(resourceName)
            };

            request.Info = new OM.EquipmentMaterialsSetup_Info
            {
                LotsInProcess = new OM.EquipmentMaterialsDetails_Info
                {
                    Product = new OM.Info(true),
                    MaterialLotName = new OM.Info(true),
                    Qty = new OM.Info(true),
                    Qty2 = new OM.Info(true),
                    Spec = new OM.Info(true)
                }
            };

            resultStatus = service.Load(serviceData, request, out result);
            if (resultStatus.IsSuccess)
            {
                OM.EquipmentMaterialsDetails[] eqpLotList = result.Value.LotsInProcess;

                if (eqpLotList == null)
                {
                    panelData.Add("Empty", "Empty");
                    return;
                }

                TileColumn column = new TileColumn()
                {
                    Name = "EqpDetails",
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

                ControlState.Tiles = new List<TileContext>();

                foreach (OM.EquipmentMaterialsDetails lot in eqpLotList)
                {
                    ControlState.Tiles.Add(CreateEqpLotTile(lot, "EqpDetails"));
                }


                //Get Material Info
                if (session != null)
                {                  
                    var recordSet1 = new RecordSet();

                    var service1 = new QueryService(session.CurrentUserProfile);
                    
                    QueryParameters oQueryParam1 = new QueryParameters();
                    oQueryParam1.Parameters = new QueryParameter[1];
                    oQueryParam1.Parameters[0] = new QueryParameter();
                    oQueryParam1.Parameters[0].Name = "EQP";
                    oQueryParam1.Parameters[0].Value = resourceName;
                    
                    QueryOptions oQueryOptions1 = new QueryOptions();
                    oQueryOptions1.QueryType = OM.QueryType.System;
                    oQueryOptions1.StartRow = 1;

                    var resultStatus1 = service1.Execute("scsGetCoomandBarMaterial", oQueryParam1, oQueryOptions1, out recordSet1);

                    if (resultStatus1.IsSuccess)
                    {
                        if (recordSet1 != null && recordSet1.Rows != null)
                        {
                            
                            foreach (var row1 in recordSet1.Rows)
                            {
                                var tile = new TileContext
                                {
                                    ColumnName = "EqpDetails",
                                    Title = row1.Values[1].ToString(),
                                };

                                var strings = new List<string>();

                                strings.Add($"{labelValues["Quantity"]}: {row1.Values[2].ToString()} _QTY_ICON_ {row1.Values[3].ToString()} _QTY2_ICON_");
                                strings.Add($"{labelValues["Material Type"]}: {row1.Values[0].ToString()}");
                                if (!(string.IsNullOrEmpty(row1.Values[3].ToString())))
                                    strings.Add($"Exp Date: {row1.Values[4].ToString()}");
                                else
                                    strings.Add($"Exp Date: ");

                                tile.Text = strings.ToArray();
                                tile.CustomData = row1.Values[1].ToString();

                                ControlState.Tiles.Add(tile);
                            }
                        }
                    }
                }

                //Get Tool Info
                if (session != null)
                {
                    var recordSet2 = new RecordSet();

                    var service2 = new QueryService(session.CurrentUserProfile);
                    
                    QueryParameters oQueryParam2 = new QueryParameters();
                    oQueryParam2.Parameters = new QueryParameter[1];
                    oQueryParam2.Parameters[0] = new QueryParameter();
                    oQueryParam2.Parameters[0].Name = "EQP";
                    oQueryParam2.Parameters[0].Value = resourceName;

                    QueryOptions oQueryOptions2 = new QueryOptions();
                    oQueryOptions2.QueryType = OM.QueryType.System;
                    oQueryOptions2.StartRow = 1;

                    var resultStatus2 = service2.Execute("scsGetCoomandBarTool", oQueryParam2, oQueryOptions2, out recordSet2);

                    if (resultStatus2.IsSuccess)
                    {
                        if (recordSet2 != null && recordSet2.Rows != null)
                        {
                            //.Tiles.Add(CreateMatLotTile(recordSet1, "EqpDetails"));
                            foreach (var row2 in recordSet2.Rows)
                            {
                                var tile = new TileContext
                                {
                                    ColumnName = "EqpDetails",
                                    Title = row2.Values[0].ToString(),
                                };

                                var strings = new List<string>();

                                if (!(string.IsNullOrEmpty(row2.Values[1].ToString())))
                                    strings.Add($"{labelValues["ObjectCategory"]}: {row2.Values[1].ToString()}");


                                tile.Text = strings.ToArray();
                                tile.CustomData = row2.Values[0].ToString();

                                ControlState.Tiles.Add(tile);
                            }
                        }
                    }
                }

                panelData.Add("TileContext", ControlState);
            }
        }

        protected TileContext CreateEqpLotTile(OM.EquipmentMaterialsDetails lot, string tileColName)
        {
            var tile = new TileContext
            {
                ColumnName = tileColName,
                Title = lot.MaterialLotName.ToString(),
            };

            var strings = new List<string>();

            strings.Add($"{labelValues["Quantity"]}: {lot.Qty} _QTY_ICON_ {lot.Qty2} _QTY2_ICON_");
            strings.Add($"{labelValues["ProductDisplay"]}: {lot.Product}");
            strings.Add($"{labelValues["WorkflowStep"]}: {lot.Spec}");

            tile.Text = strings.ToArray();
            tile.CustomData = lot.MaterialLotName.ToString();

            return tile;
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
                { "Quantity", "Web_Quantity" },
                { "QtyConsumed", "ConsumeMaterialsDetails_QtyConsumed" },
                { "Material Type", "CSICDOName_MaterialType" },
                { "Exp Date", "LotAttributes_MaterialExpiryTimestamp" },
                { "ObjectCategory", "Resource_ObjectCategory" }
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