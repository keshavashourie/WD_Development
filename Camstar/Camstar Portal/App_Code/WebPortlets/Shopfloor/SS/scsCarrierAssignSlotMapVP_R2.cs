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
using Camstar.WebPortal.Personalization;
using CamstarPortal.WebControls;
using Camstar.WCF.Services;
using System.Collections;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Constants;
using System.Text.RegularExpressions;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    /// Summary description for CarrierAssignSlotMapTxn
    /// </summary>
    public class CarrierAssignSlotMapTxn_R2 : scsShopfloorBase
    {
        CWC.TextBox _txtSelectionIdField { get { return Page.FindCamstarControl("SelectionId") as CWC.TextBox; } }
        CWC.NamedObject _ndoCarrierField { get { return Page.FindCamstarControl("CarrierAssignSlotMap_ToCarrier") as CWC.NamedObject; } }
        JQDataGrid _gridLotInfoField { get { return Page.FindCamstarControl("LotInfoFieldGrid") as JQDataGrid; } }
        JQDataGrid _gridWafersFieldhdn { get { return Page.FindCamstarControl("WafersFieldGrid") as JQDataGrid; } }
        JQDataGrid _gridWafersField { get { return Page.FindCamstarControl("WafersFieldGrid2") as JQDataGrid; } }
        JQDataGrid _gridSlotMapsDetailsField { get { return Page.FindCamstarControl("SlotMapDetailsGrid") as JQDataGrid; } }
        CWC.ContainerList _ContainerField { get { return Page.FindCamstarControl("HiddenSelectedContainer") as CWC.ContainerList; } }
        CWC.DropDownList _ddlSlotNumberField { get { return Page.FindCamstarControl("SlotNumber") as CWC.DropDownList; } }
        SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }
        CWC.Button _addButton { get { return Page.FindCamstarControl("AddButton") as CWC.Button; } }
        CWC.Button _addAllButton { get { return Page.FindCamstarControl("AddAllButton") as CWC.Button; } }
        CWC.Button _removeButton { get { return Page.FindCamstarControl("RemoveButton") as CWC.Button; } }
        CWC.Button _removeAllButton { get { return Page.FindCamstarControl("RemoveAllButton") as CWC.Button; } }
        CWC.Button _cmdbarAddButton { get { return Page.FindCamstarControl("Add") as CWC.Button; } }
        CWC.Button _cmdbarAddAllButton { get { return Page.FindCamstarControl("AddAll") as CWC.Button; } }
        CWC.Button _cmdbarRemoveButton { get { return Page.FindCamstarControl("Remove") as CWC.Button; } }
        CWC.Button _cmdbarRemoveAllButton { get { return Page.FindCamstarControl("RemoveAll") as CWC.Button; } }
        CWC.Button _cmdbarAssignSlotsButton { get { return Page.FindCamstarControl("AssignSlots") as CWC.Button; } }
        CWC.NamedObject _waferHandlerEquipment { get { return Page.FindCamstarControl("CarrierAssignSlotMap_scsWaferHandlerEquipment") as CWC.NamedObject; } }
        CWC.RevisionedObject _rdoRecipeField { get { return Page.FindCamstarControl("CarrierAssignSlotMap_Recipe") as CWC.RevisionedObject; } }
        CWC.DropDownList _SlotAssignmentMethodField { get { return Page.FindCamstarControl("CarrierAssignSlotMap_scsSlotAssignmentMethodEnum") as CWC.DropDownList; } }
        CWC.CheckBox _chkboxIsLotAssignedSlotMap { get { return Page.FindCamstarControl("IsLotAssignedSlotMap") as CWC.CheckBox; } }

        const string const_sSelectionId = "SELECTIONID";
        const string const_sCarrier = "CARRIER";
        private scsWaferSlotMappingService serviceAssignment;
        private string SlotRowID = String.Empty, inputLot = String.Empty;

        public CarrierAssignSlotMapTxn_R2()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_gridWafersFieldhdn != null)
                _gridWafersFieldhdn.Visible = false;

            _cmdbarAddButton.Click += new EventHandler(AddButton_Click);
            _cmdbarAddAllButton.Click += new EventHandler(AddAllButton_Click);
            _cmdbarRemoveButton.Click += new EventHandler(RemoveButton_Click);
            _cmdbarRemoveAllButton.Click += new EventHandler(RemoveAllButton_Click);
            _cmdbarAssignSlotsButton.Click += new EventHandler(AssignSlotsButton_Click); //Include Assign Slots Service from US101306

            ToCarrierBeforeDataLoad();

            if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                OnPopupClose();

            var lotInfoFieldGrid = (Page.FindCamstarControl("LotInfoFieldGrid")) as JQDataGrid;
            var wafersFieldGrid = (Page.FindCamstarControl("WafersFieldGrid")) as JQDataGrid;
            var slotMapDetailsGrid = (Page.FindCamstarControl("SlotMapDetailsGrid")) as JQDataGrid;

            if (IsResponsive)
            {
                if (lotInfoFieldGrid.Settings.Automation == null)
                    lotInfoFieldGrid.Settings.Automation = new GridAutomation();

                lotInfoFieldGrid.Settings.Automation.ShrinkColumnWidthToFit = false;

                if (wafersFieldGrid.Settings.Automation == null)
                    wafersFieldGrid.Settings.Automation = new GridAutomation();

                wafersFieldGrid.Settings.Automation.ShrinkColumnWidthToFit = false;

                if (slotMapDetailsGrid.Settings.Automation == null)
                    slotMapDetailsGrid.Settings.Automation = new GridAutomation();

                slotMapDetailsGrid.Settings.Automation.ShrinkColumnWidthToFit = false;
            }

            if (_waferHandlerEquipment.Data != null)
            {
                _rdoRecipeField.Visible = true;
            }
            else
            {
                _rdoRecipeField.Visible = false;
            }

            SlotAssignmentMethod();
        }

        private void SingleSelectWafer()
        {
            //_gridWafersField.GridContext.SelectedRowIDs[0] = _gridWafersField.GridContext.SelectedRowID;
            _gridWafersField.GridContext.SelectedRowIDs.RemoveRange(0, _gridWafersField.GridContext.SelectedRowIDs.Count);
            _gridWafersField.GridContext.SelectedRowIDs.Add(_gridWafersField.GridContext.SelectedRowID);
            CamstarWebControl.SetRenderToClient(_gridWafersField);
        }
        private void ResetFields()
        {
            _gridLotInfoField.ClearData();
            _gridWafersField.ClearData();
        }

        private void SetLotSelection(string sSelectionId)
        {
            JQDataGrid _gridLotInfoFieldx = Page.FindCamstarControl("LotInfoFieldGrid") as JQDataGrid;
            //_gridLotInfoField.ClearData();
            SEMI.AppCode.UIUtility.GetLotQuerySelection(this, this.PrimaryServiceType.ToString(), sSelectionId, false, ref _gridLotInfoFieldx, "LotInfoFieldGrid", false, new string[] { "__STYLE" });

        }

        public void AddButton_Click(object sender, EventArgs e)
        {
            try
            {
                //fix for IR 11094954
                //if SelectedRowID is null and SelectedRowIDs.Count > 0 then
                ////default SelectedRowID to the 1st available SelectedRowID in the list
              
                if (string.IsNullOrEmpty(_gridWafersField.GridContext.SelectedRowID) && (_gridWafersField.GridContext.SelectedRowIDs.Count != 0))
                {
                    _gridWafersField.GridContext.SelectedRowID = _gridWafersField.GridContext.SelectedRowIDs[0];
                }

                if (_gridSlotMapsDetailsField.GridContext.GetTotalRows() != 0 && _gridWafersField.GridContext.GetTotalRows() != 0 && _chkboxIsLotAssignedSlotMap.CheckControl.Checked == false)
                {
                    if ((!string.IsNullOrEmpty(_gridWafersField.GridContext.SelectedRowID) && _gridWafersField.GridContext.SelectedRowIDs.Count == 1) && (_ddlSlotNumberField.Data != null && !_ddlSlotNumberField.Text.Equals("")))
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
                    else if (string.IsNullOrEmpty(_gridWafersField.GridContext.SelectedRowID))
                    {
                        throw new Exception("No Slots have been selected from the Wafer Grid.");
                    }
                    else if (_ddlSlotNumberField.Data == null || _ddlSlotNumberField.Text.Equals(""))
                    {
                        throw new Exception("A Slot Number has not been selected.");
                    }
                    else if (_gridWafersField.GridContext.SelectedRowIDs.Count > 1 && (_ddlSlotNumberField.Data != null || !_ddlSlotNumberField.Text.Equals("")))
                    {
                        throw new Exception("The Add button can only process one wafer at a time.");
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

                if ((_gridWafersField.SelectedRowCount == _gridWafersField.TotalRowCount || _gridWafersField.SelectedRowCount == 0) && (_ddlSlotNumberField.Data == null || _ddlSlotNumberField.Text.Equals("")))
                {
                    if (CountSlot() >= _gridWafersField.TotalRowCount)
                    {
                        if (_gridSlotMapsDetailsField.GridContext.GetTotalRows() != 0 && _gridWafersField.GridContext.GetTotalRows() != 0 && _chkboxIsLotAssignedSlotMap.CheckControl.Checked == false)
                        {
                            for (int j = 0; j < _gridWafersField.GridContext.GetTotalRows(); j++)
                            {
                                string strGridWaferRowID = j.ToString().PadLeft(6, '0');

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
                                                Regex regex = new Regex(@"\(([^\}]+)\)");

                                                //string inputLot = _gridWafersField.GridContext.GetCell(strGridWaferRowID, "Container").ToString().Replace("(LOT)", "");
                                                string inputLot = _gridWafersField.GridContext.GetCell(strGridWaferRowID, "Container").ToString();
                                                inputLot = regex.Replace(inputLot, "");

                                                LotWafers wafersList = new LotWafers();
                                                wafersList.WaferNumber = _gridWafersField.GridContext.GetCell(strGridWaferRowID, "WaferNumber").ToString();
                                                wafersList.WaferScribeNumber = _gridWafersField.GridContext.GetCell(strGridWaferRowID, "WaferScribeNumber").ToString();
                                                wafersList.Container = new ContainerRef(inputLot);

                                                if (!isSlotMapAssigned((_gridSlotMapsDetailsField.GridContext as BoundContext).Data as SlotMapDetails[], wafersList))
                                                {
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
                                        }
                                        else
                                        {
                                            throw new Exception("All wafers are currently assigned.");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                else if ((_gridWafersField.SelectedRowCount != _gridWafersField.TotalRowCount) || (_gridWafersField.SelectedRowCount == _gridWafersField.TotalRowCount && (_ddlSlotNumberField.Data != null || !_ddlSlotNumberField.Text.Equals(""))))
                {
                    List<string> selectedWaferLots = new List<string>();
                    selectedWaferLots = _gridWafersField.GridContext.SelectedRowIDs;

                    if ((_ddlSlotNumberField.Data == null || _ddlSlotNumberField.Text.Equals("")))
                    {
                        int RowSelected = 0;

                        if (selectedWaferLots.Count > _gridSlotMapsDetailsField.TotalRowCount)
                        {
                            rowAdded = false;
                        }

                        else if (selectedWaferLots.Count <= _gridSlotMapsDetailsField.TotalRowCount)
                        {
                            for (int i = 0; i < selectedWaferLots.Count; i++)
                            {
                                string WaferRowID = selectedWaferLots[i].ToString().PadLeft(6, '0');
                                string strSlotMapDetailsRowID = i.ToString().PadLeft(6, '0');

                                Regex regex = new Regex(@"\(([^\}]+)\)");

                                //string inputLot = _gridWafersField.GridContext.GetCell(strGridWaferRowID, "Container").ToString().Replace("(LOT)", "");
                                string inputLot = _gridWafersField.GridContext.GetCell(WaferRowID, "Container").ToString();
                                inputLot = regex.Replace(inputLot, "");

                                LotWafers wafersList = new LotWafers();
                                wafersList.WaferNumber = _gridWafersField.GridContext.GetCell(WaferRowID, "WaferNumber").ToString();
                                wafersList.WaferScribeNumber = _gridWafersField.GridContext.GetCell(WaferRowID, "WaferScribeNumber").ToString();
                                wafersList.Container = new ContainerRef(inputLot);

                                if (!isSlotMapAssigned((_gridSlotMapsDetailsField.GridContext as BoundContext).Data as SlotMapDetails[], wafersList))
                                {
                                    _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapDetailsRowID, "WaferNumber", _gridWafersField.GridContext.GetCell(WaferRowID, "WaferNumber").ToString());
                                    _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapDetailsRowID, "WaferScribeNumber", _gridWafersField.GridContext.GetCell(WaferRowID, "WaferScribeNumber").ToString());
                                    _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapDetailsRowID, "Lot", inputLot);
                                    _gridWafersField.GridContext.SetCell(WaferRowID, "Grade", "Add");
                                    CamstarWebControl.SetRenderToClient(_gridWafersField);
                                    CamstarWebControl.SetRenderToClient(_gridSlotMapsDetailsField);
                                    RowSelected++;
                                    rowAdded = true;
                                }
                            }
                        }
                    }

                    else if ((_ddlSlotNumberField.Data != null || !_ddlSlotNumberField.Text.Equals("")))
                    {
                        if (selectedWaferLots == null)
                        {
                            selectedWaferLots = new List<string>();
                            for (int i = 0; i < _gridWafersField.GridContext.GetTotalRows(); i++)
                            {
                                selectedWaferLots.Add(i.ToString());
                            }
                        }
                        else
                        {
                            if (selectedWaferLots.Count == 0)
                            {
                                selectedWaferLots = new List<string>();
                                for (int i = 0; i < _gridWafersField.GridContext.GetTotalRows(); i++)
                                {
                                    selectedWaferLots.Add(i.ToString());
                                }
                            }
                        }

                        int SlotSelected = Int32.Parse(_ddlSlotNumberField.Data.ToString()) - 1;
                        int isEnoughRowsNotNull = (selectedWaferLots.Count) + (SlotSelected);

                        if (isEnoughRowsNotNull > _gridSlotMapsDetailsField.TotalRowCount)
                        {
                            rowAdded = false;
                        }

                        else if (isEnoughRowsNotNull <= _gridSlotMapsDetailsField.TotalRowCount)
                        {
                            for (int i = 0; i < selectedWaferLots.Count; i++)
                            {
                                string WaferRowID = selectedWaferLots[i].ToString().PadLeft(6, '0');
                                string strSlotMapDetailsRowID = SlotSelected.ToString().PadLeft(6, '0');

                                Regex regex = new Regex(@"\(([^\}]+)\)");

                                //string inputLot = _gridWafersField.GridContext.GetCell(strGridWaferRowID, "Container").ToString().Replace("(LOT)", "");
                                string inputLot = _gridWafersField.GridContext.GetCell(WaferRowID, "Container").ToString();
                                inputLot = regex.Replace(inputLot, "");

                                LotWafers wafersList = new LotWafers();
                                wafersList.WaferNumber = _gridWafersField.GridContext.GetCell(WaferRowID, "WaferNumber").ToString();
                                wafersList.WaferScribeNumber = _gridWafersField.GridContext.GetCell(WaferRowID, "WaferScribeNumber").ToString();
                                wafersList.Container = new ContainerRef(inputLot);

                                if (!isSlotMapAssigned((_gridSlotMapsDetailsField.GridContext as BoundContext).Data as SlotMapDetails[], wafersList))
                                {
                                    _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapDetailsRowID, "WaferNumber", _gridWafersField.GridContext.GetCell(WaferRowID, "WaferNumber").ToString());
                                    _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapDetailsRowID, "WaferScribeNumber", _gridWafersField.GridContext.GetCell(WaferRowID, "WaferScribeNumber").ToString());
                                    _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapDetailsRowID, "Lot", inputLot);
                                    _gridWafersField.GridContext.SetCell(WaferRowID, "Grade", "Add");
                                    CamstarWebControl.SetRenderToClient(_gridWafersField);
                                    CamstarWebControl.SetRenderToClient(_gridSlotMapsDetailsField);
                                    SlotSelected++;
                                    rowAdded = true;
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < _gridSlotMapsDetailsField.GridContext.GetTotalRows(); i++)
                {
                    SlotRowID = IdGenerator(i);
                    if (_gridSlotMapsDetailsField.GridContext.GetCell(SlotRowID, "Lot") == null) continue;
                    Regex regex = new Regex(@"\(([^\}]+)\)");
                    string inputLot = _gridSlotMapsDetailsField.GridContext.GetCell(SlotRowID, "Lot").ToString();
                    inputLot = regex.Replace(inputLot, "");
                    _gridSlotMapsDetailsField.GridContext.SetCell(SlotRowID, "Lot", inputLot);
                }

                if (!rowAdded)
                {
                    throw new Exception("The slot map has not enough slot for all wafers, either none or partial wafers are assigned.");
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }

        public void RemoveButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_gridSlotMapsDetailsField.GridContext.GetTotalRows() != 0 && _gridWafersField.GridContext.GetTotalRows() != 0 && _chkboxIsLotAssignedSlotMap.CheckControl.Checked == false)
                {
                    if (_ddlSlotNumberField.Data != null || !_ddlSlotNumberField.Text.Equals(""))
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
                    else
                    {
                        throw new Exception("A Slot must be selected from the Carrier Slot Map grid.");
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }

        public void RemoveAllButton_Click(object sender, EventArgs e)
        {
            try
            {
                bool isNotEmpty = false;
                if (_gridSlotMapsDetailsField.GridContext.GetTotalRows() != 0 && _gridWafersField.GridContext.GetTotalRows() != 0 && _chkboxIsLotAssignedSlotMap.CheckControl.Checked == false)
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
                                isNotEmpty = true;
                                break;
                            }
                        }
                    }
                }

                if (!isNotEmpty)
                {
                    throw new Exception("A Slot must be selected from the Carrier Slot Map grid.");
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }

        //Include Assign Slots Service from US101306
        public void AssignSlotsButton_Click(object sender, EventArgs e)
        {
            try
            {
                bool isAssigned = false;
                string sSlotAssignmentMethod = _SlotAssignmentMethodField.Data.ToString();

                if (_chkboxIsLotAssignedSlotMap.CheckControl.Checked) isAssigned = true;
                serviceAssignment = new scsWaferSlotMappingService(_gridWafersField, _gridSlotMapsDetailsField, isAssigned);

                if (_gridWafersField.TotalRowCount == 0)
                {
                    throw new Exception("No wafers are available.");
                }
                else
                {
                    if (_gridWafersField.TotalRowCount > CountSlot())
                    {
                        throw new Exception("The slot map has not enough slot for all wafers, either none or partial wafers are assigned.");
                    }

                    bool isAllAssigned = true;
                    foreach (LotWafers wafersList in (_gridWafersField.GridContext as BoundContext).Data as LotWafers[])
                    {
                        if (!isSlotMapAssigned((_gridSlotMapsDetailsField.GridContext as BoundContext).Data as SlotMapDetails[], wafersList))
                        {
                            isAllAssigned = false;
                            break;
                        }
                    }

                    if (isAllAssigned)
                    {
                        throw new Exception("All wafers are currently assigned.");
                    }
                }

                if (sSlotAssignmentMethod == "1")
                {
                    serviceAssignment.AssignWafersToSlots("ascall");
                }

                if (sSlotAssignmentMethod == "2")
                {
                    serviceAssignment.AssignWafersToSlots("dscall");
                }

                if (sSlotAssignmentMethod == "3")
                {
                    serviceAssignment.AssignWafersToSlots("ascfill");
                }

                if (sSlotAssignmentMethod == "4")
                {
                    serviceAssignment.AssignWafersToSlots("dscfill");
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }

        void ToCarrierBeforeDataLoad()
        {
            _ndoCarrierField.PickListPanelControl.DataProvider.BeforeDataLoad += (object sender, CWC.PickLists.DataLoadEventArgs args) =>
            {
                var data = args.ServiceData as CarrierAssignSlotMap;

                int totalRow = _gridLotInfoField.TotalRowCount;

                List<ContainerRef> Containers = new List<ContainerRef>();

                if (totalRow != 0)
                {
                    for (int i = 0; i < totalRow; i++)
                    {
                        string sSelectionId = _gridLotInfoField.GridContext.GetCell(i, "Lot").ToString();

                        if (i == 0)
                        {
                            data.ContainerNames += sSelectionId.ToString();
                        }
                        else
                        {
                            data.ContainerNames += "," + sSelectionId.ToString();
                        }
                    }
                }
            };
            _ndoCarrierField.PickListPanelControl.ReloadData();
        }

        private void FetchData(string sEventName)
        {
            try
            {
                Page.StatusBar.ClearMessage();

                // Get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                string sServiceType = this.PrimaryServiceType;

                // Run proper constructor and prepare 
                var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
                var svcConstructor = svcType.GetConstructor(new Type[] { typeof(Camstar.WCF.ObjectStack.UserProfile) });
                var svc = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

                // Service Data
                var data = CreateServiceData(sServiceType);
                var oServiceData = data as CarrierAssignSlotMap;

                CreateInsertionDetails_Info oInsertionDetailsInfo = new CreateInsertionDetails_Info();
                CreateInsertionDetails[] oInsertionDetailsData = null;

                // Service Info
                var info = CreateServiceInfo(sServiceType);
                var oServiceInfo = info as CarrierAssignSlotMap_Info;

                string sSelectionId = _txtSelectionIdField.Data != null ? _txtSelectionIdField.Data.ToString() : "";

                // Prepare the request 
                if (sEventName == const_sSelectionId)
                {
                    oServiceData.SelectionId = sSelectionId;
                    oServiceInfo.Containers = FieldInfoUtil.RequestValue();
                    oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
                    oServiceInfo.AssignedCarrier = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotWafers = new LotWafers_Info();
                    oServiceInfo.LotWafers.WaferSequence = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotWafers.WaferNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotWafers.WaferScribeNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotWafers.Container = FieldInfoUtil.RequestValue();
                    oServiceInfo.ss_IsLotAssignedSlotMap = FieldInfoUtil.RequestValue();

                    oServiceData.Carrier = new NamedObjectRef();
                    oServiceData.Carrier = _ndoCarrierField.Data as NamedObjectRef;
                    oServiceInfo.SlotMapSelection = new SlotMapDetails_Info();
                    oServiceInfo.SlotMapSelection.SlotNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.SlotMapSelection.WaferNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.SlotMapSelection.WaferScribeNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.SlotMapSelection.Lot = FieldInfoUtil.RequestValue();
                    oServiceInfo.SlotMapSelection.Status = FieldInfoUtil.RequestValue();
                }

                if (sEventName == const_sCarrier)
                {
                    oServiceData.Carrier = new NamedObjectRef();
                    oServiceData.Carrier = _ndoCarrierField.Data as NamedObjectRef;
                    oServiceInfo.SlotMapSelection = new SlotMapDetails_Info();
                    oServiceInfo.SlotMapSelection.SlotNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.SlotMapSelection.WaferNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.SlotMapSelection.WaferScribeNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.SlotMapSelection.Lot = FieldInfoUtil.RequestValue();
                    oServiceInfo.SlotMapSelection.Status = FieldInfoUtil.RequestValue();
                }

                // Prepare Request
                var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
                (oRequest as Request).Info = oServiceInfo;

                Result oResult;

                // Request the data
                ResultStatus oResultStatus = (svc as IShopFloorBase).ResolveSelectionId(oServiceData as DCObject, oRequest as Request, out oResult);

                if (oResultStatus.IsSuccess)
                {
                    // Set the result object
                    var oResponseData = oResult.Value as CarrierAssignSlotMap;

                    if (oResponseData.ss_IsLotAssignedSlotMap != null)
                        _chkboxIsLotAssignedSlotMap.CheckControl.Checked = oResponseData.ss_IsLotAssignedSlotMap.Value;

                    // Display information for resolve selection id
                    if (sEventName == const_sSelectionId)
                    {
                        // Set the value of first Wafer Carrier associated to the entered Lots in the Lots grid to the "To Carrier" field
                        if (oResponseData.AssignedCarrier != null && _ndoCarrierField.Data == null)
                        {
                            _ndoCarrierField.Data = oResponseData.AssignedCarrier.Name.ToString();
                        }

                        if (oServiceInfo.Containers != null)
                            if (oResponseData.Containers != null)
                            {
                                foreach (ContainerRef Name in oResponseData.Containers)
                                {
                                    Regex regex = new Regex(@"\(([^\}]+)\)");
                                    string inputLot = Name.ToString();
                                    inputLot = regex.Replace(inputLot, "");
                                    SetLotSelection(inputLot);
                                }

                                if (oResponseData.SelectionContainer != null && _ContainerField.Data == null)
                                {
                                    int totalRow = _gridLotInfoField.TotalRowCount;

                                    for (int i = 0; i < totalRow; i++)
                                    {
                                        UpdateWaferGrid(i.ToString());
                                    }
                                }
                            }

                        if (oResponseData.LotWafers != null)
                        {
                            foreach (LotWafers wafersList in oResponseData.LotWafers)
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
                                if (oResponseData.AssignedCarrier != null && oResponseData.SlotMapSelection != null)
                                {
                                    if (!isSlotMapAssigned((_gridSlotMapsDetailsField.GridContext as BoundContext).Data as SlotMapDetails[], wafersList))
                                    {
                                        //(waferDetails as LotWafers).Grade = "Add";
                                        waferGrade = "";
                                    }
                                }
                                else
                                {
                                    if (isSlotMapAssigned((_gridSlotMapsDetailsField.GridContext as BoundContext).Data as SlotMapDetails[], wafersList))
                                    {
                                        waferGrade = "Add";
                                    }
                                }

                                WafersGrid_AddNewRow((int)wafersList.WaferSequence, (string)wafersList.WaferNumber, (string)wafersList.WaferScribeNumber, wafersList.Container, waferGrade);

                                //_gridWafersField.GridContext.AdjustCurrentPage(id);
                                //CamstarWebControl.SetRenderToClient(_gridWafersField);
                            }
                        }

                        if (oResponseData.SlotMapSelection != null)
                        {
                            ArrayList slotNo = new ArrayList();

                            foreach (SlotMapDetails slotmapList in oResponseData.SlotMapSelection)
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
                    else if (sEventName == const_sCarrier)
                    {
                        if (oResponseData.SlotMapSelection != null)
                        {
                            ArrayList slotNo = new ArrayList();

                            foreach (SlotMapDetails slotmapList in oResponseData.SlotMapSelection)
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

                                // Update values in "Slot Number" field whenever "To Carrier" field changed
                                _ddlSlotNumberField.Data = slotNo;
                                _ddlSlotNumberField.DataBind();

                                //_gridSlotMapsDetailsField.GridContext.AdjustCurrentPage(id);
                                //CamstarWebControl.SetRenderToClient(_gridSlotMapsDetailsField);
                            }
                            AddData(slotNo);

                            if (_gridWafersField.GridContext.GetTotalRows() != 0 && _chkboxIsLotAssignedSlotMap.CheckControl.Checked == false)
                            {
                                for (int j = 0; j < _gridWafersField.GridContext.GetTotalRows(); j++)
                                {
                                    string strGridWaferRowID = j.ToString().PadLeft(6, '0');
                                    _gridWafersField.GridContext.SetCell(strGridWaferRowID, "Grade", "");
                                    CamstarWebControl.SetRenderToClient(_gridWafersField);
                                }
                            }
                        }
                    }
                }
                //_gridLotInfoField.BoundContext.LoadData();

                //Select first row in grid by default
                //_gridLotInfoField.GridContext.SelectRow(_gridLotInfoField.GridContext.GetRowId(0), true); 

                else
                {
                    DisplayMessage(oResultStatus);
                    //ResetFields();
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }

        private Boolean isSlotMapAssigned(SlotMapDetails[] slotMapList, LotWafers wafer)
        {
            if (slotMapList != null)
            {
                foreach (SlotMapDetails slotmap in slotMapList)
                {
                    if (slotmap.WaferNumber != null && slotmap.WaferScribeNumber != null && slotmap.Lot != null)
                    {
                        if (slotmap.WaferNumber == wafer.WaferNumber && slotmap.WaferScribeNumber == wafer.WaferScribeNumber && slotmap.Lot.Name == wafer.Container.Name)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private string IdGenerator(int num) { return num.ToString().PadLeft(6, '0'); }

        private int CountSlot()
        {
            int index = 0;
            for (int i = 0; i < _gridSlotMapsDetailsField.GridContext.GetTotalRows(); i++)
            {
                SlotRowID = IdGenerator(i);
                if (_gridSlotMapsDetailsField.GridContext.GetCell(SlotRowID, "Status").ToString() == "UP" &&
                    (_gridSlotMapsDetailsField.GridContext.GetCell(SlotRowID, "WaferNumber") == null || _gridSlotMapsDetailsField.GridContext.GetCell(SlotRowID, "WaferNumber").ToString() == ""))
                    index++;
            }

            return index;
        }

        public void SelectionIdField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                string sContainer = _txtSelectionIdField.Data.ToString();
                string strSelectedGridId = (_gridLotInfoField.GridContext as BoundContext).GetRowIdByCellValue("Lot", sContainer);

                if (string.IsNullOrEmpty(strSelectedGridId))
                    FetchData(const_sSelectionId);

                _txtSelectionIdField.TextControl.Text = "";
                _txtSelectionIdField.Focus();

                if (_chkboxIsLotAssignedSlotMap.CheckControl.Checked == true)
                    throw new Exception("A virtual Slop Map was already assigned with Lot Assign Experiment. Carrier slot map assigmnet is not allowed.");
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.Message, false));
            }
        }

        private void UpdateWaferGrid(string sRowID)
        {
            // Get the session and user profile
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            string sServiceType = this.PrimaryServiceType;

            // Run proper constructor and prepare 
            var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
            var svcConstructor = svcType.GetConstructor(new Type[] { typeof(Camstar.WCF.ObjectStack.UserProfile) });
            var svc = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

            // Service Data
            var data = CreateServiceData(sServiceType);
            var oServiceData = data as CarrierAssignSlotMap;

            CreateInsertionDetails_Info oInsertionDetailsInfo = new CreateInsertionDetails_Info();

            // Service Info
            var info = CreateServiceInfo(sServiceType);
            var oServiceInfo = info as CarrierAssignSlotMap_Info;

            string sSelectionId = _gridLotInfoField.GridContext.GetCell(sRowID, "Lot").ToString();

            // Prepare the request 
            oServiceData.SelectionId = sSelectionId;
            oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
            oServiceInfo.AssignedCarrier = FieldInfoUtil.RequestValue();
            oServiceInfo.LotWafers = new LotWafers_Info();
            oServiceInfo.LotWafers.WaferSequence = FieldInfoUtil.RequestValue();
            oServiceInfo.LotWafers.WaferNumber = FieldInfoUtil.RequestValue();
            oServiceInfo.LotWafers.WaferScribeNumber = FieldInfoUtil.RequestValue();
            oServiceInfo.LotWafers.Container = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_IsLotAssignedSlotMap = FieldInfoUtil.RequestValue();

            oServiceData.Carrier = new NamedObjectRef();
            oServiceData.Carrier = _txtSelectionIdField.Data as NamedObjectRef;
            oServiceInfo.SlotMapSelection = new SlotMapDetails_Info();
            oServiceInfo.SlotMapSelection.SlotNumber = FieldInfoUtil.RequestValue();
            oServiceInfo.SlotMapSelection.WaferNumber = FieldInfoUtil.RequestValue();
            oServiceInfo.SlotMapSelection.WaferScribeNumber = FieldInfoUtil.RequestValue();
            oServiceInfo.SlotMapSelection.Lot = FieldInfoUtil.RequestValue();
            oServiceInfo.SlotMapSelection.Status = FieldInfoUtil.RequestValue();

            // Prepare Request
            var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
            (oRequest as Request).Info = oServiceInfo;

            Result oResult;

            // Request the data
            ResultStatus oResultStatus = (svc as IShopFloorBase).ResolveSelectionId(oServiceData as DCObject, oRequest as Request, out oResult);

            if (oResultStatus.IsSuccess)
            {
                // Set the result object
                var oResponseData = oResult.Value as CarrierAssignSlotMap;

                // Set the value of the resolve Container Name and other information
                _ContainerField.Data = oResponseData.SelectionContainer.Name.ToString();

                // Set the value of the resolve current Assigned Carrier of a Container
                if (oResponseData.AssignedCarrier != null && _ndoCarrierField.Data == null)
                {
                    _ndoCarrierField.Data = oResponseData.AssignedCarrier.Name.ToString();
                }

                if (oResponseData.LotWafers != null)
                {
                    foreach (LotWafers wafersList in oResponseData.LotWafers)
                    {
                        string waferGrade = "";
                        if (oResponseData.AssignedCarrier != null && oResponseData.SlotMapSelection != null)
                        {
                            if (!isSlotMapAssigned((_gridSlotMapsDetailsField.GridContext as BoundContext).Data as SlotMapDetails[], wafersList))
                            {
                                //(waferDetails as LotWafers).Grade = "Add";
                                waferGrade = "";
                            }
                        }
                        else
                        {
                            if (isSlotMapAssigned((_gridSlotMapsDetailsField.GridContext as BoundContext).Data as SlotMapDetails[], wafersList))
                            {
                                waferGrade = "Add";
                            }
                        }
                        WafersGrid_AddNewRow((int)wafersList.WaferSequence, (string)wafersList.WaferNumber, (string)wafersList.WaferScribeNumber, wafersList.Container, waferGrade);
                    }
                }

                if (oResponseData.SlotMapSelection != null)
                {
                    ArrayList slotNo = new ArrayList();

                    foreach (SlotMapDetails slotmapList in oResponseData.SlotMapSelection)
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

                        // Update values in "Slot Number" field whenever "To Carrier" field changed
                        _ddlSlotNumberField.Data = slotNo;
                        _ddlSlotNumberField.DataBind();

                        //_gridSlotMapsDetailsField.GridContext.AdjustCurrentPage(id);
                        //CamstarWebControl.SetRenderToClient(_gridSlotMapsDetailsField);
                    }
                    AddData(slotNo);
                }
            }
        }

        public void LotInfoFieldGrid_RowSelected(object sender, EventArgs e)
        {
            try
            {
                if (_ndoCarrierField.Data != null)
                {
                    _ddlSlotNumberField.ClearData();
                    _ddlSlotNumberField.PickListPanelControl.ClearSelectionValues();
                    _gridWafersField.ClearData();
                }
                else
                {
                    _ndoCarrierField.ClearData();
                    _ddlSlotNumberField.ClearData();
                    _ddlSlotNumberField.PickListPanelControl.ClearSelectionValues();
                    _gridWafersField.ClearData();
                    _gridSlotMapsDetailsField.ClearData();
                }

                Array sRowIDs = _gridLotInfoField.SelectedRowIDs;

                if (sRowIDs != null && sRowIDs.Length > 0)
                {
                    //update the wafer list based on the selected lots
                    foreach (string sRowID in sRowIDs)
                    {
                        UpdateWaferGrid(sRowID);
                    }
                }
                else
                {
                    //if no lot is selected, display all the wafers
                    int totalRow = _gridLotInfoField.TotalRowCount;

                    for (int i = 0; i < totalRow; i++)
                    {
                        UpdateWaferGrid(i.ToString());
                    }
                }
            }

            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }

        public void LotInfoFieldGrid_RowDelete(object sender, EventArgs e)
        {
            // get the container that was removed
            string sContainer = _ContainerField.Data.ToString();

            if (sContainer != "")
            {
                _ndoCarrierField.ClearData();
                _gridWafersField.ClearData();
                _gridSlotMapsDetailsField.ClearData();
                _ContainerField.ClearData();

                //update the wafer list after deleting the lot
                int totalRow = _gridLotInfoField.TotalRowCount;

                for (int i = 0; i < totalRow; i++)
                {
                    UpdateWaferGrid(i.ToString());
                }
            }
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            int totalRow = _gridLotInfoField.TotalRowCount;

            List<ContainerRef> Containers = new List<ContainerRef>();
            for (int i = 0; i < totalRow; i++)
            {
                string sSelectionId = _gridLotInfoField.GridContext.GetCell(i, "Lot").ToString();
                Containers.Add(new ContainerRef(sSelectionId));
            }
                (serviceData as CarrierAssignSlotMap).Containers = Containers.ToArray();
        }

        public void ToCarrier_DataChanged(object sender, EventArgs e)
        {
            if (!_ndoCarrierField.IsEmpty)
            {
                _gridSlotMapsDetailsField.ClearData();
                _ddlSlotNumberField.ClearData();
                _ddlSlotNumberField.PickListPanelControl.ClearSelectionValues();
                FetchData(const_sCarrier);
            }
        }

        public void SlotAssignmentMethod()
        {
            if (!_SlotAssignmentMethodField.IsEmpty)
            {
                if (_SlotAssignmentMethodField.Data.ToString() == "0")
                {
                    this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "Add").First().IsDisabled = false;
                    this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "AddAll").First().IsDisabled = false;
                    this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "AssignSlots").First().IsDisabled = true;

                    _ddlSlotNumberField.Enabled = true;

                    _gridWafersField.GridContext.RowSelectionMode = JQGridSelectionMode.MultiRowSelect;
                }
                else
                {
                    this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "Add").First().IsDisabled = true;
                    this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "AddAll").First().IsDisabled = true;
                    this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "AssignSlots").First().IsDisabled = false;

                    _ddlSlotNumberField.Enabled = false;

                    _gridWafersField.GridContext.RowSelectionMode = JQGridSelectionMode.Disable;
                }
                CamstarWebControl.SetRenderToClient(_gridWafersField);
            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            if (action != null && action.Parameters == "Clear")
            {
                Page.ClearValues();
                _rdoRecipeField.Visible = false;
                ResetFields();
                _gridSlotMapsDetailsField.ClearData();
                _ddlSlotNumberField.ClearData();
                _ddlSlotNumberField.PickListPanelControl.ClearSelectionValues();
            }
        }

        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                Page.ShopfloorReset(null, null);
                ResetFields();
            }
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
                        if (oExisting[i].WaferSequence.Equals(oNewDetail[0].WaferSequence) && oExisting[i].WaferScribeNumber.Equals(oNewDetail[0].WaferScribeNumber))
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

        public void SlotMapGrid_AddNewRow(string SlotNumber, string WaferNumber, string WaferScribeNumber, ContainerRef Lot, string Status)
        {
            try
            {
                JQDataGrid _gridDetails = _gridSlotMapsDetailsField;
                SlotMapDetails[] oNewDetail = new SlotMapDetails[1];
                oNewDetail[0] = new SlotMapDetails();
                oNewDetail[0].SlotNumber = SlotNumber;
                oNewDetail[0].WaferNumber = WaferNumber;
                oNewDetail[0].WaferScribeNumber = WaferScribeNumber;
                oNewDetail[0].Lot = Lot;
                oNewDetail[0].Status = Status;
                SlotMapDetails[] oExisting = (_gridDetails.GridContext as BoundContext).Data as SlotMapDetails[];
                if (oExisting != null)
                {
                    bool isUnique = true;
                    for (int i = 0; i < oExisting.Length; i++)
                    {
                        if (oExisting[i].SlotNumber.Equals(oNewDetail[0].SlotNumber))
                        {
                            isUnique = false;
                        }
                    }
                    if (isUnique)
                    {
                        SlotMapDetails[] oMerged = new SlotMapDetails[oExisting.Length + 1];
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

        //-----------------------------------------
        // On Popup closed event
        //-----------------------------------------
        public void OnPopupClose()
        {
            try
            {
                if (Page.DataContract.GetValueByName("SelectedLotsListDM") != null)
                {
                    string[] sContainers = Page.DataContract.GetValueByName("SelectedLotsListDM") as string[];
                    _envSelectedLots.SS_ContainersList = null;
                    Page.DataContract.SetValueByName("SelectedLotsListDM", null);
                    foreach (string container in sContainers)
                        _txtSelectionIdField.Data = container;
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }
    }
}