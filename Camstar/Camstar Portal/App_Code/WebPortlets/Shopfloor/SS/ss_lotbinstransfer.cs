/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.UI;
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

/// <summary>
/// Summary description for SS_LotBinsTransfer
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_LotBinsTransfer : MatrixWebPart
    {
        //Controls declaration
        private CWC.TextBox _txtContainerField { get { return Page.FindCamstarControl("ContainerField") as CWC.TextBox; } }
        private CWC.TextBox _txtContainer2Field { get { return Page.FindCamstarControl("Container2Field") as CWC.TextBox; } }
        private CWC.TextBox _txtEmployeeField { get { return Page.FindCamstarControl("EmployeeField") as CWC.TextBox; } }
        private CWC.TextBox _txtCommentsField { get { return Page.FindCamstarControl("LotBinsTransfer_Comments") as CWC.TextBox; } }
        private JQDataGrid _gridContainerBinsToTransfer { get { return Page.FindCamstarControl("LotBinsTransfer_ContainerBinsToTransfer") as JQDataGrid; } }
        private JQDataGrid _gridContainer2BinsToTransfer { get { return Page.FindCamstarControl("LotBinsTransfer_Container2BinsToTransfer") as JQDataGrid; } }
        private CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("LotBinsTransfer_ComputerName") as CWC.TextBox; } }
        
        //-----------------------------------------
        // OnLoad Event
        //-----------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                _txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
            }
            _txtContainerField.DataChanged += new EventHandler(ContainerField_DataChanged);
            _txtContainer2Field.DataChanged += new EventHandler(Container2Field_DataChanged);
        }

        //-----------------------------------------
        // Post Execute Event
        //-----------------------------------------
        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
            {
                (Page.FindCamstarControl("ContainersField") as JQDataGrid).ClearData();
            }
        }

        //-----------------------------------------
        // Override GetInputData Function
        //-----------------------------------------
        public override void GetInputData(Service serviceData)
        {
            try
            {
                (serviceData as LotBinsTransfer).Container = new ContainerRef();
                (serviceData as LotBinsTransfer).Container.Name = _txtContainerField.Data.ToString();
                (serviceData as LotBinsTransfer).Container2 = new ContainerRef();
                (serviceData as LotBinsTransfer).Container2.Name = _txtContainer2Field.Data.ToString();
                if (_txtEmployeeField.Data != null)
                {
                    (serviceData as LotBinsTransfer).Employee = new NamedObjectRef();
                    (serviceData as LotBinsTransfer).Employee.Name = _txtEmployeeField.Data.ToString();
                }
                if (_txtComputerName.Data != null)
                {
                    (serviceData as LotBinsTransfer).ComputerName = _txtComputerName.Data.ToString();
                }
                if (serviceData is LotBinsTransfer)
                {
                    if (_gridContainerBinsToTransfer.SelectedRowIDs != null)
                    {
                        List<LotBinsDetails> selectedContainerBins = new List<LotBinsDetails>();
                        foreach (string selectedRowID in _gridContainerBinsToTransfer.SelectedRowIDs)
                        {
                            LotBinsDetails newDetails = new LotBinsDetails();
                            if (_gridContainerBinsToTransfer.GridContext.GetCell(selectedRowID, "Bin").ToString() != "")
                                newDetails.Bin = _gridContainerBinsToTransfer.GridContext.GetCell(selectedRowID, "Bin").ToString();
                            if (_gridContainerBinsToTransfer.GridContext.GetCell(selectedRowID, "BinQtyToTransfer").ToString() != "")
                                newDetails.BinQtyToTransfer = Convert.ToInt32(_gridContainerBinsToTransfer.GridContext.GetCell(selectedRowID, "BinQtyToTransfer").ToString());
                            if (_gridContainerBinsToTransfer.GridContext.GetCell(selectedRowID, "BinLotId").ToString() != "")
                                newDetails.BinLotId = _gridContainerBinsToTransfer.GridContext.GetCell(selectedRowID, "BinLotId").ToString();
                            selectedContainerBins.Add(newDetails);
                        }
                        (serviceData as LotBinsTransfer).ContainerBinsToTransfer = selectedContainerBins.ToArray();
                    }
                    if (_gridContainer2BinsToTransfer.SelectedRowIDs != null)
                    {
                        List<LotBinsDetails> selectedContainer2Bins = new List<LotBinsDetails>();
                        foreach (string selectedRowID in _gridContainer2BinsToTransfer.SelectedRowIDs)
                        {
                            LotBinsDetails newDetails = new LotBinsDetails();
                            if (_gridContainer2BinsToTransfer.GridContext.GetCell(selectedRowID, "Bin") != null)
                                newDetails.Bin = _gridContainer2BinsToTransfer.GridContext.GetCell(selectedRowID, "Bin").ToString();
                            if (_gridContainer2BinsToTransfer.GridContext.GetCell(selectedRowID, "BinQtyToTransfer") != null)
                                newDetails.BinQtyToTransfer = Convert.ToInt32(_gridContainer2BinsToTransfer.GridContext.GetCell(selectedRowID, "BinQtyToTransfer").ToString());
                            if (_gridContainer2BinsToTransfer.GridContext.GetCell(selectedRowID, "BinLotId") != null)
                                newDetails.BinLotId = _gridContainer2BinsToTransfer.GridContext.GetCell(selectedRowID, "BinLotId").ToString();
                            selectedContainer2Bins.Add(newDetails);
                        }
                        (serviceData as LotBinsTransfer).Container2BinsToTransfer = selectedContainer2Bins.ToArray();
                    }
                }
                if (_txtCommentsField.Data != null)
                    (serviceData as LotBinsTransfer).Comments = _txtCommentsField.Data.ToString();
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // FetchData Event
        //-----------------------------------------
        public void FetchData(string EventName)
        {
            try
            {
                //Initialize Service & Objects
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                LotBinsTransferService Svc = new LotBinsTransferService(profile);
                LotBinsTransfer SvcData = new LotBinsTransfer();
                LotBinsTransfer_Info SvcInfo = new LotBinsTransfer_Info();
                LotBinsTransfer_Request ReqData = new LotBinsTransfer_Request();
                LotBinsTransfer_Result ResData = new LotBinsTransfer_Result();
                string sLotId = "";

                //Set Input Data
                if (EventName == "Container")
                {
                    sLotId = _txtContainerField.Data.ToString();
                    SvcData.Container = new ContainerRef();
                    SvcData.Container.Name = sLotId;
                    SvcInfo.ContainerBins = new LotBinsDetails_Info();
                    SvcInfo.ContainerBins.RequestValue = true;
                }
                else if (EventName == "Container2")
                {
                    sLotId = _txtContainer2Field.Data.ToString();
                    SvcData.Container2 = new ContainerRef();
                    SvcData.Container2.Name = sLotId;
                    SvcInfo.Container2Bins = new LotBinsDetails_Info();
                    SvcInfo.Container2Bins.RequestValue = true;
                }
                ReqData.Info = SvcInfo;

                //Execute Request
                ResultStatus Results = Svc.ResolveSelectionId(SvcData, ReqData, out ResData);
                if (Results.IsSuccess)
                {
                    Page.StatusBar.ClearMessage();

                    //Populate containers grid data
					RecordSet rs = SEMI.AppCode.UIUtility.GetLotQuerySelection(this, this.PrimaryServiceType, sLotId);
                    DataTable containersDataTable = rs.GetAsExplicitlyDataTable();

					var myList = new Header[rs.Headers.Length+2];
					myList[0] = new Header() { Name = "Lot 1", Label = new Label("LotBinsTransfer_Lot1") };
					myList[1] = new Header() { Name = "Lot 2", Label = new Label("LotBinsTransfer_Lot2") };
					rs.Headers.CopyTo(myList, 2);

					rs.Headers = myList;

                    JQDataGrid _gridContainersField = Page.FindCamstarControl("ContainersField") as JQDataGrid;
                    int containersCount = _gridContainersField.TotalRowCount;
                    if (EventName == "Container")
                    {
						containersDataTable.Columns["Lot"].ColumnName = "Lot 1";
                        containersDataTable.Columns.Add("Lot 2").SetOrdinal(1);

                        if (containersCount > 0)
                        {
                            DataRow dtRow = containersDataTable.NewRow();
                            int correctRowPos;
                            if (_gridContainersField.GridContext.GetCell(0, "Lot 1").ToString() != "" || _gridContainersField.GridContext.GetCell(1, "Lot 1").ToString() != "")
                                correctRowPos = 1;
                            else
                                correctRowPos = 0;

                            for (int i = 0; i < _gridContainersField.Settings.Columns.Count(); i++)
                            {
                                dtRow.SetField(_gridContainersField.Settings.Columns[i].Name, _gridContainersField.GridContext.GetCell(correctRowPos, _gridContainersField.Settings.Columns[i].Name));
                            }
                            containersDataTable.Rows.InsertAt(dtRow, 1);   
                        }
                        _gridContainersField.ClearData();
                        SEMI.AppCode.GridUtility.ItemListGrid_SetColumns(this, containersDataTable, _gridContainersField.ID,null, "RefTargetGrid", false,null, true, null, rs.Headers);
                        SEMI.AppCode.GridUtility.ItemListGrid_BindDataTable(this, containersDataTable, ref _gridContainersField);
                    }
                    else if (EventName == "Container2")
                    {
                        containersDataTable.Columns["Lot"].ColumnName = "Lot 2";
                        containersDataTable.Columns.Add("Lot 1").SetOrdinal(0);

                        if (containersCount > 0)
                        {
                            DataRow dtRow = containersDataTable.NewRow();
                            for (int i = 0; i < _gridContainersField.Settings.Columns.Count(); i++)
                            {
                                dtRow.SetField(_gridContainersField.Settings.Columns[i].Name, _gridContainersField.GridContext.GetCell(0, _gridContainersField.Settings.Columns[i].Name));
                            }
                            containersDataTable.Rows.InsertAt(dtRow, 0);
                        }
                        _gridContainersField.ClearData();

						SEMI.AppCode.GridUtility.ItemListGrid_SetColumns(this, containersDataTable, _gridContainersField.ID, null, "RefTargetGrid", false, null, true, null, rs.Headers);
                        SEMI.AppCode.GridUtility.ItemListGrid_BindDataTable(this, containersDataTable, ref _gridContainersField);
                    }
                    int countColumns = _gridContainersField.BoundContext.Fields.Count;
                    if (_gridContainersField.BoundContext.Fields[countColumns - 1].ID == "__STYLE")
                        _gridContainersField.BoundContext.Fields[countColumns - 1].Visible = false;

                    //Bind material list panel with retrieved data
                    if (EventName == "Container")
                    {
                        if (ResData.Value.ContainerBins != null)
                        {
                            LotBinsDetails[] containersBins = ResData.Value.ContainerBins;
                            _gridContainerBinsToTransfer.ClearData();
                            _gridContainerBinsToTransfer.Data = containersBins.ToArray();
                            _gridContainerBinsToTransfer.OriginalData = containersBins.ToArray();
                        }
                    }
                    else if (EventName == "Container2")
                    {
                        if (ResData.Value.Container2Bins != null)
                        {
                            LotBinsDetails[] containers2Bins = ResData.Value.Container2Bins;
                            foreach (LotBinsDetails containers2Bin in containers2Bins)
                            {
                                containers2Bin.BinQtyToTransfer.Value = containers2Bin.BinQty.Value;
                            }
                            _gridContainer2BinsToTransfer.ClearData();
                            _gridContainer2BinsToTransfer.Data = containers2Bins.ToArray();
                            _gridContainer2BinsToTransfer.OriginalData = containers2Bins.ToArray();
                        }
                    }
                    
                } //Results.IsSuccess
                else
                {
                    this.DisplayMessage(Results);
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Container Field Data Changed Event
        //-----------------------------------------
        public void ContainerField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                FetchData("Container");
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Container2 Field Data Changed Event
        //-----------------------------------------
        public void Container2Field_DataChanged(object sender, EventArgs e)
        {
            try
            {
                FetchData("Container2");
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        //-----------------------------------------
        // Override WebPartCustomAction
        //-----------------------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e) //Custom action settings
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                if (action.Parameters == "Reset")
                {
                    Page.ShopfloorReset(sender, e as CustomActionEventArgs);
                    (Page.FindCamstarControl("ContainersField") as JQDataGrid).ClearData();
                }
            }
        }
    }
}



