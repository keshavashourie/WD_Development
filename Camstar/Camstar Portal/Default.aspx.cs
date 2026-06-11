// Copyright Siemens 2025
using Camstar.Util;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.Helpers;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Optimization;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using OM = Camstar.WCF.ObjectStack;

namespace Camstar.Portal
{
    /// <summary>
    /// Summary description for Default
    /// </summary>
    partial class Default : Page
    {
        string year = DateTime.Today.Year.ToString();
        public string styleSheetString { get; set; }
        private const string _appName = "CamstarPortal";
        private bool ssoLoginSuccess = false;


        #region Overrides

        protected override void OnPreInit(EventArgs e)
        {
            bool isSSO = AuthManager.DefaultInstance.IsSSO;
            if ((!Request.IsAuthenticated ||
                true == Request.LogonUserIdentity?.IsAnonymous))
            {
                if (AuthManager.DefaultInstance.IsWindows)
                {
                    Response.StatusCode = 401;
                    Response.SubStatusCode = 1;
                    Response.End();
                    return;
                }
                if (AuthManager.DefaultInstance.IsSSO)
                {
                    ssoLoginSuccess = LoginFromIdpRequest();
                }
                else if (AuthManager.DefaultInstance.IsPki)
                {

                    if (Request.ClientCertificate == null ||
                        !Request.ClientCertificate.IsPresent)
                    {
                        Response.StatusCode = 403;
                        Response.SubStatusCode = 7;
                        Response.End();
                        return;
                    }
                }
            }
            if (AuthManager.DefaultInstance.IsSSO)
            {
                SSOLogin.Visible = true;
            }

            base.OnPreInit(e);

            // For Classic the session is killed if it's not empty. For apollo it's killed in apollo service
            if (QueryDisplayMode == "classic")
            {
                if (Request.UrlReferrer == null && !string.IsNullOrEmpty(Request.Cookies["ASP.NET_SessionId"]?.Value) && Session?.Count > 0)
                {
                    if (!isSSO)
                    {
                        Session.Abandon();
                        Response.Cookies["ASP.NET_SessionId"].Value = "";
                    }

                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            string currentTheme = Request.QueryString["Theme"];
            if (currentTheme == null)
            {
                if (QueryDisplayMode == "classic")
                    currentTheme = "Horizon";
                else
                {
                    string defaultTheme = CamstarPortalSection.Settings.DefaultSettings.DefaultTheme ?? "Camstar";
                    currentTheme = defaultTheme == "Classic" ? "Camstar" : defaultTheme;
                }

            }
            Session["CurrentTheme"] = currentTheme;

            if (IsMobile)
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession();
                if (session != null)
                    NavigateToNext(session);
            }

            //Apollo Login page redirect
            if (IsApolloLink)
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession();
                PageMapping pageMapping = new PageMapping();
                pageMapping.EnsurePageCache();
                NavigateToNext(session);
            }

            base.OnInit(e);
            styleSheetString = "<link href=\"assets/image/appOcEx32.svg\" rel=\"SHORTCUT ICON\" />";
            styleSheetString += Styles.Render(
                        string.Format("~/themes/{0}/login", currentTheme)
                    ).ToHtmlString();

            string autoComplete = WebConfigUtil.GetDefaultSettingsValue(WebConfigKeyConstants.LogonAutoComplete);

            if (!string.IsNullOrEmpty(autoComplete))
            {
                if (StringUtil.ToBool(autoComplete))
                    Form.Attributes.Add(HTMLConstants.AutoComplete, HTMLConstants.On);
                else
                    Form.Attributes.Add(HTMLConstants.AutoComplete, HTMLConstants.Off);
            }

            if (Session[mRedirectToLineAssignmentKey] != null)
            {
                Session[mRedirectToLineAssignmentKey] = null;
                Response.Redirect(FrameworkManagerUtil.LineAssignmentPage);
            }

            // Inittialize serializers
            ContentXmlSerializer<PageModel>.Instantiate();
            ContentXmlSerializer<WebPartModel>.Instantiate();
            ContentXmlSerializer<ControlModel>.Instantiate();
            ContentXmlSerializer<PageContent>.Instantiate();
            ContentXmlSerializer<WebPartDefinition>.Instantiate();
            ContentXmlSerializer<PageFlowContent>.Instantiate();
        }

        protected override void OnLoad(EventArgs e)
        {
            Year.Text = year;
            base.OnLoad(e);
            this.Form.DefaultButton = LoginButton.UniqueID;
            if (AuthManager.DefaultInstance.IsWindows)
            {
                WinAuthChangeUser.Enabled = true;
                WinAuthChangeUser.Visible = true;
                UsernameTextbox.ReadOnly = true;
            }
            else
            {
                WinAuthChangeUser.Enabled = false;
                WinAuthChangeUser.Visible = false;
                UsernameTextbox.ReadOnly = false;
            }

            if (!this.IsPostBack)
            {
                InitializePage();
                bool isBattery = Convert.ToBoolean(ConfigurationManager.AppSettings["battery"]);
                if (isBattery && AuthManager.DefaultInstance.IsWindows)
                {
                    LoginButton_Click(this, e);
                }
            }
            else
            {
                string Password = PasswordTextbox.Text;
                PasswordTextbox.Attributes.Add("value", Password);
            }

            if (AuthManager.DefaultInstance.IsSSO)
            {
                if (HttpContext.Current.Request.Form.AllKeys.Any(k => k == "Token")) // Is SSO redirect // Since there is no referrer flag
                {
                    if (!ssoLoginSuccess)
                    {
                        LoginError();
                    }
                }

            }
        }

        #endregion

        #region Protected Methods

        protected void InitializePage()
        {
            if (FormsAuthentication.IsEnabled)
                FormsAuthentication.SignOut();
            HttpContext.Current.Session.RemoveAll();

            foreach (WebPortal.PortalConfiguration.Domain domain in CamstarPortalSection.Settings.DomainSettings.Domains)
            {
                DomainDropDown.Items.Add(domain.Name);
            }
            string userName = null;
            if (AuthManager.DefaultInstance.IsWindows)
            {
                userName = Request.LogonUserIdentity.Name;
                int index = userName.IndexOf('\\');
                if (index != -1)
                {
                    userName = userName.Split('\\')[1];
                }
                else
                {
                    index = userName.IndexOf('@');
                    if (index != -1)
                    {
                        userName = userName.Split('@')[0];
                    }
                }
            }
            else if (AuthManager.DefaultInstance.IsPki)
            {
                userName =
                    AuthManager.DefaultInstance.GetPkiUserName(Request.ClientCertificate);
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                UsernameTextbox.Text = userName;
                UsernameTextbox.ReadOnly = true;
                if (AuthManager.DefaultInstance.IsWindows)
                {
                    WinAuthChangeUser.Enabled = true;
                    WinAuthChangeUser.Visible = true;
                }
                else
                {
                    WinAuthChangeUser.Enabled = false;
                    WinAuthChangeUser.Visible = false;
                }
            }

            LanguageDropDown.Items.Add(string.Empty); // add default language.
            foreach (WebPortal.PortalConfiguration.Language language in CamstarPortalSection.Settings.LanguageSettings.Languages)
            {
                LanguageDropDown.Items.Add(language.Name);
            }

            string CurrentTimeZone = DateUtil.GetCurrentTimeZoneStandardName();
            string CurrentTimeZoneCode = DateUtil.GetTimeZoneCodeByStandardName(CurrentTimeZone);
            bool found = false;
            foreach (WebPortal.PortalConfiguration.TimeZone timeZone in CamstarPortalSection.Settings.TimeZoneSettings.TimeZones)
            {
                if (!string.IsNullOrEmpty(timeZone.TimeZoneCode) && DateUtil.timeZoneMap.ContainsKey(timeZone.TimeZoneCode))
                {
                    string utcOffset = DateUtil.ExtractTimeOffset(timeZone.TimeZoneCode);
                    ListItem li = new ListItem("(UTC" + utcOffset + ") " + timeZone.Name, timeZone.TimeZoneCode);
                    li.Attributes.Add("data-offset", utcOffset);
                    TimeZoneDropDown.Items.Add(li);

                    if (timeZone.TimeZoneCode == CurrentTimeZoneCode)
                        found = true;
                }
            }

            if (CamstarPortalSection.Settings.TimeZoneSettings.TimeZones.Length == 0)
                TimeZoneDropDown.Items.Add(new ListItem("(UTC-05:00) Eastern Time (US and Canada)", "-0500EST"));
            if (found)
            {
                TimeZoneDropDown.SelectedValue = CurrentTimeZoneCode;
            }


            string path = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Request.ApplicationPath);
            ClientScript.RegisterStartupScript(Page.GetType(), "bodyload", string.Format("if(top != self) {{ window.open('{0}/', '_top'); }}", path), true);

            PasswordTextbox.MaxLength = CamstarPortalSection.Settings.DefaultSettings.PasswordLengthLimit;

            if (AuthManager.DefaultInstance.IsWindows ||
                AuthManager.DefaultInstance.IsPki)
            {
                passwordAsterisk.Visible = false;
                PasswordLabel.Visible = false;
                PasswordTextbox.Visible = false;
                domainAsterisk.Visible = false;
                DomainDropDown.Visible = false;
                DomainLabel.Visible = false;
                UsernameTextbox.ReadOnly = true;
                if (AuthManager.DefaultInstance.IsWindows)
                {
                    WinAuthChangeUser.Enabled = true;
                    WinAuthChangeUser.Visible = true;
                }
                else
                {
                    WinAuthChangeUser.Enabled = false;
                    WinAuthChangeUser.Visible = false;
                }
            }


        }

        protected void LoginButton_Click(object sender, EventArgs e)
        {
            FrameworkSession fs = new FrameworkSession();

            string dictionary = LanguageDropDown.SelectedValue;

            bool authenticated = false;

            string password = PasswordTextbox.Text;

            string timeZoneVal = TimeZoneDropDown.SelectedValue;

            TimeSpan minutes = TimeSpan.Zero;
            if (timeZoneVal != null)
            {
                minutes = DateUtil.GetUTCOffsetByTimeZone(timeZoneVal);
            }
            if (AuthManager.DefaultInstance.IsPki ||
                AuthManager.DefaultInstance.IsWindows)
            {
                authenticated = fs.Login(DomainDropDown.SelectedValue,
                                         UsernameTextbox.Text,
                                         "logingViaIdp",
                                         "CamstarPortal",
                                         Application,
                                         Session,
                                         minutes,
                                         timeZoneVal,
                                         dictionary,
                                         ref mStatusMessage,
                                         true);
                UsernameTextbox.ReadOnly = true;
                if (AuthManager.DefaultInstance.IsWindows)
                {
                    WinAuthChangeUser.Enabled = true;
                    WinAuthChangeUser.Visible = true;
                }
                else
                {
                    WinAuthChangeUser.Enabled = false;
                    WinAuthChangeUser.Visible = false;
                }
            }
            else
            {
                if (password.Length <= CamstarPortalSection.Settings.DefaultSettings.PasswordLengthLimit)
                    authenticated = fs.Login(DomainDropDown.SelectedValue, UsernameTextbox.Text, password, "CamstarPortal", Application, Session,
                        minutes, timeZoneVal, dictionary, ref mStatusMessage);
            }
            if (!authenticated)
                LoginError();
            else
            {
                PageMapping pageMapping = new PageMapping();

                /* Rolling back due to P1 STBL bug 52709 (Reopening bug 49835)
                 * pageMapping.ClearCache();
                */
                pageMapping.EnsurePageCache();
                URIConstantsBase.InitializeForAbsoluteURI(Page);

                string userName = UsernameTextbox.Text;
                FormsAuthentication.SetAuthCookie(userName, false);

                NavigateToNext(fs);
            }
        }

        public bool showLineAssignmentOnLogOn
        {
            get
            {
                return Session[SessionConstants.ShowLineAssignmentOnLogon] != null ?
                Convert.ToBoolean(Session[SessionConstants.ShowLineAssignmentOnLogon]) : true;
            }
        }
        /// <summary>
        /// NavigateToNext - given a session that has been logged in, the method sets proper display settings and navigates to next step
        ///                  either navigating to to lineassignment page or main page.
        /// </summary>
        /// <param name="fs"></param>
        protected void NavigateToNext(FrameworkSession fs)
        {
            SetDisplayValues();
            SetEmployeeLogin(fs);

            string redirectPage = mDefaultPage;
            if (LineAssignmentPageCheck())
            {
                if (IsMobile)
                {
                    redirectPage = mMainPage;
                    Session[SessionConstants.IsMobileLineAssignmentRedirect] = true;
                }
                else if (IsApollo)
                {
                    redirectPage = mMainPage + "?openLineAssignmentPopup=true";
                }
                else
                {
                    redirectPage = mMainPage;
                    Session[mRedirectToLineAssignmentKey] = true;
                }

            }
            else
            {
                //set directLink
                string directLink = Request.QueryString["directLink"];
                if (directLink != null)
                    Session["DirectLink"] = Request.QueryString;

                redirectPage = mMainPage;
            }
            Response.Redirect(redirectPage);
        }

        private void SetDisplayValues()
        {
            //Default to Display to ClassicDesktop
            Session[SessionConstants.EntryPoint] = SessionConstants.Classic;
            Session[SessionConstants.RenderMode] = SessionConstants.Fixed;
            if (IsApollo)
            {
                Session[SessionConstants.EntryPoint] = SessionConstants.Apollo;
                Session[SessionConstants.RenderMode] = SessionConstants.Responsive;
            }
            if (IsMobile)
            {
                Session[SessionConstants.RenderMode] = SessionConstants.Responsive;
            }

        }

        protected void LoginError()
        {
            ErrorLabel.Visible = true;

            if (!string.IsNullOrEmpty(mStatusMessage))
                ErrorLabel.Text = mStatusMessage;
            else
                ErrorLabel.Text = "Login Failed - invalid user name or password";
        }

        private string[] GetDefaultFilterTags(FrameworkSession session)
        {
            string[] filterTags = null;

            FilterTagInquiryService service = new FilterTagInquiryService(session.CurrentUserProfile);
            FilterTagInquiry serviceData = new OM.FilterTagInquiry
            {
                CurrentEmployee = new OM.NamedObjectRef(session.CurrentUserProfile.Name)
            };

            FilterTagInquiry_Request request = new FilterTagInquiry_Request
            {
                Info = new OM.FilterTagInquiry_Info()
                {
                    EmployeeSessionFilterTag = new OM.FilterTag_Info
                    {
                        InstanceID = new OM.Info(true)
                    },
                    FilterTagAccess = new OM.Info(true)
                }
            };

            FilterTagInquiry_Result result;

            ResultStatus resultStatus = service.GetEmpSessionFilterTags(serviceData, request, out result);
            if (resultStatus.IsSuccess)
            {
                string ids = string.Empty;
                string filterTagAccess = string.Empty;
                if (result.Value != null)
                {
                    if (result.Value.FilterTagAccess != null)
                    {
                        filterTagAccess = result.Value.FilterTagAccess.Value.ToString();
                    }
                    else
                    {
                        filterTagAccess = "1";
                    }
                    if (result.Value.EmployeeSessionFilterTag != null && result.Value.EmployeeSessionFilterTag.Length > 0)
                    {
                        ids = string.Join(",", result.Value.EmployeeSessionFilterTag.Select(ft => ft.InstanceID.ID));
                    }
                }
                filterTags = new string[] { filterTagAccess, ids };
            }
            return filterTags;
        }

        private void SetEmployeeLogin(FrameworkSession fs)
        {
            if (fs == null || fs.CurrentUserProfile == null)
                return;

            //set filter tags
            string[] filterTags = GetDefaultFilterTags(fs);
            if (filterTags != null && filterTags.Length == 2)
            {
                fs.CurrentUserProfile.FilterTagAccess = filterTags[0];
                fs.CurrentUserProfile.FilterTags = filterTags[1];
            }

            EmployeeMaintService service = new EmployeeMaintService(fs.CurrentUserProfile);
            EmployeeMaint serviceData = new EmployeeMaint
            {
                ObjectToChange = new NamedObjectRef { Name = fs.CurrentUserProfile.Name },
            };

            EmployeeMaint_Request request = new EmployeeMaint_Request();
            EmployeeMaint_Result result = new EmployeeMaint_Result();

            service.BeginTransaction();
            service.Load(serviceData);

            TimeSpan utc = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            DateTime loginTime = fs.LoginDateTime.AddMilliseconds(-fs.LoginDateTime.Millisecond);
            serviceData = new EmployeeMaint
            {
                ObjectChanges = new EmployeeChanges
                {
                    EmployeeLoginInfo = new EmployeeLoginInfoChanges
                    {
                        LastLoginDateGMT = loginTime.Subtract(utc),
                        TimeZone = fs.CurrentUserProfile.TimeZone

                    }
                }
            };
            service.ExecuteTransaction(serviceData);
            service.CommitTransaction(request, out result);
        }

        private bool LineAssignmentPageCheck()
        {
            bool isAuthorized = false;
            AuthorizationManager authManager = FrameworkManagerUtil.GetAuthorizationManager(Session);
            if (authManager != null)
            {
                string pageName = FrameworkManagerUtil.LineAssignmentPage;
                if (IsMobile)
                    pageName = FrameworkManagerUtil.MobileLineAssignmentPage;
                pageName = PageMapping.ExtractPageName(pageName);
                isAuthorized = authManager.IsUIComponentAuthorized(pageName);
            }
            Session[SessionConstants.LineAssignmentPageAuthorization] = isAuthorized;
            return isAuthorized && showLineAssignmentOnLogOn;
        }

        private bool IsMobileDevice
        {
            get
            {
                if (Request.Browser.IsMobileDevice)
                    return true;

                if (Request.Browser.Type == "Safari13")
                    return true;
                string u = Request.UserAgent;
                if (u.Contains("Android"))
                {
                    HttpContext.Current.Session["isMobile"] = true;
                    return true;
                }

                return false;
            }
        }

        private bool IsMobile
        {
            get
            {
                return this.QueryDisplayMode == "mobile" || IsMobileDevice;
            }
        }

        private bool IsApollo
        {
            get
            {
                return this.QueryDisplayMode == "apollo";
            }
        }

        private string QueryDisplayMode
        {
            get
            {
                string mode = Request.QueryString["mode"];
                return mode != null ? mode.ToLower() : "classic";
            }
        }

        private bool IsApolloLink
        {
            get
            {
                return Request["apolloLink"] != null;
            }
        }

        #endregion

        #region Private Member Variables

        private string mStatusMessage = string.Empty;
        private const string mRedirectToLineAssignmentKey = "redirectToLineAssignment";

        private const string mDefaultPage = "Default.aspx";
        private const string mMainPage = "Main.aspx";

        private bool LoginFromIdpRequest()
        {
            if (!HttpContext.Current.Request.Form.AllKeys.Any(k => k == "Token"))
            {
                return false;
            }
            var token = HttpContext.Current.Request.Form["Token"];
            if (!SingleSignOnHelper.ValidateToken(token, out ClaimsPrincipal principal))
            {
                return false;
            }

            var emailId = principal.FindFirst(ClaimTypes.Email)?.Value;
            HttpContext.Current.Session.Add("LoginType", HttpContext.Current.Request.Form["LoginType"]);
            if (HttpContext.Current.Request.Form["LoginType"] == AuthManager.LoginTypeSAM)
            {
                AuthManager.AccessKey = principal.FindFirst("AccessKey")?.Value;
                AuthManager.SecretAccessKey = principal.FindFirst("SecretAccessKey")?.Value;
                AuthManager.RefreshToken = principal.FindFirst("RefreshToken")?.Value;
                AuthManager.TokenExpirationDate = DateTime.Parse(principal.FindFirst("TokenExpirationDate")?.Value);
                AuthManager.AccessToken = principal.FindFirst("AccessToken")?.Value;
                AuthManager.IdToken = principal.FindFirst("IdToken")?.Value;
           
                var stateResponse = new
                {
                    IdToken = AuthManager.IdToken
                };
                var decoder = new TokenHelper();
                var decodedToken = decoder.DecodeJwt(AuthManager.AccessToken);
                // Access claims
                foreach (var claim in decodedToken.Claims)
                {
                    if (claim.Type == "orig_ten")
                    {
                        var orig_ten = claim.Value;
                        HttpContext.Current.Session.Add("samAuthProductIdentifier", orig_ten);
                    }

                }
                // Convert to JSON and base64
                string JSONstate = JsonConvert.SerializeObject(stateResponse);
                string state = Convert.ToBase64String(Encoding.UTF8.GetBytes(JSONstate));
                HttpContext.Current.Session.Add("SSOSessionDetails", state);
                HttpContext.Current.Session.Add("SSOAccessToken", AuthManager.AccessToken);              
            }
            else
            {
                var stateResponse = new
                {
                    UserName = Convert.ToBase64String(Encoding.UTF8.GetBytes(emailId)),
                    sessionIndex = HttpContext.Current.Request.Form["sessionIndex"]
                };
                // Convert to JSON and base64
                string JSONstate = JsonConvert.SerializeObject(stateResponse);
                string state = Convert.ToBase64String(Encoding.UTF8.GetBytes(JSONstate));
                HttpContext.Current.Session.Add("SSOSessionDetails", state);

            }
            FrameworkSession fs = new FrameworkSession();
            string status = "Logon Failed!";
            string timeZoneVal = string.Empty;
            if (Session["TimeZoneSelected"] != null)
            {
                 timeZoneVal = Session["TimeZoneSelected"].ToString();
                Session.Remove("TimeZoneSelected");
            }
                TimeSpan minutes = TimeSpan.Zero;
            if (timeZoneVal != null)
            {
                minutes = DateUtil.GetUTCOffsetByTimeZone(timeZoneVal);
            }

            var authenticated = fs.LoginViaIDP(emailId, _appName, HttpContext.Current.Application, HttpContext.Current.Session, minutes, timeZoneVal, ref status);

            if (!authenticated)
            {
                return false;
            }

            System.Web.Security.FormsAuthentication.SetAuthCookie(HttpContext.Current.Session["userName"]?.ToString(), true);
            URIConstantsBase.InitializeForAbsoluteURI(HttpContext.Current.Request);
            PageMapping pageMapping = new PageMapping();
            pageMapping.EnsurePageCache();
            URIConstantsBase.InitializeForAbsoluteURI(Page);
            NavigateToNext(fs);
            return true;

        }
        #endregion


        protected void CancelButton_Click(object sender, EventArgs e)
        {
            LanguageLabel.Visible = false;
            LanguageDropDown.Visible = false;

            TimeZoneLabel.Visible = false;
            TimeZoneDropDown.Visible = false;

            userNameAsterisk.Visible = true;
            UsernameLabel.Visible = true;
            UsernameTextbox.Visible = true;

            passwordAsterisk.Visible = true;
            PasswordLabel.Visible = true;
            if (AuthManager.DefaultInstance.IsSSO)
            {
                SSOLogin.Visible = true;
            }
            if (Session["pwd"] != null)
            {
                PasswordTextbox.Text = Session["pwd"].ToString();
            }

            PasswordTextbox.Style.Add("display", "block");

            if (AuthManager.DefaultInstance.IsWindows ||
                AuthManager.DefaultInstance.IsPki)
            {
                UsernameTextbox.ReadOnly = true;
                if (AuthManager.DefaultInstance.IsWindows)
                {
                    WinAuthChangeUser.Enabled = true;
                    WinAuthChangeUser.Visible = true;
                }
                else
                {
                    WinAuthChangeUser.Enabled = false;
                    WinAuthChangeUser.Visible = false;
                }
                passwordAsterisk.Visible = false;
                PasswordLabel.Visible = false;
                PasswordTextbox.Visible = false;
                domainAsterisk.Visible = false;
                DomainDropDown.Visible = false;
                DomainLabel.Visible = false;
            }
            else
            {
                UsernameTextbox.ReadOnly = false;
                WinAuthChangeUser.Enabled = false;
                WinAuthChangeUser.Visible = false;
                passwordAsterisk.Visible = true;
                PasswordLabel.Visible = true;
                PasswordTextbox.Visible = true;
                domainAsterisk.Visible = true;
                DomainLabel.Visible = true;
                DomainDropDown.Visible = true;
            }
            // buttons
            LoginButton.Visible = true;
            SaveButton.Visible = false;
            CancelButton.Visible = false;
            SettingsButton.Visible = true;
            LoginMainBlock.Style.Remove("height");
            userNameAsterisk.Style.Remove("visibility");
            passwordAsterisk.Style.Remove("visibility");
        }
        protected void SaveButton_Click(object sender, EventArgs e)
        {
            ClientScript.RegisterStartupScript(Page.GetType(), "saveLocalStorageTimeZone", string.Format("saveLocalStorageTimeZone('{0}');", TimeZoneDropDown.SelectedValue), true);

            LanguageLabel.Visible = false;
            LanguageDropDown.Visible = false;

            TimeZoneLabel.Visible = false;
            TimeZoneDropDown.Visible = false;

            userNameAsterisk.Visible = true;
            UsernameLabel.Visible = true;
            UsernameTextbox.Visible = true;

            passwordAsterisk.Visible = true;
            PasswordLabel.Visible = true;
            if (AuthManager.DefaultInstance.IsSSO)
            {
                SSOLogin.Visible = true;
            }
            if (Session["pwd"] != null)
            {
                PasswordTextbox.Text = Session["pwd"].ToString();
            }

            PasswordTextbox.Style.Add("display", "block");

            domainAsterisk.Visible = true;
            DomainLabel.Visible = true;
            DomainDropDown.Visible = true;
            if (AuthManager.DefaultInstance.IsWindows ||
                AuthManager.DefaultInstance.IsPki)
            {
                passwordAsterisk.Visible = false;
                PasswordLabel.Visible = false;
                PasswordTextbox.Visible = false;
                domainAsterisk.Visible = false;
                DomainDropDown.Visible = false;
                DomainLabel.Visible = false;
                UsernameTextbox.ReadOnly = true;
                if (AuthManager.DefaultInstance.IsWindows)
                {
                    WinAuthChangeUser.Enabled = true;
                    WinAuthChangeUser.Visible = true;
                }
                else
                {
                    WinAuthChangeUser.Enabled = false;
                    WinAuthChangeUser.Visible = false;
                }
            }
            else
            {
                passwordAsterisk.Visible = true;
                PasswordLabel.Visible = true;
                PasswordTextbox.Visible = true;
                domainAsterisk.Visible = true;
                DomainLabel.Visible = true;
                DomainDropDown.Visible = true;
                UsernameTextbox.ReadOnly = false;
                WinAuthChangeUser.Enabled = false;
                WinAuthChangeUser.Visible = false;
            }
            // buttons
            LoginButton.Visible = true;
            SaveButton.Visible = false;
            CancelButton.Visible = false;
            ErrorLabel.Visible = false;
            SettingsButton.Visible = true;
            LoginMainBlock.Style.Remove("height");
            userNameAsterisk.Style.Remove("visibility");
            passwordAsterisk.Style.Remove("visibility");
        }
        protected void SettingsButton_Click(object sender, ImageClickEventArgs e)
        {

            LanguageLabel.Visible = true;
            LanguageDropDown.Visible = true;

            TimeZoneLabel.Visible = true;
            TimeZoneDropDown.Visible = true;

            userNameAsterisk.Style["visibility"] = "hidden";
            UsernameLabel.Visible = false;
            UsernameTextbox.Visible = false;
            WinAuthChangeUser.Visible = false;
            WinAuthChangeUser.Enabled = false;
            SSOLogin.Visible = false;

            passwordAsterisk.Style["visibility"] = "hidden";
            PasswordLabel.Visible = false;
            if (PasswordTextbox.Text != null)
            {
                Session["pwd"] = PasswordTextbox.Text.ToString();
            }
            PasswordTextbox.Style.Add("display", "none");
            domainAsterisk.Visible = false;
            DomainLabel.Visible = false;
            DomainDropDown.Visible = false;

            ErrorLabel.Visible = false;

            // buttons
            LoginButton.Visible = false;
            SaveButton.Visible = true;
            CancelButton.Visible = true;
            SettingsButton.Visible = false;

            LoginMainBlock.Style["height"] = "330px";

            ClientScript.RegisterStartupScript(Page.GetType(), "checkTimeZone", string.Format("checkTimeZone();"), true);
        }
        protected void WinAuthChangeUser_Click(object sender, EventArgs e)
        {
            string userName = null;
            userName = Request.LogonUserIdentity.Name;
            int index = userName.IndexOf('\\');
            if (index != -1)
            {
                userName = userName.Split('\\')[1];
            }
            else
            {
                index = userName.IndexOf('@');
                if (index != -1)
                {
                    userName = userName.Split('@')[0];
                }
            }
            if (userName.Equals(UsernameTextbox.Text.Trim()))
            {
                Response.StatusCode = 401;
                Response.SubStatusCode = 1;
                Response.End();
                Response.Close();
                return;
            }
            else
            {
                InitializePage();
            }
        }
        protected void OnSSO_Click(object sender, EventArgs e)
        {
            string timeZoneVal = TimeZoneDropDown.SelectedValue;
            HttpContext.Current.Session.Add("TimeZoneSelected", timeZoneVal);
            string baseUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}";
                string relativePath = ConfigurationManager.AppSettings["idpAgentApiRelativePath"];
                string redirectUrl = $"{baseUrl}{relativePath}";
                Response.Redirect(redirectUrl);
        }
    }
}