/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;

using SEMI.AppCode;
using Camstar.WCF.Services;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;

/// <summary>
/// Summary description for SS_JobAutoMaintenance
/// </summary>
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_JobAutoMaintenance : SS_JobTxn
    {
        // auto complete maintenace
        private CWC.CheckBox _chkAutoCompMaintainance { get { return Page.FindCamstarControl("Job_AutoCompleteMaintenance") as CWC.CheckBox; } }
        private CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("Job_ComputerName") as CWC.TextBox; } }

        //-----------------------------------------
        // override SetGeneralFields to set additional Field
        //-----------------------------------------
        protected override void SetGeneralFields(Result rslt)
        {
            base.SetGeneralFields(rslt);
            /// set maintenance status grid 
            // show or hide auto complete maintenance status 
            if ( (_gridMaintenanceStatus.Data as MaintenanceStatus[]).Count() > 0)
                 _chkAutoCompMaintainance.Visible = true;
        }

        //-------------------------------
        // Overwrite OnLoad event
        //-------------------------------
        protected override void OnLoad(EventArgs e) //Page on load event
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                _txtComputerName.Data = SEMI.AppCode.UIUtility.GetComputerName(this);
            }
        }

        public override void WebPartCustomAction(object sender, Personalization.CustomActionEventArgs e)
        {
            Page.ShopfloorReset(sender, e);
        }
    }
}



