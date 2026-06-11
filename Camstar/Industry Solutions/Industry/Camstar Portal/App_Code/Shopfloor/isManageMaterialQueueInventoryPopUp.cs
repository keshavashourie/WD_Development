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

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class isManageMaterialQueueInventoryPopUp : MatrixWebPart
    {
        #region Properties
        protected virtual CWC.NamedObject isInventoryLocation
        {
            get
            {
                return Page.FindCamstarControl("isManageMaterialQueue_isInventoryLocation") as CWC.NamedObject;
            }
        }

         protected virtual CWC.TextBox isContainerOrLot
        {
            get
            {
                return Page.FindCamstarControl("isContainerOrLot") as CWC.TextBox;
            }
        }

         protected virtual CWC.RevisionedObject DummyProduct
         {
             get
             {
                 return Page.FindCamstarControl("DummyProduct") as CWC.RevisionedObject;
             }
         }

         protected virtual CWC.RevisionedObject Product
         {
             get
             {
                 return Page.FindCamstarControl("Product") as CWC.RevisionedObject;
             }
         }

         protected virtual CWC.TextBox Qty
         {
             get
             {
                 return Page.FindCamstarControl("QtyField") as CWC.TextBox;
             }
         }

         protected virtual CWC.NamedObject UOM
         {
             get
             {
                 return Page.FindCamstarControl("isManageMaterialQueue_isUOM") as CWC.NamedObject;
             }
         }
        
        protected virtual JQDataGrid _gridinvDetails
        {
            get 
            { 
                return Page.FindCamstarControl("isManageMaterialQueue_isInventoryDetails") as JQDataGrid; 
            }
        }
    

     
        #endregion


        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            isInventoryLocation.DataChanged += isInventoryLocation_DataChanged;
            _gridinvDetails.RowSelected += _gridinvDetails_RowSelected;
        }
    
        ResponseData _gridinvDetails_RowSelected(object sender, JQGridEventArgs args)
        {
            if (_gridinvDetails.GridContext.SelectedItem != null)
            {
                var ContainerLotName = (_gridinvDetails.GridContext.SelectedItem as OM.isInventoryDetails).isName;
                var qty = (_gridinvDetails.GridContext.SelectedItem as OM.isInventoryDetails).isQty;
                var product = (_gridinvDetails.GridContext.SelectedItem as OM.isInventoryDetails).isProduct;


                if (_gridinvDetails.GridContext.SelectedItem != null && ContainerLotName != null)
                {
                    isContainerOrLot.Data = (_gridinvDetails.GridContext.SelectedItem as OM.isInventoryDetails).isName;
                }

                if (_gridinvDetails.GridContext.SelectedItem != null && qty != null)
                {
                    Qty.Data = (_gridinvDetails.GridContext.SelectedItem as OM.isInventoryDetails).isQty;
                }

                if (_gridinvDetails.GridContext.SelectedItem != null && product != null)
                {
                    Product.Data = (_gridinvDetails.GridContext.SelectedItem as OM.isInventoryDetails).isProduct;
                }
				//	Uncomment the line below to have the popup cloe immediately upon selecting a row
                //Page.CloseFloatingFrameOnSubmit(new OM.ResultStatus());
            } 
            return null;
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as PERS.CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "OkBtn":
                        {
                            Page.CloseFloatingFrameOnSubmit(new OM.ResultStatus()); 
                            break;
                        }
                }
            }
        }

        void isInventoryLocation_DataChanged(object sender, EventArgs e)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.isManageMaterialQueueService(session.CurrentUserProfile);
            var servicedata = new OM.isManageMaterialQueue();
            var serviceinfo = new OM.isManageMaterialQueue_Info();
            var result = new Camstar.WCF.Services.isManageMaterialQueue_Result();

            if (DummyProduct.Data != null)
                servicedata.isMaterialListProduct = DummyProduct.Data as OM.RevisionedObjectRef;
            
            if (isInventoryLocation.Data != null)
                servicedata.isInventoryLocation = isInventoryLocation.Data as OM.NamedObjectRef;

            serviceinfo.isInventoryDetails = new OM.isInventoryDetails_Info()
             {
                 isContainer = new OM.Info(true),
                 isLot = new OM.Info(true),
                 isName = new OM.Info(true),
                 isProduct = new OM.Info(true),
                 isQty = new OM.Info(true),
                 isUOM = new OM.Info(true),
                 isExpirationDate = new OM.Info(true),
                 isRemovalStrategy = new OM.Info(true)
            };

            var oServiceRequest = new Camstar.WCF.Services.isManageMaterialQueue_Request();
            oServiceRequest.Info = serviceinfo;

            var resultStatus = new OM.ResultStatus();

            OM.ResultStatus oResultStatus = service.GetEnvironment(servicedata, oServiceRequest, out result);

            if (oResultStatus.IsSuccess)
            {
                _gridinvDetails.ClearData();
                if (result.Value.isInventoryDetails != null)
                {

                    Array oInventoryLocationArray = result.Value.isInventoryDetails.ToArray();


                    (_gridinvDetails.GridContext as BoundContext).Data = oInventoryLocationArray;

                    _gridinvDetails.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(_gridinvDetails);


                }
                else
                {
                    _gridinvDetails.ClearData();
                }
            }


        }
        }
}
