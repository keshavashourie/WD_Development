// Copyright Siemens 2023 
using System;
using System.Linq;
using System.Web.UI;

using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Constants;

namespace Camstar.WebPortal.WebPortlets
{
    public class WIPMessagesProcessing : MatrixWebPart
    {
        protected override void OnLoad(EventArgs e)
        {
            WIPMessagesGrid.RowSelected += new JQGridEventHandler(WIPMessagesGrid_RowSelected);
            MessagePassword.DataChanged += MessagePassword_DataChanged;
            MessageAcknowledged.DataChanged += MessageAcknowledged_DataChanged;

            if (MessageAcknowledged.Visible == false)
                MessageAcknowledged.Data = true;

            if (!Page.IsPostBack && WIPMessagesGrid.Data != null && WIPMessagesGrid.GridContext != null && WIPMessagesGrid.TotalRowCount > 0)
            {
                SelectedRow = "000000";
                WIPMessagesGrid.GridContext.SelectRow(SelectedRow, true);
                WIPMessagesGrid.GridContext.SelectedRowID = SelectedRow;
                JQGridEventArgs args = new JQGridEventArgs(WIPMessagesGrid.GridContext, "select");
                WIPMessagesGrid_RowSelected(this, args);
            }

            base.OnLoad(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }
        protected virtual void MessageAcknowledged_DataChanged(object sender, EventArgs e)
        {
            WIPMessagesGrid.GridContext.SetCell(SelectedRow, "MsgAcknowledged", MessageAcknowledged.IsChecked);
        }

        protected virtual void MessagePassword_DataChanged(object sender, EventArgs e)
        {
            WIPMessagesGrid.GridContext.SetCell(SelectedRow, "EnteredPassword", MessagePassword.Data);
        }

        protected virtual ResponseData WIPMessagesGrid_RowSelected(object sender, JQGridEventArgs args)
        {
            SelectedRow = args.Context.SelectedRowID;
            string errorMessage;
            var wipItem = WIPMessagesGrid.GetItem(SelectedRow, out errorMessage) as WIPMsg;
            if (wipItem != null)
            {
                if (wipItem.WIPMsgDetails != null && wipItem.WIPMsgDetails.Document != null)
                {
                    var docName = wipItem.WIPMsgDetails.Document.ToString();
                    var docSet = new DocumentSet
                    {
                        DocumentEntries = new[]
                        {
                            new DocumentEntry
                            {
                                DocumentIdentifier = wipItem.WIPMsgDetails.DocumentIdentifier,
                                DocumentBrowseMode = wipItem.WIPMsgDetails.DocumentBrowseMode,
                                Name = docName,
                                DisplayName = docName,
                                Document = new RevisionedObjectRef(docName)
                            }
                        }
                    };

                    ViewDocument.Data = docSet;
                }
                else
                {
                    ViewDocument.Data = null;
                }

                MessagePassword.Data = wipItem.EnteredPassword;
                MessageAcknowledged.Data = wipItem.MsgAcknowledged.Value;
                ContactInfo.Data = wipItem.WIPMsgDetails.ContactInfo != null ? wipItem.WIPMsgDetails.ContactInfo.Value : string.Empty;

                ViewDocument.Visible = ViewDocument.Data != null;
                MessagePassword.Visible = wipItem.PasswordRequired != null && wipItem.PasswordRequired.Value;
                MessageAcknowledged.Visible = wipItem.AcknowledgementRequired != null && wipItem.AcknowledgementRequired.Value;
                ContactInfo.Visible = wipItem.WIPMsgDetails.ContactInfo != null && !string.IsNullOrEmpty(wipItem.WIPMsgDetails.ContactInfo.Value);
            }
            else
            {
                ViewDocument.Visible = false;
                MessagePassword.Visible = false;
                MessageAcknowledged.Visible = false;
                ContactInfo.Visible = false;
            }

            return args.Response;
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);

            CustomAction action = e.Action as CustomAction;
            if (action != null)
            {
                if (action.Parameters == "ResetWIPMessages")
                {
                    ResetAcknowledgement();
                }
                else if (action.Parameters == "SubmitWIPMessages")
                {
                    var boundContext = WIPMessagesGrid.BoundContext as ItemDataContext;
                    if (boundContext != null)
                    {
                        if (MessageAcknowledged != null && MessageAcknowledged.Required)
                            boundContext.SetCell(SelectedRow, "MsgAcknowledged", MessageAcknowledged.IsChecked);
                        else MessageAcknowledged.IsChecked = true;

                        if (MessagePassword != null)
                            boundContext.SetCell(SelectedRow, "EnteredPassword", MessagePassword.Data);
                    }
                    var context = WIPMessagesGrid.GridContext as ItemDataContext;
                    WIPMsg[] items = context.Data as WIPMsg[];
                    if (items != null)
                    {
                        bool hasNoAckItems = items.Any(w => ((bool)(w.AcknowledgementRequired ?? false) && !(bool)(w.MsgAcknowledged ?? false)));
                        bool noPassForItems = items.Any(w => ((bool)(w.PasswordRequired ?? false) && string.IsNullOrEmpty((string)(w.EnteredPassword ?? string.Empty))));

                        LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                        if (hasNoAckItems)
                        {
                            Camstar.WCF.ObjectStack.Label errorLabel = labelCache.GetLabelByName(AcknowledgeWIPMessageKey);
                            if (errorLabel != null)
                                e.Result = new ResultStatus(errorLabel.Value ?? AcknowledgeWIPMessageKey, false);
                        }
                        else if (noPassForItems)
                        {
                            Camstar.WCF.ObjectStack.Label errorLabel = labelCache.GetLabelByName(NoPasswordForWIPMessageKey);
                            if (errorLabel != null)
                                e.Result = new ResultStatus(errorLabel.Value ?? NoPasswordForWIPMessageKey, false);
                        }
                        else
                        {
                            e.IsSubmitted = true;
                            Page.CollectDataContract();
                        }
                    }
                }
            }
        }

        public virtual void ResetAcknowledgement()
        {
            var context = WIPMessagesGrid.GridContext as ItemDataContext;
            WIPMsg[] items = context.Data as WIPMsg[];
            if (items != null)
                items.ToList().ForEach(w => { w.MsgAcknowledged = false; w.EnteredPassword = null; });

            MessageAcknowledged.Data = false;
            MessagePassword.Data = null;
        }

        #region Properties

        protected virtual JQDataGrid WIPMessagesGrid
        {
            get { return FindCamstarControl("WIPMessagesGrid") as JQDataGrid; }
        }

        protected virtual CWC.CheckBox MessageAcknowledged
        {
            get { return FindCamstarControl("MessageAcknowledged") as CWC.CheckBox; }
        }

        protected virtual CWC.TextBox MessagePassword
        {
            get { return FindCamstarControl("MessagePassword") as CWC.TextBox; }
        }

        protected virtual CWC.TextBox ContactInfo
        {
            get { return FindCamstarControl("ContactInfo") as CWC.TextBox; }
        }

        protected virtual CWC.ViewDocumentsControl ViewDocument
        {
            get { return FindCamstarControl("ViewDocument") as CWC.ViewDocumentsControl; }
        }

        protected virtual string SelectedRow
        {
            get { return ViewState["_sr"] as string; }
            set { ViewState["_sr"] = value; }
        }

        #endregion

        private const string AcknowledgeWIPMessageKey = "WIPMsgNotAcknowledged";
        private const string NoPasswordForWIPMessageKey = "NoPasswordForWIPMessage";
    }
}

