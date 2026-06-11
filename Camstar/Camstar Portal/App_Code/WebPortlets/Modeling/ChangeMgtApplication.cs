// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.WebPortlets;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

/// <summary>
/// Summary description for ChangeMgtApplication
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class ChangeMgtApplication : MatrixWebPart
    {
        private const int MaxWhereUsed = 1;

        public override FormsFramework.ValidationStatus ValidateInputData(Service serviceData)
        {
            ValidationStatus status = base.ValidateInputData(serviceData);
            var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
            if (labelCache != null)
            {
                var label = labelCache.GetLabelByName("Lbl_NumberOfWhereUsedInstancesMustBeGreater");
                var data = serviceData as ChangeMgtApplicationMaint;
                if (data != null)
                {
                    if (data.ObjectChanges != null && data.ObjectChanges.MaxWhereUsedInstances != null && data.ObjectChanges.MaxWhereUsedInstances.Value < MaxWhereUsed)
                    {
                        string validationMessage = label.Value;
                        ValidationStatusItem statusItem = new FormsFramework.RegularValidationStatusItem(validationMessage);
                        status.Add(statusItem);
                    }
                }
            }
            return status;
        }
    }
}
