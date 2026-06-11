// Copyright Siemens 2025  
using System;
using System.Web;
using System.Web.Optimization;
using System.Web.UI;
using Camstar.WebPortal.FormsFramework.Utilities;

public partial class ErrorPage : Page
{
    public string styleSheetString { get; set; }
    public string currentTheme { get; set; }
    protected void Page_Load(object sender, EventArgs e)
    {
        // Get the exception object.
        var ex = Server.GetLastError();
        var baseEx = Server.GetLastError().GetBaseException();

        // Get the label cache
        if (Page.Session != null)
        {
            var labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
            lblTitle.Text = string.Format("{0}!", labelCache.GetLabelByName("StatusMessage_Error").Value ?? "ERROR");
            lblDescription.Text = labelCache.GetLabelByName("StatusMessage_ServerError").Value ?? "An error has occurred.  Please contact your system administrator.";
            lblDetails.Text = labelCache.GetLabelByName("Lbl_ViewDetails").Value ?? "View Details";

            if (ex != null)
            {
                lblError.Text = HttpUtility.HtmlEncode(baseEx.Message); 
                lblErrorDetail.Text = baseEx.Source + baseEx.StackTrace;
            }
        }
    }
    protected override void OnInit(EventArgs e)
    {
        // build styles
        string currentTheme = Session["CurrentTheme"] != null ? Session["CurrentTheme"].ToString() : "horizon";
        if (string.IsNullOrEmpty(currentTheme))
            currentTheme = "Horizon";
        styleSheetString = "<link href=\"assets/image/sie-logo-favicon.ico\" rel=\"SHORTCUT ICON\" />";
        styleSheetString += Styles.Render(
                    string.Format("~/themes/{0}/AJAXChildMaster", currentTheme),
                    string.Format("~/themes/{0}/workspaceoverride", currentTheme),
                    string.Format("~/themes/{0}/UserAll", currentTheme),
                    string.Format("~/themes/{0}/jstree/default/jstreeCSS", currentTheme)
                ).ToHtmlString();

        base.OnInit(e);
    }

}
