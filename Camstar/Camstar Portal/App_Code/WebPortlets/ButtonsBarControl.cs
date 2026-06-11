// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.HtmlControls;
using Camstar.WebPortal.PortalFramework;

namespace Camstar.WebPortal.WebPortlets
{
    /// <summary>
    /// The class is only need to implement IButtonsBarAccessor
    /// </summary>
    [PortalStudio("Buttons Bar")]
    public class ButtonsBarControl : ButtonsBarBase, IButtonsBarAccessor
    {
        public override void Update()
        {
            _recreationRequired = true;
        } // Update

        protected override void CreateContentControls(ControlCollection contentControls)
        {
            base.CreateContentControls(contentControls);
            LeftPane.AddCssClass("left");
            RightPane.AddCssClass("right");

            Controls.Add(LeftPane);
            Controls.Add(RightPane);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if(_recreationRequired)
            {
                ChildControlsCreated = false;
                EnsureChildControls();

                base.Update();
            }

            base.OnPreRender(e);
        }

        protected override string ClientControlTypeName
        {
            get { return "Camstar.WebPortal.WebPortlets.ButtonsBarControl"; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            List<ScriptReference> refs = base.GetScriptReferences().ToList<ScriptReference>();

            refs.Add(new ScriptReference("~/Scripts/ClientFramework/Camstar.WebPortal.WebPortlets/ButtonsBarControl.js"));

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

        // By deault add button into the right pane
        public override void AddButton(IButton button)
        {
            AddButton(RightPane, button);
        }

        public override DivElement RightPane
        {
            get { return _rightPane; }
        }

        public override DivElement LeftPane
        {
            get { return _leftPane; }
        }

        private DivElement _leftPane = new DivElement();
        private DivElement _rightPane = new DivElement();
        bool _recreationRequired = false;
    } // ButtonsBarControl
}
