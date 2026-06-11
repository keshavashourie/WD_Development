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
    /// Summary description for WIPEqpMaterialsSetup
    /// </summary>
    public class WIPEqpMaterialsSetup : MatrixWebPart
    {
        #region Properties
        CWC.CheckBox _chkIsActiveField { get { return Page.FindCamstarControl("WIPEqpMaterialsSetup_IsActive") as CWC.CheckBox; } }
        CWC.CheckBox _chkAutoReturnMaterial { get { return Page.FindCamstarControl("WIPEqpMaterialsSetup_AutoReturnMaterial") as CWC.CheckBox; } }
        CWC.CheckBox _chkAutoCombineInventory { get { return Page.FindCamstarControl("WIPEqpMaterialsSetup_AutoCombineInventory") as CWC.CheckBox; } }
        
        CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("WIPEqpMaterialsSetup_ComputerName") as CWC.TextBox; } }

        CWC.TextBox _txtMaterialLotField { get { return Page.FindCamstarControl("WIPEqpMaterialsSetup_MaterialLotName") as CWC.TextBox; } }
        CWC.RevisionedObject _rdoMaterialPartField { get { return Page.FindCamstarControl("WIPEqpMaterialsSetup_MaterialPart") as CWC.RevisionedObject; } }
        CWC.Button _btnWIPEqpMaterialsSetup_MaterialPartButton { get { return Page.FindCamstarControl("WIPEqpMaterialsSetup_MaterialPartButton") as CWC.Button; } }
        CWC.Button _btnWIPEqpMaterialsSetup_ProductButton { get { return Page.FindCamstarControl("WIPEqpMaterialsSetup_ProductButton") as CWC.Button; } }
        CWC.Button _btnWIPEqpMaterialsSetup_AddMaterialPartButton { get { return Page.FindCamstarControl("WIPEqpMaterialsSetup_AddMaterialPartButton") as CWC.Button; } }
        CWC.Button _btnSubmit { get { return Page.FindCamstarControl("WIPEqpMaterialsSetup_SubmitButton") as CWC.Button; } }
        CWC.Button _btnReset { get { return Page.FindCamstarControl("WIPEqpMaterialsSetup_ResetButton") as CWC.Button; } }
        CWC.ContainerList _ContainerField { get { return Page.FindCamstarControl("WIPEqpMaterialsSetup_Container") as CWC.ContainerList; } }
        CWC.NamedObject _ndoProcessTypeField { get { return Page.FindCamstarControl("WIPEqpMaterialsSetup_ProcessType") as CWC.NamedObject; } }
        CWC.NamedObject _ndoEquipmentField { get { return Page.FindCamstarControl("WIPEqpMaterialsSetup_Equipment") as CWC.NamedObject; } }
        JQDataGrid _gridDetailsField { get { return Page.FindCamstarControl("WIPEqpMaterialsSetup_Details") as JQDataGrid; } }
        JQDataGrid _gridMaterialsRequiredField { get { return Page.FindCamstarControl("WIPEqpMaterialsSetup_MaterialsRequired") as JQDataGrid; } }
        JQDataGrid _gridAlternateMaterialsField { get { return Page.FindCamstarControl("WIPEqpMaterialsSetup_AlternateMaterials") as JQDataGrid; } }
        JQDataGrid _gridLotInProcessField { get { return Page.FindCamstarControl("EquipmentMaterialsSetup_LotsInProcess") as JQDataGrid; } }

        protected CWC.TextBox _txtWIPEqpMaterialsSetup_Init { get { return Page.FindCamstarControl("WIPEqpMaterialsSetup_Init") as CWC.TextBox; } }
        protected bool _bIsPopup { get { return Page.IsAJAXFloatingFrame; } }

        #endregion

        #region Constants
        const string const_sResource = "Resource";
        const string const_sMaterialLot = "MaterialLot";
        const string const_sMaterialPart = "MaterialPart";

        #endregion

        #region Functions
        //--------------------------------------------------
        //
        //--------------------------------------------------
        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            if (_txtWIPEqpMaterialsSetup_Init != null)
                _txtWIPEqpMaterialsSetup_Init.DataChanged += _txtWIPEqpMaterialsSetup_Init_DataChanged;            
            _ndoEquipmentField.DataChanged += Equipment_DataChanged;
            _txtMaterialLotField.DataChanged += _txtMaterialLotField_DataChanged;
        }

        //--------------------------------------------------
        //
        //--------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Get Computername
            _txtComputerNameField.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
          
            // Hide button
            if (_btnWIPEqpMaterialsSetup_AddMaterialPartButton != null)
                _btnWIPEqpMaterialsSetup_AddMaterialPartButton.Hidden = true;

            // Add event to catch return key on Material Part control (both on Name and Revision textbox)
            _rdoMaterialPartField.TextEditControl.Attributes.Add("onkeydown", "return SS_EqpMaterialsSetup_MaterialPart_SetFocusOnScan(event, 'MaterialPartName');");
            _rdoMaterialPartField.RevisionControl.Attributes.Add("onkeydown", "return SS_EqpMaterialsSetup_MaterialPart_SetFocusOnScan(event, 'MaterialPartRev');");
            CommonWebControls.CheckBox _chkROR = _rdoMaterialPartField.RevisionControl.FindControl("IsRevisionCheckBox") as CommonWebControls.CheckBox;
            _chkROR.Attributes.Add("onkeydown", "return SS_EqpMaterialsSetup_MaterialPart_SetFocusOnScan(event, 'MaterialPartROR');");

            if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                CollectSelectedValue();

            if (!Page.IsPostBack)
            {                               
                Personalization.UIAction[] actUIActions = Page.ActionDispatcher.PageActions();
                foreach (Personalization.UIAction act in actUIActions)
                {
                    if (act.Name.ToUpper() == "RELOAD")
                    {
                        act.IsHidden = _bIsPopup;
                        act.IsDisabled = _bIsPopup;
                    }

                    if (act.Name.ToUpper() == "CLOSE")
                    {
                        act.IsHidden = !_bIsPopup;
                        act.IsDisabled = !_bIsPopup;
                    }
                }
                SEMI.AppCode.UIUtility.MaximizePopUp(this);
            }

            if (_bIsPopup)
            {
                if (_btnSubmit != null && _btnReset != null)
                {
                    _btnSubmit.Visible = false;
                    _btnSubmit.Enabled = false;
                    _btnReset.Visible = false;
                    _btnReset.Enabled = false;
                }
            }
        }

        //--------------------------------------------------
        //
        //--------------------------------------------------
        void _txtMaterialLotField_DataChanged(object sender, EventArgs e)
        {
            if (!_txtMaterialLotField.IsEmpty)
                FetchData(const_sMaterialLot);
        }

        //--------------------------------------------------
        //
        //--------------------------------------------------
        void _txtWIPEqpMaterialsSetup_Init_DataChanged(object sender, EventArgs e)
        {
            if (!_ndoEquipmentField.IsEmpty)
                FetchData(const_sResource);
        }

        //--------------------------------------------------
        //
        //--------------------------------------------------
        private void CollectSelectedValue()
        {
            // Refresh Details datagrid value from Popup selection page
            if (SEMI.AppCode.UIUtility.IsPopupClose(this))
            {
                var sAttrVal = Page.PortalContext.DataContract.GetValueByName<string>("WIPEqpMaterialsSetup_ReturnedValueDM");
                var sAttrValRev = Page.PortalContext.DataContract.GetValueByName<string>("WIPEqpMaterialsSetup_ReturnedRevisionDM");
                var sRowId = Page.PortalContext.DataContract.GetValueByName<string>("WIPEqpMaterialsSetup_SelectedRowIdDM");

                if (!string.IsNullOrEmpty(sRowId) && (_txtMaterialLotField.IsEmpty))
                {
                    // Datagrid button is selected. Update Material Part on datagrid.
                    int iRowId = int.Parse(sRowId);
                    var data = _gridDetailsField.Data as EquipmentMaterialsDetails[];
                    data[iRowId].MaterialPart = new RevisionedObjectRef();
                    data[iRowId].MaterialPart.Name = sAttrVal;
                    data[iRowId].MaterialPart.Revision = sAttrValRev;
                }
                else
                {
                    // Page button is selected. Collect value from popup page.
                    _rdoMaterialPartField.Data = sAttrVal;
                    _rdoMaterialPartField.RevisionValue = sAttrValRev;
                    MaterialPart_DataChanged(null, null);
                }
            }
        }

        //--------------------------------------------------
        //
        //--------------------------------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "EqpMaterialsSetup_Reload")
            {
                Page.ShopfloorReset(sender, e);
                ResetFields();
            }

            if (action != null && action.Parameters == "WIPEqpMaterialsSetup_Reload")
            {
                ResetFields();
                FetchData(const_sResource);
            }
        }

        //--------------------------------------------------
        //
        //--------------------------------------------------
        public void Equipment_DataChanged(object sender, EventArgs e)
        {
            if (!_ndoEquipmentField.IsEmpty)
            {
                ResetFields();
                FetchData(const_sResource);
            }
        }

        //--------------------------------------------------
        //
        //--------------------------------------------------
        private void ResetFields()
        {
            _gridDetailsField.ClearData();
            _gridMaterialsRequiredField.ClearData();
			if (_gridLotInProcessField != null)
				_gridLotInProcessField.ClearData();
            _txtMaterialLotField.ClearData();
            _rdoMaterialPartField.ClearData();

            _rdoMaterialPartField.Enabled = false;
            _txtMaterialLotField.Enabled = true;

            _chkAutoCombineInventory.ClearData();
            _chkAutoReturnMaterial.ClearData();            

            // disable Material Part and Product Available popup buttons
            _btnWIPEqpMaterialsSetup_MaterialPartButton.Enabled = false;
            _btnWIPEqpMaterialsSetup_ProductButton.Enabled = false;

            Page.SetFocus(_txtMaterialLotField);
        }

        //--------------------------------------------------
        //
        //--------------------------------------------------
        private void FetchData(string sEventName)
        {
            try
            {
                bool bExecute = false;
                if (_chkIsActiveField != null)
                    bExecute = (_chkIsActiveField.CheckControl.Checked);

                if (bExecute)
                {
                    // Prepare service
                    var fs = FrameworkManagerUtil.GetFrameworkSession();
                    EquipmentMaterialsSetup oServiceData = new EquipmentMaterialsSetup();
                    EquipmentMaterialsSetup_Info oServiceInfo = new EquipmentMaterialsSetup_Info();
                    EquipmentMaterialsSetupService oService = new EquipmentMaterialsSetupService(fs.CurrentUserProfile);
                    EquipmentMaterialsSetup_Request oRequest = new EquipmentMaterialsSetup_Request();
                    EquipmentMaterialsSetup_Result oResponseData = new EquipmentMaterialsSetup_Result();

                    // Prepare the request
                    if (sEventName == const_sMaterialLot)
                    {
                        // Set Data Input
                        oServiceData.MaterialLotName = _txtMaterialLotField.Data.ToString();

                        // Request needed information for MaterialLotDetails
                        oServiceInfo.MaterialLotDetails = new EquipmentMaterialsDetails_Info();
                        oServiceInfo.MaterialLotDetails.MaterialLotName = FieldInfoUtil.RequestValue();
                        oServiceInfo.MaterialLotDetails.MaterialPart = FieldInfoUtil.RequestValue();
                        oServiceInfo.MaterialLotDetails.ReferenceDesignator = FieldInfoUtil.RequestValue();
                        oServiceInfo.MaterialLotDetails.Qty = FieldInfoUtil.RequestValue();
                        oServiceInfo.MaterialLotDetails.Qty2 = FieldInfoUtil.RequestValue();
                        oServiceInfo.MaterialLotDetails.InvoiceNumber = FieldInfoUtil.RequestValue();
                        oServiceInfo.MaterialLotDetails.Vendor = FieldInfoUtil.RequestValue();
                        oServiceInfo.MaterialLotDetails.VendorLotNumber = FieldInfoUtil.RequestValue();
                        oServiceInfo.MaterialLotDetails.ManufacturerExpiryDate = FieldInfoUtil.RequestValue();
                        oServiceInfo.MaterialLotDetails.WithdrawalTimestamp = FieldInfoUtil.RequestValue();
                        oServiceInfo.MaterialLotDetails.ThawingTimestamp = FieldInfoUtil.RequestValue();
                        oServiceInfo.MaterialLotDetails.ExpiryTimestamp = FieldInfoUtil.RequestValue();
                    }
                    else if (sEventName == const_sResource)
                    {
                        // Set Data Input
                        oServiceData.Resource = new NamedObjectRef();
                        oServiceData.Resource = _ndoEquipmentField.Data as NamedObjectRef;

                        if (!_ndoProcessTypeField.IsEmpty)
                        {
                            oServiceData.ProcessType = new NamedObjectRef();
                            oServiceData.ProcessType = _ndoProcessTypeField.Data as NamedObjectRef;
                        }

                        // Request needed information for EquipmentMaterials object
                        oServiceInfo.EquipmentMaterials = new EquipmentMaterials_Info();
                        oServiceInfo.EquipmentMaterials.MaterialLotName = FieldInfoUtil.RequestValue();
                        oServiceInfo.EquipmentMaterials.MaterialPart = FieldInfoUtil.RequestValue();
                        oServiceInfo.EquipmentMaterials.ReferenceDesignator = FieldInfoUtil.RequestValue();
                        oServiceInfo.EquipmentMaterials.Qty = FieldInfoUtil.RequestValue();
                        oServiceInfo.EquipmentMaterials.Qty2 = FieldInfoUtil.RequestValue();
                        oServiceInfo.EquipmentMaterials.InvoiceNumber = FieldInfoUtil.RequestValue();
                        oServiceInfo.EquipmentMaterials.Vendor = FieldInfoUtil.RequestValue();
                        oServiceInfo.EquipmentMaterials.VendorLotNumber = FieldInfoUtil.RequestValue();
                        oServiceInfo.EquipmentMaterials.ManufacturerExpiryDate = FieldInfoUtil.RequestValue();
                        oServiceInfo.EquipmentMaterials.WithdrawalTimestamp = FieldInfoUtil.RequestValue();
                        oServiceInfo.EquipmentMaterials.ThawingTimestamp = FieldInfoUtil.RequestValue();
                        oServiceInfo.EquipmentMaterials.ExpiryTimestamp = FieldInfoUtil.RequestValue();

                        oServiceInfo.LotsInProcess = new EquipmentMaterialsDetails_Info();
                        oServiceInfo.LotsInProcess.Container = FieldInfoUtil.RequestValue();
                        oServiceInfo.LotsInProcess.Product = FieldInfoUtil.RequestValue();
                        oServiceInfo.LotsInProcess.Qty = FieldInfoUtil.RequestValue();
                        oServiceInfo.LotsInProcess.Qty2 = FieldInfoUtil.RequestValue();
                    }
                    // Request the data
                    oRequest.Info = oServiceInfo;
                    OM.ResultStatus oResultStatus = oService.GetEnvironment(oServiceData, oRequest, out oResponseData);
                    if (oResultStatus.IsSuccess)
                    {
                        if (sEventName == const_sMaterialLot)
                        {
                            bool bMaterialLotExists = false;
                            if (oResponseData.Value.MaterialLotDetails != null)
                            {
                                foreach (EquipmentMaterialsDetails resultDetail in oResponseData.Value.MaterialLotDetails)
                                {
                                    if (resultDetail.MaterialLotName != null)
                                    {
                                        bMaterialLotExists = true;
                                        AddDetailsRow(resultDetail);
                                    }
                                }

                                if (bMaterialLotExists)
                                {
                                    // Clear the field
                                    _txtMaterialLotField.ClearData();
                                    _txtMaterialLotField.Focus();
                                }
                                else
                                {
                                    // Enable the Material Part field if the material lot name is not serialized
                                    //_txtMaterialLotField.Enabled = false;
                                    _rdoMaterialPartField.Enabled = true;
                                    _txtMaterialLotField.Enabled = false;
                                    _rdoMaterialPartField.Focus();

                                    // Enable Material Part and Product Available popup buttons
                                    _btnWIPEqpMaterialsSetup_MaterialPartButton.Enabled = true;
                                    _btnWIPEqpMaterialsSetup_ProductButton.Enabled = true;
                                }
                            }
                        }
                        else if (sEventName == const_sResource)
                        {
                            // Display loaded materials on Equipment
                            if (oResponseData.Value.EquipmentMaterials != null)
                            {
                                EquipmentMaterialsDetails[] objEqpMaterialsDetails = new EquipmentMaterialsDetails[oResponseData.Value.EquipmentMaterials.Length];
                                int iCount = 0;
                                foreach (EquipmentMaterials resultDetail in oResponseData.Value.EquipmentMaterials)
                                {
                                    objEqpMaterialsDetails[iCount] = new EquipmentMaterialsDetails();
                                    objEqpMaterialsDetails[iCount].MaterialLotName = resultDetail.MaterialLotName;
                                    objEqpMaterialsDetails[iCount].MaterialPart = resultDetail.MaterialPart;
                                    objEqpMaterialsDetails[iCount].Qty = resultDetail.Qty;
                                    objEqpMaterialsDetails[iCount].Qty2 = resultDetail.Qty2;
                                    objEqpMaterialsDetails[iCount].ReferenceDesignator = resultDetail.ReferenceDesignator;
                                    objEqpMaterialsDetails[iCount].ExpiryTimestamp = resultDetail.ExpiryTimestamp;
                                    objEqpMaterialsDetails[iCount].InvoiceNumber = resultDetail.InvoiceNumber;
                                    objEqpMaterialsDetails[iCount].ManufacturerExpiryDate = resultDetail.ManufacturerExpiryDate;
                                    objEqpMaterialsDetails[iCount].ThawingTimestamp = resultDetail.ThawingTimestamp;
                                    objEqpMaterialsDetails[iCount].Vendor = resultDetail.Vendor;
                                    objEqpMaterialsDetails[iCount].VendorLotNumber = resultDetail.VendorLotNumber;
                                    objEqpMaterialsDetails[iCount].WithdrawalTimestamp = resultDetail.WithdrawalTimestamp;

                                    iCount++;
                                }

                                // Bind response data to datagrid
                                (_gridDetailsField.GridContext as BoundContext).Data = objEqpMaterialsDetails.ToArray();
                                _gridDetailsField.BoundContext.LoadData();
                                CamstarWebControl.SetRenderToClient(_gridDetailsField);
                            }

                            if (oResponseData.Value.LotsInProcess != null)
                            {
                                EquipmentMaterialsDetails[] objLotInProcess = new EquipmentMaterialsDetails[oResponseData.Value.LotsInProcess.Length];
                                int i = 0;
                                foreach (EquipmentMaterialsDetails result in oResponseData.Value.LotsInProcess)
                                {
                                    objLotInProcess[i] = new EquipmentMaterialsDetails();
                                    objLotInProcess[i].Container = new ContainerRef(result.Container.Name);
                                    objLotInProcess[i].Product = result.Product;
                                    objLotInProcess[i].Qty = result.Qty;
                                    objLotInProcess[i].Qty2 = result.Qty2;

                                    i++;
                                }

                                if (_gridLotInProcessField != null)
                                {
                                    (_gridLotInProcessField.GridContext as BoundContext).Data = objLotInProcess.ToArray();
                                    _gridLotInProcessField.BoundContext.LoadData();
                                    CamstarWebControl.SetRenderToClient(_gridLotInProcessField);
                                }
                            }
                            // Display Required Materials
                            FetchRequiredMaterials();
                        }
                    }
                    else
                    {
                        ResetFields();
                        DisplayMessage(oResultStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "(): " + ex.Message, false));
            }
        }

        //--------------------------------------------------
        //
        //--------------------------------------------------
        public void MaterialLot_DataChanged(object sender, EventArgs e)
        {
            if (!_txtMaterialLotField.IsEmpty)
            {
                _rdoMaterialPartField.ClearData();
                _rdoMaterialPartField.Enabled = false;

                // Check whether the lot has already been added
                int gridCount = _gridDetailsField.BoundContext.GetTotalRows();
                for (int i = 0; i < gridCount; i++)
                {
                    string sRowId = i.ToString().PadLeft(6, '0');
                    string sTempString = _gridDetailsField.GridContext.GetCell(sRowId, "MaterialLotName").ToString();
                    if (sTempString.ToUpper() == _txtMaterialLotField.Data.ToString().ToUpper())
                    {
                        _txtMaterialLotField.ClearData();
                        _txtMaterialLotField.Focus();
                        return;

                    }
                }

                // Fetch needed information
                FetchData(const_sMaterialLot);
            }
        }

        //--------------------------------------------------
        //
        //--------------------------------------------------
        public void MaterialPart_DataChanged(object sender, EventArgs e)
        {
            if ((_txtMaterialLotField.Data != null) && (_rdoMaterialPartField.Text != null))
            {
                if (_txtMaterialLotField.Data.ToString() != "")
                {
                    // Add new row
                    EquipmentMaterialsDetails objAddRow = new EquipmentMaterialsDetails();
                    objAddRow.MaterialLotName = _txtMaterialLotField.Data.ToString();
                    objAddRow.MaterialPart = new RevisionedObjectRef();
                    if (_rdoMaterialPartField.Data != null)
                        objAddRow.MaterialPart = _rdoMaterialPartField.Data as RevisionedObjectRef;
                    else
                    {
                        objAddRow.MaterialPart.Name = _rdoMaterialPartField.Text;
                        objAddRow.MaterialPart.RevisionOfRecord = true;
                    }
                    objAddRow.Qty = 0;
                    objAddRow.Qty2 = 0;
                    AddDetailsRow(objAddRow);

                    // Set the controls
                    _rdoMaterialPartField.ClearData();
                    _rdoMaterialPartField.Enabled = false;
                    _btnWIPEqpMaterialsSetup_MaterialPartButton.Enabled = false;
                    _btnWIPEqpMaterialsSetup_ProductButton.Enabled = false;
                    _txtMaterialLotField.Focus();
                    _txtMaterialLotField.ClearData();
                    _txtMaterialLotField.Enabled = true;
                }
            }
        }

        //--------------------------------------------------
        //
        //--------------------------------------------------
        public override void GetInputData(Service serviceData)
        {
            try
            {
                bool bExecute = false;

                var a = Page.DataContract.DataMembers.Any(x => x.Name == "WIPMain_PrimaryServiceType_DM");

                if (_chkIsActiveField != null || !Page.DataContract.DataMembers.Any(x => x.Name == "WIPMain_PrimaryServiceType_DM"))
                    bExecute = _chkIsActiveField.CheckControl.Checked || !Page.DataContract.DataMembers.Any(x => x.Name == "WIPMain_PrimaryServiceType_DM");

                if (bExecute)
                {
                    base.GetInputData(serviceData);
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }

        //--------------------------------------------------
        //
        //--------------------------------------------------
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            bool bExecute = false;
            if (_chkIsActiveField != null)
                bExecute = _chkIsActiveField.CheckControl.Checked;

            if (bExecute)
            {
                base.PostExecute(status, serviceData);

                if (status.IsSuccess)
                {
                    if (serviceData is OM.EquipmentMaterialsSetup)
                        Equipment_DataChanged(null, null);
                }
            }
        }

        //--------------------------------------------------
        //
        //--------------------------------------------------
        private void AddDetailsRow(EquipmentMaterialsDetails objMaterialsDetail)
        {
            try
            {
                if (!_txtMaterialLotField.IsEmpty)
                {                    
                    EquipmentMaterialsDetails[] oExistingList = (_gridDetailsField.GridContext as BoundContext).Data as EquipmentMaterialsDetails[];
                    List<EquipmentMaterialsDetails> oNewList = new List<EquipmentMaterialsDetails>();            

                    if (oExistingList == null)
                    oExistingList = new EquipmentMaterialsDetails[0];

                    foreach (EquipmentMaterialsDetails oRow in oExistingList)
                    {
                        EquipmentMaterialsDetails oCurrentRow = new EquipmentMaterialsDetails();
                        oCurrentRow.MaterialLotName = oRow.MaterialLotName;
                        oCurrentRow.MaterialPart = oRow.MaterialPart;
                        oCurrentRow.Qty = oRow.Qty;
                        oCurrentRow.Qty2 = oRow.Qty2;
                        oCurrentRow.ReferenceDesignator = oRow.ReferenceDesignator;
                        oCurrentRow.ExpiryTimestamp = oRow.ExpiryTimestamp;
                        oCurrentRow.InvoiceNumber = oRow.InvoiceNumber;
                        oCurrentRow.ManufacturerExpiryDate = oRow.ManufacturerExpiryDate;
                        oCurrentRow.ThawingTimestamp = oRow.ThawingTimestamp;
                        oCurrentRow.Vendor = oRow.Vendor;
                        oCurrentRow.VendorLotNumber = oRow.VendorLotNumber;
                        oCurrentRow.WithdrawalTimestamp = oRow.WithdrawalTimestamp;
                        oNewList.Add(oCurrentRow);
                    }

                    EquipmentMaterialsDetails oNewRow = new EquipmentMaterialsDetails();
                    oNewRow.MaterialLotName = objMaterialsDetail.MaterialLotName;
                    oNewRow.MaterialPart = objMaterialsDetail.MaterialPart;
                    oNewRow.Qty = objMaterialsDetail.Qty;
                    oNewRow.Qty2 = objMaterialsDetail.Qty2;
                    oNewRow.ReferenceDesignator = objMaterialsDetail.ReferenceDesignator;
                    oNewRow.ExpiryTimestamp = objMaterialsDetail.ExpiryTimestamp;
                    oNewRow.InvoiceNumber = objMaterialsDetail.InvoiceNumber;
                    oNewRow.ManufacturerExpiryDate = objMaterialsDetail.ManufacturerExpiryDate;
                    oNewRow.ThawingTimestamp = objMaterialsDetail.ThawingTimestamp;
                    oNewRow.Vendor = objMaterialsDetail.Vendor;
                    oNewRow.VendorLotNumber = objMaterialsDetail.VendorLotNumber;
                    oNewRow.WithdrawalTimestamp = objMaterialsDetail.WithdrawalTimestamp;
                    oNewList.Add(oNewRow);

                    (_gridDetailsField.GridContext as BoundContext).Data = oNewList.ToArray();
                    _gridDetailsField.BoundContext.LoadData();
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "() : " + Ex.Message, false));
            }
        }

        //--------------------------------------------------
        //
        //--------------------------------------------------
        private void FetchRequiredMaterials()
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            WIPMain oServiceData = new WIPMain();
            WIPMain_Info oServiceInfo = new WIPMain_Info();
            WIPMainService oService = new WIPMainService(fs.CurrentUserProfile);
            WIPMain_Request oRequest = new WIPMain_Request();
            WIPMain_Result oResponseData = new WIPMain_Result();

            if (!_ContainerField.IsEmpty)
            {
                // Prepare the request
                oServiceData.Container = new ContainerRef();
                oServiceData.Container = _ContainerField.Data as ContainerRef;

                if (!_ndoProcessTypeField.IsEmpty)
                {
                    oServiceData.ProcessType = new NamedObjectRef();
                    oServiceData.ProcessType = _ndoProcessTypeField.Data as NamedObjectRef;
                }

                oServiceInfo.MaterialsRequired = new ModifyMaterialsDetails_Info();
                oServiceInfo.MaterialsRequired.MaterialPart = FieldInfoUtil.RequestValue();
                oServiceInfo.MaterialsRequired.ConsumeFactor = FieldInfoUtil.RequestValue();
                oServiceInfo.MaterialsRequired.AlternateMaterialsCount = FieldInfoUtil.RequestValue();
                oServiceInfo.MaterialsRequired.AlternateMaterials = new ModifyMaterialsDetails_Info();
                oServiceInfo.MaterialsRequired.AlternateMaterials.MaterialPart = FieldInfoUtil.RequestValue();

                // Request the data
                oRequest.Info = oServiceInfo;
                OM.ResultStatus oResultStatus = oService.GetEnvironment(oServiceData, oRequest, out oResponseData);
                if (oResultStatus.IsSuccess)
                {
                    if ((oResponseData.Value.MaterialsRequired != null))
                    {
                        // Bind response data to datagrid
                        (_gridMaterialsRequiredField.GridContext as BoundContext).Data = oResponseData.Value.MaterialsRequired.ToArray();
                        _gridMaterialsRequiredField.BoundContext.LoadData();
                        CamstarWebControl.SetRenderToClient(_gridMaterialsRequiredField);
                    }
                }
                else
                {
                    throw new Exception(oResultStatus.ExceptionData.ToString());
                }
            }
        }

        #endregion
    }
}



