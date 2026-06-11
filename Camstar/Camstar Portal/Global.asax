<%-- Copyright Siemens 2025   --%>
<%@ Application Language="C#" %>
<%@ Import Namespace="System.Diagnostics" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="System.Threading" %>
<%@ Import Namespace="System.Windows.Interop" %>
<%@ Import Namespace="System.Web.Optimization" %>
<%@ Import Namespace="Camstar.Portal" %>
<%@ Import Namespace="Camstar.WebPortal.FormsFramework.Utilities" %>
<%@ Import Namespace="Camstar.WebPortal.Utilities" %>
<%@ Import Namespace="System.Configuration" %>
<%@ Import Namespace="Camstar.Telemetry" %>


<script runat="server">

    private string _blankRpt;
    private bool _telemetrySetupDone = false;
    private static ActivitySource activitySource;
    void Application_Start(object sender, EventArgs e)
    {

        //#if DEBUG
        BundleTable.EnableOptimizations = false;







        //#else

        //                BundleTable.EnableOptimizations = true;
        //#endif







        //#if DEBUG
        //       foreach (var bundle in BundleTable.Bundles)
        //       {
        //           bundle.Transforms.Clear();
        //       }
        //#else
        //        foreach (var bundle in BundleTable.Bundles)
        //        {
        //            bundle.Transforms.Clear();
        //        }
        //#endif

        Camstar.Portal.BundleConfig.RegisterBundles(BundleTable.Bundles);

        // Enable TLS 1.0, 1.1, or 1.2 if configured
        System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;


        // Code that runs on application startup
        try
        {
            _blankRpt = this.Server.MapPath("~/BlankReport.rpt");
            new System.Threading.Thread(new System.Threading.ThreadStart(CrystalReportStart)).Start();
            if (System.IO.Directory.Exists(Camstar.WebPortal.Utilities.CamstarPortalSection.Settings.DefaultSettings.UploadDirectory))
            {
                System.IO.Directory.GetDirectories(Camstar.WebPortal.Utilities.CamstarPortalSection.Settings.DefaultSettings.UploadDirectory).ToList().ForEach(d =>
                {
                    try
                    {
                        System.IO.Directory.Delete(d, true);
                    }
                    catch { }
                });
                System.IO.Directory.GetFiles(Camstar.WebPortal.Utilities.CamstarPortalSection.Settings.DefaultSettings.UploadDirectory).ToList().ForEach(f =>
                {
                    try
                    {
                        System.IO.File.Delete(f);
                    }
                    catch { }
                });
            }

            SetupOpenTelemetry();
        }
        catch { }
    }

    void SetupOpenTelemetry()
    {
        TelemetryOperation.SetupOpenTelemetry();
    }

    void Session_End(object sender, EventArgs e)
    {
        Camstar.WebPortal.FormsFramework.Utilities.FrameworkSession mFSession;
        if (Session[Camstar.WebPortal.Constants.SessionConstants.FrameworkSession] != null)
        {
            mFSession = Session[Camstar.WebPortal.Constants.SessionConstants.FrameworkSession] as Camstar.WebPortal.FormsFramework.Utilities.FrameworkSession;
            mFSession.Logout();
        }

    }

    void Application_BeginRequest(object sender, EventArgs e)
    {
        string[] languages = HttpContext.Current.Request.UserLanguages;

        if (languages != null && languages.Length > 0)
        {
            try
            {
                string language = languages[0].ToLowerInvariant().Trim();
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(language);
            }
            catch (ArgumentException)
            {
            }
        }

        if (CamstarPortalSection.Settings.CurrentCultureSettings != null)
        {
            var cultureSettings = CamstarPortalSection.Settings.CurrentCultureSettings;
            if (cultureSettings.DateTimeFormat != null && !Thread.CurrentThread.CurrentCulture.DateTimeFormat.IsReadOnly)
            {
                if (!string.IsNullOrEmpty(cultureSettings.DateTimeFormat.ShortDatePattern))
                    Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern = cultureSettings.DateTimeFormat.ShortDatePattern;
                if (!string.IsNullOrEmpty(cultureSettings.DateTimeFormat.LongTimePattern))
                    Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongTimePattern = cultureSettings.DateTimeFormat.LongTimePattern;
            }
            if (cultureSettings.NumberFormat != null && !Thread.CurrentThread.CurrentCulture.NumberFormat.IsReadOnly)
            {
                if (!string.IsNullOrEmpty(cultureSettings.NumberFormat.NumberDecimalSeparator))
                    Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator = cultureSettings.NumberFormat.NumberDecimalSeparator;
            }
        }

        // Replace short mounth/day like 3 for March to 03.
        if (!Thread.CurrentThread.CurrentCulture.DateTimeFormat.IsReadOnly)
        {
            var shortPattern = Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern;
            if (shortPattern.IndexOf("MM", StringComparison.Ordinal) < 0 && shortPattern.IndexOf("M", StringComparison.Ordinal) > -1)
                shortPattern = shortPattern.Replace("M", "MM");
            if (shortPattern.IndexOf("dd", StringComparison.Ordinal) < 0 && shortPattern.IndexOf("d", StringComparison.Ordinal) > -1)
                shortPattern = shortPattern.Replace("d", "dd");
            Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern = shortPattern;
        }


    }

    protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
    {
        try
        {
        var telemetryAnalyticsPlatform = ConfigurationManager.AppSettings[Settings.TELEMETRY_ANALYTICS_PLATFORM];


            if (TelemetryOperation.IsTelemetryEnabled && !string.IsNullOrEmpty(telemetryAnalyticsPlatform) &&
                telemetryAnalyticsPlatform.Equals(Settings.TELEMETRY_ANALYTICS_PLATFORM_OPENTELEMETRY, StringComparison.OrdinalIgnoreCase))
            {
                var userId = HttpContext.Current?.User?.Identity?.Name;

                if (!string.IsNullOrEmpty(userId))
                {

                    TelemetryOperation.UpdateActiveUsers(userId, true);
                }
                else
                {

                    TelemetryOperation.UpdateActiveUsers(null, false);
                }
            }
        }
        catch (Exception ex)
        {
            TelemetryOperation.LogException(ex);
        }
    }

    void Application_Error(object sender, EventArgs e)
    {
        // Get the exception object.
        var exc = Server.GetLastError();

        const string source = "Camstar Portal";
        if (exc != null)
        {

            var baseExc = Server.GetLastError().GetBaseException();
        var telemetryAnalyticsPlatform = ConfigurationManager.AppSettings[Settings.TELEMETRY_ANALYTICS_PLATFORM];

            if (TelemetryOperation.IsTelemetryEnabled && !string.IsNullOrEmpty(telemetryAnalyticsPlatform) &&
                telemetryAnalyticsPlatform.Equals(Settings.TELEMETRY_ANALYTICS_PLATFORM_OPENTELEMETRY, StringComparison.OrdinalIgnoreCase))
            {
                Camstar.Telemetry.TelemetryOperation.TrackServerException(baseExc);
            }

            var message = string.Format("{0}\n {1}\n {2}\n", baseExc.Message, baseExc.Source, baseExc.StackTrace);
            try
            {
                if (!EventLog.SourceExists(source))
                {
                    EventLog.CreateEventSource(source,
                                               "Camstar");
                }
                EventLog.WriteEntry(source, message, EventLogEntryType.Error);
            }
            catch { /*try to write to the event log, but don't crash or throw in the exception handler, please!*/ }
        }

        // Handle HTTP errors
        if (exc is HttpUnhandledException)
        {
            if ((exc.InnerException is PageFlowException))
            {
                var msgText = exc.InnerException.Message;
                Response.Write("<script type=text/javascript>\n" +
                               "__toppage.displayMessage(\"<span>ERROR!</span>" + msgText + "\", \"Error\")");

                Response.Write("<"); Response.Write("/script>");
                Server.ClearError();
            }
        }
    }

    void CrystalReportStart()
    {
        //CrystalDecisions.CrystalReports.Engine.ReportDocument doc = new CrystalDecisions.CrystalReports.Engine.ReportDocument();
        //doc.Load(_blankRpt);
        //doc.Close();
        //doc.Dispose();
    }

    void Application_End()
    {
        TelemetryOperation.DisposeOpenTelemetry();
    }

</script>