// Copyright Siemens 2023  
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

namespace Camstar.WebPortal.WebPortlets.Jobs
{
    /// <summary>
    /// Summary description for JobCreate
    /// </summary>
    public class JobCreate : MatrixWebPart
    {
        protected CWC.Button _ResourceFlyout { get { return Page.FindCamstarControl("ResourceFlyout") as CWC.Button; } }
        protected CWC.Button _JobOrderFlyout { get { return Page.FindCamstarControl("JobOrderFlyout") as CWC.Button; } }
        protected CWC.NamedObject _Resource { get { return Page.FindCamstarControl("Job_Resource") as CWC.NamedObject; } }
        protected CWC.NamedObject _JobOrder { get { return Page.FindCamstarControl("Job_JobOrder") as CWC.NamedObject; } }
        protected CWC.TextBox _SelValValue { get { return Page.FindCamstarControl("SelVal_Value") as CWC.TextBox; } }
        protected CWC.NamedObject _JobModel { get { return Page.FindCamstarControl("Job_JobModel") as CWC.NamedObject; } }
        protected CWC.TextBox _JobModelDocSet { get { return Page.FindCamstarControl("JobModelDocSet") as CWC.TextBox; } }
        protected Camstar.WebPortal.PortalFramework.SectionDropDown _DocSetSection { get { return Page.FindCamstarControl("DocSetSection") as Camstar.WebPortal.PortalFramework.SectionDropDown; } }
        protected CWC.ViewDocumentsControl _JobModelDocSetViewer { get { return Page.FindCamstarControl("JobModelDocSetViewer") as CWC.ViewDocumentsControl; } }
        protected JQDataGrid _TechGrid { get { return Page.FindCamstarControl("Job_Technicians") as JQDataGrid; } }
        protected CWC.CheckBox _IsSimpleMode { get { return Page.FindCamstarControl("JobCreate_IsSimpleMode") as CWC.CheckBox; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Page.IsPostBack)
            {
                if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument")
                {
                    OnPopupClose();
                }
            }
			else
			{
				ScriptManager.RegisterStartupScript(this.Page.Form, this.Page.GetType(), "PopUpMaximize", "pop.GetCallerPage().pop.maximize();", true);
			}

            _Resource.DataChanged += new EventHandler(_Resource_DataChanged);
            _JobOrder.DataChanged += new EventHandler(_JobOrder_DataChanged);

            ShowHideFlyoutButtons();
        }

        void _JobOrder_DataChanged(object sender, EventArgs e)
        {
            _JobModel.ClearData();
            ClearGridData();
            _JobModelDocSet.ClearData();
            _JobModelDocSetViewer.Data = null;

            if (_JobOrder.Data != null)
            {
                SetJobOrderDetails();
            }
            else
            {
                _DocSetSection.Visible = false;
                CamstarWebControl.SetRenderToClient(_DocSetSection);
            }

            ShowHideFlyoutButtons();
        }

        protected virtual void ClearGridData()
        {
            _TechGrid.ClearData();
            _TechGrid.OriginalData = null;
            _TechGrid.GridContext.CurrentPage = 1;
        }

        void _Resource_DataChanged(object sender, EventArgs e)
        {
            if (_Resource.Data != null)
            {
                _JobOrder.ClearData();
                _JobModel.ClearData();
                
				//Commented out below code because of CPR292268 - Resource has existing Job Order issue
				//if (_Resource.Data.ToString() != "")
                //    RetrieveResourceJobOrder();
                //else
                //    Page.ClearValues();
            }
            else
                Page.ClearValues();
        }

        public void SetJobOrderDetails()
        {
            OM.JobCreate objServiceData = new OM.JobCreate();
            JobCreate_Info objServiceInfo = new JobCreate_Info();
            UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
            JobCreateService objService = new JobCreateService(profile);
            ResultStatus objResultStatus = null;
            JobCreate_Result objResult = new JobCreate_Result();

            objServiceData.JobOrder = _JobOrder.Data as NamedObjectRef;
            objServiceData.Resource = _Resource.Data as NamedObjectRef;

            objServiceInfo.JobModel = FieldInfoUtil.RequestValue();
            objServiceInfo.JobModelDocumentSet = FieldInfoUtil.RequestValue();
            objServiceInfo.JobType = FieldInfoUtil.RequestValue();
            objServiceInfo.JobStatus = FieldInfoUtil.RequestValue();
            objServiceInfo.Stage = FieldInfoUtil.RequestValue();
            objServiceInfo.StageSequence = FieldInfoUtil.RequestValue();
            objServiceInfo.JobModelDetailDocumentSet = FieldInfoUtil.RequestValue();
            objServiceInfo.ExpectedStartDate = FieldInfoUtil.RequestValue();
            objServiceInfo.EstimatedDuration = FieldInfoUtil.RequestValue();
            objServiceInfo.AutoCompleteMaintenance = FieldInfoUtil.RequestValue();
            objServiceInfo.IsSimpleMode = FieldInfoUtil.RequestValue();

            objServiceInfo.TechniciansSelection = new JobOrderTechnicianChanges_Info();
            objServiceInfo.TechniciansSelection.Technician = FieldInfoUtil.RequestValue();
            objServiceInfo.TechniciansSelection.TechnicianStatus = FieldInfoUtil.RequestValue();
            objServiceInfo.TechniciansSelection.AcknowledgeCount = FieldInfoUtil.RequestValue();
            objServiceInfo.TechniciansSelection.ClockOnCount = FieldInfoUtil.RequestValue();

            objServiceInfo.MaintenanceStatuses = new MaintenanceStatus_Info();
            objServiceInfo.MaintenanceStatuses.MaintenanceReq = FieldInfoUtil.RequestValue();
            objServiceInfo.MaintenanceStatuses.MaintCompletionDate = FieldInfoUtil.RequestValue();
            objServiceInfo.MaintenanceStatuses.LastDateDue = FieldInfoUtil.RequestValue();
            objServiceInfo.MaintenanceStatuses.NextDateDue = FieldInfoUtil.RequestValue();
            objServiceInfo.MaintenanceStatuses.FirstMaintDateDue = FieldInfoUtil.RequestValue();
            objServiceInfo.MaintenanceStatuses.LastThruputQty = FieldInfoUtil.RequestValue();
            objServiceInfo.MaintenanceStatuses.LastThruputQty2 = FieldInfoUtil.RequestValue();

            objServiceInfo.SymptomCode = FieldInfoUtil.RequestValue();
            objServiceInfo.CauseCode = FieldInfoUtil.RequestValue();
            objServiceInfo.RepairCode = FieldInfoUtil.RequestValue();

            objResultStatus = objService.GetEnvironment(objServiceData, new JobCreate_Request { Info = objServiceInfo }, out objResult);
            if (objResultStatus.IsSuccess)
            {
                _JobModel.Data = objResult.Value.JobModel;
                RequestDependentSelectionValues(_JobModel.ID, new JobCreate_Info(), new OM.JobCreate());

                if (objResult.Value.IsSimpleMode != null)
                    if (objResult.Value.IsSimpleMode.Value != null)
                        _IsSimpleMode.IsChecked = objResult.Value.IsSimpleMode.Value;

                if (objResult.Value.TechniciansSelection != null)
                {
                    JobOrderTechnicianChanges[] objTech = new JobOrderTechnicianChanges[objResult.Value.TechniciansSelection.Length];
                    for (int x = 0; x < objResult.Value.TechniciansSelection.Length; x++)
                    {
                        objTech[x] = new JobOrderTechnicianChanges();
                        objTech[x] = objResult.Value.TechniciansSelection[x];
                    }

                    (_TechGrid.GridContext as ItemDataContext).Data = objTech;
                    _TechGrid.BoundContext.LoadData();
                }
            }

        }

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

            objServiceData.ObjectToChange = new NamedObjectRef(_JobModelDocSet.Data.ToString());
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
                        BrowseMode = FieldInfoUtil.RequestValue()
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
                        objDocuments[i].DocumentBrowseMode = resultDoc.Value.ObjectChanges.BrowseMode.Value;
                    }
                }

                objDocSet.DocumentEntries = objDocuments;
                _JobModelDocSetViewer.Data = objDocSet;
                _DocSetSection.Visible = true;
                CamstarWebControl.SetRenderToClient(_DocSetSection);
            }
            else
            {
                _JobModelDocSetViewer.Data = null;
                _DocSetSection.Visible = false;
                CamstarWebControl.SetRenderToClient(_DocSetSection);
            }

        }

        public void RefreshJobModelDocSet()
        {
            UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
            JobCreateService objService = new JobCreateService(profile);
            ResultStatus objResultStatus = null;
            JobCreate_Result objResult = new JobCreate_Result();

            OM.JobCreate objServiceData = new OM.JobCreate { JobModel = _JobModel.Data as NamedObjectRef };

            JobCreate_Info objServiceInfo = new JobCreate_Info
            {
                JobModelDocumentSet = FieldInfoUtil.RequestValue(),
            };

            objResultStatus = objService.GetEnvironment(objServiceData, new JobCreate_Request { Info = objServiceInfo }, out objResult);
            if (objResultStatus.IsSuccess)
            {
                //set the doc set
                if (objResult.Value.JobModelDocumentSet != null)
                {
                    _JobModelDocSet.Data = objResult.Value.JobModelDocumentSet.Name.ToString();
                    LoadDocumentSet();
                }
                else
                {
                    _JobModelDocSetViewer.Data = null;
                    _DocSetSection.Visible = false;
                    CamstarWebControl.SetRenderToClient(_DocSetSection);
                }
            } //if (objResultStatus.IsSuccess)
        }

        /// <summary>
        /// Show/hide the flyout buttons next to the Resource and Job Order fields
        /// </summary>
        public void ShowHideFlyoutButtons()
        {
            _ResourceFlyout.Style["display"] = (_Resource.Data == null) ? "none" : "inline-block";
            _JobOrderFlyout.Style["display"] = (_JobOrder.Data == null) ? "none" : "inline-block";

            CamstarWebControl.SetRenderToClient(_ResourceFlyout);
            CamstarWebControl.SetRenderToClient(_JobOrderFlyout);
        }

        public void OnPopupClose()
        {
            if (_SelValValue.Data != null)
            {
                _Resource.Data = _SelValValue.Data.ToString();
                _Resource_DataChanged(null, null);
            }
        }

        public void RetrieveResourceJobOrder()
        {
            OM.JobCreate objServiceData = new OM.JobCreate();
            JobCreate_Info objServiceInfo = new JobCreate_Info();
            UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
            JobCreateService objService = new JobCreateService(profile);
            ResultStatus objResultStatus = null;
            JobCreate_Result objResult = new JobCreate_Result();

            objServiceData.Resource = _Resource.Data as NamedObjectRef;
            objServiceInfo.JobOrder = FieldInfoUtil.RequestSelectionValue();

            objResultStatus = objService.GetEnvironment(objServiceData, new JobCreate_Request { Info = objServiceInfo }, out objResult);

            if (objResultStatus.IsSuccess)
            {
                if (objResult.Environment.JobOrder.SelectionValues != null)
                {
                    DataTable dt = objResult.Environment.JobOrder.SelectionValues.GetAsExplicitlyDataTable();
                    if (dt.Rows.Count > 0)
                    {
                        _JobOrder.Data = new NamedObjectRef(dt.Rows[0]["Name"].ToString());
                        _JobOrder_DataChanged(null, null);
                    }
                }
            }
        }
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            if (_TechGrid.BoundContext.GetTotalRows() > 0)
            {
                JobOrderTechnicianChanges[] oGridTechs = (_TechGrid.GridContext as ItemDataContext).Data as JobOrderTechnicianChanges[];
                JobOrderTechnicianChanges[] oTechs = new JobOrderTechnicianChanges[oGridTechs.Length];
                for (int x = 0; x < oGridTechs.Length; x++)
                {
                    if (oGridTechs[x] != null && oGridTechs[x].Technician != null && !string.IsNullOrEmpty(oGridTechs[x].Technician.Name))
                    {
                        oTechs[x] = new JobOrderTechnicianChanges();
                        oTechs[x].Technician = new NamedObjectRef(oGridTechs[x].Technician.Name);
                    }
                }
                (serviceData as OM.JobCreate).Technicians = oTechs;
            }
        }

    }
}
