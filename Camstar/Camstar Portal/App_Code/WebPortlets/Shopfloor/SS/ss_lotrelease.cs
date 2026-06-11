/* Copyright 2025 Siemens */
using System;
using System.Collections;
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
/// Summary description for Class1
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_LotRelease : scsShopfloorBase
    {
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotRelease_SelectionId") as CWC.TextBox; } }
        protected CWC.TextBox _txtSelectedLot { get { return Page.FindCamstarControl("LotRelease_ContainerName") as CWC.TextBox; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("LotRelease_ComputerName") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoReleaseReason { get { return Page.FindCamstarControl("LotRelease_ReleaseReason") as CWC.NamedObject; } }
        protected CWC.TextBox _txtShelfLocation { get { return Page.FindCamstarControl("LotRelease_ShelfLocation") as CWC.TextBox; } }
        protected CWC.Button _btnFailures { get { return Page.FindCamstarControl("FailuresPop") as CWC.Button; } }
        protected CWC.Button _btnMatReq { get { return Page.FindCamstarControl("MatReqPop") as CWC.Button; } }
        protected CWC.Button _btnDocSet { get { return Page.FindCamstarControl("DocSetPop") as CWC.Button; } }
        protected CWC.Button _btnLocations { get { return Page.FindCamstarControl("HoldLocationsPop") as CWC.Button; } }
        protected CWC.Button _btnOnlineTraveler { get { return Page.FindCamstarControl("OnlineTravelerPop") as CWC.Button; } }
        protected JQDataGrid _gridDetails { get { return Page.FindCamstarControl("LotRelease_Details") as JQDataGrid; } }
        protected SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("LotRelease_Lots") as SEMI.AppCode.DataEnvelopControl; } }
        protected SEMI.AppCode.DataEnvelopControl _envLotReleaseDetails { get { return Page.FindCamstarControl("LotReleaseDetails") as SEMI.AppCode.DataEnvelopControl; } }
        protected CWC.TextBox _txtSelectedRowId { get { return Page.FindCamstarControl("SelectedRowId") as CWC.TextBox; } }

        public void SelectionId_DataChanged(Object sender, EventArgs e)
        {
            Page.StatusBar.ClearMessage();
            if (_txtSelectionId.Data != null)
            {
                string sContainer = _txtSelectionId.Data.ToString();
                FetchData(sContainer);
                _txtSelectionId.Focus();
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                _txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);

                DisableActionButton(); //Disable action button after page loads 1-Online Traveler; 2-Failures; 3-Material Required; 4-LotHold Locations; 5-Document Set
                HideDetailIcon();

                if (Page.IsPostBack)
                {
                    // Check if it is a pop up close, get the return result
                    if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                    {
                        if (_envSelectedLots != null)
                            // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                            if (Page.DataContract.GetValueByName("LotRelease_Lots") != null)
                                _envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("LotRelease_Lots") as string[];

                        if (_envSelectedLots.SS_ContainersList != null)
                        {
                            string[] sContainers;
                            sContainers = _envSelectedLots.SS_ContainersList;

                            foreach (string sContainer in sContainers)
                            {
                                FetchData(sContainer);
                            }

                            //nullify the containers list
                            _envSelectedLots.SS_ContainersList = null;
                        }
                    }
                }
                else //if page is popup
                {
                    //if Title is set in data contract
                    string sPopUpTitle = Page.PortalContext.DataContract.GetValueByName("ReleasePopUpTitleDM") as string;
                    LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                    if (labelCache != null && !string.IsNullOrEmpty(sPopUpTitle))
                        Page.Title = labelCache.GetLabelByName(sPopUpTitle).Value;
                    else
                        Page.Title = labelCache.GetLabelByName("CSICDOName_LotRelease").Value;

                    //get containers info from ProductionEventManage_VP for the NCM Enchancement
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
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {
                    OnPopupClose();
                }

            }
            catch (Exception ex)
            { }
        }
        public string FetchData(string LotId)
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                // init service objects

                LotReleaseService oService = new LotReleaseService(fs.CurrentUserProfile);
                LotRelease oServiceData = new LotRelease();
                LotRelease_Info oServiceInfo = new LotRelease_Info();
                LotRelease_Result oServiceResult = new LotRelease_Result();

                //oServiceData.Container = new ContainerRef();
                //oServiceData.Container.Name = LotId;

                oServiceData.SelectionId = LotId;

                oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
                oServiceInfo.Containers = FieldInfoUtil.RequestValue();
                oServiceInfo.CurrentHoldCount = FieldInfoUtil.RequestValue();
                oServiceInfo.HoldLocation = FieldInfoUtil.RequestValue();
                oServiceInfo.HoldReason = FieldInfoUtil.RequestValue();
                oServiceInfo.ShelfLocation = FieldInfoUtil.RequestValue();
                oServiceInfo.LotHoldLocations = new LotHoldLocations_Info();
                oServiceInfo.LotHoldLocations.StartTimestamp = FieldInfoUtil.RequestValue();
                oServiceInfo.CurrentHoldCount = FieldInfoUtil.RequestValue();

                // init request
                LotRelease_Request oServiceRequest = new LotRelease_Request();
                oServiceRequest.Info = oServiceInfo;
                // execute!
                ResultStatus oResultStatus = oService.ResolveSelectionId(oServiceData, oServiceRequest, out oServiceResult);

                if (oResultStatus.IsSuccess)
                {
                    if (oServiceResult.Value.Containers != null)
                    {
                        foreach (ContainerRef oContainer in oServiceResult.Value.Containers)
                        {
                            if (oServiceResult.Value.SelectionContainer.Name == oContainer.Name)
                            {
                                if (oServiceResult.Value.ShelfLocation != null)
                                    ItemListGrid_AddNewRow(oContainer.Name, oServiceResult.Value.ShelfLocation.ToString(), int.Parse(oServiceResult.Value.CurrentHoldCount.ToString()));
                                else
                                    ItemListGrid_AddNewRow(oContainer.Name, null, int.Parse(oServiceResult.Value.CurrentHoldCount.ToString()));
                            }
                        }
                        return null;
                    }
                    return null;
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

        //-----------------------------------------
        //
        //-----------------------------------------
        public void ItemListGrid_AddNewRow(string sContainer, string ShelfLocation, int CurrentHoldCount)
        {
            try
            {
                JQDataGrid _gridDetails = Page.FindCamstarControl("LotRelease_Details") as JQDataGrid;
                _LotReleaseDetailsEx[] oNewDetail = new _LotReleaseDetailsEx[1];
                oNewDetail[0] = new _LotReleaseDetailsEx();
                oNewDetail[0].Container = new ContainerRef();
                oNewDetail[0].Container.Name = sContainer;
                oNewDetail[0].ShelfLocation = ShelfLocation;
                oNewDetail[0].CurrentHoldCount = CurrentHoldCount;
                _LotReleaseDetailsEx[] oExisting = (_gridDetails.GridContext as BoundContext).Data as _LotReleaseDetailsEx[];
                if (oExisting != null)
                {
                    bool isUnique = true;
                    for (int i = 0; i < oExisting.Length; i++)
                    {
                        if (oExisting[i].Container.Equals(oNewDetail[0].Container))
                        {
                            isUnique = false;
                        }
                    }
                    if (isUnique)
                    {
                        _LotReleaseDetailsEx[] oMerged = new _LotReleaseDetailsEx[oExisting.Length + 1];
                        Array.Copy(oExisting, oMerged, oExisting.Length);
                        Array.Copy(oNewDetail, 0, oMerged, oExisting.Length, 1);
                        (_gridDetails.GridContext as BoundContext).Data = oMerged.ToArray();
                        _txtSelectionId.ClearData();
                    }
                }
                else
                {
                    (_gridDetails.GridContext as BoundContext).Data = oNewDetail.ToArray();
                    _txtSelectionId.ClearData();
                }
                _gridDetails.BoundContext.LoadData();
                CamstarWebControl.SetRenderToClient(_gridDetails);
            }
            catch (Exception ex)
            { }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public void DetailGrid_RowSelected(Object sender, EventArgs e)
        {
            try
            {
                string sRowID = _gridDetails.SelectedRowID;
                if (sRowID != null)
                {
                    _txtSelectedLot.Data = _gridDetails.GridContext.GetCell(sRowID, "Container").ToString();
                    _txtSelectionId.Data = _txtSelectedLot.Data;
                    _btnDocSet.Enabled = true;
                    _btnFailures.Enabled = true;
                    _btnMatReq.Enabled = true;
                    _btnLocations.Enabled = true;
                    _btnOnlineTraveler.Enabled = true;

                    EnableActionButton();  //Enable action button when row selected 1-Online Traveler; 2-Failures; 3-Material Required; 4-LotHold Locations; 5-Document Set
                }
                else
                {
                    _btnDocSet.Enabled = false;
                    _btnFailures.Enabled = false;
                    _btnMatReq.Enabled = false;
                    _btnLocations.Enabled = false;
                    _btnOnlineTraveler.Enabled = false;

                    DisableActionButton();  //Disable action button after click clear button 1-Online Traveler; 2-Failures; 3-Material Required; 4-LotHold Locations; 5-Document Set
                }
            }
            catch (Exception ex)
            { }
        }

        //-----------------------------------------
        // Hold locations popup
        //-----------------------------------------
        public virtual void PopupHoldLocations(bool EndResponse = false)
        {
            try
            {
                if (Page.DataContract.GetValueByName("LotRelease_GridRowId_DM") != null)
                {
                    _envLotReleaseDetails.SS_LotReleaseDetails = _gridDetails.Data as LotReleaseDetails[];
                    _txtSelectedRowId.Data = Page.DataContract.GetValueByName("LotRelease_GridRowId_DM").ToString();
                    if (_envLotReleaseDetails.SS_LotReleaseDetails[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].ServiceDetails != null)
                    {
                        LotReleaseDetails[] newDetails = new LotReleaseDetails[1];
                        newDetails[0] = new LotReleaseDetails();
                        newDetails[0] = _envLotReleaseDetails.SS_LotReleaseDetails[Convert.ToInt32(_txtSelectedRowId.Data.ToString())];
                        Page.DataContract.SetValueByName("LotRelease_LotReleaseDetails_DM", newDetails);
                    }
                    else
                    {
                        _envLotReleaseDetails.SS_LotReleaseDetails = null;
                        Page.DataContract.SetValueByName("ConsumeMaterials_ConsumeMaterialsWafers_DM", null);
                    }

                    Camstar.WebPortal.Personalization.FloatPageOpenAction objAction = new FloatPageOpenAction();
                    objAction.PageName = "SS_LotHoldLocationsPopupVP";

                    UIComponentDataContractLink[] objLinks = new UIComponentDataContractLink[4];
                    objLinks[0] = new UIComponentDataContractLink();
                    objLinks[0].SourceMember = "PrimaryServiceType_DM";
                    objLinks[0].TargetMember = "PrimaryService_DM";
                    objLinks[1] = new UIComponentDataContractLink();
                    objLinks[1].SourceMember = "LotRelease_GridLot_DM";
                    objLinks[1].TargetMember = "SelectionId_DM";
                    objLinks[2] = new UIComponentDataContractLink();
                    objLinks[2].SourceMember = "LotRelease_LotReleaseDetails_DM";
                    objLinks[2].TargetMember = "LocationsPopup_LotReleaseDetails_DM";
                    objLinks[3] = new UIComponentDataContractLink();
                    objLinks[3].SourceMember = "LotRelease_IsLocationSelection_DM";
                    objLinks[3].TargetMember = "LocationsPopup_IsLocationSelection_DM";

                    UIComponentDataContractReturnLink[] objReturnLinks = new UIComponentDataContractReturnLink[1];
                    objReturnLinks[0] = new UIComponentDataContractReturnLink();
                    objReturnLinks[0].SourceMember = "LocationsPopup_LotHoldLocations_DM";
                    objReturnLinks[0].TargetMember = "LotRelease_HoldLocations_DM";
                    objAction.DataContractMap = new UIComponentDataContractMap();
                    objAction.DataContractMap.Links = objLinks;
                    objAction.DataContractReturnMap = new UIComponentDataContractReturnMap();
                    objAction.DataContractReturnMap.ReturnLinks = objReturnLinks;
                    objAction.FrameLocation = new UIFloatingPageLocation();
                    objAction.FrameLocation.Width = 850;
                    objAction.FrameLocation.Height = 500;
                    objAction.EndResponse = false;

                    this.Page.ActionDispatcher.ExecuteAction(objAction);
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // On Popup closed event
        //-----------------------------------------
        public void OnPopupClose()
        {
            try
            {
                Page.CollectDataContract();
                if (Page.DataContract.GetValueByName("LotRelease_HoldLocations_DM") != null)
                {
                    _envLotReleaseDetails.SS_LotHoldLocations = Page.DataContract.GetValueByName("LotRelease_HoldLocations_DM") as LotHoldLocations[];
                    int svcIndex = 0;

                    if (_gridDetails.SelectedRowID != null)
                    {
                        LotReleaseDetails[] currentDetails = _gridDetails.Data as LotReleaseDetails[];

                        currentDetails[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].ServiceDetails = new ReleaseLotDetails[_envLotReleaseDetails.SS_LotHoldLocations.Count()];

                        foreach (LotHoldLocations location in _envLotReleaseDetails.SS_LotHoldLocations)
                        {
                            currentDetails[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].ServiceDetails[svcIndex] = new ReleaseLotDetails();
                            currentDetails[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].ServiceDetails[svcIndex].LotHoldLocationsItem = new SubentityRef();
                            currentDetails[Convert.ToInt32(_txtSelectedRowId.Data.ToString())].ServiceDetails[svcIndex].LotHoldLocationsItem.ID = location.Self.ID;
                            svcIndex++;
                        }

                        _gridDetails.ClearData();
                        _gridDetails.Data = currentDetails.ToArray();
                        _gridDetails.OriginalData = currentDetails.ToArray();
						
						Page.DataContract.SetValueByName("LotRelease_HoldLocations_DM", null);
                        _envLotReleaseDetails.SS_LotHoldLocations = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            try
            {

                int _intCount = (_gridDetails.GridContext as BoundContext).GetTotalRows();

                if (_gridDetails.Data != null)
                {
                    (serviceData as LotRelease).Details = _gridDetails.Data as LotReleaseDetails[];
                    List<string> sContainerNames = new List<string>();
                    for (int i = 0; i < _intCount; i++)
                    {
                        string sName = (_gridDetails.GridContext as BoundContext).GetCell(i.ToString().PadLeft(6, '0'), "Container").ToString();
                        if (!string.IsNullOrEmpty(sName))
                            sContainerNames.Add(sName);
                    }

                    (serviceData as LotRelease).Containers = new ContainerRef[sContainerNames.Count]; //new ContainerRef[_intCount];

                    for (int i = 0; i < _intCount; i++)
                    {
                        (serviceData as LotRelease).Containers[i] = new ContainerRef();
                        (serviceData as LotRelease).Containers[i].Name = sContainerNames[i];
                    }

                    if ((serviceData as LotRelease).Details.Count() == _intCount)
                    {
                        LotReleaseDetails[] oNewDetails = new LotReleaseDetails[_intCount];

                        for (int x = 0; x < _intCount; x++)
                        {
                            oNewDetails[x] = new LotReleaseDetails()
                            {
                                Container = (serviceData as LotRelease).Details[x].Container,
                                ReleaseAllHoldLocations = (serviceData as LotRelease).Details[x].ReleaseAllHoldLocations,
                                ReleaseHoldLocationsOnly = (serviceData as LotRelease).Details[x].ReleaseHoldLocationsOnly,
                                ReleaseReason = (serviceData as LotRelease).Details[x].ReleaseReason,
                                ShelfLocation = (serviceData as LotRelease).Details[x].ShelfLocation,
                                ApplyToChildLots = (serviceData as LotRelease).Details[x].ApplyToChildLots,
                                ServiceDetails = (serviceData as LotRelease).Details[x].ServiceDetails,
                                scsForceYieldCheck = (serviceData as LotRelease).Details[x].scsForceYieldCheck
                            };

                            if ((serviceData as LotRelease).Details[x].ShelfLocation.IsNullOrEmpty() && (_txtShelfLocation.Data != null))
                                (serviceData as LotRelease).Details[x].ShelfLocation = _txtShelfLocation.Data.ToString();

                            if ((serviceData as LotRelease).Details[x].ReleaseReason.IsNullOrEmpty() && (_ndoReleaseReason.Data != null))
                                (serviceData as LotRelease).Details[x].ReleaseReason = new NamedObjectRef(_ndoReleaseReason.Data.ToString());

                        }

                        (serviceData as LotRelease).Details = oNewDetails;
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        //-----------------------------------------
        //
        //-----------------------------------------
        public class _LotReleaseDetailsEx : LotReleaseDetails
        {
            private int _strCurrentHoldCount;

            public int CurrentHoldCount
            {
                get { return _strCurrentHoldCount; }
                set { _strCurrentHoldCount = value; }
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
                    case "LotHoldLocations":
                        {
                            PopupHoldLocations();
                            break;
                        }
                    case "Reset":
                        {
                            Page.ShopfloorReset(sender, e);
                            _btnDocSet.Enabled = false;
                            _btnFailures.Enabled = false;
                            _btnMatReq.Enabled = false;
                            _btnLocations.Enabled = false;
                            _btnOnlineTraveler.Enabled = false;
                            break;
                        }
                }
            }
        }

        public void DisableActionButton()
        {
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "OnlineTravelerAction").First().IsDisabled = true;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "FailuresAction").First().IsDisabled = true;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "MaterialRequiredAction").First().IsDisabled = true;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "HoldLocationsAction").First().IsDisabled = true;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "DocumentSetAction").First().IsDisabled = true;
        }

        public void EnableActionButton()
        {
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "OnlineTravelerAction").First().IsDisabled = false;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "FailuresAction").First().IsDisabled = false;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "MaterialRequiredAction").First().IsDisabled = false;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "HoldLocationsAction").First().IsDisabled = false;
            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "DocumentSetAction").First().IsDisabled = false;
        }

        public void HideDetailIcon() //Hide detail icon if using horizon theme
        {
            if (IsHorizon())
            {
                _btnDocSet.Visible = false;
                _btnFailures.Visible = false;
                _btnMatReq.Visible = false;
                _btnOnlineTraveler.Visible = false;
                _btnLocations.Visible = false;

            }
            else
            {
                _btnDocSet.Visible = true;
                _btnFailures.Visible = true;
                _btnMatReq.Visible = true;
                _btnOnlineTraveler.Visible = true;
                _btnLocations.Visible = true;

                //Disable command bar button on classic theme
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "OnlineTravelerAction").First().IsHidden = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "FailuresAction").First().IsHidden = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "MaterialRequiredAction").First().IsHidden = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "HoldLocationsAction").First().IsHidden = true;
                this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "DocumentSetAction").First().IsHidden = true;
            }
        }
    }
}



