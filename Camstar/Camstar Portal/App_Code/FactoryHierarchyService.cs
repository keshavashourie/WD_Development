// © Siemens 2021 Siemens Product Lifecycle Management Software Inc.
using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web;
using System.Runtime.Serialization;
using System.Linq;
using System.Collections.Generic;
using System.Data;

using Camstar.WebPortal.FormsFramework.Utilities;
using OS = Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;

using Newtonsoft.Json;
using Camstar.WCF.ObjectStack;
using WcfUtil = Camstar.WebPortal.WCFUtilities;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;

namespace WebClientPortal
{

    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public partial class FactoryHierarchyService
    {
        public class LinkableChild
        {
            public string InstanceID { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        protected string GetResultErrMsg(ResultStatus result)
        {
            string errMsg = "";

            if (!result.IsSuccess)
            {
                List<string> msgParts = new List<string>();

                if (!string.IsNullOrWhiteSpace(result.Message))
                    msgParts.Add(result.Message);

                if (result.ExceptionData != null && !string.IsNullOrWhiteSpace(result.ExceptionData.Description))
                    msgParts.Add(result.ExceptionData.Description);

                errMsg = string.Join(", ", msgParts);
            }

            return errMsg;
        }

        /// <summary>
        /// Get resources that are not currently in the FHM
        /// </summary>
        /// <param name="linkableResults">Return the 'unlinked' resources in an array.</param>
        /// <returns>A status object</returns>
        /// <remarks>
        /// All get 'linkable objects' methods use the same parameters name and types. This makes the calls easier to 
        /// perform on the client.
        /// </remarks>
        [OperationContract]
        OS.ResultStatus GetLinkableResources(out List<LinkableChild> linkableResults)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            WcfUtil.QueryUtil qUtil = new WcfUtil.QueryUtil(session.CurrentUserProfile);
            List<LinkableChild> resources = new List<LinkableChild>();

            OS.ResultStatus resStatus = new OS.ResultStatus()
            {
                IsSuccess = true
            };

            try
            {
                string msg = string.Empty;
                DataTable resourceTable = qUtil.Execute("ResourceFhmLinkable", null, new QueryOptions(), ref msg);

                if (resourceTable != null)
                {
                    for (int i = 0; i < resourceTable.Rows.Count; i++)
                    {
                        var resource = new LinkableChild()
                        {
                            InstanceID = resourceTable.Rows[i]["ResourceId"] as string,
                            Name = resourceTable.Rows[i]["ResourceName"] as string,
                            Description = resourceTable.Rows[i]["Description"] as string
                        };

                        resources.Add(resource);
                    }
                }
            }
            catch (Exception ex)
            {
                resStatus.IsSuccess = false;
                resStatus.Message = ex.Message;
            }

            linkableResults = resources;

            return resStatus;
        }

        /// <summary>
        /// Get factories that are not currently in the FHM
        /// </summary>
        /// <param name="linkableResults">Return the 'unlinked' factories in a list.</param>
        /// <returns>A status object</returns>
        /// <remarks>
        /// All get 'linkable objects' methods use the same parameters name and types. This makes the calls easier to 
        /// perform on the client.
        /// </remarks>
        [OperationContract]
        OS.ResultStatus GetLinkableFactories(out List<LinkableChild> linkableResults)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            WcfUtil.QueryUtil qUtil = new WcfUtil.QueryUtil(session.CurrentUserProfile);
            
            linkableResults = new List<LinkableChild>();

            OS.ResultStatus resStatus = new OS.ResultStatus()
            {
                IsSuccess = true
            };

            try
            {
                string msg = string.Empty;
                DataTable factoryTable = qUtil.Execute("FactoryFhmLinkable", null, new QueryOptions(), ref msg);

                if (factoryTable != null)
                {
                    for (int i = 0; i < factoryTable.Rows.Count; i++)
                    {
                        var factory = new LinkableChild()
                        {
                            InstanceID = factoryTable.Rows[i]["FactoryId"] as string,
                            Name = factoryTable.Rows[i]["FactoryName"] as string,
                            Description = factoryTable.Rows[i]["Description"] as string
                        };

                        linkableResults.Add(factory);
                    }
                }
            }
            catch (Exception ex)
            {
                resStatus.IsSuccess = false;
                resStatus.Message = ex.Message;
            }

            return resStatus;
        }

        /// <summary>
        /// Take an array of resources and add them to the FHM
        /// </summary>
        /// <param name="parentName">The name of the factory or resource to link to</param>
        /// <param name="parentType">A text value of the parent type based on the FHM data types</param>
        /// <param name="childResources">An array of resources names that need to be linked</param>
        /// <returns>A status object</returns>
        /// <remarks>
        /// All 'link' methods use the same 3 parameters names and types. This makes the calls easier to 
        /// perform on the client
        /// </remarks>
        [OperationContract]
        OS.ResultStatus LinkResourcesToParent(string parentName, string parentType, string[] childItems)
        {
            OS.ResultStatus resStatus = new OS.ResultStatus()
            {
                IsSuccess = true
            };

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new ResourceMaintService(session.CurrentUserProfile);
            OS.FactoryLevelEnum childFactoryLevel;

            switch (parentType)
            {
                case "Factory":
                    childFactoryLevel = OS.FactoryLevelEnum.Area;
                    break;
                case "Area":
                    childFactoryLevel = OS.FactoryLevelEnum.Cell;
                    break;
                default:
                    childFactoryLevel = OS.FactoryLevelEnum.Equipment;
                    break;
            }

            bool isAreaResource = childFactoryLevel == FactoryLevelEnum.Area;

            try
            {
                // Link resources selected by the user to a new parent resource or factory
                foreach (var resourceName in childItems)
                {
                    ResourceMaint maint = new ResourceMaint();
                    maint.ObjectToChange = new NamedObjectRef(resourceName);
                    maint.SyncName = resourceName;

                    service.BeginTransaction();
                    resStatus = service.Load(maint);
                    if (!resStatus.IsSuccess)
                    {
                        service.RollBackTransaction();
                        resStatus.Message = GetResultErrMsg(resStatus);
                        break;
                    }

                    maint.ObjectChanges = new ResourceChanges()
                    {
                        FactoryLevel = childFactoryLevel,
                        Factory = isAreaResource ? new NamedObjectRef(parentName) : new NamedObjectRef(string.Empty),
                        ParentResource = !isAreaResource ? new NamedObjectRef(parentName) : new NamedObjectRef(string.Empty),
                        Name = resourceName
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
        /// Take an array of resources and add them to the FHM
        /// </summary>
        /// <param name="parentName">The name of the factory or resource to link to</param>
        /// <param name="parentType">A text value of the parent type based on the FHM data types. Not currently used.</param>
        /// <param name="childItems">An array of resources names that need to be linked</param>
        /// <returns>A status object</returns>
        /// /// <remarks>
        /// All 'link' methods use the same 3 parameters names and types. This makes the calls easier to 
        /// perform on the client.
        /// </remarks>
        [OperationContract]
        OS.ResultStatus LinkFactoriesToEnterprise(string parentName, string parentType, string[] childItems)
        {
            OS.ResultStatus resStatus = new OS.ResultStatus()
            {
                IsSuccess = true
            };

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new FactoryMaintService(session.CurrentUserProfile);

            try
            {
                foreach (var factoryName in childItems)
                {
                    FactoryMaint maint = new FactoryMaint();
                    maint.ObjectToChange = new NamedObjectRef(factoryName);
                    maint.SyncName = factoryName;

                    service.BeginTransaction();
                    resStatus = service.Load(maint);
                    if (!resStatus.IsSuccess)
                    {
                        service.RollBackTransaction();
                        resStatus.Message = GetResultErrMsg(resStatus);
                        break;
                    }

                    maint.ObjectChanges = new FactoryChanges()
                    {
                        Enterprise = new NamedObjectRef(parentName),
                        Name = factoryName,
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
        /// <param name="resourceId">Get Resource with this ID</param>
        /// <param name="resource">JSON string with Resource properties</param>
        /// <returns></returns>
        [OperationContract]
        OS.ResultStatus GetResource(string resourceId, out string resource)
        {
            OS.ResultStatus resultStatus = new OS.ResultStatus()
            {
                IsSuccess = true
            };

            var svcParams = new OS.ResourceMaint()
            {
                ObjectToChange = new OS.NamedObjectRef()
                {
                    ID = resourceId
                }
            };

            var request = new ResourceMaint_Request()
            {
                Info = new OS.ResourceMaint_Info
                {
                    RequestValue = true,
                    ObjectToChange = new OS.Info(true),
                    ObjectChanges = new OS.ResourceChanges_Info()
                    {
                        RequestValue = true,
                        Factory = new OS.Info(true),
                        FactoryLevel = new OS.Info(true),
                        ParentResource = new OS.Info(true)
                    }
                }
            };

            resource = null;

            try
            {
                var result = new ResourceMaint_Result();
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new ResourceMaintService(session.CurrentUserProfile);
                resultStatus = service.Load(svcParams, request, out result);

                var ObjectChanges = result.Value.ObjectChanges;
                var theResource = new
                {
                    Name = result.Value.ObjectToChange.Name,
                    ID = result.Value.ObjectToChange.ID,
                    Description = ObjectChanges.Description != null ? ObjectChanges.Description.Value : "",
                    FactoryId = ObjectChanges.Factory != null ? ObjectChanges.Factory.ID : "",
                    ParentResourceId = ObjectChanges.ParentResource != null ? ObjectChanges.ParentResource.ID : "",
                    FactoryLevel = Enum.GetName(typeof(OS.FactoryLevelEnum), ObjectChanges.FactoryLevel.Value)
                };

                // Send back JSON
                resource = JsonConvert.SerializeObject(theResource, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                resultStatus.IsSuccess = false;
                resultStatus.Message = ex.Message;
            }
            
            return resultStatus;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enterpriseId">Get Enterprise with this ID</param>
        /// <param name="enterprise">JSON string with Enterprise properties</param>
        /// <returns></returns>
        [OperationContract]
        OS.ResultStatus GetEnterprise(string enterpriseId, out string enterprise)
        {
            var svcParams = new OS.EnterpriseMaint()
            {
                ObjectToChange = new OS.NamedObjectRef()
                {
                    ID = enterpriseId
                }
            };

            var request = new EnterpriseMaint_Request()
            {
                Info = new OS.EnterpriseMaint_Info
                {
                    RequestValue = true,
                    ObjectToChange = new OS.Info(true),
                    ObjectChanges = new OS.EnterpriseChanges_Info()
                    {
                        RequestValue = true
                    }
                }
            };
            var result = new EnterpriseMaint_Result();

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new EnterpriseMaintService(session.CurrentUserProfile);
            OS.ResultStatus resultStatus = service.Load(svcParams, request, out result);

            var theEnterprise = new
            {
                Name = result.Value.ObjectChanges.Name.Value,
                ID = result.Value.ObjectToChange.ID,
                Description = result.Value.ObjectChanges.Description != null ? result.Value.ObjectChanges.Description.Value : ""
            };

            // send back JSON
            enterprise = JsonConvert.SerializeObject(theEnterprise, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            return resultStatus;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enterpriseId">Get Factory with this ID</param>
        /// <param name="factory">JSON string with factory properties</param>
        /// <returns></returns>
        [OperationContract]
        OS.ResultStatus GetFactory(string factoryId, out string factory)
        {
            var svcParams = new OS.FactoryMaint()
            {
                ObjectToChange = new OS.NamedObjectRef()
                {
                    ID = factoryId
                }
            };

            var request = new Camstar.WCF.Services.FactoryMaint_Request()
            {
                Info = new OS.FactoryMaint_Info
                {
                    RequestValue = true,
                    ObjectToChange = new OS.Info(true),
                    ObjectChanges = new OS.FactoryChanges_Info()
                    {
                        RequestValue = true
                    },
                    Factory = new OS.Info(true)
                }
            };
            var result = new FactoryMaint_Result();

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new FactoryMaintService(session.CurrentUserProfile);
            OS.ResultStatus resultStatus = service.Load(svcParams, request, out result);

            var theFactory = new
            {
                Name = result.Value.ObjectChanges.Name.Value,
                ID = result.Value.ObjectToChange.ID,
                Description = result.Value.ObjectChanges.Description != null ? result.Value.ObjectChanges.Description.Value : "",
                EnterpriseName = result.Value.ObjectChanges.Enterprise.Name,
                EnterpriseId = result.Value.ObjectChanges.Enterprise.ID
            };

            // send back JSON
            factory = JsonConvert.SerializeObject(theFactory, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            return resultStatus;
        }

        /// <summary>
        /// Call SRC to import factory hierarchy info that was previously exported from SRC
        /// </summary>
        /// <param name="srcFactoryHierarchy">JSON formatted string representing factory hierarchy and associated SRC resource settings</param>
        /// <returns></returns>
        [OperationContract]
        FHServiceResponse SrcImport(dynamic srcFactoryHierarchy)
        {
            // make API call
            string responseBody = string.Empty;
            string responseMessage = string.Empty;
            string srcUrl = string.Empty;
            bool success = false;

            try
            {
                // Construct URL
                ResultStatus getUrlResult = GetSrcApi(out string factoryName, out string settingsName, out srcUrl);
                if (!getUrlResult.IsSuccess)
                    return new FHServiceResponse() { Success = false, Message = getUrlResult.Message };

                if (string.IsNullOrEmpty(srcUrl))
                {
                    return new FHServiceResponse() { Success = false, Message = GetApiUrlErrorMessage(factoryName, settingsName, srcUrl) };
                }

                using (HttpClient httpClient = new HttpClient())
                {
                    UriBuilder uri = new UriBuilder(srcUrl);
                    httpClient.BaseAddress = uri.Uri;
                    var requestUri = $"{srcUrl}/api/ResourceSettings/ImportSRCSettingsAggregateV2/";
                    HttpContent requestContent = new StringContent(srcFactoryHierarchy.ToString(), Encoding.UTF8, "application/json");

                    var response = httpClient.PostAsync(requestUri, requestContent);
                    var result = response.Result;
                    
                    success = result.IsSuccessStatusCode;

                    if(success)
                    {
                        responseBody = result.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        responseMessage = $"{(int) result.StatusCode} - {result.ReasonPhrase}: {srcUrl}";
                    }                  
                }
            }
            catch (AggregateException ex)
            {
                ex.Handle(err => {
                    responseMessage = GetNestedExceptionMessage(err, srcUrl);
                    return true;
                });                
            }
            catch (Exception ex)
            {
                responseMessage = GetNestedExceptionMessage(ex, srcUrl);
            }

            return new FHServiceResponse() { Success = success, Message = responseMessage, SrcResult = responseBody };
        }

        public static string GetNestedExceptionMessage(Exception ex, string url = "")
        {
            List<string> exceptionMessages = new List<string>();
            GetAllExceptionMessages(exceptionMessages, ex);
            var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
            var baseMsg = labelCache.GetLabelByName("SRCAPI_ConnectionFailed").Value;
            var allMsgs = string.Join(" - ", url, exceptionMessages.Last());
            return $"{baseMsg}{System.Environment.NewLine}{allMsgs}";
        }

        /// <summary>
        /// Recursively traverse inner exceptions and add messages to given list
        /// </summary>
        /// <param name="exceptionMessages"></param>
        /// <param name="ex"></param>
        public static void GetAllExceptionMessages(List<string> exceptionMessages, Exception ex)
        {
            if (ex == null)
                return;

            exceptionMessages.Add(ex.Message);
            GetAllExceptionMessages(exceptionMessages, ex.InnerException);
        }

        /// <summary>
        /// Construct localized error message based on given parameters
        /// </summary>
        /// <param name="factoryName"></param>
        /// <param name="settingsName"></param>
        /// <param name="srcApiUrl"></param>
        /// <returns></returns>
        public static string GetApiUrlErrorMessage(string factoryName, string settingsName, string srcApiUrl)
        {
            var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
            string errMsg = "";

            var baseMsg = labelCache.GetLabelByName("SRCAPI_ConnectionFailed").Value;

            // build error message
            if (string.IsNullOrEmpty(factoryName))
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                errMsg = string.Format(labelCache.GetLabelByName("SRCAPI_NotConfigNoFactoryUser").Value, session.SessionValues.UserName);
            }
            else if (string.IsNullOrEmpty(settingsName))
                errMsg = string.Format(labelCache.GetLabelByName("SRCAPI_NotConfigNoSettingsFactory").Value, factoryName);
            else if (string.IsNullOrEmpty(srcApiUrl))
                errMsg = string.Format(labelCache.GetLabelByName("SRCAPI_NotConfigSettings").Value, settingsName);

            return $"{baseMsg}{System.Environment.NewLine}{errMsg}";
        }

        /// <summary>
        /// Query for URL to communicate with SRC
        /// </summary>
        /// <param name="factoryName"></param>
        /// <param name="settingsName"></param>
        /// <param name="srcApiUrl"></param>
        /// <returns></returns>
        public static ResultStatus GetSrcApi(out string factoryName, out string settingsName, out string srcApiUrl)
        {
            factoryName = "";
            settingsName = "";
            srcApiUrl = "";

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

            if (session == null)
                return new ResultStatus("No session available to get SRC API.", false);

            var qParams = new QueryParameters()
            {
                Parameters = new QueryParameter[]
                {
                    new QueryParameter("employee", session.SessionValues.UserName)
                }
            };

            var recordSet = new RecordSet();
            var service = new QueryService(session.CurrentUserProfile);

            var resultStatus = service.Execute("GetSrcApi", qParams, new QueryOptions(), out recordSet);

            if (resultStatus.IsSuccess && recordSet.Rows != null && recordSet.Rows.Length > 0)
            {
                factoryName = recordSet.Rows[0].Values[0];
                settingsName = recordSet.Rows[0].Values[1];
                srcApiUrl = recordSet.Rows[0].Values[2];
            }

            return resultStatus;
        }

        /// <summary>
        /// Query for URL to communicate with SRC
        /// </summary>
        /// <param name="factoryName"></param>
        /// <param name="settingsName"></param>
        /// <param name="srcApiUrl"></param>
        /// <returns></returns>
        public static ResultStatus GetFactorySrcApi(string factoryName, out string settingsName, out string srcApiUrl)
        {
            settingsName = "";
            srcApiUrl = "";

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

            if (session == null)
                return new ResultStatus("No session available to get SRC API.", false);

            var qParams = new QueryParameters()
            {
                Parameters = new QueryParameter[]
                {
                    new QueryParameter("factory", factoryName)
                }
            };

            var recordSet = new RecordSet();
            var service = new QueryService(session.CurrentUserProfile);

            var resultStatus = service.Execute("GetFactorySrcApi", qParams, new QueryOptions(), out recordSet);

            if (resultStatus.IsSuccess && recordSet.Rows != null && recordSet.Rows.Length > 0)
            {
                settingsName = recordSet.Rows[0].Values[1];
                srcApiUrl = recordSet.Rows[0].Values[2];
            }

            return resultStatus;
        }
        /// <summary>
        /// For importing all resources within Valor MSS 2 exported file
        /// </summary>
        /// <param name="area">Resources in an area to be created (Cells and Equipments)</param>
        /// <returns></returns>
        [OperationContract]
        Dictionary<string, OS.ResultStatus> ImportAreaResources(AreaImport area)
        {
            // Get Localized Message
            Dictionary<string, string> messageLabels = new Dictionary<string, string>()
            {
                { "ExportImportDetailStatusEnum_Succeeded", "Succeeded" },
                { "ExportImportDetailStatusEnum_Skipped", "Skipped" },
                { "CompletionService", "Service completed successfully" },
                { "CPImport_ImportPollCount", "Import Count" }
            };
            messageLabels = GetLocalizedLabels(messageLabels);

            //Get existed resource
            Dictionary<string, string> allResources = GetAllResourceName();
            List<string> duplicateResource = new List<string>();

            if (allResources != null && allResources.Count > 0)
            {
                List<string> importResources = new List<string>();
                importResources.AddRange((from cell in area.cells select cell.name).ToList());
                importResources.AddRange((from cell in area.cells from eqp in cell.equipments select eqp.name).ToList());

                duplicateResource = importResources.Where(r => allResources.Any(s => s.Key == r)).ToList();
            }

            OS.ResultStatus resultStatus = new OS.ResultStatus();
            int importCount = 0;
            string resourceId;

            //Create Cell
            foreach (Cell cell in area.cells)
            {
                //Skip if exist
                if (!duplicateResource.Contains(cell.name))
                {
                    resultStatus = CreateResource(cell.name, cell.factoryLevelIndex, area.name, OS.FactoryLevelEnum.Cell, out resourceId);
                    if (!resultStatus.IsSuccess)
                        return new Dictionary<string, OS.ResultStatus>() { { "OpcenterResult", new OS.ResultStatus(resultStatus.ToString(), false) } };
                    importCount++;
                }

                //Create Equipment
                foreach (Equipment eqp in cell.equipments)
                {
                    //Skip if exist
                    if (!duplicateResource.Contains(eqp.name))
                    {
                        resultStatus = CreateResource(eqp.name, eqp.factoryLevelIndex, cell.name, OS.FactoryLevelEnum.Equipment, out resourceId);
                        if (!resultStatus.IsSuccess)
                            return new Dictionary<string, OS.ResultStatus>() { { "OpcenterResult", new OS.ResultStatus(resultStatus.ToString(), false) } };
                        importCount++;
                    }
                }
            }

            string importSuccessMsg = $"{messageLabels["CompletionService"]}. ({messageLabels["CPImport_ImportPollCount"]}: {importCount} {messageLabels["ExportImportDetailStatusEnum_Succeeded"]}{(duplicateResource.Count > 0 ? ", " + duplicateResource.Count + " " + messageLabels["ExportImportDetailStatusEnum_Skipped"] : "")})";
            return new Dictionary<string, OS.ResultStatus>() { { "OpcenterResult", new OS.ResultStatus(importSuccessMsg, true) } };
        }

        protected Dictionary<string, string> GetLocalizedLabels(Dictionary<string, string> labels)
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (fs != null)
            {
                var CachedLabels = fs.GetLabelCache();
                foreach (KeyValuePair<string, string> item in labels)
                {
                    string text = CachedLabels != null ? CachedLabels.GetLabelByName(item.Key).Value : string.Empty;
                    result.Add(item.Key, (text != null ? text : item.Value));
                }
            }
            return result;
        }

        private Dictionary<string, string> GetAllResourceName()
        {
            var svcParams = new OS.ResourceMaint();

            var request = new ResourceMaint_Request()
            {
                Info = new OS.ResourceMaint_Info
                {
                    ObjectListInquiry = new OS.Info(false, true)
                }
            };

            var result = new ResourceMaint_Result();
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new ResourceMaintService(session.CurrentUserProfile);
            OS.ResultStatus resultStatus = service.GetEnvironment(svcParams, request, out result);

            if (resultStatus.IsSuccess && result.Environment.ObjectListInquiry.SelectionValues.Rows != null)
                return result.Environment.ObjectListInquiry.SelectionValues.Rows.Select(s => new { key = s.Values[0].ToString(), value = s.Values[2].ToString() }).ToDictionary(t => t.key, t => t.value);
            else
                return null;
        }

        /// <summary>
        /// Call ResourceMaint to create Resource in Opcenter DB
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryLevelIndex"></param>
        /// <param name="parentResourceName"></param>
        /// <param name="level"></param>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        private OS.ResultStatus CreateResource(string name, int? factoryLevelIndex, string parentResourceName, OS.FactoryLevelEnum level, out string resourceId)
        {
            var svcParams = new OS.ResourceMaint()
            {
                ObjectChanges = new OS.ResourceChanges
                {
                    Name = name,
                    ParentResource = new OS.NamedObjectRef(parentResourceName),
                    FactoryLevel = level,
                    FactoryLevelIndex = factoryLevelIndex
                }
            };

            var request = new ResourceMaint_Request()
            {
                Info = new OS.ResourceMaint_Info()
                {
                    ObjectToChange = new OS.Info(true)
                }
            };

            var result = new ResourceMaint_Result();
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new ResourceMaintService(session.CurrentUserProfile);

            service.BeginTransaction();
            service.New(svcParams);
            service.ExecuteTransaction();
            OS.ResultStatus resultStatus = service.CommitTransaction(request, out result);

            if (resultStatus.IsSuccess && !result.IsEmpty)
                resourceId = result.Value.ObjectToChange.ID;
            else
                resourceId = null;

            return resultStatus;
        }

        /// <summary>
        /// 
        /// </summary>
        public class FHServiceResponse
        {
            public bool Success { get; set; }
            
            /// <summary>
            /// Could be success or failure message
            /// </summary>
            public string Message { get; set; }
            
            /// <summary>
            /// Response from any SRC API that we called
            /// </summary>
            public string SrcResult { get; set; }
        }

        /// <summary>
        /// Defines an Area and default settings to be imported from Valor MSS
        /// </summary>
        [DataContract]
        protected class MssImport
        {
            [DataMember]
            public AreaImport area { get; set; }

            /// <summary>
            /// Key: setting name, Value: setting value
            /// </summary>
            [DataMember]
            public Dictionary<string, string> defaultSettings { get; set; }
        }

        /// <summary>
        /// Defines an Area to be imported from Valor MSS
        /// </summary>
        [DataContract]
        protected class AreaImport
        {
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public Cell[] cells { get; set; }
            [DataMember]
            public bool importSRCSettings { get; set; }
        }

        [DataContract]
        protected class Cell
        {
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public Equipment[] equipments { get; set; }
            [DataMember]
            public int? factoryLevelIndex { get; set; }
        }

        [DataContract]
        protected class Equipment
        {
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public string machineType { get; set; }
            public bool IsInventoryLocation { get { return machineType == "Storage"; } }
            [DataMember]
            public int machineId { get; set; }
            [DataMember]
            public int? factoryLevelIndex { get; set; }
            /// <summary>
            /// Generic JSON object that will get deserialized into a Dictionary<string, string>
            /// </summary>
            [DataMember]
            public string overrideSettings { get; set; }
        }
    }
}
