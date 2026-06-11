/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using Camstar.WebPortal.WebPortlets;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.Services;
using System.Web;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.WebControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

/// <summary>
/// Summary description for SS_SystemInfo
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_SystemInfo : MatrixWebPart
    {
        protected CWC.Label _lblVersion { get { return Page.FindCamstarControl("VersionLabel") as CWC.Label; } }
        protected override void OnLoad(EventArgs e)
        {
            string sVersion = System.Web.Configuration.WebConfigurationManager.AppSettings["SSPortalVersion"].ToString();
            _lblVersion.LabelText = sVersion;
        }
    }
}



