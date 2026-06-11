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

/// <summary>
/// Summary description for SS_Shopfloor
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_Shopfloor: MatrixWebPart 
    {
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("Shopfloor_ComputerName") as CWC.TextBox; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (_txtComputerName != null)
                _txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
        }
    }

}



