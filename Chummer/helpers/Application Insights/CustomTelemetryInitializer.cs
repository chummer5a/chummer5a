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
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using NLog;

namespace Chummer
{
    public class CustomTelemetryInitializer : ITelemetryInitializer
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // Set session data:
        //private static string Hostname =  Dns.GetHostName();
        private static readonly string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private static readonly bool IsMilestone = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Revision == 0;


        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry == null)
                throw new ArgumentNullException(nameof(telemetry));
            if (!telemetry.Context.GlobalProperties.ContainsKey("Milestone"))
            {
                telemetry.Context.GlobalProperties.Add("Milestone", IsMilestone.ToString(GlobalOptions.InvariantCultureInfo));
            }
            else
                telemetry.Context.GlobalProperties["Milestone"] = IsMilestone.ToString(GlobalOptions.InvariantCultureInfo);
             telemetry.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            if (Properties.Settings.Default.UploadClientId != Guid.Empty)
            {
                //sometimes, there are odd values stored in the UploadClientId.
                if (!Properties.Settings.Default.UploadClientId.ToString().IsGuid())
                {
                    Properties.Settings.Default.UploadClientId = Guid.NewGuid();
                    Properties.Settings.Default.Save();
                }
                telemetry.Context.Cloud.RoleInstance = Properties.Settings.Default.UploadClientId.ToString();
                telemetry.Context.Device.Id = Properties.Settings.Default.UploadClientId.ToString();
            }
            telemetry.Context.Session.Id = Properties.Settings.Default.UploadClientId.ToString(); 
            telemetry.Context.User.Id = Properties.Settings.Default.UploadClientId.ToString(); 

            telemetry.Context.User.Id = telemetry.Context.Device.Id;
            telemetry.Context.Component.Version = Version;
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //don't fill the "productive" log with garbage from debug sessions
                telemetry.Context.InstrumentationKey = "f4b2ea1b-afe4-4bd6-9175-f5bb167a4d8b";
            }
            foreach (var plugin in Program.PluginLoader.MyActivePlugins)
            {
                try
                {
                    telemetry = plugin.SetTelemetryInitialize(telemetry);
                }
                catch (Exception e)
                {
                    Log.Error(e);
#if DEBUG
                    throw;
#endif
                }
            }
        }
    }
}
