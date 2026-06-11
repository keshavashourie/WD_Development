/* Copyright 2019 Siemens */
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

/// <summary>
/// Summary description for SS_LotEquipmentReservation
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_LotEquipmentReservation : scsShopfloorBase
    {
        #region Properties

        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("SelectionIdField") as CWC.TextBox; } }
        protected CWC.TextBox _txtSelectedLotsList { get { return Page.FindCamstarControl("LotEquipmentReservation_SelectedLots") as CWC.TextBox; } }
        protected CWC.TextBox txtComputerName { get { return Page.FindCamstarControl("ComputerNameField") as CWC.TextBox; } }
        protected JQDataGrid _gridContainers { get { return Page.FindCamstarControl("ContainersField") as JQDataGrid; } }
        protected JQDataGrid _gridLotReservedEquipments { get { return Page.FindCamstarControl("CurrentLotsReservedEquipmentGridField") as JQDataGrid; } }
        protected JQDataGrid _gridEquipmentReservedLots { get { return Page.FindCamstarControl("CurrentEquipmentReservedLotsGridField") as JQDataGrid; } }
        protected CWC.NamedSubentity _subStep { get { return Page.FindCamstarControl("LotEquipmentReservation_Step") as CWC.NamedSubentity; } }
        protected CWC.NamedObject _ndoEquipment { get { return Page.FindCamstarControl("LotEquipmentReservation_Equipment") as CWC.NamedObject; } }
        private SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }

        // class level variables/properties
        protected enum FetchTxnDataEvents { GetDetailsAndStep, GetEquipmentSelection };

        #endregion

        #region Page Events

        private void FetchTxnData(FetchTxnDataEvents EventName)
        {
            try
            {
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                LotEquipmentReservationService oService = new LotEquipmentReservationService(fs.CurrentUserProfile);
                LotEquipmentReservation oServiceData = new LotEquipmentReservation();
                LotEquipmentReservation_Info oServiceInfo = new LotEquipmentReservation_Info();
                LotEquipmentReservation_Request oRequest = new LotEquipmentReservation_Request();
                LotEquipmentReservation_Result oResult = new LotEquipmentReservation_Result();
                ResultStatus oResultStatus = new ResultStatus();
                int iCurrentLotCount = 0;
                string strFirstContainerName = "";

                if (_txtSelectedLotsList.Data != null)
                {
                    iCurrentLotCount = _gridContainers.BoundContext.GetTotalRows();

                    // this logic is solely to detect if the Lot column's ID is fully upper case or mixed case
                    // when running against an Oracle DB, there are occurances that the ID is fully upper case 
                    string sLotColumnIdentifier = "Lot";
                    sLotColumnIdentifier = SEMI.AppCode.GridUtility.GetCasedColumnHeaderName((_gridContainers.GridContext as BoundContext).Fields, sLotColumnIdentifier);
                    

                    // Get first container
                    if (iCurrentLotCount > 0)
                        strFirstContainerName = _gridContainers.GridContext.GetCell(0.ToString().PadLeft(6, '0'), sLotColumnIdentifier).ToString();                   

                    if (EventName == FetchTxnDataEvents.GetDetailsAndStep)
                    {
                        //  Prepare the request
                        oServiceData.SelectedLots = _txtSelectedLotsList.TextControl.Text;

                        if (iCurrentLotCount == 1)
                        {
                            oServiceData.Container = new ContainerRef();
                            oServiceData.Container.Name = strFirstContainerName;
                            oServiceInfo.Step = FieldInfoUtil.RequestSelectionValue();
                        }

                        oServiceInfo.Details = new LotEquipmentReservationDetails_Info();
                        oServiceInfo.Details.Container = FieldInfoUtil.RequestValue();
                        oServiceInfo.Details.Step = FieldInfoUtil.RequestValue();
                        oServiceInfo.Details.ReservedEquipment = FieldInfoUtil.RequestValue();
                        oServiceInfo.Details.IsMandatory = FieldInfoUtil.RequestValue();
                        oServiceInfo.Details.ReservedBy = FieldInfoUtil.RequestValue();
                        oServiceInfo.Details.ReservedDate = FieldInfoUtil.RequestValue();

                    }

                    if (EventName == FetchTxnDataEvents.GetEquipmentSelection)
                    {						
                        oServiceData.Container = new ContainerRef();
                        oServiceData.Container.Name = strFirstContainerName;
                        oServiceData.Step = new NamedSubentityRef();
                        oServiceData.Step.Name = _subStep.TextEditControl.Text;
                        oServiceInfo.EquipmentSelection = FieldInfoUtil.RequestValue();
                    }

                    oRequest.Info = oServiceInfo;

                    oResultStatus = oService.GetEnvironment(oServiceData, oRequest, out oResult);

                    if (oResultStatus.IsSuccess)
                    {
                        var oResultValue = oResult.Value as LotEquipmentReservation;

                        if (oResult.Value.Details != null)
                        {
                            //bind to the regular grid
                            _gridLotReservedEquipments.ClearData();
                            _gridLotReservedEquipments.Data = oResult.Value.Details;
                            CamstarWebControl.SetRenderToClient(_gridLotReservedEquipments);
                        }

                        if (oResult.Value.Step != null)
                        {
                            _subStep.SetSelectionValues(oResult.Environment.Step.SelectionValues);
                        }

                        if (oResultValue.EquipmentSelection != null)
                        {
                            // clear the Equipment selection data
							_ndoEquipment.ClearData();
                            _ndoEquipment.ClearSelectionValues();
                            CWC.NamedObject _ndoTrackInEquipTemp = _ndoEquipment;
                            SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoTrackInEquipTemp, oResultValue.EquipmentSelection);
                        }
                    }
                    else
                    {
                        DisplayMessage(oResultStatus);
                    }
                }
            }
            catch (Exception Ex)
            {
                DisplayMessage(new ResultStatus((Ex.TargetSite.Name + ("(): " + Ex.Message)), false));
            }
        }

        private bool ValidateContainers(string NewLotName)
        {
            try
            {
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                LotEquipmentReservationService oService = new LotEquipmentReservationService(fs.CurrentUserProfile);
                LotEquipmentReservation oServiceData = new LotEquipmentReservation();
                LotEquipmentReservation_Info oServiceInfo = new LotEquipmentReservation_Info();
                LotEquipmentReservation_Request oRequest = new LotEquipmentReservation_Request();
                LotEquipmentReservation_Result oResult = new LotEquipmentReservation_Result();
                ResultStatus oResultStatus = new ResultStatus();

                int intTotalContainers = _gridContainers.BoundContext.GetTotalRows();

                if (intTotalContainers > 0)
                {
                    oServiceData.Containers = new ContainerRef[intTotalContainers + 1];

					// this logic is solely to detect if the Lot column's ID is fully upper case or mixed case
                    // when running against an Oracle DB, there are occurances that the ID is fully upper case 
                    string sLotColumnIdentifier = "Lot";
                    sLotColumnIdentifier = SEMI.AppCode.GridUtility.GetCasedColumnHeaderName((_gridContainers.GridContext as BoundContext).Fields, sLotColumnIdentifier);     

                    // collect the containers
                    for (int x = 0; x < intTotalContainers; x++)
                    {
                        oServiceData.Containers[x] = new ContainerRef();
						oServiceData.Containers[x].Name = _gridContainers.GridContext.GetCell(x.ToString().PadLeft(6, '0'), sLotColumnIdentifier).ToString();
                    }

                    oServiceData.Containers[intTotalContainers] = new ContainerRef();
                    oServiceData.Containers[intTotalContainers].Name = NewLotName;

                    // execute the CLF
                    oResultStatus = oService.LotEquipmentReservation_ValidateLot(oServiceData, oRequest, out oResult);

                    if (oResultStatus.IsSuccess)
                    {
                        return true;

                    }
                    else
                    {
                        DisplayMessage(oResultStatus);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception Ex)
            {
                DisplayMessage(new ResultStatus((Ex.TargetSite.Name + ("(): " + Ex.Message)), false));
                return false;
            }
        }

        private void GetSelectedLotsListInStringFormat(string LotName)
        {
            if (_txtSelectedLotsList.Data == null)
                _txtSelectedLotsList.Data = LotName;
            else
                _txtSelectedLotsList.Data = _txtSelectedLotsList.Data + "," + LotName;
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
                    LotEquipmentReservationService objSvc = new LotEquipmentReservationService(fs.CurrentUserProfile);
                    LotEquipmentReservation objSvcData = new LotEquipmentReservation { SelectionId = (string)_txtSelectionId.Data };
                    LotEquipmentReservation_Info objSvcInfo = new LotEquipmentReservation_Info { Containers = FieldInfoUtil.RequestValue() };

                    // init the result object
                    LotEquipmentReservation_Result objResult = new LotEquipmentReservation_Result();

                    // execute to request the value
                    ResultStatus resultStatus = objSvc.ResolveSelectionId(objSvcData, new LotEquipmentReservation_Request { Info = objSvcInfo }, out objResult);

                    if (resultStatus.IsSuccess)
                    {
                        JQDataGrid _gridContainers = Page.FindCamstarControl("ContainersField") as JQDataGrid;
                        foreach (OM.ContainerRef oContainer in objResult.Value.Containers)
                        {
                            if (ValidateContainers(oContainer.Name.ToString()))
                            {
                              
                                // check if lot not exist in the grid else add to the grid
                                string strSelectedGridId = (_gridContainers.GridContext as BoundContext).GetRowIdByCellValue("Lot", oContainer.Name.ToString());

                                if (string.IsNullOrEmpty(strSelectedGridId))
                                {
                                    SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "LotEquipmentReservation", oContainer.Name.ToString(), false, ref _gridContainers, "ContainersField", true);

                                    GetSelectedLotsListInStringFormat(oContainer.Name.ToString());
                                }
                            }
                        }

                        FetchTxnData(FetchTxnDataEvents.GetDetailsAndStep);
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
            base.GetInputData(serviceData);

            if (_gridContainers.Data != null)
            {
                OM.LotEquipmentReservation svcData = serviceData as OM.LotEquipmentReservation;

                int intTotalContainers = _gridContainers.BoundContext.GetTotalRows();

                svcData.Containers = new ContainerRef[intTotalContainers];

				// this logic is solely to detect if the Lot column's ID is fully upper case or mixed case
				// when running against an Oracle DB, there are occurances that the ID is fully upper case 
                string sLotColumnIdentifier = "Lot";
                sLotColumnIdentifier= SEMI.AppCode.GridUtility.GetCasedColumnHeaderName((_gridContainers.GridContext as BoundContext).Fields, sLotColumnIdentifier);

                // collect the containers
                for (int x = 0; x < intTotalContainers; x++)
                {
                    svcData.Containers[x] = new ContainerRef();
					svcData.Containers[x].Name = _gridContainers.GridContext.GetCell(x.ToString().PadLeft(6, '0'), sLotColumnIdentifier).ToString();
                }
            }
        }

        void _subStep_DataChanged(object sender, EventArgs e)
        {
			if (_subStep.TextEditControl.Text != "")
			{
				FetchTxnData(FetchTxnDataEvents.GetEquipmentSelection);
			}
			else
			{
				// clear the Equipment selection data
				_ndoEquipment.ClearData();
				_ndoEquipment.ClearSelectionValues();
			}
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);

            _subStep.DataChanged += new EventHandler(_subStep_DataChanged); 

            if (Page.IsPostBack)
                // Check if it is a pop up close, get the return result
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {
                    if (_envSelectedLots != null)
                        // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                        if (Page.DataContract.GetValueByName("LotEquipmentReservation_SelectedLotsListDM") != null)
                            _envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("LotEquipmentReservation_SelectedLotsListDM") as string[];

                    if (_envSelectedLots.SS_ContainersList != null)
                    {
                        string[] sContainers;
                        sContainers = _envSelectedLots.SS_ContainersList;

                        JQDataGrid _gridContainers = Page.FindCamstarControl("ContainersField") as JQDataGrid;
                        foreach (string sContainersItem in sContainers)
                        {
                            // check if lot not exist in the grid else add to the grid
                            string strSelectedGridId = (_gridContainers.GridContext as BoundContext).GetRowIdByCellValue("Lot", sContainersItem);

                            if (string.IsNullOrEmpty(strSelectedGridId))
                            {
                                if (ValidateContainers(sContainersItem))
                                {
                                    SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "LotEquipmentReservation", sContainersItem, false, ref _gridContainers, "ContainersField", true);

                                    GetSelectedLotsListInStringFormat(sContainersItem);
                                }
                            }
                        }

                        FetchTxnData(FetchTxnDataEvents.GetDetailsAndStep);
                        //nullify the containers list
                        _envSelectedLots.SS_ContainersList = null;
                    }
                }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Reset")
            {
                Page.ClearValues();
                _gridContainers.ClearData();
                _gridLotReservedEquipments.ClearData();
                _gridEquipmentReservedLots.ClearData();				
                _ndoEquipment.ClearData();
                _ndoEquipment.ClearSelectionValues();
                _subStep.ClearData();
                _subStep.ClearSelectionValues();
                _txtSelectedLotsList.ClearData();
                _txtSelectionId.Focus();
            }
        }

        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                Page.ClearValues();
                _gridContainers.ClearData();
                _txtSelectedLotsList.ClearData();
                _txtSelectionId.Focus();
            }
        }

        #endregion

    }
}



