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
/// Summary description for SS_ProcessSpecBinsMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_ProcessSpecBinsMaint : SS_SetupBModelingBase
    {
		protected CWC.RevisionedObject _rdoSelectionSpec { get { return Page.FindCamstarControl("ObjectChanges_SpecSel") as CWC.RevisionedObject; } }
		protected CWC.RevisionedObject _rdoSelectionProduct { get { return Page.FindCamstarControl("ObjectChanges_ProductSel") as CWC.RevisionedObject; } }
		protected CWC.RevisionedObject _rdoSelectionProcessSpec { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpecSel") as CWC.RevisionedObject; } }
		protected CWC.NamedObject _ndoSelectionOwner { get { return Page.FindCamstarControl("ObjectChanges_OwnerSel") as CWC.NamedObject; } }
		protected CWC.TextBox _txtSelectionTestProgramName { get { return Page.FindCamstarControl("ObjectChanges_TestProgramNameSel") as CWC.TextBox; } }
		protected CWC.TextBox _txtSelectionTestProgramMajor { get { return Page.FindCamstarControl("ObjectChanges_TestProgramMajorRevisionSel") as CWC.TextBox; } }
		protected CWC.TextBox _txtSelectionTestProgramMinor { get { return Page.FindCamstarControl("ObjectChanges_TestProgramMinorRevisionSel") as CWC.TextBox; } }

		protected CWC.RevisionedObject _rdoSpec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
		protected CWC.RevisionedObject _rdoProduct { get { return Page.FindCamstarControl("ObjectChanges_Product") as CWC.RevisionedObject; } }
		protected CWC.RevisionedObject _rdoProcessSpec { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpec") as CWC.RevisionedObject; } }
		protected CWC.NamedObject _ndoOwner { get { return Page.FindCamstarControl("ObjectChanges_Owner") as CWC.NamedObject; } }
		protected CWC.TextBox _txtTestProgramName { get { return Page.FindCamstarControl("ObjectChanges_TestProgramName") as CWC.TextBox; } }
		protected CWC.TextBox _txtTestProgramMajor { get { return Page.FindCamstarControl("ObjectChanges_TestProgramMajorRevision") as CWC.TextBox; } }
		protected CWC.TextBox _txtTestProgramMinor { get { return Page.FindCamstarControl("ObjectChanges_TestProgramMinorRevision") as CWC.TextBox; } }

		protected JQDataGrid _gridProcessSpecBins { get { return Page.FindCamstarControl("ObjectChanges_Details") as JQDataGrid; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
			_SelectionControls.Add(_rdoSelectionSpec);
			_SelectionControls.Add(_rdoSelectionProduct);
			_SelectionControls.Add(_rdoSelectionProcessSpec);
			_SelectionControls.Add(_ndoSelectionOwner);
			_SelectionControls.Add(_txtSelectionTestProgramMajor);
			_SelectionControls.Add(_txtSelectionTestProgramMinor);
			_SelectionControls.Add(_txtSelectionTestProgramName);
			
            // add the criteria WP controls to the control collection
			_CriteriaControls.Add(_rdoSpec);
			_CriteriaControls.Add(_rdoProduct);
			_CriteriaControls.Add(_rdoProcessSpec);
			_CriteriaControls.Add(_ndoOwner);
			_CriteriaControls.Add(_txtTestProgramName);
			_CriteriaControls.Add(_txtTestProgramMajor);
			_CriteriaControls.Add(_txtTestProgramMinor);
			
            // add any subentity grid controls to the control collection
			_SubentityGridControls.Add(_gridProcessSpecBins);

            // specify the hidden colums for the selection grid.
            _HiddenGridColumns.Add("SPSId");
			_HiddenGridColumns.Add("RN");
        }

    }
}



