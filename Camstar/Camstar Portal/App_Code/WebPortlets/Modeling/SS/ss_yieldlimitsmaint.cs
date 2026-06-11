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
/// Summary description for SS_PrintingComputerMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_YieldLimitsMaint : SS_SetupBModelingBase
    {
        //Selection
        protected CWC.NamedObject Selection_YieldType { get { return Page.FindCamstarControl("Selection_YieldType") as CWC.NamedObject; } }
        protected CWC.RevisionedObject Selection_Product { get { return Page.FindCamstarControl("Selection_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject Selection_ProcessSpec { get { return Page.FindCamstarControl("Selection_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject Selection_ProductLine { get { return Page.FindCamstarControl("Selection_ProductLine") as CWC.NamedObject; } }
        protected CWC.NamedObject Selection_Owner { get { return Page.FindCamstarControl("Selection_Owner") as CWC.NamedObject; } }
        protected CWC.NamedObject Selection_WorkCenter { get { return Page.FindCamstarControl("Selection_WorkCenter") as CWC.NamedObject; } }
        protected CWC.RevisionedObject Selection_Spec { get { return Page.FindCamstarControl("Selection_Spec") as CWC.RevisionedObject; } }
		protected CWC.WorkflowNavigator Selection_WIPStepWfNavigator { get { return Page.FindCamstarControl("Selection_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }
        protected CWC.NamedObject Selection_ProcessType { get { return Page.FindCamstarControl("Selection_ProcessType") as CWC.NamedObject; } }

        //Criteria
        protected CWC.NamedObject ObjectChanges_YieldType { get { return Page.FindCamstarControl("ObjectChanges_YieldType") as CWC.NamedObject; } }
        protected CWC.RevisionedObject ObjectChanges_Product { get { return Page.FindCamstarControl("ObjectChanges_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject ObjectChanges_ProcessSpec { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject ObjectChanges_ProductLine { get { return Page.FindCamstarControl("ObjectChanges_ProductLine") as CWC.NamedObject; } }
        protected CWC.NamedObject ObjectChanges_Owner { get { return Page.FindCamstarControl("ObjectChanges_Owner") as CWC.NamedObject; } }
        protected CWC.NamedObject ObjectChanges_WorkCenter { get { return Page.FindCamstarControl("ObjectChanges_WorkCenter") as CWC.NamedObject; } }
        protected CWC.TextBox ObjectChanges_Name { get { return Page.FindCamstarControl("ObjectChanges_Name") as CWC.TextBox; } }
        protected CWC.TextBox ObjectChanges_Description { get { return Page.FindCamstarControl("ObjectChanges_Description") as CWC.TextBox; } }
        protected CWC.RevisionedObject ObjectChanges_Spec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
		protected CWC.WorkflowNavigator ObjectChanges_WIPStepWfNavigator { get { return Page.FindCamstarControl("ObjectChanges_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }
        protected CWC.NamedObject ObjectChanges_scsProcessType { get { return Page.FindCamstarControl("ObjectChanges_scsProcessType") as CWC.NamedObject; } }

        //Common Limits
        protected CWC.DropDownList ObjectChanges_UpperYieldFailureAction { get { return Page.FindCamstarControl("ObjectChanges_UpperYieldFailureAction") as CWC.DropDownList; } }
        protected CWC.DropDownList ObjectChanges_LowerYieldFailureAction { get { return Page.FindCamstarControl("ObjectChanges_LowerYieldFailureAction") as CWC.DropDownList; } }
        protected CWC.DropDownList ObjectChanges_YieldCutYieldFailureAction { get { return Page.FindCamstarControl("ObjectChanges_YieldCutYieldFailureAction") as CWC.DropDownList; } }
        protected CWC.NamedObject ObjectChanges_UpperYieldEmailGroup { get { return Page.FindCamstarControl("ObjectChanges_UpperYieldEmailGroup") as CWC.NamedObject; } }
        protected CWC.NamedObject ObjectChanges_LowerYieldEmailGroup { get { return Page.FindCamstarControl("ObjectChanges_LowerYieldEmailGroup") as CWC.NamedObject; } }
        protected CWC.NamedObject ObjectChanges_YieldCutYieldEmailGroup { get { return Page.FindCamstarControl("ObjectChanges_YieldCutYieldEmailGroup") as CWC.NamedObject; } }
        protected CWC.TextBox ObjectChanges_UpperYieldLimit { get { return Page.FindCamstarControl("ObjectChanges_UpperYieldLimit") as CWC.TextBox; } }
        protected CWC.TextBox ObjectChanges_LowerYieldLimit { get { return Page.FindCamstarControl("ObjectChanges_LowerYieldLimit") as CWC.TextBox; } }
        protected CWC.TextBox ObjectChanges_YieldCutYieldLimit { get { return Page.FindCamstarControl("ObjectChanges_YieldCutYieldLimit") as CWC.TextBox; } }

        //Lot Size Limits
        protected CWC.CheckBox ObjectChanges_LotSizeLimitsRequired { get { return Page.FindCamstarControl("ObjectChanges_LotSizeLimitsRequired") as CWC.CheckBox; } }
        protected CWC.DropDownList ObjectChanges_LotSizeLowerFailureAction { get { return Page.FindCamstarControl("ObjectChanges_LotSizeLowerFailureAction") as CWC.DropDownList; } }
        protected CWC.NamedObject ObjectChanges_LotSizeLowerEmailGroup { get { return Page.FindCamstarControl("ObjectChanges_LotSizeLowerEmailGroup") as CWC.NamedObject; } }
        protected CWC.DropDownList ObjectChanges_LotSizeYieldCutFailureAction { get { return Page.FindCamstarControl("ObjectChanges_LotSizeYieldCutFailureAction") as CWC.DropDownList; } }
        protected CWC.NamedObject ObjectChanges_LotSizeYieldCutEmailGroup { get { return Page.FindCamstarControl("ObjectChanges_LotSizeYieldCutEmailGroup") as CWC.NamedObject; } }
        protected JQDataGrid ObjectChanges_LotSizeLimits { get { return Page.FindCamstarControl("ObjectChanges_LotSizeLimits") as JQDataGrid; } }

        //Loss Reasons Limits
        protected CWC.CheckBox ObjectChanges_LossReasonLimitsRequired { get { return Page.FindCamstarControl("ObjectChanges_LossReasonLimitsRequired") as CWC.CheckBox; } }
        protected CWC.DropDownList ObjectChanges_LossReasonLimitsFailureAction { get { return Page.FindCamstarControl("ObjectChanges_LossReasonLimitsFailureAction") as CWC.DropDownList; } }
        protected CWC.NamedObject ObjectChanges_LossReasonLimitsEmailGroup { get { return Page.FindCamstarControl("ObjectChanges_LossReasonLimitsEmailGroup") as CWC.NamedObject; } }
        protected JQDataGrid ObjectChanges_LossReasonLimits { get { return Page.FindCamstarControl("ObjectChanges_LossReasonLimits") as JQDataGrid; } }

        //Bins Limits
        protected CWC.CheckBox ObjectChanges_BinsLimitsRequired { get { return Page.FindCamstarControl("ObjectChanges_BinsLimitsRequired") as CWC.CheckBox; } }
        protected CWC.DropDownList ObjectChanges_BinsLimitsFailureAction { get { return Page.FindCamstarControl("ObjectChanges_BinsLimitsFailureAction") as CWC.DropDownList; } }
        protected CWC.NamedObject ObjectChanges_BinsLimitsEmailGroup { get { return Page.FindCamstarControl("ObjectChanges_BinsLimitsEmailGroup") as CWC.NamedObject; } }
        protected JQDataGrid ObjectChanges_BinsLimits { get { return Page.FindCamstarControl("ObjectChanges_BinsLimits") as JQDataGrid; } }
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //add the selection WP controls to the control collection
            _SelectionControls.Add(Selection_YieldType);
            _SelectionControls.Add(Selection_Product);
            _SelectionControls.Add(Selection_ProcessSpec);
            _SelectionControls.Add(Selection_ProductLine);
            _SelectionControls.Add(Selection_Owner);
            _SelectionControls.Add(Selection_WorkCenter);
            _SelectionControls.Add(Selection_Spec);
			_SelectionControls.Add(Selection_WIPStepWfNavigator);
            _SelectionControls.Add(Selection_ProcessType);

            // add the criteria WP controls to the control collection
            _CriteriaControls.Add(ObjectChanges_YieldType);
            _CriteriaControls.Add(ObjectChanges_Product);
            _CriteriaControls.Add(ObjectChanges_ProcessSpec);
            _CriteriaControls.Add(ObjectChanges_ProductLine);
            _CriteriaControls.Add(ObjectChanges_Owner);
            _CriteriaControls.Add(ObjectChanges_WorkCenter);
            _CriteriaControls.Add(ObjectChanges_Spec);
			_CriteriaControls.Add(ObjectChanges_WIPStepWfNavigator);
            _CriteriaControls.Add(ObjectChanges_scsProcessType);

            _SubentityGridControls.Add(ObjectChanges_LotSizeLimits);
            _SubentityGridControls.Add(ObjectChanges_LossReasonLimits);
            _SubentityGridControls.Add(ObjectChanges_BinsLimits);

            _HiddenGridColumns.Add("RN");
            _CriteriaWorkflowNavigators.Add(ObjectChanges_WIPStepWfNavigator);

            ObjectChanges_WIPStepWfNavigator.DataChanged += ObjectChanges_WIPStepWfNavigator_DataChanged;
        }

        void ObjectChanges_WIPStepWfNavigator_DataChanged(object sender, EventArgs e)
        {
            SS_SetupB_StackData_Update("ObjectChanges_WIPStepWfNavigator");
        }
    }
}

