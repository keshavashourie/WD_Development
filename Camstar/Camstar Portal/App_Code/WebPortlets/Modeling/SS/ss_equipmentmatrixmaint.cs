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
/// Summary description for SS_EquipmentMatrixMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_EquipmentMatrixMaint : SS_SetupBModelingBaseR2
    {
        protected CWC.RevisionedObject _rdoSelectionProduct { get { return Page.FindCamstarControl("Selection_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoSelectionProcessSpec { get { return Page.FindCamstarControl("Selection_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject _ndoSelectionProductLine { get { return Page.FindCamstarControl("Selection_ProductLine") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoSelectionOwner { get { return Page.FindCamstarControl("Selection_Owner") as CWC.NamedObject; } }
        protected CWC.RevisionedObject _rdoSelectionSpec { get { return Page.FindCamstarControl("Selection_Spec") as CWC.RevisionedObject; } }
        protected CWC.WorkflowNavigator _wfSelectionWIPStepWf { get { return Page.FindCamstarControl("Selection_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }

        protected CWC.RevisionedObject _rdoProduct { get { return Page.FindCamstarControl("ObjectChanges_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject _rdoProcessSpec { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject _ndoProductLine { get { return Page.FindCamstarControl("ObjectChanges_ProductLine") as CWC.NamedObject; } }
        protected CWC.NamedObject _ndoOwner { get { return Page.FindCamstarControl("ObjectChanges_Owner") as CWC.NamedObject; } }
        protected CWC.RevisionedObject _rdoSpec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject _ndoEquipmentGroup { get { return Page.FindCamstarControl("ObjectChanges_EquipmentGroup") as CWC.NamedObject; } }
        protected CWC.WorkflowNavigator _wfWIPStepWf { get { return Page.FindCamstarControl("ObjectChanges_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }

        protected JQDataGrid _gridExceptions { get { return Page.FindCamstarControl("ObjectChanges_Exceptions") as JQDataGrid; } }

        //---------------------------------------------------
        // Override OnLoad event
        //---------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //commented out due to use portal studio setting to populate the equipment after select equipment group from drop down list
            //PR251817 - TAC10213847 - Equipment field's drop down filter doesn't work on Equipment Matrices' Exception
            //_ndoEquipmentGroup.DataChanged += new EventHandler(EquipmentGroupField_DataChanged);
            //EquipmentGroupField_DataChanged(null, null);

            //add the selection WP controls to the control collection
            _SelectionControls.Add(_rdoSelectionProduct);
            _SelectionControls.Add(_rdoSelectionProcessSpec);
            _SelectionControls.Add(_ndoSelectionProductLine);
            _SelectionControls.Add(_ndoSelectionOwner);
            _SelectionControls.Add(_rdoSelectionSpec);
            _SelectionControls.Add(_wfSelectionWIPStepWf);

            // add the criteria WP controls to the control collection
            _CriteriaControls.Add(_rdoProduct);
            _CriteriaControls.Add(_rdoProcessSpec);
            _CriteriaControls.Add(_ndoProductLine);
            _CriteriaControls.Add(_ndoOwner);
            _CriteriaControls.Add(_rdoSpec);
            _CriteriaControls.Add(_wfWIPStepWf);

            // add any subentity grid controls to the control collection
            _SubentityGridControls.Add(_gridExceptions);

            // specify the hidden colums for the selection grid.
            _HiddenGridColumns.Add("RN");

            _CriteriaWorkflowNavigators.Add(_wfWIPStepWf);

            _wfWIPStepWf.DataChanged += _wfWIPStepWf_DataChanged;
			string viewDM = Page.PortalContext.DataContract.GetValueByName<string>("PopupDM");

            SEMI.AppCode.UIUtility.DisableMatrixFields(this, viewDM, _CriteriaControls);

            if (viewDM != null && viewDM == "View")
            {
                (_gridExceptions.GridContext as BoundContext).Fields["Equipment"].Editable = false;
                (_gridExceptions.GridContext as BoundContext).Fields["Comments"].Editable = false;
            }																				  
        }

        void _wfWIPStepWf_DataChanged(object sender, EventArgs e)
        {
            SS_SetupB_StackData_Update("ObjectChanges_WIPStepWfNavigator");
        }

        //-----------------------------------------
        // Equipment Group Field Data Changed Event
        //-----------------------------------------
        public void EquipmentGroupField_DataChanged(object sender, EventArgs e)
        {
            try
            {
                if (_ndoEquipmentGroup.Data != null)
                {
                    //Initialize Service & Objects
                    UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                    EquipmentMatrixMaintService Svc = new EquipmentMatrixMaintService(profile);
                    EquipmentMatrixMaint SvcData = new EquipmentMatrixMaint();
                    EquipmentMatrixChanges objChanges = new EquipmentMatrixChanges();
                    EquipmentMatrixChanges_Info objChangesInfo = new EquipmentMatrixChanges_Info();
                    EquipmentMatrixMaint_Info SvcInfo = new EquipmentMatrixMaint_Info();
                    EquipmentMatrixMaint_Request ReqData = new EquipmentMatrixMaint_Request();
                    EquipmentMatrixMaint_Result ResData = new EquipmentMatrixMaint_Result();

                    objChanges.EquipmentGroup = new NamedObjectRef();
                    objChanges.EquipmentGroup.Name = _ndoEquipmentGroup.Data.ToString();
                    SvcData.ObjectChanges = objChanges;
                    objChangesInfo.ExceptionEquipment = new Info(true);
                    objChangesInfo.ExceptionEquipment.RequestSelectionValues = true;
                    SvcInfo.ObjectChanges = objChangesInfo;
                    ReqData.Info = SvcInfo;

                    //Execute Request
                    ResultStatus Results = Svc.GetEnvironment(SvcData, ReqData, out ResData);

                    //Result
                    if (Results.IsSuccess)
                    {
                        if (ResData.Environment.ObjectChanges.ExceptionEquipment != null)
                        {
                            var _equipmentInline = _gridExceptions.FindControl("ObjectChanges_Exceptions_Equipment_InlineEditorControl") as CWC.NamedObject;
                            _equipmentInline.PickListPanelControl.DataProvider = new Camstar.WebPortal.FormsFramework.WebControls.PickLists.StaticValuesDataProvider(ResData.Environment.ObjectChanges.ExceptionEquipment.SelectionValues);
                            CamstarWebControl.SetRenderToClient(_equipmentInline);
                        }
                    }
                    else
                    {
                        Page.DisplayMessage(Results);
                    }
                }
            }
            catch (Exception ex)
            {
                Page.StatusBar.WriteError(ex.Message.ToString());
            }
        }
    }
}



