/* Copyright 2020 Siemens */
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
    /// Summary description for SS_LotModifyAttrs
    /// </summary>
    public class SS_LotModifyAttrs : scsShopfloorBase
    {
        #region Properties
        CWC.TextBox _txtSelectionIdField { get { return Page.FindCamstarControl("LotModifyAttrs_SelectionId") as CWC.TextBox; } }
        CWC.TextBox _txtComputerNameField { get { return Page.FindCamstarControl("LotModifyAttrs_ComputerName") as CWC.TextBox; } }
        CWC.ContainerList _clHiddenSelectedContainer { get { return Page.FindCamstarControl("HiddenSelectedContainer") as CWC.ContainerList; } }
        JQDataGrid _gridLotInfoField { get { return Page.FindCamstarControl("LotInfoFieldGrid") as JQDataGrid; } }
        JQDataGrid _gridAttributes { get { return Page.FindCamstarControl("LotModifyAttrs_ServiceAttrsDetails") as JQDataGrid; } }
        JQDataGrid _gridValidValues { get { return Page.FindCamstarControl("ValidValuesGrid") as JQDataGrid; } }
        SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }
        CWC.Button _btnLotInfo { get { return Page.FindCamstarControl("LotInfoPop") as CWC.Button; } }
        protected CWC.Button _LotInfoCmmdBar { get { return Page.FindCamstarControl("LotInfo") as CWC.Button; } }
        #endregion

        public SS_LotModifyAttrs()
        {
            //
            // TODO: Add constructor logic here
            //
        }

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

            _txtSelectionIdField.DataChanged += new EventHandler(SelectionIdField_DataChanged);

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


        public void SelectionIdField_DataChanged(object sender, EventArgs e)
        {
            if (!_txtSelectionIdField.IsEmpty)
            {
                FetchData();
            }
            else
            {
                ResetFields();
            }
        }

        private void FetchData()
        {
            string sSelectionId = "";
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            LotModifyAttrs oServiceData = new LotModifyAttrs();
            LotModifyAttrs_Info oServiceInfo = new LotModifyAttrs_Info();
            LotModifyAttrsService oService = new LotModifyAttrsService(fs.CurrentUserProfile);
            LotModifyAttrs_Request oRequest = new LotModifyAttrs_Request();
            LotModifyAttrs_Result oResponseData = new LotModifyAttrs_Result();

            // Prepare the request
            sSelectionId = _txtSelectionIdField.Data.ToString();
            oServiceData.SelectionId = sSelectionId;
            oServiceInfo.SelectionContainer = FieldInfoUtil.RequestValue();


            // Request the data
            oRequest.Info = oServiceInfo;
            OM.ResultStatus oResultStatus = oService.ResolveSelectionId(oServiceData, oRequest, out oResponseData);
            if (oResultStatus.IsSuccess)
            {
                // Clear all fields for fresh transaction
                ResetFields();

                // Check the SelectionContainer exist
                if (oResponseData.Value.SelectionContainer == null)
                {
                    DisplayMessage(new ResultStatus("Lot information could not be obtained", false));

                }
                else
                {
                    // Set the value of the resolve Container Name
                    _clHiddenSelectedContainer.Data = oResponseData.Value.SelectionContainer.Name.ToString();

                    // Fetch Lot Information and Display on datagrid
                    SetLotSelection(_clHiddenSelectedContainer.Data.ToString());
                    _btnLotInfo.Enabled = true;
                    _txtSelectionIdField.Data = sSelectionId;
                    //Request and Fetch ServiceAttributeDetails Information then Display on data grid
                    SetServiceAttributeDetails(_clHiddenSelectedContainer.Data.ToString());
                }


            }
            else
            {
                DisplayMessage(oResultStatus);
                ResetFields();
            }
        }
        protected void SetServiceAttributeDetails(string LotId)
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                // init service objects
                LotModifyAttrs oServiceData = new LotModifyAttrs();
                LotModifyAttrs_Info oServiceInfo = new LotModifyAttrs_Info();
                LotModifyAttrsService oService = new LotModifyAttrsService(fs.CurrentUserProfile);
                LotModifyAttrs_Result oServiceResult = new LotModifyAttrs_Result();

                oServiceData.Container = new ContainerRef();
                oServiceData.Container.Name = LotId;
                oServiceInfo.Containers = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection = new ServiceAttrsDetails_Info();
                oServiceInfo.ServiceAttrsDetailsSelection.Attribute = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.AttributeName = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.AlternateName1 = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.AlternateName2 = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.AttributeValue = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.AttributeRevision = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.AccessLevel = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.FieldType = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.IsRequired = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.ServiceAttrsSetupName = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.ObjectTypeName = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.ValidValues = new AttributeValidValuesChanges_Info();
                oServiceInfo.ServiceAttrsDetailsSelection.ValidValues.AttributeValue = FieldInfoUtil.RequestValue();
                oServiceInfo.ServiceAttrsDetailsSelection.ValidValues.AttributeRevision = FieldInfoUtil.RequestValue();

                // init request
                LotModifyAttrs_Request oServiceRequest = new LotModifyAttrs_Request();
                oServiceRequest.Info = oServiceInfo;

                // execute!
                ResultStatus oResultStatus = oService.GetEnvironment(oServiceData, oServiceRequest, out oServiceResult);
                if (oResultStatus.IsSuccess)
                {
                    if (oServiceResult.Value.ServiceAttrsDetailsSelection != null)
                    {
                        Array oAttrArray = oServiceResult.Value.ServiceAttrsDetailsSelection.ToArray();
                        var oAttrArray2 = DeepCopy(oAttrArray);
                        (_gridAttributes.GridContext as BoundContext).Data = oAttrArray;
                        _gridAttributes.BoundContext.LoadData();
                        _gridAttributes.OriginalData = oAttrArray2;
                        CamstarWebControl.SetRenderToClient(_gridAttributes);

                        //Create Valid Values table
                        DataTable validValuesDT = new DataTable();
                        int countSvcAttr = (oServiceResult.Value as LotModifyAttrs).ServiceAttrsDetailsSelection.Count();
                        validValuesDT.Columns.Add("Attribute", typeof(String));
                        validValuesDT.Columns.Add("AttributeValue", typeof(String));
                        validValuesDT.Columns.Add("AttributeRevision", typeof(String));
                        for (int i = 0; i < countSvcAttr; i++)
                        {
                            if (((oServiceResult.Value as LotModifyAttrs).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues != null)
                            {
                                int countValidValues = ((oServiceResult.Value as LotModifyAttrs).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.Count();
                                for (int x = 0; x < countValidValues; x++)
                                {
                                    DataRow dtRow = validValuesDT.NewRow();
                                    dtRow.SetField("Attribute", ((oServiceResult.Value as LotModifyAttrs).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).Attribute.Name);
                                    dtRow.SetField("AttributeValue", (((oServiceResult.Value as LotModifyAttrs).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.GetValue(x) as AttributeValidValuesChanges).AttributeValue);
                                    dtRow.SetField("AttributeRevision", (((oServiceResult.Value as LotModifyAttrs).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.GetValue(x) as AttributeValidValuesChanges).AttributeRevision);
                                    validValuesDT.Rows.Add(dtRow);
                                }
                            }
                        }
                        _gridValidValues.ClearData();
                        _gridValidValues.Data = validValuesDT;
                        _gridValidValues.OriginalData = validValuesDT;
                    }
                    else
                        _gridValidValues.ClearData();
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
        public void SetLotSelection(string sSelectionId)
        {
            JQDataGrid _gridLotInfoFieldx = Page.FindCamstarControl("LotInfoFieldGrid") as JQDataGrid;
            _gridLotInfoFieldx.ClearData();
            SEMI.AppCode.UIUtility.GetLotQuerySelection(this, this.PrimaryServiceType.ToString(), sSelectionId, true, ref _gridLotInfoFieldx, "LotInfoFieldGrid");
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

        public override bool PreExecute(Info serviceInfo, Service serviceData)
        {
            if ((serviceData as LotModifyAttrs).ServiceAttrsDetails != null)
                foreach (ServiceAttrsDetails CurrentServiceAttrsDetails in (serviceData as LotModifyAttrs).ServiceAttrsDetails)
                {
                    if (CurrentServiceAttrsDetails.FieldType != null && CurrentServiceAttrsDetails.FieldType.Value.Equals("NUMBER"))
                    {
                        decimal decTryParse = new decimal();
                        if (CurrentServiceAttrsDetails.AttributeValue != null && !decimal.TryParse(CurrentServiceAttrsDetails.AttributeValue.Value, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.CurrentInfo, out decTryParse))
                        {
                            ResultStatus InvalidResultStatus = new ResultStatus("The value of " + CurrentServiceAttrsDetails.AttributeName + " is not a valid value for 'NUMBER' Field Type", false);
                            DisplayMessage(InvalidResultStatus);
                            return false;
                        }
                    }
                }
            return base.PreExecute(serviceInfo, serviceData);
        }

        public override void GetInputData(Service serviceData)
        {
            try
            {
                _gridAttributes.FieldExpressions = ".ServiceAttrsDetails";
                base.GetInputData(serviceData);
                _gridAttributes.FieldExpressions = null;


                if ((serviceData as LotModifyAttrs).ServiceAttrsDetails != null)
                {
                    ServiceAttrsDetails[] oDetails = _gridAttributes.BoundContext.Data as ServiceAttrsDetails[];
                    //nullify the index, listItemAction
                    foreach (ServiceAttrsDetails CurrentServiceAttrsDetails in (serviceData as LotModifyAttrs).ServiceAttrsDetails)
                    {
                        ServiceAttrsDetails oDetail = oDetails.First(w => w.ListItemIndex == CurrentServiceAttrsDetails.ListItemIndex);
                        if (oDetail != null)
                            CurrentServiceAttrsDetails.Attribute = oDetail.Attribute;

                        CurrentServiceAttrsDetails.ListItemAction = null;
                        CurrentServiceAttrsDetails.ListItemIndex = null;
                    }
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

        }

        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                Page.ClearValues();
                DisplayMessage(status);
                ResetFields();
            }
        }
    }
}