// © 2017 Siemens Product Lifecycle Management Software Inc.
using System;
using System.Linq;
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
    public class CIOUserQuery : UserQuery
    {
        protected override void OnLoad(EventArgs e)
        {
            (Page.PortalContext as MaintenanceBehaviorContext).CDOTypeName = "UserQuery";
            base.OnLoad(e);
        }

        protected override ResultStatus AddOrUpdate()
        {
            //Perform value validation
            CIOUserQueryMaint inputForExecute = new CIOUserQueryMaint();
            Page.GetInputData(inputForExecute);

            ResultStatus resultStatus = (this.Page as CamstarForm).ValidateInputData(inputForExecute);
            if (!resultStatus.IsSuccess)
                return resultStatus;

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(Page.Session);
            CIOUserQueryMaintService service = Page.Service.GetService<Camstar.WCF.Services.CIOUserQueryMaintService>();
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
                CIOUserQueryMaint input = new CIOUserQueryMaint() { ObjectToChange = new NamedObjectRef() { Name = (Page.FindCamstarControl("NameTxt") as TextBox).Data.ToString() } };
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

        public override void RefreshInstanceList()
        {
            var service = new CIOUserQueryMaintService(FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile);
            var data = new CIOUserQueryMaint();
            var request = new CIOUserQueryMaint_Request()
            {
                Info = new CIOUserQueryMaint_Info()
                {
                    ObjectToChange = new Info { RequestSelectionValues = true }
                }
            };

            CIOUserQueryMaint_Result result = null;
            ResultStatus rs = service.GetEnvironment(data, request, out result);
            var pc = Page.PortalContext as MaintenanceBehaviorContext;
            if (rs.IsSuccess)
            {
                var result1 = result.Environment.ObjectToChange.SelectionValues;
                InstanceGrid.Data = null;
                InstanceGrid.Data = result1;
            }
        }
    }
}
