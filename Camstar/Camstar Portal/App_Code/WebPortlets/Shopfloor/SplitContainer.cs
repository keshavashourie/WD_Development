// Copyright Siemens 2023  
using System.Data;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Web;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SplitContainer : MatrixWebPart
    {

        protected virtual ContainerListGrid ContainersGrid
        {
            get { return Page.FindCamstarControl("ContainerStatus_ContainerName") as ContainerListGrid; }
        }

        protected virtual CWC.Container HiddenSelectedContainer
        {
            get { return Page.FindCamstarControl("HiddenSelectedContainer") as CWC.Container; }
        }

        protected virtual CWC.CheckBox AutoNumber
        {
            get { return Page.FindCamstarControl("Split_AutoNumber") as CWC.CheckBox; }
        }

        protected virtual CWC.NamedObject NumberingRule
        {
            get { return Page.FindCamstarControl("Split_NumberingRule") as CWC.NamedObject; }
        }

        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);
            var data = serviceData as OM.Split;
            if (data != null)
                data.ChildContainers = GetSelectedChildContainers();
        }

        public override bool PreExecute(OM.Info serviceInfo, OM.Service serviceData)
        {
            bool status = base.PreExecute(serviceInfo, serviceData);

            if (AutoNumber.IsChecked && NumberingRule.Data == null)
            {
                status = false;
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                Page.DisplayWarning(labelCache.GetLabelByName("Start_NoNumberingRuleDefined").Value);
            }

            return status;
        }


        protected override void OnPreRender(System.EventArgs e)
        {
            base.OnPreRender(e);
            if (IsFloatPage)
                Page.DataContract.SetValueByName("SelectedContainersDM", GetSelectedChildContainers());
        }
        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            //Using for Quality
            var activeTab = Page.DataContract.GetValueByName("SelectedTabsForAction");
            if (IsFloatPage && activeTab != null && activeTab.Equals("Affected Material"))
            {
                var containerName = Page.DataContract.GetValueByName("SplitOrScrapContainer");
                ContainersGrid.Data = containerName;
            }
            if (IsFloatPage && activeTab != null && activeTab.Equals("Disposition"))
            {
                var containerName = Page.DataContract.GetValueByName("DispositionSplitOrScrapContainer");
                ContainersGrid.Data = containerName;
            }
            if (!Page.IsPostBack && ContainersGrid.Data != null)
            {
                ChildContainersToSplit.Action_Reload("1");
                HiddenSelectedContainer.LoadOrClearDependentValues();                
            }
        }

        protected virtual OM.ContainerRef[] GetSelectedChildContainers()
        {
            OM.ContainerRef[] retVal = null;
            var containers = ChildContainersToSplit.Data as DataTable;
            if (containers != null && ChildContainersToSplit.GridContext.SelectedRowIDs != null && ChildContainersToSplit.GridContext.SelectedRowIDs.Count > 0)
            {
                System.Collections.Generic.List<Camstar.WCF.ObjectStack.ContainerRef> selectedContainers = new System.Collections.Generic.List<OM.ContainerRef>();
                foreach (string id in ChildContainersToSplit.GridContext.SelectedRowIDs)
                {
                    selectedContainers.Add(new OM.ContainerRef(id));
                }
                retVal = selectedContainers.ToArray();
            }
            return retVal;
        }

        protected virtual JQDataGrid ChildContainersToSplit
        {
            get { return FindCamstarControl("ChildContainersGrid") as JQDataGrid; }
        }
    }
}
