//
// Copyright Siemens 2023  
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.ChangeManagement
{
    public class OpenPackageMultiPopup : MatrixWebPart
    {
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            var data = (serviceData as OpenCPStatuses);
            var changePackages = Page.SessionVariables.GetValueByName("ChangePackage") as List<String>;
            if (data != null && changePackages != null && changePackages.Count != 0)
            {
                data.ChangePackages = new NamedObjectRef[changePackages.Count];
                int i = 0;
                foreach (var id in changePackages)
                {
                    data.ChangePackages[i++] = new NamedObjectRef { Name = id };
                }
                Page.SessionVariables.SetValueByName("ReloadPage", true);
            }

            Page.SessionVariables.SetValueByName("ChangePackage", null);

        }
    }
}
