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
    public class SS_MinTimeWindowSetupMaint : SS_SetupBModelingBase
    {
        protected CWC.RevisionedObject Selection_Product { get { return Page.FindCamstarControl("Selection_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject Selection_ProcessSpec { get { return Page.FindCamstarControl("Selection_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject Selection_ProductLine { get { return Page.FindCamstarControl("Selection_ProductLine") as CWC.NamedObject; } }
        protected CWC.NamedObject Selection_Owner { get { return Page.FindCamstarControl("Selection_Owner") as CWC.NamedObject; } }
        protected CWC.RevisionedObject Selection_Spec { get { return Page.FindCamstarControl("Selection_Spec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject Selection_ResourceFamily { get { return Page.FindCamstarControl("Selection_ResourceFamily") as CWC.NamedObject; } }
		protected CWC.WorkflowNavigator Selection_WorkflowNav { get { return Page.FindCamstarControl("Selection_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }

        protected CWC.RevisionedObject ObjectChanges_Product { get { return Page.FindCamstarControl("ObjectChanges_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject ObjectChanges_ProcessSpec { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject ObjectChanges_ProductLine { get { return Page.FindCamstarControl("ObjectChanges_ProductLine") as CWC.NamedObject; } }
        protected CWC.NamedObject ObjectChanges_Owner { get { return Page.FindCamstarControl("ObjectChanges_Owner") as CWC.NamedObject; } }
        protected CWC.RevisionedObject ObjectChanges_Spec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject ObjectChanges_ResourceFamily { get { return Page.FindCamstarControl("ObjectChanges_ResourceFamily") as CWC.NamedObject; } }
		protected CWC.WorkflowNavigator ObjectChanges_WorkflowNavigator { get { return Page.FindCamstarControl("ObjectChanges_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }


        
       

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
            _SelectionControls.Add(Selection_Product);
            _SelectionControls.Add(Selection_ProcessSpec);
            _SelectionControls.Add(Selection_ProductLine);
            _SelectionControls.Add(Selection_Owner);
            _SelectionControls.Add(Selection_Spec);
            _SelectionControls.Add(Selection_ResourceFamily);
			_SelectionControls.Add(Selection_WorkflowNav);
            
            // add the criteria WP controls to the control collection
            _CriteriaControls.Add(ObjectChanges_Product);
            _CriteriaControls.Add(ObjectChanges_ProcessSpec);
            _CriteriaControls.Add(ObjectChanges_ProductLine);
            _CriteriaControls.Add(ObjectChanges_Owner);
            _CriteriaControls.Add(ObjectChanges_Spec);
            _CriteriaControls.Add(ObjectChanges_ResourceFamily);
			_CriteriaControls.Add(ObjectChanges_WorkflowNavigator);
         
            // specify the hidden colums for the selection grid.
            _HiddenGridColumns.Add("RN");
            _CriteriaWorkflowNavigators.Add(ObjectChanges_WorkflowNavigator);

            ObjectChanges_WorkflowNavigator.DataChanged += ObjectChanges_WorkflowNavigator_DataChanged;
        }

        void ObjectChanges_WorkflowNavigator_DataChanged(object sender, EventArgs e)
        {
            SS_SetupB_StackData_Update("ObjectChanges_WIPStepWfNavigator");
        }
    }
}



