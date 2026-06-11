/* Copyright 2019 Siemens */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;

/// <summary>
/// Summary description for ss_ContainerFeeder
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_ContainerFeeder: scsShopfloorBase
    {        
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("ContainerFeeder_SelectionId") as CWC.TextBox; } }        
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("ContainerFeeder_ComputerName") as CWC.TextBox; } }
		protected CWC.TextBox _txtComments { get { return Page.FindCamstarControl("Shopfloor_Comments") as CWC.TextBox; } } 

		protected CWC.NamedObject _ndoCurrentFeeder { get { return Page.FindCamstarControl("ss_ContainerFeeder_ss_CurrentFeeder") as CWC.NamedObject; } }
		protected CWC.NamedObject _ndoNewFeeder { get { return Page.FindCamstarControl("ss_ContainerFeeder_ss_NewFeeder") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("ContainerFeeder_Employee") as CWC.NamedObject; } }

		protected JQDataGrid _gridContainer { get { return Page.FindCamstarControl("ContainerFeeder_ContainerGrid") as JQDataGrid; } }

		protected const int _kContainersPerBatch = 20;

		protected SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("ContainerFeeder_DataEnvelop") as SEMI.AppCode.DataEnvelopControl; } }
                
        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _txtSelectionId.DataChanged += new EventHandler(_txtSelectionId_DataChanged);
            _txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
            
            if (!Page.IsPostBack)
            {
                //get containers from Container Search screen
                if (Page.Session["selectedContainers"] != null)
                {
                    string[] sContainersList = Page.Session["selectedContainers"] as string[];
                    foreach (string container in sContainersList)
                    {
                        _txtSelectionId.TextControl.Text = container;
                        _txtSelectionId_DataChanged(null, null);
                    }
                }
                Page.Session.Remove("selectedContainers");
            }

            if (Page.IsPostBack)        
                // Check if it is a pop up close, get the return result
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {
                    if (_envSelectedLots != null)
                        // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
						if (Page.DataContract.GetValueByName("ContainerFeeder_DataEnvelopDM") != null)
							_envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("ContainerFeeder_DataEnvelopDM") as string[];

                    if (_envSelectedLots.SS_ContainersList != null)
                    {
                        string[] sContainers;
                        sContainers = _envSelectedLots.SS_ContainersList;

                        if (sContainers.Length > 0)
                        {
                                // check if lot not exist in the grid else add to the grid
							string strSelectedGridId = (_gridContainer.GridContext as BoundContext).GetRowIdByCellValue("Lot", sContainers[0]);

                                if (string.IsNullOrEmpty(strSelectedGridId))
                                {
									_txtSelectionId.TextControl.Text = sContainers[0];
                                    _txtSelectionId_DataChanged(null, null);
                                   // FetchData("SelectionId");
                            }
                        }
                        //nullify the containers list
                        _envSelectedLots.SS_ContainersList = null;
                    }
                }
        } // OnLoad

        
      
       
        //---------------------------------------------------
        //
        //---------------------------------------------------
        void _txtSelectionId_DataChanged(object sender, EventArgs e)
        {
			if (_txtSelectionId.Data != null)
				if (_txtSelectionId.Data.ToString() != "")
					FetchData("SelectionId");            
        } // _txtSelectionId_DataChanged

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void FetchData(string sEventType, string sContainerName = "")
        {
            // Prepare service
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            ResultStatus oServiceResult = new ResultStatus(null, false);
			string sServiceType = "ss_ContainerFeeder";
            sServiceType = Page.PrimaryServiceType.ToString();
            
            // Run proper constructor. We need to be dynamic with the primary service type
            var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
            //create a request object
            var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
            var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
            var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

            //retriving data dynamically for Flexibility of the page.
            var oServiceData = CreateServiceData(sServiceType);

            var info = CreateServiceInfo(sServiceType);
			var oServiceInfo = info as ss_ContainerFeeder_Info;
            (oRequest as Request).Info = oServiceInfo;
            
            bool bExecuteResolveSelectionId = false;
            
                    if (sContainerName == "")
                    {
                        bExecuteResolveSelectionId = true;
						(oServiceData as ss_ContainerFeeder).SelectionId = _txtSelectionId.Data.ToString();
                        oServiceInfo.Containers = FieldInfoUtil.RequestValue();
                    }
                    else
                    {
						(oServiceData as ss_ContainerFeeder).Container = new ContainerRef(sContainerName);
                    }
					oServiceInfo.ss_CurrentFeeder = FieldInfoUtil.RequestValue();
                    
            
            oServiceInfo.Container = FieldInfoUtil.RequestValue();

            // init the result object
            Result oResult = new Result();
            ResultStatus oResultStatus = new ResultStatus();

            // execute to request the value or resolveSelectionId
            if (bExecuteResolveSelectionId)
                oResultStatus = (oService as IShopFloorBase).ResolveSelectionId((oServiceData as DCObject), (oRequest as Request), out oResult);
            else
                oResultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oResult);

            if (oResultStatus.IsSuccess)
            {
				string sResolvedContainer = (oResult.Value as ss_ContainerFeeder).Container.Name;
                
				if ((oResult.Value as ss_ContainerFeeder).ss_CurrentFeeder != null)
					_ndoCurrentFeeder.Data = (oResult.Value as ss_ContainerFeeder).ss_CurrentFeeder;
				_gridContainer.ClearData();
                JQDataGrid _gridContainerTemp = Page.FindCamstarControl("ContainerFeeder_ContainerGrid") as JQDataGrid;
                SEMI.AppCode.UIUtility.GetLotQuerySelection(this, sServiceType, sResolvedContainer, false, ref _gridContainerTemp, _gridContainerTemp.ID.ToString(), true);
                    
                
            }
            else
                DisplayMessage(oResultStatus);         
        } // FetchData

      
        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Reset")
            {
                CustomReset();
                Page.ShopfloorReset(sender, e);
            }
            else if (action != null && action.Parameters == "CustomSubmit")
            {                
                e.Result = CustomSubmit();
                if (e.Result.IsSuccess)
                    CustomReset();
            }
        } // WebPartCustomAction 

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void CustomReset()
        {
            Page.ClearValues();
            _gridContainer.ClearData();
			_ndoCurrentFeeder.ClearData();
			_ndoNewFeeder.ClearData();
            _txtSelectionId.Focus();
        } // CustomReset
      
        //---------------------------------------------------
        //
        //---------------------------------------------------
        public ResultStatus CustomSubmit()
        {
            // Prepare service
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            ResultStatus oServiceResult = new ResultStatus(null, false);
			string sServiceType = "ss_ContainerFeeder";
            sServiceType = Page.PrimaryServiceType.ToString();

            // Run proper constructor. We need to be dynamic with the primary service type
            var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
            // create a request object
            var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
            var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
            var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

            // retriving data dynamically for Flexibility of the page.
            var oServiceData = CreateServiceData(sServiceType);
			(oServiceData as ss_ContainerFeeder).ComputerName = _txtComputerName.Data != null ? _txtComputerName.Data.ToString() : null;
			(oServiceData as ss_ContainerFeeder).Employee = _ndoEmployee.Data != null ? new NamedObjectRef(_ndoEmployee.Data.ToString()) : null;
			if (_txtSelectionId.Data != null)
				(oServiceData as ss_ContainerFeeder).Container = new ContainerRef(_txtSelectionId.Data.ToString());
			(oServiceData as ss_ContainerFeeder).ss_NewFeeder = _ndoNewFeeder.Data != null ? new NamedObjectRef(_ndoNewFeeder.Data.ToString()):null;
			(oServiceData as ss_ContainerFeeder).Comments = _txtComments.Data != null ? _txtComments.Data.ToString() : null;
             // init the result object
            Result oResult = new Result();
            ResultStatus oResultStatus = new ResultStatus();          
            oResultStatus = (oService as IShopFloorBase).ExecuteTransaction((oServiceData as DCObject));
            return oResultStatus;
            
        } // CustomSubmit

    }
}



