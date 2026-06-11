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
    public class SS_ConsumeMaterials : scsShopfloorBase
    {
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("ConsumeMaterials_SelectionId") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("ConsumeMaterials_Employee") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoProcessType { get { return Page.FindCamstarControl("ConsumeMaterials_ProcessType") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEquipment { get { return Page.FindCamstarControl("ConsumeMaterials_Equipment") as CWC.NamedObject; } }
        protected JQDataGrid _gridLotInfo { get { return Page.FindCamstarControl("ConsumeMaterials_LotInfo") as JQDataGrid; } }
        protected JQDataGrid _gridServiceDetails { get { return Page.FindCamstarControl("ConsumeMaterials_ServiceDetails") as JQDataGrid; } }
        protected CWC.CheckBox _chkIsWaferProcessing { get { return Page.FindCamstarControl("ConsumeMaterials_IsWaferProcessing") as CWC.CheckBox; } }
        protected ContainerListGrid _listContainer { get { return Page.FindCamstarControl("ConsumeMaterials_Container") as ContainerListGrid; } }
        protected CWC.TextBox _txtPrimarySvcType { get { return Page.FindCamstarControl("PrimarySvcType") as CWC.TextBox; } }
        protected DataEnvelopControl _envContainers { get { return Page.FindCamstarControl("ContainerLists") as DataEnvelopControl; } }
        protected DataEnvelopControl _envConsumeMaterialsDetails { get { return Page.FindCamstarControl("ConsumeMaterialsDetails") as DataEnvelopControl; } }
        protected DataEnvelopControl _envConsumeMaterialsWafers { get { return Page.FindCamstarControl("ConsumeMaterialsWafers") as DataEnvelopControl; } }
        protected CWC.TextBox _txtComments { get { return Page.FindCamstarControl("Shopfloor_Comments") as CWC.TextBox; } }
        protected CWC.TextBox _txtSelectedRowId { get { return Page.FindCamstarControl("SelectedRowId") as CWC.TextBox; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("ConsumeMaterials_ComputerName") as CWC.TextBox; } }
        protected CWC.Button _btnSelectPopup { get { return Page.FindCamstarControl("LotSelectPopup") as CWC.Button; } }
        
        //-----------------------------------------
        // Selection Id Data Changed Event
        //-----------------------------------------
        public void SelectionIdField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (_txtSelectionId.Data != null)
                {
                    ClearControls(10);
                    FetchData("SelectionIdChange");
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Process Type Field Data Changed Event
        //-----------------------------------------
        public void ProcessTypeField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                ClearControls(20);
                FetchData("ProcessTypeChange");
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
                ClearControls(30);
                FetchData("EquipmentChange");
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //---------------------------------------------------
        // Fetch Data function
        //---------------------------------------------------
        public void ClearControls(int ClearFlag)
        {
            try
            {
                Page.StatusBar.ClearMessage();
                if (ClearFlag <= 10)
                {
                    _chkIsWaferProcessing.Data = false;
                    _listContainer.ClearData();
                    _gridLotInfo.ClearData();
                    _ndoProcessType.ClearData();
                }
                if (ClearFlag <= 20)
                {
                    _ndoEquipment.ClearData();
                }
                if (ClearFlag <= 30)
                {
                    _gridServiceDetails.ClearData();
                }
                if (ClearFlag == -1)
                {
                    _txtComments.ClearData();
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
        // Copy button function
        //-----------------------------------------
        public void CopyButton()
        {
            try
            {
                if (_gridServiceDetails.SelectedRowID != null)
                {
                    int newIndex = Convert.ToInt32(_gridServiceDetails.SelectedRowID.ToString()) + 1;
                    var newDetails = new List<ConsumeMaterialsDetails>();
                    if (_gridServiceDetails.TotalRowCount > 0)
                    {
                        newDetails.AddRange(_gridServiceDetails.Data as ConsumeMaterialsDetails[]);
                    }
                    newDetails.Insert(newIndex, new ConsumeMaterialsDetails
                    {
                        MaterialPart = newDetails[newIndex - 1].MaterialPart,
                        ConsumeType = newDetails[newIndex - 1].ConsumeType,
                        ConsumeFactor = newDetails[newIndex - 1].ConsumeFactor,
                        QtyConsumed = newDetails[newIndex - 1].QtyConsumed,
                        QtyRequired = newDetails[newIndex - 1].QtyRequired,
                        MaterialLotName = "",
                        QtyToConsume = 0,
                        Wafers = newDetails[newIndex - 1].Wafers
                    });
                    _gridServiceDetails.ClearData();
                    _gridServiceDetails.Data = newDetails.ToArray();
                    _gridServiceDetails.OriginalData = newDetails.ToArray();
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
                ConsumeMaterialsService Svc = new ConsumeMaterialsService(profile);
                ConsumeMaterials SvcData = new ConsumeMaterials();
                ConsumeMaterials_Info SvcInfo = new ConsumeMaterials_Info();
                ConsumeMaterials_Request ReqData = new ConsumeMaterials_Request();
                ConsumeMaterials_Result ResData = new ConsumeMaterials_Result();
                string sServiceEventName = "";

                if (_txtSelectionId.Data != null)
                {
                    //Prepare the service data
                    if (_listContainer.Data != null)
                    {
                        SvcData.Container = new ContainerRef();
                        SvcData.Container.Name = _listContainer.Data.ToString();
                    }
                    else
                    {
                        SvcData.SelectionId = _txtSelectionId.Data.ToString();
                        sServiceEventName = "ResolveSelectionId";
                    }
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
                    
                    //Prepare the service info
                    if (EventName == "SelectionIdChange")
                    {
                        if (sServiceEventName == "ResolveSelectionId")
                            SvcInfo.Containers = new Info(true);
                        SvcInfo.ProcessTypeSelection = new Info(true);
                        SvcInfo.IsWaferProcessing = new Info(true);
                    }
                    if (EventName == "SelectionIdChange" || EventName == "ProcessTypeChange")
                    {
                        SvcInfo.EquipmentSelection = new Info(true);
                    }
                    if (SvcData.ProcessType != null || SvcData.Equipment != null)
                    {
                        SvcInfo.ServiceDetailsSelection = new ConsumeMaterialsDetails_Info();
                        SvcInfo.ServiceDetailsSelection.MaterialPart = new Info(true);
                        SvcInfo.ServiceDetailsSelection.ReferenceDesignator = new Info(true);
                        SvcInfo.ServiceDetailsSelection.ConsumeType = new Info(true);
                        SvcInfo.ServiceDetailsSelection.ConsumeFactor = new Info(true);
                        SvcInfo.ServiceDetailsSelection.QtyConsumed = new Info(true);
                        SvcInfo.ServiceDetailsSelection.QtyRequired = new Info(true);
                        SvcInfo.ServiceDetailsSelection.MaterialLotName = new Info(true);
                        SvcInfo.ServiceDetailsSelection.QtyToConsume = new Info(true);
                        SvcInfo.ServiceDetailsSelection.Wafers = new ConsumeMaterialsDetailsWafers_Info();
                        SvcInfo.ServiceDetailsSelection.Wafers.LotWafersItem = new Info(true);
                        SvcInfo.ServiceDetailsSelection.Wafers.WaferScribeNumber = new Info(true);
                        SvcInfo.ServiceDetailsSelection.Wafers.QtyConsumed = new Info(true);
                        SvcInfo.ServiceDetailsSelection.Wafers.QtyRequired = new Info(true);
                        SvcInfo.ServiceDetailsSelection.Wafers.FromLotWafersItem = new Info(true);
                        SvcInfo.ServiceDetailsSelection.Wafers.FromWaferScribeNumber = new Info(true);
                        SvcInfo.ServiceDetailsSelection.Wafers.QtyToConsume = new Info(true);
                    }
                    ReqData.Info = SvcInfo;

                    //Execute Request
                    ResultStatus Results;
                    if (sServiceEventName == "ResolveSelectionId")
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
                        if (sServiceEventName == "ResolveSelectionId")
                        {
                            if (ResData.Value.Containers != null)
                                _listContainer.Data = ResData.Value.Containers[0].Name;
                        }
                        if (ResData.Value.ProcessTypeSelection != null)
                        {
                            _ndoProcessType.ClearData();
                            _ndoProcessType.ClearSelectionValues();
                            CWC.NamedObject _ndoProcessTypeTemp = _ndoProcessType;
                            SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoProcessTypeTemp, ResData.Value.ProcessTypeSelection);
                            _ndoProcessType.Data = ResData.Value.ProcessTypeSelection[0];
                        }
                        if (ResData.Value.EquipmentSelection != null)
                        {
                            _ndoEquipment.ClearData();
                            _ndoEquipment.ClearSelectionValues();
                            CWC.NamedObject _ndoEquipmentTemp = _ndoEquipment;
                            SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoEquipmentTemp, ResData.Value.EquipmentSelection);
                            //_ndoEquipment.Data = ResData.Value.EquipmentSelection[0];
                        }
                        if (ResData.Value.IsWaferProcessing != null)
                        {
                            _chkIsWaferProcessing.Data = ResData.Value.IsWaferProcessing.ToString();
                        }
                        if (ResData.Value.ServiceDetailsSelection != null)
                        {
                            _gridServiceDetails.ClearData();
                            //Add ServiceDetailsSelection to the grid
                            _gridServiceDetails.Data = ResData.Value.ServiceDetailsSelection;
                            _gridServiceDetails.OriginalData = ResData.Value.ServiceDetailsSelection;

                            //Add ServiceDetailsSelection to the envelope control
                            _envConsumeMaterialsDetails.SS_ConsumeMaterialsDetails = ResData.Value.ServiceDetailsSelection;
                        }

                        //Hide details columns
                        if (EventName == "SelectionIdChange")
                        {
                            for (int i = 0; i < _gridServiceDetails.Settings.Columns.Count(); i++)
                            {
                                if ((_gridServiceDetails.GridContext as BoundContext).Fields[i].ID == "QtyToConsume")
                                    (_gridServiceDetails.GridContext as BoundContext).Fields[i].Visible = !_chkIsWaferProcessing.IsChecked;
                            }

                            //Fetch and display lot info
                            JQDataGrid theGrid = _gridLotInfo;
							RecordSet rs = SEMI.AppCode.UIUtility.GetLotQuerySelection(this, this.PrimaryServiceType, _listContainer.Data.ToString());
                            DataTable containersDataTable = rs.GetAsExplicitlyDataTable();
                            theGrid.ClearData();
                            SEMI.AppCode.GridUtility.ItemListGrid_SetColumns(this, containersDataTable, theGrid.ID, null, "RefTargetGrid",false,null,true,null,rs.Headers);
                            SEMI.AppCode.GridUtility.ItemListGrid_BindDataTable(this, containersDataTable, ref theGrid);

                            //Fetch equipment if required
                            if (EventName == "SelectionIdChange" && _ndoProcessType.Data != null && _ndoEquipment.DropDownControl.Items.Count <= 1)
                            {
                                ProcessTypeField_DataChanged(null, null);
                            }
                        }
                    } //Results.IsSuccess
                    else
                    {
                        this.DisplayMessage(Results);
                    }
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Consume materials details popup
        //-----------------------------------------
        public virtual void PopupConsumeMaterialsDetails(bool EndResponse = false)
        {
            try
            {
                if (Page.DataContract.GetValueByName("ConsumeMaterials_GridRowId_DM") != null)
                {
                    _envConsumeMaterialsDetails.SS_ConsumeMaterialsDetails = _gridServiceDetails.Data as ConsumeMaterialsDetails[];
                    _txtSelectedRowId.Data = Page.DataContract.GetValueByName("ConsumeMaterials_GridRowId_DM").ToString();
                    if (_envConsumeMaterialsDetails.SS_ConsumeMaterialsDetails[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].Wafers != null)
                    {
                        _envConsumeMaterialsWafers.SS_ConsumeMaterialsDetailsWafers = _envConsumeMaterialsDetails.SS_ConsumeMaterialsDetails[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].Wafers;
                        Page.DataContract.SetValueByName("ConsumeMaterials_ConsumeMaterialsWafers_DM", _envConsumeMaterialsDetails.SS_ConsumeMaterialsDetails[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].Wafers);
                    }
                    else
                    {
                        _envConsumeMaterialsWafers.SS_ConsumeMaterialsDetailsWafers = null;
                        Page.DataContract.SetValueByName("ConsumeMaterials_ConsumeMaterialsWafers_DM", null);
                    }
                    
                    Camstar.WebPortal.Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                    objAction.PageName = "SS_ConsumeMaterialsWafersPopupVP";

                    UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[1];
                    objLinks[0] = new UIComponentDataContractLink();
                    objLinks[0].SourceMember = "ConsumeMaterials_ConsumeMaterialsWafers_DM";
                    objLinks[0].TargetMember = "CMWafersPopup_ConsumeMaterialsWafers_DM";

                    UIComponentDataContractReturnLink[] objReturnLinks = new UIComponentDataContractReturnLink[1];
                    objReturnLinks[0] = new UIComponentDataContractReturnLink();
                    objReturnLinks[0].SourceMember = "CMWafersPopup_ConsumeMaterialsWafers_DM";
                    objReturnLinks[0].TargetMember = "ConsumeMaterials_ConsumeMaterialsWafers_DM";
                    objAction.DataContractMap = new UIComponentDataContractMap();
                    objAction.DataContractMap.Links = objLinks;
                    objAction.DataContractReturnMap = new UIComponentDataContractReturnMap();
                    objAction.DataContractReturnMap.ReturnLinks = objReturnLinks;
                    objAction.FrameLocation = new UIFloatingPageLocation();
                    objAction.FrameLocation.Width = 950;
                    objAction.FrameLocation.Height = 700;
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
                    case "Reset":
                        {
                            Page.ShopfloorReset(sender, e);
                            _gridLotInfo.ClearData();
                            _gridServiceDetails.ClearData();
                            _ndoProcessType.ClearSelectionValues();
                            _ndoEquipment.ClearSelectionValues();
                            break;
                        }
                    case "ConsumeMaterialsDetails":
                        {
                            PopupConsumeMaterialsDetails();
                            break;
                        }
                    case "Copy":
                        {
                            CopyButton();
                            break;
                        }
                }
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
                if (Page.DataContract.GetValueByName("ConsumeMaterials_ContainerList_DM") != null)
                {
                    string[] sContainers = Page.DataContract.GetValueByName("ConsumeMaterials_ContainerList_DM") as string[];
                    _envContainers.SS_ContainersList = null;
                    Page.DataContract.SetValueByName("ConsumeMaterials_ContainerList_DM", null);
                    _txtSelectionId.Data = sContainers[0];
                    SelectionIdField_DataChanged(null, null);
                }
                if (Page.DataContract.GetValueByName("ConsumeMaterials_ConsumeMaterialsWafers_DM") != null)
                {
                    //Bind the returned data to the data envelope control & grid
                    _envConsumeMaterialsDetails.SS_ConsumeMaterialsDetails[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].Wafers = Page.DataContract.GetValueByName("ConsumeMaterials_ConsumeMaterialsWafers_DM") as ConsumeMaterialsDetailsWafers[];
                    _gridServiceDetails.ClearData();
                    _gridServiceDetails.Data = _envConsumeMaterialsDetails.SS_ConsumeMaterialsDetails;
                    _gridServiceDetails.OriginalData = _envConsumeMaterialsDetails.SS_ConsumeMaterialsDetails;
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //------------------------
        // Override GetInputData function
        //------------------------
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            ConsumeMaterialsDetails[] getServiceDetails = _gridServiceDetails.Data as ConsumeMaterialsDetails[];
			if (getServiceDetails != null && getServiceDetails.Count() > 0)
            {
                if (_gridServiceDetails.SelectedRowIDs != null)
                {
                    (serviceData as ConsumeMaterials).ServiceDetails = new ConsumeMaterialsDetails[_gridServiceDetails.SelectedRowIDs.Length];
                    int svcIndex = 0;
                    foreach (string selectedRow in _gridServiceDetails.SelectedRowIDs)
                    {
                        int selectedRowIndex = Convert.ToInt32(selectedRow);
                        (serviceData as ConsumeMaterials).ServiceDetails[svcIndex] = new ConsumeMaterialsDetails();
                        (serviceData as ConsumeMaterials).ServiceDetails[svcIndex].MaterialPart = getServiceDetails[selectedRowIndex].MaterialPart;
                        (serviceData as ConsumeMaterials).ServiceDetails[svcIndex].MaterialLotName = getServiceDetails[selectedRowIndex].MaterialLotName;
                        (serviceData as ConsumeMaterials).ServiceDetails[svcIndex].QtyConsumed = getServiceDetails[selectedRowIndex].QtyConsumed;
                        (serviceData as ConsumeMaterials).ServiceDetails[svcIndex].QtyRequired = getServiceDetails[selectedRowIndex].QtyRequired;
                        (serviceData as ConsumeMaterials).ServiceDetails[svcIndex].QtyToConsume = getServiceDetails[selectedRowIndex].QtyToConsume;
                        if (getServiceDetails[selectedRowIndex].Wafers != null)
                        {
                            (serviceData as ConsumeMaterials).ServiceDetails[svcIndex].Wafers = new ConsumeMaterialsDetailsWafers[getServiceDetails[selectedRowIndex].Wafers.Length];
                            int waferIndex = 0;
                            foreach (ConsumeMaterialsDetailsWafers wafer in getServiceDetails[selectedRowIndex].Wafers)
                            {
                                (serviceData as ConsumeMaterials).ServiceDetails[svcIndex].Wafers[waferIndex] = new ConsumeMaterialsDetailsWafers();
                                (serviceData as ConsumeMaterials).ServiceDetails[svcIndex].Wafers[waferIndex].LotWafersItem = wafer.LotWafersItem;
                                (serviceData as ConsumeMaterials).ServiceDetails[svcIndex].Wafers[waferIndex].FromLotWafersItem = wafer.FromLotWafersItem;
                                (serviceData as ConsumeMaterials).ServiceDetails[svcIndex].Wafers[waferIndex].FromWaferScribeNumber = wafer.FromWaferScribeNumber;
                                (serviceData as ConsumeMaterials).ServiceDetails[svcIndex].Wafers[waferIndex].QtyConsumed = wafer.QtyConsumed;
                                (serviceData as ConsumeMaterials).ServiceDetails[svcIndex].Wafers[waferIndex].QtyRequired = wafer.QtyRequired;
                                (serviceData as ConsumeMaterials).ServiceDetails[svcIndex].Wafers[waferIndex].QtyToConsume = wafer.QtyToConsume;
                                (serviceData as ConsumeMaterials).ServiceDetails[svcIndex].Wafers[waferIndex].WaferScribeNumber = wafer.WaferScribeNumber;
                                (serviceData as ConsumeMaterials).ServiceDetails[svcIndex].Wafers[waferIndex].WaferMapDetails = wafer.WaferMapDetails;
                                waferIndex++;
                            }
                        }
                        svcIndex++;
                    }
                } //_gridServiceDetails.SelectedRowIDs != null
            } //getServiceDetails.Count() > 0
        }

        //---------------------------------------------------
        // Override PostExecute event
        //---------------------------------------------------
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                Page.ShopfloorReset(null, null);
                _gridLotInfo.ClearData();
                _gridServiceDetails.ClearData();
                _ndoProcessType.ClearSelectionValues();
                _ndoEquipment.ClearSelectionValues();
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
                if (!Page.IsPostBack)
                {
                    _txtPrimarySvcType.Data = Page.PrimaryServiceType.ToString();
                    _txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
                }
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {
                    OnPopupClose();
                }
                Personalization.UIAction[] actUIActions = Page.ActionDispatcher.PageActions();
                foreach (Personalization.UIAction act in actUIActions)
                {
                    if (act.Name.ToUpper() == "CLOSEACTION")
                    {
                        if (!Page.IsAJAXFloatingFrame)
                        {
                            act.IsHidden = true;
                            act.IsDisabled = true;
                        }
                    }

                    if (act.Name.ToUpper() == "RESETACTION")
                    {
                        if (Page.IsAJAXFloatingFrame)
                        {
                            act.IsHidden = true;
                            act.IsDisabled = true;
                            _btnSelectPopup.Visible = false;
                            _txtSelectionId.ReadOnly = true;
                            _ndoProcessType.Enabled = false;
                            _ndoEquipment.Enabled = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }
    }
}



