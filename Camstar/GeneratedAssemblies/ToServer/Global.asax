<%-- Copyright Siemens 2025   --%>
<%@ Application Language="C#" %>
<%@ Import Namespace="Camstar.Telemetry" %>
<%@ Import Namespace="System" %>

<script runat="server">

    void Application_Start(object sender, EventArgs e)
    {
        TelemetryOperation.SetupOpenTelemetry("wcf");
    }

    /// <summary>
    /// Get environment value in string format using key
    /// </summary>
    /// <param name="key">Key to fetch from Environment</param>
    /// <returns></returns>
    string GetEnvironmentValue(string key)
    {
        return Environment.GetEnvironmentVariable(key);
    }

    /// <summary>
    /// Get Environment value in boolean format using key
    /// </summary>
    /// <param name="key">Key to fetch from Environment</param>
    /// <param name="defaultValue">Default value if Environment value cannot be converted to boolean</param>
    /// <returns></returns>
    bool GetBooleanEnvironmentValue(string key, bool defaultValue)
    {
        string value = GetEnvironmentValue(key);
        try
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            else
            {
                return Convert.ToBoolean(value);
            }
        }
        catch (Exception exception)
        {
            return defaultValue;
        }
    }
    void Application_End()
    {
        TelemetryOperation.DisposeOpenTelemetry();
    }
</script>