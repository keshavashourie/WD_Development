/* Copyright 2023 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;

using SEMI.AppCode;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Personalization;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using CWF = Camstar.WebPortal.FormsFramework;
using WC = CamstarPortal.WebControls;

/// <summary>
/// Summary description for SS_JobProgress
/// </summary>
/// 

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_JobProgress : SS_JobTxn
    {
        #region Property

        // Grid 
        private JQDataGrid _gridChecklist { get { return Page.FindCamstarControl("Job_Checklist") as JQDataGrid; } }

        // Data Collection 
        private CWC.RevisionedObject _rdoDetailDataCollsDefField { get { return Page.FindCamstarControl("Job_DetailDataCollectionDefField") as CWC.RevisionedObject; } }
        private CWC.RevisionedObject _rdoDataCollectsDef { get { return Page.FindCamstarControl("Job_DataCollectionDef") as CWC.RevisionedObject; } }
        private WC.ShopFloorDCControl _dcParametricData { get { return Page.FindCamstarControl("Job_ParametricData") as WC.ShopFloorDCControl; } }

        // to stage
        private CWC.DropDownList _ddlStageToSequence { get { return Page.FindCamstarControl("Job_ToStageSequence") as CWC.DropDownList; } }
        private CWC.NamedObject _ndoToStage { get { return Page.FindCamstarControl("Job_ToStage") as CWC.NamedObject; } }
        private CWC.NamedObject _ndoCopyStage { get { return Page.FindCamstarControl("CopyStage") as CWC.NamedObject; } }
        private CWC.CheckBox _chkAutoClockOff { get { return Page.FindCamstarControl("Job_AutoClockOff") as CWC.CheckBox; } }
        
        // variable
        private bool isRequireDataCollection = false;
        private string cdoTypeTemp = "";

        // wei sun : edit to add SPC
        // SPC
        protected DataEnvelopControl _DataEnvelop { get { return Page.FindCamstarControl("Envelop") as DataEnvelopControl; } }
        // wei sun : edit to add SPC end 

        // computer name
        private CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("Job_ComputerName") as CWC.TextBox; } }

        #endregion Property

        #region PageEvent 

        //
        // Contructor  
        //
        public SS_JobProgress()
        {
        }

        //
        // initial , set event handler  
        //
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.LoadComplete += Page_LoadComplete;
            Page.OnPreExecute += new EventHandler<CWF.FormProcessingEventArgs>(Page_OnPreExecute);
            Page.OnPostExecute += new EventHandler<CWF.ResultEventArgs>(Page_OnPostExecute);           
        }

        //
        // onload , handling event : grid rowselected, loadparamdata, postBack,
        //
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ////if (_txtHdnIsPostBack.Data == "3")
            ////{
            ////    _txtHdnIsPostBack.Data = "0";
            ////    ScriptManager.RegisterStartupScript(Page.Form, GetType(), "loadParamAuto", "jQuery(document).ready(function(){ __doPostBack('" + _gridChecklist.ClientID + "','loadParamDataJobModel'); });", true);
            ////}

            if (!Page.IsPostBack )
            {
                _txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
                //Page.ActionDispatcher.PageActions().All(actRow => actRow.IsDisabled = true);
                hideDocument();

                // wei sun : edit to add SPC
                AddDataEnvelopDataMember();
                // wei sun : edit to add SPC end
                ManagePartButton(getPartRequest());
            }
            else if (Page.EventArgument == "OnRowSelected" || Page.EventArgument == "OnRowSelected:WebPart_SS_JobGeneralWP02~Job_Checklist")
            {
                Display_DataCollection();
                if (Page.EventArgument == "OnRowSelected")
                    isNeedRefresh = true;
                else
                    ManagePartButton(getPartRequest());
            }
            else if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument")
            {
                RefreshPartStatus();
                ManagePartButton(getPartRequest());
                // wei sun: edit to add SPC
                OnPopupClose();
                // wei sun: edit to add SPC end
            }
            
            else if (_ndoResource.IsChanged)
            {
                ClearDataCollection();
                Result rslt = GetJobData(PrimaryServiceType);
                if (rslt != null) //if the result has data then set the value of general field 
                {
                    SetGeneralFields(rslt);
                }
                ManagePartButton(getPartRequest());
            }

            if (_gridChecklist != null)
                _gridChecklist.GridContext.RowSelected += new JQGridEventHandler(ChecklistGrid_RowSelected);           
             
        } // protected override void OnLoad

        //
        // onload complete , add dependencies and render to client also refresh the data if necessary,
        //
        void Page_LoadComplete(object sender, EventArgs e)
        {
            _ndoSymptomCode.SelectionDependencies.Add(new CWF.DependsOnItem(_ndoResource.ID));
            _ndoCauseCode.SelectionDependencies.Add(new CWF.DependsOnItem(_ndoResource.ID));
            _ndoRepairCode.SelectionDependencies.Add(new CWF.DependsOnItem(_ndoResource.ID));
            Camstar.WebPortal.FormsFramework.CamstarWebControl.SetRenderToClient(_ndoSymptomCode);
            Camstar.WebPortal.FormsFramework.CamstarWebControl.SetRenderToClient(_ndoCauseCode);
            Camstar.WebPortal.FormsFramework.CamstarWebControl.SetRenderToClient(_ndoRepairCode);
            
            if (isNeedRefresh)
            {
                isNeedRefresh = false;
                _OnJobOrder_DataChanged(null, null);
            }
        }

        //
        // gather forms data before submit, 
        //
        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);
            
            if (_rdoDataCollectsDef != null)
            {
                if (_rdoDataCollectsDef.Text != "")
                {
                    DataPointSummary[] dataPointSummary = _dcParametricData.GetDataPointSummary();
                    if (dataPointSummary != null && dataPointSummary.Length > 0)
                        ((ShopFloor)serviceData).ParametricData = dataPointSummary[0];
                    else
                        _dcParametricData.Visible = false;
                }
            } // if (_rdoDataCollectsDef != null)

            if (_ndoSymptomCode.Text != "" && _ndoSymptomCode.ReadOnly == false)
                    (serviceData as OM.JobTxn).SymptomCode = new NamedObjectRef(_ndoSymptomCode.Text);

            if (_ndoCauseCode.Text != "" && _ndoCauseCode.ReadOnly == false)
                    (serviceData as OM.JobTxn).CauseCode = new NamedObjectRef(_ndoCauseCode.Text);

            if (_ndoRepairCode.Text != "" && _ndoRepairCode.ReadOnly == false)
                    (serviceData as OM.JobTxn).RepairCode = new NamedObjectRef(_ndoRepairCode.Text);

            if (_gridChecklist.GridContext.SelectedRowIDs != null)
              {
                  if (_gridChecklist.GridContext.SelectedRowIDs.Count > 0)
                    {
                        (serviceData as OM.JobTxn).Checklist = new JobModelDetailChecklistChanges[_gridChecklist.GridContext.SelectedRowIDs.Count];

                        int i = 0;
                        foreach (string rowId in _gridChecklist.GridContext.SelectedRowIDs)
                        {
                            JobModelDetailChecklistChanges selectedRow = _gridChecklist.GridContext.GetItem(rowId) as JobModelDetailChecklistChanges;
                            (serviceData as OM.JobTxn).Checklist[i] = new JobModelDetailChecklistChanges { ChecklistId = selectedRow.ChecklistId.Value, 
                                                                                                           Instruction = selectedRow.Instruction.Value };
                            i++;
                        } // foreach
                    } //if (_gridChecklist.GridContext.SelectedRowIDs.Count > 0)
              } //if (_gridChecklist.GridContext.SelectedRowIDs != null)
        } // public override void GetInputData

        //
        // Before execute transaction, gridchecklist data & spc
        //
        void Page_OnPreExecute(object sender, CWF.FormProcessingEventArgs e)
        {
            OM.Info serviceInfo = e.Info;
            OM.Service serviceData = e.Data;

            if (_gridChecklist != null)
            {
                if (_gridChecklist.GridContext.SelectedRowIDs != null)
                {
                    if (_gridChecklist.GridContext.SelectedRowIDs.Count > 0)
                    {
                        // wei sun : edit to submit multiple checklist
                        (serviceData as OM.JobTxn).Checklist = new JobModelDetailChecklistChanges[_gridChecklist.GridContext.SelectedRowIDs.Count];
                        int intRowCounter = 0;
                        foreach (string selectedRowID in _gridChecklist.GridContext.SelectedRowIDs)
                        {
                            JobModelDetailChecklistChanges selectedRow = _gridChecklist.GridContext.GetItem(selectedRowID) as JobModelDetailChecklistChanges;
                            (serviceData as OM.JobTxn).Checklist[intRowCounter] = new JobModelDetailChecklistChanges { ChecklistId = selectedRow.ChecklistId.Value,
                                                                                        Instruction = selectedRow.Instruction.Value };
                            intRowCounter++;
                        }
                        // wei sun: edit to submit multiple checklist end
                        //isNeedRefresh = true;
                    } //if (_gridChecklist.GridContext.SelectedRowIDs.Count > 0)
                } //if (_gridChecklist.GridContext.SelectedRowIDs != null)
            } //if (_gridChecklist != null)

            // wei sun: edit to add SPC
            (serviceInfo as OM.JobTxn_Info).SPCTxnDataList = new SPCTxnData_Info { 
                                                                    Name = FieldInfoUtil.RequestValue(),  SPCSetup = FieldInfoUtil.RequestValue(),
                                                                    SPCResult = FieldInfoUtil.RequestValue(), SPCResultFilename = FieldInfoUtil.RequestValue(),
                                                                    FailureDocumentSet = FieldInfoUtil.RequestValue(), ChartHeight = FieldInfoUtil.RequestValue(),
                                                                    ChartWidth = FieldInfoUtil.RequestValue()
                                                                                  };             
            // wei sun: edit to add SPC end
        } //void Page_OnPreExecute

        //
        // after execute transaction, gridchecklist data & spc
        //
        void Page_OnPostExecute(object sender, CWF.ResultEventArgs e)
        {
            if (e.Status.IsSuccess)
            {
                ClearDataCollection();
                // wei sun: edit to add SPC
                if ((e.Value as JobProgress).SPCTxnDataList != null)
                {
                    SPCTxnData[] oSPCTxnData = (e.Value as JobProgress).SPCTxnDataList;
                    if (oSPCTxnData.Length > 0)
                    {
                        // check for alert messages
                        string[] sAlertMessages;
                        string sCompletionMessage;
                        bool bAlertMsg = SEMI.AppCode.UIUtility.AlertMessagesAvailable(e.Status.Message, out sAlertMessages, out sCompletionMessage);                            

                        DataPacket SS_DataPacket = new DataPacket();
                        if (bAlertMsg)
                            SS_DataPacket.AlertMessages = sAlertMessages;
                        SS_DataPacket.ResultStatusMessage = sCompletionMessage;
                        SS_DataPacket.IsErrorResultStatusMessage = !(e.Status.IsSuccess);

                        _DataEnvelop.SS_DataPacket = SS_DataPacket;
                        DisplayChart(oSPCTxnData, e.Status);
                    }
                }
                else
                {
                    // andy: move the isNeedRefresh validation from onPreExecute to here
                    if (_gridChecklist.GridContext.SelectedRowIDs != null)
                        isNeedRefresh = true;
                    if (isNeedRefresh)
                    {
                        isNeedRefresh = false;
                        if (_gridChecklist.GridContext.SelectedRowIDs != null)
                        {
                            if (_gridChecklist.GridContext.SelectedRowIDs.Count > 0)
                            {
                                e.Status.IsSuccess = false; // set to false so that the popup window remains open
                                _gridChecklist.GridContext.SelectedRowIDs = null;
                                _gridChecklist.GridContext.SelectedRowID = null;
                                ClearDataLevel1();
                            }
                        }

                        // check if the auto clock off is true, if true then set the e.Status.IsSuccess to true to close the window
                        if (_chkAutoClockOff.CheckControl.Checked == true)
                        {
                            e.Status.IsSuccess = true;
                        }
                    }
                    _OnJobOrder_DataChanged(sender, e);
                    DisplayMessage(e.Status);
                }
                // wei sun: edit to add SPC end               
            } //if (e.Status.IsSuccess)

            if (_rdoDetailDataCollsDefField.Data != null)
                Display_DataCollection();
        } //void Page_OnPostExecute

        //
        // Before rendering, focus to the bottom of the page , refresh it is needed, enable/disable technician grid
        //
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //comment-out to keep focus after postback
            //if (Page.EventArgument == "OnRowSelected")
            //   Page.SetFocus((Page.FindCamstarControl("hdnGridBottom") as CWC.TextBox).ClientID);

            if (_pageView == DisplayMode.PopUpPage && _txtHdnIsPostBack.Data == "1")
            {
                _OnJobOrder_DataChanged(null, null);
                ManagePartButton(getPartRequest());
            }
            bool isEnableTechGrid = !(PrimaryServiceType == "JobCreate" || PrimaryServiceType == "JobAssign");
            if (isEnableTechGrid && _gridJobTechnician != null)
            {
                (_gridJobTechnician.GridContext as BoundContext).Settings.NavigatorActions.ToList().ForEach(act => act.Visible = false);
                (_gridJobTechnician.GridContext as BoundContext).Settings.Columns[0].Editable = false;
            }

        } // protected override void OnPreRender

   
        #endregion PageEvent 

        #region GridEvent

        //
        // to make the grid postback on row selected 
        //
        ResponseData ChecklistGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            ((DirectUpdateData)args.Response).PropertyValue = ((DirectUpdateData)args.Response).PropertyValue.Replace("PostBackRequested:false", "PostBackRequested:true");
            return args.Response;
        }

        #endregion //GridEvent

        #region Procedure

        //
        // override setGeneral Fields, and addon necessary field to set 
        //
        protected override void SetGeneralFields(Result rslt)
        {
            base.SetGeneralFields(rslt);
            PartRequestOrder[] partRequests = (rslt as ICreator).GetValue("Value.RequestOrders") as PartRequestOrder[];
            savePartRequest(partRequests);

            // setting readonly for code
            if ((rslt as ICreator).GetValue("Value.IsSimpleMode") != null && ((bool)(rslt as ICreator).GetValue("Value.IsSimpleMode") == true))
            {
                _ndoSymptomCode.ReadOnly = false;
                _ndoCauseCode.ReadOnly = false;
                _ndoRepairCode.ReadOnly = false;
            }
            else
            {
                _ndoSymptomCode.ReadOnly = (bool)(rslt as ICreator).GetValue("Value.RequireSymptomCode") == false ? true : false;
                _ndoCauseCode.ReadOnly = (bool)(rslt as ICreator).GetValue("Value.RequireCauseCode") == false ? true : false;
                _ndoRepairCode.ReadOnly = (bool)(rslt as ICreator).GetValue("Value.RequireRepairCode") == false ? true : false;
            }
            // wei sun: comment out to fix alignment issue temporarily
            //ctrSymptomCode.Required = !ctrSymptomCode.ReadOnly;
            //ctrCauseCode.Required = !ctrCauseCode.ReadOnly; 
            //ctrRepairCode.Required = !ctrRepairCode.ReadOnly;
            // wei sun: edit to fix alignment issue temporarily end

            /// set checklist grid & data collection
            _rdoDetailDataCollsDefField.ClearData();
            JobModelDetailChecklistChanges[] rsCheck = ((rslt as ICreator).GetValue("Value.ChecklistSelection") as JobModelDetailChecklistChanges[]);
            if (rsCheck != null)
            {
                _gridChecklist.Visible = true;
                _gridChecklist.Data = rsCheck;
                _gridChecklist.OriginalData = rsCheck;
            }
            else
                _gridChecklist.Visible = false;

            if (rsCheck == null)
                ClearDataCollection();

            RevisionedObjectRef DataCols = ((rslt as ICreator).GetValue("Value.DataCollectionDef") as RevisionedObjectRef);
            if (DataCols != null)
            {
                if ((rsCheck == null) || (DataCols.Name != "" && isChecklistCompleted()))
                {
                    _rdoDetailDataCollsDefField.Data = new RevisionedObjectRef(DataCols.Name, DataCols.Revision, DataCols.CDOTypeName);
                    (_rdoDetailDataCollsDefField.Data as RevisionedObjectRef).CDOTypeName = DataCols.CDOTypeName;

                    //if (rsCheck == null)
                    //{
                        Display_DataCollection();
                    //}
                }
            }

            if ( _dmIsPopUp != "" )
            {
                _txtHdnIsPostBack.Data = "3"; 
                ManagePartButton(partRequests);
            }

            // reset ToStage value 
            _ddlStageToSequence.Text = "";
            _ndoToStage.Text = "";
        } // protected override void SetGeneralFields

        //
        // to disable / enable partbutton base on partRequests
        //
        public void ManagePartButton(PartRequestOrder[] partRequests)
        {
            const string varRequest = "btnPartRequest", varUpdate = "btnRequestUpdate", varIssue = "btnRequestIssue", varCancel = "btnRequestCancel", varSubmit = "btnSubmit", varReset = "btnReset", varClose = "btnClose";
            const string findUpdate = " REQUESTED ACKNOWLEDGED ASSIGNED", findIssue = " COMPLETED", findCancel = " REQUESTED ACKNOWLEDGED ASSIGNED COMPLETED", findClose = " CLOSE";
            try
            {
                // Default isDisable, set other than request action to false
                Page.ActionDispatcher.PageActions().All(actRow => (actRow.IsDisabled = true) ?? true);

                // set part request to true, ActionPanelActions for panel , 
                var vReq = this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == varRequest);
                var vSubmit = this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == varSubmit);
                var vReset = this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == varReset);
                var vClose = this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == varClose);

                vClose.First().IsDisabled = false;

                if (vReq.Count() > 0)
                    vReq.First().IsDisabled = false;
                if (vSubmit.Count() > 0)
                     vSubmit.FirstOrDefault().IsDisabled = false;
                if (vReset.Count() > 0)
                     vReset.FirstOrDefault().IsDisabled = false;

                // check current real status from database 
                if (partRequests != null)
                {
                    foreach (PartRequestOrder partRequest in partRequests)
                    {
                        if (findUpdate.IndexOf(partRequest.RequestStatus.ToString()) > 0)
                            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == varUpdate).First().IsDisabled = false;// UIActionIsDisable(varUpdate, false);

                        if (findIssue.IndexOf(partRequest.RequestStatus.ToString()) > 0)
                            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == varIssue).First().IsDisabled = false;//UIActionIsDisable(varIssue, false);

                        if (findCancel.IndexOf(partRequest.RequestStatus.ToString()) > 0)
                            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == varCancel).First().IsDisabled = false;//UIActionIsDisable(varCancel, false);
                    } //foreach
                } //if (partRequests != null)
            } //try
            catch (Exception e)
            {
                Page.Response.Redirect(Page.Request.Url.AbsolutePath);
            }
        } //public void ManagePartButton

        //
        // save part data into hidden text
        //
        private void savePartRequest(PartRequestOrder[] partRequests)
        {
           // clear the hidden part request status field
            if (_txtHdnPartReq != null)
                _txtHdnPartReq.Data = null;

            if (partRequests != null)
            {
                string reqList = "";
                foreach (PartRequestOrder partRequest in partRequests)
                {
                    reqList += partRequest.RequestStatus.ToString() + ",";
                }
                if (_txtHdnPartReq != null)
                    _txtHdnPartReq.Data = reqList;
            } //if (partRequests != null)
        } // private void savePartRequest

        //
        // get part data from hidden text
        //
        private PartRequestOrder[] getPartRequest()
        {
            PartRequestOrder[] pr = null;
            if (_txtHdnPartReq != null)
            {
                if (_txtHdnPartReq.Data != null)
                {
                    string[] dat = _txtHdnPartReq.Data.ToString().Split(',');
                    dat = dat.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    if (dat.Count() > 0)
                    {
                        pr = new PartRequestOrder[dat.Count()];
                        for (int i = 0; i < dat.Count(); ++i)
                        {
                            pr[i] = new PartRequestOrder{RequestStatus = dat[i]};                          
                        }
                    }
                }
            }
            return pr;
        }

        //
        // get part data from webservices
        //
        public void RefreshPartStatus()
        {
            // Declaration 
            string serviceName = this.PrimaryServiceType.ToString();
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            ResultStatus res = new ResultStatus(null, false);
            var cdo = WCFObject.CreateObject(serviceName) as ICreator;
            var request = WCFObject.CreateObject(serviceName + "_Request") as ICreator;
            var reqInfo = WCFObject.CreateObject(serviceName + "_Info") as ICreator;
            Result result = null;
            var service = new WSDataCreator().CreateService(serviceName, profile);

            // Set Data Input
            cdo.SetValue("JobOrder", _ndoJobOrder.Data as NamedObjectRef);
            cdo.SetValue("Resource", _ndoResource.Data as NamedObjectRef);

            // Request Info
            reqInfo.SetValue("RequestOrders", new OM.PartRequestOrder_Info());
            reqInfo.SetValue("RequestOrders.RequestStatus", new OM.Info(true));
            request.SetValue("Info", reqInfo);

            // Execute
            ResultStatus rslt = service.GetEnvironment(cdo as DCObject, request as Request, out result);

            // Result
            if (rslt.IsSuccess)
            {
                PartRequestOrder[] partRequests = (result as ICreator).GetValue("Value.RequestOrders") as PartRequestOrder[];
                savePartRequest(partRequests);
            }
        }

        #endregion Procedure 
                
        #region Data Collection 

        //
        // triger datacollection and set necesarry field & view document  
        //
        public void Display_DataCollection()
        {
            string name = "", revision = "", cdoType = "", docSet = "";

            if ((_gridChecklist.GridContext.SelectedRowIDs == null || _gridChecklist.GridContext.SelectedRowID == null) && isChecklistCompleted())
            {
                ClearDataLevel1();

                if (_rdoDetailDataCollsDefField.Data != null)
                {
                    name = _rdoDetailDataCollsDefField.Text.ToString();
                    revision = _rdoDetailDataCollsDefField.RevisionValue;
                    if (_txtHdnIsPostBack.Data != null)
                        cdoType =  _txtHdnIsPostBack.Data.ToString() == "1" ? cdoTypeTemp : _rdoDetailDataCollsDefField.CDOTypeName; 
                    else
                        cdoType = _rdoDetailDataCollsDefField.CDOTypeName;

                } // if (_rdoDetailDataCollsDefField.Data != null)
                else
                    return;
            }
            else if (_gridChecklist.GridContext.SelectedRowIDs != null)
            {
                ClearDataLevel1();
                JobModelDetailChecklistChanges selectedRow = _gridChecklist.GridContext.SelectedRowIDs.Select(rowId => _gridChecklist.GridContext.GetItem(rowId)).LastOrDefault() as JobModelDetailChecklistChanges;
                if (selectedRow != null)
                {
                    if (selectedRow.DocumentSet != null)
                        docSet = selectedRow.DocumentSet.ToString();

                    name = selectedRow.DataCollectionDef == null ? "" : selectedRow.DataCollectionDef.Name;
                    revision = selectedRow.DataCollectionDef == null ? "" : selectedRow.DataCollectionDef.Revision;
                    cdoType = selectedRow.DataCollectionDef == null ? "" : selectedRow.DataCollectionDef.CDOTypeName;
                }
            }

            (_rdoDataCollectsDef.Data) = new RevisionedObjectRef(name, revision, cdoType);
            isRequireDataCollection = true;
            Service.LoadServiceValues(PrimaryServiceType, "GetDataPoints");//load DisplayValues method is called
            ViewDocumentRefresh(docSet, ESDocumentType.Checklist);
        } // public void Display_DataCollection
        
        //
        // set request value for parametric data  
        //
        public override void RequestValues(Info serviceInfo, Service serviceData)
        {
            base.RequestValues(serviceInfo, serviceData);

            if (isRequireDataCollection)
            {
                if (serviceData is ShopFloor)
                    (serviceData as ShopFloor).DataCollectionDef = (_rdoDataCollectsDef.Data) as RevisionedObjectRef;
                if (serviceInfo is ShopFloor_Info)
                {
                    (serviceInfo as JobProgress_Info).DataCollectionDef = new Info(true);
                    (serviceInfo as ShopFloor_Info).HasDataCollection = new Info(true);
                }
                _dcParametricData.Visible = true;
                _dcParametricData.RequestValues(serviceData as ShopFloor, serviceInfo as ShopFloor_Info);
            }
        }

        //
        // display data collection   
        //
        public override void DisplayValues(Service serviceData)
        {
            if (isRequireDataCollection)
            {
                _dcParametricData.DisplayValues(serviceData as ShopFloor);
                //isRequireDataCollection = true;
            }
            base.DisplayValues(serviceData);
        }

        #endregion Data Collection 
        
        #region Helper 

        //
        // handle Job Stage data changed 
        //
        public void hideDocument()
        {
            if (!Page.IsPostBack)
            {
                _ndoToStageDocumentTxt.Visible = false;
                _ndoCopyStage.Visible = false;
            }
        }//public void SetFirstToStageSequence

        //
        // handle Job Stage data changed 
        //
        public void SetFirstToStageSequence()
        {
            if (isTriggered != TrigerAfter.JobOrderDataChanged)
            {
                if (_ndoToStage.Text != "" && _ddlStageToSequence.Data != null)
                    _ddlStageToSequence.Text = _ddlStageToSequence.Data.ToString();
                else if (_ndoToStage.Text == "")
                    _ddlStageToSequence.Text = "";

                ViewDocumentRefresh(_ndoToStageDocumentTxt.Text, ESDocumentType.ToStage); 
                isTriggered = TrigerAfter.None;
            }
        }//public void SetFirstToStageSequence

        //
        // to check whether the checklist already completed or not
        //
        public bool isChecklistCompleted()
        {
            bool complete = true;
            foreach (JobModelDetailChecklistChanges row in _gridChecklist.Data as JobModelDetailChecklistChanges[])
            {
                if (row.ChecklistCount.IsEmpty || row.ChecklistCount == 0)
                {
                    complete = false;
                    break;
                }
            } // foreach
            return complete;
        } // public bool isChecklistCompleted

        //
        // Clear parametric Data 
        //
        public void ClearDataLevel1()
        {
            _rdoDataCollectsDef.ClearData();
            _dcParametricData.Clear();
            _dcParametricData.Clean();
            _dcParametricData.Data = null;
            _dcParametricData.IterationCount = 1;
            _dcParametricData.Visible = false;
        } // public void ClearDataLevel1

        //
        // Clear data colection including grid 
        //
        public void ClearDataCollection()
        {
            _rdoDataCollectsDef.ClearData();
            _dcParametricData.Clear();
            _dcParametricData.Clean();
            _dcParametricData.Data = null;
            _dcParametricData.IterationCount = 1;
            _gridChecklist.Data = null;
            _gridChecklist.OriginalData = null;
            _dcParametricData.Visible = false;
        } //public void ClearDataCollection

        #endregion Helper 
        
        # region SPC
        // wei sun : edit to add SPC
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
            objPageDataMembers[intDMIndex] = new UIComponentDataMember { 
               Key = "SPCWP.Envelop", Name = "envelopMainOutDM",
               ConnectionType = DataMemberConnectionType.Control, Property = "SS_DataPacket" 
            };

            intDMIndex++;

            // add the new envelop data member for data to be retrieved FROM popup
            objPageDataMembers[intDMIndex] = new UIComponentDataMember { 
               Key = "SPCWP.Envelop", Name = "envelopMainInDM", 
               ConnectionType = DataMemberConnectionType.Control,
               Property = "SS_DataPacket"
            };

            Page.DataContract.DataMembers = objPageDataMembers;
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public void DisplayChart(SPCTxnData[] oSPCTxnData, ResultStatus TxnResultStatus)
        {
            if (oSPCTxnData.Length > 0)
            {
                int intCHeight = 0;
                int intCWidth = 0;

                ////// set the data envelop data to pass to the popup;            
                SEMI.AppCode.Services.SPCTxn.SPCTxnDataSet SPCTxnDataSet = new SEMI.AppCode.Services.SPCTxn.SPCTxnDataSet();
                foreach (SPCTxnData oSPCTxnDataItem in oSPCTxnData)
                {
                    if (oSPCTxnDataItem.SPCResultFilename != "")
                        SPCTxnDataSet.AddDataListItem(oSPCTxnDataItem);

                    if (int.Parse(oSPCTxnDataItem.ChartHeight.ToString()) > intCHeight)
                        intCHeight = int.Parse(oSPCTxnDataItem.ChartHeight.ToString());

                    if (int.Parse(oSPCTxnDataItem.ChartWidth.ToString()) > intCHeight)
                        intCWidth = int.Parse(oSPCTxnDataItem.ChartWidth.ToString());
                }

                DataPacket SS_DataPacket = new DataPacket();
                SS_DataPacket.SPCTxnDataSet = SPCTxnDataSet;

                if (_DataEnvelop.SS_DataPacket != null)
                    SS_DataPacket = _DataEnvelop.SS_DataPacket;

                SS_DataPacket.SPCTxnDataSet = SPCTxnDataSet;
                
                // check for alert messages
                string[] sAlertMessages;
                string sCompletionMessage;
                bool bAlertMsg = SEMI.AppCode.UIUtility.AlertMessagesAvailable(TxnResultStatus.Message, out sAlertMessages, out sCompletionMessage);

                SS_DataPacket.AlertMessages = sAlertMessages;

                _DataEnvelop.SS_DataPacket = SS_DataPacket;

                Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                objAction.PageName = "SS_SPCChartsPopupVP";

                objAction.FrameLocation = new UIFloatingPageLocation { Width = intCWidth + 50, //750
                                                                       Height = intCHeight + 190 //580;
                                                                      };
              
                UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[1];
                objLinks[0] = new UIComponentDataContractLink { SourceMember = "envelopMainOutDM", TargetMember = "envelopPopupInDM" };
                objAction.DataContractMap = new UIComponentDataContractMap{ Links = objLinks };
                
                UIComponentDataContractReturnLink[] objReturnLinks = new UIComponentDataContractReturnLink[1];
                objReturnLinks[0] = new UIComponentDataContractReturnLink { SourceMember = "envelopPopupOutDM", TargetMember = "envelopMainInDM" };
                objAction.DataContractReturnMap = new UIComponentDataContractReturnMap { ReturnLinks = objReturnLinks };

                Page.ActionDispatcher.ExecuteAction(objAction);
            }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public void OnPopupClose()
        {
            if (Page.DataContract.GetValueByName("envelopMainInDM") != null)
                _DataEnvelop.SS_DataPacket = Page.DataContract.GetValueByName("envelopMainInDM") as DataPacket;

            if (_DataEnvelop.SS_DataPacket != null)
            {
                if (_DataEnvelop.SS_DataPacket.IsAlertMessageAvailable &&
                    _DataEnvelop.SS_DataPacket.AlertMessages != null && _DataEnvelop.SS_DataPacket.AlertMessages.Length > 0)
                    ShowAlerts();
                else
                {
                    if (_DataEnvelop.SS_DataPacket.ResultStatusMessage != null)
                    {
                        if (_DataEnvelop.SS_DataPacket.ResultStatusMessage != "")
                        {
                            DisplayMessage(new ResultStatus(_DataEnvelop.SS_DataPacket.ResultStatusMessage, true));
                            //Page.CloseFloatingFrame(true);
                        }
                    }
                    //else
                    //    Page.CloseFloatingFrame(true);
                } //_DataEnvelop.SS_DataPacket.IsAlertMessageAvailable            
            } // _DataEnvelop.SS_DataPacket != null
            _OnJobOrder_DataChanged(null, null);
        }

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
            objLinks[0] = new UIComponentDataContractLink { SourceMember = "envelopMainOutDM", TargetMember = "envelopAlertInDM" };
            objAction.DataContractMap = new UIComponentDataContractMap{ Links = objLinks };
            
            UIComponentDataContractReturnLink[] objReturnLinks = new UIComponentDataContractReturnLink[1];
            objReturnLinks[0] = new UIComponentDataContractReturnLink{ SourceMember = "envelopAlertOutDM" , TargetMember = "envelopMainInDM" };
            objAction.DataContractReturnMap = new UIComponentDataContractReturnMap{ ReturnLinks = objReturnLinks };

            SEMI.AppCode.UIUtility.SetHorizonAlertPopupFrameLocation(this, objAction);
        }
        // wei sun : edit to add SPC end
        # endregion //SPC
    } // class
} // name space



