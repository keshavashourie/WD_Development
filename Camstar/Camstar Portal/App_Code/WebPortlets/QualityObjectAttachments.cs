// Copyright Siemens 2023  
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.PortalFramework;
using CWF = Camstar.WebPortal.FormsFramework;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using System.Web.UI.WebControls.WebParts;
using Camstar.WebPortal.Constants;
using System.Web.UI;
using Camstar.WebPortal.Utilities;
using OM = Camstar.WCF.ObjectStack;
using CamstarPortal.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WCF.Services;
using Camstar.WebPortal.WCFUtilities;
using Camstar.Portal;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using System.Data;

namespace Camstar.WebPortal.WebPortlets
{
    public class QualityObjectAttachments : MatrixWebPart, ISupportAttachments
    {
        public QualityObjectAttachments()
        {
            Title = "Quality Object Attachments";
            this.Width = Unit.Pixel(706);
        }

        /// <summary>
        /// Processing Mode
        /// </summary>
        public virtual ProcessingModeType ProcessingMode
        {
            get
            {
                ProcessingModeType mode = ProcessingModeType.ShopFloor;
                if (Page.PageExecutionMode == WebPartPageBase.PageExecutionModeType.Pageflow)
                {
                    mode = ProcessingModeType.Maintenance;
                }
                else
                {
                    RecordViewContext context = Page.PortalContext as RecordViewContext;
                    if (context != null)
                        mode = (context.PageMode == EquivalencyPageMode.Edit ? ProcessingModeType.Maintenance : ProcessingModeType.ShopFloor);
                }
                return mode;
            }
        }

        #region ISupportAttachments Members

        string ISupportAttachments.AttachmentsID
        {
            get { return ViewState[mkAttachmentsID] as string; }
            set { ViewState[mkAttachmentsID] = value; }
        }

        bool ISupportAttachments.IsUpdate
        {
            set { ViewState[mkIsUpdate] = value; }
            get { return ViewState[mkIsUpdate] != null ? (bool)ViewState[mkIsUpdate] : false; }
        }

        #endregion

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            _helper = new AttachmentsHelper(this);

            this.Page.OnSetPageflowControls += new EventHandler<EventArgs>(Page_OnSetPageflowControls);
        }

        protected virtual void Page_OnSetPageflowControls(object sender, EventArgs e)
        {
            //_attachments.Data = this.Page.PageflowControls.GetDataByDataPath("AttachmentsGrid");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            AttachmentsGrid.GridContext.RowSelected += AttachmentsGrid_RowSelected;
            AttachmentsGrid.GridContext.RowUpdated += AttachmentsGrid_RowUpdated;
            AttachmentsGrid.GridContext.RowUpdating += AttachmentsGrid_RowUpdating;

            if (ViewAttachmentButton != null)
            {
                string attachmentsId = string.Empty;
                var locSession = new CallStack(AttachmentsGrid.CallStackKey).Context.LocalSession;
                var pageflow = StateMachineManager.GetPageflow(locSession) as PageFlowStateMachine;
                if (pageflow != null)
                    attachmentsId = (pageflow.Context as PageflowControlsCollection).GetDataByFieldExpression("AttachmentsID") as string;
               
               if (ViewAttachmentButton.OnClientClick.Contains(",''") || string.IsNullOrEmpty(ViewAttachmentButton.OnClientClick))
                {
                    ViewAttachmentButton.OnClientClick = "DownloadAttachment('" + AttachmentsGrid.ClientID + "','" +
                                                         attachmentsId + "')";
                    if(!string.IsNullOrEmpty(attachmentsId))
                        CamstarWebControl.SetRenderToClient(ViewAttachmentButton);
                }

                if (Page.Request["__EVENTARGUMENT"] == "UpdateQualityObjectInfo")
                    Page.DistributeDataContract();

                CamstarWebControl.SetRenderToClient(ViewAttachmentButton);
            }
                
            if (Page.Request["__EVENTARGUMENT"] == "SaveDataRow_completed")
            {
                AttachmentsHelper.SaveAttachment(null, null);
            }
            if (Page.CurrentPageFlow != null && Page.CurrentPageFlow.AssociatedData != null)
            {
                QualityObject.Data = Page.CurrentPageFlow.AssociatedData;
            }
        }

        protected ResponseData AttachmentsGrid_RowUpdating(object sender, JQGridEventArgs args)
        {
            if (args.InputData.Count != 0 && String.IsNullOrWhiteSpace(args.InputData["OriginalFileName"].ToString()))
            {
                var selectedItem = (AttachmentsGrid.SelectedItem as OM.AttachedDoc);

                if (selectedItem != null)
                    args.InputData["OriginalFileName"] = Path.GetFileName(selectedItem.OriginalFilePath.ToString());
            }

            return null;
        }

        private ResponseData AttachmentsGrid_RowUpdated(object sender, JQGridEventArgs args)
        {
            if (args.State.AddRow)
                return null;
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new UpdateEventDataService(session.CurrentUserProfile);
                var serviceData = new OM.UpdateEventData();
                if (QualityObject == null)
                {
                    var eventName = Page.DataContract.GetValueByName("EventNameDM");
                    if(eventName!=null)
                        serviceData.QualityObject = new OM.NamedObjectRef(eventName.ToString(), "Event");
                }
                else
                    serviceData.QualityObject = QualityObject.Data as OM.NamedObjectRef;

                var request = new UpdateEventData_Request()
                {
                    Info = new OM.UpdateEventData_Info()
                    {
                        QualityObjectDetail = new OM.QualityObjectStatusDetail_Info
                        {
                            Attachments = new OM.DocAttachments_Info
                            {
                                Documents = new OM.AttachedDoc_Info
                                {
                                    RequestValue = true
                                }
                            }
                        }
                    }
                };

                var result = new UpdateEventData_Result();

                OM.ResultStatus resultStatus = service.Load(serviceData, request, out result);

                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    if (result.Value != null && result.Value.QualityObjectDetail != null && result.Value.QualityObjectDetail.Attachments != null)
                        AttachmentsGrid.Data = result.Value.QualityObjectDetail.Attachments.Documents;           
                }
                else
                {
                    DisplayMessage(resultStatus);
                }
            }
            return null;
        }

        protected virtual ResponseData AttachmentsGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            if (args.Response != null)
               ((DirectUpdateData)args.Response).PropertyValue = ((DirectUpdateData)args.Response).PropertyValue.Replace("PostBackRequested:false", "PostBackRequested:true");
            return args.Response;
        }

        protected override void AddFieldControls()
        {
        }

        public override void DisplayValues(Camstar.WCF.ObjectStack.Service serviceData)
        {
            base.DisplayValues(serviceData);
            Page.PortalContext.LocalSession["AttachmentsID"] = _helper.GetAttachmentsID(serviceData, string.Format(mkDetailsPattern, PrimaryServiceType, "Attachments"));
        }

        protected AttachmentsHelper _helper;

        protected const string mkAttachmentsID = "poaid";
        protected const string mkIsUpdate = "isupdatea";
        protected const string mkDetailsPattern = "{0}.QualityObjectDetail.{1}";
        protected virtual CWC.Button ViewAttachmentButton
        {
            get { return Page.FindCamstarControl("ViewAttachmentButton") as CWC.Button; }
        }
        protected virtual JQDataGrid AttachmentsGrid
        {
            get { return Page.FindCamstarControl("AttachmentsGrid") as JQDataGrid; }
        }

        protected virtual CWC.NamedObject QualityObject
        {
            get { return Page.FindCamstarControl("UpdateEvent_QualityObject") as CWC.NamedObject; }
        }
    }// QOAttacments
}
