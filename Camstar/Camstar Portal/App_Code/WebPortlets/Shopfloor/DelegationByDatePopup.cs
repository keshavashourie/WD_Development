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
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.WCFUtilities;
using DocumentFormat.OpenXml.Drawing.Charts;
using UIAction = Camstar.WebPortal.Personalization.UIAction;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class DelegationByDatePopup : MatrixWebPart
    {
        protected virtual NamedObject AssignedEmployee { get { return Page.FindCamstarControl("AssignedEmployee") as NamedObject; } }
        protected virtual NamedObject TargetEmployee { get { return Page.FindCamstarControl("TargetEmployee") as NamedObject; } }
        protected virtual NamedObject DelegationReason { get { return Page.FindCamstarControl("DelegationReason") as NamedObject; } }
        protected virtual DropDownList ApplicationType { get { return Page.FindCamstarControl("ApplicationType") as DropDownList; } }
        protected virtual DateChooser StartDate { get { return Page.FindCamstarControl("StartDate") as DateChooser; } }
        protected virtual DateChooser EndDate { get { return Page.FindCamstarControl("EndDate") as DateChooser; } }
        protected virtual CheckBox EnableDelegation { get { return Page.FindCamstarControl("EnableDelegation") as CheckBox; } }
        protected virtual TextBox Comments { get { return Page.FindCamstarControl("Comments") as TextBox; } }
        protected virtual UIAction OkAction { get { return Page.ActionDispatcher.PageActions().FirstOrDefault(a => a is CustomAction); } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                var status = Page.SessionVariables.GetValueByName("DelegationStatus");
                LoadData();
                DisabledControlDependingStatus(status);
                
            }

            if (AssignedEmployee.Data == null || TargetEmployee.Data == null || DelegationReason.Data == null ||
               ApplicationType.Data == null || StartDate.Data == null || EndDate.Data == null)
                OkAction.IsDisabled = true;
            else
            {
                OkAction.IsDisabled = false;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager.RegisterStartupScript(Page.Form, Page.Form.GetType(), "DataChanged",
                string.Format("DataChangeControls();"), true);//send postback from start and end date controls
        }

        protected virtual void DisabledControlDependingStatus(object delegationStatus)
        {
            if (delegationStatus != null)
            {
                if (delegationStatus.Equals("Active"))
                {
                    AssignedEmployee.ReadOnly = true;
                    TargetEmployee.ReadOnly = true;
                    DelegationReason.ReadOnly = true;
                    ApplicationType.ReadOnly = true;
                    StartDate.ReadOnly = true;
                    EnableDelegation.ReadOnly = true;
                    Comments.ReadOnly = true;
                }
                else if (delegationStatus.Equals("Expired"))
                {
                    AssignedEmployee.ReadOnly = true;
                    TargetEmployee.ReadOnly = true;
                    DelegationReason.ReadOnly = true;
                    ApplicationType.ReadOnly = true;
                    StartDate.ReadOnly = true;
                    EndDate.ReadOnly = true;
                    EnableDelegation.ReadOnly = true;
                    Comments.ReadOnly = true;
                }

            }
        }

        protected virtual void LoadData()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new DelegationTaskMaintService(session.CurrentUserProfile);

                var serviceData = new DelegationTaskMaint();

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

                ResultStatus resultStatus = service.Load(serviceData, request, out result);
                if (resultStatus.IsSuccess)
                {
                    var data = result.Value.ObjectChanges;
                    AssignedEmployee.Data = data.AssignedEmployee;
                    TargetEmployee.Data = data.TargetEmployee;
                    DelegationReason.Data = data.DelegationReason;
                    ApplicationType.Data = data.ApplicationType.Value;
                    StartDate.Data = (DateTime)data.StartDateGMT;
                    EndDate.Data = (DateTime)data.EndDateGMT;
                    EnableDelegation.Data = (bool)data.DelegationForwarding;
                    Comments.Data = data.Comments;
                }

            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "NotifyParent":
                        {
                            if (Page.SessionVariables.GetValueByName("DelegateTaskId") == null)
                                e.Result = ExecuteAddAction();

                            else
                                e.Result = ExecuteUpdateAction();

                            if (e.Result != null && e.Result.IsSuccess)
                            {
                                Page.CloseFloatingFrame(true);
                                Page.SessionVariables.SetValueByName("Reload", true);
                                Page.SessionVariables.SetValueByName("DelegateTaskId", null);
                                Page.SessionVariables.SetValueByName("DelegationStatus",null);
                            }
                            else
                            {
                                Page.DisplayMessage(e.Result);
                            }

                            break;
                        }
                    case "Reset":
                        {
                            Page.ClearDataValues();
                            OkAction.IsDisabled = true;
                            break;
                        }
                }
            }
        }

        protected virtual ResultStatus ExecuteUpdateAction()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(Page.Session);
            WSDataCreator creator = new WSDataCreator();

            DelegationTaskMaint serviceData = creator.CreateServiceData("DelegationTaskMaint") as DelegationTaskMaint;
            DelegationTaskMaint_Info serviceInfo = creator.CreateServiceInfo("DelegationTaskMaint") as DelegationTaskMaint_Info;

            IMaintenanceBase service = creator.CreateService("DelegationTaskMaint", session.CurrentUserProfile) as IMaintenanceBase;
            Request request = creator.CreateObject("DelegationTaskMaint_Request") as Request;
            Result result = creator.CreateObject("DelegationTaskMaint_Result") as Result;

            request.Info = serviceInfo;

            service.BeginTransaction();

            DelegationTaskMaint data1 = creator.CreateServiceData("DelegationTaskMaint") as DelegationTaskMaint;
            DelegationTaskMaint data2 = creator.CreateServiceData("DelegationTaskMaint") as DelegationTaskMaint;
            WCFObject wcfData1 = new WCFObject(data1);

            Type type = new WCFObject(serviceData).GetFieldType("ObjectChanges");
            Type type2 = new WCFObject(serviceData).GetFieldType("ObjectToChange");

            data1.ObjectToChange = WCFObject.CreateObject(type2) as SubentityRef;
            data1.ObjectToChange.CDOTypeName = "DelegationTask";
            data1.ObjectToChange.ID = Page.SessionVariables.GetValueByName("DelegateTaskId").ToString();
            service.Load(data1);

            data2.ObjectChanges = WCFObject.CreateObject(type) as DelegationTaskChanges;
            data2.ObjectChanges.ApplicationType = ApplicationType.Data != null ? new Enumeration<ApplicationTypeEnum, int>((int)ApplicationType.Data) : null;
            data2.ObjectChanges.AssignedEmployee = AssignedEmployee.Data as NamedObjectRef;
            data2.ObjectChanges.DelegationForwarding = new Primitive<bool>((bool)EnableDelegation.Data);
            data2.ObjectChanges.DelegationReason = DelegationReason.Data as NamedObjectRef;
            data2.ObjectChanges.EndDateGMT = EndDate.Data != null ? new Primitive<DateTime>((DateTime)EndDate.Data) : null;
            data2.ObjectChanges.StartDateGMT = StartDate.Data != null ? new Primitive<DateTime>((DateTime)StartDate.Data) : null;
            data2.ObjectChanges.TargetEmployee = TargetEmployee.Data as NamedObjectRef;
            data2.ObjectChanges.Comments = Comments.Data as string;

            service.ExecuteTransaction(data2);
            ResultStatus status = service.CommitTransaction(request, out result);

            if (status.IsSuccess)
                Page.SessionVariables.SetValueByName("StatusMessageText", status);

            return status;
        }

        protected virtual ResultStatus ExecuteAddAction()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(Page.Session);
            WSDataCreator creator = new WSDataCreator();

            DelegationTaskMaint serviceData = creator.CreateServiceData("DelegationTaskMaint") as DelegationTaskMaint;
            DelegationTaskMaint_Info serviceInfo = creator.CreateServiceInfo("DelegationTaskMaint") as DelegationTaskMaint_Info;

            IMaintenanceBase service = creator.CreateService("DelegationTaskMaint", session.CurrentUserProfile) as IMaintenanceBase;
            Request request = creator.CreateObject("DelegationTaskMaint_Request") as Request;
            Result result = creator.CreateObject("DelegationTaskMaint_Result") as Result;

            request.Info = serviceInfo;

            service.BeginTransaction();

            DelegationTaskMaint data1 = creator.CreateServiceData("DelegationTaskMaint") as DelegationTaskMaint;
            DelegationTaskMaint data2 = creator.CreateServiceData("DelegationTaskMaint") as DelegationTaskMaint;
            WCFObject wcfData1 = new WCFObject(data1);

            Type type = new WCFObject(serviceData).GetFieldType("ObjectChanges");

            service.New(data1);
            bool isMissed = false;

            data2.ObjectChanges = WCFObject.CreateObject(type) as DelegationTaskChanges;
            data2.ObjectChanges.AssignedEmployee = (WCF.ObjectStack.NamedObjectRef)AssignedEmployee.Data;
            data2.ObjectChanges.TargetEmployee = (WCF.ObjectStack.NamedObjectRef)TargetEmployee.Data;
            data2.ObjectChanges.DelegationReason = (WCF.ObjectStack.NamedObjectRef)DelegationReason.Data;
            data2.ObjectChanges.StartDateGMT = (WCF.ObjectStack.Primitive<DateTime>)((DateTime)StartDate.Data);
            data2.ObjectChanges.EndDateGMT = (WCF.ObjectStack.Primitive<DateTime>)((DateTime)EndDate.Data);
            data2.ObjectChanges.ApplicationType = Enumeration<ApplicationTypeEnum, int>.ConvertToEnum((int)ApplicationType.Data, out isMissed);
            data2.ObjectChanges.DelegationForwarding = (WCF.ObjectStack.Primitive<bool>)((bool)EnableDelegation.Data);
            data2.ObjectChanges.Comments = (string)Comments.Data;

            service.ExecuteTransaction(data2);

            ResultStatus status = service.CommitTransaction(request, out result);

            if(status.IsSuccess)
            Page.SessionVariables.SetValueByName("StatusMessageText", status);

            return status;
        }
    }
}
