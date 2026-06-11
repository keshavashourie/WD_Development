// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.WebPortlets.Shopfloor;
using OM = Camstar.WCF.ObjectStack;
using PERS = Camstar.WebPortal.Personalization;


namespace Camstar.WebPortal.WebPortlets
{
    public class OperationalViewScanActions : ActionsControl
    {
        public OperationalViewScanActions()
        {
            Title = "Actions";
            this.CssClass = "webpart-actions";
            Actions = new List<WebPartActionData>();
            IncludeHandlerActions = true;
            ActionsStyle = PERS.ActionsStyleType.Link;
        }

        protected virtual ContainerListGrid ContainerName
        {
            get { return Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid; }
        } // ContainerName

        protected virtual PagePanel TransactionPanel
        {
            get { return Page.FindCamstarControl("PagePanelOpViewScan") as PagePanel; }
        } // TransactionPanel

        protected virtual Label PageNameLabel
        {
            get { return Page.FindCamstarControl("PageNameLabel") as Label; }
        } // TransactionPanel

        #region Overrides

        protected override void OnPreRender(EventArgs e)
        {
            var actions = Page.ActionDispatcher.ActionPanelActions();
            if (actions != null)
            {
                foreach (var submitAction in actions.OfType<PERS.SubmitAction>())
                {
                    submitAction.TimersConfirmationRequired = true;
                }
            }

            base.OnPreRender(e);
            string func = string.Format("$(function() {{ $('#{0}').opviewscanactions({{id:'{1}_OpViewScanActions',clearSelected:'{2}'}}); }});", this.ClientID, this.ClientID, mClearSelected);
            ScriptManager.RegisterStartupScript(this, typeof(string), string.Format("opviewscanactions__{0}", ClientID), func, true);
            
        }

        protected override void OnLoad(EventArgs e)
        {
            if (ContainerName != null) ContainerName.DataChanged += new EventHandler(ContainerName_DataChanged);
            base.OnLoad(e);
        }

        public override void ChildPostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.ChildPostExecute(status, serviceData);

            PostExecuteOpViewScan(status);
        }
        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);

            PostExecuteOpViewScan(status);
        }

        /// <summary>
        /// Overrides the OnActionClick function to set the Page Panel to the virtual page defined on the Action (instead of displaying a floating frame)
        /// </summary>
        /// <param name="eventArgument"></param>
        protected override void OnActionClick(string eventArgument)
        {
            var handled = false;

            string[] args = eventArgument.Split(EventArgumentConstants.ArgumentDelimeter);
            if (args.Length > 1 && args[1] == "UIAction")
            {
                PERS.UIAction action = Page.ActionDispatcher.ControlActionsForExecute.SingleOrDefault(a => a.Name == args[0]);
                if (action == null)
                    action = Page.ActionDispatcher.GetActionByName(args[0]);

                var floatAction = action as PERS.FloatPageOpenAction;
                if (floatAction != null && TransactionPanel != null)
                {
                    if (TransactionPanel.PanelSettings != null)
                    {
                        TransactionPanel.PanelSettings.PageName = floatAction.PageName;
                        CamstarWebControl.SetRenderToClient(TransactionPanel);

                        handled = true;
                    }
                }
            }

            if (!handled)
                base.OnActionClick(eventArgument);

        } //protected override void OnActionClick(string eventArgument)

        #endregion

        protected virtual void ContainerName_DataChanged(object sender, EventArgs e)
        {
            if (TransactionPanel != null)
            {
                if (TransactionPanel.PanelSettings != null)
                    TransactionPanel.PanelSettings.PageName = string.Empty;
                PageNameLabel.Text = null;
                CamstarWebControl.SetRenderToClient(PageNameLabel);
                CamstarWebControl.SetRenderToClient(TransactionPanel);
            }
            mClearSelected = true;
        }

        /// <summary>
        /// handle clearing / reloading of the page after successful submit - default is to retain the container
        /// and repopulate the actions after every submit.
        /// </summary>
        /// <param name="status"></param>
        protected virtual void PostExecuteOpViewScan(OM.ResultStatus status)
        {
            if (status.IsSuccess)
            {
                // Reload actions statuses
                Page.LoadComplete += (sender, args) => { this.LoadActionsStatus(); };

                //refresh the container header values and reload actions          
                if (ContainerName != null)
                {
                    if (ContainerName.Data != null)
                    {
                        string containerHolder = ContainerName.Data.ToString();
                        ContainerName.ClearData();
                        ContainerName.Data = containerHolder;

                        base.DisplayMessage(status);
                    }

                    // To clear the container after every submit, comment out the above code block and 
                    // uncomment this:
                    //ContainerName.ClearData();
                    //base.DisplayMessage(status);

                } //if (ContainerName != null)
            }
        }

        private bool mClearSelected = false;
    }
}
