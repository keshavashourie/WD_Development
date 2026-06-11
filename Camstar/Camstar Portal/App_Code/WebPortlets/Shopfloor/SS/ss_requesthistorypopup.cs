/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_RequestHistoryPopUp : MatrixWebPart
    {
        //Control declaration
        private CWC.TextBox _txtRequestResource { get { return Page.FindCamstarControl("RequestResource") as CWC.TextBox; } }
        private CWC.TextBox _txtRequestOrder { get { return Page.FindCamstarControl("RequestOrder") as CWC.TextBox; } }
        private JQDataGrid _gridHistory { get { return Page.FindCamstarControl("Grid_History") as JQDataGrid; } }
        private JQDataGrid _gridHistoryDetail { get { return Page.FindCamstarControl("Grid_HistoryDetail") as JQDataGrid; } }

        //------------------------
        //
        //------------------------

        private void GetSelectedPart()
        {
            //Manually assign the data contract value to the field
            if (Page.PortalContext.DataContract.GetValueByName("ResourceParts_PartRequestMain") != null && Page.PortalContext.DataContract.GetValueByName("RequestHistory_PartRequestMain") != null)
            {
                string SelectedResource = Page.PortalContext.DataContract.GetValueByName("ResourceParts_PartRequestMain").ToString();
                _txtRequestResource.Data = SelectedResource;
                string SelectedRequest = Page.PortalContext.DataContract.GetValueByName("RequestHistory_PartRequestMain").ToString();
                _txtRequestOrder.Data = SelectedRequest;
            }
            else if (Page.PortalContext.DataContract.GetValueByName("RequestResouce") != null && Page.PortalContext.DataContract.GetValueByName("RequestOrder") != null)
            {
                string SelectedResource = Page.PortalContext.DataContract.GetValueByName("RequestResouce").ToString();
                _txtRequestResource.Data = SelectedResource;
                string SelectedRequest = Page.PortalContext.DataContract.GetValueByName("RequestOrder").ToString();
                _txtRequestOrder.Data = SelectedRequest;
            }

            if (Page.PortalContext.DataContract.GetValueByName("IsPopUp_RequestHistory") == null)
            {
                this.Page.ActionDispatcher.PageActions().Where(act => act.Name == "CloseButton").FirstOrDefault().IsHidden = true;
                _gridHistory.BoundContext.Width = 580;
                _gridHistoryDetail.BoundContext.Width = 580;
            }
        }

        //------------------------
        //
        //------------------------

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e) //Custom actions settings
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                if (action.Parameters == "ClosePop")
                {
                    ScriptManager.RegisterStartupScript(Page.Form, GetType(), "CloseButton", "window.parent.__page._isTabContainerPage=null;window.parent.CloseFloatingFrame(true);", true);
                }
            }
        }

        //------------------------
        //
        //------------------------

        protected override void OnLoad(EventArgs e) //Page on load event
        {
            base.OnLoad(e);
            //this.Margin.Top = 15;
            GetSelectedPart();
        }
    }
}



