/* Copyright 2025 Siemens */
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
    /// Summary description for CheckSheetDetails
    /// </summary>
    public class SS_CheckSheetDetails : MatrixWebPart
    {
        public SS_CheckSheetDetails()
        {
            Title = "Check Sheet Details";
        }

        #region "Protected Properties"
        protected ContainerListGrid HiddenContainer
        {
            get { return Page.FindCamstarControl("HiddenSelectedContainer") as ContainerListGrid; }
        }

        protected NamedSubentity TaskField
        {
            get { return Page.FindCamstarControl("ExecuteTask_Task") as NamedSubentity; }
        } // TaskField

        protected ScrollableMenu TaskMenu
        {
            get { return Page.FindCamstarControl("TaskMenu") as ScrollableMenu; }
        } // TaskMenu

        protected TaskListSlideMenu TaskListMenu
        {
            get { return Page.FindCamstarControl("TaskListMenu") as TaskListSlideMenu; }
        } // TaskListMenu

        protected CWC.TextBox _TaskName
        {
            get { return Page.FindCamstarControl("DCTaskName") as CWC.TextBox; }
        }

        protected NamedSubentity _Task
        {
            get { return Page.FindCamstarControl("DCTask") as NamedSubentity; }
        }

        protected RevisionedObject _TaskList
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
            //  this must be done here to properly load and display Instructions
            Page.LoadComplete += new EventHandler(Page_LoadComplete);          
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            //  this must be done here to guarantee that a Data Collection is properly displayed
            PreRenderComplete();
        }

        void Page_LoadComplete(object sender, EventArgs e)
        {
            if (_instructionTextAfterReset != null && _HiddenTaskInstructionField.Data == null)
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
                OM.ExecuteCheckSheet serviceData = new OM.ExecuteCheckSheet();
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

        private void LoadTaskDetails()
        {
            ExecuteCheckSheetService serv = Page.Service.GetService<ExecuteCheckSheetService>();
            OM.ExecuteCheckSheet data = new OM.ExecuteCheckSheet();
            OM.ExecuteCheckSheet_Info info = new OM.ExecuteCheckSheet_Info();


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

                    ExecuteCheckSheet_Request request = new ExecuteCheckSheet_Request { Info = info };
                    ExecuteCheckSheet_Result resultData;

                    var res = serv.GetDataPoints(data, null, request, out resultData);
                    if (res.IsSuccess)
                    {
                        resultData.Value.TaskList = resultData.Value.Task.Parent as OM.RevisionedObjectRef;
                        _paramDataControl.Data = resultData.Value.Clone() as OM.ExecuteCheckSheet;

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

        public static bool IsShowConfirmationMessage(OM.ExecuteCheckSheet serviceData)
        {
            var service = new ExecuteCheckSheetService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var request = new ExecuteCheckSheet_Request()
            {
                Info = new OM.ExecuteCheckSheet_Info()
                {
                    IsConfirmation = new OM.Info(true)
                }
            };
            ExecuteCheckSheet_Result result;
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
            if (arg != null && arg.Contains("ResetAction"))
            {
                _instructionTextAfterReset = _HiddenTaskInstructionField.Data as string;
            }
        }

        void ChangeTaskDC(object sender, EventArgs e)
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
                    if (serviceData is OM.ExecuteCheckSheet)
                    // _paramDataControl.FillExecuteTask(serviceData as OM.ExecuteCheckSheet);
                    {
                        var status = _paramDataControl.Validate();
                        if (!status.IsSuccess)
                        {
                            DisplayMessage(status);
                            return;
                        }
                        var resStatus = _paramDataControl.FillExecuteTask(serviceData as OM.ExecuteCheckSheet);
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
