/* Copyright 2022 Siemens */
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
/// Summary description for SS_ToolPlanMatrixMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_ToolPlanMatrixMaint : SS_SetupBModelingBaseR2
    {
        protected CWC.RevisionedObject _rdoSelectionProduct { get { return Page.FindCamstarControl("Selection_Product") as CWC.RevisionedObject; } }
        protected CWC.NamedObject _ndoSelectionOwner { get { return Page.FindCamstarControl("Selection_Owner") as CWC.NamedObject; } }
        protected CWC.RevisionedObject _rdoSelectionProcessSpec { get { return Page.FindCamstarControl("Selection_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoSelectionSpec { get { return Page.FindCamstarControl("Selection_Spec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject _ndoSelectionEquipmentFamily { get { return Page.FindCamstarControl("Selection_EquipmentFamily") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoSelectionEquipment { get { return Page.FindCamstarControl("Selection_Equipment") as CWC.NamedObject; } }
		protected CWC.WorkflowNavigator Selection_WIPStepWfNavigator { get { return Page.FindCamstarControl("Selection_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }
        
        protected CWC.RevisionedObject _rdoProduct { get { return Page.FindCamstarControl("ObjectChanges_Product") as CWC.RevisionedObject; } }
        protected CWC.NamedObject _ndoOwner { get { return Page.FindCamstarControl("ObjectChanges_Owner") as CWC.NamedObject; } }
        protected CWC.RevisionedObject _rdoProcessSpec { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoSpec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject _ndoEquipmentFamily { get { return Page.FindCamstarControl("ObjectChanges_EquipmentFamily") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEquipment { get { return Page.FindCamstarControl("ObjectChanges_Equipment") as CWC.NamedObject; } }
		protected CWC.WorkflowNavigator ObjectChanges_WIPStepWfNavigator { get { return Page.FindCamstarControl("ObjectChanges_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }

        //---------------------------------------------------
        // Override OnLoad event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
            _SelectionControls.Add(_rdoSelectionProduct);
            _SelectionControls.Add(_ndoSelectionOwner);
            _SelectionControls.Add(_rdoSelectionProcessSpec);
            _SelectionControls.Add(_rdoSelectionSpec);
            _SelectionControls.Add(_ndoSelectionEquipmentFamily);
            _SelectionControls.Add(_ndoSelectionEquipment);
			_SelectionControls.Add(Selection_WIPStepWfNavigator);

            // add the criteria WP controls to the control collection
            _CriteriaControls.Add(_rdoProduct);
            _CriteriaControls.Add(_ndoOwner);
            _CriteriaControls.Add(_rdoProcessSpec);
            _CriteriaControls.Add(_rdoSpec);
            _CriteriaControls.Add(_ndoEquipmentFamily);
            _CriteriaControls.Add(_ndoEquipment);
			_CriteriaControls.Add(ObjectChanges_WIPStepWfNavigator);

            // specify the hidden colums for the selection grid.
            _HiddenGridColumns.Add("RN");

            _CriteriaWorkflowNavigators.Add(ObjectChanges_WIPStepWfNavigator);

            ObjectChanges_WIPStepWfNavigator.DataChanged += ObjectChanges_WIPStepWfNavigator_DataChanged;

            SEMI.AppCode.UIUtility.DisableMatrixFields(this, Page.PortalContext.DataContract.GetValueByName<string>("PopupDM"), _CriteriaControls);
        }

        void ObjectChanges_WIPStepWfNavigator_DataChanged(object sender, EventArgs e)
        {
            SS_SetupB_StackData_Update("ObjectChanges_WIPStepWfNavigator");
        }
    }
}



