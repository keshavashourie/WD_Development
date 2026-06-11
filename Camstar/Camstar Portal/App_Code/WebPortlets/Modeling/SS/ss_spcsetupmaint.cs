/* Copyright 2025 Siemens */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;

using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.WebPortlets;
using Camstar.WebPortal.Personalization;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CamstarPortal.WebControls;
using Camstar.WebPortal.FormsFramework.SPCChart2;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class SS_SPCSetupMaint : MatrixWebPart
    {
        #region Properties 
        CWC.TextBox _txtSelectedRowId { get { return Page.FindCamstarControl("SelectedRowId") as CWC.TextBox; } }
        CWC.TextBox _dName { get { return Page.FindCamstarControl("Details_Name") as CWC.TextBox; } }
        CWC.TextBox _dSPCQueryData { get { return Page.FindCamstarControl("Details_ss_DataValue") as CWC.TextBox; } }
        CWC.TextBox _dXAxisTrend { get { return Page.FindCamstarControl("Details_ss_XAxisTrendValue") as CWC.TextBox; } }
        CWC.TextBox _dANNVARPrimary { get { return Page.FindCamstarControl("Details_ss_ANNVARPrimaryValue") as CWC.TextBox; } }
        CWC.TextBox _dANNVARSecondary { get { return Page.FindCamstarControl("Details_ss_ANNVARSecondaryValue") as CWC.TextBox; } }
        CWC.TextBox _dANNVARTertiary { get { return Page.FindCamstarControl("Details_ss_ANNVARTertiaryValue") as CWC.TextBox; } }
        CWC.NamedObject _dSPCChartTypeLegacy { get { return Page.FindCamstarControl("Details_SPCChartTypeLegacy") as CWC.NamedObject; } }
        CWC.NamedObject _dSPCChartTypeInline { get { return Page.FindCamstarControl("Details_SPCChartTypeInline") as CWC.NamedObject; } }
        CWC.NamedObject _dSPCRules { get { return Page.FindCamstarControl("Details_SPCRules") as CWC.NamedObject; } }
        CWC.NamedObject _dFailureFutureHoldSetup { get { return Page.FindCamstarControl("Details_ss_FailureFutureHoldSetup") as CWC.NamedObject; } }
        CWC.NamedObject _dSPCMacro { get { return Page.FindCamstarControl("ObjectChanges_SPCMacro") as CWC.NamedObject; } }
        CWC.NamedObject _dFailureNCRSetup { get { return Page.FindCamstarControl("Details_ss_FailureNCRSetup") as CWC.NamedObject; } }
        CWC.NamedObject _dFailureHoldReason { get { return Page.FindCamstarControl("Details_ss_FailureHoldReason") as CWC.NamedObject; } }
        CWC.NamedObject _dSPCQueryOverride { get { return Page.FindCamstarControl("Details_scsSPCQueryOverride") as CWC.NamedObject; } }
        CWC.DropDownList _dFailureAction { get { return Page.FindCamstarControl("Details_FailureAction") as CWC.DropDownList; } }
        CWC.CheckBox _dDisplayHistoricalLimits { get { return Page.FindCamstarControl("Details_ss_DisplayHistoricalLimits") as CWC.CheckBox; } }
        CWC.CheckBox _dOverrideAnnotations { get { return Page.FindCamstarControl("Details_scsReqOverrideAnnotations") as CWC.CheckBox; } }
        CWC.CheckBox _dIsYVariable { get { return Page.FindCamstarControl("Details_ss_IsYVariable") as CWC.CheckBox; } }
        CWC.CheckBox _InLineSPC { get { return Page.FindCamstarControl("ObjectChanges_scsInLineSPC") as CWC.CheckBox; } }
        JQDataGrid _dLegacyQueryData { get { return Page.FindCamstarControl("Details_Data") as JQDataGrid; } }
        JQDataGrid _dTitle { get { return Page.FindCamstarControl("Details_Title") as JQDataGrid; } }
        JQDataGrid _dLegacyParams { get { return Page.FindCamstarControl("Details_Params") as JQDataGrid; } }
        JQDataGrid _dInlineParams { get { return Page.FindCamstarControl("Details_ParamsInline") as JQDataGrid; } }
        JQDataGrid _gridParams { get { return Page.FindCamstarControl("ObjectChanges_Params") as JQDataGrid; } }
        CWC.DropDownList _legendLocation { get { return Page.FindCamstarControl("Details_scsLegendLocation") as CWC.DropDownList; } }
        #endregion
        protected override void OnLoad(EventArgs e)
        {

            base.OnLoad(e);
            _dSPCChartTypeLegacy.DataChanged += new EventHandler(_dSPCChartTypeLegacy_DataChanged);
            _dSPCChartTypeInline.DataChanged += new EventHandler(_dSPCChartTypeInline_DataChanged);

            _InLineSPC.DataChanged += _InLineSPC_DataChanged;

            // Add Selected Row Id
            if (Page.DataContract.GetValueByName("SPCSetupMaint_GridRowIdDM") != null)
                _txtSelectedRowId.Data = Page.DataContract.GetValueByName("SPCSetupMaint_GridRowIdDM").ToString();

            // Check if it is a pop up close, get the return result
            if (SEMI.AppCode.UIUtility.IsPopupClose(this))
            {
                if (_txtSelectedRowId.Data != null)
                {
                    int selectedRowId = Convert.ToInt32(_txtSelectedRowId.Data.ToString());
                    string selectedValue = "";
                    SPCSetupParamsChanges[] getParamsDetails = _gridParams.Data as SPCSetupParamsChanges[];
                    if (Page.PortalContext.DataContract.GetValueByName<string>("SPCSetupMaint_ReturnedValueDM") != null)
                    {
                        selectedValue = Page.PortalContext.DataContract.GetValueByName<string>("SPCSetupMaint_ReturnedValueDM").ToString();
                        getParamsDetails[selectedRowId].ParamName = selectedValue;
                        Page.PortalContext.DataContract.SetValueByName("SPCSetupMaint_ReturnedValueDM", null);

                        _gridParams.ClearData();
                        _gridParams.Data = getParamsDetails;
                    }
                }
            }
        }

        public void _dSPCChartTypeLegacy_DataChanged(object sender, EventArgs e)
        {
            if (_dSPCChartTypeLegacy.Data != null && !_InLineSPC.CheckControl.Checked)
            {
                Camstar.WCF.ObjectStack.UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as Camstar.WCF.ObjectStack.UserProfile;

                SPCChartTypeMaintService oSvc = new SPCChartTypeMaintService(profile);
                SPCChartTypeMaint oChartType = new SPCChartTypeMaint();
                SPCChartTypeMaint_Info oChartTypeInfo = new SPCChartTypeMaint_Info();
                SPCChartTypeChanges oChartTypeChanges = new SPCChartTypeChanges();
                SPCChartTypeChanges_Info oChartTypeChangesInfo = new SPCChartTypeChanges_Info();

                oChartTypeChanges.Name = _dSPCChartTypeLegacy.Data.ToString();
                oChartType.ObjectToChange = new NamedObjectRef(_dSPCChartTypeLegacy.Data.ToString());
                oChartTypeChangesInfo.Params = new SPCChartTypeParamsChanges_Info();
                oChartTypeChangesInfo.Params.ParamName = FieldInfoUtil.RequestValue();
                oChartTypeChangesInfo.Params.DefaultValue = FieldInfoUtil.RequestValue();
                oChartTypeChangesInfo.Params.AllowMatrixOverride = FieldInfoUtil.RequestValue();

                oChartType.ObjectChanges = oChartTypeChanges;
                oChartTypeInfo.ObjectChanges = oChartTypeChangesInfo;
                SPCChartTypeMaint_Result oResult = new SPCChartTypeMaint_Result();
                ResultStatus oRS = oSvc.Load(oChartType, new SPCChartTypeMaint_Request { Info = oChartTypeInfo }, out oResult);

                if (oRS.IsSuccess)
                {
                    if (oResult.Value.ObjectChanges.Params != null)
                    {
                        SPCChartTypeParamsChanges[] oChartTypeParams = oResult.Value.ObjectChanges.Params;
                        SPCSetupDetailsParamsChanges[] oSetupDetailsParams = new SPCSetupDetailsParamsChanges[oChartTypeParams.Length];

                        int x = 0;
                        foreach (SPCChartTypeParamsChanges oChartTypeParam in oChartTypeParams)
                        {
                            oSetupDetailsParams[x] = new SPCSetupDetailsParamsChanges();
                            oSetupDetailsParams[x].ParamName = oChartTypeParam.ParamName;
                            oSetupDetailsParams[x].ParamValue = oChartTypeParam.DefaultValue;
                            oSetupDetailsParams[x].AllowMatrixOverride = oChartTypeParam.AllowMatrixOverride;
                            oSetupDetailsParams[x].ParamType = "CHART";
                            x++;
                        }
                        //(_gridDetails_Params.GridContext as BoundContext).Data = oResult.Value.ObjectChanges.Params;
                        (_dLegacyParams.GridContext as BoundContext).Data = oSetupDetailsParams;
                        _dLegacyParams.GridContext.LoadData();
                        CamstarWebControl.SetRenderToClient(_dLegacyParams);
                    }
                }
            }

            if (_dSPCChartTypeLegacy.Data == null)
            {
                _dLegacyParams.ClearData();
            }
        }

        public void _dSPCChartTypeInline_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (_dSPCChartTypeInline.Data != null)
                {
                    Camstar.WCF.ObjectStack.UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as Camstar.WCF.ObjectStack.UserProfile;

                    SPCChartTypeMaintService oSvc = new SPCChartTypeMaintService(profile);
                    SPCChartTypeMaint oChartType = new SPCChartTypeMaint();
                    SPCChartTypeMaint_Info oChartTypeInfo = new SPCChartTypeMaint_Info();
                    SPCChartTypeChanges oChartTypeChanges = new SPCChartTypeChanges();
                    SPCChartTypeChanges_Info oChartTypeChangesInfo = new SPCChartTypeChanges_Info();

                    oChartTypeChanges.Name = _dSPCChartTypeInline.Data.ToString();
                    oChartType.ObjectToChange = new NamedObjectRef(_dSPCChartTypeInline.Data.ToString());
                    oChartTypeChangesInfo.Params = new SPCChartTypeParamsChanges_Info();
                    oChartTypeChangesInfo.Params.ParamName = FieldInfoUtil.RequestValue();
                    oChartTypeChangesInfo.Params.DefaultValue = FieldInfoUtil.RequestValue();
                    oChartTypeChangesInfo.Params.AllowMatrixOverride = FieldInfoUtil.RequestValue();

                    oChartType.ObjectChanges = oChartTypeChanges;
                    oChartTypeInfo.ObjectChanges = oChartTypeChangesInfo;
                    SPCChartTypeMaint_Result oResult = new SPCChartTypeMaint_Result();
                    ResultStatus oRS = oSvc.Load(oChartType, new SPCChartTypeMaint_Request { Info = oChartTypeInfo }, out oResult);

                    if (oRS.IsSuccess)
                    {
                        if (oResult.Value.ObjectChanges.Params != null)
                        {
                            SPCChartTypeParamsChanges[] oChartTypeParams = oResult.Value.ObjectChanges.Params;
                            SPCSetupDetailsILParamsChanges[] oSetupDetailsParamsInline = new SPCSetupDetailsILParamsChanges[oChartTypeParams.Length];

                            int y = 0;
                            foreach (SPCChartTypeParamsChanges oChartTypeParam in oChartTypeParams)
                            {
                                oSetupDetailsParamsInline[y] = new SPCSetupDetailsILParamsChanges();
                                oSetupDetailsParamsInline[y].ParamName = oChartTypeParam.ParamName;
                                oSetupDetailsParamsInline[y].ParamValue = oChartTypeParam.DefaultValue;
                                oSetupDetailsParamsInline[y].AllowMatrixOverride = oChartTypeParam.AllowMatrixOverride;
                                oSetupDetailsParamsInline[y].ParamType = "CHART";
                                y++;
                            }
                            //(_gridDetails_Params.GridContext as BoundContext).Data = oResult.Value.ObjectChanges.Params;
                            (_dInlineParams.GridContext as BoundContext).Data = oSetupDetailsParamsInline;
                            _dInlineParams.GridContext.LoadData();
                            CamstarWebControl.SetRenderToClient(_dInlineParams);
                        }
                    }
                }
                else
                {
                    _dInlineParams.ClearData();
                }

                if (Page.VirtualPageName == "SS_SPCSetupMaintVP")
                {
                    if (_dLegacyQueryData.GridContext.GetCell((0), "DataColumn") != null)
                    {
                        string dataColumn = _dLegacyQueryData.GridContext.GetCell((0), "DataColumn").ToString();
                        string isYVariable = _dLegacyQueryData.GridContext.GetCell((0), "IsYVariable").ToString();
                        var dataArray = dataColumn.Split('.');

                        if (dataArray[0] != "" && dataArray[1] != "")
                        {
                            _dSPCQueryOverride.Data = dataArray[0];
                            _dSPCQueryData.Data = dataArray[1];
                            _dIsYVariable.Data = isYVariable;
                        }
                    }
                }

                for (int i = 0; i < _dLegacyParams.GridContext.GetTotalRows(); i++)
                {
                    string strLegacyParams = i.ToString().PadLeft(6, '0');
                    for (int j = 0; j < _dInlineParams.GridContext.GetTotalRows(); j++)
                    {
                        string strInlineParams = j.ToString().PadLeft(6, '0');
                        if (_dLegacyParams.GridContext.GetCell(strLegacyParams, "ParamName").ToString() != null && _dLegacyParams.GridContext.GetCell(strLegacyParams, "ParamName").ToString().Contains("AXIS"))
                        {
                            string paramAxis = _dLegacyParams.GridContext.GetCell(strLegacyParams, "ParamValue").ToString();
                            var dataArrayAxis = paramAxis.Split('.');
                            _dXAxisTrend.Data = dataArrayAxis[1];
                        }

                        if (_dLegacyParams.GridContext.GetCell(strLegacyParams, "ParamName").ToString() != null && _dLegacyParams.GridContext.GetCell(strLegacyParams, "ParamName").ToString().Contains("ANNVAR"))
                        {
                            string paramANNVAR = _dLegacyParams.GridContext.GetCell(strLegacyParams, "ParamValue").ToString();
                            var dataArrayANNVAR = paramANNVAR.Split('.', ' ');

                            if (dataArrayANNVAR.Length == 6)
                            {
                                _dANNVARPrimary.Data = dataArrayANNVAR[1];
                                _dANNVARSecondary.Data = dataArrayANNVAR[3];
                                _dANNVARTertiary.Data = dataArrayANNVAR[5];
                            }

                            if (dataArrayANNVAR.Length == 4)
                            {
                                _dANNVARPrimary.Data = dataArrayANNVAR[1];
                                _dANNVARSecondary.Data = dataArrayANNVAR[3];
                            }

                            if (dataArrayANNVAR.Length == 2)
                            {
                                _dANNVARPrimary.Data = dataArrayANNVAR[1];
                            }
                        }
                    }
                }

                string tipother = null;

                foreach (SPCSetupDetailsParamsChanges legacyParam in (_dLegacyParams.GridContext as BoundContext).Data as SPCSetupDetailsParamsChanges[])
                {
                    if (legacyParam.ParamName == "TIP_OTHER")
                    {
                        tipother = legacyParam.ParamValue.ToString();
                        continue;
                    }

                    if (legacyParam.ParamName == "SUBGROUPSIZE")
                    {
                        legacyParam.ParamName = "By";
                    }

                    var name = SPCHelper.LegacyCommandToInline(legacyParam.ParamName.ToString());
                    var value = legacyParam.ParamValue;

                    foreach (SPCSetupDetailsILParamsChanges inlineParam in (_dInlineParams.GridContext as BoundContext).Data as SPCSetupDetailsILParamsChanges[])
                    {
                        if (inlineParam.ParamName == name)
                        {
                            inlineParam.ParamValue = value;
                            break;
                        }
                    }

                    if (legacyParam.ParamName == "By")
                    {
                        legacyParam.ParamName = "SUBGROUPSIZE";
                    }
                }

                List<SPCSetupDetailsILParamsChanges> newParamList = new List<SPCSetupDetailsILParamsChanges>();

                SPCSetupDetailsILParamsChanges customToolTipParam = new SPCSetupDetailsILParamsChanges();
                customToolTipParam.ParamName = "CustomToolTip";
                customToolTipParam.ParamValue = tipother;


                if (tipother != null)
                {
                    newParamList.Add(customToolTipParam);
                }


                SPCSetupDetailsILParamsChanges[] inlineParams = (_dInlineParams.GridContext as BoundContext).Data as SPCSetupDetailsILParamsChanges[];
                foreach (SPCSetupDetailsILParamsChanges inlineParam in inlineParams)
                {
                    newParamList.Add(inlineParam);
                }
                (_dInlineParams.GridContext as BoundContext).Data = newParamList.ToArray();

                if (_legendLocation.Data == null)
                    _legendLocation.Data = _legendLocation.DefaultValue;
            }
            catch
            {

            }
        }

        public void _InLineSPC_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (_InLineSPC.CheckControl.Checked)
                {
                    _dSPCMacro.Enabled = false;
                    _dSPCChartTypeLegacy.Enabled = false;
                    _dSPCChartTypeInline.Enabled = true;
                    _legendLocation.Enabled = true;
                    (_dLegacyQueryData.GridContext as BoundContext).Fields["DataColumn"].Editable = false;
                    (_dLegacyQueryData.GridContext as BoundContext).Fields["IsYVariable"].Editable = false;
                    (_dLegacyParams.GridContext as BoundContext).Fields["ParamName"].Editable = false;
                    (_dLegacyParams.GridContext as BoundContext).Fields["ParamValue"].Editable = false;
                    (_dLegacyParams.GridContext as BoundContext).Fields["AllowMatrixOverride"].Editable = false;
                    (_dLegacyParams.GridContext as BoundContext).Fields["DisplaySequence"].Editable = false;
                    (_dLegacyParams.GridContext as BoundContext).Fields["ParamType"].Editable = false;
                    (_dInlineParams.GridContext as BoundContext).Fields["ParamName"].Editable = true;
                    (_dInlineParams.GridContext as BoundContext).Fields["ParamValue"].Editable = true;
                    (_dInlineParams.GridContext as BoundContext).Fields["AllowMatrixOverride"].Editable = true;
                    (_dInlineParams.GridContext as BoundContext).Fields["DisplaySequence"].Editable = true;
                    (_dInlineParams.GridContext as BoundContext).Fields["ParamType"].Editable = true;

                    _dSPCQueryOverride.Enabled = true;
                    _dSPCQueryData.Enabled = true;
                    _dIsYVariable.Enabled = true;
                    _dXAxisTrend.Enabled = true;
                    _dANNVARPrimary.Enabled = true;
                    _dANNVARSecondary.Enabled = true;
                    _dANNVARTertiary.Enabled = true;

                    _dSPCQueryData.DefaultValue = "DATAVALUE";
                    _dXAxisTrend.DefaultValue = "INSTANCEID";
                    _dANNVARPrimary.DefaultValue = "INSTANCEID";
                    _dANNVARSecondary.DefaultValue = "COMMENTS";
                    _dANNVARTertiary.DefaultValue = "COMMENTABREV";

                }

                if (!_InLineSPC.CheckControl.Checked)
                {
                    _dSPCMacro.Enabled = true;
                    _dSPCChartTypeLegacy.Enabled = true;
                    _dSPCChartTypeInline.Enabled = false;
                    _legendLocation.Enabled = false;
                    (_dLegacyQueryData.GridContext as BoundContext).Fields["DataColumn"].Editable = true;
                    (_dLegacyQueryData.GridContext as BoundContext).Fields["IsYVariable"].Editable = true;
                    (_dLegacyParams.GridContext as BoundContext).Fields["ParamName"].Editable = true;
                    (_dLegacyParams.GridContext as BoundContext).Fields["ParamValue"].Editable = true;
                    (_dLegacyParams.GridContext as BoundContext).Fields["AllowMatrixOverride"].Editable = true;
                    (_dLegacyParams.GridContext as BoundContext).Fields["DisplaySequence"].Editable = true;
                    (_dLegacyParams.GridContext as BoundContext).Fields["ParamType"].Editable = true;
                    (_dInlineParams.GridContext as BoundContext).Fields["ParamName"].Editable = false;
                    (_dInlineParams.GridContext as BoundContext).Fields["ParamValue"].Editable = false;
                    (_dInlineParams.GridContext as BoundContext).Fields["AllowMatrixOverride"].Editable = false;
                    (_dInlineParams.GridContext as BoundContext).Fields["DisplaySequence"].Editable = false;
                    (_dInlineParams.GridContext as BoundContext).Fields["ParamType"].Editable = false;
                    _dSPCQueryOverride.Enabled = false;
                    _dSPCQueryData.Enabled = false;
                    _dIsYVariable.Enabled = false;
                    _dXAxisTrend.Enabled = false;
                    _dANNVARPrimary.Enabled = false;
                    _dANNVARSecondary.Enabled = false;
                    _dANNVARTertiary.Enabled = false;
                }
            }
            catch
            {

            }
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            if (((Camstar.WCF.ObjectStack.SPCSetupMaint)serviceData).ObjectChanges != null)
            {
                if (((Camstar.WCF.ObjectStack.SPCSetupMaint)serviceData).ObjectChanges.Details != null)
                {
                    foreach (SPCSetupDetailsChanges detail in ((Camstar.WCF.ObjectStack.SPCSetupMaint)serviceData).ObjectChanges.Details)
                    {
                        if (detail.Data != null)
                            foreach (SPCSetupDetailsDataChanges dataDetail in detail.Data)
                                if (dataDetail.IsYVariable != null && dataDetail.IsYVariable.IsEmpty)
                                    dataDetail.IsYVariable = false;
                        if (detail.Params != null)
                            foreach (SPCSetupDetailsParamsChanges paramsDetail in detail.Params)
                                if (paramsDetail.AllowMatrixOverride != null && paramsDetail.AllowMatrixOverride.IsEmpty)
                                    paramsDetail.AllowMatrixOverride = false;
                    }
                }

                if (((Camstar.WCF.ObjectStack.SPCSetupMaint)serviceData).ObjectChanges.Params != null)
                {
                    foreach (SPCSetupParamsChanges param in ((Camstar.WCF.ObjectStack.SPCSetupMaint)serviceData).ObjectChanges.Params)
                    {
                        if (param.IsDynamic != null && param.IsDynamic.IsEmpty)
                            param.IsDynamic = false;
                    }
                }
            }
        }
    }
}