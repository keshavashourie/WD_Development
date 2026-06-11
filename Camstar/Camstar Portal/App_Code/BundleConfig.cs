// Copyright Siemens 2024

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.UI;
using System.IO;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.Utilities;

namespace Camstar.Portal
{
    /// <summary>
    /// Bundles and minifies javascript files for performance improvements
    /// **NOTE: There is a compilation directive below to disable bundling in DEBUG mode
    /// Also there is a debug setting under compilation in the web config that 
    /// should be set to "false" for Release/Production code and "true" for development/debugging
    /// </summary>
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // bundle jquery and jquery ui
            bool isSSO = AuthManager.DefaultInstance.IsSSO;
            
                var jqBundle = new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery/jquery.min.js",
                "~/Scripts/jquery/jquery.migrate.js",
                "~/Scripts/jquery/jquery-ui.min.js");
            jqBundle.Orderer = new NonOrderingBundleOrderer();
            bundles.Add(jqBundle);

            var bootstrapBundle = new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/jquery/bootstrap/bootstrap.bundle.min.js");
            bootstrapBundle.Orderer = new NonOrderingBundleOrderer();

            bootstrapBundle.Transforms.Clear();
            bundles.Add(bootstrapBundle);

            var mobileBundle = new ScriptBundle("~/bundles/mobileControls").Include(
                "~/Scripts/ClientFramework/CamstarPortal.WebControls/HeaderMobileControl.js");
            mobileBundle.Orderer = new NonOrderingBundleOrderer();
            bundles.Add(mobileBundle);

            // bundle specific for login page
            var loginBundle = new ScriptBundle("~/bundles/login").Include(
                "~/Scripts/jquery/jquery.min.js",
                "~/Scripts/jquery/jquery-ui.min.js",
                "~/Scripts/ClientTimeZoneMap.js",
                "~/Scripts/login.js");
            loginBundle.Orderer = new NonOrderingBundleOrderer();
            bundles.Add(loginBundle);

            bundles.Add(new ScriptBundle("~/bundles/camstar").Include(
                "~/Scripts/jquery/jquery.alerts.js",
                "~/Scripts/jquery/jquery.camstar.js",
                "~/Scripts/ClientFramework/MicrosoftAjaxExt.js",
                "~/Scripts/ClientFramework/Camstar.Ajax/CamstarAjax.js",
                "~/Scripts/ClientFramework/Camstar.UI/Control.js",
                "~/Scripts/ClientFramework/CamstarPortal.WebControls/Modal.js",
                "~/Scripts/Camstar.js"));

            bundles.Add(new ScriptBundle("~/bundles/webcontrols").Include(
                "~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/TextBox.js",
                "~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/PickLists/PickListCommon.js",
                "~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/PickLists/PickListControl.js",
                "~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/PickLists/PickListPanel.js",
                "~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/DropDownList.js",
                "~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/NamedObject.js",
                "~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/RevisionedObject.js",
                "~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/FlyoutDropDown.js",
                "~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/Button.js",
                "~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/CheckBox.js",
                "~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/RadioButtonList.js",
                "~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/PagePanel.js",
				"~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/TileContainer.js",
                "~/Scripts/ClientFramework/CamstarPortal.WebControls/TabContainer.js",
                "~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/Breadcrumb.js"));

            bundles.Add(new ScriptBundle("~/bundles/scripts").Include(
                "~/Scripts/FloatingFrame.js",
                "~/Scripts/WebParts.js",
                "~/Scripts/Controls.js",
                "~/Scripts/ClientScripts.js",
                "~/Scripts/ClientFramework/CamstarPortal.WebControls/FlyoutActionMenu.js",
                //"~/Scripts/jquery/tiny_mce/tinymce.min.js",
                "~/Scripts/jquery/datepicker/jquery-datepicker.js",
                // "~/Scripts/jquery/datepicker/jquery-ui-timepicker-addon.js",
                "~/Scripts/jquery/jquery-ui-sliderAccess.js",
                "~/Scripts/ScrollableMenu.js",
                "~/Scripts/jquery/jquery.jstree.js",
                "~/Scripts/jquery/MaskedInput.js",
                "~/Scripts/jquery/jScrollPane.js",
                "~/Scripts/ClientFramework/CamstarPortal.WebControls/InquiryControl.js",
                "~/Scripts/jquery/jquery.colorpicker.js",
                "~/Scripts/jquery/jquery.fileupload.js",
                "~/Scripts/ClientFramework/CamstarPortal.WebControls/FileInput.js",
                "~/Scripts/jquery/jquery.tooltips.js",
                "~/Scripts/CRModules.js",
                "~/Scripts/CRSlideOut.js",
                "~/Scripts/CRSideBar.js",
                "~/Scripts/jquery/jquery.iframe-transport.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqGrid").Include(
                "~/Scripts/jquery/jqGrid/grid.locale-en.js",
                "~/Scripts/jquery/jqGrid/jquery.jqGrid.min.js",
                "~/Scripts/ClientFramework/CamstarPortal.WebControls/JQGridBaseData.js",
                "~/Scripts/ClientFramework/CamstarPortal.WebControls/JQGridHelper.js"));
                bundles.Add(new ScriptBundle("~/bundles/session").Include(
                "~/Scripts/SessionTimeout.js"));            
           

            // Workspace Bundle
            var wsScriptBundle = new ScriptBundle("~/bundles/workspaceoverride").IncludeDirectory("~/Scripts/User",
                "*.js", true);
            wsScriptBundle.Orderer = new NumericBundleOrderer();
            bundles.Add(wsScriptBundle);

            ScriptManager.ScriptResourceMapping.AddDefinition("jquery", new ScriptResourceDefinition() { Path = "~/bundles/jquery", });
            ScriptManager.ScriptResourceMapping.AddDefinition("bootstrap", new ScriptResourceDefinition() { Path = "~/bundles/bootstrap" });
            ScriptManager.ScriptResourceMapping.AddDefinition("mobileControls", new ScriptResourceDefinition() { Path = "~/bundles/mobileControls" });
            ScriptManager.ScriptResourceMapping.AddDefinition("camstar", new ScriptResourceDefinition() { Path = "~/bundles/camstar" });
            ScriptManager.ScriptResourceMapping.AddDefinition("camstar.webcontrols", new ScriptResourceDefinition() { Path = "~/bundles/webcontrols" });
            ScriptManager.ScriptResourceMapping.AddDefinition("camstar.scripts", new ScriptResourceDefinition() { Path = "~/bundles/scripts" });
            ScriptManager.ScriptResourceMapping.AddDefinition("camstar.webcontrols.jqGrid", new ScriptResourceDefinition() { Path = "~/bundles/jqGrid" });
            ScriptManager.ScriptResourceMapping.AddDefinition("session", new ScriptResourceDefinition() { Path = "~/bundles/session" });
            ScriptManager.ScriptResourceMapping.AddDefinition("workspaceoverride", new ScriptResourceDefinition() { Path = "~/bundles/workspaceoverride" });

            //****** Set up CSS Bundling ********

            //Login CSS
            bundles.Add(new StyleBundle("~/themes/camstar/login").Include(
                "~/themes/camstar/camstar.helpers.min.css",
                "~/themes/camstar/camstar.common.min.css",
                "~/themes/camstar/camstar.gradients.min.css",
                "~/themes/horizon/camstar.webparts.min.css",
                "~/themes/camstar/camstar.ui.login.min.css"));
            //Login CSS
            bundles.Add(new StyleBundle("~/themes/horizon/login").Include(
                "~/themes/horizon/camstar.helpers.min.css",
                "~/themes/horizon/camstar.common.min.css",
                "~/themes/horizon/camstar.gradients.min.css",
                "~/themes/horizon/camstar.webparts.min.css",
                "~/themes/horizon/camstar.ui.login.min.css"));

            //Line Assignment CSS
            bundles.Add(new StyleBundle("~/themes/camstar/lineassignment").Include(
                "~/themes/camstar/camstar.gradients.min.css",
                "~/themes/camstar/camstar.common.min.css",
                "~/themes/camstar/camstar.fields.min.css",
                "~/themes/Camstar/camstar.controls.picklist.min.css",
                "~/themes/camstar/camstar.webparts.min.css",
                "~/themes/camstar/camstar.ui.login.min.css",
                "~/themes/camstar/jstree/default/style.css"));

            bundles.Add(new StyleBundle("~/themes/camstar/jstree/default/jstreeCSS").Include(
                "~/themes/camstar/jstree/default/style.css"));

            bundles.Add(new StyleBundle("~/themes/camstar/mobile/MobileCss").Include(
                "~/themes/camstar/mobile/bootstrap.min.css",
                "~/themes/camstar/mobile/bootstrap-grid.min.css",
                "~/themes/camstar/mobile/camstar.controls.mobile.min.css",
                "~/themes/camstar/mobile/camstar.webparts.mobile.min.css",
                "~/themes/camstar/mobile/camstar.common.mobile.min.css",
                "~/themes/camstar/mobile/camstar.datacollection.mobile.min.css"
                ));

            bundles.Add(new StyleBundle("~/themes/camstar/AJAXChildMaster").Include(
                "~/themes/camstar/camstar.gradients.min.css",
                "~/themes/camstar/camstar.common.min.css",
                "~/themes/camstar/camstar.fields.min.css",
                "~/themes/camstar/camstar.datagrid.min.css",
                "~/themes/camstar/camstar.controls.min.css",
                "~/themes/camstar/camstar.controls.picklist.min.css",
                "~/themes/camstar/camstar.webparts.min.css",
                "~/themes/camstar/camstar.webparts.min.css",
                "~/themes/camstar/camstar.pages.min.css",
                "~/themes/camstar/camstar.datacollection.min.css",
                "~/themes/camstar/camstar.helpers.min.css",
                "~/themes/camstar/camstar.modeling.min.css",
                "~/themes/camstar/jquery-ui-timepicker-addon.min.css",
                 "~/themes/Camstar/jquery.colorpicker.min.css",
                "~/themes/camstar/nv.d3.min.css"
                
                ));

            //****** Set up CSS Bundling - Horizon ********

            bundles.Add(new StyleBundle("~/themes/horizon/bootstrapCss").Include(
                "~/themes/horizon/bootstrap.min.css",
                "~/themes/horizon/bootstrap-grid.min.css"
                ));

            bundles.Add(new StyleBundle("~/themes/horizon/jstree/default/jstreeCSS").Include(
                "~/themes/horizon/jstree/default/style.css"));

            bundles.Add(new StyleBundle("~/themes/horizon/AJAXChildMaster").Include(
				"~/themes/horizon/camstar.gradients.min.css",
                "~/themes/horizon/camstar.common.min.css",
                "~/themes/horizon/camstar.fields.min.css",
                "~/themes/horizon/camstar.revisioned-object.min.css",
                "~/themes/horizon/camstar.textbox.min.css",
                "~/themes/horizon/camstar.textarea.min.css",
                "~/themes/horizon/camstar.datepicker.min.css",
                "~/themes/horizon/camstar.datagrid.min.css",
                "~/themes/horizon/camstar.controls.min.css",
                "~/themes/horizon/camstar.controls.picklist.min.css",
                "~/themes/horizon/camstar.helpers.min.css",
                "~/themes/horizon/camstar.webparts.min.css",
                "~/themes/horizon/camstar.webparts.min.css",
                "~/themes/horizon/camstar.pages.min.css",
                "~/themes/horizon/camstar.datacollection.min.css",
                "~/themes/horizon/camstar.modeling.min.css",
                "~/themes/horizon/jquery-ui-timepicker-addon.min.css",
                "~/themes/horizon/jquery.colorpicker.min.css",
                "~/themes/horizon/nv.d3.min.css",
                "~/themes/horizon/camstar.boolean-switch.min.css",
                "~/themes/horizon/camstar.checkbox.min.css",
                "~/themes/horizon/camstar.radiobutton.min.css",
                "~/themes/horizon/camstar.radiobuttonlist.min.css",
                "~/themes/horizon/camstar.button.min.css",
                "~/themes/horizon/camstar.dialog.min.css",
                "~/themes/horizon/camstar.tabs-nav.min.css",
                "~/themes/horizon/camstar.workflow-navigator.min.css",
                "~/themes/horizon/camstar.controls.tree.min.css",
                "~/themes/horizon/camstar.controls.pick-grid.min.css",
                "~/themes/horizon/camstar.breadcrumb.min.css"
                ));


            // User folder for specific themes
            bundles.Add(new StyleBundle("~/themes/camstar/UserAll")
                .IncludeDirectory("~/themes/camstar/User", "*.css", true)
                .IncludeDirectory("~/themes/User", "*.css", true)); //camstar
            bundles.Add(new StyleBundle("~/themes/horizon/UserAll")
                .IncludeDirectory("~/themes/horizon/User", "*.css", true)
                .IncludeDirectory("~/themes/User", "*.css", true)); //horizon

            // Workspace Bundle - Camstar
            var wsStyleBundle = new StyleBundle("~/themes/camstar/workspaceoverride").IncludeDirectory("~/themes/User",
                "*.css", true);
            wsStyleBundle.Orderer = new NumericBundleOrderer();
            bundles.Add(wsStyleBundle);

            // Workspace Bundle - horizon
            var wsStyleBundleHorizon = new StyleBundle("~/themes/horizon/workspaceoverride").IncludeDirectory("~/themes/User",
                "*.css", true);
            wsStyleBundleHorizon.Orderer = new NumericBundleOrderer();
            bundles.Add(wsStyleBundleHorizon);

            // Clear all items from the default ignore list to allow minified CSS and JavaScript files to be included in debug mode
            bundles.IgnoreList.Clear();

            // Add back the default ignore list rules sans the ones which affect minified files and debug mode
            bundles.IgnoreList.Ignore("*.intellisense.js");
            bundles.IgnoreList.Ignore("*-vsdoc.js");
            bundles.IgnoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);

            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862

            // If the compilation node in web.config indicates debugging mode is enabled
            // then clear all transforms. I.e. disable Js and CSS minification.
            if (HttpContext.Current.IsDebuggingEnabled)
            {
                BundleTable.EnableOptimizations = false;

                BundleTable.Bundles.ToList().ForEach(b => b.Transforms.Clear());
            }
            else
            {
                BundleTable.EnableOptimizations = true;
            }
        }
    }

    class NonOrderingBundleOrderer : IBundleOrderer
    {
        public virtual IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            return files;
        }
    }

    /// <summary>
    /// Put files with numeric filename in ascending order.
    /// </summary>
    class NumericBundleOrderer : IBundleOrderer
    {
        public virtual IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            var dic = new Dictionary<int, BundleFile>();
            var wsCodes = WorkspacesUtil.WorkspaceCodes;

            foreach (var file in files)
            {
                var path = file.IncludedVirtualPath;
                var fileName = Path.GetFileNameWithoutExtension(path);

                int num;
                if (int.TryParse(fileName, out num) && wsCodes.Contains("_" + fileName))
                    dic.Add(num, file);
            }

            return dic.OrderBy(item => item.Key).Select(item => item.Value);
        }
    }
}
