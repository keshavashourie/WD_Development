// Copyright Siemens 2023  
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.WebPortlets;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Label = Camstar.WCF.ObjectStack.Label;

namespace Camstar.WebPortal.WebPortlets.ChangeManagement
{
    public class DeployChangePackage : MatrixWebPart
    {

        #region Controls
        protected virtual JQDataGrid TargetsGrid { get { return Page.FindCamstarControl("TargetsGrid") as JQDataGrid; } }
        protected virtual JQDataGrid BuildStatusGrid { get { return Page.FindCamstarControl("BuildStatusGrid") as JQDataGrid; } }
        protected virtual JQDataGrid StatusPerTargetGrid { get { return Page.FindCamstarControl("StatusPerTargetGrid") as JQDataGrid; } }
        protected virtual NamedObject SelectedPackage { get { return Page.FindCamstarControl("SelectedPackage") as NamedObject; } }
        protected virtual Button DeployBtn { get { return Page.FindCamstarControl("DeployBtn") as Button; } }
        protected virtual Button RefreshBtn { get { return Page.FindCamstarControl("RefreshBtn") as Button; } }
        protected virtual Button ResetBtn { get { return Page.FindCamstarControl("ResetBtn") as Button; } }        
        protected virtual Camstar.WebPortal.PortalFramework.JQTabContainer TabsControl { get { return Page.FindCamstarControl("TabsControl") as Camstar.WebPortal.PortalFramework.JQTabContainer; } }
        #endregion

        #region Protected Functions

        protected override void OnLoad(EventArgs e)
        {
            SelectedPackage.DataChanged += (sender, args) =>
            {
                LoadDefaultTargets();
            };
            RefreshBtn.Click += (sender, args) =>
            {
                RequestDeploymentStatus();
            };            
            TabsControl.SelectedIndexChanged += (sender, args) =>
            {
                int tabindex = TabsControl.SelectedIndex;
                string currenttabname = TabsControl.Tabs[tabindex].Name;
                switch (currenttabname)
                {
                    case "DeployPackageTab":
                        DeployBtn.Visible = true;
                        ResetBtn.Visible = true;
                        RefreshBtn.Visible = false;
                        break;
                    case "DeploymentStatusTab":
                        DeployBtn.Visible = false;
                        ResetBtn.Visible = false;
                        RefreshBtn.Visible = true;
                        break;
                    default:
                        DeployBtn.Visible = false;
                        ResetBtn.Visible = false;
                        RefreshBtn.Visible = false;
                        break;
                }
            };
            ResetBtn.Click += (sender, args) =>
            {
                LoadDefaultTargets();
            };
            base.OnLoad(e);
        }
        #endregion
        #region Public Functions

        public override bool PreExecute(Info serviceInfo, Service serviceData)
        {
            bool isSuccess = base.PreExecute(serviceInfo, serviceData);
            if (TargetsGrid.Data == null || (TargetsGrid.Data as TargetSystem[]).Length == 0)
            {
                isSuccess = false;
                LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
                var label = labelCache.GetLabelByName("DeployChangePkg_MustAssignTargetToChangePkg");
                Page.DisplayMessage(label.Value, false);
            }

            return isSuccess;
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            var data = serviceData as WCF.ObjectStack.DeployChangePkg;
            if (data == null || TargetsGrid == null)
                return;
            if (data.ServiceDetail == null)
                data.ServiceDetail = new DeployChangePkgDetail() { FieldAction = Camstar.WCF.ObjectStack.Action.Create, CDOTypeName = "DeployChangePkgDetail" };
            var selectedTargets = TargetsGrid.GridContext.GetSelectedItems(false);
            if (selectedTargets != null)
            {
                data.ServiceDetail.TargetSystems = new TargetSystemDetail[selectedTargets.Length];
                for (var i = 0; i < selectedTargets.Length; i++)
                {
                    if (selectedTargets[i] != null)
                        data.ServiceDetail.TargetSystems[i] = new TargetSystemDetail { IsTargetSelected = true, TargetSystem = new TargetSystem { Self = ((TargetSystem)selectedTargets[i]).Self } };
                }
            }
        }

        //switch to deployment status tab after successful txn
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (!status.IsSuccess)
            {
                DeployBtn.Visible = true;
                ResetBtn.Visible = true;
                RefreshBtn.Visible = false;
                return;
            }
            else
            {
                DeployBtn.Visible = false;
                ResetBtn.Visible = false;
                RefreshBtn.Visible = true;
            }
            var statusTab = TabsControl.Tabs.OfType<Camstar.WebPortal.PortalFramework.JQTabPanel>().FirstOrDefault(t => t.Name.Equals("DeploymentStatusTab"));
            if (statusTab == null)
                return;
            TabsControl.SelectedIndex = statusTab.TabIndex;
            Camstar.WebPortal.FormsFramework.CamstarWebControl.SetRenderToClient(TabsControl);
        }

        #endregion

        #region Private Functions
        protected virtual TargetSystem[] RequestTargetSystems()
        {
            var service = new DeployChangePkgService(
                    Camstar.WebPortal.FormsFramework.Utilities.FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var cdo = new DeployChangePkg() { ChangePackage = SelectedPackage.Data as NamedObjectRef };
            var request = new DeployChangePkg_Request
            {
                Info = new DeployChangePkg_Info()
                {
                    ServiceDetail = new DeployChangePkgDetail_Info { TargetSystems = new TargetSystemDetail_Info { RequestValue = true } }
                }
            };
            DeployChangePkg_Result result;
            ResultStatus res = service.Load(cdo, request, out result);
            if (res.IsSuccess && result.Value.ServiceDetail != null && result.Value.ServiceDetail.TargetSystems != null)
                return result.Value.ServiceDetail.TargetSystems.Select(s => s.TargetSystem).ToArray();
            return null;
        }

        protected virtual void RequestDeploymentStatus()
        {
            BuildStatusGrid.ClearData();
            StatusPerTargetGrid.ClearData();

            var service = new DeployChangePkgService(
                    Camstar.WebPortal.FormsFramework.Utilities.FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var cdo = new DeployChangePkg() { ChangePackage = SelectedPackage.Data as NamedObjectRef };
            var request = new DeployChangePkg_Request
            {
                Info = new DeployChangePkg_Info()
                {
                    ServiceDetail = new DeployChangePkgDetail_Info { TargetDeployments = new TargetDeploymentDetail_Info { RequestValue = true } },
                    CPExportStatus = new Info(true),
                    BuildTimestamp = new Info(true)
                }
            };
            DeployChangePkg_Result result;
            ResultStatus res = service.Load(cdo, request, out result);
            if (!res.IsSuccess || result.Value == null)
                return;
            BuildStatusGrid.Data = new[] { new BuildStatusItem
                    {
                        BuildTimestamp = result.Value.BuildTimestamp == null ? string.Empty : result.Value.BuildTimestamp.Value.ToString(),
                        PackageBuildStatus = result.Value.CPExportStatus == null ? string.Empty : BuildStatusItem.CreateStatusFromCode(result.Value.CPExportStatus.Value),
                        PackageSize = string.Empty, // implement as soon as it is provided by BL
                        BuildMessage = result.Value.ACEMessage == null ? string.Empty : result.Value.ACEMessage.Value
                    }};
            if (result.Value.ServiceDetail != null && result.Value.ServiceDetail.TargetDeployments != null)
            {
                GetAllLabelsInOneRequest();
                StatusPerTargetGrid.Data = result.Value.ServiceDetail.TargetDeployments.Select(t => new TargetStatusItem
                {
                    Target = t.TargetSystemName == null ? string.Empty : t.TargetSystemName.Value,
                    DeployTimestamp = t.ExportDate == null ? string.Empty : t.ExportDate.Value.ToString(),
                    DeployStatus =
                        t.Status == null ? string.Empty : TargetStatusItem.CreateStatusFromCode(t.Status.Value),
                    Comments = t.Message == null || t.Status == TargetDeliveryStatusEnum.Completed
                        ? string.Empty
                        : t.Message.Value
                }).ToArray();
            }
        }
        protected virtual void LoadDefaultTargets()
        {
            TargetsGrid.ClearData();
            RequestDeploymentStatus();
            var targets = RequestTargetSystems();
            TargetsGrid.Data = targets;
            if (targets != null)
            {
                foreach (var target in targets)
                    TargetsGrid.GridContext.SelectRow(target.Name.Value, true);
            }
        }

        private void GetAllLabelsInOneRequest()
        {
            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
            var labels = new[]{
                new Label("Lbl_InProgress"),
                new Label("Lbl_Complete"),
                new Label("Lbl_Failed")
            };
            labelCache.GetLabels(new LabelList(labels));
        }
        #endregion

        #region Constants

        #endregion

        #region Private Member Variables

        #endregion
    }
    internal class BuildStatusItem
    {
        public virtual string BuildTimestamp { get; set; }
        public virtual string PackageBuildStatus { get; set; }
        public virtual string PackageSize { get; set; }
        public virtual string BuildMessage { get; set; }
        public static string CreateStatusFromCode(int statusCode)
        {
            int code = -1;
            switch (statusCode)
            {
                case 1:
                case 8:
                case 9:
                    code = 1;
                    break;
                case 2:
                case 5:
                    code = 2;
                    break;
                case 3:
                case 4:
                case 6:
                case 7:
                    code = 3;
                    break;
            }

            if (code != -1)
            {
                ExportImportStatusEnum e = (ExportImportStatusEnum)code;
                var memInfo = e.GetType().GetMember(e.ToString());
                var metaAttr = memInfo[0].GetCustomAttributes(typeof(MetadataAttribute), false) as MetadataAttribute[];

                LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                var label = labelCache.GetLabelById(metaAttr[0].LabelID);
                return label.Value;
            }
            return string.Empty;
        }
    }
    internal class TargetStatusItem
    {
        public virtual string Target { get; set; }
        public virtual string DeployTimestamp { get; set; }
        public virtual string DeployStatus { get; set; }
        public virtual string Comments { get; set; }
        public static string CreateStatusFromCode(int statusCode)
        {
            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
            switch (statusCode)
            {
                case 5:
                    return labelCache.GetLabelByName("Lbl_InProgress").Value;
                case 2:
                    return labelCache.GetLabelByName("Lbl_Complete").Value;
                case 3:
                    return labelCache.GetLabelByName("Lbl_Failed").Value;
                default:
                    return string.Empty;
            }
        }
    }
}
