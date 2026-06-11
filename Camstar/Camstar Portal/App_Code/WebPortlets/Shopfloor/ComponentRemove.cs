// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.PortalFramework;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{

    public class ComponentRemove : MatrixWebPart
    {
        #region ViewState
        protected virtual string LastContainerName 
        { 
            get { return ViewState["LastContainerName"] as string; } 
            set { ViewState["LastContainerName"] = value; } 
        }

        #endregion ViewState
        #region Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            JQDataGrid containerGrid = ContainerGrid;
            RemoveAllQty.DataChanged += new EventHandler(RemoveAllQty_DataChanged);
            CurrentContainer.DataChanged += new EventHandler(CurrentContainer_DataChanged);
            ContainerGrid.RowSelected += delegate { UpdateControlStatuses(); return null; };
            Page.OnClearValues += new EventHandler<FormsFramework.ServiceDataEventArgs>(Page_OnClearValues);            
        }

        protected virtual void Page_OnClearValues(object sender, FormsFramework.ServiceDataEventArgs e)
        {
            DestinationContainer.Data = null;
            DestinationLocation.Data = null;
            RemoveAllQty.ClearData();
            Page.DataContract.SetValueByName("SelectedContainerNameDM", null);
        }

        protected virtual void CurrentContainer_DataChanged(object sender, EventArgs e)
        {
            ContainerRef container = (ContainerRef)CurrentContainer.Data;

            var dcContainer = (Page.FindCamstarControl("DCContainer") as CWC.ContainerList);
            dcContainer.Data = null;

            if (container == null)
                Page.ClearValues();
            else
                ReloadMaterialIssueGrid();

            dcContainer.Data = container;
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null && action.Parameters == "ExecuteTransaction")
            {
                ResultStatus result = ExecuteTransaction();
                e.Result = result;
                if (result.IsSuccess)
                {
                    Page.ClearValues();
                    ContainerRef container = (ContainerRef)CurrentContainer.Data;
                    if (container != null)
                        ReloadMaterialIssueGrid();
                }
            }
        }

        protected virtual void RemoveAllQty_DataChanged(object sender, EventArgs e)
        {
            RemovalCandidate item = SelectedRowItem;

            bool removeAll = (bool)RemoveAllQty.Data;

            CWC.TextBox field = QtyRemoved;
            field.Data = removeAll && item != null ? item.NetQtyIssued : null;
            field.Enabled = !removeAll;

            field = Qty2Removed;
            field.Data = removeAll && item != null ? item.NetQty2Issued : null;
            field.Enabled = !removeAll;
        }


        /// <summary>
        /// Set initial control states when material issued is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual void UpdateControlStatuses()
        {
            DestinationContainer.Enabled = false;
            DestinationLocation.Enabled = false;

            DestinationContainer.ClearData();
            DestinationLocation.ClearData();

            RemovalCandidate selItem = SelectedRowItem;

            if (selItem != null)
            {
                // For Issue Control of Issue Container (Serial), system shall automatically select the ‘Remove All Qty’ 
                bool removeAllQtyChecked = selItem is RemovalCandidateSerial;
                RemoveAllQty.Data = removeAllQtyChecked;
                RemoveAllQty.Enabled = !removeAllQtyChecked;

                DestinationContainer.Enabled = selItem is RemovalCandidateSerial || selItem is RemovalCandidateBulk || selItem is RemovalCandidateLotStock || (bool)selItem.IsHVIssue;
                DestinationLocation.Enabled = selItem is RemovalCandidateSerial || selItem is RemovalCandidateBulk || selItem is RemovalCandidateLotStock || selItem is RemovalCandidateStock || (bool)selItem.IsHVIssue;
            }
        }

        #region Service methods


        public override FormsFramework.ValidationStatus ValidateInputData(Service serviceData)
        {
            ValidationStatus status = base.ValidateInputData(serviceData);
            if (SelectedRowItem == null)
            {
                string validationMessage = FrameworkManagerUtil.GetLabelCache().GetLabelByName("Lbl_NoRowSelected").Value.ToString();
                ValidationStatusItem statusItem = new FormsFramework.ValidationStatusItem(null, null, validationMessage);
                status.Add(statusItem);
            }

            return status;
        }

        /// <summary>
        /// Perform submit transaction.
        /// </summary>
        /// <returns></returns>
        protected virtual ResultStatus ExecuteTransaction()
        {
            //Perform value validation
            OM.ComponentRemove input = new OM.ComponentRemove();
            Page.GetInputData(input);

            OM.ComponentRemove inputForExecute = input.Clone() as OM.ComponentRemove;

            ResultStatus resultStatus = Service.Form.ValidateInputData(input);
            if (!resultStatus.IsSuccess)
                return resultStatus;

            WCF.Services.ComponentRemoveService service = Page.Service.GetService<WCF.Services.ComponentRemoveService>();
            service.BeginTransaction();
            service.GetRemovalCandidates(input);

            RemovalCandidate removalCandidate = GetUpdatedRemovalCandidate();
            if (removalCandidate != null)
            {
                inputForExecute.ServiceDetails = new OM.RemovalCandidate[] { removalCandidate };
                if ((bool)removalCandidate.IsHVIssue)
                {
                    resultStatus = ExecuteHVComponentRemove(inputForExecute);
                    service.RollBackTransaction();
                    return resultStatus;
                }
            }

            var tuple = ESigCaptureUtil.CollectESigServiceDetailsAll();
            if (tuple != null)
                inputForExecute.ESigDetails = tuple.Item1;
            Page.GetLineAssignment(inputForExecute);


            if (EProcTaskContainer.Data != null)
            {
                inputForExecute.TaskContainer = (OM.ContainerRef)EProcTaskContainer.Data;
                inputForExecute.CalledByTransactionTask = (OM.NamedSubentityRef)EProcTask.Data;
                inputForExecute.CalledByTransactionTask.Parent = (OM.BaseObjectRef)EProcTaskList.Data;
            }

            service.ExecuteTransaction(inputForExecute);
            resultStatus = service.CommitTransaction();

            ESigCaptureUtil.CleanESigCaptureDM();
            return resultStatus;
        }

        protected virtual ResultStatus ExecuteHVComponentRemove(OM.ComponentRemove inputServiceData)
        {
            //OM.ComponentRemove inputForExecute = inputServiceData.Clone() as OM.ComponentRemove;
            WCF.Services.HVComponentRemoveService service = Page.Service.GetService<WCF.Services.HVComponentRemoveService>();
            service.BeginTransaction();

            OM.HVComponentRemove hvServiceData = new OM.HVComponentRemove();
            hvServiceData.Container = inputServiceData.Container;
            hvServiceData.ServiceDetails = new OM.RemovalCandidateHV[] { (OM.RemovalCandidateHV)inputServiceData.ServiceDetails[0] };
            //TODO: any other fields needed?

            var tuple = ESigCaptureUtil.CollectESigServiceDetailsAll();
            if (tuple != null)
                hvServiceData.ESigDetails = tuple.Item1;
            Page.GetLineAssignment(hvServiceData);


            if (EProcTaskContainer.Data != null)
            {
                hvServiceData.TaskContainer = (OM.ContainerRef)EProcTaskContainer.Data;
                hvServiceData.CalledByTransactionTask = (OM.NamedSubentityRef)EProcTask.Data;
                hvServiceData.CalledByTransactionTask.Parent = (OM.BaseObjectRef)EProcTaskList.Data;
            }

            service.ExecuteTransaction(hvServiceData);
            ResultStatus resultStatus = service.CommitTransaction();

            ESigCaptureUtil.CleanESigCaptureDM();
            return resultStatus;
        }

        protected virtual void ReloadMaterialIssueGrid()
        {
            ContainerRef container = (ContainerRef)CurrentContainer.Data;
            LastContainerName = container != null ? container.Name : "";
            if (container != null)
                Page.Service.ExecuteFunction("ComponentRemove", "GetRemovalCandidates");
        }

        #endregion

        #region Save Row Values methods

        protected virtual RemovalCandidate GetHVRemovalCandidate(RemovalCandidateHV removalCandidate, RemovalCandidateHV selectedItem)
        {
            removalCandidate.ListItemAction = OM.ListItemAction.Add;
            //removalCandidate.ListItemIndex = 0;

            removalCandidate.HVComponentIssueHistory = selectedItem.HVComponentIssueHistory;
            removalCandidate.HVIssueHistoryDetail = selectedItem.HVIssueHistoryDetail;
            removalCandidate.HVSetup = selectedItem.HVSetup;
            removalCandidate.HVSetupDetail = selectedItem.HVSetupDetail;
            removalCandidate.IsHVIssue = selectedItem.IsHVIssue;
            removalCandidate.IssueControl = selectedItem.IssueControl;
            removalCandidate.IssuedFrom = selectedItem.IssuedFrom;
            removalCandidate.IssuedToContainer = selectedItem.IssuedToContainer;
            removalCandidate.MaterialListItemId = selectedItem.MaterialListItemId;
            removalCandidate.NetQtyIssued = selectedItem.NetQtyIssued;
            removalCandidate.Product = selectedItem.Product;
            removalCandidate.ProductDescription = selectedItem.ProductDescription;
            removalCandidate.QtyIssued = selectedItem.QtyIssued;
            removalCandidate.ReferenceDesignator = selectedItem.ReferenceDesignator;

            RemovalDetailHV detail = removalCandidate.RemovalDetail = new RemovalDetailHV();

            detail.DestinationLot = (string)DestinationContainer.Data;
            detail.DestinationStockPoint = (string)DestinationLocation.Data;

            detail.FieldAction = OM.Action.Create;
            detail.RemovalReason = (NamedObjectRef)RemovalReason.Data;
            detail.RemoveDifferenceReason = (NamedObjectRef)RemoveDifferenceReason.Data;

            bool removeAllQty = (bool)RemoveAllQty.Data;
            detail.RemoveAllQty = removeAllQty;

            object qtyRemoved = QtyRemoved.Data;
            // not following standard logic "if (qtyRemoved != null && !removeAllQty)" which seems wrong.
            // current page behavior sets QtyRemoved control if user ticks the RemoveAll check box.
            // so we should set QtyRemoved as long as control has value.
            if (qtyRemoved != null)
                detail.EnteredQtyRemoved = qtyRemoved.ToString();

            detail.Container = selectedItem.IssuedToContainer;
            detail.HVSetup = selectedItem.HVSetup;
            detail.HVSetupDetail = selectedItem.HVSetupDetail;
            detail.MaterialListItemId = selectedItem.MaterialListItemId;
            detail.ReferenceDesignator = selectedItem.ReferenceDesignator;
            detail.HVIssueHistoryDetail = selectedItem.HVIssueHistoryDetail;

            return removalCandidate;
        }

        protected virtual RemovalCandidate GetStandardRemovalCandidate(RemovalCandidate removalCandidate, RemovalCandidate selectedItem)
        {
            removalCandidate.ListItemAction = OM.ListItemAction.Change;
            removalCandidate.ListItemIndex = selectedItem.ListItemIndex;

            Type itemType = removalCandidate.GetType();
            if (itemType == typeof(RemovalCandidateBulk))
                SaveBulkDetail((RemovalCandidateBulk)removalCandidate);
            else if (itemType == typeof(RemovalCandidateSerial))
                SaveSerialDetail((RemovalCandidateSerial)removalCandidate);
            else if (itemType == typeof(RemovalCandidateLotStock))
                SaveLotStockDetail((RemovalCandidateLotStock)removalCandidate);
            else if (itemType == typeof(RemovalCandidateStock))
                SaveStockDetail((RemovalCandidateStock)removalCandidate);
            else if (itemType == typeof(RemovalCandidateQuantity))
                removalCandidate.RemovalDetail = new RemovalDetailQuantity();

            removalCandidate.RemovalDetail.FieldAction = OM.Action.Create;
            removalCandidate.RemovalDetail.RemovalReason = (NamedObjectRef)RemovalReason.Data;
            removalCandidate.RemovalDetail.RemoveDifferenceReason = (NamedObjectRef)RemoveDifferenceReason.Data;
            bool removeAllQty = (bool)RemoveAllQty.Data;
            removalCandidate.RemovalDetail.RemoveAllQty = removeAllQty;
            object qtyRemoved = QtyRemoved.Data;
            if (qtyRemoved != null && !removeAllQty)
                removalCandidate.RemovalDetail.EnteredQtyRemoved = qtyRemoved?.ToString();
            object qty2Removed = Qty2Removed.Data;
            if (qty2Removed != null && !removeAllQty)
                removalCandidate.RemovalDetail.EnteredQty2Removed = qty2Removed?.ToString();

            return removalCandidate;
        }

        protected virtual RemovalCandidate GetUpdatedRemovalCandidate()
        {
            RemovalCandidate selectedItem = SelectedRowItem;
            if (selectedItem == null) 
                return null;

            RemovalCandidate copyItem = (RemovalCandidate)selectedItem.GetType().GetConstructor(Type.EmptyTypes).Invoke(null);

            if ((bool)selectedItem.IsHVIssue)
                return GetHVRemovalCandidate((RemovalCandidateHV)copyItem, (RemovalCandidateHV)selectedItem);
            else
                return GetStandardRemovalCandidate(copyItem, selectedItem);
        }

        protected virtual void SaveBulkDetail(RemovalCandidateBulk item)
        {
            RemovalDetailBulk detail = item.RemovalDetail = new RemovalDetailBulk();
            bool openClosedContainer = (bool)OpenClosedContainer.Data;
            ContainerRef destinationContainer = WSObjectRef.AssignContainer((string)DestinationContainer.Data);
            object factory = new NamedObjectRef(Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Factory) as string);
            NamedSubentityRef destinationLocation = WSObjectRef.AssignNamedSubentity((string)DestinationLocation.Data, factory);
            detail.OpenClosedContainer = openClosedContainer;
            detail.DestinationContainer = destinationContainer;
            detail.DestinationLocation = destinationLocation;
        }

        protected virtual void SaveSerialDetail(RemovalCandidateSerial item)
        {
            RemovalDetailSerial detail = item.RemovalDetail = new RemovalDetailSerial();
            bool openClosedContainer = (bool)OpenClosedContainer.Data;
            ContainerRef destinationContainer = WSObjectRef.AssignContainer((string)DestinationContainer.Data);
            object factory = new NamedObjectRef(Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Factory) as string);
            NamedSubentityRef destinationLocation = WSObjectRef.AssignNamedSubentity((string)DestinationLocation.Data, factory);
            detail.OpenClosedContainer = openClosedContainer;
            detail.DestinationContainer = destinationContainer;
            detail.DestinationLocation = destinationLocation;
        }

        protected virtual void SaveLotStockDetail(RemovalCandidateLotStock item)
        {
            RemovalDetailLotStock detail = item.RemovalDetail = new RemovalDetailLotStock();
            detail.DestinationLot = (string)DestinationContainer.Data;
            detail.DestinationStockPoint = (string)DestinationLocation.Data;
        }

        protected virtual void SaveStockDetail(RemovalCandidateStock item)
        {
            RemovalDetailStock detail = item.RemovalDetail = new RemovalDetailStock();
            detail.DestinationStockPoint = (string)DestinationLocation.Data;
        }

        #endregion

        #region Scan Product/Container methods

        public virtual void ScanNewContainerOrProduct(object sender, EventArgs e)
        {
            string scanValue = (string)ScanField.Data;
            if (string.IsNullOrWhiteSpace(scanValue))
                return;
            JQDataGrid containerGrid = ContainerGrid;
            RemovalCandidate[] items = (RemovalCandidate[])containerGrid.Data;
            bool itemFound = false;
            if (items != null)
                for (int i = 0; i < items.Length; i++)
                {
                    RemovalCandidate currItem = items[i];
                    bool containerEqual = currItem.IssuedFrom != null
                        && string.Compare(currItem.IssuedFrom.Value, scanValue, true) == 0;
                    bool productEqual = string.Compare(currItem.Product.Name, scanValue, true) == 0;
                    if (containerEqual || productEqual)
                    {
                        itemFound = true;
                        SelectNewContainer(i);
                        break;
                    }
                }
            if (!itemFound)
                WriteError(ContainerNotFoundKey);
            else
                ScanField.Data = null;
        }

        protected virtual void SelectNewContainer(int index)
        {
            JQDataGrid containerGrid = ContainerGrid;
            string autoRowID = (containerGrid.GridContext as BoundContext).MakeAutoRowId(index);
            containerGrid.SelectedRowID = autoRowID;
            containerGrid.GridContext.AdjustCurrentPage(autoRowID);
        }

        protected virtual void WriteError(string labelName)
        {
            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
            Label label = labelCache.GetLabelByName(labelName);
            if (label.IsEmpty)
                Page.StatusBar.WriteError(label.DefaultValue);
            else
                Page.StatusBar.WriteError(label.Value);
        }

        #endregion

        #endregion

        #region Properties

        #region Controls

        protected virtual ContainerListGrid CurrentContainer
        {
            get
            {
                return Page.FindCamstarControl("HiddenSelectedContainer") as ContainerListGrid;
            }
        }

        protected virtual JQDataGrid ContainerGrid
        {
            get
            {
                return Page.FindCamstarControl("MaterialIssueGrid") as JQDataGrid;
            }
        }

        protected virtual CWC.TextBox ScanField
        {
            get
            {
                return Page.FindCamstarControl("ContainerScan") as CWC.TextBox;
            }
        }

        protected virtual CWC.NamedObject RemovalReason
        {
            get
            {
                return Page.FindCamstarControl("RemovalReason") as CWC.NamedObject;
            }
        }

        protected virtual CWC.NamedObject RemoveDifferenceReason
        {
            get
            {
                return Page.FindCamstarControl("RemoveDifferenceReason") as CWC.NamedObject;
            }
        }

        protected virtual CWC.TextBox QtyRemoved
        {
            get
            {
                return Page.FindCamstarControl("QtyRemoved") as CWC.TextBox;
            }
        }

        protected virtual CWC.TextBox Qty2Removed
        {
            get
            {
                return Page.FindCamstarControl("Qty2Removed") as CWC.TextBox;
            }
        }

        protected virtual CWC.CheckBox RemoveAllQty
        {
            get
            {
                return Page.FindCamstarControl("RemoveAllQty") as CWC.CheckBox;
            }
        }

        protected virtual CWC.TextBox DestinationContainer
        {
            get
            {
                return Page.FindCamstarControl("DestinationContainer") as CWC.TextBox;
            }
        }

        protected virtual CWC.TextBox DestinationLocation
        {
            get
            {
                return Page.FindCamstarControl("DestinationLocationField") as CWC.TextBox;
            }
        }

        protected virtual CWC.CheckBox OpenClosedContainer
        {
            get
            {
                return Page.FindCamstarControl("OpenClosed") as CWC.CheckBox;
            }
        }

        #endregion

        protected virtual RemovalCandidate SelectedRowItem
        {
            get
            {
                JQDataGrid grid = ContainerGrid;
                string selecteRowID = grid.SelectedRowID;
                if (selecteRowID == null) return null;
                RemovalCandidate selectedItem = (RemovalCandidate)grid.GridContext.GetItem(selecteRowID);
                return selectedItem;
            }
        }
        protected virtual ContainerListGrid EProcTaskContainer
        {
            get { return Page.FindCamstarControl("ShopFloor_TaskContainer") as ContainerListGrid; }
        }
        protected virtual CWC.RevisionedObject EProcTaskList
        {
            get { return Page.FindCamstarControl("ExecuteTask_TaskList") as CWC.RevisionedObject; }
        } // EProcTaskList

        protected virtual CWC.NamedSubentity EProcTask
        {
            get { return Page.FindCamstarControl("ShopFloor_CalledByTransactionTask") as CWC.NamedSubentity; }
        } // EProcTask
        #endregion

        private const string ContainerNotFoundKey = "Err_ComponentRemove_NonexistentContainer";
    }
}
