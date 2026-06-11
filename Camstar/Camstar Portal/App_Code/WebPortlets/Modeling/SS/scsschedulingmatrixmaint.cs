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
/// Summary description for SS_BlanketExperimentsMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class scsSchedulingMatrixMaint : SS_SetupBModelingBase
    {
		protected CWC.RevisionedObject Selection_Product { get { return Page.FindCamstarControl("Selection_Product") as CWC.RevisionedObject; } }
		protected CWC.RevisionedObject Selection_ProcessSpec { get { return Page.FindCamstarControl("Selection_ProcessSpec") as CWC.RevisionedObject; } }
		protected CWC.RevisionedObject Selection_Spec { get { return Page.FindCamstarControl("Selection_Spec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject Selection_ProcessType { get { return Page.FindCamstarControl("Selection_ProcessType") as CWC.NamedObject; } }
        protected CWC.WorkflowNavigator Selection_WIPStepWfNavigator { get { return Page.FindCamstarControl("Selection_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }
		protected CWC.NamedObject Selection_EquipmentGroup { get { return Page.FindCamstarControl("Selection_EquipmentGroup") as CWC.NamedObject; } }
		protected CWC.NamedObject Selection_Equipment { get { return Page.FindCamstarControl("Selection_Equipment") as CWC.NamedObject; } }		
		protected CWC.RevisionedObject ObjectChanges_Product { get { return Page.FindCamstarControl("ObjectChanges_Product") as CWC.RevisionedObject; } }
		protected CWC.RevisionedObject ObjectChanges_ProcessSpec { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpec") as CWC.RevisionedObject; } }
		protected CWC.RevisionedObject ObjectChanges_Spec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject ObjectChanges_ProcessType { get { return Page.FindCamstarControl("ObjectChanges_ProcessType") as CWC.NamedObject; } }
        protected CWC.WorkflowNavigator ObjectChanges_WIPStepWfNavigator { get { return Page.FindCamstarControl("ObjectChanges_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }
		protected CWC.NamedObject ObjectChanges_EquipmentGroup { get { return Page.FindCamstarControl("ObjectChanges_EquipmentGroup") as CWC.NamedObject; } }
		protected CWC.NamedObject ObjectChanges_Equipment { get { return Page.FindCamstarControl("ObjectChanges_Equipment") as CWC.NamedObject; } }
		
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
			_SelectionControls.Add(Selection_Product);
			_SelectionControls.Add(Selection_ProcessSpec);
			_SelectionControls.Add(Selection_Spec);
            _SelectionControls.Add(Selection_ProcessType);
            _SelectionControls.Add(Selection_WIPStepWfNavigator);
			_SelectionControls.Add(Selection_EquipmentGroup);
			_SelectionControls.Add(Selection_Equipment);          

            // add the criteria WP controls to the control collection
			_CriteriaControls.Add(ObjectChanges_Product);
			_CriteriaControls.Add(ObjectChanges_ProcessSpec);
			_CriteriaControls.Add(ObjectChanges_Spec);
            _CriteriaControls.Add(ObjectChanges_ProcessType);
            _CriteriaControls.Add(ObjectChanges_WIPStepWfNavigator);    
			_CriteriaControls.Add(ObjectChanges_EquipmentGroup);
			_CriteriaControls.Add(ObjectChanges_Equipment);

            // specify the hidden colums for the selection grid.
            _HiddenGridColumns.Add("RN");

			_CriteriaWorkflowNavigators.Add(ObjectChanges_WIPStepWfNavigator);
			ObjectChanges_WIPStepWfNavigator.DataChanged += ObjectChanges_WIPStepWfNavigator_DataChanged;
		}

		void ObjectChanges_WIPStepWfNavigator_DataChanged(object sender, EventArgs e)
		{
			SS_SetupB_StackData_Update("ObjectChanges_WIPStepWfNavigator");
		}
    }
}



