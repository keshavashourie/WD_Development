// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CamstarPortal.WebControls;
using WebC = System.Web.UI.WebControls;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebControls;
using PERS = Camstar.WebPortal.Personalization;
using Camstar.WebPortal.WCFUtilities;

namespace Camstar.WebPortal.WebPortlets
{
    /// <summary>
    /// Summary description for EProcedureDetails
    /// </summary>
    public class EProcedureDetails : MatrixWebPart
    {
        public EProcedureDetails()
        {
            Title = "EProcedure Details";
        }

        #region "Protected Properties"
        protected virtual ContainerListGrid HiddenContainer
        {
            get { return Page.FindCamstarControl("HiddenSelectedContainer") as ContainerListGrid; }
        }

        protected virtual NamedSubentity TaskField
        {
            get { return Page.FindCamstarControl("ExecuteTask_Task") as NamedSubentity; }
        } // TaskField

        protected virtual ScrollableMenu TaskMenu
        {
            get { return Page.FindCamstarControl("TaskMenu") as ScrollableMenu; }
        } // TaskMenu

        protected virtual TaskListSlideMenu TaskListMenu
        {
            get { return Page.FindCamstarControl("TaskListMenu") as TaskListSlideMenu; }
        } // TaskListMenu

        protected virtual CWC.TextBox _TaskName
        {
            get { return Page.FindCamstarControl("DCTaskName") as CWC.TextBox; }
        }

        protected virtual NamedSubentity _Task
        {
            get { return Page.FindCamstarControl("DCTask") as NamedSubentity; }
        }

        protected virtual RevisionedObject _TaskList
        {
            get { return Page.FindCamstarControl("DCTaskList") as RevisionedObject; }
        }
        protected virtual InstructionText _TaskInstructionField
        {
            get { return Page.FindCamstarControl("TaskInstructionField") as InstructionText; }
        }
        protected virtual CWC.TextBox _HiddenTaskInstructionField
        {
            get { return Page.FindCamstarControl("ExecuteTask_InstructionHidden") as CWC.TextBox; }
        }
        #endregion

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            //  THis must be done here to properly load and display Instructions
            Page.LoadComplete += PageLoadComplete;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            //  THis must be done here to guarantee that a Data Collection is properly displayed
            PreRenderComplete();
        }


        protected virtual void PageLoadComplete(object sender, EventArgs e)
        {
            if( _instructionTextAfterReset != null && _HiddenTaskInstructionField.Data == null)
                _HiddenTaskInstructionField.Data = _instructionTextAfterReset;

            _instructionTextAfterReset = null;
            _TaskInstructionField.Data = _HiddenTaskInstructionField.Data;
            CamstarWebControl.SetRenderToClient(_TaskInstructionField);

            PreRenderComplete();

            string script = "$(document.body).addClass('page-eproc-m').toggleClass('empty-container', false).toggleClass('eproc-panel-page', $('.body-eproc-txn').length != 0);";
            ScriptManager.RegisterStartupScript(this, Page.GetType(), "fixuppage", script, true);
        }

        protected void PreRenderComplete()
        {
            var TLCtrl = Page.FindCamstarControl("DCTaskList") as Camstar.WebPortal.FormsFramework.WebControls.RevisionedObject;
            _TaskList.Data = TLCtrl.Data;

            var taskCtrl = Page.FindCamstarControl("DCTask") as Camstar.WebPortal.FormsFramework.WebControls.NamedObject;
            _Task.Data = taskCtrl.Data;

            CWC.ContainerList _Container = Page.FindCamstarControl("ContainerStatus_ContainerName") as CWC.ContainerList;

            if (!_Task.IsEmpty && !_TaskList.IsEmpty && !_Container.IsEmpty)
            {
                OM.ExecuteTask serviceData = new OM.ExecuteTask();
                serviceData.Container = new OM.ContainerRef();
                serviceData.Container.Name = _Container.Data.ToString();
                base.GetInputData(serviceData);
            }

            if (mReloadDC)
            {
                if (TaskMenu != null)
                {
                    _paramDataControl.ProcessedIterations = TaskMenu.NumberOfTimesProcessed;
                    _paramDataControl.MaxIterations = TaskMenu.MaxIterations;
                    _paramDataControl.MinIterations = TaskMenu.MinIterations;
                    int iterationCount = 0;

                    if (TaskMenu.MaxIterations > 0 && TaskMenu.NumberOfTimesProcessed > 0 && (TaskMenu.NumberOfTimesProcessed >= TaskMenu.MaxIterations))
                        iterationCount = 0;
                    else
                    {
                        if (TaskMenu.MinIterations > 0)
                            iterationCount = TaskMenu.MinIterations - TaskMenu.NumberOfTimesProcessed;
                    }

                    if (iterationCount < 0)
                        iterationCount = 0;

                    _paramDataControl.IterationCount = iterationCount;

                    LoadTaskDetails();
                }
            }
        }

        protected virtual void LoadTaskDetails()
        {
            ExecuteTaskService serv = Page.Service.GetService<ExecuteTaskService>();
            OM.ExecuteTask data = new OM.ExecuteTask();
            OM.ExecuteTask_Info info = new OM.ExecuteTask_Info();

            CWC.ContainerList _Container = Page.FindCamstarControl("ContainerStatus_ContainerName") as CWC.ContainerList;
            if (_Container.IsEmpty)
            {
                _paramDataControl.Visible = false;
                return;
            }
            data.Container = new OM.ContainerRef(_Container.Data.ToString());

            data.Task = _Task.Data as OM.NamedSubentityRef;

            var scrollCtrl = Page.FindCamstarControl("TaskMenu") as ScrollableMenu;
            short instructionType = -1;
            bool isComputation = false;
            if (scrollCtrl != null)
            {
                if (scrollCtrl.TaskField != null && scrollCtrl.TaskField.ID != null)
                {
                    if (data.Task != null)
                    {
                        data.Task.ID = scrollCtrl.TaskField.ID;
                    }
                    else // added to resolve issue where task is cleared on reset
                    {
                        data.Task = scrollCtrl.TaskField;
                    }
                    instructionType = scrollCtrl.InstructionType;
                    isComputation = scrollCtrl.IsComputationTask;
                }
            }

            _paramDataControl.Visible = _DCLoaded;
            if (data.Task != null && !_DCLoaded)
            {
                if (!data.Task.IsEmpty && (instructionType == 2 || isComputation))
                {
                    Page.RequestValues(info, data);
                    _paramDataControl.RequestValues(data, info);
                    info.Container = new OM.Info(true);
                    info.Task = new OM.Info(true);
                    info.TaskList = new OM.Info(true);
                    info.InstructionType = new OM.Info(true);
                    info.ParametricDataDefType = new OM.Info(true);
                    info.WebPart = new OM.Info(true);
                    data.ParametricData = null;

                    ExecuteTask_Request request = new ExecuteTask_Request { Info = info };
                    ExecuteTask_Result resultData;

                    var res = serv.GetDataPoints(data, null, request, out resultData);
                    if (res.IsSuccess)
                    {
                        resultData.Value.TaskList = resultData.Value.Task.Parent as OM.RevisionedObjectRef;

                        _paramDataControl.Data = resultData.Value.Clone() as OM.ExecuteTask;

                        if (TaskMenu != null)
                        {
                            _paramDataControl.ProcessedIterations = TaskMenu.NumberOfTimesProcessed;
                            _paramDataControl.MaxIterations = TaskMenu.MaxIterations;
                            _paramDataControl.MinIterations = TaskMenu.MinIterations;
                            int iterationCount = 0;

                            if (TaskMenu.MaxIterations > 0 && TaskMenu.NumberOfTimesProcessed > 0 && (TaskMenu.NumberOfTimesProcessed >= TaskMenu.MaxIterations))
                                iterationCount = 0;
                            else
                            {
                                if (TaskMenu.MinIterations > 0)
                                    iterationCount = TaskMenu.MinIterations - TaskMenu.NumberOfTimesProcessed;
                            }

                            if (iterationCount < 0)
                                iterationCount = 0;

                            _paramDataControl.IterationCount = iterationCount;
                        }

                        _paramDataControl.RebuildControl();
                        TaskField.Data = resultData.Value.Task;
                        resultData.Value.Container = null;
                        resultData.Value.Task = null;
                        resultData.Value.Pass = resultData.Value.InstructionType == null || resultData.Value.InstructionType != OM.InstructionTypeEnum.PassFail;
                        _paramDataControl.Visible = true;
                        _DCLoaded = true;
                    }
                    else
                    {
                        DisplayMessage(res);
                    }
                } //if (!data.Task.IsEmpty)
            } //if (data.Task != null)
        }

        public static bool IsShowConfirmationMessage(OM.ExecuteTask serviceData)
        {
            var service = new ExecuteTaskService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var request = new ExecuteTask_Request()
            {
                Info = new OM.ExecuteTask_Info()
                {
                    IsConfirmation = new OM.Info(true)
                }
            };
            ExecuteTask_Result result;
            var res = service.Load(serviceData, request, out result);
            if (res.IsSuccess && result.Value != null)
                return (bool)result.Value.IsConfirmation;
            return false;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            HiddenContainer.DataChanged += new EventHandler(ChangeTaskDC);
            _Task.DataChanged += new EventHandler(ChangeTaskDC);

            var arg = Page.Request.Form["__EVENTARGUMENT"];
            if( arg != null && arg.Contains("ResetAction"))
            {
                _instructionTextAfterReset = _HiddenTaskInstructionField.Data as string;
            }
        }

        protected virtual void ChangeTaskDC(object sender, EventArgs e)
        {
            mReloadDC = true;
        }

        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);

            mReloadDC = status.IsSuccess;

            if (status.IsSuccess)
            {

                //If an esig was on the last task then force a reload here
                UIComponentDataMember member = Page.DataContract.DataMembers.SingleOrDefault(m => m.Name == "ESigCaptureDetailsDM");
                if (member != null && member.Value != null)
                {
                    if (TaskMenu != null && TaskListMenu != null)
                    {
                        //force a reload of the data to update the count/status of the task item
                        TaskListMenu.ForceReload = true;
                        TaskListMenu.ClearMenuItems();
                        TaskListMenu.LoadComplete();
                        if (!IsResponsive)
                        {
                            TaskListMenu.BuildSlideMenu();
                        }
                        else
                        {
                            TaskListMenu.BuildResponsiveSlideMenu();
                        }
                        TaskMenu.InitialLoad = true;
                        TaskMenu.LoadComplete();

                        LoadTaskDetails();
                    }
                }
            }
        }


        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);

            if (Page.ProcessingContext.Status != ProcessingStatusType.SubmitTransaction)
                return;

            if (TaskMenu != null && TaskMenu.TaskField != null && TaskMenu.TaskField.ID != null)
            {
                if (TaskMenu.InstructionType == 2 || TaskMenu.IsComputationTask)
                {
                    if (serviceData is OM.ExecuteTask)
                    // _paramDataControl.FillExecuteTask(serviceData as OM.ExecuteTask);
                    {
                        var status = _paramDataControl.Validate();
                        if (!status.IsSuccess)
                        {
                            DisplayMessage(status);
                            return;
                        }
                        var resStatus = _paramDataControl.FillExecuteTask(serviceData as OM.ExecuteTask);
                        DisplayMessage(resStatus);
                    }

                }
            }
        }

        protected override void AddFieldControls()
        {
            var IsResponsive = this.IsResponsive;// System.Web.HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.IsMobileEntryPoint] != null;
            _paramDataControl = !IsResponsive ?
                                        new ShopFloorDCControlDesktop { AllowMultipleSamples = true } as ShopFloorDCControl :
                                        new ShopFloorDCControlResponsive { AllowMultipleSamples = true } as ShopFloorDCControl;
            _paramDataControl.ID = "DCParamDataField";
            this[0, 0] = _paramDataControl;
        }

        private ShopFloorDCControl _paramDataControl;
        private bool mReloadDC;
        private bool _DCLoaded;

        private string _instructionTextAfterReset = null;
    }
}
