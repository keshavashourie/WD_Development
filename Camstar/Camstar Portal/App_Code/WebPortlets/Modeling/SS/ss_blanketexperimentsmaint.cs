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
/// Summary description for SS_BlanketExperimentsMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_BlanketExperimentsMaint : SS_SetupBModelingBaseR2
    {
        protected CWC.RevisionedObject Selection_Product { get { return Page.FindCamstarControl("Selection_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject Selection_ProcessSpec { get { return Page.FindCamstarControl("Selection_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject Selection_Spec { get { return Page.FindCamstarControl("Selection_Spec") as CWC.RevisionedObject; } }
        protected CWC.WorkflowNavigator Selection_WIPStepWfNavigator { get { return Page.FindCamstarControl("Selection_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }
        protected CWC.NamedObject Selection_EquipmentGroup { get { return Page.FindCamstarControl("Selection_EquipmentGroup") as CWC.NamedObject; } }
        protected CWC.NamedObject Selection_Equipment { get { return Page.FindCamstarControl("Selection_Equipment") as CWC.NamedObject; } }
        protected CWC.NamedObject Selection_scsWorkOrder { get { return Page.FindCamstarControl("Selection_scsWorkOrder") as CWC.NamedObject; } }

        protected CWC.RevisionedObject ObjectChanges_Product { get { return Page.FindCamstarControl("ObjectChanges_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject ObjectChanges_ProcessSpec { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject ObjectChanges_Spec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
        protected CWC.WorkflowNavigator ObjectChanges_WIPStepWfNavigator { get { return Page.FindCamstarControl("ObjectChanges_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }
        protected CWC.DateChooser ObjectChanges_EffectiveFromDate { get { return Page.FindCamstarControl("ObjectChanges_EffectiveFromDate") as CWC.DateChooser; } }
        protected CWC.DateChooser ObjectChanges_EffectiveThruDate { get { return Page.FindCamstarControl("ObjectChanges_EffectiveThruDate") as CWC.DateChooser; } }
        protected CWC.NamedObject ObjectChanges_EquipmentGroup { get { return Page.FindCamstarControl("ObjectChanges_EquipmentGroup") as CWC.NamedObject; } }
        protected CWC.NamedObject ObjectChanges_Equipment { get { return Page.FindCamstarControl("ObjectChanges_Equipment") as CWC.NamedObject; } }
        protected CWC.NamedObject ObjectChanges_scsWorkOrder { get { return Page.FindCamstarControl("ObjectChanges_scsWorkOrder") as CWC.NamedObject; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
            _SelectionControls.Add(Selection_Product);
            _SelectionControls.Add(Selection_ProcessSpec);
            _SelectionControls.Add(Selection_Spec);
            _SelectionControls.Add(Selection_scsWorkOrder);
            _SelectionControls.Add(Selection_WIPStepWfNavigator);
            _SelectionControls.Add(Selection_EquipmentGroup);
            _SelectionControls.Add(Selection_Equipment);

            // add the criteria WP controls to the control collection
            _CriteriaControls.Add(ObjectChanges_Product);
            _CriteriaControls.Add(ObjectChanges_ProcessSpec);
            _CriteriaControls.Add(ObjectChanges_scsWorkOrder);
            _CriteriaControls.Add(ObjectChanges_Spec);
            _CriteriaControls.Add(ObjectChanges_WIPStepWfNavigator);
            _CriteriaControls.Add(ObjectChanges_EquipmentGroup);
            _CriteriaControls.Add(ObjectChanges_Equipment);

            // specify the hidden colums for the selection grid.
            _HiddenGridColumns.Add("RN");

            _CriteriaWorkflowNavigators.Add(ObjectChanges_WIPStepWfNavigator);
            ObjectChanges_WIPStepWfNavigator.DataChanged += ObjectChanges_WIPStepWfNavigator_DataChanged;
       
            CamstarControlsCollection controls = new CamstarControlsCollection();
            controls.Add(ObjectChanges_Product);
            controls.Add(ObjectChanges_ProcessSpec);
            controls.Add(ObjectChanges_Spec);
            controls.Add(ObjectChanges_scsWorkOrder);
            controls.Add(ObjectChanges_WIPStepWfNavigator);
            controls.Add(ObjectChanges_EquipmentGroup);
            controls.Add(ObjectChanges_Equipment);
            controls.Add(ObjectChanges_EffectiveFromDate);
            controls.Add(ObjectChanges_EffectiveThruDate);

            SEMI.AppCode.UIUtility.DisableMatrixFields(this, Page.PortalContext.DataContract.GetValueByName<string>("PopupDM"), controls);

        }

        void ObjectChanges_WIPStepWfNavigator_DataChanged(object sender, EventArgs e)
        {
            SS_SetupB_StackData_Update("ObjectChanges_WIPStepWfNavigator");
        }
    }
}

