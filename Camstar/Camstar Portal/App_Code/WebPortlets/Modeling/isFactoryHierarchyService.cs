// © Siemens 2021 Siemens Product Lifecycle Management Software Inc.
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Personalization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using OS = Camstar.WCF.ObjectStack;
using WcfUtil = Camstar.WebPortal.WCFUtilities;

namespace WebClientPortal
{

    public partial class FactoryHierarchyService
    {
        Dictionary<string, string> localizedLabels;

        /// <summary>
        /// Get inventory locations that are not currently in the FHM
        /// </summary>
        /// <param name="linkableResults">Return the 'unlinked' resources in an array.</param>
        /// <returns>A status object</returns>
        /// <remarks>
        /// All get 'linkable objects' methods use the same parameters name and types. This makes the calls easier to 
        /// perform on the client.
        /// </remarks>
        [OperationContract]
        OS.ResultStatus GetLinkableIplEquipment(out EquipmentForLinkPopup linkableResults)
        {
            OS.ResultStatus resStatus = new OS.ResultStatus()
            {
                IsSuccess = true
            };

            List<LinkableChild> linkableResourceResults;
            GetLinkableResources(out linkableResourceResults);

            List<LinkableChild> linkableInventoryLocationsResults;
            GetLinkableIsInventoryLocations(out linkableInventoryLocationsResults);

            linkableResults = new EquipmentForLinkPopup()
            {
                Equipment = linkableResourceResults,
                InventoryLocations = linkableInventoryLocationsResults
            };

            return resStatus;
        }

        /// <summary>
        /// Get inventory locations that are not currently in the FHM
        /// </summary>
        /// <param name="linkableResults">Return the 'unlinked' resources in an array.</param>
        /// <returns>A status object</returns>
        OS.ResultStatus GetLinkableIsInventoryLocations(out List<LinkableChild> linkableResults)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            WcfUtil.QueryUtil qUtil = new WcfUtil.QueryUtil(session.CurrentUserProfile);
            List<LinkableChild> isInventoryLocations = new List<LinkableChild>();

            OS.ResultStatus resStatus = new OS.ResultStatus()
            {
                IsSuccess = true
            };

            try
            {
                string msg = string.Empty;
                DataTable isInventoryLocationTable = qUtil.Execute("isInventoryLocationsFhmLinkable", null, new QueryOptions(), ref msg);

                if (isInventoryLocationTable != null && isInventoryLocationTable.Rows.Count > 0)
                {
                    for (int i = 0; i < isInventoryLocationTable.Rows.Count; i++)
                    {
                        var resource = new LinkableChild()
                        {
                            InstanceID = isInventoryLocationTable.Rows[i]["isInventoryLocationId"] as string,
                            Name = isInventoryLocationTable.Rows[i]["isInventoryLocationName"] as string,
                            Description = isInventoryLocationTable.Rows[i]["Description"] as string
                        };

                        isInventoryLocations.Add(resource);
                    }
                }
            }
            catch (Exception ex)
            {
                resStatus.IsSuccess = false;
                resStatus.Message = ex.Message;
            }

            linkableResults = isInventoryLocations;

            return resStatus;
        }

        /// <summary>
        /// Take an array of inventory locations and add them to the FHM
        /// </summary>
        /// <param name="parentName">The name of the cell resource to link to</param>
        /// <param name="parentType">A text value of the parent type based on the FHM data types. Not currently used.</param>
        /// <param name="childItems">An array of inventory location names that need to be linked</param>
        /// <returns>A status object</returns>
        /// <remarks>
        /// All 'link' methods use the same 3 parameters names and types. This makes the calls easier to 
        /// perform on the client
        /// </remarks>
        [OperationContract]
        OS.ResultStatus LinkIplEquipmentToCell(string parentName, string parentType, Dictionary<string, string[]> childItems)
        {
            const string equipmentCollectionKey = "Equipment";
            const string inventoryLocationCollectionKey = "InventoryLocations";
        
            OS.ResultStatus resStatus = new OS.ResultStatus()
            {
                IsSuccess = true
            };
            
            string[] childResources = childItems[equipmentCollectionKey];
            resStatus = LinkResourcesToParent(parentName, parentType, childResources);

            if (resStatus.IsSuccess)
            {
                string[] childInventoryLocations = childItems[inventoryLocationCollectionKey];
                if (childInventoryLocations.Length > 0)
                {
                    LinkIsInventoryLocationsToCell(parentName, parentType, childInventoryLocations);
                }
            }

            return resStatus;
        }

        /// <summary>
        /// Take an array of inventory locations and add them to the FHM
        /// </summary>
        /// <param name="parentName">The name of the cell resource to link to</param>
        /// <param name="parentType">A text value of the parent type based on the FHM data types. Not currently used.</param>
        /// <param name="childItems">An array of inventory location names that need to be linked</param>
        /// <returns>A status object</returns>
        [OperationContract]
        OS.ResultStatus LinkIsInventoryLocationsToCell(string parentName, string parentType, string[] childItems)
        {
            OS.ResultStatus resStatus = new OS.ResultStatus()
            {
                IsSuccess = true
            };

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new isInventoryLocationMaintService(session.CurrentUserProfile);

            try
            {
                foreach (var locationName in childItems)
                {
                    isInventoryLocationMaint maint = new isInventoryLocationMaint();
                    maint.ObjectToChange = new NamedObjectRef(locationName);
                    maint.SyncName = locationName;

                    service.BeginTransaction();
                    resStatus = service.Load(maint);
                    if (!resStatus.IsSuccess)
                    {
                        service.RollBackTransaction();
                        resStatus.Message = GetResultErrMsg(resStatus);
                        break;
                    }

                    maint.ObjectChanges = new isInventoryLocationChanges()
                    {
                        isParentResource = new NamedObjectRef(parentName),
                        Name = locationName,
                    };

                    resStatus = service.ExecuteTransaction(maint);
                    if (resStatus.IsSuccess)
                    {
                        service.CommitTransaction();
                    }
                    else
                    {
                        service.RollBackTransaction();
                        resStatus.Message = GetResultErrMsg(resStatus);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                service.RollBackTransaction();

                resStatus.IsSuccess = false;
                resStatus.Message = ex.Message;
            }

            return resStatus;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inventoryLocationId">Get the inventory location with this ID</param>
        /// <param name="inventoryLocation">JSON string with inventory location properties</param>
        /// <returns></returns>
        [OperationContract]
        OS.ResultStatus GetIsInventoryLocation(string inventoryLocationId, out string inventoryLocation)
        {
            var svcParams = new OS.isInventoryLocationMaint()
            {
                ObjectToChange = new OS.NamedObjectRef()
                {
                    ID = inventoryLocationId
                }
            };

            var request = new isInventoryLocationMaint_Request()
            {
                Info = new OS.isInventoryLocationMaint_Info
                {
                    RequestValue = true,
                    ObjectToChange = new OS.Info(true),
                    ObjectChanges = new OS.isInventoryLocationChanges_Info()
                    {
                        RequestValue = true,
                        isParentResource = new OS.Info(true)
                    }
                }
            };

            var result = new isInventoryLocationMaint_Result();
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new isInventoryLocationMaintService(session.CurrentUserProfile);
            OS.ResultStatus resultStatus = service.Load(svcParams, request, out result);

            var theInventoryLocation = new
            {
                Name = result.Value.ObjectToChange.Name,
                ID = result.Value.ObjectToChange.ID,
                Description = result.Value.ObjectChanges.Description != null ? result.Value.ObjectChanges.Description.Value : "",
                ParentResourceId = result.Value.ObjectChanges.isParentResource != null ? result.Value.ObjectChanges.isParentResource.ID : ""
            };

            // send back JSON
            inventoryLocation = JsonConvert.SerializeObject(theInventoryLocation, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            return resultStatus;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        [OperationContract]
        Dictionary<string, OS.ResultStatus> isImportAreaResources(AreaImport area, string defaultSettings)
        {
            Dictionary<string, string> defaultSettingsDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(defaultSettings);

            // Get localized labels
            localizedLabels = GetLocalizedLabels(new Dictionary<string, string>()
            {
                { "ExportImportDetailStatusEnum_Succeeded", "Succeeded" },
                { "ExportImportDetailStatusEnum_Failed", "Failed" },
                { "ExportImportDetailStatusEnum_Skipped", "Skipped" },
                { "CompletionService", "Service completed successfully" },
                { "CPImport_ImportPollCount", "Import Count" },
                { "SRCAPI_ConnectionFailed", "SRC Connection Failed." },
                { "isCompletionImportSRC", "Import SRC settings completed successfully." },
                { "isFailedImportSRC", "Import SRC settings failed." },
                { "SRCAPI_NotConfigured", "No SRC API configured for Factory's Shop Floor Integration Settings." }

            });

            //Get existed resource
            Dictionary<string, string> allResources = GetAllResourceName();
            Dictionary<string, string> duplicateResource = new Dictionary<string, string>();

            if (allResources != null && allResources.Count > 0)
            {
                List<string> importResources = new List<string>();
                importResources.AddRange((from cell in area.cells select cell.name).ToList());
                importResources.AddRange((from cell in area.cells from eqp in cell.equipments where !eqp.IsInventoryLocation select eqp.name).ToList());

                duplicateResource = allResources.Where(r => importResources.Contains(r.Key)).ToDictionary(r => r.Key, r => r.Value);
            }

            Dictionary<string, string> allInventory = GetAllInventoryLocName();
            Dictionary<string, string> duplicateInventory = new Dictionary<string, string>();

            if (allInventory != null && allInventory.Count > 0)
            {
                List<string> importinventory = new List<string>();
                importinventory.AddRange((from cell in area.cells from eqp in cell.equipments where eqp.IsInventoryLocation select eqp.name).ToList());

                duplicateInventory = allInventory.Where(r => importinventory.Contains(r.Key)).ToDictionary(r => r.Key, r => r.Value);
            }

            // Import Opcenter Resources
            List<ImportResource> importedItems = new List<ImportResource>();
            Dictionary<string, OS.ResultStatus> result = new Dictionary<string, ResultStatus>();
            result.Add("OpcenterResult", ImportOpcenterResources(area, duplicateResource, duplicateInventory, out importedItems));

            // create object in format SRC expects
            SrcMssImport srcMssImport = new SrcMssImport()
            {
                ImportResources = importedItems,
                DefaultSettings = defaultSettingsDict
            };

            if (area.importSRCSettings && importedItems.Count > 0)
                result.Add("SRCResult", ImportSRCSettings(srcMssImport));

            return result;
        }

        /// <summary>
        /// Create Resources and Inventory Locations in Opcenter
        /// </summary>
        /// <param name="area"></param>
        /// <param name="duplicateResource"></param>
        /// <param name="duplicateInventory"></param>
        /// <param name="importedResources">Details of each Resource or Inventory Location imported into Opcenter</param>
        /// <returns></returns>
        private OS.ResultStatus ImportOpcenterResources(
            AreaImport area,
            Dictionary<string, string> duplicateResource,
            Dictionary<string, string> duplicateInventory,
            out List<ImportResource> importedResources)
        {
            OS.ResultStatus resultStatus = new OS.ResultStatus();
            string resourceId = String.Empty;
            importedResources = new List<ImportResource>();
            int importCount = 0;

            //Create Cell
            foreach (Cell cell in area.cells)
            {
                //Skip if exist
                if (!duplicateResource.ContainsKey(cell.name))
                {
                    resultStatus = CreateResource(cell.name, cell.factoryLevelIndex, area.name, OS.FactoryLevelEnum.Cell, out resourceId);
                    if (!resultStatus.IsSuccess)
                        return new OS.ResultStatus(resultStatus.ToString(), false);

                    importCount++;
                    importedResources.Add(new ImportResource(resourceId, cell.name, FactoryLevel.LINE, null));
                }
                else
                    importedResources.Add(new ImportResource(duplicateResource[cell.name], cell.name, FactoryLevel.LINE, null));

                //Create Equipment
                foreach (Equipment eqp in cell.equipments)
                {
                    //Skip if exist
                    if (eqp.IsInventoryLocation)
                    {
                        if (!duplicateInventory.ContainsKey(eqp.name))
                        {
                            resultStatus = CreateInventoryLocation(eqp.name, cell.name, out string locationId);
                            if (!resultStatus.IsSuccess)
                                return new OS.ResultStatus(resultStatus.ToString(), false);

                            importCount++;
                            importedResources.Add(new ImportResource(locationId, eqp.name, eqp.machineId, eqp.machineType, FactoryLevel.EQUIPMENT, eqp.overrideSettings));
                        }
                        else
                            importedResources.Add(new ImportResource(duplicateInventory[eqp.name], eqp.name, eqp.machineId, eqp.machineType, FactoryLevel.EQUIPMENT, eqp.overrideSettings));
                    }
                    else
                    {
                        if (!duplicateResource.ContainsKey(eqp.name))
                        {
                            resultStatus = CreateResource(eqp.name, eqp.factoryLevelIndex, cell.name, OS.FactoryLevelEnum.Equipment, out resourceId);
                            if (!resultStatus.IsSuccess)
                                return new OS.ResultStatus(resultStatus.ToString(), false);

                            importCount++;
                            importedResources.Add(new ImportResource(resourceId, eqp.name, eqp.machineId, eqp.machineType, FactoryLevel.EQUIPMENT, eqp.overrideSettings));
                        }
                        else
                            importedResources.Add(new ImportResource(duplicateResource[eqp.name], eqp.name, eqp.machineId, eqp.machineType, FactoryLevel.EQUIPMENT, eqp.overrideSettings));
                    }
                }
            }

            string importSuccessMsg = $"{localizedLabels["CompletionService"]}. ({localizedLabels["CPImport_ImportPollCount"]}: {importCount} {localizedLabels["ExportImportDetailStatusEnum_Succeeded"]}{((duplicateResource.Count > 0 || duplicateInventory.Count > 0) ? ", " + (duplicateResource.Count + duplicateInventory.Count) + " " + localizedLabels["ExportImportDetailStatusEnum_Skipped"] : "")})";
            return new OS.ResultStatus(importSuccessMsg, true);
        }

        private OS.ResultStatus CreateInventoryLocation(string inventoryName, string parentResourceName, out string inventoryLocationId)
        {
            var svcParams = new OS.isInventoryLocationMaint()
            {
                ObjectChanges = new OS.isInventoryLocationChanges
                {
                    Name = inventoryName,
                    isParentResource = new OS.NamedObjectRef(parentResourceName)
                }
            };

            var request = new isInventoryLocationMaint_Request()
            {
                Info = new OS.isInventoryLocationMaint_Info()
                {
                    ObjectToChange = new OS.Info(true)
                }
            };

            var result = new isInventoryLocationMaint_Result();
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new isInventoryLocationMaintService(session.CurrentUserProfile);

            service.BeginTransaction();
            service.New(svcParams);
            service.ExecuteTransaction();
            OS.ResultStatus resultStatus = service.CommitTransaction(request, out result);

            if (resultStatus.IsSuccess && !result.IsEmpty)
                inventoryLocationId = result.Value.ObjectToChange.ID;
            else
                inventoryLocationId = null;

            return resultStatus;
        }

        private Dictionary<string, string> GetAllInventoryLocName()
        {
            var svcParams = new OS.isInventoryLocationMaint();

            var request = new isInventoryLocationMaint_Request()
            {
                Info = new OS.isInventoryLocationMaint_Info
                {
                    ObjectListInquiry = new OS.Info(false, true)
                }
            };

            var result = new isInventoryLocationMaint_Result();
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new isInventoryLocationMaintService(session.CurrentUserProfile);
            OS.ResultStatus resultStatus = service.GetEnvironment(svcParams, request, out result);

            if (resultStatus.IsSuccess && result.Environment.ObjectListInquiry.SelectionValues.Rows != null)
                return result.Environment.ObjectListInquiry.SelectionValues.Rows.Select(s => new { key = s.Values[0].ToString(), value = s.Values[2].ToString() }).ToDictionary(t => t.key, t => t.value);
            else
                return null;
        }

        /// <summary>
        /// Use a query to get Employee -> Factory -> Shopfloor Integration Settings -> SRC URL
        /// </summary>
        /// <returns></returns>
        private string GetSRCAPI()
        {
            string srcapiurl = string.Empty;
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

            if (session != null)
            {
                var qparam = new OS.QueryParameters()
                {
                    Parameters = new OS.QueryParameter[]
                    {
                      new OS.QueryParameter("employee", session.SessionValues.UserName)
                    }
                };

                var recordSet = new OS.RecordSet();
                var service = new QueryService(session.CurrentUserProfile);
                var options = new OS.QueryOptions();
                var resultStatus = service.Execute("GetSRCAPI", qparam, options, out recordSet);
                if (resultStatus.IsSuccess && recordSet.Rows != null && recordSet.Rows.Length > 0)
                {
                    var qlist = new List<QueryData>();
                    foreach (var row in recordSet.Rows)
                        qlist.Add(new QueryData()
                        {
                            Text = row.Values[2],
                        });

                    srcapiurl = qlist[0].Text.ToString();
                }
            }

            return srcapiurl;
        }

        /// <summary>
        /// Call SRC API to create settings in SRC and vManaage
        /// </summary>
        /// <param name="mmImport"></param>
        /// <returns></returns>
        private ResultStatus ImportSRCSettings(SrcMssImport mmImport)
        {
            // Construct URL
            var srcurl = GetSRCAPI();
            if (String.IsNullOrEmpty(srcurl))
                return new ResultStatus(localizedLabels["SRCAPI_NotConfigured"], false);

            // Make API call
            ResultStatus result = new ResultStatus();
            HttpClient client = new HttpClient();
            UriBuilder uri = new UriBuilder(srcurl);
            client.BaseAddress = uri.Uri;

            var requestUri = $"api/ResourceSettings/ImportSRCSettingsAggregate";
            string reqData = JsonConvert.SerializeObject(mmImport);
            HttpContent content = new StringContent(reqData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;

            try
            {
                Task<HttpResponseMessage> responseTask = client.PostAsync(requestUri, content);
                responseTask.Wait();
                response = responseTask.Result;
                var responseResult = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode == false)
                {
                    dynamic errorMsg = JsonConvert.DeserializeObject(responseResult);
                    result = new ResultStatus($"{localizedLabels["isFailedImportSRC"]} { (errorMsg.ContainsKey("detail") ? errorMsg.detail.Value : "") }" , false);
                }
                else
                {
                    result = GetSRCImportResultStatus(mmImport.ImportResources, responseResult);
                }
            }
            catch (JsonReaderException jre)
            {
                // unable to deserialize a JSON result - could happen if get 404
                result = new ResultStatus($"{localizedLabels["isFailedImportSRC"]} Status Code: {(int)response.StatusCode}, reason: {response.ReasonPhrase}, request: {response.RequestMessage.RequestUri}.  Exception: {jre.ToString()}", false);
            }
            catch (AggregateException ae) 
            {
                // Go to the lowest level error to get actionable information to sent to the user
                var errors = ae.Flatten().InnerExceptions;
                var currentError = errors.First();

                while (currentError.InnerException != null)
                {
                    currentError = currentError.InnerException;
                }

                result = new ResultStatus(currentError.Message, false);
            }
            catch (Exception ex)
            {
                result = new ResultStatus(ex.Message, false);
            }

            return result;
        }

        /// <summary>
        /// Process the results of the import and turn it into a format the client can read to see the outcome
        /// </summary>
        /// <param name="importedResources"></param>
        /// <param name="responseText"></param>
        /// <returns></returns>
        private ResultStatus GetSRCImportResultStatus(List<ImportResource> importedResources, string responseText)
        {
            List<dynamic> resultsToReturn = new List<dynamic>();

            if (!string.IsNullOrEmpty(responseText) && !responseText.Equals("{}"))
            {
                dynamic res = JsonConvert.DeserializeObject(responseText);

                importedResources.Add(new ImportResource("0", "Default Settings", FactoryLevel.ENTERPRISE, string.Empty));
                foreach (ImportResource importedResource in importedResources)
                {
                    if (res.ContainsKey(importedResource.ResourceName))
                    {
                        var results = res[importedResource.ResourceName];
                        bool isSuccess = results.isSuccess.Value;
                        string resultsMsg = string.Empty;

                        if (results.exception != null && results.exception.message != null &&
                            !string.IsNullOrEmpty(results.exception.exception.Message.Value))
                            resultsMsg = $"{results.exception.message}";
                        else if (results.message != null && !string.IsNullOrEmpty(results.message.Value))
                            resultsMsg = $"{results.message.Value}";

                        dynamic output = new
                        {
                            Resource = importedResource.ResourceName,
                            Success = isSuccess,
                            Message = resultsMsg,
                            FactorLevelType = importedResource.FactoryLevel
                        };

                        resultsToReturn.Add(output);
                    }
                }
            }

            string outputData = JsonConvert.SerializeObject(resultsToReturn);         
            return new ResultStatus(outputData, true);
        }        

        /// <summary>
        /// Object format that SRC expects
        /// </summary>
        protected class SrcMssImport
        {
            public List<ImportResource> ImportResources { get; set; }

            /// <summary>
            /// Default settings on the Enterprise.  Written to vManage DB
            /// Key: setting name, Value: setting value
            /// </summary>
            public Dictionary<string, string> DefaultSettings { get; set; }
        }

        protected class ImportResource
        {
            /// <summary>
            /// Default McId to -1 and McType (machine type) to empty
            /// </summary>
            /// <param name="resourceId"></param>
            /// <param name="resourceName"></param>
            /// <param name="factoryLevel"></param>
            /// <param name="overrideSettingsJson"></param>
            public ImportResource(string resourceId, string resourceName, FactoryLevel factoryLevel, string overrideSettingsJson)
                : this(resourceId, resourceName, -1, "", factoryLevel, overrideSettingsJson)
            {
            }

            public ImportResource(string resourceId, string resourceName, int mcId, string mcType, FactoryLevel factoryLevel, string overrideSettingsJson)
            {
                ResourceId = resourceId;
                ResourceName = resourceName;
                McId = mcId;
                McType = mcType;
                FactoryLevel = factoryLevel;
                OverrideSettings = overrideSettingsJson != null ? JsonConvert.DeserializeObject<Dictionary<string, string>>(overrideSettingsJson) : new Dictionary<string, string>();
            }

            public string ResourceId { get; set; }
            public string ResourceName { get; set; }
            public int McId { get; } //Only Equipment level has McId
            /// <summary>
            /// Lines have no machine type
            /// </summary>
            public string McType { get; set; }
            public FactoryLevel FactoryLevel { get; set; }
            /// <summary>
            /// Override Enterprise settings stored in vManage
            /// Key: setting name, Value: setting value
            /// </summary>
            public Dictionary<string, string> OverrideSettings { get; set; }
        }

        protected enum FactoryLevel
        {
            ENTERPRISE = 1,
            SITE = 2,
            AREA = 4,
            LINE = 8,
            EQUIPMENT = 16
        }

        public class EquipmentForLinkPopup
        {
            public List<LinkableChild> Equipment { get; set; }
            public List<LinkableChild> InventoryLocations { get; set; }
        }

    }
}
