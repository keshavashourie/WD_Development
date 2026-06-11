// Copyright Siemens 2023
using System;
using System.Linq;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Personalization;
using System.Collections.Generic;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    public class SPCQuery : MatrixWebPart
    {
        protected virtual JQDataGrid UserQueryParamsGrid { get { return Page.FindCamstarControl("UserQueryParams") as JQDataGrid; } }
        protected virtual JQDataGrid InstanceGrid { get { return Page.FindCamstarControl("InstanceGrid") as JQDataGrid; } }
        protected virtual TextBox QueryText { get { return Page.FindCamstarControl("QueryText") as TextBox; } }

        protected virtual Button SaveAndTest
        {
            get { return Page.FindCamstarControl("SaveAndTest") as Button; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SaveAndTest.Click += SaveAndTest_Click;
        }

        protected virtual void SaveAndTest_Click(object sender, EventArgs e)
        {
            Page.DataContract.SetValueByName("QueryTxt", QueryText.Data);
            OpenTestUserQueryPage();
        }

        protected override void OnPreRender(EventArgs e)
        {
            SetAsStartupScriptResetDirtyAfterSaveAndTest();

            base.OnPreRender(e);

            RenderToClient = true;
        }

        private void SetAsStartupScriptResetDirtyAfterSaveAndTest()
        {
            string script = "resetDirtyAfterSaveAndTest();";
            string value = shouldResetDirty.ToString().ToLower();
            script = $"var shouldResetDirty = {value}; " + script;
            ScriptManager.RegisterStartupScript(this, GetType(), "resetDirtyAfterSaveAndTest", script, true);
        }

        public virtual void OpenTestUserQueryPage()
        {
            QueryParameters queryParams = null;
            Dictionary<string, int?> ParameterDataTypeMap = new Dictionary<string, int?>();
            OnlineQuerySetupParamsChanges[] parameterChanges = UserQueryParamsGrid.Data as OnlineQuerySetupParamsChanges[];

            if (UserQueryParamsGrid.Data != null)
            {
                queryParams = new QueryParameters(); //int[] numbers = new int[5];
                try
                {
                    QueryParameter[] qp = new QueryParameter[parameterChanges.Count()];
                    for (int i = 0; i < parameterChanges.Count(); i++)
                    {
                        qp[i] = new QueryParameter(parameterChanges[i].Name.ToString(), parameterChanges[i]?.DefaultValue?.Value);
                        ParameterDataTypeMap.Add(parameterChanges[i].Name.Value, parameterChanges[i].DataType == null ? (int?)null : parameterChanges[i].DataType.Value);
                    }
                    queryParams.Parameters = qp;
                }
                catch (Exception ex)
                {
                    ResultStatus rs = new ResultStatus();
                    rs.Message = ex.Message;
                    DisplayMessage(rs);
                }
            }

            Page.DataContract.SetValueByName("ParameterTypeMap", ParameterDataTypeMap);

            ResultStatus res = AddOrUpdate();

            if (res.IsSuccess)
            {
                (Page.PortalContext as MaintenanceBehaviorContext).ReloadInstanceList = true;
                RefreshInstanceList();
                OpenTestUserQueryPage((Page.FindCamstarControl(txtName) as TextBox).Data.ToString(), queryParams);
            }
            else
            {
                DisplayMessage(res);
                return;
            }
        }

        private ResultStatus AddOrUpdate() 
        {
            //Perform value validation
            SPCQueryMaint inputForExecute = new SPCQueryMaint();
            Page.GetInputData(inputForExecute);

            ResultStatus resultStatus = Service.Form.ValidateInputData(inputForExecute);

            if (!resultStatus.IsSuccess)
                return resultStatus;

            SPCQueryMaintService service = Page.Service.GetService< SPCQueryMaintService >();
            service.BeginTransaction();

            MarkUsingNewEntity(false);

            var pc = Page.PortalContext as MaintenanceBehaviorContext;

            if (NotInEditMode(pc))
            {
                service.New(); //add new cdo
                MarkUsingNewEntity(true);
            }
            else
            {
                SPCQueryMaint input = GetPreparedInput();
                service.Load(input);
                Page.DataContract.SetValueByName(isNewDataMember, false);
            }
            service.ExecuteTransaction(inputForExecute);
            resultStatus = service.CommitTransaction();

            if (!resultStatus.IsSuccess)
            {
                MarkUsingNewEntity(false);
            }
            else
            {
                shouldResetDirty = true;

                if (pc.Current == null)
                    pc.Current = new NamedObjectRef()
                    {
                        Name = (Page.FindCamstarControl(txtName) as TextBox).Data.ToString()
                    };
                Page.LoadModelingValues(true);
            }
            return resultStatus;
        }

        private SPCQueryMaint GetPreparedInput()
        {
            return new SPCQueryMaint
            {
                ObjectToChange = new NamedObjectRef()
                {
                    Name = (Page.FindCamstarControl(txtName) as TextBox).OriginalData.ToString()
                }
            };
        }

        public void RefreshInstanceList()
        {
            var service = new SPCQueryMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var data = new SPCQueryMaint();
            var request = new SPCQueryMaint_Request()
            {
                Info = new SPCQueryMaint_Info()
                {
                    ObjectToChange = new Info { RequestSelectionValues = true }
                }
            };

            SPCQueryMaint_Result result = null;
            ResultStatus rs = service.GetEnvironment(data, request, out result);
            var pc = Page.PortalContext as MaintenanceBehaviorContext;

            if (rs.IsSuccess)
            {
                var result1 = result.Environment.ObjectToChange.SelectionValues;
                InstanceGrid.Data = null;
                InstanceGrid.Data = result1;
            }
        }

        protected virtual void OpenTestUserQueryPage(string queryName, QueryParameters queryParams)
        {
            FloatPageOpenAction floatAction = new FloatPageOpenAction();
            floatAction.FrameLocation = new UIFloatingPageLocation();
            floatAction.PageName = "UserQueryTestPopup_VP";
            floatAction.FrameLocation.Width = 700;
            floatAction.FrameLocation.Height = 520;
            floatAction.EndResponse = false;

            var label = FrameworkManagerUtil.GetLabelCache().GetLabelByName("Lbl_SPCQuery");

            ActionDispatcher dispatcher = Page.ActionDispatcher;
            dispatcher.DataContract.SetValueByName("UserQueryParamsDM", queryParams);
            dispatcher.DataContract.SetValueByName("isSPCQuery", true);
            dispatcher.DataContract.SetValueByName("UserQueryNameDM", queryName);
            dispatcher.DataContract.SetValueByName("OnlineQuerySetupParamsDM", UserQueryParamsGrid.Data);
            dispatcher.DataContract.SetValueByName("label", label?.DefaultValue);
            Page.MergedContent.DynamicWebParts.SingleOrDefault(item => item.Name == "MDL_Specific").DataContract.SetValueByName("UserQueryParamsDM", queryParams);
            dispatcher.ExecuteAction(floatAction);
        }

        private bool NotInEditMode(MaintenanceBehaviorContext portalContext)
        {
            if (portalContext.State != MaintenanceBehaviorContext.MaintenanceState.Edit
                && !IsUsingNewEntity()) 
            {
                return true;
            }
            return false;
        }
        
        private void MarkUsingNewEntity(bool value)
        {
            Page.DataContract.SetValueByName(isNewDataMember, value);
        }

        private bool IsUsingNewEntity()
        {
            return (bool)(Page.DataContract.GetValueByName(isNewDataMember) ?? false);
        }

        private const string isNewKey = "IsNew";

        private const string isNewDataMember = "IsNewDataMember";

        private const string txtName = "NameTxt";

        private bool shouldResetDirty = false;
    }
}
