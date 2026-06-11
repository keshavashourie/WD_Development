/* Copyright 2019 Siemens */
using System;
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
using SEMI.AppCode;

/// <summary>
/// Summary description for SS_WIPAssociate
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_WIPAssociate : MatrixWebPart
    {
        protected CWC.TextBox _txtContainer { get { return Page.FindCamstarControl("WIPAssociate_Container") as CWC.TextBox; } }
        protected CWC.TextBox _txtChildContainer { get { return Page.FindCamstarControl("WIPAssociate_ChildContainer") as CWC.TextBox; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("WIPAssociate_ComputerName") as CWC.TextBox; } }
        protected JQDataGrid _gridExisting { get { return Page.FindCamstarControl("WIPAssociate_ExistingChildContainers") as JQDataGrid; } }
        protected JQDataGrid _gridDetails { get { return Page.FindCamstarControl("WIPAssociate_AssociateDetails") as JQDataGrid; } }

        //---------------------------------------------------
        // Override OnLoad event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _txtContainer.DataChanged += _txtContainer_DataChanged;
                _txtChildContainer.DataChanged += _txtChildContainer_DataChanged;

                if (!Page.IsPostBack)
                {
                    _txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);                    
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _txtChildContainer_DataChanged(object sender, EventArgs e)
        {
            if (_txtChildContainer.Data != null)
                GetTargetAssocParent();
        } // _txtChildContainer_DataChanged

        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _txtContainer_DataChanged(object sender, EventArgs e)
        {
            if (_txtContainer.Data != null)
            GetAssociateDetails();
        } // _txtContainer_DataChanged

        //---------------------------------------------------
        // Web part custom action
        //---------------------------------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "Reset":
                        {
                            Page.ShopfloorReset(sender, e);                          
                            break;
                        }
                }
            }
        } // WebPartCustomAction(object sender, CustomActionEventArgs e)

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void GetTargetAssocParent()
        {
            // get the session and user profile
            var fs = FrameworkManagerUtil.GetFrameworkSession();

            // init service objects
            ss_WIPAssociateService oService = new ss_WIPAssociateService(fs.CurrentUserProfile);
            ss_WIPAssociate oServiceData = new ss_WIPAssociate();
            ss_WIPAssociate_Info oServiceInfo = new ss_WIPAssociate_Info();
            ss_WIPAssociate_Result oServiceResult = new ss_WIPAssociate_Result();

            oServiceData.ChildContainer = new ContainerRef(_txtChildContainer.Data.ToString());
            oServiceInfo.ss_TargetAssocParent = FieldInfoUtil.RequestValue();

             // init request
            ss_WIPAssociate_Request oServiceRequest = new ss_WIPAssociate_Request();
            oServiceRequest.Info = oServiceInfo;
            // execute!
            ResultStatus oResultStatus = oService.GetEnvironment(oServiceData, oServiceRequest, out oServiceResult);

            _txtContainer.ClearData();
            _gridDetails.ClearData();
            _gridDetails.Data = null;
            _gridDetails.OriginalData = null;
            _gridExisting.ClearData();
            _gridExisting.Data = null;
            _gridExisting.OriginalData = null;

            if (oResultStatus.IsSuccess)
                if (oServiceResult.Value != null)
                    if (oServiceResult.Value.ss_TargetAssocParent != null)
                        _txtContainer.Data = oServiceResult.Value.ss_TargetAssocParent;                              
        } // GetTargetAssocParent

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void GetAssociateDetails()
        {
            // get the session and user profile
            var fs = FrameworkManagerUtil.GetFrameworkSession();

            // init service objects
            ss_WIPAssociateService oService = new ss_WIPAssociateService(fs.CurrentUserProfile);
            ss_WIPAssociate oServiceData = new ss_WIPAssociate();
            ss_WIPAssociate_Info oServiceInfo = new ss_WIPAssociate_Info();
            ss_WIPAssociate_Result oServiceResult = new ss_WIPAssociate_Result();

            oServiceData.Container = new ContainerRef(_txtContainer.Data.ToString());

            oServiceInfo.ss_ExistingChildContainers = new ss_WIPAssociateDetails_Info();
            oServiceInfo.ss_ExistingChildContainers.ss_ChildContainer = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_ExistingChildContainers.ss_Product = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_ExistingChildContainers.ss_Qty  = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_ExistingChildContainers.ss_XLocation = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_ExistingChildContainers.ss_YLocation = FieldInfoUtil.RequestValue();

            oServiceInfo.ss_ExpectedChildContainers = new ss_WIPAssociateDetails_Info();
            oServiceInfo.ss_ExpectedChildContainers.ss_ChildContainer = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_ExpectedChildContainers.ss_Spec = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_ExpectedChildContainers.ss_Qty = FieldInfoUtil.RequestValue();        

            // init request
            ss_WIPAssociate_Request oServiceRequest = new ss_WIPAssociate_Request();
            oServiceRequest.Info = oServiceInfo;
            // execute!
            ResultStatus oResultStatus = oService.GetEnvironment(oServiceData, oServiceRequest, out oServiceResult);

            _gridDetails.Data = null;
            _gridDetails.OriginalData = null;
            _gridExisting.Data = null;
            _gridExisting.OriginalData = null;

            if (oResultStatus.IsSuccess)
            {
               if (oServiceResult.Value != null)
               {
                   ss_WIPAssociate oValue = oServiceResult.Value;

                    if (oValue.ss_ExistingChildContainers != null)
                    {
                        //Bind result to the grid
                        _gridExisting.Data = oValue.ss_ExistingChildContainers.ToArray();
                        _gridExisting.OriginalData = oValue.ss_ExistingChildContainers.ToArray();
                    }

                    if (oValue.ss_ExpectedChildContainers != null)
                    {
                        //Bind result to the grid
                        _gridDetails.Data = oValue.ss_ExpectedChildContainers.ToArray();
                        _gridDetails.OriginalData = oValue.ss_ExpectedChildContainers.ToArray();
                    }
                }
            }

        } //GetAssociateDetails

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            if (serviceData is ss_WIPAssociate)
            {
                if (_txtContainer.Data != null)
                    (serviceData as ss_WIPAssociate).Container = new ContainerRef(_txtContainer.Data.ToString());

                if ((_gridDetails.GridContext as BoundContext).SelectedRowIDs != null)
                {
                    List<ss_WIPAssociateDetails> oDetails = new List<ss_WIPAssociateDetails>();
                    foreach (ss_WIPAssociateDetails oItem in (_gridDetails.GridContext as BoundContext).GetSelectedItems(false))
                    {
                        ss_WIPAssociateDetails oAssociate = new ss_WIPAssociateDetails();
                        oAssociate.ss_ChildContainer = new ContainerRef(oItem.ss_ChildContainer.Name);
                        oAssociate.ss_XLocation = oItem.ss_XLocation;
                        oAssociate.ss_YLocation = oItem.ss_YLocation;
                        if ((oItem.ss_XLocation == null) || (oItem.ss_YLocation == null))
                            oAssociate.ss_IsBlankXY = true;
                        else
                            oAssociate.ss_IsBlankXY = false;

                        oDetails.Add(oAssociate);
                    }
                    
                    (serviceData as ss_WIPAssociate).ss_AssociateDetails = oDetails.ToArray();
                }
            } // GetInputData
        }
    }
}



