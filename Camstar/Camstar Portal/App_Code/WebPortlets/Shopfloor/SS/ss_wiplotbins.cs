/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Windows;
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
using PERS = Camstar.WebPortal.Personalization;
using System.Collections;

/// <summary>
/// Summary description for SS_WIPLotBins
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_WIPLotBins : MatrixWebPart
    {
        #region Properties

        // ContainerLists
        CWC.ContainerList _ctlContainerField { get { return Page.FindCamstarControl("LotBinsTxn_Container") as CWC.ContainerList; } }
        // NamedObjects
        CWC.NamedObject _ndoProcessTypeField { get { return Page.FindCamstarControl("LotBinsTxn_ProcessType") as CWC.NamedObject; } }
        CWC.NamedObject _ndoEquipmentField { get { return Page.FindCamstarControl("LotBinsTxn_Equipment") as CWC.NamedObject; } }
        CWC.NamedObject _ndoEmployeeField { get { return Page.FindCamstarControl("LotBinsTxn_Employee") as CWC.NamedObject; } }
        // RevisionedObject
        CWC.RevisionedObject _rdoSubProductField { get { return Page.FindCamstarControl("LotBinsTxn_SubProduct") as CWC.RevisionedObject; } }
        // TextBoxs
        CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("LotBinsTxn_ComputerName") as CWC.TextBox; } }
        CWC.TextBox _txtMaxBinsQtyField { get { return Page.FindCamstarControl("LotBinsTxn_MaxBinsQty") as CWC.TextBox; } }
        CWC.TextBox _txtPrevWaferScribeNumber { get { return Page.FindCamstarControl("WIPLotBins_PrevWaferScribeNumber") as CWC.TextBox; } }
        // JQDataGirds
        JQDataGrid _gridLotBinsTxnDetailsFields { get { return Page.FindCamstarControl("LotBinsTxn_Details") as JQDataGrid; } }
        JQDataGrid _gridWaferBinDetails { get { return Page.FindCamstarControl("WIPLotBins_WaferBinDetails") as JQDataGrid; } }
        // Buttons
        CWC.Button _btnSubmit { get { return Page.FindCamstarControl("WIPBinning_SubmitButton") as CWC.Button; } }
        CWC.Button _btnReset { get { return Page.FindCamstarControl("WIPBinning_ResetButton") as CWC.Button; } }
        // Dropdownlists
        CWC.DropDownList _ddlServiceTypeField { get { return Page.FindCamstarControl("WIPLotBins_ServiceType") as CWC.DropDownList; } }
        CWC.DropDownList _ddlWaferScribeNumber { get { return Page.FindCamstarControl("WIPLotBins_WaferScribeNumbers") as CWC.DropDownList; } }
        // Checkboxs
        CWC.CheckBox _chkIsActiveField { get { return Page.FindCamstarControl("WIPLotBins_IsActive") as CWC.CheckBox; } }
        CWC.CheckBox _chkIsPopupField { get { return Page.FindCamstarControl("WIPLotBins_IsPopup") as CWC.CheckBox; } }
        CWC.CheckBox _chkIsWaferProcessing { get { return Page.FindCamstarControl("WIPLotBins_IsWaferProcessing") as CWC.CheckBox; } }

        CWC.TextBox _txtTotalBinQty { get { return Page.FindCamstarControl("TotalBinQty") as CWC.TextBox; } }
        CWC.TextBox _txtCurrentWaferBinQty { get { return Page.FindCamstarControl("CurrentWaferBinQty") as CWC.TextBox; } }
        CWC.TextBox _txtLotBinsInit { get { return Page.FindCamstarControl("LotBinsTxn_Init") as CWC.TextBox; } }
        protected bool _bIsPopup { get { return Page.IsAJAXFloatingFrame; } }

        protected const string kWafersBinViewState = "SS_WIPLotBins_WafersBinsViewStateVariable";

        #endregion

        //------------------------------------------------------
        //
        //------------------------------------------------------
        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);
            _txtLotBinsInit.DataChanged += _txtLotBinsInit_DataChanged;
            _ndoProcessTypeField.DataChanged += _ndoProcessTypeField_DataChanged;
            _ndoEquipmentField.DataChanged += _ndoEquipmentField_DataChanged;
            _ctlContainerField.DataChanged += _ctlContainerField_DataChanged;
        }

        //------------------------------------------------------
        //
        //------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _txtComputerNameField.Data = SEMI.AppCode.UIUtility.GetComputerName(this);

            // Set service name of the button dynamically.
            PERS.SubmitAction bSubmit = new PERS.SubmitAction();
            if (_ddlServiceTypeField.Data != null)
                bSubmit.ServiceName = _ddlServiceTypeField.Data.ToString();
            bSubmit.Location = PERS.ActionLocation.Button;
            _btnSubmit.DefaultAction = bSubmit;

            if (!Page.IsPostBack)
            {
                // additional logic for CR019 EquipmentWIPMain (or where the web part is referenced into a standalone page)
                if (Page.IsAJAXFloatingFrame)
                {
                    // hide the submit and reset buttons
                    _btnSubmit.Visible = false;
                    _btnSubmit.Enabled = false;
                    _btnReset.Visible = false;
                    _btnReset.Enabled = false;

                    Personalization.UIAction[] actUIActions = Page.ActionDispatcher.PageActions();
                    foreach (Personalization.UIAction act in actUIActions)
                    {
                        if (act.Name.ToUpper() == "SUBMIT")
                        {
                            act.ServiceName = _ddlServiceTypeField.Data.ToString();
                            act.Permissions = new PERS.PermissionDefinition();
                            act.Permissions.DisplayMode = PERS.PermissionDisplayMode.Disable;
                            act.Permissions.PagePermission = true;
                            act.Permissions.ServicePermission = false;
                        }
                    }

                    FetchData();
                }
            }
            else
            {
                //fixed for IE (data_changed event does not handled properly)
                DisplayWaferBins();
            }
        }

        //------------------------------------------------------
        //
        //------------------------------------------------------
        void _ctlContainerField_DataChanged(object sender, EventArgs e)
        {
            if (!_bIsPopup)
                FetchData();
        }

        //------------------------------------------------------
        //
        //------------------------------------------------------
        void _ndoEquipmentField_DataChanged(object sender, EventArgs e)
        {
            if (!_bIsPopup)
                FetchData();
        }

        //------------------------------------------------------
        //
        //------------------------------------------------------
        void _ndoProcessTypeField_DataChanged(object sender, EventArgs e)
        {
            if (!_bIsPopup)
                FetchData();
        }

        //------------------------------------------------------
        //
        //------------------------------------------------------
        void _txtLotBinsInit_DataChanged(object sender, EventArgs e)
        {
            if (!_bIsPopup)
                FetchData();
        }

        //------------------------------------------------------
        //
        //------------------------------------------------------
        private void SetBinningModeDisplay(bool IsWaferBinning)
        {
            _gridWaferBinDetails.Visible = IsWaferBinning;
            _ddlWaferScribeNumber.Visible = IsWaferBinning;
            _txtCurrentWaferBinQty.Visible = IsWaferBinning;
            _rdoSubProductField.Visible = IsWaferBinning;

            _gridLotBinsTxnDetailsFields.Visible = !IsWaferBinning;
            _txtTotalBinQty.Visible = !IsWaferBinning;
            _txtMaxBinsQtyField.Visible = !IsWaferBinning;
        }

        //---------------------------------------------------
        //
        //---------------------------------------------------
        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "RowCopy")
            {
                WaferBin_RowCopy();
            }
        } // WebPartCustomAction 

        //------------------------------------------------------
        //
        //------------------------------------------------------
        public void WaferBin_RowCopy()
        {
            // Here is action on click
            var parts = Page.EventArgument.Split(':');
            var rowIndex = parts[3];

            // check the row of button clicked is selected            
            if (!string.IsNullOrEmpty(rowIndex.ToString()))
            {
                LotBinsDetails[] oBinsDetails = (_gridWaferBinDetails.GridContext as BoundContext).Data as LotBinsDetails[];
                List<LotBinsDetails> oNewBinsDetailsList = new List<LotBinsDetails>();
                int iRowIndex = 0;
                if (oBinsDetails != null)
                {
                    foreach (LotBinsDetails oDetail in oBinsDetails)
                    {
                        LotBinsDetails oBinsDetailsClone = new LotBinsDetails();
                        oBinsDetailsClone = oDetail;
                        oNewBinsDetailsList.Add(oBinsDetailsClone);

                        if (iRowIndex == int.Parse(rowIndex))
                        {
                            LotBinsDetails oBinsDetailsClone2 = new LotBinsDetails();
                            oBinsDetailsClone2.Bin = oDetail.Bin;
                            oBinsDetailsClone2.BinCategory = oDetail.BinCategory;
                            oBinsDetailsClone2.BinProduct = oDetail.BinProduct;
                            oBinsDetailsClone2.BinQty = oDetail.BinQty;
                            oBinsDetailsClone2.ToWaferScribeNumber = oDetail.ToWaferScribeNumber;
                            oBinsDetailsClone2.BinComment = oDetail.BinComment;
                            oNewBinsDetailsList.Add(oBinsDetailsClone2);
                        }
                        iRowIndex++;
                    }

                    // bind to the grid
                    (_gridWaferBinDetails.GridContext as BoundContext).Data = oNewBinsDetailsList.ToArray();
                    _gridWaferBinDetails.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(_gridWaferBinDetails);

                }
            }
        }

        //------------------------------------------------------
        //
        //------------------------------------------------------
        private void FetchData()
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                bool bExecute = false;
                if (_chkIsActiveField != null)
                    bExecute = (_chkIsActiveField.CheckControl.Checked || _chkIsPopupField.CheckControl.Checked);

                if (bExecute)
                {
                    string SubProductList = "";
                    string ServiceTypeRequired = _ddlServiceTypeField.Data.ToString();
                    this.PrimaryServiceType = ServiceTypeRequired;
                    // Prepare service
                    ResultStatus oServiceResult = new ResultStatus(null, false);
                    var oServiceData = WCFObject.CreateObject(ServiceTypeRequired) as ICreator;
                    var oServiceRequest = WCFObject.CreateObject(ServiceTypeRequired + "_Request") as ICreator;
                    var oServiceInfo = WCFObject.CreateObject(ServiceTypeRequired + "_Info") as ICreator;
                    Result oResponseData = null;
                    var oService = new WSDataCreator().CreateService(ServiceTypeRequired, fs.CurrentUserProfile);

                    if (_ctlContainerField.Data != null)
                    {
                        // Prepare the service data
                        oServiceData.SetValue("Container", _ctlContainerField.Data as ContainerRef);
                        oServiceData.SetValue("ProcessType", _ndoProcessTypeField.Data as NamedObjectRef);
                        if (_ndoEquipmentField.Data != null)
                            oServiceData.SetValue("Equipment", _ndoEquipmentField.Data as NamedObjectRef);

                        // Prepare the service info
                        oServiceInfo.SetValue("Container", new OM.Info(true));
                        oServiceInfo.SetValue("ProcessTypeSelection", new OM.Info(true));
                        oServiceInfo.SetValue("IsMultiProducts", new OM.Info(true));
                        oServiceInfo.SetValue("BinsSelection", new OM.LotBinsDetails_Info());
                        oServiceInfo.SetValue("BinsSelection.Bin", new OM.Info(true));
                        oServiceInfo.SetValue("BinsSelection.BinCategory", new OM.Info(true));
                        oServiceInfo.SetValue("BinsSelection.BinProduct", new OM.Info(true));
                        oServiceInfo.SetValue("MaxBinsQty", new OM.Info(true));
                        oServiceInfo.SetValue("CurrentDetails", new OM.LotBinsDetails_Info());
                        oServiceInfo.SetValue("CurrentDetails.Bin", new OM.Info(true));
                        oServiceInfo.SetValue("CurrentDetails.BinCategory", new OM.Info(true));
                        oServiceInfo.SetValue("CurrentDetails.BinProduct", new OM.Info(true));
                        oServiceInfo.SetValue("CurrentDetails.BinLotId", new OM.Info(true));
                        oServiceInfo.SetValue("CurrentDetails.BinQty", new OM.Info(true));
                        oServiceInfo.SetValue("CurrentDetails.BinComment", new OM.Info(true));

                        oServiceInfo.SetValue("CurrentDetails.SubProduct", new OM.Info(true));

                        oServiceInfo.SetValue("CurrentDetails.ToWaferScribeNumber", new OM.Info(true));
                        oServiceInfo.SetValue("CurrentDetails.WaferScribeNumber", new OM.Info(true));

                        oServiceInfo.SetValue("IsWaferProcessing", new OM.Info(true));

                        oServiceInfo.SetValue("WafersDetailsSelection", new OM.WIPLotTxnWafersDetails_Info());
                        oServiceInfo.SetValue("WafersDetailsSelection.WaferScribeNumber", new OM.Info(true));

                        oServiceRequest.SetValue("Info", oServiceInfo);

                        // Request the data
                        ResultStatus oResultStatus = oService.GetEnvironment(oServiceData as DCObject, oServiceRequest as Request, out oResponseData);

                        if (oResultStatus.IsSuccess)
                        {
                            LotBinsDetails[] objBinSelection = ((oResponseData as ICreator).GetValue("Value.BinsSelection") as LotBinsDetails[]);
                            LotBinsDetails[] objCurrentDetails = ((oResponseData as ICreator).GetValue("Value.CurrentDetails") as LotBinsDetails[]);

                            _rdoSubProductField.Visible = (oResponseData as ICreator).GetValue("Value.IsMultiProducts") != null ? bool.Parse((oResponseData as ICreator).GetValue("Value.IsMultiProducts").ToString()) : false;

                            // Select the first record by default if it is called as a popup
                            if (_chkIsPopupField.CheckControl.Checked)
                            {
                                NamedObjectRef[] objProcessTypeSelection = ((oResponseData as ICreator).GetValue("Value.ProcessTypeSelection") as NamedObjectRef[]);
                                _ndoProcessTypeField.Data = objProcessTypeSelection != null ? objProcessTypeSelection[0] : null;
                            }

                            //------------------------ WAFER BINNING : START ------------------------
                            bool bIsWaferProcessing = bool.Parse((oResponseData as ICreator).GetValue("Value.IsWaferProcessing").ToString());
                            if (bIsWaferProcessing)
                            {
                                _chkIsWaferProcessing.CheckControl.Checked = true;

                                // retrieve the selection list of sub product
                                SubProductList = FetchSubProduct(); //bool.Parse((oResponseData as ICreator).GetValue("Value.IsMultiProducts").ToString()) ? FetchSubProduct() : "";
                                string[] SubProduct = SubProductList.Split(';');

                                if (objBinSelection != null)
                                {
                                    Hashtable htLotBins = new Hashtable();
                                    Hashtable htWafersBins = new Hashtable();
                                    Hashtable htBinSelectionIndex = new Hashtable();

                                    WIPLotTxnWafersDetails[] oWafers = ((oResponseData as ICreator).GetValue("Value.WafersDetailsSelection") as WIPLotTxnWafersDetails[]);

                                    if (oWafers != null)
                                    {
                                        // run thru the binsSelection, assign an index to each bin
                                        int iBinIndex = 0;
                                        foreach (LotBinsDetails oBin in objBinSelection)
                                        {
                                            htBinSelectionIndex.Add(oBin.Bin.ToString(), iBinIndex);
                                            iBinIndex++;
                                        }

                                        _ddlWaferScribeNumber.DropDownControl.Items.Clear();
                                        //clone the bins selection for each wafer, add the waferscribenumber to the waferscribenumber dropdown list       
                                        foreach (WIPLotTxnWafersDetails oWafer in oWafers)
                                        {
                                            for (int counter = 0; counter < SubProduct.Length; counter++)
                                            {
                                                LotBinsDetails[] oWaferBins = new LotBinsDetails[((oResponseData as ICreator).GetValue("Value.BinsSelection") as LotBinsDetails[]).Length];
                                                int iWaferBinIndex = 0;
                                                foreach (LotBinsDetails oBinDetail in objBinSelection)
                                                {
                                                    oWaferBins[iWaferBinIndex] = new LotBinsDetails();
                                                    oWaferBins[iWaferBinIndex].Bin = oBinDetail.Bin;
                                                    oWaferBins[iWaferBinIndex].BinCategory = oBinDetail.BinCategory;
                                                    oWaferBins[iWaferBinIndex].BinProduct = oBinDetail.BinProduct;
                                                    iWaferBinIndex++;
                                                }
                                                if (SubProduct[counter] == "")
                                                    htWafersBins.Add(oWafer.WaferScribeNumber.ToString(), oWaferBins);
                                                else
                                                    htWafersBins.Add(oWafer.WaferScribeNumber.ToString() + ';' + SubProduct[counter], oWaferBins);
                                            }
                                            _ddlWaferScribeNumber.DropDownControl.Items.Add(oWafer.WaferScribeNumber.ToString());
                                        }

                                        if (objCurrentDetails != null)
                                        {
                                            // assign the current details to the wafer bins
                                            foreach (LotBinsDetails oDetail in objCurrentDetails)
                                            {
                                                if (oDetail.WaferScribeNumber != null)
                                                {
                                                    string oDetailId = (oDetail.SubProduct == null) ? oDetail.WaferScribeNumber.ToString() : oDetail.WaferScribeNumber.ToString() + ";" + oDetail.SubProduct.Name + ":" + oDetail.SubProduct.Revision;
                                                    // get the wafer's bin details from the hashtable
                                                    LotBinsDetails[] oWaferBins = htWafersBins[oDetailId] as LotBinsDetails[];
                                                    // get the index of the bin from htBinSelectionIndex
                                                    int iBindex = int.Parse(htBinSelectionIndex[oDetail.Bin.ToString()].ToString());
                                                    // update the details
                                                    oWaferBins[iBindex].BinComment = oDetail.BinComment;
                                                    oWaferBins[iBindex].BinQty = oDetail.BinQty;
                                                    oWaferBins[iBindex].ToWaferScribeNumber = oDetail.ToWaferScribeNumber;
                                                    oWaferBins[iBindex].SubProduct = oDetail.SubProduct;

                                                    // set the bin details back into the hashtable
                                                    htWafersBins[oDetailId] = oWaferBins;
                                                    //htWafersBins[oDetail.WaferScribeNumber.ToString()] = oWaferBins;
                                                }
                                            }

                                        } // if (objCurrentDetails != null)

                                        // save the wafersbins hashtable into viewstate
                                        ViewState[kWafersBinViewState] = htWafersBins;

                                        _txtPrevWaferScribeNumber.ClearData();
                                        DisplayWaferBins();
                                    } // if oWafers != null
                                } // if objBinsSelection!= null
                                //------------------------ WAFER BINNING : END ------------------------
                            }
                            else
                            {
                                _chkIsWaferProcessing.CheckControl.Checked = false;
                                List<LotBinsDetails> oWaferDetail = new List<LotBinsDetails>();
                                List<LotBinsDetails> objGridData = new List<LotBinsDetails>();

                                int oIndex = 0;
                                bool Indicator = false;

                                if (objBinSelection != null)
                                {
                                    foreach (LotBinsDetails objSelectionBin in objBinSelection)
                                    {
                                        if (objCurrentDetails != null)
                                        {
                                            foreach (LotBinsDetails objCurrentBins in objCurrentDetails)
                                            {
                                                if (objCurrentBins.Bin == objSelectionBin.Bin)
                                                {
                                                    objGridData.Insert(oIndex, objCurrentBins);
                                                    Indicator = true;
                                                    oIndex += 1;
                                                }
                                            }
                                        }
                                        if (Indicator != true)
                                        {
                                            objGridData.Insert(oIndex, objSelectionBin);
                                            oIndex += 1;
                                        }
                                        else
                                            Indicator = false;
                                    }
                                }

                                (_gridLotBinsTxnDetailsFields.GridContext as BoundContext).Data = objGridData.ToArray();
                                _gridLotBinsTxnDetailsFields.BoundContext.LoadData();
                                CamstarWebControl.SetRenderToClient(_gridLotBinsTxnDetailsFields);
                            }// if bIsWaferProcessing

                            // Hide details columns
                            if (ServiceTypeRequired == "LotBinsInProcess")
                                _gridLotBinsTxnDetailsFields.BoundContext.Fields["BinLotId"].Visible = false;

                            // Display the details
                            _txtMaxBinsQtyField.Data = (oResponseData as ICreator).GetValue("Value.MaxBinsQty");

                            SetBinningModeDisplay(bIsWaferProcessing);
                        }
                        else
                        {
                            DisplayMessage(oResultStatus);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //------------------------------------------------------
        //
        //------------------------------------------------------
        public void DisplayWaferBins()
        {
            // update the current grid data back to the wafersbins hash
            if (_txtPrevWaferScribeNumber.Data != null)
            {
                Hashtable htWafersBins = ViewState[kWafersBinViewState] as Hashtable;
                if (htWafersBins != null)
                {
                    LotBinsDetails[] oWaferBinDetails = _gridWaferBinDetails.Data as LotBinsDetails[];
                    htWafersBins[_txtPrevWaferScribeNumber.Data.ToString()] = oWaferBinDetails;
                    ViewState[kWafersBinViewState] = htWafersBins;
                }
            }

            // display the bins of the selected waferscribenumber
            _txtPrevWaferScribeNumber.ClearData();

            //ensure the data is cleared when the field is empty (fixed for IE)
            if (_rdoSubProductField.IsEmpty)
            {
                _rdoSubProductField.ClearData();
            }

            string PreWaferScribeNumber = "";
            if (_ddlWaferScribeNumber.DropDownControl.SelectedValue != "" && (_rdoSubProductField.Data != null || !_rdoSubProductField.IsEmpty))
                PreWaferScribeNumber = _ddlWaferScribeNumber.DropDownControl.SelectedValue.ToString() + ";" + _rdoSubProductField.Text + ":" + _rdoSubProductField.RevisionValue;
            else if (_ddlWaferScribeNumber.DropDownControl.SelectedValue != "")
                PreWaferScribeNumber = _ddlWaferScribeNumber.DropDownControl.SelectedValue.ToString();
            else
                PreWaferScribeNumber = "";

            if (PreWaferScribeNumber != "")
            {
                Hashtable htWafersBins = ViewState[kWafersBinViewState] as Hashtable;
                if (htWafersBins != null)
                {
                    LotBinsDetails[] oWaferBinDetails = htWafersBins[PreWaferScribeNumber] as LotBinsDetails[];
                    (_gridWaferBinDetails.GridContext as BoundContext).Data = null;
                    (_gridWaferBinDetails.GridContext as BoundContext).ClearData();
                    (_gridWaferBinDetails.GridContext as BoundContext).Data = oWaferBinDetails;
                    _gridWaferBinDetails.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(_gridWaferBinDetails);
                }

                _txtPrevWaferScribeNumber.Data = PreWaferScribeNumber;
            }
        }

        //------------------------------------------------------
        //
        //------------------------------------------------------  
        public override void GetInputData(OM.Service serviceData)
        {
            try
            {
                bool bExecute = false;
                if (_chkIsActiveField != null)
                    bExecute = (_chkIsActiveField.CheckControl.Checked || _bIsPopup);

                if (bExecute)
                {
                    base.GetInputData(serviceData);
                    // manually collect the container since the field expression has been set to LotBinsTxn.Container and the get input data will not pull it for the sub classed services
                    if (serviceData is OM.LotBinsInProcess)
                        (serviceData as OM.LotBinsInProcess).Container = _ctlContainerField.Data as ContainerRef;
                    else if (serviceData is OM.LotBinsDispose)
                        (serviceData as OM.LotBinsDispose).Container = _ctlContainerField.Data as ContainerRef;
                    else if (serviceData is OM.LotBinsPostProcess)
                        (serviceData as OM.LotBinsPostProcess).Container = _ctlContainerField.Data as ContainerRef;

                    if (_chkIsWaferProcessing.CheckControl.Checked)
                    {
                        Hashtable htWafersBins = ViewState[kWafersBinViewState] as Hashtable;

                        // update the current grid data back to the wafersbins hash
                        if (_txtPrevWaferScribeNumber.Data != null)
                        {
                            if (htWafersBins != null)
                            {
                                LotBinsDetails[] oWaferBinDetails = _gridWaferBinDetails.Data as LotBinsDetails[];
                                htWafersBins[_txtPrevWaferScribeNumber.Data.ToString()] = oWaferBinDetails;
                            }
                        }

                        // collate the bins into the details to be submitted
                        if (htWafersBins != null)
                        {
                            bool containSubProduct = false;
                            List<LotBinsDetails> oBinDetailsList = new List<LotBinsDetails>();

                            foreach (DictionaryEntry oEntry in htWafersBins)
                            {
                                LotBinsDetails[] oWaferBinDetails = oEntry.Value as LotBinsDetails[];
                                foreach (LotBinsDetails oWaferBinDetail in oWaferBinDetails)
                                {
                                    if (oWaferBinDetail.BinQty != null || oWaferBinDetail.ToWaferScribeNumber != null)
                                    {
                                        containSubProduct = oEntry.Key.ToString().Contains(';');
                                        if (containSubProduct)
                                        {
                                            RevisionedObjectRef InputtedSubProduct = new RevisionedObjectRef();
                                            InputtedSubProduct.Name = oEntry.Key.ToString().Substring(oEntry.Key.ToString().IndexOf(';') + 1, oEntry.Key.ToString().IndexOf(':') - (oEntry.Key.ToString().IndexOf(';') + 1));
                                            InputtedSubProduct.Revision = oEntry.Key.ToString().Substring(oEntry.Key.ToString().IndexOf(':') + 1);

                                            oWaferBinDetail.WaferScribeNumber = oEntry.Key.ToString().Substring(0, oEntry.Key.ToString().IndexOf(';'));
                                            oWaferBinDetail.SubProduct = InputtedSubProduct;
                                        }
                                        else
                                        {
                                            oWaferBinDetail.WaferScribeNumber = oEntry.Key.ToString();
                                        }
                                        oBinDetailsList.Add(oWaferBinDetail);
                                    }
                                }
                            }

                            if (oBinDetailsList.Count > 0)
                            {
                                if (serviceData is OM.LotBinsInProcess)
                                    (serviceData as OM.LotBinsInProcess).Details = oBinDetailsList.ToArray();
                                else if (serviceData is OM.LotBinsDispose)
                                    (serviceData as OM.LotBinsDispose).Details = oBinDetailsList.ToArray();
                                else if (serviceData is OM.LotBinsPostProcess)
                                    (serviceData as OM.LotBinsPostProcess).Details = oBinDetailsList.ToArray();
                            }
                        }

                    }
                    else
                    {
                        int gridCount = _gridLotBinsTxnDetailsFields.BoundContext.GetTotalRows();
                        if (gridCount > 0)
                        {

                            LotBinsDetails[] oBinDetails = new LotBinsDetails[gridCount];
                            for (int i = 0; i < gridCount; i++)
                            {
                                string sRowId = i.ToString().PadLeft(6, '0');
                                oBinDetails[i] = new LotBinsDetails();
                                oBinDetails[i].Bin = _gridLotBinsTxnDetailsFields.GridContext.GetCell(sRowId, "Bin").ToString();
                                string binCat = _gridLotBinsTxnDetailsFields.GridContext.GetCell(sRowId, "BinCategory").ToString();
                                oBinDetails[i].BinCategory = new Enumeration<BinCategoryEnum, string>(binCat);
                                oBinDetails[i].BinCategory.Value = _gridLotBinsTxnDetailsFields.GridContext.GetCell(sRowId, "BinCategory").ToString();
                                string[] BinProduct = (_gridLotBinsTxnDetailsFields.GridContext.GetCell(sRowId, "BinProduct").ToString()).Split(':');
                                oBinDetails[i].BinProduct = new RevisionedObjectRef();
                                oBinDetails[i].BinProduct.Name = BinProduct[0];
                                oBinDetails[i].BinProduct.Revision = BinProduct[1];
                                if (_gridLotBinsTxnDetailsFields.GridContext.GetCell(sRowId, "BinQty") != null)
                                    oBinDetails[i].BinQty = int.Parse(_gridLotBinsTxnDetailsFields.GridContext.GetCell(sRowId, "BinQty").ToString());

                                if (_gridLotBinsTxnDetailsFields.GridContext.GetCell(sRowId, "BinLotId") != null)
                                    oBinDetails[i].BinLotId = _gridLotBinsTxnDetailsFields.GridContext.GetCell(sRowId, "BinLotId").ToString();
                                if (_gridLotBinsTxnDetailsFields.GridContext.GetCell(sRowId, "BinComment") != null)
                                    oBinDetails[i].BinComment = _gridLotBinsTxnDetailsFields.GridContext.GetCell(sRowId, "BinComment").ToString();
                            } //End for loop

                            if (serviceData is OM.LotBinsInProcess)
                                (serviceData as OM.LotBinsInProcess).Details = oBinDetails;
                            else if (serviceData is OM.LotBinsDispose)
                                (serviceData as OM.LotBinsDispose).Details = oBinDetails;
                            else if (serviceData is OM.LotBinsPostProcess)
                                (serviceData as OM.LotBinsPostProcess).Details = oBinDetails;
                        }
                    } // if (_chkIsWaferProcessing.CheckControl.Checked)
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //------------------------------------------------------
        //
        //------------------------------------------------------
        public void ResetButton_Click(object sender, EventArgs e)
        {
            _gridLotBinsTxnDetailsFields.ClearData();
            _gridWaferBinDetails.ClearData();
            FetchData();
        }

        private string FetchSubProduct()
        {
            string SubProductList = "";
            try
            {
                LotBinsTxn oServiceData = new LotBinsTxn();
                LotBinsTxn_Info oServiceInfo = new LotBinsTxn_Info();
                ResultStatus oResultStatus = new ResultStatus();
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                LotBinsTxnService oService = new LotBinsTxnService(fs.CurrentUserProfile);
                LotBinsTxn_Request oRequest = new LotBinsTxn_Request();
                LotBinsTxn_Result oResponseData = new LotBinsTxn_Result();


                // Ensure a selection id exist
                if ((!_ctlContainerField.IsEmpty))
                {
                    _rdoSubProductField.ClearData();
                    oServiceData.Container = new ContainerRef();
                    oServiceData.Container = _ctlContainerField.Data as ContainerRef;
                    oServiceInfo.SubProduct = FieldInfoUtil.RequestSelectionValue();

                    oRequest.Info = oServiceInfo;
                    oResultStatus = oService.GetEnvironment(oServiceData, oRequest, out oResponseData);
                    if (oResultStatus.IsSuccess)
                    {
                        if (oResponseData.Environment.SubProduct.SelectionValues != null)
                        {
                            if (oResponseData.Environment.SubProduct.SelectionValues.Rows.Length > 0)
                            {
                                for (int counter = 0; counter < oResponseData.Environment.SubProduct.SelectionValues.Rows.Length; counter++)
                                    SubProductList = SubProductList + ";" + oResponseData.Environment.SubProduct.SelectionValues.Rows[counter].Values[0] + ":" + oResponseData.Environment.SubProduct.SelectionValues.Rows[counter].Values[1];
                            }
                        }

                        _rdoSubProductField.SetSelectionValues(oResponseData.Environment.SubProduct.SelectionValues);
                        CamstarWebControl.SetRenderToClient(_rdoSubProductField);
                    }
                }


            }
            catch (Exception Ex)
            {
                DisplayMessage(new OM.ResultStatus(Ex.TargetSite.Name + "() : " + Ex.Message, false));
            }
            return SubProductList;
        }
    }
}





