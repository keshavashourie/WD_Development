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
	public class SS_ModifyInsertion : scsShopfloorBase
	{
		#region Properties

		protected JQDataGrid _gridContainers { get { return Page.FindCamstarControl("LotGrid") as JQDataGrid; } }
		protected JQDataGrid _gridWafers { get { return Page.FindCamstarControl("ModifyInsertion_Wafers") as JQDataGrid; } }
		private SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("ModifyInsertion_SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }
		private CWC.NamedObject noProcessType { get { return Page.FindCamstarControl("ModifyInsertion_ProcessType") as CWC.NamedObject; } }
		private DropDownList dlWIPStatus { get { return Page.FindCamstarControl("ModifyInsertion_WIPStatus") as DropDownList; } }
		private DropDownList dlFailureAction { get { return Page.FindCamstarControl("ModifyInsertion_FailureAction") as DropDownList; } }
		private DropDownList dlProcessStatus { get { return Page.FindCamstarControl("ModifyInsertion_ProcessStatus") as DropDownList; } }
		private CWC.CheckBox cbIsWaferProcessing { get { return Page.FindCamstarControl("ModifyInsertion_IsWaferProcessing") as CWC.CheckBox; } }
		protected CWC.TextBox txtComputerName { get { return Page.FindCamstarControl("ModifyInsertion_ComputerNameField") as CWC.TextBox; } }
		protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("ModifyInsertion_SelectionIdField") as CWC.TextBox; } }
		protected CWC.TextBox txtQtyToProcess { get { return Page.FindCamstarControl("ModifyInsertion_QtyToProcess") as CWC.TextBox; } }
		protected CWC.TextBox txtMinQtyToProcess { get { return Page.FindCamstarControl("ModifyInsertion_MinQtyToProcess") as CWC.TextBox; } }
		protected CWC.TextBox txtMaxQtyToProcess { get { return Page.FindCamstarControl("ModifyInsertion_MaxQtyToProcess") as CWC.TextBox; } }
		protected CWC.TextBox txtQtyToProcessBalance { get { return Page.FindCamstarControl("ModifyInsertion_QtyToProcessBalance") as CWC.TextBox; } }
		protected CWC.TextBox txtMinQtyToProcessBalance { get { return Page.FindCamstarControl("ModifyInsertion_MinQtyToProcessBalance") as CWC.TextBox; } }
		protected CWC.TextBox txtMaxQtyToProcessBalance { get { return Page.FindCamstarControl("ModifyInsertion_MaxQtyToProcessBalance") as CWC.TextBox; } }
		protected CWC.TextBox txtMaxInsertion { get { return Page.FindCamstarControl("ModifyInsertion_MaxInsertion") as CWC.TextBox; } }
		private ToggleContainer _toggleComments { get { return Page.FindCamstarControl("Control") as ToggleContainer; } }
		ContainerRef containerRef;

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
                    clearData(true);
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

		/// <summary>
		/// Gets the receiving data for the field
		/// </summary>
		public void ReceivingLocationDataChangeEvent()
		{
			GetData(null);
		}

        public void ProcessType_DataChanged()
        {
            dlProcessStatus.ClearData();
            dlProcessStatus.DropDownControl.ClearSelectedItem();
            dlProcessStatus.DropDownControl.Items.Clear();
            txtQtyToProcess.ClearData();
            txtMinQtyToProcess.ClearData();
            txtMaxQtyToProcess.ClearData();
            txtMaxQtyToProcessBalance.ClearData();
            txtMinQtyToProcessBalance.ClearData();

            if (_txtSelectionId .Data != null && noProcessType.Data != null 
                && noProcessType.DropDownControl.SelectedValue != "" && noProcessType.DropDownControl.SelectedValue != null)
            {                         
                GetData(_txtSelectionId.Data.ToString(), true);                
            }
            else
            {
                _gridWafers.Visible = false;
                isVisibleControls(false);
                dlWIPStatus.ClearData();
                dlWIPStatus.DropDownControl.ClearSelectedItem();
                dlWIPStatus.DropDownControl.Items.Clear();

                txtMaxInsertion.ClearData();
                noProcessType.ClearData();
                noProcessType.DropDownControl.ClearSelectedItem();
                noProcessType.DropDownControl.Items.Clear();
            }            
        }

		#endregion

		#region Private Methods

		/// <summary>
		/// Set controls to Visible
		/// </summary>
		/// <param name="isVisible"></param>
		private void isVisibleControls(bool isVisible)
		{
			txtQtyToProcess.Hidden = !isVisible;
			txtMinQtyToProcess.Hidden = !isVisible;
			txtMaxQtyToProcess.Hidden = !isVisible;
			txtQtyToProcessBalance.Hidden = !isVisible;
			txtMinQtyToProcessBalance.Hidden = !isVisible;
			txtMaxQtyToProcessBalance.Hidden = !isVisible;
		}

		/// <summary>
		/// Gets data for form based off of return container(s)
		/// </summary>
		/// <param name="lot">string lot name</param>
		private void GetData(string lot, bool IsProcessTypeChange = false)
		{
			try
			{				
				OM.ModifyInsertion_Info oServiceInfo = new OM.ModifyInsertion_Info();
				ModifyInsertion_Result resultData = new ModifyInsertion_Result();
				UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
				ModifyInsertionService svc = new ModifyInsertionService(profile);
				ModifyInsertion oServiceData = new ModifyInsertion();
				ModifyInsertion_Request reqData = new ModifyInsertion_Request();

				if (lot != null)
				{
					containerRef = new ContainerRef(lot);
				}
				oServiceData.Container = containerRef;
				oServiceData.SelectionId = lot;

				oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
				oServiceInfo.IsWaferProcessing = FieldInfoUtil.RequestValue();
				oServiceInfo.WIPStatusSelection = FieldInfoUtil.RequestValue();
				oServiceInfo.WIPStatus = FieldInfoUtil.RequestValue();
				oServiceInfo.MaxInsertion = FieldInfoUtil.RequestValue();
				oServiceInfo.FailureAction = FieldInfoUtil.RequestValue();
				oServiceInfo.HoldLotIfLimitsNotFound = FieldInfoUtil.RequestValue();
				oServiceInfo.AutoMoveOut = FieldInfoUtil.RequestValue();
				oServiceInfo.AutoRejectsRescreens = FieldInfoUtil.RequestValue();

                if (IsProcessTypeChange)
                {                    
                    oServiceData.ProcessType = new NamedObjectRef();
                    oServiceData.ProcessType.Name = noProcessType.DropDownControl.SelectedValue;                    
                    oServiceInfo.ProcessStatusSelection = FieldInfoUtil.RequestValue();
                    oServiceInfo.ProcessStatus = FieldInfoUtil.RequestValue();
                    oServiceInfo.QtyToProcess = FieldInfoUtil.RequestValue();
                    oServiceInfo.QtyToProcessBalance = FieldInfoUtil.RequestValue();

                    oServiceInfo.MinQtyToProcess = FieldInfoUtil.RequestValue();
                    oServiceInfo.MaxQtyToProcess = FieldInfoUtil.RequestValue();
                    oServiceInfo.MinQtyToProcessBalance = FieldInfoUtil.RequestValue();
                    oServiceInfo.MaxQtyToProcessBalance = FieldInfoUtil.RequestValue();

                    //request data for the wafer details
                    oServiceInfo.WafersSelection = new ModifyWafersDetails_Info();
                    oServiceInfo.WafersSelection.RequireTracking = FieldInfoUtil.RequestValue();
                    oServiceInfo.WafersSelection.WaferScribeNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.WafersSelection.WaferStatus = FieldInfoUtil.RequestValue();
                    oServiceInfo.WafersSelection.WaferNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.WafersSelection.RequireDataCollection = FieldInfoUtil.RequestValue();
                    oServiceInfo.WafersSelection.NDPW = FieldInfoUtil.RequestValue();
                    oServiceInfo.WafersSelection.GoodQty = FieldInfoUtil.RequestValue();                   
                }
                else
                {
                    oServiceInfo.ProcessTypeSelection = FieldInfoUtil.RequestValue();
                }

				reqData.Info = oServiceInfo;
				ResultStatus results = svc.GetEnvironment(oServiceData, reqData, out resultData);

				if (results.IsSuccess)
				{					
					DisplayValues(resultData.Value);				

                    // set the WIPStatus selection list and selected value
					if (resultData.Value.WIPStatusSelection != null && IsProcessTypeChange == false)
					    foreach (string wipStatus in resultData.Value.WIPStatusSelection)					    
						    dlWIPStatus.DropDownControl.Items.Add(wipStatus);
					    
					if (resultData.Value.WIPStatus != null)
					    dlWIPStatus.DropDownControl.SelectedValue = resultData.Value.WIPStatus.Value;

                    // set the max insertion value
					if (resultData.Value.MaxInsertion != null)
					    txtMaxInsertion.TextControl.Text = resultData.Value.MaxInsertion.Value.ToString();

                    // set the process status selection list and value
                    if (resultData.Value.ProcessStatusSelection != null)
                        foreach (string processStatus in resultData.Value.ProcessStatusSelection)
                            dlProcessStatus.DropDownControl.Items.Add(processStatus);

                    if (resultData.Value.ProcessStatus != null)
                        dlProcessStatus.DropDownControl.SelectedValue = resultData.Value.ProcessStatus.Value;  

                    // if its NOT a process type change event, then set the process types selection list
                    if (!IsProcessTypeChange)
                    {
                        if (resultData.Value.ProcessTypeSelection != null)
                        {
                            foreach (NamedObjectRef processType in resultData.Value.ProcessTypeSelection)
                                noProcessType.DropDownControl.Items.Add(processType.Name);
                            // re-fire the GetData to pull the wafer details
                            GetData(lot, true);
                        }                       
                    }                                                       

					if (resultData.Value.IsWaferProcessing != null)
						cbIsWaferProcessing.CheckControl.Checked = resultData.Value.IsWaferProcessing.Value;

					if (cbIsWaferProcessing.CheckControl.Checked)
					{
						_gridWafers.Visible = true;
						isVisibleControls(false);
						if (resultData.Value.WafersSelection != null)
						{
							(_gridWafers.GridContext as BoundContext).Data = resultData.Value.WafersSelection.ToArray();
							_gridWafers.GridContext.LoadData();
							CamstarWebControl.SetRenderToClient(_gridWafers);
						}						
					}
					else
					{
						if (resultData.Value.MinQtyToProcess != null)
						    txtMinQtyToProcess.TextControl.Text = resultData.Value.MinQtyToProcess.Value.ToString();
						if (resultData.Value.MaxQtyToProcess != null)
						    txtMaxQtyToProcess.TextControl.Text = resultData.Value.MaxQtyToProcess.Value.ToString();
						if (resultData.Value.MinQtyToProcessBalance != null)
						    txtMinQtyToProcessBalance.TextControl.Text = resultData.Value.MinQtyToProcessBalance.Value.ToString();
						if (resultData.Value.MaxQtyToProcessBalance != null)
						    txtMaxQtyToProcessBalance.TextControl.Text = resultData.Value.MaxQtyToProcessBalance.Value.ToString();
					
						_gridWafers.Visible = false;
						isVisibleControls(true);
					}
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
			dlProcessStatus.ClearData();
			dlProcessStatus.DropDownControl.ClearSelectedItem();
			dlProcessStatus.DropDownControl.Items.Clear();
            noProcessType.ClearData();
            noProcessType.DropDownControl.ClearSelectedItem();
            noProcessType.DropDownControl.Items.Clear();
			if (Allvalues)
			{
				dlFailureAction.ClearData();
				//dlFailureAction.DropDownControl.ClearSelectedItem();
				//dlFailureAction.DropDownControl.Items.Clear();
			}
		    txtMaxInsertion.ClearData();
			dlWIPStatus.ClearData();
			dlWIPStatus.DropDownControl.ClearSelectedItem();
			dlWIPStatus.DropDownControl.Items.Clear();
			cbIsWaferProcessing.IsChecked = false;
			_gridWafers.ClearData();
			isVisibleControls(false);
			txtMaxQtyToProcess.ClearData();
			txtMaxQtyToProcessBalance.ClearData();
			txtMinQtyToProcessBalance.ClearData();
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
				SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "ModifyInsertion", sContainersItem, false, ref _gridContainersTemp, "LotGrid", true);//_gridContainersTemp.LabelName for lotGrid string
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

			if (_gridContainers.Data != null)
			{
				OM.ModifyInsertion svcData = serviceData as OM.ModifyInsertion;
				svcData.Container = new ContainerRef(_txtSelectionId.TextControl.Text);
			}
		}

		/// <summary>
		/// On page load
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (txtComputerName.TextControl.Text == "")
				isVisibleControls(false);

			//_txtSelectionId.DataChanged += new EventHandler(_txtSelectionId_DataChanged);
			txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);

			if (Page.IsPostBack)
				// Check if it is a pop up close, get the return result
				if (SEMI.AppCode.UIUtility.IsPopupClose(this))
				{
					if (_envSelectedLots != null)
						// manually initialize the containers list data contract since it is not triggered when we do a manual popup close
						if (Page.DataContract.GetValueByName("ModifyInsertion_SelectedLotsListDM") != null)
						{
							//gets the list of returned lot from the popup form
							_envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("ModifyInsertion_SelectedLotsListDM") as string[];
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
				_gridContainers.ClearData();
				_txtSelectionId.Focus();
				isVisibleControls(false);
				clearData(true);
                dlProcessStatus.ClearData();
                dlProcessStatus.DropDownControl.ClearSelectedItem();
                dlProcessStatus.DropDownControl.Items.Clear();
				_gridWafers.Visible = false;
                Page.ClearValues();
				_toggleComments.Reset();
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
				_gridContainers.ClearData();
				_txtSelectionId.Focus();
				clearData(true);
                dlProcessStatus.ClearData();
                dlProcessStatus.DropDownControl.ClearSelectedItem();
                dlProcessStatus.DropDownControl.Items.Clear();
				isVisibleControls(false);
				_gridWafers.Visible = false;
                Page.ClearValues();
			}
		}

		#endregion
	}
}



