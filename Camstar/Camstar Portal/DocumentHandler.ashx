<%@ WebHandler Language="C#"  Class="DocumentHandler" CodeBehind="DocumentHandler.cs" %>


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;

/// <summary>
/// Summary description for DocumentHandler
/// </summary>
public partial class DocumentHandler : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{
    #region Statics
    const string IS_IMAGE = "isimage";
    const string DOCUMENT_ID = "documentid";
    const string DOCUMENT_NAME = "name";
    const string DOCUMENT_REVISION = "revision";
    const string WIDTH = "width";
    const string HEIGHT = "height";
    const string SESSION_ID = "sessionid";
    const string USER_NAME = "username";
    #endregion Statics

    #region Public Methods
    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    public void ProcessRequest(HttpContext context)
    {
        try
        {
            UserProfile profile = CurrentUserProfile(context);
            string sessionID = profile.SessionID.Value;
            string userName = profile.Name;

            string documentID = context.Request.QueryString[DOCUMENT_ID];
            string name = context.Request.QueryString[DOCUMENT_NAME];
            string revision = context.Request.QueryString[DOCUMENT_REVISION];
            bool isImage = Camstar.Util.Utilities.GetBool(context.Request.QueryString[IS_IMAGE]);
            bool returnThumbnail = Camstar.Util.Utilities.GetBool(context.Request.QueryString["thumbnail"]);

            string mimeType = string.Empty;
            System.Net.HttpStatusCode status = System.Net.HttpStatusCode.OK;
            Camstar.Portal.DocumentService svc = new Camstar.Portal.DocumentService();
            byte[] data = null;
            if (isImage)
                data = svc.LoadImage(sessionID, userName, name, revision, out status, out mimeType);
            if (data == null)
            {
                data = svc.LoadDocument(sessionID, userName, documentID, name, revision, out status, out mimeType);
                if (data == null && !isImage)
                    data = svc.LoadImage(sessionID, userName, name, revision, out status, out mimeType);
            }
            context.Response.StatusCode = (int)status;
            context.Response.ContentType = mimeType;
            if (data != null)
            {
                context.Response.BinaryWrite(data);
            }
            context.Response.End();
        }
        catch (System.Threading.ThreadAbortException)
        {
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
    #endregion Public Methods

    #region Protected Methods    
    private UserProfile CurrentUserProfile(HttpContext context)
    {
        var session = FrameworkManagerUtil.GetFrameworkSession(context.Session);
        return session.CurrentUserProfile;
    }
    #endregion Private Methods

}
