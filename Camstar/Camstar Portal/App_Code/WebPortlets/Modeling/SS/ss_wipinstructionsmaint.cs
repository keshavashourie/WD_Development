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
    public class SS_WIPInstructionsMaint : SS_SetupBModelingBase
    {
        protected CWC.RevisionedObject Selection_Spec { get { return Page.FindCamstarControl("Selection_Spec") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject ObjectChanges_Spec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
        protected CWC.TextBox Selection_ContainerName { get { return Page.FindCamstarControl("Selection_ContainerName") as CWC.TextBox; } }
        protected CWC.TextBox ObjectChanges_ContainerName { get { return Page.FindCamstarControl("ObjectChanges_ContainerName") as CWC.TextBox; } }
         protected CWC.RevisionedObject Selection_Product { get { return Page.FindCamstarControl("Selection_Product") as CWC.RevisionedObject; } }
         protected CWC.RevisionedObject ObjectChanges_Product { get { return Page.FindCamstarControl("ObjectChanges_Product") as CWC.RevisionedObject; } }
         protected CWC.RevisionedObject Selection_ProcessSpec { get { return Page.FindCamstarControl("Selection_ProcessSpec") as CWC.RevisionedObject; } }
         protected CWC.RevisionedObject ObjectChanges_ProcessSpec { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpec") as CWC.RevisionedObject; } }
         protected CWC.NamedObject Selection_Owner { get { return Page.FindCamstarControl("Selection_Owner") as CWC.NamedObject; } }
		 protected CWC.NamedObject ObjectChanges_Owner { get { return Page.FindCamstarControl("ObjectChanges_Owner") as CWC.NamedObject; } }
		 protected CWC.WorkflowNavigator Selection_WIPStepWorkflow { get { return Page.FindCamstarControl("Selection_WIPStepWorkflow") as CWC.WorkflowNavigator; } }
		 protected CWC.WorkflowNavigator ObjectChanges_WIPStepWorkflow { get { return Page.FindCamstarControl("ObjectChanges_WIPStepWorkflow") as CWC.WorkflowNavigator; } }
		 

         protected CWC.TextBox ObjectChanges_MoveInInstruction { get { return Page.FindCamstarControl("ObjectChanges_MoveInInstruction") as CWC.TextBox; } }
         protected CWC.TextBox ObjectChanges_TrackInInstruction { get { return Page.FindCamstarControl("ObjectChanges_TrackInInstruction") as CWC.TextBox; } }
         protected CWC.TextBox ObjectChanges_TrackOutInstruction { get { return Page.FindCamstarControl("ObjectChanges_TrackOutInstruction") as CWC.TextBox; } }
         protected CWC.TextBox ObjectChanges_MoveOutInstruction { get { return Page.FindCamstarControl("ObjectChanges_MoveOutInstruction") as CWC.TextBox; } }
         protected CWC.TextBox ObjectChanges_Name { get { return Page.FindCamstarControl("ObjectChanges_Name") as CWC.TextBox; } }
		//protected CWC.WorkflowNavigator 
       
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
            _SelectionControls.Add(Selection_Spec);
            _SelectionControls.Add(Selection_ContainerName);
            _SelectionControls.Add(Selection_Product);
            _SelectionControls.Add(Selection_ProcessSpec);
            _SelectionControls.Add(Selection_Owner);
			_SelectionControls.Add(Selection_WIPStepWorkflow);
            // add the criteria WP controls to the control collection
            _CriteriaControls.Add(ObjectChanges_Spec);
            _CriteriaControls.Add(ObjectChanges_ContainerName);
            _CriteriaControls.Add(ObjectChanges_Name);
			_CriteriaControls.Add(ObjectChanges_WIPStepWorkflow);
            _CriteriaControls.Add(ObjectChanges_Product);
            _CriteriaControls.Add(ObjectChanges_ProcessSpec);
            _CriteriaControls.Add(ObjectChanges_Owner);

            _CriteriaControls.Add(ObjectChanges_MoveInInstruction);
            _CriteriaControls.Add(ObjectChanges_TrackInInstruction);
            _CriteriaControls.Add(ObjectChanges_TrackOutInstruction);
            _CriteriaControls.Add(ObjectChanges_MoveOutInstruction);

            _CriteriaWorkflowNavigators.Add(ObjectChanges_WIPStepWorkflow);

            ObjectChanges_WIPStepWorkflow.DataChanged += ObjectChanges_WIPStepWorkflow_DataChanged;
        }

        void ObjectChanges_WIPStepWorkflow_DataChanged(object sender, EventArgs e)
        {
            SS_SetupB_StackData_Update("ObjectChanges_WIPStepWorkflow");
        }

    }
}



