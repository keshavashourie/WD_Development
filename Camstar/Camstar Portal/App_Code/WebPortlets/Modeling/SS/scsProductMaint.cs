//Copyright Siemens 2024
using System;
using Camstar.WebPortal.Personalization;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    /// <summary>
    /// TODO: Add a Summary description for this Camstar Web Part
    /// </summary>
    public class scsProductMaint : MatrixWebPart
    {
        protected override void OnPreRender(EventArgs e)
        {
            string helpUrl = string.Empty;

            if (string.Compare(PrimaryServiceType, "PNMaint", true) == 0 || string.Compare(PrimaryServiceType, "MaterialPartMaint", true) == 0)
                helpUrl = @"OnlineHelpOutput\SemiSuite\Modeling\PortalModeling_CSH.htm#SEMIModelingObjects\Defining_PN_and_Material_Part_Products.htm";


            if (!string.IsNullOrEmpty(helpUrl))
            {
                PageContent content = Page.Model.PublishedContent as PageContent;
                if (content != null)
                {
                    content.HelpFileURL = helpUrl;
                }
            }
            base.OnPreRender(e);
        }
    }
}