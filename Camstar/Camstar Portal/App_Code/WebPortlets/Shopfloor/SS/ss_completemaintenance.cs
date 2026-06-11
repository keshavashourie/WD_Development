/* Copyright 2022 Siemens */
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Generic;

using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CamstarPortal.WebControls;

using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.Services;
using SEMI.AppCode;
using Camstar.WebPortal.FormsFramework.WebControls;
using Microsoft.Ajax.Utilities;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_CompleteMaintenance : MatrixWebPart
    {
            
        protected const string _kSvcDataViewStateVariable = "SS_CompleteMaintenance_ServiceData";

        protected JQDataGrid StatusDetailsGrid { get { return FindCamstarControl("GetMaintenanceStatuses_StatusDetails") as JQDataGrid; } }

        protected OM.GetMaintenanceStatuses SelectionGridData { get { return StatusDetailsGrid.SelectionData as OM.GetMaintenanceStatuses; } } 

        protected CWC.ViewDocumentsControl ViewDocumentsCtrl { get { return Page.FindCamstarControl("Documents") as ViewDocumentsControl; } }

        protected CWC.NamedObject HiddenResource { get { return Page.FindCamstarControl("CompleteMaintenance_Resource") as CWC.NamedObject; } } // HiddenResource

        protected CWC.NamedSubentity DetailsMaintStatus { get { return Page.FindCamstarControl("MaintStatus") as CWC.NamedSubentity; } }

        protected CWC.RevisionedObject _rdoMaintReq { get { return Page.FindCamstarControl("MaintReq") as CWC.RevisionedObject; } }

        protected CWC.DateChooser NextDueDate { get { return Page.FindCamstarControl("StatusDetails_NextDateDue") as CWC.DateChooser; } }

        protected CWC.DateChooser NextDateWarning { get { return Page.FindCamstarControl("StatusDetails_NextDateWarning") as CWC.DateChooser; } }

        protected CWC.DateChooser NextDateLimit { get { return Page.FindCamstarControl("StatusDetails_NextDateLimit") as CWC.DateChooser; } }

		protected CWC.TextBox _txtMaintStatusID { get { return Page.FindCamstarControl("MaintStatusID") as CWC.TextBox; } }

        protected CWC.TextBox _txtThruputQty { get { return Page.FindCamstarControl("StatusDetails_ThruputQty") as CWC.TextBox; } }

        protected CWC.NamedObject DetailsUOM { get { return Page.FindCamstarControl("StatusDetails_UOM") as CWC.NamedObject; } }

        protected CWC.TextBox _txtThruputQtyDue { get { return Page.FindCamstarControl("StatusDetails_NextThruputQtyDue") as CWC.TextBox; } }

        protected CWC.TextBox _txtThruputQtyWarning { get { return Page.FindCamstarControl("StatusDetails_NextThruputQtyWarning") as CWC.TextBox; } }

        protected CWC.TextBox _txtThruputQtyLimit { get { return Page.FindCamstarControl("StatusDetails_NextThruputQtyLimit") as CWC.TextBox; } }

        protected CWC.TextBox _txtUsageCount { get { return Page.FindCamstarControl("StatusDetails_ss_UsageCount") as CWC.TextBox; } }

        protected CWC.TextBox _txtNextUsageCountDue { get { return Page.FindCamstarControl("StatusDetails_ss_NextUsageCountDue") as CWC.TextBox; } }

        protected CWC.TextBox _txtNextUsageCountWarning { get { return Page.FindCamstarControl("StatusDetails_ss_NextUsageCountWarning") as CWC.TextBox; } }

        protected CWC.TextBox _txtNextUsageCountLimit { get { return Page.FindCamstarControl("StatusDetails_ss_NextUsageCountLimit") as CWC.TextBox; } }

        protected JQDataGrid CheckListGrid { get { return Page.FindCamstarControl("ServiceDetails_Checklist") as JQDataGrid; } }

        protected CWC.CheckBox ForceMaintCheck { get { return Page.FindCamstarControl("CompleteMaintenance_ForceMaintenance") as CWC.CheckBox; } }

        protected JQDataGrid LinkedPMGrid { get { return Page.FindCamstarControl("CompleteMaintenance_ss_LinkedPMs") as JQDataGrid; } }

        protected CWC.TextBox _txtThruputQty2 { get { return Page.FindCamstarControl("StatusDetails_ThruputQty2") as CWC.TextBox; } }

        protected CWC.NamedObject DetailsUOM2 { get { return Page.FindCamstarControl("StatusDetails_UOM2") as CWC.NamedObject; } }

        protected CWC.TextBox _txtThruputQty2Due { get { return Page.FindCamstarControl("StatusDetails_NextThruputQty2Due") as CWC.TextBox; } }

        protected CWC.TextBox _txtThruputQty2Warning { get { return Page.FindCamstarControl("StatusDetails_NextThruputQty2Warning") as CWC.TextBox; } }

        protected CWC.TextBox _txtThruputQty2Limit { get { return Page.FindCamstarControl("StatusDetails_NextThruputQty2Limit") as CWC.TextBox; } }

        protected CWC.TextBox _txtNextQty2Delay { get { return Page.FindCamstarControl("StatusDetails_ss_NextQty2Delay") as CWC.TextBox; } }

        protected CWC.TextBox _txtComments { get { return Page.FindCamstarControl("CompleteMaintenance_Comments") as CWC.TextBox; } }

        protected CWC.RevisionedObject _rdoDataCollectionDef { get { return Page.FindCamstarControl("DataCollectionDef") as CWC.RevisionedObject; } }

        protected CWC.NamedSubentity HiddenMaintStatus { get { return Page.FindCamstarControl("MaintenanceStatus") as CWC.NamedSubentity; } }

        protected ShopFloorDCControl _dccDataCollection { get { return Page.FindCamstarControl("DataCollection") as ShopFloorDCControl; } }

        protected CWC.NamedSubentity MaintStatus { get { return Page.FindCamstarControl("ServiceDetails_MaintenanceStatus") as CWC.NamedSubentity; } }

        //----------------------------------------------------
        // 
        //----------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                OnPopupClose();
        } // OnLoad

        //----------------------------------------------------
        // 
        //----------------------------------------------------
        protected void OnPopupClose()
        {

            if (!IsEsigClose())
            {
                Page.ClearValues();
                Page.SessionVariables[_kSvcDataViewStateVariable] = null;
            }

        } // OnPopupClose

        private bool IsEsigClose()
        {
            //Check to see if this function is firing from the ESig Capture page closing - if so then don't reload/refresh
            bool esigClose = false;
            bool isSignature = false;
            UIComponentDataMember memberEsig = Page.DataContract.DataMembers.SingleOrDefault(m => m.Name == "ESigCaptureDetailsDM");
            if (memberEsig != null && memberEsig.Value != null)
            {
                // If the ESignature popup was displayed but was not executed, Then check if an actual signature was entered.  Item1 is associated to the Task Esig Requirement.
                var esigDetails = ESigCaptureUtil.CollectESigServiceDetailsAll();
                if (esigDetails.Item1 != null)
                {
                    string Signer = ((Camstar.WCF.ObjectStack.ESigPasswordCapture[])esigDetails.Item1[0].CaptureDetails)[0].Signer.Name;
                    isSignature = Signer != null ? true : false;
                    if (isSignature)
                    {
                        esigClose = true;
                    }
                }
            }
            return esigClose;
        }

        //----------------------------------------------------
        // 
        //----------------------------------------------------
        public virtual void MaintStatusGridRowSelected(object sender, JQGridEventArgs args)
        {
            DataRow rowItem;
            rowItem = (DataRow)StatusDetailsGrid.GridContext.GetItem(StatusDetailsGrid.SelectedRowID);
            if (rowItem != null)
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new Camstar.WCF.Services.CompleteMaintenanceService(session.CurrentUserProfile);

                var serviceDetails = new OM.CompleteMaintDetails[1];
                serviceDetails[0] = new OM.CompleteMaintDetails();
                serviceDetails[0].MaintenanceStatus = new OM.SubentityRef();
                serviceDetails[0].MaintenanceStatus.ID = rowItem.ItemArray[0].ToString();
                var resource = new OM.NamedObjectRef();
                resource.Name = rowItem.ItemArray[24].ToString();
                var serviceData = new OM.CompleteMaintenance
                {
                    Resource = resource,
                    ServiceDetails = serviceDetails,
                };

                var checklistSelection = new OM.MaintenanceReqChecklistChanges_Info();
                checklistSelection.RequestValue = true;
                var serviceInfo = new OM.CompleteMaintenance_Info
                {
                    ServiceDetails = new OM.CompleteMaintDetails_Info
                    {
                        ChecklistSelection = checklistSelection
                    },
                };

                var request = new Camstar.WCF.Services.CompleteMaintenance_Request();
                request.Info = serviceInfo;

                var result = new Camstar.WCF.Services.CompleteMaintenance_Result();
                var resultStatus = new OM.ResultStatus();

                resultStatus = service.GetEnvironment(serviceData, request, out result);

                if (resultStatus.IsSuccess)
                {
                    if (!result.IsEmpty)
                    {
                        if (CheckListGrid != null)
                        {
                            OM.CompleteMaintenance data = (OM.CompleteMaintenance)result.Value;
                            OM.CompleteMaintDetails[] sd = data.ServiceDetails;
                            CheckListGrid.Data = sd[0].ChecklistSelection;
                        }
                    }
                }

                _txtMaintStatusID.Data = rowItem.ItemArray[0].ToString();

                if (!resultStatus.IsSuccess)
                    Page.DisplayMessage(resultStatus);
            }
            else
            {
                HiddenResource.Data = null;
                ViewDocumentsCtrl.Data = null;
                DetailsMaintStatus.Data = null;
                _rdoMaintReq.Data = null;
                NextDueDate.Data = null;
                NextDateWarning.Data = null;
                NextDateLimit.Data = null;
                _txtMaintStatusID.Data = null;
                _txtThruputQty.Data = null;
                DetailsUOM.Data = null;
                _txtThruputQtyDue.Data = null;
                _txtThruputQtyWarning.Data = null;
                _txtThruputQtyLimit.Data = null;
                _txtUsageCount.Data = null;
                _txtNextUsageCountDue.Data = null;
                _txtNextUsageCountWarning.Data = null;
                _txtNextUsageCountLimit.Data = null;
                CheckListGrid.ClearData();
                ForceMaintCheck.Data = null;
                LinkedPMGrid.ClearData();
                _txtThruputQty2.Data = null;
                DetailsUOM2.Data = null;
                _txtThruputQty2Due.Data = null;
                _txtThruputQty2Warning.Data = null;
                _txtThruputQty2Limit.Data = null;
                _txtNextQty2Delay.Data = null;
                _txtComments.Data = null;
                _rdoDataCollectionDef.Data = null;
                HiddenMaintStatus.Data = null;
                _dccDataCollection.Data = null;
            }
        } // MaintStatusGridRowSelected

        //----------------------------------------------------
        // 
        //----------------------------------------------------
        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);

            //Clear checklist in case it is present in data
            OM.MaintenanceReqChecklistChanges[] cl = ((OM.CompleteMaintDetails[])(((OM.CompleteMaintenance)(serviceData)).ServiceDetails))[0].Checklist;
            if (cl != null)
            {
                Array.Clear(cl, 0, cl.Length);                
            }

            //Loop through checklist grid and add the selected items
            List<OM.MaintenanceReqChecklistChanges> list = new List<OM.MaintenanceReqChecklistChanges>();
            if (CheckListGrid.Data != null && CheckListGrid.GridContext.SelectedRowIDs != null)
            {
                CheckListGrid.GridContext.SelectedRowIDs.ForEach(n => { list.Add((CheckListGrid.GridContext as ItemDataContext).GetItem(n) as OM.MaintenanceReqChecklistChanges); });
               
                //Create new checklist array
                cl = list.Select(n => new OM.MaintenanceReqChecklistChanges()
                                {
                                    ListItemAction = OM.ListItemAction.Add,
                                    ChecklistId = n.ChecklistId,
                                    Instruction = n.Instruction,
                                    Comments = n.Comments==null?"":n.Comments
                                }).ToArray();
            }

            //Set checklist array in service data
            ((OM.CompleteMaintDetails[])(((OM.CompleteMaintenance)(serviceData)).ServiceDetails))[0].Checklist = cl;
        }

        //----------------------------------------------------
        // 
        //----------------------------------------------------
        public bool RequireParametricData(out OM.RevisionedObjectRef rdoDataCollectionDef)
        {
            bool bResult = false;
            rdoDataCollectionDef = null;

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            CompleteMaintenanceService svc = new CompleteMaintenanceService(session.CurrentUserProfile);
            OM.CompleteMaintenance svcInput = new OM.CompleteMaintenance();
            OM.CompleteMaintDetails[] svcDetails = new OM.CompleteMaintDetails[1];

            OM.CompleteMaintenance_Info svcInfo = new OM.CompleteMaintenance_Info();

            //Loop through checklist grid and add the selected items
            List<OM.MaintenanceReqChecklistChanges> listCheckList = new List<OM.MaintenanceReqChecklistChanges>();
            List<OM.MaintenanceReqChecklistChanges> listInput = new List<OM.MaintenanceReqChecklistChanges>();
            if (CheckListGrid.Data != null && CheckListGrid.GridContext.SelectedRowIDs != null)
            {
                CheckListGrid.GridContext.SelectedRowIDs.ForEach(n => { listCheckList.Add((CheckListGrid.GridContext as ItemDataContext).GetItem(n) as OM.MaintenanceReqChecklistChanges); });

                foreach (OM.MaintenanceReqChecklistChanges item in listCheckList)
                {
                    OM.MaintenanceReqChecklistChanges chkItem = new OM.MaintenanceReqChecklistChanges();
                    chkItem.ListItemAction = OM.ListItemAction.Add;
                    chkItem.ChecklistId = item.ChecklistId;
                    chkItem.Instruction = item.Instruction;
                    chkItem.Comments = item.Comments == null ? null : item.Comments;
                    listInput.Add(chkItem);
                }
            }

            svcDetails[0] = new OM.CompleteMaintDetails();

            svcDetails[0].MaintenanceStatus = new OM.SubentityRef();
			if (_txtMaintStatusID.Data!= null)
				svcDetails[0].MaintenanceStatus.ID = _txtMaintStatusID.Data.ToString();
            if (listCheckList != null)
                svcDetails[0].Checklist = listInput.ToArray();

            svcInput.ServiceDetails = svcDetails;

            svcInfo.DataCollectionDef = FieldInfoUtil.RequestValue();

            CompleteMaintenance_Request request = new CompleteMaintenance_Request();
            request.Info = svcInfo;

            CompleteMaintenance_Result result = new CompleteMaintenance_Result();

            OM.ResultStatus status = svc.ResolveParametricData(svcInput, request, out result);
            if (status.IsSuccess)
                if (result.Value != null)
                    if (result.Value.DataCollectionDef != null)
                    {
                        rdoDataCollectionDef = result.Value.DataCollectionDef;
                        bResult = true;
                    }

            return bResult;
        } // RequireParametricData

        //-----------------------------------------
        //
        //-----------------------------------------
        public override bool PreExecute(OM.Info serviceInfo, OM.Service serviceData)
        {
            Page.SessionVariables[_kSvcDataViewStateVariable] = null;

            OM.RevisionedObjectRef rdoDataCollectionDef = new OM.RevisionedObjectRef();
            if (RequireParametricData(out rdoDataCollectionDef))
            {
                _rdoDataCollectionDef.Data = rdoDataCollectionDef;
                PopupDataCollection(serviceData);
                return false;
            }
            else
            {
                return base.PreExecute(serviceInfo, serviceData);
            }
        } // PreExecute

        //-----------------------------------------
        //
        //-----------------------------------------
        public void PopupDataCollection( OM.Service serviceData, bool EndResponse = false)
        {            
            Page.SessionVariables[_kSvcDataViewStateVariable] = serviceData;
            //Page.DataContract.SetValueByName(_kSvcDataDM, serviceData);

            Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
            objAction.PageName = "SS_CompleteMaintenanceDCPopupVP";

            objAction.FrameLocation = new UIFloatingPageLocation();
            objAction.FrameLocation.Width = 800;
            objAction.FrameLocation.Height = 600;
            objAction.EndResponse = EndResponse;
            objAction.ShowButtons = false;

            UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[1];
            objLinks[0] = new UIComponentDataContractLink();
            objLinks[0].SourceMember = "DataCollectionDefDM";
            objLinks[0].TargetMember = "DataCollectionDefDM";
            objAction.DataContractMap = new UIComponentDataContractMap();
            objAction.DataContractMap.Links = objLinks;                   

            Page.ActionDispatcher.ExecuteAction(objAction);
        }  // ShowAlerts

        
    }
}





