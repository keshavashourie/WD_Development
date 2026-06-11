// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Personalization;
using CamstarPortal.WebControls;
using Camstar.WCF.Services;
using System.Collections;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class TimersView : MatrixWebPart
    {
        #region Controls
        protected virtual JQDataGrid TimersGrid
        {
            get { return FindCamstarControl("TimersGrid") as JQDataGrid; }
        }
        protected virtual SectionDropDown TimerViewer
        {
            get { return Page.FindCamstarControl("TimerViewer") as SectionDropDown; }
        }
        protected virtual CWC.Duration ActiveTimer
        {
            get { return Page.FindCamstarControl("ActiveTimer") as CWC.Duration; }
        }

        #endregion

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            TimersGrid.BoundContext.GridReloading += BoundContext_GridReloading;

            var cont = Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid;
            if (cont != null)
            {
                Page.PortalContext.LocalSession["CurrentContainer"] = cont.Data as ContainerRef;
            }
        }

        protected virtual ResponseData BoundContext_GridReloading(object sender, JQGridEventArgs args)
        {
            var locSession = new CallStack((sender as GridContext).CallStackKey).Context.LocalSession;

            if (locSession != null)
            {
                var prof = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                var svc = new WCF.Services.ContainerTxnService(prof);
                var data = new OM.ContainerTxn();
                data.Container = locSession["CurrentContainer"] as ContainerRef;
                var req = new WCF.Services.ContainerTxn_Request
                {
                    Info = new OM.ContainerTxn_Info
                    {
                        CurrentContainerStatus = new OM.CurrentContainerStatus_Info
                        {
                            Timers = new OM.Timer_Info { RequestValue = true }
                        }
                    }
                };
                WCF.Services.ContainerTxn_Result res;
                var state = svc.Load(data, req, out res);
                if (state.IsSuccess)
                {
                    (TimersGrid.GridContext as ItemDataContext).Data = res.Value.CurrentContainerStatus.Timers;
                }
            }
            return null;
        }

        protected override void OnPreRender(EventArgs e)
        {
            var timers = TimersGrid.Data as OM.Timer[];

            TimerViewer.Visible = (timers != null && timers.Any());
            ActiveTimer.Visible = TimerViewer.Visible;

            base.OnPreRender(e);

            var script = "timersWatcher.forceTimersUpdate('container-timer');";
            ScriptManager.RegisterStartupScript(this, GetType(), "forceTimersUpdate", script, true);
        }

        #endregion
    }
}
