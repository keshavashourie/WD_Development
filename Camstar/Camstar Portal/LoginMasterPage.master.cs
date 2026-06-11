// Copyright Siemens 2023  
using System;
using System.Web.Optimization;
using System.Web.UI;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.PortalFramework;

public partial class LoginMasterPage : System.Web.UI.MasterPage
{
    public string styleSheetString { get; set; }
    public string currentTheme { get; set; }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        // build styles
        currentTheme = Session["CurrentTheme"].ToString() ?? "camstar";
        styleSheetString = "<link href=\"assets/image/sie-logo-favicon.ico\" rel=\"SHORTCUT ICON\" />";
        styleSheetString += Styles.Render(
                    string.Format("~/themes/{0}/lineassignment", currentTheme),
                    string.Format("~/themes/{0}/workspaceoverride", currentTheme),
                    string.Format("~/themes/{0}/UserAll", currentTheme)
                ).ToHtmlString();

        LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
        if (labelCache != null)
            Page.Title = labelCache.GetLabelByName("UIVirtualPage_FloatPageTitle").Value;
        var page = this.AJAXContentPlaceHolder.Page as WebPartPageBase;
        if (page != null)
            page.RegisteringDescriptors += page_RegisteringDescriptors;
    }

    void page_RegisteringDescriptors(object sender, ScriptDescriptorEventArgs e)
    {
        var scd = e.Descriptor as ScriptComponentDescriptor;
        if (scd != null)
        {
            scd.AddProperty("labels", new
            {
                YesLabel = YesLabel.Text,
                NoLabel = NoLabel.Text,
                OkLabel = OkLabel.Text,
                MessageTitle = MessageTitleLabel.Text,
                CloseLabel = CloseLabel.Text
            });

        }
    }
}
