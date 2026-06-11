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
/// Summary description for SS_PrintingComputerMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_LotSizeSamplingSetupMaint : SS_SetupBModelingBase
    {
        protected CWC.RevisionedObject Selection_Spec { get { return Page.FindCamstarControl("Selection_Spec") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject ObjectChanges_Spec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
     
        protected JQDataGrid _gridDetails { get { return Page.FindCamstarControl("ObjectChanges_Details") as JQDataGrid; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
            _SelectionControls.Add(Selection_Spec);
            
            // add the criteria WP controls to the control collection
            _CriteriaControls.Add(ObjectChanges_Spec);

            // add any subentity grid controls to the control collection
            _SubentityGridControls.Add(_gridDetails);

            _HiddenGridColumns.Add("RN");
            
        }

    }
}



