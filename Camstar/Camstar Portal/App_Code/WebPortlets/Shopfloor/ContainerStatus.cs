// Copyright Siemens 2023  
using System;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.WCFUtilities;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{

    public class ContainerStatus : MatrixWebPart
    {
        #region Controls

        protected virtual JQTabContainer StatusTabs
        {
            get
            {
                return Page.FindCamstarControl("ContainerStatus_StatusTabs") as JQTabContainer;
            }
        }
        #endregion

        #region Protected Functions

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (StatusTabs != null)
                StatusTabs.LoadAllTabs = true;
        }

        public override void ChildPostExecute(ResultStatus status, Service serviceData)
        {
            base.ChildPostExecute(status, serviceData);

            // Update current container status in case of changes from child form.  Currently, only 3 controls
            // need to be updated (TimersGrid, ActiveTimer, and DocumentsViewControl
            UpdateCurrentContainerStatus();
        }

        protected virtual void UpdateCurrentContainerStatus()
        {
            var cdoSvc = Page.PrimaryServiceType;
            if (!string.IsNullOrEmpty(cdoSvc))
            {
                var service = new WSDataCreator().CreateService(cdoSvc, FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);

                if (service is IContainerTxnBase)
                {
                    var serviceData = CreateServiceData(PrimaryServiceType);
                    var serviceInfo = CreateServiceInfo(PrimaryServiceType);
                    foreach (var webPartName in InnerWebPartNames)
                    {
                        var wp = Page.Manager.GetWebPartByName(webPartName) as WebPartBase;
                        if (wp != null)
                            wp.RequestValues(serviceInfo, serviceData);
                    }
                    RequestValues(serviceInfo, serviceData);

                    var cdo = new WCFObject(serviceData);
                    var req = WCFObject.CreateObject(cdoSvc + "_Request") as ICreator;
                    req.SetValue("Info", serviceInfo);
                    Result result;

                    var resStatus = (service as IContainerTxnBase).Load(serviceData, req as Request, out result);
                    if (resStatus.IsSuccess)
                    {
                        foreach (var webPartName in InnerWebPartNames)
                        {
                            var wp = Page.Manager.GetWebPartByName(webPartName) as WebPartBase;
                            if (wp != null)
                                wp.DisplayValues(result.Value as Service);
                        }
                        DisplayValues(result.Value as Service);
                    }
                }
            }
        }
        #endregion

        protected virtual string[] InnerWebPartNames
        {
            get { return _innerWebParts; }
        }

        private string[] _innerWebParts = { "ContainerStatus_WorkflowWP", "ContainerStatusDetails", "ContainerStatus_AttributesWP", "TimersView_WP", "ContainerStatus_DocumentsWP" };
    }


}

