/* Copyright 2022 Siemens */
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
/// Summary description for SS_WIPDataSetupMatrixMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_WIPDataSetupMatrixMaint : SS_SetupBModelingBaseR2
    {
		protected CWC.RevisionedObject Selection_Product { get { return Page.FindCamstarControl("Selection_Product") as CWC.RevisionedObject; } }
		protected CWC.RevisionedObject Selection_ProcessSpec { get { return Page.FindCamstarControl("Selection_ProcessSpec") as CWC.RevisionedObject; } }
		protected CWC.RevisionedObject Selection_Spec { get { return Page.FindCamstarControl("Selection_Spec") as CWC.RevisionedObject; } }
		protected CWC.WorkflowNavigator Selection_WIPStepWfNavigator { get { return Page.FindCamstarControl("Selection_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }
		protected CWC.NamedObject Selection_ProcessType { get { return Page.FindCamstarControl("Selection_ProcessType") as CWC.NamedObject; } }
		protected CWC.NamedObject Selection_ProductLine { get { return Page.FindCamstarControl("Selection_ProductLine") as CWC.NamedObject; } }
		protected CWC.NamedObject Selection_EquipmentFamily { get { return Page.FindCamstarControl("Selection_EquipmentFamily") as CWC.NamedObject; } }
		protected CWC.NamedObject Selection_Equipment { get { return Page.FindCamstarControl("Selection_Equipment") as CWC.NamedObject; } }
		
		protected CWC.RevisionedObject ObjectChanges_Product { get { return Page.FindCamstarControl("ObjectChanges_Product") as CWC.RevisionedObject; } }
		protected CWC.RevisionedObject ObjectChanges_ProcessSpec { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpec") as CWC.RevisionedObject; } }
		protected CWC.RevisionedObject ObjectChanges_Spec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
		protected CWC.WorkflowNavigator ObjectChanges_WIPStepWfNavigator { get { return Page.FindCamstarControl("ObjectChanges_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }
		protected CWC.NamedObject ObjectChanges_ProcessType { get { return Page.FindCamstarControl("ObjectChanges_ProcessType") as CWC.NamedObject; } }
		protected CWC.NamedObject ObjectChanges_ProductLine { get { return Page.FindCamstarControl("ObjectChanges_ProductLine") as CWC.NamedObject; } }
		protected CWC.NamedObject ObjectChanges_EquipmentFamily { get { return Page.FindCamstarControl("ObjectChanges_EquipmentFamily") as CWC.NamedObject; } }
		protected CWC.NamedObject ObjectChanges_Equipment { get { return Page.FindCamstarControl("ObjectChanges_Equipment") as CWC.NamedObject; } }


		protected CWC.NamedObject ObjectChanges_WIPDataSetup { get { return Page.FindCamstarControl("ObjectChanges_WIPDataSetup") as CWC.NamedObject; } }
		protected CWC.TextBox _txtSelectionName { get { return Page.FindCamstarControl("SelectionName") as CWC.TextBox; } }

		protected JQDataGrid ObjectChanges_ss_WIPDataDetails { get { return Page.FindCamstarControl("ObjectChanges_Details") as JQDataGrid; } }
		
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
			ObjectChanges_WIPDataSetup.DataChanged += ObjectChanges_WIPDataSetup_DataChanged;
			if (Page.EventArgument == "FloatingFrameSubmitParentPostBackArgument" && Page.DataContract.GetValueByName("WIPDataSetup") != null)
			{
			var strWIPDataSetup = Page.DataContract.GetValueByName("WIPDataSetup");
			Page.DataContract.SetValueByName("WIPDataSetup", null);
			ObjectChanges_WIPDataSetup.Data = strWIPDataSetup;
			}
            //add the selection WP controls to the control collection
			_SelectionControls.Add(Selection_Product);
			_SelectionControls.Add(Selection_ProcessSpec);
			_SelectionControls.Add(Selection_Spec);
			_SelectionControls.Add(Selection_WIPStepWfNavigator);
			_SelectionControls.Add(Selection_ProcessType);
			_SelectionControls.Add(Selection_ProductLine);			
			_SelectionControls.Add(Selection_EquipmentFamily);
			_SelectionControls.Add(Selection_Equipment);          

            // add the criteria WP controls to the control collection
			_CriteriaControls.Add(ObjectChanges_Product);
			_CriteriaControls.Add(ObjectChanges_ProcessSpec);
			_CriteriaControls.Add(ObjectChanges_Spec);
			_CriteriaControls.Add(ObjectChanges_WIPStepWfNavigator);
			_CriteriaControls.Add(ObjectChanges_ProcessType);
			_CriteriaControls.Add(ObjectChanges_ProductLine);                     
			_CriteriaControls.Add(ObjectChanges_EquipmentFamily);
			_CriteriaControls.Add(ObjectChanges_Equipment);

			_SubentityGridControls.Add(ObjectChanges_ss_WIPDataDetails);
			
            // specify the hidden colums for the selection grid.
            _HiddenGridColumns.Add("RN");

			_CriteriaWorkflowNavigators.Add(ObjectChanges_WIPStepWfNavigator);
			ObjectChanges_WIPStepWfNavigator.DataChanged += ObjectChanges_WIPStepWfNavigator_DataChanged;

            string viewDM = Page.PortalContext.DataContract.GetValueByName<string>("PopupDM");

            SEMI.AppCode.UIUtility.DisableMatrixFields(this, viewDM, _CriteriaControls);
            if (viewDM != null && viewDM == "View")
            {
                //(ObjectChanges_ss_WIPDataDetails.GridContext as BoundContext).EditingMode = JQEditingModes.Disabled;
            }

        }

        void ObjectChanges_WIPDataSetup_DataChanged(object sender, EventArgs e)
		{
			if (ObjectChanges_WIPDataSetup.Data != null)
			{
				var fs = FrameworkManagerUtil.GetFrameworkSession();
				Result objResult = new Result();

				// init the service, service data and service info objects
				UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
				ss_WIPDataSetupMatrixMaintService Svc = new ss_WIPDataSetupMatrixMaintService(profile);
				ss_WIPDataSetupMatrixMaint SvcData = new ss_WIPDataSetupMatrixMaint();
				ss_WIPDataSetupMatrixChanges_Info objChangesInfo = new ss_WIPDataSetupMatrixChanges_Info();
				ss_WIPDataSetupMatrixMaint_Info SvcInfo = new ss_WIPDataSetupMatrixMaint_Info();
				ss_WIPDataSetupMatrixMaint_Request ReqData = new ss_WIPDataSetupMatrixMaint_Request();
				ss_WIPDataSetupMatrixMaint_Result ResData = new ss_WIPDataSetupMatrixMaint_Result();

				SvcData.ObjectChanges = new ss_WIPDataSetupMatrixChanges();
				if (_txtSelectionName.Data != null)
					SvcData.ObjectChanges.Name = _txtSelectionName.Data.ToString();
				SvcData.ObjectChanges.ss_WIPDataSetup = new NamedObjectRef(ObjectChanges_WIPDataSetup.Data.ToString());
				objChangesInfo.ss_WIPDataDetailsSelection = new WIPDataSetupDetailsChanges_Info() { RequestValue = true };
				SvcInfo.ObjectChanges = objChangesInfo;
				ReqData.Info = SvcInfo;

				//Execute Request
				ResultStatus Results = Svc.GetEnvironment(SvcData, ReqData, out ResData);
				if (Results.IsSuccess && ResData.Value.ObjectChanges.ss_WIPDataDetailsSelection != null)
				{
					ObjectChanges_ss_WIPDataDetails.Data = ResData.Value.ObjectChanges.ss_WIPDataDetailsSelection;

					CamstarWebControl.SetRenderToClient(ObjectChanges_ss_WIPDataDetails);
				}
			}
			else
				ObjectChanges_ss_WIPDataDetails.ClearData();
		}

        public override void _SetupB_LoadObject()
        {
            base._SetupB_LoadObject();
            ObjectChanges_WIPDataSetup_DataChanged(null,null);
        }

        void ObjectChanges_WIPStepWfNavigator_DataChanged(object sender, EventArgs e)
		{
			SS_SetupB_StackData_Update("ObjectChanges_WIPStepWfNavigator");
		}
    }
}



