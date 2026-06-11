// Copyright Siemens 2023
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Newtonsoft.Json;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Personalization;
using CWF = Camstar.WebPortal.FormsFramework;
using WC = CamstarPortal.WebControls;


/// <summary>
/// Summary description for JobMatPartPop
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Jobs
{
    public class JobMatPartPop : MatrixWebPart, IPostBackEventHandler
    {
        public JobMatPartPop()
        {
            //
            // TODO: Add constructor logic here
            //

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);


        }

        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);


        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Page.PortalContext.DataContract.GetValueByName("DCM_Grid_Resource") != null)
                (Page.FindCamstarControl("JobParts_Resource") as CWC.NamedObject).Data = Page.PortalContext.DataContract.GetValueByName("DCM_Grid_Resource");

            if (!Page.IsPostBack)
            {
                ScriptManager.RegisterStartupScript(this.Page.Form, this.Page.GetType(), "PopUpMaximize", "pop.GetCallerPage().pop.maximize();", true);
            }

        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
                if (action.Parameters == "ClosePop")
                {
                    ScriptManager.RegisterStartupScript(Page.Form, GetType(), "OKButton", "window.parent.__page._isTabContainerPage=null;window.parent.CloseFloatingFrame(true);", true);

                }
        }

        public void RaisePostBackEvent(string eventArgument)
        {

        }


    }
}
