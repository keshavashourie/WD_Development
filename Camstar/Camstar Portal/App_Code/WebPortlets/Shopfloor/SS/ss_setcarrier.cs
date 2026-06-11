/* Copyright 2019 Siemens */
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Data;
using System.Web;
using System.Linq;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using System;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.PortalFramework;

/// <summary>
/// The code for the Set Carrier virtual page.
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
	public class SS_SetCarrier : scsShopfloorBase
	{

		# region Methods

		/// <summary>
		/// constructor for the SS_SetCarrier class
		/// </summary>
		public SS_SetCarrier()
		{
			
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if(!Page.IsPostBack)
				ComputerNameTextBox.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
			else
				if (SEMI.AppCode.UIUtility.IsPopupClose(this))
				{

					if (Page.DataContract.GetValueByName("LotListDM") != null)
					{
						//gets the list of returned lot from the popup form
						string[] sContainers = Page.DataContract.GetValueByName("LotListDM") as string[];
						JQDataGrid _gridContainers = Page.FindCamstarControl("ContainersField") as JQDataGrid;
						foreach (string sContainersItem in sContainers)
						{
							// check if lot not exist in the grid else add to the grid
							string strSelectedGridId = (_gridContainers.GridContext as BoundContext).GetRowIdByCellValue("Lot", sContainersItem);

							if (string.IsNullOrEmpty(strSelectedGridId))
							{
								//setting them to the grid
								SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "SetCarrier", sContainersItem, false, ref _gridContainers, "ContainersField", true);
							}
						}

						Page.DataContract.SetValueByName("LotListDM", null);
					}


					if(Page.DataContract.GetValueByName("RetCarrierValueDM") != null)
					{
						NamedObjectRef carrier = new NamedObjectRef();

						carrier.Name=	Page.DataContract.GetValueByName<string>("RetCarrierValueDM");
						Page.DataContract.SetValueByName("RetCarrierValueDM", null);

						CarrierControl.Data = carrier;

					}
				}
		}

		/// <summary>
		/// Triggered when a lot / carrier etc... is entered on the page
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void SelectionIdField_DataChanged()
		{
			try
			{
				if (SelectionIdTextBox.Data != null)
				{
					Page.StatusBar.ClearMessage();

					// get the session and user profile
					var fs = FrameworkManagerUtil.GetFrameworkSession();

					// init the service, service data and service info objects
					SetCarrierService objSvc = new SetCarrierService(fs.CurrentUserProfile);
					SetCarrier objSvcData = new SetCarrier { SelectionId = (string)SelectionIdTextBox.Data };
					SetCarrier_Info objSvcInfo = new SetCarrier_Info { Containers = FieldInfoUtil.RequestValue() };

					// init the result object
					SetCarrier_Result objResult = new SetCarrier_Result();

					// execute to request the value
					ResultStatus resultStatus = objSvc.ResolveSelectionId(objSvcData, new SetCarrier_Request { Info = objSvcInfo }, out objResult);

					if (resultStatus.IsSuccess)
					{
						JQDataGrid _gridContainers = Page.FindCamstarControl("ContainersField") as JQDataGrid;
						foreach (OM.ContainerRef oContainer in objResult.Value.Containers)
						{
							// check if lot not exist in the grid else add to the grid
							string strSelectedGridId = (_gridContainers.GridContext as BoundContext).GetRowIdByCellValue("Lot", oContainer.Name.ToString());

							if (string.IsNullOrEmpty(strSelectedGridId))
							{
								SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "SetCarrier", oContainer.Name.ToString(), false, ref _gridContainers, "ContainersField", true);
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
				SelectionIdTextBox.TextControl.Text = "";
				SelectionIdTextBox.Focus();
			}
		}

		/// <summary>
		/// This event is fired when the submit action button is clicked.
		/// </summary>
		/// <param name="serviceData"></param>
		public override void GetInputData(Service serviceData)
		{
			//SetCarrier svcData = new SetCarrier();
			base.GetInputData(serviceData);
			if (ContainersFieldGrid.Data != null)
			{
				
				int intTotalContainers = ContainersFieldGrid.BoundContext.GetTotalRows();

				(serviceData as SetCarrier).Carrier = (NamedObjectRef)CarrierControl.Data;
				(serviceData as SetCarrier).Containers = new ContainerRef[intTotalContainers];

				// collect the containers to be submitted to the server for processing. Change of status to terminated.
				for (int x = 0; x < intTotalContainers; x++)
				{
					(serviceData as SetCarrier).Containers[x] = new ContainerRef();
					(serviceData as SetCarrier).Containers[x].Name = ContainersFieldGrid.GridContext.GetCell(x.ToString().PadLeft(6, '0'), "Lot").ToString();

				}
			}

		}



		/// <summary>
		/// simply clears the grid
		/// </summary>
		private void ClearLotDetailsGrid()
		{
			//Whack any previous data
			ContainersFieldGrid.ClearData();
			_toggleComments.Reset();

			// Whack the headers, and add back the magical space col (for looks)
			if (ContainersFieldGrid.BoundContext.Fields.Count > 0)
			{
				JQFieldCollection objFieldClear = new JQFieldCollection();
				ContainersFieldGrid.BoundContext.Fields = objFieldClear;
				ContainersFieldGrid.BoundContext.Fields.Add(new JQField("_spacer") { LabelText = "&nbsp;", Visible = true });
			}
		}

		/// <summary>
		/// Clears the query grid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
		{
			base.WebPartCustomAction(sender, e);

			ClearLotDetailsGrid();
			DisplayMessage(new ResultStatus("", true));
			
		} // WebPartCustomAction(object sender, CustomActionEventArgs e)

		/// <summary>
		/// Just tidies up the form
		/// </summary>
		/// <param name="status"></param>
		/// <param name="serviceData"></param>
		public override void PostExecute(ResultStatus status, Service serviceData)
		{
			base.PostExecute(status, serviceData);

			if (status.IsSuccess == true)
			{
				ClearLotDetailsGrid();
				CarrierControl.ClearData();
				CommentsTextBox.ClearData();
				
			}
		}

#endregion

		#region PrivateProperites

		private CWC.TextBox SelectionIdTextBox { get { return FindCamstarControl("SelectionIdTextBox") as CWC.TextBox; } }
		private CWC.TextBox ComputerNameTextBox	{get { return FindCamstarControl("SetCarrier_ComputerName") as CWC.TextBox; }	}
		private JQDataGrid ContainersFieldGrid	{	get { return FindCamstarControl("ContainersField") as JQDataGrid; }	}
		private SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("ContainersList") as SEMI.AppCode.DataEnvelopControl; } }
		private CWC.NamedObject CarrierControl { get { return FindCamstarControl("SetCarrier_Carrier") as CWC.NamedObject; } }
		private CWC.TextBox CommentsTextBox { get { return Page.FindCamstarControl("Shopfloor_Comments") as CWC.TextBox; } }
		private ToggleContainer _toggleComments { get { return Page.FindCamstarControl("CommentsToggleControl") as ToggleContainer; } } 
		#endregion
	}
		
}



