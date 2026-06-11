// Copyright Siemens 2023  
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.WebPortlets;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Camstar.WebPortal.FormsFramework.Utilities;

/// <summary>
/// Summary description for UpdateChangePackageApprovers
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.ChangeManagement
{
    public class UpdateChangePackageApprovers : MatrixWebPart
    {
        public UpdateChangePackageApprovers()
        {}

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                LoadTemplateAllowed = false;
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
            ResultStatus res = LoadApprovalSheetTemplate(template, out details);
            if (res.IsSuccess)
            {
                UpdateChangePkg service = new UpdateChangePkg() { ApprovalSheetDetails = details, ApprovalSheetTemplate = template };
                DisplayValues(service);
            }
            else
                DisplayMessage(res);
        }


        public static ResultStatus LoadApprovalSheetTemplate(NamedObjectRef template, out ApprovalSheetDetails approvalSheet)
        {
            ResultStatus res = new ResultStatus(null, false);
            approvalSheet = new ApprovalSheetDetails();
            AssignApprovalSheet_Result assignResult = new AssignApprovalSheet_Result();
            ApprovalSheetTemplateMaintService service = new ApprovalSheetTemplateMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            ApprovalSheetTemplateMaint cdo = new ApprovalSheetTemplateMaint();
            cdo.ObjectToChange = template;
            ApprovalSheetTemplateMaint_Request request = new ApprovalSheetTemplateMaint_Request();
            request.Info = new ApprovalSheetTemplateMaint_Info();
            request.Info.ObjectChanges = new ApprovalSheetTemplateChanges_Info();
            request.Info.ObjectChanges.GeneralInstructions = new Info(true);
            request.Info.ObjectChanges.ApprovalDecisionList = new Info(true);
            request.Info.ObjectChanges.ApprovalEntries = new ApprovalSheetEntryChanges_Info();
            request.Info.ObjectChanges.ApprovalEntries.RequestValue = true;
            ApprovalSheetTemplateMaint_Result result;
            res = service.Load(cdo, request, out result);
            if (res.IsSuccess)
            {
                approvalSheet.ApprovalDecisionList = result.Value.ObjectChanges.ApprovalDecisionList;
                approvalSheet.GeneralInstructions = result.Value.ObjectChanges.GeneralInstructions;
                approvalSheet.ApprovalEntries = ConvertEntries(result.Value.ObjectChanges.ApprovalEntries);
            }
            return res;
        }


        protected static ApprovalEntryDetails[] ConvertEntries(ApprovalSheetEntryChanges[] changes)
        {
            ApprovalEntryDetails[] details = null;
            if (!changes.IsNullOrEmpty())
            {
                details = new ApprovalEntryDetails[changes.Length];
                int detailsIndex = 0;

                foreach (ApprovalSheetEntryChanges entry in changes)
                {
                    details[detailsIndex] = new ApprovalEntryDetails();
                    details[detailsIndex].ListItemAction = ListItemAction.Add;
                    details[detailsIndex].Approver = entry.Approver;
                    details[detailsIndex].ApproverRole = entry.ApproverRole;
                    details[detailsIndex].CompleteWithinQty = entry.CompleteWithinQty;
                    details[detailsIndex].CompleteWithinUOM = entry.CompleteWithinUOM;
                    details[detailsIndex].SheetLevel = entry.SheetLevel;
                    details[detailsIndex].EntryRequired = entry.EntryRequired;
                    details[detailsIndex].EditOption = entry.EditOption;
                    details[detailsIndex].SubstituteOption = entry.SubstituteOption;
                    details[detailsIndex].SpecialInstructions = entry.SpecialInstructions;
                    detailsIndex++;
                }//if
            }
            return details;
        }//ConvertEntries
              
        private bool LoadTemplateAllowed = true; 
    }
}
