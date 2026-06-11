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
/// Summary description for SS_CheckSheetMatrixMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_EqpConstraintMatrixMaint : SS_SetupBModelingBase
    {
        protected CWC.NamedObject _ndoSelectionEquipmentGroup { get { return Page.FindCamstarControl("Selection_EquipmentGroup") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoSelectionEquipment { get { return Page.FindCamstarControl("Selection_Equipment") as CWC.NamedObject; } }

        protected CWC.NamedObject _ndoEquipmentGroup { get { return Page.FindCamstarControl("ObjectChanges_EquipmentGroup") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoEquipment { get { return Page.FindCamstarControl("ObjectChanges_Equipment") as CWC.NamedObject; } }
        protected JQDataGrid ObjectChanges_EqpConstraintDetails { get { return Page.FindCamstarControl("ObjectChanges_EqpConstraintDetails") as JQDataGrid; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
            _SelectionControls.Add(_ndoSelectionEquipmentGroup);
            _SelectionControls.Add(_ndoSelectionEquipment);

            // add the criteria WP controls to the control collection
            _CriteriaControls.Add(_ndoEquipmentGroup);
            _CriteriaControls.Add(_ndoEquipment);
            _SubentityGridControls.Add(ObjectChanges_EqpConstraintDetails);

            // specify the hidden colums for the selection grid.
            _HiddenGridColumns.Add("RN");
        }
    }
}



