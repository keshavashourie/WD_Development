// Copyright Siemens 2023  
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using CamstarPortal.WebControls.Accordion;
using System;
using System.Web;
using System.Web.UI;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CWGC = Camstar.WebPortal.FormsFramework.WebGridControls;

namespace Camstar.WebPortal.WebPortlets.ResourceAuditTrail
{
    public class PanelFiltersR2 : MatrixWebPart
    {
        #region Events

        public event EventHandler SearchClicked;

        #endregion

        #region Controls

        protected MatrixWebPart PanelFiltersSearchWP 
        { 
            get { return Page.FindCamstarControl("PanelFiltersSearchWP") as MatrixWebPart; } 
        }
        protected virtual CWC.TitleControl SecondaryFilter
        {
            get { return Page.FindCamstarControl("SecondaryFiltersTitle") as CWC.TitleControl; }
        } // SecondaryFilter

        protected virtual CWC.RadioButton ShowAllAvailable
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_ShowAllAvailable") as CWC.RadioButton; }
        } // ShowAllAvailable

        protected virtual CWC.RadioButton ShowDateRange
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_ShowDateRange") as CWC.RadioButton; }
        } // ShowDateRange

        protected virtual CWC.DateChooser StartDateChooser
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_StartDate") as CWC.DateChooser; }
        } // StartDate

        protected virtual CWC.DateChooser EndDateChooser
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_EndDate") as CWC.DateChooser; }
        } // EndDate

        protected virtual CWC.CheckBox SortDescending
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_SortDescending") as CWC.CheckBox; }
        } // SortAscending

        protected virtual CWC.DropDownList TransactionFilter
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_TransactionFilter") as CWC.DropDownList; }
        } // TransactionFilter

        protected virtual Camstar.WebPortal.FormsFramework.WebGridControls.MultiSelectPickList TransactionTypes
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_ShowTxnTypes") as Camstar.WebPortal.FormsFramework.WebGridControls.MultiSelectPickList; }
        } // TransactionTypes

        protected virtual CWC.CheckBox ShowReversalInfo
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_ShowReversalInfo") as CWC.CheckBox; }
        } // ShowReversalInfo

        protected virtual CWC.NamedObject EmployeeFilter
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_EmployeeFilter") as CWC.NamedObject; }
        } // EmployeeFilter

        protected virtual CWC.DropDownList AvailabilityFilter
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_AvailabilityFilter") as CWC.DropDownList; }
        } // AvailabilityFilter

        protected virtual CWC.NamedObject ResourceStatusCodeFilter
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_ResourceStatusCodeFilter") as CWC.NamedObject; }
        } // ResourceStatusCodeFilter

        protected virtual CWC.NamedObject ResourceStatusReasonFilter
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_ResourceStatusReasonFilter") as CWC.NamedObject; }
        } // ResourceStatusCodeFilter

        protected virtual CWC.NamedObject Resource
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_Resource") as CWC.NamedObject; }
        } // Resource

        protected virtual CWC.NamedObject Views
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_Views") as CWC.NamedObject; }
        } // Views

        protected virtual CWC.NamedObject ResourceType
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_ResourceType") as CWC.NamedObject; }
        } // ResourceType

        protected virtual CWC.NamedObject ResourceFamily
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_ResourceFamily") as CWC.NamedObject; }
        } // ResourceFamily

        protected virtual CWC.NamedObject ResourceGroup
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_ResourceGroup") as CWC.NamedObject; }
        } // ResourceGroup

        protected virtual CWGC.JQDataGrid ResourceStatusGrid
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_ResourceStatus") as CWGC.JQDataGrid; }
        } // ResourceStatusGrid

        protected virtual CWGC.JQDataGrid MainLineGrid
        {
            get { return Page.FindCamstarControl("ResourceAuditTrailInquiry_MainLineGrid") as CWGC.JQDataGrid; }
        } // MainLineGrid


        protected virtual JQTabContainer FilterTabs
        {
            get { return FindCamstarControl("PanelFiltersTabs") as JQTabContainer; }
        } // FilterTabs

        protected virtual CWC.Button SearchTxn
        {
            get { return Page.FindCamstarControl("SearchTxnBtn") as CWC.Button; }
        } // SearchTxn

        protected virtual string CurrentCallStackKey { get; set; } // CurrentCallStackKey

        protected virtual ResourceAuditTrailInquiry AuditTrailInquiry
        {
            get
            {
                var session = new CallStack(CurrentCallStackKey).Context.LocalSession;
                ResourceAuditTrailInquiry resultList = null;

                if (session != null)
                {
                    if (session[mkAuditTrailInquiryKey] == null)
                        session[mkAuditTrailInquiryKey] = new ResourceAuditTrailInquiry();

                    resultList = session[mkAuditTrailInquiryKey] as ResourceAuditTrailInquiry;
                }

                return resultList;
            }
            set
            {
                var session = new CallStack(CurrentCallStackKey).Context.LocalSession;

                if (session != null)
                    session[mkAuditTrailInquiryKey] = value;
            }
        } // AuditTrailInquiry

        protected virtual CWC.Breadcrumb Breadcrumb
        {
            get { return Page.FindCamstarControl("BreadCrumb") as CWC.Breadcrumb; }
        }

        #endregion

        #region Protected properties

        protected string ShouldResetDetailTabsKey
        {
            get { return $"AuditTrailInquiry_ShouldResetDetailTabs_{Page.CallStackKey}"; }
        }

        protected bool ShouldResetDetailTabs
        {
            get
            {
                bool? result = Page.PortalContext.LocalSession[ShouldResetDetailTabsKey] as bool?;
                if (result != null)
                { return result.Value; }
                else { return false; }
            }
            set
            {
                Page.PortalContext.LocalSession[ShouldResetDetailTabsKey] = value;
            }
        }
        #endregion

        #region Protected methods

        public override void RequestSelectionValues(Info serviceInfo, Service serviceData)
        {
            base.RequestSelectionValues(serviceInfo, serviceData);
            if (!Page.IsPostBack)
            {
                (serviceInfo as ResourceAuditTrailInquiry_Info).AvailabilityFilter = new Info(false, false);
                (serviceInfo as ResourceAuditTrailInquiry_Info).ResourceStatusCodeFilter = new Info(false, false);
                (serviceInfo as ResourceAuditTrailInquiry_Info).EmployeeFilter = new Info(false, false);
                (serviceInfo as ResourceAuditTrailInquiry_Info).ResourceStatusReasonFilter = new Info(false, false);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            CurrentCallStackKey = Page.CallStackKey;
            Page.OnRequestFormSelectionValues += Page_OnRequestFormSelectionValues;
            Page.OnRequestControlSelectionValues += Page_OnRequestControlSelectionValues;
            Page.OnGetSelectionData += Page_OnGetSelectionData;
            Page.OnRequestFormValues += Page_OnRequestFormValues;
            Page.OnClearValues += Page_OnClearValues;
        } // void OnInit(EventArgs e)       

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            isAutoExecute = (Page.DataContract.GetValueByName("AddContainerDM") != null);

            TransactionFilter.DataChanged += TransactionFilterOnDataChanged;
            ShowAllAvailable.DataChanged += ShowDateRange_DataChanged;
            ShowDateRange.DataChanged += ShowDateRange_DataChanged;
            SearchTxn.Click += PanelFilters_Click;
            Resource.DataChanged += RequestFilterValues;

            if (!Page.IsPostBack)
            {
                LoadDefaltValues(new ResourceAuditTrailInquiry_Info
                {
                    RecordType = new Info(true),
                    HistoryView = new Info(true)
                });
            }

            SetLayoutPanelFilter();
        }

        protected virtual void RequestFilterValues(object sender, EventArgs eventArgs)
        {
            AvailabilityFilter.Data = null;
            ResourceStatusCodeFilter.Data = null;
            ResourceStatusReasonFilter.Data = null;
            EmployeeFilter.Data = null;

            var info = new ResourceAuditTrailInquiry_Info
            {
                AvailabilityFilter = new Info(false, true),
                ResourceStatusCodeFilter = new Info(false, true),
                ResourceStatusReasonFilter = new Info(false, true),
                EmployeeFilter = new Info(false, true)
            };
            var data = new ResourceAuditTrailInquiry();
            FormProcessor.RequestSelectionValues(info, data);
            SetSecondaryFilters(false);
        }

        // void OnLoad(EventArgs e)

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (isAutoExecute && !Page.IsPostBack)
            {

                PanelFilters_Click(SearchTxn, e);

                var data = new ResourceAuditTrailInquiry
                {
                    Resource = (NamedObjectRef)Resource.Data,
                    AvailabilityFilter = (ResourceAvailabilityEnum)AvailabilityFilter.Data,
                    EmployeeFilter = (NamedObjectRef)EmployeeFilter.Data,
                    ResourceStatusCodeFilter = (NamedObjectRef)ResourceStatusCodeFilter.Data,
                    ResourceStatusReasonFilter = (NamedObjectRef)ResourceStatusReasonFilter.Data,
                    ShowAllAvailable = (bool)ShowAllAvailable.Data,
                    TransactionFilter = (ResourceAuditTrailTypeEnum?)TransactionFilter.Data ?? ResourceAuditTrailTypeEnum.AllTransactionTypes,
                    ShowFullHistory = true,
                    SortAscending = !(bool)SortDescending.Data,
                    ShowReversalInfo = (bool)ShowReversalInfo.Data,
                    HistoryView = (NamedObjectRef)Views.Data
                };

                var request = new ResourceAuditTrailInquiry_Request
                {
                    Info = new ResourceAuditTrailInquiry_Info
                    {
                        SelectedHistoryMainline = FieldInfoUtil.RequestSelectionValue(),
                        Resource = FieldInfoUtil.RequestSelectionValue()
                    }
                };
                var service = new ResourceAuditTrailInquiryService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);

                ResourceAuditTrailInquiry_Result result;
                if (request.Info.Resource.RequestSelectionValuesInfo == null)
                    request.Info.Resource.RequestSelectionValuesInfo = new SelectionValuesInfo();
                if (request.Info.Resource.RequestSelectionValuesInfo.Options == null)
                    request.Info.Resource.RequestSelectionValuesInfo.Options = new QueryOptions();
                request.Info.Resource.RequestSelectionValuesInfo.Options.RequestRecordSetAndCount = true;
                var rs = service.GetEnvironment(data, request, out result);
                if (rs.IsSuccess)
                {
                    var selectionValues = result.Environment.SelectedHistoryMainline.SelectionValues;
                    MainLineGrid.SetSelectionValues(selectionValues);

                    selectionValues = result.Environment.Resource.SelectionValues;
                    ResourceStatusGrid.SetSelectionValues(selectionValues);
                }
                isAutoExecute = false;
                Page.DataContract.SetValueByName("AddContainerDM", null);
            }
            TransactionFilter.PickListPanelControl.Style.Add("width", "300px");
            (TransactionTypes.PickListPanelControl.ViewControl as CWGC.JQDataGrid).GridContext.Width = 240;

            if (this.MainLineGrid != null)
            {
                ScriptManager.RegisterStartupScript(Page.Form, Page.Form.GetType(), "SearchLayoutFunctions", string.Format("SearchLayout_AddSlideoutToogler('{0}');", this.MainLineGrid.ClientID), true);
            }

            if (this.Breadcrumb != null)
            {
                string startupScript = "setTimeout(function(){initFiltersPanel('" + SearchTxn.ClientID + "', '" + Breadcrumb.ClientID + "')}, 300);";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "filtersPanelStartup", startupScript, true);
            }
            bool showSecondary = Resource.Data != null && this.MainLineGrid != null && MainLineGrid.Data != null;
            string script = "setTimeout(function(){showSecondaryFilters(" + showSecondary.ToString().ToLower() + ")}, 300);";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "showSecondaryFilters", script, true);

        } // void OnPreRender(EventArgs e)

        protected virtual void OnSearchClicked(object sender, EventArgs e)
        {
            Page.StatusBar.ClearMessage();
            ResetMainlineGridRowSelection();
            ShouldResetDetailTabs = true;

            if (SearchClicked != null)
                SearchClicked(sender, e);
        } // OnSearchClicked(object sender, EventArgs e)

        protected virtual void TransactionFilterOnDataChanged(object sender, EventArgs eventArgs)
        {

            if (TransactionFilter.Data != null &&
                (ResourceAuditTrailTypeEnum)TransactionFilter.Data == ResourceAuditTrailTypeEnum.SelectTransactionTypes)
                TransactionTypes.Enabled = true;
            else
            {
                TransactionTypes.Enabled = false;
                TransactionTypes.ClearData();
            }
        }

        protected virtual void PanelFilters_Click(object sender, EventArgs e)
        {
            MainLineGrid.ClearData();
            var inquiry = new ResourceAuditTrailInquiry();

            inquiry.Resource = Resource.Data as NamedObjectRef;

            inquiry.ShowAllAvailable = (bool)ShowAllAvailable.Data;
            if (!(bool)inquiry.ShowAllAvailable)
            {
                inquiry.StartDate = StartDateChooser.Data != null ? new Primitive<DateTime>((DateTime)StartDateChooser.Data) : null;
                inquiry.EndDate = EndDateChooser.Data != null ? new Primitive<DateTime>((DateTime)EndDateChooser.Data) : null;
            }

            if (TransactionFilter.Data != null)
                inquiry.TransactionFilter = (ResourceAuditTrailTypeEnum)TransactionFilter.Data;
            if (inquiry.TransactionFilter == ResourceAuditTrailTypeEnum.SelectTransactionTypes)
            {
                var wcf = new WCFUtilities.WCFObject(inquiry);
                TransactionTypes.CustomDataSetter(TransactionTypes, TransactionTypes.FieldExpressions, wcf);
            }
            inquiry.HistoryView = (NamedObjectRef)Views.Data;
            inquiry.ShowFullHistory = true;
            inquiry.SortAscending = !(bool)SortDescending.Data;
            inquiry.ShowReversalInfo = (bool)ShowReversalInfo.Data;
            if (AvailabilityFilter.Data != null)
                inquiry.AvailabilityFilter = (ResourceAvailabilityEnum)AvailabilityFilter.Data;
            inquiry.ResourceStatusCodeFilter = (NamedObjectRef)ResourceStatusCodeFilter.Data;
            inquiry.EmployeeFilter = (NamedObjectRef)EmployeeFilter.Data;
            inquiry.ResourceStatusReasonFilter = (NamedObjectRef)ResourceStatusReasonFilter.Data;

            var validateResult = Page.ValidateInputData(inquiry);

            if (validateResult.IsSuccess)
            {
                AuditTrailInquiry = inquiry;
                OnSearchClicked(sender, e);
            }
            else
                Page.DisplayMessage(validateResult);

        } // void PanelFilters_Click(object sender, EventArgs e)

        protected virtual void ShowDateRange_DataChanged(object sender, EventArgs e)
        {
            if (ShowDateRange.RadioControl.Checked)
            {
                LoadDefaltValues(new ResourceAuditTrailInquiry_Info
                {
                    StartDate = new Info(true),
                    EndDate = new Info(true)
                });
            }
            else
            {
                StartDateChooser.ClearData();
                EndDateChooser.ClearData();
            }
        } // void ShowDateRange_DataChanged(object sender, EventArgs e)

        protected virtual void Page_OnRequestFormValues(object sender, FormsFramework.FormProcessingEventArgs e)
        {
            var data = e.Data as ResourceAuditTrailInquiry;
            if (data != null && !AuditTrailInquiry.IsEmpty)
            {
                var inquiry = AuditTrailInquiry;
                var resultInquiry = data;

                resultInquiry.Resource = inquiry.Resource;
            }
        } // void Page_OnRequestFormValues(object sender, FormProcessingEventArgs e)

        protected virtual void Page_OnRequestFormSelectionValues(object sender, FormProcessingEventArgs e)
        {
            if (Page.ProcessingContext.Status == ProcessingStatusType.None)
                isSetInfoObject = false;
        } // void Page_OnRequestFormSelectionValues(object sender, FormProcessingEventArgs e)

        protected virtual void Page_OnRequestControlSelectionValues(object sender, SelectionControlProcessingEventArgs e)
        {
            if (e.Control.ID == AvailabilityFilter.ID ||
                e.Control.ID == ResourceStatusCodeFilter.ID ||
                e.Control.ID == EmployeeFilter.ID ||
                e.Control.ID == ResourceStatusReasonFilter.ID)
            {
                var inquiry = AuditTrailInquiry;
                var resultInquiry = (ResourceAuditTrailInquiry)e.Data;

                resultInquiry.Resource = inquiry.Resource;
                resultInquiry.ShowAllAvailable = inquiry.ShowAllAvailable;
                resultInquiry.StartDate = inquiry.StartDate;
                resultInquiry.EndDate = inquiry.EndDate;
                resultInquiry.TransactionFilter = inquiry.TransactionFilter;
                resultInquiry.ShowTxnTypes = inquiry.ShowTxnTypes;
                resultInquiry.ShowFullHistory = inquiry.ShowFullHistory;
                resultInquiry.SortAscending = inquiry.SortAscending;
                resultInquiry.ShowReversalInfo = inquiry.ShowReversalInfo;
                resultInquiry.HistoryView = inquiry.HistoryView;

                isSetInfoObject = false;
            }

        } // void Page_OnRequestControlSelectionValues(object sender, FormProcessingEventArgs e)

        protected virtual void Page_OnGetSelectionData(object sender, FormsFramework.ServiceDataEventArgs e)
        {
            if (isSetInfoObject && e.Data is ResourceAuditTrailInquiry && !AuditTrailInquiry.IsEmpty)
            {
                var inquiry = AuditTrailInquiry;
                var resultInquiry = (ResourceAuditTrailInquiry)e.Data;

                resultInquiry.Resource = inquiry.Resource;
                resultInquiry.ShowAllAvailable = inquiry.ShowAllAvailable;
                resultInquiry.StartDate = inquiry.StartDate;
                resultInquiry.EndDate = inquiry.EndDate;
                resultInquiry.TransactionFilter = inquiry.TransactionFilter;
                resultInquiry.ShowTxnTypes = inquiry.ShowTxnTypes;
                resultInquiry.ShowFullHistory = inquiry.ShowFullHistory;
                resultInquiry.SortAscending = inquiry.SortAscending;
                resultInquiry.ShowReversalInfo = inquiry.ShowReversalInfo;
                resultInquiry.AvailabilityFilter = inquiry.AvailabilityFilter;
                resultInquiry.ResourceStatusCodeFilter = inquiry.ResourceStatusCodeFilter;
                resultInquiry.EmployeeFilter = inquiry.EmployeeFilter;
                resultInquiry.ResourceStatusReasonFilter = inquiry.ResourceStatusReasonFilter;
                resultInquiry.HistoryView = inquiry.HistoryView;
            }
            isSetInfoObject = true;
        } // void Page_OnGetSelectionData(object sender, ServiceDataEventArgs e)        

        protected virtual void Page_OnClearValues(object sender, ServiceDataEventArgs e)
        {
            var accordion = Page.FindIForm("CollapsibleSectionsAccordion") as Accordion;

            ShowAllAvailable.Data = !(bool)(ShowDateRange.Data = false);
            TransactionFilter.Data = (int)ResourceAuditTrailTypeEnum.AllTransactionTypes;
            ShowReversalInfo.Data = false;
            SortDescending.Data = true;
            AuditTrailInquiry = null;
            LoadDefaltValues(new ResourceAuditTrailInquiry_Info
            {
                HistoryView = new Info(true)
            });

            if (!isAutoExecute)
            {
                SetSecondaryFilters(false);
            }
            ResetMainlineGridRowSelection();
            ShouldResetDetailTabs = true;

            accordion.ExpandedSections = "0";
        } // void Page_OnClearValues(object sender, ServiceDataEventArgs e)

        protected virtual void SetLayoutPanelFilter()
        {
            SetSecondaryFilters(true);
            SearchClicked += delegate { SetSecondaryFilters(true); };

            this.RenderToClient = true;
        } // void SetLayoutPanelFilter()

        protected virtual void LoadDefaltValues(ResourceAuditTrailInquiry_Info serviceInfo)
        {
            LoadDefaltValues(null, serviceInfo);
        } // void LoadDefaltValues(ResourceAuditTrailInquiry_Info serviceInfo)

        protected virtual void LoadDefaltValues(ResourceAuditTrailInquiry serviceData, ResourceAuditTrailInquiry_Info serviceInfo)
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new ResourceAuditTrailInquiryService(session.CurrentUserProfile);

            var request = new ResourceAuditTrailInquiry_Request
            {
                Info = serviceInfo
            };

            var result = new ResourceAuditTrailInquiry_Result();
            var resultStatus = new ResultStatus();

            if (serviceData != null)
                resultStatus = service.GetEnvironment(serviceData, request, out result);
            else
                resultStatus = service.GetEnvironment(request, out result);

            if (resultStatus.IsSuccess)
                Page.DisplayValues(result.Value);
        } // void LoadDefaltValues(ResourceAuditTrailInquiry serviceData, ResourceAuditTrailInquiry_Info serviceInfo)

        protected virtual void SetSecondaryFilters(bool isVisible)
        {
            if ((!Page.IsPostBack || AuditTrailInquiry.IsEmpty) && !isAutoExecute)
                isVisible = false;

        } // void SetSecondaryFilters(bool isVisible)

        #endregion

        #region Public methods        

        #endregion

        #region Private methods        

        private void ResetMainlineGridRowSelection()
        {
            MainLineGrid.BoundContext.SelectedItem = null;
            MainLineGrid.BoundContext.SelectedRowID = null;
            MainLineGrid.BoundContext.SelectedRowIDs = null;
            MainLineGrid.BoundContext.SelectedRowIndex = null;
        }
        #endregion
        #region Protected fields

        protected bool isSetInfoObject = true;

        protected bool isClearValues = true;

        protected bool isAutoExecute = false;

        #endregion

        #region Constants

        protected const string mkAttributesTab = "AttributesTab";
        protected const string mkEventLogTab = "EventLogTab";

        protected const string mkAuditTrailInquiryKey = "AuditTrailInquiryKey";

        protected const string mkEnterClickEvent = "if ( event.keyCode == 13 ) $('#{0}')[0].click();";

        #endregion
    }
}
