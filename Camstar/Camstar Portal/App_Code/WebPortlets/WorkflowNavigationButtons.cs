// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Camstar.WebPortal.FormsFramework.HtmlControls;
using CamstarWebControls = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.WebPortlets;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Utilities;

namespace Camstar.WebPortal.WebPortlets
{

    public class WorkflowNavigationButtons : ButtonsBarBase, INavigationButtonsAccessor
    {
        public event EventHandler<NavigationEventArgs> Navigate;

        public virtual CamstarWebControls.Button BackButton
        {
            get { return _BackButton; }
            set { _BackButton = value; }
        }

        public virtual CamstarWebControls.Button NextButton
        {
            get { return _NextButton; }
            set { _NextButton = value; }
        }

        public virtual CamstarWebControls.Button SubmitButton
        {
            get { return _SubmitButton; }
            set { _SubmitButton = value; }
        }

        public virtual CamstarWebControls.Button SaveButton
        {
            get { return _SaveButton; }
            set { _SaveButton = value; }
        }

        public virtual CamstarWebControls.Button CancelButton
        {
            get { return _CancelButton; }
            set { _CancelButton = value; }
        }

        public virtual CamstarWebControls.Button DeleteButton
        {
            get { return _DeleteButton; }
            set { _DeleteButton = value; }
        }

        public virtual bool IsVisible { get; set; }

        protected virtual string BackButtonText
        {
            get {
                return GetLabelCacheValue(LabelConstants.BackLabel, LabelConstants.AltBack); 
            }            
        }

        protected virtual string NextButtonText
        {
            get
            {
                return GetLabelCacheValue(LabelConstants.NextLabel, LabelConstants.AltNext); 
            }
        }

        protected virtual string SubmitButtonText
        {
            get
            {
                return GetLabelCacheValue(LabelConstants.SubmitLabel, LabelConstants.AltSubmit); 
            }
        }

        protected virtual string SaveButtonText
        {
            get
            {
                return GetLabelCacheValue(LabelConstants.SaveLabel, LabelConstants.AltSave); 
            }
        }

        protected virtual string CancelButtonText
        {
            get
            {
                return GetLabelCacheValue(LabelConstants.CancelLabel, LabelConstants.AltCancel);               
            }
        }

        protected virtual string DeleteButtonText
        {
            get
            {
                return GetLabelCacheValue(LabelConstants.DeleteLabel, LabelConstants.AltDelete);
            }
        }

        public WorkflowNavigationButtons()
        {
            IsVisible = false;
            Title = "Pageflow Navigation Buttons";

            _BackButton = CreateNavigationButton("backButton", LabelConstants.BackLabel, LabelConstants.AltBack, NavigationButtonDirection.Left, BackButton_Click);
            _NextButton = CreateNavigationButton("nextButton", LabelConstants.NextLabel, LabelConstants.AltNext, NavigationButtonDirection.Right, NextButton_Click);
            _SubmitButton = CreateNavigationButton("submitButton", LabelConstants.SubmitLabel, LabelConstants.AltSubmit, NavigationButtonDirection.None, SubmitButton_Click);
            _SubmitButton.Attributes.Add("onclick", string.Format("RequestESigMode(event, '{0}', '{1}'); return false;", EventArgumentConstants.AjaxEventESigRequirement, "__Page"));
            _SaveButton = CreateNavigationButton("saveButton", LabelConstants.SaveLabel, LabelConstants.AltSave, NavigationButtonDirection.None, SaveButton_Click);
            _CancelButton = CreateNavigationButton("cancelButton", LabelConstants.CancelLabel, LabelConstants.AltCancel, NavigationButtonDirection.None, CancelButton_Click);
            _DeleteButton = CreateNavigationButton("deleteButton", LabelConstants.DeleteLabel, LabelConstants.AltDelete, NavigationButtonDirection.None, DeleteButton_Click);
        }

        public override void DisplayLabels(LabelList labelsInfo)
        {
            base.DisplayLabels(labelsInfo);
            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
            string labelValue = string.Empty;

            string dialogTitle = labelCache.GetLabelByName("Lbl_Information").Value;
            if (_DeleteButton.Visible && _DeleteButton.Enabled)
            {
                string deletePostbackFunction = Page.ClientScript.GetPostBackEventReference(_DeleteButton, "");
                _DeleteButton.Attributes[HTMLConstants.OnClick] = JavascriptUtil.GetConfirmationCall(labelCache.GetLabelTextByName(_deleteConfirmLabelName, val =>
                    _DeleteButton.Attributes[HTMLConstants.OnClick] = JavascriptUtil.GetConfirmationCall(val, deletePostbackFunction, dialogTitle)
                    ), deletePostbackFunction, dialogTitle);
                
            }//if

            if (_CancelButton.Visible && _CancelButton.Enabled)
            {
                string postbackFunction = Page.ClientScript.GetPostBackEventReference(_CancelButton, "");
                _CancelButton.Attributes[HTMLConstants.OnClick] = JavascriptUtil.GetConfirmationCall(labelCache.GetLabelTextByName(_cancelConfirmLabelName, val =>
                    _CancelButton.Attributes[HTMLConstants.OnClick] = JavascriptUtil.GetConfirmationCall(val, postbackFunction, dialogTitle)
                    ), postbackFunction, dialogTitle);
            }//if
        }//DisplayLabels

        protected override WebPartWrapperBase CreateWebPartWrapper()
        {
            return new NavigationButtonsWrapper(this);
        } // CreateWebPartWrapper

        public override bool HasButtons
        {
            get { return true; }
        }

        protected override void CreateContentControls(ControlCollection contentControls)
        {
            base.CreateContentControls(contentControls);
            LeftPane.AddCssClass("left");
            RightPane.AddCssClass("right");

            Controls.Add(LeftPane);
            Controls.Add(RightPane);
            PageFlowStateMachine pageFlow = null;

            // Try/catch need to avoid exception on loading the control from Portal Studio where HTTPContext is not available 
            try
            {
                pageFlow = StateMachineManager.GetPageflow(Page.PortalContext.LocalSession) as PageFlowStateMachine;
            }
            catch
            {
                pageFlow = null;
            }

            if (pageFlow != null && pageFlow.IsWorkflowActive)
            {
                // Construct buttons bar table
                AddButtonToLeftPane(_NextButton);
                AddButtonToLeftPane(_BackButton);                
                
                AddButtonToRightPane(_CancelButton);
                AddButtonToRightPane(_SaveButton);
                AddButtonToRightPane(_SubmitButton);
                AddButtonToRightPane(_DeleteButton);
                

                _BackButton.IsPrimary = false;
                _SaveButton.IsPrimary = false;
                _CancelButton.IsPrimary = false;
                _DeleteButton.IsPrimary = false;
                _DeleteButton.Hidden = true; //Visible only GE saved
                if (pageFlow.AssociatedData!=null)
                    _DeleteButton.Hidden = false;
            }
        }

        protected virtual void BackButton_Click(object sender, EventArgs e)
        {
            if (Navigate != null)
                Navigate(this, new NavigationEventArgs(NavigationEventTypes.Back));
        }

        protected virtual void NextButton_Click(object sender, EventArgs e)
        {
            if (Navigate != null)
                Navigate(this, new NavigationEventArgs(NavigationEventTypes.Next));
        }

        protected virtual void SubmitButton_Click(object sender, EventArgs e)
        {
            if (Navigate != null)
                Navigate(this, new NavigationEventArgs(NavigationEventTypes.Submit));
        }

        protected virtual void SaveButton_Click(object sender, EventArgs e)
        {
            if (Navigate != null)
                Navigate(this, new NavigationEventArgs(NavigationEventTypes.Save));
        }

        protected virtual void CancelButton_Click(object sender, EventArgs e)
        {
            if (Navigate != null)
                Navigate(this, new NavigationEventArgs(NavigationEventTypes.Cancel));
        }

        protected virtual void DeleteButton_Click(object sender, EventArgs e)
        {
            if (Navigate != null)
                Navigate(this, new NavigationEventArgs(NavigationEventTypes.Delete));
        }

        protected virtual CamstarWebControls.Button CreateNavigationButton(string id, string name, string alttext, NavigationButtonDirection direction, EventHandler handler)
        {
            var button = new CamstarWebControls.Button {ID = id };
            button.Text = LabelCache.GetRuntimeCacheInstance().GetLabelTextByName(name, alttext, val => button.Text = val);
            button.Click += handler;

            switch (direction)
            {
                case NavigationButtonDirection.Left:
                    button.LeftSideCssClass = CSSConstants.NavigationButtonLeftArrow;
                    button.RightSideCssClass = CSSConstants.NavigationButtonRightBack;
                    break;
                case NavigationButtonDirection.Right:
                    button.LeftSideCssClass = CSSConstants.NavigationButtonLeftBack;
                    button.RightSideCssClass = CSSConstants.NavigationButtonRightArrow;
                    break;
                case NavigationButtonDirection.None:
                    button.LeftSideCssClass = CSSConstants.NavigationButtonLeftBack;
                    button.RightSideCssClass = CSSConstants.NavigationButtonRightBack;
                    break;
                case NavigationButtonDirection.Both:
                    button.LeftSideCssClass = CSSConstants.NavigationButtonLeftArrow;
                    button.RightSideCssClass = CSSConstants.NavigationButtonRightArrow;
                    break;
            }

            return button;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!IsVisible)
            {
                LeftPane.Visible = false;
                RightPane.Visible = false;
            }
            var pageFlow = StateMachineManager.GetPageflow(Page.PortalContext.LocalSession) as PageFlowStateMachine;
            if (pageFlow != null && pageFlow.AssociatedData != null)
                _DeleteButton.Hidden = false;

            _DeleteButton.Style.Add("margin-right", "25px");
        }

        protected override string ClientControlTypeName
        {
            get { return "Camstar.WebPortal.WebPortlets.WorkflowNavigationButtons"; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            List<ScriptReference> refs = base.GetScriptReferences().ToList<ScriptReference>();

            refs.Add(new ScriptReference("~/Scripts/ClientFramework/Camstar.WebPortal.WebPortlets/WorkflowNavigationButtons.js"));

            return refs;
        }

        public override void AddButtonToRightPane(IButton button)
        {
            AddButton(RightPane, button);
        }

        public override void AddButtonToLeftPane(IButton button)
        {
            AddButton(LeftPane, button);
        }

        // By deault add button into the left pane
        public override void AddButton(IButton button)
        {
            AddButton(LeftPane, button);
        }

        public override DivElement RightPane
        {
            get { return _rightPane; }
        }

        public override DivElement LeftPane
        {
            get { return _leftPane; }
        }

        private CamstarWebControls.Button _BackButton;
        private CamstarWebControls.Button _NextButton;
        private CamstarWebControls.Button _SubmitButton;
        private CamstarWebControls.Button _SaveButton;
        private CamstarWebControls.Button _CancelButton;
        private CamstarWebControls.Button _DeleteButton;

        private DivElement _leftPane = new DivElement();
        private DivElement _rightPane = new DivElement();

        private const string _deleteConfirmLabelName = "DeletePageflowInst_Confirm";
        private const string _cancelConfirmLabelName = "CancelPageflowInst_Confirm";

        public enum NavigationButtonDirection { Left, Right, None, Both }
    } // WorkflowButtonsControl

}
