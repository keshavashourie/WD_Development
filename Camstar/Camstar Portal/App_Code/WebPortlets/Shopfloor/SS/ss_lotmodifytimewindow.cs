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
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework.WebControls;


/// <summary>
/// The code for the move non standard virtual page.  Resolves the lot based on the 
/// selection id entered and populates the lot details grid.
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
	public class SS_LotModifyTimeWindow : MatrixWebPart
	{
		#region Properties

		protected JQDataGrid _gridContainers { get { return Page.FindCamstarControl("LotGrid") as JQDataGrid; } }
		protected JQDataGrid _gridMaxDetails { get { return Page.FindCamstarControl("LotModifyTimeWindow_MaxDetails") as JQDataGrid; } }
		protected JQDataGrid _gridMinDetails { get { return Page.FindCamstarControl("LotModifyTimeWindow_MinDetails") as JQDataGrid; } }


		private SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("LotModifyTimeWindow_SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }
		protected CWC.TextBox txtComputerName { get { return Page.FindCamstarControl("LotModifyTimeWindow_ComputerNameField") as CWC.TextBox; } }
		protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotModifyTimeWindow_SelectionIdField") as CWC.TextBox; } }
		protected ContainerList _ContainerList { get { return Page.FindCamstarControl("LotModifyTimeWindow_Container") as ContainerList; } }
		private ToggleContainer _toggleComments { get { return Page.FindCamstarControl("Control") as ToggleContainer; } }
		ContainerRef containerRef;
		bool isProcessTypeValue = false;
		int count = 0;


		#endregion

		#region DataChangeEvents

		/// <summary>
		/// This is an event that is fire when data is change in the SelectionId field is changed.
		/// </summary>
		public void SelectionIdField_DataChanged()
		{
			try
			{
				if (_txtSelectionId.Data != null)
				{
					_ContainerList.ClearData();
					_ContainerList.Data = new ContainerRef(_txtSelectionId.TextControl.Text);
					SetGridData(_txtSelectionId.Data.ToString(), _gridContainers);
					CamstarWebControl.SetRenderToClient(_gridContainers);
					GetData(_txtSelectionId.Data.ToString());
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
		/// Gets data for form based off of return container(s)
		/// </summary>
		/// <param name="lot">string lot name</param>
		private void GetData(string lot)
		{
			try
			{//Clear Display message
				DisplayMessage(new ResultStatus("", true));

				OM.LotModifyTimeWindow_Info oServiceInfo = new OM.LotModifyTimeWindow_Info();
				LotModifyTimeWindow_Result resultData = new LotModifyTimeWindow_Result();
				UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
				LotModifyTimeWindowService svc = new LotModifyTimeWindowService(profile);
				LotModifyTimeWindow oServiceData = new LotModifyTimeWindow();
				LotModifyTimeWindow_Request reqData = new LotModifyTimeWindow_Request();


				oServiceData.SelectionId = lot;
				oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
				oServiceInfo.MinCurrentDetails = new MinTimeWindowDetails_Info();
				oServiceInfo.MaxCurrentDetails = new MaxTimeWindowDetails_Info();

				oServiceInfo.MinCurrentDetails.RequestValue = true;
				oServiceInfo.MaxCurrentDetails.RequestValue = true;

				reqData.Info = oServiceInfo;
				ResultStatus results = svc.ResolveSelectionId(oServiceData, reqData, out resultData);

				if (results.IsSuccess)
				{
					clearData(false);
					DisplayValues(resultData.Value);

					///Selection Textbox
					if (resultData.Value.SelectionContainer != null)
						_txtSelectionId.TextControl.Text = resultData.Value.SelectionContainer.Name;

					///Min Details grid 
					if (resultData.Value.MinCurrentDetails != null)
					{

						if (resultData.Value.MinCurrentDetails != null)
						{
							///Gets Lot Materials array
							(_gridMinDetails.GridContext as BoundContext).Data = resultData.Value.MinCurrentDetails.ToArray();
							_gridMinDetails.GridContext.LoadData();
							CamstarWebControl.SetRenderToClient(_gridMinDetails);
						}
					}
					///Max Details grid 
					if (resultData.Value.MaxCurrentDetails != null)
					{
						(_gridMaxDetails.GridContext as BoundContext).Data = resultData.Value.MaxCurrentDetails.ToArray();
						_gridMaxDetails.GridContext.LoadData();
						CamstarWebControl.SetRenderToClient(_gridMaxDetails);
					}
					_ContainerList.Data = new ContainerRef(lot);
				}
				else
				{
					DisplayMessage(results);
				}

			}
			catch (Exception ex)
			{
				DisplayMessage(new ResultStatus(ex.Message, false));
			}
		}

		/// <summary>
		/// Clears data from the form.
		/// </summary>
		private void clearData(bool Allvalues)
		{
			if (Allvalues)
			{
				Page.ClearValues();
				_gridContainers.ClearData();
			}
			_gridMaxDetails.ClearData();
			_gridMinDetails.ClearData();
			_toggleComments.Reset();
			_txtSelectionId.Focus();
		}

		/// <summary>
		/// Set data for the lot grid
		/// </summary>
		/// <param name="sContainersItem">Container to insert</param>
		/// <param name="_gridContainersTemp">grid host</param>
		private void SetGridData(string sContainersItem, JQDataGrid _gridContainersTemp)
		{
			// check if lot not exist in the grid else add to the grid
			string strSelectedGridId = (_gridContainersTemp.GridContext as BoundContext).GetRowIdByCellValue("Lot", sContainersItem);

			if (string.IsNullOrEmpty(strSelectedGridId))
			{
				_gridContainersTemp.ClearData();
				//setting them to the grid
				SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "LotModifyTimeWindow", sContainersItem, false, ref _gridContainersTemp, "LotGrid", true);//_gridContainersTemp.LabelName for lotGrid string
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
			(serviceData as LotModifyTimeWindow).MinDetails = inputMinDetails();

			(serviceData as LotModifyTimeWindow).MaxDetails = inputMaxDetails();

			base.GetInputData(serviceData);
		}

		/// <summary>
		/// Gets the correct format to submit to the server
		/// </summary>
		/// <returns>MinTimeWindowDetails[]</returns>
		private MinTimeWindowDetails[] inputMinDetails()
		{
			MinTimeWindowDetails[] allDetails = (_gridMinDetails.GridContext as BoundContext).Data as MinTimeWindowDetails[];
            MinTimeWindowDetails[] newDetails = null ;

			if (allDetails != null)
			{
                newDetails = new MinTimeWindowDetails[allDetails.Length];
				int waferIndex = 0;

				foreach (MinTimeWindowDetails detail in allDetails)
				{
					newDetails[waferIndex] = new MinTimeWindowDetails();
					newDetails[waferIndex].ListItemAction = ListItemAction.Add;
					newDetails[waferIndex].AlertFrame = detail.AlertFrame;
					newDetails[waferIndex].BeforetimeAction = detail.BeforetimeAction;
					newDetails[waferIndex].Container = detail.Container;
					newDetails[waferIndex].Equipment = detail.Equipment;
					newDetails[waferIndex].MinTime = detail.MinTime;
					newDetails[waferIndex].OvertimeAction = detail.OvertimeAction;
					newDetails[waferIndex].OvertimeFrame = detail.OvertimeFrame;
					newDetails[waferIndex].TrackInEmployeeName = detail.TrackInEmployeeName;
					newDetails[waferIndex].TrackInTimeStamp = detail.TrackInTimeStamp;
					newDetails[waferIndex].TrackInUsername = detail.TrackInUsername;
					newDetails[waferIndex].WaferMapDetails = detail.WaferMapDetails;
					newDetails[waferIndex].WaferMapDetailsType = detail.WaferMapDetailsType;
					newDetails[waferIndex].LotWafersItem = detail.LotWafersItem;
					newDetails[waferIndex].Comments = detail.Comments;
					newDetails[waferIndex].EmailGroup = detail.EmailGroup;
					newDetails[waferIndex].FieldAction = detail.FieldAction;
					waferIndex++;
				}
			}
			return newDetails;
		}

		/// <summary>
		/// Gets the correct format to submit to the server
		/// </summary>
		/// <returns> MaxTimeWindowDetails[]</returns>
		private MaxTimeWindowDetails[] inputMaxDetails()
		{
			MaxTimeWindowDetails[] allDetails = (_gridMaxDetails.GridContext as BoundContext).Data as MaxTimeWindowDetails[];
			MaxTimeWindowDetails[] newDetails = null;

			if (allDetails != null)
			{
                newDetails = new MaxTimeWindowDetails[allDetails.Length];
				int waferIndex = 0;
				foreach (MaxTimeWindowDetails detail in allDetails)
				{
					newDetails[waferIndex] = new MaxTimeWindowDetails();
					newDetails[waferIndex].ListItemAction = ListItemAction.Add;
					newDetails[waferIndex].MaxTime = detail.MaxTime;
					newDetails[waferIndex].OvertimeAction = detail.OvertimeAction;
					newDetails[waferIndex].StartSpec = detail.StartSpec;
					newDetails[waferIndex].StartTimestamp = detail.StartTimestamp;
					newDetails[waferIndex].StartUsername = detail.StartUsername;
					newDetails[waferIndex].StopSpec = detail.StopSpec;
					newDetails[waferIndex].TotalStopTime = detail.TotalStopTime;
					newDetails[waferIndex].WaferMapDetails = detail.WaferMapDetails;
					newDetails[waferIndex].WaferMapDetailsType = detail.WaferMapDetailsType;	
					newDetails[waferIndex].LotWafersItem = detail.LotWafersItem;
					newDetails[waferIndex].Comments = detail.Comments;
					newDetails[waferIndex].EmailGroup = detail.EmailGroup;
					newDetails[waferIndex].FieldAction = detail.FieldAction;
					waferIndex++;
				}
			}

			return newDetails;
		}

		/// <summary>
		/// On page load
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			//_txtSelectionId.DataChanged += new EventHandler(_txtSelectionId_DataChanged);
			txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);

			if (Page.IsPostBack)
				// Check if it is a pop up close, get the return result
				if (SEMI.AppCode.UIUtility.IsPopupClose(this))
				{

					if (_envSelectedLots != null)


						// manually initialize the containers list data contract since it is not triggered when we do a manual popup close
						if (Page.DataContract.GetValueByName("LotModifyTimeWindow_SelectedLotsListDM") != null)
						{
							//gets the list of returned lot from the popup form
							_envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("LotModifyTimeWindow_SelectedLotsListDM") as string[];
						}

					if (_envSelectedLots.SS_ContainersList != null)
					{
						JQDataGrid _gridContainersTemp = Page.FindCamstarControl("LotGrid") as JQDataGrid;

						SetGridData(_envSelectedLots.SS_ContainersList[0].ToString(), _gridContainersTemp);

						GetData(_envSelectedLots.SS_ContainersList[0].ToString());

						_txtSelectionId.TextControl.Text = _envSelectedLots.SS_ContainersList[0].ToString();

						CamstarWebControl.SetRenderToClient(_gridContainersTemp);
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

			var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

			if (action != null && action.Parameters == "Reset")
			{
				Page.ClearValues();
				_gridContainers.ClearData();
				_txtSelectionId.Focus();
				clearData(true);
			}
			base.WebPartCustomAction(sender, e);
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
				clearData(true);
			}
		}

		#endregion
	}
}



