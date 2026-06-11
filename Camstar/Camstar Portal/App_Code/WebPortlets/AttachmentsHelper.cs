// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Camstar.WCF.Services;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using CGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CWF = Camstar.WebPortal.FormsFramework;
using OM = Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets
{
    public interface ISupportAttachments
    {
        string AttachmentsID { get; set; }
        bool IsUpdate { get; set; }
    }

    public sealed class AttachmentsHelper 
    {
        private ISupportAttachments _supportAttachment = null;

        private WebPartBase WebPart { get; set; }

        public AttachmentsHelper(WebPartBase webpart)
        {
            WebPart = webpart;
            _supportAttachment = webpart as ISupportAttachments;
            if (_supportAttachment == null)
                throw new Exception("WebPart " + webpart.GetType().Name + "does not support ISupportAttachments interface");

            WebPart.Page.OnPreExecute += new EventHandler<FormProcessingEventArgs>(Page_OnPreExecute);
        }
      
        void Page_OnPreExecute(object sender, FormProcessingEventArgs e)
        {
            if (e.Info != null && e.Data != null)
            {
                var attachmentsInfo = new OM.DocAttachments_Info {RequestValue = true};

                if (WCFObject.IsFieldExist(e.Data.GetType().Name + mkAttachmentsExpr))
                    new WCFObject(e.Info).SetValue(mkAttachmentsExpr, attachmentsInfo);

                if (WebPart.Page.PortalContext.DataContract.GetValueByName("EventNameDM") != null)
                {
                    var evName = new OM.NamedObjectRef(WebPart.Page.PortalContext.DataContract.GetValueByName("EventNameDM").ToString());
                    if (evName != null && e.Data is OM.CreateEvent)
                        (e.Data as OM.CreateEvent).EventName = evName.Name;
                }
            }
        }

        public bool SetAttachmentsID(ref OM.DocAttachmentsTxn data)
        {
            bool eventSaved = false;

            if (!string.IsNullOrEmpty(_supportAttachment.AttachmentsID))
            {
                OM.DocAttachments dAttachments = new Camstar.WCF.ObjectStack.DocAttachments();
                dAttachments.Self = new OM.BaseObjectRef(_supportAttachment.AttachmentsID);
                data.Attachments = dAttachments;
                eventSaved = true;
            }//if
            return eventSaved;
        }//SetAttachmentsID

        static bool SetAttachmentsID(string originalAttID, ref OM.DocAttachmentsTxn data)
        {
            bool eventSaved = false;

            if (!string.IsNullOrEmpty(originalAttID))
            {
                OM.DocAttachments dAttachments = new Camstar.WCF.ObjectStack.DocAttachments();
                dAttachments.Self = new OM.BaseObjectRef(originalAttID);
                data.Attachments = dAttachments;
                eventSaved = true;
            }//if
            return eventSaved;
        }//SetAttachmentsID

        public string GetAttachmentsID(Camstar.WCF.ObjectStack.Service serviceData, string fieldExp)
        {
            if (serviceData != null && serviceData.GetType().Name == WebPart.PrimaryServiceType && WCFObject.IsFieldExist(fieldExp))
            {
                WCFObject wcfo = new WCFObject(serviceData);
                OM.DocAttachments docAtt = wcfo.GetValue(fieldExp) as OM.DocAttachments;
                if (docAtt != null && docAtt.Self != null && !string.IsNullOrEmpty(docAtt.Self.ID))
                    _supportAttachment.AttachmentsID = docAtt.Self.ID;
                else
                    _supportAttachment.AttachmentsID = string.Empty;
            }
            return _supportAttachment.AttachmentsID;
        }

        public static object DeleteAttachment(object docValues, GridContext gridContext)
        {
            OM.ResultStatus txnStatus;
            var doc = docValues as OM.AttachedDoc;
            if (doc != null && doc.FileName != null && !string.IsNullOrEmpty(doc.FileName.Value))
            {
                var locSession = new CallStack(gridContext.CallStackKey).Context.LocalSession;
                var attachmentsID = locSession["AttachmentsID"] as string;
                if (attachmentsID == null)
                {
                    var pageflow = StateMachineManager.GetPageflow(locSession) as PageFlowStateMachine;
                    if (pageflow != null)
                        attachmentsID = (pageflow.Context as PageflowControlsCollection).GetDataByFieldExpression("AttachmentsID") as string;
                }

                var data = new OM.DocAttachmentsTxn
                {
                    DocDetail = new OM.AttachedDocDetail {Version = doc.Version, Name = doc.Name}
                };
                if (SetAttachmentsID(attachmentsID, ref data))
                {
                    DocAttachmentsTxn_Result txnResult = null;
                    txnStatus = AttachmentExecutor.DeleteAttachment(data, UserProfile(), out txnResult);
                }
                else
                {
                    txnStatus = new OM.ResultStatus("AttachmentID is not correct", false);
                }
            }
            else
            {
                // If parameters empty - nothing to delete
                // result is OK
                txnStatus = new OM.ResultStatus(string.Empty, true);
            }
            gridContext.BubbleMessage = txnStatus.Message;
            return txnStatus;
        }

        public static object SaveAttachment(object docValues, GridContext gridContext)
        {
            if (gridContext == null)
                return null;

            var isAdding = gridContext.SelectedRowIDs != null && gridContext.SelectedRowIDs.Count == 0;

            var pars = docValues as Dictionary<string, object>;
            if (pars != null && pars.Count == 0)
                return null;

            object ret = null;

            var txnStatus = new OM.ResultStatus(string.Empty, true);
            var callStackKey = gridContext.CallStackKey;

            var frm = (gridContext.Host as Control).Page as CamstarForm;

            string docPath = CWC.FileInput.UploadFilePath;
            if (docPath != null)
            {
                HttpContext.Current.Session.Add("uploadedFile", docPath);
            }
            else
            {
                docPath = HttpContext.Current.Session["uploadedFile"] as string;
                HttpContext.Current.Session.Remove("uploadedFile");
            }
            var locSession = new CallStack(callStackKey).Context.LocalSession;
            locSession.Remove("uploadedFile");

            if (string.IsNullOrEmpty(docPath) && isAdding)
            {
                txnStatus.Message = "The File field should be entered";
                txnStatus.IsSuccess = false;
                gridContext.BubbleMessage = txnStatus.Message;
                return txnStatus;
            }

            string docName = "";
            string docVersion, docDescription;

            if (docValues != null && pars != null)
            {
                if(pars.Keys.Contains("Name"))
                    docName = (string) pars["Name"];
                else if (pars.Keys.Contains("Title"))
                    docName = (string)pars["Title"];

                docVersion = pars["Version"] as string;
                docDescription = pars["Description"] as string;
            }
            else
            {
                docName = HttpContext.Current.Session["attachmentDocName"] as string;
                HttpContext.Current.Session.Remove("attachmentDocName");
                docVersion = HttpContext.Current.Session["attachmentDocVersion"] as string;
                HttpContext.Current.Session.Remove("attachmentDocVersion");
                docDescription = HttpContext.Current.Session["attachmentDocDescription"] as string;
                HttpContext.Current.Session.Remove("attachmentDocDescription");
            }

            var data = new OM.DocAttachmentsTxn();
            var attachmentsID = locSession["AttachmentsID"] as string;
            var pageflow = StateMachineManager.GetPageflow(locSession) as PageFlowStateMachine;
            if (attachmentsID == null)
            {
                if (pageflow != null)
                    attachmentsID = (pageflow.Context as PageflowControlsCollection).GetDataByFieldExpression("AttachmentsID") as string;
            }
            if (attachmentsID == null && frm != null)
            {
                var serviceName = frm.Service.Service.GetType().Name;
                var qt = serviceName.Replace("Service", "");

                // Create quality object
                object svc = null;
                var cs = frm.Service.Service.GetType().GetConstructor(new [] { typeof(OM.UserProfile) });
                if (cs != null)
                    svc = cs.Invoke(new object[] { UserProfile() });

                var cdo = WCFObject.CreateObject(qt);
                var dataContracts = new CallStack(callStackKey).Context.DataContract;

                var cdoCreate = cdo as OM.CreateEvent;
                if (cdoCreate != null)
                {
                    cdoCreate.Organization = dataContracts.GetValueByName<OM.NamedObjectRef>("OrganizationDM");
                    cdoCreate.Classification = new OM.NamedObjectRef(dataContracts.GetValueByName("Classification") == null ? null : dataContracts.GetValueByName("Classification").ToString());
                    cdoCreate.SubClassification = new OM.NamedObjectRef(dataContracts.GetValueByName("SubClassification") == null ? null : dataContracts.GetValueByName("SubClassification").ToString());
                    cdoCreate.Submit = false;
                    if (pageflow != null && pageflow.AssociatedData != null)
                        cdoCreate.EventName = pageflow.AssociatedData.ToString();
                }

                var req = WCFObject.CreateObject(qt + "_Request");

                var reqInfo = WCFObject.CreateObject(qt + "_Info") as OM.CreateEvent_Info;
                reqInfo.QualityObject = new OM.Info(true);
                reqInfo.QualityObjectDetail = new OM.QualityObjectStatusDetail_Info() { Attachments = new OM.DocAttachments_Info() { RequestValue = true } };

                new WCFObject(req).SetValue("Info", reqInfo);

                var exeMethod = svc.GetType().GetMethods().FirstOrDefault(m => m.Name == "ExecuteTransaction" && m.GetParameters().Count() == 3);
                if (exeMethod != null)
                {
                    var parms = new object[] { cdo, req, null };
                    txnStatus = exeMethod.Invoke(svc, parms) as OM.ResultStatus;
                    if (txnStatus != null && txnStatus.IsSuccess)
                    {
                        var res = new WCFObject(parms[2]);
                        var value = res.GetValue("Value") as OM.QualityTxn;
                        frm.DisplayValues(value);

                        var qdet = res.GetValue("Value.QualityObjectDetail") as OM.QualityObjectStatusDetail;
                        if (qdet != null)
                        {
                            attachmentsID = qdet.Attachments.Self.ID;
                            locSession["AttachmentsID"] = attachmentsID;
                            var eventObj = res.GetValue("Value.QualityObject") as OM.NamedObjectRef;
                            var eventName = eventObj != null ? eventObj.Name : null;
                            dataContracts.SetValueByName("EventNameDM", eventName);
                            locSession["EventNameDM"] = eventName;

                            // Save Header control values - in the Pageflow collection
                            if (pageflow != null)
                            {
                                var pfControls = pageflow.Context as PageflowControlsCollection;
                                var ctl = pfControls["UpdateEvent_QualityObject"];
                                if (ctl != null)
                                    ctl.Data = eventObj;
                                pfControls.Add("AttachmentsID", attachmentsID);
                                pageflow.AssociatedData = eventObj;
                                pageflow.Flush();
                            }
                        }
                    }
                }
            }

            // Try to get Doc attachments ID 
            if (SetAttachmentsID(attachmentsID, ref data))
            {
                data.DocDetail = new OM.AttachedDocDetail
                {
                    Name = docName,
                    Version = docVersion,
                    Description = docDescription,
                    OriginalFilePath = docPath
                };

                DocAttachmentsTxn_Result txnResult;

                if (isAdding)
                {
                    txnStatus = AttachmentExecutor.AttachFile(data, UserProfile(), UserDomain(), out txnResult);
                    CWC.FileInput.RemoveFile(docPath);
                }
                else
                {
                    var context = (gridContext.SelectedItem as OM.AttachedDoc);
                    if (context != null)
                    {
                        data.DocDetail.FileExtension = context.FileExtension;
                        data.DocDetail.OriginalFileName = context.OriginalFileName;
                        data.DocDetail.OriginalFilePath = context.OriginalFilePath;
                        data.DocDetail.FileName = context.FileName;
                        data.DocDetail.FileExtension = context.FileExtension;
                        data.DocDetail.DocContentsId = context.DocContentsId;
                    }

                    var fileShareFileName =
                        FileMgmtUtil.GenerateUniqueFileName(data.DocDetail.OriginalFilePath.Value, UserProfile().Name);
                    string filePath = URIConstantsBase.GetURI(CamstarPortalSection.Settings.DefaultSettings.UploadDirectory, fileShareFileName);

                    txnStatus = AttachmentExecutor.UpdateAttachmentTxn(data, UserProfile(), filePath, out txnResult);
                }

                if (!txnStatus.IsSuccess && string.IsNullOrEmpty(txnStatus.Message) && txnStatus.ExceptionData != null)
                {
                    txnStatus.Message = txnStatus.ExceptionData.Description;
                }

                ret = txnStatus;
            }
            else
            {
                if (pars != null)
                {
                    var ss = HttpContext.Current.Session;
                    ss.Add("attachmentDocName", pars["Name"]);
                    ss.Add("attachmentDocVersion", pars["Version"]);
                    ss.Add("attachmentDocDescription", pars["Description"]);
                }
                ret = new StatusData(false, "AttachmentID is not correct");
            }

            if (ret is OM.ResultStatus)
                gridContext.BubbleMessage = (ret as OM.ResultStatus).Message;
            return ret;
        }

        public static object DownloadAttachment(CellActionCommand cmd, object docValues, GridContext gridContext)
        {
            var doc = docValues as OM.AttachedDoc;
            string attachmentsID = new CallStack(gridContext.CallStackKey).Context.LocalSession["AttachmentsID"] as string;
            string script = string.Format("StartDownloadFile(\"{0}\", \"{1}\", \"{2}\")",
                QueryStringUtil.SafeURL(doc.Name.Value),
                QueryStringUtil.SafeURL(doc.Version.Value),
                attachmentsID);
            return script;
        }

        static OM.UserProfile UserProfile()
        {
            return FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session).CurrentUserProfile;
        }//CurrentUserProfile

        static string UserDomain()
        {
            string domainName = string.Empty;
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (!string.IsNullOrEmpty(session.SessionValues.UserDomain))
            {
                domainName = session.SessionValues.UserDomain;
            }//if
            return domainName;
        }//CurrentUserDomain    

        private const string mkAttachmentsExpr = ".QualityObjectDetail.Attachments";

    }
}
