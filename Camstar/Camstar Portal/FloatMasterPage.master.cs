// Copyright Siemens 2023  
using System;
using System.Web;
using System.Web.UI;
using System.Data;

using Camstar.WebPortal.Constants;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.PortalFramework;
using System.Web.Optimization;

/// <summary>
/// Summary description for FloatMasterPage
/// </summary>
namespace Camstar.Portal
{
    public partial class FloatMasterPage : System.Web.UI.MasterPage
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
                        string.Format("~/themes/{0}/workspaceoverride", currentTheme),
                        string.Format("~/themes/{0}/UserAll", currentTheme)
                    ).ToHtmlString();

            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
            if (labelCache != null)
                Page.Title = labelCache.GetLabelByName("UIVirtualPage_FloatPageTitle").Value;
            WebPartPageBase page = this.AJAXContentPlaceHolder.Page as WebPartPageBase;
            if (page != null)
                page.RegisteringDescriptors += new EventHandler<ScriptDescriptorEventArgs>(page_RegisteringDescriptors);
        }

        void page_RegisteringDescriptors(object sender, ScriptDescriptorEventArgs e)
        {
            ScriptComponentDescriptor scd = e.Descriptor as ScriptComponentDescriptor;
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
}
