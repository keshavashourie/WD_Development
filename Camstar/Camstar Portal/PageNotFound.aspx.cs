// Copyright Siemens 2023  
using System;
using System.Web.Optimization;
using System.Web.UI;

public partial class PageNotFound : Page
{
    public string styleSheetString { get; set; }
    public string currentTheme { get; set; }
    protected override void OnInit(EventArgs e)
    {
        // build styles
        string currentTheme = "camstar";
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
