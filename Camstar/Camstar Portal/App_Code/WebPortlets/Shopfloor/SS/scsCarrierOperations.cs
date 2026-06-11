/* Copyright 2024 Siemens */
using System;
using System.Linq;
using Camstar.WebPortal.FormsFramework;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using CGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class CarrierOperationsTxn : scsShopfloorBase
    {
        CWC.NamedObject _ndoCarrierField { get { return Page.FindCamstarControl("CarrierPositionAssign_Carrier") as CWC.NamedObject; } }
        CWC.BooleanSwitch _boolAutoManualToggle { get { return Page.FindCamstarControl("CarrierAssignPostion_Action") as CWC.BooleanSwitch; } }
        CWC.RadioButton _loadButton { get { return Page.FindCamstarControl("CarrierPositionAssign_Load") as CWC.RadioButton; } }
        CWC.RadioButton _unloadButton { get { return Page.FindCamstarControl("CarrierPositionAssign_Unload") as CWC.RadioButton; } }
        CWC.ContainerList _carrierContainerField { get { return Page.FindCamstarControl("CarrierPositionAssign_CarrierContainer") as CWC.ContainerList; } }
        CWC.DropDownList _slotPositionList { get { return Page.FindCamstarControl("SlotPosition") as CWC.DropDownList; } }
        CWC.TextBox _containerField { get { return Page.FindCamstarControl("CarrierPositionAssign_Container") as CWC.TextBox; } }
        CWC.Button _containerButton { get { return Page.FindCamstarControl("BtnContainer") as CWC.Button; } }
        CWC.Button _containerReloadButton { get { return Page.FindCamstarControl("BtnReloadContainer") as CWC.Button; } }
        CWC.Button _containerChgSlotButton { get { return Page.FindCamstarControl("BtnChgContSlot") as CWC.Button; } }
        CWC.CheckBox _chkboxCloseCarrierContainer { get { return Page.FindCamstarControl("Control0") as CWC.CheckBox; } }
        CWC.DropDownList _transferCarrierList { get { return Page.FindCamstarControl("CarrierPositionAssign_TransferCarrierSelection") as CWC.DropDownList; } }
        CWC.NamedObject _ndoTransferCarrierField { get { return Page.FindCamstarControl("CarrierPositionAssign_TransferCarrier") as CWC.NamedObject; } }
        JQDataGrid _gridCarrierPosition { get { return Page.FindCamstarControl("CarrierPositionAssign_Details") as JQDataGrid; } }
        JQDataGrid _gridHiddenLabel { get { return Page.FindCamstarControl("CarrierPositionAssign_HiddenLabel") as JQDataGrid; } }
		CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("scsCarrierPositionAssign_ComputerName") as CWC.TextBox; } }
		 CWC.DateChooser DueDate { get { return Page.FindCamstarControl("scsCarrierPositionAssign_DueDate") as CWC.DateChooser; } }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "HiddenUnloadAllAction").First().IsHidden = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "HiddenTransferAllAction").First().IsHidden = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "UnloadAllAction").First().IsDisabled = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "TransferAllAction").First().IsDisabled = true;
            }

            _containerButton.Hidden = true;
            _containerReloadButton.Hidden = true;
            _containerChgSlotButton.Hidden = true;

            //_containerField.Attributes.Add("onchange", "return CarrierOperations_ContainerFieldValidation(event);");
            //_containerField.Attributes.Add("onkeydown", "return CarrierOperations_ContainerFieldValidation(event);");

            _carrierContainerField.DataChanged += new EventHandler(CarrierContainerField_Change);
            _ndoCarrierField.DataChanged += new EventHandler(CarrierField_Change);
            _boolAutoManualToggle.RadioListControl.SelectedIndexChanged += new EventHandler(AutoManualToggle_Click);
            _loadButton.RadioControl.CheckedChanged += new EventHandler(LoadButton_Click);
            _unloadButton.RadioControl.CheckedChanged += new EventHandler(UnloadButton_Click);
            _slotPositionList.DataChanged += new EventHandler(SlotPosition_Change);
            _containerButton.Click += new EventHandler(ContainerButton_Click);
            _containerReloadButton.Click += new EventHandler(ContainerReloadButton_Click);
            _containerChgSlotButton.Click += new EventHandler(ContainerChgSlotButton_Click);
            _transferCarrierList.DataChanged += new EventHandler(TransferCarrier_Change);
			_txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);

            //Populate hidden label grid
            List<Label> labels = new List<Label>();
            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
            var vlabels = new[]{
                //add list of label
                new WCF.ObjectStack.Label("scsContainerValidation"),
                new WCF.ObjectStack.Label("scsStatusValidation"),
                new WCF.ObjectStack.Label("scsWorkflowStepValidation"),
                new WCF.ObjectStack.Label("scsParentContainerValidation"),
                new WCF.ObjectStack.Label("scsScheduleDataValidation"),
                new WCF.ObjectStack.Label("scsWorkOrderValidation"),
                new WCF.ObjectStack.Label("scsWIPTransactionValidation"),
            };
            labelCache.GetLabels(new LabelList(vlabels));
            if (vlabels.Length != 0)
            {
                for (int i = 0; i < vlabels.Length; i++)
                {
                    labels.Add(new Label() { Name = vlabels[i].Name, Value = vlabels[i].Value });
                }
                _gridHiddenLabel.Data = labels.ToArray();
            }
        }

        private void CarrierField_Change(object sender, EventArgs e)
        {
            _boolAutoManualToggle.RadioListControl.SelectedIndex = 1;
            AutoManualToggle_Click(null, null);
        }

        private void CarrierContainerField_Change(object sender, EventArgs e)
        {
            if (_carrierContainerField.Data == null)
            {
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "UnloadAllAction").First().IsDisabled = true;
            }
            else
            {
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "UnloadAllAction").First().IsDisabled = false;
            }
            TransferCarrier_Change(null, null);
        }

        private void AutoManualToggle_Click(object sender, EventArgs e)
        {
            if (_boolAutoManualToggle.RadioListControl.SelectedIndex == 1)
            {
                LoadButton_Click(null, null);
                _unloadButton.Enabled = true;
                _slotPositionList.Enabled = true;
                _chkboxCloseCarrierContainer.Enabled = true;
                _transferCarrierList.Enabled = true;
            }
            else
            {
                LoadButton_Click(null, null);
                _unloadButton.Enabled = false;
                _slotPositionList.ClearData();
                _slotPositionList.Enabled = false;
                _chkboxCloseCarrierContainer.CheckControl.Checked = false;
                // _chkboxCloseCarrierContainer.Enabled = false;
                _transferCarrierList.ClearData();
                _transferCarrierList.Enabled = false;
            }

            SlotPosition_Change(null, null);
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            _loadButton.RadioControl.Checked = true;
            _unloadButton.RadioControl.Checked = false;
            _slotPositionList.Enabled = true;
            _containerField.ClearData();
            _containerField.Enabled = false;
        }

        private void UnloadButton_Click(object sender, EventArgs e)
        {
            _loadButton.RadioControl.Checked = false;
            _unloadButton.RadioControl.Checked = true;
            _slotPositionList.ClearData();
            _slotPositionList.Enabled = false;
            _containerField.Enabled = true;
        }

        private void SlotPosition_Change(object sender, EventArgs e)
        {
            if (_slotPositionList.Data == null && _boolAutoManualToggle.RadioListControl.SelectedIndex == 1)
            {
                _containerField.ClearData();
                _containerField.Enabled = false;
            }
            else
            {
                _containerField.ClearData();
                _containerField.Enabled = true;
            }
        }

        private void TransferCarrier_Change(object sender, EventArgs e)
        {
            if (_carrierContainerField.Data == null || _transferCarrierList.Data == null)
            {
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "TransferAllAction").First().IsDisabled = true;
            }
            else
            {
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "TransferAllAction").First().IsDisabled = false;
            }
        }

        private void ContainerButton_Click(object sender, EventArgs e)
        {
            if (_loadButton.RadioControl.Checked == true)
            {
                if (_boolAutoManualToggle.RadioListControl.SelectedIndex == 1)
                {
                    ManualLoadContainer();
                }
                else
                {
                    AutoLoadContainer();
                }

            }
            else if (_unloadButton.RadioControl.Checked == true)
            {
                ManualUnloadContainer();
            }
        }

        private void ContainerReloadButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_gridCarrierPosition.GridContext.GetTotalRows() != 0)
                {
                    if (_containerField.Data != null)
                    {
                        if (_boolAutoManualToggle.RadioListControl.SelectedIndex == 1)
                        {
                            //ManualLoadContainer();
                            CarrierPositionAssignDetails[] oDetails = (_gridCarrierPosition.GridContext as BoundContext).Data as CarrierPositionAssignDetails[];

                            if (oDetails != null)
                            {
                                //Unloads container
                                oDetails.First(x => x.Container != null ? x.Container.Name == new ContainerRef(_containerField.Data.ToString()).Name : false).SetupAction = new Enumeration<SetupActionEnum, int>(1);
                                //Loads container
                                oDetails.First(x => x.PositionNumber == _slotPositionList.Data.ToString()).SetupAction = new Enumeration<SetupActionEnum, int>(0);
                                oDetails.First(x => x.PositionNumber == _slotPositionList.Data.ToString()).Container = new ContainerRef(_containerField.Data.ToString());
                            }

                            (_gridCarrierPosition.GridContext as BoundContext).Data = oDetails;
                            _gridCarrierPosition.DataBind();
                            CamstarWebControl.SetRenderToClient(_gridCarrierPosition);
                            _containerField.ClearData();
                        }
                        else
                        {
                            //AutoLoadContainer();
                            CarrierPositionAssignDetails[] oDetails = (_gridCarrierPosition.GridContext as BoundContext).Data as CarrierPositionAssignDetails[];

                            if (oDetails != null)
                            {
                                //Unloads container
                                oDetails.First(x => x.Container != null ? x.Container.Name == new ContainerRef(_containerField.Data.ToString()).Name : false).SetupAction = new Enumeration<SetupActionEnum, int>(1);
                                //Loads container
                                oDetails.First(x => x.Container == null && x.PositionStatus != "DOWN" && x.SetupAction == null).SetupAction = new Enumeration<SetupActionEnum, int>(0);
                                oDetails.First(x => x.Container == null && x.PositionStatus != "DOWN").Container = new ContainerRef(_containerField.Data.ToString());
                            }

                            (_gridCarrierPosition.GridContext as BoundContext).Data = oDetails;
                                _gridCarrierPosition.DataBind();
                                CamstarWebControl.SetRenderToClient(_gridCarrierPosition);
                                _containerField.ClearData();
                                _containerField.Focus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
                DisplayMessage(new OM.ResultStatus("Container is not found", false));
            }
        }

        private void ManualLoadContainer()
        {
            try
            {
                if (_gridCarrierPosition.GridContext.GetTotalRows() != 0)
                {
                    if (_slotPositionList.Data != null && _containerField.Data != null)
                    {
                        CarrierPositionAssignDetails[] oDetails = (_gridCarrierPosition.GridContext as BoundContext).Data as CarrierPositionAssignDetails[];

                        if (oDetails != null)
                        {
                            oDetails.First(x => x.PositionNumber == _slotPositionList.Data.ToString()).SetupAction = new Enumeration<SetupActionEnum, int>(0);
                            oDetails.First(x => x.PositionNumber == _slotPositionList.Data.ToString()).Container = new ContainerRef(_containerField.Data.ToString());
                        }

                        (_gridCarrierPosition.GridContext as BoundContext).Data = oDetails;
                        _gridCarrierPosition.DataBind();
                        CamstarWebControl.SetRenderToClient(_gridCarrierPosition);
                        _containerField.ClearData();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }

        }

        private void AutoLoadContainer()
        {
            try
            {
                if (_gridCarrierPosition.GridContext.GetTotalRows() != 0)
                {
                    if (_containerField.Data != null)
                    {
                        CarrierPositionAssignDetails[] oDetails = (_gridCarrierPosition.GridContext as BoundContext).Data as CarrierPositionAssignDetails[];

                        if (oDetails != null)
                        {
                            oDetails.First(x => x.Container == null && x.PositionStatus != "DOWN").SetupAction = new Enumeration<SetupActionEnum, int>(0);
                            oDetails.First(x => x.Container == null && x.PositionStatus != "DOWN").Container = new ContainerRef(_containerField.Data.ToString());
                        }

                        (_gridCarrierPosition.GridContext as BoundContext).Data = oDetails;
                        _gridCarrierPosition.DataBind();
                        CamstarWebControl.SetRenderToClient(_gridCarrierPosition);
                        _containerField.ClearData();
                        _containerField.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus("There is no empty slots available", false));
            }

        }

        private void ContainerChgSlotButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_gridCarrierPosition.GridContext.GetTotalRows() != 0)
                {
                    if (_slotPositionList.Data != null && _containerField.Data != null)
                    {
                        CarrierPositionAssignDetails[] oDetails = (_gridCarrierPosition.GridContext as BoundContext).Data as CarrierPositionAssignDetails[];

                        if (oDetails != null)
                        {
                            //remove pending load container from grid	
                            oDetails.First(x => x.Container != null ? x.Container.Name == new ContainerRef(_containerField.Data.ToString()).Name && x.SetupAction == 0 : false).SetupAction = null;
                            oDetails.First(x => x.Container != null ? x.Container.Name == new ContainerRef(_containerField.Data.ToString()).Name && x.Product == null : false).Container = null;
                            //insert cotainer into new slot position	
                            oDetails.First(x => x.PositionNumber == _slotPositionList.Data.ToString()).SetupAction = new Enumeration<SetupActionEnum, int>(0);
                            oDetails.First(x => x.PositionNumber == _slotPositionList.Data.ToString()).Container = new ContainerRef(_containerField.Data.ToString());
                        }

                        (_gridCarrierPosition.GridContext as BoundContext).Data = oDetails;
                        _gridCarrierPosition.DataBind();
                        CamstarWebControl.SetRenderToClient(_gridCarrierPosition);
                        _containerField.ClearData();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
            }
        }

        private void ManualUnloadContainer()
        {
            try
            {
                if (_gridCarrierPosition.GridContext.GetTotalRows() != 0)
                {
                    if (_containerField.Data != null)
                    {
                        CarrierPositionAssignDetails[] oDetails = (_gridCarrierPosition.GridContext as BoundContext).Data as CarrierPositionAssignDetails[];

                        if (oDetails != null)
                        {
                            oDetails.First(x => x.Container != null ? x.Container.Name == new ContainerRef(_containerField.Data.ToString()).Name : false).SetupAction = new Enumeration<SetupActionEnum, int>(1);
                        }

                        (_gridCarrierPosition.GridContext as BoundContext).Data = oDetails;
                        _gridCarrierPosition.DataBind();
                        CamstarWebControl.SetRenderToClient(_gridCarrierPosition);
                        _containerField.ClearData();
                        _containerField.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                //DisplayMessage(new OM.ResultStatus(ex.TargetSite.Name + "() : " + ex.Message, false));
                DisplayMessage(new OM.ResultStatus("Container is not found", false));
            }
        }

        private void ResetAllFields()
        {
            Page.ClearValues();
            _unloadButton.Enabled = true;
			_slotPositionList.ClearData();	
            _slotPositionList.Enabled = true;
            _containerField.Enabled = true;
            _chkboxCloseCarrierContainer.Enabled = true;
            _transferCarrierList.Enabled = true;

            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "UnloadAllAction").First().IsDisabled = true;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "TransferAllAction").First().IsDisabled = true;
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            ResultStatus myResultStatus = new ResultStatus();

            if (action != null && action.Parameters == "Reset")
            {
                ResetAllFields();
            }
            else if (action != null && action.Parameters == "CustomSubmit")
            {
                DisplayMessage(new ResultStatus("", true));
                myResultStatus = CustomSubmit();
                if(myResultStatus.Message != null)
                {
                    if (myResultStatus.Message != null && myResultStatus.IsSuccess)
                    {
                        e.Result = new ResultStatus(myResultStatus.Message, true);
                        ResetAllFields();
                    }
                    else
                        e.Result = new ResultStatus(myResultStatus.Message, false);
                }
                else
                {
                    e.Result = new ResultStatus(myResultStatus.ToString(), false);
                }

            }
            else if (action != null && action.Parameters == "ConfirmTransferAll")
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                // init service objects
                scsCarrierPositionAssignService oService = new scsCarrierPositionAssignService(fs.CurrentUserProfile);
                scsCarrierPositionAssign oServiceData = new scsCarrierPositionAssign();
                scsCarrierPositionAssign_Info oServiceInfo = new scsCarrierPositionAssign_Info();
                scsCarrierPositionAssign_Result oServiceResult = new scsCarrierPositionAssign_Result();

                //Passing the input
                oServiceData.Carrier = (NamedObjectRef)_ndoCarrierField.Data;
                oServiceData.TransferCarrier = (NamedObjectRef)_ndoTransferCarrierField.Data;

                oServiceInfo.scsTransferAllConfirmation = FieldInfoUtil.RequestValue();

                // init request
                scsCarrierPositionAssign_Request oServiceRequest = new scsCarrierPositionAssign_Request();

                oServiceRequest.Info = oServiceInfo;

                // execute Unload All
                ResultStatus oResultStatus = oService.GetEnvironment(oServiceData, oServiceRequest, out oServiceResult);

                if (oResultStatus.IsSuccess)
                {
                    if (oServiceResult.Value.scsTransferAllConfirmation == true)
                    {
                        ScriptManager.RegisterStartupScript(Page.Form, this.GetType(), "myConfirm", "CarrierOperations_TransferAllConfirmation();", true);
                    }
                    else
                    {
                        DisplayMessage(new ResultStatus("", true));
                        myResultStatus = TransferAll();
                        if (myResultStatus.Message != null)
                        {
                            if (myResultStatus.Message != null && myResultStatus.IsSuccess)
                            {
                                e.Result = new ResultStatus(myResultStatus.Message, true);
                                ResetAllFields();
                            }
                            else
                                e.Result = new ResultStatus(myResultStatus.Message, false);
                        }
                        else
                        {
                            e.Result = new ResultStatus(myResultStatus.ToString(), false);
                        }
                    }
                }                
            }
            else if (action != null && action.Parameters == "TransferAll")
            {
                DisplayMessage(new ResultStatus("", true));
                myResultStatus = TransferAll();
                if (myResultStatus.Message != null)
                {
                    if (myResultStatus.Message != null && myResultStatus.IsSuccess)
                    {
                        e.Result = new ResultStatus(myResultStatus.Message, true);
                        ResetAllFields();
                    }
                    else
                        e.Result = new ResultStatus(myResultStatus.Message, false);
                }
                else
                {
                    e.Result = new ResultStatus(myResultStatus.ToString(), false);
                }
            }
            else if (action != null && action.Parameters == "ConfirmUnloadAll")
            {
                ScriptManager.RegisterStartupScript(Page.Form, this.GetType(), "myConfirm", "CarrierOperations_UnloadAllConfirmation();", true);
            }
            else if (action != null && action.Parameters == "UnloadAll")
            {
                DisplayMessage(new ResultStatus("", true));
                myResultStatus = UnloadAll();
                if (myResultStatus.Message != null)
                {
                    if (myResultStatus.Message != null && myResultStatus.IsSuccess)
                    {
                        e.Result = new ResultStatus(myResultStatus.Message, true);
                        ResetAllFields();
                    }
                    else
                        e.Result = new ResultStatus(myResultStatus.Message, false);
                }
                else
                {
                    e.Result = new ResultStatus(myResultStatus.ToString(), false);
                }
            }
        }

        public ResultStatus CustomSubmit()
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                // init service objects
                scsCarrierPositionAssignService oService = new scsCarrierPositionAssignService(fs.CurrentUserProfile);
                scsCarrierPositionAssign oServiceData = new scsCarrierPositionAssign();
                scsCarrierPositionAssign_Info oServiceInfo = new scsCarrierPositionAssign_Info();
                scsCarrierPositionAssign_Result oServiceResult = new scsCarrierPositionAssign_Result();

                if(_ndoCarrierField.Data == null)
                {
                    Exception ex = new Exception("Please select a Carrier.");
                    throw ex;
                }

                //Passing the input
				oServiceData.ComputerName = _txtComputerName.TextControl.Text;
                oServiceData.Carrier = (NamedObjectRef)_ndoCarrierField.Data;
				oServiceData.CloseCarrierContainer = _chkboxCloseCarrierContainer.IsChecked;
                oServiceData.UnloadAll = false;
				if (DueDate.Data != null)
				oServiceData.DueDate = Convert.ToDateTime(DueDate.Data.ToString());
                int changesMade = 0;
                int gridRowCount = _gridCarrierPosition.BoundContext.GetTotalRows();
                if (gridRowCount > 0)
                {
                    CarrierPositionAssignDetails[] oDetails = _gridCarrierPosition.Data as CarrierPositionAssignDetails[];
                    if (oDetails != null)
                        if (oDetails.Length > 0)
                        {
                            for (int i = 0; i < oDetails.Length; i++)
                            {
                                CarrierPositionAssignDetails oDetail = oDetails[i];
                                if ((oDetail.Container != null) && (oDetail.SetupAction != null))
                                {
                                    oDetail.WaferMapDetailsType = null;
                                    oDetail.ListItemIndex = null;
                                    oDetail.Self = null;
                                    changesMade++;
                                }
                                else
                                {
                                    oDetails[i] = null;
                                }
                            }

                            oServiceData.Details = oDetails;
                        }
                }

                if(changesMade == 0)
                {
                    Exception ex = new Exception("No changes have been made.");
                    throw ex;
                }

                // init request
                scsCarrierPositionAssign_Request oServiceRequest = new scsCarrierPositionAssign_Request();
                oServiceRequest.Info = oServiceInfo;

                // execute
                ResultStatus oResultStatus = oService.ExecuteTransaction(oServiceData, oServiceRequest, out oServiceResult);

                if (oResultStatus.IsSuccess)
                {
                    return oResultStatus;
                }
                else
                    return oResultStatus;
            }
            catch (Exception ex)
            {
                ResultStatus oResultStatus = new ResultStatus(ex.Message,false);
                return oResultStatus;
            }

        } // CustomSubmit

        public ResultStatus UnloadAll()
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                // init service objects
                scsCarrierPositionAssignService oService = new scsCarrierPositionAssignService(fs.CurrentUserProfile);
                scsCarrierPositionAssign oServiceData = new scsCarrierPositionAssign();
                scsCarrierPositionAssign_Info oServiceInfo = new scsCarrierPositionAssign_Info();
                scsCarrierPositionAssign_Result oServiceResult = new scsCarrierPositionAssign_Result();

                if (_ndoCarrierField.Data == null)
                {
                    Exception ex = new Exception("Please select a Carrier.");
                    throw ex;
                }

                //Passing the input
                oServiceData.Carrier = (NamedObjectRef)_ndoCarrierField.Data;
                oServiceData.CloseCarrierContainer = _chkboxCloseCarrierContainer.IsChecked;
                oServiceData.UnloadAll = true;

                // init request
                scsCarrierPositionAssign_Request oServiceRequest = new scsCarrierPositionAssign_Request();
                oServiceRequest.Info = oServiceInfo;

                // execute Unload All
                ResultStatus oResultStatus = oService.ExecuteTransaction(oServiceData, oServiceRequest, out oServiceResult);

                if (oResultStatus.IsSuccess)
                {
                    return oResultStatus;
                }
                else
                    return oResultStatus;
            }
            catch (Exception ex)
            {
                ResultStatus oResultStatus = new ResultStatus(ex.Message, false);
                return oResultStatus;
            }

        } // UnloadAll

        public ResultStatus TransferAll()
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                // init service objects
                scsCarrierPositionAssignService oService = new scsCarrierPositionAssignService(fs.CurrentUserProfile);
                scsCarrierPositionAssign oServiceData = new scsCarrierPositionAssign();
                scsCarrierPositionAssign_Info oServiceInfo = new scsCarrierPositionAssign_Info();
                scsCarrierPositionAssign_Result oServiceResult = new scsCarrierPositionAssign_Result();

                //Passing the input
                oServiceData.Carrier = (NamedObjectRef)_ndoCarrierField.Data;
                oServiceData.TransferCarrier = (NamedObjectRef)_ndoTransferCarrierField.Data;
                oServiceData.CloseCarrierContainer = _chkboxCloseCarrierContainer.IsChecked;
                oServiceData.TransferAll = true;

                // init request
                scsCarrierPositionAssign_Request oServiceRequest = new scsCarrierPositionAssign_Request();

                oServiceRequest.Info = oServiceInfo;

                // execute Transfer All
                ResultStatus oResultStatus = oService.ExecuteTransaction(oServiceData, oServiceRequest, out oServiceResult);

                if (oResultStatus.IsSuccess)
                {
                    return oResultStatus;
                }
                else
                    return oResultStatus;
            }
            catch (Exception ex)
            {
                ResultStatus oResultStatus = new ResultStatus(ex.Message, false);
                return oResultStatus;
            }

        } // TransferAll

        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                ResetAllFields();
            }
        }
    }
}