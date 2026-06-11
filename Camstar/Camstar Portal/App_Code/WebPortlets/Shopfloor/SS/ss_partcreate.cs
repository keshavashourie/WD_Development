/* Copyright 2019 Siemens */
using System;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets.Parts
{
    public class SS_PartCreate : PartCreate
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
                if (!Page.IsPostBack)
                {
                    _txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }
    }
}




