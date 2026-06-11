// Copyright Siemens 2025
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Camstar.Util;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.HtmlControls;
using Camstar.WebPortal.Utilities;


namespace Camstar.WebPortal.WebPortlets
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:StatusBarControl runat=server></{0}:StatusBar>")]
    public class StatusBarControl : WebPartBase, IStatusBarAccessor, IWebPartNoConnect, IFocusableControl
    {
        /// <summary>
        /// Status Bar states
        /// </summary>
        public enum StatusBarMessageType
        {
            Error, Warning, Success
        } // StatusBarMessageType

        #region Constructors

        public StatusBarControl()
        {
            Title = "Status Bar";

            StatusContent = new MessageLabel(this);
            InitializeStyles();
            MessageType = StatusBarMessageType.Success;
            KeepVisible = true;
        }

        #endregion

        #region Public Properties

        public virtual string Message
        {
            get { return StatusContent.Text; }
            set { StatusContent.Text = value; }
        } // Message

        public virtual bool KeepVisible { get; set; }

        #endregion

        #region Internal Properties

        public virtual StatusBarMessageType MessageType { get; set; }

        #endregion

        #region Public Methods

        public virtual void ClearMessage()
        {
            KeepVisible = false;
            Write(StatusBarMessageType.Success, string.Empty);
        }
        public virtual void WriteError(string message)
        {
            Write(StatusBarMessageType.Error, message);
        } // WriteError

        public virtual void WriteWarning(string message)
        {
            Write(StatusBarMessageType.Warning, message);
        } // WriteWarning

        public virtual void WriteSuccess(string message)
        {
            // success message will be displayed on the parent page when submitting transaction from child.
            if (!Page.IsChild || Page.ProcessingContext == null || Page.ProcessingContext.Status != ProcessingStatusType.SubmitTransaction)
                Write(StatusBarMessageType.Success, message);
        } // WriteSuccess

        public virtual void WriteStatus(Camstar.WCF.ObjectStack.ResultStatus status)
        {
            Write(status);
        } // WriteSuccess

        public virtual void Write(Camstar.WCF.ObjectStack.ResultStatus status)
        {
            if (status != null)
            {
                if (status is ValidationStatus)
                {
                    WriteValidationWarning(status as ValidationStatus);
                }
                else
                {
                    int aceStatusInt;
                    if (!string.IsNullOrEmpty(status.ACEMessage) && !string.IsNullOrEmpty(status.ACEStatus) && int.TryParse(status.ACEStatus, out aceStatusInt) && aceStatusInt != 1)
                        WriteWarning(status.ACEMessage);
                    else
                    {
                        if (status.IsSuccess)
                        {
                            if (!string.IsNullOrEmpty(status.ToString()) || MessageType != StatusBarMessageType.Success || string.IsNullOrEmpty(Message))
                                WriteSuccess(status.ToString());
                        }
                        else
                            WriteError(status.ToString());
                    }
                }
            }
            else
            {
                Write(StatusBarMessageType.Success, string.Empty);
            }
        } // WriteSuccess

        public virtual void Write(StatusBarMessageType messageType, string message)
        {
            StatusContent.MessageType = messageType;
            if (string.IsNullOrEmpty(message))
                KeepVisible = false;
            MessageType = messageType;
            Message = DateUtil.ApplyUserLocaleSettings(HttpUtility.HtmlEncode(message), Page.Session[SessionConstants.ServerDateFormat] as string, Page.Session[SessionConstants.ServerTimeFormat] as string);
            // Recreate child controls
            ChildControlsCreated = false;
            EnsureChildControls();

            RenderToClient = true;
        } // Write


        #endregion

        #region Protected Methods

        protected virtual void InitializeStyles()
        {
            // Initialize styles
            this.CssClass = ThemeConstants.WebpartStatus;


        } // StatusBarControl

        /// <summary>
        /// Create StatusBar wrapper
        /// </summary>
        /// <returns></returns>
        protected override WebPartWrapperBase CreateWebPartWrapper()
        {
            return null;
        } // CreateWebPartWrapper

        protected override void CreateContentControls(ControlCollection contentControls)
        {
            Attributes.Add("messageType", MessageType.ToString());

            contentControls.Add(StatusContent as Control);

            HtmlInputHidden hidMessageExists = new HtmlInputHidden();
            hidMessageExists.ID = "StatusMessageExists";
            hidMessageExists.Value = StatusContent != null && !string.IsNullOrEmpty(StatusContent.Text) ? "1" : "0";
            contentControls.Add(hidMessageExists);

            _closeButton = new HtmlAnchor() { ID = "CloseStatusButton" };
            _closeButton.Attributes.Add("class", "close");
            _closeButton.Name = "CloseStatusButton";
            contentControls.Add(_closeButton);
            _closeButton.Attributes.Add("onclick", $"$('#{_closeButton.ClientID}').parent().clearQueue().stop(true, true).hide();");

            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);
            labelCache.GetLabelTextByName("StatusMessage_Error", val => { });
            labelCache.GetLabelTextByName("StatusMessage_Success", val => { });
            labelCache.GetLabelTextByName("StatusMessage_Warning", val => { });

            var statusSettings = CamstarPortalSection.Settings.StatusMessageSettings;
            if (statusSettings != null && statusSettings.PlaySound)
            {
                var audioControl = new HtmlGenericControl("audio") { ID = "audioSoundControl" };
                contentControls.Add(audioControl);
            }
        }

        protected virtual void WriteValidationWarning(ValidationStatus validation)
        {
            if (validation != null)
            {
                MessageType = StatusBarMessageType.Warning;
                StatusContent = new ValidationLabel(validation, this);

                // Recreate child controls
                ChildControlsCreated = false;
                EnsureChildControls();
            }

            this.RenderToClient = true;
        }

        protected override void OnPreRender(EventArgs e)
        {
            //should clear the previous error message, if the current postback doesn't output any message
            bool clearPreviousMessage = Page.IsPostBack && !RenderToClient && !string.IsNullOrEmpty(Message) && !KeepVisible;
            if (clearPreviousMessage)
                ClearMessage();

            if (_messagePanel != null && !_messagePanel.Controls.Contains(StatusContent as Control))
                _messagePanel.Controls.Add(StatusContent as Control);

            Hidden = false;

            if (!Page.IsPostBack)
            {
                RenderToClient = true;
            }

            base.OnPreRender(e);

            //startup script should be formed after OnPreRender have been done for ever control on the page.
            //note: picklist performs ValidateFreeFormTextEntry during PreRender
            this.Page.PreRenderComplete += delegate { RegisterStartupScripts(); };
        }

        protected virtual void RegisterStartupScripts()
        {
            if (!string.IsNullOrEmpty(StatusContent.Text))
            {
                string script = string.Format("$(function() {{ ShowPopupMessage('{0}', {1}); }});", UIComponentID, Page.IsPostBack.ToString().ToLower());
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ToggleStatusBar", script, true);
            }
            else
            {
                string script = string.Format("$(function() {{var o=$find('{0}'); if(o!=null)o.clear();}});", UIComponentID);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ClearMessage", script, true);
            }
        }

        protected override string ClientControlTypeName
        {
            get { return "Camstar.WebPortal.WebPortlets.StatusBar"; }
        }

        public override IEnumerable<IFocusableControl> GetFocusableControls()
        {
            yield return this;
        }

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            List<ScriptReference> list = base.GetScriptReferences().ToList();

            list.Add(new ScriptReference("~/Scripts/ClientFramework/Camstar.WebPortal.WebPortlets/StatusBar.js"));
            return list;
        }

        protected override IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            var list = base.GetScriptDescriptors().ToList();
            ScriptControlDescriptor descriptor = null;
            if (list.Count > 0)
                descriptor = list[0] as ScriptControlDescriptor;
            if (descriptor != null)
            {
                descriptor.AddProperty("errorMessageFadeOutTime", CamstarPortalSection.Settings.DefaultSettings.ErrorMessageFadeOutTime);
                descriptor.AddProperty("infoMessageFadeOutTime", CamstarPortalSection.Settings.DefaultSettings.InfoMessageFadeOutTime);
                descriptor.AddProperty("warningMessageFadeOutTime", CamstarPortalSection.Settings.DefaultSettings.WarningMessageFadeOutTime);
                var statusSettings = CamstarPortalSection.Settings.StatusMessageSettings;
                var playSound = false;
                string warningSound = "", successSound = "", errorSound = "";
                if (statusSettings != null)
                {
                    playSound = statusSettings.PlaySound;
                    var userSoundsDir = string.Format("{0}/{1}", FolderConstants.Sounds, FolderConstants.UserResource);
                    warningSound = string.Format("{0}/{1}", userSoundsDir, statusSettings.WarningSoundPath);
                    successSound = string.Format("{0}/{1}", userSoundsDir, statusSettings.SuccessSoundPath);
                    errorSound = string.Format("{0}/{1}", userSoundsDir, statusSettings.ErrorSoundPath);
                }
                descriptor.AddProperty("playSound", playSound);
                descriptor.AddProperty("warningSound", warningSound);
                descriptor.AddProperty("successSound", successSound);
                descriptor.AddProperty("errorSound", errorSound);
            }
            return list;
        }

        #endregion



        #region IFocusableControl Members

        IEnumerable<string> IFocusableControl.GetOrderedFocusableControlIDs()
        {
            var s = new List<string>();
            if (_closeButton != null)
            {
                s.Add(_closeButton.ClientID);
            }

            if (StatusContent is ValidationLabel)
            {
                s.AddRange((StatusContent as ValidationLabel).GetOrderedFocusableControlIDs());
            }
            return s;
        }

        #endregion

        #region Private Member Variables

        private Panel _messagePanel = null;
        protected virtual IStatusMessageContent StatusContent { get; set; }
        private HtmlAnchor _closeButton = null;

        #endregion

    } // StatusBar

    public interface IStatusMessageContent
    {
        string Text { get; set; }
        StatusBarControl.StatusBarMessageType MessageType { get; set; }
    }

    /// <summary>
    /// Custom label is used to show status message.
    /// </summary>
    internal class MessageLabel : WebControl, IStatusMessageContent
    {
        public MessageLabel(StatusBarControl owner)
            : base(HtmlTextWriterTag.Div)
        {
            _owner = owner;
        } // TextLabel

        StatusBarControl _owner;

        private StatusBarControl.StatusBarMessageType _type = StatusBarControl.StatusBarMessageType.Success;
        private bool _typeSet = false;
        public StatusBarControl.StatusBarMessageType MessageType
        {
            get
            {
                if (_typeSet)
                    return _type;
                else
                    return _owner.MessageType;
            }
            set
            {
                _type = value;
                _typeSet = true;
            }
        }

        public virtual string Text
        {
            get { return ViewState["Text"] as string; }
            set { ViewState["Text"] = value; }
        } // Text

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            base.RenderBeginTag(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
        } // RenderBeginTag

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            writer.RenderEndTag();
            base.RenderEndTag(writer);
        } // RenderEndTag

        protected override void RenderContents(HtmlTextWriter writer)
        {
            string messageType = string.Empty;
            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);

            string msgClass = "";
            switch (MessageType)
            {
                case StatusBarControl.StatusBarMessageType.Error:
                    messageType = labelCache.GetLabelByName("StatusMessage_Error").Value;
                    msgClass = " noty_type_error";
					if (string.IsNullOrEmpty(messageType))
						messageType = "error";
                    break;
                case StatusBarControl.StatusBarMessageType.Success:
                    messageType = labelCache.GetLabelByName("StatusMessage_Success").Value;
					if (string.IsNullOrEmpty(messageType))
						messageType = "success";
                    break;
                case StatusBarControl.StatusBarMessageType.Warning:
                    messageType = labelCache.GetLabelByName("StatusMessage_Warning").Value;
					if (string.IsNullOrEmpty(messageType))
						messageType = "warning";
                    msgClass = " noty_type_warning";
                    break;
            }
            string text = string.IsNullOrEmpty(Text) ? "  " : Text;
            writer.Write(string.Format("<div class=\"message {0}\" id=\"divStatusMessage\"><span class=\"message-status-type\">{1}</span><span class=\"instruction noty_text\" id=\"SpanStatusMessage\">{2}</span></div>", msgClass, messageType, text));

            /*switch (_owner.MessageType)
            {
                case StatusBarControl.StatusBarMessageType.Error:
                    messageType = labelCache.GetLabelByName("StatusMessage_Error").Value; break;
                case StatusBarControl.StatusBarMessageType.Success:
                    messageType = labelCache.GetLabelByName("StatusMessage_Success").Value; break;
                case StatusBarControl.StatusBarMessageType.Warning:
                    messageType = labelCache.GetLabelByName("StatusMessage_Warning").Value; break;
            }

            writer.Write(string.Format("<div class=\"message\"><span class=\"messageType\">{0}!&nbsp;</span><span class=\"instruction\">{1}</span></div>", messageType, string.IsNullOrEmpty(Text) ? "&nbsp;" : Text));*/
        } // RenderContents
    } // MessageLabel

    internal class ValidationLabel : WebControl, IStatusMessageContent, IFocusableControl
    {
        public event EventHandler<PageflowProgressEventArgs> Navigate;

        public ValidationLabel(ValidationStatus validation, StatusBarControl owner)
            : base(HtmlTextWriterTag.Div)
        {
            _validation = validation;
            _owner = owner;
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Div div = new Div();
            div.Attributes.Add("id", "divMessage");
            div.CssClass = "message noty_type_warning";

            SpanElement span = new SpanElement();
            span.Attributes.Add("id", "spanMessage");
            span.Attributes.Add("style", "display: none");

            UnorderedListElement ul = new UnorderedListElement();
            ListItemElement li = new ListItemElement();
            ul.ListItems.Add(li);

            string messageType = string.Empty;
            LabelCache labelCache = FrameworkManagerUtil.GetLabelCache(Page.Session);

            switch (_owner.MessageType)
            {
                case StatusBarControl.StatusBarMessageType.Error:
                    messageType = labelCache.GetLabelByName("StatusMessage_Error").Value; break;
                case StatusBarControl.StatusBarMessageType.Success:
                    messageType = labelCache.GetLabelByName("StatusMessage_Success").Value; break;
                case StatusBarControl.StatusBarMessageType.Warning:
                    messageType = labelCache.GetLabelByName("StatusMessage_Warning").Value; break;
                default: break;

            }

            li.InnerText = string.Format("{0}", messageType);

            var items = _validation.ToList();
            if (_validation.Any(item => item.IsPageFlowRequired))
                items = _validation.Where(item => item.IsPageFlowRequired).ToList();

            foreach (ValidationStatusItem item in items)
            {
                li = new ListItemElement();
                ul.ListItems.Add(li);
                li.InnerHtml = string.Format("- {0}", item);
				if (!string.IsNullOrEmpty(item.ValidationMessage))
					span.InnerText += $"- {item.ValidationMessage}";
				else
					span.InnerText += $"- {item}";
            }

            if (Page is WebPartPageBase)
                (Page as WebPartPageBase).PortalContext.LocalSession[Camstar.WebPortal.Constants.SessionConstants.MarkedFields] = _validation.OfType<RequiredFieldStatusItem>().Select(s => s.ID).ToArray();

            div.Controls.Add(ul);
            div.Controls.Add(span);
            Controls.Add(div);
        }

        protected virtual void OnNavigate(PageflowProgressEventArgs e)
        {
            if (Navigate != null)
                Navigate(this, e);
        }

        public virtual string Text
        {
            get { return "Text"; }
            set { }
        } // Text

        private StatusBarControl.StatusBarMessageType _type = StatusBarControl.StatusBarMessageType.Success;
        private bool _typeSet = false;
        public StatusBarControl.StatusBarMessageType MessageType
        {
            get
            {
                if (_typeSet)
                    return _type;
                else
                    return _owner.MessageType;
            }
            set
            {
                _type = value;
                _typeSet = true;
            }
        }
        #region IFocusableControl Members

        public IEnumerable<string> GetOrderedFocusableControlIDs()
        {
            return
                from v in _validation
                from s in v.GetOrderedFocusableControlIDs()
                select s;
        }

        #endregion

        ValidationStatus _validation;
        StatusBarControl _owner;
    }
}
