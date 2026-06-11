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
/// Summary description for SS_PackingQtyMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_CountVarianceLimitsMaint : SS_SetupBModelingBase
    {
        protected CWC.RevisionedObject _rdoSelectionSpec { get { return Page.FindCamstarControl("Selection_Spec") as CWC.RevisionedObject; } }       
        protected CWC.RevisionedObject _rdoSpec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
        protected JQDataGrid _gridDetails { get { return Page.FindCamstarControl("ObjectChanges_Details") as JQDataGrid; } }
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _SelectionControls.Add(_rdoSelectionSpec);            
            _CriteriaControls.Add(_rdoSpec);
            _SubentityGridControls.Add(_gridDetails);

            _HiddenGridColumns.Add("RN");
            _HiddenGridColumns.Add("QtyPerPack");
        }

    }
}



