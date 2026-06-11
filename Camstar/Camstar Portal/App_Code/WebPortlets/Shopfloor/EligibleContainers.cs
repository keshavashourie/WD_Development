// Copyright Siemens 2023  
using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using System.Linq;
using System.Data.Linq;

using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Personalization;
using CamstarPortal.WebControls;
using Camstar.WCF.Services;
using System.Collections;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class EligibleContainers : MatrixWebPart, IPostBackEventHandler
    {
        public EligibleContainers()
        {

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            CWC.Button _SearchButton = Page.FindCamstarControl("SearchButton") as CWC.Button;
            CWC.ContainerList _ContainerStatus_ContainerName = Page.FindCamstarControl("ContainerStatus_ContainerName") as CWC.ContainerList;
            if (_ContainerStatus_ContainerName.IsEmpty)
            {
                _SearchButton.Enabled = false;
            }
            else
            {
                _SearchButton.Enabled = isSearchEnabled;
            }
        }
        protected override void OnPreLoad(object sender, EventArgs e)
        {
            base.OnPreLoad(sender, e);

            if (!Page.IsPostBack)
            {
                CWC.TextBox _QtyLessThen = Page.FindCamstarControl("QtyLessThan") as CWC.TextBox;
                CWC.TextBox _QtyGreaterThen = Page.FindCamstarControl("QtyGreaterThan") as CWC.TextBox;
                if (_QtyLessThen != null)
                {
                    _QtyLessThen.AutoPostBack = true;
                }
                if (_QtyGreaterThen != null)
                {
                    _QtyGreaterThen.AutoPostBack = true;
                }
            }
        }

        public virtual void _QtyGreaterThen_TextChanged(object sender, EventArgs e)
        {
            var labelCache = LabelCache.GetRuntimeCacheInstance();
            var label = labelCache.GetLabelByName("Lbl_InvalidQtyMore");

            CWC.ContainerList _ContainerStatus_ContainerName = Page.FindCamstarControl("ContainerStatus_ContainerName") as CWC.ContainerList;
            CWC.TextBox _QtyLessThen = Page.FindCamstarControl("QtyLessThan") as CWC.TextBox;
            CWC.TextBox _QtyGreaterThen = Page.FindCamstarControl("QtyGreaterThan") as CWC.TextBox;
            if (!System.Text.RegularExpressions.Regex.IsMatch(_QtyGreaterThen.TextControl.Text, @"^\d*\.?\d*$"))
            {
                _QtyGreaterThen.TextControl.BorderColor = System.Drawing.Color.Red;
                isSearchEnabled = false;
                DisplayMessage(new ResultStatus(label.Value, false));
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(_QtyLessThen.TextControl.Text, @"^\d*\.?\d*$") && !_ContainerStatus_ContainerName.IsEmpty)
            {
                isSearchEnabled = true;
                _QtyGreaterThen.TextControl.BorderColor = System.Drawing.Color.FromArgb(0, 223, 223, 223);
            }
            else
            {
                isSearchEnabled = false;
                _QtyGreaterThen.TextControl.BorderColor = System.Drawing.Color.FromArgb(0, 223, 223, 223);
            }
        }

        public virtual void _QtyLessThen_TextChanged(object sender, EventArgs e)
        {
            var labelCache = LabelCache.GetRuntimeCacheInstance();
            var label = labelCache.GetLabelByName("Lbl_InvalidQtyLess");

            CWC.ContainerList _ContainerStatus_ContainerName = Page.FindCamstarControl("ContainerStatus_ContainerName") as CWC.ContainerList;
            CWC.TextBox _QtyLessThen = Page.FindCamstarControl("QtyLessThan") as CWC.TextBox;
            CWC.TextBox _QtyGreaterThen = Page.FindCamstarControl("QtyGreaterThan") as CWC.TextBox;
            if (!System.Text.RegularExpressions.Regex.IsMatch(_QtyLessThen.TextControl.Text, @"^\d*\.?\d*$"))
            {
                _QtyLessThen.TextControl.BorderColor = System.Drawing.Color.Red;
                isSearchEnabled = false;
                DisplayMessage(new ResultStatus(label.Value, false));
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(_QtyGreaterThen.TextControl.Text, @"^\d*\.?\d*$") && !_ContainerStatus_ContainerName.IsEmpty)
            {
                isSearchEnabled = true;
                _QtyLessThen.TextControl.BorderColor = System.Drawing.Color.FromArgb(0, 223, 223, 223);
            }
            else
            {
                isSearchEnabled = false;
                _QtyLessThen.TextControl.BorderColor = System.Drawing.Color.FromArgb(0, 223, 223, 223);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //SelectionValuesBoundJGrid grid = Page.FindCamstarControl("EligibleContainersGrid") as SelectionValuesBoundJGrid;
            CWC.Label lbl = Page.FindCamstarControl("EligibleRulesText") as CWC.Label;
            lbl.Text = string.Format(lbl.Text, Page.PrimaryServiceType, "");
        }

        public virtual void Clear(object sender, EventArgs e)
        {
            Page.StatusBar.ClearMessage();

            CWC.CheckBox chkBox = Page.FindCamstarControl("RemoveSelectedContainers") as CWC.CheckBox;
            CWC.CheckBox chkSingleContainer = Page.FindCamstarControl("SingleContainerMismatchAllowed") as CWC.CheckBox;
            CWC.CheckBox chkIncludeIneligible = Page.FindCamstarControl("IncludeIneligibleContainers") as CWC.CheckBox;

            //SelectionValuesBoundJGrid grid = Page.FindCamstarControl("EligibleContainersGrid") as SelectionValuesBoundJGrid;

            //if (chkBox.CheckControl.Checked && grid.SelectedKeys != null)
            //{
            //    var containers = grid.Data as System.Data.DataTable;
            //    var headers = containers.Columns.OfType<DataColumn>();
            //    int idKey = containers.Columns.IndexOf(grid.Options.ColumnModelArray.Find(delegate(ColumnModel m) { return m.IsKey == true; }).ColumnName);
            //    var selectedContainers = from row in containers.Rows.OfType<DataRow>()
            //                             join key in grid.SelectedKeys on
            //                             row.ItemArray[idKey] equals key
            //                             select row;


            //    OM.RecordSet newReCSet = new OM.RecordSet()
            //    {
            //        Headers = headers.Select(c => new OM.Header { Name = c.ColumnName, TypeCode = TypeCode.String }).ToArray(),
            //        TotalCount = selectedContainers.Count(),
            //        Rows = selectedContainers.Select(r => new OM.Row
            //        {
            //            Values = r.ItemArray.Select(c => c.ToString()).ToArray()
            //        }).ToArray()
            //    };

            //    bool isSingleContainerChecked = (chkSingleContainer != null) ? chkSingleContainer.CheckControl.Checked : false;
            //    bool isIncludeIneligibleChecked = (chkIncludeIneligible != null) ? chkIncludeIneligible.CheckControl.Checked : false;

            //    ClearValues(new OM.EligibleContainersInquiry());
            //    grid.SetSelectionValues(newReCSet);

            //    if (chkSingleContainer != null)
            //        chkSingleContainer.CheckControl.Checked = isSingleContainerChecked;
            //    if (chkIncludeIneligible != null)
            //        chkIncludeIneligible.CheckControl.Checked = isIncludeIneligibleChecked;

            //    //keep items selected:
            //    grid.Options.gridComplete = "function (data) { $('.cbox').click(); }";
            //}
            //else
            //    grid.ClearData();
        }

        public virtual void Search(object sender, EventArgs e)
        {
            Page.StatusBar.ClearMessage();
            var ctrl = Page.FindCamstarControl("EligibleContainersGrid");
            ((IFieldSelection)ctrl).RetrieveList = RetrieveListType.OnPageLoad;
            Service.LoadSingleSelectionValues(ctrl as IFieldSelection);

            CWC.CheckBox chkBox = Page.FindCamstarControl("IncludeIneligibleContainers") as CWC.CheckBox;
            //SelectionValuesBoundJGrid grid = Page.FindCamstarControl("EligibleContainersGrid") as SelectionValuesBoundJGrid;
            //if (chkBox.CheckControl.Checked && grid.GridDataSource.Rows.Count > 0)
            //{
            //    StringBuilder ineligibleRowIDs = new StringBuilder("|");

            //    DataRow[] ineligibleRows = grid.GridDataSource.Select("EligibleContainer <> True");
            //    foreach (DataRow r in ineligibleRows)
            //        ineligibleRowIDs.Append(r["ContainerName"].ToString() + "|");//can use GetKeyColumn if this needs to be abstracted further

            //    //disable ineligible items:
            //    grid.Options.afterInsertRow = "function (rowid, rowdata, rowelem) { if(('" + ineligibleRowIDs.ToString() + "').indexOf('|'+rowid+'|')>-1){ $('input[id=\"jqg_" + grid.ClientID + "_'+rowid+'\"]').attr('disabled',true).parents('tr:first').addClass('ui-jqgrid-multiselect-disabled');} }";
            //}
        }

        public virtual void Add(object sender, EventArgs e)
        {
            Camstar.WebPortal.FormsFramework.Utilities.FrameworkSession fs = Camstar.WebPortal.FormsFramework.Utilities.FrameworkManagerUtil.GetFrameworkSession();

            Page.StatusBar.ClearMessage();

            var csCtrl = Page.FindCamstarControl("SingleContainer") as CWC.TextBox;
            CWC.Button addButton = Page.FindCamstarControl("AddButton") as CWC.Button;
            if (!csCtrl.IsEmpty)
            {
                var allowedMismatch = Page.FindCamstarControl("SingleContainerMismatchAllowed") as CWC.CheckBox;
                var containerCtrl = Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid;
                //var gridCtrl = Page.FindCamstarControl("EligibleContainersGrid") as SelectionValuesBoundJGrid;

                ////Checks for container, which is already in the grid
                //string containerName = csCtrl.Text;
                //var containers = gridCtrl.Data as System.Data.DataTable;
                //int nameColumnPosition = containers.Columns.IndexOf(gridCtrl.Options.ColumnModelArray.Find(delegate(ColumnModel m) { return m.ColumnName.Equals("ContainerName"); }).ColumnName);
                //var isDuplicate = from row in containers.Rows.OfType<DataRow>()
                //                  where (row.ItemArray[nameColumnPosition] as string).Equals(csCtrl.TextControl.Text)
                //                  select row;
                //if (isDuplicate.Count() == 0)
                //{
                //    OM.EligibleContainersInquiry data = new OM.EligibleContainersInquiry()
                //                    {
                //                        FilterForAssociate = true,
                //                        ParentContainer = containerCtrl.Data as OM.ContainerRef,
                //                        SingleContainer = new OM.ContainerRef(csCtrl.Data as string),
                //                        SingleContainerMismatchAllowed = (bool)allowedMismatch.Data
                //                    };

                //    EligibleContainersInquiry_Request request = new EligibleContainersInquiry_Request()
                //                    {
                //                        Info = new OM.EligibleContainersInquiry_Info() { EligibleContainer = FieldInfoUtil.RequestSelectionValue() }
                //                    };


                //    EligibleContainersInquiryService service = new EligibleContainersInquiryService(fs.CurrentUserProfile);

                //    EligibleContainersInquiry_Result result;
                //    OM.ResultStatus rs = service.GetEnvironment(data, request, out result);

                //    if (rs.IsSuccess)
                //    {
                //        OM.RecordSet recSet = result.Environment.EligibleContainer.SelectionValues;

                //        containers.Merge(recSet.GetAsDataTable(false, true));

                //        var headers = containers.Columns.OfType<DataColumn>();
                //        OM.RecordSet newReCSet = new OM.RecordSet()
                //        {
                //            Headers = headers.Select(c => new OM.Header { Name = c.ColumnName, TypeCode = TypeCode.String }).ToArray(),
                //            TotalCount = containers.Rows.Count,
                //            Rows = containers.Rows.OfType<DataRow>().Select(r => new OM.Row
                //            {

                //                Values = r.ItemArray.Select(c => c.ToString()).ToArray()
                //            }).ToArray()
                //        };

                //        gridCtrl.SetSelectionValues(newReCSet);
                //    }
                //    else
                //        DisplayMessage(rs);
                //}
                //else
                //{
                //    LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
                //    string errorMessage = string.Format(labelCache.GetLabelByName("DuplicateContainerMessage").Value, (string)isDuplicate.FirstOrDefault().ItemArray[nameColumnPosition]);
                //    DisplayMessage(new ResultStatus(errorMessage, false));
                //}
            }
        }

        public override void GetSelectionData(OM.Service serviceData)
        {
            if (serviceData is OM.EligibleContainersInquiry)
            {
                (serviceData as OM.EligibleContainersInquiry).FilterForAssociate = (Page.PrimaryServiceType == "Associate") ? true : (bool?)null;
                (serviceData as OM.EligibleContainersInquiry).FilterForCombine = (Page.PrimaryServiceType == "Combine") ? true : (bool?)null;
                (serviceData as OM.EligibleContainersInquiry).ParentContainer = (Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid).Data as OM.ContainerRef;
            }
            base.GetSelectionData(serviceData);
        }

        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);

            //SelectionValuesBoundJGrid grid = Page.FindCamstarControl("EligibleContainersGrid") as SelectionValuesBoundJGrid;
            //if (serviceData is OM.Associate)
            //{
            //    var data = serviceData as OM.Associate;
            //    if (data != null && grid.SelectedKeys != null)
            //    {
            //        var containers = grid.Data as System.Data.DataTable;
            //        if (containers != null)
            //        {
            //            var selectedContainers = grid.SelectedKeys.Select(containerName => new OM.ContainerRef(containerName)).ToList();
            //            (serviceData as OM.Associate).ChildContainers = selectedContainers.ToArray();
            //        }
            //    }
            //}
            //else if (serviceData is OM.Combine)
            //{
            //    var data = serviceData as OM.Combine;
            //    if (data != null && grid.SelectedKeys != null)
            //    {
            //        var gridData = grid.Data;
            //        var containers = grid.Data as System.Data.DataTable;
            //        if (containers != null)
            //        {
            //            var selectedContainers = grid.SelectedKeys.Select(containerName => new OM.ContainerRef(containerName)).ToList();
            //            List<CombineFromDetail> combineFromContainers = new List<CombineFromDetail>();
            //            foreach (ContainerRef container in selectedContainers)
            //            {
            //                CombineFromDetail combineFromParent = new CombineFromDetail();
            //                ArrayList combineFromChildren = new ArrayList();
            //                combineFromParent.FromContainer = container;
            //                //Implement after adding the checkbox column to JGrid
            //                //combineFromParent.CloseWhenEmpty=
            //                //IMplement after adding child containers grid
            //                //combineFromParent.ChildContainers=
            //                combineFromParent.CombineAllQty = true;
            //                combineFromContainers.Add(combineFromParent);
            //            }
            //            (serviceData as OM.Combine).FromContainerDetails = combineFromContainers.ToArray();
            //        }
            //    }
            //}
        }

        public virtual void ClearGrid()
        {
            //SelectionValuesBoundJGrid grid = Page.FindCamstarControl("EligibleContainersGrid") as SelectionValuesBoundJGrid;
            //if (grid != null)
            //    grid.ClearData();
        }

        public virtual void RaisePostBackEvent(string eventArgument)
        {

        }

        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);
            if (status.IsSuccess)
                ClearGrid();
        }
        public override void ClearValues(Service serviceData)
        {
            base.ClearValues(serviceData);
            ClearGrid();
        }
        private bool isSearchEnabled = true; //Used to pass values from QtyChanged event handlers
    }
}
