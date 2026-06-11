/* Copyright 2020 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    /// Summary description for scsWorkflowStepSelectionPopup
    /// </summary>
    public class scsWorkflowStepSelectionPopup : MatrixWebPart
    {

        CWC.WorkflowNavigator _workflowNavigator { get { return Page.FindCamstarControl("WIPStepWfNavigator") as CWC.WorkflowNavigator; } }

        //---------------------------------------
        //
        //---------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            if (Page.DataContract.GetValueByName("Popup_PrimaryService") != null)
                Page.PrimaryServiceType = Page.DataContract.GetValueByName("Popup_PrimaryService").ToString();
            if (Page.DataContract.GetValueByName("Popup_FieldExpressionsDM") != null)
                _workflowNavigator.FieldExpressions = Page.DataContract.GetValueByName("Popup_FieldExpressionsDM").ToString();
            if (Page.DataContract.GetValueByName("Popup_ToStepValuesExpressionsDM") != null)
            {
                _workflowNavigator.ToStepValuesExpressions = Page.DataContract.GetValueByName("Popup_ToStepValuesExpressionsDM").ToString();
                _workflowNavigator.StepControl.FieldExpressions = Page.DataContract.GetValueByName("Popup_ToStepValuesExpressionsDM").ToString();
            }
            if (Page.DataContract.GetValueByName("Popup_ToWorkflowStackExpressionsDM") != null)
                _workflowNavigator.StackFieldExpressions = Page.DataContract.GetValueByName("Popup_ToWorkflowStackExpressionsDM").ToString();

            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(base.Page.Session);
            if (labelCache != null)
            {
                if (Page.DataContract.GetValueByName("Popup_WorkflowLabelName") != null)
                    _workflowNavigator.LabelControl.Text = labelCache.GetLabelByName(Page.DataContract.GetValueByName("Popup_WorkflowLabelName").ToString()).Value;
                else
                    _workflowNavigator.LabelControl.Text = labelCache.GetLabelByName("ss_WebUI_WIPStepWorkflow").Value;
                if (Page.DataContract.GetValueByName("Popup_StepLabelName") != null)
                    _workflowNavigator.StepControl.LabelControl.Text = labelCache.GetLabelByName(Page.DataContract.GetValueByName("Popup_StepLabelName").ToString()).Value;
                else
                    _workflowNavigator.StepControl.LabelControl.Text = labelCache.GetLabelByName("ss_WebUI_WIPStep").Value;
            }

        } // OnLoad

        // WebPartCustomAction  
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            Page.CollectDataContract();
            var stackControl = _workflowNavigator.FindControl(_workflowNavigator.ClientID + "_Stack") as FieldControl;
            NamedSubentityRef[] stack = (stackControl.Data) as NamedSubentityRef[];
            Page.DataContract.SetValueByName("Popup_SelectedStackDM", stack);
            Page.DataContract.SetValueByName("Popup_SelectedStepDM", _workflowNavigator.StepControl.Data);
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Personalization.CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "Close":
                        {
                            Page.CloseFloatingFrame(false);
                            break;
                        }
                    case "OK":
                        {
                            
                            Page.CloseFloatingFrameOnSubmit(new ResultStatus());
                            break;
                        }
                }
            }
        } // WebPartCustomAction  
        

    }
}