// Copyright Siemens 2023  
using System.Collections.Generic;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RevisionedObject = Camstar.WebPortal.FormsFramework.WebControls.RevisionedObject;
using System.Data;

namespace Camstar.WebPortal.WebPortlets.ChangeManagement
{
    public class StartChangePackage : MatrixWebPart
    {

        #region Controls

        protected virtual JQDataGrid TargetsGrid { get { return Page.FindCamstarControl("TargetsGrid") as JQDataGrid; } }
        protected virtual TextBox ChangePackageName { get { return Page.FindCamstarControl("ChangePackageName") as TextBox; } }
        protected virtual NamedObject PackageCreationTemplate { get { return Page.FindCamstarControl("PackageCreationTemplate") as NamedObject; } }
        protected virtual NamedObject PackageOwner { get { return Page.FindCamstarControl("PackageOwner") as NamedObject; } }
        protected virtual NamedObject CreationReason { get { return Page.FindCamstarControl("CreationReason") as NamedObject; } }
        protected virtual NamedObject PkgPriority { get { return Page.FindCamstarControl("PkgPriority") as NamedObject; } }
        protected virtual NamedObject PackageType { get { return Page.FindCamstarControl("StartChangePkg_PackageType") as NamedObject; } }
        protected virtual NamedObject OwnerRole { get { return Page.FindCamstarControl("OwnerRole") as NamedObject; } }
        protected virtual TextBox EcoEcn { get { return Page.FindCamstarControl("EcoEcn") as TextBox; } }
        protected virtual TextBox DescriptionField { get { return Page.FindCamstarControl("DescriptionField") as TextBox; } }
        protected virtual RevisionedObject StartChangePkgWorkflow { get { return Page.FindCamstarControl("StartChangePkg_Workflow") as RevisionedObject; } }
        protected virtual TextBox PackageDescription { get { return Page.FindCamstarControl("PackageDescription") as TextBox; } }
        protected virtual JQDataGrid PreReqChangePkgsGrid { get { return Page.FindCamstarControl("PreReqChangePkgsGrid") as JQDataGrid; } }

        #endregion

        #region Protected Override Methods

        protected override void OnLoad(EventArgs e)
        {
            var popupData = Page.DataContract.GetValueByName<PopupData>("__Page_PopupData");
            if (popupData != null && popupData.ReturnedData != null)
            {
                var preReqs = popupData.ReturnedData as PackageInquiryDetail[] ?? new PackageInquiryDetail[0];
                if (preReqChangePkgs != null)
                {
                    var newList = preReqChangePkgs.ToList();
                    foreach (var returnedData in preReqs)
                    {
                        var doesExist = preReqChangePkgs.ToList().Exists(i => i.PackageName.Value == returnedData.PackageName.Value);
                        if (!doesExist)
                        {
                            newList.Add(returnedData);
                        }
                    }
                    preReqChangePkgs = newList.OrderBy(x => x.PackageName.Value).ToArray();
                }
                else
                {
                    preReqChangePkgs = preReqs;
                }
                Page.DataContract.SetValueByName("__Page_PopupData", null);
            }
            var targets = RequestTargetSystems();
            if (TargetsGrid != null)
                TargetsGrid.Data = targets;
            if (Page.PortalContext.LocalSession["DifferentVersionsWarning"] != null)
                Page.DisplayWarning(Page.PortalContext.LocalSession["DifferentVersionsWarning"] as string);
            TargetsGrid.GridContext.PostBackOnSelect = true;
            TargetsGrid.GridContext.RowSelected += (sender, args) =>
            {
                if (!args.State.IsRowSelected)
                    return args.Response;
                var selectedTarget = TargetsGrid.GridContext.GetItem(TargetsGrid.SelectedRowID) as TargetSystem;
                StringBuilder warning = new StringBuilder(200);
                var sourseSystemVersion = ViewState["SourceSystemVersion"] as Primitive<string>;
                if (selectedTarget != null && selectedTarget.CamstarVersion != null
                    && sourseSystemVersion != null
                    && !selectedTarget.CamstarVersion.Value.Equals(sourseSystemVersion.Value)
                    && !selectedTarget.IsNotACamstarServer.Value)
                {
                    string msg = FrameworkManagerUtil.GetLabelValue("Lbl_DifferentCamstarVersion");
                    warning.Append(string.Format(msg, ViewState["SourceSystemVersion"], selectedTarget.DisplayName, selectedTarget.CamstarVersion));
                }
                var sourceMdbVersion = ViewState["SourceMdbVersion"] as Primitive<string>;
                if (selectedTarget != null && selectedTarget.MDBVersion != null
                    && sourceMdbVersion != null
                    && !selectedTarget.MDBVersion.Value.Equals(sourceMdbVersion.Value)
                    && !selectedTarget.IsNotACamstarServer.Value)
                {
                    string msg = FrameworkManagerUtil.GetLabelValue("Lbl_DifferentMDBVersion");
                    warning.Append(string.Format(msg, ViewState["SourceMdbVersion"], selectedTarget.DisplayName, selectedTarget.MDBVersion));
                }

                Page.PortalContext.LocalSession["DifferentVersionsWarning"] = warning.Length > 0 ? warning.ToString() : null;
                return args.Response;
            };
            PackageCreationTemplate.DataChanged += PackageCreationTemplate_DataChanged;
            PreReqChangePkgsGrid.GridContext.RowDeleting += GridContext_RowDeleting;
            PreReqChangePkgsGrid.PreRender += PreReqChangePkgsGrid_PreRender;
            base.OnLoad(e);
        }

        #endregion

        #region Protected Virtual Methods

        protected virtual ResponseData GridContext_RowDeleting(object sender, JQGridEventArgs args)
        {
            var deletedRowIDs = args.Context.SelectedRowIDs;
            if (deletedRowIDs != null)
            {
                var newList = preReqChangePkgs.ToList();
                foreach (var deletedRow in deletedRowIDs)
                {
                    var doesExist = newList.Exists(i => i.PackageName.Value == deletedRow);
                    if (doesExist)
                    {
                        var pkg = PreReqChangePkgsGrid.GridContext.GetItem(deletedRow) as PackageInquiryDetail;
                        newList.Remove(pkg);
                    }
                }
                preReqChangePkgs = newList.ToArray();
            }
            return null;
        }

        protected virtual void PreReqChangePkgsGrid_PreRender(object sender, EventArgs e)
        {
            PreReqChangePkgsGrid.Data = preReqChangePkgs;
            PreReqChangePkgsGrid.DataBind();
        }

        protected virtual void PackageCreationTemplate_DataChanged(object sender, EventArgs e)
        {
            var result = RequestPkgCreationTemplate() as StartChangePkg_Result;
            IEnumerable<TargetSystemDetail> selectedTargets = new List<TargetSystemDetail>();
            if (result != null && result.Value != null)
            {
                if (TargetsGrid.SelectedRowIDs != null)
                    TargetsGrid.GridContext.SelectedRowIDs = null;
                PackageOwner.Data = result.Value.Owner;
                CreationReason.Data = result.Value.Reason;
                PackageType.Data = result.Value.PackageType;
                PkgPriority.Data = result.Value.Priority;
                OwnerRole.Data = result.Value.OwnerRole;
                EcoEcn.Data = result.Value.ECOECN;
                PackageDescription.Data = result.Value.Comments;
                StartChangePkgWorkflow.Data = result.Value.Workflow;
                DescriptionField.Data = result.Value.WorkflowDescription;
                if (result.Value.ServiceDetail != null)
                    selectedTargets = result.Value.ServiceDetail.
                        TargetSystems.Where(s => !s.TargetSystem.IsSourceSystem.Value).Where(s => s.IsTargetSelected == true);
            }
            var dataGrid = (TargetsGrid.Data as TargetSystem[]);
            var warning = String.Empty;
            if (dataGrid != null)
            {
                foreach (var target in dataGrid)
                {
                    if (selectedTargets.Any(t => t.TargetSystem.Equals(target)))
                    {
                        TargetsGrid.GridContext.SelectRow(target.Name.ToString(), true);
                        TargetsGrid.GridContext.OnRowSelected(new ClientGridState
                        {
                            ContextID = TargetsGrid.GridContext.ContextID,
                            CallStackKey = TargetsGrid.GridContext.CallStackKey,
                            RowID = null,
                            IsRowSelected = TargetsGrid.IsRowSelected
                        });
                    }

                    if (Page.PortalContext.LocalSession["DifferentVersionsWarning"] != null)
                        warning = Page.PortalContext.LocalSession["DifferentVersionsWarning"] as string;
                }
            }
            if (!string.IsNullOrEmpty(warning))
                Page.DisplayWarning(warning);

        }

        #endregion

        #region Public Override Methods
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "Reset":
                        {
                            if (TargetsGrid.BoundContext.SelectedRowIDs != null)
                                TargetsGrid.BoundContext.SelectedRowIDs.Clear();
                            TargetsGrid.BoundContext.SelectedRowID = null;
                            Page.PortalContext.LocalSession["DifferentVersionsWarning"] = null;
                            break;
                        }
                    case "Save":
                        {
                            e.Result = SubmitSave();
                            break;
                        }
                    case "SaveUpdate":
                        {
                            e.Result = SubmitSave();
                            if (e.Result.IsSuccess)
                                RedirectToUpdatePage();
                            break;
                        }
                }
            }
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            var data = serviceData as StartChangePkg;
            if (TargetsGrid != null)
            {
                var selectedTargets = TargetsGrid.GridContext.GetSelectedItems(false);
                if (selectedTargets == null || selectedTargets.Length == 0)
                    return;
                data.ServiceDetail = new StartChangePkgDetail { TargetSystems = new TargetSystemDetail[selectedTargets.Length], FieldAction = Camstar.WCF.ObjectStack.Action.Create, CDOTypeName = "StartChangePkgDetail" };
                for (var i = 0; i < selectedTargets.Length; i++)
                    data.ServiceDetail.TargetSystems[i] = new TargetSystemDetail { IsTargetSelected = true, TargetSystem = new TargetSystem { Self = ((TargetSystem)selectedTargets[i]).Self } };
            }
            if (preReqChangePkgs != null)
            {
                if (data.ServiceDetail == null)
                {
                    data.ServiceDetail = new StartChangePkgDetail();
                }

                if (preReqChangePkgs != null && preReqChangePkgs.Length > 0)
                {
                    if (data.PreReqChangePkgs == null)
                    {
                        data.PreReqChangePkgs = new Primitive<string>[preReqChangePkgs.Length];
                    }
                    for (var i = 0; i < preReqChangePkgs.Length; i++)
                    {
                        NamedObjectRef pkg = new NamedObjectRef(preReqChangePkgs[i].PackageName.Value);
                        data.PreReqChangePkgs[i] = pkg.Name;
                    }
                }
            }
        }
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (TargetsGrid.GridContext.SelectedRowIDs != null)
                TargetsGrid.GridContext.SelectedRowIDs.Clear();

            if (status.IsSuccess)
            {
                Page.DataContract.SetValueByName("__Page_PopupData", null);
                preReqChangePkgs = null;
            }
            Page.PortalContext.LocalSession["DifferentVersionsWarning"] = null;
        }

        #endregion        

        #region Public Functions
        public virtual Result RequestPkgCreationTemplate()
        {
            var serviceData = new StartChangePkg();
            Page.GetInputData(serviceData);
            var service = new StartChangePkgService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var request = new StartChangePkg_Request
            {
                Info = new StartChangePkg_Info()
                {
                    Owner = new Info(true),
                    OwnerRole = new Info(true),
                    Priority = new Info(true),
                    PackageType = new Info(true),
                    Reason = new Info(true),
                    ECOECN = new Info(true),
                    Comments = new Info(true),
                    Workflow = new Info(true),
                    WorkflowDescription = new Info(true),
                    ServiceDetail = new StartChangePkgDetail_Info { TargetSystems = new TargetSystemDetail_Info { RequestValue = true } }
                }
            };
            StartChangePkg_Result result;
            var res = service.LoadTemplate(serviceData, request, out result);
            if (!res.IsSuccess)
            {
                DisplayMessage(res);
                return null;
            }
            return result;
        }

        #endregion        

        #region Private Functions

        protected virtual ResultStatus SubmitSave()
        {
            if (ChangePackageName.Data != null)
            {
                _packageName = ChangePackageName.Data.ToString();
                const string expression = @"^[ a-zA-Z0-9_.-]*$"; // @"([A-Za-z0-9\._-])+"; //expression allows only numbers, letters, underscores, dashes, and periods.
                var pattern = new Regex(expression);
                //Regex check
                if (!pattern.IsMatch(_packageName))
                {
                    string msg = FrameworkManagerUtil.GetLabelValue("Lbl_PackageNameInvalid");
                    var result = new ResultStatus(msg, false);
                    return result;
                }
            }
            return Page.Service.Submit(PrimaryServiceType, true);
        }

        protected virtual void RedirectToUpdatePage()
        {
            var packageNameRef = new NamedObjectRef() { Name = _packageName };
            var redirectAction = new PageRedirectAction() { PageName = "UpdateChangePkg_VP" };
            redirectAction.DataContractMap = new UIComponentDataContractMap
            {
                Links = new[]
                {
                    new UIComponentDataContractLink
                    {
                        SourceMember = "ChangePackage",
                        TargetMember = "ChangePackage"
                    }
                }
            };
            Page.DataContract.SetValueByName("ChangePackage", packageNameRef);
            Page.ActionDispatcher.ExecuteAction(redirectAction);

            Page.PortalContext.LocalSession["DifferentVersionsWarning"] = null;
        }

        protected virtual TargetSystem[] RequestTargetSystems()
        {
            var service = new StartChangePkgService(
                    Camstar.WebPortal.FormsFramework.Utilities.FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var cdo = new StartChangePkg();
            var request = new StartChangePkg_Request
            {
                Info = new StartChangePkg_Info()
                {
                    ServiceDetail = new StartChangePkgDetail_Info { TargetSystems = new TargetSystemDetail_Info { RequestValue = true } }
                }
            };
            StartChangePkg_Result result;
            ResultStatus res = service.Load(cdo, request, out result);
            if (res.IsSuccess && result.Value.ServiceDetail.TargetSystems != null)
            {
                var sourse = result.Value.ServiceDetail.TargetSystems.FirstOrDefault(s => s.TargetSystem.IsSourceSystem.Value);
                if (sourse != null)
                {
                    ViewState["SourceSystemVersion"] = sourse.TargetSystem.CamstarVersion;
                    ViewState["SourceMdbVersion"] = sourse.TargetSystem.MDBVersion;
                }
                return result.Value.ServiceDetail.TargetSystems.Where(s => !s.TargetSystem.IsSourceSystem.Value).Select(s => s.TargetSystem).ToArray();
            }
            return new TargetSystem[0];
        }

        #endregion

        #region Constants
        private string _preReqChangePkgsString = "PreReqChangePkgs";
        #endregion       

        #region Private Member Variables

        private string _packageName;

        protected virtual PackageInquiryDetail[] preReqChangePkgs
        {
            get
            {
                return Page.DataContract.GetValueByName<PackageInquiryDetail[]>(_preReqChangePkgsString);
            }
            set
            {
                Page.DataContract.SetValueByName(_preReqChangePkgsString, value);
            }
        }

        #endregion
    }
}
