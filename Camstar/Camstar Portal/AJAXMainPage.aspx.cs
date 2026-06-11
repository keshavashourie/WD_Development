// Copyright Siemens 2023  

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace Camstar.Portal
{
    /// <summary>
    /// The main application page
    /// </summary>
    partial class AJAXMainPage : Camstar.WebPortal.PortalFramework.WebPartPageBase
    {
        public override bool PageAuthorizationCheck()
        {
            return true;
        }
    } // AJAXMainPage
}
