/* Copyright 2022 Siemens */
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
	public class SS_LotSchedulePrep : scsShopfloorBase
	{
		#region Properties

		protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotSchedulePrep_SelectionId") as CWC.TextBox; } }		
		protected CWC.TextBox txtComputerName { get { return Page.FindCamstarControl("LotSchedulePrep_ComputerNameField") as CWC.TextBox; } }
		protected Camstar.WebPortal.FormsFramework.WebControls.ContainerList containerSelectionValues { get { return Page.FindCamstarControl("ContainerField") as Camstar.WebPortal.FormsFramework.WebControls.ContainerList; } }
		protected CWC.NamedSubentity _subScheduleData { get { return Page.FindCamstarControl("LotSchedulePrep_ScheduleData") as CWC.NamedSubentity; } }
		SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("LotSchedulePrep_SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }
		protected CWC.DateChooser DueDate { get { return Page.FindCamstarControl("LotSchedulePrep_DueDate") as CWC.DateChooser; } }

		#endregion

		#region DataChangeEvents

		/// <summary>
		/// Gets Schedule data to bind data to dropdown
		/// </summary>
		public void GetScheduleData()
		{
			try
			{
				if (_txtSelectionId.Data != null)
				{
					FrameworkSession fs = FrameworkManagerUtil.GetFrameworkSession();
					LotSchedulePreparationService svc = new LotSchedulePreparationService(fs.CurrentUserProfile);
					LotSchedulePreparation txn = new LotSchedulePreparation();
					LotSchedulePreparation_Info txnInfo = new LotSchedulePreparation_Info();

					//request of data to be retrived.	
					txn.Container = new ContainerRef(_txtSelectionId.TextControl.Text);

					//What field would we like data
					txnInfo.ScheduleData = new OM.Info();
					txnInfo.ScheduleData.RequestSelectionValues = true;

					//Request object
					LotSchedulePreparation_Request req = new LotSchedulePreparation_Request();
					//Result object
					LotSchedulePreparation_Result res = new LotSchedulePreparation_Result();

					req.Info = txnInfo;

					ResultStatus rs = svc.GetEnvironment(txn, req, out res);

					if (rs.IsSuccess)
					{
					    if (res.Environment.ScheduleData.SelectionValues != null)
						{						
							_subScheduleData.DropDownControl.Items.Clear();
							if (res.Environment.ScheduleData.SelectionValues.Rows != null)
							{
								foreach (Row data in res.Environment.ScheduleData.SelectionValues.Rows)
								{
									_subScheduleData.DropDownControl.Items.Add(data.Values[0].ToString());
								}
								ScheduleData_DataChanged();
							}
							else
							{
								ResultStatus errorMessage = new ResultStatus(_txtSelectionId.TextControl.Text + ": is not a vaild lot or has not been scheduled!", false);
								DisplayMessage(errorMessage);
							}							
						}
						else 
						{
							ResultStatus errorMessage1 = new ResultStatus(_txtSelectionId.TextControl.Text + ": is not a valid lot or has not been scheduled!", false);
							DisplayMessage(errorMessage1);
						}						
					}						
					else
					{
						DisplayMessage(rs);
					}
				}
			}
			catch (Exception Ex)
			{
				DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
			}
			finally
			{
				_txtSelectionId.Focus();
			}
		}

		/// <summary>
		/// This is an event that is fire when data is change in the SelectionId field is changed.
		/// </summary>
		public void ScheduleData_DataChanged()
		{

			try
			{
				if (_txtSelectionId.Data != null && _subScheduleData.Data != null)
				{
					//clear the clear message
					Page.StatusBar.ClearMessage();

					// get the session and user profile
					var fs = FrameworkManagerUtil.GetFrameworkSession();

					// Get the service type of the page
					string sServiceType = Page.PrimaryServiceType;

					// Run proper constructor. We need to be dynamic with the primary service type
					var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
					//create a request object
					var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
					var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
					var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

					//retriving data dynamically for Flexibility of the page.
					var oServiceData = CreateServiceData(sServiceType);

					var info = CreateServiceInfo(sServiceType);
					var oServiceInfo = info as LotSchedulePreparation_Info;
					(oRequest as Request).Info = oServiceInfo;

					//setting the schedulingData id for retrival of data.
					(oServiceData as LotSchedulePreparation).ScheduleData = new SubentityRef();
					int i = _subScheduleData.DropDownControl.SelectedIndex;
					(oServiceData as LotSchedulePreparation).ScheduleData.ID = _subScheduleData.DropDownControl.Items[i].Text;

					//request of data to be retrived.
					oServiceInfo.Product = FieldInfoUtil.RequestValue();
					oServiceInfo.MfgOrder = FieldInfoUtil.RequestValue();
					oServiceInfo.ProductBOM = FieldInfoUtil.RequestValue();
					oServiceInfo.ProcessSpec = FieldInfoUtil.RequestValue();
					oServiceInfo.FirstWIPStep = FieldInfoUtil.RequestValue();
					oServiceInfo.Owner = FieldInfoUtil.RequestValue();
					oServiceInfo.ShipToFactory = FieldInfoUtil.RequestValue();
					oServiceInfo.ExpectedStartDate = FieldInfoUtil.RequestValue();
					oServiceInfo.ExpectedEndDate = FieldInfoUtil.RequestValue();
					oServiceInfo.CycleTime = FieldInfoUtil.RequestValue();
					oServiceInfo.Priority = FieldInfoUtil.RequestValue();
					oServiceInfo.PackingType = FieldInfoUtil.RequestValue();
					oServiceInfo.ExternalComments = FieldInfoUtil.RequestValue();
					oServiceInfo.SalesOrderNumber = FieldInfoUtil.RequestValue();
					oServiceInfo.NewLotId = FieldInfoUtil.RequestValue();
					oServiceInfo.AutoSetNewLotId = FieldInfoUtil.RequestValue();
					oServiceInfo.Comments = FieldInfoUtil.RequestValue();
					oServiceInfo.Containers = FieldInfoUtil.RequestValue();

					oServiceInfo.DetailsSelection = new SchedulingTxnDetails_Info();
					oServiceInfo.DetailsSelection.Container = FieldInfoUtil.RequestValue();
					oServiceInfo.DetailsSelection.ActualScheduleQty = FieldInfoUtil.RequestValue();
					oServiceInfo.DetailsSelection.ScheduleQty = FieldInfoUtil.RequestValue();
					oServiceInfo.DetailsSelection.ScheduleResidueQty = FieldInfoUtil.RequestValue();
					oServiceInfo.DetailsSelection.LotQty = FieldInfoUtil.RequestValue();
					oServiceInfo.DetailsSelection.IsPartialLot = FieldInfoUtil.RequestValue();
					oServiceInfo.WafersSelection = new SchedulingTxnWafers_Info();
					oServiceInfo.WafersSelection.Container = FieldInfoUtil.RequestValue();
					//oServiceInfo.WafersSelection.LotWafersItem = FieldInfoUtil.RequestValue();
					oServiceInfo.WafersSelection.WaferScribeNumber = FieldInfoUtil.RequestValue();
					oServiceInfo.WafersSelection.NDPW = FieldInfoUtil.RequestValue();
					oServiceInfo.WafersSelection.GoodQty = FieldInfoUtil.RequestValue();


					// init the result object
					Result oResult = new Result();

					// execute to request the value
					ResultStatus resultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oResult);

					if (resultStatus.IsSuccess)
					{
						DisplayValues((oResult.Value as LotSchedulePreparation));

						//Population of grid
						JQDataGrid DetailsGrid = Page.FindCamstarControl("LotSchedulePreparation_Details") as JQDataGrid;
						DetailsGrid.ClearData();
						if ((oResult.Value as LotSchedulePreparation).DetailsSelection != null)
						{
							//Binding the Data to the Grid						
							(DetailsGrid.GridContext as BoundContext).Data = (oResult.Value as LotSchedulePreparation).DetailsSelection.ToArray();
							DetailsGrid.BoundContext.LoadData();
							//Rendering the Grid with data
							CamstarWebControl.SetRenderToClient(DetailsGrid);
						}

						//Population of grid
						JQDataGrid WafersGrid = Page.FindCamstarControl("LotSchedulePreparation_Wafers") as JQDataGrid;
						WafersGrid.ClearData();
						if ((oResult.Value as LotSchedulePreparation).WafersSelection != null)
						{
							//Binding the Data to the Grid						
							(WafersGrid.GridContext as BoundContext).Data = (oResult.Value as LotSchedulePreparation).WafersSelection.ToArray();
							WafersGrid.BoundContext.LoadData();
							//Rendering the Grid with data
							CamstarWebControl.SetRenderToClient(WafersGrid);
                            //sets all rows to be selected by default
                            int countRows = (oResult.Value as LotSchedulePreparation).WafersSelection.Length; //WafersGrid.GridContext.DataWindow.Rows.Count;

                            for (int row = 0; row < countRows; row++)
							{
                                string sRowId = row.ToString().PadLeft(6, '0');
                                //_gridItemData.Action_SelectRow(sRowId, "select");
                                WafersGrid.GridContext.SelectRow(sRowId, true);
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
				_txtSelectionId.Focus();
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Clears data from the form
		/// </summary>
		private void ClearData()
		{
			JQDataGrid WafersGrid = Page.FindCamstarControl("LotSchedulePreparation_Wafers") as JQDataGrid;
			JQDataGrid DetailsGrid = Page.FindCamstarControl("LotSchedulePreparation_Details") as JQDataGrid;
			Page.ClearValues();
			WafersGrid.ClearData();
			DetailsGrid.ClearData();
			_txtSelectionId.Focus();
			containerSelectionValues.ClearData();			
			_subScheduleData.DropDownControl.Items.Clear();
			_envSelectedLots.ClearData();
			OnLoad(null);
		}

		/// <summary>
		/// Updates Details Grid with ActualSchedule Qty values
		/// </summary>
		public void GridUpdatedGridQty()
		{
			JQDataGrid DetailsGrid = Page.FindCamstarControl("LotSchedulePreparation_Details") as JQDataGrid;
			SchedulingTxnDetails[] aDetails = (DetailsGrid.GridContext as BoundContext).Data as SchedulingTxnDetails[];

			JQDataGrid WafersGrid = Page.FindCamstarControl("LotSchedulePreparation_Wafers") as JQDataGrid;
			SchedulingTxnWafers[] aWafers = (WafersGrid.GridContext as BoundContext).Data as SchedulingTxnWafers[];
			double dTotalNDPW = 0;
			double dTotalGoodQty = 0;


			if (WafersGrid.IsRowSelected)
			{
				int detailCount = 0;
				var wafersList = WafersGrid.GridContext.GetSelectedItems(false);
				foreach (SchedulingTxnDetails detail in aDetails)
				{
					dTotalNDPW = 0;
					dTotalGoodQty = 0;

					foreach (SchedulingTxnWafers wafer in wafersList)
					{
						if (detail.Container.Name == wafer.Container.Name)
						{
							if (wafer.NDPW.Value != null && wafer.NDPW.Value > 0)
								dTotalNDPW = dTotalNDPW + wafer.NDPW.Value;
							if (wafer.GoodQty.Value != null && wafer.GoodQty.Value > 0)
								dTotalGoodQty = dTotalGoodQty + wafer.GoodQty.Value;
						}
					}//end of foreach
					if (dTotalGoodQty > 0)
					{
						aDetails[detailCount].ActualScheduleQty.Value = dTotalGoodQty;
					}
					else if (dTotalNDPW > 0)
					{
						aDetails[detailCount].ActualScheduleQty.Value = dTotalNDPW;
					}
					detailCount++;
				}//end of foreach

				//Binding the Data to the Grid						
				(DetailsGrid.GridContext as BoundContext).Data = aDetails;
				DetailsGrid.BoundContext.LoadData();
				//Rendering the Grid with data
				CamstarWebControl.SetRenderToClient(DetailsGrid);
			}
		}

		#endregion

		#region Page Events
				

		/// <summary>
		/// This event is fired when the submit action button is clicked.
		/// </summary>
		/// <param name="serviceData"></param>
		public override void GetInputData(Service serviceData)
		{
			base.GetInputData(serviceData);

			try
			{				
				JQDataGrid WafersGrid = Page.FindCamstarControl("LotSchedulePreparation_Wafers") as JQDataGrid;
				SchedulingTxnWafers[] allWafers = (WafersGrid.GridContext as BoundContext).Data as SchedulingTxnWafers[];				
				

				var selectedWafers = WafersGrid.GridContext.GetSelectedItems(false);
				
				if (selectedWafers != null)
				{

                    selectedWafers.Cast<SchedulingTxnWafers>().ToArray();
					int waferIndex = 0;
					SchedulingTxnWafers[] newWafers = new SchedulingTxnWafers[selectedWafers.Length];
					
					foreach (SchedulingTxnWafers wafer in selectedWafers)
					{
						newWafers[waferIndex] = new SchedulingTxnWafers();
						newWafers[waferIndex].ListItemAction = ListItemAction.Add;
						newWafers[waferIndex].Container = wafer.Container;
						newWafers[waferIndex].GoodQty = wafer.GoodQty;
						newWafers[waferIndex].LotWafersItem = wafer.LotWafersItem;
						newWafers[waferIndex].NDPW = wafer.NDPW;
						newWafers[waferIndex].WaferScribeNumber = wafer.WaferScribeNumber;
						waferIndex++;
					}
					(serviceData as LotSchedulePreparation).Wafers = newWafers;
				}

				(serviceData as LotSchedulePreparation).Container = new ContainerRef(_txtSelectionId.TextControl.Text);
				//Get the input data
				base.GetInputData(serviceData);
				(serviceData as LotSchedulePreparation).ScheduleData = new SubentityRef();
				(serviceData as LotSchedulePreparation).ScheduleData.ID = _subScheduleData.DropDownControl.SelectedValue;
				(serviceData as LotSchedulePreparation).DueDate = Convert.ToDateTime(DueDate.Data.ToString());

				JQDataGrid DetailsGrid = Page.FindCamstarControl("LotSchedulePreparation_Details") as JQDataGrid;
				SchedulingTxnDetails[] aDetails = (DetailsGrid.GridContext as BoundContext).Data as SchedulingTxnDetails[];
				SchedulingTxnDetails[] newDetails = new SchedulingTxnDetails[aDetails.Length];			

				if (aDetails != null)
				{
					int detailIndex = 0;
					foreach (SchedulingTxnDetails detail in aDetails)
					{
						newDetails[detailIndex] = new SchedulingTxnDetails();
						newDetails[detailIndex].ListItemAction = ListItemAction.Add;
						newDetails[detailIndex].ActualScheduleQty = detail.ActualScheduleQty;
						newDetails[detailIndex].Container = detail.Container;
						newDetails[detailIndex].IsPartialLot = detail.IsPartialLot;
						newDetails[detailIndex].LotQty = detail.LotQty;
						newDetails[detailIndex].ScheduleQty = detail.ScheduleQty;
						newDetails[detailIndex].ScheduleResidueQty = detail.ScheduleResidueQty;
						detailIndex++;
					}
					(serviceData as LotSchedulePreparation).Details = newDetails;
				}
				//Binding Orginal data back to the grid						
				(WafersGrid.GridContext as BoundContext).Data = allWafers.ToArray();
				WafersGrid.BoundContext.LoadData();
				//Rendering the Grid with data
				CamstarWebControl.SetRenderToClient(WafersGrid);
			}
			catch (Exception Ex)
			{
				DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
			}
		}

		/// <summary>
		/// On page load
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			//Clear Display message
			DisplayMessage(new ResultStatus("", true));

			// Get Computername
			txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);

			if (Page.IsPostBack)

				// Check if it is a pop up close, get the return result
				if (SEMI.AppCode.UIUtility.IsPopupClose(this))
				{
					if (_envSelectedLots != null)

						// manually initialize the containers list data contract since it is not triggered when we do a manual popup close
						if (Page.DataContract.GetValueByName("LotSchedule_SelectedLotsListDM") != null)
						{
							//gets the list of returned lot from the popup form
							_envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("LotSchedule_SelectedLotsListDM") as string[];
						}

					if (_envSelectedLots.SS_ContainersList != null)
					{
						string[] sContainers;
						sContainers = _envSelectedLots.SS_ContainersList;
						_txtSelectionId.TextControl.Text = sContainers[0].ToString();
						GetScheduleData();
					}
					//nullify the containers list
					_envSelectedLots.SS_ContainersList = null;
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

	}
}



