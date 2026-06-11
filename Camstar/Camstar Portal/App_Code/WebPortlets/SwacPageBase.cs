//Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Newtonsoft.Json;

namespace Camstar.WebPortal.WebPortlets
{
    /// <summary>
    /// Holds SWAC related configuration data.
    /// </summary>
    public class SwacConfiguration
    {
        /// <summary>
        /// The name of the SwacWP web part to which the config info applies.
        /// </summary>
        public string WebPartName { get; set; }

        /// <summary>
        /// The name of the button to open SWAC popup.
        /// </summary>
        public string ButtonName { get; set; }

        /// <summary>
        /// Flag indicating if this configuration is to be used for the SWAC popup page.
        /// </summary>
        public bool UsePopup { get; set; }

        /// <summary>
        /// URL to component page on remote system
        /// </summary>
        public string ComponentUrl { get; set; }

        /// <summary>
        /// Identifies block of client script that interacts with remote component.
        /// </summary>
        public string ComponentName { get; set; }

        /// <summary>
        /// A value to use as the title of the popup page. May be a label or string literal.
        /// </summary>
        public string PopupTitle { get; set; }

        /// <summary>
        /// Help topic URL to be used for the component when showing in popup page.
        /// </summary>
        public string ComponentHelpUrl { get; set; }

        private List<string> _componentArgs;
        /// <summary>
        /// Initialization arguments to be passed to component when creating it.
        /// </summary>
        public List<string> ComponentArgs
        {
            get
            {
                if (_componentArgs == null)
                    _componentArgs = new List<string>();
                return _componentArgs;
            }
            set { _componentArgs = value; }
        }

        /// <summary>
        /// Configured height property of web part.
        /// </summary>
        public string FixedHeight { get; set; }

        /// <summary>
        /// Configured width property of wep part.
        /// </summary>
        public string FixedWidth { get; set; }

        /// <summary>
        /// Identifies if the page is a popup page. 
        /// Auto-close param from submit event of component can be used to close the page in this case.
        /// </summary>
        public bool IsPopup { get; set; }
    }

    /// <summary>
    /// Base class for any page that wants to use the SWAC popup page or host a SWAC container.
    /// </summary>
    public class SwacPageBase : MatrixWebPart
    {
        readonly string TARGET_DCM_LINK = "SwacComponentInitJson";  // fixed name for data contract member that accepts config data for the swac popup page.
        readonly string POPUP_PAGE_NAME = "SwacContainerPopupVP";   // VP name of the popup page.
        readonly string POPUP_SWAC_WP_NAME = "SwacWPForPopup";      // fixed name for the SwacWP web part in the popup page.
        readonly string POPUP_DCM_NAME = "SwacContainerInitJson";   // data contract member on the popup page that gets the swac initialization info via data contract link from calling page.

        bool isSwacPopupPage = false;
        protected virtual CWC.TextBox InitJsonTextBox { get { return Page.FindCamstarControl("InitializationJson") as CWC.TextBox; } }

        private string _swacComponentUrl = string.Empty;

        public string SwacComponentUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_swacComponentUrl))
                {
                    // get all the swac web parts added to the page
                    var wpList = Page.FindCamstarControls<MatrixWebPart>().Where(wp =>
                        wp.Model != null &&
                        wp.Model.PublishedContent != null &&
                        wp.Model.PublishedContent.DataContract != null &&
                        wp.Model.PublishedContent.DataContract.DataMembers != null &&
                        Array.Find(wp.Model.PublishedContent.DataContract.DataMembers, dm => dm.Name == "SwacComponentUrl") != null);

                    // get config settings for each web part
                    foreach (var wp in wpList)
                    {
                        if (string.IsNullOrEmpty(_swacComponentUrl))
                            _swacComponentUrl = GetDataMemberValue(wp, "SwacComponentUrl");
                        else
                            break;
                    }
                }
                return _swacComponentUrl;// GetDataMemberValue(this, "SwacComponentUrl");
            }
        }
    

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            isSwacPopupPage = Page.VirtualPageName == POPUP_PAGE_NAME;
            if (isSwacPopupPage)
                InitializePopup();
            else
                InitializeSwac(true);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            
            isSwacPopupPage = Page.VirtualPageName == POPUP_PAGE_NAME;
            if (!isSwacPopupPage)
                InitializeSwac(false);            
        }
        /// <summary>
        /// Initialization logic specific to the popup page
        /// </summary>
        protected virtual void InitializePopup()
        {
            var labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);

            // get json initialization data and convert to an object for easier referencing
            SwacConfiguration initData;
            string initJson = Page.DataContract.GetValueByName(POPUP_DCM_NAME) as string;
            try
            {
                initData = JsonConvert.DeserializeObject<SwacConfiguration>(initJson);
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError($"Page: {Page.VirtualPageName}, Error: {ex.ToString()}");
                initData = null;
            }

            if (initData != null)
            {
                initData.IsPopup = isSwacPopupPage;
                initData.WebPartName = POPUP_SWAC_WP_NAME;

                // verify URL is set. should never happen as URL must be set to get here.
                if (string.IsNullOrWhiteSpace(initData.ComponentUrl))
                {
                    Page.StatusBar.WriteError(labelCache.GetLabelByName("SWAC_ErrorNoComponentUrl").Value);
                }
                else
                {
                    // get popup title
                    // support setting as a label name or string literal and use a default if not set.
                    if (string.IsNullOrWhiteSpace(initData.PopupTitle))
                    {
                        initData.PopupTitle = labelCache.GetLabelByName("SWAC_DefaultPopupTitle").Value;
                    }
                    else
                    {
                        var label = labelCache.GetLabelByName(initData.PopupTitle);
                        if (label.Value != null)
                            initData.PopupTitle = label.Value;
                    }

                    Page.Title = initData.PopupTitle;

                    Personalization.PageContent content = Page.Model.PublishedContent as Personalization.PageContent;
                    if (content != null && !string.IsNullOrWhiteSpace(initData.ComponentHelpUrl))
                    {
                        content.HelpFileURL = initData.ComponentHelpUrl;
                    }

                    // get the initialization data to client script
                    List<SwacConfiguration> wpInitList = new List<SwacConfiguration>();
                    wpInitList.Add(initData);

                    string json = JsonConvert.SerializeObject(wpInitList.ToArray());
                    string script = $"swacConnector.initializeSwacConnections({json});";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "InitializeSwacConnections", script, true);
                }
            }
        }

        /// <summary>
        /// Initialization logic for all pages other than the popup page.
        /// </summary>
        protected virtual void InitializeSwac(bool setDC)
        {
            List<SwacConfiguration> wpInitList = new List<SwacConfiguration>();

            // get all the swac web parts added to the page
            var wpList = Page.FindCamstarControls<MatrixWebPart>().Where(wp => 
                wp.Model != null &&
                wp.Model.PublishedContent != null &&
                wp.Model.PublishedContent.DataContract != null &&
                wp.Model.PublishedContent.DataContract.DataMembers != null &&
                Array.Find(wp.Model.PublishedContent.DataContract.DataMembers, dm => dm.Name == "SwacComponentUrl") != null);

            // get config settings for each web part
            foreach (var wp in wpList)
            {
                var wpConfig = new SwacConfiguration
                {
                    ComponentUrl = GetDataMemberValue(wp, "SwacComponentUrl"),
                    ComponentName = GetDataMemberValue(wp, "SwacComponentName"),
                    UsePopup = GetUsePopup(wp),
                    PopupTitle = GetDataMemberValue(wp, "SwacPopupTitle"),
                    ComponentHelpUrl = GetDataMemberValue(wp, "SwacComponentHelpUrl"),
                    WebPartName = wp.WebPartName,
                    FixedHeight = wp.Height.Value.ToString(),
                    FixedWidth = wp.Width.Value.ToString(),
                    IsPopup = isSwacPopupPage
                };

                // there should be only one button in the WP, but can't stop users from adding another, so treat as a list.
                List<CWC.Button> buttonList = wp.CamstarControls.FindControls<CWC.Button>().Where(b => b.DefaultAction != null && b.DefaultAction.GetType() == typeof(Personalization.FloatPageOpenAction)).ToList();
                var item = !setDC ? Page.DataContract.GetValueByName("SelectedInstanceRef") as NamedObjectRef : null;
                CWC.TextBox txtName = Page.FindCamstarControl("NameTxt") as CWC.TextBox;
                // hide buttons where web part not configured to use the popup or url not set.
                if (!wpConfig.UsePopup || string.IsNullOrWhiteSpace(wpConfig.ComponentUrl) || item == null)
                {
                    buttonList.ForEach(b => b.Visible = false);
                } else
                    buttonList.ForEach(b => b.Visible = true);

                // initialize any buttons that will show the popup
                if (wpConfig.UsePopup && !string.IsNullOrWhiteSpace(wpConfig.ComponentUrl))
                {
                    foreach (var button in buttonList)
                    {
                        // enable handling the button click
                        button.Click += ShowSwacPopup_Click;

                        // set buttons data contract links to pass initialization data to the popup
                        var link = new Personalization.UIComponentDataContractLink
                        {
                            SourceMember = button.ClientID,
                            TargetMember = TARGET_DCM_LINK
                        };
                        var links = new Personalization.UIComponentDataContractLink[1];
                        links[0] = link;
                        ((Personalization.FloatPageOpenAction)button.DefaultAction).DataContractMap.Links = links;

                        // add button name to the config
                        wpConfig.ButtonName = button.ID;

                        // store config in session so can get when button clicked.
                        if (setDC)
                            Page.SessionVariables.SetValueByName(button.ClientID, wpConfig);
                    }
                }

                // prepare startup script for any swac containers embedded in this page
                if (!wpConfig.UsePopup && !string.IsNullOrWhiteSpace(wpConfig.ComponentUrl))
                {
                    wpConfig.ComponentArgs = GetSwacComponentInitializationArgs(wpConfig.WebPartName, wpConfig.ButtonName);
                    wpInitList.Add(wpConfig);
                }
            }

            // register startup script to call initialize function for embedded swac containers
            if (wpInitList.Count > 0)
            {
                string json = JsonConvert.SerializeObject(wpInitList.ToArray());
                string script = $"swacConnector.initializeSwacConnections({json});";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "InitializeSwacConnections", script, true);
            }
        }

        /// <summary>
        /// Reads the value set for UseSwacPopup property on the web part and interpret the value as true/false.
        /// </summary>
        /// <param name="wp"></param>
        /// <returns></returns>
        protected virtual bool GetUsePopup(MatrixWebPart wp)
        {
            string usePopupString = GetDataMemberValue(wp, "UseSwacPopup");
            return !string.IsNullOrWhiteSpace(usePopupString) && (usePopupString.Equals("true", StringComparison.InvariantCultureIgnoreCase) || usePopupString.Equals("1", StringComparison.InvariantCulture));
        }

        /// <summary>
        /// Gets the value of a data contract member on a web part
        /// </summary>
        /// <param name="wp"></param>
        /// <param name="dmName"></param>
        /// <returns></returns>
        protected virtual string GetDataMemberValue(MatrixWebPart wp, string dmName)
        {
            string retVal = string.Empty;
            if (wp.Model.PublishedContent.DataContract.DataMembers != null)
            {
                var dm = Array.Find(wp.Model.PublishedContent.DataContract.DataMembers, d => d.Name == dmName);
                if (dm != null)
                {
                    retVal = dm.Key;
                }
            }
            return retVal;
        }

        /// <summary>
        /// handle ShowSwacPopup click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void ShowSwacPopup_Click(object sender, EventArgs e)
        {
            CWC.Button button = sender as CWC.Button;

            // update config with initialization args
            var wpConfig = Page.SessionVariables.GetValueByName(button.ClientID) as SwacConfiguration;
            wpConfig.ComponentArgs = GetSwacComponentInitializationArgs(wpConfig.WebPartName, button.ID);

            // then serialize it to the data contract member for passing to the swac popup.
            string json = JsonConvert.SerializeObject(wpConfig);
            Page.DataContract.SetValueByName(button.ClientID, json);
        }

        /// <summary>
        /// If SWAC component requires initialization data, override this.
        /// Add args as string values in order they need to be passed to the Component initialization function.
        /// </summary>
        /// <returns></returns>
        protected virtual List<string> GetSwacComponentInitializationArgs(string webPartName, string buttonName)
        {
            return new List<string>();
        }

        /// <summary>
        /// Required script files
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            //yield return new ScriptReference("Scripts/camstar.js");
            yield return new ScriptReference("assets/lib/@swac/swac-base.min.js");
            yield return new ScriptReference("assets/lib/@swac/swac-container.min.js");
            yield return new ScriptReference("Scripts/swac-connector.js");
        }
    }
}

