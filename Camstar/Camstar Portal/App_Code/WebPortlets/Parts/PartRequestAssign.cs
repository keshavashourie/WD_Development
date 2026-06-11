/* Copyright 2023 Siemens */
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;

namespace Camstar.WebPortal.WebPortlets.Parts
{
    public class PartRequestAssign : MatrixWebPart, IPostBackEventHandler
    {
        private CWC.TextBox SvcTypeTextBox { get { return Page.FindCamstarControl("ServiceType") as CWC.TextBox; } }
        private CWC.TextBox ReadOnlyInfoTextBox { get { return Page.FindCamstarControl("ReadOnlyInfo") as CWC.TextBox; } }

        private CWC.NamedObject ResourceFieldNO { get { return Page.FindCamstarControl("ResourceField") as CWC.NamedObject; } }
		private CWC.FlyoutDropDown ResourcePartsButton { get { return Page.FindCamstarControl("ResourcePartsButton") as CWC.FlyoutDropDown; } }
        private CWC.NamedObject JobOrderFieldNO { get { return Page.FindCamstarControl("JobOrderField") as CWC.NamedObject; } }
		private CWC.FlyoutDropDown JobHistoryButton { get { return Page.FindCamstarControl("JobHistoryButton") as CWC.FlyoutDropDown; } }
        private CWC.NamedObject RequestOrderFieldNO { get { return Page.FindCamstarControl("RequestOrderField") as CWC.NamedObject; } }
		private CWC.FlyoutDropDown RequestHistoryButton { get { return Page.FindCamstarControl("RequestHistoryButton") as CWC.FlyoutDropDown; } }
        private CWC.DropDownList RequestTypeFieldDdl { get { return Page.FindCamstarControl("RequestTypeField") as CWC.DropDownList; } }

        /// <summary>
        /// This hidden grid appears to hold the list of parts that the user may select from in the ServiceDetailsGrid
        /// </summary>
        private JQDataGrid PartsSelectionValuesGrid { get { return Page.FindCamstarControl("SelectionValuePanel") as JQDataGrid; } }
        private JQDataGrid RequstOrderMatPartsPanelGrid { get { return Page.FindCamstarControl("RequestOrderMaterialPartsPanel") as JQDataGrid; } }
        private JQDataGrid SvcDetailsGrid { get { return Page.FindCamstarControl("ServiceDetailsGrid") as JQDataGrid; } }
        private JQDataGrid SvcDetailsToSubmitGrid { get { return Page.FindCamstarControl("ServiceDetailsToSubmit") as JQDataGrid; } }
        private CWC.CheckBox AutoCompleteFieldCB { get { return Page.FindCamstarControl("AutoCompleteField") as CWC.CheckBox; } }
        private CWC.CheckBox FilterBoxCB { get { return Page.FindCamstarControl("FilterBox") as CWC.CheckBox; } }

        private CWC.TextBox MaterialPartQueryTB { get { return Page.FindCamstarControl("MaterialPartQuery") as CWC.TextBox; } }
        private CWC.TextBox SelectedMaterialPartTB { get { return Page.FindCamstarControl("SelectedMaterialPart") as CWC.TextBox; } }
        private CWC.TextBox ShopfloorCommentsTB { get { return Page.FindCamstarControl("Shopfloor_Comments") as CWC.TextBox; } }
        private CWC.TextBox HiddenSelectedRowId { get { return Page.FindCamstarControl("HiddenSelectedRowId") as CWC.TextBox; } }
        private Camstar.WebPortal.PortalFramework.ToggleContainer PartRequestCommentsTC { get { return Page.FindCamstarControl("PartRequestComments_Toggle") as Camstar.WebPortal.PortalFramework.ToggleContainer; } }
        private CWC.NamedObject PartInlineEditorNO { get { return SvcDetailsGrid.FindControl("ServiceDetailsGrid_Part_InlineEditorControl") as CWC.NamedObject; } }

        private DataTable PartsSelectionValuesDT = new DataTable();

        public PartRequestAssign()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public void RaisePostBackEvent(string eventArgument)
        {

        }

        private void SetControls()
        {
            //int mygridsize = 1020;

            if (this.PrimaryServiceType == "PartRequestAssign")
            {
                if (ReadOnlyInfoTextBox.Data != null)
                {
                    ResourceFieldNO.ReadOnly = true;
                    JobOrderFieldNO.ReadOnly = true;
                    RequestOrderFieldNO.ReadOnly = true;
                }
                RequestTypeFieldDdl.ReadOnly = true;

                //for (int i = 0; i < (requestorderpanel.GridContext as BoundContext).Fields.Count; i++)
                //{
                //    if ((requestorderpanel.GridContext as BoundContext).Fields[i].ID == "_extender_")
                //        (requestorderpanel.GridContext as BoundContext).Fields[i].Width = 80;
                //}

                //for (int i = 0; i < (servicedetailspanel.GridContext as BoundContext).Fields.Count; i++)
                //{
                //    if ((servicedetailspanel.GridContext as BoundContext).Fields[i].ID == "RequestPartQty")
                //        (servicedetailspanel.GridContext as BoundContext).Fields[i].Visible = false;
                //    else if ((servicedetailspanel.GridContext as BoundContext).Fields[i].ID == "PartToReplaceName")
                //        (servicedetailspanel.GridContext as BoundContext).Fields[i].Visible = false;
                //    else if ((servicedetailspanel.GridContext as BoundContext).Fields[i].ID == "PartToReplaceQty")
                //        (servicedetailspanel.GridContext as BoundContext).Fields[i].Visible = false;
                //    else if ((servicedetailspanel.GridContext as BoundContext).Fields[i].ID == "ScrapQty")
                //        (servicedetailspanel.GridContext as BoundContext).Fields[i].Visible = false;
                //    else if ((servicedetailspanel.GridContext as BoundContext).Fields[i].ID == "PhysicalLocation")
                //        (servicedetailspanel.GridContext as BoundContext).Fields[i].Visible = false;
                //    else if ((servicedetailspanel.GridContext as BoundContext).Fields[i].ID == "PhysicalPosition")
                //        (servicedetailspanel.GridContext as BoundContext).Fields[i].Visible = false;
                //    else if ((servicedetailspanel.GridContext as BoundContext).Fields[i].ID == "_extender_")
                //        (servicedetailspanel.GridContext as BoundContext).Fields[i].Width = 480;
                //}
            }
        }

        public void SetGridButtons()
        {
            if (RequestTypeFieldDdl.Data == null)
            {
                SvcDetailsGrid.Settings.NavigatorActions[0].Enable = false;
                SvcDetailsGrid.Settings.NavigatorActions[0].Visible = false;
                SvcDetailsGrid.Settings.NavigatorActions[2].Enable = false;
                SvcDetailsGrid.Settings.NavigatorActions[2].Visible = false;
            }
            else
            {
                SvcDetailsGrid.Settings.NavigatorActions[0].Enable = true;
                SvcDetailsGrid.Settings.NavigatorActions[0].Visible = true;
                SvcDetailsGrid.Settings.NavigatorActions[2].Enable = true;
                SvcDetailsGrid.Settings.NavigatorActions[2].Visible = true;
            }
        }

        public void FetchGridData()
        {
            string serviceName = this.PrimaryServiceType.ToString();

            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            ResultStatus res = new ResultStatus(null, false);
            var cdo = WCFObject.CreateObject(serviceName) as ICreator;
            var request = WCFObject.CreateObject(serviceName + "_Request") as ICreator;
            var reqInfo = WCFObject.CreateObject(serviceName + "_Info") as ICreator;
            Result result = null;
            var service = new WSDataCreator().CreateService(serviceName, profile);

            // Set Data Input
            cdo.SetValue("Resource", ResourceFieldNO.Data as NamedObjectRef);
            cdo.SetValue("RequestOrder", RequestOrderFieldNO.Data as NamedObjectRef);
            cdo.SetValue("JobOrder", JobOrderFieldNO.Data as NamedObjectRef);

            // Set Request Value
            if (this.PrimaryServiceType == "PartRequestAssign")
            {
                reqInfo.SetValue("RequestType", new OM.Info(true));
                reqInfo.SetValue("RequestOrderMaterialParts", new OM.PartRequestOrderMaterialPart_Info());
                reqInfo.SetValue("RequestOrderMaterialParts.MaterialPart", new OM.Info(true));
                reqInfo.SetValue("RequestOrderMaterialParts.RequestPartQty", new OM.Info(true));
                reqInfo.SetValue("RequestOrderMaterialParts.AssignPartQty", new OM.Info(true));

                reqInfo.SetValue("RequestOrderParts", new OM.PartRequestOrderPart_Info());
                reqInfo.SetValue("RequestOrderParts.MaterialPart", new OM.Info(true));
                reqInfo.SetValue("RequestOrderParts.PartName", new OM.Info(true));
                reqInfo.SetValue("RequestOrderParts.PartQty", new OM.Info(true));
                reqInfo.SetValue("RequestOrderParts.PhysicalLocation", new OM.Info(true));
                reqInfo.SetValue("RequestOrderParts.PhysicalPosition", new OM.Info(true));

                request.SetValue("Info", reqInfo);

                // Execute Request 
                ResultStatus rslt = service.GetEnvironment(cdo as DCObject, request as Request, out result);

                // Result
                if (rslt.IsSuccess)
                {
                    RequestTypeFieldDdl.Data = (result as ICreator).GetValue("Value.RequestType");
                    PartRequestOrderPart[] collectedData = ((result as ICreator).GetValue("Value.RequestOrderParts") as PartRequestOrderPart[]);

                    var newSvcDetails = new List<PartTxnServiceDetail>();
                    int rowCount = 0;

                    if (collectedData != null)
                        rowCount = collectedData.Count();

                    for (int i = 0; i < rowCount; i++)
                    {
                        newSvcDetails.Add(new PartTxnServiceDetail
                        {
                            MaterialPart = collectedData[i].MaterialPart,
                            PartName = collectedData[i].PartName,
                            PartQty = collectedData[i].PartQty,
                            PhysicalLocation = collectedData[i].PhysicalLocation,
                            PhysicalPosition = collectedData[i].PhysicalPosition
                        });
                    }

                    SvcDetailsToSubmitGrid.Data = newSvcDetails.ToArray();
                    SvcDetailsToSubmitGrid.OriginalData = newSvcDetails.ToArray();

                    RequstOrderMatPartsPanelGrid.Data = ((result as ICreator).GetValue("Value.RequestOrderMaterialParts"));
                    RequstOrderMatPartsPanelGrid.OriginalData = ((result as ICreator).GetValue("Value.RequestOrderMaterialParts"));
                }
            }
        }

        public void ForceRequestType()
        {
            if (RequestTypeFieldDdl.Text == "Request")
                RequestTypeFieldDdl.Data = 1;
            else if (RequestTypeFieldDdl.Text == "Return")
                RequestTypeFieldDdl.Data = 2;
        }

        public void UpdateQueryParams()
        {
            if ((Page.FindCamstarControl("SelectedMaterialPart") as CWC.TextBox).Data != null)
            {
                string selectedmaterialpart = (Page.FindCamstarControl("SelectedMaterialPart") as CWC.TextBox).Data.ToString();
                string[] splitmaterialpart = selectedmaterialpart.Split(':');
                (Page.FindCamstarControl("MaterialPartQuery") as CWC.TextBox).Data = splitmaterialpart[0];
                (Page.FindCamstarControl("HiddenMaterialPart") as CWC.RevisionedObject).Data = splitmaterialpart[0];
            }
        }

        public void UpdateServiceDetailsToSubmit()
        {
            bool matched = false;
            var savedSvcDetails = new List<PartTxnServiceDetail>();
            int savedCount = SvcDetailsToSubmitGrid.GridContext.GetTotalRows();
            DataTable getSavedTable;
            (SvcDetailsToSubmitGrid.GridContext as BoundContext).GenerateFullExcelData(savedCount, out getSavedTable);
            for (int i = 0; i < savedCount; i++)
            {
                string matpart = getSavedTable.Rows[i].Field<String>("MaterialPart").ToString();
                string[] splitmatpart = matpart.Split(':');
                savedSvcDetails.Add(new PartTxnServiceDetail
                {
                    PartName = getSavedTable.Rows[i].Field<String>("PartName").ToString(),
                    PartQty = Convert.ToInt32(getSavedTable.Rows[i].Field<String>("PartQty")),
                    MaterialPart = new RevisionedObjectRef { Name = splitmatpart[0], Revision = splitmatpart[1] },
                });
            }

            int gridCount = SvcDetailsGrid.GridContext.GetTotalRows();
            DataTable getDataTable;
            (SvcDetailsGrid.GridContext as BoundContext).GenerateFullExcelData(gridCount, out getDataTable);

            for (int i = 0; i < gridCount; i++)
            {
                matched = false;
                for (int x = 0; x < savedCount; x++)
                {
                    if (getDataTable.Rows[i].Field<String>("Part").ToString() == getSavedTable.Rows[x].Field<String>("PartName").ToString())
                    {
                        matched = true;
                        if (getDataTable.Rows[i].Field<String>("AssignPartQty") != null)
                        {
                            if (getDataTable.Rows[i].Field<String>("AssignPartQty").ToString() != getSavedTable.Rows[x].Field<String>("PartQty").ToString())
                            {
                                savedSvcDetails.RemoveAt(x);
                                string matpart = getDataTable.Rows[i].Field<String>("MaterialPart").ToString();
                                string[] splitmatpart = matpart.Split(':');

                                savedSvcDetails.Add(new PartTxnServiceDetail
                                {
                                    PartName = getDataTable.Rows[i].Field<String>("Part").ToString(),
                                    PartQty = Convert.ToInt32(getDataTable.Rows[i].Field<String>("AssignPartQty")),
                                    MaterialPart = new RevisionedObjectRef { Name = splitmatpart[0], Revision = splitmatpart[1] },
                                });
                            }
                        }
                        else
                        {
                            savedSvcDetails.RemoveAt(x);
                        }
                    }
                }
                if (getDataTable.Rows[i].Field<String>("AssignPartQty") != null && matched == false)
                {
                    string matpart = getDataTable.Rows[i].Field<String>("MaterialPart").ToString();
                    string[] splitmatpart = matpart.Split(':');

                    savedSvcDetails.Add(new PartTxnServiceDetail
                    {
                        PartName = getDataTable.Rows[i].Field<String>("Part").ToString(),
                        PartQty = Convert.ToInt32(getDataTable.Rows[i].Field<String>("AssignPartQty")),
                        MaterialPart = new RevisionedObjectRef { Name = splitmatpart[0], Revision = splitmatpart[1] },
                    });
                }
            }
            SvcDetailsToSubmitGrid.Data = savedSvcDetails.ToArray();
            SvcDetailsToSubmitGrid.OriginalData = savedSvcDetails.ToArray();
        }

        public void UpdateSvcDetailsGrid()
        {
            bool matched;

            // Grid loaded based on query
            (PartsSelectionValuesGrid.GridContext as BoundContext).LoadData();

            int savedCount = SvcDetailsToSubmitGrid.GridContext.GetTotalRows();
            DataTable getSavedTable;
            (SvcDetailsToSubmitGrid.GridContext as BoundContext).GenerateFullExcelData(savedCount, out getSavedTable);

            // Number of parts the user may choose from
            int selValPartsCount = PartsSelectionValuesGrid.GridContext.GetTotalRows();
            DataTable partsSelectionValuesDT;
            (PartsSelectionValuesGrid.GridContext as BoundContext).GenerateFullExcelData(
                selValPartsCount, 
                out partsSelectionValuesDT);

            var newSvcDetails = new List<PartTxnServiceDetail>();
            for (int x = 0; x < savedCount; x++)
            {
                matched = false;
                for (int i = 0; i < selValPartsCount; i++)
                {
                    if (partsSelectionValuesDT.Rows[i].Field<String>("Name").ToString() == getSavedTable.Rows[x].Field<String>("PartName").ToString())
                    {
                        string matpart = partsSelectionValuesDT.Rows[i].Field<String>("MaterialPart").ToString();
                        string[] splitmatpart = matpart.Split(':');
                        newSvcDetails.Add(new PartTxnServiceDetail
                        {
                            Part = new NamedObjectRef { Name = partsSelectionValuesDT.Rows[i].Field<String>("Name") },
                            PartQty = partsSelectionValuesDT.Rows[i].Field<int>("PartQty"),
                            MaterialPart = new RevisionedObjectRef { Name = partsSelectionValuesDT.Rows[i].Field<String>("MaterialPart"), Revision = partsSelectionValuesDT.Rows[i].Field<String>("Revision") },
                            AssignPartQty = Convert.ToInt32(getSavedTable.Rows[x].Field<String>("PartQty"))
                        });
                        matched = true;
                    }
                }

                if (matched == false)
                {
                    string matpart = getSavedTable.Rows[x].Field<String>("MaterialPart").ToString();
                    string[] splitmatpart = matpart.Split(':');
                    newSvcDetails.Add(new PartTxnServiceDetail
                    {
                        Part = new NamedObjectRef { Name = getSavedTable.Rows[x].Field<String>("PartName").ToString() },
                        MaterialPart = new RevisionedObjectRef { Name = splitmatpart[0], Revision = splitmatpart[1] },
                        AssignPartQty = Convert.ToInt32(getSavedTable.Rows[x].Field<String>("PartQty"))
                    });
                }
            }

            //if (filterbox.IsChecked)
            //{
            //    int dataCount = newSvcDetails.Count();
            //    for (int i = 0; i < dataCount; i++)
            //    {
            //        if (newSvcDetails[i].AssignPartQty == null)
            //        {
            //            newSvcDetails.RemoveAt(i);
            //            i = i - 1;
            //            dataCount = dataCount - 1;
            //        }
            //    }
            //}

            SvcDetailsGrid.Data = newSvcDetails.ToArray();
            SvcDetailsGrid.OriginalData = newSvcDetails.ToArray();

            CamstarWebControl.SetRenderToClient(SvcDetailsGrid);
        }

        protected void ServiceDetailsGrid_GetRowSnapItem(object item, IEnumerable<DataColumn> dataColumns, DataRow svcDetailRow)
        {
            if (!(item is OM.PartTxnServiceDetail) || svcDetailRow["Part"] == System.DBNull.Value)
            {
                return;
            }

            int partsSelValCount = PartsSelectionValuesGrid.GridContext.GetTotalRows();
            for (int i = 0; i < partsSelValCount; i++)
            {
                if (PartsSelectionValuesDT.Rows.Count > 0)
                {
                    if (svcDetailRow["Part"].ToString() == PartsSelectionValuesDT.Rows[i].Field<String>("Name").ToString())
                    {
                        svcDetailRow["MaterialPart"] = PartsSelectionValuesDT.Rows[i].Field<String>("MaterialPart").ToString() + ":" + PartsSelectionValuesDT.Rows[i].Field<String>("Revision").ToString();
                        svcDetailRow["PartQty"] = PartsSelectionValuesDT.Rows[i].Field<int>("PartQty");
                    }
                }
            }
        }

        public void FilterBox_DataChanged(object sender, EventArgs e)
        {
            RequstOrderMatPartsPanelGrid.SelectedRowID = null;
            MaterialPartQueryTB.Data = "%";
            SelectedMaterialPartTB.Data = "";
        }

        public void RequestType_DataChanged(object sender, EventArgs e)
        {
            //SetControls();
        }

        public void ResourceField_DataChanged(object sender, EventArgs e)
        {
            if (this.PrimaryServiceType == "PartRequestAssign")
            {
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                ResultStatus res = new ResultStatus(null, false);

                PartRequestAssignService Svc = new PartRequestAssignService(fs.CurrentUserProfile);
                OM.PartRequestAssign SvcData = new OM.PartRequestAssign();
                PartRequestAssign_Info SvcInfo = new PartRequestAssign_Info();
                PartRequestAssign_Request ReqData = new PartRequestAssign_Request();
                PartRequestAssign_Result ResData = new PartRequestAssign_Result();

                SvcData.Resource = new NamedObjectRef();
                if (ResourceFieldNO.Data != null)
                {
                    SvcData.Resource.Name = ResourceFieldNO.Data.ToString();

                    SvcInfo.JobOrder = new Info();
                    SvcInfo.JobOrder.RequestSelectionValues = true;

                    ReqData.Info = SvcInfo;
                    OM.ResultStatus Results = Svc.GetEnvironment(SvcData, ReqData, out ResData);
                    if (Results.IsSuccess & ResData.Environment != null)
                    {
                        if (ResData.Environment.JobOrder.SelectionValues.Rows != null)
                            JobOrderFieldNO.Data = ResData.Environment.JobOrder.SelectionValues.Rows[0].Values[0];
                    }
                    JobOrderField_DataChanged(sender, e);
                }
                else
                {
                    JobOrderFieldNO.Data = null;
                    RequestOrderFieldNO.Data = null;
                    RequestTypeFieldDdl.Data = null;
                    RequstOrderMatPartsPanelGrid.ClearData();
                    SvcDetailsGrid.ClearData();
                }
            }
        }

        public void JobOrderField_DataChanged(object sender, EventArgs e)
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            ResultStatus res = new ResultStatus(null, false);

            PartRequestAssignService Svc = new PartRequestAssignService(fs.CurrentUserProfile);
            OM.PartRequestAssign SvcData = new OM.PartRequestAssign();
            PartRequestAssign_Info SvcInfo = new PartRequestAssign_Info();
            PartRequestAssign_Request ReqData = new PartRequestAssign_Request();
            PartRequestAssign_Result ResData = new PartRequestAssign_Result();

            SvcData.Resource = new NamedObjectRef();
            if (ResourceFieldNO.Data != null)
                SvcData.Resource.Name = ResourceFieldNO.Data.ToString();
            SvcData.JobOrder = new NamedObjectRef();
            if (JobOrderFieldNO.Data != null)
                SvcData.JobOrder.Name = JobOrderFieldNO.Data.ToString();

            if (ResourceFieldNO.Data != null)
            {
                SvcInfo.RequestOrder = new Info();
                SvcInfo.RequestOrder.RequestSelectionValues = true;

                SvcInfo.RequestSelectionValuesInfo = new SelectionValuesInfo();
                SvcInfo.RequestSelectionValuesInfo.Parameters = new QueryParameter[]
                {
                    new QueryParameter {Name="NameFilter", Value="%" },
                };

                SvcInfo.RequestSelectionValuesInfo.Options = new QueryOptions();
                SvcInfo.RequestSelectionValuesInfo.Options.StartRow = 1;
                SvcInfo.RequestSelectionValuesInfo.Options.RowSetSize = 50;

                ReqData.Info = SvcInfo;

                OM.ResultStatus Results = Svc.GetEnvironment(SvcData, ReqData, out ResData);
                if (Results.IsSuccess & ResData.Environment != null)
                {
                    if (ResData.Environment.RequestOrder.SelectionValues.Rows != null)
                        RequestOrderFieldNO.Data = ResData.Environment.RequestOrder.SelectionValues.Rows[0].Values[0];
                }
            }
        }

        protected void RequestOrderField_DataChanged(object sender, EventArgs e)
        {
            RequstOrderMatPartsPanelGrid.ClearData();
            SvcDetailsGrid.ClearData();
            FetchGridData();
            UpdateSvcDetailsGrid();
            SetControls();
            SetGridButtons();
        }

        private PartTxnServiceDetail[] GetServiceDetailsData()
        {
            return SvcDetailsGrid.Data != null ? (SvcDetailsGrid.Data as Array).Cast<PartTxnServiceDetail>().ToArray() : new PartTxnServiceDetail[] { };
        }

        protected string GetCellValue(int row, string column)
        {
            string result = string.Empty;
            var obj = SvcDetailsGrid.GridContext.GetCell(row.ToString(), column);
            if (obj != null)
                result = obj.ToString();
            return result;
        }
        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);
            if (this.PrimaryServiceType != "PartRequestAssign" || !(serviceData is OM.PartRequestAssign))
            {
                return;
            }

            //UpdateServiceDetailsToSubmit();
            PartTxnServiceDetail[] partTxnServiceDetails = GetServiceDetailsData();
            if (partTxnServiceDetails.Count() > 0)
            {
                (serviceData as OM.PartTxn).ServiceDetails = new PartTxnServiceDetail[partTxnServiceDetails.Count()];
                for (int i = 0; i < partTxnServiceDetails.Count(); i++)
                {
                    string partName = GetCellValue(i, "Part");
                    if (!string.IsNullOrEmpty(partName))
                    {
                        string partQty = GetCellValue(i, "AssignPartQty");
                        string materialPart = GetCellValue(i, "MaterialPart");
                        string[] splitMaterialPart = materialPart.Split(':');

                        if (string.IsNullOrEmpty(partQty))
                        {
                            partQty = "0";
                        }

                        (serviceData as OM.PartTxn).ServiceDetails[i] = new PartTxnServiceDetail();
                        (serviceData as OM.PartTxn).ServiceDetails[i].PartName = partName;
                        (serviceData as OM.PartTxn).ServiceDetails[i].PartQty = Convert.ToInt32(partQty);
                        if (splitMaterialPart.Length == 1)
                        {
                            (serviceData as OM.PartTxn).ServiceDetails[i].MaterialPart = new RevisionedObjectRef { Name = splitMaterialPart[0], RevisionOfRecord = true };
                        }
                        else if (splitMaterialPart.Length == 2)
                        {
                            (serviceData as OM.PartTxn).ServiceDetails[i].MaterialPart = new RevisionedObjectRef { Name = splitMaterialPart[0], Revision = splitMaterialPart[1] };
                        }
                    }
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.Page.PrimaryServiceType = SvcTypeTextBox.Data.ToString();
            if (this.PrimaryServiceType == "PartRequestAssign")
            {
                if (this.Page.IsPostBack)
                {
                    UpdateQueryParams();
                    //UpdateServiceDetailsToSubmit();
                }
            }
            //UpdateSvcDetailsGrid();
        }

        //----------------------------------------------
        // Part Inline Editor Control Data Changed event
        //----------------------------------------------
        public void PartInlineEditorControl_DataChanged(object sender, EventArgs e)
        {
            if ((sender as CWC.NamedObject).Data == null)
            {
                return;
            }

            Page.StatusBar.ClearMessage();
            string selectedPart = (sender as CWC.NamedObject).Data.ToString();
            int selectedRowId = Convert.ToInt32(HiddenSelectedRowId.Data);

            PartTxnServiceDetail[] newSvcDetail = SvcDetailsGrid.Data as PartTxnServiceDetail[];
            int svcDetailCount = SvcDetailsGrid.GridContext.GetTotalRows();
            if (svcDetailCount > 0)
            {
                // Search the grid of part selection values for the part the user selected in the dropdown
                DataRow selectedPartRow = PartsSelectionValuesDT.AsEnumerable().SingleOrDefault(part => part["Name"].ToString() == selectedPart);
                if(selectedPartRow != null)
                {
                    newSvcDetail[selectedRowId].MaterialPart = new RevisionedObjectRef();
                    newSvcDetail[selectedRowId].MaterialPart.Name = selectedPartRow.Field<String>("MaterialPart");
                    newSvcDetail[selectedRowId].MaterialPart.Revision = selectedPartRow.Field<String>("Revision");
                    newSvcDetail[selectedRowId].PartQty = selectedPartRow.Field<int>("PartQty");
                }

                newSvcDetail[selectedRowId].Part = new NamedObjectRef();
                newSvcDetail[selectedRowId].Part.Name = selectedPart;
            }
            SvcDetailsGrid.ClearData();
            SvcDetailsGrid.Data = newSvcDetail;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (ReadOnlyInfoTextBox.Data == null)
            {
                ResourceFieldNO.DataChanged += new EventHandler(ResourceField_DataChanged);
                JobOrderFieldNO.DataChanged += new EventHandler(JobOrderField_DataChanged);
                RequestOrderFieldNO.DataChanged += new EventHandler(RequestOrderField_DataChanged);
                RequestTypeFieldDdl.DataChanged += new EventHandler(RequestType_DataChanged);
            }

            if (!this.Page.IsPostBack && ResourceFieldNO.Data != null)
            {
                FetchGridData();
                UpdateSvcDetailsGrid();
            }

            (SvcDetailsGrid.GridContext as ItemDataContext).GetRowSnapItem += ServiceDetailsGrid_GetRowSnapItem;

            PartInlineEditorNO.DataChanged += new EventHandler(PartInlineEditorControl_DataChanged);
            PartInlineEditorNO.AutoPostBack = true;
            int savedCount = PartsSelectionValuesGrid.GridContext.GetTotalRows();
            (PartsSelectionValuesGrid.GridContext as BoundContext).GenerateFullExcelData(savedCount, out PartsSelectionValuesDT);

			CamstarWebControl.SetRenderToClient(ResourcePartsButton);
			CamstarWebControl.SetRenderToClient(JobHistoryButton);
			CamstarWebControl.SetRenderToClient(RequestHistoryButton);

            SetControls();
            ForceRequestType();
            SetGridButtons();
            //ResourceFieldNO.Focus();

            if (!Page.IsPostBack && Page.IsAJAXFloatingFrame)
                ScriptManager.RegisterStartupScript(this.Page.Form, this.Page.GetType(), "PopUpMaximize", "pop.GetCallerPage().pop.maximize();", true);
        }

        //---------------------------------
        // Web part custom action
        //---------------------------------
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e) //Custom action settings
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                if (action.Parameters == "ResetFields")
                {
                    if (ReadOnlyInfoTextBox.Data != null)
                    {
                        RequestOrderField_DataChanged(sender, e);
                        AutoCompleteFieldCB.ClearData();
                        ShopfloorCommentsTB.ClearData();
                        PartRequestCommentsTC.Reset();
                    }
                    else
                        Page.ShopfloorReset(sender, e);
                }
            }
        }
    }
}
