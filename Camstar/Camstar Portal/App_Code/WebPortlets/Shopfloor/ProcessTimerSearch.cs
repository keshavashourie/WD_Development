//Copyright Siemens 2023
using System;
using System.Windows.Threading;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.PortalFramework;
using System.Text.RegularExpressions;
using System.Globalization;

using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using PERS = Camstar.WebPortal.Personalization;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{

    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    /// 
    public class ProcessTimerSearch : MatrixWebPart
    {
        #region Controls

        //use this region to make properties that reference controls on the Portal page using Page.FindCamstarControl. 
        //Example:
        //private CWC.RevisionedObject MaintenanceReqField
        //{
        //    get { return Page.FindCamstarControl("ResourceActivation_MaintenanceReq") as CWC.RevisionedObject; }
        //}
        protected virtual ProcessTimerInquiryDetail[] processTimerSearchResult
        {
            get
            {
                ProcessTimerInquiryDetail[] processTimerSearchResult = Page.DataContract.GetValueByName<ProcessTimerInquiryDetail[]>(processTimerSearchDetail);
                return processTimerSearchResult ?? new ProcessTimerInquiryDetail[0];
            }
            set
            {
                Page.DataContract.SetValueByName(processTimerSearchDetail, value);
            }

        }

        protected virtual CWC.Button SearchButton { get { return Page.FindCamstarControl("SearchButton") as CWC.Button; } }
        protected virtual CWC.TextBox ContainerFilter { get { return Page.FindCamstarControl("ProcessTimerInquiry_ContainerFilter") as CWC.TextBox; } }
        protected virtual CWC.NamedObject MfgOrderFilter { get { return Page.FindCamstarControl("ProcessTimerInquiry_MfgOrderFilter") as CWC.NamedObject; } }
        protected virtual CWC.NamedObject ProcessTimerTypeFilter { get { return Page.FindCamstarControl("ProcessTimerInquiry_ProcessTimerTypeFilter") as CWC.NamedObject; } }
        protected virtual CWC.RevisionedObject ProcessTimerFilter { get { return Page.FindCamstarControl("ProcessTimerInquiry_ProcessTimerFilter") as CWC.RevisionedObject; } }
        protected virtual CWC.RevisionedObject ProductFilter { get { return Page.FindCamstarControl("ProcessTimerInquiry_ProductFilter") as CWC.RevisionedObject; } }
        protected virtual CWC.RevisionedObject SpecFilter { get { return Page.FindCamstarControl("ProcessTimerInquiry_SpecFilter") as CWC.RevisionedObject; } }
        protected virtual CWC.DropDownList CurrentStatus { get { return Page.FindCamstarControl("ProcessTimerInquiry_CurrentStatus") as CWC.DropDownList; } }
        protected virtual CWC.DropDownList CompletionStatus { get { return Page.FindCamstarControl("ProcessTimerInquiry_CompletionStatus") as CWC.DropDownList; } }
        protected virtual CWC.TextBox ShowTimers { get { return Page.FindCamstarControl("ProcessTimerInquiry_ShowTimers") as CWC.TextBox; } }
        protected virtual CWC.DateChooser StartTimerBeginTimestamp { get { return Page.FindCamstarControl("ProcessTimerInquiry_StartTimerBeginGMT") as CWC.DateChooser; } }
        protected virtual CWC.DateChooser StartTimerEndTimestamp { get { return Page.FindCamstarControl("ProcessTimerInquiry_StartTimerEndGMT") as CWC.DateChooser; } }
        protected virtual CWC.DateChooser EndTimerBeginTimestamp { get { return Page.FindCamstarControl("ProcessTimerInquiry_EndTimerBeginGMT") as CWC.DateChooser; } }
        protected virtual CWC.DateChooser EndTimerEndTimestamp { get { return Page.FindCamstarControl("ProcessTimerInquiry_EndTimerEndGMT") as CWC.DateChooser; } }
        protected virtual JQDataGrid SearchResults { get { return Page.FindCamstarControl("ProcessTimerInquiry_ProcessTimerDetails") as JQDataGrid; } }
        protected virtual CWC.TextBox EndTimeGMT { get { return Page.FindCamstarControl("EndTimeGMT") as CWC.TextBox; } }
        protected virtual CWC.TextBox ParentContainer { get { return Page.FindCamstarControl("ParentContainer") as CWC.TextBox; } }
        protected virtual CWC.RevisionedObject TimerStartedonSpec { get { return Page.FindCamstarControl("ProcessTimerInquiry_TimerStartedonSpec") as CWC.RevisionedObject; } }
        protected virtual CWC.RevisionedObject TimerEndsonSpec { get { return Page.FindCamstarControl("ProcessTimerInquiry_TimerEndsonSpec") as CWC.RevisionedObject; } } 

        protected virtual ActionsControl ProcessTimerActions
        {
            get { return Page.FindIForm("ActionsControl") as ActionsControl; }
        }

        protected virtual CWC.Button ClearAllButton
        {
            get { return Page.FindCamstarControl("ClearAllButton") as CWC.Button; }
        }
        #endregion

        #region Protected Functions

        /// <summary>
        /// TODO: Summary Description of function
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SearchButton.Click += new EventHandler(SearchProcessTimer);
            SearchResults.RowSelected += SearchResults_RowSelected;
            SearchResults.GridContext.Sorting = string.Empty;
            ClearAllButton.Click += ClearAll;
            EndTimeGMT.DataChanged += EndTimeGMT_DataChanged;
            ParentContainer.DataChanged += ParentContainer_DataChanged;
            (SearchResults.GridContext as ItemDataContext).SnapCompleted += SearchResults_SnapCompleted;

        }

        private void ParentContainer_DataChanged(object sender, EventArgs e)
        {
            if (ParentContainer.Data != null)
            {
                ProcessTimerActions.Hidden = true;
            }
        }

        protected virtual void SearchResults_SnapCompleted(System.Data.DataTable dataWindowTable)
        {
            var profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            if (profile != null)
            {

                var fields = new[] { "MinEndTimeGMT", "MinEndWarningTimeGMT", "MaxEndWarningTimeGMT", "MaxEndTimeGMT", "StartTimeGMT", "EndTimeGMT" };
                TimersSupport.AdjustGridSnapData(dataWindowTable, profile.UTCOffset, fields);
            }
        }

        protected virtual ResponseData SearchResults_RowSelected(object sender, JQGridEventArgs args)
        {

            if (EndTimeGMT.Data == null && EndTimeGMT.IsEmpty && ParentContainer.Data == null && ParentContainer.IsEmpty)
            {
                ProcessTimerActions.Hidden = false;
                ProcessTimerActions.RenderToClient = true;
            }
            return null;
        }

        protected virtual void EndTimeGMT_DataChanged(object sender, EventArgs e)
        {
            if (EndTimeGMT.Data != null)
            {
                ProcessTimerActions.Hidden = true;
            }
                
        }

        protected override void OnPreRender(EventArgs e)
        {

            //Call for Grid Reload
            if (isReload)
            {
                SearchProcessTimer(this, e);
                ProcessTimerActions.Hidden = true;
                isReload = false;
            }

            //When CurrentStatus value is InProcess the CompletionStatus will be disabled.
            if (CurrentStatus.Data != null && (TimerCurrentStatusEnum)CurrentStatus.Data == TimerCurrentStatusEnum.InProcess)
            {
                CompletionStatus.ClearData();
                CompletionStatus.Enabled = false;
            }
            else if (CurrentStatus.IsEmpty)
            {
                CompletionStatus.Enabled = true;
                CompletionStatus.ReadOnly = false;
            }

            base.OnPreRender(e);
            //Javascript for ProcessTimerSearch Filter section
            ScriptManager.RegisterStartupScript(Page.Form, Page.Form.GetType(), "ProcessTimerSearchFunctions", "ProcessTimerSearch_Filter();", true);
        }

        // After StopTimer has been executed successfully the grid will be reloaded
        public override void ChildPostExecute(WCF.ObjectStack.ResultStatus status, WCF.ObjectStack.Service serviceData)
        {
            base.ChildPostExecute(status, serviceData);

            if (status.IsSuccess)
            {
                isReload = true;
            }
        }

        // Search button Click Event
        protected virtual void SearchProcessTimer(object sender, EventArgs e)
        {
            ClearGridData();

            if (HttpContext.Current != null)
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                if (session != null)
                {
                    
                    var service = new ProcessTimerInquiryService(session.CurrentUserProfile);

                    //// Set up parameters for the service here
                    var serviceData = new ProcessTimerInquiry()
                    {
                        ContainerFilter = ContainerFilter.Data as string,
                        MfgOrderFilter = MfgOrderFilter.Data as NamedObjectRef,
                        ProcessTimerTypeFilter = ProcessTimerTypeFilter.Data as NamedObjectRef,
                        ProcessTimerFilter = ProcessTimerFilter.Data as RevisionedObjectRef,
                        ProductFilter = ProductFilter.Data as RevisionedObjectRef,
                        SpecFilter = SpecFilter.Data as RevisionedObjectRef,
                        CurrentStatus = CurrentStatus.Data != null ? new Enumeration<TimerCurrentStatusEnum, int>((int)CurrentStatus.Data) : null,
                        CompletionStatus = CompletionStatus.Data != null ? new Enumeration<TimerCompletionStatusEnum, int>((int)CompletionStatus.Data) : null,
                        ShowTimers = ShowTimers.Data != null ? new Primitive<int>((int)ShowTimers.Data) : null,
                        EndTimerEndGMT = EndTimerEndTimestamp.Data != null ? new Primitive<DateTime>((DateTime)EndTimerEndTimestamp.Data) : null,
                        EndTimerBeginGMT = EndTimerBeginTimestamp.Data != null ? new Primitive<DateTime>((DateTime)EndTimerBeginTimestamp.Data) : null,
                        StartTimerEndGMT = StartTimerEndTimestamp.Data != null ? new Primitive<DateTime>((DateTime)StartTimerEndTimestamp.Data) : null,
                        StartTimerBeginGMT = StartTimerBeginTimestamp.Data != null ? new Primitive<DateTime>((DateTime)StartTimerBeginTimestamp.Data) : null,
                        TimerStartedonSpec = TimerStartedonSpec.Data as RevisionedObjectRef,
                        TimerEndsonSpec = TimerEndsonSpec.Data as RevisionedObjectRef
                    };


                    var request = new ProcessTimerInquiry_Request()
                    {
                        Info = new ProcessTimerInquiry_Info()
                        {
                            ProcessTimerDetails = new ProcessTimerInquiryDetail_Info { RequestValue = true }
                        }
                    };

                    var result = new ProcessTimerInquiry_Result();

                    ResultStatus resultStatus = service.GetTimers(serviceData, request, out result);
                    if (resultStatus != null && resultStatus.IsSuccess)
                    {
                        var searchResults = result.Value.ProcessTimerDetails;
                        if (searchResults != null)
                        {
                            var srList =
                                         from searchResult in searchResults 
                                         select new ProcessTimerInquiryDetail
                                         {
                                             ActualElapse = searchResult.ActualElapse == null ? null : GetTimeSpan(searchResult.ActualElapse.Value),
                                             CompletionStatus = searchResult.CompletionStatus == null ? null : new Enumeration<TimerCompletionStatusEnum, int>((int)searchResult.CompletionStatus.Value),
                                             CompletionStatusName = searchResult.CompletionStatusName == null ? null : searchResult.CompletionStatusName.Value,                                             
                                             ContainerName = searchResult.ContainerName == null ? null : searchResult.ContainerName.Value,
                                             EndTimeGMT = searchResult.EndTimeGMT == null ? (DateTime?)null : searchResult.EndTimeGMT.Value,
                                             MaxTime = searchResult.MaxTime == null ? null : GetTimeSpan(searchResult.MaxTime.Value),
                                             MinTime = searchResult.MinTime == null ? null : GetTimeSpan(searchResult.MinTime.Value),
                                             MinTimeColor = searchResult.MinTimeColor == null ? null : searchResult.MinTimeColor,
                                             MaxTimeColor = searchResult.MaxTimeColor == null ? null : searchResult.MaxTimeColor,
                                             ParentContainer = searchResult.ParentContainer == null ? null : searchResult.ParentContainer, 
                                             ProcessTimer = searchResult.ProcessTimer == null ? null : new RevisionedObjectRef(searchResult.ProcessTimer.CDOTypeName),
                                             ProcessTimerName = searchResult.ProcessTimerName == null ? null : searchResult.ProcessTimerName.Value,
                                             ProcessTimerRev = searchResult.ProcessTimerRev == null ? null : searchResult.ProcessTimerRev.Value,
                                             Product = searchResult.Product == null ? null : new RevisionedObjectRef(searchResult.Product.CDOTypeName),
                                             ProductName = searchResult.ProductName == null ? null : searchResult.ProductName.Value,
                                             ProductRev = searchResult.ProductRev == null ? null : searchResult.ProductRev.Value,
                                             Spec = searchResult.Spec == null ? null : new RevisionedObjectRef(searchResult.Spec.CDOTypeName),
                                             SpecName = searchResult.SpecName == null ? null : searchResult.SpecName.Value,
                                             SpecRev = searchResult.SpecRev == null ? null : searchResult.SpecRev.Value,
                                             StartTimeGMT = searchResult.StartTimeGMT == null ? (DateTime?)null : searchResult.StartTimeGMT,
                                             Timer = searchResult.Timer == null ? null : new SubentityRef(searchResult.Timer.CDOTypeName),
                                             TimerId = searchResult.TimerId == null ? null : searchResult.TimerId.Value,
                                             MaxEndWarningTimeGMT = searchResult.MaxEndWarningTimeGMT == null ? (DateTime?)null :searchResult.MaxEndWarningTimeGMT,
                                             MinEndWarningTimeGMT = searchResult.MinEndWarningTimeGMT == null ? (DateTime?)null : searchResult.MinEndWarningTimeGMT,
                                             MinEndTimeGMT = searchResult.MinEndTimeGMT == null ? (DateTime?)null : searchResult.MinEndTimeGMT,
                                             MaxEndTimeGMT = searchResult.MaxEndTimeGMT == null ? (DateTime?)null : searchResult.MaxEndTimeGMT,
                                             MaxWarningTimeColor = searchResult.MaxWarningTimeColor == null ? null : searchResult.MaxWarningTimeColor,
                                             MinWarningTimeColor = searchResult.MinWarningTimeColor == null ? null : searchResult.MinWarningTimeColor,
                                             TimetoMax = searchResult.TimetoMax == null ? null : GetTimeSpan(searchResult.TimetoMax.Value, true),
                                             TimetoMin = searchResult.TimetoMin == null ? null : GetTimeSpan(searchResult.TimetoMin.Value, true),
                                             TimersCount = searchResult.TimersCount == null ? null : searchResult.TimersCount,
                                             TimerStartedonSpec = searchResult.TimerStartedonSpec == null ? null : searchResult.TimerStartedonSpec.Value,
                                             TimerEndsonSpec = searchResult.TimerEndsonSpec == null ? null : searchResult.TimerEndsonSpec.Value.Equals(":") ? null : searchResult.TimerEndsonSpec.Value
                                         };                                                        
                            processTimerSearchResult = srList
                                .OrderBy(x => x.EndTimeGMT == null ? x.MaxEndTimeGMT != null ? x.MaxEndTimeGMT.Value : DateTime.MaxValue : DateTime.MaxValue)
                                .ThenBy(x => x.EndTimeGMT == null ? x.MinEndTimeGMT != null ? x.MinEndTimeGMT.Value : DateTime.MaxValue : DateTime.MaxValue)
                                .ThenByDescending(x => x.EndTimeGMT != null ? x.EndTimeGMT.Value : DateTime.MinValue)
                                .ToArray();
                            SearchResults.Data = processTimerSearchResult;
                        }
                        SearchResults.DataBind();
                    }
                    else
                    {
                        DisplayMessage(resultStatus);
                    }
                }
            }
        }

        protected virtual void ClearAll(object sender, EventArgs e)
        {
            ClearPageData();
            ClearGridData();
            ProcessTimerActions.Hidden = true;
            CompletionStatus.Enabled = true;
            CompletionStatus.ReadOnly = false;
        }

        protected virtual void ClearPageData()
        {
            Page.ClearValues();
        }

        protected virtual void ClearGridData()
        {
            SearchResults.ClearData();
            SearchResults.OriginalData = null;
            SearchResults.GridContext.CurrentPage = 1;
        }

        protected virtual string GetTimeSpan(string value, bool addSign = false)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            double d;
            if (Double.TryParse(Regex.Replace(value, @"[,.]", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.ToString(), RegexOptions.IgnoreCase), out d))
            {
                string result = TimeSpan.FromDays(d).ToString(@"dd\.hh\:mm\:ss");
                if (d > 0 && addSign)
                    return "+ " + result;
                else
                    return result;
            }

            return string.Empty;
        }

        #endregion

        #region Public Functions

        #endregion

        #region Private Functions

        #endregion

        #region Constants
        protected const string processTimerSearchDetail = "ProcessTimerSearchDetail";
        private bool isReload = false;
        #endregion

        #region Private Member Variables

        #endregion

    }
}



