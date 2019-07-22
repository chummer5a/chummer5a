using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using NLog;

namespace Chummer
{
    public class CustomTelemetryInitializer : ITelemetryInitializer
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();
        // Set session data:
        private static string SessionId = Guid.NewGuid().ToString();
        //private static string Hostname =  Dns.GetHostName();
        private static string Version = System.Reflection.Assembly
            .GetExecutingAssembly().GetName().Version.ToString();

        private static bool IsMilestone = System.Reflection.Assembly
                                            .GetExecutingAssembly().GetName().Version.Revision == 0
            ? true
            : false;
        public static string Ip = null;

      
        public void Initialize(ITelemetry telemetry)
        {
            //personal data should not be submited
            //telemetry.Context.User.Id = Environment.UserName;
            if (!telemetry.Context.GlobalProperties.ContainsKey("Milestone"))
                telemetry.Context.GlobalProperties.Add("Milestone", IsMilestone.ToString());
            telemetry.Context.Session.Id = SessionId;
            telemetry.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            if (Properties.Settings.Default.UploadClientId != Guid.Empty)
            {
                telemetry.Context.Cloud.RoleInstance = Properties.Settings.Default.UploadClientId.ToString();
                telemetry.Context.Device.Id = Properties.Settings.Default.UploadClientId.ToString();
            }
            telemetry.Context.Component.Version = Version;
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //don't fill the "productive" log with garbage from debug sessions
                telemetry.Context.InstrumentationKey = "f4b2ea1b-afe4-4bd6-9175-f5bb167a4d8b";
            }
            if (Program.PluginLoader?.MyActivePlugins != null)
            {
                foreach (var plugin in Program.PluginLoader?.MyActivePlugins)
                {
                    try
                    {
                        telemetry = plugin.SetTelemetryInitialize(telemetry);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                    
                }
            }
        }
    }
}
