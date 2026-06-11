// Copyright Siemens 2023
using System;
using System.Linq;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Personalization;
using System.Collections.Generic;
using Camstar.WebPortal.Utilities;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class SPCViolationsPopup : MatrixWebPart
    {
        protected virtual NamedObject SPCViolation { get { return Page.FindCamstarControl("Violations_SPCViolation") as NamedObject; } }
        protected virtual JQDataGrid ViolationParamsGrid { get { return Page.FindCamstarControl("Violations_ViolationParams") as JQDataGrid; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SPCViolation.DataChanged += SPCViolationChanged;
        }

        protected virtual void SPCViolationChanged(object sender, EventArgs e)
        {
            if (SPCViolation.Data != null)
            {
                var service = new SPCViolationMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
                var cdo = new SPCViolationMaint { ObjectToChange = new NamedObjectRef(SPCViolation.Data.ToString()) };
                var request = new SPCViolationMaint_Request
                {
                    Info = new SPCViolationMaint_Info
                    {
                        ObjectChanges = new SPCViolationChanges_Info
                        {
                            SPCViolationParams = new SPCViolationParamsChanges_Info
                            {
                                ParamName = FieldInfoUtil.RequestValue(),
                                ParamValue = FieldInfoUtil.RequestValue()
                            }
                        }
                    }
                };

                ResultStatus oRS = service.Load(cdo, request, out var oResult);

                SPCViolationParamsChanges[] vParams = oResult?.Value?.ObjectChanges?.SPCViolationParams;
                if (oRS.IsSuccess && vParams != null)
                {

                    var spcViolationParams = vParams
                    .Select(p => new SPCViolationParamsChanges
                    {
                        ParamValue = p.ParamValue,
                        ParamName = p.ParamName
                    })
                    .ToArray();

                    (ViolationParamsGrid.GridContext as BoundContext).Data = spcViolationParams;
                    ViolationParamsGrid.GridContext.LoadData();

                    CamstarWebControl.SetRenderToClient(ViolationParamsGrid);
                }
            }
            else
            {
                ViolationParamsGrid.ClearData();

                CamstarWebControl.SetRenderToClient(ViolationParamsGrid);
            }
        }

    }
}
