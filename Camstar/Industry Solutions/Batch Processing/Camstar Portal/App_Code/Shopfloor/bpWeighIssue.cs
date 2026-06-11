// © 2025 Siemens Product Lifecycle Management Software Inc.
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using System.Collections.Generic;

using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CamstarPortal.WebControls;

using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class bpWeighIssue : MatrixWebPart
    {
        public bpWeighIssue() { }

        #region ClientScript Section

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/user/BatchProcessing/scale.js");
        }

        #endregion

        #region Protected properties

        protected string DecimalSeparator
        {
            get
            {
                string decimalSeparator = Camstar.WebPortal.Utilities.CamstarPortalSection.Settings.CurrentCultureSettings.NumberFormat.NumberDecimalSeparator;
                if (string.IsNullOrEmpty(decimalSeparator)) decimalSeparator = ".";

                return decimalSeparator;
            }
        }

        protected virtual OM.IssueDetails[] MaterialRequirements
        {
            get
            {
                OM.IssueDetails[] reqs = Page.DataContract.GetValueByName<OM.IssueDetails[]>(mkServiceDetailsStorageKey);
                return reqs ?? new OM.IssueDetails[0];
            }
            set
            {
                Page.DataContract.SetValueByName(mkServiceDetailsStorageKey, value);
            }
        }

        protected virtual CWC.ContainerList DCContainer
        {
            get { return Page.FindCamstarControl("DCContainer") as CWC.ContainerList; }
        } // DataColection

        protected virtual CWC.RevisionedObject DataColection
        {
            get { return Page.FindCamstarControl("DCCollectionDef") as CWC.RevisionedObject; }
        } // DataColection

        protected virtual CWC.TextBox Product
        {
            get { return Page.FindCamstarControl("ComponentIssueUDA_Product") as CWC.TextBox; }
        } // Product

        protected virtual CWC.TextBox ProductDescription
        {
            get { return Page.FindCamstarControl("ComponentIssueUDA_ProductDescription") as CWC.TextBox; }
        } // ProductDescription

        protected virtual CWC.DropDownList IssueControl
        {
            get { return Page.FindCamstarControl("ComponentIssueUDA_IssueControl") as CWC.DropDownList; }
        } // IssueControl

        protected virtual CWC.TextBox NetQtyRequired
        {
            get { return Page.FindCamstarControl("ComponentIssueUDA_NetQtyRequired") as CWC.TextBox; }
        } // NetQtyRequired

        protected virtual CWC.TextBox ScanContainer
        {
            get { return FindCamstarControl("ComponentIssueUDA_ScanContainer") as CWC.TextBox; }
        } // ScanContainer

        protected virtual ContainerListGrid ContainerName
        {
            get { return Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid; }
        } // ContainerName

        protected virtual CWC.TextBox MaterialsRequirementContainerName
        {
            get { return FindCamstarControl("MaterialRequirements_ContainerName") as CWC.TextBox; }
        } // MaterialsRequirementContainerName

        protected virtual CWC.TextBox ContainerQty
        {
            get { return FindCamstarControl("ComponentIssueUDA_ContainerQty") as CWC.TextBox; }
        } // ContainerQty

        protected virtual object ContainerQty2
        {
            get { return ViewState[mkContainerQty2]; }
            set { ViewState.Add(mkContainerQty2, value); }
        } // ContainerQty2

        protected virtual CWC.TextBox UOM
        {
            get { return Page.FindCamstarControl("ComponentIssueUDA_UOM") as CWC.TextBox; }
        } // UOM

        protected virtual CWC.TextBox UOM2
        {
            get { return Page.FindCamstarControl("AdditionalFields_UOM2") as CWC.TextBox; }
        } // UOM2

        protected virtual CWC.TextBox IssueQty
        {
            get { return Page.FindCamstarControl("ServiceDetails_IssueQty") as CWC.TextBox; }
        } // IssueQty

        protected virtual CWC.TextBox IssueQty2
        {
            get { return Page.FindCamstarControl("AdditionalFields_Qty2Issued") as CWC.TextBox; }
        } // IssueQty2

        protected virtual CWC.TextBox LotNumber
        {
            get { return Page.FindCamstarControl("ComponentIssueUDA_LotNumber") as CWC.TextBox; }
        } // LotNumber

        protected virtual CWC.TextBox StockPoint
        {
            get { return Page.FindCamstarControl("ActualsStock_FromStockPoint") as CWC.TextBox; }
        } // StockPoint

        protected virtual CWC.NamedObject IssueDifferenceReason
        {
            get { return Page.FindCamstarControl("ServiceDetails_IssueDifferenceReason") as CWC.NamedObject; }
        } // IssueDifferenceReason

        protected virtual CWC.NamedObject IssueReason
        {
            get { return Page.FindCamstarControl("AdditionalFields_IssueReason") as CWC.NamedObject; }
        } // IssueReason

        protected virtual CWC.NamedObject SubstitutionReason
        {
            get { return Page.FindCamstarControl("AdditionalFields_SubstitutionReason") as CWC.NamedObject; }
        } // SubstitutionReason

        protected virtual CWC.TextBox Comments
        {
            get { return Page.FindCamstarControl("AdditionalFields_Comments") as CWC.TextBox; }
        } // Comments

        protected virtual CWC.NamedObject bpScale
        {
            get { return Page.FindCamstarControl("IssueActualDetails_bpScale") as CWC.NamedObject; }
        } // bpScale

        protected virtual CWC.RevisionedObject IssueActualProduct
        {
            get { return Page.FindCamstarControl("IssueActualDetails_Product") as CWC.RevisionedObject; }
        }  //IssueActualProduct

        protected virtual CWC.NamedObject bpScaleGroup
        {
            get { return Page.FindCamstarControl("IssueActualDetails_bpScaleGroup") as CWC.NamedObject; }
        } // bpScaleGroup

        protected virtual CWC.Button CaptureWeightButton
        {
            get { return Page.FindCamstarControl("btnCaptureWeight") as CWC.Button; }
        }

        protected virtual CWC.Button TareButton
        {
            get { return Page.FindCamstarControl("btnTare") as CWC.Button; }
        }

        protected virtual CWC.TitleControl ScaleInfoTitle
        {
            get { return Page.FindCamstarControl("ScaleInfoTitleControl") as CWC.TitleControl; }
        }

        protected virtual CWC.InquiryControl ProductInquiry
        {
            get { return Page.FindCamstarControl("IssueDetails_Product") as CWC.InquiryControl; }
        } // ProductInquiry
        protected virtual CWC.InquiryControl ProductDescriptionInquiry
        {
            get { return Page.FindCamstarControl("IssueDetails_ProductDescription") as CWC.InquiryControl; }
        } // ProductDescriptionInquiry

        protected virtual CWC.InquiryControl UOMInquiry
        {
            get { return Page.FindCamstarControl("IssueDetails_UOM") as CWC.InquiryControl; }
        } // UOMInquiry

        protected virtual CWC.InquiryControl MaxQtyInquiry
        {
            get { return Page.FindCamstarControl("IssueDetails_MaxQty") as CWC.InquiryControl; }
        } // MaxQtyInquiry

        protected virtual CWC.InquiryControl TargetQtyInquiry
        {
            get { return Page.FindCamstarControl("IssueDetails_TargetQty") as CWC.InquiryControl; }
        } // TargetQtyInquiry

        protected virtual CWC.InquiryControl MinQtyInquiry
        {
            get { return Page.FindCamstarControl("IssueDetails_MinQty") as CWC.InquiryControl; }
        } // MinQtyInquiry

        protected virtual CWC.InquiryControl RemainingQtyInquiry
        {
            get { return Page.FindCamstarControl("IssueDetails_RemainingQty") as CWC.InquiryControl; }
        } // RemainingQtyInquiry

        protected virtual CWC.InquiryControl TotalQtyInquiry
        {
            get { return Page.FindCamstarControl("IssueDetails_TotalQty") as CWC.InquiryControl; }
        } // TotalQtyInquiry

        protected virtual CWC.InquiryControl TareWeightInquiry
        {
            get { return Page.FindCamstarControl("IssueDetails_TareWeight") as CWC.InquiryControl; }
        } // TareWeightInquiry

        protected virtual CWC.NamedSubentity BOMLineItemInquiry
        {
            get { return Page.FindCamstarControl("IssueDetails_BOMLineItem") as CWC.NamedSubentity; }
        } // BOMLineItemInquiry

        protected virtual CWC.CheckBox ManualOverrideField
        {
            get { return Page.FindCamstarControl("ServiceDetails_bpManualWeightOverride") as CWC.CheckBox; }
        } // ManualOverrideField

        protected virtual CWC.CheckBox ToleranceOverrideField
        {
            get { return Page.FindCamstarControl("ServiceDetails_bpOverrideTolerances") as CWC.CheckBox; }
        } // ToleranceOverrideField

        protected virtual CWC.CheckBox ExpressionDependsOnFromContainer
        {
            get { return Page.FindCamstarControl("ServiceDetails_bpExDependsOnFromContainer") as CWC.CheckBox; }
        } //ExpressionDependsOnFromContainer

        protected virtual CWC.CheckBox IsManualReadingOnlyField
        {
            get { return Page.FindCamstarControl("ServiceDetails_IsManualReadingOnly") as CWC.CheckBox; }
        } //IsManualReadingOnlyField


        /// <summary>
        /// EProcedure specific hidden controls
        /// </summary>
        /// 
        protected virtual CWC.TextBox HiddenTare
        {
            get { return Page.FindCamstarControl("HiddenTare") as CWC.TextBox; }
        } // HiddenTare

        protected virtual ContainerListGrid EProcTaskContainer
        {
            get { return Page.FindCamstarControl("ShopFloor_TaskContainer") as ContainerListGrid; }
        } // EProcTaskContainer

        protected virtual CWC.RevisionedObject EProcTaskList
        {
            get { return Page.FindCamstarControl("ExecuteTask_TaskList") as CWC.RevisionedObject; }
        } // EProcTaskList

        protected virtual CWC.NamedSubentity EProcTask
        {
            get { return Page.FindCamstarControl("ShopFloor_CalledByTransactionTask") as CWC.NamedSubentity; }
        } // EProcTask

        protected virtual CWC.NamedObject Vendor
        {
            get { return Page.FindCamstarControl("Vendor") as CWC.NamedObject; }
        }//Vendor

        protected virtual CWC.NamedSubentity VendorItem
        {
            get { return Page.FindCamstarControl("VendorItem") as CWC.NamedSubentity; }
        }//VendorItem

        protected virtual OM.ComponentIssueInquiry ExecuteData
        {
            get
            {
                return ViewState[mkComponentIssueExecuteData] as OM.ComponentIssueInquiry;
            }
            set
            {
                ViewState[mkComponentIssueExecuteData] = value;
            }
        } // ExecuteData

        protected virtual IssueDetailsTypeEnum IssueDetailsType
        {
            get
            {
                var issueDetails = (ExecuteData != null && ExecuteData.IssueDetails != null) ? ExecuteData.IssueDetails : null;
                var type = IssueDetailsTypeEnum.Nothing;

                if (issueDetails != null && issueDetails is OM.IssueDetails)
                {
                    if (issueDetails is OM.IssueDetailsBulk)
                        type = IssueDetailsTypeEnum.Lot;
                    else if (issueDetails is OM.IssueDetailsSerial)
                        type = IssueDetailsTypeEnum.Serial;
                    else if (issueDetails is OM.IssueDetailsLotStock)
                        type = IssueDetailsTypeEnum.LotAndStockpoint;
                    else if (issueDetails is OM.IssueDetailsStock)
                        type = IssueDetailsTypeEnum.Stockpoint;
                    else if (issueDetails is OM.IssueDetailsQuantity)
                        type = IssueDetailsTypeEnum.Qty;
                    else if (issueDetails is OM.IssueDetailsDisplayOnly)
                        type = IssueDetailsTypeEnum.DisplayOnly;
                }

                return type;
            }
        } // IssueDetailsType

        protected virtual CWC.Scales bpScaleInfo
        {
            get { return Page.FindCamstarControl("bpScaleDetail") as CWC.Scales; }
        }

        #endregion

        #region Public methods


        public virtual void ScanFieldChanged(object sender, EventArgs e)
        {
            var objectName = ScanContainer.Data != null ? ScanContainer.Data.ToString() : string.Empty;

            if (!string.IsNullOrEmpty(objectName))
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new Camstar.WCF.Services.ComponentIssueInquiryService(session.CurrentUserProfile);
                var serviceData = new OM.ComponentIssueInquiry
                {
                    BOMLineItem = (OM.NamedSubentityRef)BOMLineItemInquiry.Data,
                    ParentContainer = (OM.ContainerRef)ContainerName.Data,
                    ObjectName = objectName
                };
                serviceData.CalledByTransactionTask = (OM.NamedSubentityRef)EProcTask.Data;
                serviceData.CalledByTransactionTask.Parent = (OM.BaseObjectRef)EProcTaskList.Data;
                var serviceInfo = new OM.ComponentIssueInquiry_Info
                {
                    IssueDetails = new OM.IssueDetails_Info
                    {
                        RequestValue = true
                    },
                    Container = new OM.Info(true),
                    bpFromContainer = new OM.Info(true),
                    Product = new OM.Info(true),
                    Qty = new OM.Info(true),
                    UOM = new OM.Info(true),
                    Qty2 = new OM.Info(true),
                    UOM2 = new OM.Info(true)
                };

                var request = new Camstar.WCF.Services.ComponentIssueInquiry_Request();
                request.Info = serviceInfo;

                var result = new Camstar.WCF.Services.ComponentIssueInquiry_Result();
                var resultStatus = new OM.ResultStatus();

                resultStatus = service.ExecuteTransaction(serviceData, request, out result);

                if (resultStatus.IsSuccess)
                {
                    ExecuteData = result.Value;
                    if (!result.IsEmpty)
                    {
                        if (result.Value.Qty != null)
                            ContainerQty.Data = result.Value.Qty;
                        if (result.Value.UOM != null)
                            UOM.Data = result.Value.UOM;
                        if (result.Value.UOM2 != null)
                            UOM2.Data = result.Value.UOM2;
                        if (result.Value.Qty2 != null)
                            ContainerQty2 = result.Value.Qty2;
                        if (result.Value.IssueDetails != null)
                        {
                            var DecimalScale = result.Value.IssueDetails.DecimalScale;
                            OM.RoundingRuleEnum RoundingRule = OM.RoundingRuleEnum.RoundToNearest;
                            if (result.Value.IssueDetails != null && result.Value.IssueDetails.RoundingRule != null)
                            {
                                RoundingRule = (OM.RoundingRuleEnum)result.Value.IssueDetails.RoundingRule;
                            }
                            Page.SessionVariables.SetValueByName("DecimalScale", DecimalScale);
                            Page.SessionVariables.SetValueByName("RoundingRule", RoundingRule);
                            if (DecimalScale != null)
                            {

                                MaxQtyInquiry.Data = AddDecimalPart(result.Value.IssueDetails.QtyUpperLimit, DecimalScale, RoundingRule);
                                TargetQtyInquiry.Data = AddDecimalPart(result.Value.IssueDetails.QtyRequired, DecimalScale, RoundingRule);
                                MinQtyInquiry.Data = AddDecimalPart(result.Value.IssueDetails.QtyLowerLimit, DecimalScale, RoundingRule);
                                RemainingQtyInquiry.Data = AddDecimalPart(result.Value.IssueDetails.NetQtyRequired, DecimalScale, RoundingRule);
                                TotalQtyInquiry.Data = result.Value.IssueDetails.QtyIssued;
                            }
                            else
                            {
                                MaxQtyInquiry.Data = result.Value.IssueDetails.QtyUpperLimit;
                                TargetQtyInquiry.Data = result.Value.IssueDetails.QtyRequired;
                                MinQtyInquiry.Data = result.Value.IssueDetails.QtyLowerLimit;
                                RemainingQtyInquiry.Data = result.Value.IssueDetails.NetQtyRequired;
                                TotalQtyInquiry.Data = result.Value.IssueDetails.QtyIssued;
                            }
                            //var info = result.Value.IssueDetails.DecimalScale;
                        }

                        if (IssueDetailsType == IssueDetailsTypeEnum.Serial)
                            IssueQty.Data = ContainerQty.Data;
                    }
                }

                if (e == null)
                    e = new CustomActionEventArgs();

                if (e is CustomActionEventArgs)
                    (e as CustomActionEventArgs).Result = resultStatus;

                if (!resultStatus.IsSuccess)
                    Page.DisplayMessage(resultStatus);
            }
        } // ScanFieldChanged(object sender, EventArgs e)          

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "Clear":
                        {
                            Page.ShopfloorReset(sender, e as CustomActionEventArgs);
                            //ReloadMaterialsRequirementGrid();
                            break;
                        }
                    case "IssueComponent":
                        {
                            IssueComponentAction(e);
                            break;
                        }
                    case "Close":
                        {
                            Page.CloseFloatingFrame(false);
                            break;
                        }
                }
            }
        } // WebPartCustomAction(object sender, CustomActionEventArgs e)

        #endregion

        #region Protected methods
        protected override void OnLoad(EventArgs e)
        {

            base.OnLoad(e);
            //if (!Page.IsPostBack)
            //SetSatisfiedWidgetCountControl(null);
            //Page.Header.Controls.Add(new LiteralControl("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + ResolveUrl("~/Themes/User/BatchProcessing/bpWeighIssue.css") + "\" />"));

            if (Page.IsFloatingFrame && !Page.IsPostBack)
                ReloadMaterialsRequirementGrid();

            bpScale.DataChanged += bpScale_DataChanged;
            TareButton.Click += new EventHandler(TareButton_Click);
            Page.OnRequestDependentESigValues += Page_OnRequestDependentESigValues;
            SetUserDataEntryLayout(IssueDetailsType);


            if (Page.IsPostBack && bpScale.Data != null)
            {
                SetScaleInfoLayout();
            }
            else if (Page.IsPostBack && bpScale.Data == null)
            {
                //Initialize ScaleInfo

                bpScaleInfo.ScaleMaxValue = 0;
                bpScaleInfo.ScaleMinValue = 0;
                bpScaleInfo.TargetMaximumWeightValue = 0;
                bpScaleInfo.TargetMinimumWeightValue = 0;
                bpScaleInfo.TargetWeightValue = 0;

            }
        } // OnLoad(EventArgs e)

        void SetScaleInfoLayout()
        {
            if (bpScale.Data != null)
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var SVC = new Camstar.WCF.Services.bpScaleMaintService(session.CurrentUserProfile);
                var oserviceData = new OM.bpScaleMaint();

                var ReqData = new Camstar.WCF.Services.bpScaleMaint_Request();
                var ResData = new Camstar.WCF.Services.bpScaleMaint_Result();
                var oStatus = new OM.ResultStatus();


                oserviceData.ObjectToChange = bpScale.Data as OM.NamedObjectRef;

                ReqData.Info = new OM.bpScaleMaint_Info
                {
                    ObjectChanges = new OM.bpScaleChanges_Info
                    {
                        MaximumCapacity = new OM.Info(true),
                        UOM = new OM.Info(true),
                        MinimumCapacity = new OM.Info(true)

                    }
                };

                oStatus = SVC.Load(oserviceData, ReqData, out ResData);
                if (oStatus != null)
                {
                    if (!oStatus.IsSuccess)
                        Page.DisplayMessage(oStatus);
                    else
                    {
                        bpScaleInfo.ScaleMaxValue = ResData.Value.ObjectChanges.MaximumCapacity;
                        bpScaleInfo.ScaleMinValue = ResData.Value.ObjectChanges.MinimumCapacity;
                        bpScaleInfo.ActualWeightUOM = ResData.Value.ObjectChanges.UOM != null ? ResData.Value.ObjectChanges.UOM.ToString() : null;

                        if (TargetQtyInquiry.Data == null)
                        {
                            bpScaleInfo.ScaleMaxValue = 0;
                            bpScaleInfo.ScaleMinValue = 0;
                            bpScaleInfo.TargetMaximumWeightValue = 0;
                            bpScaleInfo.TargetMinimumWeightValue = 0;
                            bpScaleInfo.TargetWeightValue = 0;
                            bpScaleInfo.ZoomPercentage = 20;
                        }
                        else
                        {
                            if (bpScaleInfo.ScaleMaxValue == null && MaxQtyInquiry.Data == null)
                                bpScaleInfo.ScaleMaxValue = (double.Parse(TargetQtyInquiry.Data.ToString()) + (double.Parse(TargetQtyInquiry.Data.ToString()) * 0.1)).ToString();

                            if (bpScaleInfo.ScaleMaxValue != null && MaxQtyInquiry.Data != null)
                                if (double.Parse(bpScaleInfo.ScaleMaxValue.ToString()) < double.Parse(MaxQtyInquiry.Data.ToString()))
                                    DisplayMessage(new OM.ResultStatus(ScaleMaxCapErrorMsg, false));

                            if (bpScaleInfo.ScaleMinValue != null && MinQtyInquiry.Data != null)
                                if (double.Parse(bpScaleInfo.ScaleMinValue.ToString()) > double.Parse(MinQtyInquiry.Data.ToString()))
                                {
                                    bpScaleInfo.ScaleMinValue = MinQtyInquiry.Data.ToString();
                                    DisplayMessage(new OM.ResultStatus(ScaleMinCapErrorMsg, false));
                                }

                            //TargetQtyInquiryMaxQtyInquiryMinQtyInquiry
                            bpScaleInfo.TargetMaximumWeightValue = MaxQtyInquiry.Data;
                            bpScaleInfo.TargetMinimumWeightValue = MinQtyInquiry.Data;
                            bpScaleInfo.TargetWeightValue = TargetQtyInquiry.Data;
                            bpScaleInfo.ZoomPercentage = 20;
                            if (Page.SessionVariables.GetValueByName("DecimalScale") != null)
                            {
                                if (int.TryParse(Page.SessionVariables.GetValueByName("DecimalScale").ToString(), out int scaleValue))
                                {
                                    bpScaleInfo.DecimalScale = scaleValue;

                                    if (Page.SessionVariables.GetValueByName("RoundingRule") != null)
                                    {
                                        bpScaleInfo.RoundingRule = (OM.RoundingRuleEnum)Page.SessionVariables.GetValueByName("RoundingRule");
                                    }
                                }

                            }
                            else
                            {
                                bpScaleInfo.DecimalScale = null;
                            }

                        }
                    }

                }
            }
            else
            {
                //Initialize ScaleInfo
                bpScaleInfo.ScaleMaxValue = 0;
                bpScaleInfo.ScaleMinValue = 0;
                bpScaleInfo.TargetMaximumWeightValue = 0;
                bpScaleInfo.TargetMinimumWeightValue = 0;
                bpScaleInfo.TargetWeightValue = 0;

            }

        }

        void bpScale_DataChanged(object sender, EventArgs e)
        {
            if (bpScale.Data != null)
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new Camstar.WCF.Services.ComponentIssueService(session.CurrentUserProfile);
                var servicedata = new OM.ComponentIssue();

                servicedata.Container = new OM.ContainerRef(ContainerName.Data.ToString());
                servicedata.IssueActualDetails = new OM.IssueActualDetail[1];
                servicedata.IssueActualDetails[0] = new OM.IssueActualDetail();
                servicedata.IssueActualDetails[0].bpScale = new OM.NamedObjectRef(bpScale.Data.ToString());

                var request = new Camstar.WCF.Services.ComponentIssue_Request();
                var result = new Camstar.WCF.Services.ComponentIssue_Result();
                var resultStatus = new OM.ResultStatus();

                request.Info = new OM.ComponentIssue_Info
                {
                    RequestValue = true,
                    IssueActualDetails = new OM.IssueActualDetail_Info
                    {
                        RequestValue = true,
                        bpSignalRURL = new OM.Info
                        {
                            RequestValue = true
                        },
                        bpMIOChannelAdapter = new OM.Info
                        {
                            RequestValue = true
                        }
                    }
                };

                resultStatus = service.Load(servicedata, request, out result);

                if (result != null && result.Value != null && result.Value.IssueActualDetails != null != null && result.Value.IssueActualDetails[0].bpSignalRURL != null && result.Value.IssueActualDetails[0].bpMIOChannelAdapter != null)
                {
                    string startupScript = string.Format("scaleConnect('{0}','{1}','{2}', '{3}');", result.Value.IssueActualDetails[0].bpSignalRURL, bpScale.Data.ToString() + ContainerName.Data.ToString(), result.Value.IssueActualDetails[0].bpMIOChannelAdapter.ToString(), DecimalSeparator);
                    if (!Page.ClientScript.IsStartupScriptRegistered("ScaleConnectScript"))
                        ScriptManager.RegisterStartupScript(this, GetType(), "ScaleConnectScript", startupScript, true);

                    RenderToClient = true;

                }

                //retrieve Scale Info from selected bpScale
                bpScaleInfo.ClearData();
                HiddenTare.ClearData();
                TareWeightInquiry.ClearData();
                IssueQty.ClearData();
                if (resultStatus != null && resultStatus.IsSuccess)
                    SetScaleInfoLayout();
                //end
            }
        }

        protected virtual void TareButton_Click(object sender, EventArgs e)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var CIOSendMessage = new Camstar.WCF.Services.CIOSendMessageService(session.CurrentUserProfile);
            var serviceData = new OM.CIOSendMessage();

            serviceData.ConditionValue = "SendTare";

            var resultStatus = new OM.ResultStatus();


            resultStatus = CIOSendMessage.ExecuteTransaction(serviceData);

            if (resultStatus.IsSuccess == false)
            {
                Page.DisplayMessage(resultStatus);
            }


        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);



            Page.SetFocus(ScanContainer.ClientID);

            //Hidden fields
            DataColection.Hidden = true;
            CaptureWeightButton.Hidden = true;
            HiddenTare.Hidden = true;

        } // void OnPreRender(EventArgs e)

        public virtual void Page_OnRequestDependentESigValues(object sender, FormProcessingEventArgs e)
        {
            var data = e.Data as OM.ComponentIssue;
            if (data != null)
            {
                if (ManualOverrideField.IsChecked || ToleranceOverrideField.IsChecked)
                {
                    data.CalledByTransactionTask = (OM.NamedSubentityRef)EProcTask.Data;
                    data.CalledByTransactionTask.Parent = (OM.BaseObjectRef)EProcTaskList.Data;
                    data.ServiceDetails = new OM.IssueDetails[1];
                    data.ServiceDetails[0] = new OM.IssueDetails();
                    data.ServiceDetails[0].bpOverrideTolerances = (bool)ToleranceOverrideField.Data;
                    data.ServiceDetails[0].bpManualWeightOverride = (bool)ManualOverrideField.Data;
                }
            }
        }

        public override void GetSelectionData(OM.Service serviceData)
        {
            if (serviceData is OM.ComponentIssue && EProcTask.Data != null)
            {
                (serviceData as OM.ComponentIssue).CalledByTransactionTask = (OM.NamedSubentityRef)EProcTask.Data;
                (serviceData as OM.ComponentIssue).CalledByTransactionTask.Parent = (OM.BaseObjectRef)EProcTaskList.Data;
            }
            base.GetSelectionData(serviceData);

        }

        protected virtual void IssueComponentAction(Personalization.CustomActionEventArgs e)
        {
            if (ExecuteData != null)
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new Camstar.WCF.Services.ComponentIssueService(session.CurrentUserProfile);

                var serviceData = new OM.ComponentIssue
                {
                    Container = (OM.ContainerRef)ContainerName.Data,
                    bpFromContainer = (OM.ContainerRef)(IssueDetailsType == IssueDetailsTypeEnum.Lot || IssueDetailsType == IssueDetailsTypeEnum.Serial ? ExecuteData.Container : null),
                    IssueActualDetails = new OM.IssueActualDetail[] { CreateIssueActualDetail() }
                };

                Page.GetInputData(serviceData);
                GetLineAssignment(serviceData);
                serviceData.ServiceDetails = null;

                var request = new Camstar.WCF.Services.ComponentIssue_Request();
                var result = new Camstar.WCF.Services.ComponentIssue_Result();
                var resultStatus = new OM.ResultStatus();

                if (EProcTaskContainer.Data != null)
                {
                    serviceData.TaskContainer = (OM.ContainerRef)EProcTaskContainer.Data;
                    serviceData.CalledByTransactionTask = (OM.NamedSubentityRef)EProcTask.Data;
                    serviceData.CalledByTransactionTask.Parent = (OM.BaseObjectRef)EProcTaskList.Data;
                }

                OM.ESigServiceDetail[] eSigDetails = ESigCaptureUtil.CollectESigServiceDetails();
                serviceData.ESigDetails = eSigDetails;

                ShopFloorDCControl dcControl = Page.FindCamstarControl("ParamDataField") as ShopFloorDCControl;
                if (dcControl != null)
                {
                    OM.DataPointSummary[] dataPointSummary = dcControl.GetDataPointSummary();
                    if (dataPointSummary != null && dataPointSummary.Length > 0)
                        serviceData.ParametricData = dataPointSummary[0];
                }
                serviceData.AllowZeroQtys = false;
                e.Result = service.ExecuteTransaction(serviceData, request, out result);

                if (e.Result.IsSuccess)
                {
                    WebPart paramDataWP = (Page as WebPartPageBase).Manager.WebParts["ParametricDataWP"];
                    (paramDataWP as WebPartBase).ClearValues(serviceData);
                    DCContainer.Data = ContainerName.Data;

                    //if (!Page.IsFloatingFrame)
                    //    ReloadMaterialsRequirementGrid();
                    //else
                    //    e.IsSubmitted = true;
                    if (Page.IsFloatingFrame)
                        e.IsSubmitted = true;
                }

                ESigCaptureUtil.CleanESigCaptureDM();
            }
            else if (e.Result == null)
            {
                LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);

                OM.Label errorMessage = null;
                if (labelCache != null)
                    errorMessage = labelCache.GetLabelByName("ScanField_RequiredErrorMessage");

                e.Result = new OM.ResultStatus
                {
                    IsSuccess = false,
                    ExceptionData = new OM.ExceptionDataType
                    {
                        Description = errorMessage != null && !string.IsNullOrEmpty(errorMessage.Value) ? errorMessage.Value : "Field \"Scan Container/Product\" requires input.",
                        ExceptionLevel = OM.ExceptionLevel.Client
                    }
                };
            }

            Page.SetFocus(ScanContainer.ClientID);
        } // IssueComponentAction()

        protected virtual void ReloadMaterialsRequirementGrid()
        {
            //ClearGridData();
            ClearUserDataEntryAreaControls();

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.ComponentIssueService(session.CurrentUserProfile);

            var serviceData = new OM.ComponentIssue();

            Page.ProcessingContext[ProcessingFlagType.RequestOnSubmit] = true;
            Page.RequestValues(new OM.ComponentIssue_Info(), serviceData);
            Page.ProcessingContext[ProcessingFlagType.RequestOnSubmit] = false;

            if (MaterialsRequirementContainerName.Data != null)
                serviceData.Container = new OM.ContainerRef(MaterialsRequirementContainerName.Data.ToString());
            serviceData.CalledByTransactionTask = (OM.NamedSubentityRef)EProcTask.Data;
            serviceData.CalledByTransactionTask.Parent = (OM.BaseObjectRef)EProcTaskList.Data;

            var request = new Camstar.WCF.Services.ComponentIssue_Request();
            var result = new Camstar.WCF.Services.ComponentIssue_Result();
            var resultStatus = new OM.ResultStatus();

            request.Info = new OM.ComponentIssue_Info
            {
                RequestValue = true,
                ServiceDetails = new OM.IssueDetails_Info
                {
                    RequestValue = true
                }
            };

            resultStatus = service.GetRequirements(serviceData, request, out result);

            if (resultStatus != null && resultStatus.IsSuccess && result.Value.ServiceDetails != null)
            {
                MaterialRequirements = result.Value.ServiceDetails;

                ProductInquiry.Data = result.Value.ServiceDetails[0].Product;
                ProductDescriptionInquiry.Data = result.Value.ServiceDetails[0].ProductDescription;
                UOMInquiry.Data = result.Value.ServiceDetails[0].UOM;
                if (ExpressionDependsOnFromContainer.IsChecked == false)
                {
                    var DecimalScale = result.Value.ServiceDetails[0]?.DecimalScale;
                    OM.RoundingRuleEnum RoundingRule = OM.RoundingRuleEnum.RoundToNearest;
                    if (result.Value.ServiceDetails[0] != null && result.Value.ServiceDetails[0].RoundingRule != null)
                    {

                        RoundingRule = (OM.RoundingRuleEnum)result.Value.ServiceDetails[0]?.RoundingRule;
                    }
                    Page.SessionVariables.SetValueByName("DecimalScale", DecimalScale);
                    Page.SessionVariables.SetValueByName("RoundingRule", RoundingRule);

                    if (DecimalScale != null)
                    {
                        MaxQtyInquiry.Data = AddDecimalPart(result.Value.ServiceDetails[0].QtyUpperLimit, DecimalScale, RoundingRule);
                        TargetQtyInquiry.Data = AddDecimalPart(result.Value.ServiceDetails[0].QtyRequired, DecimalScale, RoundingRule);
                        MinQtyInquiry.Data = AddDecimalPart(result.Value.ServiceDetails[0].QtyLowerLimit, DecimalScale, RoundingRule);
                        RemainingQtyInquiry.Data = AddDecimalPart(result.Value.ServiceDetails[0].NetQtyRequired, DecimalScale, RoundingRule);
                        TotalQtyInquiry.Data = result.Value.ServiceDetails[0].QtyIssued;
                    }
                    else
                    {
                        MaxQtyInquiry.Data = result.Value.ServiceDetails[0].QtyUpperLimit;
                        TargetQtyInquiry.Data = result.Value.ServiceDetails[0].QtyRequired;
                        MinQtyInquiry.Data = result.Value.ServiceDetails[0].QtyLowerLimit;
                        RemainingQtyInquiry.Data = result.Value.ServiceDetails[0].NetQtyRequired;
                        TotalQtyInquiry.Data = result.Value.ServiceDetails[0].QtyIssued;
                    }
                }

                TareWeightInquiry.Data = null;
                BOMLineItemInquiry.Data = result.Value.ServiceDetails[0].BOMLineItem;

                var issueDetails = (result.Value.ServiceDetails[0] as OM.IssueDetails);

                if (mkIsNeedClearControls)
                {
                    ClearUserDataEntryAreaControls();

                    ExecuteData = new OM.ComponentIssueInquiry
                    {
                        Product = issueDetails.Product,
                        IssueDetails = issueDetails
                    };
                }

                SetUserDataEntryLayout(IssueDetailsType);
                //SetIssueQtyControl(IssueDetailsType, issueDetails);

                serviceData = new OM.ComponentIssue
                {
                    ServiceDetails = new[] { issueDetails }
                };

                this.DisplayValues(serviceData);



            }
            else
                DisplayMessage(resultStatus);

            //SetUserDataEntryLayout(IssueDetailsTypeEnum.Nothing);
            Page.SetFocus(ScanContainer.ClientID);

        } // ReloadMaterialsRequirementGrid()
        private string AddDecimalPart(OM.Primitive<double> input, OM.Primitive<int> decimalplace, OM.RoundingRuleEnum RoundingRule)
        {
            if (input != null)
            {
                int digitsAfterPoint = GetScaleCount(input.ToString());

                string numberAsString = input.ToString();

                if (digitsAfterPoint < (int)decimalplace)
                {
                    for (int i = 0; i < (int)decimalplace - digitsAfterPoint; i++)
                    {
                        if (digitsAfterPoint == 0 && i == 0)
                            numberAsString = $"{numberAsString}.{"0"}";
                        else
                            numberAsString = $"{numberAsString}{"0"}";
                    }
                }
                else
                {

                    double multiplier = Math.Pow(10, (int)decimalplace);
                    if (OM.RoundingRuleEnum.RoundDown == RoundingRule)
                    {
                        numberAsString = (Math.Floor((double)input * multiplier) / multiplier).ToString(); // down
                    }
                    else if (OM.RoundingRuleEnum.RoundUp == RoundingRule)
                    {
                        numberAsString = (Math.Ceiling((double)input * multiplier) / multiplier).ToString(); // up                     
                    }
                    else
                    {
                        numberAsString = Math.Round((double)input, (int)decimalplace).ToString(); //default
                    }

                    digitsAfterPoint = GetScaleCount(numberAsString);
                    for (int i = 0; i < (int)decimalplace - digitsAfterPoint; i++)
                    {
                        if (digitsAfterPoint == 0 && i == 0)
                            numberAsString = $"{numberAsString}.{"0"}";
                        else
                            numberAsString = $"{numberAsString}{"0"}";
                    }
                }

                return numberAsString;
            }
            else
                return string.Empty;
        }
        private int GetScaleCount(string input)
        {
            int digitsAfterPoint = 0;
            if (Double.TryParse(input, out double doubleValue))
            {
                while (Math.Round(doubleValue, digitsAfterPoint) != doubleValue)
                    digitsAfterPoint++;
                return digitsAfterPoint;

            }
            return digitsAfterPoint;
        }


        protected virtual bool IsIssueDifferenceReasonRequired()
        {
            bool result = false;

            if (IssueDetailsType != IssueDetailsTypeEnum.DisplayOnly &&
                IssueDetailsType != IssueDetailsTypeEnum.Nothing)
            {
                bool res1 = false;

                double issuedQtyValue = 0;
                double netQtyValue = ExecuteData != null && ExecuteData.IssueDetails != null ? (double)ExecuteData.IssueDetails.NetQtyRequired : double.NaN;
                double qtyIssued = ExecuteData != null && ExecuteData.IssueDetails != null ? (double)ExecuteData.IssueDetails.QtyIssued : double.NaN;
                //double qtyLowerLimit = ExecuteData != null && ExecuteData.IssueDetails != null ? (double)ExecuteData.IssueDetails.QtyLowerLimit : double.NaN;
                double qtyUpperLimit = 0;
                double qtyLowerLimit = 0;

                if (IssueQty.Data != null)
                {
                    if (double.TryParse(IssueQty.Data.ToString(), out issuedQtyValue))
                        issuedQtyValue += qtyIssued;

                    if (MinQtyInquiry.Data == null && MaxQtyInquiry.Data == null)
                    {
                        if (netQtyValue != double.NaN && double.TryParse(IssueQty.Data.ToString(), out issuedQtyValue))
                            res1 = issuedQtyValue != netQtyValue;
                    }
                    else
                    {
                        if (MinQtyInquiry.Data != null)
                        {
                            double.TryParse(MinQtyInquiry.Data.ToString(), out qtyLowerLimit);
                            if (issuedQtyValue < qtyLowerLimit)
                                res1 = true;
                        }

                        if (MaxQtyInquiry.Data != null)
                        {
                            double.TryParse(MaxQtyInquiry.Data.ToString(), out qtyUpperLimit);
                            if (issuedQtyValue > qtyUpperLimit)
                                res1 = true;
                        }
                    }
                }
                else
                {
                    res1 = false;
                }


                result = res1;
            }
            ToleranceOverrideField.Visible = result;
            IssueDifferenceReason.Required = result;

            return result;
        } // IsIssueDifferenceReasonRequired()

        public virtual void IsIssueDifferenceReasonRequired(object sender, EventArgs e)
        {
            IssueDifferenceReason.Visible = IsIssueDifferenceReasonRequired();
            ToleranceOverrideField.Visible = IssueDifferenceReason.Visible;
        } // IsIssueDifferenceReasonRequired(object sender, EventArgs e)

        protected virtual void SetUserDataEntryLayout(IssueDetailsTypeEnum type)
        {
            ContainerQty.Visible = UOM.Visible = (type == IssueDetailsTypeEnum.Lot || type == IssueDetailsTypeEnum.Serial);
            LotNumber.Visible = type == IssueDetailsTypeEnum.LotAndStockpoint;
            IssueQty.Visible = (type != IssueDetailsTypeEnum.Serial) && (type != IssueDetailsTypeEnum.DisplayOnly) && (type != IssueDetailsTypeEnum.Nothing);
            StockPoint.Visible = (type == IssueDetailsTypeEnum.LotAndStockpoint) || (type == IssueDetailsTypeEnum.Stockpoint);
            IssueDifferenceReason.Visible = IsIssueDifferenceReasonRequired();
            Vendor.Visible = (type == IssueDetailsTypeEnum.LotAndStockpoint || type == IssueDetailsTypeEnum.Stockpoint || type == IssueDetailsTypeEnum.Qty);
            VendorItem.Visible = (type == IssueDetailsTypeEnum.LotAndStockpoint || type == IssueDetailsTypeEnum.Stockpoint || type == IssueDetailsTypeEnum.Qty);
            ScanContainer.Required = ScanContainer.PageFlowRequired = (type == IssueDetailsTypeEnum.Serial || type == IssueDetailsTypeEnum.Stockpoint);
            IssueQty2.Visible = UOM2.Visible = type != IssueDetailsTypeEnum.Serial;
            bpScale.Visible = (type != IssueDetailsTypeEnum.Serial);
            TareButton.Visible = (type != IssueDetailsTypeEnum.Serial);
            CaptureWeightButton.Visible = (type != IssueDetailsTypeEnum.Serial);
            ScaleInfoTitle.Visible = (type != IssueDetailsTypeEnum.Serial);
            ManualOverrideField.Visible = (type != IssueDetailsTypeEnum.Serial);
            ToleranceOverrideField.Visible = (type != IssueDetailsTypeEnum.Serial && IssueDifferenceReason.Visible);
            bpScaleInfo.Visible = (type != IssueDetailsTypeEnum.Serial);
            TareWeightInquiry.Visible = (type != IssueDetailsTypeEnum.Serial);

            Product.Visible = false;




            if (ManualOverrideField.IsChecked)
            {
                bpScaleInfo.Hidden = true;
                bpScaleInfo.Visible = false;
                IssueQty.Hidden = false;
                IssueQty.Visible = true;
                IssueQty2.Hidden = false;

            }
            else if (ManualOverrideField.IsChecked == false)
            {
                bpScaleInfo.Hidden = false;
                bpScaleInfo.Visible = true;
                IssueQty.Hidden = true;
                IssueQty2.Hidden = true;

            }

            if (IsManualReadingOnlyField.IsChecked == true)
            {


                bpScaleInfo.Hidden = true;
                bpScaleInfo.Visible = false;
                IssueQty.Hidden = false;
                IssueQty2.Hidden = false;
            }


            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
            OM.Label Lbl = null;
            if (labelCache != null)
            {
                switch (type)
                {
                    case IssueDetailsTypeEnum.Lot:
                    case IssueDetailsTypeEnum.Serial:
                        {
                            Lbl = labelCache.GetLabelByName("Lbl_ScanContainer");
                            if (Lbl != null)
                                ScanContainer.LabelText = Lbl.Value;
                            break;
                        }
                    case IssueDetailsTypeEnum.LotAndStockpoint:
                    case IssueDetailsTypeEnum.Stockpoint:
                        {
                            Lbl = labelCache.GetLabelByName("Lbl_ScanProduct");
                            if (Lbl != null)
                                ScanContainer.LabelText = Lbl.Value;
                            break;
                        }
                    case IssueDetailsTypeEnum.Qty:
                        {
                            ScanContainer.Visible = false;
                            break;
                        }
                    case IssueDetailsTypeEnum.DisplayOnly:
                    case IssueDetailsTypeEnum.Nothing:
                        {
                            Lbl = labelCache.GetLabelByName("Lbl_ScanContainerProduct");
                            if (Lbl != null)
                                ScanContainer.LabelText = Lbl.Value;
                            break;
                        }
                }

            }
            RenderToClient = true;
        } // SetUserDataEntryLayout(IssueDetailsTypeEnum type)

        protected virtual void SetFocusToControl(IssueDetailsTypeEnum type)
        {
            switch (type)
            {
                case IssueDetailsTypeEnum.Lot:
                case IssueDetailsTypeEnum.Stockpoint:
                case IssueDetailsTypeEnum.Qty:
                    {
                        Page.SetFocus(IssueQty.ClientID);
                        break;
                    }
                case IssueDetailsTypeEnum.Serial:
                    {
                        Page.SetFocus(IssueDifferenceReason.ClientID);
                        break;
                    }
                case IssueDetailsTypeEnum.LotAndStockpoint:
                    {
                        Page.SetFocus(LotNumber.ClientID);
                        break;
                    }
            }
        } // SetIssueQtyControl(IssueDetailsTypeEnum type)



        protected virtual OM.IssueActualDetail CreateIssueActualDetail()
        {
            var issueActualDetail = new OM.IssueActualDetail();

            if (ExecuteData != null && ExecuteData.IssueDetails != null)
            {
                double qtyIssued, qty2Issued, tareValueNum;
                string tareValue = HiddenTare.Data != null ? HiddenTare.Data.ToString().Split(' ')[0] : null;

                issueActualDetail = new OM.IssueActualDetail
                {
                    FieldAction = OM.Action.Create,
                    BOMLineItem = ExecuteData.IssueDetails.BOMLineItem,
                    Product = ExecuteData.Product,
                    FromLot = LotNumber.Data != null && !string.IsNullOrEmpty(LotNumber.Data.ToString()) ? LotNumber.Data.ToString() : null,
                    FromContainer = ExecuteData.Container != null ?
                    new OM.ContainerRef
                    {
                        Name = ExecuteData.Container.Name
                    } : null,
                    IssueDifferenceReason = IssueDifferenceReason.Data != null ?
                    new OM.NamedObjectRef
                    {
                        Name = (IssueDifferenceReason.Data as OM.NamedObjectRef).Name
                    } : null,

                    ManualOverride = ManualOverrideField.IsChecked ? true : false,
                    bpToleranceOverride = ToleranceOverrideField.IsChecked ? true : false,


                    EnteredQtyIssued = IssueDetailsType != IssueDetailsTypeEnum.Serial ?
                    (IssueQty.Data != null) ? IssueQty.Data.ToString() : null : ExecuteData.Qty?.ToString(),

                    EnteredQty2Issued = IssueDetailsType != IssueDetailsTypeEnum.Serial ?
                    (IssueQty2.Data != null) ? IssueQty2.Data.ToString() : null : ExecuteData.Qty2?.ToString(),

                    QtyIssued = IssueDetailsType != IssueDetailsTypeEnum.Serial ?
                    (IssueQty.Data != null && double.TryParse(IssueQty.Data.ToString(), out qtyIssued) ? (OM.Primitive<double>)qtyIssued : null) :
                    ExecuteData.Qty,

                    Qty2Issued = IssueDetailsType != IssueDetailsTypeEnum.Serial ?
                    (IssueQty2.Data != null && double.TryParse(IssueQty2.Data.ToString(), out qty2Issued) ? (OM.Primitive<double>)qty2Issued : null) :
                    ExecuteData.Qty2,

                    bpTareWeight = IssueDetailsType != IssueDetailsTypeEnum.Serial ?
                    (tareValue != null && double.TryParse(tareValue.ToString(), out tareValueNum) ? (OM.Primitive<double>)tareValueNum : null) :
                    null,

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
                    Comments = Comments.Data != null ? Comments.Data.ToString() : null

                };
            }

            return issueActualDetail;
        } // CreateIssueActualDetail()

        protected virtual void ClearPageData()
        {
            Page.ClearValues();
            MaterialsRequirementContainerName.ClearData();
            MaterialsRequirementContainerName.OriginalData = null;
        } // ClearPageData()

        protected virtual void ClearUserDataEntryAreaControls()
        {
            Product.ClearData();

            ScanContainer.ClearData();
            ContainerQty.ClearData();
            UOM.ClearData();
            LotNumber.ClearData();
            IssueQty.ClearData();
            IssueQty.OriginalData = null;
            IssueDifferenceReason.ClearData();
            StockPoint.ClearData();
            UOM2.ClearData();
            IssueQty2.ClearData();
            IssueQty2.OriginalData = null;
            IssueReason.ClearData();
            SubstitutionReason.ClearData();
            Comments.ClearData();
            ExecuteData = null;
            bpScale.ClearData();
            TareWeightInquiry.ClearData();
            HiddenTare.ClearData();
            TareWeightInquiry.ClearData();

        } // ClearUserDataEntryAreaControls()      

        #endregion

        #region Privet methods 

        protected virtual void ClearAllAction()
        {
            ClearPageData();
            //ClearGridData();
            ClearUserDataEntryAreaControls();
            SetUserDataEntryLayout(IssueDetailsType);

        } // ClearAllAction()

        #endregion

        #region Fields

        private bool mkIsNeedClearControls = true;

        #endregion

        #region Constants

        protected const string mkIssueStatusColumn = "IssueStatus";
        protected const string mkQtyIssuedColumn = "QtyIssued";
        protected const string mkDisplayOnlyQtyIssuedValue = "---";
        protected const string mkNetQtyRequiredColumn = "NetQtyRequired";
        protected const string mkBOMLineItemColumn = "BOMLineItem";
        protected const string mkContainerQty2 = "ContainerQty2";
        protected const string mkComponentIssueExecuteData = "ComponentIssueExecuteData";
        protected const string mkServiceDetailsStorageKey = "ServiceDetailsStorage";
        protected const string ScaleMaxCapErrorMsg = "Maximum Qty was exceeded maximum capacity of scale.";
        protected const string ScaleMinCapErrorMsg = "Minimum Qty was fall below minimum capacity of scale.";

        #endregion

        protected enum IssueDetailsTypeEnum
        {
            Nothing,
            Lot,
            Serial,
            LotAndStockpoint,
            Stockpoint,
            Qty,
            DisplayOnly
        }
    }
}
