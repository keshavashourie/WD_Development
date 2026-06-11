// Copyright Siemens 2025
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Siemens.OPCR.Diagnostics;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;

/// <summary>
/// 
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class ERPBOMMaint : ErpBomImportVP<BOMMaterialListItemChanges>
    {
        #region Controls
        protected override JQDataGrid MaterialListItemGrid { get { return Page.FindCamstarControl("MaterialList") as JQDataGrid; } }
        #endregion Controls


        #region Page Lifecycle Methods
        public ERPBOMMaint()
        {
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allItems"></param>
        /// <param name="filterValue"></param>
        /// <returns></returns>
        protected override IEnumerable<BOMMaterialListItemChanges> GetMatchingItems(BOMMaterialListItemChanges[] allItems, string filterValue)
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
        public override void GetInputData(OM.Service service)
        {
            base.GetInputData(service);
            OM.ERPBOMMaint erpBomMaintSvc = (OM.ERPBOMMaint)service;
            
            if (erpBomMaintSvc?.ObjectChanges?.MaterialList == null)
                return;

            UpdateForUnverified(erpBomMaintSvc.ObjectChanges.MaterialList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="changes"></param>
        /// <returns></returns>
        protected override dynamic GetItemsToUpdate(BOMMaterialListItemChanges[] changes)
        {
            return changes.Select(bmlic => new {
                Name = bmlic.Name?.ToString(),
                Product = bmlic.Product?.ToString(),
                IssueControl = bmlic.IssueControl == null ? null : GetIssueControlLabel((IssueControlEnum)bmlic.IssueControl),
                RouteStep = bmlic.RouteStep?.Name?.ToString(),
                ReferenceDesignator = bmlic.ReferenceDesignator?.ToString(),
                QtyRequired = bmlic.QtyRequired?.ToString(),
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
            ERPBOMMaintService svc = new ERPBOMMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);

            string bomName = BomName.Data.ToString();
            string bomRevision = BomRevision.Data.ToString();

            WCF.ObjectStack.ERPBOMMaint objToChangeMaint = new WCF.ObjectStack.ERPBOMMaint()
            {
                ObjectToChange = new RevisionedObjectRef(bomName, bomRevision)
            };
            WCF.ObjectStack.ERPBOMMaint changesMaint = new WCF.ObjectStack.ERPBOMMaint()
            {
                ObjectChanges = new ERPBOMChanges()
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
        /// 
        /// </summary>
        /// <param name="listItemIndex"></param>
        /// <param name="updateItem"></param>
        protected override BOMMaterialListItemChanges GetItemChangesFromUpdate(int? listItemIndex, UpdateItem updateItem)
        {
            BOMMaterialListItemChanges changes = base.GetItemChangesFromUpdate(listItemIndex, updateItem);

            // add Route Step to base item changes
            if (!string.IsNullOrWhiteSpace(updateItem.RouteStepName))
            {
                changes.RouteStep = new NamedSubentityRef(updateItem.RouteStepName);
            }

            return changes;
        }

        #endregion Page Lifecycle Methods
    }

    public partial class UpdateItem
    {
        public string RouteStepName { get; set; }
    }
}