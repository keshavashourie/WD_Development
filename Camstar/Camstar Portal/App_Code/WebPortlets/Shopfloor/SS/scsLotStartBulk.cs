/* Copyright 2022 Siemens */
using Camstar.Util;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    public class scsLotStartBulk : scsDBLotStartSimple
    {
        #region Controls

        //use this region to make properties that reference controls on the Portal page using Page.FindCamstarControl. 
        //Example:
        //private CWC.RevisionedObject MaintenanceReqField
        //{
        //    get { return Page.FindCamstarControl("ResourceActivation_MaintenanceReq") as CWC.RevisionedObject; }
        //}
        #endregion

        #region Protected Functions

        /// <summary>
        /// TODO: Summary Description of function
        /// </summary>
        /// <param name="e"></param>
        /// 
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "displayIndividualContainer",
                "if(document.getElementById('ctl00_WebPartManager_LotWP_scsDBLotStartSimple_scsIsIndividualContainer_ctl00').checked)" +
                "{" +
                "document.getElementsByClassName('child-level')[0].style.display = 'none';" +
                "document.getElementsByClassName('child-level2')[0].style.display = 'inline-block';" +
                "}else{document.getElementsByClassName('child-level2')[0].style.display = 'none';}",
                true);
        }
        #endregion

        #region Public Functions

        #endregion

        #region Private Functions

        #endregion

        #region Constants

        #endregion

        #region Private Member Variables

        #endregion

    }

}

