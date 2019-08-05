using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace ChummerHub.Services.Application_Insights
{
    /// <summary>
    /// 
    /// </summary>
    public class MyTelemetryInitializer : ITelemetryInitializer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="telemetry"></param>
        public void Initialize(ITelemetry telemetry)
        {
            switch (telemetry)
            {
                case RequestTelemetry request when request.ResponseCode == "404":
                    request.Success = true;
                    break;
            }
            //var requestTelemetry = telemetry as RequestTelemetry;
            //// Is this a TrackRequest() ?
            //if (requestTelemetry == null) return;
            //int code;
            //bool parsed = Int32.TryParse(requestTelemetry.ResponseCode, out code);
            //if (!parsed) return;
            //if (code >= 400 && code < 500)
            //{
            //    // If we set the Success property, the SDK won't change it:
            //    requestTelemetry.Success = true;
            //    // Allow us to filter these requests in the portal:
            //    requestTelemetry.Context.Properties["Overridden400s"] = "true";
            //}
            
        }
    }
}
