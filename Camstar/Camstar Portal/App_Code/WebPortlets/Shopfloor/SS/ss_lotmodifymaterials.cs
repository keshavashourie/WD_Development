/* Copyright 2021 Siemens */
using System;
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
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework.WebControls;


/// <summary>
/// The code for the move non standard virtual page.  Resolves the lot based on the 
/// selection id entered and populates the lot details grid.
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_LotModifyMaterials : scsShopfloorBase
    {
        #region Properties

        protected JQDataGrid _gridContainers { get { return Page.FindCamstarControl("LotGrid") as JQDataGrid; } }
        protected JQDataGrid _gridLotMaterials { get { return Page.FindCamstarControl("LotModifyMaterials_Materials") as JQDataGrid; } }
        private SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("LotModifyMaterials_SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }
        protected CWC.TextBox txtComputerName { get { return Page.FindCamstarControl("LotModifyMaterials_ComputerNameField") as CWC.TextBox; } }
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotModifyMaterials_SelectionIdField") as CWC.TextBox; } }
        protected ContainerList _ContainerList { get { return Page.FindCamstarControl("LotModifyMaterials_Container") as ContainerList; } }
        protected DropDownList _WaferScribeNumber { get { return Page.FindCamstarControl("LotModifyMaterials_WaferScribeNumber") as DropDownList; } }
        CWC.TextBox _txtMaterialLotField { get { return Page.FindCamstarControl("LotModifyMaterials_MaterialLotName") as CWC.TextBox; } }
        protected Button _LotAttributesBtn { get { return Page.FindCamstarControl("LotModifyBins_LotAttributesButton") as Button; } }
        protected Button _LotAttributesCmdBar { get { return Page.FindCamstarControl("LotModifyMaterialPopup") as Button; } }

        ContainerRef containerRef;
        bool isProcessTypeValue = false;
        int count = 0;

        #endregion

        #region DataChangeEvents

        /// <summary>
        /// DataChange event for the _WaferScribeNumber control
        /// </summary>
        public void WaferScribeNumber_DataChanged()
        {
            if (_WaferScribeNumber.DropDownControl.SelectedItem.Value == "" && _WaferScribeNumber.DropDownControl.Items.Count != 0)
                GetData(_txtSelectionId.TextControl.Text);
            else
                GetData(null);
        }

        /// <summary>
        /// This is an event that is fire when data is change in the SelectionId field is changed.
        /// </summary>
        public void SelectionIdField_DataChanged()
        {
            try
            {
                if (_txtSelectionId.Data != null)
                {
                    _ContainerList.ClearData();
                    _ContainerList.Data = new ContainerRef(_txtSelectionId.TextControl.Text);
                    SetGridData(_txtSelectionId.Data.ToString(), _gridContainers);
                    CamstarWebControl.SetRenderToClient(_gridContainers);
                    GetData(_txtSelectionId.Data.ToString());
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "(): " + Ex.Message, false));
            }
            finally
            {
                _txtSelectionId.Focus();
            }
        }

        #endregion

        #region Private Methods

        private void CollectSelectedValue()
        {
            // Refresh Details datagrid value from Popup selection page
            if (SEMI.AppCode.UIUtility.IsPopupClose(this))
            {
                var sAttrVal = Page.PortalContext.DataContract.GetValueByName<string>("LotModifyMaterials_ReturnedValueDM");
                var sAttrValRev = Page.PortalContext.DataContract.GetValueByName<string>("LotModifyMaterials_ReturnedRevisionDM");
                var sRowId = Page.PortalContext.DataContract.GetValueByName<string>("LotModifyMaterials_SelectedRowIdDM");

                if (!string.IsNullOrEmpty(sRowId))
                {
                    //Row Id for selecting the row
                    int iRowId = int.Parse(sRowId);

                    //Choose the type of data to insert
                    var dataModifyMaterialsDetails = _gridLotMaterials.Data as ModifyMaterialsDetails[];
                    if (dataModifyMaterialsDetails != null)
                    {
                        dataModifyMaterialsDetails[iRowId].MaterialPart = new RevisionedObjectRef();
                        dataModifyMaterialsDetails[iRowId].MaterialPart.Name = sAttrVal;
                        dataModifyMaterialsDetails[iRowId].MaterialPart.Revision = sAttrValRev;
                    }
                }
            }
        }

        /// <summary>
        /// Gets data for form based off of return container(s)
        /// </summary>
        /// <param name="lot">string lot name</param>
        private void GetData(string lot)
        {
            try
            {
                //Clear Display message
                DisplayMessage(new ResultStatus("", true));

                OM.LotModifyMaterials_Info oServiceInfo = new OM.LotModifyMaterials_Info();
                LotModifyMaterials_Result resultData = new LotModifyMaterials_Result();
                UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;
                LotModifyMaterialsService svc = new LotModifyMaterialsService(profile);
                LotModifyMaterials oServiceData = new LotModifyMaterials();
                LotModifyMaterials_Request reqData = new LotModifyMaterials_Request();

                if (lot != null)
                {
                    //Selection population

                    oServiceData.SelectionId = lot;
                    oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotWafers = new LotWafers_Info();
                    oServiceInfo.LotWafers.WaferScribeNumber = FieldInfoUtil.RequestValue();

                    oServiceInfo.LotMaterials = new LotMaterials_Info();
                    oServiceInfo.LotMaterials.RequestValue = true;

                }
                else
                {
                    oServiceData.Container = new ContainerRef(_ContainerList.Data.ToString());
                    oServiceData.WaferScribeNumber = _WaferScribeNumber.DropDownControl.SelectedItem.Value;

                    oServiceInfo.LotWafersMaterials = new LotWafersMaterials_Info();
                    oServiceInfo.LotWafersMaterials.RequestValue = true;
                }
                reqData.Info = oServiceInfo;
                ResultStatus results = svc.ResolveSelectionId(oServiceData, reqData, out resultData);

                if (results.IsSuccess)
                {
                    //clearData(false);

                    DisplayValues(resultData.Value);

                    ///Selection Textbox
                    if (resultData.Value.SelectionContainer != null)
                        _txtSelectionId.TextControl.Text = resultData.Value.SelectionContainer.Name;

                    if (lot != null)
                    {
                        _WaferScribeNumber.DropDownControl.Items.Clear();
                        _WaferScribeNumber.ClearData();
                        _WaferScribeNumber.DropDownControl.ClearSelection();
                        _WaferScribeNumber.DropDownControl.ClearSelectedItem();

                        //WaferScribeNumber population						
                        if (resultData.Value.LotWafers != null)
                        {
                            _WaferScribeNumber.DropDownControl.Items.Add("");
                            foreach (LotWafers item in resultData.Value.LotWafers)
                            {
                                _WaferScribeNumber.DropDownControl.Items.Add(item.WaferScribeNumber.Value);
                            }
                        }
                    }

                    //populate the Materials grid
                    if (_WaferScribeNumber.DropDownControl.SelectedValue == "")
                    {
                        _gridLotMaterials.ClearData();
                        if (resultData.Value.LotMaterials != null)
                        {
                            ///Gets Lot Materials array
                            LotMaterials[] allDetails = resultData.Value.LotMaterials.ToArray();

                            if (allDetails != null)
                            {
                                int waferIndex = 0;
                                ModifyMaterialsDetails[] newDetails = new ModifyMaterialsDetails[allDetails.Length];

                                foreach (LotMaterials detail in allDetails)
                                {
                                    newDetails[waferIndex] = new ModifyMaterialsDetails();
                                    newDetails[waferIndex].ListItemAction = ListItemAction.Add;
                                    newDetails[waferIndex].MaterialLotName = detail.MaterialLotName;
                                    newDetails[waferIndex].MaterialPart = detail.MaterialPart;
                                    newDetails[waferIndex].QtyConsumed = detail.QtyConsumed;
                                    newDetails[waferIndex].TxnTimestamp = detail.TxnTimestamp;
                                    newDetails[waferIndex].Username = detail.Username;
                                    newDetails[waferIndex].ExpiryTimestamp = detail.ExpiryTimestamp;
                                    newDetails[waferIndex].InvoiceNumber = detail.InvoiceNumber;
                                    newDetails[waferIndex].ManufacturerExpiryDate = detail.ManufacturerExpiryDate;
                                    newDetails[waferIndex].ReferenceDesignator = detail.ReferenceDesignator;
                                    newDetails[waferIndex].ThawingTimestamp = detail.ThawingTimestamp;
                                    newDetails[waferIndex].VendorLotNumber = detail.VendorLotNumber;
                                    newDetails[waferIndex].WithdrawalTimestamp = detail.WithdrawalTimestamp;
                                    waferIndex++;
                                }

                                (_gridLotMaterials.GridContext as BoundContext).Data = newDetails; //resultData.Value.LotMaterials.ToArray()
                                _gridLotMaterials.GridContext.LoadData();
                                CamstarWebControl.SetRenderToClient(_gridLotMaterials);
                            }
                        }                       
                    }
                    else
                    {
                        _gridLotMaterials.ClearData();
                        //populate the LotWaferMaterials
                        if (resultData.Value.LotWafersMaterials != null)
                        {
                            ///Get Lot Wafer Material array
                            LotWafersMaterials[] allDetails = resultData.Value.LotWafersMaterials.ToArray();
                            if (allDetails != null)
                            {
                                int waferIndex = 0;
                                ModifyMaterialsDetails[] newDetails = new ModifyMaterialsDetails[allDetails.Length];

                                foreach (LotWafersMaterials detail in allDetails)
                                {
                                    newDetails[waferIndex] = new ModifyMaterialsDetails();
                                    newDetails[waferIndex].ListItemAction = ListItemAction.Add;
                                    newDetails[waferIndex].MaterialLotName = detail.MaterialLotName;
                                    newDetails[waferIndex].MaterialPart = detail.MaterialPart;
                                    newDetails[waferIndex].QtyConsumed = detail.QtyConsumed;
                                    newDetails[waferIndex].TxnTimestamp = detail.TxnTimestamp;
                                    newDetails[waferIndex].Username = detail.Username;
                                    newDetails[waferIndex].ExpiryTimestamp = detail.ExpiryTimestamp;
                                    newDetails[waferIndex].InvoiceNumber = detail.InvoiceNumber;
                                    newDetails[waferIndex].ManufacturerExpiryDate = detail.ManufacturerExpiryDate;
                                    newDetails[waferIndex].ReferenceDesignator = detail.ReferenceDesignator;
                                    newDetails[waferIndex].ThawingTimestamp = detail.ThawingTimestamp;
                                    newDetails[waferIndex].VendorLotNumber = detail.VendorLotNumber;
                                    newDetails[waferIndex].WithdrawalTimestamp = detail.WithdrawalTimestamp;
                                    waferIndex++;
                                }

                                (_gridLotMaterials.GridContext as BoundContext).Data = newDetails; //resultData.Value.LotWafersMaterials.ToArray()
                                _gridLotMaterials.GridContext.LoadData();
                                CamstarWebControl.SetRenderToClient(_gridLotMaterials);
                            }
                        }
                    }
                    if (lot != null)
                        _ContainerList.Data = new ContainerRef(lot);
                }
                else
                {
                    clearData(false);
                    DisplayMessage(results);
                }

            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message, false));
            }
        }

        /// <summary>
        /// Clears data from the form.
        /// </summary>
        private void clearData(bool Allvalues)
        {
            if (Allvalues)
            {
                Page.ClearValues();
                _gridContainers.ClearData();
            }
            _gridLotMaterials.ClearData();
            _WaferScribeNumber.DropDownControl.Items.Clear();
            _txtSelectionId.Focus();
        }

        /// <summary>
        /// Set data for the lot grid
        /// </summary>
        /// <param name="sContainersItem">Container to insert</param>
        /// <param name="_gridContainersTemp">grid host</param>
        private void SetGridData(string sContainersItem, JQDataGrid _gridContainersTemp)
        {
            DisplayMessage(new ResultStatus("", true));
            // check if lot not exist in the grid else add to the grid
            string strSelectedGridId = (_gridContainersTemp.GridContext as BoundContext).GetRowIdByCellValue("Lot", sContainersItem);

            if (string.IsNullOrEmpty(strSelectedGridId))
            {
                _gridContainersTemp.ClearData();
                //setting them to the grid
                SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "LotModifyMaterials", sContainersItem, false, ref _gridContainersTemp, "LotGrid", true);
            }
        }

        #endregion

        #region Page Events

        /// <summary>
        /// This event is fired when the submit action button is clicked.
        /// </summary>
        /// <param name="serviceData"></param>
        public override void GetInputData(Service serviceData)
        {


            ModifyMaterialsDetails[] allDetails = (_gridLotMaterials.GridContext as BoundContext).Data as ModifyMaterialsDetails[];

            if (allDetails != null)
            {
                int waferIndex = 0;
                ModifyMaterialsDetails[] newDetails = new ModifyMaterialsDetails[allDetails.Length];

                foreach (ModifyMaterialsDetails detail in allDetails)
                {
                    newDetails[waferIndex] = new ModifyMaterialsDetails();
                    newDetails[waferIndex].ListItemAction = ListItemAction.Add;
                    newDetails[waferIndex].MaterialLotName = detail.MaterialLotName;
                    newDetails[waferIndex].MaterialPart = detail.MaterialPart;
                    newDetails[waferIndex].QtyConsumed = detail.QtyConsumed;
                    newDetails[waferIndex].TxnTimestamp = detail.TxnTimestamp;
                    newDetails[waferIndex].Username = detail.Username;
                    newDetails[waferIndex].ExpiryTimestamp = detail.ExpiryTimestamp;
                    newDetails[waferIndex].InvoiceNumber = detail.InvoiceNumber;
                    newDetails[waferIndex].ManufacturerExpiryDate = detail.ManufacturerExpiryDate;
                    newDetails[waferIndex].ReferenceDesignator = detail.ReferenceDesignator;
                    newDetails[waferIndex].ThawingTimestamp = detail.ThawingTimestamp;
                    newDetails[waferIndex].VendorLotNumber = detail.VendorLotNumber;
                    newDetails[waferIndex].WithdrawalTimestamp = detail.WithdrawalTimestamp;
                    waferIndex++;
                }
                (serviceData as LotModifyMaterials).Materials = newDetails;
            }
            base.GetInputData(serviceData);
        }

        /// <summary>
        /// On page load
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //_txtSelectionId.DataChanged += new EventHandler(_txtSelectionId_DataChanged);
            txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);


            if (IsHorizon())
            {
                _LotAttributesBtn.Visible = false;
                _LotAttributesCmdBar.Visible = true;
            }
            else
            {
                _LotAttributesBtn.Visible = true;
                _LotAttributesCmdBar.Visible = false;
            }

            if (Page.IsPostBack)
                // Check if it is a pop up close, get the return result
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {

                    if (_envSelectedLots != null)
                    {
                        // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                        if (Page.DataContract.GetValueByName("LotModifyMaterials_SelectedLotsListDM") != null)
                        {
                            //gets the list of returned lot from the popup form
                            _envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("LotModifyMaterials_SelectedLotsListDM") as string[];
                        }

                        if (_envSelectedLots.SS_ContainersList != null)
                        {
                            JQDataGrid _gridContainersTemp = Page.FindCamstarControl("LotGrid") as JQDataGrid;

                            SetGridData(_envSelectedLots.SS_ContainersList[0].ToString(), _gridContainersTemp);

                            GetData(_envSelectedLots.SS_ContainersList[0].ToString());

                            _txtSelectionId.TextControl.Text = _envSelectedLots.SS_ContainersList[0].ToString();

                            CamstarWebControl.SetRenderToClient(_gridContainersTemp);
                            //nullify the containers list
                            _envSelectedLots.SS_ContainersList = null;
                        }
                    }
                    CollectSelectedValue();
                }
        }

        /// <summary>
        /// Resets the page value to default
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {

            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Reset")
            {
                Page.ShopfloorReset(sender, e);
                _txtSelectionId.Focus();
                clearData(true);
            }
            base.WebPartCustomAction(sender, e);
        }

        /// <summary>
        /// Excutes after the submit button is clicked
        /// </summary>
        /// <param name="status"></param>
        /// <param name="serviceData"></param>
        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                clearData(true);
            }
        }

        #endregion
    }
}



