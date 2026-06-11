// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.WebPortlets;
using OM = Camstar.WCF.ObjectStack;

/// <summary>
/// Summary description for TriageQualityObject
/// </summary>

namespace Camstar.WebPortal.WebPortlets
{
    public class TriageObject : MatrixWebPart
    {

        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);

            if ((bool)ChecklistAssigned.Data && ChecklistTemplate.Data != null)
            {
                Page.ActionDispatcher.PageActions().FirstOrDefault(a => a is SubmitAction).Confirmation = new Confirmation { OK_LabelName = "Web_Yes", Cancel_LabelName = "Web_No", Message_LabelName = "Lbl_ChecklistAlreadyAssignedToEvent", Title_LabelName = "StatusMessage_Warning" };
                CamstarWebControl.SetRenderToClient(submitBtn);
            }
            else
                Page.ActionDispatcher.PageActions().FirstOrDefault(a => a is SubmitAction).Confirmation = null;
        }


        protected virtual Button submitBtn
        {
            get { return Page.FindCamstarControl("Submit") as Button; }
        }

        protected virtual FormsFramework.WebControls.CheckBox ChecklistAssigned
        {
            get { return Page.FindCamstarControl("ChecklistAssigned") as FormsFramework.WebControls.CheckBox; }
        }
        protected virtual FormsFramework.WebControls.RevisionedObject ChecklistTemplate
        {
            get { return Page.FindCamstarControl("TriageQualityObject_ChecklistTemplate") as FormsFramework.WebControls.RevisionedObject; }
        }

    }
}
