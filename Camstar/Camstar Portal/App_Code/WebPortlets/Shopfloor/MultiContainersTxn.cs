// Copyright Siemens 2025  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WebPortal.WebPortlets;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Data;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.Utilities;
using System.Web.UI;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.PortalFramework;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    enum Status
    {
        NotExecuted,
        Failed,
        Success
    };

    public class MultiContainersTxn : MatrixWebPart
    {
        #region Properties

        bool cleanGlobalWarning = true;
        List<MultiContainerTxnItem> allContainers = new List<MultiContainerTxnItem>();

        protected virtual TextBox ContainerTextBox
        {
            get
            {
                return Page.FindCamstarControl("MultiContainer_Container") as TextBox;
            }
        }

        protected virtual JQDataGrid ContainerGrid
        {
            get
            {
                return Page.FindCamstarControl("MultiContainer_Containers") as JQDataGrid;
            }
        }

        protected virtual CheckBox HideSuccessfullTransactionsCheckbox
        {
            get
            {
                return Page.FindCamstarControl("HideSuccessfullTransactions") as CheckBox;
            }
        }

        #endregion

        #region Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsFloatingFrame)
                Page.DataContract.SetValueByName("Containers", null);

            string[] callerContainers = GetCallerContainers();

            if (callerContainers != null && callerContainers.Length > 0 && Page.DataContract.GetValueByName("MultiContainer_AllContainers") == null)
            {
                AddNewContainer(callerContainers.OfType<string>().ToArray());
            }

            if (Page.DataContract.GetValueByName("MultiContainer_AllContainers") != null)
                allContainers = Page.DataContract.GetValueByName("MultiContainer_AllContainers") as List<MultiContainerTxnItem>;

            if (ContainerGrid.GridContext as ItemDataContext != null)
            {
                (ContainerGrid.GridContext as ItemDataContext).GetRowSnapItem += new ItemDataContext.GetRowSnapItemHandler(MultiContainer_GetRowSnapItem);
                (ContainerGrid.GridContext as ItemDataContext).RowDeleting += new JQGridEventHandler(MultiContainersTxn_RowDeleting);
            }
        }

        public virtual void HideResourceField()
        {
            JQDataGrid grid = ContainerGrid;
            bool isFound = false;

            if (grid.GridContext.SelectedRowIDs != null && grid.GridContext.SelectedRowIDs.Count > 0)
            {
                var initialItem = grid.GridContext.GetItem(grid.GridContext.SelectedRowIDs[0]) as ContainerSearchDetail;

                foreach (var item in grid.GridContext.SelectedRowIDs)
                {
                    var obj = grid.GridContext.GetItem(item) as ContainerSearchDetail;
                    if ((initialItem != null && obj != null) && initialItem.Resource != obj.Resource)
                    {
                        isFound = true;
                        break;
                    }
                }

                NamedObject resourceField = Page.FindCamstarControl("MoveNonStd_Resource") as NamedObject;
                if (resourceField != null && initialItem != null)
                {
                    resourceField.Data = initialItem.Resource;
                    resourceField.Hidden = isFound;
                }
            }
            else
            {
                NamedObject resourceField = Page.FindCamstarControl("MoveNonStd_Resource") as NamedObject;
                if (resourceField != null)
                {
                    resourceField.ClearData();
                    resourceField.Hidden = false;
                }
            }
        }

        protected virtual ResponseData MultiContainersTxn_RowDeleting(object sender, JQGridEventArgs args)
        {
            foreach (var item in args.Context.SelectedRowIDs)
            {
                var obj = args.Context.GetItem(item) as ContainerSearchDetail;
                allContainers.Remove(allContainers.Where(n => n.Detail == obj).FirstOrDefault());
            }
            return null;
        }

        public override void ClearValues(Service serviceData)
        {
            allContainers.Clear();

            HideSuccessfullTransactionsCheckbox.Data = false;
            base.ClearValues(serviceData);

            string[] callerContainers = GetCallerContainers();
            if (callerContainers != null && callerContainers.Length > 0)
                AddNewContainer(callerContainers.OfType<string>().ToArray());
        }

        protected virtual void MultiContainer_GetRowSnapItem(object item, IEnumerable<DataColumn> dataColumns, DataRow row)
        {
            if (item is ContainerSearchDetail && allContainers != null && cleanGlobalWarning)
            {
                var statusCol = dataColumns.Where(n => n.ColumnName == "Status").FirstOrDefault();
                var msgCol = dataColumns.Where(n => n.ColumnName == "StatusMsg").FirstOrDefault();
                string containerName = (item as ContainerSearchDetail).ContainerName.Value;

                MultiContainerTxnItem txnItem = allContainers.Where(n => n.Detail.ContainerName == containerName).FirstOrDefault();

                if (txnItem != null)
                {
                    LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                    if (txnItem.Status != Status.NotExecuted)
                    {
                        string statusInfo = txnItem.Status == Status.Success ? string.Format(mkDivPattern, mkSuccessfulClass, labelCache.GetLabelByName("Lbl_Passed").Value, ImageConstants.SuccessStatus) :
                                                                                         string.Format(mkDivPattern, mkFailedClass, labelCache.GetLabelByName("Lbl_Failed").Value, ImageConstants.FailedStatus);

                        row[statusCol] = statusInfo;
                        row[msgCol] = txnItem.StatusMsg;
                    }
                }
            }
        }

        public virtual void ContainerFieldChanged(object sender, EventArgs e)
        {
            TextBox containerControl = ContainerTextBox;
            if (!containerControl.IsEmpty)
            {
                string containerName = (string)containerControl.Data;

                var container = allContainers.Where(n => n.Detail.ContainerName == containerName);
                if (container == null || container.Count() == 0)
                    AddNewContainer(new string[] { containerName });

                ContainerTextBox.ClearData();
            }

            HideSuccessfullTransactions();
            HideResourceField();
        }

        protected virtual string[] GetCallerContainers()
        {
            string[] containers = null;
            //Using for Quality
            var activeTab = Page.DataContract.GetValueByName("SelectedTabsForAction");

            var callerContainers = ((activeTab == null) || (activeTab.Equals("Affected Material"))) ? Page.DataContract.GetValueByName<Array>("Containers") : Page.DataContract.GetValueByName<Array>("DispositionContainers");
            if (callerContainers != null)
            {
                if (callerContainers.GetType() == typeof(EventLotDetail[]))
                    containers = callerContainers.OfType<EventLotDetail>().Where(n => n.IsContainer.Value).Select(n => n.Lot.Value).Distinct().ToArray();
                else
                    containers = callerContainers.OfType<string>().ToArray();
            }

            return containers;
        }

        protected virtual void AddNewContainer(string[] containerNames)
        {
            var fs = FrameworkManagerUtil.GetFrameworkSession();
            WSDataCreator creator = new WSDataCreator();

            IShopFloorBase service = creator.CreateService(PrimaryServiceType, fs.CurrentUserProfile) as IShopFloorBase;

            Service data = Page.CreateServiceData(PrimaryServiceType);
            Info info = Page.CreateServiceInfo(PrimaryServiceType);
            Request request = creator.CreateObject(PrimaryServiceType + "_Request") as Request;
            request.Info = info;
            Result result = creator.CreateObject(PrimaryServiceType + "_Result") as Result;

            WCFObject cdoObj = new WCFObject(data);
            cdoObj.SetValue(".Containers", containerNames.Select(c => new ContainerRef(c)).ToArray());

            WCFObject infoObj = new WCFObject(info);
            infoObj.SetValue(".ContainerSearchDetails", new ContainerSearchDetail_Info
                                                            {
                                                                ContainerName = FieldInfoUtil.RequestValue(),
                                                                ContainerLevelName = FieldInfoUtil.RequestValue(),
                                                                Qty = FieldInfoUtil.RequestValue(),
                                                                Product = FieldInfoUtil.RequestValue(),
                                                                OperationName = FieldInfoUtil.RequestValue(),
                                                                Resource = FieldInfoUtil.RequestValue(),
                                                                InRework = FieldInfoUtil.RequestValue()
                                                            });

            ResultStatus status = service.Load(data, request, out result);
            if (status.IsSuccess)
            {
                WCFObject serviceResult = new WCFObject(result.Value);
                ContainerSearchDetail[] newDetails = serviceResult.GetValue(".ContainerSearchDetails") as ContainerSearchDetail[];

                Array.ForEach(newDetails, n => allContainers.Add(new MultiContainerTxnItem() { Status = Status.NotExecuted, StatusMsg = String.Empty, Detail = n }));
                Page.DataContract.SetValueByName("MultiContainer_AllContainers", allContainers);

                JQDataGrid grid = ContainerGrid;
                grid.Data = allContainers.Select(n => n.Detail).ToArray();

                Array.ForEach(containerNames, n =>
                                            {
                                                (grid.GridContext as BoundContext).SelectRow(n as string, true);

                                                int index = allContainers.IndexOf(allContainers.Where(j => j.Detail.ContainerName == n).FirstOrDefault());
                                                grid.GridContext.AdjustCurrentPage(index);

                                            });
        }
            else
                DisplayMessage(status);
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            Page.ProcessingContext.TransactionContext.FieldExpression = ".Containers";

            List<string> selectedIDs = ContainerGrid.GridContext.SelectedRowIDs;
            if (selectedIDs != null && selectedIDs.Count > 0)
                (serviceData as ContainersTxn).Containers = selectedIDs.Select(id => new ContainerRef(id)).ToArray();

            UIComponentDataContract dataContract = Page.PortalContext.DataContract;
             if (dataContract != null)
             {
                 var allEsigDetails =
                     (Tuple<ESigServiceDetail[], ESigProcessTimerServiceDetail[]>)dataContract.GetValueByName("ESigCaptureDetailsDM");
                 ESigServiceCaptureWrapper[] gridCaptures =
                     (ESigServiceCaptureWrapper[])dataContract.GetValueByName("ESigCaptureDM");
                 if (gridCaptures != null && allEsigDetails.Item1 != null)
                     foreach (var item in allEsigDetails.Item1)
                     {
                         if (item.CaptureDetails == null)
                         {
                             item.CaptureDetails = gridCaptures.Where(c => c.RequirementID == item.ESigReqDetail.ID).Select(c => c.Capture).ToArray();
                         }
                     }
                 dataContract.SetValueByName("ESigCaptureDetailsDM", allEsigDetails);
             }

        }
        public override bool PreExecute(Info serviceInfo, Service serviceData)
        {
            base.PreExecute(serviceInfo, serviceData);

            return true;
        }

        public override void PostExecute(ResultStatus status, Service serviceData)
        {
            base.PostExecute(status, serviceData);

            List<string> selectedIDs = new List<string>();
            if (ContainerGrid.GridContext.SelectedRowIDs != null)
                selectedIDs.AddRange(ContainerGrid.GridContext.SelectedRowIDs);

            if (selectedIDs != null && selectedIDs.Count > 0)
            {
                selectedIDs.ForEach(n =>
                {
                    allContainers.ForEach(i =>
                    {
                        if (i.Detail.ContainerName == n)
                        {
                            i.Status = Status.Success;
                            i.StatusMsg = string.Empty;
                        }
                    });
                });
            }

            if (Page.ProcessingContext.TransactionContext.Statuses != null)
            {
                foreach (var txnStatus in Page.ProcessingContext.TransactionContext.Statuses)
                {
                    if (!txnStatus.IsSuccess)
                    {
                        string containerName = txnStatus.ExceptionData.ExceptionParameter("ContainerName");
                        string statusMsg = txnStatus.ExceptionData.ExceptionParameter("SystemMsg");
                        if (!string.IsNullOrWhiteSpace(containerName))
                        {
                            allContainers.ForEach(n =>
                                                        {
                                                            if (n.Detail.ContainerName == containerName)
                                                            {
                                                                n.Status = txnStatus.IsSuccess ? Status.Success : Status.Failed;
                                                                n.StatusMsg = statusMsg;
                                                            }
                                                        });
                        }
                        else
                            cleanGlobalWarning = false;

                        if (Page.ProcessingContext.TransactionContext.Mode != FormsFramework.TransactionMode.Individual)
                            selectedIDs.ForEach(n =>
                            {
                                allContainers.ForEach(i =>
                                {
                                    if (i.Detail.ContainerName == n)
                                        i.Status = Status.Failed;
                                });
                            });
                    }
                }
            }
            else
                cleanGlobalWarning = false;

            if (cleanGlobalWarning)
            {
                if (ContainerGrid.GridContext.SelectedRowIDs != null)
                {
                    ContainerGrid.GridContext.SelectedRowIDs.Clear();
                    selectedIDs.ForEach(n =>
                    {
                        allContainers.ForEach(i =>
                        {
                            if (i.Status == Status.Failed && n == i.Detail.ContainerName)
                                ContainerGrid.GridContext.SelectedRowIDs.Add(i.Detail.ContainerName.Value);
                        });
                    });
                }

                if(PrimaryServiceType == "MoveNonStds")
                    ContainerGrid.OriginalData = null;
                else
                    ContainerGrid.OriginalData = ContainerGrid.Data;

                status.Message = string.Empty;
                status.IsSuccess = true;
                HideSuccessfullTransactions();
                HideResourceField();
            }
        }

        public virtual void HideSuccessfullTransactions()
        {
            JQDataGrid grid = ContainerGrid;
            if (allContainers != null && allContainers.Count > 0)
            {
                if (HideSuccessfullTransactionsCheckbox.IsChecked)
                {
                    var successfullTxns = allContainers.Where(n => n.Status == Status.Success);
                    grid.Data = allContainers.Except(successfullTxns).Select(n => n.Detail).ToArray();
                }
                else
                    grid.Data = allContainers.Select(n => n.Detail).ToArray();
            }
        }

        public override void WebPartCustomAction(object sender, CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);

            e.IsSubmitted = false;

            ResultStatus status = Page.Service.Submit(PrimaryServiceType, false);
            e.Result = status;
        }

        #endregion

        protected const string mkDivPattern = "<DIV cellClass='{0}'>{1} &nbsp;<img src='{2}'/></DIV>";
        protected const string mkFailedClass = "ui-jqgrid-cell-failed";
        protected const string mkSuccessfulClass = "ui-jqgrid-cell-success";
    }

    class MultiContainerTxnItem
    {
        public Status Status;
        public string StatusMsg;
        public ContainerSearchDetail Detail;
    }
}
