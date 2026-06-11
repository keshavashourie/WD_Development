/* Copyright 2023 Siemens */
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
/// Summary description for scsCarrierFamilyMAtrixMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class scsCarrierFamilyMatrixMaint : SS_SetupBModelingBaseR2
    {
		protected CWC.RevisionedObject Selection_scsProduct { get { return Page.FindCamstarControl("Selection_scsProduct") as CWC.RevisionedObject; } }
		protected CWC.RevisionedObject Selection_scsProcessSpec { get { return Page.FindCamstarControl("Selection_scsProcessSpec") as CWC.RevisionedObject; } }
		protected CWC.RevisionedObject Selection_scsSpec { get { return Page.FindCamstarControl("Selection_scsSpec") as CWC.RevisionedObject; } }
		protected CWC.WorkflowNavigator Selection_scsWIPStepWorkflow { get { return Page.FindCamstarControl("Selection_scsWIPStepWorkflow") as CWC.WorkflowNavigator; } }
		protected CWC.NamedObject Selection_scsProcessType { get { return Page.FindCamstarControl("Selection_scsProcessType") as CWC.NamedObject; } }
		protected CWC.NamedObject Selection_scsProductLine { get { return Page.FindCamstarControl("Selection_scsProductLine") as CWC.NamedObject; } }
		protected CWC.NamedObject Selection_scsEquipmentFamily { get { return Page.FindCamstarControl("Selection_scsEquipmentFamily") as CWC.NamedObject; } }
		protected CWC.NamedObject Selection_scsEquipment { get { return Page.FindCamstarControl("Selection_scsEquipment") as CWC.NamedObject; } }
		
		protected CWC.RevisionedObject ObjectChanges_scsProduct { get { return Page.FindCamstarControl("ObjectChanges_scsProduct") as CWC.RevisionedObject; } }
		protected CWC.RevisionedObject ObjectChanges_scsProcessSpec { get { return Page.FindCamstarControl("ObjectChanges_scsProcessSpec") as CWC.RevisionedObject; } }
		protected CWC.RevisionedObject ObjectChanges_scsSpec { get { return Page.FindCamstarControl("ObjectChanges_scsSpec") as CWC.RevisionedObject; } }
		protected CWC.WorkflowNavigator ObjectChanges_scsWIPStepWorkflow { get { return Page.FindCamstarControl("ObjectChanges_scsWIPStepWorkflow") as CWC.WorkflowNavigator; } }
		protected CWC.NamedObject ObjectChanges_scsProcessType { get { return Page.FindCamstarControl("ObjectChanges_scsProcessType") as CWC.NamedObject; } }
		protected CWC.NamedObject ObjectChanges_scsProductLine { get { return Page.FindCamstarControl("ObjectChanges_scsProductLine") as CWC.NamedObject; } }
		protected CWC.NamedObject ObjectChanges_scsEquipmentFamily { get { return Page.FindCamstarControl("ObjectChanges_scsEquipmentFamily") as CWC.NamedObject; } }
		protected CWC.NamedObject ObjectChanges_scsEquipment { get { return Page.FindCamstarControl("ObjectChanges_scsEquipment") as CWC.NamedObject; } }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
			_SelectionControls.Add(Selection_scsProduct);
			_SelectionControls.Add(Selection_scsProcessSpec);
			_SelectionControls.Add(Selection_scsSpec);
			_SelectionControls.Add(Selection_scsWIPStepWorkflow);
			_SelectionControls.Add(Selection_scsProcessType);
			_SelectionControls.Add(Selection_scsProductLine);			
			_SelectionControls.Add(Selection_scsEquipmentFamily);
			_SelectionControls.Add(Selection_scsEquipment);          

            // add the criteria WP controls to the control collection
			_CriteriaControls.Add(ObjectChanges_scsProduct);
			_CriteriaControls.Add(ObjectChanges_scsProcessSpec);
			_CriteriaControls.Add(ObjectChanges_scsSpec);
			_CriteriaControls.Add(ObjectChanges_scsWIPStepWorkflow);
			_CriteriaControls.Add(ObjectChanges_scsProcessType);
			_CriteriaControls.Add(ObjectChanges_scsProductLine);                     
			_CriteriaControls.Add(ObjectChanges_scsEquipmentFamily);
			_CriteriaControls.Add(ObjectChanges_scsEquipment);

            // specify the hidden colums for the selection grid.
            _HiddenGridColumns.Add("RN");
            _CriteriaWorkflowNavigators.Add(ObjectChanges_scsWIPStepWorkflow);

            ObjectChanges_scsWIPStepWorkflow.DataChanged += ObjectChanges_scsWIPStepWorkflow_DataChanged;
			SEMI.AppCode.UIUtility.DisableMatrixFields(this, Page.PortalContext.DataContract.GetValueByName<string>("PopupDM"), _CriteriaControls);
        }

        void ObjectChanges_scsWIPStepWorkflow_DataChanged(object sender, EventArgs e)
        {
            SS_SetupB_StackData_Update("ObjectChanges_scsWIPStepWorkflow");
        }

    }
}

