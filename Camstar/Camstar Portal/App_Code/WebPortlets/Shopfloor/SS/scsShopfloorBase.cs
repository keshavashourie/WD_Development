/* Copyright 2024 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Collections;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using SEMI.AppCode;

/// <summary>
/// Summary description for scsShopfloorBase
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class scsShopfloorBase : MatrixWebPart
    {

        protected override void OnLoad(EventArgs e)
        {

            base.OnLoad(e);

            var theme = Page.Session["CurrentTheme"] != null ? Page.Session["CurrentTheme"].ToString() : string.Empty;
            if (Page.DataContract.GetValueByName("SelectedContainerNameDM") != null && Page.DataContract.GetValueByName("SelectedContainerNameDM") is string)
                Page.DataContract.SetValueByName("SelectedContainerNameDM", new ContainerRef(Page.DataContract.GetValueByName("SelectedContainerNameDM") as string));

            if (theme.ToLower() == "horizon")
            {
                List<Camstar.WebPortal.FormsFramework.WebControls.Button> lsb = Page.FindCamstarControls<Camstar.WebPortal.FormsFramework.WebControls.Button>();
                var empndo = Page.FindCamstarControls<Camstar.WebPortal.FormsFramework.WebControls.NamedObject>().FirstOrDefault(d => d.CDOTypeName == "Employee");

                foreach (var x in lsb)
                {
                    if (x.DefaultAction != null && x.DefaultAction.ToString().Contains(UIActionTypeEnum.FloatPageOpenAction.ToString("G")))
                    {
                        var floatPageOpenAction = x.DefaultAction as FloatPageOpenAction;
                        if (floatPageOpenAction.FrameLocation != null && floatPageOpenAction.FrameLocation.Height != 0 && floatPageOpenAction.FrameLocation.Width != 0)
                        {
                            floatPageOpenAction.FrameLocation.Height = 0;
                            floatPageOpenAction.FrameLocation.Width = 0;
                        }
                    }
                }

                //set AutoPostBack of employee field NDO to true for CPR 406252
                if (empndo != null)
                {
                    empndo.AutoPostBack = true;
                }
            }
        }

        protected bool IsHorizon()
        {
            var theme = Page.Session["CurrentTheme"] != null ? Page.Session["CurrentTheme"].ToString() : string.Empty;

            if (theme.ToLower() == "horizon")
            {
                return true;
            }
            return false;
        }
    }
}


