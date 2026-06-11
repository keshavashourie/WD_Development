// Copyright Siemens 2024
using System;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.Utilities;

/// <summary>
/// Launching page for PortalStudio access
/// </summary>
public partial class PortalStudio : System.Web.UI.Page
{
    /// <summary>
    /// Validate user has the Role to access Portal Studio. This is in the case a logged in user tries 
    /// to manually access PortalStudio by typing in the url so they cannot bypass the role check.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnLoad(EventArgs e)
    {
        isSSO = AuthManager.DefaultInstance.IsSSO;
        base.OnLoad(e);

        if (Page != null && Page.Session != null)
        {
            if (!FrameworkManagerUtil.PortalStudioAccess(Page.Session))
            {
                Response.Write(LabelConstantsAux.PortalStudioAccessDenied);
                Response.End();
            }
        }

        var entryPoint = Page.Session[SessionConstants.EntryPoint] as string;
        portalMode = entryPoint ?? "Classic";
    }
    protected string portalMode;
    protected bool isSSO;
}
