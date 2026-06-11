// Copyright Siemens 2025
using System;
using System.Collections.Generic;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.PortalFramework;
using Camstar.WCF.Services;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.FormsFramework.WebControls;
using CWGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;


namespace Camstar.WebPortal.WebPortlets.Modeling
{
    /// <summary>
    /// Summary description for NamedObjectGroup
    /// </summary>
    public class NamedObjectGroup : MatrixWebPart
    {
        private Dictionary<string, string> portalHelpFile = new Dictionary<string, string> {
            { "ToolGroupMaint", @"onlinehelpoutput/portalmodeling_Help/PortalModeling_CSH.htm#ModelingObjects/Defining_Tool_Groups.htm" }
        };

        protected virtual CWGC.MultiSelectPickList ObjectChanges_FilterTags { get { return Page.FindCamstarControl("ObjectChanges_FilterTags") as CWGC.MultiSelectPickList; } }
        protected virtual CWC.TextBox FilterTagsHidden { get { return Page.FindCamstarControl("FilterTagsHidden") as CWC.TextBox; } }
        protected JQDataGrid FilterTagGrid { get { return ObjectChanges_FilterTags.PickListPanelControl.ViewControl as JQDataGrid; } }

        protected virtual Button ResolveEntriesBtn
        {
            get { return Page.FindCamstarControl("ResolveEntriesBtn") as Button; }
        }

        protected virtual DropDownList EntryTypePicklist
        {
            get { return Page.FindCamstarControl("EntryTypePicklist") as DropDownList; }
        }

        protected virtual TextBox EntryType
        {
            get { return Page.FindCamstarControl("EntryType") as TextBox; }
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
            EntriesGrid.GridContext.RowDeleted += EntriesGrid_RowDeleted;
            EntriesGrid.GridContext.RowUpdated += EntriesGrid_RowUpdated;
            //Page.DataSubmissionMode = DataSubmissionModeType.NotSet;
        }
        public override void DisplayValues(Service serviceData)
        {
            base.DisplayValues(serviceData);

            var pc = Page.PortalContext as MaintenanceBehaviorContext;
            if (pc != null)
            {
                if (pc.MaintService == "NamedObjectGroupMaint" || pc.MaintService == "UserCodeGroupMaint")
                {
                    if (pc.State == MaintenanceBehaviorContext.MaintenanceState.New)
                    {
                        EntryTypePicklist.Data = null;
                        EntryType.Data = null;
                    }
                    EntryType.Visible = false;
                    EntryTypePicklist.Visible = true;

                    if (EntriesGrid.IsEmpty == false)
                    {
                        if (EntriesGrid.Data != null)
                            EntryTypePicklist.Enabled = false;
                    }
                    else
                    {
                        EntryTypePicklist.Enabled = true;
                        EntryTypePicklist.Data = null;
                        EntryType.Data = null;
                    }
                }
                else
                {
                    EntryType.Visible = true;
                    EntryTypePicklist.Visible = false;
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            string helpUrl = string.Empty;
            //Set the EntryTypePicklist field to null, so no values are displayed when the page opens.
            if (!Page.IsPostBack)
            {
                var pc = Page.PortalContext as MaintenanceBehaviorContext;
                if (pc != null && pc.State == MaintenanceBehaviorContext.MaintenanceState.New)
                {
                    if (pc.MaintService == "NamedObjectGroupMaint" || pc.MaintService == "UserCodeGroupMaint")
                        EntryTypePicklist.Data = null;
                    EntryType.Data = null;
                }
            }
            //If the grid is empty, then the Entry Type Picklist is Enabled, otherwsie it should be disabled.
            if (EntryTypePicklist.Visible == true)
            {
                if (EntryTypePicklist.Data != null && EntriesGrid.IsEmpty == false)
                    EntryTypePicklist.Enabled = false;
                else
                    EntryTypePicklist.Enabled = true;
            }

            // Programmatically include helpfile
            if (portalHelpFile.ContainsKey(PrimaryServiceType))
                helpUrl = portalHelpFile[PrimaryServiceType];

            if (!string.IsNullOrEmpty(helpUrl))
            {
                Personalization.PageContent content = Page.Model.PublishedContent as PageContent;
                if (content != null)
                {
                    content.HelpFileURL = helpUrl;
                }
            }
        }
        
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            //if (EntryTypePicklist.Visible == true)
            //{
                NamedObjectGroupChanges data = (serviceData as NamedObjectGroupMaint).ObjectChanges;
                if (data.Entries != null)
                {
                    string type = string.Empty;
                    // Get EntryType from the EntryTypeField or from the serviceData
                    
                    if (EntryTypePicklist.Data != null && !string.IsNullOrEmpty(EntryTypePicklist.Data.ToString()))
                        type = EntryTypePicklist.Data.ToString();
                    else if (data.EntryType != null && !string.IsNullOrEmpty(data.EntryType.Value))
                        type = data.EntryType.Value;

                    if (!string.IsNullOrEmpty(type))
                    {
                        foreach (var entry in data.Entries)
                        {
                            entry.CDOTypeName = type;
                        }
                    }
                }
            //}
        }

        public virtual ResponseData EntriesGrid_RowUpdated(object sender, JQGridEventArgs e)
        {
            var response = e.Response as DirectUpdateData;
            //If a row is inserted in the Entries Grid, the EntryTypePicklist must be disabled
            if (EntryTypePicklist.Visible == true)
            {
                if (EntryTypePicklist.Data != null && !string.IsNullOrEmpty(EntryTypePicklist.Data.ToString()))
                {
                    EntryTypePicklist.Enabled = false;
                    //set the post back requested to true so that it triggers the OnPreRender method.
                    response.PropertyValue = response.PropertyValue.Replace("PostBackRequested:false",
                                                                           "PostBackRequested:true");
                }
            }
            return response;
        }

        public virtual ResponseData EntriesGrid_RowDeleted(object sender, JQGridEventArgs e)
        {
            var response = e.Response as DirectUpdateData;
            if (EntryTypePicklist.Visible == true && !EntryTypePicklist.Enabled)
            {
                //check if there are no rows left in the grid.  If not, enable the EntryTypePicklist
                if (EntriesGrid.TotalRowCount == 0)
                {
                    EntryTypePicklist.Enabled = true;
                    //set the post back requested to true so that it triggers the OnPreRender method.
                    response.PropertyValue = response.PropertyValue.Replace("PostBackRequested:false",
                                                                          "PostBackRequested:true");
                }
            }
            return response;
        }


        public virtual void ResolveEntriesBtn_Click(object sender, EventArgs e)
        {
            OpenResolvedEntriesPage();
        }

        public virtual void OpenResolvedEntriesPage()
        {
            System.Data.DataTable resolvedEntries = null;

            if (!(EntriesGrid.Data as NamedObjectRef[]).IsNullOrEmpty() || !(GroupsGrid.Data as NamedObjectRef[]).IsNullOrEmpty())
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

        public virtual void OpenResolvedEntriesPage(System.Data.DataTable resolvedEntries)
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

        public virtual System.Data.DataTable LoadResolvedEntries()
        {
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            WSDataCreator creator = new WSDataCreator();

            NamedObjectGroupMaint serviceData = creator.CreateServiceData(PrimaryServiceType) as NamedObjectGroupMaint;
            Maintenance_Info serviceInfo = creator.CreateServiceInfo(PrimaryServiceType) as Maintenance_Info;

            IMaintenanceBase service = creator.CreateService(PrimaryServiceType, profile) as IMaintenanceBase;
            Request request = creator.CreateObject(PrimaryServiceType + "_Request") as Request;
            Result result = creator.CreateObject(PrimaryServiceType + "_Result") as Result;
            Page.GetInputData(serviceData);
            service.BeginTransaction();

            NamedObjectGroupMaint reqData = creator.CreateServiceData(PrimaryServiceType) as NamedObjectGroupMaint;

            Maintenance data1 = creator.CreateServiceData(PrimaryServiceType) as Maintenance;
            Type type = new WCFObject(serviceData).GetFieldType("ObjectChanges");
            data1.ObjectChanges = WCFObject.CreateObject(type) as NamedObjectGroupChanges;
            (data1.ObjectChanges as NamedObjectGroupChanges).Entries = serviceData.ObjectChanges.Entries;
            (data1.ObjectChanges as NamedObjectGroupChanges).Groups = serviceData.ObjectChanges.Groups;

            var item = Page.PortalContext.DataContract.GetValueByName("SelectedInstanceRef") as NamedObjectRef;
            if (item != null && !string.IsNullOrEmpty(item.Name))
            {
                reqData.ObjectToChange = new NamedObjectRef(item.Name);
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
            (request.Info as Maintenance_Info).ObjectChanges = WCFObject.CreateObject(type) as NamedObjectGroupChanges_Info;
            ((request.Info as Maintenance_Info).ObjectChanges as NamedObjectGroupChanges_Info).ResolvedEntries = new Info(false, true);

            ResultStatus status = service.CommitTransaction(request, out result);

            if (!status.IsSuccess)
                throw new ApplicationException(status.ExceptionData.Description);

            if ((result.Environment as NamedObjectGroupMaint_Environment).ObjectChanges.ResolvedEntries.SelectionValues != null)
                return (result.Environment as NamedObjectGroupMaint_Environment).ObjectChanges.ResolvedEntries.SelectionValues.GetAsDataTable();

            return null;
        }
    }
}
