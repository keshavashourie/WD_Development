/* Copyright 2019 Siemens */
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
/// Summary description for SS_LotSendAhead
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_LotSendAhead: scsShopfloorBase
    {
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("ss_LotSendAhead_SelectionId") as CWC.TextBox; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("ss_LotSendAhead_ComputerName") as CWC.TextBox; } }
        protected CWC.TextBox _txtNewContainerName { get { return Page.FindCamstarControl("ss_LotSendAhead_ToContainerName") as CWC.TextBox; } }
        protected CWC.TextBox _txtContainerToDelete { get { return Page.FindCamstarControl("ss_LotSendAhead_ContainerToDelete") as CWC.TextBox; } }

        protected CWC.TextBox _txtServiceName { get { return Page.FindCamstarControl("ss_LotSendAhead_ServiceName") as CWC.TextBox; } }

        protected CWC.NamedObject _ndoProcessType { get { return Page.FindCamstarControl("ss_LotSendAhead_ProcessType") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEquipment { get { return Page.FindCamstarControl("ss_LotSendAhead_Equipment") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("ss_LotSendAhead_Employee") as CWC.NamedObject; } }

        protected CWC.RevisionedObject _rdoFutureCombineSpec { get { return Page.FindCamstarControl("ss_LotSendAhead_FutureCombineSpec") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoDefaultFutureCombineSpec { get { return Page.FindCamstarControl("ss_LotSendAhead_DefaultFutureCombineSpec") as CWC.RevisionedObject; } }

        protected CWC.DropDownList _ddlFromLot { get { return Page.FindCamstarControl("ss_LotSendAhead_FromLot") as CWC.DropDownList; } }
        protected CWC.DropDownList _ddlMainLot { get { return Page.FindCamstarControl("ss_LotSendAhead_MainLot") as CWC.DropDownList; } }

        protected JQDataGrid _gridContainer { get { return Page.FindCamstarControl("ss_LotSendAhead_ContainerGrid") as JQDataGrid; } }

        protected JQDataGrid _gridByLotSourceLot { get { return Page.FindCamstarControl("ss_LotSendAhead_ByLot_SourceLot") as JQDataGrid; } }

        protected JQDataGrid _gridByItemSourceItem { get { return Page.FindCamstarControl("ss_LotSendAhead_ByItem_SourceItems") as JQDataGrid; } }
        protected JQDataGrid _gridByItemWafers { get { return Page.FindCamstarControl("ss_LotSendAhead_ByItem_Wafers") as JQDataGrid; } }

        protected const string _kProcessTypeViewStateKey = "ss_LotSendAhead_ProcessTypeSelection_ViewStateVariableKey";
        protected const int _kContainersPerBatch = 20;

        protected SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("ss_LotSendAhead_DataEnvelop") as SEMI.AppCode.DataEnvelopControl; } }

        CWC.Button _btnAddWafer { get { return Page.FindCamstarControl("ss_LotSendAhead_AddWaferBtn") as CWC.Button; } }

        protected CWC.TextBox _txtMaxStandbyQty { get { return Page.FindCamstarControl("ss_LotSendAhead_MaxStandbyQty") as CWC.TextBox; } }
        protected CWC.TextBox _txtMaxQtyToProcess { get { return Page.FindCamstarControl("ss_LotSendAhead_MaxQtyToProcess") as CWC.TextBox; } }
        protected CWC.TextBox _txtMaxInProcessQty { get { return Page.FindCamstarControl("ss_LotSendAhead_MaxInProcessQty") as CWC.TextBox; } }
        protected CWC.TextBox _txtMaxProcessedQty { get { return Page.FindCamstarControl("ss_LotSendAhead_MaxProcessedQty") as CWC.TextBox; } } 
                
        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _txtSelectionId.DataChanged += new EventHandler(_txtSelectionId_DataChanged);
            _ddlMainLot.DataChanged += new EventHandler(_ddlMainLot_DataChanged);
            _ddlFromLot.DataChanged += new EventHandler(_ddlFromLot_DataChanged);
            _ndoEquipment.DataChanged += new EventHandler(_ndoEquipment_DataChanged);
            _ndoProcessType.DataChanged += new EventHandler(_ndoProcessType_DataChanged);
            _txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
            _txtServiceName.Data = Page.PrimaryServiceType.ToString();

            _btnAddWafer.Click += new EventHandler(_btnAddWafer_Click);

            if (Page.IsPostBack)        
                // Check if it is a pop up close, get the return result
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {
                    if (_envSelectedLots != null)
                        // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                        if (Page.DataContract.GetValueByName("ss_LotSendAhead_DataEnvelopDM") != null)
                            _envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("ss_LotSendAhead_DataEnvelopDM") as string[];

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
                                    
                                }
                            }
                        }
                        //nullify the containers list
                        _envSelectedLots.SS_ContainersList = null;
                    }
                }
        } // OnLoad        

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _txtSelectionId_DataChanged(object sender, EventArgs e)
        {
            string sContainer = _txtSelectionId.Data.ToString();
            string strSelectedGridId = (_gridContainer.GridContext as BoundContext).GetRowIdByCellValue("Lot", sContainer);
            if (string.IsNullOrEmpty(strSelectedGridId))
                FetchData("SelectionId");

            _txtSelectionId.TextControl.Text = "";
            _txtSelectionId.Focus();
        } // _txtSelectionId_DataChanged
        
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
                    
                    CamstarWebControl.SetRenderToClient(_ndoProcessType);
                    CamstarWebControl.SetRenderToClient(_ndoEquipment);
                    // call the processType_datachanged event
                    _ndoProcessType_DataChanged(null, null);
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
                if (_ndoEquipment.DropDownControl.SelectedValue != "" && Page.PrimaryServiceType != "ss_LotSendAheadByWafers" && _ndoProcessType.DropDownControl.Items.Count > 1)
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
                && Page.PrimaryServiceType != "ss_LotSendAheadByWafers")
            {
                foreach (System.Web.UI.WebControls.ListItem oItem in _ddlMainLot.DropDownControl.Items)
                    FetchData("Equipment", oItem.Value);  
            }
        } // _ndoEquipment_DataChanged
        
        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _ddlFromLot_DataChanged(object sender, EventArgs e)
        {
            if (_ddlFromLot.DropDownControl.SelectedValue != "")
            {
                AddLotWaferRow(_ddlFromLot.DropDownControl.SelectedValue);
            }
        } // _ddlFromLot_DataChanged

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void FetchData(string sEventType, string sContainerName = "")
        {
            // Prepare service
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            ResultStatus oServiceResult = new ResultStatus(null, false);
            string sServiceType = "ss_LotSendAhead";
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
            var oServiceInfo = info as ss_LotSendAhead_Info;
            (oRequest as Request).Info = oServiceInfo;
            
            bool bExecuteResolveSelectionId = false;
            switch (sEventType)
            {
                case "SelectionId":
                    if (sContainerName == "")
                    {
                        bExecuteResolveSelectionId = true;
                        (oServiceData as ss_LotSendAhead).SelectionId = _txtSelectionId.Data.ToString();
                        oServiceInfo.Containers = FieldInfoUtil.RequestValue();
                    }
                    else
                    {
                        (oServiceData as ss_LotSendAhead).Container = new ContainerRef(sContainerName);
                    }

                    if (_ndoProcessType.Data != null)
                        (oServiceData as ss_LotSendAhead).ProcessType = new NamedObjectRef(_ndoProcessType.Data.ToString());

                    if (_ndoEquipment.Data != null)
                        (oServiceData as ss_LotSendAhead).Equipment = new NamedObjectRef(_ndoEquipment.Data.ToString());

                    oServiceInfo.ProcessTypeSelection = FieldInfoUtil.RequestValue();

                    if (sServiceType == "ss_LotSendAheadByWafers")
                    {
                        oServiceInfo.LotWafers = new LotWafers_Info();
                        oServiceInfo.LotWafers.WaferScribeNumber = FieldInfoUtil.RequestValue();
                        oServiceInfo.LotWafers.WaferNumber = FieldInfoUtil.RequestValue();
                        oServiceInfo.LotWafers.NDPW = FieldInfoUtil.RequestValue();
                        oServiceInfo.LotWafers.GoodQty = FieldInfoUtil.RequestValue();
                    }
                    break;

                case "ProcessType":
                    (oServiceData as ss_LotSendAhead).Container = new ContainerRef(sContainerName);
                    (oServiceData as ss_LotSendAhead).ProcessType = new NamedObjectRef(_ndoProcessType.DropDownControl.SelectedValue);
                    oServiceInfo.EquipmentSelection = FieldInfoUtil.RequestValue();                                        
                    break;

                case "Equipment":
                    (oServiceData as ss_LotSendAhead).Container = new ContainerRef(sContainerName);
                    (oServiceData as ss_LotSendAhead).ProcessType = new NamedObjectRef(_ndoProcessType.DropDownControl.SelectedValue);
                    (oServiceData as ss_LotSendAhead).Equipment = new NamedObjectRef(_ndoEquipment.DropDownControl.SelectedValue);                    
                    break;
            }

            oServiceInfo.Container = FieldInfoUtil.RequestValue();

            oServiceInfo.ss_FutureCombineSpec = FieldInfoUtil.RequestSelectionValue();
            oServiceInfo.ss_DefaultFutureCombineSpec = FieldInfoUtil.RequestValue();
            
            if (sServiceType == "ss_LotSendAhead")
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

            if (oResultStatus.IsSuccess)
            {
                string sResolvedContainer = (oResult.Value as ss_LotSendAhead).Container.Name;
                // check if the input container already exists in the grid
                bool bNonExistingContainer = true;

                if ((_gridContainer.GridContext as BoundContext).GetTotalRows() > 0)
                {
                    string strSelectedGridId = (_gridContainer.GridContext as BoundContext).GetRowIdByCellValue("Lot", sResolvedContainer);
                    if (string.IsNullOrEmpty(strSelectedGridId))
                        bNonExistingContainer = true;
                    else
                        bNonExistingContainer = false;
                }

                if (sServiceType == "ss_LotSendAhead")
                {

                    _txtMaxStandbyQty.Data = (oResult.Value as ss_LotSendAhead).MaxStandbyQty.ToString();
                    _txtMaxQtyToProcess.Data = (oResult.Value as ss_LotSendAhead).MaxQtyToProcess.ToString();
                    _txtMaxInProcessQty.Data = (oResult.Value as ss_LotSendAhead).MaxInProcessQty.ToString();
                    _txtMaxProcessedQty.Data = (oResult.Value as ss_LotSendAhead).MaxProcessedQty.ToString();
                }

                if (sEventType == "SelectionId")
                {
                    if (bNonExistingContainer)
                    {
                        // add the ContainerName to the MainLot dropdown
                        _ddlFromLot.DropDownControl.Items.Add(sResolvedContainer);
                        _ddlMainLot.DropDownControl.Items.Add(sResolvedContainer);

                        if (sServiceType == "ss_LotSendAheadByWafers")                        
                            if ((oResult.Value as ss_LotSendAheadByWafers).LotWafers != null)
                                AddLotWafersToViewState((oResult.Value as ss_LotSendAheadByWafers).LotWafers, sResolvedContainer);

                        if (sServiceType != "ss_LotSendAheadByWafers")
                        { 
                            if (bNonExistingContainer)
                                AddLotQtyRow(oResult.Value as ss_LotSendAhead);  
                        } 

                        if ((oResult.Value as ss_LotSendAhead).ProcessTypeSelection != null)
                        {
                            // get the processTypes and store it in ViewState
                            Hashtable htProcessTypesSelection = new Hashtable();
                            if (ViewState[_kProcessTypeViewStateKey] != null)                            
                                htProcessTypesSelection = ViewState[_kProcessTypeViewStateKey] as Hashtable;
                            
                            if (htProcessTypesSelection == null)
                                htProcessTypesSelection = new Hashtable();

                            if (htProcessTypesSelection.ContainsKey(sResolvedContainer))
                                htProcessTypesSelection[sResolvedContainer] = (oResult.Value as ss_LotSendAhead).ProcessTypeSelection;
                            else
                                htProcessTypesSelection.Add(sResolvedContainer, (oResult.Value as ss_LotSendAhead).ProcessTypeSelection);

                            ViewState[_kProcessTypeViewStateKey] = htProcessTypesSelection;
                        }

                        JQDataGrid _gridContainerTemp = Page.FindCamstarControl("ss_LotSendAhead_ContainerGrid") as JQDataGrid;
                        SEMI.AppCode.UIUtility.GetLotQuerySelection(this, sServiceType, sResolvedContainer, false, ref _gridContainerTemp, _gridContainerTemp.ID.ToString(), true);
                    }                                                                                     

                    if (sContainerName == "")
                    {
                        if (_ddlFromLot.DropDownControl.SelectedValue == sResolvedContainer)
                            _ddlFromLot_DataChanged(null, null);

                        if (_ddlMainLot.DropDownControl.SelectedValue == sResolvedContainer)
                            _ddlMainLot_DataChanged(null, null); 

                        // loop through the return containers and recursively call FetchData
                        foreach (ContainerRef oContainer in (oResult.Value as ss_LotSendAhead).Containers) 
                            if (sResolvedContainer != oContainer.Name)
                                FetchData("SelectionId", oContainer.Name);                        
                    }                    
                    
                    if ((oResult.Environment as ss_LotSendAhead_Environment).ss_FutureCombineSpec.SelectionValues != null)
                        _rdoFutureCombineSpec.SetSelectionValues((oResult.Environment as ss_LotSendAhead_Environment).ss_FutureCombineSpec.SelectionValues);

                    if ((oResult.Value as ss_LotSendAhead).ss_DefaultFutureCombineSpec != null)
                        _rdoDefaultFutureCombineSpec.Data = (oResult.Value as ss_LotSendAhead).ss_DefaultFutureCombineSpec;

                }
                else
                {
                    if (sEventType == "ProcessType")
                    {
                        if ((oResult.Value as ss_LotSendAhead).EquipmentSelection != null)
                        {
                            // for each equipment, add to the equipment selection                        
                            foreach (NamedObjectRef oEquipment in (oResult.Value as ss_LotSendAhead).EquipmentSelection)                            
                                _ndoEquipment.DropDownControl.Items.Add(oEquipment.Name);                            
                        }
                        // call the equipment data changed event 
                        _ndoEquipment_DataChanged(null, null);
                    }
                }                
            }
            else
                DisplayMessage(oResultStatus);         
        } // FetchData

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void AddLotWafersToViewState(LotWafers[] oWaferData, string sContainer)
        {
            List<SS_LotSendAhead_LotItem> oNewList = new List<SS_LotSendAhead_LotItem>();

            // add the new row
            foreach (LotWafers oWafer in oWaferData)
            {
                SS_LotSendAhead_LotItem oNewRow = new SS_LotSendAhead_LotItem();
                oNewRow.FromContainer = sContainer;
                oNewRow.WaferScribeNumber = oWafer.WaferScribeNumber.ToString();
                oNewRow.ToWaferScribeNumber = "";
                oNewRow.WaferNumber = oWafer.WaferNumber != null ? oWafer.WaferNumber.ToString() : "";
                oNewRow.NDPW = oWafer.NDPW != null ? oWafer.NDPW.ToString() : "";
                oNewRow.GoodQty = oWafer.GoodQty != null ? oWafer.GoodQty.ToString() : "";
                oNewRow.MaxNDPW = oWafer.NDPW != null ? oWafer.NDPW.ToString() : "";
                oNewRow.MaxGoodQty = oWafer.GoodQty != null ? oWafer.GoodQty.ToString() : "";
                oNewRow.LotWaferItemId = oWafer.Self.ID;
                oNewList.Add(oNewRow);
            }

            ViewState[sContainer] = oNewList;
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void AddLotQtyRow(ss_LotSendAhead oServiceData)
        {
            SS_LotSendAhead_LotQty[] oExistingList = (_gridByLotSourceLot.GridContext as BoundContext).Data as SS_LotSendAhead_LotQty[];
            List<SS_LotSendAhead_LotQty> oNewList = new List<SS_LotSendAhead_LotQty>();            

            if (oExistingList == null)
                oExistingList = new SS_LotSendAhead_LotQty[0];

            // add back the existing rows
            foreach (SS_LotSendAhead_LotQty oRow in oExistingList)
            {
                SS_LotSendAhead_LotQty oCurrentRow = new SS_LotSendAhead_LotQty();              
                oCurrentRow = oRow;
                oNewList.Add(oCurrentRow);
            }

            // add the new row
            SS_LotSendAhead_LotQty oNewRow = new SS_LotSendAhead_LotQty();
            oNewRow.FromContainer = oServiceData.Container.Name;
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
        private void RemoveLotQtyRow(string sContainer)
        {
            SS_LotSendAhead_LotQty[] oExistingList = (_gridByLotSourceLot.GridContext as BoundContext).Data as SS_LotSendAhead_LotQty[];
            List<SS_LotSendAhead_LotQty> oNewList = new List<SS_LotSendAhead_LotQty>();

            if (oExistingList == null)
                oExistingList = new SS_LotSendAhead_LotQty[0];

            // add back the existing rows
            foreach (SS_LotSendAhead_LotQty oRow in oExistingList)
            {
                if (oRow.FromContainer != sContainer)
                {
                    SS_LotSendAhead_LotQty oCurrentRow = new SS_LotSendAhead_LotQty();
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
        private void AddLotWaferRow(string sContainer)
        {
            List<SS_LotSendAhead_LotItem> oNewList = ViewState[sContainer] as List<SS_LotSendAhead_LotItem>;

            if (oNewList != null)
            { 
                // bind to the grid
                (_gridByItemSourceItem.GridContext as BoundContext).Data = oNewList.ToArray();
                _gridByItemSourceItem.BoundContext.LoadData();
                CamstarWebControl.SetRenderToClient(_gridByItemSourceItem);
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void RemoveLotWaferRow(string sContainer)
        {
            if (_ddlFromLot.DropDownControl.SelectedItem.Text == sContainer)
            {
                _gridByItemSourceItem.ClearData();
            }

            SS_LotSendAhead_LotItem[] oExistingList = (_gridByItemWafers.GridContext as BoundContext).Data as SS_LotSendAhead_LotItem[];
            List<SS_LotSendAhead_LotItem> oNewList = new List<SS_LotSendAhead_LotItem>();

            if (oExistingList == null)
                oExistingList = new SS_LotSendAhead_LotItem[0];

            // add back the existing rows
            foreach (SS_LotSendAhead_LotItem oRow in oExistingList)
            {
                if (oRow.FromContainer != sContainer)
                {
                    SS_LotSendAhead_LotItem oCurrentRow = new SS_LotSendAhead_LotItem();
                    oCurrentRow = oRow;
                    oNewList.Add(oCurrentRow);
                }
            }          

            // bind to the grid
            (_gridByItemWafers.GridContext as BoundContext).Data = oNewList.ToArray();
            _gridByItemWafers.BoundContext.LoadData();
            CamstarWebControl.SetRenderToClient(_gridByItemWafers);
        } //RemoveLotWaferRow

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void ContainerGrid_RowDelete()
        {
            // get the container that was removed
            string sContainer = _txtContainerToDelete.Data.ToString();
                        
            if (sContainer != "")
            {
                // remove the row from the SourceLot grid
                if (Page.PrimaryServiceType == "ss_LotSendAhead")
                    RemoveLotQtyRow(sContainer);
                else
                    RemoveLotWaferRow(sContainer);

                if (Page.PrimaryServiceType == "ss_LotSendAheadByWafers")
                {
                    // remove the container from the container list
                    _ddlFromLot.DropDownControl.Items.Remove(sContainer);

                    // call _ddlFromLot_DataChanged
                    if (_ddlFromLot.DropDownControl.Items.Count > 0)
                    {
                        _ddlFromLot.DropDownControl.SelectedValue = _ddlFromLot.DropDownControl.Items[0].Value;
                        _ddlFromLot_DataChanged(null, null);
                    }
                }

                // remove the container from the container list
                _ddlMainLot.DropDownControl.Items.Remove(sContainer);

                // call _ddlMainLot_DataChanged
                if (_ddlMainLot.DropDownControl.Items.Count > 0)
                {
                    _ddlMainLot.DropDownControl.SelectedValue = _ddlMainLot.DropDownControl.Items[0].Value;
                    _ddlMainLot_DataChanged(null, null);
                }

                // Remove view state for the deleted Container
                ViewState.Remove(sContainer);

                _txtContainerToDelete.ClearData();
                
            }            
        } // ContainerGridRowDelete

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void _btnAddWafer_Click(object sender, EventArgs e)
        {
            var oSelectedWafers = (_gridByItemSourceItem.GridContext as BoundContext).GetSelectedItems(true);
            SS_LotSendAhead_LotItem[] oExistingWafers = (_gridByItemWafers.GridContext as BoundContext).Data as SS_LotSendAhead_LotItem[];
            List<SS_LotSendAhead_LotItem> oNewList = new List<SS_LotSendAhead_LotItem>();

            if (oSelectedWafers == null)
                return;

            if (oExistingWafers == null)
                oExistingWafers = new SS_LotSendAhead_LotItem[0];            

            // add back the existing rows
            foreach (SS_LotSendAhead_LotItem oRow in oExistingWafers)
            {
                SS_LotSendAhead_LotItem oCurrentRow = new SS_LotSendAhead_LotItem();
                oCurrentRow = oRow;
                oNewList.Add(oCurrentRow);
            }
             
            foreach (SS_LotSendAhead_LotItem sRow in oSelectedWafers)
            {
                if (oExistingWafers.Length == 0)
                    break;
                
                foreach (SS_LotSendAhead_LotItem eRow in oExistingWafers)
                {
                    // find duplicate by comparing selected wafers to existing wafers
                    if (sRow.LotWaferItemId == eRow.LotWaferItemId)
                    {
                        sRow.IsCopy = false;
                    }
                }
            }

            foreach (SS_LotSendAhead_LotItem oRow in oSelectedWafers)
            {
                if (oRow.IsCopy == true)
                {
                    SS_LotSendAhead_LotItem oCurrentRow = new SS_LotSendAhead_LotItem();
                    oCurrentRow = oRow;
                    oNewList.Add(oCurrentRow);
                }
            }

            // bind to the grid
            (_gridByItemWafers.GridContext as BoundContext).Data = oNewList.ToArray();
            _gridByItemWafers.BoundContext.LoadData();
            CamstarWebControl.SetRenderToClient(_gridByItemSourceItem);
        }

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
                Page.ShopfloorReset(sender, e);
            }
            else if (action != null && action.Parameters == "Submit")
            {                
                e.Result = CustomSubmit();
                if (e.Result.IsSuccess)
                    CustomReset();
            }
        } // WebPartCustomAction 

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void CustomReset()
        {
            Page.ClearValues();

            _ndoEquipment.DropDownControl.ClearSelection();
            _ndoProcessType.DropDownControl.ClearSelection();
            _rdoFutureCombineSpec.DropDownControl.ClearSelection();
            _ddlMainLot.DropDownControl.ClearSelection();
            _ddlFromLot.DropDownControl.ClearSelection();

            _ndoEquipment.DropDownControl.Items.Clear();
            _ndoProcessType.DropDownControl.Items.Clear();
            _rdoFutureCombineSpec.DropDownControl.Items.Clear();
            _ddlMainLot.DropDownControl.Items.Clear();
            _ddlFromLot.DropDownControl.Items.Clear();

            _gridContainer.ClearData();
            _gridByLotSourceLot.ClearData();
            _gridByItemSourceItem.ClearData();
            _gridByItemWafers.ClearData();

            _txtNewContainerName.ClearData();
            _txtContainerToDelete.ClearData();

            _txtSelectionId.Focus();
        } // CustomReset
      
        //---------------------------------------------------
        //
        //---------------------------------------------------
        public ResultStatus CustomSubmit()
        {
            // Prepare service
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            ResultStatus oServiceResult = new ResultStatus(null, false);
            string sServiceType = "ss_LotSendAhead";
            sServiceType = Page.PrimaryServiceType.ToString();

            // Run proper constructor. We need to be dynamic with the primary service type
            var svcType = WCFObject.CreateObjectType(sServiceType + "Service");

            // create a request object
            var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
            var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
            var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

            // retriving data dynamically for Flexibility of the page.
            var oServiceData = CreateServiceData(sServiceType);

            //(oServiceData as LotSplit).Container = new ContainerRef(_txtSelectionId.Data.ToString());
            (oServiceData as ss_LotSendAhead).Container = new ContainerRef(_ddlMainLot.DropDownControl.SelectedValue);

            if (_txtNewContainerName.Data != null)
                (oServiceData as ss_LotSendAhead).ss_ToContainerName = _txtNewContainerName.Data.ToString();

            if (_rdoFutureCombineSpec.Data != null)
                (oServiceData as ss_LotSendAhead).ss_FutureCombineSpec = _rdoFutureCombineSpec.Data as RevisionedObjectRef;

            if (_ndoEquipment.DropDownControl.SelectedValue != "")
                (oServiceData as ss_LotSendAhead).Equipment = new NamedObjectRef(_ndoEquipment.DropDownControl.SelectedValue);

            if (_ndoProcessType.DropDownControl.SelectedValue != "")
                (oServiceData as ss_LotSendAhead).ProcessType = new NamedObjectRef(_ndoProcessType.DropDownControl.SelectedValue);

            (oServiceData as ss_LotSendAhead).ComputerName = _txtComputerName.Data != null ? _txtComputerName.Data.ToString() : null;
            (oServiceData as ss_LotSendAhead).Employee = _ndoEmployee.Data != null ? new NamedObjectRef(_ndoEmployee.Data.ToString()) : null;

            List<ss_LotSendAheadDetails> oDetails = new List<ss_LotSendAheadDetails>();
            int iToContainerQty = 0;

            if (Page.PrimaryServiceType == "ss_LotSendAhead")
            { 
                SS_LotSendAhead_LotQty[] oItems = (_gridByLotSourceLot.GridContext as BoundContext).Data as SS_LotSendAhead_LotQty[];

                if (oItems != null)
                {
                    foreach (SS_LotSendAhead_LotQty oSourceLot in oItems)
                    {
                        ss_LotSendAheadDetails oDetail = new ss_LotSendAheadDetails();
                        oDetail.ss_ToContainerName = new Primitive<string>(_txtNewContainerName.Data != null ? _txtNewContainerName.Data.ToString() : "");
                        oDetail.ss_FromContainer = new ContainerRef(oSourceLot.FromContainer);
                        oDetail.ss_StandbyQty = oSourceLot.StandbyQty != "" ? int.Parse(oSourceLot.StandbyQty) : 0;
                        oDetail.ss_QtyToProcess = oSourceLot.QtyToProcess != "" ? int.Parse(oSourceLot.QtyToProcess) : 0;
                        oDetail.ss_InProcessQty = oSourceLot.InProcessQty != "" ? int.Parse(oSourceLot.InProcessQty) : 0;
                        oDetail.ss_ProcessedQty = oSourceLot.ProcessedQty != "" ? int.Parse(oSourceLot.ProcessedQty) : 0;
                        oDetails.Add(oDetail);
                    }
                }
            }

            if (Page.PrimaryServiceType == "ss_LotSendAheadByWafers")
            {
                List<ss_LotSendAheadWafers> oWafers = new List<ss_LotSendAheadWafers>();
                SS_LotSendAhead_LotItem[] oItems = (_gridByItemWafers.GridContext as BoundContext).Data as SS_LotSendAhead_LotItem[];

                if (oItems != null)
                {
                    if (oItems.Length > 0)
                    {
                        foreach (SS_LotSendAhead_LotItem oItem in oItems)
                        {
                            ss_LotSendAheadWafers oWafer = new ss_LotSendAheadWafers();

                            oWafer.ListItemAction = ListItemAction.Add;
                            oWafer.LotWafersItem = new SubentityRef();
                            oWafer.LotWafersItem.ID = oItem.LotWaferItemId;
                            oWafer.ss_FromContainer = new ContainerRef(oItem.FromContainer);
                            oWafer.ss_WaferScribeNumber = oItem.WaferScribeNumber;
                            oWafer.ss_ToWaferScribeNumber = oItem.ToWaferScribeNumber;
                            oWafer.ss_NDPW = int.Parse(oItem.NDPW);
                            oWafer.ss_GoodQty = int.Parse(oItem.GoodQty);
                            oWafer.ss_WaferNumber = oItem.WaferNumber;
                            oWafers.Add(oWafer);

                            if(oWafer.ss_FromContainer.Name == _ddlMainLot.DropDownControl.SelectedValue)
                            {                                
                                if (oWafer.ss_GoodQty != null)
                                { 
                                    if (oWafer.ss_GoodQty.ToString() != "" && int.Parse(oWafer.ss_GoodQty.ToString()) > 0)
                                        iToContainerQty += int.Parse(oWafer.ss_GoodQty.ToString());
                                    else
                                        iToContainerQty += int.Parse(oWafer.ss_NDPW.ToString());
                                }
                            }
                        }
                    }
                }

                // collect the containerNames from the dropdownlist
                foreach (System.Web.UI.WebControls.ListItem oItem in _ddlMainLot.DropDownControl.Items)
                {
                    ss_LotSendAheadDetails oDetail = new ss_LotSendAheadDetails();
                    oDetail.ss_ToContainerName = new Primitive<string>(_txtNewContainerName.Data != null ? _txtNewContainerName.Data.ToString() : "");
                    oDetail.ss_StandbyQty = iToContainerQty > 0 ? iToContainerQty : oWafers.Count;
                    oDetail.ss_FromContainer = new ContainerRef(oItem.Value);
                    oDetails.Add(oDetail);
                }

                if (oWafers != null)
                    if (oWafers.Count > 0)
                        (oServiceData as ss_LotSendAhead).ss_Wafers = oWafers.ToArray();
            }

            if (oDetails != null)
                if (oDetails.Count > 0)
                    (oServiceData as ss_LotSendAhead).ss_Details = oDetails.ToArray(); 

             // init the result object
            Result oResult = new Result();
            ResultStatus oResultStatus = new ResultStatus();          
            oResultStatus = (oService as IShopFloorBase).ExecuteTransaction((oServiceData as DCObject));
            return oResultStatus;
            
        } // CustomSubmit

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void RemoveSelectedWaferRow()
        {
            var selectedItems = _gridByItemWafers.GridContext.GetSelectedItems(true);
            SS_LotSendAhead_LotItem[] oExistingList = (_gridByItemWafers.GridContext as BoundContext).Data as SS_LotSendAhead_LotItem[];
            List<SS_LotSendAhead_LotItem> oNewList = new List<SS_LotSendAhead_LotItem>();

            if (selectedItems == null)
                return;

            // add back the existing rows
            foreach (SS_LotSendAhead_LotItem oRow in oExistingList)
            {
                bool bToRemove = false;
                foreach (SS_LotSendAhead_LotItem iRow in selectedItems)
                {
                    if (oRow.LotWaferItemId == iRow.LotWaferItemId)
                    {
                        bToRemove = true;
                    }
                }
                if (!bToRemove)
                {
                    SS_LotSendAhead_LotItem oCurrentRow = new SS_LotSendAhead_LotItem();
                    oCurrentRow = oRow;
                    oNewList.Add(oCurrentRow);
                    
                }
            }

            // bind to the grid
            (_gridByItemWafers.GridContext as BoundContext).Data = oNewList.ToArray();
            _gridByItemWafers.BoundContext.LoadData();
            CamstarWebControl.SetRenderToClient(_gridByItemWafers);
        } //RemoveLotWaferRow

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private class SS_LotSendAhead_LotQty
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

            public SS_LotSendAhead_LotQty()
            {
                iStandbyQty = "";
                iQtyToProcess = "";
                iInProcessQty = "";
                iProcessedQty = "";
            }

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
        } // SS_LotSendAhead_LotQty

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private class SS_LotSendAhead_LotItem
        {
            private string sWaferScribeNumber;
            private string sToWaferScribeNumber;
            private string sWaferNumber;
            private string sNDPW;
            private string sGoodQty;
            private string sMaxNDPW;
            private string sMaxGoodQty;
            private string sFromContainer;
            private string sLotWaferItemId;
            private bool bIsCopy;

            // constructor
            public SS_LotSendAhead_LotItem()
            {
                bIsCopy = true;
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

            public string WaferNumber
            {
                get { return sWaferNumber; }
                set { sWaferNumber = value; }
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

            public bool IsCopy
            {
                get { return bIsCopy; }
                set { bIsCopy = value; }
            }
        } // SS_LotSendAhead_LotItem
    }
}



