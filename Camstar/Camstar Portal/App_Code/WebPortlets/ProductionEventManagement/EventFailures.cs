// Copyright Siemens 2023  
using System;
using System.Linq;
using System.Web;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Helpers;

namespace Camstar.WebPortal.WebPortlets
{
    public class EventFailures : MatrixWebPart
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            string pageName = Page.ToString();

            if (pageName.Contains("generic"))
            {
                FailuresProduct.Visible = true;
                FailuresOperation.Visible = true;
            }

            UpdateFailures.Click += UpdateFailuresOnClick;

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (TabContainer.SelectedItem.Name == "Failures" && Page.SessionVariables["Failures"] == null)
                LoadFailures();
        }

        private void UpdateFailuresOnClick(object sender, EventArgs eventArgs)
        {
            SubmitFailures();
        }

        protected virtual void SubmitFailures()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new UpdateEventFailuresService(session.CurrentUserProfile);
                var serviceData = new UpdateEventFailures();
                serviceData.QualityESigDetail = ESigCaptureUtil.CollectQualityESigDetail();
                serviceData.QualityObject = new NamedObjectRef() { CDOTypeName = "Event", Name = InstanceID.Data.ToString() };
                var failures = Page.SessionVariables["Failures"] as EventFailureDetail[];

                if(failures!=null && failures.Length != 0)
                {
                    serviceData.EventFailureDetails = ProductionEventManagementHelper.CloneEventFailures(failures);
                }
                var request = ProductionEventManagementHelper.CreateUpdateEventFailures_Request();
                var result = new UpdateEventFailures_Result();

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
                            EventFailure=new Info(true)
                        }
                    }
                };

                var result = new UpdateEventFailures_Result();

                ResultStatus resultStatus = service.Load(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    FailureModesGrid.Data = result.Value.EventFailureDetails;
                    FailureModesGrid.DataBind();
                    Page.SessionVariables["Failures"] = result.Value.EventFailureDetails;
                }
                else
                {

                    DisplayMessage(resultStatus);
                }
            }
        }


        #region Properties
        protected virtual JQTabContainer TabContainer { get { return Page.FindCamstarControl("Tabs") as JQTabContainer; } }
        protected virtual Camstar.WebPortal.FormsFramework.WebControls.RevisionedObject FailuresProduct
        {
            get { return Page.FindCamstarControl("Failures_Product") as Camstar.WebPortal.FormsFramework.WebControls.RevisionedObject; }
        }

        protected virtual Camstar.WebPortal.FormsFramework.WebControls.NamedObject FailuresOperation
        {
            get { return Page.FindCamstarControl("Failures_Operation") as Camstar.WebPortal.FormsFramework.WebControls.NamedObject; }
        }
        protected virtual Camstar.WebPortal.FormsFramework.WebControls.NamedObject InstanceID
        {
            get { return Page.FindCamstarControl("InstanceID") as Camstar.WebPortal.FormsFramework.WebControls.NamedObject; }
        }
        protected virtual JQDataGrid FailureModesGrid
        {
            get { return Page.FindCamstarControl("FailureModesGrid") as JQDataGrid; }
        }

        protected virtual Button UpdateFailures
        {
            get { return Page.FindCamstarControl("UpdateFailures") as Button; }
        }

        #endregion
    }
}
