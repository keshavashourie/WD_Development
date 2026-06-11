// © 2020 Siemens Product Lifecycle Management Software Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.WebPortlets;

/// <summary>
/// Summary description for ProductionEventGeneral
/// </summary>

namespace Camstar.WebPortal.WebPortlets
{
    public class ProductionEventGeneral : MatrixWebPart
    {
        #region Properties

        protected virtual TextBox BriefDescription
        {
            get { return Page.FindCamstarControl("BriefDescription") as TextBox; }
        }

        protected virtual Camstar.WebPortal.FormsFramework.WebControls.TextEditor DescriptionAria
        {
            get { return Page.FindCamstarControl("Description") as Camstar.WebPortal.FormsFramework.WebControls.TextEditor; }
        }
        protected virtual NamedObject PriorityLevel
        {
            get { return Page.FindCamstarControl("PriorityLevel") as NamedObject; }
        }
        protected virtual DateChooser OccurenceDate
        {
            get { return Page.FindCamstarControl("OccurenceDate") as DateChooser; }
        }
        protected virtual DiscoveryAreaControl DiscoveryArea
        {
            get { return Page.FindCamstarControl("DiscoveryArea") as DiscoveryAreaControl; }
        }
        protected virtual NamedObject InstanceID
        {
            get { return Page.FindCamstarControl("InstanceID") as NamedObject; }
        }
        protected virtual NamedObject Initiator
        {
            get { return Page.FindCamstarControl("Initiator") as NamedObject; }
        }
        protected virtual DropDownList Resource
        {
            get { return Page.FindCamstarControl("Resource") as DropDownList; }
        }
        protected virtual NamedObject ResourceGroup
        {
            get { return Page.FindCamstarControl("ResourceGroup") as NamedObject; }
        }
        protected virtual NamedObject Reporter
        {
            get { return Page.FindCamstarControl("Reporter") as NamedObject; }
        }
        protected virtual DateChooser ReportedDate
        {
            get { return Page.FindCamstarControl("ReportedDate") as DateChooser; }
        }
        protected virtual NamedObject InitiatorOrganization
        {
            get { return Page.FindCamstarControl("InitiatorOrganization") as NamedObject; }
        }
        protected virtual NamedObject ReporterOrganization
        {
            get { return Page.FindCamstarControl("ReporterOrganization") as NamedObject; }
        }
        protected virtual WorkflowNavigator ChargeToWorkflow
        {
            get { return Page.FindCamstarControl("ChargeToWorkflow") as WorkflowNavigator; }
        }

        protected virtual Button GeneralSave
        {
            get { return Page.FindCamstarControl("GeneralSave") as Button; }
        }

        protected virtual JQDataGrid DisallowedTxnGrid
        {
            get { return Page.FindCamstarControl("DisallowedTransactionsGrid") as JQDataGrid; }
        }

        protected virtual Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity ApprovalCycleInquiry
        {
            get { return Page.FindCamstarControl("ApprovalCycleInquiry_ApprovalSheet") as Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity; }
        }
        protected virtual Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity QualityObjectDetail_CompletionApprovalSheet
        {
            get { return Page.FindCamstarControl("QualityObjectDetail_CompletionApprovalSheet") as Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity; }
        }
        

        #endregion

        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            var gridSettings =
                    ((DisallowedTxnGrid.GridContext as ItemDataContext).Settings as GridDataSettingsItemList);
            var actions = gridSettings.NavigatorActions.Where(a => a.Action == JQGridNavActionType.Add || a.Action == JQGridNavActionType.Delete);
            if (Page.PortalContext is ViewEditBehaviorContext)
            {
                if (!(Page.PortalContext as ViewEditBehaviorContext).IsInEditMode)
                {
                    gridSettings.SelectionMode = JQGridSelectionMode.Disable;
                    gridSettings.EditorSettings.EditingMode = JQEditingModes.Disabled;
                    foreach (var action in actions)
                    {
                        action.Visible = false;
                    }
                }
                else
                {
                    gridSettings.SelectionMode = JQGridSelectionMode.SingleRowSelect;
                    gridSettings.EditorSettings.EditingMode = JQEditingModes.Inline;
                    foreach (var action in actions)
                    {
                        action.Visible = true;
                    }
                }
            }
            CamstarWebControl.SetRenderToClient(DisallowedTxnGrid);
            DisallowedTxnGrid.GridContext.RenderToClient = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsTestMode)
            {
                GeneralSave.Click += GeneralSave_Click;
                if (!Page.IsPostBack)
                    LoadGeneral();
            }
        }

        protected virtual void LoadGeneral()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new UpdateEventDataService(session.CurrentUserProfile);
                var serviceData = new UpdateEventData();

                serviceData.QualityObject = new NamedObjectRef()
                {
                    CDOTypeName = "Event",
                    Name = InstanceID.Data.ToString()
                };

                var request = new UpdateEventData_Request()
                {
                    Info = new UpdateEventData_Info()
                    {
                        BriefDescription = new Info(true),
                        Description = new Info(true),
                        DiscoveryArea = new Info(true),
                        DisallowedTxns = new Info(true),
                        Initiator = new Info(true),
                        InitiatorOrganization = new Info(true),
                        OccurrenceDate = new Info(true),
                        PriorityLevel = new Info(true),
                        ReportedDate = new Info(true),
                        Reporter = new Info(true),
                        ReporterOrganization = new Info(true),
                        EventDataDetail = new EventDataDetails_Info()
                        {
                            OperationName = new Info(true),
                            ProductName = new Info(true),
                            ProductRev = new Info(true),
                            ResourceGroup = new Info(true),
                            ResourceName = new Info(true),
                            WorkflowName = new Info(true),
                            WorkflowRev = new Info(true),
                            WorkflowStack = new Info(true),
                            WorkflowStepName = new Info(true)
                        }
                    }
                };

                var result = new UpdateEventData_Result();

                ResultStatus resultStatus = service.Load(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    ApprovalCycleInquiry.Data = QualityObjectDetail_CompletionApprovalSheet.Data;
                    BriefDescription.Data = result.Value.BriefDescription;
                    DescriptionAria.Data = result.Value.Description;
                    DisallowedTxnGrid.Data = result.Value.DisallowedTxns;
                    PriorityLevel.Data = result.Value.PriorityLevel;
                    OccurenceDate.Data = (DateTime)result.Value.OccurrenceDate;
                    DiscoveryArea.Data = result.Value.DiscoveryArea;
                    Initiator.Data = result.Value.Initiator;
                    Resource.Data = result.Value.EventDataDetail.ResourceName;
                    ResourceGroup.Data = result.Value.EventDataDetail.ResourceGroup;
                    Reporter.Data = result.Value.Reporter;
                    ReportedDate.Data = (DateTime)result.Value.ReportedDate;
                    InitiatorOrganization.Data = result.Value.InitiatorOrganization;
                    ReporterOrganization.Data = result.Value.ReporterOrganization;
                    if (ChargeToWorkflow == null || result.Value.EventDataDetail == null ||
                        result.Value.EventDataDetail.WorkflowName == null ||
                        result.Value.EventDataDetail.WorkflowStepName == null) return;
                    ChargeToWorkflow.Data = result.Value.EventDataDetail.WorkflowName.Value;
                    ChargeToWorkflow.StepControl.Data = result.Value.EventDataDetail.WorkflowStepName.Value;
                }
                else
                {
                    DisplayMessage(resultStatus);
                }
            }
        }

        private void GeneralSave_Click(object sender, EventArgs e)
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new UpdateEventDataService(session.CurrentUserProfile);
                var serviceData = new UpdateEventData()
                {
                    QualityESigDetail = ESigCaptureUtil.CollectQualityESigDetail(),
                    QualityObject = new NamedObjectRef() { CDOTypeName = "Event", Name = InstanceID.Data.ToString() },
                    BriefDescription = BriefDescription.Data != null ? BriefDescription.Data.ToString() : null,
                    Description = DescriptionAria.Data != null ? DescriptionAria.Data.ToString() : null,
                    DisallowedTxns = DisallowedTxnGrid.Data != null ? (DisallowedTxnGrid.Data as Primitive<int>[]).Where(t=>t!=null).Select(t => new Primitive<int> { Value = t.Value }).ToArray() : null,
                    PriorityLevel = PriorityLevel.Data as NamedObjectRef,
                    OccurrenceDate = OccurenceDate.Data != null ? new Primitive<DateTime>((DateTime)OccurenceDate.Data) : null,
                    DiscoveryArea = DiscoveryArea.Data != null ? DiscoveryArea.Data.ToString() : null,
                    Initiator = Initiator.Data as NamedObjectRef,
                    Reporter = Reporter.Data as NamedObjectRef,
                    ReportedDate = ReportedDate.Data != null ? new Primitive<DateTime>((DateTime)ReportedDate.Data) : null,
                    InitiatorOrganization = InitiatorOrganization.Data as NamedObjectRef,
                    ReporterOrganization = ReporterOrganization.Data as NamedObjectRef,
                    EventDataDetail = new EventDataDetails()
                    {
                        ResourceName = Resource.Data != null ? Resource.Data.ToString() : null,
                        ResourceGroup = ResourceGroup.Data as NamedObjectRef,
                        WorkflowName = (ChargeToWorkflow != null && ChargeToWorkflow.Data != null) ? ((Camstar.WCF.ObjectStack.RevisionedObjectRef)ChargeToWorkflow.Data).Name : null,
                        WorkflowRev = (ChargeToWorkflow != null && ChargeToWorkflow.Data != null) ? ((Camstar.WCF.ObjectStack.RevisionedObjectRef)ChargeToWorkflow.Data).Revision : null,
                        WorkflowStepName = (ChargeToWorkflow != null && ChargeToWorkflow.Data != null) ? (ChargeToWorkflow.StepControl.Data as NamedSubentityRef).Name : null
                    }
                };

                var request = new UpdateEventData_Request();
                var result = new UpdateEventData_Result();

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
}
