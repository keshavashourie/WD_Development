/* Copyright 2025 Siemens */
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
using SWC = System.Web.UI.WebControls;

/// <summary>
/// Summary description for SS_WIPMain
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_WIPMainFrameworkR2 : scsShopfloorBase
    {
        // WIPMain_ControllerWP Controls
        protected CWC.BooleanSwitch _bsLotsEquipmentSwitch { get { return Page.FindCamstarControl("WIPMain_LotsEquipmentSwitch") as CWC.BooleanSwitch; } }
        protected CWC.RadioButton _rdbLotSelect { get { return Page.FindCamstarControl("WIPMain_LotSelectRadio") as CWC.RadioButton; } }
        protected CWC.RadioButton _rdbResourceSelect { get { return Page.FindCamstarControl("WIPMain_ResourceSelectRadio") as CWC.RadioButton; } }
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("WIPMain_SelectionId") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("WIPMain_EmployeeR2") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoProcessType { get { return Page.FindCamstarControl("WIPMain_ProcessType") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEquipment { get { return Page.FindCamstarControl("WIPMain_Equipment") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoLoadPort { get { return Page.FindCamstarControl("WIPMain_scsLoadPort") as CWC.NamedObject; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("WIPMain_ComputerName") as CWC.TextBox; } }

        protected CWC.TextBox _txtWIPFlag { get { return Page.FindCamstarControl("WIPMain_WIPFlag") as CWC.TextBox; } }
        protected CWC.TextBox _txtWIPFlagTxn { get { return Page.FindCamstarControl("WIPMain_WIPFlagTxn") as CWC.TextBox; } }
        protected CWC.DropDownList _ddlTxnDataList { get { return Page.FindCamstarControl("WIPMain_TxnDataList") as CWC.DropDownList; } }
        protected CWC.TextBox _txtWIPMainTxnType { get { return Page.FindCamstarControl("WIPMain_TxnType") as CWC.TextBox; } }
        protected CWC.TextBox _txtRejectsSvcType { get { return Page.FindCamstarControl("WIPMain_RejectsSvcType") as CWC.TextBox; } }
        protected CWC.TextBox _txtDataSvcType { get { return Page.FindCamstarControl("WIPMain_DataSvcType") as CWC.TextBox; } }
        protected CWC.TextBox _txtBinningSvcType { get { return Page.FindCamstarControl("WIPMain_BinningSvcType") as CWC.TextBox; } }
        protected CWC.TextBox _txtEqpSetupSvcType { get { return Page.FindCamstarControl("WIPMain_EqpSetupSvcType") as CWC.TextBox; } }

        protected CWC.TextBox _txtWIPInstructions { get { return Page.FindCamstarControl("WIPMain_WIPInstructions") as CWC.TextBox; } }
        protected CWC.TextBox _txtPrimaryServiceType { get { return Page.FindCamstarControl("WIPMain_PrimaryServiceType") as CWC.TextBox; } }
        protected CWC.Button _btnLotSelectPopup { get { return Page.FindCamstarControl("WIPMain_LotSelectPopup") as CWC.Button; } }
        protected CWC.Button _btnResourcetSelectPopup { get { return Page.FindCamstarControl("WIPMain_ResourceSelectPopup") as CWC.Button; } }

        protected CWC.CheckBox _chkSPCDataAvailable { get { return Page.FindCamstarControl("WIPMain_SPCDataAvailable") as CWC.CheckBox; } }
        protected CWC.DropDownList _ddlNextSteps { get { return Page.FindCamstarControl("WIPMain_NextSteps") as CWC.DropDownList; } }
        protected DataEnvelopControl _envWIPMainLotList { get { return Page.FindCamstarControl("WIPMain_LotList") as DataEnvelopControl; } }

        protected CWC.TextBox _txtWebPartInit { get { return Page.FindCamstarControl("WIPMain_WebPartInit") as CWC.TextBox; } }

        // WIPMain_LotItemWP Controls
        protected JQTabContainer _tabLotItem { get { return Page.FindCamstarControl("WIPMain_LotItemTab") as JQTabContainer; } }
        protected JQDataGrid _gridLotData { get { return Page.FindCamstarControl("scsWIPMain_LotDataGrid") as JQDataGrid; } }
        protected JQDataGrid _gridItemData { get { return Page.FindCamstarControl("scsWIPMain_ItemDataGrid") as JQDataGrid; } }
        protected CWC.TextBox _txtSelectedContainerID { get { return Page.FindCamstarControl("WIPMain_LotItem_ContainerId") as CWC.TextBox; } }
        protected CWC.TextBox _txtContainerToDelete { get { return Page.FindCamstarControl("WIPMain_LotItem_ContainerToDelete") as CWC.TextBox; } }

        // WIPMain_TxnTabWP Controls
        protected JQTabContainer _tabTxn { get { return Page.FindCamstarControl("WIPMain_TxnTab") as JQTabContainer; } }

        // WIPMain_InfoWP Controls
        protected DataEnvelopControl _envWIPMainInfoEnvelop { get { return Page.FindCamstarControl("WIPMain_Info_Envelop") as DataEnvelopControl; } }

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
        protected CWC.TextBox _txtSamplingRejectQty { get { return Page.FindCamstarControl("WIPMain_SamplingRejectQty") as CWC.TextBox; } }
        protected CWC.RevisionedObject WIPMain_Workflow { get { return Page.FindCamstarControl("WIPMain_Workflow") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject WIPMain_WorkflowStep { get { return Page.FindCamstarControl("WIPMain_WorkflowStep") as CWC.RevisionedObject; } }

        // class level variables/properties
        protected enum FetchTxnDataEvents { SelectionIdEntry, ProcessTypeChange, TrackOutEquipmentChange, WIPFlagChange, PopupWithQtyChange, MainContainerChange, NextStepsChange };
        protected enum WIPFlagTxnTypes { MOVEIN = 5, TRACKIN = 1, TRACKOUT = 2, MOVEOUT = 4, NONE = 0 }

        // WIP Main Activity
        protected JQDataGrid _gridRequiredActivitesEx { get { return Page.FindCamstarControl("RequiredActivitiesEx") as JQDataGrid; } }

        // Check Sheet controls
        protected CWC.RevisionedObject _rdoEProcField { get { return Page.FindCamstarControl("WIPMain_HiddenElectronicProcedure") as CWC.RevisionedObject; } }

        // WIP Main txn ribbon
        protected CWC.PagePanel _MoveInTile { get { return Page.FindCamstarControl("MoveInTile") as CWC.PagePanel; } }
        protected CWC.PagePanel _TrackInTile { get { return Page.FindCamstarControl("TrackInTile") as CWC.PagePanel; } }
        protected CWC.PagePanel _TrackOutTile { get { return Page.FindCamstarControl("TrackOutTile") as CWC.PagePanel; } }
        protected CWC.PagePanel _MoveOutTile { get { return Page.FindCamstarControl("MoveOutTile") as CWC.PagePanel; } }

        protected MatrixWebPart wpLotInfo_LotGridWP
        {
            get
            {
                MatrixWebPart wp = null;
                int iParentControlsCount = Parent.Controls.Count;
                for (int z = 0; z < iParentControlsCount; z++)
                {
                    if (Parent.Controls[z].ID == "WIPMain_LotItemWP_LotGridWP")
                        wp = Parent.Controls[z] as MatrixWebPart;
                }
                return wp;
            }
        }

        protected const string _dynamicTypeName = "__WIPMainFramework_LotData_";
        protected const string _SPCViewStateIdentifier = "__WIPMainFramework_SPCTxnData";
        protected const string _PostExecuteServicesViewStateIdentifier = "__WIPMainFramework_PostExecuteServices";
        protected const string _WIPMainContainerList = "_ContainerList";
        protected const string _SPCTxnDataSessionIdentifier = "_SPCTxnDataList";
        protected const string _ShowSPCChartSessionIdentifier = "_ShowSPCChart";

        protected Hashtable _postExecuteServices = new Hashtable();

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            // --ExecutionPathPlaceHolder
            base.OnLoad(e);

            _txtSelectionId.DataChanged += new EventHandler(_txtSelectionId_DataChanged);

            // WIPMain_TrackInTrackOutWP             
            _bsLotsEquipmentSwitch.RadioListControl.SelectedIndexChanged += new EventHandler(LotsEquipmentSwitch_IndexChanged);
            _rdbMoveInRadioButton.RadioControl.CheckedChanged += new EventHandler(MoveInRadioButtonControl_CheckedChanged);
            _rdbTrackInRadioButton.RadioControl.CheckedChanged += new EventHandler(TrackInRadioButtonControl_CheckedChanged);
            _rdbTrackOutRadioButton.RadioControl.CheckedChanged += new EventHandler(TrackOutRadioButtonControl_CheckedChanged);
            _rdbMoveOutRadioButton.RadioControl.CheckedChanged += new EventHandler(MoveOutRadioButtonControl_CheckedChanged);

            _ndoEquipment.DataChanged += new EventHandler(_ndoEquipment_DataChanged);
            _ndoProcessType.DataChanged += new EventHandler(_ndoProcessType_DataChanged);
            _txtSelectedContainerID.DataChanged += new EventHandler(_txtSelectedContainerID_DataChanged);
            _subNextStepField.DataChanged += new EventHandler(_subNextStepField_DataChanged);
            _subTrackOutNextStepField.DataChanged += new EventHandler(_subTrackOutNextStepField_DataChanged);

            //Wafer Sampling
            _gridItemData.RowSelected += new JQGridEventHandler(ItemsGrid_RowSelected);

            // if the WIPFlag = 4 then disallow selection of the Wafers
            if (GetWIPFlag() == "4")
                _gridItemData.GridContext.RowSelectionMode = JQGridSelectionMode.Disable;

            //Item Data Grid PostBackOnSelect will only True when wafer sampling required
            _gridItemData.GridContext.PostBackOnSelect = GetTxnData("RequiredWaferSampling").ToUpper() == "TRUE";

            if (!Page.IsPostBack)
            {
                _txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);

                _txtPrimaryServiceType.Data = Page.PrimaryServiceType.ToString();

                string sEquipmentSetupServiceType = Page.PrimaryServiceType.ToString();
                sEquipmentSetupServiceType = sEquipmentSetupServiceType.Replace("WIPMain", "EquipmentSetup");
                sEquipmentSetupServiceType = sEquipmentSetupServiceType.Replace("SubLot", "");
                sEquipmentSetupServiceType = sEquipmentSetupServiceType.Replace("MotherLot", "");
                sEquipmentSetupServiceType = sEquipmentSetupServiceType.Replace("Carrier", "");
                sEquipmentSetupServiceType = sEquipmentSetupServiceType.Replace("FinalTest", "");
                sEquipmentSetupServiceType = sEquipmentSetupServiceType.Replace("Test", "");

                _txtEqpSetupSvcType.Data = sEquipmentSetupServiceType;

                // disable all the Actions                    
                DisableActionIcons();

                _rdbLotSelect.RadioControl.Checked = true;
                SetSelectionMode("LOT");

                _rdbTrackInRadioButton.Enabled = false;
                _rdbTrackOutRadioButton.Enabled = false;
            }


            if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                OnPopupClose();

            if (IsCalledViaResourceLayoutView())
            {
                if (_txtSelectionId.Data != null && FetchTxnData(FetchTxnDataEvents.SelectionIdEntry))
                {
                    _txtSelectionId.ClearData();
                    _txtSelectionId.Focus();
                }
            }
			
            if (!Page.IsPostBack)
            {
                DisableActionIcons();
                //get containers from Container Search screen
                if (Page.Session["selectedContainers"] != null)
                {
                    Page.DataContract.SetValueByName("WIPMain_SelectionId", "Test");
                    string[] sContainersList = Page.Session["selectedContainers"] as string[];
                    foreach (string container in sContainersList)
                    {
                        _txtSelectionId.TextControl.Text = container;
                        _txtSelectionId_DataChanged(null, null);
                    }
                }
                Page.Session.Remove("selectedContainers");

                Page.PortalContext.LocalSession["EsigPostback"] = false;

                if (Page.DataContract.GetValueByName("WIPMain_RedirectSelectedLot") != null)
                {
                    string container = Page.DataContract.GetValueByName("WIPMain_RedirectSelectedLot") as string;
                    _txtSelectionId.TextControl.Text = container;
                    if (_txtSelectionId.Data != null && FetchTxnData(FetchTxnDataEvents.SelectionIdEntry))
                    {
                        _txtSelectionId.ClearData();
                        _txtSelectionId.Focus();
                    }
                }

                if (Page.DataContract.GetValueByName("WIPMain_RedirectSelectedEqp") != null)
                {
                    if (_ndoEquipment.Data == null && GetWIPFlag() == "1")
                    {
                        string equipment = Page.DataContract.GetValueByName("WIPMain_RedirectSelectedEqp") as string;
                        _ndoEquipment.Data = equipment;
                    }
                }
            }
            else
            {
                Page.DataContract.SetValueByName("WIPMain_RedirectSelectedLot", null);
                Page.DataContract.SetValueByName("WIPMain_RedirectSelectedEqp", null);
            }

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

        private void _txtSelectedContainerID_DataChanged(object sender, EventArgs e)
        {
            if (Page.DataContract.GetValueByName("WIPMain_RedirectSelectedLot") != null)
            {
                return;
            }
            if (_ndoEquipment.Data != null)
                FetchTxnData(FetchTxnDataEvents.MainContainerChange);
        }

        private void LotsEquipmentSwitch_IndexChanged(object sender, EventArgs e)
        {
            if (_bsLotsEquipmentSwitch.RadioListControl.SelectedValue == "Yes")
            {
                ClearControls();
                _rdbLotSelect.RadioControl.Checked = false;
                _rdbResourceSelect.RadioControl.Checked = true;
                _rdbResourceSelect.RadioControl.CheckedChanged += new EventHandler(ResourceSelectRadioButtonControl_CheckedChanged);
            }
            else if (_bsLotsEquipmentSwitch.RadioListControl.SelectedValue == "No")
            {
                ClearControls();
                _rdbResourceSelect.RadioControl.Checked = false;
                _rdbLotSelect.RadioControl.Checked = true;
                _rdbLotSelect.RadioControl.CheckedChanged += new EventHandler(LotSelectRadioButtonControl_CheckedChanged);
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _ndoProcessType_DataChanged(object sender, EventArgs e)
        {
            if (Page.DataContract.GetValueByName("WIPMain_RedirectSelectedLot") != null)
            {
                return;
            }
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
            // --ExecutionPathPlaceHolder
            EquipmentDataChange();
            if (_ndoEquipment.Data != null && GetWIPFlag() == "1")
                //Added last parameter for Front End Conversion to v5.8:
                FetchTxnData(FetchTxnDataEvents.WIPFlagChange, null, false, true);

            _ndoLoadPort.Visible = false;

            if (_ndoEquipment.Data != null && GetTxnData("RequiredLoadPort").ToUpper() == "TRUE" && GetWIPFlag() == "1")
            {
                _ndoLoadPort.Visible = true;
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void EquipmentDataChange()
        {
            // --ExecutionPathPlaceHolder
            bool bVisible = false;

            if (_ndoEquipment.Data != null)
            {
                if (_ndoEquipment.Data.ToString() != "")
                    bVisible = true;
            }
            else
                bVisible = false;

            if (_rdbTrackOutRadioButton.RadioControl.Checked)
                FetchTxnData(FetchTxnDataEvents.TrackOutEquipmentChange);

        }
        //MOVE OUT
        public void _subNextStepField_DataChanged(object sender, EventArgs e)
        {
            if (Page.DataContract.GetValueByName("WIPMain_RedirectSelectedLot") != null)
            {
                return;
            }

            if (GetWIPFlag() == "4" && GetTxnData("AutoMoveOut").ToUpper() == "FALSE" && _subNextStepField.Data != null)
            {
                FetchTxnData(FetchTxnDataEvents.NextStepsChange);
            }

        }

        //TRACK OUT
        public void _subTrackOutNextStepField_DataChanged(object sender, EventArgs e)
        {
            if (GetWIPFlag() == "2" && GetTxnData("AutoMoveOut").ToUpper() == "TRUE" && _subTrackOutNextStepField.Data != null)
            {
                FetchTxnData(FetchTxnDataEvents.NextStepsChange);
            }
        }

        //---------------------------------------------------
        //Items Grid Selected Rows Changed
        protected virtual ResponseData ItemsGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            RefreshRequiredActivitiesList();
            return new StatusData(true, "Selected rows updated.");
        }
        //Items Grid Selected Rows Changed
        //---------------------------------------------------
        void LotSelectRadioButtonControl_CheckedChanged(object sender, EventArgs e)
        {
            // --ExecutionPathPlaceHolder
            SetSelectionMode("LOT");
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void ResourceSelectRadioButtonControl_CheckedChanged(object sender, EventArgs e)
        {
            // --ExecutionPathPlaceHolder            
            SetSelectionMode("RESOURCE");
        }

        //---------------------------------------------------
        // add containers into placeholder to process delete from grid
        //---------------------------------------------------
        private void AddContainerToDelete()
        {
            if (_gridLotData.GridContext.SelectedRowIDs != null)
            {
                string[] selectedContainers = _gridLotData.GridContext.SelectedRowIDs.ToArray();
                int totalRow = _gridLotData.GridContext.SelectedRowIDs.Count;
                //for (int i = 0; i < _gridLotData.GridContext.SelectedRowIDs.Count(); i++)
                foreach (string selectedRowID in _gridLotData.SelectedRowIDs)
                {
                    _txtContainerToDelete.Data = _gridLotData.GridContext.GetCell(selectedRowID, "Container").ToString();
                    LotDataGrid_DeleteEvent();
                }
            }
        }

        //---------------------------------------------------
        // note: this event is triggered when a row is deleted from the LotDataGrid. The data grid will call javascript to click on the WIPMain_Controller_ContainerDelete button which calls this event
        // the main reason for performing the delete in this manner is due to the page not performing a full post back on delete.
        //---------------------------------------------------
        public void LotDataGrid_DeleteEvent()
        {
            // --ExecutionPathPlaceHolder
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


                // reset the (_gridItemData.GridContext as BoundContext).SelectedRowIDs
                // it will still hold the rowIDs of the removed items
                // assumption is that the rowIDs are reset after the loadData above so just make sure that the number of items match the number of selectedRowIds

                int iItemCount = oItemsList.Count;
                int iSelectedRowIDCount = sSelectedRowIDs.Count;

                for (int x = iItemCount; x < iSelectedRowIDCount; x++)
                {
                    sSelectedRowIDs.RemoveAt(iItemCount);
                }
                (_gridItemData.GridContext as BoundContext).SelectedRowIDs = sSelectedRowIDs;

                CamstarWebControl.SetRenderToClient(_gridItemData);
            }

            if (_txtSelectedContainerID.Data.ToString() == sContainerToDelete)
            {
                // get the 1st valid container in the grid 
                for (int i = 0; i < _gridLotData.GridContext.GetTotalRows(); i++)
                {
                    if ((_gridLotData.GridContext as BoundContext).GetCell(i, "Container") != sContainerToDelete)
                    {
                        _txtSelectedContainerID.Data = (_gridLotData.GridContext as BoundContext).GetCell(i, "Container");
                        break;
                    }
                }
            }

            _txtContainerToDelete.ClearData();

            // if there is only 1 lot remaining SetWIPMainControls, to redisplay the trackin/trackout fields and set the values
            // note: the trackin/trackout fields are hidded when there is more than one lot entered
            if (_gridLotData.TotalRowCount == 1)
            {
                FetchTxnData(FetchTxnDataEvents.MainContainerChange);
                SetWIPMainControls();
            }

            if (_gridLotData.Data != null && _gridLotData.TotalRowCount > 0)
            {
                ContainerRef[] _newContainerList = new ContainerRef[_gridLotData.BoundContext.GetTotalRows() - 1];
                int j = 0;
                for (int i = 0; i < _newContainerList.Length + 1; i++)
                {
                    if (_gridLotData.GridContext.GetCell(i.ToString().PadLeft(6, '0'), "Container").ToString() != sContainerToDelete)
                        _newContainerList[j++] = new ContainerRef()
                        {
                            Name = _gridLotData.GridContext.GetCell(i.ToString().PadLeft(6, '0'), "Container").ToString()
                        };
                }
                //update the container list for the grouped wip data collection
                Page.Session[_WIPMainContainerList] = _newContainerList;
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void SetSelectionMode(string mode)
        {
            // --ExecutionPathPlaceHolder
            if (mode.ToUpper() == "LOT")
            {
                ////_btnLotSelectPopup.Visible = true;
                ////_btnResourcetSelectPopup.Visible = false;
                _rdbResourceSelect.RadioControl.Checked = false;
                if (_rdbTrackInRadioButton.RadioControl.Checked || _rdbTrackOutRadioButton.RadioControl.Checked || _gridLotData.IsEmpty)
                    _ndoEquipment.Enabled = true;
            }
            else
            {
                ////_btnLotSelectPopup.Visible = false;
                ////_btnResourcetSelectPopup.Visible = true;
                _rdbLotSelect.RadioControl.Checked = false;
                _ndoEquipment.Enabled = false;
            }

            CamstarWebControl.SetRenderToClient(_ndoEquipment);
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _txtSelectionId_DataChanged(object sender, EventArgs e)
        {
            // --ExecutionPathPlaceHolder
            if (_txtSelectionId.Data != null)
            {
                if (_txtSelectionId.Data.ToString() != "")
                {
                    Page.StatusBar.ClearMessage();

                    if (FetchTxnData(FetchTxnDataEvents.SelectionIdEntry))
                    {
                        _txtSelectionId.ClearData();
                        _txtSelectionId.Focus();
                        //Page.PortalContext.DataContract.SetValueByName("WIPMain_SelectionId", null);
                    }
                }
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
        protected virtual bool SetControls(string WIPFlag = "", string WebPartStatus = "Initialize", bool IsPageClearEvent = false, bool IsSamplingEvent = false)
        {
            // --ExecutionPathPlaceHolder
            if (GetTxnData("IsWaferProcessing").ToUpper() == "TRUE" && WIPFlag != "5")
            {
                // show the wafers tab
                SetTabVisiblity(_tabLotItem, "Item", true);

                // if the WIPFlag = 4 then disallow selection of the Wafers
                if (WIPFlag == "4")
                    _gridItemData.GridContext.RowSelectionMode = JQGridSelectionMode.Disable;
            }
            else
            {
                _tabLotItem.SelectedIndex = 0;
                SetTabVisiblity(_tabLotItem, "Item", false);
            }

            // set the WIPMain home panel controls
            SetWIPMainControls(WIPFlag);

            // set the Action Icons enabled/disabled
            int iContainerCount = _gridLotData.BoundContext.GetTotalRows();
            bool bContainerSelected = _txtSelectedContainerID.Data != null ? true : false;
            EnableActionIcons(iContainerCount, bContainerSelected, WIPFlag);

            // set the service types for the web parts            
            string sRejectsSvcType = "";
            string sDataSvcType = "";
            string sBinningSvcType = "";

            switch (GetWIPFlag())
            {
                case "1": // trackin
                    sDataSvcType = "TrackInLot";
                    _txtWIPMainTxnType.Data = "Track In";
                    if (!IsSamplingEvent)
                        if (!_rdbResourceSelect.RadioControl.Checked)
                            _ndoEquipment.ClearData();
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
                        break;
                    case "4":
                        _rdbMoveInRadioButton.Enabled = false;
                        _rdbTrackInRadioButton.Enabled = false;
                        _rdbTrackOutRadioButton.Enabled = false;
                        _rdbMoveOutRadioButton.Enabled = true;
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
                    //_rdbMoveInRadioButton.Visible = true;
                    _ndoProcessType.Enabled = false;
                    _ndoEquipment.Enabled = false;

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
                    if (_rdbLotSelect.RadioControl.Checked)
                        _ndoEquipment.Enabled = true;

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

                    //Set load port field invisible
                    if (_ndoEquipment.Data != null && GetTxnData("RequiredLoadPort").ToUpper() == "TRUE" && GetWIPFlag() == "1")
                        _ndoLoadPort.Visible = true;
                    else
                        _ndoLoadPort.Visible = false;

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
                    if (_rdbLotSelect.RadioControl.Checked)
                        _ndoEquipment.Enabled = true;

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

                    //Set load port field invisible
                    _ndoLoadPort.Visible = false;

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
                    _ndoProcessType.Enabled = false;
                    _ndoEquipment.Enabled = false;
                    //Set move out controls
                    if ((_gridLotData.TotalRowCount > 1) || (GetTxnData("IsWaferProcessing").ToUpper() == "TRUE"))
                    {
                        _txtMoveOutQtyField.ClearData();
                        _txtMoveOutQtyField.Visible = false;
                        _txtSamplingRejectQty.ClearData();
                        _txtSamplingRejectQty.Visible = false;
                    }
                    else
                    {
                        _txtMoveOutQtyField.Visible = true;
                        if (GetTxnData("AutoSetMoveOutQty").ToUpper() == "TRUE")
                        {
                            int calculatedQty = Convert.ToInt32(GetTxnData("MoveOutQty").ToString());
                            _txtSamplingRejectQty.Data = GetTxnData("SamplingRejectQty").ToString();
                            if (_txtSamplingRejectQty.Data != null)
                            {
                                calculatedQty = calculatedQty - Convert.ToInt32(GetTxnData("SamplingRejectQty").ToString());
                            }
                            _txtMoveOutQtyField.Data = calculatedQty;
                            _txtMoveOutQtyField.ReadOnly = true;
                            _txtSamplingRejectQty.ReadOnly = true;

                            //set the sampling reject qty in lot data grid
                            _gridLotData.BoundContext.SetCell("000000", "MoveOutQty", calculatedQty.ToString());
                        }
                        else
                        {
                            _txtMoveOutQtyField.ClearData();
                            _txtMoveOutQtyField.ReadOnly = false;
                            _txtSamplingRejectQty.ClearData();
                            _txtSamplingRejectQty.ReadOnly = false;
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

        //---------------------------------------------------
        // FetchTxnData
        // Dec 19, 2014:    Added parameter DoSetControls which (when true) will enable this function to call SetControls
        //                  This change was necessary to prevent this function from calling SetControls in some cases
        //                  Specific case:  front end conversion to v5.8: this function is called when the Resource changes
        //                                  to check for CheckSheets but the v5.8 OOB version of SetControls clears the Resource again

        //---------------------------------------------------
        protected virtual bool FetchTxnData(FetchTxnDataEvents EventName, string[] SelectionLotIds = null, Boolean DoSetControls = true, Boolean bTrackInEquipChange = false)
        {
            // --ExecutionPathPlaceHolder
            try
            {
                // if there is a WIPflagChange then clear the item selection
                ////if (EventName == FetchTxnDataEvents.WIPFlagChange || EventName == FetchTxnDataEvents.TrackOutEquipmentChange)
                ////    _gridItemData.ClearData();

                string sServiceType = _txtPrimaryServiceType.Data != null ? _txtPrimaryServiceType.Data.ToString() : "WIPMain";
                string sWIPFlag = GetWIPFlag();
                string sServiceEventName = "";
                bool bWIPMsgRequested = false;

                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                var fieldList = new string[] { "LOT", "WAFER", "CARRIER", "BATCHID", "RESOURCE", "WAFERBATCHID" };
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

                // User Story 485015:Operator able to view all the WIP Instructions that loaded from Selection Popup
                if (SelectionLotIds != null)
                {
                    // If a user selects lot(s) from Selection Popup, the logic push the collected lot(s) from Selection Popup to scsSelectedContainers
                    (oServiceData as WIPMain).scsSelectedContainers = new Primitive<string>[SelectionLotIds.Length];
                    for (int i = 0; i < SelectionLotIds.Length; i++) 
                    {
                        (oServiceData as WIPMain).scsSelectedContainers[i] = SelectionLotIds[i];
                    }
                }
                else if (_txtSelectionId.Data != null)
                {
                    // Also include the scnaned lot from SelectionId to make the flow to be smooth instead of handling it in existing variable 
                    (oServiceData as WIPMain).scsSelectedContainers = new Primitive<string>[1];
                    (oServiceData as WIPMain).scsSelectedContainers[0] = _txtSelectionId.Data.ToString();
                }

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
                    if (_txtSelectionId.Data != null)
                    {
                        (oServiceData as WIPMain).SelectionId = _txtSelectionId.Data.ToString();
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
                    for (int x = 0; x <= iCurrentLotCount - 1; x++)
                    {
                        (oServiceData as WIPMain).Containers[x] = new ContainerRef();
                        if (IsCalledViaResourceLayoutView()) // use the rowid to get the value instead of the direct cell since the UI has not been rendered yet
                            (oServiceData as WIPMain).Containers[x].Name = (_gridLotData.GridContext as BoundContext).GetCell(x.ToString().PadLeft(6, '0'), "Container").ToString();
                        else
                            (oServiceData as WIPMain).Containers[x].Name = (_gridLotData.GridContext as BoundContext).GetCell(x.ToString().PadLeft(6, '0'), "Container").ToString();
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
                        if (IsCalledViaResourceLayoutView() || Page.DataContract.GetValueByName("WIPMain_RedirectSelectedLot") != null || IsCalledViaEquipmentMode()) // use rowID to get the cell value instead of the direct cell as the UI has not been rendered yet
                            (oServiceData as WIPMain).Container.Name = (_gridLotData.GridContext as BoundContext).GetCell("000000", "Container").ToString();
                        else
                            (oServiceData as WIPMain).Container.Name = (_gridLotData.GridContext as BoundContext).GetCell(0, "Container").ToString();
                    }

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
                    //-----------------------------------------------------
                    if ((EventName == FetchTxnDataEvents.SelectionIdEntry && sWIPFlag == "2")
                        || (EventName == FetchTxnDataEvents.TrackOutEquipmentChange)
                        || (EventName == FetchTxnDataEvents.WIPFlagChange && (sWIPFlag == "2" || sWIPFlag == "1"))
                        || (EventName == FetchTxnDataEvents.PopupWithQtyChange && iCurrentLotCount == 1 && sWIPFlag == "2")
                        || (EventName == FetchTxnDataEvents.MainContainerChange))
                    {
                        if (_ndoEquipment.Data != null)
                            (oServiceData as WIPMain).Equipment = new NamedObjectRef(_ndoEquipment.Data.ToString());//_ndoEquipment.Data as NamedObjectRef;

                    }

                    if(EventName == FetchTxnDataEvents.PopupWithQtyChange)
                    {
                        oServiceInfo.RequiredCheckSheet = FieldInfoUtil.RequestValue();
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
                    // To be requested if there are existing lots
                    oServiceInfo.WIPDataExists = FieldInfoUtil.RequestValue();
                    oServiceInfo.DataCollectionPresent = FieldInfoUtil.RequestValue();

                    //-----------------------------------------------------
                    // Pass in the SelectionId value through SelectionId if
                    // i)    There is at least one lot selected
                    // ii)   Event name = WIPFlagChange
                    // iii)  SelectionIdType = WAFERBATCHID
                    // iv)   Not submit
                    // v)    WIPFlagSelection = 2                    
                    //------------------------------------------------------
                    if (EventName == FetchTxnDataEvents.WIPFlagChange && !bIsSubmit)
                    {
                        // get the carrier of the selected row                        
                        //string sSelectedRowId = (_gridLotData.GridContext as BoundContext).SelectedRowID.ToString();
                        string sWaferBatchId = "";
                        string sSelectionIdType = "";
                        if(Page.DataContract.GetValueByName("WIPMain_RedirectSelectedLot") != null)
                        {
                            sSelectionIdType = (_gridLotData.GridContext as BoundContext).GetCell("000000", "__SelectionIdType").ToString();
                            sWaferBatchId = (_gridLotData.GridContext as BoundContext).GetCell("000000", "__SelectionId").ToString();

                        }
                        else
                        {
                            sSelectionIdType = (_gridLotData.GridContext as BoundContext).GetCell(0, "__SelectionIdType").ToString();
                            sWaferBatchId = (_gridLotData.GridContext as BoundContext).GetCell(0, "__SelectionId").ToString();
                        }

                        if (sSelectionIdType == "WAFERBATCHID")
                        {
                            sServiceEventName = "ResolveSelectionId";
                            (oServiceData as WIPMain).SelectionId = sWaferBatchId;
                            oServiceInfo.SelectionIdType = FieldInfoUtil.RequestValue();
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
                if (EventName == FetchTxnDataEvents.SelectionIdEntry && sWIPFlag == "0" || EventName == FetchTxnDataEvents.PopupWithQtyChange)
                {
                    if (_ndoEquipment.Data != null)
                        (oServiceData as WIPMain).Equipment = _ndoEquipment.Data as NamedObjectRef;
                }

                //-----------------------------------------------------
                // Information to be requested only once
                //-----------------------------------------------------
                if (iCurrentLotCount == 0)
                {
                    // WIP Data
                    oServiceInfo.IsWaferProcessing = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPStatus = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPYieldResult = FieldInfoUtil.RequestValue();
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
                    oServiceInfo.RequiredActivitiesEx = new scsRequiredActivityEx_Info();
                    oServiceInfo.RequiredActivitiesEx.Activity = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredActivitiesEx.Message = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredActivitiesEx.Status = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredActivitiesEx.SubMessage01 = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredActivitiesEx.StatusMessage = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPDataExists = FieldInfoUtil.RequestValue();
                    oServiceInfo.DataCollectionPresent = FieldInfoUtil.RequestValue();
                    //Required Check Sheet
                    //oServiceInfo.RequiredCheckSheet = FieldInfoUtil.RequestValue();
                    oServiceInfo.SelectionResource = FieldInfoUtil.RequestValue();

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
                    oServiceInfo.RequiredActivitiesEx = new scsRequiredActivityEx_Info();
                    oServiceInfo.RequiredActivitiesEx.Activity = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredActivitiesEx.Message = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredActivitiesEx.Status = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredActivitiesEx.SubMessage01 = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredActivitiesEx.StatusMessage = FieldInfoUtil.RequestValue();
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
                    || (EventName == FetchTxnDataEvents.MainContainerChange)
                    || (EventName == FetchTxnDataEvents.PopupWithQtyChange))
                {
                    oServiceInfo.MaxTrackInQty = FieldInfoUtil.RequestValue();
                    oServiceInfo.SourceEquipmentSelection = new TrackInLotSourceEquipment_Info();
                    oServiceInfo.SourceEquipmentSelection.Equipment = FieldInfoUtil.RequestValue();
                    oServiceInfo.SourceEquipmentSelection.TrackInQty = FieldInfoUtil.RequestValue();

                    if (_txtSelectedContainerID.Data != null)
                    {
                        (oServiceData as WIPMain).Container = new ContainerRef();
                        (oServiceData as WIPMain).Container.Name = _txtSelectedContainerID.Data.ToString();
                        (oServiceData as WIPMain).SelectedContainer = new ContainerRef();
                        (oServiceData as WIPMain).SelectedContainer.Name = _txtSelectedContainerID.Data.ToString();
                        oServiceInfo.RequiredCheckSheet = FieldInfoUtil.RequestValue();
                    }

                }

                //-----------------------------------------------------
                //  Request EquipmentSelection if:
                //  i)    No existing lots, or
                //  ii)   When Process Type is changed, or
                //  iii)  When WIP Flag is changed to 1 or 2
                //-----------------------------------------------------
                if (iCurrentLotCount == 0
                    || (EventName == FetchTxnDataEvents.ProcessTypeChange)
                    || (EventName == FetchTxnDataEvents.PopupWithQtyChange)
                    || (EventName == FetchTxnDataEvents.WIPFlagChange && (sWIPFlag == "1" || sWIPFlag == "2")))
                {
                    oServiceInfo.EquipmentSelection = FieldInfoUtil.RequestValue();
                    oServiceInfo.scsEquipmentSelection = FieldInfoUtil.RequestSelectionValue();

                    //Load Port
                    if (_ndoEquipment.Data != null
                        && ((EventName == FetchTxnDataEvents.WIPFlagChange) || (EventName == FetchTxnDataEvents.PopupWithQtyChange))
                        && (sWIPFlag == "1" || GetTxnData("WIPFlagSelection") == "1"))
                    {
                        oServiceInfo.scsLoadPortSelection = FieldInfoUtil.RequestValue();
                        oServiceInfo.scsLoadPort = FieldInfoUtil.RequestValue();
                        oServiceInfo.scsRequiredLoadPort = FieldInfoUtil.RequestValue();
                    }
                }
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
                    oServiceInfo.SamplingRejectQty = FieldInfoUtil.RequestValue();
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
                    // User Story 485015:Operator able to view all the WIP Instructions that loaded from Selection Popup
                    // Replacing WIPInstruction RequestValue with scsWIPInstructions
                    oServiceInfo.scsWIPInstructions = FieldInfoUtil.RequestValue();
                    bWIPMsgRequested = true;
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
                     || EventName == FetchTxnDataEvents.TrackOutEquipmentChange
                     || EventName == FetchTxnDataEvents.MainContainerChange)
                )
                {
                    oServiceInfo.RequiredActivities = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredCheckSheet = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredActivitiesEx = new scsRequiredActivityEx_Info();
                    oServiceInfo.RequiredActivitiesEx.Activity = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredActivitiesEx.Message = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredActivitiesEx.Status = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredActivitiesEx.SubMessage01 = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredActivitiesEx.StatusMessage = FieldInfoUtil.RequestValue();

                    if (_txtSelectedContainerID.Data != null)
                    {
                        (oServiceData as WIPMain).SelectedContainer = new ContainerRef();
                        (oServiceData as WIPMain).SelectedContainer.Name = _txtSelectedContainerID.Data.ToString();
                    }
                }

                //-----------------------------------------------------
                //  Request WafersDetailsSelectionAll and SelectionWafers if:
                //  i)    No existing lots, or
                //  ii)   When Process Type is changed for wafer processing, or
                //  iii)  When a Selection Id is entered with existing lots for wafer processing, or
                //  iv)   WIP Flag changed to 1 for wafer processing, or
                //  v)    WIP Flag changed to 2 for wafer processing, or
                //  vi)    When Track Out Equipment is changed for wafer processing
                //  vii)   When Move Out with possible quantity change for wafer processing
                //-----------------------------------------------------
                if (iCurrentLotCount == 0
                    || (EventName == FetchTxnDataEvents.ProcessTypeChange && GetTxnData("IsWaferProcessing").ToUpper() == "TRUE")
                    || (EventName == FetchTxnDataEvents.SelectionIdEntry && iCurrentLotCount > 0 && GetTxnData("IsWaferProcessing").ToUpper() == "TRUE")
                    || (EventName == FetchTxnDataEvents.WIPFlagChange && sWIPFlag == "2" && GetTxnData("IsWaferProcessing").ToUpper() == "TRUE" && _ndoEquipment.Data != null)
                    || (EventName == FetchTxnDataEvents.WIPFlagChange && sWIPFlag == "1" && GetTxnData("IsWaferProcessing").ToUpper() == "TRUE")
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

                //request for Sampling Wafers when Track In Equipment Changed
                if (
                    (EventName == FetchTxnDataEvents.WIPFlagChange && (sWIPFlag == "1") && GetTxnData("IsWaferProcessing").ToUpper() == "TRUE" && bTrackInEquipChange)
                    || (EventName == FetchTxnDataEvents.MainContainerChange && sWIPFlag == "1" && _ndoEquipment.Data != null)
                )
                {
                    oServiceInfo.scsSamplingWafers = new WIPLotTxnWafersDetails_Info();
                    oServiceInfo.scsSamplingWafers.WaferScribeNumber = FieldInfoUtil.RequestValue();
                    oServiceInfo.scsSamplingWafers.Container = FieldInfoUtil.RequestValue();
                }

                // Request Reserved Equipment 
                oServiceInfo.ReservedEquipment = FieldInfoUtil.RequestValue();

                //Request FutureLotHold
                if (EventName == FetchTxnDataEvents.NextStepsChange && ((sWIPFlag == "2" || sWIPFlag == "4")))
                {
                    if (_subTrackOutNextStepField != null || _subNextStepField != null)
                    {
                        NamedSubentityRef nextStep = _subTrackOutNextStepField.Data != null ? new NamedSubentityRef(_subTrackOutNextStepField.Text) : new NamedSubentityRef(_subNextStepField.Text);

                        // search the hidden TrackOutNextStep dropdown list to get the ID of the name
                        string sNextStepID = _ddlNextSteps.DropDownControl.Items.FindByText(nextStep.Name).Value;
                        nextStep.ID = sNextStepID;
                        (oServiceData as WIPMain).NextStep = nextStep;
                        oServiceInfo.RequiredActivities = FieldInfoUtil.RequestValue();
                        oServiceInfo.RequiredActivitiesEx = new scsRequiredActivityEx_Info();
                        oServiceInfo.RequiredActivitiesEx.Activity = FieldInfoUtil.RequestValue();
                        oServiceInfo.RequiredActivitiesEx.Message = FieldInfoUtil.RequestValue();
                        oServiceInfo.RequiredActivitiesEx.Status = FieldInfoUtil.RequestValue();
                        oServiceInfo.RequiredActivitiesEx.SubMessage01 = FieldInfoUtil.RequestValue();
                        oServiceInfo.RequiredActivitiesEx.StatusMessage = FieldInfoUtil.RequestValue();
                        oServiceInfo.WIPFlagSelection = FieldInfoUtil.RequestValue();
                        oServiceInfo.AutoMoveOut = FieldInfoUtil.RequestValue();
                    }
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
                    var oResultEnv = oResult.Environment as WIPMain_Environment;
                    SetTxnData(oResultValue, (oRequest as Request), iCurrentLotCount, sWIPFlag, oResultEnv);
                    if (EventName == FetchTxnDataEvents.SelectionIdEntry && oResultValue.NextSteps == null)
                    {
                        _subNextStepField.SetSelectionValues(null);
                    }

                    if (EventName != FetchTxnDataEvents.MainContainerChange)
                    {
                        if (oServiceInfo.Containers != null)
                            if (oResultValue.Containers != null)
                            {
                                SetContainers(oResultValue.Containers, sServiceType, iCurrentLotCount, (oResultValue.SelectionIdType != null ? oResultValue.SelectionIdType.ToString() : ""), oResultValue.SelectionContainer != null ? oResultValue.SelectionContainer.Name : "");
                            }

                        if ( ( oServiceInfo.WafersDetailsSelectionAll != null && !bTrackInEquipChange )
                            || (oServiceInfo.WafersDetailsSelectionAll != null && bTrackInEquipChange && oResultValue.scsWaferSamplingRequired.ToString() == "True")
                            )
                        {

                            (_gridItemData.GridContext as BoundContext).ClearData();
                            if (oResultValue.WafersDetailsSelectionAll != null)
                            {
                                SetItems(oResultValue.WafersDetailsSelectionAll, oResultValue.SelectionWafers, sWIPFlag, this.PrimaryServiceType, (oResultValue.SelectionIdType != null ? oResultValue.SelectionIdType.ToString() : ""));
                            }
                        }

                        //-----------------------------------------------------
                        // Set the default selected container
                        //-----------------------------------------------------
                        if (_txtSelectedContainerID.Data == null)
                            if (_gridLotData.BoundContext.GetTotalRows() > 0)
                            {
                                Array arGridData = _gridLotData.Data as Array;
                                var vRowData = arGridData.GetValue(0);
                                Type _dynamicType = SEMI.AppCode.GridUtility.RetrieveDynamicType(_dynamicTypeName + _txtPrimaryServiceType.Data.ToString());
                                var vPropertyInfo = _dynamicType.GetProperties();
                                var pp = vPropertyInfo.FirstOrDefault(p => p.Name == "Container");
                                var vContainer = pp.GetValue(vRowData, null);

                                _txtSelectedContainerID.Data = vContainer.ToString();
                            }

                        //-----------------------------------------------------
                        // Set the required activities
                        //-----------------------------------------------------
                        if (oServiceInfo.RequiredActivitiesEx != null)
                            if (oResultValue.RequiredActivitiesEx != null)
                                SetRequiredActivities(oResultValue.RequiredActivitiesEx);

                        // comment out _txtWIPInstructions clear data to fix wip instruction showing during equipment change
                        //_txtWIPInstructions.ClearData();
                        if (oResultValue.scsWIPInstructions != null)
                        {
                            // User Story 485015:Operator able to view all the WIP Instructions that loaded from Selection Popup
                            // A hard coded value been pushed to _txtWIPInstructions instead of the requested WIPInstruction value to make use of command bar enable & disable operation for WIP Msg
                            _txtWIPInstructions.Data = "PlaceHolder";
                        }
                        

                        //Set Sampling Wafers to be auto selected
                        if (EventName == FetchTxnDataEvents.WIPFlagChange && sWIPFlag == "1" && bTrackInEquipChange && GetTxnData("IsWaferProcessing").ToUpper() == "TRUE"
                                    && (oResultValue.scsSamplingWafers != null || GetTxnData("RequiredWaferSampling").ToUpper() == "TRUE"))
                        {
                            SetItems_WaferSamplingSelectionState(oResultValue.scsSamplingWafers);
                        }

                        // Change for FrontEnd conversion to v5.8: do not set the Controls if DoSetControls is false
                        if (DoSetControls)
                        {
                            if (EventName == FetchTxnDataEvents.PopupWithQtyChange)
                                SetControls(sWIPFlag, "Initialize", false, true);
                            else if (sWIPFlag != "0" && EventName != FetchTxnDataEvents.ProcessTypeChange)
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
                        //if (_txtWIPInstructions.Data != null && oResultValue.WIPInstruction != null)
                        //    DisplayAlerts(new string[] { _txtWIPInstructions.Data.ToString() });

                        //---------------------------------------------------------------------------------------------------
                        // Re-Set WIP Data tab visibility during Track Out as the data is only available at this point
                        //---------------------------------------------------------------------------------------------------
                        if ((GetWIPFlag() == "2") && (GetTxnData("WIPDataExists").ToUpper() == "TRUE"))
                        {
                            //EquipmentDataChange_SetWIPDataTab();
                        }
                    }

                    if( (EventName == FetchTxnDataEvents.MainContainerChange && sWIPFlag == "1" && _ndoEquipment.Data != null) && (oResultValue.scsSamplingWafers != null || GetTxnData("RequiredWaferSampling").ToUpper() == "TRUE"))
                    {
                        SetItems_WaferSamplingSelectionState(oResultValue.scsSamplingWafers);
                    }

                    //-----------------------------------------------------
                    // Set the required Check Sheet
                    //-----------------------------------------------------
                    if (oServiceInfo.RequiredCheckSheet != null)
                    {
                        if (oResultValue.RequiredCheckSheet != null && ((GetTxnData("WIPFlagSelection") != "1") || (GetTxnData("WIPFlagSelection") == "1" && _ndoEquipment.Data != null)))
                        {
                            _rdoEProcField.Data = oResultValue.RequiredCheckSheet;
                            // force the checksheet name in to the data contract member
                            // this is for the issue where the checksheet name does not bubble up into the datacontract member when accessing WIP Main from ResourceLayout
                            Page.DataContract.SetValueByName("WIPMain_HiddenElectronicProcedure_DM", _rdoEProcField.Data);
                        }
                        else
                        {
                            _rdoEProcField.ClearData();
                            Page.DataContract.SetValueByName("WIPMain_HiddenElectronicProcedure_DM", null);
                        }

                        if (oServiceInfo.RequiredActivitiesEx != null)
                            if (oResultValue.RequiredActivitiesEx != null)
                                SetRequiredActivities(oResultValue.RequiredActivitiesEx);
                    }
                    else
                    {
                        _rdoEProcField.ClearData();
                        Page.DataContract.SetValueByName("WIPMain_HiddenElectronicProcedure_DM", null);
                    }

                    // Request Workflow
                    FetchWorkflowData();

                    if (bTrackInEquipChange && GetTxnData("RequiredWaferSampling").ToUpper() == "TRUE" && sWIPFlag == "1")
                        RefreshRequiredActivitiesList();

                    // check for any alert messages (e.g - due to max time window..etc)
                    // need to check for the WIPInstructions too
                    bool bAlertAvailable = false;
                    string sResultMessage = oResultStatus.Message;

                    if (_txtWIPInstructions.Data != null && bWIPMsgRequested)
                    {
                        oResultStatus.Message = "";
                    }

                    // User Story 485015:Operator able to view all the WIP Instructions that loaded from Selection Popup
                    // Load into newly created string array from scsWIPInstructions
                    string[] collectedWIPInstruction = null;
                    if (oResultValue.scsWIPInstructions != null)
                    {
                        collectedWIPInstruction = new string[oResultValue.scsWIPInstructions.Length];
                        for (int i = 0; i < oResultValue.scsWIPInstructions.Length; i++)
                        {
                            collectedWIPInstruction[i] = oResultValue.scsWIPInstructions[i].ToString();
                        }
                    }

                    // User Story 485015:Operator able to view all the WIP Instructions that loaded from Selection Popup
                    // Replaces the sWIPInstruction to send value as multiple index array to display instead of single like sWIPInstruction
                    DisplayAlerts(oResultStatus, collectedWIPInstruction, out bAlertAvailable, out sResultMessage);
                    oResultStatus.Message = sResultMessage;

                } // if (oResultStatus.IsSuccess)
                else
                {
                    DisplayMessage(oResultStatus);
                }

                if (GetWIPFlag() == "5")
                {
                    ResetRibbonCss();
                    _MoveInTile.CssClass = "WIPMain_Txn_MoveIn active";
                    CamstarWebControl.SetRenderToClient(_MoveOutTile);
                }
                else if (GetWIPFlag() == "1")
                {
                    ResetRibbonCss();
                    _TrackInTile.CssClass = "WIPMain_Txn_TrackIn active";
                    CamstarWebControl.SetRenderToClient(_MoveOutTile);
                }
                else if (GetWIPFlag() == "2")
                {
                    ResetRibbonCss();
                    _TrackOutTile.CssClass = "WIPMain_Txn_TrackOut active";
                    CamstarWebControl.SetRenderToClient(_MoveOutTile);
                }
                else if (GetWIPFlag() == "4")
                {
                    ResetRibbonCss();
                    _MoveOutTile.CssClass = "WIPMain_Txn_MoveOut active";
                    CamstarWebControl.SetRenderToClient(_MoveOutTile);
                }

                return true;
            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus("FetchTxnData::" + ex.Message.ToString(), false));
                return false;
            }
        } // FetchTxnData

        private void FetchWorkflowData()
        {
            if (_txtSelectedContainerID.Data != null)
            {
                ViewContainerStatus inputData = new ViewContainerStatus { Container = new ContainerRef(_txtSelectedContainerID.Data.ToString()) };
                ViewContainerStatus_Info info = new ViewContainerStatus_Info
                {
                    Workflow = FieldInfoUtil.RequestValue(),
                    Step = FieldInfoUtil.RequestValue(),
                };
                UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
                ViewContainerStatusService serv = new ViewContainerStatusService(profile);
                ViewContainerStatus_Result result = null;
                ResultStatus resultStatus = serv.ExecuteTransaction(inputData, new ViewContainerStatus_Request { Info = info }, out result);
                if (resultStatus.IsSuccess)
                {
                    if (result.Value.Workflow != null)
                        WIPMain_Workflow.Data = new RevisionedObjectRef(result.Value.Workflow.ToString());
                    if (result.Value.Step != null)
                        WIPMain_WorkflowStep.Data = new RevisionedObjectRef(result.Value.Step.ToString());
                }
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void SetContainers(ContainerRef[] Containers, string PrimaryServiceType, int CurrentLotCount, string SelectionIdType, string SelectionContainer)
        {
            // --ExecutionPathPlaceHolder
            try
            {
                bool bGridColumnsSet = false;

                foreach (ContainerRef Container in Containers)
                {
                    // check if lot not exist in the grid else add to the grid
                    string sRowId = "";

                    //if (IsCalledViaResourceLayoutView()) // this is required for integration with the resourcelayout view page, 
                    //{
                    for (int x = 0; x < (_gridLotData.GridContext as BoundContext).GetTotalRows(); x++)
                        if ((_gridLotData.GridContext as BoundContext).GetCell(x.ToString().PadLeft(6, '0'), "Container").ToString() == Container.Name.ToString())
                        {
                            sRowId = x.ToString().PadLeft(6, '0');
                            break;
                        }
                    //}
                    //else
                    //{
                    //    sRowId = (_gridLotData.GridContext as BoundContext).GetRowIdByCellValue("Container", Container.Name.ToString());
                    //}


                    if (sRowId == "")
                    {
                        RecordSet rsLotData = new RecordSet();
                        rsLotData = SEMI.AppCode.UIUtility.GetLotQuerySelection(this, PrimaryServiceType, Container.Name.ToString());

                        // generate a new recordset
                        RecordSet rsGridData = new RecordSet();
                        Header[] GridHeaders = new Header[rsLotData.Headers.Length + 4];

                        // add the container header
                        GridHeaders[0] = new Header();
                        GridHeaders[0].TypeCode = TypeCode.String;
                        GridHeaders[0].Name = "Container";
                        GridHeaders[0].Label = new Label("SelVal_Container");

                        // clone the query headers
                        int x = 1;
                        foreach (Header header in rsLotData.Headers)
                        {
                            GridHeaders[x] = new Header();
                            GridHeaders[x].TypeCode = header.TypeCode;
                            GridHeaders[x].Name = header.Name;
                            GridHeaders[x].Label = header.Label;
                            x++;
                        }

                        // add the __selectionid and __outputcarrier headers
                        GridHeaders[x] = new Header();
                        GridHeaders[x].TypeCode = TypeCode.String;
                        GridHeaders[x].Name = "__SelectionId";
                        x++;
                        GridHeaders[x] = new Header();
                        GridHeaders[x].TypeCode = TypeCode.String;
                        GridHeaders[x].Name = "__SelectionIdType";
                        x++;
                        GridHeaders[x] = new Header();
                        GridHeaders[x].TypeCode = TypeCode.String;
                        GridHeaders[x].Name = "__OutputCarrier";

                        // clone the data across
                        Row[] rsRows = new Row[rsLotData.Rows.Length];
                        for (int i = 0; i < rsLotData.Rows.Length; i++)
                        {
                            string[] sRowData = new string[GridHeaders.Length];
                            rsRows[i] = new Row();
                            // 1st row is Container, assumption is that the 1st value of the LotData is ALWAYS LotId
                            string sContainerName = "";
                            sContainerName = rsLotData.Rows[i].Values[0].ToString();
                            sRowData[0] = sContainerName;

                            // clone the remaining values
                            int j = 1;
                            foreach (string sData in rsLotData.Rows[i].Values)
                            {
                                sRowData[j] = rsLotData.Rows[i].Values[j - 1].ToString();
                                j++;
                            }

                            // 3rd last value is __SelectionId
                            string sSelectionId = "";
                            if (_txtSelectionId.Data != null)
                            {
                                if (_txtSelectionId.Data.ToString() != "")
                                    sSelectionId = _txtSelectionId.Data.ToString();
                                else
                                    sSelectionId = sContainerName;
                            }
                            else
                                sSelectionId = sContainerName;
                            sRowData[j] = sSelectionId;
                            j++;

                            // 2nd last value is __SelectionIdType                            
                            sRowData[j] = SelectionIdType;
                            j++;
                            // last value is __OutputCarrier
                            string sOutputCarrier = "";
                            if (CurrentLotCount == 0 && PrimaryServiceType == "AssemblyCarrierWIPMain")
                            {
                                if (SelectionIdType == "CARRIER" && SelectionContainer == "")
                                    sOutputCarrier = _txtSelectionId.Data.ToString();
                            }
                            sRowData[j] = sOutputCarrier;

                            rsRows[i].Values = sRowData;
                        } // for (int i = 0; i < rsLotData.Rows.Length; i++)

                        rsGridData.Headers = GridHeaders;
                        rsGridData.Rows = rsRows;

                        if (!bGridColumnsSet)
                        {
                            bGridColumnsSet = true;
                            string[] sHiddenColumnNames = new string[] { "Lot", "__STYLE", "__OutputCarrier", "__SelectionId", "ProcessTimerName", "ProcessTimerRevision", "StartTimeGMT", "MinEndWarningTimeGMT", "MinWarningTimeColor", "MinEndTimeGMT", "MinTimeColor", "MaxEndWarningTimeGMT", "MaxWarningTimeColor", "MaxEndTimeGMT", "MaxTimeColor" }; //
                            string[] sSpecificWidthColumns = new string[] { "Qty|70", "Qty2|70" };
                            SEMI.AppCode.GridUtility.ItemListGrid_SetColumns(wpLotInfo_LotGridWP, rsGridData.GetAsExplicitlyDataTable(), _gridLotData.ID, null, _dynamicTypeName + PrimaryServiceType, true, sHiddenColumnNames, true, sSpecificWidthColumns, rsGridData.Headers);
                        }

                        JQDataGrid _gridLotDataTemp = _gridLotData;
                        SEMI.AppCode.GridUtility.ItemListGrid_AddDataRow(wpLotInfo_LotGridWP, rsGridData.GetAsExplicitlyDataTable(), ref _gridLotDataTemp, _dynamicTypeName + PrimaryServiceType);

                        //set container list for grouped data collection
                        Page.Session[_WIPMainContainerList] = Containers;

                    } // if (sRowId == "")    
                } //  foreach (ContainerRef Container in Containers)                
            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus("SetContainers::" + ex.Message.ToString(), false));
            }
        }

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

            foreach (SS_WIPMain_ItemData oNewItem in oNewItemList)
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
        //
        //---------------------------------------------------
        private void SetTxnData(WIPMain WIPMainServiceData, Request WIPMainRequest, int CurrentLotCount, string WIPFlag, WIPMain_Environment WIPMainEnv = null)
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
                    if (CurrentLotCount == 0)
                    {
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

                        //string sWIPDataExists = "";
                        //if (oResultData.WIPDataExists != null && oResultData.RequiredActivities != null)
                        //	if (oResultData.WIPDataExists == true)
                        //		if (IsExistsInRequiredActivitiesList("DATA COLLECTION", oResultData.RequiredActivities))
                        //			sWIPDataExists = "TRUE";
                        //AddTxnData("WIPDataExists", sWIPDataExists, true);
                        //AddTxnData("WIPDataExists", oResultData.WIPDataExists != null ? oResultData.WIPDataExists.ToString() : "", true);
                        AddTxnData("SamplingWIPDataExists", oResultData.SamplingWIPDataExists != null ? oResultData.SamplingWIPDataExists.ToString() : "", true);
                    } // if (iCurrentLotCount == 0)

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

                    }

                    //  if (oServiceInfo.SourceEquipment != null)

                    bool bFoundSameTrackInEquipment = false;
                    bool bFoundSameTrackOutEquipment = false;

                    if (oServiceInfo.EquipmentSelection != null)
                    {
                        if (GetTxnData("WIPFlagSelection") == "1" || WIPFlag == "1")
                        {
                            // get the existing selected TrackInEquipment
                            string sTrackInEquip = _ndoEquipment.Data != null ? _ndoEquipment.Data.ToString() : "";
                            // clear the TrackInEquipment selection data
                            _ndoEquipment.ClearSelectionValues();

                            if (oResultData.EquipmentSelection != null)
                            {
                                if (sTrackInEquip != "")
                                {
                                    foreach (NamedObjectRef oEquipment in oResultData.EquipmentSelection)
                                        if (oEquipment.Name == sTrackInEquip)
                                            bFoundSameTrackInEquipment = true;
                                }

                                // set the selection values
                                CWC.NamedObject _ndoTrackInEquipTemp = _ndoEquipment;
                                _ndoTrackInEquipTemp.AutopopulateSingleItem = false;
                                //SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoTrackInEquipTemp, oResultData.EquipmentSelection);
                                _ndoTrackInEquipTemp.SetSelectionValues(WIPMainEnv.scsEquipmentSelection.SelectionValues);
                                // set the selected 
                                if (!bFoundSameTrackInEquipment)
                                    if (oResultData.ReservedEquipment != null)
                                        _ndoEquipment.Data = new NamedObjectRef(oResultData.ReservedEquipment.Name);

                                    //else
                                    // _ndoEquipment.ClearData();
                                    else
                                        _ndoEquipment.Data = new NamedObjectRef(sTrackInEquip);
                            }
                        } // if (GetTxnData("WIPFlagSelection") == "1" || WIPFlag == "1")
                        else if (GetTxnData("WIPFlagSelection") == "3" || GetTxnData("WIPFlagSelection") == "2" || WIPFlag == "1")
                        {
                            // get the existing selected _ndoTrackOutEquipment
                            string sTrackOutEquip = _ndoEquipment.Data != null ? _ndoEquipment.Data.ToString() : "";
                            // clear the _ndoTrackOutEquipment selection data
                            _ndoEquipment.ClearSelectionValues();

                            if (oResultData.EquipmentSelection != null)
                            {
                                if (sTrackOutEquip != "")
                                {
                                    foreach (NamedObjectRef oEquipment in oResultData.EquipmentSelection)
                                        if (oEquipment.Name == sTrackOutEquip)
                                            bFoundSameTrackOutEquipment = true;
                                }

                                // set the selection values
                                CWC.NamedObject _ndoTrackOutEquipTemp = _ndoEquipment;
                                //SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoTrackOutEquipTemp, oResultData.EquipmentSelection);
                                _ndoTrackOutEquipTemp.SetSelectionValues(WIPMainEnv.scsEquipmentSelection.SelectionValues);
                                if (!bFoundSameTrackOutEquipment)
                                {
                                    if ((oResultData.SelectionIdType == "RESOURCE") && (_txtSelectionId.Data != null))
                                    {
                                        foreach (NamedObjectRef oEquipment in oResultData.EquipmentSelection)
                                            if (oEquipment.Name == _txtSelectionId.Data.ToString())
                                            {
                                                _ndoEquipment.Data = new NamedObjectRef(oEquipment.Name);
                                                break;
                                            }
                                    }
                                    else
                                    {
                                        if (oResultData.SelectionResource != null)
                                        {
                                            _ndoEquipment.Data = oResultData.SelectionResource;
                                        }
                                        else
                                        {
                                            _ndoEquipment.Data = oResultData.EquipmentSelection[0];
                                        }
                                    }
                                } // if (bFoundSameTrackOutEquipment)                                
                                else
                                {
                                    _ndoEquipment.Data = new NamedObjectRef(sTrackOutEquip);
                                    //_ndoEquipment.ClearData();
                                }
                            }
                        } //else if (GetTxnData("WIPFlagSelection") == "3" || GetTxnData("WIPFlagSelection") == "2" || WIPFlag == "1")
                    } // if (oServiceInfo.EquipmentSelection != null)

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
                    {
                        AddTxnData("MoveOutQty", oResultData.MoveOutQty != null ? oResultData.MoveOutQty.ToString() : "", true);
                        AddTxnData("SamplingRejectQty", oResultData.SamplingRejectQty != null ? oResultData.SamplingRejectQty.ToString() : "", true);
                    }

                    if (oResultData.NextSteps != null)
                    {
                        if (oResultData.WIPFlagSelection != null && ((oResultData.WIPFlagSelection.ToString() == "4")
                            || ((oResultData.WIPFlagSelection.ToString() == "3") && (oResultData.AutoMoveOut.ToString().ToUpper() == "TRUE")))
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

                    }
                    // For Data Collection tab
                    string sWIPDataExists = "FALSE";
                    if (oResultData.WIPDataExists != null && oResultData.RequiredActivities != null)
                        if (oResultData.WIPDataExists == true || oResultData.DataCollectionPresent == true) //2018-03-20 Changed the "&&" to "||" to fix issue with WIP Data Setup Matrix
                            sWIPDataExists = "TRUE";
                    AddTxnData("WIPDataExists", sWIPDataExists, true);

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
        //
        //---------------------------------------------------
        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Reset")
            {
                ClearControls();
            }
            else if (action != null && action.Parameters == "CustomSubmit")
            {
                ConfirmSubmit(e);
            }
            else if (action != null && action.Parameters == "DeleteContainers")
            {
                AddContainerToDelete();
            }
            else if (action != null && action.Parameters == "WIPMessages")
            {
                WIPMessagesAction();
            }
            else if (action != null && action.Parameters == "DisplaySPC")
            {
                LastSPCChartAction();
            }
            else if (action != null && action.Parameters == "WaferSampling")
            {
                SetTabVisiblity(_tabLotItem, "Item", true);
                _tabLotItem.SelectedIndex = 1;
            }
        } // WebPartCustomAction 

        //---------------------------------------------------
        // Submit Button Codes
        //---------------------------------------------------
        public void SubmitTransactions(CustomActionEventArgs e = null)
        {
            try
            {
                //GetEsig
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


                //If ESig exist then submit it
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
                //Batch tracking is disabled for Track In (this means ALL lots will be transacted together)
                //if ((ConstContainersPerBatch > 0) && ((_gridLotData.GridContext.GetTotalRows() > ConstContainersPerBatch) && (sWIPFlag != "1")))
                //{
                //    iLotsPerBatch = ConstContainersPerBatch;
                //}
                //else
                //{
                iLotsPerBatch = _gridLotData.GridContext.GetTotalRows();
                //}

                //Add the Containers and Wafers
                ContainerRef[] containerNames = new ContainerRef[iLotsPerBatch];
                for (int i = 0; i < iLotsPerBatch; i++)
                {
                    containerNames[i] = new ContainerRef();
                    containerNames[i].Name = _gridLotData.GridContext.GetCell(i.ToString().PadLeft(6, '0'), "Container").ToString();
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
                            SvcInfo.SetValue("ss_IsExceededMaxNoProcessTime", new Info(true));
                            ReqData.SetValue("Info", SvcInfo);
                        }

                        if (_ndoLoadPort.Data != null)
                        {
                            NamedObjectRef loadport = new NamedObjectRef();
                            loadport.Name = _ndoLoadPort.Data.ToString();
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
                    string[] sCompletionMessages = new string[] { Results.Message.Substring(Results.Message.IndexOf('@') + 3) };

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
                        if ((result.Value as WIPMain).ss_IsExceededMaxNoProcessTime != null && (result.Value as WIPMain).ss_IsExceededMaxNoProcessTime == true)
                            DisplayAlerts(sCompletionMessages);
                        else
                        {
                            DisplayAlerts(Results, out bAlertAvailable, out sCompletionMessage);
                            Results.Message = sCompletionMessage;
                        }
                    }

                    if (!bAlertAvailable && ((result.Value as WIPMain).ss_IsExceededMaxNoProcessTime == false || (result.Value as WIPMain).ss_IsExceededMaxNoProcessTime == null))
                    {
                        if (e != null)
                            e.Result = Results;
                        else
                            this.DisplayMessage(Results);

                    }

                    ClearControls(0, true);
                }
                else
                {
                    if (e != null)
                        e.Result = Results;
                    else
                        this.DisplayMessage(Results);
                }
                //return Results;
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
            //return Page.Service.Submit(PrimaryServiceType, true);
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
                //Batch tracking is disabled for Track In (this means ALL lots will be transacted together)
                //if ((ConstContainersPerBatch > 0) && ((_gridLotData.GridContext.GetTotalRows() > ConstContainersPerBatch) && ((txn as WIPMain).WIPFlag != 1)))
                //{
                //    iLotsPerBatch = ConstContainersPerBatch;
                //}
                //else
                //{
                iLotsPerBatch = _gridLotData.GridContext.GetTotalRows();
                //}

                //Add the Containers
                ContainerRef[] containerNames = new ContainerRef[iLotsPerBatch];
                for (int i = 0; i < iLotsPerBatch; i++)
                {
                    containerNames[i] = new ContainerRef();
                    containerNames[i].Name = _gridLotData.GridContext.GetCell(i.ToString().PadLeft(6, '0'), "Container").ToString();
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

                //Determine the number of lots to transact in a batch
                //Batch tracking is disabled for Track In (this means ALL lots will be transacted together)
                //if ((ConstContainersPerBatch > 0) && ((_gridLotData.GridContext.GetTotalRows() > ConstContainersPerBatch) && (sWIPFlag != "1")))                
                //    iLotsPerBatch = ConstContainersPerBatch;                
                //else                
                iLotsPerBatch = _gridLotData.GridContext.GetTotalRows();

                //Add the Containers and Wafers
                ContainerRef[] containerNames = new ContainerRef[iLotsPerBatch];
                for (int i = 0; i < iLotsPerBatch; i++)
                {
                    containerNames[i] = new ContainerRef();
                    containerNames[i].Name = _gridLotData.GridContext.GetCell(i.ToString().PadLeft(6, '0'), "Container").ToString();
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
                    CWC.Button oSubmit = Page.FindCamstarControl("Main_SubmitButton") as CWC.Button;

                    bool bConfirmationRequired = false;
                    string sCompletionMessage = Results.Message;

                    if ((result.Value as WIPMain).scsIsTxnConfirmationReq != null)
                        bConfirmationRequired = bool.Parse((result.Value as WIPMain).scsIsTxnConfirmationReq.ToString());

                    if (bConfirmationRequired)
                        ScriptManager.RegisterStartupScript(Page.Form, this.GetType(), "myConfirm", "WIPMainAdvanced_TxnConfirmation();", true);
                    else
                        SubmitTransactions(e);
                }
                else
                {
                    e.Result = Results;
                }

                //return (e.Result != null) ? e.Result : Results;
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
            //return Page.Service.Submit(PrimaryServiceType, true);
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public virtual void ClearControls(int ClearFlag = 0, bool SkipDisplayMessage = false)
        {
            // --ExecutionPathPlaceHolder
            try
            {
                if (!SkipDisplayMessage)
                {
                    Page.StatusBar.ClearMessage();
                }

                if (ClearFlag <= 10)
                {
                    //Clear Process Types Controls
                    _rdbMoveInRadioButton.RadioControl.Checked = false;
                    _rdbMoveInRadioButton.Enabled = false;
                    _rdbMoveInRadioButton.Data = false;
                    _rdbTrackInRadioButton.RadioControl.Checked = false;
                    _rdbTrackInRadioButton.Enabled = false;
                    _rdbTrackInRadioButton.Data = false;
                    _rdbTrackOutRadioButton.RadioControl.Checked = false;
                    _rdbTrackOutRadioButton.Enabled = false;
                    _rdbTrackOutRadioButton.Data = false;
                    _rdbMoveOutRadioButton.RadioControl.Checked = false;
                    _rdbMoveOutRadioButton.Enabled = false;
                    _rdbMoveOutRadioButton.Data = false;

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
                    //Clear employee
                    _ndoEmployee.ClearData();

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

                    //Clear Track In & Track Out Equipment
                    _ndoEquipment.ClearData();
                    _ndoEquipment.ClearSelectionValues();

                    //Clear Load Port
                    _ndoLoadPort.ClearData();
                    _ndoLoadPort.ClearSelectionValues();

                    //Clear Move Out Controls
                    _txtMoveOutQtyField.ClearData();
                    _subNextStepField.ClearData();

                    //Clear Required ActivitiesEx
                    JQDataGrid _gridRequiredActivitesEx = Page.FindCamstarControl("RequiredActivitiesEx") as JQDataGrid;
                    _gridRequiredActivitesEx.ClearData();

                    //Clear Workflow
                    WIPMain_Workflow.ClearData();
                    WIPMain_WorkflowStep.ClearData();

                }
                if (ClearFlag == 0)
                {
                    _ddlTxnDataList.ClearData();
                    _ddlTxnDataList.DropDownControl.Items.Clear();
                    _txtCommentsField.ClearData();
                    _txtWIPInstructions.ClearData();
                    _txtSelectedContainerID.ClearData();

                    _envWIPMainInfoEnvelop.SS_ContainersList = null;
                    _envWIPMainInfoEnvelop.SS_DataPacket = null;

                    //Clear selection controls
                    _gridItemData.ClearData();
                    _gridLotData.ClearData();

                    //Clear process type controls
                    _ndoProcessType.ClearData();
                    _ndoProcessType.ClearSelectionValues();

                    //Clear Check Sheet RDO control
                    _rdoEProcField.ClearData();

                    DisableActionIcons();

                    _chkSPCDataAvailable.CheckControl.Checked = false;
                    ViewState[_SPCViewStateIdentifier] = null;
                    Page.Session[_SPCTxnDataSessionIdentifier] = null;
                    Page.Session[_ShowSPCChartSessionIdentifier] = null;

                    SetControls("", "Shutdown", true);

                    //Clear Selected Lot field & data contract member
                    _txtSelectedContainerID.ClearData();

                    Page.DataContract.SetValueByName("WIPMain_SelectionId", null); //commented out because this line caused error for WIP Main when called from Resource Layout / Container Search page.
                }

                Page.DataContract.SetValueByName("WIPMain_Equipment_DM", null);// fix for T136256: clear specific data contract members that causes stuff to go screwy when selecting from the lot selection popup

                ResetRibbonCss();

                _txtSelectionId.Focus();

                //clear the container list
                Page.Session[_WIPMainContainerList] = null;
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        private void ResetRibbonCss()
        {
            _MoveInTile.CssClass = "inactive";
            CamstarWebControl.SetRenderToClient(_MoveOutTile);

            _TrackInTile.CssClass = "inactive";
            CamstarWebControl.SetRenderToClient(_MoveOutTile);

            _TrackOutTile.CssClass = "inactive";
            CamstarWebControl.SetRenderToClient(_MoveOutTile);

            _MoveOutTile.CssClass = "inactive";
            CamstarWebControl.SetRenderToClient(_MoveOutTile);
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override bool PreExecute(Info serviceInfo, Service serviceData)
        {
            _postExecuteServices = new Hashtable();
            Page.Session[_PostExecuteServicesViewStateIdentifier] = _postExecuteServices;
            return base.PreExecute(serviceInfo, serviceData);
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
            if (serviceData != null)
            {
                _postExecuteServices = Page.Session[_PostExecuteServicesViewStateIdentifier] as Hashtable;
                if (!_postExecuteServices.ContainsKey(serviceData.GetType().Name))
                {
                    _postExecuteServices.Add(serviceData.GetType().Name, serviceData.GetType().Name);
                    Page.Session[_PostExecuteServicesViewStateIdentifier] = _postExecuteServices;

                    if (serviceData is OM.LotRejectsInProcess || serviceData is OM.LotBinsInProcess || serviceData is OM.CarrierEquipmentRejects || serviceData is OM.SamplingWIPData)
                        FetchTxnData(FetchTxnDataEvents.PopupWithQtyChange);

                    if (serviceData is OM.LotBinsPostProcess || serviceData is OM.LotRejectsPostProcess || serviceData is OM.LotBinsDispose || serviceData is LotInProcessSplit)
                        if (_txtSelectedContainerID.Data != null)
                            FetchTxnData(FetchTxnDataEvents.PopupWithQtyChange);

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
                }
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
        public void DisplayAlerts(ResultStatus status, string[] ExtAlertMessages, out bool AlertMessageAvailable, out string CompletionMessage)
        {
            // check for alert messages
            string[] sAlertMessages;
            string sCompletionMessage = "";
            bool bAlertMsg = false;
            List<string> lstAlertMessages = new List<string>();

            AlertMessageAvailable = false;

            if (status.IsSuccess)
            {
                bAlertMsg = SEMI.AppCode.UIUtility.AlertMessagesAvailable(status.Message, out sAlertMessages, out sCompletionMessage);
                AlertMessageAvailable = bAlertMsg;

                status.Message = sCompletionMessage;
                // merge all all alert messages (from the completion message and from the input string list)
                foreach (string sMessage in sAlertMessages)
                    lstAlertMessages.Add(sMessage);
            }


            if (ExtAlertMessages != null)
                if (ExtAlertMessages.Length > 0)
                {
                    bAlertMsg = true;
                    // merge all all alert messages (from the completion message and from the input string list)
                    foreach (string sExtMessage in ExtAlertMessages)
                        lstAlertMessages.Add(sExtMessage);
                }


            if (bAlertMsg)
            {
                SEMI.AppCode.DataPacket oData = new DataPacket();
                oData.IsAlertMessageAvailable = false;
                oData.AlertMessages = lstAlertMessages.ToArray();
                oData.ResultStatusMessage = sCompletionMessage;
                _envWIPMainInfoEnvelop.SS_DataPacket = oData;
                PopupWIPMessages(false);
            }

            CompletionMessage = status.Message;
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public void SetSPCControls(bool SPCDataAvailable, SPCTxnData[] SPCTxnDataList = null)
        {
            if (SPCDataAvailable)
            {
                //ViewState[_SPCViewStateIdentifier] = SPCTxnDataList;
                //Page.SessionVariables["SPCTxnDataList"] = SPCTxnDataList;
                Page.Session[_SPCTxnDataSessionIdentifier] = SPCTxnDataList;
                Page.Session[_ShowSPCChartSessionIdentifier] = true;
                _chkSPCDataAvailable.CheckControl.Checked = true;
            }
            else
            {
                //ViewState[_SPCViewStateIdentifier] = null;
                Page.Session[_SPCTxnDataSessionIdentifier] = null;
                Page.Session[_ShowSPCChartSessionIdentifier] = null;
                _chkSPCDataAvailable.CheckControl.Checked = false;
            }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public SPCTxnData[] GetSPCTxnDataViewState()
        {
            SPCTxnData[] oTxnData = null;
            //if (ViewState[_SPCViewStateIdentifier] != null)
            //    oTxnData = ViewState[_SPCViewStateIdentifier] as SPCTxnData[];
            if (Page.Session[_SPCTxnDataSessionIdentifier] != null)
            {
                //oTxnData = Page.SessionVariables["SPCTxnDataList"] as SPCTxnData[];
                oTxnData = Page.Session[_SPCTxnDataSessionIdentifier] as SPCTxnData[];
                Page.Session[_ShowSPCChartSessionIdentifier] = null;
            }

            return oTxnData;
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
        public void OnPopupClose()
        {
            if (Page.DataContract.GetValueByName("WIPMain_SelectedEquipment_DM") != null)
            {
                if (_rdbResourceSelect.RadioControl.Checked)
                {
                    string sEquipment = Page.DataContract.GetValueByName("WIPMain_SelectedEquipment_DM").ToString();
                    _ndoEquipment.Data = sEquipment;
                    EquipmentDataChange();
                }
            }

            string[] sContainers = Page.DataContract.GetValueByName("WIPMain_LotList_DM") as string[];
            int iCurrentLotCount = _gridLotData.BoundContext.GetTotalRows();

            //check container object to avoid null reference exception
            if (_txtSelectedContainerID.Data != null || IsCalledViaEquipmentMode())
            {
                if (IsCalledViaEquipmentMode() && _txtSelectedContainerID.Data == null)
                    _txtSelectedContainerID.Data = sContainers[0].ToString();

                FetchTxnData(FetchTxnDataEvents.PopupWithQtyChange, sContainers); //fetch new data due to qty change
            }

            if (_gridLotData.Data != null && _gridRequiredActivitesEx.Data != null && iCurrentLotCount > 0)
                RefreshRequiredActivitiesList();

            //clear the data contract to prevent a secondary fetchTxnData call (which clears the Equipment field and makes things go bonkers)
            Page.PortalContext.DataContract.SetValueByName("WIPMain_SelectionId", null);

            Page.DataContract.SetValueByName("WIPMain_SelectedEquipment_DM", null);

            // check for the lots
            if (Page.DataContract.GetValueByName("WIPMain_LotList_DM") != null)
            {
                sContainers = Page.DataContract.GetValueByName("WIPMain_LotList_DM") as string[];
                /*commented below line for CPR 258945*/
                // _txtSelectedContainerID.Data = sContainers[0].ToString();
                _envWIPMainLotList.SS_ContainersList = null;
                Page.DataContract.SetValueByName("WIPMain_LotList_DM", null);
                FetchTxnData(FetchTxnDataEvents.SelectionIdEntry, sContainers);

                // set the data of the radio buttons to force the UI to display proper
                if (_rdbTrackInRadioButton.RadioControl.Checked)
                    _rdbTrackInRadioButton.Data = true;

                if (_rdbTrackOutRadioButton.RadioControl.Checked)
                    _rdbTrackOutRadioButton.Data = true;

                if (_rdbMoveInRadioButton.RadioControl.Checked)
                    _rdbMoveInRadioButton.Data = true;

                if (_rdbMoveOutRadioButton.RadioControl.Checked)
                    _rdbMoveOutRadioButton.Data = true;

            }

            // check to see if there is a need to display the status message after closing the alert message popup
            if (Page.DataContract.GetValueByName("WIPMain_DataEnvelopDM") != null)
            {
                _envWIPMainInfoEnvelop.SS_DataPacket = Page.DataContract.GetValueByName("WIPMain_DataEnvelopDM") as DataPacket;
                string sStatusMessage = _envWIPMainInfoEnvelop.SS_DataPacket.ResultStatusMessage;

                string[] sAlertMessages = _envWIPMainInfoEnvelop.SS_DataPacket.AlertMessages;
                bool bIsAlertMessageAvailable = _envWIPMainInfoEnvelop.SS_DataPacket.IsAlertMessageAvailable;
                int iAlertMessageCnt = 0;

                if (sAlertMessages != null)
                    iAlertMessageCnt = _envWIPMainInfoEnvelop.SS_DataPacket.AlertMessages.Count();

                if (sAlertMessages != null)
                    if (bIsAlertMessageAvailable)
                        if (iAlertMessageCnt >= 1)
                            DisplayAlerts(sAlertMessages);

                _envWIPMainInfoEnvelop.SS_DataPacket = null;
                Page.DataContract.SetValueByName("WIPMain_DataEnvelopDM", null);

                Page.DisplayMessage(sStatusMessage, true);
            }

            if (Page.Session[_ShowSPCChartSessionIdentifier] != null)
            {
                LastSPCChartAction();
            }

            if (_ndoEquipment.Data != null && GetTxnData("RequiredLoadPort").ToUpper() == "TRUE" && GetWIPFlag() == "1")
            {
                _ndoLoadPort.Visible = true;
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
        // SetRequiredActivities
        //---------------------------------------------------
        private void SetRequiredActivities(scsRequiredActivityEx[] RequiredActivitiesEx)
        {
            // --ExecutionPathPlaceHolder
            try
            {
                if (RequiredActivitiesEx != null)
                {
                    DataTable dtReqActEx = new DataTable();
                    dtReqActEx.Columns.Add("Activity", typeof(String));
                    dtReqActEx.Columns.Add("Status", typeof(String));
                    dtReqActEx.Columns.Add("Message", typeof(String));
                    dtReqActEx.Columns.Add("ActivityValue", typeof(String));
                    dtReqActEx.Columns.Add("StatusValue", typeof(String));
                    dtReqActEx.Columns.Add("ClickEnabled", typeof(String));

                    for (int i = 0; i < RequiredActivitiesEx.Count(); i++)
                    {
                        DataRow dtRow = dtReqActEx.NewRow();
                        dtRow.SetField("Activity", RequiredActivitiesEx[i].Activity.ToString());
                        string status = RequiredActivitiesEx[i].Status.ToString();
                        var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);

                        if (RequiredActivitiesEx[i].Status.ToString() == "NotExecuted")
                            status = labelCache.GetLabelByName("TaskStatusEnum_NotExecuted").Value;

                        if (RequiredActivitiesEx[i].StatusMessage != null && !string.IsNullOrEmpty(RequiredActivitiesEx[i].StatusMessage.ToString()))
                            status = RequiredActivitiesEx[i].StatusMessage.ToString();

                        dtRow.SetField("Status", status);
                        dtRow.SetField("Message", RequiredActivitiesEx[i].Message);
                        dtRow.SetField("ActivityValue", RequiredActivitiesEx[i].Activity.Value);
                        dtRow.SetField("StatusValue", RequiredActivitiesEx[i].Status.Value);

                        string sEnabled = "Y";
                        if (RequiredActivitiesEx[i].Activity.Value == "2")
                        {
                            if (_rdoEProcField.Data == null)
                            {
                                sEnabled = "N";
                            }
                        }

                        dtRow.SetField("ClickEnabled", sEnabled);
                        dtReqActEx.Rows.Add(dtRow);
                    }
                    JQDataGrid _gridRequiredActivitesEx = Page.FindCamstarControl("RequiredActivitiesEx") as JQDataGrid;
                    _gridRequiredActivitesEx.ClearData();
                    SEMI.AppCode.GridUtility.ItemListGrid_SetColumns(this, dtReqActEx, _gridRequiredActivitesEx.ID);
                    SEMI.AppCode.GridUtility.ItemListGrid_BindDataTable(this, dtReqActEx, ref _gridRequiredActivitesEx);
                }
            }
            catch (Exception ex) //Catch errors
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }

        }

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
                    System.IO.StreamWriter oWriter = new System.IO.StreamWriter(@"C:\temp\WIPMain_ExecutionPath.log", true);
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
        //
        //---------------------------------------------------
        protected virtual bool RefreshRequiredActivitiesList()
        {
            // --ExecutionPathPlaceHolder
            try
            {
                string sServiceType = _txtPrimaryServiceType.Data != null ? _txtPrimaryServiceType.Data.ToString() : "WIPMain";
                string sWIPFlag = GetWIPFlag();

                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                var fieldList = new string[] { "LOT", "WAFER", "CARRIER", "BATCHID", "RESOURCE", "WAFERBATCHID" };
                var selectionIds =
                    (from s in fieldList
                     select new Primitive<string>(s)).ToArray();

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
                // set the containers 
                // total the containers in the _gridLotData and the SelectionLotIds parameter
                //-----------------------------------------------------
                int iIndex = iCurrentLotCount;

                if (iIndex > 0)
                {
                    (oServiceData as WIPMain).Containers = new ContainerRef[iIndex];
                    for (int x = 0; x <= iCurrentLotCount - 1; x++)
                    {
                        (oServiceData as WIPMain).Containers[x] = new ContainerRef();
                        if (IsCalledViaResourceLayoutView()) // use the rowid to get the value instead of the direct cell since the UI has not been rendered yet
                            (oServiceData as WIPMain).Containers[x].Name = (_gridLotData.GridContext as BoundContext).GetCell(x.ToString().PadLeft(6, '0'), "Container").ToString();
                        else
                            (oServiceData as WIPMain).Containers[x].Name = (_gridLotData.GridContext as BoundContext).GetCell(x.ToString().PadLeft(6, '0'), "Container").ToString();
                    }

                } // if (iIndex > 0)

                //++++++++++++++++++++++++++
                // BEGIN: set additional data if there are existing lots
                //++++++++++++++++++++++++++
                if (iCurrentLotCount > 0)
                {
                    //-----------------------------------------------------
                    // pass in the 1st container if the transaction is not a ResolveSelectionID
                    //-----------------------------------------------------                    
                    (oServiceData as WIPMain).Container = new ContainerRef();
                    if (IsCalledViaResourceLayoutView()) // use rowID to get the cell value instead of the direct cell as the UI has not been rendered yet
                        (oServiceData as WIPMain).Container.Name = (_gridLotData.GridContext as BoundContext).GetCell("000000", "Container").ToString();
                    else
                        (oServiceData as WIPMain).Container.Name = (_gridLotData.GridContext as BoundContext).GetCell(0, "Container").ToString();

                    //-----------------------------------------------------
                    // pass in the ProcessType if selected
                    //-----------------------------------------------------
                    if (_ndoProcessType.Data != null)
                        (oServiceData as WIPMain).ProcessType = _ndoProcessType.Data as NamedObjectRef;

                    if (_ndoEquipment.Data != null)
                        (oServiceData as WIPMain).Equipment = new NamedObjectRef(_ndoEquipment.Data.ToString());//_ndoEquipment.Data as NamedObjectRef;   

                    (oServiceData as WIPMain).WIPFlag = int.Parse(sWIPFlag);
                } // if (iCurrentLotCount > 0)

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
                {
                    oServiceInfo.RequiredActivities = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredActivitiesEx = new scsRequiredActivityEx_Info();
                    oServiceInfo.RequiredActivitiesEx.Activity = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredActivitiesEx.Status = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredActivitiesEx.Message = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredActivitiesEx.StatusMessage = FieldInfoUtil.RequestValue();
                    oServiceInfo.RequiredCheckSheet = FieldInfoUtil.RequestValue();
                }

                //-----------------------------------------------------
                // request the data
                //-----------------------------------------------------                

                var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
                (oRequest as Request).Info = oServiceInfo;

                ResultStatus oResultStatus = new ResultStatus();
                Result oResult = new Result();
                oResultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oResult);

                if (oResultStatus.IsSuccess)
                {
                    //-----------------------------------------------------
                    // store the return information into the TxnDataList
                    //-----------------------------------------------------
                    var oResultValue = oResult.Value as WIPMain;

                    if (oResultValue.RequiredCheckSheet != null)
                    {
                        _rdoEProcField.Data = oResultValue.RequiredCheckSheet;
                        // force the checksheet name in to the data contract member
                        //this is for the issue where the checksheet name does not bubble up into the datacontract member when accessing WIP Main from ResourceLayout
                        Page.DataContract.SetValueByName("WIPMain_HiddenElectronicProcedure_DM", _rdoEProcField.Data);
                    }

                    //-----------------------------------------------------
                    // Set the required activities
                    //-----------------------------------------------------
                    if (oServiceInfo.RequiredActivities != null)
                        if (oResultValue.RequiredActivities != null)
                        {
                            SetRequiredActivities(oResultValue.RequiredActivitiesEx);
                        }

                } // if (oResultStatus.IsSuccess)
                else
                {
                    DisplayMessage(oResultStatus);
                }

                return true;
            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus("FetchTxnData::" + ex.Message.ToString(), false));
                return false;
            }
        } // RefreshRequiredActivitiesList

        //---------------------------------------------------
        //WaferSampling_ActivityTileUpdate
        protected virtual void WaferSampling_ActivityTileUpdate(WIPMain oResultValue)
        {
            if (_gridItemData.SelectedRowCount > 0)
            {
                for (int i = 0; i < oResultValue.RequiredActivitiesEx.Count(); i++)
                {
                    if (oResultValue.RequiredActivitiesEx[i].Activity == OM.scsRequiredActivityEnum.WaferSampling)
                    {
                        oResultValue.RequiredActivitiesEx[i].Status = OM.scsRequiredActivityStatusEnum.WafersSelected;
                    }

                }

            }
            else
            {
                for (int i = 0; i < oResultValue.RequiredActivitiesEx.Count(); i++)
                {
                    if (oResultValue.RequiredActivitiesEx[i].Activity == OM.scsRequiredActivityEnum.WaferSampling)
                    {
                        oResultValue.RequiredActivitiesEx[i].Status = OM.scsRequiredActivityStatusEnum.WafersNotSelected;
                    }
                }
            }
        }

        //WaferSampling_ActivityTileUpdate
        //---------------------------------------------------
        private void WIPMessagesAction()
        {
            if (_txtSelectedContainerID.Data != null)
                if (_txtSelectedContainerID.Data.ToString() != "")
                {
                    // get the session and user profile
                    var fs = FrameworkManagerUtil.GetFrameworkSession();

                    AssemblyMotherLotWIPMainService oService = new AssemblyMotherLotWIPMainService(fs.CurrentUserProfile);
                    AssemblyMotherLotWIPMain oServiceData = new AssemblyMotherLotWIPMain();
                    AssemblyMotherLotWIPMain_Info oServiceInfo = new AssemblyMotherLotWIPMain_Info();
                    AssemblyMotherLotWIPMain_Request oRequest = new AssemblyMotherLotWIPMain_Request();
                    AssemblyMotherLotWIPMain_Result oResult = new AssemblyMotherLotWIPMain_Result();

                    oServiceData.SelectionId = _txtSelectedContainerID.Data.ToString();
                    if (GetWIPFlag() != "")
                        oServiceData.WIPFlag = int.Parse(GetWIPFlag());

                    oServiceInfo.WIPInstruction = FieldInfoUtil.RequestValue();

                    oRequest = new AssemblyMotherLotWIPMain_Request();
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
                }
        } // WIPMessagesAction

        //-----------------------------------------
        //
        //-----------------------------------------
        public void LastSPCChartAction()
        {
            SPCTxnData[] oSPCTxnData = GetSPCTxnDataViewState();
            if (oSPCTxnData != null)
                DisplaySPCChart(oSPCTxnData, new ResultStatus("", true));
        } // LastSPCChartAction

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void DisableActionIcons()
        {
            /*
             * Enabling and disabling of the command bar actions, is done via the javascript function in scsWIPMainFrameworkR2.js.
             * Unable to use the CEP IsDisabled or IsHidden conditions/property, as it conflicts with the javascript that sets the action icons.
             * The ActionToggle textbox will hold the index of the controls to enable and the script will set the CSS accordingly.
             */
            //string sToggleAction = "2:F,3:F,4:F,5:F,6:F,7:F,8:F,9:F";

            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "CarrierAction").First().IsDisabled = true;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "WIPMsgAction").First().IsDisabled = true;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "OnlineTravelerAction").First().IsDisabled = true;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "DocumentSetAction").First().IsDisabled = true;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "FailuresAction").First().IsDisabled = true;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "LotInfoAction").First().IsDisabled = true;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "ResourceInfoAction").First().IsDisabled = true;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "LastSPCDisplayAction").First().IsDisabled = true;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "ProductionEventAction").First().IsDisabled = true;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "WorkflowAction").First().IsDisabled = true;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "PreTrackIn").First().IsDisabled = true;

        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void EnableActionIcons(int iContainerCount, bool bContainerSelected, string WIPFlag)
        {
            /*
            * Enabling and disabling of the command bar actions, is done via the javascript function in scsWIPMainFrameworkR2.js.
            * Unable to use the CEP IsDisabled or IsHidden conditions/property, as it conflicts with the javascript that sets the action icons.
            * The ActionToggle textbox will hold the index of the controls to enable and the script will set the CSS accordingly.
            */

            bool bLotActions = (iContainerCount > 0) && (bContainerSelected);
            bool bWIPInstruction = (iContainerCount > 0) && (bContainerSelected) && (_txtWIPInstructions.Data != null);
            bool bFailures = (iContainerCount > 0) && (WIPFlag == "4") && (bContainerSelected);
            bool bEquipmentLots = (_ndoEquipment.Data != null);
            bool bLastSPCDisplay = _chkSPCDataAvailable.CheckControl.Checked || Page.Session[_SPCTxnDataSessionIdentifier] != null;

            /*
             * 2 - Carrier
             * 3 - WIP Messages
             * 4 - Online Traveler
             * 5 - Documents
             * 6 - Failures
             * 7 - Lot Info
             * 8 - Resource Info
             * 9 - SPC Display
             * 10 - Surveillance
             * 11 - Production Event
             */

            string sActionIndex = "";

            if (bLotActions)
            {
                //sActionIndex = sActionIndex + "2:T,4:T,5:T,7:T,";
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "CarrierAction").First().IsDisabled = false;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "OnlineTravelerAction").First().IsDisabled = false;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "DocumentSetAction").First().IsDisabled = false;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "LotInfoAction").First().IsDisabled = false;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "ProductionEventAction").First().IsDisabled = false;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "WorkflowAction").First().IsDisabled = false;
                if (GetWIPFlag() == "1")
                {
                    this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "PreTrackIn").First().IsDisabled = false;
                }

            }
            else
            {
                //sActionIndex = sActionIndex + "2:F,4:F,5:F,7:F,";
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "CarrierAction").First().IsDisabled = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "OnlineTravelerAction").First().IsDisabled = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "DocumentSetAction").First().IsDisabled = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "LotInfoAction").First().IsDisabled = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "ProductionEventAction").First().IsDisabled = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "WorkflowAction").First().IsDisabled = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "PreTrackIn").First().IsDisabled = true;
            }

            if (bWIPInstruction)
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "WIPMsgAction").First().IsDisabled = false;  //sActionIndex = sActionIndex + "3:T,";
            else
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "WIPMsgAction").First().IsDisabled = true;   //sActionIndex = sActionIndex + "3:F,";

            if (bFailures)
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "FailuresAction").First().IsDisabled = false; //sActionIndex = sActionIndex + "6:T,";
            else
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "FailuresAction").First().IsDisabled = true;  //sActionIndex = sActionIndex + "6:F,";

            if (bEquipmentLots)
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "ResourceInfoAction").First().IsDisabled = false; //sActionIndex = sActionIndex + "8:T,";
            else
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "ResourceInfoAction").First().IsDisabled = true;  //sActionIndex = sActionIndex + "8:F,";

            if (bLastSPCDisplay)
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "LastSPCDisplayAction").First().IsDisabled = false;   // sActionIndex = sActionIndex + "9:T";
            else
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "LastSPCDisplayAction").First().IsDisabled = true;    // sActionIndex = sActionIndex + "9:F";

        }

        //---------------------------------------------------
        // IsCalledViaEquipmentMode
        //---------------------------------------------------
        private bool IsCalledViaEquipmentMode()
        {
            if (Page.DataContract.GetValueByName("WIPMain_SelectedEquipment_DM") != null)
                return true;
            else
                return false;

        }


    } // public class SS_WIPMain
}