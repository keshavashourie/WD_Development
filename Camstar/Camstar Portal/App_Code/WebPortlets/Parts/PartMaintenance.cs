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

    public class PartMaintenance : MatrixWebPart
    {

        #region Controls

        protected virtual JQDataGrid resultsGrid { get { return Page.FindCamstarControl("PartsResultGrid") as JQDataGrid; } }
        protected virtual CWC.Button horizonSearchButton { get { return Page.FindCamstarControl("SearchButton") as CWC.Button; } }
        protected virtual CWC.TextBox selectedPhysicalLocation { get { return Page.FindCamstarControl("SelectedPhysicalLocation") as CWC.TextBox; } }
        protected virtual CWC.TextBox selectedPhysicalPosition { get { return Page.FindCamstarControl("SelectedPhysicalPosition") as CWC.TextBox; } }
        protected virtual CWC.TextBox selectedMaterialPart { get { return Page.FindCamstarControl("SelectedMaterialPart") as CWC.TextBox; } }
        protected virtual CWC.TextBox selectedPartFamily { get { return Page.FindCamstarControl("SelectedPartFamily") as CWC.TextBox; } }
        protected virtual CWC.TextBox selectedPartType { get { return Page.FindCamstarControl("SelectedPartType") as CWC.TextBox; } }
        protected virtual CWC.RevisionedObject materialPart { get { return Page.FindCamstarControl("PartTxn_MaterialPart") as CWC.RevisionedObject; } }
        protected virtual CWC.DropDownList partType { get { return Page.FindCamstarControl("PartTxn_PartType") as CWC.DropDownList; } }
        protected virtual CWC.NamedObject partFamily { get { return Page.FindCamstarControl("PartTxn_PartFamily") as CWC.NamedObject; } }
        protected virtual CWC.NamedObject physicalLocation { get { return Page.FindCamstarControl("PartTxn_PhysicalLocation") as CWC.NamedObject; } }
        protected virtual CWC.NamedObject physicalPosition { get { return Page.FindCamstarControl("PartTxn_PhysicalPosition") as CWC.NamedObject; } }

        #endregion 

        #region Protected Functions

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ForceDataContractMembers();
            ManagePanelButton();

            if (IsPopupClose(this))
            {
                UpdateParamPartMain();
            }

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
                if (action.Parameters == "updatePartMaint")
                {
                    UpdateParamPartMain();
                }
                else if (action.Parameters == "ResetValues")
                {
                    Page.ClearValues();
                    ClearFieldsPartMain();
                }
            }
        }


        public void ClearFieldsPartMain()
        {
            partFamily.Data = null;
            partType.ClearData();
            physicalLocation.Data = null;
            physicalPosition.Data = null;
            materialPart.Data = null;

            selectedPhysicalLocation.Data = "%";
            selectedMaterialPart.Data = "%";
            selectedPartFamily.Data = "%";
            selectedPartType.Data = "%";
            selectedPhysicalPosition.Data = "%";

            resultsGrid.ClearData();
        }

        public void UpdateParamPartMain()
        {
            if (partFamily.Data == null)
                selectedPartFamily.Data = "%";
            else
                selectedPartFamily.Data = partFamily.Data;

            if (partType.Text == "")
                selectedPartType.Data = "%";
            else if (partType.Text == "Non-Serialized")
                selectedPartType.Data = "NonSerialized";
            else
                selectedPartType.Data = partType.Text;

            if (physicalLocation.Data == null)
                selectedPhysicalLocation.Data = "%";
            else
                selectedPhysicalLocation.Data = physicalLocation.Data;

            if (physicalPosition.Data == null)
                selectedPhysicalPosition.Data = "%";
            else
                selectedPhysicalPosition.Data = physicalPosition.Data;

            if (materialPart.Data == null)
                selectedMaterialPart.Data = "%";
            else
                selectedMaterialPart.Data = materialPart.Data;

            resultsGrid.ClearData();
            ManagePanelButton();
        }

        public void ManagePanelButton()
        {
            string varCreate = "PartCreateButton";
            string varSetup = "PartSetupButton";
            string varScrap = "PartScrapButton";

            if (resultsGrid != null)
            {
                // set all hidden
                this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varCreate).FirstOrDefault().IsHidden = false;
                this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varSetup).FirstOrDefault().IsHidden = true;
                this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varScrap).FirstOrDefault().IsHidden = true;

                if (resultsGrid.GridContext.SelectedRowID != null)
                {
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varCreate).FirstOrDefault().IsHidden = false;
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varSetup).FirstOrDefault().IsHidden = false;
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varScrap).FirstOrDefault().IsHidden = false;
                }
            }
        }

        public void ForceDataContractMembers()
        {
            if (resultsGrid != null)
            {
                if (resultsGrid.SelectedRowID != null)
                {
                    Page.PortalContext.DataContract.SetValueByName("PartName_PartMain", resultsGrid.GridContext.GetSelectedCell("Name"));
                    Page.PortalContext.DataContract.SetValueByName("MaterialPartName_Part", resultsGrid.GridContext.GetSelectedCell("MaterialPart"));
                    Page.PortalContext.DataContract.SetValueByName("MaterialPartRevision_PartMain", resultsGrid.GridContext.GetSelectedCell("Revision"));
                    Page.PortalContext.DataContract.SetValueByName("PartFamily_PartMain", resultsGrid.GridContext.GetSelectedCell("PartFamily"));
                    Page.PortalContext.DataContract.SetValueByName("PartQty_PartMain", resultsGrid.GridContext.GetSelectedCell("PartQty"));
                    Page.PortalContext.DataContract.SetValueByName("Factory_PartMain", resultsGrid.GridContext.GetSelectedCell("Factory"));
                    Page.PortalContext.DataContract.SetValueByName("Vendor_PartMain", resultsGrid.GridContext.GetSelectedCell("Vendor"));
                    Page.PortalContext.DataContract.SetValueByName("VendorModel_PartMain", resultsGrid.GridContext.GetSelectedCell("VendorModel"));
                    Page.PortalContext.DataContract.SetValueByName("VendorSerialNumber_PartMain", resultsGrid.GridContext.GetSelectedCell("VendorSerialNumber"));
                    Page.PortalContext.DataContract.SetValueByName("PartExpiryDate_PartMain", resultsGrid.GridContext.GetSelectedCell("PartExpiryDate"));
                    Page.PortalContext.DataContract.SetValueByName("PhysicalLocation_PartMain", resultsGrid.GridContext.GetSelectedCell("PhysicalLocation"));
                    Page.PortalContext.DataContract.SetValueByName("PhysicalPosition_PartMain", resultsGrid.GridContext.GetSelectedCell("PhysicalPosition"));
                }
            }
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
