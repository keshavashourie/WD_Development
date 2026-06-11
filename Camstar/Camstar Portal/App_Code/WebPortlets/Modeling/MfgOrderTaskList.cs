// Copyright Siemens 2022  
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WCF.ObjectStack;

namespace Camstar.WebPortal.WebPortlets.Modeling
{

    /// <summary>
    /// Task List maintenance 
    /// </summary>
    public class MfgOrderTaskList : MatrixWebPart   //TaskList
    {
        #region Controls
        protected virtual CWC.RevisionedObject PrerequisiteTaskList { get { return Page.FindCamstarControl("PrerequisiteTaskList") as CWC.RevisionedObject; } }
        protected virtual CWC.NamedObject Workstation { get { return Page.FindCamstarControl("Workstation") as CWC.NamedObject; } }
        protected virtual CWC.NamedObject WorkstationGroup { get { return Page.FindCamstarControl("WorkstationGroup") as CWC.NamedObject; } }
        #endregion

        #region Event Handlers
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            PrerequisiteTaskList.Visible = false;
            Workstation.Visible = false;
            WorkstationGroup.Visible = false;
        }
        #endregion

    }

}

