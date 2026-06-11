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
    public class SS_PartHistoryPopUp : MatrixWebPart
    {
        //Control declaration
        private CWC.TextBox _txtSelectedPart { get { return Page.FindCamstarControl("SelectedPart") as CWC.TextBox; } }
        
        //------------------------
        // Assign data contract to the textbox
        //------------------------
        private void GetSelectedPart()
        {
            try
            {
                //Manually assign the data contract value to the field
                string PartSelected = Page.PortalContext.DataContract.GetValueByName("SelectedPart").ToString();
                _txtSelectedPart.Data = PartSelected;
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            } 
        }

        //------------------------
        // WebPartCustomAction handler
        //------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e) //Custom actions
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
                if (action.Parameters == "ClosePop")
                {
                    ScriptManager.RegisterStartupScript(Page.Form, GetType(), "CloseButton", "window.parent.__page._isTabContainerPage=null;window.parent.CloseFloatingFrame(true);", true);
                }
        }

        //------------------------
        // On load, get the selected part
        //------------------------
        protected override void OnLoad(EventArgs e) //Page on load event
        {
            base.OnLoad(e);
            GetSelectedPart();
        }
	}
}



