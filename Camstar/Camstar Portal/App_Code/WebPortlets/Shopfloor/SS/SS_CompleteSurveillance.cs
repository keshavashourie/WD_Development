/* Copyright 2019 Siemens */
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

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_CompleteSurveillance : MatrixWebPart
    {
            
        protected const string _kSvcDataViewStateVariable = "SS_CompleteSurveillance_ServiceData";

        protected JQDataGrid StatusDetailsGrid { get { return FindCamstarControl("GetSurvStatuses_StatusDetails") as JQDataGrid; } }

        protected OM.GetMaintenanceStatuses SelectionGridData { get { return StatusDetailsGrid.SelectionData as OM.GetMaintenanceStatuses; } }

        protected CWC.NamedSubentity MaintStatus { get { return Page.FindCamstarControl("ss_ServiceDetails_ss_SurveillanceStatus") as CWC.NamedSubentity; } }

        protected CWC.TextBox _txtSurvStatusID { get { return Page.FindCamstarControl("SurvStatusID") as CWC.TextBox; } }

        protected CWC.RevisionedObject _rdoDataCollectionDef { get { return Page.FindCamstarControl("ss_CompleteSurveillance_DataCollectionDef") as CWC.RevisionedObject; } }

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
            Page.ClearValues();
            Page.SessionVariables[_kSvcDataViewStateVariable] = null;
        } // OnPopupClose

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
                var service = new Camstar.WCF.Services.ss_CompleteSurveillanceService(session.CurrentUserProfile);

                var serviceDetails = new OM.ss_CompleteSurvDetails[1];
                serviceDetails[0] = new OM.ss_CompleteSurvDetails();
                serviceDetails[0].ss_SurveillanceStatus = new OM.SubentityRef();
                serviceDetails[0].ss_SurveillanceStatus.ID = rowItem.ItemArray[0].ToString();
                var resource = new OM.NamedObjectRef();
                resource.Name = rowItem.ItemArray[24].ToString();
                var serviceData = new OM.ss_CompleteSurveillance
                {
                    //Resource = resource,
                    ServiceDetails = serviceDetails,
                };

                var request = new Camstar.WCF.Services.ss_CompleteSurveillance_Request();

                var result = new Camstar.WCF.Services.ss_CompleteSurveillance_Result();
                var resultStatus = new OM.ResultStatus();

                resultStatus = service.GetEnvironment(serviceData, request, out result);                

                _txtSurvStatusID.Data = rowItem.ItemArray[0].ToString();

                if (!resultStatus.IsSuccess)
                    Page.DisplayMessage(resultStatus);
            }
        } // MaintStatusGridRowSelected

        //----------------------------------------------------
        // 
        //----------------------------------------------------
        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);
        }

        //----------------------------------------------------
        // 
        //----------------------------------------------------
        public bool RequireParametricData(out OM.RevisionedObjectRef rdoDataCollectionDef)
        {
            bool bResult = false;
            rdoDataCollectionDef = null;

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            ss_CompleteSurveillanceService svc = new ss_CompleteSurveillanceService(session.CurrentUserProfile);
            OM.ss_CompleteSurveillance svcInput = new OM.ss_CompleteSurveillance();
            OM.ss_CompleteSurvDetails[] svcDetails = new OM.ss_CompleteSurvDetails[1];

            OM.ss_CompleteSurveillance_Info svcInfo = new OM.ss_CompleteSurveillance_Info();

            svcDetails[0] = new OM.ss_CompleteSurvDetails();

            svcDetails[0].ss_SurveillanceStatus = new OM.SubentityRef();
            svcDetails[0].ss_SurveillanceStatus.ID = _txtSurvStatusID.Data.ToString();
            
            svcInput.ServiceDetails = svcDetails;

            svcInfo.DataCollectionDef = FieldInfoUtil.RequestValue();

            ss_CompleteSurveillance_Request request = new ss_CompleteSurveillance_Request();
            request.Info = svcInfo;

            ss_CompleteSurveillance_Result result = new ss_CompleteSurveillance_Result();

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
            objAction.PageName = "SS_CompleteSurveillanceDCPopupVP";

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





