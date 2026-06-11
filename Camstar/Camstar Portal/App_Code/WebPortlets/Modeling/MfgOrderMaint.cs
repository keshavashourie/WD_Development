// Copyright Siemens 2025
using Camstar.WCF.ObjectStack;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Newtonsoft.Json;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Siemens.OPCR.Diagnostics;
using System.Collections.Generic;
using Microsoft.Build.Tasks;
using System.Linq;
using System;

/// <summary>
/// 
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class MfgOrderMaint : ErpBomImportVP<MfgOrderMaterialListItmChanges>
    {
        #region Controls
        protected override JQDataGrid MaterialListItemGrid { get { return Page.FindCamstarControl("ObjectChanges_MaterialList") as JQDataGrid; } }
        protected CWC.TextBox MfgOrderName { get { return Page.FindCamstarControl("NameTxt") as CWC.TextBox; } }
        #endregion Controls

        #region Page Lifecycle Methods
        public MfgOrderMaint()
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ApplyFilter.Click += ApplyFilter_Click;
        }

        private void ApplyFilter_Click(object sender, EventArgs e)
        {
            ApplyFilterToGrid();
        }

        protected override IEnumerable<MfgOrderMaterialListItmChanges> GetMatchingItems(MfgOrderMaterialListItmChanges[] allItems, string filterValue)
        {
            return allItems.Where(ic =>
                (ic.Product?.Name != null && ic.Product.Name.ToLower().Contains(filterValue))
                || (ic.IssueControl != null && GetIssueControlLabel((IssueControlEnum)ic.IssueControl).ToLower().Contains(filterValue))
                || (ic.RouteStep?.Name != null && ic.RouteStep.Name.ToLower().Contains(filterValue))
                || (ic.ReferenceDesignator != null && ic.ReferenceDesignator.ToString().ToLower().Contains(filterValue))
                || (ic.QtyRequired != null && ic.QtyRequired.ToString().ToLower().Contains(filterValue))
                );
        }

        /// <summary>
        /// Blank out the Product from any item being imported from file
        /// </summary>
        /// <param name="service"></param>
        public override void GetInputData(Service service)
        {
            base.GetInputData(service);
            OM.MfgOrderMaint mfgOrderMaintSvc = (OM.MfgOrderMaint)service;

            if (mfgOrderMaintSvc?.ObjectChanges?.MaterialList == null)
                return;

            UpdateForUnverified(mfgOrderMaintSvc.ObjectChanges.MaterialList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateMe"></param>
        /// <param name="importItem"></param>
        protected override bool UpdateFromImport(MfgOrderMaterialListItmChanges updateMe, ImportItem importItem)
        {
            bool success = base.UpdateFromImport(updateMe, importItem);

            if (!string.IsNullOrEmpty(importItem.RouteStep))
            {
                updateMe.RouteStep = new NamedSubentityRef(importItem.RouteStep);
            }

            return success;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="changes"></param>
        /// <returns></returns>
        protected override dynamic GetItemsToUpdate(MfgOrderMaterialListItmChanges[] changes)
        {
            return changes.Select(momlic => new {
                Name = momlic.Name?.ToString(),
                Product = momlic.Product?.ToString(),
                IssueControl = momlic.IssueControl == null ? null : GetIssueControlLabel((IssueControlEnum)momlic.IssueControl),
                RouteStep = momlic.RouteStep?.Name?.ToString(),
                ReferenceDesignator = momlic.ReferenceDesignator?.ToString(),
                QtyRequired = momlic.QtyRequired?.ToString(),
            });
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
            MfgOrderMaintService svc = new MfgOrderMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);

            string bomName = MfgOrderName.Data.ToString();

            WCF.ObjectStack.MfgOrderMaint objToChangeMaint = new WCF.ObjectStack.MfgOrderMaint()
            {
                ObjectToChange = new NamedObjectRef(bomName)
            };
            WCF.ObjectStack.MfgOrderMaint changesMaint = new WCF.ObjectStack.MfgOrderMaint()
            {
                ObjectChanges = new Camstar.WCF.ObjectStack.MfgOrderChanges()
                {
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
        /// 
        /// </summary>
        /// <param name="listItemIndex"></param>
        /// <param name="updateItem"></param>
        protected override MfgOrderMaterialListItmChanges GetItemChangesFromUpdate(int? listItemIndex, UpdateItem updateItem)
        {
            MfgOrderMaterialListItmChanges changes = base.GetItemChangesFromUpdate(listItemIndex, updateItem);

            // add Route Step to base item changes
            if (!string.IsNullOrWhiteSpace(updateItem.RouteStepName))
            {
                changes.RouteStep = new NamedSubentityRef(updateItem.RouteStepName);
            }

            return changes;
        }

        #endregion Page Lifecycle Methods
    }
}