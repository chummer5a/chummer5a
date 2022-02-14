using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Collections;

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
        private static void ModifyItem(ITelemetry item)
        {
            if (item is ExceptionTelemetry exception)
            {
                if (exception.Exception != null)
                {
                    foreach (DictionaryEntry de in exception.Exception.Data)
                    {
                        string strKey = de.Key.ToString();
                        if (!string.IsNullOrEmpty(strKey) && !exception.Properties.ContainsKey(strKey))
                            exception.Properties.Add(strKey, de.Value?.ToString());
                    }
                }
            }
        }
    }
}

