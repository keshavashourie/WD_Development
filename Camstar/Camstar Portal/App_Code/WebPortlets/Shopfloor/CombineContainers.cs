// Copyright Siemens 2023  
using System;
using System.Linq;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using System.Collections.Generic;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class CombineContainers : MatrixWebPart
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.OnGetSelectionData += Page_OnGetSelectionData;
        }

        protected virtual void Page_OnGetSelectionData(object sender, FormsFramework.ServiceDataEventArgs e)
        {
            if (e.Data is OM.Combine)
                (e.Data as OM.Combine).CombineType = 1;
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ContainerStatusContainer.DataChanged += new EventHandler(ContainerChanged);
            EligibleContainerGrid.GridContext.RowExpanded += new JQGridEventHandler(GridContext_RowExpanded);
            EligibleContainerGrid.GridContext.GridReloading += new JQGridEventHandler(GridContext_GridReloading);
        }

        protected virtual void ContainerChanged(object sender, EventArgs e)
        {
            if (ContainerStatusContainer.IsEmpty)
            {
                ChildContainersGrid.ClearData();
                EligibleContainerGrid.ClearData();
            }
            CombineContainerControl.ClearData();
        }


        public virtual ResponseData GridContext_GridReloading(object sender, JQGridEventArgs args)
        {
            var targetContainer = Page.FindCamstarControl("HiddenSelectedContainer") as ContainerListGrid;

            Combine data = new Combine()
                                {
                                    CombineType = 1,
                                    Container = targetContainer.Data as OM.ContainerRef
                                };
            Combine_Info info = new Combine_Info()
                                {
                                    FromContainerDetails = new CombineFromDetail_Info()
                                    {
                                        ChildContainers = FieldInfoUtil.RequestValue(),
                                        CloseWhenEmpty = FieldInfoUtil.RequestValue(),
                                        CombineAllQty = FieldInfoUtil.RequestValue(),
                                        FromContainer = FieldInfoUtil.RequestValue(),
                                        Qty = FieldInfoUtil.RequestValue(),
                                        UOMName = FieldInfoUtil.RequestValue()
                                    }
                                };

            object ret = null;
            ResultStatus res = Service.GetLoadValues(data as Update, info as Info, ref ret);
            if (res.IsSuccess)
            {
                CombineFromDetail[] details = (ret as Combine).FromContainerDetails;
                if (details != null)
                {
                    EligibleContainerGrid.ClearData();
                    ChildContainersGrid.ClearData();
                    EligibleContainerGrid.Data = details;
                    (EligibleContainerGrid.GridContext as BoundContext).LoadData();
                }
            }

            return null;
        }

        public virtual ResponseData GridContext_RowExpanded(object sender, JQGridEventArgs args)
        {
            ResponseData rd = args.Response;

            GridContext context = EligibleContainerGrid.GridContext.GetSubgridRowContext(args.State.RowID);
            if (context != null)
            {
                string containerToSelect = Page.DataContract.GetValueByName<string>("SelectedContainer");
                if (!string.IsNullOrEmpty(containerToSelect))
                {
                    SelValGridContext childContext = EligibleContainerGrid.GridContext.GetSubgridRowContext(args.State.RowID) as SelValGridContext;
                    if (childContext != null)
                    {
                        string rowId = childContext.GetRowIdByCellValue("Name", containerToSelect);
                        if (!string.IsNullOrEmpty(rowId))
                        {
                            childContext.SelectRow(rowId, true);
                            Page.DataContract.SetValueByName("SelectedContainer", string.Empty);
                            var st = args.State;
                            st.CurrentPage = childContext.CurrentPage;
                            st.ContextID = childContext.ContextID;
                            rd = context.Reload(st);
                        }
                    }
                }
            }

            return rd;
        }

        public virtual void CombineContainerControl_DataChanged(object sender, EventArgs e)
        {
            var targetContainer = Page.FindCamstarControl("HiddenSelectedContainer") as ContainerListGrid;
            string contName = CombineContainerControl.Data as string;
            if (string.IsNullOrEmpty(contName))
                return;

            Page.DataContract.SetValueByName("SelectedContainer", string.Empty);

            var data = new OM.Combine()
                                {
                                    EligibleContainersInquiry = new OM.EligibleContainersInquiry
                                                                {
                                                                    CombineType = 1,
                                                                    EligibleContainer = new OM.ContainerRef(contName),
                                                                },
                                    Container = targetContainer.Data as OM.ContainerRef
                                };

            var request = new Combine_Request()
                            {
                                Info = new Combine_Info()
                                        {
                                            EligibleContainersInquiry = new OM.EligibleContainersInquiry_Info { EligibleContainer = FieldInfoUtil.RequestSelectionValue() }
                                        }
                            };

            var fs = FrameworkManagerUtil.GetFrameworkSession();
            var service = new CombineService(fs.CurrentUserProfile);
            Combine_Result result;
            OM.ResultStatus rs = service.GetEnvironment(data, request, out result);
            if (rs.IsSuccess)
            {
                RecordSet recordSet = result.Environment.EligibleContainersInquiry.EligibleContainer.SelectionValues;
                if (recordSet != null && recordSet.Rows != null && recordSet.Rows.Length > 0)
                {
                    CombineFromDetail[] details = EligibleContainerGrid.Data as CombineFromDetail[];
                    if (details != null)
                    {
                        for (int i = 0; i < details.Length; i++)
                        {
                            string row = (EligibleContainerGrid.GridContext as BoundContext).MakeAutoRowId(i);
                            CombineFromDetail detail = EligibleContainerGrid.GridContext.GetItem(row) as CombineFromDetail;
                            if (recordSet.Rows[0].Values[0] == detail.FromContainer.Name)
                            {
                                EligibleContainerGrid.GridContext.ExpandedRowIDs.Add(row);
                                EligibleContainerGrid.GridContext.AdjustCurrentPage(row);
                                Page.DataContract.SetValueByName("SelectedContainer", contName);
                            }
                        }
                    }
                }
                CombineContainerControl.Data = null;
            }
            else
                DisplayMessage(rs);
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            OM.Combine svc = serviceData as OM.Combine;
            if (svc != null)
            {
                List<CombineFromDetail> list = new List<CombineFromDetail>();
                CombineFromDetail[] details = EligibleContainerGrid.Data as CombineFromDetail[];
                for (int i = 0; i < details.Length; i++)
                {
                    string row = (EligibleContainerGrid.GridContext as BoundContext).MakeAutoRowId(i);
                    GridContext context = EligibleContainerGrid.GridContext.GetSubgridRowContext(row);
                    if (context != null)
                    {
                        var selItems = context.GetSelectedItems(false);
                        int selCount = context.GetSelectedCount();
                        if (selCount > 0)
                        {
                            CombineFromDetail detail = EligibleContainerGrid.GridContext.GetItem(row) as CombineFromDetail;
                            int count = context.DataWindow != null ? context.DataWindow.Rows.Count : 0;
                            bool combineAllQty = selCount == count;
                            detail.CombineAllQty = combineAllQty;
                            if (!combineAllQty)
                            {
                                detail.ChildContainers = selItems
                                    .Select(it => new ContainerRef() { ID = (it as System.Data.DataRow)["InstanceId"] as string })
                                    .ToArray();
                            }
                            list.Add(detail);
                        }
                    }
                }

                (serviceData as OM.Combine).FromContainerDetails = list.Select(n => new CombineFromDetail()
                {
                    ListItemAction = ListItemAction.Add,
                    FromContainer = new ContainerRef() { ID = n.FromContainer.ID },
                    CombineAllQty = n.CombineAllQty,
                    CloseWhenEmpty = n.CloseWhenEmpty,
                    ChildContainers = n.CombineAllQty.Value ? null : n.ChildContainers,
                }).ToArray();


            }
        }
        protected virtual CWC.TextBox CombineContainerControl
        {
            get { return Page.FindCamstarControl("CombineContainer") as CWC.TextBox; }
        }

        protected virtual JQDataGrid EligibleContainerGrid
        {
            get { return Page.FindCamstarControl("EligibleContainersGrid") as JQDataGrid; }
        }

        protected virtual JQDataGrid ChildContainersGrid
        {
            get { return Page.FindCamstarControl("ChildContainers") as JQDataGrid; }
        }

        protected virtual ContainerListGrid ContainerStatusContainer
        {
            get { return Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid; }
        }
    }
}
