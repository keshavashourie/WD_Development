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
    public class SS_PackingQtyMaint : SS_SetupBModelingBase
    {
        protected CWC.NamedObject _ndoSelectionPackingType { get { return Page.FindCamstarControl("Selection_PackingType") as CWC.NamedObject; } }
        protected CWC.RevisionedObject _rdoSelectionProduct { get { return Page.FindCamstarControl("Selection_Product") as CWC.RevisionedObject;} }

        protected CWC.NamedObject _ndoPackingType { get { return Page.FindCamstarControl("ObjectChanges_PackingType") as CWC.NamedObject; } }
        protected CWC.RevisionedObject _rdoProduct { get { return Page.FindCamstarControl("ObjectChanges_Product") as CWC.RevisionedObject; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _SelectionControls.Add(_ndoSelectionPackingType);
            _SelectionControls.Add(_rdoSelectionProduct);

            _CriteriaControls.Add(_ndoPackingType);
            _CriteriaControls.Add(_rdoProduct);

            _HiddenGridColumns.Add("RN");
            _HiddenGridColumns.Add("QtyPerPack");
        }
        
    }
}



