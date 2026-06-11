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
/// Summary description for SS_MultiLevelStart
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_MultiLevelStart : MatrixWebPart
    {
        protected CWC.TextBox _txtContainerName { get { return Page.FindCamstarControl("LotStart_ContainerName") as CWC.TextBox; } }
        protected CWC.RevisionedObject _rdoProduct { get { return Page.FindCamstarControl("LotStart_Product") as CWC.RevisionedObject; } }
        protected CWC.NamedObject _ndoStartReason { get { return Page.FindCamstarControl("LotStart_StartReason") as CWC.NamedObject; } }
        protected CWC.RevisionedObject _rdoWorkflow { get { return Page.FindCamstarControl("LotStart_Workflow") as CWC.RevisionedObject; } }
        protected CWC.NamedSubentity _ndsWorkflowStep { get { return Page.FindCamstarControl("LotStart_WorkflowStep") as CWC.NamedSubentity; } }
        
        protected CWC.NamedObject _ndoOwner { get { return Page.FindCamstarControl("LotStart_Owner") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoVendor { get { return Page.FindCamstarControl("LotStart_Vendor") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoFactory { get { return Page.FindCamstarControl("LotStart_Factory") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoLevel { get { return Page.FindCamstarControl("LotStart_Level") as CWC.NamedObject; } }

        protected CWC.TextBox _txtQty2 { get { return Page.FindCamstarControl("LotStart_Qty2") as CWC.TextBox; } }
        protected CWC.TextBox _txtQty { get { return Page.FindCamstarControl("LotStart_Qty") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoUOM { get { return Page.FindCamstarControl("LotStart_UOM") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoShipFromFactory { get { return Page.FindCamstarControl("LotStart_ShipFromFactory") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoShipToProcess { get { return Page.FindCamstarControl("LotStart_ShipToProcess") as CWC.NamedObject; } }
        protected CWC.TextBox _txtAssociateParentContainerName { get { return Page.FindCamstarControl("ss_AssociateParentContainerName") as CWC.TextBox; } }        
        
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("LotStart_ComputerName") as CWC.TextBox; } }

        protected JQDataGrid _gridExistingChildren { get { return Page.FindCamstarControl("ExistingChildContainers") as JQDataGrid; } }
        protected JQDataGrid _gridAssociateChildren { get { return Page.FindCamstarControl("AssociateChildContainers") as JQDataGrid; } }

        protected CWC.CheckBox _chkMainLotExists { get { return Page.FindCamstarControl("ss_MultiLevelStart_ss_MainContainerExists") as CWC.CheckBox; } }

        //---------------------------------------------------
        // Override OnLoad event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _txtContainerName.DataChanged += _txtContainerName_DataChanged;

                if (!Page.IsPostBack)
                {
                    _txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);                    
                    assignFactory();
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
        void _txtContainerName_DataChanged(object sender, EventArgs e)
        {
            if (_txtContainerName.Data != null)
            VerifyContainer(_txtContainerName.Data.ToString());
        } // _txtContainerName_DataChanged

        //---------------------------------------------------
        // Get Factory from session data contract
        //---------------------------------------------------
        protected void assignFactory()
        {
            if (Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Factory) != null)
                _ndoFactory.Data = Page.SessionDataContract.GetValueByName(Camstar.WebPortal.Constants.DataMemberConstants.Factory).ToString();
        } // assignFactory

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
                            ResetControlStates();
                            assignFactory();
                            break;
                        }
                }
            }
        } // WebPartCustomAction(object sender, CustomActionEventArgs e)

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void VerifyContainer(string sContainer)
        {
            // get the session and user profile
            var fs = FrameworkManagerUtil.GetFrameworkSession();

            // init service objects
            ss_MultiLevelStartService oService = new ss_MultiLevelStartService(fs.CurrentUserProfile);
            ss_MultiLevelStart oServiceData = new ss_MultiLevelStart();
            ss_MultiLevelStart_Info oServiceInfo = new ss_MultiLevelStart_Info();
            ss_MultiLevelStart_Result oServiceResult = new ss_MultiLevelStart_Result();

            oServiceData.ContainerName = sContainer;

            oServiceInfo.ss_MainContainerExists =  FieldInfoUtil.RequestValue();
            oServiceInfo.ss_MainContainer = FieldInfoUtil.RequestValue();            

            oServiceInfo.ss_ExistingChildContainers = new ss_MultiLevelStartChildDetails_Info();
            oServiceInfo.ss_ExistingChildContainers.ss_ContainerName = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_ExistingChildContainers.ss_Product = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_ExistingChildContainers.ss_XLocation = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_ExistingChildContainers.ss_YLocation = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_ExistingChildContainers.ss_Qty = FieldInfoUtil.RequestValue();

            oServiceInfo.ss_MainContainerDetails = new ss_MultiLevelStartDetails_Info();
            oServiceInfo.ss_MainContainerDetails.ss_AssociateParentContainerName = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_MainContainerDetails.ss_Product = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_MainContainerDetails.ss_StartReason = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_MainContainerDetails.ss_Workflow = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_MainContainerDetails.ss_WorkflowStep = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_MainContainerDetails.ss_Owner = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_MainContainerDetails.ss_Vendor = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_MainContainerDetails.ss_Factory = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_MainContainerDetails.ss_ContainerLevel = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_MainContainerDetails.ss_Qty = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_MainContainerDetails.ss_UOM = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_MainContainerDetails.ss_ShipFromFactory = FieldInfoUtil.RequestValue();
            oServiceInfo.ss_MainContainerDetails.ss_ShipToProcess = FieldInfoUtil.RequestValue();

            // init request
            ss_MultiLevelStart_Request oServiceRequest = new ss_MultiLevelStart_Request();
            oServiceRequest.Info = oServiceInfo;
            // execute!
            ResultStatus oResultStatus = oService.VerifyContainer(oServiceData, oServiceRequest, out oServiceResult);

            if (oResultStatus.IsSuccess)
            {
                if (oServiceResult.Value.ss_MainContainerExists == true)
                {
                    // set the controls with the existing container's details                    
                    ss_MultiLevelStart oValue = oServiceResult.Value;

                    _chkMainLotExists.CheckControl.Checked = bool.Parse(oValue.ss_MainContainerExists.ToString());

                    if (oValue.ss_MainContainerDetails != null)
                    {
                        _rdoProduct.Data = oValue.ss_MainContainerDetails.ss_Product;
                        _ndoStartReason.Data = oValue.ss_MainContainerDetails.ss_StartReason;
                        _rdoWorkflow.Data = oValue.ss_MainContainerDetails.ss_Workflow;
                        _ndsWorkflowStep.Data = oValue.ss_MainContainerDetails.ss_WorkflowStep;
                        _ndoOwner.Data = oValue.ss_MainContainerDetails.ss_Owner;
                        _ndoVendor.Data = oValue.ss_MainContainerDetails.ss_Vendor;
                        _ndoFactory.Data = oValue.ss_MainContainerDetails.ss_Factory;
                        _ndoLevel.Data = oValue.ss_MainContainerDetails.ss_ContainerLevel;
                        _txtQty.Data = oValue.ss_MainContainerDetails.ss_Qty;
                        _ndoUOM.Data = oValue.ss_MainContainerDetails.ss_UOM;
                        _ndoShipFromFactory.Data = oValue.ss_MainContainerDetails.ss_ShipFromFactory;
                        _ndoShipToProcess.Data = oValue.ss_MainContainerDetails.ss_ShipToProcess;

                        SetControlStates(false);

                        _txtAssociateParentContainerName.Data = oValue.ss_MainContainerDetails.ss_AssociateParentContainerName;
                        if (_txtAssociateParentContainerName.Data == null)
                            _txtAssociateParentContainerName.Enabled = true;
                    }

                    if (oValue.ss_ExistingChildContainers != null)
                    {
                        //Bind result to the grid
                        _gridExistingChildren.Data = oValue.ss_ExistingChildContainers.ToArray();
                        _gridExistingChildren.OriginalData = oValue.ss_ExistingChildContainers.ToArray();
                    }
                }
            }
        } // VerifyContainer

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void ResetControlStates()
        {
            SetControlStates(true);
        } //ResetControlStates

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            if (_gridAssociateChildren.Data != null)
            {
                ss_MultiLevelStartChildDetails[] AssociateChildren = _gridAssociateChildren.Data as ss_MultiLevelStartChildDetails[];
                foreach (ss_MultiLevelStartChildDetails oItem in AssociateChildren)
                {
                    if ((oItem.ss_XLocation == null) || (oItem.ss_YLocation == null))
                        oItem.ss_IsBlankXY = true;
                    else
                        oItem.ss_IsBlankXY = false;
                }

                if (serviceData is ss_MultiLevelStart)                
                    (serviceData as ss_MultiLevelStart).ss_AssociateChildContainers = AssociateChildren;
            }
        } // GetInputData

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void SetControlStates(bool bEnabled)
        {
            _txtContainerName.Enabled = bEnabled;
            _rdoProduct.Enabled = bEnabled;
            _rdoProduct.RevisionControl.Enabled = bEnabled;
            _ndoStartReason.Enabled = bEnabled;
            _rdoWorkflow.Enabled = bEnabled;
            _rdoWorkflow.RevisionControl.Enabled = bEnabled;
            _ndsWorkflowStep.Enabled = bEnabled;
            _ndoOwner.Enabled = bEnabled;
            _ndoVendor.Enabled = bEnabled;
            _ndoFactory.Enabled = bEnabled;
            _ndoLevel.Enabled = bEnabled;
            _txtQty.Enabled = bEnabled;
            _ndoUOM.Enabled = bEnabled;
            _ndoShipFromFactory.Enabled = bEnabled;
            _ndoShipToProcess.Enabled = bEnabled;
            _txtAssociateParentContainerName.Enabled = bEnabled;
        } // SetControlStates

        //---------------------------------------------------
        // Override Post Execute Event
        //---------------------------------------------------
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                Page.ShopfloorReset(null, null);
                ResetControlStates();
                assignFactory();
            }
        }
    }
}



