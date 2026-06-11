// © 2018 Siemens Product Lifecycle Management Software Inc.
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using System.Collections.Generic;

using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CamstarPortal.WebControls;

using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;



namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class isOEEIndicator : MatrixWebPart
    {
        public isOEEIndicator() { }

        #region Controls
        //isResourceOEEInquiry_isRealTimeOEEDuration

        protected virtual CWC.DropDownList isRealTimeOEEDuration
        {
            get { return Page.FindCamstarControl("isResourceOEEInquiry_isRealTimeOEEDuration") as CWC.DropDownList; }
        }




        protected virtual CWC.CheckBox ChkCustomDates { get { return Page.FindCamstarControl("ctl") as CWC.CheckBox; } }

        protected virtual CWC.DateChooser StartDateChooser
        {
            get { return Page.FindCamstarControl("isResourceOEEInquiry_isStartTime") as CWC.DateChooser; }
        } // StartDate

        protected virtual CWC.DateChooser EndDateChooser
        {
            get { return Page.FindCamstarControl("isResourceOEEInquiry_isEndTime") as CWC.DateChooser; }
        } // EndDate
        #endregion

        #region ClientScript Section

        protected override IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("~/Scripts/user/Industry Solutions/barOEE.js");
        }

        #endregion

        #region Protected properties

       

        /// <summary>
        /// EProcedure specific hidden controls
        /// </summary>
        /// 
       

        #endregion

        #region Public methods


        

        #endregion

        #region Protected methods

        protected override void OnLoad(EventArgs e)
        {
          
            base.OnLoad(e);
            ChkCustomDates.DataChanged += ChkCustomDates_DataChanged;
            if (!Page.IsPostBack)
                //SetSatisfiedWidgetCountControl(null);
                ChkCustomDates_DataChanged(null, new EventArgs());
                //Page.Header.Controls.Add(new LiteralControl("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + ResolveUrl("~/Themes/User/Industry Solutions/barOEE.css") + "\" />"));

            
        } // OnLoad(EventArgs e)   

        private void ChkCustomDates_DataChanged(object sender, EventArgs e)
        {
            switch (ChkCustomDates.IsChecked)
            {
                case true:
                    // Code for checked state.  
                    StartDateChooser.Enabled = true;
                    EndDateChooser.Enabled = true;
                    isRealTimeOEEDuration.Enabled = false;
                    isRealTimeOEEDuration.ClearData();

                    break;
                case false:
                    // Code for unchecked state.
                    StartDateChooser.Enabled = false;
                    EndDateChooser.Enabled = false;
                    isRealTimeOEEDuration.Enabled = true;
                    isRealTimeOEEDuration.Data = 1;
                    StartDateChooser.ClearData();
                    EndDateChooser.ClearData();

                    break;
            }
        }


    
        #endregion
    }
}
