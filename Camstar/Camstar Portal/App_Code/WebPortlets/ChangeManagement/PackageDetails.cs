// Copyright Siemens 2023  
using System;
using System.Linq;
using System.Data;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Collections.Generic;

namespace Camstar.WebPortal.WebPortlets.ChangeManagement
{
    public class PackageDetailWP : MatrixWebPart
    {
        protected virtual JQDataGrid DeploymentDetails { get { return Page.FindCamstarControl("DeploymentDetails") as JQDataGrid; } }
        protected virtual JQDataGrid ApproversGrid { get { return Page.FindCamstarControl("ApproversGrid") as JQDataGrid; } }
        protected virtual CheckBox HideDependenciesChk { get { return Page.FindCamstarControl("HideDependenciesChk") as CheckBox; } }
        protected virtual NamedObject SelectedPackageNamedObject { get { return Page.FindCamstarControl("HiddenSelectedPackage_ChangePackage") as NamedObject; } }
        protected virtual RadioButton ViewCurrent { get { return Page.FindCamstarControl("ViewCurrent") as RadioButton; } }
        protected virtual RadioButton ViewHistory { get { return Page.FindCamstarControl("ViewHistory") as RadioButton; } }
        protected virtual Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity ApprovalSheet { get { return Page.FindCamstarControl("ApprovalCycleInquiry_ApprovalSheet") as Camstar.WebPortal.FormsFramework.WebControls.NamedSubentity; } }
        protected virtual DropDownList ApprovalCycle { get { return Page.FindCamstarControl("ApprovalCycle") as DropDownList; } }
        protected virtual JQDataGrid ObjectTypeGrid { get { return Page.FindCamstarControl("ObjectTypeGrid") as JQDataGrid; } }
        protected virtual JQDataGrid CollaboratorDetails { get { return Page.FindCamstarControl("CollaboratorEntriesGrid") as JQDataGrid; } }
        protected virtual TextBox InstructionAllApprovers { get { return Page.FindCamstarControl("InstructionAllApprovers") as TextBox; } }

        protected virtual JQDataGrid IncludedInstances { get { return Page.FindCamstarControl("IncludedInstances") as JQDataGrid; } }

        protected virtual CheckBox IsImportedPackageChk { get { return Page.FindCamstarControl("IsImportedPackage_Check") as CheckBox; } }

        protected virtual JQDataGrid PreReqChangePkgsGrid { get { return Page.FindCamstarControl("PreReqChangePkgsGrid") as JQDataGrid; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                SelectedPackageNamedObject.LoadOrClearDependentValues();
                LoadDetailsSummary();
                ViewCurrent.RadioControl.Checked = true;
            }
            HideDependenciesChk.DataChanged += (sender, args) => ShowHideDependencies();
            ApprovalCycle.DataChanged += ApprovalCycle_DataChanged;
            SelectedPackageNamedObject.DataChanged += SelectedPackageNamedObject_DataChanged;
            Page.OnRequestFormValues += Page_OnRequestFormValues;
            ViewCurrent.RadioControl.CheckedChanged += ViewCurrent_CheckedChanged;

            IncludedInstances.BoundContext.SnapCompleted += IncludedInstances_SnapCompleted;
        }

        protected virtual void IncludedInstances_SnapCompleted(DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                row["IsImported"] = Convert.ToInt32(IsImportedPackageChk.IsChecked);
                row["IsModelingObjectExist"] = 0;
                if (new MaintCDOCache().MaintCdoData.Any(x => x.CDODefID == row["CDOTypeID"] as string))
                    row["IsModelingObjectExist"] = 1;
            }
        }

        protected virtual void ApprovalCycle_DataChanged(object sender, EventArgs e)
        {

        }

        protected virtual void ViewCurrent_CheckedChanged(object sender, EventArgs e)
        {
            ApprovalCycle.ClearData();
            ApprovalCycle.TextEditControl.Text = string.Empty;
        }

        protected virtual void Page_OnRequestFormValues(object sender, FormsFramework.FormProcessingEventArgs e)
        {
            var data = e.Data as GetChangePackageDetails;
            if (data != null)
                data.ChangePackageHeader = null; // this helps to request change pkg header info.
        }

        protected virtual void SelectedPackageNamedObject_DataChanged(object sender, EventArgs e)
        {
            LoadDetailsSummary();
        }

        public virtual void ShowHideDependencies()
        {
            if (HideDependenciesChk.IsChecked)
            {
                var data = ObjectTypeGrid.Data as ObjectTypeItem[];
                if (data != null)
                {
                    var newdata =
                        from d in data
                        let inst = d.Instances.Where(n => !n.IsRef)
                        where inst.Any()
                        select new ObjectTypeItem()
                        {
                            Name = d.Name,
                            DisplayName = d.DisplayName,
                            CDOID = d.CDOID,
                            Instances = inst.ToArray()
                        };
                    if ((ObjectTypeGrid.Data as ObjectTypeItem[]).Count() != newdata.ToArray().Count() || Page.PortalContext.LocalSession["DataWithDependencies"] == null)
                    {
                        Page.PortalContext.LocalSession["DataWithDependencies"] = ObjectTypeGrid.Data;
                    }
                    ObjectTypeGrid.Data = newdata.ToArray();
                }
            }
            else
            {
                ObjectTypeGrid.Data = Page.PortalContext.LocalSession["DataWithDependencies"];
            }
        }

        protected virtual void LoadDetailsSummary()
        {
            var selectedPackage = SelectedPackageNamedObject.Data as NamedObjectRef;
            if (selectedPackage != null && !selectedPackage.IsEmpty)
            {
                var service = new GetChangePackageDetailsService(FormsFramework.Utilities.FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                var request = new GetChangePackageDetails_Request();
                request.Info = new GetChangePackageDetails_Info
                {
                    PackageDetails = new ChangePackageDetails_Info
                    {
                        TargetSystems = new TargetSystemDetail_Info
                        {
                            TargetSystem = new TargetSystem_Info { Name = new Info(true), },
                            TargetSystemName = new Info(true)
                        },
                        Instances = new CPModelingInstanceDtl_Info
                        {
                            ObjectInstanceId = new Info(true),
                            ObjectType = new Info(true),
                            ObjectTypeValue = new Info(true),
                            IsReference = new Info(true),
                            Description = new Info(true),
                            ObjectName = new Info(true),
                            LastUpdatedBy = new Info(true),
                            LastUpdatedDate = new Info(true),
                            Revision = new Info(true),
                            DisplayedName = new Info(true),
                            IsBadReference = new Info(true),
                            ErrorMessage = new Info(true)
                        },
                        PreReqChangePkgs = new PackageInquiryDetail_Info
                        {
                            PackageName = new Info(true),
                            Description = new Info(true),
                            Step = new Info(true),
                            LastStateUpdate = new Info(true)
                        }
                    },
                    ApprovalCycleInquiryDetails = new ApprovalCycleInquiryDetails_Info() { RequestValue = true },
                    ApprovalSheet = new Info(true, false),
                    GeneralInstructions = new Info(true),
                    CollaboratorDetails = new CollaboratorDetails_Info()
                    {
                        CollaboratorEntries = new CollaboratorEntryDetails_Info
                        {
                            SheetLevel = new Info(true),
                            Collaborator = new Info(true),
                            Status = new Info(true),
                            AssignedDateGMT = new Info(true),
                            DueDateGMT = new Info(true),
                            CompletionDateGMT = new Info(true),
                            CollaboratorComments = new Info(true),
                            SpecialInstructions = new Info(true)
                        }
                    }
                };

                var data = new GetChangePackageDetails { ChangePackage = selectedPackage };

                GetChangePackageDetails_Result result;
                var res = service.GetPackageDetails(data, request, out result);
                if (res.IsSuccess)
                {
                    if (result.Value.PackageDetails != null)
                    {
                        DisplayValues(result.Value);
                        InstructionAllApprovers.Data = result.Value.GeneralInstructions;
                        ApprovalSheet.Data = result.Value.ApprovalSheet;
                        ApproversGrid.Data = result.Value.ApprovalCycleInquiryDetails;
                        if (result.Value.CollaboratorDetails != null && result.Value.CollaboratorDetails.CollaboratorEntries != null)
                            CollaboratorDetails.Data = result.Value.CollaboratorDetails.CollaboratorEntries;

                        var instances = result.Value.PackageDetails.Instances;
                        if (instances != null)
                        {
                            var otList =
                                from inst in instances
                                group inst by inst.ObjectTypeValue.Value
                                into g
                                orderby g.Key
                                select
                                    new ObjectTypeItem()
                                    {
                                        Name = g.Key,
                                        DisplayName = g.Key,
                                        Instances =
                                            (from n in instances
                                             where n.ObjectTypeValue.Value == g.Key
                                             select new SelectedInstanceItem
                                             {
                                                 CDOTypeID = n.ObjectType.Value.ToString(),
                                                 InstanceID = n.ObjectInstanceId.Value,
                                                 Name = n.ObjectName == null ? null : n.ObjectName.Value,
                                                 DisplayedName = n.DisplayedName == null ? (n.IsBadReference.Value && n.ObjectName != null ? n.ObjectName.Value : null) : n.DisplayedName.Value,
                                                 IsRef = n.IsReference.Value,
                                                 Description = n.Description == null ? null : n.Description.Value,
                                                 LastEditedBy = n.LastUpdatedBy == null ? null : n.LastUpdatedBy.Name,
                                                 LastEditedTime = n.LastUpdatedDate == null ? (DateTime?)null : n.LastUpdatedDate.Value
                                             }
                                            ).OrderBy(f => f.Name).ToArray()
                                    };
                            ObjectTypeGrid.Data = otList.OrderBy(v => v.DisplayName).ToArray();
                        }
                        DeploymentDetails.Data = result.Value.PackageDetails.TargetSystems;
                        PreReqChangePkgsGrid.Data = result.Value.PackageDetails.PreReqChangePkgs;
                    }
                }
            }
            else
                Page.ClearValues();

            var preReqs = (PreReqChangePkgsGrid.Data as PackageInquiryDetail[]) ?? new PackageInquiryDetail[0];
            if (preReqs.Length == 0)
            {
                var svc = new ActivationInquiryService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                var pkgName = SelectedPackageNamedObject.Data.ToString();
                var svcDet = new ActivationInquiry()
                {
                    PackageName = pkgName
                };
                var req = new ActivationInquiry_Request
                {
                    Info = new ActivationInquiry_Info
                    {
                        PackageDetails = new ActivationInquiryDetail_Info
                        {
                            ChangePackage = new ChangePackage_Info
                            {
                                PreReqChangePkgs = new Info(true)
                            }
                        }
                    }
                };
                var res = new ActivationInquiry_Result();
                var result = svc.GetPackages(svcDet, req, out res);
                if (result.IsSuccess)
                {
                    if (res.Value != null && res.Value.PackageDetails != null && res.Value.PackageDetails.Length > 0)
                    {
                        var pkg = res.Value.PackageDetails[0];
                        var newList = new List<PackageInquiryDetail>();
                        if (pkg.ChangePackage != null && pkg.ChangePackage.PreReqChangePkgs != null)
                        {
                            foreach (var det in pkg.ChangePackage.PreReqChangePkgs)
                            {
                                var pkgInqDet = new PackageInquiryDetail
                                {
                                    PackageName = det
                                };
                                newList.Add(pkgInqDet);
                            }
                            PreReqChangePkgsGrid.Data = newList.ToArray();
                            PreReqChangePkgsGrid.DataBind();
                        }
                    }
                }
            }
        }

        public override void RequestValues(Info serviceInfo, Service serviceData)
        {
            base.RequestValues(serviceInfo, serviceData);

            if (!ApprovalCycle.IsEmpty)
                (serviceData as ApprovalCycleInquiry).ApprovalCycleGMT = new Primitive<DateTime>((DateTime)ApprovalCycle.Data);
            (serviceData as ApprovalCycleInquiry).ApprovalSheet = ApprovalSheet.Data as NamedSubentityRef;
            (serviceInfo as ApprovalCycleInquiry_Info).ApprovalCycleInquiryDetails = new ApprovalCycleInquiryDetails_Info() { RequestValue = true };
            (serviceInfo as ApprovalCycleInquiry_Info).GeneralInstructions = new Info(true);
        }
    }
}
