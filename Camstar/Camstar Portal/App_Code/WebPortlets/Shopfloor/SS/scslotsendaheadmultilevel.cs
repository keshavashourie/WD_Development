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
/// Summary description for scslotsendaheadmultilevel
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class scsLotSendAheadMultiLevel: scsShopfloorBase
    {
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("ss_LotSendAheadMultiLevel_SelectionId") as CWC.TextBox; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("ss_LotSendAheadMultiLevel_ComputerName") as CWC.TextBox; } }
        protected CWC.TextBox _txtNewContainerName { get { return Page.FindCamstarControl("ss_LotSendAheadMultiLevel_ToContainerName") as CWC.TextBox; } }
        protected CWC.TextBox _txtServiceName { get { return Page.FindCamstarControl("ss_LotSendAheadMultiLevel_ServiceName") as CWC.TextBox; } }
        protected JQDataGrid _gridContainer { get { return Page.FindCamstarControl("ss_LotSendAheadMultiLevel_ContainerGrid") as JQDataGrid; } }
        protected JQDataGrid _gridChildContainersList { get { return Page.FindCamstarControl("scsLotSendAheadMultiLevel_ChildContainersList") as JQDataGrid; } }
        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("scsLotSendAheadMultiLevel_Employee") as CWC.NamedObject; } }

        protected SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("ss_LotSendAhead_DataEnvelop") as SEMI.AppCode.DataEnvelopControl; } }
        
        //---------------------------------------------------
        //
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _txtSelectionId.DataChanged += new EventHandler(_txtSelectionId_DataChanged);
            _txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
            _txtServiceName.Data = Page.PrimaryServiceType.ToString();

            if (Page.IsPostBack)        
                // Check if it is a pop up close, get the return result
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {
                    if (_envSelectedLots != null)
                        // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                        if (Page.DataContract.GetValueByName("ss_LotSendAhead_DataEnvelopDM") != null)
                            _envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("ss_LotSendAhead_DataEnvelopDM") as string[];

                    if (_envSelectedLots.SS_ContainersList != null)
                    {
                        string[] sContainers;
                        sContainers = _envSelectedLots.SS_ContainersList;

                        if (sContainers.Length > 0)
                        {
                            foreach (string sContainer in sContainers)
                            {
                                // check if lot not exist in the grid else add to the grid
                                string strSelectedGridId = (_gridContainer.GridContext as BoundContext).GetRowIdByCellValue("Lot", sContainer);

                                if (string.IsNullOrEmpty(strSelectedGridId))
                                {
                                    _txtSelectionId.TextControl.Text = sContainer;
                                    _txtSelectionId_DataChanged(null, null);                                    
                                }
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

            _txtSelectionId.Focus();
        } // _txtSelectionId_DataChanged
        

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void FetchData(string sEventType, string sContainerName = "")
        {
            // Prepare service
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            ResultStatus oServiceResult = new ResultStatus(null, false);
            string sServiceType = "scsLotSendAheadMultiLevel";
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
            var oServiceInfo = info as scsLotSendAheadMultiLevel_Info;
            (oRequest as Request).Info = oServiceInfo;
            
            bool bExecuteResolveSelectionId = false;

            if (sContainerName == "")
            {
                bExecuteResolveSelectionId = true;
                (oServiceData as OM.scsLotSendAheadMultiLevel).SelectionId = _txtSelectionId.Data.ToString();
                oServiceInfo.Containers = FieldInfoUtil.RequestValue();
            }
            else
            {
                (oServiceData as OM.scsLotSendAheadMultiLevel).Container = new ContainerRef(sContainerName);
            }

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
                string sResolvedContainer = (oResult.Value as OM.scsLotSendAheadMultiLevel).Container.Name;
                // check if the input container already exists in the grid
                bool bNonExistingContainer = true;

                if ((_gridContainer.GridContext as BoundContext).GetTotalRows() > 0)
                {
                    string strSelectedGridId = (_gridContainer.GridContext as BoundContext).GetRowIdByCellValue("Lot", sResolvedContainer);
                    if (string.IsNullOrEmpty(strSelectedGridId))
                        bNonExistingContainer = true;
                    else
                        bNonExistingContainer = false;
                }

                if (sEventType == "SelectionId")
                {
                    if (bNonExistingContainer)
                    {                        
                        JQDataGrid _gridContainerTemp = Page.FindCamstarControl("ss_LotSendAheadMultiLevel_ContainerGrid") as JQDataGrid;
                        SEMI.AppCode.UIUtility.GetLotQuerySelection(this, sServiceType, sResolvedContainer, true, ref _gridContainerTemp, _gridContainerTemp.ID.ToString(), true);
                        getChildContainers(sResolvedContainer);
                    }                                                                                     

                    if (sContainerName == "")
                    {
                        // loop through the return containers and recursively call FetchData
                        foreach (ContainerRef oContainer in (oResult.Value as OM.scsLotSendAheadMultiLevel).Containers) 
                            if (sResolvedContainer != oContainer.Name)
                                FetchData("SelectionId", oContainer.Name);                        
                    }                    
                }           
            }
            else
                DisplayMessage(oResultStatus);         
        } // FetchData

        //---------------------------------------------------
        //
        //---------------------------------------------------
        private void getChildContainers(String strParentContainerName)
        {
            // Prepare service
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            ResultStatus oServiceResult = new ResultStatus(null, false);
            string sServiceType = "scsLotSendAheadMultiLevel";
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
            var oServiceInfo = info as scsLotSendAheadMultiLevel_Info;
            (oRequest as Request).Info = oServiceInfo;
           
            (oServiceData as OM.scsLotSendAheadMultiLevel).Container = new ContainerRef(strParentContainerName);
            oServiceInfo.ChildContainersList = new Info(true);
            
            // init the result object
            Result oResult = new Result();
            ResultStatus oResultStatus = new ResultStatus();

            // execute to request the value
            oResultStatus = (oService as IShopFloorBase).GetEnvironment(oServiceData, (oRequest as Request), out oResult);

            if (oResultStatus.IsSuccess)
            {
                if ((oResult.Value as OM.scsLotSendAheadMultiLevel).ChildContainersList != null && (oResult.Value as OM.scsLotSendAheadMultiLevel).ChildContainersList.Length > 0)
                {
                    var ChildContainersList = (oResult.Value as OM.scsLotSendAheadMultiLevel).ChildContainersList;
                    JQDataGrid _gridChildContainersListTemp =Page.FindCamstarControl("scsLotSendAheadMultiLevel_ChildContainersList") as JQDataGrid; 
                    foreach(var childlot in ChildContainersList)
                        SEMI.AppCode.UIUtility.GetLotQuerySelection(this, sServiceType, childlot.Name, false, ref _gridChildContainersListTemp, _gridChildContainersListTemp.ID.ToString(), true);

                }
            }
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public void settocontainername()
        {
           _txtNewContainerName.Data= _gridChildContainersList.GridContext.GetCell(_gridChildContainersList.SelectedRowID, "Lot").ToString();
        }
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
            else if (action != null && action.Parameters == "Submit")
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
            _txtNewContainerName.ClearData();
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
            string sServiceType = "scsLotSendAheadMultiLevel";
            sServiceType = Page.PrimaryServiceType.ToString();

            // Run proper constructor. We need to be dynamic with the primary service type
            var svcType = WCFObject.CreateObjectType(sServiceType + "Service");

            // create a request object
            var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
            var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
            var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

            // retriving data dynamically for Flexibility of the page.
            var oServiceData = CreateServiceData(sServiceType);

            (oServiceData as OM.scsLotSendAheadMultiLevel).Employee = _ndoEmployee.Data != null ? new NamedObjectRef(_ndoEmployee.Data.ToString()) : null;

            if (_txtSelectionId.Data!=null)
            {
                (oServiceData as OM.scsLotSendAheadMultiLevel).Container = new ContainerRef(_gridContainer.GridContext.GetCell(0, "Lot").ToString());
            }

            if (_txtNewContainerName.Data != null)
            {              
                (oServiceData as OM.scsLotSendAheadMultiLevel).TargetSendAheadLot = new ContainerRef(_txtNewContainerName.Data.ToString());
            }
             // init the result object
            Result oResult = new Result();
            ResultStatus oResultStatus = new ResultStatus();          
            oResultStatus = (oService as IShopFloorBase).ExecuteTransaction((oServiceData as DCObject));
            return oResultStatus;
            
        } // CustomSubmit




    }
}



