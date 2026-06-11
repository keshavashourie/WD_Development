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
    public class OpenPackageSinglePopup : MatrixWebPart
    {
        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            var data = (serviceData as OpenCPStatus);
            var changePackage = Page.DataContract.GetValueByName("ChangePackage");
            if (data != null && changePackage != null)
            {
                data.ChangePackage = new NamedObjectRef(changePackage.ToString());
                Page.SessionVariables.SetValueByName("ReloadPage", true);
            }
        }
    }
}
