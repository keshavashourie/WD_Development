/* Copyright 2023 Siemens */
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
    public class SS_OnlineQuerySetupMaint : MatrixWebPart
    {
        private JQDataGrid UserQueryParamsGrid { get { return Page.FindCamstarControl("UserQueryParams") as JQDataGrid; } }
        protected JQDataGrid InstanceGrid { get { return Page.FindCamstarControl("InstanceGrid") as JQDataGrid; } }
        protected TextBox QueryTextTxt { get { return Page.FindCamstarControl("QueryText") as TextBox; } }

        private Button SaveAndTest
        {
            get { return Page.FindCamstarControl("SaveAndTest") as Button; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SaveAndTest.Click += SaveAndTest_Click;
        }

        void SaveAndTest_Click(object sender, EventArgs e)
        {
            Page.DataContract.SetValueByName("QueryTxt", QueryTextTxt.Data);
            OpenTestUserQueryPage();
        }

        public void OpenTestUserQueryPage()
        {
            QueryParameters queryParams = null;
            Dictionary<string, int?> ParameterDataTypeMap = new Dictionary<string, int?>();
            UserQueryParameterChanges[] parameterChanges = UserQueryParamsGrid.Data as UserQueryParameterChanges[];

            if (UserQueryParamsGrid.Data != null)
            {
                queryParams = new QueryParameters(); //int[] numbers = new int[5];

                QueryParameter[] qp = new QueryParameter[(UserQueryParamsGrid.Data as OnlineQuerySetupParamsChanges[]).Count()];
                for (int i = 0; i < (UserQueryParamsGrid.Data as UserQueryParameterChanges[]).Count(); i++)
                {
					qp[i] = new QueryParameter((UserQueryParamsGrid.Data as OnlineQuerySetupParamsChanges[])[i].Name.ToString(), null);
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
                OpenTestUserQueryPage((Page.FindCamstarControl("NameTxt") as TextBox).Data.ToString(), queryParams);
            }
            else
            {
                DisplayMessage(res);
                return;
            }

        }

        private ResultStatus AddOrUpdate()
        {
            if (Page.PrimaryServiceType.Equals("GUIQueryMaint"))
            {
                //Perform value validation
                GUIQueryMaint inputForExecute = new GUIQueryMaint();
                Page.GetInputData(inputForExecute);

                ResultStatus resultStatus = Service.Form.ValidateInputData(inputForExecute);
                if (!resultStatus.IsSuccess)
                    return resultStatus;

                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(Page.Session);
                GUIQueryMaintService service = Page.Service.GetService<Camstar.WCF.Services.GUIQueryMaintService>();
                service.BeginTransaction();
                var pc = Page.PortalContext as MaintenanceBehaviorContext;
                Page.DataContract.SetValueByName("IsNewDM", false);
                if (pc.State != MaintenanceBehaviorContext.MaintenanceState.Edit && !(Page.DataContract.GetValueByName("IsNewDM") == null ? false : (bool)Page.DataContract.GetValueByName("IsNewDM")))
                {
                    service.New(); //add new cdo
                    Page.DataContract.SetValueByName("IsNewDM", true);
                }
                else
                {
                    GUIQueryMaint input = new GUIQueryMaint() { ObjectToChange = new NamedObjectRef() { Name = (Page.FindCamstarControl("NameTxt") as TextBox).Data.ToString() } };
                    service.Load(input);
                    Page.DataContract.SetValueByName("IsNewDM", false);
                }

                service.ExecuteTransaction(inputForExecute);
                resultStatus = service.CommitTransaction();

                if (!resultStatus.IsSuccess)
                {
                    Page.DataContract.SetValueByName("IsNewDM", false);
                }
                else
                {
                    if (pc.Current == null)
                        pc.Current = new NamedObjectRef()
                        {
                            Name = (Page.FindCamstarControl("NameTxt") as TextBox).Data.ToString()
                        };
                    Page.LoadModelingValues(true);
                }
                return resultStatus;
            }
            else if (Page.PrimaryServiceType.Equals("SPCQueryMaint"))
            {
                //Perform value validation
                SPCQueryMaint inputForExecute = new SPCQueryMaint();
                Page.GetInputData(inputForExecute);

                ResultStatus resultStatus = Service.Form.ValidateInputData(inputForExecute);
                if (!resultStatus.IsSuccess)
                    return resultStatus;

                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(Page.Session);
                SPCQueryMaintService service = Page.Service.GetService<Camstar.WCF.Services.SPCQueryMaintService>();
                service.BeginTransaction();
                var pc = Page.PortalContext as MaintenanceBehaviorContext;
                Page.DataContract.SetValueByName("IsNewDM", false);
                if (pc.State != MaintenanceBehaviorContext.MaintenanceState.Edit && !(Page.DataContract.GetValueByName("IsNewDM") == null ? false : (bool)Page.DataContract.GetValueByName("IsNewDM")))
                {
                    service.New(); //add new cdo
                    Page.DataContract.SetValueByName("IsNewDM", true);
                }
                else
                {
                    SPCQueryMaint input = new SPCQueryMaint() { ObjectToChange = new NamedObjectRef() { Name = (Page.FindCamstarControl("NameTxt") as TextBox).Data.ToString() } };
                    service.Load(input);
                    Page.DataContract.SetValueByName("IsNewDM", false);
                }

                service.ExecuteTransaction(inputForExecute);
                resultStatus = service.CommitTransaction();

                if (!resultStatus.IsSuccess)
                {
                    Page.DataContract.SetValueByName("IsNewDM", false);
                }
                else
                {
                    if (pc.Current == null)
                        pc.Current = new NamedObjectRef()
                        {
                            Name = (Page.FindCamstarControl("NameTxt") as TextBox).Data.ToString()
                        };
                    Page.LoadModelingValues(true);
                }
                return resultStatus;
            }
			else if (Page.PrimaryServiceType.Equals("ss_PMReqQueryMaint"))
			{
				//Perform value validation
				ss_PMReqQueryMaint inputForExecute = new ss_PMReqQueryMaint();
				Page.GetInputData(inputForExecute);

				ResultStatus resultStatus = Service.Form.ValidateInputData(inputForExecute);
				if (!resultStatus.IsSuccess)
					return resultStatus;

				FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(Page.Session);
				ss_PMReqQueryMaintService service = Page.Service.GetService<Camstar.WCF.Services.ss_PMReqQueryMaintService>();
				service.BeginTransaction();
				var pc = Page.PortalContext as MaintenanceBehaviorContext;
				Page.DataContract.SetValueByName("IsNewDM", false);
				if (pc.State != MaintenanceBehaviorContext.MaintenanceState.Edit && !(Page.DataContract.GetValueByName("IsNewDM") == null ? false : (bool)Page.DataContract.GetValueByName("IsNewDM")))
				{
					service.New(); //add new cdo
					Page.DataContract.SetValueByName("IsNewDM", true);
				}
				else
				{
					ss_PMReqQueryMaint input = new ss_PMReqQueryMaint() { ObjectToChange = new NamedObjectRef() { Name = (Page.FindCamstarControl("NameTxt") as TextBox).Data.ToString() } };
					service.Load(input);
					Page.DataContract.SetValueByName("IsNewDM", false);
				}

				service.ExecuteTransaction(inputForExecute);
				resultStatus = service.CommitTransaction();

				if (!resultStatus.IsSuccess)
				{
					Page.DataContract.SetValueByName("IsNewDM", false);
				}
				else
				{
					if (pc.Current == null)
						pc.Current = new NamedObjectRef()
						{
							Name = (Page.FindCamstarControl("NameTxt") as TextBox).Data.ToString()
						};
					Page.LoadModelingValues(true);
				}
				return resultStatus;
			}
			else if (Page.PrimaryServiceType.Equals("ss_EqpConstraintQueryMaint"))
			{
				//Perform value validation
				ss_EqpConstraintQueryMaint inputForExecute = new ss_EqpConstraintQueryMaint();
				Page.GetInputData(inputForExecute);

				ResultStatus resultStatus = Service.Form.ValidateInputData(inputForExecute);
				if (!resultStatus.IsSuccess)
					return resultStatus;

				FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(Page.Session);
				ss_EqpConstraintQueryMaintService service = Page.Service.GetService<Camstar.WCF.Services.ss_EqpConstraintQueryMaintService>();
				service.BeginTransaction();
				var pc = Page.PortalContext as MaintenanceBehaviorContext;
				Page.DataContract.SetValueByName("IsNewDM", false);
				if (pc.State != MaintenanceBehaviorContext.MaintenanceState.Edit && !(Page.DataContract.GetValueByName("IsNewDM") == null ? false : (bool)Page.DataContract.GetValueByName("IsNewDM")))
				{
					service.New(); //add new cdo
					Page.DataContract.SetValueByName("IsNewDM", true);
				}
				else
				{
					ss_EqpConstraintQueryMaint input = new ss_EqpConstraintQueryMaint() { ObjectToChange = new NamedObjectRef() { Name = (Page.FindCamstarControl("NameTxt") as TextBox).Data.ToString() } };
					service.Load(input);
					Page.DataContract.SetValueByName("IsNewDM", false);
				}

				service.ExecuteTransaction(inputForExecute);
				resultStatus = service.CommitTransaction();

				if (!resultStatus.IsSuccess)
				{
					Page.DataContract.SetValueByName("IsNewDM", false);
				}
				else
				{
					if (pc.Current == null)
						pc.Current = new NamedObjectRef()
						{
							Name = (Page.FindCamstarControl("NameTxt") as TextBox).Data.ToString()
						};
					Page.LoadModelingValues(true);
				}
				return resultStatus;
			}
			else
            {
                //Perform value validation
                OnlineQuerySetupMaint inputForExecute = new OnlineQuerySetupMaint();
                Page.GetInputData(inputForExecute);

                ResultStatus resultStatus = Service.Form.ValidateInputData(inputForExecute);
                if (!resultStatus.IsSuccess)
                    return resultStatus;

                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(Page.Session);
                OnlineQuerySetupMaintService service = Page.Service.GetService<Camstar.WCF.Services.OnlineQuerySetupMaintService>();
                service.BeginTransaction();
                var pc = Page.PortalContext as MaintenanceBehaviorContext;
                Page.DataContract.SetValueByName("IsNewDM", false);
                if (pc.State != MaintenanceBehaviorContext.MaintenanceState.Edit && !(Page.DataContract.GetValueByName("IsNewDM") == null ? false : (bool)Page.DataContract.GetValueByName("IsNewDM")))
                {
                    service.New(); //add new cdo
                    Page.DataContract.SetValueByName("IsNewDM", true);
                }
                else
                {
                    OnlineQuerySetupMaint input = new OnlineQuerySetupMaint() { ObjectToChange = new NamedObjectRef() { Name = (Page.FindCamstarControl("NameTxt") as TextBox).Data.ToString() } };
                    service.Load(input);
                    Page.DataContract.SetValueByName("IsNewDM", false);
                }

                service.ExecuteTransaction(inputForExecute);
                resultStatus = service.CommitTransaction();

                if (!resultStatus.IsSuccess)
                {
                    Page.DataContract.SetValueByName("IsNewDM", false);
                }
                else
                {
                    if (pc.Current == null)
                        pc.Current = new NamedObjectRef()
                        {
                            Name = (Page.FindCamstarControl("NameTxt") as TextBox).Data.ToString()
                        };
                    Page.LoadModelingValues(true);
                }
                return resultStatus;
            }
        }

        public void RefreshInstanceList()
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

        private void OpenTestUserQueryPage(string queryName, QueryParameters queryParams)
        {
            FloatPageOpenAction floatAction = new FloatPageOpenAction();
            floatAction.FrameLocation = new UIFloatingPageLocation();
			floatAction.PageName = "SS_OnlineQuerySetupTestPopupVP";
            floatAction.FrameLocation.Width = 700;
            floatAction.FrameLocation.Height = 500;
            floatAction.EndResponse = false;

            ActionDispatcher dispatcher = Page.ActionDispatcher;
            dispatcher.DataContract.SetValueByName("UserQueryParamsDM", queryParams);
            dispatcher.DataContract.SetValueByName("UserQueryNameDM", queryName);
			dispatcher.DataContract.SetValueByName("OnlineQuerySetupParamsDM", UserQueryParamsGrid.Data);
            dispatcher.ExecuteAction(floatAction);
        }

        private const string isNewKey = "IsNew";

    }
}




