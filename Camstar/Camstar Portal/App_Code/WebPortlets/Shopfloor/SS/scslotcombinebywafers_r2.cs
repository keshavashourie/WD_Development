/* Copyright 2025 Siemens */
using System;
using System.Collections;
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

/// <summary>
/// Summary description for SS_LotCombineMain
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class scsLotCombineByItemsR2 : scsShopfloorBase
    {
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotCombine_SelectionId") as CWC.TextBox; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("LotCombine_ComputerName") as CWC.TextBox; } }
        protected CWC.TextBox _txtNewContainerName { get { return Page.FindCamstarControl("LotCombine_NewContainerName") as CWC.TextBox; } }
        protected CWC.TextBox _txtContainerToDelete { get { return Page.FindCamstarControl("LotCombine_ContainerToDelete") as CWC.TextBox; } }

        protected CWC.TextBox _txtServiceName { get { return Page.FindCamstarControl("LotCombine_ServiceName") as CWC.TextBox; } }
        protected CWC.TextBox _txtComment { get { return Page.FindCamstarControl("Shopfloor_Comments") as CWC.TextBox; } }

        protected CWC.NamedObject _ndoProcessType { get { return Page.FindCamstarControl("LotCombine_ProcessType") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEquipment { get { return Page.FindCamstarControl("LotCombine_Equipment") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("LotCombine_Employee") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoWaferHandler { get { return Page.FindCamstarControl("LotCombine_scsWaferHandlerEquipment") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoCarrier { get { return Page.FindCamstarControl("LotCombine_Carrier") as CWC.NamedObject; } }
        CWC.DropDownList _SlotAssignmentMethodField { get { return Page.FindCamstarControl("LotCombine_scsSlotAssignmentMethodEnum") as CWC.DropDownList; } }


        protected CWC.RevisionedObject _rdoRecipe { get { return Page.FindCamstarControl("LotCombine_Recipe") as CWC.RevisionedObject; } }

        protected CWC.DropDownList _ddlMainLot { get { return Page.FindCamstarControl("LotCombine_MainLot") as CWC.DropDownList; } }
        protected CWC.CheckBox _chkAutoSetNewName { get { return Page.FindCamstarControl("LotCombine_AutoSetNewContainerName") as CWC.CheckBox; } }
        protected JQDataGrid _gridContainer { get { return Page.FindCamstarControl("LotCombine_ContainerGrid") as JQDataGrid; } }
        CWC.Button _cmdbarAssignSlotsButton { get { return Page.FindCamstarControl("AssignSlots") as CWC.Button; } }

        protected JQDataGrid _gridByLotTargetLot { get { return Page.FindCamstarControl("LotCombine_ByLot_TargetLot") as JQDataGrid; } }
        protected JQDataGrid _gridByLotSourceLot { get { return Page.FindCamstarControl("LotCombine_ByLot_SourceLot") as JQDataGrid; } }

        protected JQDataGrid _gridByItemTargetItem { get { return Page.FindCamstarControl("LotCombine_ByItem_TargetItems") as JQDataGrid; } }
        protected JQDataGrid _gridByItemSourceItem { get { return Page.FindCamstarControl("LotCombine_ByItem_SourceItems") as JQDataGrid; } }

        protected JQDataGrid _gridSlotMapsDetailsField { get { return Page.FindCamstarControl("LotCombine_SlotMapDetailsGrid") as JQDataGrid; } }

        protected const string _kProcessTypeViewStateKey = "LotCombine_ProcessTypeSelection_ViewStateVariableKey";
        protected const int _kContainersPerBatch = 20;

        private SlotMapDetails[] mainSlotMapList;

        protected SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("LotCombine_DataEnvelop") as SEMI.AppCode.DataEnvelopControl; } }
        private scsWaferSlotMappingService serviceAssignment;

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _txtSelectionId.DataChanged += new EventHandler(_txtSelectionId_DataChanged);
            _ddlMainLot.DataChanged += new EventHandler(_ddlMainLot_DataChanged);
            _ndoEquipment.DataChanged += new EventHandler(_ndoEquipment_DataChanged);
            _ndoProcessType.DataChanged += new EventHandler(_ndoProcessType_DataChanged);
            _txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
            _cmdbarAssignSlotsButton.Click += new EventHandler(AssignSlotsButton_Click);
            _ndoCarrier.DataChanged += new EventHandler(_ndoCarrier_DataChanged);
            _SlotAssignmentMethodField.DataChanged += new EventHandler(_SlotAssignmentMethodField_DataChanged);
            _txtServiceName.Data = Page.PrimaryServiceType.ToString();

            ToCarrierBeforeDataLoad();

            if (!Page.IsPostBack)
            {
                //get containers from Container Search screen
                if (Page.Session["selectedContainers"] != null)
                {
                    string[] sContainersList = Page.Session["selectedContainers"] as string[];
                    foreach (string container in sContainersList)
                    {
                        _txtSelectionId.TextControl.Text = container;
                        _txtSelectionId_DataChanged(null, null);
                    }
                }
                Page.Session.Remove("selectedContainers");
            }

            if (Page.IsPostBack)
                // Check if it is a pop up close, get the return result
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {
                    if (_envSelectedLots != null)
                        // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                        if (Page.DataContract.GetValueByName("LotCombine_DataEnvelopDM") != null)
                            _envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("LotCombine_DataEnvelopDM") as string[];

                    if (_envSelectedLots.SS_ContainersList != null)
                    {
                        string[] sContainers;
                        sContainers = _envSelectedLots.SS_ContainersList;

                        if (sContainers.Length > 0)
                        {
                            foreach (string sContainer in sContainers)
                            {
                                // check if lot not exist in the grid else add to the grid
                                string strSelectedGridId = (_gridContainer.GridContext as BoundContext).GetRowIdByCellValue("Lot", sContainer);

                                if (string.IsNullOrEmpty(strSelectedGridId))
                                {
                                    _txtSelectionId.TextControl.Text = sContainer;
                                    _txtSelectionId_DataChanged(null, null);
                                    // FetchData("SelectionId");

                                }
                            }
                        }
                        //nullify the containers list
                        _envSelectedLots.SS_ContainersList = null;
                    }
                }

            if (_ndoWaferHandler.Data == null)
            {
                _rdoRecipe.Hidden = true;
            }
            else
            {
                _rdoRecipe.Hidden = false;
            }

            SlotAssignmentMethod();
        } // OnLoad

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _ddlMainLot_DataChanged(object sender, EventArgs e)
        {
            if (_ddlMainLot.DropDownControl.Items.Count > 0)
            {
                // clear the process type selection dropdown 
                _ndoProcessType.DropDownControl.Items.Clear();

                // clear the equipment selection dropdown
                _ndoEquipment.DropDownControl.Items.Clear();

                if (_ddlMainLot.DropDownControl.SelectedValue != "")
                {
                    // retrieve the process type selection of the selected container from viewstate                
                    Hashtable htProcessTypesSelection = new Hashtable();
                    if (ViewState[_kProcessTypeViewStateKey] != null)
                        htProcessTypesSelection = ViewState[_kProcessTypeViewStateKey] as Hashtable;

                    if (htProcessTypesSelection == null)
                        htProcessTypesSelection = new Hashtable();

                    // populate the process type selection
                    if (htProcessTypesSelection.ContainsKey(_ddlMainLot.DropDownControl.SelectedValue))
                    {
                        NamedObjectRef[] oProcessTypes = htProcessTypesSelection[_ddlMainLot.DropDownControl.SelectedValue] as NamedObjectRef[];
                        // add the processTypes to the ProcessType dropdown
                        foreach (NamedObjectRef oProcessType in oProcessTypes)
                            _ndoProcessType.DropDownControl.Items.Add(oProcessType.Name);

                    }

                    // if _txtNewContainerName == "" OR _chkAutoSetNewName.Checked = false
                    if (_txtNewContainerName.Data == "" || !(_chkAutoSetNewName.CheckControl.Checked))
                        AssignMainLot_ByItem(_ddlMainLot.DropDownControl.SelectedValue);

                    //_ndoCarrier.Data = opro;


                    CamstarWebControl.SetRenderToClient(_ndoProcessType);
                    CamstarWebControl.SetRenderToClient(_ndoEquipment);
                    // call the processType_datachanged event
                    _ndoProcessType_DataChanged(null, null);

                    _ndoCarrier_DataChanged(null, null);
                }
            }
        } // _ddlMainLot_DataChanged

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _ndoProcessType_DataChanged(object sender, EventArgs e)
        {
            // clear the equipment selection dropdown
            _ndoEquipment.DropDownControl.Items.Clear();

            if (_ddlMainLot.DropDownControl.SelectedValue != "" && _ndoProcessType.DropDownControl.SelectedValue != "")
            {
                FetchData("ProcessType", _ddlMainLot.DropDownControl.SelectedValue);
                if (_ndoEquipment.DropDownControl.SelectedValue != "" && Page.PrimaryServiceType != "LotCombineByWafers" && _ndoProcessType.DropDownControl.Items.Count > 1)
                    foreach (System.Web.UI.WebControls.ListItem oItem in _ddlMainLot.DropDownControl.Items)
                        FetchData("Equipment", oItem.Value);
            }

        } // _ndoProcessType_DataChanged

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _ndoEquipment_DataChanged(object sender, EventArgs e)
        {
            if (_ddlMainLot.DropDownControl.SelectedValue != ""
                && _ndoProcessType.DropDownControl.SelectedValue != ""
                && _ndoEquipment.DropDownControl.SelectedValue != ""
                && Page.PrimaryServiceType != "LotCombineByWafers")
            {
                foreach (System.Web.UI.WebControls.ListItem oItem in _ddlMainLot.DropDownControl.Items)
                    FetchData("Equipment", oItem.Value);
            }
        } // _ndoEquipment_DataChanged

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _txtSelectionId_DataChanged(object sender, EventArgs e)
        {
            string sContainer = _txtSelectionId.Data.ToString();
            //string strSelectedGridId= "";
            string sRowId = "";

            if (Page.IsPostBack)
            {
                //strSelectedGridId = (_gridContainer.GridContext as BoundContext).getGetRowIdByCellValue("Lot", sContainer);
                for (int x = 0; x < (_gridContainer.GridContext as BoundContext).GetTotalRows(); x++)
                    if ((_gridContainer.GridContext as BoundContext).GetCell(x.ToString().PadLeft(6, '0'), "Lot").ToString() == sContainer)
                    {
                        sRowId = x.ToString().PadLeft(6, '0');
                        break;
                    }
            }

            if (string.IsNullOrEmpty(sRowId))
                FetchData("SelectionId");

            _txtSelectionId.TextControl.Text = "";
            _txtSelectionId.Focus();
            // _txtSelectionId_DataChanged
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _ndoCarrier_DataChanged(object sender, EventArgs e)
        {
            clearAllSlotNumber();
            FetchExistingSlotMapGridData();
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _SlotAssignmentMethodField_DataChanged(object sender, EventArgs e)
        {
            _ndoCarrier_DataChanged(null, null);
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void SlotAssignmentMethod()
        {
            if (!_SlotAssignmentMethodField.IsEmpty)
            {
                if (_SlotAssignmentMethodField.Data.ToString() == "0")
                {
                    this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "AssignSlots").First().IsDisabled = true;
                    (_gridByItemSourceItem.GridContext as BoundContext).Fields["ToWaferScribeNumber"].Editable = true;
                    (_gridByItemSourceItem.GridContext as BoundContext).Fields["NDPW"].Editable = true;
                    (_gridByItemSourceItem.GridContext as BoundContext).Fields["GoodQty"].Editable = true;
                    (_gridByItemSourceItem.GridContext as BoundContext).Fields["SlotNumber"].Editable = false;
                    if (_ndoCarrier.Data == null || _ndoCarrier.Data.ToString() == "")
                    {
                        (_gridByItemTargetItem.GridContext as BoundContext).Fields["TargetItem_SlotNumber"].Visible = true;
                        (_gridByItemTargetItem.GridContext as BoundContext).Fields["TargetItem_SlotNumber_Manual"].Visible = false;
                        (_gridByItemSourceItem.GridContext as BoundContext).Fields["SlotNumber"].Visible = true;
                        (_gridByItemSourceItem.GridContext as BoundContext).Fields["SlotNumber_Manual"].Visible = false;

                    }
                    else
                    {
                        (_gridByItemTargetItem.GridContext as BoundContext).Fields["TargetItem_SlotNumber"].Visible = false;
                        (_gridByItemTargetItem.GridContext as BoundContext).Fields["TargetItem_SlotNumber_Manual"].Visible = true;
                        (_gridByItemSourceItem.GridContext as BoundContext).Fields["SlotNumber"].Visible = false;
                        (_gridByItemSourceItem.GridContext as BoundContext).Fields["SlotNumber_Manual"].Visible = true;
                    }
                }
                else
                {
                    this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "AssignSlots").First().IsDisabled = false;
                    (_gridByItemTargetItem.GridContext as BoundContext).Fields["TargetItem_SlotNumber"].Visible = true;
                    (_gridByItemTargetItem.GridContext as BoundContext).Fields["TargetItem_SlotNumber_Manual"].Visible = false;
                    (_gridByItemSourceItem.GridContext as BoundContext).Fields["ToWaferScribeNumber"].Editable = false;
                    (_gridByItemSourceItem.GridContext as BoundContext).Fields["NDPW"].Editable = false;
                    (_gridByItemSourceItem.GridContext as BoundContext).Fields["GoodQty"].Editable = false;
                    (_gridByItemSourceItem.GridContext as BoundContext).Fields["SlotNumber"].Visible = true;
                    (_gridByItemSourceItem.GridContext as BoundContext).Fields["SlotNumber"].Editable = false;
                    (_gridByItemSourceItem.GridContext as BoundContext).Fields["SlotNumber_Manual"].Visible = false;
                }
                _gridByItemTargetItem.GridContext.RowSelectionMode = JQGridSelectionMode.Disable;
                CamstarWebControl.SetRenderToClient(_gridByItemTargetItem);
                CamstarWebControl.SetRenderToClient(_gridByItemSourceItem);
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void ToCarrierBeforeDataLoad()
        {
            _ndoCarrier.PickListPanelControl.DataProvider.BeforeDataLoad += (object sender, CWC.PickLists.DataLoadEventArgs args) =>
            {
                var data = args.ServiceData as LotCombineByWafers;

                if (_ddlMainLot.Data != null)
                {
                    int i = 0;
                    foreach (System.Web.UI.WebControls.ListItem oItem in _ddlMainLot.DropDownControl.Items)
                    {
                        if (i == 0)
                        {
                            data.ContainerNames += oItem.ToString();
                        }
                        else
                        {
                            data.ContainerNames += "," + oItem.ToString();
                        }
                        i++;
                    }
                }
            };
            _ndoCarrier.PickListPanelControl.ReloadData();
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void FetchData(string sEventType, string sContainerName = "")
        {
            // Prepare service
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            ResultStatus oServiceResult = new ResultStatus(null, false);
            string sServiceType = "LotCombine";
            sServiceType = Page.PrimaryServiceType.ToString();

            // Run proper constructor. We need to be dynamic with the primary service type
            var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
            //create a request object
            var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
            var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
            var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

            //retriving data dynamically for Flexibility of the page.
            var oServiceData = CreateServiceData(sServiceType);

            var info = CreateServiceInfo(sServiceType);
            var oServiceInfo = info as LotCombine_Info;
            (oRequest as Request).Info = oServiceInfo;

            bool bExecuteResolveSelectionId = false;
            switch (sEventType)
            {
                case "SelectionId":
                    if (sContainerName == "")
                    {
                        bExecuteResolveSelectionId = true;
                        (oServiceData as LotCombine).SelectionId = _txtSelectionId.Data.ToString();
                        oServiceInfo.Containers = FieldInfoUtil.RequestValue();
                        oServiceInfo.MainCarrier = FieldInfoUtil.RequestValue();
                    }
                    else
                    {
                        (oServiceData as LotCombine).Container = new ContainerRef(sContainerName);
                    }

                    if (_ndoProcessType.Data != null)
                        (oServiceData as LotCombine).ProcessType = new NamedObjectRef(_ndoProcessType.Data.ToString());

                    if (_ndoEquipment.Data != null)
                        (oServiceData as LotCombine).Equipment = new NamedObjectRef(_ndoEquipment.Data.ToString());

                    oServiceInfo.ProcessTypeSelection = FieldInfoUtil.RequestValue();

                    if (sServiceType == "LotCombineByWafers")
                    {
                        oServiceInfo.LotWafers = new LotWafers_Info();
                        oServiceInfo.LotWafers.WaferScribeNumber = FieldInfoUtil.RequestValue();
                        oServiceInfo.LotWafers.NDPW = FieldInfoUtil.RequestValue();
                        oServiceInfo.LotWafers.GoodQty = FieldInfoUtil.RequestValue();
                        oServiceInfo.LotWafers.WaferNumber = FieldInfoUtil.RequestValue();
                    }
                    break;

                case "ProcessType":
                    (oServiceData as LotCombine).Container = new ContainerRef(sContainerName);
                    (oServiceData as LotCombine).ProcessType = new NamedObjectRef(_ndoProcessType.DropDownControl.SelectedValue);
                    oServiceInfo.EquipmentSelection = FieldInfoUtil.RequestValue();
                    break;

                case "Equipment":
                    (oServiceData as LotCombine).Container = new ContainerRef(sContainerName);
                    (oServiceData as LotCombine).ProcessType = new NamedObjectRef(_ndoProcessType.DropDownControl.SelectedValue);
                    (oServiceData as LotCombine).Equipment = new NamedObjectRef(_ndoEquipment.DropDownControl.SelectedValue);
                    break;
            }

            oServiceInfo.Container = FieldInfoUtil.RequestValue();

            if (sServiceType != "LotCombineByWafers")
            {
                oServiceInfo.MaxStandbyQty = FieldInfoUtil.RequestValue();
                oServiceInfo.MaxQtyToProcess = FieldInfoUtil.RequestValue();
                oServiceInfo.MaxInProcessQty = FieldInfoUtil.RequestValue();
                oServiceInfo.MaxProcessedQty = FieldInfoUtil.RequestValue();
            }
            // init the result object
            Result oResult = new Result();
            ResultStatus oResultStatus = new ResultStatus();

            // execute to request the value or resolveSelectionId
            if (bExecuteResolveSelectionId)
                oResultStatus = (oService as IShopFloorBase).ResolveSelectionId((oServiceData as DCObject), (oRequest as Request), out oResult);
            else
                oResultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oResult);

            try
            {
                if (oResultStatus.IsSuccess)
                {
                    string sResolvedContainer = (oResult.Value as LotCombine).Container.Name;
                    // check if the input container already exists in the grid
                    bool bNonExistingContainer = true;

                    if ((_gridContainer.GridContext as BoundContext).GetTotalRows() > 0)
                    {
                        string strSelectedGridId = "";

                        if (Page.IsPostBack)
                            strSelectedGridId = (_gridContainer.GridContext as BoundContext).GetRowIdByCellValue("Lot", sResolvedContainer);

                        if (string.IsNullOrEmpty(strSelectedGridId))
                            bNonExistingContainer = true;
                        else
                            bNonExistingContainer = false;
                    }

                    if ((oResult.Value as LotCombine).MainCarrier != null && _ndoCarrier.Data == null)
                    {
                        _ndoCarrier.Data = (oResult.Value as LotCombine).MainCarrier.ToString();
                    }

                    if (sEventType == "SelectionId")
                    {
                        if (bNonExistingContainer)
                        {
                            if (sServiceType == "LotCombineByWafers")
                                if ((oResult.Value as LotCombineByWafers).LotWafers != null)
                                    AddLotWaferRow((oResult.Value as LotCombineByWafers).LotWafers, sResolvedContainer);

                            if ((oResult.Value as LotCombine).ProcessTypeSelection != null)
                            {
                                // get the processTypes and store it in ViewState
                                Hashtable htProcessTypesSelection = new Hashtable();
                                if (ViewState[_kProcessTypeViewStateKey] != null)
                                    htProcessTypesSelection = ViewState[_kProcessTypeViewStateKey] as Hashtable;

                                if (htProcessTypesSelection == null)
                                    htProcessTypesSelection = new Hashtable();

                                if (htProcessTypesSelection.ContainsKey(sResolvedContainer))
                                    htProcessTypesSelection[sResolvedContainer] = (oResult.Value as LotCombine).ProcessTypeSelection;
                                else
                                    htProcessTypesSelection.Add(sResolvedContainer, (oResult.Value as LotCombine).ProcessTypeSelection);

                                ViewState[_kProcessTypeViewStateKey] = htProcessTypesSelection;

                                ////// add the processTypes to the ProcessType dropdown
                                ////foreach (NamedObjectRef oProcessType in (oResult.Value as LotCombine).ProcessTypeSelection)
                                ////    _ndoProcessType.DropDownControl.Items.Add(oProcessType.Name);                            
                            }

                            // add the ContainerName to the MainLot dropdown
                            _ddlMainLot.DropDownControl.Items.Add(sResolvedContainer);

                            JQDataGrid _gridContainerTemp = Page.FindCamstarControl("LotCombine_ContainerGrid") as JQDataGrid;
                            SEMI.AppCode.UIUtility.GetLotQuerySelection(this, sServiceType, sResolvedContainer, false, ref _gridContainerTemp, _gridContainerTemp.ID.ToString(), true);
                        }

                        if (sServiceType != "LotCombineByWafers")
                            if (bNonExistingContainer)
                                AddLotQtyRow(oResult.Value as LotCombine);

                        if (sContainerName == "")
                        {
                            if (_ddlMainLot.DropDownControl.SelectedValue == sResolvedContainer)
                                _ddlMainLot_DataChanged(null, null);

                            // loop through the return containers and recursively call FetchData
                            foreach (ContainerRef oContainer in (oResult.Value as LotCombine).Containers)
                                if (sResolvedContainer != oContainer.Name)
                                    FetchData("SelectionId", oContainer.Name);
                        }
                    }
                    else
                    {
                        if (sEventType == "ProcessType")
                        {
                            if ((oResult.Value as LotCombine).EquipmentSelection != null)
                            {
                                // for each equipment, add to the equipment selection                        
                                foreach (NamedObjectRef oEquipment in (oResult.Value as LotCombine).EquipmentSelection)
                                    _ndoEquipment.DropDownControl.Items.Add(oEquipment.Name);
                            }
                            // call the equipment data changed event 
                            _ndoEquipment_DataChanged(null, null);
                        }

                        if (sEventType == "Equipment")
                            UpdateLotQtyRow(oResult.Value as LotCombine);
                    }
                }
                else
                    DisplayMessage(oResultStatus);
            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message, false));
            }
        } // FetchData

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void FetchExistingSlotMapGridData()
        {
            try
            {
                Page.StatusBar.ClearMessage();
                _gridSlotMapsDetailsField.ClearData();

                // Get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                string sServiceType = "CarrierAssignSlotMap";

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

                oServiceData.Carrier = new NamedObjectRef();
                oServiceData.Carrier = _ndoCarrier.Data as NamedObjectRef;
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

                    if (oResponseData.SlotMapSelection != null)
                    {
                        mainSlotMapList = oResponseData.SlotMapSelection;
                        ArrayList slotNo = new ArrayList();
                        foreach (SlotMapDetails slotmapList in oResponseData.SlotMapSelection)
                        {
                            SlotMapGrid_AddNewRow((string)slotmapList.SlotNumber, (string)slotmapList.WaferNumber, (string)slotmapList.WaferScribeNumber, slotmapList.Lot, (string)slotmapList.Status);
                            slotNo.Add(Convert.ToString(slotmapList.SlotNumber));
                            if (slotmapList.Lot != null)
                            {
                                SetExistingSlotNo((string)slotmapList.SlotNumber, (string)slotmapList.WaferScribeNumber, (string)slotmapList.Lot.Name.ToString());
                            }
                        }
                        AddData(slotNo);
                    }
                    else
                    {
                        AddData(new ArrayList());
                    }
                }
                else
                {
                    DisplayMessage(oResultStatus);
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void SetExistingSlotNo(string SlotNumber, string WaferScribeNumber, string Lot)
        {
            SS_LotCombine_LotItem[] oExistingTargetList = (_gridByItemTargetItem.GridContext as BoundContext).Data as SS_LotCombine_LotItem[];
            SS_LotCombine_LotItem[] oExistingSourceList = (_gridByItemSourceItem.GridContext as BoundContext).Data as SS_LotCombine_LotItem[];

            foreach (SS_LotCombine_LotItem oSourceItem in oExistingTargetList)
            {
                if (oSourceItem.WaferScribeNumber == WaferScribeNumber && oSourceItem.FromContainer == Lot)
                {
                    oSourceItem.SlotNumber = SlotNumber;
                    oSourceItem.SlotNumber_Manual = SlotNumber;
                }
            }

            foreach (SS_LotCombine_LotItem oSourceItem in oExistingSourceList)
            {
                if (oSourceItem.WaferScribeNumber == WaferScribeNumber && oSourceItem.FromContainer == Lot)
                {
                    oSourceItem.SlotNumber = SlotNumber;
                    oSourceItem.SlotNumber_Manual = SlotNumber;
                }
            }

        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void AddData(ArrayList data)
        {
            var _ddlSlotNumberFieldEditor = _gridByItemTargetItem.FindControl("LotCombine_ByItem_TargetItems_TargetItem_SlotNumber_Manual_InlineEditorControl") as CWC.DropDownList;
            if (_ddlSlotNumberFieldEditor != null && _ddlSlotNumberFieldEditor.PickListPanelControl != null)
            {
                string[] slotNo = data.ToArray(typeof(string)) as string[];
                RecordSet rsNamedObject = new RecordSet();
                OM.Header[] rsHeaders = new OM.Header[2];
                Row[] rsRows = new Row[slotNo.Length];

                rsHeaders[0] = new OM.Header();
                rsHeaders[0].TypeCode = TypeCode.String;
                rsHeaders[0].Name = "Label";

                rsHeaders[1] = new OM.Header();
                rsHeaders[1].TypeCode = TypeCode.String;
                rsHeaders[1].Name = "Value";

                rsNamedObject.Headers = rsHeaders;

                for (int x = 0; x < slotNo.Length; x++)
                {
                    rsRows[x] = new Row();
                    string[] strRowValues = new string[2];
                    strRowValues[0] = slotNo[x];
                    strRowValues[1] = slotNo[x].TrimStart(new char[] {'0'});
                    rsRows[x].Values = strRowValues;
                }

                rsNamedObject.Rows = rsRows;

                ((FormsFramework.WebGridControls.JQDropDownList)(_gridByItemTargetItem.GridContext as BoundContext).Fields["TargetItem_SlotNumber_Manual"]).SetSelectionValues(rsNamedObject);
                ((FormsFramework.WebGridControls.JQDropDownList)(_gridByItemSourceItem.GridContext as BoundContext).Fields["SlotNumber_Manual"]).SetSelectionValues(rsNamedObject);
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private List<SlotMapDetails> FetchFinalSlotMapGridData(List<SlotMapDetails> oSlotMapDetails)
        {
            SlotMapDetails[] oFinalSlotMapItem = (_gridSlotMapsDetailsField.GridContext as BoundContext).Data as SlotMapDetails[];
            SS_LotCombine_LotItem[] oExistingTargetList = (_gridByItemTargetItem.GridContext as BoundContext).Data as SS_LotCombine_LotItem[];


            if (_SlotAssignmentMethodField.Data.ToString() == "0")
            {
                if (oExistingTargetList != null)
                {
                    foreach (SS_LotCombine_LotItem oSourceItem in oExistingTargetList)
                    {
                        SlotMapForManualAssign(oSourceItem);
                    }
                }

                if ((_gridByItemSourceItem.GridContext as BoundContext).GetSelectedItems(false) != null)
                {
                    foreach (SS_LotCombine_LotItem oSourceItem in (_gridByItemSourceItem.GridContext as BoundContext).GetSelectedItems(false))
                    {
                        SlotMapForManualAssign(oSourceItem);
                    }
                }
            }

            if (oFinalSlotMapItem != null)
            {
                foreach (SlotMapDetails oSourceItem in oFinalSlotMapItem)
                {
                    SlotMapDetails oSlotMapDetail = new SlotMapDetails();
                    oSlotMapDetail.SlotMapItem = new SubentityRef();
                    //oSlotMapDetail.Lot = new ContainerRef();
                    oSlotMapDetail.SlotNumber = oSourceItem.SlotNumber;
                    oSlotMapDetail.WaferScribeNumber = oSourceItem.WaferScribeNumber;
                    oSlotMapDetail.WaferNumber = oSourceItem.WaferNumber;
                    oSlotMapDetail.Status = oSourceItem.Status;
                    oSlotMapDetails.Add(oSlotMapDetail);
                }
            }


            return oSlotMapDetails;
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void SlotMapForManualAssign(SS_LotCombine_LotItem oSourceItem)
        {
            for (int i = 0; i < _gridSlotMapsDetailsField.GridContext.GetTotalRows(); i++)
            {
                string strSlotMapsDetailsRowID = i.ToString().PadLeft(6, '0');
                if (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapsDetailsRowID, "SlotNumber").ToString() == oSourceItem.SlotNumber_Manual)
                {
                    if (_gridSlotMapsDetailsField.GridContext.GetCell(strSlotMapsDetailsRowID, "Status").ToString() == "UP")
                    {
                        _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapsDetailsRowID, "WaferNumber", oSourceItem.WaferNumber);
                        if(oSourceItem.ToWaferScribeNumber != "")
                        {
                            _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapsDetailsRowID, "WaferScribeNumber", oSourceItem.ToWaferScribeNumber);
                        }
                        else
                        {
                            _gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapsDetailsRowID, "WaferScribeNumber", oSourceItem.WaferScribeNumber);
                        }
                        //_gridSlotMapsDetailsField.GridContext.SetCell(strSlotMapsDetailsRowID, "Lot", selectedRow.Container.Name);
                        CamstarWebControl.SetRenderToClient(_gridSlotMapsDetailsField);
                    }
                    else
                    {
                        throw new Exception("Slot number " + oSourceItem.SlotNumber_Manual + " is either occupied or the status is down.");
                    }

                }
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void IsAllSlotNumberSelected()
        {

            if (_ndoCarrier.Data != null)
            {
                SS_LotCombine_LotItem[] oExistingTargetList = (_gridByItemTargetItem.GridContext as BoundContext).Data as SS_LotCombine_LotItem[];

                foreach (SS_LotCombine_LotItem oSourceItem in oExistingTargetList)
                {
                    if (_SlotAssignmentMethodField.Data.ToString() != "0")
                    {
                        if (oSourceItem.SlotNumber == "" || oSourceItem.SlotNumber == null)
                        {
                            throw new Exception("All wafers must be assigned a slot number");
                        }
                    }
                    else
                    {
                        if (oSourceItem.SlotNumber_Manual == "" || oSourceItem.SlotNumber_Manual == null)
                        {
                            throw new Exception("All wafers must be assigned a slot number");
                        }
                    }
                }

                if ((_gridByItemSourceItem.GridContext as BoundContext).GetSelectedItems(false) != null)
                {
                    foreach (SS_LotCombine_LotItem oSourceItem in (_gridByItemSourceItem.GridContext as BoundContext).GetSelectedItems(false))
                    {
                        if (_SlotAssignmentMethodField.Data.ToString() != "0")
                        {
                            if (oSourceItem.SlotNumber == "" || oSourceItem.SlotNumber == null)
                            {
                                throw new Exception("All wafers must be assigned a slot number");
                            }
                        }
                        else
                        {
                            if (oSourceItem.SlotNumber_Manual == "" || oSourceItem.SlotNumber_Manual == null)
                            {
                                throw new Exception("All wafers must be assigned a slot number");
                            }
                        }
                    }
                }
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void IsAllSlotNumberNotDuplicated()
        {
            SS_LotCombine_LotItem[] oExistingTargetList = (_gridByItemTargetItem.GridContext as BoundContext).Data as SS_LotCombine_LotItem[];
            SlotMapDetails[] oSlotMapItem = (_gridSlotMapsDetailsField.GridContext as BoundContext).Data as SlotMapDetails[];
            List<string> ExistingSLotNoList = new List<string>();

            if (_SlotAssignmentMethodField.Data.ToString() == "0" && _ndoCarrier.Data != null)
            {
                foreach (SS_LotCombine_LotItem oSourceItem in oExistingTargetList)
                {
                    if (ExistingSLotNoList.Contains(oSourceItem.SlotNumber_Manual))
                    {
                        throw new Exception("Duplicate slot numbers detected");
                    }
                    ExistingSLotNoList.Add(oSourceItem.SlotNumber_Manual);
                }


                if ((_gridByItemSourceItem.GridContext as BoundContext).GetSelectedItems(false) != null)
                {
                    foreach (SS_LotCombine_LotItem oSourceItem in (_gridByItemSourceItem.GridContext as BoundContext).GetSelectedItems(false))
                    {
                        if (ExistingSLotNoList.Contains(oSourceItem.SlotNumber_Manual))
                        {
                            IsToItemIdSlotNoDuplicated(oSourceItem.ToWaferScribeNumber);
                        }
                        ExistingSLotNoList.Add(oSourceItem.SlotNumber_Manual);
                    }
                }
            }
        }

        private void IsToItemIdSlotNoDuplicated(string ToWaferScribeNumber)
        {
            SS_LotCombine_LotItem[] oExistingTargetList = (_gridByItemTargetItem.GridContext as BoundContext).Data as SS_LotCombine_LotItem[];
            List<string> TargetLotList = new List<string>();

            foreach (SS_LotCombine_LotItem oSourceItem in oExistingTargetList)
            {
                TargetLotList.Add(oSourceItem.WaferScribeNumber);
            }

            if (!TargetLotList.Contains(ToWaferScribeNumber))
            {
                throw new Exception("Duplicate slot numbers detected");
            }

        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void clearAllSlotNumber()
        {
            SS_LotCombine_LotItem[] oExistingTargetList = (_gridByItemTargetItem.GridContext as BoundContext).Data as SS_LotCombine_LotItem[];
            SS_LotCombine_LotItem[] oExistingSourceList = (_gridByItemSourceItem.GridContext as BoundContext).Data as SS_LotCombine_LotItem[];

            if (oExistingSourceList != null)
            {
                foreach (SS_LotCombine_LotItem oSourceItem in oExistingTargetList)
                {
                    oSourceItem.SlotNumber = "";
                    oSourceItem.SlotNumber_Manual = "";
                }
            }

            if (oExistingTargetList != null)
            {
                foreach (SS_LotCombine_LotItem oSourceItem in oExistingSourceList)
                {
                    oSourceItem.SlotNumber = "";
                    oSourceItem.SlotNumber_Manual = "";
                }
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private bool IsAllLotEmptyOfCarrier()
        {
            List<string> LotCarrierList = new List<string>();


            for (int i = 0; i < (_gridContainer.GridContext as BoundContext).GetTotalRows(); i++)
            {
                if ((_gridContainer.GridContext as BoundContext).GetCell(i, "Carrier").ToString() != "")
                {
                    LotCarrierList.Add((_gridContainer.GridContext as BoundContext).GetCell(i, "Carrier").ToString());
                }
            }

            if (LotCarrierList.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private bool IsContainForeignLot()
        {
            if (_SlotAssignmentMethodField.Data.ToString() == "1" || _SlotAssignmentMethodField.Data.ToString() == "2")
            {
                SlotMapDetails[] oSlotMapItem = (_gridSlotMapsDetailsField.GridContext as BoundContext).Data as SlotMapDetails[];
                List<string> ExistingLotList = new List<string>();
                List<string> ExistingLotList2 = new List<string>();
                List<string> NewLotList = new List<string>();
                bool IsContainLot = false;
                int i = 0;


                foreach (System.Web.UI.WebControls.ListItem ExistingLot in _ddlMainLot.DropDownControl.Items)
                {
                    if (ExistingLot != null)
                    {
                        ExistingLotList.Add(_ddlMainLot.DropDownControl.Items[i].Value);
                        ExistingLotList2.Add(_ddlMainLot.DropDownControl.Items[i].Value + "(LOT)");
                        i++;
                    }
                }

                if (oSlotMapItem != null)
                {
                    foreach (SlotMapDetails oSourceItem in oSlotMapItem)
                    {
                        if (oSourceItem.Lot != null && oSourceItem.Lot.Name.ToString() != "")
                        {
                            if (!ExistingLotList.Contains(oSourceItem.Lot.Name.ToString()) && !ExistingLotList2.Contains(oSourceItem.Lot.Name.ToString()))
                            {
                                IsContainLot = true;
                                break;
                            }
                        }
                    }
                }

                return IsContainLot;
            }
            else
            {
                return false;
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private bool IsContainSecondaryLot()
        {
            bool IsContainSecondaryLot = false;

            if (_SlotAssignmentMethodField.Data.ToString() == "1" || _SlotAssignmentMethodField.Data.ToString() == "2")
            {

                for (int i = 0; i < (_gridContainer.GridContext as BoundContext).GetTotalRows(); i++)
                {
                    if ((_gridContainer.GridContext as BoundContext).GetCell(i, "Lot").ToString() != _ddlMainLot.DropDownControl.SelectedValue)
                    {
                        if ((_gridContainer.GridContext as BoundContext).GetCell(i, "Carrier").ToString() != "")
                        {
                            IsContainSecondaryLot = true;
                            break;
                        }
                    }
                }

                return IsContainSecondaryLot;

            }
            else
            {
                return false;
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private bool IsNonMainLotPartiallySelected()
        {
            SS_LotCombine_LotItem[] oFullSourceItem = (_gridByItemSourceItem.GridContext as BoundContext).Data as SS_LotCombine_LotItem[];
            List<SS_LotCombine_LotItem> selectedList = new List<SS_LotCombine_LotItem>();
            List<SS_LotCombine_LotItem> fullList = new List<SS_LotCombine_LotItem>();
            bool IsPartiallySelected = false;

            if (IsContainSecondaryLot() == true)
            {
                if ((_gridByItemSourceItem.GridContext as BoundContext).GetSelectedItems(false) != null)
                {
                    foreach (SS_LotCombine_LotItem oSelectedSourceItem in (_gridByItemSourceItem.GridContext as BoundContext).GetSelectedItems(false))
                    {
                        selectedList.Add(oSelectedSourceItem);
                    }
                }
                if (fullList != null)
                {
                    foreach (SS_LotCombine_LotItem oSourceItem in oFullSourceItem)
                    {
                        fullList.Add(oSourceItem);
                    }
                }

                if (selectedList.Count != fullList.Count)
                {
                    IsPartiallySelected = true;
                }
            }

            return IsPartiallySelected;
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
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

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void AddLotQtyRow(LotCombine oServiceData)
        {
            SS_LotCombine_LotQty[] oExistingList = (_gridByLotSourceLot.GridContext as BoundContext).Data as SS_LotCombine_LotQty[];
            List<SS_LotCombine_LotQty> oNewList = new List<SS_LotCombine_LotQty>();

            if (oExistingList == null)
                oExistingList = new SS_LotCombine_LotQty[0];

            // add back the existing rows
            foreach (SS_LotCombine_LotQty oRow in oExistingList)
            {
                SS_LotCombine_LotQty oCurrentRow = new SS_LotCombine_LotQty();
                oCurrentRow = oRow;
                oNewList.Add(oCurrentRow);
            }

            // add the new row
            SS_LotCombine_LotQty oNewRow = new SS_LotCombine_LotQty();
            oNewRow.FromContainer = oServiceData.Container.Name;
            oNewRow.InProcessQty = oServiceData.MaxInProcessQty.ToString();
            oNewRow.ProcessedQty = oServiceData.MaxProcessedQty.ToString();
            oNewRow.QtyToProcess = oServiceData.MaxQtyToProcess.ToString();
            oNewRow.StandbyQty = oServiceData.MaxStandbyQty.ToString();

            oNewRow.MaxInProcessQty = oServiceData.MaxInProcessQty.ToString();
            oNewRow.MaxProcessedQty = oServiceData.MaxProcessedQty.ToString();
            oNewRow.MaxQtyToProcess = oServiceData.MaxQtyToProcess.ToString();
            oNewRow.MaxStandbyQty = oServiceData.MaxStandbyQty.ToString();

            oNewList.Add(oNewRow);

            (_gridByLotSourceLot.GridContext as BoundContext).Data = oNewList.ToArray();
            _gridByLotSourceLot.BoundContext.LoadData();
            CamstarWebControl.SetRenderToClient(_gridByLotSourceLot);
        } // AddLotQtyRow

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void UpdateLotQtyRow(LotCombine oServiceData)
        {

            // source lot            
            SS_LotCombine_LotQty[] oExistingSourceList = (_gridByLotSourceLot.GridContext as BoundContext).Data as SS_LotCombine_LotQty[];
            List<SS_LotCombine_LotQty> oNewSourceList = new List<SS_LotCombine_LotQty>();

            // add back the existing rows
            foreach (SS_LotCombine_LotQty oRow in oExistingSourceList)
            {
                SS_LotCombine_LotQty oCurrentRow = new SS_LotCombine_LotQty();
                if ((oRow.FromContainer) == oServiceData.Container.Name)
                {
                    oCurrentRow.FromContainer = oServiceData.Container.Name;
                    oCurrentRow.InProcessQty = oServiceData.MaxInProcessQty.ToString();
                    oCurrentRow.ProcessedQty = oServiceData.MaxProcessedQty.ToString();
                    oCurrentRow.QtyToProcess = oServiceData.MaxQtyToProcess.ToString();
                    oCurrentRow.StandbyQty = oServiceData.MaxStandbyQty.ToString();

                    oCurrentRow.MaxInProcessQty = oServiceData.MaxInProcessQty.ToString();
                    oCurrentRow.MaxProcessedQty = oServiceData.MaxProcessedQty.ToString();
                    oCurrentRow.MaxQtyToProcess = oServiceData.MaxQtyToProcess.ToString();
                    oCurrentRow.MaxStandbyQty = oServiceData.MaxStandbyQty.ToString();
                }
                else
                {
                    oCurrentRow = oRow;
                }

                oNewSourceList.Add(oCurrentRow);
            }

            // target lots
            SS_LotCombine_LotQty[] oExistingTargetList = (_gridByLotTargetLot.GridContext as BoundContext).Data as SS_LotCombine_LotQty[];
            List<SS_LotCombine_LotQty> oNewTargetList = new List<SS_LotCombine_LotQty>();

            // add back the existing rows
            foreach (SS_LotCombine_LotQty oRow in oExistingTargetList)
            {
                SS_LotCombine_LotQty oCurrentRow = new SS_LotCombine_LotQty();
                if ((oRow.FromContainer) == oServiceData.Container.Name)
                {
                    oCurrentRow.FromContainer = oServiceData.Container.Name;
                    oCurrentRow.InProcessQty = oServiceData.MaxInProcessQty.ToString();
                    oCurrentRow.ProcessedQty = oServiceData.MaxProcessedQty.ToString();
                    oCurrentRow.QtyToProcess = oServiceData.MaxQtyToProcess.ToString();
                    oCurrentRow.StandbyQty = oServiceData.MaxStandbyQty.ToString();

                    oCurrentRow.MaxInProcessQty = oServiceData.MaxInProcessQty.ToString();
                    oCurrentRow.MaxProcessedQty = oServiceData.MaxProcessedQty.ToString();
                    oCurrentRow.MaxQtyToProcess = oServiceData.MaxQtyToProcess.ToString();
                    oCurrentRow.MaxStandbyQty = oServiceData.MaxStandbyQty.ToString();
                }
                else
                {
                    oCurrentRow = oRow;
                }
                oNewTargetList.Add(oCurrentRow);
            }

            (_gridByLotSourceLot.GridContext as BoundContext).Data = oNewSourceList.ToArray();
            _gridByLotSourceLot.BoundContext.LoadData();
            (_gridByLotTargetLot.GridContext as BoundContext).Data = oNewTargetList.ToArray();
            _gridByLotTargetLot.BoundContext.LoadData();
            CamstarWebControl.SetRenderToClient(_gridByLotSourceLot);
            CamstarWebControl.SetRenderToClient(_gridByLotTargetLot);
        } //UpdateLotQtyRow

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void RemoveLotQtyRow(string sContainer)
        {
            SS_LotCombine_LotQty[] oExistingList = (_gridByLotSourceLot.GridContext as BoundContext).Data as SS_LotCombine_LotQty[];
            List<SS_LotCombine_LotQty> oNewList = new List<SS_LotCombine_LotQty>();

            if (oExistingList == null)
                oExistingList = new SS_LotCombine_LotQty[0];

            // add back the existing rows
            foreach (SS_LotCombine_LotQty oRow in oExistingList)
            {
                if (oRow.FromContainer != sContainer)
                {
                    SS_LotCombine_LotQty oCurrentRow = new SS_LotCombine_LotQty();
                    oCurrentRow = oRow;
                    oNewList.Add(oCurrentRow);
                }
            }

            (_gridByLotSourceLot.GridContext as BoundContext).Data = oNewList.ToArray();
            _gridByLotSourceLot.BoundContext.LoadData();
            CamstarWebControl.SetRenderToClient(_gridByLotSourceLot);
        } // RemoveLotQtyRow

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void AddLotWaferRow(LotWafers[] oWaferData, string sContainer)
        {
            SS_LotCombine_LotItem[] oExistingList = (_gridByItemSourceItem.GridContext as BoundContext).Data as SS_LotCombine_LotItem[];
            SS_LotCombine_LotItem[] oTargetItemList = (_gridByItemTargetItem.GridContext as BoundContext).Data as SS_LotCombine_LotItem[];
            List<SS_LotCombine_LotItem> oNewList = new List<SS_LotCombine_LotItem>();

            if (oExistingList == null)
                oExistingList = new SS_LotCombine_LotItem[0];

            // add back the existing rows
            foreach (SS_LotCombine_LotItem oRow in oExistingList)
            {
                SS_LotCombine_LotItem oCurrentRow = new SS_LotCombine_LotItem();
                oCurrentRow = oRow;
                oNewList.Add(oCurrentRow);
            }

            // add the new row
            foreach (LotWafers oWafer in oWaferData)
            {
                SS_LotCombine_LotItem oNewRow = new SS_LotCombine_LotItem();
                oNewRow.FromContainer = sContainer;
                oNewRow.WaferScribeNumber = oWafer.WaferScribeNumber.ToString();
                oNewRow.ToWaferScribeNumber = "";
                oNewRow.NDPW = oWafer.NDPW != null ? oWafer.NDPW.ToString() : "";
                oNewRow.GoodQty = oWafer.GoodQty != null ? oWafer.GoodQty.ToString() : "";
                oNewRow.MaxNDPW = oWafer.NDPW != null ? oWafer.NDPW.ToString() : "";
                oNewRow.MaxGoodQty = oWafer.GoodQty != null ? oWafer.GoodQty.ToString() : "";
                oNewRow.LotWaferItemId = oWafer.Self.ID;
                oNewRow.WaferNumber = oWafer.WaferNumber != null ? oWafer.WaferNumber.ToString() : "";
                oNewList.Add(oNewRow);
            }

            // bind to the grid
            (_gridByItemSourceItem.GridContext as BoundContext).Data = oNewList.ToArray();
            _gridByItemSourceItem.BoundContext.LoadData();
            CamstarWebControl.SetRenderToClient(_gridByItemSourceItem);
        } //AddLotWaferRow

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void RemoveLotWaferRow(string sContainer)
        {
            SS_LotCombine_LotItem[] oExistingList = (_gridByItemSourceItem.GridContext as BoundContext).Data as SS_LotCombine_LotItem[];
            List<SS_LotCombine_LotItem> oNewList = new List<SS_LotCombine_LotItem>();

            if (oExistingList == null)
                oExistingList = new SS_LotCombine_LotItem[0];

            // add back the existing rows
            foreach (SS_LotCombine_LotItem oRow in oExistingList)
            {
                if (oRow.FromContainer != sContainer)
                {
                    SS_LotCombine_LotItem oCurrentRow = new SS_LotCombine_LotItem();
                    oCurrentRow = oRow;
                    oNewList.Add(oCurrentRow);
                }
            }

            // bind to the grid
            (_gridByItemSourceItem.GridContext as BoundContext).Data = oNewList.ToArray();
            _gridByItemSourceItem.BoundContext.LoadData();
            CamstarWebControl.SetRenderToClient(_gridByItemSourceItem);
        } //RemoveLotWaferRow

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void AssignMainLot_ByItem(string sMainLot)
        {
            SS_LotCombine_LotItem[] oSourceItemList = (_gridByItemSourceItem.GridContext as BoundContext).Data as SS_LotCombine_LotItem[];
            SS_LotCombine_LotItem[] oTargetItemList = (_gridByItemTargetItem.GridContext as BoundContext).Data as SS_LotCombine_LotItem[];

            List<SS_LotCombine_LotItem> oFullItemList = new List<SS_LotCombine_LotItem>();
            List<SS_LotCombine_LotItem> oNewSourceList = new List<SS_LotCombine_LotItem>();
            List<SS_LotCombine_LotItem> oNewTargetList = new List<SS_LotCombine_LotItem>();

            if (oSourceItemList == null)
                oSourceItemList = new SS_LotCombine_LotItem[0];

            if (oTargetItemList == null)
                oTargetItemList = new SS_LotCombine_LotItem[0];

            // merge source and target lot lists
            foreach (SS_LotCombine_LotItem oSourceLot in oSourceItemList)
            {
                SS_LotCombine_LotItem oFullLotData = new SS_LotCombine_LotItem();
                oFullLotData = oSourceLot;
                oFullItemList.Add(oFullLotData);
            }

            foreach (SS_LotCombine_LotItem oTargetLot in oTargetItemList)
            {
                SS_LotCombine_LotItem oFullLotData = new SS_LotCombine_LotItem();
                oFullLotData = oTargetLot;
                oFullItemList.Add(oFullLotData);
            }

            // split out the main lot wafers and source lot wafers
            foreach (SS_LotCombine_LotItem oLotData in oFullItemList.ToArray())
            {
                if (oLotData.FromContainer.ToUpper() == sMainLot.ToUpper())
                {
                    // if its a copied row then ignore
                    if (!oLotData.IsCopy)
                    {
                        SS_LotCombine_LotItem oTargetLotData = new SS_LotCombine_LotItem();
                        oTargetLotData = oLotData;
                        oNewTargetList.Add(oTargetLotData);
                    }
                }
                else
                {
                    SS_LotCombine_LotItem oSourceLotData = new SS_LotCombine_LotItem();
                    oSourceLotData = oLotData;
                    oNewSourceList.Add(oSourceLotData);
                }
            }

            // bind the lists
            _gridByItemSourceItem.ClearData();
            (_gridByItemSourceItem.GridContext as BoundContext).Data = oNewSourceList.ToArray();
            _gridByItemSourceItem.BoundContext.LoadData();

            _gridByItemTargetItem.ClearData();
            (_gridByItemTargetItem.GridContext as BoundContext).Data = oNewTargetList.ToArray();
            _gridByItemTargetItem.BoundContext.LoadData();

            CamstarWebControl.SetRenderToClient(_gridByItemSourceItem);
            CamstarWebControl.SetRenderToClient(_gridByItemTargetItem);
        } // AssignMainLot_ByItem

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void AssignSlotsButton_Click(object sender, EventArgs e)
        {
            try
            {
                //bool isAssigned = false;
                if (_SlotAssignmentMethodField.Data == null || _SlotAssignmentMethodField.Data.ToString() == "0") return;
                string sSlotAssignmentMethod = _SlotAssignmentMethodField.Data.ToString();
                FetchExistingSlotMapGridData();
                SlotMapDetails[] slotMap = (_gridSlotMapsDetailsField.GridContext as BoundContext).Data as SlotMapDetails[];
                serviceAssignment = new scsWaferSlotMappingService(GetAllWafersToAssign(), slotMap);

                if (sSlotAssignmentMethod == "1")
                {
                    serviceAssignment.AssignWafersToSlots("ascall", out mainSlotMapList);
                }
                else if (sSlotAssignmentMethod == "2")
                {
                    serviceAssignment.AssignWafersToSlots("dscall", out mainSlotMapList);
                }
                else if (sSlotAssignmentMethod == "3")
                {
                    serviceAssignment.AssignWafersToSlots("ascfill", out mainSlotMapList);
                }
                else if (sSlotAssignmentMethod == "4")
                {
                    serviceAssignment.AssignWafersToSlots("dscfill", out mainSlotMapList);
                }

                (_gridSlotMapsDetailsField.GridContext as BoundContext).Data = mainSlotMapList.ToArray();
                (_gridSlotMapsDetailsField.GridContext as BoundContext).LoadData();
                CamstarWebControl.SetRenderToClient(_gridSlotMapsDetailsField);

                PopulateSlotNumberIntoGrid();
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        } //AssignSlotsButton_Click

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private List<scsWaferSlotMappingService.AssignementModel> GetAllWafersToAssign()
        {
            List<scsWaferSlotMappingService.AssignementModel> list = new List<scsWaferSlotMappingService.AssignementModel>();
            SS_LotCombine_LotItem[] oSourceItemList = (_gridByItemSourceItem.GridContext as BoundContext).Data as SS_LotCombine_LotItem[];
            SS_LotCombine_LotItem[] oTargetItemList = (_gridByItemTargetItem.GridContext as BoundContext).Data as SS_LotCombine_LotItem[];

            _ndoCarrier_DataChanged(null, null);

            if (_SlotAssignmentMethodField.Data.ToString() == "3" || _SlotAssignmentMethodField.Data.ToString() == "4")
            {
                foreach (SS_LotCombine_LotItem oSourceItem in oTargetItemList)
                {
                    if (oSourceItem.SlotNumber.ToString() == "")
                    {
                        list.Add(new scsWaferSlotMappingService.AssignementModel(
                                    oSourceItem.WaferNumber,
                                    oSourceItem.WaferScribeNumber, ""));
                    }
                }

                if ((_gridByItemSourceItem.GridContext as BoundContext).GetSelectedItems(false) != null)
                {
                    foreach (SS_LotCombine_LotItem oSourceItem in (_gridByItemSourceItem.GridContext as BoundContext).GetSelectedItems(false))
                    {
                        if (oSourceItem.SlotNumber.ToString() == "")
                        {
                            string _waferId = oSourceItem.WaferScribeNumber;
                            if (!oSourceItem.ToWaferScribeNumber.Equals("")) _waferId = oSourceItem.ToWaferScribeNumber;
                            list.Add(new scsWaferSlotMappingService.AssignementModel(
                                     oSourceItem.WaferNumber, _waferId, ""));
                        }
                    }
                }
            }
            else
            {
                foreach (SS_LotCombine_LotItem oSourceItem in oTargetItemList)
                {
                    list.Add(new scsWaferSlotMappingService.AssignementModel(
                                oSourceItem.WaferNumber,
                                oSourceItem.WaferScribeNumber, ""));
                }

                if ((_gridByItemSourceItem.GridContext as BoundContext).GetSelectedItems(false) != null)
                {
                    foreach (SS_LotCombine_LotItem oSourceItem in (_gridByItemSourceItem.GridContext as BoundContext).GetSelectedItems(false))
                    {
                        string _waferId = oSourceItem.WaferScribeNumber;
                        if (!oSourceItem.ToWaferScribeNumber.Equals("")) _waferId = oSourceItem.ToWaferScribeNumber;
                        list.Add(new scsWaferSlotMappingService.AssignementModel(
                                 oSourceItem.WaferNumber, _waferId, ""));
                    }
                }
            }
            return list;
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void PopulateSlotNumberIntoGrid()
        {

            SS_LotCombine_LotItem[] oTargetItemList = (_gridByItemTargetItem.GridContext as BoundContext).Data as SS_LotCombine_LotItem[];
            SS_LotCombine_LotItem[] oSourceItemList = (_gridByItemSourceItem.GridContext as BoundContext).Data as SS_LotCombine_LotItem[];

            foreach (SS_LotCombine_LotItem oTargetItem in oTargetItemList)
            {
                oTargetItem.SlotNumber = GetSlotNumberByWafer(oTargetItem.WaferScribeNumber, oTargetItem.WaferNumber);
            }

            foreach (SS_LotCombine_LotItem oSourceItem in oSourceItemList)
            {
                oSourceItem.SlotNumber = "";
            }
            if ((_gridByItemSourceItem.GridContext as BoundContext).GetSelectedItems(false) != null)
            {
                foreach (SS_LotCombine_LotItem oSourceItem in (_gridByItemSourceItem.GridContext as BoundContext).GetSelectedItems(false))
                {
                    if (oSourceItem.ToWaferScribeNumber.Equals(""))
                        oSourceItem.SlotNumber = GetSlotNumberByWafer(oSourceItem.WaferScribeNumber, oSourceItem.WaferNumber);
                    else
                        oSourceItem.SlotNumber = GetSlotNumberByWafer(oSourceItem.ToWaferScribeNumber, oSourceItem.WaferNumber);
                }
            }


            Array selectedRowIDs = _gridByItemSourceItem.SelectedRowIDs;

            // bind the lists
            _gridByItemTargetItem.ClearData();
            (_gridByItemTargetItem.GridContext as BoundContext).Data = oTargetItemList.ToArray();
            _gridByItemTargetItem.BoundContext.LoadData();
            CamstarWebControl.SetRenderToClient(_gridByItemTargetItem);

            _gridByItemSourceItem.ClearData();
            (_gridByItemSourceItem.GridContext as BoundContext).Data = oSourceItemList.ToArray();
            _gridByItemSourceItem.BoundContext.LoadData();
            CamstarWebControl.SetRenderToClient(_gridByItemSourceItem);

            //select the selected source wafer in the source item grid
            if (selectedRowIDs != null)
            {
                foreach (string sRowID in selectedRowIDs)
                {
                    _gridByItemSourceItem.GridContext.SelectRow(sRowID, true);
                }
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private string GetSlotNumberByWafer(string waferId, string waferNumber)
        {
            foreach (SlotMapDetails slotmapList in (_gridSlotMapsDetailsField.GridContext as BoundContext).Data as SlotMapDetails[])
            {
                if (slotmapList.WaferScribeNumber == null || slotmapList.WaferNumber == null || slotmapList.WaferScribeNumber.Equals("")) continue;
                if (slotmapList.WaferScribeNumber.ToString().Equals(waferId) &&
                    slotmapList.WaferNumber.Value.ToString().Equals(waferNumber))
                    return slotmapList.SlotNumber.ToString();
            }
            return "";
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void NewLotIdControls_DataChanged()
        {
            if (_txtNewContainerName.Data != null || _chkAutoSetNewName.CheckControl.Checked == true)
                AssignMainLot_ByItem("");
            else
                AssignMainLot_ByItem(_ddlMainLot.DropDownControl.SelectedValue);
        } // NewLotIdControls_DataChanged

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void ContainerGrid_RowDelete()
        {
            // get the container that was removed
            string sContainer = _txtContainerToDelete.Data.ToString();

            if (sContainer != "")
            {
                // remove the container from the container list
                _ddlMainLot.DropDownControl.Items.Remove(sContainer);
                // call _ddlMainLot_DataChanged
                if (_ddlMainLot.DropDownControl.Items.Count > 0)
                {
                    _ddlMainLot.DropDownControl.SelectedValue = _ddlMainLot.DropDownControl.Items[0].Value;
                    _ddlMainLot_DataChanged(null, null);
                }

                // remove the row from the SourceLot grid
                if (Page.PrimaryServiceType == "LotCombine")
                    RemoveLotQtyRow(sContainer);
                else
                    RemoveLotWaferRow(sContainer);

                _txtContainerToDelete.ClearData();
            }
        } // ContainerGridRowDelete

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Reset")
            {
                CustomReset();
                //Page.ShopfloorReset(sender, e);
            }
            else if (action != null && action.Parameters == "CustomSubmit")
            {
                e.Result = CustomSubmit();
                if (e.Result.IsSuccess)
                    CustomReset();
            }
            else if (action != null && action.Parameters == "Assign Slots")
            {
                CustomReset();
            }
        } // WebPartCustomAction 

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void CustomReset()
        {
            //Page.ClearValues();

            _ndoEquipment.DropDownControl.ClearSelection();
            _ndoProcessType.DropDownControl.ClearSelection();
            _ddlMainLot.DropDownControl.ClearSelection();
            _txtNewContainerName.ClearData();
            _txtComment.ClearData();
            _chkAutoSetNewName.ClearData();
            _ndoEquipment.DropDownControl.Items.Clear();
            _ndoProcessType.DropDownControl.Items.Clear();
            _ddlMainLot.DropDownControl.Items.Clear();
            _ndoWaferHandler.ClearData();
            _rdoRecipe.ClearData();
            _ndoCarrier.ClearData();
            _ndoEmployee.ClearData();

            _gridContainer.ClearData();
            _gridByLotSourceLot.ClearData();
            _gridByLotTargetLot.ClearData();
            _gridByItemSourceItem.ClearData();
            _gridByItemTargetItem.ClearData();
            //_txtContainerToDelete.ClearData();

            _SlotAssignmentMethodField.Data = _SlotAssignmentMethodField.DefaultValue;
            SlotAssignmentMethod();

            _txtSelectionId.Focus();
            //_SlotAssignmentMethodField.DropDownControl.SelectedIndex = 1;
        } // CustomReset

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                Page.ShopfloorReset(null, null);
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public ResultStatus CustomSubmit()
        {
            try
            {
                // Prepare service
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                ResultStatus oServiceResult = new ResultStatus(null, false);
                string sServiceType = "LotCombine";
                sServiceType = Page.PrimaryServiceType.ToString();

                // Run proper constructor. We need to be dynamic with the primary service type
                var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
                // create a request object
                var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
                var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
                var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

                // retriving data dynamically for Flexibility of the page.
                var oServiceData = CreateServiceData(sServiceType);

                List<CombineLotDetails> oDetails = new List<CombineLotDetails>();
                List<CombineLotWafers> oWafers = new List<CombineLotWafers>();
                List<SlotMapDetails> oSlotMapDetails = new List<SlotMapDetails>();


                // collect the containerNames from the dropdownlist
                foreach (System.Web.UI.WebControls.ListItem oItem in _ddlMainLot.DropDownControl.Items)
                {
                    CombineLotDetails oCombineLot = new CombineLotDetails();
                    oCombineLot.FromContainer = new ContainerRef(oItem.Value);
                    oDetails.Add(oCombineLot);
                }

                //SS_LotCombine_LotItem[] oSourceItemList = (_gridByItemSourceItem.GridContext as BoundContext).GetSelectedItems(false) as SS_LotCombine_LotItem[];
                if ((_gridByItemSourceItem.GridContext as BoundContext).GetSelectedItems(false) != null)
                {
                    foreach (SS_LotCombine_LotItem oSourceItem in (_gridByItemSourceItem.GridContext as BoundContext).GetSelectedItems(false))
                    {
                        CombineLotWafers oWafer = new CombineLotWafers();
                        oWafer.LotWafersItem = new SubentityRef();
                        oWafer.LotWafersItem.ID = oSourceItem.LotWaferItemId;
                        oWafer.FromContainer = new ContainerRef(oSourceItem.FromContainer);
                        oWafer.WaferScribeNumber = oSourceItem.WaferScribeNumber;
                        oWafer.ToWaferScribeNumber = oSourceItem.ToWaferScribeNumber;
                        oWafer.NDPW = int.Parse(oSourceItem.NDPW);
                        oWafer.GoodQty = int.Parse(oSourceItem.GoodQty);
                        oWafers.Add(oWafer);
                    }
                }

                //Validation for slot number
                IsAllSlotNumberSelected();
                IsAllSlotNumberNotDuplicated();


                //Fetch data for carrier assign slot service
                oSlotMapDetails = FetchFinalSlotMapGridData(oSlotMapDetails);

                (oServiceData as LotCombine).ComputerName = _txtComputerName.Data != null ? _txtComputerName.Data.ToString() : null;
                (oServiceData as LotCombine).Employee = _ndoEmployee.Data != null ? new NamedObjectRef(_ndoEmployee.Data.ToString()) : null;

                (oServiceData as LotCombine).Container = new ContainerRef(_ddlMainLot.DropDownControl.SelectedValue);

                int totalRow = _gridContainer.TotalRowCount;
                List<ContainerRef> Containers = new List<ContainerRef>();
                for (int i = 0; i < totalRow; i++)
                {
                    string sSelectionId = _gridContainer.GridContext.GetCell(i, "Lot").ToString();
                    Containers.Add(new ContainerRef(sSelectionId));
                }
               (oServiceData as LotCombineByWafers).Containers = Containers.ToArray();

                if (_txtNewContainerName.Data != null)
                {
                    (oServiceData as LotCombine).NewContainerName = _txtNewContainerName.Data.ToString();
                }

                if (_chkAutoSetNewName.CheckControl.Checked)
                {
                    (oServiceData as LotCombine).AutoSetNewContainerName = true;
                }

                if (_ndoEquipment.DropDownControl.SelectedValue != "")
                    (oServiceData as LotCombine).Equipment = new NamedObjectRef(_ndoEquipment.DropDownControl.SelectedValue);

                if (_ndoProcessType.DropDownControl.SelectedValue != "")
                    (oServiceData as LotCombine).ProcessType = new NamedObjectRef(_ndoProcessType.DropDownControl.SelectedValue);

                if (_ndoWaferHandler.Data != null)
                    (oServiceData as LotCombineByWafers).scsWaferHandlerEquipment = new NamedObjectRef(_ndoWaferHandler.Data.ToString());

                if (_ndoCarrier.Data != null)
                    (oServiceData as LotCombine).Carrier = new NamedObjectRef(_ndoCarrier.Data.ToString());

                if (_SlotAssignmentMethodField.Data != null)
                    (oServiceData as LotCombineByWafers).scsSlotAssignmentMethodEnum = new BaseObjectRef(_SlotAssignmentMethodField.Data.ToString());

                if (_rdoRecipe.Data != null)
                    (oServiceData as LotCombineByWafers).Recipe = _rdoRecipe.Data as RevisionedObjectRef;

                (oServiceData as LotCombine).Details = oDetails.ToArray();

                if (Page.PrimaryServiceType == "LotCombineByWafers")
                {
                    (oServiceData as LotCombine).Wafers = oWafers.ToArray();
                    (oServiceData as LotCombineByWafers).SlotMapDetails = oSlotMapDetails.ToArray();
                }

                //Submit Validation
                (oServiceData as LotCombineByWafers).IsHorizon = true;
                (oServiceData as LotCombineByWafers).IsAllLotEmptyOfCarrier = IsAllLotEmptyOfCarrier();
                (oServiceData as LotCombineByWafers).IsContainForeignLot = IsContainForeignLot();
                (oServiceData as LotCombineByWafers).IsNonMainLotPartiallySelected = IsNonMainLotPartiallySelected();
                (oServiceData as LotCombineByWafers).Comments = _txtComment.Data != null ? _txtComment.Data.ToString() : null;

                // init the result object
                Result oResult = new Result();
                ResultStatus oResultStatus = new ResultStatus();
                oResultStatus = (oService as IShopFloorBase).ExecuteTransaction((oServiceData as DCObject));

                return oResultStatus;
            }
            catch (Exception ex)
            {
                return new ResultStatus(ex.Message, false);
            }


        } //CustomSubmit

        protected WSDataCreator dataCreator = new WSDataCreator();
    } // CustomSubmit

    //---------------------------------------------------
    //
    //---------------------------------------------------
    class SS_LotCombine_LotQty
    {
        private string sFromContainer;
        private string iStandbyQty;
        private string iQtyToProcess;
        private string iInProcessQty;
        private string iProcessedQty;
        private string iMaxStandbyQty;
        private string iMaxQtyToProcess;
        private string iMaxInProcessQty;
        private string iMaxProcessedQty;

        public string FromContainer
        {
            get { return sFromContainer; }
            set { sFromContainer = value; }
        }

        public string StandbyQty
        {
            get { return iStandbyQty; }
            set { iStandbyQty = value; }
        }

        public string QtyToProcess
        {
            get { return iQtyToProcess; }
            set { iQtyToProcess = value; }
        }

        public string InProcessQty
        {
            get { return iInProcessQty; }
            set { iInProcessQty = value; }
        }

        public string ProcessedQty
        {
            get { return iProcessedQty; }
            set { iProcessedQty = value; }
        }

        public string MaxStandbyQty
        {
            get { return iMaxStandbyQty; }
            set { iMaxStandbyQty = value; }
        }

        public string MaxQtyToProcess
        {
            get { return iMaxQtyToProcess; }
            set { iMaxQtyToProcess = value; }
        }

        public string MaxInProcessQty
        {
            get { return iMaxInProcessQty; }
            set { iMaxInProcessQty = value; }
        }

        public string MaxProcessedQty
        {
            get { return iMaxProcessedQty; }
            set { iMaxProcessedQty = value; }
        }
    } // SS_LotCombine_LotQty

    //---------------------------------------------------
    //
    //---------------------------------------------------
    class SS_LotCombine_LotItem
    {
        private string sWaferScribeNumber;
        private string sToWaferScribeNumber;
        private string sNDPW;
        private string sGoodQty;
        private string sMaxNDPW;
        private string sMaxGoodQty;
        private string sFromContainer;
        private string sLotWaferItemId;
        private string sSlotNumber;
        private string sSlotNumberManual;
        private string sWaferNumber;
        private bool bIsCopy;

        // constructor
        public SS_LotCombine_LotItem()
        {
            bIsCopy = false;
        }

        public string WaferScribeNumber
        {
            get { return sWaferScribeNumber; }
            set { sWaferScribeNumber = value; }
        }

        public string ToWaferScribeNumber
        {
            get { return sToWaferScribeNumber; }
            set { sToWaferScribeNumber = value; }
        }

        public string NDPW
        {
            get { return sNDPW; }
            set { sNDPW = value; }
        }

        public string GoodQty
        {
            get { return sGoodQty; }
            set { sGoodQty = value; }
        }

        public string MaxNDPW
        {
            get { return sMaxNDPW; }
            set { sMaxNDPW = value; }
        }

        public string MaxGoodQty
        {
            get { return sMaxGoodQty; }
            set { sMaxGoodQty = value; }
        }

        public string FromContainer
        {
            get { return sFromContainer; }
            set { sFromContainer = value; }
        }

        public string LotWaferItemId
        {
            get { return sLotWaferItemId; }
            set { sLotWaferItemId = value; }
        }

        public string SlotNumber
        {
            get { return sSlotNumber; }
            set { sSlotNumber = value; }
        }

        public string SlotNumber_Manual
        {
            get
            {
                if (sSlotNumberManual == null) return sSlotNumberManual;
                return (sSlotNumberManual.ToString() == "") ? null : sSlotNumberManual.PadLeft(3, '0');
            }
            set { sSlotNumberManual = value; }
        }

        public string WaferNumber
        {
            get { return sWaferNumber; }
            set { sWaferNumber = value; }
        }

        public bool IsCopy
        {
            get { return bIsCopy; }
            set { bIsCopy = value; }
        }
    } // SS_LotCombine_LotItem
}




