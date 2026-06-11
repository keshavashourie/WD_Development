// Copyright Siemens 2023  
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;

using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.FormsFramework.Utilities;

namespace Camstar.Portal
{
    public partial class ExportToPDF : System.Web.UI.Page
    {
        protected override void Render(HtmlTextWriter writer)
        {
            if (Session["ExportToPDF"] != null)
            {
                string xml = Session["ExportToPDF"].ToString();
                Response.Clear();
                Response.ContentType = "application/x-pdf";
                Response.AddHeader("Content-Type", "application/vnd.adobe.xfdf");
                System.Text.ASCIIEncoding ascii = new System.Text.ASCIIEncoding();
                Response.AddHeader("Content-Header", xml);
                Response.BinaryWrite(ascii.GetBytes(xml));
                Response.End();


                Session.Remove("ExportToPDF");
            }
        }
    }
}
