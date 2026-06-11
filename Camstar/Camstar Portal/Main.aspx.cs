// Copyright Siemens 2025

using System;
using System.Web.UI;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.IO;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;
using System.Web;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Web.Services;
using System.Web.Script.Services;

namespace Camstar.Portal
{
    /// <summary>
    /// The main application page
    /// </summary>
    partial class Main : Camstar.WebPortal.PortalFramework.WebPartPageBase
    {
        private const string mRedirectToLineAssignmentKey = "redirectToLineAssignment";
        private bool isSSO = AuthManager.DefaultInstance.IsSSO;
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

        public override bool PageAuthorizationCheck()
        {
            return true;
        }
		private Dictionary<string, string> GetMendixUrls()
		{
			var urls = new Dictionary<string, string>();

			string jsonPath = Path.Combine(
				Path.GetDirectoryName(MapPath("~")),
				"Camstar Portal",
				"Config",
				"MXConfiguration.json"
			);

			if (File.Exists(jsonPath))
			{
				string jsonContent = File.ReadAllText(jsonPath);
				dynamic jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonContent);

				if (jsonObject.mendixApps != null)
				{
					foreach (var app in jsonObject.mendixApps)
					{
						if (app == null) continue;
						if (app.id == null || app.url == null) continue;
						
						string id = app.id.ToString();
						string url = app.url.ToString();

						switch (id)
						{
							case "MX-Core":
								urls["Core"] = url;
								break;
							case "MX-Semi":
								urls["Semiconductor"] = url;
								break;
							case "MX-Elec":
								urls["Electronics"] = url;									
								break;
						}
					}
				}
			}
			return urls;
		}	

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Page.ClientScript.RegisterHiddenField("SessionTimeoutType", "warn");
            Page.ClientScript.RegisterHiddenField("SessionTimeoutTime", Session.Timeout.ToString());
			
			// Store the domain
			string userDomain = GetUserDomain();
			Page.ClientScript.RegisterHiddenField("UserDomain", userDomain);
			
			// Store the login method in session when user first logs in
			if (Session["LoginMethod"] == null)
			{
				string loginMethod;
				if (isSSO)
				{
					loginMethod = "SAML";
                    if (HttpContext.Current.Session["LoginType"]?.ToString() == AuthManager.LoginTypeSAM)
                    {
                        loginMethod = "SAM";
                    }
                }              
                else if (AuthManager.DefaultInstance.IsForms)
                {
                    loginMethod = "Default";
                }
                else
				{
					loginMethod = "OtherAuth";  // SAM, Card, Windows authentication methods
				}

				Session["LoginMethod"] = loginMethod;
			}

			// Pass the login method to client
			Page.ClientScript.RegisterHiddenField("LoginMethod", Session["LoginMethod"]?.ToString() ?? "Default");
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            if (session.SessionValues.AccessOnlineChat && CamstarPortalSection.Settings.DefaultSettings.AccessOnlineChat && false)
            {
                // Pass the pl chat url method to client
                Page.ClientScript.RegisterHiddenField("plChatUrl", ConfigurationManager.AppSettings["plChatUIUrl"]);
            }          
           
            // Register Mendix URL to client side
            var mendixUrls = GetMendixUrls();
			var mendixSettings = new { urls = mendixUrls };
			string script = $"var mendixSettings = {Newtonsoft.Json.JsonConvert.SerializeObject(mendixSettings)};";
			Page.ClientScript.RegisterClientScriptBlock(typeof(string), "mendixSettings", script, true);

            if (ShowLineAssignment && !(GetPagesToBypassLineAssignmentPopup().Contains(Request.QueryString["redirectToPageflow"]?.ToString())))
            {
                var setLineAssignmentText = FrameworkManagerUtil.GetLabelValue("Lbl_SetLineAssignment_Title") ?? "Set Line Assignment";
                var openLineAssignment = "pop.showAjax('./LineAssignmentPage.aspx?IsFloatingFrame=2','" + setLineAssignmentText + "', 520, 662, 0, 0, true, '', '', this, true, '', null, false, false, 'Loading ...');";
                Page.ClientScript.RegisterStartupScript(GetType(), "displayLineAssignemnt", JavascriptUtil.GetStartTag() + 
                    openLineAssignment + JavascriptUtil.GetEndTag());
            }

            if (!IsPostBack)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "renderPrimaryMenu", "setTimeout(loadPortalMenu, 100,'" + isSSO + "');", true);
            }
            string wcfUrl = GetWcfUrl();
            Page.ClientScript.RegisterClientScriptBlock(typeof(string), "ps2ver",
               $"top.revDelimiter = \"{HttpUtility.JavaScriptStringEncode(CamstarPortalSection.GetRevisionDelimiter())}\"; top.wcfUrl = \"{wcfUrl}\""
                , true);
        }

        private string GetWcfUrl()
        {
            string url = Cache.Get("WcfUrl") as string;
            if (string.IsNullOrEmpty(url) || !url.Contains(Request.Url.Host))
            {
                url = "https://localhost/CamstarWCFServices";
                string address = string.Empty;
                try
                {
                    string file = Path.Combine(MapPath("~"), "endpoints.config");
                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    doc.Load(file);
                    System.Xml.XmlElement elem = (System.Xml.XmlElement)doc.DocumentElement.FirstChild;
                    if (elem != null)
                        address = elem.GetAttribute("address");
                    if (!string.IsNullOrEmpty(address))
                    {
                        address = address.Replace("localhost", Request.Url.Host);
                        var i = address.LastIndexOf('/');
                        url = address.Substring(0, i);
                    }
                }
                catch
                {
                }
                Cache.Insert("WcfUrl", url);
            }
            return url;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetSSOSessionDetails()
        {
            try
            {
                if (HttpContext.Current.Session["SSOSessionDetails"] != null)
                {
                    var response = new
                    {
                        success = true,
                        SSOSessionId = HttpContext.Current.Session["SSOSessionDetails"]
                    };

                    // Convert to JSON and base64
                    return JsonConvert.SerializeObject(response);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetPlChatDetails()
        {
            try
            {
                var response = new
                {
                    success = true,
                     accSessionId = HttpContext.Current.Session["accSessionId"]
            };

                // Convert to JSON and base64
                return JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        //Created List of pages to bypass line assignment popup
        private List<string> GetPagesToBypassLineAssignmentPopup()
        {
            return new List<string> { "DataTransferPF.1" };
        }
		
		private string GetUserDomain()
		{
			string domainName = string.Empty;
			FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

			if (!string.IsNullOrEmpty(session.SessionValues.UserDomain))
			{
				domainName = session.SessionValues.UserDomain;
			}
			return domainName;
		}	
    
    } // MainPage
}
