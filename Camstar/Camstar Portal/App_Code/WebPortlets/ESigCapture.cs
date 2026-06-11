// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Personalization;
using Camstar.WCF.Services;
using Camstar.WebPortal.WCFUtilities;

namespace Camstar.WebPortal.WebPortlets
{
    public class ESigCapture : MatrixWebPart
    {

        public enum ESigCaptureMode
        {
            Single,
            Multiple
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
            Page.FloatingFrameSubmitting += new EventHandler<System.ComponentModel.CancelEventArgs>(CheckRowsToBeFilled);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                BatchSignoffRadioButton.Data = true;
                BuildSignerRoleList();
            }
                
            BuildContainerGrid();
            BatchSignoffRadioButton.Parent.Visible = Mode == ESigCaptureMode.Multiple;
            SignerRoleList.Enabled = IsBatchMode;
            GetAllLabelsInOneRequest();
            base.OnLoad(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (Mode == ESigCaptureMode.Single & !Page.IsPostBack)
                this.CssClass = SingleContainerClass;
            AdjustGridContext();
            base.OnPreRender(e);
        }

        protected virtual void AdjustGridContext()
        {
            BoundContext context = (BoundContext)ESigCaptureDetails.GridContext;

            bool selectFirstRow = !IsBatchMode && context.SelectedRowID == null;//select the first row in individual mode by default
            context.Settings.Automation.SelectFirstRow = selectFirstRow;
            if (selectFirstRow)
                context.AutoActionsExecuted = false; //forces the action to be executed on more time

            if (IsBatchMode && context.SelectedRowID != null)//selection is disabled in batch mode, old selection should be removed
            {
                context.SelectRow(null, false);
            }

            context.RowSelectionMode = IsBatchMode ? JQGridSelectionMode.Disable : JQGridSelectionMode.SingleRowSelectWithPostback;
        }

        protected virtual void BuildSignerRoleList()
        {
            DropDownList listControl = SignerRoleList;
            bool visible = Mode == ESigCaptureMode.Multiple && IsBatchMode;
            listControl.Parent.Visible = visible;
            if (!visible) return;
            ESigServiceDetail[] details = ESigCaptureUtil.GetDistinctESigSericeDetails();
            if (details == null) return;
            NamedObjectRef[] singerRoles = details.Select(d => d.Role).Distinct().ToArray();
            System.Web.UI.WebControls.ListItemCollection items = SignerRoleList.DropDownControl.Items;
            items.Clear();
            foreach (NamedObjectRef role in singerRoles)
                items.Add(role.Name);
        }

        protected virtual void BuildContainerGrid()
        {
            string targetName = Page.Request.Form["__EVENTTARGET"];
            ESigServiceDetail[] data = null;
            if (string.Compare(targetName, "ESigClick", true) == 0)
            {
                string esigID = Page.Request.Form["__EVENTARGUMENT"];
                data = ESigCaptureUtil.FilterDetailsByESigID(esigID);
            }
            ESigContainers.Data = data;
        }

        protected virtual void CheckRowsToBeFilled(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BoundContext context = (BoundContext)(ESigCaptureDetails.GridContext);
            ESigServiceCaptureWrapper[] items = (ESigServiceCaptureWrapper[])context.Data;
            bool hasNotValidItem = items.Any(t => !t.IsValid);

            //adds message to the status bar
            if (hasNotValidItem)
            {
                Camstar.WCF.ObjectStack.Label errorLabel = labelCache.GetLabelByName(SignaturesRequiredKey);
                if (errorLabel != null)
                    Page.StatusBar.WriteWarning(errorLabel.Value);
                e.Cancel = true;
            }
        }

        public virtual void AddSignature(object sender, EventArgs e)
        {
            bool isBatchMode = IsBatchMode;
            AddSignature(isBatchMode);
        }

        protected virtual void AddSignature(bool batchMode)
        {
            ShopFloor shopfloor = new ShopFloor();
            Page.GetInputData(shopfloor);
            ESigPasswordCapture capture = null;
            var captures = new WCFObject(shopfloor).GetValue("ESigDetails.CaptureDetails:ESigPasswordCapture") as ESigCaptureDetail[];
            if (captures != null && captures.Length > 0)
                capture = captures[0] as ESigPasswordCapture;
            GridContext context = ESigCaptureDetails.GridContext;
            ResultStatus validationStatus = ValidateCosingReason(capture);
            if (!validationStatus.IsSuccess)
            {
                context.BubbleMessage = validationStatus.ToString();
                return;
            }
            bool success = false;   
            
            // throw message if capture is null
            if (capture == null)
            {
                var errorLabel = labelCache.GetLabelByName(SignaturesRequiredKey);
                if (errorLabel != null)
                    Page.StatusBar.WriteError(errorLabel.Value);
                return;
            }

            if (batchMode)
            {
                string[] rowIds = SelectCaptureRowsIfAvailable(capture);
                success = rowIds != null && rowIds.Length > 0;
                AddCapture(capture, rowIds);
                Page.SetupFocus(null);
            }
            else
            {
                //individual mode
                string selecteRowID = context.SelectedRowID;
                ESigServiceCaptureWrapper selectedRow = (ESigServiceCaptureWrapper)context.GetItem(selecteRowID);
                ResultStatus verificationStatus = VerifyPassword(capture, selectedRow.RequirementID);
                if (verificationStatus.IsSuccess)
                {
                    AddCapture(capture, new string[] { context.SelectedRowID });
                    Page.SetupFocus(null);
                    success = true;
                }
                else
                    context.BubbleMessage = verificationStatus.ToString();
            }
            if (success)
                Page.ClearValues(true);
        }

        public override string FindFirstFocusableControlID()
        {
            NamedObject signer = (NamedObject)(Page.FindCamstarControl("SignerField"));
            if (signer != null)
                return signer.ClientID + "_Edit";
            return base.FindFirstFocusableControlID();
        }

        protected virtual ResultStatus ValidateCosingReason(ESigPasswordCapture capture)
        {
            //validate CosignReason for cosigner
            if (capture != null && capture.Cosigner != null && !capture.Cosigner.IsNullOrEmpty() &&
               (capture.CosignReason == null || capture.CosignReason.IsNullOrEmpty()))
            {
                Camstar.WCF.ObjectStack.Label validationLabel = labelCache.GetLabelByName(CosignReasonRequiredKey);
                return new ResultStatus(validationLabel.Value, false);
            }
            return new ResultStatus { IsSuccess = true };
        }

        protected virtual string[] SelectCaptureRowsIfAvailable(ESigPasswordCapture capture)
        {
            string role = SignerRoleList.DropDownControl.SelectedValue;
            ESigServiceDetail[] allDetails = ESigCaptureUtil.GetDistinctESigSericeDetails();
            ESigServiceDetail[] filteredDetails = null;
            GridContext context = ESigCaptureDetails.GridContext;
            if (!capture.Cosigner.IsNullOrEmpty())//otherwise verify cosigner credentials
            {
                ESigServiceDetail[] detailsWithRole = ESigCaptureUtil.FilterDetailsWithCosignerRole();
                if (detailsWithRole.Length < 1)//there is no detail with a  cosiner role defined
                {
                    WCF.ObjectStack.Label label = labelCache.GetLabelByName(CosignerRoleNotDefinedKey);
                    context.BubbleMessage = !string.IsNullOrEmpty(label.Value) ?
                        string.Format(label.Value,
                        string.Join(",", allDetails.Select(d => d.Meaning.Name).ToArray())) : label.DefaultValue;
                    return new string[0];
                }
                //verify every detail
                filteredDetails = detailsWithRole
                    .Where(it => { ResultStatus st = VerifyPassword(capture, it.ESigReqDetail.ID); return st.IsSuccess; })
                    .ToArray();
                if (!filteredDetails.Any())//input cosigner credentials don't provide required role
                {
                    WCF.ObjectStack.Label label = labelCache.GetLabelByName(CosignerRoleNotProvidedKey);
                    context.BubbleMessage = !string.IsNullOrEmpty(label.Value) ?
                        string.Format(label.Value,
                        string.Join(",", detailsWithRole.Select(d => d.Meaning.Name).ToArray())) : label.DefaultValue;
                    return new string[0];
                }
            }
            else // otherwise otherwiseverify signer credentials even if it's empty
            {
                filteredDetails = ESigCaptureUtil.FilterDetailsBySignerRole(role);
                ResultStatus status = VerifyPassword(capture, filteredDetails[0].ESigReqDetail.ID);
                if (!status.IsSuccess)
                {
                    context.BubbleMessage = status.ToString();
                    return new string[0];
                }
            }
            string[] rowIds = GetFirstEmptyRowIDs(filteredDetails);
            if (filteredDetails.Length >= 1 && rowIds.Length < 1)//shows message that all details have been filled
            {
                Camstar.WCF.ObjectStack.Label messageLabel = labelCache.GetLabelByName(SignaturesCapturedKey);
                context.BubbleMessage = messageLabel.Value;
            }
            context.SelectedRowIDs = new List<string>(rowIds);
            return rowIds;
        }

        protected virtual void AddCapture(ESigPasswordCapture capture, IEnumerable<string> rowIds)
        {
            GridContext context = ESigCaptureDetails.GridContext;
            foreach (string id in rowIds)
            {
                ESigServiceCaptureWrapper item = (ESigServiceCaptureWrapper)context.GetItem(id);
                item.ApplyCapture(capture);
            }
            ESigServiceCaptureWrapper[] data = (ESigServiceCaptureWrapper[])(((BoundContext)ESigCaptureDetails.GridContext).Data);
        }

        protected virtual void ResetSignatures()
        {
            ESigCaptureUtil.ResetCaptures();
            ESigCaptureDetails.GridContext.SelectRow(null, false);
            Page.DistributeDataContract();
            Page.ClearValues(true);
            Page.SetupFocus(null);
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            CustomAction action = e.Action as CustomAction;
            if (action != null)
            {
                if (action.Parameters == "ResetSignatures")
                {
                    ResetSignatures();
                }
                else if (action.Parameters == "SubmitSignatures")
                {
                    BoundContext context = (BoundContext)(ESigCaptureDetails.GridContext);
                    ESigServiceCaptureWrapper[] items = (ESigServiceCaptureWrapper[])context.Data;
                    bool hasNotValidItem = items.Any(t => !t.IsValid);

                    //adds message to the status bar
                    if (hasNotValidItem)
                    {
                        Camstar.WCF.ObjectStack.Label errorLabel = labelCache.GetLabelByName(SignaturesRequiredKey);
                        if (errorLabel != null)
                            e.Result = new ResultStatus(errorLabel.Value, false);
                    }
                    else
                        e.IsSubmitted = true;
                    Page.CollectDataContract();
                }
                else if (action.Parameters == "ClearSelectedSignatures")
                    ClearSelectedSignatured();
            }
        }

        /// <summary>
        /// Clears selected rows and then adds them to the end.
        /// 1. Clears selected items.
        /// 2. Moves cleared items to the bottom of the group.
        /// </summary>
        public virtual void ClearSelectedSignatured()
        {
            BoundContext context = (BoundContext)(ESigCaptureDetails.GridContext);
            List<ESigServiceCaptureWrapper> items = ((ESigServiceCaptureWrapper[])context.Data).ToList();
            List<ESigServiceCaptureWrapper> checkedItems = items.Where(it => it.Checked).ToList();

            //clear items
            foreach (ESigServiceCaptureWrapper item in checkedItems)
                item.Clear();


            //move empty items to the bottom of group
            var groups = items.GroupBy(it => it.Info);
            List<ESigServiceCaptureWrapper> resultItems = new List<ESigServiceCaptureWrapper>(); 
            foreach(var group in groups)
            {
                //devides the group in to subgroups: changed and not changed
                //not changed items are put at the end of the group
                var changedItems = group.Intersect(checkedItems);
                var notChangedItem = group.Except(changedItems);
                resultItems.AddRange(notChangedItem);
                resultItems.AddRange(changedItems);
            }
            context.Data = resultItems.ToArray();

            Page.SetupFocus(null);

            CamstarWebControl.SetRenderToClient(ESigCaptureDetails);
        }

        protected virtual ResultStatus VerifyPassword(ESigPasswordCapture capture, string esigID)
        {
            ResultStatus status = null;

            if (capture != null)
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession();
                ShopFloorService serv = new ShopFloorService(session.CurrentUserProfile);
                ShopFloor obj = new ShopFloor();
                ShopFloor_Request req = new ShopFloor_Request();
                var res = new ShopFloor_Result();
                var parm = new ShopFloor_VerifyPasswordSignature_Parameters()
                {
                    Signer = capture.Signer != null ? capture.Signer.Name : string.Empty,
                    Password = capture.SignerPassword != null ? capture.SignerPassword.Value : string.Empty,
                    Cosigner = capture.Cosigner != null ? capture.Cosigner.Name : string.Empty,
                    CSPassword = capture.CosignerPassword != null ? capture.CosignerPassword.Value : string.Empty,
                    ESigReqDetail = esigID
                };
                status = serv.VerifyPasswordSignature(obj, parm, req, out res);
            }
            else
            {
                status = new ResultStatus();
            }

            return status;
        }

        protected virtual string[] GetFirstEmptyRowIDs(IEnumerable<ESigServiceDetail> parents)
        {
            BoundContext context = (BoundContext)ESigCaptureDetails.GridContext;
            ESigServiceCaptureWrapper[] items = (ESigServiceCaptureWrapper[])context.Data;
            List<string> rowIds = new List<string>();
            if (parents == null)
                return rowIds.ToArray();
            foreach (ESigServiceDetail detail in parents)
                for (int rowIndex = 0; rowIndex < items.Length; rowIndex++)
                {
                    ESigServiceCaptureWrapper currItem = items[rowIndex];
                    if (!currItem.IsValid && currItem.RequirementID == detail.ESigReqDetail.ID)
                    {
                        string rowId = context.GetRowId(rowIndex);
                        rowIds.Add(rowId);
                        break;
                    }
                }
            return rowIds.ToArray();
        }

        private void GetAllLabelsInOneRequest()
        {
            var labels = new[]{
                new WCF.ObjectStack.Label(SignaturesRequiredKey),
                new WCF.ObjectStack.Label(CosignReasonRequiredKey),
                new WCF.ObjectStack.Label(CosignerRoleNotDefinedKey),
                new WCF.ObjectStack.Label(CosignerRoleNotProvidedKey),
                new WCF.ObjectStack.Label(SignaturesCapturedKey)
            };
            labelCache.GetLabels(new LabelList(labels));
        }

        #region Properties

        protected virtual ESigCaptureMode Mode
        {
            get
            {
                object isMultiple = this.Page.DataContract.GetValueByName("IsMultiple");
                return isMultiple == null ? ESigCaptureMode.Single : ESigCaptureMode.Multiple;
            }
        }

        protected virtual bool IsBatchMode
        {
            get { return Mode == ESigCaptureMode.Multiple && BatchSignoffRadioButton.RadioControl.Checked; }
        }

        protected virtual JQDataGrid ESigCaptureDetails
        {
            get { return FindCamstarControl("CaptureDetails") as JQDataGrid; }
        }

        protected virtual JQDataGrid ESigContainers
        {
            get { return FindControl("ContainerDetails") as JQDataGrid; }
        }

        protected virtual DropDownList SignerRoleList
        {
            get { return FindControl("SignerRoleList") as DropDownList; }
        }

        protected virtual RadioButton BatchSignoffRadioButton
        {
            get { return FindControl("BatchSignoffRadioButton") as RadioButton; }
        }

        #endregion

        private LabelCache labelCache;

        private const string SignaturesRequiredKey = "Msg_ESigCapture_SignaturesRequired";
        private const string CosignReasonRequiredKey = "CosignReasonRequired";
        private const string CosignerRoleNotDefinedKey = "Msg_ESigCapture_CosignerRoleNotDefined";
        private const string CosignerRoleNotProvidedKey = "Msg_ESigCapture_CosignerRoleNotProvidedKey";
        private const string SignaturesCapturedKey = "Msg_ESigCapture_SignaturesCaptured";
        private const string SingleContainerClass = "single-container";
    }
}

