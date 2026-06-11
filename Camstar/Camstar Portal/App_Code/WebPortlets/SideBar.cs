/* Copyright 2023 Siemens */
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework.HtmlControls;
using Camstar.WebPortal.Helpers;
using Camstar.WebPortal.PortalFramework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Camstar.WebPortal.WebPortlets
{
    [PortalStudio("Side Bar")]
    public class SideBar : WebControl, IScriptControl, ICallbackEventHandler
    {

        public SideBar()
        {
            ID = "SideBar";
        }

        public CommandBarSides Side { get; set; }

        public bool IsResponsive
        {
            get
            {
                return (Page as WebPartPageBase).IsResponsive;

            }
            set { }
        }

        public bool IsApollo
        {
            get
            {
                return Page.Session[SessionConstants.EntryPoint] != null && Page.Session[SessionConstants.EntryPoint].ToString() == SessionConstants.Apollo;
            }
        }


        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            CssClass = "cs-command-sidebar cs-side-" + Side.ToString().ToLower();
            if (IsResponsive || IsApollo)
                CssClass += " cs-responsive";
            _bar.AddCssClass("cs-sidebar");
            _contentPanel.AddCssClass("cs-sidebar-panel");
            _contentPanel.AddCssClass("ui-widget-content");
            _contentPanel.AddCssClass("ui-corner-all");
            Controls.Add(_bar);

            Controls.Add(_contentPanel);
            _container.AddCssClass("sidebar-panel-container");
            var pnlHtml = "<div class='header'>";
            pnlHtml += "<span class='title'></span>";
            pnlHtml += "<span class='close-button desktop'><img class='item-icon' src='Themes/Horizon/images/close-panel.svg' /></span>";
            pnlHtml += "</div><div class='content'></div>";
            _container.InnerHtml = pnlHtml;
            _contentPanel.Controls.Add(_container);
        }

        protected override HtmlTextWriterTag TagKey
        {
            get { return HtmlTextWriterTag.Div; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Visible = (Page as WebPartPageBase).IsSideCommandBar;
        }

        protected override void OnPreRender(EventArgs e)
        {

            if (!DesignMode)
            {
                // Test for ScriptManager and register if it exists
                sm = ScriptManager.GetCurrent(Page);

                if (sm == null)
                    throw new System.Web.HttpException("A ScriptManager control must exist on the current page.");

                sm.RegisterScriptControl(this);

                ClientScriptManager cm = Page.ClientScript;
                var cbReference = cm.GetCallbackEventReference(this, "arg", "$find('" + ClientID + "').setCallbackData", ClientID);
                var callbackScript = "function CallServer(arg, context) {" + cbReference + "; }";
                cm.RegisterClientScriptBlock(this.GetType(), "CallServer", callbackScript, true);

            }
            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!DesignMode)
                sm.RegisterScriptDescriptors(this);
            base.Render(writer);
        }

        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/ClientFramework/Camstar.WebPortal.WebPortlets/SideBar.js");
        }

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            var descriptor = new ScriptControlDescriptor("Camstar.WebPortal.WebPortlets.SideBar", ClientID);
            descriptor.AddProperty("controlId", ID);
            yield return descriptor;
        }

        public void RaiseCallbackEvent(string eventArgument)
        {
            if (!string.IsNullOrEmpty(eventArgument))
                _callbackArgs = JsonConvert.DeserializeObject<CommandBarCallBackArgs>(eventArgument);
        }

        private CommandBarHelper GetServerObject()
        {
            if (_callbackArgs.serverType != null)
            {
                var t = Type.GetType(_callbackArgs.serverType);
                if (t != null)
                    return Activator.CreateInstance(t) as CommandBarHelper;
            }

            return null;
        }

        public string GetCallbackResult()
        {
            var ret = "";

            if (_callbackArgs != null && !string.IsNullOrEmpty(_callbackArgs.serverType))
            {
                var obj = GetServerObject();
                if (obj != null)
                {
                    var pdata = obj.GetPanelData(_callbackArgs);
                    if (pdata != null)
                        return JsonConvert.SerializeObject(pdata);
                }
            }
            return ret;
        }

        private DivElement _bar = new DivElement { ID = "sidebar" };
        private DivElement _contentPanel = new DivElement { ID = "sidebarPanel" };
        private DivElement _container = new DivElement();
        private CommandBarCallBackArgs _callbackArgs;
        private ScriptManager sm;
    }

    public enum CommandBarSides { Unset, Left, Top, Right, Bottom }

    public class CommandBarCallBackArgs
    {
        public string serverType { get; set; }
        public string clientType { get; set; }
        public string fun { get; set; }
        public string containerName { get; set; }
        public string filter { get; set; }
        public string documentName { get; set; }
        public string documentRev { get; set; }
        public string tasklistName { get; set; }
        public string taskName { get; set; }
        public string eProcedure { get; set; }
        public string tasklistId { get; set; }
        public string tasklistRevision { get; set; }
        /// <summary>
        /// Untyped information that can be used by overriding controls
        /// </summary>
        public string tag { get; set; }

    }

}
