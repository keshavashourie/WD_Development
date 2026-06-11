/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using SEMI.AppCode;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
	public class SS_CarrierChange : scsShopfloorBase
	{

		#region PrivateConstants

		private const string cLotGrid = "LotDetailsGrid";
		private const string cReset = "Reset";

		#endregion // PrivateConstants

		#region PrivateProperties

		protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("CarrierChange_SelectionId") as CWC.TextBox; } }
		protected SEMI.AppCode.DataEnvelopControl _envContainer { get { return Page.FindCamstarControl("CarrierChange_ContainerDE") as SEMI.AppCode.DataEnvelopControl; } }
		protected JQDataGrid _grdLotDetails { get { return FindCamstarControl(cLotGrid) as JQDataGrid; } }
		protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("CarrierChange_Employee") as CWC.NamedObject; } }
		protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("CarrierChange_ComputerName") as CWC.TextBox; } }
		protected CWC.NamedObject _ndoCarrier { get { return Page.FindCamstarControl("CarrierChange_Carrier") as CWC.NamedObject; } }
		protected CWC.TextBox _txtComments { get { return Page.FindCamstarControl("CarrierChange_Comments") as CWC.TextBox; } }

		#endregion // PrivateProperties

		#region PrivateFunctions

		/// <summary>
		/// clears the lots grid (and adds 1 empty placeholder row)
		/// </summary>
		private void ResetLotDetailsGrid()
		{
			// clear any previous data
			_grdLotDetails.ClearData();

			// clear the headers, and add back the magical space col (for looks)
			if (_grdLotDetails.BoundContext.Fields.Count > 0)
			{
				JQFieldCollection objFieldClear = new JQFieldCollection();
				_grdLotDetails.BoundContext.Fields = objFieldClear;
				_grdLotDetails.BoundContext.Fields.Add(new JQField("_spacer") { LabelText = "&nbsp;", Visible = true });
			}
		} //end ResetLotDetailsGrid

		/// <summary>
		/// reset all input fields to empty / nothing
		/// </summary>
		private void ResetFields(Boolean bIncludeSelectionId = true)
		{
			if (bIncludeSelectionId)
				_txtSelectionId.ClearData();
			_envContainer.ClearData();
			_ndoEmployee.ClearData();
			_ndoCarrier.ClearData();
			_txtComments.ClearData();
			ResetLotDetailsGrid();
		} //end ResetFields

		/// <summary>
		/// set the service name of page submit action (for dynamic PrimaryServiceType)
		/// </summary>		
		private void SetPageActionServiceName()
		{
			var actSubmit = (Page as Camstar.WebPortal.FormsFramework.IActionContainer).ActionDispatcher.GetActionByName("SubmitAction");
			if (actSubmit != null)
			{
			    (actSubmit as Personalization.SubmitAction).ServiceName = this.PrimaryServiceType;
			}
		} //end SetPageActionServiceName

		#endregion // PrivateFunctions

		#region Handlers

		/// <summary>
		/// triggered when a lot name is entered in the selection ID field (params not used)
		/// fetches lot related data (Containers, ...) based on lot name (SelectionId)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void SelectionIdControl_DataChanged()
		{
			try
			{
				if (string.IsNullOrWhiteSpace(_txtSelectionId.TextControl.Text))
				{
					ResetFields(false);
					_txtSelectionId.TextControl.Text = "";
				}
				else
				{
					Page.StatusBar.ClearMessage();

					// get the session and user profile
					var fs = FrameworkManagerUtil.GetFrameworkSession();

					// get the page primary service type, and run appropriate constructor for service object
					string sServiceType = Page.PrimaryServiceType;
					var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
					var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
					var objService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });	// CarrierChangeService oService = new CarrierChangeService(fs.CurrentUserProfile);

					// create / init the service data and service info objects
					var objSvcData = CreateServiceData(sServiceType);		// CarrierChange oSvcData = new CarrierChange();
					(objSvcData as CarrierChange).SelectionId = _txtSelectionId.TextControl.Text;
					var objSvcInfo = CreateServiceInfo(sServiceType);		// CarrierChange_Info oSvcInfo = new CarrierChange_Info();
					(objSvcInfo as CarrierChange_Info).Containers = new Info(true);
					(objSvcInfo as CarrierChange_Info).NewCarrier = new Info(true);

					// init the request and result object
					var objRequest = WCFObject.CreateObject(sServiceType + "_Request");	// CarrierChange_Request oRequest = new CarrierChange_Request();
					(objRequest as Request).Info = (objSvcInfo as CarrierChange_Info);
					Result objResult = new Result();

					// execute to request the value(s)
					OM.ResultStatus oResStat = (objService as IShopFloorBase).ResolveSelectionId(objSvcData, (objRequest as Request), out objResult);
					// OM.ResultStatus oResStat = oService.ResolveSelectionId(oSvcData, oRequest, out oResult);

					if (oResStat.IsSuccess) 
					{
                        if ((objResult.Value as CarrierChange).Containers != null)
                        {
						    JQDataGrid gridContainers = Page.FindCamstarControl(cLotGrid) as JQDataGrid;
						    if (gridContainers != null)
						    {
							    string lotName = (objResult.Value as CarrierChange).Containers[0].Name.ToString();
							    SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "CarrierChange", lotName, true, ref gridContainers);
							    if ((objResult.Value as CarrierChange).NewCarrier != null)
							    {
								    _ndoCarrier.Text = (objResult.Value as CarrierChange).NewCarrier.Name;
							    }
							    else
							    {
								    _ndoCarrier.Text = "";
							    }
							    _txtSelectionId.TextControl.Text = lotName;
						    }
                        }
					}
					else
					{
						DisplayMessage(oResStat);
					}
				}
			}
			catch (Exception Ex)
			{
				DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
			}
		} //end SelectionIdControl_DataChanged

		#endregion // Handlers

		#region Overrides

		/// <summary>
		/// override of the Get Input Data method
		/// </summary>
		/// <param name="serviceData" type="Service"></param>
		public override void GetInputData(Service serviceData)
		{
			base.GetInputData(serviceData);

			//NewCarrier
			if (_ndoCarrier.Data != null)
			{
				(serviceData as CarrierChange).NewCarrier = new NamedObjectRef();
				(serviceData as CarrierChange).NewCarrier.Name = _ndoCarrier.Text;
			}
			//Container
			if (_grdLotDetails.Data != null)
			{
				(serviceData as CarrierChange).Container = new ContainerRef();
				(serviceData as CarrierChange).Container.Name = _txtSelectionId.TextControl.Text;
			}
		} //end GetInputData

		/// <summary>
		/// override of the Web part custom action method
		/// </summary>
		/// <param name="sender" type="object"></param>
		/// <param name="e" type="Personalization.CustomActionEventArgs"></param>
		public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
		{
			base.WebPartCustomAction(sender, e);
			var action = e.Action as CustomAction;
			if (action != null)
			{
				switch (action.Parameters)
				{
					case cReset:
					{
						// called on ResetButton click (ID = 'ctl00_WebPartManager_ButtonsBar_ResetAction')
						// this button click is also triggered via JS function "SS_LotShip_ContainerGrid_rowDelete"
						// (a custom handler for rowDelete on the "LotDetailsGrid" grid)
						// when the last row in the lot details grid is deleted ...
						Page.ClearValues();
                        Page.ShopfloorReset(sender, e);
						ResetFields();
						_txtSelectionId.Focus();
						break;
					}
				}
			}
		} //end WebPartCustomAction

		/// <summary>
		/// override of the On Load event handler
		/// </summary>
		/// <param name="e" type="EventArgs"></param>
		protected override void OnLoad(EventArgs e)
		{
			try
			{
				base.OnLoad(e);
				Boolean bIsPopupClose = SEMI.AppCode.UIUtility.IsPopupClose(this);
				Boolean bIsPostBack = Page.IsPostBack;
				if (!bIsPostBack)
				{
					_txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
					SetPageActionServiceName();
				}
				else if (bIsPopupClose)
				{
					var lotSel = Page.DataContract.GetValueByName("CarrierChange_ContainerDataEnvDM");
					Boolean bLSPopupClosed = (lotSel != null);
					if (bLSPopupClosed && _envContainer != null) // [NH] there's data in CarrierChange_ContainerDataEnvDM, so assume LotSelection popup closed
					{
						if (lotSel != null) // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
						{
							_envContainer.SS_ContainersList = lotSel as string[];
						}

						if (_envContainer.SS_ContainersList != null) // only true when lotselection popup closes
						{
							string sContainer = _envContainer.SS_ContainersList[0];
							_envContainer.SS_ContainersList = null;
							_txtSelectionId.Data = sContainer;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Page.StatusBar.WriteError(ex.Message.ToString());
			}
		} //end OnLoad

		/// <summary>
		/// override of the Post Execute event handler
		/// </summary>
		/// <param name="status" type="ResultStatus"></param>
		/// <param name="serviceData" type="Service"></param>
		public override void PostExecute(ResultStatus status, Service serviceData)
		{
			base.PostExecute(status, serviceData);
			if (status.IsSuccess)
			{
				Page.ClearValues();
				ResetFields();
				_txtSelectionId.Focus();
			}
		} //end PostExecute

		#endregion // Overrides
	}
}




