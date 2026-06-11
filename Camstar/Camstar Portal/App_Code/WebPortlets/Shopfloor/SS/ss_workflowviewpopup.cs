// Copyright Siemens 2019  
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

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_WorkflowViewPopup : MatrixWebPart
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Page.DataContract.GetValueByName("WorkflowStepCtl") != null)
            {
                string workflowStep = Page.DataContract.GetValueByName("WorkflowStepCtl").ToString();
                if (!string.IsNullOrEmpty(workflowStep))
                    WorkflowViewer.SelectedStep = new NamedSubentityRef(workflowStep);
            }
            if (!Page.IsPostBack)
            {
                SEMI.AppCode.UIUtility.MaximizePopUp(this);
            }

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }

        protected virtual WorkflowViewerControl WorkflowViewer
        {
            get { return Page.FindCamstarControl("WFViewer") as WorkflowViewerControl; }
        }

    }
}
