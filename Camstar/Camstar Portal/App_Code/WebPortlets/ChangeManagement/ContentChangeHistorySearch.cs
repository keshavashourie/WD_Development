// Copyright Siemens 2023  
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.WebPortlets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebControls;
using UIAction = Camstar.WebPortal.Personalization.UIAction;
using Camstar.WebPortal.Personalization;

namespace Camstar.WebPortal.WebPortlets.ChangeManagement
{
    /// <summary>
    /// Summary description for ContentChangeHistorySearch
    /// </summary>
    public class ContentChangeHistorySearch : MatrixWebPart
    {
        protected virtual JQDataGrid ContentChangeHistoryGrid
        {
            get { return Page.FindCamstarControl("ContentChangeHistoryGrid") as JQDataGrid; }
        }

        protected virtual JQDataGrid ContentChangeHistoryGridPdf
        {
            get { return Page.FindCamstarControl("ContentChangeHistoryGridPdf") as JQDataGrid; }
        }

        protected virtual JQDataGrid TxnGrid
        {
            get { return Page.FindCamstarControl("TxnGrid") as JQDataGrid; }
        }

        protected virtual JQDataGrid DetailsGrid
        {
            get { return Page.FindCamstarControl("DetailsGrid") as JQDataGrid; }
        }

        protected virtual CWC.Button SearchButton
        {
            get { return Page.FindCamstarControl("SearchButton") as CWC.Button; }
        }
        protected virtual CWC.Button ClearAllButton
        {
            get { return Page.FindCamstarControl("ClearAllButton") as CWC.Button; }
        }

        protected virtual UIAction ViewDocPdf { get { return Page.ActionDispatcher.PageActions().FirstOrDefault(a => a.Name == "ViewDocument"); } }

        protected virtual NamedObject SelectedPackage
        {
            get { return Page.FindCamstarControl("ChangePackageHeader_ChangePackage") as NamedObject; }
        }

        protected virtual CWC.DateChooser StartTimestamp
        {
            get { return Page.FindCamstarControl("ContentChangeHistoryInquiry_StartTimestamp") as CWC.DateChooser; }
        }
        protected virtual CWC.DateChooser EndTimestamp
        {
            get { return Page.FindCamstarControl("ContentChangeHistoryInquiry_EndTimestamp") as CWC.DateChooser; }
        }

        protected virtual CWC.DropDownList ObjectType
        {
            get { return Page.FindCamstarControl("ContentChangeHistoryInquiry_ObjectType") as CWC.DropDownList; }
        }

        protected virtual CWC.NamedObject LastChangeUser
        {
            get { return Page.FindCamstarControl("ContentChangeHistoryInquiry_LastChangeUser") as CWC.NamedObject; }
        }

        protected virtual CWC.DropDownList InstanceName
        {
            get { return Page.FindCamstarControl("ContentChangeHistoryInquiry_InstanceName") as CWC.DropDownList; }
        }

        protected virtual CWC.RadioButton ShowAllPackageHistory
        {
            get { return Page.FindCamstarControl("ShowAllPackageHistory") as CWC.RadioButton; }
        }

        protected virtual CWC.RadioButton SelectDateRange
        {
            get { return Page.FindCamstarControl("SelectDateRange") as CWC.RadioButton; }
        }
        public ContentChangeHistorySearch()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            StartTimestamp.Enabled = EndTimestamp.Enabled = true;

            SearchButton.Click += SearchButton_Click;
            ClearAllButton.Click += ClearAllButton_Click;
            ShowAllPackageHistory.DataChanged += ShowPackageHistory_DataChanged;
            SelectDateRange.DataChanged += ShowPackageHistory_DataChanged;

            ViewDocPdf.Execution = new UIActionExecution();
            ViewDocPdf.Execution.Executed += ViewDocPdf_Click;
        }

        private void ViewDocPdf_Click(object sender, EventArgs e)
        {
            RequestCPModelingInstanceDtl();

            ContentChangeHistoryGrid.Visible = false;
            ContentChangeHistoryGridPdf.Visible = true;
            TxnGrid.Visible = true;
            DetailsGrid.Visible = true;

            Page.DownloadDocument();
        }
        
        protected virtual void ShowPackageHistory_DataChanged(object sender, EventArgs e)
        {
            StartTimestamp.ReadOnly = EndTimestamp.ReadOnly = !(bool)SelectDateRange.Data;
        }

        protected virtual void SearchButton_Click(object sender, EventArgs e)
        {

            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new ContentChangeHistoryInquiryService(session.CurrentUserProfile);
                var serviceData = new ContentChangeHistoryInquiry()
                {
                    ChangePackage = SelectedPackage.Data as NamedObjectRef,
                    StartTimestamp = StartTimestamp.Data as DateTime?,
                    EndTimestamp = EndTimestamp.Data as DateTime?,
                    ObjectType = ObjectType.Data as string,
                    LastChangeUser = LastChangeUser.Data as NamedObjectRef,
                    InstanceName = InstanceName.Data as string,
                    ShowAllPackageHistory = ShowAllPackageHistory.Data as bool?
                };

                var request = new ContentChangeHistoryInquiry_Request()
                {
                    Info = new ContentChangeHistoryInquiry_Info
                    {
                        Instances = new CPModelingInstanceDtl_Info { RequestValue = true }
                    }
                };

                var result = new ContentChangeHistoryInquiry_Result();
                ResultStatus resultStatus = service.SetInstances(serviceData, request, out result);
                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    var inst = result.Value.Instances;
                    if (inst != null && inst.Any())
                        inst = result.Value.Instances.OrderBy(k => k.LastUpdatedDate != null ? k.LastUpdatedDate.Value : DateTime.MinValue).ToArray();
                    ContentChangeHistoryGrid.Data = inst;

                    if (result.Value.Instances != null && result.Value.Instances.Length != 0)
                        ViewDocPdf.IsDisabled = false;
                    else
                        ViewDocPdf.IsDisabled = true;
                }

                else
                {
                    DisplayMessage(resultStatus);
                    ViewDocPdf.IsDisabled = true;
                }
            }
        }

        protected void RequestCPModelingInstanceDtl()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session != null)
            {
                var service = new ContentChangeHistoryInquiryService(session.CurrentUserProfile);
                var serviceData = new ContentChangeHistoryInquiry()
                {
                    ChangePackage = SelectedPackage.Data as NamedObjectRef,
                    StartTimestamp = StartTimestamp.Data as DateTime?,
                    EndTimestamp = EndTimestamp.Data as DateTime?,
                    ObjectType = ObjectType.Data as string,
                    LastChangeUser = LastChangeUser.Data as NamedObjectRef,
                    InstanceName = InstanceName.Data as string,
                    ShowAllPackageHistory = ShowAllPackageHistory.Data as bool?
                };

                var request = new ContentChangeHistoryInquiry_Request()
                {
                    Info = new ContentChangeHistoryInquiry_Info
                    {
                        Headers = new ModelingAuditTrailHeader_Info { RequestValue = true }
                    }
                };

                var result = new ContentChangeHistoryInquiry_Result();
                ResultStatus resultStatus = service.GetAuditTrailHeaders(serviceData, request, out result);
                if (resultStatus != null && resultStatus.IsSuccess)
                {
                    ContentChangeHistoryGridPdf.Data = GetInstancesData((ContentChangeHistoryGrid.Data as CPModelingInstanceDtl[]), result.Value.Headers);
                }

                else
                {
                    DisplayMessage(resultStatus);
                }

            }
        }

        private CPModelingAuditTrailFields[] GetHeaderFields(ModelingAuditTrailHeader header)
        {
            if (header.AuditTrail != null)
            {
                var headerFields = new List<AuditTrailField>();
                foreach (var auditTrail in header.AuditTrail)
                {
                    if (auditTrail.Fields != null)
                    {
                        foreach (var field in auditTrail.Fields)
                            headerFields.Add(field);
                    }
                }
                if (headerFields.Count != 0)
                    return headerFields.Select(field =>
                    {
                        var detailsField = new CPModelingAuditTrailFields
                        {
                            DisplayLabel = field.DisplayLabel == null ? string.Empty : field.DisplayLabel.Value,
                            FieldName = field.FieldName == null ? string.Empty : field.FieldName.Value,
                            DisplayName = field.DisplayName == null ? string.Empty : field.DisplayName.Value
                        };

                        if (field is AuditTrailTrivialField)
                        {
                            var auditTrailfield = field as AuditTrailTrivialField;
                            detailsField.ObjectDisplayName = auditTrailfield.FieldValues.FirstOrDefault().DisplayName == null ? string.Empty :
                                auditTrailfield.FieldValues.FirstOrDefault().DisplayName.Value;
                            detailsField.OldValue = auditTrailfield.FieldValues.FirstOrDefault().OldValue == null ? string.Empty :
                                auditTrailfield.FieldValues.FirstOrDefault().OldValue.Value;
                            detailsField.NewValue = auditTrailfield.FieldValues.FirstOrDefault().NewValue == null ? string.Empty :
                               auditTrailfield.FieldValues.FirstOrDefault().NewValue.Value;
                            detailsField.Action = auditTrailfield.FieldValues.FirstOrDefault().Action == null ? string.Empty :
                                auditTrailfield.FieldValues.FirstOrDefault().Action.Value;

                        }
                        else if (field is AuditTrailSubentityField)
                        {
                            var auditTrailfield = field as AuditTrailSubentityField;
                            detailsField.ObjectDisplayName = auditTrailfield.FieldValues.FirstOrDefault().ObjectDisplayName == null ? string.Empty :
                                auditTrailfield.FieldValues.FirstOrDefault().ObjectDisplayName.Value;
                            detailsField.Action = auditTrailfield.FieldValues.FirstOrDefault().Action == null ? string.Empty :
                                auditTrailfield.FieldValues.FirstOrDefault().Action.Value;
                        }
                        return detailsField;
                    }
                    ).ToArray();
            }
            return null;
        }

        private CPModelingInstance[] GetInstancesData(CPModelingInstanceDtl[] instances, ModelingAuditTrailHeader[] headers)
        {
            if (instances == null || instances.Length == 0)
                return null;
            var cpInstaces = new List<CPModelingInstance>();

            var filteredInstances = instances.Distinct(new CPModelingInstanceComparer()).OrderBy(instance => instance.ObjectTypeName.Value).ThenBy(instance => instance.ObjectName.Value);

            foreach (var instance in filteredInstances)
            {
                cpInstaces.Add(new CPModelingInstance
                {
                    ObjectTypeName = instance.ObjectTypeName == null ? string.Empty : instance.ObjectTypeName.Value,
                    DisplayedName = instance.DisplayedName == null ? string.Empty : instance.DisplayedName.Value,
                    ObjectName = instance.ObjectName == null ? string.Empty : instance.ObjectName.Value,
                    Description = instance.Description == null ? string.Empty : instance.Description.Value,
                    LastUpdatedDate = instance.LastUpdatedDate == null ? string.Empty : instance.LastUpdatedDate.Value.ToString(),
                    LastUpdatedBy = instance.LastUpdatedBy == null ? string.Empty : instance.LastUpdatedBy.ToString(),
                    ObjectInstanceId = instance.ObjectInstanceId == null ? string.Empty : instance.ObjectInstanceId.Value,
                    Revision = instance.Revision == null ? string.Empty : instance.Revision.Value,
                    ObjectTypeValue = instance.ObjectTypeValue == null ? string.Empty : instance.ObjectTypeValue.Value,
                    Headers = (headers == null || headers.Where(header => header.ObjectInstanceId == instance.ObjectInstanceId).Count() == 0)
                        ? null : headers.Where(header => header.ObjectInstanceId == instance.ObjectInstanceId)
                        .Select(header => new CPModelingAuditTrailHeaders
                        {
                            HeaderId = header.Self.ID,
                            ObjectInstanceId = header.ObjectInstanceId == null ? string.Empty : header.ObjectInstanceId.Value,
                            TxnDateGMT = header.TxnDateGMT == null ? string.Empty : header.TxnDateGMT.Value.ToString(),
                            User = header.User == null ? string.Empty : header.User.ToString(),
                            ExecuteAction = header.ExecuteAction == null ? string.Empty : header.ExecuteAction.Value,
                            Comments = header.Comments == null ? string.Empty : header.Comments.Value,
                            Fields = GetHeaderFields(header)
                        }).ToArray()
                });
            }
            return cpInstaces.ToArray();
        }

        protected virtual void ClearAllButton_Click(object sender, EventArgs e)
        {
            var selectedPackage = SelectedPackage.Data;

            Page.ClearValues();

            if (selectedPackage != null)
                SelectedPackage.Data = selectedPackage;
            ViewDocPdf.IsDisabled = true;
            ShowAllPackageHistory.Data = true;
        }
    }

    class CPModelingInstance
    {
        public string ObjectTypeName { get; set; }
        public string DisplayedName { get; set; }
        public string ObjectName { get; set; }
        public string Description { get; set; }
        public string LastUpdatedDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public string ObjectInstanceId { get; set; }
        public string Revision { get; set; }
        public string ObjectTypeValue { get; set; }
        public CPModelingAuditTrailHeaders[] Headers { get; set; }
    }

    class CPModelingAuditTrailHeaders
    {
        public string HeaderId { get; set; }
        public string ObjectInstanceId { get; set; }
        public string TxnDateGMT { get; set; }
        public string User { get; set; }
        public string ExecuteAction { get; set; }
        public string PackageName { get; set; }
        public string Comments { get; set; }
        public CPModelingAuditTrailFields[] Fields { get; set; }
    }

    class CPModelingAuditTrailFields
    {
        public string DisplayLabel { get; set; }
        public string ObjectDisplayName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Action { get; set; }
        public string FieldName { get; set; }
        public string ObjectInstanceId { get; set; }
        public string DisplayName { get; set; }
    }

    class CPModelingInstanceComparer : IEqualityComparer<CPModelingInstanceDtl>
    {
        public bool Equals(CPModelingInstanceDtl x, CPModelingInstanceDtl y)
        {
            return x.DisplayedName.Value == y.DisplayedName.Value;
        }

        public int GetHashCode(CPModelingInstanceDtl instance)
        {
            if (Object.ReferenceEquals(instance, null)) return 0;
            
            int hashDisplayedName = instance.DisplayedName.Value == null ? 0 : instance.DisplayedName.Value.GetHashCode();
            
            return hashDisplayedName;
        }
    }
}
