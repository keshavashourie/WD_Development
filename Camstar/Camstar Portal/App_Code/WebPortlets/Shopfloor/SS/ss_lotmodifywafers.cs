/* Copyright 2025 Siemens */
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
	public class SS_LotModifyWafers : scsShopfloorBase
	{
		#region PrivateConstants

		private const string cLotGrid = "LotDetailsGrid";
		private const string cWafersGrid = "WaferDetailsGrid";
		private const string cReset = "Reset";
		private const string cUp = "DownArrow";
		private const string cDown = "UpArrow";

		#endregion // PrivateConstants

		#region PrivateProperties

		protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("LotModifyWafers_Employee") as CWC.NamedObject; } }
		protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("LotModifyWafers_ComputerName") as CWC.TextBox; } }
		protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotModifyWafers_SelectionId") as CWC.TextBox; } }
		protected SEMI.AppCode.DataEnvelopControl _envContainer { get { return Page.FindCamstarControl("LotModifyWafers_ContainerDE") as SEMI.AppCode.DataEnvelopControl; } }
		protected JQDataGrid _grdLotDetails { get { return FindCamstarControl(cLotGrid) as JQDataGrid; } }
		protected CWC.CheckBox _chkUpdateOnly { get { return Page.FindCamstarControl("LotModifyWafers_UpdateOnly") as CWC.CheckBox; } }
		protected JQDataGrid _grdWaferDetails { get { return Page.FindCamstarControl(cWafersGrid) as JQDataGrid; } }
		protected CWC.Button _btnAutoGenerate { get { return Page.FindCamstarControl("AutoGenerateButton") as CWC.Button; } }
		protected SEMI.AppCode.DataEnvelopControl _envOrigWafer { get { return Page.FindCamstarControl("LotModifyWafers_OrigWaferDE") as SEMI.AppCode.DataEnvelopControl; } }

		#endregion // PrivateProperties

		#region PrivateFunctions

		/// <summary>
		/// assigns "src" value to "dst" if "src" is really different from "orig"
		/// </summary>
		private Primitive<T> ChangedValue<T>(Primitive<T> oSrc, Primitive<T> oOrig = null)
		{
			Boolean bIsOrigEmpty = (oOrig.IsNullOrEmpty() || string.IsNullOrEmpty(oOrig.ToString()));
			Boolean bIsSrcEmpty = (oSrc.IsNullOrEmpty() || string.IsNullOrEmpty(oSrc.ToString()));
			if ((bIsOrigEmpty != bIsSrcEmpty) || (!bIsOrigEmpty && !bIsSrcEmpty && !oSrc.Value.Equals(oOrig.Value)))
				return oSrc;
			else
				return null;
		} // end ChangedValue<T>

		/// <summary>
		/// assigns "src" value to "dst" if "src" is really different from "orig"
		/// </summary>
		private NamedObjectRef ChangedValue(NamedObjectRef oSrc, NamedObjectRef oOrig = null)
		{
			Boolean bIsOrigEmpty = (oOrig.IsNullOrEmpty() || string.IsNullOrEmpty(oOrig.ToString()));
			Boolean bIsSrcEmpty = (oSrc.IsNullOrEmpty() || string.IsNullOrEmpty(oSrc.ToString()));
			if ((bIsOrigEmpty != bIsSrcEmpty) || (!bIsOrigEmpty && !bIsSrcEmpty && !oSrc.Equals(oOrig)))
			{
				if (oOrig != null)
				{
					if (oSrc.IsNullOrEmpty())
						return new NamedObjectRef("");
					else
						return oSrc;
				}
				else
					return oSrc;
			}						
			else
				return null;
		} // end ChangedValue

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
		private void ResetFields(Boolean bIncludeSelectionId=true)
		{
			if (bIncludeSelectionId) 
				_txtSelectionId.ClearData();
			_envContainer.ClearData();
			_ndoEmployee.ClearData();
			_grdWaferDetails.ClearData();
			ResetLotDetailsGrid();
			_envOrigWafer.ClearData();
			_chkUpdateOnly.Data = true;
			_btnAutoGenerate.Enabled = false;
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
		private void ReassignRows(int opCode)
		{
			// get current rows as array
			ModifyWafersDetails[] curRows = (_grdWaferDetails.GridContext as BoundContext).Data as ModifyWafersDetails[];
			// get counters
			int iCurRows = curRows == null ? 0 : curRows.Length;
			int iSelRows = _grdWaferDetails.GridContext.SelectedRowIDs.Count;
			if (iCurRows > 0 && iSelRows > 0)
			{
				// get selected row IDs as array
				string[] sSelRowIds = _grdWaferDetails.GridContext.SelectedRowIDs.ToArray();

                // foreach of the selected RowIDs, generate their new rowIDs to auto select after the move
                List<string> sNewSelRowIds = new List<string>();
                foreach (string sSelRow in sSelRowIds)
                {
                    int iRow = -1;
                    try { iRow = int.Parse(sSelRow); }
                    catch { iRow = -1; }

                    if (iRow != -1)
                    {
                        if (opCode == 1) // increment the index
                            iRow++;

                        if (opCode == -1)
                            iRow--;

                        if (iRow >= 0)
                            sNewSelRowIds.Add(iRow.ToString().PadLeft(6, '0'));
                    }
                }


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
				List<ModifyWafersDetails> newRows = null;
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
							int x = iNewIdxs[i - iInc]; iNewIdxs[i - iInc] = i; iNewIdxs[i] = x;
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
					newRows = new List<ModifyWafersDetails>();
					for (int j = 0; j < iNewRows; ++j)
					{
						//ModifyWafersDetails row = new ModifyWafersDetails();
						//row = curRows[iNewIdxs[j]];
						newRows.Add(curRows[iNewIdxs[j]]);
					}
				}

				// clear old grid and setup the new grid, if rows were deleted or actually moved
				if (bRowsMoved || opCode == 0)
				{
					_grdWaferDetails.ClearData();
					(_grdWaferDetails.GridContext as BoundContext).Data = (newRows != null) ? newRows.ToArray() : null;
					_grdWaferDetails.BoundContext.LoadData();

                    CamstarWebControl.SetRenderToClient(_grdWaferDetails);
				}
			} // if iCurRows > 0 && iSelRows > 0
		} //end ReassignRows

		/// <summary>
		/// initializes the wafer details grid
		/// </summary>
		private OM.ResultStatus InitWaferDetailsGrid()
		{
			// get the session and user profile
			var fs = FrameworkManagerUtil.GetFrameworkSession();

			// get the page primary service type, and run appropriate constructor for service object
			string sServiceType = Page.PrimaryServiceType;
			var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
			var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
			var objService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });	// LotModifyWafersService oService = new LotModifyWafersService(fs.CurrentUserProfile);

			// create / init the service data and service info objects
			var objSvcData = CreateServiceData(sServiceType);		// LotModifyWafers oSvcData = new LotModifyWafers();
			(objSvcData as LotModifyWafers).SelectionId = (string)_txtSelectionId.Data;
			var objSvcInfo = CreateServiceInfo(sServiceType);		// LotModifyWafers_Info oSvcInfo = new LotModifyWafers_Info();

			(objSvcInfo as LotModifyWafers_Info).SelectionContainer = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).Qty = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).Qty2 = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers = new LotWafers_Info();
			(objSvcInfo as LotModifyWafers_Info).LotWafers.WaferScribeNumber = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.WaferNumber = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.RequireDataCollection = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.RequireTracking = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.NDPW = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.GoodQty = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.WaferProduct = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.Grade = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.ProductionGrade = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.ChipGrade = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.VendorLotNumber = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.VendorName = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.WaferSize = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.DieSize = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.WaferThickness = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.OxideThickness = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.BrokenPieces = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.Brightness = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.FilmId = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.Color = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.Flux = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.VF = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.SortIQCResult = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.SortIQCComments = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.SortEquipment = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.SortNoteCode = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.ResortNoteCode = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.SortComments = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.SortSQAResult = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.SortSQAComments = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.TxnTimestamp = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.Username = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.WaferBatchId = new Info(true);
			(objSvcInfo as LotModifyWafers_Info).LotWafers.WaferSequence = new Info(true);
			//(objSvcInfo as LotModifyWafers_Info).LotWafers.InventoryId = new Info(true);

			// init the request and result object
			var objRequest = WCFObject.CreateObject(sServiceType + "_Request");	// LotModifyWafers_Request oRequest = new LotModifyWafers_Request();
			(objRequest as Request).Info = (objSvcInfo as LotModifyWafers_Info);
			Result objResult = new Result();

			// execute to request the value(s)
			OM.ResultStatus oResStat = (objService as IShopFloorBase).ResolveSelectionId(objSvcData, (objRequest as Request), out objResult);
			// OM.ResultStatus oResStat = oService.ResolveSelectionId(oSvcData, oRequest, out oResult);

			if (oResStat.IsSuccess && (objResult.Value as LotModifyWafers).SelectionContainer != null)
			{
				JQDataGrid gridContainers = Page.FindCamstarControl(cLotGrid) as JQDataGrid;
				if (gridContainers != null) // always true unless someone messed with the page ...
				{
					string lotName = (objResult.Value as LotModifyWafers).SelectionContainer.Name.ToString();	// .Containers[0].Name.ToString();
					SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "LotModifyWafers", lotName, true, ref gridContainers, cLotGrid);
                    //Clear WaferDetailsgrid
                    _grdWaferDetails.ClearData();
                    _envOrigWafer.ClearData();
					if ((objResult.Value as LotModifyWafers).LotWafers != null)
					{
						List<ModifyWafersDetails> newRows = new List<ModifyWafersDetails>();
						Dictionary<string, ModifyWafersDetails> newDict = new Dictionary<string, ModifyWafersDetails>();
						// int iSeqNo = 0;
						foreach (OM.LotWafers oWafer in (objResult.Value as LotModifyWafers).LotWafers)
						{
							ModifyWafersDetails row = new ModifyWafersDetails();
							// temporary storage for original sequence: field "OriginalNDPW" will not (can not) be submitted, and sequence is implied by itemList order on submit ...
							row.OriginalNDPW = oWafer.WaferSequence; // alt: row.OriginalNDPW = ++iSeqNo;
							// temporary storage for InstanceId: field "InventoryId" will not be submitted (but InstanceId will be used to identify items in itemList)
							row.InventoryId = oWafer.Self.ID.ToString();
							row.WaferScribeNumber = oWafer.WaferScribeNumber; // set to read-only in grid columns definition
							row.WaferNumber = oWafer.WaferNumber;
							row.RequireDataCollection = oWafer.RequireDataCollection;
							row.RequireTracking = oWafer.RequireTracking;
							row.NDPW = (int)oWafer.NDPW;
							row.GoodQty = (int)oWafer.GoodQty;
							row.WaferProduct = oWafer.WaferProduct;
							row.VendorLotNumber = oWafer.VendorLotNumber;
							row.VendorName = oWafer.VendorName;
							row.Grade = oWafer.Grade;
							row.ProductionGrade = oWafer.ProductionGrade;
							row.ChipGrade = oWafer.ChipGrade;
							row.WaferSize = oWafer.WaferSize;
							row.DieSize = oWafer.DieSize;
							row.WaferThickness = oWafer.WaferThickness;
							row.OxideThickness = oWafer.OxideThickness;
							row.BrokenPieces = oWafer.BrokenPieces;
							row.FilmId = oWafer.FilmId;
							row.Brightness = oWafer.Brightness;
							row.Color = oWafer.Color;
							row.Flux = oWafer.Flux;
							row.VF = oWafer.VF;
							row.SortIQCResult = oWafer.SortIQCResult;
							row.SortIQCComments = oWafer.SortIQCComments;
							row.SortEquipment = oWafer.SortEquipment;
							row.SortNoteCode = oWafer.SortNoteCode;
							row.ResortNoteCode = oWafer.ResortNoteCode;
							row.SortComments = oWafer.SortComments;
							row.SortSQAResult = oWafer.SortSQAResult;
							row.SortSQAComments = oWafer.SortSQAComments;
							row.TxnTimestamp = oWafer.TxnTimestamp;
							row.Username = oWafer.Username;
							row.WaferBatchId = oWafer.WaferBatchId;
							newRows.Add(row);
							newDict.Add(row.InventoryId.ToString(), (ModifyWafersDetails)row.Clone());
						}

						(_grdWaferDetails.GridContext as BoundContext).Data = newRows.ToArray();
						_grdWaferDetails.BoundContext.LoadData();
						_envOrigWafer.SS_Hashtable = new Hashtable(newDict);
						_txtSelectionId.TextControl.Text = lotName;
					}
				}
			}
			return oResStat;
		} // end InitWaferDetailsGrid

		/// <summary>
		/// transfer data from _SelValEx_ popup to wafer detail grid cell 
		/// </summary>
		private void SetWaferDetailsGridCell(string sRowID)
		{
			if (!string.IsNullOrEmpty(sRowID))
			{
				var oSelKey = Page.DataContract.GetValueByName("SelectedKeyDM");
				var oSelVal = Page.DataContract.GetValueByName("SelectedValDM");
				var oSelRev = Page.DataContract.GetValueByName("SelectedRevDM");
				if (oSelKey != null && oSelVal != null)
				{
					string sKey = oSelKey.ToString();
					string sColumn;
					switch (sKey)
					{
						case "Product":
							sColumn = "WaferProduct";
							break;
						case "MaterialPart":
							sColumn = "WaferProduct";
							break;
						case "Vendor":
							sColumn = "VendorName";
							break;
						case "WaferSortEquipment":
							sColumn = "SortEquipment";
							break;
						default:
							sColumn = sKey;
							break;
					}
					(_grdWaferDetails.GridContext as ItemDataContext).SetCell(sRowID, sColumn, oSelVal);
				}
				Page.DataContract.SetValueByName("SelectedKeyDM", null);
				Page.DataContract.SetValueByName("SelectedValDM", null);
				Page.DataContract.SetValueByName("SelectedRevDM", null);
			}
		} // end SetWaferDetailsGridCell

		/// <summary>
		/// setup one wafer details row of service data (LotModifyWafers) 
		/// </summary>
		private void SetLotModifyWafersRow(Service svc, ModifyWafersDetails oRow, ModifyWafersDetails[] aRows, int idx, Boolean bOnly)
		{
			if (0 <= idx && idx < aRows.Length) // always true in this context
			{
				// if (aRows[idx].InventoryId != null)
				Boolean bForce = (oRow == null || !bOnly);
				// always copy current wafer scribe number and wafer number
				(svc as LotModifyWafers).Wafers[idx].WaferScribeNumber = ChangedValue(aRows[idx].WaferScribeNumber);
				(svc as LotModifyWafers).Wafers[idx].WaferNumber = ChangedValue(aRows[idx].WaferNumber);
				if (oRow != null) // not a newly created row
				{
					// get InstanceID from InventoryId field for previously existing rows
					(svc as LotModifyWafers).Wafers[idx].LotWafersItem = new SubentityRef(oRow.InventoryId.ToString());
					// get new wafer scribe number if provided
					(svc as LotModifyWafers).Wafers[idx].NewWaferScribeNumber = ChangedValue(aRows[idx].NewWaferScribeNumber);
				}
				else // consider NewWaferScribeNumber as wafer scribe number
				{
					if ((svc as LotModifyWafers).Wafers[idx].WaferScribeNumber.IsNullOrEmpty())
						(svc as LotModifyWafers).Wafers[idx].WaferScribeNumber = ChangedValue(aRows[idx].NewWaferScribeNumber);
				}

				// copy other values only if there really was a change or updateOnly is false (inserting new rows)
				(svc as LotModifyWafers).Wafers[idx].RequireDataCollection = ChangedValue(aRows[idx].RequireDataCollection, bForce ? null : oRow.RequireDataCollection);
				(svc as LotModifyWafers).Wafers[idx].RequireTracking = ChangedValue(aRows[idx].RequireTracking, bForce ? null : oRow.RequireTracking);
				(svc as LotModifyWafers).Wafers[idx].NDPW = ChangedValue(aRows[idx].NDPW, bForce ? null : oRow.NDPW);
				(svc as LotModifyWafers).Wafers[idx].GoodQty = ChangedValue(aRows[idx].GoodQty, bForce ? null : oRow.GoodQty);
				(svc as LotModifyWafers).Wafers[idx].WaferProduct = ChangedValue(aRows[idx].WaferProduct, bForce ? null : oRow.WaferProduct);
				(svc as LotModifyWafers).Wafers[idx].VendorLotNumber = ChangedValue(aRows[idx].VendorLotNumber, bForce ? null : oRow.VendorLotNumber);
				(svc as LotModifyWafers).Wafers[idx].VendorName = ChangedValue(aRows[idx].VendorName, bForce ? null : oRow.VendorName);
				(svc as LotModifyWafers).Wafers[idx].Grade = ChangedValue(aRows[idx].Grade, bForce ? null : oRow.Grade);
				(svc as LotModifyWafers).Wafers[idx].ProductionGrade = ChangedValue(aRows[idx].ProductionGrade, bForce ? null : oRow.ProductionGrade);
				(svc as LotModifyWafers).Wafers[idx].ChipGrade = ChangedValue(aRows[idx].ChipGrade, bForce ? null : oRow.ChipGrade);
				(svc as LotModifyWafers).Wafers[idx].WaferSize = ChangedValue(aRows[idx].WaferSize, bForce ? null : oRow.WaferSize);
				(svc as LotModifyWafers).Wafers[idx].DieSize = ChangedValue(aRows[idx].DieSize, bForce ? null : oRow.DieSize);
				(svc as LotModifyWafers).Wafers[idx].WaferThickness = ChangedValue(aRows[idx].WaferThickness, bForce ? null : oRow.WaferThickness);
				(svc as LotModifyWafers).Wafers[idx].OxideThickness = ChangedValue(aRows[idx].OxideThickness, bForce ? null : oRow.OxideThickness);
				(svc as LotModifyWafers).Wafers[idx].BrokenPieces = ChangedValue(aRows[idx].BrokenPieces, bForce ? null : oRow.BrokenPieces);
				(svc as LotModifyWafers).Wafers[idx].FilmId = ChangedValue(aRows[idx].FilmId, bForce ? null : oRow.FilmId);
				(svc as LotModifyWafers).Wafers[idx].Brightness = ChangedValue(aRows[idx].Brightness, bForce ? null : oRow.Brightness);
				(svc as LotModifyWafers).Wafers[idx].Color = ChangedValue(aRows[idx].Color, bForce ? null : oRow.Color);
				(svc as LotModifyWafers).Wafers[idx].Flux = ChangedValue(aRows[idx].Flux, bForce ? null : oRow.Flux);
				(svc as LotModifyWafers).Wafers[idx].VF = ChangedValue(aRows[idx].VF, bForce ? null : oRow.VF);
				(svc as LotModifyWafers).Wafers[idx].SortIQCResult = ChangedValue(aRows[idx].SortIQCResult, bForce ? null : oRow.SortIQCResult);
				(svc as LotModifyWafers).Wafers[idx].SortIQCComments = ChangedValue(aRows[idx].SortIQCComments, bForce ? null : oRow.SortIQCComments);
				(svc as LotModifyWafers).Wafers[idx].SortEquipment = ChangedValue(aRows[idx].SortEquipment, bForce ? null : oRow.SortEquipment);
				(svc as LotModifyWafers).Wafers[idx].SortNoteCode = ChangedValue(aRows[idx].SortNoteCode, bForce ? null : oRow.SortNoteCode);
				(svc as LotModifyWafers).Wafers[idx].ResortNoteCode = ChangedValue(aRows[idx].ResortNoteCode, bForce ? null : oRow.ResortNoteCode);
				(svc as LotModifyWafers).Wafers[idx].SortComments = ChangedValue(aRows[idx].SortComments, bForce ? null : oRow.SortComments);
				(svc as LotModifyWafers).Wafers[idx].SortSQAResult = ChangedValue(aRows[idx].SortSQAResult, bForce ? null : oRow.SortSQAResult);
				(svc as LotModifyWafers).Wafers[idx].SortSQAComments = ChangedValue(aRows[idx].SortSQAComments, bForce ? null : oRow.SortSQAComments);
				(svc as LotModifyWafers).Wafers[idx].TxnTimestamp = ChangedValue(aRows[idx].TxnTimestamp, bForce ? null : oRow.TxnTimestamp);
				(svc as LotModifyWafers).Wafers[idx].Username = ChangedValue(aRows[idx].Username, bForce ? null : oRow.Username);
				(svc as LotModifyWafers).Wafers[idx].WaferBatchId = ChangedValue(aRows[idx].WaferBatchId, bForce ? null : oRow.WaferBatchId);
			}
		} // end SetLotModifyWafersRow

		#endregion // PrivateFunctions

		#region Handlers

		/// <summary>
		/// triggered when a lot name is entered in the selection ID field (params not used)
		/// fetches lot related data (Attributes, ...) based on lot name (SelectionId)
		/// </summary>
		/// <param name="sender" type="object"></param>
		/// <param name="e" type="EventArgs"></param>
		public void SelectionId_DataChanged(/*object sender, EventArgs e*/)
		{
			try
			{
				Page.StatusBar.ClearMessage();
				if (_txtSelectionId.Data != null)
				{
					OM.ResultStatus oResStat = InitWaferDetailsGrid();
					if (oResStat.IsSuccess)
					{
						_btnAutoGenerate.Enabled = true;
						CamstarWebControl.SetRenderToClient(_grdWaferDetails);
					}
					else
					{
						ResetFields(false);
						DisplayMessage(oResStat);
					}
				}
				else
				{
					ResetFields(false);
				}
			}
			catch (Exception Ex)
			{
				DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
			}
		} //end SelectionId_DataChanged

		/// <summary>
		/// triggered when a new value is set in the wafer grid -- [NH] currently not used
		/// </summary>
		/// <param name="sender" type="object"></param>
		/// <param name="e" type="EventArgs"></param>
		public void WaferGrid_DataChanged(object sender, EventArgs e)
		{
			string strSender = sender.ToString();
			string strEType = e.GetType().ToString();
		} //end WaferGrid_DataChanged

		/// <summary>
		/// triggered when a wafer grid row should move up in the array (params not used)
		/// </summary>
		/// <param name="sender" type="object"></param>
		/// <param name="e" type="EventArgs"></param>
		public void WaferGrid_RowMoveUp(/*object sender, EventArgs e*/)
		{
			if (_grdWaferDetails.GridContext.SelectedRowIDs != null
				&& _grdWaferDetails.GridContext.SelectedRowIDs.Count > 0)
			{
				ReassignRows(1);
			}
		} //end WaferGrid_RowMoveUp

		/// <summary>
		/// triggered when a wafer grid row should move down in the array (params not used)
		/// </summary>
		/// <param name="sender" type="object"></param>
		/// <param name="e" type="EventArgs"></param>
		public void WaferGrid_RowMoveDown(/*object sender, EventArgs e*/)
		{
			if (_grdWaferDetails.GridContext.SelectedRowIDs != null
				&& _grdWaferDetails.GridContext.SelectedRowIDs.Count > 0)
			{
				ReassignRows(-1);
			}
		} //end WaferGrid_RowMoveDown

		/// <summary>
		/// triggered when a row is added to wafer grid (params not used) -- [NH] currently not used
		/// </summary>
		/// <param name="sender" type="object"></param>
		/// <param name="e" type="EventArgs"></param>
		public void WaferGrid_RowAdded(/*object sender, EventArgs e*/)
		{
		} //end WaferGrid_RowAdded

		/// <summary>
		/// triggered when a row is deleted from wafer grid (params not used)
		/// </summary>
		/// <param name="sender" type="object"></param>
		/// <param name="e" type="EventArgs"></param>
		public void WaferGrid_RowDeleted(/*object sender, EventArgs e*/)
		{
			if (_grdWaferDetails.GridContext.SelectedRowIDs != null
				&& _grdWaferDetails.GridContext.SelectedRowIDs.Count > 0)
			{
				ReassignRows(0);
			}
		} //end WaferGrid_RowDeleted

		/// <summary>
		/// handler for AutoGenerate button click: creates new wafer list based on Lot Qty and Qty2
		/// </summary>
		/// <param name="sender" type="object"></param>
		/// <param name="e" type="EventArgs"></param>
		public void WaferGrid_AutoGenerate(/*object sender, EventArgs e*/)
		{
			if (_grdLotDetails != null)
			{
				ModifyWafersDetails[] waferRows = _grdWaferDetails.Data as ModifyWafersDetails[]; // current wafer details grid as array
				var newWaferRows = new List<ModifyWafersDetails>();
				string strQty = _grdLotDetails.GridContext.GetCell(0, "Qty").ToString();
				string strQty2 = _grdLotDetails.GridContext.GetCell(0, "Qty2").ToString();
				int iQty = string.IsNullOrEmpty(strQty) ? 0 : Convert.ToInt32(strQty);
				int iQty2 = string.IsNullOrEmpty(strQty2) ? 0 : Convert.ToInt32(strQty2);
				int iNDPW = 0;
				if (iQty2 > 0)
				{
					int gridNDPW = 0;
					string gridWaferScribeNumber = "";
					iNDPW = iQty / iQty2;
					for (int i = 1; i <= iQty2; i++)
					{
						if (i < iQty2)
						{
							gridNDPW = iNDPW; iQty -= iNDPW;
						}
						else
						{
							gridNDPW = iQty;
						}
						if (_txtSelectionId.Data != null)
							gridWaferScribeNumber = _txtSelectionId.Data.ToString() + "-" + string.Format("{0:00}", i);
						else
							gridWaferScribeNumber = string.Format("{0:00}", i);

						ModifyWafersDetails newRow = null;
						if (waferRows != null && 0 <= i - 1 && i - 1 < waferRows.Length)
							newRow = (ModifyWafersDetails)waferRows[i - 1].Clone();
						else
							newRow = new ModifyWafersDetails();
						newRow.NDPW = gridNDPW;
						newRow.GoodQty = 0;
						newRow.WaferNumber = string.Format("{0:00}", i);
						if (newRow.WaferScribeNumber.IsNullOrEmpty())
							newRow.WaferScribeNumber = gridWaferScribeNumber;
						else if (newRow.WaferScribeNumber != gridWaferScribeNumber)
							newRow.NewWaferScribeNumber = gridWaferScribeNumber;

                        newRow.RequireTracking = true;
                        newRow.RequireDataCollection = true;

						newWaferRows.Add(newRow);
					}
					_grdWaferDetails.ClearData();
					_grdWaferDetails.Data = newWaferRows.ToArray();
					_grdWaferDetails.OriginalData = newWaferRows.ToArray();
				}
			}
		} // end WaferGrid_AutoGenerate

		#endregion // Handlers

		#region Overrides

		/// <summary>
		// override of the Get Input Data method
		/// </summary>
		/// <param name="serviceData" type="Service"></param>
		public override void GetInputData(Service serviceData)
		{
			base.GetInputData(serviceData);

			//Wafers
			if (_grdWaferDetails.Data != null)  // _grdWaferDetails.BoundContext.Fields.Count
			{
				ModifyWafersDetails[] waferRows = _grdWaferDetails.Data as ModifyWafersDetails[]; // current wafer details grid as array
				Hashtable origRows = _envOrigWafer.SS_Hashtable; // original wafer details grid as hashtable
				int iMatchingRows = 0;
				Boolean bIsUpdateOnly = _chkUpdateOnly.IsChecked;
				if (origRows == null || origRows.Count != waferRows.Length) // don't allow update only if number of rows has changed
				{
					_chkUpdateOnly.Data = bIsUpdateOnly = false;
					(serviceData as LotModifyWafers).UpdateOnly = false;
				}

				if (serviceData is LotModifyWafers && waferRows.Length > 0) // still some rows in the grid
				{
					(serviceData as LotModifyWafers).Wafers = new ModifyWafersDetails[waferRows.Length];
					for (int i = 0; i < waferRows.Length; i++)
				    {
						(serviceData as LotModifyWafers).Wafers[i] = new ModifyWafersDetails();
						object oKey = (waferRows[i].InventoryId != null) ? waferRows[i].InventoryId.ToString() : null;
						if (origRows != null && oKey != null && origRows.ContainsKey(oKey)) // potentially modified original row
						{
							ModifyWafersDetails origRow = (ModifyWafersDetails)origRows[oKey];
							iMatchingRows += 1;
							SetLotModifyWafersRow(serviceData, origRow, waferRows, i, bIsUpdateOnly);
						}
						else if (!(waferRows[i].WaferScribeNumber.IsNullOrEmpty() && waferRows[i].NewWaferScribeNumber.IsNullOrEmpty())
								&& !waferRows[i].WaferNumber.IsNullOrEmpty()) // a valid new row with minimum data ...
						{
							SetLotModifyWafersRow(serviceData, null, waferRows, i, bIsUpdateOnly);
						}
				    }
				}
			}

			//Container
			if (_grdLotDetails.Data != null)
			{
                if (_grdLotDetails.BoundContext.Fields[0].ID != "_spacer")
                {
                    (serviceData as LotModifyWafers).Container = new ContainerRef();
                    (serviceData as LotModifyWafers).Container.Name = _grdLotDetails.GridContext.GetCell(0, "Lot").ToString();
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
						// called on ResetButton click (ID = 'ctl00_WebPartManager_ButtonsBar_ResetAction')
						Page.ClearValues();
						ResetFields();
						_txtSelectionId.Focus();
						break;
					}
					case cUp: // down-arrow default action -- [NH] not currently used
					{
						WaferGrid_RowMoveUp();
						break;
					}
					case cDown: // up-arrow default action -- [NH] not currently used
					{
						WaferGrid_RowMoveDown();
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
				// string grdCtx = _grdWaferDetails.GridContext.ContextID;
				if (!bIsPostBack)
				{
					Page.ClearValues();
					ResetFields(false);
					SetPageActionServiceName();
					_txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
					_txtSelectionId.Focus();

                    //get containers from Container Search screen
                    if (Page.Session["selectedContainers"] != null)
                    {
                        string[] sContainersList = Page.Session["selectedContainers"] as string[];
                        foreach (string container in sContainersList)
                        {
                            _txtSelectionId.TextControl.Text = container;
                            SelectionId_DataChanged();
                        }
                    }
				}
				else if (bIsPopupClose)
				{
					var oLotSel = Page.DataContract.GetValueByName("LotModifyWafers_ContainerDataEnvDM");
					var oWaferRowSel = Page.DataContract.GetValueByName("WaferGridRowDM");
					Boolean bLSPopupClosed = (oLotSel != null);
					Boolean bWDPopupClosed = (oWaferRowSel != null);
					if (bLSPopupClosed && _envContainer != null) // [NH] there's data in LotModifyWafers_ContainerDataEnvDM, so assume LotSelection popup closed
					{
						if (oLotSel != null) // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
							_envContainer.SS_ContainersList = oLotSel as string[];

						if (_envContainer.SS_ContainersList != null) // only true when lotselection popup closes
						{
							string sContainer = _envContainer.SS_ContainersList[0];
							_envContainer.SS_ContainersList = null;
							_txtSelectionId.ClearData();
							_txtSelectionId.Data = sContainer; // implicitly calls SelectionId_DataChanged()
						}
					}
					if (bWDPopupClosed)
					{
						SetWaferDetailsGridCell(oWaferRowSel.ToString());
						Page.DataContract.SetValueByName("WaferGridRowDM", null);
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
                DisplayMessage(status);
                ResetFields();
				_txtSelectionId.Focus();
			}
		} //end PostExecute

		#endregion // Overrides
	}
}




