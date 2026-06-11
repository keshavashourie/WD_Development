using Microsoft.Owin;
using Owin;
using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using Microsoft.AspNet.SignalR;
using System.Web.Cors;
using Microsoft.Owin.Cors;
using System.Threading.Tasks;
using System.IO;
using System.Web.Hosting;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

[assembly: OwinStartup(typeof(WebClientPortal.Startup))]

namespace WebClientPortal
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public partial class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            var corsPolicy = new CorsPolicy
            {                
                AllowAnyMethod = true,
                AllowAnyHeader = true,
                SupportsCredentials = true
            };

           

            List<string> allowedOrigins = GetAllowedOrigins();

            foreach (var origin in allowedOrigins)
            {
                corsPolicy.Origins.Add(origin);
            }

            

            app.UseCors(new CorsOptions
            {
                PolicyProvider = new CorsPolicyProvider
                {
                    PolicyResolver = context => Task.FromResult(corsPolicy)
                }
            });

            app.Map("/signalr", map =>
            {
                map.Use<AuthenticationMiddleware>();

                map.RunSignalR(new HubConfiguration
                {
                    EnableDetailedErrors = true,
                    EnableJSONP = true
                });
            });
        }

        protected static List<string> GetAllowedOrigins()
        {
            try
            {
                string configFilePath = HostingEnvironment.MapPath("~/appsettings.json");
                if (string.IsNullOrEmpty(configFilePath) || !File.Exists(configFilePath))
                {
                    return new List<string>();
                }


                var json = File.ReadAllText(configFilePath);
                var jObject = JObject.Parse(json);


                
				var origins = jObject["CORSConfig"]?["CORSWhitelist"]?["Origin"]?.Select(o => o.ToString().Trim().ToLower().TrimEnd('/')).ToList();
                return origins ?? new List<string>();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

    }
}
