<%-- Copyright Siemens 2025   --%>
<%@ Application Language="C#" %>
<%@ Import Namespace="Camstar.Telemetry" %>
 
<script runat="server">
    void Application_Start(object sender, EventArgs e)
    {
        TelemetryOperation.SetupOpenTelemetry();
    }
    void Application_Error(object sender, EventArgs e)
    {
        var telemetryAnalyticsPlatform = ConfigurationManager.AppSettings[Settings.TELEMETRY_ANALYTICS_PLATFORM];

        if (TelemetryOperation.IsTelemetryEnabled && !string.IsNullOrEmpty(telemetryAnalyticsPlatform) &&
            telemetryAnalyticsPlatform.Equals(Settings.TELEMETRY_ANALYTICS_PLATFORM_OPENTELEMETRY, StringComparison.OrdinalIgnoreCase))
        {
            // Get the exception object.
            var exc = Server.GetLastError();

            if (exc != null)
            {

                var baseExc = Server.GetLastError().GetBaseException();

                Camstar.Telemetry.TelemetryOperation.TrackServerException(baseExc);
            }
        }
    }


    void Application_End()
    {
        TelemetryOperation.DisposeOpenTelemetry();
    }
</script>