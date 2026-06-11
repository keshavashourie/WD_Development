// Copyright Siemens 2023  
using System;
using System.Linq;
using System.Web;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.Utilities;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class ReversalTxn : MatrixWebPart
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var historyGrid = (Page.FindCamstarControl("HistoryGrid")) as JQDataGrid;

            if (IsResponsive)
            {
                if (historyGrid.Settings.Automation == null)
                    historyGrid.Settings.Automation = new GridAutomation();

                historyGrid.Settings.Automation.ShrinkColumnWidthToFit = false;

            }

            var container = (Page.FindCamstarControl("HiddenSelectedContainer") as ContainerList);
            container.DataChanged += new EventHandler(container_DataChanged);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            var historyGrid = (Page.FindCamstarControl("HistoryGrid")) as JQDataGrid;

            if (historyGrid.Data != null)
                historyGrid.BoundContext.Fields["Dummy"].Visible = false;
        }

        protected virtual void container_DataChanged(object sender, EventArgs e)
        {
            var container = (Page.FindCamstarControl("HiddenSelectedContainer") as ContainerList);

            var viewList = (Page.FindCamstarControl("ViewList") as NamedObject);
            if (!viewList.IsEmpty)
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
                ContainerTxnRevService service = new ContainerTxnRevService(session.CurrentUserProfile);

                ContainerTxnRev data = new ContainerTxnRev() { Container = container.Data as ContainerRef };
                ContainerTxnRev_Request request = new ContainerTxnRev_Request();
                request.Info = new ContainerTxnRev_Info()
                {
                    HistoryCDOs = new HistoryMainline_Info()
                    {
                        Container = FieldInfoUtil.RequestValue(),
                        DisplayName = FieldInfoUtil.RequestValue(),
                        Employee = FieldInfoUtil.RequestValue(),
                        TxnDate = FieldInfoUtil.RequestValue()
                    }
                };
                ContainerTxnRev_Result result = new ContainerTxnRev_Result();

                ResultStatus rs = service.GetLastTxnHistory(data, request, out result);
                if (rs.IsSuccess)
                    DisplayValues(result.Value);
            }

            var dcContainer = (Page.FindCamstarControl("DCContainer") as ContainerList);

            dcContainer.Data = container.Data;
        }

        public override void RequestValues(Info serviceInfo, Service serviceData)
        {
            base.RequestValues(serviceInfo, serviceData);
            var info = serviceInfo as ContainerTxnRev_Info;
            if (info != null && info.HistoryCDOs != null)
            {
                info.HistoryCDOs.RequestValue = false;
                info.HistoryCDOs.Container = FieldInfoUtil.RequestValue();
                info.HistoryCDOs.DisplayName = FieldInfoUtil.RequestValue();
                info.HistoryCDOs.Employee = FieldInfoUtil.RequestValue();
                info.HistoryCDOs.TxnDate = FieldInfoUtil.RequestValue();
            }
        }

        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);

            if (!status.IsSuccess)
            {
                return;
            }
            //Reversing a Start transaction delete the container, so we have to set the ReloadValues on submit action to false and clears the values on the page
            var actions = Page.ActionDispatcher.PageActions();
            var cache = FrameworkManagerUtil.GetCDOCache(System.Web.HttpContext.Current.Session);
            var historyGrid = (Page.FindCamstarControl("HistoryGrid")) as JQDataGrid;

            if (historyGrid.Data == null)
            {
                return;
            }

            serviceData = null;
            var selValGridContext = historyGrid.GridContext as SelValGridContext;

            if (selValGridContext == null)
            {
                return;
            }

            var test = historyGrid.BoundContext.SelectedItem;
            var baseTxnType = selValGridContext.GetCachedGridCell(historyGrid.BoundContext.SelectedRowID,
                historyGrid.BoundContext.DataWindow.Columns["TxnType"].ColumnName);

            if (baseTxnType != null)
            {
                bool isStart = baseTxnType.ToString() == cache.GetCDONameByCDOID(2400) && actions != null;
                foreach (var submitAction in actions.OfType<SubmitAction>())
                {
                    submitAction.ReloadValues = !isStart;
                }

                if (isStart)
                {
                    Page.ClearValues();
                }
            }

            historyGrid.ClearData();

        }
    }
}
