// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Camstar.WebPortal.Constants;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.HtmlControls;
using Camstar.WebPortal.PortalFramework;
using CamstarWebControls = Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets
{
    [PortalStudio(false)]
    public class ButtonsBarBase : WebPartBase, IButtonsBarBase, IWebPartNoConnect 
    {
        public event ButtonsBarDelegate ButtonAdded;
        public event ButtonsBarDelegate ButtonRemoved;

        public ButtonsBarBase()
        {
            Title = "Buttons Bar";
            CssClass = "webpart-buttonbar";
            focusableButtons = new List<IFocusableControl>();
            PersonalizationEnabled = false;

        } // ButtonsBarControl

        protected virtual Table ButtonsTable { get; private set; }

        public virtual void AddButtonToRightPane(IButton button)
        {
            AddButton(button);
        }

        public virtual void AddButtonToLeftPane(IButton button)
        {
            AddButton(button);
        }

        public virtual bool HasButtons
        {
            get
            {
                var buttonControls = new List<CamstarWebControls.Button>();
                buttonControls.AddRange(Controls.OfType<CamstarWebControls.Button>());
                if (LeftPane != null)
                    buttonControls.AddRange(LeftPane.Controls.OfType<CamstarWebControls.Button>());
                if (RightPane != null)
                    buttonControls.AddRange(RightPane.Controls.OfType<CamstarWebControls.Button>());

                // Hide button's bar if all buttons invisible.
                return buttonControls.Any(ctrl => ctrl != null && ctrl.Visible && !ctrl.Hidden);
            }
        }

        protected override WebPartWrapperBase CreateWebPartWrapper()
        {
            return null;
        }

        public new Control Control
        {
            get { return this; }
        }

        protected override void CreateContentControls(ControlCollection contentControls)
        {
        } // CreateContentControls

        protected virtual void OnButtonAdded(IButton button)
        {
            RenderToClient = true;

            if (ButtonAdded != null)
                ButtonAdded(this, new BunttonsBarEventArgs(button));

            if(button is IFocusableControl)
                focusableButtons.Add(button as IFocusableControl);
        }

        protected virtual void OnButtonRemoved(IButton button)
        {
            RenderToClient = true;

            if (ButtonRemoved != null)
                ButtonRemoved(this, new BunttonsBarEventArgs(button));

            if (focusableButtons.Contains(button as IFocusableControl))            
                focusableButtons.Remove(button as IFocusableControl);
            
        }

        /// <summary>
        /// Adds button to the Buttons bar.
        /// The button will be always visible inside the bar.
        /// </summary>
        /// <param name="button"></param>
        public virtual void AddButton(IButton button)
        {
            if (button != null)
            {
                AddButton(this, button);
            }
        } // AddButton

        /// <summary>
        /// Protected method is used to add button as a child for a specific control
        /// </summary>
        /// <param name="owner">Owner control</param>
        /// <param name="button">Button control</param>
        protected virtual void AddButton(Control owner, IButton button)
        {
            owner.Controls.AddAt(0, button as Control);
            OnButtonAdded(button);
        } // AddButton

        /// <summary>
        /// Removes cell that contains button with the specified id from the Buttons bar
        /// </summary>
        /// <param name="id">Id of the button to remove</param>
        public virtual void RemoveButton(string id)
        {
            var buttonControls = new List<CamstarWebControls.Button>();
            buttonControls.AddRange(Controls.OfType<CamstarWebControls.Button>());
            if (LeftPane != null)
                buttonControls.AddRange(LeftPane.Controls.OfType<CamstarWebControls.Button>());
            if (RightPane != null)
                buttonControls.AddRange(RightPane.Controls.OfType<CamstarWebControls.Button>());

            foreach (CamstarWebControls.Button button in buttonControls)
            {
                if (String.Compare(button.ID, id, false) == 0)
                {
                    var container = button.Parent;
                    container.Controls.Remove(button);
                    OnButtonRemoved(button);
                    break;
                }
            }//foreach
        } // RemoveButton

        public virtual DivElement LeftPane
        {
            get { return null; }
        }

        public virtual DivElement RightPane
        {
            get { return null; }
        }

        public override IEnumerable<IFocusableControl> GetFocusableControls()
        {
            return focusableButtons;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //if (!HasButtons)
            //    CssClass = "ui-form-invisible";

            if (!Page.IsPostBack)
                RenderToClient = true;

            if (RenderToClient)
                Page.ClientScript.RegisterArrayDeclaration("ButtonsBarControlParams", "'" + ClientID + "'");
        } // OnPreRender


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected List<IFocusableControl> focusableButtons;

    } // ButtonsBarControl
}
