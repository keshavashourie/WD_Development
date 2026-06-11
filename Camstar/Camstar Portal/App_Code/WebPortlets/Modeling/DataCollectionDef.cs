// Copyright Siemens 2023  
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;

using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.WebPortlets;
using Camstar.WebPortal.Personalization;

using CamstarPortal.WebControls;


namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class DataCollectionDef : MatrixWebPart
    {
        protected virtual DropDownList ParametricDataDef
        {
            get
            {
                return Page.FindCamstarControl("ParametricDataDef") as DropDownList;
            }

        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);

            if ((serviceData as DataCollectionDefMaint).ObjectChanges != null)
            {
                if (ParametricDataDef.Data == null)
                    (serviceData as DataCollectionDefMaint).ObjectChanges.ParametricDataDef = new Primitive<string>();
            }
        }

    }
}
