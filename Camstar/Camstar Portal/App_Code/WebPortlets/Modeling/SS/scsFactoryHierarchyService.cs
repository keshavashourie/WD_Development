// Copyright Siemens 2024  
using System.ServiceModel;
using System.Web;

using Camstar.WebPortal.FormsFramework.Utilities;
using OS = Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;

using Newtonsoft.Json;
using Camstar.WCF.ObjectStack;
using System.Collections.Generic;
using System;
using WcfUtil = Camstar.WebPortal.WCFUtilities;
using System.Data;

namespace WebClientPortal
{

    public partial class FactoryHierarchyService
    {
        /// <summary>
        /// Get all linkable objects that are not currently in the FHM
        /// </summary>
        /// <param name="linkableResults">Return the 'unlinked' resources in an array.</param>
        /// <returns>A status object</returns>
        /// <remarks>
        /// All get 'linkable objects' methods use the same parameters name and types. This makes the calls easier to 
        /// perform on the client.
        /// </remarks>
        [OperationContract]
        OS.ResultStatus GetLinkableEquipmentWS40(out EquipmentWS40ForLinkPopup linkableResults)
        {
            OS.ResultStatus resStatus = new OS.ResultStatus()
            {
                IsSuccess = true
            };

            List<LinkableChild> linkableResourceResults;
            GetLinkableResources(out linkableResourceResults);

            List<LinkableChild> linkableInventoryLocationsResults;
            GetLinkableIsInventoryLocations(out linkableInventoryLocationsResults);

            List<LinkableChild> linkableAssemblyEquipmentResults;
            GetLinkableSemiEquipment("ASSEMBLY", out linkableAssemblyEquipmentResults);

            List<LinkableChild> linkableBackGrindEquipmentResults;
            GetLinkableSemiEquipment("BACKGRIND", out linkableBackGrindEquipmentResults);

            List<LinkableChild> linkableTestEquipmentResults;
            GetLinkableSemiEquipment("TEST", out linkableTestEquipmentResults);

            List<LinkableChild> linkableWaferEquipmentResults;
            GetLinkableSemiEquipment("WAFER", out linkableWaferEquipmentResults);

            List<LinkableChild> linkableWaferSortEquipmentResults;
            GetLinkableSemiEquipment("WAFERSORT", out linkableWaferSortEquipmentResults);

            linkableResults = new EquipmentWS40ForLinkPopup()
            {
                Equipment = linkableResourceResults,
                InventoryLocations = linkableInventoryLocationsResults,
                AssemblyEquipment = linkableAssemblyEquipmentResults,
                BackGrindEquipment = linkableBackGrindEquipmentResults,
                TestEquipment = linkableTestEquipmentResults,
                WaferEquipment = linkableWaferEquipmentResults,
                WaferSortEquipment = linkableWaferSortEquipmentResults
            };

            return resStatus;
        }

        /// <summary>
        /// Get SEMI Equipment objects that are not currently in the FHM
        /// </summary>
        /// <param name="linkableResults">Return the 'unlinked' resources in an array.</param>
        /// <returns>A status object</returns>
        OS.ResultStatus GetLinkableSemiEquipment(string objectType, out List<LinkableChild> linkableResults)
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
                QueryParameters queryParameters = new QueryParameters();
                queryParameters.Parameters = new QueryParameter[1];
                queryParameters.Parameters[0] = new QueryParameter("ObjectType", objectType);
                DataTable resourceTable = qUtil.Execute("scsResourceFhmLinkable", queryParameters.Parameters, new QueryOptions(), ref msg);

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
        /// Take an array of child items and add them to the FHM
        /// </summary>
        /// <param name="parentName">The name of the cell resource to link to</param>
        /// <param name="parentType">A text value of the parent type based on the FHM data types. Not currently used.</param>
        /// <param name="childItems">An array of object names that need to be linked</param>
        /// <returns>A status object</returns>
        /// <remarks>
        /// All 'link' methods use the same 3 parameters names and types. This makes the calls easier to 
        /// perform on the client
        /// </remarks>
        [OperationContract]
        OS.ResultStatus LinkEquipmentWS40ToCell(string parentName, string parentType, Dictionary<string, string[]> childItems)
        {
            const string equipmentCollectionKey = "Equipment";
            const string inventoryLocationCollectionKey = "InventoryLocations";
            const string assemblyEquipmentCollectionKey = "AssemblyEquipment";
            const string waferEquipmentCollectionKey = "WaferEquipment";
            const string waferSortEquipmentCollectionKey = "WaferSortEquipment";
            const string backGrindEquipmentCollectionKey = "BackGrindEquipment";
            const string testEquipmentCollectionKey = "TestEquipment";

            OS.ResultStatus resStatus = new OS.ResultStatus()
            {
                IsSuccess = true
            };

            string[] childResources = childItems[equipmentCollectionKey];
            resStatus = LinkResourcesToParent(parentName, parentType, childResources);

            //only applicable if Industry Solution is installed.
			if (childItems.ContainsKey(inventoryLocationCollectionKey))
			{
				string[] childInventoryLocations = childItems[inventoryLocationCollectionKey];
				if (childInventoryLocations.Length > 0)
				{
					resStatus = LinkIsInventoryLocationsToCell(parentName, parentType, childInventoryLocations);
				}
			}

            string[] childAssemblyEquipment = childItems[assemblyEquipmentCollectionKey];
            if (childAssemblyEquipment.Length > 0)
            {
                resStatus = LinkAssemblyEquipmentToCell(parentName, parentType, childAssemblyEquipment);
            }

            string[] childBackGrindEquipment = childItems[backGrindEquipmentCollectionKey];
            if (childBackGrindEquipment.Length > 0)
            {
                resStatus = LinkBackGrindEquipmentToCell(parentName, parentType, childBackGrindEquipment);
            }

            string[] childTestEquipment = childItems[testEquipmentCollectionKey];
            if (childTestEquipment.Length > 0)
            {
                resStatus = LinkTestEquipmentToCell(parentName, parentType, childTestEquipment);
            }

            string[] childWaferEquipment = childItems[waferEquipmentCollectionKey];
            if (childWaferEquipment.Length > 0)
            {
                resStatus = LinkWaferEquipmentToCell(parentName, parentType, childWaferEquipment);
            }

            string[] childWaferSortEquipment = childItems[waferSortEquipmentCollectionKey];
            if (childWaferSortEquipment.Length > 0)
            {
                resStatus = LinkWaferSortEquipmentToCell(parentName, parentType, childWaferSortEquipment);
            }

            return resStatus;
        }


        /// <summary>
        /// Take an array of Assembly Equipment objects and add them to the FHM
        /// </summary>
        /// <param name="parentName">The name of the cell resource to link to</param>
        /// <param name="parentType">A text value of the parent type based on the FHM data types. Not currently used.</param>
        /// <param name="childItems">An array of Assembly Equipment names that need to be linked</param>
        /// <returns>A status object</returns>
        [OperationContract]
        OS.ResultStatus LinkAssemblyEquipmentToCell(string parentName, string parentType, string[] childItems)
        {
            OS.ResultStatus resStatus = new OS.ResultStatus()
            {
                IsSuccess = true
            };

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new AssemblyEquipmentMaintService(session.CurrentUserProfile);

            try
            {
                foreach (var locationName in childItems)
                {
                    AssemblyEquipmentMaint maint = new AssemblyEquipmentMaint();
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

                    maint.ObjectChanges = new AssemblyEquipmentChanges()
                    {
                        ParentResource = new NamedObjectRef(parentName),
                        Name = locationName,
                        FactoryLevel = OS.FactoryLevelEnum.Equipment
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
        /// Take an array of Back Grind Equipment and add them to the FHM
        /// </summary>
        /// <param name="parentName">The name of the cell resource to link to</param>
        /// <param name="parentType">A text value of the parent type based on the FHM data types. Not currently used.</param>
        /// <param name="childItems">An array of Back Grind Equipment names that need to be linked</param>
        /// <returns>A status object</returns>
        [OperationContract]
        OS.ResultStatus LinkBackGrindEquipmentToCell(string parentName, string parentType, string[] childItems)
        {
            OS.ResultStatus resStatus = new OS.ResultStatus()
            {
                IsSuccess = true
            };

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new BackGrindEquipmentMaintService(session.CurrentUserProfile);

            try
            {
                foreach (var locationName in childItems)
                {
                    BackGrindEquipmentMaint maint = new BackGrindEquipmentMaint();
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

                    maint.ObjectChanges = new BackGrindEquipmentChanges()
                    {
                        ParentResource = new NamedObjectRef(parentName),
                        Name = locationName,
                        FactoryLevel = OS.FactoryLevelEnum.Equipment
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
        /// Take an array of Test Equipment and add them to the FHM
        /// </summary>
        /// <param name="parentName">The name of the cell resource to link to</param>
        /// <param name="parentType">A text value of the parent type based on the FHM data types. Not currently used.</param>
        /// <param name="childItems">An array of Test Equipment names that need to be linked</param>
        /// <returns>A status object</returns>
        [OperationContract]
        OS.ResultStatus LinkTestEquipmentToCell(string parentName, string parentType, string[] childItems)
        {
            OS.ResultStatus resStatus = new OS.ResultStatus()
            {
                IsSuccess = true
            };

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new TestEquipmentMaintService(session.CurrentUserProfile);

            try
            {
                foreach (var locationName in childItems)
                {
                    TestEquipmentMaint maint = new TestEquipmentMaint();
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

                    maint.ObjectChanges = new TestEquipmentChanges()
                    {
                        ParentResource = new NamedObjectRef(parentName),
                        Name = locationName,
                        FactoryLevel = OS.FactoryLevelEnum.Equipment
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
        /// Take an array of Wafer Equipment and add them to the FHM
        /// </summary>
        /// <param name="parentName">The name of the cell resource to link to</param>
        /// <param name="parentType">A text value of the parent type based on the FHM data types. Not currently used.</param>
        /// <param name="childItems">An array of Wafer Equipment names that need to be linked</param>
        /// <returns>A status object</returns>
        [OperationContract]
        OS.ResultStatus LinkWaferEquipmentToCell(string parentName, string parentType, string[] childItems)
        {
            OS.ResultStatus resStatus = new OS.ResultStatus()
            {
                IsSuccess = true
            };

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new WaferEquipmentMaintService(session.CurrentUserProfile);

            try
            {
                foreach (var locationName in childItems)
                {
                    WaferEquipmentMaint maint = new WaferEquipmentMaint();
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

                    maint.ObjectChanges = new WaferEquipmentChanges()
                    {
                        ParentResource = new NamedObjectRef(parentName),
                        Name = locationName,
                        FactoryLevel = OS.FactoryLevelEnum.Equipment
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
        /// Take an array of Wafer Sort Equipment and add them to the FHM
        /// </summary>
        /// <param name="parentName">The name of the cell resource to link to</param>
        /// <param name="parentType">A text value of the parent type based on the FHM data types. Not currently used.</param>
        /// <param name="childItems">An array of Wafer Sort Equipment names that need to be linked</param>
        /// <returns>A status object</returns>
        [OperationContract]
        OS.ResultStatus LinkWaferSortEquipmentToCell(string parentName, string parentType, string[] childItems)
        {
            OS.ResultStatus resStatus = new OS.ResultStatus()
            {
                IsSuccess = true
            };

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new WaferSortEquipmentMaintService(session.CurrentUserProfile);

            try
            {
                foreach (var locationName in childItems)
                {
                    WaferSortEquipmentMaint maint = new WaferSortEquipmentMaint();
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

                    maint.ObjectChanges = new WaferSortEquipmentChanges()
                    {
                        ParentResource = new NamedObjectRef(parentName),
                        Name = locationName,
                        FactoryLevel = OS.FactoryLevelEnum.Equipment
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

        public class EquipmentWS40ForLinkPopup
        {
            public List<LinkableChild> Equipment { get; set; }
            public List<LinkableChild> InventoryLocations { get; set; }
            public List<LinkableChild> AssemblyEquipment { get; set; }
            public List<LinkableChild> BackGrindEquipment { get; set; }
            public List<LinkableChild> TestEquipment { get; set; }
            public List<LinkableChild> WaferEquipment { get; set; }
            public List<LinkableChild> WaferSortEquipment { get; set; }

        }
    }
}
