// Copyright Siemens 2023  
using System;
using System.Linq;
using FF = Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using PERS = Camstar.WebPortal.Personalization;
using Camstar.WCF.Services;


namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class ContainerDefect : MatrixWebPart
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            currentContainer.DataChanged += new EventHandler(CurrentContainer_DataChanged);
        }
        protected virtual void CurrentContainer_DataChanged(object sender, EventArgs e)
        {
            OM.ContainerRef container = (OM.ContainerRef)currentContainer.Data;
            var dcContainer = (Page.FindCamstarControl("DCContainer") as CWC.ContainerList);
            dcContainer.Data = null;
            if (container == null)
                Page.ClearValues();
            else
                ReloadMaterialIssueGrid();
            dcContainer.Data = container;
        }
        protected virtual void ReloadMaterialIssueGrid()
        {
            Page.Service.ExecuteFunction("ContainerDefect", "GetActuals");
        }
        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);
            JQDataGrid grid = Page.FindCamstarControl("DefectList") as JQDataGrid;
            OM.ContainerDefect svc = (serviceData as OM.ContainerDefect);
            if (svc != null)
            {
                if (grid != null && grid.Data != null)
                {
                    var items = (grid.Data as OM.ContainerDefectDetail[]).Select(i =>
                             new OM.ContainerDefectDetail()
                             {
                                 ListItemAction = OM.ListItemAction.Add,
                                 ReasonCode = i.ReasonCode,
                                 DefectCount = i.DefectCount,
                                 Comment = i.Comment
                             }).Where(n => n.ReasonCode != null);

                    if ((serviceData as OM.ContainerDefect).ChargeToStep != null)
                        (serviceData as OM.ContainerDefect).ChargeToStep.Parent = new OM.BaseObjectRef(GetContainerWorkflow());
                    (serviceData as OM.ContainerDefect).ServiceDetails = items.ToArray();
                }
            }
        }
        
        protected virtual string GetContainerWorkflow()
        {
            OM.ViewContainer containerStatusData = new OM.ViewContainer();
            OM.ViewContainer_Info containerStatusInfo = new OM.ViewContainer_Info
            {
                ContainerStatusDetails = new OM.CurrentContainerStatus_Info()
            };
            containerStatusInfo.ContainerStatusDetails.Workflow = FieldInfoUtil.RequestValue();
            containerStatusData.ContainerStatusDetails = new OM.CurrentContainerStatus();
            containerStatusData.Container = currentContainer.Data as OM.ContainerRef;

            ViewContainer_Request request = new ViewContainer_Request
            {
                Info = containerStatusInfo
            };
            ViewContainer_Result result = new ViewContainer_Result();
            ViewContainerService service = new ViewContainerService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);

            OM.ResultStatus resultStatus = service.GetEnvironment(containerStatusData, request, out result);
            if (resultStatus.IsSuccess)
            {
                return result.Value.ContainerStatusDetails.Workflow.ID;
            }
            else
                DisplayMessage(resultStatus);
            return null;
        }

        protected virtual ContainerListGrid currentContainer
        {
            get
            {
                return Page.FindCamstarControl("HiddenSelectedContainer") as ContainerListGrid;
            }
        }

    }
}
