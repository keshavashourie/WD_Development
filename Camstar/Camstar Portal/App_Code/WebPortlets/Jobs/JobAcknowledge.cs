/* Copyright 2022 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;

using Camstar.WCF.Services;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
/// <summary>
/// Summary description for JobAcknowledge
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Jobs
{
    public class JobAcknowledge : JobTxn
    {
        //-----------------------------------------
        // Property
        //-----------------------------------------
        private CWC.DateChooser _dateExpectedStart { get { return Page.FindCamstarControl("Job_ExpectedStartDate") as CWC.DateChooser; } }
        private CWC.TextBox _txtEstimatedDuration { get { return Page.FindCamstarControl("Job_EstimatedDuration") as CWC.TextBox; } }
        private CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("Job_ComputerName") as CWC.TextBox; } }

        //-----------------------------------------
        // override SetGeneralFields to set additional Field
        //-----------------------------------------
        protected override void SetGeneralFields(Result rslt)
        {
            base.SetGeneralFields(rslt);
            if (_txtEstimatedDuration != null)
                _txtEstimatedDuration.Data = (rslt as JobAcknowledge_Result).Value.EstimatedDuration;
            if (_dateExpectedStart != null)
            {
                if ((rslt as JobAcknowledge_Result).Value.ExpectedStartDate != null)
                {
                    _dateExpectedStart.Data = (rslt as JobAcknowledge_Result).Value.ExpectedStartDate.ToString();
                    Camstar.WebPortal.FormsFramework.CamstarWebControl.SetRenderToClient(_dateExpectedStart);
                }
            }   
        }  // SetGeneralFields
    }
}



