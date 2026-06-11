// Copyright Siemens 2020
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
using PERS = Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;

/// <summary>
/// Summary description for isMaterialRequest
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class isMaterialRequest : MatrixWebPart
    {
        protected virtual CWC.NamedObject _InventoryLocation
        {
            get { return Page.FindCamstarControl("isMaterialRequest_isInventoryLocation") as CWC.NamedObject; }
        }

        protected virtual CWC.NamedObject _MaterialQueue
        {
            get { return Page.FindCamstarControl("isMaterialRequest_MaterialQueue") as CWC.NamedObject; }
        }

        protected virtual CWC.NamedObject _Resource
        {
            get { return Page.FindCamstarControl("isMaterialRequest_Resource") as CWC.NamedObject; }
        }
		
		 protected virtual CWC.NamedObject _MfgOrder
        {
            get { return Page.FindCamstarControl("isMaterialRequest_isMfgOrder") as CWC.NamedObject; }
        }

        protected virtual JQDataGrid _gridServiceDetails
        {
            get { return Page.FindCamstarControl("isMaterialRequest_ServiceDetails") as JQDataGrid; }
        }

        // Keeps track of what control had the first DataChanged event
        // Behavior changes if control is first changed, or if it is being cleared due to another controls value changed
        private string dataChangedInitiator = "";
        private bool IsDataChangedInitiator(string id)
        {
            bool retVal = false;

            if (string.IsNullOrEmpty(dataChangedInitiator))
            {
                dataChangedInitiator = id;
                retVal = true;
            }
            else if (dataChangedInitiator == id)
            {
                retVal = true;
            }

            return retVal;
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _InventoryLocation.DataChanged += new EventHandler(InventoryLocation_DataChanged);
            _MaterialQueue.DataChanged += new EventHandler(MaterialQueue_DataChanged);
            _Resource.DataChanged += new EventHandler(Resource_DataChanged);
        }

        /// <summary>
        /// Data Changed event for InventoryLocation.
        /// </summary>
        public void InventoryLocation_DataChanged(object sender, EventArgs e)
        {
            if (!IsDataChangedInitiator("IL"))
                return;

            _MaterialQueue.ClearData();
            _Resource.ClearData();
            _MfgOrder.ClearData();
            _gridServiceDetails.ClearData();
            _Resource.Enabled = true;
            _Resource.DisplayMode = PERS.DisplayModeType.PickList;
        }

        public void MaterialQueue_DataChanged(object sender, EventArgs e)
        {
            if (!IsDataChangedInitiator("MQ"))
                return;

            _InventoryLocation.ClearData();
            _Resource.ClearData();
            _MfgOrder.ClearData();
            _gridServiceDetails.ClearData();
            _Resource.Enabled = true;
            _Resource.DisplayMode = PERS.DisplayModeType.PickList;


            if (_MaterialQueue.Data != null)
            {
                //Get Resource 
                FrameworkSession fs = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                var service = new Camstar.WCF.Services.isManageMaterialQueueService(fs.CurrentUserProfile);
                var servicedata = new OM.isManageMaterialQueue();
                var serviceinfo = new OM.isManageMaterialQueue_Info();
                var result = new Camstar.WCF.Services.isManageMaterialQueue_Result();

                servicedata.isMaterialQueue = _MaterialQueue.Data as OM.NamedObjectRef;
                serviceinfo.isResource = FieldInfoUtil.RequestValue();
                var oServiceRequest = new Camstar.WCF.Services.isManageMaterialQueue_Request();
                oServiceRequest.Info = serviceinfo;

                var resultStatus = new OM.ResultStatus();

                OM.ResultStatus oResultStatus = service.GetEnvironment(servicedata, oServiceRequest, out result);

                if (oResultStatus.IsSuccess)
                {
                    if (result.Value.isResource != null)
                    {
                        _Resource.Data = result.Value.isResource.ToString();
                        _Resource.Enabled = false;
                        _Resource.DisplayMode = PERS.DisplayModeType.None;
                    }
                }
            }
        }

        public void Resource_DataChanged(object sender, EventArgs e)
        {
            if (!IsDataChangedInitiator("RS"))
                return;

            _MaterialQueue.ClearData();
            _InventoryLocation.ClearData();
            _MfgOrder.ClearData();
            _gridServiceDetails.ClearData();
        }

    }
}