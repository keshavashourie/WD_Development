// Copyright Siemens 2023 
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using CamstarPortal.WebControls.Accordion;
using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CWGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.MfgAuditTrailR2
{
    public class PanelFiltersR2 : MatrixWebPart
    {
        #region Events

        public event EventHandler SearchClicked;

        #endregion

        #region Controls

        protected virtual CWC.TextBox QOName
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_Name") as CWC.TextBox; }
        } // QOName

        protected virtual CWC.ContainerList HiddenContainerField
        {
            get { return Page.FindCamstarControl("HiddenContainerField") as CWC.ContainerList; }
        } // HiddenContainerField

        protected virtual CWC.DropDownList RecordType
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_RecordType") as CWC.DropDownList; }
        } // RecordType

        protected virtual CWC.NamedObject Views
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_Views") as CWC.NamedObject; }
        } // Views

        protected virtual CWC.RadioButton ShowAllAvailable
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_ShowAllAvailable") as CWC.RadioButton; }
        } // ShowAllAvailable

        protected virtual CWC.RadioButton ShowDateRange
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_ShowDateRange") as CWC.RadioButton; }
        } // ShowDateRange

        protected virtual CWC.DateChooser StartDateChooser
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_StartDate") as CWC.DateChooser; }
        } // StartDate

        protected virtual CWC.DateChooser EndDateChooser
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_EndDate") as CWC.DateChooser; }
        } // EndDate

        protected virtual CWC.CheckBox SortDescending
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_SortDescending") as CWC.CheckBox; }
        } // SortAscending

        protected virtual CWC.RadioButton ShowAllTypes
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_ShowAllTypes") as CWC.RadioButton; }
        } // ShowAllTypes

        protected virtual CWC.RadioButton SelectTypes
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_SelectTypes") as CWC.RadioButton; }
        } // SelectTypes

        protected virtual Camstar.WebPortal.FormsFramework.WebGridControls.MultiSelectPickList TransactionTypes
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_TransactionTypes") as Camstar.WebPortal.FormsFramework.WebGridControls.MultiSelectPickList; }
        } // TransactionTypes

        protected virtual CWC.CheckBox ShowReversalInfo
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_ShowReversalInfo") as CWC.CheckBox; }
        } // ShowReversalInfo

        protected virtual CWC.CheckBox SelectCurrent
        {
            get { return Page.FindCamstarControl("CurrentSecondaryFilter") as CWC.CheckBox; }
        } // SelectCurrent

        protected virtual CWC.NamedObject OperationFilter
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_OperationFilter") as CWC.NamedObject; }
        } // OperationFilter

        protected virtual CWC.NamedObject EmployeeFilter
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_EmployeeFilter") as CWC.NamedObject; }
        } // EmployeeFilter

        protected virtual CWC.NamedObject ResourceFilter
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_ResourceFilter") as CWC.NamedObject; }
        } // ResourceFilter

        protected virtual CWGC.JQDataGrid ContainerStatusGrid
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_ContainerStatus") as CWGC.JQDataGrid; }
        } // ContainerStatusGrid

        protected virtual CWGC.JQDataGrid QualityObjectStatusGrid
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_QualityObjectStatus") as CWGC.JQDataGrid; }
        } // QualityObjectStatusGrid

        protected virtual CWGC.JQDataGrid ContainerAttributesGrid
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_ContainerAttributes") as CWGC.JQDataGrid; }
        } // QualityObjectStatusGrid

        protected virtual CWGC.JQDataGrid EventLogGrid
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_EventLog") as CWGC.JQDataGrid; }
        } // QualityObjectStatusGrid

        protected virtual CWGC.JQDataGrid MainLineGrid
        {
            get { return Page.FindCamstarControl("MfgAuditTrailInquiry_MainLineGrid") as CWGC.JQDataGrid; }
        } // MainLineGrid

        protected virtual JQTabContainer FilterTabs
        {
            get { return FindCamstarControl("PanelFiltersTabs") as JQTabContainer; }
        } // FilterTabs

        protected virtual JQTabContainer ControlTabs
        {
            get { return Page.FindCamstarControl("ControlTabs") as JQTabContainer; }
        } // FilterTabs

        protected virtual CWC.Button SearchTxn
        {
            get { return Page.FindCamstarControl("SearchTxnBtn") as CWC.Button; }
        } // SearchTxn

        protected virtual CWC.TextBox DateTimeFormat
        {
            get { return Page.FindCamstarControl("DateTimeFormat") as CWC.TextBox; }
        } // SearchTxn

        protected virtual Accordion Accordion
        {
            get { return Page.FindIForm("CollapsibleSectionsAccordion") as Accordion; }
        }

        protected virtual string CurrentCallStackKey { get; set; } // CurrentCallStackKey

        protected virtual OM.MfgAuditTrailInquiry AuditTrailInquiry
        {
            get
            {
                var session = new CallStack(CurrentCallStackKey).Context.LocalSession;
                OM.MfgAuditTrailInquiry resultList = null;

                if (session != null)
                {
                    if (session[mkAuditTrailInquiryKey] == null)
                        session[mkAuditTrailInquiryKey] = new OM.MfgAuditTrailInquiry();

                    resultList = session[mkAuditTrailInquiryKey] as OM.MfgAuditTrailInquiry;
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

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            CurrentCallStackKey = Page.CallStackKey;
            Page.OnRequestFormSelectionValues += new EventHandler<FormProcessingEventArgs>(Page_OnRequestFormSelectionValues);
            Page.OnRequestControlSelectionValues += new EventHandler<SelectionControlProcessingEventArgs>(Page_OnRequestControlSelectionValues);
            Page.OnGetSelectionData += Page_OnGetSelectionData;
            Page.OnRequestFormValues += Page_OnRequestFormValues;
            Page.OnClearValues += Page_OnClearValues;
        } // void OnInit(EventArgs e)       

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            isAutoExecute = (Page.DataContract.GetValueByName("AddContainerDM") != null);

            RecordType.DataChanged += RecordType_DataChanged;
            SelectCurrent.DataChanged += SelectCurrent_DataChanged;
            ShowAllAvailable.DataChanged += ShowDateRange_DataChanged;
            ShowDateRange.DataChanged += ShowDateRange_DataChanged;
            SearchTxn.Click += new EventHandler(PanelFilters_Click);

            if (!Page.IsPostBack)
            {
                LoadDefaltValues(new OM.MfgAuditTrailInquiry_Info
                {
                    RecordType = new OM.Info(true),
                    HistoryView = new OM.Info(true)
                });
            }
            SetLayoutPanelFilter();
        } // void OnLoad(EventArgs e)

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            isAutoExecute = (Page.DataContract.GetValueByName("AddContainerDM") != null);

            var shortDatePattern = "";
            var longTimePattern = "";

            var uiCulture = Array.Find(System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.SpecificCultures), culture => culture.DisplayName == Page.Culture);
            if (uiCulture != null)
            {
                shortDatePattern = uiCulture.DateTimeFormat.ShortDatePattern;
                longTimePattern = uiCulture.DateTimeFormat.LongTimePattern;
            }

            var cultureSettings = CamstarPortalSection.Settings.CurrentCultureSettings;
            if (cultureSettings.DateTimeFormat != null)
            {
                if (!string.IsNullOrEmpty(cultureSettings.DateTimeFormat.ShortDatePattern))
                {
                    shortDatePattern = cultureSettings.DateTimeFormat.ShortDatePattern;
                }
                if (!string.IsNullOrEmpty(cultureSettings.DateTimeFormat.LongTimePattern))
                {
                    longTimePattern = cultureSettings.DateTimeFormat.LongTimePattern;
                }
            }

            if (!string.IsNullOrEmpty(shortDatePattern) && !string.IsNullOrEmpty(longTimePattern))
            {
                var dateTimeFormat = String.Format("{0} {1}", shortDatePattern, longTimePattern);
                DateTimeFormat.Data = dateTimeFormat;
            }

            QOName.Attributes.Add(ClientEvents.onkeypress.ToString(), string.Format(mkEnterClickEvent, SearchTxn.ClientID));

            if (isAutoExecute && !Page.IsPostBack)
            {

                PanelFilters_Click(SearchTxn, e);

                var data = new OM.MfgAuditTrailInquiry
                {
                    Container = new OM.ContainerRef(QOName.Data.ToString()),
                    Carrier = null,
                    QualityObject = null,
                    OperationFilter = (OM.NamedObjectRef)OperationFilter.Data,
                    EmployeeFilter = (OM.NamedObjectRef)EmployeeFilter.Data,
                    ResourceFilter = (OM.NamedObjectRef)ResourceFilter.Data,
                    RecordType = (OM.HistoryRecordTypeEnum)RecordType.Data,
                    HistoryView = (OM.NamedObjectRef)Views.Data,
                    ShowAllAvailable = (bool)ShowAllAvailable.Data,
                    ShowAllTxnTypes = (bool)ShowAllTypes.Data,
                    ShowFullHistory = true,
                    SortAscending = !(bool)SortDescending.Data,
                    ShowReversalInfo = (bool)ShowReversalInfo.Data,
                    DateTimeFormat = DateTimeFormat.Data != null ? DateTimeFormat.Data.ToString() : ""
                };

                var request = new MfgAuditTrailInquiry_Request
                {
                    Info = new OM.MfgAuditTrailInquiry_Info
                    {
                        SelectedHistoryMainline = FieldInfoUtil.RequestSelectionValue(),
                        Container = FieldInfoUtil.RequestSelectionValue(),

                        ContainerAttributes = new OM.UserAttribute_Info
                        {
                            Name = FieldInfoUtil.RequestValue(),
                            AttributeValue = FieldInfoUtil.RequestValue()
                        }
                    }
                };
                
                var service = new MfgAuditTrailInquiryService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);

                MfgAuditTrailInquiry_Result result;
                if (request.Info.Container.RequestSelectionValuesInfo == null)
                    request.Info.Container.RequestSelectionValuesInfo = new SelectionValuesInfo();
                if (request.Info.Container.RequestSelectionValuesInfo.Options == null)
                    request.Info.Container.RequestSelectionValuesInfo.Options = new QueryOptions();
                request.Info.Container.RequestSelectionValuesInfo.Options.RequestRecordSetAndCount = true;
                OM.ResultStatus rs = service.GetEnvironment(data, request, out result);
                if (rs.IsSuccess)
                {
                    OM.RecordSet selectionValues = result.Environment.SelectedHistoryMainline.SelectionValues;
                    MainLineGrid.SetSelectionValues(selectionValues);

                    selectionValues = result.Environment.Container.SelectionValues;
                    ContainerStatusGrid.SetSelectionValues(selectionValues);

                    if (result.Value.ContainerAttributes != null)
                    {
                        ContainerAttributesGrid.Data = result.Value.ContainerAttributes;
                    }

                }
                isAutoExecute = false;
                Page.DataContract.SetValueByName("AddContainerDM", null);
            }

            if (this.MainLineGrid != null)
            {
                ScriptManager.RegisterStartupScript(Page.Form, Page.Form.GetType(), "SearchLayoutFunctions", string.Format("SearchLayout_AddSlideoutToogler('{0}');", this.MainLineGrid.ClientID), true);
            }
            if (this.Breadcrumb != null)
            {
                string startupScript = "setTimeout(function(){initFiltersPanel('" + SearchTxn.ClientID + "', '" + Breadcrumb.ClientID + "')}, 300);";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "filtersPanelStartup", startupScript, true);
            }
        } // void OnPreRender(EventArgs e)

        protected virtual void OnSearchClicked(object sender, EventArgs e)
        {
            (ContainerStatusGrid.GridContext as DataGridContext).ClearCache();
            Page.StatusBar.ClearMessage();

            ResetMainlineGridRowSelection();
            ShouldResetDetailTabs = true;

            if (SearchClicked != null)
                SearchClicked(sender, e);

        } // OnSearchClicked(object sender, EventArgs e)

        protected virtual void RecordType_DataChanged(object sender, EventArgs e)
        {
            if (isClearValues && RecordType.Data != null)
            {
                var recordType = RecordType.Data;
                Page.ClearValues();
                RecordType.Data = recordType;

                Accordion.ExpandedSections = "0";
            }
            isClearValues = false;
        } // RecordType_OnDataChanged(object sender, EventArgs e)

        protected virtual void SelectCurrent_DataChanged(object sender, EventArgs e)
        {
            if (SelectCurrent.IsChecked && !AuditTrailInquiry.IsEmpty)
            {
                var inquiryData = AuditTrailInquiry;
                inquiryData.SelectCurrentOperation = true;

                LoadDefaltValues(
                    inquiryData,
                    new OM.MfgAuditTrailInquiry_Info
                    {
                        OperationFilter = new OM.Info(true)
                    });
                OperationFilter.Enabled = false;
            }
            else
            {
                OperationFilter.ClearData();
                EmployeeFilter.ClearData();
                ResourceFilter.ClearData();

                OperationFilter.Enabled = true;
                EmployeeFilter.Enabled = true;
                ResourceFilter.Enabled = true;
            }
        } // void SelectCurrent_DataChanged(object sender, EventArgs e)

        protected virtual void PanelFilters_Click(object sender, EventArgs e)
        {
            MainLineGrid.ClearData();
            var inquiry = new OM.MfgAuditTrailInquiry();
            if (RecordType.Data != null && QOName.Data != null)
                switch ((OM.HistoryRecordTypeEnum)RecordType.Data)
                {
                    case OM.HistoryRecordTypeEnum.Container:
                        {
                            inquiry.Container = new OM.ContainerRef(QOName.Data.ToString());
                            inquiry.Carrier = null;
                            inquiry.QualityObject = null;
                            break;
                        }
                    case OM.HistoryRecordTypeEnum.ContainerInCarrier:
                        {
                            inquiry.Carrier = new OM.NamedObjectRef(QOName.Data.ToString());
                            inquiry.Container = null;
                            inquiry.QualityObject = null;
                            break;
                        }
                    default:
                        {
                            inquiry.QualityObject = new OM.NamedObjectRef(QOName.Data.ToString(), Enum.GetName(typeof(OM.HistoryRecordTypeEnum), RecordType.Data));
                            inquiry.Carrier = null;
                            inquiry.Container = null;
                            OperationFilter.ClearData();
                            ResourceFilter.ClearData();
                            EmployeeFilter.ClearData();
                            break;
                        }
                }

            if (RecordType.Data != null)
                inquiry.RecordType = (OM.HistoryRecordTypeEnum)RecordType.Data;

            inquiry.HistoryView = (OM.NamedObjectRef)Views.Data;

            inquiry.ShowAllAvailable = (bool)ShowAllAvailable.Data;
            if (!(bool)inquiry.ShowAllAvailable)
            {

                inquiry.StartDate = StartDateChooser.Data != null ? new OM.Primitive<DateTime>((DateTime)StartDateChooser.Data) : null;
                inquiry.EndDate = EndDateChooser.Data != null ? new OM.Primitive<DateTime>((DateTime)EndDateChooser.Data) : null;
            }

            inquiry.ShowAllTxnTypes = (bool)ShowAllTypes.Data;
            if (!(bool)inquiry.ShowAllTxnTypes)
            {
                var wcf = new WCFUtilities.WCFObject(inquiry);
                TransactionTypes.CustomDataSetter(TransactionTypes, TransactionTypes.FieldExpressions, wcf);
            }

            inquiry.ShowFullHistory = true;
            inquiry.SortAscending = !(bool)SortDescending.Data;
            inquiry.ShowReversalInfo = (bool)ShowReversalInfo.Data;

            inquiry.OperationFilter = (OM.NamedObjectRef)OperationFilter.Data;
            inquiry.EmployeeFilter = (OM.NamedObjectRef)EmployeeFilter.Data;
            inquiry.ResourceFilter = (OM.NamedObjectRef)ResourceFilter.Data;

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
                LoadDefaltValues(new OM.MfgAuditTrailInquiry_Info
                {
                    StartDate = new OM.Info(true),
                    EndDate = new OM.Info(true)
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
            if (e.Data is OM.MfgAuditTrailInquiry && !AuditTrailInquiry.IsEmpty)
            {
                var inquiry = AuditTrailInquiry;
                var resultInquiry = (OM.MfgAuditTrailInquiry)e.Data;

                resultInquiry.Container = inquiry.Container;
                resultInquiry.Carrier = inquiry.Carrier;
                resultInquiry.QualityObject = inquiry.QualityObject;
            }
        } // void Page_OnRequestFormValues(object sender, FormProcessingEventArgs e)

        protected virtual void Page_OnRequestFormSelectionValues(object sender, FormProcessingEventArgs e)
        {
            if (Page.ProcessingContext.Status == ProcessingStatusType.None)
                isSetInfoObject = false;
        } // void Page_OnRequestFormSelectionValues(object sender, FormProcessingEventArgs e)

        protected virtual void Page_OnRequestControlSelectionValues(object sender, SelectionControlProcessingEventArgs e)
        {
            if (e.Control.ID == OperationFilter.ID ||
                e.Control.ID == ResourceFilter.ID ||
                e.Control.ID == EmployeeFilter.ID)
            {
                var inquiry = AuditTrailInquiry;
                var resultInquiry = (OM.MfgAuditTrailInquiry)e.Data;

                resultInquiry.Container = inquiry.Container;
                resultInquiry.RecordType = inquiry.RecordType;
                resultInquiry.HistoryView = inquiry.HistoryView;
                resultInquiry.ShowAllAvailable = inquiry.ShowAllAvailable;
                resultInquiry.StartDate = inquiry.StartDate;
                resultInquiry.EndDate = inquiry.EndDate;
                resultInquiry.ShowAllTxnTypes = inquiry.ShowAllTxnTypes;
                resultInquiry.ShowTxnTypes = inquiry.ShowTxnTypes;
                resultInquiry.ShowFullHistory = inquiry.ShowFullHistory;
                resultInquiry.SortAscending = inquiry.SortAscending;
                resultInquiry.ShowReversalInfo = inquiry.ShowReversalInfo;
            }

        } // void Page_OnRequestControlSelectionValues(object sender, FormProcessingEventArgs e)

        protected virtual void Page_OnGetSelectionData(object sender, FormsFramework.ServiceDataEventArgs e)
        {
            if (isSetInfoObject && e.Data is OM.MfgAuditTrailInquiry && !AuditTrailInquiry.IsEmpty)
            {
                var inquiry = AuditTrailInquiry;
                var resultInquiry = (OM.MfgAuditTrailInquiry)e.Data;

                resultInquiry.Container = inquiry.Container;
                resultInquiry.Carrier = inquiry.Carrier;
                resultInquiry.QualityObject = inquiry.QualityObject;
                resultInquiry.RecordType = inquiry.RecordType;
                resultInquiry.HistoryView = inquiry.HistoryView;
                resultInquiry.ShowAllAvailable = inquiry.ShowAllAvailable;
                resultInquiry.StartDate = inquiry.StartDate;
                resultInquiry.EndDate = inquiry.EndDate;
                resultInquiry.ShowAllTxnTypes = inquiry.ShowAllTxnTypes;
                resultInquiry.ShowTxnTypes = inquiry.ShowTxnTypes;
                resultInquiry.ShowFullHistory = inquiry.ShowFullHistory;
                resultInquiry.SortAscending = inquiry.SortAscending;
                resultInquiry.ShowReversalInfo = inquiry.ShowReversalInfo;
                resultInquiry.OperationFilter = inquiry.OperationFilter;
                resultInquiry.EmployeeFilter = inquiry.EmployeeFilter;
                resultInquiry.ResourceFilter = inquiry.ResourceFilter;
            }
            isSetInfoObject = true;
        } // void Page_OnGetSelectionData(object sender, ServiceDataEventArgs e)        

        protected virtual void Page_OnClearValues(object sender, FormsFramework.ServiceDataEventArgs e)
        {

            ShowAllAvailable.Data = !(bool)(ShowDateRange.Data = false);
            ShowAllTypes.Data = !(bool)(SelectTypes.Data = false);
            ShowReversalInfo.Data = false;
            SortDescending.Data = true;
            SelectCurrent.Data = false;
            AuditTrailInquiry = null;
            LoadDefaltValues(new OM.MfgAuditTrailInquiry_Info
            {
                RecordType = new OM.Info(true),
                HistoryView = new OM.Info(true)
            });

            if (!isAutoExecute)
            {
                SetSecondaryFilters(false);

                QOName.Data = null;
                HiddenContainerField.Data = null;
            }
            ResetMainlineGridRowSelection();
            ShouldResetDetailTabs = true;

            Accordion.ExpandedSections = "0";

        } // void Page_OnClearValues(object sender, ServiceDataEventArgs e)

        protected virtual void SetLayoutPanelFilter()
        {
            OM.HistoryRecordTypeEnum recordType;
            if (RecordType.Data != null && Enum.TryParse(RecordType.Data.ToString(), true, out recordType))
            {
                var attributeTab = FilterTabs.Tabs.OfType<JQTabPanel>().FirstOrDefault(item => mkAttributesTab.Equals(item.Name));
                var eventLogTab = FilterTabs.Tabs.OfType<JQTabPanel>().FirstOrDefault(item => mkEventLogTab.Equals(item.Name));

                switch (recordType)
                {
                    case OM.HistoryRecordTypeEnum.Container:
                    case OM.HistoryRecordTypeEnum.ContainerInCarrier:
                        {
                            QualityObjectStatusGrid.Visible = !(ContainerStatusGrid.Visible = true);

                            SetSecondaryFilters(true);

                            SearchClicked += delegate (object sender, EventArgs e) { SetSecondaryFilters(true); };

                            if (attributeTab != null)
                                attributeTab.Visible = true;


                            if (eventLogTab != null)
                                eventLogTab.Visible = false;

                            break;
                        }
                    default:
                        {
                            ContainerStatusGrid.Visible = !(QualityObjectStatusGrid.Visible = true);
                            SetSecondaryFilters(false);

                            if (eventLogTab != null)
                                eventLogTab.Visible = true;
                            if (attributeTab != null)
                                attributeTab.Visible = false;

                            break;
                        }
                }
            }

            // Performance Issue
            this.RenderToClient = true;
        } // void SetLayoutPanelFilter()

        protected virtual void LoadDefaltValues(OM.MfgAuditTrailInquiry_Info serviceInfo)
        {
            LoadDefaltValues(null, serviceInfo);
        } // void LoadDefaltValues(MfgAuditTrailInquiry_Info serviceInfo)

        protected virtual void LoadDefaltValues(OM.MfgAuditTrailInquiry serviceData, OM.MfgAuditTrailInquiry_Info serviceInfo)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.MfgAuditTrailInquiryService(session.CurrentUserProfile);

            var request = new Camstar.WCF.Services.MfgAuditTrailInquiry_Request
            {
                Info = serviceInfo
            };

            var result = new Camstar.WCF.Services.MfgAuditTrailInquiry_Result();
            var resultStatus = new OM.ResultStatus();

            if (serviceData != null)
                resultStatus = service.GetEnvironment(serviceData, request, out result);
            else
                resultStatus = service.GetEnvironment(request, out result);

            if (resultStatus.IsSuccess)
                Page.DisplayValues(result.Value);
        } // void LoadDefaltValues(MfgAuditTrailInquiry serviceData, MfgAuditTrailInquiry_Info serviceInfo)

        protected virtual void SetSecondaryFilters(bool isVisible)
        {
            if ((!Page.IsPostBack || AuditTrailInquiry.IsEmpty) && !isAutoExecute)
                isVisible = false;

            OperationFilter.Visible =
            EmployeeFilter.Visible =
            ResourceFilter.Visible =
            SelectCurrent.Visible = isVisible;
            //ControlTabs.RaisePostBackEvent("0"); // for TransactionsPanelR2
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
        protected const string mkSearchTab = "SearchTab";

        protected const string mkAuditTrailInquiryKey = "AuditTrailInquiryKey";

        protected const string mkEnterClickEvent = "if ( event.keyCode == 13 ) $('#{0}')[0].click();";

        #endregion
    }
}
