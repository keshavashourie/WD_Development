using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;

namespace Camstar.WebPortal.Helpers
{
    /// <summary>
    /// Summary description for Class1
    /// </summary>
    public static class ProductionEventManagementHelper
    {
        public static EventFailureDetail[] CloneEventFailures(EventFailureDetail[] failures)
        {
            return failures.Select(f =>
           (f.EventFailureCauseDetails != null)
           ? new EventFailureDetail
               {
                   FailureMode = f.FailureMode,
                   Description = f.Description,
                   FailureSeverity = f.FailureSeverity,
                   FailureType = f.FailureType,
                   Comments = f.Comments,
                   EventFailure = f.EventFailure,
                   EventFailureCauseDetails = f.EventFailureCauseDetails.Select(d =>
                           new EventFailureCauseDetail
                           {
                               CauseCode = d.CauseCode,
                               IsRootCause = d.IsRootCause,
                               Comments = d.Comments,
                               EventFailureCause = d.EventFailureCause,
                               EventFailureActionDetails = (d.EventFailureActionDetails != null)
                                    ? d.EventFailureActionDetails.Select(a =>
                                      new EventFailureActionDetail
                                        {
                                            ActionType = a.ActionType,
                                            EventFailureAction = a.EventFailureAction,
                                            ActionOwner = a.ActionOwner,
                                            CompletionDate = a.CompletionDate,
                                            Comments = a.Comments
                                        }).ToArray()
                                    : null
                           }).ToArray()
               }
           : new EventFailureDetail
             {
                FailureMode = f.FailureMode,
                Description = f.Description,
                FailureSeverity = f.FailureSeverity,
                FailureType = f.FailureType,
                Comments = f.Comments,
                EventFailure = f.EventFailure
             }
           ).ToArray();
        }

        public static UpdateEventFailures_Request CreateUpdateEventFailures_Request()
        {
            return new UpdateEventFailures_Request()
            {
                Info = new UpdateEventFailures_Info()
                {
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
                    }
                }
            };
        }

        public static UpdateEventFailureCauses_Request CreateUpdateEventFailureCauses_Request()
        {
            return new UpdateEventFailureCauses_Request()
            {
                Info = new UpdateEventFailureCauses_Info()
                {
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
                    }
                }
            };
        }
    }

}