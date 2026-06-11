// Copyright Siemens 2023  
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.DataTransfer
{

    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    public class TargetImpactWP : MatrixWebPart
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
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
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

