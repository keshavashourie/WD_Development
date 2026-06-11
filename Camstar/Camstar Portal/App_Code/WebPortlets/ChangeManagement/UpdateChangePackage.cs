// Copyright Siemens 2023  
using System.Text;
using System.Web.UI.WebControls.WebParts;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.WebPortlets;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Label = Camstar.WCF.ObjectStack.Label;

namespace Camstar.WebPortal.WebPortlets.ChangeManagement
{
    public class UpdateChangePackage : MatrixWebPart
    {
        #region Controls

        protected virtual JQDataGrid TargetsGrid { get { return Page.FindCamstarControl("TargetsGrid") as JQDataGrid; } }
        protected virtual NamedObject SelectedPackage { get { return Page.FindCamstarControl("SelectedPackage") as NamedObject; } }
        protected virtual JQTabContainer UpdateChangePkgTabContainer { get { return Page.FindCamstarControl("UpdateChangePkg_TabContainer") as JQTabContainer; } }
        protected virtual CheckBox HasSpecApprovals { get { return Page.FindCamstarControl("HasSpecApprovals") as CheckBox; } }
        protected virtual JQDataGrid CollaboratorsGrid { get { return Page.FindCamstarControl("CollaboratorEntriesGrid") as JQDataGrid; } }
        protected virtual RadioButton NotUseContentCollaborators { get { return Page.FindCamstarControl("FalseControl") as RadioButton; } }
        protected virtual RadioButton UseContentCollaborators { get { return Page.FindCamstarControl("TrueControl") as RadioButton; } }
        protected virtual Button SwitchToOwnerOnly { get { return Page.FindCamstarControl("SwitchToOwnerOnly") as Button; } }
        protected virtual JQDataGrid PreReqChangePkgsGrid { get { return Page.FindCamstarControl("PreReqChangePkgsGrid") as JQDataGrid; } }

        #endregion

        #region Protected Override Functions

        protected override void OnLoad(EventArgs e)
        {
            var popupData = Page.DataContract.GetValueByName<PopupData>("__Page_PopupData");
            if (popupData != null && popupData.Caller.ClientID != CollaboratorsGrid.ClientID)
            {
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
                else
                {
                    preReqChangePkgs = PreReqChangePkgsGrid.Data as PackageInquiryDetail[];
                }
            }
            SelectedPackage.DataChanged += (sender, args) =>
            {
                if (TargetsGrid.GridContext.SelectedRowIDs != null)
                    TargetsGrid.GridContext.SelectedRowIDs.Clear();
                var targets = RequestTargetSystems();
                TargetsGrid.Data = targets.Select(t => t.TargetSystem).ToArray();
                foreach (var detail in targets)
                {
                    if (!detail.IsTargetSelected.Value)
                        continue;
                    TargetsGrid.GridContext.SelectRow(detail.TargetSystem.Name.Value, true);
                }
            };
            NotUseContentCollaborators.DataChanged += (senser, args) =>
            {
                if (!(bool)NotUseContentCollaborators.Data) return;
                var collaboratorPage = Page.FindCamstarControl("Collaborators") as UpdateChangePackageCollaborators;
                if (collaboratorPage != null)
                {
                    foreach (var control in collaboratorPage.CamstarControls)
                    {
                        if (control.Data != null)
                            control.ClearData();
                    }
                }
            };
            SwitchToOwnerOnly.Click += (senser, args) =>
            {
                NotUseContentCollaborators.Data = true;
                UseContentCollaborators.Data = false;
            };

            if (Page.PortalContext.LocalSession["DifferentVersionsWarning"] != null)
                Page.DisplayWarning(Page.PortalContext.LocalSession["DifferentVersionsWarning"] as string);

            TargetsGrid.GridContext.PostBackOnSelect = true;
            TargetsGrid.GridContext.RowSelected += (sender, args) =>
            {
                if (!args.State.IsRowSelected)
                {
                    Page.PortalContext.LocalSession["DifferentVersionsWarning"] = null;
                    return args.Response;
                }
                ((FormsFramework.Utilities.DirectUpdateData)args.Response).PropertyValue = ((FormsFramework.Utilities.DirectUpdateData)args.Response).PropertyValue.Replace("PostBackRequested:false", "PostBackRequested:true");
                var selectedTarget = TargetsGrid.GridContext.GetItem(TargetsGrid.SelectedRowID) as TargetSystem;
                StringBuilder warning = new StringBuilder(200);
                var sourseSystemVersion = ViewState["SourceSystemVersion"] as Primitive<string>;
                if (selectedTarget != null && selectedTarget.CamstarVersion != null
                    && sourseSystemVersion != null
                    && !selectedTarget.CamstarVersion.Value.Equals(sourseSystemVersion.Value)
                    && !selectedTarget.IsNotACamstarServer.Value)
                    warning.Append(string.Format("Source is on 'Camstar version {0}' and {1} is on 'Camstar version {2}' ", ViewState["SourceSystemVersion"], selectedTarget.DisplayName, selectedTarget.CamstarVersion));
                var sourceMdbVersion = ViewState["SourceMdbVersion"] as Primitive<string>;
                if (selectedTarget != null && selectedTarget.MDBVersion != null
                    && sourceMdbVersion != null
                    && !selectedTarget.MDBVersion.Value.Equals(sourceMdbVersion.Value)
                    && !selectedTarget.IsNotACamstarServer.Value)
                    warning.Append(string.Format("Source is on 'MDB version {0}' and {1} is on 'MDB version {2}'", ViewState["SourceMdbVersion"], selectedTarget.DisplayName, selectedTarget.MDBVersion));
                Page.PortalContext.LocalSession["DifferentVersionsWarning"] = warning.Length > 0 ? warning.ToString() : null;
                return args.Response;
            };
            PreReqChangePkgsGrid.PreRender += PreReqChangePkgsGrid_PreRender;
            PreReqChangePkgsGrid.GridContext.RowDeleting += GridContext_RowDeleting;
            base.OnLoad(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            UpdateChangePkgTabContainer.Tabs[1].Visible = (bool)HasSpecApprovals.Data;
            base.OnPreRender(e);
            var gridData = CollaboratorsGrid.Data as Array;
            if (gridData != null && gridData.Length != 0)
            {
                string yesFunction = string.Format(@"OwnerOnly('{0}','{1}')", NotUseContentCollaborators.ClientID, SwitchToOwnerOnly.ButtonControl.ClientID);
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                NotUseContentCollaborators.Attributes["onclick"] = ControlUtil.MakeConfirmation(
                        new Confirmation { OK_LabelName = "Web_Yes", Cancel_LabelName = "Web_No", Message_LabelName = "Lbl_CollaboratorDataWillBeDeleted", Title_LabelName = "StatusMessage_Warning" },
                    labelCache,
                   yesFunction);
                CamstarWebControl.SetRenderToClient(NotUseContentCollaborators);
            }
            else
            {
                NotUseContentCollaborators.Attributes.Remove("onclick");
            }
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
            if (!Page.IsPostBack)
            {
                preReqChangePkgs = (PreReqChangePkgsGrid.Data ?? new PackageInquiryDetail[0]) as PackageInquiryDetail[];
                return;
            }
            PreReqChangePkgsGrid.Data = preReqChangePkgs;
            PreReqChangePkgsGrid.DataBind();
        }

        #endregion

        #region Public Override Methods

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            var package = SelectedPackage.Data;
            var useContentCollaborators = (bool)Page.SessionVariables.GetValueByName("UseContentCollaborators");
            Page.ClearValues();
            if (UpdateChangePkgTabContainer.Tabs[2].Selected && !useContentCollaborators)
            {
                UpdateChangePkgTabContainer.SelectedIndex = 0;
            }
            SelectedPackage.Data = package;
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            var data = serviceData as UpdateChangePkg;
            if (TargetsGrid != null)
            {
                var selectedTargets = TargetsGrid.GridContext.GetSelectedItems(false);
                if (selectedTargets == null)
                    return;
                data.ServiceDetail = new UpdateChangePkgDetail { TargetSystems = new TargetSystemDetail[selectedTargets.Length], FieldAction = Camstar.WCF.ObjectStack.Action.Create, CDOTypeName = "UpdateChangePkgDetail" };
                for (var i = 0; i < selectedTargets.Length; i++)
                    data.ServiceDetail.TargetSystems[i] = new TargetSystemDetail { IsTargetSelected = true, TargetSystem = new TargetSystem { Self = ((TargetSystem)selectedTargets[i]).Self } };
            }

            if (preReqChangePkgs != null)
            {
                if (data.ServiceDetail == null)
                {
                    data.ServiceDetail = new UpdateChangePkgDetail();
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
            if (SelectedPackage != null && SelectedPackage.Data != null && status.IsSuccess)
            {
                var package = SelectedPackage.Data;
                Page.ClearValues();
                SelectedPackage.Data = package;
            }
            Page.SetFocus(SelectedPackage.ClientID);
        }

        #endregion

        #region Private Functions

        protected virtual TargetSystemDetail[] RequestTargetSystems()
        {
            var service = new UpdateChangePkgService(
                    Camstar.WebPortal.FormsFramework.Utilities.FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var cdo = new UpdateChangePkg() { ChangePackage = SelectedPackage.Data as NamedObjectRef };
            var request = new UpdateChangePkg_Request
            {
                Info = new UpdateChangePkg_Info()
                {
                    ServiceDetail = new UpdateChangePkgDetail_Info { TargetSystems = new TargetSystemDetail_Info { RequestValue = true } }
                }
            };
            UpdateChangePkg_Result result;
            ResultStatus res = service.Load(cdo, request, out result);
            if (res.IsSuccess && result.Value.ServiceDetail != null && result.Value.ServiceDetail.TargetSystems != null)
            {
                var sourse = result.Value.ServiceDetail.TargetSystems.FirstOrDefault(s => s.TargetSystem.IsSourceSystem.Value);
                if (sourse != null)
                {
                    ViewState["SourceSystemVersion"] = sourse.TargetSystem.CamstarVersion;
                    ViewState["SourceMdbVersion"] = sourse.TargetSystem.MDBVersion;
                }
                return result.Value.ServiceDetail.TargetSystems.Where(s => !s.TargetSystem.IsSourceSystem.Value).ToArray();
            }
            return new TargetSystemDetail[0];
        }

        #endregion

        #region Constants
        private const string _preReqDM = "PreReqChangePkgs";
        #endregion

        #region Private Member Variables
        protected virtual PackageInquiryDetail[] preReqChangePkgs
        {
            get
            {
                return Page.DataContract.GetValueByName<PackageInquiryDetail[]>(_preReqDM);
            }
            set
            {
                Page.DataContract.SetValueByName(_preReqDM, value);
            }
        }
        #endregion

    }
}
