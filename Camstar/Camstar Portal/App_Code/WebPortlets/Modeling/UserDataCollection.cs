// Copyright Siemens 2023  
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.UI;
using OM=Camstar.WCF.ObjectStack;
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
    public class UserDataCollection: MatrixWebPart
    {
        protected virtual JQDataGrid DataPointGrid { get { return Page.FindCamstarControl("DataPoints") as JQDataGrid; } }

        protected virtual CheckBox isScaleEnabled { get { return Page.FindCamstarControl("ObjectChanges_isScaleEnabled") as CheckBox; } }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        public override void DisplaySelectionValues(OM.Environment environment)
        {
            base.DisplaySelectionValues(environment);

            if (!_loadingObjectDPSelVal)
            {
                _loadingObjectDPSelVal = true;
                // Load and display selection values for ObjectDataPoint in the grid
                var data = Page.CreateServiceData(PrimaryServiceType);
                var info = Page.CreateServiceInfo(PrimaryServiceType);

                var wcfData = new WCFObject(data);
                var wcfInfo = new WCFObject(info);

                foreach (var s in new string[] { "ObjectType", "ObjectSelValType", "DisplayMode", "QueryType", "QueryName" })
                {
                    wcfData.SetValue(".ObjectChanges.DataPoints:ObjectDataPointChanges." + s, null);
                    wcfInfo.SetValue(".ObjectChanges.DataPoints:ObjectDataPointChanges." + s, new OM.Info(false, true));
                }
                var stats = Page.Service.LoadSelectionValues(data, ref info);
                _loadingObjectDPSelVal = false;
            }
        }

        private bool _loadingObjectDPSelVal = false;
    }
}
