/* Copyright 2019 Siemens */
using System;
using System.Collections;
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

/// <summary>
/// Summary description for ss_FeederContainer
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_FeederContainer: MatrixWebPart
    {        
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("ContainerFeeder_SelectionId") as CWC.TextBox; } }
		protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("FeederContainer_ComputerName") as CWC.TextBox; } }
		protected CWC.TextBox _txtComments { get { return Page.FindCamstarControl("Shopfloor_Comments") as CWC.TextBox; } }

		protected CWC.NamedObject _ndoFeeder { get { return Page.FindCamstarControl("FeederContainer_Resource") as CWC.NamedObject; } }
		protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("FeederContainer_Employee") as CWC.NamedObject; } }

		protected CWC.ContainerList _clNewContainer { get { return Page.FindCamstarControl("ss_FeederContainer_ss_NewContainer") as CWC.ContainerList; } }
		
                
        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
        } // OnLoad

    }
}



