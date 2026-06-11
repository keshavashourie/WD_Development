// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WebPortal.WebPortlets;
using CamstarPortal.WebControls;
using Camstar.WCF.Services;
using Camstar.WCF.ObjectStack;
using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework;

using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class PrintContainerLabel : MatrixWebPart
    {

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (ContainerTmpControl != null)
                ContainerTmpControl.DataChanged += new EventHandler(ContainerTmpControl_DataChanged);
            
        }

        protected virtual ContainerListGrid ContainerTmpControl
        {
            get { return Page.FindCamstarControl("ContainerTmp") as ContainerListGrid; }
        }

        protected virtual ContainerListGrid HiddenSelectedContainerControl
        {
            get { return Page.FindCamstarControl("HiddenSelectedContainer") as ContainerListGrid; }
        }
        

        protected virtual void ContainerTmpControl_DataChanged(object sender, EventArgs e)
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.PrintContainerLabelService(session.CurrentUserProfile);

            var serviceData = new OM.PrintContainerLabel();
            var request = new Camstar.WCF.Services.PrintContainerLabel_Request();
            var result = new Camstar.WCF.Services.PrintContainerLabel_Result();

            request.Info = new OM.PrintContainerLabel_Info
            {
                LabelSummaries = new LabelSummary_Info()
                    {
                        PrinterLabelDefinition = FieldInfoUtil.RequestValue(),
                        LabelCount = FieldInfoUtil.RequestValue(),
                        PrintQueue = FieldInfoUtil.RequestValue()
                    }                
            };

            if (HiddenSelectedContainerControl != null)
                serviceData.Container = (ContainerRef)HiddenSelectedContainerControl.Data;

            serviceData.NoPrinterDefinition = 1;

            ResultStatus resultStatus = service.GetLabelInformation(serviceData, request, out result);

            if (resultStatus != null && resultStatus.IsSuccess)
            {
                DisplayValues(result.Value);
            }
        }
        

        public override void DisplayValues(WCF.ObjectStack.Service serviceData)
        {
            base.DisplayValues(serviceData);
            if (serviceData is OM.PrintContainerLabel && (serviceData as OM.PrintContainerLabel).LabelSummaries != null && (serviceData as OM.PrintContainerLabel).LabelSummaries.Length > 0)
            {
                var printerLabelDef = Page.FindCamstarControl("PrinterLabelDef") as CWC.RevisionedObject;
                if (printerLabelDef != null)
                    printerLabelDef.Data = (serviceData as OM.PrintContainerLabel).LabelSummaries[0].PrinterLabelDefinition;
                var printQueue = Page.FindCamstarControl("PrintQueue") as CWC.NamedObject;
                if (printQueue != null)
                    printQueue.Data = (serviceData as OM.PrintContainerLabel).LabelSummaries[0].PrintQueue;
                var labelCount = Page.FindCamstarControl("LabelCount") as CWC.TextBox;
                if (labelCount != null)
                    labelCount.Data = (serviceData as OM.PrintContainerLabel).LabelSummaries[0].LabelCount;
            }
        }        
    }
}
