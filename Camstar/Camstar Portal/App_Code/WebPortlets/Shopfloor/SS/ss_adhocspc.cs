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
    /// Summary description for SS_AdHocSPC
    /// </summary>
    public class SS_AdHocSPC: MatrixWebPart 
    {
        protected JQDataGrid _gridSPCParamsGrid { get { return Page.FindCamstarControl("AdHocSPC_SPCParams") as JQDataGrid; } }
        protected CWC.NamedObject _ndoSPCSetupField { get { return Page.FindCamstarControl("AdHocSPC_SPCSetup") as CWC.NamedObject; } }
        protected CWC.TextBox _txtNameField { get { return Page.FindCamstarControl("SPCTxnDataList_Name") as CWC.TextBox; } }
        protected CWC.TextBox _txtSPCFailureActionField { get { return Page.FindCamstarControl("SPCTxnDataList_SPCFailureAction") as CWC.TextBox; } }
        protected CWC.TextBox _txtSPCResultField { get { return Page.FindCamstarControl("SPCTxnDataList_SPCResult") as CWC.TextBox; } }
        protected CWC.TextBox _txtSPCResultFileNameField { get { return Page.FindCamstarControl("SPCTxnDataList_SPCResultFilename") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoEmployeeField { get { return Page.FindCamstarControl("AdHocSPC_Employee") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoFailureDocumentSetField { get { return Page.FindCamstarControl("SPCTxnDataList_FailureDocumentSet") as CWC.NamedObject; } }
        protected DataEnvelopControl _envDataEnvelop { get { return Page.FindCamstarControl("Envelop") as DataEnvelopControl; } }      
        protected CWC.ViewDocumentsControl _docSPCFailureDocumentSetView { get { return Page.FindCamstarControl("FailDocSetViewer") as CWC.ViewDocumentsControl; } }
        protected CWC.TextBox _txtCPHeight { get { return Page.FindCamstarControl("CPHeight") as CWC.TextBox; } }
        protected CWC.TextBox _txtCPWidth { get { return Page.FindCamstarControl("CPWidth") as CWC.TextBox; } }
        protected CWC.TextBox _txtCPHeightOffset { get { return Page.FindCamstarControl("CPHeightOffset") as CWC.TextBox; } }
        protected CWC.TextBox _txtCPWidthOffset { get { return Page.FindCamstarControl("CPWidthOffset") as CWC.TextBox; } }

        //-----------------------------------------
        //
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Page.IsPostBack)
            {                               
                _ndoSPCSetupField.DataChanged += delegate { LoadDependentControls(); };
                
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))              
                    OnPopupClose();                               
            }
            else
            {
                AddDataEnvelopDataMember();
            }
        } // OnLoad

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

            // add the new envelop data member for data to be passed TO popup
            objPageDataMembers[intDMIndex] = new UIComponentDataMember();
            objPageDataMembers[intDMIndex].Key = "BlankWP.Envelop";
            objPageDataMembers[intDMIndex].Name = "envelopMainOutDM";
            objPageDataMembers[intDMIndex].ConnectionType = DataMemberConnectionType.Control;
            objPageDataMembers[intDMIndex].Property = "SS_DataPacket";
            intDMIndex++;

            // add the new envelop data member for data to be retrieved FROM popup
            objPageDataMembers[intDMIndex] = new UIComponentDataMember();
            objPageDataMembers[intDMIndex].Key = "BlankWP.Envelop";
            objPageDataMembers[intDMIndex].Name = "envelopMainInDM";
            objPageDataMembers[intDMIndex].ConnectionType = DataMemberConnectionType.Control;
            objPageDataMembers[intDMIndex].Property = "SS_DataPacket";

            Page.DataContract.DataMembers = objPageDataMembers;
        } // AddDataEnvelopDataMember

        //-----------------------------------------
        //
        //-----------------------------------------
        private void LoadDependentControls()
        {
            _gridSPCParamsGrid.ClearData();
            _txtNameField.ClearData();
            _txtSPCFailureActionField.ClearData();
            _txtSPCResultField.ClearData();
            _txtSPCResultFileNameField.ClearData();
            _ndoFailureDocumentSetField.ClearData();

            if (_ndoSPCSetupField.Data != null)
            {
                AdHocSPC inputData = new AdHocSPC
                {
                    SPCSetup = _ndoSPCSetupField.Data as NamedObjectRef
                };
                
                AdHocSPC_Info info = new AdHocSPC_Info
                { 
                    SPCParamsSelection = new SPCTxnDataParamsChanges_Info
                    {
                        ParamName = FieldInfoUtil.RequestValue(),
                        ParamValue = FieldInfoUtil.RequestValue() 
                    }
                };

                UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
                AdHocSPCService serv = new AdHocSPCService(profile);
                AdHocSPC_Result result = null;
                ResultStatus resultStatus = serv.GetEnvironment(inputData, new AdHocSPC_Request { Info = info }, out result);
                if (resultStatus.IsSuccess)
                {
                    if (result.Value.SPCParamsSelection != null)
                    {
                        SPCTxnDataParamsChanges[] objParams = new SPCTxnDataParamsChanges[result.Value.SPCParamsSelection.Length];
                        int intParamIndex = 0;
                        foreach (SPCTxnDataParamsChanges param in result.Value.SPCParamsSelection)
                        {
                            objParams[intParamIndex] = new SPCTxnDataParamsChanges();
                            objParams[intParamIndex].ParamName = param.ParamName;
                            objParams[intParamIndex].ParamValue = param.ParamValue;
                            intParamIndex++;
                        }

                        _gridSPCParamsGrid.OriginalData = null;
                        _gridSPCParamsGrid.ClearData();
                        (_gridSPCParamsGrid.GridContext as ItemDataContext).Data = objParams;
                        _gridSPCParamsGrid.BoundContext.LoadData();                
                    }
                    else
                    {
                        _gridSPCParamsGrid.OriginalData = null;
                        _gridSPCParamsGrid.ClearData();
                    }
                    
                }
            }
        } // LoadDependentControls

        //-----------------------------------------
        //
        //-----------------------------------------
        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
          
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            if (action != null)
                switch (action.Parameters)
                {
                    case "ExecuteSPC":              
                        e.Result = ExecuteSPC();
                        break;
                    case "DisplayChart":                       
                        DisplayChart();
                        break;
                }           
        } // WebPartCustomAction

        //-----------------------------------------
        //
        //-----------------------------------------
        public ResultStatus ExecuteSPC()
        {
            ResultStatus ReturnResult = new ResultStatus();

            if (_ndoSPCSetupField.Data != null)
            {
                // clear fields

                // prepare request
                AdHocSPC inputData = new AdHocSPC
                {
                    SPCSetup = _ndoSPCSetupField.Data as NamedObjectRef,
                    Employee = _ndoEmployeeField.Data as NamedObjectRef
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

                AdHocSPC_Info info = new AdHocSPC_Info
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
                AdHocSPCService serv = new AdHocSPCService(profile);
                AdHocSPC_Result result = null;
                ResultStatus resultStatus = serv.ExecuteTransaction(inputData, new AdHocSPC_Request { Info = info }, out result);

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
                                _txtCPHeight.Data = oSPCData.ChartHeight;
                                _txtCPWidth.Data = oSPCData.ChartWidth;
                            }

                            if (_ndoFailureDocumentSetField.Data != null)
                                LoadDocumentSet();

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
            }

            return ReturnResult;
                              
        } // ExecuteSPC

        //-----------------------------------------
        //
        //-----------------------------------------
        public void DisplayChart()
        {
            // set the data envelop data to pass to the popup;            
            SPCTxnData oSPCData = new SPCTxnData();
            oSPCData.Name = _txtNameField.Data.ToString();
            oSPCData.SPCResult = _txtSPCResultField.Data != null ? _txtSPCResultField.Data.ToString() : "";
            oSPCData.SPCSetup = _ndoSPCSetupField.Data != null ? new NamedObjectRef(_ndoSPCSetupField.Data.ToString()) : null;
            oSPCData.SPCResultFilename = _txtSPCResultFileNameField.Data != null ? _txtSPCResultFileNameField.Data.ToString() : "";
            oSPCData.ChartHeight = _txtCPHeight.Data != null ? int.Parse(_txtCPHeight.Data.ToString()) : 0;
            oSPCData.ChartWidth = _txtCPWidth.Data != null ? int.Parse(_txtCPWidth.Data.ToString()) : 0;
            oSPCData.FailureDocumentSet = _ndoFailureDocumentSetField.Data != null ? new NamedObjectRef(_ndoFailureDocumentSetField.Data.ToString()) : null;

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

            SEMI.AppCode.Services.SPCTxn.PopupSPCChart(this, objLinks, objReturnLinks, int.Parse(_txtCPWidth.Data.ToString()) + int.Parse(_txtCPWidthOffset.Data.ToString()), int.Parse(_txtCPHeight.Data.ToString()) + int.Parse(_txtCPHeightOffset.Data.ToString()));          
        }// DisplayChart

        //-----------------------------------------
        //
        //-----------------------------------------
        public void OnPopupClose()
        {      
             if (Page.DataContract.GetValueByName("envelopMainInDM") != null)
                    _envDataEnvelop.SS_DataPacket = Page.DataContract.GetValueByName("envelopMainInDM") as DataPacket;

             if (_envDataEnvelop.SS_DataPacket.IsAlertMessageAvailable)
                 ShowAlerts();
             else
             {
                 UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;

                 SPCTxnDataMaint_Info objServiceInfo = new SPCTxnDataMaint_Info();
                 SPCTxnDataMaint objServiceData = new SPCTxnDataMaint();
                 SPCTxnDataChanges objChanges = new SPCTxnDataChanges();
                 SPCTxnDataChanges_Info objChangesInfo = new SPCTxnDataChanges_Info
                 {
                     SPCFailureAction = FieldInfoUtil.RequestValue(),
                     SPCResultFilename = FieldInfoUtil.RequestValue(),
                     SPCResult = FieldInfoUtil.RequestValue(),
                     FailureDocumentSet = FieldInfoUtil.RequestValue()
                 };
                 objServiceInfo.ObjectChanges = objChangesInfo;
                 objServiceData.ObjectToChange = new NamedObjectRef(_txtNameField.Data.ToString());

                 SPCTxnDataMaintService objService = new SPCTxnDataMaintService(profile);
                 SPCTxnDataMaint_Result result = null;
                 ResultStatus resultStatus = objService.Load(objServiceData, new SPCTxnDataMaint_Request { Info = objServiceInfo }, out result);

                 if (resultStatus.IsSuccess)
                 {
                    // DisplayValues(result.Value);                    
                     _txtSPCFailureActionField.Data = result.Value.ObjectChanges.SPCFailureAction;
                     _txtSPCResultFileNameField.Data = result.Value.ObjectChanges.SPCResultFilename;
                     _txtSPCResultField.Data = result.Value.ObjectChanges.SPCResult;
                     _ndoFailureDocumentSetField.Data = result.Value.ObjectChanges.FailureDocumentSet;
                 }
                 
                 DisplayMessage(resultStatus);                 
             }
        } // OnPopupClose

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

            objServiceData.ObjectToChange = _ndoFailureDocumentSetField.Data as NamedObjectRef;
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

                for (int i = 0; i<intDocCount; i++)
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
                        objDocuments[i].DocumentBrowseMode = resultDoc.Value.ObjectChanges.BrowseMode;
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
        public void ShowAlerts()
        {
            Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
            objAction.PageName = "SS_AlertMessagePopupVP";

            objAction.FrameLocation = new UIFloatingPageLocation();
            objAction.FrameLocation.Width = 430;
            objAction.FrameLocation.Height = 300;

            UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[1];
            objLinks[0] = new UIComponentDataContractLink();
            objLinks[0].SourceMember = "envelopMainOutDM";
            objLinks[0].TargetMember = "envelopAlertInDM";
            objAction.DataContractMap = new UIComponentDataContractMap();
            objAction.DataContractMap.Links = objLinks;

            UIComponentDataContractReturnLink[] objReturnLinks = new UIComponentDataContractReturnLink[1];
            objReturnLinks[0] = new UIComponentDataContractReturnLink();
            objReturnLinks[0].SourceMember = "envelopAlertOutDM";
            objReturnLinks[0].TargetMember = "envelopMainInDM";
            objAction.DataContractReturnMap = new UIComponentDataContractReturnMap();
            objAction.DataContractReturnMap.ReturnLinks = objReturnLinks;

            SEMI.AppCode.UIUtility.SetHorizonAlertPopupFrameLocation(this, objAction);
        }  // ShowAlerts
        
    }
}



