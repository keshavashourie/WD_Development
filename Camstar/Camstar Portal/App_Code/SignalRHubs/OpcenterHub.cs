using System;
using Microsoft.AspNet.SignalR;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using System.Web.Hosting;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "authorized_user")]
public class OpcenterHub : Hub
{
    // Keep track of connected clients and thier subscribed groups
    private static readonly ConcurrentDictionary<string, List<string>> ClientGroups = new ConcurrentDictionary<string, List<string>>();

    public async Task TransactionNotifier(string groupName, string transactiontype, string identifier, string transactionMessage, bool isFailureMessage, DateTime transactionMessageDatetime)
    {
        try
        {
            if (IsGroupExist(groupName) && !string.IsNullOrEmpty(identifier) && !string.IsNullOrEmpty(transactionMessage))
            {
                DateTime transactionMessageDateTimeUTC = DateTime.SpecifyKind(transactionMessageDatetime, DateTimeKind.Utc);
                await Clients.Group(groupName).ReceiveMessage(identifier, transactionMessage, isFailureMessage, transactionMessageDateTimeUTC);
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task BroadcastResourceSetupTxn(string groupName, string jsonParams)
    {
        try
        {
            List<List<string>> keyValuePairs = JsonConvert.DeserializeObject<List<List<string>>>(jsonParams);

            string resource = null;
            string statusCodeName = null;
            string activeMfgOrder = null;
            string parentResource = null;
            string reasonCodeName = null;
            string transactionType = "ResourceSetup";
            double timeAtStatus = 0;
            int resourceAvailability = 0;


            foreach (var pair in keyValuePairs)
            {
                if (pair.Count == 2)
                {
                    var key = pair[0];
                    var value = pair[1];

                    switch (key)
                    {
                        case "Resource":
                            resource = value;
                            break;
                        case "ParentResource":
                            parentResource = value;
                            break;
                        case "ReasonCodeName":
                            reasonCodeName = value;
                            break;
                        case "ResourceAvailability":
                            resourceAvailability = int.Parse(value);
                            break;
                        case "StatusCodeName":
                            statusCodeName = value;
                            break;
                        case "ActiveMfgOrder":
                            activeMfgOrder = value;
                            break;
                    }
                }
            }

            var resourceActiveMfgOrder = new ActiveMfgOrder
            {
                Name = activeMfgOrder
            };

            var resourceStatusDetails = new ResourceStatusDetails
            {
                Resource = resource,
                ReasonCodeName = reasonCodeName,
                ResourceAvailability = resourceAvailability,
                StatusCodeName = statusCodeName,
                TimeAtStatus = timeAtStatus
            };

            var valueResourceStatusDetails = new Value
            {
                ActiveMfgOrder = resourceActiveMfgOrder,
                ResourceStatusDetails = resourceStatusDetails
            };

            var rootResourceStatusDetails = new Root
            {
                value = new[] { valueResourceStatusDetails }
            };

            string resourceStatusData = JsonConvert.SerializeObject(rootResourceStatusDetails, Formatting.None);

            // Broadcast data to the client who selected the related transaction resource (equipment level)

            if (IsGroupExist(groupName))
            {
                await Clients.Group(groupName).receiveResourceSetupTxnUpdate(transactionType, resourceStatusData);
            }

            // Broadcast data to the client who selected the parent resource of the related transaction resource (line level)

            if (!string.IsNullOrEmpty(parentResource))
            {
                if (IsGroupExist(parentResource))
                {
                    await Clients.Group(parentResource).receiveResourceSetupTxnUpdate(transactionType, resourceStatusData);
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task BroadcastMessage(string groupName, string jsonParams)
    {
        try
        {
            List<List<string>> keyValuePairs = JsonConvert.DeserializeObject<List<List<string>>>(jsonParams);

            string message = string.Empty;
            bool isFailureMessage = false;
            string messageDateTime = string.Empty;

            foreach (var pair in keyValuePairs)
            {
                if (pair.Count == 2)
                {
                    string key = pair[0];
                    string value = pair[1];

                    switch (key)
                    {
                        case "Message":
                            message = value;
                            break;
                        case "IsFailureMessage":
                            isFailureMessage = bool.Parse(value);
                            break;
                        case "MessageDateTimeGMT":
                            messageDateTime = value;
                            break;
                    }
                }
            }

            if (IsGroupExist(groupName) && !string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(messageDateTime))
            {
                DateTime messageDateTimeUTC = DateTime.SpecifyKind(DateTime.Parse(messageDateTime), DateTimeKind.Utc);
                await Clients.Group(groupName).ReceiveMessage(groupName, message, isFailureMessage, messageDateTimeUTC);
            }
        }
        catch (Exception)
        {
            throw;
        }
    }




    // Subscribe the client to a specific group to ensure that only members of the specified group receive updates.
    public async Task SubscribeToGroup(string groupName)
    {
        try
        {
            if (!string.IsNullOrEmpty(groupName))
            {

                await RemoveFromAnyPreviousGroup(Context.ConnectionId);

                if (TryAddGroup(Context.ConnectionId, groupName))
                {
                    await Groups.Add(Context.ConnectionId, groupName);
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task SubscribeToMultipleGroups(List<string> groupNames, bool removeFromAPreviousGroups)
    {
        try
        {
            if (groupNames == null || groupNames.Count == 0)
            {
                throw new ArgumentException("Group names list cannot be null or empty.");
            }

            if (removeFromAPreviousGroups)
            {
                await RemoveFromAnyPreviousGroup(Context.ConnectionId);
            }

            foreach (var groupName in groupNames)
            {
                if (!string.IsNullOrEmpty(groupName) && TryAddGroup(Context.ConnectionId, groupName))
                {
                    await Groups.Add(Context.ConnectionId, groupName);
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }


    // Remove the client from any previous subscribed group.
    private async Task RemoveFromAnyPreviousGroup(string connectionId)
    {
        try
        {
            List<string> result;
            TryRemoveConnection(connectionId, out result);
            if (result != null)
            {
                foreach (var item in result)
                {
                    await Groups.Remove(Context.ConnectionId, item);
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    // Adding the client to group.
    private static bool TryAddGroup(string connectionId, string groupName)
    {
        List<string> groups;

        if (!ClientGroups.TryGetValue(connectionId, out groups))
        {
            return ClientGroups.TryAdd(connectionId, new List<string>() { groupName });
        }

        if (!groups.Contains(groupName))
        {
            groups.Add(groupName);
        }

        return true;
    }

    private static bool TryRemoveConnection(string connectionId, out List<string> result)
    {
        return ClientGroups.TryRemove(connectionId, out result);
    }

    // Method to verify if a group exists before broadcasting. We want to avoid broadcasting to non-existent groups.
    protected bool IsGroupExist(string groupName)
    {
        if (!string.IsNullOrEmpty(groupName))
        {
            foreach (var groups in ClientGroups.Values)
            {
                if (groups.Contains(groupName))
                {
                    return true;
                }
            }
        }

        return false;
    }

    protected async Task<(string URL, string BearerToken)> GetSystemInfo()
    {
        try
        {
            string configFilePath = HostingEnvironment.MapPath(@"~/appsettings.json");

            if (!File.Exists(configFilePath))
            {
                return (null, null);
            }

            using (var reader = new StreamReader(configFilePath))
            {
                var json = await reader.ReadToEndAsync();
                var jObject = JObject.Parse(json);

                var systemInfo = jObject["AppServer"];
                var systemCredentials = jObject["SystemCredentials"];

                if (systemInfo == null || systemCredentials == null)
                {
                    return (null, null);
                }

                string host = systemInfo["Host"]?.ToString();
                string port = systemInfo["Port"]?.ToString();
                bool useSSL = systemInfo["UseSSL"]?.ToObject<bool>() ?? false;


                UriBuilder uriBuilder = new UriBuilder
                {
                    Scheme = useSSL ? "https" : "http",
                    Host = host,
                    Port = int.Parse(port)
                };

                string url = uriBuilder.ToString();

                string utcOffset = "-00:00:00";
                string userName = systemCredentials["UserName"]?.ToString();
                string password = systemCredentials["Password"]?.ToString();
                bool isEncrypted = systemCredentials["IsEncrypted"]?.ToObject<bool>() ?? false;

                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                {
                    return (null, null);
                }
                else
                {
                    AuthorizationTokenJSONInfo authorizationToken = new AuthorizationTokenJSONInfo
                    {
                        Username = userName,
                        Password = new Password
                        {
                            Value = password,
                            IsEncrypted = isEncrypted
                        },
                        Utcoffset = utcOffset
                    };

                    return (url, Base64Encode(authorizationToken));
                }

            }
        }
        catch (Exception)
        {
            throw;
        }
    }



    public static string Base64Encode(object obj)
    {
        string json = JsonConvert.SerializeObject(obj);
        byte[] bytes = Encoding.Default.GetBytes(json);
        return Convert.ToBase64String(bytes);
    }

    public class Password
    {
        public string Value { get; set; }
        public bool IsEncrypted { get; set; }
    }

    public class AuthorizationTokenJSONInfo
    {
        public string Username { get; set; }
        public Password Password { get; set; }
        public string Utcoffset { get; set; }
    }

    public class ActiveMfgOrder
    {
        public string Name { get; set; }
    }

    public class ResourceStatusDetails
    {
        public string Resource { get; set; }
        public string ReasonCodeName { get; set; }
        public string StatusCodeName { get; set; }
        public double TimeAtStatus { get; set; }
        public int ResourceAvailability { get; set; }
    }

    public class Value
    {
        public ActiveMfgOrder ActiveMfgOrder { get; set; }
        public ResourceStatusDetails ResourceStatusDetails { get; set; }
    }

    public class Root
    {
        public Value[] value { get; set; }
    }
}
