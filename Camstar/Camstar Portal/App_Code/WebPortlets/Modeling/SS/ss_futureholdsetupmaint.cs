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
/// Summary description for SS_FutureHoldSetupMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_FutureHoldSetupMaint : SS_SetupBModelingBaseR2
    {
        protected CWC.RevisionedObject _rdoSelectionSpec { get { return Page.FindCamstarControl("Selection_Spec") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoSelectionProduct { get { return Page.FindCamstarControl("Selection_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoSelectionProcessSpec { get { return Page.FindCamstarControl("Selection_ProcessSpec") as CWC.RevisionedObject; } }
		protected CWC.NamedObject _ndoSelectionWorkOrder { get { return Page.FindCamstarControl("Selection_scsWorkOrder") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoSelectionOwner { get { return Page.FindCamstarControl("Selection_Owner") as CWC.NamedObject; } }
        protected CWC.TextBox _txtSelectionWaferRunNumber { get { return Page.FindCamstarControl("Selection_WaferRunNumber") as CWC.TextBox; } }
        protected CWC.TextBox _txtSelectionReceivingLotId { get { return Page.FindCamstarControl("Selection_ReceivingLotId") as CWC.TextBox; } }
        protected CWC.TextBox _txtSelectionDateCode { get { return Page.FindCamstarControl("Selection_DateCode") as CWC.TextBox; } }
        protected JQDataGrid _gridSeletionGrid { get { return Page.FindCamstarControl("SelectionGrid") as JQDataGrid; } }

        protected CWC.RevisionedObject _rdoSpec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoProduct { get { return Page.FindCamstarControl("ObjectChanges_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoProcessSpec { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject _ndoWorkOrder { get { return Page.FindCamstarControl("ObjectChanges_scsWorkOrder") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoOwner { get { return Page.FindCamstarControl("ObjectChanges_Owner") as CWC.NamedObject; } }
        protected CWC.TextBox _txtWaferRunNumber { get { return Page.FindCamstarControl("ObjectChanges_WaferRunNumber") as CWC.TextBox; } }
        protected CWC.TextBox _txtReceivingLotId { get { return Page.FindCamstarControl("ObjectChanges_ReceivingLotId") as CWC.TextBox; } }
        protected CWC.TextBox _txtDateCode { get { return Page.FindCamstarControl("ObjectChanges_DateCode") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoHoldReason { get { return Page.FindCamstarControl("ObjectChanges_HoldReason") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoHoldLocation { get { return Page.FindCamstarControl("ObjectChanges_HoldLocation") as CWC.NamedObject; } }
        protected CWC.TextBox _txtExpectedHoldDays { get { return Page.FindCamstarControl("ObjectChanges_ExpectedHoldDays") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoEmailGroup { get { return Page.FindCamstarControl("ObjectChanges_EmailGroup") as CWC.NamedObject; } }
        protected CWC.CheckBox _chkIncludeChildLots { get { return Page.FindCamstarControl("ObjectChanges_IncludeChildLots") as CWC.CheckBox; } }
        protected CWC.TextBox _txtComments { get { return Page.FindCamstarControl("ObjectChanges_Comments") as CWC.TextBox; } }
        protected CWC.WorkflowNavigator _wfSelectionWIPStepWf { get { return Page.FindCamstarControl("Selection_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }
        protected CWC.WorkflowNavigator _wfWIPStepWf { get { return Page.FindCamstarControl("ObjectChanges_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
            _SelectionControls.Add(_rdoSelectionSpec);
            _SelectionControls.Add(_rdoSelectionProduct);
            _SelectionControls.Add(_rdoSelectionProcessSpec);
            _SelectionControls.Add(_ndoSelectionWorkOrder);
            _SelectionControls.Add(_ndoSelectionOwner);
            _SelectionControls.Add(_txtSelectionWaferRunNumber);
            _SelectionControls.Add(_txtSelectionReceivingLotId);
            _SelectionControls.Add(_txtSelectionDateCode);
            _SelectionControls.Add(_wfSelectionWIPStepWf);

            // add the criteria WP controls to the control collection
            _CriteriaControls.Add(_rdoSpec);
            _CriteriaControls.Add(_rdoProduct);
            _CriteriaControls.Add(_rdoProcessSpec);
            _CriteriaControls.Add(_ndoWorkOrder);
            _CriteriaControls.Add(_ndoOwner);
            _CriteriaControls.Add(_txtWaferRunNumber);
            _CriteriaControls.Add(_txtReceivingLotId);
            _CriteriaControls.Add(_txtDateCode);
            _CriteriaControls.Add(_wfWIPStepWf);

            // specify the hidden columns for the selection grid.
            _HiddenGridColumns.Add("RN");

            _CriteriaWorkflowNavigators.Add(_wfWIPStepWf);

            _wfWIPStepWf.DataChanged += _wfWIPStepWf_DataChanged;
			SEMI.AppCode.UIUtility.DisableMatrixFields(this, Page.PortalContext.DataContract.GetValueByName<string>("PopupDM"), _CriteriaControls);																																														
        }

        void _wfWIPStepWf_DataChanged(object sender, EventArgs e)
        {
            SS_SetupB_StackData_Update("ObjectChanges_WIPStepWfNavigator");
        }

    }
}