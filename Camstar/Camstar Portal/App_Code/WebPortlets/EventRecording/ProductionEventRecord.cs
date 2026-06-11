// Copyright Siemens 2023  
using System;
using System.Linq;
using System.Web;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using UIAction = Camstar.WebPortal.Personalization.UIAction;

namespace Camstar.WebPortal.WebPortlets.EventRecording
{
    /// <summary>
    /// ProductionEventRecord - handles adding items to the grid when they are entered into the textbox or
    /// when the page is launched from another page like Container Search.
    /// </summary>
    public class ProductionEventRecord : MatrixWebPart
    {

        #region Controls

        protected virtual TextBox AddContainerTextBox
        {
            get { return Page.FindCamstarControl("AddContainer") as TextBox; }
        }

        protected virtual JQDataGrid AffectedContainersGrid
        {
            get { return Page.FindCamstarControl("AffectedContainers") as JQDataGrid; }
        }

        protected virtual TextBox EventName
        {
            get { return Page.FindCamstarControl("EventName") as TextBox; }
        }

        protected virtual NamedObject Classification
        {
            get { return Page.FindCamstarControl("Classification") as NamedObject; }
        }

        protected virtual NamedObject Subclassification
        {
            get { return Page.FindCamstarControl("Subclassification") as NamedObject; }
        }

        protected virtual NamedObject FailureModeGroup
        {
            get { return Page.FindCamstarControl("CreateProductionEvent_FailureModeGroup") as NamedObject; }
        }

        protected virtual TextBox DescriptionField
        {
            get { return Page.FindCamstarControl("ServiceDetail_Description") as TextBox; }
        }

        protected virtual UIAction ResetAction { get { return Page.ActionDispatcher.PageActions().FirstOrDefault(a => a is CustomAction); } }

        #endregion

        #region Protected Functions

        /// <summary>
        /// Evaluate data contracts. Automatically populate containers into the grid if this page is launched from the Container Search page.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            (ResetAction.Control as Button).Click += ProductionEventRecordReset_Click;
            Page.OnClearValues += Page_OnClearValues;

            Page.DataContract.SetValueByName("Classification", Classification.Data);
            Page.DataContract.SetValueByName("SubClassification", Subclassification.Data);

            var eventNameDM = Page.DataContract.GetValueByName("EventNameDM");
            if (EventName != null && eventNameDM != null && !string.IsNullOrEmpty(eventNameDM.ToString()))
            {
                EventName.Data = eventNameDM.ToString();
            }

            //This data contract gets set from the container search 
            //to automatically populate the container in the grid
            if (!Page.IsPostBack && Page.DataContract.GetValueByName("Containers") != null)
            {
                var containers = Page.DataContract.GetValueByName("Containers") as string[];
                if (containers != null)
                    Array.ForEach(containers, n => AddNewContainer(n));
            }
            if (!Page.IsPostBack)
                LoadDefaultValues();
        }

        protected virtual void Page_OnClearValues(object sender, FormsFramework.ServiceDataEventArgs e)
        {
            LoadDefaultValues();
        }

        protected virtual void ProductionEventRecordReset_Click(object sender, EventArgs e)
        {
            LoadDefaultValues();
        }
        #endregion

        #region Public Functions

        /// <summary>
        /// Adds container and sets focus back to the text box
        /// </summary>
        public virtual void AddNewContainer()
        {
            var container = AddContainerTextBox.TextControl.Text;
            if (string.IsNullOrEmpty(container))
                return;

            AddNewContainer(container);
            AddContainerTextBox.Focus();
        }

        /// <summary>
        /// Adds the container to the grid
        /// </summary>
        /// <param name="container"></param>
        public virtual void AddNewContainer(string container)
        {
            var service = new CreateProductionEventService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);

            CreateProductionEvent data = new CreateProductionEvent();
            CreateProductionEvent_Request request = new CreateProductionEvent_Request();
            CreateProductionEvent_Result result;

            WCFObject cdoObj = new WCFObject(data);
            cdoObj.SetValue(".ContainerName", container);

            CreateProductionEvent_Info infoObj = new CreateProductionEvent_Info() { Containers = FieldInfoUtil.RequestSelectionValue() };
            request.Info = infoObj;

            ResultStatus status = service.GetEnvironment(data, request, out result);
            if (status.IsSuccess)
            {
                RecordSet selectionValues = result.Environment.Containers.SelectionValues;
                if (selectionValues != null)
                    AffectedContainersGrid.SetSelectionValues(selectionValues);
            }
            else
                DisplayMessage(status);
        }


        #endregion

        protected virtual void LoadDefaultValues()
        {
            var service = new CreateProductionEventService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);

            CreateProductionEvent data = new CreateProductionEvent();
            var serviceDetail = new EventDetail();
            var obj = new WCFObject(serviceDetail);
            var eventDetail = WCFObject.CreateObject(obj.GetFieldType("EventDataDetail")) as EventDataDetails;
            if (eventDetail != null)
                obj.SetValue("EventDataDetail", eventDetail);
            data.ServiceDetail = serviceDetail;
            data.ServiceDetail.EventDataDetail.EventFailureDetails = new[] { new EventFailureDetail() };
            CreateProductionEvent_Request request = new CreateProductionEvent_Request()
            {
                Info = new CreateProductionEvent_Info()
                {
                    Organization = new Info(true),
                    Classification = new Info(true),
                    SubClassification = new Info(true)
                }
            };
            var serviceDetailInfo = new EventDetail_Info();
            var objInf = new WCFObject(serviceDetailInfo);
            var eventDataDetailsInfo = WCFObject.CreateObject(objInf.GetFieldType("EventDataDetail")) as EventDataDetails_Info;
            if (eventDataDetailsInfo != null)
            {
                eventDataDetailsInfo.EventFailureDetails = new EventFailureDetail_Info
                {
                    FailureModeGroup = new Info(true)
                };
                objInf.SetValue("EventDataDetail", eventDataDetailsInfo);
            }
            request.Info.ServiceDetail = serviceDetailInfo;
            CreateProductionEvent_Result result;

            ResultStatus status = service.GetEnvironment(data, request, out result);
            if (status.IsSuccess)
            {
                if (result != null && result.Value != null)
                {
                    if (result.Value.Classification == null && result.Value.SubClassification == null)
                    {
                        var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                        Page.DisplayWarning(labelCache.GetLabelByName("Lbl_PEDefaultNotCheckedInEventSpecMap").Value);
                    }

                    Classification.Data = result.Value.Classification;
                    Subclassification.Data = result.Value.SubClassification;
                    FailureModeGroup.Data =
                        result.Value.ServiceDetail.EventDataDetail.EventFailureDetails[0].FailureModeGroup;
                }
            }
            else
                DisplayMessage(status);

            DescriptionField.Focus();
        }
    }
}
