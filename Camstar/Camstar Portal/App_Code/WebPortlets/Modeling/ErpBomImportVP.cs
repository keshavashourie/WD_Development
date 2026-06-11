// Copyright Siemens 2023 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Newtonsoft.Json;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Siemens.OPCR.Diagnostics;
using System.Web;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.PortalFramework;


namespace Camstar.WebPortal.WebPortlets.Modeling
{
    /// <summary>
    /// Any page that imports BOMs consisting of a list of ERPMaterialListItems (ERP BOM Maint)
    /// </summary>
    /// <typeparam name="TItemChanges">Item grid contains changes of this type</typeparam>
    public class ErpBomImportVP<TListItemChanges> : BillImportVP<TListItemChanges> 
        where TListItemChanges : ERPMaterialListItemChanges, new()
    {

        protected CWC.RevisionedObject ERPRoute { get { return Page.FindCamstarControl("ERPRoute") as CWC.RevisionedObject; } }

        #region Page Lifecycle Methods
        public ErpBomImportVP()
        {
            _log.ClassName = "ErpBomImportVP";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ii"></param>
        /// <returns></returns>
        protected virtual BOMMaterialListItemChanges GetListItemChanges(ImportItem ii)
        {
            return
            new BOMMaterialListItemChanges()
            {
                ReferenceDesignator = ii.RefDes,
                //Product = new RevisionedObjectRef(ii.ProductName, ii.ProductRevision),
                RouteStep = new NamedSubentityRef(ii.RouteStep),
                UnverifiedProductName = ii.ProductName,
                UnverifiedProductRevision = ii.ProductRevision,
                QtyRequired = ii.Quantity,
                IssueControl = (IssueControlEnum)ii.IssueControl,
                AllowOverConsumption = ii.AllowOverConsumption,
                AllowUnderConsumption = ii.AllowUnderConsumption
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateMe"></param>
        /// <param name="importItem"></param>
        protected override bool UpdateFromImport(TListItemChanges updateMe, ImportItem importItem)
        {
            bool success = base.UpdateFromImport(updateMe, importItem);
            updateMe.RouteStep = new NamedSubentityRef(importItem.RouteStep);

            return success;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="revision"></param>
        /// <param name="importItems"></param>
        /// <returns></returns>
        protected override ResultStatus CreateBom(string name, string revision, List<ImportItem> importItems)
        {
            bool allStepsValid = true;
            RevisionedObjectRef erpRoute = null;
            if (ERPRoute.Data !=null)
            {
                erpRoute = (RevisionedObjectRef)ERPRoute.Data;
                allStepsValid = ValidateRouteSteps(importItems, erpRoute);
            }

            BOMMaterialListItemChanges[] listItems;
            bool bomRevExists = LoadMaterialList(name, revision, out listItems);
            bool bomBaseExists = !bomRevExists && LoadMaterialList(name, out listItems);

            ERPBOMMaintService svc = new ERPBOMMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);

            var bomChanges = new ERPBOMChanges()
            {
                Name = name,
                Revision = revision,
                ERPRoute = erpRoute,
                MaterialList =
                // add new items to create
                    importItems
                    .Select(ii => GetListItemChanges(ii)).ToArray()
            };

            svc.BeginTransaction();

            if (bomRevExists)
            {
                // add list of existing items to delete
                bomChanges.MaterialList = bomChanges.MaterialList.Concat(
                    listItems
                    .OrderByDescending(li => li.ListItemIndex)
                    .Select(
                        li => new BOMMaterialListItemChanges()
                        {
                            ListItemIndex = li.ListItemIndex,
                            ListItemAction = ListItemAction.Delete
                        })
                ).ToArray();

                WCF.ObjectStack.ERPBOMMaint objToChangeMaint = new WCF.ObjectStack.ERPBOMMaint()
                {
                    ObjectToChange = new RevisionedObjectRef(name, revision)
                };
                WCF.ObjectStack.ERPBOMMaint changesMaint = new WCF.ObjectStack.ERPBOMMaint()
                {
                    ObjectChanges = bomChanges
                };

                // update existing
                svc.Load(objToChangeMaint);
                svc.ExecuteTransaction(changesMaint);
            }
            else
            {
                if (bomBaseExists)
                {
                    WCF.ObjectStack.ERPBOMMaint baseMaint = new WCF.ObjectStack.ERPBOMMaint()
                    {
                        BaseToChange = new RevisionedObjectRef(name)
                        {
                            Revision = null,
                            RevisionOfRecord = null
                        },
                    };

                    WCF.ObjectStack.ERPBOMMaint changesMaint = new WCF.ObjectStack.ERPBOMMaint()
                    {
                        SyncName = name,
                        SyncRevision = revision,
                        ObjectChanges = bomChanges
                    };

                    svc.NewRev(baseMaint);
                    svc.ExecuteTransaction(changesMaint);
                }
                else
                {
                    // create new
                    WCF.ObjectStack.ERPBOMMaint maint = new WCF.ObjectStack.ERPBOMMaint()
                    {
                        SyncName = name,
                        SyncRevision = revision,
                        ObjectChanges = bomChanges,
                    };

                    svc.New(maint);
                    svc.ExecuteTransaction();
                }

            }

            ResultStatus bomCreateStatus = svc.CommitTransaction();
            if (bomCreateStatus.IsSuccess && !allStepsValid)
                bomCreateStatus = new ResultStatus($"BOM {string.Join(",", name, revision)} Route Step validation failed", false);

            return bomCreateStatus;
        }

        /// <summary>
        /// Make sure any Route Step assigned to an import items exists.  If it doesn't, clear the Route Step from the import item
        /// </summary>
        /// <param name="importItems"></param>
        /// <returns></returns>
        protected bool ValidateRouteSteps(List<ImportItem> importItems, RevisionedObjectRef erpRoute)
        {
            bool allStepsExist = true;

            if (erpRoute == null || string.IsNullOrEmpty(erpRoute.Name))
                return true;

            // grab list of distinct
            bool anyRouteSteps = importItems.Any(ii => !string.IsNullOrEmpty(ii.RouteStep));
            if(!anyRouteSteps)
            {
                // no Route Steps, nothing to validate
                return true;
            }

            // load all steps in the Route
            ERPRouteMaintService svc = new ERPRouteMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            ERPRouteMaint_Result result = new ERPRouteMaint_Result();
            ERPRouteMaint maint = new ERPRouteMaint()
            {
                ObjectToChange = erpRoute
            };

            // request list of steps for this route
            ERPRouteMaint_Request request = new ERPRouteMaint_Request()
            {
                Info = new ERPRouteMaint_Info()
                {
                    ObjectChanges = new ERPRouteChanges_Info()
                    {
                        RouteSteps = new RouteStepChanges_Info()
                        {
                            RequestValue = true
                        }
                    }
                }
            };

            ResultStatus resStatus = svc.Load(maint, request, out result);
            if (!resStatus.IsSuccess)
            {
                _log.LogMessage(LogSeverity.Error, "ValidateRouteSteps", $"Route {string.Join(",", erpRoute.Name, erpRoute.Revision)} does not exist.  Unable to validate steps.");
                foreach(ImportItem ii in importItems)
                {
                    // blank out invalid (all) Route Steps and keep going with the import
                    ii.RouteStep = "";
                }

                return false;
            }

            foreach(ImportItem ii in importItems)
            {
                if(!string.IsNullOrEmpty(ii.RouteStep))
                {
                    bool stepExistsInRoute = result.Value.ObjectChanges.RouteSteps.Any(rsc => string.Compare(rsc.Name.Value, ii.RouteStep, true) == 0); 
                    if(!stepExistsInRoute)
                    {
                        _log.LogMessage(LogSeverity.Error, "ValidateRouteSteps", $"Item {ii.RefDes} assigned Route Step {ii.RouteStep} that does not exist in Route {string.Join(",", erpRoute.Name, erpRoute.Revision)}.");
                        ii.RouteStep = "";
                        allStepsExist = false;
                    }
                }
            }

             return allStepsExist;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="revision"></param>
        /// <param name="listItems">Material list</param>
        /// <returns>Does the BOM already exist?</returns>
        protected bool LoadMaterialList(string name, string revision, out BOMMaterialListItemChanges[] listItems)
        {
            return LoadMaterialList(new RevisionedObjectRef(name, revision), out listItems);
        }

        protected bool LoadMaterialList(string name, out BOMMaterialListItemChanges[] listItems)
        {
            return LoadMaterialList(new RevisionedObjectRef(name), out listItems);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bomRef"></param>
        /// <param name="listItems"></param>
        /// <returns></returns>
        protected bool LoadMaterialList(RevisionedObjectRef bomRef, out BOMMaterialListItemChanges[] listItems)
        {
            listItems = new BOMMaterialListItemChanges[0];

            ERPBOMMaintService svc = new ERPBOMMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);

            WCF.ObjectStack.ERPBOMMaint maint = new WCF.ObjectStack.ERPBOMMaint();
            maint.ObjectToChange = bomRef;

            ERPBOMMaint_Request request = new ERPBOMMaint_Request()
            {
                Info = new ERPBOMMaint_Info()
                {
                    ObjectChanges = new ERPBOMChanges_Info()
                    {
                        MaterialList = new BOMMaterialListItemChanges_Info()
                        {
                            RequestValue = true
                        }
                    }
                }
            };

            // does this BOM already exist?
            ERPBOMMaint_Result result;
            ResultStatus existingLoadStatus = svc.Load(maint, request, out result);

            if (existingLoadStatus.IsSuccess)
                listItems = result.Value.ObjectChanges.MaterialList;

            return existingLoadStatus.IsSuccess;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="revision"></param>
        /// <returns>Object to update a Product's ERP BOM</returns>
        protected override ProductChanges GetProductChanges(string name, string revision)
        {
            return new ProductChanges()
            {
                Name = name,
                Revision = revision,
                ERPBOM = new RevisionedObjectRef(name, revision)
            };
        }

        #endregion Page Lifecycle Methods
    }

    public partial class ImportItem
    {
        public string RouteStep { get; set; }
    }
}