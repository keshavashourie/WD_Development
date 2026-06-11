// Copyright Siemens 2025 
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.WCFUtilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Personalization;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class CollectSamplingData : MatrixWebPart
    {
        protected virtual CWC.Button GenerateBtn
        {
            get
            {
                return Page.FindCamstarControl("GenerateButton") as CWC.Button;
            }
        }

        protected virtual JQDataGrid DataPointGrid
        {
            get
            {
                return Page.FindCamstarControl("DataPointGrid") as JQDataGrid;
            }
        }

        protected virtual CWC.TextBox TestedSamples
        {
            get
            {
                return Page.FindCamstarControl("TestedSamplesControl") as CWC.TextBox;
            }
        }
        protected virtual CWC.DropDownList SampleType
        {
            get
            {
                return Page.FindCamstarControl("SampleTypeControl") as CWC.DropDownList;
            }
        }

        protected virtual CWC.TextBox NumberofSamplesControl
        {
            get
            {
                return Page.FindCamstarControl("NumberofSamplesControl") as CWC.TextBox;
            }
        }

        protected virtual CWC.RevisionedObject SampleTest
        {
            get
            {
                return Page.FindCamstarControl("SampleTestControl") as CWC.RevisionedObject;
            }
        }

        protected virtual CWC.NamedObject RejectReasons
        {
            get
            {
                return Page.FindCamstarControl("CollectSamplingData_RejectReasons") as CWC.NamedObject;
            }
        }

        protected virtual JQDataGrid SamplingGrid
        {
            get
            {
                return Page.FindCamstarControl("SamplingDataGrid") as JQDataGrid;
            }
        }

        protected virtual JQDataGrid sampleTestGrid
        {
            get
            {
                return Page.FindCamstarControl("CollectSamplingData_ContainerSampleData") as JQDataGrid;
            }
        }

        protected virtual ToggleContainer MoveDetailsToggle
        {
            get
            {
                return Page.FindCamstarControl("MoveOnCompletionToggleContainer") as ToggleContainer;
            }
        }

        protected virtual CWC.NamedObject MoveStd_Resource
        {
            get
            {
                return Page.FindCamstarControl("MoveStd_Resource") as CWC.NamedObject;
            }
        }

        protected virtual CWC.TextBox MoveStd_Comments
        {
            get
            {
                return Page.FindCamstarControl("MoveStd_Comments") as CWC.TextBox;
            }
        }

        protected virtual CWC.RevisionedObject CollectionDef
        {
            get { return Page.FindCamstarControl("DCCollectionDef") as CWC.RevisionedObject; }
        }

        protected virtual CamstarPortal.WebControls.ShopFloorDCControl ParamDataControl
        {
            get { return Page.FindCamstarControl("ParamDataField") as CamstarPortal.WebControls.ShopFloorDCControl; }
        }

        protected bool PreventClearSavedIdentifier
        {
            get { return (bool)HttpContext.Current.Session["PreventClearSavedIdentifier"]; }
            set { HttpContext.Current.Session["PreventClearSavedIdentifier"] = value; }
        }

        public CollectSamplingData()
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GenerateBtn.Click += new EventHandler(GenerateBtn_Click);

            SampleTest.DataChanged += new EventHandler(SampleTest_DataChanged);

            var containerCtrl = Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid;
            containerCtrl.DataChanged += new EventHandler(containerCtrl_DataChanged);

            TestedSamples.Visible = false;

            if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument")
                Page.PortalContext.LocalSession["EsigPostback"] = true;
            else
            {
                Page.PortalContext.LocalSession["EsigPostback"] = false;
                Page.PortalContext.LocalSession["EsigEventArgument"] = null;
            }

            if ((bool)Page.PortalContext.LocalSession["EsigPostback"] && Page.PortalContext.LocalSession["EsigEventArgument"] != null)
                WebPartCustomAction(null, Page.PortalContext.LocalSession["EsigEventArgument"] as CustomActionEventArgs);

            //if ((Page.IsFloatingFrame && !Page.IsPostBack) || (!Page.IsPostBack && containerCtrl.Data is OM.ContainerRef && !(containerCtrl.Data as OM.ContainerRef).IsEmpty))
            //   containerCtrl_DataChanged(new Object(), new EventArgs());
        }



        protected virtual void SampleTest_DataChanged(object sender, EventArgs e)
        {
            ContainerListGrid contGrid = Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid;

            ClearDynamicGridData();

            SampleType.Data = new WCFObject(sampleTestGrid.SelectionData).GetValue(".SampleType");

            var labelCache = LabelCache.GetRuntimeCacheInstance();

            if (SampleType.Data != null && (SampleTypeEnum)SampleType.Data == SampleTypeEnum.Counted)
            {
                TestedSamples.LabelControl.Visible = true;
                TestedSamples.TextControl.Visible = true;
                TestedSamples.Visible = true;
                NumberofSamplesControl.Visible = false;
                GenerateBtn.Visible = false;
                RenderToClient = true;
                CamstarWebControl.SetRenderToClient(TestedSamples);

                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new Camstar.WCF.Services.CollectSamplingDataService(session.CurrentUserProfile);
                var servicedata = new OM.CollectSamplingData();

                servicedata.Container = (OM.ContainerRef)contGrid.Data;
                servicedata.SampleTest = (OM.RevisionedObjectRef)SampleTest.Data;

                var request = new Camstar.WCF.Services.CollectSamplingData_Request();
                var result = new Camstar.WCF.Services.CollectSamplingData_Result();
                var resultStatus = new OM.ResultStatus();

                request.Info = new OM.CollectSamplingData_Info();
                request.Info.RejectReasons = new OM.Info();
                request.Info.RejectReasons.RequestSelectionValues = true;

                resultStatus = service.Load(servicedata, request, out result);

                if (result != null && result.Environment != null && result.Environment.RejectReasons != null && result.Environment.RejectReasons.SelectionValues != null)
                {
                    //Build Table
                    JQDataGrid RejectGrid = Page.FindCamstarControl("SamplingDataGrid") as JQDataGrid;

                    //Build Columns
                    DataTable SamplingDataTable = new DataTable();

                    DataColumn SampleColumn = new DataColumn();
                    SampleColumn.ColumnName = RejectReasons.LabelControl.Text;
                    SampleColumn.Caption = RejectReasons.LabelControl.Text;
                    SampleColumn.DataType = typeof(string);

                    SamplingDataTable.Columns.Add(SampleColumn);

                    var labelQty = labelCache.GetLabelByName("Container_Qty");

                    SampleColumn = new DataColumn();
                    SampleColumn.ColumnName = "Qty";
                    SampleColumn.Caption = labelQty.Value;
                    SampleColumn.DataType = typeof(string);

                    SamplingDataTable.Columns.Add(SampleColumn);

                    string[] Textboxes = { "Qty" };

                    //Add Rows
                    int i = 0;
                    foreach (Row RejectReason in result.Environment.RejectReasons.SelectionValues.Rows)
                    {
                        SamplingDataTable.Rows.Add(RejectReason.Values[0].ToString());
                        i++;
                    }

                    RejectGrid.LabelText = SampleTest.TextEditControl.Text;
                    RejectGrid.TotalRowCount = i;
                    RejectGrid.Settings.RowsPerPage = i;
                    RejectGrid.Settings.VisibleRows = i;
                    RejectGrid.Height = System.Web.UI.WebControls.Unit.Percentage(100);

                    Type _dynamictype = WebClientPortal.GridUtility.RetrieveDynamicType(SampleTest.TextEditControl.Text);

                    if (_dynamictype != null)
                    {
                        WebClientPortal.GridUtility.resetDynamicType(SampleTest.TextEditControl.Text);
                    }

                    WebClientPortal.GridUtility.ItemListGrid_SetColumns(this, SamplingDataTable, "SamplingDataGrid", Textboxes, SampleTest.TextEditControl.Text);
                    WebClientPortal.GridUtility.ItemListGrid_BindDataTable(this, SamplingDataTable, ref RejectGrid, SampleTest.TextEditControl.Text);

                    RejectGrid.Visible = true;
                    CamstarWebControl.SetRenderToClient(RejectGrid);
                }
                else
                {
                    (Page as IForm).Page.DisplayMessage(new OM.ResultStatus(labelCache.GetLabelByName("Lbl_AQLNoReasonCode").Value, false));
                }
            }
            if (SampleType.Data != null && (SampleTypeEnum)SampleType.Data == SampleTypeEnum.Measured)
            {
                TestedSamples.Visible = false;
                NumberofSamplesControl.Visible = true;
                GenerateBtn.Visible = true;
                SamplingGrid.Visible = false;
                NumberofSamplesControl.TextControl.Text = "";
            }
        }


        protected virtual void GenerateBtn_Click(object sender, EventArgs e)
        {
            ClearDynamicGridData();

            var labelCache = LabelCache.GetRuntimeCacheInstance();

            if (SampleType.Data != null && (SampleTypeEnum)SampleType.Data == SampleTypeEnum.Measured)
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new Camstar.WCF.Services.CollectSamplingDataService(session.CurrentUserProfile);
                var servicedata = new OM.CollectSamplingData();

                servicedata.SampleTest = (OM.RevisionedObjectRef)SampleTest.Data;

                var request = new Camstar.WCF.Services.CollectSamplingData_Request();
                var result = new Camstar.WCF.Services.CollectSamplingData_Result();
                var resultStatus = new OM.ResultStatus();

                request.Info = new OM.CollectSamplingData_Info
                {
                    RequestValue = true,
                    CollectSamplingDataPoints = new CollectSamplingDataPoints_Info
                    {
                        RequestValue = true
                    }
                };

                resultStatus = service.GetDataPoints(servicedata, request, out result);

                if (resultStatus.IsSuccess)
                {

                    JQDataGrid DataPointGrid = Page.FindCamstarControl("SamplingDataGrid") as JQDataGrid;

                    DataTable SamplingDataTable = new DataTable();

                    GetAllLabelsInOneRequest(labelCache);

                    DataColumn SampleColumn = new DataColumn();
                    var labelSample = labelCache.GetLabelByName("ValueDataPoint_Sample");
                    SampleColumn.ColumnName = "SampleNo";
                    SampleColumn.Caption = labelSample.Value + "#";
                    SampleColumn.DataType = typeof(string);

                    SamplingDataTable.Columns.Add(SampleColumn);

                    string[] Textboxes = new string[result.Value.CollectSamplingDataPoints.Length];
                    Dictionary<string, string> ColumnDefaultValue = new Dictionary<string, string>();

                    int i = 0;
                    foreach (CollectSamplingDataPoints sampleDataPoint in result.Value.CollectSamplingDataPoints)
                    {
                        string strLimits = "";
                        if (sampleDataPoint.ShowLimits == true)
                        {
                            strLimits = sampleDataPoint.LowerLimit + " - " + sampleDataPoint.UpperLimit;
                        }

                        var labelUOM = labelCache.GetLabelByName("Container_UOM");
                        var labelLimits = labelCache.GetLabelByName("ValueDataPoint_Limits");
                        string strUOM = labelUOM.Value + ":" + sampleDataPoint.UOM ?? sampleDataPoint.UOM.Name;

                        DataColumn SampleDataColumn = new DataColumn();
                        SampleDataColumn = new DataColumn();
                        SampleDataColumn.ColumnName = sampleDataPoint.SampleDataPoint.Name + ":" + sampleDataPoint.SampleDataPoint.Revision;
                        SampleDataColumn.Caption = sampleDataPoint.SampleDataPoint.Name + "<br>" + strUOM + "<br>" + labelLimits.Value + " " + strLimits;
                        SampleDataColumn.DataType = typeof(string);

                        SamplingDataTable.Columns.Add(SampleDataColumn);

                        var sampleDataDefaultValue = sampleDataPoint.DefaultValue;
                        if (sampleDataDefaultValue == null)
                            sampleDataDefaultValue = string.Empty;

                        ColumnDefaultValue.Add(sampleDataPoint.SampleDataPoint.Name + ":" + sampleDataPoint.SampleDataPoint.Revision, sampleDataDefaultValue.ToString());

                        Textboxes[i] = sampleDataPoint.SampleDataPoint.Name + ":" + sampleDataPoint.SampleDataPoint.Revision;
                        i++;
                    }

                    int samplesToGenerate;
                    if (String.IsNullOrEmpty(NumberofSamplesControl.TextControl.Text))
                    {	//Set Default Value for number of Rows
                        samplesToGenerate = 10;
                    }
                    else
                    {
                        samplesToGenerate = Convert.ToInt32(NumberofSamplesControl.TextControl.Text);
                    }

                    for (i = 1; i <= samplesToGenerate; i++)
                    {
                        String sampleString = string.Concat(labelSample.Value + " ", i.ToString());

                        DataRow sampleRow = SamplingDataTable.NewRow();
                        sampleRow["SampleNo"] = sampleString;

                        foreach (KeyValuePair<string, string> item in ColumnDefaultValue)
                        {
                            sampleRow[item.Key] = item.Value.ToString();
                        }

                        SamplingDataTable.Rows.Add(sampleRow);
                    }

                    DataPointGrid.LabelText = SampleTest.TextEditControl.Text;
                    DataPointGrid.TotalRowCount = samplesToGenerate;
                    DataPointGrid.Settings.RowsPerPage = samplesToGenerate;
                    DataPointGrid.Settings.VisibleRows = samplesToGenerate;
                    DataPointGrid.Height = System.Web.UI.WebControls.Unit.Percentage(100);

                    Type _dynamictype = WebClientPortal.GridUtility.RetrieveDynamicType(SampleTest.TextEditControl.Text);

                    if (_dynamictype != null)
                    {
                        WebClientPortal.GridUtility.resetDynamicType(SampleTest.TextEditControl.Text);
                    }

                    WebClientPortal.GridUtility.ItemListGrid_SetColumns(this, SamplingDataTable, "SamplingDataGrid", Textboxes, SampleTest.TextEditControl.Text);
                    WebClientPortal.GridUtility.ItemListGrid_BindDataTable(this, SamplingDataTable, ref DataPointGrid, SampleTest.TextEditControl.Text);

                    DataPointGrid.Visible = true;
                    CamstarWebControl.SetRenderToClient(DataPointGrid);

                    TestedSamples.Visible = false;
                    RenderToClient = true;
                }
            }
        }

        private void GetAllLabelsInOneRequest(LabelCache labelCache)
        {
            var labels = new[]{
                new Label("ValueDataPoint_Sample"),
                new Label("Container_UOM"),
                new Label("ValueDataPoint_Limits")
            };
            labelCache.GetLabels(new LabelList(labels));
        }

        protected virtual void containerCtrl_DataChanged(object sender, EventArgs e)
        {
            ClearDynamicGridData();
            SampleTest.ClearData();
            MoveStd_Resource.ClearData();
            MoveStd_Comments.ClearData();

            if (sampleTestGrid != null)
            {
                sampleTestGrid.SelectedRowID = "";
            }

            SamplingGrid.Visible = false;
            ClearSamplingStatus();

            if (IsMoveOnCompletion())
                MoveDetailsToggle.Visible = true;
            else
                MoveDetailsToggle.Visible = false;

            CamstarWebControl.SetRenderToClient(MoveDetailsToggle);

        }


        protected virtual void ClearDynamicGridData()
        {
            SamplingGrid.ClearData();
            if ((SamplingGrid.GridContext as ItemDataContext).UnboundData != null)
                (SamplingGrid.GridContext as ItemDataContext).UnboundData.Clear();
            SamplingGrid.Data = null;

        }

        protected virtual void ClearSamplingStatus()
        {
            var requiredSamplesCtrl = Page.FindCamstarControl("RequiredSamplesControl") as CWC.InquiryControl;
            var inspectionlevelCtrl = Page.FindCamstarControl("InspectionLevelControl") as CWC.InquiryControl;
            var passedSamplesCtrl = Page.FindCamstarControl("PassedSamplesControl") as CWC.InquiryControl;
            var totalSamplesCtrl = Page.FindCamstarControl("TotalSamplesControl") as CWC.InquiryControl;
            var failedSamplesCtrl = Page.FindCamstarControl("FailedSamplesControl") as CWC.InquiryControl;
            var AQLLevelCtrl = Page.FindCamstarControl("AQLLevelControl") as CWC.InquiryControl;

            requiredSamplesCtrl.ClearData();
            inspectionlevelCtrl.ClearData();
            passedSamplesCtrl.ClearData();
            totalSamplesCtrl.ClearData();
            failedSamplesCtrl.ClearData();
            AQLLevelCtrl.ClearData();

        }

        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);

            if (status.IsSuccess)
            {
                ClearDynamicGridData();
                NumberofSamplesControl.TextControl.Text = "";
                SamplingGrid.Visible = false;

                //reload container to refresh the page state
                var containerCtrl = Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid;

                if (containerCtrl != null && containerCtrl.SelectionData != null)
                {
                    object temp = new Object();
                    temp = containerCtrl.SelectionData;

                    Page.ClearValues();
                    containerCtrl.Data = temp;

                }
            }
            else if(!status.IsSuccess && (SampleTypeEnum)SampleType.Data == SampleTypeEnum.Counted)
            {
                TestedSamples.Visible = true;
            }
        }

        public override bool PreExecute(Info serviceInfo, Service serviceData)
        {
            bool isSuccess = base.PreExecute(serviceInfo, serviceData);
            
            if (IsMoveOnCompletion())
            {
                // Override default assignment
                OM.CollectSamplingData samplingServiceData = (serviceData as OM.CollectSamplingData);

                samplingServiceData.DataCollectionDef = null;
                samplingServiceData.ParametricData = null;

                ESigCaptureUtil.CleanESigCaptureDM();
            }

            return isSuccess;
        }

        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);
            PreventClearSavedIdentifier = true;
            OM.CollectSamplingData svcData = serviceData as OM.CollectSamplingData;
            if (svcData != null)
            {
                JQDataGrid SamplingGrid = Page.FindCamstarControl("SamplingDataGrid") as JQDataGrid;

                if (SamplingGrid != null && SamplingGrid.Data != null && SampleType.Data != null)
                {
                    switch ((SampleTypeEnum)SampleType.Data)
                    {
                        case SampleTypeEnum.Measured:
                            string[] Colunnames = new String[SamplingGrid.Settings.Columns.Count() - 1];
                            int totalRows = SamplingGrid.GridContext.GetTotalRows();
                            int totalColumns = SamplingGrid.Settings.Columns.Count();
                            for (int y = 1; y < totalColumns; y++)
                            {

                                Colunnames[y - 1] = SamplingGrid.Settings.Columns[y].Name;
                            }


                            svcData.ServiceDetails = new CollectSamplingDataDetails[totalRows];
                            svcData.SamplesTested = totalRows;

                            for (int x = 0; x < totalRows; x++)
                            {
                                svcData.ServiceDetails[x] = new CollectSamplingDataDetails();
                                svcData.ServiceDetails[x].ListItemAction = OM.ListItemAction.Add;
                                svcData.ServiceDetails[x].SampleTest = (OM.RevisionedObjectRef)SampleTest.Data;
                                svcData.ServiceDetails[x].SamplingDataPointDetails = new OM.SamplingDataPointDetails[Colunnames.Length];

                                for (int z = 0; z < Colunnames.Length; z++)
                                {
                                    var delimeterPos = Colunnames[z].LastIndexOf(':');

                                    svcData.ServiceDetails[x].SamplingDataPointDetails[z] = new SamplingDataPointDetails
                                    {
                                        ListItemAction = OM.ListItemAction.Add,
                                        SampleDataPoint = new RevisionedObjectRef
                                        {
                                            Name = Colunnames[z].Substring(0, delimeterPos),
                                            Revision = Colunnames[z].Substring(delimeterPos + 1)
                                        },
                                        DataValue = SamplingGrid.GridContext.GetCell(x, Colunnames[z]).ToString()
                                    };
                                }
                            }

                            break;
                        case SampleTypeEnum.Counted:
                            OM.CollectSamplingData countedData = serviceData as OM.CollectSamplingData;
                            countedData.ServiceDetails = new CollectSamplingDataDetails[1];
                            if (String.IsNullOrEmpty(TestedSamples.TextControl.Text) == false)
                            {
                                countedData.SamplesTested = Convert.ToInt32(TestedSamples.TextControl.Text);
                            }


                            countedData.ServiceDetails[0] = new CollectSamplingDataDetails();
                            countedData.ServiceDetails[0].ListItemAction = OM.ListItemAction.Add;
                            countedData.ServiceDetails[0].SampleTest = (OM.RevisionedObjectRef)SampleTest.Data;
                            countedData.ServiceDetails[0].SamplingDataPointDetails = new OM.SamplingDataPointDetails[SamplingGrid.TotalRowCount];

                            for (int x = 0; x < SamplingGrid.GridContext.GetTotalRows(); x++)
                            {
                                countedData.ServiceDetails[0].SamplingDataPointDetails[x] = new SamplingDataPointDetails();
                                countedData.ServiceDetails[0].SamplingDataPointDetails[x].ListItemAction = OM.ListItemAction.Add;
                                countedData.ServiceDetails[0].SamplingDataPointDetails[x].RejectReason = new NamedObjectRef();
                                countedData.ServiceDetails[0].SamplingDataPointDetails[x].RejectReason.Name = SamplingGrid.GridContext.GetCell(x, RejectReasons.LabelControl.Text).ToString();
                                countedData.ServiceDetails[0].SamplingDataPointDetails[x].DataValue = SamplingGrid.GridContext.GetCell(x, "Qty").ToString();
                            }
                            break;
                    }

                }

                // Handling of Move Std Data Collection & ESig
                if (IsMoveOnCompletion())
                {
                    OM.CollectSamplingData collectSamplingData = serviceData as OM.CollectSamplingData;
                    if (collectSamplingData.MoveStd == null)
                        collectSamplingData.MoveStd = new MoveStd();

                    //Data Collection
                    if (CollectionDef != null && CollectionDef.Data != null)
                    {
                        collectSamplingData.MoveStd.DataCollectionDef = CollectionDef.Data as RevisionedObjectRef;
                    }

                    if (ParamDataControl != null)
                    {
                        DataPointSummary[] dataPointSummary = ParamDataControl.GetDataPointSummary();
                        if (dataPointSummary != null && dataPointSummary.Length > 0)
                            collectSamplingData.MoveStd.ParametricData = dataPointSummary[0];
                    }

                    //ESig
                    ESigServiceDetail[] collectSampingESigTemp = Page.DataContract.GetValueByName("CollectSamplingESig") as ESigServiceDetail[];
                    var moveStdESig = ESigCaptureUtil.CollectESigServiceDetailsAll();

                    if (collectSampingESigTemp != null)
                        collectSamplingData.ESigDetails = collectSampingESigTemp;

                    if (moveStdESig != null)
                        collectSamplingData.MoveStd.ESigDetails = moveStdESig.Item1;

                }
            }
        }

        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null && action.Parameters == "submit")
            {
                if ((!(bool)Page.PortalContext.LocalSession["EsigPostback"] || Page.PortalContext.LocalSession["EsigEventArgument"] == null)  && IsMoveOnCompletion())
                {
                    var collectSampingESig = ESigCaptureUtil.CollectESigServiceDetailsAll();
                    if (collectSampingESig != null)
                        Page.PortalContext.DataContract.SetValueByName("CollectSamplingESig", collectSampingESig.Item1);

                    ProcessMoveStdESig(e);
                }
                
                Page.PortalContext.LocalSession["EsigPostback"] = false;
                Page.PortalContext.LocalSession["EsigEventArgument"] = null;
                ResultStatus result = Page.Service.Submit(PrimaryServiceType, false);

                Page.PortalContext.DataContract.SetValueByName("CollectSamplingESig", null);

                e.Result = result;
                Page.StatusBar.WriteStatus(result);
            }
        }

        protected bool IsMoveOnCompletion()
        {
            ContainerListGrid containerCtrl = Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid;
            if (containerCtrl.Data == null)
                return false;

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.CollectSamplingDataService(session.CurrentUserProfile);
            var serviceData = new OM.CollectSamplingData();
            var serviceInfo = new OM.CollectSamplingData_Info();

            serviceData.Container = containerCtrl.Data as ContainerRef;
            serviceInfo.MoveContainerOnCompletion = FieldInfoUtil.RequestValue();

            var request = new Camstar.WCF.Services.CollectSamplingData_Request();
            var result = new Camstar.WCF.Services.CollectSamplingData_Result();
            var resultStatus = new OM.ResultStatus();

            request.Info = serviceInfo;
            resultStatus = service.GetEnvironment(serviceData, request, out result);

            if (resultStatus.IsSuccess)
            {
                return result.Value?.MoveContainerOnCompletion?.Value ?? false;
            }
            else
            {
                DisplayMessage(resultStatus);
                return false;
            }
        }
       
        protected virtual void ProcessMoveStdESig(Personalization.CustomActionEventArgs e)
        {
            // Reset previous value
            Page.PortalContext.LocalSession["EsigEventArgument"] = null;  
            Page.PortalContext.LocalSession["EsigPostback"] = false;
            ESigCaptureUtil.CleanESigCaptureDM();

            ContainerListGrid containerCtrl = Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid;

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.MoveStdService(session.CurrentUserProfile);
            var request = new Camstar.WCF.Services.MoveStd_Request();
            var result = new Camstar.WCF.Services.MoveStd_Result();
            var param = new Camstar.WCF.Services.MoveStd_LoadESigDetails_Parameters
            {
                ESigMaintAction = "0"
            };

            var serviceData = new OM.MoveStd
            {
                Container = containerCtrl.Data as ContainerRef
            };

            request.Info = new MoveStd_Info()
            {
                ESigRequirement = new OM.Info(true),
                ESigDetails = new ESigServiceDetail_Info() { RequestValue = true }
            };

            ResultStatus resultStatus = service.LoadESigDetails(serviceData, param, request, out result);

            if (resultStatus.IsSuccess)
            {
                if (!result.IsEmpty)
                {
                    Page.PortalContext.LocalSession["EsigEventArgument"] = e;
                    Page.OpenContainerPopupESigCapture(PrimaryServiceType, Tuple.Create(result.Value.ESigDetails, (OM.ESigProcessTimerServiceDetail[])null));
                }
            }
            else
            {
                DisplayMessage(resultStatus);
            }
        }
    }
}
