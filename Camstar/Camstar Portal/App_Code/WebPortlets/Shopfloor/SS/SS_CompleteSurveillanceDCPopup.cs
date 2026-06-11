/* Copyright 2024 Siemens */
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
using CamstarPortal.WebControls;

/// <summary>
/// Summary description for SS_CompleteSurveillanceDCPopup
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_CompleteSurveillanceDCPopup: MatrixWebPart
    {
        protected CWC.RevisionedObject _rdoDataCollectionDef { get { return Page.FindCamstarControl("DataCollectionDef") as CWC.RevisionedObject; } }
        private ShopFloorDCControl _dcParamData { get { return Page.FindCamstarControl("CompleteSurv_DataCollection") as ShopFloorDCControl; } }
        private bool dataPointsRequested = true;
        protected const string _kSvcDataViewStateVariable = "SS_CompleteSurveillance_ServiceData";

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
                Service.LoadServiceValues("ss_CompleteSurveillance", "GetDataPoints");
        }

        public override void DisplayValues(WCF.ObjectStack.Service serviceData)
        {
            ShopFloor data = (ShopFloor)serviceData;
            _dcParamData.DisplayValues(data);

            base.DisplayValues(serviceData);
        }

        public override void RequestValues(Info serviceInfo, Service serviceData)
        {
            base.RequestValues(serviceInfo, serviceData);
            ShopFloor_Info info = (ShopFloor_Info)serviceInfo;

            Camstar.WCF.ObjectStack.ss_CompleteSurveillance data = (Camstar.WCF.ObjectStack.ss_CompleteSurveillance)serviceData;
            if (dataPointsRequested)
                _dcParamData.RequestValues(data, info);
            data.DataCollectionDef = _rdoDataCollectionDef.Data as RevisionedObjectRef;
        }

        public override void ClearValues(Service serviceData)
        {
            base.ClearValues(serviceData);
            _dcParamData.Clean();
            _dcParamData.IterationCount = 1;           
        }

        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            ResultStatus oResultStatus = new ResultStatus();

            if (action != null && action.Parameters == "DCPopupOkay")
            {
                e.Result = CompleteSurveillanceExecute();
                if (e.Result.IsSuccess)
                {
                    Page.SessionVariables[_kSvcDataViewStateVariable] = null;
                    // pass the message from child form to parent form before popup is closed
                    var parentCallStack = Page.PortalContext.LocalSession["ParentCallStack"] as CallStack;
                    parentCallStack.Context.Message = e.Result.Message;

                    Page.CloseFloatingFrameOnSubmit(e.Result);
                }
                else
                    oResultStatus = e.Result;
            }
        }

        protected ResultStatus CompleteSurveillanceExecute()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            ss_CompleteSurveillanceService svc = new ss_CompleteSurveillanceService(session.CurrentUserProfile);
            ss_CompleteSurveillance svcData = new ss_CompleteSurveillance();
            ResultStatus oStatus = new ResultStatus();
           

            if (Page.SessionVariables[_kSvcDataViewStateVariable] != null)
            {
                
                svcData = Page.SessionVariables[_kSvcDataViewStateVariable] as ss_CompleteSurveillance;
                DataPointSummary[] dataPointSummary = _dcParamData.GetDataPointSummary();
                if (dataPointSummary != null && dataPointSummary.Length > 0)
                    svcData.ParametricData = dataPointSummary[0];

                if (_rdoDataCollectionDef.Data != null)
                    svcData.DataCollectionDef = _rdoDataCollectionDef.Data as RevisionedObjectRef;

                ss_CompleteSurveillance_Result svcResult = new ss_CompleteSurveillance_Result();
                oStatus = svc.ExecuteTransaction(svcData);  
               
            }
            else
            {
                oStatus.IsSuccess = false;
                oStatus.ExceptionData = new ExceptionDataType();
                oStatus.ExceptionData.Description = "Missing service data. Try closing and resubmtting the transaction again";
            }

            return oStatus;
        }
    }

}



