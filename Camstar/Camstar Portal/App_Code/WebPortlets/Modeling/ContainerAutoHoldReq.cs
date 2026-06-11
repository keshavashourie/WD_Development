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
    public class ContainerAutoHoldReq : MatrixWebPart
    {
        protected virtual JQDataGrid ContainerList
        {
            get
            {
                return Page.FindCamstarControl("ObjectChanges_ContainerList") as JQDataGrid;
            }

        }
        protected virtual JQDataGrid LotList
        {
            get
            {
                return Page.FindCamstarControl("ObjectChanges_LotList") as JQDataGrid;
            }

        }

        private ValidationStatus ValidateList(JQDataGrid ListData, ValidationStatus status, string validationMessage)
        {
            foreach (Primitive<string> item in (dynamic)(ListData.Data))
            {
                if (item == null)
                {
                    status.Add(new RequiredFieldStatusItem(ListData.Caption, null) { RequiredMessage = string.Format(validationMessage, ListData.Caption) });
                    return status;
                }
            }

            return status;
        }

        public override FormsFramework.ValidationStatus ValidateInputData(Service serviceData)
        {
            ValidationStatus status = base.ValidateInputData(serviceData);
            var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
            if (labelCache != null)
            {
                var validationMessage = labelCache.GetLabelByName("Lbl_InvalidInput").Value;
                var data = serviceData as ContainerAutoHoldReqMaint;
                if (data != null)
                {
                    if (ContainerList.GridContext.GetTotalRows() > 0)
                    {
                        status = ValidateList(ContainerList, status, validationMessage);
                        if(status.Message != "")
                        {
                            return status;
                        }
                    }

                    if (LotList.GridContext.GetTotalRows() > 0)
                    {
                        status = ValidateList(LotList, status, validationMessage);
                        if (status.Message != "")
                        {
                            return status;
                        }
                    }
                }
            }
            return status;
        }
    }
}
