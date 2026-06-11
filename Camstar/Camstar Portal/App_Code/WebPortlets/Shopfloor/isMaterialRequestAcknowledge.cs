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
/// Summary description for isMaterialRequestAcknowledge
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class isMaterialRequestAcknowledge : MatrixWebPart
    {
        protected virtual CWC.NamedObject InventoryLocation
        {
            get { return Page.FindCamstarControl("isMaterialRequestAcknowledge_isInventoryLocation") as CWC.NamedObject; }
        }

        protected virtual JQDataGrid _gridServiceDetails
        {
            get { return Page.FindCamstarControl("isMaterialRequestAcknowledge_ServiceDetails") as JQDataGrid; }
        }

        protected virtual CWC.NamedObject MaterialQueue
        {
            get { return Page.FindCamstarControl("isMaterialRequestAcknowledge_isMaterialQueue") as CWC.NamedObject; }
        }

        protected virtual CWC.NamedObject Resource
        {
            get { return Page.FindCamstarControl("isMaterialRequestAcknowledge_Resource") as CWC.NamedObject; }
        }

        protected virtual PERS.UIAction SubmitAction
        {
            get { return Page.ActionDispatcher.GetActionByName("SubmitAction"); }
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
            InventoryLocation.DataChanged += new EventHandler(InventoryLocation_DataChanged);
            MaterialQueue.DataChanged += new EventHandler(MaterialQueue_DataChanged);
            Resource.DataChanged += new EventHandler(Resource_DataChanged);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            if (serviceData is Camstar.WCF.ObjectStack.isMaterialReqAcknowledge_v2)
            {
                int NoOfServiceDetails = 0;
                if (_gridServiceDetails.TotalRowCount != 0)
                {
                    NoOfServiceDetails = _gridServiceDetails.TotalRowCount;
                }

                OM.isMaterialReqTxnSvcDetail[] SvcDetails = new OM.isMaterialReqTxnSvcDetail[NoOfServiceDetails];
                int x = 0;
                if (_gridServiceDetails.Data != null)
                {
                    foreach (OM.isMaterialReqTxnSvcDetail resultReqAcknowledgeList in _gridServiceDetails.Data as OM.isMaterialReqTxnSvcDetail[])
                    {

                        SvcDetails[x] = new OM.isMaterialReqTxnSvcDetail();
                        SvcDetails[x].ListItemAction = OM.ListItemAction.Add;
                        SvcDetails[x].isMaterialRequestStatus = resultReqAcknowledgeList.isMaterialRequestStatus;
                        SvcDetails[x].Product = resultReqAcknowledgeList.Product;
                        SvcDetails[x].ProductDescription = resultReqAcknowledgeList.ProductDescription;
                        SvcDetails[x].isUOM = resultReqAcknowledgeList.isUOM;
                        SvcDetails[x].QtyRequested = resultReqAcknowledgeList.QtyRequested;
                        SvcDetails[x].RemainingQty = resultReqAcknowledgeList.RemainingQty;
                        SvcDetails[x].isLot = resultReqAcknowledgeList.isLot;
                        SvcDetails[x].ReceivedQty = resultReqAcknowledgeList.ReceivedQty;
						SvcDetails[x].isMfgOrder = resultReqAcknowledgeList.isMfgOrder;
                        SvcDetails[x].CancelRequest = resultReqAcknowledgeList.CancelRequest;
                        x++;

                    }
                }
                int y = 0;
                var servicedata2 = serviceData as Camstar.WCF.ObjectStack.isMaterialReqAcknowledge_v2;
                servicedata2.ServiceDetails = new isMaterialReqTxnSvcDetail[NoOfServiceDetails];
                servicedata2.isMaterialQueue = MaterialQueue.Data as OM.NamedObjectRef;

                foreach (OM.isMaterialReqTxnSvcDetail resultRequestDetaillist in SvcDetails)
                {
                    servicedata2.ServiceDetails[y] = new isMaterialReqTxnSvcDetail();
                    {
                        servicedata2.ServiceDetails[y].ListItemAction = OM.ListItemAction.Add;
                        servicedata2.ServiceDetails[y].isMaterialRequestStatus = resultRequestDetaillist.isMaterialRequestStatus;
                        servicedata2.ServiceDetails[y].Product = resultRequestDetaillist.Product;
                        servicedata2.ServiceDetails[y].ProductDescription = resultRequestDetaillist.ProductDescription;
                        servicedata2.ServiceDetails[y].isUOM = resultRequestDetaillist.isUOM;
                        servicedata2.ServiceDetails[y].QtyRequested = resultRequestDetaillist.QtyRequested;
                        servicedata2.ServiceDetails[y].RemainingQty = resultRequestDetaillist.RemainingQty;
                        servicedata2.ServiceDetails[y].isLot = resultRequestDetaillist.isLot;
                        servicedata2.ServiceDetails[y].ReceivedQty = resultRequestDetaillist.ReceivedQty;
						servicedata2.ServiceDetails[y].isMfgOrder = resultRequestDetaillist.isMfgOrder;
                        servicedata2.ServiceDetails[y].CancelRequest = resultRequestDetaillist.CancelRequest;

                    }
                    y++;
                }

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
                    case "Submit":
                        {
                            if (MaterialQueue.Data != null)
                            {
                                e.Result = Page.Service.Submit("isMaterialReqAcknowledge_v2");
                            }
                            else
                            {                                
                                e.Result = Page.Service.Submit("isMaterialRequestAcknowledge");
                            }
                            break;
                        }
                }
            }
        }


        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);

            if (status.IsSuccess)
            {
                var materialQueue = Page.FindCamstarControl("isMaterialRequestAcknowledge_isMaterialQueue") as CWC.NamedObject;

                if (materialQueue != null && materialQueue.SelectionData != null)
                {
                    object temp = new Object();
                    temp = materialQueue.SelectionData;

                    Page.ClearValues();
                    materialQueue.Data = temp;

                }
                else
                {
                    Page.ClearValues();
                }
            }
        }

        /// <summary>
        /// Data Changed event for InventoryLocation.
        /// </summary>
        public void InventoryLocation_DataChanged(object sender, EventArgs e)
        {
            if (!IsDataChangedInitiator("IL"))
                return;

            MaterialQueue.ClearData();
            Resource.ClearData();
            Resource.Enabled = true;
            Resource.DisplayMode = PERS.DisplayModeType.PickList;

            UpdateColumnsForInventoryLocationOrResource();

            if (InventoryLocation.Data == null)
            {
                _gridServiceDetails.ClearData();
                return;
            }

            GetDetailsForInventoryLocationOrResource();
        }

        public void MaterialQueue_DataChanged(object sender, EventArgs e)
        {
            if (!IsDataChangedInitiator("MQ"))
                return;

            InventoryLocation.ClearData();
            Resource.ClearData();
            Resource.Enabled = true;
            Resource.DisplayMode = PERS.DisplayModeType.PickList;

            UpdateColumnsForMaterialQueue();

            if (MaterialQueue.Data == null)
            {
                _gridServiceDetails.ClearData();
                return;
            }

            //Check for associated Resource and update resource control if have one
            FrameworkSession fs = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.isManageMaterialQueueService(fs.CurrentUserProfile);
            var servicedata = new OM.isManageMaterialQueue();
            var serviceinfo = new OM.isManageMaterialQueue_Info();
            var result = new Camstar.WCF.Services.isManageMaterialQueue_Result();

            servicedata.isMaterialQueue = MaterialQueue.Data as OM.NamedObjectRef;
            serviceinfo.isResource = FieldInfoUtil.RequestValue();
            var oServiceRequest = new Camstar.WCF.Services.isManageMaterialQueue_Request();
            oServiceRequest.Info = serviceinfo;

            var resultStatus = new OM.ResultStatus();
            OM.ResultStatus oResultStatus = service.GetEnvironment(servicedata, oServiceRequest, out result);
            if (oResultStatus.IsSuccess)
            {
                if (result.Value.isResource != null)
                {
                    Resource.Data = result.Value.isResource.ToString();
                    Resource.Enabled = false;
                    Resource.DisplayMode = PERS.DisplayModeType.None;
                }
            }

            GetDetailsForMaterialQueue();
        }

        public void Resource_DataChanged(object sender, EventArgs e)
        {
            if (!IsDataChangedInitiator("RS"))
                return;

            InventoryLocation.ClearData();
            MaterialQueue.ClearData();

            UpdateColumnsForInventoryLocationOrResource();

            if (Resource.Data == null)
            {
                _gridServiceDetails.ClearData();
                return;
            }

            GetDetailsForInventoryLocationOrResource();
        }

        public void UpdateColumnsForInventoryLocationOrResource()
        {
            _gridServiceDetails.BoundContext.Fields["AcceptNoContainer"].Visible = true;
            _gridServiceDetails.BoundContext.Fields["isLot"].LabelName = "ExecuteRecipeTask_MaterialContainer";
        }

        public void UpdateColumnsForMaterialQueue()
        {
            _gridServiceDetails.BoundContext.Fields["AcceptNoContainer"].Visible = false;
            _gridServiceDetails.BoundContext.Fields["isUOM"].Visible = true;
            _gridServiceDetails.BoundContext.Fields["isLot"].LabelName = "Lbl_ContainerLot";
        }

        public void GetDetailsForInventoryLocationOrResource()
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            isMaterialRequestAcknowledgeService Svc = new isMaterialRequestAcknowledgeService(fs.CurrentUserProfile);
            OM.isMaterialRequestAcknowledge SvcData = new OM.isMaterialRequestAcknowledge();
            isMaterialRequestAcknowledge_Info SvcInfo = new isMaterialRequestAcknowledge_Info();
            isMaterialRequestAcknowledge_Request ReqData = new isMaterialRequestAcknowledge_Request();
            isMaterialRequestAcknowledge_Result ResData = new isMaterialRequestAcknowledge_Result();

            if (InventoryLocation.Data != null)
            {
                SvcData.isInventoryLocation = new NamedObjectRef();
                SvcData.isInventoryLocation.Name = InventoryLocation.Data.ToString();
            }
            else if (Resource.Data != null)
            {
                SvcData.Resource = new NamedObjectRef();
                SvcData.Resource.Name = Resource.Data.ToString();
            }

            SvcInfo.ServiceDetails = new isMaterialReqTxnSvcDetail_Info();
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

        public void GetDetailsForMaterialQueue()
        {
            FrameworkSession fs = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            isMaterialReqAcknowledge_v2Service Svc = new isMaterialReqAcknowledge_v2Service(fs.CurrentUserProfile);
            OM.isMaterialReqAcknowledge_v2 SvcData = new OM.isMaterialReqAcknowledge_v2();
            isMaterialReqAcknowledge_v2_Info SvcInfo = new isMaterialReqAcknowledge_v2_Info();
            isMaterialReqAcknowledge_v2_Request ReqData = new isMaterialReqAcknowledge_v2_Request();
            isMaterialReqAcknowledge_v2_Result ResData = new isMaterialReqAcknowledge_v2_Result();

            SvcData.isMaterialQueue = new NamedObjectRef();
            SvcData.isMaterialQueue.Name = MaterialQueue.Data.ToString();

            SvcInfo.ServiceDetails = new isMaterialReqTxnSvcDetail_Info();
            SvcInfo.ServiceDetails.RequestValue = true;
            ReqData.Info = SvcInfo;

            OM.ResultStatus Results = Svc.GetServiceDetails_V2(SvcData, ReqData, out ResData);
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


    }
}