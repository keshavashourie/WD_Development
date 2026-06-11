// Copyright Siemens 2023  
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.WCFUtilities;

namespace Camstar.WebPortal.WebPortlets.ApprovalProcess
{
    public class ApprovalCommon
    {
        public static ResultStatus LoadApprovalSheetTemplate(NamedObjectRef template, string serviceName, out ApprovalSheetChanges approvalSheet)
        {
            UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            ResultStatus res = new ResultStatus(null, false);
            approvalSheet = null;
            ICreator cdo = WCFObject.CreateObject(serviceName) as ICreator;
            ICreator request = WCFObject.CreateObject(serviceName + "_Request") as ICreator;
            Result result = null;
            IWCFService service = new WSDataCreator().CreateService(serviceName, profile);            
            if (!template.IsNullOrEmpty())
            {
                cdo.SetValue("ObjectChanges.ApprovalSheets.ApprovalSheetTemplate", template);
                ApprovalSheetChanges_Info approvalInfo = new ApprovalSheetChanges_Info();
                approvalInfo.ApprovalDecisionList = new Info(true);
                approvalInfo.GeneralInstructions = new Info(true);
                approvalInfo.ApprovalEntries = new ApprovalSheetEntryChanges_Info();
                approvalInfo.ApprovalEntries.RequestValue = true;
                request.SetValue("Info.ObjectChanges.ApprovalSheets", approvalInfo);
                if (service is IProcessObjectMaintBase)
                    res = (service as IProcessObjectMaintBase).GetApprovalSheetTemplate(cdo as DCObject, request as Request, out result);
                if (res.IsSuccess)
                {
                    approvalSheet = ((result as ICreator).GetValue("Value.ObjectChanges.ApprovalSheets") as ApprovalSheetChanges[])[0];
                    if (!approvalSheet.ApprovalEntries.IsNullOrEmpty())
                    {
                        foreach (ApprovalSheetEntryChanges entry in approvalSheet.ApprovalEntries)
                        {
                            entry.ListItemIndex = null;
                            entry.ListItemAction = ListItemAction.Add;
                        }
                    }
                }
            }
            return res;
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
    }
}
