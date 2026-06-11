// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using PERS = Camstar.WebPortal.Personalization;
using CamstarPortal.WebControls;
using System.Collections;
using Camstar.WebPortal.WCFUtilities;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class BillOfProcessOverrides : MatrixWebPart
    {
        protected virtual JQDataGrid OverridesGrid
        {
            get
            {
                return Page.FindCamstarControl("ObjectChanges_BillOfProcessOverrides") as JQDataGrid;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            OverridesGrid.GridContext.RowUpdated += OverridesGrid_RowUpdated;
            //Solution for bug 221052
            this.RenderToClient = true;
        }

        protected virtual ResponseData OverridesGrid_RowUpdated(object sender, JQGridEventArgs e)
        {
            ResponseData rd = e.Response;
            OM.RevisionedObjectRef spec;

            var data = (OM.BillOfProcessOverrideChanges[])OverridesGrid.Data;
            OM.BillOfProcessOverrideChanges addedItem = null;
            if (data != null)
                addedItem = data.Where(c => c.SpecDescription == "").FirstOrDefault();
            
            if (addedItem == null)
               spec = (OverridesGrid.SelectionData as OM.BillOfProcessOverrideChanges).Spec;
            else
               spec = addedItem.Spec;
                        
            if (spec == null) 
                return rd;

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.SpecMaintService(session.CurrentUserProfile);

            var serviceData = new OM.SpecMaint();

            serviceData.ObjectToChange = new OM.RevisionedObjectRef();
            serviceData.ObjectToChange = spec;

            var request = new Camstar.WCF.Services.SpecMaint_Request();
            var result = new Camstar.WCF.Services.SpecMaint_Result();
            var resultStatus = new OM.ResultStatus();

            request.Info = new OM.SpecMaint_Info
            {
                RequestValue = true,
                ObjectChanges = new OM.SpecChanges_Info
                {
                    Description = new OM.Info(true),
                    IsRevOfRcd = new OM.Info(true),
                    Revision = new OM.Info(true)
                }
            };

            resultStatus = service.Load(serviceData, request, out result);

            if (resultStatus.IsSuccess && result.Value != null)
            {
                if (addedItem == null)
                {
                    (OverridesGrid.SelectionData as OM.BillOfProcessOverrideChanges).SpecDescription = result.Value.ObjectChanges.Description == "" ? " " : result.Value.ObjectChanges.Description;
                    (OverridesGrid.SelectionData as OM.BillOfProcessOverrideChanges).Name = spec.Name + " ("+result.Value.ObjectChanges.Revision+")"+((bool)result.Value.ObjectChanges.IsRevOfRcd ? "*":"");
                }
                else
                    Array.ForEach(data, r =>
                    {
                        if (r.Spec == spec)
                        {
                            r.SpecDescription = result.Value.ObjectChanges.Description == "" ? " " : result.Value.ObjectChanges.Description;
                            r.Name = spec.Name + " ("+result.Value.ObjectChanges.Revision+")"+((bool)result.Value.ObjectChanges.IsRevOfRcd ? "*":"");
                        }
                    });
                e.State.Action = "Reload";
                e.Cancel = true;
                rd = OverridesGrid.GridContext.Reload(e.State);

            }
            return rd;
        }

    }
}
