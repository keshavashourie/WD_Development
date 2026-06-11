// Copyright Siemens 2023  
using System;
using System.Web;
using System.Web.UI;
using Camstar.WebPortal.WebPortlets;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.FormsFramework.WebControls;



namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    /// <summary>
    ///  SamplingCurrentStatus is the functionality to control the SamplingCurrentStatus_VP
    /// </summary>
    public class SamplingCurrentStatus : MatrixWebPart
    {
        public SamplingCurrentStatus()
        {

        }
        
        #region Properties
        protected virtual JQDataGrid ServiceDetail { get { return Page.FindCamstarControl("CurrentSamplingStatusUpdate_SamplingCurrentStatus") as JQDataGrid; } }
        protected virtual CWC.TextBox newSampleRateCounterValue { get { return Page.FindCamstarControl("CurrentSamplingStatusUpdate_NewSampleRateCounterValue") as CWC.TextBox; } }
        protected virtual CWC.NamedObject newInspectionLevel { get { return Page.FindCamstarControl("CurrentSamplingStatusUpdate_NewInspectionLevel") as CWC.NamedObject; } }
        protected virtual CWC.RevisionedObject productSection { get { return Page.FindCamstarControl("CurrentSamplingStatusUpdate_Product") as CWC.RevisionedObject; } }
        #endregion
        
        #region Methods and Events
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            //if (action != null && action.Parameters == "ApplyData")
            //{
            //    object[] o = ServiceDetail.GridContext.GetSelectedItems(false);

            //    if (o != null)
            //    {
            //        foreach (Camstar.WCF.ObjectStack.SamplingCurrentStatus xx in o)
            //        {
            //            if(newSampleRateCounterValue!=null)
            //            xx.CurrentSampleRate = (int)newSampleRateCounterValue.Data;
                        
            //            if(newInspectionLevel!=null)
            //            xx.CurrentInspectionLevel = (Camstar.WCF.ObjectStack.NamedObjectRef)newInspectionLevel.Data;
            //        }
            //    }
            //}
            


            if (action != null && action.Parameters == "ResetData")
            {
                productSection.ClearData();
                ServiceDetail.ClearData();
                newInspectionLevel.ClearData();
                newSampleRateCounterValue.ClearData();
            }
        }

        public override bool PreExecute(OM.Info serviceInfo, OM.Service serviceData)
        {

            OM.CurrentSamplingStatusUpdate serviceDataCopy = (OM.CurrentSamplingStatusUpdate)serviceData;

                object[] o = ServiceDetail.GridContext.GetSelectedItems(false);

                if (o != null)
                {
                    OM.SamplingCurrentStatus[] selectedItems = new OM.SamplingCurrentStatus [o.Length];
                    int index = 0;
                    foreach (Camstar.WCF.ObjectStack.SamplingCurrentStatus xx in o)
                    {
                        OM.SamplingCurrentStatus tempSCS = new OM.SamplingCurrentStatus();
                        tempSCS.Self = xx.Self;
                        selectedItems.SetValue(tempSCS, index);
                        index++;

                    }
                    
                   serviceDataCopy.ServiceDetails = selectedItems;
                }

                serviceData = serviceDataCopy;

            return base.PreExecute(serviceInfo, serviceData);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
                return;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }
        #endregion
    }
}
