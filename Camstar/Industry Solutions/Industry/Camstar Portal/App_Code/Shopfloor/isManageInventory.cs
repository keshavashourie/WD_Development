// Copyright Siemens 2020
using System.Data;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Web;
using System.Linq;
using OM = Camstar.WCF.ObjectStack;
using System;
using System.Collections.Generic;
using System.Data;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.Utilities;
using DateTime = System.DateTime;
using PERS = Camstar.WebPortal.Personalization;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class isManageInventory : MatrixWebPart
    {
        #region Properties
        protected virtual CWC.NamedObject InventoryLocation
        {
            get
            {
                return Page.FindCamstarControl("isManageInventory_isInventoryLocation") as CWC.NamedObject;
            }
        }
        protected virtual JQDataGrid _gridIsInventoryDetails
        {
            get { return Page.FindCamstarControl("isManageInventory_isInventoryDetails") as JQDataGrid; }
        }

        protected virtual ContainerListGrid ContainerName
        {
            get { return Page.FindCamstarControl("isManageInventory_isContainer") as ContainerListGrid; }
        } // ContainerName

        protected virtual CWC.TextBox QtyField
        {
            get
            {
                return Page.FindCamstarControl("isManageInventory_isQty") as CWC.TextBox;
            }
        }

        protected virtual CWC.NamedObject ToLocation
        {
            get
            {
                return Page.FindCamstarControl("isTransferInventory_isToInventoryLocation") as CWC.NamedObject;
            }
        }

        protected virtual CWC.TextBox LotField
        {
            get
            {
                return Page.FindCamstarControl("isManageInventory_isLot") as CWC.TextBox;
            }
        }

        protected virtual CWC.TextBox Containerorlotname
        {
            get
            {
                return Page.FindCamstarControl("isManageInventory_isName") as CWC.TextBox;
            }
        }


        protected virtual CWC.RevisionedObject product
        {
            get
            {
                return Page.FindCamstarControl("isManageInventory_isProduct") as CWC.RevisionedObject;
            }
        }
        protected virtual CWC.NamedObject UOM
        {
            get
            {
                return Page.FindCamstarControl("isManageInventory_isUOM") as CWC.NamedObject;
            }
        }

        protected virtual CWC.NamedObject Level
        {
            get
            {
                return Page.FindCamstarControl("isManageInventory_isContainerLevel") as CWC.NamedObject;
            }
        }

        protected virtual PERS.UIAction TransferAction
        {
            get
            {
                return Page.ActionDispatcher.GetActionByName("Transfer");
            }
        }

        protected virtual PERS.UIAction RemoveAction
        {
            get
            {
                return Page.ActionDispatcher.GetActionByName("Remove");
            }
        }


        protected virtual PERS.UIAction SubmitAction
        {
            get
            {
                return Page.ActionDispatcher.GetActionByName("Submit");
            }
        }

        protected virtual PERS.UIAction ResetAction
        {
            get
            {
                return Page.ActionDispatcher.GetActionByName("Reset");
            }
        }
        protected virtual CWC.DropDownList RemovalStrategy
        {
            get
            {
                return Page.FindCamstarControl("isManageInventory_isRemovalStrategy") as CWC.DropDownList;
            }
        }

        protected virtual CWC.DateChooser ExpirationDate
        {
            get
            {
                return Page.FindCamstarControl("isManageInventory_isExpirationDate") as CWC.DateChooser;
            }
        }
        #endregion

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            InventoryLocation.DataChanged += InventoryLocation_DataChanged;
            ContainerName.DataChanged += ContainerName_DataChanged;
            LotField.DataChanged += LotField_DataChanged;
            product.DataChanged += product_DataChanged;
            RemovalStrategy.DataChanged += RemovalStrategy_DataChanged;


            if (!Page.IsPostBack)
                FetchData();

            //Set Containerorlotname
            if (ContainerName.Data != null)
            {
                Containerorlotname.Data = ContainerName.Data;

            }
            else if (LotField.Data != null)
            {
                Containerorlotname.Data = LotField.Data;
            }
            else
            {
                Containerorlotname.Data = null;
            }
            ContainerName.Enabled = true;
            LotField.Enabled = true;
            ExpirationDate.Enabled = RemovalStrategy.SelectionData != null && (OM.isRemovalStrategyEnum)RemovalStrategy.SelectionData == OM.isRemovalStrategyEnum.FEFO;

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var oservice = new Camstar.WCF.Services.isTransferInventoryService(session.CurrentUserProfile);
            var oservicedata = new OM.isTransferInventory();
            var oserviceinfo = new OM.isTransferInventory_Info();
            var oresult = new Camstar.WCF.Services.isTransferInventory_Result();

            oservicedata.isInventoryLocation = InventoryLocation.Data as OM.NamedObjectRef;

            oserviceinfo.isToInventoryLocation = FieldInfoUtil.RequestSelectionValue();
            var ServiceRequest = new Camstar.WCF.Services.isTransferInventory_Request();
            ServiceRequest.Info = oserviceinfo;

            var oresultStatus = new OM.ResultStatus();

            OM.ResultStatus ResultStatus = oservice.GetEnvironment(oservicedata, ServiceRequest, out oresult);

            if (ResultStatus.IsSuccess)
            {
                if (oresult.Environment.isToInventoryLocation != null)
                {
                    ToLocation.SetSelectionValues(oresult.Environment.isToInventoryLocation.SelectionValues);
                    CamstarWebControl.SetRenderToClient(ToLocation);
                }
            }

            if (InventoryLocation.Data == null)
            {
                SubmitAction.IsHidden = true;
                TransferAction.IsHidden = true;
                RemoveAction.IsHidden = true;
                ResetAction.IsHidden = true;

                ContainerName.Enabled = false;
                LotField.Enabled = false;
                product.Enabled = false;
                RemovalStrategy.Enabled = false;
                QtyField.Enabled = false;
                UOM.Enabled = false;
                ExpirationDate.Enabled = false;
            }
            else
            {
                SubmitAction.IsHidden = false;
                TransferAction.IsHidden = false;
                RemoveAction.IsHidden = false;
                ResetAction.IsHidden = false;

                if (!Page.IsPostBack && ContainerName.Data == null && LotField.Data == null && product.Data == null && QtyField.Data == null)
                {
                    ContainerName.Enabled = false;
                    LotField.Enabled = false;
                    product.Enabled = false;
                    SubmitAction.IsDisabled = true;
                }
                if (_gridIsInventoryDetails.GridContext.SelectedRowIDs != null && _gridIsInventoryDetails.GridContext.SelectedRowIDs.Count() > 0)
                {
                    ContainerName.Enabled = false;
                    ContainerName.ClearData();
                    LotField.Enabled = false;
                    LotField.ClearData();
                    QtyField.ClearData();
                    product.ClearData();
                    UOM.ClearData();
                    TransferAction.IsDisabled = false;
                    ToLocation.Visible = true;
                    RemoveAction.IsDisabled = false;
                }
                else
                {
                    TransferAction.IsDisabled = true;
                    ToLocation.Visible = false;
                    RemoveAction.IsDisabled = true;
                }
            }
        }

        void product_DataChanged(object sender, EventArgs e)
        {
            if (product.Data != null)
            {
                SubmitAction.IsDisabled = false;
            }
            else
            {
                SubmitAction.IsDisabled = true;
                RemovalStrategy.Enabled = true;
            }


            if (LotField.Data != null)
                ContainerName.Enabled = false;

            if (LotField.Data == null && ContainerName.Data == null && product.Data != null)
            {
                RemovalStrategy.ClearData();
                RemovalStrategy.Enabled = false;
            }
        }



        void LotField_DataChanged(object sender, EventArgs e)
        {
            if (LotField.Data == null)
            {
                QtyField.ClearData();
                product.ClearData();
                UOM.ClearData();
                RemovalStrategy.ClearData();
                ExpirationDate.ClearData();
                ContainerName.Enabled = true;
                LotField.Enabled = true;
                SubmitAction.IsDisabled = true;
                RemovalStrategy.ClearData();
            }
            else
            {
                ContainerName.Enabled = false;
                LotField.Enabled = true;
                SubmitAction.IsDisabled = false;
                RemovalStrategy.Enabled = true;
                RemovalStrategy.Data = (int)OM.isRemovalStrategyEnum.FIFO;
            }
        }

        void ContainerName_DataChanged(object sender, EventArgs e)
        {
            if (ContainerName.Data == null)
            {
                QtyField.ClearData();
                product.ClearData();
                UOM.ClearData();
                LotField.Enabled = true;
                Containerorlotname.Data = null;
                SubmitAction.IsDisabled = true;
                QtyField.Enabled = true;
                product.Enabled = true;
                UOM.Enabled = true;
            }
            else
            {
                ContainerName.LoadOrClearDependentValues();
                if (ExpirationDate.Data != null)
                {
                    RemovalStrategy.Data = (int)OM.isRemovalStrategyEnum.FEFO;
                    //RemovalStrategy.Enabled = false;
                }
                else
                {
                    RemovalStrategy.Data = (int)OM.isRemovalStrategyEnum.FIFO;
                    //RemovalStrategy.Enabled = false;
                }

                LotField.Enabled = false;
                SubmitAction.IsDisabled = false;
                Containerorlotname.Data = ContainerName.Data;
                QtyField.Enabled = false;
                product.Enabled = false;
                UOM.Enabled = false;
                RemovalStrategy.Enabled = true;
            }

            if (_gridIsInventoryDetails.GridContext.SelectedRowIDs != null)
                if (_gridIsInventoryDetails.GridContext.SelectedRowIDs.Count() > 0)
                {

                    ToLocation.Visible = true;
                    TransferAction.IsDisabled = false;
                    RemoveAction.IsDisabled = false;
                    SubmitAction.IsDisabled = true;
                }
                else
                {
                    ToLocation.Visible = false;
                    TransferAction.IsDisabled = true;
                    RemoveAction.IsDisabled = true;
                    SubmitAction.IsDisabled = false;
                }

        }
        void InventoryLocation_DataChanged(object sender, System.EventArgs e)
        {
            _gridIsInventoryDetails.ClearData();
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.isManageInventoryService(session.CurrentUserProfile);
            var servicedata = new OM.isManageInventory();
            var serviceinfo = new OM.isManageInventory_Info();
            var result = new Camstar.WCF.Services.isManageInventory_Result();

            servicedata.isInventoryLocation = InventoryLocation.Data as OM.NamedObjectRef;

            serviceinfo.isInventoryDetails = new OM.isInventoryDetails_Info()
            {
                isContainer = new OM.Info(true),
                isLot = new OM.Info(true),
                isName = new OM.Info(true),
                isProduct = new OM.Info(true),
                isQty = new OM.Info(true),
                isUOM = new OM.Info(true),
                isRemovalStrategy = new OM.Info(true),
                isExpirationDate = new OM.Info(true)
            };

            var oServiceRequest = new Camstar.WCF.Services.isManageInventory_Request();
            oServiceRequest.Info = serviceinfo;

            var resultStatus = new OM.ResultStatus();

            OM.ResultStatus oResultStatus = service.GetEnvironment(servicedata, oServiceRequest, out result);

            if (oResultStatus.IsSuccess)
            {
                if (result.Value.isInventoryDetails != null)
                {

                    Array oInventoryLocationArray = result.Value.isInventoryDetails.ToArray();


                    (_gridIsInventoryDetails.GridContext as BoundContext).Data = oInventoryLocationArray;

                    _gridIsInventoryDetails.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(_gridIsInventoryDetails);


                }
                else
                {
                    _gridIsInventoryDetails.ClearData();
                }
                ContainerName.Enabled = true;
                LotField.Enabled = true;
                ContainerName.ClearData();
                QtyField.Enabled = true;
                product.Enabled = true;
                UOM.Enabled = true;
                LotField.ClearData();
                QtyField.ClearData();
                UOM.ClearData();
                product.ClearData();
                ExpirationDate.ClearData();
                TransferAction.IsDisabled = true;
                RemoveAction.IsDisabled = true;
                SubmitAction.IsDisabled = true;
                ToLocation.Visible = false;
                ToLocation.ClearData();
            }

            var oservice = new Camstar.WCF.Services.isTransferInventoryService(session.CurrentUserProfile);
            var oservicedata = new OM.isTransferInventory();
            var oserviceinfo = new OM.isTransferInventory_Info();
            var oresult = new Camstar.WCF.Services.isTransferInventory_Result();

            oservicedata.isInventoryLocation = InventoryLocation.Data as OM.NamedObjectRef;

            oserviceinfo.isToInventoryLocation = FieldInfoUtil.RequestSelectionValue();
            var ServiceRequest = new Camstar.WCF.Services.isTransferInventory_Request();
            ServiceRequest.Info = oserviceinfo;

            var oresultStatus = new OM.ResultStatus();

            OM.ResultStatus ResultStatus = oservice.GetEnvironment(oservicedata, ServiceRequest, out oresult);

            if (ResultStatus.IsSuccess)
            {
                if (oresult.Environment.isToInventoryLocation != null)
                {
                    ToLocation.SetSelectionValues(oresult.Environment.isToInventoryLocation.SelectionValues);
                    CamstarWebControl.SetRenderToClient(ToLocation);
                }
            }

        }

        protected void FetchData()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.isManageInventoryService(session.CurrentUserProfile);
            var servicedata = new OM.isManageInventory();
            var serviceinfo = new OM.isManageInventory_Info();
            var result = new Camstar.WCF.Services.isManageInventory_Result();

            var oServiceRequest = new Camstar.WCF.Services.isManageInventory_Request();
            serviceinfo.isDefaultInventoryLocation = FieldInfoUtil.RequestValue();

            oServiceRequest.Info = serviceinfo;

            var resultStatus = new OM.ResultStatus();


            OM.ResultStatus oResultStatus = service.GetEnvironment(servicedata, oServiceRequest, out result);

            if (oResultStatus.IsSuccess)
            {
                if (result.Value.isDefaultInventoryLocation != null)
                {
                    InventoryLocation.Data = result.Value.isDefaultInventoryLocation.ToString();
                }
            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as PERS.CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "Reset":
                        {
                            PageClearData();
                            break;
                        }
                    case "CommitAdd":
                        {
                            e.Result = CommitAddToInventory();
                            var templocation = InventoryLocation.Data;
                            DisplayMessage(e.Result);
                            InventoryLocation.ClearData();
                            InventoryLocation.Data = templocation;

                            break;
                        }
                    case "CommitRemove":
                        {
                            e.Result = CommitRemoveFromInventory();
                            var templocation = InventoryLocation.Data;
                            DisplayMessage(e.Result);
                            InventoryLocation.ClearData();
                            InventoryLocation.Data = templocation;

                            break;
                        }
                    case "CommitTransfer":
                        {
                            e.Result = CommitTransferFromInventory();
                            var templocation = InventoryLocation.Data;
                            DisplayMessage(e.Result);
                            InventoryLocation.ClearData();
                            InventoryLocation.Data = templocation;

                            break;
                        }
                }
            }
        }

        protected virtual OM.ResultStatus CommitTransferFromInventory()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.isTransferInventoryService(session.CurrentUserProfile);
            var servicedata = new OM.isTransferInventory();
            var serviceinfo = new OM.isTransferInventory_Info();
            var result = new Camstar.WCF.Services.isTransferInventory_Result();

            servicedata.isInventoryLocation = InventoryLocation.Data as OM.NamedObjectRef;
            servicedata.isToInventoryLocation = ToLocation.Data as OM.NamedObjectRef;
            if (ToLocation.Data == null)
                return new OM.ResultStatus("Please select a to inventory location.", false);

            int NoOfDetails = 0;
            if (_gridIsInventoryDetails.TotalRowCount != 0)
            {
                NoOfDetails = _gridIsInventoryDetails.GridContext.SelectedRowIDs.Count();
            }

            OM.isInventoryDetails[] InvServiceDetails = new OM.isInventoryDetails[NoOfDetails];
            int i = 0;

            if ((_gridIsInventoryDetails.GridContext as BoundContext).GetSelectedItems(false) != null)
            {

                foreach (OM.isInventoryDetails resultDetaillist in (_gridIsInventoryDetails.GridContext as BoundContext).GetSelectedItems(false))
                {

                    InvServiceDetails[i] = new OM.isInventoryDetails();
                    {
                        InvServiceDetails[i].isContainer = new OM.ContainerRef();
                        if (resultDetaillist.isContainer != null)
                            InvServiceDetails[i].isContainer.Name = resultDetaillist.isContainer.Name;
                    }

                    InvServiceDetails[i].isLot = resultDetaillist.isLot;
                    InvServiceDetails[i].isProduct = resultDetaillist.isProduct;
                    InvServiceDetails[i].isQty = resultDetaillist.isQty;
                    InvServiceDetails[i].isUOM = resultDetaillist.isUOM;
                    InvServiceDetails[i].isName = resultDetaillist.isName;
                    InvServiceDetails[i].isRemovalStrategy = resultDetaillist.isRemovalStrategy;
                    InvServiceDetails[i].isExpirationDate = resultDetaillist.isExpirationDate;

                    i++;
                }
            }

            int j = 0;
            servicedata.isServiceDetails = new OM.isInventoryServiceDetails[NoOfDetails];
            foreach (OM.isInventoryDetails resultDetaillist in InvServiceDetails)
            {
                servicedata.isServiceDetails[j] = new OM.isInventoryServiceDetails();
                {
                    servicedata.isServiceDetails[j].isContainer = resultDetaillist.isContainer;
                    servicedata.isServiceDetails[j].isProduct = resultDetaillist.isProduct;
                    servicedata.isServiceDetails[j].isQty = resultDetaillist.isQty;
                    servicedata.isServiceDetails[j].isUOM = resultDetaillist.isUOM;
                    servicedata.isServiceDetails[j].isLot = resultDetaillist.isLot;
                    servicedata.isServiceDetails[j].isName = resultDetaillist.isName;
                    if (resultDetaillist.isRemovalStrategy != null)
                        servicedata.isServiceDetails[j].isRemovalStrategy = (int)resultDetaillist.isRemovalStrategy;
                    servicedata.isServiceDetails[j].isExpirationDate = resultDetaillist.isExpirationDate;

                }
                j++;
            }
            var oServiceRequest = new Camstar.WCF.Services.isTransferInventory_Request();
            oServiceRequest.Info = serviceinfo;

            var resultStatus = new OM.ResultStatus();

            OM.ResultStatus oResultStatus = service.ExecuteTransaction(servicedata, oServiceRequest, out result);

            return oResultStatus;
        }

        void RemovalStrategy_DataChanged(object sender, System.EventArgs e)
        {
            if (RemovalStrategy.SelectionData != null && (OM.isRemovalStrategyEnum)RemovalStrategy.SelectionData ==
                OM.isRemovalStrategyEnum.FEFO)
            {
                if (ContainerName.Data != null)
                    ContainerName.LoadOrClearDependentValues();
                ExpirationDate.Enabled = true;
            }
            else
            {
                ExpirationDate.ClearData();
                ExpirationDate.Enabled = false;
            }

        }

        protected virtual OM.ResultStatus CommitRemoveFromInventory()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.isRemoveInventoryService(session.CurrentUserProfile);
            var servicedata = new OM.isRemoveInventory();
            var serviceinfo = new OM.isRemoveInventory_Info();
            var result = new Camstar.WCF.Services.isRemoveInventory_Result();

            servicedata.isInventoryLocation = InventoryLocation.Data as OM.NamedObjectRef;
            if (InventoryLocation.Data == null)
                return new OM.ResultStatus("Please select an inventory location.", false);


            int NoOfDetails = 0;
            if (_gridIsInventoryDetails.TotalRowCount != 0)
            {
                if (_gridIsInventoryDetails.GridContext.SelectedRowIDs != null)
                    NoOfDetails = _gridIsInventoryDetails.GridContext.SelectedRowIDs.Count();
                else
                    return new OM.ResultStatus("You must select an item in order to proceed with Remove.", false);
            }

            OM.isInventoryDetails[] InvServiceDetails = new OM.isInventoryDetails[NoOfDetails];
            int i = 0;

            if ((_gridIsInventoryDetails.GridContext as BoundContext).GetSelectedItems(false) != null)
            {

                foreach (OM.isInventoryDetails resultDetaillist in (_gridIsInventoryDetails.GridContext as BoundContext).GetSelectedItems(false))
                {

                    InvServiceDetails[i] = new OM.isInventoryDetails();
                    {
                        InvServiceDetails[i].isContainer = new OM.ContainerRef();
                        if (resultDetaillist.isContainer != null)
                            InvServiceDetails[i].isContainer.Name = resultDetaillist.isContainer.Name;
                    }

                    InvServiceDetails[i].isLot = resultDetaillist.isLot;
                    InvServiceDetails[i].isProduct = resultDetaillist.isProduct;
                    InvServiceDetails[i].isQty = resultDetaillist.isQty;
                    InvServiceDetails[i].isUOM = resultDetaillist.isUOM;
                    InvServiceDetails[i].isName = resultDetaillist.isName;

                    i++;

                }
            }
            //servicedata.isServiceDetails = InvServiceDetails;

            int j = 0;
            servicedata.isServiceDetails = new OM.isInventoryServiceDetails[NoOfDetails];
            foreach (OM.isInventoryDetails resultDetaillist in InvServiceDetails)
            {
                servicedata.isServiceDetails[j] = new OM.isInventoryServiceDetails();
                {
                    servicedata.isServiceDetails[j].isContainer = resultDetaillist.isContainer;
                    servicedata.isServiceDetails[j].isProduct = resultDetaillist.isProduct;
                    servicedata.isServiceDetails[j].isQty = resultDetaillist.isQty;
                    servicedata.isServiceDetails[j].isUOM = resultDetaillist.isUOM;
                    servicedata.isServiceDetails[j].isLot = resultDetaillist.isLot;
                    servicedata.isServiceDetails[j].isName = resultDetaillist.isName;

                }
                j++;
            }
            var iServiceRequest = new Camstar.WCF.Services.isRemoveInventory_Request();
            iServiceRequest.Info = serviceinfo;

            var resultStatus = new OM.ResultStatus();

            OM.ResultStatus iResultStatus = service.ExecuteTransaction(servicedata, iServiceRequest, out result);

            return iResultStatus;
        }

        protected virtual OM.ResultStatus CommitAddToInventory()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.isManageInventoryService(session.CurrentUserProfile);
            var servicedata = new OM.isManageInventory();
            var serviceinfo = new OM.isManageInventory_Info();
            var result = new Camstar.WCF.Services.isManageInventory_Result();

            servicedata.isInventoryLocation = InventoryLocation.Data as OM.NamedObjectRef;
            if (InventoryLocation.Data == null)
                return new OM.ResultStatus("Please select an inventory location.", false);

            servicedata.isContainer = ContainerName.Data as OM.ContainerRef;
            if (product.Data == null && servicedata.isContainer == null)
                return new OM.ResultStatus("Please select a product.", false);

            if (LotField.Data != null)
                servicedata.isLot = LotField.Data.ToString();
            servicedata.isProduct = product.Data as OM.RevisionedObjectRef;
            if (QtyField.Data != null)
                servicedata.isQty = new OM.Primitive<double>((Double)QtyField.Data);
            if (RemovalStrategy.SelectionData != null)
                servicedata.isRemovalStrategy = new OM.Enumeration<OM.isRemovalStrategyEnum, int>((OM.isRemovalStrategyEnum)RemovalStrategy.SelectionData);
            if (ExpirationDate.Data != null && (OM.isRemovalStrategyEnum)RemovalStrategy.SelectionData == OM.isRemovalStrategyEnum.FEFO)
                servicedata.isExpirationDate = new OM.Primitive<DateTime>((DateTime)ExpirationDate.Data);
            else if (ExpirationDate.Data == null && RemovalStrategy.SelectionData != null && (OM.isRemovalStrategyEnum)RemovalStrategy.SelectionData == OM.isRemovalStrategyEnum.FEFO)
            {
                var label = "Please select an expiration date for FEFO strategy.";
                var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                if (labelCache != null)
                {
                    OM.Label lbl = labelCache.GetLabelByName("isExpirationDateRequired");
                    if (lbl != null)
                        label = lbl.Value;
                }
                return new OM.ResultStatus(label, false);
            }



            servicedata.isUOM = UOM.Data as OM.NamedObjectRef;
            servicedata.isInventoryLocation = InventoryLocation.Data as OM.NamedObjectRef;
            if (Containerorlotname.Data != null)
                servicedata.isName = Containerorlotname.Data.ToString();

            var oServiceRequest = new Camstar.WCF.Services.isManageInventory_Request();
            oServiceRequest.Info = serviceinfo;

            var resultStatus = new OM.ResultStatus();

            OM.ResultStatus oResultStatus = service.ExecuteTransaction(servicedata, oServiceRequest, out result);

            return oResultStatus;
        }



        public void PageClearData()
        {
            ContainerName.Enabled = true;
            LotField.Enabled = true;
            ContainerName.ClearData();
            LotField.ClearData();
            QtyField.ClearData();
            UOM.ClearData();
            product.ClearData();
            TransferAction.IsDisabled = true;
            RemoveAction.IsDisabled = true;
            InventoryLocation.Data = null;
            _gridIsInventoryDetails.ClearData();
            RemovalStrategy.ClearData();
            ExpirationDate.ClearData();
            FetchData();
        }

    }
}
