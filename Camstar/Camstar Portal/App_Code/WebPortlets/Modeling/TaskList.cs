// Copyright Siemens 2024  
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    /// <summary>
    /// Task List maintenance 
    /// </summary>
    public class TaskList : MatrixWebPart
    {
        #region Controls
                
        protected virtual JQDataGrid TaskEntriesGrid
        {
            get { return Page.FindCamstarControl("Tasks") as JQDataGrid; }
        }

        protected virtual CWC.TextBox InstanceName
        {
            get { return Page.FindCamstarControl("NameTxt") as CWC.TextBox; }
        }

        protected virtual CWC.TextBox Revision
        {
            get { return Page.FindCamstarControl("RevisionTxt") as CWC.TextBox; }
        }

        protected virtual CWC.CheckBox IsROR
        {
            get { return Page.FindCamstarControl("IsRORChk") as CWC.CheckBox; }
        }

        #endregion

        #region Protected Functions

        /// <summary>
        /// Load the instance name
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _instanceName = InstanceName == null ? null : (string)InstanceName.Data;
            TaskEntriesGrid.BoundContext.SnapCompleted += BoundContext_SnapCompleted;
        }

        protected virtual void BoundContext_SnapCompleted(DataTable dataWindowTable)
        {
            var revDelimiter = CamstarPortalSection.GetRevisionDelimiter();
            foreach (var r in dataWindowTable.Rows.OfType<DataRow>())
            {
                r.BeginEdit();
                var id = r["_id_column"] as string;
                var item = TaskEntriesGrid.BoundContext.GetItem(id) as TaskItemChanges;
                if (item.StartTimerTaskDtl != null)
                {
                    string str = string.Empty;
                    Array.ForEach(item.StartTimerTaskDtl, n =>
                    {
                        if (n.StartTimer != null) {
                            str += n.StartTimer.Name + (string.IsNullOrEmpty(n.StartTimer.Revision) ? string.Empty : revDelimiter + n.StartTimer.Revision) + ",";                           
                        }
                    });

                    r["StartTimer"] = str.TrimEnd(',');
                }
                if (item.EndTimerTaskDtl != null)
                {
                    string str = string.Empty;
                    Array.ForEach(item.EndTimerTaskDtl, n =>
                    {
                        if (n.EndTimer != null) {
                            str += n.EndTimer.Name + (string.IsNullOrEmpty(n.EndTimer.Revision) ? string.Empty : revDelimiter + n.EndTimer.Revision) + ",";
                        }
                    });
                    
                    r["EndTimer"] = str.TrimEnd(',');
                }
            }
            dataWindowTable.AcceptChanges();
        }

        /// <summary>
        /// Set the instance name
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(System.EventArgs e)
        {
            _instanceName = InstanceName == null ? null : (string)InstanceName.Data;
            Page.SessionVariables.SetValueByName("InstanceName", _instanceName);

            base.OnPreRender(e);
        }

        public override void DisplayValues(Service serviceData)
        {
            base.DisplayValues(serviceData);
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Clean ReportInstruction characters and handle prerequisite task items
        /// </summary>
        /// <param name="serviceData"></param>
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            if ((serviceData as TaskListMaint).ObjectChanges != null && (serviceData as TaskListMaint).ObjectChanges.Instruction != null)
             (serviceData as TaskListMaint).ObjectChanges.ReportInstruction = Regex.Replace((serviceData as TaskListMaint).ObjectChanges.Instruction.ToString(), @"<[^>]*>|&nbsp;", String.Empty); 
            if ((serviceData as TaskListMaint).ObjectChanges != null && (serviceData as TaskListMaint).ObjectChanges.Tasks != null)
            {
                foreach (var taskItem in (serviceData as TaskListMaint).ObjectChanges.Tasks)
                {
                    if (taskItem.PrerequisiteTasks != null)
                        foreach (var prerequisiteTaskItem in taskItem.PrerequisiteTasks)
                        {
                            if ((prerequisiteTaskItem.Parent == null || (prerequisiteTaskItem.Parent as RevisionedObjectRef) == null) && (prerequisiteTaskItem.ListItemAction != ListItemAction.Delete))
                            {
                                prerequisiteTaskItem.Parent = new RevisionedObjectRef();
                                (prerequisiteTaskItem.Parent as RevisionedObjectRef).Revision = (string)Revision.Data;
                                (prerequisiteTaskItem.Parent as RevisionedObjectRef).RevisionOfRecord = false; // (bool)IsROR.Data;
                                (prerequisiteTaskItem.Parent as RevisionedObjectRef).Name = (string)InstanceName.Data;
                            }
                            if (prerequisiteTaskItem.Parent != null && prerequisiteTaskItem.Parent.ID != null)
                                prerequisiteTaskItem.Parent.ID = null;
                            if (prerequisiteTaskItem.ID != null)
                                prerequisiteTaskItem.ID = null;
                        }
                    if (taskItem.Instruction != null)
                        taskItem.ReportInstruction = Regex.Replace(taskItem.Instruction.ToString(), @"<[^>]*>|&nbsp;", String.Empty);
                }
            }

        }

        #endregion

        #region Private Functions

        #endregion

        #region Constants

        #endregion

        #region Private Member Variables

        private string _instanceName;

        #endregion

    }

}

