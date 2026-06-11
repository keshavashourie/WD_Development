/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using SEMI.AppCode;

/// <summary>
/// Summary description for SS_CarrierFamilyMaint
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class SS_CarrierFamilyMaint : MatrixWebPart
    {
        #region Controls

        CWC.CheckBox SysForceToUpperField { get { return Page.FindCamstarControl("ObjectChanges_SysForceToUpper") as CWC.CheckBox; } }
        CWC.TextBox NameField { get { return Page.FindCamstarControl("NameTxt") as CWC.TextBox; } }

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

        }
        #endregion
    }
}



