// © Siemens 2025 Siemens Product Lifecycle Management Software Inc.
using System;
using System.Web;
using System.Net;
using System.Text;
using System.Data;
using System.Net.Http;
using System.ServiceModel;
using System.Threading.Tasks;
using System.ServiceModel.Web;
using System.Runtime.Serialization;
using System.ServiceModel.Activation;

using Camstar.Util;
using Camstar.WCF.ObjectStack;
using Camstar.WebPortal.WCFUtilities;
using Camstar.WebPortal.FormsFramework.Utilities;

namespace WebClientPortal
{
    [DataContract]
    public class AIServerConfig
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string APIHost { get; set; }
        [DataMember]
        public string APIPort { get; set; }
        [DataMember]
        public string DBHost { get; set; }
        [DataMember]
        public string DBPort { get; set; }
        [DataMember]
        public string DBName { get; set; }
        [DataMember]
        public string DBSchema { get; set; }
        [DataMember]
        public string DBUser { get; set; }
        [DataMember]
        public string DBPassword { get; set; }

    }

    public class AIStartTrainingRequestContent
    {
        public string SessionId { get; set; }
        public string RequestId { get; set; }
        public AIServerConfig ServerConfig { get; set; }
    }

    [ServiceContract(Namespace = "")]
    public interface IAIServerService
    {
        #region original API for early adopters
        [OperationContract]
        ResultStatus GetConfiguration(out string host, out string port, out string token);

        [OperationContract]
        ResultStatus IsConfigurationSet(out bool configSet);
        #endregion original API for early adopters

        #region refactored API for authentication and configuration
        [OperationContract]
        ResultStatus IsInferenceConfigurationSet(out bool configSet);

        [OperationContract]
        ResultStatus IsTrainConfigurationSet(out bool configSet);

        [OperationContract]
        ResultStatus StartTraining(string configName, out string trainingStatus);

        [OperationContract]
        ResultStatus GetTrainingStatus(string configName, out string trainingStatus);
        #endregion refactored API for authentication and configuration
    }

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class AIServerService : IAIServerService
    {
        #region original API implementation for early adopters
        [WebInvoke]
        public virtual ResultStatus GetConfiguration(out string host, out string port, out string token)
        {
            host = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.AIServerHost] as string;
            port = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.AIServerPort] as string;
            token = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.AIServerToken] as string;
            return new ResultStatus("", true);
        }

        [WebGet]
        public virtual ResultStatus IsConfigurationSet(out bool configSet)
        {
            string host = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.AIServerHost] as string;
            string port = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.AIServerPort] as string;
            string token = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.AIServerToken] as string;
            configSet = !(string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(token));

            return new ResultStatus("", true);
        }
        #endregion original API implementation for early adopters

        #region refactored API implementation for authentication and configuration
        [WebGet]
        public virtual ResultStatus IsInferenceConfigurationSet(out bool configSet)
        {
            string apiHost = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.AIInferenceServerConfigName] as string;
            configSet = !(string.IsNullOrEmpty(apiHost));

            return new ResultStatus("", true);
        }

        [WebGet]
        public virtual ResultStatus IsTrainConfigurationSet(out bool configSet)
        {
            string trainHost = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.AITrainServerConfigName] as string;
            configSet = !(string.IsNullOrEmpty(trainHost));

            return new ResultStatus("", true);
        }

        [WebInvoke]
        public virtual ResultStatus StartTraining(string configName, out string trainingStatus)
        {
            ResultStatus status = new ResultStatus();
            trainingStatus = string.Empty;
            string configJson = string.Empty;
            string errorMessage = string.Empty;

            AIServerConfig config = GetTrainConfiguration(configName, out errorMessage);
            if (string.IsNullOrEmpty(errorMessage) && !string.IsNullOrEmpty(config.APIHost))
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(20);  // Set a 20-second timeout to avoid hanging when the Start Training button is clicked.
                UriBuilder uri = new UriBuilder($"https://{config.APIHost}:{config.APIPort}/mltrain/run");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri.ToString());

                AIStartTrainingRequestContent content = new AIStartTrainingRequestContent
                {
                    SessionId = GetSessionId(),
                    RequestId = Guid.NewGuid().ToString("D"),
                    ServerConfig = config
                };

                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                string contentJson = serializer.Serialize(content);
                string encryptedContent = CryptUtil.Encrypt(contentJson);
                request.Content = new StringContent(encryptedContent, Encoding.UTF8, "application/octet-stream");

                //JJR: return OK until have API server to connect to
                //trainingStatus = "Started";
                //status.IsSuccess = true;

                //JJR: enable when have API server to connect to
                HttpResponseMessage response = null;
                try
                {
                    Task<HttpResponseMessage> responseTask = client.SendAsync(request);
                    responseTask.Wait();
                    response = responseTask.Result;
                    string resultJson = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var resultObj = serializer.Deserialize<AIStartTrainingPipelineApiResponse>(resultJson);

                        if (resultObj.status.Equals("Success", StringComparison.OrdinalIgnoreCase))
                        {
                            trainingStatus = "Started";
                            status.Message = resultObj.message;
                            status = new ResultStatus("", true);
                        }
                        else
                        {
                            status.Message = resultObj.message;
                            status.IsSuccess = false;
                        }

                    }
                    else if (response.StatusCode == HttpStatusCode.Conflict) // 409
                    {
                        string conflictMessage = "Training is already running.";
                        try
                        {
                            var resultObj = serializer.Deserialize<AIStartTrainingPipelineApiResponse>(resultJson);
                            if (!string.IsNullOrWhiteSpace(resultObj?.message))
                                conflictMessage = resultObj.message;
                        }
                        catch { }
                        status.Message = conflictMessage;
                        status.IsSuccess = false;
                        status.ExceptionData = new ExceptionDataType
                        {
                            Description = conflictMessage,
                            ExceptionLevel = ExceptionLevel.Server
                        };
                    }
                    else
                    {
                        //TODO: localize?
                        string errMsg = $"Status Code:{response.StatusCode}, Content: {response.Content}, Reason Phrase: {response.ReasonPhrase}";
                        status.Message = errMsg;
                        status.IsSuccess = false;
                        status.ExceptionData = new ExceptionDataType
                        {
                            Description = errMsg,
                            ExceptionLevel = ExceptionLevel.Server
                        };
                    }
                }
                catch (Exception ex)
                {
                    status = new ResultStatus(ex.ToString(), false);
                    status.ExceptionData = new ExceptionDataType
                    {
                        Description = ex.ToString(),
                        ExceptionLevel = ExceptionLevel.Server
                    };
                }
            }
            else
            {
                status.Message = errorMessage;
                status.IsSuccess = false;
            }

            return status;
        }


        [WebInvoke]
        public virtual ResultStatus GetTrainingStatus(string configName, out string trainingStatus)
        {
            ResultStatus status = new ResultStatus();
            trainingStatus = string.Empty;
            string configJson = string.Empty;
            string errorMessage = string.Empty;

            AIServerConfig config = GetTrainConfiguration(configName, out errorMessage);
            if (string.IsNullOrEmpty(errorMessage) && !string.IsNullOrEmpty(config.APIHost))
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(20);  // Set a 20-second timeout to ensure the HTTP request doesn't delay page loading.
                UriBuilder uri = new UriBuilder($"https://{config.APIHost}:{config.APIPort}/mltrain/status");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri.ToString());

                AIStartTrainingRequestContent content = new AIStartTrainingRequestContent
                {
                    SessionId = GetSessionId(),
                    RequestId = Guid.NewGuid().ToString("D"),
                    ServerConfig = config
                };

                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                string contentJson = serializer.Serialize(content);
                string encryptedContent = CryptUtil.Encrypt(contentJson);
                request.Content = new StringContent(encryptedContent, Encoding.UTF8, "application/octet-stream");

                HttpResponseMessage response = null;
                try
                {
                    Task<HttpResponseMessage> responseTask = client.SendAsync(request);
                    responseTask.Wait();
                    response = responseTask.Result;
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    // For the /status endpoint:
                    // - A 200 OK response indicates the AI training server is idle and available to start a new training.
                    // - A 409 Conflict response indicates that AI training is already running.
                    if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Conflict)
                    {
                        trainingStatus = FormatTrainingStatus((int)response.StatusCode, responseContent);
                        status = new ResultStatus("", true);
                    }
                    else
                    {
                        //TODO: localize?
                        string errMsg = $"Status Code:{response.StatusCode}, Content: {responseContent}, Reason Phrase: {response.ReasonPhrase}";
                        status.Message = errMsg;
                        status.IsSuccess = false;
                    }
                }
                catch (Exception ex)
                {
                    status = new ResultStatus(ex.ToString(), false);
                }
            }
            else
            {
                status.Message = errorMessage;
                status.IsSuccess = false;
            }

            return status;
        }
        #endregion refactored API implementation for authentication and configuration

        #region helper methods
        public AIServerConfig GetInferenceConfiguration(out string errorMessage)
        {
            AIServerConfig config = null;
            string configName = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.AIInferenceServerConfigName] as string;
            if (string.IsNullOrEmpty(configName))
            {
                LabelCache labelCache = LabelCache.GetRuntimeCacheInstance();
                var label = labelCache.GetLabelByName("AIInferenceServerNotConfigured");
                errorMessage = label != null ? label.Value : "AI Inference Server not configured";
            }
            else
            {
                config = GetAIServerConfiguration(configName, out errorMessage);
            }

            return config;
        }

        public AIServerConfig GetTrainConfiguration(string configName, out string errorMessage)
        {
            AIServerConfig config = new AIServerConfig();

            if (string.IsNullOrEmpty(configName))
                configName = HttpContext.Current.Session[Camstar.WebPortal.Constants.SessionConstants.AITrainServerConfigName] as string;

            if (string.IsNullOrEmpty(configName))
            {
                LabelCache labelCache = LabelCache.GetRuntimeCacheInstance();
                errorMessage = labelCache.GetLabelByName("AITrainingServerNotConfigured").Value;
            }
            else
            {
                config = GetAIServerConfiguration(configName, out errorMessage);
            }

            return config;
        }

        public AIServerConfig GetAIServerConfiguration(string configName, out string errorMessage)
        {
            AIServerConfig config = new AIServerConfig();
            config.Name = configName;
            errorMessage = string.Empty;

            try
            {
                FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);

                var qUtil = new QueryUtil(session.CurrentUserProfile);
                QueryParameter[] queryParameters = new QueryParameter[1]
                {
                    new QueryParameter("ConfigName", configName)
                };
                string msg = string.Empty;
                System.Data.DataTable dt = qUtil.Execute("GetAIServerConfigDetails", queryParameters, new QueryOptions(), ref msg);
                if (dt != null && dt.Rows.Count == 1)
                {
                    DataRow dr = dt.Rows[0];
                    config.APIHost = dr["APIHost"].ToString();
                    config.APIPort = dr["APIPort"].ToString();
                    config.DBHost = dr["DBHost"].ToString();
                    config.DBPort = dr["DBPort"].ToString();
                    config.DBName = dr["DBName"].ToString();
                    config.DBSchema = dr["DBSchema"].ToString();
                    config.DBUser = dr["DBUser"].ToString();
                    config.DBPassword = string.IsNullOrEmpty(dr["DBPassword"]?.ToString()) ? string.Empty : CryptUtil.Decrypt(dr["DBPassword"].ToString());
                }
                else
                {
                    // handle edge case where query doesn't return data
                    LabelCache labelCache = LabelCache.GetRuntimeCacheInstance();
                    var label = labelCache.GetLabelByName("AIServerConfigurationNotFound");
                    errorMessage = label.Value.Replace("#ErrorMsg.Name", configName);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            return config;
        }

        public string GetSessionId()
        {
            FrameworkSession session = FrameworkManagerUtil.GetFrameworkSession(HttpContext.Current.Session);
            return CryptUtil.Decrypt(session.CurrentUserProfile.SessionID.Value);
        }

        private string FormatTrainingStatus(int httpCode, string rawJson)
        {
            if (string.IsNullOrEmpty(rawJson)) return string.Empty;

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var parsed = serializer.Deserialize<AITrainStatusRawResponse>(rawJson);

            string ToLocal(string utc)
            {
                if (string.IsNullOrEmpty(utc)) return "";
                DateTime dt = DateTime.Parse(utc, null, System.Globalization.DateTimeStyles.RoundtripKind);
                return dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            }

            if (httpCode == 409)
            {
                return $"Running";
            }
            else if (httpCode == 200)
            {
                var exec = parsed.last_training_execution;
                if (exec == null || string.IsNullOrEmpty(exec.status))
                {
                    return string.Empty; // nothing to show due to no previous execution result
                }
                else if (exec.status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
                {
                    return $"Completed at {ToLocal(exec.timestamp)}";
                }
                else if (exec.status.Equals("FAILED", StringComparison.OrdinalIgnoreCase))
                {
                    return $"Failed at {ToLocal(exec.timestamp)}. Error: {exec.exception}";
                }
            }

            return string.Empty; // fallback
        }
        #endregion helper methods


        #region classes

        public class AIStartTrainingPipelineApiResponse
        {
            public string status { get; set; }
            public string message { get; set; }
        }

        private class AITrainStatusRawResponse
        {
            public string status { get; set; }
            public string detail { get; set; }
            public string last_updated { get; set; }
            public LastTrainingExecution last_training_execution { get; set; }
        }

        private class LastTrainingExecution
        {
            public string status { get; set; }
            public string timestamp { get; set; }
            public string exception { get; set; }
        }

        #endregion classes
    }
}
