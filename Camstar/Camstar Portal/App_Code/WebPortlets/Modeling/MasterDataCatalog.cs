// Copyright Siemens 2023  
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.WCFUtilities;
using System.Web.UI.WebControls;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    /// <summary>
    /// Summary description for MasterDataCatalogMaint
    /// </summary
    public class MasterDataCatalog : MatrixWebPart
    {
        protected virtual JQDataGrid MasterDataCatalogGrid { get { return Page.FindCamstarControl("ObjectChanges_MasterDataCatalogDtl") as JQDataGrid; } }
        protected virtual JQDataGrid InstanceGrid { get { return Page.FindCamstarControl("InstanceGrid") as JQDataGrid; } }
        protected virtual CWC.RadioButton AllObjectButton { get { return Page.FindCamstarControl("allObjects") as CWC.RadioButton; } }
        protected virtual CWC.RadioButton ObjectsWChangeButton { get { return Page.FindCamstarControl("objectsWChange") as CWC.RadioButton; } }
        protected virtual CWC.RadioButton ObjectsWOChangeButton { get { return Page.FindCamstarControl("objectsWOChange") as CWC.RadioButton; } }

        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            if (ModelingContext != null)
                Page.RenderActions += Page_RenderActions;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            AllObjectButton.DataChanged += AllObjectButton_CheckedChanged;
            ObjectsWChangeButton.DataChanged += ObjectsWChangeButton_CheckedChanged;
            ObjectsWOChangeButton.DataChanged += ObjectsWOChangeButton_CheckedChanged;
        }

        public virtual void LoadMasterDataCatalogDtls()
        {
            var serv = new MasterDataCatalogMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var data = new MasterDataCatalogMaint();
            data.ObjectChanges = new MasterDataCatalogChanges();
            if (ModelingContext.Current != null)
                data.ObjectChanges.ObjectToChange = ModelingContext.Current as NamedObjectRef;

            var request = new MasterDataCatalogMaint_Request();
            request.Info = new MasterDataCatalogMaint_Info();
            request.Info.ObjectChanges = new MasterDataCatalogChanges_Info();
            request.Info.ObjectChanges.MasterDataCatalogDtl = new MasterDataCatalogDtlChanges_Info();
            request.Info.ObjectChanges.MasterDataCatalogDtl.RequestValue = true;

            var result = new MasterDataCatalogMaint_Result();
            var res = serv.LoadMasterDataCatalogDtl(data, request, out result);
            if (res.IsSuccess)
            {
                if (result != null)
                {
                    Page.PortalContext.LocalSession.Remove("MasterDataCatalogDtl");
                    new CallStack(Page.CallStackKey).Context.LocalSession.Add("MasterDataCatalogDtl", result.Value.ObjectChanges.MasterDataCatalogDtl);
                }
            }
            else
                DisplayMessage(res);
        }

        public override void DisplayValues(Service serviceData)
        {
            base.DisplayValues(serviceData);
            if (ModelingContext.State != MaintenanceBehaviorContext.MaintenanceState.New)
            {
                new CallStack(Page.CallStackKey).Context.LocalSession.Remove("MasterDataCatalogDtl");
                new CallStack(Page.CallStackKey).Context.LocalSession.Add("MasterDataCatalogDtl", (serviceData as MasterDataCatalogMaint).ObjectChanges.MasterDataCatalogDtl);
            }
            else
            {
                LoadMasterDataCatalogDtls();
                MasterDataCatalogGrid.Data = new CallStack(Page.CallStackKey).Context.LocalSession["MasterDataCatalogDtl"] as MasterDataCatalogDtlChanges[];
            }
            AllObjectButton.Data = true;
        }

        public override void GetInputData(Service serviceData)
        {
            MasterDataCatalogGrid.Data = new CallStack(Page.CallStackKey).Context.LocalSession["MasterDataCatalogDtl"] as MasterDataCatalogDtlChanges[];
            base.GetInputData(serviceData);
        }

        protected virtual void AllObjectButton_CheckedChanged(object sender, EventArgs e)
        {
            MasterDataCatalogGrid.Data = new CallStack(Page.CallStackKey).Context.LocalSession["MasterDataCatalogDtl"] as MasterDataCatalogDtlChanges[];
        }

        protected virtual void ObjectsWChangeButton_CheckedChanged(object sender, EventArgs e)
        {
            var list = new CallStack(Page.CallStackKey).Context.LocalSession["MasterDataCatalogDtl"] as MasterDataCatalogDtlChanges[];
            MasterDataCatalogGrid.Data = list.Where(n => n.AnyWorkflowControlled.Value || n.ApprovalWorkflowControlled.Value).ToArray();
        }

        protected virtual void ObjectsWOChangeButton_CheckedChanged(object sender, EventArgs e)
        {
            var list = new CallStack(Page.CallStackKey).Context.LocalSession["MasterDataCatalogDtl"] as MasterDataCatalogDtlChanges[];
            MasterDataCatalogGrid.Data = list.Where(n => !n.AnyWorkflowControlled.Value && !n.ApprovalWorkflowControlled.Value).ToArray();
        }

        protected virtual void Page_RenderActions(object sender, EventArgs e)
        {
            if (ModelingContext != null)
            {
                var panelActions = Page.ActionDispatcher.ActionPanelActions();
                var newAction = panelActions.FirstOrDefault(act => act.Name == "NewBtn");                
                if (ModelingContext.State == MaintenanceBehaviorContext.MaintenanceState.Edit)
                {
                    var copyAction = panelActions.FirstOrDefault(act => act.Name == "CopyBtn");
                    if (copyAction != null)
                        copyAction.IsHidden = true;
                    if (newAction != null)
                        newAction.IsHidden = true;
                    var managePkgAction = panelActions.FirstOrDefault(act => act.Name == "ManagePkgBtn");
                    if (managePkgAction != null)
                        managePkgAction.IsHidden = true;
                }
                else if (ModelingContext.State == MaintenanceBehaviorContext.MaintenanceState.None)
                {
                    if (!Page.IsPostBack && InstanceGrid != null && InstanceGrid.Data != null)
                    {
                        if (newAction != null)
                            newAction.IsHidden = true;
                    }
                }
            }
        }
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            AllObjectButton.Data = true;
        }

        protected virtual MaintenanceBehaviorContext ModelingContext
        {
            get { return Page.PortalContext as MaintenanceBehaviorContext; }
        }
    }
}






