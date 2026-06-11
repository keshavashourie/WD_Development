/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using SEMI.AppCode;

/// <summary>
/// Summary description for SS_LotAttributes
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_HoldLocations : MatrixWebPart
    {
        #region Properties

        // TextBoxs
        CWC.TextBox _txtSelectionIdField { get { return Page.FindCamstarControl("HoldLocations_SelectionId") as CWC.TextBox; } }
        CWC.TextBox _txtLotIdField { get { return Page.FindCamstarControl("HoldLocations_LotId") as CWC.TextBox; } }
        CWC.TextBox _txtPrimaryService { get { return Page.FindCamstarControl("HoldLocations_PrimaryService") as CWC.TextBox; } }        
        // JQDataGrids
        JQDataGrid _gridHoldLocations { get { return Page.FindCamstarControl("HoldLocations_Locations") as JQDataGrid; } }
        // DataEnvelop
        protected DataEnvelopControl _envDataCollection { get { return Page.FindCamstarControl("LotHoldLocations") as DataEnvelopControl; } }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                FetchData();
                HideOKButton();
            }
        }

        public void FetchData()
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                if (_txtPrimaryService.TextControl.Text == "LotHold")
                {
                    // init service objects
                    LotHoldService oService = new LotHoldService(fs.CurrentUserProfile);
                    LotHold oServiceData = new LotHold();
                    LotHold_Info oServiceInfo = new LotHold_Info();
                    LotHold_Result oServiceResult = new LotHold_Result();

                    if (_txtLotIdField.Data == null)
                    {
                        oServiceData.Container = new ContainerRef();
                        oServiceData.Container.Name = _txtSelectionIdField.Data.ToString();                        
                    }
                    else
                    {
                        oServiceData.Container = new ContainerRef();                        
                        oServiceData.Container.Name = _txtLotIdField.Data.ToString();                        
                    }

                    oServiceInfo.Container = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations = new LotHoldLocations_Info();
                    oServiceInfo.LotHoldLocations.Spec = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations.HoldReason = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations.HoldLocation = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations.HoldCount = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations.ExpectedHoldDays = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations.HoldUsername = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations.StartShelfLocation = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations.StartComments = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations.StartUsername = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations.StartTimestamp = FieldInfoUtil.RequestValue();

                    // init request
                    LotHold_Request oServiceRequest = new LotHold_Request();
                    oServiceRequest.Info = oServiceInfo;
                    // execute!
                    ResultStatus oResultStatus = oService.GetEnvironment(oServiceData, oServiceRequest, out oServiceResult);

                    // execute the request selection values
                    ResultStatus objRS = oService.ResolveSelectionId(oServiceData, oServiceRequest, out oServiceResult);

                    if (objRS.IsSuccess)
                    {
                        if (_txtLotIdField.Data == null)
                            _txtLotIdField.Data = oServiceResult.Value.Container.Name.ToString();

                        if (_txtSelectionIdField.Data == null)
                            _txtSelectionIdField.Data = oServiceResult.Value.Container.Name.ToString();

                        // bind the results to the grid.
                        _gridHoldLocations.ClearData();
                        (_gridHoldLocations.GridContext as BoundContext).Data = oServiceResult.Value.LotHoldLocations.ToArray();
                        _gridHoldLocations.BoundContext.LoadData();
                        CamstarWebControl.SetRenderToClient(_gridHoldLocations);
                    }
                }
                else if (_txtPrimaryService.TextControl.Text == "LotRelease")
                {
                    // init service objects
                    LotReleaseService oService = new LotReleaseService(fs.CurrentUserProfile);
                    LotRelease oServiceData = new LotRelease();
                    LotRelease_Info oServiceInfo = new LotRelease_Info();
                    LotRelease_Result oServiceResult = new LotRelease_Result();

                    if (_txtLotIdField.Data == null)
                    {
                        oServiceData.Container = new ContainerRef();
                        oServiceData.Container.Name = _txtSelectionIdField.Data.ToString();
                    }
                    else
                    {
                        oServiceData.Container = new ContainerRef();
                        oServiceData.Container.Name = _txtLotIdField.Data.ToString();
                    }

                    oServiceInfo.Container = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations = new LotHoldLocations_Info();
                    oServiceInfo.LotHoldLocations.Spec = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations.HoldReason = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations.HoldLocation = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations.HoldCount = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations.ExpectedHoldDays = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations.HoldUsername = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations.StartShelfLocation = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations.StartComments = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations.StartUsername = FieldInfoUtil.RequestValue();
                    oServiceInfo.LotHoldLocations.StartTimestamp = FieldInfoUtil.RequestValue();                    

                    // init request
                    LotRelease_Request oServiceRequest = new LotRelease_Request();
                    oServiceRequest.Info = oServiceInfo;
                    // execute!
                    ResultStatus oResultStatus = oService.GetEnvironment(oServiceData, oServiceRequest, out oServiceResult);

                    // execute the request selection values
                    ResultStatus objRS = oService.ResolveSelectionId(oServiceData, oServiceRequest, out oServiceResult);

                    if (objRS.IsSuccess)
                    {
                        if (_txtLotIdField.Data == null)
                            _txtLotIdField.Data = oServiceResult.Value.Container.Name.ToString();

                        if (_txtSelectionIdField.Data == null)
                            _txtSelectionIdField.Data = oServiceResult.Value.Container.Name.ToString();

                        // bind the results to the grid.
                        _gridHoldLocations.ClearData();
                        (_gridHoldLocations.GridContext as BoundContext).Data = oServiceResult.Value.LotHoldLocations.ToArray();
                        _gridHoldLocations.BoundContext.LoadData();
                        CamstarWebControl.SetRenderToClient(_gridHoldLocations);
                    }

                    if (Page.DataContract.GetValueByName("LocationsPopup_LotReleaseDetails_DM") != null)
                    {
                        LotReleaseDetails[] currentDetails = Page.DataContract.GetValueByName("LocationsPopup_LotReleaseDetails_DM") as LotReleaseDetails[];
                        int locationsCount = oServiceResult.Value.LotHoldLocations.Count();
                        for (int i = 0; i < locationsCount; i++)
                        {
                            foreach (ReleaseLotDetails detail in currentDetails[0].ServiceDetails)
                            {
                                if (oServiceResult.Value.LotHoldLocations[i].Self.ID == detail.LotHoldLocationsItem.ID)
                                {
                                    _gridHoldLocations.SelectedRowID = i.ToString("000000");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        //-----------------------------------------
        // On Popup closed event
        //-----------------------------------------
        public void ClosePopup()
        {
            try
            {
                LotHoldLocations[] currentLocations = _gridHoldLocations.Data as LotHoldLocations[];
                var newWaferMaps = new List<LotHoldLocations>();
                if (_gridHoldLocations.SelectedRowID != null)
                {
                    foreach (string selectedRowId in _gridHoldLocations.SelectedRowIDs)
                    {
                        LotHoldLocations newLocation = new LotHoldLocations();
                        newLocation = currentLocations[Convert.ToInt32(selectedRowId)];
                        newWaferMaps.Add(newLocation);
                    }
                }
                _envDataCollection.SS_LotHoldLocations = newWaferMaps.ToArray();
                Page.CollectDataContractByName("LocationsPopup_LotHoldLocations_DM");
                Page.DistributeDataContract();
                Page.CloseFloatingFrameOnSubmit(new ResultStatus());
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //---------------------------------------------------
        // Web part custom action
        //---------------------------------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "Submit":
                        {
                            ClosePopup();
                            break;
                        }
                }
            }
        }

        //---------------------------------------------------
        // Hide OK Button
        //---------------------------------------------------
        public void HideOKButton()
        {
            string OKButton = "Submit";
            if (Page.DataContract.GetValueByName("LocationsPopup_IsLocationSelection_DM") != null)
                this.Page.ActionDispatcher.GetActionByName(OKButton).IsHidden = false;
            else
                this.Page.ActionDispatcher.GetActionByName(OKButton).IsHidden = true;
        }
    }
}



