//  Copyright Siemens 2023
using System;
using System.Linq;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class ChangeQtyMultiReason : MatrixWebPart
    {
        protected virtual CWC.RadioButtonList ChangeTypesList
        {
            get
            {
                return Page.FindCamstarControl("ChangeQty_ChangeTypes") as CWC.RadioButtonList;
            }
        }

        protected virtual JQDataGrid ReasonsGrid
        {
            get
            {
                return Page.FindCamstarControl("ReasonsGrid") as JQDataGrid;
            }
        }

        protected virtual CWC.NamedSubentity ChargeToStep
        {
            get { return Page.FindCamstarControl("ChargeToStep") as CWC.NamedSubentity; }
        } // ChargeToStep

        protected virtual ContainerListGrid ContainersGrid
        {
            get { return Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid; }
        } // ContainersGrid

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            CWC.RadioButtonList list = Page.FindCamstarControl("ChangeQty_ChangeTypes") as CWC.RadioButtonList;
            list.SelectionDependencies.Add(new FormsFramework.DependsOnItem(FormsFramework.DependsOnItemType.CamstarControl, "ContainerStatus_ContainerName", true));
            list.ListDisplayExpression = "CDODisplayName";
            list.ListValueColumn = "CDOName";

            var containerCtrl = Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid;
            containerCtrl.DataChanged += new EventHandler(containerCtrl_DataChanged);

            if ((Page.IsFloatingFrame && !Page.IsPostBack) || (!Page.IsPostBack && containerCtrl.Data is OM.ContainerRef && !(containerCtrl.Data as OM.ContainerRef).IsEmpty))
                containerCtrl_DataChanged(new Object(), new EventArgs());
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            CWC.RadioButtonList list = ChangeTypesList;
            if (ViewState[string.Format("{0}_selIndex", list.ListControl.ClientID)] != null && list.ListControl.Items.Count > 0)
                list.ListControl.SelectedIndex = Convert.ToInt32(ViewState[string.Format("{0}_selIndex", list.ListControl.ClientID)]);

            CWC.TextBox qtyTotal = Page.FindCamstarControl("QtyTotal") as CWC.TextBox;
            qtyTotal.TextControl.Text = this.Page.Request.Params.Get(qtyTotal.TextControl.UniqueID);

        }

        protected virtual void containerCtrl_DataChanged(object sender, EventArgs e)
        {
            CWC.RadioButtonList list = Page.FindCamstarControl("ChangeQty_ChangeTypes") as CWC.RadioButtonList;
            var containerCtrl = Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid;
            ResultStatus res = Service.LoadDependentSelectionValues(containerCtrl.ID);

            if (!res.IsSuccess)
            {
                if (list != null)
                    list.Enabled = false;
                if (ReasonsGrid != null)
                    ReasonsGrid.ClearSelectionValues();

                //if the load of the selection values failed - clear the container and load the sel vals again to get the default disabled 
                //list of all possible change qty types
                string containerHolder = containerCtrl.Data.ToString();
                containerCtrl.DataChanged -= new EventHandler(containerCtrl_DataChanged);
                containerCtrl.ClearData();

                //load the sel vals with an empty container to get the default list
                Service.LoadDependentSelectionValues((sender as ContainerListGrid).ID);

                //reset the container back so the user can see the container that caused the original sel val call to fail
                containerCtrl.Data = containerHolder;

                //display the original failed message to the user
                DisplayMessage(res);
            }
            else
            {
                if (list != null)
                {
                    list.Enabled = true;

                    if (list.ListControl.Items.Count > 0)
                    {
                        list.ListControl.SelectedIndex = 0;
                        ChangeQtyType_SelectedIndexChanged(new Object(), new EventArgs());
                    }
                }

            } // if (!res.IsSuccess)

        }

        public virtual void ChangeQtyType_SelectedIndexChanged(object sender, EventArgs e)
        {
            Camstar.WebPortal.FormsFramework.Utilities.FrameworkSession fs = Camstar.WebPortal.FormsFramework.Utilities.FrameworkManagerUtil.GetFrameworkSession();

            CWC.RadioButtonList list = Page.FindCamstarControl("ChangeQty_ChangeTypes") as CWC.RadioButtonList;
            ContainerListGrid contGrid = Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid;

            PopulateLabels();

            OM.ChangeQty data = new OM.ChangeQty() { Container = contGrid.Data as OM.ContainerRef };

            ChangeQty_GetChangeQtyDetails_Parameters param = new ChangeQty_GetChangeQtyDetails_Parameters();
            param.ChangeQtyType = list.ListControl.SelectedItem.Value;

            ChangeQty_Request request = new ChangeQty_Request()
                            {
                                Info = new OM.ChangeQty_Info()
                                        {
                                            ServiceDetails = new OM.ChangeQtyDetails_Info() { RequestValue = true }
                                        }
                            };

            ChangeQty_Result result = null;
            ChangeQtyService service = new ChangeQtyService(fs.CurrentUserProfile);
            OM.ResultStatus res = service.GetChangeQtyDetails(data, param, request, out result);

            if (res.IsSuccess)
            {
                JQDataGrid grid = Page.FindCamstarControl("ReasonsGrid") as JQDataGrid;
                grid.Data = result.Value.ServiceDetails;

                int reasonCodeIndex = (grid.GridContext as SubentityDataContext).Fields.IndexOf((grid.GridContext as SubentityDataContext).Fields.Where(n => n.ID == "ReasonCode").FirstOrDefault());
                int reasonQtyIndex = (grid.GridContext as SubentityDataContext).Fields.IndexOf((grid.GridContext as SubentityDataContext).Fields.Where(n => n.ID == "Qty").FirstOrDefault());

                (grid.GridContext as SubentityDataContext).Fields[reasonCodeIndex].LabelName = list.ListControl.SelectedItem.Value + "_ReasonCode";
                (grid.GridContext as SubentityDataContext).Fields[reasonQtyIndex].LabelName = list.ListControl.SelectedItem.Value + "_Qty";
            }

            ViewState[string.Format("{0}_selIndex", list.ListControl.ClientID)] = (list.ListControl as System.Web.UI.WebControls.RadioButtonList).SelectedIndex;
        }

        protected virtual string GetContainerWorkflow()
        {
            OM.ViewContainer containerStatusData = new OM.ViewContainer();
            OM.ViewContainer_Info containerStatusInfo = new OM.ViewContainer_Info
            {
                ContainerStatusDetails = new CurrentContainerStatus_Info()
            };
            containerStatusInfo.ContainerStatusDetails.Workflow = FieldInfoUtil.RequestValue();
            containerStatusData.ContainerStatusDetails = new CurrentContainerStatus();

            if (ContainersGrid != null)
                containerStatusData.Container = ContainersGrid.Data as ContainerRef;

            ViewContainer_Request request = new ViewContainer_Request
            {
                Info = containerStatusInfo
            };
            ViewContainer_Result result = new ViewContainer_Result();
            ViewContainerService service = new ViewContainerService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);

            OM.ResultStatus resultStatus = service.GetEnvironment(containerStatusData, request, out result);
            if (resultStatus.IsSuccess)
            {
                return result.Value.ContainerStatusDetails.Workflow.ID;
            }
            else
                DisplayMessage(resultStatus);
            return null;
        }

        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);

            if (serviceData != null && (serviceData as OM.ChangeQty) != null)
            {
                CWC.RadioButtonList list = Page.FindCamstarControl("ChangeQty_ChangeTypes") as CWC.RadioButtonList;

                OM.NamedSubentityRef chargeToStep = null;
                chargeToStep = (OM.NamedSubentityRef)ChargeToStep.Data;
                if (chargeToStep != null)
                {
                    chargeToStep.Parent = new BaseObjectRef(GetContainerWorkflow());
                }

                JQDataGrid grid = Page.FindCamstarControl("ReasonsGrid") as JQDataGrid;
                if (grid != null && grid.Data != null)
                {
                    try
                    {
                        var items = (grid.Data as OM.ChangeQtyDetails[]).Select(i =>
                                 new OM.ChangeQtyDetails()
                                 {
                                     ListItemAction = OM.ListItemAction.Add,
                                     CDOTypeName = list.ListControl.SelectedItem.Value,
                                     ReasonCode = i.ReasonCode,
                                     EnteredQty = i.EnteredQty,
                                     Comments = i.Comments,
                                     ChargeToStep = chargeToStep
                                 }).Where(n => n.EnteredQty != null);

                        System.Collections.Generic.List<ChangeQtyDetails> details = new System.Collections.Generic.List<ChangeQtyDetails>();
                        foreach (ChangeQtyDetails item in items)
                        {
                            if (item.EnteredQty != null && !string.IsNullOrEmpty(item.EnteredQty.Value) && item.EnteredQty.Value != "0")
                                details.Add(item);
                        }

                        (serviceData as OM.ChangeQty).ServiceDetails = items.Where(item => !((item.Comments == null || string.IsNullOrEmpty(item.Comments.Value)) && (item.EnteredQty == null || string.IsNullOrEmpty(item.EnteredQty.Value) || item.EnteredQty.Value == "0"))).ToArray();
                    }
                    catch (Exception ex)
                    {
                        string err = ex.ToString();
                    }
                }
            }
        }

        public virtual void PopulateLabels()
        {
            CWC.RadioButtonList list = Page.FindCamstarControl("ChangeQty_ChangeTypes") as CWC.RadioButtonList;
            CWC.TextBox qtyTotal = Page.FindCamstarControl("QtyTotal") as CWC.TextBox;

            string labelName = (list.Data as string ?? string.Empty) + "QtyTotal";
            CWC.Label label = Page.FindCamstarControl(labelName) as CWC.Label;
            if (label != null)
                qtyTotal.LabelControl.Text = label.Text;
        }

    }
}
