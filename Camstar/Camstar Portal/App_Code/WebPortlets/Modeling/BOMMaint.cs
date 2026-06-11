// Copyright Siemens 2025 
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Camstar.WCF.ObjectStack;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Siemens.OPCR.Diagnostics;
using System;

/// <summary>
/// 
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class BOMMaint : BillImportVP<ProductMaterialListItemChanges>
    {
        #region Controls
        #endregion Controls

        #region Page Lifecycle Methods
        public BOMMaint()
        {
            _log.ClassName = "BOMMaint";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ApplyFilter.Click += ApplyFilter_Click;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyFilter_Click(object sender, EventArgs e)
        {
            ApplyFilterToGrid();
        }

        protected override IEnumerable<ProductMaterialListItemChanges> GetMatchingItems(ProductMaterialListItemChanges[] allItems, string filterValue)
        {
            return allItems.Where(ic =>
                (ic.Product?.Name != null && ic.Product.Name.ToLower().Contains(filterValue))
                || (ic.IssueControl != null && GetIssueControlLabel((IssueControlEnum)ic.IssueControl).ToLower().Contains(filterValue))
                || (ic.Spec?.Name != null && ic.Spec.Name.ToLower().Contains(filterValue))
                || (ic.ReferenceDesignator != null && ic.ReferenceDesignator.ToString().ToLower().Contains(filterValue))
                || (ic.QtyRequired != null && ic.QtyRequired.ToString().ToLower().Contains(filterValue))
                );
        }

        /// <summary>
        /// Blank out the Product from any item being imported from file
        /// </summary>
        /// <param name="service"></param>
        public override void GetInputData(OM.Service service)
        {
            base.GetInputData(service);
            OM.BOMMaint bomMaintSvc = (OM.BOMMaint)service;
            
            if (bomMaintSvc?.ObjectChanges?.MaterialList == null)
                return;

            UpdateForUnverified(bomMaintSvc.ObjectChanges.MaterialList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="changes"></param>
        /// <returns></returns>
        protected override dynamic GetItemsToUpdate(ProductMaterialListItemChanges[] changes)
        {
            return changes.Select(pmlic => new {
                Name = pmlic.Name?.ToString(),
                Product = pmlic.Product?.ToString(),
                IssueControl = pmlic.IssueControl == null ? null : GetIssueControlLabel((IssueControlEnum)pmlic.IssueControl),
                Spec = pmlic.Spec?.ToString(),
                ReferenceDesignator = pmlic.ReferenceDesignator?.ToString(),
                QtyRequired = pmlic.QtyRequired?.ToString(),
            });
        }

        /// <summary>
        /// Update the given list item changes object with info from the given line item from the import file
        /// </summary>
        /// <param name="updateMe"></param>
        /// <param name="importItem"></param>
        override protected bool UpdateFromImport(ProductMaterialListItemChanges updateMe, ImportItem importItem)
        {
            bool success = base.UpdateFromImport(updateMe, importItem);

            RevisionedObjectRef spec = GetSpec(importItem);
            if (spec != null)
                updateMe.Spec = spec;

            return success;
        }

        /// <summary>
        /// Used for creating sub-BOMs
        /// </summary>
        /// <param name="ii"></param>
        /// <returns></returns>
        protected virtual ProductMaterialListItemChanges GetListItemChanges(ImportItem ii)
        {
            return
            new ProductMaterialListItemChanges()
            {
                ReferenceDesignator = ii.RefDes,
                //Product = new RevisionedObjectRef(ii.ProductName, ii.ProductRevision),
                UnverifiedProductName = ii.ProductName,
                UnverifiedProductRevision = ii.ProductRevision,
                QtyRequired = ii.Quantity,
                IssueControl = (IssueControlEnum)ii.IssueControl,
                AllowOverConsumption = ii.AllowOverConsumption,
                AllowUnderConsumption = ii.AllowUnderConsumption
            };
        }

        /// <summary>
        /// Construct Spec obj from given imported item
        /// </summary>
        /// <param name="importItem"></param>
        /// <returns>null if <paramref name="importItem"/> does not contain any Spec info</returns>
        protected RevisionedObjectRef GetSpec(ImportItem importItem)
        {
            RevisionedObjectRef spec = null;

            if (!string.IsNullOrWhiteSpace(importItem.SpecName))
                spec = string.IsNullOrWhiteSpace(importItem.SpecRevision)
                    ? new RevisionedObjectRef(importItem.SpecName)
                    : new RevisionedObjectRef(importItem.SpecName, importItem.SpecRevision);

            return spec;
        }

        /// <summary>
        /// Make sure any Spec assigned to an import items exists.  If it doesn't, clear the Spec value from the import item
        /// </summary>
        /// <param name="importItems"></param>
        /// <returns></returns>
        protected bool ValidateSpecs(List<ImportItem> importItems)
        {
            bool allSpecsExist = true;

            // grab one import item for each non-empty spec value
            List<ImportItem> specItems = importItems
                .Where(ii => !string.IsNullOrWhiteSpace(ii.SpecName))
                .GroupBy(ii => new { ii.SpecName, ii.SpecRevision })
                .Select(g => g.First())
                .ToList();

            SpecMaintService svc = new SpecMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            
            // try to load each Spec
            foreach(var specItem in specItems)
            {
                SpecMaint_Result result = new SpecMaint_Result();

                Camstar.WCF.ObjectStack.SpecMaint maint = new Camstar.WCF.ObjectStack.SpecMaint()
                {
                    ObjectToChange = GetSpec(specItem)
                };
                
                ResultStatus resStatus = svc.Load(maint, new SpecMaint_Request(), out result);
                if(!resStatus.IsSuccess)
                {
                    allSpecsExist = false;

                    // write to log file
                    _log.LogMessage(LogSeverity.Error, "VerifySpecsExist", $"Spec {string.Join(",", specItem.SpecName, specItem.SpecRevision)} does not exist.  Cannot assign to items.");

                    // remove this Spec from any import item
                    foreach (ImportItem importItem in importItems) 
                    {
                        if (importItem.SpecName == specItem.SpecName && importItem.SpecRevision == specItem.SpecRevision)
                        {
                            importItem.SpecName = "";
                            importItem.SpecRevision = "";
                        }
                    }
                }
            }

            return allSpecsExist;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="rev"></param>
        /// <param name="importItems"></param>
        /// <returns></returns>
        protected override ResultStatus CreateBom(string name, string revision, List<ImportItem> importItems)
        {
            // verify any Specs already exist
            bool allSpecsValid = ValidateSpecs(importItems);

            ProductMaterialListItemChanges[] listItems;
            bool bomRevExists = LoadMaterialList(name, revision, out listItems);
            bool bomBaseExists = !bomRevExists && LoadMaterialList(name, out listItems);

            BOMMaintService svc = new BOMMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);

            var bomChanges = new BOMChanges()
            {
                Name = name,
                Revision = revision,
                // add new items to create
                MaterialList = importItems.Select(ii => GetListItemChanges(ii)).ToArray()
            };

            svc.BeginTransaction();

            if (bomRevExists)
            {
                // add list of existing items to delete
                bomChanges.MaterialList = bomChanges.MaterialList.Concat(
                    listItems
                    .OrderByDescending(li => li.ListItemIndex)
                    .Select(
                        li => new ProductMaterialListItemChanges()
                        {
                            ListItemIndex = li.ListItemIndex,
                            ListItemAction = ListItemAction.Delete
                        })
                ).ToArray();

                WCF.ObjectStack.BOMMaint objToChangeMaint = new WCF.ObjectStack.BOMMaint()
                {
                    ObjectToChange = new RevisionedObjectRef(name, revision)
                };
                WCF.ObjectStack.BOMMaint changesMaint = new WCF.ObjectStack.BOMMaint()
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
                    WCF.ObjectStack.BOMMaint baseMaint = new WCF.ObjectStack.BOMMaint()
                    {
                        BaseToChange = new RevisionedObjectRef(name)
                        {
                            Revision = null,
                            RevisionOfRecord = null
                        },
                    };

                    WCF.ObjectStack.BOMMaint changesMaint = new WCF.ObjectStack.BOMMaint()
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
                    WCF.ObjectStack.BOMMaint maint = new WCF.ObjectStack.BOMMaint()
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
            if (bomCreateStatus.IsSuccess && !allSpecsValid)
                bomCreateStatus = new ResultStatus($"BOM {string.Join(",", name, revision)} Spec validation failed", false);

            return bomCreateStatus;
        }

        /// <summary>
        /// BOM items have been updated - save those changes
        /// </summary>
        /// <param name="updateItems"></param>
        /// <returns></returns>
        protected override void UpdateBom(List<UpdateItem> updateItems)
        {
            if (MaterialListItemGrid?.Data == null || string.IsNullOrEmpty(UpdatedBomItemsJson.Data?.ToString()))
                return;

            // save
            BOMMaintService svc = new BOMMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);

            string bomName = BomName.Data.ToString();
            string bomRevision = BomRevision.Data.ToString();

            WCF.ObjectStack.BOMMaint objToChangeMaint = new WCF.ObjectStack.BOMMaint()
            {
                ObjectToChange = new RevisionedObjectRef(bomName, bomRevision)
            };
            WCF.ObjectStack.BOMMaint changesMaint = new WCF.ObjectStack.BOMMaint()
            {
                ObjectChanges = new BOMChanges()
                {
                    Name = bomName,
                    Revision = bomRevision,
                    MaterialList = GetUpdatedChanges(updateItems)
                }
            };

            // update existing
            svc.BeginTransaction();
            svc.Load(objToChangeMaint);
            svc.ExecuteTransaction(changesMaint);
            ResultStatus result = svc.CommitTransaction();
            Page.StatusBar.WriteStatus(result);

            if (!result.IsSuccess)
            {
                _log.LogMessage(LogSeverity.Error, "UpdateBom", result.Message);
                return;
            }

            Page.LoadModelingValues(false);
        }

        /// <summary>
        /// Construct changes obj to use in maint update txn
        /// </summary>
        /// <param name="listItemIndex"></param>
        /// <param name="updateItem"></param>
        protected override ProductMaterialListItemChanges GetItemChangesFromUpdate(int? listItemIndex, UpdateItem updateItem)
        {
            ProductMaterialListItemChanges changes = base.GetItemChangesFromUpdate(listItemIndex, updateItem);

            // add Spec to base item changes
            if (!string.IsNullOrWhiteSpace(updateItem.SpecNameRev))
            {
                string[] specNameRev = ParseRevisionedObject(updateItem.SpecNameRev);

                changes.Spec = specNameRev.Length == 1
                    ? new RevisionedObjectRef(specNameRev[0])
                    : new RevisionedObjectRef(specNameRev[0], specNameRev[1]);
            }

            return changes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="revision"></param>
        /// <param name="listItems">Material list</param>
        /// <returns>Does the BOM already exist?</returns>
        protected bool LoadMaterialList(string name, string revision, out ProductMaterialListItemChanges[] listItems)
        {
            return LoadMaterialList(new RevisionedObjectRef(name, revision), out listItems);
        }

        protected bool LoadMaterialList(string name, out ProductMaterialListItemChanges[] listItems)
        {
            return LoadMaterialList(new RevisionedObjectRef(name), out listItems);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bomRef">Load list for this BOM</param>
        /// <param name="listItems">Put list items here</param>
        /// <returns></returns>
        protected bool LoadMaterialList(RevisionedObjectRef bomRef, out ProductMaterialListItemChanges[] listItems)
        {
            listItems = new ProductMaterialListItemChanges[0];

            BOMMaintService svc = new BOMMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);

            WCF.ObjectStack.BOMMaint maint = new WCF.ObjectStack.BOMMaint();
            maint.ObjectToChange = bomRef;

            BOMMaint_Request request = new BOMMaint_Request()
            {
                Info = new BOMMaint_Info()
                {
                    ObjectChanges = new BOMChanges_Info()
                    {
                        MaterialList = new ProductMaterialListItemChanges_Info()
                        {
                            RequestValue = true
                        }
                    }
                }
            };

            BOMMaint_Result result;
            ResultStatus loadStatus = svc.Load(maint, request, out result);

            if (loadStatus.IsSuccess)
                listItems = result.Value.ObjectChanges.MaterialList;

            return loadStatus.IsSuccess;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="revision"></param>
        /// <returns>Object for updating a Product with a new BOM</returns>
        protected override ProductChanges GetProductChanges(string name, string revision)
        {
            return new ProductChanges()
            {
                Name = name,
                Revision = revision,
                BOM = new RevisionedObjectRef(name, revision)
            };
        }

        #endregion Page Lifecycle Methods
    }

    /// <summary>
    /// Add BOM-specific fields
    /// </summary>
    public partial class ImportItem
    {
        public string SpecName { get; set; }
        public string SpecRevision { get; set; }
    }
}