/* Copyright 2024 Siemens */
using System;
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
using Camstar.WebPortal.PortalFramework;

/// <summary>
/// Summary description for SS_WIPMain
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_WIPMain : SS_WIPMainFramework
    {
        private int ConstContainersPerBatch = 20;
        protected override void OnLoad(EventArgs e)
        {
            if (!Page.IsPostBack)
                Page.PortalContext.LocalSession["EsigPostback"] = false;

            bool IsEsigValid = true;
            UIComponentDataContract dataContract = Page.PortalContext.DataContract;
            var allEsigDetails =
                    (Tuple<ESigServiceDetail[], ESigProcessTimerServiceDetail[]>)dataContract.GetValueByName("ESigCaptureDetailsDM");
            ESigServiceCaptureWrapper[] gridCaptures =
                    (ESigServiceCaptureWrapper[])dataContract.GetValueByName("ESigCaptureDM");


            if (gridCaptures != null && allEsigDetails != null && allEsigDetails.Item2 != null)
            {
                foreach (var capture in gridCaptures)
                {
                    if (capture.IsValid == false)
                    {
                        IsEsigValid = false;
                        break;
                    }
                }
                foreach (var ESigProcessTimerServiceDetail in allEsigDetails.Item2)
                {
                    foreach (var item in ESigProcessTimerServiceDetail.ESigProcessTimerDtls)
                        if (item.CaptureDetails == null)
                        {
                            item.CaptureDetails = gridCaptures.Where(c => c.RequirementID == item.ESigReqDetail.ID).Select(c => c.Capture).ToArray();
                        }
                }
            }
            dataContract.SetValueByName("ESigCaptureDetailsDM", allEsigDetails);

            var tuple = ESigCaptureUtil.CollectESigServiceDetailsAll();
            if ((bool)Page.PortalContext.LocalSession["EsigPostback"] && IsEsigValid && allEsigDetails.Item2 != null)
                SubmitTransactions();
        }

        //---------------------------------------------------
        // Clear Button Codes
        //---------------------------------------------------
        public override void ClearControls(int ClearFlag = 0, bool SkipDisplayMessage = false)
        {
            base.ClearControls(ClearFlag, SkipDisplayMessage);
        }


        //---------------------------------------------------
        // GetEsigRequirementMaintenanceTxn Codes
        //---------------------------------------------------
        protected virtual void GetEsigRequirementMaintenanceTxn(string servName, string eSigMaintAction, Personalization.DependsOnItem[] dependencies)
        {

            if (!string.IsNullOrEmpty(eSigMaintAction))
            {
                WSDataCreator creator = new WSDataCreator();
                IWCFService service = creator.CreateService(servName, FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                Request request = WCFObject.CreateObject(servName + "_Request") as Request;
                WCFObject wcfo = new WCFObject(request);
                Info info = WCFObject.CreateObject(servName + "_Info") as Info;
                int iLotsPerBatch = 0;

                wcfo.SetValue("Info", info);
                wcfo.SetValue("Info.ESigRequirement", new Info(true));
                wcfo.SetValue("Info.ESigDetails", new ESigServiceDetail_Info { RequestValue = true });
                wcfo.SetValue("Info.ss_ESigProcessTimerDetails", new ESigProcessTimerServiceDetail_Info { RequestValue = true });
                var txn = creator.CreateServiceData(servName);
                (txn as WIPMain).WIPFlag = Convert.ToInt32(GetWIPFlag());
                //Determine the number of lots to transact in a batch
                //Batch tracking is disabled for Track In (this means ALL lots will be transacted together)
                //if ((ConstContainersPerBatch > 0) && ((_gridLotData.GridContext.GetTotalRows() > ConstContainersPerBatch) && ((txn as WIPMain).WIPFlag != 1)))
                //{
                //    iLotsPerBatch = ConstContainersPerBatch;
                //}
                //else
                //{
                iLotsPerBatch = _gridLotData.GridContext.GetTotalRows();
                //}

                //Add the Containers
                ContainerRef[] containerNames = new ContainerRef[iLotsPerBatch];
                for (int i = 0; i < iLotsPerBatch; i++)
                {
                    containerNames[i] = new ContainerRef();
                    containerNames[i].Name = _gridLotData.GridContext.GetCell(i.ToString().PadLeft(6, '0'), "Container").ToString();
                }
                (txn as WIPMain).Containers = containerNames;

                //Set the Process Type, WIP Flag and Comments
                if (_ndoProcessType.Data != null)
                {
                    NamedObjectRef processType = new NamedObjectRef();
                    processType.Name = _ndoProcessType.Data.ToString();

                    (txn as WIPMain).ProcessType = processType;
                }

                //Add common data to submit
                if (_ndoEmployee.Data != null)
                    (txn as WIPMain).Employee = new NamedObjectRef(_ndoEmployee.Data.ToString());

                if (_txtComputerName.Data != null)
                    (txn as WIPMain).ComputerName = _txtComputerName.Data.ToString();

                if (_txtCommentsField.Data != null)
                    (txn as WIPMain).Comments = _txtCommentsField.Data.ToString();

                var parameters = WCFObject.CreateObject(string.Format("{0}_LoadESigDetails_Parameters", servName)) as Parameters;
                WCFObject wcfoParams = new WCFObject(parameters);
                wcfoParams.SetValue("ESigMaintAction", eSigMaintAction);


                Result res = null;
                ResultStatus status = (service as IShopFloorBase).LoadESigDetails(txn, parameters, request, out res);

                if (status.IsSuccess && res.Value != null)
                {
                    var wcfoRes = new WCFObject(res);
                    var EsigDetails = wcfoRes.GetValue("Value.ESigDetails") as ESigServiceDetail[];
                    var EsigProcessTimerServiceDetails = wcfoRes.GetValue("Value.ss_ESigProcessTimerDetails") as ESigProcessTimerServiceDetail[];
                    bool IsSignatureRequired = false;
                    if (EsigDetails != null)
                        foreach (var EsigDetail in EsigDetails)
                        {
                            if (EsigDetail.CaptureCount.Value < EsigDetail.SignatureCount.Value)
                            {
                                IsSignatureRequired = true;
                                break;
                            }
                        }
                    if (EsigProcessTimerServiceDetails != null)
                        foreach (var EsigProcessTimerServiceDetail in EsigProcessTimerServiceDetails)
                        {
                            if (EsigProcessTimerServiceDetail.ESigProcessTimerDtls != null)
                                foreach (var EsigDetail in EsigProcessTimerServiceDetail.ESigProcessTimerDtls)
                                {
                                    if (EsigDetail.CaptureCount.Value < EsigDetail.SignatureCount.Value)
                                    {
                                        IsSignatureRequired = true;
                                        break;
                                    }
                                }
                        }
                    if ((EsigDetails != null || EsigProcessTimerServiceDetails != null) && IsSignatureRequired)
                    {
                        List<ESigProcessTimerServiceDetail> abc = EsigProcessTimerServiceDetails.ToList();
                        Page.PortalContext.LocalSession["EsigPostback"] = true;

                        Page.OpenContainerPopupESigCapture(PrimaryServiceType, Tuple.Create(EsigDetails, EsigProcessTimerServiceDetails));

                    }

                }
                else if (status.IsSuccess == false)
                {
                    this.DisplayMessage(status);
                }


            }
        }

        //---------------------------------------------------
        // Submit Button Codes
        //---------------------------------------------------
        public void SubmitTransactions()
        {
            try
            {
                //GetEsig
                ESigServiceCaptureWrapper[] gridCaptures = (ESigServiceCaptureWrapper[])Page.DataContract.GetValueByName("ESigCaptureDM");
                bool IsEsigValid = true;
                if (gridCaptures != null)
                    foreach (var capture in gridCaptures)
                    {
                        if (capture.IsValid == false)
                        {
                            IsEsigValid = false;
                            break;
                        }
                    }
                if (!(bool)Page.PortalContext.LocalSession["EsigPostback"] || !IsEsigValid)
                    GetEsigRequirementMaintenanceTxn(_txtPrimaryServiceType.Data != null ? _txtPrimaryServiceType.Data.ToString() : "WIPMain", "0", null);
                Page.PortalContext.LocalSession["EsigPostback"] = false;


                //Initialize the Service Data
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                //string ServiceType = this.PrimaryServiceType;
                string sServiceType = _txtPrimaryServiceType.Data != null ? _txtPrimaryServiceType.Data.ToString() : "WIPMain";
                var Svc = new WSDataCreator().CreateService(sServiceType, profile);
                var SvcData = WCFObject.CreateObject(sServiceType) as ICreator;
                var SvcInfo = WCFObject.CreateObject(sServiceType + "_Info") as ICreator;
                var ReqData = WCFObject.CreateObject(sServiceType + "_Request") as ICreator;
                var ResData = WCFObject.CreateObject(sServiceType + "_Result") as ICreator;
                Result result = null;

                //Initialize the variables
                var cSPCTxnDataset = new List<string>();
                var cAlertMessages = new List<string>();
                int iLotsPerBatch = 0;
                int selectedRowCount = 0;
                string sWIPFlag = GetWIPFlag();


                //If ESig exist then submit it
                var tuple = ESigCaptureUtil.CollectESigServiceDetailsAll();
                ESigProcessTimerServiceDetail[] eSigDetails = tuple == null ? null : tuple.Item2;
                gridCaptures =
                    (ESigServiceCaptureWrapper[])Page.DataContract.GetValueByName("ESigCaptureDM");
                IsEsigValid = true;
                if (gridCaptures != null)
                    foreach (var capture in gridCaptures)
                    {
                        if (capture.IsValid == false)
                        {
                            IsEsigValid = false;
                            break;
                        }
                    }
                if (eSigDetails != null && eSigDetails.Length > 0 && IsEsigValid)
                    SvcData.SetValue("ss_ESigProcessTimerDetails", eSigDetails);



                //Determine the number of lots to transact in a batch
                //Batch tracking is disabled for Track In (this means ALL lots will be transacted together)
                //if ((ConstContainersPerBatch > 0) && ((_gridLotData.GridContext.GetTotalRows() > ConstContainersPerBatch) && (sWIPFlag != "1")))
                //{
                //    iLotsPerBatch = ConstContainersPerBatch;
                //}
                //else
                //{
                iLotsPerBatch = _gridLotData.GridContext.GetTotalRows();
                //}

                //Add the Containers and Wafers
                ContainerRef[] containerNames = new ContainerRef[iLotsPerBatch];
                for (int i = 0; i < iLotsPerBatch; i++)
                {
                    containerNames[i] = new ContainerRef();
                    containerNames[i].Name = _gridLotData.GridContext.GetCell(i.ToString().PadLeft(6, '0'), "Container").ToString();
                }
                SvcData.SetValue("Containers", containerNames);

                if (GetTxnData("IsWaferProcessing").ToUpper() == "TRUE" && (sWIPFlag == "1" || sWIPFlag == "2"))
                {
                    if ((_gridItemData.GridContext as BoundContext).SelectedRowIDs != null)
                    {
                        selectedRowCount = (_gridItemData.GridContext as BoundContext).SelectedRowIDs.Count;

                        WIPLotTxnWafersDetails[] waferNames = new WIPLotTxnWafersDetails[selectedRowCount];

                        int selectedItemIndex = 0;
                        List<string> sSelectedRowIDs = (_gridItemData.GridContext as BoundContext).SelectedRowIDs;
                        foreach (string selectedRowID in sSelectedRowIDs)
                        {
                            waferNames[selectedItemIndex] = new WIPLotTxnWafersDetails();
                            waferNames[selectedItemIndex].Container = new ContainerRef();
                            waferNames[selectedItemIndex].Container.Name = _gridItemData.GridContext.GetCell(selectedRowID, "Container").ToString();
                            waferNames[selectedItemIndex].WaferScribeNumber = _gridItemData.GridContext.GetCell(selectedRowID, "WaferScribeNumber").ToString();
                            selectedItemIndex = selectedItemIndex + 1;
                        }
                        SvcData.SetValue("WafersDetails", waferNames);

                    }
                }

                //Set the Process Type, WIP Flag and Comments
                if (_ndoProcessType.Data != null)
                {
                    NamedObjectRef processType = new NamedObjectRef();
                    processType.Name = _ndoProcessType.Data.ToString();

                    SvcData.SetValue("ProcessType", processType);
                }
                SvcData.SetValue("WIPFlag", Convert.ToInt32(sWIPFlag));

                //Add common data to submit
                if (_ndoEmployee.Data != null)
                    SvcData.SetValue("Employee", _ndoEmployee.Data);

                if (_txtComputerName.Data != null)
                    SvcData.SetValue("ComputerName", _txtComputerName.Data);

                if (_txtCommentsField.Data != null)
                    SvcData.SetValue("Comments", _txtCommentsField.Data);

                //Set service type specific data
                switch (sWIPFlag)
                {
                    case "1": //Track In Data
                        if (_ndoEquipment.Data != null)
                        {
                            NamedObjectRef eqpment = new NamedObjectRef();
                            eqpment.Name = _ndoEquipment.Data.ToString();
                            SvcData.SetValue("Equipment", eqpment);
                            SvcInfo.SetValue("ss_IsExceededMaxNoProcessTime", new Info(true));
                            ReqData.SetValue("Info", SvcInfo);
                        }

                        if (_ndoLoadPort.Data != null)
                        {
                            NamedObjectRef loadport = new NamedObjectRef();
                            loadport.Name = _ndoLoadPort.Data.ToString().ToUpper();
                            SvcData.SetValue("scsLoadPort", loadport);
                        }

                        if (_txtTrackInQtyField.Visible)
                        {
                            SvcData.SetValue("TrackInQty", _txtTrackInQtyField.Data);
                        }

                        //Add Source Equipment Panel Data
                        if (_gridSourceEquipmentField.Visible)
                        {
                            int iSelected = 0;
                            List<TrackInLotSourceEquipment> oTrackInLotSourceEquipmentList = new List<TrackInLotSourceEquipment>();
                            TrackInLotSourceEquipment[] oTrackInLotSourceEquipment = _gridSourceEquipmentField.Data as TrackInLotSourceEquipment[];
                            foreach (TrackInLotSourceEquipment oSourceEquipment in oTrackInLotSourceEquipment)
                            {
                                if (oSourceEquipment.Qty != null)
                                    if (oSourceEquipment.Qty.ToString() != "" && oSourceEquipment.Qty.ToString() != "0")
                                    {
                                        TrackInLotSourceEquipment oTILSE = new TrackInLotSourceEquipment();
                                        oTILSE.Equipment = new NamedObjectRef(oSourceEquipment.Equipment.Name);
                                        oTILSE.Qty = oSourceEquipment.Qty;
                                        oTrackInLotSourceEquipmentList.Add(oTILSE);
                                        iSelected++;
                                    }
                            }

                            if (iSelected > 0)
                                SvcData.SetValue("SourceEquipment", oTrackInLotSourceEquipmentList.ToArray());
                        }
                        break;

                    case "2": //Track Out Data
                        if (_ndoEquipment.Data != null)
                        {
                            NamedObjectRef eqpment = new NamedObjectRef();
                            eqpment.Name = _ndoEquipment.Data.ToString();
                            SvcData.SetValue("Equipment", eqpment);
                        }
                        if (_txtTrackOutQtyField.Visible)
                        {
                            SvcData.SetValue("TrackOutQty", _txtTrackOutQtyField.Data);
                        }
                        if (_subTrackOutNextStepField.Data != null)
                        {
                            NamedSubentityRef nextStep = new NamedSubentityRef();
                            nextStep.Name = _subTrackOutNextStepField.TextEditControl.Text.ToString();

                            // search the dropdown list to get the ID of the name 
                            string sTrackOutNextStepID = _ddlNextSteps.DropDownControl.Items.FindByText(_subTrackOutNextStepField.TextEditControl.Text.ToString()).Value;
                            nextStep.ID = sTrackOutNextStepID;
                            SvcData.SetValue("NextStep", nextStep);
                        }
                        if (_chkRemainInEquipmentIfPossibleField.Enabled && _chkRemainInEquipmentIfPossibleField.CheckControl.Checked)
                        {
                            SvcData.SetValue("RemainInEquipmentIfPossible", true);
                        }
                        if (_chkRemainInEquipmentField.Enabled && _chkRemainInEquipmentField.CheckControl.Checked)
                        {
                            SvcData.SetValue("RemainInEquipment", true);
                        }
                        if (_chkCancelTrackInField.Enabled && _chkCancelTrackInField.CheckControl.Checked)
                        {
                            SvcData.SetValue("CancelTrackIn", true);
                        }
                        if (_txtDummyQtyField.Visible && _txtDummyQtyField.Data != null)
                        {
                            SvcData.SetValue("DummyQty", _txtDummyQtyField.Data.ToString());
                        }
                        if (_txtNumberOfStripsField.Visible && _txtNumberOfStripsField.Data != null)
                        {
                            SvcData.SetValue("NumberOfStrips", _txtNumberOfStripsField.Data.ToString());
                        }
                        if (_chkSplitUnProcessedField.Visible && _chkSplitUnProcessedField.CheckControl.Checked)
                        {
                            SvcData.SetValue("SplitUnProcessed", "true");
                            if (_chkSplitUnProcessedAsNewScheduleField.CheckControl.Checked)
                            {
                                SvcData.SetValue("SplitUnProcessedAsNewSchedule", "true");
                            }
                            else
                            {
                                SvcData.SetValue("SplitUnProcessedAsNewSchedule", "false");
                            }
                            SvcData.SetValue("SplitUnProcessedLotId", _txtSplitUnProcessedLotIdField.Data);
                            SvcData.SetValue("SplitUnProcessedQty", _txtSplitUnProcessedQtyField.Data);
                        }
                        SvcInfo.SetValue("SPCTxnDataList", new SPCTxnData_Info());
                        SvcInfo.SetValue("SPCTxnDataList.Name", new Info(true));
                        SvcInfo.SetValue("SPCTxnDataList.SPCSetup", new Info(true));
                        SvcInfo.SetValue("SPCTxnDataList.SPCResult", new Info(true));
                        SvcInfo.SetValue("SPCTxnDataList.SPCResultFilename", new Info(true));
                        SvcInfo.SetValue("SPCTxnDataList.FailureDocumentSet", new Info(true));
                        SvcInfo.SetValue("SPCTxnDataList.ChartHeight", new Info(true));
                        SvcInfo.SetValue("SPCTxnDataList.ChartWidth", new Info(true));
                        ReqData.SetValue("Info", SvcInfo);

                        //Add carrier tracking data for Assembly Carrier WIP Main if output carrier is detected
                        if (sServiceType == "AssemblyCarrierWIPMain" && _gridLotData.TotalRowCount == 1)
                        {
                            if (_gridLotData.GridContext.GetCell(0, "__OutputCarrier") != null)
                                if (_gridLotData.GridContext.GetCell(0, "__OutputCarrier").ToString() != "")
                                {
                                    NamedObjectRef[] carrierNames = new NamedObjectRef[1];
                                    carrierNames[0] = new NamedObjectRef();
                                    carrierNames[0].Name = _gridLotData.GridContext.GetCell(0, "__OutputCarrier").ToString();
                                    SvcData.SetValue("Carriers", carrierNames);
                                }
                        }
                        break;

                    case "4": //Move Out Data
                        if (_txtMoveOutQtyField.Visible)
                        {
                            SvcData.SetValue("MoveOutQty", _txtMoveOutQtyField.Data);
                        }
                        if (_subNextStepField.Data != null)
                        {
                            NamedSubentityRef nextStep = _subNextStepField.Data as NamedSubentityRef;
                            nextStep.Name = _subNextStepField.TextEditControl.Text.ToString();
                            // search the hidden TrackOutNextStep dropdown list to get the ID of the name
                            string sNextStepID = _ddlNextSteps.DropDownControl.Items.FindByText(_subNextStepField.TextEditControl.Text.ToString()).Value;
                            nextStep.ID = sNextStepID;
                            SvcData.SetValue("NextStep", nextStep);
                        }
                        break;

                    default: sWIPFlag = ""; break;
                }

                //Submit the transaction
                ResultStatus Results = Svc.ExecuteTransaction(SvcData as DCObject, ReqData as Request, out result);
                if (Results.IsSuccess)
                {
                    bool bSPCAvailable = false;
                    bool bAlertAvailable = false;
                    string sCompletionMessage = Results.Message;
                    string[] sCompletionMessages = new string[] { Results.Message.Substring(Results.Message.IndexOf('@') + 3) };

                    //Check for SPC Txn Data List
                    SPCTxnData[] oSPCTxnData = null;
                    if ((result.Value as WIPMain).SPCTxnDataList != null)
                    {
                        oSPCTxnData = (result.Value as WIPMain).SPCTxnDataList;
                        if (oSPCTxnData.Length > 0)
                            bSPCAvailable = true;
                    }

                    if (bSPCAvailable)
                    {
                        if (oSPCTxnData != null)
                            SetSPCControls(true, oSPCTxnData);
                        DisplaySPCChart(oSPCTxnData, Results);
                    }
                    else
                    {
                        if ((result.Value as WIPMain).ss_IsExceededMaxNoProcessTime != null && (result.Value as WIPMain).ss_IsExceededMaxNoProcessTime == true)
                            DisplayAlerts(sCompletionMessages);
                        else
                        {
                            DisplayAlerts(Results, out bAlertAvailable, out sCompletionMessage);
                            Results.Message = sCompletionMessage;
                        }
                    }

                    if (!bAlertAvailable && ((result.Value as WIPMain).ss_IsExceededMaxNoProcessTime == false || (result.Value as WIPMain).ss_IsExceededMaxNoProcessTime == null))
                        this.DisplayMessage(Results);

                    ClearControls(0, true);
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

        //---------------------------------------------------
        // Txn to check if there is a need for the confirmation window (currently only used together with Process Timers)
        //---------------------------------------------------
        public void ConfirmSubmit()
        {
            try
            {
                //Initialize the Service Data
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                //string ServiceType = this.PrimaryServiceType;
                string sServiceType = _txtPrimaryServiceType.Data != null ? _txtPrimaryServiceType.Data.ToString() : "WIPMain";
                var Svc = new WSDataCreator().CreateService(sServiceType, profile);
                var SvcData = WCFObject.CreateObject(sServiceType) as ICreator;
                var SvcInfo = WCFObject.CreateObject(sServiceType + "_Info") as ICreator;
                var ReqData = WCFObject.CreateObject(sServiceType + "_Request") as ICreator;
                var ResData = WCFObject.CreateObject(sServiceType + "_Result") as ICreator;
                Result result = null;

                int iLotsPerBatch = 0;
                string sWIPFlag = GetWIPFlag();

                //Determine the number of lots to transact in a batch
                //Batch tracking is disabled for Track In (this means ALL lots will be transacted together)
                //if ((ConstContainersPerBatch > 0) && ((_gridLotData.GridContext.GetTotalRows() > ConstContainersPerBatch) && (sWIPFlag != "1")))                
                //    iLotsPerBatch = ConstContainersPerBatch;                
                //else                
                iLotsPerBatch = _gridLotData.GridContext.GetTotalRows();

                //Add the Containers and Wafers
                ContainerRef[] containerNames = new ContainerRef[iLotsPerBatch];
                for (int i = 0; i < iLotsPerBatch; i++)
                {
                    containerNames[i] = new ContainerRef();
                    containerNames[i].Name = _gridLotData.GridContext.GetCell(i.ToString().PadLeft(6, '0'), "Container").ToString();
                }

                SvcData.SetValue("Containers", containerNames);
                SvcData.SetValue("WIPFlag", Convert.ToInt32(sWIPFlag));

                //Add common data to submit
                if (_ndoEmployee.Data != null)
                    SvcData.SetValue("Employee", _ndoEmployee.Data);

                if (_txtComputerName.Data != null)
                    SvcData.SetValue("ComputerName", _txtComputerName.Data);

                SvcInfo.SetValue("scsIsTxnConfirmationReq", new Info(true));
                ReqData.SetValue("Info", SvcInfo);

                //Submit the transaction
                ResultStatus Results = Svc.GetEnvironment(SvcData as DCObject, ReqData as Request, out result);
                if (Results.IsSuccess)
                {
                    CWC.Button oSubmit = Page.FindCamstarControl("Main_SubmitButton") as CWC.Button;

                    bool bConfirmationRequired = false;
                    string sCompletionMessage = Results.Message;

                    if ((result.Value as WIPMain).scsIsTxnConfirmationReq != null)
                        bConfirmationRequired = bool.Parse((result.Value as WIPMain).scsIsTxnConfirmationReq.ToString());

                    if (bConfirmationRequired)
                        ScriptManager.RegisterStartupScript(Page.Form, this.GetType(), "myConfirm", "WIPMain_TxnConfirmation();", true);
                    else
                        SubmitTransactions();
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

    }
}



