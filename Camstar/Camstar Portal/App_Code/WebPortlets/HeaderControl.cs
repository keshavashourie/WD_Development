// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.ComponentModel;
using System.Web.UI.HtmlControls;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.WCFUtilities;
using System.Linq;
using System.Collections;

using CWF = Camstar.WebPortal.FormsFramework;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Camstar.WebPortal.Constants;
using System.Web.UI;
using Camstar.WebPortal.Utilities;
using Camstar.WCF.ObjectStack;
using Camstar.Portal;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using OM = Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets
{
    public class CustomActionContext : QualityObjectContext
    {
        public virtual CustomActionParameter[] ActionParameters { get; set; }
    }

    public class CustomActionParameter
    {
        public virtual string Name { get; set; }
        public virtual object Value { get; set; }
    }
}
