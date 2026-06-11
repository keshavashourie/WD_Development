/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
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
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;

using SEMI.AppCode;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{

    /// <summary>
    /// Summary description for SS_SPCRecords
    /// </summary>
    public class SS_SPCRecords : MatrixWebPart
    {
        protected JQDataGrid _gridSPCRecords_Results { get { return Page.FindCamstarControl("SPCRecords_Results") as JQDataGrid; } }
        protected JQDataGrid _gridParamsGrid { get { return Page.FindCamstarControl("ObjectChanges_Params") as JQDataGrid; } }
        protected JQDataGrid _gridMessagesGrid { get { return Page.FindCamstarControl("ObjectChanges_Messages") as JQDataGrid; } }

        protected CWC.TextBox _txtSPCTxnDataName { get { return Page.FindCamstarControl("SPCTxnDataName") as CWC.TextBox; } }        
        protected CWC.TextBox _txtSetupName { get { return Page.FindCamstarControl("ObjectChanges_Name") as CWC.TextBox; } }
        protected CWC.TextBox _txtSPCResult { get { return Page.FindCamstarControl("ObjectChanges_SPCResultEx") as CWC.TextBox; } }
        protected CWC.TextBox _txtSPCFailureAction { get { return Page.FindCamstarControl("ObjectChanges_SPCFailureActionEx") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoSPCFailureDocumentSet { get { return Page.FindCamstarControl("ObjectChanges_FailureDocumentSet") as CWC.NamedObject; } }
        protected CWC.TextBox _txtSPCResultFilename { get { return Page.FindCamstarControl("ObjectChanges_SPCResultFilename") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoResource { get { return Page.FindCamstarControl("ObjectChanges_ResourceEx") as CWC.NamedObject; } }
        protected ContainerListGrid _contContainer { get { return Page.FindCamstarControl("ObjectChanges_ContainerEx") as ContainerListGrid; } }
        protected CWC.TextBox _txtCDOName { get { return Page.FindCamstarControl("ObjectChanges_CDONameEx") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoUser { get { return Page.FindCamstarControl("ObjectChanges_User") as CWC.NamedObject; } }
        protected CWC.DateChooser _dateTxnDate { get { return Page.FindCamstarControl("ObjectChanges_TxnDate") as CWC.DateChooser; } }
        protected CWC.TextBox _txtSPCErrorMessage { get { return Page.FindCamstarControl("ObjectChanges_SPCErrorMessage") as CWC.TextBox; } }
        protected CWC.CheckBox _chkSkipFailureTxn { get { return Page.FindCamstarControl("ObjectChanges_SkipFailureTxns") as CWC.CheckBox; } }
        protected CWC.TextBox _txtEquipmentMatrix { get { return Page.FindCamstarControl("ObjectChanges_EquipmentMatrix") as CWC.TextBox; } }
        protected CWC.TextBox _txtSPCMatrix { get { return Page.FindCamstarControl("ObjectChanges_SPCMatrix") as CWC.TextBox; } }
        protected CWC.TextBox _txtSPCObjectTypeName { get { return Page.FindCamstarControl("ObjectChanges_SPCObjectTypeName") as CWC.TextBox; } }

        protected CWC.NamedObject _ndoQuerySPCSetup { get { return Page.FindCamstarControl("ObjectChanges_SPCSetup") as CWC.NamedObject; } }
        protected CWC.TextBox _contQueryContainer { get { return Page.FindCamstarControl("ObjectChanges_Container") as CWC.TextBox ; } }
        protected CWC.NamedObject _ndoQueryResource { get { return Page.FindCamstarControl("ObjectChanges_Resource") as CWC.NamedObject; } }
        protected CWC.TextBox _txtQueryCDOName { get { return Page.FindCamstarControl("ObjectChanges_CDOName") as CWC.TextBox; } }
        protected CWC.DropDownList _ddlQuerySPCResult { get { return Page.FindCamstarControl("ObjectChanges_SPCResult") as CWC.DropDownList; } }
        protected CWC.DropDownList _ndoQuerySPCFailureAction { get { return Page.FindCamstarControl("ObjectChanges_SPCFailureAction") as CWC.DropDownList; } }
        protected CWC.TextBox _txtQueryStartRowNum { get { return Page.FindCamstarControl("StartRowNum") as CWC.TextBox; } }
        protected CWC.TextBox _txtQueryStopRowNum { get { return Page.FindCamstarControl("StopRowNum") as CWC.TextBox; } }
		protected CWC.NamedObject _ndo_ss_ProcessEquipment { get { return Page.FindCamstarControl("ObjectChanges_ss_ProcessEquipment") as CWC.NamedObject; } }
		protected CWC.RevisionedObject _rdo_ss_ProcessRecipe { get { return Page.FindCamstarControl("ObjectChanges_ss_ProcessRecipe") as CWC.RevisionedObject; } }
		protected CWC.TextBox _txt_ss_SPCMatrixContext { get { return Page.FindCamstarControl("ObjectChanges_ss_SPCMatrixContext") as CWC.TextBox; } }
		protected CWC.DateChooser _dtc_ss_StartDate { get { return Page.FindCamstarControl("ss_StartDate") as CWC.DateChooser; } }
		protected CWC.DateChooser _dtc_ss_EndDate { get { return Page.FindCamstarControl("ss_EndDate") as CWC.DateChooser; } }

        protected CWC.ViewDocumentsControl _docSPCFailureDocumentSetView { get { return Page.FindCamstarControl("FailDocSetViewer") as CWC.ViewDocumentsControl; } }

        protected CWC.TextBox _txtCPHeight { get { return Page.FindCamstarControl("CPHeight") as CWC.TextBox; } }
        protected CWC.TextBox _txtCPWidth { get { return Page.FindCamstarControl("CPWidth") as CWC.TextBox; } }
        protected CWC.TextBox _txtCPHeightOffset { get { return Page.FindCamstarControl("CPHeightOffset") as CWC.TextBox; } }
        protected CWC.TextBox _txtCPWidthOffset { get { return Page.FindCamstarControl("CPWidthOffset") as CWC.TextBox; } }

        protected DataEnvelopControl _DataEnvelop { get { return Page.FindCamstarControl("Envelop") as DataEnvelopControl; } }

        protected MatrixWebPart SPCResultsWP
        {
            get
            {
                MatrixWebPart wp = null;
                int iParentControlsCount = Parent.Controls.Count;
                for (int z = 0; z < iParentControlsCount; z++)
                {
                    if (Parent.Controls[z].ID == "SPCResultsWP")
                        wp = Parent.Controls[z] as MatrixWebPart;
                }
                return wp;
            }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                AddDataEnvelopDataMember();
                SetDropDownListValues();
            }

            if (_ndoQuerySPCFailureAction.PickListPanelControl != null)
                _ndoQuerySPCFailureAction.PickListPanelControl.PostProcessData += new CWC.PickLists.DataRequestEventHandler(_ndoQuerySPCFailureAction_PostProcessData);

            if (_ddlQuerySPCResult.PickListPanelControl != null)
                _ddlQuerySPCResult.PickListPanelControl.PostProcessData += new CWC.PickLists.DataRequestEventHandler(_ddlQuerySPCResult_PostProcessData);
        }


        //-----------------------------------------
        //
        //-----------------------------------------
        void _ndoQuerySPCFailureAction_PostProcessData(object sender, CWC.PickLists.DataRequestEventArgs e)
        {
            var rowsPerPage = 50;           
            var pickListData = new DataTable();
            pickListData.Columns.Add("Name");
            pickListData.Columns.Add("Value");
          
            pickListData.Rows.Add("", "");
            pickListData.Rows.Add("HOLD", "HOLD");
            pickListData.Rows.Add("HOLDALL", "HOLDALL");
            pickListData.Rows.Add("WAIVE", "WAIVE");

            e.Data = pickListData;
            e.ResultStatus = new ResultStatus("", true);
            e.PagesCount = (e.TotalRecords / rowsPerPage) + ((e.TotalRecords % rowsPerPage) > 0 ? 1 : 0);
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        void _ddlQuerySPCResult_PostProcessData(object sender, CWC.PickLists.DataRequestEventArgs e)
        {
            var rowsPerPage = 50;
            var pickListData = new DataTable();
            pickListData.Columns.Add("Name");
            pickListData.Columns.Add("Value");

            pickListData.Rows.Add("", "");
            pickListData.Rows.Add("PASS", "PASS");
            pickListData.Rows.Add("FAIL", "FAIL");            

            e.Data = pickListData;
            e.ResultStatus = new ResultStatus("", true);
            e.PagesCount = (e.TotalRecords / rowsPerPage) + ((e.TotalRecords % rowsPerPage) > 0 ? 1 : 0);
        }

        private void SetDropDownListValues()
        {
            _ddlQuerySPCResult.DropDownControl.Items.Add("");
            _ddlQuerySPCResult.DropDownControl.Items.Add("PASS");
            _ddlQuerySPCResult.DropDownControl.Items.Add("FAIL");
            _ndoQuerySPCFailureAction.DropDownControl.Items.Add("");
            _ndoQuerySPCFailureAction.DropDownControl.Items.Add("HOLD");
            _ndoQuerySPCFailureAction.DropDownControl.Items.Add("HOLDALL");
            _ndoQuerySPCFailureAction.DropDownControl.Items.Add("WAIVE");
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public override string FindFirstFocusableControlID()
        {
            return null;
        } // FindFirstFocusableControlID

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
            UIComponentDataMember[] objPageDataMembers = new UIComponentDataMember[intConfiguredDataMemberCount + 2];
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
            objPageDataMembers[intDMIndex].Key = "SPCDetailsWP.Envelop";
            objPageDataMembers[intDMIndex].Name = "envelopMainOutDM";
            objPageDataMembers[intDMIndex].ConnectionType = DataMemberConnectionType.Control;
            objPageDataMembers[intDMIndex].Property = "SS_DataPacket";
            intDMIndex++;

            // add the new envelop data member
            objPageDataMembers[intDMIndex] = new UIComponentDataMember();
            objPageDataMembers[intDMIndex].Key = "SPCDetailsWP.Envelop";
            objPageDataMembers[intDMIndex].Name = "envelopMainInDM";
            objPageDataMembers[intDMIndex].ConnectionType = DataMemberConnectionType.Control;
            objPageDataMembers[intDMIndex].Property = "SS_DataPacket";

            Page.DataContract.DataMembers = objPageDataMembers;
        } // AddDataEnvelopDataMember

        //-----------------------------------------
        //
        //-----------------------------------------
        public void LoadSPCRecord()
        {
            if (_txtSPCTxnDataName.Data != null)
            {
                _txtSetupName.ClearData();
                _txtSPCResult.ClearData();
                _txtSPCFailureAction.ClearData();
                _ndoSPCFailureDocumentSet.ClearData();
                _txtSPCResultFilename.ClearData();
                _ndoResource.ClearData();
                _contContainer.ClearData();
                _txtCDOName.ClearData();
                _ndoUser.ClearData();
                _dateTxnDate.ClearData();
                _txtSPCErrorMessage.ClearData();
                _gridParamsGrid.ClearData();
                _gridMessagesGrid.ClearData();

                SPCTxnDataMaint inputData = new SPCTxnDataMaint();
                inputData.ObjectToChange = new NamedObjectRef();

                inputData.ObjectToChange.Name = _txtSPCTxnDataName.Data.ToString();// _gridSPCRecords_Results.BoundContext.GetSelectedCell("SPCTxnDataName").ToString();

                SPCTxnDataMaint_Info info = new SPCTxnDataMaint_Info
                {
                    ObjectChanges = new SPCTxnDataChanges_Info
                    {
                        SPCSetup = FieldInfoUtil.RequestValue(),
                        SPCFailureAction = FieldInfoUtil.RequestValue(),
                        FailureDocumentSet = FieldInfoUtil.RequestValue(),
                        SkipFailureTxns = FieldInfoUtil.RequestValue(),
                        SPCResultFilename = FieldInfoUtil.RequestValue(),
                        CDOName = FieldInfoUtil.RequestValue(),
                        User = FieldInfoUtil.RequestValue(),
                        SPCResult = FieldInfoUtil.RequestValue(),
                        TxnDate = FieldInfoUtil.RequestValue(),
                        SPCObjectTypeName = FieldInfoUtil.RequestValue(),
                        EquipmentMatrix = FieldInfoUtil.RequestValue(),
                        SPCMatrix = FieldInfoUtil.RequestValue(),
                        Params = new SPCTxnDataParamsChanges_Info
                        {
                            ParamName = FieldInfoUtil.RequestValue(),
                            ParamValue = FieldInfoUtil.RequestValue()
                        },
                        Container = FieldInfoUtil.RequestValue(),
                        Resource = FieldInfoUtil.RequestValue(),
                        Messages = new SPCTxnDataMessagesChanges_Info
                        {
                            Message = FieldInfoUtil.RequestValue()
                        },
                        SPCErrorMessage = FieldInfoUtil.RequestValue()
                    }
                };

                UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
                SPCTxnDataMaintService svc = new SPCTxnDataMaintService(profile);
                SPCTxnDataMaint_Result result = null;
                ResultStatus resultStatus = svc.Load(inputData, new SPCTxnDataMaint_Request { Info = info }, out result);

                if (resultStatus.IsSuccess)
                {
                    SPCTxnDataChanges objectChanges = result.Value.ObjectChanges as SPCTxnDataChanges;
                    
                    _txtSPCTxnDataName.Data = _txtSPCTxnDataName.Data;
                    _txtSetupName.Data = objectChanges.SPCSetup;
                    _txtSPCResult.Data = objectChanges.SPCResult;
                    _txtSPCFailureAction.Data = objectChanges.SPCFailureAction;
                    _ndoSPCFailureDocumentSet.Data = objectChanges.FailureDocumentSet;
                    _txtSPCResultFilename.Data = objectChanges.SPCResultFilename;
                    _ndoResource.Data = objectChanges.Resource;
                    _contContainer.Data = objectChanges.Container;
                    _txtCDOName.Data = objectChanges.CDOName;
                    _ndoUser.Data = objectChanges.User;
                    _dateTxnDate.Data = objectChanges.TxnDate != null ? objectChanges.TxnDate.ToString() : "";
                    _txtSPCErrorMessage.Data = objectChanges.SPCErrorMessage;
                    _txtSPCObjectTypeName.Data = objectChanges.SPCObjectTypeName;
                    _txtSPCMatrix.Data = objectChanges.SPCMatrix;
                    _txtEquipmentMatrix.Data = objectChanges.EquipmentMatrix;
                    _chkSkipFailureTxn.CheckControl.Checked = objectChanges.SkipFailureTxns != null ? bool.Parse(objectChanges.SkipFailureTxns.ToString()) : false;

                    _gridParamsGrid.ClearData();
                    (_gridParamsGrid.GridContext as ItemDataContext).Data = objectChanges.Params;
                    _gridParamsGrid.BoundContext.LoadData();

                    _gridMessagesGrid.ClearData();
                    (_gridMessagesGrid.GridContext as ItemDataContext).Data = objectChanges.Messages;
                    _gridMessagesGrid.BoundContext.LoadData();

                    if (_ndoSPCFailureDocumentSet.Data != null)
                        LoadDocumentSet();
                }
                
            }
        } // LoadSPCRecord

        //-----------------------------------------
        //
        //-----------------------------------------
        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            try
            {

                base.WebPartCustomAction(sender, e);
                var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
                if (action != null)
                    switch (action.Parameters)
                    {
                        case "Delete":
                            e.Result = DeleteSPCRecord();
                            break;
                        case "Replot":
                            e.Result = Replot();
                            break;
                        case "Display":
                            DisplayChart();
                            break;
                        case "Reset":
                            ResetPage();
                            break;
                    }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        } // WebPartCustomAction

        //-----------------------------------------
        //
        //-----------------------------------------
        public ResultStatus DeleteSPCRecord()
        {
            ResultStatus ReturnResultStatus = new ResultStatus();

            if (_txtSPCTxnDataName.Data != null)
            {
                UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
                SPCTxnDataMaintService objService = new SPCTxnDataMaintService(profile);

                SPCTxnDataMaint objServiceData = new SPCTxnDataMaint();
                SPCTxnDataChanges objChanges = new SPCTxnDataChanges();
                SPCTxnDataMaint_Info objServiceInfo = new SPCTxnDataMaint_Info();
                SPCTxnDataChanges_Info objChangesInfo = new SPCTxnDataChanges_Info();

                objServiceData.ObjectChanges = objChanges;
                objServiceInfo.ObjectChanges = objChangesInfo;

                objChanges.Name = _txtSPCTxnDataName.Data.ToString();
                objServiceData.ObjectToChange = new NamedObjectRef(_txtSPCTxnDataName.Data.ToString().ToString());

                OM.ResultStatus resultStatus = null;

                objService.BeginTransaction();

                objService.Delete(objServiceData);
                objService.ExecuteTransaction();

                resultStatus = objService.CommitTransaction();

                if (resultStatus.IsSuccess)
                    RetrieveSPCRecords();

                ReturnResultStatus = resultStatus;
            }
            else
            {
                ReturnResultStatus = new ResultStatus(_txtSPCTxnDataName.LabelControl.Text + " is required ", false);
            }

            return ReturnResultStatus;
        } // DeleteSPCRecord

        //-----------------------------------------
        //
        //-----------------------------------------
        public void DisplayChart()
        {
            if (_txtSPCTxnDataName.Data != null)
            {
                SPCTxnData objSPCTxnData = new SPCTxnData();
                SEMI.AppCode.Services.SPCTxn.LoadSPCTxnDataDetails(_txtSPCTxnDataName.Data.ToString(), ref objSPCTxnData);

                // set the data envelop data to pass to the popup;
                SEMI.AppCode.Services.SPCTxn.SPCTxnDataSet SPCTxnDataSet = new SEMI.AppCode.Services.SPCTxn.SPCTxnDataSet();
                SPCTxnDataSet.AddDataListItem(objSPCTxnData);

                DataPacket SS_DataPacket = new DataPacket();

                SS_DataPacket.SPCTxnDataSet = SPCTxnDataSet;
                _DataEnvelop.SS_DataPacket = SS_DataPacket;

                _txtCPWidth.Data = objSPCTxnData.ChartWidth;
                _txtCPHeight.Data = objSPCTxnData.ChartHeight;

                Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                objAction.PageName = "SS_SPCChartsPopupVP";
                objAction.EndResponse = true;

                objAction.FrameLocation = new UIFloatingPageLocation();
                objAction.FrameLocation.Width = int.Parse(_txtCPWidth.Data.ToString()) + int.Parse(_txtCPWidthOffset.Data.ToString()); //750
                objAction.FrameLocation.Height = int.Parse(_txtCPHeight.Data.ToString()) + int.Parse(_txtCPHeightOffset.Data.ToString()); //580;

                UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[1];
                objLinks[0] = new UIComponentDataContractLink();
                objLinks[0].SourceMember = "envelopMainOutDM";
                objLinks[0].TargetMember = "envelopPopupInDM";
                objAction.DataContractMap = new UIComponentDataContractMap();
                objAction.DataContractMap.Links = objLinks;

                UIComponentDataContractReturnLink[] objReturnLinks = new UIComponentDataContractReturnLink[1];
                objReturnLinks[0] = new UIComponentDataContractReturnLink();
                objReturnLinks[0].SourceMember = "envelopPopupOutDM";
                objReturnLinks[0].TargetMember = "envelopMainInDM";
                objAction.DataContractReturnMap = new UIComponentDataContractReturnMap();
                objAction.DataContractReturnMap.ReturnLinks = objReturnLinks;

                Page.ActionDispatcher.ExecuteAction(objAction);

            }
            else
            {
                Page.StatusBar.WriteError(_txtSPCTxnDataName.LabelControl.Text + " is required ");
            }
        } // DisplayChart

        //-----------------------------------------
        //
        //-----------------------------------------
        public ResultStatus Replot()
        {
            ResultStatus ReturnResultStatus = new ResultStatus();

            if (_txtSPCTxnDataName.Data != null)
            {
                UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
                SPCTxnDataMaintService objService = new SPCTxnDataMaintService(profile);

                SPCTxnDataMaint objServiceData = new SPCTxnDataMaint();
                SPCTxnDataChanges objChanges = new SPCTxnDataChanges();
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
                    LoadSPCRecord();
                    DisplayChart();
                }

                ReturnResultStatus = resultStatus;
            }
            else
            {
                ReturnResultStatus = new ResultStatus(_txtSPCTxnDataName.LabelControl.Text + " is required ", false);
            }

            return ReturnResultStatus;
        } // Replot

        //-----------------------------------------
        //
        //-----------------------------------------
        public void LoadDocumentSet()
        {
            UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
            DocumentSetMaintService objService = new DocumentSetMaintService(profile);

            DocumentSetMaint objServiceData = new DocumentSetMaint();
            DocumentSetMaint_Info objServiceInfo = new DocumentSetMaint_Info();
            DocumentSetChanges_Info objChangesInfo = new DocumentSetChanges_Info();
            DocumentSetMaint_Result result = null;

            OM.ResultStatus resultStatus = null;

            objServiceData.ObjectToChange = _ndoSPCFailureDocumentSet.Data as NamedObjectRef;
            objChangesInfo.DocumentEntries = new DocumentEntryChanges_Info
            {
                Document = FieldInfoUtil.RequestValue()
            };

            objServiceInfo.ObjectChanges = objChangesInfo;
            resultStatus = objService.Load(objServiceData, new DocumentSetMaint_Request { Info = objServiceInfo }, out result);

            if (resultStatus.IsSuccess)
            {
                DocumentSet objDocSet = new DocumentSet();
                int intDocCount = 0;
                intDocCount = result.Value.ObjectChanges.DocumentEntries.Length;
                DocumentEntry[] objDocuments = new DocumentEntry[intDocCount];

                for (int i = 0; i < intDocCount; i++)
                {
                    DocumentMaintService objDocService = new DocumentMaintService(profile);
                    DocumentMaint objDocServiceData = new DocumentMaint();
                    DocumentMaint_Info objDocServiceInfo = new DocumentMaint_Info();
                    DocumentChanges_Info objDocChangesInfo = new DocumentChanges_Info();
                    DocumentMaint_Result resultDoc = null;

                    objDocServiceData.ObjectToChange = result.Value.ObjectChanges.DocumentEntries[i].Document;
                    objDocChangesInfo = new DocumentChanges_Info
                    {
                        Name = FieldInfoUtil.RequestValue(),
                        Identifier = FieldInfoUtil.RequestValue(),
                        BrowseMode = FieldInfoUtil.RequestValue(),
                    };

                    objDocServiceInfo.ObjectChanges = objDocChangesInfo;
                    resultStatus = objDocService.Load(objDocServiceData, new DocumentMaint_Request { Info = objDocServiceInfo }, out resultDoc);

                    if (resultStatus.IsSuccess)
                    {
                        objDocuments[i] = new DocumentEntry();
                        objDocuments[i].Document = result.Value.ObjectChanges.DocumentEntries[i].Document;
                        objDocuments[i].DocumentIdentifier = resultDoc.Value.ObjectChanges.Identifier;
                        objDocuments[i].Name = resultDoc.Value.ObjectChanges.Name;
                        objDocuments[i].DisplayName = resultDoc.Value.ObjectChanges.Name;
                        objDocuments[i].DocumentBrowseMode= resultDoc.Value.ObjectChanges.BrowseMode;
                    }
                }

                objDocSet.DocumentEntries = objDocuments;
                _docSPCFailureDocumentSetView.Data = objDocSet;
            }
            else
            {
                _docSPCFailureDocumentSetView.Data = null;
            }
        } // LoadDocumentSet

        //-----------------------------------------
        //
        //-----------------------------------------
        public void RetrieveSPCRecords()
        {
            FrameworkSession currentSession = FrameworkManagerUtil.GetFrameworkSession(this.Page.Session);
            QueryUtil QueryService = new QueryUtil(currentSession.CurrentUserProfile);
            RecordSet queryResult = null;
            ResultStatus resultStatus = null;
            QueryOptions queryOption = new QueryOptions();
            QueryParameter[] queryParams = new QueryParameter[14];
			string formattedStartDate = "%";
			string formattedEndDate = "%";

            queryOption.QueryType = Camstar.WCF.ObjectStack.QueryType.System; /// querytype = system
            queryOption.StartRow = 1;// Start Row;
            queryOption.RowSetSize = 10000;// Row Set Size;

            queryParams[0] = new QueryParameter();
            queryParams[0].Name = "SPCSetupName";
            queryParams[0].Value = _ndoQuerySPCSetup.Data != null ? (_ndoQuerySPCSetup.Data.ToString() != "" ? _ndoQuerySPCSetup.Data.ToString() : "%") : "%";

            queryParams[1] = new QueryParameter();
            queryParams[1].Name = "CDOName";
            queryParams[1].Value = _txtQueryCDOName.Data != null ? (_txtQueryCDOName.Data.ToString() != "" ? _txtQueryCDOName.Data.ToString() : "%") : "%";

            queryParams[2] = new QueryParameter();
            queryParams[2].Name = "ContainerName";
            queryParams[2].Value = _contQueryContainer.Data != null ? (_contQueryContainer.Data.ToString() != "" ? _contQueryContainer.Data.ToString() : "%") : "%";

            queryParams[3] = new QueryParameter();
            queryParams[3].Name = "ResourceName";
            queryParams[3].Value = _ndoQueryResource.Data != null ? (_ndoQueryResource.Data.ToString() != "" ? _ndoQueryResource.Data.ToString() : "%") : "%";

            queryParams[4] = new QueryParameter();
            queryParams[4].Name = "SPCResult";
            queryParams[4].Value = _ddlQuerySPCResult.Data != null ? (_ddlQuerySPCResult.Data.ToString() != "" ? _ddlQuerySPCResult.Data.ToString() : "%") : "%";

            queryParams[5] = new QueryParameter();
            queryParams[5].Name = "SPCFailureAction";
            queryParams[5].Value = _ndoQuerySPCFailureAction.Data != null ? (_ndoQuerySPCFailureAction.Data.ToString() != "" ? _ndoQuerySPCFailureAction.Data.ToString() : "%") : "%";

            queryParams[6] = new QueryParameter();
            queryParams[6].Name = "STARTROWNUM";
            queryParams[6].Value = _txtQueryStartRowNum.Data.ToString();

            queryParams[7] = new QueryParameter();
            queryParams[7].Name = "STOPROWNUM";
            queryParams[7].Value = _txtQueryStopRowNum.Data.ToString();

			queryParams[8] = new QueryParameter();
			queryParams[8].Name = "ss_RelatedEquipmentName";
			queryParams[8].Value = _ndo_ss_ProcessEquipment.Data != null ? (_ndo_ss_ProcessEquipment.Data.ToString() != "" ? _ndo_ss_ProcessEquipment.Data.ToString() : "%") : "%";

			queryParams[9] = new QueryParameter();
			queryParams[9].Name = "ss_RelatedRecipeName";
			queryParams[9].Value = _rdo_ss_ProcessRecipe.Data != null ? (_rdo_ss_ProcessRecipe.Data.ToString() != "" ? _rdo_ss_ProcessRecipe.Data.ToString() : "%") : "%";

			queryParams[10] = new QueryParameter();
			queryParams[10].Name = "ss_SPCMatrixContext";
			queryParams[10].Value = _txt_ss_SPCMatrixContext.Data != null ? (_txt_ss_SPCMatrixContext.Data.ToString() != "" ? _txt_ss_SPCMatrixContext.Data.ToString() : "%") : "%";

			if (_dtc_ss_StartDate.Data != null)
			{
				var Value = _dtc_ss_StartDate.Value.Value;
				DateTime StartDate = new DateTime(Value.Year, Value.Month, Value.Day, Value.Hour, Value.Minute, Value.Second);
				formattedStartDate = StartDate.ToString("dd-MMM-yyyy HH:mm:ss");
			}
			
			queryParams[11] = new QueryParameter();
			queryParams[11].Name = "ss_StartDate";
			queryParams[11].Value = formattedStartDate;

			if (_dtc_ss_EndDate.Data != null)
			{
				var Value = _dtc_ss_EndDate.Value.Value;
				DateTime EndDate = new DateTime(Value.Year, Value.Month, Value.Day, Value.Hour, Value.Minute, Value.Second);
				formattedEndDate = EndDate.ToString("dd-MMM-yyyy HH:mm:ss");
			}

			queryParams[12] = new QueryParameter();
			queryParams[12].Name = "ss_EndDate";
			queryParams[12].Value = formattedEndDate;

			queryParams[13] = new QueryParameter();
			queryParams[13].Name = "OracleDateFormat";
			queryParams[13].Value = "dd-mon-yyyy hh24:mi:ss";

            QueryService.Execute("_SPCRecords", queryParams, queryOption, ref queryResult, ref resultStatus);
            if (resultStatus.IsSuccess)
            {
                DataTable dtResult = new DataTable();
                dtResult = queryResult.GetAsExplicitlyDataTable();
                _gridSPCRecords_Results.ClearData();
                JQDataGrid _gridSPCRecords_ResultsTemp = _gridSPCRecords_Results;

                if (dtResult.Rows.Count > 0)
                {
                    string[] sHiddenColumns = new string[] { "InstanceID", "SPCTxnDataName", "CDOName" };
                    SEMI.AppCode.GridUtility.ItemListGrid_SetColumns(SPCResultsWP, dtResult, _gridSPCRecords_Results.ID, null, "_SPCRecordsTxn_SPCRecordsQuery", true, sHiddenColumns, true, null, queryResult.Headers);
                    SEMI.AppCode.GridUtility.ItemListGrid_BindDataTable(SPCResultsWP, dtResult, ref _gridSPCRecords_ResultsTemp, "_SPCRecordsTxn_SPCRecordsQuery");

                    CamstarWebControl.SetRenderToClient(_gridSPCRecords_Results);
                    DisplayMessage(new ResultStatus());
                }
                else
                {
                    DisplayMessage(new ResultStatus("No Records found", true));
                }
            }
            else
            {
                DisplayMessage(resultStatus);
            }
        } // RetrieveSPCRecords

        //-----------------------------------------
        //
        //-----------------------------------------
        public void ClearSearchFields()
        {
            _ndoQuerySPCSetup.ClearData();
            _ndoQueryResource.ClearData();
            _contQueryContainer.ClearData();
            _ndoQuerySPCFailureAction.ClearData();
            _ddlQuerySPCResult.ClearData();
            _txtQueryCDOName.ClearData();
			_ndo_ss_ProcessEquipment.ClearData();
			_rdo_ss_ProcessRecipe .ClearData();
			_txt_ss_SPCMatrixContext.ClearData();
			_dtc_ss_StartDate.ClearData();
			_dtc_ss_EndDate.ClearData();

            RetrieveSPCRecords();
        } // ClearSearchFields

        //-----------------------------------------
        //
        //-----------------------------------------
        public void ResetPage()
        {
            Page.ClearValues();
            _txtSPCTxnDataName.ClearData();
        } // ResetPage

    }
}



