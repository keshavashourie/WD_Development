// Copyright Siemens 2023  
using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Camstar.WebPortal.PortalFramework;
using System.Web.UI;
using Camstar.WebPortal.FormsFramework;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CWGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Personalization;


namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class OperationalView : MatrixWebPart
    {
        #region Events

        public event EventHandler ReloadData;

        public event EventHandler ReloadActions;

        public event EventHandler FirstPageLoading;

        public event EventHandler FirstPagePreRendering;

        public event EventHandler SetInProcessType;

        #endregion

        #region Controls
        protected virtual CWC.ImageControl StatusIcon { get { return Page.FindCamstarControl("ContainerStatus_Icon") as CWC.ImageControl; } }
        protected virtual CWC.TextBox ExecutedContainerName
        {
            get { return FindCamstarControl("ExecutedContainerName") as CWC.TextBox; }
        } // ExecutedContainerName

        protected virtual CWGC.JQDataGrid InQueueContainersGrid
        {
            get { return FindCamstarControl("InQueueContainersGrid") as CWGC.JQDataGrid; }
        } // InQueueContainersGrid

        protected virtual CWGC.JQDataGrid InProcessContainersGrid
        {
            get { return FindCamstarControl("InProcessContainersGrid") as CWGC.JQDataGrid; }
        } // InProcessContainersGrid

        protected virtual CWC.CheckBox IsQueueChecked
        {
            get { return FindCamstarControl("IsQueueChecked") as CWC.CheckBox; }
        } // IsQueueChecked

        protected virtual JQTabContainer StatusTabs
        {
            get
            {
                return Page.FindCamstarControl("ContainerStatus_StatusTabs") as JQTabContainer;
            }
        }

        protected CWGC.ContainerListGrid _ContainerName { get { return Page.FindCamstarControl("ContainerStatus_ContainerName") as CWGC.ContainerListGrid; } }

        #endregion

        #region Public methods

        public override void PostExecute(WCF.ObjectStack.ResultStatus status, WCF.ObjectStack.Service serviceData)
        {
            base.PostExecute(status, serviceData);

            if (status.IsSuccess)
            {
                isReload = true;
                OnReloadActions(this, EventArgs.Empty);

                (InProcessContainersGrid.GridContext as CWGC.SelValGridContext).ClearCache();
            }
        } // void PostExecute(ResultStatus status, Service serviceData)

        public override void ChildPostExecute(WCF.ObjectStack.ResultStatus status, WCF.ObjectStack.Service serviceData)
        {
            base.ChildPostExecute(status, serviceData);

            if (status.IsSuccess)
            {
                isReload = true;
                OnReloadActions(this, EventArgs.Empty);
            }
        } // void ChildPostExecute(ResultStatus status, Service serviceData)

        public virtual void SetReloadActions()
        {
            isReloadAction = true;
        } // void SetReloadActions()

        #endregion

        #region Protected methods

        protected override void OnLoad(EventArgs e)
        {
            Page.LoadComplete += Page_LoadComplete;
            base.OnLoad(e);            

            if (!Page.IsPostBack)
                isReload = true;

            (InProcessContainersGrid.GridContext as CWGC.SelValGridContext).SnapCompleted += OperationalView_SnapCompleted;
            (InQueueContainersGrid.GridContext as CWGC.SelValGridContext).SnapCompleted += OperationalView_SnapCompleted;

            if (StatusTabs != null)
                StatusTabs.LoadAllTabs = true;

        }

        protected virtual void OperationalView_SnapCompleted(DataTable dataWindowTable)
        {
            var profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            if (profile != null)
            {

                var fields = new[] { "MinEndTimeGMT", "MinEndWarningTimeGMT", "MaxEndWarningTimeGMT", "MaxEndTimeGMT" };
                TimersSupport.AdjustGridSnapData(dataWindowTable, profile.UTCOffset, fields);
            }
        }

        protected virtual void Page_LoadComplete(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
                OnFirstPageLoading(sender, e);
            //this should be caled prior to ActionDispatcher.SetActionsVisibility(); in WebPartPageBase's OnPreRender(), where actions are made visible/hidden
            if (isReloadAction)
                OnReloadActions(this, e);
        } // void OnLoad(EventArgs e)

        protected virtual void OnFirstPageLoading(object sender, EventArgs e)
        {
            if (FirstPageLoading != null)
                FirstPageLoading(sender, e);
        } // void OnFirstPageLoading(object sender, EventArgs e)

        protected virtual void OnFirstPagePreRendering(object sender, EventArgs e)
        {
            if (FirstPagePreRendering != null)
                FirstPagePreRendering(sender, e);
        } // void FirstPagePreRendering(object sender, EventArgs e)        

        protected virtual void OnReloadData(object sender, EventArgs e)
        {
            if (ReloadData != null)
                ReloadData(sender, e);
            isReload = false;
            isReloadAction = true;
        } // void OnReloadData(object sender, EventArgs e)

        protected virtual void OnReloadActions(object sender, EventArgs e)
        {
            if (ReloadActions != null)
                ReloadActions(sender, e);
            isReloadAction = false;
        } // void OnReloadActions(object sender, EventArgs e)

        protected virtual void OnSetInProcessType(object sender, EventArgs e)
        {
            if (SetInProcessType != null)
                SetInProcessType(sender, e);
        } // void SetInProcessType(object sender, EventArgs e)

        protected override void OnPreRender(EventArgs e)
        {
            if (isReload)
            {
                OnReloadData(this, e);
                if (ExecutedContainerName.Data != null && !string.IsNullOrEmpty(ExecutedContainerName.Data.ToString()))
                {
                    if (IsQueueChecked.IsChecked)
                        OnSetInProcessType(this, e);

                    InProcessContainersGrid.Action_SelectRow(ExecutedContainerName.Data.ToString(), "select");                    
                }
            }

            if (!Page.IsPostBack)
                OnFirstPagePreRendering(this, e);

            var actions = Page.ActionDispatcher.ActionPanelActions();
            if (actions != null)
            {
                foreach (var submitAction in actions.OfType<SubmitAction>())
                {
                    submitAction.TimersConfirmationRequired = true;
                }
            }

            base.OnPreRender(e);
            
            
            if (TypeInQueueFilterChecked)
                InQueueContainersGrid.GridContext.VisibleRows = InQueueContainersGrid.GridContext.RowsPerPage; // extend to the bottom of the page.
            else if (TypeInProcessFilterChecked)
                InProcessContainersGrid.GridContext.VisibleRows = InProcessContainersGrid.GridContext.RowsPerPage; // extend to the bottom of the page.
            if (StatusIcon != null)
                StatusIcon.Visible = _ContainerName.Data != null;
        } // void OnPreRender(EventArgs e)

        public virtual void SelectContainerInQueue(object container)
        {
            if (container != null && container is OM.ContainerRef && !(container as OM.ContainerRef).IsEmpty)
            {
                InQueueContainersGrid.Action_SelectRow((container as OM.ContainerRef).Name, "select");
                InProcessContainersGrid.Action_SelectRow((container as OM.ContainerRef).Name, "select");
                if (StatusIcon != null)
                    StatusIcon.Visible = true;
            }
            else
            {
                InQueueContainersGrid.Action_SelectRow(null, "deselect");
                InProcessContainersGrid.Action_SelectRow(null, "deselect");
            }
        }

        #endregion

        protected virtual bool TypeInQueueFilterChecked
        {
            get
            {
                var control = Page.FindCamstarControl("ContainerStatus_TypeInQueue");
                if (control != null && control is CWC.RadioButton)
                    return Convert.ToBoolean((control as CWC.RadioButton).Data);
                return false;
            }
        }

        protected virtual bool TypeInProcessFilterChecked
        {
            get
            {
                var control = Page.FindCamstarControl("ContainerStatus_TypeInProcess");
                if (control != null && control is CWC.RadioButton)
                    return Convert.ToBoolean((control as CWC.RadioButton).Data);
                return false;
            }
        }

        #region Fields

        private bool isReload = false;
        private bool isReloadAction = false;

        #endregion
    }
}
