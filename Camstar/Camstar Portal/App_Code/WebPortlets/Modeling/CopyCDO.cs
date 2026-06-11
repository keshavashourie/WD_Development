//
// Copyright Siemens 2023  
//
using Camstar.WebPortal.FormsFramework.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for CopyCDO
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Modeling
{

    public class CopyCDO : MatrixWebPart
    {

        protected virtual TextBox NameToTxt { get { return Page.FindCamstarControl("NameToTxt") as TextBox; } }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Page.PortalContext.DataContract.SetValueByName("SuggestedInstanceName", (NameToTxt.Data as string));

        }
    }
}
