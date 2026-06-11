/* Copyright 2019 Siemens */
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
/// Summary description for SS_JobMatPartPop
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_JobMatPartPop : MatrixWebPart
    {
        protected CWC.NamedObject _ndoResource { get { return Page.FindCamstarControl("JobParts_Resource") as CWC.NamedObject; } }
        protected JQDataGrid _gridResourcePart { get { return Page.FindCamstarControl("JobParts_ResourceParts") as JQDataGrid; } }

        //-----------------------------------------
        // 
        //-----------------------------------------
        public SS_JobMatPartPop()
	    {   
	    }

        //-----------------------------------------
        // 
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Page.CollectDataContract();

            if (Page.PortalContext.DataContract.GetValueByName("popResourceMatParts") != null)
                _ndoResource.Data = Page.PortalContext.DataContract.GetValueByName("popResourceMatParts");

            if (Page.PortalContext.DataContract.GetValueByName("IsPopUpMat") != null)
            {
                (Page.FindCamstarControl("JobParts_Resource") as CWC.NamedObject).Visible = false; 
                (Page.FindCamstarControl("JobParts_ResourceParts") as JQDataGrid).BoundContext.Width = 580;
                //this.Margin.Top = 15; 
                (Page.FindCamstarControl("JobParts_ResourceParts") as JQDataGrid).BoundContext.Fields[3].Width = 130;
                this.Page.ActionDispatcher.PageActions().Where(act => act.Name == "btnClose" ).FirstOrDefault().IsHidden = true; 
            }
        }

        //-----------------------------------------
        // 
        //-----------------------------------------
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

    } //public class SS_JobMatPartPop : MatrixWebPart
} // namespace Camstar.WebPortal.WebPortlets.Shopfloor




