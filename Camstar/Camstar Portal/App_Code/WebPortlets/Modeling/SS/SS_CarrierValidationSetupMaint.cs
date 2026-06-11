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
/// Summary description for SS_PackingQtyMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_CarrierValidationSetupMaint : SS_SetupBModelingBase
    {
        protected CWC.RevisionedObject _rdoSelectionSpec { get { return Page.FindCamstarControl("Selection_Spec") as CWC.RevisionedObject; } }
		protected CWC.DropDownList _ddlSelectionWIPMainTxn { get { return Page.FindCamstarControl("Selection_WIPMainTxn") as CWC.DropDownList; } }
		protected CWC.WorkflowNavigator Selection_WIPStepWfNavigator { get { return Page.FindCamstarControl("Selection_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }

        protected CWC.RevisionedObject _rdoSpec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
        protected CWC.DropDownList _ddlWIPMainTxn { get { return Page.FindCamstarControl("ObjectChanges_WIPMainTxn") as CWC.DropDownList; } }
		protected CWC.WorkflowNavigator ObjectChanges_WIPStepWfNavigator { get { return Page.FindCamstarControl("ObjectChanges_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _SelectionControls.Add(_rdoSelectionSpec);
            _SelectionControls.Add(_ddlSelectionWIPMainTxn);
			_SelectionControls.Add(Selection_WIPStepWfNavigator);

            _CriteriaControls.Add(_rdoSpec);
            _CriteriaControls.Add(_ddlWIPMainTxn);
			_CriteriaControls.Add(ObjectChanges_WIPStepWfNavigator);
           
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



