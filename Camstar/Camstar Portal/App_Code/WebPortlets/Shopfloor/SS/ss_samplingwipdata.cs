/* Copyright 2020 Siemens */
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
/// Summary description for SS_SamplingWIPData
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_SamplingWIPData : MatrixWebPart
    {
        #region Properties

        protected CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("SamplingWIPData_ComputerName") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoEmployeeField { get { return Page.FindCamstarControl("SamplingWIPData_Employee") as CWC.NamedObject; } }
        protected CWC.ContainerList _ContainerField { get { return Page.FindCamstarControl("SamplingWIPData_Container") as CWC.ContainerList; } }
        protected CWC.NamedObject _ndoProcessTypeField { get { return Page.FindCamstarControl("SamplingWIPData_ProcessType") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEquipmentField { get { return Page.FindCamstarControl("SamplingWIPData_Equipment") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoServiceNameField { get { return Page.FindCamstarControl("SamplingWIPData_ServiceName") as CWC.NamedObject; } }
        protected CWC.CheckBox _ShowHiddenField { get { return Page.FindCamstarControl("SamplingWIPData_ShowHiddenCheckBox") as CWC.CheckBox; } }
        protected CWC.CheckBox _SkipSPCFailureEmailField { get { return Page.FindCamstarControl("SamplingWIPData_SkipSPCFailureEmail") as CWC.CheckBox; } }
        protected CWC.NamedObject _ndoDisplayFilterField { get { return Page.FindCamstarControl("SamplingWIPData_DisplayFilter") as CWC.NamedObject; } }
        protected CWC.DropDownList _LotItemGridDisplayField { get { return Page.FindCamstarControl("SamplingWIPData_LotItemGridDisplay") as CWC.DropDownList; } }
        protected JQDataGrid _gridByLotDetails { get { return Page.FindCamstarControl("SamplingWIPData_ByLotDetails") as JQDataGrid; } }
        protected JQDataGrid _gridByWaferDetails { get { return Page.FindCamstarControl("SamplingWIPData_ByWaferDetails") as JQDataGrid; } }
        protected JQDataGrid _gridDetailsInfo { get { return Page.FindCamstarControl("SamplingWIPData_DetailsInfo") as JQDataGrid; } }
        protected CWC.TextBox _txtCommentsField { get { return Page.FindCamstarControl("SamplingWIPData_Comments") as CWC.TextBox; } }
        protected CWC.TextBox _txtSamplesTestedField { get { return Page.FindCamstarControl("SamplingWIPData_SamplesTested") as CWC.TextBox; } }
        protected CWC.Label _lblWaferScribeNumber { get { return Page.FindCamstarControl("SamplingWIPData_WaferScribeNumber_Label") as CWC.Label; } }
		protected CWC.Label _lblUOM { get { return Page.FindCamstarControl("SamplingWIPData_UOM_Label") as CWC.Label; } }

        // framework control flag
        CWC.CheckBox _chkIsActive { get { return Page.FindCamstarControl("SamplingWIPData_IsActive") as CWC.CheckBox; } }
        CWC.CheckBox _chkIsPopup { get { return Page.FindCamstarControl("SamplingWIPData_IsPopup") as CWC.CheckBox; } }

        protected CWC.TextBox _txtSamplingWIPDataInit { get { return Page.FindCamstarControl("SamplingWIPData_Init") as CWC.TextBox; } }
        protected CWC.Button _btnReset { get { return Page.FindCamstarControl("SamplingWIPData_ResetButton") as CWC.Button; } }
        protected CWC.Button _btnSubmit { get { return Page.FindCamstarControl("SamplingWIPData_SubmitButton") as CWC.Button; } }
        protected bool _bIsPopup { get { return Page.IsAJAXFloatingFrame; } }

        protected const string _sWIPDataSetupNameSessionId = "WIPData_ValidValuesPopup_WIPDataSetupName_SessionIdentifier";
        protected const string _sSamplingWIPDataServiceIdentifierSessionId = "WIPData_ValidValuesPopup_IsSamplingWIPDataService_SessionIdentifier";
		protected const string _sSamplingWIPDataContainerSessionId = "WIPData_ValidValuesPopup_Container_SessionIdentifier";
		protected const string _sSamplingWIPDataServiceNameSessionId = "WIPData_ValidValuesPopup_ServiceName_SessionIdentifier";
		#endregion

        #region Page Events
        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            if (!_bIsPopup)
            {
                _txtSamplingWIPDataInit.DataChanged += _txtSamplingWIPDataInit_DataChanged;
                _ContainerField.DataChanged += _ContainerField_DataChanged;
                _ndoProcessTypeField.DataChanged += _ndoProcessTypeField_DataChanged;
                _ndoEquipmentField.DataChanged += _ndoEquipmentField_DataChanged;
                _ndoServiceNameField.DataChanged += _ndoServiceNameField_DataChanged;
            }
        }
        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.OnPreExecute += new EventHandler<FormProcessingEventArgs>(Page_OnPreExecute);
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);              

                if (!Page.IsPostBack)
                {
                    _txtComputerNameField.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
					if (_txtSamplesTestedField.Required == true)
					{
						_txtSamplesTestedField.Visible = true;
						_txtSamplesTestedField.Hidden = false;
					}
					else
					{
						_txtSamplesTestedField.Visible = false;
						_txtSamplesTestedField.Hidden = true;
					}

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
                }

                if (_chkIsPopup.CheckControl.Checked)                
                    _ndoServiceNameField.Enabled = false;                

                bool bIsValidValuesPopupCall = false;
                // Assign the WIP Data Name to the data contract for pop up
                if (Page.EventTarget == "ctl00$WebPartManager$SS_SamplingWIPDataWP$SamplingWIPData_ByWaferDetails" && Page.EventArgument.Contains("OnCellActionClick"))
                {
                    var parts = Page.EventArgument.Split(':');
                    string strWIPDataName = parts[1].Substring(4);

                    Page.PortalContext.DataContract.SetValueByName("SamplingWIPData_WIPDataNameDM", new NamedObjectRef(strWIPDataName));
                    Page.CollectDataContract();
                    Page.Session[_sWIPDataSetupNameSessionId] = strWIPDataName;
                    Page.Session[_sSamplingWIPDataServiceIdentifierSessionId] = "SamplingWIPData";
					Page.Session[_sSamplingWIPDataContainerSessionId] = _ContainerField.Data.ToString();
					Page.Session[_sSamplingWIPDataServiceNameSessionId] = _ndoServiceNameField.Data.ToString();
                    bIsValidValuesPopupCall = true;
                }

                // Refresh Details datagrid value from Popup selection page
                if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument")
                {
                    var KeyDM = Page.PortalContext.DataContract.GetValueByName<string>("SamplingWIPData_KeyDM");
                    var GridRowIdDM = Page.PortalContext.DataContract.GetValueByName<string>("SamplingWIPData_GridRowIdDM");
                    var WIPDataNameDM = Page.PortalContext.DataContract.GetValueByName<NamedObjectRef>("SamplingWIPData_WIPDataNameDM") != null ? Page.PortalContext.DataContract.GetValueByName<NamedObjectRef>("SamplingWIPData_WIPDataNameDM").Name : null;
                    var WIPDataValueDM = Page.PortalContext.DataContract.GetValueByName("SamplingWIPData_WIPDataValueDM");

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
                    Page.PortalContext.DataContract.SetValueByName("SamplingWIPData_KeyDM", null);
                    Page.PortalContext.DataContract.SetValueByName("SamplingWIPData_GridRowIdDM", null);
                    Page.PortalContext.DataContract.SetValueByName("SamplingWIPData_WIPDataNameDM", null);
                    Page.PortalContext.DataContract.SetValueByName("SamplingWIPData_WIPDataValueDM", null);
                }

                if (!Page.IsPostBack)
                {
                    if (_bIsPopup)
                    {
                        _btnReset.Enabled = false;
                        _btnReset.Visible = false;
                        _btnReset.Hidden = true;
                        _btnSubmit.Enabled = false;
                        _btnSubmit.Visible = false;

                        FetchData();
                    }
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _ndoServiceNameField_DataChanged(object sender, EventArgs e)
        {
            ResetControls(30);
            FetchData();
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _ndoEquipmentField_DataChanged(object sender, EventArgs e)
        {
            _ndoEquipmentField.TextEditControl.ToolTip = _ndoEquipmentField.TextEditControl.Text;
            ResetControls(20);
            FetchData();
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _ndoProcessTypeField_DataChanged(object sender, EventArgs e)
        {
            ResetControls(10);
            FetchData();
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _ContainerField_DataChanged(object sender, EventArgs e)
        {
            ResetControls(5);
            FetchData();
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _txtSamplingWIPDataInit_DataChanged(object sender, EventArgs e)
        {
            FetchData();
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void SamplingWIPData_ShowHiddenCheckBox_DataChanged()
        {
            SetControls();
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void SamplingWIPData_DisplayFilter_DataChanged()
        {
            SetControls();
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void SamplingWIPData_ResetButton_Click()
        {
            ResetControls(0);
            FetchData();
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void SamplingWIPData_LotItemGridDisplay_DataChanged()
        {
            SetGridDisplay();
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

			if (serviceData is OM.SamplingWIPData)
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

                    OM.SamplingWIPData svcData = serviceData as OM.SamplingWIPData;
                    svcData.Details = new WIPDataDetails[intTotalWIPDataByLot + (intTotalWafers * intWIPDataForWafer)];

                    // collect the wip data for lot
                    for (int x = 0; x < intTotalWIPDataByLot; x++)
                    {
                        iIndex = iIndex + 1;
                        svcData.Details[iIndex] = new WIPDataDetails();
                        svcData.Details[iIndex].ListItemAction = OM.ListItemAction.Add;
                        svcData.Details[iIndex].WIPDataName = new NamedObjectRef();
                        svcData.Details[iIndex].WIPDataName.Name = ByLotDetails[x].WIPDataName.Name;
                        svcData.Details[iIndex].WIPDataValue = ByLotDetails[x].WIPDataValue;
                        svcData.Details[iIndex].ForProcessType = ByLotDetails[x].ForProcessType;
						if (ByLotDetails[x].ss_UOM != null)
						{
							svcData.Details[iIndex].ss_UOM = new NamedObjectRef();
							svcData.Details[iIndex].ss_UOM.Name = ByLotDetails[x].ss_UOM.Name;
						}
                    }

                    // collect the wip data for wafers
                    WIPDataDetails[] DetailsInfo = (WIPDataDetails[])((_gridDetailsInfo.GridContext as ItemDataContext).Data);
                    for (int x = 0; x < intTotalWafers; x++)
                    {
                        foreach (WIPDataDetails DetailsInfoRow in DetailsInfo)
                        {
                            string sRowId = x.ToString().PadLeft(6, '0');

                            iIndex = iIndex + 1;
                            svcData.Details[iIndex] = new WIPDataDetails();
                            svcData.Details[iIndex].ListItemAction = OM.ListItemAction.Add;
                            svcData.Details[iIndex].WaferScribeNumber = _gridByWaferDetails.GridContext.GetCell(sRowId, "WaferScribeNumber").ToString();
                            svcData.Details[iIndex].WIPDataName = new NamedObjectRef();
                            svcData.Details[iIndex].WIPDataName.Name = DetailsInfoRow.WIPDataName.Name;
                            svcData.Details[iIndex].WIPDataValue = _gridByWaferDetails.GridContext.GetCell(sRowId, DetailsInfoRow.WIPDataName.Name).ToString();
                            svcData.Details[iIndex].ForProcessType = DetailsInfoRow.ForProcessType;
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

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void Page_OnPreExecute(object sender, FormProcessingEventArgs e)
        {
            if (e.Info is OM.SamplingWIPData_Info && e.Data is OM.SamplingWIPData)
            {
                OM.Info serviceInfo = e.Info;
                OM.Service serviceData = e.Data;

                (serviceInfo as OM.SamplingWIPData_Info).SPCTxnDataList = new SPCTxnData_Info
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

        #endregion

        #region Methods

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void FetchData()
        {
            try
            {
                bool bExecute = false;
                if (_chkIsActive != null)
                    bExecute = (_chkIsActive.CheckControl.Checked || _chkIsPopup.CheckControl.Checked);
                if (bExecute)
                {
                    // get the session and user profile
                    var fs = FrameworkManagerUtil.GetFrameworkSession();

                    // init the service, service data and service info objects
                    SamplingWIPDataService objSvc = new SamplingWIPDataService(fs.CurrentUserProfile);
                    SamplingWIPData objSvcData = new SamplingWIPData();
                    objSvcData.Container = new ContainerRef((string)_ContainerField.TextEditControl.Text);
                    objSvcData.ProcessType = new NamedObjectRef(_ndoProcessTypeField.TextEditControl.Text);
                    objSvcData.Equipment = new NamedObjectRef(_ndoEquipmentField.TextEditControl.Text);
                    objSvcData.ServiceName = _ndoServiceNameField.Data != null ? _ndoServiceNameField.Data.ToString() : "";

                    SamplingWIPData_Info objSvcInfo = new SamplingWIPData_Info();
                    objSvcInfo.Container = FieldInfoUtil.RequestValue();
                    objSvcInfo.ProcessTypeSelection = FieldInfoUtil.RequestValue();
                    objSvcInfo.EquipmentSelection = FieldInfoUtil.RequestValue();
                    objSvcInfo.ServiceNameSelection = FieldInfoUtil.RequestValue();
                    objSvcInfo.LotItemGridDisplay = FieldInfoUtil.RequestSelectionValue();
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
                    objSvcInfo.SamplesTested = FieldInfoUtil.RequestValue();
                    objSvcInfo.SampleType = FieldInfoUtil.RequestValue();
                    objSvcInfo.WIPDataSetup = FieldInfoUtil.RequestValue();

                    // init the result object
                    SamplingWIPData_Result objResult = new SamplingWIPData_Result();

                    // execute to request the value
                    ResultStatus resultStatus = objSvc.GetEnvironment(objSvcData, new SamplingWIPData_Request { Info = objSvcInfo }, out objResult);

                    if (resultStatus.IsSuccess)
                    {
                        if (objResult.Value.SampleType != null && objResult.Value.SampleType == SampleTypeEnum.Counted)
                        {
                            _txtSamplesTestedField.Required = true;
                            _txtSamplesTestedField.Visible = true;
							_txtSamplesTestedField.Hidden = false;
                        }
                        else
                        {
                            _txtSamplesTestedField.Required = false;
                            _txtSamplesTestedField.Visible = false;
							_txtSamplesTestedField.Hidden = true;
                        }
                        if (objResult.Value.SamplesTested != null)
                            _txtSamplesTestedField.Data = objResult.Value.SamplesTested;

						// comment out to fix dropdownlist with empty row
						//_LotItemGridDisplayField.SetSelectionValues(objResult.Environment.LotItemGridDisplay.SelectionValues);
                        _LotItemGridDisplayField.TextEditControl.Text = objResult.Environment.LotItemGridDisplay.SelectionValues.Rows[0].Values[1].ToString();

                        // Select the first record by default if it is called as a popup
                        if (objResult.Value.ProcessTypeSelection != null)
                        {
                            List<NamedObjectRef> oProcessTypeSelection = new List<NamedObjectRef>();
                            foreach (NamedObjectRef oProcessType in objResult.Value.ProcessTypeSelection)
                            {
                                oProcessTypeSelection.Add(new NamedObjectRef() { Name = oProcessType.Name });
                            }
                            _ndoProcessTypeField.Data = oProcessTypeSelection != null ? oProcessTypeSelection[0] : null;
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

                            CWC.NamedObject _ndoDisplayFilterField = Page.FindCamstarControl("SamplingWIPData_DisplayFilter") as CWC.NamedObject;
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
                                                                 select WIPDataDetail).ToList();
                            //Assign to the ViewState variable to later use for the hide and show the lot grid rows
                            ViewState["ByLotDetails"] = ByLotDetails;

                            //Bind the WIP data for lot to lot grid
                            if (ByLotDetails != null && ByLotDetails.Count() > 0)
                            {
                                _gridByLotDetails.Data = ByLotDetails.ToArray();
                                CamstarWebControl.SetRenderToClient(_gridByLotDetails);
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

                            if (Wafers != null && Wafers.Count() > 0)
                            {
								if (objResult.Value.SampleType != null && objResult.Value.SampleType == SampleTypeEnum.Counted)
								{
									if (_txtSamplesTestedField.TextControl.Text == "")
										_txtSamplesTestedField.TextControl.Text = Wafers.Count().ToString();
								}

                                DataTable dtByWaferDetails = new DataTable();
                                DataColumn dcCol = new DataColumn();
                                dcCol.ColumnName = "WaferScribeNumber";
                                dcCol.Caption = _lblWaferScribeNumber.Text;
                                dtByWaferDetails.Columns.Add(dcCol);

								DataColumn dcCol2 = new DataColumn();
								dcCol2.ColumnName = "WIPDataUOM";

                                string strHiddenCol = null;
                                int intNewWIPDataInfoId = 0;

                                _gridDetailsInfo.ClearData();

                                foreach (string strColName in ByWaferWIPDataName)
                                {
                                    WIPDataDetails DetailsInfoRow = ByWaferDetails.FirstOrDefault(WD => WD.WIPDataName.Name == strColName);

                                    if (DetailsInfoRow != null)
                                    {
                                        if (DetailsInfoRow.WIPDataName != null)
                                        {
                                            //Add the WIP Data column to the DataTable for binding to the wafer grid
											dtByWaferDetails.Columns.Add(DetailsInfoRow.WIPDataName.Name);
											dtByWaferDetails.Columns.Add(new DataColumn { ColumnName = dcCol2 + DetailsInfoRow.WIPDataName.Name, Caption = _lblUOM.Text });                                           										
                                            dtByWaferDetails.Columns.Add(new DataColumn { ColumnName = "_btn" + DetailsInfoRow.WIPDataName.Name, Caption = "&nbsp;" });
                                            dtByWaferDetails.Columns.Add(new DataColumn { ColumnName = "_boolFailureStatus" + DetailsInfoRow.WIPDataName.Name, Caption = "&nbsp;" });
                                            strHiddenCol = string.IsNullOrEmpty(strHiddenCol) ? "_boolFailureStatus" + DetailsInfoRow.WIPDataName.Name : strHiddenCol + "@;@_boolFailureStatus" + DetailsInfoRow.WIPDataName.Name;

                                            intNewWIPDataInfoId += 1;

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

                                                oNewList.Add(oCurrentRow);
                                            }

                                            WIPDataDetails oNewRow = new WIPDataDetails();
                                            oNewRow.WIPDataName = DetailsInfoRow.WIPDataName == null ? null : new NamedObjectRef(DetailsInfoRow.WIPDataName.Name);
                                            oNewRow.ForProcessType = DetailsInfoRow.ForProcessType.Value;
                                            oNewRow.IsRequired = DetailsInfoRow.IsRequired.Value;
                                            oNewRow.IsHidden = DetailsInfoRow.IsHidden.Value;
                                            oNewRow.DisplayFilter = DetailsInfoRow.DisplayFilter == null ? null : new NamedObjectRef(DetailsInfoRow.DisplayFilter.Name);
											oNewRow.ss_UOM = DetailsInfoRow.ss_UOM == null ? null : new NamedObjectRef(DetailsInfoRow.ss_UOM.Name);

                                            oNewList.Add(oNewRow);
                                            intNewWIPDataInfoId += 1;

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
                                ViewState["SamplingWIPData_WaferDetails_TypeName"] = sUniqueTypeName;

                                JQDataGrid _gridByWaferDetailsTemp = Page.FindCamstarControl("SamplingWIPData_ByWaferDetails") as JQDataGrid;

                                var theme = Page.Session["CurrentTheme"] != null ? Page.Session["CurrentTheme"].ToString() : string.Empty;
                                SEMI.AppCode.GridUtility.WIPData_ItemListGrid_SetColumns(this, dtByWaferDetails, "SamplingWIPData_ByWaferDetails", ByWaferWIPDataName.ToArray(), sUniqueTypeName, false, strHiddenCol.Split(new string[] { "@;@" }, StringSplitOptions.RemoveEmptyEntries),true,null,null,!theme.ToLower().Equals("horizon"));
                                SEMI.AppCode.GridUtility.ItemListGrid_BindDataTable(this, dtByWaferDetails, ref _gridByWaferDetailsTemp, sUniqueTypeName);
                                CamstarWebControl.SetRenderToClient(_gridByWaferDetailsTemp);                                
                            }
                            SetControls();
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

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void SetControls()
        {
            try
            {
                List<WIPDataDetails> ByLotDetails = (List<WIPDataDetails>)ViewState["ByLotDetails"];
                if (ByLotDetails != null)
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
                }
                if (_gridByWaferDetails.BoundContext.Fields.Count() > 1)
                {
                    WIPDataDetails[] DetailsInfo = (WIPDataDetails[])((_gridDetailsInfo.GridContext as ItemDataContext).Data);
                    for (int i = 0; i < _gridByWaferDetails.BoundContext.Fields.Count(); i++)
                    {                        
                        if (_gridByWaferDetails.BoundContext.Fields[i].ID != "WaferScribeNumber" && _gridByWaferDetails.BoundContext.Fields[i].ID.IndexOf("_btn",0) < 0 && _gridByWaferDetails.BoundContext.Fields[i].ID.IndexOf("_boolFailureStatus",0) < 0)
                        {
                            if (DetailsInfo != null)
                            {
                                WIPDataDetails DetailsInfoRow = DetailsInfo.FirstOrDefault(WD => WD.WIPDataName.Name == _gridByWaferDetails.BoundContext.Fields[i].ID);
                                if (DetailsInfoRow != null)
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
                                        }
                                    }
                                    else
                                    {
                                        _gridByWaferDetails.BoundContext.Fields[i].Visible = !((_ndoDisplayFilterField.TextEditControl.Text != "") && (_ndoDisplayFilterField.TextEditControl.Text != (DetailsInfoRow.DisplayFilter != null ? DetailsInfoRow.DisplayFilter.Name : "")));
                                        _gridByWaferDetails.BoundContext.Fields[i + 1].Visible = !((_ndoDisplayFilterField.TextEditControl.Text != "") && (_ndoDisplayFilterField.TextEditControl.Text != (DetailsInfoRow.DisplayFilter != null ? DetailsInfoRow.DisplayFilter.Name : "")));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void ResetControls(int ClearFlag)
        {
            try
            {
                if (ClearFlag <= 0)
                {
                    if (_txtCommentsField != null)
						_txtCommentsField.TextControl.Text = "";
                    _SkipSPCFailureEmailField.CheckControl.Checked = false;
                }
                _txtSamplesTestedField.ClearData();
                _ndoDisplayFilterField.PickListPanelControl.ClearSelectionValues();
               // _LotItemGridDisplayField.PickListPanelControl.ClearSelectionValues();
                _ndoDisplayFilterField.TextEditControl.Text = "";
               // _LotItemGridDisplayField.TextEditControl.Text = "";
                _ShowHiddenField.CheckControl.Checked = false;

				//set default value for dropdownlist
				_LotItemGridDisplayField.Data = _LotItemGridDisplayField.DefaultValue;

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
                ViewState["SamplingWIPData_WaferDetails_TypeName"] = null;

                ////if (_ndoServiceNameField.Data != null)
                ////{
                ////    if (_ndoServiceNameField.Data.ToString().ToLower() != "trackinlot" && _ndoServiceNameField.Data.ToString().ToLower() != "trackoutlot" && _ndoServiceNameField.DropDownControl.SelectedValue.ToString().ToLower() != "completeinsertion")
                ////    {
                ////        _ndoEquipmentField.TextEditControl.Text = "";
                ////        _ndoEquipmentField.Enabled = false;
                ////    }
                ////    else
                ////    {
                ////        _ndoEquipmentField.TextEditControl.Text = _ndoEquipmentField.TextEditControl.ToolTip;
                ////        _ndoEquipmentField.Enabled = true;
                ////    }
                ////}

                // clear the data contracts as the mess with the data loading of grid values
                Page.PortalContext.DataContract.SetValueByName("SamplingWIPData_KeyDM", null);
                Page.PortalContext.DataContract.SetValueByName("SamplingWIPData_GridRowIdDM", null);
                Page.PortalContext.DataContract.SetValueByName("SamplingWIPData_WIPDataNameDM", null);
                Page.PortalContext.DataContract.SetValueByName("SamplingWIPData_WIPDataValueDM", null);  

            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void SetGridDisplay()
        {
            switch (_LotItemGridDisplayField.TextEditControl.Text)
            {
                case "Lot Only":
                    _gridByLotDetails.Hidden = false;
                    _gridByWaferDetails.Hidden = true;
                    break;
                case "Item Only":
                    _gridByLotDetails.Hidden = true;
                    _gridByWaferDetails.Hidden = false;
                    break;
                default:
                    _gridByLotDetails.Hidden = false;
                    _gridByWaferDetails.Hidden = false;
                    break;
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void UpdateWaferGridCell(string RowID, string WIPDataName, string WIPDataValue)
        {
            if (ViewState["SamplingWIPData_WaferDetails_TypeName"] != null)
            {
                string sTypeName = ViewState["SamplingWIPData_WaferDetails_TypeName"].ToString();
                Type _dynamicItemType = SEMI.AppCode.GridUtility.RetrieveDynamicType(sTypeName);
                Array aData = _gridByWaferDetails.Data as Array;
                var properties = _dynamicItemType.GetProperties();
                int iRowIndex = int.Parse(RowID);

                var ob = aData.GetValue(iRowIndex);
                var pp = properties.FirstOrDefault(p => p.Name == WIPDataName);
                if (pp != null)
                    pp.SetValue(ob, WIPDataValue, null);

                _gridByWaferDetails.Data = aData;

                var itc = (_gridByWaferDetails.GridContext as ItemDataContext);
                if (itc.UnboundData == null)
                    itc.UnboundData = new Dictionary<UnboundKey, object>();

                var Keys = (from uk in itc.UnboundData.Keys
                            where (uk.Row == RowID && uk.Column == WIPDataName)
                            select uk).ToList();

                if (Keys.Count > 0)
                    itc.UnboundData[Keys[0]] = WIPDataValue;
                else
                {
                    var k = new UnboundKey() { Row = RowID, Column = WIPDataName };
                    itc.UnboundData.Add(k, WIPDataValue);
                }

                CamstarWebControl.SetRenderToClient(_gridByWaferDetails);
                Page.RenderToClient = true;
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

        #endregion

    }
}



