/* Copyright 2019 Siemens */
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
	public class SS_LotScheduleRelease : scsShopfloorBase
    {
        #region Properties

		protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotScheduleRelease_SelectionIdField") as CWC.TextBox; } }
		protected JQDataGrid _gridContainers { get { return Page.FindCamstarControl("ContainersField") as JQDataGrid; } }
		protected CWC.TextBox txtComputerName { get { return Page.FindCamstarControl("LotScheduleRelease_ComputerNameField") as CWC.TextBox; } }
		private SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("LotScheduleRelease_SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }
        
		#endregion
        
        #region Page Events
		
			/// <summary>
			/// This event is fired when the submit action button is clicked.
			/// </summary>
			/// <param name="serviceData"></param>
            public override void GetInputData(Service serviceData)
            {
                base.GetInputData(serviceData);
				if (_gridContainers.Data != null)
				{
					OM.LotScheduleRelease svcData = serviceData as OM.LotScheduleRelease;
					int intTotalContainers = _gridContainers.BoundContext.GetTotalRows();

					svcData.Containers = new ContainerRef[intTotalContainers];

					string AllContainerName = "";
					// collect the containers to be submitted to the server for processing. Change of status to terminated.
					for (int x = 0; x < intTotalContainers; x++)
					{
						svcData.Containers[x] = new ContainerRef();
						svcData.Containers[x].Name = _gridContainers.GridContext.GetCell(x.ToString().PadLeft(6, '0'), "Lot").ToString();
						
						if (x == 0)
						{
							AllContainerName = AllContainerName + _gridContainers.GridContext.GetCell(x.ToString().PadLeft(6, '0'), "Lot").ToString();
						} 
						else
							AllContainerName = AllContainerName + "|" + _gridContainers.GridContext.GetCell(x.ToString().PadLeft(6, '0'), "Lot").ToString(); 
					}

					if (PrimaryServiceType.Equals("scsDBLotScheRelease"))
					{
						(serviceData as Camstar.WCF.ObjectStack.scsDBLotScheRelease).ContainerNamesStr = AllContainerName;
					}
				}
            }

			/// <summary>
			/// On page load
			/// </summary>
			/// <param name="e"></param>
            protected override void OnLoad(EventArgs e)
            {
				DisplayMessage(new ResultStatus("", true));
                base.OnLoad(e);

				//_txtSelectionId.DataChanged += new EventHandler(_txtSelectionId_DataChanged);
				txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
				
				if (Page.IsPostBack)
					// Check if it is a pop up close, get the return result
					if (SEMI.AppCode.UIUtility.IsPopupClose(this))
					{
						if (_envSelectedLots != null)
							// manually initialize the containers list data contract since it is not triggered when we do a manual popup close
							if (Page.DataContract.GetValueByName("LotScheduleRelease_SelectedLotsListDM") != null)
							{
								//gets the list of returned lot from the popup form
								_envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("LotScheduleRelease_SelectedLotsListDM") as string[];
							}

						if (_envSelectedLots.SS_ContainersList != null)
						{
							string[] sContainers;
							sContainers = _envSelectedLots.SS_ContainersList;

							JQDataGrid _gridContainers = Page.FindCamstarControl("ContainersField") as JQDataGrid;
							foreach (string sContainersItem in sContainers)
							{
								// check if lot not exist in the grid else add to the grid
								string strSelectedGridId = (_gridContainers.GridContext as BoundContext).GetRowIdByCellValue("Lot", sContainersItem);

								if (string.IsNullOrEmpty(strSelectedGridId))
								{
									//setting them to the grid
									SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "LotScheduleRelease", sContainersItem, false, ref _gridContainers, "ContainersField", true);
								}
							}
							//nullify the containers list
							_envSelectedLots.SS_ContainersList = null;
						}
					}
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
					ClearData();
                    Page.ShopfloorReset(sender, e);
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
					ClearData();		
				}
				OnLoad(null);
            }

        #endregion

		#region Private Methods

			/// <summary>
			/// Clears data from the form
			/// </summary>
			private void ClearData()
			{
				Page.ClearValues();
				_gridContainers.ClearData();
				_txtSelectionId.Focus();				
				OnLoad(null);
			}

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
						
						LotScheduleReleaseService objSvc = new LotScheduleReleaseService(fs.CurrentUserProfile);
						LotScheduleRelease objSvcData = new LotScheduleRelease { SelectionId = (string)_txtSelectionId.Data };
						LotScheduleRelease_Info objSvcInfo = new LotScheduleRelease_Info { Containers = FieldInfoUtil.RequestValue() };

						// init the result object
						LotScheduleRelease_Result objResult = new LotScheduleRelease_Result();

						// execute to request the value
						ResultStatus resultStatus = objSvc.ResolveSelectionId(objSvcData, new LotScheduleRelease_Request { Info = objSvcInfo }, out objResult);

						if (resultStatus.IsSuccess)
						{
							JQDataGrid _gridContainers = Page.FindCamstarControl("ContainersField") as JQDataGrid;
							foreach (OM.ContainerRef oContainer in objResult.Value.Containers)
							{
								// check if lot not exist in the grid else add to the grid
								string strSelectedGridId = (_gridContainers.GridContext as BoundContext).GetRowIdByCellValue("Lot", oContainer.Name.ToString());

								if (string.IsNullOrEmpty(strSelectedGridId))
								{
									SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "LotScheduleRelease", oContainer.Name.ToString(), false, ref _gridContainers, "ContainersField", true);
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

		#endregion

	}
}



