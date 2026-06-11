// Copyright Siemens 2025
using System;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.PortalFramework;
using Camstar.WCF.Services;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    /// <summary>
    /// Summary description for RevisionedObjectGroup
    /// </summary>
    public class RevisionedObjectGroup : MatrixWebPart
    {

        protected virtual Button ResolveEntriesBtn
        {
            get { return Page.FindCamstarControl("ResolveEntriesBtn") as Button; }
        }

        protected virtual JQDataGrid EntriesGrid
        {
            get { return Page.FindCamstarControl("EntriesGrid") as JQDataGrid; }
        }

        protected virtual JQDataGrid GroupsGrid
        {
            get { return Page.FindCamstarControl("GroupsGrid") as JQDataGrid; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ResolveEntriesBtn.Click += ResolveEntriesBtn_Click;
            Page.DataSubmissionMode = DataSubmissionModeType.NotSet;
        }

        protected virtual void ResolveEntriesBtn_Click(object sender, EventArgs e)
        {
            OpenResolvedEntriesPage();
        }

        public virtual void OpenResolvedEntriesPage()
        {
            System.Data.DataTable resolvedEntries = null;
            if (!(EntriesGrid.Data as RevisionedObjectRef[]).IsNullOrEmpty() || !(GroupsGrid.Data as NamedObjectRef[]).IsNullOrEmpty())
                try
                {
                    resolvedEntries = LoadResolvedEntries();
                }
                catch (ApplicationException ex)
                {
                    Page.StatusBar.WriteError(ex.Message);
                    return;
                }

            OpenResolvedEntriesPage(resolvedEntries);
        }

        protected virtual void OpenResolvedEntriesPage(System.Data.DataTable resolvedEntries)
        {
            FloatPageOpenAction floatAction = new FloatPageOpenAction();
            floatAction.FrameLocation = new UIFloatingPageLocation();
            floatAction.PageName = "ResolvedEntries_VP";
            floatAction.FrameLocation.Width = 350;
            floatAction.FrameLocation.Height = 500;


            ActionDispatcher dispatcher = Page.ActionDispatcher;


            dispatcher.DataContract.SetValueByName("ResolvedEntriesDM", resolvedEntries);
            dispatcher.ExecuteAction(floatAction);
        }

        protected virtual System.Data.DataTable LoadResolvedEntries()
        {
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            WSDataCreator creator = new WSDataCreator();

            RevisionedObjectGroupMaint serviceData = creator.CreateServiceData(PrimaryServiceType) as RevisionedObjectGroupMaint;
            Maintenance_Info serviceInfo = creator.CreateServiceInfo(PrimaryServiceType) as Maintenance_Info;

            IMaintenanceBase service = creator.CreateService(PrimaryServiceType, profile) as IMaintenanceBase;
            Request request = creator.CreateObject(PrimaryServiceType + "_Request") as Request;
            Result result = creator.CreateObject(PrimaryServiceType + "_Result") as Result;

            Page.GetInputData(serviceData);
            service.BeginTransaction();

            RevisionedObjectGroupMaint reqData = creator.CreateServiceData(PrimaryServiceType) as RevisionedObjectGroupMaint;

            Maintenance data1 = creator.CreateServiceData(PrimaryServiceType) as Maintenance;
            Type type = new WCFObject(serviceData).GetFieldType("ObjectChanges");
            data1.ObjectChanges = WCFObject.CreateObject(type) as RevisionedObjectGroupChanges;
            (data1.ObjectChanges as RevisionedObjectGroupChanges).Entries = serviceData.ObjectChanges.Entries;
            (data1.ObjectChanges as RevisionedObjectGroupChanges).Groups = serviceData.ObjectChanges.Groups;

            if (Page.PortalContext.DataContract.GetValueByName("SelectedInstanceRef") != null)
            {
                reqData.ObjectToChange = new NamedObjectRef(serviceData.ObjectChanges.Name.Value);
                service.Load(reqData);
                serviceData.ObjectToChange = null;
                service.AddDataTransaction(data1);
            }
            else 
            { 
                service.New(data1);
            }

            request.Info = serviceInfo;

            type = new WCFObject(serviceInfo).GetFieldType("ObjectChanges");
            (request.Info as Maintenance_Info).ObjectChanges = WCFObject.CreateObject(type) as RevisionedObjectGroupChanges_Info;
            ((request.Info as Maintenance_Info).ObjectChanges as RevisionedObjectGroupChanges_Info).ResolvedEntries = new Info(false, true);

            ResultStatus status = service.CommitTransaction(request, out result);

            if (!status.IsSuccess)
                throw new ApplicationException(status.ExceptionData.Description);

            return (result.Environment as RevisionedObjectGroupMaint_Environment).ObjectChanges.ResolvedEntries.SelectionValues.GetAsDataTable();
        }

    }
}
