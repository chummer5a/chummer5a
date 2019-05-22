using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Chummer
{
    public class CustomTelemetryInitializer : ITelemetryInitializer
    {
        // Set session data:
        private static string SessionId = Guid.NewGuid().ToString();
        private static string Hostname =  Dns.GetHostName();
        private static string Version = System.Reflection.Assembly
            .GetExecutingAssembly().GetName().Version.ToString();
        private static string Ip = GetPublicIPAddress();

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.User.Id = Environment.UserName;
            telemetry.Context.Session.Id = SessionId;
            telemetry.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            telemetry.Context.Device.Id = Hostname;
            telemetry.Context.Component.Version = Version;
            telemetry.Context.Location.Ip = Ip;
            if (Program.MainForm?.PluginLoader?.MyActivePlugins != null)
            {
                foreach (var plugin in Program.MainForm?.PluginLoader?.MyActivePlugins)
                {
                    try
                    {
                        telemetry = plugin.SetTelemetryInitialize(telemetry);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    
                }
            }
        }

        public static string GetPublicIPAddress()
        {
            try
            {
                string pubIp = new System.Net.WebClient().DownloadString("https://api.ipify.org");
                return pubIp;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            //throw new Exception("No network adapters with an IPv4 address in the system!");
            return null;
        }
    }

}
