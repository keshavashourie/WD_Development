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
using System.Web.UI.WebControls;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class TimersViewM : MatrixWebPart
    {
        #region Controls
        protected virtual JQDataGrid TimersGrid
        {
            get { return FindCamstarControl("TimersGrid") as JQDataGrid; }
        }
        protected virtual CWC.TextBox ContainerField
        {
            get { return FindCamstarControl("SelectedContainerName") as CWC.TextBox; }
        }
        #endregion

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var selectedContainer = HttpContext.Current.Request["Container"];
            var prof = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            var svc = new WCF.Services.ContainerTxnService(prof);
            var data = new OM.ContainerTxn();
            data.Container = new ContainerRef(selectedContainer);
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


        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            var script = "timersWatcher.forceTimersUpdate('container-timer');";
            ScriptManager.RegisterStartupScript(this, GetType(), " ", script, true);
        }

        #endregion
    }
}
