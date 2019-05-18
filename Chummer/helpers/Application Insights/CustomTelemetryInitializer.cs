using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Chummer
{
    public class CustomTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.User.Id = Environment.UserName;
            telemetry.Context.Session.Id = Program.ApplicationInsightsTelemetryClient.Context.Session.Id;
            telemetry.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            telemetry.Context.Device.Id = Program.ApplicationInsightsTelemetryClient.Context.Device.Id;
            telemetry.Context.Component.Version = Program.ApplicationInsightsTelemetryClient.Context.Component.Version;
            telemetry.Context.Location.Ip = Program.ApplicationInsightsTelemetryClient.Context.Location.Ip;
        }
    }

}
