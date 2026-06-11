// Copyright Siemens 2023  
using System;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Generic;
using Camstar.WebPortal.Constants;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using Camstar.WebPortal.FormsFramework;
using Camstar.WebPortal.FormsFramework.Utilities;
using Camstar.WebPortal.FormsFramework.WebGridControls;
using CamstarPortal.WebControls;

using OM = Camstar.WCF.ObjectStack;
using CWC = Camstar.WebPortal.FormsFramework.WebControls;

/// <summary>
/// Summary description for UserProfileControl_Updated
/// </summary>
namespace Camstar.WebPortal.WebPortlets
{
    public class SystemInfo : MatrixWebPart
    {
        public SystemInfo()
        {
            this.Title = "User Profile";
        }

        protected JQDataGrid SystemInfoGrid
        {
            get { return Page.FindCamstarControl("SystemInfoGrid") as JQDataGrid; }
        }

        protected JQDataGrid ActiveWorkspacesGrid
        {
            get { return Page.FindCamstarControl("ActiveWorkspacesGrid") as JQDataGrid; }
        }

        protected JQDataGrid RolesGrid
        {
            get { return Page.FindCamstarControl("RolesGrid") as JQDataGrid; }
        }

        protected CWC.TextBox TitleTextBox
        {
            get { return Page.FindCamstarControl("TitleTextBox") as CWC.TextBox; }
        }

        protected CWC.TextBox OrganizationTextBox
        {
            get { return Page.FindCamstarControl("OrganizationTextBox") as CWC.TextBox; }
        }

        protected CWC.TextBox UserNameTextBox
        {
            get { return Page.FindCamstarControl("UserNameTextBox") as CWC.TextBox; }
        }

        protected CWC.TextBox FullNameTextBox
        {
            get { return Page.FindCamstarControl("FullNameTextBox") as CWC.TextBox; }
        }

        protected CWC.TextBox PrimaryLangTextBox
        {
            get { return Page.FindCamstarControl("PrimaryLangTextBox") as CWC.TextBox; }
        }

        protected CWC.TextBox SecondaryLangTextBox
        {
            get { return Page.FindCamstarControl("SecondaryLangTextBox") as CWC.TextBox; }
        }
        protected CWC.TextBox SessionLangTextBox
        {
            get { return Page.FindCamstarControl("SessionLangTextBox") as CWC.TextBox; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            OM.ResultStatus[] tasks = new OM.ResultStatus[3];

            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

            tasks[0] = GetSystemInfo(session);
            tasks[1] = GetActiveWorkspaces(session);
            tasks[2] = GetEmployeeDetails(session);
            
            foreach (OM.ResultStatus t in tasks)
            {
                if (t != null)
                {
                    if (!t.IsSuccess)
                        Page.DisplayMessage(t);
                }
            }
        }

        private OM.ResultStatus GetSystemInfo(FrameworkSession session)
        {
            OM.ResultStatus rs = null;

            if (session != null)
            {
                var service = new WCF.Services.SystemInfoInquiryService(session.CurrentUserProfile);
                var systemInfo = new OM.SystemInfoInquiry();

                var request = new WCF.Services.SystemInfoInquiry_Request()
                {
                    Info = new OM.SystemInfoInquiry_Info()
                    {
                        SystemInfoDetails = new OM.SystemInfoDetail_Info()
                        {
                            RequestSelectionValues = true
                        }
                    }
                };

                var result = new WCF.Services.SystemInfoInquiry_Result();

                rs = service.GetEnvironment(systemInfo, request, out result);

                if (rs.IsSuccess)
                {
                    List<OM.SystemInfoDetail> details = new List<OM.SystemInfoDetail>();

                    OM.Header[] headers = result.Environment.SystemInfoDetails.SelectionValues.Headers;
                    OM.Row[] rows = result.Environment.SystemInfoDetails.SelectionValues.Rows;

                    int srcIndex = Array.FindIndex(headers, h => h.Name.Equals("InfoSource"));
                    int nameIndex = Array.FindIndex(headers, h => h.Name.Equals("InfoName"));
                    int valIndex = Array.FindIndex(headers, h => h.Name.Equals("InfoValue"));
                    int licenseInUse = GetLicensedInUseCount();
                    int totalLicenses = -1;
                    foreach (OM.Row row in rows)
                    {
                        OM.SystemInfoDetail detail = new OM.SystemInfoDetail()
                        {
                            InfoSource = row.Values[srcIndex],
                            InfoName = row.Values[nameIndex],
                            InfoValue = row.Values[valIndex]
                        };
                        if (string.Compare(detail.InfoName.Value, "Licenses In Use", true) == 0)
                        {
                            detail.InfoValue = licenseInUse.ToString();
                        }
                        else if (string.Compare(detail.InfoName.Value, "License Total", true) == 0)
                        {
                            int.TryParse(detail.InfoValue.Value, out totalLicenses);
                        }
                        else if (detail.InfoName != null && detail.InfoValue != null && (string.Compare(detail.InfoName.Value, "MDB Version", true) == 0 || string.Compare(detail.InfoName.Value, "VERSION", true) == 0))
                        {
                            detail.InfoValue.Value = GetDisplayVersion(detail.InfoValue.Value);
                        }
                        details.Add(detail);
                    }
                    if (totalLicenses > 0)
                    {
                        OM.SystemInfoDetail find = details.Find(s => string.Compare(s.InfoName.Value, "Licenses Available", true) == 0);
                        if (find != null)
                            find.InfoValue.Value = (totalLicenses - licenseInUse).ToString();
                    }

                    UpdateSystemInfoGrid(details);
                }
            }

            return rs;
        }

        protected int GetLicensedInUseCount()
        {
            int nLicenseInUseCount = 0;
            var recordSet = new OM.RecordSet();
            string queryText = "SELECT COUNT(*) CNT FROM ActiveUserSession";
            var session = FrameworkManagerUtil.GetFrameworkSession();
            var service = new Camstar.WCF.Services.QueryService(session.CurrentUserProfile);
            var options = new OM.QueryOptions();
            var resultStatus = service.ExecuteAdHoc(queryText, options, out recordSet);

            if (resultStatus.IsSuccess)
            {
                var rows = recordSet.Rows;
                if (rows != null && !rows.First().Values[0].ToString().Equals(string.Empty))
                {
                    nLicenseInUseCount = int.Parse(rows.First().Values[0]);
                }
            }
            return nLicenseInUseCount;
        }

        private string GetDisplayVersion(string version)
        {
            if (version.StartsWith("10.4.2"))
                version = "2504.0002";
            else if (version.StartsWith("10.4.1"))
                version = "2504.0001";
            else if (version.StartsWith("10.4"))
                version = "2504";
            if (version.StartsWith("10.3.2"))
                version = "2410.0002";
            else if (version.StartsWith("10.3.1"))
                version = "2410.0001";
            else if (version.StartsWith("10.3"))
                version = "2410";
            else if (version.StartsWith("10.2.2"))
                version = "2404.0002";
            else if (version.StartsWith("10.2.1"))
                version = "2404.0001";
            else if (version.StartsWith("10.2"))
                version = "2404";
            if (version.StartsWith("10.1.2"))
                version = "2310.0002";
            else if (version.StartsWith("10.1.1"))
                version = "2310.0001";
            else if (version.StartsWith("10.1"))
                version = "2310";
            return version;
        }

        private OM.ResultStatus GetActiveWorkspaces(FrameworkSession session)
        {
            OM.ResultStatus rs = null;

            if (session != null)
            {
                var service = new WCF.Services.SystemInfoInquiryService(session.CurrentUserProfile);
                var systemInfo = new OM.SystemInfoInquiry();

                var request = new WCF.Services.SystemInfoInquiry_Request()
                {
                    Info = new OM.SystemInfoInquiry_Info()
                    {
                        ActiveWorkspaces = new OM.WorkspaceDetail_Info()
                        {
                            RequestSelectionValues = true
                        }
                    }
                };

                var result = new WCF.Services.SystemInfoInquiry_Result();

                rs = service.GetEnvironment(systemInfo, request, out result);

                if (rs.IsSuccess)
                {
                    List<OM.WorkspaceDetail> details = new List<OM.WorkspaceDetail>();

                    OM.Header[] headers = result.Environment.ActiveWorkspaces.SelectionValues.Headers;
                    OM.Row[] rows = result.Environment.ActiveWorkspaces.SelectionValues.Rows;

                    int codeIndex = Array.FindIndex(headers, h => h.Name.Equals("WorkspaceCode"));
                    int desIndex = Array.FindIndex(headers, h => h.Name.Equals("WorkspaceDescription"));

                    foreach (OM.Row row in rows)
                    {
                        details.Add(new OM.WorkspaceDetail()
                        {
                            Code = row.Values[codeIndex],
                            Description = row.Values[desIndex],
                        });
                    }

                   UpdateActiveWorkspacesGrid(details);
                }
            }

            return rs;
        }

        protected OM.ResultStatus GetEmployeeDetails(FrameworkSession session)
        {
            OM.EmployeeChanges employee = Context.Session[SessionConstants.Employee] as OM.EmployeeChanges;
            OM.ResultStatus status = null;
            if (employee == null)
            {
                WCF.Services.EmployeeMaintService maintService = new WCF.Services.EmployeeMaintService(session.CurrentUserProfile);
                OM.EmployeeMaint data = new OM.EmployeeMaint();
                data.ObjectToChange = new OM.NamedObjectRef(FrameworkManagerUtil.GetFrameworkSession(Page.Session).CurrentUserProfile.Name);
                WCF.Services.EmployeeMaint_Request request = new WCF.Services.EmployeeMaint_Request();
                request.Info = new OM.EmployeeMaint_Info();
                request.Info.ObjectChanges = new OM.EmployeeChanges_Info();
                request.Info.ObjectChanges.Name = new OM.Info(true);
                request.Info.ObjectChanges.Description = new OM.Info(true);
                request.Info.ObjectChanges.PrimaryOrganization = new OM.Info(true);
                request.Info.ObjectChanges.Roles = new OM.EmployeeRoleChanges_Info();
                request.Info.ObjectChanges.Roles.RequestValue = true;

                WCF.Services.EmployeeMaint_Result result = null;
                status = maintService.Load(data, request, out result);
                if (status.IsSuccess && result.Value != null)
                {
                    employee = result.Value.ObjectChanges as OM.EmployeeChanges;
                    Context.Session[SessionConstants.Employee] = employee;
                }
            }

            if (employee != null)
            {
                EmployeeProfile profile = new EmployeeProfile();
                List<EmployeeDetail> employeeDetails = new List<EmployeeDetail>();

                Camstar.WCF.ObjectStack.EmployeeRoleChanges[] roles = employee.Roles;
                profile.Title = employee.Description == null ? string.Empty : employee.Description.ToString();
                profile.PrimaryOrg = employee.PrimaryOrganization == null ? string.Empty : employee.PrimaryOrganization.ToString();

                for (int x = 0; x < roles.Length; x++)
                {
                    employeeDetails.Add(new EmployeeDetail(Convert.ToBoolean(roles[x].PropagateToChildOrgs.Value) ? "Y" : "N",
                                                            roles[x].Organization == null ? string.Empty : roles[x].Organization.Name.ToString(),
                                                            roles[x].Role == null ? string.Empty : roles[x].Role.Name.ToString()));
                }

                //ensure that at least 8 rows exist in grid
                int diff = 8 - roles.Length;

                for (int x = 0; x < diff; x++)
                {
                    employeeDetails.Add(new EmployeeDetail("&nbsp;", "&nbsp;", "&nbsp;"));
                }

                profile.Roles = employeeDetails;
                profile.Username = Context.User.Identity.Name;
                profile.FullName = Context.Session[SessionConstants.FullName] == null ? string.Empty : Context.Session[SessionConstants.FullName].ToString();
                profile.PrimaryLang = Context.Session[SessionConstants.PrimaryLanguage] == null ? string.Empty : Context.Session[SessionConstants.PrimaryLanguage].ToString();
                profile.SecondaryLang = Context.Session[SessionConstants.SecondaryLanguage] == null ? string.Empty : Context.Session[SessionConstants.SecondaryLanguage].ToString();
                profile.SessionLang = !string.IsNullOrEmpty(session.CurrentUserProfile.Dictionary) ? session.CurrentUserProfile.Dictionary : profile.PrimaryLang;

                UpdateEmployeeProfile(profile);
            }

            return status;
        }

        private void UpdateSystemInfoGrid(List<OM.SystemInfoDetail> details)
        {
            if (details != null)
                SystemInfoGrid.Data = details.ToArray();
        }

        private void UpdateActiveWorkspacesGrid(List<OM.WorkspaceDetail> details)
        {
            if (details != null)
            {
                ActiveWorkspacesGrid.Data = details.ToArray();
            }
        }

        private void UpdateEmployeeProfile(EmployeeProfile profile)
        {
            if (profile != null)
            {
                UserNameTextBox.TextControl.Text = profile.Username;
                FullNameTextBox.TextControl.Text = profile.FullName;
                TitleTextBox.TextControl.Text = profile.Title;
                PrimaryLangTextBox.TextControl.Text = profile.PrimaryLang;
                SecondaryLangTextBox.TextControl.Text = profile.SecondaryLang;
                SessionLangTextBox.TextControl.Text = profile.SessionLang;
                OrganizationTextBox.TextControl.Text = profile.PrimaryOrg;

                RolesGrid.Data = profile.Roles.ToArray();
            }
        }

        protected class EmployeeProfile
        {
            public string Username { get; set; }
            public string FullName { get; set; }
            public string Title { get; set; }
            public string PrimaryLang { get; set; }
            public string SecondaryLang { get; set; }
            public string SessionLang { get; set; }
            public string PrimaryOrg { get; set; }
            public List<EmployeeDetail> Roles { get; set; }
        }
    }
}
