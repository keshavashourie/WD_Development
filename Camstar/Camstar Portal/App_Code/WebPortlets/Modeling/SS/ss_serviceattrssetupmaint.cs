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
/// Summary description for SS_ServiceAttrsSetup
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_ServiceAttrsSetupMaint : SS_SetupBModelingBase
    {
        protected CWC.TextBox _txtSelectionServiceName { get { return Page.FindCamstarControl("ObjectChanges_ServiceNameSel") as CWC.TextBox; } }
		protected CWC.RevisionedObject _rdoSelectionSpec { get { return Page.FindCamstarControl("ObjectChanges_SpecSel") as CWC.RevisionedObject; } }
		protected CWC.NamedObject _ndoSelectionExternalLocation { get { return Page.FindCamstarControl("ObjectChanges_ExternalLocationSel") as CWC.NamedObject; } }
		protected CWC.NamedObject _ndoSelectionStartReason { get { return Page.FindCamstarControl("ObjectChanges_StartReasonSel") as CWC.NamedObject; } }

		protected CWC.TextBox _txtServiceName { get { return Page.FindCamstarControl("ObjectChanges_ServiceName") as CWC.TextBox; } }
		protected CWC.RevisionedObject _rdoSpec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
		protected CWC.NamedObject _ndoExternalLocation { get { return Page.FindCamstarControl("ObjectChanges_ExternalLocation") as CWC.NamedObject; } }
		protected CWC.NamedObject _ndoStartReason { get { return Page.FindCamstarControl("ObjectChanges_StartReason") as CWC.NamedObject; } }

		protected JQDataGrid _gridDetails { get { return Page.FindCamstarControl("ObjectChanges_Details") as JQDataGrid; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
			_SelectionControls.Add(_ndoSelectionExternalLocation);
			_SelectionControls.Add(_rdoSelectionSpec);
			_SelectionControls.Add(_txtSelectionServiceName);
			_SelectionControls.Add(_ndoSelectionStartReason);
			
            // add the criteria WP controls to the control collection
			_CriteriaControls.Add(_ndoExternalLocation);
			_CriteriaControls.Add(_rdoSpec);
			_CriteriaControls.Add(_txtServiceName);
			_CriteriaControls.Add(_ndoStartReason);			
			
            // add any subentity grid controls to the control collection
			_SubentityGridControls.Add(_gridDetails);

            // specify the hidden colums for the selection grid.
            _HiddenGridColumns.Add("SPSId");
			_HiddenGridColumns.Add("RN");
        }

    }
}



