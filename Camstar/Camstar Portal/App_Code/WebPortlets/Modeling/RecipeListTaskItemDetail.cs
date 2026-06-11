// Copyright Siemens 2023  
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;
using Camstar.WebPortal.FormsFramework.WebGridControls;

using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    public class RecipeListTaskItemDetail : MatrixWebPart
    {
        #region Controls

        //use this region to make properties that reference controls on the Portal page using Page.FindCamstarControl. 
        //Example:
        //private CWC.RevisionedObject MaintenanceReqField
        //{
        //    get { return Page.FindCamstarControl("ResourceActivation_MaintenanceReq") as CWC.RevisionedObject; }
        //}

        protected virtual JQDataGrid PrerequisiteTasksGrid
        {
            get { return Page.FindCamstarControl("PrerequisiteTasks") as JQDataGrid; }
        }

        protected virtual JQDataGrid ComputationParamMapGrid
        {
            get { return Page.FindCamstarControl("ComputationParamMapGrid") as JQDataGrid; }
        }

        protected virtual CWC.NamedObject Computation
        {
            get { return Page.FindCamstarControl("Computation") as CWC.NamedObject; }
        }

        protected virtual CWC.TextBox Sequence
        {
            get { return Page.FindCamstarControl("Sequence") as CWC.TextBox; }
        }

        protected virtual CWC.TextBox TaskName
        {
            get { return Page.FindCamstarControl("Name") as CWC.TextBox; }
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

            if (ComputationParamMapGrid != null)
            {
                var dataPointField = ComputationParamMapGrid.BoundContext.Fields["DataPoint"];
                var dataPointSelector = ComputationParamMapGrid.Controls.OfType<CWC.NamedSubentity>().FirstOrDefault(c => c.ID == dataPointField.GetInlineID());

                if (dataPointSelector != null)
                    dataPointSelector.PickListPanelControl.PostProcessData += dp_PostProcessData;
            }

            if (PrerequisiteTasksGrid != null)
            {
                var nameField = PrerequisiteTasksGrid.BoundContext.Fields["Name"];
                var nameSelector = PrerequisiteTasksGrid.Controls.OfType<CWC.NamedSubentity>().FirstOrDefault(c => c.ID == nameField.GetInlineID());

                if (nameSelector != null)
                {
                    nameSelector.PickListPanelControl.DataProvider.AfterDataLoad += pt_DataProvider_AfterDataLoad;
                    nameSelector.SelectChildOnly = true;
                }

            }


            if (Computation != null)
                Computation.DataChanged += Computation_DataChanged;

            if (Sequence.Data == null)
                Sequence.Data = (((Page.DataContract.GetValueByName("__Page_PopupData") as PopupData).Caller as JQDataGrid).Data as ProcessItemChanges[]).Count() + 1;
        }

        protected virtual void Computation_DataChanged(object sender, EventArgs e)
        {
            if (!Page.IsPostBack) return;
            if (Computation.Data == null)
            {
                ComputationParamMapGrid.ClearData();
                return;
            }

            //Load ItemList grid from selection values

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new RecipeListMaintService(session.CurrentUserProfile);

            var serviceData = new RecipeListMaint();
            serviceData.ObjectChanges = new RecipeListChanges();

            List<ComputationParamMapChanges> cp = new List<ComputationParamMapChanges>();
            cp.Add(new ComputationParamMapChanges() { ListItemAction = ListItemAction.Add });

            List<ComputationItemChanges> ci = new List<ComputationItemChanges>();
            ci.Add(new ComputationItemChanges()
            {
                ListItemAction = ListItemAction.Add,
                Computation = (NamedObjectRef)Computation.Data,
                ComputationParams = cp.ToArray()
            }
            );

            serviceData.ObjectChanges.Tasks = ci.ToArray();

            var request = new RecipeListMaint_Request();
            var result = new RecipeListMaint_Result();
            var resultStatus = new ResultStatus();

            request.Info = new RecipeListMaint_Info
            {
                ObjectChanges = new RecipeListChanges_Info
                {
                    Tasks = new ComputationItemChanges_Info()
                    {
                        ComputationParams = new ComputationParamMapChanges_Info()
                        {
                            ComputationVariable = new Info { RequestSelectionValues = true }
                        }
                    }
                }
            };

            resultStatus = service.GetEnvironment(serviceData, request, out result);

            if (!resultStatus.IsSuccess)
                throw new ApplicationException(resultStatus.ExceptionData.Description);

            ComputationParamMapGrid.ClearData();

            var selVal = ((result.Environment as RecipeListMaint_Environment).ObjectChanges.Tasks as ComputationItemChanges_Environment).ComputationParams.ComputationVariable.SelectionValues;
            if (selVal != null && selVal.Rows != null)
            {
                RecordSet _params = ((result.Environment as RecipeListMaint_Environment).ObjectChanges.Tasks as ComputationItemChanges_Environment).ComputationParams.ComputationVariable.SelectionValues;
                List<ComputationParamMapChanges> compParamMap = new List<ComputationParamMapChanges>();
                foreach (var _row in _params.Rows)
                {
                    ComputationParamMapChanges _compParamMap = new ComputationParamMapChanges()
                    {
                        ComputationVariable = new NamedSubentityRef(_row.Values[0], new NamedObjectRef(_row.Values[1])),
                        DataPoint = null
                    };
                    compParamMap.Add(_compParamMap);
                }
                ComputationParamMapGrid.Data = compParamMap.ToArray();
            }
        }

        protected virtual void pt_DataProvider_AfterDataLoad(object sender, DataLoadEventArgs e)
        {
            var nameField = PrerequisiteTasksGrid.BoundContext.Fields["Name"];
            var nameSelector = PrerequisiteTasksGrid.Controls.OfType<CWC.NamedSubentity>().FirstOrDefault(c => c.ID == nameField.GetInlineID());


            ProcessItemChanges[] taskItems = ((Page.DataContract.GetValueByName("__Page_PopupData") as PopupData).Caller as JQDataGrid).Data as ProcessItemChanges[];
            if (taskItems != null && taskItems.Any())
            {
                string instanceName = Page.SessionVariables.GetValueByName("InstanceName") as string;
                if (string.IsNullOrEmpty(instanceName))
                    instanceName = " ";

                string parentName = taskItems[0].Self == null ? instanceName : ((taskItems[0].Self as NamedSubentityRef).Parent as RevisionedObjectRef).ToString();
                string parentID = taskItems[0].Self == null ? null : ((taskItems[0].Self as NamedSubentityRef).Parent.ID).ToString();

                var selVals = new RecordSet()
                {
                    Headers = new[]
                            {
                                new Header() {Name = "Name"},
                                new Header() {Name = "OwnerName"},
                                new Header() {Name = "OwnerRevision"},
                                new Header() {Name = "OwnerInstanceId"},
                                new Header() {Name = "IsFrozen"},
                                new Header() {Name = "InstanceId"}
                            },
                    //Rows = (taskItems as ProcessItemChanges[]).Where(t => (t.ObjectToChange != null /*&& t.Name == t.ObjectToChange.Name*/)).Select(p => new Row()
                    Rows = (taskItems as ProcessItemChanges[]).Where(t => (t.Name != (string)TaskName.Data)).Select(p => new Row()
                    {
                        Values = new[] {
                                                  p.Name.ToString(),
                                                  p.Self == null ? parentName : ((p.Self as NamedSubentityRef).Parent as RevisionedObjectRef).ToString(),
                                                  p.Self == null ? parentID : (p.Self as NamedSubentityRef).Parent.ID,
                                                  p.Self == null ? "1" : ((p.Self as NamedSubentityRef).Parent as RevisionedObjectRef).Revision,
                                                  p.IsFrozen == null ? "False" : p.IsFrozen.ToString(),
                                                  p.Self == null ? null : p.Self.ID
                                              }
                    }).ToArray(),
                    TotalCount = taskItems.Count()
                };

                if (nameSelector != null && selVals != null)
                    (nameSelector.PickListPanelControl.DataProvider as SelectionValuesDataProvider)
                        .LastQueryResult = selVals;
            }
        }


        protected virtual void dp_PostProcessData(object sender, DataRequestEventArgs e)
        {

            RecordSet data = ((sender as PickListPanel).DataProvider as SelectionValuesDataProvider).LastQueryResult;

            if (data != null)
            {
                foreach (var r in data.Rows)
                    r.Values[1] = string.Format("{0}:{1}", r.Values[1], r.Values[2]);
                e.Data = data.GetAsExplicitlyDataTable();
            }

        }

        #endregion

        #region Public Functions

        #endregion

        #region Private Functions

        #endregion

        #region Constants

        #endregion

        #region Private Member Variables

        #endregion
    }

}
