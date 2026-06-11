/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

/// <summary>
/// Summary description for SS_SendAheadMatrixMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_SendAheadMatrixMaint : SS_SetupBModelingBase
    {
        protected CWC.RevisionedObject Selection_Spec { get { return Page.FindCamstarControl("Selection_Spec") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject Selection_Product { get { return Page.FindCamstarControl("Selection_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject Selection_ProcessSpec { get { return Page.FindCamstarControl("Selection_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.WorkflowNavigator _wfSelectionWIPStepWf { get { return Page.FindCamstarControl("Selection_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }

        protected CWC.RevisionedObject ObjectChanges_Spec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject ObjectChanges_Product { get { return Page.FindCamstarControl("ObjectChanges_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject ObjectChanges_ProcessSpec { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.WorkflowNavigator _wfWIPStepWf { get { return Page.FindCamstarControl("ObjectChanges_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }

        protected CWC.TextBox ObjectChanges_Name { get { return Page.FindCamstarControl("ObjectChanges_Name") as CWC.TextBox; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
            _SelectionControls.Add(Selection_Spec);
            _SelectionControls.Add(Selection_Product);
            _SelectionControls.Add(Selection_ProcessSpec);
            _SelectionControls.Add(_wfSelectionWIPStepWf);

            // add the criteria WP controls to the control collection
            _CriteriaControls.Add(ObjectChanges_Spec);
            _CriteriaControls.Add(ObjectChanges_Product);
            _CriteriaControls.Add(ObjectChanges_ProcessSpec);
            _CriteriaControls.Add(ObjectChanges_Name);
            _CriteriaControls.Add(_wfWIPStepWf);

            _HiddenGridColumns.Add("RN");

            _CriteriaWorkflowNavigators.Add(_wfWIPStepWf);

            _wfWIPStepWf.DataChanged += _wfWIPStepWf_DataChanged;
        }

        void _wfWIPStepWf_DataChanged(object sender, EventArgs e)
        {
            SS_SetupB_StackData_Update("ObjectChanges_WIPStepWfNavigator");
        }

    }
}



