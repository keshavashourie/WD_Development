/* Copyright 2025 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using SEMI.AppCode;



/// <summary>
/// Summary description for SS_LotUnTerminate
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_LotUnTerminateEx : scsShopfloorBase
    {
        #region Properties

			protected CWC.NamedObject UnTerminateReason { get { return Page.FindCamstarControl("LotUnTerminate_UnTerminateReason") as CWC.NamedObject; } }
			protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotUnTerminate_SelectionIdField") as CWC.TextBox; } }
			protected JQDataGrid _gridContainers { get { return Page.FindCamstarControl("ContainersField") as JQDataGrid; } }
			protected CWC.TextBox txtComputerName { get { return Page.FindCamstarControl("LotUnTerminate_ComputerNameField") as CWC.TextBox; } }
			private DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("LotUnTerminate_SelectedLotsList") as DataEnvelopControl; } }
			protected Camstar.WebPortal.FormsFramework.WebControls.ContainerList containerSelectionValues { get { return Page.FindCamstarControl("LotUnTerminate_ContainerList") as Camstar.WebPortal.FormsFramework.WebControls.ContainerList; } }
			protected CWC.RevisionedObject unTerminatedWorkflow { get { return Page.FindCamstarControl("LotUnTerminate_UnTerminateWorkflow") as CWC.RevisionedObject; } }			
			protected CWC.NamedSubentity UnTerminateWorkflowStep { get { return Page.FindCamstarControl("LotUnTerminate_UnTerminateWorkflowStep") as CWC.NamedSubentity; } }

        #endregion
        
        #region Page Events

			/// <summary>
			/// This is an event that is fire when data is change in the SelectionId field is changed.
			/// </summary>
            public void SelectionIdField_DataChanged()
            {
                try
                {
                    if (_txtSelectionId.Data != null)
                    {
                        Page.StatusBar.ClearMessage();

                        // get the session and user profile
                        var fs = FrameworkManagerUtil.GetFrameworkSession();

                        // init the service, service data and service info objects

						LotUnTerminateService objSvc = new LotUnTerminateService(fs.CurrentUserProfile);
						LotUnTerminate objSvcData = new LotUnTerminate { SelectionId = (string)_txtSelectionId.Data };
						LotUnTerminate_Info objSvcInfo = new LotUnTerminate_Info { Containers = FieldInfoUtil.RequestValue() };

                        // init the result object
						LotUnTerminate_Result objResult = new LotUnTerminate_Result();

                        // execute to request the value
						ResultStatus resultStatus = objSvc.ResolveSelectionId(objSvcData, new LotUnTerminate_Request { Info = objSvcInfo }, out objResult);

                        if (resultStatus.IsSuccess)
                        {
                            JQDataGrid _gridContainers = Page.FindCamstarControl("ContainersField") as JQDataGrid;
                            foreach (OM.ContainerRef oContainer in objResult.Value.Containers)
                            {
                                // check if lot not exist in the grid else add to the grid
                                string strSelectedGridId = (_gridContainers.GridContext as BoundContext).GetRowIdByCellValue("Lot", oContainer.Name.ToString());

                                if (string.IsNullOrEmpty(strSelectedGridId))
                                {
									SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "LotUnTerminate", oContainer.Name.ToString(), false, ref _gridContainers, "ContainersField", true);
                                }
                            }
                        }
                        else
                            DisplayMessage(resultStatus);
                    }
                }
                catch (Exception Ex)
                {
                    DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
                }
                finally
                {
                    _txtSelectionId.TextControl.Text = "";
                    _txtSelectionId.Focus();
                }
            }

			/// <summary>
			/// This event is fired when the submit action button is clicked.
			/// </summary>
			/// <param name="serviceData"></param>
            public override void GetInputData(Service serviceData)
            {
                base.GetInputData(serviceData);

				if (_gridContainers.Data != null)
				{					
					OM.LotUnTerminate svcData = serviceData as OM.LotUnTerminate;

					int intTotalContainers = _gridContainers.BoundContext.GetTotalRows();

					svcData.Containers = new ContainerRef[intTotalContainers];

					// collect the containers
					for (int x = 0; x < intTotalContainers; x++)
					{						
						svcData.Containers[x] = new ContainerRef();
						svcData.Containers[x].Name = _gridContainers.GridContext.GetCell(x.ToString().PadLeft(6, '0'), "Lot").ToString();
					}
				}
            }

			/// <summary>
			/// On page load
			/// </summary>
			/// <param name="e"></param>
            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);


				if (string.IsNullOrEmpty(containerSelectionValues.TextEditControl.Text))
				{
					unTerminatedWorkflow.Enabled = false;
					UnTerminateWorkflowStep.Enabled = false;
				}
				else
				{
					unTerminatedWorkflow.Enabled = true;
					if (!string.IsNullOrEmpty(unTerminatedWorkflow.TextEditControl.Text))
					{
						UnTerminateWorkflowStep.Enabled = true;
						UnTerminateWorkflowStep.LabelControl.CssClass = UnTerminateWorkflowStep.LabelControl.CssClass.Replace($" cs-label-disabled", "");
					}
				}

				//_txtSelectionId.DataChanged += new EventHandler(_txtSelectionId_DataChanged);
				txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
				LotUnTerminate_Info lotUnTerminate_Info = new LotUnTerminate_Info();
				

				if (Page.IsPostBack)
					// Check if it is a pop up close, get the return result
					if (SEMI.AppCode.UIUtility.IsPopupClose(this))
					{
						if (_envSelectedLots != null)
							// manually initialize the containers list data contract since it is not triggered when we do a manual popup close
							if (Page.DataContract.GetValueByName("LotUnTerminate_SelectedLotsListDM") != null)
								_envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("LotUnTerminate_SelectedLotsListDM") as string[];

						if (_envSelectedLots.SS_ContainersList != null)
						{
							string[] sContainers;

							sContainers = _envSelectedLots.SS_ContainersList;


							containerSelectionValues.Data = new ContainerRef(sContainers[0]);
							JQDataGrid _gridContainers = Page.FindCamstarControl("ContainersField") as JQDataGrid;
							foreach (string sContainersItem in sContainers)
							{
								// check if lot not exist in the grid else add to the grid
								string strSelectedGridId = (_gridContainers.GridContext as BoundContext).GetRowIdByCellValue("Lot", sContainersItem);

								if (string.IsNullOrEmpty(strSelectedGridId))
								{
									SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "LotUnTerminate", sContainersItem, false, ref _gridContainers, "ContainersField", true);
								}
							}
							//nullify the containers list
							_envSelectedLots.SS_ContainersList = null;
						}
						
					}
					else
						if(_txtSelectionId.TextControl.Text != "")
						containerSelectionValues.Data = new ContainerRef(_txtSelectionId.TextControl.Text.ToUpper().Trim());
            }

			/// <summary>
			/// Resets the page value to default
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="e"></param>
			public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
			{
				base.WebPartCustomAction(sender, e);
				var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

				if (action != null && action.Parameters == "Reset")
				{
					Page.ClearValues();
					_gridContainers.ClearData();
					_txtSelectionId.Focus();
					containerSelectionValues.ClearData();
					OnLoad(null);
				}
			}

			/// <summary>
			/// Excutes after the submit button is clicked
			/// </summary>
			/// <param name="status"></param>
			/// <param name="serviceData"></param>
            public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
            {
                base.PostExecute(status, serviceData);
				if (status.IsSuccess)
				{
					Page.ClearValues();
					_gridContainers.ClearData();
					_txtSelectionId.Focus();
					containerSelectionValues.ClearData();
				}
				OnLoad(null);
            }

        #endregion

    }
}



