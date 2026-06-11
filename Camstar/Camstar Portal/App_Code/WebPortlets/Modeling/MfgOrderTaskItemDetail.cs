// Copyright Siemens 2019  
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;

using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    /// <summary>
    /// Used to add additional functionality to the TaskItemDetail popup on the TaskList Maint page
    /// </summary>
    public class MfgOrderTaskItemDetail : TaskItemDetail
    {
        #region Controls
        protected virtual CWC.DropDownList TaskType { get { return Page.FindCamstarControl("TaskType") as CWC.DropDownList; } }
        protected virtual CWC.NamedObject DocSet { get { return Page.FindCamstarControl("DocSet") as CWC.NamedObject; } }
        protected virtual CWC.NamedObject ESig { get { return Page.FindCamstarControl("ESig") as CWC.NamedObject; } }
        protected virtual JQDataGrid StartTimersTasksGrid { get { return Page.FindCamstarControl("Tasks_StartTimerTaskDtl") as JQDataGrid; } }
        protected virtual JQDataGrid EndTimersTasksGrid { get { return Page.FindCamstarControl("Tasks_EndTimerTaskDtl") as JQDataGrid; } }
        protected virtual JQDataGrid PrerequisiteTasks { get { return Page.FindCamstarControl("PrerequisiteTasks") as JQDataGrid; } }
        #endregion

        /// <summary>
        /// Handle values from parent page
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            TaskType.Data = 3;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            TaskType.Enabled = false;

            DocSet.Visible = false;
            ESig.Visible = false;
            StartTimersTasksGrid.Visible = false;
            EndTimersTasksGrid.Visible = false;
            PrerequisiteTasks.Visible = false;

        }

    }

}

