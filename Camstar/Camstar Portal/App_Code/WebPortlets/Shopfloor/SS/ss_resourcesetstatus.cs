/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

/// <summary>
/// Summary description for SS_ResourceSetStatus
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_ResourceSetStatus : MatrixWebPart
    {
        //Controls declaration
        private CWC.Button _btnSetStatus { get { return (Page.FindCamstarControl("Set_Status") as CWC.Button); } }
        private CWC.Button _btnPartSetStatus { get { return (Page.FindCamstarControl("PartSetStatus") as CWC.Button); } }
        private CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("EquipmentSetStatus_ComputerName") as CWC.TextBox; } }

        //---------------------------------------------------
        // On Load Personalization Event
        //---------------------------------------------------
        protected override void OnLoadPersonalization()
        {
            base.OnLoadPersonalization();
            if (!Page.IsPostBack)
            {
                if (Page.DataContract.DataMembers != null)
                    if (Page.DataContract.GetValueByName("ResourceSetStatus_PrimaryServiceType") != null)
                        Page.PrimaryServiceType = (Page.PortalContext.DataContract.GetValueByName("ResourceSetStatus_PrimaryServiceType").ToString());
            }
        }

        //---------------------------------------------------
        // On Load Event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                _txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
                _btnSetStatus.Visible = true;
                _btnPartSetStatus.Visible = false;
                if (Page.DataContract.DataMembers != null)
                {
                    if (Page.DataContract.GetValueByName("ResourceSetStatus_PrimaryServiceType") != null)
                    {
                        if (Page.DataContract.GetValueByName("ResourceSetStatus_PrimaryServiceType").ToString().Equals("PartSetStatus"))
                        {
                            // Set the visibility of the buttons
                            _btnPartSetStatus.Visible = true;
                            _btnSetStatus.Visible = false;
                            //CamstarWebControl.SetRenderToClient(_btnPartSetStatus);
                            //CamstarWebControl.SetRenderToClient(_btnSetStatus);
                        }
                    }
                }

                if ((Page.IsFloatingFrame || Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument") && (this.PrimaryServiceType == "PartSetStatus"))
                { ScriptManager.RegisterStartupScript(Page.Form, GetType(), "PopUpMaximize", "pop.GetCallerPage().pop.maximize();", true); }
            }

	    //---------additional code required for resource layout to run----------
            if (Page.IsAJAXFloatingFrame)
            {
                Personalization.UIAction[] actUIActions = Page.ActionDispatcher.PageActions();
                foreach (Personalization.UIAction act in actUIActions)
                {
                    if (act.Name.ToUpper() == "SET_STATUS")
                    {
                        (act as Personalization.SubmitAction).ServiceName = Page.DataContract.GetValueByName("ResourceSetStatus_PrimaryServiceType").ToString();
                    }
                }

            }
        }
    }
}



