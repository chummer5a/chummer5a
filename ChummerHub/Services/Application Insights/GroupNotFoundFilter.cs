using System;
using System.Net;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace ChummerHub.Services.Application_Insights
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupNotFoundFilter : ITelemetryProcessor
    {

        private ITelemetryProcessor Next { get; set; }

        // You can pass values from .config
        //public string MyParamFromConfigFile { get; set; }

        // Link processors to each other in a chain.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        public GroupNotFoundFilter(ITelemetryProcessor next)
        {
            this.Next = next;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
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
            if (item is RequestTelemetry requestTelemetry && int.Parse(requestTelemetry.ResponseCode) == (int) HttpStatusCode.NotFound)
            {
                if (requestTelemetry.Context?.Operation?.Name.Contains("GetSINnerGroupFromSINerById") == true)
                    return false;
            }

            return true;
        }

        // Example: replace with your own modifiers.
        private void ModifyItem(ITelemetry item)
        {
            if (item is RequestTelemetry requestTelemetry && int.Parse(requestTelemetry.ResponseCode) == (int)HttpStatusCode.NotFound)
            {
                requestTelemetry.Success = true;
            }

            this.Next.Process(item);
        }
    }
}
