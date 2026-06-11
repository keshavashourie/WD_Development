﻿// Copyright Siemens 2023 
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
/// Summary description for PartRequestHistoryPopup
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Parts
{
    public class RequestHistoryPopUp : MatrixWebPart
    {
        private CWC.TextBox requestresource { get { return Page.FindCamstarControl("RequestResource") as CWC.TextBox; } }
        private CWC.TextBox requestorder { get { return Page.FindCamstarControl("RequestOrder") as CWC.TextBox; } }
        
        public RequestHistoryPopUp()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private void GetSelectedPart()
        {
            if (Page.PortalContext.DataContract.GetValueByName("ResourceParts_PartRequestMain") != null && Page.PortalContext.DataContract.GetValueByName("RequestHistory_PartRequestMain") != null)
            {
                string SelectedResource = Page.PortalContext.DataContract.GetValueByName("ResourceParts_PartRequestMain").ToString();
                requestresource.Data = SelectedResource;
                string SelectedRequest = Page.PortalContext.DataContract.GetValueByName("RequestHistory_PartRequestMain").ToString();
                requestorder.Data = SelectedRequest;
            }
            else if (Page.PortalContext.DataContract.GetValueByName("RequestResouce") != null && Page.PortalContext.DataContract.GetValueByName("RequestOrder") != null)
            {
                string SelectedResource = Page.PortalContext.DataContract.GetValueByName("RequestResouce").ToString();
                requestresource.Data = SelectedResource;
                string SelectedRequest = Page.PortalContext.DataContract.GetValueByName("RequestOrder").ToString();
                requestorder.Data = SelectedRequest;
            }

            string isPopup = Page.PortalContext.DataContract.GetValueByName("IsPopUp_RequestHistory") as string;
            if (isPopup != "Yes")
            {
                //selectedresource.Visible = false;
                //resourcepartspanel.BoundContext.Width = 580;
                this.Page.ActionDispatcher.PageActions().Where(act => act.Name == "CloseButton").FirstOrDefault().IsHidden = true;
                if (!Page.IsResponsive)
                {
                    (Page.FindCamstarControl("Grid_History") as JQDataGrid).BoundContext.Width = 580;
                    (Page.FindCamstarControl("Grid_HistoryDetail") as JQDataGrid).BoundContext.Width = 580;
                }
                else
                {
                    var width = Convert.ToInt32(this.Width.Value) - 30;
                    (Page.FindCamstarControl("Grid_History") as JQDataGrid).BoundContext.Width = width;
                    (Page.FindCamstarControl("Grid_HistoryDetail") as JQDataGrid).BoundContext.Width = width;
                }
            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
                if (action.Parameters == "ClosePop")
                {
                    ScriptManager.RegisterStartupScript(Page.Form, GetType(), "CloseButton", "window.parent.__page._isTabContainerPage=null;window.parent.CloseFloatingFrame(true);", true);
                }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Margin.Top = 15;
            GetSelectedPart();
			if (!Page.IsPostBack)
			{
				ScriptManager.RegisterStartupScript(this.Page.Form, this.Page.GetType(), "PopUpMaximize", "pop.GetCallerPage().pop.maximize();", true);
			}
        }
    }
}