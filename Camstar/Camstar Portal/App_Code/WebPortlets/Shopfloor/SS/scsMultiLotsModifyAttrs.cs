/* Copyright 2024 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Reflection;
using System.Reflection.Emit;
using System.ComponentModel;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using PERS = Camstar.WebPortal.Personalization;
using CamstarPortal.WebControls;
using Camstar.WCF.Services;
using System.Collections;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Constants;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Web.UI;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    /// Summary description for SS_scsDBMultiLotsModifyAttrs
    /// </summary>
    public class scsMultiLotsModifyAttrs : scsShopfloorBase
    {
        #region Properties
        CWC.TextBox _txtSelectionIdField { get { return Page.FindCamstarControl("MultiLotsModifyAttrs_SelectionId") as CWC.TextBox; } }
        CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("MultiLotsModifyAttrs_ComputerName") as CWC.TextBox; } }
        CWC.ContainerList _clHiddenSelectedContainer { get { return Page.FindCamstarControl("HiddenSelectedContainer") as CWC.ContainerList; } }
        JQDataGrid _gridLotInfoField { get { return Page.FindCamstarControl("LotInfoFieldGrid") as JQDataGrid; } }
        JQDataGrid _gridAttributes { get { return Page.FindCamstarControl("MultiLotsModifyAttrs_ServiceAttrsDetails") as JQDataGrid; } }
        JQDataGrid _gridValidValues { get { return Page.FindCamstarControl("ValidValuesGrid") as JQDataGrid; } }
        SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }
        CWC.Button _btnLotInfo { get { return Page.FindCamstarControl("LotInfoPop") as CWC.Button; } }
        protected CWC.Button _LotInfoCmmdBar { get { return Page.FindCamstarControl("LotInfo") as CWC.Button; } }
        CWC.Button _btnLotDelete { get { return Page.FindCamstarControl("MultiLotsModifyAttrs_Delete") as CWC.Button; } }
        CWC.Button _btnConfirmLotDelete { get { return Page.FindCamstarControl("MultiLotsModifyAttrs_ConfirmDelete") as CWC.Button; } }
        #endregion
        /*
        public SS_scsDBMultiLotsModifyAttrs()
        {
            //
            // TODO: Add constructor logic here
            //
        }*/

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //hide or show the lot detail button based on current theme

            var currentTheme = Page.Session["CurrentTheme"].ToString();

            if (currentTheme.ToLower() == "horizon")
            {
                _btnLotInfo.Visible = false;
                _LotInfoCmmdBar.Visible = true;
            }
            else if (currentTheme.ToLower() == "camstar")
            {
                _btnLotInfo.Visible = true;
                _LotInfoCmmdBar.Visible = false;
            }

            //hide button to be clicked by javascript
            _btnLotDelete.Hidden = true;
            _btnConfirmLotDelete.Hidden = true;

            _txtSelectionIdField.DataChanged += new EventHandler(SelectionIdField_DataChanged);
            _gridLotInfoField.RowSelected += new JQGridEventHandler(LotInfoField_RowSelected);
            _gridLotInfoField.RowDeleted += new JQGridEventHandler(LotInfoField_RowDeleted);
            _btnLotDelete.Click += new EventHandler(LotInfoField_LotDeleteConfirmation);
            _btnConfirmLotDelete.Click += new EventHandler(LotInfoField_LotDeleteConfirmed);
            _txtComputerNameField.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);

            if (Page.IsPostBack)
            {
                // Check if it is a pop up close, get the return result
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                {

                    if (_envSelectedLots != null)
                        // manually initialize the containers list data contract since it is not triggered when we do a manual popup close
                        if (Page.DataContract.GetValueByName("SelectedLotsListDM") != null)
                            _envSelectedLots.SS_ContainersList = Page.DataContract.GetValueByName("SelectedLotsListDM") as string[];

                    if (_envSelectedLots.SS_ContainersList != null)
                    {
                        string[] sContainers;
                        sContainers = _envSelectedLots.SS_ContainersList;
                        // Set Selection Id textbox value
                        _txtSelectionIdField.Data = sContainers[0];
                        _btnLotInfo.Enabled = true;

                        JQDataGrid _gridContainers = Page.FindCamstarControl("LotInfoFieldGrid") as JQDataGrid;
                        var gridTotalRows = _gridContainers.BoundContext.GetTotalRows();

                        for (int i = 0; i < sContainers.Length; i++)
                        {
                            // check if lot not exist in the grid else add to the grid
                            string sExistingName = (_gridContainers.GridContext as BoundContext).GetCell((gridTotalRows - 1).ToString().PadLeft(6, '0'), "Lot").ToString();

                            string strSelectedGridId = (_gridContainers.GridContext as BoundContext).GetRowIdByCellValue("Lot", sContainers[i]);

                            if (string.IsNullOrEmpty(strSelectedGridId) && sExistingName != sContainers[i])
                            {
                                //setting them to the grid
                                LotGrid_Population(sContainers[i], _gridContainers);
                                SetServiceAttributeDetails(sContainers[i]);
                            }
                        }

                        //nullify the containers list
                        _envSelectedLots.SS_ContainersList = null;
                    }

                }
                if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument" && !string.IsNullOrEmpty(Page.PortalContext.DataContract.GetValueByName<string>("LotModifyAttrs_ReturnedValueDM")))
                {
                    var attrVal = Page.PortalContext.DataContract.GetValueByName<string>("LotModifyAttrs_ReturnedValueDM");
                    var attrRev = Page.PortalContext.DataContract.GetValueByName<string>("LotModifyAttrs_ReturnedRevisonDM");
                    var rowid = Page.PortalContext.DataContract.GetValueByName<string>("LotModifyAttrs_SelectedRowIdDM");
                    if (!string.IsNullOrEmpty(rowid))
                    {
                        int iRowId = int.Parse(rowid);
                        var data = _gridAttributes.Data as ServiceAttrsDetails[];
                        data[iRowId].AttributeValue = attrVal;
                        data[iRowId].AttributeRevision = attrRev;

                    }

                    // clear the data contracts as the mess with the data loading of grid values
                    Page.PortalContext.DataContract.SetValueByName("LotModifyAttrs_ReturnedValueDM", null);
                    Page.PortalContext.DataContract.SetValueByName("LotModifyAttrs_ReturnedRevisonDM", null);
                    Page.PortalContext.DataContract.SetValueByName("LotModifyAttrs_SelectedRowIdDM", null);
                }
            }
            else
            {
                _txtComputerNameField.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
                Page.Session["AttrArray"] = null;

                //get containers from Container Search screen
                if (Page.Session["selectedContainers"] != null)
                {
                    string[] sContainersList = Page.Session["selectedContainers"] as string[];
                    foreach (string container in sContainersList)
                    {
                        _txtSelectionIdField.TextControl.Text = container;
                        SelectionIdField_DataChanged(null, null);
                    }
                }
            }
        }

        public void LotGrid_Population(string sContainer, JQDataGrid _gridContainers)
        {
            var rsGridData = SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "scsDBMultiLotsModifyAttrs", sContainer);
            string[] sHiddenColumnNames = new string[] { "Lot", "__STYLE", "__OutputCarrier", "__SelectionId", "ProcessTimerName", "ProcessTimerRevision", "StartTimeGMT", "MinEndWarningTimeGMT", "MinWarningTimeColor", "MinEndTimeGMT", "MinTimeColor", "MaxEndWarningTimeGMT", "MaxWarningTimeColor", "MaxEndTimeGMT", "MaxTimeColor" };
            string[] sSpecificWidthColumns = new string[] { "scsApplyToChildLots|130", "Qty|70", "Qty2|70" };
            SEMI.AppCode.GridUtility.ItemListGrid_SetColumns(this, rsGridData.GetAsExplicitlyDataTable(), _gridContainers.ID, null, "scsDBMultiLotsModifyAttrs", true, null, true, sSpecificWidthColumns, rsGridData.Headers, new string[] { "scsApplyToChildLots" });
            SEMI.AppCode.GridUtility.ItemListGrid_AddDataRow(this, rsGridData.GetAsExplicitlyDataTable(), ref _gridContainers, "scsDBMultiLotsModifyAttrs");
        }

        public void ItemListGrid_AddNewRow(string sContainer)
        {
            try
            {
                JQDataGrid _gridContainers = Page.FindCamstarControl("LotInfoFieldGrid") as JQDataGrid;
                if (_gridContainers.Data != null)
                {
                    bool isUnique = true;
                    for (int i = 0; i < _gridContainers.BoundContext.GetTotalRows(); i++)
                    {
                        string sExistingName = (_gridContainers.GridContext as BoundContext).GetCell(i.ToString().PadLeft(6, '0'), "Lot").ToString();
                        if (sContainer.Equals(sExistingName))
                        {
                            isUnique = false;
                        }
                    }
                    if (isUnique)
                    {
                        LotGrid_Population(sContainer, _gridContainers);
                        SetServiceAttributeDetails(sContainer);
                    }
                }
                else
                {
                    LotGrid_Population(sContainer, _gridContainers);
                    SetServiceAttributeDetails(sContainer);
                }

            }
            catch (Exception ex)
            { }
            finally
            {
                _txtSelectionIdField.ClearData();
                _txtSelectionIdField.Focus();
            }
        }

        public void LotInfoField_LotDelete()
        {
            //Delete selected row from lot grid
            _gridLotInfoField.Action_DeleteRow(_gridLotInfoField.SelectedRowID);

            //Empty lot attributes grid
            _gridAttributes.ClearData();
            CamstarWebControl.SetRenderToClient(_gridAttributes);

            //Grab value from session
            var scsLotAttributesList = Page.Session["AttrArray"] as List<scsLotAttributes>;

            //Clear deleted container attributes from session
            var scsLotAttributes = new scsLotAttributes();
            scsLotAttributes = scsLotAttributesList.Find(item => item.Container == _clHiddenSelectedContainer.Data.ToString());
            scsLotAttributesList.Remove(scsLotAttributes);

            //Clear hidden selected container
            _clHiddenSelectedContainer.ClearData();
        }

        //Called by clicking yes on confirmation pop-up from javascript
        public void LotInfoField_LotDeleteConfirmed(object sender, EventArgs e)
        {
            LotInfoField_LotDelete();
        }

        //Called by clicking the trash-can icon on lot grid, will check whether the selected lot requires confirmation to delete
        public void LotInfoField_LotDeleteConfirmation(object sender, EventArgs e)
        {
            if (_gridLotInfoField.SelectedRowID != null)
            {
                //Compare original & current data to see if there is any changes.
                var scsLotAttributesList = Page.Session["AttrArray"] as List<scsLotAttributes>;

                var selectedContainerName = (_gridLotInfoField.GridContext as BoundContext).GetCell(_gridLotInfoField.SelectedRowID.PadLeft(6, '0'), "Lot").ToString();

                if (scsLotAttributesList.Count > 0)
                {
                    var attrDetails = new scsDBServiceAttrsDetails[scsLotAttributesList.Count];

                    for (int i = 0; i < scsLotAttributesList.Count; i++)
                    {
                        if (scsLotAttributesList[i].Container.ToString() == selectedContainerName)
                        {
                            bool confirmationNeeded = false;
                            for (int j = 0; j < scsLotAttributesList[i].ServiceDetails.Count; j++)
                            {
                                if (
                                    scsLotAttributesList[i].ServiceDetails[j].AttributeValue != scsLotAttributesList[i].OriServiceDetails[j].AttributeValue
                                    || scsLotAttributesList[i].ServiceDetails[j].AttributeRevision != scsLotAttributesList[i].OriServiceDetails[j].AttributeRevision
                                        )
                                {
                                    if (!scsLotAttributesList[i].ServiceDetails[j].AttributeValue.IsNullOrEmpty() || !scsLotAttributesList[i].OriServiceDetails[j].AttributeValue.IsNullOrEmpty())
                                    {
                                        //prompt confirmation pop-up if there is different between original and current
                                        ScriptManager.RegisterStartupScript(Page.Form, this.GetType(), "myConfirm", "scsMultiLotModifyAttribute_LotDeleteConfirmation();", true);
                                        confirmationNeeded = true;
                                    }
                                }

                            }
                            // if confirmation is not needed, proceed deleting the selected lot
                            if (!confirmationNeeded)
                                LotInfoField_LotDelete();
                        }

                    }

                }
            }

        }

        public void SelectionIdField_DataChanged(object sender, EventArgs e)
        {
            if (!_txtSelectionIdField.IsEmpty)
            {
                Validate_Container();
                string sContainer = _txtSelectionIdField.Data.ToString();
                ItemListGrid_AddNewRow(sContainer);
            }
        }

        protected void SetServiceAttributeDetails(string LotId)
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                // init service objects
                scsDBMultiLotsModifyAttrs oServiceData = new scsDBMultiLotsModifyAttrs();
                scsDBMultiLotsModifyAttrs_Info oServiceInfo = new scsDBMultiLotsModifyAttrs_Info();
                scsDBMultiLotsModifyAttrsService oService = new scsDBMultiLotsModifyAttrsService(fs.CurrentUserProfile);
                scsDBMultiLotsModifyAttrs_Result oServiceResult = new scsDBMultiLotsModifyAttrs_Result();

                oServiceData.Container = new ContainerRef();
                oServiceData.Container.Name = LotId;
                oServiceInfo.Containers = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection = new scsDBServiceAttrsDetails_Info();
                oServiceInfo.ServiceAttrsDetailsSelection.Attribute = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.AttributeName = FieldInfoUtil.RequestValue();
                //oServiceInfo.ServiceAttrsDetailsSelection.AlternateName1 = FieldInfoUtil.RequestValue();
                //oServiceInfo.ServiceAttrsDetailsSelection.AlternateName2 = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.AttributeValue = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.AttributeRevision = FieldInfoUtil.RequestValue();
                //oServiceInfo.ServiceAttrsDetailsSelection.AccessLevel = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.FieldType = FieldInfoUtil.RequestValue();
                //oServiceInfo.ServiceAttrsDetailsSelection.IsRequired = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.ServiceAttrsSetupName = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.ObjectTypeName = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.ValidValues = new AttributeValidValuesChanges_Info();
                oServiceInfo.ServiceAttrsDetailsSelection.ValidValues.AttributeValue = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.ValidValues.AttributeRevision = FieldInfoUtil.RequestValue();

                // init request
                scsDBMultiLotsModifyAttrs_Request oServiceRequest = new scsDBMultiLotsModifyAttrs_Request();
                oServiceRequest.Info = oServiceInfo;

                // execute!
                ResultStatus oResultStatus = oService.GetEnvironment(oServiceData, oServiceRequest, out oServiceResult);
                if (oResultStatus.IsSuccess)
                {
                    if (oServiceResult.Value.ServiceAttrsDetailsSelection != null)
                    {
                        Array oAttrArray = oServiceResult.Value.ServiceAttrsDetailsSelection.ToArray();
                        var oAttrArray2 = DeepCopy(oAttrArray);

                        var scsLotAttributesList = Page.Session["AttrArray"] as List<scsLotAttributes> ?? new List<scsLotAttributes>();
                        var scsLotAttributes = new scsLotAttributes();
                        scsLotAttributes.Container = LotId;

                        var scsLotAttributesItem = scsLotAttributesList.Find(item => item.Container == LotId);

                        if (scsLotAttributesItem == null)
                        {
                            scsLotAttributes.ServiceDetails.AddRange(oServiceResult.Value.ServiceAttrsDetailsSelection);
                            scsLotAttributes.OriServiceDetails.AddRange((IEnumerable<scsDBServiceAttrsDetails>)oAttrArray2);
                            scsLotAttributesList.Add(scsLotAttributes);
                        }

                        Page.Session["AttrArray"] = scsLotAttributesList;

                        //if (scsLotAttributesList.Count == 1)
                        //{
                        (_gridAttributes.GridContext as BoundContext).Data = oAttrArray;
                        _gridAttributes.BoundContext.LoadData();
                        _gridAttributes.OriginalData = oAttrArray2;
                        _gridAttributes.BoundContext.Width = 200;
                        _gridAttributes.Width = 200;
                        CamstarWebControl.SetRenderToClient(_gridAttributes);

                        //Create Valid Values table
                        DataTable validValuesDT = new DataTable();
                        int countSvcAttr = (oServiceResult.Value as scsDBMultiLotsModifyAttrs).ServiceAttrsDetailsSelection.Count();
                        validValuesDT.Columns.Add("Attribute", typeof(String));
                        validValuesDT.Columns.Add("AttributeValue", typeof(String));
                        validValuesDT.Columns.Add("AttributeRevision", typeof(String));
                        for (int i = 0; i < countSvcAttr; i++)
                        {
                            if (((oServiceResult.Value as scsDBMultiLotsModifyAttrs).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues != null)
                            {
                                int countValidValues = ((oServiceResult.Value as scsDBMultiLotsModifyAttrs).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.Count();
                                for (int x = 0; x < countValidValues; x++)
                                {
                                    DataRow dtRow = validValuesDT.NewRow();
                                    dtRow.SetField("Attribute", ((oServiceResult.Value as scsDBMultiLotsModifyAttrs).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).Attribute.Name);
                                    dtRow.SetField("AttributeValue", (((oServiceResult.Value as scsDBMultiLotsModifyAttrs).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.GetValue(x) as AttributeValidValuesChanges).AttributeValue);
                                    dtRow.SetField("AttributeRevision", (((oServiceResult.Value as scsDBMultiLotsModifyAttrs).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.GetValue(x) as AttributeValidValuesChanges).AttributeRevision);
                                    validValuesDT.Rows.Add(dtRow);
                                }
                            }
                        }
                        _gridValidValues.ClearData();
                        _gridValidValues.Data = validValuesDT;
                        _gridValidValues.OriginalData = validValuesDT;
                        //}
                    }
                    else
                        _gridValidValues.ClearData();

                    //LOT SELECTION ON THE NEWLY SCANNED ROW/ LAST ROW
                    var lastIndex = _gridLotInfoField.BoundContext.GetTotalRows() - 1;
                    _gridLotInfoField.GridContext.SelectRow(lastIndex.ToString().PadLeft(6, '0'), true);

                    var selectedId = _gridLotInfoField.GridContext.SelectedRowID;

                    if (_gridLotInfoField.GridContext.GetCell(selectedId, "Lot") != null)
                    {
                        var selectedContainer = _gridLotInfoField.GridContext.GetCell(selectedId, "Lot").ToString();
                        _clHiddenSelectedContainer.Data = selectedContainer;
                    }
                }
                else
                {
                    DisplayMessage(oResultStatus);
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message, false));
            }
        }

        public T DeepCopy<T>(T item)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, item);
            stream.Seek(0, SeekOrigin.Begin);
            T result = (T)formatter.Deserialize(stream);
            stream.Close();
            return result;
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;
            if (action != null && action.Parameters == "Reset")
            {
                Page.ClearValues();
                Page.ShopfloorReset(sender, e);
                ResetFields();
            }
        }

        public override void GetInputData(Service serviceData)
        {
            try
            {
                base.GetInputData(serviceData);

                //grab value from session
                var scsLotAttributesList = Page.Session["AttrArray"] as List<scsLotAttributes>;

                if (scsLotAttributesList.Count > 0)
                {
                    var attrDetails = new scsDBServiceAttrsDetails[scsLotAttributesList.Count];

                    for (int i = 0; i < scsLotAttributesList.Count; i++)
                    {
                        attrDetails[i] = new scsDBServiceAttrsDetails();
                        attrDetails[i].scsContainerName = scsLotAttributesList[i].Container;

                        var scsApplyToChildLots = (_gridLotInfoField.GridContext as BoundContext).GetCell(i.ToString().PadLeft(6, '0'), "scsApplyToChildLots").ToString().ToUpper();
                        attrDetails[i].scsApplyToChildLots = scsApplyToChildLots == "TRUE" ? true : false;

                        var details = new List<scsDBServiceAttrsDetails>();


                        for (int j = 0; j < scsLotAttributesList[i].ServiceDetails.Count; j++)
                        {
                            if (
                                scsLotAttributesList[i].ServiceDetails[j].AttributeValue != scsLotAttributesList[i].OriServiceDetails[j].AttributeValue
                                || scsLotAttributesList[i].ServiceDetails[j].AttributeRevision != scsLotAttributesList[i].OriServiceDetails[j].AttributeRevision
                                    )
                            {
                                if (!scsLotAttributesList[i].ServiceDetails[j].AttributeValue.IsNullOrEmpty() || !scsLotAttributesList[i].OriServiceDetails[j].AttributeValue.IsNullOrEmpty())
                                {
                                    var scsAttrDetails = scsLotAttributesList[i].ServiceDetails;
                                    scsAttrDetails[j].ListItemIndex = null;
                                    scsAttrDetails[j].Self = null;
                                    scsAttrDetails[j].ValidValues = null;
                                    details.Add(scsLotAttributesList[i].ServiceDetails[j]);
                                }
                            }
                            attrDetails[i].scsAttrDetails = details.ToArray();
                        }

                        //SET TO NULL TO PREVENT SUBMISSION IF LOT HAS NO CHANGES
                        if (attrDetails[i].scsAttrDetails.Length < 1)
                            attrDetails[i] = null;
                    }

                    if (attrDetails.Length > 0)
                        (serviceData as scsDBMultiLotsModifyAttrs).ServiceAttrsDetails = attrDetails;
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message, false));
            }
        }

        private void ResetFields()
        {
            _btnLotInfo.Enabled = false;
            _gridLotInfoField.ClearData();
            _gridAttributes.ClearData();
            _clHiddenSelectedContainer.ClearData();
            Page.Session["AttrArray"] = null;

        }

        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                Page.ClearValues();
                Page.Session["AttrArray"] = null;
                DisplayMessage(status);
                ResetFields();
            }
        }

        //Lots Grid Selected Row Changed
        protected virtual ResponseData LotInfoField_RowSelected(object sender, JQGridEventArgs args)
        {
            var scsLotAttributes = new scsLotAttributes();

            //grab value from session
            var scsLotAttributesList = Page.Session["AttrArray"] as List<scsLotAttributes>;

            if (_clHiddenSelectedContainer.Data != null)
            {
                //Save Current Container attributes to session
                var sSelectionId = _clHiddenSelectedContainer.Data.ToString();
                scsLotAttributes = scsLotAttributesList.Find(item => item.Container == sSelectionId);
                var data = _gridAttributes.Data as List<scsDBServiceAttrsDetails>;
                scsLotAttributes = scsLotAttributesList.FirstOrDefault(item => item.ServiceDetails == data);
                Page.Session["AttrArray"] = scsLotAttributesList;
            }

            //Display current selected container's attributes
            var selectedId = _gridLotInfoField.GridContext.SelectedRowID;
            var selectedContainer = "";

            if (selectedId != null)
            {
                selectedContainer = _gridLotInfoField.GridContext.GetCell(selectedId, "Lot").ToString();
                scsLotAttributes = scsLotAttributesList.Find(item => item.Container == selectedContainer);

                var oAttrArray = scsLotAttributes.ServiceDetails.ToArray();
                (_gridAttributes.GridContext as BoundContext).Data = oAttrArray;
            }
            else
            {
                (_gridAttributes.GridContext as BoundContext).Data = null;
            }

            //render the grid
            _gridAttributes.BoundContext.LoadData();
            CamstarWebControl.SetRenderToClient(_gridAttributes);

            //Set the current selected container name back to the hidden field
            _clHiddenSelectedContainer.Data = selectedContainer;

            return new StatusData(true, "Selected row updated.");
        }

        //Lots Grid Selected Row Deleted
        protected virtual ResponseData LotInfoField_RowDeleted(object sender, JQGridEventArgs args)
        {
            //Empty lot attributes grid
            _gridAttributes.ClearData();
            CamstarWebControl.SetRenderToClient(_gridAttributes);

            //Grab value from session
            var scsLotAttributesList = Page.Session["AttrArray"] as List<scsLotAttributes>;

            //Clear deleted container attributes from session
            var scsLotAttributes = new scsLotAttributes();
            scsLotAttributes = scsLotAttributesList.Find(item => item.Container == _clHiddenSelectedContainer.Data.ToString());
            scsLotAttributesList.Remove(scsLotAttributes);

            //Clear hidden selected container
            _clHiddenSelectedContainer.ClearData();

            return new StatusData(true, "Selected row deleted.");
        }

        private void Validate_Container()
        {
            string sSelectionId = "";
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            scsDBMultiLotsModifyAttrs oServiceData = new scsDBMultiLotsModifyAttrs();
            scsDBMultiLotsModifyAttrs_Info oServiceInfo = new scsDBMultiLotsModifyAttrs_Info();
            scsDBMultiLotsModifyAttrsService oService = new scsDBMultiLotsModifyAttrsService(fs.CurrentUserProfile);
            scsDBMultiLotsModifyAttrs_Request oRequest = new scsDBMultiLotsModifyAttrs_Request();
            scsDBMultiLotsModifyAttrs_Result oResponseData = new scsDBMultiLotsModifyAttrs_Result();

            // Prepare the request
            sSelectionId = _txtSelectionIdField.Data.ToString();
            oServiceData.SelectionId = sSelectionId;
            oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();

            // Request the data
            oRequest.Info = oServiceInfo;
            OM.ResultStatus oResultStatus = oService.ResolveSelectionId(oServiceData, oRequest, out oResponseData);
            if (oResultStatus.IsSuccess)
            {
                // Check the SelectionContainer exist
                if (oResponseData.Value.SelectionContainer == null)
                {
                    DisplayMessage(new ResultStatus("Lot information could not be obtained", false));
                }
            }
            else
            {
                DisplayMessage(oResultStatus);
            }
        }

        private class scsLotAttributes
        {
            private List<scsDBServiceAttrsDetails> lServiceDetails;
            private List<scsDBServiceAttrsDetails> lOriServiceDetails;
            private string sContainer;

            // constructor
            public scsLotAttributes()
            {
                lServiceDetails = new List<scsDBServiceAttrsDetails>();
                lOriServiceDetails = new List<scsDBServiceAttrsDetails>();
            }

            public List<scsDBServiceAttrsDetails> ServiceDetails
            {
                get { return lServiceDetails; }
                set { lServiceDetails = value; }
            }

            public List<scsDBServiceAttrsDetails> OriServiceDetails
            {
                get { return lOriServiceDetails; }
                set { lOriServiceDetails = value; }
            }

            public string Container
            {
                get { return sContainer; }
                set { sContainer = value; }
            }

        } // scsLotAttributes
    }
}