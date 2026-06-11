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

using System.Collections;

/// <summary>
/// Summary description for SS_PackingQtyMaint
/// </summary>

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class SS_SPCMatrixMaint : SS_SetupBModelingBaseR2
    {
        protected CWC.RevisionedObject Selection_Product { get { return Page.FindCamstarControl("Selection_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject Selection_ProcessSpec { get { return Page.FindCamstarControl("Selection_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject Selection_ProductLine { get { return Page.FindCamstarControl("Selection_ProductLine") as CWC.NamedObject; } }
        protected CWC.NamedObject Selection_Owner { get { return Page.FindCamstarControl("Selection_Owner") as CWC.NamedObject; } }
        protected CWC.RevisionedObject Selection_Spec { get { return Page.FindCamstarControl("Selection_Spec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject Selection_ResourceFamily { get { return Page.FindCamstarControl("Selection_ResourceFamily") as CWC.NamedObject; } }
        protected CWC.RevisionedObject Selection_ProcessRecipe { get { return Page.FindCamstarControl("Selection_ProcessRecipe") as CWC.RevisionedObject; } }
        protected CWC.NamedObject Selection_ProcessEquipment { get { return Page.FindCamstarControl("Selection_ProcessEquipment") as CWC.NamedObject; } }
        protected CWC.NamedObject Selection_ProcessEquipmentFamily { get { return Page.FindCamstarControl("Selection_ProcessEquipmentFamily") as CWC.NamedObject; } }
        protected CWC.WorkflowNavigator _wfSelectionWIPStepWf { get { return Page.FindCamstarControl("Selection_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }

        protected CWC.RevisionedObject ObjectChanges_Product { get { return Page.FindCamstarControl("ObjectChanges_Product") as CWC.RevisionedObject; } }
        protected CWC.RevisionedObject ObjectChanges_ProcessSpec { get { return Page.FindCamstarControl("ObjectChanges_ProcessSpec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject ObjectChanges_ProductLine { get { return Page.FindCamstarControl("ObjectChanges_ProductLine") as CWC.NamedObject; } }
        protected CWC.NamedObject ObjectChanges_Owner { get { return Page.FindCamstarControl("ObjectChanges_Owner") as CWC.NamedObject; } }
        protected CWC.RevisionedObject ObjectChanges_Spec { get { return Page.FindCamstarControl("ObjectChanges_Spec") as CWC.RevisionedObject; } }
        protected CWC.NamedObject ObjectChanges_ResourceFamily { get { return Page.FindCamstarControl("ObjectChanges_ResourceFamily") as CWC.NamedObject; } }
        protected CWC.RevisionedObject ObjectChanges_ProcessRecipe { get { return Page.FindCamstarControl("ObjectChanges_ProcessRecipe") as CWC.RevisionedObject; } }
        protected CWC.NamedObject ObjectChanges_ProcessEquipment { get { return Page.FindCamstarControl("ObjectChanges_ProcessEquipment") as CWC.NamedObject; } }
        protected CWC.NamedObject ObjectChanges_ProcessEquipmentFamily { get { return Page.FindCamstarControl("ObjectChanges_ProcessEquipmentFamily") as CWC.NamedObject; } }
        protected CWC.NamedObject ObjectChanges_SPCSetup { get { return Page.FindCamstarControl("ObjectChanges_SPCSetup") as CWC.NamedObject; } }
        protected JQDataGrid ObjectChanges_Params { get { return Page.FindCamstarControl("ObjectChanges_Params") as JQDataGrid; } }
        protected JQDataGrid ObjectChanges_CustomSPCGridFilter { get { return Page.FindCamstarControl("ObjectChanges_ss_CustomSPCGridFilter") as JQDataGrid; } }
        protected CWC.TextBox ObjectChanges_Name { get { return Page.FindCamstarControl("ObjectChanges_Name") as CWC.TextBox; } }
        protected CWC.NamedObject ndoCustomSPCGridFilterTableEditor { get { return ObjectChanges_CustomSPCGridFilter.FindControl("ObjectChanges_ss_CustomSPCGridFilter_ss_SPCCustomFilterTable_InlineEditorControl") as CWC.NamedObject; } }
        protected CWC.DropDownList ddlCustomSPCGridFilterColumnEditor { get { return ObjectChanges_CustomSPCGridFilter.FindControl("ObjectChanges_ss_CustomSPCGridFilter_ss_Column_InlineEditorControl") as CWC.DropDownList; } }
        private CWC.TextBox _txtHiddenSelectedRowID { get { return Page.FindCamstarControl("HiddenSelectedRowIDTextBox") as CWC.TextBox; } }
        protected CWC.WorkflowNavigator _wfWIPStepWf { get { return Page.FindCamstarControl("ObjectChanges_WIPStepWfNavigator") as CWC.WorkflowNavigator; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _SelectionControls.Add(Selection_Product);
            _SelectionControls.Add(Selection_ProcessSpec);
            _SelectionControls.Add(Selection_ProductLine);
            _SelectionControls.Add(Selection_Owner);
            _SelectionControls.Add(Selection_Spec);
            _SelectionControls.Add(Selection_ResourceFamily);
            _SelectionControls.Add(Selection_ProcessRecipe);
            _SelectionControls.Add(Selection_ProcessEquipment);
            _SelectionControls.Add(Selection_ProcessEquipmentFamily);
            _SelectionControls.Add(_wfSelectionWIPStepWf);

            // add the criteria WP controls to the control collection
            _CriteriaControls.Add(ObjectChanges_Product);
            _CriteriaControls.Add(ObjectChanges_ProcessSpec);
            _CriteriaControls.Add(ObjectChanges_ProductLine);
            _CriteriaControls.Add(ObjectChanges_Owner);
            _CriteriaControls.Add(ObjectChanges_Spec);
            _CriteriaControls.Add(ObjectChanges_ResourceFamily);
            _CriteriaControls.Add(ObjectChanges_SPCSetup);
            _SubentityGridControls.Add(ObjectChanges_Params);
            _CriteriaControls.Add(ObjectChanges_ProcessRecipe);
            _CriteriaControls.Add(ObjectChanges_ProcessEquipment);
            _CriteriaControls.Add(ObjectChanges_ProcessEquipmentFamily);
            _SubentityGridControls.Add(ObjectChanges_CustomSPCGridFilter);
            _CriteriaControls.Add(_wfWIPStepWf);

            // specify the hidden colums for the selection grid.
            _HiddenGridColumns.Add("RN");
            //_HiddenGridColumns.Add("Description");

            //detect if there is there is value coming from the selection value pop
            if (Page.DataContract.GetValueByName("SPCSetup") != null)
            {
                ObjectChanges_SPCSetup.Data = Page.DataContract.GetValueByName("SPCSetup");
                Page.DataContract.SetValueByName("SPCSetup", null);
                ObjectChanges_SPCSetup_DataChanged(null, null);
            }

            if (Page.IsPostBack)
                ObjectChanges_SPCSetup.DataChanged +=ObjectChanges_SPCSetup_DataChanged;

            _CriteriaWorkflowNavigators.Add(_wfWIPStepWf);
            _wfWIPStepWf.DataChanged += _wfWIPStepWf_DataChanged;

            if (Page.IsPostBack)
                ddlCustomSPCGridFilterColumnEditor.DisplayingData += new EventHandler<CWC.PickLists.DataRequestEventArgs>(ddlCustomSPCGridFilterColumnEditor_DisplayingData);

            string viewDM = Page.PortalContext.DataContract.GetValueByName<string>("PopupDM");
            SEMI.AppCode.UIUtility.DisableMatrixFields(this, viewDM, _CriteriaControls);
            if (viewDM != null && viewDM == "View")
            {
               (ObjectChanges_Params.GridContext as BoundContext).EditingMode = JQEditingModes.Disabled;
                (ObjectChanges_Params.GridContext as BoundContext).RowSelectionMode = JQGridSelectionMode.Disable;

                (ObjectChanges_CustomSPCGridFilter.GridContext as BoundContext).EditingMode = JQEditingModes.Disabled;
                (ObjectChanges_CustomSPCGridFilter.GridContext as BoundContext).RowSelectionMode = JQGridSelectionMode.Disable;

            }

        }

        void _wfWIPStepWf_DataChanged(object sender, EventArgs e)
        {
            SS_SetupB_StackData_Update("ObjectChanges_WIPStepWfNavigator");
        }

        void ddlCustomSPCGridFilterColumnEditor_DisplayingData(object sender, CWC.PickLists.DataRequestEventArgs e)
        {
            if (e.TotalRecords == 0 && !string.IsNullOrEmpty(ndoCustomSPCGridFilterTableEditor.Text))
            {
                var fs = FrameworkManagerUtil.GetFrameworkSession();
                Result objResult = new Result();

                // init the service, service data and service info objects
                UserProfile profile = FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
                SPCMatrixMaintService Svc = new SPCMatrixMaintService(profile);
                SPCMatrixMaint SvcData = new SPCMatrixMaint();
                SPCMatrixChanges_Info objChangesInfo = new SPCMatrixChanges_Info() { ss_CustomSPCGridFilter = new ss_CustomSPCGridFilterChanges_Info() };
                SPCMatrixMaint_Info SvcInfo = new SPCMatrixMaint_Info();
                SPCMatrixMaint_Request ReqData = new SPCMatrixMaint_Request();
                SPCMatrixMaint_Result ResData = new SPCMatrixMaint_Result();

                SvcData.ObjectChanges = new SPCMatrixChanges();
                SvcData.ObjectChanges.ss_CustomSPCGridFilter = new ss_CustomSPCGridFilterChanges[1] { new ss_CustomSPCGridFilterChanges() };
                SvcData.ObjectChanges.ss_CustomSPCGridFilter[0].ss_SPCCustomFilterTable = ndoCustomSPCGridFilterTableEditor.Data as NamedObjectRef;
                objChangesInfo.ss_CustomSPCGridFilter.ss_Column = FieldInfoUtil.RequestSelectionValue();
                SvcInfo.ObjectChanges = objChangesInfo;
                ReqData.Info = SvcInfo;

                //Execute Request
                ResultStatus Results = Svc.GetEnvironment(SvcData, ReqData, out ResData);
                if (Results.IsSuccess && ResData.Environment.ObjectChanges.ss_CustomSPCGridFilter.ss_Column.SelectionValues.Rows != null)
                {
                    ddlCustomSPCGridFilterColumnEditor.SetSelectionValues(ResData.Environment.ObjectChanges.ss_CustomSPCGridFilter.ss_Column.SelectionValues);
                    CamstarWebControl.SetRenderToClient(ddlCustomSPCGridFilterColumnEditor);
                }


            }
        }

        public void ObjectChanges_SPCSetup_DataChanged(object sender, EventArgs e)
        {
            if (ObjectChanges_SPCSetup.Data != null)
            {
                Camstar.WCF.ObjectStack.UserProfile profile = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.UserProfile] as Camstar.WCF.ObjectStack.UserProfile;

                var isInlineSPC = false;
                string scsInlineSPCFlagQuery = string.Format(@"SELECT A.scsInlineSPC AS IsInlineSPC FROM A_SPCSetup A WHERE A.SPCSetupName = '{0}'", ObjectChanges_SPCSetup.Data);

                QueryService scsObjQueryService = new QueryService(profile);
                // set the query options
                OM.QueryOptions scsObjQueryOptions = new OM.QueryOptions();
                scsObjQueryOptions.StartRow = 1;
                scsObjQueryOptions.RowSetSize = 1;
                OM.RecordSet scsObjRecordSet = new OM.RecordSet();
                OM.ResultStatus scsObjResultStatus = scsObjQueryService.ExecuteAdHoc(scsInlineSPCFlagQuery, scsObjQueryOptions, out scsObjRecordSet);
                if (scsObjResultStatus.IsSuccess)
                {
                    DataTable scsObjDT = scsObjRecordSet.GetAsDataTable();
                    if (scsObjDT.Rows.Count > 0)
                    {
                        DataRow row = scsObjDT.Rows[0];
                        var data = row["IsInlineSPC"];
                        if (data != null && data.ToString().ToLower() == "true" || data.ToString() == "1")
                        {
                            isInlineSPC = true;
                        }
                        else
                        {
                            isInlineSPC = false;
                        }
                    }
                }

                SPCSetupMaintService oService = new SPCSetupMaintService(profile);
                SPCSetupMaint oServiceData = new SPCSetupMaint();
                SPCSetupMaint_Info oServiceInfo = new SPCSetupMaint_Info();
                SPCSetupChanges oChanges = new SPCSetupChanges();
                SPCSetupChanges_Info oChangesInfo = new SPCSetupChanges_Info();

                //set the input value
                oChanges.Name = ObjectChanges_SPCSetup.TextEditControl.Text;
                oServiceData.ObjectToChange = new NamedObjectRef(ObjectChanges_SPCSetup.Data.ToString());
                //set the output request
                oChangesInfo.Details = new SPCSetupDetailsChanges_Info();
                oChangesInfo.Details.Name = FieldInfoUtil.RequestValue();
                if (!isInlineSPC)
                {
                    oChangesInfo.Details.Params = new SPCSetupDetailsParamsChanges_Info();
                    oChangesInfo.Details.Params.ParamName = FieldInfoUtil.RequestValue();
                    oChangesInfo.Details.Params.ParamValue = FieldInfoUtil.RequestValue();
                    oChangesInfo.Details.Params.AllowMatrixOverride = FieldInfoUtil.RequestValue();
                }
                else
                {
                    oChangesInfo.Details.ParamsInline = new SPCSetupDetailsILParamsChanges_Info();
                    oChangesInfo.Details.ParamsInline.ParamName = FieldInfoUtil.RequestValue();
                    oChangesInfo.Details.ParamsInline.ParamValue = FieldInfoUtil.RequestValue();
                    oChangesInfo.Details.ParamsInline.AllowMatrixOverride = FieldInfoUtil.RequestValue();
                }
                //Set the object changes 
                oServiceData.ObjectChanges = oChanges;
                oServiceInfo.ObjectChanges = oChangesInfo;
                SPCSetupMaint_Result oResult = new SPCSetupMaint_Result();
                ResultStatus oResultStatus = oService.Load(oServiceData, new SPCSetupMaint_Request { Info = oServiceInfo }, out oResult);

                if (oResultStatus.IsSuccess)
                {
                    if (oResult.Value.ObjectChanges.Details.Count() > 0)
                    {
                        List<SPCMatrixParamsChanges> oSPCMatrixParam = new List<SPCMatrixParamsChanges>();

                        // comment out to avoid building the param grid ontop of existing records
                        //	if (ObjectChanges_Params.GridContext.GetTotalRows() > 0)
                        //	{
                        //		for (int i = 0; i < ObjectChanges_Params.GridContext.GetTotalRows(); i++)
                        //		{
                        //			string strParamsRowID = i.ToString().PadLeft(6, '0');
                        //			SPCMatrixParamsChanges oSPCMatrixParamTemp = new SPCMatrixParamsChanges();
                        //			oSPCMatrixParamTemp.Name = ObjectChanges_Params.GridContext.GetCell(strParamsRowID, "Name").ToString();
                        //			oSPCMatrixParamTemp.ParamName = ObjectChanges_Params.GridContext.GetCell(strParamsRowID, "ParamName").ToString();
                        //			oSPCMatrixParamTemp.ParamValue = ObjectChanges_Params.GridContext.GetCell(strParamsRowID, "ParamValue").ToString();
                        //			oSPCMatrixParam.Add(oSPCMatrixParamTemp);
                        //		}
                        //	}

                        foreach (SPCSetupDetailsChanges oSPCSetupDetailsItem in oResult.Value.ObjectChanges.Details)
                        {
                            if (oSPCSetupDetailsItem.Params != null)
                            {
                                SPCSetupDetailsParamsChanges[] oSPCSetupDetailsParams = oSPCSetupDetailsItem.Params;

                                foreach (SPCSetupDetailsParamsChanges oSPCSetupDetailsParamsItem in oSPCSetupDetailsParams)
                                {
                                    if (oSPCSetupDetailsParamsItem.AllowMatrixOverride == true)
                                    {
                                        SPCMatrixParamsChanges oSPCMatrixParamTemp = new SPCMatrixParamsChanges();
                                        oSPCMatrixParamTemp.Name = oSPCSetupDetailsItem.Name;
                                        oSPCMatrixParamTemp.ParamName = oSPCSetupDetailsParamsItem.ParamName;
                                        oSPCMatrixParamTemp.ParamValue = oSPCSetupDetailsParamsItem.ParamValue;
                                        oSPCMatrixParam.Add(oSPCMatrixParamTemp);
                                    }
                                }
                            }
                            else if (oSPCSetupDetailsItem.ParamsInline != null)
                            {
                                SPCSetupDetailsILParamsChanges[] oSPCSetupDetailsParams = oSPCSetupDetailsItem.ParamsInline;

                                foreach (SPCSetupDetailsILParamsChanges oSPCSetupDetailsParamsItem in oSPCSetupDetailsParams)
                                {
                                    if (oSPCSetupDetailsParamsItem.AllowMatrixOverride == true)
                                    {
                                        SPCMatrixParamsChanges oSPCMatrixParamTemp = new SPCMatrixParamsChanges();
                                        oSPCMatrixParamTemp.Name = oSPCSetupDetailsItem.Name;
                                        oSPCMatrixParamTemp.ParamName = oSPCSetupDetailsParamsItem.ParamName;
                                        oSPCMatrixParamTemp.ParamValue = oSPCSetupDetailsParamsItem.ParamValue;
                                        oSPCMatrixParam.Add(oSPCMatrixParamTemp);
                                    }
                                }
                            }
                        }
                        ObjectChanges_Params.ClearData();
                        ObjectChanges_Params.Data = oSPCMatrixParam.ToArray();
                        ObjectChanges_Params.GridContext.LoadData();
                        CamstarWebControl.SetRenderToClient(ObjectChanges_Params);
                    }
                }
                else
                {
                    Page.DisplayMessage(oResultStatus);
                }
            }
        }
    }
}



