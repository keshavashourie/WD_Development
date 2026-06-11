// Copyright Siemens 2023  
using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework;


namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class OrganizationMaint : MatrixWebPart
    {
        #region Protected methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Category.DataChanged += Category_DataChanged;
        }

        protected virtual void Category_DataChanged(object sender, EventArgs e)
        {
            if (CategoryMap.BoundContext.SelectedItem != null)
            {
                Role.Data = (CategoryMap.BoundContext.SelectedItem as CategoryMapChanges).Role;
                Owner.Data = (CategoryMap.BoundContext.SelectedItem as CategoryMapChanges).Owner;
            }
        }

        #endregion

        #region Controls

        protected virtual JQDataGrid CategoryMap
        {
            get { return Page.FindCamstarControl("CategoryMap") as JQDataGrid; }
        }

        protected virtual CWC.DropDownList Category
        {
            get { return Page.FindCamstarControl("Category") as CWC.DropDownList; }
        }

        protected virtual CWC.NamedObject Role
        {
            get { return Page.FindCamstarControl("Role") as CWC.NamedObject; }
        }

        protected virtual CWC.NamedObject Owner
        {
            get { return Page.FindCamstarControl("Owner") as CWC.NamedObject; }
        }

        #endregion

        #region Private Member Variables

        #endregion
    }
}
