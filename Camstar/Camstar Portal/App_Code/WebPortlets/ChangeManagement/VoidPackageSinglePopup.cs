// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
/// <summary>
/// Summary description for VoidPackageSinglePopup
/// </summary>
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.ChangeManagement
{
    public class VoidPackageSinglePopup : MatrixWebPart
    {
       public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            var data = (serviceData as VoidCPStatus);
            var changePackage = Page.SessionVariables.GetValueByName("ChangePackage");
            if (data != null && changePackage != null)
            {
                data.ChangePackage = new NamedObjectRef(changePackage.ToString());
                Page.SessionVariables.SetValueByName("ReloadPage", true);
            }
        }
    }
}
