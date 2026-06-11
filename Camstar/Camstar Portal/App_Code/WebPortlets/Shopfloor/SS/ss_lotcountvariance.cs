/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Reflection;
using System.Reflection.Emit;
using System.ComponentModel;

using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using PERS = Camstar.WebPortal.Personalization;
using CamstarPortal.WebControls;
using Camstar.WCF.Services;
using System.Collections;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Constants;


namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    /// Summary description for LotCountVariance
    /// </summary>
    public class LotCountVarianceTxn : scsShopfloorBase
    {
        #region Properties 
        CWC.TextBox _txtSelectionIdField { get {return Page.FindCamstarControl("SelectionIdField") as CWC.TextBox; } }
        CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("ComputerNameField") as CWC.TextBox; } }
        CWC.TextBox _txtEmployeeField { get { return Page.FindCamstarControl("EmployeeField") as CWC.TextBox; } }
        CWC.TextBox _txtNewQtyField { get { return Page.FindCamstarControl("NewQtyField") as CWC.TextBox; } }
        CWC.TextBox _txtTotalCountVariancePercentageField { get { return Page.FindCamstarControl("TotalCountVariancePercentageField") as CWC.TextBox; } }
        CWC.NamedObject _ndoProcessTypeField { get { return Page.FindCamstarControl("ProcessTypeField") as CWC.NamedObject; } }
        CWC.NamedObject _ndoEquipmentField { get { return Page.FindCamstarControl("EquipmentField") as CWC.NamedObject; } }
        JQDataGrid _gridLotInfoField { get { return Page.FindCamstarControl("LotInfoFieldGrid") as JQDataGrid; } }
        JQDataGrid _gridWafersDetailsField { get { return Page.FindCamstarControl("WafersDetailsField") as JQDataGrid; } }
        CWC.NamedObject _ndoVarianceReasonField { get { return Page.FindCamstarControl("VarianceReasonField") as CWC.NamedObject; } }
        CWC.ContainerList _clHiddenSelectedContainer { get { return Page.FindCamstarControl("HiddenSelectedContainer") as CWC.ContainerList; } }
        SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }
        #endregion

        #region Constants
        const string const_sNewQty = "NewQty";
        const string const_sNDPW = "NDPW";
        const string const_sGoodQty = "GoodQty";
        const string const_sProcessType = "ProcessType";
        const string const_sSelectionId = "SelectionId";
        const string const_sWaferScribeNumber = "WaferScribeNumber";

        #endregion

        public LotCountVarianceTxn()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Page.IsPostBack)
            {
                // Check if it is a pop up close, get the return result
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {
                    if (_envSelectedLots != null)
                        // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                        if (Page.DataContract.GetValueByName("SelectedLotsListDM") != null)
                            _envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("SelectedLotsListDM") as string[];

                    if (_envSelectedLots.SS_ContainersList != null)
                    {
                        string[] sContainers;
                        sContainers = _envSelectedLots.SS_ContainersList;

                        // Set Selection Id textbox value
                        _txtSelectionIdField.Data = sContainers[0];
                        
                        //nullify the containers list
                        _envSelectedLots.SS_ContainersList = null;
                    }
                }
            }
            else
            {
                _txtComputerNameField.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
                _txtNewQtyField.Hidden = true;
            }
        }

        public static System.Boolean IsNumeric (System.Object Expression)
        {
            if(Expression == null || Expression is DateTime)
                return false;

            if(Expression is Int16 || Expression is Int32 || Expression is Int64 || Expression is Decimal || Expression is Single || Expression is Double || Expression is Boolean)
                return true;

            try 
            {
                if(Expression is string)
                    Double.Parse(Expression as string);
                else
                    Double.Parse(Expression.ToString());
                    return true;
            } 
            catch 
            {
            } // just dismiss errors but return false
            return false;
            
        }

        public void SelectionIdField_DataChanged(object sender, EventArgs e)
        {
            if (!_txtSelectionIdField.IsEmpty)
            {
                FetchData(const_sSelectionId);
            }
        }

        public void ProcessTypeField_DataChanged(object sender, EventArgs e)
        {
            if (!_ndoProcessTypeField.IsEmpty)
                FetchData(const_sProcessType);
        }

        public void NewQtyField_DataChanged(object sender, EventArgs e)
        {
            if (!_clHiddenSelectedContainer.IsEmpty && !_txtNewQtyField.IsEmpty) 
            {
                FetchData(const_sNewQty);
            }
        }
        
        public void SetLotSelection(string sSelectionId)
        {
            JQDataGrid _gridLotInfoFieldx = Page.FindCamstarControl("LotInfoFieldGrid") as JQDataGrid;
            _gridLotInfoFieldx.ClearData();
            SEMI.AppCode.UIUtility.GetLotQuerySelection(this, this.PrimaryServiceType.ToString(), sSelectionId, true, ref _gridLotInfoFieldx, "LotInfoFieldGrid");
        }

        public void ComputeNewQty()
        {
            _txtTotalCountVariancePercentageField.Data = String.Empty;
            if (!_txtSelectionIdField.IsEmpty)
            {
                int savedCount = _gridWafersDetailsField.GridContext.GetTotalRows();
                int iNewQty = 0;
                int iCellQty = 0;
                for (int x = 0; x < savedCount; x++)
                {
                    string sRowId = x.ToString().PadLeft(6, '0');

                    string sNewQty = _gridWafersDetailsField.GridContext.GetCell(sRowId, const_sNewQty).ToString();
                    Int32.TryParse(sNewQty, out iCellQty);
                    iNewQty = iNewQty + iCellQty;
                }

                if (iNewQty > 0)
                {
                    _txtNewQtyField.Data = iNewQty;
                }

                NewQtyField_DataChanged(null, null);
            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            if (action != null && action.Parameters == "Compute")
            {
                ComputeNewQty();
            }
            else if (action != null && action.Parameters == "Reset")
            {
               // Page.ClearValues();
                _gridLotInfoField.ClearData();
                _gridWafersDetailsField.ClearData();
                Page.ShopfloorReset(sender, e);
            }
        }

        private void FetchData(string sEventName)
        {
            string sSelectionId = "";
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            LotCountVariance oServiceData = new LotCountVariance();
            LotCountVariance_Info oServiceInfo = new LotCountVariance_Info();
            LotCountVarianceService oService = new LotCountVarianceService(fs.CurrentUserProfile);
            LotCountVariance_Request oRequest = new LotCountVariance_Request();
            LotCountVariance_Result oResponseData = new LotCountVariance_Result();

            // Prepare the request
            if (sEventName == const_sSelectionId)
            {
                sSelectionId = _txtSelectionIdField.Data.ToString();
                oServiceData.SelectionId = sSelectionId;
                oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
                oServiceInfo.ProcessTypeSelection = FieldInfoUtil.RequestValue();
                oServiceInfo.LotWafers = new LotWafers_Info();
                oServiceInfo.LotWafers.WaferScribeNumber = FieldInfoUtil.RequestValue();
                oServiceInfo.LotWafers.WaferNumber = FieldInfoUtil.RequestValue();
                oServiceInfo.LotWafers.GoodQty = FieldInfoUtil.RequestValue();
                oServiceInfo.LotWafers.NDPW = FieldInfoUtil.RequestValue();
                oServiceInfo.IsWaferProcessing = FieldInfoUtil.RequestValue();
            }
            else if (sEventName == const_sNewQty)
            {
                oServiceData.Container = new ContainerRef();
                oServiceData.Container.Name = _clHiddenSelectedContainer.Data.ToString();
                oServiceData.ProcessType = _ndoProcessTypeField.Data as NamedObjectRef;
                oServiceData.Equipment = _ndoEquipmentField.Data as NamedObjectRef;
                oServiceData.NewQty = Double.Parse(_txtNewQtyField.Data.ToString());
                oServiceInfo.TotalCountVariancePercentage = FieldInfoUtil.RequestValue();
            }
            else if (sEventName == const_sProcessType)
            {
                oServiceData.Container = new ContainerRef();
                oServiceData.Container.Name = _clHiddenSelectedContainer.Data.ToString();
                oServiceData.ProcessType = _ndoProcessTypeField.Data as NamedObjectRef;
                oServiceInfo.EquipmentSelection = FieldInfoUtil.RequestValue();
            }

            // Request the data
            oRequest.Info = oServiceInfo;
            OM.ResultStatus oResultStatus = oService.ResolveSelectionId(oServiceData, oRequest, out oResponseData);
            if (oResultStatus.IsSuccess)
            {
                if (sEventName == const_sSelectionId)
                {
                    // Clear all fields for fresh transaction
                    ResetFields();

                    // Set the value of the resolve Container Name
                    _clHiddenSelectedContainer.Data = oResponseData.Value.SelectionContainer.Name.ToString();

                    int iWaferCount = oResponseData.Value.LotWafers != null ? oResponseData.Value.LotWafers.Count() : 0;
                    if (iWaferCount > 0)
                    {
                        LotWafers[] objWafers = new LotWafers[iWaferCount];
                        objWafers = oResponseData.Value.LotWafers;

                        // wrap LotWafers to add NewQty column
                        LotWafersExWrap[] objLotWafersEx = new LotWafersExWrap[iWaferCount];
                        int intWaferIndex = 0;
                        foreach (LotWafers objWafer in objWafers)
                        {
                            objLotWafersEx[intWaferIndex] = new LotWafersExWrap(objWafer);
                            intWaferIndex++;
                        }

                        // Bind response data to datagrid
                        (_gridWafersDetailsField.GridContext as BoundContext).Data = objLotWafersEx.ToArray();
                        _gridWafersDetailsField.BoundContext.LoadData();
                        CamstarWebControl.SetRenderToClient(_gridWafersDetailsField);
                    }

                    // Determine if NewQty textbox is displayed.
                    _txtNewQtyField.Hidden = (oResponseData.Value.IsWaferProcessing == true);
                    CamstarWebControl.SetRenderToClient(_txtNewQtyField);

                    // Fetch Lot Information and Display on datagrid
                    SetLotSelection(_clHiddenSelectedContainer.Data.ToString());

                    if (oResponseData.Value.ProcessTypeSelection != null)
                    {
                        CWC.NamedObject _ndoProcessTypeFieldEx = Page.FindCamstarControl("ProcessTypeField") as CWC.NamedObject;
                        SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoProcessTypeFieldEx, oResponseData.Value.ProcessTypeSelection);

                        // Set the first process type as a default
                        _ndoProcessTypeField.Data = oResponseData.Value.ProcessTypeSelection[0].ToString();

                        CamstarWebControl.SetRenderToClient(_ndoProcessTypeFieldEx);
                    }
                    _txtSelectionIdField.Data = sSelectionId;
                }
                else if (sEventName == const_sProcessType)
                {
                    if (oResponseData.Value.EquipmentSelection != null)
                    {
                        CWC.NamedObject _ndoEquipmentFieldEx = Page.FindCamstarControl("EquipmentField") as CWC.NamedObject;
                        SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoEquipmentFieldEx, oResponseData.Value.EquipmentSelection);
                        
                        // Set the first equipment as a default
                        _ndoEquipmentField.Data = oResponseData.Value.EquipmentSelection[0].ToString();

                        CamstarWebControl.SetRenderToClient(_ndoEquipmentFieldEx);
                    }
                }
                else if (sEventName == const_sNewQty)
                {
                    _txtTotalCountVariancePercentageField.Data = oResponseData.Value.TotalCountVariancePercentage;
                }
            }
            else
            {
                DisplayMessage(oResultStatus);
                ResetFields();
            }
        }      

        public override void GetInputData(OM.Service serviceData)
        {
            try
            {
                bool bUseGoodQty = false;
                int iWaferQty = 0;
                int iWafersWithVariance = 0;
                int iNewQty = 0;
                int iVarianceQty = 0;
                base.GetInputData(serviceData);
                if (this.PrimaryServiceType == "LotCountVariance")
                {
                    int gridCount = _gridWafersDetailsField.BoundContext.GetTotalRows();
                    if (gridCount > 0)
                    {
                        if (serviceData is OM.LotCountVariance)
                        {
                            // For Loop
                            (serviceData as OM.LotCountVariance).WafersDetails = new ModifyWafersDetails[gridCount];
                            
                            for (int i = 0; i < gridCount; i++)
                            {
                                string sRowId = i.ToString().PadLeft(6, '0');

                                (serviceData as OM.LotCountVariance).WafersDetails[i] = new ModifyWafersDetails();
                                (serviceData as OM.LotCountVariance).WafersDetails[i].WaferScribeNumber = _gridWafersDetailsField.GridContext.GetCell(sRowId, const_sWaferScribeNumber).ToString();

                                string sGoodQty = _gridWafersDetailsField.GridContext.GetCell(sRowId, const_sGoodQty).ToString();
                                if (IsNumeric(sGoodQty) && int.Parse(sGoodQty) > 0)
                                {
                                    bUseGoodQty = true;
                                }

                                if (bUseGoodQty)
                                {
                                    iWaferQty = int.Parse(_gridWafersDetailsField.GridContext.GetCell(sRowId, const_sGoodQty).ToString());
                                }
                                else
                                {
                                    iWaferQty = int.Parse(_gridWafersDetailsField.GridContext.GetCell(sRowId, const_sNDPW).ToString());
                                }

                                iNewQty = int.Parse(_gridWafersDetailsField.GridContext.GetCell(sRowId, const_sNewQty).ToString());

                                if (IsNumeric(iWaferQty) && IsNumeric(iNewQty))
                                {
                                    iVarianceQty = iNewQty - iWaferQty;
                                    (serviceData as OM.LotCountVariance).WafersDetails[i].VarianceQty = iVarianceQty;
                                
                                    if (iVarianceQty != 0)
                                    {
                                        iWafersWithVariance = iWafersWithVariance + 1;
                                    }
                                }
                                else
                                {
                                    (serviceData as OM.LotCountVariance).WafersDetails[i].VarianceQty = 0;
                                }
                            } //End for loop
                        }
                    }
                    if (iWafersWithVariance == 0)
                    {
                        (serviceData as OM.LotCountVariance).WafersDetails = null;
                    }

                    if (_txtNewQtyField.Visible && iWafersWithVariance == 0)
                    {
                        (serviceData as OM.LotCountVariance).NewQty = _txtNewQtyField.Data != null ? double.Parse(_txtNewQtyField.Data.ToString()) : 0;
                    }

                    (serviceData as OM.LotCountVariance).ComputerName = _txtComputerNameField.Data.ToString();
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }

        private void ResetFields()
        {
            _gridLotInfoField.ClearData();
            _gridWafersDetailsField.ClearData();
            _ndoVarianceReasonField.ClearData();
            _ndoEquipmentField.ClearData();
            _ndoEquipmentField.ClearSelectionValues();
            _ndoProcessTypeField.ClearData();
            _ndoProcessTypeField.ClearSelectionValues();
            _txtNewQtyField.ClearData();
            _txtTotalCountVariancePercentageField.ClearData();
        }
        
        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                Page.ShopfloorReset(null, null);
                ResetFields();
            }
        }

        public class LotWafersExWrap
        {
            private string strNewQty;
            private LotWafers objLotWafers;

            public string NewQty
            {
                get { return strNewQty; }
                set { strNewQty = value; }
            }

            public string WaferScribeNumber
            {
                get { return objLotWafers.WaferScribeNumber != null ? objLotWafers.WaferScribeNumber.ToString() : ""; }
                set { objLotWafers.WaferScribeNumber = value; }
            }

            public string WaferNumber
            {
                get { return objLotWafers.WaferScribeNumber != null ? objLotWafers.WaferNumber.ToString() : ""; }
                set { objLotWafers.WaferNumber = value; }
            }

            public int NDPW
            {
                get { return objLotWafers.NDPW != null ? int.Parse(objLotWafers.NDPW.ToString()) : 0; }
                set { objLotWafers.NDPW = value; }
            }

            public int GoodQty
            {
                get { return objLotWafers.GoodQty != null ? int.Parse(objLotWafers.GoodQty.ToString()) : 0; }
                set { objLotWafers.GoodQty = value; }
            }

            public LotWafersExWrap(LotWafers WaferData)
            {
                objLotWafers = new LotWafers();
                objLotWafers = WaferData;

                objLotWafers.WaferScribeNumber = WaferData.WaferScribeNumber;

                string strGoodQty = objLotWafers.GoodQty != null ? objLotWafers.GoodQty.ToString() : "0";
                string strNDPW = objLotWafers.NDPW != null ? objLotWafers.NDPW.ToString() : "0";
                if (IsNumeric(strGoodQty) && Convert.ToInt32(strGoodQty) > 0)
                {
                    strNewQty = strGoodQty;
                }
                else
                {
                    strNewQty = strNDPW;
                }
            }
        }
    }
}



