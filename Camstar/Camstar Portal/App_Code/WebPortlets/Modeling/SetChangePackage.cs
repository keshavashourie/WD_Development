// Copyright Siemens 2023  
using System;
using System.Data;
using System.Web;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    /// <summary>
    /// Summary description for SetChangePackage
    /// </summary>
    public class SetChangePackage : MatrixWebPart
    {
        protected virtual JQDataGrid AvailablePackages
        {
            get { return Page.FindCamstarControl("AvailablePackages") as JQDataGrid; }
        }
        protected virtual Button SetAsDefault
        {
            get { return Page.FindCamstarControl("SetAsDefault") as Button; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (SetAsDefault != null)
                SetAsDefault.Click += SetAsDefault_Click;
        }

        protected virtual void SetAsDefault_Click(object sender, EventArgs e)
        {
            if (AvailablePackages.SelectedItem != null)
            {
                var changePkgId = (AvailablePackages.SelectedItem as DataRow)["ChangePackageId"];
                Page.SessionVariables[SessionConstants.ModelingCDOList_DefaultPkgID] = changePkgId;
				
				var changePkgName = (AvailablePackages.SelectedItem as DataRow)["PackageName"];
                ScriptManager.RegisterStartupScript(this, GetType(), "refresh", string.Format("$(function(){{parent.RefreshModelingSelectedDefaultPkg('{0}');}});", changePkgName), true);
            }

        }
        public override FormsFramework.ValidationStatus ValidateInputData(Service serviceData)
        {
            ValidationStatus status = base.ValidateInputData(serviceData);
            if (AvailablePackages.SelectedItem == null)
            {
                var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                if (labelCache != null)
                {
                    var label = labelCache.GetLabelByName("Lbl_RequiredGridMessage");
                    string validationMessage = String.Format(label.Value, "Available Packages");
                    ValidationStatusItem statusItem = new RequiredFieldStatusItem(AvailablePackages.Caption, null)
                    {
                        ID = AvailablePackages.ID,
                        RequiredMessage = label.Value
                    };
                    status.Add(statusItem);
                }
            }

            return status;
        }
    }
}
