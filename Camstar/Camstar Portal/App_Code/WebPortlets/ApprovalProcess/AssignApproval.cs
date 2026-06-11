// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets.ApprovalProcess
{
    public class AssignApproval : MatrixWebPart
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                LoadTemplateAllowed = false;
                Page.Service.LoadServiceValues("AssignApprovalSheet", "GetApprovalSheetDetails");
                NamedSubentityRef approvalSheet = null;
                if (FindCamstarControl("ApprovalSheet") is CWC.NamedSubentity)
                    approvalSheet = (FindCamstarControl("ApprovalSheet") as CWC.NamedSubentity).Data as NamedSubentityRef;
                NamedObjectRef template = null;
                if (FindCamstarControl("ApprovalSheetTemplate") is CWC.NamedObject)
                    template = (FindCamstarControl("ApprovalSheetTemplate") as CWC.NamedObject).Data as NamedObjectRef;
                if (approvalSheet.IsNullOrEmpty() && !template.IsNullOrEmpty())
                    LoadApprovalSheetTemplate(template);
            }
        }
        
        public virtual void LoadTemplate(NamedObjectRef template)
        {
            if (LoadTemplateAllowed)
            {
                LoadTemplateAllowed = false;
                ClearValues();
                if (!template.IsNullOrEmpty())
                {
                    LoadApprovalSheetTemplate(template);
                }
            }
        }

        protected virtual void LoadApprovalSheetTemplate(NamedObjectRef template)
        {
            ApprovalSheetDetails details;
            ResultStatus res = ApprovalCommon.LoadApprovalSheetTemplate(template, out details);
            if (res.IsSuccess)
            {
                AssignApprovalSheet service = new AssignApprovalSheet() { ApprovalSheetDetails = details, ApprovalSheetTemplate = template };
                DisplayValues(service);
            }
            else
                DisplayMessage(res);
        }

        private bool LoadTemplateAllowed = true;
    }
}
