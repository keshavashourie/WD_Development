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
/// Summary description for SS_WIPDataSetupMatrixMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_ProcessTimerMatrixMaint : SS_SetupBModelingBase
    {
		protected CWC.RevisionedObject Selection_Product { get { return Page.FindCamstarControl("Selection_Product") as CWC.RevisionedObject; } }
		protected CWC.RevisionedObject Selection_Spec { get { return Page.FindCamstarControl("Selection_Spec") as CWC.RevisionedObject; } }
		protected CWC.WorkflowNavigator Selection_WIPStepWfNavigator { get { return Page.FindCamstarControl("Selection_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }
		
		protected CWC.RevisionedObject ObjectChanges_Product { get { return Page.FindCamstarControl("ObjectChanges_Product") as CWC.RevisionedObject; } }
		protected CWC.RevisionedObject ObjectChanges_Spec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
		protected CWC.WorkflowNavigator ObjectChanges_WIPStepWfNavigator { get { return Page.FindCamstarControl("ObjectChanges_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }


		protected CWC.NamedObject ObjectChanges_WIPDataSetup { get { return Page.FindCamstarControl("ObjectChanges_WIPDataSetup") as CWC.NamedObject; } }
		protected CWC.TextBox _txtSelectionName { get { return Page.FindCamstarControl("SelectionName") as CWC.TextBox; } }

		protected JQDataGrid ObjectChanges_ss_StartTimerTxnMap { get { return Page.FindCamstarControl("ObjectChanges_ss_StartTimerTxnMap") as JQDataGrid; } }
        protected JQDataGrid ObjectChanges_ss_StopTimerTxnMap { get { return Page.FindCamstarControl("ObjectChanges_ss_StopTimerTxnMap") as JQDataGrid; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
			_SelectionControls.Add(Selection_Product);
			_SelectionControls.Add(Selection_Spec);
			_SelectionControls.Add(Selection_WIPStepWfNavigator);

            // add the criteria WP controls to the control collection
			_CriteriaControls.Add(ObjectChanges_Product);
			_CriteriaControls.Add(ObjectChanges_Spec);
			_CriteriaControls.Add(ObjectChanges_WIPStepWfNavigator);

			_SubentityGridControls.Add(ObjectChanges_ss_StartTimerTxnMap);
            _SubentityGridControls.Add(ObjectChanges_ss_StopTimerTxnMap);
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



