/* Copyright 2019 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;

using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;

/// <summary>
/// Summary description for ss_equipmentdispatchquerymatrixsetupmaint
/// </summary>
/// 
namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
	public class SS_EquipmentDispatchQueryMatrixSetupMaint : SS_SetupBModelingBase
	{
		protected CWC.NamedObject SelectionResourceGroup { get { return Page.FindCamstarControl("Selection_ResourceGroup") as CWC.NamedObject; } }
		protected CWC.NamedObject SelectionResource { get { return Page.FindCamstarControl("Selection_Resource") as CWC.NamedObject; } }
        protected CWC.DropDownList SelectionEquipmentType { get { return Page.FindCamstarControl("Selection_EquipmentType") as CWC.DropDownList; } }
		protected CWC.NamedObject SelectionEquipmentFamily { get { return Page.FindCamstarControl("Selection_EquipmentFamily") as CWC.NamedObject; } }
		protected CWC.NamedObject SelectionWorkCenter { get { return Page.FindCamstarControl("Selection_WorkCenter") as CWC.NamedObject; } }

		protected CWC.NamedObject ObjectChanges_ResourceGroup { get { return Page.FindCamstarControl("ObjectChanges_ResourceGroup") as CWC.NamedObject; } }
		protected CWC.NamedObject ObjectChanges_Resource { get { return Page.FindCamstarControl("ObjectChanges_Resource") as CWC.NamedObject; } }
        protected CWC.DropDownList ObjectChanges_EquipmentType { get { return Page.FindCamstarControl("ObjectChanges_EquipmentType") as CWC.DropDownList; } }
		protected CWC.NamedObject ObjectChanges_EquipmentFamily { get { return Page.FindCamstarControl("ObjectChanges_EquipmentFamily") as CWC.NamedObject; } }
		protected CWC.NamedObject ObjectChanges_WorkCenter { get { return Page.FindCamstarControl("ObjectChanges_WorkCenter") as CWC.NamedObject; } }
		protected CWC.NamedObject ObjectChanges_DispatchQuery { get { return Page.FindCamstarControl("ObjectChanges_DispatchQuery") as CWC.NamedObject; } }


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			//add the selection WP controls to the control collection
			_SelectionControls.Add(SelectionResourceGroup);
			_SelectionControls.Add(SelectionResource);
			_SelectionControls.Add(SelectionEquipmentType);
			_SelectionControls.Add(SelectionEquipmentFamily);
			_SelectionControls.Add(SelectionWorkCenter);

			// add the criteria WP controls to the control collection
			_CriteriaControls.Add(ObjectChanges_ResourceGroup);
			_CriteriaControls.Add(ObjectChanges_Resource);
			_CriteriaControls.Add(ObjectChanges_EquipmentType);
			_CriteriaControls.Add(ObjectChanges_EquipmentFamily);
			_CriteriaControls.Add(ObjectChanges_WorkCenter);
			_CriteriaControls.Add(ObjectChanges_DispatchQuery);

			// specify the hidden colums for the selection grid.
			_HiddenGridColumns.Add("RN");
		}
	}
}



