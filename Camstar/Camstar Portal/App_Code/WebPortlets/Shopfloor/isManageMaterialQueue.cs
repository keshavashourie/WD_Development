using System.Data;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Web;
using System.Linq;
using OM = Camstar.WCF.ObjectStack;
using System;
using System.Collections.Generic;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.Utilities;
using PERS = Camstar.WebPortal.Personalization;
using System.Web.UI;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class isManageMaterialQueue : MatrixWebPart
    {
        #region Properties

        protected bool _MaterialQueueChanging = false;
        protected bool isIsKit
        {
            get
            {
                return KitContainer.Data != null || KitMfgOrder.Data != null || KitOperation.Data != null || false;
            }
        }

        protected virtual CWC.NamedObject MaterialQueue
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isMaterialQueue") as CWC.NamedObject;
            }
        }

        protected virtual CWC.NamedObject Resource
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isResource") as CWC.NamedObject;
            }
        }


        protected virtual ContainerListGrid KitContainer
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isContainer") as ContainerListGrid;
            }
        }

        protected virtual CWC.NamedObject KitMfgOrder
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isKitMfgOrder") as CWC.NamedObject;
            }
        }

        protected virtual CWC.NamedObject KitOperation
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isOperation") as CWC.NamedObject;
            }
        }

        protected virtual CWC.NamedObject MfgOrder
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isMfgOrder") as CWC.NamedObject;
            }
        }

        protected virtual CWC.RevisionedObject Product
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isProduct") as CWC.RevisionedObject;
            }
        }

        protected virtual CWC.RevisionedObject isERPRoute
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isERPRoute") as CWC.RevisionedObject;
            }
        }

        protected virtual CWC.RevisionedObject Spec
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isSpec") as CWC.RevisionedObject;
            }
        }

        protected virtual CWC.NamedSubentity RouteStep
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isRouteStep") as CWC.NamedSubentity;
            }
        }

        protected virtual JQDataGrid _gridisMaterialQueueDetails
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isMaterialQueueDetails") as JQDataGrid;
            }
        }

        protected virtual JQDataGrid _gridisMaterialQueueReplenishDetails
        {
            get
            {
                return Page.FindCamstarControl("isMatQueueMatReplenishmentDtlsGrid") as JQDataGrid;
            }
        }

        protected virtual CWC.CheckBox isActive
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isActive") as CWC.CheckBox;
            }
        }

        protected virtual CWC.NamedObject InventoryLocation
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isInventoryLocation") as CWC.NamedObject;
            }
        }

        protected virtual CWC.NamedObject UOM
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isUOM") as CWC.NamedObject;
            }
        }

        protected virtual CWC.DropDownList ContainerOrLot
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isContainerorLot") as CWC.DropDownList;
            }
        }

        protected virtual CWC.TextBox Qty
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isQty") as CWC.TextBox;
            }
        }
        protected virtual CWC.TextBox QtyToValidate
        {
            get
            {
                return Page.FindCamstarControl("isQtyToValidate") as CWC.TextBox;
            }
        }


        protected virtual PERS.UIAction ResetAction {
            get {
                return Page.ActionDispatcher.GetActionByName("Reset");
            }
        }

        protected virtual PERS.UIAction LoadAction {
            get {
                return Page.ActionDispatcher.GetActionByName("Load");
            }
        }

        protected virtual PERS.UIAction UnloadAction {
            get {
                return Page.ActionDispatcher.GetActionByName("Unload");
            }
        }

        protected virtual CWC.RevisionedObject ProductFromLoc
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isProductFromInventoryLoc") as CWC.RevisionedObject;
            }
        }

        protected virtual CWC.CheckBox isKittingOrder
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isKitting") as CWC.CheckBox;
            }
        }

        protected virtual CWC.CheckBox isERP
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isERP") as CWC.CheckBox;
            }
        }

        protected virtual CWC.CheckBox isManualConsumed
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isManualConsumed") as CWC.CheckBox;
            }
        }

        #endregion

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/User/Industry Solutions/isMaterialQueue.js");
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            Resource.Enabled = !(KitMfgOrder.Data != null || KitOperation.Data != null || KitContainer.Data != null);
            KitMfgOrder.Enabled = !(Resource.Data != null || KitOperation.Data != null || KitContainer.Data != null);
            KitOperation.Enabled = !(Resource.Data != null || KitMfgOrder.Data != null || KitContainer.Data != null);
            KitContainer.Enabled = !(Resource.Data != null || KitOperation.Data != null || KitMfgOrder.Data != null);
        }

        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            if (IsResponsive)
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                if (labelCache != null)
                {
                    var label = labelCache.GetLabelByName("CreateEvent_Submit");
                    LoadAction.LabelName = label.Name;
                    LoadAction.LabelText = FrameworkManagerUtil.GetLabelValue("CreateEvent_Submit");
                }
            }
        }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

            MaterialQueue.DataChanged += MaterialQueue_DataChanged;
            Resource.DataChanged += Resource_DataChanged;
            MfgOrder.DataChanged += MfgOrder_DataChanged;
            RouteStep.DataChanged += RouteStep_DataChanged;
            Spec.DataChanged += Spec_DataChanged;
            Product.DataChanged += Product_DataChanged;
            InventoryLocation.DataChanged += InventoryLocation_DataChanged;
            ContainerOrLot.DataChanged += ContainerOrLot_DataChanged;
            _gridisMaterialQueueDetails.RowSelected += _gridisMaterialQueueDetails_RowSelected;
            isKittingOrder.DataChanged += IsKittingOrder_DataChanged;
            KitContainer.DataChanged += KitContainer_DataChanged;
            KitMfgOrder.DataChanged += KitMfgOrder_DataChanged;
            KitOperation.DataChanged += KitOperation_DataChanged;
            ProductFromLoc.DataChanged += ProductFromLoc_DataChanged;


            Qty.DataChanged += Qty_DataChanged;
            //Load.Enabled = true;
            //Unload.Enabled = false;
            //Reset.Enabled = true;
            ResetAction.IsDisabled = false;
			LoadAction.IsDisabled = false;
			UnloadAction.IsDisabled = true;

        }

        private void IsKittingOrder_DataChanged(object sender, EventArgs e)
        {
            MfgOrder.ClearData();
        }

        void PopulateExData()
        {
            var tempMaterialQueue = MaterialQueue.Data;
            var tempInvLoc = InventoryLocation.Data;
            var tempmfgorder = MfgOrder.Data;
            var tempprod = Product.Data;

            MaterialQueue.ClearData();
            MaterialQueue.Data = tempMaterialQueue;
            InventoryLocation.Data = tempInvLoc;
            MfgOrder.Data = tempmfgorder;
            Product.Data = tempprod;
        }

        void Qty_DataChanged(object sender, EventArgs e)
        {
            if (Qty.Data != null && QtyToValidate.Data != null)
            {


                int actualqty = int.Parse(QtyToValidate.Data.ToString());
                int inputqty = int.Parse(Qty.Data.ToString());

                if (inputqty > actualqty)
                {
                    DisplayMessage(new OM.ResultStatus("Qty Exceeded original Container or Lot or Product Qty", false));
                    ContainerOrLot.ClearData();

                }
            }
        }



        ResponseData _gridisMaterialQueueDetails_RowSelected(object sender, JQGridEventArgs args)
        {
            if (_gridisMaterialQueueDetails.GridContext.SelectedRowIDs != null)
            {
                if (_gridisMaterialQueueDetails.GridContext.SelectedRowIDs.Count() == 0)
                {
                    UnloadAction.IsDisabled = true;
                }
                else
                {
                    UnloadAction.IsDisabled = false;

                }

                if (_gridisMaterialQueueDetails.GridContext.SelectedRowIDs.Count() > 1)
                {
                    LoadAction.IsDisabled = true;
                }
                else
                {
                    LoadAction.IsDisabled = false;
                }
            }
            return null;
        }

        void Product_DataChanged(object sender, EventArgs e)
        {
            ReloadMaterialRequirements();
        }

        void Spec_DataChanged(object sender, EventArgs e)
        {
            ReloadMaterialRequirements();
        }

        void RouteStep_DataChanged(object sender, EventArgs e)
        {
            ReloadMaterialRequirements();
        }

        void MfgOrder_DataChanged(object sender, EventArgs e)
        {
            ReloadMaterialRequirements();
        }

        void KitContainer_DataChanged(object sender, EventArgs e)
        {
            FillFieldsOfMQ(sender, e);
        }

        void KitMfgOrder_DataChanged(object sender, EventArgs e)
        {
            FillFieldsOfMQ(sender, e);
        }

        void KitOperation_DataChanged(object sender, EventArgs e)
        {
            FillFieldsOfMQ(sender, e);
        }

        void KitResource_DataChanged(object sender, EventArgs e)
        {
            FillFieldsOfMQ(sender, e);
        }

        void ReloadMaterialRequirements()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.isManageMaterialQueueService(session.CurrentUserProfile);
            var servicedata = new OM.isManageMaterialQueue();
            var serviceinfo = new OM.isManageMaterialQueue_Info();
            var result = new Camstar.WCF.Services.isManageMaterialQueue_Result();

            if (InventoryLocation.Data != null)
                servicedata.isInventoryLocation = InventoryLocation.Data as OM.NamedObjectRef;

            if (MfgOrder.Data != null)
            {
                servicedata.isMfgOrder = MfgOrder.Data as OM.NamedObjectRef;
                Product.Enabled = false;
            }
            else
            {
                Product.Enabled = true;
            }

            if (RouteStep.Data != null)
            {
                servicedata.isRouteStep = RouteStep.Data as OM.NamedSubentityRef;
                if (isERPRoute.Data != null)
                    servicedata.isRouteStep.Parent = isERPRoute.Data as OM.BaseObjectRef;
            }

            if (Spec.Data != null)
                servicedata.isSpec = Spec.Data as OM.RevisionedObjectRef;

            if (Product.Data != null)
            {
                servicedata.isProduct = Product.Data as OM.RevisionedObjectRef;
                MfgOrder.Enabled = false;
            }
            else
            {
                MfgOrder.Enabled = true;
            }

            serviceinfo.isMaterialRequirements = new OM.isMaterialRequirementService_Info()
            {
                Product = new OM.Info(true),
                isProductDescription = new OM.Info(true),
                IssueControl = new OM.Info(true),
                QtyRequired = new OM.Info(true),
                RouteStep = new OM.Info(true),
                UOM = new OM.Info(true),
                Spec = new OM.Info(true)
            };

            serviceinfo.isERPRoute = FieldInfoUtil.RequestValue();

            var oServiceRequest = new Camstar.WCF.Services.isManageMaterialQueue_Request();
            oServiceRequest.Info = serviceinfo;

            var resultStatus = new OM.ResultStatus();

            OM.ResultStatus oResultStatus = service.GetEnvironment(servicedata, oServiceRequest, out result);


        }
        void Resource_DataChanged(object sender, EventArgs e)
        {
            if (!_MaterialQueueChanging)
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new Camstar.WCF.Services.isManageMaterialQueueService(session.CurrentUserProfile);
                var servicedata = new OM.isManageMaterialQueue();
                var serviceinfo = new OM.isManageMaterialQueue_Info();
                var result = new Camstar.WCF.Services.isManageMaterialQueue_Result();

                servicedata.isResource = Resource.Data as OM.NamedObjectRef;

                serviceinfo.isMaterialQueueTmp = FieldInfoUtil.RequestValue();

                var oServiceRequest = new Camstar.WCF.Services.isManageMaterialQueue_Request();
                oServiceRequest.Info = serviceinfo;

                var resultStatus = new OM.ResultStatus();

                OM.ResultStatus oResultStatus = service.GetEnvironment(servicedata, oServiceRequest, out result);

                if (oResultStatus.IsSuccess)
                {
                    if (result.Value.isMaterialQueueTmp != null)
                    {
                        MaterialQueue.Data = result.Value.isMaterialQueueTmp.ToString();
                    }

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

                    case "Load":
                        {
                            if (ContainerOrLot.Data != null || ProductFromLoc.Data != null)
                            {
                                e.Result = LoadToMaterialQueue();

                                DisplayMessage(e.Result);
                                PopulateExData();
                            }
                            else

                            {
                                e.Result = SortingMaterialQueue();

                                DisplayMessage(e.Result);
                                PopulateExData();
                            }


                            break;
                        }

                    case "Unload":
                        {
                            e.Result = UnloadFromMaterialQueue();

                            DisplayMessage(e.Result);
                            PopulateExData();


                            break;
                        }
                }
            }
        }

        protected virtual OM.ResultStatus SortingMaterialQueue()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.isLoadMaterialQueueService(session.CurrentUserProfile);
            var servicedata = new OM.isLoadMaterialQueue();
            var serviceinfo = new OM.isLoadMaterialQueue_Info();
            var result = new Camstar.WCF.Services.isLoadMaterialQueue_Result();

            if (MaterialQueue.Data == null)
                return new OM.ResultStatus("Material Queue is required.", false);

            servicedata.isMaterialQueue = MaterialQueue.Data as OM.NamedObjectRef;
            servicedata.isResourceChange = Resource.Data as OM.NamedObjectRef;
            servicedata.isActive = isActive.IsChecked;

            servicedata.isContainerChange = KitContainer.Data as OM.ContainerRef;
            servicedata.isMfgOrderChange = KitMfgOrder.Data as OM.NamedObjectRef;
            servicedata.isOperationChange = KitOperation.Data as OM.NamedObjectRef;
            servicedata.isIsKit = isIsKit;

            //Replenish Grid

            int NoOfReplenishDetails = 0;
            if (_gridisMaterialQueueReplenishDetails.TotalRowCount != 0)
            {
                NoOfReplenishDetails = _gridisMaterialQueueReplenishDetails.TotalRowCount;
            }

            OM.isMatQueueMatReplenishmentDtls[] MaterialQueueReplenishDetails = new OM.isMatQueueMatReplenishmentDtls[NoOfReplenishDetails];
            int x = 0;
            if (_gridisMaterialQueueReplenishDetails.Data != null)
            {
                foreach (OM.isMatQueueMatReplenishmentDtls resultReplenishDetaillist in _gridisMaterialQueueReplenishDetails.Data as OM.isMatQueueMatReplenishmentDtls[])
                {

                    MaterialQueueReplenishDetails[x] = new OM.isMatQueueMatReplenishmentDtls();
                    MaterialQueueReplenishDetails[x].isProduct = resultReplenishDetaillist.isProduct;
                    MaterialQueueReplenishDetails[x].isThresholdQty = resultReplenishDetaillist.isThresholdQty;
                    MaterialQueueReplenishDetails[x].isUOM = resultReplenishDetaillist.isUOM;
                    MaterialQueueReplenishDetails[x].isReplenishQty = resultReplenishDetaillist.isReplenishQty;
                    MaterialQueueReplenishDetails[x].isManualReplenish = resultReplenishDetaillist.isManualReplenish;
                    x++;

                }

                int y = 0;
                servicedata.isMatQueueMatReplenishmentDtls = new OM.isMatQueueMatReplenishmentDtls[NoOfReplenishDetails];
                foreach (OM.isMatQueueMatReplenishmentDtls resultReplenishDetaillist in MaterialQueueReplenishDetails)
                {
                    servicedata.isMatQueueMatReplenishmentDtls[y] = new OM.isMatQueueMatReplenishmentDtls();
                    {
                        servicedata.isMatQueueMatReplenishmentDtls[y].isProduct = resultReplenishDetaillist.isProduct;
                        servicedata.isMatQueueMatReplenishmentDtls[y].isThresholdQty = resultReplenishDetaillist.isThresholdQty;
                        servicedata.isMatQueueMatReplenishmentDtls[y].isUOM = resultReplenishDetaillist.isUOM;
                        servicedata.isMatQueueMatReplenishmentDtls[y].isReplenishQty = resultReplenishDetaillist.isReplenishQty;
                        servicedata.isMatQueueMatReplenishmentDtls[y].isManualReplenish = resultReplenishDetaillist.isManualReplenish;

                    }
                    y++;
                }
                var iServiceReplenishRequest = new Camstar.WCF.Services.isLoadMaterialQueue_Request();
                iServiceReplenishRequest.Info = serviceinfo;

                var resultReplenishStatus = new OM.ResultStatus();

                OM.ResultStatus iresultReplenishStatus = service.ExecuteTransaction(servicedata, iServiceReplenishRequest, out result);

                if (!iresultReplenishStatus.IsSuccess)
                    Page.DisplayMessage(iresultReplenishStatus);
            }


            //Details Grid

            int NoOfDetails = 0;
            if (_gridisMaterialQueueDetails.TotalRowCount != 0)
            {

                NoOfDetails = _gridisMaterialQueueDetails.GridContext.GetTotalRows();

            }

            OM.isMaterialQueueDetails[] InvServiceDetails = new OM.isMaterialQueueDetails[NoOfDetails];
            int i = 0;

            if (_gridisMaterialQueueDetails.Data != null)
            {
                foreach (OM.isMaterialQueueDetails resultDetaillist in _gridisMaterialQueueDetails.Data as OM.isMaterialQueueDetails[])
                {

                    InvServiceDetails[i] = new OM.isMaterialQueueDetails();
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
                    InvServiceDetails[i].isContainer = resultDetaillist.isContainer;
                    InvServiceDetails[i].isSequence = resultDetaillist.isSequence;
                    InvServiceDetails[i].isInventoryLocation = resultDetaillist.isInventoryLocation;

                    i++;

                }

                int j = 0;
                int sequence = 1;
                servicedata.isMaterialQueueServiceDetails = new OM.isMaterialQueueServiceDetails[NoOfDetails];
                foreach (OM.isMaterialQueueDetails resultDetaillist in InvServiceDetails)
                {
                    servicedata.isMaterialQueueServiceDetails[j] = new OM.isMaterialQueueServiceDetails();
                    {
                        servicedata.isMaterialQueueServiceDetails[j].isContainer = resultDetaillist.isContainer;
                        servicedata.isMaterialQueueServiceDetails[j].isProduct = resultDetaillist.isProduct;
                        servicedata.isMaterialQueueServiceDetails[j].isQty = resultDetaillist.isQty;
                        servicedata.isMaterialQueueServiceDetails[j].isUOM = resultDetaillist.isUOM;
                        servicedata.isMaterialQueueServiceDetails[j].isLot = resultDetaillist.isLot;
                        servicedata.isMaterialQueueServiceDetails[j].isName = resultDetaillist.isName;
                        servicedata.isMaterialQueueServiceDetails[j].isSequence = sequence;
                        servicedata.isMaterialQueueServiceDetails[j].isInventoryLocation = resultDetaillist.isInventoryLocation;

                    }
                    j++;
                    sequence++;
                }
                var iServiceRequest = new Camstar.WCF.Services.isLoadMaterialQueue_Request();
                iServiceRequest.Info = serviceinfo;

                var resultStatus = new OM.ResultStatus();

                OM.ResultStatus iResultStatus = service.ExecuteTransaction(servicedata, iServiceRequest, out result);

                return iResultStatus;
            }
            else
            {
                return new OM.ResultStatus("Grid details is empty.", false);
            }
        }

        protected virtual OM.ResultStatus UnloadFromMaterialQueue()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.isUnloadMaterialQueueService(session.CurrentUserProfile);
            var servicedata = new OM.isUnloadMaterialQueue();
            var serviceinfo = new OM.isUnloadMaterialQueue_Info();
            var result = new Camstar.WCF.Services.isUnloadMaterialQueue_Result();

            if (MaterialQueue.Data == null)
                return new OM.ResultStatus("Material Queue is required.", false);

            servicedata.isMaterialQueue = MaterialQueue.Data as OM.NamedObjectRef;

            //Replenish Grid

            int NoOfReplenishDetails = 0;
            if (_gridisMaterialQueueReplenishDetails.TotalRowCount != 0)
            {
                NoOfReplenishDetails = _gridisMaterialQueueReplenishDetails.TotalRowCount;
            }

            OM.isMatQueueMatReplenishmentDtls[] MaterialQueueReplenishDetails = new OM.isMatQueueMatReplenishmentDtls[NoOfReplenishDetails];
            int x = 0;
            if (_gridisMaterialQueueReplenishDetails.Data != null)
            {
                foreach (OM.isMatQueueMatReplenishmentDtls resultReplenishDetaillist in _gridisMaterialQueueReplenishDetails.Data as OM.isMatQueueMatReplenishmentDtls[])
                {

                    MaterialQueueReplenishDetails[x] = new OM.isMatQueueMatReplenishmentDtls();
                    MaterialQueueReplenishDetails[x].isProduct = resultReplenishDetaillist.isProduct;
                    MaterialQueueReplenishDetails[x].isThresholdQty = resultReplenishDetaillist.isThresholdQty;
                    MaterialQueueReplenishDetails[x].isUOM = resultReplenishDetaillist.isUOM;
                    MaterialQueueReplenishDetails[x].isReplenishQty = resultReplenishDetaillist.isReplenishQty;
                    MaterialQueueReplenishDetails[x].isManualReplenish = resultReplenishDetaillist.isManualReplenish;
                    x++;

                }

                int y = 0;
                servicedata.isMatQueueMatReplenishmentDtls = new OM.isMatQueueMatReplenishmentDtls[NoOfReplenishDetails];
                foreach (OM.isMatQueueMatReplenishmentDtls resultReplenishDetaillist in MaterialQueueReplenishDetails)
                {
                    servicedata.isMatQueueMatReplenishmentDtls[y] = new OM.isMatQueueMatReplenishmentDtls();
                    {
                        servicedata.isMatQueueMatReplenishmentDtls[y].isProduct = resultReplenishDetaillist.isProduct;
                        servicedata.isMatQueueMatReplenishmentDtls[y].isThresholdQty = resultReplenishDetaillist.isThresholdQty;
                        servicedata.isMatQueueMatReplenishmentDtls[y].isUOM = resultReplenishDetaillist.isUOM;
                        servicedata.isMatQueueMatReplenishmentDtls[y].isReplenishQty = resultReplenishDetaillist.isReplenishQty;
                        servicedata.isMatQueueMatReplenishmentDtls[y].isManualReplenish = resultReplenishDetaillist.isManualReplenish;

                    }
                    y++;
                }
            }
            var iServiceReplenishRequest = new Camstar.WCF.Services.isUnloadMaterialQueue_Request();
            iServiceReplenishRequest.Info = serviceinfo;

            var resultReplenishStatus = new OM.ResultStatus();

            OM.ResultStatus iResultReplenishStatus = service.ExecuteTransaction(servicedata, iServiceReplenishRequest, out result);
            if (!iResultReplenishStatus.IsSuccess)
                Page.DisplayMessage(iResultReplenishStatus);

            //Details Grid

            int NoOfDetails = 0;
            if (_gridisMaterialQueueDetails.TotalRowCount != 0)
            {
                if (_gridisMaterialQueueDetails.GridContext.SelectedRowIDs != null)
                    NoOfDetails = _gridisMaterialQueueDetails.GridContext.SelectedRowIDs.Count();
                else
                    return new OM.ResultStatus("You must select an item in order to proceed with Unload.", false);
            }

            OM.isMaterialQueueDetails[] InvServiceDetails = new OM.isMaterialQueueDetails[NoOfDetails];
            int i = 0;

            if ((_gridisMaterialQueueDetails.GridContext as BoundContext).GetSelectedItems(false) != null)
            {

                foreach (OM.isMaterialQueueDetails resultDetaillist in (_gridisMaterialQueueDetails.GridContext as BoundContext).GetSelectedItems(false))
                {

                    InvServiceDetails[i] = new OM.isMaterialQueueDetails();
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
                    InvServiceDetails[i].isContainer = resultDetaillist.isContainer;
                    InvServiceDetails[i].isSequence = resultDetaillist.isSequence;
                    InvServiceDetails[i].isInventoryLocation = resultDetaillist.isInventoryLocation;
                    InvServiceDetails[i].isRemovalStrategy = resultDetaillist.isRemovalStrategy;
                    InvServiceDetails[i].isExpirationDate = resultDetaillist.isExpirationDate;

                    i++;

                }
            }


            int j = 0;
            servicedata.isMaterialQueueServiceDetails = new OM.isMaterialQueueServiceDetails[NoOfDetails];
            foreach (OM.isMaterialQueueDetails resultDetaillist in InvServiceDetails)
            {
                servicedata.isMaterialQueueServiceDetails[j] = new OM.isMaterialQueueServiceDetails();
                {
                    servicedata.isMaterialQueueServiceDetails[j].isContainer = resultDetaillist.isContainer;
                    servicedata.isMaterialQueueServiceDetails[j].isProduct = resultDetaillist.isProduct;
                    servicedata.isMaterialQueueServiceDetails[j].isQty = resultDetaillist.isQty;
                    servicedata.isMaterialQueueServiceDetails[j].isUOM = resultDetaillist.isUOM;
                    servicedata.isMaterialQueueServiceDetails[j].isLot = resultDetaillist.isLot;
                    servicedata.isMaterialQueueServiceDetails[j].isName = resultDetaillist.isName;
                    servicedata.isMaterialQueueServiceDetails[j].isSequence = resultDetaillist.isSequence;
                    servicedata.isMaterialQueueServiceDetails[j].isInventoryLocation = resultDetaillist.isInventoryLocation;
                    servicedata.isMaterialQueueServiceDetails[j].isRemovalStrategy = resultDetaillist.isRemovalStrategy;
                    servicedata.isMaterialQueueServiceDetails[j].isExpirationDate = resultDetaillist.isExpirationDate;

                }
                j++;
            }
            var iServiceRequest = new Camstar.WCF.Services.isUnloadMaterialQueue_Request();
            iServiceRequest.Info = serviceinfo;

            var resultStatus = new OM.ResultStatus();

            OM.ResultStatus iResultStatus = service.ExecuteTransaction(servicedata, iServiceRequest, out result);

            return iResultStatus;
        }

        protected virtual OM.ResultStatus LoadToMaterialQueue()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.isLoadMaterialQueueService(session.CurrentUserProfile);
            var servicedata = new OM.isLoadMaterialQueue();
            var serviceinfo = new OM.isLoadMaterialQueue_Info();
            var result = new Camstar.WCF.Services.isLoadMaterialQueue_Result();

            servicedata.isInventoryLocation = InventoryLocation.Data as OM.NamedObjectRef;

            if (MaterialQueue.Data == null)
                return new OM.ResultStatus("Material Queue is required.", false);


            if (ContainerOrLot.Data != null)
                servicedata.isContainerorLot = ContainerOrLot.Data.ToString();

            if (ProductFromLoc.Data != null)
                servicedata.isProductFromInventoryLoc = ProductFromLoc.Data as OM.RevisionedObjectRef;
            servicedata.isMaterialQueue = MaterialQueue.Data as OM.NamedObjectRef;
            servicedata.isProduct = Product.Data as OM.RevisionedObjectRef;
            servicedata.isMfgOrder = MfgOrder.Data as OM.NamedObjectRef;
            servicedata.isResourceChange = Resource.Data as OM.NamedObjectRef;
            servicedata.isActive = isActive.IsChecked;
            if (Qty.Data != null)
                servicedata.isQty = new OM.Primitive<double>((Double)Qty.Data);
            servicedata.isInventoryLocation = InventoryLocation.Data as OM.NamedObjectRef;


            servicedata.isContainerChange = KitContainer.Data as OM.ContainerRef;
            servicedata.isMfgOrderChange = KitMfgOrder.Data as OM.NamedObjectRef;
            servicedata.isOperationChange = KitOperation.Data as OM.NamedObjectRef;
            servicedata.isIsKit = isIsKit;
            servicedata.isERP = isERP.IsChecked;
            servicedata.isManualConsumed = isManualConsumed.IsChecked;

            //Replenish Grid

            int NoOfReplenishDetails = 0;
            if (_gridisMaterialQueueReplenishDetails.TotalRowCount != 0)
            {
                NoOfReplenishDetails = _gridisMaterialQueueReplenishDetails.TotalRowCount;
            }

            OM.isMatQueueMatReplenishmentDtls[] MaterialQueueReplenishDetails = new OM.isMatQueueMatReplenishmentDtls[NoOfReplenishDetails];
            int x = 0;
            if (_gridisMaterialQueueReplenishDetails.Data != null)
            {
                foreach (OM.isMatQueueMatReplenishmentDtls resultReplenishDetaillist in _gridisMaterialQueueReplenishDetails.Data as OM.isMatQueueMatReplenishmentDtls[])
                {

                    MaterialQueueReplenishDetails[x] = new OM.isMatQueueMatReplenishmentDtls();
                    MaterialQueueReplenishDetails[x].isProduct = resultReplenishDetaillist.isProduct;
                    MaterialQueueReplenishDetails[x].isThresholdQty = resultReplenishDetaillist.isThresholdQty;
                    MaterialQueueReplenishDetails[x].isUOM = resultReplenishDetaillist.isUOM;
                    MaterialQueueReplenishDetails[x].isReplenishQty = resultReplenishDetaillist.isReplenishQty;
                    MaterialQueueReplenishDetails[x].isManualReplenish = resultReplenishDetaillist.isManualReplenish;
                    x++;

                }

                int y = 0;
                servicedata.isMatQueueMatReplenishmentDtls = new OM.isMatQueueMatReplenishmentDtls[NoOfReplenishDetails];
                foreach (OM.isMatQueueMatReplenishmentDtls resultReplenishDetaillist in MaterialQueueReplenishDetails)
                {
                    servicedata.isMatQueueMatReplenishmentDtls[y] = new OM.isMatQueueMatReplenishmentDtls();
                    {
                        servicedata.isMatQueueMatReplenishmentDtls[y].isProduct = resultReplenishDetaillist.isProduct;
                        servicedata.isMatQueueMatReplenishmentDtls[y].isThresholdQty = resultReplenishDetaillist.isThresholdQty;
                        servicedata.isMatQueueMatReplenishmentDtls[y].isUOM = resultReplenishDetaillist.isUOM;
                        servicedata.isMatQueueMatReplenishmentDtls[y].isReplenishQty = resultReplenishDetaillist.isReplenishQty;
                        servicedata.isMatQueueMatReplenishmentDtls[y].isManualReplenish = resultReplenishDetaillist.isManualReplenish;

                    }
                    y++;
                }
            }

            var oServiceRequest = new Camstar.WCF.Services.isLoadMaterialQueue_Request();
            oServiceRequest.Info = serviceinfo;

            var resultStatus = new OM.ResultStatus();

            OM.ResultStatus oResultStatus = service.ExecuteTransaction(servicedata, oServiceRequest, out result);

            ContainerOrLot.ClearData();
            ProductFromLoc.ClearData();
            Qty.ClearData();

            return oResultStatus;
        }

        public void PageClearData()
        {
            MaterialQueue.Enabled = true;
            Resource.Enabled = true;
            KitMfgOrder.Enabled = true;
            KitOperation.Enabled = true;
            KitContainer.Enabled = true;
            KitMfgOrder.Enabled = true;
            KitOperation.Enabled = true;
            MfgOrder.Enabled = true;
            Product.Enabled = true;
            isActive.Enabled = true;
            InventoryLocation.Enabled = true;
            ContainerOrLot.Enabled = true;
            Qty.Enabled = true;
            ProductFromLoc.Enabled = true;
            MaterialQueue.ClearData();
            Resource.ClearData();
            KitContainer.ClearData();
            KitMfgOrder.ClearData();
            KitOperation.ClearData();
            MfgOrder.ClearData();
            Product.ClearData();
            QtyToValidate.ClearData();
            ProductFromLoc.ClearData();
            isActive.ClearData();
            InventoryLocation.ClearData();
            _gridisMaterialQueueDetails.ClearData();
            _gridisMaterialQueueReplenishDetails.ClearData();
            ContainerOrLot.ClearData();
            Qty.ClearData();
            isKittingOrder.ClearData();
            isERP.ClearData();
            isManualConsumed.ClearData();
        }

        void InventoryLocation_DataChanged(object sender, EventArgs e)
        {
            Qty.Data = null;
            ProductFromLoc.Data = null;
            ContainerOrLot.Data = null;
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.isManageMaterialQueueService(session.CurrentUserProfile);
            var servicedata = new OM.isManageMaterialQueue();
            var serviceinfo = new OM.isManageMaterialQueue_Info();
            var result = new Camstar.WCF.Services.isManageMaterialQueue_Result();

            servicedata.isInventoryLocation = InventoryLocation.Data as OM.NamedObjectRef;

            serviceinfo.isContainerorLot = FieldInfoUtil.RequestSelectionValue();
            serviceinfo.isProductFromInventoryLoc = FieldInfoUtil.RequestSelectionValue();

            var oServiceRequest = new Camstar.WCF.Services.isManageMaterialQueue_Request();
            oServiceRequest.Info = serviceinfo;

            var resultStatus = new OM.ResultStatus();

            OM.ResultStatus oResultStatus = service.GetEnvironment(servicedata, oServiceRequest, out result);

            if (oResultStatus.IsSuccess)
            {
                if (result.Value.isContainerorLot != null)
                {
                    ContainerOrLot.SetSelectionValues(result.Environment.isContainerorLot.SelectionValues);
                }
                if (result.Value.isProductFromInventoryLoc != null)
                {
                    ProductFromLoc.SetSelectionValues(result.Environment.isProductFromInventoryLoc.SelectionValues);
                }
            }
        }

        void ContainerOrLot_DataChanged(object sender, EventArgs e)
        {
            QtyToValidate.ClearData();
            ProductFromLoc.ClearData();
            Qty.ClearData();
            ProductFromLoc.Enabled = true;
            Qty.Enabled = true;

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.isManageMaterialQueueService(session.CurrentUserProfile);
            var servicedata = new OM.isManageMaterialQueue();
            var serviceinfo = new OM.isManageMaterialQueue_Info();
            var result = new Camstar.WCF.Services.isManageMaterialQueue_Result();

            bool isInContainerList = false;

            if (ContainerOrLot.Data != null && !string.IsNullOrEmpty(ContainerOrLot.Data.ToString()))
            {
                servicedata.isContainerorLot = ContainerOrLot.Data.ToString();
                var containerService = new WCF.Services.ContainerInfoInquiryService(session.CurrentUserProfile);
                var containerServiceData = new OM.ContainerInfoInquiry()
                {
                    Container = new OM.ContainerRef(servicedata.isContainerorLot.ToString())
                };

                var containerRequest = new WCF.Services.ContainerInfoInquiry_Request();
                var containerResult = new WCF.Services.ContainerInfoInquiry_Result();
                var containerResultStatus = new OM.ResultStatus();

                containerResultStatus = containerService.ContainerInfoInquiry_GetContainerInfo(containerServiceData, containerRequest, out containerResult);
                if (containerResultStatus != null && containerResultStatus.IsSuccess)
                {
                    isInContainerList = true;
                }

                servicedata.isInventoryLocation = new WCF.ObjectStack.NamedObjectRef(InventoryLocation.Data as string);
                serviceinfo.isQtyFromContainerorLot = FieldInfoUtil.RequestValue();
                serviceinfo.isContainerProduct = FieldInfoUtil.RequestValue();
                //serviceinfo.isContainerInventory = FieldInfoUtil.RequestValue();

                var oServiceRequest = new Camstar.WCF.Services.isManageMaterialQueue_Request();
                oServiceRequest.Info = serviceinfo;

                var resultStatus = new OM.ResultStatus();

                OM.ResultStatus oResultStatus = service.GetEnvironment(servicedata, oServiceRequest, out result);

                if (oResultStatus.IsSuccess)
                {
                    Qty.Enabled = !isInContainerList;
                    if (result.Value.isQtyFromContainerorLot != null)
                    {
                        Qty.Data = result.Value.isQtyFromContainerorLot.ToString();
                        QtyToValidate.Data = result.Value.isQtyFromContainerorLot.ToString();
                    }
                    if (result.Value.isContainerProduct != null)
                    {
                        ProductFromLoc.Data = result.Value.isContainerProduct;
                    }
                    ProductFromLoc.Enabled = result.Value.isContainerProduct == null;
                    /*if (result.Value.isContainerInventory != null)
                    {
                        InventoryLocation.Data = result.Value.isContainerInventory;
                    }*/
                }
            }
        }

        bool _RetrievedQty = false;
        private void ProductFromLoc_DataChanged(object sender, EventArgs e)
        {
            if (ContainerOrLot.Data != null || _RetrievedQty) 
                return;
            QtyToValidate.ClearData();
            Qty.ClearData();

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.isManageMaterialQueueService(session.CurrentUserProfile);
            var servicedata = new OM.isManageMaterialQueue();
            var serviceinfo = new OM.isManageMaterialQueue_Info();
            var result = new Camstar.WCF.Services.isManageMaterialQueue_Result();

            if (ProductFromLoc.Data != null)
            {
                servicedata.isProductFromInventoryLoc = ProductFromLoc.Data as OM.RevisionedObjectRef;
                servicedata.isInventoryLocation = InventoryLocation.Data as OM.NamedObjectRef;
            }
            else
                return;

            _RetrievedQty = true;
            serviceinfo.isQtyFromContainerorLot = FieldInfoUtil.RequestValue();
            var oServiceRequest = new Camstar.WCF.Services.isManageMaterialQueue_Request();
            oServiceRequest.Info = serviceinfo;

            var resultStatus = new OM.ResultStatus();

            OM.ResultStatus oResultStatus = service.GetEnvironment(servicedata, oServiceRequest, out result);

            if (oResultStatus.IsSuccess)
            {
                if (result.Value.isQtyFromContainerorLot != null)
                {
                    Qty.Data = result.Value.isQtyFromContainerorLot.ToString();
                    QtyToValidate.Data = result.Value.isQtyFromContainerorLot.ToString();
                }
            }
        }

        void MaterialQueue_DataChanged(object sender, EventArgs e)
        {
            _MaterialQueueChanging = true;
            _gridisMaterialQueueDetails.ClearData();
            _gridisMaterialQueueReplenishDetails.ClearData();
            Resource.ClearData();
            KitContainer.ClearData();
            KitMfgOrder.ClearData();
            KitOperation.ClearData();
            MfgOrder.ClearData();
            Product.ClearData();
            Qty.ClearData();
            ContainerOrLot.ClearData();
            InventoryLocation.ClearData();
            isKittingOrder.ClearData();
            isERP.ClearData();
            isManualConsumed.ClearData();

            //Get Resource and is active
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.isManageMaterialQueueService(session.CurrentUserProfile);
            var servicedata = new OM.isManageMaterialQueue();
            var serviceinfo = new OM.isManageMaterialQueue_Info();
            var result = new Camstar.WCF.Services.isManageMaterialQueue_Result();

            servicedata.isMaterialQueue = MaterialQueue.Data as OM.NamedObjectRef;

            serviceinfo.isResource = FieldInfoUtil.RequestValue();
            serviceinfo.isMaterialQueueActive = FieldInfoUtil.RequestValue();

            serviceinfo.isContainer = FieldInfoUtil.RequestValue();
            serviceinfo.isKittingMfgOrder = FieldInfoUtil.RequestValue();
            serviceinfo.isOperation = FieldInfoUtil.RequestValue();
            serviceinfo.isIsKit = FieldInfoUtil.RequestValue();

            serviceinfo.isMaterialQueueDetails = new OM.isMaterialQueueDetails_Info()
            {
                isContainer = new OM.Info(true),
                isLot = new OM.Info(true),
                isName = new OM.Info(true),
                isProduct = new OM.Info(true),
                isQty = new OM.Info(true),
                isUOM = new OM.Info(true),
                isSequence = new OM.Info(true),
                isConsumedQty = new OM.Info(true),
                isInventoryLocation = new OM.Info(true),
                isQtyAvailable = new OM.Info(true),
                isExpirationDate = new OM.Info(true),
                isRemovalStrategy = new OM.Info(true),
                isERP = new OM.Info(true),
                isManualConsumed = new OM.Info(true),
                isValidForIssue = new OM.Info(true)
            };

            serviceinfo.isMatQueueMatReplenishmentDtls = new OM.isMatQueueMatReplenishmentDtls_Info()
            {
                RequestSelectionValues = true

            };
            var oServiceRequest = new Camstar.WCF.Services.isManageMaterialQueue_Request();
            oServiceRequest.Info = serviceinfo;

            var resultStatus = new OM.ResultStatus();

            OM.ResultStatus oResultStatus = service.GetEnvironment(servicedata, oServiceRequest, out result);

            if (oResultStatus.IsSuccess)
            {
                if (result.Value.isMaterialQueueActive == true)
                {
                    isActive.CheckControl.Checked = true;
                }
                else
                {
                    isActive.CheckControl.Checked = false;
                }

                if (result.Value.isResource != null)
                {
                    Resource.Data = result.Value.isResource.ToString();
                }

                if (result.Value.isContainer != null)
                {
                    KitContainer.Data = result.Value.isContainer.Name;
                }

                if (result.Value.isKittingMfgOrder != null)
                {
                    KitMfgOrder.Data = result.Value.isKittingMfgOrder.ToString();
                }

                if (result.Value.isOperation != null)
                {
                    KitOperation.Data = result.Value.isOperation.ToString();
                }

                if (result.Value.isMaterialQueueDetails != null)
                {
                    Array oMaterialQueueDetailsArray = result.Value.isMaterialQueueDetails.ToArray();

                    (_gridisMaterialQueueDetails.GridContext as BoundContext).Data = oMaterialQueueDetailsArray;

                    _gridisMaterialQueueDetails.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(_gridisMaterialQueueDetails);
                }


                var selVal = result.Environment.isMatQueueMatReplenishmentDtls.SelectionValues;
                if (selVal != null && selVal.Rows != null)
                {

                    List<OM.isMatQueueMatReplenishmentDtls> details = new List<OM.isMatQueueMatReplenishmentDtls>();

                    OM.Header[] headers = selVal.Headers;
                    OM.Row[] rows = selVal.Rows;

                    foreach (OM.Row row in rows)
                    {
                        details.Add(new OM.isMatQueueMatReplenishmentDtls
                        {
                            isProduct = new OM.RevisionedObjectRef(row.Values[1],row.Values[9]),
                            isThresholdQty = Convert.ToDouble(row.Values[2]),
                            isUOM = new OM.NamedObjectRef(row.Values[3]),
                            isReplenishQty = Convert.ToDouble(row.Values[4]),
                            isManualReplenish = Convert.ToBoolean(row.Values[5])
                        });
                    }

                    (_gridisMaterialQueueReplenishDetails.GridContext as BoundContext).Data = details.ToArray();
                    _gridisMaterialQueueReplenishDetails.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(_gridisMaterialQueueReplenishDetails);
                }
            }
            _MaterialQueueChanging = false;
        }

        void FillFieldsOfMQ(object sender, EventArgs e)
        {
            if (!_MaterialQueueChanging)
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new Camstar.WCF.Services.isManageMaterialQueueService(session.CurrentUserProfile);
                var servicedata = new OM.isManageMaterialQueue();
                var serviceinfo = new OM.isManageMaterialQueue_Info();
                var result = new Camstar.WCF.Services.isManageMaterialQueue_Result();

                servicedata.isResource = Resource.Data as OM.NamedObjectRef;
                servicedata.isContainer = KitContainer.Data as OM.ContainerRef;
                servicedata.isKittingMfgOrder = KitMfgOrder.Data as OM.NamedObjectRef;
                servicedata.isOperation = KitOperation.Data as OM.NamedObjectRef;
                servicedata.isIsKit = isIsKit;

                serviceinfo.isMaterialQueueTmp = FieldInfoUtil.RequestValue();
                if (servicedata.isResource != null || servicedata.isContainer != null || servicedata.isKittingMfgOrder != null || servicedata.isOperation != null)
                {

                    var oServiceRequest = new Camstar.WCF.Services.isManageMaterialQueue_Request();
                    oServiceRequest.Info = serviceinfo;

                    var resultStatus = new OM.ResultStatus();

                    OM.ResultStatus oResultStatus = service.GetEnvironment(servicedata, oServiceRequest, out result);

                    if (oResultStatus.IsSuccess)
                    {
                        if (result.Value.isMaterialQueueTmp != null)
                        {
                            MaterialQueue.Data = result.Value.isMaterialQueueTmp.ToString();
                        }

                    }
                }
            }
        }
    }
}
