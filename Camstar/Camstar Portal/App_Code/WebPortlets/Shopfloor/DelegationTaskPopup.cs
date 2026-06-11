//
// Copyright Siemens 2023  
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
/// <summary>
/// Summary description for DelegationTaskPopup
/// </summary>
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Personalization;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class DelegationTaskPopup : MatrixWebPart
    {

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            var data = (serviceData as DelegateTasks);
            var delegationIds = Page.DataContract.GetValueByName("DelegationSearchSelRows") as Array;
            if (data != null && delegationIds != null && delegationIds.Length != 0)
            {
                data.TrackableObjects = new BaseObjectRef[delegationIds.Length];
                int i = 0;
                foreach (var id in delegationIds)
                {
                    data.TrackableObjects[i++] = new BaseObjectRef { ID = id.ToString() };
                }
            }
            Page.SessionVariables.SetValueByName("UpdateGrid", "true");
        }

    }
}
