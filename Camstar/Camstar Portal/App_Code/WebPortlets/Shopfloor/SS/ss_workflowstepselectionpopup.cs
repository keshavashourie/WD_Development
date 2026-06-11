/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Reflection;
using System.Reflection.Emit;
using System.ComponentModel;

using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using PERS = Camstar.WebPortal.Personalization;
using CamstarPortal.WebControls;
using Camstar.WCF.Services;
using System.Collections;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    /// Summary description for SS_WorkflowStepSelectionPopup
    /// </summary>
    public class WorkflowStepSelectionPopup : MatrixWebPart
    {
        JQDataGrid _gridSpecsSelectionField { get { return Page.FindCamstarControl("SpecsSelection") as JQDataGrid; } }
		JQDataGrid _gridStepsSelectionField { get { return Page.FindCamstarControl("StepsSelection") as JQDataGrid; } }
        CWC.TextBox _txtKeyField { get { return Page.FindCamstarControl("Key") as CWC.TextBox; } }
        CWC.TextBox _txtCaptionField { get { return Page.FindCamstarControl("Caption") as CWC.TextBox; } }
        CWC.TextBox _txtValueField { get { return Page.FindCamstarControl("Value") as CWC.TextBox; } }
        SEMI.AppCode.DataEnvelopControl _envWIPDataValidValuesList { get { return Page.FindCamstarControl("WIPDataValidValuesList") as SEMI.AppCode.DataEnvelopControl; } }
		CWC.WorkflowNavigator _workflowNavigator { get { return Page.FindCamstarControl("WIPStepWfNavigator") as CWC.WorkflowNavigator; } }
			
        //---------------------------------------
        //
        //---------------------------------------
        protected override void OnLoad(EventArgs e)
        {                    
            base.OnLoad(e);

			if (Page.DataContract.GetValueByName("Popup_SSReturnedSelectedStep") != null)
			{
				_workflowNavigator.StepControl.Data = Page.DataContract.GetValueByName("Popup_SSReturnedSelectedStep").ToString();
				Page.PortalContext.DataContract.SetValueByName("Popup_SSReturnedSelectedStep", null);
			}
        } // OnLoad

    }

}



