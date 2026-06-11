/* Copyright 2022 Siemens */
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
    public class SS_LotReject : scsShopfloorBase
    {
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotReject_SelectionId") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("LotReject_Employee") as CWC.NamedObject; } }
        protected CWC.CheckBox _chkIsWaferProcessing { get { return Page.FindCamstarControl("LotReject_IsWaferProcessing") as CWC.CheckBox; } }
        protected ContainerListGrid _listContainer { get { return Page.FindCamstarControl("LotReject_Container") as ContainerListGrid; } }
        protected CWC.NamedObject _ndoProcessType { get { return Page.FindCamstarControl("LotReject_ProcessType") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEquipment { get { return Page.FindCamstarControl("LotReject_Equipment") as CWC.NamedObject; } }
        protected CWC.TextBox _txtMaxStandbyQty { get { return Page.FindCamstarControl("LotReject_MaxStandbyQty") as CWC.TextBox; } }
        protected CWC.TextBox _txtMaxQtyToProcess { get { return Page.FindCamstarControl("LotReject_MaxQtyToProcess") as CWC.TextBox; } }
        protected CWC.TextBox _txtMaxInProcessQty { get { return Page.FindCamstarControl("LotReject_MaxInProcessQty") as CWC.TextBox; } }
        protected CWC.TextBox _txtMaxProcessedQty { get { return Page.FindCamstarControl("LotReject_MaxProcessedQty") as CWC.TextBox; } }
        protected JQDataGrid _gridLotInfo { get { return Page.FindCamstarControl("LotReject_LotInfo") as JQDataGrid; } }
        protected JQDataGrid _gridLotWafers { get { return Page.FindCamstarControl("LotReject_LotWafers") as JQDataGrid; } }
        protected JQDataGrid _gridDetails { get { return Page.FindCamstarControl("LotReject_Details") as JQDataGrid; } }
        protected DataEnvelopControl _envContainers { get { return Page.FindCamstarControl("ContainerLists") as DataEnvelopControl; } }
        protected DataEnvelopControl _envWaferMapDetails { get { return Page.FindCamstarControl("WaferMapDetails") as DataEnvelopControl; } }
        protected DataEnvelopControl _envDataCollection { get { return Page.FindCamstarControl("EnvDataCollection") as DataEnvelopControl; } }
        protected CWC.TextBox _txtQuantity { get { return Page.FindCamstarControl("Quantity") as CWC.TextBox; } }
        protected CWC.TextBox _txtPrimarySvcType { get { return Page.FindCamstarControl("PrimarySvcType") as CWC.TextBox; } }
        protected CWC.TextBox _txtSelectedRowId { get { return Page.FindCamstarControl("SelectedRowId") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoWaferScribeNumberInlineControl { get { return FindControl("LotReject_Details_WaferScribeNumber_InlineEditorControl") as CWC.NamedObject; } }
        protected CWC.TextBox _txtComments { get { return Page.FindCamstarControl("Shopfloor_Comments") as CWC.TextBox; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("LotReject_ComputerName") as CWC.TextBox; } }
		protected ToggleContainer _togglecontainerControl { get { return Page.FindCamstarControl("CommentToggle") as ToggleContainer; } }
        
        //---------------------------------------------------
        // Clear Controls function
        //---------------------------------------------------
        private void ClearControls(int ClearFlag)
        {
            try
            {
                Page.StatusBar.ClearMessage();
                if (ClearFlag <= 20)
                {
                    _gridLotInfo.ClearData();
                    _listContainer.ClearData();
                    _ndoProcessType.ClearData();
                    _ndoProcessType.ClearSelectionValues();
                    _gridDetails.ClearData();
                    _gridLotWafers.ClearData();
                    _txtComments.ClearData();
                }
                if (ClearFlag <= 30)
                {
                    _ndoEquipment.ClearData();
                    _ndoEquipment.ClearSelectionValues();
                }
                if (ClearFlag <= 40)
                {
                    _txtMaxStandbyQty.Data = "0";
                    _txtMaxQtyToProcess.Data = "0";
                    _txtMaxInProcessQty.Data = "0";
                    _txtMaxProcessedQty.Data = "0";
                }
                if (ClearFlag <= 10)
                {
                    _ndoEmployee.ClearData();
                    _txtSelectionId.ClearData();
                    SetControls();
                    _txtSelectionId.Focus();
                }
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.TargetSite.Name + "(): " + Ex.Message);
            }
        }

        //---------------------------------------------------
        // Set Controls function
        //---------------------------------------------------
        private void SetControls()
        {
            try
            {
                int iFlag;
                if (_listContainer.Data == null)
                {
                    iFlag = 0;
                    _gridLotWafers.Visible = false;
                    _ndoProcessType.Visible = false;
                    _ndoEquipment.Visible = false;
                    _txtMaxStandbyQty.Visible = false;
                    _txtMaxQtyToProcess.Visible = false;
                    _txtMaxInProcessQty.Visible = false;
                    _txtMaxProcessedQty.Visible = false;
                }
                else if (_chkIsWaferProcessing.IsChecked)
                {
                    iFlag = 1;
                    _gridLotWafers.Visible = true;
                    _ndoProcessType.Visible = false;
                    _ndoEquipment.Visible = false;
                    _txtMaxStandbyQty.Visible = false;
                    _txtMaxQtyToProcess.Visible = false;
                    _txtMaxInProcessQty.Visible = false;
                    _txtMaxProcessedQty.Visible = false;
                }
                else
                {
                    iFlag = 2;
                    _gridLotWafers.Visible = false;
                    _ndoProcessType.Visible = true;
                    _ndoEquipment.Visible = true;
                    _txtMaxStandbyQty.Visible = true;
                    _txtMaxQtyToProcess.Visible = true;
                    _txtMaxInProcessQty.Visible = true;
                    _txtMaxProcessedQty.Visible = true;
                }

                for (int i = 0; i < _gridDetails.Settings.Columns.Count(); i++)
                {
                    if ((_gridDetails.GridContext as BoundContext).Fields[i].ID == "WaferScribeNumber")
                        (_gridDetails.GridContext as BoundContext).Fields[i].Visible = !(iFlag != 1);
                    else if ((_gridDetails.GridContext as BoundContext).Fields[i].ID == "WaferRejectsQty")
                        (_gridDetails.GridContext as BoundContext).Fields[i].Visible = !(iFlag != 1);
                    else if ((_gridDetails.GridContext as BoundContext).Fields[i].ID == "StandbyQty")
                        (_gridDetails.GridContext as BoundContext).Fields[i].Visible = !(iFlag != 2);
                    else if ((_gridDetails.GridContext as BoundContext).Fields[i].ID == "QtyToProcess")
                        (_gridDetails.GridContext as BoundContext).Fields[i].Visible = !(iFlag != 2);
                    else if ((_gridDetails.GridContext as BoundContext).Fields[i].ID == "InProcessQty")
                        (_gridDetails.GridContext as BoundContext).Fields[i].Visible = !(iFlag != 2);
                    else if ((_gridDetails.GridContext as BoundContext).Fields[i].ID == "ProcessedQty")
                        (_gridDetails.GridContext as BoundContext).Fields[i].Visible = !(iFlag != 2);
                    else if ((_gridDetails.GridContext as BoundContext).Fields[i].ID == "WaferMapDetailsBtn")
                        (_gridDetails.GridContext as BoundContext).Fields[i].Visible = !(iFlag != 1);
                }
                //Hide/show wafer map details
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.TargetSite.Name + "(): " + Ex.Message);
            }
        }

        //-----------------------------------------
        // Selection Id Data Changed Event
        //-----------------------------------------
        public void SelectionIdField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                ClearControls(20);
                if (_txtSelectionId.Data != null)
                {
                    FetchData("SelectionId");
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // ProcessType Data Changed Event
        //-----------------------------------------
        public void ProcessTypeField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_chkIsWaferProcessing.IsChecked)
                {
                    ClearControls(30);
                    if (_ndoProcessType.Data != null)
                    {
                        FetchData("ProcessType");
                    }
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Equipment Data Changed Event
        //-----------------------------------------
        public void EquipmentField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_chkIsWaferProcessing.IsChecked)
                {
                    ClearControls(40);
                    if (_ndoEquipment.Data != null)
                    {
                        FetchData("Equipment");
                    }
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //---------------------------------------------------
        // Fetch Data function
        //---------------------------------------------------
        public void FetchData(string EventName)
        {
            try
            {
                //Initialize Service & Objects
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                LotRejectService Svc = new LotRejectService(profile);
                LotReject SvcData = new LotReject();
                LotReject_Info SvcInfo = new LotReject_Info();
                LotReject_Request ReqData = new LotReject_Request();
                LotReject_Result ResData = new LotReject_Result();
                string sServiceTypeName = "";

                if (EventName == "SelectionId")
                {
                    sServiceTypeName = "ResolveSelectionId";
                    SvcData.SelectionId = _txtSelectionId.Data.ToString();
                    SvcInfo.SelectionContainer = new Info(true);
                    SvcInfo.ProcessTypeSelection = new Info(true);
                    SvcInfo.IsWaferProcessing = new Info(true);
                    SvcInfo.LotWafers = new LotWafers_Info();
                    SvcInfo.LotWafers.WaferScribeNumber = new Info(true);
                    SvcInfo.LotWafers.WaferNumber = new Info(true);
                    SvcInfo.LotWafers.NDPW = new Info(true);
                    SvcInfo.LotWafers.GoodQty = new Info(true);
                }
                else
                {
                    SvcData.Container = new ContainerRef();
                    SvcData.Container.Name = _listContainer.Data.ToString();
                    SvcData.ProcessType = new NamedObjectRef();
                    SvcData.ProcessType.Name = _ndoProcessType.Data.ToString();
                    if (EventName == "ProcessType")
                    {
                        SvcInfo.EquipmentSelection = new Info(true);
                    }
                    else if (EventName == "Equipment")
                    {
                        SvcData.Equipment = new NamedObjectRef();
                        SvcData.Equipment.Name = _ndoEquipment.Data.ToString();
                    }
                }
                SvcInfo.MaxStandbyQty = FieldInfoUtil.RequestValue();
                SvcInfo.MaxQtyToProcess = FieldInfoUtil.RequestValue();
                SvcInfo.MaxInProcessQty = FieldInfoUtil.RequestValue();
                SvcInfo.MaxProcessedQty = FieldInfoUtil.RequestValue();
                ReqData.Info = SvcInfo;

                //Execute Request
                ResultStatus Results;
                if (sServiceTypeName == "ResolveSelectionId")
                {
                    Results = Svc.ResolveSelectionId(SvcData, ReqData, out ResData);
                }
                else
                {
                    Results = Svc.Load(SvcData, ReqData, out ResData);
                }

                //Result				
                if (Results.IsSuccess)
                {
                    //Display the data
                    _txtMaxStandbyQty.Data = ResData.Value.MaxStandbyQty;
                    _txtMaxQtyToProcess.Data = ResData.Value.MaxQtyToProcess;
                    _txtMaxInProcessQty.Data = ResData.Value.MaxInProcessQty;
                    _txtMaxProcessedQty.Data = ResData.Value.MaxProcessedQty;
                    if (EventName == "SelectionId")
                    {
                        _gridDetails.ClearData();
                        _listContainer.Data = ResData.Value.SelectionContainer;
                        if (ResData.Value.ProcessTypeSelection != null)
                        {
                            CWC.NamedObject _ndoProcessTypeTemp = _ndoProcessType;
                            SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoProcessTypeTemp, ResData.Value.ProcessTypeSelection);
                            _ndoProcessType.Data = ResData.Value.ProcessTypeSelection[0];
                        }
                        _chkIsWaferProcessing.Data = Convert.ToBoolean(ResData.Value.IsWaferProcessing.ToString());
                        if (ResData.Value.IsWaferProcessing == true)
                        {
                            _gridLotWafers.ClearData();
                            _gridLotWafers.Data = ResData.Value.LotWafers;
                            _gridLotWafers.OriginalData = ResData.Value.LotWafers;
                            CamstarWebControl.SetRenderToClient(_gridLotWafers);
                        }

                        //Fetch lot info
                        JQDataGrid theGrid = _gridLotInfo;
						RecordSet rs = SEMI.AppCode.UIUtility.GetLotQuerySelection(this, this.PrimaryServiceType, ResData.Value.SelectionContainer.Name);
						DataTable containersDataTable = rs.GetAsExplicitlyDataTable();
                        theGrid.ClearData();
                        SEMI.AppCode.GridUtility.ItemListGrid_SetColumns(this, containersDataTable, theGrid.ID, null, "RefTargetGrid",false, null, true, null, rs.Headers);
                        SEMI.AppCode.GridUtility.ItemListGrid_BindDataTable(this, containersDataTable, ref theGrid);

                        //Set the control
                        SetControls();

                        //Trigger process type if selected
                        if (ResData.Value.IsWaferProcessing == false)
                        {
                            if (_ndoProcessType.Data != null)
                            {
                                ProcessTypeField_DataChanged(null, null);
                            }
                        }
                    }
                    else if (EventName == "ProcessType")
                    {
                        if (ResData.Value.EquipmentSelection != null)
                        {
                            CWC.NamedObject _ndoEquipmentTemp = _ndoEquipment;
                            SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoEquipmentTemp, ResData.Value.EquipmentSelection);
                            _ndoEquipment.Data = ResData.Value.EquipmentSelection[0];
                            EquipmentField_DataChanged(null, null);
                        }
                    }
                }
                else
                {
                    Page.DisplayMessage(Results);
                }
            } //Results.IsSuccess
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Wafer Scribe Number Selection Value
        //-----------------------------------------
        public void FetchWaferScribeNumber()
        {
            try
            {
                if (_gridLotWafers.Data != null)
                {
                    var _waferInline = _gridDetails.FindControl("LotReject_Details_WaferScribeNumber_InlineEditorControl") as CWC.DropDownList;
                    if (_waferInline != null && _waferInline.PickListPanelControl != null)
                    {
                        // Load static custom values into the inline control
                        var _gridItemData = _gridLotWafers as JQDataGrid;
                        var rs = new OM.RecordSet()
                        {
                            Headers = new OM.Header[] 
                            { 
                                new OM.Header() { Name = "Label" }, 
                                new OM.Header() { Name = "Value" } 
                            }
                        };

                        if (_gridItemData != null && _gridItemData.BoundContext.Data != null)
                        {
                            rs.Rows =
                                (from dc in _gridItemData.BoundContext.Data as IEnumerable<LotWafers>
                                 select new OM.Row() { Values = new string[] { dc.WaferScribeNumber.ToString(), dc.WaferScribeNumber.ToString() } }).ToArray();
                        }

                        _waferInline.PickListPanelControl.DataProvider = new Camstar.WebPortal.FormsFramework.WebControls.PickLists.StaticValuesDataProvider(rs);

                        _gridDetails.GridContext.RowUpdated += new JQGridEventHandler(GridContext_RowUpdated);
                        _gridDetails.GridContext.RowUpdating += new JQGridEventHandler(GridContext_RowUpdating);
                    }
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Row Updating function for Details grid
        //-----------------------------------------
        private string _wafeName = null;
        ResponseData GridContext_RowUpdating(object sender, JQGridEventArgs args)
        {
            if (args.InputData.Count > 0)
                _wafeName = args.InputData["WaferScribeNumber"] as string;
            return null;
        }

        //-----------------------------------------
        // Row Updated function for Details grid
        //-----------------------------------------
        ResponseData GridContext_RowUpdated(object sender, JQGridEventArgs args)
        {
            var dx = _gridDetails.BoundContext.Data as LotRejectsDetails[];
            if (dx != null)
                dx[0].WaferScribeNumber = _wafeName;
            args.Cancel = true;
            return _gridDetails.GridContext.Reload(args.State);
        }

        //-----------------------------------------
        // On Wafer Map Details
        //-----------------------------------------
        public virtual void PopupWaferMapDetails(bool EndResponse = false)
        {
            try
            {
                if (Page.DataContract.GetValueByName("LotReject_GridRowId_DM").ToString() != "" && Convert.ToUInt32(Page.DataContract.GetValueByName("LotReject_Quantity_DM").ToString()) >= 1)
                {
                    _envDataCollection.SS_RejectLotDetails = _gridDetails.Data as RejectLotDetails[];
                    _txtSelectedRowId.Data = Page.DataContract.GetValueByName("LotReject_GridRowId_DM").ToString();
                    if (_envDataCollection.SS_RejectLotDetails.Count() > Convert.ToInt32(_txtSelectedRowId.Data.ToString()))
                    {
                        if (_envDataCollection.SS_RejectLotDetails[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].WaferMapDetails != null)
                        {
                            _envWaferMapDetails.SS_WaferMapDetails = _envDataCollection.SS_RejectLotDetails[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].WaferMapDetails;
                            Page.DataContract.SetValueByName("LotReject_WaferMapDetails_DM", _envDataCollection.SS_RejectLotDetails[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].WaferMapDetails);
                        }
                        else
                        {
                            _envWaferMapDetails.SS_WaferMapDetails = null;
                            Page.DataContract.SetValueByName("LotReject_WaferMapDetails_DM", null);
                        }
                    }

                    Camstar.WebPortal.Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                    objAction.PageName = "SS_WaferMapDetailsPopupVP";

                    UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[4];
                    objLinks[0] = new UIComponentDataContractLink();
                    objLinks[0].SourceMember = "LotReject_PrimaryServiceType_DM";
                    objLinks[0].TargetMember = "Popup_PrimarySvcType_DM";
                    objLinks[1] = new UIComponentDataContractLink();
                    objLinks[1].SourceMember = "LotReject_Quantity_DM";
                    objLinks[1].TargetMember = "Popup_Quantity_DM";
                    objLinks[2] = new UIComponentDataContractLink();
                    objLinks[2].SourceMember = "LotReject_WaferMapDetails_DM";
                    objLinks[2].TargetMember = "Popup_WaferMapDetails_DM";
                    objLinks[3] = new UIComponentDataContractLink();
                    objLinks[3].SourceMember = "LotReject_GridRowId_DM";
                    objLinks[3].TargetMember = "Popup_SelectedItem_DM";

                    UIComponentDataContractReturnLink[] objReturnLinks = new UIComponentDataContractReturnLink[1];
                    objReturnLinks[0] = new UIComponentDataContractReturnLink();
                    objReturnLinks[0].SourceMember = "Popup_WaferMapDetails_DM";
                    objReturnLinks[0].TargetMember = "LotReject_WaferMapDetails_DM";
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
                if (Page.DataContract.GetValueByName("LotReject_LotList_DM") != null)
                {
                    string[] sContainers = Page.DataContract.GetValueByName("LotReject_LotList_DM") as string[];
                    _envContainers.SS_ContainersList = null;
                    Page.DataContract.SetValueByName("LotReject_LotList_DM", null);
                    _txtSelectionId.Data = sContainers[0];
                    FetchData("SelectionId");
                }
                if (Page.DataContract.GetValueByName("LotReject_WaferMapDetails_DM") != null)
                {
                    //Bind the returned data to the data envelope control & grid
                    _envDataCollection.SS_RejectLotDetails[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].WaferMapDetails = Page.DataContract.GetValueByName("LotReject_WaferMapDetails_DM") as WaferMapDetails[];
                    _gridDetails.ClearData();
                    _gridDetails.Data = _envDataCollection.SS_RejectLotDetails;
                    _gridDetails.OriginalData = _envDataCollection.SS_RejectLotDetails;
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

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
							//if (e.Result.IsSuccess)
							//	Page.CloseFloatingFrameOnSubmit(e.Result);

                            break;
                        }
                    case "WaferMapDetails":
                        {
                            PopupWaferMapDetails();
                            break;
                        }
                    case "Reset":
                        {
                            Page.ShopfloorReset(sender, e);
                            _gridLotInfo.ClearData();
                            _gridLotWafers.ClearData();
                            _gridDetails.ClearData();
                            _ndoProcessType.ClearSelectionValues();
                            _ndoEquipment.ClearSelectionValues();
                            SetControls();
                            break;
                        }
                }
            }
        }

        //---------------------------------------------------
        // Override OnLoad event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                _txtSelectionId.DataChanged += new EventHandler(SelectionIdField_DataChanged);
                _ndoProcessType.DataChanged += new EventHandler(ProcessTypeField_DataChanged);
                _ndoEquipment.DataChanged += new EventHandler(EquipmentField_DataChanged);
                if (!Page.IsPostBack)
                {
                    _txtPrimarySvcType.Data = Page.PrimaryServiceType.ToString();
                    _txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);

					string sContainer = Page.DataContract.GetValueByName("SelectedContainerNameDM") is ContainerRef? (Page.DataContract.GetValueByName("SelectedContainerNameDM") as ContainerRef).Name : Page.DataContract.GetValueByName("SelectedContainerNameDM") as string;

					if (!string.IsNullOrEmpty(sContainer))
					{
						_txtSelectionId.Data = sContainer;
						SelectionIdField_DataChanged(null, null);
					}
                }
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {
                    OnPopupClose();
                }
				FetchWaferScribeNumber();
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if (_togglecontainerControl.State == CollapsableState.Collapsed)
				_togglecontainerControl.LabelName = _togglecontainerControl.CollapsedLabelName;
			else
				_togglecontainerControl.LabelName = _togglecontainerControl.ExpandedLabelName;
		}
        //---------------------------------------------------
        // Submit Button
        //---------------------------------------------------
        public ResultStatus SubmitTransactions()
        {
            try
            {
                ResultStatus ReturnResultStatus = new ResultStatus();
                //Initialize Service & Objects
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                if (Page.PrimaryServiceType == "LotReject")
                {
                    LotRejectService Svc = new LotRejectService(profile);
                    LotReject SvcData = new LotReject();
                    LotReject_Info SvcInfo = new LotReject_Info();
                    LotReject_Request ReqData = new LotReject_Request();
                    LotReject_Result ResData = new LotReject_Result();

                    if (_txtComputerName.Data != null)
                        SvcData.ComputerName = _txtComputerName.Data.ToString();

                    if (_ndoEmployee.Data != null)
                        SvcData.Employee = _ndoEmployee.Data as NamedObjectRef;
                    if (_listContainer.Data != null)
                    {
                        SvcData.Container = new ContainerRef();
                        SvcData.Container.Name = _listContainer.Data.ToString();
                    }
                    if (_txtComments.Data != null)
                        SvcData.Comments = _txtComments.Data.ToString();

                    if (!_chkIsWaferProcessing.IsChecked)
                    {
                        if (_ndoProcessType.Data != null)
                            SvcData.ProcessType = _ndoProcessType.Data as NamedObjectRef;
                        if (_ndoEquipment.Data != null)
                            SvcData.Equipment = _ndoEquipment.Data as NamedObjectRef;
                    } //!_chkIsWaferProcessing.IsChecked

                    int iIndex = 0;
                    RejectLotDetails[] getDetails = _gridDetails.Data as RejectLotDetails[];
                    if (_chkIsWaferProcessing.IsChecked)
                    {
                        if (_gridDetails.TotalRowCount > 0)
                        {
                            SvcData.Details = new RejectLotDetails[getDetails.Length];
                            foreach (RejectLotDetails detail in getDetails)
                            {
                                SvcData.Details[iIndex] = new RejectLotDetails();
                                if (detail.WaferScribeNumber != null)
                                    SvcData.Details[iIndex].WaferScribeNumber = detail.WaferScribeNumber;
                                if (detail.Spec != null)
                                {
                                    SvcData.Details[iIndex].Spec = new RevisionedObjectRef();
                                    string[] aSpec = detail.Spec.ToString().Split(':');
                                    if (aSpec.Length > 1)
                                    {
                                        SvcData.Details[iIndex].Spec.Name = aSpec[0];
                                        SvcData.Details[iIndex].Spec.Revision = aSpec[1];
                                        SvcData.Details[iIndex].Spec.RevisionOfRecord = false;
                                    }
                                    else
                                    {
                                        SvcData.Details[iIndex].Spec.Name = aSpec[0];
                                        SvcData.Details[iIndex].Spec.Revision = "";
                                        SvcData.Details[iIndex].Spec.RevisionOfRecord = true;
                                    }
                                } //oRow.Field<string>("Spec") != null
                                if (detail.LossReason != null)
                                    SvcData.Details[iIndex].LossReason = detail.LossReason;
                                if (detail.WaferRejectsQty != null)
                                    SvcData.Details[iIndex].WaferRejectsQty = detail.WaferRejectsQty;
                                if (detail.RejectCategory != null)
                                    SvcData.Details[iIndex].RejectCategory = detail.RejectCategory;
                                if (detail.RejectCause != null)
                                    SvcData.Details[iIndex].RejectCause = detail.RejectCause;
                                if (detail.RejectComment != null)
                                    SvcData.Details[iIndex].RejectComment = detail.RejectComment;
                                if (detail.WaferMapDetails != null)
                                    SvcData.Details[iIndex].WaferMapDetails = detail.WaferMapDetails;
                                if (detail.scsChargeToResource != null)
                                    SvcData.Details[iIndex].scsChargeToResource = detail.scsChargeToResource;
                                iIndex = iIndex + 1;
                            }
                        }
                    }
                    else
                    {
                        SvcData.Details = _gridDetails.Data as RejectLotDetails[];
                    }

                    //Execute Request 
                    ResultStatus Results = Svc.ExecuteTransaction(SvcData, ReqData, out ResData);

                    //Result
                    if (Results.IsSuccess)
                    {
                        _gridLotInfo.ClearData();
                        _gridLotWafers.ClearData();
                        _gridDetails.ClearData();
                        _ndoProcessType.ClearSelectionValues();
                        _ndoEquipment.ClearSelectionValues();
                        _listContainer.ClearData();
                        _txtComments.ClearData();
                        SetControls();
                    }

                    ReturnResultStatus = Results;
                    return ReturnResultStatus;
                } //Page.PrimaryServiceType == "LotReject"
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                ResultStatus ReturnResultStatus = new ResultStatus();
                ReturnResultStatus.IsSuccess = false;
                ReturnResultStatus.Message = ex.Message.ToString();
                return ReturnResultStatus;
            }
        }
    }
}



