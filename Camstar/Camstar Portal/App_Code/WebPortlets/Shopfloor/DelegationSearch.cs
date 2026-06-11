//
// Copyright Siemens 2023  
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Tools.ASPXConverter;
using UIAction = Camstar.WebPortal.Personalization.UIAction;

/// <summary>
/// Summary description for DelegationSearch
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class DelegationSearch : MatrixWebPart
    {
        //DelegateByTask
        protected virtual CheckBox TaskDueTodayOrLaterFilter { get { return Page.FindCamstarControl("TaskDueTodayOrLaterFilter") as CheckBox; } }
        protected virtual DateChooser DueDateBeginFilter { get { return Page.FindCamstarControl("DueDateBeginFilter") as DateChooser; } }
        protected virtual DateChooser DueDateEndFilter { get { return Page.FindCamstarControl("DueDateEndFilter") as DateChooser; } }
        protected virtual JQDataGrid SearchResultsGrid { get { return Page.FindCamstarControl("SearchResultsGrid") as JQDataGrid; } }
        protected virtual Button SearchButton { get { return Page.FindCamstarControl("SearchButton") as Button; } }
        protected virtual Button ClearAllByTask { get { return Page.FindCamstarControl("ClearAllByTask") as Button; } }
        protected virtual UIAction DelegateAction { get { return Page.ActionDispatcher.GetActionByName("OpenDelegate") as FloatPageOpenAction; } }
        protected virtual NamedObject AssignedEmployeeFilter { get { return Page.FindCamstarControl("AssignedEmployeeFilter") as NamedObject; } }
        protected virtual DropDownList ApplicationFilter { get { return Page.FindCamstarControl("ApplicationFilter") as DropDownList; } }
        protected virtual TextBox InstanceNameFilter { get { return Page.FindCamstarControl("InstanceNameFilter") as TextBox; } }
        protected virtual DropDownList TaskFilter { get { return Page.FindCamstarControl("TaskFilter") as DropDownList; } }

        //DelegateByDate
        protected virtual Button ClearAllButton { get { return Page.FindCamstarControl("ClearButtonByDate") as Button; } }
        protected virtual Button SearchButtonByDate { get { return Page.FindCamstarControl("SearchButtonByDate") as Button; } }
        protected virtual CheckBox ExpiredByDate { get { return Page.FindCamstarControl("ExpiredByDate") as CheckBox; } }
        protected virtual CheckBox FutureByDate { get { return Page.FindCamstarControl("FutureByDate") as CheckBox; } }
        protected virtual CheckBox ActiveByDate { get { return Page.FindCamstarControl("ActiveByDate") as CheckBox; } }
        protected virtual NamedObject AssignedEmployeeByDate { get { return Page.FindCamstarControl("AssignedEmployeeByDate") as NamedObject; } }
        protected virtual DropDownList ApplicationByDate { get { return Page.FindCamstarControl("ApplicationByDate") as DropDownList; } }
        protected virtual NamedObject TargetEmployeeByDate { get { return Page.FindCamstarControl("TargetEmployeeByDate") as NamedObject; } }
        protected virtual NamedObject DelegationReasonByDate { get { return Page.FindCamstarControl("DelegationReasonByDate") as NamedObject; } }
        protected virtual NamedObject DelegatedByByDate { get { return Page.FindCamstarControl("DelegatedByByDate") as NamedObject; } }
        protected virtual JQDataGrid SearchByDateGrid { get { return Page.FindCamstarControl("SearchByDateGrid") as JQDataGrid; } }
        protected virtual JQTabContainer TabContainer { get { return Page.FindCamstarControl("PackageInquery_TabContainer") as JQTabContainer; } }

        protected string StatusMessageText = null;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Page.EventTarget == "AddButton")
            {
                SearchByDateGrid.GridContext.SelectedItem = null;
                SearchByDateGrid.GridContext.SelectRow(null,false);
                Page.SessionVariables.SetValueByName("DelegateTaskId", null);
                Page.SessionVariables.SetValueByName("DelegationStatus", null);
                OpenDelegateByDatePopup();
            }

            ClearAllByTask.Click += ClearAllByTask_Click;
            ClearAllButton.Click += ClearAllByDate_Click;
            SearchButtonByDate.Click += SearchButtonByDate_Click;
            SearchByDateGrid.RowSelected += SearchByDateGrid_RowSelected;

            DelegateAction.ConditionHandler = "WebPartConditionActionHandler";
            if (Page.SessionVariables.GetValueByName("UpdateGrid") != null)
            {
                RequestForByTaskGrid();
                Page.SessionVariables.SetValueByName("UpdateGrid", null);
            }
        }

        protected virtual void OpenDelegateByDatePopup()
        {
            FloatPageOpenAction floatAction = new FloatPageOpenAction();
            floatAction.FrameLocation = new UIFloatingPageLocation();
            floatAction.PageName = "DelegateByDatePopup_VP";
            floatAction.FrameLocation.Width = 746;
            floatAction.FrameLocation.Height = 450;
            floatAction.EndResponse = false;

            ActionDispatcher dispatcher = Page.ActionDispatcher;

            dispatcher.ExecuteAction(floatAction);
        }
        protected virtual ResponseData SearchByDateGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            object selRowId = null;
            object delegationStatus = null;
            if (SearchByDateGrid.GridContext.SelectedItem != null)
            {
                selRowId = (SearchByDateGrid.GridContext.SelectedItem as DelegationTaskInquiryDetail).DelegationTask.ToString();
                delegationStatus = (SearchByDateGrid.GridContext.SelectedItem as DelegationTaskInquiryDetail).DelegationStatus.ToString();
            }
            Page.SessionVariables.SetValueByName("DelegateTaskId", selRowId);
            Page.SessionVariables.SetValueByName("DelegationStatus", delegationStatus);

            return null;
        }

        protected virtual void ClearAllByTask_Click(object sender, EventArgs e)
        {
            TaskDueTodayOrLaterFilter.Data = false;
            DueDateBeginFilter.Data = null;
            DueDateEndFilter.Data = null;
            SearchResultsGrid.ClearData();
            SearchResultsGrid.OriginalData = null;
            SearchResultsGrid.GridContext.CurrentPage = 1;
            AssignedEmployeeFilter.Data = null;
            ApplicationFilter.Data = null;
            InstanceNameFilter.Data = null;
            TaskFilter.Data = null;
        }

        protected virtual void ClearAllByDate_Click(object sender, EventArgs e)
        {
            ClearPageData();
            ClearGridData();
        }

        protected virtual void ClearPageData()
        {
            ExpiredByDate.Data = false;
            FutureByDate.Data = false;
            ActiveByDate.Data = false;
            AssignedEmployeeByDate.Data = null;
            TargetEmployeeByDate.Data = null;
            DelegationReasonByDate.Data = null;
            DelegatedByByDate.Data = null;
            ApplicationByDate.Data = null;

        }

        protected virtual void ClearGridData()
        {
            SearchByDateGrid.ClearData();
            SearchByDateGrid.OriginalData = null;
            SearchByDateGrid.GridContext.CurrentPage = 1;
        }
        protected virtual void SearchButtonByDate_Click(object sender, EventArgs e)
        {
            RequestForByDateGrid();
        }

        protected virtual void RequestForByDateGrid()
        {
            ClearGridData();

            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new DelegateDateInquiryService(session.CurrentUserProfile);

                //// Set up parameters for the service here
                var serviceData = new DelegateDateInquiry()
                {
                    ApplicationFilter = ApplicationByDate.Data != null ? new Enumeration<ApplicationTypeEnum, int>((int)ApplicationByDate.Data) : null,
                    DelegatedByFilter = DelegatedByByDate.Data as NamedObjectRef,
                    IsActive = new Primitive<bool>((bool)ActiveByDate.Data),
                    IsExpired = new Primitive<bool>((bool)ExpiredByDate.Data),
                    IsFuture = new Primitive<bool>((bool)FutureByDate.Data),
                    TargetEmployeeFilter = TargetEmployeeByDate.Data as NamedObjectRef,
                    AssignedEmployeeFilter = AssignedEmployeeByDate.Data as NamedObjectRef,
                    DelegationReasonFilter = DelegationReasonByDate.Data as NamedObjectRef

                };

                var request = new DelegateDateInquiry_Request()
                {
                    Info = new DelegateDateInquiry_Info()
                    {
                        DelegationTasks = new DelegationTaskInquiryDetail_Info() { RequestValue = true },
                    }
                };

                var result = new DelegateDateInquiry_Result();

                ResultStatus resultStatus = service.GetTasks(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    var resultValue = result.Value.DelegationTasks;
                    SearchByDateGrid.Data = resultValue;
                    if (Page.SessionVariables.GetValueByName("StatusMessageText") != null)
                    {
                        StatusMessageText = Page.SessionVariables.GetValueByName("StatusMessageText").ToString();
                        DisplayMessage(new ResultStatus(StatusMessageText, true));//Show success message from the popup
                        Page.SessionVariables.SetValueByName("StatusMessageText", null);
                    }
                }
                else
                {
                    DisplayMessage(resultStatus);
                }
                Page.SessionVariables.SetValueByName("Reload", null);
            }
        }

        protected virtual void RequestForByTaskGrid()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new DelegateTaskInquiryService(session.CurrentUserProfile);

                //// Set up parameters for the service here
                var serviceData = new DelegateTaskInquiry()
                {
                    AssignedEmployee = AssignedEmployeeFilter.Data as NamedObjectRef,
                    ApplicationType = ApplicationFilter.Data != null ? new Enumeration<ApplicationTypeEnum, int>((int)ApplicationFilter.Data) : null,
                    InstanceName = InstanceNameFilter.Data as string,
                    DelegationTaskType = TaskFilter.Data != null ? new Enumeration<DelegationTaskTypeEnum, int>((int)TaskFilter.Data) : null,
                    TaskIsDue = new Primitive<bool>((bool)TaskDueTodayOrLaterFilter.Data),
                    DueDateBegin = DueDateBeginFilter.Data != null ? new Primitive<DateTime>((DateTime)DueDateBeginFilter.Data) : null,
                    DueDateEnd = DueDateEndFilter.Data != null ? new Primitive<DateTime>((DateTime)DueDateEndFilter.Data) : null,

                };

                var request = new DelegateTaskInquiry_Request()
                {
                    Info = new DelegateTaskInquiry_Info()
                    {
                        DelegationTarget = new Info(true, true)
                    }
                };

                var result = new DelegateTaskInquiry_Result();

                ResultStatus resultStatus = service.GetEnvironment(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    var selVal = result.Environment.DelegationTarget.SelectionValues;
                    SearchResultsGrid.SetSelectionValues(selVal);
                }
                else
                {
                    DisplayMessage(resultStatus);
                }
            }
        }

        protected virtual void DeleteDelegation()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new DelegationTaskMaintService(session.CurrentUserProfile);

                var serviceData = new DelegationTaskMaint();
                service.BeginTransaction();
                serviceData.ObjectToChange = new SubentityRef();
                serviceData.ObjectToChange.CDOTypeName = "DelegationTask";

                if (Page.SessionVariables.GetValueByName("DelegateTaskId") != null)
                    serviceData.ObjectToChange.ID = Page.SessionVariables.GetValueByName("DelegateTaskId").ToString();

                var request = new DelegationTaskMaint_Request()
                {
                    Info = new DelegationTaskMaint_Info()
                    {
                        ObjectChanges = new DelegationTaskChanges_Info() { RequestValue = true }
                    }
                };

                var result = new DelegationTaskMaint_Result();


                service.Delete(serviceData);
                service.ExecuteTransaction();
                ResultStatus resultStatus = service.CommitTransaction(request, out result);

                if (resultStatus.IsSuccess)
                {
                    DisplayMessage(resultStatus);
                    Page.SessionVariables.SetValueByName("DelegateTaskId", null);
                    Page.SessionVariables.SetValueByName("DelegationStatus", null);
                }
                else
                {
                    DisplayMessage(resultStatus);
                }

            }
        }

        public override void WebPartConditionActionHandler(object sender, ConditionActionEventArgs e)
        {
            base.WebPartConditionActionHandler(sender, e);
            if (Page.EventTarget == "DelButton")//if click on delete button on grid
            {
                DeleteDelegation();
            }

            if (Page.EventTarget != string.Empty)
            {
                Page.DataContract.SetValueByName("DelegationSearchSelRows", SearchResultsGrid.SelectedRowIDs);
                Page.DataContract.SetValueByName("SelectedCount", SearchResultsGrid.SelectedRowCount.ToString());
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!Page.IsPostBack || Page.SessionVariables.GetValueByName("Reload") != null || Page.EventTarget == "DelButton")
                RequestForByDateGrid();

            ScriptManager.RegisterStartupScript(Page.Form, Page.Form.GetType(), "DelegationSearchFunctions",
                string.Format("DelegationSearch_AddToggleSearchAttr();"), true);

            if (TaskDueTodayOrLaterFilter.IsChecked)
            {
                DueDateBeginFilter.Data = null;
                DueDateBeginFilter.Enabled = false;
                DueDateEndFilter.Data = null;
                DueDateEndFilter.Enabled = false;
            }
            else
            {
                DueDateBeginFilter.Enabled = true;
                DueDateEndFilter.Enabled = true;
            }
            if (ApplicationFilter.Data == null)
                TaskFilter.Enabled = false;
            else TaskFilter.Enabled = true;

            Page.RenderToClient = true;
        }

    }
}
