// Copyright Siemens 2023
using System;
using System.Collections.Generic;
using System.Linq;
using Camstar.WebPortal.WebPortlets;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.Services;
using System.Web;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{

    public class ContainerMaintenance : MatrixWebPart
    {
        protected string LastContainer
        {
            get
            {
                return ViewState["LastContainer"] as string;
            }
            set
            {
                ViewState["LastContainer"] = value;
            }
        }

        protected virtual ContainerListGrid ContainerControl
        {
            get
            {
                ContainerListGrid containerControl = Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid;
                return containerControl;
            }
        }

        protected virtual DropDownList ChildProcessingControl
        {
            get { return Page.FindCamstarControl("ChildProcessingMode") as DropDownList; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Page.IsPostBack)
                ContainerControl.DataChanged += delegate { LoadDependentControls(); };
            else
                LoadDependentControls();
            this.Page.PreRenderComplete += delegate { FillDataContract(); };
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            if (action != null && action.Parameters == "Clear")
            {
                Page.ClearValues();
                LastContainer = string.Empty;
            }
        }

        protected virtual void LoadDependentControls()
        {
            if (ContainerControl == null)
                throw new ApplicationException("The control is not found");
            ContainerMaint inputData = new ContainerMaint { Container = ContainerControl.Data as ContainerRef, ServiceDetail = new ContainerMaintDetail() };
            if (ContainerControl.Data != null && !string.IsNullOrEmpty(ContainerControl.Data.ToString()) && string.Compare(LastContainer, ContainerControl.Data.ToString(), true) != 0)
            {
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
                        SamplingLot = FieldInfoUtil.RequestValue(),
                        SamplingRequired = FieldInfoUtil.RequestValue(),
                        VendorItem = FieldInfoUtil.RequestValue(),
                        ConsumingOrder = FieldInfoUtil.RequestValue(),
                        MfgPartNumber = FieldInfoUtil.RequestValue(),
                        ShapeName = FieldInfoUtil.RequestValue(),
                        Supplier = FieldInfoUtil.RequestValue(),
                        SupplyFromName = FieldInfoUtil.RequestValue(),
                        SupplyFromType = FieldInfoUtil.RequestValue(),
                        BinSize = FieldInfoUtil.RequestValue(),
                        DateCode = FieldInfoUtil.RequestValue(),
                        Description = FieldInfoUtil.RequestValue(),
                        Direction = FieldInfoUtil.RequestValue(),
                        NickName = FieldInfoUtil.RequestValue(),
                        LeadFree = FieldInfoUtil.RequestValue(),
                        IssueConditions = FieldInfoUtil.RequestValue(),
                        RemainingProcessTime = FieldInfoUtil.RequestValue(),
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
                    DisplayValues(result.Value);
                LastContainer = ContainerControl.Data.ToString();
            }
            else if (ContainerControl.Data == null || string.IsNullOrEmpty(ContainerControl.Data.ToString()))
            {
                LastContainer = string.Empty;
                Page.ClearValues();
            }
        }

        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                Page.ClearValues();
            }
        }
        protected virtual void FillDataContract()
        {
            if (IsFloatPage)
            {
                ContainerMaint data = new ContainerMaint();
                this.GetInputData(data);
                Page.DataContract.SetValueByName("ContainerMaintDetailDM", data.ServiceDetail);
            }
        }
    }
}
