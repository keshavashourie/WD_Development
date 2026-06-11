/* Copyright 2019 Siemens */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using SEMI.AppCode;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_AdHocWIPData : MatrixWebPart
    {
        //Controls Declaration
        protected CWC.Label _lblCreationTimeStamp { get { return Page.FindCamstarControl("CreationTimestampLabel") as CWC.Label; } }
        protected CWC.Label _lblCreationUsername { get { return Page.FindCamstarControl("CreationUsernameLabel") as CWC.Label; } }
        protected CWC.Label _lblTxnUsername { get { return Page.FindCamstarControl("TxnUsernameLabel") as CWC.Label; } }
        protected CWC.Label _lblTxnTimeStamp { get { return Page.FindCamstarControl("TxnTimestampLabel") as CWC.Label; } }
        protected CWC.NamedObject _ndoObjectType { get { return Page.FindCamstarControl("AdHocWIPData_ObjectType") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoWIPDataSetup { get { return Page.FindCamstarControl("AdHocWIPData_WIPDataSetup") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("AdHocWIPData_Employee") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoDisplayFilter { get { return Page.FindCamstarControl("AdHocWIPData_DisplayFilter") as CWC.NamedObject; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("ComputerNameField") as CWC.TextBox; } }
        protected CWC.TextBox _txtRecordSequence { get { return Page.FindCamstarControl("AdHocWIPData_RecordSequence") as CWC.TextBox; } }
        protected CWC.TextBox _txtObjectName { get { return Page.FindCamstarControl("AdHocWIPData_ObjectName") as CWC.TextBox; } }
        protected CWC.DropDownList _ddlObjectRevision { get { return Page.FindCamstarControl("AdHocWIPData_ObjectRevision") as CWC.DropDownList; } }
        protected CWC.CheckBox _chkShowHidden { get { return Page.FindCamstarControl("ShowHiddenCheckBox") as CWC.CheckBox; } }
        protected CWC.Button _btnPrevious { get { return Page.FindCamstarControl("PreviousButton") as CWC.Button; } }
        protected CWC.Button _btnRefresh { get { return Page.FindCamstarControl("RefreshButton") as CWC.Button; } }
        protected CWC.Button _btnNext { get { return Page.FindCamstarControl("NextButton") as CWC.Button; } }
        protected CWC.Button _btnRecordSequence { get { return Page.FindCamstarControl("ClearRecordSequenceButton") as CWC.Button; } }
        protected CWC.Button _btnSelectionPopup { get { return Page.FindCamstarControl("SelectionPopup") as CWC.Button; } }
        protected JQDataGrid _gridRecordSequence { get { return Page.FindCamstarControl("RecordSequenceGrid") as JQDataGrid; } }
        protected JQDataGrid _gridDetails { get { return Page.FindCamstarControl("AdHocWIPData_Details") as JQDataGrid; } }
        protected JQDataGrid _gridFullDetails { get { return Page.FindCamstarControl("AdHocWIPData_FullDetails") as JQDataGrid; } }
        protected DataEnvelopControl _envAdHocWIPDataEnvelop { get { return Page.FindCamstarControl("AdHocWIPData_Envelop") as DataEnvelopControl; } }
        protected CWC.Button _btnSubmit { get { return Page.FindCamstarControl("SubmitAction") as CWC.Button; } }
        protected CWC.TextBox _txtTxnFailureCount { get { return Page.FindCamstarControl("AdHocWIPData_ss_TxnFailureCount") as CWC.TextBox; } }

        //---------------------------------------------------
        // Clear Controls function
        //---------------------------------------------------
        public void ClearControls(int ClearFlag)
        {
            try
            {
                Page.StatusBar.ClearMessage();
                if (ClearFlag <= 10)
                {
                    _ndoWIPDataSetup.ClearData();
                    _ndoWIPDataSetup.ClearSelectionValues();
                    _ndoWIPDataSetup.ToolTip = "1";
                    _ddlObjectRevision.Visible = false;
                }
                if (ClearFlag <= 20)
                {
                    _btnRecordSequence.Enabled = false;
                    _txtRecordSequence.ClearData();
                    _txtObjectName.ClearData();
                    _txtObjectName.Enabled = true;
                    _btnSelectionPopup.Enabled = true;
                    _ddlObjectRevision.ClearData();
                    _ddlObjectRevision.Enabled = true;
                    _ndoDisplayFilter.ClearSelectionValues();
                    _ndoDisplayFilter.ClearData();
                    _gridDetails.ClearData();
                    _gridFullDetails.ClearData();
                }
                if (ClearFlag <= 30)
                {
                    _gridRecordSequence.ClearData();
                    _btnPrevious.Enabled = false;
                    _btnNext.Enabled = false;
                }
                if (ClearFlag <= 40)
                {
                    _ddlObjectRevision.ClearSelectionValues();
                    _ddlObjectRevision.ClearData();
                }
                if (ClearFlag == 50)
                {
                    _btnRecordSequence.Enabled = false;
                    _txtRecordSequence.ClearData();
                    _txtObjectName.ClearData();
                    _txtObjectName.Enabled = true;
                    _btnSelectionPopup.Enabled = true;
                    _ddlObjectRevision.ClearData();
                    _ddlObjectRevision.Enabled = true;
					FetchDetails();
                    //Delete the values in the WIPData Value column
                }
                if (ClearFlag == -1)
                {
                    _ndoEmployee.ClearData();
                    _ndoObjectType.ClearData();
                    Page.SetFocus(_ndoObjectType);
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Object Type Data Changed Event
        //-----------------------------------------
        public void ObjectTypeField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                ClearControls(10);
                if (_ndoObjectType.Data != null)
                    FetchData("ObjectType");
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // WIP Data Setup Data Changed Event
        //-----------------------------------------
        public void WIPDataSetup_DataChanged(object sender, EventArgs e)
        {
            try
            {
                ClearControls(20);
                if (_ndoWIPDataSetup.Data != null)
                {
                    FetchData("WIPDataSetup");
                    FetchDetails();
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Object Name Data Changed Event
        //-----------------------------------------
        public void ObjectName_DataChanged(object sender, EventArgs e)
        {
            try
            {
                ClearControls(40);
                if (_txtObjectName.Data != null)
                {
                    FetchData("ObjectName");
                    FetchDetails();
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Display Filter Data Changed Event
        //-----------------------------------------
        public void DisplayFilter_DataChanged(object sender, EventArgs e)
        {
            try
            {
                List<WIPDataDetails> newListWIPDataDetails = new List<WIPDataDetails>();
                newListWIPDataDetails = (_gridFullDetails.Data as WIPDataDetails[]).ToList();
                for (int i = 0; i < newListWIPDataDetails.Count; i++)
                {
                    if (newListWIPDataDetails[i].IsHidden == true)
                    {
                        if (_chkShowHidden.Data.Equals(false))
                        {
                            newListWIPDataDetails.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            if (_ndoDisplayFilter.Data != null)
                            {
                                if (newListWIPDataDetails[i].DisplayFilter != null)
                                {
                                    if (!newListWIPDataDetails[i].DisplayFilter.ToString().Equals(_ndoDisplayFilter.Data.ToString()))
                                    {
                                        newListWIPDataDetails.RemoveAt(i);
                                        i--;
                                    }
                                } //newListWIPDataDetails[i].DisplayFilter != null
                                else
                                {
                                    newListWIPDataDetails.RemoveAt(i);
                                    i--;
                                }
                            } //_ndoDisplayFilter.Data != null
                        }
                    } //newListWIPDataDetails[i].IsHidden == true
                    else
                    {
                        if (_ndoDisplayFilter.Data != null)
                        {
                            if (newListWIPDataDetails[i].DisplayFilter != null)
                            {
                                if (!newListWIPDataDetails[i].DisplayFilter.ToString().Equals(_ndoDisplayFilter.Data.ToString()))
                                {
                                    newListWIPDataDetails.RemoveAt(i);
                                    i--;
                                }
                            } //newListWIPDataDetails[i].DisplayFilter != null
                            else
                            {
                                newListWIPDataDetails.RemoveAt(i);
                                i--;
                            }
                        } //_ndoDisplayFilter.Data != null
                    } //else
                } //end for
                _gridDetails.ClearData();
                _gridDetails.Data = newListWIPDataDetails.ToArray();
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Show Hidden Check Box Data Changed Event
        //-----------------------------------------
        public void ShowHiddenCheckBox_DataChanged(object sender, EventArgs e)
        {
            try
            {
                DisplayFilter_DataChanged(sender, e);
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Record Sequence Grid Row Selected Event
        //-----------------------------------------
        public ResponseData RecordSequenceGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            try
            {
                ClearControls(40);
                if (_gridRecordSequence.SelectedRowID != null)
                {
                    _btnRecordSequence.Enabled = true;
                    _txtRecordSequence.Data = _gridRecordSequence.GridContext.GetCell(_gridRecordSequence.SelectedRowID, "RecordSequence").ToString();
                    _txtObjectName.Data = _gridRecordSequence.GridContext.GetCell(_gridRecordSequence.SelectedRowID, "ObjectName").ToString();
                    if (_ddlObjectRevision.Visible)
                    {
                        _ddlObjectRevision.Data = _gridRecordSequence.GridContext.GetCell(_gridRecordSequence.SelectedRowID, "ObjectRevision").ToString();
                        _ddlObjectRevision.Enabled = false;
                    }
                    _txtObjectName.Enabled = false;
                    _btnSelectionPopup.Enabled = false;
                    FetchDetails();
                }
                return null;
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
                return null;
            }
        }

        //---------------------------------------------------
        // Fetch Data function
        //---------------------------------------------------
        public void FetchData(string EventName, int BlockRows = 10)
        {
            try
            {
                //Initialize Service & Objects
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                AdHocWIPDataService Svc = new AdHocWIPDataService(profile);
                AdHocWIPData SvcData = new AdHocWIPData();
                AdHocWIPData_Info SvcInfo = new AdHocWIPData_Info();
                AdHocWIPData_Request ReqData = new AdHocWIPData_Request();
                AdHocWIPData_Result ResData = new AdHocWIPData_Result();
                int iBlock = 1;

                //Prepare the query
                if (EventName == "ObjectType")
                {
                    SvcData.ObjectType = new NamedObjectRef();
                    SvcData.ObjectType.Name = _ndoObjectType.Data.ToString();
                    SvcInfo.WIPDataSetup = new Info(true);
                    SvcInfo.WIPDataSetup.RequestSelectionValues = true;
                }
                else if (EventName == "WIPDataSetup" || EventName == "Previous" || EventName == "Refresh" || EventName == "Next")
                {
                    SvcData.ObjectType = new NamedObjectRef();
                    SvcData.ObjectType.Name = _ndoObjectType.Data.ToString();
                    SvcData.WIPDataSetup = new NamedObjectRef();
                    SvcData.WIPDataSetup.Name = _ndoWIPDataSetup.Data.ToString();
                    if (IsNumber(_ndoWIPDataSetup.ToolTip))
                        iBlock = Convert.ToInt32(_ndoWIPDataSetup.ToolTip);
                    if (EventName == "Previous")
                    {
                        if (iBlock > 1)
                            iBlock = iBlock - 1;
                    }
                    else if (EventName == "Next")
                    {
                        iBlock = iBlock + 1;
                    }
                    _ndoWIPDataSetup.ToolTip = iBlock.ToString();
                    SvcData.STARTROWNUM = (BlockRows * (iBlock - 1)) + 1;
                    SvcData.STOPROWNUM = (BlockRows * iBlock) + 1;
                    SvcInfo.RecordSequence = new Info(true);
                    SvcInfo.RecordSequence.RequestSelectionValues = true;
                    SvcInfo.DisplayFilter = new Info(true);
                    SvcInfo.DisplayFilter.RequestSelectionValues = true;
                    _btnPrevious.Enabled = (iBlock > 1);
                    _btnNext.Enabled = false;
                }
                else if (EventName == "ObjectName")
                {
                    SvcData.ObjectType = new NamedObjectRef();
                    SvcData.ObjectType.Name = _ndoObjectType.Data.ToString();
                    SvcData.ObjectName = _txtObjectName.Data.ToString();
                    SvcInfo.ObjectRevision = new Info(true);
                    SvcInfo.ObjectRevision.RequestSelectionValues = true;
                }
                ReqData.Info = SvcInfo;

                //Execute Request
                ResultStatus Results;
                Results = Svc.GetEnvironment(SvcData, ReqData, out ResData);

                //Result
                if (Results.IsSuccess)
                {
                    if (EventName == "ObjectType")
                    {
                        //Display data set for WIPDataSetupField
                        if (ResData.Environment.WIPDataSetup.SelectionValues != null)
                        {
                            if (ResData.Environment.WIPDataSetup.SelectionValues.Rows != null)
                                _ndoWIPDataSetup.Data = ResData.Environment.WIPDataSetup.SelectionValues.Rows[0].Values[0].ToString();
                        }

                        if (_ndoWIPDataSetup.Data != null)
                            WIPDataSetup_DataChanged(null, null);

                        //Fetch other data
                        SvcInfo = new AdHocWIPData_Info();
                        SvcInfo.ObjectRevisionRequired = new Info(true);
                        ReqData.Info = SvcInfo;
                        Results = Svc.Load(SvcData, ReqData, out ResData);
                        if (Results.IsSuccess)
                        {
                            _ddlObjectRevision.Visible = (ResData.Value.ObjectRevisionRequired == true);
                        }
                        else
                        {
                            this.DisplayMessage(Results);
                        }
                    } //EventName == "ObjectType"
                    else if (EventName == "WIPDataSetup" || EventName == "Previous" || EventName == "Refresh" || EventName == "Next")
                    {
                        if (ResData.Environment.RecordSequence.SelectionValues != null)
                        {
                            if (ResData.Environment.RecordSequence.SelectionValues.Rows != null)
                            {
                                _btnNext.Enabled = (ResData.Environment.RecordSequence.SelectionValues.Rows.Count() > BlockRows);
                                _gridRecordSequence.ClearData();
                                JQDataGrid _tempRecordSequence = _gridRecordSequence;
                                GridUtility.SelectionValuesGrid_AddDataRow(ref _tempRecordSequence, ResData.Environment.RecordSequence.SelectionValues);
                                //_gridRecordSequence.Data = ResData.Environment.RecordSequence.SelectionValues;
                                if (ResData.Environment.DisplayFilter.SelectionValues != null)
                                {
                                    if (ResData.Environment.DisplayFilter.SelectionValues.Rows != null)
                                    {
                                        NamedObjectRef[] displayFilters = new NamedObjectRef[ResData.Environment.DisplayFilter.SelectionValues.Rows.Count()];
                                        for (int i = 0; i < ResData.Environment.DisplayFilter.SelectionValues.Rows.Count(); i++)
                                        {
                                            displayFilters[i] = new NamedObjectRef();
                                            displayFilters[i].Name = ResData.Environment.DisplayFilter.SelectionValues.Rows[i].Values[0];
                                        }
                                        CWC.NamedObject _tempDisplayFilter = _ndoDisplayFilter;
                                        ControlsUtility.NamedObjectControl_SetSelectionValues(ref _tempDisplayFilter, displayFilters);
                                        _ndoDisplayFilter.ClearData();
                                    }
                                }
                            }
                        }
                    }
                    else if (EventName == "ObjectName")
                    {
                        if (ResData.Environment.ObjectRevision.SelectionValues != null)
                        {
                            if (ResData.Environment.ObjectRevision.SelectionValues.Rows != null)
                            {
                                //Display Revision
                                _ddlObjectRevision.SetSelectionValues(ResData.Environment.ObjectRevision.SelectionValues);
                                _ddlObjectRevision.Data = ResData.Environment.ObjectRevision.SelectionValues.Rows[0].Values[2];
                                CamstarWebControl.SetRenderToClient(_ddlObjectRevision);
                            }
                        }
                    }
                } //Results.IsSuccess
                else
                {
                    this.DisplayMessage(Results);
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //---------------------------------------------------
        // Fetch Details function
        //---------------------------------------------------
        public void FetchDetails()
        {
            try
            {
                //Initialize Service & Objects
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                AdHocWIPDataService Svc = new AdHocWIPDataService(profile);
                AdHocWIPData SvcData = new AdHocWIPData();
                AdHocWIPData_Info SvcInfo = new AdHocWIPData_Info();
                AdHocWIPData_Request ReqData = new AdHocWIPData_Request();
                AdHocWIPData_Result ResData = new AdHocWIPData_Result();

                //Prepare the request
                if (_ndoObjectType.Data != null)
                {
                    SvcData.ObjectType = new NamedObjectRef();
                    SvcData.ObjectType.Name = _ndoObjectType.Data.ToString();
                }
                if (_txtObjectName.Data != null)
                    SvcData.ObjectName = _txtObjectName.Data.ToString();
                if (_ddlObjectRevision.Data != null)
                    SvcData.ObjectRevision = _ddlObjectRevision.Data.ToString();
                if (_ndoWIPDataSetup.Data != null)
                {
                    SvcData.WIPDataSetup = new NamedObjectRef();
                    SvcData.WIPDataSetup.Name = _ndoWIPDataSetup.Data.ToString();
                }
                if (_txtRecordSequence.Data != null)
                    SvcData.RecordSequence = Convert.ToInt32(_txtRecordSequence.Data.ToString());
                SvcInfo.DetailsSelection = new WIPDataDetails_Info();
                SvcInfo.DetailsSelection.WIPDataName = new Info(true);
				SvcInfo.DetailsSelection.ss_UOM = new Info(true);
                SvcInfo.DetailsSelection.WIPDataValue = new Info(true);
                SvcInfo.DetailsSelection.ForProcessType = new Info(true);
                SvcInfo.DetailsSelection.IsRequired = new Info(true);
                SvcInfo.DetailsSelection.IsHidden = new Info(true);
                SvcInfo.DetailsSelection.WaferScribeNumber = new Info(true);
                SvcInfo.DetailsSelection.DisplayFilter = new Info(true);

                //Request the data
                ReqData.Info = SvcInfo;

                //Execute Request
                ResultStatus Results;
                Results = Svc.Load(SvcData, ReqData, out ResData);

                //Result
                if (Results.IsSuccess)
                {
                    if (ResData.Value.DetailsSelection != null)
                    {
                        _gridFullDetails.ClearData();
                        _gridFullDetails.Data = ResData.Value.DetailsSelection;
                        //WIPDataDetails[] newWIPDataDetails = ResData.Value.DetailsSelection;
                        List<WIPDataDetails> newListWIPDataDetails = new List<WIPDataDetails>();
                        newListWIPDataDetails = ResData.Value.DetailsSelection.ToList();
                        for (int i = 0; i < newListWIPDataDetails.Count; i++)
                        {
                            if (newListWIPDataDetails[i].IsHidden == true && _chkShowHidden.Data.Equals(false))
                            {
                                newListWIPDataDetails.RemoveAt(i);
                                i--;
                            }
                        }
                        _gridDetails.ClearData();
                        _gridDetails.Data = newListWIPDataDetails.ToArray();
                    }
                }
                else
                {
                    this.DisplayMessage(Results);
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Display SPC Chart Function
        //-----------------------------------------
        public void DisplaySPCChart(SPCTxnData[] oSPCTxnData, ResultStatus TxnResultStatus)
        {
            if (oSPCTxnData.Length > 0)
            {
                int intCHeight = 0;
                int intCWidth = 0;

                // set the data envelop data to pass to the popup;            
                SEMI.AppCode.Services.SPCTxn.SPCTxnDataSet SPCTxnDataSet = new SEMI.AppCode.Services.SPCTxn.SPCTxnDataSet();
                foreach (SPCTxnData oSPCTxnDataItem in oSPCTxnData)
                {
                    if (oSPCTxnDataItem.SPCResultFilename != "")
                        SPCTxnDataSet.AddDataListItem(oSPCTxnDataItem);

                    if (int.Parse(oSPCTxnDataItem.ChartHeight.ToString()) > intCHeight)
                        intCHeight = int.Parse(oSPCTxnDataItem.ChartHeight.ToString());

                    if (int.Parse(oSPCTxnDataItem.ChartWidth.ToString()) > intCWidth)
                        intCWidth = int.Parse(oSPCTxnDataItem.ChartWidth.ToString());
                }

                DataPacket SS_DataPacket = new DataPacket();
                if (_envAdHocWIPDataEnvelop.SS_DataPacket != null)
                    SS_DataPacket = _envAdHocWIPDataEnvelop.SS_DataPacket;

                SS_DataPacket.SPCTxnDataSet = SPCTxnDataSet;

                // check for alert messages
                string[] sAlertMessages;
                string sCompletionMessage;
                // check for alert messages               
                bool bAlertMsg = SEMI.AppCode.UIUtility.AlertMessagesAvailable(TxnResultStatus.Message, out sAlertMessages, out sCompletionMessage);

                if (bAlertMsg)
                    SS_DataPacket.AlertMessages = sAlertMessages;
                SS_DataPacket.ResultStatusMessage = sCompletionMessage;
                SS_DataPacket.IsErrorResultStatusMessage = !(TxnResultStatus.IsSuccess);
                SS_DataPacket.IsAlertMessageAvailable = true;
                SS_DataPacket.AlertMessages = sAlertMessages;

                // set the data packet
                _envAdHocWIPDataEnvelop.SS_DataPacket = SS_DataPacket;

                UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[1];
                objLinks[0] = new UIComponentDataContractLink();
                objLinks[0].SourceMember = "AdhocWIPData_DataEnvelop_DM";
                objLinks[0].TargetMember = "envelopPopupInDM";

                UIComponentDataContractReturnLink[] objReturnLinks = new UIComponentDataContractReturnLink[1];
                objReturnLinks[0] = new UIComponentDataContractReturnLink();
                objReturnLinks[0].SourceMember = "envelopPopupOutDM";
                objReturnLinks[0].TargetMember = "AdhocWIPData_DataEnvelop_DM";

                SEMI.AppCode.Services.SPCTxn.PopupSPCChart(this, objLinks, objReturnLinks, (intCWidth + 110), (intCHeight + 200));
            }
        }

        //---------------------------------------------------
        // Submit Button Codes
        //---------------------------------------------------
        public ResultStatus SubmitTransactions()
        {
            try
            {
                //Initialize Service & Objects
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                AdHocWIPDataService Svc = new AdHocWIPDataService(profile);
                AdHocWIPData SvcData = new AdHocWIPData();
                AdHocWIPData_Info SvcInfo = new AdHocWIPData_Info();
                AdHocWIPData_Request ReqData = new AdHocWIPData_Request();
                AdHocWIPData_Result ResData = new AdHocWIPData_Result();

                //Prepare data to be submitted
                Page.GetInputData(SvcData);
                if (_gridFullDetails.Data != null)
                {
                    WIPDataDetails[] getServiceDetails = _gridFullDetails.Data as WIPDataDetails[];
                    SvcData.Details = new WIPDataDetails[getServiceDetails.Count()];
                    int svcIndex = 0;
                    foreach (WIPDataDetails detail in getServiceDetails)
                    {
                        SvcData.Details[svcIndex] = new WIPDataDetails();
                        SvcData.Details[svcIndex].WIPDataName = detail.WIPDataName;
						SvcData.Details[svcIndex].ss_UOM = detail.ss_UOM;
                        SvcData.Details[svcIndex].WIPDataValue = detail.WIPDataValue;
                        SvcData.Details[svcIndex].ForProcessType = detail.ForProcessType;
                        svcIndex++;
                    }
                }

                //Add request information
                SvcInfo.SPCTxnDataList = new SPCTxnData_Info();
                SvcInfo.SPCTxnDataList.Name = new Info(true);
                SvcInfo.SPCTxnDataList.SPCSetup = new Info(true);
                SvcInfo.SPCTxnDataList.SPCResult = new Info(true);
                SvcInfo.SPCTxnDataList.SPCResultFilename = new Info(true);
                SvcInfo.SPCTxnDataList.ChartHeight = new Info(true);
                SvcInfo.SPCTxnDataList.ChartWidth = new Info(true);
                ReqData.Info = SvcInfo;

                //Execute Transaction 
                ResultStatus Results = Svc.ExecuteTransaction(SvcData, ReqData, out ResData);

                //Result
                if (Results.IsSuccess)
                {
                    // SPC stuff
                    bool bSPCAvailable = false;
                    SPCTxnData[] oSPCTxnData = null;
                    string sCompletionMessage = Results.Message;

                    if (ResData.Value.SPCTxnDataList != null)
                    {
                        oSPCTxnData = ResData.Value.SPCTxnDataList;
                        if (oSPCTxnData.Length > 0)
                            bSPCAvailable = true;
                    }
                    
                    if (bSPCAvailable)
                    {
                        DisplaySPCChart(oSPCTxnData, Results);
                    }
                    else
                    {
                        DisplayAlerts(Results, out sCompletionMessage);
                        Results.Message = sCompletionMessage;
                    }

                    //Display status message
                    ClearControls(50);
                    return Results;
                }
                else
                {
                    return Results;
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
                return null;
            }
        }

        //---------------------------------------------------
        // Web part custom action
        //---------------------------------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            try
            {
                base.WebPartCustomAction(sender, e);
                var action = e.Action as CustomAction;
                if (action != null)
                {
                    switch (action.Parameters)
                    {
                        case "Submit":
                            {
                                e.Result = SubmitTransactions();
                                break;
                            }
                        case "Previous":
                            {
                                ClearControls(30);
                                if (_ndoWIPDataSetup.Data != null)
                                    FetchData("Previous");
                                break;
                            }
                        case "Refresh":
                            {
                                ClearControls(30);
                                if (_ndoWIPDataSetup.Data != null)
                                    FetchData("Refresh");
                                break;
                            }
                        case "Next":
                            {
                                ClearControls(30);
                                if (_ndoWIPDataSetup.Data != null)
                                    FetchData("Next");
                                break;
                            }
                        case "ClearRecordSequence":
                            {
                                ClearControls(50);
                                _gridRecordSequence.SelectedRowID = null;
                                break;
                            }
                        case "Reset":
                            {
                                Page.ShopfloorReset(sender, e);
                                _chkShowHidden.ClearData();
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        public void OnPopupClose()
        {
            if (Page.DataContract.GetValueByName("AdhocWIPData_DataEnvelop_DM") != null)
            {
                DataPacket oData = Page.DataContract.GetValueByName("AdhocWIPData_DataEnvelop_DM") as DataPacket;
				if (oData.AlertMessages != null && oData.AlertMessages.GetLength(0) != 0)
                {
                    PopupWIPMessages(false);
                }
                if (oData.ResultStatusMessage != null)
                {
                    Page.StatusBar.WriteSuccess(oData.ResultStatusMessage);
                }
            }
            //UpdateParams();
        }

        public virtual void PopupWIPMessages(bool EndResponse = false)
        {
            Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
            objAction.PageName = "SS_AlertMessagePopupVP";

            objAction.FrameLocation = new UIFloatingPageLocation();
            objAction.FrameLocation.Width = 430;
            objAction.FrameLocation.Height = 230;
            objAction.EndResponse = EndResponse;
            objAction.ShowButtons = false;

            UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[1];
            objLinks[0] = new UIComponentDataContractLink();
            objLinks[0].SourceMember = "AdhocWIPData_DataEnvelop_DM";
            objLinks[0].TargetMember = "envelopAlertInDM";
            objAction.DataContractMap = new UIComponentDataContractMap();
            objAction.DataContractMap.Links = objLinks;

            //UIComponentDataContractReturnLink[] objReturnLinks = new UIComponentDataContractReturnLink[1];
            //objReturnLinks[0] = new UIComponentDataContractReturnLink();
            //objReturnLinks[0].SourceMember = "envelopAlertOutDM";
            //objReturnLinks[0].TargetMember = "PartRequestMain_Envelope";
            //objAction.DataContractReturnMap = new UIComponentDataContractReturnMap();
            //objAction.DataContractReturnMap.ReturnLinks = objReturnLinks;

            SEMI.AppCode.UIUtility.SetHorizonAlertPopupFrameLocation(this, objAction);
        }  // ShowAlerts

        //---------------------------------------------------
        // Override OnLoad event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                Page.StatusBar.ClearMessage();
                base.OnLoad(e);
                if (!Page.IsPostBack)
                {
                    ClearControls(-1);
                    _txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
                }
                _ndoObjectType.DataChanged += new EventHandler(ObjectTypeField_DataChanged);
                _ndoWIPDataSetup.DataChanged += new EventHandler(WIPDataSetup_DataChanged);
                _txtObjectName.DataChanged += new EventHandler(ObjectName_DataChanged);
                _ndoDisplayFilter.DataChanged += new EventHandler(DisplayFilter_DataChanged);
                _chkShowHidden.DataChanged += new EventHandler(ShowHiddenCheckBox_DataChanged);
                _gridRecordSequence.RowSelected += RecordSequenceGrid_RowSelected;
                if (_ndoObjectType.Data != null && _ndoWIPDataSetup.Data != null)
                    _btnSelectionPopup.Enabled = true;
                else
                    _btnSelectionPopup.Enabled = false;
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }

            if (SEMI.AppCode.UIUtility.IsPopupClose(this))
            {
                OnPopupClose();
                Page.DataContract.SetValueByName("AdhocWIPData_DataEnvelop_DM", null);
            }
        }

        //---------------------------------------------------
        // IsNumber function
        //---------------------------------------------------
        public bool IsNumber(String value)
        {
            return value.ToCharArray().Where(x => !Char.IsDigit(x)).Count() == 0;
        }

        //------------------------
        // Display alerts function
        //------------------------
        public void DisplayAlerts(ResultStatus status, out string CompletionMessage)
        {
            // check for alert messages
            string[] sAlertMessages;
            string sCompletionMessage;

            if (status.IsSuccess)
            {
                bool bAlertMsg = SEMI.AppCode.UIUtility.AlertMessagesAvailable(status.Message, out sAlertMessages, out sCompletionMessage);
                status.Message = sCompletionMessage;

                if (bAlertMsg)
                {
                    SEMI.AppCode.DataPacket oData = new DataPacket();
                    oData.IsAlertMessageAvailable = false;
                    oData.AlertMessages = sAlertMessages;
                    oData.ResultStatusMessage = status.Message;
                    _envAdHocWIPDataEnvelop.SS_DataPacket = oData;
                    PopupWIPMessages(false);
                }
            }
            CompletionMessage = status.Message;
        }

        //-------------------------
        // Display alerts function
        //-------------------------
        public void DisplayAlerts(string[] AlertMessages)
        {
            SEMI.AppCode.DataPacket oData = new DataPacket();
            oData.IsAlertMessageAvailable = false;
            oData.AlertMessages = AlertMessages;
            _envAdHocWIPDataEnvelop.SS_DataPacket = oData;
        }
    }
}



