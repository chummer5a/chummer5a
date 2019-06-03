using System;
using System.Net;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace ChummerHub.Services.Application_Insights
{
    public class GroupNotFoundFilter : ITelemetryProcessor
    {

        private ITelemetryProcessor Next { get; set; }

        // You can pass values from .config
        //public string MyParamFromConfigFile { get; set; }

        // Link processors to each other in a chain.
        public GroupNotFoundFilter(ITelemetryProcessor next)
        {
            this.Next = next;
        }

        public void Process(ITelemetry item)
        {
            // To filter out an item, just return
            if (!OKtoSend(item))
            {
                return;
            }

            // Modify the item if required
            ModifyItem(item);

            this.Next.Process(item);
        }

        // Example: replace with your own criteria.
        private bool OKtoSend(ITelemetry item)
        {
            var requestTelemetry = item as RequestTelemetry;
            if (requestTelemetry != null && int.Parse(requestTelemetry.ResponseCode) == (int) HttpStatusCode.NotFound)
            {
                if (requestTelemetry.Context?.Operation?.Name.Contains("GetSINnerGroupFromSINerById") == true)
                    return false;
            }

            return true;
        }

        // Example: replace with your own modifiers.
        private void ModifyItem(ITelemetry item)
        {
            //done in MyTelemetryInitializer

            //var requestTelemetry = item as RequestTelemetry;
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
            //// else leave the SDK to set the Success property    
        }
    }
}
