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
/// Summary description for SS_SamplingSetupMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_SamplingPlanSetupMaint : SS_SetupBModelingBase
    {
		protected CWC.NamedObject _ndoSelectionProductFamily { get { return Page.FindCamstarControl("ObjectChanges_ProductFamilySel") as CWC.NamedObject; } }
        protected CWC.RevisionedObject _rdoSelectionProduct { get { return Page.FindCamstarControl("ObjectChanges_ProductSel") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoSelectionProcessSpec { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpecSel") as CWC.RevisionedObject; } }
        protected CWC.NamedObject _ndoSelectionOwner { get { return Page.FindCamstarControl("ObjectChanges_OwnerSel") as CWC.NamedObject; } }
        protected CWC.TextBox _txtSelectionLotName { get { return Page.FindCamstarControl("ObjectChanges_LotNameSel") as CWC.TextBox; } }

        protected CWC.NamedObject _ndoProductFamily { get { return Page.FindCamstarControl("ObjectChanges_ProductFamily") as CWC.NamedObject; } }
        protected CWC.RevisionedObject _rdoProduct { get { return Page.FindCamstarControl("ObjectChanges_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoProcessSpec { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject _ndoOwner { get { return Page.FindCamstarControl("ObjectChanges_Owner") as CWC.NamedObject; } }
        protected CWC.TextBox _txtLotName { get { return Page.FindCamstarControl("ObjectChanges_LotName") as CWC.TextBox; } }
        protected CWC.RevisionedObject _rdoSamplingPlan { get { return Page.FindCamstarControl("ObjectChanges_SamplingPlan") as CWC.RevisionedObject; } }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
			_SelectionControls.Add(_ndoSelectionProductFamily);
			_SelectionControls.Add(_rdoSelectionProduct);
			_SelectionControls.Add(_rdoSelectionProcessSpec);
			_SelectionControls.Add(_ndoSelectionOwner);
			_SelectionControls.Add(_txtSelectionLotName);
			
            // add the criteria WP controls to the control collection
			_CriteriaControls.Add(_ndoProductFamily);
			_CriteriaControls.Add(_rdoProduct);
			_CriteriaControls.Add(_rdoProcessSpec);
			_CriteriaControls.Add(_ndoOwner);
			_CriteriaControls.Add(_txtLotName);
			_CriteriaControls.Add(_rdoSamplingPlan);

			
            // add any subentity grid controls to the control collection
			//_SubentityGridControls.Add(_gridSamplingPlans);

            // specify the hidden colums for the selection grid.
            _HiddenGridColumns.Add("SPSId");
			_HiddenGridColumns.Add("RN");
        }

    }
}



