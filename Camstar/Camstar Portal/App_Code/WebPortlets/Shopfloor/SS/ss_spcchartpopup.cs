/* Copyright 2025 Siemens */
using System;
using System.Collections.Generic;
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
using Camstar.WebPortal.Personalization;
using SEMI.AppCode;

/// <summary>
/// Summary description for SS_SPCChartPopup
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_SPCChartPopup : MatrixWebPart
    {
        protected CWC.PagePanel _pnlChartPanel { get { return Page.FindCamstarControl("ChartPanel") as CWC.PagePanel; } }
        protected JQDataGrid _gridSPCTxnDataList { get { return Page.FindCamstarControl("TxnDataList") as JQDataGrid; } }
        protected CWC.TextBox _txtDataPointID { get { return Page.FindCamstarControl("DataPointID") as CWC.TextBox; } }
        protected CWC.TextBox _txtDataPointName { get { return Page.FindCamstarControl("DataPointName") as CWC.TextBox; } }
        protected CWC.TextBox _txtFailureDocumentSet { get { return Page.FindCamstarControl("FailureDocumentSet") as CWC.TextBox; } }
        protected CWC.TextBox _txtSPCResult { get { return Page.FindCamstarControl("SPCResult") as CWC.TextBox; } }
        protected CWC.TextBox _txtSPCTxnDataName { get { return Page.FindCamstarControl("SPCTxnDataName") as CWC.TextBox; } }
        protected CWC.TextBox _txtSPCTxnDataIndex { get { return Page.FindCamstarControl("SPCTxnDataIndex") as CWC.TextBox; } }
        protected DataEnvelopControl _envDataEnvelop { get { return Page.FindCamstarControl("Envelop") as DataEnvelopControl; } }
        protected CWC.Button _AnnotateButton { get { return Page.FindCamstarControl("btnAnnotate") as CWC.Button; } }
        protected CWC.TextBox _txtDVPopupHeight { get { return Page.FindCamstarControl("DVPHeight") as CWC.TextBox; } }
        protected CWC.TextBox _txtDVPopupWidth { get { return Page.FindCamstarControl("DVPWidth") as CWC.TextBox; } }
        protected CWC.TextBox _txtSPCSetupName { get { return Page.FindCamstarControl("SPCSetupName") as CWC.TextBox; } }
        protected CWC.TextBox _txtIsAnnotationRequired { get { return Page.FindCamstarControl("IsAnnotationRequired") as CWC.TextBox; } }

        protected CWC.Button _btnPrevChart { get { return Page.FindCamstarControl("PrevChart") as CWC.Button; } }
        protected CWC.Button _btnNextChart { get { return Page.FindCamstarControl("NextChart") as CWC.Button; } }
        protected CWC.Label _lblCurrentChartLabel { get { return Page.FindCamstarControl("CurrentChartLabel") as CWC.Label; } }
        protected CWC.Label _lblTotalChartLabel { get { return Page.FindCamstarControl("TotalChartLabel") as CWC.Label; } }

        //-----------------------------------------
        //
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                AddDataEnvelopDataMember();

                // add the data contract member        
                if (Page.DataContract.GetValueByName("envelopPopupInDM") != null)
                    _envDataEnvelop.SS_DataPacket = Page.DataContract.GetValueByName("envelopPopupInDM") as DataPacket;

                LoadSPCTxnDataList();
                SetChartControl();
                if (int.Parse(_txtSPCTxnDataIndex.Data.ToString()) > -1)
                {
                    SetChart(int.Parse(_txtSPCTxnDataIndex.Data.ToString()));
                }

            }
            else
            {
                // detect if the popup page was closed
                if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument")
                {
                    OnDataValuePopupClose();
                }
            }
        } // OnLoad

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

        //-----------------------------------------
        //
        //-----------------------------------------
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
            objPageDataMembers[intDMIndex].Key = "BlankWP.Envelop";
            objPageDataMembers[intDMIndex].Name = "envelopPopupOutDM";
            objPageDataMembers[intDMIndex].ConnectionType = DataMemberConnectionType.Control;
            objPageDataMembers[intDMIndex].Property = "SS_DataPacket";
            intDMIndex++;

            // add the new envelop data member
            objPageDataMembers[intDMIndex] = new UIComponentDataMember();
            objPageDataMembers[intDMIndex].Key = "BlankWP.Envelop";
            objPageDataMembers[intDMIndex].Name = "envelopPopupInDM";
            objPageDataMembers[intDMIndex].ConnectionType = DataMemberConnectionType.Control;
            objPageDataMembers[intDMIndex].Property = "SS_DataPacket";
            intDMIndex++;

            // add the new envelop data member
            objPageDataMembers[intDMIndex] = new UIComponentDataMember();
            objPageDataMembers[intDMIndex].Key = "BlankWP.Envelop";
            objPageDataMembers[intDMIndex].Name = "envelopPopupInFromSubPopupDM";
            objPageDataMembers[intDMIndex].ConnectionType = DataMemberConnectionType.Control;
            objPageDataMembers[intDMIndex].Property = "SS_DataPacket";

            Page.DataContract.DataMembers = objPageDataMembers;
        } // AddDataEnvelopDataMember

        //-----------------------------------------
        //
        //-----------------------------------------
        public void LoadSPCTxnDataList()
        {
            //retrieve the dataEnvelopData
            SEMI.AppCode.Services.SPCTxn.SPCTxnDataSet oSPCDataSet = new SEMI.AppCode.Services.SPCTxn.SPCTxnDataSet(); ;
            oSPCDataSet = _envDataEnvelop.SS_DataPacket.SPCTxnDataSet as SEMI.AppCode.Services.SPCTxn.SPCTxnDataSet;

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

                if (oData.FailureDocumentSet != null)
                    _txtFailureDocumentSet.Data = oData.FailureDocumentSet.Name;

                _txtSPCResult.Data = oData.SPCResult;

                if (_txtSPCTxnDataName.Data == null)
                    _txtSPCTxnDataName.Data = oData.Name;

                if (oData.SPCSetup != null)
                    _txtSPCSetupName.Data = oData.SPCSetup.Name;

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
                _lblTotalChartLabel.Text = oSPCDataSet.Count.ToString();
            }
        } // LoadSPCTxnDataList

        //-----------------------------------------
        //
        //-----------------------------------------
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

        //-----------------------------------------
        //
        //-----------------------------------------
        public void PopupDocumentSetViewer()
        {
            Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
            objAction.PageName = "SS_DocumentSetPopupVP";

            objAction.FrameLocation = new UIFloatingPageLocation();
            objAction.FrameLocation.Width = int.Parse(_txtDVPopupWidth.Data.ToString()); //750
            objAction.FrameLocation.Height = int.Parse(_txtDVPopupHeight.Data.ToString()); //580;

            UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[1];
            objLinks[0] = new UIComponentDataContractLink();
            objLinks[0].SourceMember = "DocumentSetDM";
            objLinks[0].TargetMember = "DocumentSetDM";

            objAction.DataContractMap = new UIComponentDataContractMap();
            objAction.DataContractMap.Links = objLinks;

            Page.ActionDispatcher.ExecuteAction(objAction);
        } // PopupDocumentSetViewer

        //-----------------------------------------
        //
        //-----------------------------------------
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
            }
            strHTML = "<div>" + strChartHTML + "</div>";

            //replace the current jsTips.js src link to point the one in ..Scripts/Statit.js
            string sCurrentURL = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + HttpContext.Current.Request.ApplicationPath;
            string sNewScriptLink = "<script type='text/javascript' src=\"" + sCurrentURL + "/Scripts/Statit.js\"></script>";
            int iScriptStart = strHTML.ToUpper().IndexOf("<SCRIPT TYPE");
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

        //-----------------------------------------
        //
        //-----------------------------------------
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

        //-----------------------------------------
        //
        //-----------------------------------------
        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
                switch (action.Parameters)
                {
                    case "ClosePop":
                        ScriptManager.RegisterStartupScript(Page.Form, GetType(), "ClosePopup", "window.parent.__page._isTabContainerPage=null;window.parent.CloseFloatingFrame(true);", true);
                        break;
                    case "Next":
                        NextChartClick();
                        break;
                    case "Prev":
                        PrevChartClick();
                        break;
                }
        } // WebPartCustomAction

        //-----------------------------------------
        //
        //-----------------------------------------
        public void SetChartControl()
        {
            int intTotalChart = int.Parse(_lblTotalChartLabel.Text.ToString());
            int intCurrentChart = int.Parse(_txtSPCTxnDataIndex.Data.ToString()) + 1;
            _lblCurrentChartLabel.Text = intCurrentChart.ToString();

            if (intCurrentChart > 1)
                _btnPrevChart.Enabled = true;
            else
                _btnPrevChart.Enabled = false;

            if (intCurrentChart < intTotalChart)
                _btnNextChart.Enabled = true;
            else
                _btnNextChart.Enabled = false;
        } // SetChartControl

        //-----------------------------------------
        //
        //-----------------------------------------
        public void PrevChartClick()
        {
            int intCurrentIndex = int.Parse(_txtSPCTxnDataIndex.Data.ToString());
            intCurrentIndex--;
            _txtSPCTxnDataIndex.Data = intCurrentIndex.ToString();
            SetChart(intCurrentIndex);
            SetChartControl();
        } // PrevChartClick

        //-----------------------------------------
        //
        //-----------------------------------------
        public void NextChartClick()
        {
            int intCurrentIndex = int.Parse(_txtSPCTxnDataIndex.Data.ToString());
            intCurrentIndex++;
            _txtSPCTxnDataIndex.Data = intCurrentIndex.ToString();
            SetChart(intCurrentIndex);
            SetChartControl();
        } // NextChartClick

        //-----------------------------------------
        //
        //-----------------------------------------
        public void OnDataValuePopupClose()
        {
            ReplotSPCChart();
            SetChart(int.Parse(_txtSPCTxnDataIndex.Data != null ? _txtSPCTxnDataIndex.Data.ToString() : "0"));

            if (Page.DataContract.GetValueByName("envelopPopupInFromSubPopupDM") != null)
                _envDataEnvelop.SS_DataPacket = Page.DataContract.GetValueByName("envelopPopupInFromSubPopupDM") as DataPacket;

            if (_envDataEnvelop.SS_DataPacket.ResultStatusMessage != null)
                DisplayMessage(new ResultStatus(_envDataEnvelop.SS_DataPacket.ResultStatusMessage, true));
        } // OnDataValuePopupClose

        //-----------------------------------------
        //
        //-----------------------------------------
        public void ReplotSPCChart()
        {
            if (_txtSPCTxnDataName.Data != null)
            {
                UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
                SPCTxnDataMaintService objService = new SPCTxnDataMaintService(profile);

                SPCTxnDataMaint objServiceData = new SPCTxnDataMaint();
                SPCTxnDataMaint_Info objServiceData_Info = new SPCTxnDataMaint_Info();
                SPCTxnDataChanges objChanges = new SPCTxnDataChanges();
                SPCTxnDataChanges_Info objChanges_Info = new SPCTxnDataChanges_Info();
                SPCTxnDataMaint_Result result = null;

                OM.ResultStatus resultStatus = null;

                objServiceData.ObjectToChange = new NamedObjectRef(_txtSPCTxnDataName.Data.ToString().ToString());

                objService.BeginTransaction();
                objService.Load(objServiceData);

                objServiceData.ObjectChanges = objChanges;
                objChanges.Replot = true;

                objService.ExecuteTransaction(objServiceData);
                resultStatus = objService.CommitTransaction();

                if (resultStatus.IsSuccess)
                {
                    objServiceData_Info = new SPCTxnDataMaint_Info
                    {
                        ObjectChanges = new SPCTxnDataChanges_Info
                        {
                            Name = FieldInfoUtil.RequestValue(),
                            SPCResultFilename = FieldInfoUtil.RequestValue()
                        }
                    };

                    objChanges.Replot = false;
                    resultStatus = objService.Load(objServiceData, new SPCTxnDataMaint_Request { Info = objServiceData_Info }, out result);

                    if (resultStatus.IsSuccess)
                    {
                        SPCTxnDataChanges objectChanges = result.Value.ObjectChanges as SPCTxnDataChanges;
                        UpdateSPCTxnDataList(objectChanges.Name.ToString(), objectChanges.SPCResultFilename.ToString());
                    }
                }

            }
        } // ReplotSPCChart

        //-----------------------------------------
        //
        //-----------------------------------------
        public void UpdateSPCTxnDataList(string SPCTxnDataName, string SPCResultFileName)
        {
            DataTable dtSPCTxnData = new DataTable();
            DataColumn dc;
            dc = new DataColumn("SPCTxnDataName");
            dtSPCTxnData.Columns.Add(dc);
            dc = new DataColumn("SPCSetupName");
            dtSPCTxnData.Columns.Add(dc);
            dc = new DataColumn("SPCResult");
            dtSPCTxnData.Columns.Add(dc);
            dc = new DataColumn("SPCResultFilename");
            dtSPCTxnData.Columns.Add(dc);
            dc = new DataColumn("ChartHeight");
            dtSPCTxnData.Columns.Add(dc);
            dc = new DataColumn("ChartWidth");
            dtSPCTxnData.Columns.Add(dc);
            dc = new DataColumn("ParamsIndexString");
            dtSPCTxnData.Columns.Add(dc);

            dtSPCTxnData = _gridSPCTxnDataList.BoundContext.Data as DataTable;

            // get the index of the row to update
            int intIndex = -1;
            for (int x = 0; x < dtSPCTxnData.Rows.Count; x++)
            {
                if (dtSPCTxnData.Rows[x]["SPCTxnDataName"].ToString() == SPCTxnDataName)
                {
                    intIndex = x;
                    dtSPCTxnData.Rows[x]["SPCResultFilename"] = SPCResultFileName;
                    break;
                }
            }

            // bind dataset to the grid
            _gridSPCTxnDataList.ClearData();
            (_gridSPCTxnDataList.GridContext as BoundContext).Data = dtSPCTxnData;
            _gridSPCTxnDataList.BoundContext.LoadData();
            CamstarWebControl.SetRenderToClient(_gridSPCTxnDataList);
        } // UpdateSPCTxnDataList

    }
}



