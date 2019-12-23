/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
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
            telemetry.Context.User.Id = SessionId;
            telemetry.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            if (Properties.Settings.Default.UploadClientId != Guid.Empty)
            {
                //sometimes, there are odd values stored in the UploadClientId.
                bool isValid = Guid.TryParse(Properties.Settings.Default.UploadClientId.ToString(),
                    out Guid guidOutput);
                if (!isValid)
                {
                    Properties.Settings.Default.UploadClientId = Guid.NewGuid();
                    Properties.Settings.Default.Save();
                }
                telemetry.Context.Cloud.RoleInstance = Properties.Settings.Default.UploadClientId.ToString();
                telemetry.Context.Device.Id = Properties.Settings.Default.UploadClientId.ToString();
            }
            telemetry.Context.User.Id = telemetry.Context.Device.Id;
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
