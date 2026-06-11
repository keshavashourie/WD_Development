/* Copyright 2025 Siemens */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using SEMI.AppCode;
using Camstar.WebPortal.FormsFramework;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.FormsFramework.WebGridControls;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
	public class SS_ProductBOMMaint : MatrixWebPart
	{

		CWC.WorkflowNavigator _MaterialListWfNav { get { return Page.FindCamstarControl("MaterialList_WorkflowNav") as CWC.WorkflowNavigator; } }
		CWC.NamedSubentity _MaterialListStep { get { return Page.FindCamstarControl("MaterialList_ss_Step") as CWC.NamedSubentity; } }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			var stackControl = Page.FindCamstarControl(_MaterialListWfNav.ClientID + "_Stack") as FieldControl;
			stackControl.DataSubmissionMode = Personalization.DataSubmissionModeType.Skip;
			_MaterialListWfNav.StepControl.DataSubmissionMode = Personalization.DataSubmissionModeType.Skip;
			_MaterialListWfNav.StepControl.Hidden = true;

			_MaterialListWfNav.StepControl.DataChanged += _MaterialListWfNav_DataChanged;

			if (_MaterialListWfNav.Data == null)
			{
				_MaterialListStep.Data = null;
				_MaterialListWfNav.StepControl.Data = null;
			}
		}


		//-----------------------------------------------------------
		//
		//-----------------------------------------------------------
		void _MaterialListWfNav_DataChanged(object sender, EventArgs e)
		{
			var stackControl = Page.FindCamstarControl(_MaterialListWfNav.ClientID + "_Stack") as FieldControl;
			NamedSubentityRef[] stack = (stackControl.Data) as NamedSubentityRef[];
			JQDataGrid grdStack = Page.FindCamstarControl("MaterialList_ssWorkflowStack") as JQDataGrid;

			if (stack != null)
				grdStack.Data = stack;
			else
				grdStack.Data = null;

			if (_MaterialListWfNav.StepControl.Data != null)
				_MaterialListStep.Data = _MaterialListWfNav.StepControl.Data;
			else
				_MaterialListStep.Data = null;        

			stackControl.Data = null;
			CamstarWebControl.SetRenderToClient(grdStack);
			CamstarWebControl.SetRenderToClient(_MaterialListStep);
		}

		//-----------------------------------------------------------
		//
		//-----------------------------------------------------------
		public override void GetInputData(Service serviceData)
		{
			base.GetInputData(serviceData);
			if (serviceData != null)
				if ((serviceData as ProductBOMMaint).ObjectChanges != null)
					if ((serviceData as ProductBOMMaint).ObjectChanges.MaterialList != null)
					{
						JQDataGrid _gridMaterialList = Page.FindCamstarControl("ObjectChanges_MaterialList") as JQDataGrid;
						ProductBOMMaterialListChanges[] oMaterialList = _gridMaterialList.BoundContext.Data as ProductBOMMaterialListChanges[];

						foreach (ProductBOMMaterialListChanges oMaterialListChanges in (serviceData as ProductBOMMaint).ObjectChanges.MaterialList)
						{
							if (oMaterialListChanges.ListItemAction == ListItemAction.Change)
							{
								oMaterialListChanges.ss_Workflow = new RevisionedObjectRef("");
								if (oMaterialList[int.Parse(oMaterialListChanges.ListItemIndex.ToString())].ss_Workflow != null)
									oMaterialListChanges.ss_Workflow = oMaterialList[int.Parse(oMaterialListChanges.ListItemIndex.ToString())].ss_Workflow;
							}

							if (oMaterialListChanges.ss_Step != null)
								if (oMaterialListChanges.ss_Workflow != null)
								{
									NamedSubentityRef WIPStep = new NamedSubentityRef() { Name = (oMaterialListChanges.ss_Step as NamedSubentityRef).Name, Parent = oMaterialListChanges.ss_Workflow as RevisionedObjectRef };
									oMaterialListChanges.ss_Step = WIPStep;
								}
						}
					}
		}
	}
}



