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
    public class SS_LotCombine: scsShopfloorBase
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
        
        protected CWC.DropDownList _ddlMainLot { get { return Page.FindCamstarControl("LotCombine_MainLot") as CWC.DropDownList; } }
        protected CWC.CheckBox _chkAutoSetNewName { get { return Page.FindCamstarControl("LotCombine_AutoSetNewContainerName") as CWC.CheckBox; } }
        protected JQDataGrid _gridContainer { get { return Page.FindCamstarControl("LotCombine_ContainerGrid") as JQDataGrid; } }

        protected JQDataGrid _gridByLotTargetLot { get { return Page.FindCamstarControl("LotCombine_ByLot_TargetLot") as JQDataGrid; } }
        protected JQDataGrid _gridByLotSourceLot { get { return Page.FindCamstarControl("LotCombine_ByLot_SourceLot") as JQDataGrid; } }

        protected JQDataGrid _gridByItemTargetItem { get { return Page.FindCamstarControl("LotCombine_ByItem_TargetItems") as JQDataGrid; } }
        protected JQDataGrid _gridByItemSourceItem { get { return Page.FindCamstarControl("LotCombine_ByItem_SourceItems") as JQDataGrid; } }

        protected const string _kProcessTypeViewStateKey = "LotCombine_ProcessTypeSelection_ViewStateVariableKey";
        protected const int _kContainersPerBatch = 20;
        
        protected SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("LotCombine_DataEnvelop") as SEMI.AppCode.DataEnvelopControl; } }
                
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
            _txtServiceName.Data = Page.PrimaryServiceType.ToString();

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
                            AssignMainLotEx(_ddlMainLot.DropDownControl.SelectedValue);


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
        } // _txtSelectionId_DataChanged

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
            catch(Exception ex)
            {
                DisplayMessage(new ResultStatus (ex.Message, false));
            }    
        } // FetchData

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
        }

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
                oNewList.Add(oNewRow);
            }

            // bind to the grid
            (_gridByItemSourceItem.GridContext as BoundContext).Data = oNewList.ToArray();
            _gridByItemSourceItem.BoundContext.LoadData();
            CamstarWebControl.SetRenderToClient(_gridByItemSourceItem);
        }

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
        private void AssignMainLotEx(string sMainLot)
        {
            if (Page.PrimaryServiceType == "LotCombine")
                AssignMainLot_ByLot(sMainLot);
            else
                AssignMainLot_ByItem(sMainLot);
        } // AssignMainLotEx

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void AssignMainLot_ByLot(string sMainLot)
        {
            SS_LotCombine_LotQty[] oSourceLotList = (_gridByLotSourceLot.GridContext as BoundContext).Data as SS_LotCombine_LotQty[];
            SS_LotCombine_LotQty[] oTargetLotList = (_gridByLotTargetLot.GridContext as BoundContext).Data as SS_LotCombine_LotQty[];

            List<SS_LotCombine_LotQty> oFullLotList = new List<SS_LotCombine_LotQty>();
            List<SS_LotCombine_LotQty> oNewSourceList = new List<SS_LotCombine_LotQty>();
            List<SS_LotCombine_LotQty> oNewTargetList = new List<SS_LotCombine_LotQty>();

            if (oSourceLotList == null)
                oSourceLotList = new SS_LotCombine_LotQty[0];

            if (oTargetLotList == null)
                oTargetLotList = new SS_LotCombine_LotQty[0];

            // merge source and target lot lists
            foreach (SS_LotCombine_LotQty oSourceLot in oSourceLotList)
            {
                SS_LotCombine_LotQty oFullLotData = new SS_LotCombine_LotQty();
                oFullLotData = oSourceLot;
                oFullLotList.Add(oFullLotData);
            }

            foreach (SS_LotCombine_LotQty oTargetLot in oTargetLotList)
            {
                SS_LotCombine_LotQty oFullLotData = new SS_LotCombine_LotQty();
                oFullLotData = oTargetLot;
                oFullLotList.Add(oFullLotData);
            }

            foreach (SS_LotCombine_LotQty oLotData in oFullLotList.ToArray())
            {
                if (oLotData.FromContainer.ToUpper() == sMainLot.ToUpper())
                {
                    SS_LotCombine_LotQty oTargetLotData = new SS_LotCombine_LotQty();
                    oTargetLotData = oLotData;
                    oNewTargetList.Add(oTargetLotData);
                }
                else
                {
                    SS_LotCombine_LotQty oSourceLotData = new SS_LotCombine_LotQty();
                    oSourceLotData = oLotData;
                    oNewSourceList.Add(oSourceLotData);
                }
            }

            // bind the lists
            _gridByLotSourceLot.ClearData();
            (_gridByLotSourceLot.GridContext as BoundContext).Data = oNewSourceList.ToArray();
            _gridByLotSourceLot.BoundContext.LoadData();

            _gridByLotTargetLot.ClearData();
            (_gridByLotTargetLot.GridContext as BoundContext).Data = oNewTargetList.ToArray();
            _gridByLotTargetLot.BoundContext.LoadData();

            CamstarWebControl.SetRenderToClient(_gridByLotSourceLot);
            CamstarWebControl.SetRenderToClient(_gridByLotTargetLot);
        } // AssignMainLot_ByLot

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
        public void NewLotIdControls_DataChanged()
        {
            if (_txtNewContainerName.Data != null || _chkAutoSetNewName.CheckControl.Checked == true)            
                AssignMainLotEx("");            
            else            
                AssignMainLotEx(_ddlMainLot.DropDownControl.SelectedValue);            
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
                Page.ShopfloorReset(sender, e);
            }
            else if (action != null && action.Parameters == "CustomSubmit")
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
            _ddlMainLot.DropDownControl.ClearSelection();
            _ndoEquipment.DropDownControl.Items.Clear();
            _ndoProcessType.DropDownControl.Items.Clear();
            _ddlMainLot.DropDownControl.Items.Clear();

            _gridContainer.ClearData();
            _gridByLotSourceLot.ClearData();
            _gridByLotTargetLot.ClearData();
            _gridByItemSourceItem.ClearData();
            _gridByItemTargetItem.ClearData();
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

            if (Page.PrimaryServiceType == "LotCombine")
            {
                SS_LotCombine_LotQty[] oTargetList = (_gridByLotTargetLot.GridContext as BoundContext).Data as SS_LotCombine_LotQty[];
                SS_LotCombine_LotQty[] oSourceList = (_gridByLotSourceLot.GridContext as BoundContext).Data as SS_LotCombine_LotQty[];

                if (oTargetList != null)
                {
                    foreach (SS_LotCombine_LotQty oTargetLot in oTargetList)
                    {
                        CombineLotDetails oCombineLot = new CombineLotDetails();
                        oCombineLot.FromContainer = new ContainerRef(oTargetLot.FromContainer);
                        oCombineLot.StandbyQty = int.Parse(oTargetLot.MaxStandbyQty);
                        oCombineLot.QtyToProcess = int.Parse(oTargetLot.MaxQtyToProcess);
                        oCombineLot.InProcessQty = int.Parse(oTargetLot.MaxInProcessQty);
                        oCombineLot.ProcessedQty = int.Parse(oTargetLot.MaxProcessedQty);
                        oDetails.Add(oCombineLot);
                    }
                }

                if (oSourceList != null)
                {
                    foreach (SS_LotCombine_LotQty oSourceLot in oSourceList)
                    {
                        CombineLotDetails oCombineLot = new CombineLotDetails();
                        oCombineLot.FromContainer = new ContainerRef(oSourceLot.FromContainer);
                        oCombineLot.StandbyQty = int.Parse(oSourceLot.StandbyQty);
                        oCombineLot.QtyToProcess = int.Parse(oSourceLot.QtyToProcess);
                        oCombineLot.InProcessQty = int.Parse(oSourceLot.InProcessQty);
                        oCombineLot.ProcessedQty = int.Parse(oSourceLot.ProcessedQty);
                        oDetails.Add(oCombineLot);
                    }
                }
            }
            else // LotCombineByWafers
            {
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
            }

            (oServiceData as LotCombine).ComputerName = _txtComputerName.Data != null ? _txtComputerName.Data.ToString() : null;
            (oServiceData as LotCombine).Employee = _ndoEmployee.Data != null ? new NamedObjectRef(_ndoEmployee.Data.ToString()) : null;
            (oServiceData as LotCombine).Comments = _txtComment.Data != null ? _txtComment.Data.ToString() : null;

            (oServiceData as LotCombine).Container = new ContainerRef(_ddlMainLot.DropDownControl.SelectedValue);

            if (_txtNewContainerName.Data != null)
                (oServiceData as LotCombine).NewContainerName = _txtNewContainerName.Data.ToString();

            if (_chkAutoSetNewName.CheckControl.Checked)
                (oServiceData as LotCombine).AutoSetNewContainerName = true;

            if (_ndoEquipment.DropDownControl.SelectedValue != "")
                (oServiceData as LotCombine).Equipment = new NamedObjectRef(_ndoEquipment.DropDownControl.SelectedValue);

            if (_ndoProcessType.DropDownControl.SelectedValue != "")
                (oServiceData as LotCombine).ProcessType = new NamedObjectRef(_ndoProcessType.DropDownControl.SelectedValue);
          
            (oServiceData as LotCombine).Details = oDetails.ToArray();           

            if (Page.PrimaryServiceType == "LotCombineByWafers")
                (oServiceData as LotCombine).Wafers = oWafers.ToArray();

             // init the result object
            Result oResult = new Result();
            ResultStatus oResultStatus = new ResultStatus();          
            oResultStatus = (oService as IShopFloorBase).ExecuteTransaction((oServiceData as DCObject));
            return oResultStatus;
            
        } // CustomSubmit

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private class SS_LotCombine_LotQty
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
        private class SS_LotCombine_LotItem
        {
            private string sWaferScribeNumber;
            private string sToWaferScribeNumber;
            private string sNDPW;
            private string sGoodQty;
            private string sMaxNDPW;
            private string sMaxGoodQty;
            private string sFromContainer;
            private string sLotWaferItemId;
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

            public bool IsCopy
            {
                get { return bIsCopy; }
                set { bIsCopy = value; }
            }
        } // SS_LotCombine_LotItem
    }
}



