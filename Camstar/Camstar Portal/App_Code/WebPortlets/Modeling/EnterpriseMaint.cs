//Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.Utilities;
using Newtonsoft.Json;
using Camstar.WCF.Services;
using System.Data;
using System.Linq;
using Camstar.WebPortal.Helpers;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    public class EnterpriseMaint : SwacPageBase
    {
        #region Controls
        //TODO: following properties only needed for testing SWAC popup.
        protected virtual CWC.TextBox EnterpriseName { get { return Page.FindCamstarControl("NameTxt") as CWC.TextBox; } }

        #endregion

        #region Protected Functions
        //TODO: update to get correct data when have actual SWAC component interface.
        protected override List<string> GetSwacComponentInitializationArgs(string webPartName, string buttonName)
        {
            var item = Page.DataContract.GetValueByName("SelectedInstanceRef") as NamedObjectRef;
            string accessPermission = SrcIntegrationHelper.GetUserAccessPermission();
            string bearerToken = SrcIntegrationHelper.BearerToken;

            List<string> args = new List<string>
            {
                item.ID,
                item.Name,
                "ENTERPRISE",
                accessPermission,
                bearerToken
            };
            return args;
        }


        #endregion

    }

}
