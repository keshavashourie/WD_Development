// Copyright Siemens 2023  
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using PERS = Camstar.WebPortal.Personalization;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    public class PortalMessageCategoryMaint : MatrixWebPart
    {
 
        #region Protected Functions

        protected override void OnLoadPersonalization()
        {
            base.OnLoadPersonalization();

            var newAction = Page.ActionDispatcher.GetActionByName("NewBtn");
            if (newAction != null)
                newAction.ConditionHandler = "WebPartConditionActionHandler"; // need to hide New action.

            var copyAction = Page.ActionDispatcher.GetActionByName("CopyBtn");
            if (copyAction != null)
                copyAction.ConditionHandler = "WebPartConditionActionHandler"; // need to hide Copy action.

            var deleteAction = Page.ActionDispatcher.GetActionByName("DeleteBtn");
            if (deleteAction != null)
                deleteAction.ConditionHandler = "WebPartConditionActionHandler"; // need to hide Delete action.
        }

        public override void WebPartConditionActionHandler(object sender, ConditionActionEventArgs e)
        {
            base.WebPartConditionActionHandler(sender, e);
            e.IsHidden = true;
        }

     
        #endregion


    }

}

