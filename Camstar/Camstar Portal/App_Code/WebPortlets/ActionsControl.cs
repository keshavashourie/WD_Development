// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

using Camstar.Portal;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.HtmlControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using CWF = Camstar.WebPortal.FormsFramework;
using OM = Camstar.WCF.ObjectStack;
using PERS = Camstar.WebPortal.Personalization;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CamstarPortal.WebControls.Constants;

namespace Camstar.WebPortal.WebPortlets
{
    public class ActionsControl : WebPartBase, IActionsConsumer, IPostBackEventHandler, IClientSideUIComponent
    {
        public ActionsControl()
        {
            Title = "Actions";
            this.CssClass = "webpart-actions";
            Actions = new List<WebPartActionData>();
            IncludeHandlerActions = true;
            ActionsStyle = PERS.ActionsStyleType.Link;
        }

        bool IClientSideUIComponent.Visible
        {
            get { return true; }
        }

        [Category(ControlCategoryConstants.Camstar)]
        [CWF.WebProperty]
        public virtual bool IncludeHandlerActions { get; set; }
        [Category(ControlCategoryConstants.Camstar)]
        [CWF.WebProperty]
        public virtual string AvaliableActionsContainer { get; set; }
        [Category(ControlCategoryConstants.Camstar)]
        [CWF.WebProperty]
        public virtual string AvaliableActionsType { get; set; }
        [Category(ControlCategoryConstants.Camstar)]
        [CWF.WebProperty]
        public virtual PERS.ActionsStyleType ActionsStyle { get; set; }

        [CWF.WebProperty]
        public virtual OrientationType Orientation { get; set; }

        public virtual PERS.DynamicActionsType DynamicActions
        {
            get { return Page.DynamicActionsSettings ?? new PERS.DynamicActionsType(); }
        }

        public virtual OM.ActionDetails[] ActionStatuses
        {
            get { return Page.ActionDispatcher.ActionStatuses; }
            set { Page.ActionDispatcher.ActionStatuses = value; }
        }

        public virtual List<string> SelectedActions
        {
            get
            {
                if (ViewState["SelectedActions"] == null)
                    ViewState["SelectedActions"] = new List<string>();
                return ViewState["SelectedActions"] as List<string>;
            }
            set { ViewState["SelectedActions"] = value; }
        }

        public virtual bool EditActionIsAllowed
        {
            get
            {
                bool res = true;
                if (!string.IsNullOrEmpty(AvaliableActionsContainer) && !string.IsNullOrEmpty(AvaliableActionsType))
                {
                    IFieldData control = (Page as CamstarForm).GetControlByReference(Page as CamstarForm, new PERS.ValueReference { ConnectionType = PERS.DataMemberConnectionType.Control, Key = AvaliableActionsContainer }) as IFieldData;
                    int rules = int.MaxValue;
                    if (control != null && control.OriginalData != null)
                        rules = (int)control.OriginalData;
                    if (Camstar.WebPortal.WCFUtilities.WCFObject.CreateObjectType(AvaliableActionsType).GetFields().FirstOrDefault(f => f.Name == "Edit") != null)
                    {
                        object v1rule = Enum.Parse(Camstar.WebPortal.WCFUtilities.WCFObject.CreateObjectType(AvaliableActionsType), "Edit");
                        if (v1rule != null)
                            res = (rules & (int)v1rule) != 0;
                    }
                }
                return res;
            }
        }

        public virtual bool ActionsAreDisallowed
        {
            get
            {
                bool res = false;
                if (!string.IsNullOrEmpty(AvaliableActionsContainer) && !string.IsNullOrEmpty(AvaliableActionsType))
                {
                    IFieldData control = (Page as CamstarForm).GetControlByReference(Page as CamstarForm, new PERS.ValueReference { ConnectionType = PERS.DataMemberConnectionType.Control, Key = AvaliableActionsContainer }) as IFieldData;
                    int rules = int.MaxValue;
                    if (control != null && control.OriginalData != null)
                        rules = (int)control.OriginalData;
                    if (Camstar.WebPortal.WCFUtilities.WCFObject.CreateObjectType(AvaliableActionsType).GetFields().FirstOrDefault(f => f.Name == "View") != null)
                    {
                        object v1rule = Enum.ToObject(Camstar.WebPortal.WCFUtilities.WCFObject.CreateObjectType(AvaliableActionsType), rules);
                        if (v1rule != null)
                            res = v1rule.ToString() == "View";
                    }
                }
                return res;
            }
        }

        protected virtual List<WebPartActionData> Actions { get; set; }

        public virtual bool IsFloatingFrame
        {
            get { return Page.IsFloatingFrame; }
        }

        protected virtual List<ActionData> PageActions { get; private set; }

        protected virtual PERS.UIAction[] UIActions { get; set; }

        protected virtual PERS.ActionGroupType[] ActionGroups { get; set; }

        [ActionsListConsumer(ConnectionConstants.ActionsListConnectionName, ConnectionConstants.ActionsListConnectionID, typeof(CamstarConsumerConnectionPoint), AllowsMultipleConnections = true, ConnectionDataType = typeof(WebPartActionData))]
        public virtual void SetActionList(WebPartActionData webPartsActions, Type connectionDataType)
        {
            //TODO : Connecting ActionsControl with multiple providers remains not implemented.
            //An idea for multiple connections implementation is to create a kind of sections for each web part`s actions
            //so that Activating connection with any provider will update actions list only in provider`s section leaving other sections unchanged
            if (webPartsActions != null)
            {
                if (webPartsActions.WebPartActions != null && webPartsActions.WebPartActions.Count > 0 && !String.IsNullOrEmpty(webPartsActions.ProviderID))
                {
                    if (webPartsActions is HeaderWebPartActionData)
                        _HeaderWebPartActionData = webPartsActions as HeaderWebPartActionData;
                    else
                    {
                        WebPartActionData existingItem = Actions.Find(item => item.ProviderID == webPartsActions.ProviderID);
                        if (existingItem != null)
                        {
                            int indexOf = Actions.IndexOf(existingItem);
                            if (Actions.Remove(existingItem))
                            {
                                Actions.Insert(indexOf, webPartsActions);
                            }//if
                        }//if
                        else
                        {
                            Actions.Add(webPartsActions);
                        }//else
                    }
                }//if
            }//if
        }//SetActionList

        public virtual void ReloadActions()
        {
            if (Actions != null)
                Actions.Clear();
            GetPageActionsList();
        }

        protected override void ProcessUndefinedProperty(PERS.PropertyBase property)
        {
            if (property is PERS.IncludeHandlerActions)
            {
                this.IncludeHandlerActions = (bool)property.Value;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Page.IsSideCommandBar)
                this.CssClass = " hidden";  //  NEVER use += as it results in duplication of value
            if (DisplayHeader)
            {
                DivElement header = new DivElement();
                header.AddCssClass("header");
                header.InnerText = string.Empty;

                var label = FrameworkManagerUtil.GetLabelCache().GetLabelByName("Lbl_AvailableTransactions");
                if (label != null)
                    header.InnerText = label.Value ?? "Available Transactions";

                ContentControls.Add(header);
            }
           

            if (PageActions != null && PageActions.Count > 0)
            {
                GeneratePageActionsList();
            }
            if (Actions != null && Actions.Count > 0)
            {
                GenerateWebPartActionsList(Actions);
            }
            if (_HeaderWebPartActionData != null)
            {
                foreach (ActionData action in _HeaderWebPartActionData.WebPartActions)
                    ContentControls.Add(CreateActionItem(action, _HeaderWebPartActionData.ProviderID));
            }
            _HeaderWebPartActionData = null;

            if (UIActions.Any(action => action.Caption == null))
            {
                Page.ActionDispatcher.UpdateActionsLabels();
            }

            RenderUIActions();

            Update();

            if (CssClass.Contains("lying"))
            {
                Width = UnitValues.Percent100;
            }
        }

        protected virtual void ApplyToActions(PERS.UIAction[] actions)
        {
            if (!string.IsNullOrEmpty(AvaliableActionsContainer) && !string.IsNullOrEmpty(AvaliableActionsType) && actions != null)
            {
                IFieldData control = (Page as CamstarForm).GetControlByReference(Page as CamstarForm, new PERS.ValueReference { ConnectionType = PERS.DataMemberConnectionType.Control, Key = AvaliableActionsContainer }) as IFieldData;
                int rules = int.MaxValue;
                if (control != null && control.OriginalData != null)
                    rules = (int)control.OriginalData;
                Array.ForEach(actions, action =>
                {
                    if (!string.IsNullOrEmpty(action.Name))
                    {
                        if (Camstar.WebPortal.WCFUtilities.WCFObject.CreateObjectType(AvaliableActionsType).GetFields().FirstOrDefault(f => f.Name == action.Name) != null)
                        {
                            object v1rule = Enum.Parse(Camstar.WebPortal.WCFUtilities.WCFObject.CreateObjectType(AvaliableActionsType), action.Name);
                            if (v1rule != null)
                                action.IsDisabled = (rules & (int)v1rule) == 0;
                        }
                    }
                });
            }
        }

        public virtual void LoadActionsStatus()
        {
            Page.ActionDispatcher.LoadActionsStatus();
            Page.ActionDispatcher.SetActionsVisibility();
        }

        protected virtual bool RenderUIActions()
        {
            bool res = false;
            string serviceTypeName = string.Empty;
            bool timersConfirmationRequired = false;
            if (UIActions != null && IncludeHandlerActions)
            {
                Dictionary<string, OM.ActionCategoryChanges> categories = new Dictionary<string, OM.ActionCategoryChanges>();
                foreach (var action in UIActions)
                {
                    System.Web.UI.HtmlControls.HtmlGenericControl control = ContentControls.OfType<System.Web.UI.HtmlControls.HtmlGenericControl>().FirstOrDefault(c => c.Controls[0].ID == action.Name);
                    if (control != null)
                        ContentControls.Remove(control);
                    if (!string.IsNullOrEmpty(action.ActionCategory) && !categories.ContainsKey(action.ActionCategory))
                        categories.Add(action.ActionCategory, Camstar.WebPortal.Personalization.ActionCategoryCache.Instance.GetCategory(action.ActionCategory));
                }

                foreach (var action in UIActions.OrderBy(a => string.IsNullOrEmpty(a.ActionCategory) ? 0 : (int)categories[a.ActionCategory].Sequence))
                {
                    if (!(action.IsHidden ?? false))
                    {
                        serviceTypeName = string.Empty;
                        var submitAction = action as PERS.SubmitAction;
                        if (submitAction != null)
                        {
                            serviceTypeName = submitAction.ServiceName;
                            timersConfirmationRequired =
                                submitAction.TimersConfirmationRequired.HasValue &&
                                submitAction.TimersConfirmationRequired.Value;
                        }

                        var esig = CamstarPortalSection.Settings.DefaultSettings.ESignatureRequired;
                        var wips = CamstarPortalSection.Settings.DefaultSettings.WIPMessagesRequired;
                        if (submitAction == null || (action is PERS.RedirectAction && string.IsNullOrEmpty((action as PERS.RedirectAction).ServiceName)))
                            esig = wips = false;
                        if (action.ESignatureRequired != PERS.BooleanTriState.NotSet)
                            esig = action.ESignatureRequired == PERS.BooleanTriState.Yes;
                        if (action.WIPMessagesRequired != PERS.BooleanTriState.NotSet)
                            wips = action.WIPMessagesRequired == PERS.BooleanTriState.Yes;

                        var data = new CustomPageActionData(
                            null, null, null)
                            {
                                ActionName = action.Caption,
                                IsEsigActionEnabled = esig,
                                IsWIPActionEnabled = wips,
                                ActionID = action.Name,
                                ActionEnabled = !(action.IsDisabled ?? false),
                                IsPrimary = action.IsPrimary ?? true,
                                ServiceTypeName = serviceTypeName,
                                LabelName = null,
                                ActionGroupName = action.GroupName,
                                ActionCommandName = action.CommandName,
                                ActionCssClass = action.CssClass
                            };
                        data.IsTimersConfirmationRequired = timersConfirmationRequired;
                        if (action.Confirmation != null)
                        {
                            data.IsConfirmationRequired = true;
                        }

                        if (!action.IsServerAction)
                            data.AbsoluteRedirectLink = Page.ActionDispatcher.GetExecuteActionURL(action);

                        if (action.GetType() == typeof(PERS.FloatPageOpenAction))
                            data.VirtualPageName = (action as PERS.FloatPageOpenAction).PageName;
                        
                            
                        if (!string.IsNullOrEmpty(action.ActionCategory) && categories.ContainsKey(action.ActionCategory))
                        {
                            ContentControls.Add(CreateActionItem(new ActionSeparator { ActionName = (string)categories[action.ActionCategory].LabelText, ActionID = action.ActionCategory }, EventArgumentConstants.UIAction));
                            categories.Remove(action.ActionCategory);
                        }

                        //render hidden to avoid "changing controls" effect while editing DOM in 'SplitActionsToGroups' js method
                        var control = (ActionGroups != null && ActionGroups.Length > 0) ?
                            CreateActionItem(data, EventArgumentConstants.UIAction, true) : 
                            CreateActionItem(data, EventArgumentConstants.UIAction);

                        #region sidebarExtension
                        if (Page.IsSideCommandBar && control.Controls != null && action != null)
                        {
                            var btn = (control.Controls[control.Controls.Count - 1] as Button);
                            var dmode = PERS.CommandBarDisplayModes.TransPanel;

                            if (action.CommandBar != null)
                            {
                                btn.Attributes.Add("itemCSS", action.CommandBar.ItemCSS);
                                btn.Attributes.Add("imageCSS", action.CommandBar.ImageCSS);
                                btn.Attributes.Add("actionGroup", action.CommandBar.Group);
                                btn.Attributes.Add("actionOrder", (action.CommandBar.Order ?? 0) .ToString());
                                dmode = action.CommandBar.DisplayMode ?? PERS.CommandBarDisplayModes.TransPanel;
                            }

                            btn.Attributes.Add("displayMode", dmode.ToString());
                            btn.Attributes.Add("isPrimary", (action.IsPrimary??false).ToString().ToLowerInvariant());

                            if (action is PERS.CustomAction)
                                btn.Attributes.Add("customMethodHandler", (action as PERS.CustomAction).CustomMethodHandler);
                            btn.Attributes.Add("actionIndex", (action.Index ?? 0).ToString());
                        }
                        #endregion
                        if (control != null){
                            ContentControls.Add(control);
                            res = true;
                        }
                    }
                }
                if (ActionGroups != null && ActionGroups.Length > 0)
                {
                    foreach (var group in ActionGroups)
                    {
                        string selectedActions = string.Empty;
                        // TODO: add logic for CheckBox.
                        if (group.GroupBehaviour == PERS.GroupBehaviourType.RadioButton)
                        {
                            var group1 = group;
                            var groupActions = UIActions.Where(act => act.GroupName == group1.Name).Select(act => act.Name).ToList();
                            foreach (var action in new List<string>(SelectedActions).Where(groupActions.Contains))
                            {
                                if (!string.IsNullOrEmpty(selectedActions))
                                    SelectedActions.Remove(selectedActions); // remove previously selected radio button item.
                                selectedActions = action;
                            }
                            if (!string.IsNullOrEmpty(selectedActions))
                                selectedActions = string.Format("'{0}'", selectedActions);
                        }

                        var script = string.Format("SplitActionsToGroups('{0}', '{1}', '{2}', '{3}', '{4}', [{5}]);",
                            ClientID, group.Name, group.CssClass, (int)group.GroupBehaviour, (group.DisplayDelimeter ?? false) ? "1" : string.Empty, selectedActions);
                        ScriptManager.RegisterStartupScript(this, GetType(), group.Name, script, true);
                    }
                }
                else
                    SelectedActions.Clear();
            }
            return res;
        }

        protected virtual void GenerateWebPartActionsList(List<WebPartActionData> webPartsActions)
        {
            //Sort groups of items according to Provider`s title
            //webPartsActions.Sort((firstItem, secItem) => firstItem.ProviderTitle.CompareTo(secItem.ProviderTitle));

            int indexOf = 0;
            foreach (WebPartActionData data in webPartsActions)
            {
                if ((indexOf == 0) && (PageActions != null && PageActions.Count > 0))
                {
                    ContentControls.Add(CreateSeparatorControl(data.ProviderTitle));
                }
                else if (indexOf != 0)
                {
                    ContentControls.Add(CreateSeparatorControl(data.ProviderTitle));
                }

                foreach (ActionData webPartAction in data.WebPartActions)
                {
                    if (webPartAction != null)
                    {
                        if (webPartAction is HeaderActionData)
                            InitHeaderWebPartActions((webPartAction as HeaderActionData).Enabled);
                        else
                            ContentControls.Add(CreateActionItem(webPartAction, webPartAction.ProviderID ?? data.ProviderID));
                    }//if
                }//foreach
                indexOf++;
            }//foreach
        }//GenerateWebPartActionsList

        protected virtual Control CreateSeparatorControl(string caption)
        {
            DivElement header = new DivElement();
            header.AddCssClass("header");
            header.InnerText = caption;
            return header;
        }//CreateSeparatorControl

        protected virtual void InitHeaderWebPartActions(bool enabled)
        {
            if (_HeaderWebPartActionData != null && _HeaderWebPartActionData.WebPartActions.Count > 0)
            {
                List<ActionData> deleted = new List<ActionData>();
                foreach (ActionData action in _HeaderWebPartActionData.WebPartActions.Where(act => !(act is CustomerActionData)))
                {
                    action.ActionEnabled = action.ActionEnabled && enabled;
                    ContentControls.Add(CreateActionItem(action, _HeaderWebPartActionData.ProviderID));
                    deleted.Add(action);
                }
                foreach (ActionData action in deleted)
                    _HeaderWebPartActionData.WebPartActions.Remove(action);
            }
        }

        protected virtual void GeneratePageActionsList()
        {
            if (IncludeHandlerActions)
            {
                if (PageActions != null)
                {
                    foreach (ActionData handlerAction in PageActions)
                    {
                        if (handlerAction != null)
                        {
                            if (handlerAction is HeaderActionData)
                                InitHeaderWebPartActions((handlerAction as HeaderActionData).Enabled);
                            else
                                ContentControls.Add(CreateActionItem(handlerAction));
                        }
                    }
                }
            }
        }//GeneratePageActionsList

        protected virtual Control CreateActionItem(ActionData data)
        {
            return CreateActionItem(data, string.Empty);
        }

        protected virtual Control CreateActionItem(ActionData data, string providerID, bool renderHidden = false)
        {
            System.Web.UI.Control returnControl = null;
            if (data is ActionDataGroup)
            {
                ActionDataGroup group = data as ActionDataGroup;
                if (group.Actions != null)
                {
                    returnControl = CWF.ControlUtil.CreateDiv(string.Empty, "action");
                    string editViewSetMode = string.Empty;
                    string editViewInMode = string.Empty;
                    bool disabledGroup = false;
                    if (group is ActionModeDataGroup)
                    {
                        disabledGroup = ((ActionModeDataGroup)group).IsDisabled;
                        returnControl = CWF.ControlUtil.CreateDiv(string.Empty, "action-mode");
                        var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
                        if (labelCache != null)
                        {
                            var label = labelCache.GetLabelByName("EditViewSetMode");
                            if (label != null)
                                editViewSetMode = label.Value ?? "Set {0} Mode";
                            label = labelCache.GetLabelByName("EditViewInMode");
                            if (label != null)
                                editViewInMode = label.Value ?? "In {0} Mode";
                        }
                    }
                    var separator = new System.Web.UI.WebControls.Label();
                    separator.Text = (ActionsStyle == PERS.ActionsStyleType.Link) ? "|" : (HTMLConstants.BlankSpace + HTMLConstants.BlankSpace);
                    bool isFirstItem = true;
                    foreach (var action in group.Actions)
                    {
                        if (action.Action != null && !string.IsNullOrEmpty(action.ActionName))
                        {
                            if (!isFirstItem)
                                returnControl.Controls.Add(separator);
                            isFirstItem = false;
                            PERS.ActionsStyleType style = this.ActionsStyle;
                            this.ActionsStyle = action.ActionEnabled ? PERS.ActionsStyleType.Button : PERS.ActionsStyleType.Link;
                            System.Web.UI.WebControls.WebControl button = createHandlerButton(action, providerID);
                            if (group is ActionModeDataGroup)
                            {
                                if (this.ActionsStyle == PERS.ActionsStyleType.Button)
                                    (button as Camstar.WebPortal.FormsFramework.WebControls.Button).Text = string.Format(editViewSetMode, (button as Camstar.WebPortal.FormsFramework.WebControls.Button).Text);
                                else
                                    (button as Camstar.WebPortal.FormsFramework.WebControls.LinkButton).Text = string.Format(editViewInMode, (button as Camstar.WebPortal.FormsFramework.WebControls.LinkButton).Text);
                                if (disabledGroup)
                                    button.Enabled = false;
                            }
                            button.Style.Add("min-width", "100px");
                            if(renderHidden)
                                button.Style.Add("display", "none");
                            returnControl.Controls.Add(button);
                            ActionsStyle = style;
                        }
                    }
                }
            }

            else if (data is ActionSeparator)
            {
                returnControl = CreateSeparatorControl(data.ActionName);
            }

            else if (data is CustomPageActionData)
            {
                if (!string.IsNullOrEmpty(data.ActionName))
                {
                    returnControl = CWF.ControlUtil.CreateDiv(string.Empty, "action");
                    System.Web.UI.WebControls.WebControl button = createHandlerButton(data, providerID);
                    if (renderHidden)
                        button.Style.Add("display", "none");
                    returnControl.Controls.Add(button);
                }
            }
            else
            {
                if (data.Action != null && !string.IsNullOrEmpty(data.ActionName))
                {
                    returnControl = CWF.ControlUtil.CreateDiv("box_" + data.ActionID, "action");
                    System.Web.UI.WebControls.WebControl button = createHandlerButton(data, providerID);
                    if (renderHidden)
                        button.Style.Add("display", "none");
                    returnControl.Controls.Add(button);
                }
            }
            return returnControl;
        }

        protected virtual System.Web.UI.WebControls.WebControl createHandlerButton(ActionData data, string providerID)
        {
            var labelText = data.ActionName;
            var labelCache = FrameworkManagerUtil.GetLabelCache(System.Web.HttpContext.Current.Session);
            if (!string.IsNullOrEmpty(data.LabelName))
            {
                if (labelCache != null)
                {
                    var label = labelCache.GetLabelByName(data.LabelName);
                    if (label != null)
                        labelText = label.Value ?? labelText;
                }
            }

            System.Web.UI.WebControls.WebControl handlerButton;
            if (ActionsStyle == PERS.ActionsStyleType.Button)
            {
                handlerButton = new Camstar.WebPortal.FormsFramework.WebControls.Button
                {
                    Text = labelText,
                    ToolTip = labelText,
                    CssClass = null
                };
                if (!string.IsNullOrEmpty(data.ActionGroupName))
                    handlerButton.Attributes.Add("actionGroupName", data.ActionGroupName);
                if (!string.IsNullOrEmpty(data.ActionCommandName))
                    handlerButton.Attributes.Add("actionCommandName", data.ActionCommandName);
                if (!string.IsNullOrEmpty(data.ActionCssClass))
                    handlerButton.CssClass = data.ActionCssClass;
            }
            else
            {
                //handlerButton = new System.Web.UI.WebControls.Label {Text = labelText};
                handlerButton = new Camstar.WebPortal.FormsFramework.WebControls.LinkButton
                {
                    Text = labelText
                };
            }

            //remove any empty spaces from the button id name so the control can be referenced from jquery
            handlerButton.ID = data.ActionID.Replace(" ", "_");

            if (data.ActionEnabled)
            {
                if (string.IsNullOrEmpty(handlerButton.CssClass))
                {
                switch (ActionsStyle)
                {
                    case PERS.ActionsStyleType.Link: handlerButton.CssClass = "ActionsItem"; break;
                    case PERS.ActionsStyleType.Button: handlerButton.CssClass =
                        (data.IsPrimary ? ThemeConstants.CamstarButton : ThemeConstants.CamstarButtonSecondary); break;
                }
                }

                StringBuilder argbuilder = new StringBuilder(data.ActionID);
                if (!String.IsNullOrEmpty(providerID))
                {
                    argbuilder.Append(EventArgumentConstants.ArgumentDelimeter);
                    argbuilder.Append(providerID);
                    if ((data is FloatingFrameActionData) && ((data as FloatingFrameActionData).FloatingFrameParameters != null))
                    {
                        argbuilder.Append(EventArgumentConstants.ArgumentDelimeter);
                        argbuilder.Append(EventArgumentConstants.AjaxEventShowFloatingFrame);
                        argbuilder.Append(EventArgumentConstants.ArgumentDelimeter);
                        argbuilder.Append((data as FloatingFrameActionData).FloatingFrameParameters.FrameURL);
                        argbuilder.Append(EventArgumentConstants.ArgumentDelimeter);
                        argbuilder.Append((data as FloatingFrameActionData).FloatingFrameParameters.FrameTitle);
                        argbuilder.Append(EventArgumentConstants.ArgumentDelimeter);
                        argbuilder.Append((data as FloatingFrameActionData).FloatingFrameParameters.FrameHeight);
                        argbuilder.Append(EventArgumentConstants.ArgumentDelimeter);
                        argbuilder.Append((data as FloatingFrameActionData).FloatingFrameParameters.FrameWidth);
                        argbuilder.Append(EventArgumentConstants.ArgumentDelimeter);
                        argbuilder.Append((data as FloatingFrameActionData).FloatingFrameParameters.FrameTopLocation);
                        argbuilder.Append(EventArgumentConstants.ArgumentDelimeter);
                        argbuilder.Append((data as FloatingFrameActionData).FloatingFrameParameters.FrameLeftLocation);
                        argbuilder.Append(EventArgumentConstants.ArgumentDelimeter);
                        argbuilder.Append((data as FloatingFrameActionData).FloatingFrameParameters.IsFloatingFrameButtonsVisible.ToString().ToLower());
                    }
                    else
                    {
                        if (data.IsTimersConfirmationRequired)
                        {
                            argbuilder.Append(EventArgumentConstants.ArgumentDelimeter);
                            argbuilder.Append(EventArgumentConstants.IsTimersConfirmationRequired);
                        }
                        if (data.IsWIPActionEnabled)
                        {
                            argbuilder.Append(EventArgumentConstants.ArgumentDelimeter);
                            argbuilder.Append(EventArgumentConstants.AjaxEventWIPMessages);
                        }
                        if (data.IsEsigActionEnabled)
                        {
                            if (!string.IsNullOrEmpty(data.ActionCommandName))
                            {
                            argbuilder.Append(EventArgumentConstants.ArgumentDelimeter);
                                argbuilder.Append(EventArgumentConstants.ESigMaintAction);
                                argbuilder.Append(EventArgumentConstants.ArgumentDelimeter);
                                argbuilder.Append(data.ActionCommandName);
                            }
                            argbuilder.Append(EventArgumentConstants.ArgumentDelimeter);
                            argbuilder.Append(EventArgumentConstants.AjaxEventESigRequirement);
                        }
                        if (data.IsEsigActionEnabled || data.IsWIPActionEnabled)
                        {
                            if (!string.IsNullOrEmpty(data.ServiceTypeName))
                            {
                                argbuilder.Append(EventArgumentConstants.ArgumentDelimeter);
                                argbuilder.Append(data.ServiceTypeName);
                            }
                        }
                    }
                }

                if (data.GetType() == typeof(CustomPageActionData))
                    argbuilder.Append(AppendArgs((CustomPageActionData)data));
                

                if (data.IsConfirmationRequired && !String.IsNullOrEmpty(data.ConfirmationText))
                {
                    string dialogTitle = labelCache.GetLabelByName("Lbl_Information").Value;
                    string postbackFunction = Page.ClientScript.GetPostBackEventReference(this, argbuilder.ToString());
                    handlerButton.Attributes[HTMLConstants.OnClick] = JavascriptUtil.GetConfirmationCall(data.ConfirmationText, postbackFunction, dialogTitle);
                }//if
                else if (data.IsScriptRequired && !string.IsNullOrEmpty(data.Script))
                {
                    handlerButton.Attributes[HTMLConstants.OnClick] = data.Script;
                }
                else
                {
                    if (string.IsNullOrEmpty(data.AbsoluteRedirectLink))
                    {
                        handlerButton.Attributes[HTMLConstants.OnClick] = "return WaitForGridOpCompletionOnSave('" +
                                                                          UniqueID + "','" + argbuilder + "','" +
                                                                          ClientID + "');";
                        var newLabel = labelCache.GetLabelByName("MaintBtnNew");
                        var newLabelValue = newLabel != null ? newLabel.Value : "New";
                        if (data.ActionName == newLabelValue || data.ActionName == "Copy")
                            handlerButton.Attributes[HTMLConstants.OnClick] = " return DirtyFlagforNew('" +
                                                                          UniqueID + "','" + argbuilder + "','" +
                                                                          ClientID + "');";
                    }
                    else
                        handlerButton.Attributes[HTMLConstants.OnClick] = data.AbsoluteRedirectLink;

                    if (data.IsConfirmationRequired)
                    {
                        var uiAction = UIActions.FirstOrDefault(ac => ac.Name == data.ActionID);
                        if (uiAction != null)
                        {
                            var confScript = ControlUtil.MakeConfirmation(uiAction.Confirmation, labelCache, handlerButton.Attributes[HTMLConstants.OnClick]);
                            handlerButton.Attributes[HTMLConstants.OnClick] = confScript;
                        }
                    }
                }
            }
            else
            {
                handlerButton.Enabled = false;
                switch (ActionsStyle)
                {
                    case PERS.ActionsStyleType.Link: handlerButton.CssClass = "action-disabled"; break;
                    case PERS.ActionsStyleType.Button:
                        if (string.IsNullOrEmpty(data.ActionCssClass))
                            handlerButton.CssClass = (data.IsPrimary ? ThemeConstants.CamstarButton : ThemeConstants.CamstarButtonSecondary); break;
                }
                handlerButton.Attributes[HTMLConstants.OnSelectStart] = "return false;";
            }

            if(handlerButton is IFocusableControl)
                _focusableButtons.Add(handlerButton as IFocusableControl);

            return handlerButton;
        }

        /// <summary>
        /// This function is intended to be overridden in any subclass of ActionsControl so any additional arguments needed by a subclass can be inserted
        /// during the createHandlerButton functionality
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual string AppendArgs(CustomPageActionData data)
        {
            return string.Empty;
        }

        protected override void CreateContentControls(ControlCollection contentControls)
        {
            ContentControls = contentControls;
        }

        public virtual void GetPageActionsList()
        {
            if (IncludeHandlerActions)
            {
                PageActions = Page.GetPageActions();
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            Page.LoadComplete += Page_LoadComplete;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Page.MergedContent.Behavior == PERS.PageBehaviorType.Modeling)
                Page.RenderActions += Page_RenderActions;

        }

        protected virtual void Page_RenderActions(object sender, EventArgs e)
        {
            RenderUIActions();
            Update();
        }

        protected virtual void Page_LoadComplete(object sender, EventArgs e)
        {
            if ((this as System.Web.UI.WebControls.WebParts.WebPart).Page == null)
                return;

            // get Personalization Actions
            Page.PreparePageActions();
            UIActions = Page.ActionDispatcher.ActionPanelActions();
            ActionGroups = Page.ActionDispatcher.ActionGroups;

            CWF.CallStack callStack = new CWF.CallStack();
            CWF.CallStackStateBase state = callStack.CurrentState;
            bool reloadActions = true;

            if (state != null && (state is RecordViewPageState /*&& (state as RecordViewPageState).ProcessModelState != null*/))
            {
                //Actions list for Process Model is to be reloaded on QualityRecordViewHandlerBase.RestoreState
                //No need to reload Actions list for Quality Object here
                reloadActions = false;
            }

            if (!_actionsLoaded && reloadActions)
            {
                this.GetPageActionsList();
            }
            ApplyToActions(UIActions);
        }

        public virtual void RaisePostBackEvent(string eventArgument)
        {
            if (!String.IsNullOrEmpty(eventArgument))
            {
                this.GetPageActionsList();
                _actionsLoaded = true;
                OnActionClick(eventArgument);
            }
        }

        protected virtual void OnActionClick(string eventArgument)
        {
            string[] args = eventArgument.Split(EventArgumentConstants.ArgumentDelimeter);

            if (args.Length > 1)
            {
                if (!SelectedActions.Contains(args[0]))
                    SelectedActions.Add(args[0]);
                if (!String.IsNullOrEmpty(args[0]) && !String.IsNullOrEmpty(args[1]))
                {
                    if (args[1] == EventArgumentConstants.UIAction)
                        Page.ActionDispatcher.ExecuteAction(args[0]);
                    else if (_HeaderWebPartActionData != null && _HeaderWebPartActionData.ProviderID == args[1])
                        InvokeAction(args[0], _HeaderWebPartActionData.WebPartActions);
                    else if (Actions != null && Actions.Count > 0)
                    {
                        WebPartActionData invokedGroup = Actions.Find(data => data.ProviderID == args[1]);
                        if (invokedGroup.WebPartActions != null && invokedGroup.WebPartActions.Count > 0)
                        {
                            InvokeAction(args[0], invokedGroup.WebPartActions);
                        }
                    }
                }
            }
            else if (args.Length == 1)
            {
                if (!SelectedActions.Contains(eventArgument))
                    SelectedActions.Add(eventArgument);
                if (PageActions != null && PageActions.Count > 0)
                {
                    InvokeAction(eventArgument, PageActions);
                }
            }
        }

        public override IEnumerable<IFocusableControl> GetFocusableControls()
        {
            var removeList = _focusableButtons.OfType<Button>().Where(b => !b.Enabled);
            return _focusableButtons.Except(removeList);
        }

        protected virtual bool InvokeAction(string eventArgument, List<ActionData> actionsList)
        {
            bool actionInvoked = false;
            ActionData data = actionsList.Find(item => item.ActionID == eventArgument);
            if (data != null && data.Action != null)
            {
                data.Action.Invoke(this, new WebPartActionEventArgs(data));
                actionInvoked = true;
            }
            else
            {
                foreach (ActionData actionData in actionsList)
                {
                    if (actionData is ActionDataGroup)
                        InvokeAction(eventArgument, (actionData as ActionDataGroup).Actions);
                }
            }

            return actionInvoked;
        }

        private bool _actionsLoaded;
        private HeaderWebPartActionData _HeaderWebPartActionData;
        private List<IFocusableControl> _focusableButtons = new List<IFocusableControl>();
    }

    public class WebPartActionsEventArgs : EventArgs
    {
        public WebPartActionsEventArgs(List<ActionData> actionList)
        {
            _actions = actionList;
        }

        public WebPartActionsEventArgs()
        {
        }

        public virtual List<ActionData> WebPartActions
        {
            set { _actions = value; }
            get { return _actions; }
        }

        protected List<ActionData> _actions;
    }//WebPartActionsEventArgs

    public class WebPartActionEventArgs : EventArgs
    {
        public WebPartActionEventArgs(ActionData actionData)
        {
            ActionData = actionData;
        }

        public WebPartActionEventArgs()
        {
        }

        public virtual ActionData ActionData { get; set; }
    }
}
