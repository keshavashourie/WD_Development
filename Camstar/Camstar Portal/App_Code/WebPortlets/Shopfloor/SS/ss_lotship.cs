/* Copyright 2019 Siemens */
// modified 1-Feb-2014 12:15 EST by NH
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
    public class SS_LotShip : scsShopfloorBase
	{
		#region PrivateConstants

		private const string cLotGrid = "LotDetailsGrid";
		private const string cAttrGrid = "LotShip_ServiceAttrsDetails";
		private const string cValuesGrid = "ValidValuesGrid";

		private const string cShipToFactory = "ShipToFactory";
		private const string cShipToProcess = "ShipToProcess";
		private const string cDocNo = "Generate";
		private const string cReset = "Reset";

		#endregion // PrivateConstants

		#region PrivateProperties

		protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotShip_SelectionId") as CWC.TextBox; } }
		protected SEMI.AppCode.DataEnvelopControl _envContainer { get { return Page.FindCamstarControl("LotShip_ContainerDE") as SEMI.AppCode.DataEnvelopControl; } }
		protected JQDataGrid _grdLotDetails { get { return FindCamstarControl(cLotGrid) as JQDataGrid; } }
		protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("LotShip_Employee") as CWC.NamedObject; } }
		protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("LotShip_ComputerName") as CWC.TextBox; } }
		protected CWC.NamedObject _ndoToFactory { get { return Page.FindCamstarControl("LotShip_ShipToFactory") as CWC.NamedObject; } }
		protected CWC.NamedObject _ndoToProcess { get { return Page.FindCamstarControl("LotShip_ShipToProcess") as CWC.NamedObject; } }
		protected CWC.TextBox _txtDocNo { get { return Page.FindCamstarControl("LotShip_DocNo") as CWC.TextBox; } }
		protected CWC.TextBox _txtShipNo { get { return Page.FindCamstarControl("LotShip_ShipNo") as CWC.TextBox; } }
		protected CWC.TextBox _txtAttentionTo { get { return Page.FindCamstarControl("LotShip_AttentionTo") as CWC.TextBox; } }
		protected CWC.CheckBox _chkReShip { get { return Page.FindCamstarControl("LotShip_ReShip") as CWC.CheckBox; } }
		protected JQDataGrid _grdValidValues { get { return Page.FindCamstarControl(cValuesGrid) as JQDataGrid; } }
		protected JQDataGrid _grdSvcAttrs { get { return Page.FindCamstarControl(cAttrGrid) as JQDataGrid; } }
        protected CWC.ContainerList _conHiddenContainer { get { return Page.FindCamstarControl("HiddenContainer") as CWC.ContainerList; } }

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
				_grdLotDetails.BoundContext.Fields.Add(new JQField("_spacer") { LabelText = "&nbsp;", Visible = true } );
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
			_ndoToFactory.ClearData();
			_ndoToFactory.ClearSelectionValues();
			_ndoToProcess.ClearData();
			_ndoToProcess.ClearSelectionValues();
			_txtDocNo.ClearData();
			_txtShipNo.ClearData();
			_txtAttentionTo.ClearData();
			_chkReShip.ClearData();
			_grdSvcAttrs.ClearData();
			_grdValidValues.ClearData();
			ResetLotDetailsGrid();

            _txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
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
			var objService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });	// LotShipService oService = new LotShipService(fs.CurrentUserProfile);

			// create / init the service data and service info objects
			var objSvcData = CreateServiceData(sServiceType);		// LotShip oSvcData = new LotShip();
			(objSvcData as LotShip).SelectionId = sSelectionId;
			var objSvcInfo = CreateServiceInfo(sServiceType);		// LotShip_Info oSvcInfo = new LotShip_Info();

			(objSvcInfo as LotShip_Info).ServiceAttrsDetailsSelection = new ServiceAttrsDetails_Info();			//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection", new ServiceAttrsDetails_Info());
			(objSvcInfo as LotShip_Info).ServiceAttrsDetailsSelection.Attribute = new Info(true);				//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.Attribute", new Info(true));
			(objSvcInfo as LotShip_Info).ServiceAttrsDetailsSelection.AttributeName = new Info(true);			//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.AttributeName", new Info(true));
			(objSvcInfo as LotShip_Info).ServiceAttrsDetailsSelection.AlternateName1 = new Info(true); 			//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.AlternateName1", new Info(true));
			(objSvcInfo as LotShip_Info).ServiceAttrsDetailsSelection.AlternateName2 = new Info(true);			//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.AlternateName2", new Info(true));
			(objSvcInfo as LotShip_Info).ServiceAttrsDetailsSelection.AttributeValue = new Info(true);			//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.AttributeValue", new Info(true));
			(objSvcInfo as LotShip_Info).ServiceAttrsDetailsSelection.AttributeRevision = new Info(true);		//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.AttributeRevision", new Info(true));
			(objSvcInfo as LotShip_Info).ServiceAttrsDetailsSelection.AccessLevel = new Info(true);				//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.AccessLevel", new Info(true));
			(objSvcInfo as LotShip_Info).ServiceAttrsDetailsSelection.FieldType = new Info(true);				//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.FieldType", new Info(true));
			(objSvcInfo as LotShip_Info).ServiceAttrsDetailsSelection.IsRequired = new Info(true);				//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.IsRequired", new Info(true));
			(objSvcInfo as LotShip_Info).ServiceAttrsDetailsSelection.ServiceAttrsSetupName = new Info(true);	//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.ServiceAttrsSetupName", new Info(true));
			(objSvcInfo as LotShip_Info).ServiceAttrsDetailsSelection.ObjectTypeName = new Info(true);			//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.ObjectTypeName", new Info(true));
			(objSvcInfo as LotShip_Info).ServiceAttrsDetailsSelection.ValidValues = new AttributeValidValuesChanges_Info();	//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.ValidValues", new AttributeValidValuesChanges_Info());
			(objSvcInfo as LotShip_Info).ServiceAttrsDetailsSelection.ValidValues.AttributeValue = new Info(true);		//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.ValidValues.AttributeValue", new Info(true));
			(objSvcInfo as LotShip_Info).ServiceAttrsDetailsSelection.ValidValues.AttributeRevision = new Info(true);	//(objSvcInfo as ICreator).SetValue("ServiceAttrsDetailsSelection.ValidValues.AttributeRevision", new Info(true));

			// init the request and result object
			var objRequest = WCFObject.CreateObject(sServiceType + "_Request");	// LotShip_Request oRequest = new LotShip_Request();
			(objRequest as Request).Info = (objSvcInfo as LotShip_Info);
			Result objResult = new Result();

			// submit the transaction as IShopFloorBase
			OM.ResultStatus oResStat = (objService as IShopFloorBase).ResolveSelectionId(objSvcData, (objRequest as Request), out objResult);

			if (oResStat.IsSuccess && (objResult.Value as LotShip).ServiceAttrsDetailsSelection != null)
            {
                // bind result to the grid
                _grdSvcAttrs.ClearData();
				_grdSvcAttrs.Data = (objResult.Value as LotShip).ServiceAttrsDetailsSelection.ToArray();
				_grdSvcAttrs.OriginalData = (objResult.Value as LotShip).ServiceAttrsDetailsSelection.ToArray();

                // create Valid Values table
				DataTable validValuesDT = new DataTable();
				int countSvcAttr = (objResult.Value as LotShip).ServiceAttrsDetailsSelection.Count();
				validValuesDT.Columns.Add("Attribute", typeof(String));
				validValuesDT.Columns.Add("AttributeValue", typeof(String));
				validValuesDT.Columns.Add("AttributeRevision", typeof(String));
				for (int i = 0; i < countSvcAttr; i++)
				{
					if (((objResult.Value as LotShip).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues != null)
				    {
						int countValidValues = ((objResult.Value as LotShip).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.Count();
				        for (int x = 0; x < countValidValues; x++)
				        {
				            DataRow dtRow = validValuesDT.NewRow();
							dtRow.SetField("Attribute", ((objResult.Value as LotShip).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).Attribute.Name);
							dtRow.SetField("AttributeValue", (((objResult.Value as LotShip).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.GetValue(x) as AttributeValidValuesChanges).AttributeValue);
							dtRow.SetField("AttributeRevision", (((objResult.Value as LotShip).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.GetValue(x) as AttributeValidValuesChanges).AttributeRevision);
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
		/// fetch additional data
		/// </summary>		
		private void FetchMoreData(string sEventName)
		{
			if (_grdLotDetails.Data == null || _grdLotDetails.GridContext.GetTotalRows() < 1)
				return;

			if (sEventName == cShipToProcess && string.IsNullOrEmpty(_ndoToFactory.Text))
			{
				_ndoToProcess.ClearData();
				_ndoToProcess.ClearSelectionValues();
				return;
			}

			// get selectionId from 1st grid row
			string sSelectionId = _grdLotDetails.GridContext.GetCell("000000", "Lot").ToString();
			
			// get the session and user profile
			var fs = FrameworkManagerUtil.GetFrameworkSession();

			// get the page primary service type, and run appropriate constructor for service object
			string sServiceType = Page.PrimaryServiceType;
			var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
			var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
			var objService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });	// LotShipService oService = new LotShipService(fs.CurrentUserProfile);

			// create / init the service data and service info objects
			var objSvcData = CreateServiceData(sServiceType);		// LotShip oSvcData = new LotShip();
			// (objSvcData as LotShip).SelectionId = sSelectionId;
			(objSvcData as LotShip).Container = new ContainerRef();
			(objSvcData as LotShip).Container.Name = sSelectionId;
			var objSvcInfo = CreateServiceInfo(sServiceType);		// LotShip_Info oSvcInfo = new LotShip_Info();

			// prepare the request
			switch (sEventName)
			{
				case cDocNo:
				{
					// request document number info
					(objSvcInfo as LotShip_Info).DocumentNumber = FieldInfoUtil.RequestValue();
					break;
				}            
			}

			// init the request and result object
			var objRequest = WCFObject.CreateObject(sServiceType + "_Request");	// LotShip_Request oRequest = new LotShip_Request();
			(objRequest as Request).Info = (objSvcInfo as LotShip_Info);		// oRequest.Info = objSvcInfo;
			Result objResult = new Result();									// LotShip_Result oResult = new LotShip_Result();

			// execute the transaction
			OM.ResultStatus oResStat = (objService as IShopFloorBase).GetEnvironment(objSvcData, (objRequest as Request), out objResult);
			//OM.ResultStatus oResStat = oService.GetEnvironment(oSvcData, oRequest, out oResult);

			// handle results
			if (oResStat.IsSuccess && objResult.Environment != null)
			{
				switch (sEventName)
				{
					case cDocNo:
					{
						if (objResult.Value != null && (objResult.Value as LotShip).DocumentNumber != null)
						{
							_txtDocNo.Data = (objResult.Value as LotShip).DocumentNumber;
						}
						break;
					}                    
				}
			}
			else
			{
				DisplayMessage(oResStat);
			}
		} //end FetchMoreData

		/// <summary>
		/// transfer data from lot selection popup via data envelope to lot details grid 
		/// and initialize other dependend fields like ship-to-factory and ship-to-process
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
							FetchMoreData(cShipToFactory);
							FetchMoreData(cShipToProcess);
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
			var attrVal = Page.DataContract.GetValueByName("LotShip_ReturnedValue");
			var attrRev = Page.DataContract.GetValueByName("LotShip_ReturnedRevision");
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
		/// and fills in dependend fields (like ship-to-factory, ship-to-process, ...)
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
					var objService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });	// LotShipService oService = new LotShipService(fs.CurrentUserProfile);

					// create / init the service data and service info objects
					var objSvcData = CreateServiceData(sServiceType);		// LotShip oSvcData = new LotShip();
					(objSvcData as LotShip).SelectionId = (string)_txtSelectionId.Data;
					var objSvcInfo = CreateServiceInfo(sServiceType);		// LotShip_Info oSvcInfo = new LotShip_Info();
					(objSvcInfo as LotShip_Info).Containers = new Info(true); // oSvcInfo.Containers = FieldInfoUtil.RequestValue();

					// init the request and result object
					var objRequest = WCFObject.CreateObject(sServiceType + "_Request");	// LotShip_Request oRequest = new LotShip_Request();
					(objRequest as Request).Info = (objSvcInfo as LotShip_Info); // oRequest.Info = oSvcInfo;
					Result objResult = new Result();

					// execute to request the value(s)
					OM.ResultStatus oResStat = (objService as IShopFloorBase).ResolveSelectionId(objSvcData, (objRequest as Request), out objResult);
					// OM.ResultStatus oResStat = oService.ResolveSelectionId(oSvcData, oRequest, out oResult);

					if (oResStat.IsSuccess && (objResult.Value as LotShip).Containers != null)
					{
						Boolean bIsFirst = true;
						JQDataGrid _gridContainers = Page.FindCamstarControl(cLotGrid) as JQDataGrid;
						foreach (OM.ContainerRef oContainer in (objResult.Value as LotShip).Containers)
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
                                    FetchMoreData(cDocNo);
                                    _conHiddenContainer.Data = oContainer;                                    
									bIsFirst = false;
								}
							}
						}
						if ((objResult.Value as LotShip).DocumentNumber != null)
						{
							_txtDocNo.Data = (objResult.Value as LotShip).DocumentNumber;
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

		#endregion // Handlers

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
				if (serviceData is LotShip)
				{
					(serviceData as LotShip).ServiceAttrsDetails = new ServiceAttrsDetails[srvAttrsDetails.Count()];

					// collect the attributes
					for (int i = 0; i < srvAttrsDetails.Count(); i++)
					{
						(serviceData as LotShip).ServiceAttrsDetails[i] = new ServiceAttrsDetails();
						(serviceData as LotShip).ServiceAttrsDetails[i].Attribute = new NamedObjectRef();
						(serviceData as LotShip).ServiceAttrsDetails[i].Attribute.Name = srvAttrsDetails[i].Attribute.Name;
						(serviceData as LotShip).ServiceAttrsDetails[i].FieldType = srvAttrsDetails[i].FieldType;
						(serviceData as LotShip).ServiceAttrsDetails[i].ServiceAttrsSetupName = srvAttrsDetails[i].ServiceAttrsSetupName;
						(serviceData as LotShip).ServiceAttrsDetails[i].AttributeValue = srvAttrsDetails[i].AttributeValue;
						(serviceData as LotShip).ServiceAttrsDetails[i].AttributeRevision = srvAttrsDetails[i].AttributeRevision;
					}
				}
			}

			//Containers
			if (_grdLotDetails.Data != null)
			{
				int intTotalContainers = _grdLotDetails.BoundContext.GetTotalRows();
				(serviceData as LotShip).Containers = new ContainerRef[intTotalContainers];

				// collect the containers
				for (int x = 0; x < intTotalContainers; x++)
				{
					(serviceData as LotShip).Containers[x] = new ContainerRef();
					(serviceData as LotShip).Containers[x].Name = _grdLotDetails.GridContext.GetCell(x.ToString().PadLeft(6, '0'), "Lot").ToString();
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
					case cDocNo:
					{
						FetchMoreData(cDocNo);
					    break;
					}
					case cReset:
					{
						// called on ResetButton click (ID = 'ctl00_WebPartManager_ButtonsBar_ResetAction')
						// this button click is also triggered via JS function "SS_ContainerGrid_rowDelete"
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
					var attrSel = Page.DataContract.GetValueByName("LotShip_SelectedAttribute");
					var lotSel = Page.DataContract.GetValueByName("LotShip_ContainerDataEnvDM");
					Boolean bVVPopupClosed = (attrSel != null);
					Boolean bLSPopupClosed = (lotSel != null);
					if (bLSPopupClosed && _envContainer != null) // [NH] there's data in LotShip_ContainerDataEnvDM, so assume LotSelection popup closed
					{

						// set first item from strLotSelectionIDs into _txtSelectionId.Data when LotSelection popup closed
						string[] strLotSelectionIDs = lotSel as string[];
						if (strLotSelectionIDs != null && strLotSelectionIDs.Length != 0)
						{
							if (strLotSelectionIDs.Length > 1)
							{
								_txtSelectionId.Data = strLotSelectionIDs[0];
								string[] strNewLotSelIDs = new String[strLotSelectionIDs.Length - 1]; // get remaining Lot set into strNewLotSelIDs
								Array.Copy(strLotSelectionIDs, 1, strNewLotSelIDs, 0, strLotSelectionIDs.Length - 1);
								SetLotDetailsGrid(strNewLotSelIDs);
							}
							else
							{
								_txtSelectionId.Data = strLotSelectionIDs[0];
							}
						}

					}
					if (bVVPopupClosed) // [NH] there's data in LotShip_SelectedAttribute so assume ValidValues popup closed
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



