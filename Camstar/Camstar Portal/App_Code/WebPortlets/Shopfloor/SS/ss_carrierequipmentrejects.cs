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
	public class SS_CarrierEquipmentRejects : MatrixWebPart
	{
		#region PrivateConstants

		private const string cLotGrid = "LotDetailsGrid";
		private const string cRejectsGrid = "RejectDetailsGrid";
		private const string cReset = "Reset";

		private const string cColDefects = "DefectQty";
		private const string cColReworkable = "ReworkableRejectQty";
		private const string cColUnidentifiable = "UnidentifiableRejectQty";

		private const string cAllowDefects = "AllowDefectsDM";
		private const string cAllowReworkable = "AllowReworkableDM";
		private const string cAllowUnidentifiable = "AllowUnidentifiableDM";

		#endregion // PrivateConstants

		#region PrivateProperties

		protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("CarrierEquipmentRejects_Employee") as CWC.NamedObject; } }
		protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("CarrierEquipmentRejects_ComputerName") as CWC.TextBox; } }
		protected CWC.NamedObject _ndoEquipment { get { return Page.FindCamstarControl("CarrierEquipmentRejects_Equipment") as CWC.NamedObject; } }
		protected CWC.TextBox _txtMaxRejectQty { get { return Page.FindCamstarControl("CarrierEquipmentRejects_MaxRejectQty") as CWC.TextBox; } }
		protected JQDataGrid _grdLotDetails { get { return FindCamstarControl(cLotGrid) as JQDataGrid; } }
		protected JQDataGrid _grdRejectDetails { get { return Page.FindCamstarControl(cRejectsGrid) as JQDataGrid; } }

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
			_ndoEquipment.ClearData();
			_txtMaxRejectQty.ClearData();
			_ndoEmployee.ClearData();
			_grdRejectDetails.ClearData();
			Page.DataContract.SetValueByName(cAllowDefects, new string('1', 1));
			Page.DataContract.SetValueByName(cAllowReworkable, new string('0', 1));
			Page.DataContract.SetValueByName(cAllowUnidentifiable, new string('0', 1));
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
		private OM.ResultStatus InitDetailGrids()
		{
			// get the session and user profile
			var fs = FrameworkManagerUtil.GetFrameworkSession();

			// get the page primary service type, and run appropriate constructor for service object
			string sServiceType = Page.PrimaryServiceType;
			var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
			var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
			var objService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });	// CarrierEquipmentRejectsService oService = new CarrierEquipmentRejectsService(fs.CurrentUserProfile);

			// create / init the service data and service info objects
			var objSvcData = CreateServiceData(sServiceType);		// CarrierEquipmentRejects oSvcData = new CarrierEquipmentRejects();
			(objSvcData as CarrierEquipmentRejects).Equipment = new NamedObjectRef(_ndoEquipment.Data.ToString());
			var objSvcInfo = CreateServiceInfo(sServiceType);		// CarrierEquipmentRejects_Info oSvcInfo = new CarrierEquipmentRejects_Info();

			(objSvcInfo as CarrierEquipmentRejects_Info).Equipment = new Info(true);
			(objSvcInfo as CarrierEquipmentRejects_Info).MaxRejectQty = new Info(true);
			(objSvcInfo as CarrierEquipmentRejects_Info).AllowDefectQty = new Info(true);
			(objSvcInfo as CarrierEquipmentRejects_Info).AllowReworkableRejectQty = new Info(true);
			(objSvcInfo as CarrierEquipmentRejects_Info).AllowUnidentifiableRejectQty = new Info(true);
			(objSvcInfo as CarrierEquipmentRejects_Info).Containers = new Info(true);
			(objSvcInfo as CarrierEquipmentRejects_Info).CurrentDetails = new LotRejectsDetails_Info();
			(objSvcInfo as CarrierEquipmentRejects_Info).CurrentDetails.RequestValue = true;

			// init the request and result object
			var objRequest = WCFObject.CreateObject(sServiceType + "_Request");	// CarrierEquipmentRejects_Request oRequest = new CarrierEquipmentRejects_Request();
			(objRequest as Request).Info = (objSvcInfo as CarrierEquipmentRejects_Info);
			Result objResult = new Result();

			// execute to request the value(s)
			OM.ResultStatus oResStat = (objService as IShopFloorBase).ResolveSelectionId(objSvcData, (objRequest as Request), out objResult);
			// OM.ResultStatus oResStat = oService.ResolveSelectionId(oSvcData, oRequest, out oResult);

			if (oResStat.IsSuccess && (objResult.Value as CarrierEquipmentRejects).Containers != null)
			{
				// initialize containers list
				JQDataGrid gridContainers = Page.FindCamstarControl(cLotGrid) as JQDataGrid;
				if (gridContainers != null) // always true unless someone messed with the page ...
				{
					Boolean bHaveLots = gridContainers.Settings.Columns[0].Name == "Lot" && (gridContainers.Data as object[]).Length > 0;
					foreach (ContainerRef oContainersItem in (objResult.Value as CarrierEquipmentRejects).Containers)
					{
						string sLotName = oContainersItem.Name.ToString();
						string strSelectedGridId = "";
						// check if this lot already is in the grid (if there are any lots in the grid)
						if (bHaveLots)
						{
							strSelectedGridId = (gridContainers.GridContext as BoundContext).GetRowIdByCellValue("Lot", sLotName);
						}
						if (string.IsNullOrEmpty(strSelectedGridId)) // not in the grid: add it
						{
							SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "CarrierEquipmentRejects", sLotName
													, false, ref gridContainers, cLotGrid, true);
						}
					}

					// make sure the __STYLE column is not visible
					if (_grdLotDetails.GridContext != null && (_grdLotDetails.GridContext as BoundContext).Fields["__STYLE"] != null)
						(_grdLotDetails.GridContext as BoundContext).Fields["__STYLE"].Visible = false;

					// initialize rejects details list from the 1st lot only
					if (!bHaveLots)
					{
						Boolean bIsAllowed;
						_grdRejectDetails.ClearData();
						// set column "DefectQty" visibility based on "AllowDefectQty" value
						if ((objResult.Value as CarrierEquipmentRejects).AllowDefectQty != null)
						{
							bIsAllowed = (Boolean)((objResult.Value as CarrierEquipmentRejects).AllowDefectQty);
							Page.DataContract.SetValueByName(cAllowDefects, new string((bIsAllowed ? '1' : '0'), 1));
						}
						// set column "ReworkableRejectQty" visibility based on "AllowReworkableRejectQty" value
						if ((objResult.Value as CarrierEquipmentRejects).AllowReworkableRejectQty != null)
						{
							bIsAllowed = (Boolean)((objResult.Value as CarrierEquipmentRejects).AllowReworkableRejectQty);
							Page.DataContract.SetValueByName(cAllowReworkable, new string((bIsAllowed ? '1' : '0'), 1));
						}
						// set column "UnidentifiableRejectQty" visibility based on "AllowUnidentifiableRejectQty" value
						if ((objResult.Value as CarrierEquipmentRejects).AllowUnidentifiableRejectQty != null)
						{
							bIsAllowed = (Boolean)((objResult.Value as CarrierEquipmentRejects).AllowUnidentifiableRejectQty);
							Page.DataContract.SetValueByName(cAllowUnidentifiable, new string((bIsAllowed ? '1' : '0'), 1));
						}

						// set Details
						if ((objResult.Value as CarrierEquipmentRejects).CurrentDetails != null)
						{
							List<LotRejectsDetails> newRows = new List<LotRejectsDetails>();
							foreach (LotRejectsDetails oDetail in (objResult.Value as CarrierEquipmentRejects).CurrentDetails)
							{
								LotRejectsDetails row = (LotRejectsDetails)oDetail.Clone();
								newRows.Add(row);
							}
							(_grdRejectDetails.GridContext as BoundContext).Data = newRows.ToArray();
							_grdRejectDetails.BoundContext.LoadData();
						}

						// set Equipment
						if ((objResult.Value as CarrierEquipmentRejects).Equipment != null)
							_ndoEquipment.TextEditControl.Text = (objResult.Value as CarrierEquipmentRejects).Equipment.ToString();
						// set MaxRejectQty
						if ((objResult.Value as CarrierEquipmentRejects).MaxRejectQty != null)
							_txtMaxRejectQty.TextControl.Text = (objResult.Value as CarrierEquipmentRejects).MaxRejectQty.ToString();

					}
				}
			}

			return oResStat;
		} // end InitDetailGrids

		#endregion // PrivateFunctions

		#region Handlers

		/// <summary>
		/// triggered when the equipment name (in _ndoEquipment) is changed (params not used)
		/// </summary>
		/// <param name="sender" type="object"></param>
		/// <param name="e" type="EventArgs"></param>
		public void Equipment_DataChanged(object sender, EventArgs e)
		{
			try
			{
				Page.StatusBar.ClearMessage();
				if (_ndoEquipment.Data!=null && !string.IsNullOrWhiteSpace(_ndoEquipment.Data.ToString()))
				{
					OM.ResultStatus oResStat = InitDetailGrids();
					if (oResStat.IsSuccess)
					{
						CamstarWebControl.SetRenderToClient(_grdRejectDetails);
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
		} //end Equipment_DataChanged

		#endregion // Handlers

		#region Overrides

		/// <summary>
		// override of the Get Input Data method
		/// </summary>
		/// <param name="serviceData" type="Service"></param>
		public override void GetInputData(Service serviceData)
		{
			base.GetInputData(serviceData);

			//Rejects
			if (_grdRejectDetails.Data != null)  // _grdRejectDetails.BoundContext.Fields.Count
			{
				LotRejectsDetails[] rejectRows = _grdRejectDetails.Data as LotRejectsDetails[]; // current wafer details grid as array

				if (serviceData is CarrierEquipmentRejects && rejectRows.Length > 0) // some rows in the grid
				{
					(serviceData as CarrierEquipmentRejects).Details = new LotRejectsDetails[rejectRows.Length];
					Boolean bIsDefectAllowed = (_grdRejectDetails.GridContext as BoundContext).Fields[cColDefects] != null
												&& (_grdRejectDetails.GridContext as BoundContext).Fields[cColDefects].Visible;
					Boolean bIsReworkableAllowed = (_grdRejectDetails.GridContext as BoundContext).Fields[cColReworkable] != null
												&& (_grdRejectDetails.GridContext as BoundContext).Fields[cColReworkable].Visible;
					Boolean bIsUnidentifiableAllowed = (_grdRejectDetails.GridContext as BoundContext).Fields[cColUnidentifiable] != null
												&& (_grdRejectDetails.GridContext as BoundContext).Fields[cColUnidentifiable].Visible;

					for (int i = 0; i < rejectRows.Length; i++)
				    {
						(serviceData as CarrierEquipmentRejects).Details[i] = new LotRejectsDetails();
						if (rejectRows[i].LossReason != null)
						{
							(serviceData as CarrierEquipmentRejects).Details[i].LossReason = new NamedObjectRef();
							(serviceData as CarrierEquipmentRejects).Details[i].LossReason.Name = rejectRows[i].LossReason.Name;
						}
						(serviceData as CarrierEquipmentRejects).Details[i].LossReasonDescription = rejectRows[i].LossReasonDescription;
						if (rejectRows[i].RejectCategory != null)
						{
							(serviceData as CarrierEquipmentRejects).Details[i].RejectCategory = new NamedObjectRef();
							(serviceData as CarrierEquipmentRejects).Details[i].RejectCategory.Name = rejectRows[i].RejectCategory.Name;
						}
						(serviceData as CarrierEquipmentRejects).Details[i].RejectCause = rejectRows[i].RejectCause; 
						(serviceData as CarrierEquipmentRejects).Details[i].RejectComment = rejectRows[i].RejectComment;
						(serviceData as CarrierEquipmentRejects).Details[i].RejectQty = rejectRows[i].RejectQty;
						if (bIsDefectAllowed)
							(serviceData as CarrierEquipmentRejects).Details[i].DefectQty = rejectRows[i].DefectQty;
						if (bIsReworkableAllowed)
							(serviceData as CarrierEquipmentRejects).Details[i].ReworkableRejectQty = rejectRows[i].ReworkableRejectQty;
						if (bIsUnidentifiableAllowed)
							(serviceData as CarrierEquipmentRejects).Details[i].UnidentifiableRejectQty = rejectRows[i].UnidentifiableRejectQty;
				    }
				}
			}

		} //end GetInputData

		/// <summary>
		// override of the Pre Render method
		/// </summary>
		/// <param name="serviceData" type="Service"></param>
		protected override void OnPreRender(EventArgs e)
		{
			Boolean bIsAllowed = false;
			base.OnPreRender(e);

			// set column "DefectQty" visibility based on "AllowDefectQty" value stored in AllowDefectsDM
			if ((_grdRejectDetails.GridContext as BoundContext).Fields[cColDefects] != null)
			{
				var oDefectsAllowed = Page.DataContract.GetValueByName(cAllowDefects);
				bIsAllowed = (oDefectsAllowed != null && oDefectsAllowed.ToString() == "1");
				(_grdRejectDetails.GridContext as BoundContext).Fields[cColDefects].Visible = bIsAllowed;
				(_grdRejectDetails.GridContext as BoundContext).Fields[cColDefects].ReadOnly = !bIsAllowed;
			}
			// set column "ReworkableRejectQty" visibility based on "AllowReworkableRejectQty" value stored in AllowReworkableDM
			if ((_grdRejectDetails.GridContext as BoundContext).Fields[cColReworkable] != null)
			{
				var oReworkableAllowed = Page.DataContract.GetValueByName(cAllowReworkable);
				bIsAllowed = (oReworkableAllowed != null && oReworkableAllowed.ToString() == "1");
				(_grdRejectDetails.GridContext as BoundContext).Fields[cColReworkable].Visible = bIsAllowed;
				(_grdRejectDetails.GridContext as BoundContext).Fields[cColReworkable].ReadOnly = !bIsAllowed;
			}
			// set column "UnidentifiableRejectQty" visibility based on "AllowUnidentifiableRejectQty" value stored in AllowUnidentifiableDM
			if ((_grdRejectDetails.GridContext as BoundContext).Fields[cColUnidentifiable] != null)
			{
				var oUnidentifiableAllowed = Page.DataContract.GetValueByName(cAllowUnidentifiable);
				bIsAllowed = (oUnidentifiableAllowed != null && oUnidentifiableAllowed.ToString() == "1");
				(_grdRejectDetails.GridContext as BoundContext).Fields[cColUnidentifiable].Visible = bIsAllowed;
				(_grdRejectDetails.GridContext as BoundContext).Fields[cColUnidentifiable].ReadOnly = !bIsAllowed;
			}
		}
			
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




