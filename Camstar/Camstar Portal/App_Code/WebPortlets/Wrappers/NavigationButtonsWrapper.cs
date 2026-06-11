// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls.WebParts;

using Camstar.WebPortal.PortalFramework;

namespace Camstar.WebPortal.WebPortlets
{
    public class NavigationButtonsWrapper : ButtonsBarWrapper
    {
        public NavigationButtonsWrapper(WebPartBase webPart) : base(webPart)
        {
        }

        public override void Wrap(ControlCollection controls, WebPartCreateContentMethod createContentMethod)
        {
            base.Wrap(controls, createContentMethod);
        }
    }
}
