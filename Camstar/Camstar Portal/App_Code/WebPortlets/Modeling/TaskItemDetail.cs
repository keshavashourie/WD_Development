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
using Camstar.WebPortal.Personalization;

using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    /// <summary>
    /// Used to add additional functionality to the TaskItemDetail popup on the TaskList Maint page
    /// </summary>
    public class TaskItemDetail : MatrixWebPart
    {
        #region Controls


        protected virtual JQDataGrid TaskMaterials
        {
            get { return Page.FindCamstarControl("Tasks_TaskMaterials") as JQDataGrid; }
        }
        protected virtual JQDataGrid TaskQueries
        {
            get { return Page.FindCamstarControl("Tasks_TaskQueries") as JQDataGrid; }
        }
        protected virtual CWC.CheckBox RequirePassFail
        {
            get { return Page.FindCamstarControl("RequirePassFail") as CWC.CheckBox; }
        }
        protected virtual CWC.DropDownList InstructionType
        {
            get { return Page.FindCamstarControl("InstructionType") as CWC.DropDownList; }
        }
        protected virtual JQDataGrid PrerequisiteTasksGrid
        {
            get { return Page.FindCamstarControl("PrerequisiteTasks") as JQDataGrid; }
        }

        protected virtual JQDataGrid ComputationParamMapGrid
        {
            get { return Page.FindCamstarControl("ComputationParamMapGrid") as JQDataGrid; }
        }

        protected virtual CWC.RevisionedObject DataCollectionDef
        {
            get { return Page.FindCamstarControl("DataCollectionDef") as CWC.RevisionedObject; }
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
        protected virtual CWC.TextEditor Instruction
        {
            get { return Page.FindCamstarControl("Instruction") as CWC.TextEditor; }
        }
        protected virtual CWC.TextBox DecimalScale
        {
            get { return Page.FindCamstarControl("bpWeighItemChanges_DecimalScale") as CWC.TextBox; }
        }

        protected MatrixWebPart TransactionTaskDetailWP { get { return Page.FindCamstarControl("TransactionTaskDetailWP") as MatrixWebPart; } }
        
        #endregion

        #region Protected Functions

        /// <summary>
        /// Handle values from parent page
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
                    nameSelector.PickListPanelControl.DataProvider.AfterDataLoad += pt_DataProvider_AfterDataLoad;

            }
			
            if (Computation != null)
                Computation.DataChanged += Computation_DataChanged;

            if (DataCollectionDef != null)
                DataCollectionDef.DataChanged += Computation_DataChanged;

            if (InstructionType != null)
            {
                InstructionType.DataChanged += InstructionType_DataChanged;
                if (RequirePassFail != null)
                {
                    RequirePassFail.Style.Clear();
                    if (InstructionType.Data != null && InstructionType.Data.ToString() == "4")
                        RequirePassFail.Style.Add("display", "block");
                    else
                        RequirePassFail.Style.Add("display", "none");
                }
            }
            else if (RequirePassFail != null)
            {
                RequirePassFail.Style.Clear();
                RequirePassFail.Style.Add("display", "none");
            }
            if (Sequence.Data == null)
            {
                ProcessItemChanges[] taskItems = ((Page.DataContract.GetValueByName("__Page_PopupData") as PopupData).Caller as JQDataGrid).Data as ProcessItemChanges[];
                if (taskItems != null)
                {
                    int tasksCount = taskItems.Count(), i = 1;
                    if (tasksCount > 0)
                    {
                        foreach (var task in taskItems.OrderBy(a => a.Sequence.Value))
                        {
                            if (task.Sequence.Value != i)
                            {
                                task.Sequence.Value = i;
                            }
                            i++;
                        }

                        Sequence.Data = taskItems[tasksCount - 1].Sequence.Value + 1;
                    }
                    else
                        Sequence.Data = (((Page.DataContract.GetValueByName("__Page_PopupData") as PopupData).Caller as JQDataGrid).Data as object[]).Count() + 1;
                }
            }
        }

        protected virtual void InstructionType_DataChanged(object sender, EventArgs e)
        {
            if (RequirePassFail != null)
            {
                RequirePassFail.Style.Clear();
                if (InstructionType.Data != null && InstructionType.Data.ToString() == "4")
                    RequirePassFail.Style.Add("display", "block");
                else
                    RequirePassFail.Style.Add("display", "none");
            }
            else if (RequirePassFail != null)
            {
                RequirePassFail.Style.Clear();
                RequirePassFail.Style.Add("display", "none");
            }
            this.WebPartManager.RenderToClient(TransactionTaskDetailWP);
		}
		
        /// <summary>
        /// Load ItemList grid from selection values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Computation_DataChanged(object sender, EventArgs e)
        {
            if (!Page.IsPostBack) return;
            if (Computation == null || Computation.Data == null)
            {
                if(ComputationParamMapGrid != null)
                    ComputationParamMapGrid.ClearData();
                return;
            }

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new TaskListMaintService(session.CurrentUserProfile);

            var serviceData = new TaskListMaint();
            serviceData.ObjectChanges = new TaskListChanges();

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

            var request = new TaskListMaint_Request();
            var result = new TaskListMaint_Result();
            var resultStatus = new ResultStatus();

            request.Info = new TaskListMaint_Info
            {
                ObjectChanges = new TaskListChanges_Info
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

            var selVal = ((result.Environment as TaskListMaint_Environment).ObjectChanges.Tasks as ComputationItemChanges_Environment).ComputationParams.ComputationVariable.SelectionValues;
            if (selVal != null && selVal.Rows != null)
            {
                RecordSet _params = ((result.Environment as TaskListMaint_Environment).ObjectChanges.Tasks as ComputationItemChanges_Environment).ComputationParams.ComputationVariable.SelectionValues;
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
            var currentPage = nameSelector.PickListPanelControl.PagerControl.CurrentPage;
            var itemsPerPage = ((Camstar.WebPortal.FormsFramework.WebControls.PickLists.DataProviderBase)sender).ItemsPerPage;

            TaskItemChanges[] taskItems = ((Page.DataContract.GetValueByName("__Page_PopupData") as PopupData).Caller as JQDataGrid).Data as TaskItemChanges[];
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
                    Rows = (taskItems as TaskItemChanges[]).Where(t => (t.Name != (string)TaskName.Data)).Select(p => new Row()
                    {
                        Values = new[] {
                                                  p.Name.ToString(),
                                                  p.Self == null ? parentName : ((p.Self as NamedSubentityRef).Parent as RevisionedObjectRef).ToString(),
                                                  p.Self == null ? parentID : (p.Self as NamedSubentityRef).Parent.ID,
                                                  p.Self == null ? "1" : ((p.Self as NamedSubentityRef).Parent as RevisionedObjectRef).Revision,
                                                  p.IsFrozen == null ? "False" : p.IsFrozen.ToString(),
                                                  p.Self == null ? null : p.Self.ID
                                              }
                    }).Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage).ToArray(),
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
        public override FormsFramework.ValidationStatus ValidateInputData(Service serviceData)
        {
            ValidationStatus status = base.ValidateInputData(serviceData);
            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);

            JQDataGrid grid = ComputationParamMapGrid;
            if (grid != null)
            {
                foreach (ComputationParamMapChanges cp in grid.Data as ComputationParamMapChanges[])
                {
                    if (cp.ComputationVariable == null || cp.DataPoint == null)
                    {

                        if (cp.ComputationVariable == null)
                        {
                            Label lblFieldVariable = labelCache.GetLabelByName("ComputationItemParam_ComputationVariable");
                            ValidationStatusItem statusItem = new RequiredFieldStatusItem(lblFieldVariable.Value);
                            status.Add(statusItem);
                            break;
                        }
                        else if (cp.DataPoint == null)
                        {
                            Label lblFieldVariable = labelCache.GetLabelByName("DataPointHistoryDetail_DataPoint");
                            ValidationStatusItem statusItem = new RequiredFieldStatusItem(lblFieldVariable.Value);
                            status.Add(statusItem);
                            break;
                        }

                    }
                }
            }
            if (Instruction.Data != null)
            {
                int instructionLength = Instruction.Data.ToString().Length;
                if (instructionLength > 4000)
                {
                    ValidationStatusItem statusItem = new MaxLengthFieldStatusItem("Instruction");
                    status.Add(statusItem);
                }

            }
            if (DecimalScale != null && DecimalScale.Data != null)
            {
                if (int.TryParse(DecimalScale.Data.ToString(), out int result) && result < 0)
                {
                    Label lblFieldVariable = labelCache.GetLabelByName("Lbl_ScaleNegativeValidation");
                    ValidationStatusItem statusItem = new FormsFramework.ValidationStatusItem(null, null, lblFieldVariable.Value);
                    status.Add(statusItem);

                }
           }
         
            return status;
        }

        #endregion

        #region Private Functions

        #endregion

        #region Constants

        #endregion

        #region Private Member Variables

        #endregion

    }

}

