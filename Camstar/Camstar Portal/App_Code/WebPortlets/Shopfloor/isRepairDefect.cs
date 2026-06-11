// © 2018 Siemens Product Lifecycle Management Software Inc.
using System.Data;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Web;
using System.Linq;
using OM = Camstar.WCF.ObjectStack;
using System;
using System.Collections.Generic;
using System.Data;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.Utilities;
using PERS = Camstar.WebPortal.Personalization;
using System.Runtime.Serialization;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    /// For deserializing JSON data sent in.  Represents the current defects that are going to be repaired.
    /// </summary>
    [DataContract]
    public class IS_isCurrentDefect
    {
        [DataMember(Name = "isCurrentDefectsIDString")]
        public string isCurrentDefectsId;
        [DataMember(Name = "isContainerName")]
        public string ContainerName;
        [DataMember(Name = "isRefDes")]
        public string isRefDes;
        [DataMember(Name = "Identifier")]
        public string Identifier;
        [DataMember(Name = "isDefectReasonId")]
        public string isDefectReasonId;
        [DataMember(Name = "isX")]
        public string isX;
        [DataMember(Name = "isY")]
        public string isY;
    }
    public class isRepairDefect : MatrixWebPart
    {
        #region Properties

        protected bool usingRepairAdvisorData = false;

        protected virtual CWC.ContainerList Container
        {
            get { return Page.FindCamstarControl("isDefectRepair_Container") as CWC.ContainerList; }
        }


        protected virtual CWC.TextBox RepairNotes
        {
            get
            {
                return Page.FindCamstarControl("ServiceDetails_isRepairNotes") as CWC.TextBox;
            }
        }
        protected virtual CWC.TextBox ReplacementCode
        {
            get
            {
                return Page.FindCamstarControl("isDefectRepair_isLotNumber") as CWC.TextBox;
            }
        }
        

        protected virtual JQDataGrid _gridDefectToDelete
        {
            get
            {
                return Page.FindCamstarControl("isDefectRepair_isCurrentDefectsToDelete") as JQDataGrid;
            }
        }
        protected virtual JQDataGrid _gridDefectAssociate
        {
            get
            {
                return Page.FindCamstarControl("isDefectRepair_isCurrentDefectsAssociate") as JQDataGrid;
            }
        }
        

        protected virtual JQDataGrid _gridRepairActionDetails
        {
            get
            {
                return Page.FindCamstarControl("ServiceDetails_isRepairActionDetails") as JQDataGrid;
            }
        }

        protected virtual CWC.CheckBox cbReplaceLot
        {
            get
            {
                return Page.FindCamstarControl("isDefectRepair_isReplaceComponent") as CWC.CheckBox;
            }
        }

        protected virtual CWC.TextBox DefectsToRepairJson
        {
            get { return Page.FindCamstarControl("isDefectRepair_defectsToRepairJson") as CWC.TextBox; }
        }

        protected virtual CWC.TextBox RepairAdvisorActionNames { get { return Page.FindCamstarControl("RepairAdvisorActionNames") as CWC.TextBox; } }
        #endregion


        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

            usingRepairAdvisorData = RepairAdvisorActionNames.Data != null;
            ReplacementCode.DataChanged += ReplacementCode_DataChanged;
            cbReplaceLot.DataChanged += cbReplaceLot_DataChanged;
            if (!Page.IsPostBack)
            {
                if (usingRepairAdvisorData)
                    SetRepairAdvisorSelections();
                else
                    FetchData();
            }
        }

        private void SetRepairAdvisorSelections()
        {
            string nameList = RepairAdvisorActionNames.Data.ToString();
            Array names = nameList.Split(';');
            List<OM.isRepairActionDetails> repairActions = new List<OM.isRepairActionDetails>();
            foreach (string name in names)
            {
                repairActions.Add(GetRepairActionFromName(name));
            }
            var actionsArray = repairActions.ToArray();
            (_gridRepairActionDetails.GridContext as BoundContext).Data = actionsArray;

            //TODO: below copied from other logic, but don't know what it does. verify if needed
            _gridRepairActionDetails.BoundContext.LoadData();
            CamstarWebControl.SetRenderToClient(_gridRepairActionDetails);

            //Auto select all selected actions
            for (int i = 0; i < _gridRepairActionDetails.TotalRowCount; i++)
            {
                string rowid = "00000" + i.ToString();
                _gridRepairActionDetails.GridContext.SelectRow(rowid, true);
            }

            // assign value to isCurrentDefectsToDelete grid
            _gridDefectToDelete.Data = GetDefectsToRepairSubentitiyRefs();
        }

        private OM.isRepairActionDetails GetRepairActionFromName(string name)
        {
            OM.isRepairActionDetails detail = null;

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            WCF.Services.isRepairActionMaintService service = new WCF.Services.isRepairActionMaintService(session.CurrentUserProfile);
            WCF.Services.isRepairActionMaint_Request request = new WCF.Services.isRepairActionMaint_Request();
            WCF.Services.isRepairActionMaint_Result result = new WCF.Services.isRepairActionMaint_Result();
            OM.isRepairActionMaint maint = new OM.isRepairActionMaint();
            maint.ObjectToChange = new OM.NamedObjectRef(name);
            request.Info = new OM.isRepairActionMaint_Info()
            {
                RequestValue = true,
                ObjectChanges = new OM.isRepairActionChanges_Info()
                {
                    Name = new OM.Info(true),
                    Description = new OM.Info(true),
                    isRequiresReplacement = new OM.Info(true)
                }
            };
            OM.ResultStatus status = service.Load(maint, request, out result);
            if (status.IsSuccess && result.Value.ObjectChanges != null)
            {
                detail = new WCF.ObjectStack.isRepairActionDetails();
                detail.isRepairAction = new WCF.ObjectStack.NamedObjectRef(result.Value.ObjectChanges.Name.ToString());
                detail.isRepairActionDescription = (result.Value.ObjectChanges.Description ?? "").ToString();
            }

            return detail;
        }

        void cbReplaceLot_DataChanged(object sender, EventArgs e)
        {
            if (cbReplaceLot.IsChecked)
            {
                ReplacementCode.Visible = false;
                ReplacementCode.Data = null;
            }
            else
            {
                ReplacementCode.Visible = true;
                ReplacementCode.Data = null;
            }
            FetchAssociateDefects();
        }

        void ReplacementCode_DataChanged(object sender, EventArgs e)
        {
            if (ReplacementCode.Data != null)
            {
                cbReplaceLot.Visible = false;
               
            }
            else
            {
                cbReplaceLot.Visible = true;
               
            }
            FetchAssociateDefects();
        }
        public void FetchAssociateDefects()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var oservice = new Camstar.WCF.Services.isDefectRepairService(session.CurrentUserProfile);
            var oservicedata = new OM.isDefectRepair();
            var oserviceinfo = new OM.isDefectRepair_Info();
            var oresult = new Camstar.WCF.Services.isDefectRepair_Result();

            oserviceinfo.isCurrentDefectsAssociate = FieldInfoUtil.RequestValue();

            var oServiceRequest = new Camstar.WCF.Services.isDefectRepair_Request();
            oServiceRequest.Info = oserviceinfo;
            oservicedata.Container = Container.Data as OM.ContainerRef;
            oservicedata.isCurrentDefectsToDelete = GetDefectsToRepairSubentitiyRefs();

            var resultStatus = new OM.ResultStatus();

            OM.ResultStatus oResultStatus = oservice.GetEnvironment(oservicedata, oServiceRequest, out oresult);

            if (oResultStatus.IsSuccess)
            {
                if (oresult.Value.isCurrentDefectsAssociate != null)
                {

                    var a = oresult.Value.isCurrentDefectsAssociate as OM.SubentityRef[];

                    (_gridDefectAssociate.GridContext as BoundContext).Data = a;

                    _gridDefectAssociate.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(_gridDefectAssociate);


                }
            }
        }

        /// <summary>
        /// Get all defect IDs from the JSON string and turn them into a list of SubentityRefs
        /// </summary>
        /// <returns></returns>
        public OM.SubentityRef[] GetDefectsToRepairSubentitiyRefs()
        {
            return GetDefectsToRepairSubentitiyRefs(string.Empty);
        }

        /// <summary>
        /// Get defect IDs from the JSON string and turn them into a list of SubentityRefs
        /// </summary>
        /// <param name="matchContainerName"></param>
        /// <returns></returns>
        public OM.SubentityRef[] GetDefectsToRepairSubentitiyRefs(string matchContainerName)
        {
            string defectsJson = DefectsToRepairJson.Data as string;
            // Deserialize defects into an array of objects (from a string)
            System.IO.MemoryStream defectsStream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(defectsJson));
            var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(IS_isCurrentDefect[]));
            IS_isCurrentDefect[] defectsToRepair = (IS_isCurrentDefect[])serializer.ReadObject(defectsStream);

            List<OM.SubentityRef> defectSubEntityRefs = new List<OM.SubentityRef>();
            foreach (IS_isCurrentDefect defect in defectsToRepair)
            {
                if (string.IsNullOrWhiteSpace(matchContainerName) || defect.ContainerName == matchContainerName)
                    defectSubEntityRefs.Add(new OM.SubentityRef(defect.isCurrentDefectsId));
            }
            return defectSubEntityRefs.ToArray();
        }

        public void FetchData()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var oservice = new Camstar.WCF.Services.isDefectRepairService(session.CurrentUserProfile);
            var oservicedata = new OM.isDefectRepair();
            var oserviceinfo = new OM.isDefectRepair_Info();
            var oresult = new Camstar.WCF.Services.isDefectRepair_Result();


            oservicedata.isCurrentDefectsToDelete = GetDefectsToRepairSubentitiyRefs();
            oserviceinfo.isEnableReplacementCode = FieldInfoUtil.RequestValue();
            oserviceinfo.isRepairActionDetailsSelection = new OM.isRepairActionDetails_Info()
            {
                isRepairAction = new OM.Info(true),
                isRepairActionDescription = new OM.Info(true)
               
            };
            var oServiceRequest = new Camstar.WCF.Services.isDefectRepair_Request();
            oServiceRequest.Info = oserviceinfo;

            // assign value to isCurrentDefectsToDelete grid
            _gridDefectToDelete.Data = oservicedata.isCurrentDefectsToDelete;

            var resultStatus = new OM.ResultStatus();

            OM.ResultStatus oResultStatus = oservice.GetEnvironment(oservicedata, oServiceRequest, out oresult);

            if (oResultStatus.IsSuccess)
            {
                if (oresult.Value.isRepairActionDetailsSelection != null)
                {
                    Array oRepairActionArray = oresult.Value.isRepairActionDetailsSelection.ToArray();

                    (_gridRepairActionDetails.GridContext as BoundContext).Data = oRepairActionArray;

                    _gridRepairActionDetails.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(_gridRepairActionDetails);
                }
                if (oresult.Value.isEnableReplacementCode == true)
                {
                    ReplacementCode.Enabled = true;
                }
                else
                {
                    ReplacementCode.Enabled = false;
                }

            }

            if (_gridRepairActionDetails.TotalRowCount == 1)
            {

               _gridRepairActionDetails.GridContext.SelectRow("000000",true);
                    
            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as PERS.CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {

                    case "RepairDefect":
                        {


                            e.Result = RepairDefect();
                          
                            DisplayMessage(e.Result);

                            if (e.Result.IsSuccess == true)
                                Page.CloseFloatingFrame(true);
                            break;

                        }
                }
                
            }
        }

        protected virtual OM.ResultStatus RepairDefect()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.isDefectRepairService(session.CurrentUserProfile);
            var servicedata = new OM.isDefectRepair();
            var serviceinfo = new OM.isDefectRepair_Info();
            var result = new Camstar.WCF.Services.isDefectRepair_Result();
          

            int noOfRepair = _gridDefectToDelete.TotalRowCount + _gridDefectAssociate.TotalRowCount;
            JQDataGrid selectedActionGrid = _gridRepairActionDetails;

            // Make sure they selected at least one repair action
            int noOfRepairReason = selectedActionGrid.GridContext.SelectedRowIDs != null ? selectedActionGrid.GridContext.SelectedRowIDs.Count() : 0;
            if (noOfRepairReason == 0)
                return new OM.ResultStatus("Repair Actions is required for repair defect.", false);

            servicedata.Container = Container.Data as OM.ContainerRef;
            if(cbReplaceLot.IsChecked)
                servicedata.isReplaceComponent = true;
            else
                servicedata.isReplaceComponent = false;
      
            if(ReplacementCode.Data !=null)
                servicedata.isLotNumber = ReplacementCode.Data.ToString();
            
            OM.isDefectDetail[] DefectServiceDetails = new OM.isDefectDetail[noOfRepair];

            for (int i = 0; i < noOfRepair; i++)
            {
                DefectServiceDetails[i] = new OM.isDefectDetail();
                {
                    DefectServiceDetails[i].Container = Container.Data as OM.ContainerRef;
                    if (RepairNotes.Data != null)
                        DefectServiceDetails[i].isRepairNotes = RepairNotes.Data.ToString();

                    int j = 0;

                    if (noOfRepairReason > 0)
                    {
                        DefectServiceDetails[i].isRepairActionDetails = new OM.isRepairActionDetails[noOfRepairReason];
                        foreach (OM.isRepairActionDetails repairDetaillist in (selectedActionGrid.GridContext as BoundContext).GetSelectedItems(false))
                        {
                            DefectServiceDetails[i].isRepairActionDetails[j] = new OM.isRepairActionDetails();
                            {
                                DefectServiceDetails[i].isRepairActionDetails[j].isRepairAction = repairDetaillist.isRepairAction;
                            }
                            j++;
                        }
                    }
                }
            }


            int a = 0;
            servicedata.ServiceDetails = new OM.isDefectDetail[noOfRepair];
            foreach (OM.isDefectDetail resultDetaillist in DefectServiceDetails)
            {
                servicedata.ServiceDetails[a] = new OM.isDefectDetail();
                {
                    servicedata.ServiceDetails[a].Container = resultDetaillist.Container;
                    servicedata.ServiceDetails[a].isRepairNotes = resultDetaillist.isRepairNotes;
                    servicedata.ServiceDetails[a].isRepairActionDetails = resultDetaillist.isRepairActionDetails;
                }
                a++;
            }

            servicedata.isCurrentDefectsToDelete = GetDefectsToRepairSubentitiyRefs();
            servicedata.isCurrentDefectsAssociateDetails = (_gridDefectAssociate.GridContext as BoundContext).Data as OM.SubentityRef[];
            servicedata.isIgnoreRequireReplacement = true;
            var iServiceRequest = new Camstar.WCF.Services.isDefectRepair_Request();
            iServiceRequest.Info = serviceinfo;

            var resultStatus = new OM.ResultStatus();

            OM.ResultStatus iResultStatus = service.ExecuteTransaction(servicedata, iServiceRequest, out result);

            return iResultStatus;
        }
     
     }
    
}
