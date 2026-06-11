/* Copyright 2025 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web;
using System.Web.UI;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;

using SEMI.AppCode;

/// <summary>
/// Summary description for SS_SPCRealTimeMonitoring
/// </summary>
/// 

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_SPCRealTimeMonitoring : MatrixWebPart
    {
        protected CWC.NamedObject _ndoSPCSetup { get { return Page.FindCamstarControl("ObjectChanges_SPCSetup") as CWC.NamedObject; } }
        protected CWC.TextBox _txtIntervalIDs { get { return Page.FindCamstarControl("IntervalIDs") as CWC.TextBox; } }
        protected CWC.PagePanel _pnlRefreshSlider { get { return Page.FindCamstarControl("RefreshSliderPanel") as CWC.PagePanel; } }
        protected CWC.Button _btnExecuteSPC { get { return Page.FindCamstarControl("Btn_ExecuteSPC") as CWC.Button; } }
        protected CWC.Button _btnClear { get { return Page.FindCamstarControl("SearchClear") as CWC.Button; } }
        protected CWC.Button _btnStopResume { get { return Page.FindCamstarControl("StopResumeBtn") as CWC.Button; } }
        protected CWC.TextBox _txtStatus { get { return Page.FindCamstarControl("Status") as CWC.TextBox; } }
        protected CWC.TextBox _txtInputData { get { return Page.FindCamstarControl("txthiddenInputData") as CWC.TextBox; } }
        protected JQDataGrid _gridSPCParamsGrid { get { return Page.FindCamstarControl("AdHocSPC_SPCParams") as JQDataGrid; } }
        protected CWC.TextBox _txtQueryParams { get { return Page.FindCamstarControl("txtQueryParams") as CWC.TextBox; } }
        protected CWC.TextBox _txtSPCFailureActionField { get { return Page.FindCamstarControl("SPCTxnDataList_SPCFailureAction") as CWC.TextBox; } }
        protected CWC.TextBox _txtNameField { get { return Page.FindCamstarControl("SPCTxnDataList_Name") as CWC.TextBox; } }
        protected CWC.TextBox _txtSPCResultField { get { return Page.FindCamstarControl("SPCTxnDataList_SPCResult") as CWC.TextBox; } }
        protected CWC.TextBox _txtSPCResultFileNameField { get { return Page.FindCamstarControl("SPCTxnDataList_SPCResultFilename") as CWC.TextBox; } }
        protected DataEnvelopControl _envDataEnvelop { get { return Page.FindCamstarControl("Envelop") as DataEnvelopControl; } }
        protected CWC.TextBox _txtCPHeight { get { return Page.FindCamstarControl("CPHeight") as CWC.TextBox; } }
        protected CWC.TextBox _txtCPWidth { get { return Page.FindCamstarControl("CPWidth") as CWC.TextBox; } }
        protected CWC.TextBox _txtCPHeightOffset { get { return Page.FindCamstarControl("CPHeightOffset") as CWC.TextBox; } }
        protected CWC.TextBox _txtCPWidthOffset { get { return Page.FindCamstarControl("CPWidthOffset") as CWC.TextBox; } }
        protected CWC.PagePanel _pnlChartPanel { get { return Page.FindCamstarControl("ChartPanel") as CWC.PagePanel; } }
        protected JQDataGrid _gridSPCTxnDataList { get { return Page.FindCamstarControl("TxnDataList") as JQDataGrid; } }
        protected CWC.TextBox _txtSPCTxnDataIndex { get { return Page.FindCamstarControl("SPCTxnDataIndex") as CWC.TextBox; } }
        protected CWC.TextBox _txtDataPointID { get { return Page.FindCamstarControl("DataPointID") as CWC.TextBox; } }
        protected CWC.TextBox _txtDataPointName { get { return Page.FindCamstarControl("DataPointName") as CWC.TextBox; } }
        protected CWC.Button _AnnotateButton { get { return Page.FindCamstarControl("btnAnnotate") as CWC.Button; } }
        protected CWC.TextBox _txtDVPopupHeight { get { return Page.FindCamstarControl("DVPHeight") as CWC.TextBox; } }
        protected CWC.TextBox _txtDVPopupWidth { get { return Page.FindCamstarControl("DVPWidth") as CWC.TextBox; } }
        protected CWC.Button _btnSearch { get { return Page.FindCamstarControl("SearchButton") as CWC.Button; } }
        protected CWC.DropDownList _dllSPCMatrix { get { return Page.FindCamstarControl("ObjectChanges_SPCMatrix") as CWC.DropDownList; } }
        protected CWC.TextBox _txtCDOName { get { return Page.FindCamstarControl("ObjectChanges_CDOName") as CWC.TextBox; } }
        protected CWC.TextBox _txtContainer { get { return Page.FindCamstarControl("ObjectChanges_Container") as CWC.TextBox; } }
        protected CWC.NamedObject ndoResource { get { return Page.FindCamstarControl("ObjectChanges_Resource") as CWC.NamedObject; } }
        protected CWC.NamedObject ndoProcessEquipment { get { return Page.FindCamstarControl("ObjectChanges_ss_ProcessEquipment") as CWC.NamedObject; } }
        protected CWC.RevisionedObject rdoProcessRecipe { get { return Page.FindCamstarControl("ObjectChanges_ss_ProcessRecipe") as CWC.RevisionedObject; } }
        protected CWC.TextBox txtSPCMatrixContext { get { return Page.FindCamstarControl("ObjectChanges_ss_SPCMatrixContext") as CWC.TextBox; } }
        protected CWC.TextBox txtEmployeeName { get { return Page.FindCamstarControl("EmployeeName") as CWC.TextBox; } }
        protected CWC.TextBox _txtIsAnnotationRequired { get { return Page.FindCamstarControl("IsAnnotationRequired") as CWC.TextBox; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _btnClear.Click += _btnClear_Click;
            _btnStopResume.Click += _btnStopResume_Click;
            _ndoSPCSetup.DataChanged += _ndoSPCSetup_DataChanged;
            _gridSPCParamsGrid.RowSelecting += _gridSPCParamsGrid_RowSelecting;

            if (Page.IsPostBack)
            {
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {
                    if (Page.DataContract.GetValueByName("envelopPopupInFromSubPopupDM") != null)
                        _envDataEnvelop.SS_DataPacket = Page.DataContract.GetValueByName("envelopPopupInFromSubPopupDM") as DataPacket;

                    if (_envDataEnvelop.SS_DataPacket.ResultStatusMessage != null)
                        DisplayMessage(new ResultStatus(_envDataEnvelop.SS_DataPacket.ResultStatusMessage, true));

                    ExecuteSPC();
                    StartLiveMonitoring(_txtStatus.Data.ToString());
                }
            }
            else
            {
                AddDataEnvelopDataMember();
            }

            if (Page.IsPostBack && Page.EventTarget.Contains("ctl00$WebPartManager$SPCRealTimeMonitoringSelectionWP$"))
            {
                StopLiveMonitoring();
            }
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/SPC/libs/Highstock/highstock.js");
            yield return new ScriptReference("~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/SPC/libs/Highstock/highcharts-more.js");
            yield return new ScriptReference("~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/SPC/libs/Highstock/histogram-bellcurve.js");
            yield return new ScriptReference("~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/SPC/libs/Highstock/highcharts-3d.js");
            yield return new ScriptReference("~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/SPC/libs/Highstock/exporting.js");
            yield return new ScriptReference("~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/SPC/libs/Highstock/offline-exporting.js");
            yield return new ScriptReference("~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/SPC/libs/canvg.js");

            yield return new ScriptReference("~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/SPC/ui-charts/ui-charts.min.js");
            yield return new ScriptReference("~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/SPC/localizations/ui-charts-en.min.js");
            yield return new ScriptReference("~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/SPC/localizations/ui-charts-de.min.js");

            yield return new ScriptReference("~/Scripts/User/user.js");

        }

        ResponseData _gridSPCParamsGrid_RowSelecting(object sender, JQGridEventArgs args)
        {
            throw new NotImplementedException();
        }

        void _ndoSPCSetup_DataChanged(object sender, EventArgs e)
        {
            if (_ndoSPCSetup.Data != null)
            {
                _btnSearch.Enabled = true;
            }
        }

        void _btnStopResume_Click(object sender, EventArgs e)
        {
            _pnlChartPanel.Focus();
        }

        void _btnClear_Click(object sender, EventArgs e)
        {
            Page.ClearValues();
            StopLiveMonitoring();
        }

        void StopLiveMonitoring()
        {
            _txtStatus.Data = "OFF";
            CamstarWebControl.SetRenderToClient(_txtStatus);
            _btnStopResume.Visible = false;
            _btnExecuteSPC.Enabled = false;
            _gridSPCParamsGrid.ClearData();

            if (_ndoSPCSetup.Data == null)
            {
                _btnSearch.Enabled = false;
            }
            else
            {
                _btnSearch.Enabled = true;
            }
        }

        void StartLiveMonitoring(string status)
        {
            RenderIntervalSlider();
            _btnStopResume.Visible = true;
            _txtStatus.Data = status;
            _btnExecuteSPC.Enabled = false;
            _btnSearch.Enabled = false;
            CamstarWebControl.SetRenderToClient(_txtStatus);
        }

        void RenderIntervalSlider()
        {
            _pnlRefreshSlider.Controls.Clear();
            System.Web.UI.WebControls.Panel pnlSlider = new System.Web.UI.WebControls.Panel();
            pnlSlider.ID = "refreshSlider";
            string sRefreshIntervalLabel = @"<label id='refreshIntervalLabel' class='refreshIntervalLabelClass'>Refresh Interval:</label> " +
            "<input type='text' id='refreshIntervalValue' style='margin-bottom:5px;' class='refreshIntervalValueClass' readonly>";

            _pnlRefreshSlider.Controls.Add(new LiteralControl(sRefreshIntervalLabel));
            _pnlRefreshSlider.Controls.Add(pnlSlider);
            CamstarWebControl.SetRenderToClient(_pnlRefreshSlider);

            if (!Page.ClientScript.IsStartupScriptRegistered("SPCRealTimeMonitoringScript"))
            {
                string startupScript = string.Format("SPCRealTimeMonitoringScript('{0}');", _pnlRefreshSlider.ClientID);
                ScriptManager.RegisterStartupScript(this, GetType(), "SPCRealTimeMonitoringScript", startupScript, true);
            }

            RenderToClient = true;
        }

        public void setQueryParamsString()
        {
            SPCTxnDataParamsChanges[] SPCTxnDataParams = _gridSPCParamsGrid.Data as SPCTxnDataParamsChanges[];
            string SPCQueryParams = "";

            if (SPCTxnDataParams.Length != 0)
            {
                foreach (SPCTxnDataParamsChanges item in SPCTxnDataParams)
                {
                    if (SPCQueryParams != "")
                        SPCQueryParams = SPCQueryParams + ";";
                    SPCQueryParams = SPCQueryParams + item.ParamName + ":" + item.ParamValue;
                }
                _txtQueryParams.Data = SPCQueryParams;
            }
        }

        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            if (action != null)
                switch (action.Parameters)
                {
                    case "ExecuteSPC":
                        e.Result = ExecuteSPC();
                        if (e.Result.IsSuccess)
                        {
                            StartLiveMonitoring("ON");
                            //getServiceInputData();
                            setQueryParamsString();
                        }
                        break;
                    case "Reset":
                        Page.ClearValues();
                        StopLiveMonitoring();
                        break;
                }
        } // WebPartCustomAction

        //public void getServiceInputData()
        //{
        //	setQueryParamsString();
        //	_txtInputData.Data = _ndoSPCSetup.Data.ToString() + ',' + Page.SessionDataContract.GetValueByName("User") + ',' + _txtQueryParams.Data.ToString(); 
        //}

        public ResultStatus ExecuteSPC()
        {
            ResultStatus ReturnResult = new ResultStatus();

            ss_SPCRealTime inputData = new ss_SPCRealTime
            {
                SPCSetup = _ndoSPCSetup.Data as NamedObjectRef,
                Employee = Page.SessionDataContract.GetValueByName("User") as NamedObjectRef
            };

            inputData.SPCParams = new SPCTxnDataParamsChanges[_gridSPCParamsGrid.TotalRowCount];
            for (int x = 0; x < _gridSPCParamsGrid.TotalRowCount; x++)
            {
                string strRowId = _gridSPCParamsGrid.GridContext.GetRowId(x);
                _gridSPCParamsGrid.GridContext.SelectRow(strRowId, true);

                inputData.SPCParams[x] = new OM.SPCTxnDataParamsChanges();
                inputData.SPCParams[x].ParamName = _gridSPCParamsGrid.GridContext.GetCell(strRowId, "ParamName").ToString();
                if (_gridSPCParamsGrid.GridContext.GetCell(strRowId, "ParamValue") != null)
                    inputData.SPCParams[x].ParamValue = _gridSPCParamsGrid.GridContext.GetCell(strRowId, "ParamValue").ToString();
                else
                    inputData.SPCParams[x].ParamValue = "";
            }

            ss_SPCRealTime_Info info = new ss_SPCRealTime_Info
            {
                SPCTxnDataList = new SPCTxnData_Info
                {
                    SPCFailureAction = FieldInfoUtil.RequestValue(),
                    SPCResult = FieldInfoUtil.RequestValue(),
                    SPCResultFilename = FieldInfoUtil.RequestValue(),
                    FailureDocumentSet = FieldInfoUtil.RequestValue(),
                    SPCErrorMessage = FieldInfoUtil.RequestValue(),
                    ChartHeight = FieldInfoUtil.RequestValue(),
                    ChartWidth = FieldInfoUtil.RequestValue(),
                    Name = FieldInfoUtil.RequestValue()
                }
            };

            UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
            ss_SPCRealTimeService serv = new ss_SPCRealTimeService(profile);
            ss_SPCRealTime_Result result = null;
            ResultStatus resultStatus = serv.ExecuteTransaction(inputData, new ss_SPCRealTime_Request { Info = info }, out result);

            ReturnResult = resultStatus;
            if (ReturnResult.Message != null)
            {
                if (ReturnResult.Message.ToUpper().Contains("ERROR"))
                {
                    ReturnResult.IsSuccess = false;
                    ReturnResult.ExceptionData = new ExceptionDataType();
                    ReturnResult.ExceptionData.Description = ReturnResult.Message;
                }
            }

            if (resultStatus.IsSuccess)
            {
                if (result.Value.SPCTxnDataList != null)
                {
                    if (result.Value.SPCTxnDataList.Length >= 0)
                    {

                        SPCTxnData oSPCData = result.Value.SPCTxnDataList[0];

                        if (oSPCData.SPCErrorMessage != null)
                        {
                            ReturnResult = new ResultStatus(oSPCData.SPCErrorMessage.ToString(), false);
                        }
                        else
                        {
                            DisplayValues(result.Value);
                            _txtSPCFailureActionField.Data = oSPCData.SPCFailureAction;
                            _txtSPCResultField.Data = oSPCData.SPCResult;
                            _txtNameField.Data = oSPCData.Name;
                            _txtSPCResultFileNameField.Data = oSPCData.SPCResultFilename;
                            _txtCPHeight.Data = oSPCData.ChartHeight;
                            _txtCPWidth.Data = oSPCData.ChartWidth;
                        }


                        if (_txtSPCResultFileNameField.Data != null)
                        {
                            DisplayChart();

                            // check for alert messages
                            string[] sAlertMessages;
                            string sCompletionMessage;
                            bool bAlertMsg = SEMI.AppCode.UIUtility.AlertMessagesAvailable(resultStatus.Message, out sAlertMessages, out sCompletionMessage);

                            DataPacket SS_DataPacket = new DataPacket();
                            if (bAlertMsg)
                                SS_DataPacket.AlertMessages = sAlertMessages;

                            _envDataEnvelop.SS_DataPacket = SS_DataPacket;

                            ReturnResult = new ResultStatus(sCompletionMessage, resultStatus.IsSuccess);
                        } // if (_txtSPCResultFileNameField.Data != null)                                
                    } // if (result.Value.SPCTxnDataList.Length >= 0)
                } //  if (result.Value.SPCTxnDataList != null)
            } // if (resultStatus.IsSuccess)
            return ReturnResult;
        }

        public void DisplayChart()
        {
            // set the data envelop data to pass to the popup;            
            SPCTxnData oSPCData = new SPCTxnData();
            oSPCData.Name = _txtNameField.Data.ToString();
            oSPCData.SPCResult = _txtSPCResultField.Data != null ? _txtSPCResultField.Data.ToString() : "";
            oSPCData.SPCSetup = _ndoSPCSetup.Data != null ? new NamedObjectRef(_ndoSPCSetup.Data.ToString()) : null;
            oSPCData.SPCResultFilename = _txtSPCResultFileNameField.Data != null ? _txtSPCResultFileNameField.Data.ToString() : "";
            oSPCData.ChartHeight = _txtCPHeight.Data != null ? int.Parse(_txtCPHeight.Data.ToString()) : 0;
            oSPCData.ChartWidth = _txtCPWidth.Data != null ? int.Parse(_txtCPWidth.Data.ToString()) : 0;

            SEMI.AppCode.Services.SPCTxn.SPCTxnDataSet SPCTxnDataSet = new SEMI.AppCode.Services.SPCTxn.SPCTxnDataSet();
            SPCTxnDataSet.AddDataListItem(oSPCData);

            DataPacket SS_DataPacket = new DataPacket();
            if (_envDataEnvelop.SS_DataPacket != null)
                SS_DataPacket = _envDataEnvelop.SS_DataPacket;

            SS_DataPacket.SPCTxnDataSet = SPCTxnDataSet;
            _envDataEnvelop.SS_DataPacket = SS_DataPacket;

            UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[1];
            objLinks[0] = new UIComponentDataContractLink();
            objLinks[0].SourceMember = "envelopMainOutDM";
            objLinks[0].TargetMember = "envelopPopupInDM";

            UIComponentDataContractReturnLink[] objReturnLinks = new UIComponentDataContractReturnLink[1];
            objReturnLinks[0] = new UIComponentDataContractReturnLink();
            objReturnLinks[0].SourceMember = "envelopPopupOutDM";
            objReturnLinks[0].TargetMember = "envelopMainInDM";

            // add the data contract member        
            if (Page.DataContract.GetValueByName("envelopPopupInDM") != null)
                _envDataEnvelop.SS_DataPacket = Page.DataContract.GetValueByName("envelopPopupInDM") as DataPacket;

            LoadSPCTxnDataList(SPCTxnDataSet);

            if (int.Parse(_txtSPCTxnDataIndex.Data.ToString()) > -1)
            {
                SetChart(int.Parse(_txtSPCTxnDataIndex.Data.ToString()));
            }
        }// DisplayChart

        public void LoadSPCTxnDataList(SEMI.AppCode.Services.SPCTxn.SPCTxnDataSet oSPCDataSet)
        {
            DataTable dtSPCTxnData = new DataTable();
            DataRow drSPCTxnData;
            DataColumn dc;
            dc = new DataColumn("SPCTxnDataName");
            dtSPCTxnData.Columns.Add(dc);
            _gridSPCTxnDataList.AddField(dc);
            dc = new DataColumn("SPCSetupName");
            dtSPCTxnData.Columns.Add(dc);
            _gridSPCTxnDataList.AddField(dc);
            dc = new DataColumn("SPCResult");
            dtSPCTxnData.Columns.Add(dc);
            _gridSPCTxnDataList.AddField(dc);
            dc = new DataColumn("SPCResultFilename");
            dtSPCTxnData.Columns.Add(dc);
            _gridSPCTxnDataList.AddField(dc);
            dc = new DataColumn("ChartHeight");
            dtSPCTxnData.Columns.Add(dc);
            _gridSPCTxnDataList.AddField(dc);
            dc = new DataColumn("ChartWidth");
            dtSPCTxnData.Columns.Add(dc);
            _gridSPCTxnDataList.AddField(dc);
            dc = new DataColumn("ParamsIndexString");
            dtSPCTxnData.Columns.Add(dc);
            _gridSPCTxnDataList.AddField(dc);

            foreach (SPCTxnData oData in oSPCDataSet.DataList)
            {
                drSPCTxnData = dtSPCTxnData.NewRow();
                drSPCTxnData["SPCTxnDataName"] = oData.Name;
                drSPCTxnData["SPCResult"] = oData.SPCResult;
                drSPCTxnData["SPCResultFilename"] = oData.SPCResultFilename;
                drSPCTxnData["ChartHeight"] = oData.ChartHeight;
                drSPCTxnData["ChartWidth"] = oData.ChartWidth;
                drSPCTxnData["ParamsIndexString"] = ""; //not used currently

                dtSPCTxnData.Rows.Add(drSPCTxnData);
            }

            // bind dataset to the grid
            _gridSPCTxnDataList.ClearData();
            (_gridSPCTxnDataList.GridContext as BoundContext).Data = dtSPCTxnData;
            _gridSPCTxnDataList.BoundContext.LoadData();
            CamstarWebControl.SetRenderToClient(_gridSPCTxnDataList);

            // set the 1st item in the grid to display
            if ((oSPCDataSet.Count > 0))
            {
                _txtSPCTxnDataIndex.Data = "0";
            }
        } // LoadSPCTxnDataList

        public bool SetChart(int intSPCTxnDataIndex)
        {
            _txtDataPointID.Style["display"] = "none";
            _txtDataPointName.Style["display"] = "none";
            _AnnotateButton.Style["display"] = "none";
            _txtIsAnnotationRequired.Style["display"] = "none";

            string strSPCResultFilename;
            string strChartHeight;
            string strChartWidth;
            string strSPCSetupName;
            string strSPCResult;
            string strSPCTxnDataName;

            DataTable dtTxnDataList = _gridSPCTxnDataList.BoundContext.Data as DataTable;
            DataRow drTxnDataItem = dtTxnDataList.Rows[intSPCTxnDataIndex];
            strSPCTxnDataName = drTxnDataItem[0].ToString();
            strSPCSetupName = drTxnDataItem[1].ToString();
            strSPCResult = drTxnDataItem[2].ToString();
            strSPCResultFilename = drTxnDataItem[3].ToString();
            strChartHeight = drTxnDataItem[4].ToString();
            strChartWidth = drTxnDataItem[5].ToString();

            string strHTML = "";
            string strChartHTML = "";
            strChartHTML = ReadChart(strSPCResultFilename);

            if (String.IsNullOrEmpty(strChartHTML))
            {
                if (!String.IsNullOrEmpty(strSPCResultFilename))
                {
                    DisplayMessage(new ResultStatus("Unable to read chart data at :" + strSPCResultFilename, false));
                }
                else
                {
                    DisplayMessage(new ResultStatus("No chart data to display. See SPC Error Message in SPC Records for details.", false));
                }
                return false;
            }

            if (!String.IsNullOrEmpty(strChartHTML) && strSPCResultFilename.ToLower().IndexOf("spcchart") > -1)
            {
                strChartHTML = strChartHTML.Substring(strChartHTML.IndexOf("<table"));
                ScriptManager.RegisterStartupScript(this, GetType(), "ExecuteSPC", "__page.set_eventArgument('ExecuteSPCPostBackArgument');", false);
            }
            strHTML = "<div>" + strChartHTML + "</div>";

            //replace the current jsTips.js src link to point the one in ..Scripts/Statit.js
            string sCurrentURL = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + HttpContext.Current.Request.ApplicationPath;
            string sNewScriptLink = "<script src=\"" + sCurrentURL + "/Scripts/Statit.js\"></script>";
            int iScriptStart = strHTML.ToUpper().IndexOf("<SCRIPT SRC");
            int iScriptStop = strHTML.ToUpper().IndexOf("</SCRIPT>");

            if ((iScriptStart > 0) && (iScriptStop > iScriptStart))
            {
                string sScriptLink = strHTML.Substring(iScriptStart, (iScriptStop - iScriptStart) + 9);
                int iJSFileIndex = sScriptLink.ToUpper().IndexOf("JSTIPS.JS");
                if (iJSFileIndex > 0)
                {
                    strHTML = strHTML.Replace(sScriptLink, sNewScriptLink);
                }
            }

            // set the chart panel height and width
            LiteralControl chartLiteral = new LiteralControl(strHTML);
            _pnlChartPanel.Width = (int.Parse(strChartWidth) + 25);

            _pnlChartPanel.ScrollBars = System.Web.UI.WebControls.ScrollBars.Auto;
            _pnlChartPanel.Height = (int.Parse(strChartHeight) + 5);
            _pnlChartPanel.Controls.Clear();
            _pnlChartPanel.Controls.Add(chartLiteral);

            CamstarWebControl.SetRenderToClient(_pnlChartPanel);
            return true;
        } // SetChart

        public string ReadChart(string strFileName)
        {
            try
            {
                //This function reads the HTML that defines a SPC Chart that was Statit generated
                //It must be run before placing the chart into the page's panel or placeholder control
                System.Net.WebClient objWebClient = new System.Net.WebClient();
                System.IO.Stream objStream;


                objStream = objWebClient.OpenRead(strFileName);
                System.IO.StreamReader objStreamReader = new System.IO.StreamReader(objStream);
                string strContents = objStreamReader.ReadToEnd();
                objStreamReader.Close();

                return strContents;
            }
            catch (Exception ex)
            {
                return "";
            }
        } // ReadChart        

        public void PopupAnnotation()
        {
            Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
            objAction.PageName = "SS_SPCChartsDataValuesPopupVP";

            objAction.FrameLocation = new UIFloatingPageLocation();
            objAction.FrameLocation.Width = int.Parse(_txtDVPopupWidth.Data.ToString()); //750
            objAction.FrameLocation.Height = int.Parse(_txtDVPopupHeight.Data.ToString()); //580;

            UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[6];
            objLinks[0] = new UIComponentDataContractLink();
            objLinks[0].SourceMember = "dataPointNameDM";
            objLinks[0].TargetMember = "dataPointNameDM";

            objLinks[1] = new UIComponentDataContractLink();
            objLinks[1].SourceMember = "dataPointIDDM";
            objLinks[1].TargetMember = "dataPointIDDM";

            objLinks[2] = new UIComponentDataContractLink();
            objLinks[2].SourceMember = "SPCTxnDataNameDM";
            objLinks[2].TargetMember = "SPCTxnDataNameDM";

            objLinks[3] = new UIComponentDataContractLink();
            objLinks[3].SourceMember = "envelopPopupOutDM";
            objLinks[3].TargetMember = "envelopSubPopupInDM";

            objLinks[4] = new UIComponentDataContractLink();
            objLinks[4].SourceMember = "SPCSetupNameDM";
            objLinks[4].TargetMember = "SPCSetupNameDM";

            objLinks[5] = new UIComponentDataContractLink();
            objLinks[5].SourceMember = "IsAnnotationRequiredDM";
            objLinks[5].TargetMember = "IsAnnotationRequiredDM";

            objAction.DataContractMap = new UIComponentDataContractMap();
            objAction.DataContractMap.Links = objLinks;

            UIComponentDataContractReturnLink[] objReturnLinks = new UIComponentDataContractReturnLink[1];
            objReturnLinks[0] = new UIComponentDataContractReturnLink();
            objReturnLinks[0].SourceMember = "envelopSubPopupOutDM";
            objReturnLinks[0].TargetMember = "envelopPopupInFromSubPopupDM";
            objAction.DataContractReturnMap = new UIComponentDataContractReturnMap();
            objAction.DataContractReturnMap.ReturnLinks = objReturnLinks;

            Page.ActionDispatcher.ExecuteAction(objAction);
        } // PopupAnnotation

        private void AddDataEnvelopDataMember()
        {
            int intConfiguredDataMemberCount = 0;

            if (Page.DataContract != null)
            {
                if (Page.DataContract.DataMembers != null)
                    intConfiguredDataMemberCount = Page.DataContract.DataMembers.Length;
            }
            else
                Page.DataContract = new UIComponentDataContract();

            // manually add the dataContractMember since the custom control's property does not show up at design time
            UIComponentDataMember[] objPageDataMembers = new UIComponentDataMember[intConfiguredDataMemberCount + 3];
            int intDMIndex = 0;

            if (Page.DataContract.DataMembers != null)
            {
                foreach (UIComponentDataMember objDM in Page.DataContract.DataMembers)
                {
                    objPageDataMembers[intDMIndex] = new UIComponentDataMember();
                    objPageDataMembers[intDMIndex] = objDM;
                    intDMIndex++;
                }
            }

            // add the new envelop data member
            objPageDataMembers[intDMIndex] = new UIComponentDataMember();
            objPageDataMembers[intDMIndex].Key = "ChartAreaWP.Envelop";
            objPageDataMembers[intDMIndex].Name = "envelopPopupOutDM";
            objPageDataMembers[intDMIndex].ConnectionType = DataMemberConnectionType.Control;
            objPageDataMembers[intDMIndex].Property = "SS_DataPacket";
            intDMIndex++;

            // add the new envelop data member
            objPageDataMembers[intDMIndex] = new UIComponentDataMember();
            objPageDataMembers[intDMIndex].Key = "ChartAreaWP.Envelop";
            objPageDataMembers[intDMIndex].Name = "envelopPopupInDM";
            objPageDataMembers[intDMIndex].ConnectionType = DataMemberConnectionType.Control;
            objPageDataMembers[intDMIndex].Property = "SS_DataPacket";
            intDMIndex++;

            // add the new envelop data member
            objPageDataMembers[intDMIndex] = new UIComponentDataMember();
            objPageDataMembers[intDMIndex].Key = "ChartAreaWP.Envelop";
            objPageDataMembers[intDMIndex].Name = "envelopPopupInFromSubPopupDM";
            objPageDataMembers[intDMIndex].ConnectionType = DataMemberConnectionType.Control;
            objPageDataMembers[intDMIndex].Property = "SS_DataPacket";

            Page.DataContract.DataMembers = objPageDataMembers;
        } // AddDataEnvelopDataMember		

        public void RetrieveSPCRecords()
        {
            FrameworkSession currentSession = FrameworkManagerUtil.GetFrameworkSession(this.Page.Session);
            QueryUtil QueryService = new QueryUtil(currentSession.CurrentUserProfile);
            RecordSet queryResult = null;
            ResultStatus resultStatus = null;
            QueryOptions queryOption = new QueryOptions();
            QueryParameter[] queryParams = new QueryParameter[12];

            queryOption.QueryType = Camstar.WCF.ObjectStack.QueryType.System; /// querytype = system
			queryOption.StartRow = 1;// Start Row;
            queryOption.RowSetSize = 10000;// Row Set Size;

            queryParams[0] = new QueryParameter();
            queryParams[0].Name = "SPCSetupName";
            queryParams[0].Value = _ndoSPCSetup.Data != null ? (_ndoSPCSetup.Data.ToString() != "" ? _ndoSPCSetup.Data.ToString() : "%") : "%";


            queryParams[1] = new QueryParameter();
            queryParams[1].Name = "CDOName";
            queryParams[1].Value = _txtCDOName.Data != null ? (_txtCDOName.Data.ToString() != "" ? _txtCDOName.Data.ToString() : "%") : "%";

            queryParams[2] = new QueryParameter();
            queryParams[2].Name = "ContainerName";
            queryParams[2].Value = _txtContainer.Data != null ? (_txtContainer.Data.ToString() != "" ? _txtContainer.Data.ToString() : "%") : "%";

            queryParams[3] = new QueryParameter();
            queryParams[3].Name = "ResourceName";
            queryParams[3].Value = ndoResource.Data != null ? (ndoResource.Data.ToString() != "" ? ndoResource.Data.ToString() : "%") : "%";

            queryParams[4] = new QueryParameter();
            queryParams[4].Name = "SPCResult";
            queryParams[4].Value = "%";

            queryParams[5] = new QueryParameter();
            queryParams[5].Name = "SPCFailureAction";
            queryParams[5].Value = "%";

            queryParams[6] = new QueryParameter();
            queryParams[6].Name = "STARTROWNUM";
            queryParams[6].Value = "1";

            queryParams[7] = new QueryParameter();
            queryParams[7].Name = "STOPROWNUM";
            queryParams[7].Value = "10000";

            queryParams[8] = new QueryParameter();
            queryParams[8].Name = "ss_RelatedEquipmentName";
            queryParams[8].Value = ndoProcessEquipment.Data != null ? (ndoProcessEquipment.Data.ToString() != "" ? ndoProcessEquipment.Data.ToString() : "%") : "%";

            queryParams[9] = new QueryParameter();
            queryParams[9].Name = "ss_RelatedRecipeName";
            queryParams[9].Value = rdoProcessRecipe.Data != null ? (rdoProcessRecipe.Data.ToString() != "" ? rdoProcessRecipe.Data.ToString() : "%") : "%";

            queryParams[10] = new QueryParameter();
            queryParams[10].Name = "ss_SPCMatrixContext";
            queryParams[10].Value = txtSPCMatrixContext.Data != null ? (txtSPCMatrixContext.Data.ToString() != "" ? txtSPCMatrixContext.Data.ToString() : "%") : "%";

            queryParams[11] = new QueryParameter();
            queryParams[11].Name = "ss_SPCMatrixID";
            queryParams[11].Value = _dllSPCMatrix.Data != null ? (_dllSPCMatrix.Data.ToString() != "" ? _dllSPCMatrix.Data.ToString() : "%") : "%"; ;

            QueryService.Execute("_SPCRecordsRealTime", queryParams, queryOption, ref queryResult, ref resultStatus);

            if (resultStatus.IsSuccess)
            {
                if (queryResult.Rows != null)
                {
                    DisplayMessage(new ResultStatus("Records found", true));
                    _gridSPCParamsGrid.ClearData();
                    LoadSPCTxnDataParam(queryResult.Rows[0].Values[0].ToString());
                    _btnExecuteSPC.Enabled = true;
                    _btnSearch.Enabled = false;
                }
                else
                {
                    DisplayMessage(new ResultStatus("No Records found", false));
                }
            }
            else
            {
                DisplayMessage(resultStatus);
            }
        } // RetrieveSPCRecords

        public void LoadSPCTxnDataParam(string SPCTxnDataName)
        {
            if (SPCTxnDataName != null)
            {
                SPCTxnDataMaint inputData = new SPCTxnDataMaint();
                inputData.ObjectToChange = new NamedObjectRef();

                inputData.ObjectToChange.Name = SPCTxnDataName;// _gridSPCRecords_Results.BoundContext.GetSelectedCell("SPCTxnDataName").ToString();

                SPCTxnDataMaint_Info info = new SPCTxnDataMaint_Info
                {
                    ObjectChanges = new SPCTxnDataChanges_Info
                    {
                        Params = new SPCTxnDataParamsChanges_Info
                        {
                            ParamName = FieldInfoUtil.RequestValue(),
                            ParamValue = FieldInfoUtil.RequestValue()
                        },
                    }
                };

                UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
                SPCTxnDataMaintService svc = new SPCTxnDataMaintService(profile);
                SPCTxnDataMaint_Result result = null;
                ResultStatus resultStatus = svc.Load(inputData, new SPCTxnDataMaint_Request { Info = info }, out result);

                if (resultStatus.IsSuccess)
                {
                    SPCTxnDataChanges objectChanges = result.Value.ObjectChanges as SPCTxnDataChanges;

                    if (objectChanges.Params != null)
                        if (objectChanges.Params.ToList().Find(a => a.ParamName.Value.ToString() == "CDONAME") != null)
                            objectChanges.Params.ToList().Find(a => a.ParamName.Value.ToString() == "CDONAME").ParamValue = "ss_SPCRealTime";

                    (_gridSPCParamsGrid.GridContext as ItemDataContext).Data = objectChanges.Params;
                    _gridSPCParamsGrid.BoundContext.LoadData();
                }
            }
        }
    }
}



