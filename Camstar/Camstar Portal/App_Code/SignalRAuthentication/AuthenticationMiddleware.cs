using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Camstar.WebPortal.WCFUtilities.Authentication;
using System.Security.Claims;
using System.Net;

public class AuthenticationMiddleware : OwinMiddleware
{
    public AuthenticationMiddleware(OwinMiddleware next) : base(next) { }

    public override async Task Invoke(IOwinContext context)
    {

        if (context.Request.Path.Value.EndsWith("/hubs"))
        {
            await Next.Invoke(context);
            return;
        }

        if (!context.Request.Path.Value.EndsWith("/hubs"))
        {

            string authHeader = context.Request.Headers.Get("Authentication");
            string clientIdentityHeader = context.Request.Headers.Get("ClientIdentity");
            ClaimsIdentity identity;
            if (!string.IsNullOrEmpty(authHeader))
            {
                string authToken = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? authHeader.Substring("Bearer ".Length) : authHeader;

                identity = IsAuthorised(authToken);

                if (identity != null)
                {
                    context.Authentication.User = new ClaimsPrincipal(identity);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    await context.Response.WriteAsync("Forbidden: Invalid token");
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(clientIdentityHeader))
            {
                
                identity = IsAuthorised(clientIdentityHeader);

                if (identity != null)
                {
                    context.Authentication.User = new ClaimsPrincipal(identity);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    await context.Response.WriteAsync("Forbidden: Invalid token");
                    return;
                }
  
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }
        }

        await Next.Invoke(context);
    }

    public ClaimsIdentity IsAuthorised(string AuthToken)
    {
        try
        {
            if (IsBase64Encoded(AuthToken))
            {
                var decodeToken = Base64Decode(AuthToken);
                AuthenticationTokenJSONAttributes JSONTokenCredentials = JsonConvert.DeserializeObject<AuthenticationTokenJSONAttributes>(decodeToken);
                AuthenticationServiceClient client = new AuthenticationServiceClient();
                if (JSONTokenCredentials.sessionID != null && !string.IsNullOrEmpty(JSONTokenCredentials.sessionID.value))
                {
                    if (client.ValidateSession(JSONTokenCredentials.sessionID.value).IsSuccess)
                    {
                        var claimsidentity = new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.Name, JSONTokenCredentials.Username),
                            new Claim(ClaimTypes.Role, "authorized_user")
                        }, "CamstarTokenBasedAuthentication");

                        return claimsidentity;
                    }
                }
                else if (!string.IsNullOrEmpty(JSONTokenCredentials.AuthenticationCode.ToString()))
                {

                    long getUnixTimeSeconds = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();

                    bool isAuthenticationCodeNotExpired = (getUnixTimeSeconds - JSONTokenCredentials.AuthenticationCode) <= 300;

                    if (isAuthenticationCodeNotExpired)
                    {
                        var claimsidentity = new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.Name, "DesignerClient"),
                            new Claim(ClaimTypes.Role, "authorized_user")
                        }, "CamstarTokenBasedAuthentication");

                        return claimsidentity;
                    }
                }

            }
        }
        catch(Exception)
        {
            return null;
        }

        return null;
    }



    public bool IsBase64Encoded(string str)
    {
        try
        {

            byte[] data = Convert.FromBase64String(str);
            return (str.Replace(" ", "").Length % 4 == 0);
        }
        catch
        {
            return false;
        }
    }

    public static string Base64Decode(string base64)
    {
        var base64Bytes = System.Convert.FromBase64String(base64);
        return System.Text.Encoding.UTF8.GetString(base64Bytes);
    }

    public class sessionID
    {
        public string value { get; set; }
        public bool isEncrypted { get; set; }
    }

    public class AuthenticationTokenJSONAttributes
    {
        public string Username { get; set; }

        public sessionID sessionID { get; set; }

        public string Utcoffset { get; set; }

        public long AuthenticationCode { get; set; }
    }
}
