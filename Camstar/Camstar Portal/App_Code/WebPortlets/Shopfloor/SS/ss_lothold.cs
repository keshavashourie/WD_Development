/* Copyright 2025 Siemens */
using System;
using System.Data;
using System.Web;
using System.Linq;
using System.Collections.Generic;

using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.PortalFramework;

using SEMI.AppCode;
using System.Web.UI.WebControls;

/// <summary>
/// Summary description for Class1
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_LotHold : scsShopfloorBase
    {
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotHold_SelectionId") as CWC.TextBox; } }
        protected CWC.TextBox _txtSelectedLot { get { return Page.FindCamstarControl("LotHold_ContainerName") as CWC.TextBox; } }
        protected CWC.TextBox _txtComments { get { return Page.FindCamstarControl("LotHold_Comments") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("LotHold_Employee") as CWC.NamedObject; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("LotHold_ComputerName") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoHoldLocation { get { return Page.FindCamstarControl("LotHold_HoldLocation") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoHoldReason { get { return Page.FindCamstarControl("LotHold_HoldReason") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEmailGroup { get { return Page.FindCamstarControl("LotHold_EmailGroup") as CWC.NamedObject; } }
        protected CWC.TextBox _txtShelfLocation { get { return Page.FindCamstarControl("LotHold_ShelfLocation") as CWC.TextBox; } }
        protected CWC.DropDownList _drpLocationProxy { get { return Page.FindCamstarControl("LotHold_LocationProxy") as CWC.DropDownList; } }
        protected CWC.Button _btnFailures { get { return Page.FindCamstarControl("FailuresPop") as CWC.Button; } }
        protected CWC.Button _btnMatReq { get { return Page.FindCamstarControl("MatReqPop") as CWC.Button; } }
        protected CWC.Button _btnDocSet { get { return Page.FindCamstarControl("DocSetPop") as CWC.Button; } }
        protected CWC.Button _btnLocations { get { return Page.FindCamstarControl("HoldLocationsPop") as CWC.Button; } }
        protected CWC.Button _btnOnlineTraveler { get { return Page.FindCamstarControl("OnlineTravelerPop") as CWC.Button; } }
        protected JQDataGrid _gridDetails { get { return Page.FindCamstarControl("LotHold_Details") as JQDataGrid; } }
        protected SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("LotHold_Lots") as SEMI.AppCode.DataEnvelopControl; } }
        protected ContainerListGrid _container { get { return Page.FindCamstarControl("LotHold_ContainerSelection") as ContainerListGrid; } }
        protected ToggleContainer _toggleComments { get { return Page.FindCamstarControl("LotHold_Toggle") as ToggleContainer; } }

        public void SelectionId_DataChanged(Object sender, EventArgs e)
        {
            if (_txtSelectionId.Data != null)
            {
                FetchData();
                _txtSelectionId.TextControl.Text = "";
                _txtSelectionId.Focus();
            }
        }

        //----------------------------------------------
        //
        //----------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                _txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);

                DisableActionButton(); //disable action button after page loads 1-Online Traveler; 2-Failure; 3-Material Required; 4-LotHold Location; 5-Document set
                HideDetailIcon();

                if (Page.IsPostBack)
                {
                    // Check if it is a pop up close, get the return result
                    if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                    {
                        if (_envSelectedLots != null)
                            // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                            if (Page.DataContract.GetValueByName("LotHold_Lots") != null)
                                _envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("LotHold_Lots") as string[];

                        if (_envSelectedLots.SS_ContainersList != null)
                        {
                            string[] sContainers;
                            sContainers = _envSelectedLots.SS_ContainersList;

                            foreach (string sContainer in sContainers)
                            {
                                _txtSelectionId.TextControl.Text = sContainer;
                                SelectionId_DataChanged(null, null);
                            }

                            //nullify the containers list
                            _envSelectedLots.SS_ContainersList = null;
                        }
                    }
                }
                else //if page is popup
                {
                    //if title is set in data contract
                    string sPopUpTitle = Page.PortalContext.DataContract.GetValueByName("HoldPopUpTitleDM") as string;
                    LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                    if (labelCache != null && !string.IsNullOrEmpty(sPopUpTitle))
                        Page.Title = labelCache.GetLabelByName(sPopUpTitle).Value;
                    else
                        Page.Title = labelCache.GetLabelByName("CSICDOName_LotHold").Value;

                    //get containers info from for the NCM Enchancement
                    EventLotDetail[] AffectedMaterial = Page.SessionVariables.GetValueByName("AffectedMaterial") as EventLotDetail[];
                    if (AffectedMaterial != null)
                    {
                        foreach (var item in AffectedMaterial)
                        {
                            _txtSelectionId.Data = item.Lot;
                            SelectionId_DataChanged(null, null);
                        }
                    }

                    //get containers from Container Search screen
                    if (Page.Session["selectedContainers"] != null)
                    {
                        string[] sContainersList = Page.Session["selectedContainers"] as string[];
                        foreach (string container in sContainersList)
                        {
                            _txtSelectionId.TextControl.Text = container;
                            SelectionId_DataChanged(null, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Page.DisplayMessage(ex.Message.ToString(), false);
            }
        }

        //----------------------------------------------
        //
        //----------------------------------------------
        public RecordSet FetchHoldLocation(string LotId)
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                // init service objects

                LotHoldService oService = new LotHoldService(fs.CurrentUserProfile);
                LotHold oServiceData = new LotHold();
                LotHold_Info oServiceInfo = new LotHold_Info();
                LotHold_Result oServiceResult = new LotHold_Result();

                oServiceData.Container = new ContainerRef();
                oServiceData.Container.Name = LotId;

                oServiceInfo.Containers = FieldInfoUtil.RequestValue();
                oServiceInfo.HoldLocation = FieldInfoUtil.RequestSelectionValue();

                // init request
                LotHold_Request oServiceRequest = new LotHold_Request();
                oServiceRequest.Info = oServiceInfo;
                // execute!
                ResultStatus oResultStatus = oService.GetEnvironment(oServiceData, oServiceRequest, out oServiceResult);

                if (oResultStatus.IsSuccess)
                {
                    if (oServiceResult.Environment.HoldLocation.IsNullOrEmpty())
                    {
                        return null;
                    }
                    else
                    {
                        return oServiceResult.Environment.HoldLocation.SelectionValues;
                    }
                }
                else
                {
                    DisplayMessage(oResultStatus);
                    return null;
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message, false));
                return null;
            }
        }

        //----------------------------------------------
        //
        //----------------------------------------------
        public void ItemListGrid_AddNewRowEx(_LotHoldDetailsEx oHoldDetail)
        {
            _LotHoldDetailsEx[] oCurrentDetails = (_gridDetails.GridContext as BoundContext).Data as _LotHoldDetailsEx[];
            List<_LotHoldDetailsEx> oNewDetails = new List<_LotHoldDetailsEx>();

            if (oCurrentDetails == null)
                oCurrentDetails = new _LotHoldDetailsEx[0];

            // add back the existing rows
            foreach (_LotHoldDetailsEx oRow in oCurrentDetails)
            {
                _LotHoldDetailsEx oCurrentRow = new _LotHoldDetailsEx();
                oCurrentRow = oRow;
                oNewDetails.Add(oCurrentRow);
            }

            // add the new row
            oNewDetails.Add(oHoldDetail);

            // rebind back to the grid
            (_gridDetails.GridContext as BoundContext).Data = oNewDetails.ToArray();
            _gridDetails.BoundContext.LoadData();
            CamstarWebControl.SetRenderToClient(_gridDetails);
        }

        //----------------------------------------------
        //
        //----------------------------------------------
        public void FetchData()
        {
            // get the session and user profile
            var fs = FrameworkManagerUtil.GetFrameworkSession();

            // init service objects

            LotHoldService oService = new LotHoldService(fs.CurrentUserProfile);
            LotHold oServiceData = new LotHold();
            LotHold_Info oServiceInfo = new LotHold_Info();
            LotHold_Result oServiceResult = new LotHold_Result();

            oServiceData.SelectionId = _txtSelectionId.Data.ToString();
            oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
            oServiceInfo.Containers = FieldInfoUtil.RequestValue();
            oServiceInfo.CurrentHoldCount = FieldInfoUtil.RequestValue();
            oServiceInfo.HoldLocation = FieldInfoUtil.RequestValue();
            oServiceInfo.HoldReason = FieldInfoUtil.RequestValue();
            oServiceInfo.ShelfLocation = FieldInfoUtil.RequestValue();

            // init request
            LotHold_Request oServiceRequest = new LotHold_Request();
            oServiceRequest.Info = oServiceInfo;
            ResultStatus oResultStatus = oService.ResolveSelectionId(oServiceData, oServiceRequest, out oServiceResult);

            if (oResultStatus.IsSuccess)
            {
                if (oServiceResult.Value.Containers != null)
                {
                    //Fetch Hold Locations for Default Hold Location field for the first scanned lot
                    if (_gridDetails.Data == null)
                    {
                        RecordSet holdLocations = FetchHoldLocation(oServiceResult.Value.Containers[0].Name);
                        _ndoHoldLocation.SetSelectionValues(holdLocations);
                        _ndoHoldLocation.ClearData();
                    }
                    //End of Fetch Hold Locations
                    foreach(ContainerRef oContainer in oServiceResult.Value.Containers)
                    {
                        _txtSelectedLot.Data = oContainer.Name;
                        // check if lot not exist in the grid else add to the grid

                        string strSelectedGridId = "";
                        if (Page.IsPostBack)
                            strSelectedGridId = (_gridDetails.GridContext as BoundContext).GetRowIdByCellValue("Lot", oContainer.Name.ToString());

                        if (string.IsNullOrEmpty(strSelectedGridId))
                        {
                            if (oServiceResult.Value.SelectionContainer.Name == oContainer.Name)
                            {
                                _LotHoldDetailsEx oHoldDetail = new _LotHoldDetailsEx();
                                oHoldDetail.Container = new ContainerRef();
                                oHoldDetail.Container.Name = oServiceResult.Value.SelectionContainer.Name;
                                oHoldDetail.ShelfLocation = oServiceResult.Value.ShelfLocation;
                                oHoldDetail.HoldLocation = oServiceResult.Value.HoldLocation;
                                oHoldDetail.HoldReason = oServiceResult.Value.HoldReason;
                                oHoldDetail.CurrentHoldCount = oServiceResult.Value.CurrentHoldCount.Value;
                                ItemListGrid_AddNewRowEx(oHoldDetail);
                            }
                            else
                            {
                                oServiceData = new LotHold();
                                oServiceData.Container = new ContainerRef();
                                oServiceData.Container.Name = oContainer.Name;
                                oServiceInfo = new LotHold_Info();
                                oServiceInfo.CurrentHoldCount = FieldInfoUtil.RequestValue();
                                oServiceInfo.HoldLocation = FieldInfoUtil.RequestValue();
                                oServiceInfo.HoldReason = FieldInfoUtil.RequestValue();
                                oServiceInfo.ShelfLocation = FieldInfoUtil.RequestValue();

                                oServiceResult = new LotHold_Result();
                                ResultStatus oResultStatus2 = oService.GetEnvironment(oServiceData, oServiceRequest, out oServiceResult);
                                if (oResultStatus2.IsSuccess)
                                {
                                    _LotHoldDetailsEx oHoldDetail = new _LotHoldDetailsEx();
                                    oHoldDetail = new _LotHoldDetailsEx();
                                    oHoldDetail.Container = new ContainerRef();
                                    oHoldDetail.Container.Name = oContainer.Name;
                                    oHoldDetail.ShelfLocation = oServiceResult.Value.ShelfLocation;
                                    oHoldDetail.HoldLocation = oServiceResult.Value.HoldLocation;
                                    oHoldDetail.HoldReason = oServiceResult.Value.HoldReason;
                                    oHoldDetail.CurrentHoldCount = oServiceResult.Value.CurrentHoldCount.Value;
                                    ItemListGrid_AddNewRowEx(oHoldDetail);
                                } // if (oResultStatus2.IsSuccess)
                            } //  if (oServiceResult.Value.SelectionContainer.Name == oContainer.Name)
                        } // foreach(ContainerRef oContainer in oServiceResult.Value.Containers)
                    } // if (oServiceResult.Value.Containers != null)  
                }

                _txtSelectionId.ClearData();
            }
            else
            {
                DisplayMessage(oResultStatus);
            }
        }

        //----------------------------------------------
        //
        //----------------------------------------------
        public void DetailGrid_RowSelected(Object sender, EventArgs e)
        {
            try
            {
                string sRowID = _gridDetails.SelectedRowID;
                if (sRowID != null)
                {
                    _txtSelectedLot.Data = _gridDetails.GridContext.GetCell(sRowID, "Container").ToString();

                    EnableActionButton();  //enable action button when row selected 1-Online Traveler; 2-Failure; 3-Material Required; 4-LotHold Location; 5-Document set
                }
                else
                {
                    DisableActionButton();  //disable action button after click clear button 1-Online Traveler; 2-Failure; 3-Material Required; 4-LotHold Location; 5-Document set
                }
            }
            catch (Exception ex)
            {
                Page.DisplayMessage("DetailsGrid_RowSelected: " + ex.Message.ToString(), false);
            }
        }

        //----------------------------------------------
        //
        //----------------------------------------------
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            try
            {
                if (_ndoEmployee.Data != null)
                {
                    (serviceData as LotHold).Employee = new NamedObjectRef();
                    (serviceData as LotHold).Employee.Name = _ndoEmployee.Data.ToString();
                }
                if (_gridDetails.Data != null)
                {
                    int _intCount = (_gridDetails.GridContext as BoundContext).GetTotalRows();

                    List<string> sContainerNames = new List<string>();
                    for (int i = 0; i < _intCount; i++)
                    {
                        string sName = (_gridDetails.GridContext as BoundContext).GetCell(i.ToString().PadLeft(6, '0'), "Container").ToString();
                        if (!string.IsNullOrEmpty(sName))
                            sContainerNames.Add(sName);
                    }

                    (serviceData as LotHold).Containers = new ContainerRef[sContainerNames.Count]; //new ContainerRef[_intCount];

                    for (int i = 0; i < _intCount; i++)
                    {
                        (serviceData as LotHold).Containers[i] = new ContainerRef();
                        (serviceData as LotHold).Containers[i].Name = sContainerNames[i];//(_gridDetails.GridContext as BoundContext).GetCell(i.ToString().PadLeft(6, '0'), "Container").ToString();
                    }

                    if (_intCount != 0)
                    {
                        var gridData =_gridDetails.Data as LotHoldDetails[];
                        (serviceData as LotHold).Details = new LotHoldDetails[_intCount];

                        for (int j = 0; j < _intCount; j++)
                        {
                            var newDetails = new LotHoldDetails()
                            {
                                Container = gridData[j].Container,
                                HoldReason = gridData[j].HoldReason,
                                HoldLocation = gridData[j].HoldLocation,
                                ExpectedHoldDays = gridData[j].ExpectedHoldDays,
                                HoldUsername = gridData[j].HoldUsername,
                                ShelfLocation = gridData[j].ShelfLocation,
                                ApplyToChildLots = gridData[j].ApplyToChildLots,
                            };

                            (serviceData as LotHold).Details[j] = newDetails;

                            if ((serviceData as LotHold).Details[j].HoldLocation.IsNullOrEmpty() && (_ndoHoldLocation.Data != null))
                                (serviceData as LotHold).Details[j].HoldLocation = new NamedObjectRef(_ndoHoldLocation.Data.ToString());

                            if ((serviceData as LotHold).Details[j].ShelfLocation.IsNullOrEmpty() && (_txtShelfLocation.Data != null))
                                (serviceData as LotHold).Details[j].ShelfLocation = _txtShelfLocation.Data.ToString();

                            if ((serviceData as LotHold).Details[j].HoldReason.IsNullOrEmpty() && (_ndoHoldReason.Data != null))
                                (serviceData as LotHold).Details[j].HoldReason = new NamedObjectRef(_ndoHoldReason.Data.ToString());

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Page.DisplayMessage("GetInputData: " + ex.Message.ToString(), false);
            }
        }

        public class _LotHoldDetailsEx : LotHoldDetails
        {
            private int _strCurrentHoldCount;

            public int CurrentHoldCount
            {
                get { return _strCurrentHoldCount; }
                set { _strCurrentHoldCount = value; }
            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            if (action != null && action.Parameters == "Reset")
            {
                _txtSelectionId.ClearData();
                _ndoEmployee.ClearData();
                _gridDetails.ClearData();
                _ndoHoldReason.ClearData();
                _txtShelfLocation.ClearData();
                _ndoHoldLocation.ClearData();
                _ndoEmailGroup.ClearData();
                _txtSelectedLot.ClearData();
                _container.ClearData();
                _txtComments.ClearData();
                _toggleComments.Reset();
            }
        }

        public void DisableActionButton()
        {
            //horizon
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "OnlineTravelerAction").First().IsDisabled = true;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "FailureAction").First().IsDisabled = true;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "MaterialRequiredAction").First().IsDisabled = true;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "HoldLocationAction").First().IsDisabled = true;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "DocumentSetAction").First().IsDisabled = true;

            //classic
            _btnDocSet.Enabled = false;
            _btnFailures.Enabled = false;
            _btnMatReq.Enabled = false;
            _btnOnlineTraveler.Enabled = false;
            _btnLocations.Enabled = false;

        }

        public void EnableActionButton()
        {
            //horizon
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "OnlineTravelerAction").First().IsDisabled = false;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "FailureAction").First().IsDisabled = false;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "MaterialRequiredAction").First().IsDisabled = false;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "HoldLocationAction").First().IsDisabled = false;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "DocumentSetAction").First().IsDisabled = false;

            //classic

            //_txtSelectionId.Data = _txtSelectedLot.Data;
            _btnDocSet.Enabled = true;
            _btnFailures.Enabled = true;
            _btnMatReq.Enabled = true;
            _btnLocations.Enabled = true;
            _btnOnlineTraveler.Enabled = true;
        }

        public void HideDetailIcon() //hide detail icon if using horizon theme
        {
            var theme = Page.Session["CurrentTheme"] != null ? Page.Session["CurrentTheme"].ToString() : string.Empty;

            if (theme.ToLower() == "horizon")
            {
                _btnDocSet.Visible = false;
                _btnFailures.Visible = false;
                _btnMatReq.Visible = false;
                _btnOnlineTraveler.Visible = false;
                _btnLocations.Visible = false;
            }
            else if (theme.ToLower() == "camstar")
            {
                _btnDocSet.Visible = true;
                _btnFailures.Visible = true;
                _btnMatReq.Visible = true;
                _btnOnlineTraveler.Visible = true;
                _btnLocations.Visible = true;

                //disable commamd bar button on classic theme
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "OnlineTravelerAction").First().IsHidden = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "FailureAction").First().IsHidden = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "MaterialRequiredAction").First().IsHidden = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "HoldLocationAction").First().IsHidden = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "DocumentSetAction").First().IsHidden = true;
            }
        }
    }
}



