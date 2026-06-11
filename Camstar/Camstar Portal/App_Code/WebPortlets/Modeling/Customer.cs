// Copyright Siemens 2023  
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;

using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.WebPortlets;
using Camstar.WebPortal.Personalization;

using CamstarPortal.WebControls;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class Customer : MatrixWebPart
    {
        protected virtual JQDataGrid ContactsGrid
        {
            get
            {
                return Page.FindCamstarControl("CustomerContacts") as JQDataGrid;
            }
        }
   
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ContactsGrid.GridContext.RowUpdated += ContactsGrid_RowUpdated;
         }

        protected virtual ResponseData ContactsGrid_RowUpdated(object sender, JQGridEventArgs e)
        {
            var data = (CustomerContactChanges[])ContactsGrid.Data;
                        
            CustomerContactChanges selectedItem = null;

            if (ContactsGrid.SelectionData != null )
                selectedItem = ContactsGrid.SelectionData as CustomerContactChanges;
            else
                selectedItem = data[data.Count() - 1];
       
            if ((bool)selectedItem.PrimaryContact)
                Array.ForEach(data, i => { if (i.Name != selectedItem.Name) i.PrimaryContact = false; });

            e.State.Action = "Reload";
            e.Cancel = true;
           
            return ContactsGrid.GridContext.Reload(e.State); 

        }

	}
}
