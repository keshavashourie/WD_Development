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

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_LotMoveInventory : scsShopfloorBase
    {
        protected CWC.TextBox _txtSelectionId { get { return Page.FindCamstarControl("LotMoveInventory_SelectionId") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoEmployee { get { return Page.FindCamstarControl("LotMoveInventory_Employee") as CWC.NamedObject; } }
        protected JQDataGrid _gridContainers { get { return Page.FindCamstarControl("LotMoveInventory_Containers") as JQDataGrid; } }
        protected JQDataGrid _gridSvcAttribute { get { return Page.FindCamstarControl("LotMoveInventory_ServiceAttrsDetails") as JQDataGrid; } }
        protected JQDataGrid _gridValidValues { get { return Page.FindCamstarControl("ValidValuesGrid") as JQDataGrid; } }
        protected CWC.RevisionedObject _rdoToWorkflow { get { return Page.FindCamstarControl("LotMoveInventory_ToWorkflow") as CWC.RevisionedObject; } }
        protected CWC.NamedSubentity _subToStep { get { return Page.FindCamstarControl("LotMoveInventory_ToStep") as CWC.NamedSubentity; } }
        protected CWC.TextBox _txtComments { get { return Page.FindCamstarControl("Shopfloor_Comments") as CWC.TextBox; } }
        protected CWC.TextBox _txtPrimarySvcType { get { return Page.FindCamstarControl("PrimarySvcType") as CWC.TextBox; } }
        protected DataEnvelopControl _envContainers { get { return Page.FindCamstarControl("ContainerLists") as DataEnvelopControl; } }
        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("LotMoveInventory_ComputerName") as CWC.TextBox; } }

        //---------------------------------------------------
        // Get Row By Key Value function
        //---------------------------------------------------
        public static string GetRowByKeyValueInFieldGrid(Camstar.WebPortal.WebPortlets.MatrixWebPart RefPage, ref JQDataGrid TargetGrid, string Key1, string Value1, string Key2 = "", string Value2 = "")
        {
            try
            {
                DataTable tempDTable = new DataTable();
                (TargetGrid.GridContext as BoundContext).GenerateFullExcelData(TargetGrid.TotalRowCount, out tempDTable);
                for (int i = 0; i < TargetGrid.TotalRowCount; i++)
                {
                    if (string.IsNullOrEmpty(Key2))
                    {
                        if (tempDTable.Rows[i][Key1].ToString() == Value1)
                        {
                            return "FOUND";
                        }
                    }
                    else
                    {
                        if (tempDTable.Rows[i][Key1].ToString() == Value1 & tempDTable.Rows[i][Key2].ToString() == Value2)
                        {
                            return "FOUND";
                        }
                    }
                }
                return null;
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.TargetSite.Name + "(): " + Ex.Message);
            }
        }

        //---------------------------------------------------
        // Fetch Data function
        //---------------------------------------------------
        public void FetchData(string EventName)
        {
            try
            {
                bool bIsFirstLot = (_gridContainers.TotalRowCount == 0);
                //Initialize the Service Data
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                string ServiceType = this.Page.PrimaryServiceType;
                OM.Service serviceData = CreateServiceData(ServiceType);
                var Svc = new WSDataCreator().CreateService(ServiceType, profile);
                var SvcData = WCFObject.CreateObject(ServiceType) as ICreator;
                var SvcInfo = WCFObject.CreateObject(ServiceType + "_Info") as ICreator;
                var ReqData = WCFObject.CreateObject(ServiceType + "_Request") as ICreator;
                var ResData = WCFObject.CreateObject(ServiceType + "_Result") as ICreator;
                Result result = null;

                if (EventName == "SelectionId")
                {
                    SvcData.SetValue("SelectionId", _txtSelectionId.Data.ToString());
                    SvcInfo.SetValue("Containers", new Info(true));
                    if (bIsFirstLot)
                        SvcInfo.SetValue("ToWorkflow", new Info(true));
                }
                else if (EventName == "ToStepField")
                {
                    //SvcData.SetValue("Container", new ContainerRef());
                    //SvcData.SetValue("Container.Name", _gridContainers.GridContext.GetCell(0, "Lot").ToString());
                    if (Page.PrimaryServiceType == "LotMoveInventory")
                    {
                        (SvcData as LotMoveInventory).Container = new ContainerRef();
                        (SvcData as LotMoveInventory).Container.Name = _gridContainers.GridContext.GetCell(0, "Lot").ToString();
                    }
                    else if (Page.PrimaryServiceType == "DieBankLotMove")
                    {
                        (SvcData as DieBankLotMove).Container = new ContainerRef();
                        (SvcData as DieBankLotMove).Container.Name = _gridContainers.GridContext.GetCell(0, "Lot").ToString();
                    }
                    else if (Page.PrimaryServiceType == "TestStoreLotMove")
                    {
                        (SvcData as TestStoreLotMove).Container = new ContainerRef();
                        (SvcData as TestStoreLotMove).Container.Name = _gridContainers.GridContext.GetCell(0, "Lot").ToString();
                    }
                    else if (Page.PrimaryServiceType == "WaferSortInventoryLotMove")
                    {
                        (SvcData as WaferSortInventoryLotMove).Container = new ContainerRef();
                        (SvcData as WaferSortInventoryLotMove).Container.Name = _gridContainers.GridContext.GetCell(0, "Lot").ToString();
                    }
                    else if (Page.PrimaryServiceType == "WaferInventoryLotMove")
                    {
                        (SvcData as WaferInventoryLotMove).Container = new ContainerRef();
                        (SvcData as WaferInventoryLotMove).Container.Name = _gridContainers.GridContext.GetCell(0, "Lot").ToString();
                    }
                    else if (Page.PrimaryServiceType == "MaterialInventoryLotMove")
                    {
                        (SvcData as MaterialInventoryLotMove).Container = new ContainerRef();
                        (SvcData as MaterialInventoryLotMove).Container.Name = _gridContainers.GridContext.GetCell(0, "Lot").ToString();
                    }
                    SvcData.SetValue("ToWorkflow", _rdoToWorkflow.Data);
                    SvcData.SetValue("ToStep", _subToStep.Data);
                    //ServiceAttrsDetails
                    SvcInfo.SetValue("ServiceAttrsDetailsSelection", new ServiceAttrsDetails_Info());
                    SvcInfo.SetValue("ServiceAttrsDetailsSelection.Attribute", new Info(true));
                    SvcInfo.SetValue("ServiceAttrsDetailsSelection.AttributeName", new Info(true));
                    SvcInfo.SetValue("ServiceAttrsDetailsSelection.AlternateName1", new Info(true));
                    SvcInfo.SetValue("ServiceAttrsDetailsSelection.AlternateName2", new Info(true));
                    SvcInfo.SetValue("ServiceAttrsDetailsSelection.AttributeValue", new Info(true));
                    SvcInfo.SetValue("ServiceAttrsDetailsSelection.AttributeRevision", new Info(true));
                    SvcInfo.SetValue("ServiceAttrsDetailsSelection.AccessLevel", new Info(true));
                    SvcInfo.SetValue("ServiceAttrsDetailsSelection.FieldType", new Info(true));
                    SvcInfo.SetValue("ServiceAttrsDetailsSelection.IsRequired", new Info(true));
                    SvcInfo.SetValue("ServiceAttrsDetailsSelection.ServiceAttrsSetupName", new Info(true));
                    SvcInfo.SetValue("ServiceAttrsDetailsSelection.ObjectTypeName", new Info(true));
                    SvcInfo.SetValue("ServiceAttrsDetailsSelection.ValidValues", new AttributeValidValuesChanges_Info());
                    SvcInfo.SetValue("ServiceAttrsDetailsSelection.ValidValues.AttributeValue", new Info(true));
                    SvcInfo.SetValue("ServiceAttrsDetailsSelection.ValidValues.AttributeRevision", new Info(true));
                }
                ReqData.SetValue("Info", SvcInfo);
                //Request the data
                ResultStatus Results = (Svc as IShopFloorBase).ResolveSelectionId(SvcData as DCObject, ReqData as Request, out result);

                if (Results.IsSuccess)
                {
                    if (EventName == "SelectionId")
                    {
                        JQDataGrid theGrid = _gridContainers;
                        DataTable existingDataTable = new DataTable();
                        if (theGrid.TotalRowCount > 0)
                            (theGrid.GridContext as BoundContext).GenerateFullExcelData(theGrid.TotalRowCount, out existingDataTable);

                        if (existingDataTable.Columns.Contains("_leftSelector_column"))
                            existingDataTable.Columns.Remove("_leftSelector_column");

                        //string[] sHiddenColumnNames = new string[1];
                        //sHiddenColumnNames[0] = "SelectionId";
                        foreach (ContainerRef container in (result.Value as LotMoveInventory).Containers)
                        {
                            string findLot = GetRowByKeyValueInFieldGrid(this, ref theGrid, "Lot", container.Name);
                            if (findLot == null)
                            {
                                DataTable containersDataTable = SEMI.AppCode.UIUtility.GetLotQuerySelection(this, this.PrimaryServiceType, container.Name).GetAsExplicitlyDataTable();
                                //containersDataTable.Columns.Add("SelectionId").SetOrdinal(0);
                                
                                //foreach (DataRow row in containersDataTable.Rows)
                                    //row["SelectionId"] = _txtSelectionId.Data.ToString();

                                if (existingDataTable.Columns.Count == 0)
                                {
                                    foreach (DataColumn colname in containersDataTable.Columns)
                                        existingDataTable.Columns.Add(colname.ColumnName, colname.DataType);
                                }

                                foreach (DataRow dr in containersDataTable.Rows)
                                    existingDataTable.Rows.Add(dr.ItemArray);
                            }
                        }
                        theGrid.ClearData();
						RecordSet rs = SEMI.AppCode.UIUtility.GetLotQuerySelection(this, this.PrimaryServiceType, _txtSelectionId.Data.ToString());
						//existingDataTable = rs.GetAsExplicitlyDataTable();
                        SEMI.AppCode.GridUtility.ItemListGrid_SetColumns(this, existingDataTable, theGrid.ID, null, "RefTargetGrid", false, null, true, null, rs.Headers);
                        SEMI.AppCode.GridUtility.ItemListGrid_BindDataTable(this, existingDataTable, ref theGrid);
                        if (bIsFirstLot)
                        {
                            if ((result.Value as LotMoveInventory).ToWorkflow.Name != null)
                            {
                                _rdoToWorkflow.Data = (result.Value as LotMoveInventory).ToWorkflow;
                            }
                        }
                    }
                    else if (EventName == "ToStepField")
                    {
                        if ((result.Value as LotMoveInventory).ServiceAttrsDetailsSelection != null)
                        {
                            //Bind result to the grid
                            _gridSvcAttribute.ClearData();
                            _gridSvcAttribute.Data = (result.Value as LotMoveInventory).ServiceAttrsDetailsSelection.ToArray();
                            _gridSvcAttribute.OriginalData = (result.Value as LotMoveInventory).ServiceAttrsDetailsSelection.ToArray();

                            //Create Valid Values table
                            DataTable validValuesDT = new DataTable();
                            int countSvcAttr = (result.Value as LotMoveInventory).ServiceAttrsDetailsSelection.Count();
                            validValuesDT.Columns.Add("Attribute", typeof(String));
                            validValuesDT.Columns.Add("AttributeValue", typeof(String));
                            validValuesDT.Columns.Add("AttributeRevision", typeof(String));
                            for (int i = 0; i < countSvcAttr; i++)
                            {
                                int countValidValues = 0;
                                if (((result.Value as LotMoveInventory).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues != null)
                                    countValidValues = ((result.Value as LotMoveInventory).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.Count();
                                for (int x = 0; x < countValidValues; x++)
                                {
                                    DataRow dtRow = validValuesDT.NewRow();
                                    dtRow.SetField("Attribute", ((result.Value as LotMoveInventory).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).Attribute.Name);
                                    dtRow.SetField("AttributeValue", (((result.Value as LotMoveInventory).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.GetValue(x) as AttributeValidValuesChanges).AttributeValue);
                                    dtRow.SetField("AttributeRevision", (((result.Value as LotMoveInventory).ServiceAttrsDetailsSelection.GetValue(i) as ServiceAttrsDetails).ValidValues.GetValue(x) as AttributeValidValuesChanges).AttributeRevision);
                                    validValuesDT.Rows.Add(dtRow);
                                }
                            }
                            _gridValidValues.ClearData();
                            _gridValidValues.Data = validValuesDT;
                            _gridValidValues.OriginalData = validValuesDT;
                        }
                    }
                }
                else
                {
                    this.DisplayMessage(Results);
                    _txtSelectionId.ClearData();
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Selection Id Data Changed Event
        //-----------------------------------------
        public void SelectionIdField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (_txtSelectionId.Data != null)
                {
                    Page.StatusBar.ClearMessage();
                    JQDataGrid theGrid = _gridContainers;
                    string findLot = GetRowByKeyValueInFieldGrid(this, ref theGrid, "Lot", _txtSelectionId.Data.ToString());
                    if (findLot == null)
                    {
                        FetchData("SelectionId");
                    }
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
            finally
            {
                _txtSelectionId.ClearData();
            }
        }

        //-----------------------------------------
        // ToWorkflow Data Changed Event
        //-----------------------------------------
        public void ToWorkflowField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                _subToStep.ClearData();
                _gridSvcAttribute.ClearData();
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // ToStep Data Changed Event
        //-----------------------------------------
        public void ToStepField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                _gridSvcAttribute.ClearData();
                if (_subToStep.Data != null && _gridContainers.TotalRowCount > 0)
                    FetchData("ToStepField");
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //---------------------------------------------------
        // Override OnLoad event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                _txtSelectionId.DataChanged += new EventHandler(SelectionIdField_DataChanged);
                _rdoToWorkflow.DataChanged += new EventHandler(ToWorkflowField_DataChanged);
                _subToStep.DataChanged += new EventHandler(ToStepField_DataChanged);
                if (SEMI.AppCode.UIUtility.IsPopupClose(this))
                    OnPopupClose();
                if (Page.IsPostBack)
                {
                    _txtPrimarySvcType.Data = Page.PrimaryServiceType.ToString();
                    _txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
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
                if (Page.DataContract.GetValueByName("LotMove_LotList_DM") != null)
                {
                    string[] sContainers = Page.DataContract.GetValueByName("LotMove_LotList_DM") as string[];
                    _envContainers.SS_ContainersList = null;
                    Page.DataContract.SetValueByName("LotMove_LotList_DM", null);
                    foreach (string container in sContainers)
                        _txtSelectionId.Data = container;
                }
                if (Page.DataContract.GetValueByName("LotMove_ReturnedValue") != null)
                {
                    var attrVal = Page.PortalContext.DataContract.GetValueByName<string>("LotMove_ReturnedValue");
                    var attrRev = Page.PortalContext.DataContract.GetValueByName<string>("LotMove_ReturnedRevision");
                    string selectedAttr = Page.PortalContext.DataContract.GetValueByName("LotMove_SelectedAttribute").ToString();
                    if (!string.IsNullOrEmpty(selectedAttr))
                    {
                        ServiceAttrsDetails[] getServiceAttrsDetails = _gridSvcAttribute.Data as ServiceAttrsDetails[];
                        for (int i = 0; i < getServiceAttrsDetails.Count(); i++)
                        {
                            if (getServiceAttrsDetails[i].Attribute.Name == selectedAttr)
                            {
                                getServiceAttrsDetails[i].AttributeValue = attrVal;
                                getServiceAttrsDetails[i].AttributeRevision = attrRev;
                            }
                        }
                        _gridSvcAttribute.ClearData();
                        _gridSvcAttribute.Data = getServiceAttrsDetails;
                        _gridSvcAttribute.OriginalData = getServiceAttrsDetails;
                    }
                }
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
                    case "Reset":
                        {
                            Page.ShopfloorReset(sender, e);
                            break;
                        }
                }
            }
        } // WebPartCustomAction(object sender, CustomActionEventArgs e)

        //---------------------------------------------------------
        // Get Input Data override function
        //---------------------------------------------------------
        public override void GetInputData(Service serviceData)
        {
            try
            {
                base.GetInputData(serviceData);
                //Containers
                DataTable getContainers = new DataTable();
                (_gridContainers.GridContext as BoundContext).GenerateFullExcelData(_gridContainers.TotalRowCount, out getContainers);
                if (_gridContainers.Data != null)
                {
                    (serviceData as LotMoveInventory).Containers = new ContainerRef[_gridContainers.TotalRowCount];
                    for (int i = 0; i < _gridContainers.TotalRowCount; i++)
                    {
                        (serviceData as LotMoveInventory).Containers[i] = new ContainerRef();
                        (serviceData as LotMoveInventory).Containers[i].Name = getContainers.Rows[i].Field<string>("Lot").ToString();
                    }
                }
                //ServiceAttributes
                if (_gridSvcAttribute.Data != null)
                {
                    ServiceAttrsDetails[] getServiceAttrsDetails = _gridSvcAttribute.Data as ServiceAttrsDetails[];
                    if (serviceData is LotMoveInventory)
                    {
                        (serviceData as LotMoveInventory).ServiceAttrsDetails = new ServiceAttrsDetails[getServiceAttrsDetails.Count()];
                        for (int i = 0; i < getServiceAttrsDetails.Count(); i++)
                        {
                            (serviceData as LotMoveInventory).ServiceAttrsDetails[i] = new ServiceAttrsDetails();
                            (serviceData as LotMoveInventory).ServiceAttrsDetails[i].Attribute = new NamedObjectRef();
                            (serviceData as LotMoveInventory).ServiceAttrsDetails[i].Attribute.Name = getServiceAttrsDetails[i].Attribute.Name;
                            (serviceData as LotMoveInventory).ServiceAttrsDetails[i].FieldType = getServiceAttrsDetails[i].FieldType;
                            (serviceData as LotMoveInventory).ServiceAttrsDetails[i].ServiceAttrsSetupName = getServiceAttrsDetails[i].ServiceAttrsSetupName;
                            (serviceData as LotMoveInventory).ServiceAttrsDetails[i].AttributeValue = getServiceAttrsDetails[i].AttributeValue;
                            (serviceData as LotMoveInventory).ServiceAttrsDetails[i].AttributeRevision = getServiceAttrsDetails[i].AttributeRevision;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }
        //---------------------------------------------------------
        //
        //---------------------------------------------------------
        public void ResetPage()
        {
            Page.ShopfloorReset(null, null);
            _txtSelectionId.Focus();
        }
    }
}



