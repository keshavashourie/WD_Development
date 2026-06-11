/* Copyright 2024 Siemens */
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
	public class SS_LotCarriersSetup : scsShopfloorBase
	{
		#region PrivateConstants

		private const string cLotGrid = "LotDetailsGrid";
		private const string cCarriersGrid = "CarriersGrid";
		private const string cReset = "Reset";
		private const string cUp = "DownArrow";
		private const string cDown = "UpArrow";

		#endregion // PrivateConstants

		#region PrivateProperties

		protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("CarriersSetup_SelectionId") as CWC.TextBox; } }
		protected SEMI.AppCode.DataEnvelopControl _envContainer { get { return Page.FindCamstarControl("CarriersSetup_ContainerDE") as SEMI.AppCode.DataEnvelopControl; } }
		protected JQDataGrid _grdLotDetails { get { return FindCamstarControl(cLotGrid) as JQDataGrid; } }
		protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("CarriersSetup_Employee") as CWC.NamedObject; } }
		protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("CarriersSetup_ComputerName") as CWC.TextBox; } }
		protected CWC.NamedObject _ndoCarrier { get { return Page.FindCamstarControl("CarriersSetup_Carrier") as CWC.NamedObject; } }
		protected JQDataGrid _grdCarriers { get { return Page.FindCamstarControl(cCarriersGrid) as JQDataGrid; } }

		#endregion // PrivateProperties

		#region PrivateClasses

		private class SS_CarriersRow // rows for the "CarriersGrid" grid
		{
			private string sCarrier;

			public string Carriers
			{
				get { return sCarrier; }
				set { sCarrier = value; }
			}
		}

		#endregion // PrivateClasses

		#region PrivateFunctions

		/// <summary>
		/// clears the lots grid (and adds an empty placeholder row)
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
			_grdCarriers.ClearData();
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

		/// <summary>
		/// worker function for row adjustments: -1 is move down, +1 is move up, 0 is delete
		/// "down" refers to index (closer to 0), which is actually the up in the grid, therefore triggered by up-arrow button
		/// similarly "up" means "higher index value", which is down in the grid, therefore triggered by down-arrow button
		/// </summary>
		private void reassignRows(int opCode)
		{
			// get current rows as array
			SS_CarriersRow[] curRows = (_grdCarriers.GridContext as BoundContext).Data as SS_CarriersRow[];
			// get counters
			int iCurRows = curRows == null ? 0 : curRows.Length;
			int iSelRows = _grdCarriers.GridContext.SelectedRowIDs.Count;
			if (iCurRows > 0 && iSelRows > 0)
			{
				// get selected row IDs as array
				string[] sSelRowIds = _grdCarriers.GridContext.SelectedRowIDs.ToArray();
				Boolean[] bSelRows = new Boolean[iCurRows];
				int[] iNewIdxs = new int[iCurRows];
				Boolean bRowsMoved = false;

				// init helper arrays, mark selected rows
				for (int i = 0; i < iCurRows; i++)
				{
					bSelRows[i] = false; iNewIdxs[i] = -1;
				}
				for (int i = 0; i < iSelRows; i++)
				{
					int index = Int32.Parse(sSelRowIds[i]);
					if (0 <= index && index < iCurRows)
						bSelRows[index] = true;
				}

				// setup iteration boundaries, number of rows in new grid
				int iStart, iInc, iLast, iNewRows;
				if (opCode > 0) // moving rows up: traverse down from second-to-last row to first row
				{
					iStart = iCurRows - 2; iInc = -1; iLast = 0; iNewRows = iCurRows;
				}
				else if (opCode < 0) // moving rows down: traverse up from second row to last row
				{
					iStart = 1; iInc = 1; iLast = iCurRows - 1; iNewRows = iCurRows;
				}
				else // deleting rows: traverse up from first row to last row
				{
					iStart = 0; iInc = 1; iLast = iCurRows - 1; iNewRows = iCurRows - iSelRows;
					// alternative: count the non-selected rows
					// iNewRows=0; for (int i = 0; i < iCurRows; i++) if (!bSelRows[i]) ++iNewRows;
				}

				// start with an empty newRows list
				List<SS_CarriersRow> newRows = null;
				// determine new row sequence
				if (opCode == 0 && iNewRows > 0) // new row sequence when deleting rows
				{
					int iLastUsed = -1;
					for (int j = 0; j < iNewRows; ++j) // assign new rows, skipping deleted ones
					{
						// find next unused row to assign, adjust iLastUsed and stop loop when assigned
						for (int i = iLastUsed + iInc; i != iLast + iInc && iNewIdxs[j] < 0; i += iInc)
							if (!bSelRows[i])
								iNewIdxs[j] = iLastUsed = i;
					}
				}
				else if (opCode != 0 && iCurRows > 1) // new row sequence when moving rows
				{
					iNewIdxs[iStart - iInc] = iStart - iInc;
					for (int i = iStart; i != iLast + iInc; i += iInc)
					{
						if (bSelRows[i]) // swap with up/down neighbor
						{
							int x = iNewIdxs[i - iInc];	iNewIdxs[i - iInc] = i;	iNewIdxs[i] = x;
							bRowsMoved = true;
						}
						else // copy as is
						{
							iNewIdxs[i] = i;
						}
					}
				}

				// build up the newRows list if there are any rows in the new grid and op was delete or rows were moved
				if (iNewRows > 0 && (bRowsMoved || opCode == 0))
				{
					newRows = new List<SS_CarriersRow>();
					for (int j = 0; j < iNewRows; ++j)
					{
						SS_CarriersRow row = new SS_CarriersRow();
						row.Carriers = curRows[iNewIdxs[j]].Carriers;
						newRows.Add(row);
					}
				}

				// clear old grid and setup the new grid, if rows were deleted or actually moved
				if (bRowsMoved || opCode == 0)
				{
					_grdCarriers.ClearData();
					(_grdCarriers.GridContext as BoundContext).Data = (newRows != null) ? newRows.ToArray() : null;
					_grdCarriers.BoundContext.LoadData();
					CamstarWebControl.SetRenderToClient(_grdCarriers);
				}
			} // if iCurRows > 0 && iSelRows > 0
		} //end reassignRows

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
					var objService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });	// LotCarriersSetupService oService = new LotCarriersSetupService(fs.CurrentUserProfile);

					// create / init the service data and service info objects
					var objSvcData = CreateServiceData(sServiceType);		// LotCarriersSetup oSvcData = new LotCarriersSetup();
					(objSvcData as LotCarriersSetup).SelectionId = _txtSelectionId.TextControl.Text;
					var objSvcInfo = CreateServiceInfo(sServiceType);		// LotCarriersSetup_Info oSvcInfo = new LotCarriersSetup_Info();
					(objSvcInfo as LotCarriersSetup_Info).Containers = new Info(true);
					//(objSvcInfo as LotCarriersSetup_Info).LotCarriers = new Carrier_Info();
					//(objSvcInfo as LotCarriersSetup_Info).LotCarriers.Name = new Info(true);
                    (objSvcInfo as LotCarriersSetup_Info).CurrentLotCarriers = new Info(true);
                                      

					// init the request and result object
					var objRequest = WCFObject.CreateObject(sServiceType + "_Request");	// LotCarriersSetup_Request oRequest = new LotCarriersSetup_Request();
					(objRequest as Request).Info = (objSvcInfo as LotCarriersSetup_Info);
					Result objResult = new Result();

					// execute to request the value(s)
					OM.ResultStatus oResStat = (objService as IShopFloorBase).ResolveSelectionId(objSvcData, (objRequest as Request), out objResult);
					// OM.ResultStatus oResStat = oService.ResolveSelectionId(oSvcData, oRequest, out oResult);

					if (oResStat.IsSuccess)
					{ 
						if ((objResult.Value as LotCarriersSetup).Containers != null)
						{
							JQDataGrid gridContainers = Page.FindCamstarControl(cLotGrid) as JQDataGrid;
							if (gridContainers != null)
							{
								string lotName = (objResult.Value as LotCarriersSetup).Containers[0].Name.ToString();
								SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "LotCarriersSetup", lotName, true, ref gridContainers);
								_txtSelectionId.TextControl.Text = lotName;
								_grdCarriers.ClearData();
								_ndoCarrier.Text = "";

								if ((objResult.Value as LotCarriersSetup).CurrentLotCarriers != null)
								{
									List<SS_CarriersRow> newRows = new List<SS_CarriersRow>();
									for (int x = 0; x < (objResult.Value as LotCarriersSetup).CurrentLotCarriers.Length; x++)
									{
										OM.NamedObjectRef oRef = (objResult.Value as LotCarriersSetup).CurrentLotCarriers[x];
										SS_CarriersRow row = new SS_CarriersRow();
										row.Carriers = oRef.Name.ToString();
										newRows.Add(row);
									}
									
									////foreach (OM.Carrier oCarrier in (objResult.Value as LotCarriersSetup).LotCarriers)
									////{
									////    SS_CarriersRow row = new SS_CarriersRow();
									////    row.Carriers = oCarrier.Name.ToString();
									////    newRows.Add(row);
									////}
									(_grdCarriers.GridContext as BoundContext).Data = newRows.ToArray();
									_grdCarriers.BoundContext.LoadData();
									CamstarWebControl.SetRenderToClient(_grdCarriers);
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
		} //end SelectionIdControl_DataChanged

		/// <summary>
		/// triggered when a new value is set in the Carrier field (params not used)
		/// adjusts carriers grid accordingly
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void Carrier_DataChanged()
		{
			if (!string.IsNullOrEmpty(_ndoCarrier.Text)) // add ndoCarrier.Text as new row to Carriers grid
			{
				string newName = _ndoCarrier.Text;
				SS_CarriersRow[] curRows = (_grdCarriers.GridContext as BoundContext).Data as SS_CarriersRow[];
				Boolean nameFound = false;
				int maxLen = curRows == null ? 0 : curRows.Length;
				for (int i = 0; i < maxLen && !nameFound; ++i)
				{
					if (newName == curRows[i].Carriers)
						nameFound = true;
				}
				if (!nameFound)
				{
					List<SS_CarriersRow> newRows = new List<SS_CarriersRow>();
					for (int i = 0; i <= maxLen; ++i)
					{
						SS_CarriersRow row = new SS_CarriersRow();
						row.Carriers = ((i == maxLen) ? newName : curRows[i].Carriers);
						newRows.Add(row);
					}

					_grdCarriers.ClearData();
					(_grdCarriers.GridContext as BoundContext).Data = newRows.ToArray();
					_grdCarriers.BoundContext.LoadData();
					CamstarWebControl.SetRenderToClient(_grdCarriers);
					_ndoCarrier.Text = "";
				}
			}
		} //end Carrier_DataChanged

		/// <summary>
		/// triggered when a carrier grid row should move up in the array (params not used)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void CarrierGrid_RowMoveUp()
		{
			if (_grdCarriers.GridContext.SelectedRowIDs != null
				&& _grdCarriers.GridContext.SelectedRowIDs.Count > 0)
			{
				reassignRows(1);
			}
		} //end CarrierGrid_RowMoveUp

		/// <summary>
		/// triggered when a carrier grid row should move down in the array (params not used)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void CarrierGrid_RowMoveDown()
		{
			if (_grdCarriers.GridContext.SelectedRowIDs != null
				&& _grdCarriers.GridContext.SelectedRowIDs.Count > 0)
			{
				reassignRows(-1);
			}
		} //end CarrierGrid_RowMoveDown

		/// <summary>
		/// triggered when a row is added to carrier grid (params not used) -- [NH] currently not used
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void CarrierGrid_RowAdded()
		{
		} //end CarrierGrid_RowAdded

		/// <summary>
		/// triggered when a row is deleted from carrier grid (params not used)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void CarrierGrid_RowDeleted()
		{
			if (_grdCarriers.GridContext.SelectedRowIDs != null
				&& _grdCarriers.GridContext.SelectedRowIDs.Count > 0)
			{
				reassignRows(0);
			}
		} //end CarrierGrid_RowDeleted

		#endregion // Handlers

		#region Overrides

		/// <summary>
		// override of the Get Input Data method
		/// </summary>
		/// <param name="serviceData" type="Service"></param>
		public override void GetInputData(Service serviceData)
		{
			base.GetInputData(serviceData);

			//Carriers
			if (_grdCarriers.Data != null)  // _grdCarriers.BoundContext.Fields.Count
			{
				SS_CarriersRow[] carrierRows = _grdCarriers.Data as SS_CarriersRow[];
				string cn0 = _grdCarriers.GridContext.GetCell(0, "Carriers").ToString();
				if (serviceData is LotCarriersSetup && carrierRows.Length > 0)
				{
					(serviceData as LotCarriersSetup).Carriers = new NamedObjectRef[carrierRows.Length];
					for (int i = 0; i < carrierRows.Length; i++)
				    {
						(serviceData as LotCarriersSetup).Carriers[i] = new NamedObjectRef();
						(serviceData as LotCarriersSetup).Carriers[i].Name = carrierRows[i].Carriers;
				    }
				}
			}

			//Container
			if (!string.IsNullOrWhiteSpace(_txtSelectionId.TextControl.Text))
			{
				(serviceData as LotCarriersSetup).Container = new ContainerRef();
				(serviceData as LotCarriersSetup).Container.Name = _txtSelectionId.TextControl.Text;
			}
		} //end GetInputData

		/// <summary>
		// override of the Web Part Custom Action method
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
						ResetFields();
						_txtSelectionId.Focus();
						break;
					}
					case cUp: // down-arrow default action -- [NH] not currently used
					{
						CarrierGrid_RowMoveUp();
						break;
					}
					case cDown: // up-arrow default action -- [NH] not currently used
					{
						CarrierGrid_RowMoveDown();
						break;
					}
				}
			}
		} //end WebPartCustomAction

		/// <summary>
		// override of the On Load event handler
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
					var lotSel = Page.DataContract.GetValueByName("CarriersSetup_ContainerDataEnvDM");
					Boolean bLSPopupClosed = (lotSel != null);
					if (bLSPopupClosed && _envContainer != null) // [NH] there's data in LotCarriersSetup_ContainerDataEnvDM, so assume LotSelection popup closed
					{
						if (lotSel != null) // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
						{
							_envContainer.SS_ContainersList = lotSel as string[];
						}

						if (_envContainer.SS_ContainersList != null) // only true when lotselection popup closes
						{
							string sContainer = _envContainer.SS_ContainersList[0];
							_envContainer.SS_ContainersList = null;
							_txtSelectionId.ClearData();
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
		// override of the Post Execute event handler
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




