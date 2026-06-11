<%-- Copyright Siemens 2023   --%>
<%@ WebHandler Language="C#" Class="SessionHandler" %>

using System;
using System.Linq;
using System.Web;
using System.Web.Security;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.PortalConfiguration;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;

public class SessionHandler : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{
    public void ProcessRequest (HttpContext context)
    {
        context.Response.ClearContent();
        context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        context.Response.Cache.SetNoServerCaching();

        if (context.Session != null && context.Request.QueryString["keyId"] != null)
        {
            var keyId = context.Request.QueryString["keyId"];
            var keysToRemove = context.Session.Keys.Cast<string>().Where(key => key.StartsWith(keyId)).ToList();
            foreach (var key in keysToRemove)
            {
                if (context.Session[key] is System.IDisposable)
                    (context.Session[key] as System.IDisposable).Dispose();
                context.Session.Remove(key);

                context.Response.Write("Session key " + key + " removed\n");
            }
        }
        else if (context.Session != null && context.Request.QueryString["refresh"] != null)
        {
            try
            {
                if (context.Request.QueryString["keepalive"] != null)
                {
                    FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession();
                    QueryService oService = new QueryService(session.CurrentUserProfile);
                    QueryOptions oOptions = new QueryOptions();
                    RecordSet oData = new RecordSet();
                    RecordSet oInstanceID = new RecordSet();
                    ResultStatus oResult = oService.ExecuteAdHoc("SELECT COUNT(*) FROM ActiveUserSession WHERE 1=1", oOptions, out oData);
                }
            }
            catch { }
            context.Session["SessionRefreshTime"] = DateTime.Now;
        }
        else
        {
            if (context.Session != null)
            {
                var keysToDispose = context.Session.Keys.Cast<string>().ToList();
                foreach (var key in keysToDispose)
                {
                    if (context.Session[key] is System.IDisposable)
                        (context.Session[key] as System.IDisposable).Dispose();
                }
                context.Session.Abandon();
                context.Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", ""));

                if (context.Request.QueryString["callback"] != null)
                {
                    CallBackResponse(context);
                }
                else
                {
                    context.Response.Write("Session closed");
                }
            }
            else
            {
                context.Response.Write("Session empty");
            }
        }
    }

    public bool IsReusable
    {
        get { return true; }
    }

    private void CallBackResponse(HttpContext context)
    {
        context.Response.Write(
            "s111 _ig_start[[\"" + context.Request.QueryString["callback"] + "\",\"SessionClosed\"]]_ig_end");
    }
}
