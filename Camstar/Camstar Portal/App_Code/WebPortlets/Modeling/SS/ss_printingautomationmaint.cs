/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

/// <summary>
/// Summary description for SS_PrintingAutomationMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_PrintingAutomationMaint : SS_SetupBModelingBase
    {
        protected CWC.TextBox _rdoSelectionServiceName { get { return Page.FindCamstarControl("Selection_ServiceName") as CWC.TextBox; } }
        protected CWC.RevisionedObject _rdoSelectionSpec { get { return Page.FindCamstarControl("Selection_Spec") as CWC.RevisionedObject; } }
        protected CWC.TextBox _txtSelectionProcessSpecObjectType { get { return Page.FindCamstarControl("Selection_ProcessSpecObjectType") as CWC.TextBox; } }
        protected CWC.TextBox _txtSelectionCustomerCode { get { return Page.FindCamstarControl("Selection_CustomerCode") as CWC.TextBox; } }


        protected CWC.TextBox _rdoServiceName { get { return Page.FindCamstarControl("ObjectChanges_ServiceName") as CWC.TextBox; } }
        protected CWC.RevisionedObject _rdoSpec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
        protected CWC.TextBox _txtProcesSpecObjectType { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpecObjectType") as CWC.TextBox; } }
        protected CWC.TextBox _txtCustomerCode { get { return Page.FindCamstarControl("ObjectChanges_CustomerCode") as CWC.TextBox; } }

        protected JQDataGrid _gridDetails { get { return Page.FindCamstarControl("ObjectChanges_Details") as JQDataGrid; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
            _SelectionControls.Add(_rdoSelectionServiceName);
            _SelectionControls.Add(_rdoSelectionSpec);
            _SelectionControls.Add(_txtSelectionProcessSpecObjectType);
            _SelectionControls.Add(_txtSelectionCustomerCode);

            // add the criteria WP controls to the control collection
            _CriteriaControls.Add(_rdoServiceName);
            _CriteriaControls.Add(_rdoSpec);
            _CriteriaControls.Add(_txtProcesSpecObjectType);
            _CriteriaControls.Add(_txtCustomerCode);
            _CriteriaControls.Add(_gridDetails);

            // add any subentity grid controls to the control collection
            // _DetailControl.Add(_txtInstructions);

            // specify the hidden colums for the selection grid.
            _HiddenGridColumns.Add("RN");
        }
    }
}



