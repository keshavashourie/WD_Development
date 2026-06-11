// © 2018 Siemens Product Lifecycle Management Software Inc.

using System;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.Services;
using System.Web;
using Camstar.WebPortal.FormsFramework.WebGridControls;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{

    public class mdContainerMaintenance : MatrixWebPart
    {
        protected ContainerListGrid ContainerControl
        {
            get
            {
                ContainerListGrid containerControl = Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid;
                return containerControl;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Page.IsPostBack)
                ContainerControl.DataChanged += delegate { LoadDependentControls(); };
            else
                LoadDependentControls();
        }

        private void LoadDependentControls()
        {
            if (ContainerControl == null)
                throw new ApplicationException("The control is not found");
            ContainerMaint inputData = new ContainerMaint { Container = ContainerControl.Data as ContainerRef, ServiceDetail = new ContainerMaintDetail() };
            ClearValues();

            ContainerMaint_Info info = new ContainerMaint_Info
            {
                ServiceDetail = new ContainerMaintDetail_Info
                {
                    Level = FieldInfoUtil.RequestValue(),
                    Owner = FieldInfoUtil.RequestValue(),
                    StartReason = FieldInfoUtil.RequestValue(),
                    UOM = FieldInfoUtil.RequestValue(),
                    UOM2 = FieldInfoUtil.RequestValue(),
                    Product = FieldInfoUtil.RequestValue(),
                    MfgOrder = FieldInfoUtil.RequestValue(),
                    DueDate = FieldInfoUtil.RequestValue(),
                    Customer = FieldInfoUtil.RequestValue(),
                    SalesOrder = FieldInfoUtil.RequestValue(),
                    RequestDate = FieldInfoUtil.RequestValue(),
                    PlannedProduct = FieldInfoUtil.RequestValue(),
                    PlannedQty = FieldInfoUtil.RequestValue(),
                    PlannedQtyUOM = FieldInfoUtil.RequestValue(),
                    PlannedQty2 = FieldInfoUtil.RequestValue(),
                    BillOfProcess = FieldInfoUtil.RequestValue(),
                    ExpirationDate = FieldInfoUtil.RequestValue(),
                    PlannedQtyUOM2 = FieldInfoUtil.RequestValue(),
                    ContainerComments = FieldInfoUtil.RequestValue(),
                    Priority = FieldInfoUtil.RequestValue(),
                    DeviceIdentifier = FieldInfoUtil.RequestValue(),
                    UDI = FieldInfoUtil.RequestValue(),
                    ProductionIdentifier = FieldInfoUtil.RequestValue()

                },
                ChildProcessingMode = FieldInfoUtil.RequestValue(),
                Container = FieldInfoUtil.RequestValue(),
                CurrentContainerStatus = new CurrentContainerStatus_Info
                {
                    NextOperationName = FieldInfoUtil.RequestValue(),
                    Qty = FieldInfoUtil.RequestValue(),
                    Qty2 = FieldInfoUtil.RequestValue()
                }
            };
            UserProfile profile = HttpContext.Current.Session[Constants.SessionConstants.UserProfile] as UserProfile;
            ContainerMaintService serv = new ContainerMaintService(profile);
            ContainerMaint_Result result;
            ResultStatus resultStatus = serv.GetAttributes(inputData, new ContainerMaint_Request { Info = info }, out result);

            if (resultStatus.IsSuccess)
                Page.DisplayValues(result.Value);
        }

    }
}