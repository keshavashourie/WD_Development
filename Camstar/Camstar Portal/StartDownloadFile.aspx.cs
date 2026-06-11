// Copyright Siemens 2023  
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using OM = Camstar.WCF.ObjectStack;

public partial class StartDownloadFile : System.Web.UI.Page
{
    protected override void OnPreInit(EventArgs e)
    {
        base.OnPreInit(e);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(Request.QueryString["AttachmentsID"]) && !string.IsNullOrEmpty(Request.QueryString["Name"]))
        {
            string version = Request.QueryString["Version"] != null ? Request.QueryString["Version"].ToString() : string.Empty;
            CreateDownloadFileScript(Request.QueryString["Name"], version, Request.QueryString["AttachmentsID"]);
        }
    }

    private void CreateDownloadFileScript(string name, string version, string attachmentID)
    {
        string script = string.Format("<script language=\"JavaScript\">Name=\"{0}\", Version=\"{1}\", AttachmentsID=\"{2}\"</script>", name, version, attachmentID);
        
        ClientScript.RegisterClientScriptBlock(this.GetType(), "DownloadFile", script);
    }

}
