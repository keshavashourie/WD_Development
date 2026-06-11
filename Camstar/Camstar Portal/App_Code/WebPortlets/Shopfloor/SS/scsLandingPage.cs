//© 2022 Siemens Product Lifecycle Management Software Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

/// <summary>
/// Summary description for scsLandingPage
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{

    public class scsLandingPage : scsShopfloorBase
    {
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "initialize", $"scsLandingPage.initialize();", true);
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/User/SCS/scsLandingPage.js");
        }
    }
}