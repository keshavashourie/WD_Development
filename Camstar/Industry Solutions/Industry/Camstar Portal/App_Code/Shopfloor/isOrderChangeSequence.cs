// © 2018 Siemens Product Lifecycle Management Software Inc.
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;

/// <summary>
/// Summary description for isMaterialRequestAcknowledge
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class isOrderChangeSequence : MatrixWebPart
    {
        protected virtual CWC.NamedObject _ndoLineAssignment { get { return Page.FindCamstarControl("isOrderChangeSequence_LineAssignment") as CWC.NamedObject; } }
        protected virtual JQDataGrid _gridServiceDetails { get { return Page.FindCamstarControl("isOrderChangeSequence_ServiceDetails") as JQDataGrid; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _ndoLineAssignment.DataChanged += new EventHandler(LineAssignment_DataChanged);
        }

        // Data Changed event for Line Assignment control.
        public void LineAssignment_DataChanged(object sender, EventArgs e)
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;


            isOrderChangeSequenceService Svc = new isOrderChangeSequenceService(fs.CurrentUserProfile);
            OM.isOrderChangeSequence SvcData = new OM.isOrderChangeSequence();
            isOrderChangeSequence_Info SvcInfo = new isOrderChangeSequence_Info();
            isOrderChangeSequence_Request ReqData = new isOrderChangeSequence_Request();
            isOrderChangeSequence_Result ResData = new isOrderChangeSequence_Result();

            if (_ndoLineAssignment.Data != null)
            {
                SvcData.LineAssignment = new NamedObjectRef();
                SvcData.LineAssignment.Name = _ndoLineAssignment.Data.ToString();

                SvcInfo.ServiceDetails = new isOrderChangeSeqSvcDetails_Info();
                SvcInfo.ServiceDetails.RequestValue = true;
                ReqData.Info = SvcInfo;

                OM.ResultStatus Results = Svc.GetServiceDetails(SvcData, ReqData, out ResData);
                if (Results.IsSuccess)
                {
                    if (ResData.Value.ServiceDetails != null)
                    {
                        _gridServiceDetails.Data = ResData.Value.ServiceDetails;
                    }
                    else
                    {
                        _gridServiceDetails.ClearData();
                    }
                }
            }
            else
            {
                _gridServiceDetails.ClearData();
            }
        }
    }
}