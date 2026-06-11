using Camstar.Util;
using Camstar.WCF.ObjectStack;
using OM = Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using Camstar.WebPortal.FormsFramework.Utilities;
using Newtonsoft.Json;
using System;
using System.Data;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using Camstar.WebPortal.WCFUtilities.Authentication;
using System.Net;
using System.Net.Http;

namespace WebClientPortal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class SignalRAuthenticationTokenService
    {
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        public string GenerateAuthenticationToken()
        {
            var session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            string UserName = session.CurrentUserProfile.Name;
            string SessionID = CryptUtil.Decrypt(session.CurrentUserProfile.SessionID.Value.ToString());
            string Utcoffset = session.CurrentUserProfile.UTCOffset.ToString();
            string Response = string.Empty;
            bool isEncrypted = true;

           
                if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(SessionID))
                {
                    string JSON = "{\"username\": \"" + UserName + "\",\"SessionID\": {\"value\": \"" + SessionID + "\",\"isEncrypted\": \"" + isEncrypted + "\"},\"utcoffset\":\"" + Utcoffset + "\"}";
                    AuthenticationTokenJSONAttributes AuthenticationToken = JsonConvert.DeserializeObject<AuthenticationTokenJSONAttributes>(JSON);
                    Response = ToBase64(AuthenticationToken);
                }

            return Response;
        }

     
        public string ToBase64(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);

            byte[] bytes = Encoding.Default.GetBytes(json);

            return Convert.ToBase64String(bytes);
        }

        public class SessionID
        {
            public string value { get; set; }
            public bool isEncrypted { get; set; }
        }

        public class AuthenticationTokenJSONAttributes
        {
            public string Username { get; set; }

            public SessionID SessionID { get; set; }

            public string Utcoffset { get; set; }
        }
    }
}
