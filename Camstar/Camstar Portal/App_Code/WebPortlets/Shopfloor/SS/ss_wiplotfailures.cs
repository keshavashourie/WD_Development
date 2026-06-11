/* Copyright Siemens 2025 */
using System;
using System.Collections.Generic;
using System.Linq;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Request = Camstar.WCF.Services.Request;

/// <summary>
/// Summary description for SS_WIPLotFailures
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_WIPLotFailures : MatrixWebPart
    {
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("WIPLotFailures_SelectionId") as CWC.TextBox; } }
        protected CWC.TextBox _txtPrimaryServiceType { get { return Page.FindCamstarControl("WIPLotFailures_PrimarySvcType") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoProcessType { get { return Page.FindCamstarControl("WIPLotFailures_ProcessType") as CWC.NamedObject; } }
        protected JQDataGrid _gridCommonLimits { get { return Page.FindCamstarControl("WIPLotFailures_CommonLimits") as JQDataGrid; } }
        protected JQDataGrid _gridLossReasonLimits { get { return Page.FindCamstarControl("WIPLotFailures_LossReasonLimits") as JQDataGrid; } }
        protected JQDataGrid _gridPDALimits { get { return Page.FindCamstarControl("WIPLotFailures_PDALimits") as JQDataGrid; } }
        protected JQDataGrid _gridBinsLimits { get { return Page.FindCamstarControl("WIPLotFailures_BinsLimits") as JQDataGrid; } }
        protected JQDataGrid _gridWIPDataLimits { get { return Page.FindCamstarControl("WIPLotFailures_WIPDataLimits") as JQDataGrid; } }

        protected CWC.RadioButton _rdbCommon { get { return Page.FindCamstarControl("CommonRadio") as CWC.RadioButton; } }
        protected CWC.RadioButton _rdbLossReason { get { return Page.FindCamstarControl("LossReasonRadio") as CWC.RadioButton; } }
        protected CWC.RadioButton _rdbPDA { get { return Page.FindCamstarControl("PDARadio") as CWC.RadioButton; } }
        protected CWC.RadioButton _rdbBins { get { return Page.FindCamstarControl("BinsRadio") as CWC.RadioButton; } }
        protected CWC.RadioButton _rdbWIPData { get { return Page.FindCamstarControl("WIPDataRadio") as CWC.RadioButton; } }

        protected CWC.Label _lblCommonCount { get { return Page.FindCamstarControl("CommonCountLabel") as CWC.Label; } }
        protected CWC.Label _lblLossReasonCount { get { return Page.FindCamstarControl("LossReasonCountLabel") as CWC.Label; } }
        protected CWC.Label _lblPDACount { get { return Page.FindCamstarControl("PDACountLabel") as CWC.Label; } }
        protected CWC.Label _lblBinsCount { get { return Page.FindCamstarControl("BinsCountLabel") as CWC.Label; } }
        protected CWC.Label _lblWIPDataCount { get { return Page.FindCamstarControl("WIPDataCountLabel") as CWC.Label; } }
        protected CWC.TextBox _txtTxnTypeField { get { return Page.FindCamstarControl("WIPLotFailures_TxnType") as CWC.TextBox; } }
        protected CWC.CheckBox _chkIsWIPProcessType { get { return Page.FindCamstarControl("WIPLotFailures_scsIsWIPProcessType") as CWC.CheckBox; } }

        //-----------------------------------------
        //
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _txtSelectionId.DataChanged += new EventHandler(_txtSelectionId_DataChanged);
            _ndoProcessType.DataChanged += new EventHandler(_ndoProcessType_DataChanged);

            _rdbBins.DataChanged += new EventHandler(_rdbBins_DataChanged);
            _rdbLossReason.DataChanged += new EventHandler(_rdbLossReason_DataChanged);
            _rdbCommon.DataChanged += new EventHandler(_rdbCommon_DataChanged);
            _rdbPDA.DataChanged += new EventHandler(_rdbPDA_DataChanged);
            _rdbWIPData.DataChanged += new EventHandler(_rdbWIPData_DataChanged);

            // Make Process Type field visible when Container at Move Out State
            if (IsMoveOutTxn())
                _ndoProcessType.Enabled = true;

            if((_txtTxnTypeField?.Data?.ToString() ?? "") == "")
                _ndoProcessType.Visible = false;

            if (!Page.IsPostBack)
            {
                _rdbCommon.RadioControl.Checked = true;
                SetDisplay("COMMON");

                // add the data contract member        
                if (Page.DataContract.GetValueByName("WIPLotFailures_SelectionIdDM") != null)
                    _txtSelectionId.Data = Page.DataContract.GetValueByName("WIPLotFailures_SelectionIdDM") as string;

                if (Page.DataContract.GetValueByName("WIPLotFailures_ProcessTypeDM") != null && !IsMoveOutTxn())
                    _ndoProcessType.Data = Page.DataContract.GetValueByName("WIPLotFailures_ProcessTypeDM").ToString();

                if (IsMoveOutTxn())
                    _ndoProcessType.Data = null;

                if (_txtSelectionId.Data != null)
                    FetchFailureData();
            }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        void _rdbWIPData_DataChanged(object sender, EventArgs e)
        {
            SetDisplay("WIP");
            FetchFailureData();
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        void _rdbPDA_DataChanged(object sender, EventArgs e)
        {
            SetDisplay("PDA");
            FetchFailureData();
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        void _rdbCommon_DataChanged(object sender, EventArgs e)
        {
            SetDisplay("COMMON");
            FetchFailureData();
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        void _rdbLossReason_DataChanged(object sender, EventArgs e)
        {
            SetDisplay("LOSSREASON");
            FetchFailureData();
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        void _rdbBins_DataChanged(object sender, EventArgs e)
        {
            SetDisplay("BINS");
            FetchFailureData();
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        protected void SetDisplay(string SelectionType)
        {
            int controlsCount = Parent.Controls.Count;
            MatrixWebPart BinsWP = null;
            MatrixWebPart CommonWP = null;
            MatrixWebPart LossReasonWP = null;
            MatrixWebPart PDAWP = null;
            MatrixWebPart WIPDataWP = null;

            for (int i = 0; i < controlsCount; i++)
            {
                if (Parent.Controls[i].ID == "BinsWP")
                    BinsWP = Parent.Controls[i] as MatrixWebPart;
                else if (Parent.Controls[i].ID == "CommonWP")
                    CommonWP = Parent.Controls[i] as MatrixWebPart;
                else if (Parent.Controls[i].ID == "LossReasonsWP")
                    LossReasonWP = Parent.Controls[i] as MatrixWebPart;
                else if (Parent.Controls[i].ID == "PDAWP")
                    PDAWP = Parent.Controls[i] as MatrixWebPart;
                else if (Parent.Controls[i].ID == "WIPDataWP")
                    WIPDataWP = Parent.Controls[i] as MatrixWebPart;
            }

            _gridBinsLimits.Hidden = true;
            _gridCommonLimits.Hidden = true;
            _gridLossReasonLimits.Hidden = true;
            _gridPDALimits.Hidden = true;
            _gridWIPDataLimits.Hidden = true;

            BinsWP.Hidden = true;
            CommonWP.Hidden = true;
            LossReasonWP.Hidden = true;
            PDAWP.Hidden = true;
            WIPDataWP.Hidden = true;

            _rdbBins.RadioControl.Checked = false;
            _rdbCommon.RadioControl.Checked = false;
            _rdbLossReason.RadioControl.Checked = false;
            _rdbPDA.RadioControl.Checked = false;
            _rdbWIPData.RadioControl.Checked = false;

            switch (SelectionType)
            {
                case "BINS" :
                    _gridBinsLimits.Hidden = false;
                    BinsWP.Hidden = false;
                    _rdbBins.RadioControl.Checked = true;
                    break;
                case "LOSSREASON" :
                    _gridLossReasonLimits.Hidden = false;
                    LossReasonWP.Hidden = false;
                    _rdbLossReason.RadioControl.Checked = true;
                    break;
                case "COMMON" :
                    _gridCommonLimits.Hidden = false;
                    CommonWP.Hidden = false;
                    _rdbCommon.RadioControl.Checked = true;
                    break;
                case "PDA" :
                    _gridPDALimits.Hidden = false;
                    PDAWP.Hidden = false;
                    _rdbPDA.RadioControl.Checked = true;
                    break;
                case "WIP" :
                    _gridWIPDataLimits.Hidden = false;
                    WIPDataWP.Hidden = false;
                    _rdbWIPData.RadioControl.Checked = true;
                    break;
            }

            CamstarWebControl.SetRenderToClient(_gridBinsLimits);
            CamstarWebControl.SetRenderToClient(_gridCommonLimits);
            CamstarWebControl.SetRenderToClient(_gridLossReasonLimits);
            CamstarWebControl.SetRenderToClient(_gridPDALimits);
            CamstarWebControl.SetRenderToClient(_gridWIPDataLimits);

            CamstarWebControl.SetRenderToClient(_rdbBins);
            CamstarWebControl.SetRenderToClient(_rdbCommon);
            CamstarWebControl.SetRenderToClient(_rdbLossReason);
            CamstarWebControl.SetRenderToClient(_rdbPDA);
            CamstarWebControl.SetRenderToClient(_rdbWIPData);
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        void _ndoProcessType_DataChanged(object sender, EventArgs e)
        {
            FetchFailureData();
        } // _ndoProcessType_DataChanged

        //-----------------------------------------
        //
        //-----------------------------------------
        void _txtSelectionId_DataChanged(object sender, EventArgs e)
        {
            FetchFailureData();
        } // _txtSelectionId_DataChanged

        //-----------------------------------------
        //
        //-----------------------------------------
        protected void FetchFailureData()
        {
            try
            {
                string sServiceType = _txtPrimaryServiceType.Data != null ? _txtPrimaryServiceType.Data.ToString() : "WIPMain";

                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                // Run proper constructor
                var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
                var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
                var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

                var oServiceData = CreateServiceData(sServiceType);

                var info = CreateServiceInfo(sServiceType);
                var oServiceInfo = info as WIPMain_Info;

                if (_txtSelectionId.Data != null)
                {
                    (oServiceData as WIPMain).SelectionId = _txtSelectionId.Data.ToString();

                    (oServiceData as WIPMain).scsIsWIPProcessType = true;

                    if (_ndoProcessType.Data != null)
                        (oServiceData as WIPMain).ProcessType = _ndoProcessType.Data as NamedObjectRef;

                    if (_ndoProcessType.SelectionData == null || IsMoveOutTxn())
                        oServiceInfo.ProcessTypeSelection = FieldInfoUtil.RequestValue();

                    oServiceInfo.WIPLotFailures = new WIPLotFailures_Info();
                    oServiceInfo.WIPLotFailures.FailureAction = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPLotFailures.FailureCategory = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPLotFailures.FailureCondition = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPLotFailures.FailureType = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPLotFailures.Bins = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPLotFailures.LossReasonNames = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPLotFailures.LossReasonName = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPLotFailures.WIPDataObjectType = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPLotFailures.WIPDataEquipmentName = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPLotFailures.WIPDataServiceName = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPLotFailures.WIPDataNameName = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPLotFailures.WIPDataValue = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPLotFailures.WIPDataLimit = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPLotFailures.YieldLimit = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPLotFailures.YieldValue = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPLotFailures.YieldType = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPLotFailures.YieldTypeName = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPLotFailures.YieldResult = FieldInfoUtil.RequestValue();
                    oServiceInfo.WIPLotFailures.WaferScribeNumber = FieldInfoUtil.RequestValue();

                    var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
                    (oRequest as Request).Info = oServiceInfo;

                    ResultStatus oResultStatus = new ResultStatus();
                    Result oResult = new Result();

                    oResultStatus = (oService as IShopFloorBase).ResolveSelectionId(oServiceData as DCObject, (oRequest as Request), out oResult);

                    if (oResultStatus.IsSuccess)
                    {
                        var oResultValue = oResult.Value as WIPMain;

                        if (oServiceInfo.ProcessTypeSelection != null || IsMoveOutTxn())
                        {
                            CWC.NamedObject _ndoProcessTypeTemp = _ndoProcessType;
                            SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoProcessTypeTemp, oResultValue?.ProcessTypeSelection);
                        }

                        // US494280: Set the Selection Values when the lot is in Move Out State
                        if (IsMoveOutTxn())
                        {
                            NamedObjectRef[] objProcessTypeSelection = oResultValue?.ProcessTypeSelection as NamedObjectRef[];
                            NamedObjectRef objProcessType = objProcessTypeSelection.FirstOrDefault(p => p.Name == (_ndoProcessType?.Data?.ToString() ?? ""));
                            _ndoProcessType.Data = objProcessType ?? objProcessTypeSelection[0];
                        }

                        if (oResultValue.WIPLotFailures != null)
                        {
                            int iCommon = 0, iCommonFailures = 0;
                            int iLoss = 0, iLossFailures = 0;
                            int iBins = 0, iBinsFailures = 0;
                            int iPDA = 0, iPDAFailures = 0;
                            int iWIPData = 0, iWIPDataFailures = 0;

                            List<WIPLotFailures> oCommonLimits = new List<WIPLotFailures>();
                            List<WIPLotFailures> oLossReasonLimits = new List<WIPLotFailures>();
                            List<WIPLotFailures> oBinsLimits = new List<WIPLotFailures>();
                            List<WIPLotFailures> oPDALimits = new List<WIPLotFailures>();
                            List<WIPLotFailures> oWIPDataLimits = new List<WIPLotFailures>();

                            foreach (WIPLotFailures oFailure in oResultValue.WIPLotFailures)
                            {
                                WIPLotFailures oClone = oFailure;

                                switch (oClone.FailureCategory.ToString().ToUpper())
                                {
                                    case "COMMON":
                                    case "NOSETUP":
                                    case "LOTSIZE":
                                        iCommon++;
                                        if (oClone.YieldType == null && oClone.YieldTypeName != null)
                                            oClone.YieldType = new NamedObjectRef(oClone.YieldTypeName.ToString());
                                        if (oClone.YieldResult == "FAIL")
                                            iCommonFailures++;
                                        oCommonLimits.Add(oClone);
                                        break;
                                    case "LOSSREASON":
                                        iLoss++;
                                        if (oClone.YieldResult == "FAIL")
                                            iLossFailures++;
                                        oLossReasonLimits.Add(oClone);
                                        break;
                                    case "BINS":
                                        iBins++;
                                        if (oClone.YieldResult == "FAIL")
                                            iBinsFailures++;
                                        oBinsLimits.Add(oClone);
                                        break;
                                    case "PDA":
                                        iPDA++;
                                        if (oClone.YieldResult == "FAIL")
                                            iPDAFailures++;
                                        oPDALimits.Add(oClone);
                                        break;
                                    case "WIPDATA":
                                        iWIPData++;
                                        if (oClone.YieldResult == "FAIL")
                                            iWIPDataFailures++;
                                        oWIPDataLimits.Add(oClone);
                                        break;
                                } // switch
                            } // foreach (WIPLotFailures oFailure in oResult.Value.WIPLotFailures)

                            // set the total records next to the radio buttons

                            // bind the data to the respective grids
                            (_gridBinsLimits.GridContext as BoundContext).Data = oBinsLimits.ToArray();
                            (_gridCommonLimits.GridContext as BoundContext).Data = oCommonLimits.ToArray();
                            (_gridLossReasonLimits.GridContext as BoundContext).Data = oLossReasonLimits.ToArray();
                            (_gridPDALimits.GridContext as BoundContext).Data = oPDALimits.ToArray();
                            (_gridWIPDataLimits.GridContext as BoundContext).Data = oWIPDataLimits.ToArray();

                            _lblCommonCount.LabelText = "(" + iCommonFailures.ToString() + "/" + iCommon.ToString() + ")";
                            _lblLossReasonCount.LabelText = "(" + iLossFailures.ToString() + "/" + iLoss.ToString() + ")";
                            _lblBinsCount.LabelText = "(" + iBinsFailures.ToString() + "/" + iBins.ToString() + ")";
                            _lblPDACount.LabelText = "(" + iPDAFailures.ToString() + "/" + iPDA.ToString() + ")";
                            _lblWIPDataCount.LabelText = "(" + iWIPDataFailures.ToString() + "/" + iWIPData.ToString() + ")";

                            CamstarWebControl.SetRenderToClient(_gridBinsLimits);
                            CamstarWebControl.SetRenderToClient(_gridCommonLimits);
                            CamstarWebControl.SetRenderToClient(_gridLossReasonLimits);
                            CamstarWebControl.SetRenderToClient(_gridPDALimits);
                            CamstarWebControl.SetRenderToClient(_gridWIPDataLimits);

                        } // if (oResult.Value.WIPLotFailures != null)
                    } // if (oResultStatus.IsSuccess)
                    else
                    {
                        DisplayMessage(oResultStatus);
                    }

                } // if (_txtSelectionId.Data != null)

            }
            catch(Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message.ToString(), false));
            }
        } // FetchFailureData

        private Boolean IsMoveOutTxn()
        {
            return (_txtTxnTypeField?.Data?.ToString() ?? "") == "Move Out";
        }
    }
}