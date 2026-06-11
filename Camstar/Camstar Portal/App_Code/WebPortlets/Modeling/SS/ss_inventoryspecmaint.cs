/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using SEMI.AppCode;

/// <summary>
/// Summary description for SS_InventorySpecMaint
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class SS_InventorySpecMaint : MatrixWebPart
    {
        #region Controls
        CWC.CheckBox RemoveDateCodeField { get { return Page.FindCamstarControl("ObjectChanges_RemoveDateCode") as CWC.CheckBox; } }
        CWC.CheckBox SetDateCodeField { get { return Page.FindCamstarControl("ObjectChanges_SetDateCode") as CWC.CheckBox; } }
        CWC.CheckBox TestIQCRequiredField { get { return Page.FindCamstarControl("ObjectChanges_TestIQCRequired") as CWC.CheckBox; } }
        CWC.CheckBox SysForceToUpperField { get { return Page.FindCamstarControl("ObjectChanges_SysForceToUpper") as CWC.CheckBox; } }
        CWC.TextBox UTAPrefixField { get { return Page.FindCamstarControl("ObjectChanges_UTAPrefix") as CWC.TextBox; } }
        CWC.TextBox NameField { get { return Page.FindCamstarControl("NameTxt") as CWC.TextBox; } }
        CWC.NamedObject DefaultMaterialWIPLocationField { get { return Page.FindCamstarControl("ObjectChanges_DefaultMaterialWIPLocation") as CWC.NamedObject; } }
        CWC.NamedObject DispatchPlanField { get { return Page.FindCamstarControl("ObjectChanges_DispatchPlan") as CWC.NamedObject; } }

        #endregion


        #region PageEvents

        /// <summary>
        /// On Page load
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UIUtility.SetCapital(NameField, SysForceToUpperField.CheckControl.Checked);
            if (PrimaryServiceType.IndexOf("InventorySpec") > 0)
            {
                TestIQCRequiredField.DataSubmissionMode = Personalization.DataSubmissionModeType.NotSet;
                TestIQCRequiredField.Enabled = true;
                TestIQCRequiredField.ReadOnly = false;
                SetDateCodeField.DataSubmissionMode = Personalization.DataSubmissionModeType.NotSet;
                SetDateCodeField.Enabled = true;
                SetDateCodeField.ReadOnly = false;
                RemoveDateCodeField.DataSubmissionMode = Personalization.DataSubmissionModeType.NotSet;
                RemoveDateCodeField.Enabled = true;
                RemoveDateCodeField.ReadOnly = false;
                UTAPrefixField.DataSubmissionMode = Personalization.DataSubmissionModeType.NotSet;
                UTAPrefixField.Enabled = true;
                UTAPrefixField.ReadOnly = false;
                DefaultMaterialWIPLocationField.DataSubmissionMode = Personalization.DataSubmissionModeType.NotSet;
                DefaultMaterialWIPLocationField.Enabled = true;
                DefaultMaterialWIPLocationField.ReadOnly = false;

            }
            if (PrimaryServiceType.IndexOf("ScheduledSpec") > 0)
            {
                SetDateCodeField.DataSubmissionMode = Personalization.DataSubmissionModeType.NotSet;
                SetDateCodeField.Enabled = true;
                SetDateCodeField.ReadOnly = false;
                DispatchPlanField.DataSubmissionMode = Personalization.DataSubmissionModeType.NotSet;
                DispatchPlanField.Enabled = true;
                DispatchPlanField.ReadOnly = false;
            }

        }
        #endregion
    }
}



