// Copyright 2019 Siemens
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.PortalFramework;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_WIPSurveillanceR2 : MatrixWebPart
    {
        protected JQDataGrid _gridItemData { get { return Page.FindCamstarControl("SurveillanceStatusDetails") as JQDataGrid; } }
        protected CWC.NamedObject _ndoResourceCriteria { get { return Page.FindCamstarControl("ss_GetSurveillanceStatuses_ss_Resource") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoResourceGroupCriteria { get { return Page.FindCamstarControl("ss_GetSurveillanceStatuses_ss_ResourceGroup") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoOperationCriteria { get { return Page.FindCamstarControl("ss_GetSurveillanceStatuses_ss_Operation") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoWorkCenterCriteria { get { return Page.FindCamstarControl("ss_GetSurveillanceStatuses_ss_WorkCenter") as CWC.NamedObject; } }
        protected CWC.DropDownList _ddlFilterCriteria { get { return Page.FindCamstarControl("FilterCriteria") as CWC.DropDownList; } }
        protected CWC.Button _btnResetButton { get { return Page.FindCamstarControl("ClearAllButton") as CWC.Button; } }
        protected CWC.Button _btnSearchButton { get { return Page.FindCamstarControl("SearchButton") as CWC.Button; } }
        protected CamstarControlsCollection _SearchCriterias = new CamstarControlsCollection();
        protected JQTabContainer _tabWIPTab{ get { return Page.FindCamstarControl("WIPMain_TxnTab") as JQTabContainer; } }
        private string ServerURL = "";
        private string SurvCIOChannelAdapter = "";
        private string correlationId = "Surveillance 1.0.0";

        #region ClientScript Section

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/user/SurveillanceSignalR_R2.js");
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _SearchCriterias.Add(_ndoResourceCriteria);
            _SearchCriterias.Add(_ndoResourceGroupCriteria);
            _SearchCriterias.Add(_ndoOperationCriteria);
            _SearchCriterias.Add(_ndoWorkCenterCriteria);
            _btnResetButton.Click += _btnResetButton_Click;
            _btnSearchButton.Click += _btnSearchButton_Click;

            if (!Page.IsPostBack)
                StartHubScript();
        }

        void _btnResetButton_Click(object sender, EventArgs e)
        {
            foreach (NamedObject criteriaControl in _SearchCriterias)
            {
                criteriaControl.ClearData();
                _ddlFilterCriteria.ClearData();
                criteriaControl.Enabled = false;
                _ddlFilterCriteria.Enabled = true;
                //_gridItemData.ClearData();
            }
        }

        void _btnSearchButton_Click(object sender, EventArgs e)
        {
            StartHubScript();
        }

        public void GetSelectedFilterCriteria(object sender, EventArgs e)
        {
            if (_ddlFilterCriteria.Data != null)
            {
                foreach (NamedObject criteriaControl in _SearchCriterias)
                {
                    if (string.Equals(criteriaControl.ID, sender))
                    {
                        criteriaControl.Enabled = true;
                    }
                    else
                    {
                        criteriaControl.ClearData();
                        criteriaControl.Enabled = false;
                    }
                }
            }
            else
            {
                foreach (NamedObject criteriaControl in _SearchCriterias)
                {
                    criteriaControl.ClearData();
                    criteriaControl.Enabled = false;
                }
            }
        }

        public void StartHubScript()
        {
            GetSurveillanceServerName();

            string startupScript = string.Format("SurveillanceStartup('{0}','{1}','{2}');", ServerURL, correlationId, SurvCIOChannelAdapter);

            if (!Page.ClientScript.IsStartupScriptRegistered("SurveillanceStartup"))
                ScriptManager.RegisterStartupScript(this, GetType(), "SurveillanceStartup", startupScript, true);
            RenderToClient = true;
        }

        private void GetSurveillanceServerName()
        {
            string factoryName = Page.SessionDataContract.GetValueByName("Factory").ToString();
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            ss_SignalRSurveillanceStatusService Svc = new ss_SignalRSurveillanceStatusService(profile);
            ss_SignalRSurveillanceStatus SvcData = new ss_SignalRSurveillanceStatus();
            ss_SignalRSurveillanceStatus_Request ReqData = new ss_SignalRSurveillanceStatus_Request();
            ss_SignalRSurveillanceStatus_Result ResData = new ss_SignalRSurveillanceStatus_Result();
            ss_SignalRSurveillanceStatus_Info SvcInfo = new ss_SignalRSurveillanceStatus_Info();
            SvcInfo.ss_SurveillanceURL = FieldInfoUtil.RequestValue();
            SvcInfo.ss_SurveillanceCIOChannelAdapter = FieldInfoUtil.RequestValue();
            ReqData.Info = SvcInfo;
            ResultStatus oResultStatus = new ResultStatus();
            oResultStatus = Svc.GetEnvironment(ReqData, out ResData);


            if (oResultStatus.IsSuccess)
            {
                if (ResData.Value.ss_SurveillanceURL != null)
                    ServerURL = ResData.Value.ss_SurveillanceURL.Value;
                if (ResData.Value.ss_SurveillanceCIOChannelAdapter != null)
                    SurvCIOChannelAdapter = ResData.Value.ss_SurveillanceCIOChannelAdapter.Value;
            }
        }
    }
}


