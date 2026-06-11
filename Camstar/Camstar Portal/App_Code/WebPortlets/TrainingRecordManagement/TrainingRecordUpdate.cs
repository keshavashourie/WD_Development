// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.WebPortlets;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.WebControls;
using System.Data;
using System.Linq;
using System.Collections;
using Camstar.WCF.ObjectStack;
using Action = Camstar.WCF.ObjectStack.Action;
using Camstar.WebPortal.FormsFramework;

using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CWGC = Camstar.WebPortal.FormsFramework.WebGridControls;

using PERS = Camstar.WebPortal.Personalization;

using Camstar.WebPortal.WCFUtilities;
using Camstar.WCF.Services;
using Camstar.WebPortal.PortalFramework;


namespace Camstar.WebPortal.WebPortlets
{
    public class TrainingRecordUpdate : MatrixWebPart
    {
        #region Controls

        protected virtual CWC.NamedObject Employee
        {
            get { return Page.FindCamstarControl("Employee") as NamedObject; }
        }

        protected virtual CWC.NamedObject Status
        {
            get { return Page.FindCamstarControl("Status") as NamedObject; }
        }

        protected virtual CWC.RevisionedObject TrainingRequirement
        {
            get { return Page.FindCamstarControl("TrainingRequirement") as CWC.RevisionedObject; }
        }

        protected virtual CWC.DateChooser ExpirationDate
        {
            get { return Page.FindCamstarControl("ExpirationDate") as DateChooser; }
        }

        protected virtual CWC.TextBox DataVersion
        {
            get { return Page.FindCamstarControl("DataVersion") as TextBox; }
        }

        protected virtual CWC.NamedObject ESigRequirement
        {
            get { return Page.FindCamstarControl("ESigRequirement") as CWC.NamedObject; }
        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Status.DataChanged += Status_DataChanged;
            var action = Page.ActionDispatcher.GetActionByName("SubmitAction");
            if (action != null)
            {
                var button = action.Control as Button;
                if (button != null)
                    button.Attributes["actionCommandName"] = ((int)ESigMaintActions.Update).ToString();
            }
        }
        
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (!Page.IsPostBack)
            {
                TrainingRequirement.OriginalData =
                (OM.RevisionedObjectRef)TrainingRequirement.Data;
            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as PERS.CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "NotifyParent":
                        {
                            e.Result = ExecuteUpdateAction();
                            if (e.Result != null && e.Result.IsSuccess)
                            {
                                Page.CloseFloatingFrame(true);
                                if( !Page.IsResponsive )
                                    Page.CurrentCallStack.Parent.Context.Message = e.Result.Message;
                            }

                            break;
                        }
                }
            }
        }
        protected virtual void Status_DataChanged(object sender, EventArgs e)
        {
            if (Status.Data == null)
            {
                return;
            }

            var maint = new OM.TrainingRecordStatusMaint
            {
                ObjectToChange = new OM.NamedObjectRef(Status.Data.ToString())
            };
            var info = new OM.TrainingRecordStatusMaint_Info
            {
                ObjectChanges = new OM.TrainingRecordStatusChanges_Info
                {
                    Permission = new OM.Info(true)
                }
            };
            OM.Service output = new OM.TrainingRecordStatusMaint();
            OM.ResultStatus status = Service.ExecuteFunction(maint, info, "Load", ref output);
            OM.TrainingRecordStatusMaint output2 = output as OM.TrainingRecordStatusMaint;

            if (status.IsSuccess && output2 != null && output2.ObjectChanges != null)
            {
                if (output2.ObjectChanges.Permission == PermissionEnum.AllowWithESig)
                {
                    ESigRequirement.Visible = true;
                    ESigRequirement.Required = true;
                }
                else
                {
                    ESigRequirement.Visible = false;
                    ESigRequirement.Required = false;
                    ESigRequirement.Data = null;
                }
            }
        }
        protected virtual ResultStatus ExecuteUpdateAction()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(Page.Session);
            WSDataCreator creator = new WSDataCreator();

            TrainingRecordMaint serviceData = creator.CreateServiceData("TrainingRecordMaint") as TrainingRecordMaint;
            TrainingRecordMaint_Info serviceInfo = creator.CreateServiceInfo("TrainingRecordMaint") as TrainingRecordMaint_Info;

            IMaintenanceBase service = creator.CreateService("TrainingRecordMaint", session.CurrentUserProfile) as IMaintenanceBase;
            Request request = creator.CreateObject("TrainingRecordMaint_Request") as Request;
            Result result = creator.CreateObject("TrainingRecordMaint_Result") as Result;

            request.Info = serviceInfo;

            service.BeginTransaction();

            TrainingRecordMaint data1 = creator.CreateServiceData("TrainingRecordMaint") as TrainingRecordMaint;
            TrainingRecordMaint data2 = creator.CreateServiceData("TrainingRecordMaint") as TrainingRecordMaint;

            Type type = new WCFObject(serviceData).GetFieldType("ObjectChanges");

            data1.ParentDataObject = (OM.NamedObjectRef)Employee.Data;
            data1.TrainingRequirement = (OM.RevisionedObjectRef)TrainingRequirement.OriginalData;

            var eSigDetails = ESigCaptureUtil.CollectESigServiceDetailsAll();
            if (eSigDetails != null)
            {
                if (eSigDetails.Item1 != null && eSigDetails.Item1.Length > 0)
                    data1.ESigDetails = eSigDetails.Item1;
            }

            service.Load(data1);

            data2.DataVersion = DataVersion.Data != null ? new OM.Primitive<int>((int)DataVersion.Data) : null;
            data2.ObjectChanges = WCFObject.CreateObject(type) as TrainingRecordChanges;
            data2.ObjectChanges.ExpirationDate = ExpirationDate.Data != null ? new OM.Primitive<DateTime>((DateTime)ExpirationDate.Data) : new OM.Primitive<DateTime>();
            data2.ObjectChanges.Status = (OM.NamedObjectRef)Status.Data;
            data2.ObjectChanges.TrainingRequirement = (OM.RevisionedObjectRef)TrainingRequirement.Data;

            if ((OM.NamedObjectRef)ESigRequirement.Data == null)
                 data2.ObjectChanges.ESigRequirement = new NamedObjectRef("");
            else
                 data2.ObjectChanges.ESigRequirement = (OM.NamedObjectRef)ESigRequirement.Data;

            service.ExecuteTransaction(data2);

            ResultStatus status = service.CommitTransaction(request, out result);

            ESigCaptureUtil.CleanESigCaptureDM();
            return status;
        }
        
    }
}
