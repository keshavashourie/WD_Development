// Copyright Siemens 2023
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebControls.PickLists;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.UI;
using OM = Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.SPCRealtimeMonitoring
{
    public class SPCRealtimeMonitoringPanelFilter : MatrixWebPart
    {
        #region Controls

        protected TitleControl TitleControl
        {
            get { return Page.FindCamstarControl("TitleControl") as TitleControl; }
        }

        protected Button GenerateCharts
        {
            get { return Page.FindCamstarControl("GenerateCharts") as Button; }
        }
        protected Button RefreshChart
        {
            get { return Page.FindCamstarControl("RefreshChart") as Button; }
        }
        
        protected Button ClearAllButton
        {
            get { return Page.FindCamstarControl("ClearAllButton") as Button; }
        }
    
        protected ContainerList ContainerForChart
        {
            get { return Page.FindCamstarControl("ContainerForChart") as ContainerList; }
        }

        protected NamedObject SPCChart
        {
            get { return Page.FindCamstarControl("SPCChart") as NamedObject; }
        }

        protected NamedObject SPCChartGroup
        {
            get { return Page.FindCamstarControl("SPCChartGroup") as NamedObject; }
        }

        protected RevisionedObject DataCollection
        {
            get { return Page.FindCamstarControl("DataCollection") as RevisionedObject; }
        }

        protected PagePanel SliderContainer
        {
            get { return Page.FindCamstarControl("SliderContainer") as PagePanel; }
        }
        
        protected PagePanel SpcChartPanel
        {
            get { return Page.FindCamstarControl("SpcChartPanel") as PagePanel; }
        }

        protected TextBox ContainerName
        {
            get { return Page.FindCamstarControl("ContainerName") as TextBox; }
        }

        protected TextBox ChartName
        {
            get { return Page.FindCamstarControl("ChartName") as TextBox; }
        }

        protected TextBox ChartGroupName
        {
            get { return Page.FindCamstarControl("ChartGroupName") as TextBox; }
        }

        protected TextBox DataCollectionName
        {
            get { return Page.FindCamstarControl("DataCollectionName") as TextBox; }
        }

        static List<OM.RevisionedObjectRef> DCList;
        static List<OM.NamedObjectRef> GroupsList;
        static List<OM.NamedObjectRef> ChartsList;

        #endregion

        #region Protected methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GenerateCharts.Click += GenerateCharts_Click;
            ContainerForChart.DataChanged += ContainerForChart_DataChanged;
            DataCollection.PickListPanelControl.PostProcessData += DCPickListPanelControl_PostProcessData;
            SPCChart.PickListPanelControl.PostProcessData += SPCChartPickListPanelControl_PostProcessData;
            SPCChartGroup.PickListPanelControl.PostProcessData += SPCChartGroupPickListPanelControl_PostProcessData;
            ClearAllButton.Click += ClearAllButton_Click;

            if (Page.EventTarget == "ctl00$WebPartManager$SPCChartsViewerWP$SPCChartControl$SaveChartButton")
                SaveChartButton();

            if (Page.IsPostBack && !Page.IsCallback && String.IsNullOrEmpty(Page.EventTarget))
            {
                System.Web.UI.Control control = null;
               
                System.Web.UI.Control foundControl;

                foreach (string ctl in Page.Request.Form)
                {
                    if (String.IsNullOrEmpty(ctl)) continue;
                    foundControl = Page.FindControl(ctl);

                    if (!(foundControl is System.Web.UI.WebControls.IButtonControl)) continue;
                        control = foundControl;                                          
                }

                var postbackControl = control == null ? string.Empty : control.ID;

                if (postbackControl != "ClearAllButton")
                    CreateChart(postbackControl);
            }
        }

       
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager.RegisterStartupScript(Page.Form, Page.Form.GetType(), "SearchLayoutFunctions", string.Format("SearchLayout_AddSlideoutTooglerSPCRealtimeMonitoring('{0}');", TitleControl.ClientID), true);
            ScriptManager mgr = ScriptManager.GetCurrent(this.Page);
            mgr.Scripts.Add(new ScriptReference("~/Scripts/ClientFramework/Camstar.WebPortal.FormsFramework.WebControls/SPC/libs/Highstock/highstock.js"));
        }
        #endregion

        #region Private methods
        private void ClearAllButton_Click(object sender, EventArgs e)
        {
            ContainerForChart.Data = null;
            SPCChart.Data = null;
            SPCChartGroup.Data = null;
            DataCollection.Data = null;
        }

        private void GenerateCharts_Click(object sender, EventArgs e)
        {            
            if (!String.IsNullOrEmpty(SPCChart.Text) || !String.IsNullOrEmpty(SPCChartGroup.Text) || !String.IsNullOrEmpty(DataCollection.Text) || !String.IsNullOrEmpty(ContainerForChart.Text))
            {
                ContainerName.Text = "";
                ChartName.Text = "";
                ChartGroupName.Text = "";
                DataCollectionName.Text = "";

                ContainerName.Text = ContainerForChart.Text;
                ChartName.Text = SPCChart.Text;
                ChartGroupName.Text = SPCChartGroup.Text;
                DataCollectionName.Text = DataCollection.Text;
            }
        }

        private void CreateChart(string postbackControl)
        {
            if (postbackControl == "GenerateCharts")
            {
                var validateResult = Page.ValidateInputData();

                if (!validateResult.IsSuccess)
                {
                    Page.DisplayWarning(validateResult.Message);
                    return;
                }
               
                Page.CurrentCallStack.Context.LocalSession["ctl00_WebPartManager_SPCChartsViewerWP_SPCChartControl_ChartViewer"] = null;
            }
            SPCChartControl chartControl = new SPCChartControl();
           
            chartControl.ID = "SPCChartControl";
            chartControl.SPCChartParams = new SPCChartData();

            chartControl.CssClass = "spcChartWrapper";
            chartControl.LabelPosition = LabelPositionType.Hidden;
            chartControl.SPCChartParams.TestMode = "TRUE";

            if (postbackControl == "GenerateCharts")
            {
                chartControl.SPCChartParams.SPCChartName = SPCChart.Text;
                chartControl.SPCChartParams.SPCChartGroupName = SPCChartGroup.Text;
                chartControl.SPCChartParams.DCDName = DataCollection.Text;
                chartControl.SPCChartParams.ContainerName = ContainerForChart.Text;                           
            }
            else
            {
                chartControl.SPCChartParams.SPCChartName = ChartName.Text;
                chartControl.SPCChartParams.SPCChartGroupName = ChartGroupName.Text;
                chartControl.SPCChartParams.DCDName = DataCollectionName.Text;
                chartControl.SPCChartParams.ContainerName = ContainerName.Text;
            }

            SpcChartPanel.Parent.Controls.Add(chartControl);

            RenderIntervalSlider();
        }

        private void RenderIntervalSlider()
        {
            SliderContainer.Controls.Clear();
            System.Web.UI.WebControls.Panel pnlSlider = new System.Web.UI.WebControls.Panel();
            pnlSlider.ID = "refreshSlider";
            var refreshLbl = FrameworkManagerUtil.GetLabelCache().GetLabelByName("Lbl_RefreshInterval");
            string sRefreshIntervalLabel = string.Format(@"<label id='refreshIntervalLabel' class='refreshIntervalLabelClass'>{0}:</label> " +
            "<input type='text' id='refreshIntervalValue' style='margin-bottom:5px;' class='refreshIntervalValueClass' readonly>", refreshLbl.Value);

            SliderContainer.Controls.Add(new LiteralControl(sRefreshIntervalLabel));
            SliderContainer.Controls.Add(pnlSlider);
            CamstarWebControl.SetRenderToClient(SliderContainer);

            if (!Page.ClientScript.IsStartupScriptRegistered("SPCRealTimeMonitoringScript"))
            {
                string startupScript = string.Format("InlineSPCRealTimeMonitoringScript('{0}');", SliderContainer.ClientID);
                ScriptManager.RegisterStartupScript(this, GetType(), "InlineSPCRealTimeMonitoringScript", startupScript, true);
            }

            RenderToClient = true;
        }
        private void SaveChartButton()
        {
            string groupNameForUDCD = null;
            if (!String.IsNullOrEmpty(DataCollectionName.Text))
            {
                groupNameForUDCD = getGroupNamebyUDCDname(DataCollectionName.Text);

            }
            string _strInlineSPCOutputFolder = @"Camstar\InlineSPC\Chart Output";
            string strSPCDailyFolder;
            string strNewSPCFolder;
            strSPCDailyFolder = string.Format("{0:yyyyMMdd}", DateTime.Now);

            var FileName = String.IsNullOrEmpty(ChartName.Text) && String.IsNullOrEmpty(ChartGroupName.Text) ? groupNameForUDCD : (String.IsNullOrEmpty(ChartName.Text) ? ChartGroupName.Text : ChartName.Text);
            strNewSPCFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData) + @"\" + _strInlineSPCOutputFolder + @"\" + strSPCDailyFolder;
            try
            {
                if (!Directory.Exists(strNewSPCFolder))
                {
                    Directory.CreateDirectory(strNewSPCFolder);
                }
            }
            catch (Exception )
            {
            }

            string relativeOutputFileDir = string.Format(@"{0}/{1}.htm", strSPCDailyFolder, FileName?.Trim());
            string physicalOutputFileDir = System.IO.Path.Combine(strNewSPCFolder);
            ScriptManager.RegisterStartupScript(Page.Form, Page.Form.GetType(), "SaveSPCChartHTML", string.Format("SaveSPCChartHTML('{0}');", relativeOutputFileDir), true);


        }


        private string getGroupNamebyUDCDname(string dcdName)
        {
            string groupNameForUDCD = null;
            if (dcdName != null)
            {
                var service = new DataCollectionDefMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                var cdo = new OM.DataCollectionDefMaint { ObjectToChange = new OM.RevisionedObjectRef(dcdName?.ToString()) };
                var request = new DataCollectionDefMaint_Request
                {
                    Info = new OM.DataCollectionDefMaint_Info
                    {
                        ObjectChanges = new OM.DataCollectionDefChanges_Info
                        {
                            SPCChartDefGroup = new OM.Info(true),
                            SPCChartDefEntries = new OM.Info(true)
                        }
                    }
                };

                OM.ResultStatus oRS = service.Load(cdo, request, out var oResult);
                if (oRS.IsSuccess && oResult.Value.ObjectChanges.SPCChartDefGroup != null)
                {
                    groupNameForUDCD = oResult.Value.ObjectChanges.SPCChartDefGroup.Name;
                }
            }
            return groupNameForUDCD;
        }
        private void SPCChartGroupPickListPanelControl_PostProcessData(object sender, DataRequestEventArgs e)
        {
            if (GroupsList != null)
            {
                var rs = e.Data as DataTable;
                List<DataRow> RowsToDelete = new List<DataRow>();
                foreach (var r in rs.Rows)
                {
                    var row = r as DataRow;
                    if (row != null && !GroupsList.Exists(m => m.Name == row["Name"].ToString()))
                    {
                        RowsToDelete.Add(row);
                    }
                }

                foreach (var dr in RowsToDelete)
                {
                    rs.Rows.Remove(dr);
                }
                e.Data = rs;
            }
        }

        private void SPCChartPickListPanelControl_PostProcessData(object sender, DataRequestEventArgs e)
        {
            if (ChartsList != null)
            {
                var rs = e.Data as DataTable;
                List<DataRow> RowsToDelete = new List<DataRow>();
                foreach (var r in rs.Rows)
                {
                    var row = r as DataRow;
                    if (row != null && !ChartsList.Exists(m => m.Name == row["Name"].ToString()))
                    {
                        RowsToDelete.Add(row);
                    }
                }

                foreach (var dr in RowsToDelete)
                {
                    rs.Rows.Remove(dr);
                }
                e.Data = rs;
            }
        }

        private void DCPickListPanelControl_PostProcessData(object sender, FormsFramework.WebControls.PickLists.DataRequestEventArgs e)
        {
            if (DCList != null)
            {
                var rs = e.Data as DataTable;
                List<DataRow> RowsToDelete = new List<DataRow>();
                foreach (var r in rs.Rows)
                {
                    var row = r as DataRow;
                    if (row != null && !DCList.Exists(m => m.Name == row["Name"].ToString()))
                    {
                        RowsToDelete.Add(row);
                    }
                }

                foreach (var dr in RowsToDelete)
                {
                    rs.Rows.Remove(dr);
                }
                e.Data = rs;
            }
        }

        private void ContainerForChart_DataChanged(object sender, EventArgs e)
        {
            DCList = GetDCList();
            ProcessChartGroupsAndEntries();
            DataCollection.ClearData();
            SPCChart.ClearData();
            SPCChartGroup.ClearData();
            DataCollection.RequestSelectionValues();
            SPCChart.RequestSelectionValues();
            SPCChartGroup.RequestSelectionValues();
        }

        private List<OM.RevisionedObjectRef> GetDCList()
        {
            List<OM.RevisionedObjectRef> dCList = new List<OM.RevisionedObjectRef>();
            if (ContainerForChart.Data != null)
            {
                var selectedContainer = ContainerForChart.Data.ToString();
                var prof = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                var svc = new WCF.Services.ContainerTxnService(prof);
                var data = new OM.ContainerTxn();

                data.Container = new OM.ContainerRef(selectedContainer);
                var req = new WCF.Services.ContainerTxn_Request
                {
                    Info = new OM.ContainerTxn_Info
                    {
                        CurrentContainerStatus = new OM.CurrentContainerStatus_Info
                        {
                            SpecName = new OM.Info(true),
                            SpecRevision = new OM.Info()
                        }
                    }
                };
                WCF.Services.ContainerTxn_Result res;
                var state = svc.Load(data, req, out res);
                if (state.IsSuccess)
                {
                    var qServ = new SpecMaintService(prof);
                    var request = new SpecMaint_Request
                    {
                        Info = new OM.SpecMaint_Info
                        {
                            UDCList = new OM.Info(true)
                        }
                    };
                    OM.SpecMaint dt;
                    if (res.Value.CurrentContainerStatus.SpecRevision != null)
                        dt = new OM.SpecMaint { ObjectToChange = new OM.RevisionedObjectRef(res.Value.CurrentContainerStatus.SpecName.Value, res.Value.CurrentContainerStatus.SpecRevision.Value) };
                    else
                        dt = new OM.SpecMaint { ObjectToChange = new OM.RevisionedObjectRef(res.Value.CurrentContainerStatus.SpecName.Value) };
                    var status = qServ.Load(dt, request, out var result);
                    if (status.IsSuccess && result.Value.UDCList != null)
                    {
                        foreach (var udc in result.Value.UDCList)
                        {
                            dCList.Add(udc);
                        }
                    }
                }
            }
            else
                return null;
            return dCList;
        }

        private void ProcessChartGroupsAndEntries()
        {
            if (DCList != null)
            {
                GroupsList = new List<OM.NamedObjectRef>();
                ChartsList = new List<OM.NamedObjectRef>();
                foreach (var dc in DCList)
                {
                    var service = new DataCollectionDefMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                    var cdo = new OM.DataCollectionDefMaint { ObjectToChange = new OM.RevisionedObjectRef(dc.Name.ToString()) };
                    var request = new DataCollectionDefMaint_Request
                    {
                        Info = new OM.DataCollectionDefMaint_Info
                        {
                            ObjectChanges = new OM.DataCollectionDefChanges_Info
                            {
                                SPCChartDefGroup = new OM.Info(true),
                                SPCChartDefEntries = new OM.Info(true)
                            }
                        }
                    };

                    OM.ResultStatus oRS = service.Load(cdo, request, out var oResult);
                    if (oRS.IsSuccess)
                    {
                        if (oResult.Value.ObjectChanges.SPCChartDefGroup != null)
                        {
                            GroupsList.Add(oResult.Value.ObjectChanges.SPCChartDefGroup);
                            var _srv = new SPCChartDefGroupMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                            var _cdo = new OM.SPCChartDefGroupMaint { ObjectToChange = new OM.NamedObjectRef(oResult.Value.ObjectChanges.SPCChartDefGroup.Name.ToString()) };
                            var _request = new SPCChartDefGroupMaint_Request
                            {
                                Info = new OM.SPCChartDefGroupMaint_Info
                                {
                                    ObjectChanges = new OM.SPCChartDefGroupChanges_Info
                                    {
                                        Groups = new OM.Info(true)
                                    }
                                }
                            };

                            OM.ResultStatus _oRS = _srv.Load(_cdo, _request, out var _oResult);
                            if (_oRS.IsSuccess)
                            {
                                if (_oResult.Value.ObjectChanges.Groups != null && _oResult.Value.ObjectChanges.Groups.Length > 0)
                                    GroupsList.AddRange(_oResult.Value.ObjectChanges.Groups);
                            }
                        }
                        if (oResult.Value.ObjectChanges.SPCChartDefEntries != null)
                            ChartsList.AddRange(oResult.Value.ObjectChanges.SPCChartDefEntries);
                    }
                }
            }
            else
            {
                GroupsList = null;
                ChartsList = null;
            }
        }
        #endregion
    }
}
