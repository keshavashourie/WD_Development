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
using CamstarPortal.WebControls;

namespace Camstar.WebPortal.WebPortlets.ChangeManagement
{
    public class ChangePackageWorkflowPopup : MatrixWebPart
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var workflow = Page.DataContract.GetValueByName<RevisionedObjectRef>("ChangePkgWorkflow");
            if (workflow == null)
                return;
            WorkflowViewer.WorkflowValue = workflow;
            string title = Camstar.WebPortal.FormsFramework.Utilities.FrameworkManagerUtil.GetLabelValue("Lbl_ChangePackageWorkflow");
            Page.Title = string.Format(title, workflow.Name);
            var workflowStep = Page.DataContract.GetValueByName<string>("ChangePkgWorkflowStep");
            if(!string.IsNullOrEmpty(workflowStep))
                WorkflowViewer.SelectedStep = new NamedSubentityRef(workflowStep);

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            var workflow = Page.DataContract.GetValueByName<RevisionedObjectRef>("ChangePkgWorkflow");
            if (workflow != null)
            {
                string title = Camstar.WebPortal.FormsFramework.Utilities.FrameworkManagerUtil.GetLabelValue("Lbl_ChangePackageWorkflow");
                Page.Title = string.Format(title, workflow.Name);
            }
        }

        protected virtual WorkflowViewerControl WorkflowViewer
        {
            get { return Page.FindCamstarControl("WorkflowViewer") as WorkflowViewerControl; }
        }

    }
}
