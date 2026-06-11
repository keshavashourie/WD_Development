//© Copyright 2022 Siemens 
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Windows.Forms;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets.Parts
{

    public class MaterialPart : MatrixWebPart
    {

        #region Controls

        protected virtual JQDataGrid resultsGrid { get { return Page.FindCamstarControl("MaterialPartResultGrid") as JQDataGrid; } }
        protected virtual CWC.Button horizonSearchButton { get { return Page.FindCamstarControl("SearchForPart") as CWC.Button; } }
        protected virtual CWC.TextBox selectedMaterialPart { get { return Page.FindCamstarControl("SelectedMaterialPart") as CWC.TextBox; } }
        protected virtual CWC.TextBox selectedPartType { get { return Page.FindCamstarControl("SelectedPartType") as CWC.TextBox; } }
        protected virtual CWC.RevisionedObject materialPart { get { return Page.FindCamstarControl("PartTxn_MaterialPart") as CWC.RevisionedObject; } }
        protected virtual CWC.DropDownList partType { get { return Page.FindCamstarControl("PartTxn_PartType") as CWC.DropDownList; } }
        #endregion 

        #region Protected Functions

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (IsPopupClose(this)) UpdateParamMaterialPartMain();
            horizonSearchButton.Enabled = false;

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "SearchLayout_AddSlideoutToogler", string.Format("SearchLayout_AddSlideoutToogler('{0}');", resultsGrid.ClientID), true);
            resultsGrid.BoundContext.LBL("Lbl_NoDataToDisplay", "No DATA to display");
        }
        #endregion

        #region Public Functions

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            base.WebPartCustomAction(sender, e);
            var action = e.Action as CustomAction;
            if (action != null)
            {
                if (action.Parameters == "updateMaterialPart")
                {
                    UpdateParamMaterialPartMain();
                }
                else if (action.Parameters == "ResetValues")
                {
                    Page.ClearValues();
                    ClearFieldsMaterialPartMain();
                }
            }
        }

        public void ClearFieldsMaterialPartMain()
        {
            materialPart.Data = null;
            partType.ClearData();
            resultsGrid.ClearData();

            selectedMaterialPart.Data = "%";
            selectedPartType.Data = "%";
        }

        public void UpdateParamMaterialPartMain()
        {

            if (materialPart.Data == null) selectedMaterialPart.Data = "%";
            else selectedMaterialPart.Data = ((RevisionedObjectRef)materialPart.Data).Name;

            if (partType.Text == "") selectedPartType.Data = "%";
            else if (partType.Text == "Non-Serialized") selectedPartType.Data = "NonSerialized";
            else selectedPartType.Data = partType.Text;

            resultsGrid.ClearData();
        }


        #endregion

        #region Private Functions
        private static bool IsPopupClose(Camstar.WebPortal.WebPortlets.MatrixWebPart RefPage)
        {
            if (RefPage.Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument")
                return true;
            else
                return false;
        } // IsPopupClose
        #endregion

        #region Constants

        #endregion

        #region Private Member Variables

        #endregion

    }

}
