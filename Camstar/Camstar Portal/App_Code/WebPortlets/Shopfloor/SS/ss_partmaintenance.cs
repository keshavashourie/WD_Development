/* Copyright 2023 Siemens */
using System;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using System.Linq;

namespace Camstar.WebPortal.WebPortlets.Parts
{
    public class SS_PartMaintenance : PartMaintenance
    {
        //Controls declaration
        private CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("PartCreate_ComputerName") as CWC.TextBox; } }

        //---------------------------------------------------
        // Override OnLoad event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                ManageSetStatusButton();
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }

        public void ManageSetStatusButton()
        {
            string varSetStatus = "PartSetStatusButton";

            if (resultsGrid != null)
            {
                // set all hidden
                this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varSetStatus).FirstOrDefault().IsHidden = true;

                if (resultsGrid.GridContext.SelectedRowID != null)
                {
                    this.Page.ActionDispatcher.ActionPanelActions().Where(prop => prop.Name == varSetStatus).FirstOrDefault().IsHidden = false;
                }
            }
        }
    }
}




