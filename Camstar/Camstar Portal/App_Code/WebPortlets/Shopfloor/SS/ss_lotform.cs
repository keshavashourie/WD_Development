/* Copyright 2021 Siemens */
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
using Camstar.WebPortal.PortalFramework;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_LotForm : scsShopfloorBase
    {
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotForm_SelectionId") as CWC.TextBox; } }
        protected JQDataGrid _gridDetails { get { return Page.FindCamstarControl("LotForm_Details") as JQDataGrid; } }
        protected JQDataGrid _gridWafers { get { return Page.FindCamstarControl("LotForm_Wafers") as JQDataGrid; } }
        protected Button _btnAutoGenerate { get { return Page.FindCamstarControl("AutoGenerateIdButton") as Button; } }
        protected Button _btnCopy { get { return Page.FindCamstarControl("CopyButton") as Button; } }
        protected ContainerListGrid _listgridContainer { get { return Page.FindCamstarControl("LotForm_Container") as ContainerListGrid; } }
        protected CWC.NamedObject _ndoProcessType { get { return Page.FindCamstarControl("LotForm_ProcessType") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEquipment { get { return Page.FindCamstarControl("LotForm_Equipment") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("LotForm_Employee") as CWC.NamedObject; } }
        protected CWC.CheckBox _chkCreateNewSchedule { get { return Page.FindCamstarControl("LotForm_CreateNewSchedule") as CWC.CheckBox; } }
        protected CWC.TextBox _txtNewContainerName { get { return Page.FindCamstarControl("LotForm_NewContainerName") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoCarrier { get { return Page.FindCamstarControl("LotForm_Carrier") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoMfgOrder { get { return Page.FindCamstarControl("LotForm_MfgOrder") as CWC.NamedObject; } }
        protected CWC.DateChooser _dateExpectedStartDate { get { return Page.FindCamstarControl("LotForm_ExpectedStartDate") as CWC.DateChooser; } }
        protected CWC.TextBox _txtCycleTime { get { return Page.FindCamstarControl("LotForm_CycleTime") as CWC.TextBox; } }
        protected CWC.TextBox _txtSalesOrderNumber { get { return Page.FindCamstarControl("LotForm_SalesOrderNumber") as CWC.TextBox; } }
        protected CWC.RevisionedObject _rdoProduct { get { return Page.FindCamstarControl("LotForm_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoProductBOM { get { return Page.FindCamstarControl("LotForm_ProductBOM") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoProcessSpec { get { return Page.FindCamstarControl("LotForm_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.NamedSubentity _ndsFirstWIPStep { get { return Page.FindCamstarControl("LotForm_FirstWIPStep") as CWC.NamedSubentity; } }
        protected CWC.NamedObject _ndoShipToFactory { get { return Page.FindCamstarControl("LotForm_ShipToFactory") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoOwner { get { return Page.FindCamstarControl("LotForm_Owner") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoPriority { get { return Page.FindCamstarControl("LotForm_Priority") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoPackingType { get { return Page.FindCamstarControl("LotForm_PackingType") as CWC.NamedObject; } }
        protected CWC.CheckBox _chkAutoPrepare { get { return Page.FindCamstarControl("LotForm_AutoPrepare") as CWC.CheckBox; } }
        protected CWC.TextBox _txtComments { get { return Page.FindCamstarControl("Shopfloor_Comments") as CWC.TextBox; } }
        protected CWC.TextBox _txtPrimarySvcType { get { return Page.FindCamstarControl("PrimarySvcType") as CWC.TextBox; } }
        protected CWC.TextBox _txtSelectedRowId { get { return Page.FindCamstarControl("SelectedRowId") as CWC.TextBox; } }
        protected DataEnvelopControl _envContainers { get { return Page.FindCamstarControl("ContainerLists") as DataEnvelopControl; } }
        protected DataEnvelopControl _envWaferMapDetails { get { return Page.FindCamstarControl("WaferMapDetails") as DataEnvelopControl; } }
        protected DataEnvelopControl _envDataCollection { get { return Page.FindCamstarControl("EnvDataCollection") as DataEnvelopControl; } }
        protected CWC.DropDownList _ddlMainLot { get { return Page.FindCamstarControl("MainLotSelect") as CWC.DropDownList; } }
        protected CWC.TextBox _txtQuantity { get { return Page.FindCamstarControl("Quantity") as CWC.TextBox; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("LotForm_ComputerName") as CWC.TextBox; } }
        protected CWC.TextBox _txtSelectedLot { get { return Page.FindCamstarControl("SelectedLot") as CWC.TextBox; } }
        protected CWC.Button _btnRefreshWafersAndMainLot { get { return Page.FindCamstarControl("RefreshWafersAndMainLot") as CWC.Button; } }
        private ToggleContainer _toggleComments { get { return Page.FindCamstarControl("CommentToggle") as ToggleContainer; } }
        private int ConstContainersPerBatch = 20;
        Hashtable htWaferMapDetails = new Hashtable();

        //---------------------------------------------------
        // Get Row By Key Value function
        //---------------------------------------------------
        public static bool GetRowByKeyValueInFieldGrid(Camstar.WebPortal.WebPortlets.MatrixWebPart RefPage, ref JQDataGrid TargetGrid, out int RowID, string Key1, string Value1, string Key2 = "", string Value2 = "")
        {
            try
            {
                RowID = -1;
                bool bExisting = false;
                DataTable tempDTable = new DataTable();
                (TargetGrid.GridContext as BoundContext).GenerateFullExcelData(TargetGrid.TotalRowCount, out tempDTable);
                for (int i=0; i<TargetGrid.TotalRowCount; i++)
                {
                    if (string.IsNullOrEmpty(Key2))
                    {
                        if (tempDTable.Rows[i].Field<string>(Key1).ToString() == Value1)
                        {
                            RowID = i;
                            bExisting = true;                            
                        }
                    }
                    else
                    {
                        if (tempDTable.Rows[i].Field<string>(Key1).ToString() == Value1 & tempDTable.Rows[i].Field<string>(Key2).ToString() == Value2)
                        {
                            RowID = i;
                            bExisting = true;
                        }
                    }
                }

                return bExisting;
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.TargetSite.Name + "(): " + Ex.Message);
            }
        }

        //---------------------------------------------------
        // Fetch Data function
        //---------------------------------------------------
        public void FetchData(string EventName, string LotId = "")
        {
            try
            {
                //Initialize the Service Data
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                string ServiceType = this.Page.PrimaryServiceType;
                OM.Service serviceData = CreateServiceData(ServiceType);
                var Svc = new WSDataCreator().CreateService(ServiceType, profile);
                var SvcData = WCFObject.CreateObject(ServiceType) as ICreator;
                var SvcInfo = WCFObject.CreateObject(ServiceType + "_Info") as ICreator;
                var ReqData = WCFObject.CreateObject(ServiceType + "_Request") as ICreator;
                var ResData = WCFObject.CreateObject(ServiceType + "_Result") as ICreator;
                Result result = null;

                if (EventName == "SelectionId")
                {
                    SvcData.SetValue("SelectionId", LotId);
                    SvcInfo.SetValue("SelectionContainer", new Info(true));
                    SvcInfo.SetValue("SelectionIdType", new Info(true));
                    SvcInfo.SetValue("Containers", new Info(true));
                    SvcInfo.SetValue("ProcessTypeSelection", new Info(true));
                    if (_ndoProcessType.Data != null)
                    {
                        if (Page.PrimaryServiceType == "LotForm")
                        {
                            (SvcData as LotForm).ProcessType = new NamedObjectRef();
                            (SvcData as LotForm).ProcessType.Name = _ndoProcessType.Data.ToString();
                        }
                        else if (Page.PrimaryServiceType == "LotFormByWafers")
                        {
                            (SvcData as LotFormByWafers).ProcessType = new NamedObjectRef();
                            (SvcData as LotFormByWafers).ProcessType.Name = _ndoProcessType.Data.ToString();
                        }
                    }
                    if (_ndoEquipment.Data != null)
                    {
                        if (Page.PrimaryServiceType == "LotForm")
                        {
                            (SvcData as LotForm).Equipment = new NamedObjectRef();
                            (SvcData as LotForm).Equipment.Name = _ndoEquipment.Data.ToString();
                        }
                        else if (Page.PrimaryServiceType == "LotFormByWafers")
                        {
                            (SvcData as LotFormByWafers).Equipment = new NamedObjectRef();
                            (SvcData as LotFormByWafers).Equipment.Name = _ndoEquipment.Data.ToString();
                        }
                    }
                    if (Page.PrimaryServiceType == "LotFormByWafers")
                    {
                        SvcInfo.SetValue("LotWafers", new LotWafers_Info());
                        SvcInfo.SetValue("LotWafers.WaferScribeNumber", new Info(true));
                        SvcInfo.SetValue("LotWafers.NDPW", new Info(true));
                        SvcInfo.SetValue("LotWafers.GoodQty", new Info(true));
                        SvcInfo.SetValue("LotWafers.WaferNumber", new Info(true));
                    }
                }
                else if (EventName == "ProcessType")
                {
                    if (Page.PrimaryServiceType == "LotForm")
                    {
                        (SvcData as LotForm).Container = new ContainerRef();
                        (SvcData as LotForm).Container.Name = LotId;
                        (SvcData as LotForm).ProcessType = new NamedObjectRef();
                        (SvcData as LotForm).ProcessType.Name = _ndoProcessType.Data.ToString();
                    }
                    else if (Page.PrimaryServiceType == "LotFormByWafers")
                    {
                        (SvcData as LotFormByWafers).Container = new ContainerRef();
                        (SvcData as LotFormByWafers).Container.Name = LotId;
                        (SvcData as LotFormByWafers).ProcessType = new NamedObjectRef();
                        (SvcData as LotFormByWafers).ProcessType.Name = _ndoProcessType.Data.ToString();
                    }
                    SvcInfo.SetValue("EquipmentSelection", new Info(true));
                }
                else if (EventName == "Equipment")
                {
                    if (Page.PrimaryServiceType == "LotForm")
                    {
                        (SvcData as LotForm).Container = new ContainerRef();
                        (SvcData as LotForm).Container.Name = LotId;
                        (SvcData as LotForm).ProcessType = new NamedObjectRef();
                        (SvcData as LotForm).ProcessType.Name = _ndoProcessType.Data.ToString();
                        (SvcData as LotForm).Equipment = new NamedObjectRef();
                        (SvcData as LotForm).Equipment.Name = _ndoEquipment.Data.ToString();
                    }
                    else if (Page.PrimaryServiceType == "LotFormByWafers")
                    {
                        (SvcData as LotFormByWafers).Container = new ContainerRef();
                        (SvcData as LotFormByWafers).Container.Name = LotId;
                        (SvcData as LotFormByWafers).ProcessType = new NamedObjectRef();
                        (SvcData as LotFormByWafers).ProcessType.Name = _ndoProcessType.Data.ToString();
                        (SvcData as LotFormByWafers).Equipment = new NamedObjectRef();
                        (SvcData as LotFormByWafers).Equipment.Name = _ndoEquipment.Data.ToString();
                    }
                }
                SvcInfo.SetValue("Container", new Info(true));
                if (Page.PrimaryServiceType == "LotForm")
                {
                    SvcInfo.SetValue("MaxStandbyQty", new Info(true));
                    SvcInfo.SetValue("MaxQtyToProcess", new Info(true));
                    SvcInfo.SetValue("MaxInProcessQty", new Info(true));
                    SvcInfo.SetValue("MaxProcessedQty", new Info(true));
                }
                ReqData.SetValue("Info", SvcInfo);
                //Request the data
                ResultStatus Results = (Svc as IShopFloorBase).ResolveSelectionId(SvcData as DCObject, ReqData as Request, out result);
                if (Results.IsSuccess)
                {
                    if (EventName == "SelectionId")
                    {
                        if ((result.Value as LotForm).Containers != null)
                        {
                            foreach (ContainerRef container in (result.Value as LotForm).Containers)
                            {
                                if ((result.Value as LotForm).SelectionContainer.Name == container.Name)
                                {
                                    DisplayLotInfo(container.Name, result);
                                    _txtSelectionId.ClearData();
                                }
                                else
                                {
                                    if (Page.PrimaryServiceType == "LotForm")
                                    {
                                        (SvcData as LotForm).Container = new ContainerRef();
                                        (SvcData as LotForm).Container.Name = container.Name;
                                    }
                                    else if (Page.PrimaryServiceType == "LotFormByWafers")
                                    {
                                        (SvcData as LotFormByWafers).Container = new ContainerRef();
                                        (SvcData as LotFormByWafers).Container.Name = container.Name;
                                    }
                                    if (Page.PrimaryServiceType == "LotForm")
                                    {
                                        SvcInfo.SetValue("MaxStandbyQty", new Info(true));
                                        SvcInfo.SetValue("MaxQtyToProcess", new Info(true));
                                        SvcInfo.SetValue("MaxInProcessQty", new Info(true));
                                        SvcInfo.SetValue("MaxProcessedQty", new Info(true));
                                    }
                                    else
                                    {
                                        SvcInfo.SetValue("LotWafers", new LotWafers_Info());
                                        //SvcInfo.SetValue("LotWafers.InstanceID", new Info(true));
                                        SvcInfo.SetValue("LotWafers.WaferScribeNumber", new Info(true));
                                        SvcInfo.SetValue("LotWafers.NDPW", new Info(true));
                                        SvcInfo.SetValue("LotWafers.GoodQty", new Info(true));
                                        SvcInfo.SetValue("LotWafers.WaferNumber", new Info(true));
                                    }
                                    SvcInfo.SetValue("Container", new Info(true));
                                    ReqData.SetValue("Info", SvcInfo);
                                    ResultStatus Results2 = Svc.GetEnvironment(SvcData as DCObject, ReqData as Request, out result);
                                    if (Results2.IsSuccess)
                                    {
                                        DisplayLotInfo(container.Name, result);
                                        //_txtSelectionId.ClearData();
                                    }
                                }
                            }
                        }
                        if ((result.Value as LotForm).ProcessTypeSelection != null && LotId == _ddlMainLot.Data.ToString())
                        {
                            CWC.NamedObject _ndoProcessTypeTemp = _ndoProcessType;
                            SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoProcessTypeTemp, (result.Value as LotForm).ProcessTypeSelection);
                            _ndoProcessType.Data = (result.Value as LotForm).ProcessTypeSelection[0];
                        }
                    } //EventName == "SelectionId"
                    else
                    {
                        if (Page.PrimaryServiceType == "LotForm")
                        {
                            int detailsCount = _gridDetails.TotalRowCount;
                            for (int i = 0; i < detailsCount; i++)
                            {
                                string currentIndex = i.ToString("000000");
                                string currentLot = _gridDetails.GridContext.GetCell(currentIndex, "Lot").ToString();
                                if (currentLot == (result.Value as LotForm).Container.Name)
                                {
                                    _gridDetails.GridContext.SetCell(currentIndex, "StandbyQty", (result.Value as LotForm).MaxStandbyQty.ToString());
                                    _gridDetails.GridContext.SetCell(currentIndex, "QtyToProcess", (result.Value as LotForm).MaxQtyToProcess.ToString());
                                    _gridDetails.GridContext.SetCell(currentIndex, "InProcessQty", (result.Value as LotForm).MaxInProcessQty.ToString());
                                    _gridDetails.GridContext.SetCell(currentIndex, "ProcessedQty", (result.Value as LotForm).MaxProcessedQty.ToString());
                                }
                            }
                            CamstarWebControl.SetRenderToClient(_gridDetails);
                        }
                        if (EventName == "ProcessType")
                        {
                            CWC.NamedObject _ndoEquipmentTemp = _ndoEquipment;
                            if ((result.Value as LotForm).EquipmentSelection != null)
                            {
                                SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoEquipmentTemp, (result.Value as LotForm).EquipmentSelection);
                                _ndoEquipment.Data = (result.Value as LotForm).EquipmentSelection[0];
                            }
                        }
                    }
                }
                else
                {
                    this.DisplayMessage(Results);
                    _txtSelectionId.ClearData();
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //---------------------------------------------------
        // Display Lot Info
        //---------------------------------------------------
        public void DisplayLotInfo(string LotId, Result ResponseData)
        {
            try
            {
                JQDataGrid theGrid = Page.FindCamstarControl("LotForm_Details") as JQDataGrid;
                int iExistingRow = -1;
                bool bExistingLot = GetRowByKeyValueInFieldGrid(this, ref theGrid, out iExistingRow, "Lot", (ResponseData.Value as LotForm).Container.Name);
                if (!bExistingLot)
                {
                    _ddlMainLot.DropDownControl.Items.Add(new System.Web.UI.WebControls.ListItem() { Text = LotId, Value = LotId });
					RecordSet rs;
                    DataTable existingDataTable = new DataTable();
					rs = SEMI.AppCode.UIUtility.GetLotQuerySelection(this, this.PrimaryServiceType, LotId);
                    DataTable containersDataTable = rs.GetAsExplicitlyDataTable();
                    
                    if (theGrid.TotalRowCount > 0)                    
                        (theGrid.GridContext as BoundContext).GenerateFullExcelData(theGrid.TotalRowCount, out existingDataTable);                    

                    if (existingDataTable.Columns.Contains("_leftSelector_column"))
                        existingDataTable.Columns.Remove("_leftSelector_column");

                    if (existingDataTable.Columns.Count == 0)                    
                        foreach (DataColumn colname in containersDataTable.Columns)                        
                            existingDataTable.Columns.Add(colname.ColumnName, colname.DataType);                                            

                    string[] _TextBoxColumns = null;
                    if (Page.PrimaryServiceType == "LotForm")
                    {
                        containersDataTable.Rows[0].SetField<string>("StandbyQty", (ResponseData.Value as LotForm).MaxStandbyQty.ToString());
                        containersDataTable.Rows[0].SetField<string>("QtyToProcess", (ResponseData.Value as LotForm).MaxQtyToProcess.ToString());
                        containersDataTable.Rows[0].SetField<string>("InProcessQty", (ResponseData.Value as LotForm).MaxInProcessQty.ToString());
                        containersDataTable.Rows[0].SetField<string>("ProcessedQty", (ResponseData.Value as LotForm).MaxProcessedQty.ToString());

                        _TextBoxColumns = new string[4] { "StandbyQty", "QtyToProcess", "InProcessQty", "ProcessedQty" };
                    }
                    
                    foreach (DataRow dr in containersDataTable.Rows)                    
                        existingDataTable.Rows.Add(dr.ItemArray);                    
                    
                    theGrid.ClearData();

                    SEMI.AppCode.GridUtility.ItemListGrid_SetColumns(this, existingDataTable, theGrid.ID, _TextBoxColumns, "RefTargetGrid", false, null, true, null, rs.Headers);
                    SEMI.AppCode.GridUtility.ItemListGrid_BindDataTable(this, existingDataTable, ref theGrid);

                    if (Page.PrimaryServiceType != "LotForm")
                    {
                        if ((ResponseData.Value as LotForm).LotWafers != null)
                        {
                            var newLotWafers = new List<CombineLotWafers>();
                            if (_gridWafers.TotalRowCount > 0)                            
                                newLotWafers.AddRange(_gridWafers.Data as CombineLotWafers[]);
                            
                            for (int i = 0; i < (ResponseData.Value as LotForm).LotWafers.Count(); i++)
                            {
                                newLotWafers.Add(new CombineLotWafers
                                {
                                    FromContainer = new ContainerRef() { Name = LotId },
                                    WaferScribeNumber = (ResponseData.Value as LotForm).LotWafers[i].WaferScribeNumber,
                                    NDPW = Convert.ToInt32((ResponseData.Value as LotForm).LotWafers[i].NDPW.ToString()),
                                    GoodQty = Convert.ToInt32((ResponseData.Value as LotForm).LotWafers[i].GoodQty.ToString()),
                                    ToWaferNumber = (ResponseData.Value as LotForm).LotWafers[i].WaferNumber
                                });
                            }

                            string[] sSelectedRowIDs = _gridWafers.SelectedRowIDs as string[];                            

                            _gridWafers.ClearData();
                            _gridWafers.Data = newLotWafers.ToArray();
                            _gridWafers.OriginalData = newLotWafers.ToArray();

                            if (sSelectedRowIDs != null)
                                foreach (string sRowID in sSelectedRowIDs)
                                    _gridWafers.Action_SelectRow(sRowID, "select");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Refresh wafers grid & Main Lot dropdown
        //-----------------------------------------
        public void RefreshWafersAndMainLot(string Lot)
        {
            _ddlMainLot.DropDownControl.Items.Remove(Lot);
            CamstarWebControl.SetRenderToClient(_ddlMainLot);
            MainLotField_DataChanged(null, null);
            if (Page.PrimaryServiceType == "LotFormByWafers")
            {
                int lotCount = _gridWafers.TotalRowCount;
                CombineLotWafers[] newWafers = new CombineLotWafers[lotCount];
                newWafers = _gridWafers.Data as CombineLotWafers[];
                List<CombineLotWafers> newWafersList = newWafers.ToList();
                for (int i = 0; i < lotCount; i++)
                {
                    if (newWafersList[i].FromContainer.Name == Lot)
                    {
                        newWafersList.RemoveAt(i);
                        lotCount--;
                        i--;
                    }
                }
                _gridWafers.ClearData();
                _gridWafers.Data = newWafersList.ToArray();
            }
        }

        //-----------------------------------------
        // Selection Id Data Changed Event
        //-----------------------------------------
        public void SelectionIdField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (_txtSelectionId.Data != null)
                {
                    Page.StatusBar.ClearMessage();
                    JQDataGrid detailsGrid = Page.FindCamstarControl("LotForm_Details") as JQDataGrid;
                    int iExistingRow = -1;
                    bool bExistingLot = GetRowByKeyValueInFieldGrid(this, ref detailsGrid, out iExistingRow, "Lot", _txtSelectionId.Data.ToString());
                    bool bExistingWafer = false;

                    if (Page.PrimaryServiceType == "LotFormByWafers")
                    {
                        // check if the entered selectionID matches any existing wafers
                        iExistingRow = -1;
                        JQDataGrid wafersGrid = Page.FindCamstarControl("LotForm_Wafers") as JQDataGrid;
                        bExistingWafer = GetRowByKeyValueInFieldGrid(this, ref wafersGrid, out iExistingRow, "WaferScribeNumber", _txtSelectionId.Data.ToString());
                    }

                    if (!bExistingLot && !bExistingWafer)
                    {
                        // fetch the lot based on the selectionID
                        string sSelectionID = _txtSelectionId.Data.ToString();
                        FetchData("SelectionId", sSelectionID);
                        // recheck to see if the entered selectionID matches any newly retrieved wafers
                        if (Page.PrimaryServiceType == "LotFormByWafers")
                        {                            
                            iExistingRow = -1;
                            JQDataGrid wafersGrid = Page.FindCamstarControl("LotForm_Wafers") as JQDataGrid;
                            bExistingWafer = GetRowByKeyValueInFieldGrid(this, ref wafersGrid, out iExistingRow, "WaferScribeNumber", sSelectionID);
                        }
                    }

                    if (bExistingWafer && iExistingRow >= 0)
                    {
                        string sRowId = iExistingRow.ToString().PadLeft(6, '0');
                        _gridWafers.Action_SelectRow(sRowId, "select");                        
                    }                

                    _txtSelectionId.ClearData();
                    _txtSelectionId.Focus();
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Main Lot Data Changed Event
        //-----------------------------------------
        public void MainLotField_DataChanged(object sender, EventArgs e)
        {
            if (_ddlMainLot.DropDownControl.Items.Count > 0)
            {
                FetchData("SelectionId", _ddlMainLot.Data.ToString());
                ProcessTypeField_DataChanged(sender, e);
            }
        }

        //-----------------------------------------
        // Process Type Data Changed Event
        //-----------------------------------------
        public void ProcessTypeField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                _ndoEquipment.ClearData();
                _ndoEquipment.ClearSelectionValues();
                if (_ddlMainLot.Data != null && _ndoProcessType.Data != null)
                {
                    FetchData("ProcessType", _ddlMainLot.Data.ToString());
                    if (_ndoEquipment.Data != null && Page.PrimaryServiceType == "LotForm" && _ndoProcessType.DropDownControl.Items.Count > 1)
                    {
                        foreach (ListItem item in _ddlMainLot.DropDownControl.Items)
                        {
                            FetchData("Equipment", item.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Equipment Field Data Changed Event
        //-----------------------------------------
        public void EquipmentField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (_ddlMainLot.Data != null && _ndoProcessType.Data != null && _ndoEquipment.Data != null && Page.PrimaryServiceType == "LotForm")
                {
                    foreach (ListItem item in _ddlMainLot.DropDownControl.Items)
                    {
                        FetchData("Equipment", item.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Details Grid Row Selected Event
        //-----------------------------------------
        protected ResponseData DetailsGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            if (args.Context.SelectedRowID != null)
            {
                string selectedLot = _gridDetails.GridContext.GetCell(args.Context.SelectedRowID, "Lot").ToString();
                _txtSelectedLot.Data = selectedLot;
            }
            return null;
        } // DetailsGrid_RowDeleting(object sender, JQGridEventArgs args)

        //---------------------------------------------------
        // Override OnLoad event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                _txtSelectionId.DataChanged += new EventHandler(SelectionIdField_DataChanged);
                _ddlMainLot.DataChanged += new EventHandler(MainLotField_DataChanged);
                _ndoProcessType.DataChanged += new EventHandler(ProcessTypeField_DataChanged);
                _ndoEquipment.DataChanged += new EventHandler(EquipmentField_DataChanged);
                _gridDetails.RowSelected += new JQGridEventHandler(DetailsGrid_RowSelected);                
                _btnRefreshWafersAndMainLot.Hidden = true;

                _txtPrimarySvcType.Data = Page.PrimaryServiceType.ToString();
                _txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
               
                if (Page.PrimaryServiceType == "LotForm")
                {
                    _gridWafers.Visible = false;
                    _btnAutoGenerate.Visible = false;
                    _btnCopy.Visible = false;
                }
                else
                {
                    _gridWafers.Visible = true;
                    _btnAutoGenerate.Visible = true;
                    _btnCopy.Visible = true;
                }

                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                    OnPopupClose();
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // On Popup closed event
        //-----------------------------------------
        public void OnPopupClose()
        {
            try
            {
                Page.CollectDataContract();
                if (Page.DataContract.GetValueByName("LotForm_LotList_DM") != null)
                {
                    string[] sContainers = Page.DataContract.GetValueByName("LotForm_LotList_DM") as string[];
                    _envContainers.SS_ContainersList = null;
                    Page.DataContract.SetValueByName("LotForm_LotList_DM", null);
                    foreach (string container in sContainers)
                        FetchData("SelectionId", container);
                }
                if (Page.DataContract.GetValueByName("LotForm_WaferMapDetails_DM") != null)
                {
                    //Bind the returned data to the data envelope control & grid
                    _envDataCollection.SS_CombineLotWafers[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].WaferMapDetails = Page.DataContract.GetValueByName("LotForm_WaferMapDetails_DM") as WaferMapDetails[];
                    _gridWafers.ClearData();
                    _gridWafers.Data = _envDataCollection.SS_CombineLotWafers;
                    _gridWafers.OriginalData = _envDataCollection.SS_CombineLotWafers;
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // On Wafer Map Details
        //-----------------------------------------
        public virtual void PopupWaferMapDetails(bool EndResponse = false)
        {
            try
            {
                _envDataCollection.SS_CombineLotWafers = _gridWafers.Data as CombineLotWafers[];
                _txtSelectedRowId.Data = Page.DataContract.GetValueByName("LotForm_GridRowId_DM").ToString();
                if (_envDataCollection.SS_CombineLotWafers.Count() > Convert.ToInt32(_txtSelectedRowId.Data.ToString()))
                {
                    if (_envDataCollection.SS_CombineLotWafers[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].WaferMapDetails != null)
                    {
                        _envWaferMapDetails.SS_WaferMapDetails = _envDataCollection.SS_CombineLotWafers[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].WaferMapDetails;
                        Page.DataContract.SetValueByName("LotForm_WaferMapDetails_DM", _envDataCollection.SS_CombineLotWafers[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].WaferMapDetails);
                    }
                    else
                    {
                        _envWaferMapDetails.SS_WaferMapDetails = null;
                        Page.DataContract.SetValueByName("LotForm_WaferMapDetails_DM", null);
                    }
                }

                int iQty = 0;
                if (Page.DataContract.GetValueByName("LotForm_GridGoodQty_DM") != null)
                {
                    iQty = Convert.ToInt32(Page.DataContract.GetValueByName("LotForm_GridGoodQty_DM").ToString());
                }
                if (iQty == 0)
                {
                    if (Page.DataContract.GetValueByName("LotForm_GridNDPW_DM") != null)
                    {
                        iQty = Convert.ToInt32(Page.DataContract.GetValueByName("LotForm_GridNDPW_DM").ToString());
                    }
                }
                if (iQty > 0)
                {
                    _txtQuantity.Data = iQty;
                }
                Page.CollectDataContractByName("LotForm_Quantity_DM");
                
                Camstar.WebPortal.Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                objAction.PageName = "SS_WaferMapDetailsPopupVP";
                
                UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[4];
                objLinks[0] = new UIComponentDataContractLink();
                objLinks[0].SourceMember = "LotForm_PrimaryServiceType_DM";
                objLinks[0].TargetMember = "Popup_PrimarySvcType_DM";
                objLinks[1] = new UIComponentDataContractLink();
                objLinks[1].SourceMember = "LotForm_Quantity_DM";
                objLinks[1].TargetMember = "Popup_Quantity_DM";
                objLinks[2] = new UIComponentDataContractLink();
                objLinks[2].SourceMember = "LotForm_WaferMapDetails_DM";
                objLinks[2].TargetMember = "Popup_WaferMapDetails_DM";
                objLinks[3] = new UIComponentDataContractLink();
                objLinks[3].SourceMember = "LotForm_GridItemId_DM";
                objLinks[3].TargetMember = "Popup_SelectedItem_DM";
                
                UIComponentDataContractReturnLink[] objReturnLinks = new UIComponentDataContractReturnLink[1];
                objReturnLinks[0] = new UIComponentDataContractReturnLink();
                objReturnLinks[0].SourceMember = "Popup_WaferMapDetails_DM";
                objReturnLinks[0].TargetMember = "LotForm_WaferMapDetails_DM";
                objAction.DataContractMap = new UIComponentDataContractMap();
                objAction.DataContractMap.Links = objLinks;
                objAction.DataContractReturnMap = new UIComponentDataContractReturnMap();
                objAction.DataContractReturnMap.ReturnLinks = objReturnLinks;
                objAction.FrameLocation = new UIFloatingPageLocation();
                objAction.FrameLocation.Width = 850;
                objAction.FrameLocation.Height = 600;
                objAction.EndResponse = false;

                this.Page.ActionDispatcher.ExecuteAction(objAction);
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Copy function
        //-----------------------------------------
        public void CopyButton()
        {
            try
            {
                if (_gridWafers.SelectedRowID != null)
                {
                    int newIndex = Convert.ToInt32(_gridWafers.SelectedRowID.ToString()) + 1;
                    var newLotWafers = new List<CombineLotWafers>();
                    if (_gridWafers.TotalRowCount > 0)
                    {
                        newLotWafers.AddRange(_gridWafers.Data as CombineLotWafers[]);
                    }
                    newLotWafers.Insert(newIndex, new CombineLotWafers
                    {
                        FromContainer = newLotWafers[newIndex-1].FromContainer,
                        WaferScribeNumber = newLotWafers[newIndex-1].WaferScribeNumber,
                        NDPW = 0,
                        GoodQty =0,
                        ToWaferNumber = newLotWafers[newIndex-1].ToWaferNumber
                    });
                    _gridWafers.ClearData();
                    _gridWafers.Data = newLotWafers.ToArray();
                    _gridWafers.OriginalData = newLotWafers.ToArray();
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Auto Generate To Item Id function
        //-----------------------------------------
        public void AutoGenerateToItemId()
        {
            try
            {
                if (_txtNewContainerName.Data != null)
                {
                    int iCount = 0;
                    List<string> selectedIDs = _gridWafers.GridContext.SelectedRowIDs;
                    if (selectedIDs != null)
                    {
                        selectedIDs.Sort();
                        for (int i = 0; i < _gridWafers.TotalRowCount; i++)
                        {
                            string rowId = string.Format("{0:000000}", i);
                            _gridWafers.GridContext.SetCell(rowId, "ToWaferScribeNumber", "");
                        }
                        foreach (string selectedID in selectedIDs)
                        {
                            iCount = iCount + 1;
                            string newValue = _txtNewContainerName.Data.ToString() + "-" + string.Format("{0:00}", iCount);
                            _gridWafers.GridContext.SetCell(selectedID, "ToWaferScribeNumber", newValue);
                        }
                    }
                    CamstarWebControl.SetRenderToClient(_gridWafers);
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void CustomReset()
        {
            _txtSelectionId.ClearData();
            _ndoEmployee.ClearData();
            _gridDetails.ClearData();
            _gridWafers.ClearData();
            _ddlMainLot.ClearSelectionValues();
            _ddlMainLot.ClearData();
            _ndoProcessType.ClearData();
            _ndoEquipment.ClearData();
            _chkCreateNewSchedule.CheckControl.Checked = false;
            _txtNewContainerName.ClearData();
            _ndoCarrier.ClearData();
            _ndoMfgOrder.ClearData();
            _dateExpectedStartDate.ClearData();
            _txtCycleTime.ClearData();
            _txtSalesOrderNumber.ClearData();
            _rdoProduct.ClearData();
            _rdoProductBOM.ClearData();
            _rdoProcessSpec.ClearData();
            _ndsFirstWIPStep.ClearData();
            _ndoShipToFactory.ClearData();
            _ndoOwner.ClearData();
            _ndoPriority.ClearData();
            _ndoPackingType.ClearData();
            _chkAutoPrepare.CheckControl.Checked = false;
            _toggleComments.Reset();
            _txtComments.ClearData();

            _txtSelectionId.Focus();
        } // CustomReset

        //---------------------------------------------------
        // Web part custom action
        //---------------------------------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "Submit":
                        {
                            e.Result = SubmitTransactions();
                            break;
                        }
                    case "Reset":
                        {
                            CustomReset();
                            break;
                        }
                    case "AutoGenerateId":
                        {
                            AutoGenerateToItemId();
                            break;
                        }
                    case "Copy":
                        {
                            CopyButton();
                            break;
                        }
                    case "WaferMapDetails":
                        {
                            PopupWaferMapDetails();
                            break;
                        }
                }
            }
        } // WebPartCustomAction(object sender, CustomActionEventArgs e)

        //---------------------------------------------------
        // Submit Button Codes
        //---------------------------------------------------
        public ResultStatus SubmitTransactions()
        {
            try
            {
                ResultStatus ReturnResultStatus = new ResultStatus();
                //Initialize Service & Objects
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                if (Page.PrimaryServiceType == "LotForm")
                {
                    LotFormService Svc = new LotFormService(profile);
                    LotForm SvcData = new LotForm();
                    LotForm_Info SvcInfo = new LotForm_Info();
                    LotForm_Request ReqData = new LotForm_Request();
                    LotForm_Result ResData = new LotForm_Result();

                    var cLots = new List<string>();
                    int iIndex = 0;
                    DataTable getDetails = new DataTable();
                    (_gridDetails.GridContext as BoundContext).GenerateFullExcelData(_gridDetails.TotalRowCount, out getDetails);

                    SvcData.Details = new CombineLotDetails[_gridDetails.TotalRowCount];
                    SvcData.Details[0] = new CombineLotDetails();
                    SvcData.Details[0].FromContainer = new ContainerRef();
                    SvcData.Details[0].FromContainer.Name = _ddlMainLot.Data.ToString();
                    cLots.Add(_ddlMainLot.Data.ToString());
                    iIndex = 1;
                    foreach (DataRow oRow in getDetails.Rows)
                    {
                        if (_ddlMainLot.Data.ToString() != oRow.Field<string>("Lot").ToString())
                        {
                            SvcData.Details[iIndex] = new CombineLotDetails();
                            SvcData.Details[iIndex].FromContainer = new ContainerRef();
                            SvcData.Details[iIndex].FromContainer.Name = oRow.Field<string>("Lot").ToString();
                            if (Page.PrimaryServiceType == "LotForm")
                            {
                                SvcData.Details[iIndex].StandbyQty = Convert.ToDouble(oRow.Field<string>("StandbyQty").ToString());
                                SvcData.Details[iIndex].QtyToProcess = Convert.ToDouble(oRow.Field<string>("QtyToProcess").ToString());
                                SvcData.Details[iIndex].InProcessQty = Convert.ToDouble(oRow.Field<string>("InProcessQty").ToString());
                                SvcData.Details[iIndex].ProcessedQty = Convert.ToDouble(oRow.Field<string>("ProcessedQty").ToString());
                            }
                            cLots.Add(oRow.Field<string>("Lot").ToString());
                            iIndex = iIndex + 1;
                        }
                        else
                        {
                            if (Page.PrimaryServiceType == "LotForm")
                            {
                                SvcData.Details[0].StandbyQty = Convert.ToDouble(oRow.Field<string>("StandbyQty").ToString());
                                SvcData.Details[0].QtyToProcess = Convert.ToDouble(oRow.Field<string>("QtyToProcess").ToString());
                                SvcData.Details[0].InProcessQty = Convert.ToDouble(oRow.Field<string>("InProcessQty").ToString());
                                SvcData.Details[0].ProcessedQty = Convert.ToDouble(oRow.Field<string>("ProcessedQty").ToString());
                            }
                        }
                    }

                    SvcData.Container = new ContainerRef();
                    SvcData.Container.Name = _ddlMainLot.Data.ToString();
                    if (_ndoProcessType.Data != null)
                    {
                        SvcData.ProcessType = new NamedObjectRef();
                        SvcData.ProcessType.Name = _ndoProcessType.Data.ToString();
                    }
                    if (_ndoEquipment.Data != null)
                    {
                        SvcData.Equipment = new NamedObjectRef();
                        SvcData.Equipment.Name = _ndoEquipment.Data.ToString();
                    }
                    if (SvcData.Details.GetUpperBound(0) + 1 == _gridDetails.TotalRowCount)
                    {
                        base.GetInputData(SvcData);
                    }
                    //Execute Request 
                    ResultStatus Results = Svc.ExecuteTransaction(SvcData, ReqData, out ResData);

                    //Result
                    if (Results.IsSuccess)
                    {
                        _gridDetails.ClearData();
                        _gridWafers.ClearData();
                        _ndoProcessType.ClearSelectionValues();
                        _ndoEquipment.ClearSelectionValues();
                        _ddlMainLot.ClearSelectionValues();
                        _ddlMainLot.ClearData();
                        _txtComments.ClearData();
                    }

                    ReturnResultStatus = Results;
                    return ReturnResultStatus;
                } //Page.PrimaryServiceType == "LotForm"
                else if (Page.PrimaryServiceType != "LotForm")
                {
                    LotFormByWafersService Svc = new LotFormByWafersService(profile);
                    LotFormByWafers SvcData = new LotFormByWafers();
                    LotFormByWafers_Info SvcInfo = new LotFormByWafers_Info();
                    LotFormByWafers_Request ReqData = new LotFormByWafers_Request();
                    LotFormByWafers_Result ResData = new LotFormByWafers_Result();

                    var cLots = new List<string>();
                    int iIndex = 0;
                    DataTable getDetails = new DataTable();
                    (_gridDetails.GridContext as BoundContext).GenerateFullExcelData(_gridDetails.TotalRowCount, out getDetails);

                    SvcData.Details = new CombineLotDetails[_gridDetails.TotalRowCount];
                    SvcData.Details[0] = new CombineLotDetails();
                    SvcData.Details[0].FromContainer = new ContainerRef();
                    SvcData.Details[0].FromContainer.Name = _ddlMainLot.Data.ToString();
                    cLots.Add(_ddlMainLot.Data.ToString());
                    iIndex = 1;
                    foreach (DataRow oRow in getDetails.Rows)
                    {
                        if (_ddlMainLot.Data.ToString() != oRow.Field<string>("Lot").ToString())
                        {
                            SvcData.Details[iIndex] = new CombineLotDetails();
                            SvcData.Details[iIndex].FromContainer = new ContainerRef();
                            SvcData.Details[iIndex].FromContainer.Name = oRow.Field<string>("Lot").ToString();
                            cLots.Add(oRow.Field<string>("Lot").ToString());
                            iIndex = iIndex + 1;
                        }
                    }
                    if (Page.PrimaryServiceType != "LotForm")
                    {
                        CombineLotWafers[] getWafers = _gridWafers.Data as CombineLotWafers[];
                        if (_gridWafers.GridContext.SelectedRowIDs != null)
                        {
                            List<string> selectedIDs = _gridWafers.GridContext.SelectedRowIDs;
                            if (selectedIDs != null || selectedIDs.Count < 1)
                            {
                                SvcData.Wafers = new CombineLotWafers[selectedIDs.Count];
                                iIndex = 0;
                                foreach (string selectedID in selectedIDs)
                                {
                                    SvcData.Wafers[iIndex] = new CombineLotWafers();
                                    if (getWafers[Convert.ToInt32(selectedID)].FromContainer != null)
                                        SvcData.Wafers[iIndex].FromContainer = getWafers[Convert.ToInt32(selectedID)].FromContainer;
                                    if (getWafers[Convert.ToInt32(selectedID)].LotWafersItem != null)
                                        SvcData.Wafers[iIndex].LotWafersItem = getWafers[Convert.ToInt32(selectedID)].LotWafersItem;
                                    if (getWafers[Convert.ToInt32(selectedID)].WaferScribeNumber != null)
                                        SvcData.Wafers[iIndex].WaferScribeNumber = getWafers[Convert.ToInt32(selectedID)].WaferScribeNumber;
                                    if (getWafers[Convert.ToInt32(selectedID)].ToWaferScribeNumber != null)
                                        SvcData.Wafers[iIndex].ToWaferScribeNumber = getWafers[Convert.ToInt32(selectedID)].ToWaferScribeNumber;
                                    if (getWafers[Convert.ToInt32(selectedID)].NDPW != null)
                                        SvcData.Wafers[iIndex].NDPW = getWafers[Convert.ToInt32(selectedID)].NDPW;
                                    if (getWafers[Convert.ToInt32(selectedID)].GoodQty != null)
                                        SvcData.Wafers[iIndex].GoodQty = getWafers[Convert.ToInt32(selectedID)].GoodQty;
                                    if (getWafers[Convert.ToInt32(selectedID)].ToWaferNumber != null)
                                        SvcData.Wafers[iIndex].ToWaferNumber = getWafers[Convert.ToInt32(selectedID)].ToWaferNumber;
                                    if (getWafers[Convert.ToInt32(selectedID)].WaferMapDetails != null)
                                        SvcData.Wafers[iIndex].WaferMapDetails = getWafers[Convert.ToInt32(selectedID)].WaferMapDetails;
                                    iIndex = iIndex + 1;
                                }
                            }
                        }
                    }
                    SvcData.Container = new ContainerRef();
                    SvcData.Container.Name = _ddlMainLot.Data.ToString();
                    if (_ndoProcessType.Data != null)
                    {
                        SvcData.ProcessType = new NamedObjectRef();
                        SvcData.ProcessType.Name = _ndoProcessType.Data.ToString();
                    }
                    if (_ndoEquipment.Data != null)
                    {
                        SvcData.Equipment = new NamedObjectRef();
                        SvcData.Equipment.Name = _ndoEquipment.Data.ToString();
                    }
                    if (SvcData.Details.GetUpperBound(0) + 1 == _gridDetails.TotalRowCount)
                    {
                        base.GetInputData(SvcData);
                    }
                    //Execute Request 
                    ResultStatus Results = Svc.ExecuteTransaction(SvcData, ReqData, out ResData);

                    //Result
                    if (Results.IsSuccess)
                    {
                        _gridDetails.ClearData();
                        _gridWafers.ClearData();
                        _ndoProcessType.ClearSelectionValues();
                        _ndoEquipment.ClearSelectionValues();
                        _ddlMainLot.ClearSelectionValues();
                        _ddlMainLot.ClearData();
                        _txtComments.ClearData();
                    }

                    ReturnResultStatus = Results;
                    return ReturnResultStatus;
                } //Page.PrimaryServiceType != "LotForm"
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                ResultStatus Results = new ResultStatus(ex.Message.ToString(), false);
                return Results;
            }
        }
    }
}



