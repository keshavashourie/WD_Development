/* Copyright 2023 Siemens */
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

using SEMI.AppCode;
using System.Web.UI.WebControls;

/// <summary>
/// Summary description for SetBatchId
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_ModifyMultiLotsAttribute : scsShopfloorBase
    {
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("ModifyMultiLotsAttribute_SelectionId") as CWC.TextBox; } }
        protected CWC.TextBox _txtEmployee { get { return Page.FindCamstarControl("ModifyMultiLotsAttribute_Employee") as CWC.TextBox; } }
        protected CWC.TextBox _txtSelectedLot { get { return Page.FindCamstarControl("ModifyMultiLotsAttribute_ContainerName") as CWC.TextBox; } }
        protected JQDataGrid _gridContainers { get { return Page.FindCamstarControl("ModifyMultiLotsAttribute_Containers") as JQDataGrid; } }
        protected JQDataGrid _gridAttributes { get { return Page.FindCamstarControl("ModifyMultiLotsAttribute_ServiceAttrsDetails") as JQDataGrid; } }
        protected JQDataGrid _gridValidValues { get { return Page.FindCamstarControl("ValidValuesGrid") as JQDataGrid; } }
        protected CWC.Button _btnLotInfo { get { return Page.FindCamstarControl("LotInfoPop") as CWC.Button; } }
        protected CWC.Button _btnSelection { get { return Page.FindCamstarControl("SelectionValuesButton") as CWC.Button; } }
        protected CWC.Button _btnValid { get { return Page.FindCamstarControl("ValidValuesButton") as CWC.Button; } }
        protected CWC.TextBox txtComputerName { get { return Page.FindCamstarControl("ModifyMultiLotsAttribute_ComputerName") as CWC.TextBox; } }
        protected CWC.NamedObject _txtAttributeField { get { return Page.FindCamstarControl("ServiceAttrsDetails_Attribute") as CWC.NamedObject; } }
        protected CWC.NamedObject _txtModifyAttrsReasonField { get { return Page.FindCamstarControl("ModifyMultiLotsAttribute_ServiceAttrsModifyAttrsReason") as CWC.NamedObject; } }
        protected CWC.TextBox _txtAttributeValueField { get { return Page.FindCamstarControl("ServiceAttrsDetails_AttributeValue") as CWC.TextBox; } }
        protected CWC.TextBox _txtAttributeRevisionField { get { return Page.FindCamstarControl("ServiceAttrsDetails_AttributeRevision") as CWC.TextBox; } }
        protected CWC.TextBox _txtAttrObjectType { get { return Page.FindCamstarControl("AttrObjectType") as CWC.TextBox; } }
        protected CWC.TextBox _txtAttrRequestField { get { return Page.FindCamstarControl("ModifyMultiLotsAttribute_ServiceAttrsRequestForm") as CWC.TextBox; } }
        protected CWC.TextBox _txtAttrRequestorField { get { return Page.FindCamstarControl("ModifyMultiLotsAttribute_ServiceAttrsRequestor") as CWC.TextBox; } }
        protected CWC.CheckBox _txtApplyToChildLots { get { return Page.FindCamstarControl("ModifyMultiLotsAttribute_ApplyToChildLots") as CWC.CheckBox; } }
        private SEMI.AppCode.DataEnvelopControl _envSelectedLots { get { return Page.FindCamstarControl("SelectedLotsList") as SEMI.AppCode.DataEnvelopControl; } }

        protected Button _btnLotInfoCmdBar { get { return Page.FindCamstarControl("LotInfoPopup") as Button; } }


        public SS_ModifyMultiLotsAttribute()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        public void ItemListGrid_AddNewRow(string sContainer)
        {
            try
            {
                JQDataGrid _gridContainers = Page.FindCamstarControl("ModifyMultiLotsAttribute_Containers") as JQDataGrid;
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
                        SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "ModifyMultiLotsAttribute", sContainer, false, ref _gridContainers, "ModifyMultiLotsAttribute_Containers", true);
                        SetServiceAttributeDetails(sContainer);
                    }
                }
                else
                {
                    SEMI.AppCode.UIUtility.GetLotQuerySelection(this, "ModifyMultiLotsAttribute", sContainer, false, ref _gridContainers, "ModifyMultiLotsAttribute_Containers", true);
                    SetServiceAttributeDetails(sContainer);
                }
            }
            catch (Exception ex)
            { }
            finally
            {
                _txtSelectionId.ClearData();
                _txtSelectionId.Focus();
            }
        }
        public void SelectionId_DataChanged(Object sender, EventArgs e)
        {
            if (_txtSelectionId.Data != null)
            {
                string sContainer = _txtSelectionId.Data.ToString();
                ItemListGrid_AddNewRow(sContainer);
            }
        }
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as Camstar.WebPortal.Personalization.CustomAction;

            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "Reset":
                        {
                            Page.ClearValues();
                            Page.ShopfloorReset(sender, e);
                            _envSelectedLots.ClearData();
                            _txtAttributeField.DropDownControl.Items.Clear();
                            _gridContainers.ClearData();
                            _txtSelectionId.Focus();
                            break;
                        }
                    case "Popup":
                        {
                            try
                            {
                                int i = 0;
                                bool isMatch = false;
                                bool isObject = true;
                                int failsafe = (_gridAttributes.GridContext as BoundContext).GetTotalRows();
                                while (!isMatch && i < failsafe)
                                {
                                    string sCurrentAttribute = (_gridAttributes.GridContext as BoundContext).GetCell(i.ToString().PadLeft(6, '0'), "AttributeName").ToString();
                                    if (_txtAttributeField.DropDownControl.SelectedItem.Text.Equals(sCurrentAttribute))
                                    {
                                        isMatch = true;
                                    }
                                    else
                                    {
                                        i++;
                                    }
                                }
                                Object oObjectTypeName = (_gridAttributes.GridContext as BoundContext).GetCell(i.ToString().PadLeft(6, '0'), "ObjectTypeName");
                                Object oValidValues = (_gridAttributes.GridContext as BoundContext).GetCell(i.ToString().PadLeft(6, '0'), "ValidValues");
                                if (oObjectTypeName != null && oValidValues == null)
                                {
                                    isObject = true;
                                }
                                else
                                {
                                    isObject = false;
                                }
                                if (isObject)
                                {
                                    System.Web.UI.ScriptManager.RegisterStartupScript(Page.Form, GetType(), "SelectionValuesButton", " $('#ctl00_WebPartManager_SS_MMLA_WP_SelectionValuesButton').click();", true);
                                }
                                else
                                {
                                    if (oValidValues != null)
                                    {
                                        System.Web.UI.ScriptManager.RegisterStartupScript(Page.Form, GetType(), "ValidValuesButton", " $('#ctl00_WebPartManager_SS_MMLA_WP_ValidValuesButton').click();", true);
                                    }
                                    else
                                    {
                                        e.Result = new OM.ResultStatus("Selected attribute has no Selection Values or Valid Values.", false);
                                    }
                                }
                            }
                            catch (Exception ex)
                            { }
                            break;
                        }
                }
            }
        }
        public override void GetInputData(Service serviceData)
        {
            try
            {
                if (_txtEmployee.Data != null)
                {
                    (serviceData as ModifyMultiLotsAttribute).Employee = new NamedObjectRef();
                    (serviceData as ModifyMultiLotsAttribute).Employee.Name = _txtEmployee.Data.ToString();
                }
                if (_gridContainers.Data != null)
                {
                    int intTotalContainers = _gridContainers.BoundContext.GetTotalRows();

                    (serviceData as ModifyMultiLotsAttribute).Containers = new ContainerRef[intTotalContainers];

                    // collect the containers
                    for (int x = 0; x < intTotalContainers; x++)
                    {
                        (serviceData as ModifyMultiLotsAttribute).Containers[x] = new ContainerRef();
                        (serviceData as ModifyMultiLotsAttribute).Containers[x].Name = _gridContainers.GridContext.GetCell(x.ToString().PadLeft(6, '0'), "Lot").ToString();
                    }
                }
                if (_txtAttributeValueField.Data != null)
                {
                    if (serviceData is ModifyMultiLotsAttribute)
                    {
                        (serviceData as ModifyMultiLotsAttribute).ServiceAttrsDetails = new ServiceAttrsDetails[1];
                        (serviceData as ModifyMultiLotsAttribute).ServiceAttrsDetails[0] = new ServiceAttrsDetails();
                        (serviceData as ModifyMultiLotsAttribute).ServiceAttrsDetails[0].Attribute = new NamedObjectRef();
                        (serviceData as ModifyMultiLotsAttribute).ServiceAttrsDetails[0].Attribute.Name = _txtAttributeField.DropDownControl.SelectedValue.ToString();
                        (serviceData as ModifyMultiLotsAttribute).ServiceAttrsDetails[0].AttributeValue = _txtAttributeValueField.Data.ToString();
                        if (_txtAttributeRevisionField.Data != null)
                            (serviceData as ModifyMultiLotsAttribute).ServiceAttrsDetails[0].AttributeRevision = _txtAttributeRevisionField.Data.ToString();
                        else
                            (serviceData as ModifyMultiLotsAttribute).ServiceAttrsDetails[0].AttributeRevision = "";
                        if (_txtAttrRequestField.Data != null)
                            (serviceData as ModifyMultiLotsAttribute).ServiceAttrsRequestForm = _txtAttributeRevisionField.Data.ToString();
                        if (_txtAttrRequestorField.Data != null)
                            (serviceData as ModifyMultiLotsAttribute).ServiceAttrsRequestor = _txtAttributeRevisionField.Data.ToString();
                        if (_txtModifyAttrsReasonField.Data != null)
                        {
                            (serviceData as ModifyMultiLotsAttribute).ServiceAttrsModifyAttrsReason = new NamedObjectRef();
                            (serviceData as ModifyMultiLotsAttribute).ServiceAttrsModifyAttrsReason.Name = _txtModifyAttrsReasonField.Data.ToString();
                        }
                        if (_txtApplyToChildLots.CheckControl.Checked == true)
                            (serviceData as ModifyMultiLotsAttribute).ApplyToChildLots = true;
                    }
                }
                base.GetInputData(serviceData);
            }
            catch (Exception ex)
            { }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //hide or show the lot detail button based on current theme
            var theme = Page.Session["CurrentTheme"] != null ? Page.Session["CurrentTheme"].ToString() : string.Empty;

            if (theme.ToLower() == "horizon")
            {
                _btnLotInfo.Visible = false;
                _btnLotInfoCmdBar.Visible = true;
            }
            else if (theme.ToLower() == "camstar")
            {
                _btnLotInfo.Visible = true;
                _btnLotInfoCmdBar.Visible = false;
            }

            txtComputerName.TextControl.Text = SEMI.AppCode.UIUtility.GetComputerName(this);
            _btnSelection.Hidden = true;
            _btnValid.Hidden = true;

            this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "LotInfoPopup").First().IsDisabled = true;

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

                        JQDataGrid _gridContainers = Page.FindCamstarControl("ModifyMultiLotsAttribute_Containers") as JQDataGrid;
                        JQDataGrid _gridAttributes = Page.FindCamstarControl("ModifyMultiLotsAttribute_ServiceAttrsDetails") as JQDataGrid;
                        foreach (string sContainer in sContainers)
                        {
                            ItemListGrid_AddNewRow(sContainer);
                        }
                        //nullify the containers list
                        _envSelectedLots.SS_ContainersList = null;
                    }
                    if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument" && Page.PortalContext.DataContract.GetValueByName("ModifyLotAttr_Attribute") != null)
                    {
                        var attrVal = Page.PortalContext.DataContract.GetValueByName<string>("ModifyLotAttr_PopupValue");
                        var attrRev = Page.PortalContext.DataContract.GetValueByName<string>("ModifyLotAttr_PopupRevision");
                        string attrSel = Page.PortalContext.DataContract.GetValueByName("ModifyLotAttr_Attribute").ToString();
                        if (!string.IsNullOrEmpty(attrSel))
                        {
                            ServiceAttrsDetails[] getServiceAttrsDetails = _gridAttributes.Data as ServiceAttrsDetails[];
                            for (int i = 0; i < getServiceAttrsDetails.Count(); i++)
                            {
                                if (getServiceAttrsDetails[i].Attribute.Name == attrSel)
                                {
                                    getServiceAttrsDetails[i].AttributeValue = attrVal;
                                    _txtAttributeValueField.Data = attrVal;
                                    getServiceAttrsDetails[i].AttributeRevision = attrRev;
                                    _txtAttributeRevisionField.Data = attrRev;
                                }
                            }
                            _gridAttributes.ClearData();
                            _gridAttributes.Data = getServiceAttrsDetails;
                            _gridAttributes.OriginalData = getServiceAttrsDetails;
                        }
                    }
                }
            }
        }
        protected void SetServiceAttributeDetails(string LotId)
        {
            try
            {
                // get the session and user profile
                var fs = FrameworkManagerUtil.GetFrameworkSession();

                // init service objects

                ModifyMultiLotsAttributeService oService = new ModifyMultiLotsAttributeService(fs.CurrentUserProfile);
                ModifyMultiLotsAttribute oServiceData = new ModifyMultiLotsAttribute();
                ModifyMultiLotsAttribute_Info oServiceInfo = new ModifyMultiLotsAttribute_Info();
                ModifyMultiLotsAttribute_Result oServiceResult = new ModifyMultiLotsAttribute_Result();

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
                ModifyMultiLotsAttribute_Request oServiceRequest = new ModifyMultiLotsAttribute_Request();
                oServiceRequest.Info = oServiceInfo;
                // execute!
                ResultStatus oResultStatus = oService.GetEnvironment(oServiceData, oServiceRequest, out oServiceResult);

                if (oResultStatus.IsSuccess)
                {
                    Array oAttrArray = oServiceResult.Value.ServiceAttrsDetailsSelection.ToArray();
                    foreach (ServiceAttrsDetails oAttr in oServiceResult.Value.ServiceAttrsDetailsSelection)
                    {
                        if (!_txtAttributeField.DropDownControl.Items.Contains(new ListItem(oAttr.Attribute.ToString())))
                        {
                            _txtAttributeField.DropDownControl.Items.Add(oAttr.Attribute.ToString());
                        }                            
                    }
                    (_gridAttributes.GridContext as BoundContext).Data = oAttrArray;
                    _gridAttributes.BoundContext.LoadData();
                    CamstarWebControl.SetRenderToClient(_gridAttributes);

                    AttributeField_DataChanged(null, null);

                    //Create Valid Values table
                    DataTable validValuesDT = new DataTable();
                    int countSvcAttr = (oServiceResult.Value as ModifyMultiLotsAttribute).ServiceAttrsDetailsSelection.Count();
                    validValuesDT.Columns.Add("Attribute", typeof(String));
                    validValuesDT.Columns.Add("AttributeValue", typeof(String));
                    validValuesDT.Columns.Add("AttributeRevision", typeof(String));
                    for (int i = 0; i < countSvcAttr; i++)
                    {
                        if (((oServiceResult.Value as ModifyMultiLotsAttribute).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues != null)
                        {
                            int countValidValues = ((oServiceResult.Value as ModifyMultiLotsAttribute).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.Count();
                            for (int x = 0; x < countValidValues; x++)
                            {
                                DataRow dtRow = validValuesDT.NewRow();
                                dtRow.SetField("Attribute", ((oServiceResult.Value as ModifyMultiLotsAttribute).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).Attribute.Name);
                                dtRow.SetField("AttributeValue", (((oServiceResult.Value as ModifyMultiLotsAttribute).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.GetValue(x) as AttributeValidValuesChanges).AttributeValue);
                                dtRow.SetField("AttributeRevision", (((oServiceResult.Value as ModifyMultiLotsAttribute).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.GetValue(x) as AttributeValidValuesChanges).AttributeRevision);
                                validValuesDT.Rows.Add(dtRow);
                            }
                        }
                    }
                    _gridValidValues.ClearData();
                    _gridValidValues.Data = validValuesDT;
                    _gridValidValues.OriginalData = validValuesDT;
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
        public void ContainerGrid_RowSelected(Object sender, EventArgs e)
        {
            try
            {
                string sRowID = _gridContainers.SelectedRowID;
                if (sRowID != null)
                {
                    _txtSelectedLot.Data = _gridContainers.GridContext.GetCell(sRowID, "Lot").ToString();
                    _txtSelectionId.Data = _txtSelectedLot.Data;
                    _btnLotInfo.Enabled = true;
                    this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "LotInfoPopup").First().IsDisabled = false;
                }
                else
                {
                    _btnLotInfo.Enabled = false;
                    this.Page.ActionDispatcher.PageActions().Where(prop => prop.Name == "LotInfoPopup").First().IsDisabled = true;
                }
            }
            catch (Exception ex)
            {
            }
        }
        public void AttributeField_DataChanged(Object sender, EventArgs e)
        {
            try
            {
                int i = 0;
                ServiceAttrsDetails[] getServiceAttrsDetails = _gridAttributes.Data as ServiceAttrsDetails[];
                int j = getServiceAttrsDetails.Count();
                string sSelectedValue = _txtAttributeField.DropDownControl.SelectedValue.ToString();

                bool sLoopValue = true;
                while (sLoopValue && i < j)
                {
                    string sRowID = i.ToString().PadLeft(6);
                    var vRow = ((OM.ServiceAttrsDetails)_gridAttributes.GridContext.GetItem(sRowID));
                    if (vRow.Attribute.ToString() == sSelectedValue)
                    {
                        if (!vRow.AttributeValue.IsNullOrEmpty())
                            _txtAttributeValueField.Data = vRow.AttributeValue.ToString();
                        else
                            _txtAttributeValueField.Data = "";

                        if (!vRow.AttributeRevision.IsNullOrEmpty())
                            _txtAttributeRevisionField.Data = vRow.AttributeRevision.ToString();
                        else
                            _txtAttributeRevisionField.Data = "";

                        if (!vRow.ObjectTypeName.IsNullOrEmpty())
                            _txtAttrObjectType.Data = vRow.ObjectTypeName.ToString();
                        else
                            _txtAttrObjectType.Data = "";

                        sLoopValue = false;
                    }
                    i++;
                }
            }
            catch (Exception ex)
            {
                DisplayMessage(new ResultStatus(ex.Message, false));
            }
        }
        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                Page.ClearValues();
                _envSelectedLots.ClearData();
                _txtAttributeField.DropDownControl.Items.Clear();
                _gridContainers.ClearData();
                _txtSelectionId.Focus();
            }
        }
    }
}



