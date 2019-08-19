using System;
using System.Collections;
using System.Net;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace ChummerHub.Services.Application_Insights
{
    /// <summary>
    /// 
    /// </summary>
    public class ExceptionDataProcessor : ITelemetryProcessor
    {

        private ITelemetryProcessor Next { get; set; }

        // You can pass values from .config
        //public string MyParamFromConfigFile { get; set; }

        // Link processors to each other in a chain.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        public ExceptionDataProcessor(ITelemetryProcessor next)
        {
            this.Next = next;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Process(ITelemetry item)
        {
            // Modify the item if required
            ModifyItem(item);

            this.Next.Process(item);
        }

        // Example: replace with your own modifiers.
        private void ModifyItem(ITelemetry item)
        {
            if (item is ExceptionTelemetry exception)
            {

                if (exception.Exception != null)
                {
                    foreach (DictionaryEntry de in exception.Exception.Data)
                    {
                        if (!exception.Properties.ContainsKey(de.Key.ToString()))
                            exception.Properties.Add(de.Key.ToString(), de.Value?.ToString());
                    }
                }
                return;
            }
        }
    }
}

