//
// Copyright Siemens 2024  
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

/// <summary>
/// Summary description for ResourceFamilyMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class ResourceFamilyMaint : MatrixWebPart
    {
        #region Controls

        private Dictionary<string, string> portalHelpFile = new Dictionary<string, string> {
            { "ResourceFamilyMaint", @"onlinehelpoutput/portalmodeling_Help/PortalModeling.htm#cshid=ModelingObjects/Defining_Resource_Families_P.htm" },
            { "ToolFamilyMaint", @"onlinehelpoutput/portalmodeling_Help/PortalModeling_CSH.htm#ModelingObjects/Defining_Tool_Families.htm" },
        };

        protected virtual ToggleContainer GeneralTab
        {
            get { return Page.FindCamstarControl("GeneralGroupToggle") as ToggleContainer; }
        }

        protected virtual CheckBox UseUIPreference
        {
            get { return Page.FindCamstarControl("UseUIPreference") as CheckBox; }
        }

        #endregion

        #region Overrided methods

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            string helpUrl = string.Empty;
            if (!(bool)UseUIPreference.Data && Page.IsPostBack)
                GeneralTab.Visible = false;

            // Programmatically include helpfile
            if (portalHelpFile.ContainsKey(PrimaryServiceType))
                helpUrl = portalHelpFile[PrimaryServiceType];

            if (!string.IsNullOrEmpty(helpUrl))
            {
                Personalization.PageContent content = Page.Model.PublishedContent as PageContent;
                if (content != null)
                {
                    content.HelpFileURL = helpUrl;
                }
            }
        }

        #endregion
    }
}
