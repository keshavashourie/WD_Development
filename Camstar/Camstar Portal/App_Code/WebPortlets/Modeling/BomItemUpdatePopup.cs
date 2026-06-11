// Copyright Siemens 2023 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Newtonsoft.Json;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Siemens.OPCR.Diagnostics;
using System.Web;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.PortalFramework;


namespace Camstar.WebPortal.WebPortlets.Modeling
{
    /// <summary>
    /// Lets user select BOM items and specify new values for 1 or more of them
    /// </summary>
    public class BomItemUpdatePopup : MatrixWebPart
    {
        #region Controls
        protected CWC.TextBox BomItemsJson { get { return Page.FindCamstarControl("BomItemsJson") as CWC.TextBox; } }
        protected CWC.RevisionedObject ERPRoute { get { return Page.FindCamstarControl("ERPRoute") as CWC.RevisionedObject; } }
        protected CWC.CheckBox ShowRouteStep {  get {  return Page.FindCamstarControl("ShowRouteStep") as CWC.CheckBox; } }
        #endregion Controls

        string ErrorMessage = "";
        string WarningMessage = "";

        #region Page Lifecycle Methods
        public BomItemUpdatePopup()
        {
            _log.ClassName = "BomItemUpdate";
        }

        static protected LogClient _log = new LogClient("BomitemUpdate");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!string.IsNullOrEmpty(ErrorMessage))
                Page.StatusBar.WriteError(ErrorMessage);
            else if(!string.IsNullOrEmpty(WarningMessage))
                Page.StatusBar.WriteWarning(WarningMessage);

            ErrorMessage = "";
            WarningMessage = "";

            if (!Page.IsPostBack)
            {
                // if we have an ERP Route, assume we will show Route Step instead of Spec
                bool showRouteStep = ShowRouteStep?.Data != null && (bool)ShowRouteStep.Data;
                string showRouteStepString = showRouteStep ? "true" : "false";
                string showSpecString = showRouteStep ? "false" : "true";

                string initScript = $"CR.BomItemUpdatePopup.initialize({showSpecString}, {showRouteStepString});";
                ScriptManager.RegisterStartupScript(this, GetType(), "BomItemUpdatePopupInitialize", initScript, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);

            var action = e.Action as Personalization.CustomAction;
            if (action != null)
            {
                switch (action.Parameters)
                {
                    case "Update":
                        {
                            Page.CloseFloatingFrameOnSubmit(new ResultStatus());
                            break;
                        }
                }
            }
        }

        #endregion Page Lifecycle Methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/CRPickGrid.js");
            yield return new ScriptReference("~/Scripts/BomItemUpdatePopup.js");
        }

    }
}