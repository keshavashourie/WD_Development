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
using Camstar.Telemetry;

/// <summary>
/// Summary description for FloatAJAXMasterPage
/// </summary>
namespace Camstar.Portal
{
    public partial class AJAXChildMasterPage : System.Web.UI.MasterPage
    {
        public string styleSheetString { get; set; }
        public string currentTheme { get; set; }
        public string pagePanelClass { get; set; }
        public bool enableTelemetry { get; set; }
        public string telemetryConnectionString { get; set; }
        protected override void OnInit(EventArgs e)
        {
            if (!Request.Browser.Type.Contains("IE") && !Request.Browser.Type.Contains("InternetExplorer"))
            {
                ScriptManager.Scripts.Add(new ScriptReference("~/Scripts/jquery/bootstrap/bootstrap.bundle.min.js"));
                ScriptManager.Scripts.Add(new ScriptReference("~/Scripts/Barcode/distributables/dynamsoft-barcode-reader-bundle@10.4.3100/dist/dbr.bundle.js"));
                ScriptManager.Scripts.Add(new ScriptReference("~/Scripts/Barcode/mobileBarcode.js"));
            }

            base.OnInit(e);
            
            // build styles

            styleSheetString = "<link href=\"assets/image/sie-logo-favicon.ico\" rel=\"SHORTCUT ICON\" />";

            var isResponsive = !string.IsNullOrEmpty(Request.QueryString["responsive"]);
            pagePanelClass = !string.IsNullOrEmpty(Request.QueryString["pagePanelOf"] as string) ? "page-panel" : "";

            currentTheme = Session["CurrentTheme"].ToString() ?? "camstar";
            if (!isResponsive)
                styleSheetString += Styles.Render(
                        string.Format("~/themes/{0}/AJAXChildMaster", currentTheme),
                        string.Format("~/themes/{0}/workspaceoverride", currentTheme),
                        string.Format("~/themes/{0}/UserAll", currentTheme),
                        string.Format("~/themes/{0}/jstree/default/jstreeCSS", currentTheme)
                    ).ToHtmlString();
            else
            {
                if (currentTheme == "camstar")
                {
                    styleSheetString += Styles.Render(
                       string.Format("~/themes/{0}/AJAXChildMaster", currentTheme),
                       string.Format("~/themes/{0}/mobile/MobileCss", currentTheme)
                    ).ToHtmlString();
                }
                else
                {
                    styleSheetString += Styles.Render(
                       string.Format("~/themes/{0}/AJAXChildMaster", currentTheme),
                       string.Format("~/themes/{0}/bootstrapCss", currentTheme)
                    ).ToHtmlString();
                }

                styleSheetString += Styles.Render(
                      string.Format("~/themes/{0}/workspaceoverride", currentTheme),
                      string.Format("~/themes/{0}/UserAll", currentTheme),
                      string.Format("~/themes/{0}/jstree/default/jstreeCSS", currentTheme)
                 ).ToHtmlString();

            }
                

            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
            if (labelCache != null)
                Page.Title = labelCache.GetLabelByName("UIVirtualPage_FloatPageTitle").Value;
            WebPartPageBase page = this.AJAXContentPlaceHolder.Page as WebPartPageBase;
            if (page != null) 
                page.RegisteringDescriptors += new EventHandler<ScriptDescriptorEventArgs>(page_RegisteringDescriptors);
            
            telemetryConnectionString = TelemetryOperation.ConnectionString;
            enableTelemetry = TelemetryOperation.IsTelemetryEnabled;
        }

        void page_RegisteringDescriptors(object sender, ScriptDescriptorEventArgs e)
        {
            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
            Camstar.WCF.ObjectStack.Label label = new Camstar.WCF.ObjectStack.Label();

            if (labelCache != null)
            {
                YesLabel.Text = labelCache.GetLabelByName("Web_Yes").Value;
                NoLabel.Text = labelCache.GetLabelByName("Web_No").Value;
                OkLabel.Text = labelCache.GetLabelByName("OKButton").Value;
                MessageTitleLabel.Text = labelCache.GetLabelByName("ConfirmationMessageTitle").Value;
                CloseLabel.Text = labelCache.GetLabelByName("Web_Close").Value;
            }

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

                scd.AddProperty("pageType", "ajax-child-master");
            }
        }

    }
}
