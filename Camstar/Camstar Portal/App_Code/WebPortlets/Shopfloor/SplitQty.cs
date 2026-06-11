// Copyright Siemens 2023  
using System.Data;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using System.Web;
using System.Linq;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SplitQty : MatrixWebPart
    {
        protected virtual CWC.CheckBox AutoNumber
        {
            get { return Page.FindCamstarControl("Split_AutoNumber") as CWC.CheckBox; }
        }

        protected virtual CWC.NamedObject NumberingRule
        {
            get { return Page.FindCamstarControl("Split_NumberingRule") as CWC.NamedObject; }
        }

        protected virtual JQDataGrid ToContainersGrid
        {
            get { return Page.FindCamstarControl("ToContainerGrid") as JQDataGrid; }
        }

        protected virtual string ToContainerNameRequiredErrorMessage
        {
            get
            {
                return FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session).GetLabelByName("SplitQty_ToContainerNameRequired").Value;
            }
        }

        protected virtual CWC.Container HiddenSelectedContainer
        {
            get { return Page.FindCamstarControl("HiddenSelectedContainer") as CWC.Container; }
        }

        public override bool PreExecute(Info serviceInfo, Service serviceData)
        {
            bool status = base.PreExecute(serviceInfo, serviceData);

            if (AutoNumber.IsChecked && NumberingRule.Data == null)
            {
                status = false;
                var labelCache = FrameworkManagerUtil.GetLabelCache(HttpContext.Current.Session);
                Page.DisplayWarning(labelCache.GetLabelByName("Start_NoNumberingRuleDefined").Value);
            }

            if (!AutoNumber.IsChecked)
            {
                SplitDetails[] rows = ToContainersGrid.Data as SplitDetails[];
                foreach (var row in rows)
                {
                    if (string.IsNullOrEmpty(row.ToContainerName.Value))
                    {
                        Page.DisplayMessage(new ResultStatus(ToContainerNameRequiredErrorMessage, false));
                        status = false;
                    }
                    
                }
            }

            return status;
        }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

            AutoNumber.CheckControl.CheckedChanged += CheckControl_CheckedChanged;

            if (!Page.IsPostBack && HiddenSelectedContainer.Data != null)
            {
                HiddenSelectedContainer.LoadOrClearDependentValues();
            }
        }

        protected virtual void CheckControl_CheckedChanged(object sender, System.EventArgs e)
        {
            if (AutoNumber.IsChecked && ToContainersGrid.Data != null)
            {
                SplitDetails[] rows = ToContainersGrid.Data as SplitDetails[];
                foreach (var row in rows)
                    row.ToContainerName = null;
            }
        }
    }
}
