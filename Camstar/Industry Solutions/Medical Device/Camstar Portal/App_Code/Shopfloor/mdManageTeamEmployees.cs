// © 2018 Siemens Product Lifecycle Management Software Inc.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using Camstar.WebPortal.FormsFramework;

namespace Camstar.WebPortal.WebPortlets.Shopfloor
{
    public class ManageTeamEmployees : MatrixWebPart
    {
        protected JQDataGrid TeamEmployees { get { return Page.FindCamstarControl("TeamEmployees_Employees") as JQDataGrid; } }

        protected JQDataGrid AvailableEmployees { get { return Page.FindCamstarControl("TeamEmployees_AvailableEmployees") as JQDataGrid; } }

        protected CWC.NamedObject Team
        {
            get { return Page.FindCamstarControl("AddTeamEmployees_Team") as CWC.NamedObject; }
        } // AddTeamEmployees_Team

        protected override void OnLoad(EventArgs e)
        {
            AvailableEmployees.GridContext.PostBackOnSelect = true;
            TeamEmployees.GridContext.PostBackOnSelect = true;
            base.OnLoad(e);
        }

        public override void GetInputData(OM.Service serviceData)
        {
            base.GetInputData(serviceData);
            if (serviceData is Camstar.WCF.ObjectStack.mdRemoveTeamMembers)
            {
                //(serviceData as Camstar.WCF.ObjectStack.mdRemoveTeamMembers).Employees = TeamEmployees.GridContext.GetSelectedItems(true) as OM.NamedObjectRef[];
                OM.NamedObjectRef[] teamEmployeesAsNamedObjectRef = new OM.NamedObjectRef[TeamEmployees.GridContext.GetSelectedCount()];
                var teamEmployees = TeamEmployees.GridContext.GetSelectedItems(false);
                for (int i = 0; i < teamEmployeesAsNamedObjectRef.Length; i++)
                    teamEmployeesAsNamedObjectRef[i] = teamEmployees[i] as OM.NamedObjectRef;
                (serviceData as Camstar.WCF.ObjectStack.mdRemoveTeamMembers).Employees = teamEmployeesAsNamedObjectRef;
            }
            else 
                if(serviceData is Camstar.WCF.ObjectStack.mdAddTeamMembers)
                {
                    OM.NamedObjectRef[] selectedAvailableEmployeesAsNamedObjectRef = new OM.NamedObjectRef[AvailableEmployees.GridContext.GetSelectedCount()];
                    var selectedAvailableEmployees = AvailableEmployees.GridContext.GetSelectedItems(false);
                    for (int i = 0; i < selectedAvailableEmployeesAsNamedObjectRef.Length; i++)
                        selectedAvailableEmployeesAsNamedObjectRef[i] = new OM.NamedObjectRef((selectedAvailableEmployees[i] as System.Data.DataRow).ItemArray[5] as String);
                    (serviceData as Camstar.WCF.ObjectStack.mdAddTeamMembers).Employees = selectedAvailableEmployeesAsNamedObjectRef;
                }
            
        }

        public override void PostExecute(OM.ResultStatus status, OM.Service serviceData)
        {

             base.PostExecute(status, serviceData);

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            var service = new Camstar.WCF.Services.mdAddTeamMembersService(session.CurrentUserProfile);

            var serviceDataCustom = new OM.mdAddTeamMembers();
            serviceDataCustom.Team = new OM.NamedObjectRef(Team.Data.ToString());

            var request = new Camstar.WCF.Services.mdAddTeamMembers_Request();
            var result = new Camstar.WCF.Services.mdAddTeamMembers_Result();
            var resultStatus = new OM.ResultStatus();

            request.Info = new OM.mdAddTeamMembers_Info
            {
                mdTeamEmployees = new OM.Info
                {
                    RequestValue = true
                },
                Employees = new OM.Info
                {
                    RequestSelectionValues = true
                }
            };

            resultStatus = service.GetEnvironment(serviceDataCustom, request, out result);

            if (resultStatus != null && resultStatus.IsSuccess)
            {
                AvailableEmployees.ClearSelectionValues();
                AvailableEmployees.ClearData();
                TeamEmployees.ClearData();

                TeamEmployees.GridContext.GetSelectedItems(true);
                TeamEmployees.Data = result.Value.mdTeamEmployees;

                AvailableEmployees.GridContext.GetSelectedItems(true);
                AvailableEmployees.Data = result.Environment.Employees.SelectionValues;
                AvailableEmployees.SetSelectionValues(result.Environment.Employees.SelectionValues);
                
        
           
            }
            else
                DisplayMessage(resultStatus);

            
        }

    }
}