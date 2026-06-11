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
/// Summary description for SS_CheckSheetMatrixMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_CheckSheetMatrixMaint : SS_SetupBModelingBase
    {
        protected CWC.TextBox _txtSelectionLotId { get { return Page.FindCamstarControl("Selection_LotId") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoSelectionEquipment { get { return Page.FindCamstarControl("Selection_Equipment") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoSelectionProcessType { get { return Page.FindCamstarControl("Selection_ProcessType") as CWC.NamedObject; } }
        protected CWC.RevisionedObject _rdoSelectionProduct { get { return Page.FindCamstarControl("Selection_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoSelectionProcessSpec { get { return Page.FindCamstarControl("Selection_ProcessSpec") as CWC.RevisionedObject; } }
		protected CWC.RevisionedObject _rdoSelectionSpec { get { return Page.FindCamstarControl("Selection_Spec") as CWC.RevisionedObject; } }
		protected CWC.WorkflowNavigator Selection_WIPStepWorkflow { get { return Page.FindCamstarControl("Selection_WIPStepWorkflow") as CWC.WorkflowNavigator; } }
		protected CWC.WorkflowNavigator ObjectChanges_WIPStepWorkflow { get { return Page.FindCamstarControl("ObjectChanges_WIPStepWorkflow") as CWC.WorkflowNavigator; } }


        protected CWC.TextBox _txtLotId { get { return Page.FindCamstarControl("ObjectChanges_LotId") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoEquipment { get { return Page.FindCamstarControl("ObjectChanges_Equipment") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoProcessType { get { return Page.FindCamstarControl("ObjectChanges_ProcessType") as CWC.NamedObject; } }
        protected CWC.RevisionedObject _rdoProduct { get { return Page.FindCamstarControl("ObjectChanges_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoProcessSpec { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoSpec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
        protected JQDataGrid ObjectChanges_CheckSheetDetails { get { return Page.FindCamstarControl("ObjectChanges_CheckSheetDetails") as JQDataGrid; } }
		

     
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            //add the selection WP controls to the control collection
            _SelectionControls.Add(_txtSelectionLotId);
            _SelectionControls.Add(_ndoSelectionEquipment);
            _SelectionControls.Add(_rdoSelectionProduct);
            _SelectionControls.Add(_rdoSelectionProcessSpec);
            _SelectionControls.Add(_rdoSelectionSpec);
            _SelectionControls.Add(_ndoSelectionProcessType);
			_SelectionControls.Add(Selection_WIPStepWorkflow);
            // add the criteria WP controls to the control collection
            _CriteriaControls.Add(_txtLotId);
            _CriteriaControls.Add(_ndoEquipment);
            _CriteriaControls.Add(_rdoProduct);
            _CriteriaControls.Add(_rdoProcessSpec);
            _CriteriaControls.Add(_rdoSpec);
            _CriteriaControls.Add(_ndoProcessType);
			_SubentityGridControls.Add(ObjectChanges_CheckSheetDetails);
			_CriteriaControls.Add(ObjectChanges_WIPStepWorkflow);

            // specify the hidden colums for the selection grid.
            _HiddenGridColumns.Add("RN");
            _HiddenGridColumns.Add("ElectronicProcedure");
            _HiddenGridColumns.Add("FailureAction");

            _CriteriaWorkflowNavigators.Add(ObjectChanges_WIPStepWorkflow);

            ObjectChanges_WIPStepWorkflow.DataChanged += ObjectChanges_WIPStepWorkflow_DataChanged;
        }

        void ObjectChanges_WIPStepWorkflow_DataChanged(object sender, EventArgs e)
        {
            SS_SetupB_StackData_Update("ObjectChanges_WIPStepWorkflow");
        }


        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
        }

       

        

    }
}



