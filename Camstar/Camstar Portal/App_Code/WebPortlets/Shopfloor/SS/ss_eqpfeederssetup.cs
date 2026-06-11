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
using SEMI.AppCode;


namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    /// Summary description for WIPEqpMaterialsSetup
    /// </summary>
    public class EqpFeedersSetup : MatrixWebPart
    {
        #region Properties

		CWC.NamedObject _ndoEquipmentField { get { return Page.FindCamstarControl("EquipmentMaterialsSetup_Equipment") as CWC.NamedObject; } }
		CWC.NamedObject _ndoFromFeederField { get { return Page.FindCamstarControl("EquipmentMaterialsSetup_ss_FromFeeder") as CWC.NamedObject; } }
		CWC.NamedObject _ndoFromFeederSlotField { get { return Page.FindCamstarControl("EquipmentMaterialsSetup_ss_FromFeederSlot") as CWC.NamedObject; } }
		CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("EqpMaterialsSetup_ComputerName") as CWC.TextBox; } }
		JQDataGrid _gridFeederMaterialsDetailsField { get { return Page.FindCamstarControl("EquipmentMaterialsSetup_ss_EqpFeederMaterialsDetails") as JQDataGrid; } }
		JQDataGrid _gridDetailsField { get { return Page.FindCamstarControl("EqpMaterialsSetup_Details") as JQDataGrid; } }
		JQDataGrid _gridLotInProcessField { get { return Page.FindCamstarControl("EquipmentMaterialsSetup_LotsInProcess") as JQDataGrid; } }
		JQDataGrid _gridFeederBanksField { get { return Page.FindCamstarControl("EquipmentMaterialsSetup_ss_FeederBanks") as JQDataGrid; } }
		CWC.Button _btnAddFeederButton { get { return Page.FindCamstarControl("AddFeederButton") as CWC.Button; } }
		CWC.TextBox _txtFromFeederBankField { get { return Page.FindCamstarControl("EquipmentMaterialsSetup_ss_FromFeederBank") as CWC.TextBox; } }


		

        #endregion

     

        #region Functions
        //--------------------------------------------------
        //
        //--------------------------------------------------
        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);       
            _ndoEquipmentField.DataChanged += Equipment_DataChanged;
			_btnAddFeederButton.Click += _btnAddFeederButton_Click;
        }

		void _btnAddFeederButton_Click(object sender, EventArgs e)
		{
			AddDetailsRow();
		}

        //--------------------------------------------------
        //
        //--------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Get Computername
            _txtComputerNameField.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
			_txtFromFeederBankField.DataChanged += _txtFromFeederBankField_DataChanged;
        }

		void _txtFromFeederBankField_DataChanged(object sender, EventArgs e)
		{
			if (_txtFromFeederBankField.Data != null)
			{
				NamedObjectRef[] ndoFeederbanks = new NamedObjectRef[_gridFeederBanksField.TotalRowCount + 1];
				if (_gridFeederBanksField.TotalRowCount > 0)
				{
					(_gridFeederBanksField.Data as NamedObjectRef[]).CopyTo(ndoFeederbanks, 0);
					ndoFeederbanks[_gridFeederBanksField.TotalRowCount] = new NamedObjectRef(_txtFromFeederBankField.Data.ToString());
				}
				else 
				{
					ndoFeederbanks[0] = new NamedObjectRef(_txtFromFeederBankField.Data.ToString()); 
				}

				_gridFeederBanksField.Data = ndoFeederbanks;
				_txtFromFeederBankField.ClearData();
				Page.SetFocus(_txtFromFeederBankField.ClientID);
			}
		}

        

        //--------------------------------------------------
        //
        //--------------------------------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "EqpFeederSetup_Reload")
            {
                Page.ShopfloorReset(sender, e);
                ResetFields();
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
                FetchData();
            }
        }

        //--------------------------------------------------
        //
        //--------------------------------------------------
        private void ResetFields()
        {
            _gridDetailsField.ClearData();
			_gridFeederMaterialsDetailsField.ClearData();
			_ndoFromFeederField.ClearData();
			_ndoFromFeederSlotField.ClearData();
			_gridFeederBanksField.ClearData();
			Page.SetFocus(_ndoFromFeederField);
        }

        //--------------------------------------------------
        //
        //--------------------------------------------------
        private void FetchData()
        {
            try
            {
                

                    // Prepare service
                    var fs = FrameworkManagerUtil.GetFrameworkSession();
                    EquipmentMaterialsSetup oServiceData = new EquipmentMaterialsSetup();
                    EquipmentMaterialsSetup_Info oServiceInfo = new EquipmentMaterialsSetup_Info();
                    EquipmentMaterialsSetupService oService = new EquipmentMaterialsSetupService(fs.CurrentUserProfile);
                    EquipmentMaterialsSetup_Request oRequest = new EquipmentMaterialsSetup_Request();
                    EquipmentMaterialsSetup_Result oResponseData = new EquipmentMaterialsSetup_Result();

                    // Prepare the request
                    
                        // Set Data Input
                        oServiceData.Resource = new NamedObjectRef();
                        oServiceData.Resource = _ndoEquipmentField.Data as NamedObjectRef;


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

						// Request needed information for EquipmentFeederMaterials object
						oServiceInfo.ss_EquipmentFeederMaterials = new ss_EquipmentFeederMaterials_Info();
						oServiceInfo.ss_EquipmentFeederMaterials.ss_FromFeeder = FieldInfoUtil.RequestValue();
						oServiceInfo.ss_EquipmentFeederMaterials.ss_FromFeederSlot = FieldInfoUtil.RequestValue();

						oServiceInfo.ss_CurrentFeederBanks = FieldInfoUtil.RequestValue();

						oServiceInfo.LotsInProcess = new EquipmentMaterialsDetails_Info();
						oServiceInfo.LotsInProcess.Container = FieldInfoUtil.RequestValue();
						oServiceInfo.LotsInProcess.Product = FieldInfoUtil.RequestValue();
						oServiceInfo.LotsInProcess.Qty = FieldInfoUtil.RequestValue();
						oServiceInfo.LotsInProcess.Qty2 = FieldInfoUtil.RequestValue();


                    // Request the data
                    oRequest.Info = oServiceInfo;
                    OM.ResultStatus oResultStatus = oService.GetEnvironment(oServiceData, oRequest, out oResponseData);
                    if (oResultStatus.IsSuccess)
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
						 if (oResponseData.Value.ss_EquipmentFeederMaterials != null)
                            {
							 
							    ss_EqpFeederMaterialsDetails[] objEqpFeederMaterialsDetails= new ss_EqpFeederMaterialsDetails[oResponseData.Value.ss_EquipmentFeederMaterials.Length];
                                int iCount = 0;
                                foreach (ss_EquipmentFeederMaterials resultDetail in oResponseData.Value.ss_EquipmentFeederMaterials)
                                {
                                    objEqpFeederMaterialsDetails[iCount] = new ss_EqpFeederMaterialsDetails();
                                    objEqpFeederMaterialsDetails[iCount].ss_MaterialLotName = resultDetail.ss_MaterialLotName;
                                    objEqpFeederMaterialsDetails[iCount].ss_MaterialPart = resultDetail.ss_MaterialPart;
                                    objEqpFeederMaterialsDetails[iCount].ss_Qty = resultDetail.ss_Qty;
                                    objEqpFeederMaterialsDetails[iCount].ss_Qty2 = resultDetail.ss_Qty2;
                                    objEqpFeederMaterialsDetails[iCount].ss_ReferenceDesignator = resultDetail.ss_ReferenceDesignator;
                                    objEqpFeederMaterialsDetails[iCount].ss_ExpiryTimestamp = resultDetail.ss_ExpiryTimestamp;
                                    objEqpFeederMaterialsDetails[iCount].ss_InvoiceNumber = resultDetail.ss_InvoiceNumber;
                                    objEqpFeederMaterialsDetails[iCount].ss_ManufacturerExpiryDate = resultDetail.ss_ManufacturerExpiryDate;
                                    objEqpFeederMaterialsDetails[iCount].ss_ThawingTimestamp = resultDetail.ss_ThawingTimestamp;
                                    objEqpFeederMaterialsDetails[iCount].ss_Vendor = resultDetail.ss_Vendor;
                                    objEqpFeederMaterialsDetails[iCount].ss_VendorLotNumber = resultDetail.ss_VendorLotNumber;
                                    objEqpFeederMaterialsDetails[iCount].ss_WithdrawalTimestamp = resultDetail.ss_WithdrawalTimestamp;
									objEqpFeederMaterialsDetails[iCount].ss_FromFeeder = resultDetail.ss_FromFeeder;
									objEqpFeederMaterialsDetails[iCount].ss_FromFeederSlot = resultDetail.ss_FromFeederSlot;

                                    iCount++;
                                }

                                // Bind response data to datagrid
                                (_gridFeederMaterialsDetailsField.GridContext as BoundContext).Data = objEqpFeederMaterialsDetails.ToArray();
                                _gridFeederMaterialsDetailsField.BoundContext.LoadData();
                                CamstarWebControl.SetRenderToClient(_gridFeederMaterialsDetailsField);
                            }
						 if (oResponseData.Value.ss_CurrentFeederBanks != null)
						 {
							 (_gridFeederBanksField.GridContext as BoundContext).Data = oResponseData.Value.ss_CurrentFeederBanks;
							 _gridFeederBanksField.BoundContext.LoadData();
							 CamstarWebControl.SetRenderToClient(_gridFeederBanksField);
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

							 (_gridLotInProcessField.GridContext as BoundContext).Data = objLotInProcess.ToArray();
							 _gridLotInProcessField.BoundContext.LoadData();
							 CamstarWebControl.SetRenderToClient(_gridLotInProcessField);
						 }
                            
                        
                    }
                    else
                    {
                        ResetFields();
                        DisplayMessage(oResultStatus);
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
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            
                base.PostExecute(status, serviceData);

                if (status.IsSuccess)
                {
                    if (serviceData is OM.EquipmentMaterialsSetup)
                        Equipment_DataChanged(null, null);
                }
            
        }

        //--------------------------------------------------
        //
        //--------------------------------------------------
        private void AddDetailsRow()
        {
            try
            {
				if (!_ndoFromFeederField.IsEmpty)
                {
					ss_EqpFeederMaterialsDetails[] oExistingList = (_gridFeederMaterialsDetailsField.GridContext as BoundContext).Data as ss_EqpFeederMaterialsDetails[];
					List<ss_EqpFeederMaterialsDetails> oNewList = new List<ss_EqpFeederMaterialsDetails>();            

                    if (oExistingList == null)
						oExistingList = new ss_EqpFeederMaterialsDetails[0];

					foreach (ss_EqpFeederMaterialsDetails oRow in oExistingList)
                    {
						ss_EqpFeederMaterialsDetails oCurrentRow = new ss_EqpFeederMaterialsDetails();
						oCurrentRow.ss_FromFeeder = oRow.ss_FromFeeder;
						oCurrentRow.ss_FromFeederSlot = oRow.ss_FromFeederSlot;
                        oNewList.Add(oCurrentRow);
                    }

					ss_EqpFeederMaterialsDetails oNewRow = new ss_EqpFeederMaterialsDetails();
					oNewRow.ss_FromFeeder = _ndoFromFeederField.Data != null ? new NamedObjectRef(_ndoFromFeederField.Data.ToString()) : null;
					oNewRow.ss_FromFeederSlot = _ndoFromFeederSlotField.Data != null ? new NamedObjectRef(_ndoFromFeederSlotField.Data.ToString()) : null;

                    oNewList.Add(oNewRow);

					(_gridFeederMaterialsDetailsField.GridContext as BoundContext).Data = oNewList.ToArray();
					_gridFeederMaterialsDetailsField.BoundContext.LoadData();
					CamstarWebControl.SetRenderToClient(_gridFeederMaterialsDetailsField);
					_ndoFromFeederField.ClearData();
					_ndoFromFeederSlotField.ClearData();
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "() : " + Ex.Message, false));
            }
        }

        

        #endregion
    }
}



