// Copyright Siemens 2023  
using System;
using System.Activities.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using CPF = Camstar.WebPortal.PortalFramework;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CWF = Camstar.WebPortal.FormsFramework;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;

namespace Camstar.WebPortal.WebPortlets.GenericEvent
{
    public class ClassificationControl : MatrixWebPart
    {
        private object UpdateOrg = null;
        #region Property

        protected virtual CWC.NamedObject ServiceDetail
        {
            get { return Page.FindCamstarControl("ServiceDetail_Organization") as CWC.NamedObject; }
        }

        protected virtual CWC.NamedObject UpdateEventQualityObject
        {
            get { return Page.FindCamstarControl("UpdateEvent_QualityObject") as CWC.NamedObject; }
        }

        #endregion

        #region Events

        public event EventHandler FirstPageLoading;

        public event EventHandler FirstPagePreRendering;

        #endregion

        #region Protected methods

        protected override void OnLoad(EventArgs e)
        {
            Page.LoadComplete += Page_LoadComplete;

            if(UpdateEventQualityObject.Data != null)
            LoadOrgMaintForm();

            if (ServiceDetail.Data != null && UpdateOrg != null)
            {
                if (!UpdateOrg.Equals(ServiceDetail.Data))
                {
                    ServiceDetail.Data = (OM.NamedObjectRef)UpdateOrg;
                }

            }

            if (ServiceDetail.Data == null && UpdateEventQualityObject.Data == null)
            {
                LoadOrganization();
            }

            base.OnLoad(e);
        }

        protected virtual void Page_LoadComplete(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
                OnFirstPageLoading(sender, e);
        } // void OnLoad(EventArgs e)

        protected virtual void OnFirstPageLoading(object sender, EventArgs e)
        {
            if (FirstPageLoading != null)
                FirstPageLoading(sender, e);
        } // void OnFirstPageLoading(object sender, EventArgs e)

        protected virtual void OnFirstPagePreRendering(object sender, EventArgs e)
        {
            if (FirstPagePreRendering != null)
                FirstPagePreRendering(sender, e);
        } // void FirstPagePreRendering(object sender, EventArgs e)

        protected override void OnPreRender(EventArgs e)
        {
            if (!Page.IsPostBack)
                OnFirstPagePreRendering(this, e);

            base.OnPreRender(e);
        } // void OnPreRender(EventArgs e)

        #endregion

        #region Public methods

        public virtual void LoadOrganization()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.CreateEventService(session.CurrentUserProfile);
            var serviceData = new OM.CreateEvent();
            var request = new Camstar.WCF.Services.CreateEvent_Request();
            var result = new Camstar.WCF.Services.CreateEvent_Result();
            var resultStatus = new OM.ResultStatus();

            request.Info = new OM.CreateEvent_Info()
            {
                RequestValue = true,
                Organization = new OM.Info(true)

            };

            resultStatus = service.Load(serviceData, request, out result);

            if (resultStatus.IsSuccess)
                ServiceDetail.Data = result.Value.Organization;
        }

        public virtual void LoadOrgMaintForm()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.UpdateEventService(session.CurrentUserProfile);
            var serviceData = new OM.UpdateEvent();
            serviceData.QualityObject = (OM.NamedObjectRef)UpdateEventQualityObject.Data;
            var request = new Camstar.WCF.Services.UpdateEvent_Request();
            var result = new Camstar.WCF.Services.UpdateEvent_Result();
            var resultStatus = new OM.ResultStatus();

            request.Info = new OM.UpdateEvent_Info()
            {
                RequestValue = true,
                QualityObjectDetail = new OM.QualityObjectStatusDetail_Info()
                {

                    Organization = new OM.Info(true)
                }

            };

            resultStatus = service.Load(serviceData, request, out result);
            if (resultStatus.IsSuccess)
                UpdateOrg = result.Value.QualityObjectDetail.Organization;

        }

        #endregion
    }
}
