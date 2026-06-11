/* Copyright 2024 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Collections;

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
using Camstar.WebPortal.PortalFramework;
using SEMI.AppCode;

/// <summary>
/// Summary description for SS_EquipmentWIPMain
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_EquipmentWIPMain : MatrixWebPart
    {
        //ResourceStatusWP Controls
        protected CWC.NamedObject _ndoEquipmentDispatchQuery { get { return Page.FindCamstarControl("WIPMain_EquipmentDispatchQuery") as CWC.NamedObject; } }
        protected CWC.TextBox _txtSelectionID { get { return Page.FindCamstarControl("WIPMain_SelectionID") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoEquipmentGroup { get { return Page.FindCamstarControl("ResourceGroup") as CWC.NamedObject; } }
        protected CWC.DropDownList _ddlInfo { get { return Page.FindCamstarControl("WIPMain_Information") as CWC.DropDownList; } }
        protected CWC.NamedObject _ndoLoadPort { get { return Page.FindCamstarControl("WIPMain_scsLoadPort") as CWC.NamedObject; } }

        //DispatchListLotsWP Controls
        protected JQDataGrid _gridDispatchListLot { get { return Page.FindCamstarControl("DispatchListLotGrid") as JQDataGrid; } }

        //LotItemWP Controls
        protected JQTabContainer _tabLotItem { get { return Page.FindCamstarControl("EqpWIPMain_LotItemTab") as JQTabContainer; } }
        protected JQDataGrid _gridLotData { get { return Page.FindCamstarControl("WIPMain_LotDataGrid") as JQDataGrid; } }
        protected JQDataGrid _gridItemData { get { return Page.FindCamstarControl("WIPMain_ItemDataGrid") as JQDataGrid; } }
        protected CWC.TextBox _txtContainerToDelete { get { return Page.FindCamstarControl("EqpWIPMain_ContainerToRemove") as CWC.TextBox; } }
        protected JQDataGrid _gridCarrierData { get { return Page.FindCamstarControl("WIPMain_CarriersSelection") as JQDataGrid; } }
        protected CWC.TextBox _txtSelectedContainerID { get { return Page.FindCamstarControl("WIPMain_LotItem_ContainerId") as CWC.TextBox; } }

        protected virtual ActionsControl WIPMainActions
        {
            get { return Page.FindIForm("ActionsControl") as ActionsControl; }
        }

        // WIPMain_ControllerWP Controls
        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("WIPMain_Employee") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoProcessType { get { return Page.FindCamstarControl("WIPMain_ProcessType") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEquipment { get { return Page.FindCamstarControl("WIPMain_Equipment") as CWC.NamedObject; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("WIPMain_ComputerName") as CWC.TextBox; } }

        protected CWC.TextBox _txtWIPFlag { get { return Page.FindCamstarControl("WIPMain_WIPFlag") as CWC.TextBox; } }
        protected CWC.TextBox _txtWIPFlagTxn { get { return Page.FindCamstarControl("WIPMain_WIPFlagTxn") as CWC.TextBox; } }
        protected CWC.DropDownList _ddlTxnDataList { get { return Page.FindCamstarControl("WIPMain_TxnDataList") as CWC.DropDownList; } }

        protected CWC.TextBox _txtRejectsSvcType { get { return Page.FindCamstarControl("WIPMain_RejectsSvcType") as CWC.TextBox; } }
        protected CWC.TextBox _txtDataSvcType { get { return Page.FindCamstarControl("WIPMain_DataSvcType") as CWC.TextBox; } }
        protected CWC.TextBox _txtBinningSvcType { get { return Page.FindCamstarControl("WIPMain_BinningSvcType") as CWC.TextBox; } }
        protected CWC.TextBox _txtEqpSetupSvcType { get { return Page.FindCamstarControl("WIPMain_EqpSetupSvcType") as CWC.TextBox; } }

        protected CWC.TextBox _txtWIPInstructions { get { return Page.FindCamstarControl("WIPMain_WIPInstructions") as CWC.TextBox; } }
        protected CWC.TextBox _txtPrimaryServiceType { get { return Page.FindCamstarControl("WIPMain_PrimaryServiceType") as CWC.TextBox; } }

        protected CWC.DropDownList _ddlNextSteps { get { return Page.FindCamstarControl("WIPMain_NextSteps") as CWC.DropDownList; } }
        protected DataEnvelopControl _envWIPMainLotList { get { return Page.FindCamstarControl("WIPMain_LotList") as DataEnvelopControl; } }

        protected CWC.TextBox _txtWebPartInit { get { return Page.FindCamstarControl("WIPMain_WebPartInit") as CWC.TextBox; } }

        // WIPMain_TxnTabWP Controls
        protected JQTabContainer _tabTxn { get { return Page.FindCamstarControl("WIPMain_TxnTab") as JQTabContainer; } }

        // WIPMain_InfoWP Controls
        protected DataEnvelopControl _envWIPMainInfoEnvelop { get { return Page.FindCamstarControl("WIPMain_Info_Envelop") as DataEnvelopControl; } }
        protected CWC.TextBox _txtInfoSelectedContainerId { get { return Page.FindCamstarControl("WIPMain_Info_ContainerId") as CWC.TextBox; } }
        protected CWC.TextBox _txtInfoPrimarySvcType { get { return Page.FindCamstarControl("WIPMain_Info_PrimarySvcType") as CWC.TextBox; } }

        // WIPMain_TrackInTrackOutWP Controls
        protected CWC.TextBox _txtContainerField { get { return Page.FindCamstarControl("ContainerField") as CWC.TextBox; } }
        protected CWC.TextBox _txtCommentsField { get { return Page.FindCamstarControl("Main_Comments") as CWC.TextBox; } }
        protected CWC.RadioButton _rdbMoveInRadioButton { get { return Page.FindCamstarControl("Main_MoveInRadioButton") as CWC.RadioButton; } }
        protected CWC.RadioButton _rdbMoveOutRadioButton { get { return Page.FindCamstarControl("Main_MoveOutRadioButton") as CWC.RadioButton; } }
        protected CWC.RadioButton _rdbTrackInRadioButton { get { return Page.FindCamstarControl("Main_TrackInRadioButton") as CWC.RadioButton; } }
        protected CWC.RadioButton _rdbTrackOutRadioButton { get { return Page.FindCamstarControl("Main_TrackOutRadioButton") as CWC.RadioButton; } }
        protected CWC.TextBox _txtTrackInQtyField { get { return Page.FindCamstarControl("Main_TrackInQtyField") as CWC.TextBox; } }
        protected JQDataGrid _gridSourceEquipmentField { get { return Page.FindCamstarControl("Main_SourceEquipmentField") as JQDataGrid; } }
        protected CWC.CheckBox _chkSplitUnProcessedAsNewScheduleField { get { return Page.FindCamstarControl("Main_SplitUnProcessedAsNewScheduleField") as CWC.CheckBox; } }
        protected CWC.CheckBox _chkRemainInEquipmentField { get { return Page.FindCamstarControl("Main_RemainInEquipmentField") as CWC.CheckBox; } }
        protected CWC.CheckBox _chkRemainInEquipmentIfPossibleField { get { return Page.FindCamstarControl("Main_RemainInEquipmentIfPossibleField") as CWC.CheckBox; } }
        protected CWC.CheckBox _chkSplitUnProcessedField { get { return Page.FindCamstarControl("Main_SplitUnProcessedField") as CWC.CheckBox; } }
        protected CWC.CheckBox _chkCancelTrackInField { get { return Page.FindCamstarControl("Main_CancelTrackInField") as CWC.CheckBox; } }
        protected CWC.TextBox _txtTrackOutQtyField { get { return Page.FindCamstarControl("Main_TrackOutQtyField") as CWC.TextBox; } }
        protected CWC.TextBox _txtDummyQtyField { get { return Page.FindCamstarControl("Main_DummyQtyField") as CWC.TextBox; } }
        protected CWC.TextBox _txtSplitUnProcessedLotIdField { get { return Page.FindCamstarControl("Main_SplitUnProcessedLotIdField") as CWC.TextBox; } }
        protected CWC.NamedSubentity _subTrackOutNextStepField { get { return Page.FindCamstarControl("Main_TrackOutNextStepField") as CWC.NamedSubentity; } }
        protected CWC.TextBox _txtNumberOfStripsField { get { return Page.FindCamstarControl("Main_NumberOfStripsField") as CWC.TextBox; } }
        protected CWC.TextBox _txtSplitUnProcessedQtyField { get { return Page.FindCamstarControl("Main_SplitUnProcessedQtyField") as CWC.TextBox; } }
        protected CWC.TextBox _txtMoveOutQtyField { get { return Page.FindCamstarControl("Main_MoveOutQtyField") as CWC.TextBox; } }
        protected CWC.NamedSubentity _subNextStepField { get { return Page.FindCamstarControl("Main_NextStepField") as CWC.NamedSubentity; } }

        protected CWC.Button _btnClearMain { get { return Page.FindCamstarControl("Main_ClearButton") as CWC.Button; } }
        protected CWC.Button _btnEProcedure { get { return Page.FindCamstarControl("Main_EProcedureButton") as CWC.Button; } }
        protected CWC.Button _btnCreatePE { get { return Page.FindCamstarControl("Main_CreatePEButton") as CWC.Button; } }

        // class level variables/properties
        protected enum FetchTxnDataEvents { SelectionIdEntry, ProcessTypeChange, TrackOutEquipmentChange, WIPFlagChange, PopupWithQtyChange, MainContainerChange };
        protected enum WIPFlagTxnTypes { MOVEIN = 5, TRACKIN = 1, TRACKOUT = 2, MOVEOUT = 4, NONE = 0 }
        protected enum PopupTxnType { EquipmentSetup, EqpMaterialSetup, LotReject, ItemReject, WIPData, SamplingWIPData, LotPacking, LotBins, InProcessSplit, CarrierValidate, SetTestProgram, CheckSheet, EProcedure }

        // Check Sheet controls
        protected CWC.RevisionedObject _rdoEProcField { get { return Page.FindCamstarControl("Main_HiddenElectronicProcedure") as CWC.RevisionedObject; } }
        protected CWC.Button _btnCheckSheet { get { return Page.FindCamstarControl("Main_CheckSheetButton") as CWC.Button; } }

        protected MatrixWebPart wpLotInfo_LotGridWP
        {
            get
            {
                MatrixWebPart wp = null;
                int iParentControlsCount = Parent.Controls.Count;
                for (int z = 0; z < iParentControlsCount; z++)
                {
                    if (Parent.Controls[z].ID == "LotItemWP")
                        wp = Parent.Controls[z] as MatrixWebPart;
                }
                return wp;
            }
        }

        enum Information
        {
            WIPMessages = 0,
            OnlineTraveler = 1,
            Documents = 2,
            Failures = 3,
            LotInfo = 4,
            ResourceDetails = 5,
            SPCChart = 6,
            ProcessTimers = 7
        }
        protected const string _dynamicTypeName = "__WIPMainFramework_LotData_";
        protected const string _SPCViewStateIdentifier = "__WIPMainFramework_SPCTxnData";
        protected const string _DispatchLotViewStateIdentifier = "__EquipmentWIPMain_DispatchLotsMasterList";
        protected const string _LotIndexViewStateIdentifier = "__EquipmentWIPMain_SelectedLotsHash";
        protected const string _PopupTxnViewStateIdentifier = "__EquipmentWIPMain_PopupTxnCall";
        protected const string _PriorityColumnName = "_PriorityIndex";
        //protected const string _SPCTxnDataSessionIdentifier = "__EquipmentWIPMain_SPCTxnData";
        protected const string _SPCTxnDataSessionIdentifier = "_SPCTxnDataList";
        protected const string _WIPMainContainerList = "_ContainerList";

        protected Hashtable _htLotIndexHash;
        protected DataTable _dtDispatchListMasterCopy;
        protected DataTable _dtGridLotDataMasterCopy;
        protected CWC.Button _btnRemoveSelection { get { return Page.FindCamstarControl("EqpWIPMain_RemoveSelection") as CWC.Button; } }
        protected CWC.TextBox _txtPostBackFlag { get { return Page.FindCamstarControl("EqpWIPMain_PostBackFlag") as CWC.TextBox; } }
        protected CWC.Button _btnReloadDispatchList { get { return Page.FindCamstarControl("EqpWIPMain_Reload") as CWC.Button; } }
        protected CWC.TextBox _txtWIPMainTxnType { get { return Page.FindCamstarControl("WIPMain_TxnType") as CWC.TextBox; } }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            // --ExecutionPathPlaceHolder
            base.OnLoad(e);

            _txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);

            //_ndoEquipmentDispatchQuery.DataChanged += _ndoEquipmentDispatchQuery_DataChanged;
            _gridDispatchListLot.RowSelected += _gridDispatchListLot_RowSelected;
            _gridLotData.RowSelected += _gridLotData_RowSelected;
            _ndoEquipment.DataChanged += new EventHandler(_ndoEquipment_DataChanged);
            _ndoProcessType.DataChanged += new EventHandler(_ndoProcessType_DataChanged);
            _btnRemoveSelection.Click += _btnRemoveSelection_Click;

            //WIPMain_TrackInTrackOutWP
            _rdbMoveInRadioButton.RadioControl.CheckedChanged += new EventHandler(MoveInRadioButtonControl_CheckedChanged);
            _rdbTrackInRadioButton.RadioControl.CheckedChanged += new EventHandler(TrackInRadioButtonControl_CheckedChanged);
            _rdbTrackOutRadioButton.RadioControl.CheckedChanged += new EventHandler(TrackOutRadioButtonControl_CheckedChanged);
            _rdbMoveOutRadioButton.RadioControl.CheckedChanged += new EventHandler(MoveOutRadioButtonControl_CheckedChanged);

            if (GetWIPFlag() == "4")
                _gridItemData.GridContext.RowSelectionMode = JQGridSelectionMode.Disable;

            //Item Data Grid PostBackOnSelect will only True when wafer sampling required
            _gridItemData.GridContext.PostBackOnSelect = GetTxnData("RequiredWaferSampling").ToUpper() == "TRUE";

            if (!Page.IsPostBack)
            {
                Page.PortalContext.LocalSession["EsigPostback"] = false;
                _txtPrimaryServiceType.Data = Page.PrimaryServiceType.ToString();

                string sEquipmentSetupServiceType = Page.PrimaryServiceType.ToString();
                sEquipmentSetupServiceType = sEquipmentSetupServiceType.Replace("WIPMain", "EquipmentSetup");
                sEquipmentSetupServiceType = sEquipmentSetupServiceType.Replace("SubLot", "");
                sEquipmentSetupServiceType = sEquipmentSetupServiceType.Replace("MotherLot", "");
                sEquipmentSetupServiceType = sEquipmentSetupServiceType.Replace("Carrier", "");
                sEquipmentSetupServiceType = sEquipmentSetupServiceType.Replace("FinalTest", "");
                sEquipmentSetupServiceType = sEquipmentSetupServiceType.Replace("Test", "");

                _txtEqpSetupSvcType.Data = sEquipmentSetupServiceType;
                Page.DataContract.SetValueByName("WIPMain_EqpSetupSvcType_DM", _txtEqpSetupSvcType.Data);

                if (Page.SessionDataContract.GetValueByName("Resource") != null)
                    _ndoEquipment.Data = Page.SessionDataContract.GetValueByName("Resource").ToString();

                InitViewStates();
                Page.Session[_PopupTxnViewStateIdentifier] = null;
            }
            if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                OnPopupClose();

            //Esig
            bool IsEsigValid = true;
            UIComponentDataContract dataContract = Page.PortalContext.DataContract;
            var allEsigDetails =
                    (Tuple<ESigServiceDetail[], ESigProcessTimerServiceDetail[]>)dataContract.GetValueByName("ESigCaptureDetailsDM");
            ESigServiceCaptureWrapper[] gridCaptures =
                    (ESigServiceCaptureWrapper[])dataContract.GetValueByName("ESigCaptureDM");


            if (gridCaptures != null && allEsigDetails != null && allEsigDetails.Item2 != null)
            {
                foreach (var capture in gridCaptures)
                {
                    if (capture.IsValid == false)
                    {
                        IsEsigValid = false;
                        break;
                    }
                }
                foreach (var ESigProcessTimerServiceDetail in allEsigDetails.Item2)
                {
                    foreach (var item in ESigProcessTimerServiceDetail.ESigProcessTimerDtls)
                        if (item.CaptureDetails == null)
                        {
                            item.CaptureDetails = gridCaptures.Where(c => c.RequirementID == item.ESigReqDetail.ID).Select(c => c.Capture).ToArray();
                        }
                }
            }
            dataContract.SetValueByName("ESigCaptureDetailsDM", allEsigDetails);

            var tuple = ESigCaptureUtil.CollectESigServiceDetailsAll();
            if ((bool)Page.PortalContext.LocalSession["EsigPostback"] && IsEsigValid && allEsigDetails.Item2 != null)
                SubmitTransactions();

        }

        //-----------------------------------------
        //
        //-----------------------------------------
        void InitViewStates()
        {
            _htLotIndexHash = new Hashtable();
            _dtDispatchListMasterCopy = new DataTable();

            ViewState[_LotIndexViewStateIdentifier] = _htLotIndexHash;
            ViewState[_DispatchLotViewStateIdentifier] = _dtDispatchListMasterCopy;
            Page.Session[_SPCTxnDataSessionIdentifier] = null;
        } // InitViewStates       

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void ReloadDispatchList()
        {
            ClearControls(0, false, false);
            string sDispatchQuery = "";
            if (_ndoEquipment.Data != null)
                ExecuteInProcessLotQuery("GetInProcessLotsByResource");
            if (!_ndoEquipmentDispatchQuery.IsEmpty && _ndoEquipment.Data != null)
            {
                sDispatchQuery = _ndoEquipmentDispatchQuery.Data.ToString();
                ExecuteDispatchQuery(sDispatchQuery);

                bool bVisible = false;
                if (_ndoEquipment.Data != null)
                {
                    if (_ndoEquipment.Data.ToString() != "")
                        bVisible = true;
                }
                else
                    bVisible = false;
                Page.DataContract.SetValueByName("WIPMain_AllowWIPEquipmentSetup", bVisible);
            }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        void ExecuteDispatchQuery(string DispatchQueryName)
        {
            string queryTxt = DispatchQueryName;
            FrameworkSession currentSession = FrameworkManagerUtil.GetFrameworkSession(this.Page.Session);
            QueryUtil QueryService = new QueryUtil(currentSession.CurrentUserProfile);
            RecordSet queryResult = null;
            ResultStatus resultStatus = null;
            QueryOptions queryOption = new QueryOptions();
            queryOption.QueryType = Camstar.WCF.ObjectStack.QueryType.User;
            queryOption.StartRow = 1;
            queryOption.RowSetSize = 1000;

            // set the query parameters
            int intTotalParams = 1;
            OM.QueryParameters objQueryParameters = new OM.QueryParameters();
            OM.QueryParameter[] objParameters = new OM.QueryParameter[intTotalParams];
            objParameters[0] = new OM.QueryParameter();
            objParameters[0].Name = "ResourceName";
            objParameters[0].Value = _ndoEquipment.Data.ToString();
            objQueryParameters.Parameters = objParameters;

            QueryService.Execute(queryTxt, objParameters, queryOption, ref queryResult, ref resultStatus);

            if (queryResult.Rows != null)
            {
                JQDataGrid _gridDispatchListLotTemp = _gridDispatchListLot;
                BuildQueryGrid(queryResult, _gridDispatchListLotTemp);

                DataTable dtIndexed = new DataTable("_mod");
                dtIndexed.BeginLoadData();
                DataTableReader dtReader = new DataTableReader(queryResult.GetAsExplicitlyDataTable());
                dtIndexed.Load(dtReader);
                dtIndexed.EndLoadData();

                DataTable _dtDispatchListMasterCopy = dtIndexed.Copy();
                DataTable _dtDispatchListTemp = dtIndexed.Copy();

                // remove the the container from Dispatch List Lots grid which exist in the Lot grid
                if (_dtGridLotDataMasterCopy != null)
                    foreach (DataRow _drGridLot in _dtGridLotDataMasterCopy.Rows)
                    {
                        string sContainerName = _drGridLot["Container"].ToString();
                        string sRowSelectSQL = "Container" + " = " + "\'" + sContainerName + "\'";
                        DataRow[] _drToRemove = _dtDispatchListTemp.Select(sRowSelectSQL);
                        foreach (DataRow r in _drToRemove)
                            _dtDispatchListTemp.Rows.Remove(_drToRemove[0]);
                    }

                if (_dtDispatchListTemp.Rows.Count < _dtDispatchListMasterCopy.Rows.Count)
                    _gridDispatchListLot.Data = _dtDispatchListTemp.Copy();

                // add a extra column that will be used at the priority index since the SQL query many not have it
                DataColumn dc = new DataColumn(_PriorityColumnName);
                dc.AutoIncrement = true;
                dc.AutoIncrementSeed = 0;
                dc.AutoIncrementStep = 1;
                dc.DataType = typeof(Int32);
                _dtDispatchListTemp.Columns.Add(dc);

                int index = -1;
                foreach (DataRow _drDispacthGrid in _dtDispatchListTemp.Rows)
                {
                    _drDispacthGrid.SetField(_PriorityColumnName, ++index);
                }

                // clone a master copy and keep in viewstate
                _dtDispatchListMasterCopy = new DataTable();
                _dtDispatchListMasterCopy = _dtDispatchListTemp.Copy();
                ViewState[_DispatchLotViewStateIdentifier] = _dtDispatchListMasterCopy;

                _gridDispatchListLotTemp.Data = _dtDispatchListTemp; //queryResult.GetAsExplicitlyDataTable();					
            }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        void ExecuteInProcessLotQuery(string LotQueryName)
        {
            string queryTxt = LotQueryName;
            FrameworkSession currentSession = FrameworkManagerUtil.GetFrameworkSession(this.Page.Session);
            QueryUtil QueryService = new QueryUtil(currentSession.CurrentUserProfile);
            RecordSet queryResult = null;
            ResultStatus resultStatus = null;
            QueryOptions queryOption = new QueryOptions();
            queryOption.QueryType = Camstar.WCF.ObjectStack.QueryType.User;
            queryOption.StartRow = 1;
            queryOption.RowSetSize = 1000;

            // set the query parameters
            int intTotalParams = 1;
            OM.QueryParameters objQueryParameters = new OM.QueryParameters();
            OM.QueryParameter[] objParameters = new OM.QueryParameter[intTotalParams];
            objParameters[0] = new OM.QueryParameter();
            objParameters[0].Name = "ResourceName";
            objParameters[0].Value = _ndoEquipment.Data.ToString();
            objQueryParameters.Parameters = objParameters;

            JQDataGrid _gridLotTemp = _gridLotData;

            QueryService.Execute(queryTxt, objParameters, queryOption, ref queryResult, ref resultStatus);
            _gridLotData.ClearData();

            if (queryResult.Rows != null)
            {
                // add a extra column that will be used at the priority index since the SQL query many not have it
                DataTable dtIndexed = new DataTable("_mod2");
                DataColumn dc = new DataColumn(_PriorityColumnName);
                dc.AutoIncrement = true;
                dc.AutoIncrementSeed = 0;
                dc.AutoIncrementStep = 1;
                dc.DataType = typeof(Int32);
                dtIndexed.Columns.Add(dc);

                dtIndexed.BeginLoadData();
                DataTableReader dtReader = new DataTableReader(queryResult.GetAsExplicitlyDataTable());
                dtIndexed.Load(dtReader);
                dtIndexed.EndLoadData();

                // clone a master copy and keep in viewstate
                _dtGridLotDataMasterCopy = new DataTable();
                _dtGridLotDataMasterCopy = dtIndexed.Copy();
                // ViewState[_LotIndexViewStateIdentifier] = _dtGridLotDataMasterCopy;		

                // select first row of Container name and populate to SelectionID
                DataRow _drGridLotData = dtIndexed.Rows[0];
                _txtSelectionID.Data = _drGridLotData["Container"];

                // temporary remove the first row of data in the Lot grid to prevent duplicate entries during FecthTxnData
                dtIndexed.Rows.RemoveAt(0);
                _gridLotTemp.Data = dtIndexed;
                BuildQueryGrid(queryResult, _gridLotTemp);
                FetchTxnData(FetchTxnDataEvents.SelectionIdEntry);

                _gridLotTemp.Data = _dtGridLotDataMasterCopy; //queryResult.GetAsExplicitlyDataTable();	
            }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        void BuildQueryGrid(RecordSet queryResult, JQDataGrid grid)
        {
            JQFieldBase[] Columns = grid.Settings.Columns;
            Header[] headerColumns = queryResult.Headers;
            String[] HiddenColumns = new string[] { "ProcessTimerName", "ProcessTimerRevision", "StartTimeGMT", "MinEndWarningTimeGMT", "MinWarningTimeColor", "MinEndTimeGMT", "MinTimeColor", "MaxEndWarningTimeGMT", "MaxWarningTimeColor", "MaxEndTimeGMT", "MaxTimeColor" };
            foreach (Header headerColumn in headerColumns)
                if (!Columns.Any(x => x.Name.ToUpper().Equals(headerColumn.Name.ToUpper())))
                {
                    grid.AddField(new DataColumn(headerColumn.Name));

                }
            foreach (var field in grid.BoundContext.Fields)
            {
                if (HiddenColumns.Any(x => x.Equals(field.ID)))
                {
                    field.Visible = false;
                }
            }

        } // BuildQueryGrid

        //---------------------------------------------------
        // note: this event is triggered when a row is deleted from the LotDataGrid. The data grid will call javascript to click on the WIPMain_Controller_ContainerDelete button which calls this event
        // the main reason for performing the delete in this manner is due to the page not performing a full post back on delete.
        //---------------------------------------------------
        void LotDataGrid_DeleteEvent()
        {
            string sContainerToDelete = _txtContainerToDelete.Data.ToString();

            // from the item grid, get all the dataList
            SS_WIPMain_ItemData[] oExistingItems = (_gridItemData.GridContext as BoundContext).Data as SS_WIPMain_ItemData[];

            if (oExistingItems != null)
            {
                List<SS_WIPMain_ItemData> oItemsList = oExistingItems.ToList();
                List<SS_WIPMain_ItemData> oItemsToRemove = new List<SS_WIPMain_ItemData>();
                List<string> oIDsToRemove = new List<string>();

                for (int x = 0; x < oItemsList.Count; x++)
                {
                    if (oItemsList[x].Container.ToString() == sContainerToDelete)
                        oItemsToRemove.Add(oItemsList[x]);

                    oIDsToRemove.Add(x.ToString().PadLeft(6, '0'));
                }
                List<string> sSelectedRowIDs = (_gridItemData.GridContext as BoundContext).SelectedRowIDs;

                if (GetTxnData("RequiredWaferSampling").ToUpper() == "TRUE")
                {
                    foreach (string oIDToRemove in oIDsToRemove)
                        sSelectedRowIDs.Remove(oIDToRemove);
                }

                foreach (SS_WIPMain_ItemData removeItem in oItemsToRemove)
                    oItemsList.Remove(removeItem);

                (_gridItemData.GridContext as BoundContext).Data = oItemsList.ToArray();
                _gridItemData.BoundContext.LoadData();
            }

            // get the selected container from Lot grid
            string sContainer = _gridLotData.SelectedRowID;
            DataRow drSelectedRow = _gridLotData.BoundContext.GetItem(sContainer) as DataRow;

            //// add back the removed row to the DispatchList            
            _dtDispatchListMasterCopy = ViewState[_DispatchLotViewStateIdentifier] as DataTable;
            _htLotIndexHash = ViewState[_LotIndexViewStateIdentifier] as Hashtable;

            DataTable dtGridDispatchListLot = _gridDispatchListLot.Data as DataTable;
            DataTable dtGridLot = _gridLotData.Data as DataTable;
            DataTable dtGridDispatchListLotTemp = new DataTable();

            if (_gridDispatchListLot == null)
                dtGridDispatchListLotTemp = dtGridDispatchListLot.Clone();
            else
                dtGridDispatchListLotTemp = dtGridLot.Clone();

            // get the index from LotIndexHash else get the Priority value as index             	
            string sIndex = "-1";
            int iIndex = -1;
            if (_htLotIndexHash.Count != 0)
            {
                if (_htLotIndexHash[sContainerToDelete] != null)
                    sIndex = _htLotIndexHash[sContainerToDelete].ToString();
                else
                    sIndex = drSelectedRow["Priority"].ToString();
            }
            else if (drSelectedRow["Priority"].ToString() != null)
                sIndex = drSelectedRow["Priority"].ToString();

            try { iIndex = int.Parse(sIndex); }
            catch { }

            // insert back the row based on the PriorityIndex column value                    
            string sPriority = sIndex; //drNewRow[_PriorityColumnName].ToString();
            int iPriority = int.Parse(sPriority);
            int iIndexToCheck = iPriority;

            // first row
            if (iPriority == 0)
            {
                if (drSelectedRow["Priority"].ToString() != "")
                {
                    dtGridDispatchListLotTemp.ImportRow(drSelectedRow);
                    if (dtGridDispatchListLot != null)
                        foreach (DataRow drGridDispatchListLot in dtGridDispatchListLot.Rows)
                            dtGridDispatchListLotTemp.ImportRow(drGridDispatchListLot);

                    iIndexToCheck = -1;
                    dtGridDispatchListLot = dtGridDispatchListLotTemp.Copy();
                }
                else
                {
                    drSelectedRow[_PriorityColumnName] = iIndex;
                    iIndexToCheck = 1;
                }
            }

            // if no data in Dispatch grid
            if (dtGridDispatchListLot.Rows.Count == 0)
            {
                dtGridDispatchListLotTemp.ImportRow(drSelectedRow);
                iIndexToCheck = -1;
                dtGridDispatchListLot = dtGridDispatchListLotTemp.Copy();
            }

            while (iIndexToCheck >= 0)
            {
                if (iIndexToCheck >= dtGridDispatchListLot.Rows.Count)
                    iIndexToCheck = dtGridDispatchListLot.Rows.Count - 1;

                // check the current data of the row at the index
                DataRow drRowToCheck = dtGridDispatchListLot.Rows[iIndexToCheck];
                string sRowToCheckPriority = drRowToCheck[_PriorityColumnName].ToString();
                int iRowToCheckPriority = int.Parse(sRowToCheckPriority);

                Boolean isExist = false;
                foreach (DataRow drGridDispatchListLot in dtGridDispatchListLot.Rows)
                {
                    if (drGridDispatchListLot["Priority"].ToString() == "")
                    {
                        if (int.Parse(drGridDispatchListLot[_PriorityColumnName].ToString()) > iIndex)
                        {
                            drSelectedRow[_PriorityColumnName] = iIndex;
                            DataRow[] foundrows = dtGridDispatchListLotTemp.Select("Container" + "=" + "\'" + drSelectedRow["Container"].ToString() + "\'");
                            if (foundrows.Count() == 0 && drGridDispatchListLot["Priority"].ToString() == "" && iIndex < (int.Parse(drGridDispatchListLot[_PriorityColumnName].ToString())))
                            {
                                dtGridDispatchListLotTemp.ImportRow(drSelectedRow);
                                dtGridDispatchListLotTemp.ImportRow(drGridDispatchListLot);
                                isExist = true;
                            }
                            else
                                dtGridDispatchListLotTemp.ImportRow(drGridDispatchListLot);
                        }
                        else
                            dtGridDispatchListLotTemp.ImportRow(drGridDispatchListLot);
                    }
                    else if (iPriority < int.Parse(drGridDispatchListLot["Priority"].ToString()))
                    {
                        if (isExist == false)
                        {
                            dtGridDispatchListLotTemp.ImportRow(drSelectedRow);
                            isExist = true;
                        }
                        dtGridDispatchListLotTemp.ImportRow(drGridDispatchListLot);
                    }
                    else
                        dtGridDispatchListLotTemp.ImportRow(drGridDispatchListLot);
                }

                if (isExist == false)
                    dtGridDispatchListLotTemp.ImportRow(drSelectedRow);

                iIndexToCheck = -1;
            } // while  
            _gridDispatchListLot.ClearData();
            _gridDispatchListLot.Data = dtGridDispatchListLotTemp;

            _txtContainerToDelete.ClearData();

            // if there is not lot reamaining in Lot grid, all controls will be reset except Dispatch grid, Equipment and Dispacth Query
            if (_gridLotData.TotalRowCount == 1)
            {
                //FetchTxnData(FetchTxnDataEvents.MainContainerChange);
                ClearControls(10, false, false);

                //Hide all action buttons
                Page.DataContract.SetValueByName("WIPMain_AllowWIPEquipmentSetup", null);
                Page.DataContract.SetValueByName("WIPMain_AllowWIPEqpMaterialsSetup", null);
                Page.DataContract.SetValueByName("WIPMain_AllowWIPItemRejects", null);
                Page.DataContract.SetValueByName("WIPMain_AllowWIPLotRejects", null);
                Page.DataContract.SetValueByName("WIPMain_AllowWIPData", null);
                Page.DataContract.SetValueByName("WIPMain_AllowSamplingWIPData", null);
                Page.DataContract.SetValueByName("WIPMain_AllowWIPLotBins", null);
                Page.DataContract.SetValueByName("WIPMain_AllowLotPacking", null);
                Page.DataContract.SetValueByName("WIPMain_AllowInProcessSplit", null);
                Page.DataContract.SetValueByName("WIPMain_AllowSetTestProgram", null);
                Page.DataContract.SetValueByName("WIPMain_AllowSorting", null);
                Page.DataContract.SetValueByName("WIPMain_AllowValidateCarrier", null);
                Page.DataContract.SetValueByName("WIPMain_AllowCheckSheet", null);
                Page.DataContract.SetValueByName("WIPMain_AllowEProcedure", null);
                SetWIPMainControls();

                _ddlTxnDataList.ClearData();
                _ddlTxnDataList.DropDownControl.Items.Clear();
                _txtCommentsField.ClearData();
                _txtWIPInstructions.ClearData();
                _txtSelectionID.ClearData();

                _envWIPMainInfoEnvelop.SS_ContainersList = null;
                _envWIPMainInfoEnvelop.SS_DataPacket = null;

                //Clear process type controls
                _ndoProcessType.ClearData();
                _ndoProcessType.ClearSelectionValues();

                //Clear Check Sheet RDO control
                _rdoEProcField.ClearData();
            }
            FetchTxnData(FetchTxnDataEvents.MainContainerChange);
            if (_gridLotData.Data != null && _gridLotData.TotalRowCount > 0)
            {
                ContainerRef[] _newContainerList = new ContainerRef[_gridLotData.BoundContext.GetTotalRows() - 1];
                int j = 0;
                for (int i = 0; i < _newContainerList.Length + 1; i++)
                {
                    if ((_gridLotData.GridContext as BoundContext).GetCell(i, "Container").ToString() != sContainerToDelete)
                        _newContainerList[j++] = new ContainerRef()
                        {
                            Name = (_gridLotData.GridContext as BoundContext).GetCell(i, "Container").ToString()
                        };
                }
                //update the container list for the grouped wip data collection
                Page.Session[_WIPMainContainerList] = _newContainerList;
            }
        }


        //-----------------------------------------
        //
        //-----------------------------------------
        ResponseData _gridDispatchListLot_RowSelected(object sender, JQGridEventArgs args)
        {
            if (_gridDispatchListLot.SelectedRowID != null)
            {
                string sDispatchContainer = _gridDispatchListLot.SelectedRowID;

                DataTable dtGridDispatchListLot = _gridDispatchListLot.Data as DataTable;
                DataTable dtGridLotData = _gridLotData.Data != null ? _gridLotData.Data as DataTable : (_gridDispatchListLot.Data as DataTable).Clone();
                if (_gridLotData.Data == null)
                    dtGridLotData.Rows.Clear();

                bool bIsRowExist = false;

                foreach (DataRow datarow in dtGridLotData.Rows)
                {
                    if ((string)datarow["Container"] == sDispatchContainer)
                        bIsRowExist = true;
                }

                if (!bIsRowExist)
                {
                    bool bIsSuccess = true;
                    _txtSelectionID.Data = sDispatchContainer;
                    bIsSuccess = FetchTxnData(FetchTxnDataEvents.SelectionIdEntry);
                    if (!bIsSuccess)
                        _txtSelectionID.ClearData();

                    if (bIsSuccess)
                    {
                        // add the row to the LotData grid
                        DataRow drNewRow = dtGridLotData.NewRow();
                        drNewRow = _gridDispatchListLot.BoundContext.GetItem(sDispatchContainer) as DataRow;

                        dtGridLotData.ImportRow(drNewRow);
                        _gridLotData.Data = dtGridLotData;
                        _gridLotData.GridContext.RenderToClient = true;

                        // remove the row from the DispatchList grid
                        string sPriority = drNewRow[_PriorityColumnName].ToString();
                        string sRowSelectSQL = _PriorityColumnName + " = " + sPriority;
                        DataRow[] drRowsToRemove = dtGridDispatchListLot.Select(sRowSelectSQL);

                        if (drRowsToRemove.Length > 0)
                            dtGridDispatchListLot.Rows.Remove(drRowsToRemove[0]);

                        _gridDispatchListLot.Data = dtGridDispatchListLot;
                        _gridDispatchListLot.GridContext.RenderToClient = true;

                        // select the first row of lot in LotData grid and populate to SelectionID
                        DataRow firstRow = dtGridLotData.Rows[0];
                        _txtSelectionID.Data = firstRow["Container"].ToString();

                        // store the Container and index into the viewstate hash (store the view state for the Container originally from Dispatch grid only)
                        if (drNewRow["Priority"].ToString() == "")
                        {
                            _htLotIndexHash = ViewState[_LotIndexViewStateIdentifier] as Hashtable;
                            if (_htLotIndexHash == null)
                                _htLotIndexHash = new Hashtable();

                            if (!_htLotIndexHash.ContainsKey(sDispatchContainer))
                                _htLotIndexHash.Add(sDispatchContainer, sPriority);
                            else
                                _htLotIndexHash[sDispatchContainer] = sPriority;

                            ViewState[_LotIndexViewStateIdentifier] = _htLotIndexHash;
                        }
                    }
                }

                _ndoLoadPort.Visible = false;

                if (_ndoEquipment.Data != null && GetTxnData("RequiredLoadPort").ToUpper() == "TRUE" && GetWIPFlag() == "1")
                {
                    _ndoLoadPort.Visible = true;
                }
            }

            return null;

        }

        ResponseData _gridLotData_RowSelected(object sender, JQGridEventArgs args)
        {
            string sDispatchContainer = _gridLotData.SelectedRowID;

            // DataRow drSelectedRow = _gridLotData.BoundContext.GetItem(sDispatchContainer) as DataRow;
            string sSelectedContainer = (_gridLotData.GridContext as BoundContext).GetCell(sDispatchContainer, "Container").ToString();
            _txtSelectionID.Data = sSelectedContainer;
            bool bIsSuccess = true;
            bIsSuccess = FetchTxnData(FetchTxnDataEvents.SelectionIdEntry);

            return null;
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public void SubmitTransactions(CustomActionEventArgs e = null)
        {
            try
            {
                ESigServiceCaptureWrapper[] gridCaptures = (ESigServiceCaptureWrapper[])Page.DataContract.GetValueByName("ESigCaptureDM");
                bool IsEsigValid = true;
                if (gridCaptures != null)
                    foreach (var capture in gridCaptures)
                    {
                        if (capture.IsValid == false)
                        {
                            IsEsigValid = false;
                            break;
                        }
                    }
                if (!(bool)Page.PortalContext.LocalSession["EsigPostback"] || !IsEsigValid)
                    GetEsigRequirementMaintenanceTxn(_txtPrimaryServiceType.Data != null ? _txtPrimaryServiceType.Data.ToString() : "WIPMain", "0", null);
                Page.PortalContext.LocalSession["EsigPostback"] = false;

                //Initialize the Service Data
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                //string ServiceType = this.PrimaryServiceType;
                string sServiceType = _txtPrimaryServiceType.Data != null ? _txtPrimaryServiceType.Data.ToString() : "WIPMain";
                var Svc = new WSDataCreator().CreateService(sServiceType, profile);
                var SvcData = WCFObject.CreateObject(sServiceType) as ICreator;
                var SvcInfo = WCFObject.CreateObject(sServiceType + "_Info") as ICreator;
                var ReqData = WCFObject.CreateObject(sServiceType + "_Request") as ICreator;
                var ResData = WCFObject.CreateObject(sServiceType + "_Result") as ICreator;
                Result result = null;

                //Initialize the variables
                var cSPCTxnDataset = new List<string>();
                var cAlertMessages = new List<string>();
                int iLotsPerBatch = 0;
                int selectedRowCount = 0;
                string sWIPFlag = GetWIPFlag();

                //submit the Esig if have any
                var tuple = ESigCaptureUtil.CollectESigServiceDetailsAll();
                ESigProcessTimerServiceDetail[] eSigDetails = tuple == null ? null : tuple.Item2;
                gridCaptures =
                    (ESigServiceCaptureWrapper[])Page.DataContract.GetValueByName("ESigCaptureDM");
                IsEsigValid = true;
                if (gridCaptures != null)
                    foreach (var capture in gridCaptures)
                    {
                        if (capture.IsValid == false)
                        {
                            IsEsigValid = false;
                            break;
                        }
                    }
                if (eSigDetails != null && eSigDetails.Length > 0 && IsEsigValid)
                    SvcData.SetValue("ss_ESigProcessTimerDetails", eSigDetails);

                //Determine the number of lots to transact in a batch
                iLotsPerBatch = _gridLotData.GridContext.GetTotalRows();

                //Add the Containers and Wafers
                ContainerRef[] containerNames = new ContainerRef[iLotsPerBatch];
                for (int i = 0; i < iLotsPerBatch; i++)
                {
                    containerNames[i] = new ContainerRef();
                    //
                    containerNames[i].Name = _gridLotData.GridContext.GetCell(i, "Container").ToString();
                }
                SvcData.SetValue("Containers", containerNames);

                if (GetTxnData("IsWaferProcessing").ToUpper() == "TRUE" && (sWIPFlag == "1" || sWIPFlag == "2"))
                {
                    if ((_gridItemData.GridContext as BoundContext).SelectedRowIDs != null)
                    {
                        selectedRowCount = (_gridItemData.GridContext as BoundContext).SelectedRowIDs.Count;

                        WIPLotTxnWafersDetails[] waferNames = new WIPLotTxnWafersDetails[selectedRowCount];

                        int selectedItemIndex = 0;
                        List<string> sSelectedRowIDs = (_gridItemData.GridContext as BoundContext).SelectedRowIDs;
                        foreach (string selectedRowID in sSelectedRowIDs)
                        {
                            waferNames[selectedItemIndex] = new WIPLotTxnWafersDetails();
                            waferNames[selectedItemIndex].Container = new ContainerRef();
                            waferNames[selectedItemIndex].Container.Name = _gridItemData.GridContext.GetCell(selectedRowID, "Container").ToString();
                            waferNames[selectedItemIndex].WaferScribeNumber = _gridItemData.GridContext.GetCell(selectedRowID, "WaferScribeNumber").ToString();
                            selectedItemIndex = selectedItemIndex + 1;
                        }
                        SvcData.SetValue("WafersDetails", waferNames);
                    }
                }

                //Set the Process Type, WIP Flag and Comments
                if (_ndoProcessType.Data != null)
                {
                    NamedObjectRef processType = new NamedObjectRef();
                    processType.Name = _ndoProcessType.Data.ToString();

                    SvcData.SetValue("ProcessType", processType);
                }
                SvcData.SetValue("WIPFlag", Convert.ToInt32(sWIPFlag));

                //Add common data to submit
                if (_ndoEmployee.Data != null)
                    SvcData.SetValue("Employee", _ndoEmployee.Data);

                if (_txtComputerName.Data != null)
                    SvcData.SetValue("ComputerName", _txtComputerName.Data);

                if (_txtCommentsField.Data != null)
                    SvcData.SetValue("Comments", _txtCommentsField.Data);

                //Set service type specific data
                switch (sWIPFlag)
                {
                    case "1": //Track In Data
                        if (_ndoEquipment.Data != null)
                        {
                            NamedObjectRef eqpment = new NamedObjectRef();
                            eqpment.Name = _ndoEquipment.Data.ToString();
                            SvcData.SetValue("Equipment", eqpment);
                        }

                        if (_ndoLoadPort.Data != null)
                        {
                            NamedObjectRef loadport = new NamedObjectRef();
                            loadport.Name = _ndoLoadPort.Data.ToString().ToUpper();
                            SvcData.SetValue("scsLoadPort", loadport);
                        }

                        if (_txtTrackInQtyField.Visible)
                        {
                            SvcData.SetValue("TrackInQty", _txtTrackInQtyField.Data);
                        }

                        //Add Source Equipment Panel Data
                        if (_gridSourceEquipmentField.Visible)
                        {
                            int iSelected = 0;
                            List<TrackInLotSourceEquipment> oTrackInLotSourceEquipmentList = new List<TrackInLotSourceEquipment>();
                            TrackInLotSourceEquipment[] oTrackInLotSourceEquipment = _gridSourceEquipmentField.Data as TrackInLotSourceEquipment[];
                            foreach (TrackInLotSourceEquipment oSourceEquipment in oTrackInLotSourceEquipment)
                            {
                                if (oSourceEquipment.Qty != null)
                                    if (oSourceEquipment.Qty.ToString() != "" && oSourceEquipment.Qty.ToString() != "0")
                                    {
                                        TrackInLotSourceEquipment oTILSE = new TrackInLotSourceEquipment();
                                        oTILSE.Equipment = new NamedObjectRef(oSourceEquipment.Equipment.Name);
                                        oTILSE.Qty = oSourceEquipment.Qty;
                                        oTrackInLotSourceEquipmentList.Add(oTILSE);
                                        iSelected++;
                                    }
                            }

                            if (iSelected > 0)
                                SvcData.SetValue("SourceEquipment", oTrackInLotSourceEquipmentList.ToArray());
                        }
                        break;

                    case "2": //Track Out Data
                        if (_ndoEquipment.Data != null)
                        {
                            NamedObjectRef eqpment = new NamedObjectRef();
                            eqpment.Name = _ndoEquipment.Data.ToString();
                            SvcData.SetValue("Equipment", eqpment);
                        }
                        if (_txtTrackOutQtyField.Visible)
                        {
                            SvcData.SetValue("TrackOutQty", _txtTrackOutQtyField.Data);
                        }
                        if (_subTrackOutNextStepField.Data != null)
                        {
                            NamedSubentityRef nextStep = new NamedSubentityRef();
                            nextStep.Name = _subTrackOutNextStepField.TextEditControl.Text.ToString();

                            // search the dropdown list to get the ID of the name 
                            string sTrackOutNextStepID = _ddlNextSteps.DropDownControl.Items.FindByText(_subTrackOutNextStepField.TextEditControl.Text.ToString()).Value;
                            nextStep.ID = sTrackOutNextStepID;
                            SvcData.SetValue("NextStep", nextStep);
                        }
                        if (_chkRemainInEquipmentIfPossibleField.CheckControl.Checked)
                        {
                            SvcData.SetValue("RemainInEquipmentIfPossible", true);
                        }
                        if (_chkRemainInEquipmentField.Enabled && _chkRemainInEquipmentField.CheckControl.Checked)
                        {
                            SvcData.SetValue("RemainInEquipment", true);
                        }
                        if (_chkCancelTrackInField.Enabled && _chkCancelTrackInField.CheckControl.Checked)
                        {
                            SvcData.SetValue("CancelTrackIn", true);
                        }
                        if (_txtDummyQtyField.Visible && _txtDummyQtyField.Data != null)
                        {
                            SvcData.SetValue("DummyQty", _txtDummyQtyField.Data.ToString());
                        }
                        if (_txtNumberOfStripsField.Visible && _txtNumberOfStripsField.Data != null)
                        {
                            SvcData.SetValue("NumberOfStrips", _txtNumberOfStripsField.Data.ToString());
                        }
                        if (_chkSplitUnProcessedField.Visible && _chkSplitUnProcessedField.CheckControl.Checked)
                        {
                            SvcData.SetValue("SplitUnProcessed", "true");
                            if (_chkSplitUnProcessedAsNewScheduleField.CheckControl.Checked)
                            {
                                SvcData.SetValue("SplitUnProcessedAsNewSchedule", "true");
                            }
                            else
                            {
                                SvcData.SetValue("SplitUnProcessedAsNewSchedule", "false");
                            }
                            SvcData.SetValue("SplitUnProcessedLotId", _txtSplitUnProcessedLotIdField.Data);
                            SvcData.SetValue("SplitUnProcessedQty", _txtSplitUnProcessedQtyField.Data);
                        }
                        SvcInfo.SetValue("SPCTxnDataList", new SPCTxnData_Info());
                        SvcInfo.SetValue("SPCTxnDataList.Name", new Info(true));
                        SvcInfo.SetValue("SPCTxnDataList.SPCSetup", new Info(true));
                        SvcInfo.SetValue("SPCTxnDataList.SPCResult", new Info(true));
                        SvcInfo.SetValue("SPCTxnDataList.SPCResultFilename", new Info(true));
                        SvcInfo.SetValue("SPCTxnDataList.FailureDocumentSet", new Info(true));
                        SvcInfo.SetValue("SPCTxnDataList.ChartHeight", new Info(true));
                        SvcInfo.SetValue("SPCTxnDataList.ChartWidth", new Info(true));
                        ReqData.SetValue("Info", SvcInfo);

                        //Add carrier tracking data for Assembly Carrier WIP Main if output carrier is detected
                        if (sServiceType == "AssemblyCarrierWIPMain" && _gridLotData.TotalRowCount == 1)
                        {
                            if (_gridLotData.GridContext.GetCell(0, "__OutputCarrier") != null)
                                if (_gridLotData.GridContext.GetCell(0, "__OutputCarrier").ToString() != "")
                                {
                                    NamedObjectRef[] carrierNames = new NamedObjectRef[1];
                                    carrierNames[0] = new NamedObjectRef();
                                    carrierNames[0].Name = _gridLotData.GridContext.GetCell(0, "__OutputCarrier").ToString();
                                    SvcData.SetValue("Carriers", carrierNames);
                                }
                        }
                        break;

                    case "4": //Move Out Data
                        if (_txtMoveOutQtyField.Visible)
                        {
                            SvcData.SetValue("MoveOutQty", _txtMoveOutQtyField.Data);
                        }
                        if (_subNextStepField.Data != null)
                        {
                            NamedSubentityRef nextStep = _subNextStepField.Data as NamedSubentityRef;
                            nextStep.Name = _subNextStepField.TextEditControl.Text.ToString();
                            // search the hidden TrackOutNextStep dropdown list to get the ID of the name
                            string sNextStepID = _ddlNextSteps.DropDownControl.Items.FindByText(_subNextStepField.TextEditControl.Text.ToString()).Value;
                            nextStep.ID = sNextStepID;
                            SvcData.SetValue("NextStep", nextStep);
                        }
                        break;

                    default: sWIPFlag = ""; break;
                }

                //Submit the transaction
                ResultStatus Results = Svc.ExecuteTransaction(SvcData as DCObject, ReqData as Request, out result);
                if (Results.IsSuccess)
                {
                    bool bSPCAvailable = false;
                    bool bAlertAvailable = false;
                    string sCompletionMessage = Results.Message;

                    //Check for SPC Txn Data List
                    SPCTxnData[] oSPCTxnData = null;
                    if ((result.Value as WIPMain).SPCTxnDataList != null)
                    {
                        oSPCTxnData = (result.Value as WIPMain).SPCTxnDataList;
                        if (oSPCTxnData.Length > 0)
                            bSPCAvailable = true;
                    }

                    if (bSPCAvailable)
                    {
                        if (oSPCTxnData != null)
                            SetSPCControls(true, oSPCTxnData);
                        DisplaySPCChart(oSPCTxnData, Results);
                    }
                    else
                    {
                        DisplayAlerts(Results, out bAlertAvailable, out sCompletionMessage);
                        Results.Message = sCompletionMessage;

                        if (e != null)
                            e.Result = Results;
                        else
                            this.DisplayMessage(Results);
                    }

                    //ClearControls(0, true);
                    ReloadDispatchList();

                    if (!bAlertAvailable)
                    {
                        if (e != null)
                            e.Result = Results;
                        else
                            this.DisplayMessage(Results);

                    }
                }
                else
                {
                    if (e != null)
                        e.Result = Results;
                    else
                        this.DisplayMessage(Results);
                }
                ESigCaptureUtil.CleanESigCaptureDM();
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }

        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            // clear the PopupCall viewstate
            Page.Session[_PopupTxnViewStateIdentifier] = null;

            if (action != null && action.Parameters == "Infor")
            {
                GetInfoPopup();
            }
            else if (action != null && action.Parameters == "Reset")
            {
                ClearControls();
            }
            else if (action != null && action.Parameters == "ReloadDispatchList")
            {
                ReloadDispatchList();
            }
            else if (action != null && action.Parameters == "TrackIn" || action.Parameters == "TrackOut" || action.Parameters == "MoveIn" || action.Parameters == "MoveOut")
            {
                ConfirmSubmit(e);
                //SubmitTransactions(e);
            }
            else if (action != null && action.Parameters == "EquipmentSetup")
            {
                Page.Session[_PopupTxnViewStateIdentifier] = PopupTxnType.EquipmentSetup;

                Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                objAction.PageName = "SS_SetupVP";
                objAction.IsPrimary = true;

                objAction.FrameLocation = new UIFloatingPageLocation();
                objAction.FrameLocation.Width = 1150;
                objAction.FrameLocation.Height = 700;
                objAction.EndResponse = true;
                objAction.ShowButtons = true;

                UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[4];
                objLinks[0] = new UIComponentDataContractLink();
                objLinks[0].SourceMember = "WIPMain_Equipment_DM";
                objLinks[0].TargetMember = "EquipmentSetup_ResourceDM";
                objLinks[1] = new UIComponentDataContractLink();
                objLinks[1].SourceMember = "WIPMain_EqpSetupSvcType_DM";
                objLinks[1].TargetMember = "WIPEquipmentSetup_ServiceTypeDM";
                objLinks[2] = new UIComponentDataContractLink();
                objLinks[2].SourceMember = "WIPMain_Employee_DM";
                objLinks[2].TargetMember = "EquipmentSetup_EmployeeDM";
                objLinks[3] = new UIComponentDataContractLink();
                objLinks[3].SourceMember = "WIPMain_ProcessType_DM";
                objLinks[3].TargetMember = "EquipmentSetup_ToolPlanProcessTypeDM";
                objAction.DataContractMap = new UIComponentDataContractMap();
                objAction.DataContractMap.Links = objLinks;
                Page.ActionDispatcher.ExecuteAction(objAction);
            }
            else if (action != null && action.Parameters == "WIPEqpMaterialsSetup")
            {
                Page.Session[_PopupTxnViewStateIdentifier] = PopupTxnType.EqpMaterialSetup;

                Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                objAction.PageName = "SS_EquipmentMaterialsSetupVP";
                objAction.IsPrimary = true;

                objAction.FrameLocation = new UIFloatingPageLocation();
                objAction.FrameLocation.Width = 1050;
                objAction.FrameLocation.Height = 700;
                objAction.EndResponse = true;
                objAction.ShowButtons = true;

                UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[3];
                objLinks[0] = new UIComponentDataContractLink();
                objLinks[0].SourceMember = "WIPMain_Equipment_DM";
                objLinks[0].TargetMember = "WIPEqpMaterialsSetup_EquipmentDM";
                objLinks[1] = new UIComponentDataContractLink();
                objLinks[1].SourceMember = "WIPMain_ContainerId_DM";
                objLinks[1].TargetMember = "WIPEqpMaterialsSetup_ContainerDM";
                objLinks[2] = new UIComponentDataContractLink();
                objLinks[2].SourceMember = "WIPMain_ProcessType_DM";
                objLinks[2].TargetMember = "WIPEqpMaterialsSetup_ProcessTypeDM";
                objAction.DataContractMap = new UIComponentDataContractMap();
                objAction.DataContractMap.Links = objLinks;
                Page.ActionDispatcher.ExecuteAction(objAction);
            }
            else if (action != null && action.Parameters == "ItemRejects")
            {
                Page.Session[_PopupTxnViewStateIdentifier] = PopupTxnType.ItemReject;

                Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                objAction.PageName = "SS_ItemRejectsPopUpVP";
                objAction.IsPrimary = true;

                objAction.FrameLocation = new UIFloatingPageLocation();
                objAction.FrameLocation.Width = 790;
                objAction.FrameLocation.Height = 700;
                objAction.EndResponse = true;
                objAction.ShowButtons = true;

                UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[5];
                objLinks[0] = new UIComponentDataContractLink();
                objLinks[0].SourceMember = "WIPMain_RejectsSvcType_DM";
                objLinks[0].TargetMember = "WIPItemRejects_ServiceTypeDM";
                objLinks[1] = new UIComponentDataContractLink();
                objLinks[1].SourceMember = "WIPMain_Equipment_DM";
                objLinks[1].TargetMember = "WIPItemRejects_EquipmentDM";
                objLinks[2] = new UIComponentDataContractLink();
                objLinks[2].SourceMember = "WIPMain_ProcessType_DM";
                objLinks[2].TargetMember = "WIPItemRejects_ProcessTypeDM";
                objLinks[3] = new UIComponentDataContractLink();
                objLinks[3].SourceMember = "WIPMain_ContainerId_DM";
                objLinks[3].TargetMember = "WIPItemRejects_ContainerDM";
                objLinks[4] = new UIComponentDataContractLink();
                objLinks[4].SourceMember = "WIPMain_IsPopup";
                objLinks[4].TargetMember = "WIPItemRejects_IsPopupDM";
                objAction.DataContractMap = new UIComponentDataContractMap();
                objAction.DataContractMap.Links = objLinks;
                Page.ActionDispatcher.ExecuteAction(objAction);
            }
            else if (action != null && action.Parameters == "LotRejects")
            {
                Page.Session[_PopupTxnViewStateIdentifier] = PopupTxnType.LotReject;

                Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                objAction.PageName = "SS_LotRejectsPopUpVP";
                objAction.IsPrimary = true;

                objAction.FrameLocation = new UIFloatingPageLocation();
                objAction.FrameLocation.Width = 770;
                objAction.FrameLocation.Height = 600;
                objAction.EndResponse = true;
                objAction.ShowButtons = true;

                UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[6];
                objLinks[0] = new UIComponentDataContractLink();
                objLinks[0].SourceMember = "WIPMain_ProcessType_DM";
                objLinks[0].TargetMember = "WIPLotRejects_ProcessTypeDM";
                objLinks[1] = new UIComponentDataContractLink();
                objLinks[1].SourceMember = "WIPMain_RejectsSvcType_DM";
                objLinks[1].TargetMember = "WIPLotRejects_ServiceTypeDM";
                objLinks[2] = new UIComponentDataContractLink();
                objLinks[2].SourceMember = "WIPMain_Equipment_DM";
                objLinks[2].TargetMember = "WIPLotRejects_EquipmentDM";
                objLinks[3] = new UIComponentDataContractLink();
                objLinks[3].SourceMember = "WIPMain_ContainerId_DM";
                objLinks[3].TargetMember = "WIPLotRejects_ContainerDM";
                objLinks[4] = new UIComponentDataContractLink();
                objLinks[4].SourceMember = "WIPMain_Employee_DM";
                objLinks[4].TargetMember = "WIPLotRejects_EmployeeDM";
                objLinks[5] = new UIComponentDataContractLink();
                objLinks[5].SourceMember = "WIPMain_IsPopup";
                objLinks[5].TargetMember = "WIPLotRejects_IsPopupDM";
                objAction.DataContractMap = new UIComponentDataContractMap();
                objAction.DataContractMap.Links = objLinks;
                Page.ActionDispatcher.ExecuteAction(objAction);
            }
            else if (action != null && action.Parameters == "WIPData")
            {
                Page.Session[_PopupTxnViewStateIdentifier] = PopupTxnType.WIPData;

                Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                objAction.PageName = "SS_WIPDataPopupVP";
                objAction.IsPrimary = true;

                objAction.FrameLocation = new UIFloatingPageLocation();
                objAction.FrameLocation.Width = 800;
                objAction.FrameLocation.Height = 700;
                objAction.EndResponse = true;
                objAction.ShowButtons = true;

                UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[6];
                objLinks[0] = new UIComponentDataContractLink();
                objLinks[0].SourceMember = "WIPMain_DataSvcType_DM";
                objLinks[0].TargetMember = "WIPData_ServiceNameDM";
                objLinks[1] = new UIComponentDataContractLink();
                objLinks[1].SourceMember = "WIPMain_Equipment_DM";
                objLinks[1].TargetMember = "WIPData_EquipmentDM";
                objLinks[2] = new UIComponentDataContractLink();
                objLinks[2].SourceMember = "WIPMain_ProcessType_DM";
                objLinks[2].TargetMember = "WIPData_ProcessTypeDM";
                objLinks[3] = new UIComponentDataContractLink();
                objLinks[3].SourceMember = "WIPMain_Employee_DM";
                objLinks[3].TargetMember = "WIPData_EmployeeDM";
                objLinks[4] = new UIComponentDataContractLink();
                objLinks[4].SourceMember = "WIPMain_ContainerId_DM";
                objLinks[4].TargetMember = "WIPData_ContainerDM";
                objLinks[5] = new UIComponentDataContractLink();
                objLinks[5].SourceMember = "WIPMain_IsPopup";
                objLinks[5].TargetMember = "WIPData_IsPopupDM";
                objAction.DataContractMap = new UIComponentDataContractMap();
                objAction.DataContractMap.Links = objLinks;
                Page.ActionDispatcher.ExecuteAction(objAction);
            }
            else if (action != null && action.Parameters == "SamplingWIPData")
            {
                Page.Session[_PopupTxnViewStateIdentifier] = PopupTxnType.SamplingWIPData;

                Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                objAction.PageName = "SS_SamplingWIPDataPopupVP";
                objAction.IsPrimary = true;

                objAction.FrameLocation = new UIFloatingPageLocation();
                objAction.FrameLocation.Width = 800;
                objAction.FrameLocation.Height = 700;
                objAction.EndResponse = true;
                objAction.ShowButtons = true;

                UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[6];
                objLinks[0] = new UIComponentDataContractLink();
                objLinks[0].SourceMember = "WIPMain_DataSvcType_DM";
                objLinks[0].TargetMember = "SamplingWIPData_ServiceNameDM";
                objLinks[1] = new UIComponentDataContractLink();
                objLinks[1].SourceMember = "WIPMain_Equipment_DM";
                objLinks[1].TargetMember = "SamplingWIPData_EquipmentDM";
                objLinks[2] = new UIComponentDataContractLink();
                objLinks[2].SourceMember = "WIPMain_ProcessType_DM";
                objLinks[2].TargetMember = "SamplingWIPData_ProcessTypeDM";
                objLinks[3] = new UIComponentDataContractLink();
                objLinks[3].SourceMember = "WIPMain_Employee_DM";
                objLinks[3].TargetMember = "SamplingWIPData_EmployeeDM";
                objLinks[4] = new UIComponentDataContractLink();
                objLinks[4].SourceMember = "WIPMain_ContainerId_DM";
                objLinks[4].TargetMember = "SamplingWIPData_ContainerDM";
                objLinks[5] = new UIComponentDataContractLink();
                objLinks[5].SourceMember = "WIPMain_IsPopup";
                objLinks[5].TargetMember = "SamplingWIPData_IsPopupDM";
                objAction.DataContractMap = new UIComponentDataContractMap();
                objAction.DataContractMap.Links = objLinks;
                Page.ActionDispatcher.ExecuteAction(objAction);
            }
            else if (action != null && action.Parameters == "WIPLotBins")
            {
                Page.Session[_PopupTxnViewStateIdentifier] = PopupTxnType.LotBins;

                Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                objAction.PageName = "SS_LotBinsPopUpVP";
                objAction.IsPrimary = true;

                objAction.FrameLocation = new UIFloatingPageLocation();
                objAction.FrameLocation.Width = 800;
                objAction.FrameLocation.Height = 700;
                objAction.EndResponse = true;
                objAction.ShowButtons = true;

                UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[6];
                objLinks[0] = new UIComponentDataContractLink();
                objLinks[0].SourceMember = "WIPMain_Employee_DM";
                objLinks[0].TargetMember = "LotBinsTxn_EmployeeDM";
                objLinks[1] = new UIComponentDataContractLink();
                objLinks[1].SourceMember = "WIPMain_Equipment_DM";
                objLinks[1].TargetMember = "LotBinsTxn_EquipmentDM";
                objLinks[2] = new UIComponentDataContractLink();
                objLinks[2].SourceMember = "WIPMain_ProcessType_DM";
                objLinks[2].TargetMember = "LotBinsTxn_ProcessTypeDM";
                objLinks[3] = new UIComponentDataContractLink();
                objLinks[3].SourceMember = "WIPMain_BinningSvcType_DM";
                objLinks[3].TargetMember = "LotBinsTxn_ServiceTypeDM";
                objLinks[4] = new UIComponentDataContractLink();
                objLinks[4].SourceMember = "WIPMain_ContainerId_DM";
                objLinks[4].TargetMember = "LotBinsTxn_ContainerDM";
                objLinks[5] = new UIComponentDataContractLink();
                objLinks[5].SourceMember = "WIPMain_IsPopup";
                objLinks[5].TargetMember = "WIPLotBins_IsPopupDM";
                objAction.DataContractMap = new UIComponentDataContractMap();
                objAction.DataContractMap.Links = objLinks;
                Page.ActionDispatcher.ExecuteAction(objAction);
            }
            else if (action != null && action.Parameters == "LotPacking")
            {
                Page.Session[_PopupTxnViewStateIdentifier] = PopupTxnType.LotPacking;

                Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                objAction.PageName = "SS_LotPackingPopupVP";
                objAction.IsPrimary = true;

                objAction.FrameLocation = new UIFloatingPageLocation();
                objAction.FrameLocation.Width = 800;
                objAction.FrameLocation.Height = 700;
                objAction.EndResponse = true;
                objAction.ShowButtons = true;

                UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[2];
                objLinks[0] = new UIComponentDataContractLink();
                objLinks[0].SourceMember = "WIPMain_Employee_DM";
                objLinks[0].TargetMember = "LotPacking_EmployeeDM";
                objLinks[1] = new UIComponentDataContractLink();
                objLinks[1].SourceMember = "WIPMain_ContainerId_DM";
                objLinks[1].TargetMember = "LotPacking_SelectionIdDM";
                objAction.DataContractMap = new UIComponentDataContractMap();
                objAction.DataContractMap.Links = objLinks;
                Page.ActionDispatcher.ExecuteAction(objAction);
            }
            else if (action != null && action.Parameters == "InProcessSplit")
            {
                Page.Session[_PopupTxnViewStateIdentifier] = PopupTxnType.InProcessSplit;

                Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                objAction.PageName = "SS_InProcessSplitPopupVP";
                objAction.IsPrimary = true;

                objAction.FrameLocation = new UIFloatingPageLocation();
                objAction.FrameLocation.Width = 700;
                objAction.FrameLocation.Height = 700;
                objAction.EndResponse = true;
                objAction.ShowButtons = true;

                UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[3];
                objLinks[0] = new UIComponentDataContractLink();
                objLinks[0].SourceMember = "WIPMain_Equipment_DM";
                objLinks[0].TargetMember = "InProcessSplit_SetEquipmentDM";
                objLinks[1] = new UIComponentDataContractLink();
                objLinks[1].SourceMember = "WIPMain_ProcessType_DM";
                objLinks[1].TargetMember = "InProcessSplit_SetProcessTypeDM";
                objLinks[2] = new UIComponentDataContractLink();
                objLinks[2].SourceMember = "WIPMain_ContainerId_DM";
                objLinks[2].TargetMember = "InProcessSplit_SetSelectionIdDM";

                objAction.DataContractMap = new UIComponentDataContractMap();
                objAction.DataContractMap.Links = objLinks;
                Page.ActionDispatcher.ExecuteAction(objAction);
            }
            else if (action != null && action.Parameters == "CarrierValidate")
            {
                Page.Session[_PopupTxnViewStateIdentifier] = PopupTxnType.CarrierValidate;

                Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                objAction.PageName = "SS_CarrierValidateVP";
                objAction.IsPrimary = true;

                objAction.FrameLocation = new UIFloatingPageLocation();
                objAction.FrameLocation.Width = 700;
                objAction.FrameLocation.Height = 700;
                objAction.EndResponse = true;
                objAction.ShowButtons = true;

                UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[2];
                objLinks[0] = new UIComponentDataContractLink();
                objLinks[0].SourceMember = "WIPMain_ContainerId_DM";
                objLinks[0].TargetMember = "ValidateCarrier_Container_DM";
                objLinks[1] = new UIComponentDataContractLink();
                objLinks[1].SourceMember = "WIPMain_TxnType";
                objLinks[1].TargetMember = "ValidateCarrier_WIPMainTxn_DM";
                objAction.DataContractMap = new UIComponentDataContractMap();
                objAction.DataContractMap.Links = objLinks;
                Page.ActionDispatcher.ExecuteAction(objAction);
            }
            else if (action != null && action.Parameters == "SetTestProgram")
            {
                Page.Session[_PopupTxnViewStateIdentifier] = PopupTxnType.SetTestProgram;

                Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                objAction.PageName = "SS_SetTestProgramVP";
                objAction.IsPrimary = true;

                objAction.FrameLocation = new UIFloatingPageLocation();
                objAction.FrameLocation.Width = 780;
                objAction.FrameLocation.Height = 600;
                objAction.EndResponse = true;
                objAction.ShowButtons = true;

                UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[1];
                objLinks[0] = new UIComponentDataContractLink();
                objLinks[0].SourceMember = "WIPMain_ContainerId_DM";
                objLinks[0].TargetMember = "SetTestProgram_SelectionId_DM";
                objAction.DataContractMap = new UIComponentDataContractMap();
                objAction.DataContractMap.Links = objLinks;
                Page.ActionDispatcher.ExecuteAction(objAction);
            }
            else if (action != null && action.Parameters == "CheckSheet")
            {
                Page.Session[_PopupTxnViewStateIdentifier] = PopupTxnType.CheckSheet;

                //Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                Personalization.PageRedirectAction objAction = new PageRedirectAction();

                var theme = Page.Session["CurrentTheme"] != null ? Page.Session["CurrentTheme"].ToString() : string.Empty;

                if (theme.ToLower() == "horizon")
                {
                    objAction.PageName = "scsCheckSheetVPR2";
                }
                else
                {
                    objAction.PageName = "SS_CheckSheetVP";
                }

                objAction.IsPrimary = true;
                objAction.PortapTabOption = PortalTabOptionType.NewTab;

                ////objAction.FrameLocation = new UIFloatingPageLocation();
                ////objAction.FrameLocation.Width = 1300;
                ////objAction.FrameLocation.Height = 700;
                ////objAction.EndResponse = true;
                ////objAction.ShowButtons = true;

                UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[3];
                objLinks[0] = new UIComponentDataContractLink();
                objLinks[0].SourceMember = "WIPMain_ContainerId_DM";
                objLinks[0].TargetMember = "ContainerDM";
                objLinks[1] = new UIComponentDataContractLink();
                objLinks[1].SourceMember = "WIPMain_ContainerId_DM";
                objLinks[1].TargetMember = "SelectedContainerNameDM";
                objLinks[2] = new UIComponentDataContractLink();
                objLinks[2].SourceMember = "WIPMain_Home_HiddenElectronicProcedure_DM";
                objLinks[2].TargetMember = "ElectronicProcedureDM";
                objAction.DataContractMap = new UIComponentDataContractMap();
                objAction.DataContractMap.Links = objLinks;
                if (Page.Request.Browser.IsMobileDevice)
                {
                    Personalization.FloatPageOpenAction objMobileAction = new FloatPageOpenAction() { PageName = objAction.PageName, IsPrimary = objAction.IsPrimary, DataContractMap = objAction.DataContractMap };
                    Page.ActionDispatcher.ExecuteAction(objMobileAction);
                }
                else
                    Page.ActionDispatcher.ExecuteAction(objAction);
            }
            else if (action != null && action.Parameters == "EProcedure")
            {
                Page.Session[_PopupTxnViewStateIdentifier] = PopupTxnType.EProcedure;

                //Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                Personalization.PageRedirectAction objAction = new PageRedirectAction();

                var theme = Page.Session["CurrentTheme"] != null ? Page.Session["CurrentTheme"].ToString() : string.Empty;

                if (theme.ToLower() == "horizon")
                {
                    objAction.PageName = "EProcedureVPR2";
                }
                else
                {
                    objAction.PageName = "EProcedureVP";
                }
                objAction.IsPrimary = true;
                objAction.PortapTabOption = PortalTabOptionType.NewTab;

                ////objAction.FrameLocation = new UIFloatingPageLocation();
                ////objAction.FrameLocation.Width = 1300;
                ////objAction.FrameLocation.Height = 700;
                ////objAction.EndResponse = true;
                ////objAction.ShowButtons = true;

                UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[2];
                objLinks[0] = new UIComponentDataContractLink();
                objLinks[0].SourceMember = "WIPMain_ContainerId_DM";
                objLinks[0].TargetMember = "ContainerDM";
                objLinks[1] = new UIComponentDataContractLink();
                objLinks[1].SourceMember = "WIPMain_ContainerId_DM";
                objLinks[1].TargetMember = "SelectedContainerNameDM";
                objAction.DataContractMap = new UIComponentDataContractMap();
                objAction.DataContractMap.Links = objLinks;
                if (Page.Request.Browser.IsMobileDevice)
                {
                    Personalization.FloatPageOpenAction objMobileAction = new FloatPageOpenAction() { PageName = objAction.PageName, IsPrimary = objAction.IsPrimary, DataContractMap = objAction.DataContractMap };
                    Page.ActionDispatcher.ExecuteAction(objMobileAction);
                }
                else
                    Page.ActionDispatcher.ExecuteAction(objAction);
            }
            else if (action != null && action.Parameters == "WaferSampling")
            {
                SetTabVisiblity(_tabLotItem, "Item", true);
                _tabLotItem.SelectedIndex = 1;
            }

        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void GetInfoPopup()
        {
            int intSelectedInforValue = -1;
            if (_ddlInfo.Data != null && int.TryParse(_ddlInfo.Data.ToString(), out intSelectedInforValue))
            {
                if (intSelectedInforValue == (int)Information.WIPMessages)
                {
                    GetWIPMessages();
                }
                else if (intSelectedInforValue == (int)Information.OnlineTraveler)
                {
                    Page.Session[_PopupTxnViewStateIdentifier] = PopupTxnType.CheckSheet;

                    Personalization.PageRedirectAction objAction = new PageRedirectAction();

                    objAction.PageName = "scsOnlineTravelerR2_VP";

                    objAction.IsPrimary = true;

                    objAction.PortapTabOption = PortalTabOptionType.NewTab;

                    UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[1];
                    objLinks[0] = new UIComponentDataContractLink();
                    objLinks[0].SourceMember = "WIPMain_ContainerId_DM";
                    objLinks[0].TargetMember = "OnlineTraveler_SelectionId_DM";
                    objAction.DataContractMap = new UIComponentDataContractMap();
                    objAction.DataContractMap.Links = objLinks;
                    Page.ActionDispatcher.ExecuteAction(objAction);
                    
                }
                else if (intSelectedInforValue == (int)Information.Documents)
                {
                    Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                                       
                    objAction.PageName = "SS_DocumentSetVP";
                    objAction.IsPrimary = true;

                    objAction.FrameLocation = new UIFloatingPageLocation();
                    objAction.FrameLocation.Width = 600;
                    objAction.FrameLocation.Height = 400;
                    objAction.EndResponse = true;
                    objAction.ShowButtons = true;

                    UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[2];
                    objLinks[0] = new UIComponentDataContractLink();
                    objLinks[0].SourceMember = "WIPMain_ContainerId_DM";
                    objLinks[0].TargetMember = "DocSetContainerDM";
                    objLinks[1] = new UIComponentDataContractLink();
                    objLinks[1].SourceMember = "WIPMain_Equipment_DM";
                    objLinks[1].TargetMember = "DocSetEquipmentDM";
                    objAction.DataContractMap = new UIComponentDataContractMap();
                    objAction.DataContractMap.Links = objLinks;
                    Page.ActionDispatcher.ExecuteAction(objAction);

                }
                else if (intSelectedInforValue == (int)Information.Failures)
                {
                    Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                    objAction.PageName = "SS_WIPLotFailuresVP";
                    objAction.IsPrimary = true;

                    objAction.FrameLocation = new UIFloatingPageLocation();
                    objAction.FrameLocation.Width = 830;
                    objAction.FrameLocation.Height = 630;
                    objAction.EndResponse = true;
                    objAction.ShowButtons = true;

                    UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[3];
                    objLinks[0] = new UIComponentDataContractLink();
                    objLinks[0].SourceMember = "WIPMain_ContainerId_DM";
                    objLinks[0].TargetMember = "WIPLotFailures_SelectionIdDM";
                    objLinks[1] = new UIComponentDataContractLink();
                    objLinks[1].SourceMember = "WIPMain_ProcessType_DM";
                    objLinks[1].TargetMember = "WIPLotFailures_ProcessTypeDM";
                    objLinks[2] = new UIComponentDataContractLink();
                    objLinks[2].SourceMember = "WIPMain_PrimaryServiceType_DM";
                    objLinks[2].TargetMember = "WIPLotFailures_PrimarySvcTypeDM";
                    objAction.DataContractMap = new UIComponentDataContractMap();
                    objAction.DataContractMap.Links = objLinks;
                    Page.ActionDispatcher.ExecuteAction(objAction);

                }
                else if (intSelectedInforValue == (int)Information.LotInfo)
                {
                    Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                    objAction.PageName = "SS_LotAttributesPopUpVP";
                    objAction.IsPrimary = true;

                    objAction.FrameLocation = new UIFloatingPageLocation();
                    objAction.FrameLocation.Width = 670;
                    objAction.FrameLocation.Height = 670;
                    objAction.EndResponse = true;
                    objAction.ShowButtons = true;

                    UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[15];
                    objLinks[0] = new UIComponentDataContractLink();
                    objLinks[0].SourceMember = "WIPMain_ContainerId_DM";
                    objLinks[0].TargetMember = "ViewContainerStatus_Container_DM";
                    objLinks[1] = new UIComponentDataContractLink();
                    objLinks[1].SourceMember = "WIPMain_ContainerId_DM";
                    objLinks[1].TargetMember = "LotAttributes_LotId_DM";
                    objLinks[2] = new UIComponentDataContractLink();
                    objLinks[2].SourceMember = "WIPMain_ContainerId_DM";
                    objLinks[2].TargetMember = "MaterialRequired_SelectionId_DM";
                    objLinks[3] = new UIComponentDataContractLink();
                    objLinks[3].SourceMember = "WIPMain_ProcessType_DM";
                    objLinks[3].TargetMember = "MaterialRequired_ProcessType_DM";
                    objLinks[4] = new UIComponentDataContractLink();
                    objLinks[4].SourceMember = "WIPMain_ContainerId_DM";
                    objLinks[4].TargetMember = "MaterialRequired_Container_DM";
                    objLinks[5] = new UIComponentDataContractLink();
                    objLinks[5].SourceMember = "WIPMain_ContainerId_DM";
                    objLinks[5].TargetMember = "ProcessSpecParam_SelectionId_DM";
                    objLinks[6] = new UIComponentDataContractLink();
                    objLinks[6].SourceMember = "WIPMain_ContainerId_DM";
                    objLinks[6].TargetMember = "ProcessSpecParam_Container_DM";
                    objLinks[7] = new UIComponentDataContractLink();
                    objLinks[7].SourceMember = "WIPMain_Equipment_DM";
                    objLinks[7].TargetMember = "Recipe_Equipment_DM";
                    objLinks[8] = new UIComponentDataContractLink();
                    objLinks[8].SourceMember = "WIPMain_ContainerId_DM";
                    objLinks[8].TargetMember = "Recipe_Container_DM";
                    objLinks[9] = new UIComponentDataContractLink();
                    objLinks[9].SourceMember = "WIPMain_ContainerId_DM";
                    objLinks[9].TargetMember = "Recipe_SelectionId_DM";
                    objLinks[10] = new UIComponentDataContractLink();
                    objLinks[10].SourceMember = "WIPMain_ContainerId_DM";
                    objLinks[10].TargetMember = "ToolPlan_SelectionId_DM";
                    objLinks[11] = new UIComponentDataContractLink();
                    objLinks[11].SourceMember = "WIPMain_Equipment_DM";
                    objLinks[11].TargetMember = "ToolPlan_Equipment_DM";
                    objLinks[12] = new UIComponentDataContractLink();
                    objLinks[12].SourceMember = "WIPMain_ContainerId_DM";
                    objLinks[12].TargetMember = "ToolPlan_Container_DM";
                    objLinks[13] = new UIComponentDataContractLink();
                    objLinks[13].SourceMember = "WIPMain_ContainerId_DM";
                    objLinks[13].TargetMember = "ToolPlan_ToolPlanLot_DM";
                    objLinks[14] = new UIComponentDataContractLink();
                    objLinks[14].SourceMember = "WIPMain_ProcessType_DM";
                    objLinks[14].TargetMember = "ToolPlan_ToolPlanProcessType_DM";

                    objAction.DataContractMap = new UIComponentDataContractMap();
                    objAction.DataContractMap.Links = objLinks;
                    Page.ActionDispatcher.ExecuteAction(objAction);

                }
                else if (intSelectedInforValue == (int)Information.ResourceDetails)
                {
                    Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                    objAction.PageName = "SS_LotsInProcessVP";
                    objAction.IsPrimary = true;

                    objAction.FrameLocation = new UIFloatingPageLocation();
                    objAction.FrameLocation.Width = 680;
                    objAction.FrameLocation.Height = 520;
                    objAction.EndResponse = true;
                    objAction.ShowButtons = true;

                    UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[1];
                    objLinks[0] = new UIComponentDataContractLink();
                    objLinks[0].SourceMember = "WIPMain_Equipment_DM";
                    objLinks[0].TargetMember = "ResourceValue";
                    objAction.DataContractMap = new UIComponentDataContractMap();
                    objAction.DataContractMap.Links = objLinks;
                    Page.ActionDispatcher.ExecuteAction(objAction);

                }
                else if (intSelectedInforValue == (int)Information.SPCChart)
                {
                    PopupLastSPCChart();
                }

            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void PopupLastSPCChart()
        {
            SPCTxnData[] oSPCTxnData = GetSPCTxnDataViewState();
            if (oSPCTxnData != null)
                DisplaySPCChart(oSPCTxnData, new ResultStatus("", true));
        }


        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _ndoProcessType_DataChanged(object sender, EventArgs e)
        {
            // --ExecutionPathPlaceHolder
            if (_ndoProcessType.Data != null)
                if (_ndoProcessType.Data.ToString() != "")
                    if (GetTxnData("ProcessTypeInit") == "TRUE")
                    {
                        ClearControls(10);
                        FetchTxnData(FetchTxnDataEvents.ProcessTypeChange);
                    }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _ndoEquipment_DataChanged(object sender, EventArgs e)
        {
            bool bVisible = false;
            ClearControls(0, false, false);
            if (_ndoEquipment.Data != null)
                if (_ndoEquipment.Data.ToString() != "")
                {
                    bVisible = true;
                    ExecuteInProcessLotQuery("GetInProcessLotsByResource");
                    _ndoEquipmentDispatchQuery_DataChanged(sender, e);
                }

            if (_ndoEquipmentDispatchQuery.Data == null)
                _gridDispatchListLot.ClearData();

            Page.DataContract.SetValueByName("WIPMain_AllowWIPEquipmentSetup", bVisible);
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        void _btnRemoveSelection_Click(object sender, EventArgs e)
        {
            // The PostBackFlag control is used to check and prevent the LotData_DeleteEvent from being triggered twice
            // When the _gridDispatchListLot's DataTable is updated (via an InsertAt), it does a postback again.
            // That will trigger this _btnRemoveSelection_Click a second time which causes bad things to happen

            string sPostBackFlag = _txtPostBackFlag.Data.ToString();
            int iPostBackFlag = int.Parse(sPostBackFlag);

            if (iPostBackFlag == 0) // valid 'click' via Javascript, call LotDataGrid_DeleteEvent
            {
                LotDataGrid_DeleteEvent();
                iPostBackFlag++;
                _txtPostBackFlag.Data = iPostBackFlag.ToString();
            }
            else // repeated 'click' which is not required so don't call the LotDataGrid_DeleteEvent
            {
                iPostBackFlag = 0;
                _txtPostBackFlag.Data = iPostBackFlag.ToString();
                _txtContainerToDelete.ClearData();
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _ndoEquipmentDispatchQuery_DataChanged(object sender, EventArgs e)
        {
            if (_ndoEquipmentDispatchQuery.Data != null && !string.IsNullOrEmpty(_ndoEquipmentDispatchQuery.Data.ToString()))
                ExecuteDispatchQuery(_ndoEquipmentDispatchQuery.Data.ToString());
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void MoveInRadioButtonControl_CheckedChanged(object sender, EventArgs e)
        {
            // --ExecutionPathPlaceHolder
            try
            {
                SetControls("5");
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void TrackInRadioButtonControl_CheckedChanged(object sender, EventArgs e)
        {
            // --ExecutionPathPlaceHolder
            try
            {
                if (SetControls("1"))
                    FetchTxnData(FetchTxnDataEvents.WIPFlagChange);
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void TrackOutRadioButtonControl_CheckedChanged(object sender, EventArgs e)
        {
            // --ExecutionPathPlaceHolder
            try
            {
                if (SetControls("2"))
                    FetchTxnData(FetchTxnDataEvents.WIPFlagChange);
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void MoveOutRadioButtonControl_CheckedChanged(object sender, EventArgs e)
        {
            // --ExecutionPathPlaceHolder
            try
            {
                SetControls("4");
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected virtual string GetWIPFlag()
        {
            // --ExecutionPathPlaceHolder
            if (_rdbMoveInRadioButton.RadioControl.Checked)
                return "5";
            else if (_rdbTrackInRadioButton.RadioControl.Checked)
                return "1";
            else if (_rdbTrackOutRadioButton.RadioControl.Checked)
                return "2";
            else if (_rdbMoveOutRadioButton.RadioControl.Checked)
                return "4";
            else
                return "0";
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected virtual void SetWIPFlag(WIPFlagTxnTypes WIPFlagType)
        {
            // --ExecutionPathPlaceHolder
            if (_txtWIPFlag != null)
                _txtWIPFlag.Data = ((int)WIPFlagType).ToString();

            // define the WIPTxn. Not required in any transaction, just useful during dev/debug
            string sWIPFlagTxn = "";
            switch (WIPFlagType)
            {
                case WIPFlagTxnTypes.MOVEIN: sWIPFlagTxn = "MOVEIN"; break;
                case WIPFlagTxnTypes.TRACKIN: sWIPFlagTxn = "TRACKIN"; break;
                case WIPFlagTxnTypes.TRACKOUT: sWIPFlagTxn = "TRACKOUT"; break;
                case WIPFlagTxnTypes.MOVEOUT: sWIPFlagTxn = "MOVEOUT"; break;
                case WIPFlagTxnTypes.NONE: sWIPFlagTxn = ""; break;
                default: sWIPFlagTxn = ""; break;
            }

            if (_txtWIPFlagTxn != null)
                _txtWIPFlagTxn.Data = sWIPFlagTxn;
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void SetTabVisiblity(JQTabContainer TabContainer, string TabName, bool TabVisiblity)
        {
            // --ExecutionPathPlaceHolder
            JQTabPanelCollection tabPanels = TabContainer.Tabs;
            foreach (JQTabPanel tabPanel in tabPanels)
            {
                if (tabPanel.Name == TabName)
                {
                    tabPanel.Visible = TabVisiblity;
                    tabPanel.Enabled = TabVisiblity;

                    foreach (Control oTabControls in tabPanel.Controls)
                    {
                        oTabControls.Visible = TabVisiblity;
                    }
                    CamstarWebControl.SetRenderToClient(TabContainer);
                    break;
                }
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected virtual bool SetControls(string WIPFlag = "", string WebPartStatus = "Initialize", bool IsPageClearEvent = false)
        {
            // --ExecutionPathPlaceHolder
            if (GetTxnData("IsWaferProcessing").ToUpper() == "TRUE" && WIPFlag != "5")
            {
                // show the wafers tab
                SetTabVisiblity(_tabLotItem, "ItemsTab", true);

                // if the WIPFlag = 4 then disallow selection of the Wafers
                if (WIPFlag == "4")
                    _gridItemData.GridContext.RowSelectionMode = JQGridSelectionMode.Disable;
            }
            else
            {
                _tabLotItem.SelectedIndex = 0;
                SetTabVisiblity(_tabLotItem, "ItemsTab", false);
            }

            // set the WIPMain home panel controls
            SetWIPMainControls(WIPFlag);

            // set the tab visiblity
            //int iContainerCount = _gridLotData.BoundContext.GetTotalRows();
            bool bContainerSelected = _txtSelectionID.Data != null ? true : false;
            bool bSetTestProgramTabVisible = (GetTxnData("IsTest").ToUpper() == "TRUE") && (WIPFlag == "1" || WIPFlag == "2");
            bool bWIPDataTabVisible = WIPFlag != "5" && bContainerSelected && (GetTxnData("WIPDataExists").ToUpper() == "TRUE") && (GetTxnData("DataCollectionPresent").ToUpper() == "TRUE");
            bool bSamplingWIPDataTabVisible = WIPFlag != "5" && bContainerSelected && (GetTxnData("SamplingWIPDataExists").ToUpper() == "TRUE");
            bool bRejectsLotTabVisible = (GetTxnData("IsWaferProcessing").ToUpper() == "FALSE") && (GetTxnData("AllowRejectsRecording").ToUpper() == "TRUE") && (WIPFlag == "2" || WIPFlag == "3" || WIPFlag == "4");
            bool bRejectsWaferTabVisible = (GetTxnData("IsWaferProcessing").ToUpper() == "TRUE") && (GetTxnData("AllowRejectsRecording").ToUpper() == "TRUE") && (WIPFlag == "2" || WIPFlag == "3" || WIPFlag == "4");
            bool bBinningTabVisible = (GetTxnData("AllowBinsRecording").ToUpper() == "TRUE") && (WIPFlag == "2" || WIPFlag == "3" || WIPFlag == "4");

            bool bCarrierValidationTabVisible = (GetTxnData("AllowCarrierValidation").ToUpper() == "TRUE");
            bool bInProcessSplitTabVisible = false;
            bool bPackingTabVisible = false;
            bool bSortingTabVisible = false;

            string sStepLogicName = GetTxnData("StepLogicName").ToString();

            switch (sStepLogicName)
            {
                case "INPROCESSSPLIT":
                    bInProcessSplitTabVisible = _rdbTrackOutRadioButton.RadioControl.Checked;
                    break;
                case "PACKING":
                    bPackingTabVisible = _rdbMoveOutRadioButton.RadioControl.Checked;
                    break;
                case "SORTING":
                    bSortingTabVisible = _rdbTrackOutRadioButton.RadioControl.Checked;
                    break;
                case "":
                    bPackingTabVisible = false;
                    bInProcessSplitTabVisible = false;
                    break;
                default:
                    bPackingTabVisible = false;
                    bInProcessSplitTabVisible = false;
                    break;
            }

            bool bEqpMaterialsTabVisible = (_ndoEquipment.Data != null) && (GetTxnData("MaterialsSetupRequired").ToUpper() == "TRUE");
            bool bEqpSetupTabVisible = (_ndoEquipment.Data != null) && (WIPFlag == "1") && ((GetTxnData("RecipeRequired").ToUpper() == "TRUE") || (GetTxnData("MaskRequired").ToUpper() == "TRUE") || (GetTxnData("ToolPlanRequired").ToUpper() == "TRUE"));

            //Update DataContract for Action buttons
            Page.DataContract.SetValueByName("WIPMain_AllowWIPEquipmentSetup", bEqpSetupTabVisible);
            Page.DataContract.SetValueByName("WIPMain_AllowWIPEqpMaterialsSetup", bEqpMaterialsTabVisible);
            Page.DataContract.SetValueByName("WIPMain_AllowWIPItemRejects", bRejectsWaferTabVisible);
            Page.DataContract.SetValueByName("WIPMain_AllowWIPLotRejects", bRejectsLotTabVisible);
            Page.DataContract.SetValueByName("WIPMain_AllowWIPData", bWIPDataTabVisible);
            Page.DataContract.SetValueByName("WIPMain_AllowSamplingWIPData", bSamplingWIPDataTabVisible);
            Page.DataContract.SetValueByName("WIPMain_AllowWIPLotBins", bBinningTabVisible);
            Page.DataContract.SetValueByName("WIPMain_AllowLotPacking", bPackingTabVisible);
            Page.DataContract.SetValueByName("WIPMain_AllowInProcessSplit", bInProcessSplitTabVisible);
            Page.DataContract.SetValueByName("WIPMain_AllowSetTestProgram", bSetTestProgramTabVisible);
            Page.DataContract.SetValueByName("WIPMain_AllowSorting", bSortingTabVisible);
            Page.DataContract.SetValueByName("WIPMain_AllowValidateCarrier", bCarrierValidationTabVisible);

            bool bFailuresButtonEnabled = (WIPFlag == "4") && (bContainerSelected);
            bool bDocumentSetButtonEnabled = bContainerSelected;
            bool bOnlineTravelerButtonEnabled = bContainerSelected;
            bool bWIPInstructionButtonEnabled = (bContainerSelected) && (_txtWIPInstructions.Data != null);
            bool bEquipmentLotsEnabled = (_ndoEquipment.Data != null);
            bool bLotInfoEnabled = bContainerSelected;

            // set the service types for the web parts            
            string sRejectsSvcType = "";
            string sDataSvcType = "";
            string sBinningSvcType = "";

            switch (GetWIPFlag())
            {
                case "1": // trackin
                    sDataSvcType = "TrackInLot";
                    _txtWIPMainTxnType.Data = "Track In";
                    break;
                case "2": // trackout
                    sRejectsSvcType = "LotRejectsInProcess";
                    sBinningSvcType = "LotBinsInProcess";
                    sDataSvcType = "TrackOutLot";
                    _txtWIPMainTxnType.Data = "Track Out";
                    break;
                case "4": // moveout
                    if (GetTxnData("AllowRejectsDispose").ToUpper() == "TRUE")
                        sRejectsSvcType = "LotRejectsDispose";
                    else
                        sRejectsSvcType = "LotRejectsPostProcess";

                    if (GetTxnData("AllowBinsDispose").ToUpper() == "TRUE")
                        sBinningSvcType = "LotBinsDispose";
                    else
                        sBinningSvcType = "LotBinsPostProcess";

                    sDataSvcType = "LotMoveOut";
                    _txtWIPMainTxnType.Data = "Move Out";
                    break;
                case "5": // movein
                    _txtWIPMainTxnType.Data = "Move In";
                    break;
            }

            _txtDataSvcType.Data = sDataSvcType;
            _txtRejectsSvcType.Data = sRejectsSvcType;
            _txtBinningSvcType.Data = sBinningSvcType;

            string sEquipmentSetupServiceType = _txtPrimaryServiceType.Data.ToString();
            sEquipmentSetupServiceType = sEquipmentSetupServiceType.Replace("WIPMain", "EquipmentSetup");
            sEquipmentSetupServiceType = sEquipmentSetupServiceType.Replace("SubLot", "");
            sEquipmentSetupServiceType = sEquipmentSetupServiceType.Replace("MotherLot", "");
            sEquipmentSetupServiceType = sEquipmentSetupServiceType.Replace("Carrier", "");
            sEquipmentSetupServiceType = sEquipmentSetupServiceType.Replace("FinalTest", "");
            sEquipmentSetupServiceType = sEquipmentSetupServiceType.Replace("Test", "");

            _txtEqpSetupSvcType.Data = sEquipmentSetupServiceType;
            return true;
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected virtual bool SetWIPMainControls(string mWIPFlag = "")
        {
            // --ExecutionPathPlaceHolder
            try
            {
                // wip main home tab set controls
                int controlsCount = Parent.Controls.Count;
                MatrixWebPart trackinpanel = null;
                MatrixWebPart trackoutpanel = null;
                MatrixWebPart moveoutpanel = null;

                // set load port control not visible
                _ndoLoadPort.Visible = false;

                for (int i = 0; i < controlsCount; i++)
                {
                    if (Parent.Controls[i].ID == "Main_TrackInPanel")
                    {
                        trackinpanel = Parent.Controls[i] as MatrixWebPart;
                    }
                    else if (Parent.Controls[i].ID == "Main_TrackOutPanel")
                    {
                        trackoutpanel = Parent.Controls[i] as MatrixWebPart;
                    }
                    else if (Parent.Controls[i].ID == "Main_MoveOutPanel")
                    {
                        moveoutpanel = Parent.Controls[i] as MatrixWebPart;
                    }
                }


                //Set the WIPFlag controls
                switch (mWIPFlag)
                {
                    case "1":
                    case "2":
                    case "3":
                        _rdbMoveInRadioButton.Enabled = false;
                        _rdbTrackInRadioButton.Enabled = (GetTxnData("WIPFlagSelection") == "1") || (GetTxnData("WIPFlagSelection") == "3");
                        _rdbTrackOutRadioButton.Enabled = (GetTxnData("WIPFlagSelection") == "3");
                        _rdbMoveOutRadioButton.Enabled = false;
                        if (mWIPFlag == "1")
                        {
                            _rdbTrackInRadioButton.RadioControl.Checked = true;
                            _rdbTrackOutRadioButton.RadioControl.Checked = false;
                        }
                        else if (mWIPFlag == "2")
                        {
                            _rdbTrackInRadioButton.RadioControl.Checked = false;
                            _rdbTrackOutRadioButton.RadioControl.Checked = true;
                        }
                        else
                        {
                            if (_rdbTrackInRadioButton.RadioControl.Checked == false && _rdbTrackOutRadioButton.RadioControl.Checked == false)
                            {
                                _rdbTrackOutRadioButton.RadioControl.Checked = true;
                            }
                            if (_rdbTrackInRadioButton.RadioControl.Checked == true && _rdbTrackOutRadioButton.RadioControl.Checked == true)
                            {
                                _rdbTrackInRadioButton.RadioControl.Checked = false;
                            }
                        }
                        _rdbMoveOutRadioButton.RadioControl.Checked = false;

                        _rdbMoveInRadioButton.Visible = false;
                        _rdbTrackInRadioButton.Visible = true;
                        _rdbTrackOutRadioButton.Visible = true;
                        _rdbMoveOutRadioButton.Visible = false;

                        break;
                    case "4":
                        _rdbMoveInRadioButton.Enabled = false;
                        _rdbTrackInRadioButton.Enabled = false;
                        _rdbTrackOutRadioButton.Enabled = false;
                        _rdbMoveOutRadioButton.Enabled = true;

                        _rdbMoveInRadioButton.Visible = false;
                        _rdbTrackInRadioButton.Visible = false;
                        _rdbTrackOutRadioButton.Visible = false;
                        _rdbMoveOutRadioButton.Visible = true;

                        _rdbMoveInRadioButton.RadioControl.Checked = false;
                        _rdbTrackInRadioButton.RadioControl.Checked = false;
                        _rdbTrackOutRadioButton.RadioControl.Checked = false;
                        _rdbMoveOutRadioButton.RadioControl.Checked = true;
                        break;
                    case "5":
                        _rdbMoveInRadioButton.Enabled = true;
                        _rdbTrackInRadioButton.Enabled = false;
                        _rdbTrackOutRadioButton.Enabled = false;
                        _rdbMoveOutRadioButton.Enabled = false;

                        _rdbMoveInRadioButton.Visible = true;
                        _rdbTrackInRadioButton.Visible = false;
                        _rdbTrackOutRadioButton.Visible = false;
                        _rdbMoveOutRadioButton.Visible = false;

                        _rdbMoveInRadioButton.RadioControl.Checked = true;
                        _rdbTrackInRadioButton.RadioControl.Checked = false;
                        _rdbTrackOutRadioButton.RadioControl.Checked = false;
                        _rdbMoveOutRadioButton.RadioControl.Checked = false;
                        break;
                }

                //Set WIP Flag related controls
                if (_rdbMoveInRadioButton.RadioControl.Checked)
                {
                    //Set the wafers details record
                    SetItems_SelectionState(false);

                    //Set Process Types Controls
                    _rdbMoveInRadioButton.Visible = true;
                    _ndoProcessType.Enabled = false;


                    //Set the panels
                    trackinpanel.Hidden = true;
                    trackoutpanel.Hidden = true;
                    moveoutpanel.Hidden = true;
                }
                else if (_rdbTrackInRadioButton.RadioControl.Checked)
                {
                    //Set the wafers details record
                    if (GetTxnData("AutoSetTrackInQty").ToUpper() == "TRUE")
                        SetItems_SelectionState(true);

                    //Set process type controls
                    _rdbMoveInRadioButton.Enabled = false;
                    _ndoProcessType.Visible = true;
                    _ndoProcessType.Enabled = true;

                    //Set track in controls
                    if ((_gridLotData.TotalRowCount > 1) || (GetTxnData("IsWaferProcessing").ToUpper() == "TRUE"))
                    {
                        _txtTrackInQtyField.ClearData();
                        _txtTrackInQtyField.Visible = false;
                        _gridSourceEquipmentField.Visible = false;
                    }
                    else
                    {
                        _txtTrackInQtyField.Visible = true;
                        if (GetTxnData("AutoSetTrackInQty").ToUpper() == "TRUE")
                            _txtTrackInQtyField.Data = GetTxnData("MaxTrackInQty").ToString();
                        else
                            _txtTrackInQtyField.ClearData();
                        _gridSourceEquipmentField.Visible = (GetTxnData("WIPFlagSelection") == "3");
                    }
                    //Set the panels
                    trackinpanel.Hidden = false;
                    trackoutpanel.Hidden = true;
                    moveoutpanel.Hidden = true;

                    //Set load port control
                    if (_ndoEquipment.Data != null && GetTxnData("RequiredLoadPort").ToUpper() == "TRUE")
                    {
                        _ndoLoadPort.Visible = true;
                    }
                    //Set PostBackOnSelect when Wafer Sampling Required
                    _gridItemData.GridContext.PostBackOnSelect = GetTxnData("RequiredWaferSampling").ToUpper() == "TRUE";
                }
                else if (_rdbTrackOutRadioButton.RadioControl.Checked)
                {
                    //Set wafers details records
                    if (GetTxnData("AutoSetTrackOutQty").ToUpper() == "TRUE")
                        SetItems_SelectionState(true);

                    //Set process type controls
                    _rdbMoveInRadioButton.Enabled = false;
                    _ndoProcessType.Visible = true;
                    _ndoProcessType.Enabled = true;

                    //Set track out controls
                    _subTrackOutNextStepField.Visible = (GetTxnData("AutoMoveOut").ToUpper() == "TRUE");
                    _txtDummyQtyField.Visible = (GetTxnData("AllowDummyQty").ToUpper() == "TRUE") && (_gridLotData.TotalRowCount == 1);
                    _txtNumberOfStripsField.Visible = (GetTxnData("AllowNumberOfStrips").ToUpper() == "TRUE") && (_gridLotData.TotalRowCount == 1);
                    if ((_gridLotData.TotalRowCount > 1) || (GetTxnData("IsWaferProcessing").ToUpper() == "TRUE"))
                    {
                        _txtTrackOutQtyField.ClearData();
                        _txtTrackOutQtyField.Visible = false;
                        _chkCancelTrackInField.Visible = (_gridLotData.TotalRowCount > 1);
                        _chkSplitUnProcessedField.Visible = false;
                        _chkSplitUnProcessedAsNewScheduleField.Visible = false;
                        _txtSplitUnProcessedLotIdField.Visible = false;
                        _txtSplitUnProcessedQtyField.Visible = false;
                    }
                    else
                    {
                        _txtTrackOutQtyField.Visible = true;
                        if (GetTxnData("AutoSetTrackOutQty").ToUpper() == "TRUE")
                            _txtTrackOutQtyField.Data = GetTxnData("MaxTrackOutQty").ToString();
                        else
                            _txtTrackOutQtyField.ClearData();
                        _chkCancelTrackInField.Visible = false;
                        bool bAllowSplitUnProcessed = (GetTxnData("AllowSplitUnProcessed").ToUpper() == "TRUE");
                        _chkSplitUnProcessedField.Visible = bAllowSplitUnProcessed;
                        _chkSplitUnProcessedAsNewScheduleField.Visible = bAllowSplitUnProcessed;
                        _txtSplitUnProcessedLotIdField.Visible = bAllowSplitUnProcessed;
                        _txtSplitUnProcessedQtyField.Visible = bAllowSplitUnProcessed;
                    }

                    //Set the panels
                    trackinpanel.Hidden = true;
                    trackoutpanel.Hidden = false;
                    moveoutpanel.Hidden = true;

                    if (GetTxnData("RequiredWaferSampling").ToUpper() == "TRUE")
                    {
                        _chkRemainInEquipmentIfPossibleField.Enabled = false;
                        _chkRemainInEquipmentIfPossibleField.Data = true;
                    }
                }
                else if (_rdbMoveOutRadioButton.RadioControl.Checked)
                {
                    //Set wafers details records
                    SetItems_SelectionState(false);

                    //Set process type controls
                    _rdbMoveInRadioButton.Enabled = false;
                    _ndoProcessType.Visible = true;
                    _ndoProcessType.Enabled = true;
                    //Set move out controls
                    if ((_gridLotData.TotalRowCount > 1) || (GetTxnData("IsWaferProcessing").ToUpper() == "TRUE"))
                    {
                        _txtMoveOutQtyField.ClearData();
                        _txtMoveOutQtyField.Visible = false;
                    }
                    else
                    {
                        _txtMoveOutQtyField.Visible = true;
                        if (GetTxnData("AutoSetMoveOutQty").ToUpper() == "TRUE")
                        {
                            _txtMoveOutQtyField.Data = GetTxnData("MoveOutQty").ToString();
                            _txtMoveOutQtyField.ReadOnly = true;
                        }
                        else
                        {
                            _txtMoveOutQtyField.ClearData();
                            _txtMoveOutQtyField.ReadOnly = false;
                        }
                    }
                    //Set the panels
                    trackinpanel.Hidden = true;
                    trackoutpanel.Hidden = true;
                    moveoutpanel.Hidden = false;
                }
                else
                {
                    //Set the panels
                    trackinpanel.Hidden = true;
                    trackoutpanel.Hidden = true;
                    moveoutpanel.Hidden = true;
                }
                Page.DataContract.SetValueByName("WIPMain_AllowMoveIn", _rdbMoveInRadioButton.RadioControl.Checked);
                Page.DataContract.SetValueByName("WIPMain_AllowTrackIn", _rdbTrackInRadioButton.RadioControl.Checked);
                Page.DataContract.SetValueByName("WIPMain_AllowTrackOut", _rdbTrackOutRadioButton.RadioControl.Checked);
                Page.DataContract.SetValueByName("WIPMain_AllowMoveOut", _rdbMoveOutRadioButton.RadioControl.Checked);
                return true;
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
                return false;
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected virtual string GetTxnData(string DataName)
        {
            System.Web.UI.WebControls.ListItem oItem = _ddlTxnDataList.DropDownControl.Items.FindByText(DataName);
            if (oItem == null)
                return "";
            else
                return oItem.Value;
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void AddTxnData(string DataName, string DataValue, bool DataValueMustExist)
        {
            if (DataValue == "" && DataValueMustExist)
            { }
            else
            {
                System.Web.UI.WebControls.ListItem oItem = null;
                oItem = _ddlTxnDataList.DropDownControl.Items.FindByText(DataName);
                if (oItem == null)
                {
                    oItem = new System.Web.UI.WebControls.ListItem();
                    oItem.Text = DataName;
                }
                oItem.Value = DataValue;

                _ddlTxnDataList.DropDownControl.Items.Add(oItem);
            }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        protected virtual bool FetchTxnData(FetchTxnDataEvents EventName, string[] SelectionLotIds = null, Boolean DoSetControls = true)
        {
            // --ExecutionPathPlaceHolder
            try
            {
                // if there is a WIPflagChange then clear the item selection
                ////if (EventName == FetchTxnDataEvents.WIPFlagChange || EventName == FetchTxnDataEvents.TrackOutEquipmentChange)
                ////    _gridItemData.ClearData();

                string sServiceType = Page.PrimaryServiceType;// _txtPrimaryServiceType.Data != null ? _txtPrimaryServiceType.Data.ToString() : "WIPMain";
                string sWIPFlag = GetWIPFlag();
                string sServiceEventName = "";

                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                var fieldList = new string[] { "LOT" };
                var selectionIds =
                    (from s in fieldList
                     select new Primitive<string>(s)).ToArray();

                bool bIsSubmit = false;

                int iCurrentLotCount = _gridLotData.BoundContext.GetTotalRows();

                // Run proper constructor
                var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
                var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
                var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

                var oServiceData = CreateServiceData(sServiceType);

                var info = CreateServiceInfo(sServiceType);
                var oServiceInfo = info as WIPMain_Info;

                //-----------------------------------------------------
                // set the actual service objects
                //-----------------------------------------------------
                if (_ndoEmployee.Data != null)
                    (oServiceData as WIPMain).Employee = _ndoEmployee.Data as NamedObjectRef;
                if (_txtComputerName.Data != null)
                    (oServiceData as WIPMain).ComputerName = _txtComputerName.Data.ToString();

                //-----------------------------------------------------
                // set criteria for SelectionIdEntry
                //-----------------------------------------------------
                if (EventName == FetchTxnDataEvents.SelectionIdEntry)
                {
                    bIsSubmit = true;
                    sServiceEventName = "ResolveSelectionId";
                    if (_txtSelectionID.Data != null)
                    {
                        (oServiceData as WIPMain).SelectionId = _txtSelectionID.Data.ToString();
                        (oServiceData as WIPMain).SelectionIdTypes = selectionIds;
                    }

                    if (iCurrentLotCount > 0)
                        (oServiceData as WIPMain).ResolveSelIDWIPFlag = int.Parse(sWIPFlag);
                }

                //-----------------------------------------------------
                // set the containers 
                // total the containers in the _gridLotData and the SelectionLotIds parameter
                //-----------------------------------------------------
                int iIndex = iCurrentLotCount;
                if (SelectionLotIds != null)
                    iIndex = iIndex + SelectionLotIds.Length;

                if (iIndex > 0)
                {
                    (oServiceData as WIPMain).Containers = new ContainerRef[iIndex];
                    DataTable dtGridLotData = _gridLotData.Data as DataTable;
                    DataRow[] drGridLotData = dtGridLotData.AsEnumerable().ToArray();
                    for (int x = 0; x <= iCurrentLotCount - 1; x++)
                    {
                        (oServiceData as WIPMain).Containers[x] = new ContainerRef();
                        (oServiceData as WIPMain).Containers[x].Name = drGridLotData[x]["Container"].ToString();
                    }

                    int y = iCurrentLotCount;
                    if (SelectionLotIds != null)
                    {
                        foreach (string sSelectionLotId in SelectionLotIds)
                        {
                            (oServiceData as WIPMain).Containers[y] = new ContainerRef();
                            (oServiceData as WIPMain).Containers[y].Name = sSelectionLotId;
                            y++;
                        }
                    } // if (SelectionLotIds != null)
                } // if (iIndex > 0)

                //++++++++++++++++++++++++++
                // BEGIN: set additional data if there are existing lots
                //++++++++++++++++++++++++++
                if (iCurrentLotCount > 0)
                {
                    //-----------------------------------------------------
                    // pass in the 1st container if the transaction is not a ResolveSelectionID
                    //-----------------------------------------------------
                    if (!bIsSubmit)
                    {
                        (oServiceData as WIPMain).Container = new ContainerRef();
                        if (IsCalledViaResourceLayoutView()) // use rowID to get the cell value instead of the direct cell as the UI has not been rendered yet
                            (oServiceData as WIPMain).Container.Name = _txtSelectionID.Data.ToString();
                        else
                            (oServiceData as WIPMain).Container.Name = _txtSelectionID.Data.ToString();
                    }
                     (oServiceData as WIPMain).SelectedContainer = new ContainerRef();
                    if (_txtSelectionID.Data != null)
                        (oServiceData as WIPMain).SelectedContainer.Name = _txtSelectionID.Data.ToString();
                    else
                        (oServiceData as WIPMain).SelectedContainer.Name = (_gridLotData.GridContext as BoundContext).GetCell(0, "Container").ToString();

                    //-----------------------------------------------------
                    // pass in the ProcessType if selected
                    //-----------------------------------------------------
                    if (_ndoProcessType.Data != null)
                        (oServiceData as WIPMain).ProcessType = _ndoProcessType.Data as NamedObjectRef;

                    //-----------------------------------------------------
                    // Indicate the type of transaction as long as the change is not a ProcessTypeChange
                    // A ProcessType change means the WIPFlagSelection will be recalculated.
                    //-----------------------------------------------------
                    if (EventName != FetchTxnDataEvents.ProcessTypeChange && !bIsSubmit)
                        (oServiceData as WIPMain).WIPFlag = int.Parse(sWIPFlag);

                    //-----------------------------------------------------
                    // pass in the equipment if:
                    // i)   SelectionID is entered when WIPFlag is 2, or
                    // ii)  Equipment is changed when WIPFlag is 2 (track out equipment changed)
                    // iii) WIP Flag is changed to 2
                    // iv) 1st Container in containers grid is changed (due to deletion) when WIPFlag is 2
                    // v)  SelectionID is entered when WIPFlag is 1 and Wafer Sampling Required
                    //-----------------------------------------------------
                    if ((EventName == FetchTxnDataEvents.SelectionIdEntry && sWIPFlag == "2")
                        || (EventName == FetchTxnDataEvents.TrackOutEquipmentChange)
                        || (EventName == FetchTxnDataEvents.WIPFlagChange && (sWIPFlag == "2" || sWIPFlag == "1"))
                        || (EventName == FetchTxnDataEvents.PopupWithQtyChange && iCurrentLotCount == 1 && sWIPFlag == "2")
                        || (EventName == FetchTxnDataEvents.MainContainerChange && sWIPFlag == "2")
                        || (EventName == FetchTxnDataEvents.SelectionIdEntry && sWIPFlag == "1" && GetTxnData("RequiredWaferSampling").ToUpper() == "TRUE"))
                    {
                        if (_ndoEquipment.Data != null)
                            (oServiceData as WIPMain).Equipment = _ndoEquipment.Data as NamedObjectRef;
                    }

                    //-----------------------------------------------------
                    // Pass in Output Carrier through SelectionId if
                    // i)    There is at least one lot selected
                    // ii)   Event name = PopupWithQtyChange
                    // iii)  Service Type = AssemblyCarrierWIPMain
                    // iv)   Not submit
                    // v)    WIPFlagSelection = 2
                    // vi)   OutputCarrier exist
                    //-----------------------------------------------------
                    if (EventName == FetchTxnDataEvents.PopupWithQtyChange
                        && sServiceType == "AssemblyCarrierWIPMain"
                        && !bIsSubmit
                        && GetTxnData("WIPFlagSelection") == "2"
                        && (_gridLotData.GridContext as BoundContext).SelectedRowID != null)
                    {
                        // get the carrier of the selected row                        
                        string sSelectedRowId = (_gridLotData.GridContext as BoundContext).SelectedRowID.ToString();
                        string sOutputCarrier = "";

                        sOutputCarrier = (_gridLotData.GridContext as BoundContext).GetCell(sSelectedRowId, "__OutputCarrier").ToString();

                        if (sOutputCarrier != "")
                        {
                            sServiceEventName = "ResolveSelectionId";
                            (oServiceData as WIPMain).SelectionId = sOutputCarrier;
                        }
                    }
                } // if (iCurrentLotCount > 0)
                //++++++++++++++++++++++++++
                // END: set additional data if there are existing lots
                //++++++++++++++++++++++++++

                //-----------------------------------------------------
                // pass in the equipment if:
                // i)   SelectionID is entered when WIPFlag is 0
                //-----------------------------------------------------
                if (EventName == FetchTxnDataEvents.SelectionIdEntry && sWIPFlag == "0")
                {
                    if (_ndoEquipment.Data != null)
                        (oServiceData as WIPMain).Equipment = _ndoEquipment.Data as NamedObjectRef;
                }

                //-----------------------------------------------------
                // Information to be requested only once
                //-----------------------------------------------------
                //if (iCurrentLotCount == 0)
                //{
                // WIP Data
                oServiceInfo.IsWaferProcessing = FieldInfoUtil.RequestValue();
                oServiceInfo.WIPStatus = FieldInfoUtil.RequestValue();
                oServiceInfo.WIPYieldResult = FieldInfoUtil.RequestValue();

                if (EventName != FetchTxnDataEvents.ProcessTypeChange)
                    oServiceInfo.ProcessTypeSelection = FieldInfoUtil.RequestValue();

                // Sampling WIP Data
                oServiceInfo.SamplingRequired = FieldInfoUtil.RequestValue();
                oServiceInfo.SamplingWIPDataExists = FieldInfoUtil.RequestValue();
                // Process Data
                oServiceInfo.AllowBinsRecording = FieldInfoUtil.RequestValue();
                oServiceInfo.AllowBinsInProcess = FieldInfoUtil.RequestValue();
                oServiceInfo.AllowBinsPostProcess = FieldInfoUtil.RequestValue();
                oServiceInfo.AllowBinsDispose = FieldInfoUtil.RequestValue();
                oServiceInfo.AllowRejectsRecording = FieldInfoUtil.RequestValue();
                oServiceInfo.AllowRejectsInProcess = FieldInfoUtil.RequestValue();
                oServiceInfo.AllowRejectsPostProcess = FieldInfoUtil.RequestValue();
                oServiceInfo.AllowRejectsDispose = FieldInfoUtil.RequestValue();
                oServiceInfo.AllowDummyQty = FieldInfoUtil.RequestValue();
                oServiceInfo.AllowNumberOfStrips = FieldInfoUtil.RequestValue();
                oServiceInfo.IsTest = FieldInfoUtil.RequestValue();
                oServiceInfo.StepLogicName = FieldInfoUtil.RequestValue();
                oServiceInfo.AllowCarrierValidation = FieldInfoUtil.RequestValue();
                oServiceInfo.ss_EProcedureExists = FieldInfoUtil.RequestValue();
                // Track In Data
                oServiceInfo.AutoSetTrackInQty = FieldInfoUtil.RequestValue();
                // Track Out Data
                oServiceInfo.AutoSetTrackOutQty = FieldInfoUtil.RequestValue();
                oServiceInfo.AllowSplitUnProcessed = FieldInfoUtil.RequestValue();
                // Move Out Data
                oServiceInfo.AutoMoveOut = FieldInfoUtil.RequestValue();
                oServiceInfo.AutoSetMoveOutQty = FieldInfoUtil.RequestValue();

                //Required Activities Data
                oServiceInfo.RequiredActivities = FieldInfoUtil.RequestValue();
                oServiceInfo.WIPDataExists = FieldInfoUtil.RequestValue();

                oServiceInfo.WIPFlagSelection = FieldInfoUtil.RequestValue();

                //Required Check Sheet
                //oServiceInfo.RequiredCheckSheet = FieldInfoUtil.RequestValue();
                //}
                //-----------------------------------------------------
                // Set WafersDetails if:
                // i)    RequiredWaferSampling and sWIPFlag == "1"
                //----------------------------------------------------- 
                if (GetTxnData("RequiredWaferSampling").ToUpper() == "TRUE" && sWIPFlag == "1")
                {
                    if ((_gridItemData.GridContext as BoundContext).SelectedRowIDs != null)
                    {
                        var selectedRowCount = (_gridItemData.GridContext as BoundContext).SelectedRowIDs.Count;

                        WIPLotTxnWafersDetails[] waferNames = new WIPLotTxnWafersDetails[selectedRowCount];

                        int selectedItemIndex = 0;
                        List<string> sSelectedRowIDs = (_gridItemData.GridContext as BoundContext).SelectedRowIDs;
                        foreach (string selectedRowID in sSelectedRowIDs)
                        {
                            waferNames[selectedItemIndex] = new WIPLotTxnWafersDetails();
                            waferNames[selectedItemIndex].Container = new ContainerRef();
                            waferNames[selectedItemIndex].Container.Name = _gridItemData.GridContext.GetCell(selectedRowID, "Container").ToString();
                            waferNames[selectedItemIndex].WaferScribeNumber = _gridItemData.GridContext.GetCell(selectedRowID, "WaferScribeNumber").ToString();
                            selectedItemIndex = selectedItemIndex + 1;
                        }
                         (oServiceData as WIPMain).WafersDetails = waferNames;

                    }
                }

                //-----------------------------------------------------
                // Request WIPFlagSelection and RequiredActivities if:
                // i)    No existing lots, or
                // ii)   When Process Type is changed
                //-----------------------------------------------------
                if (iCurrentLotCount == 0 || EventName == FetchTxnDataEvents.ProcessTypeChange)
                {
                    oServiceInfo.WIPFlagSelection = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredActivities = FieldInfoUtil.RequestValue();
                }
                //-----------------------------------------------------
                //  Request MaxTrackInQty and SourceEquipment if:
                //  i)    No existing lots, or
                //  ii)   When Process Type is changed, or
                //  iii)  When WIP Flag is changed to 1 for 1 lot
                //  iv)   when 1st container in the container grid is changed (due to deletion)
                //-----------------------------------------------------
                if (iCurrentLotCount == 0
                    || (EventName == FetchTxnDataEvents.ProcessTypeChange)
                    || (EventName == FetchTxnDataEvents.WIPFlagChange && sWIPFlag == "1" && iCurrentLotCount == 1)
                    || (EventName == FetchTxnDataEvents.MainContainerChange))
                {
                    oServiceInfo.MaxTrackInQty = FieldInfoUtil.RequestValue();
                    oServiceInfo.SourceEquipmentSelection = new TrackInLotSourceEquipment_Info();
                    oServiceInfo.SourceEquipmentSelection.Equipment = FieldInfoUtil.RequestValue();
                    oServiceInfo.SourceEquipmentSelection.TrackInQty = FieldInfoUtil.RequestValue();
                    // FF for IR 9758326 : start 
                    oServiceInfo.RequiredCheckSheet = FieldInfoUtil.RequestValue();
                    // FF for IR 9758326 : end 
                }

                //-----------------------------------------------------
                //  Request EquipmentSelection if:
                //  i)    No existing lots, or
                //  ii)   When Process Type is changed, or
                //  iii)  When WIP Flag is changed to 1 or 2
                //-----------------------------------------------------
                if (iCurrentLotCount == 0
                    || (EventName == FetchTxnDataEvents.ProcessTypeChange)
                    || (EventName == FetchTxnDataEvents.WIPFlagChange && (sWIPFlag == "1" || sWIPFlag == "2")))
                    oServiceInfo.EquipmentSelection = FieldInfoUtil.RequestValue();

                //-----------------------------------------------------
                //  Request MaxTrackOut if:
                //  i)    No existing lots, or
                //  ii)   When Process Type is changed, or
                //  iii)  When Track Out Equipment is changed for 1 lot, or
                //  iv)   When WIP Flag is changed to 2 for 1 lot
                //  v)    When a popup that possibly change quantity if closed (e.g. rejects or bins recording) for 1 lot and WIP Flag = 2
                //  vi)   when 1st container in the container grid is changed (due to deletion)
                //-----------------------------------------------------
                if (iCurrentLotCount == 0
                    || (EventName == FetchTxnDataEvents.ProcessTypeChange)
                    || (EventName == FetchTxnDataEvents.TrackOutEquipmentChange && iCurrentLotCount == 1)
                    || (EventName == FetchTxnDataEvents.WIPFlagChange && sWIPFlag == "2" && iCurrentLotCount == 1)
                    || (EventName == FetchTxnDataEvents.PopupWithQtyChange && iCurrentLotCount == 1 && sWIPFlag == "2")
                    || (EventName == FetchTxnDataEvents.MainContainerChange))
                    oServiceInfo.MaxTrackOutQty = FieldInfoUtil.RequestValue();

                //-----------------------------------------------------
                //  Request MoveOutQty and NextSteps if:
                //  i)    No existing lots, or
                //  ii)   When a popup that possibly change quantity if closed (e.g. rejects or bins recording) for 1 lot and WIP Flag = 4
                //-----------------------------------------------------
                if (iCurrentLotCount == 0
                    || (EventName == FetchTxnDataEvents.PopupWithQtyChange && iCurrentLotCount == 1 && sWIPFlag == "4"))
                {
                    oServiceInfo.MoveOutQty = FieldInfoUtil.RequestValue();
                    oServiceInfo.NextSteps = FieldInfoUtil.RequestValue();
                }

                //-----------------------------------------------------
                //  Request Containers if:
                //  i)    When a Selection Id is entered
                //-----------------------------------------------------
                if (EventName == FetchTxnDataEvents.SelectionIdEntry)
                {
                    oServiceInfo.Containers = FieldInfoUtil.RequestValue();
                    oServiceInfo.CarriersSelection = new CarriersSelection_Info();
                    oServiceInfo.CarriersSelection.Carrier = FieldInfoUtil.RequestValue();
                    oServiceInfo.CarriersSelection.ContainerName = FieldInfoUtil.RequestValue();

                    oServiceInfo.SelectionIdType = FieldInfoUtil.RequestValue();
                    if (iCurrentLotCount == 0 && sServiceType == "AssemblyCarrierWIPMain")
                        oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
                }

                //-----------------------------------------------------
                //  Request WIPInstruction if:
                //  i)    Selection Id is entered or WIP Flag Change
                //-----------------------------------------------------
                if (EventName == FetchTxnDataEvents.SelectionIdEntry || EventName == FetchTxnDataEvents.WIPFlagChange && _txtWIPInstructions.Data == null || EventName == FetchTxnDataEvents.TrackOutEquipmentChange)
                {
                    oServiceInfo.WIPInstruction = FieldInfoUtil.RequestValue();
                }

                //-----------------------------------------------------
                //  Request Activities and Check Sheet if:
                //  i)    Selection Id is entered or 
                //  ii)   WIP Flag Change to 2 and Equipment is not null or
                //  iii)  WIP Flag Change to 1 or
                //  iv)   Track out equipment changed
                //-----------------------------------------------------
                if (EventName == FetchTxnDataEvents.SelectionIdEntry
                     || (EventName == FetchTxnDataEvents.WIPFlagChange && sWIPFlag == "2" && _ndoEquipment.Data != null)
                     || (EventName == FetchTxnDataEvents.WIPFlagChange && sWIPFlag == "1"
                     || EventName == FetchTxnDataEvents.TrackOutEquipmentChange)
                )
                {
                    oServiceInfo.RequiredActivities = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredCheckSheet = FieldInfoUtil.RequestValue();
                }

                //-----------------------------------------------------
                //  Request WafersDetailsSelectionAll and SelectionWafers if:
                //  i)    No existing lots, or
                //  ii)   When Process Type is changed for wafer processing, or
                //  iii)  When a Selection Id is entered with existing lots for wafer processing, or
                //  iv)   WIP Flag changed to 1 or 2 for wafer processing, or
                //  v)    When Track Out Equipment is changed for wafer processing
                //  vi)   When Move Out with possible quantity change for wafer processing
                //-----------------------------------------------------
                if (iCurrentLotCount >= 0
                    || (EventName == FetchTxnDataEvents.ProcessTypeChange && GetTxnData("IsWaferProcessing").ToUpper() == "TRUE")
                    || (EventName == FetchTxnDataEvents.SelectionIdEntry && iCurrentLotCount >= 0 && GetTxnData("IsWaferProcessing").ToUpper() == "TRUE")
                    || (EventName == FetchTxnDataEvents.WIPFlagChange && (sWIPFlag == "1" || sWIPFlag == "2") && GetTxnData("IsWaferProcessing").ToUpper() == "TRUE")
                    || (EventName == FetchTxnDataEvents.TrackOutEquipmentChange && GetTxnData("IsWaferProcessing").ToUpper() == "TRUE")
                    || (EventName == FetchTxnDataEvents.PopupWithQtyChange && sWIPFlag == "4" && GetTxnData("IsWaferProcessing").ToUpper() == "TRUE")
                    )
                {
                    oServiceInfo.WafersDetailsSelectionAll = new WIPLotTxnWafersDetails_Info();
                    oServiceInfo.WafersDetailsSelectionAll.LotWafersItem = FieldInfoUtil.RequestValue();
                    oServiceInfo.WafersDetailsSelectionAll.WIPLotDetailsWafersItem = FieldInfoUtil.RequestValue();
                    oServiceInfo.WafersDetailsSelectionAll.Container = FieldInfoUtil.RequestValue();
                    oServiceInfo.WafersDetailsSelectionAll.WaferScribeNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.WafersDetailsSelectionAll.WaferNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.WafersDetailsSelectionAll.IsBadWafer = FieldInfoUtil.RequestValue();
                    oServiceInfo.WafersDetailsSelectionAll.YieldOffWafer = FieldInfoUtil.RequestValue();
                    oServiceInfo.WafersDetailsSelectionAll.NDPW = FieldInfoUtil.RequestValue();
                    oServiceInfo.WafersDetailsSelectionAll.GoodQty = FieldInfoUtil.RequestValue();
                    oServiceInfo.WafersDetailsSelectionAll.scsSlotNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.WafersDetailsSelectionAll.scsSamplingEnforced = FieldInfoUtil.RequestValue();
                    oServiceInfo.WafersDetailsSelectionAll.scsWaferSamplingMethod = FieldInfoUtil.RequestValue();
                    oServiceInfo.SelectionWafers = new WIPLotTxnWafersDetails_Info();
                    oServiceInfo.SelectionWafers.LotWafersItem = FieldInfoUtil.RequestValue();
                    oServiceInfo.SelectionWafers.WaferScribeNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.SelectionWafers.Container = FieldInfoUtil.RequestValue();
                    oServiceInfo.scsWaferSamplingRequired = FieldInfoUtil.RequestValue();
                }

                // Request Sampling Wafers
                oServiceInfo.scsSamplingWafers = new WIPLotTxnWafersDetails_Info();
                oServiceInfo.scsSamplingWafers.WaferScribeNumber = FieldInfoUtil.RequestValue();
                oServiceInfo.scsSamplingWafers.Container = FieldInfoUtil.RequestValue();

                // Request Reserved Equipment 
                oServiceInfo.ReservedEquipment = FieldInfoUtil.RequestValue();
                oServiceInfo.DataCollectionPresent = FieldInfoUtil.RequestValue();

                //Load Port
                if (_ndoEquipment.Data != null &&
                    ((EventName == FetchTxnDataEvents.SelectionIdEntry && sWIPFlag == "0") || (EventName == FetchTxnDataEvents.WIPFlagChange && sWIPFlag == "1")))
                {
                    oServiceInfo.scsLoadPortSelection = FieldInfoUtil.RequestValue();
                    oServiceInfo.scsLoadPort = FieldInfoUtil.RequestValue();
                    oServiceInfo.scsRequiredLoadPort = FieldInfoUtil.RequestValue();
                }

                //-----------------------------------------------------
                // request the data
                //-----------------------------------------------------
                if (bIsSubmit)
                    (oService as IShopFloorBase).BeginTransaction();

                var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
                (oRequest as Request).Info = oServiceInfo;

                ResultStatus oResultStatus = new ResultStatus();
                Result oResult = new Result();

                if (sServiceEventName == "ResolveSelectionId")
                    if (bIsSubmit)
                        (oService as IShopFloorBase).ResolveSelectionId(oServiceData as DCObject);
                    else
                        oResultStatus = (oService as IShopFloorBase).ResolveSelectionId((oServiceData as DCObject), (oRequest as Request), out oResult);
                else
                    oResultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oResult);

                if (bIsSubmit)
                {
                    (oService as IShopFloorBase).ExecuteTransaction();
                    oResultStatus = (oService as IShopFloorBase).CommitTransaction((oRequest as Request), out oResult);
                }

                if (oResultStatus.IsSuccess)
                {
                    //-----------------------------------------------------
                    // store the return information into the TxnDataList
                    //-----------------------------------------------------
                    var oResultValue = oResult.Value as WIPMain;
                    SetTxnData(oResultValue, (oRequest as Request), iCurrentLotCount, sWIPFlag);

                    if (EventName != FetchTxnDataEvents.MainContainerChange)
                    {
                        if (oServiceInfo.Containers != null)
                            if (oResultValue.Containers != null)
                            {
                                //Populate the Carriers Selection grid
                                if (oResultValue.CarriersSelection != null)
                                    (_gridCarrierData.GridContext as BoundContext).Data = oResultValue.CarriersSelection;

                                //update the container list for the grouped wip data collection
                                Page.Session[_WIPMainContainerList] = oResultValue.Containers;
                            }

                        if (oServiceInfo.WafersDetailsSelectionAll != null)
                        {
                            (_gridItemData.GridContext as BoundContext).ClearData();
                            if (oResultValue.WafersDetailsSelectionAll != null)
                            {
                                SetItems(oResultValue.WafersDetailsSelectionAll, oResultValue.SelectionWafers, sWIPFlag, this.PrimaryServiceType, (oResultValue.SelectionIdType != null ? oResultValue.SelectionIdType.ToString() : ""));
                            }
                        }

                        //-----------------------------------------------------
                        // Set the required activities
                        //-----------------------------------------------------
                        if (oServiceInfo.RequiredActivities != null)
                            if (oResultValue.RequiredActivities != null)
                                SetRequiredActivities(oResultValue.RequiredActivities);

                        // comment out _txtWIPInstructions clear data to fix wip instruction showing during equipment change
                        //_txtWIPInstructions.ClearData();
                        if (oResultValue.WIPInstruction != null)
                            _txtWIPInstructions.Data = oResultValue.WIPInstruction.ToString();

                        //Set Sampling Wafers to be auto selected
                        if (EventName == FetchTxnDataEvents.SelectionIdEntry && GetTxnData("WIPFlagSelection") == "1" && GetTxnData("IsWaferProcessing").ToUpper() == "TRUE"
                                    && (oResultValue.scsSamplingWafers != null || GetTxnData("RequiredWaferSampling").ToUpper() == "TRUE"))
                        {
                            SetItems_WaferSamplingSelectionState(oResultValue.scsSamplingWafers);
                        }

                        // Change for FrontEnd conversion to v5.8: do not set the Controls if DoSetControls is false
                        if (DoSetControls)
                        {
                            if (sWIPFlag != "0" && EventName != FetchTxnDataEvents.ProcessTypeChange)
                                SetControls(sWIPFlag);
                            else

                                SetControls(GetTxnData("WIPFlagSelection"));
                        }
                        else
                        {
                            if (_rdbTrackOutRadioButton.RadioControl.Checked)
                            {
                                //Set wafers details records
                                if (GetTxnData("AutoSetTrackOutQty").ToUpper() == "TRUE")
                                    SetItems_SelectionState(true);
                            }
                            else if (_rdbTrackInRadioButton.RadioControl.Checked)
                            {
                                //Set wafers details records
                                if (GetTxnData("AutoSetTrackInQty").ToUpper() == "TRUE")
                                    SetItems_SelectionState(true);
                            }
                        }

                        //-----------------------------------------------------
                        // show the WIP alerts
                        //-----------------------------------------------------
                        string sCompletionMessage = oResultStatus.Message;
                        if (_txtWIPInstructions.Data != null && oResultValue.WIPInstruction != null)
                            DisplayAlerts(new string[] { _txtWIPInstructions.Data.ToString() });

                        //-----------------------------------------------------
                        // Set the required Check Sheet
                        //-----------------------------------------------------
                        Page.DataContract.SetValueByName("WIPMain_AllowCheckSheet", false);
                        if (oServiceInfo.RequiredCheckSheet != null)
                        {
                            if (oResultValue.RequiredCheckSheet != null && ((GetTxnData("WIPFlagSelection") != "1") || (GetTxnData("WIPFlagSelection") == "1" && _ndoEquipment.Data != null)))
                            {
                                _rdoEProcField.Data = oResultValue.RequiredCheckSheet;
                                Page.DataContract.SetValueByName("WIPMain_AllowCheckSheet", true);
                                Page.DataContract.SetValueByName("WIPMain_Home_HiddenElectronicProcedure_DM", _rdoEProcField.Data);

                            }
                            else
                            {
                                //_btnCheckSheet.Enabled = false;
                                Page.DataContract.SetValueByName("WIPMain_Home_HiddenElectronicProcedure_DM", null);
                            }
                        }

                        //-----------------------------------------------------
                        // Set the required Electronic Procedure
                        //-----------------------------------------------------
                        Page.DataContract.SetValueByName("WIPMain_AllowEProcedure", false);
                        if (oServiceInfo.ss_EProcedureExists != null)
                        {
                            if (oResultValue.ss_EProcedureExists == true)
                                Page.DataContract.SetValueByName("WIPMain_AllowEProcedure", true);
                        }

                    }
                    //Set Sampling Wafers to be auto selected
                    if (EventName == FetchTxnDataEvents.MainContainerChange && GetTxnData("WIPFlagSelection") == "1" && GetTxnData("IsWaferProcessing").ToUpper() == "TRUE"
                                && (oResultValue.scsSamplingWafers != null || GetTxnData("RequiredWaferSampling").ToUpper() == "TRUE"))
                    {
                        SetItems_WaferSamplingSelectionState(oResultValue.scsSamplingWafers);
                    }

                    // check for any alert messages (e.g - due to max time window..etc)
                    bool bAlertAvailable = false;
                    string sResultMessage = oResultStatus.Message;
                    DisplayAlerts(oResultStatus, out bAlertAvailable, out sResultMessage);
                    oResultStatus.Message = sResultMessage;

                } // if (oResultStatus.IsSuccess)
                else
                {
                    DisplayMessage(oResultStatus);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus("FetchTxnData::" + ex.Message.ToString(), false));
                return false;
            }
        } // FetchTxnData

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void SetTxnData(WIPMain WIPMainServiceData, Request WIPMainRequest, int CurrentLotCount, string WIPFlag)
        {
            // --ExecutionPathPlaceHolder
            try
            {
                if (WIPMainServiceData != null)
                {
                    WIPMain oResultData = WIPMainServiceData;
                    //-----------------------------------------------------
                    // Store information that is requested once
                    //-----------------------------------------------------
                    //if (CurrentLotCount == 0)
                    //{
                    AddTxnData("IsWaferProcessing", oResultData.IsWaferProcessing != null ? oResultData.IsWaferProcessing.ToString() : "", true);
                    AddTxnData("WIPStatus", oResultData.WIPStatus != null ? oResultData.WIPStatus.ToString() : "", true);
                    AddTxnData("WIPYieldResult", oResultData.WIPYieldResult != null ? oResultData.WIPYieldResult.ToString() : "", true);
                    // set the process type    
                    if (oResultData.ProcessTypeSelection != null)
                    {
                        CWC.NamedObject _ndoTempProcessType = _ndoProcessType;
                        SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoTempProcessType, oResultData.ProcessTypeSelection);

                        // have to manually set the process type if there is more than one selection value
                        if (oResultData.ProcessTypeSelection.Length > 1)
                            _ndoProcessType.Data = oResultData.ProcessTypeSelection[0];
                    }
                    // add a flag value into the TxnDataList that will allow us to control if the ProcessType_DataChanged event should trigger 
                    AddTxnData("ProcessTypeInit", "TRUE", true);

                    AddTxnData("AllowBinsRecording", oResultData.AllowBinsRecording != null ? oResultData.AllowBinsRecording.ToString() : "", true);
                    AddTxnData("AllowBinsInProcess", oResultData.AllowBinsInProcess != null ? oResultData.AllowBinsInProcess.ToString() : "", true);
                    AddTxnData("AllowBinsPostProcess", oResultData.AllowBinsPostProcess != null ? oResultData.AllowBinsPostProcess.ToString() : "", true);
                    AddTxnData("AllowBinsDispose", oResultData.AllowBinsDispose != null ? oResultData.AllowBinsDispose.ToString() : "", true);
                    AddTxnData("AllowRejectsRecording", oResultData.AllowRejectsRecording != null ? oResultData.AllowRejectsRecording.ToString() : "", true);
                    AddTxnData("AllowRejectsInProcess", oResultData.AllowRejectsInProcess != null ? oResultData.AllowRejectsInProcess.ToString() : "", true);
                    AddTxnData("AllowRejectsPostProcess", oResultData.AllowRejectsPostProcess != null ? oResultData.AllowRejectsPostProcess.ToString() : "", true);
                    AddTxnData("AllowRejectsDispose", oResultData.AllowRejectsDispose != null ? oResultData.AllowRejectsDispose.ToString() : "", true);
                    AddTxnData("AllowDummyQty", oResultData.AllowDummyQty != null ? oResultData.AllowDummyQty.ToString() : "", true);
                    AddTxnData("AllowNumberOfStrips", oResultData.AllowNumberOfStrips != null ? oResultData.AllowNumberOfStrips.ToString() : "", true);
                    AddTxnData("IsTest", oResultData.IsTest != null ? oResultData.IsTest.ToString() : "", true);
                    AddTxnData("StepLogicName", oResultData.StepLogicName != null ? oResultData.StepLogicName.ToString() : "", true);
                    //StepLogicButton.ToolTip = oResultData.StepLogicName
                    AddTxnData("AllowCarrierValidation", oResultData.AllowCarrierValidation != null ? oResultData.AllowCarrierValidation.ToString() : "", true);
                    // Track In Data
                    AddTxnData("AutoSetTrackInQty", oResultData.AutoSetTrackInQty != null ? oResultData.AutoSetTrackInQty.ToString() : "", true);
                    // Track Out Data
                    AddTxnData("AutoSetTrackOutQty", oResultData.AutoSetTrackOutQty != null ? oResultData.AutoSetTrackOutQty.ToString() : "", true);
                    AddTxnData("AllowSplitUnProcessed", oResultData.AllowSplitUnProcessed != null ? oResultData.AllowSplitUnProcessed.ToString() : "", true);
                    // Move Out Data
                    AddTxnData("AutoMoveOut", oResultData.AutoMoveOut != null ? oResultData.AutoMoveOut.ToString() : "", true);
                    AddTxnData("AutoSetMoveOutQty", oResultData.AutoSetMoveOutQty != null ? oResultData.AutoSetMoveOutQty.ToString() : "", true);
                    AddTxnData("WIPDataExists", oResultData.WIPDataExists != null ? oResultData.WIPDataExists.ToString() : "", true);
                    AddTxnData("SamplingWIPDataExists", oResultData.SamplingWIPDataExists != null ? oResultData.SamplingWIPDataExists.ToString() : "", true);
                    AddTxnData("ss_EProcedureExists", oResultData.ss_EProcedureExists != null ? oResultData.ss_EProcedureExists.ToString() : "", true);
                    //} // if (iCurrentLotCount == 0)

                    //-----------------------------------------------------
                    // Store information that have been requested
                    //-----------------------------------------------------
                    Info oInfo = WIPMainRequest.Info;
                    WIPMain_Info oServiceInfo = oInfo as WIPMain_Info;
                    if (oServiceInfo.WIPFlagSelection != null)
                        AddTxnData("WIPFlagSelection", oResultData.WIPFlagSelection != null ? oResultData.WIPFlagSelection.ToString() : "", true);

                    if (oServiceInfo.MaxTrackInQty != null)
                        AddTxnData("MaxTrackInQty", oResultData.MaxTrackInQty != null ? oResultData.MaxTrackInQty.ToString() : "", true);

                    if (oServiceInfo.SourceEquipmentSelection != null)
                    {
                        if (oResultData.SourceEquipmentSelection != null)
                        {
                            TrackInLotSourceEquipment[] oTrackInLotSourceEquipment = new TrackInLotSourceEquipment[oResultData.SourceEquipmentSelection.Length];
                            int iEquipmentIndex = 0;
                            foreach (TrackInLotSourceEquipment oSourceEquipment in oResultData.SourceEquipmentSelection)
                            {
                                oTrackInLotSourceEquipment[iEquipmentIndex] = new TrackInLotSourceEquipment();
                                oTrackInLotSourceEquipment[iEquipmentIndex] = oSourceEquipment;
                                oTrackInLotSourceEquipment[iEquipmentIndex].Qty = 0;

                                iEquipmentIndex++;
                            }

                            if (_gridSourceEquipmentField != null)
                            {
                                (_gridSourceEquipmentField.GridContext as BoundContext).Data = oTrackInLotSourceEquipment;
                                _gridSourceEquipmentField.BoundContext.LoadData();
                            }
                        }

                    } //  if (oServiceInfo.SourceEquipment != null)

                    if (oServiceInfo.scsLoadPortSelection != null)
                    {
                        if (GetTxnData("WIPFlagSelection") == "1" || WIPFlag == "1")
                        {

                            // clear the TrackInEquipment selection data
                            _ndoLoadPort.ClearSelectionValues();

                            if (oResultData.scsLoadPortSelection != null)
                            {
                                // set the selection values
                                CWC.NamedObject _ndoTrackInEquipTemp = _ndoLoadPort;
                                SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoTrackInEquipTemp, oResultData.scsLoadPortSelection);

                                // set the selected 
                                if (oResultData.scsLoadPortSelection.Count() == 1)
                                    _ndoLoadPort.Data = new NamedObjectRef(oResultData.scsLoadPortSelection[0].Name);
                            }
                        }

                    } // if (oServiceInfo.scsLoadPortSelection != null)

                    if (oServiceInfo.scsRequiredLoadPort != null)
                        AddTxnData("RequiredLoadPort", oResultData.scsRequiredLoadPort.ToString(), true);


                    if (oServiceInfo.MaxTrackOutQty != null)
                        AddTxnData("MaxTrackOutQty", oResultData.MaxTrackOutQty != null ? oResultData.MaxTrackOutQty.ToString() : "", true);

                    if (oServiceInfo.MoveOutQty != null)
                        AddTxnData("MoveOutQty", oResultData.MoveOutQty != null ? oResultData.MoveOutQty.ToString() : "", true);

                    if (oResultData.NextSteps != null)
                    {
                        if ((oResultData.WIPFlagSelection.ToString() == "4")
                            || ((oResultData.WIPFlagSelection.ToString() == "3") && (oResultData.AutoMoveOut.ToString().ToUpper() == "TRUE"))
                            )
                        {
                            if (_subTrackOutNextStepField != null)
                            {
                                CWC.NamedSubentity _subTrackOutNextStepsTemp = _subTrackOutNextStepField;
                                SEMI.AppCode.ControlsUtility.NamedSubentityControl_SetSelectionValues(ref _subTrackOutNextStepsTemp, oResultData.NextSteps);

                                // store the name and ID in the dropdown list so that the ID can be retrieved later when transacting since submitting a named subentity ref requires the ID
                                foreach (NamedSubentityRef oNextStep in oResultData.NextSteps)
                                    _ddlNextSteps.DropDownControl.Items.Add(new System.Web.UI.WebControls.ListItem() { Text = oNextStep.Name, Value = oNextStep.ID });
                            }

                            if (_subNextStepField != null)
                            {
                                CWC.NamedSubentity _subNextStepsTemp = _subNextStepField;
                                SEMI.AppCode.ControlsUtility.NamedSubentityControl_SetSelectionValues(ref _subNextStepsTemp, oResultData.NextSteps);

                                // store the name and ID in the dropdown list so that the ID can be retrieved later when transacting since submitting a named subentity ref requires the ID
                                foreach (NamedSubentityRef oNextStep in oResultData.NextSteps)
                                    _ddlNextSteps.DropDownControl.Items.Add(new System.Web.UI.WebControls.ListItem() { Text = oNextStep.Name, Value = oNextStep.ID });
                            }
                        }
                    }

                    if (oResultData.RequiredActivities != null)
                    {
                        if (IsExistsInRequiredActivitiesList("MATERIALS", oResultData.RequiredActivities))
                            AddTxnData("MaterialsSetupRequired", "TRUE", true);

                        if (IsExistsInRequiredActivitiesList("RECIPE", oResultData.RequiredActivities))
                            AddTxnData("RecipeRequired", "TRUE", true);

                        if (IsExistsInRequiredActivitiesList("MASK", oResultData.RequiredActivities))
                            AddTxnData("MaskRequired", "TRUE", true);

                        if (IsExistsInRequiredActivitiesList("TOOL", oResultData.RequiredActivities))
                            AddTxnData("ToolPlanRequired", "TRUE", true);
                    }

                    if (oResultData.DataCollectionPresent != null)
                        AddTxnData("DataCollectionPresent", oResultData.DataCollectionPresent == true ? "TRUE" : "FALSE", true);
                    // For Wafer Sampling 
                    if (oServiceInfo.scsWaferSamplingRequired != null)
                        AddTxnData("RequiredWaferSampling", oResultData.scsWaferSamplingRequired.ToString(), true);
                } // if (WIPMainResult.Value != null)
            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus("SetTxnData::" + ex.Message.ToString(), false));
            }
        } // SetTxnData   

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void SetItems(WIPLotTxnWafersDetails[] Wafers, WIPLotTxnWafersDetails[] SelectionWafers, string WIPFlag, string PrimaryServiceType, string SelectionIdType)
        {
            // --ExecutionPathPlaceHolder
            bool bAddWafer = true;
            string sWIPFlagSelection = GetTxnData("WIPFlagSelection");

            //List<SS_WIPMain_ItemData> oExistingItemList = (_gridItemData.GridContext as BoundContext).Data as List<SS_WIPMain_ItemData>;
            SS_WIPMain_ItemData[] oExistingItemList = (_gridItemData.GridContext as BoundContext).Data as SS_WIPMain_ItemData[];
            List<SS_WIPMain_ItemData> oNewItemList = new List<SS_WIPMain_ItemData>();
            Hashtable htItemsToUpdate = new Hashtable();

            if (oExistingItemList == null)
                oExistingItemList = new SS_WIPMain_ItemData[0];

            foreach (WIPLotTxnWafersDetails oWafer in Wafers)
            {
                bAddWafer = true;

                // check if the selection is by WAFERBATCHID. If so, only add the wafer if the wafer exists in the batch
                if (SelectionIdType == "WAFERBATCHID" && (sWIPFlagSelection == "1" || sWIPFlagSelection == "2" || sWIPFlagSelection == "3" || WIPFlag == "1" || WIPFlag == "2"))
                {
                    if (SelectionWafers != null)
                    {
                        bAddWafer = false;
                        foreach (WIPLotTxnWafersDetails oSelectionWafer in SelectionWafers)
                        {
                            if (oSelectionWafer.Container.Name == oWafer.Container.Name
                                && oSelectionWafer.WaferScribeNumber == oWafer.WaferScribeNumber)
                            {
                                bAddWafer = true;
                                //**// RemainInEquipmentIfPossibleField.CheckControl.Checked = true;
                            }
                        } // foreach (WIPLotTxnWafersDetails oSelectionWafer in SelectionWafers)
                    }// if (SelectionWafers != null)
                }

                if (bAddWafer)
                {
                    // check if the wafer exists in the current data
                    var ExistingItems = from ItemData in oExistingItemList where ItemData.WaferScribeNumber == oWafer.WaferScribeNumber.ToString() select ItemData;

                    if (ExistingItems.Count() == 0)
                    {
                        // add the item to the newItemList
                        SS_WIPMain_ItemData oItemData = new SS_WIPMain_ItemData();
                        oItemData.__Action = "New";
                        oItemData.LotWafersItem = oWafer.LotWafersItem.ID;
                        if (oWafer.WIPLotDetailsWafersItem != null)
                            oItemData.WIPLotDetailsWafersItem = oWafer.WIPLotDetailsWafersItem.ID;
                        oItemData.Container = oWafer.Container.Name;
                        oItemData.WaferScribeNumber = oWafer.WaferScribeNumber.ToString();
                        oItemData.WaferNumber = oWafer.WaferNumber.ToString();
                        oItemData.IsBadWafer = oWafer.IsBadWafer != null ? bool.Parse(oWafer.IsBadWafer.ToString()) : false;
                        oItemData.YieldOffWafer = oWafer.YieldOffWafer != null ? bool.Parse(oWafer.YieldOffWafer.ToString()) : false;
                        oItemData.NDPW = oWafer.NDPW.ToString();
                        oItemData.GoodQty = oWafer.GoodQty.ToString();
                        if (oWafer.scsSlotNumber != null)
                            oItemData.scsSlotNumber = oWafer.scsSlotNumber.ToString();
                        oItemData.scsSamplingEnforced = oWafer.scsSamplingEnforced != null ? bool.Parse(oWafer.scsSamplingEnforced.ToString()) : false;
                        oItemData.scsWaferSamplingMethod = oWafer.scsWaferSamplingMethod != null ? oWafer.scsWaferSamplingMethod.ToString() : null;

                        oNewItemList.Add(oItemData);
                    }
                    else
                    {
                        foreach (SS_WIPMain_ItemData ExistingItem in ExistingItems)
                        {
                            // add the container name to the hashtable
                            if (!htItemsToUpdate.ContainsKey(ExistingItem.WaferScribeNumber.ToString()))
                                htItemsToUpdate.Add(ExistingItem.WaferScribeNumber.ToString(), oWafer);
                        }
                    } // if (ExistingItems.Count() == 0)         
                }
            } // foreach (WIPLotTxnWafersDetails oWafer in Wafers)

            // clone the ExistingItemList and merge with the new ItemList
            List<SS_WIPMain_ItemData> oUpdatedItemList = new List<SS_WIPMain_ItemData>();
            foreach (SS_WIPMain_ItemData oExistingItem in oExistingItemList)
            {
                if (htItemsToUpdate.ContainsKey(oExistingItem.WaferScribeNumber.ToString()))
                {
                    WIPLotTxnWafersDetails oWaferDetail = htItemsToUpdate[oExistingItem.WaferScribeNumber.ToString()] as WIPLotTxnWafersDetails;
                    oExistingItem.__Action = "";
                    oExistingItem.IsBadWafer = oWaferDetail.IsBadWafer != null ? bool.Parse(oWaferDetail.IsBadWafer.ToString()) : false;
                    oExistingItem.YieldOffWafer = oWaferDetail.YieldOffWafer != null ? bool.Parse(oWaferDetail.YieldOffWafer.ToString()) : false;
                    oExistingItem.NDPW = oWaferDetail.NDPW.ToString();
                    oExistingItem.GoodQty = oWaferDetail.GoodQty.ToString();
                }

                oUpdatedItemList.Add(oExistingItem);
            }

            // get all the wafers of the first row container and populate it into Item grid then only populate the others
            List<SS_WIPMain_ItemData> oWafers = new List<SS_WIPMain_ItemData>();
            string sContainer = _dtGridLotDataMasterCopy != null ? _txtSelectionID.Data.ToString() : _gridLotData.GridContext.GetCell(0, "Container").ToString();
            oWafers = oNewItemList.FindAll(oItem => oItem.Container.ToString() == sContainer);
            foreach (SS_WIPMain_ItemData wafer in oWafers)
                oUpdatedItemList.Add(wafer);

            foreach (SS_WIPMain_ItemData oNewItem in oNewItemList)
                if (oNewItem.Container.ToString() != sContainer)
                    oUpdatedItemList.Add(oNewItem);

            (_gridItemData.GridContext as BoundContext).Data = oUpdatedItemList.ToArray();
            _gridItemData.BoundContext.LoadData();
            CamstarWebControl.SetRenderToClient(_gridItemData);
        }
        //---------------------------------------------------
        // Set items Cheked/Unchecked for wafer sampling step
        //---------------------------------------------------
        private void SetItems_WaferSamplingSelectionState(WIPLotTxnWafersDetails[] oWafers = null)
        {
            _gridItemData.GridContext.PostBackOnSelect = true;

            SS_WIPMain_ItemData[] oItems = (_gridItemData.GridContext as BoundContext).Data as SS_WIPMain_ItemData[];
            if (oItems != null)
            {
                int iRowId = 0;
                List<string> sRowIds = new List<string>();
                foreach (SS_WIPMain_ItemData oItem in oItems)
                {
                    if (oItem.__Action == "New")
                    {
                        oItem.__Action = "";
                        string sRowId = iRowId.ToString().PadLeft(6, '0');
                        sRowIds.Add(sRowId);
                    }
                    iRowId++;
                }
                (_gridItemData.GridContext as BoundContext).Data = oItems;
                _gridItemData.BoundContext.LoadData();
                iRowId = 0;
                foreach (SS_WIPMain_ItemData oItem in oItems)
                {
                    string sRowId = iRowId.ToString().PadLeft(6, '0');
                    _gridItemData.Action_SelectRow(sRowId, "deselect");
                    if (oWafers != null)
                    {
                        foreach (WIPLotTxnWafersDetails oWafer in oWafers)
                        {
                            if (oWafer.Container.Name == oItem.Container.ToString() && oWafer.WaferScribeNumber == oItem.WaferScribeNumber)
                            {
                                _gridItemData.Action_SelectRow(sRowId, "select");
                            }
                        }
                    }
                    iRowId++;
                }

                CamstarWebControl.SetRenderToClient(_gridItemData);
            }
        }
        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void SetItems_SelectionState(bool IsSelected)
        {
            // --ExecutionPathPlaceHolder
            SS_WIPMain_ItemData[] oItems = (_gridItemData.GridContext as BoundContext).Data as SS_WIPMain_ItemData[];
            if (oItems != null)
            {
                List<string> sRowIds = new List<string>();
                int iRowId = 0;
                foreach (SS_WIPMain_ItemData oItem in oItems)
                {
                    string sRowId = iRowId.ToString().PadLeft(6, '0');
                    if (oItem.__Action == "New")
                    {
                        sRowIds.Add(sRowId);
                        oItem.__Action = "";
                    }
                    iRowId++;
                }

                (_gridItemData.GridContext as BoundContext).Data = oItems;
                _gridItemData.BoundContext.LoadData();

                if (IsSelected)
                {
                    foreach (string sRowId in sRowIds)
                        _gridItemData.Action_SelectRow(sRowId, "select");
                }
            }
        }

        //---------------------------------------------------
        // SetRequiredActivities
        //---------------------------------------------------
        private void SetRequiredActivities(Primitive<string>[] RequiredActivities)
        {
            // --ExecutionPathPlaceHolder
            try
            {
                if (RequiredActivities != null)
                {
                    DataTable activitiesDataTable = new DataTable();
                    activitiesDataTable.Columns.Add("Activities", typeof(String));
                    for (int i = 0; i < RequiredActivities.Count(); i++)
                    {
                        DataRow dtRow = activitiesDataTable.NewRow();
                        dtRow.SetField("Activities", RequiredActivities[i].Value);
                        activitiesDataTable.Rows.Add(dtRow);
                    }
                    JQDataGrid _gridRequiredActivities = Page.FindCamstarControl("Main_RequiredActivitiesGrid") as JQDataGrid;
                    _gridRequiredActivities.ClearData();
                    SEMI.AppCode.GridUtility.ItemListGrid_SetColumns(this, activitiesDataTable, _gridRequiredActivities.ID);
                    SEMI.AppCode.GridUtility.ItemListGrid_BindDataTable(this, activitiesDataTable, ref _gridRequiredActivities);
                }
            }
            catch (Exception ex) //Catch errors
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }

        } //SetRequiredActivities

        //-----------------------------------------
        //
        //-----------------------------------------
        public virtual void ClearControls(int ClearFlag = 0, bool SkipDisplayMessage = false, bool ClearResourceWPControls = true)
        {
            // --ExecutionPathPlaceHolder
            try
            {
                if (!SkipDisplayMessage)
                    Page.StatusBar.ClearMessage();

                if (ClearFlag <= 10)
                {
                    //Clear Process Types Controls
                    _rdbMoveInRadioButton.RadioControl.Checked = false;
                    _rdbMoveInRadioButton.Enabled = false;
                    _rdbTrackInRadioButton.RadioControl.Checked = false;
                    _rdbTrackInRadioButton.Enabled = false;
                    _rdbTrackOutRadioButton.RadioControl.Checked = false;
                    _rdbTrackOutRadioButton.Enabled = false;
                    _rdbMoveOutRadioButton.RadioControl.Checked = false;
                    _rdbMoveOutRadioButton.Enabled = false;

                    int controlsCount = Parent.Controls.Count;
                    MatrixWebPart trackinpanel = null;
                    MatrixWebPart trackoutpanel = null;
                    MatrixWebPart moveoutpanel = null;

                    for (int i = 0; i < controlsCount; i++)
                    {
                        if (Parent.Controls[i].ID == "Main_TrackInPanel")
                        {
                            trackinpanel = Parent.Controls[i] as MatrixWebPart;
                        }
                        else if (Parent.Controls[i].ID == "Main_TrackOutPanel")
                        {
                            trackoutpanel = Parent.Controls[i] as MatrixWebPart;
                        }
                        else if (Parent.Controls[i].ID == "Main_MoveOutPanel")
                        {
                            moveoutpanel = Parent.Controls[i] as MatrixWebPart;
                        }
                    }

                    //Clear Track In Controls
                    _txtTrackInQtyField.ClearData();
                    _gridSourceEquipmentField.ClearData();

                    //Clear Track Out Controls
                    _txtTrackOutQtyField.ClearData();
                    _subTrackOutNextStepField.ClearData();
                    _chkRemainInEquipmentField.ClearData();
                    _chkRemainInEquipmentIfPossibleField.ClearData();
                    _chkCancelTrackInField.ClearData();
                    _txtDummyQtyField.ClearData();
                    _txtNumberOfStripsField.ClearData();
                    _chkSplitUnProcessedField.ClearData();
                    _chkSplitUnProcessedAsNewScheduleField.ClearData();
                    _txtSplitUnProcessedQtyField.ClearData();
                    _txtSplitUnProcessedLotIdField.ClearData();

                    //Clear Move Out Controls
                    _txtMoveOutQtyField.ClearData();
                    _subNextStepField.ClearData();

                    //Clear Required Activities
                    JQDataGrid _gridRequiredActivities = Page.FindCamstarControl("Main_RequiredActivitiesGrid") as JQDataGrid;
                    _gridRequiredActivities.ClearData();

                    //Clear Carriers Selection grid
                    _gridCarrierData.ClearData();

                    //Clear Load Port
                    _ndoLoadPort.ClearData();
                    _ndoLoadPort.ClearSelectionValues();
                }

                if (ClearFlag == 0)
                {
                    if (ClearResourceWPControls)
                    {
                        _ndoEquipmentDispatchQuery.ClearData();
                        _ndoEquipment.ClearData();
                        _ndoEmployee.ClearData();
                        _ndoEquipmentGroup.ClearData();
                    }

                    //Hide all action buttons
                    Page.DataContract.SetValueByName("WIPMain_AllowWIPEquipmentSetup", null);
                    Page.DataContract.SetValueByName("WIPMain_AllowWIPEqpMaterialsSetup", null);
                    Page.DataContract.SetValueByName("WIPMain_AllowWIPItemRejects", null);
                    Page.DataContract.SetValueByName("WIPMain_AllowWIPLotRejects", null);
                    Page.DataContract.SetValueByName("WIPMain_AllowWIPData", null);
                    Page.DataContract.SetValueByName("WIPMain_AllowSamplingWIPData", null);
                    Page.DataContract.SetValueByName("WIPMain_AllowWIPLotBins", null);
                    Page.DataContract.SetValueByName("WIPMain_AllowLotPacking", null);
                    Page.DataContract.SetValueByName("WIPMain_AllowInProcessSplit", null);
                    Page.DataContract.SetValueByName("WIPMain_AllowSetTestProgram", null);
                    Page.DataContract.SetValueByName("WIPMain_AllowSorting", null);
                    Page.DataContract.SetValueByName("WIPMain_AllowValidateCarrier", null);
                    Page.DataContract.SetValueByName("WIPMain_AllowCheckSheet", null);
                    Page.DataContract.SetValueByName("WIPMain_AllowEProcedure", null);
                    SetWIPMainControls();

                    _ddlTxnDataList.ClearData();
                    _ddlTxnDataList.DropDownControl.Items.Clear();
                    _txtCommentsField.ClearData();
                    _txtWIPInstructions.ClearData();
                    _txtSelectionID.ClearData();

                    _envWIPMainInfoEnvelop.SS_ContainersList = null;
                    _envWIPMainInfoEnvelop.SS_DataPacket = null;

                    //Clear selection controls
                    _gridItemData.ClearData();
                    _gridLotData.ClearData();
                    _gridDispatchListLot.ClearData();

                    //Clear process type controls
                    _ndoProcessType.ClearData();
                    _ndoProcessType.ClearSelectionValues();

                    //Clear Check Sheet RDO control
                    _rdoEProcField.ClearData();

                    ViewState[_SPCViewStateIdentifier] = null;

                    SetControls("", "Shutdown", true);

                    //Clear Selected Lot field & data contract member                    
                    //_btnEProcedure.Enabled = false;
                    //_btnCreatePE.Enabled = false;
                }

                // fix for T136256: clear specific data contract members that causes stuff to go screwy when selecting from the lot selection popup
                Page.DataContract.SetValueByName("WIPMain_Equipment_DM", null);

                InitViewStates();
                Page.Session[_PopupTxnViewStateIdentifier] = null;

                _txtSelectionID.Focus();

                //clear the container list
                Page.Session[_WIPMainContainerList] = null;
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            // --ExecutionPathPlaceHolder
            base.PostExecute(status, serviceData);

            bool bSPCAvailable = false;
            string sCompletionMessage = status.Message;

            // SPC stuff
            SPCTxnData[] oSPCTxnData = null;
            if (serviceData is OM.WIPData)
                if ((serviceData as WIPData).SPCTxnDataList != null)
                {
                    oSPCTxnData = (serviceData as WIPData).SPCTxnDataList;
                    if (oSPCTxnData.Length > 0)
                        bSPCAvailable = true;
                }

            if (bSPCAvailable)
            {
                if (oSPCTxnData != null)
                    SetSPCControls(true, oSPCTxnData);
                DisplaySPCChart(oSPCTxnData, status);
            }
            else
            {
                bool bAlertAvailable = false;
                DisplayAlerts(status, out bAlertAvailable, out sCompletionMessage);
            }
            //SPC Stuff         
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public void DisplayAlerts(ResultStatus status, out bool AlertMessageAvailable, out string CompletionMessage)
        {
            // check for alert messages
            string[] sAlertMessages;
            string sCompletionMessage;

            AlertMessageAvailable = false;

            if (status.IsSuccess)
            {
                bool bAlertMsg = SEMI.AppCode.UIUtility.AlertMessagesAvailable(status.Message, out sAlertMessages, out sCompletionMessage);
                AlertMessageAvailable = bAlertMsg;

                status.Message = sCompletionMessage;

                if (bAlertMsg)
                {
                    SEMI.AppCode.DataPacket oData = new DataPacket();
                    oData.IsAlertMessageAvailable = false;
                    oData.AlertMessages = sAlertMessages;
                    oData.ResultStatusMessage = sCompletionMessage;
                    _envWIPMainInfoEnvelop.SS_DataPacket = oData;
                    PopupWIPMessages(false);
                }
            }

            CompletionMessage = status.Message;
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public void DisplayAlerts(string[] AlertMessages)
        {
            SEMI.AppCode.DataPacket oData = new DataPacket();
            oData.IsAlertMessageAvailable = false;
            oData.AlertMessages = AlertMessages;
            oData.ResultStatusMessage = null;
            _envWIPMainInfoEnvelop.SS_DataPacket = oData;
            PopupWIPMessages(false);
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public void SetSPCControls(bool SPCDataAvailable, SPCTxnData[] SPCTxnDataList = null)
        {
            if (SPCDataAvailable)
                ViewState[_SPCViewStateIdentifier] = SPCTxnDataList;
            else
                ViewState[_SPCViewStateIdentifier] = null;
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public void DisplaySPCChart(SPCTxnData[] oSPCTxnData, ResultStatus TxnResultStatus)
        {
            if (oSPCTxnData.Length > 0)
            {
                int intCHeight = 0;
                int intCWidth = 0;

                // set the data envelop data to pass to the popup;            
                SEMI.AppCode.Services.SPCTxn.SPCTxnDataSet SPCTxnDataSet = new SEMI.AppCode.Services.SPCTxn.SPCTxnDataSet();
                foreach (SPCTxnData oSPCTxnDataItem in oSPCTxnData)
                {
                    if (oSPCTxnDataItem.SPCResultFilename != "")
                        SPCTxnDataSet.AddDataListItem(oSPCTxnDataItem);

                    if (int.Parse(oSPCTxnDataItem.ChartHeight.ToString()) > intCHeight)
                        intCHeight = int.Parse(oSPCTxnDataItem.ChartHeight.ToString());

                    if (int.Parse(oSPCTxnDataItem.ChartWidth.ToString()) > intCWidth)
                        intCWidth = int.Parse(oSPCTxnDataItem.ChartWidth.ToString());
                }

                DataPacket SS_DataPacket = new DataPacket();
                if (_envWIPMainInfoEnvelop.SS_DataPacket != null)
                    SS_DataPacket = _envWIPMainInfoEnvelop.SS_DataPacket;

                SS_DataPacket.SPCTxnDataSet = SPCTxnDataSet;

                // check for alert messages
                string[] sAlertMessages;
                string sCompletionMessage;
                // check for alert messages               
                bool bAlertMsg = SEMI.AppCode.UIUtility.AlertMessagesAvailable(TxnResultStatus.Message, out sAlertMessages, out sCompletionMessage);

                if (bAlertMsg)
                    SS_DataPacket.AlertMessages = sAlertMessages;
                SS_DataPacket.ResultStatusMessage = sCompletionMessage;
                SS_DataPacket.IsErrorResultStatusMessage = !(TxnResultStatus.IsSuccess);
                SS_DataPacket.IsAlertMessageAvailable = true;
                SS_DataPacket.AlertMessages = sAlertMessages;

                // set the data packet
                _envWIPMainInfoEnvelop.SS_DataPacket = SS_DataPacket;

                UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[1];
                objLinks[0] = new UIComponentDataContractLink();
                objLinks[0].SourceMember = "WIPMain_DataEnvelopDM";
                objLinks[0].TargetMember = "envelopPopupInDM";

                UIComponentDataContractReturnLink[] objReturnLinks = new UIComponentDataContractReturnLink[1];
                objReturnLinks[0] = new UIComponentDataContractReturnLink();
                objReturnLinks[0].SourceMember = "envelopPopupOutDM";
                objReturnLinks[0].TargetMember = "WIPMain_DataEnvelopDM";

                SEMI.AppCode.Services.SPCTxn.PopupSPCChart(this, objLinks, objReturnLinks, (intCWidth + 110), (intCHeight + 200));
            }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public SPCTxnData[] GetSPCTxnDataViewState()
        {
            SPCTxnData[] oTxnData = null;
            if (ViewState[_SPCViewStateIdentifier] != null)
                oTxnData = ViewState[_SPCViewStateIdentifier] as SPCTxnData[];

            return oTxnData;
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public void OnPopupClose()
        {
            if (Page.Session[_PopupTxnViewStateIdentifier] != null)
            {
                PopupTxnType ptType = new PopupTxnType();
                ptType = (PopupTxnType)Page.Session[_PopupTxnViewStateIdentifier];
                switch (ptType)
                {
                    case PopupTxnType.InProcessSplit:
                    case PopupTxnType.LotReject:
                    case PopupTxnType.ItemReject:
                    case PopupTxnType.LotBins:
                        FetchTxnData(FetchTxnDataEvents.PopupWithQtyChange);
                        break;
                    case PopupTxnType.WIPData:
                        // check the SPCTxnData session variable (set during the WIPData PostExecute event)
                        if (Page.Session[_SPCTxnDataSessionIdentifier] != null)
                        {
                            SPCTxnData[] oSPCTxnData = Page.Session[_SPCTxnDataSessionIdentifier] as SPCTxnData[];
                            Page.Session[_SPCTxnDataSessionIdentifier] = null;
                            Page.DisplayMessage("", true);
                            SetSPCControls(true, oSPCTxnData);
                            DisplaySPCChart(oSPCTxnData, new ResultStatus("", true));
                        }
                        break;
                }
                Page.Session[_PopupTxnViewStateIdentifier] = null;
            }

            // check to see if there is a need to display the status message after closing the alert message popup
            if (Page.DataContract.GetValueByName("WIPMain_DataEnvelopDM") != null)
            {
                _envWIPMainInfoEnvelop.SS_DataPacket = Page.DataContract.GetValueByName("WIPMain_DataEnvelopDM") as DataPacket;
                string sStatusMessage = _envWIPMainInfoEnvelop.SS_DataPacket.ResultStatusMessage;

                _envWIPMainInfoEnvelop.SS_DataPacket = null;
                Page.DataContract.SetValueByName("WIPMain_DataEnvelopDM", null);

                Page.DisplayMessage(sStatusMessage, true);
            }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public virtual void PopupWIPMessages(bool EndResponse = false)
        {
            Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
            objAction.PageName = "SS_AlertMessagePopupVP";

            objAction.FrameLocation = new UIFloatingPageLocation();
            objAction.FrameLocation.Width = 430;
            objAction.FrameLocation.Height = 230;
            objAction.EndResponse = EndResponse;
            objAction.ShowButtons = false;

            UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[1];
            objLinks[0] = new UIComponentDataContractLink();
            objLinks[0].SourceMember = "WIPMain_DataEnvelopDM";
            objLinks[0].TargetMember = "envelopAlertInDM";
            objAction.DataContractMap = new UIComponentDataContractMap();
            objAction.DataContractMap.Links = objLinks;

            UIComponentDataContractReturnLink[] objReturnLinks = new UIComponentDataContractReturnLink[1];
            objReturnLinks[0] = new UIComponentDataContractReturnLink();
            objReturnLinks[0].SourceMember = "envelopAlertOutDM";
            objReturnLinks[0].TargetMember = "WIPMain_DataEnvelopDM";
            objAction.DataContractReturnMap = new UIComponentDataContractReturnMap();
            objAction.DataContractReturnMap.ReturnLinks = objReturnLinks;

            SEMI.AppCode.UIUtility.SetHorizonAlertPopupFrameLocation(this, objAction);
        }  // ShowAlerts

        //-----------------------------------------
        //
        //-----------------------------------------
        public void GetWIPMessages()
        {
            // get the session and user profile
            var fs = FrameworkManagerUtil.GetFrameworkSession();

            WIPMainService oService = new WIPMainService(fs.CurrentUserProfile);
            WIPMain oServiceData = new WIPMain();
            WIPMain_Info oServiceInfo = new WIPMain_Info();
            WIPMain_Request oRequest = new WIPMain_Request();
            WIPMain_Result oResult = new WIPMain_Result();

            if (_txtSelectionID.Data != null)
            {
                oServiceData.SelectionId = _txtSelectionID.Data.ToString();
            }

            if (GetWIPFlag() != "")
                oServiceData.WIPFlag = int.Parse(GetWIPFlag());

            oServiceInfo.WIPInstruction = FieldInfoUtil.RequestValue();

            oRequest = new WIPMain_Request();
            oRequest.Info = oServiceInfo;

            ResultStatus oResultStatus = new ResultStatus();
            oResultStatus = oService.ResolveSelectionId(oServiceData, oRequest, out oResult);

            if (oResultStatus.IsSuccess)
            {
                if (oResult.Value != null)
                    if (oResult.Value.WIPInstruction != null)
                    {
                        SEMI.AppCode.DataPacket oData = new DataPacket();
                        oData.IsAlertMessageAvailable = true;
                        string[] sMessages = new string[1] { oResult.Value.WIPInstruction.ToString() };
                        oData.AlertMessages = sMessages;

                        _envWIPMainInfoEnvelop.SS_DataPacket = oData;

                        PopupWIPMessages();
                    }
            }
        } // GetWIPMessages

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private bool IsExistsInRequiredActivitiesList(string SearchValue, Camstar.WCF.ObjectStack.Primitive<string>[] RequiredActivities)
        {
            // --ExecutionPathPlaceHolder
            bool bIsExists = false;

            foreach (string sActivity in RequiredActivities)
            {
                if (sActivity.ToUpper().Contains(SearchValue.ToUpper()))
                {
                    bIsExists = true;
                    break;
                }
            }

            return bIsExists;
        } // IsExistsInRequiredActivitiesList

        //---------------------------------------------------
        // IsCalledViaResourceLayoutView
        //---------------------------------------------------
        private bool IsCalledViaResourceLayoutView()
        {
            if (Page.DataContract.GetValueByName("WIPMain_SelectionId") != null)
                return true;
            else
                return false;
        }

        //---------------------------------------------------
        // Txn to check if there is a need for the confirmation window (currently only used together with Process Timers)
        //---------------------------------------------------
        public void ConfirmSubmit(CustomActionEventArgs e)
        {
            try
            {
                //Initialize the Service Data
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                //string ServiceType = this.PrimaryServiceType;
                string sServiceType = _txtPrimaryServiceType.Data != null ? _txtPrimaryServiceType.Data.ToString() : "WIPMain";
                var Svc = new WSDataCreator().CreateService(sServiceType, profile);
                var SvcData = WCFObject.CreateObject(sServiceType) as ICreator;
                var SvcInfo = WCFObject.CreateObject(sServiceType + "_Info") as ICreator;
                var ReqData = WCFObject.CreateObject(sServiceType + "_Request") as ICreator;
                var ResData = WCFObject.CreateObject(sServiceType + "_Result") as ICreator;
                Result result = null;

                int iLotsPerBatch = 0;
                string sWIPFlag = GetWIPFlag();
                iLotsPerBatch = _gridLotData.GridContext.GetTotalRows();

                //Add the Containers and Wafers
                ContainerRef[] containerNames = new ContainerRef[iLotsPerBatch];
                for (int i = 0; i < iLotsPerBatch; i++)
                {
                    containerNames[i] = new ContainerRef();
                    containerNames[i].Name = _gridLotData.GridContext.GetCell(i, "Container").ToString();
                }

                SvcData.SetValue("Containers", containerNames);
                SvcData.SetValue("WIPFlag", Convert.ToInt32(sWIPFlag));

                //Add common data to submit
                if (_ndoEmployee.Data != null)
                    SvcData.SetValue("Employee", _ndoEmployee.Data);

                if (_txtComputerName.Data != null)
                    SvcData.SetValue("ComputerName", _txtComputerName.Data);

                SvcInfo.SetValue("scsIsTxnConfirmationReq", new Info(true));
                ReqData.SetValue("Info", SvcInfo);

                //Submit the transaction
                ResultStatus Results = Svc.GetEnvironment(SvcData as DCObject, ReqData as Request, out result);
                if (Results.IsSuccess)
                {
                    bool bConfirmationRequired = false;
                    string sCompletionMessage = Results.Message;

                    if ((result.Value as WIPMain).scsIsTxnConfirmationReq != null)
                        bConfirmationRequired = bool.Parse((result.Value as WIPMain).scsIsTxnConfirmationReq.ToString());

                    if (bConfirmationRequired)
                        ScriptManager.RegisterStartupScript(Page.Form, this.GetType(), "myConfirm", "EquipmentWIPMain_TxnConfirmation();", true);
                    else
                        SubmitTransactions(e);
                }
                else
                {
                    e.Result = Results;
                    //this.DisplayMessage(Results);
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
        private class SS_WIPMain_ItemData
        {
            private string sAction;
            private string sLotWafersItem;
            private string sWIPLotDetailsWafersItem;
            private string sWaferScribeNumber;
            private string sContainer;
            private string sNDPW;
            private string sGoodQty;
            private string sWaferNumber;
            private bool bIsBadWafer;
            private bool bYieldOffWafer;
            private string sSlotNumber;
            private bool bSamplingEnforced;
            private string bscsSamplingMethod;

            public SS_WIPMain_ItemData()
            { }

            public string __Action
            {
                get { return sAction; }
                set { sAction = value; }
            }

            public string LotWafersItem
            {
                get { return sLotWafersItem; }
                set { sLotWafersItem = value; }
            }

            public string WIPLotDetailsWafersItem
            {
                get { return sWIPLotDetailsWafersItem; }
                set { sWIPLotDetailsWafersItem = value; }
            }

            public string WaferScribeNumber
            {
                get { return sWaferScribeNumber; }
                set { sWaferScribeNumber = value; }
            }

            public string Container
            {
                get { return sContainer; }
                set { sContainer = value; }
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

            public string WaferNumber
            {
                get { return sWaferNumber; }
                set { sWaferNumber = value; }
            }

            public bool IsBadWafer
            {
                get { return bIsBadWafer; }
                set { bIsBadWafer = value; }
            }

            public bool YieldOffWafer
            {
                get { return bYieldOffWafer; }
                set { bYieldOffWafer = value; }
            }

            public string scsSlotNumber
            {
                get { return sSlotNumber; }
                set { sSlotNumber = value; }
            }

            public bool scsSamplingEnforced
            {
                get { return bSamplingEnforced; }
                set { bSamplingEnforced = value; }
            }

            public string scsWaferSamplingMethod
            {
                get { return bscsSamplingMethod; }
                set { bscsSamplingMethod = value; }
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void ExecutionPathOutput(string sTextValue)
        {
            try
            {
                // ExecutionPathOutput(System.Reflection.MethodBase.GetCurrentMethod().Name + " (" + new System.Diagnostics.StackFrame(1).GetMethod().Name + ")");
                // System.Reflection.MethodBase.GetCurrentMethod().Name + " (" + new System.Diagnostics.StackFrame(1).GetMethod().Name + ")"
                if (false)
                {
                    System.IO.StreamWriter oWriter = new System.IO.StreamWriter(@"C:\temp\EquipmentWIPMain.log", true);
                    oWriter.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + sTextValue);
                    oWriter.Close();
                    oWriter.Dispose();
                }
            }
            catch (Exception ex)
            {
            }
        }
        //---------------------------------------------------
        // GetEsigRequirementMaintenanceTxn Codes
        //---------------------------------------------------
        protected virtual void GetEsigRequirementMaintenanceTxn(string servName, string eSigMaintAction, Personalization.DependsOnItem[] dependencies)
        {

            if (!string.IsNullOrEmpty(eSigMaintAction))
            {
                WSDataCreator creator = new WSDataCreator();
                IWCFService service = creator.CreateService(servName, FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                Request request = WCFObject.CreateObject(servName + "_Request") as Request;
                WCFObject wcfo = new WCFObject(request);
                Info info = WCFObject.CreateObject(servName + "_Info") as Info;
                int iLotsPerBatch = 0;

                wcfo.SetValue("Info", info);
                wcfo.SetValue("Info.ESigRequirement", new Info(true));
                wcfo.SetValue("Info.ESigDetails", new ESigServiceDetail_Info { RequestValue = true });
                wcfo.SetValue("Info.ss_ESigProcessTimerDetails", new ESigProcessTimerServiceDetail_Info { RequestValue = true });
                var txn = creator.CreateServiceData(servName);
                (txn as WIPMain).WIPFlag = Convert.ToInt32(GetWIPFlag());
                //Determine the number of lots to transact in a batch
                iLotsPerBatch = _gridLotData.GridContext.GetTotalRows();

                //Add the Containers
                ContainerRef[] containerNames = new ContainerRef[iLotsPerBatch];
                for (int i = 0; i < iLotsPerBatch; i++)
                {
                    containerNames[i] = new ContainerRef();
                    containerNames[i].Name = _gridLotData.GridContext.GetCell(i, "Container").ToString();
                }
                (txn as WIPMain).Containers = containerNames;

                //Set the Process Type, WIP Flag and Comments
                if (_ndoProcessType.Data != null)
                {
                    NamedObjectRef processType = new NamedObjectRef();
                    processType.Name = _ndoProcessType.Data.ToString();

                    (txn as WIPMain).ProcessType = processType;
                }

                //Add common data to submit
                if (_ndoEmployee.Data != null)
                    (txn as WIPMain).Employee = new NamedObjectRef(_ndoEmployee.Data.ToString());

                if (_txtComputerName.Data != null)
                    (txn as WIPMain).ComputerName = _txtComputerName.Data.ToString();

                if (_txtCommentsField.Data != null)
                    (txn as WIPMain).Comments = _txtCommentsField.Data.ToString();

                var parameters = WCFObject.CreateObject(string.Format("{0}_LoadESigDetails_Parameters", servName)) as Parameters;
                WCFObject wcfoParams = new WCFObject(parameters);
                wcfoParams.SetValue("ESigMaintAction", eSigMaintAction);


                Result res = null;
                ResultStatus status = (service as IShopFloorBase).LoadESigDetails(txn, parameters, request, out res);

                if (status.IsSuccess && res.Value != null)
                {
                    var wcfoRes = new WCFObject(res);
                    var EsigDetails = wcfoRes.GetValue("Value.ESigDetails") as ESigServiceDetail[];
                    var EsigProcessTimerServiceDetails = wcfoRes.GetValue("Value.ss_ESigProcessTimerDetails") as ESigProcessTimerServiceDetail[];
                    bool IsSignatureRequired = false;
                    if (EsigDetails != null)
                        foreach (var EsigDetail in EsigDetails)
                        {
                            if (EsigDetail.CaptureCount.Value < EsigDetail.SignatureCount.Value)
                            {
                                IsSignatureRequired = true;
                                break;
                            }
                        }
                    if (EsigProcessTimerServiceDetails != null)
                        foreach (var EsigProcessTimerServiceDetail in EsigProcessTimerServiceDetails)
                        {
                            if (EsigProcessTimerServiceDetail.ESigProcessTimerDtls != null)
                                foreach (var EsigDetail in EsigProcessTimerServiceDetail.ESigProcessTimerDtls)
                                {
                                    if (EsigDetail.CaptureCount.Value < EsigDetail.SignatureCount.Value)
                                    {
                                        IsSignatureRequired = true;
                                        break;
                                    }
                                }
                        }
                    if ((EsigDetails != null || EsigProcessTimerServiceDetails != null) && IsSignatureRequired)
                    {
                        List<ESigProcessTimerServiceDetail> abc = EsigProcessTimerServiceDetails.ToList();
                        Page.PortalContext.LocalSession["EsigPostback"] = true;

                        Page.OpenContainerPopupESigCapture(PrimaryServiceType, Tuple.Create(EsigDetails, EsigProcessTimerServiceDetails));

                    }

                }
                else if (status.IsSuccess == false)
                {
                    this.DisplayMessage(status);
                }


            }
        }

    } // public class SS_WIPMain
}