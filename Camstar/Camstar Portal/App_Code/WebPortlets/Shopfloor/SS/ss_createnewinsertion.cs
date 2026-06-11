/* Copyright 2025 Siemens */
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
/// Summary description for Class1
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_CreateNewInsertion : scsShopfloorBase
	{
        protected CWC.TextBox _txtSelectionId                           { get { return Page.FindCamstarControl("CreateNewInsertion_SelectionId") as CWC.TextBox; } }
        protected CWC.TextBox _txtComputerName                          { get { return Page.FindCamstarControl("CreateNewInsertion_ComputerName") as CWC.TextBox; } }
        protected CWC.TextBox _txtEmployee                              { get { return Page.FindCamstarControl("CreateNewInsertion_Employee") as CWC.TextBox; } }
        protected CWC.TextBox _lstWaferNumbers                          { get { return Page.FindCamstarControl("CreateNewInsertion_WaferNumbers") as CWC.TextBox; } }
        protected CWC.TextBox _txtComments                              { get { return Page.FindCamstarControl("CreateNewInsertion_Comments") as CWC.TextBox; } }
        protected JQDataGrid _gridLotInfo                               { get { return Page.FindCamstarControl("CreateNewInsertion_SelectedLotInfo") as JQDataGrid; } }
        protected JQDataGrid _gridDetails                               { get { return Page.FindCamstarControl("CreateNewInsertion_ServiceDetails") as JQDataGrid; } }
        protected JQDataGrid _gridAvailableDetails                      { get { return Page.FindCamstarControl("CreateNewInsertion_AvailableDetails") as JQDataGrid; } }        
        protected JQDataGrid _gridAvailableWafers                       { get { return Page.FindCamstarControl("ServiceDetails_WafersDetails") as JQDataGrid; } }        
        protected CWC.Button _btnWaferPopup                             { get { return Page.FindCamstarControl("CreateNewInsertion_WaferPopup") as CWC.Button; } }
        protected CWC.CheckBox _checkQty                                { get { return Page.FindCamstarControl("IsWaferProcessingField") as CWC.CheckBox; } }
        protected SEMI.AppCode.DataEnvelopControl _envSelectedLots      { get { return Page.FindCamstarControl("CreateNewInsertion_Lots") as SEMI.AppCode.DataEnvelopControl; } }
        protected SEMI.AppCode.DataEnvelopControl _envSelectedWafers    { get { return Page.FindCamstarControl("CreateNewInsertion_Wafers") as SEMI.AppCode.DataEnvelopControl; } }           
       	
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            Page.ClearValues();
            Page.ShopfloorReset(sender, e);
            _gridLotInfo.ClearData();
            _txtSelectionId.Focus();
        }

        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                Page.ClearValues();                
                _gridLotInfo.ClearData();
                _txtSelectionId.Focus();
            }
        }

        public override void GetInputData(Service serviceData)
        {
			base.GetInputData(serviceData);

			if (_txtSelectionId.Data != null)
            {
                (serviceData as CreateNewInsertion).Container = new ContainerRef();
                (serviceData as CreateNewInsertion).Container.Name = _txtSelectionId.Data.ToString();
            }

            if (_txtEmployee.Data != null)
            {
                (serviceData as CreateNewInsertion).Employee = new NamedObjectRef();
                (serviceData as CreateNewInsertion).Employee.Name = _txtEmployee.Data.ToString();
            }

            if (_gridDetails.Data != null)
            {
                int _intCount = (_gridDetails.GridContext as BoundContext).GetTotalRows();                    
                if (!_checkQty.CheckControl.Checked)
                {
                    (serviceData as CreateNewInsertion).ServiceDetails = new CreateInsertionDetails[_intCount];

                    for (int i = 0; i < _intCount; i++)
                    {
                        (serviceData as CreateNewInsertion).ServiceDetails[i] = new CreateInsertionDetails();
                        (serviceData as CreateNewInsertion).ServiceDetails[i].WafersDetails = ((_gridAvailableDetails.GridContext as BoundContext).GetCell(i.ToString().PadLeft(6, '0'), "SelWafersItem") as WIPLotTxnWafersDetails[]);
						(serviceData as CreateNewInsertion).ServiceDetails[i].ProcessType = ((_gridDetails.GridContext as BoundContext).GetCell(i.ToString().PadLeft(6, '0'), "ProcessType") as NamedObjectRef);
						(serviceData as CreateNewInsertion).ServiceDetails[i].ProcessStatus = ((_gridDetails.GridContext as BoundContext).GetCell(i.ToString().PadLeft(6, '0'), "ProcessStatus").ToString());
					}
                }
            }                
        }

        public void DetailGrid_RowSelected(Object sender, EventArgs e)
        {
            JQDataGrid _gridDetails = Page.FindCamstarControl("CreateNewInsertion_ServiceDetails") as JQDataGrid;                        
            string sRowID = _gridDetails.SelectedRowID;
            if (sRowID != null && !_checkQty.CheckControl.Checked)
            {
                WIPLotTxnWafersDetails[] oWafers = ((_gridDetails.GridContext as BoundContext).GetCell(sRowID, "WafersDetails") as WIPLotTxnWafersDetails[]);                    
                string[] sWaferNumbers = new string[oWafers.Count()];
                for (int i = 0; i < oWafers.Count(); i++)
                {
                    sWaferNumbers[i] = oWafers[i].WaferScribeNumber.ToString();                        
                }                    
                _envSelectedWafers.SS_WafersList = sWaferNumbers.ToArray();

                WIPLotTxnWafersDetails[] oAvWafers = ((_gridAvailableDetails.GridContext as BoundContext).GetCell(sRowID, "SelWafersItem") as WIPLotTxnWafersDetails[]);
                _gridAvailableWafers.Data = oAvWafers.ToArray();

                _btnWaferPopup.Enabled = true;                    
            }          
        }

        public void SelectionId_DataChanged(Object sender, EventArgs e)
        {            
            CWC.TextBox _txtSelectionId = Page.FindCamstarControl("CreateNewInsertion_SelectionId") as CWC.TextBox;
            JQDataGrid _gridLotInfo = Page.FindCamstarControl("CreateNewInsertion_SelectedLotInfo") as JQDataGrid;            
            _gridLotInfo.ClearData();
            _gridAvailableDetails.ClearData();
            _gridAvailableWafers.ClearData();
            _gridDetails.ClearData();

            if (_txtSelectionId.Data != null)
            {
                SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "CreateNewInsertion", _txtSelectionId.Data.ToString(), false, ref _gridLotInfo, "CreateNewInsertion_SelectedLotInfo", false);
                FetchData(_txtSelectionId.Data.ToString());
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            CWC.CheckBox _checkQty = Page.FindCamstarControl("IsWaferProcessingField") as CWC.CheckBox;
            JQDataGrid _gridDetails = Page.FindCamstarControl("CreateNewInsertion_ServiceDetails") as JQDataGrid;            
            (_gridDetails.GridContext as BoundContext).Fields["AllowQtyOverride"].Visible = (bool)_checkQty.Data;
            (_gridDetails.GridContext as BoundContext).Fields["DisplayName"].Visible = (bool)_checkQty.Data;
            (_gridDetails.GridContext as BoundContext).Fields["LotWafersItem"].Visible = (bool)_checkQty.Data;
            (_gridDetails.GridContext as BoundContext).Fields["MaxQtyToProcess"].Visible = (bool)_checkQty.Data;
            (_gridDetails.GridContext as BoundContext).Fields["MinQtyToProcess"].Visible = (bool)_checkQty.Data;
            (_gridDetails.GridContext as BoundContext).Fields["QtyToProcess"].Visible = (bool)_checkQty.Data;
            (_gridDetails.GridContext as BoundContext).Fields["QtyToProcess"].Editable = (bool)_checkQty.Data;
            (_gridDetails.GridContext as BoundContext).Fields["TestPlan"].Visible = (bool)_checkQty.Data;
            (_gridDetails.GridContext as BoundContext).Fields["TestStatus"].Visible = (bool)_checkQty.Data;
            (_gridDetails.GridContext as BoundContext).Fields["TestSubPlan"].Visible = (bool)_checkQty.Data;
            (_gridDetails.GridContext as BoundContext).Fields["TestSubPlanType"].Visible = (bool)_checkQty.Data;
            (_gridDetails.GridContext as BoundContext).Fields["WaferMapDetailsType"].Visible = (bool)_checkQty.Data;
            (_gridDetails.GridContext as BoundContext).Fields["WIPTestStatus"].Visible = (bool)_checkQty.Data; 
			(_gridDetails.GridContext as BoundContext).Fields["ProcessStatus"].Editable = true;			
            Page.RenderToClient = true;
        }        

        protected override void OnLoad(EventArgs e)
        {            
            _txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);

            if (Page.IsPostBack)
            {
                // Check if it is a pop up close, get the return result
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {
                    if (_envSelectedLots != null)
                        // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                        if (Page.DataContract.GetValueByName("CreateNewInsertion_Lots") != null)
                            _envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("CreateNewInsertion_Lots") as string[];

                    if (_envSelectedLots.SS_ContainersList != null)
                    {
                        string[] sContainers;
                        sContainers = _envSelectedLots.SS_ContainersList;

                        CWC.TextBox _txtSelectionId = Page.FindCamstarControl("CreateNewInsertion_SelectionId") as CWC.TextBox;
                        JQDataGrid _gridLotInfo = Page.FindCamstarControl("CreateNewInsertion_SelectedLotInfo") as JQDataGrid;
                        _txtSelectionId.Data = sContainers[0].ToString();
                        _gridLotInfo.ClearData();

                        SelectionId_DataChanged(null, null);
                        
                        //nullify the containers list
                        _envSelectedLots.SS_ContainersList = null;
                    }

                    if (!_gridAvailableWafers.IsEmpty)
                    {
                        _gridAvailableDetails.GridContext.SetCell(_gridDetails.SelectedRowID, "SelWafersItem", Page.DataContract.GetValueByName("WaferPopupReturn_DM"));
                        DetailGrid_RowSelected(null, null);
                    }
                }
            }                        
            base.OnLoad(e);
        }


        protected void FetchData(string LotId)
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                // init service objects

                CreateNewInsertionService oService = new CreateNewInsertionService(fs.CurrentUserProfile);
                CreateNewInsertion oServiceData = new CreateNewInsertion();
                CreateNewInsertion_Info oServiceInfo = new CreateNewInsertion_Info();
                CreateNewInsertion_Result oServiceResult = new CreateNewInsertion_Result();

                oServiceData.Container = new ContainerRef();
                oServiceData.Container.Name = LotId;

                oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
                oServiceInfo.IsWaferProcessing = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceDetailsSelection = new CreateInsertionDetails_Info();
                oServiceInfo.ServiceDetailsSelection.ProcessType = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceDetailsSelection.ProcessStatus = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceDetailsSelection.WafersDetails = new WIPLotTxnWafersDetails_Info();
                oServiceInfo.ServiceDetailsSelection.WafersDetails.WaferScribeNumber = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceDetailsSelection.AllowQtyOverride = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceDetailsSelection.QtyToProcess = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceDetailsSelection.ValidQtys = new CreateInsertionDetailsQtys_Info();
                oServiceInfo.ServiceDetailsSelection.ValidQtys.QtyToProcess = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceDetailsSelection.MinQtyToProcess = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceDetailsSelection.MaxQtyToProcess = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceDetailsSelection.TestStatus = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceDetailsSelection.WIPTestStatus = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceDetailsSelection.TestPlan = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceDetailsSelection.TestSubPlan = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceDetailsSelection.TestSubPlanType = FieldInfoUtil.RequestValue();

                // init request
                CreateNewInsertion_Request oServiceRequest = new CreateNewInsertion_Request();
                oServiceRequest.Info = oServiceInfo;
                // execute!
                ResultStatus oResultStatus = oService.GetEnvironment(oServiceData, oServiceRequest, out oServiceResult);

                JQDataGrid _gridQtyDetails = Page.FindCamstarControl("CreateNewInsertion_QtyDetails") as JQDataGrid;
                JQDataGrid _gridWaferDetails = Page.FindCamstarControl("CreateNewInsertion_WaferDetails") as JQDataGrid;

                if (oResultStatus.IsSuccess)
                {
                    if (oServiceResult.Value.IsWaferProcessing == true)
                    {
                        _checkQty.CheckControl.Checked = false;
                        _checkQty.Data = false;
                    }
                    else
                    {
                        _checkQty.CheckControl.Checked = true;
                        _checkQty.Data = true;
                    }

                    if (!oServiceResult.Value.ServiceDetailsSelection.IsNullOrEmpty())
                    {           
						
                        Array oDetailArray = oServiceResult.Value.ServiceDetailsSelection.ToArray();
						foreach (CreateInsertionDetails oDetailRecord in oDetailArray)
                        {
							if (oDetailRecord.ProcessStatus.ToString().ToUpper() == "NR")
								oDetailRecord.ProcessStatus = "ACTIVE";
						}
                        (_gridDetails.GridContext as BoundContext).Data = oDetailArray;
                        _gridDetails.BoundContext.LoadData();

                        List<SS_SelWafers_ItemData> oNewItemList = new List<SS_SelWafers_ItemData>();    
                        int i = 0;
                        RecordSet rsWafersDetails = new RecordSet();
                        foreach (CreateInsertionDetails oDetailRecord in oDetailArray)
                        {
                            SS_SelWafers_ItemData oCurrentRow = new SS_SelWafers_ItemData();

                            if (!oDetailRecord.WafersDetails.IsNullOrEmpty())
                            {
                                foreach (WIPLotTxnWafersDetails oWaferDetail in oDetailRecord.WafersDetails)
                                {
                                    oWaferDetail.Self = null;
                                    oWaferDetail.ListItemIndex = null;
                                }
                            }

                            oCurrentRow.SelWafersItem = oDetailRecord.WafersDetails;

                            oNewItemList.Add(oCurrentRow);
                        }
                        (_gridAvailableDetails.GridContext as BoundContext).Data = oNewItemList.ToArray();
                        _gridAvailableDetails.BoundContext.LoadData();
                    }
                }
                else
                {
                    DisplayMessage(oResultStatus);
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message, false));
            }
        }

        private class SS_SelWafers_ItemData
        {
            private WIPLotTxnWafersDetails[] oSelWafers;            

            public SS_SelWafers_ItemData()
            { }

            public WIPLotTxnWafersDetails[] SelWafersItem
            {
                get { return oSelWafers; }
                set { oSelWafers = value; }
            }
        }             
	}
}



