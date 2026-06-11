/* Copyright 2019 Siemens */
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
    public class SS_ItemCategoryMaint : MatrixWebPart
    {
		protected override void OnLoad(EventArgs e)
		{
			if (((CheckBox)Page.FindCamstarControl("ObjectChanges_SysForceToUpper")).CheckControl.Checked == true)
				SEMI.AppCode.UIUtility.SetCapital((TextBox)Page.FindCamstarControl("NameTxt"), ((CheckBox)Page.FindCamstarControl("ObjectChanges_SysForceToUpper")).CheckControl.Checked);			
			
			base.OnLoad(e);
		}
    }
}




