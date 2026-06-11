// Copyright Siemens 2024  
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
using Camstar.WebPortal.PortalFramework;
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

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
	public class EProcedure : MatrixWebPart, IPostBackEventHandler
	{

		#region "Controls"

		protected virtual ContainerListGrid ContainerName
		{
			get { return Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid; }
		} // ContainerName

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

		protected virtual ContainerListGrid HiddenContainer
		{
			get { return Page.FindCamstarControl("HiddenSelectedContainer") as ContainerListGrid; }
		}

		protected virtual ShopFloorDCControl DataCollectionControl
		{
			get { return Page.FindCamstarControl("DCParamDataField") as ShopFloorDCControl; }
		}

		protected virtual CWC.TextBox CommentsField
		{
			get { return Page.FindCamstarControl("Shopfloor_Comments") as CWC.TextBox; }
		}

		protected virtual NamedObject WorkstationField
		{
			get { return Page.FindCamstarControl("ExecuteTask_Workstation") as NamedObject; }
		}

		protected virtual NamedObject HiddenWorkstationField
		{
			get { return Page.FindCamstarControl("ExecuteTask_WorkstationHidden") as NamedObject; }
		}

		protected virtual NamedObject HiddenResource
		{
			get { return Page.FindCamstarControl("HiddenResource") as NamedObject; }
		}

		protected virtual CWC.TextBox ResourceAvailabilityField
		{
			get { return Page.FindCamstarControl("ResourceAvailability") as CWC.TextBox; }
		}

		protected virtual CWC.PagePanel PagePanelField
		{
			get { return Page.FindCamstarControl("PagePanelEProcTxn") as CWC.PagePanel; }
		}

		protected virtual WorkflowViewerControl WFControl
		{
			get { return Page.FindCamstarControl("ContainerStatus_WorkflowViewer") as WorkflowViewerControl; }
		}

		protected virtual CWC.Button CreateProdEventButton
		{
			get { return Page.FindCamstarControl("CreateProdEventButton") as CWC.Button; }
		}

		protected virtual NamedSubentity _Task
		{
			get { return Page.FindCamstarControl("DCTask") as NamedSubentity; }
		}
		protected virtual string OldSelectedContainerName {get; set;}

		#endregion
		protected override void OnLoad(EventArgs e)
		{
            SetWorkstation();
            base.OnLoad(e);

			if (Page.Request.Form["__EVENTARGUMENT"] != null)
			{
				string eventArgument = Page.Request.Form["__EVENTARGUMENT"];

				if (eventArgument.Contains("AccordionMenuPB") || eventArgument.Contains("AccordionMenuPB") || eventArgument.Contains("leftClick"))
				{
					RaisePostBackEvent(Page.Request.Form["__EVENTARGUMENT"]);
				}
			}

			if(ContainerName.SelectionData != null)
				OldSelectedContainerName = ContainerName.SelectionData.ToString();
			
			ContainerName.DataChanged += new EventHandler(ContainerChanged);
			TaskField.DataChanged += new EventHandler(TaskMenuChanged);
			PagePanelField.PanelLoaded += new EventHandler(PagePanelChanged);

			if (!Page.IsPostBack && HttpContext.Current.Session["ContainerName"] != null)
			{
				var containerName = HttpContext.Current.Session["ContainerName"] as string;
				ContainerName.Data = containerName;
			}
		}
		/// <summary>
		/// Reload the workflow control after the PagePanel changes (for transactions tasks)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void PagePanelChanged(object sender, EventArgs e)
		{
			if (WFControl != null)
			{
				WFControl.LoadWorkflow();
				WFControl.WFControl.BuildWorkflowGraph();
			}
		}
		protected virtual void TaskMenuChanged(object sender, EventArgs e)
		{			
            if (_Task.Data == null || TaskField.Data != null && _Task.Data != null &&_Task.Data.ToString() != TaskField.Data.ToString())
                _Task.Data = TaskField.Data;
			if (Page.IsPostBack)
			{
				UpdateWorkstation();

				if (CommentsField != null)
					CommentsField.ClearData();

				//line assignment value takes precedence
				SetWorkstation();
			}
		}
        public override void GetLineAssignment(OM.Service serviceData)
		{
			base.GetLineAssignment(serviceData);

			object lineAssignmentResource = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Resource);
			if (lineAssignmentResource != null)
			{
				(serviceData as OM.ExecuteTask).WorkCell = lineAssignmentResource as OM.NamedObjectRef;
			}

		}
		public virtual void UpdateWorkstation()
		{
			//if the task changes, clear the workstation
			if (WorkstationField != null)
			{
				if (WorkstationField.Data != null)
                {
                    if (ContainerName.Data != null && string.Compare(ContainerName.Data.ToString(), OldSelectedContainerName, true) == 0)
                    {
                        HiddenWorkstationField.Data = WorkstationField.SelectionData;
                    }
                    WorkstationField.ClearData();
                }

                if (HiddenWorkstationField != null && HiddenWorkstationField.Data != null)
					WorkstationField.Data = HiddenWorkstationField.Data;
			}
		}
		/// <summary>
		/// default the workstation and make it readonly if it is set from the line assignment
		/// </summary>
		public virtual void SetWorkstation()
		{
			if (WorkstationField != null)
			{
				object lineAssignmentWorkstation = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.WorkStation);
				if (lineAssignmentWorkstation != null && WorkstationField != null)
				{
					WorkstationField.Data = lineAssignmentWorkstation;
					WorkstationField.DataDependencies = null;
					WorkstationField.ReadOnly = true;
				}
				else
					WorkstationField.ReadOnly = false;
				if (Page.IsPostBack)
					Page.SetFocus(WorkstationField.ClientID);
			}
		}
		protected virtual void ContainerChanged(object sender, EventArgs e)
		{
			if (CommentsField != null)
				CommentsField.ClearData();

			if (TaskListMenu != null)
			{
				TaskListMenu.StartIndex = 1;
				TaskListMenu.ActiveSlide = 1;
			}

			if (ContainerName.IsEmpty)
			{
				ClearValues();
				ClearControls();

				Page.RenderToClient = true;
			}

			if (Page.StatusBar != null)
			{
				Page.StatusBar.ClearMessage();
			}

			TaskMenu.ClearData();
			TaskListMenu.ClearData();

            var theme = Page.Session["CurrentTheme"] != null ? Page.Session["CurrentTheme"].ToString() : string.Empty;
            if (!IsResponsive && (theme.ToLower() != "horizon"))
            {
                CreateProdEventButton.Visible = !ContainerName.IsEmpty;
            }
		}
		protected virtual void ClearControls()
		{
			if (DataCollectionControl != null)
			{
				DataCollectionControl.Clean();

				if (DataCollectionControl.Data != null && HiddenContainer.Data != null)
				{
					DataCollectionControl.Data.Container = (OM.ContainerRef)HiddenContainer.Data;
				}
			}

			if (ContainerName.Data == null && HiddenContainer.Data != null)
			{
				HiddenContainer.ClearData();
			}

			var passfailCtrl = Page.FindCamstarControl("PassFailField") as Camstar.WebPortal.FormsFramework.WebControls.RadioButtonList;
			if (passfailCtrl != null)
				passfailCtrl.ClearData();

			if (CommentsField != null)
				CommentsField.ClearData();

			SetWorkstation();
		}
		public override void GetInputData(OM.Service serviceData)
		{
			base.GetInputData(serviceData);
		}
		public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
		{
			base.WebPartCustomAction(sender, e);
			var action = e.Action as CustomAction;
			if (action != null)
			{
				switch (action.Parameters)
				{
					case "Reset":
                        { 
							Page.ShopfloorReset(sender, e as CustomActionEventArgs);
							ClearWorkstationImage();
							ClearControls();

							if (DataCollectionControl != null)
							{
								if (DataCollectionControl.Data != null && HiddenContainer.Data != null)
									DataCollectionControl.Data.Container = (OM.ContainerRef)HiddenContainer.Data;
								DataCollectionControl.RebuildControl();
							}

							break;
                        }
                }
            }
		} // WebPartCustomAction(object sender, CustomActionEventArgs e)

        /// <summary>
        /// Handle refreshing/reloading the task controls and container field after PagePanel execution
        /// that occurs when transaction tasks are executed successfully.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="serviceData"></param>
        public override void ChildPostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.ChildPostExecute(status, serviceData);
            bool isSignature = false;
            string specControlName = !IsResponsive ? "ContainerStatusDetail_Spec" : "ContainerStatus_SpecName";
            string specRevControlName = !IsResponsive ? "ContainerStatusDetails_SpecRevision" : "ContainerStatus_SpecRevision";
            //save the current spec value to be used for comparison later
            var specControl = Page.FindCamstarControl(specControlName) as InquiryControl;
            var specRevControl = Page.FindCamstarControl(specRevControlName) as InquiryControl;
            string currentSpec = string.Empty;
            if (specControl != null && specControl.Data != null && specRevControl != null && specRevControl.Data != null)
				currentSpec = $"{specControl.Data}{CamstarPortalSection.GetRevisionDelimiter()}{specRevControl.Data}";

			//Check to see if this function is firing from the ESig Capture page closing - if so then don't reload/refresh
			//this will be taken care of in the standard PostExecute override. This function is intended to handle PagePanel (transaction tasks)
			bool esigOrWipClose = false;
            UIComponentDataMember memberEsig = Page.DataContract.DataMembers.SingleOrDefault(m => m.Name == "ESigCaptureDetailsDM");
			/* CPR 367659:PR ? TAC ? Tasklist navigation
			 * when ESig popup will open on EProcedure page then IsESigAdded is set to true
			 * This session value is used in method DetermineStartTaskListAndTask() in TaskListSlideMenu.cs
			 * After task submit, If Advance to next task false and ESig is added to task.
             * then we required not to change SelectedNonSeqTaskId and SelectedNonSeqTaskListId session value
             * So added below condition, when isEsigAdded is true then do not reset SelectedNonSeqTaskId and SelectedNonSeqTaskListId session value 
             * and do not move to next task
			 */
			HttpContext.Current.Session["IsESigAdded"] = true;
			if (memberEsig != null && memberEsig.Value != null)
            {
                // If the ESignature popup was dispalyed but was not executed, Then check if an actual signature was entered.  Item1 is associated to the Task Esig Requirement.
                //esigClose = false;
                var esigDetails = ESigCaptureUtil.CollectESigServiceDetailsAll();				
				if (esigDetails.Item1 != null)
                {
					Camstar.WCF.ObjectStack.ESigPasswordCapture[] details = (Camstar.WCF.ObjectStack.ESigPasswordCapture[])esigDetails.Item1[0].CaptureDetails;
					if (details != null && details.Length > 0)
					{
						string Signer = details[0].Signer != null ? details[0].Signer.Name : string.Empty;
						if (string.IsNullOrEmpty(Signer))
						{
							Signer = details[0].Cosigner != null ? details[0].Cosigner.Name : string.Empty;
						}
						isSignature = Signer != null ? true : false;
						if (isSignature)
						{
							esigOrWipClose = true;
						}
					}
					//string Signer = ((Camstar.WCF.ObjectStack.ESigPasswordCapture[])esigDetails.Item1[0].CaptureDetails)[0].Signer.Name;
				}
                else
                {
                    if (esigDetails.Item2 != null)   //Item2 is associated to the Process timer Esig Requirement.
                    {
                        string SignerPT = esigDetails.Item2[0].ESigProcessTimerDtls[0].ESigReqDetail.Name;
                        isSignature = SignerPT != null ? true : false;
                        if (isSignature)
                        {
                            esigOrWipClose = true;
                        }
                    }
                }
            }

            if (!esigOrWipClose)
            {
                var wip = Page.DataContract.DataMembers.FirstOrDefault(m => m.Name == "WIPMessagesDM");
                esigOrWipClose = wip != null && wip.Value != null;
            }

            if (status.IsSuccess)
            {
                //Trigger task list reload if the child page submit was successful
                if (TaskListMenu != null)
                {
                    TaskListMenu.ClearMenuItems();					
						TaskListMenu.ForceReload = true;

					if (!esigOrWipClose)  /* Do not clear container ESig is required and is not closed */
                    {
						string selContainer = Page.DataContract.GetValueByName("SelectedContainerNameDM") as string;
						if (ContainerName == null)
							return;
						if (!string.IsNullOrEmpty(selContainer) || !ContainerName.IsEmpty)
                        {
                            string containerHolder = !ContainerName.IsEmpty ? ContainerName.Data.ToString() : selContainer;
                            ContainerName.ClearData();
                            ContainerName.Data = containerHolder;
                            ClearControls();

                            //
                            //  Begin Get Acutal Current Spec and Revision
                            //
                            if (!string.IsNullOrEmpty(currentSpec))
                            {
                                string newSpec = GetContainerInfo(currentSpec);
                                ////
                                // End Get Acutal Current Spec and Revision
                                //
                                //Check the Spec - if it has changed then clear the page (ie - the Container has moved to a new spec and no longer valid with this spec)
                                if (currentSpec != newSpec)
                                {
                                    ContainerName.ClearData();
                                }
                            }
                            base.DisplayMessage(status);
                        }
                    }
                }
            }
        } //ChildPostExecute
        protected virtual string GetContainerInfo(string currentSpec)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new WCF.Services.ContainerInfoInquiryService(session.CurrentUserProfile);
            var serviceData = new OM.ContainerInfoInquiry()
            {
                Container = new OM.ContainerRef(Convert.ToString(ContainerName.Data))
            };

            var request = new WCF.Services.ContainerInfoInquiry_Request();
            var result = new WCF.Services.ContainerInfoInquiry_Result();
            var resultStatus = new OM.ResultStatus();

            request.Info = new OM.ContainerInfoInquiry_Info
            {
                ContainerInfo = new OM.ContainerInfo_Info()
                {
                    CurrentStatus = new OM.CurrentStatus_Info()
                    {
                        Spec = new OM.Info(true)
                        
                    }
                }
            };

            resultStatus = service.ContainerInfoInquiry_GetContainerInfo(serviceData, request, out result);
            if (resultStatus != null && resultStatus.IsSuccess)
            {
                string newSpec = "";
                if (result.Value.ContainerInfo != null && result.Value.ContainerInfo.CurrentStatus != null)
                {
                    if (result.Value.ContainerInfo.CurrentStatus.Spec != null)
                    {
						newSpec = ControlUtil.GetDataValueAsString(result.Value.ContainerInfo.CurrentStatus.Spec);						
                        return newSpec;
                    }
                }
            }
            return currentSpec;
        }
        public virtual void RaisePostBackEvent(string eventArgument)
		{
			if (!String.IsNullOrEmpty(eventArgument))
			{

				if (WorkstationField != null)
					WorkstationField.ClearData();

				ClearWorkstationImage();
				SetWorkstation();

			} // if (!String.IsNullOrEmpty(eventArgument))           
        } // public virtual void RaisePostBackEvent(string eventArgument)
        protected virtual void ClearWorkstationImage()
		{
			var imageCtrl = Page.FindCamstarControl("ResourceAvailability_Image") as ImageControl;
			if (imageCtrl != null)
			{
				imageCtrl.ImagePath = imageCtrl.DefaultImage;
			}
		}
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (Page.Session["CurrentTheme"].ToString().ToLower() == "horizon" &&
                !(Page.FindCamstarControl("ActionsControl") as Camstar.WebPortal.PortalFramework.WebPartBase).CssClass.Contains("zero-opacity"))
                (Page.FindCamstarControl("ActionsControl") as Camstar.WebPortal.PortalFramework.WebPartBase).CssClass = "zero-opacity";
        }
        public override bool PreExecute(OM.Info serviceInfo, OM.Service serviceData)
		{
            var scrollCtrl = Page.FindCamstarControl("TaskMenu") as ScrollableMenu;
			var selectedTask = Page.FindCamstarControl("ExecuteTask_Task") as NamedSubentity;
			var selectedTaskList = Page.FindCamstarControl("ExecuteTask_TaskList") as RevisionedObject;

			if (HiddenContainer != null && scrollCtrl != null)
			{
				if (HiddenContainer.Data != null && selectedTask.Data != null && selectedTaskList.Data != null)
				{
					//Create the parent reference
					OM.RevisionedObjectRef parent = new OM.RevisionedObjectRef();
					parent = (OM.RevisionedObjectRef)selectedTaskList.Data;
					parent.CDOTypeName = "TaskList";

					//Set the Task
					OM.NamedSubentityRef taskItem = new OM.NamedSubentityRef();
					taskItem = (OM.NamedSubentityRef)selectedTask.Data;
					taskItem.Parent = parent;
					taskItem.CDOTypeName = null;

					if (serviceData.GetType() == typeof(OM.ExecuteTask))
					{
						switch (scrollCtrl.InstructionType)
						{
							case 1: //PassFail
								(serviceData as OM.ExecuteTask).ParametricData = null;
								(serviceData as OM.ExecuteTask).ParametricDataDef = null;
								(serviceData as OM.ExecuteTask).DataCollectionDef = null;
								break;

							case 2: //Data Collection
								(serviceData as OM.ExecuteTask).Pass = null;
								break;

							case 3: //Acknowledgment
								(serviceData as OM.ExecuteTask).Pass = null;
								(serviceData as OM.ExecuteTask).ParametricData = null;
								(serviceData as OM.ExecuteTask).ParametricDataDef = null;
								(serviceData as OM.ExecuteTask).DataCollectionDef = null;
								break;
							default:
								(serviceData as OM.ExecuteTask).Pass = null;
								break;
						}

						(serviceData as OM.ExecuteTask).TaskList = parent;
						(serviceData as OM.ExecuteTask).Task = taskItem;

					}
				}
			}

            return base.PreExecute(serviceInfo, serviceData);
		}
		public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
		{
			base.PostExecute(status, serviceData);

            if (status.IsSuccess)
			{
				if (TaskMenu != null && TaskListMenu != null)
				{
					//force a reload of the data to update the count/status of the task item					
					TaskListMenu.ForceReload = true;
					TaskListMenu.ClearMenuItems();
					TaskListMenu.LoadComplete();
					// These should happen during the TaskMenu Page_LoadComplete() event.
					//TaskMenu.InitialLoad = true;
					//TaskMenu.LoadComplete();
					TaskMenu.DidExecuteOccur = true;
					//refresh the container header values  
					if (ContainerName != null && !ContainerName.IsEmpty)
					{
						string containerHolder = ContainerName.Data.ToString();

						/* Added below If Condition to fix CPR 316951:2210.0002 PR223354 TAC10013388 - Nonsequential Task Lists
						 * If the AdvanceToNextTask has been set to false and Execution Mode of Task List is set to Nonsequential then unless the user selects the next task it should not move to the next Task List.                    
						 * So after adding below code when Container get reloaded, Unless the user selects the next task List control will not move to the next Task List.
						 */
						/* Updated below code to fix CPR 392791:PR 392553 TAC 10937418 Transaction throwing error for a task with the Cosign in V2304-02*/
						var selectedTask = Page.FindCamstarControl("ExecuteTask_Task") as NamedSubentity;
						var taskItem = (OM.NamedSubentityRef)selectedTask.Data;
						var selectedTaskId = !string.IsNullOrEmpty(taskItem.ID) ? taskItem.ID : "0";
						if (TaskListMenu.TaskResult.Select("TaskID = '" + selectedTaskId + "'").Length > 0 && TaskListMenu.TaskResult.Select("TaskID = '" + selectedTaskId + "'")[0]["AdvanceToNextTask"].ToString() == Boolean.FalseString.ToLower()
						&& Convert.ToInt16(TaskListMenu.TaskResult.Select("TaskID = '" + selectedTaskId + "'")[0]["TaskListExecutionMode"].ToString()) == 2)
						{				
							
							var selectedTaskListId = !string.IsNullOrEmpty(taskItem.Parent.ID) ? taskItem.Parent.ID : "0";
							HttpContext.Current.Session["SelectedNonSeqTaskId"] = Convert.ToString(selectedTaskId);
							HttpContext.Current.Session["SelectedNonSeqTaskListId"] = Convert.ToString(selectedTaskListId);
						}

						ContainerName.ClearData();
						ContainerName.Data = containerHolder;						
						UpdateWorkstation();
						ClearControls();
					}
				}

				if (ContainerName.SelectionData != null)
				{
					object temp = new Object();
					temp = ContainerName.SelectionData;
				}


				ClearControls();


            } // if (status.IsSuccess)
        } // public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)		

	} //public class EProcedure : MatrixWebPart, IPostBackEventHandler

} //namespace Camstar.WebPortal.WebPortlets.Shopfloor
