// Copyright Siemens 2023  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using OM=Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.PortalFramework;

namespace Camstar.WebPortal.Helpers
{
    /// <summary>
    /// Summary description for GridActions
    /// </summary>
    public class GridActionHelperBase
    {
        static public ResponseData RowSelect(object sender, JQGridEventArgs args)
        {
            args.Cancel = true;
            return new StatusData(true, "Request completed!");
        }
    }
}
