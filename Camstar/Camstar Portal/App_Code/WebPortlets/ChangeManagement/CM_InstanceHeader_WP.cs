// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.WebPortlets;

/// <summary>
/// Summary description for CM_InstanceHeader_WP
/// </summary>

namespace Camstar.WebPortal.WebPortlets.ChangeManagement
{
    public class CM_InstanceHeader_WP : MatrixWebPart
    {
        protected virtual SectionDropDown ContentButton
        {
            get { return Page.FindCamstarControl("Content") as SectionDropDown; }
        }
        protected virtual SectionDropDown HistoryButton
        {
            get { return Page.FindCamstarControl("History") as SectionDropDown; }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ContentButton.Click += ContentButton_Click;
            HistoryButton.Click += HistoryButton_Click;
        }

        protected virtual void HistoryButton_Click(object sender, EventArgs e)
        {
            ContentButton.Collapse();
        }

        protected virtual void ContentButton_Click(object sender, EventArgs e)
        {
            HistoryButton.Collapse();
        }
        public override void ClearValues(Service serviceData)
        {
            base.ClearValues(serviceData);
            ContentButton.Collapse();
            HistoryButton.Collapse();
        }
    }
}
