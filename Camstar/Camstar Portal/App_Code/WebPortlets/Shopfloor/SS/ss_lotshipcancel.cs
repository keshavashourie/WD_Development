/* Copyright 2019 Siemens */
// modified 3-March-2014 11:29 EST by GLD
//Updated the GetLotQuerySelection with a dynamic approach for the primary service

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
	public class SS_LotShipCancel : scsShopfloorBase
	{
		#region PrivateConstants

		private const string cLotGrid = "LotDetailsGrid";
		private const string cAttrGrid = "LotShipCancel_ServiceAttrsDetails";
		private const string cValuesGrid = "ValidValuesGrid";
		private const string cReset = "Reset";

		#endregion // PrivateConstants

		#region PrivateProperties

		protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotShipCancel_SelectionId") as CWC.TextBox; } }
		protected SEMI.AppCode.DataEnvelopControl _envContainer { get { return Page.FindCamstarControl("LotShipCancel_ContainerDE") as SEMI.AppCode.DataEnvelopControl; } }
		protected JQDataGrid _grdLotDetails { get { return FindCamstarControl(cLotGrid) as JQDataGrid; } }
		protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("LotShipCancel_Employee") as CWC.NamedObject; } }
		protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("LotShipCancel_ComputerName") as CWC.TextBox; } }
		protected CWC.CheckBox _chkSkipMail { get { return Page.FindCamstarControl("LotShipCancel_SkipEmail") as CWC.CheckBox; } }
		protected CWC.CheckBox _chkReActLot { get { return Page.FindCamstarControl("LotShipCancel_ReactivateLot") as CWC.CheckBox; } }
		protected JQDataGrid _grdSvcAttrs { get { return Page.FindCamstarControl(cAttrGrid) as JQDataGrid; } }
		protected JQDataGrid _grdValidValues { get { return Page.FindCamstarControl(cValuesGrid) as JQDataGrid; } }

		#endregion // PrivateProperties

		#region PrivateFunctions

		/// <summary>
		/// clears the grid (and adds 1 empty placeholder row)
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
		private void ResetFields()
		{
			_txtSelectionId.ClearData();
			_envContainer.ClearData();
			_ndoEmployee.ClearData();
			_chkSkipMail.ClearData();
			_chkReActLot.ClearData();
			_grdSvcAttrs.ClearData();
			_grdValidValues.ClearData();
			ResetLotDetailsGrid();
		} //end ResetFields

		/// <summary>
		/// set the service name of page submit action
		/// </summary>		
		private void SetPageActionServiceName()
		{
			var actSubmit = (Page as Camstar.WebPortal.FormsFramework.IActionContainer).ActionDispatcher.GetActionByName("SubmitAction");
			if (actSubmit != null)
			{
				(actSubmit as Personalization.SubmitAction).ServiceName = this.PrimaryServiceType;
			}
		} //end SetPageActionServiceName

		/// <summary>
		/// fetch service attr details and bind it to the grid
		/// </summary>		
		private void GetServiceAttrsDetails()
		{
			if (_grdLotDetails.Data == null || _grdLotDetails.GridContext.GetTotalRows() < 1)
				return;

			// get selectionId from 1st grid row
			string sSelectionId = _grdLotDetails.GridContext.GetCell("000000", "Lot").ToString();

			// get the session and user profile
			var fs = FrameworkManagerUtil.GetFrameworkSession();

			// get the page primary service type, and run appropriate constructor for service object
			string sServiceType = Page.PrimaryServiceType;
			var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
			var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
			var objService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });	// LotShipCancelService oService = new LotShipCancelService(fs.CurrentUserProfile);

			// create / init the service data and service info objects
			var objSvcData = CreateServiceData(sServiceType);		// LotShipCancel oSvcData = new LotShipCancel();
			(objSvcData as LotShipCancel).SelectionId = sSelectionId;
			var objSvcInfo = CreateServiceInfo(sServiceType);		// LotShipCancel_Info oSvcInfo = new LotShipCancel_Info();

			(objSvcInfo as LotShipCancel_Info).ServiceAttrsDetailsSelection = new ServiceAttrsDetails_Info();		//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection", new ServiceAttrsDetails_Info());
			(objSvcInfo as LotShipCancel_Info).ServiceAttrsDetailsSelection.Attribute = new Info(true);				//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.Attribute", new Info(true));
			(objSvcInfo as LotShipCancel_Info).ServiceAttrsDetailsSelection.AttributeName = new Info(true);			//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.AttributeName", new Info(true));
			(objSvcInfo as LotShipCancel_Info).ServiceAttrsDetailsSelection.AlternateName1 = new Info(true);		//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.AlternateName1", new Info(true));
			(objSvcInfo as LotShipCancel_Info).ServiceAttrsDetailsSelection.AlternateName2 = new Info(true);		//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.AlternateName2", new Info(true));
			(objSvcInfo as LotShipCancel_Info).ServiceAttrsDetailsSelection.AttributeValue = new Info(true);		//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.AttributeValue", new Info(true));
			(objSvcInfo as LotShipCancel_Info).ServiceAttrsDetailsSelection.AttributeRevision = new Info(true);		//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.AttributeRevision", new Info(true));
			(objSvcInfo as LotShipCancel_Info).ServiceAttrsDetailsSelection.AccessLevel = new Info(true);			//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.AccessLevel", new Info(true));
			(objSvcInfo as LotShipCancel_Info).ServiceAttrsDetailsSelection.FieldType = new Info(true);				//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.FieldType", new Info(true));
			(objSvcInfo as LotShipCancel_Info).ServiceAttrsDetailsSelection.IsRequired = new Info(true);			//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.IsRequired", new Info(true));
			(objSvcInfo as LotShipCancel_Info).ServiceAttrsDetailsSelection.ServiceAttrsSetupName = new Info(true);	//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.ServiceAttrsSetupName", new Info(true));
			(objSvcInfo as LotShipCancel_Info).ServiceAttrsDetailsSelection.ObjectTypeName = new Info(true);		//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.ObjectTypeName", new Info(true));
			(objSvcInfo as LotShipCancel_Info).ServiceAttrsDetailsSelection.ValidValues = new AttributeValidValuesChanges_Info();//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.ValidValues", new AttributeValidValuesChanges_Info());
			(objSvcInfo as LotShipCancel_Info).ServiceAttrsDetailsSelection.ValidValues.AttributeValue = new Info(true);		//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.ValidValues.AttributeValue", new Info(true));
			(objSvcInfo as LotShipCancel_Info).ServiceAttrsDetailsSelection.ValidValues.AttributeRevision = new Info(true);		//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.ValidValues.AttributeRevision", new Info(true));

			// init the request and result object
			var objRequest = WCFObject.CreateObject(sServiceType + "_Request");	// LotShipCancel_Request oRequest = new LotShipCancel_Request();
			(objRequest as Request).Info = (objSvcInfo as LotShipCancel_Info);
			Result objResult = new Result();

			// submit the transaction as IShopFloorBase
			OM.ResultStatus oResStat = (objService as IShopFloorBase).ResolveSelectionId(objSvcData, (objRequest as Request), out objResult);

			if (oResStat.IsSuccess && (objResult.Value as LotShipCancel).ServiceAttrsDetailsSelection != null)
			{
				// bind result to the grid
                _grdSvcAttrs.ClearData();
				_grdSvcAttrs.Data = (objResult.Value as LotShipCancel).ServiceAttrsDetailsSelection.ToArray();
				_grdSvcAttrs.OriginalData = (objResult.Value as LotShipCancel).ServiceAttrsDetailsSelection.ToArray();

				// create Valid Values table
				DataTable validValuesDT = new DataTable();
				int countSvcAttr = (objResult.Value as LotShipCancel).ServiceAttrsDetailsSelection.Count();
				validValuesDT.Columns.Add("Attribute", typeof(String));
				validValuesDT.Columns.Add("AttributeValue", typeof(String));
				validValuesDT.Columns.Add("AttributeRevision", typeof(String));
				for (int i = 0; i < countSvcAttr; i++)
				{
					if (((objResult.Value as LotShipCancel).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues != null)
					{
						int countValidValues = ((objResult.Value as LotShipCancel).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.Count();
						for (int x = 0; x < countValidValues; x++)
						{
							DataRow dtRow = validValuesDT.NewRow();
							dtRow.SetField("Attribute", ((objResult.Value as LotShipCancel).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).Attribute.Name);
							dtRow.SetField("AttributeValue", (((objResult.Value as LotShipCancel).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.GetValue(x) as AttributeValidValuesChanges).AttributeValue);
							dtRow.SetField("AttributeRevision", (((objResult.Value as LotShipCancel).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.GetValue(x) as AttributeValidValuesChanges).AttributeRevision);
							validValuesDT.Rows.Add(dtRow);
						}
					}
				}

				_grdValidValues.ClearData();
				_grdValidValues.Data = validValuesDT;
				_grdValidValues.OriginalData = validValuesDT;
			}
		} //end GetServiceAttrsDetails

		/// <summary>
		/// transfer data from lot selection popup via data envelope to lot details grid 
		/// </summary>
		private void SetLotDetailsGrid(object lotSel)
		{
			if (lotSel != null) // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
			{
				_envContainer.SS_ContainersList = lotSel as string[];
			}

			if (_envContainer.SS_ContainersList != null) // only true when lotselection popup closes
			{
				Boolean bIsFirst = true;
				string[] sContainers = _envContainer.SS_ContainersList;
				JQDataGrid _gridContainers = Page.FindCamstarControl(cLotGrid) as JQDataGrid;

				foreach (string sContainersItem in sContainers)
				{
					// check if lot exists already in the grid
					string strSelectedGridId = (_gridContainers.GridContext as BoundContext).GetRowIdByCellValue("Lot", sContainersItem);

					if (string.IsNullOrEmpty(strSelectedGridId)) // not in the grid: add it
					{
						SEMI.AppCode.UIUtility.GetLotQuerySelection(this, this.PrimaryServiceType.ToString(), sContainersItem
												, false, ref _gridContainers, cLotGrid, true);
						if (bIsFirst)
						{
							GetServiceAttrsDetails();
							bIsFirst = false;
						}
					}
				}

				// clean out the container list
				_envContainer.SS_ContainersList = null;
			}
		} // end SetLotDetailsGrid

		/// <summary>
		/// transfer data from valid values popup to service attributes grid 
		/// </summary>
		private void SetServiceAttrsGrid(object attrSel)
		{
			var attrVal = Page.DataContract.GetValueByName("LotShipCancel_ReturnedValue");
			var attrRev = Page.DataContract.GetValueByName("LotShipCancel_ReturnedRevision");
			string selectedAttr = attrSel.ToString();
			if (!string.IsNullOrEmpty(selectedAttr))
			{
				ServiceAttrsDetails[] getServiceAttrsDetails = _grdSvcAttrs.Data as ServiceAttrsDetails[];
				for (int i = 0; i < getServiceAttrsDetails.Count(); i++)
				{
					if (getServiceAttrsDetails[i].Attribute.Name == selectedAttr)
					{
						getServiceAttrsDetails[i].AttributeValue = (attrVal != null ? attrVal.ToString() : "");
						getServiceAttrsDetails[i].AttributeRevision = (attrRev != null ? attrRev.ToString() : "");
					}
				}
				_grdSvcAttrs.ClearData();
				_grdSvcAttrs.Data = getServiceAttrsDetails;
				_grdSvcAttrs.OriginalData = getServiceAttrsDetails;
			}
		} // end SetServiceAttrsGrid

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
				if (_txtSelectionId.Data != null)
				{
					Page.StatusBar.ClearMessage();

					// get the session and user profile
					var fs = FrameworkManagerUtil.GetFrameworkSession();

					// get the page primary service type, and run appropriate constructor for service object
					string sServiceType = Page.PrimaryServiceType;
					var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
					var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
					var objService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });	// LotShipCancelService oService = new LotShipCancelService(fs.CurrentUserProfile);

					// create / init the service data and service info objects
					var objSvcData = CreateServiceData(sServiceType);		// LotShipCancel oSvcData = new LotShipCancel();
					(objSvcData as LotShipCancel).SelectionId = (string)_txtSelectionId.Data;
					var objSvcInfo = CreateServiceInfo(sServiceType);		// LotShipCancel_Info oSvcInfo = new LotShipCancel_Info();
					(objSvcInfo as LotShipCancel_Info).Containers = new Info(true); // oSvcInfo.Containers = FieldInfoUtil.RequestValue();

					// init the request and result object
					var objRequest = WCFObject.CreateObject(sServiceType + "_Request");	// LotShipCancel_Request oRequest = new LotShipCancel_Request();
					(objRequest as Request).Info = (objSvcInfo as LotShipCancel_Info); // oRequest.Info = oSvcInfo;
					Result objResult = new Result();

					// execute to request the value(s)
					OM.ResultStatus oResStat = (objService as IShopFloorBase).ResolveSelectionId(objSvcData, (objRequest as Request), out objResult);
					// OM.ResultStatus oResStat = oService.ResolveSelectionId(oSvcData, oRequest, out oResult);

					if (oResStat.IsSuccess && (objResult.Value as LotShipCancel).Containers != null)
					{
						Boolean bIsFirst = true;
						JQDataGrid _gridContainers = Page.FindCamstarControl(cLotGrid) as JQDataGrid;
						foreach (OM.ContainerRef oContainer in (objResult.Value as LotShipCancel).Containers)
						{
							// check if lot exists already in the grid
							string strSelectedGridId = (_gridContainers.GridContext as BoundContext).GetRowIdByCellValue("Lot", oContainer.Name.ToString());

							if (string.IsNullOrEmpty(strSelectedGridId)) // not in the grid: add it
							{
								SEMI.AppCode.UIUtility.GetLotQuerySelection(this, this.PrimaryServiceType.ToString(), oContainer.Name.ToString()
																, false, ref _gridContainers, cLotGrid, true);
								if (bIsFirst)
								{
									GetServiceAttrsDetails();
									bIsFirst = false;
								}
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

			finally
			{
				_txtSelectionId.TextControl.Text = "";
				_txtSelectionId.Focus();
			}
		} //end SelectionIdControl_DataChanged

		#endregion Handlers

		#region Overrides

		/// <summary>
		/// override Get Input Data method
		/// </summary>
		public override void GetInputData(Service serviceData)
		{
			base.GetInputData(serviceData);

			//ServiceAttributes
			if (_grdSvcAttrs.Data != null)
			{
				ServiceAttrsDetails[] srvAttrsDetails = _grdSvcAttrs.Data as ServiceAttrsDetails[];
				if (serviceData is LotShipCancel)
				{
					(serviceData as LotShipCancel).ServiceAttrsDetails = new ServiceAttrsDetails[srvAttrsDetails.Count()];

					// collect the attributes
					for (int i = 0; i < srvAttrsDetails.Count(); i++)
					{
						(serviceData as LotShipCancel).ServiceAttrsDetails[i] = new ServiceAttrsDetails();
						(serviceData as LotShipCancel).ServiceAttrsDetails[i].Attribute = new NamedObjectRef();
						(serviceData as LotShipCancel).ServiceAttrsDetails[i].Attribute.Name = srvAttrsDetails[i].Attribute.Name;
						(serviceData as LotShipCancel).ServiceAttrsDetails[i].FieldType = srvAttrsDetails[i].FieldType;
						(serviceData as LotShipCancel).ServiceAttrsDetails[i].ServiceAttrsSetupName = srvAttrsDetails[i].ServiceAttrsSetupName;
						(serviceData as LotShipCancel).ServiceAttrsDetails[i].AttributeValue = srvAttrsDetails[i].AttributeValue;
						(serviceData as LotShipCancel).ServiceAttrsDetails[i].AttributeRevision = srvAttrsDetails[i].AttributeRevision;
					}
				}
			}

			//Containers
			if (_grdLotDetails.Data != null)
			{
				int intTotalContainers = _grdLotDetails.BoundContext.GetTotalRows();
				(serviceData as LotShipCancel).Containers = new ContainerRef[intTotalContainers];

				// collect the containers
				for (int x = 0; x < intTotalContainers; x++)
				{
					(serviceData as LotShipCancel).Containers[x] = new ContainerRef();
					(serviceData as LotShipCancel).Containers[x].Name = _grdLotDetails.GridContext.GetCell(x.ToString().PadLeft(6, '0'), "Lot").ToString();
				}
			}
		} //end GetInputData

		/// <summary>
		/// override Web Part Custom Action method
		/// </summary>
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
						// this button click is also triggered via JS function "SS_ContainerGrid_rowDelete"
						// (a custom handler for rowDelete on the "LotDetailsGrid" grid)
						// when the last row in the lot details grid is deleted ...
						_txtSelectionId.Focus();
                        Page.ShopfloorReset(sender, e);
						break;
					}
				}
			}
		} //end WebPartCustomAction

		/// <summary>
		/// override On Load event handler
		/// </summary>
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
					var attrSel = Page.DataContract.GetValueByName("LotShipCancel_SelectedAttribute");
					var lotSel = Page.DataContract.GetValueByName("LotShipCancel_ContainerDataEnvDM");
					Boolean bVVPopupClosed = (attrSel != null);
					Boolean bLSPopupClosed = (lotSel != null);
					if (bLSPopupClosed && _envContainer != null) // [NH] there's data in LotShipCancel_ContainerDataEnvDM, so assume LotSelection popup closed
					{
						SetLotDetailsGrid(lotSel);
					}
					if (bVVPopupClosed) // [NH] there's data in LotShipCancel_SelectedAttribute so assume ValidValues popup closed
					{
						SetServiceAttrsGrid(attrSel);
					}
				}
			}
			catch (Exception ex)
			{
				Page.StatusBar.WriteError(ex.Message.ToString());
			}
		} //end OnLoad

		/// </summary>
		/// override Post Execute event handler
		/// </summary>
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




