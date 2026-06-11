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
    public class SS_PrintingComputerMaint : SS_SetupBModelingBase
    {
        protected CWC.TextBox _txtSelectionComputerName { get { return Page.FindCamstarControl("Selection_ComputerName") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoSelectionPrintingComputer { get { return Page.FindCamstarControl("Selection_PrintingSetup") as CWC.NamedObject; } }

        protected CWC.TextBox _txtComputerName { get { return Page.FindCamstarControl("ObjectChanges_ComputerName") as CWC.TextBox; } }
        protected CWC.NamedObject _ndoPrintingComputer { get { return Page.FindCamstarControl("ObjectChanges_PrintingSetup") as CWC.NamedObject; } }

        protected JQDataGrid _gridPrinters { get { return Page.FindCamstarControl("ObjectChanges_Details") as JQDataGrid; } }

        protected JQDataGrid _gridSelection { get { return Page.FindCamstarControl("SelectionGrid") as JQDataGrid; } }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
            _SelectionControls.Add(_txtSelectionComputerName);
            _SelectionControls.Add(_ndoSelectionPrintingComputer);

            // add the criteria WP controls to the control collection
            _CriteriaControls.Add(_txtComputerName);
            _CriteriaControls.Add(_ndoPrintingComputer);

            // add any subentity grid controls to the control collection
            _SubentityGridControls.Add(_gridPrinters);

            // specify the hidden colums for the selection grid.
            _HiddenGridColumns.Add("RN");
            //_HiddenGridColumns.Add("Description");            
        }

    }
}



