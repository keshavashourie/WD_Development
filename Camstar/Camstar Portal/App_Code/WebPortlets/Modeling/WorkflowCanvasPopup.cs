// Copyright Siemens 2023  
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.WebPortlets;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using CamstarPortal.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.FormsFramework;
using System.Web.UI;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.PortalFramework;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class WorkflowCanvasPopup : MatrixWebPart
    {
        #region Protected Functions

      

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                var elements = Page.DataContract.GetValueByName("CanvasElements") as CanvasContext.CanvasElement[];
                if (elements != null && elements.Length != 0)
                    WorkflowViewer.WFControl.CanvasElements = new List<CanvasContext.CanvasElement>(elements);
            }
            Page.DataContract.SetValueByName("CanvasElements", null);
            ScriptManager.RegisterStartupScript(Page, GetType(), "Maximize", string.Format("MaximizeCanvasPopup();"), true);

            var resetBtn = ResetAction.Control as Camstar.WebPortal.FormsFramework.WebControls.Button;
            resetBtn.Confirmation = new Confirmation { OK_LabelName = "Web_Yes", Cancel_LabelName = "Web_No", Message_LabelName = "Lbl_LoseAllChanges", Title_LabelName = "StatusMessage_Warning" };
            CamstarWebControl.SetRenderToClient(resetBtn);
        }
        #endregion

        #region Public Functions

        public override void LoadPersonalization()
        {
            base.LoadPersonalization();
            var parentCtx = Page.CurrentCallStack.Parent?.Context as MaintenanceBehaviorContext;
            if (parentCtx != null && !string.IsNullOrEmpty(parentCtx.MaintService))
            {
                Page.PrimaryServiceType = parentCtx.MaintService;
            }
        }

        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action == null || !action.Parameters.Equals("NotifyParent"))
                return;

            Page.CurrentCallStack.Parent.Context.LocalSession["CanvasElements"] = WorkflowViewer.WFControl.CanvasElements;
            Page.CloseFloatingFrame(true);
        }
        #endregion

        #region Controls

        protected virtual Camstar.WebPortal.Personalization.UIAction ResetAction { get { return Page.ActionDispatcher.PageActions().FirstOrDefault(a => a.Name.Equals("ClosePopup")); } }
        protected virtual WorkflowViewerControl WorkflowViewer
        {
            get { return Page.FindCamstarControl("CanvasControl") as WorkflowViewerControl; }
        }
        #endregion
    }
}
