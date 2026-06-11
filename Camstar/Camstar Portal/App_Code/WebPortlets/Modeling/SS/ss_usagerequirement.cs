/* Copyright 2019 Siemens */
//Copyright © 1995-2013, Camstar Systems, Inc. All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using SEMI.AppCode;
using Camstar.WebPortal.FormsFramework;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using System.Web.UI;

/// <summary>
/// Summary description for SS_ProcessSpecMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Modeling
{
	public class SS_UsageReqMaint : MatrixWebPart
	{
		protected JQDataGrid ObjectChanges_UsageReqSetupDetails { get { return Page.FindCamstarControl("ObjectChanges_ss_UsageReqSetupDetails") as JQDataGrid; } }
		protected JQDataGrid ObjectChanges_Checklist { get { return Page.FindCamstarControl("ObjectChanges_Checklist") as JQDataGrid; } }
		protected CWC.DropDownList ddlUsageReqSetupDetail_UsageReqCountType_Editor { get { return ObjectChanges_UsageReqSetupDetails.FindControl("ObjectChanges_ss_UsageReqSetupDetails_ss_UsageReqCountType_InlineEditorControl") as CWC.DropDownList; } }
		protected CWC.DropDownList ddlUsageReqSetupDetail_UsageReqTxn_Editor { get { return ObjectChanges_UsageReqSetupDetails.FindControl("ObjectChanges_ss_UsageReqSetupDetails_ss_UsageReqTxn_InlineEditorControl") as CWC.DropDownList; } }
		protected CWC.TextBox txtUsageReqSetupDetail_UsageCountExpression_Editor { get { return ObjectChanges_UsageReqSetupDetails.FindControl("ObjectChanges_ss_UsageReqSetupDetails_ss_UsageCountExpression_InlineEditorControl") as CWC.TextBox; } }
		protected CWC.NamedObject ndoUsageReqSetupDetail_UsageCountQuery_Editor { get { return ObjectChanges_UsageReqSetupDetails.FindControl("ObjectChanges_ss_UsageReqSetupDetails_ss_UsageCountQuery_InlineEditorControl") as CWC.NamedObject; } }
		protected CWC.TextBox txtHiddenSelectedRowID { get { return Page.FindCamstarControl("HiddenSelectedRowIDTextBox") as CWC.TextBox; } }


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ddlUsageReqSetupDetail_UsageReqCountType_Editor.AutoPostBack = true;

			if (Page.IsPostBack)
				ddlUsageReqSetupDetail_UsageReqCountType_Editor.DataChanged += ndUsageReqSetupDetail_UsageReqCountType_Editor_DataChanged;
		}

		void ndUsageReqSetupDetail_UsageReqCountType_Editor_DataChanged(object sender, EventArgs e)
		{
			if (ddlUsageReqSetupDetail_UsageReqTxn_Editor.Data != null)	{
				ObjectChanges_UsageReqSetupDetails.GridContext.SetCell(txtHiddenSelectedRowID.Data.ToString(), "ss_UsageReqTxn", ddlUsageReqSetupDetail_UsageReqTxn_Editor.Data);
				//ddlUsageReqSetupDetail_UsageReqTxn_Editor.ClearData();
			}

			if (ddlUsageReqSetupDetail_UsageReqCountType_Editor.Data != null) {
				ObjectChanges_UsageReqSetupDetails.GridContext.SetCell(txtHiddenSelectedRowID.Data.ToString(), "ss_UsageReqCountType", ddlUsageReqSetupDetail_UsageReqCountType_Editor.Data);

				if (ddlUsageReqSetupDetail_UsageReqCountType_Editor.Text == "Expression") {
					ObjectChanges_UsageReqSetupDetails.GridContext.SetCell(txtHiddenSelectedRowID.Data.ToString(), "ss_UsageCountQuery", null);
					ndoUsageReqSetupDetail_UsageCountQuery_Editor.ClearData();
				}
				else if (ddlUsageReqSetupDetail_UsageReqCountType_Editor.Text == "Query") {
					ObjectChanges_UsageReqSetupDetails.GridContext.SetCell(txtHiddenSelectedRowID.Data.ToString(), "ss_UsageCountExpression", null);
					txtUsageReqSetupDetail_UsageCountExpression_Editor.ClearData();
				}

				ddlUsageReqSetupDetail_UsageReqCountType_Editor.ClearData();
			}


			if (txtUsageReqSetupDetail_UsageCountExpression_Editor.Data != null) {
				ObjectChanges_UsageReqSetupDetails.GridContext.SetCell(txtHiddenSelectedRowID.Data.ToString(), "ss_UsageCountExpression", txtUsageReqSetupDetail_UsageCountExpression_Editor.Data);
			}

			if (ndoUsageReqSetupDetail_UsageCountQuery_Editor.Data != null) {
				ObjectChanges_UsageReqSetupDetails.GridContext.SetCell(txtHiddenSelectedRowID.Data.ToString(), "ss_UsageCountQuery", ndoUsageReqSetupDetail_UsageCountQuery_Editor.Text);
			}

			Page.SetFocus(ObjectChanges_Checklist.ClientID);
			RenderToClient = true;
		}
	}
}



