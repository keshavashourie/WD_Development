/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.Script.Serialization;

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
using SEMI.AppCode;
using SWC = System.Web.UI.WebControls;
using System.Collections;

/// <summary>
/// Summary description for scsMapDataDisplayFullView
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class scsMapDataDisplayFullView : MatrixWebPart
    {
        //-----------------------------------------
        // Override OnLoad event
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (!this.Page.IsPostBack)
                {
                    ScriptManager.RegisterStartupScript(this.Page.Form, this.Page.GetType(), "RenderFullViewMapData", "RenderMapDataInFullViewCanvas();", true);
                }
            }
            catch (Exception ex) //Catch errors
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }
    }
}



