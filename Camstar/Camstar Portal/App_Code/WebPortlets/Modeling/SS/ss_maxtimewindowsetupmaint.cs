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
/// Summary description for SS_PrintingComputerMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_MaxTimeWindowSetupMaint : SS_SetupBModelingBase
    {
        protected CWC.RevisionedObject Selection_StartSpec { get { return Page.FindCamstarControl("Selection_StartSpec") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject Selection_StopSpec { get { return Page.FindCamstarControl("Selection_StopSpec") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject Selection_Product { get { return Page.FindCamstarControl("Selection_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject Selection_ProcessSpec { get { return Page.FindCamstarControl("Selection_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject Selection_ProductLine { get { return Page.FindCamstarControl("Selection_ProductLine") as CWC.NamedObject; } }
        protected CWC.NamedObject Selection_Owner { get { return Page.FindCamstarControl("Selection_Owner") as CWC.NamedObject; } }

        protected CWC.RevisionedObject ObjectChanges_StartSpec { get { return Page.FindCamstarControl("ObjectChanges_StartSpec") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject ObjectChanges_StopSpec { get { return Page.FindCamstarControl("ObjectChanges_StopSpec") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject ObjectChanges_Product { get { return Page.FindCamstarControl("ObjectChanges_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject ObjectChanges_ProcessSpec { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject ObjectChanges_ProductLine { get { return Page.FindCamstarControl("ObjectChanges_ProductLine") as CWC.NamedObject; } }
        protected CWC.NamedObject ObjectChanges_Owner { get { return Page.FindCamstarControl("ObjectChanges_Owner") as CWC.NamedObject; } }
		protected CWC.WorkflowNavigator Selection_StartWorkflowNav { get { return Page.FindCamstarControl("Selection_StartStepWfNavigator") as CWC.WorkflowNavigator; } }
		protected CWC.WorkflowNavigator Selection_StopWorkflowNav { get { return Page.FindCamstarControl("Selection_StopStepWfNavigator") as CWC.WorkflowNavigator; } }
        protected CWC.TextBox ObjectChanges_Name { get { return Page.FindCamstarControl("ObjectChanges_Name") as CWC.TextBox; } }

        protected CWC.TextBox ObjectChanges_MaxTime { get { return Page.FindCamstarControl("ObjectChanges_MaxTime") as CWC.TextBox; } }
        protected CWC.DropDownList ObjectChanges_OvertimeAction { get { return Page.FindCamstarControl("ObjectChanges_OvertimeAction") as CWC.DropDownList; } }
        protected CWC.NamedObject ObjectChanges_HoldReason { get { return Page.FindCamstarControl("ObjectChanges_HoldReason") as CWC.NamedObject; } }
        protected CWC.NamedObject ObjectChanges_EmailGroup { get { return Page.FindCamstarControl("ObjectChanges_EmailGroup") as CWC.NamedObject; } }
		protected CWC.WorkflowNavigator ObjectChanges_StartWorkflowNavigator { get { return Page.FindCamstarControl("ObjectChanges_StartStepWfNavigator") as CWC.WorkflowNavigator; } }
		protected CWC.WorkflowNavigator ObjectChanges_StopWorkflowNavigator { get { return Page.FindCamstarControl("ObjectChanges_StopStepWfNavigator") as CWC.WorkflowNavigator; } }
       

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
            _SelectionControls.Add(Selection_StartSpec);
            _SelectionControls.Add(Selection_StopSpec);
            _SelectionControls.Add(Selection_Product);
            _SelectionControls.Add(Selection_ProcessSpec);
            _SelectionControls.Add(Selection_ProductLine);
            _SelectionControls.Add(Selection_Owner);
			_SelectionControls.Add(Selection_StartWorkflowNav);
			_SelectionControls.Add(Selection_StopWorkflowNav);

            // add the criteria WP controls to the control collection
            _CriteriaControls.Add(ObjectChanges_StartSpec);
            _CriteriaControls.Add(ObjectChanges_StopSpec);
            _CriteriaControls.Add(ObjectChanges_Product);
            _CriteriaControls.Add(ObjectChanges_ProcessSpec);
            _CriteriaControls.Add(ObjectChanges_ProductLine);
            _CriteriaControls.Add(ObjectChanges_Owner);
            _CriteriaControls.Add(ObjectChanges_Name);
			_CriteriaControls.Add(ObjectChanges_StartWorkflowNavigator);
			_CriteriaControls.Add(ObjectChanges_StopWorkflowNavigator);

            _HiddenGridColumns.Add("RN");

            _CriteriaWorkflowNavigators.Add(ObjectChanges_StartWorkflowNavigator);
            _CriteriaWorkflowNavigators.Add(ObjectChanges_StopWorkflowNavigator);

            ObjectChanges_StartWorkflowNavigator.DataChanged += ObjectChanges_StartWorkflowNavigator_DataChanged;
            ObjectChanges_StopWorkflowNavigator.DataChanged += ObjectChanges_StopWorkflowNavigator_DataChanged;
        }

        void ObjectChanges_StartWorkflowNavigator_DataChanged(object sender, EventArgs e)
        {
            SS_SetupB_StackData_Update("ObjectChanges_StartStepWfNavigator");
        }

        void ObjectChanges_StopWorkflowNavigator_DataChanged(object sender, EventArgs e)
        {
            SS_SetupB_StackData_Update("ObjectChanges_StopStepWfNavigator");
        }

    }
}



