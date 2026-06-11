//© Copyright Siemens 2023  
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.MetadataServices;
using System.Web;
using System.Web.UI;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Parts
{
    
    public class PartHistoryPopUp : MatrixWebPart
    {
        #region Controls

        private CWC.TextBox _txtSelectedPart { get { return Page.FindCamstarControl("SelectedPart") as CWC.TextBox; } }

        #endregion

        #region Protected Functions

        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GetSelectedPart();
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (!Page.IsPostBack)
            {
                ScriptManager.RegisterStartupScript(this.Page.Form, this.Page.GetType(), "PopUpMaximize", "pop.GetCallerPage().pop.maximize();", true);
            }
        }
        #endregion

        #region Public Functions

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
        #endregion

        #region Private Functions

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

        #endregion

        #region Constants

        #endregion

        #region Private Member Variables

        #endregion

    }

}

