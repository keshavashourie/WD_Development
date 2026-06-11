/* Copyright 2025 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Reflection;
using System.Reflection.Emit;
using System.ComponentModel;

using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using PERS = Camstar.WebPortal.Personalization;
using CamstarPortal.WebControls;
using Camstar.WCF.Services;
using System.Collections;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Constants;


namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
	public class SS_LotAssignExperiment : scsShopfloorBase
    {
        #region Properties

        CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("ComputerNameField") as CWC.TextBox; } }
        JQDataGrid _gridLotInfoField { get { return Page.FindCamstarControl("LotInfoFieldGrid") as JQDataGrid; } }
        JQDataGrid _gridDetailsField { get { return Page.FindCamstarControl("DetailsField") as JQDataGrid; } }
        CWC.TextBox _txtSelectionIdField { get { return Page.FindCamstarControl("SelectionIdField") as CWC.TextBox; } }
        CWC.NamedObject _txtEmployeeField { get { return Page.FindCamstarControl("EmployeeField") as CWC.NamedObject; } }
        CWC.ContainerList _clHiddenSelectedContainer { get { return Page.FindCamstarControl("HiddenSelectedContainer") as CWC.ContainerList; } }
        SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }
		CWC.Button _addButton { get { return Page.FindCamstarControl("AddButton") as CWC.Button; } }
		CWC.Button _addAllButton { get { return Page.FindCamstarControl("AddAllButton") as CWC.Button; } }
		CWC.Button _removeButton { get { return Page.FindCamstarControl("RemoveButton") as CWC.Button; } }
		CWC.Button _removeAllButton { get { return Page.FindCamstarControl("RemoveAllButton") as CWC.Button; } }
		JQDataGrid _gridWafersField { get { return Page.FindCamstarControl("WafersFieldGrid") as JQDataGrid; } }
		JQDataGrid _gridSlotMapsDetailsField { get { return Page.FindCamstarControl("SlotMapDetailsGrid") as JQDataGrid; } }
		CWC.DropDownList _ddlSlotNumberField { get { return Page.FindCamstarControl("SlotNumber") as CWC.DropDownList; } }
		CWC.NamedObject _ndoWaferCarrierFamilyField { get { return Page.FindCamstarControl("LotAssignExperiment_ss_WaferCarrierFamily") as CWC.NamedObject; } }
		CWC.CheckBox _chkboxIsCarrierAssignedSlotMap { get { return Page.FindCamstarControl("IsCarrierAssignedSlotMap") as CWC.CheckBox; } }
		const string const_sSelectionId = "SELECTIONID";
		const string const_sCarrier = "CARRIER";
		#endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
			_ndoWaferCarrierFamilyField.AutoPostBack = true;
			_ndoWaferCarrierFamilyField.DataChanged += _ndoWaferCarrierFamilyField_DataChanged;
			_addButton.Click += new EventHandler(AddButton_Click);
			_addAllButton.Click += new EventHandler(AddAllButton_Click);
			_removeButton.Click += new EventHandler(RemoveButton_Click);
			_removeAllButton.Click += new EventHandler(RemoveAllButton_Click);
            if (!Page.IsPostBack)
            {
                // Get Computer name
                _txtComputerNameField.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
            }
            else
            {
                //if the page has been submitted and cleared, get the computer name again.
                if (_txtComputerNameField.Data != null)
                    _txtComputerNameField.Data = SEMI.AppCode.UIUtility.GetComputerName(this);

                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {
                    if (_envSelectedLots != null)
                        // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                        if (Page.DataContract.GetValueByName("SelectedLotsListDM") != null)
                            _envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("SelectedLotsListDM") as string[];

                    if (_envSelectedLots.SS_ContainersList != null)
                    {
                        string[] sContainers;
                        sContainers = _envSelectedLots.SS_ContainersList;

                        // Set Selection Id textbox value
                        _txtSelectionIdField.Data = sContainers[0];

                        //nullify the containers list
                        _envSelectedLots.SS_ContainersList = null;
                    }
                }
            }
        }

		void _ndoWaferCarrierFamilyField_DataChanged(object sender, EventArgs e)
		{
			try
			{
				if (!_ndoWaferCarrierFamilyField.IsEmpty)
				{
					if (_chkboxIsCarrierAssignedSlotMap.IsChecked == true)
					{
						_ddlSlotNumberField.ClearData();
						throw new Exception("Lot already has an assigned Carrier with associated Slot Map. Virtual slot map assignmnet is not allowed.");
					}
					else
					{
						_gridSlotMapsDetailsField.ClearData();
						_ddlSlotNumberField.ClearData();
						_ddlSlotNumberField.PickListPanelControl.ClearSelectionValues();
						FetchData(const_sCarrier);
					}
				}
			}
			catch (Exception ex)
			{
				DisplayMessage(new OM.ResultStatus(ex.Message, false));
			}
		}

        public void SelectionIdField_DataChanged(object sender, EventArgs e)
        {
            if (!_txtSelectionIdField.IsEmpty)
            {
                FetchData(const_sSelectionId);
            }
            else
            {
                _txtSelectionIdField.TextControl.Text = "";
                ResetFields();
            }
        }

		private void FetchData(string sEventName)
		{
			try
			{
				string sSelectionId = "";
				var fs = FrameworkManagerUtil.GetFrameworkSession();
				LotAssignExperiment oServiceData = new LotAssignExperiment();
				LotAssignExperiment_Info oServiceInfo = new LotAssignExperiment_Info();
				LotAssignExperimentService oService = new LotAssignExperimentService(fs.CurrentUserProfile);
				LotAssignExperiment_Request oRequest = new LotAssignExperiment_Request();
				LotAssignExperiment_Result oResponseData = new LotAssignExperiment_Result();

				// Prepare the request
				if (sEventName == const_sSelectionId)
				{
					sSelectionId = _txtSelectionIdField.Data.ToString();
					oServiceData.SelectionId = sSelectionId;
					oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
					oServiceInfo.CurrentDetails = new LotAssignExpDetails_Info();
					oServiceInfo.CurrentDetails.ExperimentPlan = FieldInfoUtil.RequestValue();
					oServiceInfo.CurrentDetails.IsActivated = FieldInfoUtil.RequestValue();
					oServiceInfo.CurrentDetails.AssignByUser = FieldInfoUtil.RequestValue();
					oServiceInfo.CurrentDetails.ActivatedByUser = FieldInfoUtil.RequestValue();
					oServiceInfo.CurrentDetails.LotExpPlansInstance = FieldInfoUtil.RequestValue();
					oServiceInfo.LotWafers = new LotWafers_Info();
					oServiceInfo.LotWafers.WaferSequence = FieldInfoUtil.RequestValue();
					oServiceInfo.LotWafers.WaferNumber = FieldInfoUtil.RequestValue();
					oServiceInfo.LotWafers.WaferScribeNumber = FieldInfoUtil.RequestValue();
					oServiceInfo.LotWafers.Container = FieldInfoUtil.RequestValue();
					oServiceInfo.ss_LotVirtualSlotMap = new SlotMapDetails_Info() { RequestValue=true };
					oServiceInfo.ss_IsCarrierAssignedSlotMap = FieldInfoUtil.RequestValue();
				}
				else if (sEventName == const_sCarrier)
				{
					oServiceData.ss_WaferCarrierFamily = _ndoWaferCarrierFamilyField.Data as NamedObjectRef;
					oServiceInfo.ss_VirtualSlotMapSelection = new SlotMap_Info() { RequestValue=true};
				}

				// Request the data
				oRequest.Info = oServiceInfo;
				OM.ResultStatus oResultStatus = oService.ResolveSelectionId(oServiceData, oRequest, out oResponseData);
				if (oResultStatus.IsSuccess)
				{
					// Clear all fields for fresh transaction
					//ResetFields();

					if (sEventName == const_sSelectionId)
					{
                        // Clear fields for fresh transaction
                        ResetFields();

						// Set the value of the resolve Container Name
						_clHiddenSelectedContainer.Data = oResponseData.Value.SelectionContainer.Name.ToString();

						if (oResponseData.Value.ss_IsCarrierAssignedSlotMap != null)
							_chkboxIsCarrierAssignedSlotMap.CheckControl.Checked = oResponseData.Value.ss_IsCarrierAssignedSlotMap.Value;

						if (oResponseData.Value.CurrentDetails != null)
						{
							LotAssignExpDetails[] objDetails = new LotAssignExpDetails[oResponseData.Value.CurrentDetails.Length];
							int iCount = 0;
							foreach (LotAssignExpDetails resultDetail in oResponseData.Value.CurrentDetails)
							{
								objDetails[iCount] = new LotAssignExpDetails();
								objDetails[iCount].ExperimentPlan = resultDetail.ExperimentPlan;
								objDetails[iCount].IsActivated = resultDetail.IsActivated;
								objDetails[iCount].AssignByUser = resultDetail.AssignByUser;
								objDetails[iCount].ActivatedByUser = resultDetail.ActivatedByUser;
								objDetails[iCount].LotExpPlansInstance = resultDetail.LotExpPlansInstance;

								iCount++;
							}

							// Bind response data to datagrid
							(_gridDetailsField.GridContext as BoundContext).Data = objDetails.ToArray();
							_gridDetailsField.BoundContext.LoadData();
							CamstarWebControl.SetRenderToClient(_gridDetailsField);
						}

						// Fetch Lot Information and Display on datagrid
						SetLotSelection(_clHiddenSelectedContainer.Data.ToString());

						_txtSelectionIdField.Data = sSelectionId;
						// Fetch Lot Information and Display on datagrid
						SetLotSelection(oResponseData.Value.SelectionContainer.Name.ToString());

						if (oResponseData.Value.ss_LotVirtualSlotMap != null)
						{
							_gridSlotMapsDetailsField.Data = oResponseData.Value.ss_LotVirtualSlotMap;
							ArrayList slotNo = new ArrayList();

							foreach (SlotMapDetails slotmapList in oResponseData.Value.ss_LotVirtualSlotMap)
							{
								slotNo.Add(Convert.ToString(slotmapList.SlotNumber));
							}

							AddData(slotNo);
							foreach (LotWafers wafersList in oResponseData.Value.LotWafers)
							{
								string waferGrade = "";
								waferGrade = "Add";
								WafersGrid_AddNewRow((int)wafersList.WaferSequence, (string)wafersList.WaferNumber, (string)wafersList.WaferScribeNumber, wafersList.Container, waferGrade);
							}
						}

						if (oResponseData.Value.LotWafers != null)
						{
							foreach (LotWafers wafersList in oResponseData.Value.LotWafers)
							{
								string waferGrade = "";
								//int iNewRowCount = _gridWafersField.BoundContext.GetTotalRows();
								//(_gridWafersField.GridContext as ItemDataContext).MakeAutoRowId(iNewRowCount);
								//string id = (_gridWafersField.GridContext as ItemDataContext).AddNewRow(iNewRowCount.ToString());
								//object waferDetails = ((_gridWafersField.GridContext as ItemDataContext).Data as Array).GetValue(iNewRowCount);
								//(waferDetails as LotWafers).WaferSequence = wafersList.WaferSequence;
								//(waferDetails as LotWafers).WaferNumber = wafersList.WaferNumber;
								//(waferDetails as LotWafers).WaferScribeNumber = wafersList.WaferScribeNumber;
								//(waferDetails as LotWafers).Container = wafersList.Container;
								//if (oResponseData.Value.AssignedCarrier != null)
								//{
								//	//(waferDetails as LotWafers).Grade = "Add";
								//	waferGrade = "Add";
								//}
								WafersGrid_AddNewRow((int)wafersList.WaferSequence, (string)wafersList.WaferNumber, (string)wafersList.WaferScribeNumber, wafersList.Container, waferGrade);

								//_gridWafersField.GridContext.AdjustCurrentPage(id);
								//CamstarWebControl.SetRenderToClient(_gridWafersField);
							}
						}
					}
					else if (sEventName == const_sCarrier)
					{
						if (oResponseData != null)
						{
							ArrayList slotNo = new ArrayList();

							foreach (SlotMap slotmapList in oResponseData.Value.ss_VirtualSlotMapSelection)
							{
								//int iNewRowCount = _gridSlotMapsDetailsField.BoundContext.GetTotalRows();
								//(_gridSlotMapsDetailsField.GridContext as ItemDataContext).MakeAutoRowId(iNewRowCount);
								//string id = (_gridSlotMapsDetailsField.GridContext as ItemDataContext).AddNewRow(iNewRowCount.ToString());
								//object slotmapDetails = ((_gridSlotMapsDetailsField.GridContext as ItemDataContext).Data as Array).GetValue(iNewRowCount);
								//(slotmapDetails as SlotMapDetails).SlotNumber = slotmapList.SlotNumber;
								//(slotmapDetails as SlotMapDetails).WaferNumber = slotmapList.WaferNumber;
								//(slotmapDetails as SlotMapDetails).WaferScribeNumber = slotmapList.WaferScribeNumber;
								//(slotmapDetails as SlotMapDetails).Lot = slotmapList.Lot;
								//(slotmapDetails as SlotMapDetails).Status = slotmapList.Status;

								SlotMapGrid_AddNewRow((string)slotmapList.SlotNumber, (string)slotmapList.WaferNumber, (string)slotmapList.WaferScribeNumber, slotmapList.Lot, (string)slotmapList.Status);

								slotNo.Add(Convert.ToString(slotmapList.SlotNumber));
								//_gridSlotMapsDetailsField.GridContext.AdjustCurrentPage(id);
								//CamstarWebControl.SetRenderToClient(_gridSlotMapsDetailsField);
							}

							AddData(slotNo);

						}

					}
				}

			}
			catch (Exception Ex)
			{
				DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
			}
		}

		public void SlotMapGrid_AddNewRow(string SlotNumber, string WaferNumber, string WaferScribeNumber, ContainerRef Lot, string Status)
		{
			try
			{
				JQDataGrid _gridDetails = _gridSlotMapsDetailsField;
				SlotMapDetails[] oNewSlotMap = new SlotMapDetails[1];
				oNewSlotMap[0] = new SlotMapDetails();
				oNewSlotMap[0].SlotNumber = SlotNumber;
				oNewSlotMap[0].WaferNumber = WaferNumber;
				oNewSlotMap[0].WaferScribeNumber = WaferScribeNumber;
				oNewSlotMap[0].Lot = Lot;
				oNewSlotMap[0].Status = Status;
				SlotMapDetails[] oExisting = (_gridDetails.GridContext as BoundContext).Data as SlotMapDetails[];
				if (oExisting != null)
				{
					bool isUnique = true;
					for (int i = 0; i < oExisting.Length; i++)
					{
						if (oExisting[i].SlotNumber.Equals(oNewSlotMap[0].SlotNumber))
						{
							isUnique = false;
						}
					}
					if (isUnique)
					{
						SlotMapDetails[] oMerged = new SlotMapDetails[oExisting.Length + 1];
						Array.Copy(oExisting, oMerged, oExisting.Length);
						Array.Copy(oNewSlotMap, 0, oMerged, oExisting.Length, 1);
						(_gridDetails.GridContext as BoundContext).Data = oMerged.ToArray();
					}
				}
				else
				{
					(_gridDetails.GridContext as BoundContext).Data = oNewSlotMap.ToArray();
				}
				_gridDetails.BoundContext.LoadData();
				CamstarWebControl.SetRenderToClient(_gridDetails);
			}
			catch (Exception ex)
			{ }
		}

       public void AddData(ArrayList data)
        {
            string[] slotNo = data.ToArray(typeof(string)) as string[];
            RecordSet rsNamedObject = new RecordSet();
            OM.Header[] rsHeaders = new OM.Header[2];
            Row[] rsRows = new Row[slotNo.Length];

            rsHeaders[0] = new OM.Header();
            rsHeaders[0].TypeCode = TypeCode.String;
            rsHeaders[0].Name = "Name";

            rsHeaders[1] = new OM.Header();
            rsHeaders[1].TypeCode = TypeCode.String;
            rsHeaders[1].Name = "Value";

            rsNamedObject.Headers = rsHeaders;

            for (int x = 0; x < slotNo.Length; x++)
            {
                rsRows[x] = new Row();
                string[] strRowValues = new string[2];
                strRowValues[0] = slotNo[x];
                strRowValues[1] = slotNo[x];
                rsRows[x].Values = strRowValues;
            }

            rsNamedObject.Rows = rsRows;

            _ddlSlotNumberField.PickListPanelControl.SetSelectionValues(rsNamedObject);
        }

        public void SetLotSelection(string sSelectionId)
        {
            JQDataGrid _gridLotInfoFieldx = Page.FindCamstarControl("LotInfoFieldGrid") as JQDataGrid;
            _gridLotInfoFieldx.ClearData();
            SEMI.AppCode.UIUtility.GetLotQuerySelection(this, this.PrimaryServiceType.ToString(), sSelectionId, true, ref _gridLotInfoFieldx, "LotInfoFieldGrid");
        }

        public override void GetInputData(OM.Service serviceData)
        {
            try
            {
                base.GetInputData(serviceData);

                // Manually build the transaction by populating all the data
                ((OM.LotAssignExperiment)serviceData).ComputerName = _txtComputerNameField.TextControl.Text;
                ((OM.LotAssignExperiment)serviceData).Employee = (OM.NamedObjectRef)_txtEmployeeField.Data;

                if (_gridDetailsField.GridContext.GetTotalRows() > 0)
                {
                    OM.LotAssignExpDetails[] details = new LotAssignExpDetails[_gridDetailsField.GridContext.GetTotalRows()];
                    int i = 0;
                    foreach (OM.LotAssignExpDetails detail in ((OM.LotAssignExpDetails[])_gridDetailsField.Data))
                    {
                        OM.LotAssignExpDetails newDetail = new LotAssignExpDetails();
						newDetail.ListItemAction = ListItemAction.Add;
                        newDetail.ExperimentPlan = detail.ExperimentPlan;
                        newDetail.IsActivated = detail.IsActivated;
                        //newDetail.AssignByUser = detail.AssignByUser;
                        //newDetail.ActivatedByUser = detail.ActivatedByUser;
                        newDetail.LotExpPlansInstance = detail.LotExpPlansInstance;
                        details.SetValue(newDetail, i);
                        i++;
                    }
                    ((OM.LotAssignExperiment)serviceData).Details = details;
                }

				int o=0;
				if (_gridSlotMapsDetailsField.GridContext.GetTotalRows() > 0)
				{
					OM.SlotMapDetails[] slotmaps = new SlotMapDetails[_gridSlotMapsDetailsField.GridContext.GetTotalRows()];
					int i = 0;
					foreach (SlotMapDetails slotmap in ((SlotMapDetails[])_gridSlotMapsDetailsField.Data))
					{
						SlotMapDetails newSlotMap = new SlotMapDetails();
						newSlotMap.Lot = slotmap.Lot;
						newSlotMap.SlotNumber = slotmap.SlotNumber;
						newSlotMap.Status = slotmap.Status;
						newSlotMap.WaferNumber = slotmap.WaferNumber;
						newSlotMap.WaferScribeNumber = slotmap.WaferScribeNumber;
						slotmaps.SetValue(newSlotMap,o);
						o++;
					}
					((OM.LotAssignExperiment)serviceData).ss_VirtualSlotMap = slotmaps;
				}

                //Container
                if (!string.IsNullOrWhiteSpace(_txtSelectionIdField.Text))
                {
                    (serviceData as LotAssignExperiment).Container = new ContainerRef();
                    (serviceData as LotAssignExperiment).Container.Name = _txtSelectionIdField.TextControl.Text;
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }


        private void ResetFields()
        {
            _gridLotInfoField.ClearData();
			_gridDetailsField.ClearData();
			_gridSlotMapsDetailsField.ClearData();
			_gridWafersField.ClearData();
            _ndoWaferCarrierFamilyField.ClearData();
            _ddlSlotNumberField.ClearData();
            Page.StatusBar.ClearMessage();
        }

        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                Page.ClearValues();
                ResetFields();
                //_txtSelectionIdField.Focus();
            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            if (action != null && action.Parameters == "Reset")
            {
                Page.ClearValues();
                _gridLotInfoField.ClearData();
                _gridDetailsField.ClearData();
            }
        }

		public void AddButton_Click(object sender, EventArgs e)
		{
			try
			{

				if (_gridSlotMapsDetailsField.GridContext.GetTotalRows() != 0 && _gridWafersField.GridContext.GetTotalRows() != 0)
				{
					if (_ddlSlotNumberField.Data != null)
					{
						if (!string.IsNullOrEmpty(_gridWafersField.GridContext.SelectedRowID))
						{
							string selectedRowId = _gridWafersField.GridContext.SelectedRowID;
							LotWafers selectedRow = (_gridWafersField.GridContext as ItemDataContext).GetItem(selectedRowId) as LotWafers;
							if (selectedRow != null)
							{
								for (int i = 0; i < _gridSlotMapsDetailsField.GridContext.GetTotalRows(); i++)
								{
									string strSlotMapsDetailsRowID = i.ToString().PadLeft(6, '0');
									if (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapsDetailsRowID, "SlotNumber").ToString() == _ddlSlotNumberField.Data.ToString())
									{
										if (selectedRow.Grade != "Add")
										{
											if (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapsDetailsRowID, "Status").ToString() == "UP" && (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapsDetailsRowID, "WaferNumber") == null || _gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapsDetailsRowID, "WaferNumber").ToString() == ""))
											{
												_gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapsDetailsRowID, "WaferNumber", selectedRow.WaferNumber.Value);
												_gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapsDetailsRowID, "WaferScribeNumber", selectedRow.WaferScribeNumber.Value);
												_gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapsDetailsRowID, "Lot", selectedRow.Container.Name);
												_gridWafersField.GridContext.SetCell(selectedRowId, "Grade", "Add");
												CamstarWebControl.SetRenderToClient(_gridWafersField);
												CamstarWebControl.SetRenderToClient(_gridSlotMapsDetailsField);
											}
											else
											{
												throw new Exception("The slot number is either occupied or the status is down.");
											}
										}
										else
										{
											throw new Exception("The wafer is already added before.");
										}
									}
								}
							}
						}

					}
				}
			}
			catch (Exception ex)
			{
				DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
			}
		}

		public void AddAllButton_Click(object sender, EventArgs e)
		{
			try
			{
				List<String> previousAddedRow = new List<string>();
				bool rowAdded = false;

				if (_gridSlotMapsDetailsField.GridContext.GetTotalRows() != 0 && _gridWafersField.GridContext.GetTotalRows() != 0)
				{

					for (int j = 0; j < _gridWafersField.GridContext.GetTotalRows(); j++)
					{
						string strGridWaferRowID = j.ToString().PadLeft(6, '0');
						rowAdded = false;
						for (int i = 0; i < _gridSlotMapsDetailsField.GridContext.GetTotalRows(); i++)
						{
							// string strSlotMapDetailsRowID = _gridSlotMapsDetailsField.GridContext.GetRowId(i);
							string strSlotMapDetailsRowID = i.ToString().PadLeft(6, '0');
							if (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapDetailsRowID, "Status").ToString() == "UP" && (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapDetailsRowID, "WaferNumber") == null || _gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapDetailsRowID, "WaferNumber").ToString() == ""))
							{
								if (_gridWafersField.GridContext.GetCell(strGridWaferRowID, "Grade").ToString() != "Add")
								{
									if (!previousAddedRow.Contains(strSlotMapDetailsRowID))
									{
										string inputLot = _gridWafersField.GridContext.GetCell(strGridWaferRowID, "Container").ToString().Replace("(LOT)", "");
										_gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapDetailsRowID, "WaferNumber", _gridWafersField.GridContext.GetCell(strGridWaferRowID, "WaferNumber").ToString());
										_gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapDetailsRowID, "WaferScribeNumber", _gridWafersField.GridContext.GetCell(strGridWaferRowID, "WaferScribeNumber").ToString());
										_gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapDetailsRowID, "Lot", inputLot);
										_gridWafersField.GridContext.SetCell(strGridWaferRowID, "Grade", "Add");
										CamstarWebControl.SetRenderToClient(_gridWafersField);
										CamstarWebControl.SetRenderToClient(_gridSlotMapsDetailsField);
										previousAddedRow.Add(strSlotMapDetailsRowID);
										rowAdded = true;
										break;
									}
								}
								else
								{
									rowAdded = true;
								}
							}
						}
					}

					//if (!rowAdded)
					//{
					//	throw new Exception("The slot map has not enough slot for all wafers, either none or partial wafers are assigned.");
					//}
				}
			}
			catch (Exception ex)
			{
				DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
			}
		}

		public void RemoveButton_Click(object sender, EventArgs e)
		{
			if (_gridSlotMapsDetailsField.GridContext.GetTotalRows() != 0 && _gridWafersField.GridContext.GetTotalRows() != 0)
			{
				if (_ddlSlotNumberField.Data != null)
				{
					for (int i = 0; i < _gridSlotMapsDetailsField.GridContext.GetTotalRows(); i++)
					{
						string strSlotMapsDetailsRowID = i.ToString().PadLeft(6, '0');
						if (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapsDetailsRowID, "SlotNumber").ToString() == _ddlSlotNumberField.Data.ToString())
						{
							// string strSlotMapDetailsRowID = _gridSlotMapsDetailsField.GridContext.GetRowId(i);
							for (int j = 0; j < _gridWafersField.GridContext.GetTotalRows(); j++)
							{
								string strGridWaferRowID = j.ToString().PadLeft(6, '0');
								if (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapsDetailsRowID, "WaferScribeNumber") != null && (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapsDetailsRowID, "WaferScribeNumber").ToString() == _gridWafersField.GridContext.GetCell(strGridWaferRowID, "WaferScribeNumber").ToString()))
								{

									_gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapsDetailsRowID, "WaferNumber", "");
									_gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapsDetailsRowID, "WaferScribeNumber", "");
									_gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapsDetailsRowID, "Lot", "");
									_gridWafersField.GridContext.SetCell(strGridWaferRowID, "Grade", "");
									CamstarWebControl.SetRenderToClient(_gridWafersField);
									CamstarWebControl.SetRenderToClient(_gridSlotMapsDetailsField);
									break;
								}
							}
						}
					}

				}
			}
		}

		public void WafersGrid_AddNewRow(int WaferSequence, string WaferNumber, string WaferScribeNumber, ContainerRef Container, string Grade)
		{
			try
			{
				JQDataGrid _gridDetails = _gridWafersField;
				LotWafers[] oNewDetail = new LotWafers[1];
				oNewDetail[0] = new LotWafers();
				oNewDetail[0].WaferSequence = WaferSequence;
				oNewDetail[0].WaferNumber = WaferNumber;
				oNewDetail[0].WaferScribeNumber = WaferScribeNumber;
				oNewDetail[0].Container = Container;
				oNewDetail[0].Grade = Grade;
				LotWafers[] oExisting = (_gridDetails.GridContext as BoundContext).Data as LotWafers[];
				if (oExisting != null)
				{
					bool isUnique = true;
					for (int i = 0; i < oExisting.Length; i++)
					{
						if (oExisting[i].WaferSequence.Equals(oNewDetail[0].WaferSequence))
						{
							isUnique = false;
						}
					}
					if (isUnique)
					{
						LotWafers[] oMerged = new LotWafers[oExisting.Length + 1];
						Array.Copy(oExisting, oMerged, oExisting.Length);
						Array.Copy(oNewDetail, 0, oMerged, oExisting.Length, 1);
						(_gridDetails.GridContext as BoundContext).Data = oMerged.ToArray();
					}
				}
				else
				{
					(_gridDetails.GridContext as BoundContext).Data = oNewDetail.ToArray();
				}
				_gridDetails.BoundContext.LoadData();
				CamstarWebControl.SetRenderToClient(_gridDetails);
			}
			catch (Exception ex)
			{ }
		}


		public void RemoveAllButton_Click(object sender, EventArgs e)
		{
			if (_gridSlotMapsDetailsField.GridContext.GetTotalRows() != 0 && _gridWafersField.GridContext.GetTotalRows() != 0)
			{

				for (int i = 0; i < _gridSlotMapsDetailsField.GridContext.GetTotalRows(); i++)
				{

					string strSlotMapDetailsRowID = i.ToString().PadLeft(6, '0');
					for (int j = 0; j < _gridWafersField.GridContext.GetTotalRows(); j++)
					{
						string strGridWaferRowID = j.ToString().PadLeft(6, '0');
						if (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapDetailsRowID, "WaferScribeNumber") != null && (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapDetailsRowID, "WaferScribeNumber").ToString() == _gridWafersField.GridContext.GetCell(strGridWaferRowID, "WaferScribeNumber").ToString()))
						{
							_gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapDetailsRowID, "WaferNumber", "");
							_gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapDetailsRowID, "WaferScribeNumber", "");
							_gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapDetailsRowID, "Lot", "");
							_gridWafersField.GridContext.SetCell(strGridWaferRowID, "Grade", "");
							CamstarWebControl.SetRenderToClient(_gridWafersField);
							CamstarWebControl.SetRenderToClient(_gridSlotMapsDetailsField);
							break;
						}
					}
				}

			}
		}

    }
}



