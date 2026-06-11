// Copyright Siemens 2023
using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.UI;
using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Personalization;

namespace Camstar.WebPortal.WebPortlets.Modeling
{
    /// <summary>
    /// Summary description for OnlineQuerySetup
    /// </summary>
    /// 
    public class OnlineQuerySetup : MatrixWebPart
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

        protected virtual void SaveAndTest_Click(object sender, EventArgs e)
        {
            Page.DataContract.SetValueByName("QueryTxt", QueryText.Data);
            OpenTestUserQueryPage();
        }

        public virtual void OpenTestUserQueryPage()
        {
            QueryParameters queryParams = null;
            Dictionary<string, int?> ParameterDataTypeMap = new Dictionary<string, int?>();
            OnlineQuerySetupParamsChanges[] parameterChanges = UserQueryParamsGrid.Data as OnlineQuerySetupParamsChanges[];

            if (UserQueryParamsGrid.Data != null)
            {
                queryParams = new QueryParameters(); //int[] numbers = new int[5];

                QueryParameter[] qp = new QueryParameter[parameterChanges.Count()];
                for (int i = 0; i < parameterChanges.Count(); i++)
                {
                    qp[i] = new QueryParameter(parameterChanges[i].Name.ToString(), null);
                    ParameterDataTypeMap.Add(parameterChanges[i].Name.Value, parameterChanges[i].DataType == null ? (int?)null : parameterChanges[i].DataType.Value);
                }
                queryParams.Parameters = qp;
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

        protected virtual ResultStatus AddOrUpdate()
        {
            //Perform value validation
            OnlineQuerySetupMaint inputForExecute = new OnlineQuerySetupMaint();
            Page.GetInputData(inputForExecute);

            ResultStatus resultStatus = (this.Page as CamstarForm).ValidateInputData(inputForExecute);
            if (!resultStatus.IsSuccess)
                return resultStatus;

            OnlineQuerySetupMaintService service = Page.Service.GetService<Camstar.WCF.Services.OnlineQuerySetupMaintService>();

            service.BeginTransaction();

            Page.DataContract.SetValueByName(isNewDataMember, false);

            var pc = Page.PortalContext as MaintenanceBehaviorContext;

            if (NotInEditMode(pc))
            {
                service.New(); //add new cdo

                Page.DataContract.SetValueByName(isNewDataMember, true);
            }
            else
            {
                OnlineQuerySetupMaint input = new OnlineQuerySetupMaint()
                {
                    ObjectToChange = new NamedObjectRef()
                    {
                        Name = (Page.FindCamstarControl(txtName) as TextBox).OriginalData.ToString()
                    }
                };

                service.Load(input);

                Page.DataContract.SetValueByName(isNewDataMember, false);
            }
            service.ExecuteTransaction(inputForExecute);
            resultStatus = service.CommitTransaction();

            if (!resultStatus.IsSuccess)
            {
                Page.DataContract.SetValueByName(isNewDataMember, false);
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

        private bool NotInEditMode(MaintenanceBehaviorContext portalContext)
        {
            if (portalContext.State != MaintenanceBehaviorContext.MaintenanceState.Edit
                && !(bool)(Page.DataContract.GetValueByName(isNewDataMember) ?? false))
            {
                return true;
            }
            return false;
        }

        public virtual void RefreshInstanceList()
        {
            var service = new OnlineQuerySetupMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var data = new OnlineQuerySetupMaint();

            var request = new OnlineQuerySetupMaint_Request()
            {
                Info = new OnlineQuerySetupMaint_Info()
                {
                    ObjectToChange = new Info { RequestSelectionValues = true }
                }
            };

            OnlineQuerySetupMaint_Result result = null;
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
            var label = FrameworkManagerUtil.GetLabelCache().GetLabelByName("Lbl_UserQueryTest");

            ActionDispatcher dispatcher = Page.ActionDispatcher;
            dispatcher.DataContract.SetValueByName("UserQueryParamsDM", queryParams);
            dispatcher.DataContract.SetValueByName("UserQueryNameDM", queryName);
            dispatcher.DataContract.SetValueByName("label", label?.DefaultValue);
            Page.MergedContent.DynamicWebParts.SingleOrDefault(item => item.Name == "MDL_Specific").DataContract.SetValueByName("UserQueryParamsDM", queryParams);
            dispatcher.ExecuteAction(floatAction);
        }

        private const string isNewKey = "IsNew";

        private const string isNewDataMember = "IsNewDataMember";

        private const string txtName = "NameTxt";

        private bool shouldResetDirty = false;
    }

}
