// Copyright Siemens 2023  
using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Camstar.WebPortal.PortalFramework;
using System.Web.UI;
using Camstar.WebPortal.FormsFramework;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using CWGC = Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Personalization;


namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class ResourceCollectData : MatrixWebPart
    {
        public ResourceCollectData()
        {
        }

        #region Protected methods
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var resource = this.CamstarControls.FindControl("HiddenSelectedResource") as CWC.NamedObject;

            if (resource == null
                || resource.Data == null
                || resource.OriginalData == null)
            {
                return;
            }

            string dataName = (resource.Data as OM.NamedObjectRef).Name;
            string originalDataName = (resource.OriginalData as OM.NamedObjectRef).Name;

            if (dataName == originalDataName)
            {
                resource.OriginalData = null;
            }
        }
        #endregion
    }
}