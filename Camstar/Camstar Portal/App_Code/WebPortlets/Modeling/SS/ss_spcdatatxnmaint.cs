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
    /// Summary description for SS_SPCDataTxnMaint
    /// </summary>
    public class SS_SPCDataTxnMaint: MatrixWebPart 
    {
        protected JQDataGrid _gridSPCParamsGrid { get { return Page.FindCamstarControl("AdHocSPC_SPCParams") as JQDataGrid; } }
        protected CWC.NamedObject _ndoSPCSetupField { get { return Page.FindCamstarControl("ObjectChanges_SPCSetup") as CWC.NamedObject; } }
        protected CWC.TextBox _txtNameField { get { return Page.FindCamstarControl("ObjectChanges_Name") as CWC.TextBox; } }
        protected CWC.TextBox _txtSPCFailureActionField { get { return Page.FindCamstarControl("SPCTxnDataList_SPCFailureAction") as CWC.TextBox; } }
        protected CWC.TextBox _txtSPCResultField { get { return Page.FindCamstarControl("ObjectChanges_SPCResult") as CWC.TextBox; } }
        protected CWC.TextBox _txtSPCResultFileNameField { get { return Page.FindCamstarControl("ObjectChanges_SPCResultFilename") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoEmployeeField { get { return Page.FindCamstarControl("AdHocSPC_Employee") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoFailureDocumentSetField { get { return Page.FindCamstarControl("ObjectChanges_FailureDocumentSet") as CWC.NamedObject; } }
        protected DataEnvelopControl _envDataEnvelop { get { return Page.FindCamstarControl("ssDataEnvelop") as DataEnvelopControl; } }      
        protected CWC.ViewDocumentsControl _docSPCFailureDocumentSetView { get { return Page.FindCamstarControl("FailDocSetViewer") as CWC.ViewDocumentsControl; } }
        protected CWC.TextBox _txtCPHeight { get { return Page.FindCamstarControl("ObjectChanges_ChartHeight") as CWC.TextBox; } }
        protected CWC.TextBox _txtCPWidth { get { return Page.FindCamstarControl("ObjectChanges_ChartWidth") as CWC.TextBox; } }
        protected CWC.TextBox _txtCPHeightOffset { get { return Page.FindCamstarControl("ChartHeightOffset") as CWC.TextBox; } }
        protected CWC.TextBox _txtCPWidthOffset { get { return Page.FindCamstarControl("ChartWidthOffset") as CWC.TextBox; } }      
        protected CWC.Button btnDisplay { get { return Page.FindCamstarControl("btnDisplay") as CWC.Button; } }
        

        //-----------------------------------------
        //
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            btnDisplay.Click += new EventHandler(btnDisplay_Click);           
            if (Page.IsPostBack)
            {                               
                //_ndoSPCSetupField.DataChanged += delegate { LoadDependentControls(); };
                
                //if (SEMI.AppCode.UIUtility.IsPopupClose(this))              
                //    OnPopupClose();                               
            }
            else
            {
                AddDataEnvelopDataMember();
            }
        }

        void btnViewFailureDocumentSet_Click(object sender, EventArgs e)
        {
            LoadDocumentSet();
        }

        void btnDisplay_Click(object sender, EventArgs e)
        {
            DisplayChart();
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
            objPageDataMembers[intDMIndex].Key = "MDL_Specific.ssDataEnvelop";
            objPageDataMembers[intDMIndex].Name = "envelopMainOutDM";
            objPageDataMembers[intDMIndex].ConnectionType = DataMemberConnectionType.Control;
            objPageDataMembers[intDMIndex].Property = "SS_DataPacket";
            intDMIndex++;

            // add the new envelop data member for data to be retrieved FROM popup
            objPageDataMembers[intDMIndex] = new UIComponentDataMember();
            objPageDataMembers[intDMIndex].Key = "MDL_Specific.ssDataEnvelop";
            objPageDataMembers[intDMIndex].Name = "envelopMainInDM";
            objPageDataMembers[intDMIndex].ConnectionType = DataMemberConnectionType.Control;
            objPageDataMembers[intDMIndex].Property = "SS_DataPacket";

            Page.DataContract.DataMembers = objPageDataMembers;
        } // AddDataEnvelopDataMember

        //-----------------------------------------
        //
        //-----------------------------------------
        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
        } // WebPartCustomAction  

  
        //-----------------------------------------
        public void DisplayChart()
        {
            if (_txtSPCResultFileNameField.Data != null)
            {
                int iChartHeight = 600;
                int iChartWidth = 800;

                try { iChartHeight = int.Parse(_txtCPHeight.Data.ToString()); }
                catch { iChartHeight = 600; }

                try { iChartWidth = int.Parse(_txtCPWidth.Data.ToString()); }
                catch { iChartWidth = 800; }

                // set the data envelop data to pass to the popup;            
                SPCTxnData oSPCData = new SPCTxnData();
                oSPCData.Name = _txtNameField.Data.ToString();
                oSPCData.SPCResult = _txtSPCResultField.Data != null ? _txtSPCResultField.Data.ToString() : "";
                oSPCData.SPCSetup = _ndoSPCSetupField.Data != null ? new NamedObjectRef(_ndoSPCSetupField.Data.ToString()) : null;
                oSPCData.SPCResultFilename = _txtSPCResultFileNameField.Data.ToString();
                oSPCData.ChartHeight = iChartHeight;
                oSPCData.ChartWidth = iChartWidth;
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

                int iPopupHeight = iChartHeight + int.Parse(_txtCPHeightOffset.Data.ToString());
                int iPopupWidth = iChartWidth + int.Parse(_txtCPWidthOffset.Data.ToString());

                SEMI.AppCode.Services.SPCTxn.PopupSPCChart(this, objLinks, objReturnLinks, iPopupWidth, iPopupHeight);
            }
        }// DisplayChart
 
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



