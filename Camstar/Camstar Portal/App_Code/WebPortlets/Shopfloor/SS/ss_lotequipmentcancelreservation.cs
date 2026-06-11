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
using Camstar.WebPortal.FormsFramework.WebControls;
using SEMI.AppCode;

/// <summary>
/// Summary description for SS_LotEquipmentCancelReservation
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_LotEquipmentCancelReservation : scsShopfloorBase
    {
        #region Properties

        // TextBoxs
        CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("ComputerNameField") as CWC.TextBox; } }
        CWC.TextBox _txtLotEquipmentCancelReservationContainer { get { return Page.FindCamstarControl("LotEquipmentCancelReservation_Container") as CWC.TextBox; } }
        CWC.TextBox _txtContainerField { get { return Page.FindCamstarControl("SelectionIdField") as CWC.TextBox; } }
        CWC.TextBox _txtServiceTypeField { get { return Page.FindCamstarControl("ServiceType") as CWC.TextBox; } }
        // JQDataGrids
        JQDataGrid _gridReservedEquipmentsFields { get { return Page.FindCamstarControl("ReservedEquipmentsFields") as JQDataGrid; } }
        JQDataGrid _gridContainersFields { get { return Page.FindCamstarControl("ContainersFields") as JQDataGrid; } }
        // Buttons
        CWC.Button _btnSelection { get { return Page.FindCamstarControl("SelectionButton") as CWC.Button; } }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (!Page.IsPostBack)
                    _txtComputerNameField.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
                else
                {
                    if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                    {
                        DataEnvelopControl _envObject = Page.FindCamstarControl("HiddenSelection") as DataEnvelopControl;
                        if (_envObject != null)
                            // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                            if (Page.DataContract.GetValueByName("LotEquipmentCancelReservation_HiddenSelection_DM") != null)
                                _envObject.SS_ContainersList = Page.DataContract.GetValueByName("LotEquipmentCancelReservation_HiddenSelection_DM") as string[];

                        if (_envObject.SS_ContainersList != null)
                        {
                            string[] sContainers;
                            sContainers = _envObject.SS_ContainersList;
                            //nullify the containers list
                            _envObject.SS_ContainersList = null;

                            _txtContainerField.Data = sContainers[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            _txtServiceTypeField.Data = this.PrimaryServiceType.ToString();
        }

        public void SelectionIdField_DataChanged(string sContainerName)
        {
            if (sContainerName != null)
            {
                JQDataGrid _gridContainersFields = FindCamstarControl("ContainersFields") as JQDataGrid;

                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                LotEquipmentCancelReservationService oService = new LotEquipmentCancelReservationService(fs.CurrentUserProfile);
                LotEquipmentCancelReservation oServiceData = new LotEquipmentCancelReservation();
                LotEquipmentCancelReservation_Info oServiceInfo = new LotEquipmentCancelReservation_Info();
                ResultStatus oResultStatus = new ResultStatus();

                LotEquipmentCancelReservationService lService = new LotEquipmentCancelReservationService(fs.CurrentUserProfile);
                LotEquipmentCancelReservation lServiceData = new LotEquipmentCancelReservation();
                LotEquipmentCancelReservation_Info lServiceInfo = new LotEquipmentCancelReservation_Info();
                ResultStatus lResultStatus = new ResultStatus();

                oServiceData.SelectionId = sContainerName;
                oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
                oServiceInfo.Details = new LotEquipmentReservationDetails_Info();

                // Set the request
                LotEquipmentCancelReservation_Request oServiceRequest = new LotEquipmentCancelReservation_Request();
                oServiceRequest.Info = oServiceInfo;

                // Set the status
                LotEquipmentCancelReservation_Result oServiceResult = new LotEquipmentCancelReservation_Result();

                ResultStatus objRS = oService.ResolveSelectionId(oServiceData, oServiceRequest, out oServiceResult);

                if (objRS.IsSuccess)
                {
                    string strContainer = "";
                    // Get SelectionContainer
                    if (oServiceResult.Value.SelectionContainer != null)
                    {
                         strContainer = oServiceResult.Value.SelectionContainer.Name.ToString();
                        _txtLotEquipmentCancelReservationContainer.Data = strContainer;

                        lServiceData.SelectionId = strContainer;
                        lServiceInfo.Details = new LotEquipmentReservationDetails_Info();
                        lServiceInfo.Details.Container = FieldInfoUtil.RequestValue();
                        lServiceInfo.Details.Step = FieldInfoUtil.RequestValue();
                        lServiceInfo.Details.ReservedEquipment = FieldInfoUtil.RequestValue();
                        lServiceInfo.Details.IsMandatory = FieldInfoUtil.RequestValue();
                        lServiceInfo.Details.ReservedBy = FieldInfoUtil.RequestValue();
                        lServiceInfo.Details.ReservedDate = FieldInfoUtil.RequestValue();

                        // Set the request
                        LotEquipmentCancelReservation_Request lServiceRequest = new LotEquipmentCancelReservation_Request();
                        lServiceRequest.Info = lServiceInfo;

                        // Set the status
                        LotEquipmentCancelReservation_Result lServiceResult = new LotEquipmentCancelReservation_Result();

                        // execute the request selection values
                        ResultStatus lobjRS = lService.ResolveSelectionId(lServiceData, lServiceRequest, out lServiceResult);

                        if (lobjRS.IsSuccess)
                        {
                            // get single selection data for _gridContainersFields
                            SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "LotEquipmentCancelReservation", strContainer, true, ref _gridContainersFields);

                            // bind the results to the grid.
                            _gridReservedEquipmentsFields.ClearData();
                            (_gridReservedEquipmentsFields.GridContext as BoundContext).Data = lServiceResult.Value.Details;
                            _gridReservedEquipmentsFields.BoundContext.LoadData();
                            CamstarWebControl.SetRenderToClient(_gridReservedEquipmentsFields);
                        }
                    }
                }
                else
                {
                    DisplayMessage(objRS);
                    // clear data if invalid selectionId
                    _txtLotEquipmentCancelReservationContainer.ClearData();
                    _gridContainersFields.ClearData();
                    _gridReservedEquipmentsFields.ClearData();
                }
            }
        }

        public override void GetInputData(Service serviceData)
        {
            try
            {
                base.GetInputData(serviceData);

                if (_txtLotEquipmentCancelReservationContainer.Data != null)
                {
                    OM.LotEquipmentCancelReservation objSvcData = serviceData as OM.LotEquipmentCancelReservation;
                    objSvcData.Container = new ContainerRef();
                    objSvcData.Container.Name = _txtLotEquipmentCancelReservationContainer.Data.ToString();

                    // get the total number of reserved equipment in the grid
                    int intTotalRow = _gridReservedEquipmentsFields.BoundContext.GetTotalRows();

                    // init the parameters object
                    LotEquipmentReservationDetails[] objReservedEqp = new LotEquipmentReservationDetails[intTotalRow];

                    // loop thru the selected parameters in the Reserved Equipment grid and get the info to set in the parameters object.
                    for (int x = 0; x < intTotalRow; x++)
                    {
                        string getStep = _gridReservedEquipmentsFields.GridContext.GetCell(x, "Step").ToString();
                        objReservedEqp[x] = new LotEquipmentReservationDetails();

                        objReservedEqp[x].Container = new ContainerRef();
                        objReservedEqp[x].Container.Name = _txtLotEquipmentCancelReservationContainer.Data.ToString();
                        objReservedEqp[x].Step = new NamedSubentityRef();
                        objReservedEqp[x].Step.Name = getStep.Split(':').Last();
                        objReservedEqp[x].ReservedEquipment = new NamedObjectRef();
                        objReservedEqp[x].ReservedEquipment.Name = _gridReservedEquipmentsFields.GridContext.GetCell(x, "ReservedEquipment").ToString();
                    }
					
                    objSvcData.ReservedEquipments = objReservedEqp;
					
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            if (action != null && action.Parameters == "Reset")
            {
                Page.ShopfloorReset(sender, e);
                _gridContainersFields.ClearData();
            }
        }

        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                Page.ClearValues();
                _gridContainersFields.ClearData();
            }
        }
    }
}



