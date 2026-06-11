using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.WebPortlets.Shopfloor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebClientPortal;
using static Camstar.WebPortal.FormsFramework.WebControls.TileContainer;
using cbArgs = Camstar.WebPortal.WebPortlets.CommandBarCallBackArgs;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.Helpers
{
    /// <summary>
    /// Summary description for scsSimpleWipMainSlideOutHelper
    /// </summary>
    public class scsSimpleWipMainSlideOutHelper : CommandBarHelper
    {
        public scsSimpleWipMainSlideOutHelper()
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

                if (callbackArgs.fun == "getLotDetails")
                {
                    GetLotDetails(panelData, callbackArgs.containerName, callbackArgs.filter);
                }
                else if (callbackArgs.fun == "getEqpStatus")
                {
                    GetEquipmentStatus(panelData, callbackArgs.containerName);
                }
            }
            return panelData;
        }

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
                { "EquipmentName", "CSICDOName_Equipment" },
                { "EquipmentDescription", "SelVal_Description" },
                { "Status", "Status" },
                { "CurrentAvailability", "Resource_ResourceIcon" },
                { "LotCount", "Resource_LotCount" },
                { "ShowingEntriesLbl", "Lbl_GridShowingToOfItems" }
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

        #region Lot Details

        public virtual void GetLotDetails(PanelNameValueData panelData, string containerName, string filter)
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

        #region Equipment Status

        private class EquipmentItem
        {
            public string EquipmentName;
            public string EquipmentDescription;
            public string EquipmentGrp;
            public string EquipmentStatus;
            public bool EquipmentAvailability;
            public bool RequiredPM;
            public int LotCount;
            public string EquipmentCategory;
            public string EquipmentType;
            public string Factory;
        }

        private void GetEquipmentStatus(PanelNameValueData panelData, string containerName)
        {
            string lineAssigWorkCenter = String.Empty, lineAssigResource = String.Empty, lineAssigWorkStation = String.Empty, lineAssigFactory = String.Empty;

            ApolloPortalService appolloSvc = new ApolloPortalService();
            var appolloSettings = new ApolloPortalService.ApolloSettings();
            ResultStatus status = appolloSvc.GetApolloSettings(out appolloSettings);
            if (status.IsSuccess)
            {
                object resource = appolloSettings.Resource;
                object operation = appolloSettings.Operation;
                object workcenter = appolloSettings.Workcenter;
                object workstation = appolloSettings.Workstation;

                //Retrieve Line Assigment Data
                lineAssigWorkCenter = (workcenter != null ? workcenter.ToString() : String.Empty);
                lineAssigResource = (resource != null ? resource.ToString() : String.Empty);
                lineAssigWorkStation = (workstation != null ? workstation.ToString() : String.Empty);
                //lineAssigFactory = (factory != null ? factory.ToString() : String.Empty);
            }

            /*
            scsSimpleWIPMain simple_wipmain = new scsSimpleWIPMain();
            string equipmentSelected = String.Empty,
                equipmentGroupSelected = simple_wipmain.GetEquipmentGroupSelected(lineAssigWorkCenter);
                */
            string equipmentSelected = String.Empty;
            string equipmentGroupSelected = String.Empty;

            if (!String.IsNullOrEmpty(lineAssigWorkStation) || !String.IsNullOrEmpty(lineAssigResource))
                equipmentSelected = (!String.IsNullOrEmpty(lineAssigWorkStation) ? lineAssigWorkStation : lineAssigResource);

            List<EquipmentItem> equipmentList = new List<EquipmentItem>();

            if (!String.IsNullOrEmpty(equipmentSelected))
            {
                //Get selected equipment
                var fs = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                QueryService oSvc = new QueryService(fs.CurrentUserProfile);

                QueryParameters oQueryParam = new QueryParameters();
                oQueryParam.Parameters = new QueryParameter[1];
                oQueryParam.Parameters[0] = new QueryParameter();
                oQueryParam.Parameters[0].Name = "EQUIPMENT";
                oQueryParam.Parameters[0].Value = equipmentSelected;

                QueryOptions oQueryOptions = new QueryOptions();
                oQueryOptions.QueryType = OM.QueryType.User;
                oQueryOptions.StartRow = 1;

                RecordSet oQueryResult = new RecordSet();
                ResultStatus oResultStatus = new ResultStatus();

                oResultStatus = oSvc.Execute("__Equipment", oQueryParam, oQueryOptions, out oQueryResult);

                if (oResultStatus.IsSuccess && oQueryResult.Rows != null && oQueryResult.Rows.Count() > 0)
                    equipmentList = ConvertRecordSetToEqpList(oQueryResult, false);
            }
            else if (!String.IsNullOrEmpty(equipmentGroupSelected))
            {
                System.Data.DataTable resolvedEquipmentList = LoadResolvedEntries(equipmentGroupSelected);
                if (resolvedEquipmentList != null && resolvedEquipmentList.Rows != null)
                {
                    foreach (System.Data.DataRow row in resolvedEquipmentList.Rows)
                    {
                        //Get selected equipment
                        var fs = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                        QueryService oSvc = new QueryService(fs.CurrentUserProfile);

                        QueryParameters oQueryParam = new QueryParameters();
                        oQueryParam.Parameters = new QueryParameter[1];
                        oQueryParam.Parameters[0] = new QueryParameter();
                        oQueryParam.Parameters[0].Name = "EQUIPMENT";
                        oQueryParam.Parameters[0].Value = row[0].ToString();

                        QueryOptions oQueryOptions = new QueryOptions();
                        oQueryOptions.QueryType = OM.QueryType.User;
                        oQueryOptions.StartRow = 1;

                        RecordSet oQueryResult = new RecordSet();
                        ResultStatus oResultStatus = new ResultStatus();

                        oResultStatus = oSvc.Execute("__Equipment", oQueryParam, oQueryOptions, out oQueryResult);
                        List<EquipmentItem> equipmentListTemp = new List<EquipmentItem>();
                        if (oResultStatus.IsSuccess && oQueryResult.Rows != null && oQueryResult.Rows.Count() > 0)
                        {
                            equipmentListTemp = ConvertRecordSetToEqpList(oQueryResult, false);
                            equipmentList.Add(equipmentListTemp[0]);
                        }
                    }
                }

            }

            //Filter out equipments not belongs to Factory Assigned
            //lineAssigFactory = containerName;
            //if (!String.IsNullOrEmpty(lineAssigFactory))
            //    equipmentList = equipmentList.Where(r => r.Factory == lineAssigFactory).ToList();

            //Get Equipment PM Status
            if (equipmentList.Count > 0)
                GetEquipmentPMStatus(equipmentList);

            //return equipmentList;

            if (equipmentList == null)
            {
                panelData.Add("Empty", "Empty");
                return;
            }
            else if (equipmentList.Count == 0)
            {
                panelData.Add("Empty", "Empty");
                return;
            }

            //TileColumn column = new TileColumn()
            //{
            //    Name = "EqpStatus",
            //    Visible = true
            //};

            //TilesContext ControlState = new TilesContext();

            //ControlState.Columns = new List<TileColumnContext>();

            //ControlState.Columns.Add(new TileColumnContext
            //{
            //    Name = column.Name,
            //    Title = column.LabelText,
            //    CssClass = column.CssClass,
            //    ColumnStyle = column.ColumnStyle,
            //    TileStyle = column.TileStyle,
            //    DefaultImage = column.DefaultImage,
            //    Visible = column.Visible
            //});

            //ControlState.Tiles = new List<TileContext>();

            //foreach (EquipmentItem item in equipmentList)
            //{
            //    ControlState.Tiles.Add(CreateTile(item, "Equipment"));
            //}

            panelData.Add("TileContext", equipmentList);
        }

        private static System.Data.DataTable LoadResolvedEntries(string resourceGroup)
        {
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            WSDataCreator creator = new WSDataCreator();

            NamedObjectGroupMaint serviceData = creator.CreateServiceData("EquipmentGroupMaint") as NamedObjectGroupMaint;
            Maintenance_Info serviceInfo = creator.CreateServiceInfo("EquipmentGroupMaint") as Maintenance_Info;

            IMaintenanceBase service = creator.CreateService("EquipmentGroupMaint", profile) as IMaintenanceBase;
            Request request = creator.CreateObject("EquipmentGroupMaint_Request") as Request;
            Result result = creator.CreateObject("EquipmentGroupMaint_Result") as Result;
            //Page.GetInputData(serviceData);
            service.BeginTransaction();

            NamedObjectGroupMaint reqData = creator.CreateServiceData("EquipmentGroupMaint") as NamedObjectGroupMaint;

            Type type = new WCFObject(serviceData).GetFieldType("ObjectChanges");
            reqData.ObjectToChange = new NamedObjectRef(resourceGroup);
            service.Load(reqData);
            serviceData.ObjectToChange = null;

            request.Info = serviceInfo;

            type = new WCFObject(serviceInfo).GetFieldType("ObjectChanges");
            (request.Info as Maintenance_Info).ObjectChanges = WCFObject.CreateObject(type) as NamedObjectGroupChanges_Info;
            ((request.Info as Maintenance_Info).ObjectChanges as NamedObjectGroupChanges_Info).ResolvedEntries = new Info(false, true);
            ((request.Info as Maintenance_Info).ObjectChanges as NamedObjectGroupChanges_Info).Groups = new Info(true, false);


            ResultStatus status = service.CommitTransaction(request, out result);

            if (!status.IsSuccess)
                throw new ApplicationException(status.ExceptionData.Description);

            if ((result.Environment as NamedObjectGroupMaint_Environment).ObjectChanges.ResolvedEntries.SelectionValues != null)
                return (result.Environment as NamedObjectGroupMaint_Environment).ObjectChanges.ResolvedEntries.SelectionValues.GetAsDataTable();

            return null;
        }

        private List<EquipmentItem> ConvertRecordSetToEqpList(RecordSet recordSet, bool isResourceGrp)
        {
            List<EquipmentItem> equipmentList = new List<EquipmentItem>();

            if (isResourceGrp)
            {
                foreach (Row row in recordSet.Rows)
                {
                    var item = new EquipmentItem
                    {
                        EquipmentName = row.Values[1],
                        EquipmentDescription = row.Values[2],
                        Factory = row.Values[4],
                        EquipmentStatus = row.Values[5],
                        EquipmentAvailability = (row.Values[6] == "YES" ? true : false),
                        EquipmentCategory = row.Values[7],
                        EquipmentType = row.Values[8],
                        LotCount = Convert.ToInt32(row.Values[9])
                    };

                    equipmentList.Add(item);
                }
            }
            else
            {
                foreach (Row row in recordSet.Rows)
                {
                    var item = new EquipmentItem
                    {
                        EquipmentName = row.Values[0],
                        EquipmentDescription = row.Values[12],
                        Factory = row.Values[6],
                        EquipmentStatus = row.Values[2],
                        EquipmentAvailability = (row.Values[3] == "YES" ? true : false),
                        EquipmentCategory = row.Values[10],
                        EquipmentType = row.Values[11],
                        LotCount = Convert.ToInt32(row.Values[7])
                    };

                    equipmentList.Add(item);
                }
            }

            return equipmentList;
        }

        private void GetEquipmentPMStatus(List<EquipmentItem> equipmentList)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new WCF.Services.GetMaintenanceStatusesService(session.CurrentUserProfile);
            var serviceData = new OM.GetMaintenanceStatuses();
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
                foreach (EquipmentItem item in equipmentList)
                {
                    if (!result.Environment.MaintenanceStatus.IsEmpty)
                    {
                        //25 - EquipmentName, 6 - Maintenance Status (Pending/Due/PastDue), 10 - Next Due Date
                        //PM Required, when Equipment is found, Maintenance Status is not null or Due Date within 3 days
                        bool requiredPM = result.Environment.MaintenanceStatus.SelectionValues.Rows.Where(r => item.EquipmentName == r.Values[25].ToString() &&
                        (!String.IsNullOrEmpty(r.Values[6]) || (!String.IsNullOrEmpty(r.Values[10]) && DateTime.Parse(r.Values[10]).Subtract(DateTime.Now).Days <= 3))).Any();
                        item.RequiredPM = requiredPM;
                    }
                }
            }
            //else
            //{
            //    DisplayMessage(resultStatus);
            //}
        }

        private CWC.TileContainer.TileContext CreateTile(EquipmentItem equipment, string tileColName)
        {
            var tile = new CWC.TileContainer.TileContext
            {
                ColumnName = tileColName,
                Title = equipment.EquipmentName,
                Image = "typeMachine48.svg"
            };

            var strings = new List<string>();
            if (true)
            {
                strings.Add($"{labelValues["EquipmentDescription"]} : {equipment.EquipmentDescription}");
                strings.Add($"{labelValues["Status"]} : {equipment.EquipmentStatus}");
            }
            tile.Text = strings.ToArray();
            tile.CustomData = equipment.EquipmentName;

            return tile;
        }

        #endregion

    }
}