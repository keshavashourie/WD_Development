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
/// Summary description for SS_WIPDisassociate
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_WIPDisassociate : MatrixWebPart
    {
        protected CWC.TextBox _txtContainer { get { return Page.FindCamstarControl("WIPDisassociate_Container") as CWC.TextBox; } }        
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("WIPDisassociate_ComputerName") as CWC.TextBox; } }        
        protected JQDataGrid _gridDetails { get { return Page.FindCamstarControl("WIPDisassociate_DisassociateDetails") as JQDataGrid; } }

        //---------------------------------------------------
        // Override OnLoad event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _txtContainer.DataChanged += _txtContainer_DataChanged;
               

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
        void _txtContainer_DataChanged(object sender, EventArgs e)
        {
            if (_txtContainer.Data != null)
                GetDisassociateDetails();
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
        public void GetDisassociateDetails()
        {
            // get the session and user profile
            var fs = FrameworkManagerUtil.GetFrameworkSession();

            // init service objects
            ss_WIPDisassociateService oService = new ss_WIPDisassociateService(fs.CurrentUserProfile);
            ss_WIPDisassociate oServiceData = new ss_WIPDisassociate();
            ss_WIPDisassociate_Info oServiceInfo = new ss_WIPDisassociate_Info();
            ss_WIPDisassociate_Result oServiceResult = new ss_WIPDisassociate_Result();

            oServiceData.Container = new ContainerRef(_txtContainer.Data.ToString());

            oServiceInfo.ss_ExistingAssociateDetails = new ss_WIPAssociateDetails_Info();
            oServiceInfo.ss_ExistingAssociateDetails.ss_ChildContainer = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_ExistingAssociateDetails.ss_Product = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_ExistingAssociateDetails.ss_Qty = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_ExistingAssociateDetails.ss_Spec = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_ExistingAssociateDetails.ss_XLocation = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_ExistingAssociateDetails.ss_YLocation = FieldInfoUtil.RequestValue();

            // init request
            ss_WIPDisassociate_Request oServiceRequest = new ss_WIPDisassociate_Request();
            oServiceRequest.Info = oServiceInfo;
            // execute!
            ResultStatus oResultStatus = oService.GetEnvironment(oServiceData, oServiceRequest, out oServiceResult);

            _gridDetails.Data = null;
            _gridDetails.OriginalData = null;

            if (oResultStatus.IsSuccess)
            {
                if (oServiceResult.Value != null)
                {
                    ss_WIPDisassociate oValue = oServiceResult.Value;

                    if (oValue.ss_ExistingAssociateDetails != null)
                    {
                        //Bind result to the grid
                        _gridDetails.Data = oValue.ss_ExistingAssociateDetails.ToArray();
                        _gridDetails.OriginalData = oValue.ss_ExistingAssociateDetails.ToArray();
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

            if (serviceData is ss_WIPDisassociate)
            {
                if (_txtContainer.Data != null)
                    (serviceData as ss_WIPDisassociate).Container = new ContainerRef(_txtContainer.Data.ToString());

                if ((_gridDetails.GridContext as BoundContext).SelectedRowIDs != null)
                {
                    List<ss_WIPAssociateDetails> oDetails = new List<ss_WIPAssociateDetails>();
                    foreach (ss_WIPAssociateDetails oItem in (_gridDetails.GridContext as BoundContext).GetSelectedItems(false))
                    {
                        ss_WIPAssociateDetails oAssociate = new ss_WIPAssociateDetails();
                        oAssociate.ss_ChildContainer = new ContainerRef(oItem.ss_ChildContainer.Name);
                        oAssociate.ss_XLocation = oItem.ss_XLocation;
                        oAssociate.ss_YLocation = oItem.ss_YLocation;
                        oDetails.Add(oAssociate);
                    }

                    //if (oDetails.Count > 0)
                        (serviceData as ss_WIPDisassociate).ss_DisassociateDetails = oDetails.ToArray();
                }
            }
        } //GetInputData
	}
}



