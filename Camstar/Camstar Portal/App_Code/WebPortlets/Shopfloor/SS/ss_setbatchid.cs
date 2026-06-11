/* Copyright 2019 Siemens */
using System;
using System.Data;
using System.Web;
using System.Linq;
using System.Collections.Generic;

using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;

using SEMI.AppCode;
using System.Web.UI.WebControls;

/// <summary>
/// Summary description for SetBatchId
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_SetBatchId : scsShopfloorBase  
    {
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("SetBatchId_SelectionId") as CWC.TextBox; } }
        protected CWC.TextBox _txtEmployee { get { return Page.FindCamstarControl("SetBatchId_Employee") as CWC.TextBox; } }
        protected CWC.TextBox _txtBatchId { get { return Page.FindCamstarControl("SetBatchId_BatchId") as CWC.TextBox; } }
        protected JQDataGrid _gridContainers { get { return Page.FindCamstarControl("ContainersField") as JQDataGrid; } }
        protected CWC.TextBox txtComputerName { get { return Page.FindCamstarControl("ComputerNameField") as CWC.TextBox; } }
        private SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }
        
        public SS_SetBatchId()
	    {
		    //
		    // TODO: Add constructor logic here
		    //
	    }
        public void ContainersGrid_RowSelected(Object sender, EventArgs e)
        {
            try
            {
                JQDataGrid _gridContainers = Page.FindCamstarControl("ContainersField") as JQDataGrid;
                string sRowID = _gridContainers.SelectedRowID;
                if (sRowID != null)
                {
                    string _txtCurrentId = (_gridContainers.GridContext as BoundContext).GetCell(sRowID, "BatchNo").ToString();
                    if (_txtCurrentId != null)
                    {
                        _txtBatchId.Data = _txtCurrentId.ToString();
                    }
                }
                else
                {
                }
            }
            catch (Exception ex)
            { }
        }
        public void SelectionIdField_DataChanged()
        {
            try
            {
                if (_txtSelectionId.Data != null)
                {
                    Page.StatusBar.ClearMessage();

                    // get the session and user profile
                    var fs = FrameworkManagerUtil.GetFrameworkSession();

                    // init the service, service data and service info objects
                    SetBatchIdService objSvc = new SetBatchIdService(fs.CurrentUserProfile);
                    SetBatchId objSvcData = new SetBatchId { SelectionId = (string)_txtSelectionId.Data };
                    SetBatchId_Info objSvcInfo = new SetBatchId_Info { Containers = FieldInfoUtil.RequestValue() };

                    // init the result object
                    SetBatchId_Result objResult = new SetBatchId_Result();

                    // execute to request the value
                    ResultStatus resultStatus = objSvc.ResolveSelectionId(objSvcData, new SetBatchId_Request { Info = objSvcInfo }, out objResult);

                    if (resultStatus.IsSuccess)
                    {
                        JQDataGrid _gridContainers = Page.FindCamstarControl("ContainersField") as JQDataGrid;
                        bool isFirst = true;
                        int i = _gridContainers.BoundContext.GetTotalRows();                        
                        foreach (OM.ContainerRef oContainer in objResult.Value.Containers)
                        {
                            // check if lot not exist in the grid else add to the grid
                            string strSelectedGridId = (_gridContainers.GridContext as BoundContext).GetRowIdByCellValue("Lot", oContainer.Name.ToString());

                            if (string.IsNullOrEmpty(strSelectedGridId))
                            {
                                SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "SetBatchId", oContainer.Name.ToString(), false, ref _gridContainers, "ContainersField", true);
                                if (isFirst)
                                {
                                    string batchID = (_gridContainers.GridContext as BoundContext).GetCell(i.ToString().PadLeft(6, '0'), "BatchNo").ToString();
                                    _txtBatchId.Data = batchID;
                                    isFirst = false;
                                }
                            }
                        }
                    }
                    else
                        DisplayMessage(resultStatus);
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
            finally
            {
                _txtSelectionId.TextControl.Text = "";
                _txtSelectionId.Focus();
            }
        }
        public override void GetInputData(Service serviceData)
        {
            if (_txtEmployee.Data != null)
            {
                (serviceData as SetBatchId).Employee = new NamedObjectRef();
                (serviceData as SetBatchId).Employee.Name = _txtEmployee.Data.ToString();
            }
            if (_gridContainers.Data != null)
            {
                int intTotalContainers = _gridContainers.BoundContext.GetTotalRows();

                (serviceData as SetBatchId).Containers = new ContainerRef[intTotalContainers];

                // collect the containers
                for (int x = 0; x < intTotalContainers; x++)
                {
                    (serviceData as SetBatchId).Containers[x] = new ContainerRef();
                    (serviceData as SetBatchId).Containers[x].Name = _gridContainers.GridContext.GetCell(x.ToString().PadLeft(6, '0'), "Lot").ToString();
                }
            }
            base.GetInputData(serviceData);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);

            if (Page.IsPostBack)
            {
                // Check if it is a pop up close, get the return result
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {
                    if (_envSelectedLots != null)
                        // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                        if (Page.DataContract.GetValueByName("SetBatchId_SelectedLotsListDM") != null)
                            _envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("SetBatchId_SelectedLotsListDM") as string[];

                    if (_envSelectedLots.SS_ContainersList != null)
                    {
                        string[] sContainers;
                        sContainers = _envSelectedLots.SS_ContainersList;

                        JQDataGrid _gridContainers = Page.FindCamstarControl("ContainersField") as JQDataGrid;
                        bool isFirst = true;
                        int i = _gridContainers.BoundContext.GetTotalRows();                        
                        foreach (string sContainersItem in sContainers)
                        {
                            // check if lot not exist in the grid else add to the grid
                            string strSelectedGridId = (_gridContainers.GridContext as BoundContext).GetRowIdByCellValue("Lot", sContainersItem);                            

                            if (string.IsNullOrEmpty(strSelectedGridId))
                            {
                                SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "SetBatchId", sContainersItem, false, ref _gridContainers, "ContainersField", true);
                                if (isFirst)
                                {                                                                        
                                    string batchID = (_gridContainers.GridContext as BoundContext).GetCell(i.ToString().PadLeft(6, '0'), "BatchNo").ToString();
                                    _txtBatchId.Data = batchID;
                                    isFirst = false;
                                }
                            }
                        }
                        //nullify the containers list
                        _envSelectedLots.SS_ContainersList = null;
                    }
                }
            }
        }
        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                Page.ClearValues();
                _gridContainers.ClearData();
                _txtSelectionId.Focus(); 
            }
        }
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Reset")
            {
                Page.ShopfloorReset(sender, e);
                _txtSelectionId.Focus();
            }
        }
    }
}



