// Copyright Siemens 2025  
using System;
using System.Web;
using System.Web.UI;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.PortalFramework;
using CamstarPortal.WebControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.FormsFramework;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using System.Collections.Specialized;
using Camstar.WebPortal.Utilities;
using System.Web.Optimization;
using System.Configuration;
using System.Linq;
using System.Web.UI.HtmlControls;
using WebClientPortal;

namespace Camstar.Portal
{
    public partial class AJAXTabMasterPage : MasterPage
    {
        public string PortalHomePage
        {
            get { return portalHomePage; }
        }

        public string PortalQuery
        {
            get { return portalQuery; }
        }

        public string RedirectPage
        {
            get
            {
                return Request.QueryString["redirectToPage"];
            }
        }

        public bool TestMode
        {
            get
            {
                bool testit = false;
                return bool.TryParse(Request.QueryString[QueryStringConstants.IsTestMode], out testit) && testit;
            }
        }

        public string RedirectPageCaption
        {
            get
            {
                return string.IsNullOrEmpty(RedirectPage) ? string.Empty : new PageMapping().GetPageDescription(RedirectPage);
            }
        }

        public string HomePageCaption
        {
            get
            {
                return string.IsNullOrEmpty(PortalHomePage) ? string.Empty : new PageMapping().GetPageDescription(PortalHomePage);
            }
        }

        public string RedirectPageflow
        {
            get
            {
                string res = null;
                string redirectTo = Request.QueryString["redirectToPageflow"];
                if (!string.IsNullOrEmpty(redirectTo))
                    res = redirectTo;
                return res;
            }
        }

        public string KillSessionScript
        {
            get
            {
                string res = string.Empty;
                if (!IsApollo)
                {
                    if (string.IsNullOrEmpty(RedirectPageflow) && string.IsNullOrEmpty(RedirectPage))
                        res = "onLoadTabMasterPage();";
                }
                return res;
            }
        }


        public string CloseLabel_GenMessage
        {
            get
            {
                string closeLabel = "Close";
                LabelCache labelCache = LabelCache.GetRuntimeCacheInstance();
                closeLabel = labelCache.GetLabelTextByName("LblMenuClose", "Close", val => closeLabel = val);
                return closeLabel;
            }

        }

        public string GeneralMessageLabel
        {
            get
            {
                string generalMessageLabel = "General Message";
                LabelCache labelCache = LabelCache.GetRuntimeCacheInstance();
                generalMessageLabel = labelCache.GetLabelTextByName("Factory_GeneralMessage", "General Message", val => generalMessageLabel = val);
                return generalMessageLabel;
            }

        }

        public string MessageLabel
        {
            get
            {
                string messageLabel = "Message";
                LabelCache labelCache = LabelCache.GetRuntimeCacheInstance();
                messageLabel = labelCache.GetLabelTextByName("EMailMessage_Message", "Message", val => messageLabel = val);
                return messageLabel;
            }

        }

        public string BodyCssClass
        {
            get
            {
                var cssClass = "body-tabbed";
                if (IsClassicMobile)
                    cssClass += " mobile";
                if (CamstarPortalSection.Settings.DefaultSettings.FullScreenMode || IsClassicMobile)
                    cssClass += " body-fullScreenMode";
                if (IsClassicDesktop)
                    cssClass = "application-frame body-tabbed";
                return cssClass;
            }
        }

        public bool IsResponsive
        {
            get
            {
                return Session[SessionConstants.RenderMode].ToString() == SessionConstants.Responsive;
            }
        }

        public bool IsClassicDesktop
        {
            get
            {
                return Session[SessionConstants.EntryPoint].ToString() == SessionConstants.Classic &&
                    Session[SessionConstants.RenderMode].ToString() == SessionConstants.Fixed;
            }
        }
        public bool IsClassicMobile
        {
            get
            {
                return Session[SessionConstants.EntryPoint].ToString() == SessionConstants.Classic &&
                    Session[SessionConstants.RenderMode].ToString() == SessionConstants.Responsive;

            }
        }

        public bool IsApollo
        {
            get
            {
                return Session[SessionConstants.EntryPoint].ToString() == SessionConstants.Apollo;
            }
        }

        public bool ShowGeneralMessage
        {
            get
            {
                if (Session["ShowGeneralMessage"] != null)
                    return false;

                bool retVal = false;
                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

                if (session != null)
                {
                    GetGeneralMessageService service = new GetGeneralMessageService(session.CurrentUserProfile);

                    var request = new GetGeneralMessage_Request()
                    {
                        Info = new GetGeneralMessage_Info()
                        {
                            DisplayGeneralMessage = new Info(true, false)
                        }
                    };

                    var result = new GetGeneralMessage_Result();
                    ResultStatus resultStatus = service.GetEnvironment(request, out result);
                    if (resultStatus != null && resultStatus.IsSuccess)
                    {
                        if (result.Value.DisplayGeneralMessage != null)
                            retVal = result.Value.DisplayGeneralMessage.Value;
                    }
                }
                Session["ShowGeneralMessage"] = retVal;
                return retVal;
            }
        }

        public string GenMessage
        {
            get
            {
                var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                string message = "";
                if (session != null)
                {
                    GetGeneralMessageService service = new GetGeneralMessageService(session.CurrentUserProfile);

                    var request = new GetGeneralMessage_Request()
                    {
                        Info = new GetGeneralMessage_Info()
                        {
                            GeneralMessage = new Info(true, false)
                        }
                    };

                    var result = new GetGeneralMessage_Result();
                    ResultStatus resultStatus = service.GetEnvironment(request, out result);
                    if (resultStatus != null && resultStatus.IsSuccess)
                        message = Convert.ToString(result.Value.GeneralMessage);
                }
                return message;
            }
        }

        public class ApolloMenuItem
        {
            public string Id;
            public string DisplayName;
            public string DisplayValue;
            public string QueryString;
            public string ParentName;
            public string Order;
            public string UIVirtualPageName;
            public string UIPageFlowName;
            public string ApolloIcon;
            public bool IsHomePage;
            public string PageURL;
            public string PageDisplay;
            public ApolloMenuItem[] Children;
        }
        private string portalHomePage;
        private string portalQuery;
        protected bool isSSO { get; set; }
        protected bool isBattery { get; set; }
        public string styleSheetString { get; set; }
        public string currentTheme { get; set; }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            isSSO = AuthManager.DefaultInstance.IsSSO;
            isBattery = Convert.ToBoolean(ConfigurationManager.AppSettings["battery"]);

            if (Session["CurrentTheme"] == null)
            {
                string defaultTheme = CamstarPortalSection.Settings.DefaultSettings.DefaultTheme ?? "Horizon";
                Session["CurrentTheme"] = defaultTheme == "Classic" ? "Camstar" : defaultTheme;
            }
            currentTheme = Session["CurrentTheme"].ToString();
            styleSheetString = "<link href=\"assets/image/sie-logo-favicon.ico\" rel=\"SHORTCUT ICON\" />";
            styleSheetString += Styles.Render(
                        string.Format("~/themes/{0}/AJAXChildMaster", currentTheme),
                        string.Format("~/themes/{0}/workspaceoverride", currentTheme),
                        string.Format("~/themes/{0}/UserAll", currentTheme),
                        string.Format("~/themes/{0}/jstree/default/jstreeCSS", currentTheme),
                         string.Format("~/themes/{0}/Horizon-application-frame.css", currentTheme),
                         string.Format("~/themes/{0}/bootstrap.min.css", currentTheme),
                         string.Format("~/themes/{0}/bootstrap-grid.min.css", currentTheme)						 
                    ).ToHtmlString();

            if (IsClassicMobile)
            {
                if (string.Compare(currentTheme, "camstar", true) == 0)
                {
                    styleSheetString += Styles.Render(
                        string.Format("~/themes/{0}/mobile/MobileCss", currentTheme)
                    ).ToHtmlString();
                }
                else
                {
                    styleSheetString += Styles.Render(
                        string.Format("~/themes/{0}/bootstrapCss", currentTheme)
                    ).ToHtmlString();
                }

            }

            WebPartPageBase page = this.AJAXContentPlaceHolder.Page as WebPartPageBase;
            if (page != null)
            {
                page.RegisteringDescriptors += Page_RegisteringDescriptors;
            }

            var directLinkParameters = (NameValueCollection)Session["DirectLink"];
            if (directLinkParameters != null)
            {
                var directLink = directLinkParameters["directLink"];
                if (isBattery)
                {
                    HttpContext.Current.Session["ContainerName"] = directLinkParameters["ctrName"];
                }
                portalQuery = directLinkParameters.ToString();
                portalHomePage = directLink != null ? directLink : GetHomePage();
                Session.Remove("DirectLink");
            }
            else
            {
                portalHomePage = GetHomePage();
            }

            Session.Remove(SessionConstants.IsMobileLineAssignmentRedirect);
            /*  This will be done via a JavaScript AJAX call when the page loads - no need to do it here
             *  
             * if (IsClassicDesktop)
            {
                var menuName = FrameworkManagerUtil.GetFrameworkSession(Page.Session).SessionValues.UserPortalV8MenuName;
                var MenuItems = CamstarPortal.WebControls.NavigationMenu.GetMenuRows();
                var menuItems = MenuItems.Where(r => r.Values[11] == menuName).Select(r => string.IsNullOrEmpty(r.Values[2]) ?
               new PortalMenuCommandChanges()
               {
                   Caption = r.Values[1],
                   PageFlow = new NamedObjectRef(r.Values[7]),
                   StartPage = new NamedObjectRef(r.Values[8]),
                   VirtualPage = new NamedObjectRef(r.Values[9]),
                   PageURL = r.Values[3],
                   QueryString = r.Values[4],
                   ServiceName = r.Values[5],
                   PortalTab = new NamedObjectRef(r.Values[10]),
                   PortalTabOption = string.IsNullOrEmpty(r.Values[15]) ? default(PortalTabOptionEnum) : (PortalTabOptionEnum)int.Parse(r.Values[15]),
                   Sequence = string.IsNullOrEmpty(r.Values[16]) ? 0 : int.Parse(r.Values[16])
               } as PortalMenuItemChanges : new PortalMenuSubMenuChanges() { Caption = r.Values[1], MenuDefinition = new NamedObjectRef(r.Values[2]), ApolloIcon = r.Values[17] } as PortalMenuItemChanges).ToArray();                

                ApolloMenuItem[] items = GetChildMenuItems(menuName, MenuItems);
                ApolloMenuItem[] apItems = items.Where(x => x.Children.Length > 0 || x.IsHomePage).ToArray();
                HttpContext.Current.Session["menuItems"] = apItems;
            }*/

            ApolloPortalService appolloSvc = new ApolloPortalService();

            ResultStatus status = appolloSvc.GetPlchatSession();
            if (status.IsSuccess)
            {
                var accsessionId = status.Message;
                HttpContext.Current.Session.Add("accSessionId", accsessionId);
            }
        }

        private ApolloMenuItem[] GetChildMenuItems(string parentName, Row[] menuItems)
        {
            var resultMenuItems = new System.Collections.Generic.List<ApolloMenuItem>();
            if (string.IsNullOrEmpty(parentName))
                return resultMenuItems.ToArray();
            var session = FrameworkManagerUtil.GetFrameworkSession();
            var items = menuItems.Where(i => i.Values[11] == parentName).ToArray();
            foreach (var item in items)
            {
                var pageFlowName = item.Values[7];
                var virtualPageName = item.Values[9];

                var navigateToUrl = !string.IsNullOrEmpty(virtualPageName) ? virtualPageName : pageFlowName;
                var queryString = string.Empty;
                if (item.Values[4] != null)
                {
                    queryString += item.Values[4].ToString();
                }
                if (item.Values[5] != null)
                {
                    queryString += queryString.EndsWith("&") ? "" : "&";
                    queryString += "ServiceName=" + item.Values[5];
                }


                var newItem = new ApolloMenuItem()
                {
                    Id = item.Values[0],
                    DisplayName = item.Values[1],
                    DisplayValue = item.Values[2],
                    PageURL = item.Values[3],
                    QueryString = queryString,
                    PageDisplay = item.Values[6],
                    UIPageFlowName = item.Values[7],
                    UIVirtualPageName = navigateToUrl,
                    ParentName = item.Values[11],
                    Order = item.Values[16],
                    ApolloIcon = item.Values[17]
                };
                var children = this.GetChildMenuItems(newItem.DisplayValue, menuItems);
                if (children != null)
                    newItem.Children = children;
                if (string.IsNullOrEmpty(newItem.UIVirtualPageName) || session.GetAuthorizationManager().IsUIComponentAuthorized(newItem.UIVirtualPageName) || !string.IsNullOrEmpty(pageFlowName))
                {
                    resultMenuItems.Add(newItem);
                }
            }

            // Set home page
            var homePage = session.SessionValues.UserPortalProfile.PortalV8HomePage;
            var rootMenuName = session.SessionValues.UserPortalV8MenuName;
            var homePageLbl = FrameworkManagerUtil.GetLabelValue("HomePageLbl") ?? "Home Page";
            if (!string.IsNullOrEmpty(homePage))
            {
                var homeMenuItem = resultMenuItems.FirstOrDefault(hmi => hmi.UIVirtualPageName == homePage) ?? null;
                if (homeMenuItem != null)
                {
                    homeMenuItem.IsHomePage = true;
                    homeMenuItem.DisplayValue = homePageLbl;
                }
                else if (resultMenuItems != null && parentName == rootMenuName) // add home menu item if configured and parent is root and not included in menu items
                {
                    resultMenuItems.Add(new ApolloMenuItem
                    {
                        UIVirtualPageName = homePage,
                        DisplayName = homePageLbl,
                        DisplayValue = homePageLbl,
                        Children = new ApolloMenuItem[0],
                        ParentName = rootMenuName,
                        IsHomePage = true
                    });
                }
            }

            return resultMenuItems.ToArray();
        }
        protected override void OnLoad(EventArgs e)
        {

            base.OnLoad(e);
            ButtonsBar.Hidden = false;
            NavigationButtonsBar.Hidden = false;
            if (IsClassicMobile)
            {
                ScriptManager.Scripts.Add(new ScriptReference { Name = "bootstrap" });
                ScriptManager.Scripts.Add(new ScriptReference { Name = "mobileControls" });
            }

        }
        private const string mRedirectToLineAssignmentKey = "redirectToLineAssignment";
        public bool ShowLineAssignment
        {
            get
            {
                object val = Session[mRedirectToLineAssignmentKey];
                if (Request.QueryString["openLineAssignmentPopup"] != null)
                    return true;
                else if (val != null)
                    return (bool)val;
                else
                    return false;
            }
        }
        protected virtual void Page_RegisteringDescriptors(object sender, ScriptDescriptorEventArgs e)
        {
            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
            if (labelCache != null)
            {
                YesLabel.Text = labelCache.GetLabelByName("Web_Yes").Value;
                NoLabel.Text = labelCache.GetLabelByName("Web_No").Value;
                OkLabel.Text = labelCache.GetLabelByName("OKButton").Value;
                MessageTitleLabel.Text = labelCache.GetLabelByName("ConfirmationMessageTitle").Value;
                CloseLabel.Text = labelCache.GetLabelByName("Web_Close").Value;
                LoadingLabel.Text = labelCache.GetLabelByName("Lbl_PopupLoadingTitle").Value;
            }

            ScriptComponentDescriptor scd = e.Descriptor as ScriptComponentDescriptor;
            if (scd != null)
            {
                if (IsClassicDesktop)
                {
                    // scd.AddComponentProperty("header", Header.UIComponentID);                    
                }
                scd.AddProperty("labels", new
                {
                    YesLabel = YesLabel.Text,
                    NoLabel = NoLabel.Text,
                    OkLabel = OkLabel.Text,
                    MessageTitle = MessageTitleLabel.Text,
                    CloseLabel = CloseLabel.Text
                });


                scd.AddProperty("LoadingLabelText", LoadingLabel.Text);
                var startInfo = new
                {
                    RedirectPage,
                    RedirectPageCaption,
                    PortalQuery,
                    HomePageCaption,
                    PortalHomePage,
                    RedirectPageflow,
                    TestMode,
                    ShowGeneralMessage,
                    GeneralMessageLabel,
                    CloseLabel_GenMessage,
                    GenMessage,
                    ShowLineAssignment
                };

                scd.AddProperty("startInfo", startInfo);
                scd.AddProperty("pageType", "ajax-tab-master");
            }
        }

        protected string GetHomePage()
        {
            string homePage = FrameworkManagerUtil.GetHomePage();
            if (IsClassicMobile)
            {
                if (Session[SessionConstants.IsMobileLineAssignmentRedirect] != null)
                {
                    Session[SessionConstants.ReturnToMobileHomePage] = homePage;
                    homePage = PageMapping.ExtractPageName(FrameworkManagerUtil.MobileLineAssignmentPage);
                }
            }
            else if (IsApollo || IsClassicDesktop)
            {
                homePage = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session).SessionValues.UserPortalProfile.PortalV8HomePage;
            }
           return homePage;
        }
    }
}
