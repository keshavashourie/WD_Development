// Copyright Siemens 2024 
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using CamstarPortal.WebControls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;

namespace Camstar.WebPortal.WebPortlets.ComponentIssue
{
    public class MaterialsRequirement : MatrixWebPart
    {
        protected enum PageMode
        {
            Normal = 0,
            Advanced = 1,
            Light = 2
        }

        protected PageMode Mode = PageMode.Normal;
        public MaterialsRequirement() { }

        #region Protected properties

        protected OM.ResultStatus _addToPendingStatus = null;

        protected MatrixWebPart MaterialRequirementsWP { get { return Page.FindCamstarControl("MaterialRequirementsWP") as MatrixWebPart; } }
        protected List<IssueDetailsEx> MaterialRequirementsEx
        {
            get
            {
                // Using DataContract like Session storage?
                List<IssueDetailsEx> reqs = Page.DataContract.GetValueByName<List<IssueDetailsEx>>(MaterialRequirementsExKey);
                return reqs ?? new List<IssueDetailsEx>();
            }
            set
            {
                Page.DataContract.SetValueByName(MaterialRequirementsExKey, value);
            }
        }

        protected int GridCurrentPage
        {
            get
            {
                object val = Page.DataContract.GetValueByName(GridCurrentPageKey);
                return val != null ? (int)val : 1;
            }
            set
            {
                Page.DataContract.SetValueByName(GridCurrentPageKey, value);
            }
        }

        protected virtual OM.IssueDetails[] MaterialRequirements
        {
            get
            {
                return MaterialRequirementsEx.Select(mr => mr.IssueDetails).ToArray();
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

        protected virtual JQDataGrid MaterialsRequirementGrid
        {
            get { return FindCamstarControl("MaterialRequirementsGrid") as JQDataGrid; }
        } // MaterialsRequirementGrid

        protected virtual OM.IssueDetails SelectionGridData
        {
            get { return MaterialsRequirementGrid.SelectionData as OM.IssueDetails; }
        } // SelectionGridData

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

        protected virtual CWC.CheckBox HideSatisfiedReq
        {
            get { return Page.FindCamstarControl("HideSatisfiedReq") as CWC.CheckBox; }
        } // HideSatisfiedReq

        protected virtual CWC.TextBox SatisfiedWidgetCount
        {
            get { return Page.FindCamstarControl("SatisfiedWidgetCount") as CWC.TextBox; }
        } // SatisfiedWidgetCount

        /// <summary>
        /// EProcedure specific hidden controls
        /// </summary>
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

        protected virtual CWC.TextBox ContainerSpecName
        {
            get { return Page.FindCamstarControl("ContainerStatus_SpecName") as CWC.TextBox; }
        }

        protected virtual CWC.TextBox ContainerSpecRevision
        {
            get { return Page.FindCamstarControl("ContainerStatus_SpecRevision") as CWC.TextBox; }
        }

        protected virtual CWC.TextBox ContainerOperation
        {
            get { return Page.FindCamstarControl("ContainerStatus_Operation") as CWC.TextBox; }
        }

        protected virtual string DefaultIssueDifferenceReason
        {
            get
            {
                return ViewState[mkDefaultIssueDifferenceReason] as string;
            }
            set
            {
                ViewState[mkDefaultIssueDifferenceReason] = value;
            }
        } // ExecuteData

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

        protected virtual List<OM.IssueActualDetail> ExecuteDataList
        {
            get
            {
                var session = new CallStack(MaterialsRequirementGrid.CallStackKey).Context.LocalSession;
                List<OM.IssueActualDetail> resultList = null;

                if (session != null)
                {
                    if (session[mkComponentIssueExecuteDataList] == null)
                        session[mkComponentIssueExecuteDataList] = new List<OM.IssueActualDetail>();

                    resultList = session[mkComponentIssueExecuteDataList] as List<OM.IssueActualDetail>;
                }

                return resultList;
            }
        } // ExecuteDataList

        protected virtual List<OM.IssueActualDetail> ExecuteDataListNonSerial
        {
            get
            {
                var session = new CallStack(MaterialsRequirementGrid.CallStackKey).Context.LocalSession;
                List<OM.IssueActualDetail> resultList = null;

                if (session != null)
                {
                    if (session[mkComponentIssueExecuteDataListNonSerial] == null)
                        session[mkComponentIssueExecuteDataListNonSerial] = new List<OM.IssueActualDetail>();

                    resultList = session[mkComponentIssueExecuteDataListNonSerial] as List<OM.IssueActualDetail>;
                }

                return resultList;
            }
        } // ExecuteDataListNonSerial
        protected virtual IssueDetailsTypeEnum IssueDetailsType
        {
            get
            {
                var issueDetails = (ExecuteData != null && ExecuteData.IssueDetails != null) ? ExecuteData.IssueDetails : null;
                var type = IssueDetailsTypeEnum.Nothing;

                if (issueDetails != null && issueDetails is OM.IssueDetails)
                {
                    if (issueDetails.IssueControl == OM.IssueControlEnum.Bulk)
                        type = IssueDetailsTypeEnum.Lot;
                    else if (issueDetails.IssueControl == OM.IssueControlEnum.Serialized)
                        type = IssueDetailsTypeEnum.Serial;
                    else if (issueDetails.IssueControl == OM.IssueControlEnum.LotAndStockPoint)
                        type = IssueDetailsTypeEnum.LotAndStockpoint;
                    else if (issueDetails.IssueControl == OM.IssueControlEnum.StockPointOnly)
                        type = IssueDetailsTypeEnum.Stockpoint;
                    else if (issueDetails.IssueControl == OM.IssueControlEnum.NoTracking)
                        type = IssueDetailsTypeEnum.Qty;
                    else if (issueDetails.IssueControl == OM.IssueControlEnum.CommentOnly)
                        type = IssueDetailsTypeEnum.DisplayOnly;
                }

                return type;
            }
        } // IssueDetailsType

        #endregion

        #region Public methods

        public virtual void ScanFieldChanged(object sender, EventArgs e)
        {
            var objectName = ScanContainer.Data != null ? ScanContainer.Data.ToString() : string.Empty;

            if (!string.IsNullOrEmpty(objectName))
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new Camstar.WCF.Services.ComponentIssueInquiryService(session.CurrentUserProfile);
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

                        OM.NamedSubentityRef bOMLineItem = result.Value.IssueDetails.BOMLineItem;
                        if (MaterialRequirementsEx != null)
                        {
                            IssueDetailsEx item = MaterialRequirementsEx.Find(s => s.IssueDetails.BOMLineItem != null && string.Compare(s.IssueDetails.BOMLineItem.Name, result.Value.IssueDetails.BOMLineItem.Name, true) == 0);
                            //  Item already issued - see if there is another item of same product
                            if (item != null && item.IssueDetails != null && item.IssueDetails.NetQtyRequired.Value <= 0)
                            {
                                item = MaterialRequirementsEx.Find(s => IsProductMatch(s.IssueDetails.Product, result.Value.IssueDetails.Product) && s.IssueDetails.NetQtyRequired.Value > 0);
                                if (item != null)
                                {
                                    ExecuteData.IssueDetails = item.IssueDetails;
                                    bOMLineItem = item.IssueDetails.BOMLineItem;
                                }
                            }
                        }
                        SetSelectedRow(bOMLineItem);
                        SetFocusToControl(IssueDetailsType);
                    }
                }

                if (e == null)
                    e = new CustomActionEventArgs();

                if (e is CustomActionEventArgs)
                    (e as CustomActionEventArgs).Result = resultStatus;

                if (!resultStatus.IsSuccess)
                    Page.DisplayMessage(resultStatus);
            }
            else
                ClearUserDataEntryAreaControls();
        } // ScanFieldChanged(object sender, EventArgs e)        

        protected bool IsProductMatch(OM.RevisionedObjectRef obj1, OM.RevisionedObjectRef obj2)
        {
            return string.Compare(obj1.ID, obj2.ID, true) == 0 || (string.Compare(obj1.Name, obj2.Name, true) == 0 && (string.Compare(obj1.Revision, obj2.Revision, true) == 0 || obj1.RevisionOfRecord == obj2.RevisionOfRecord));
        }
        public virtual void CheckControl_CheckedChanged(object sender, EventArgs e)
        {
            FilterAndBindMaterialReqs(false);
        } // CheckControl_CheckedChanged(object sender, EventArgs e)

        public virtual void MaterialsRequirement_GetRowSnapItem(object item, IEnumerable<DataColumn> dataColumns, DataRow row)
        {
            if ((item as OM.IssueDetails).IssueControl == OM.IssueControlEnum.CommentOnly)
            {
                foreach (var c in dataColumns)
                    if (c.ColumnName.Equals(mkQtyIssuedColumn))
                        row[c] = mkDisplayOnlyQtyIssuedValue;
            }
        } // MaterialsRequirement_GetRowSnapItem(object item, IEnumerable<DataColumn> dataColumns, DataRow row)        

        public virtual ResponseData MaterialsRequirementGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            if (SelectionGridData != null)
            {
                var issueDetails = (SelectionGridData as OM.IssueDetails);

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
                SetIssueQtyControl(IssueDetailsType, issueDetails);

                var serviceData = new OM.ComponentIssue
                {
                    ServiceDetails = new[] { issueDetails }
                };

                this.DisplayValues(serviceData);

                SetFocusToControl(IssueDetailsType);
            }
            mkIsNeedClearControls = true;
            return null;
        } // MaterialsRequirementGrid_RowSelected(object sender, JQGridEventArgs args)

        public virtual void ContainerNameControl_DataChanged(object sender, EventArgs e)
        {
            ReloadMaterialsRequirementGrid();
        } // ContainerNameControl_DataChanged(object sender, EventArgs e)    

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
                            ExecuteDataList.Clear();
                            ExecuteDataListNonSerial.Clear();
                            ReloadMaterialsRequirementGrid();
                            break;
                        }
                    case "IssueComponent":
                        {
                            IssueComponentAction(e);
                            
                            break;
                        }
                    case "DoComponentIssue":
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

            if (!Page.IsPostBack)
            {
                SetSatisfiedWidgetCountControl();
                GridCurrentPage = 1;
            } else
                GridCurrentPage = MaterialsRequirementGrid.GridContext.CurrentPage;
            if (Page.IsFloatingFrame && !Page.IsPostBack)
                ReloadMaterialsRequirementGrid();
            else if (Page.IsPostBack)
            {

                var selContainerName = Page.DataContract.GetValueByName("SelectedContainerNameDM");
                string container = Convert.ToString(MaterialsRequirementContainerName.Data);
                /*if (selContainerName != null && MaterialsRequirementGrid.Data == null && string.Compare(selContainerName.ToString(), container, true) != 0)
                {
                    MaterialsRequirementContainerName.Data = selContainerName;
                    ReloadMaterialsRequirementGrid();
                }*/
            }

            HideSatisfiedReq.CheckControl.CheckedChanged += CheckControl_CheckedChanged;
            MaterialsRequirementGrid.RowSelected += MaterialsRequirementGrid_RowSelected;
            (MaterialsRequirementGrid.GridContext as SubentityDataContext).GetRowSnapItem += MaterialsRequirement_GetRowSnapItem;
        } // OnLoad(EventArgs e)

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (GridCurrentPage != MaterialsRequirementGrid.GridContext.CurrentPage)
                MaterialsRequirementGrid.GridContext.CurrentPage = GridCurrentPage;

            SetUserDataEntryLayout(IssueDetailsType);

            if (ContainerName.Data != null)
            {
                SetFocusToControl(IssueDetailsType);
            }
            DataColection.Hidden = true;

            var submitAction = Page.ActionDispatcher.GetActionByName("IssueComponentButton");
            if (submitAction != null)
            {
                var button = submitAction.Control as WebControl;
                if (button != null)
                    button.Attributes[ControlAttributeConstants.IsTimersConfirmationRequired] = "true";
            }
            if (_addToPendingStatus != null && !_addToPendingStatus.IsSuccess)
            {
                if (_addToPendingStatus.ExceptionData != null)
                    Page.StatusBar.WriteError(_addToPendingStatus.ExceptionData.Description);
                else
                    Page.StatusBar.WriteError(_addToPendingStatus.ToString());
            }
        } // void OnPreRender(EventArgs e)

        protected OM.ResultStatus PerformExecuteTask(Personalization.CustomActionEventArgs e)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.ExecuteTaskService(session.CurrentUserProfile);
            var serviceData = new OM.ExecuteTask()
            {
                Container = (OM.ContainerRef)ContainerName.Data
            };
            GetLineAssignment(serviceData);

            var request = new Camstar.WCF.Services.ExecuteTask_Request();
            var result = new Camstar.WCF.Services.ExecuteTask_Result();
            var resultStatus = new OM.ResultStatus();

            if (EProcTaskContainer.Data != null)
            {
                //serviceData.TaskContainer = (OM.ContainerRef)EProcTaskContainer.Data;
                serviceData.Task = (OM.NamedSubentityRef)EProcTask.Data;
                serviceData.TaskList = (OM.RevisionedObjectRef)EProcTaskList.Data;
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
            if (e.Result.IsSuccess)
            {
                if (e.Result.IsSuccess)
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
            return e.Result;
        }

        protected virtual void IssueComponentAction(Personalization.CustomActionEventArgs e)
        {
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
                        }
                    }
                }
            }
            if (shouldExecute)
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new Camstar.WCF.Services.ComponentIssueService(session.CurrentUserProfile);

                var serviceData = new OM.ComponentIssue();
                if (ExecuteData != null)
                {
                    serviceData = new OM.ComponentIssue()
                    {
                        Container = (OM.ContainerRef)ContainerName.Data,
                        IssueActualDetails = new OM.IssueActualDetail[] { CreateIssueActualDetail() },
                    };
                }
                else
                {
                    serviceData = new OM.ComponentIssue()
                    {
                        Container = (OM.ContainerRef)ContainerName.Data
                    };
                }

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
                if (Comments.Data != null)
                    serviceData.Comments = Comments.Data.ToString();

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
                Page.SetFocus(ScanContainer.ClientID);
        } // IssueComponentAction()

        protected virtual void ReloadMaterialsRequirementGrid()
        {
            ClearGridData();
            ClearUserDataEntryAreaControls();

            if (MaterialsRequirementContainerName.Data != null && !string.IsNullOrEmpty(MaterialsRequirementContainerName.Data.ToString()) && string.Compare(MaterialsRequirementContainerName.Data.ToString(), "<>") != 0)
            {
                WCF.ObjectStack.ContainerRef val = new OM.ContainerRef(MaterialsRequirementContainerName.Data.ToString());
                Page.DataContract.SetValueByName("SelectedContainerNameDM", val);
                GetContainerInfo();

                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new Camstar.WCF.Services.ComponentIssueService(session.CurrentUserProfile);

                var serviceData = new OM.ComponentIssue();

                Page.ProcessingContext[ProcessingFlagType.RequestOnSubmit] = true;
                Page.RequestValues(new OM.ComponentIssue_Info(), serviceData);
                Page.ProcessingContext[ProcessingFlagType.RequestOnSubmit] = false;

                if (MaterialsRequirementContainerName.Data != null)
                {
                    serviceData.Container = new OM.ContainerRef(MaterialsRequirementContainerName.Data.ToString());

                    var result = new Camstar.WCF.Services.ComponentIssue_Result();
                    List<IssueDetailsEx> details = new List<IssueDetailsEx>();
                    var resultStatus = LoadRequirements(out details);// new OM.ResultStatus();

                    if (resultStatus != null && resultStatus.IsSuccess)
                    {
                        MaterialRequirementsEx = details;
                        FilterAndBindMaterialReqs(false);
                        SetSatisfiedWidgetCountControl();
                    }
                    else
                    {
                        MaterialRequirementsEx = null;
                        SetSatisfiedWidgetCountControl();
                        DisplayMessage(resultStatus);
                    }
                }
                else
                {
                    MaterialRequirementsEx = null;
                    SetSatisfiedWidgetCountControl();
                }
                SetUserDataEntryLayout(IssueDetailsTypeEnum.Nothing);
            }
            if (ContainerName.Data != null)
                Page.SetFocus(ScanContainer.ClientID);
        } // ReloadMaterialsRequirementGrid()

        protected void GetContainerInfo()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new WCF.Services.ContainerInfoInquiryService(session.CurrentUserProfile);
            var serviceData = new OM.ContainerInfoInquiry()
            {
                Container = new OM.ContainerRef(Convert.ToString(MaterialsRequirementContainerName.Data))
            };

            var request = new WCF.Services.ContainerInfoInquiry_Request();
            var result = new WCF.Services.ContainerInfoInquiry_Result();
            var resultStatus = new OM.ResultStatus();

            request.Info = new OM.ContainerInfoInquiry_Info
            {
                ContainerInfo = new OM.ContainerInfo_Info()
                {
                    CurrentStatus = new OM.CurrentStatus_Info()
                    {
                        Spec = new OM.Info(true)
                    }
                },
                CurrentRouteStep = new OM.Info(true)
            };
            request.Info.ContainerInfo.CurrentStatus.Spec.RequestSelectionValues = true;
            resultStatus = service.ContainerInfoInquiry_GetContainerInfo(serviceData, request, out result);
            if (resultStatus != null && resultStatus.IsSuccess)
            {
                string specId = "", stepId = "", specName = "", stepName = "";
                if (result.Value.ContainerInfo != null && result.Value.ContainerInfo.CurrentStatus != null)
                {
                    if (result.Value.ContainerInfo.CurrentStatus.Spec != null)
                    {
                        specId = result.Value.ContainerInfo.CurrentStatus.Spec.ID;
                        specName = result.Value.ContainerInfo.CurrentStatus.Spec.Name;
                    }
                    if (result.Value.CurrentRouteStep != null)
                    {
                        stepId = result.Value.CurrentRouteStep.ID;
                        stepName = result.Value.CurrentRouteStep.Name;
                    }
                    //if (!string.IsNullOrEmpty(specId))
                    Page.DataContract.SetValueByName(_ContainerSpecId, specId);
                    Page.DataContract.SetValueByName(_ContainerSpecName, specName);

                    //if (!string.IsNullOrEmpty(stepId))
                    Page.DataContract.SetValueByName(_ContainerWorkflowStepId, stepId);
                    Page.DataContract.SetValueByName(_ContainerWorkflowStepName, stepName);

                    CheckSpecOptions(specId, specName);
                }
            }
        }

        protected virtual void CheckSpecOptions(string specId, string specName)
        {
        }

        private List<IssueDetailsEx> GetPhantomBill(OM.Row row, OM.MaterialListItemSettings settings, Dictionary<IssueDetailsHeaderEnum, int> columnIndexes, List<string> visitedPhantomBillIds, int containerQty, bool calculateQtyRequired)
        {
            List<IssueDetailsEx> details = new List<IssueDetailsEx>();

            string phantomBillId = GetRowValue<string>(row, columnIndexes[IssueDetailsHeaderEnum.PhantomBillIdIndex]);
            visitedPhantomBillIds.Add(phantomBillId);

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new WCF.Services.ComponentIssueR2Service(session.CurrentUserProfile);
            var serviceData = new OM.ComponentIssueR2()
            {
                PhantomBillId = phantomBillId,
                Container = new OM.ContainerRef(MaterialsRequirementContainerName.Data.ToString())
            };

            var request = new WCF.Services.ComponentIssueR2_Request();
            var result = new WCF.Services.ComponentIssueR2_Result();
            var resultStatus = new OM.ResultStatus();

            request.Info = new OM.ComponentIssueR2_Info
            {
                RequestValue = true,
                PhantomBillDetails = new OM.IssueDetails_Info() { RequestSelectionValues = true }
            };

            resultStatus = service.Load(serviceData, request, out result);

            if (resultStatus.IsSuccess && resultStatus.IsSuccess &&
                    result.Environment.PhantomBillDetails.SelectionValues != null)
            {
                OM.Header[] headers = result.Environment.PhantomBillDetails.SelectionValues.Headers;
                OM.Row[] rows = result.Environment.PhantomBillDetails.SelectionValues.Rows;

                columnIndexes = GetColumnIndexes(headers);

                List<OM.IssueDetails> issueDetails = new List<OM.IssueDetails>();
                List<string> issueDetailsIds = new List<string>();

                if (headers != null && rows != null)
                {
                    for (int i = rows.Length - 1; i >= 0; i--)
                    {
                        string id = GetRowValue<string>(rows[i], columnIndexes[IssueDetailsHeaderEnum.MaterialListItemIdIndex]);
                        if (!issueDetailsIds.Contains(id)) // Make sure the IssueDetail has not been already added
                        {
                            issueDetailsIds.Add(id);
                            string currentPhantomBillId = GetRowValue<string>(rows[i], columnIndexes[IssueDetailsHeaderEnum.PhantomBillIdIndex]);
                            if (string.IsNullOrEmpty(currentPhantomBillId)) // Not a phantom bill.
                            {
                                IssueDetailsEx detail = CreateIssueDetail(rows[i], settings, columnIndexes, containerQty, calculateQtyRequired);

                                if (detail != null)
                                    details.Add(detail);
                            }
                            else // Is Phantom Bill
                            {
                                // Logic to prevent circular reference ie Phantom Bill 1 -> Phantom Bill 2 -> Phantom Bill 1, etc.
                                if (!visitedPhantomBillIds.Contains(currentPhantomBillId))
                                {
                                    DateTime? from = GetRowValue<DateTime?>(rows[i], columnIndexes[IssueDetailsHeaderEnum.EffectiveFromDateGMT], (x) => Convert.ToDateTime(x));
                                    DateTime? thru = GetRowValue<DateTime?>(rows[i], columnIndexes[IssueDetailsHeaderEnum.EffectiveThruDateGMT], (x) => Convert.ToDateTime(x));

                                    string IssueDetailSpecStepId = GetRowValue<string>(rows[i], columnIndexes[IssueDetailsHeaderEnum.SpecIdIndex]);
                                    string IssueDetailSpecStepName = GetRowValue<string>(rows[i], columnIndexes[IssueDetailsHeaderEnum.SpecNameIndex]);

                                    if (ItemIsAvailable(from, thru, true) && ItemIsOnCorrectSpecStep(settings, IssueDetailSpecStepId, IssueDetailSpecStepName, out string notUsed, out string notUsed2))
                                    {
                                        int totalQtyRequired = containerQty;
                                        int phantomQtyRequired = GetRowValue<int>(rows[i], columnIndexes[IssueDetailsHeaderEnum.QtyRequiredIndex], (x) => Convert.ToInt32(x));
                                        if (phantomQtyRequired > 0)
                                            totalQtyRequired *= phantomQtyRequired;
                                        List<IssueDetailsEx> phantomDetails = GetPhantomBill(rows[i], settings, columnIndexes, visitedPhantomBillIds, totalQtyRequired, calculateQtyRequired);
                                        details.AddRange(phantomDetails);
                                    }
                                }
                                else
                                {
                                    // Circular Reference not allowed
                                    LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
                                    string labelValue, defaultValue;
                                    labelCache.GetLabelByName("PhantomBillCircularReferenceLight", out labelValue, out defaultValue);
                                    DisplayMessage(new OM.ResultStatus(!string.IsNullOrEmpty(labelValue) ? labelValue : defaultValue, false));
                                    return details;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                DisplayMessage(resultStatus);
                return details;
            }

            return details;
        }

        internal class TaskMaterial
        {
            public string ProductId;
            public string ProductName;
            public string ProductRevision;
            public string RefDes;
            public double Qty;
            public bool IsRoR;
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="details"></param>
        /// <returns></returns>
        protected OM.ResultStatus LoadRequirements(out List<IssueDetailsEx> details)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new WCF.Services.ComponentIssueR2Service(session.CurrentUserProfile);
            var serviceData = new OM.ComponentIssueR2();

            Page.ProcessingContext[ProcessingFlagType.RequestOnSubmit] = true;
            Page.RequestValues(new OM.ComponentIssue_Info(), serviceData);
            Page.ProcessingContext[ProcessingFlagType.RequestOnSubmit] = false;

            if (MaterialsRequirementContainerName.Data != null)
                serviceData.Container = new OM.ContainerRef(MaterialsRequirementContainerName.Data.ToString());

            var request = new WCF.Services.ComponentIssueR2_Request();
            var result = new WCF.Services.ComponentIssueR2_Result();
            var resultStatus = new OM.ResultStatus();

            request.Info = new OM.ComponentIssueR2_Info
            {
                RequestValue = true,
                // Determines whether the QtyRequired is [ContainerQty * Component's QtyRequired] (default) or just Component's Qty Required. 
                // Modify business logic (ServiceDetails' Selection Values event logic) to set field CalculateQtyRequired to false
                // if you want Qty Required to simply be Component's Qty Required.
                CalculateQtyRequired = new OM.Info(true),
                ServiceDetails = new OM.IssueDetails_Info() { RequestSelectionValues = true },
                ContainerQty = new OM.Info(true),
                zMaterialListItemSettings = new OM.MaterialListItemSettings_Info()
                {
                    TypeName = new OM.Info(true),
                    ParentName = new OM.Info(true),
                    QueryName = new OM.Info(true)
                }
            };

            resultStatus = service.Load(serviceData, request, out result);

            if (resultStatus != null && resultStatus.IsSuccess &&
                    result.Environment.ServiceDetails.SelectionValues != null)
            {
                List<TaskMaterial> taskMaterials = new List<TaskMaterial>();
                if (EProcTask.Data != null)
                {
                    OM.NamedSubentityRef taskRef = EProcTask.Data as OM.NamedSubentityRef;
                    
                    var qParams = new OM.QueryParameters()
                    {
                        Parameters = new OM.QueryParameter[]
                        {
                            new OM.QueryParameter("TaskId", taskRef.ID)
                        }
                    };
                    var recordSet = new OM.RecordSet();
                    var svc = new WCF.Services.QueryService(session.CurrentUserProfile);
                    resultStatus = svc.Execute("GetTaskMaterials", qParams, new OM.QueryOptions(), out recordSet);

                    if (resultStatus.IsSuccess && recordSet != null)
                    {
                        DataTable dt = recordSet.GetAsDataTable();
                        if (dt != null && dt.Rows.Count > 0)
                        {       
                            foreach(DataRow row in dt.Rows)
                            {
                                TaskMaterial item = new TaskMaterial();
                                item.ProductId = row.Field<string>("ProductId");
                                item.ProductName = row.Field<string>("ProductName");
                                item.ProductRevision = row.Field<string>("ProductRevision");
                                item.RefDes = row.Field<string>("ReferenceDesignator");
                                item.Qty = Camstar.Util.Utilities.GetDouble(Camstar.Util.Utilities.GetDataRowField(row, "Qty"));
                                item.IsRoR = string.Compare(item.ProductId, Camstar.Util.Utilities.GetDataRowField(row, "RevOfRcdId"), true) == 0;
                                taskMaterials.Add(item);
                            }
                        }
                    }
                }
                List<IssueDetailsEx> issueDetails = new List<IssueDetailsEx>();

                OM.Header[] headers = result.Environment.ServiceDetails.SelectionValues.Headers;
                OM.Row[] rows = result.Environment.ServiceDetails.SelectionValues.Rows;

                int containerQty = 0;
                if (result.Value.ContainerQty != null)
                    containerQty = result.Value.ContainerQty.Value;

                if (headers != null && rows != null)
                {
                    Dictionary<IssueDetailsHeaderEnum, int> columnIndexes = GetColumnIndexes(headers);
                    List<string> issueDetailsIds = new List<string>();

                    for (int i = 0; i < rows.Length;  i++)
                    {
                        string id = Convert.ToString(rows[i].Values[columnIndexes[IssueDetailsHeaderEnum.MaterialListItemIdIndex]]);
                        if (!issueDetailsIds.Contains(id)) // Make sure the IssueDetail has not been already added
                        {
                            issueDetailsIds.Add(id);

                            if (string.IsNullOrEmpty(GetRowValue<string>(rows[i], columnIndexes[IssueDetailsHeaderEnum.PhantomBillIdIndex]))) // Not a phantom bill.
                            {
                                IssueDetailsEx detail = CreateIssueDetail(rows[i], result.Value.zMaterialListItemSettings, columnIndexes, containerQty, result.Value.CalculateQtyRequired.Value);

                                if (detail != null)
                                {
                                    bool add = taskMaterials.Count == 0;
                                    if (taskMaterials.Count > 0)    
                                    {
                                        foreach (TaskMaterial item in taskMaterials)
                                        {
                                            if (!string.IsNullOrEmpty(item.RefDes) && string.Compare(item.RefDes, detail.IssueDetails.ReferenceDesignator.Value, true) == 0)
                                            {
                                                add = true;
                                                if (item.Qty > 0)
                                                {
                                                    detail.IssueDetails.QtyRequired = item.Qty * containerQty;
                                                    detail.IssueDetails.NetQtyRequired = detail.IssueDetails.QtyRequired.Value- detail.IssueDetails?.QtyIssued.Value;
                                                }
                                            }
                                            else if (string.IsNullOrEmpty(item.RefDes) && string.Compare(item.ProductName, detail.IssueDetails.Product.Name, true) == 0 && string.Compare(item.ProductRevision, detail.IssueDetails.Product.Revision, true) == 0)
                                            {
                                                add = true;
                                                if (item.Qty > 0)
                                                {
                                                    detail.IssueDetails.QtyRequired = item.Qty * containerQty;
                                                    detail.IssueDetails.NetQtyRequired = detail.IssueDetails.QtyRequired.Value - detail.IssueDetails.QtyIssued?.Value;
                                                }
                                            }
                                            if (add)
                                                break;
                                        }
                                    }
                                    if (add)
                                        issueDetails.Add(detail);
                                }
                            }
                            else // Is Phantom Bill, need to get child material items.
                            {
                                DateTime? from = GetRowValue<DateTime?>(rows[i], columnIndexes[IssueDetailsHeaderEnum.EffectiveFromDateGMT], (x) => Convert.ToDateTime(x));
                                DateTime? thru = GetRowValue<DateTime?>(rows[i], columnIndexes[IssueDetailsHeaderEnum.EffectiveThruDateGMT], (x) => Convert.ToDateTime(x));

                                string IssueDetailSpecStepId = GetRowValue<string>(rows[i], columnIndexes[IssueDetailsHeaderEnum.SpecIdIndex]);
                                string IssueDetailSpecStepName = GetRowValue<string>(rows[i], columnIndexes[IssueDetailsHeaderEnum.SpecNameIndex]);

                                if (ItemIsAvailable(from, thru, true) && ItemIsOnCorrectSpecStep(result.Value.zMaterialListItemSettings, IssueDetailSpecStepId, IssueDetailSpecStepName, out string notUsed, out string notUsed2))
                                {
                                    int totalQtyRequired = containerQty;
                                    int phantomQtyRequired = GetRowValue<int>(rows[i], columnIndexes[IssueDetailsHeaderEnum.QtyRequiredIndex], (x) => Convert.ToInt32(x));
                                    if (phantomQtyRequired > 0)
                                        totalQtyRequired *= phantomQtyRequired;
                                    List<IssueDetailsEx> phantomDetails = GetPhantomBill(rows[i], result.Value.zMaterialListItemSettings, columnIndexes, new List<string>(), totalQtyRequired, result.Value.CalculateQtyRequired.Value);
                                    issueDetails.AddRange(phantomDetails);
                                }
                            }
                        }
                    }
                }

                details = issueDetails;
                return resultStatus;
            }
            else
            {
                details = new List<IssueDetailsEx>();
                return resultStatus;
            }
        }

        private bool ItemIsOnCorrectSpecStep(
            OM.MaterialListItemSettings settings,
            string issueDetailSpecStepId,               // either Spec or Step ID, depending on settings.TypeName
            string issueDetailSpecStepName,
            out string itemSpecId,
            out string itemStepId)
        {
            bool correctSpecStep = true;
            itemSpecId = "";
            itemStepId = "";

            if (settings.TypeName.Value.Equals("ProductMaterialListItem"))
            {
                itemSpecId = issueDetailSpecStepId;

                string containerSpecId = Page.DataContract.GetValueByName<string>(_ContainerSpecId);
                correctSpecStep =
                       string.IsNullOrEmpty(issueDetailSpecStepId)
                    || string.IsNullOrEmpty(containerSpecId)
                    || issueDetailSpecStepId.Equals(containerSpecId);
            }
            else if (settings.TypeName.Value.Equals("MfgOrderMaterialListItem") || settings.TypeName.Value.Equals("BOMMaterialListItem"))
            {
                itemStepId = issueDetailSpecStepId;

                string containerStepId = Page.DataContract.GetValueByName<string>(_ContainerWorkflowStepId);
                string containerStepName = Page.DataContract.GetValueByName<string>(_ContainerWorkflowStepName);
                correctSpecStep =
                       string.IsNullOrEmpty(issueDetailSpecStepId)
                    //|| string.IsNullOrEmpty(containerStepId   CPR 425820
                    || string.Compare(issueDetailSpecStepId, containerStepId, true) == 0
                    || string.Compare(issueDetailSpecStepName, containerStepName, true) == 0;
            }

            return correctSpecStep;
        }

        private bool ItemIsAvailable(DateTime? from, DateTime? to, bool isGMT)
        {
            DateTime currentTime = isGMT ? DateTime.UtcNow : DateTime.Now;

            // No Effective From and To Times set
            if (from == null && to == null)
                return true;

            // Current Time is between the Effective From and To Times
            if (from != null && to != null && (from <= currentTime && to > currentTime))
                return true;

            // Current Time is after Effective From Time, Effective To Time not set
            if (from != null && to == null && (from <= currentTime))
                return true;

            // Current Time is before Effective To Time, Effective From Time not set
            if (from == null && to != null && (to > currentTime))
                return true;

            return false;
        }

        private IssueDetailsEx CreateIssueDetail(
            OM.Row row,
            OM.MaterialListItemSettings settings,
            Dictionary<IssueDetailsHeaderEnum, int> columnIndexes,
            int containerQty,
            bool calculateQtyRequired)
        {
            // Check EffectiveFrom and EffectiveThru dates to see if valid
            DateTime? from = GetRowValue<DateTime?>(row, columnIndexes[IssueDetailsHeaderEnum.EffectiveFromDateGMT], (x) => Convert.ToDateTime(x));
            DateTime? thru = GetRowValue<DateTime?>(row, columnIndexes[IssueDetailsHeaderEnum.EffectiveThruDateGMT], (x) => Convert.ToDateTime(x));

            string itemSpecId = string.Empty;
            string itemStepId = string.Empty;

            string issueDetailSpecStepId = GetRowValue<string>(row, columnIndexes[IssueDetailsHeaderEnum.SpecIdIndex]);
            string issueDetailSpecStepName = GetRowValue<string>(row, columnIndexes[IssueDetailsHeaderEnum.SpecNameIndex]);
            bool itemAvailable = ItemIsAvailable(from, thru, true);
            bool correctSpecStep = ItemIsOnCorrectSpecStep(settings, issueDetailSpecStepId, issueDetailSpecStepName, out itemSpecId, out itemStepId) || (Mode != PageMode.Light && Mode != PageMode.Advanced);
            if (itemAvailable && correctSpecStep)
            { // if item is within effective from and thru dates and item's spec/route step is the same as the container's current spec/route step
                OM.Enumeration<OM.IssueControlEnum, int> issueControl = (OM.Enumeration<OM.IssueControlEnum, int>)GetRowValue<OM.IssueControlEnum>(row, columnIndexes[IssueDetailsHeaderEnum.IssueControlIndex], (x) => Convert.ToInt32(x));
                OM.IssueDetails detail = new OM.IssueDetails() { IssueControl = issueControl };

                if (issueControl == OM.IssueControlEnum.Serialized)
                    detail = new OM.IssueDetailsSerial() { IssueControl = issueControl };
                else if (issueControl == OM.IssueControlEnum.Bulk)
                    detail = new OM.IssueDetailsBulk() { IssueControl = issueControl };
                else if (issueControl == OM.IssueControlEnum.LotAndStockPoint)
                    detail = new OM.IssueDetailsLotStock() { IssueControl = issueControl };
                else if (issueControl == OM.IssueControlEnum.StockPointOnly)
                    detail = new OM.IssueDetailsStock() { IssueControl = issueControl };
                else if (issueControl == OM.IssueControlEnum.NoTracking)
                    detail = new OM.IssueDetailsQuantity() { IssueControl = issueControl };
                else if (issueControl == OM.IssueControlEnum.CommentOnly)
                    detail = new OM.IssueDetailsDisplayOnly() { IssueControl = issueControl };

                string productName = GetRowValue<string>(row, columnIndexes[IssueDetailsHeaderEnum.ProductIndex]);
                string productRevision = GetRowValue<string>(row, columnIndexes[IssueDetailsHeaderEnum.ProductRevisionIndex]);
                if (!string.IsNullOrEmpty(productName) && !string.IsNullOrEmpty(productRevision))
                    detail.Product = new OM.RevisionedObjectRef(productName, productRevision, "Product");

                detail.ProductDescription = GetRowValue<string>(row, columnIndexes[IssueDetailsHeaderEnum.ProductDescriptionIndex]);
                detail.QtyIssued = GetRowValue<double>(row, columnIndexes[IssueDetailsHeaderEnum.QtyIssuedIndex], (x) => Convert.ToDouble(x, CultureInfo.InvariantCulture));
                //detail.IssueControl = (OM.Enumeration<OM.IssueControlEnum, int>)GetRowValue<OM.IssueControlEnum>(row, columnIndexes[IssueDetailsHeaderEnum.IssueControlIndex], (x) => Convert.ToInt32(x));

                string uomName = GetRowValue<string>(row, columnIndexes[IssueDetailsHeaderEnum.UOMIndex]);
                if (!string.IsNullOrEmpty(uomName))
                    detail.UOM = new OM.NamedObjectRef(uomName, "UOM");

                string assmeblySequence = GetRowValue<string>(row, columnIndexes[IssueDetailsHeaderEnum.AssemblySequenceIndex]);
                if (!string.IsNullOrEmpty(assmeblySequence))
                    detail.AssemblySequence = int.Parse(assmeblySequence);

                detail.QtyRequired = GetRowValue<double>(row, columnIndexes[IssueDetailsHeaderEnum.QtyRequiredIndex], (x) => Convert.ToDouble(x, CultureInfo.InvariantCulture)); // Convert.ToDouble(row.Values[columnIndexes[IssueDetailsHeaderEnum.QtyRequiredIndex]]));
                if (calculateQtyRequired)
                    detail.QtyRequired = detail.QtyRequired.Value * containerQty;
                
                double setupQty = GetRowValue<double>(row, columnIndexes[IssueDetailsHeaderEnum.SetupQtyIndex], (x) => Convert.ToDouble(x, CultureInfo.InvariantCulture));
                detail.QtyRequired = (double)detail.QtyRequired + setupQty;

                OM.BaseObjectRef parentRef = null;

                string parentName = GetRowValue<string>(row, columnIndexes[IssueDetailsHeaderEnum.ParentNameIndex]);
                string parentRevision = GetRowValue<string>(row, columnIndexes[IssueDetailsHeaderEnum.ParentRevisionIndex]);

                if (!string.IsNullOrEmpty(parentName) && settings.ParentName != null && !string.IsNullOrEmpty(settings.ParentName.Value))
                {
                    if (columnIndexes[IssueDetailsHeaderEnum.ParentRevisionIndex] != -1 && !string.IsNullOrEmpty(parentRevision))
                        parentRef = new OM.RevisionedObjectRef(parentName, parentRevision, settings.ParentName.Value);
                    else
                        parentRef = new OM.NamedObjectRef(parentName, settings.ParentName.Value);
                }

                string bomLineItem = GetRowValue<string>(row, columnIndexes[IssueDetailsHeaderEnum.BOMLineItemIndex]);
                if (!string.IsNullOrEmpty(bomLineItem) && settings.TypeName != null && !string.IsNullOrEmpty(settings.TypeName.Value))
                {
                    detail.BOMLineItem = new OM.NamedSubentityRef(bomLineItem, parentRef, settings.TypeName.Value);
                    detail.BOMLineItem.ID = GetRowValue<string>(row, columnIndexes[IssueDetailsHeaderEnum.MaterialListItemIdIndex]);
                }

                detail.NetQtyRequired = new OM.Primitive<double>((double)(Convert.ToDecimal(detail.QtyRequired.Value) - Convert.ToDecimal(detail.QtyIssued.Value)));
                detail.ReferenceDesignator = GetRowValue<string>(row, columnIndexes[IssueDetailsHeaderEnum.ReferenceDesignatorIndex]);

                LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);

                if (detail.IssueControl == OM.IssueControlEnum.Serialized)
                    detail.IssueControlName = labelCache.GetLabelByName("IssueControlEnum_Serialized").Value;
                else if (detail.IssueControl == OM.IssueControlEnum.Bulk)
                    detail.IssueControlName = labelCache.GetLabelByName("IssueControlEnum_Bulk").Value;
                else if (detail.IssueControl == OM.IssueControlEnum.LotAndStockPoint)
                    detail.IssueControlName = labelCache.GetLabelByName("IssueControlEnum_LotAndStockPoint").Value;
                else if (detail.IssueControl == OM.IssueControlEnum.StockPointOnly)
                    detail.IssueControlName = labelCache.GetLabelByName("IssueControlEnum_StockPointOnly").Value;
                else if (detail.IssueControl == OM.IssueControlEnum.NoTracking)
                    detail.IssueControlName = labelCache.GetLabelByName("IssueControlEnum_NoTracking").Value;
                else if (detail.IssueControl == OM.IssueControlEnum.CommentOnly)
                    detail.IssueControlName = labelCache.GetLabelByName("IssueControlEnum_CommentOnly").Value;

                return new IssueDetailsEx()
                {
                    IssueDetails = detail,
                    SpecId = itemSpecId,
                    StepId = itemStepId
                };
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        protected Dictionary<IssueDetailsHeaderEnum, int> GetColumnIndexes(OM.Header[] headers)
        {
            Dictionary<IssueDetailsHeaderEnum, int> columnIndexes = new Dictionary<IssueDetailsHeaderEnum, int>();

            columnIndexes.Add(IssueDetailsHeaderEnum.BOMLineItemIndex, Array.FindIndex(headers, h => h.Name.Equals("MaterialListItem")));
            columnIndexes.Add(IssueDetailsHeaderEnum.MaterialListItemIdIndex, Array.FindIndex(headers, h => h.Name.Equals("MaterialListItemId")));
            columnIndexes.Add(IssueDetailsHeaderEnum.ProductIndex, Array.FindIndex(headers, h => h.Name.Equals("ComponentProduct")));
            columnIndexes.Add(IssueDetailsHeaderEnum.ProductRevisionIndex, Array.FindIndex(headers, h => h.Name.Equals("ProductRevision")));
            columnIndexes.Add(IssueDetailsHeaderEnum.ProductDescriptionIndex, Array.FindIndex(headers, h => h.Name.Equals("ComponentDescription")));
            columnIndexes.Add(IssueDetailsHeaderEnum.QtyRequiredIndex, Array.FindIndex(headers, h => h.Name.Equals("QtyRequired")));
            columnIndexes.Add(IssueDetailsHeaderEnum.QtyIssuedIndex, Array.FindIndex(headers, h => h.Name.Equals("QtyIssued")));
            columnIndexes.Add(IssueDetailsHeaderEnum.UOMIndex, Array.FindIndex(headers, h => h.Name.Equals("UOM")));
            columnIndexes.Add(IssueDetailsHeaderEnum.AssemblySequenceIndex, Array.FindIndex(headers, h => h.Name.Equals("AssemblySequence")));
            columnIndexes.Add(IssueDetailsHeaderEnum.IssueControlIndex, Array.FindIndex(headers, h => h.Name.Equals("IssueControl")));
            columnIndexes.Add(IssueDetailsHeaderEnum.ParentNameIndex, Array.FindIndex(headers, h => h.Name.Equals("ParentName")));
            columnIndexes.Add(IssueDetailsHeaderEnum.ParentRevisionIndex, Array.FindIndex(headers, h => h.Name.Equals("ParentRevision")));
            columnIndexes.Add(IssueDetailsHeaderEnum.PhantomBillIdIndex, Array.FindIndex(headers, h => h.Name.Equals("PhantomBillId")));
            columnIndexes.Add(IssueDetailsHeaderEnum.EffectiveFromDateGMT, Array.FindIndex(headers, h => h.Name.Equals("EffectiveFromDateGMT")));
            columnIndexes.Add(IssueDetailsHeaderEnum.EffectiveThruDateGMT, Array.FindIndex(headers, h => h.Name.Equals("EffectiveThruDateGMT")));
            columnIndexes.Add(IssueDetailsHeaderEnum.SpecIdIndex, Array.FindIndex(headers, h => h.Name.Equals("SpecId")));
            columnIndexes.Add(IssueDetailsHeaderEnum.SetupQtyIndex, Array.FindIndex(headers, h => h.Name.Equals("SetupQty")));
            columnIndexes.Add(IssueDetailsHeaderEnum.ReferenceDesignatorIndex, Array.FindIndex(headers, h => h.Name.Equals("ReferenceDesignator")));
            columnIndexes.Add(IssueDetailsHeaderEnum.SpecNameIndex, Array.FindIndex(headers, h => h.Name.Equals("SpecName")));

            return columnIndexes;
        }

        private T GetRowValue<T>(OM.Row row, int key)
        {
            return GetRowValue<T>(row, key, null);
        }

        private T GetRowValue<T>(OM.Row row, int key, Func<object, object> f)
        {
            T result = default(T);
            try
            {
                object r = null;
                if (key >= 0)
                {
                    if (f != null)
                        r = f(row.Values[key]);
                    else r = row.Values[key];

                    result = (T)r;
                }
            }
            catch { }

            return result;
        }

        /// <summary>
        /// Set data to MaterialsRequirementsGrid
        /// </summary>
        protected virtual void FilterAndBindMaterialReqs(bool sort)
        {
            IEnumerable<IssueDetailsEx> reqs = MaterialRequirementsEx;

            string containerStepId = Page.DataContract.GetValueByName<string>(_ContainerWorkflowStepId);
            string containerSpecId = Page.DataContract.GetValueByName<string>(_ContainerSpecId);

            // filter
            if (HideSatisfiedReq.IsChecked)
                reqs = reqs.Where(r => r.IssueDetails.QtyIssued.Value < (r.IssueDetails.QtyRequired != null ? r.IssueDetails.QtyRequired.Value : 0));

            bool anyStepMatches = reqs.Any(r => r.MatchesStepOrSpecId(containerStepId, containerSpecId));
            // sort
            if (Mode == PageMode.Advanced && (!string.IsNullOrEmpty(containerSpecId) || !string.IsNullOrEmpty(containerStepId)) && anyStepMatches)
            {
                reqs = reqs.Where(r => r.MatchesStepOrSpecIdOrNone(containerStepId, containerSpecId));
            }
            //  We only will sort if there are any items with an 'AssemblySequence' defined and Spec/Step set.  This is to not break existing FF tests that do not expect the results to be re-sorted
            if ((!string.IsNullOrEmpty(containerSpecId) || !string.IsNullOrEmpty(containerStepId)) && reqs.Any(r => r.IssueDetails.AssemblySequence != null && r.IssueDetails.AssemblySequence.Value > 0 && (!string.IsNullOrEmpty(r.SpecId) || !string.IsNullOrEmpty(r.StepId))) && reqs.Any(r => r.MatchesStepOrSpecId(containerStepId, containerSpecId) && r.IssueDetails.AssemblySequence != null))
            {
                reqs =
                    reqs
                    .OrderBy(r => r.MatchesStepOrSpecId(containerStepId, containerSpecId) ? -1 : 0)
                    .ThenBy(r => r.IssueDetails.AssemblySequence == null ? 10000000 : r.IssueDetails.AssemblySequence.Value)
                    .ThenBy(r => r.IssueDetails.ReferenceDesignator == null ? "" : r.IssueDetails.ReferenceDesignator.Value);
            }
            else if (anyStepMatches)
                reqs =
                   reqs
                   .OrderBy(r => r.MatchesStepOrSpecId(containerStepId, containerSpecId) ? -1 : 0)
                   .ThenBy(r => r.IssueDetails.Product.Name)
                   .ThenBy(r => r.IssueDetails.ReferenceDesignator == null ? "" : r.IssueDetails.ReferenceDesignator.Value);
            else if (sort)
                reqs = reqs.OrderBy(r => r.IssueDetails.Product.Name)
                        .ThenBy(r => r.IssueDetails.ReferenceDesignator == null ? "" : r.IssueDetails.ReferenceDesignator.Value);

            MaterialsRequirementGrid.Data = reqs.Select(r => r.IssueDetails).ToArray();
        }
        bool adjustNotRequired = false;
        protected virtual bool IsIssueDifferenceReasonRequired()
        {
            bool result = false;
            adjustNotRequired = false;
            if (IssueDetailsType != IssueDetailsTypeEnum.DisplayOnly &&
                IssueDetailsType != IssueDetailsTypeEnum.Nothing)
            {
                bool res1 = false;
                double issueQtyValue;
                double netQtyValue = ExecuteData != null && ExecuteData.IssueDetails != null ? (double)ExecuteData.IssueDetails.NetQtyRequired : double.NaN;

                if (IssueQty.Data != null && netQtyValue != double.NaN && double.TryParse(IssueQty.Data.ToString(), out issueQtyValue))
                    res1 = issueQtyValue != netQtyValue;
                else
                    res1 = true;

                bool res2 = false;
                double issueQty2Value;
                double netQty2Value = ExecuteData != null && ExecuteData.IssueDetails != null ? (double)ExecuteData.IssueDetails.NetQty2Required : double.NaN;

                if (IssueQty2.Data != null && netQty2Value != double.NaN && double.TryParse(IssueQty2.Data.ToString(), out issueQty2Value))
                    res2 = issueQty2Value != netQty2Value;

                result = res1 || res2;

                if (ExecuteData.IssueDetails.IssueControl == OM.IssueControlEnum.Serialized && ContainerQty.Data != null)
                {
                    double containerQty;
                    if (double.TryParse(ContainerQty.Data.ToString(), out containerQty))
                    {
                        result = ExecuteData.IssueDetails.QtyRequired != containerQty;
                    }
                }
            }

            IssueDifferenceReason.Required = result;

            if (ExecuteData != null && ExecuteData.IssueDetails != null && ExecuteData.IssueDetails.IssueControl == OM.IssueControlEnum.Bulk && ExecuteData.IssueDetails.AdjustmentType != null && ExecuteData.IssueDetails.AdjustmentValue != null)
            {
                bool isRequired = false;
                double issueQtyValue;
                double netQtyValue = ExecuteData != null && ExecuteData.IssueDetails != null ? (double)ExecuteData.IssueDetails.NetQtyRequired : double.NaN;
                double adjustmentPercentage = (double)ExecuteData.IssueDetails.AdjustmentValue / 100;
                double adjustmentQuantity = (double)ExecuteData.IssueDetails.AdjustmentValue;

                if (IssueQty.Data != null && netQtyValue != double.NaN && double.TryParse(IssueQty.Data.ToString(), out issueQtyValue))
                {
                    OM.AdjustmentTypeEnum adjustmentType = (OM.AdjustmentTypeEnum)ExecuteData.IssueDetails.AdjustmentType;
                    switch (adjustmentType)
                    {
                        case OM.AdjustmentTypeEnum.Percentage:
                            isRequired = (issueQtyValue >= netQtyValue * (1 - adjustmentPercentage) && issueQtyValue <= netQtyValue * (1 + adjustmentPercentage) ? false : true);
                            IssueDifferenceReason.Required = isRequired;
                            adjustNotRequired = !isRequired;
                            break;

                        case OM.AdjustmentTypeEnum.Quantity:
                            isRequired = (issueQtyValue >= netQtyValue - adjustmentQuantity && issueQtyValue <= netQtyValue + adjustmentQuantity ? false : true);
                            IssueDifferenceReason.Required = isRequired;
                            adjustNotRequired = !isRequired;
                            break;
                    }
                }
            }

            return result;
        } // IsIssueDifferenceReasonRequired()

        public virtual void IsIssueDifferenceReasonRequired(object sender, EventArgs e)
        {
            IssueDifferenceReason.Visible = IsIssueDifferenceReasonRequired();
            IssueDifferenceReason.Required = !adjustNotRequired && IssueDifferenceReason.Visible && string.IsNullOrEmpty(DefaultIssueDifferenceReason);
            if (!string.IsNullOrEmpty(DefaultIssueDifferenceReason) && IssueDifferenceReason.Visible && IssueDifferenceReason.Data == null)
                IssueDifferenceReason.Data = DefaultIssueDifferenceReason;
            else if (!IssueDifferenceReason.Visible)
                IssueDifferenceReason.ClearData();
        } // IsIssueDifferenceReasonRequired(object sender, EventArgs e)

        protected virtual void SetUserDataEntryLayout(IssueDetailsTypeEnum type)
        {
            ContainerQty.Visible = UOM.Visible = (type == IssueDetailsTypeEnum.Lot || type == IssueDetailsTypeEnum.Serial);
            LotNumber.Visible = type == IssueDetailsTypeEnum.LotAndStockpoint;
            IssueQty.Visible = (type != IssueDetailsTypeEnum.Serial) && (type != IssueDetailsTypeEnum.DisplayOnly) && (type != IssueDetailsTypeEnum.Nothing);
            StockPoint.Visible = (type == IssueDetailsTypeEnum.LotAndStockpoint) || (type == IssueDetailsTypeEnum.Stockpoint);
            IssueDifferenceReason.Visible = IsIssueDifferenceReasonRequired();
            IssueDifferenceReason.Required = !adjustNotRequired && IssueDifferenceReason.Visible && string.IsNullOrEmpty(DefaultIssueDifferenceReason);
            if (!string.IsNullOrEmpty(DefaultIssueDifferenceReason) && IssueDifferenceReason.Visible && IssueDifferenceReason.Data == null)
                IssueDifferenceReason.Data = DefaultIssueDifferenceReason;
            else if (!IssueDifferenceReason.Visible)
                IssueDifferenceReason.ClearData();
            Vendor.Visible = (type == IssueDetailsTypeEnum.LotAndStockpoint || type == IssueDetailsTypeEnum.Stockpoint || type == IssueDetailsTypeEnum.Qty);
            VendorItem.Visible = (type == IssueDetailsTypeEnum.LotAndStockpoint || type == IssueDetailsTypeEnum.Stockpoint || type == IssueDetailsTypeEnum.Qty);
            ScanContainer.Required = ScanContainer.PageFlowRequired = (type == IssueDetailsTypeEnum.Serial || type == IssueDetailsTypeEnum.Lot);
            IssueQty2.Style["display"] = UOM2.Style["display"] = type != IssueDetailsTypeEnum.Serial ? "" : "none";

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
        } // SetUserDataEntryLayout(IssueDetailsTypeEnum type)

        protected virtual void SetSatisfiedWidgetCountControl()
        {
            int satisfied = 0;
            int required = 0;
            if (MaterialRequirements != null && MaterialRequirements.Count() > 0)
            {
                satisfied = MaterialRequirements.Where(item => (item.IssueControl != OM.IssueControlEnum.CommentOnly && item.QtyIssued != null && item.QtyRequired != null) ? ((double)item.QtyIssued >= (double)item.QtyRequired) : false).Count();
                required = MaterialRequirements.Where(item => item.IssueControl != OM.IssueControlEnum.CommentOnly).Count();
            }

            var labelCache = LabelCache.GetRuntimeCacheInstance();
            var label = labelCache.GetLabelByName("Lbl_SatisfiedOf");
            SatisfiedWidgetCount.Data = string.Format(label.Value, satisfied, required);
        } // SetSatisfiedWidgetCountControl(ComponentIssue componentIssue)

        protected virtual void SetFocusToControl(IssueDetailsTypeEnum type)
        {
            switch (type)
            {
                case IssueDetailsTypeEnum.Stockpoint:
                    {
                        Page.SetFocus(IssueQty.ClientID);   //  Why not StockPoint?
                        break;
                    }
                case IssueDetailsTypeEnum.Qty:
                    {
                        Page.SetFocus(IssueQty.ClientID);
                        break;
                    }
                case IssueDetailsTypeEnum.Lot:
                case IssueDetailsTypeEnum.Serial:
                    {
                        if (ScanContainer.Data == null)
                        {
                            Page.SetFocus(ScanContainer.ClientID);
                        }
                        else
                        {
                            Page.SetFocus(IssueDifferenceReason.ClientID);
                        }
                        break;
                    }
                case IssueDetailsTypeEnum.LotAndStockpoint:
                    {
                        Page.SetFocus(LotNumber.ClientID);
                        break;
                    }
                case IssueDetailsTypeEnum.Nothing:
                    {
                        Page.SetFocus(ScanContainer.ClientID);
                        break;
                    }
            }
            //Performance issue
            //Page.RenderToClient = true;
        } // SetIssueQtyControl(IssueDetailsTypeEnum type)

        protected virtual void SetIssueQtyControl(IssueDetailsTypeEnum type, OM.IssueDetails issueDetails)
        {
            switch (type)
            {
                case IssueDetailsTypeEnum.Lot:
                    {
                        double containerQty;
                        double containerQty2;

                        if (issueDetails.NetQtyRequired != null && ContainerQty.Data != null && double.TryParse(ContainerQty.Data.ToString(), out containerQty))
                            IssueQty.Data = (double)issueDetails.NetQtyRequired >= containerQty ? containerQty : issueDetails.NetQtyRequired;

                        if (issueDetails.NetQty2Required != null && ContainerQty2 != null && double.TryParse(ContainerQty2.ToString(), out containerQty2))
                            IssueQty2.Data = (double)issueDetails.NetQty2Required >= containerQty2 ? containerQty2 : issueDetails.NetQty2Required;

                        break;
                    }
                case IssueDetailsTypeEnum.Serial:
                    {
                        double containerQty;
                        double containerQty2;

                        if (ContainerQty.Data != null && double.TryParse(ContainerQty.Data.ToString(), out containerQty))
                            IssueQty.Data = containerQty;

                        if (ContainerQty2 != null && double.TryParse(ContainerQty2.ToString(), out containerQty2))
                            IssueQty2.Data = containerQty2;

                        break;
                    }
                case IssueDetailsTypeEnum.LotAndStockpoint:
                case IssueDetailsTypeEnum.Stockpoint:
                case IssueDetailsTypeEnum.Qty:
                case IssueDetailsTypeEnum.DisplayOnly:
                    {
                        IssueQty.Data = issueDetails.NetQtyRequired;
                        IssueQty2.Data = issueDetails.NetQty2Required;
                        break;
                    }
            }
        } // SetIssueQtyControl(IssueDetailsTypeEnum type, IssueDetails issueDetails)        

        protected virtual OM.IssueActualDetail CreateIssueActualDetail()
        {
            var issueActualDetail = new OM.IssueActualDetail();

            if (ExecuteData != null && ExecuteData.IssueDetails != null)
            {
                double qtyIssued, qty2Issued;

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
            // TODO - Page.ClearValues does this?
            Page.DataContract.SetValueByName(_ContainerSpecId, "");
            Page.DataContract.SetValueByName(_ContainerWorkflowStepId, "");
            Page.DataContract.SetValueByName("SelectedContainerNameDM", null);

        } // ClearPageData()

        protected virtual void ClearGridData()
        {
            MaterialsRequirementGrid.ClearData();
            MaterialsRequirementGrid.OriginalData = null;
            MaterialsRequirementGrid.GridContext.CurrentPage = 1;
            GridCurrentPage = 1;
            SetSatisfiedWidgetCountControl();
        } // ClearGridData()

        protected virtual void SetSelectedRow(OM.NamedSubentityRef bOMLineItem)
        {
            if (MaterialsRequirementGrid.Data != null)
            {
                mkIsNeedClearControls = false;
                MaterialsRequirementGrid.SelectedRowID = GetRowIdByBOMLineItem(bOMLineItem);
            }
        } // SetSelectedRow(NamedSubentityRef bOMLineItem)

        protected virtual string GetRowIdByBOMLineItem(OM.NamedSubentityRef bOMLineItem)
        {
            string bOMLineItemStr = bOMLineItem.Name;
            string rowID = (MaterialsRequirementGrid.GridContext as SubentityDataContext).GetRowIdByCellValue(mkBOMLineItemColumn, bOMLineItemStr);
            if (string.IsNullOrEmpty(rowID))
            {
                OM.IssueDetails[] gridData = MaterialsRequirementGrid.Data as OM.IssueDetails[];
                for (int i = 0; i < gridData.Length; i++)
                {
                    OM.IssueDetails item = gridData[i];
                    if (string.Compare(bOMLineItem.Name, item.BOMLineItem.Name, true) == 0)
                    {
                        int page = i / MaterialsRequirementGrid.GridContext.RowsPerPage;
                        MaterialsRequirementGrid.GridContext.CurrentPage = page + 1;
                        GridCurrentPage = page + 1;
                        MaterialsRequirementGrid.GridContext.LoadData();
                        rowID = (MaterialsRequirementGrid.GridContext as SubentityDataContext).GetRowIdByCellValue(mkBOMLineItemColumn, bOMLineItemStr);
                        MaterialsRequirementGrid.BoundContext.SelectRow(rowID, true);
                        //this.WebPartManager.RenderToClient(MaterialRequirementsWP);
                        //CamstarWebControl.SetRenderToClient(MaterialsRequirementGrid);
                    }
                }
            }
            return rowID;
        } // GetRowIdByBOMLineItem(NamedSubentityRef bOMLineItem)

        protected virtual void ClearUserDataEntryAreaControls()
        {
            Product.ClearData();
            ProductDescription.ClearData();
            if (IssueControl != null)
                IssueControl.ClearData();
            NetQtyRequired.ClearData();
            NetQtyRequired.OriginalData = null;
            ScanContainer.ClearData();
            ContainerQty.ClearData();
            UOM.ClearData();
            LotNumber.ClearData();
            IssueQty.ClearData();
            IssueQty.OriginalData = null;
            IssueDifferenceReason.ClearData();
            StockPoint.ClearData();
            UOM2.ClearData();
            IssueReason.ClearData();
            SubstitutionReason.ClearData();
            Comments.ClearData();
            if (IssueQty2.Visible)
            {
                IssueQty2.ClearData();
                IssueQty2.OriginalData = null;
            }
            ExecuteData = null;
        } // ClearUserDataEntryAreaControls()      

        #endregion

        protected virtual void ClearAllAction()
        {
            ClearPageData();
            ClearGridData();
            ClearUserDataEntryAreaControls();
            SetUserDataEntryLayout(IssueDetailsType);
        } // ClearAllAction()

        #region Fields

        private bool mkIsNeedClearControls = true;

        #endregion

        #region Constants
        protected const string _ContainerSpecId = "ContainerSpecId";
        protected const string _ContainerSpecName = "ContainerSpecName";
        protected const string _ContainerWorkflowStepId = "ContainerWorkflowStepId";
        protected const string _ContainerWorkflowStepName = "ContainerWorkflowStepName";

        protected const string mkIssueStatusColumn = "IssueStatus";
        protected const string mkQtyIssuedColumn = "QtyIssued";
        protected const string mkDisplayOnlyQtyIssuedValue = "---";
        protected const string mkNetQtyRequiredColumn = "NetQtyRequired";
        protected const string mkBOMLineItemColumn = "BOMLineItem";
        protected const string mkContainerQty2 = "ContainerQty2";
        protected const string mkDefaultIssueDifferenceReason = "DefaultIssueDifferenceReason";
        protected const string mkComponentIssueExecuteData = "ComponentIssueExecuteData";
        private const string mkComponentIssueExecuteDataList = "ComponentIssueExecuteDataList";
        private const string mkComponentIssueExecuteDataListNonSerial = "ComponentIssueExecuteDataListNonSerial";
        protected const string mkServiceDetailsStorageKey = "ServiceDetailsStorage";
        protected const string MaterialRequirementsExKey = "MaterialRequirementsExKey";
        protected const string GridCurrentPageKey = "GridCurrentPageKey";
        protected enum IssueDetailsHeaderEnum
        {
            BOMLineItemIndex,
            MaterialListItemIdIndex,
            ProductIndex,
            ProductRevisionIndex,
            ProductDescriptionIndex,
            QtyRequiredIndex,
            QtyIssuedIndex,
            UOMIndex,
            IssueControlIndex,
            AssemblySequenceIndex,
            ParentNameIndex,
            ParentRevisionIndex,
            PhantomBillIdIndex,
            EffectiveFromDateGMT,
            EffectiveThruDateGMT,
            SpecIdIndex,
            SetupQtyIndex,
            ReferenceDesignatorIndex,
            SpecNameIndex
        }

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

    /// <summary>
    /// Adds SpecId and tepId (Could not override/extend IssueDetails)
    /// </summary>
    public class IssueDetailsEx
    {
        public string StepId { get; set; }
        public string SpecId { get; set; }

        public OM.IssueDetails IssueDetails { get; set; }

        public bool HasSpecOrStepId()
        {
            return !string.IsNullOrEmpty(this.StepId) || !string.IsNullOrEmpty(this.SpecId);
        }

        public bool MatchesStepOrSpecId(string stepId, string specId)
        {
            return MatchesStepOrSpecId(stepId, false) || MatchesStepOrSpecId(specId, false);
        }

        public bool MatchesStepOrSpecIdOrNone(string stepId, string specId)
        {
            return MatchesStepOrSpecId(stepId, true) || MatchesStepOrSpecId(specId, true);
        }

        public bool MatchesStepOrSpecId(string stepOrSpecId, bool matchEmpty)
        {
            return (matchEmpty && !HasSpecOrStepId()) || (!string.IsNullOrEmpty(stepOrSpecId) && (StepId == stepOrSpecId || SpecId == stepOrSpecId));
        }

    }
}
