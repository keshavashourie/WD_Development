/* Copyright 2021 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.WCFUtilities;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{

    public class scsPreTrackInVerification : MatrixWebPart
    {
        #region Controls
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("scsPreTrackInVerification_SelectionId") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoProcessType { get { return Page.FindCamstarControl("scsPreTrackInVerification_ProcessType") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("scsPreTrackInVerification_EmployeeR2") as CWC.NamedObject; } }
        protected JQDataGrid _gridContainer { get { return Page.FindCamstarControl("scsPreTrackInVerification_ContainerGrid") as JQDataGrid; } }
        protected JQDataGrid _HiddenMaterialDetails { get { return Page.FindCamstarControl("HiddenMaterialDetails") as JQDataGrid; } }
        protected SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("scsPreTrackInVerification_DataEnvelop") as SEMI.AppCode.DataEnvelopControl; } }
        protected virtual CWC.TileContainer TileContainer { get { return Page.FindCamstarControl("EquipmentList") as CWC.TileContainer; } }
        protected CWC.PagePanel MaskPanel { get { return Page.FindCamstarControl("MaskPanel") as CWC.PagePanel; } }
        protected CWC.PagePanel MaterialPanel { get { return Page.FindCamstarControl("MaterialPanel") as CWC.PagePanel; } }
        protected CWC.PagePanel ToolPanel { get { return Page.FindCamstarControl("ToolPanel") as CWC.PagePanel; } }
        protected CWC.PagePanel ToolFamilyPanel { get { return Page.FindCamstarControl("ToolFamilyPanel") as CWC.PagePanel; } }
        protected MatrixWebPart EquipmentTileWP { get { return Page.FindCamstarControl("EquipmentTileWP") as MatrixWebPart; } }
        protected MatrixWebPart MaskWP { get { return Page.FindCamstarControl("MaskWP") as MatrixWebPart; } }
        protected MatrixWebPart MaterialWP { get { return Page.FindCamstarControl("MaterialWP") as MatrixWebPart; } }
        protected MatrixWebPart ToolWP { get { return Page.FindCamstarControl("ToolWP") as MatrixWebPart; } }
        protected MatrixWebPart ToolFamilyWP { get { return Page.FindCamstarControl("ToolFamilyWP") as MatrixWebPart; } }
        protected CWC.TextBox _txtHiddenSelectedEquipment { get { return Page.FindCamstarControl("HiddenSelectedEquipment") as CWC.TextBox; } }
        protected CWC.TextBox _txtHiddenSelectedValue { get { return Page.FindCamstarControl("HiddenSelectedValueField") as CWC.TextBox; } }
        protected CWC.TextBox _txtHiddenOnChangeField { get { return Page.FindCamstarControl("HiddenOnChangeField") as CWC.TextBox; } }
        protected CWC.TextBox _txtHiddenExecuteInputData { get { return Page.FindCamstarControl("HiddenExecuteInputData") as CWC.TextBox; } }
        protected CWC.TextBox _txtHiddenPhysicalLocation { get { return Page.FindCamstarControl("HiddenPhysicalLocation") as CWC.TextBox; } }
        protected CWC.TextBox _txtHiddenToolFamily { get { return Page.FindCamstarControl("HiddenToolFamily") as CWC.TextBox; } }
        protected CWC.TextBox _txtHiddenSpecName { get { return Page.FindCamstarControl("HiddenSpecName") as CWC.TextBox; } }
        protected CWC.TextBox _txtHiddenMaterialLotQty { get { return Page.FindCamstarControl("HiddenMaterialLotQty") as CWC.TextBox; } }
        protected CWC.Button _btnFetchReqDetails { get { return Page.FindCamstarControl("btnFetchReqDetails") as CWC.Button; } }
        protected CWC.CheckBox _btnRefreshFieldData { get { return Page.FindCamstarControl("RefreshFieldData") as CWC.CheckBox; } }
        protected CWC.CheckBox _btnExecuteMaskSetup { get { return Page.FindCamstarControl("ExecuteMaskSetup") as CWC.CheckBox; } }
        protected CWC.CheckBox _btnExecuteMaterialSetup { get { return Page.FindCamstarControl("ExecuteMaterialSetup") as CWC.CheckBox; } }
        protected CWC.CheckBox _btnExecuteUnloadMaterialSetup { get { return Page.FindCamstarControl("ExecuteUnloadMaterialSetup") as CWC.CheckBox; } }
        protected CWC.CheckBox _btnExecuteToolSetup { get { return Page.FindCamstarControl("ExecuteToolSetup") as CWC.CheckBox; } }
        protected CWC.CheckBox _btnExecuteToolFamilySetup { get { return Page.FindCamstarControl("ExecuteToolFamilySetup") as CWC.CheckBox; } }
        protected CWC.CheckBox _IsMaterialPopup { get { return Page.FindCamstarControl("IsMaterialPopup") as CWC.CheckBox; } }
        protected CWC.Button _PhysicalLocationMaskSelValPopupBtn { get { return Page.FindCamstarControl("PhysicalLocationMaskSelValPopupBtn") as CWC.Button; } }
        protected CWC.Button _PhysicalLocationToolSelValPopupBtn { get { return Page.FindCamstarControl("PhysicalLocationToolSelValPopupBtn") as CWC.Button; } }
        protected CWC.Button _PhysicalPositionSelValPopupBtn { get { return Page.FindCamstarControl("PhysicalPositionSelValPopupBtn") as CWC.Button; } }
        protected CWC.Button _ToolFamilySelValPopupBtn { get { return Page.FindCamstarControl("ToolFamilySelValPopupBtn") as CWC.Button; } }
        protected CWC.Button _MaterialLotSelValPopupBtn { get { return Page.FindCamstarControl("MaterialLotSelValPopupBtn") as CWC.Button; } }
        #endregion

        #region Override Methods 
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager.RegisterStartupScript(this, this.GetType(), "initializescsPreTrackInVerification", $"scsPreTrackInVerification.initialize({GetClientLabels()});", true);

            TabControlElements = Page.Session[_TabElementsIdentifier] as List<string>;

            if (TabControlElements.Count > 0)
            {
                StringBuilder script = new StringBuilder();
                for (int i = 0; i < TabControlElements.Count; i++)
                {
                    script.Append(string.Format("window['__focusableElements'].push('#{0}');", TabControlElements[i]));
                }

                string clientId = _txtSelectionId.ClientID;
                string focusOnLoadControl = string.Format("window['__startFocusElement'] = ['#{0}_ctl00'];", clientId);
                if (_txtHiddenOnChangeField.Data != null)
                {
                    focusOnLoadControl = string.Format("if(__page.lastFocusedControlId != null){{window['__startFocusElement'] = [__page.lastFocusedControlId];}}else{{window['__startFocusElement'] = ['#{0}_ctl00'];}}", clientId);
                }

                ScriptManager.RegisterStartupScript(this, this.GetType(), "focusableElements", string.Format("function AddControl(){{if(window['__focusableElements'] != null){{{0}}};}}; $(function(){{AddControl();}});{1}", script.ToString(), focusOnLoadControl), true);
            }
        }
        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/User/scs/scsPreTrackInVerification_VP.js");
        }

        /// <summary>
        /// On page load
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _txtSelectionId.DataChanged += new EventHandler(_txtSelectionId_DataChanged);
            _txtHiddenSelectedEquipment.DataChanged += new EventHandler(_txtHiddenSelectedEquipment_DataChanged);
            _ndoProcessType.DataChanged += new EventHandler(_ndoProcessType_DataChanged);

            LoadLabels();

            if (!Page.IsPostBack)
            {
                MaskWP.Hidden = true;
                MaterialWP.Hidden = true;
                ToolWP.Hidden = true;
                ToolFamilyWP.Hidden = true;
                EquipmentTileWP.Hidden = true;
                Page.Session[_FieldDataListIdentifier] = FieldDataListDic;
                Page.Session["_Container"] = null;
                Page.Session[_TabElementsIdentifier] = TabControlElements;

                if (_txtSelectionId.Data != null)
                    _txtSelectionId_DataChanged(null, null);
            }

            _txtHiddenSelectedEquipment.Style["display"] = "none";
            _btnFetchReqDetails.Style["display"] = "none";
            _PhysicalLocationMaskSelValPopupBtn.Style["display"] = "none";
            _PhysicalLocationToolSelValPopupBtn.Style["display"] = "none";
            _PhysicalPositionSelValPopupBtn.Style["display"] = "none";
            _txtHiddenSelectedValue.Style["display"] = "none";
            _txtHiddenOnChangeField.Style["display"] = "none";
            _btnRefreshFieldData.Style["display"] = "none";
            _btnExecuteMaskSetup.Style["display"] = "none";
            _btnExecuteMaterialSetup.Style["display"] = "none";
            _btnExecuteUnloadMaterialSetup.Style["display"] = "none";
            _btnExecuteToolSetup.Style["display"] = "none";
            _btnExecuteToolFamilySetup.Style["display"] = "none";
            _txtHiddenExecuteInputData.Style["display"] = "none";
            _txtHiddenPhysicalLocation.Style["display"] = "none";
            _txtHiddenToolFamily.Style["display"] = "none";
            _txtHiddenSpecName.Style["display"] = "none";
            _txtHiddenMaterialLotQty.Style["display"] = "none";
            _ToolFamilySelValPopupBtn.Style["display"] = "none";
            _MaterialLotSelValPopupBtn.Style["display"] = "none";
            _IsMaterialPopup.Style["display"] = "none";

            if (SEMI.AppCode.UIUtility.IsPopupClose(this))
            {
                OnPopupClose();
                _txtSelectionId_DataChanged(null, null);
                _IsMaterialPopup.Data = false;
            }

            if (_txtHiddenSelectedValue.Data != null)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "FocusOnFieldData", string.Format("$(function(){{setTimeout(setFocus, 300); function setFocus(){{$('#{0}').focus();}};}});", _txtHiddenSelectedValue.Data.ToString()), true);
            }
        }// OnLoad

        /// <summary>
        /// Page Actions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Reset")
            {
                _HiddenMaterialDetails.ClearData();
                Page.ClearValues();
                ClearData(true);
            }
            else if (action != null && action.Parameters == "Refresh")
            {
                _HiddenMaterialDetails.ClearData();
                FetchTxnData();
            }
        }
        #endregion

        #region Private Methods
        private Dictionary<string, string> labelNames;
        private static Dictionary<string, string> labelValues;
        private const string _FieldDataListIdentifier = "_FieldDataList";
        private Dictionary<string, string> FieldDataListDic = new Dictionary<string, string>();
        private const string _TabElementsIdentifier = "_TabElements";
        private List<string> TabControlElements = new List<string>();
        private void OnPopupClose()
        {
            // check for the lots
            if (Page.DataContract.GetValueByName("WIPMain_LotList_DM") != null)
            {
                string[] sContainers = Page.DataContract.GetValueByName("WIPMain_LotList_DM") as string[];
                if (_txtSelectionId.Data == null)
                {
                    _txtSelectionId.Data = sContainers[0];
                }
                else if (sContainers[0].ToString() != _txtSelectionId.Data.ToString())
                {
                    _txtSelectionId.Data = sContainers[0];
                    _txtHiddenSelectedValue.ClearData();
                    Page.Session[_FieldDataListIdentifier] = new Dictionary<string, string>();
                }
                Page.DataContract.SetValueByName("WIPMain_LotList_DM", null);
            }


            if (_txtHiddenSelectedValue.Data != null)
            {
                FieldDataListDic = Page.Session[_FieldDataListIdentifier] as Dictionary<string, string>;
                string selectedValue = Page.DataContract.GetValueByName("SelectedValueDM") as string;
                Page.DataContract.SetValueByName("SelectedValueDM", null);
                if (selectedValue != null)
                {
                    if (FieldDataListDic.Keys.Contains(_txtHiddenSelectedValue.Data.ToString()))
                    {
                        FieldDataListDic[_txtHiddenSelectedValue.Data.ToString()] = selectedValue;
                    }
                    else
                    {
                        FieldDataListDic.Add(_txtHiddenSelectedValue.Data.ToString(), selectedValue);
                    }

                    if (_IsMaterialPopup.CheckControl.Checked == true)
                    {
                        //retrieve Qty of Material Lot and set the data to
                        FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                        QueryService oService = new QueryService(session.CurrentUserProfile);
                        QueryParameters objQueryParameters = new QueryParameters();
                        QueryParameter[] queryParameters = new QueryParameter[1]
                        {
                            new QueryParameter("ContainerName", selectedValue.ToString())
                        };

                        objQueryParameters.Parameters = queryParameters;
                        RecordSet recordSet = new RecordSet();
                        ResultStatus status = oService.Execute(GetMaterialLotQtyQuery(), objQueryParameters, new QueryOptions(), out recordSet);

                        string IdMaterialLotQty = _txtHiddenMaterialLotQty.Data != null ? _txtHiddenMaterialLotQty.Data.ToString() : null;
                        if (_txtHiddenMaterialLotQty.Data != null)
                        {
                            _txtHiddenMaterialLotQty.Data = IdMaterialLotQty + ":" + recordSet.Rows[0].Values[0];

                            if (FieldDataListDic.Keys.Contains(IdMaterialLotQty))
                            {
                                FieldDataListDic[IdMaterialLotQty] = recordSet.Rows[0].Values[0];
                            }
                            else
                            {
                                FieldDataListDic.Add(IdMaterialLotQty, recordSet.Rows[0].Values[0]);
                            }
                        }
                    }
                    Page.Session[_FieldDataListIdentifier] = FieldDataListDic;
                }
                Page.Session["_Container"] = null;
            }
            else
            {
                Page.Session[_FieldDataListIdentifier] = new Dictionary<string, string>();
            }

        }
        private void _txtSelectionId_DataChanged(object sender, EventArgs e)
        {
            if (_txtSelectionId.Data != null && _ndoProcessType.Data != null)
            {
                if (Page.Session["_Container"] != null && Page.Session["_Container"].ToString() != _txtSelectionId.Data.ToString())
                {
                    _ndoProcessType.Data = null;
                    return;
                }
            }
            EquipmentTileWP.Hidden = false;
            MaskWP.Hidden = false;
            MaterialWP.Hidden = false;
            ToolWP.Hidden = false;
            ToolFamilyWP.Hidden = false;
            FetchTxnData();
            if (_txtSelectionId.Data != null)
                Page.Session["_Container"] = _txtSelectionId.Data.ToString();
        } // _txtSelectionId_DataChanged
        private void _txtHiddenSelectedEquipment_DataChanged(object sender, EventArgs e)
        {
            _txtHiddenSelectedValue.ClearData();
            Page.Session[_FieldDataListIdentifier] = new Dictionary<string, string>();
        } // _txtHiddenSelectedEquipment_DataChanged
        private void _ndoProcessType_DataChanged(object sender, EventArgs e)
        {
            if (_txtSelectionId.Data != null && Page.Session["_Container"] != null && Page.Session["_Container"].ToString() == _txtSelectionId.Data.ToString() && _ndoProcessType.Data != null && _txtHiddenSelectedEquipment.Data != null)
            {
                FetchDetails();
            }

        } // _ndoProcessType_DataChanged
        private void ClearData(bool clearSelectionId)
        {
            if (clearSelectionId)
            {
                _txtSelectionId.ClearData();
                Page.Session["_Container"] = null;
            }
            _ndoProcessType.ClearData();
            _ndoProcessType.ClearSelectionValues();
            _ndoEmployee.ClearData();
            _txtSelectionId.Focus();
            MaskWP.Hidden = true;
            MaterialWP.Hidden = true;
            ToolWP.Hidden = true;
            ToolFamilyWP.Hidden = true;
            EquipmentTileWP.Hidden = true;
            _txtHiddenSelectedValue.ClearData();
            Page.Session[_FieldDataListIdentifier] = new Dictionary<string, string>();
        }
        #endregion

        #region Label
        //Set the label names
        protected virtual void SetLabelNames()
        {
            labelNames = new Dictionary<string, string>
            {
                //Labels for GetEquipmentList resultset
                { "EquipmentName", "CSICDOName_Equipment" },          // Equipment Name
                { "EquipmentDescription", "SelVal_Description" },     // Equipment Description
                { "CurrentlyAssignedRecipe", "SelVal_Recipe" },       // Currently Assigned Recipe
                { "CurrentEquipmentStatus", "Status" },               // Current Equipment Status
                { "CurrentAvailability", "Resource_ResourceIcon" },   // Current Availability: Up or Down
                { "State", "scsTrackinSetupState" },
                { "CurrentEquipment", "scsTrackinSetupCurrEqpmt" },
                { "CurrentPhysicalLocation", "scsTrackinSetupCurrPhysicalLocation" },
                { "CurrentPhysicalPosition", "scsTrackinSetupCurrPhysicalPosition" },
                { "NewPhysicalLocation", "scsPreTrackInNewPhysicalLocation" },
                { "NewPhysicalPosition", "scsPreTrackInNewPhysicalPosition" },
                { "Mask", "scsTrackinSetupMask" },
                { "Material", "scsTrackinSetupMaterial" },
                { "RequestedQty", "scsTrackinSetupReqQty" },
                { "MaterialLot", "CSICDOName_MaterialLot" },
                { "MaterialQty", "MfgOrderChanges_Qty" },
                { "Tool", "CSICDOName_Tool" },
                { "ToolFamily", "CSICDOName_ToolFamily" },
                { "Qty", "Web_Quantity" },
                { "InvalidLotErrorMsg", "scsPreTrackIn_InvalidLot" },
                { "PhysicalLocationEmpty", "scsPreTrackIn_EmptyPhysicalLocation"},
                { "StateInUse", "scsPreTrackIn_InUse" },
                { "StateNotInUse", "scsPreTrackIn_NotInUse"},
                { "NoRequirementFound", "scsPreTrackIn_NoRequirementFound"},
                { "ToolSelectionEmpty", "scsPreTrackIn_EmptyToolSelection"}
            };
        }
        protected virtual void LoadLabels()
        {
            //Create mapping of friendly label names to actual label names
            SetLabelNames();

            //Load labels to the cache
            var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
            LabelList labelList = new LabelList();
            foreach (KeyValuePair<string, string> item in labelNames)
            {
                labelList.Add(new OM.Label(item.Value));
            }
            labelCache.GetLabels(labelList);

            //Create mapping of friendly names to label values (e.g. CurrentlyAssignedRecipe = Recipe)
            labelValues = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> item in labelNames)
            {
                var label = labelCache.GetLabelByName(item.Value);
                labelValues.Add(item.Key, label.Value);
            }
        }
        protected virtual string GetClientLabels()
        {
            List<string> labelMap = new List<string>();
            foreach (KeyValuePair<string, string> label in labelValues)
            {
                labelMap.Add("\"" + label.Key + "\"" + ":" + "\"" + label.Value + "\"");
            }
            return "{" + String.Join(",", labelMap) + "}";
        }
        protected static string GetStateLabel(int state)
        {
            if (state == 1)
            {
                return (labelValues["StateInUse"] != null) ? labelValues["StateInUse"] : "In Use";
            }
            else
            {
                return (labelValues["StateNotInUse"] != null) ? labelValues["StateNotInUse"] : "Not In Use";
            }
        }
        #endregion

        #region EquipmentTiles
        protected virtual void BuildTiles(NamedObjectRef[] EquipmentSelection)
        {
            TileContainer.DefaultImage = "typeMachine48.svg";
            if (TileContainer.ControlState.Tiles != null)
            {
                TileContainer.ControlState.Tiles.Clear();
                MaskWP.Hidden = true;
                MaterialWP.Hidden = true;
                ToolWP.Hidden = true;
                ToolFamilyWP.Hidden = true;
            }
            ScriptManager.RegisterStartupScript(this, this.GetType(), "CreateTileScript", $"scsPreTrackInVerification.setOrderList({GetEquipmentListJSON(EquipmentSelection)});", true);

            this.WebPartManager.RenderToClient(EquipmentTileWP);
        }
        //Query used for getting Equipment list
        protected virtual string GetEquipmentListQuery()
        {
            return "GetEquipmentList";
        }
        protected virtual string GetMaterialLotQtyQuery()
        {
            return "scsGetMaterialLotQty";
        }
        //Execute query for getting Equipment list and return it in JSON format
        protected virtual string GetEquipmentListJSON(NamedObjectRef[] EquipmentSelection)
        {
            var ordList = new List<EquipmentListItem>();
            foreach (NamedObjectRef Equipment in EquipmentSelection)
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                QueryService oService1 = new QueryService(session.CurrentUserProfile);
                QueryParameters objQueryParameters = new QueryParameters();
                QueryParameter[] queryParameters = new QueryParameter[1]
                {
                    new QueryParameter("ResourceName", Equipment.ToString()) //To retrieve each Equipment details in Equipment list
                };

                objQueryParameters.Parameters = queryParameters;
                RecordSet recordSet1 = new RecordSet();
                ResultStatus status = oService1.Execute(GetEquipmentListQuery(), objQueryParameters, new QueryOptions(), out recordSet1);

                var item = new EquipmentListItem
                {
                    EquipmentName = recordSet1.Rows[0].Values[0],
                    EquipmentDescription = recordSet1.Rows[0].Values[1],
                    CurrentlyAssignedRecipe = recordSet1.Rows[0].Values[2],
                    CurrentEquipmentStatus = recordSet1.Rows[0].Values[3],
                    CurrentAvailability = recordSet1.Rows[0].Values[4],
                };
                item.UniqueId = item.EquipmentName;

                ordList.Add(item);
            }


            TileContainer.ControlState.DefaultImage = "typeMachine48.svg";
            TileContainer.ControlState.Tiles = new List<CWC.TileContainer.TileContext>();

            foreach (EquipmentListItem item in ordList)
            {
                TileContainer.ControlState.Tiles.Add(CreateTile(item, "Equipment"));
            }
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(ordList);
        }
        //Creates tile for tile container based on Equipment list data
        protected virtual CWC.TileContainer.TileContext CreateTile(EquipmentListItem ordListItem, string tileColName)
        {
            var tile = new CWC.TileContainer.TileContext
            {
                ColumnName = tileColName,
                Title = ordListItem.EquipmentName,
                Image = "typeMachine48.svg"
            };

            var strings = new List<string>();
            if (labelValues != null)
            {
                strings.Add($"{labelValues["EquipmentDescription"]} : {ordListItem.EquipmentDescription}");
                strings.Add($"{labelValues["CurrentlyAssignedRecipe"]} : {ordListItem.CurrentlyAssignedRecipe}");
                strings.Add($"{labelValues["CurrentEquipmentStatus"]} : {ordListItem.CurrentEquipmentStatus}");
            }
            tile.Text = strings.ToArray();
            tile.CustomData = ordListItem.UniqueId;

            return tile;
        }
        #endregion

        #region FetchData
        public void FetchTxnData()
        {
            // Prepare service
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            ResultStatus oServiceResult = new ResultStatus(null, false);
            string sServiceType = "WIPMain";

            // Run proper constructor. We need to be dynamic with the primary service type
            var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
            //create a request object
            var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
            var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
            var oService = svcConstructor.Invoke(new object[] { fs.CurrentUserProfile });

            //retriving data dynamically for Flexibility of the page.
            var oServiceData = CreateServiceData(sServiceType);

            var info = CreateServiceInfo(sServiceType);
            var oServiceInfo = info as WIPMain_Info;
            (oRequest as Request).Info = oServiceInfo;

            if (_txtSelectionId.Data != null)
            {
                (oServiceData as WIPMain).SelectionId = _txtSelectionId.Data.ToString();
                if (_ndoEmployee.Data != null)
                    (oServiceData as WIPMain).Employee = _ndoEmployee.Data as NamedObjectRef;
                if (_ndoProcessType.Data != null && Page.Session["_Container"] != null && Page.Session["_Container"].ToString() == _txtSelectionId.Data.ToString())
                    (oServiceData as WIPMain).ProcessType = _ndoProcessType.Data as NamedObjectRef;

                oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();
                oServiceInfo.ProcessTypeSelection = FieldInfoUtil.RequestValue();
                oServiceInfo.EquipmentSelection = FieldInfoUtil.RequestValue(); //To retrieve Equipment list based on OnGetValue event in TrackInLot.EquipmentSelection
                oServiceInfo.WIPFlagSelection = FieldInfoUtil.RequestValue();
                oServiceInfo.SpecName = FieldInfoUtil.RequestValue();
            }

            // init the result object
            Result oResult = new Result();
            ResultStatus oResultStatus = new ResultStatus();

            (oService as IShopFloorBase).BeginTransaction();
            (oService as IShopFloorBase).ResolveSelectionId(oServiceData as DCObject);
            (oService as IShopFloorBase).ExecuteTransaction();
            oResultStatus = (oService as IShopFloorBase).CommitTransaction((oRequest as Request), out oResult);
            if (oResultStatus.IsSuccess)
            {
                _txtHiddenSpecName.Data = ((oResult.Value as WIPMain).SpecName != null) ? (oResult.Value as WIPMain).SpecName : "";
                if ((oResult.Value as WIPMain).EquipmentSelection != null)
                {
                    if ((oResult.Value as WIPMain).WIPFlagSelection == 1)
                    {
                        if ((oResult.Value as WIPMain).ProcessTypeSelection != null)
                        {
                            CWC.NamedObject _ndoTempProcessType = _ndoProcessType;
                            SEMI.AppCode.ControlsUtility.NamedObjectControl_SetSelectionValues(ref _ndoTempProcessType, (oResult.Value as WIPMain).ProcessTypeSelection);

                            // have to manually set the process type if there is more than one selection value
                            if ((oResult.Value as WIPMain).ProcessTypeSelection.Length > 1 && _ndoProcessType.Data == null)
                                _ndoProcessType.Data = (oResult.Value as WIPMain).ProcessTypeSelection[0];
                        }
                        BuildTiles((oResult.Value as WIPMain).EquipmentSelection);
                    }
                    else
                    {
                        if (labelValues["InvalidLotErrorMsg"] != null)
                        {
                            DisplayMessage(new ResultStatus(labelValues["InvalidLotErrorMsg"].Replace("#ErrorMsg.Name", _txtSelectionId.Data.ToString()), false));
                        }
                        else
                        {
                            DisplayMessage(new ResultStatus(string.Format("Lot '{0}' is not waiting for Track-In", _txtSelectionId.Data.ToString()), false));
                        }
                        ClearData(false);
                    }
                }
                else
                {
                    if (labelValues["InvalidLotErrorMsg"] != null)
                    {
                        DisplayMessage(new ResultStatus(labelValues["InvalidLotErrorMsg"].Replace("#ErrorMsg.Name", _txtSelectionId.Data.ToString()), false));
                    }
                    else
                    {
                        DisplayMessage(new ResultStatus(string.Format("Lot '{0}' is not waiting for Track-In", _txtSelectionId.Data.ToString()), false));
                    }
                    ClearData(false);
                }
            }
            else
            {
                DisplayMessage(oResultStatus);
                ClearData(false);
            }
        }
        public void FetchDetails()
        {
            MaskWP.Hidden = true;
            MaterialWP.Hidden = true;
            ToolWP.Hidden = true;
            ToolFamilyWP.Hidden = true;

            Page.Session[_TabElementsIdentifier] = new List<string>();
            _txtHiddenOnChangeField.ClearData();
            _HiddenMaterialDetails.ClearData();

            bool isMaskToolRequirementsEmpty = FetchMaskToolRequirements();
            bool isMaterialRequirementsEmpty = FetchMaterialRequirements();

            CamstarWebControl.SetRenderToClient(MaskPanel);
            CamstarWebControl.SetRenderToClient(ToolPanel);

            if (isMaskToolRequirementsEmpty && isMaterialRequirementsEmpty)
            {
                if (labelValues["NoRequirementFound"] != null)
                {
                    DisplayMessage(new ResultStatus(labelValues["NoRequirementFound"], true));
                }
                else
                {
                    DisplayMessage(new ResultStatus("There is no Mask, Tool or Material requirements for the selected equipment and process type", true));
                }
            }

            Page.IsChild = true;
            Page.ProcessingContext.Status = ProcessingStatusType.SubmitTransaction;
        }
        public void RefreshFieldDataList()
        {
            FieldDataListDic = Page.Session[_FieldDataListIdentifier] as Dictionary<string, string>;
            if (FieldDataListDic != null)
            {
                string id = _txtHiddenOnChangeField.Data.ToString().Split(':')[0];
                string newValue = _txtHiddenOnChangeField.Data.ToString().Split(':')[1];
                if (FieldDataListDic.Keys.Contains(id))
                {
                    FieldDataListDic[id] = newValue;
                }
                else
                {
                    FieldDataListDic.Add(id, newValue);
                }
            }
        }
        protected bool FetchMaterialRequirements()
        {
            bool isMaterialRequirementsEmpty = true;

            // Prepare service
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            ResultStatus oServiceResult = new ResultStatus(null, false);
            string sServiceType = "EquipmentMaterialsSetup";

            // Run proper constructor. We need to be dynamic with the primary service type
            var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
            //create a request object
            var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
            var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
            var oService = new WSDataCreator().CreateService(sServiceType, fs.CurrentUserProfile);

            //retriving data dynamically for Flexibility of the page.
            var oServiceData = CreateServiceData(sServiceType);

            var info = CreateServiceInfo(sServiceType);
            var oServiceInfo = info as EquipmentMaterialsSetup_Info;
            (oRequest as Request).Info = oServiceInfo;

            if (_txtSelectionId.Data != null)
            {
                (oServiceData as EquipmentMaterialsSetup).scsContainer = new ContainerRef(_txtSelectionId.Data.ToString());
                if (_ndoEmployee.Data != null)
                    (oServiceData as EquipmentMaterialsSetup).Employee = _ndoEmployee.Data as NamedObjectRef;
                (oServiceData as EquipmentMaterialsSetup).Equipment = new NamedObjectRef(_txtHiddenSelectedEquipment.Data.ToString());
                (oServiceData as EquipmentMaterialsSetup).ProcessType = new NamedObjectRef(_ndoProcessType.Data.ToString());

                (oServiceData as EquipmentMaterialsSetup).Resource = new NamedObjectRef(_txtHiddenSelectedEquipment.Data.ToString());

                //Request the required details
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
                oServiceInfo.scsEquipmentMaterials = FieldInfoUtil.RequestSelectionValue();
            }

            // init the result object
            Result oResult = new Result();
            ResultStatus oResultStatus = new ResultStatus();
            oResultStatus = oService.GetEnvironment(oServiceData as DCObject, oRequest as Request, out oResult);

            if (oResultStatus.IsSuccess)
            {
                var oResultValue = oResult as EquipmentMaterialsSetup_Result;

                if (oResultValue.Value.EquipmentMaterials != null)
                {
                    EquipmentMaterialsDetails[] objEqpMaterialsDetails = new EquipmentMaterialsDetails[oResultValue.Value.EquipmentMaterials.Length];
                    int iCount = 0;
                    foreach (EquipmentMaterials resultDetail in oResultValue.Value.EquipmentMaterials)
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
                     (_HiddenMaterialDetails.GridContext as BoundContext).Data = objEqpMaterialsDetails.ToArray();
                    _HiddenMaterialDetails.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(_HiddenMaterialDetails);
                }

                // Display the Material details
                if (oResultValue.Environment.scsEquipmentMaterials.SelectionValues.Rows != null)
                {
                    DisplayMaterialDetail(oResultValue.Environment.scsEquipmentMaterials.SelectionValues.Rows);
                    isMaterialRequirementsEmpty = false;
                }
            }
            else
            {
                DisplayMessage(oResultStatus);
                return false;
            }

            return isMaterialRequirementsEmpty;
        }

        protected bool FetchMaskToolRequirements()
        {
            bool isMaskToolRequirementsEmpty = true;

            // Prepare service
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            ResultStatus oServiceResult = new ResultStatus(null, false);
            string sServiceType = "EquipmentSetup";

            // Run proper constructor. We need to be dynamic with the primary service type
            var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
            //create a request object
            var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
            var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
            var oService = new WSDataCreator().CreateService(sServiceType, fs.CurrentUserProfile);

            //retriving data dynamically for Flexibility of the page.
            var oServiceData = CreateServiceData(sServiceType);

            var info = CreateServiceInfo(sServiceType);
            var oServiceInfo = info as EquipmentSetup_Info;
            (oRequest as Request).Info = oServiceInfo;

            if (_txtSelectionId.Data != null)
            {
                (oServiceData as EquipmentSetup).ToolPlanLot = new ContainerRef(_txtSelectionId.Data.ToString());
                if (_ndoEmployee.Data != null)
                    (oServiceData as EquipmentSetup).Employee = _ndoEmployee.Data as NamedObjectRef;
                (oServiceData as EquipmentSetup).Equipment = new NamedObjectRef(_txtHiddenSelectedEquipment.Data.ToString());
                (oServiceData as EquipmentSetup).ProcessType = new NamedObjectRef(_ndoProcessType.Data.ToString());

                //Request the required details
                oServiceInfo.scsRequiredMaskDetails = FieldInfoUtil.RequestSelectionValue();
                oServiceInfo.scsRequiredToolDetails = FieldInfoUtil.RequestSelectionValue();
                oServiceInfo.scsRequiredToolFamilyDetails = FieldInfoUtil.RequestSelectionValue();
                oServiceInfo.scsRequiredMaskValidationMsg = FieldInfoUtil.RequestValue();
            }

            // init the result object
            Result oResult = new Result();
            ResultStatus oResultStatus = new ResultStatus();
            oResultStatus = oService.GetEnvironment(oServiceData as DCObject, oRequest as Request, out oResult);

            if (oResultStatus.IsSuccess)
            {
                var oResultValue = oResult as EquipmentSetup_Result;
                // Display the Mask details
                if (oResultValue.Environment.scsRequiredMaskDetails.SelectionValues != null && oResultValue.Environment.scsRequiredMaskDetails.SelectionValues.Rows != null)
                {
                    DisplayMaskSpecificToolDetail(oResultValue.Environment.scsRequiredMaskDetails.SelectionValues.Rows, ResourceTypeEnum.Mask);
                    isMaskToolRequirementsEmpty = false;
                }
                else
                {
                    if (oResultValue.Value.scsRequiredMaskValidationMsg != null)
                    {
                        MaskWP.Hidden = false;
                        MaskPanel.Controls.Clear();
                        MaskPanel.Style["width"] = "100%";
                        MaskPanel.Style["height"] = "100%";
                        isMaskToolRequirementsEmpty = false;
                        Page.DisplayWarning(oResultValue.Value.scsRequiredMaskValidationMsg.ToString());
                    }
                }

                // Display the Tool details
                if (oResultValue.Environment.scsRequiredToolDetails.SelectionValues != null && oResultValue.Environment.scsRequiredToolDetails.SelectionValues.Rows != null)
                {
                    DisplayMaskSpecificToolDetail(oResultValue.Environment.scsRequiredToolDetails.SelectionValues.Rows, ResourceTypeEnum.Tool);
                    isMaskToolRequirementsEmpty = false;
                }

                // Display the Tool Family details
                if (oResultValue.Environment.scsRequiredToolFamilyDetails.SelectionValues != null && oResultValue.Environment.scsRequiredToolFamilyDetails.SelectionValues.Rows != null)
                {
                    DisplayToolFamilyDetail(oResultValue.Environment.scsRequiredToolFamilyDetails.SelectionValues.Rows, ResourceTypeEnum.ToolFamily);
                    isMaskToolRequirementsEmpty = false;
                }
            }
            else
            {
                DisplayMessage(oResultStatus);
                return false;
            }

            return isMaskToolRequirementsEmpty;
        }
        #endregion

        #region Execute
        public void ExecuteToolSetup(object sender, EventArgs e)
        {
            string toolName = _txtHiddenExecuteInputData.Data.ToString().Split(':')[0];
            string newPhysicalLocation = _txtHiddenExecuteInputData.Data.ToString().Split(':')[1];
            string newPhysicalPosition = _txtHiddenExecuteInputData.Data.ToString().Split(':')[2];
            string currentEquipment = _txtHiddenExecuteInputData.Data.ToString().Split(':')[3];

            // Prepare service
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            ResultStatus oServiceResult = new ResultStatus(null, false);
            string sServiceType = "ToolSetup";

            // Run proper constructor. We need to be dynamic with the primary service type
            var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
            //create a request object
            var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
            var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
            var oService = new WSDataCreator().CreateService(sServiceType, fs.CurrentUserProfile);

            //retriving data dynamically for Flexibility of the page.
            var oServiceData = CreateServiceData(sServiceType);

            var info = CreateServiceInfo(sServiceType);
            var oServiceInfo = info as ToolSetup_Info;
            (oRequest as Request).Info = oServiceInfo;

            (oServiceData as ToolSetup).Resource = new NamedObjectRef(toolName);
            (oServiceData as ToolSetup).ResourceChangeCount = int.Parse(CurrentChangeCount(sServiceType, toolName));
            if (_ndoEmployee.Data != null)
                (oServiceData as ToolSetup).Employee = _ndoEmployee.Data as NamedObjectRef;

            (oServiceData as ToolSetup).ParentResource = new NamedObjectRef("");
            (oServiceData as ToolSetup).PhysicalLocation = new NamedObjectRef(newPhysicalLocation);
            (oServiceData as ToolSetup).PhysicalPosition = new NamedObjectRef(newPhysicalPosition);

            // init the result object
            Result oResult = new Result();
            ResultStatus oResultStatus = new ResultStatus();
            oResultStatus = oService.ExecuteTransaction(oServiceData as DCObject, oRequest as Request, out oResult);

            if (oResultStatus.IsSuccess)
            {
                DisplayMessage(oResultStatus);
                FetchDetails();
            }
            else
            {
                DisplayMessage(oResultStatus);
            }

            Page.IsChild = true;
            Page.ProcessingContext.Status = ProcessingStatusType.SubmitTransaction;
        }

        public void ExecuteToolFamilySetup(object sender, EventArgs e)
        {
            string toolFamilyName = _txtHiddenExecuteInputData.Data.ToString().Split(':')[0];
            string toolName = _txtHiddenExecuteInputData.Data.ToString().Split(':')[4];
            string toolQtyName = _txtHiddenExecuteInputData.Data.ToString().Split(':')[5];
            string newPhysicalLocation = _txtHiddenExecuteInputData.Data.ToString().Split(':')[1];
            string newPhysicalPosition = _txtHiddenExecuteInputData.Data.ToString().Split(':')[2];
            string currentEquipment = _txtHiddenExecuteInputData.Data.ToString().Split(':')[3];

            // Prepare service
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            ResultStatus oServiceResult = new ResultStatus(null, false);
            string sServiceType = "ToolSetup";

            // Run proper constructor. We need to be dynamic with the primary service type
            var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
            //create a request object
            var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
            var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
            var oService = new WSDataCreator().CreateService(sServiceType, fs.CurrentUserProfile);

            //retriving data dynamically for Flexibility of the page.
            var oServiceData = CreateServiceData(sServiceType);

            var info = CreateServiceInfo(sServiceType);
            var oServiceInfo = info as ToolSetup_Info;
            (oRequest as Request).Info = oServiceInfo;

            (oServiceData as ToolSetup).Resource = new NamedObjectRef(toolName);
            (oServiceData as ToolSetup).ResourceChangeCount = int.Parse(CurrentChangeCount(sServiceType, toolName));
            if (_ndoEmployee.Data != null)
                (oServiceData as ToolSetup).Employee = _ndoEmployee.Data as NamedObjectRef;

            (oServiceData as ToolSetup).ParentResource = new NamedObjectRef("");
            (oServiceData as ToolSetup).PhysicalLocation = new NamedObjectRef(newPhysicalLocation);
            (oServiceData as ToolSetup).PhysicalPosition = new NamedObjectRef(newPhysicalPosition);

            // init the result object
            Result oResult = new Result();
            ResultStatus oResultStatus = new ResultStatus();
            oResultStatus = oService.ExecuteTransaction(oServiceData as DCObject, oRequest as Request, out oResult);

            if (oResultStatus.IsSuccess)
            {
                DisplayMessage(oResultStatus);
                FetchDetails();
            }
            else
            {
                DisplayMessage(oResultStatus);
            }

            Page.IsChild = true;
            Page.ProcessingContext.Status = ProcessingStatusType.SubmitTransaction;
        }

        public void ExecuteMaskSetup(object sender, EventArgs e)
        {
            string maskName = _txtHiddenExecuteInputData.Data.ToString().Split(':')[0];
            string newPhysicalLocation = _txtHiddenExecuteInputData.Data.ToString().Split(':')[1];
            string newPhysicalPosition = _txtHiddenExecuteInputData.Data.ToString().Split(':')[2];
            string currentEquipment = _txtHiddenExecuteInputData.Data.ToString().Split(':')[3];
            string hiddenSelectedEquipment = _txtHiddenSelectedEquipment.Data.ToString();

            // Prepare service
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            ResultStatus oServiceResult = new ResultStatus(null, false);
            string sServiceType = "MaskSetup";

            // Run proper constructor. We need to be dynamic with the primary service type
            var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
            //create a request object
            var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
            var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
            var oService = new WSDataCreator().CreateService(sServiceType, fs.CurrentUserProfile);

            //retriving data dynamically for Flexibility of the page.
            var oServiceData = CreateServiceData(sServiceType);

            var info = CreateServiceInfo(sServiceType);
            var oServiceInfo = info as MaskSetup_Info;
            (oRequest as Request).Info = oServiceInfo;

            (oServiceData as MaskSetup).Resource = new NamedObjectRef(maskName);
            (oServiceData as MaskSetup).ResourceChangeCount = int.Parse(CurrentChangeCount(sServiceType, maskName));
            if (_ndoEmployee.Data != null)
                (oServiceData as MaskSetup).Employee = _ndoEmployee.Data as NamedObjectRef;

            (oServiceData as MaskSetup).ParentResource = new NamedObjectRef(hiddenSelectedEquipment);
            (oServiceData as MaskSetup).PhysicalLocation = new NamedObjectRef(newPhysicalLocation);
            (oServiceData as MaskSetup).PhysicalPosition = new NamedObjectRef(newPhysicalPosition);

            // init the result object
            Result oResult = new Result();
            ResultStatus oResultStatus = new ResultStatus();
            oResultStatus = oService.ExecuteTransaction(oServiceData as DCObject, oRequest as Request, out oResult);

            if (oResultStatus.IsSuccess)
            {
                DisplayMessage(oResultStatus);
                FetchDetails();
            }
            else
            {
                DisplayMessage(oResultStatus);
            }

            Page.IsChild = true;
            Page.ProcessingContext.Status = ProcessingStatusType.SubmitTransaction;
        }

        public void ExecuteMaterialSetup(object sender, EventArgs e)
        {
            string materialName = _txtHiddenExecuteInputData.Data.ToString().Split(':')[0];
            string materialLot = _txtHiddenExecuteInputData.Data.ToString().Split(':')[1];
            string materialQty = _txtHiddenExecuteInputData.Data.ToString().Split(':')[2];
            string hiddenSelectedEquipment = _txtHiddenSelectedEquipment.Data.ToString();

            // Prepare service
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            ResultStatus oServiceResult = new ResultStatus(null, false);
            string sServiceType = "EquipmentMaterialsSetup";

            // Run proper constructor. We need to be dynamic with the primary service type
            var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
            //create a request object
            var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
            var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
            var oService = new WSDataCreator().CreateService(sServiceType, fs.CurrentUserProfile);

            //retriving data dynamically for Flexibility of the page.
            var oServiceData = CreateServiceData(sServiceType);

            var info = CreateServiceInfo(sServiceType);
            var oServiceInfo = info as EquipmentMaterialsSetup_Info;
            (oRequest as Request).Info = oServiceInfo;

            if (_ndoEmployee.Data != null)
                (oServiceData as EquipmentMaterialsSetup).Employee = _ndoEmployee.Data as NamedObjectRef;

            (oServiceData as EquipmentMaterialsSetup).Resource = new NamedObjectRef(hiddenSelectedEquipment);

            EquipmentMaterialsDetails[] oDetails = (_HiddenMaterialDetails.GridContext as BoundContext).Data as EquipmentMaterialsDetails[];

            if (oDetails != null)
            {
                EquipmentMaterialsDetails[] oDetails1 = new EquipmentMaterialsDetails[oDetails.Length + 1];

                for (int i = 0; i < oDetails.Length; i++)
                {
                    oDetails1[i] = oDetails[i];
                }

                oDetails1[oDetails.Length] = new EquipmentMaterialsDetails();
                oDetails1[oDetails.Length].MaterialLotName = materialLot;
                oDetails1[oDetails.Length].MaterialPart = new RevisionedObjectRef(materialName);
                oDetails1[oDetails.Length].Qty = Double.Parse(materialQty);
                (oServiceData as EquipmentMaterialsSetup).Details = oDetails1 as EquipmentMaterialsDetails[];
            }
            else
            {
                EquipmentMaterialsDetails[] oDetails2 = new EquipmentMaterialsDetails[1];
                oDetails2[0] = new EquipmentMaterialsDetails();
                oDetails2[0].MaterialLotName = materialLot;
                oDetails2[0].MaterialPart = new RevisionedObjectRef(materialName);
                oDetails2[0].Qty = Double.Parse(materialQty);
                (oServiceData as EquipmentMaterialsSetup).Details = oDetails2 as EquipmentMaterialsDetails[];
            }

            // init the result object
            Result oResult = new Result();
            ResultStatus oResultStatus = new ResultStatus();
            oResultStatus = oService.ExecuteTransaction(oServiceData as DCObject, oRequest as Request, out oResult);

            if (oResultStatus.IsSuccess)
            {
                DisplayMessage(oResultStatus);
                FetchDetails();
            }
            else
            {
                DisplayMessage(oResultStatus);
            }

            Page.IsChild = true;
            Page.ProcessingContext.Status = ProcessingStatusType.SubmitTransaction;
        }

        public void ExecuteUnloadMaterialSetup(object sender, EventArgs e)
        {
            string materialName = _txtHiddenExecuteInputData.Data.ToString().Split(':')[0];
            string materialLot = _txtHiddenExecuteInputData.Data.ToString().Split(':')[1];
            string materialQty = _txtHiddenExecuteInputData.Data.ToString().Split(':')[2];
            string hiddenSelectedEquipment = _txtHiddenSelectedEquipment.Data.ToString();

            // Prepare service
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            ResultStatus oServiceResult = new ResultStatus(null, false);
            string sServiceType = "EquipmentMaterialsSetup";

            // Run proper constructor. We need to be dynamic with the primary service type
            var svcType = WCFObject.CreateObjectType(sServiceType + "Service");
            //create a request object
            var oRequest = WCFObject.CreateObject(sServiceType + "_Request");
            var svcConstructor = svcType.GetConstructor(new Type[] { typeof(UserProfile) });
            var oService = new WSDataCreator().CreateService(sServiceType, fs.CurrentUserProfile);

            //retriving data dynamically for Flexibility of the page.
            var oServiceData = CreateServiceData(sServiceType);

            var info = CreateServiceInfo(sServiceType);
            var oServiceInfo = info as EquipmentMaterialsSetup_Info;
            (oRequest as Request).Info = oServiceInfo;

            if (_ndoEmployee.Data != null)
                (oServiceData as EquipmentMaterialsSetup).Employee = _ndoEmployee.Data as NamedObjectRef;

            (oServiceData as EquipmentMaterialsSetup).Resource = new NamedObjectRef(hiddenSelectedEquipment);

            EquipmentMaterialsDetails[] oDetails = (_HiddenMaterialDetails.GridContext as BoundContext).Data as EquipmentMaterialsDetails[];

            if (oDetails != null)
            {
                EquipmentMaterialsDetails[] oDetails1 = new EquipmentMaterialsDetails[oDetails.Length - 1];

                int index = 0;

                for (int i = 0; i < oDetails.Length; i++)
                {
                    if (oDetails[i].MaterialLotName == materialLot)
                    {
                        continue;
                    }
                    oDetails1[index++] = oDetails[i];
                }

                (oServiceData as EquipmentMaterialsSetup).Details = oDetails1 as EquipmentMaterialsDetails[];
            }

            // init the result object
            Result oResult = new Result();
            ResultStatus oResultStatus = new ResultStatus();
            oResultStatus = oService.ExecuteTransaction(oServiceData as DCObject, oRequest as Request, out oResult);

            if (oResultStatus.IsSuccess)
            {
                DisplayMessage(oResultStatus);
                FetchDetails();
            }
            else
            {
                DisplayMessage(oResultStatus);
            }

            Page.IsChild = true;
            Page.ProcessingContext.Status = ProcessingStatusType.SubmitTransaction;
        }

        private string CurrentChangeCount(string serviceType, string Name)
        {
            string sChangeCount = "0";
            // get the session and user profile
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            string ServiceTypeRequired = serviceType;
            ResultStatus oServiceResult = new ResultStatus(null, false);
            var oServiceData = WCFObject.CreateObject(ServiceTypeRequired) as ICreator;
            var oServiceRequest = WCFObject.CreateObject(ServiceTypeRequired + "_Request") as ICreator;
            var oServiceInfo = WCFObject.CreateObject(ServiceTypeRequired + "_Info") as ICreator;
            Result oResponseData = null;
            var oService = new WSDataCreator().CreateService(ServiceTypeRequired, fs.CurrentUserProfile);
            oServiceData.SetValue("Resource", new NamedObjectRef(Name));
            oServiceInfo.SetValue("ResourceChangeCount", new OM.Info(true));
            oServiceRequest.SetValue("Info", oServiceInfo);

            // Request the data
            ResultStatus oResultStatus = oService.GetEnvironment(oServiceData as DCObject, oServiceRequest as Request, out oResponseData);
            if (oResultStatus.IsSuccess)
            {
                sChangeCount = (oResponseData as ICreator).GetValue("Value.ResourceChangeCount").ToString();
            }

            return sChangeCount;
        }
        #endregion

        #region DisplayPanel
        private void DisplayMaskSpecificToolDetail(Row[] rows, ResourceTypeEnum type)
        {
            if (type == ResourceTypeEnum.Tool)
            {
                ToolWP.Hidden = false;
                ToolPanel.Controls.Clear();
                ToolPanel.Style["width"] = "100%";
                ToolPanel.Style["height"] = "100%";
            }
            else if (type == ResourceTypeEnum.Mask)
            {
                MaskWP.Hidden = false;
                MaskPanel.Controls.Clear();
                MaskPanel.Style["width"] = "100%";
                MaskPanel.Style["height"] = "100%";
            }

            FieldDataListDic = Page.Session[_FieldDataListIdentifier] as Dictionary<string, string>;

            for (int i = 0; i < rows.Count(); i++)
            {
                MaskToolDetail detail = new MaskToolDetail
                {
                    Name = rows[i].Values[0],
                    State = (rows[i].Values[3] == "0") ? StateEnum.NOT_IN_USE : StateEnum.IN_USE,
                    CurrentEquipment = (rows[i].Values[4] == "") ? "-" : rows[i].Values[4],
                    CurrentPhysicalLocation = (rows[i].Values[5] == "") ? "-" : rows[i].Values[5],
                    CurrentPhysicalPosition = (rows[i].Values[6] == "") ? "-" : rows[i].Values[6],
                    Type = type
                };
                LiteralControl chartLiteral = new LiteralControl(detail.getHtml(i, _txtHiddenSelectedEquipment.Data.ToString(), FieldDataListDic));
                if (type == ResourceTypeEnum.Tool)
                {
                    ToolPanel.Controls.Add(chartLiteral);
                }
                else if (type == ResourceTypeEnum.Mask)
                {
                    MaskPanel.Controls.Add(chartLiteral);
                }

                if (detail.State == StateEnum.NOT_IN_USE && _txtHiddenSelectedEquipment.Data.ToString() != detail.CurrentEquipment)
                {
                    string IdPhysicalLocation = string.Format("{0}{1}_Physical_Location", detail.Type.ToString(), i);
                    string IdPhysicalPosition = string.Format("{0}{1}_Physical_Position", detail.Type.ToString(), i);
                    string IdPhysicalLocationSelValBtn = string.Format("{0}{1}_Physical_Location_SelValBtn", detail.Type.ToString(), i);
                    string IdPhysicalPositionSelValBtn = string.Format("{0}{1}_Physical_Position_SelValBtn", detail.Type.ToString(), i);
                    string IdExecuteBtn = string.Format("{0}_Execute_{1}", detail.Type.ToString(), i);
                    TabControlElements = Page.Session[_TabElementsIdentifier] as List<string>;
                    if (!TabControlElements.Contains(IdPhysicalLocation))
                        TabControlElements.Add(IdPhysicalLocation);
                    if (!TabControlElements.Contains(IdPhysicalLocationSelValBtn))
                        TabControlElements.Add(IdPhysicalLocationSelValBtn);
                    if (!TabControlElements.Contains(IdPhysicalPosition))
                        TabControlElements.Add(IdPhysicalPosition);
                    if (!TabControlElements.Contains(IdPhysicalPositionSelValBtn))
                        TabControlElements.Add(IdPhysicalPositionSelValBtn);
                    if (!TabControlElements.Contains(IdExecuteBtn))
                        TabControlElements.Add(IdExecuteBtn);
                    Page.Session[_TabElementsIdentifier] = TabControlElements;
                }
            }

            if (_txtHiddenSelectedValue.Data != null)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "FocusOnFieldData", string.Format("$('#{0}').focus();", _txtHiddenSelectedValue.Data.ToString()), false);
                _txtHiddenSelectedValue.ClearData();
            }
        }

        private void DisplayMaterialDetail(Row[] rows)
        {
            MaterialWP.Hidden = false;

            Dictionary<string, MaterialDetail> materialDict = new Dictionary<string, MaterialDetail>();

            List<MaterialDetail> details = new List<MaterialDetail>();
            for (int i = 0; i < rows.Count(); i++)
            {
                MaterialDetail materialDetail = new MaterialDetail
                {
                    Name = rows[i].Values[0],
                    RequestedQty = rows[i].Values[1]
                };
                materialDict.Add(materialDetail.Name, materialDetail);
                //details.Add(materialDetail);
            }

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            QueryService oService = new QueryService(session.CurrentUserProfile);
            QueryParameters objQueryParameters = new QueryParameters();
            QueryParameter[] queryParameters = new QueryParameter[1]
            {
                    new QueryParameter("ResourceName", _txtHiddenSelectedEquipment.Data.ToString())
            };

            objQueryParameters.Parameters = queryParameters;
            RecordSet recordSet = new RecordSet();
            ResultStatus status = oService.Execute(GetMaterialLotDetails(), objQueryParameters, new QueryOptions(), out recordSet);

            if (recordSet != null && recordSet.Rows != null && recordSet.Rows.Count() > 0)
            {
                for (int i = 0; i < recordSet.Rows.Count(); i++)
                {
                    MaterialDetail materialDetail = new MaterialDetail
                    {
                        Name = recordSet.Rows[i].Values[0],
                        MaterialLot = recordSet.Rows[i].Values[1],
                        LoadedQty = recordSet.Rows[i].Values[2],
                        Load = MaterialLoadEnum.LOADED_NOT_REQUIRED
                    };
                    if (materialDict.ContainsKey(materialDetail.Name))
                    {
                        materialDict[materialDetail.Name].MaterialLot = materialDetail.MaterialLot;
                        materialDict[materialDetail.Name].LoadedQty = materialDetail.LoadedQty;
                        materialDict[materialDetail.Name].Load = MaterialLoadEnum.LOADED;
                    }
                    else
                    {
                        materialDict.Add(materialDetail.Name, materialDetail);
                    }
                    //details.Add(materialDetail);
                }
            }

            details = materialDict.Values.ToList();

            //run separate query, compare with rows
            MaterialPanel.Controls.Clear();
            MaterialPanel.Style["width"] = "100%";
            MaterialPanel.Style["height"] = "100%";

            FieldDataListDic = Page.Session[_FieldDataListIdentifier] as Dictionary<string, string>;

            for (int i = 0; i < details.Count; i++)
            {
                LiteralControl chartLiteral = new LiteralControl(details[i].getHtml(_txtSelectionId.Data.ToString(), i, _txtHiddenSelectedEquipment.Data.ToString(), "", "", FieldDataListDic, _txtHiddenMaterialLotQty.Data != null ? _txtHiddenMaterialLotQty.Data.ToString() : null));
                MaterialPanel.Controls.Add(chartLiteral);
            }

            if (_txtHiddenSelectedValue.Data != null)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "FocusOnFieldData", string.Format("$('#{0}').focus();", _txtHiddenSelectedValue.Data.ToString()), false);
                _txtHiddenSelectedValue.ClearData();
            }
        }

        private void DisplayToolFamilyDetail(Row[] rows, ResourceTypeEnum type)
        {

            ToolFamilyWP.Hidden = false;
            ToolFamilyPanel.Controls.Clear();
            ToolFamilyPanel.Style["width"] = "100%";
            ToolFamilyPanel.Style["height"] = "100%";

            FieldDataListDic = Page.Session[_FieldDataListIdentifier] as Dictionary<string, string>;

            for (int i = 0; i < rows.Count(); i++)
            {
                ToolFamilyDetail detail = new ToolFamilyDetail
                {
                    Name = rows[i].Values[0],
                    State = (rows[i].Values[3] == "0") ? StateEnum.NOT_IN_USE : StateEnum.IN_USE,
                    CurrentEquipment = (rows[i].Values[4] == "") ? "-" : rows[i].Values[4],
                    Qty = rows[i].Values[5],
                    ToolPlanId = rows[i].Values[6],
                    Type = type
                };
                LiteralControl chartLiteral = new LiteralControl(detail.getHtml(i, _txtHiddenSelectedEquipment.Data.ToString(), FieldDataListDic));
                if (type == ResourceTypeEnum.ToolFamily)
                {
                    ToolFamilyPanel.Controls.Add(chartLiteral);
                }

                if (detail.State == StateEnum.NOT_IN_USE && _txtHiddenSelectedEquipment.Data.ToString() != detail.CurrentEquipment)
                {
                    string IdPhysicalLocation = string.Format("{0}{1}_Physical_Location", detail.Type.ToString(), i);
                    string IdPhysicalPosition = string.Format("{0}{1}_Physical_Position", detail.Type.ToString(), i);
                    string IdPhysicalLocationSelValBtn = string.Format("{0}{1}_Physical_Location_SelValBtn", detail.Type.ToString(), i);
                    string IdPhysicalPositionSelValBtn = string.Format("{0}{1}_Physical_Position_SelValBtn", detail.Type.ToString(), i);
                    string IdExecuteBtn = string.Format("{0}_Execute_{1}", detail.Type.ToString(), i);
                    TabControlElements = Page.Session[_TabElementsIdentifier] as List<string>;
                    if (!TabControlElements.Contains(IdPhysicalLocation))
                        TabControlElements.Add(IdPhysicalLocation);
                    if (!TabControlElements.Contains(IdPhysicalLocationSelValBtn))
                        TabControlElements.Add(IdPhysicalLocationSelValBtn);
                    if (!TabControlElements.Contains(IdPhysicalPosition))
                        TabControlElements.Add(IdPhysicalPosition);
                    if (!TabControlElements.Contains(IdPhysicalPositionSelValBtn))
                        TabControlElements.Add(IdPhysicalPositionSelValBtn);
                    if (!TabControlElements.Contains(IdExecuteBtn))
                        TabControlElements.Add(IdExecuteBtn);
                    Page.Session[_TabElementsIdentifier] = TabControlElements;
                }
            }
            if (_txtHiddenSelectedValue.Data != null)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "FocusOnFieldData", string.Format("$('#{0}').focus();", _txtHiddenSelectedValue.Data.ToString()), false);
                _txtHiddenSelectedValue.ClearData();
            }
        }

        #endregion

        #region Enum
        protected class EquipmentListItem
        {
            public string EquipmentName;
            public string EquipmentDescription;
            public string CurrentlyAssignedRecipe;
            public string CurrentEquipmentStatus;
            public string CurrentAvailability;
            public string UniqueId;
        }

        protected class ToolFamilyListItem
        {
            public string ToolFamilyName;
            public string ToolName;
            public string ToolId;
            public string CurrentState;
            public string CurrentEquipment;
            public string CurrentPhysicalLocation;
            public string CurrentPhysicalPosition;
            public string ToolFamilyQty;
            public string UniqueId;
        }
        protected enum StateEnum
        {
            NOT_IN_USE, IN_USE
        }
        protected enum ResourceTypeEnum
        {
            Mask, Tool, ToolFamily
        }

        protected enum MaterialLoadEnum
        {
            NOT_LOADED, LOADED, LOADED_NOT_REQUIRED
        }
        #endregion

        #region Class
        protected class MaskToolDetail
        {
            public string Name;
            public StateEnum State;
            public string CurrentEquipment;
            public string CurrentPhysicalLocation;
            public string CurrentPhysicalPosition;
            public ResourceTypeEnum Type;

            public string getHtml(int index, string selectedEquipment, Dictionary<string, string> FieldDataListDic)
            {
                StringBuilder htmlText = new StringBuilder();
                string style = "background: #EDFBF5;";
                string icon = null;
                if ((State == StateEnum.IN_USE || State == StateEnum.NOT_IN_USE) && selectedEquipment == CurrentEquipment)
                {
                    style = "background: #EDFBF5;";
                    icon = "<div style='float:right;background-image:url(./assets/image/indicatorCheckmarkGreen16.svg);min-height:32px;min-width:32px;margin-top:10px;'></div>";
                }
                else if (State == StateEnum.IN_USE && selectedEquipment != CurrentEquipment)
                {
                    style = "background: #FBEEED;";
                    icon = " <div style='float:right;background-image:url(./assets/image/indicatorVoided16.svg);min-height:32px;min-width:32px;margin-top:10px;'></div>";
                }
                else
                {
                    style = "background: #FFFFFF;";
                }

                string lblType = (labelValues["Tool"] != null) ? labelValues["Tool"] : "Tool";
                if (Type == ResourceTypeEnum.Mask)
                {
                    lblType = (labelValues["Mask"] != null) ? labelValues["Mask"] : "Mask";
                }

                string lblState = (labelValues["State"] != null) ? labelValues["State"] : "State";
                string lblCurrentEqp = (labelValues["CurrentEquipment"] != null) ? labelValues["CurrentEquipment"] : "Current Equipment";
                string lblCurrentPhysLoc = (labelValues["CurrentPhysicalLocation"] != null) ? labelValues["CurrentPhysicalLocation"] : "Current Physical Location";
                string lblCurrentPhysPos = (labelValues["CurrentPhysicalPosition"] != null) ? labelValues["CurrentPhysicalPosition"] : "Current Physical Position";
                string lblNewPhysLoc = (labelValues["NewPhysicalLocation"] != null) ? labelValues["NewPhysicalLocation"] : "New Physical Location";
                string lblNewPhysPos = (labelValues["NewPhysicalPosition"] != null) ? labelValues["NewPhysicalPosition"] : "New Physical Position";

                htmlText.Append(string.Format(@"<div class='pretrackin-card' style='{0}  max-height: 400px; overflow-x: hidden; overflow-y:auto;'>
                                  <div class='pretrackin-card-container'>
                                  {1}
                                  <div><span style='font-weight: 600;margin-left:0px;'>{7}:</span>&nbsp;<span id='{13}_Name_{12}' style='margin-left:0px;'>{2}</span></div>
                                  <div><span style='font-weight: 600;margin-left:0px;'>{8}:</span>&nbsp;<span id='{13}_State_{12}' style='margin-left:0px;'>{3}</span></div>
                                  <div><span style='font-weight: 600;margin-left:0px;'>{9}:</span>&nbsp;<span id='{13}_CurrentEquipment_{12}' style='margin-left:0px;'>{4}</span></div>
                                  <div><span style='font-weight: 600;margin-left:0px;'>{10}:</span>&nbsp;<span id='{13}_CurrentPhysicalLocation_{12}' style='margin-left:0px;'>{5}</span></div>
                                  <div><span style='font-weight: 600;margin-left:0px;'>{11}:</span>&nbsp;<span id='{13}_CurrentPhysicalPosition_{12}' style='margin-left:0px;'>{6}</span></div>
                                  </div>", style, icon, Name, GetStateLabel((int)State), CurrentEquipment, CurrentPhysicalLocation, CurrentPhysicalPosition, lblType, lblState, lblCurrentEqp, lblCurrentPhysLoc, lblCurrentPhysPos, index, Type.ToString()));
                if (State == StateEnum.NOT_IN_USE && selectedEquipment != CurrentEquipment)
                {
                    string IdPhysicalLocation = string.Format("{0}{1}_Physical_Location", Type.ToString(), index);
                    string IdPhysicalPosition = string.Format("{0}{1}_Physical_Position", Type.ToString(), index);
                    string exisitingPhysicalLocation = (FieldDataListDic != null && FieldDataListDic.Keys.Contains(IdPhysicalLocation)) ? FieldDataListDic[IdPhysicalLocation] : "";
                    string exisitingPhysicalPosition = (FieldDataListDic != null && FieldDataListDic.Keys.Contains(IdPhysicalPosition)) ? FieldDataListDic[IdPhysicalPosition] : "";

                    string disabled = null;
                    if (exisitingPhysicalPosition == "")
                    {
                        disabled = "disabled='disabled'";
                    }

                    htmlText.AppendLine(string.Format(@"<div class='pretrackin-card-container' style='padding-top:0px !important;'>
                                      <div style='margin:30px 0 0 0; float:right;'><input id='{11}_Execute_{8}' type='submit' value='' onclick=""$('#ctl00_WebPartManager_BlankWP0_HiddenExecuteInputData_ctl00').val('{6}:'.concat($('#{0}').val().concat(':'.concat($('#{2}').val()).concat(':{7}')))); $('#ctl00_WebPartManager_BlankWP0_Execute{11}Setup_ctl00')[0].click();"" {9} class='cs-button-image' style='width: 28px; background-repeat:no-repeat; background-position:center; background-image:url(./assets/image/cmdMaterialRequest24.svg); border-color:#D4D4D4;margin-left: 0px !important;margin-top: 7px !important;'></div>
                                      <div style='margin:0px; width:400px;'><span class='cs-label' locallabel='New Physical Location' defaultlabel='New Physical Location'>{4}</span>
                                      <span class='cs-textbox'>
                                      <input id='{0}' type='text' autocomplete='off' style='margin-left:0px;' value='{1}' onchange=""function _onChange(_value){{$('#ctl00_WebPartManager_BlankWP0_HiddenOnChangeField_ctl00').val('{0}:'.concat(_value));return true;}} _onChange($(this).val()); $('#ctl00_WebPartManager_BlankWP0_RefreshFieldData_ctl00')[0].click();""></span>
                                      <input id='{0}_SelValBtn' type='submit' value='' onclick=""$('#ctl00_WebPartManager_BlankWP0_HiddenSelectedValueField_ctl00').val('{0}'); $('#ctl00_WebPartManager_BlankWP0_PhysicalLocation{11}SelValPopupBtn')[0].click();"" class='cs-button-image' style='width: 28px; background-repeat:no-repeat; background-position:center; background-image:url(themes/Horizon/images/User/SCS/SS_LotSelection.svg); border-color:#D4D4D4;margin-left: -50px !important;margin-top: 7px !important;'>
                                      </div>
                                      <div style='margin:0px; width:400px;'>
                                      <span class='cs-label' locallabel='New Physical Position' defaultlabel='New Physical Position'>{5}</span>
                                      <span class='cs-textbox'>
                                      <input id='{2}' type='text' autocomplete='off' style='margin-left:0px;' value='{3}' onchange=""function _onChange(_value){{$('#ctl00_WebPartManager_BlankWP0_HiddenOnChangeField_ctl00').val('{2}:'.concat(_value));$('#{11}_Execute_{8}').attr('disabled', 'disabled'); if(_value != ''){{$('#{11}_Execute_{8}').removeAttr('disabled');}}return true;}} _onChange($(this).val());$('#ctl00_WebPartManager_BlankWP0_RefreshFieldData_ctl00')[0].click();""></span>
                                      <input id='{2}_SelValBtn' type='submit' value='' onclick=""if($('#{0}').val() == ''){{__page.displayStatus('{10}', 'Warning', 'Warning'); return false;}}$('#ctl00_WebPartManager_BlankWP0_HiddenSelectedValueField_ctl00').val('{2}');$('#ctl00_WebPartManager_BlankWP0_HiddenPhysicalLocation_ctl00').val($('#{0}').val().trim());$('#ctl00_WebPartManager_BlankWP0_PhysicalPositionSelValPopupBtn')[0].click();"" class='cs-button-image' style='width: 28px; background-repeat:no-repeat; background-position:center; background-image:url(themes/Horizon/images/User/SCS/SS_LotSelection.svg); border-color:#D4D4D4;margin-left: -50px !important;margin-top: 7px !important;'>
                    </div>", IdPhysicalLocation, exisitingPhysicalLocation, IdPhysicalPosition, exisitingPhysicalPosition, lblNewPhysLoc, lblNewPhysPos, Name, CurrentEquipment, index, disabled, labelValues["PhysicalLocationEmpty"], Type.ToString()));
                }
                htmlText.Append("</div>");

                return htmlText.ToString();
            }
        }

        public static string GetToolFamilyToolListQuery()
        {
            return "GetToolFamilyToolList";
        }

        public static string GetMaterialLotDetails()
        {
            return "scsGetMaterialLotDetails";
        }

        protected class ToolFamilyDetail
        {
            public string Name;
            public StateEnum State;
            public string CurrentEquipment;
            public string CurrentPhysicalLocation;
            public string CurrentPhysicalPosition;
            public ResourceTypeEnum Type;
            public string Qty;
            public string ToolPlanId;

            public string getHtml(int index, string selectedEquipment, Dictionary<string, string> FieldDataListDic)
            {
                StringBuilder htmlText = new StringBuilder();
                string style = "background: #FFFFFF;";
                string icon = null;
                string lblType = (labelValues["ToolFamily"] != null) ? labelValues["ToolFamily"] : "Tool Family";
                string lblTool = (labelValues["Tool"] != null) ? labelValues["Tool"] : "Tool";
                string lblQty = (labelValues["Qty"] != null) ? labelValues["Qty"] : "Quantity";
                string lblState = (labelValues["State"] != null) ? labelValues["State"] : "State";
                string lblCurrentEqp = (labelValues["CurrentEquipment"] != null) ? labelValues["CurrentEquipment"] : "Current Equipment";
                string lblCurrentPhysLoc = (labelValues["CurrentPhysicalLocation"] != null) ? labelValues["CurrentPhysicalLocation"] : "Current Physical Location";
                string lblCurrentPhysPos = (labelValues["CurrentPhysicalPosition"] != null) ? labelValues["CurrentPhysicalPosition"] : "Current Physical Position";
                string lblNewPhysLoc = (labelValues["NewPhysicalLocation"] != null) ? labelValues["NewPhysicalLocation"] : "New Physical Location";
                string lblNewPhysPos = (labelValues["NewPhysicalPosition"] != null) ? labelValues["NewPhysicalPosition"] : "New Physical Position";

                htmlText.Append(string.Format(@"<div class='pretrackin-card' style='{0} max-height: 450px; overflow-x: hidden; overflow-y:auto;'>
                                  <div class='pretrackin-card-container'>
                                  {1}
                                  <div><span style='font-weight: 600;margin-left:0px;'>{7}:</span>&nbsp;<span id='{13}_Name_{12}' style='margin-left:0px;'>{2}</span></div>
                                  <div><span style='font-weight: 600;margin-left:0px;'>{14}:</span>&nbsp;<span id='{13}_Qty_{12}' style='margin-left:0px;'>{15}</span></div>
                                  </div>", style, icon, Name, GetStateLabel((int)State), CurrentEquipment, CurrentPhysicalLocation, CurrentPhysicalPosition, lblType, lblState, lblCurrentEqp, lblCurrentPhysLoc, lblCurrentPhysPos, index, Type.ToString(), lblQty, Qty));

                for (int i = 0; i < Int32.Parse(Qty); i++)
                {
                    var ordList = new List<ToolFamilyListItem>();
                    FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                    QueryService oService = new QueryService(session.CurrentUserProfile);
                    QueryParameters objQueryParameters = new QueryParameters();
                    QueryParameter[] queryParameters = new QueryParameter[2] //To retrieve each Tool Family's Tool list
                    {
                        new QueryParameter("ToolPlanId", ToolPlanId),
                        new QueryParameter("ToolFamilyName", Name)
                    };

                    objQueryParameters.Parameters = queryParameters;
                    RecordSet recordSet = new RecordSet();
                    ResultStatus status = oService.Execute(GetToolFamilyToolListQuery(), objQueryParameters, new QueryOptions(), out recordSet);

                    string IdTool = null;
                    string existingToolName = null;
                    string currentState = "0";
                    string currentToolEquipment = "-";
                    string IdPhysicalLocation = null;
                    string IdPhysicalPosition = null;
                    string currentPhysicalLocation = "-";
                    string currentPhysicalPosition = "-";
                    string exisitingPhysicalLocation = null;
                    string exisitingPhysicalPosition = null;
                    string selectedTool = null;

                    if (recordSet != null && recordSet.Rows != null && recordSet.Rows.Length > 0)
                    {
                        if (recordSet.Rows.Count() > i)
                        {
                            if (recordSet.Rows[i].Values[0] == Name)
                            {
                                var item = new ToolFamilyListItem
                                {
                                    ToolFamilyName = recordSet.Rows[i].Values[0] == Name ? recordSet.Rows[0].Values[0] : "",
                                    ToolName = recordSet.Rows[i].Values[1] == "" ? "-" : recordSet.Rows[i].Values[1],
                                    ToolId = recordSet.Rows[i].Values[2] == "" ? "-" : recordSet.Rows[i].Values[2],
                                    CurrentState = recordSet.Rows[i].Values[3] == "" ? "-" : recordSet.Rows[i].Values[3],
                                    CurrentEquipment = recordSet.Rows[i].Values[4] == "" ? "-" : recordSet.Rows[i].Values[4],
                                    CurrentPhysicalLocation = recordSet.Rows[i].Values[5] == "" ? "-" : recordSet.Rows[i].Values[5],
                                    CurrentPhysicalPosition = recordSet.Rows[i].Values[6] == "" ? "-" : recordSet.Rows[i].Values[6],
                                    ToolFamilyQty = recordSet.Rows[i].Values[7] == "" ? "-" : recordSet.Rows[i].Values[7],
                                };
                                item.UniqueId = item.ToolName;

                                ordList.Add(item);

                                IdTool = string.Format("{0}_Tool_{1}", Name, i);
                                selectedTool = (FieldDataListDic != null && FieldDataListDic.Keys.Contains(IdTool)) ? FieldDataListDic[IdTool] : "";
                                existingToolName = ordList[0].ToolName;
                                currentState = ordList[0].CurrentState;
                                currentToolEquipment = ordList[0].CurrentEquipment;
                                IdPhysicalLocation = string.Format("{0}_Physical_Location_{1}", Name, i);
                                IdPhysicalPosition = string.Format("{0}_Physical_Position_{1}", Name, i);
                                currentPhysicalLocation = ordList[0].CurrentPhysicalLocation;
                                currentPhysicalPosition = ordList[0].CurrentPhysicalPosition;
                                exisitingPhysicalLocation = (FieldDataListDic != null && FieldDataListDic.Keys.Contains(IdPhysicalLocation)) ? FieldDataListDic[IdPhysicalLocation] : "";
                                exisitingPhysicalPosition = (FieldDataListDic != null && FieldDataListDic.Keys.Contains(IdPhysicalPosition)) ? FieldDataListDic[IdPhysicalPosition] : "";
                            }
                            else
                            {
                                IdTool = string.Format("{0}_Tool_{1}", Name, i);
                                selectedTool = (FieldDataListDic != null && FieldDataListDic.Keys.Contains(IdTool)) ? FieldDataListDic[IdTool] : "";
                                IdPhysicalLocation = string.Format("{0}_Physical_Location_{1}", Name, i);
                                IdPhysicalPosition = string.Format("{0}_Physical_Position_{1}", Name, i);
                                exisitingPhysicalLocation = (FieldDataListDic != null && FieldDataListDic.Keys.Contains(IdPhysicalLocation)) ? FieldDataListDic[IdPhysicalLocation] : "";
                                exisitingPhysicalPosition = (FieldDataListDic != null && FieldDataListDic.Keys.Contains(IdPhysicalPosition)) ? FieldDataListDic[IdPhysicalPosition] : "";
                            }
                        }
                        else
                        {
                            IdTool = string.Format("{0}_Tool_{1}", Name, i);
                            selectedTool = (FieldDataListDic != null && FieldDataListDic.Keys.Contains(IdTool)) ? FieldDataListDic[IdTool] : "";
                            IdPhysicalLocation = string.Format("{0}_Physical_Location_{1}", Name, i);
                            IdPhysicalPosition = string.Format("{0}_Physical_Position_{1}", Name, i);
                            exisitingPhysicalLocation = (FieldDataListDic != null && FieldDataListDic.Keys.Contains(IdPhysicalLocation)) ? FieldDataListDic[IdPhysicalLocation] : "";
                            exisitingPhysicalPosition = (FieldDataListDic != null && FieldDataListDic.Keys.Contains(IdPhysicalPosition)) ? FieldDataListDic[IdPhysicalPosition] : "";
                        }
                    }
                    else
                    {
                        IdTool = string.Format("{0}_Tool_{1}", Name, i);
                        selectedTool = (FieldDataListDic != null && FieldDataListDic.Keys.Contains(IdTool)) ? FieldDataListDic[IdTool] : "";
                        IdPhysicalLocation = string.Format("{0}_Physical_Location_{1}", Name, i);
                        IdPhysicalPosition = string.Format("{0}_Physical_Position_{1}", Name, i);
                        exisitingPhysicalLocation = (FieldDataListDic != null && FieldDataListDic.Keys.Contains(IdPhysicalLocation)) ? FieldDataListDic[IdPhysicalLocation] : "";
                        exisitingPhysicalPosition = (FieldDataListDic != null && FieldDataListDic.Keys.Contains(IdPhysicalPosition)) ? FieldDataListDic[IdPhysicalPosition] : "";
                    }
                
                    if ((currentState == "0" || currentState == "1") && selectedEquipment == currentToolEquipment)
                    {
                        style = "background: #EDFBF5;";
                        icon = "<div style='float:right;background-image:url(./assets/image/indicatorCheckmarkGreen16.svg);min-height:32px;min-width:32px;margin-top:10px;'></div>";
                    }
                    else if (currentState == "1" && selectedEquipment != currentToolEquipment)
                    {
                        style = "background: #FBEEED;";
                        icon = " <div style='float:right;background-image:url(./assets/image/indicatorVoided16.svg);min-height:32px;min-width:32px;margin-top:10px;'></div>";
                    }
                    else
                    {
                        style = "background: #FFFFFF;";
                        icon = null;
                    }

                    if (!string.IsNullOrWhiteSpace(existingToolName))
                    {
                        htmlText.AppendLine(string.Format(@"<div class='pretrackin-card-container' style='{0}'>
                                  {1}
                                  <div><span style='font-weight: 600;margin-left:0px;'>{14}:</span>&nbsp;<span id='{15}_Tool_{12}' style='margin-left:0px;'>{16}</span></div>
                                  <div><span style='font-weight: 600;margin-left:0px;'>{8}:</span>&nbsp;<span id='{15}_State_{12}' style='margin-left:0px;'>{3}</span></div>
                                  <div><span style='font-weight: 600;margin-left:0px;'>{9}:</span>&nbsp;<span id='{15}_CurrentEquipment_{12}' style='margin-left:0px;'>{4}</span></div>
                                  <div><span style='font-weight: 600;margin-left:0px;'>{10}:</span>&nbsp;<span id='{15}_CurrentPhysicalLocation_{12}' style='margin-left:0px;'>{5}</span></div>
                                  <div><span style='font-weight: 600;margin-left:0px;'>{11}:</span>&nbsp;<span id='{15}_CurrentPhysicalPosition_{12}' style='margin-left:0px;'>{6}</span></div>
                                  </div>", style, icon, Name, GetStateLabel(Int32.Parse(currentState)), currentToolEquipment, currentPhysicalLocation, currentPhysicalPosition, lblType, lblState, lblCurrentEqp, lblCurrentPhysLoc, lblCurrentPhysPos, i, Type.ToString(), lblTool, IdTool, existingToolName));
                    }
                    else
                    {
                        string IdToolSelValBtn = string.Format("{0}_SelValBtn", IdTool);

                        List<string> TabControlElements = new List<string>();
                        TabControlElements = HttpContext.Current.Session[_TabElementsIdentifier] as List<string>;

                        if (!TabControlElements.Contains(IdTool))
                            TabControlElements.Add(IdTool);
                        if (!TabControlElements.Contains(IdToolSelValBtn))
                            TabControlElements.Add(IdToolSelValBtn);
                        HttpContext.Current.Session["_TabElementsIdentifier"] = TabControlElements;

                        htmlText.AppendLine(string.Format(@"<div class='pretrackin-card-container' style='{0}'>
                                  {1}
                                  <div style='margin:0px; width:400px;'><span class='cs-label' locallabel='Tool' defaultlabel='Tool'>{14}</span>
                                      <span class='cs-textbox'>
                                      <input id='{15}' type='text' autocomplete='off' style='margin-left:0px;' value='{17}' onchange=""function _onChange(_value){{$('#ctl00_WebPartManager_BlankWP0_HiddenOnChangeField_ctl00').val('{15}_Tool_{12}:'.concat(_value));return true;}} _onChange($(this).val()); $('#ctl00_WebPartManager_BlankWP0_RefreshFieldData_ctl00')[0].click();""></span>
                                      <input id='{15}_SelValBtn' type='submit' value='' onclick=""$('#ctl00_WebPartManager_BlankWP0_HiddenSelectedValueField_ctl00').val('{15}'); $('#ctl00_WebPartManager_BlankWP0_HiddenToolFamily_ctl00').val('{2}'); $('#ctl00_WebPartManager_BlankWP0_{13}SelValPopupBtn')[0].click();"" class='cs-button-image' style='width: 28px; background-repeat:no-repeat; background-position:center; background-image:url(themes/Horizon/images/User/SCS/SS_LotSelection.svg); border-color:#D4D4D4;margin-left: -50px !important;margin-top: 7px !important;'>
								  </div>
                                  <div style='margin:50px 0 0 0; width:400px;'><span style='font-weight: 600;margin-left:0px;'>{8}:</span>&nbsp;<span id='{15}_State_{12}' style='margin-left:0px;'>{3}</span></div>
                                  </div>
                                  <div class='pretrackin-card-container' style='{0}'>
                                  {1}
                                  <div><span style='font-weight: 600;margin-left:0px;'>{9}:</span>&nbsp;<span id='{15}_CurrentEquipment_{12}' style='margin-left:0px;'>{4}</span></div>                                 
                                  <div><span style='font-weight: 600;margin-left:0px;'>{10}:</span>&nbsp;<span id='{15}_CurrentPhysicalLocation_{12}' style='margin-left:0px;'>{5}</span></div>
                                  <div><span style='font-weight: 600;margin-left:0px;'>{11}:</span>&nbsp;<span id='{15}_CurrentPhysicalPosition_{12}' style='margin-left:0px;'>{6}</span></div>
                                  </div>", style, icon, Name, GetStateLabel(Int32.Parse(currentState)), currentToolEquipment, currentPhysicalLocation, currentPhysicalPosition, lblType, lblState, lblCurrentEqp, lblCurrentPhysLoc, lblCurrentPhysPos, i, Type.ToString(), lblTool, IdTool, existingToolName, selectedTool));
                    }

                    if (currentState == "0" && selectedEquipment != currentToolEquipment)
                    {
                        string IdPhysicalLocationSelValBtn = string.Format("{0}_SelValBtn", IdPhysicalLocation);
                        string IdPhysicalPositionSelValBtn = string.Format("{0}_SelValBtn", IdPhysicalPosition);
                        string IdExecuteBtn = string.Format("{0}_Execute_{1}", Type.ToString(), i);

                        List<string> TabControlElements = new List<string>();
                        TabControlElements = HttpContext.Current.Session[_TabElementsIdentifier] as List<string>;

                        if (!TabControlElements.Contains(IdPhysicalLocation))
                            TabControlElements.Add(IdPhysicalLocation);
                        if (!TabControlElements.Contains(IdPhysicalLocationSelValBtn))
                            TabControlElements.Add(IdPhysicalLocationSelValBtn);
                        if (!TabControlElements.Contains(IdPhysicalPosition))
                            TabControlElements.Add(IdPhysicalPosition);
                        if (!TabControlElements.Contains(IdPhysicalPositionSelValBtn))
                            TabControlElements.Add(IdPhysicalPositionSelValBtn);
                        if (!TabControlElements.Contains(IdExecuteBtn))
                            TabControlElements.Add(IdExecuteBtn);
                        HttpContext.Current.Session["_TabElementsIdentifier"] = TabControlElements;

                        string disabled = null;
                        if (string.IsNullOrWhiteSpace(exisitingPhysicalPosition))
                        {
                            disabled = "disabled='disabled'";
                        }

                        if (string.IsNullOrWhiteSpace(selectedTool))
                        {
                            selectedTool = existingToolName;
                        }

                        htmlText.AppendLine(string.Format(@"<div class='pretrackin-card-container' style='padding-top:0px !important;'>
                                      <div style='margin:30px 0 0 0; float:right;'><input id='{11}_Execute_{8}' type='submit' value='' onclick=""$('#ctl00_WebPartManager_BlankWP0_HiddenExecuteInputData_ctl00').val('{18}:'.concat($('#{0}').val().concat(':'.concat($('#{2}').val()).concat(':{7}')))); $('#ctl00_WebPartManager_BlankWP0_Execute{11}Setup_ctl00')[0].click();"" {9} class='cs-button-image' style='width: 28px; background-repeat:no-repeat; background-position:center; background-image:url(./assets/image/cmdMaterialRequest24.svg); border-color:#D4D4D4;margin-left: 0px !important;margin-top: 7px !important;'></div>
                                      <div style='margin:0px; width:400px;'><span class='cs-label' locallabel='New Physical Location' defaultlabel='New Physical Location'>{4}</span>
                                      <span class='cs-textbox'>
                                      <input id='{0}' type='text' autocomplete='off' style='margin-left:0px;' value='{1}' onchange=""function _onChange(_value){{$('#ctl00_WebPartManager_BlankWP0_HiddenOnChangeField_ctl00').val('{0}:'.concat(_value));return true;}} _onChange($(this).val()); $('#ctl00_WebPartManager_BlankWP0_RefreshFieldData_ctl00')[0].click();""></span>
                                      <input id='{0}_SelValBtn' type='submit' value='' onclick=""$('#ctl00_WebPartManager_BlankWP0_HiddenSelectedValueField_ctl00').val('{0}'); $('#ctl00_WebPartManager_BlankWP0_PhysicalLocationToolSelValPopupBtn')[0].click();"" class='cs-button-image' style='width: 28px; background-repeat:no-repeat; background-position:center; background-image:url(themes/Horizon/images/User/SCS/SS_LotSelection.svg); border-color:#D4D4D4;margin-left: -50px !important;margin-top: 7px !important;'>
                                      </div>
                                      <div style='margin:0px; width:400px;'>
                                      <span class='cs-label' locallabel='New Physical Position' defaultlabel='New Physical Position'>{5}</span>
                                      <span class='cs-textbox'>
                                      <input id='{2}' type='text' autocomplete='off' style='margin-left:0px;' value='{3}' onchange=""function _onChange(_value){{$('#ctl00_WebPartManager_BlankWP0_HiddenOnChangeField_ctl00').val('{2}:'.concat(_value));$('#{11}_Execute_{8}').attr('disabled', 'disabled'); if(_value != ''){{$('#{11}_Execute_{8}').removeAttr('disabled');}}return true;}} _onChange($(this).val());$('#ctl00_WebPartManager_BlankWP0_RefreshFieldData_ctl00')[0].click();""></span>
                                      <input id='{2}_SelValBtn' type='submit' value='' onclick=""if($('#{0}').val() == ''){{__page.displayStatus('{10}', 'Warning', 'Warning'); return false;}}$('#ctl00_WebPartManager_BlankWP0_HiddenSelectedValueField_ctl00').val('{2}');$('#ctl00_WebPartManager_BlankWP0_HiddenPhysicalLocation_ctl00').val($('#{0}').val().trim());$('#ctl00_WebPartManager_BlankWP0_PhysicalPositionSelValPopupBtn')[0].click();"" class='cs-button-image' style='width: 28px; background-repeat:no-repeat; background-position:center; background-image:url(themes/Horizon/images/User/SCS/SS_LotSelection.svg); border-color:#D4D4D4;margin-left: -50px !important;margin-top: 7px !important;'>
                                      </div>
                        </div>", IdPhysicalLocation, exisitingPhysicalLocation, IdPhysicalPosition, exisitingPhysicalPosition, lblNewPhysLoc, lblNewPhysPos, Name, CurrentEquipment, i, disabled, labelValues["PhysicalLocationEmpty"], Type.ToString(), lblQty, Qty, lblTool, IdTool, existingToolName, labelValues["ToolSelectionEmpty"], selectedTool));
                    }
                }
                htmlText.Append("</div>");
                return htmlText.ToString();
            }
        }

        protected class MaterialDetail
        {
            public string Name;
            public string MaterialLot;
            public MaterialLoadEnum Load;
            public string RequestedQty;
            public string LoadedQty;

            public string getHtml(string id, int index, string selectedEquipment, string materialLot, string materialQty, Dictionary<string, string> FieldDataListDic, string hiddenQty)
            {
                string style = "background: #EDFBF5;";
                string icon = null;

                if (Load == MaterialLoadEnum.NOT_LOADED)
                {
                    style = "background: #FFF7CD;";
                    icon = "<div style='float:right;min-height:32px;min-width:32px;margin-top:10px;'></div>";
                }
                else if (Load == MaterialLoadEnum.LOADED)
                {
                    style = "background: #EDFBF5;";
                    icon = "<div style='float:right;background-image:url(./assets/image/indicatorCheckmarkGreen16.svg);min-height:32px;min-width:32px;margin-top:10px;'></div>";
                }
                else if (Load == MaterialLoadEnum.LOADED_NOT_REQUIRED)
                {
                    style = "background: #E3FAFF;";
                    icon = "<div style='float:right;min-height:32px;min-width:32px;margin-top:10px;'></div>";
                }

                StringBuilder htmlText = new StringBuilder();
                string lblMaterial = (labelValues["Material"] != null) ? labelValues["Material"] : "Material";
                string lblRequestedQty = (labelValues["RequestedQty"] != null) ? labelValues["RequestedQty"] : "Requested Quantity";
                string lblMaterialLot = (labelValues["MaterialLot"] != null) ? labelValues["MaterialLot"] : "Material Lot";
                string lblMaterialQty = (labelValues["MaterialQty"] != null) ? labelValues["MaterialQty"] : "Quantity";

                htmlText.Append(string.Format(@"<div class='pretrackin-card' style='{0} max-height: 356px; overflow-x: hidden; overflow-y:auto;'>
                                  <div class='pretrackin-card-container'>
                                  {1}
                                  <div><span style='font-weight: 600;margin-left:0px;'>{4}:</span> {2}</div>
                                  <div><span style='font-weight: 600;margin-left:0px;'>{5}:</span> {3}</div>
                                  </div>", style, icon, Name, RequestedQty, lblMaterial, lblRequestedQty, lblMaterialLot, lblMaterialQty));

                if (Load == MaterialLoadEnum.LOADED)
                {
                    string IdMaterialLot = string.Format("material{0}-material-lot", index);
                    string IdMaterialQty = string.Format("material{0}-material-quantity", index);
                    string existingMaterialLot = MaterialLot;
                    string existingMaterialQty = LoadedQty;

                    string disabled = null;
                    if (existingMaterialLot == "")
                    {
                        disabled = "disabled='disabled'";
                    }

                    htmlText.AppendLine(string.Format(@"<div class='pretrackin-card-container';'>
                                      <div style='margin:0px; width:400px;'>
                                      <span class='cs-label' locallabel='Material Lot' defaultlabel='Material Lot'>{4}</span>
                                      <span class='cs-textbox'>
                                      <input id='{0}' type='text' disabled='disabled' autocomplete='off' style='margin-left:0px;' value='{1}' onchange=""function _onChange(_value){{$('#ctl00_WebPartManager_BlankWP0_HiddenOnChangeField_ctl00').val('{0}:'.concat(_value));return true;}} _onChange($(this).val()); $('#ctl00_WebPartManager_BlankWP0_btnRefreshFieldData')[0].click();""></span>
                                      </div>
                                      <div style='margin:0px; width:400px;'>
                                      <span class='cs-label' locallabel='Quantity' defaultlabel='Quantity'>{5}</span>
                                      <span class='cs-textbox'>
                                     <input id='{2}' type='text' disabled='disabled' autocomplete='off' style='margin-left:0px;' value='{3}' onchange=""function _onChange(_value){{$('#ctl00_WebPartManager_BlankWP0_HiddenOnChangeField_ctl00').val('{2}:'.concat(_value));$('#materialExecute{7}').attr('disabled', 'disabled'); if(_value != ''){{$('#materialExecute{7}').removeAttr('disabled');}}return true;}} _onChange($(this).val());$('#ctl00_WebPartManager_BlankWP0_btnRefreshFieldData')[0].click();""></span>
                    </div>", IdMaterialLot, existingMaterialLot, IdMaterialQty, existingMaterialQty, lblMaterialLot, lblMaterialQty, Name, index, disabled));
                }

                if (Load == MaterialLoadEnum.NOT_LOADED && selectedEquipment != null)
                {
                    string IdMaterialLot = string.Format("material{0}-material-lot", index);
                    string IdMaterialQty = string.Format("material{0}-material-quantity", index);
                    string existingMaterialLot = (FieldDataListDic != null && FieldDataListDic.Keys.Contains(IdMaterialLot)) ? FieldDataListDic[IdMaterialLot] : "";
                    string existingMaterialQty = (FieldDataListDic != null && FieldDataListDic.Keys.Contains(IdMaterialQty)) ? FieldDataListDic[IdMaterialQty] : "";

                    string disabled = null;
                    if (existingMaterialLot == "")
                    {
                        disabled = "disabled='disabled'";
                    }

                    htmlText.AppendLine(string.Format(@"<div class='pretrackin-card-container';'>
                                      <div style='margin:0px; float:right;'><input id='materialExecute{7}' type='submit' value='' onclick=""$('#ctl00_WebPartManager_BlankWP0_HiddenExecuteInputData_ctl00').val('{6}:'.concat($('#{0}').val().concat(':'.concat($('#{2}').val())))); $('#ctl00_WebPartManager_BlankWP0_ExecuteMaterialSetup_ctl00')[0].click();"" {8} class='cs-button-image' style='width: 28px; background-repeat:no-repeat; background-position:center; background-image:url(./assets/image/cmdMaterialRequest24.svg); border-color:#D4D4D4;margin-left: 0px !important;'></div>
                                      <div style='margin:0px; width:400px;'>
                                      <span class='cs-label' locallabel='Material Lot' defaultlabel='Material Lot'>{4}</span>
                                      <span class='cs-textbox'>
                                      <input id='{0}' type='text' autocomplete='off' style='margin-left:0px;' value='{1}' onchange=""function _onChange(_value){{$('#ctl00_WebPartManager_BlankWP0_HiddenOnChangeField_ctl00').val('{0}:'.concat(_value));return true;}} _onChange($(this).val()); $('#ctl00_WebPartManager_BlankWP0_btnRefreshFieldData')[0].click();""></span>
                                      <input id='{0}_SelValBtn' type='submit' value='' onclick=""$('#ctl00_WebPartManager_BlankWP0_HiddenSelectedValueField_ctl00').val('{0}'); $('#ctl00_WebPartManager_BlankWP0_MaterialLotSelValPopupBtn')[0].click(); $('#ctl00_WebPartManager_BlankWP0_IsMaterialPopup_ctl00').prop('checked', true); $('#ctl00_WebPartManager_BlankWP0_HiddenMaterialLotQty_ctl00').val('{2}');"" class='cs-button-image' style='width: 28px; background-repeat:no-repeat; background-position:center; background-image:url(themes/Horizon/images/User/SCS/SS_LotSelection.svg); border-color:#D4D4D4;margin-left: -50px !important;margin-top: 7px !important;'>
                                      </div>
                                      <div style='margin:0px; width:400px;'>
                                      <span class='cs-label' locallabel='Quantity' defaultlabel='Quantity'>{5}</span>
                                      <span class='cs-textbox'>
                                      <input id='{2}' type='text' autocomplete='off' style='margin-left:0px;' value='{3}' onchange=""function _onChange(_value){{$('#ctl00_WebPartManager_BlankWP0_HiddenOnChangeField_ctl00').val('{2}:'.concat(_value));$('#materialExecute{7}').attr('disabled', 'disabled'); if(_value != ''){{$('#materialExecute{7}').removeAttr('disabled');}}return true;}} _onChange($(this).val());$('#ctl00_WebPartManager_BlankWP0_btnRefreshFieldData')[0].click();""></span>
                    </div>", IdMaterialLot, existingMaterialLot, IdMaterialQty, existingMaterialQty, lblMaterialLot, lblMaterialQty, Name, index, disabled));
                }

                else if (Load == MaterialLoadEnum.LOADED_NOT_REQUIRED)
                {
                    string IdMaterialLot = string.Format("material{0}-material-lot", index);
                    string IdMaterialQty = string.Format("material{0}-material-quantity", index);
                    string existingMaterialLot = MaterialLot;
                    string existingMaterialQty = LoadedQty;
                    string newMaterialLot = (FieldDataListDic != null && FieldDataListDic.Keys.Contains(IdMaterialLot)) ? FieldDataListDic[IdMaterialLot] : "";
                    string newMaterialQty = (FieldDataListDic != null && FieldDataListDic.Keys.Contains(IdMaterialQty)) ? FieldDataListDic[IdMaterialQty] : "";

                    string disabled = null;
                    if (existingMaterialLot == "")
                    {
                        disabled = "disabled='disabled'";
                    }

                    htmlText.AppendLine(string.Format(@"<div class='pretrackin-card-container';'>
                                      <div style='margin:0px; float:right;'><input id='materialExecute{7}' type='submit' value='' onclick=""$('#ctl00_WebPartManager_BlankWP0_HiddenExecuteInputData_ctl00').val('{6}:'.concat($('#{0}').val().concat(':'.concat($('#{2}').val())))); $('#ctl00_WebPartManager_BlankWP0_ExecuteUnloadMaterialSetup_ctl00')[0].click();"" {8} class='cs-button-image' style='width: 28px; background-repeat:no-repeat; background-position:center; background-image:url(./assets/image/cmdMaterialRequest24.svg); border-color:#D4D4D4;margin-left: 0px !important;'></div>
                                      <div style='margin:0px; width:400px;'>
                                      <span class='cs-label' locallabel='Material Lot' defaultlabel='Material Lot'>{4}</span>
                                      <span class='cs-textbox'>
                                      <input id='{0}' type='text' disabled='disabled' autocomplete='off' style='margin-left:0px;' value='{1}' onchange=""function _onChange(_value){{$('#ctl00_WebPartManager_BlankWP0_HiddenOnChangeField_ctl00').val('{0}:'.concat(_value));return true;}} _onChange($(this).val()); $('#ctl00_WebPartManager_BlankWP0_btnRefreshFieldData')[0].click();""></span>                                                                                            
                                      </div>
                                      <div style='margin:0px; width:400px;'>
                                      <span class='cs-label' locallabel='Quantity' defaultlabel='Quantity'>{5}</span>
                                      <span class='cs-textbox'>
                                      <input id='{2}' type='text' disabled='disabled' autocomplete='off' style='margin-left:0px;' value='{3}' onchange=""function _onChange(_value){{$('#ctl00_WebPartManager_BlankWP0_HiddenOnChangeField_ctl00').val('{2}:'.concat(_value)); return true;}} _onChange($(this).val());$('#ctl00_WebPartManager_BlankWP0_btnRefreshFieldData')[0].click();""></span>
                    </div>", IdMaterialLot, existingMaterialLot, IdMaterialQty, existingMaterialQty, lblMaterialLot, lblMaterialQty, Name, index, disabled));
                }
                htmlText.Append("</div>");

                return htmlText.ToString();
            }
        }
        #endregion
    }
}