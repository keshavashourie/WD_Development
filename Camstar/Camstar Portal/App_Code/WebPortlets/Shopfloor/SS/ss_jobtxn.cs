/* Copyright 2024 Siemens */
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
using Camstar.WebPortal.FormsFramework;


/// <summary>
/// Summary description for SS_JobTxn
/// General property & method for Job Transaction 
/// </summary>
/// 

/*
 *      Naming Conversion :
        JQDataGrid _gridXXX { }
        CWC.TextBox _txtXXX { }
        CWC.NamedObject _ndoXXX { }
        CWC.RevisionedObject _rdoXXX { }
        CWC.DropDownList _ddlXXX { }
        CWC.Button _btnXXX { }
        CWC.CheckBox _chkXXX { }
        CWC.RadioButton _rdbXXX { }
        CWC.DateChooser _dateXXX { }
        CWC.FlyoutDropDown _flyXXX { }
        Camstar.WebPortal.PortalFramework.SectionDropDown _sectXXX { }
        CamstarPortal.WebControls.ShopFloorDCControl _dcXXXX { }
        CWC.ViewDocumentsControl _docXX { }
        ContainerListGrid _contXXX { }
        Camstar.WebPortal.PortalFramework.ToggleContainer _togXXX { }
 
 * */
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_JobTxn : MatrixWebPart
    {
        #region Property
        
        // Constant value 
        public enum TrigerAfter { None = 0, StageDataChanged = 1, ToStageDataChanged = 2, ResourceDataChanged = 3, JobOrderDataChanged = 4 };
        public enum ESDocumentType { JobModel = 1, Stage = 2, ToStage = 3, Checklist };
        public enum DisplayMode { StandAlone = 1, PopUpPage = 2 };
        public enum TransactionMode { JobCreate, JobAssign, JobAcknowledge, JobClockOn, JobClockOff, JobProgress, JobCancel, JobComplete };

        // Resource panel
        protected CWC.NamedObject _ndoResource { get { return Page.FindCamstarControl("Job_Resource") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoJobOrder { get { return Page.FindCamstarControl("Job_JobOrder") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoJobModel { get { return Page.FindCamstarControl("Job_JobModel") as CWC.NamedObject; } }
        protected CWC.ReadOnlyEnum _enumJobType { get { return Page.FindCamstarControl("Job_JobType") as CWC.ReadOnlyEnum; } }
        protected CWC.CheckBox _chkIsSimpleMode { get { return Page.FindCamstarControl("Job_IsSimpleMode") as CWC.CheckBox; } }

        // Code Symptom,Cause , Repair 
        protected CWC.NamedObject _ndoSymptomCode { get { return Page.FindCamstarControl("Job_SymptomCode") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoCauseCode { get { return Page.FindCamstarControl("Job_CauseCode") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoRepairCode { get { return Page.FindCamstarControl("Job_RepairCode") as CWC.NamedObject; } }
        
        // Grid 
        protected JQDataGrid _gridJobTechnician { get { return Page.FindCamstarControl("Job_Technicians") as JQDataGrid; } }
        protected JQDataGrid _gridMaintenanceStatus { get { return Page.FindCamstarControl("Job_MaintenanceStatuses") as JQDataGrid; } }
        
        // Document
        protected CWC.ViewDocumentsControl _docStageDocument { get { return Page.FindCamstarControl("StageDocSet") as CWC.ViewDocumentsControl; } }
        protected CWC.ViewDocumentsControl _docModelDocument { get { return Page.FindCamstarControl("ModelDocSet") as CWC.ViewDocumentsControl; } }
        protected CWC.ViewDocumentsControl _docChecklistDocument { get { return Page.FindCamstarControl("DocumentList") as CWC.ViewDocumentsControl; } }
        protected CWC.ViewDocumentsControl _docToStageDocument { get { return Page.FindCamstarControl("ToStageDocSet") as CWC.ViewDocumentsControl; } }
        protected CWC.NamedObject _ndoJobModelDocumentTxt { get { return Page.FindCamstarControl("JobModelDoc") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoStageDocumentTxt { get { return Page.FindCamstarControl("SS_JobModelDocumentSet") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoToStageDocumentTxt { get { return Page.FindCamstarControl("SS_JobDocSetToStage") as CWC.NamedObject; } }
        protected SectionDropDown _sectJobModel { get { return Page.FindCamstarControl("btnModelDoc") as SectionDropDown; } }
        protected SectionDropDown _sectStageDoc { get { return Page.FindCamstarControl("btnStageDoc") as SectionDropDown; } }
        protected SectionDropDown _sectToStageDoc { get { return Page.FindCamstarControl("btnToStageDoc") as SectionDropDown; } }  

        // Stage 
        protected CWC.DropDownList _ddlStageSequence { get { return Page.FindCamstarControl("Job_StageSequence") as CWC.DropDownList; } }
        protected CWC.NamedObject _ndoStage { get { return Page.FindCamstarControl("Job_Stage") as CWC.NamedObject; } }
        
        // hidden value 
        protected CWC.NamedObject _ndoHdnResource { get { return Page.FindCamstarControl("HiddenResource") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoHdnJobOrder { get { return Page.FindCamstarControl("HiddenJobOrder") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoHdnJobModel { get { return Page.FindCamstarControl("HiddenJobModel") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoHdnResourceCode { get { return Page.FindCamstarControl("Hidden_JobResource") as CWC.NamedObject; } }
        protected CWC.TextBox _txtHdnIsPostBack { get { return Page.FindCamstarControl("hdnIsPostBack") as CWC.TextBox; } }
        protected CWC.TextBox _txtHdnPartReq { get { return Page.FindCamstarControl("HiddenPartRequest") as CWC.TextBox; } }
        private CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("Job_ComputerName") as CWC.TextBox; } }

        // Data contract 
        protected String _dmIsPopUp; 

        // variable 
        protected DisplayMode _pageView; // page view mode, the value is StandAlone or PopUpPage 
        protected TrigerAfter isTriggered; // indicate event after triggered
        protected bool isNeedRefresh;
        
        #endregion // Property
        
        /*
         * All method to handle event from a Page will be in this region 
         */
        #region Page Event 

        //
        // Contructor SS_JobTxn, set default value 
        //
        public SS_JobTxn()
        {
            // set default value
            _pageView = DisplayMode.StandAlone; 
            isTriggered = TrigerAfter.None;
            isNeedRefresh = false;
        }

        //
        // Page OnLoad Event 
        //
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                _txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
                SEMI.AppCode.UIUtility.MaximizePopUp(this);
            }

            try
            {
                // check whether this is popup page or standalone 
                _dmIsPopUp = Page.PortalContext.DataContract.GetValueByName("IsPopUp") == null ? "" : Page.PortalContext.DataContract.GetValueByName("IsPopUp").ToString();
            }
            catch (Exception x)
            {
                _dmIsPopUp = ""; 
            }

            _ddlStageSequence.PreRender += new EventHandler(SetFirstStageSequence); 
            if( _ndoResource != null )
                _ndoResource.DataChanged += new EventHandler(_OnJobResource_DataChanged);

            // if job order exist and it is not pop up page, OnJobOrder datachanged method activate  & the pageview is assigned 
            if (_ndoJobOrder != null && _dmIsPopUp == "")
            {
                _ndoJobOrder.DataChanged += new EventHandler(_OnJobOrder_DataChanged);
                _pageView = DisplayMode.StandAlone; // set page view mode to standalone
            }
            else
            {
                _pageView = DisplayMode.PopUpPage; // set page view mode to popup 
                if (!Page.IsPostBack)
                {
                    _OnJobOrder_DataChanged(null, null);
                }
                if (PrimaryServiceType != TransactionMode.JobCreate.ToString())
                {
                    _ndoJobOrder.ReadOnly = true;
                    _ndoResource.ReadOnly = true;
                }

                try
                {
                    (this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name.Equals("btnReset")).FirstOrDefault() as Personalization.UIAction).IsHidden = true;
                }
                catch(Exception ex)
                {
                    //throw (ex); 
                    // currently do nothing
                }
            }
        } // protected override void OnLoad

        //
        // Page OnLoad Event 
        //
        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);
            JobOrderTechnicianChanges[] getGrid = _gridJobTechnician.Data != null ? (_gridJobTechnician.Data as Array).Cast<JobOrderTechnicianChanges>().ToArray() : new JobOrderTechnicianChanges[] { };
            if (getGrid.Count() > 0)
            {
                if (serviceData is OM.JobAssign || serviceData is OM.JobCreate)
                {
                    (serviceData as OM.JobTxn).Technicians = new JobOrderTechnicianChanges[getGrid.Count()];
                    for (int i = 0; i < getGrid.Count(); i++)
                    {
						if (getGrid[i].Technician != null)
							(serviceData as OM.JobTxn).Technicians[i] = new JobOrderTechnicianChanges { Technician = new NamedObjectRef{ Name = getGrid[i].Technician.Name} }; 
                    }
                } //if (serviceData is OM.JobAssign || serviceData is OM.JobCreate)
                else
                    (serviceData as OM.JobTxn).Technicians = null;
            } //if (getGrid.Count() > 0)
            else
                (serviceData as OM.JobTxn).Technicians = null;
        } //public override void GetInputData

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "fixFlyoutDropDown", "fixFlyoutDropDown(['btnMaterialfly','btnHistory']);", true);
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Name == "btnReset")
                Page.ShopfloorReset(sender, e);
        }
      
        #endregion Page Event

        /*
         * All method to handle event from a control will be in this region 
         */
        #region Control Event 

        //
        // handle Resource data changed, set the first list as default value for Job order 
        //
        protected void _OnJobResource_DataChanged(object sender, EventArgs e)
        {
			if (Page.EventArgument != "FloatingFrameSubmitParentPostBackArgument")
				Page.StatusBar.ClearMessage();
				
            _ndoResource.OriginalData = _ndoResource.Data;
            isTriggered = TrigerAfter.ResourceDataChanged; 
            Result rslt = GetJobOrderData(PrimaryServiceType); // get Job order data
            if (rslt != null && _pageView == DisplayMode.StandAlone ) // check if resource has job order 
            {
                Camstar.WCF.ObjectStack.Environment order = (rslt as ICreator).GetValue("Environment.JobOrder") as Camstar.WCF.ObjectStack.Environment;
                if (order.SelectionValues != null)
                {
                    if (order.SelectionValues.Rows != null)
                    {
                        _ndoJobOrder.Data = order.SelectionValues.Rows[0].Values[0];
                        _ndoJobOrder.SetSelectionValues(order.SelectionValues);
                    }
                }
                
            } //if (rslt != null )
			_OnJobOrder_DataChanged(sender, e);
        } // protected void _OnJobResource_DataChanged
        
        //
        // handle Job Order data changed, set all necessary field value from database
        //
        protected void _OnJobOrder_DataChanged(object sender, EventArgs e)
        {
            if (Page.EventArgument != "FloatingFrameSubmitParentPostBackArgument")
				Page.StatusBar.ClearMessage();
				
            if (!_ndoJobOrder.IsEmpty )
            { 
                Result rslt = GetJobData(PrimaryServiceType);
                if (rslt != null) //if the result has data then set the value of general field 
                {
                    SetGeneralFields(rslt);
                }
            }
            else
            {
                _chkIsSimpleMode.ClearData();
            }
        } //protected void _OnJobOrder_DataChanged
                
        #endregion Control Event

        /*
         * All method that return some data  
         */
        #region Get/Load data

        //
        // return all field data 
        //
        protected Result GetJobData(string pService)
        {
            // set event status 
            isTriggered = TrigerAfter.JobOrderDataChanged;
           
            // set data request 
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            var service = new WSDataCreator().CreateService(pService, profile);
            var cdo = WCFObject.CreateObject(pService) as ICreator;
            var request = WCFObject.CreateObject(pService + "_Request") as ICreator;
            var reqInfo = WCFObject.CreateObject(pService + "_Info") as ICreator;
            ResultStatus res = new ResultStatus(null, false);
            Result result = null;            

            // Set Data Input
            cdo.SetValue("JobOrder", _ndoJobOrder.Data as NamedObjectRef);
            cdo.SetValue("Resource", _ndoResource.Data as NamedObjectRef);

            // Set Request Value
            reqInfo.SetValue("JobModel", new OM.Info(true, true));
            reqInfo.SetValue("IsSimpleMode", new OM.Info(true));
            reqInfo.SetValue("JobModelDocumentSet", new OM.Info(true));
            reqInfo.SetValue("JobModelDetailDocumentSet", new OM.Info(true));
            if (_enumJobType != null) // check is the control exist or not, in job create actually doesnt exist
                reqInfo.SetValue("JobType", _enumJobType.RequestValue());
            reqInfo.SetValue("JobStatus", new OM.Info(true));
            reqInfo.SetValue("ExpectedStartDate", new OM.Info(true));
            reqInfo.SetValue("EstimatedDuration", new OM.Info(true));
            reqInfo.SetValue("AutoCompleteMaintenance", new OM.Info(true));
            // request stage
            reqInfo.SetValue("Stage", new OM.Info(true, true));
            reqInfo.SetValue("StageSequence", new OM.Info(true, true));
            // request technician grid
            reqInfo.SetValue("TechniciansSelection", new OM.JobOrderTechnicianChanges_Info());
            reqInfo.SetValue("TechniciansSelection.Technician", new OM.Info(true));
            reqInfo.SetValue("TechniciansSelection.TechnicianStatus", new OM.Info(true));
            reqInfo.SetValue("TechniciansSelection.AcknowledgeCount", new OM.Info(true));
            reqInfo.SetValue("TechniciansSelection.ClockOnCount", new OM.Info(true));
            // request maintenance statutes grid
            reqInfo.SetValue("MaintenanceStatuses", new OM.MaintenanceStatus_Info());
            reqInfo.SetValue("MaintenanceStatuses.MaintenanceReq", new OM.Info(true));
            reqInfo.SetValue("MaintenanceStatuses.MaintCompletionDate", new OM.Info(true));
            reqInfo.SetValue("MaintenanceStatuses.LastDateDue", new OM.Info(true));
            reqInfo.SetValue("MaintenanceStatuses.NextDateDue", new OM.Info(true));
            reqInfo.SetValue("MaintenanceStatuses.FirstMaintDateDue", new OM.Info(true));
            reqInfo.SetValue("MaintenanceStatuses.LastThruputQty", new OM.Info(true));
            reqInfo.SetValue("MaintenanceStatuses.LastThruputQty2", new OM.Info(true));
            // request code
            reqInfo.SetValue("SymptomCode", new OM.Info(true, true));
            reqInfo.SetValue("CauseCode", new Info(true, true));
            reqInfo.SetValue("RepairCode", new Info(true, true));

            if ( pService == TransactionMode.JobProgress.ToString() )
            {
                reqInfo.SetValue("RequestOrders", new OM.PartRequestOrder_Info());
                reqInfo.SetValue("RequestOrders.RequestStatus", new OM.Info(true));
                // request is require code 
                reqInfo.SetValue("RequireSymptomCode", new OM.Info(true));
                reqInfo.SetValue("RequireCauseCode", new OM.Info(true));
                reqInfo.SetValue("RequireRepairCode", new OM.Info(true));
                // request checklist selection grid
                reqInfo.SetValue("ChecklistSelection", new OM.JobModelDetailChecklistChanges_Info());
                reqInfo.SetValue("ChecklistSelection.ChecklistId", new OM.Info(true));
                reqInfo.SetValue("ChecklistSelection.Instruction", new OM.Info(true));
                reqInfo.SetValue("ChecklistSelection.DataCollectionDef", new OM.Info(true));
                reqInfo.SetValue("ChecklistSelection.DocumentSet", new OM.Info(true));
                reqInfo.SetValue("ChecklistSelection.ChecklistCount", new OM.Info(true));
                // request datacollection 
                reqInfo.SetValue("DataCollectionDef", new OM.Info(true));
            } // if pService == TransactionMode.JobProgress.ToString()
           
            request.SetValue("Info", reqInfo);

            // Execute Request 
            ResultStatus rslt = service.GetEnvironment(cdo as DCObject, request as Request, out result);

            if (rslt.IsSuccess)
                return result;
            else
                return null;

        } //public Result GetJobData

        //
        // return job order data
        //
        protected Result GetJobOrderData(string pService)
        {
            isTriggered = TrigerAfter.JobOrderDataChanged;

            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            ResultStatus res = new ResultStatus(null, false);
            var cdo = WCFObject.CreateObject(pService) as ICreator;
            var request = WCFObject.CreateObject(pService + "_Request") as ICreator;
            var reqInfo = WCFObject.CreateObject(pService + "_Info") as ICreator;
            Result result = null;
            var service = new WSDataCreator().CreateService(pService, profile);

            cdo.SetValue("Resource", _ndoResource.Data as NamedObjectRef);
            cdo.SetValue("JobOrder", _ndoJobOrder.Data as NamedObjectRef);

            reqInfo.SetValue("JobOrder", new OM.Info(true, true));
            request.SetValue("Info", reqInfo);

            ResultStatus rslt = service.GetEnvironment(cdo as DCObject, request as Request, out result);

            if (rslt.IsSuccess)
                return result;
            else
                return null; 
        }

        //
        // return document data
        //
        protected RecordSet GetDocumentData(string paramDocumentSet)
        {
            if (string.IsNullOrEmpty(paramDocumentSet))
                return null;
            // begin to get data
                FrameworkSession currentSession = FrameworkManagerUtil.GetFrameworkSession(this.Page.Session);
                QueryUtil QueryService = new QueryUtil(currentSession.CurrentUserProfile);
                RecordSet queryResult = null;
                ResultStatus resultStatus = null;
                QueryOptions queryOption = new QueryOptions
                {
                    QueryType = Camstar.WCF.ObjectStack.QueryType.User, /// querytype = system
                    StartRow = 1,// Start Row;
                    RowSetSize = 100 // Row Set Size;
                };
                // Query 
                string Sql = " SELECT " +
                                "  DB.DocumentName , D.DocumentRevision , D.BrowseMode, " +
                                " CASE WHEN DE.DocumentId = D.DocumentId THEN 'true' ELSE 'false' END ISROR , D.Identifier " +
                             " FROM  DocumentSet DS " +
                                " INNER JOIN DocumentEntry DE ON DS.DocumentSetId = DE.DocumentSetId " +
                                " INNER JOIN Document D ON (DE.DocumentId = D.DocumentId OR DE.DocumentBaseId = D.DocumentBaseId) " +
                                " INNER JOIN DocumentBase DB ON D.DocumentBaseId = DB.DocumentBaseId " +
                             " WHERE " +
                                " DS.DocumentSetName = '" + paramDocumentSet + "'";
                // execute query
                QueryService.ExecuteSQL(Sql, queryOption, ref queryResult, ref resultStatus);
                if (queryResult.Rows != null)
                    return queryResult;
                else
                    return null;
            
        }

        #endregion Get/Load data
        
        /*
         * All function / procedure 
         */
        #region Method

        //
        // Assign Data to the fields  
        //
        protected virtual void SetGeneralFields(Result rslt)
        {
            // only for popup page 
            if (_pageView == DisplayMode.PopUpPage)
            {
                var webpartResource = (MatrixWebPart)Page.CamstarControls.Where(r => r.ID == "SS_JobResourceWP").FirstOrDefault();
                (webpartResource as MatrixWebPart).CssClass = "webpart-ResourceJobstatusPop";
                // for general webpart
                _ndoHdnResource.Data = _ndoResource.Data;
                _ndoHdnJobOrder.Data = _ndoJobOrder.Data;
                _ndoHdnJobModel.Data = _ndoJobModel.Data;
                // for symptom code webpart
                if (_ndoHdnResourceCode != null)
                    _ndoHdnResourceCode.Data = _ndoResource.Data;
            } //if ( _pageView != DisplayMode.PopUpPage )

            // set stage
            string stage = ((rslt as ICreator).GetValue("Value.Stage") as NamedObjectRef).Name;
            _ndoStage.Text = stage == null ? "" : stage;

            // set Stage Sequence
            Camstar.WCF.ObjectStack.Environment stageSequence = (rslt as ICreator).GetValue("Environment.StageSequence") as Camstar.WCF.ObjectStack.Environment;
            // string stageSequenceValue = "";
            if (stageSequence.SelectionValues != null)
            {
                //stageSequenceValue = stageSequence.SelectionValues.Rows[0].Values[0];
                _ddlStageSequence.SetSelectionValues(stageSequence.SelectionValues);
                //_ddlStageSequence.Text = stageSequenceValue == null ? "" : stageSequenceValue;
                CamstarWebControl.SetRenderToClient(_ddlStageSequence);
            }

            /// set technician grid 
            _gridJobTechnician.Data = ((rslt as ICreator).GetValue("Value.TechniciansSelection") as JobOrderTechnicianChanges[]);
            _gridJobTechnician.OriginalData = _gridJobTechnician.Data;

            /// set maintenance status grid 
            if (_gridMaintenanceStatus != null)
            {
                _gridMaintenanceStatus.Data = ((rslt as ICreator).GetValue("Value.MaintenanceStatuses") as MaintenanceStatus[]);
                _gridMaintenanceStatus.OriginalData = _gridMaintenanceStatus.Data;
            }

            // set for symtomCode 
            if (_ndoSymptomCode != null)
                _ndoSymptomCode.Data = ((rslt as ICreator).GetValue("Value.SymptomCode") as NamedObjectRef).Name;
            
            if (_ndoCauseCode != null)
                _ndoCauseCode.Data = ((rslt as ICreator).GetValue("Value.CauseCode") as NamedObjectRef).Name; 
            
            if (_ndoRepairCode != null)
                _ndoRepairCode.Data = ((rslt as ICreator).GetValue("Value.RepairCode") as NamedObjectRef).Name;

            // job model document 
            if (_sectJobModel != null)
                if (((rslt as ICreator).GetValue("Value.JobModelDocumentSet") as NamedObjectRef) != null)
                {
                    ViewDocumentRefresh(((rslt as ICreator).GetValue("Value.JobModelDocumentSet") as NamedObjectRef).Name, ESDocumentType.JobModel);
                    _ndoJobModelDocumentTxt.Data = ((rslt as ICreator).GetValue("Value.JobModelDocumentSet") as NamedObjectRef).Name;
                }


            // stage document 
            if (((rslt as ICreator).GetValue("Value.JobModelDetailDocumentSet") as NamedObjectRef) != null)
            {
                _ndoStageDocumentTxt.Data = ((rslt as ICreator).GetValue("Value.JobModelDetailDocumentSet") as NamedObjectRef);
                ViewDocumentRefresh(((rslt as ICreator).GetValue("Value.JobModelDetailDocumentSet") as NamedObjectRef).Name, ESDocumentType.Stage);
            }

            if (_chkIsSimpleMode != null)
            {
          
                    _chkIsSimpleMode.CheckControl.Checked = ((rslt as ICreator).GetValue("Value.IsSimpleMode") as Boolean?) == null ? false : (bool)((rslt as ICreator).GetValue("Value.IsSimpleMode") as Boolean?); 
            }
            } // protected void SetGeneralData

        //
        // Refresh document view
        //
        public void ViewDocumentRefresh(string paramDocumentSet, ESDocumentType docType)
        {
            // Get Document Control 
            CWC.ViewDocumentsControl ctrViewDocument = GetDocumentControl(docType);
            // Get document Data
            RecordSet queryResult = GetDocumentData(paramDocumentSet);
            if (queryResult == null)
            {
                ctrViewDocument.Data = null;
                ctrViewDocument.Visible = false;
                setSectVisible(docType, ctrViewDocument.Visible);
            }
            else if (queryResult.Rows != null)
            {
                if (queryResult.Rows.Count() > 0)
                {
                    DataTable dtResult = queryResult.GetAsDataTable();
                    DocumentSet docSet = new DocumentSet();
                    docSet.DocumentEntries = new DocumentEntry[dtResult.Rows.Count];
                    dtResult.Select().ToList().ForEach(dtRow =>
                                docSet.DocumentEntries[dtRow.Table.Rows.IndexOf(dtRow)] =
                                    new DocumentEntry
                                    {
                                        DocumentIdentifier = dtRow["Identifier"].ToString(),
                                        Name = dtRow["DocumentName"].ToString(),
                                        DisplayName = dtRow["DocumentName"].ToString(),
                                        Document = new RevisionedObjectRef(dtRow["DocumentName"].ToString()),
                                        DocumentBrowseMode = int.Parse(dtRow["BrowseMode"].ToString())
                                    }
                       );
                    ctrViewDocument.Data = docSet;
                    ctrViewDocument.Visible = true;

                    setSectVisible(docType, ctrViewDocument.Visible);
                } //if (queryResult.Rows.Count() > 0)
                else
                {
                    ctrViewDocument.Visible = false;
                    setSectVisible(docType, ctrViewDocument.Visible);
                }
            } // queryResult.Rows != null
            else
            {
                ctrViewDocument.Data = null;
                ctrViewDocument.Visible = false;
                setSectVisible(docType, ctrViewDocument.Visible); 
            }
        } // public void ViewDocumentRefresh

        //
        // Set Document visibility
        //
        protected void setSectVisible(ESDocumentType docType, bool set)
        {   
            switch (docType)
                {
                    case ESDocumentType.Stage:
                        _sectStageDoc.Visible = set;
                        break; 
                    case ESDocumentType.ToStage:
                        if( !set)
                          _sectToStageDoc.Visible = set; 
                        break;
                    case ESDocumentType.Checklist:
                    case ESDocumentType.JobModel:
                        break;
                } // end switch 
        }

        #endregion Method

        /*
         * All method that return some data  
         */
        #region Helper Function 

        //
        // Set the first item of List as Data Selected 
        //
        protected virtual void SetFirstStageSequence(object sender, EventArgs e)
        {
            if (isTriggered != TrigerAfter.JobOrderDataChanged)
            {
                _ddlStageSequence.RequestValue();
                if (_ndoStage.Text != "" && _ddlStageSequence.Data != null)
                    _ddlStageSequence.Text = _ddlStageSequence.Data.ToString();
                else if (_ndoStage.Text == "")
                    _ddlStageSequence.Text = "";

                ViewDocumentRefresh(_ndoStageDocumentTxt.Text, ESDocumentType.Stage);
                isTriggered = TrigerAfter.None;
            }
        }//public void SetFirstStageSequence

        //
        // Get document control base on Document Type 
        //
        public CWC.ViewDocumentsControl GetDocumentControl(ESDocumentType docType)
        {
            CWC.ViewDocumentsControl ctrViewDocument = null;
            // return document control base on document type
            switch (docType)
            {
                case ESDocumentType.Checklist:
                    ctrViewDocument = _docChecklistDocument;
                    break;
                case ESDocumentType.Stage:
                    ctrViewDocument = _docStageDocument;
                    break;
                case ESDocumentType.ToStage:
                    ctrViewDocument = _docToStageDocument;
                    break;
                case ESDocumentType.JobModel:
                    ctrViewDocument = _docModelDocument;
                    break;
            } // end switch 

            return ctrViewDocument;
        }

        //
        // Clear Hidden Fields
        //
        public void ClearHiddenValue()
        {
            // Clear hidden value 
            if (_ndoHdnResource != null)
                _ndoHdnResource.ClearData();
            if (_ndoHdnJobOrder != null)
                _ndoHdnJobOrder.ClearData();
            if (_ndoHdnJobModel != null)
                _ndoHdnJobModel.ClearData();
        }//public void ClearHiddenValue()

        #endregion Helper Function 
    } //public class 
} //namespace



