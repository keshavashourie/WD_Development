using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace Camstar.WebPortal.Helpers
{
    // This class is used to generate information that is needed to interface with SRC. There are several 
    // classes in the portal that need the same information to interface with SRC.This class is therefore 
    // a shared pool of methods to serve portal pages that interface with SRC.
    public class SrcIntegrationHelper
    {
        static UserProfile CurrentUserProfile
        {
            get
            {
                return FrameworkManagerUtil.GetFrameworkSession().CurrentUserProfile;
            }
        }

        /// <summary>
        /// Get a bearer token that can be passed to SRC. The bearer token is used for labeling, and could be used for any other
        /// code that needs to know the current user information.
        /// </summary>
        public static string BearerToken
        {
            get
            {
                string token = "{}";
                var pass = new OpcenterRestAuth();
                try
                {

                    if (FrameworkManagerUtil.GetFrameworkSession() != null && CurrentUserProfile != null)
                    {
                        var currentPW = HttpContext.Current.Session[Constants.SessionConstants.UserPassword] as EncryptedField;

                        string username = CurrentUserProfile.Name;
                        pass = new OpcenterRestAuth();
                        pass.UserName = username;
                        pass.UtcOffset = CurrentUserProfile.UTCOffset;
                        pass.TimeZone = CurrentUserProfile.TimeZone;
                        if (CurrentUserProfile.SessionID != null)
                        {
                            pass.SessionId = new OpcenterSessionId();
                            pass.SessionId.Value = CurrentUserProfile.SessionID.Value;
                            pass.SessionId.IsEncrypted = CurrentUserProfile.SessionID.IsEncrypted;
                        }
                        else if (currentPW != null)
                        {
                            pass.Password = new OpcenterPassword();
                            pass.Password.Value = currentPW != null ? currentPW.Value : string.Empty;
                            pass.Password.IsEncrypted = true;
                        }
                    }
                    token = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(
                            JsonConvert.SerializeObject(pass)));
                }
                catch (Exception ex)
                {
                    token = ex.ToString();
                }
                return token;
            }
        }

        /// <summary>
        /// Check the external permissions that exist in modeling. Does the current user have access permission to modify SRC's data or not
        /// </summary>
        /// <returns>blank if the user has write permission to SRC. Otherwise return 'IPLSettingsReadOnly'</returns>
        public static string GetUserAccessPermission()
        {
            string permissionName = "IPL Configuration";
            Service serviceData = new Service();
            FrameworkSession fs = FrameworkManagerUtil.GetFrameworkSession();
            QueryService queryService = new QueryService(fs.CurrentUserProfile);

            string empname = CurrentUserProfile.Name;
            string application = "Workspace";
            string module = "SRC";

            QueryParameters queryParam = new QueryParameters();
            queryParam.Parameters = new QueryParameter[3];
            queryParam.Parameters[0] = new QueryParameter();
            queryParam.Parameters[0].Name = "EmployeeName";
            queryParam.Parameters[0].Value = empname;
            queryParam.Parameters[1] = new QueryParameter();
            queryParam.Parameters[1].Name = "Application";
            queryParam.Parameters[1].Value = application;
            queryParam.Parameters[2] = new QueryParameter();
            queryParam.Parameters[2].Name = "Module";
            queryParam.Parameters[2].Value = module;

            RecordSet permissions = new RecordSet();
            ResultStatus Result = queryService.Execute("PermissionInquiry_GetExternalPermissions", queryParam, new QueryOptions(), out permissions);

            if (Result.IsSuccess && permissions.TotalCount.ToString() != null)
            {
                DataTable resultDt = permissions.GetAsDataTable();

                if (resultDt != null && resultDt.Rows != null && resultDt.Rows.Count > 0)
                {
                    var exist = resultDt.AsEnumerable()
                                        .Where(x => x.Field<string>("ExternalPermissionName")
                                        .Equals(permissionName)).Count() > 0;

                    if (exist)
                    {
                        // We used to return the IPLSettingsReadOnly permission. But the name was changed
                        // to "IPL Configuration". We are not modifying SRC so we have to make this code
                        // work with the old IPLSettingsReadOnly permission setting.
                        // Read/Write access is actually the default for SRC. So if
                        // "IPL Configuration" is set as the external permission then return blank. This will 
                        // not pass any permission to SRC and therefore the user can edit data.
                        return string.Empty;
                    }
                }
            }

            // No match on the setting IPL Configuration role. Make it the user's access read only to make the system
            // work the way it used to.
            return "IPLSettingsReadOnly";
        }
    }

    internal class OpcenterRestAuth
    {
        public string UserName { get; set; }
        public OpcenterPassword Password { get; set; }
        public OpcenterSessionId SessionId { get; set; }
        public TimeSpan UtcOffset { get; set; }
        public string TimeZone { get; set; }
    }

    internal class OpcenterPassword
    {
        public bool IsEncrypted { get; set; }
        public string Value { get; set; }
    }

    internal class OpcenterSessionId
    {
        public bool IsEncrypted { get; set; }
        public string Value { get; set; }
    }
}