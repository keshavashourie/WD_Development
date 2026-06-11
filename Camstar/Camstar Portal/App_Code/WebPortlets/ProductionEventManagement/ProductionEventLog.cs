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
using Camstar.WebPortal.WebPortlets;

/// <summary>
/// Summary description for ProductionEventLog
/// </summary>

namespace Camstar.WebPortal.WebPortlets
{
    public class ProductionEventLog : MatrixWebPart
    {
        #region Properties
        protected virtual JQDataGrid EventLogDetails
        {
            get { return Page.FindCamstarControl("EventLogDetails_CommentsLog") as JQDataGrid; }
        }
        protected virtual Camstar.WebPortal.FormsFramework.WebControls.NamedObject InstanceID
        {
            get { return Page.FindCamstarControl("InstanceID") as Camstar.WebPortal.FormsFramework.WebControls.NamedObject; }
        }
        protected virtual Button UpdateComment
        {
            get { return Page.FindCamstarControl("UpdateComment") as Button; }
        }

        protected virtual JQTabContainer TabContainer { get { return Page.FindCamstarControl("Tabs") as JQTabContainer; } }

        #endregion
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsTestMode)
                UpdateComment.Click += UpdateCommentsOnClick;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (TabContainer.SelectedItem.Name == "Log")
                LoadEventLogs();
        }

        private void UpdateCommentsOnClick(object sender, EventArgs e)
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new UpdateEventLogsService(session.CurrentUserProfile);
                var serviceData = new UpdateEventLogs();
                serviceData.QualityESigDetail = ESigCaptureUtil.CollectQualityESigDetail();
                serviceData.QualityObject = new NamedObjectRef() { CDOTypeName = "Event", Name = InstanceID.Data.ToString() };
                var submitLogs = new List<EventLogDetail>();
                var logs = (EventLogDetails.Data as object[]);
                if (logs != null)
                {                    
                    submitLogs.AddRange(logs.Select(log => new EventLogDetail
                    {
                        CreationDateGMT = (log as EventLogDetail).CreationDateGMT,
                        Employee = (log as EventLogDetail).Employee,
                        CommentType = (log as EventLogDetail).CommentType,
                        Comments = (log as EventLogDetail).Comments,
                        EventLog= (log as EventLogDetail).EventLog
                    }));
                    serviceData.EventLogDetails = submitLogs.ToArray();
                    var request = new UpdateEventLogs_Request();
                    var result = new UpdateEventLogs_Result();

                    ResultStatus resultStatus = service.ExecuteTransaction(serviceData, request, out result);

                    if (resultStatus != null && resultStatus.IsSuccess)
                    {
                        DisplayMessage(resultStatus);
                    }
                    else
                    {
                        DisplayMessage(resultStatus);
                    }
                    ESigCaptureUtil.CleanQualityESigCaptureDM();
                }
            }
        }

        protected virtual void LoadEventLogs()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new UpdateEventLogsService(session.CurrentUserProfile);

                //// Set up parameters for the service here
                var serviceData = new UpdateEventLogs();

                serviceData.QualityObject = new NamedObjectRef() { CDOTypeName = "Event", Name = InstanceID.Data.ToString() };

                var request = new UpdateEventLogs_Request()
                {
                    Info = new UpdateEventLogs_Info()
                    {
                        QualityObject = new Info(true),
                        EventLogDetails = new EventLogDetail_Info() { RequestValue = true}
                    }
                };

                var result = new UpdateEventLogs_Result();

                ResultStatus resultStatus = service.Load(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    EventLogDetails.Data = result.Value.EventLogDetails;
                    EventLogDetails.DataBind();
                }
                else
                {
                    DisplayMessage(resultStatus);
                }
            }
        }
    }
}
