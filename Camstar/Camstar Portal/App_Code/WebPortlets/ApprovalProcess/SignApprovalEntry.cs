// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.Services;

namespace Camstar.WebPortal.WebPortlets.ApprovalProcess
{
    public class SignApprovalEntry : MatrixWebPart
    {
        public virtual RecordSet ApprovalDecisions { get { return ViewState["ApprovalDecisions"] as RecordSet; } set { ViewState["ApprovalDecisions"] = value; } }
        protected virtual string ApprovalDecisionId { get { return ViewState["ApprovalDecisionId"] as string; } set { ViewState["ApprovalDecisionId"] = value; } }

        #region Private Member Variables

        private string _parentId;
        private string _approvingForName;

        #endregion

        #region Controls
        protected virtual FormsFramework.WebControls.DropDownList ApprovingForList
        {
            get { return Page.FindCamstarControl("ApprovingFor") as FormsFramework.WebControls.DropDownList; }
        }
        #endregion

        public virtual void DecisionChanged(NamedSubentityRef decision)
        {
            bool required;
            if (ApprovalDecisions != null && ApprovalDecisions.Rows.Length > 0 && !decision.IsNullOrEmpty())
            {
                required = ApprovalDecisions.Rows.Where(r => r.Values[4] == decision.Name).Select(r => bool.Parse(r.Values[2])).FirstOrDefault();
            }
            else
            {
                required = false;
            }
            (FindCamstarControl("Comments") as CWC.TextBox).Required = required;
            if (!decision.IsNullOrEmpty())
            {
            ApprovalDecisionId = ApprovalDecisions.Rows.Where(r => r.Values[4] == decision.Name).Select(r => r.Values[0].ToString()).FirstOrDefault();
            }
        }

        public override void GetInputData(Service serviceData)
        {
            if (ApprovingForList.Data != null)
            {
                _parentId = ApprovingForList.Data.ToString();
                _approvingForName = ApprovingForList.Text;
                ApprovingForList.Data = null;
            }

            base.GetInputData(serviceData);

            (serviceData as SignApproval).ApprovalDecision.ID = ApprovalDecisionId;

            if (_parentId == null || _approvingForName == null) return;
            var signApproval = serviceData as SignApproval;
            if (signApproval != null)
            {
                signApproval.ApprovingFor = new NamedObjectRef(_approvingForName);
                signApproval.ApprovalSheetEntry.ID = _parentId;
            }
            ApprovingForList.Data = _parentId;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                if (Page.DataContract != null && Page.DataContract.GetValueByName("ApprovalSheetEntry") != null)
                {
                    SubentityRef entry = new SubentityRef(Page.DataContract.GetValueByName("ApprovalSheetEntry").ToString());
                    if (Page.DataContract.GetValueByName("ApprovalSheetEntry") is NamedSubentityRef)
                        entry = Page.DataContract.GetValueByName("ApprovalSheetEntry") as NamedSubentityRef;
                    Result result = null;
                    SignApproval service = new SignApproval() { QualityObject = Page.DataContract.GetValueByName("QualityObject") as NamedObjectRef, ApprovalSheetEntry = entry };
                    SignApproval_Info info = new SignApproval_Info() { };
                    Page.RequestValues(info, service);                    

                    info.ApprovalDecision = new Info(false, true);

                    ResultStatus res = Page.Service.GetEnvironment(service, info, null, null, ref result);
                    if (res.IsSuccess)
                    {
                        ApprovalDecisions = (result.Environment as SignApproval_Environment).ApprovalDecision.SelectionValues;
                        Page.DisplayValues(result.Value as SignApproval);
                        if (ApprovalDecisions != null && ApprovalDecisions.Rows.Length > 0)
                            (FindCamstarControl("ApprovalDecisionList") as CWC.NamedSubentity).Data = new SubentityRef(ApprovalDecisions.Rows[0].Values[5]);
                    }
                    else
                    {
                        DisplayMessage(res);
                    }   
                }
            }
        }
    }
}
