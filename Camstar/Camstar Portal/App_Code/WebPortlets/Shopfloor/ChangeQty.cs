// Copyright Siemens 2025
using System;
using System.Web;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.Services;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class ChangeQty : MatrixWebPart
    {

        #region Properties
        protected CWC.InquiryControl ContainerStatus_Qty { get { return Page.FindCamstarControl("ContainerStatus_Qty") as CWC.InquiryControl; } }

        protected CWC.TextBox EnteredQty { get { return Page.FindCamstarControl("ChangeQty_Change") as CWC.TextBox; } }
        protected CWC.TextBox Qty { get { return Page.FindCamstarControl("ChangeQty_Qty") as CWC.TextBox; } }

        protected virtual ContainerListGrid ContainersGrid
        {
            get { return Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid; }
        } // ContainersGrid

        protected virtual CWC.NamedObject ReasonCodesField
        {
            get { return Page.FindCamstarControl("ChangeQty_Reason") as CWC.NamedObject; }
        } // ReasonCodesField

        protected virtual CWC.NamedSubentity ChargeToField
        {
            get { return Page.FindCamstarControl("ChangeQtyHiddenFieldsWebPart_Step") as CWC.NamedSubentity; }
        } // ChargeToField

        protected virtual CWC.CheckBox UseCurrentQtyField
        {
            get { return Page.FindCamstarControl("ChangeQty_RecordAllQty") as CWC.CheckBox; }
        } // UseCurrentQtyField

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ReasonCodesField.PickListPanelControl.DataProvider.BeforeDataLoad += (object sender, CWC.PickLists.DataLoadEventArgs args) =>
            {
                var changeTypeListValue = GetChangeTypeListValue();
                var data = args.ServiceData as OM.ChangeQty;

                if (!string.IsNullOrEmpty(changeTypeListValue) && data != null)
                {
                    string detailsTypeName = changeTypeListValue;
                    data.ServiceDetails = dataCreator.CreateArrayObject(detailsTypeName, 1) as ChangeQtyDetails[];
                    data.ServiceDetails[0].ListItemAction = ListItemAction.Add;
                    data.ServiceDetails[0].CDOTypeName = detailsTypeName;
                    data.Operation = GetContainerOperation();
                }
            };

            //Using for Quality
            var activeTab = Page.DataContract.GetValueByName("SelectedTabsForAction");
            if (IsFloatPage && activeTab != null && activeTab.Equals("Affected Material"))
            {
                var containerName = Page.DataContract.GetValueByName("SplitOrScrapContainer");
                ContainersGrid.Data = containerName;
            }
            if (IsFloatPage && activeTab != null && activeTab.Equals("Disposition"))
            {
                var containerName = Page.DataContract.GetValueByName("DispositionSplitOrScrapContainer");
                ContainersGrid.Data = containerName;
            }

            changeTypesList = Page.FindCamstarControl("ChangeQty_ChangeTypes") as CWC.RadioButtonList;
            if (changeTypesList != null)
            {
                changeTypesList.SelectionDependencies.Add(new FormsFramework.DependsOnItem(FormsFramework.DependsOnItemType.CamstarControl, "ContainerStatus_ContainerName", true));
                changeTypesList.ListControl.SelectedIndexChanged += changeTypesList_SelectedIndexChanged;
            }
            
            ContainersGrid.DataChanged += ContainersGrid_DataChanged;
            if ((Page.IsFloatingFrame && !Page.IsPostBack) || (!Page.IsPostBack && ContainersGrid.Data is OM.ContainerRef && !(ContainersGrid.Data as OM.ContainerRef).IsEmpty))
                ContainersGrid_DataChanged(ContainersGrid, new EventArgs());
            ReasonCodesField.DataChanged += resetSessionVariables;

            recordAllQty = Page.FindCamstarControl("ChangeQty_RecordAllQty") as CWC.CheckBox;
            qty = Page.FindCamstarControl("ChangeQty_Qty1") as CWC.TextBox;
            qty2 = Page.FindCamstarControl("ChangeQtyHiddenFieldsWebPart_Qty2") as CWC.TextBox;
            changeQty = Page.FindCamstarControl("ChangeQty_Change") as CWC.TextBox;
            changeQty2 = Page.FindCamstarControl("ChangeQtyHiddenFieldsWebPart_Change2") as CWC.TextBox;
            recordAllQty.CheckControl.Attributes.Add("onclick", "javascript: var changeQty=$('#" + changeQty.TextControl.ClientID + "'); var changeQty2=$('#" + changeQty2.TextControl.ClientID + "'); if($('#" + recordAllQty.CheckControl.ClientID +
                "').is(':checked')) { var qty=$('#" + qty.TextControl.ClientID + "').val(); var qty2=$('#" + qty2.TextControl.ClientID +
                "').val(); changeQty.val(qty); changeQty2.val(qty2); changeQty.attr('disabled','true'); changeQty2.attr('disabled','true');}else{changeQty.val('');changeQty.removeAttr('disabled'); changeQty2.val('');changeQty2.removeAttr('disabled');}");

            var ChangeQty_HiddenFieldsSection = Page.FindCamstarControl("ChangeQty_HiddenFieldsSection") as ToggleContainer;
            CamstarWebControl.SetRenderToClient(ChangeQty_HiddenFieldsSection);
        }

        protected virtual void resetSessionVariables(object sender, EventArgs e)
        {
            DisplayedChangeQtyWarning = false;
            CheckedChangeQty = false;
        }
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (DisplayedChangeQtyWarning)
                CheckedChangeQty = true;
        }
        /// <summary>
        /// Resets fields that don't have request dependencies so they get cleared/reset if the container field changes
        /// </summary>
        protected virtual void ResetFieldStates()
        {
            if (ChargeToField != null)
                ChargeToField.ClearData();

            if (UseCurrentQtyField != null)
                UseCurrentQtyField.ClearData();

            DisplayedChangeQtyWarning = false;
            CheckedChangeQty = false;
        }

        protected virtual void ContainersGrid_DataChanged(object sender, EventArgs e)
        {
            ResetFieldStates();

            if (ContainersGrid.Data != null)
            {
                ResultStatus res = Service.LoadDependentSelectionValues((sender as ContainerListGrid).ID);
                if (!res.IsSuccess)
                {
                    if (changeTypesList != null)
                        changeTypesList.Enabled = false;

                    //if the load of the selection values failed - clear the container and load the sel vals again to get the default disabled 
                    //list of all possible change qty types
                    string containerHolder = ContainersGrid.Data.ToString();
                    ContainersGrid.DataChanged -= new EventHandler(ContainersGrid_DataChanged);
                    ContainersGrid.ClearData();

                    //load the sel vals with an empty container to get the default list
                    Service.LoadDependentSelectionValues((sender as ContainerListGrid).ID);

                    //reset the container back so the user can see the container that caused the original sel val call to fail
                    ContainersGrid.Data = containerHolder;

                    //display the original failed message to the user
                    DisplayMessage(res);

                }
                else
                {
                    if (changeTypesList != null)
                    {
                        changeTypesList.Enabled = true;
                        if (changeTypesList.ListControl.Items.Count > 0)
                        {
                            changeTypesList.ListControl.SelectedIndex = 0;
                            changeTypesList_SelectedIndexChanged(changeTypesList.ListControl, new EventArgs());
                        }
                    }
                }
            } //if (ContainersGrid.Data != null)

            CamstarWebControl.SetRenderToClient(changeTypesList);
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

        protected virtual void changeTypesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Web.UI.WebControls.RadioButtonList list = sender as System.Web.UI.WebControls.RadioButtonList;
            if (list.SelectedItem != null)
            {
                RefreshControlsLabels(list.SelectedItem.Text);
                LoadReasonCodes(list.SelectedValue);
            }
        }

        protected virtual void RefreshControlsLabels(string typelistValue)
        {            
            if (reasonCodes == null)
                reasonCodes = Page.FindCamstarControl("ChangeQty_Reason") as CWC.NamedObject;
            if (changeQty == null)
                changeQty = Page.FindCamstarControl("ChangeQty_Change") as CWC.TextBox;
            if (changeQty2 == null)
                changeQty2 = Page.FindCamstarControl("ChangeQtyHiddenFieldsWebPart_Change2") as CWC.TextBox;
            if (labelCache == null)
                labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
            reasonCodes.LabelText = string.Format(labelCache.GetLabelByName("Lbl_ChangeQty_Reason").Value, typelistValue);
            changeQty.LabelText = string.Format(labelCache.GetLabelByName("Lbl_ChangeQty_Qty").Value, typelistValue);
            changeQty2.LabelText = string.Format(labelCache.GetLabelByName("Lbl_ChangeQty_Qty2").Value, typelistValue);

        }

        protected virtual void LoadReasonCodes(string detailsTypeName)
        {
            OM.ChangeQty data = new OM.ChangeQty();

            ChangeQtyDetails_Info detailInfo = new ChangeQtyDetails_Info() { ReasonCode = FieldInfoUtil.RequestSelectionValue() };
            ChangeQty_Request request = new ChangeQty_Request()
            {
                Info = new ChangeQty_Info() { ServiceDetails = detailInfo }
            };
            data.ServiceDetails = dataCreator.CreateArrayObject(detailsTypeName, 1) as ChangeQtyDetails[];
            data.ServiceDetails[0].ListItemAction = ListItemAction.Add;
            data.ServiceDetails[0].CDOTypeName = detailsTypeName;
            data.Operation = GetContainerOperation();

            if (data.Operation != null)
            {
                ChangeQtyService service = new ChangeQtyService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                ChangeQty_Result result = new ChangeQty_Result();
                OM.ResultStatus resultStatus = service.GetEnvironment(data, request, out result);
                if (resultStatus.IsSuccess)
                {
                    reasonCodes = Page.FindCamstarControl("ChangeQty_Reason") as CWC.NamedObject;
                    reasonCodes.ClearData();
                    reasonCodes.SetSelectionValues(result.Environment.ServiceDetails.ReasonCode.SelectionValues);
                }
                else
                {
                    DisplayMessage(resultStatus);
                    reasonCodes.ClearSelectionValues();
                }
            }
        }

        protected virtual NamedObjectRef GetContainerOperation()
        {
            ViewContainerStatus containerStatusData = new ViewContainerStatus();
            ViewContainerStatus_Info containerStatusInfo = new ViewContainerStatus_Info();

            containerStatusInfo.Operation = FieldInfoUtil.RequestValue();
            if (string.IsNullOrEmpty(ContainersGrid.TextEditControl.Text))
                return null;
            else
                containerStatusData.Container = new ContainerRef(ContainersGrid.TextEditControl.Text);
            ViewContainerStatusService service = new ViewContainerStatusService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            ViewContainerStatus_Result result;
            ViewContainerStatus_Request request = new ViewContainerStatus_Request() { Info = containerStatusInfo };
            OM.ResultStatus resultStatus = service.GetEnvironment(containerStatusData, request, out result);
            if (resultStatus.IsSuccess)
                return WSObjectRef.AssignNamedObject(result.Value.Operation.Value);
            else
            {
                DisplayMessage(resultStatus);
                return null;
            }
        }
        
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            var changeTypeValue = GetChangeTypeListValue();
            if (!string.IsNullOrEmpty(changeTypeValue))
            {
                OM.ChangeQty data = serviceData as OM.ChangeQty;
                if (data != null && data.ServiceDetails != null)
                {
                    foreach (var serviceDetail in data.ServiceDetails)
                    {
                        serviceDetail.CDOTypeName = changeTypeValue;
                        serviceDetail.ChangeQtyType = null;
                        if (serviceDetail.ChargeToStep != null)
                            serviceDetail.ChargeToStep.Parent = new BaseObjectRef(GetContainerWorkflow());
                    }
                }
            }
        }

        protected bool DisplayedChangeQtyWarning
        {
            get
            {
                if (HttpContext.Current.Session["DisplayedChangeQtyWarning"] != null)
                    return (bool)HttpContext.Current.Session["DisplayedChangeQtyWarning"];
                else
                    return false;
            }
            set
            {
                HttpContext.Current.Session["DisplayedChangeQtyWarning"] = value;
            }
        }

        protected bool CheckedChangeQty
        {
            get
            {
                if (HttpContext.Current.Session["CheckedChangeQty"] != null)
                    return (bool)HttpContext.Current.Session["CheckedChangeQty"];
                else
                    return false;
            }
            set
            {
                HttpContext.Current.Session["CheckedChangeQty"] = value;
            }
        }

        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                DisplayedChangeQtyWarning = false;
                CheckedChangeQty = false;
            }
        }
        public override ValidationStatus ValidateInputData(Service serviceData)
        {
            ValidationStatus status = base.ValidateInputData(serviceData);
            if (status.IsSuccess)
            {
                var changeTypeValue = GetChangeTypeListValue();
                if (!string.IsNullOrEmpty(changeTypeValue) && EnteredQty != null && EnteredQty.Data != null)
                {
                    double enteredQty = double.Parse(EnteredQty.Data.ToString());
                    Qty.Data = enteredQty.ToString();
                    /*if (!CheckedChangeQty)
                    {
                        bool useCurQty = UseCurrentQtyField.IsChecked;
                        double curQty = ContainerStatus_Qty != null && ContainerStatus_Qty.Data != null ? double.Parse(ContainerStatus_Qty.Data.ToString()) : -1;
                        if (string.Compare(changeTypeValue, "LossDetails", true) == 0 || string.Compare(changeTypeValue, "SellDetails", true) == 0)
                            enteredQty *= -1;
                        if (useCurQty || curQty + enteredQty <= 0)
                        {
                            RegularValidationStatusItem item = new RegularValidationStatusItem("Entered value will result in an empty container!");
                            status.Add(item);
                            DisplayedChangeQtyWarning = true;
                        }
                    }*/
                }
            }
            return status;
        }
        protected virtual string GetChangeTypeListValue()
        {
            string retVal = string.Empty;
            var changeTypesList = Page.FindCamstarControl("ChangeQty_ChangeTypes") as CWC.RadioButtonList;
            if (changeTypesList != null)
            {
                if (changeTypesList.ListControl.SelectedItem != null && !string.IsNullOrEmpty(changeTypesList.ListControl.SelectedItem.Text))
                    retVal = changeTypesList.ListControl.SelectedValue;
            }
            return retVal;
        }

        private CWC.RadioButtonList changeTypesList;
        protected CWC.NamedObject reasonCodes;

        private CWC.CheckBox recordAllQty;
        private CWC.TextBox changeQty;
        private CWC.TextBox changeQty2;
        private CWC.TextBox qty;
        private CWC.TextBox qty2;

        protected WSDataCreator dataCreator = new WSDataCreator();
        protected LabelCache labelCache;
    }
}
