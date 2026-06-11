// © 2018 Siemens Product Lifecycle Management Software Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class isCarrier : MatrixWebPart
    {
        protected virtual TextBox txtName { get { return Page.FindCamstarControl("NameTxt") as TextBox; } }
        protected virtual CWC.NamedObject ddlMfgOrder { get { return Page.FindCamstarControl("ObjectChanges_isMfgOrder") as CWC.NamedObject; } }
        protected virtual CWC.RevisionedObject ddlProduct { get { return Page.FindCamstarControl("ObjectChanges_isProduct") as CWC.RevisionedObject; } }

        protected virtual DropDownList ddlSingleMfgOrder { get { return Page.FindCamstarControl("ObjectChanges_isSingleMfgOrder") as DropDownList; } }
        protected virtual DropDownList ddlSingleProduct { get { return Page.FindCamstarControl("ObjectChanges_isSingleProduct") as DropDownList; } }
        protected virtual DropDownList ddlUsePosition { get { return Page.FindCamstarControl("ObjectChanges_isUsePosition") as DropDownList; } }
        protected virtual JQDataGrid gridCarrierContainers { get { return Page.FindCamstarControl("ObjectChanges_isCarrierContainers") as JQDataGrid; } }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            txtName.DataChanged += TxtName_DataChanged;
        }

        void TxtName_DataChanged(object sender, EventArgs e)
        {
            ClientGridState state = new ClientGridState();
            state.ActionID = "refr";
            state.Action = "Reload";
            gridCarrierContainers.GridContext.Reload(state);
            if (gridCarrierContainers.TotalRowCount > 0)
            {
                ddlSingleMfgOrder.ReadOnly = true;
                ddlSingleProduct.ReadOnly = true;
                ddlUsePosition.ReadOnly = true;
                ddlMfgOrder.ReadOnly = true;
                ddlProduct.ReadOnly = true;
            }
            else
            {
                ddlSingleMfgOrder.ReadOnly = false;
                ddlSingleProduct.ReadOnly = false;
                ddlUsePosition.ReadOnly = false;
                ddlMfgOrder.ReadOnly = false;
                ddlProduct.ReadOnly = false;
            }
        }
    }
}