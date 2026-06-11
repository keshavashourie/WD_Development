/* Copyright 2022 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Newtonsoft.Json;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.Personalization;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{

    public class EqpStatusWP : MatrixWebPart
    {
        protected virtual CWC.TileContainer equipmentTiles { get { return Page.FindCamstarControl("EquipmentTile") as CWC.TileContainer; } }
        protected MatrixWebPart EquipmentTilesWP { get { return Page.FindCamstarControl("EqpStatusWP") as MatrixWebPart; } }
        protected CWC.Label LblShowingEntries { get { return Page.FindCamstarControl("LblShowingEntries") as CWC.Label; } }

        private const int TOTAL_EQP_DISPLAY = 6;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadLabels();
            BuildTiles();
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "initializeEqpStatusWP", $"EqpStatusWP.initialize({GetClientLabels()});", true);
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/User/EqpStatusWP.js");
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Refresh")
            {
                BuildTiles();
            }
        }

        private static string GetResourceGroup(string workCenter)
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new WorkCenterMaintService(session.CurrentUserProfile);
                var serviceData = new WorkCenterMaint
                {
                    ObjectToChange = new NamedObjectRef(workCenter)
                };

                var request = new WorkCenterMaint_Request
                {
                    Info = new WorkCenterMaint_Info
                    {
                        ObjectToChange = new Info(true),
                        ObjectChanges = new WorkCenterChanges_Info
                        {
                            ResourceGroup = new Info(true)
                        }
                    }
                };

                var result = new WorkCenterMaint_Result();
                ResultStatus status = service.Load(serviceData, request, out result);

                if (status != null && status.IsSuccess && result.Value.ObjectChanges.ResourceGroup != null)
                    return result.Value.ObjectChanges.ResourceGroup.ToString();
            }

            return String.Empty;
        }

        private List<EquipmentItem> GetLineAssigmentEquipment(out int totalCount)
        {
            //Retrieve Line Assigment Data
            string lineAssigWorkCenter = (Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.WorkCenter) != null ?
                                          Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.WorkCenter).ToString() : String.Empty);
            string lineAssigResource = (Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Resource) != null ?
                                        Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Resource).ToString() : String.Empty);
            string lineAssigWorkStation = (Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.WorkStation) != null ?
                                           Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.WorkStation).ToString() : String.Empty);
            string lineAssigFactory = (Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Factory) != null ?
                                       Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Factory).ToString() : String.Empty);

            string equipmentSelected = "";
            if (!String.IsNullOrEmpty(lineAssigWorkStation) || !String.IsNullOrEmpty(lineAssigResource))
                equipmentSelected = (!String.IsNullOrEmpty(lineAssigWorkStation) ? lineAssigWorkStation : lineAssigResource);

            List<EquipmentItem> equipmentList = new List<EquipmentItem>();

            totalCount = 0;
            if (queryName == null) { }

            else if (!String.IsNullOrEmpty(equipmentSelected))
            {
                //Get selected equipment
                var fs = FrameworkManagerUtil.GetFrameworkSession(this.Page.Session);
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

                oResultStatus = oSvc.Execute(queryName, oQueryParam, oQueryOptions, out oQueryResult);

                if (oResultStatus.IsSuccess && oQueryResult.Rows != null && oQueryResult.Rows.Count() > 0)
                    equipmentList = ConvertRecordSetToEqpList(oQueryResult, false);

                totalCount = equipmentList.Count;
            }
            else if (!String.IsNullOrEmpty(lineAssigWorkCenter))
            {
                //Get a list of equipments based on the resource group defined in work center
                string resourceGroup = GetResourceGroup(lineAssigWorkCenter);

                if (!String.IsNullOrEmpty(resourceGroup))
                {
                    System.Data.DataTable resolvedEquipmentList = LoadResolvedEntries(resourceGroup);
                    if (resolvedEquipmentList != null && resolvedEquipmentList.Rows != null)
                    {
                        totalCount = resolvedEquipmentList.Rows.Count;
                        foreach (System.Data.DataRow row in resolvedEquipmentList.Rows)
                        {
                            //Get selected equipment
                            var fs = FrameworkManagerUtil.GetFrameworkSession(this.Page.Session);
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

                            oResultStatus = oSvc.Execute(queryName, oQueryParam, oQueryOptions, out oQueryResult);
                            List<EquipmentItem> equipmentListTemp = new List<EquipmentItem>();
                            if (oResultStatus.IsSuccess && oQueryResult.Rows != null && oQueryResult.Rows.Count() > 0)
                            {
                                equipmentListTemp = ConvertRecordSetToEqpList(oQueryResult, false);
                                equipmentList.Add(equipmentListTemp[0]);
                                if (equipmentList.Count > TOTAL_EQP_DISPLAY)
                                    break;
                            }
                        }
                    }
                }
            }

            //Filter out equipments not belongs to Factory Assigned
            //if (!String.IsNullOrEmpty(lineAssigFactory))
            //     equipmentList = equipmentList.Where(r => r.Factory == lineAssigFactory).ToList();

            //Get Equipment PM Status
            if (equipmentList.Count > 0)
                GetEquipmentPMStatus(equipmentList);

            return equipmentList;

        }

        private static void GetEquipmentPMStatus(List<EquipmentItem> equipmentList)
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
            else
            {
                //DisplayMessage(resultStatus);
            }
        }

        private static List<EquipmentItem> ConvertRecordSetToEqpList(RecordSet recordSet, bool isResourceGrp)
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

        protected void SetLabelNames()
        {
            labelNames = new Dictionary<string, string>
            {
                { "EquipmentName", "FactoryLevelEnum_Equipment" },
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

        /// <summary>
        /// Creates a JSON string representing an object with label name values for passing to the client
        /// Other workspaces should not need to override this
        /// </summary>
        /// <returns></returns>
        protected string GetClientLabels()
        {
            List<string> labelMap = new List<string>();
            foreach (KeyValuePair<string, string> label in labelValues)
            {
                labelMap.Add("\"" + label.Key + "\"" + ":" + "\"" + label.Value + "\"");
            }

            return "{" + String.Join(",", labelMap) + "}";
        }

        private void BuildTiles()
        {
            int totalEqpListCount = 0;
            List<EquipmentItem> equipmentList = GetLineAssigmentEquipment(out totalEqpListCount);

            // Only show top 6 of equipment if equipment more than 6
            if (totalEqpListCount > TOTAL_EQP_DISPLAY)
                equipmentList = equipmentList.Take(TOTAL_EQP_DISPLAY).ToList();

            if (totalEqpListCount == 0)
            {
                //Display No Data img
                if (equipmentTiles.ControlState.Tiles != null)
                    equipmentTiles.ControlState.Tiles.Clear();
                LblShowingEntries.Text = "";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "displayEmptyMessage", $"EqpStatusWP.displayEmptyMessage();", true);
            }
            else
            {
                LblShowingEntries.Text = "(" + (totalEqpListCount > TOTAL_EQP_DISPLAY ? TOTAL_EQP_DISPLAY : totalEqpListCount) + " of " + totalEqpListCount + ")";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "eqpTileNotEmpty", $"EqpStatusWP.eqpTileNotEmpty({GetEquipmentListJSON(equipmentList)});", true);
            }

            this.WebPartManager.RenderToClient(EquipmentTilesWP);
        }

        //Execute query for getting Equipment list and return it in JSON format
        private string GetEquipmentListJSON(List<EquipmentItem> equipmentList)
        {
            equipmentTiles.ControlState.Tiles = new List<CWC.TileContainer.TileContext>();

            foreach (EquipmentItem item in equipmentList)
            {
                equipmentTiles.ControlState.Tiles.Add(CreateTile(item, "Equipment"));
            }
            var serializer = new JavaScriptSerializer();

            return serializer.Serialize(equipmentList);

        }

        //Creates tile for tile container based on Equipment list data
        private CWC.TileContainer.TileContext CreateTile(EquipmentItem equipment, string tileColName)
        {
            var tile = new CWC.TileContainer.TileContext
            {
                ColumnName = tileColName,
                Title = equipment.EquipmentName
            };

            var strings = new List<string>();
            if (true)
            {
                strings.Add($"{labelValues["EquipmentDescription"]}: {equipment.EquipmentDescription}");
                strings.Add($"{labelValues["Status"]}: {equipment.EquipmentStatus}");
            }
            tile.Text = strings.ToArray();
            tile.CustomData = equipment.EquipmentName;

            return tile;
        }

        private static string getContractSafeText(UIComponentDataContract contract, string name)
        {
            var ret = string.Empty;
            var v = contract.GetValueByName(name);
            if (v != null)
            {
                ret = System.Net.WebUtility.HtmlEncode(v.ToString());
            }
            return ret;
        }

        public static bool RefreshEquipmentStatus(AjaxTransition transition)
        {
            //Retrieve Line Assigment Data
            string lineAssigWorkCenter = null;
            string lineAssigResource = null;
            string lineAssigWorkStation = null;
            string lineAssigFactory = null;

            var session = FrameworkManagerUtil.GetFrameworkSession();
            var contract = HttpContext.Current.Session[SessionConstants.SessionDataContract] as UIComponentDataContract;
            if (contract != null)
            {
                object resource = getContractSafeText(contract, DataMemberConstants.Resource);
                object factory = getContractSafeText(contract, DataMemberConstants.Factory);
                object workcenter = getContractSafeText(contract, DataMemberConstants.WorkCenter);
                object workstation = getContractSafeText(contract, DataMemberConstants.WorkStation);

                //Retrieve Line Assigment Data
                lineAssigResource = (resource != null ? resource.ToString() : String.Empty);
                lineAssigWorkCenter = (workcenter != null ? workcenter.ToString() : String.Empty);
                lineAssigWorkStation = (workstation != null ? workstation.ToString() : String.Empty);
                lineAssigFactory = (factory != null ? factory.ToString() : String.Empty);
            }

            string equipmentSelected = "";
            if (!String.IsNullOrEmpty(lineAssigWorkStation) || !String.IsNullOrEmpty(lineAssigResource))
                equipmentSelected = (!String.IsNullOrEmpty(lineAssigWorkStation) ? lineAssigWorkStation : lineAssigResource);

            List<EquipmentItem> equipmentList = new List<EquipmentItem>();
            int totalCount = 0;
            if (queryName == null) { }
            else if (!String.IsNullOrEmpty(equipmentSelected))
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

                oResultStatus = oSvc.Execute(queryName, oQueryParam, oQueryOptions, out oQueryResult);

                if (oResultStatus.IsSuccess && oQueryResult.Rows != null && oQueryResult.Rows.Count() > 0)
                    equipmentList = ConvertRecordSetToEqpList(oQueryResult, false);

                totalCount = equipmentList.Count;
            }
            else if (!String.IsNullOrEmpty(lineAssigWorkCenter))
            {
                //Get a list of equipments based on the resource group defined in work center
                string resourceGroup = GetResourceGroup(lineAssigWorkCenter);
                if (!String.IsNullOrEmpty(resourceGroup))
                {
                    System.Data.DataTable resolvedEquipmentList = LoadResolvedEntries(resourceGroup);
                    if (resolvedEquipmentList != null && resolvedEquipmentList.Rows != null)
                    {
                        totalCount = resolvedEquipmentList.Rows.Count;
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

                            oResultStatus = oSvc.Execute(queryName, oQueryParam, oQueryOptions, out oQueryResult);
                            List<EquipmentItem> equipmentListTemp = new List<EquipmentItem>();
                            if (oResultStatus.IsSuccess && oQueryResult.Rows != null && oQueryResult.Rows.Count() > 0)
                            {
                                equipmentListTemp = ConvertRecordSetToEqpList(oQueryResult, false);
                                equipmentList.Add(equipmentListTemp[0]);
                                if (equipmentList.Count > TOTAL_EQP_DISPLAY)
                                    break;
                            }
                        }
                    }
                }
            }

            //Filter out equipments not belongs to Factory Assigned
            //if (!String.IsNullOrEmpty(lineAssigFactory))
            //    equipmentList = equipmentList.Where(r => r.Factory == lineAssigFactory).ToList();

            //Get Equipment PM Status
            if (equipmentList.Count > 0)
                GetEquipmentPMStatus(equipmentList);

            string txnData = null;

            if (equipmentList.Count > 0)
            {
                var serializer = new JavaScriptSerializer();
                var labels = serializer.Deserialize<Dictionary<string, string>>(transition.CommandParameters);

                var tiles = new List<CWC.TileContainer.TileContext>();
                foreach (EquipmentItem equipment in equipmentList)
                {
                    var tile = new CWC.TileContainer.TileContext
                    {
                        ColumnName = "Equipment",
                        Title = equipment.EquipmentName
                    };

                    var strings = new List<string>();

                    strings.Add($"{labels["EquipmentDescription"]}: {equipment.EquipmentDescription}");
                    strings.Add($"{labels["Status"]}: {equipment.EquipmentStatus}");

                    tile.Text = strings.ToArray();
                    tile.CustomData = equipment.EquipmentName;
                    tiles.Add(tile);

                    if (tiles.Count > TOTAL_EQP_DISPLAY - 1)
                        break;
                }

                var equipmentListJSON = serializer.Serialize(equipmentList);

                var tileContext = new CWC.TileContainer.TilesContext
                {
                    Tiles = tiles,
                    CustomData = totalCount.ToString()
                };

                txnData = JsonConvert.SerializeObject(tileContext, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }

            transition.Response = new[] { new ResponseSection(ResponseType.Command, transition.ID, txnData) };

            return true;

        } // RefreshEquipmentStatus

        private static System.Data.DataTable LoadResolvedEntries(string resourceGroup)
        {
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            WSDataCreator creator = new WSDataCreator();

            NamedObjectGroupMaint serviceData = creator.CreateServiceData("ResourceGroupMaint") as NamedObjectGroupMaint;
            Maintenance_Info serviceInfo = creator.CreateServiceInfo("ResourceGroupMaint") as Maintenance_Info;

            IMaintenanceBase service = creator.CreateService("ResourceGroupMaint", profile) as IMaintenanceBase;
            Request request = creator.CreateObject("ResourceGroupMaint_Request") as Request;
            Result result = creator.CreateObject("ResourceGroupMaint_Result") as Result;
            //Page.GetInputData(serviceData);
            service.BeginTransaction();

            NamedObjectGroupMaint reqData = creator.CreateServiceData("ResourceGroupMaint") as NamedObjectGroupMaint;

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

        private Dictionary<string, string> labelNames;
        private Dictionary<string, string> labelValues;
        private static string queryName;
    }
}