// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.HtmlControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using PERS=Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using OM = Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets
{
    public class WorkflowProgress : ButtonsBarBase, IWorkflowProgressAccessor
    {
        public event EventHandler<PageflowProgressEventArgs> Navigate;

        public virtual bool IsVisible { get; set; }

        public virtual int CountPages { get; set; }

        public virtual int ActivityIndex { get; set; }

        public override bool HasButtons
        {
            get { return true; }
        }

        public virtual void Reconstruct(int pagesCount)
        {
            _leftPane.Controls.Clear();
            Construct(pagesCount);
        }

        public virtual void ResetHistory()
        {
            Page.CurrentPageFlow.HistoryLog.Clear();
            Reconstruct(1);
        }
        protected override WebPartWrapperBase CreateWebPartWrapper()
        {
            return new NavigationButtonsWrapper(this);
        } // CreateWebPartWrapper

        /// <summary>
        /// NOTE: The method is not called when HasButtons returns false
        /// </summary>
        /// <param name="contentControls"></param>
        protected override void CreateContentControls(ControlCollection contentControls)
        {

            contentControls.Add(_leftPane);
            Construct(_stepsQueue.Count);
        }

        protected override string ClientControlTypeName
        {
            get { return "Camstar.WebPortal.WebPortlets.WorkflowProgress"; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            List<ScriptReference> refs = base.GetScriptReferences().ToList();
            refs.Add(new ScriptReference("~/Scripts/ClientFramework/Camstar.WebPortal.WebPortlets/WorkflowProgress.js"));
            return refs;
        }

        protected virtual void OnNavigate(PageflowProgressEventArgs e)
        {
            if(Navigate != null)
                Navigate(this, e);
        }

        protected virtual void DirectLink_Command(object sender, CommandEventArgs e)
        {
            NavigationEventTypes eventType = NavigationEventTypes.Cancel;
            if (e.CommandName == "Left")
                eventType = NavigationEventTypes.BackTo;
            else if (e.CommandName == "Right")
                eventType = NavigationEventTypes.NextTo;
            if (!string.IsNullOrEmpty(Page.Request["__EVENTARGUMENT"]))
            {
                // This is direct click from message of reuired fields on the last page
                Page.PageflowControls.Add(SessionConstants.FocusedControlId, Page.Request["__EVENTARGUMENT"]);
                // Event type should be back
                eventType = NavigationEventTypes.BackTo;
            }
            var args = new PageflowProgressEventArgs(eventType) { ActivityName = e.CommandArgument.ToString() };
            OnNavigate(args);
        }

        protected override void OnInit(EventArgs e)
        {
            Title = "Pageflow Progress";
            CssClass = "wp-workflow-progress";
            ActivityIndex = -1;
            CountPages = 0;
            IsVisible = true;

            _pageFlow = Page.CurrentPageFlow;
            if (_pageFlow != null)
            {

                foreach (var n in _pageFlow.GetConsequenceNodes())
                {
                    var st = new PageflowStep(n as PageFlowNode, n.Transitions, _stepsQueue.Count + 1);

                    var tr = n.Transitions.FirstOrDefault(t => _pageFlow.HistoryLog.Any(h => h.ToNode == t.TargetNode.Key.Key));
                    if (tr != null)
                        st.Resolved = true;
                    _stepsQueue.Enqueue(st);
                }

                CountPages = _stepsQueue.Count;
            }

            base.OnInit(e);
        }

        protected virtual void Construct(int count)
        {
            if (_pageFlow != null)
            {
                var currentActivity = _pageFlow.ActiveNodeKey.Key;
                var currStep = _stepsQueue.FirstOrDefault(s => s.Name == currentActivity);
                int currentIndex = (currStep != null) ? currStep.Index : -1;

                var mapping = new PERS.PageMapping();

                _leftPane.ID = "WorkflowProgress";
                _leftPane.AddCssClass(ThemeConstants.CamstarUIWorkflowProgress);

                ListItemElement spacerItem;
                for (int i = 0; i < Math.Min(count, _stepsQueue.Count); i++)
                {
                    PageflowStep step = _stepsQueue.ElementAt(i);
                    spacerItem = new ListItemElement();
                    _leftPane.Controls.Add(spacerItem);

                    ListItemElement stepItem = new ListItemElement();
                    _leftPane.Controls.Add(stepItem);

                    string linkText = mapping.GetPageDescription(step.Name);
                    var link = new LinkButton
                    {
                        ID = step.Name,
                        CommandName = currentIndex > step.Index ? "Left" : "Right",
                        CommandArgument = step.Name
                    };
                    link.Command += DirectLink_Command;

                    string linkDescription = string.Empty;

                    var spanTitle = new HtmlGenericControl("span") {InnerHtml = linkText};
                    spanTitle.Attributes["class"] = "title";
                    var spanDescription = new HtmlGenericControl("span") {InnerHtml = linkDescription};
                    spanDescription.Attributes["class"] = "description";

                    link.Controls.Add(spanTitle);
                    link.Controls.Add(spanDescription);

                    var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                    if (labelCache != null)
                    {
                        if (!string.IsNullOrEmpty(step.PageTitleLabelName))
                        {
                            var span = spanTitle;
                            spanTitle.InnerHtml = labelCache.GetLabelTextByName(step.PageTitleLabelName,
                                val => span.InnerHtml = val);
                        }
                        else if (!string.IsNullOrEmpty(step.PageTitleLabelText))
                            spanTitle.InnerHtml = step.PageTitleLabelText;

                        if (!string.IsNullOrEmpty(step.PageDescriptionLabelName))
                        {
                            var span = spanDescription;
                            var l = link;
                            spanDescription.InnerHtml = labelCache.GetLabelTextByName(step.PageDescriptionLabelName,
                                val =>
                                {
                                    if (!string.IsNullOrEmpty(val))
                                    {
                                        l.CssClass = "twoLines";
                                        span.InnerHtml = val;
                                        span.Style.Add(HtmlTextWriterStyle.Display, "inline-block");
                                    }
                                });
                        }
                        else if (!string.IsNullOrEmpty(step.PageDescriptionLabelText) && currentActivity == step.Name)
                            spanDescription.InnerHtml = step.PageDescriptionLabelText;
                    }

                    if (!string.IsNullOrEmpty(spanDescription.InnerHtml))
                    {
                        link.CssClass = "twoLines";
                        spanDescription.Style.Add(HtmlTextWriterStyle.Display, "inline-block");
                    }

                    stepItem.Attributes.Add("stepindex", step.Index.ToString());
                    spacerItem.Attributes.Add("spacerindex", step.Index.ToString());

                    if (currentActivity == step.Name)
                    {
                        stepItem.Attributes.Add("selected", "true");
                        spacerItem.Attributes.Add("selected", "true");
                    }
                    var isConditionalResolved = (step.Stage == PageflowStepStage.Conditional &&
                                                 (!step.Resolved || count < _stepsQueue.Count));
                    if (step.Index == Math.Min(count, _stepsQueue.Count) && !isConditionalResolved)
                    {
                        stepItem.Attributes.Add("laststep", "true");
                        if (currentActivity == step.Name)
                            CssClass = "wp-workflow-progress workflowprogress-last-selected";
                    }

                    stepItem.Controls.Add(link);

                    if (isConditionalResolved)
                    {
                        spacerItem = new ListItemElement();
                        _leftPane.Controls.Add(spacerItem);

                        stepItem = new ListItemElement();
                        stepItem.Attributes.Add("stepindex", step.Index.ToString());
                        _leftPane.Controls.Add(stepItem);

                        stepItem.Controls.Add(new LiteralControl(@"<a>...</a>"));
                        stepItem.Attributes.Add("laststep", "true");
                    }
                }

                spacerItem = new ListItemElement();
                spacerItem.Attributes.Add("last", "true");
                _leftPane.Controls.Add(spacerItem);
            }
        }

        public override void RequestLabels(out LabelList labelsInfo)
        {
            base.RequestLabels(out labelsInfo);

            labelsInfo.Add(new OM.Label("WorkflowProgress_Condition"));
        }

        public override void Dispose()
        {
            base.Dispose();

            // Prevent memory leaks
            _stepsQueue = null;
        }

        protected virtual PageFlowStateMachine _pageFlow { get; set; }

        private Queue<PageflowStep> _stepsQueue = new Queue<PageflowStep>();
        private UnorderedListElement _leftPane = new UnorderedListElement();
    } // WorkflowButtonsControl

    enum PageflowStepStage { Straight, Conditional, Final }

    class PageflowStep
    {
        public PageflowStep(PageFlowNode n, Transition[] trans, int index)
        {
            _activity = n;
            _transitions = trans;
            _index = index;

            if (n.Transitions.Length > 1)
            {
                Stage = PageflowStepStage.Conditional;
            }
            else
            {
                if(n.Transitions.Any(t=> t.TargetNode is FinalNode))
                    Stage = PageflowStepStage.Final;
                else
                    Stage = PageflowStepStage.Straight;
            }
        }
        public virtual PageflowStepStage Stage { get; private set; }

        public virtual string Name
        {
            get { return _activity.ToString(); }
        }

        public virtual string PageDescriptionLabelName
        {
            get { return _activity.DescLabelName; }
        }

        public virtual string PageDescriptionLabelText
        {
            get { return _activity.DescLabelText; }
        }

        public virtual string PageTitleLabelName
        {
            get { return _activity.TitleLabelName; }
        }

        public virtual string PageTitleLabelText
        {
            get { return _activity.TitleLabelText; }
        }

        public virtual int Index
        {
            get { return _index; }
        }

        public virtual bool Resolved { get; set; }

        private int _index;
        private PageFlowNode _activity;
        private Transition[] _transitions;

    }
}
