/* Copyright 2025 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using PERS = Camstar.WebPortal.Personalization;

/// <summary>
/// Summary description for SS_WIPData
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_WIPData : MatrixWebPart
    {
        #region Properties

        protected CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("WIPData_ComputerName") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoEmployeeField { get { return Page.FindCamstarControl("WIPData_Employee") as CWC.NamedObject; } }
        protected CWC.ContainerList _ContainerField { get { return Page.FindCamstarControl("WIPData_Container") as CWC.ContainerList; } }
        protected CWC.NamedObject _ndoProcessTypeField { get { return Page.FindCamstarControl("WIPData_ProcessType") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEquipmentField { get { return Page.FindCamstarControl("WIPData_Equipment") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoServiceNameField { get { return Page.FindCamstarControl("WIPData_ServiceName") as CWC.NamedObject; } }
        protected CWC.CheckBox _ShowHiddenField { get { return Page.FindCamstarControl("WIPData_ShowHiddenCheckBox") as CWC.CheckBox; } }
        protected CWC.CheckBox _SkipSPCFailureEmailField { get { return Page.FindCamstarControl("WIPData_SkipSPCFailureEmail") as CWC.CheckBox; } }
        protected CWC.NamedObject _ndoDisplayFilterField { get { return Page.FindCamstarControl("WIPData_DisplayFilter") as CWC.NamedObject; } }
        protected CWC.DropDownList _LotItemGridDisplayField { get { return Page.FindCamstarControl("WIPData_LotItemGridDisplay") as CWC.DropDownList; } }
        protected JQDataGrid _gridByLotDetails { get { return Page.FindCamstarControl("WIPData_ByLotDetails") as JQDataGrid; } }
        protected JQDataGrid _gridByWaferDetails { get { return Page.FindCamstarControl("WIPData_ByWaferDetails") as JQDataGrid; } }
        protected JQDataGrid _gridDetailsInfo { get { return Page.FindCamstarControl("WIPData_DetailsInfo") as JQDataGrid; } }
        // protected CWC.TextBox _txtCommentsField { get { return Page.FindCamstarControl("WIPData_Comments") as CWC.TextBox; } }
        protected CWC.Label _lblWaferScribeNumber { get { return Page.FindCamstarControl("WIPData_WaferScribeNumber_Label") as CWC.Label; } }
        protected CWC.Label _lblUOM { get { return Page.FindCamstarControl("WIPData_UOM_Label") as CWC.Label; } }
        protected CWC.CheckBox _HideDataPointDetails { get { return Page.FindCamstarControl("WIPData_HideDataPointDetails") as CWC.CheckBox; } }

        // framework control flag
        CWC.CheckBox _chkIsActive { get { return Page.FindCamstarControl("WIPData_IsActive") as CWC.CheckBox; } }
        CWC.CheckBox _chkIsPopup { get { return Page.FindCamstarControl("WIPData_IsPopup") as CWC.CheckBox; } }

        protected CWC.TextBox _txtWIPData_Init { get { return Page.FindCamstarControl("WIPData_Init") as CWC.TextBox; } }
        protected CWC.Button _btnReset { get { return Page.FindCamstarControl("WIPData_ResetButton") as CWC.Button; } }
        protected CWC.Button _btnSubmit { get { return Page.FindCamstarControl("WIPData_SubmitButton") as CWC.Button; } }
        protected bool _bIsPopup { get { return Page.IsAJAXFloatingFrame; } }
        //protected const string _SPCTxnDataSessionIdentifier = "__EquipmentWIPMain_SPCTxnData";
        protected const string _SPCTxnDataSessionIdentifier = "_SPCTxnDataList";
        protected const string _ShowSPCChartSessionIdentifier = "_ShowSPCChart";
        protected const string _WIPMainContainerList = "_ContainerList";
        #endregion

        #region Page Events

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.OnPreExecute += new EventHandler<FormProcessingEventArgs>(Page_OnPreExecute);
            Page.OnPostExecute += new EventHandler<ResultEventArgs>(Page_OnPostExecute);
        }

        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            if (!_bIsPopup || _chkIsActive.CheckControl.Checked)
            {
                _txtWIPData_Init.DataChanged += _txtWIPData_Init_DataChanged;
                _ndoEquipmentField.DataChanged += WIPData_Equipment_DataChanged;
                _ndoProcessTypeField.DataChanged += WIPData_ProcessType_DataChanged;
                _ndoServiceNameField.DataChanged += WIPData_ServiceName_DataChanged;
                _ContainerField.DataChanged += WIPData_Container_DataChanged;
            }
        }

        public override void WebPartCustomAction(object sender, PERS.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "ResetPopup")
            {
                ResetControls(0);
                FetchData();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                if (!Page.IsPostBack)
                {
                    _txtComputerNameField.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
                    var theme = Page.Session["CurrentTheme"] != null ? Page.Session["CurrentTheme"].ToString() : string.Empty;

                    if (theme.ToLower() == "horizon")
                    {
                        foreach (PERS.JQFieldBase Col in _gridByLotDetails.Settings.Columns)
                            if (Col.Name.Equals("WIPDataValidValuesButton"))
                            {
                                var cellActionFrameLocation = (Col.CellActions[0].DefaultAction as PERS.FloatPageOpenAction).FrameLocation;
                                cellActionFrameLocation.Height = 0;
                                cellActionFrameLocation.Width = 0;
                                break;
                            }
                    }

                    //hide the label used for the tooltip in the grid
                    CWC.Label HiddenToolTipLabel = Page.FindCamstarControl("HiddenToolTipLabel") as CWC.Label;
                    if (HiddenToolTipLabel != null)
                        HiddenToolTipLabel.Style["display"] = "none";

                    SEMI.AppCode.UIUtility.MaximizePopUp(this);
                }

                if (_chkIsPopup.CheckControl.Checked)
                    _ndoServiceNameField.Enabled = false;

                bool bIsValidValuesPopupCall = false;
                // Assign the WIP Data Name to the data contract for pop up
                if (Page.EventTarget == "ctl00$WebPartManager$SS_WIPDataWP$WIPData_ByWaferDetails" && Page.EventArgument.Contains("OnCellActionClick"))
                {
                    var parts = Page.EventArgument.Split(':');
                    string strWIPDataName = parts[1].Substring(4);

                    Page.PortalContext.DataContract.SetValueByName("WIPData_WIPDataNameDM", new NamedObjectRef(strWIPDataName));
                    Page.CollectDataContract();
                    bIsValidValuesPopupCall = true;
                }

                // Refresh Details datagrid value from Popup selection page
                if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument")
                {
                    var KeyDM = Page.PortalContext.DataContract.GetValueByName<string>("WIPData_KeyDM");
                    var GridRowIdDM = Page.PortalContext.DataContract.GetValueByName<string>("WIPData_GridRowIdDM");
                    var WIPDataNameDM = Page.PortalContext.DataContract.GetValueByName<NamedObjectRef>("WIPData_WIPDataNameDM") != null ? Page.PortalContext.DataContract.GetValueByName<NamedObjectRef>("WIPData_WIPDataNameDM").Name : null;
                    var WIPDataValueDM = Page.PortalContext.DataContract.GetValueByName("WIPData_WIPDataValueDM");

                    if (!string.IsNullOrEmpty(GridRowIdDM))
                    {
                        if (WIPDataValueDM != null)
                        {
                            if (KeyDM == "ByLot")
                            {
                                (_gridByLotDetails.GridContext as ItemDataContext).SetCell(GridRowIdDM, "WIPDataValue", WIPDataValueDM);
                            }
                            else if (KeyDM == "ByWafer")
                            {
                                //(_gridByWaferDetails.GridContext as ItemDataContext).SetCell(GridRowIdDM, "WIPDataValue", WIPDataValueDM);
                                UpdateWaferGridCell(GridRowIdDM, WIPDataNameDM, WIPDataValueDM.ToString());
                            }
                        }
                    }
                }

                if (!bIsValidValuesPopupCall)
                {
                    // clear the data contracts as the mess with the data loading of grid values
                    Page.PortalContext.DataContract.SetValueByName("WIPData_KeyDM", null);
                    Page.PortalContext.DataContract.SetValueByName("WIPData_GridRowIdDM", null);
                    Page.PortalContext.DataContract.SetValueByName("WIPData_WIPDataNameDM", null);
                    Page.PortalContext.DataContract.SetValueByName("WIPData_WIPDataValueDM", null);
                }

                var activitySectionDM = Page.PortalContext.DataContract.GetValueByName<string>("WIPData_ActivitySectionDM");
                if (_bIsPopup && activitySectionDM != null && activitySectionDM.ToLower() == "true")
                {
                    _HideDataPointDetails.IsChecked = true;
                }
                if (!Page.IsPostBack)
                {
                    if (_bIsPopup)
                    {
                        if (activitySectionDM != null && activitySectionDM.ToLower() == "true")
                        {
                            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "Close").First().IsHidden = true;

                            _ndoServiceNameField.Hidden = true;
                            _SkipSPCFailureEmailField.Hidden = true;
                            _ShowHiddenField.Hidden = true;
                            _HideDataPointDetails.Hidden = true;
                            _HideDataPointDetails.CheckControl.Checked = true;
                            _ndoDisplayFilterField.Hidden = true;
                            _LotItemGridDisplayField.Hidden = true;
                        }

                        // session value is used to pass the SPC Txn Data back to the EquipmentWIPMain page
                        Page.Session[_SPCTxnDataSessionIdentifier] = null;
                        Page.Session[_ShowSPCChartSessionIdentifier] = null;
                        _btnReset.Enabled = false;
                        _btnReset.Visible = false;
                        _btnReset.Hidden = true;

                        _btnSubmit.Enabled = false;
                        _btnSubmit.Visible = false;

                        FetchData();
                    }
                    else
                    {
                        if (activitySectionDM != null && activitySectionDM.ToLower() == "true")
                        {
                            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "Close").First().IsHidden = true;
                            _btnReset.Enabled = false;
                            _btnReset.Visible = false;
                            _btnReset.Hidden = true;

                            _btnSubmit.Enabled = false;
                            _btnSubmit.Visible = false;
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
        }

        void _txtWIPData_Init_DataChanged(object sender, EventArgs e)
        {
            FetchData();
        }

        public void WIPData_ShowHiddenCheckBox_DataChanged()
        {
            SetControls();
        }

        public void WIPData_HideDataPointDetails_DataChanged()
        {
            FetchData();
        }

        public void WIPData_DisplayFilter_DataChanged()
        {
            SetControls();
        }

        public void WIPData_Container_DataChanged(object sender, EventArgs e)
        {
            ResetControls(5);
            if (_ContainerField.Data != null)
                FetchData();
        }

        public void WIPData_ProcessType_DataChanged(object sender, EventArgs e)
        {
            ResetControls(10);
            FetchData();
        }

        public void WIPData_Equipment_DataChanged(object sender, EventArgs e)
        {
            _ndoEquipmentField.TextEditControl.ToolTip = _ndoEquipmentField.TextEditControl.Text;
            ResetControls(20);
            FetchData();
        }

        public void WIPData_ServiceName_DataChanged(object sender, EventArgs e)
        {
            ResetControls(30);
            if (_ndoServiceNameField.Data != null)
                FetchData();
        }

        public void WIPData_ResetButton_Click()
        {
            ResetControls(0);
            FetchData();
        }

        public void WIPData_LotItemGridDisplay_DataChanged()
        {
            SetGridDisplay();
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            if (serviceData is OM.WIPData)
            {
                if (_gridByLotDetails.Data != null || _gridByWaferDetails.Data != null)
                {
                    int intTotalWIPDataByLot = 0;
                    int intTotalWafers = 0;
                    int intWIPDataForWafer = 0;
                    int iIndex = -1;

                    List<WIPDataDetails> ByLotDetails = (List<WIPDataDetails>)ViewState["ByLotDetails"];
                    intTotalWIPDataByLot = (ByLotDetails == null ? 0 : ByLotDetails.Count());
                    intTotalWafers = _gridByWaferDetails.GridContext.GetTotalRows();
                    intWIPDataForWafer = _gridDetailsInfo.GridContext.GetTotalRows();

                    OM.WIPData svcData = serviceData as OM.WIPData;
                    svcData.Details = new WIPDataDetails[intTotalWIPDataByLot + (intTotalWafers * intWIPDataForWafer)];

                    // collect the wip data for lot
                    for (int x = 0; x < intTotalWIPDataByLot; x++)
                    {
                        iIndex = iIndex + 1;
                        bool bWIPDataValue = false;
                        bWIPDataValue = ByLotDetails[x].WIPDataValue != null ? (ByLotDetails[x].WIPDataValue.ToString() != "" ? true : false) : false;

                        svcData.Details[iIndex] = new WIPDataDetails();
                        svcData.Details[iIndex].ListItemAction = OM.ListItemAction.Add;
                        svcData.Details[iIndex].WIPDataName = new NamedObjectRef();
                        svcData.Details[iIndex].WIPDataName.Name = ByLotDetails[x].WIPDataName.Name;
                        svcData.Details[iIndex].WIPDataValue = ByLotDetails[x].WIPDataValue;
                        if (!bWIPDataValue && bool.Parse(ByLotDetails[x].IsCalculatedData.ToString()))
                            svcData.Details[iIndex].WIPDataValue = "";

                        svcData.Details[iIndex].ForProcessType = ByLotDetails[x].ForProcessType;
                        svcData.Details[iIndex].IsCalculatedData = bool.Parse(ByLotDetails[x].IsCalculatedData.ToString());
                        svcData.Details[iIndex].IsRequired = bool.Parse(ByLotDetails[x].IsRequired.ToString());

                        if (ByLotDetails[x].ss_UOM != null)
                        {
                            svcData.Details[iIndex].ss_UOM = new NamedObjectRef();
                            svcData.Details[iIndex].ss_UOM.Name = ByLotDetails[x].ss_UOM.Name;
                        }

                        if (Page.VirtualPageName == "SS_WIPDataPopupVP")
                        {
                            if (ByLotDetails[x].scsWIPDataGroupingType != null)
                            {
                                svcData.Details[iIndex].scsWIPDataGroupingType = ByLotDetails[x].scsWIPDataGroupingType;

                                if (Page.Session[_WIPMainContainerList] != null)
                                    svcData.scsWIPMainContainerList = Page.Session[_WIPMainContainerList] as ContainerRef[];
                            }
                        }
                    }

                    // collect the wip data for wafers
                    WIPDataDetails[] DetailsInfo = (WIPDataDetails[])((_gridDetailsInfo.GridContext as ItemDataContext).Data);
                    for (int x = 0; x < intTotalWafers; x++)
                    {
                        foreach (WIPDataDetails DetailsInfoRow in DetailsInfo)
                        {
                            iIndex = iIndex + 1;
                            bool bWIPDataValue = false;
                            bWIPDataValue = _gridByWaferDetails.GridContext.GetCell(x, DetailsInfoRow.WIPDataName.Name) != null ? (_gridByWaferDetails.GridContext.GetCell(x, DetailsInfoRow.WIPDataName.Name).ToString() != "" ? true : false) : false;

                            svcData.Details[iIndex] = new WIPDataDetails();
                            svcData.Details[iIndex].ListItemAction = OM.ListItemAction.Add;
                            svcData.Details[iIndex].WaferScribeNumber = _gridByWaferDetails.GridContext.GetCell(x, "WaferScribeNumber").ToString();
                            svcData.Details[iIndex].WIPDataName = new NamedObjectRef();
                            svcData.Details[iIndex].WIPDataName.Name = DetailsInfoRow.WIPDataName.Name;
                            svcData.Details[iIndex].WIPDataValue = _gridByWaferDetails.GridContext.GetCell(x, DetailsInfoRow.WIPDataName.Name).ToString();
                            if (!bWIPDataValue && bool.Parse(DetailsInfoRow.IsCalculatedData.ToString()))
                                svcData.Details[iIndex].WIPDataValue = "";

                            svcData.Details[iIndex].ForProcessType = DetailsInfoRow.ForProcessType;
                            svcData.Details[iIndex].IsCalculatedData = bool.Parse(DetailsInfoRow.IsCalculatedData.ToString());
                            svcData.Details[iIndex].IsRequired = bool.Parse(DetailsInfoRow.IsRequired.ToString());
                            if (DetailsInfoRow.ss_UOM != null)
                            {
                                svcData.Details[iIndex].ss_UOM = new NamedObjectRef();
                                svcData.Details[iIndex].ss_UOM.Name = DetailsInfoRow.ss_UOM.Name;
                            }
                        }
                    }
                }
            }
        }

        void Page_OnPreExecute(object sender, FormProcessingEventArgs e)
        {
            if (e.Info is OM.WIPData_Info && e.Data is OM.WIPData)
            {
                OM.Info serviceInfo = e.Info;
                OM.Service serviceData = e.Data;

                (serviceInfo as OM.WIPData_Info).SPCTxnDataList = new SPCTxnData_Info
                {
                    Name = FieldInfoUtil.RequestValue(),
                    SPCSetup = FieldInfoUtil.RequestValue(),
                    SPCResult = FieldInfoUtil.RequestValue(),
                    SPCResultFilename = FieldInfoUtil.RequestValue(),
                    ChartHeight = FieldInfoUtil.RequestValue(),
                    ChartWidth = FieldInfoUtil.RequestValue()
                };
            }
        }

        void Page_OnPostExecute(object sender, ResultEventArgs e)
        {
            if (e.Status.IsSuccess)
                if (e.Value.GetType().Name == "WIPData")
                {
                    // if the page is running as a popup, check for the SPCTxnData and set to the session object                    
                    SPCTxnData[] oSPCTxnData = null;
                    Page.Session[_SPCTxnDataSessionIdentifier] = null;
                    Page.Session[_ShowSPCChartSessionIdentifier] = null;
                    if (e.Value is OM.WIPData)
                        if ((e.Value as WIPData).SPCTxnDataList != null)
                        {
                            oSPCTxnData = (e.Value as WIPData).SPCTxnDataList;
                            Page.Session[_SPCTxnDataSessionIdentifier] = oSPCTxnData;
                            Page.Session[_ShowSPCChartSessionIdentifier] = true;
                        }

                    SEMI.AppCode.UIUtility.SimpleWIPMainActivityPage_OnPostExecute(e, this);

                    if (!Page.IsAJAXFloatingFrame)
                    {
                        ResetControls(0);
                        FetchData();
                    }

                }
        }

        //public override void PostExecute(ResultStatus status, Service serviceData)
        //{
        //    //base.PostExecute(status, serviceData);

        //    if (status.IsSuccess)
        //        if (serviceData is OM.WIPData)
        //        {
        //            // if the page is running as a popup, check for the SPCTxnData and set to the session object                    
        //            SPCTxnData[] oSPCTxnData = null;
        //            Page.Session[_SPCTxnDataSessionIdentifier] = null;
        //            if (serviceData is OM.WIPData)
        //                if ((serviceData as WIPData).SPCTxnDataList != null)
        //                {
        //                    oSPCTxnData = (serviceData as WIPData).SPCTxnDataList;
        //                    Page.Session[_SPCTxnDataSessionIdentifier] = oSPCTxnData;
        //                }

        //            ResetControls(0);
        //            FetchData();
        //        }
        //}
        #endregion

        #region Methods

        public void FetchData()
        {
            try
            {
                bool bExecute = false;
                if (_chkIsActive != null)
                    bExecute = (_chkIsActive.CheckControl.Checked || _chkIsPopup.CheckControl.Checked);
                if (bExecute)
                    if (_ContainerField.Data != null)
                    {

                        // get the session and user profile
                        var fs = FrameworkManagerUtil.GetFrameworkSession();

                        // init the service, service data and service info objects
                        WIPDataService objSvc = new WIPDataService(fs.CurrentUserProfile);
                        WIPData objSvcData = new WIPData();
                        objSvcData.Container = new ContainerRef((string)_ContainerField.TextEditControl.Text);
                        objSvcData.ProcessType = new NamedObjectRef(_ndoProcessTypeField.TextEditControl.Text);
                        objSvcData.Equipment = new NamedObjectRef(_ndoEquipmentField.TextEditControl.Text);
                        objSvcData.ServiceName = _ndoServiceNameField.Data != null ? _ndoServiceNameField.Data.ToString() : "";

                        WIPData_Info objSvcInfo = new WIPData_Info();
                        objSvcInfo.Container = FieldInfoUtil.RequestValue();
                        if (Page.IsAJAXFloatingFrame)
                            objSvcInfo.ProcessTypeSelection = FieldInfoUtil.RequestValue();
                        objSvcInfo.EquipmentSelection = FieldInfoUtil.RequestValue();
                        objSvcInfo.ServiceNameSelection = FieldInfoUtil.RequestValue();
                        objSvcInfo.LotItemGridDisplay = FieldInfoUtil.RequestSelectionValue();
                        objSvcInfo.WIPDataSetup = FieldInfoUtil.RequestValue();
                        objSvcInfo.ss_WIPDataSetupByBlanketExp = FieldInfoUtil.RequestValue();
                        objSvcInfo.ss_WIPDataSetupMatrix = FieldInfoUtil.RequestValue();
                        objSvcInfo.DetailsSelection = new WIPDataDetails_Info();
                        objSvcInfo.DetailsSelection.WIPDataName = FieldInfoUtil.RequestValue();
                        objSvcInfo.DetailsSelection.ss_UOM = FieldInfoUtil.RequestValue();
                        objSvcInfo.DetailsSelection.WIPDataValue = FieldInfoUtil.RequestValue();
                        objSvcInfo.DetailsSelection.ForProcessType = FieldInfoUtil.RequestValue();
                        objSvcInfo.DetailsSelection.IsRequired = FieldInfoUtil.RequestValue();
                        objSvcInfo.DetailsSelection.IsHidden = FieldInfoUtil.RequestValue();
                        objSvcInfo.DetailsSelection.WaferScribeNumber = FieldInfoUtil.RequestValue();
                        objSvcInfo.DetailsSelection.DisplayFilter = FieldInfoUtil.RequestValue();
                        objSvcInfo.DetailsSelection.LowerLimitFailed = FieldInfoUtil.RequestValue();
                        objSvcInfo.DetailsSelection.UpperLimitFailed = FieldInfoUtil.RequestValue();
                        objSvcInfo.DetailsSelection.ChangeLimitFailed = FieldInfoUtil.RequestValue();
                        objSvcInfo.DetailsSelection.FieldType = FieldInfoUtil.RequestValue();
                        objSvcInfo.DetailsSelection.LowerLimit = FieldInfoUtil.RequestValue();
                        objSvcInfo.DetailsSelection.UpperLimit = FieldInfoUtil.RequestValue();
                        objSvcInfo.DetailsSelection.MinDataValue = FieldInfoUtil.RequestValue();
                        objSvcInfo.DetailsSelection.MaxDataValue = FieldInfoUtil.RequestValue();
                        objSvcInfo.DetailsSelection.IsCalculatedData = FieldInfoUtil.RequestValue();
                        objSvcInfo.DetailsSelection.DisplaySequence = FieldInfoUtil.RequestValue();

                        if (Page.VirtualPageName == "SS_WIPDataPopupVP")
                        {
                            objSvcInfo.DetailsSelection.scsWIPDataGroupingType = FieldInfoUtil.RequestValue();
                            objSvcInfo.scsWIPMainContainerList = FieldInfoUtil.RequestValue();
                        }

                        bool IsWIPDataByLotExist = true;
                        bool IsWIPDataByItemExist = true;
                        // init the result object
                        WIPData_Result objResult = new WIPData_Result();

                        // execute to request the value
                        ResultStatus resultStatus = objSvc.GetEnvironment(objSvcData, new WIPData_Request { Info = objSvcInfo }, out objResult);

                        if (resultStatus.IsSuccess)
                        {
                            string sCurrentServiceName = _ndoServiceNameField.Data != null ? _ndoServiceNameField.Data.ToString() : "";
                            string sWIPDataSetup = "";
                            if (objResult.Value.WIPDataSetup != null)
                                sWIPDataSetup = objResult.Value.WIPDataSetup.Name.ToString();

                            // Select the first record by default if it is called as a popup
                            if (objResult.Value.ProcessTypeSelection != null)
                            {
                                List<NamedObjectRef> oProcessTypeSelection = new List<NamedObjectRef>();
                                foreach (NamedObjectRef oProcessType in objResult.Value.ProcessTypeSelection)
                                {
                                    oProcessTypeSelection.Add(new NamedObjectRef() { Name = oProcessType.Name });
                                }
                                //_ndoProcessTypeField.Data = oProcessTypeSelection != null ? oProcessTypeSelection[0] : null;
                            }

                            if (objResult.Value.ServiceNameSelection != null)
                            {
                                List<NamedObjectRef> oServiceNames = new List<NamedObjectRef>();
                                foreach (string sServiceName in objResult.Value.ServiceNameSelection)
                                {
                                    oServiceNames.Add(new NamedObjectRef() { Name = sServiceName });
                                }
                                CWC.NamedObject _ndoSvcNameTemp = _ndoServiceNameField;
                                SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoSvcNameTemp, oServiceNames.ToArray());
                            }
                            if (objResult.Value.DetailsSelection != null)
                            {
                                //Populate the DisplayFilter field
                                List<string> _strDisplayFilterSelection = (from WIPDataDetail in objResult.Value.DetailsSelection
                                                                           where WIPDataDetail.DisplayFilter != null
                                                                           select WIPDataDetail.DisplayFilter.Name).Distinct().ToList();

                                CWC.NamedObject _ndoDisplayFilterField = Page.FindCamstarControl("WIPData_DisplayFilter") as CWC.NamedObject;
                                NamedObjectRef[] NamedObjectRefList = new NamedObjectRef[_strDisplayFilterSelection.Count + 1];
                                NamedObjectRefList[0] = new NamedObjectRef();
                                NamedObjectRefList[0].Name = "";
                                int x = 1;
                                foreach (string DisplayFilterName in _strDisplayFilterSelection)
                                {
                                    NamedObjectRefList[x] = new NamedObjectRef();
                                    NamedObjectRefList[x].Name = DisplayFilterName;
                                    x += 1;
                                }
                                _ndoDisplayFilterField.DropDownControl.Items.Clear();
                                SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoDisplayFilterField, NamedObjectRefList);
                                _ndoDisplayFilterField.TextEditControl.Text = "";

                                //Prepare the WIP data for lot
                                List<WIPDataDetails> ByLotDetails = (from WIPDataDetail in objResult.Value.DetailsSelection
                                                                     where WIPDataDetail.WaferScribeNumber == null
                                                                     orderby WIPDataDetail.DisplaySequence.Value
                                                                     select WIPDataDetail).ToList();
                                //Assign to the ViewState variable to later use for the hide and show the lot grid rows
                                ViewState["ByLotDetails"] = ByLotDetails;

                                //Bind the WIP data for lot to lot grid
                                if (ByLotDetails != null && ByLotDetails.Count() > 0)
                                {
                                    if (_HideDataPointDetails != null && _HideDataPointDetails.IsChecked)
                                    {
                                        (_gridByLotDetails.GridContext as BoundContext).Fields["WIPDataValidValuesButton"].Visible = false;
                                        (_gridByLotDetails.GridContext as BoundContext).Fields["ss_UOM"].Visible = false;
                                    }
                                    else
                                    {
                                        (_gridByLotDetails.GridContext as BoundContext).Fields["WIPDataValidValuesButton"].Visible = true;
                                        (_gridByLotDetails.GridContext as BoundContext).Fields["ss_UOM"].Visible = true;
                                    }
                                    _gridByLotDetails.Data = ByLotDetails.ToArray();
                                    CamstarWebControl.SetRenderToClient(_gridByLotDetails);
                                }
                                else
                                {
                                    IsWIPDataByLotExist = false;
                                }

                                //Prepare the WIP data for wafers
                                List<WIPDataDetails> ByWaferDetails = (from WIPDataDetail in objResult.Value.DetailsSelection
                                                                       where WIPDataDetail.WaferScribeNumber != null
                                                                       select WIPDataDetail).ToList();

                                //Get the list of wafers
                                List<string> Wafers = ((from WIPDataDetail in ByWaferDetails
                                                        where WIPDataDetail.WaferScribeNumber != null
                                                        select WIPDataDetail.WaferScribeNumber.ToString()).Distinct().ToList());
                                //Get the list of WIP data name
                                List<string> ByWaferWIPDataName = (from WIPDataDetail in ByWaferDetails
                                                                   where WIPDataDetail.WaferScribeNumber != null
                                                                   select WIPDataDetail.WIPDataName.Name).Distinct().ToList();

                                // IR 8959513 fix
                                // sort the Wafer WIPDataNames according to DisplaySequence
                                int iTopIndex = 0;
                                SortedDictionary<int, string> sdWaferWIPDataName = new SortedDictionary<int, string>();

                                foreach (string sWIPDataName in ByWaferWIPDataName)
                                {
                                    if (!sdWaferWIPDataName.ContainsValue(sWIPDataName))
                                    {
                                        WIPDataDetails oDetail = ByWaferDetails.FirstOrDefault(WD => WD.WIPDataName.Name == sWIPDataName);
                                        if (oDetail != null)
                                        {
                                            int iSequence = oDetail.DisplaySequence != null ? int.Parse(oDetail.DisplaySequence.ToString()) : iTopIndex++;
                                            if (sdWaferWIPDataName.ContainsKey(iSequence))
                                                iSequence = iTopIndex++;

                                            sdWaferWIPDataName.Add(iSequence, sWIPDataName);
                                            iTopIndex = iSequence > iTopIndex ? iSequence : iTopIndex;
                                        }
                                    }
                                }


                                if (Wafers != null && Wafers.Count() > 0)
                                {
                                    DataTable dtByWaferDetails = new DataTable();
                                    DataColumn dcCol = new DataColumn();
                                    dcCol.ColumnName = "WaferScribeNumber";
                                    dcCol.Caption = _lblWaferScribeNumber.Text;
                                    dtByWaferDetails.Columns.Add(dcCol);

                                    DataColumn dcCol2 = new DataColumn();
                                    dcCol2.ColumnName = "WIPDataUOM";

                                    string strHiddenCol = null;

                                    List<string> sRequiredColumnNames = new List<string>();

                                    _gridDetailsInfo.ClearData();

                                    int counter = 0;
                                    foreach (KeyValuePair<int, string> entry in sdWaferWIPDataName)
                                    {
                                        string sWIPDataName = entry.Value;
                                        WIPDataDetails DetailsInfoRow = ByWaferDetails.FirstOrDefault(WD => WD.WIPDataName.Name == sWIPDataName);
                                        counter++;
                                        if (DetailsInfoRow != null)
                                        {
                                            if (DetailsInfoRow.WIPDataName != null)
                                            {
                                                //Add the WIP Data column to the DataTable for binding to the wafer grid
                                                dtByWaferDetails.Columns.Add(DetailsInfoRow.WIPDataName.Name);

                                                //strHiddenCol = string.IsNullOrEmpty(strHiddenCol) ? "_boolFailureStatus" + DetailsInfoRow.WIPDataName.Name : strHiddenCol + "@;@_boolFailureStatus" + DetailsInfoRow.WIPDataName.Name;

                                                if (_HideDataPointDetails == null || !_HideDataPointDetails.IsChecked)
                                                {
                                                    dtByWaferDetails.Columns.Add(new DataColumn { ColumnName = "_btn" + DetailsInfoRow.WIPDataName.Name, Caption = "&nbsp;" });
                                                    dtByWaferDetails.Columns.Add(new DataColumn { ColumnName = dcCol2 + DetailsInfoRow.WIPDataName.Name, Caption = _lblUOM.Text });
                                                    dtByWaferDetails.Columns.Add(new DataColumn { ColumnName = "_boolFailureStatus" + DetailsInfoRow.WIPDataName.Name, Caption = "&nbsp;" });
                                                    strHiddenCol = string.IsNullOrEmpty(strHiddenCol) ? "_boolFailureStatus" + DetailsInfoRow.WIPDataName.Name : strHiddenCol + "@;@_boolFailureStatus" + DetailsInfoRow.WIPDataName.Name; //original
                                                }
                                                else
                                                {
                                                    dtByWaferDetails.Columns.Add(new DataColumn { ColumnName = "_boolFailureStatus" + DetailsInfoRow.WIPDataName.Name, Caption = "&nbsp;" });
                                                    strHiddenCol = string.IsNullOrEmpty(strHiddenCol) ? "_boolFailureStatus" + DetailsInfoRow.WIPDataName.Name + "@;@WIPDataUOM" + DetailsInfoRow.WIPDataName.Name + "@;@_btn" + DetailsInfoRow.WIPDataName.Name : strHiddenCol + "@;@_boolFailureStatus" + DetailsInfoRow.WIPDataName.Name + "@;@_btn" + DetailsInfoRow.WIPDataName.Name + "@;@WIPDataUOM" + DetailsInfoRow.WIPDataName.Name;
                                                    if (counter == sdWaferWIPDataName.Count)
                                                        dtByWaferDetails.Columns.Add(new DataColumn { ColumnName = "_spacer", Caption = "&nbsp;" });
                                                }

                                                if (DetailsInfoRow.IsRequired == true)
                                                    sRequiredColumnNames.Add(DetailsInfoRow.WIPDataName.Name);

                                                WIPDataDetails[] oExistingList = (_gridDetailsInfo.GridContext as BoundContext).Data as WIPDataDetails[];
                                                List<WIPDataDetails> oNewList = new List<WIPDataDetails>();

                                                if (oExistingList == null)
                                                    oExistingList = new WIPDataDetails[0];

                                                foreach (WIPDataDetails oRow in oExistingList)
                                                {
                                                    WIPDataDetails oCurrentRow = new WIPDataDetails();
                                                    oCurrentRow.WIPDataName = oRow.WIPDataName;
                                                    oCurrentRow.ForProcessType = oRow.ForProcessType;
                                                    oCurrentRow.IsRequired = oRow.IsRequired;
                                                    oCurrentRow.IsHidden = oRow.IsHidden;
                                                    oCurrentRow.DisplayFilter = oRow.DisplayFilter;
                                                    oCurrentRow.ss_UOM = oRow.ss_UOM;
                                                    oCurrentRow.IsCalculatedData = oRow.IsCalculatedData;

                                                    oNewList.Add(oCurrentRow);
                                                }

                                                WIPDataDetails oNewRow = new WIPDataDetails();
                                                oNewRow.WIPDataName = DetailsInfoRow.WIPDataName == null ? null : new NamedObjectRef(DetailsInfoRow.WIPDataName.Name);
                                                oNewRow.ForProcessType = DetailsInfoRow.ForProcessType.Value;
                                                oNewRow.IsRequired = DetailsInfoRow.IsRequired.Value;
                                                oNewRow.IsHidden = DetailsInfoRow.IsHidden.Value;
                                                oNewRow.DisplayFilter = DetailsInfoRow.DisplayFilter == null ? null : new NamedObjectRef(DetailsInfoRow.DisplayFilter.Name);
                                                oNewRow.ss_UOM = DetailsInfoRow.ss_UOM == null ? null : new NamedObjectRef(DetailsInfoRow.ss_UOM.Name);
                                                oNewRow.IsCalculatedData = DetailsInfoRow.IsCalculatedData.Value;

                                                oNewList.Add(oNewRow);

                                                (_gridDetailsInfo.GridContext as BoundContext).Data = oNewList.ToArray();
                                                _gridDetailsInfo.BoundContext.LoadData();
                                            }
                                        }
                                    }

                                    //Populate WIP data for wafer to the DataTable for binding to the wafer grid
                                    for (int intNewId = 0; intNewId < Wafers.Count(); intNewId++)
                                    {
                                        DataRow drByWaferDetails = dtByWaferDetails.NewRow();
                                        drByWaferDetails["WaferScribeNumber"] = Wafers[intNewId].ToString();
                                        foreach (string strColName in ByWaferWIPDataName)
                                        {
                                            WIPDataDetails WIPDataRow = ByWaferDetails.FirstOrDefault(WD => WD.WaferScribeNumber == Wafers[intNewId].ToString() && WD.WIPDataName.Name == strColName);
                                            if (WIPDataRow.ss_UOM != null)
                                                drByWaferDetails[dcCol2 + strColName] = WIPDataRow.ss_UOM.Name;
                                            if (WIPDataRow != null && WIPDataRow.WIPDataValue != null)
                                            {
                                                drByWaferDetails[WIPDataRow.WIPDataName.Name] = WIPDataRow.WIPDataValue;
                                                if (WIPDataRow.ChangeLimitFailed == true || WIPDataRow.LowerLimitFailed == true || WIPDataRow.UpperLimitFailed == true) { drByWaferDetails["_boolFailureStatus" + WIPDataRow.WIPDataName.Name] = "ss_wipdata_datavaluefailed=true"; }
                                            }
                                        }
                                        dtByWaferDetails.Rows.Add(drByWaferDetails);
                                    }

                                    // generate the typename based on the combination of WIPDataNames
                                    string sUniqueTypeName = "";
                                    foreach (string sWaferWIPDataName in ByWaferWIPDataName)
                                        sUniqueTypeName = sUniqueTypeName + sWaferWIPDataName.Replace(" ", "") + "|";

                                    if (sUniqueTypeName.Length > 100)
                                        sUniqueTypeName = sWIPDataSetup + "_" + sCurrentServiceName + "_" + sUniqueTypeName.Substring(0, 100);

                                    ViewState["WIPData_WaferDetails_TypeName"] = sUniqueTypeName;

                                    JQDataGrid _gridByWaferDetailsTemp = Page.FindCamstarControl("WIPData_ByWaferDetails") as JQDataGrid;

                                    var theme = Page.Session["CurrentTheme"] != null ? Page.Session["CurrentTheme"].ToString() : string.Empty;
                                    SEMI.AppCode.GridUtility.WIPData_ItemListGrid_SetColumns(this, dtByWaferDetails, "WIPData_ByWaferDetails", ByWaferWIPDataName.ToArray(), sUniqueTypeName, false, strHiddenCol.Split(new string[] { "@;@" }, StringSplitOptions.RemoveEmptyEntries), true, null, sRequiredColumnNames.ToArray(), !theme.ToLower().Equals("horizon"));
                                    SEMI.AppCode.GridUtility.ItemListGrid_BindDataTable(this, dtByWaferDetails, ref _gridByWaferDetailsTemp, sUniqueTypeName);
                                    CamstarWebControl.SetRenderToClient(_gridByWaferDetailsTemp);
                                }
                                else
                                {
                                    DataTable dtNullData = new DataTable();
                                    DataColumn dcCol = new DataColumn();
                                    dcCol.ColumnName = "SpacerColumn";
                                    dcCol.Caption = "&nbsp;";
                                    dtNullData.Columns.Add(dcCol);
                                    string sNullTypeName = "WIPData_WaferNullType";
                                    JQDataGrid _gridByWaferDetailsTemp = Page.FindCamstarControl("WIPData_ByWaferDetails") as JQDataGrid;
                                    SEMI.AppCode.GridUtility.WIPData_ItemListGrid_SetColumns(this, dtNullData, "WIPData_ByWaferDetails", null, sNullTypeName);
                                    IsWIPDataByItemExist = false;
                                }
                                SetControls();
                            }


                            if (IsWIPDataByLotExist && !IsWIPDataByItemExist)
                            {
                                _LotItemGridDisplayField.Data = 1;
                            }

                            if (!IsWIPDataByLotExist && IsWIPDataByItemExist)
                            {
                                _LotItemGridDisplayField.Data = 2;
                            }

                            SetGridDisplay();
                            Page.CollectDataContract();
                        }
                        else
                            DisplayMessage(resultStatus);
                    }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
        }

        public void SetControls()
        {
            try
            {
                List<WIPDataDetails> ByLotDetails = (List<WIPDataDetails>)ViewState["ByLotDetails"];
                if (ByLotDetails != null && ByLotDetails.Count() > 0)
                {
                    if (!_ShowHiddenField.CheckControl.Checked)
                    {
                        if (_ndoDisplayFilterField.TextEditControl.Text != "")
                            _gridByLotDetails.Data = ByLotDetails.Where(WIPData => WIPData.IsHidden == false && WIPData.DisplayFilter != null && WIPData.DisplayFilter.Name == _ndoDisplayFilterField.TextEditControl.Text).ToArray();
                        else
                            _gridByLotDetails.Data = ByLotDetails.Where(WIPData => WIPData.IsHidden == false).ToArray();
                    }
                    else
                    {
                        if (_ndoDisplayFilterField.TextEditControl.Text != "")
                            _gridByLotDetails.Data = ByLotDetails.Where(WIPData => WIPData.DisplayFilter != null && WIPData.DisplayFilter.Name.ToString() == _ndoDisplayFilterField.TextEditControl.Text).ToArray();
                        else
                            _gridByLotDetails.Data = ByLotDetails.ToArray();
                    }
                    CamstarWebControl.SetRenderToClient(_gridByLotDetails);
                }

                if (_gridByWaferDetails.BoundContext.Fields.Count() > 1)
                {
                    WIPDataDetails[] DetailsInfo = (WIPDataDetails[])((_gridDetailsInfo.GridContext as ItemDataContext).Data);
                    for (int i = 0; i < _gridByWaferDetails.BoundContext.Fields.Count(); i++)
                    {
                        if (_gridByWaferDetails.BoundContext.Fields[i].ID != "WaferScribeNumber" && _gridByWaferDetails.BoundContext.Fields[i].ID.IndexOf("_btn", 0) < 0 && _gridByWaferDetails.BoundContext.Fields[i].ID.IndexOf("_boolFailureStatus", 0) < 0)
                        {
                            if (DetailsInfo != null)
                            {
                                WIPDataDetails DetailsInfoRow = DetailsInfo.FirstOrDefault(WD => WD.WIPDataName.Name == _gridByWaferDetails.BoundContext.Fields[i].ID);

                                if (DetailsInfoRow != null)
                                {
                                    if (_HideDataPointDetails == null || !_HideDataPointDetails.IsChecked)
                                    {
                                        if ((bool)DetailsInfoRow.IsHidden)
                                        {
                                            if (!_ShowHiddenField.CheckControl.Checked)
                                            {
                                                _gridByWaferDetails.BoundContext.Fields[i].Visible = false;
                                                _gridByWaferDetails.BoundContext.Fields[i + 1].Visible = false;
                                            }
                                            else
                                            {
                                                _gridByWaferDetails.BoundContext.Fields[i].Visible = !((_ndoDisplayFilterField.TextEditControl.Text != "") && (_ndoDisplayFilterField.TextEditControl.Text != (DetailsInfoRow.DisplayFilter != null ? DetailsInfoRow.DisplayFilter.Name : "")));
                                                _gridByWaferDetails.BoundContext.Fields[i + 1].Visible = !((_ndoDisplayFilterField.TextEditControl.Text != "") && (_ndoDisplayFilterField.TextEditControl.Text != (DetailsInfoRow.DisplayFilter != null ? DetailsInfoRow.DisplayFilter.Name : "")));
                                            } // (!_ShowHiddenField.CheckControl.Checked)
                                        }
                                        else
                                        {
                                            _gridByWaferDetails.BoundContext.Fields[i].Visible = !((_ndoDisplayFilterField.TextEditControl.Text != "") && (_ndoDisplayFilterField.TextEditControl.Text != (DetailsInfoRow.DisplayFilter != null ? DetailsInfoRow.DisplayFilter.Name : "")));
                                            _gridByWaferDetails.BoundContext.Fields[i + 1].Visible = !((_ndoDisplayFilterField.TextEditControl.Text != "") && (_ndoDisplayFilterField.TextEditControl.Text != (DetailsInfoRow.DisplayFilter != null ? DetailsInfoRow.DisplayFilter.Name : "")));
                                        } // if ((bool)DetailsInfoRow.IsHidden)

                                        (_gridByLotDetails.GridContext as BoundContext).Fields["ss_UOM"].Visible = true;
                                        (_gridByLotDetails.GridContext as BoundContext).Fields["WIPDataValidValuesButton"].Visible = true;
                                    }
                                    else
                                    {
                                        (_gridByLotDetails.GridContext as BoundContext).Fields["ss_UOM"].Visible = false;
                                        (_gridByLotDetails.GridContext as BoundContext).Fields["WIPDataValidValuesButton"].Visible = false;
                                    }
                                } // if (DetailsInfoRow != null)
                            } // if (_gridByWaferDetails.BoundContext.Fields[i].ID != "WaferScribeNumber" && _gridByWaferDetails.BoundContext.Fields[i].ID.IndexOf("_btn",0) < 0 && _gridByWaferDetails.BoundContext.Fields[i].ID.IndexOf("_boolFailureStatus",0) < 0)
                        }
                    } // for (int i = 0; i < _gridByWaferDetails.BoundContext.Fields.Count(); i++)\
                    CamstarWebControl.SetRenderToClient(_gridByWaferDetails);
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
        }

        public void ResetControls(int ClearFlag)
        {
            try
            {
                if (ClearFlag <= 0)
                {
                    //  _txtCommentsField.TextControl.Text = "";
                    _SkipSPCFailureEmailField.CheckControl.Checked = false;
                    _LotItemGridDisplayField.Data = _LotItemGridDisplayField.DefaultValue;
                }

                _ndoDisplayFilterField.PickListPanelControl.ClearSelectionValues();
                //_LotItemGridDisplayField.PickListPanelControl.ClearSelectionValues();
                _ndoDisplayFilterField.TextEditControl.Text = "";
                //_LotItemGridDisplayField.TextEditControl.Text = "";
                _ShowHiddenField.CheckControl.Checked = false;

                _gridByLotDetails.ClearData();
                _gridByWaferDetails.ClearData();
                _gridByWaferDetails.Data = null;
                var itc = (_gridByWaferDetails.GridContext as ItemDataContext);
                itc.UnboundData = null;
                (_gridByWaferDetails.GridContext as ItemDataContext).Fields.Clear();

                _gridDetailsInfo.ClearData();

                JQField colSpacer = new JQField("_spacer");
                colSpacer.LabelText = " ";
                (_gridByWaferDetails.GridContext as ItemDataContext).Fields.Add(colSpacer);

                _gridDetailsInfo.ClearData();
                ViewState["ByLotDetails"] = null;
                ViewState["WIPData_WaferDetails_TypeName"] = null;

                // clear the data contracts as the mess with the data loading of grid values
                Page.PortalContext.DataContract.SetValueByName("WIPData_KeyDM", null);
                Page.PortalContext.DataContract.SetValueByName("WIPData_GridRowIdDM", null);
                Page.PortalContext.DataContract.SetValueByName("WIPData_WIPDataNameDM", null);
                Page.PortalContext.DataContract.SetValueByName("WIPData_WIPDataValueDM", null);

            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
        }

        public void SetGridDisplay()
        {
            int MaxRowCount = 15;
            switch ((LotItemGridDisplayEnum)_LotItemGridDisplayField.Data)
            {
                case LotItemGridDisplayEnum.LotOnly:
                    _gridByLotDetails.Hidden = false;
                    _gridByWaferDetails.Hidden = true;
                    _gridByLotDetails.BoundContext.VisibleRows = MaxRowCount;
                    if (_gridByLotDetails.BoundContext.GetTotalRows() > 0 && _gridByLotDetails.BoundContext.GetTotalRows() < MaxRowCount)
                    {
                        _gridByLotDetails.BoundContext.VisibleRows = 0;
                    }
                    _gridByWaferDetails.BoundContext.VisibleRows = 0;
                    break;
                case LotItemGridDisplayEnum.ItemOnly:
                    _gridByLotDetails.Hidden = true;
                    _gridByWaferDetails.Hidden = false;
                    _gridByWaferDetails.BoundContext.VisibleRows = MaxRowCount;
                    if (_gridByWaferDetails.BoundContext.GetTotalRows() > 0 && _gridByWaferDetails.BoundContext.GetTotalRows() < MaxRowCount)
                    {
                        _gridByWaferDetails.BoundContext.VisibleRows = 0;
                    }
                    _gridByLotDetails.BoundContext.VisibleRows = 0;
                    break;
                default:
                    _gridByLotDetails.Hidden = false;
                    _gridByWaferDetails.Hidden = false;
                    _gridByLotDetails.BoundContext.VisibleRows = 10;
                    _gridByWaferDetails.BoundContext.VisibleRows = 10;
                    if (_gridByLotDetails.BoundContext.GetTotalRows() > 0 && _gridByLotDetails.BoundContext.GetTotalRows() < 10)
                    {
                        _gridByLotDetails.BoundContext.VisibleRows = 0;
                    }
                    if (_gridByWaferDetails.BoundContext.GetTotalRows() > 0 && _gridByWaferDetails.BoundContext.GetTotalRows() < 10)
                    {
                        _gridByWaferDetails.BoundContext.VisibleRows = 0;
                    }
                    break;
            }
        }

        public void UpdateWaferGridCell(string RowID, string WIPDataName, string WIPDataValue)
        {
            if (ViewState["WIPData_WaferDetails_TypeName"] != null)
            {
                string sTypeName = ViewState["WIPData_WaferDetails_TypeName"].ToString();
                Type _dynamicItemType = SEMI.AppCode.GridUtility.RetrieveDynamicType(sTypeName);
                Array aData = _gridByWaferDetails.Data as Array;
                var properties = _dynamicItemType.GetProperties();
                int iRowIndex = int.Parse(RowID);

                var ob = aData.GetValue(iRowIndex);
                var pp = properties.FirstOrDefault(p => p.Name == WIPDataName);
                if (pp != null)
                    pp.SetValue(ob, WIPDataValue, null);

                _gridByWaferDetails.Data = aData;

                ////var itc = (_gridByWaferDetails.GridContext as ItemDataContext);
                ////if (itc.UnboundData == null)
                ////    itc.UnboundData = new Dictionary<UnboundKey, object>();

                ////var Keys = (from uk in itc.UnboundData.Keys
                ////            where (uk.Row == RowID && uk.Column == WIPDataName)
                ////            select uk).ToList();

                ////if (Keys.Count > 0)
                ////    itc.UnboundData[Keys[0]] = WIPDataValue;
                ////else
                ////{
                ////    var k = new UnboundKey() { Row = RowID, Column = WIPDataName };
                ////    itc.UnboundData.Add(k, WIPDataValue);
                ////}

                CamstarWebControl.SetRenderToClient(_gridByWaferDetails);
                Page.RenderToClient = true;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            foreach (PERS.JQFieldBase Col in _gridByWaferDetails.Settings.Columns)
            {
                if (Col.Name.Length > 3 && Col.Name.Substring(0, 4) == "_btn")
                {
                    (Col.CellActions[0].DefaultAction as PERS.FloatPageOpenAction).FrameLocation.Width = 0;
                    (Col.CellActions[0].DefaultAction as PERS.FloatPageOpenAction).FrameLocation.Height = 0;
                }
            }
            base.OnPreRender(e);

        }
        #endregion

    }
}



