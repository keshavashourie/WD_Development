/* Copyright 2019 Siemens */
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
    public class SS_LotBonus : scsShopfloorBase
    {
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotBonus_SelectionId") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("LotBonus_Employee") as CWC.NamedObject; } }
        protected CWC.CheckBox _chkIsWaferProcessing { get { return Page.FindCamstarControl("LotBonus_IsWaferProcessing") as CWC.CheckBox; } }
        protected ContainerListGrid _listContainer { get { return Page.FindCamstarControl("LotBonus_Container") as ContainerListGrid; } }
        protected CWC.NamedObject _ndoProcessType { get { return Page.FindCamstarControl("LotBonus_ProcessType") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEquipment { get { return Page.FindCamstarControl("LotBonus_Equipment") as CWC.NamedObject; } }
        protected JQDataGrid _gridLotInfo { get { return Page.FindCamstarControl("LotBonus_LotInfo") as JQDataGrid; } }
        protected JQDataGrid _gridLotWafers { get { return Page.FindCamstarControl("LotBonus_LotWafers") as JQDataGrid; } }
        protected JQDataGrid _gridDetails { get { return Page.FindCamstarControl("LotBonus_Details") as JQDataGrid; } }
        protected DataEnvelopControl _envContainers { get { return Page.FindCamstarControl("ContainerLists") as DataEnvelopControl; } }
        protected DataEnvelopControl _envWaferMapDetails { get { return Page.FindCamstarControl("WaferMapDetails") as DataEnvelopControl; } }
        protected DataEnvelopControl _envDataCollection { get { return Page.FindCamstarControl("EnvDataCollection") as DataEnvelopControl; } }
        protected CWC.TextBox _txtQuantity { get { return Page.FindCamstarControl("Quantity") as CWC.TextBox; } }
        protected CWC.TextBox _txtPrimarySvcType { get { return Page.FindCamstarControl("PrimarySvcType") as CWC.TextBox; } }
        protected CWC.TextBox _txtSelectedRowId { get { return Page.FindCamstarControl("SelectedRowId") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoWaferScribeNumberInlineControl { get { return FindControl("WaferScribeNumber_InlineEditorControl") as CWC.NamedObject; } }
        protected CWC.TextBox _txtComments { get { return Page.FindCamstarControl("Shopfloor_Comments") as CWC.TextBox; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("LotBonus_ComputerName") as CWC.TextBox; } }
        Hashtable htWaferMapDetails = new Hashtable();

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
                }
                else if (_chkIsWaferProcessing.IsChecked)
                {
                    iFlag = 1;
                    _gridLotWafers.Visible = true;
                    _ndoProcessType.Visible = false;
                    _ndoEquipment.Visible = false;
                }
                else
                {
                    iFlag = 2;
                    _gridLotWafers.Visible = false;
                    _ndoProcessType.Visible = true;
                    _ndoEquipment.Visible = true;
                }

                for (int i = 0; i < _gridDetails.Settings.Columns.Count(); i++)
                {
                    if ((_gridDetails.GridContext as BoundContext).Fields[i].ID == "WaferScribeNumber")
                        (_gridDetails.GridContext as BoundContext).Fields[i].Visible = !(iFlag != 1);
                    else if ((_gridDetails.GridContext as BoundContext).Fields[i].ID == "StandbyQty")
                        (_gridDetails.GridContext as BoundContext).Fields[i].Visible = !(iFlag == 0);
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
                if (_txtSelectionId.Data != null)
                {
                    Page.StatusBar.ClearMessage();
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

        //---------------------------------------------------
        // Fetch Data function
        //---------------------------------------------------
        public void FetchData(string EventName)
        {
            try
            {
                //Initialize Service & Objects
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                LotBonusService Svc = new LotBonusService(profile);
                LotBonus SvcData = new LotBonus();
                LotBonus_Info SvcInfo = new LotBonus_Info();
                LotBonus_Request ReqData = new LotBonus_Request();
                LotBonus_Result ResData = new LotBonus_Result();

                if (EventName == "SelectionId")
                {
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
                ReqData.Info = SvcInfo;

                //Execute Request 
                ResultStatus Results = Svc.ResolveSelectionId(SvcData, ReqData, out ResData);

                //Result
                if (Results.IsSuccess)
                {
                    //Display the data
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
                        }
                    }
                } //Results.IsSuccess
                else
                {
                    Page.DisplayMessage(Results);
                }
            }
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
                    var _waferInline = _gridDetails.FindControl("LotBonus_Details_WaferScribeNumber_InlineEditorControl") as CWC.DropDownList;
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
                if (Page.DataContract.GetValueByName("LotBonus_GridWaferScribeNumber_DM").ToString() != "" && Page.DataContract.GetValueByName("LotBonus_GridBonusQty_DM") != null)
                {
                    _envDataCollection.SS_BonusLotDetails = _gridDetails.Data as BonusLotDetails[];
                    _txtSelectedRowId.Data = Page.DataContract.GetValueByName("LotBonus_GridRowId_DM").ToString();
                    if (_envDataCollection.SS_BonusLotDetails.Count() > Convert.ToInt32(_txtSelectedRowId.Data.ToString()))
                    {
                        if (_envDataCollection.SS_BonusLotDetails[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].WaferMapDetails != null)
                        {
                            _envWaferMapDetails.SS_WaferMapDetails = _envDataCollection.SS_BonusLotDetails[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].WaferMapDetails;
                            Page.DataContract.SetValueByName("LotBonus_WaferMapDetails_DM", _envDataCollection.SS_BonusLotDetails[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].WaferMapDetails);
                        }
                        else
                        {
                            _envWaferMapDetails.SS_WaferMapDetails = null;
                            Page.DataContract.SetValueByName("LotBonus_WaferMapDetails_DM", null);
                        }
                    }

                    int iQty = 0;
                    if (Page.DataContract.GetValueByName("LotBonus_GridBonusQty_DM") != null)
                    {
                        iQty = Convert.ToInt32(Page.DataContract.GetValueByName("LotBonus_GridBonusQty_DM").ToString());
                    }
                    if (iQty == 0)
                    {
                        if (Page.DataContract.GetValueByName("LotBonus_GridBonusQty_DM") != null)
                        {
                            iQty = Convert.ToInt32(Page.DataContract.GetValueByName("LotBonus_GridBonusQty_DM").ToString());
                        }
                    }
                    if (iQty > 0)
                    {
                        _txtQuantity.Data = iQty;
                    }
                    Page.CollectDataContractByName("LotBonus_Quantity_DM");

                    Camstar.WebPortal.Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                    objAction.PageName = "SS_WaferMapDetailsPopupVP";

                    UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[4];
                    objLinks[0] = new UIComponentDataContractLink();
                    objLinks[0].SourceMember = "LotBonus_PrimaryServiceType_DM";
                    objLinks[0].TargetMember = "Popup_PrimarySvcType_DM";
                    objLinks[1] = new UIComponentDataContractLink();
                    objLinks[1].SourceMember = "LotBonus_Quantity_DM";
                    objLinks[1].TargetMember = "Popup_Quantity_DM";
                    objLinks[2] = new UIComponentDataContractLink();
                    objLinks[2].SourceMember = "LotBonus_WaferMapDetails_DM";
                    objLinks[2].TargetMember = "Popup_WaferMapDetails_DM";
                    objLinks[3] = new UIComponentDataContractLink();
                    objLinks[3].SourceMember = "LotBonus_GridRowId_DM";
                    objLinks[3].TargetMember = "Popup_SelectedItem_DM";

                    UIComponentDataContractReturnLink[] objReturnLinks = new UIComponentDataContractReturnLink[1];
                    objReturnLinks[0] = new UIComponentDataContractReturnLink();
                    objReturnLinks[0].SourceMember = "Popup_WaferMapDetails_DM";
                    objReturnLinks[0].TargetMember = "LotBonus_WaferMapDetails_DM";
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
                            Page.ShopfloorReset(sender, e);
                            _gridLotInfo.ClearData();
                            _gridLotWafers.ClearData();
                            _gridDetails.ClearData();
                            _ndoProcessType.ClearSelectionValues();
                            _ndoEquipment.ClearSelectionValues();
                            SetControls();
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

        //-----------------------------------------
        // On Popup closed event
        //-----------------------------------------
        public void OnPopupClose()
        {
            try
            {
                Page.CollectDataContract();
                if (Page.DataContract.GetValueByName("LotBonus_LotList_DM") != null)
                {
                    string[] sContainers = Page.DataContract.GetValueByName("LotBonus_LotList_DM") as string[];
                    _envContainers.SS_ContainersList = null;
                    Page.DataContract.SetValueByName("LotBonus_LotList_DM", null);
                    _txtSelectionId.Data = sContainers[0];
                    FetchData("SelectionId");
                }
                if (Page.DataContract.GetValueByName("LotBonus_WaferMapDetails_DM") != null)
                {
                    //Bind the returned data to the data envelope control & grid
                    _envDataCollection.SS_BonusLotDetails[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].WaferMapDetails = Page.DataContract.GetValueByName("LotBonus_WaferMapDetails_DM") as WaferMapDetails[];
                    _gridDetails.ClearData();
                    _gridDetails.Data = _envDataCollection.SS_BonusLotDetails;
                    _gridDetails.OriginalData = _envDataCollection.SS_BonusLotDetails;
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
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
                if (!Page.IsPostBack)
                {
                    _txtPrimarySvcType.Data = Page.PrimaryServiceType.ToString();
                    _txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
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
                if (Page.PrimaryServiceType == "LotBonus")
                {
                    LotBonusService Svc = new LotBonusService(profile);
                    LotBonus SvcData = new LotBonus();
                    LotBonus_Info SvcInfo = new LotBonus_Info();
                    LotBonus_Request ReqData = new LotBonus_Request();
                    LotBonus_Result ResData = new LotBonus_Result();

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
                    BonusLotDetails[] getDetails = _gridDetails.Data as BonusLotDetails[];
                    if (_chkIsWaferProcessing.IsChecked)
                    {
                        if (getDetails.Length > 0)
                        {
                            SvcData.Details = new BonusLotDetails[getDetails.Length];
                            foreach (BonusLotDetails detail in getDetails)
                            {
                                SvcData.Details[iIndex] = new BonusLotDetails();
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
                                if (detail.BonusReason != null)
                                    SvcData.Details[iIndex].BonusReason = detail.BonusReason;
                                if (detail.StandbyQty != null)
                                    SvcData.Details[iIndex].StandbyQty = detail.StandbyQty;
                                if (detail.BonusComment != null)
                                    SvcData.Details[iIndex].BonusComment = detail.BonusComment;
                                if (detail.WaferMapDetails != null)
                                    SvcData.Details[iIndex].WaferMapDetails = detail.WaferMapDetails;
                                iIndex = iIndex + 1;
                            }
                        }
                    }
                    else
                    {
                        SvcData.Details = _gridDetails.Data as BonusLotDetails[];
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
                } //Page.PrimaryServiceType == "LotForm"
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



