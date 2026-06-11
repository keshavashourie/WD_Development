// Copyright Siemens 2023  
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
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
using OM = Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    public class RecipeList : MatrixWebPart
    {
        #region Controls

        protected virtual JQDataGrid TaskEntriesGrid
        {
            get { return FindCamstarControl("Tasks") as JQDataGrid; }
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
        /// TODO: Summary Description of function
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _instanceName = InstanceName == null ? null : (string)InstanceName.Data;
        }

        protected override void OnPreRender(System.EventArgs e)
        {
            _instanceName = InstanceName == null ? null : (string)InstanceName.Data;
            Page.SessionVariables.SetValueByName("InstanceName", _instanceName);

            base.OnPreRender(e);
        }

        #endregion

        #region Public Functions

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            var recipeServiceData = serviceData as RecipeListMaint;

            if (recipeServiceData != null && recipeServiceData.ObjectChanges != null && recipeServiceData.ObjectChanges.Tasks != null && recipeServiceData.ObjectChanges.Tasks.Length > 0)
            {
                var recTasks = WCFUtilities.WCFObject.CloneArray(recipeServiceData.ObjectChanges.Tasks, typeof(ProcessItemChanges)) as ProcessItemChanges[];
                foreach (var taskItem in recipeServiceData.ObjectChanges.Tasks)
                {
                    taskItem.PrerequisiteTasks = null;
                }
                var count = recipeServiceData.ObjectChanges.Tasks.Length;
                int j = 0;
                var taskList = new List<ProcessItemChanges>(count);
                taskList.AddRange(recipeServiceData.ObjectChanges.Tasks);

                WCFUtilities.WSDataCreator creator = new WCFUtilities.WSDataCreator();
                for (int i = 0; i < count; i++)
                {
                    if (recTasks[i].PrerequisiteTasks != null && recTasks[i].ListItemAction != ListItemAction.Delete)
                    {
                        var taskItem = creator.CreateObject(recTasks[i].GetType().Name) as ProcessItemChanges;
                        taskItem.Key = WSObjectRef.AssignNamedObject(recTasks[i].Name.ToString());
                        taskItem.PrerequisiteTasks = recTasks[i].PrerequisiteTasks;
                        foreach (var prereqTask in taskItem.PrerequisiteTasks.Where(prereq => prereq.ListItemAction != ListItemAction.Delete))
                        {
                                //Scenario where RL is new and was NOT saved previously
                            if ((prereqTask.Parent == null || (prereqTask.Parent as RevisionedObjectRef) == null) && (prereqTask.ListItemAction != ListItemAction.Delete))
                                {
                                    prereqTask.Parent = new RevisionedObjectRef();
                                    (prereqTask.Parent as RevisionedObjectRef).Revision = (string)Revision.Data;
                                    (prereqTask.Parent as RevisionedObjectRef).RevisionOfRecord = false; // (bool)IsROR.Data;
                                    (prereqTask.Parent as RevisionedObjectRef).Name = (string)InstanceName.Data;
                                }
                            if (prereqTask.ListItemAction == ListItemAction.Add && prereqTask.Parent != null)
                            {
                                prereqTask.Parent.ID = null;
                                prereqTask.Parent.CDOTypeName = recipeServiceData.ObjectChanges.GetType().Name;
                        }
                        }
                        taskItem.ListItemAction = ListItemAction.Change;
                        // TODO: SetParentToPrerequisiteTasks.
                        taskList.Add(taskItem);
                        j++;
                    }
                }
                recipeServiceData.ObjectChanges.Tasks = taskList.ToArray();
            }
        }

        public override void RequestSelectionValues(Info serviceInfo, Service serviceData)
        {
            base.RequestSelectionValues(serviceInfo, serviceData);

            (serviceData as RecipeListMaint).ObjectChanges.Tasks = new ProcessItemChanges[1];
            (serviceData as RecipeListMaint).ObjectChanges.Tasks[0] = new ProcessItemChanges() { TaskUsage = TaskUsageEnum.Batch };
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
