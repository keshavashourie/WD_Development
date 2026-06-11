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
/// Summary description for SS_BaseMatrixMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_BaseMatrixMaint : SS_SetupBModelingBaseR2
    {
        protected CWC.RevisionedObject Selection_Product { get { return Page.FindCamstarControl("Selection_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject Selection_ProcessSpec { get { return Page.FindCamstarControl("Selection_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject Selection_Spec { get { return Page.FindCamstarControl("Selection_Spec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject Selection_Equipment { get { return Page.FindCamstarControl("Selection_Equipment") as CWC.NamedObject; } }
        protected CWC.WorkflowNavigator Selection_WIPStepWorkflow { get { return Page.FindCamstarControl("Selection_WIPStepWorkflow") as CWC.WorkflowNavigator; } }
		protected CWC.WorkflowNavigator ObjectChanges_WIPStepWorkflow { get { return Page.FindCamstarControl("ObjectChanges_WIPStepWorkflow") as CWC.WorkflowNavigator; } }
        // protected CWC.NamedObject Selection_Owner { get { return Page.FindCamstarControl("Selection_Owner") as CWC.NamedObject; } }
        // protected CWC.NamedObject Selection_EquipmentFamily { get { return Page.FindCamstarControl("Selection_EquipmentFamily") as CWC.NamedObject; } }
        // protected CWC.NamedObject Selection_ToolPlan { get { return Page.FindCamstarControl("Selection_ToolPlan") as CWC.NamedObject; } }
		// protected CWC.NamedObject Selection_ProcessCapability { get { return Page.FindCamstarControl("Selection_ProcessCapability") as CWC.NamedObject; } }
        // protected CWC.NamedObject Selection_ProcessType { get { return Page.FindCamstarControl("Selection_ProcessType") as CWC.NamedObject; } }
        // protected CWC.NamedObject Selection_EquipmentGroup { get { return Page.FindCamstarControl("Selection_EquipmentGroup") as CWC.NamedObject; } }
        // protected CWC.NamedObject Selection_ProductLine { get { return Page.FindCamstarControl("Selection_ProductLine") as CWC.NamedObject; } }
        //Insert new Matrix Page Camstar Control


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
            _SelectionControls.Add(Selection_Product);
            _SelectionControls.Add(Selection_ProcessSpec);
            _SelectionControls.Add(Selection_Spec);
            _SelectionControls.Add(Selection_Equipment);
			_SelectionControls.Add(Selection_WIPStepWorkflow);
            // _SelectionControls.Add(Selection_Owner);
            // _SelectionControls.Add(Selection_ProcessType);
            // _SelectionControls.Add(Selection_EquipmentFamily);
            // _SelectionControls.Add(Selection_ToolPlan);
			// _SelectionControls.Add(Selection_ProcessCapability);
            // _SelectionControls.Add(Selection_EquipmentGroup);
            // _SelectionControls.Add(Selection_ProductLine);

            // specify the hidden colums for the selection grid.
            _HiddenGridColumns.Add("RN");
            // _CriteriaWorkflowNavigators.Add(ObjectChanges_WIPStepWorkflow);

            // ObjectChanges_WIPStepWorkflow.DataChanged += ObjectChanges_WIPStepWorkflow_DataChanged;

            SEMI.AppCode.UIUtility.DisableMatrixFields(this, Page.PortalContext.DataContract.GetValueByName<string>("PopupDM"), _CriteriaControls);        
        }

        // void ObjectChanges_WIPStepWorkflow_DataChanged(object sender, EventArgs e)
        // {
        //     SS_SetupB_StackData_Update("ObjectChanges_WIPStepWorkflow");
        // }

    }
}

