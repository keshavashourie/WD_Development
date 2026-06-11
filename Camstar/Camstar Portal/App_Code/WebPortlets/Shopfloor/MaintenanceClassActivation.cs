/* Copyright 2025 Siemens */
using System;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using System.Linq;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WCF.Services;
using System.Collections.Generic;
using Camstar.WebPortal.WebPortlets.ComponentIssue;
using Camstar.WebPortal.Utilities;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Web;
using Camstar.WebPortal.FormsFramework;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class MaintenanceClassActivation : MatrixWebPart
    {
        //Controls declaration
        private CWC.RevisionedObject MaintenanceReqField { get { return Page.FindCamstarControl("MaintenanceReqField") as CWC.RevisionedObject; } }
        private JQDataGrid MaintClassesGrid { get { return Page.FindCamstarControl("MaintClassesGrid") as JQDataGrid; } }


        //---------------------------------------------------
        // Override PostExecute event
        //---------------------------------------------------
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (MaintenanceReqField.Data != null)
                FetchGridData();
        }

        private void FetchGridData()
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            MaintClassActivation oServiceData = new MaintClassActivation();
            MaintClassActivation_Info oServiceInfo = new MaintClassActivation_Info();
            MaintClassActivation_Request oRequest = new MaintClassActivation_Request();
            MaintClassActivation_Result oResult = new MaintClassActivation_Result();
            MaintClassActivationService oService = new MaintClassActivationService(fs.CurrentUserProfile);
            ResultStatus oResultStatus = new ResultStatus();

            oServiceData.MaintenanceReq = MaintenanceReqField.Data as RevisionedObjectRef;

            oServiceInfo.ServiceDetails = new MaintClassActivationDetails_Info();
            oServiceInfo.ServiceDetails.Activated = FieldInfoUtil.RequestValue();
            oServiceInfo.ServiceDetails.OriginalActivated = FieldInfoUtil.RequestValue();
            oServiceInfo.ServiceDetails.MaintenanceClass = FieldInfoUtil.RequestValue();
            oServiceInfo.ServiceDetails.MaintenanceClassName = FieldInfoUtil.RequestValue();
            oServiceInfo.ServiceDetails.ResourceCount = FieldInfoUtil.RequestValue();
            oServiceInfo.ServiceDetails.ActiveCount = FieldInfoUtil.RequestValue();
            oServiceInfo.ServiceDetails.InactiveCount = FieldInfoUtil.RequestValue();

            oRequest.Info = oServiceInfo;

            oResultStatus = oService.Load(oServiceData, oRequest, out oResult);
            if (oResultStatus.IsSuccess)
            {
                if (oResult.Value.ServiceDetails != null)
                {
                    (MaintClassesGrid.GridContext as BoundContext).Data = oResult.Value.ServiceDetails.ToArray();
                    MaintClassesGrid.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(MaintClassesGrid);
                }
            }
            else
            {
                DisplayMessage(oResultStatus);
            }
        }

    }
}




