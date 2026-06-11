/* Copyright 2019 Siemens */
using System;
using System.Collections;
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
	public class SS_TransferEquipmentRejects : MatrixWebPart
	{
		#region PrivateConstants

		private const string cLotGrid = "LotDetailsGrid";
		private const string cRejectsGrid = "RejectTransfersGrid";
		private const string cReset = "Reset";

		#endregion // PrivateConstants

		#region PrivateProperties

		protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("TransferEquipmentRejects_Employee") as CWC.NamedObject; } }
		protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("TransferEquipmentRejects_ComputerName") as CWC.TextBox; } }
		protected CWC.NamedObject _ndoEquipment { get { return Page.FindCamstarControl("TransferEquipmentRejects_Equipment") as CWC.NamedObject; } }
		protected JQDataGrid _grdLotDetails { get { return FindCamstarControl(cLotGrid) as JQDataGrid; } }
		protected JQDataGrid _grdRejectTransfers { get { return Page.FindCamstarControl(cRejectsGrid) as JQDataGrid; } }

		#endregion // PrivateProperties

		#region PrivateFunctions

		/// <summary>
		/// clears the lots grid (and adds an empty placeholder row)
		/// </summary>
		private void ResetLotDetailsGrid()
		{
			// clear any previous data
			_grdLotDetails.ClearData();

			// clear the headers, and add back the magical space col (for looks)
			if (_grdLotDetails.BoundContext.Fields.Count > 1)
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
			_ndoEquipment.ClearData();
			_ndoEmployee.ClearData();
			_grdRejectTransfers.ClearData();
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
		/// initializes the wafer details grid
		/// </summary>
		private OM.ResultStatus InitAllGrids()
		{
			// get the session and user profile
			var fs = FrameworkManagerUtil.GetFrameworkSession();

			// get the page primary service type, and run appropriate constructor for service object
			string sServiceType = Page.PrimaryServiceType;
			var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
			var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
			var objService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });	// TransferEquipmentRejectsService oService = new TransferEquipmentRejectsService(fs.CurrentUserProfile);

			// create / init the service data and service info objects
			var objSvcData = CreateServiceData(sServiceType);		// TransferEquipmentRejects oSvcData = new TransferEquipmentRejects();
			(objSvcData as TransferEquipmentRejects).Equipment = new NamedObjectRef(_ndoEquipment.TextEditControl.Text);
			var objSvcInfo = CreateServiceInfo(sServiceType);		// TransferEquipmentRejects_Info oSvcInfo = new TransferEquipmentRejects_Info();

			(objSvcInfo as TransferEquipmentRejects_Info).Equipment = new Info(true);
			(objSvcInfo as TransferEquipmentRejects_Info).Containers = new Info(true);
			(objSvcInfo as TransferEquipmentRejects_Info).EquipmentRejects = new EquipmentRejects_Info();
			(objSvcInfo as TransferEquipmentRejects_Info).EquipmentRejects.RequestValue = true;

			// init the request and result object
			var objRequest = WCFObject.CreateObject(sServiceType + "_Request");	// TransferEquipmentRejects_Request oRequest = new TransferEquipmentRejects_Request();
			(objRequest as Request).Info = (objSvcInfo as TransferEquipmentRejects_Info);
			Result objResult = new Result();

			// execute to request the value(s)
			OM.ResultStatus oResStat = (objService as IShopFloorBase).ResolveSelectionId(objSvcData, (objRequest as Request), out objResult);
			// OM.ResultStatus oResStat = oService.ResolveSelectionId(oSvcData, oRequest, out oResult);

			if (oResStat.IsSuccess && (objResult.Value as TransferEquipmentRejects).Containers != null)
			{
				// initialize containers list
				JQDataGrid gridContainers = Page.FindCamstarControl(cLotGrid) as JQDataGrid;
				if (gridContainers != null) // always true unless someone messed with the page ...
				{
					ResetLotDetailsGrid();
					int iLotCnt = 0;	// (gridContainers.Settings.Columns[0].Name == "Lot"
										// && gridContainers.Data != null
										// && (gridContainers.Data as object[]).Length > 0);
					foreach (ContainerRef oContainersItem in (objResult.Value as TransferEquipmentRejects).Containers)
					{
						string sLotName = oContainersItem.Name.ToString();
						string strSelectedGridId = "";
						// check if this lot already is in the grid (if there are any lots in the grid)
						if (iLotCnt > 0 && (gridContainers.GridContext as BoundContext).GetRowId(0) != null)
						{
							strSelectedGridId = (gridContainers.GridContext as BoundContext).GetRowIdByCellValue("Lot", sLotName);
						}
						if (string.IsNullOrEmpty(strSelectedGridId)) // not in the grid: add it
						{
							SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "TransferEquipmentRejects"
																		, sLotName, false, ref gridContainers, cLotGrid, true);
							iLotCnt++;
						}
					}

					// make sure the __STYLE column is not visible
					if (_grdLotDetails.GridContext != null && (_grdLotDetails.GridContext as BoundContext).Fields["__STYLE"] != null)
						(_grdLotDetails.GridContext as BoundContext).Fields["__STYLE"].Visible = false;

					// initialize rejects details list for lots on current equipment
					_grdRejectTransfers.ClearData();
					if (iLotCnt > 0)
					{
						if ((objResult.Value as TransferEquipmentRejects).EquipmentRejects != null)
						{
							List<EquipmentRejects> newRows = new List<EquipmentRejects>();
							foreach (EquipmentRejects oDetail in (objResult.Value as TransferEquipmentRejects).EquipmentRejects)
							{
								EquipmentRejects row = (EquipmentRejects)oDetail.Clone();
								newRows.Add(row);
							}
							(_grdRejectTransfers.GridContext as BoundContext).Data = newRows.ToArray();
							_grdRejectTransfers.BoundContext.LoadData();
						}
					}

					// set Equipment (to adjust upper/lower case)
					if ((objResult.Value as TransferEquipmentRejects).Equipment != null)
						_ndoEquipment.TextEditControl.Text = (objResult.Value as TransferEquipmentRejects).Equipment.ToString();
				}
			}
			else
			{
				_grdLotDetails.ClearData();
				_grdRejectTransfers.ClearData();
			}

			return oResStat;
		} // end InitAllGrids

		#endregion // PrivateFunctions

		#region Handlers

		/// <summary>
		/// triggered when an equipment name is entered in the equipment NDO field (params not used)
		/// </summary>
		/// <param name="sender" type="object"></param>
		/// <param name="e" type="EventArgs"></param>
		public void Equipment_DataChanged(object sender, EventArgs e)
		{
			try
			{
				Page.StatusBar.ClearMessage();

				if (_ndoEquipment.Data != null && !string.IsNullOrWhiteSpace(_ndoEquipment.TextEditControl.Text))
				{
					OM.ResultStatus oResStat = InitAllGrids();
					if (oResStat.IsSuccess)
					{
						CamstarWebControl.SetRenderToClient(_grdRejectTransfers);
						Page.RenderToClient = true;
					}
					else
					{
						ResetFields();
						DisplayMessage(oResStat);
					}
				}
				else
				{
					ResetFields();
				}
			}
			catch (Exception Ex)
			{
				DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
			}
		} //end Equipmen_DataChanged

		#endregion // Handlers

		#region Overrides

		/// <summary>
		// override of the Get Input Data method
		/// </summary>
		/// <param name="serviceData" type="Service"></param>
		public override void GetInputData(Service serviceData)
		{
			base.GetInputData(serviceData);

			//Container
			string sFirstCol = _grdLotDetails.BoundContext.Fields[1].ID;
			if (serviceData is TransferEquipmentRejects && _grdLotDetails.Data != null && sFirstCol == "Lot")
			{
				object[] curRows = (_grdLotDetails.GridContext as BoundContext).Data as object[];
				int iCurRows = curRows == null ? 0 : curRows.Length;
				int iSelRows = (_grdLotDetails.GridContext.SelectedRowIDs == null ? 0 : _grdLotDetails.GridContext.SelectedRowIDs.Count);
				int iSelIdx = -1;
				if (iCurRows == 1) // don't bother about selection - there's only one container
				{
					iSelIdx = 0;
				}
				else if (iSelRows > 0) // get rowId of 1st selected row
				{
					// get selected row IDs as array, choose 1st (and only) selected rowId
					string[] sSelRowIds = _grdLotDetails.GridContext.SelectedRowIDs.ToArray();
					iSelIdx = int.Parse(sSelRowIds[0]);
				}
				if (0 <= iSelIdx && iSelIdx < iCurRows)
				{
					(serviceData as TransferEquipmentRejects).Container = new ContainerRef();
					(serviceData as TransferEquipmentRejects).Container.Name = _grdLotDetails.GridContext.GetCell(iSelIdx, "Lot").ToString();
				}
			}

			//Rejects
			if (serviceData is TransferEquipmentRejects && _grdRejectTransfers.Data != null)  // _grdRejectTransfers.BoundContext.Fields.Count
			{
				EquipmentRejects[] rejectRows = _grdRejectTransfers.Data as EquipmentRejects[]; // current rejects grid as array

				if (rejectRows != null && rejectRows.Length > 0) // some rows in the grid
				{
					(serviceData as TransferEquipmentRejects).Details = new LotRejectsDetails[rejectRows.Length];
					for (int i = 0; i < rejectRows.Length; i++)
				    {
						(serviceData as TransferEquipmentRejects).Details[i] = new LotRejectsDetails();
						if (rejectRows[i].LossReason != null)
						{
							(serviceData as TransferEquipmentRejects).Details[i].LossReason = new NamedObjectRef();
							(serviceData as TransferEquipmentRejects).Details[i].LossReason.Name = rejectRows[i].LossReason.Name;
						}
						(serviceData as TransferEquipmentRejects).Details[i].DefectQty = (int)rejectRows[i].DefectQty;
						(serviceData as TransferEquipmentRejects).Details[i].RejectQty = (int)rejectRows[i].RejectQty;
						(serviceData as TransferEquipmentRejects).Details[i].ReworkableRejectQty = (int)rejectRows[i].ReworkableRejectQty;
						(serviceData as TransferEquipmentRejects).Details[i].UnidentifiableRejectQty = (int)rejectRows[i].UnidentifiableRejectQty;
				    }
				}
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
						Page.ClearValues();
						ResetFields();
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
				if (!Page.IsPostBack)
				{
					Page.ClearValues();
					ResetFields();
					SetPageActionServiceName();
					_txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
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
			}
		} //end PostExecute

		#endregion // Overrides
	}
}




