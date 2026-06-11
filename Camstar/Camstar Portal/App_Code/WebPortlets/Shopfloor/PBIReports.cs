// Copyright Siemens 2024 
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;

using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    /// Summary description for PBIReports
    /// </summary>
    public class PBIReports : MatrixWebPart
    {

        public PBIReports()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "initializePage", $"CR.PBIReports.initialize();", true);
			}
        }
		
        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/CRModules.js");
        }		
    }
}