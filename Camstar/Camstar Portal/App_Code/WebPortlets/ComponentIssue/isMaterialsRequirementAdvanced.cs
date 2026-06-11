using System;
using System.Linq;
using System.Web;
using System.Collections.Generic;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.Services;
using Camstar.WebPortal.PortalFramework;

namespace Camstar.WebPortal.WebPortlets.ComponentIssue
{
    /// <summary>
    /// Summary description for isMaterialsRequirementAdvanced
    /// The class was produced for industry Edition. 
    /// This module can help user to use Kit as a box of Material Details.
    /// </summary>
    public class isMaterialsRequirementAdvanced : MaterialsRequirementAdvanced
    {
        public isMaterialsRequirementAdvanced() { }

        protected enum QtyOperator
        {
            Consume = 0,
            Restore = 1,
        }

        #region Protected properties

        protected virtual JQDataGrid KitMaterialQueueDetailsGrid
        {
            get { return Page.FindCamstarControl("ContainerTxn_isMaterialQueueDetailsMap") as JQDataGrid; }
        }

        protected virtual CWC.NamedObject KitControl
        {
            get
            {
                return Page.FindCamstarControl("ContainerTxn_isKit") as CWC.NamedObject;
            }
        }

        protected virtual CWC.CheckBox isKitAvailable
        {
            get
            {
                return Page.FindCamstarControl("ComponentIssue_isKitAvailable") as CWC.CheckBox;
            }
        }

        protected virtual ToggleContainer ToggleKittingBlock
        {
            get
            {
                return Page.FindCamstarControl("ToggleKittingBlock") as ToggleContainer;
            }
        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            KitControl.DataChanged += KitControl_DataChanged;
        }

        protected List<T> GetSelectedItemsFromGrid<T>(JQDataGrid grid)
        {
            var selectedItems = new List<T>();

            string[] rowIndexes = grid.SelectedRowIDs as string[];
            var data = grid.Data as T[];
            if (rowIndexes != null && rowIndexes.Length > 0 && data != null)
            {
                int[] myInts = Array.ConvertAll(rowIndexes, int.Parse);
                Array.ForEach(myInts, i => selectedItems.Add(data[i]));
            }
            return selectedItems;
        }

        protected override ResponseData MaterialsRequirementSubGrid_RowDeleting(object sender, JQGridEventArgs args)
        {
            if (args.Context.SelectedRowID != null)
            {
                var rowItem = (OM.IssueActualDetail)args.Context.GetItem(args.Context.SelectedRowID);
                if (rowItem != null)
                {
                    var selectedBomLineItem = rowItem.BOMLineItem;
                    var selectedUniqueID = rowItem.UniqueID;
                    ExecuteDataList.RemoveAll(delegate (OM.IssueActualDetail removeItem)
                    {
                        var result = false;
                        if (removeItem.BOMLineItem.Equals(selectedBomLineItem) && removeItem.UniqueID.Equals(selectedUniqueID))
                        {
                            if (MaterialsRequirementGrid.Data != null)
                            {
                                foreach (OM.IssueDetails item in (MaterialsRequirementGrid.Data as Array))
                                {
                                    if (item.BOMLineItem.Equals(selectedBomLineItem))
                                    {
                                        if (removeItem.QtyIssued != null)
                                        {
                                            item.QtyIssued = (item.QtyIssued != null ? item.QtyIssued.Value : 0) - removeItem.QtyIssued.Value;
                                            item.NetQtyRequired = (item.NetQtyRequired != null ? item.NetQtyRequired.Value : 0) + removeItem.QtyIssued.Value;

                                            ChangeMaterialQueueDetailsGridQty(removeItem, QtyOperator.Restore);
                                        }
                                        break;
                                    }
                                }
                            }
                            result = true;
                        }

                        return result;
                    });
                }
            }
            return null;
        }

        private ValidationStatus ValidateAndAddQty(OM.IssueActualDetail issueActualDetail)
        {
            if (issueActualDetail == null) return null;

            var validationStatus = ValidateInputData(new OM.ComponentIssue { IssueActualDetails = new OM.IssueActualDetail[] { issueActualDetail } });
            if (validationStatus.IsSuccess)
            {
                ExecuteDataList.Add(issueActualDetail);

                var uniqueID = 0;
                ExecuteDataList.ForEach(item => item.UniqueID = uniqueID++);

                if (MaterialsRequirementGrid.Data != null)
                {
                    foreach (OM.IssueDetails item in (MaterialsRequirementGrid.Data as Array))
                    {
                        if (item.BOMLineItem.Equals(issueActualDetail.BOMLineItem))
                        {
                            if (issueActualDetail.QtyIssued != null)
                            {
                                item.QtyIssued = (item.QtyIssued != null ? item.QtyIssued.Value : 0) +
                                                 issueActualDetail.QtyIssued.Value;
                                item.NetQtyRequired = (item.NetQtyRequired != null ? item.NetQtyRequired.Value : 0) -
                                                      issueActualDetail.QtyIssued.Value;
                                ChangeMaterialQueueDetailsGridQty(issueActualDetail, QtyOperator.Consume);
                            }

                            break;
                        }
                    }
                }
            }

            return validationStatus;
        }

        private void ChangeMaterialQueueDetailsGridQty(OM.IssueActualDetail issueActualDetail, QtyOperator action)
        {
            if (issueActualDetail == null || issueActualDetail.isMaterialQueueDetails == null ||
                issueActualDetail.QtyIssued == null || !(KitMaterialQueueDetailsGrid.Data is OM.isMaterialQueueDetails[])) return;
            var mqdetail =
                ((OM.isMaterialQueueDetails[])KitMaterialQueueDetailsGrid.Data).FirstOrDefault(m => m.Self == issueActualDetail.isMaterialQueueDetails);
            if (mqdetail == null) return;

            var qtyAvailable = mqdetail.isQtyAvailable != null ? mqdetail.isQtyAvailable.Value : 0;
            var consumedQty = mqdetail.isConsumedQty != null ? mqdetail.isConsumedQty.Value : 0;
            switch (action)
            {
                case QtyOperator.Consume:
                    mqdetail.isQtyAvailable = qtyAvailable - issueActualDetail.QtyIssued.Value;
                    mqdetail.isConsumedQty = consumedQty + issueActualDetail.QtyIssued.Value;
                    break;
                case QtyOperator.Restore:
                    mqdetail.isQtyAvailable = qtyAvailable + issueActualDetail.QtyIssued.Value;
                    mqdetail.isConsumedQty = consumedQty - issueActualDetail.QtyIssued.Value;
                    break;
            }
        }

        protected virtual OM.IssueActualDetail ConvertMaterialQueueDetailToIssueActualDetail(OM.isMaterialQueueDetails mqDetails)
        {
            var scanObject = (mqDetails.isContainer != null) ? mqDetails.isContainer.Name :
                                    (mqDetails.isProduct != null) ? mqDetails.isProduct.Name : String.Empty;
            var scanResult = GetScanProduct(scanObject);

            if (scanResult.Key.IsSuccess)
            {
                var executeData = scanResult.Value.Value;
                if (executeData != null && executeData.IssueDetails != null)
                {
                    double qty2Issued;

                    if (executeData.IssueDetails is OM.IssueDetailsStock ||                             // we dont work with Stock and DisplayOnly Types..
                        executeData.IssueDetails is OM.IssueDetailsDisplayOnly ||
                        executeData.IssueDetails is OM.IssueDetailsLotStock &&
                            (mqDetails.isLot == null || string.IsNullOrEmpty(mqDetails.isLot.Value)) || // and Lot&Stock type without lot name
                        executeData.IssueDetails is OM.IssueDetailsBulk &&                              // and Container(Lot) type without container name
                            mqDetails.isContainer == null
                        )
                        return null;

                    return new OM.IssueActualDetail
                    {
                        FieldAction = OM.Action.Create,
                        BOMLineItem = executeData.IssueDetails.BOMLineItem,
                        Product = executeData.Product,
                        FromLot = mqDetails.isLot != null && !string.IsNullOrEmpty(mqDetails.isLot.Value) ? mqDetails.isLot.Value : null,
                        FromContainer = executeData.Container != null ?
                                new OM.ContainerRef
                                {
                                    Name = executeData.Container.Name
                                } : null,

                        IssueDifferenceReason = IssueDifferenceReason.Data != null ?
                                                    new OM.NamedObjectRef
                                                    {
                                                        Name = (IssueDifferenceReason.Data as OM.NamedObjectRef).Name
                                                    } : null,
                        QtyIssued = GetNetQtyRequired(executeData, mqDetails.isQtyAvailable),
                        Qty2Issued = IssueQty2.Data != null && executeData.IssueDetails is OM.IssueDetailsSerial
                            ? ExecuteData.Qty2
                                : (IssueQty2.Data != null && double.TryParse(IssueQty2.Data.ToString(), out qty2Issued) ? (OM.Primitive<double>)qty2Issued : null),

                        FromStockPoint = StockPoint.Data != null ? StockPoint.Data.ToString() : null,
                        IssueReason = IssueReason.Data != null && !(IssueReason.Data as OM.NamedObjectRef).IsEmpty ?
                                                    new OM.NamedObjectRef
                                                    {
                                                        Name = (IssueReason.Data as OM.NamedObjectRef).Name
                                                    } : null,
                        SubstitutionReason = SubstitutionReason.Data != null && !(SubstitutionReason.Data as OM.NamedObjectRef).IsEmpty ?
                                                    new OM.NamedObjectRef
                                                    {
                                                        Name = (SubstitutionReason.Data as OM.NamedObjectRef).Name
                                                    } : null,
                        Comments = Comments.Data != null ? Comments.Data.ToString() : null,
                        isMaterialQueueDetails = mqDetails.Self as OM.SubentityRef
                    };
                }
            }
            return null;
        }

        private OM.Primitive<double> GetNetQtyRequired(OM.ComponentIssueInquiry executeData, OM.Primitive<double> allAvailableQty)
        {
            OM.Primitive<double> qty = 0;
            var issueDetail = MaterialRequirements.FirstOrDefault(mq => mq.Product != null
                                                                        && mq.Product.Name == executeData.Product.Name
                                                                        && mq.IssueControl != OM.IssueControlEnum.CommentOnly);
            if (issueDetail != null)
            {
                qty = executeData.IssueDetails is OM.IssueDetailsSerial
                    ? allAvailableQty
                    : (issueDetail.NetQtyRequired != null && issueDetail.NetQtyRequired.Value <= allAvailableQty.Value)
                        ? issueDetail.NetQtyRequired
                        : allAvailableQty;
            }

            return qty;
        }

        protected virtual KeyValuePair<OM.ResultStatus, ComponentIssueInquiry_Result> GetScanProduct(string objectName)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new ComponentIssueInquiryService(session.CurrentUserProfile);
            var isRowSelected = SelectionGridData != null;

            var serviceData = new OM.ComponentIssueInquiry
            {
                BOMLineItem = isRowSelected ? SelectionGridData.BOMLineItem : null,
                ParentContainer = (OM.ContainerRef)ContainerName.Data,
                ObjectName = objectName
            };

            var serviceInfo = new OM.ComponentIssueInquiry_Info
            {
                IssueDetails = new OM.IssueDetails_Info
                {
                    RequestValue = true
                },
                Container = new OM.Info(true),
                Product = new OM.Info(true),
                Qty = new OM.Info(true),
                UOM = new OM.Info(true),
                Qty2 = new OM.Info(true),
                UOM2 = new OM.Info(true)
            };

            var request = new ComponentIssueInquiry_Request();
            request.Info = serviceInfo;

            var result = new ComponentIssueInquiry_Result();
            var resultStatus = new OM.ResultStatus();

            resultStatus = service.ExecuteTransaction(serviceData, request, out result);

            return new KeyValuePair<OM.ResultStatus, ComponentIssueInquiry_Result>(resultStatus, result);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (KitMaterialQueueDetailsGrid.SelectedRowCount > 0)
            {
                AddToPendingBtn.Enabled = true;
            }
            //ScanContainer.Visible = !isKitAvailable.IsChecked;
            KitControl.Visible = isKitAvailable.IsChecked;
            ToggleKittingBlock.Visible = isKitAvailable.IsChecked;
        }

        public override void AddToPending(object sender, EventArgs e)
        {
            base.AddToPending(sender, e);

            var isMqDetailsArray = GetSelectedItemsFromGrid<OM.isMaterialQueueDetails>(KitMaterialQueueDetailsGrid);
            if (isMqDetailsArray == null || isMqDetailsArray.Count == 0) return;
            List<ValidationStatus> errors = new List<ValidationStatus>();
            foreach (var mqDetail in isMqDetailsArray)
            {
                if (mqDetail.isQty == 0) continue;
                var error = ValidateAndAddQty(ConvertMaterialQueueDetailToIssueActualDetail(mqDetail));
                if (error != null && !error.IsSuccess)
                    errors.Add(error);
            }
            if (errors.Count > 0)
                Page.DisplayMessage(errors.FirstOrDefault());

            KitMaterialQueueDetailsGrid.GridContext.SelectedRowIDs = null;
        }

        protected override void ClearGridData()
        {
            base.ClearGridData();
            ClearMaterialQueueDetailsGrid();
            ClearAndPrepareKitControl();
        }

        protected virtual void ClearMaterialQueueDetailsGrid()
        {
            KitMaterialQueueDetailsGrid.ClearData();
            KitMaterialQueueDetailsGrid.OriginalData = null;
            KitMaterialQueueDetailsGrid.GridContext.CurrentPage = 1;
        }

        public override void ContainerNameControl_DataChanged(object sender, EventArgs e)
        {
            base.ContainerNameControl_DataChanged(sender, e);
            ClearMaterialQueueDetailsGrid();
            ClearAndPrepareKitControl();
        }

        protected void ClearAndPrepareKitControl()
        {
            KitControl.Data = null;
        }

        private void KitControl_DataChanged(object sender, EventArgs e)
        {
            if (KitControl.Data == null) KitMaterialQueueDetailsGrid.ClearData();
            if (KitMaterialQueueDetailsGrid.TotalRowCount == 0) return;
            for (int i = 0; i < KitMaterialQueueDetailsGrid.TotalRowCount; i++)
            {
                string id = i.ToString().PadLeft(6, '0');
                KitMaterialQueueDetailsGrid.Action_SelectRow(id, "select");
            }
        }
    }
}