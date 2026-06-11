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
/// Summary description for SS_LotBinsSplit
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_LotBinsSplit : scsShopfloorBase
    {
        #region Properties

        // TextBoxs
        CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("ComputerNameField") as CWC.TextBox; } }
        CWC.TextBox _txtLotBinsSplitContainer { get { return Page.FindCamstarControl("LotBinsSplit_Container") as CWC.TextBox; } }
        CWC.TextBox _txtContainerField { get { return Page.FindCamstarControl("SelectionIdField") as CWC.TextBox; } }
        CWC.TextBox _txtServiceTypeField { get { return Page.FindCamstarControl("ServiceType") as CWC.TextBox; } }
        // JQDataGrids
        JQDataGrid _gridBinsToSplitFields { get { return Page.FindCamstarControl("BinsToSplitFields") as JQDataGrid; } }
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
                            if (Page.DataContract.GetValueByName("LotBinsSplit_HiddenSelection_DM") != null)
                                _envObject.SS_ContainersList = Page.DataContract.GetValueByName("LotBinsSplit_HiddenSelection_DM") as string[];

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

                LotBinsSplitService oService = new LotBinsSplitService(fs.CurrentUserProfile);
                LotBinsSplit oServiceData = new LotBinsSplit();
                LotBinsSplit_Info oServiceInfo = new LotBinsSplit_Info();
                ResultStatus oResultStatus = new ResultStatus();

                LotBinsSplitService lService = new LotBinsSplitService(fs.CurrentUserProfile);
                LotBinsSplit lServiceData = new LotBinsSplit();
                LotBinsSplit_Info lServiceInfo = new LotBinsSplit_Info();
                ResultStatus lResultStatus = new ResultStatus();

                oServiceData.SelectionId = sContainerName;
                oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
                oServiceInfo.BinsToSplitSelection = new LotBinsDetails_Info();

                // Set the request
                LotBinsSplit_Request oServiceRequest = new LotBinsSplit_Request();
                oServiceRequest.Info = oServiceInfo;

                // Set the status
                LotBinsSplit_Result oServiceResult = new LotBinsSplit_Result();

                ResultStatus objRS = oService.ResolveSelectionId(oServiceData, oServiceRequest, out oServiceResult);

                if (objRS.IsSuccess)
                {
                    // Get SelectionContainer
                    string strContainer = oServiceResult.Value.SelectionContainer.Name.ToString();
                    _txtLotBinsSplitContainer.Data = strContainer;

                    lServiceData.SelectionId = strContainer;
                    lServiceInfo.BinsToSplitSelection = new LotBinsDetails_Info();
                    lServiceInfo.BinsToSplitSelection.Bin = FieldInfoUtil.RequestInfo(true, false, true, true);
                    lServiceInfo.BinsToSplitSelection.BinQty = FieldInfoUtil.RequestInfo(true, false, true, true);
                    lServiceInfo.BinsToSplitSelection.BinCategory = FieldInfoUtil.RequestInfo(true, false, true, true);
                    lServiceInfo.BinsToSplitSelection.BinLotId = FieldInfoUtil.RequestInfo(true, false, true, true);
                    lServiceInfo.BinsToSplitSelection.BinProduct = FieldInfoUtil.RequestInfo(true, true, true, false);
                    lServiceInfo.BinsToSplitSelection.WaferScribeNumber = FieldInfoUtil.RequestInfo(true, false, true, true);
                    lServiceInfo.BinsToSplitSelection.ToWaferScribeNumber = FieldInfoUtil.RequestInfo(true, false, true, true);
                    lServiceInfo.IsWaferProcessing = FieldInfoUtil.RequestValue();

                    // Set the request
                    LotBinsSplit_Request lServiceRequest = new LotBinsSplit_Request();
                    lServiceRequest.Info = lServiceInfo;

                    // Set the status
                    LotBinsSplit_Result lServiceResult = new LotBinsSplit_Result();

                    // execute the request selection values
                    ResultStatus lobjRS = lService.ResolveSelectionId(lServiceData, lServiceRequest, out lServiceResult);


                    if (lobjRS.IsSuccess)
                    {
                        // get single selection data for _gridContainersFields
                        SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "LotBinsSplit", strContainer, true, ref _gridContainersFields);

                        // bind the results to the grid.
                        _gridBinsToSplitFields.ClearData();
                        (_gridBinsToSplitFields.GridContext as BoundContext).Data = lServiceResult.Value.BinsToSplitSelection;
                        _gridBinsToSplitFields.BoundContext.LoadData();
                        CamstarWebControl.SetRenderToClient(_gridBinsToSplitFields);

                        // column visibility for WaferScribeNumber & ToWaferScribeNumber
                        if (lServiceResult.Value.IsWaferProcessing == false)
                        {
                            _gridBinsToSplitFields.BoundContext.Fields["WaferScribeNumber"].Visible = false;
                            _gridBinsToSplitFields.BoundContext.Fields["ToWaferScribeNumber"].Visible = false;
                        }
                    }
                }
                else
                {
                    DisplayMessage(objRS);
                    // clear data if invalid selectionId
                    _txtLotBinsSplitContainer.ClearData();
                    _gridContainersFields.ClearData();
                    _gridBinsToSplitFields.ClearData();
                }
            }
        }

        public override void GetInputData(Service serviceData)
        {
            try
            {
                base.GetInputData(serviceData);

                if (_txtLotBinsSplitContainer.Data != null)
                {
                    OM.LotBinsSplit objSvcData = serviceData as OM.LotBinsSplit;
                    objSvcData.Container = new ContainerRef();
                    objSvcData.Container.Name = _txtLotBinsSplitContainer.Data.ToString();

                    if (_gridBinsToSplitFields.GridContext.SelectedRowIDs != null)
                    {
                        // set selected row ID to an array
                        string[] strRowId = _gridBinsToSplitFields.GridContext.SelectedRowIDs.ToArray();

                        // get the total number of Bin in the grid
                        int intTotalBins = _gridBinsToSplitFields.GridContext.SelectedRowIDs.Count;

                        // init the parameters object
                        LotBinsDetails[] objBins = new LotBinsDetails[intTotalBins];

                        // loop thru the selected parameters in the BinsToSplit grid and get the info to set in the parameters object.
                        for (int x = 0; x < intTotalBins; x++)
                        {
                            _gridBinsToSplitFields.GridContext.SelectRow(strRowId[x], true);
                            objBins[x] = new LotBinsDetails();
                            objBins[x].Bin = _gridBinsToSplitFields.GridContext.GetCell(strRowId[x], "Bin").ToString();
                            objBins[x].BinQty = Int32.Parse(_gridBinsToSplitFields.GridContext.GetCell(strRowId[x], "BinQty").ToString());
							string binCat = _gridBinsToSplitFields.GridContext.GetCell(strRowId[x], "BinCategory").ToString();
							//objBins[x].BinCategory = new Enumeration<BinCategoryEnum, string>(binCat);
                            //objBins[x].BinCategory.Value = ;
                            if (_gridBinsToSplitFields.GridContext.GetCell(strRowId[x], "BinLotId") != null)
                                objBins[x].BinLotId = _gridBinsToSplitFields.GridContext.GetCell(strRowId[x], "BinLotId").ToString();

                            if (_gridBinsToSplitFields.GridContext.GetCell(strRowId[x], "BinProduct").ToString() != "")
                            {
                                string[] BinProduct = (_gridBinsToSplitFields.GridContext.GetCell(strRowId[x], "BinProduct").ToString()).Split(':');
                                objBins[x].BinProduct = new RevisionedObjectRef();
                                objBins[x].BinProduct.Name = BinProduct[0];
                                objBins[x].BinProduct.Revision = BinProduct[1];
                            }
                            if (_gridBinsToSplitFields.GridContext.GetCell(strRowId[x], "WaferScribeNumber") != null)
                                objBins[x].WaferScribeNumber = _gridBinsToSplitFields.GridContext.GetCell(strRowId[x], "WaferScribeNumber").ToString();

                            if (_gridBinsToSplitFields.GridContext.GetCell(strRowId[x], "ToWaferScribeNumber") != null)
                                objBins[x].ToWaferScribeNumber = _gridBinsToSplitFields.GridContext.GetCell(strRowId[x], "ToWaferScribeNumber").ToString();
                        }
                        objSvcData.BinsToSplit = objBins;
                    }
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
                Page.ClearValues();
                _gridContainersFields.ClearData();
            }
        }
    }
}



