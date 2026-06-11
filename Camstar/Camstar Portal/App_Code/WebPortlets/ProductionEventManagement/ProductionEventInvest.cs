// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Helpers;

/// <summary>
/// Summary description for ProductionEventInvest
/// </summary>

namespace Camstar.WebPortal.WebPortlets
{
    public class ProductionEventInvest : MatrixWebPart
    {
        #region Properties
        protected virtual Button UpdateCauses
        {
            get { return Page.FindCamstarControl("UpdateCauses") as Button; }
        }
        protected virtual Button UpdateActions
        {
            get { return Page.FindCamstarControl("UpdateActions") as Button; }
        }
        protected virtual Camstar.WebPortal.FormsFramework.WebControls.NamedObject InstanceID
        {
            get { return Page.FindCamstarControl("InstanceID") as Camstar.WebPortal.FormsFramework.WebControls.NamedObject; }
        }
        protected virtual JQDataGrid EventCauseDetail
        {
            get { return Page.FindCamstarControl("EventDataDetail_EventFailureDetails") as JQDataGrid; }
        }
        protected virtual JQDataGrid EventActionDetail
        {
            get { return Page.FindCamstarControl("EventDataDetail_EventFailureDetailsAction") as JQDataGrid; }
        }
        protected virtual JQDataGrid EventFailureActionDetails
        {
            get { return Page.FindCamstarControl("EventDataDetail_EventFailureActionDetails") as JQDataGrid; }
        }

        protected virtual JQTabContainer TabContainer { get { return Page.FindCamstarControl("Tabs") as JQTabContainer; } }

        #endregion
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsTestMode)
            {
                UpdateActions.Click += UpdateActions_Click;
                UpdateCauses.Click += UpdateCauses_Click;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (TabContainer.SelectedItem.Name == "Investigation")
                LoadActions();
            if (TabContainer.SelectedItem.Name == "Investigation" && Page.SessionVariables["Failures"] == null)
                LoadFailures();


        }
        protected virtual void LoadFailures()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new UpdateEventFailuresService(session.CurrentUserProfile);
                var serviceData = new UpdateEventFailures();

                serviceData.QualityObject = new NamedObjectRef() { CDOTypeName = "Event", Name = InstanceID.Data.ToString() };

                var request = new UpdateEventFailures_Request()
                {
                    Info = new UpdateEventFailures_Info()
                    {
                        QualityObject = new Info(true),
                        EventFailureDetails = new EventFailureDetail_Info()
                        {
                            Comments = new Info(true),
                            Description = new Info(true),
                            EventFailureCauseDetails = new EventFailureCauseDetail_Info() { RequestValue = true },
                            FailureMode = new Info(true),
                            FailureModeGroup = new Info(true),
                            FailureSeverity = new Info(true),
                            FailureType = new Info(true),
                            EventFailure = new Info(true)
                        },

                    }
                };

                var result = new UpdateEventFailures_Result();

                ResultStatus resultStatus = service.Load(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    EventActionDetail.Data = result.Value.EventFailureDetails;
                    EventCauseDetail.Data = result.Value.EventFailureDetails;
                    Page.SessionVariables["Failures"] = result.Value.EventFailureDetails;
                }
                else
                {

                    DisplayMessage(resultStatus);
                }
            }
        }
        protected virtual void LoadActions()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new UpdateEventFailureActionsService(session.CurrentUserProfile);
                var serviceData = new UpdateEventFailureActions();

                serviceData.QualityObject = new NamedObjectRef() { CDOTypeName = "Event", Name = InstanceID.Data.ToString() };

                var request = new UpdateEventFailureActions_Request()
                {
                    Info = new UpdateEventFailureActions_Info()
                    {
                        QualityObject = new Info(true),
                        EventFailureActionDetails = new EventFailureActionDetail_Info()
                        {
                            ActionOwner = new Info(true),
                            ActionRole = new Info(true),
                            ActionType = new Info(true),
                            Comments = new Info(true),
                            ActionTypeGroup = new Info(true),
                            CompletionDate = new Info(true),
                            EventFailureAction = new Info(true),
                        }
                    }
                };

                var result = new UpdateEventFailureActions_Result();

                ResultStatus resultStatus = service.Load(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    EventFailureActionDetails.Data = result.Value.EventFailureActionDetails;
                }
                else
                {

                    DisplayMessage(resultStatus);
                }
            }
        }

        private void UpdateCauses_Click(object sender, EventArgs e)
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new UpdateEventFailureCausesService(session.CurrentUserProfile);
                var serviceData = new UpdateEventFailureCauses();
                serviceData.QualityESigDetail = ESigCaptureUtil.CollectQualityESigDetail();
                serviceData.QualityObject = new NamedObjectRef() { CDOTypeName = "Event", Name = InstanceID.Data.ToString() };

                var failures = Page.SessionVariables["Failures"] as EventFailureDetail[];
                if (failures != null && failures.Length != 0)
                {
                    serviceData.EventFailureDetails = ProductionEventManagementHelper.CloneEventFailures(failures);
                }

                var request = ProductionEventManagementHelper.CreateUpdateEventFailureCauses_Request();

                var result = new UpdateEventFailureCauses_Result();

                ResultStatus resultStatus = service.ExecuteTransaction(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    DisplayMessage(resultStatus);
                    Page.SessionVariables["Failures"] = result.Value.EventFailureDetails;
                }
                else
                {
                    DisplayMessage(resultStatus);
                }
                ESigCaptureUtil.CleanQualityESigCaptureDM();
            }
        }

        void UpdateActions_Click(object sender, EventArgs e)
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new UpdateEventFailureActionsService(session.CurrentUserProfile);
                var serviceData = new UpdateEventFailureActions();
                serviceData.QualityESigDetail = ESigCaptureUtil.CollectQualityESigDetail();
                serviceData.QualityObject = new NamedObjectRef() { CDOTypeName = "Event", Name = InstanceID.Data.ToString() };
                var actionEventData = EventFailureActionDetails.Data as EventFailureActionDetail[];
                var failures = Page.SessionVariables["Failures"] as EventFailureDetail[];

                if (failures != null && failures.Length != 0)
                {
                    serviceData.EventFailureActionDetails = actionEventData != null
                        ? actionEventData
                            .Select(a => new EventFailureActionDetail
                            {
                                EventFailureAction = a.EventFailureAction,
                                ActionType = a.ActionType,
                                ActionOwner = a.ActionOwner,
                                CompletionDate = a.CompletionDate,
                                Comments = a.Comments
                            }).ToArray()
                        : null;

                    serviceData.EventFailureDetails = failures.Select(f => f.EventFailureCauseDetails != null ? new EventFailureDetail
                    {
                        EventFailure = f.EventFailure,
                        FailureMode = f.FailureMode,
                        Description = f.Description,
                        FailureSeverity = f.FailureSeverity,
                        FailureType = f.FailureType,
                        Comments = f.Comments,
                        EventFailureCauseDetails = f.EventFailureCauseDetails.Select(d => new EventFailureCauseDetail
                        {
                            EventFailureCause = d.EventFailureCause,
                            CauseCode = d.CauseCode,
                            IsRootCause = d.IsRootCause,
                            Comments = d.Comments,
                            EventFailureActionDetails = d.EventFailureActionDetails != null ? d.EventFailureActionDetails
                                .Select(a => new EventFailureActionDetail
                                {
                                    ActionType = a.ActionType,
                                    ActionOwner = a.ActionOwner,
                                    CompletionDate = a.CompletionDate,
                                    Comments = a.Comments,
                                    EventFailureAction = a.EventFailureAction,
                                }).ToArray() : null
                        }).ToArray()
                    } :
                    new EventFailureDetail
                    {
                        FailureMode = f.FailureMode,
                        Description = f.Description,
                        FailureSeverity = f.FailureSeverity,
                        FailureType = f.FailureType,
                        Comments = f.Comments,
                        EventFailure = f.EventFailure,
                        EventFailureCauseDetails = f.EventFailureCauseDetails,
                    }).ToArray();
                }

                var request = new UpdateEventFailureActions_Request()
                {
                    Info = new UpdateEventFailureActions_Info()
                    {
                        QualityObject = new Info(true),
                        EventFailureActionDetails = new EventFailureActionDetail_Info()
                        {
                            ActionOwner = new Info(true),
                            ActionRole = new Info(true),
                            ActionType = new Info(true),
                            Comments = new Info(true),
                            ActionTypeGroup = new Info(true),
                            CompletionDate = new Info(true),
                            EventFailureAction = new Info(true),
                        }
                    }
                };
                var result = new UpdateEventFailureActions_Result();

                ResultStatus resultStatus = service.ExecuteTransaction(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    DisplayMessage(resultStatus);
                    Page.SessionVariables["Failures"] = result.Value.EventFailureDetails;
                }
                else
                {
                    DisplayMessage(resultStatus);
                }
                ESigCaptureUtil.CleanQualityESigCaptureDM();
            }
        }

    }
}
