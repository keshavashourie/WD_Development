// Copyright Siemens 2023 
using System;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Generic;
using System.Globalization;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CamstarPortal.WebControls;

using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets.ComponentIssue
{
    public class MaterialsRequirement_Light : MaterialsRequirement
    {
        public MaterialsRequirement_Light() { Mode = PageMode.Light; }

        #region Protected properties

        protected virtual CWC.TextBox IssueControl_Textbox
        {
            get { return Page.FindCamstarControl("ServiceDetails_IssueControl") as CWC.TextBox; }
        } // IssueControl

        protected override IssueDetailsTypeEnum IssueDetailsType
        {
            get
            {
                var issueDetails = (ExecuteData != null && ExecuteData.IssueDetails != null) ? ExecuteData.IssueDetails : null;
                var type = IssueDetailsTypeEnum.Nothing;

                if (issueDetails != null && issueDetails is OM.IssueDetails)
                {
                    if (issueDetails is OM.IssueDetailsBulk || issueDetails.IssueControl == OM.IssueControlEnum.Bulk)
                        type = IssueDetailsTypeEnum.Lot;
                    else if (issueDetails is OM.IssueDetailsSerial || issueDetails.IssueControl == OM.IssueControlEnum.Serialized)
                        type = IssueDetailsTypeEnum.Serial;
                    else if (issueDetails is OM.IssueDetailsLotStock || issueDetails.IssueControl == OM.IssueControlEnum.LotAndStockPoint)
                        type = IssueDetailsTypeEnum.LotAndStockpoint;
                    else if (issueDetails is OM.IssueDetailsStock || issueDetails.IssueControl == OM.IssueControlEnum.StockPointOnly)
                        type = IssueDetailsTypeEnum.Stockpoint;
                    else if (issueDetails is OM.IssueDetailsQuantity || issueDetails.IssueControl == OM.IssueControlEnum.NoTracking)
                        type = IssueDetailsTypeEnum.Qty;
                    else if (issueDetails is OM.IssueDetailsDisplayOnly || issueDetails.IssueControl == OM.IssueControlEnum.CommentOnly)
                        type = IssueDetailsTypeEnum.DisplayOnly;
                }

                return type;
            }
        } // IssueDetailsType

        protected JQDataGrid MaterialListAbbr { get { return Page.FindCamstarControl("ComponentIssue_MaterialListAbbreviated") as JQDataGrid; } }
        protected ContainerListGrid HiddenContainer { get { return Page.FindCamstarControl("HiddenSelectedContainer") as ContainerListGrid; } }

        #endregion

        #region Public methods

        private int SelectedRowIndex = -1;

        public override void DisplayValues(OM.Service serviceData)
        {
            base.DisplayValues(serviceData);

            try
            {
                OM.ComponentIssue ci = serviceData as OM.ComponentIssue;
                if (ci.ServiceDetails.Length > 0)
                {
                    OM.IssueDetails detail = ci.ServiceDetails[0];

                    NetQtyRequired.Data = detail.NetQtyRequired;
                    Product.Data = detail.Product;
                    ProductDescription.Data = detail.ProductDescription;
                    IssueControl_Textbox.Data = detail.IssueControlName.Value;
                }
            }
            catch { }
        }

        #endregion

        #region Protected methods

        //protected override void OnLoad(EventArgs e)
        //{
        //    base.OnLoad(e);

        //    if (!Page.IsPostBack)
        //        SetSatisfiedWidgetCountControl();

        //    if (Page.IsFloatingFrame && !Page.IsPostBack)
        //        ReloadMaterialsRequirementGrid();            

        //    HideSatisfiedReq.CheckControl.CheckedChanged += CheckControl_CheckedChanged;
        //    MaterialsRequirementGrid.RowSelected += MaterialsRequirementGrid_RowSelected;
        //    (MaterialsRequirementGrid.GridContext as SubentityDataContext).GetRowSnapItem += MaterialsRequirement_GetRowSnapItem;            
        //} // OnLoad(EventArgs e)

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (ContainerName.Data != null)
            {
                SetFocusToControl(IssueDetailsType);
            }
            if (DataColection != null)
                DataColection.Hidden = true;

            var submitAction = Page.ActionDispatcher.GetActionByName("IssueComponentButton");
            if (submitAction != null)
            {
                var button = submitAction.Control as WebControl;
                if (button != null)
                    button.Attributes[ControlAttributeConstants.IsTimersConfirmationRequired] = "true";
            }
            SetUserDataEntryLayout(IssueDetailsType);

        } // void OnPreRender(EventArgs e)


        protected override void IssueComponentAction(Personalization.CustomActionEventArgs e)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            bool shouldExecute = ExecuteData != null;
            if (!shouldExecute)
            {
                if (MaterialRequirementsEx != null)
                {
                    IssueDetailsEx item = MaterialRequirementsEx.Find(s => s.IssueDetails.IssueControl.Value != (int)OM.IssueControlEnum.CommentOnly && s.IssueDetails.NetQtyRequired.Value > 0);
                    if (item == null)
                    {
                        if (EProcTaskContainer.Data != null)
                        {
                            e.Result = PerformExecuteTask(e);
                        }
                        else
                        {
                            WebPart paramDataWP = (Page as WebPartPageBase).Manager.WebParts["ParametricDataWP"];
                            (paramDataWP as WebPartBase).ClearValues();
                            DCContainer.Data = ContainerName.Data;

                            if (!Page.IsFloatingFrame)
                            {
                                int savePage = GridCurrentPage;
                                ReloadMaterialsRequirementGrid();
                                if (savePage != GridCurrentPage)
                                {
                                    GridCurrentPage = savePage;
                                }
                            }
                            else
                                e.IsSubmitted = true;

                            ESigCaptureUtil.CleanESigCaptureDM();
                            e.Result = new OM.ResultStatus(string.Empty, true);
                        }
                    }
                }
            }
            if (shouldExecute)
            {
                var service = new Camstar.WCF.Services.ComponentIssueR2Service(session.CurrentUserProfile);

                OM.IssueDetails[] gridData = MaterialsRequirementGrid.Data as OM.IssueDetails[];

                try { SelectedRowIndex = Convert.ToInt32(MaterialsRequirementGrid.SelectedRowID); }
                catch { }

                var serviceData = new OM.ComponentIssueR2();
                if (ExecuteData != null)
                {
                    serviceData = new OM.ComponentIssueR2()
                    {
                        Container = (OM.ContainerRef)ContainerName.Data,
                        IssueActualDetails = new OM.IssueActualDetail[] { CreateIssueActualDetail() },
                        ServiceDetails = new OM.IssueDetails[] { gridData[SelectedRowIndex] }
                    };
                }
                else
                {
                    serviceData = new OM.ComponentIssueR2()
                    {
                        Container = (OM.ContainerRef)ContainerName.Data
                    };
                }

                Page.GetInputData(serviceData);
                if (ExecuteData == null && serviceData.ServiceDetails != null && serviceData.ServiceDetails.Length > 0)
                {                    
                    serviceData.ServiceDetails = null;
                }
                GetLineAssignment(serviceData);

                var request = new Camstar.WCF.Services.ComponentIssueR2_Request();
                var result = new Camstar.WCF.Services.ComponentIssueR2_Result();
                var resultStatus = new OM.ResultStatus();

                if (EProcTaskContainer.Data != null)
                {
                    serviceData.TaskContainer = (OM.ContainerRef)EProcTaskContainer.Data;
                    serviceData.CalledByTransactionTask = (OM.NamedSubentityRef)EProcTask.Data;
                    serviceData.CalledByTransactionTask.Parent = (OM.BaseObjectRef)EProcTaskList.Data;
                }
                if (Comments.Data != null)
                    serviceData.Comments = Comments.Data.ToString();

                var tuple = ESigCaptureUtil.CollectESigServiceDetailsAll();
                if (tuple != null)
                    serviceData.ESigDetails = ESigCaptureUtil.CollectESigServiceDetailsAll().Item1;

                ShopFloorDCControl dcControl = Page.FindCamstarControl("ParamDataField") as ShopFloorDCControl;
                if (dcControl != null)
                {
                    OM.DataPointSummary[] dataPointSummary = dcControl.GetDataPointSummary();
                    if (dataPointSummary != null && dataPointSummary.Length > 0)
                        serviceData.ParametricData = dataPointSummary[0];
                }

                e.Result = service.ExecuteTransaction(serviceData, request, out result);

                if (e.Result.IsSuccess || ExecuteData == null)
                {
                    WebPart paramDataWP = (Page as WebPartPageBase).Manager.WebParts["ParametricDataWP"];
                    (paramDataWP as WebPartBase).ClearValues(serviceData);
                    DCContainer.Data = ContainerName.Data;

                    if (!Page.IsFloatingFrame)
                    {
                        int savePage = GridCurrentPage;
                        ReloadMaterialsRequirementGrid();
                        if (savePage != GridCurrentPage)
                        {
                            GridCurrentPage = savePage;
                        }
                    }
                    else
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
            if (ContainerName.Data != null)
                SetFocusToControl(IssueDetailsType);
        } // IssueComponentAction()

        protected bool _haveReloadedRequirements = false;
        protected override void ReloadMaterialsRequirementGrid()
        {
            if (!_haveReloadedRequirements)
            {
                ClearGridData();
                ClearUserDataEntryAreaControls();

                if (MaterialsRequirementContainerName.Data != null && !string.IsNullOrEmpty(MaterialsRequirementContainerName.Data.ToString()) && string.Compare(MaterialsRequirementContainerName.Data.ToString(), "<>") != 0)
                {
                    WCF.ObjectStack.ContainerRef val = new OM.ContainerRef(MaterialsRequirementContainerName.Data.ToString());
                    Page.DataContract.SetValueByName("SelectedContainerNameDM", val);

                    // Get Basic Container Info that is used in all MaterialListItems
                    GetContainerInfo();

                    if (MaterialsRequirementContainerName.Data != null && !string.IsNullOrEmpty(MaterialsRequirementContainerName.Data.ToString()))
                    {
                        // Material List
                        List<IssueDetailsEx> details = new List<IssueDetailsEx>();
                        OM.ResultStatus resultStatus = GetMaterialList(out details);

                        if (resultStatus != null && resultStatus.IsSuccess)
                        {
                            MaterialRequirementsEx = details;
                            FilterAndBindMaterialReqs(true);
                            SetSatisfiedWidgetCountControl();
                        }
                        else
                        {
                            MaterialRequirementsEx = null;
                            SetSatisfiedWidgetCountControl();
                            DisplayMessage(resultStatus);
                        }

                        _haveReloadedRequirements = true;
                    }
                }
                else
                {
                    MaterialRequirementsEx = null;
                }

                SetSatisfiedWidgetCountControl();
                SetUserDataEntryLayout(IssueDetailsTypeEnum.Nothing);
            }
        } // ReloadMaterialsRequirementGrid()

        private OM.ResultStatus GetMaterialList(out List<IssueDetailsEx> details)
        {
            var resultStatus = LoadRequirements(out details);// new OM.ResultStatus();

            return resultStatus;
        }

        protected override void ClearGridData()
        {
            MaterialsRequirementGrid.ClearData();
            MaterialsRequirementGrid.OriginalData = null;
            MaterialsRequirementGrid.GridContext.CurrentPage = 1;
            GridCurrentPage = 1;
            SetSatisfiedWidgetCountControl();
        } // ClearGridData()

        protected override void ClearUserDataEntryAreaControls()
        {
            base.ClearUserDataEntryAreaControls();
            IssueControl_Textbox.ClearData();
        } // ClearUserDataEntryAreaControls()      

        #endregion
    }
}
