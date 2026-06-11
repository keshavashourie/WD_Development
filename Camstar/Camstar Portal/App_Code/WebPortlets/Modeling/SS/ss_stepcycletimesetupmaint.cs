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
/// Summary description for SS_StepCycleTimeSetupMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_StepCycleTimeSetupMaint : SS_SetupBModelingBase
    {
        protected CWC.RevisionedObject _rdoSelectionSpec { get { return Page.FindCamstarControl("Selection_Spec") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoSelectionProduct { get { return Page.FindCamstarControl("Selection_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoSelectionProcessSpec { get { return Page.FindCamstarControl("Selection_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject _ndoSelectionProductLine { get { return Page.FindCamstarControl("Selection_ProductLine") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoSelectionOwner { get { return Page.FindCamstarControl("Selection_Owner") as CWC.NamedObject; } }
        protected JQDataGrid _gridSeletionGrid { get { return Page.FindCamstarControl("SelectionGrid") as JQDataGrid; } }

        protected CWC.RevisionedObject _rdoSpec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoProduct { get { return Page.FindCamstarControl("ObjectChanges_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoProcessSpec { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject _rdoProductLine { get { return Page.FindCamstarControl("ObjectChanges_ProductLine") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoOwner { get { return Page.FindCamstarControl("ObjectChanges_Owner") as CWC.NamedObject; } }
        protected CWC.TextBox _txtStepCycleTime { get { return Page.FindCamstarControl("ObjectChanges_StepCycleTime") as CWC.TextBox; } }

     
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
            _SelectionControls.Add(_rdoSelectionSpec);
            _SelectionControls.Add(_rdoSelectionProduct);
            _SelectionControls.Add(_rdoSelectionProcessSpec);
            _SelectionControls.Add(_ndoSelectionProductLine);
            _SelectionControls.Add(_ndoSelectionOwner);

            // add the criteria WP controls to the control collection
            _CriteriaControls.Add(_rdoSpec);
            _CriteriaControls.Add(_rdoProduct);
            _CriteriaControls.Add(_rdoProcessSpec);
            _CriteriaControls.Add(_rdoProductLine);
            _CriteriaControls.Add(_ndoOwner);

            

            // specify the hidden colums for the selection grid.
            _HiddenGridColumns.Add("StepCycleTime");
            _HiddenGridColumns.Add("RN"); 
                     
        }

    }
}



