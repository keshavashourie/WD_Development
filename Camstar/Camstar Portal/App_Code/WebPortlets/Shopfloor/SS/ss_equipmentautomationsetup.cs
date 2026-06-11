/* Copyright 2019 Siemens */
using System;
using System.Data;
using System.Web;
using System.Linq;
using System.Collections.Generic;

using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;

using SEMI.AppCode;
using System.Web.UI.WebControls;

/// <summary>
/// Summary description for ss_equipmentautomationsetup
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_EquipmentAutomationSetup : MatrixWebPart
    {

        protected CWC.NamedObject _ndoResource { get { return Page.FindCamstarControl("EqpAutomationSetup_Resource") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoAutomationPlan { get { return Page.FindCamstarControl("EqpAutomationSetup_AutomationPlan") as CWC.NamedObject; } }
        protected CWC.TextBox _txtComputerName { get { return FindCamstarControl("EqpAutomationSetup_ComputerName") as CWC.TextBox; } }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (!Page.IsPostBack)
                    _txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);

                Personalization.UIAction[] actUIActions = Page.ActionDispatcher.PageActions();
                foreach (Personalization.UIAction act in actUIActions)
                {
                    if (act.Name.ToUpper() == "CLOSEACTION")
                    {
                        if (!Page.IsAJAXFloatingFrame)
                        {
                            act.IsHidden = true;
                            act.IsDisabled = true;
                            _ndoResource.Enabled = true;
                        }
                    }

                    if (act.Name.ToUpper() == "CLEARBUTTON")
                    {
                        if (Page.IsAJAXFloatingFrame)
                        {
                            act.IsHidden = true;
                            act.IsDisabled = true;
                            _ndoResource.Enabled = false;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            Page.ShopfloorReset(sender, e);
        }
    }
}



