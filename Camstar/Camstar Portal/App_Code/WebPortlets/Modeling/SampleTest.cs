// Copyright Siemens 2023  
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    public class SampleTest : MatrixWebPart
    {
        #region Controls

        protected virtual CWC.NamedObject AQLRejectReasons
        {
            get { return Page.FindCamstarControl("ObjectChanges_AQLRejectReasons") as CWC.NamedObject; }
        }

        protected virtual CWC.NamedObject DecreaseByRejectCountReason
        {
            get { return Page.FindCamstarControl("ObjectChanges_DecreaseByRejectCountReason") as CWC.NamedObject; }
        }

        protected virtual CWC.CheckBox ScrapCountedRejectsByReason
        {
            get { return Page.FindCamstarControl("ObjectChanges_ScrapCountedRejectsByReason") as CWC.CheckBox; }
        }

        protected virtual CWC.DropDownList SampleType
        {
            get { return Page.FindCamstarControl("ObjectChanges_SampleType") as CWC.DropDownList; }
        }

        #endregion

        #region Protected Functions

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Page.PreRender += Page_PreRender;
        }

        protected virtual void Page_PreRender(object sender, EventArgs e)
        {
            AQLRejectReasons.Enabled = (SampleType.Data == null || (SampleTypeEnum)SampleType.Data != SampleTypeEnum.Measured);

            if (SampleType.Data != null && (SampleTypeEnum)SampleType.Data == SampleTypeEnum.Counted)
            {
                DecreaseByRejectCountReason.Enabled = !ScrapCountedRejectsByReason.IsChecked;
                ScrapCountedRejectsByReason.Enabled = (DecreaseByRejectCountReason.Data == null);
                AQLRejectReasons.Required = true;

            }
            else
            {
                DecreaseByRejectCountReason.Enabled = true;
                ScrapCountedRejectsByReason.Enabled = (SampleType.Data == null || (SampleTypeEnum)SampleType.Data != SampleTypeEnum.Measured);
                AQLRejectReasons.Required = false;
            }
        }

        public override void GetInputData(Service serviceData)
        {
            base.GetInputData(serviceData);
            if ((serviceData as SampleTestMaint).ObjectChanges != null) {
                if (DecreaseByRejectCountReason.Data == null)
                    (serviceData as SampleTestMaint).ObjectChanges.DecreaseByRejectCountReason = new NamedObjectRef("");
                (serviceData as SampleTestMaint).ObjectChanges.ScrapCountedRejectsByReason = ScrapCountedRejectsByReason.IsChecked;
                if (SampleType.Data != null && (SampleTypeEnum)SampleType.Data == SampleTypeEnum.Measured)
                    (serviceData as SampleTestMaint).ObjectChanges.AQLRejectReasons = new NamedObjectRef("");
            }
        }

        #endregion


    }

}

