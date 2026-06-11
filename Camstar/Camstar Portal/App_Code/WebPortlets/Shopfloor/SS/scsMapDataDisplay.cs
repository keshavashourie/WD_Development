/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.Script.Serialization;

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
using SEMI.AppCode;
using SWC = System.Web.UI.WebControls;
using System.Collections;

/// <summary>
/// Summary description for scsMapDataDisplay
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class scsMapDataDisplay : MatrixWebPart
    {
        protected virtual CWC.TextBox _txtSubstrateId { get { return Page.FindCamstarControl("scsMapDataDisplay_SubstrateId") as CWC.TextBox;  } }
        protected virtual CWC.DropDownList _ddlLotId { get { return Page.FindCamstarControl("scsMapDataDisplay_LotId") as CWC.DropDownList; } }
        protected virtual CWC.TextBox _txtMapName { get { return Page.FindCamstarControl("scsMapDataDisplay_MapName") as CWC.TextBox; } }
        protected virtual CWC.TextBox _txtMapVersion { get { return Page.FindCamstarControl("scsMapDataDisplay_MapVersion") as CWC.TextBox; } }
        protected virtual CWC.TextBox _txtXDimension { get { return Page.FindCamstarControl("scsMapDataDisplay_XDimension") as CWC.TextBox; } }
        protected virtual CWC.TextBox _txtYDimension { get { return Page.FindCamstarControl("scsMapDataDisplay_YDimension") as CWC.TextBox; } }
        protected virtual CWC.TextBox _txtCurrentBinCode { get { return Page.FindCamstarControl("scsMapDataDisplay_CurrentBinCode") as CWC.TextBox; } }
        protected virtual CWC.TextBox _txtNewBinCode { get { return Page.FindCamstarControl("scsMapDataDisplay_NewBinCode") as CWC.TextBox; } }
        protected virtual CWC.TextBox _txtHiddenMapDataName { get { return Page.FindCamstarControl("HiddenMapDataName") as CWC.TextBox; } }
        protected virtual CWC.TextBox _txtHiddenMapType { get { return Page.FindCamstarControl("HiddenMapType") as CWC.TextBox; } }
        protected virtual CWC.TextBox _txtHiddenBinType { get { return Page.FindCamstarControl("HiddenBinType") as CWC.TextBox; } }
        protected virtual CWC.TextBox _txtHiddenOriginLocation { get { return Page.FindCamstarControl("HiddenOriginLocation") as CWC.TextBox; } }
        protected virtual CWC.TextBox _txtHiddenAxisDirection { get { return Page.FindCamstarControl("HiddenAxisDirection") as CWC.TextBox; } }
        protected virtual CWC.TextBox _txtHiddenGoodDevices { get { return Page.FindCamstarControl("HiddenGoodDevices") as CWC.TextBox; } }
        protected virtual CWC.TextBox _txtHiddenDimensionX { get { return Page.FindCamstarControl("HiddenDimensionX") as CWC.TextBox; } }
        protected virtual CWC.TextBox _txtHiddenDimensionY { get { return Page.FindCamstarControl("HiddenDimensionY") as CWC.TextBox; } }
        protected virtual CWC.TextBox _txtSubstrateMapIndex { get { return Page.FindCamstarControl("scsMapDataDisplay_SubstrateMapIndex") as CWC.TextBox; } }
        protected virtual CWC.TextBox _txtOverlayIndex { get { return Page.FindCamstarControl("scsMapDataDisplay_OverlayIndex") as CWC.TextBox; } }
        protected virtual CWC.TextBox _txtBinCodeMapIndex { get { return Page.FindCamstarControl("scsMapDataDisplay_BinCodeMapIndex") as CWC.TextBox; } }
        protected virtual CWC.TextBox _txtSubstrateIndex { get { return Page.FindCamstarControl("scsMapDataDisplay_SubstrateIndex") as CWC.TextBox; } }
        protected virtual CWC.TextBox _txtBinDefinitions { get { return Page.FindCamstarControl("BinDefinitions") as CWC.TextBox; } }
        protected virtual JQDataGrid _gridBinCodeUpdateDetails { get { return Page.FindCamstarControl("scsMapDataDisplay_BinCodeUpdateDetails") as JQDataGrid; } }
        protected virtual JQDataGrid _gridBinDefinitionDetails { get { return Page.FindCamstarControl("scsMapDataDisplay_BinDefinitionDetails") as JQDataGrid; } }
        protected virtual JQDataGrid _gridBinCodeDetails {  get { return Page.FindCamstarControl("scsMapDataDisplay_BinCodeDetails") as JQDataGrid; } }
        protected virtual CWC.Button _btnFullView { get { return Page.FindCamstarControl("FullViewBtn") as CWC.Button; } }
        
        //-----------------------------------------
        // Substrate Id Data Changed Event
        //-----------------------------------------
        public void SubstrateIdField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (_txtSubstrateId.Data != null)
                {
                    Page.StatusBar.ClearMessage();
                    _ddlLotId.ClearData();
                    _ddlLotId.ClearSelectionValues();
                    _gridBinCodeUpdateDetails.ClearData();
                    FetchLotIdSelection(_txtSubstrateId.Data.ToString());
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // LotId Data Changed Event
        //-----------------------------------------
        public void LotIdField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (_txtSubstrateId.Data != null && _ddlLotId.Data != null)
                {
                    Page.StatusBar.ClearMessage();
                    _gridBinCodeUpdateDetails.ClearData();
                    FetchData(_txtSubstrateId.Data.ToString());
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //---------------------------------------------------
        // Fetch Data function
        //---------------------------------------------------
        public void FetchData(string SubstrateId)
        {
            try
            {
                int queryResultsetSize = 2000;
                var mergedBinCodeDetails = new List<scsBinCodeDetails>();
                var binDefinitionList = new List<scsBinDefinitionTxn>();
                var mapDataList = new List<scsMapDataTxn>();

                var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue, RecursionLimit = 100 };

                //Initialize Service & Objects
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                scsMapDataDisplayService Svc = new scsMapDataDisplayService(profile);
                Camstar.WCF.ObjectStack.scsMapDataDisplay SvcData = new Camstar.WCF.ObjectStack.scsMapDataDisplay();
                scsMapDataDisplay_Info SvcInfo = new scsMapDataDisplay_Info();
                scsMapDataDisplay_Request ReqData = new scsMapDataDisplay_Request();
                scsMapDataDisplay_Result ResData = new scsMapDataDisplay_Result();

                //Set Input Data
                SvcData.SubstrateId = new Primitive<string>();
                SvcData.SubstrateId = SubstrateId;

                if (_ddlLotId.Data != null && _ddlLotId.Data.ToString() != "NULL")
                {
                    SvcData.LotId = new Primitive<string>();
                    SvcData.LotId = _ddlLotId.Data.ToString();
                }

                //Set Request Value
                SvcInfo.MapDataDetails = new scsMapDataDetails_Info();
                SvcInfo.MapDataDetails.RequestValue = true;
                ReqData.Info = SvcInfo;
                
                //Execute Request 
                ResultStatus GetMapDataDetails = Svc.GetMapDataDetails(SvcData, ReqData, out ResData);

                //Result
                if (GetMapDataDetails.IsSuccess)
                {
                    if (ResData.Value.MapDataDetails.scsBinCodeMapId != null)
                    {
                        this.Page.ShopfloorReset(null, null);
                        _txtHiddenMapDataName.Data = ResData.Value.MapDataDetails.scsMapDataName.ToString();
                        _txtHiddenBinType.Data = ResData.Value.MapDataDetails.BinType.ToString();

                        if (ResData.Value.MapDataDetails.MapName != null)
                            _txtMapName.Data = ResData.Value.MapDataDetails.MapName.ToString();

                        if (ResData.Value.MapDataDetails.MapVersion != null)
                            _txtMapVersion.Data = ResData.Value.MapDataDetails.MapVersion.ToString();

                        if (ResData.Value.MapDataDetails.SubstrateMapIndex != null)
                            _txtSubstrateMapIndex.Data = ResData.Value.MapDataDetails.SubstrateMapIndex.ToString();

                        if (ResData.Value.MapDataDetails.OverlayIndex != null)
                            _txtOverlayIndex.Data = ResData.Value.MapDataDetails.OverlayIndex.ToString();

                        if (ResData.Value.MapDataDetails.BinCodeMapIndex != null)
                            _txtBinCodeMapIndex.Data = ResData.Value.MapDataDetails.BinCodeMapIndex.ToString();

                        if (ResData.Value.MapDataDetails.SubstrateIndex != null)
                            _txtSubstrateIndex.Data = ResData.Value.MapDataDetails.SubstrateIndex.ToString();

                        if (ResData.Value.MapDataDetails.GoodDevices != null)
                            _txtHiddenGoodDevices.Data = ResData.Value.MapDataDetails.GoodDevices.ToString();

                        if (ResData.Value.MapDataDetails.DimensionX != null)
                            _txtHiddenDimensionX.Data = ResData.Value.MapDataDetails.DimensionX.ToString();

                        if (ResData.Value.MapDataDetails.DimensionY != null)
                            _txtHiddenDimensionY.Data = ResData.Value.MapDataDetails.DimensionY.ToString();

                        if (ResData.Value.MapDataDetails.MapType != null)
                            _txtHiddenMapType.Data = ResData.Value.MapDataDetails.MapType.ToString();
                        else
                            _txtHiddenMapType.Data = "2DArray";

                        if (ResData.Value.MapDataDetails.OriginLocation != null)
                            _txtHiddenOriginLocation.Data = ResData.Value.MapDataDetails.OriginLocation.ToString();
                        else
                            _txtHiddenOriginLocation.Data = "Default";

                        if (ResData.Value.MapDataDetails.AxisDirection != null)
                            _txtHiddenAxisDirection.Data = ResData.Value.MapDataDetails.AxisDirection.ToString();
                        else
                            _txtHiddenAxisDirection.Data = "Default";

                        //Set Input Data
                        SvcData.scsBinCodeMapId = new Primitive<string>();
                        SvcData.scsBinCodeMapId = ResData.Value.MapDataDetails.scsBinCodeMapId;

                        //Set Request Value
                        SvcInfo.BinDefinitionDetails = new scsBinDefinitionDetails_Info();
                        SvcInfo.BinDefinitionDetails.RequestValue = true;

                        SvcInfo.TxnCount = new Info(true);

                        SvcInfo.BinCodeDetails = new scsBinCodeDetails_Info();
                        SvcInfo.BinCodeDetails.RequestValue = true;

                        ReqData.Info = SvcInfo;

                        scsMapDataDisplay_Result BinDefinitionResData = new scsMapDataDisplay_Result();
                        scsMapDataDisplay_Result CountBinCodeData = new scsMapDataDisplay_Result();
                        scsMapDataDisplay_Result BinCodeResData = new scsMapDataDisplay_Result();

                        //Execute Request
                        ResultStatus GetBinDefinitionDetails = Svc.GetBinDefinitionDetails(SvcData, ReqData, out BinDefinitionResData);
                        if (GetBinDefinitionDetails.IsSuccess)
                        {
                            if (BinDefinitionResData.Value.BinDefinitionDetails != null)
                            {
                                //Save bin definition details to grid
                                _gridBinDefinitionDetails.Data = BinDefinitionResData.Value.BinDefinitionDetails;

                                //Build custom bin definition list for javascript function
                                foreach (scsBinDefinitionDetails binDefinitionDetails in BinDefinitionResData.Value.BinDefinitionDetails)
                                {
                                    string binCode = binDefinitionDetails.BinCode != null ? binDefinitionDetails.BinCode.Value : null;
                                    int binCount = binDefinitionDetails.BinCount != null ? binDefinitionDetails.BinCount.Value : 0;
                                    string binDescription = binDefinitionDetails.BinDescription != null ? binDefinitionDetails.BinDescription.Value : null;
                                    string binQuality = binDefinitionDetails.BinQuality != null ? binDefinitionDetails.BinQuality.Value : null;
                                    binDefinitionList.Add(new scsBinDefinitionTxn() { code = binCode, count = binCount, description = binDescription, quality = binQuality });
                                }
                            }
                        }

                        ResultStatus CountBinCodes = Svc.CountBinCodeDetails(SvcData, ReqData, out CountBinCodeData);
                        if (CountBinCodes.IsSuccess)
                        {
                            var LoopCount = CountBinCodeData.Value.TxnCount.Value / queryResultsetSize;
                            for (int i = 0; i <= LoopCount; i++)
                            {
                                int startSequence = (i * queryResultsetSize) + 1;
                                int stopSequence = queryResultsetSize * (i + 1);
                                SvcData.SequenceStart = startSequence;
                                SvcData.SequenceEnd = stopSequence;
                                ResultStatus GetBinCodeDetails = Svc.GetBinCodeDetails(SvcData, ReqData, out BinCodeResData);
                                if (GetBinCodeDetails.IsSuccess)
                                {
                                    if (BinCodeResData.Value.BinCodeDetails != null)
                                    {
                                        mergedBinCodeDetails.AddRange(BinCodeResData.Value.BinCodeDetails.ToList());
                                    }
                                }
                            }
                            if (mergedBinCodeDetails.Count() > 0)
                            {
                                //Sort bin code details by sequence
                                List<scsBinCodeDetails> SortedBinCodeDetailsList = mergedBinCodeDetails.OrderBy(o => o.BinCodeSequence.Value).ToList();

                                //Save bin code details to grid
                                _gridBinCodeDetails.Data = SortedBinCodeDetailsList.ToArray();

                                //Build custom map data list for javascript function
                                foreach (scsBinCodeDetails binCodeDetails in SortedBinCodeDetailsList.ToArray())
                                {
                                    int binCodeSequence = binCodeDetails.BinCodeSequence != null ? binCodeDetails.BinCodeSequence.Value : 0;
                                    int binCodeX = binCodeDetails.BinCodeX != null ? binCodeDetails.BinCodeX.Value : 0;
                                    int binCodeY = binCodeDetails.BinCodeY != null ? binCodeDetails.BinCodeY.Value : 0;
                                    string binCodeValues = binCodeDetails.BinCodeValues != null ? binCodeDetails.BinCodeValues.Value : "";
                                    mapDataList.Add(new scsMapDataTxn() { sequence = binCodeSequence, x = binCodeX, y = binCodeY, value = binCodeValues });
                                }
                                string serializedBinDefinitionList = null;
                                if (binDefinitionList.Count > 0)
                                {
                                    serializedBinDefinitionList = serializer.Serialize(binDefinitionList);
                                }
                                string serializedMapDataList = serializer.Serialize(mapDataList);

                                //If MapDataList not null, call javascript to render the map (and enable full view button)
                                if (serializedMapDataList != null)
                                {
                                    string oMapType = ResData.Value.MapDataDetails.MapType != null ? ResData.Value.MapDataDetails.MapType.Value : "2DArray";
                                    int oDeviceSizeX = ResData.Value.MapDataDetails.DeviceSizeX != null ? Convert.ToInt32(ResData.Value.MapDataDetails.DeviceSizeX.Value) : 0;
                                    int oDeviceSizeY = ResData.Value.MapDataDetails.DeviceSizeY != null ? Convert.ToInt32(ResData.Value.MapDataDetails.DeviceSizeY.Value) : 0;
                                    ScriptManager.RegisterStartupScript(this.Page.Form, GetType(), "RenderMapDataCall", String.Format("RenderMapDataInCanvasRaw('{0}', '{1}', '{2}', '{3}', {4}, {5}, {6}, {7}, '{8}', {9});", serializedMapDataList, serializedBinDefinitionList, oMapType, ResData.Value.MapDataDetails.BinType, oDeviceSizeX, oDeviceSizeY, ResData.Value.MapDataDetails.DimensionX, ResData.Value.MapDataDetails.DimensionY, ResData.Value.MapDataDetails.NullBin, 0), true);
                                    _btnFullView.Enabled = true;
                                }
                            }
                        }
                        else
                        {
                            Page.DisplayMessage(CountBinCodes);
                        }
                    }
                } //Results.IsSuccess
                else
                {
                    Page.ShopfloorReset(null, null);
                    _ddlLotId.ClearData();
                    _ddlLotId.ClearSelectionValues();
                    _gridBinCodeUpdateDetails.ClearData();
                    _btnFullView.Enabled = false;
                    ScriptManager.RegisterStartupScript(this.Page.Form, this.Page.GetType(), "ClearMapData", "ClearMapDataCanvas();", true);
                    Page.DisplayMessage(GetMapDataDetails);
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //---------------------------------------------------
        // Fetch Data function
        //---------------------------------------------------
        public void FetchLotIdSelection(string SubstrateId)
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                QueryService objSvc = new QueryService(fs.CurrentUserProfile);
                QueryParameters objParameters = new QueryParameters();
                objParameters.Parameters = new QueryParameter[1];
                objParameters.Parameters[0] = new QueryParameter("SubstrateId", _txtSubstrateId.Data.ToString());

                QueryOptions objOptions = new QueryOptions();
                objOptions.QueryType = OM.QueryType.System;
                objOptions.StartRow = 1;
                //objOptions.RowSetSize = intRowSize;
                RecordSet objResult;

                // execute the query
                ResultStatus objRS = objSvc.Execute("scsMapDataDisplay_SelVal_LotId", objParameters, objOptions, out objResult);

                if (objRS.IsSuccess)
                {
                    if (objResult.Rows == null)
                    {
                        FetchData(_txtSubstrateId.Data.ToString());
                    }
                    else if (objResult.Rows.Length > 0)
                    {
                        //Manually generate drop down list selection values
                        RecordSet rsNamedObject = new RecordSet();
                        Header[] rsHeaders = new Header[2];
                        Row[] rsRows = new Row[objResult.Rows.Length];

                        rsHeaders[0] = new Header();
                        rsHeaders[0].TypeCode = TypeCode.String;
                        rsHeaders[0].Name = "Name";

                        rsHeaders[1] = new Header();
                        rsHeaders[1].TypeCode = TypeCode.String;
                        rsHeaders[1].Name = "ID";

                        rsNamedObject.Headers = rsHeaders;

                        for (int x = 0; x < objResult.Rows.Length; x++)
                        {
                            rsRows[x] = new Row();
                            string[] strRowValues = new string[2];
                            if (objResult.Rows[x].Values[0] != "")
                            {
                                strRowValues[0] = objResult.Rows[x].Values[0];
                                strRowValues[1] = objResult.Rows[x].Values[0];
                            }
                            else
                            {
                                strRowValues[0] = "NULL";
                                strRowValues[1] = "NULL";
                            }
                            rsRows[x].Values = strRowValues;
                        }

                        rsNamedObject.Rows = rsRows;
                        _ddlLotId.SetSelectionValues(rsNamedObject);

                        //Assign LotId to the first one if only 1 LotId is returned
                        if (objResult.Rows.Length == 1)
                        {
                            _ddlLotId.Data = rsNamedObject.Rows[0].Values[0].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }
        
        //--------------------------------------------------------------------------------------
        // Function to add the modified bin code information into the Bin Code Update Details grid
        //--------------------------------------------------------------------------------------
        protected void AddToBinCodeUpdateDetails()
        {
            try
            {
                scsBinCodeUpdateDetails[] oExistingList = (_gridBinCodeUpdateDetails.GridContext as BoundContext).Data as scsBinCodeUpdateDetails[];
                List<scsBinCodeUpdateDetails> oNewList = new List<scsBinCodeUpdateDetails>();

                if (oExistingList == null)
                    oExistingList = new scsBinCodeUpdateDetails[0];

                // add back the existing rows
                foreach (scsBinCodeUpdateDetails oRow in oExistingList)
                {
                    scsBinCodeUpdateDetails oCurrentRow = new scsBinCodeUpdateDetails();
                    oCurrentRow = oRow;
                    oNewList.Add(oCurrentRow);
                }

                // add the new row
                scsBinCodeUpdateDetails oNewRow = new scsBinCodeUpdateDetails();
                oNewRow.XDimension = Convert.ToInt32(_txtXDimension.Data.ToString());
                oNewRow.YDimension = Convert.ToInt32(_txtYDimension.Data.ToString());
                oNewRow.CurrentBinCode = _txtCurrentBinCode.Data.ToString();
                oNewRow.NewBinCode = _txtNewBinCode.Data.ToString();
                oNewList.Add(oNewRow);

                (_gridBinCodeUpdateDetails.GridContext as BoundContext).Data = oNewList.ToArray();
                _gridBinCodeUpdateDetails.BoundContext.LoadData();
                CamstarWebControl.SetRenderToClient(_gridBinCodeUpdateDetails);
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //---------------------------------------------------
        // Execute Submit
        //---------------------------------------------------
        public ResultStatus ExecuteSubmit(string mapDataName, string mapType, string binType, scsBinCodeUpdateDetails[] binCodeList)
        {
            ResultStatus ReturnResultStatus = new ResultStatus();
            try
            {
                UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as UserProfile;

                //Get the max x & y dimension
                int maxX = _txtHiddenDimensionX.Data != null ? Convert.ToInt32(_txtHiddenDimensionX.Data.ToString()) : 0;
                int maxDimensionX = _txtHiddenDimensionX.Data != null ? Convert.ToInt32(_txtHiddenDimensionX.Data.ToString()) - 1 : 0;
                int maxDimensionY = _txtHiddenDimensionY.Data != null ? Convert.ToInt32(_txtHiddenDimensionY.Data.ToString()) - 1 : 0;
                int goodDevices = _txtHiddenGoodDevices.Data != null ? Convert.ToInt32(_txtHiddenGoodDevices.Data.ToString()) : 0;

                //Get the index of Map Data objects
                int substrateIndex = _txtSubstrateIndex.Data != null ? Convert.ToInt32(_txtSubstrateIndex.Data.ToString()) - 1 : 0;
                int substrateMapIndex = _txtSubstrateMapIndex.Data != null ? Convert.ToInt32(_txtSubstrateMapIndex.Data.ToString()) - 1 : 0;
                int overlayIndex = _txtOverlayIndex.Data != null ? Convert.ToInt32(_txtOverlayIndex.Data.ToString()) - 1 : 0;
                int binCodeMapIndex = _txtBinCodeMapIndex.Data != null ? Convert.ToInt32(_txtBinCodeMapIndex.Data.ToString()) - 1 : 0;
                int newBinCodeIndex = 0;
                int newBinDefinitionDetailsIndex = 0;

                string hiddenBinType = _txtHiddenBinType.Data != null ? _txtHiddenBinType.Data.ToString() : "";
                //Define bin type length according to the bin type
                int binTypeLength = 1;
                if (hiddenBinType.Equals("Decimal", StringComparison.OrdinalIgnoreCase))
                    binTypeLength = 3;
                else if (hiddenBinType.Equals("HexaDecimal", StringComparison.OrdinalIgnoreCase))
                    binTypeLength = 2;
                else if (hiddenBinType.Equals("Integer2", StringComparison.OrdinalIgnoreCase))
                    binTypeLength = 4;

                //Get existing bin code details & bin definition details
                scsBinCodeDetails[] oExistingBinCodeDetails = (_gridBinCodeDetails.GridContext as BoundContext).Data as scsBinCodeDetails[];
                scsBinDefinitionDetails[] oExistingBinDefinitionDetails;
                if (_gridBinDefinitionDetails.TotalRowCount > 0)
                {
                    oExistingBinDefinitionDetails = (_gridBinDefinitionDetails.GridContext as BoundContext).Data as scsBinDefinitionDetails[];
                }
                else
                {
                    string binDefinitions = _txtBinDefinitions.Data != null ? _txtBinDefinitions.Data.ToString() : "";
                    string[] separators = { "," };
                    string[] binDefinitionsArray = binDefinitions.Split(separators, StringSplitOptions.None);
                    oExistingBinDefinitionDetails = new scsBinDefinitionDetails[binDefinitionsArray.Length];
                    for (var i = 0; i < binDefinitionsArray.Length; i++)
                    {
                        oExistingBinDefinitionDetails[i] = new scsBinDefinitionDetails();
                        oExistingBinDefinitionDetails[i].BinCode = binDefinitionsArray[i];
                    }
                }

                //Order the list
                if (binCodeList != null)
                    binCodeList = binCodeList.OrderBy(x => x.YDimension.Value).ToArray();
                
                //Initialize Map Data Display Service to validate input
                scsMapDataDisplayService Svc = new scsMapDataDisplayService(profile);
                Camstar.WCF.ObjectStack.scsMapDataDisplay SvcData = new Camstar.WCF.ObjectStack.scsMapDataDisplay();
                scsMapDataDisplay_Info SvcInfo = new scsMapDataDisplay_Info();
                scsMapDataDisplay_Request ReqData = new scsMapDataDisplay_Request();
                scsMapDataDisplay_Result ResData = new scsMapDataDisplay_Result();

                SvcData.BinCodeUpdateDetails = binCodeList;
                SvcData.BinDefinitionDetails = new scsBinDefinitionDetails[oExistingBinDefinitionDetails.Length];
                int binDefIndex = 0;
                foreach(scsBinDefinitionDetails binDef in oExistingBinDefinitionDetails)
                {
                    SvcData.BinDefinitionDetails[binDefIndex] = new scsBinDefinitionDetails();
                    SvcData.BinDefinitionDetails[binDefIndex].BinCode = binDef.BinCode;
                    binDefIndex++;
                }

                //Execute Validation 
                ResultStatus validateBinCodeDetails = Svc.ValidateBinCodeUpdateDetails(SvcData, ReqData, out ResData);

                //Result
                if (validateBinCodeDetails.IsSuccess)
                {
                    //Initiate Map Data Maint Service
                    scsMapDataMaintService objService = new scsMapDataMaintService(profile);
                    scsMapDataMaint objServiceData = new scsMapDataMaint();
                    scsMapDataChanges objChanges = new scsMapDataChanges();
                    scsMapDataMaint_Info objServiceInfo = new scsMapDataMaint_Info();
                    scsMapDataChanges_Info objChangesInfo = new scsMapDataChanges_Info();
                    ResultStatus resultStatus = null;

                    //Build the map data substrate map object changes
                    objChanges.MapDataSubstrateMaps = new scsMapDataSubstrateMapChanges[1];
                    objChanges.MapDataSubstrateMaps[0] = new scsMapDataSubstrateMapChanges();
                    objChanges.MapDataSubstrateMaps[0].ListItemAction = ListItemAction.Change;
                    objChanges.MapDataSubstrateMaps[0].ListItemIndex = substrateMapIndex;
                    objChanges.MapDataSubstrateMaps[0].Overlays = new scsOverlayChanges[1];
                    objChanges.MapDataSubstrateMaps[0].Overlays[0] = new scsOverlayChanges();
                    objChanges.MapDataSubstrateMaps[0].Overlays[0].ListItemAction = ListItemAction.Change;
                    objChanges.MapDataSubstrateMaps[0].Overlays[0].ListItemIndex = overlayIndex;
                    objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps = new scsBinCodeMapChanges[1];
                    objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0] = new scsBinCodeMapChanges();
                    objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].ListItemAction = ListItemAction.Change;
                    objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].ListItemIndex = binCodeMapIndex;

                    //Build the bin definitions changes
                    if (_gridBinDefinitionDetails.TotalRowCount > 0)
                    {
                        objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinDefinitions = new scsBinDefinitionChanges[oExistingBinDefinitionDetails.Length];
                        foreach (scsBinCodeUpdateDetails eachBinCode in binCodeList)
                        {
                            int oldDefIndex = oExistingBinDefinitionDetails.First(item => item.BinCode == eachBinCode.CurrentBinCode.ToString()).DisplaySequence.Value - 1;
                            oExistingBinDefinitionDetails[oldDefIndex].BinCount = oExistingBinDefinitionDetails[oldDefIndex].BinCount.Value - 1;
                            string oldDefQuality = oExistingBinDefinitionDetails.First(item => item.BinCode == eachBinCode.CurrentBinCode.ToString()).BinQuality.Value;
                            if (oldDefQuality.Equals("Pass", StringComparison.OrdinalIgnoreCase))
                                goodDevices = goodDevices - 1;

                            int newDefIndex = oExistingBinDefinitionDetails.First(item => item.BinCode == eachBinCode.NewBinCode.ToString()).DisplaySequence.Value - 1;
                            oExistingBinDefinitionDetails[newDefIndex].BinCount = oExistingBinDefinitionDetails[newDefIndex].BinCount.Value + 1;
                            string newDefQuality = oExistingBinDefinitionDetails.First(item => item.BinCode == eachBinCode.NewBinCode.ToString()).BinQuality.Value;
                            if (newDefQuality.Equals("Pass", StringComparison.OrdinalIgnoreCase))
                                goodDevices = goodDevices + 1;
                        }
                        foreach (scsBinDefinitionDetails eachBinDefinition in oExistingBinDefinitionDetails)
                        {
                            objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinDefinitions[newBinDefinitionDetailsIndex] = new scsBinDefinitionChanges();
                            objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinDefinitions[newBinDefinitionDetailsIndex].ListItemAction = ListItemAction.Change;
                            objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinDefinitions[newBinDefinitionDetailsIndex].ListItemIndex = eachBinDefinition.DisplaySequence.Value - 1;
                            objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinDefinitions[newBinDefinitionDetailsIndex].BinCount = eachBinDefinition.BinCount;
                            newBinDefinitionDetailsIndex++;
                        }
                    }

                    //Build the bin codes changes
                    if (_txtHiddenMapType.Data.ToString().Equals("2DArray", StringComparison.OrdinalIgnoreCase) || _txtHiddenMapType.Data.ToString().Equals("Row/Column", StringComparison.OrdinalIgnoreCase))
                    {
                        //Get unique y dimensions
                        scsBinCodeUpdateDetails[] uniqueYDimensions = binCodeList.GroupBy(x => x.YDimension.Value).Select(g => g.First()).ToArray();

                        objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinCodes = new scsBinCodeChanges[uniqueYDimensions.Length];
                        foreach (scsBinCodeUpdateDetails uniqueY in uniqueYDimensions)
                        {
                            int binCodeIndex = 0;
                            if (_txtHiddenMapType.Data.ToString().Equals("2DArray", StringComparison.OrdinalIgnoreCase))
                            {
                                binCodeIndex = maxDimensionY - uniqueY.YDimension.Value;
                            }
                            else if (_txtHiddenMapType.Data.ToString().Equals("Row/Column", StringComparison.OrdinalIgnoreCase))
                            {
                                binCodeIndex = oExistingBinCodeDetails.First(item => item.BinCodeY == uniqueY.YDimension.Value).BinCodeSequence.Value - 1;
                            }
                            objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinCodes[newBinCodeIndex] = new scsBinCodeChanges();
                            objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinCodes[newBinCodeIndex].ListItemAction = ListItemAction.Change;
                            objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinCodes[newBinCodeIndex].ListItemIndex = binCodeIndex;

                            string originalValues = oExistingBinCodeDetails[binCodeIndex].BinCodeValues.ToString();
                            int startingBinCodeX = 0;
                            if (_txtHiddenMapType.Data.ToString().Equals("Row/Column", StringComparison.OrdinalIgnoreCase))
                                startingBinCodeX = oExistingBinCodeDetails[binCodeIndex].BinCodeX.Value;
                            foreach (scsBinCodeUpdateDetails binCode in binCodeList)
                            {
                                if (binCode.YDimension.Value == uniqueY.YDimension.Value)
                                {
                                    var aStringBuilder = new StringBuilder(originalValues);
                                    aStringBuilder.Remove((binCode.XDimension.Value - startingBinCodeX) * binTypeLength, binTypeLength);
                                    aStringBuilder.Insert((binCode.XDimension.Value - startingBinCodeX) * binTypeLength, binCode.NewBinCode.Value);
                                    originalValues = aStringBuilder.ToString();
                                }
                            }
                            objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinCodes[newBinCodeIndex].BinCodeValues = originalValues;
                            newBinCodeIndex++;
                        }
                    }
                    else if (_txtHiddenMapType.Data.ToString().Equals("Array", StringComparison.OrdinalIgnoreCase))
                    {
                        objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinCodes = new scsBinCodeChanges[1];
                        objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinCodes[newBinCodeIndex] = new scsBinCodeChanges();
                        objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinCodes[newBinCodeIndex].ListItemAction = ListItemAction.Change;
                        objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinCodes[newBinCodeIndex].ListItemIndex = newBinCodeIndex;

                        string originalValues = oExistingBinCodeDetails[newBinCodeIndex].BinCodeValues.ToString();
                        foreach (scsBinCodeUpdateDetails binCode in binCodeList)
                        {
                            int stringIndex = ((maxDimensionY - binCode.YDimension.Value) * maxX) + binCode.XDimension.Value;
                            var aStringBuilder = new StringBuilder(originalValues);
                            aStringBuilder.Remove(stringIndex * binTypeLength, binTypeLength);
                            aStringBuilder.Insert(stringIndex * binTypeLength, binCode.NewBinCode.Value);
                            originalValues = aStringBuilder.ToString();
                        }
                        objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinCodes[newBinCodeIndex].BinCodeValues = originalValues;
                    }
                    else if (_txtHiddenMapType.Data.ToString().Equals("Coordinate", StringComparison.OrdinalIgnoreCase))
                    {
                        objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinCodes = new scsBinCodeChanges[binCodeList.Length];
                        foreach (scsBinCodeUpdateDetails binCode in binCodeList)
                        {
                            int binCodeIndex = oExistingBinCodeDetails.First(item => item.BinCodeY == binCode.YDimension.Value && item.BinCodeX == binCode.XDimension.Value).BinCodeSequence.Value - 1;
                            objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinCodes[newBinCodeIndex] = new scsBinCodeChanges();
                            objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinCodes[newBinCodeIndex].ListItemAction = ListItemAction.Change;
                            objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinCodes[newBinCodeIndex].ListItemIndex = binCodeIndex;
                            objChanges.MapDataSubstrateMaps[0].Overlays[0].BinCodeMaps[0].BinCodes[newBinCodeIndex].BinCodeValues = binCode.NewBinCode.Value;
                            newBinCodeIndex++;
                        }
                    }

                    //Assign the new good devices count
                    objChanges.MapDataSubstrates = new scsMapDataSubstrateChanges[1];
                    objChanges.MapDataSubstrates[0] = new scsMapDataSubstrateChanges();
                    objChanges.MapDataSubstrates[0].ListItemAction = ListItemAction.Change;
                    objChanges.MapDataSubstrates[0].ListItemIndex = substrateIndex;
                    objChanges.MapDataSubstrates[0].GoodDevices = goodDevices;

                    objService.BeginTransaction();

                    objServiceData.ObjectToChange = new NamedObjectRef(mapDataName);

                    objService.Load(objServiceData);
                    objServiceData.ObjectChanges = objChanges;
                    objService.ExecuteTransaction(objServiceData);

                    resultStatus = objService.CommitTransaction();

                    ReturnResultStatus = resultStatus;
                }
                else
                {
                    ReturnResultStatus = validateBinCodeDetails;
                }
            }
            catch (Exception ex)
            {
                ReturnResultStatus = new ResultStatus((ex.Message.ToString()), false);
            }
            return ReturnResultStatus;
        }

        //---------------------------------------------------
        //Web Part Custom Action
        //---------------------------------------------------
        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null && action.Parameters == "Submit")
            {
                scsBinCodeUpdateDetails[] binCodeUpdateList = (_gridBinCodeUpdateDetails.GridContext as BoundContext).Data as scsBinCodeUpdateDetails[];
                string sMapDataName = _txtHiddenMapDataName.Data != null ? _txtHiddenMapDataName.Data.ToString() : null;
                string sMapType = _txtHiddenMapType.Data != null ? _txtHiddenMapType.Data.ToString() : null;
                string sBinType = _txtHiddenBinType.Data != null ? _txtHiddenBinType.Data.ToString() : null;
                e.Result = ExecuteSubmit(sMapDataName, sMapType, sBinType, binCodeUpdateList);
                if (e.Result.IsSuccess)
                {
                    Page.ShopfloorReset(sender, e);
                    ScriptManager.RegisterStartupScript(this.Page.Form, this.Page.GetType(), "ClearMapData", "ClearMapDataCanvas();", true);
                    _gridBinCodeUpdateDetails.ClearData();
                    //_ddlLotId.ClearData();
                    //_ddlLotId.ClearSelectionValues();
                    //_txtSubstrateId.ClearData();
                    //_txtSubstrateId.Focus();
                    //_btnFullView.Enabled = false;
                    LotIdField_DataChanged(sender, e);
                }
                else
                {
                    //fixed for wafer map disappeared when hitting error after submit (Bug 74300)
                    try
                    {
                        if (_txtSubstrateId.Data != null && _ddlLotId.Data != null)
                        {
                            FetchData(_txtSubstrateId.Data.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        Page.StatusBar.WriteError(ex.Message.ToString());
                    }
                }
            }
            else if (action != null && action.Parameters == "AddToGrid")
            {
                if (_txtNewBinCode.Data != null)
                {
                    AddToBinCodeUpdateDetails();
                    _txtXDimension.ClearData();
                    _txtYDimension.ClearData();
                    _txtCurrentBinCode.ClearData();
                    _txtNewBinCode.ClearData();
                }
            }
            else if (action != null && action.Parameters == "Reset")
            {
                ScriptManager.RegisterStartupScript(this.Page.Form, this.Page.GetType(), "ClearMapData", "ClearMapDataCanvas();", true);
                Page.ShopfloorReset(sender, e);
                _gridBinCodeUpdateDetails.ClearData();
                _ddlLotId.ClearData();
                _ddlLotId.ClearSelectionValues();
                _txtSubstrateId.ClearData();
                //_txtSubstrateId.Focus();
                _btnFullView.Enabled = false;
            }
            else if (action != null && action.Parameters == "Back")
            {
                string jsonDisplaySettings = "{\"FullScreen\":false, \"HideFooter\":false, \"HideHeader\":false, \"HideTopMenu\":false, \"HideTabs\":false}";

                //reset display back to normal settings before closing tab
                ScriptManager.RegisterStartupScript(Page.Form, Page.Form.GetType(), "EnsureDisplaySettings",
                   string.Format("$(function(){{ if(__page) {{__page.ensureDisplaySettings('{0}'); __page.closeTab('');}} }});", jsonDisplaySettings), true);
            }
        } // WebPartCustomAction 

        //---------------------------------------------------
        // Override OnLoad event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                _txtSubstrateId.DataChanged += new EventHandler(SubstrateIdField_DataChanged);
                _ddlLotId.DataChanged += new EventHandler(LotIdField_DataChanged);
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }
    }
}



