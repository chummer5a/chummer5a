using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;

namespace Chummer
{
    public class DropUserdataTelemetryProcessor : ITelemetryProcessor
    {
        
        private string UserProfilePath = String.Empty;
        private ITelemetryProcessor Next { get; set; }

        // You can pass values from .config
        public string MyParamFromConfigFile { get; set; }

        // Link processors to each other in a chain.
        public DropUserdataTelemetryProcessor(ITelemetryProcessor next, string UserProfilePath)
        {
            this.Next = next;
            this.UserProfilePath = UserProfilePath;
        }
        public void Process(ITelemetry item)
        {
            ModifyItem(item);

            this.Next.Process(item);
        }

        
        // Example: replace with your own modifiers.
        private void ModifyItem(ITelemetry item)
        {
            if (item is TraceTelemetry trace)
            {
                trace.Message = trace.Message?.CheapReplace(UserProfilePath, () => @"{username}", true);
                return;
            }

            if (item is RequestTelemetry req)
            {
                string newurl = req.Url?.ToString()?.CheapReplace(UserProfilePath, () => @"{username}", true);
                if (!String.IsNullOrEmpty(newurl))
                    req.Url = new Uri(newurl);
                return;
            }

            if (item is ExceptionTelemetry exception)
            {
                if (exception.Exception != null)
                {
                    if (exception.Message == null)
                    {
                        exception.Message = exception.Exception.Message?.CheapReplace(UserProfilePath, () => @"{username}", true);
                    }
                }
                else
                    exception.Message = exception.Message?.CheapReplace(UserProfilePath, () => @"{username}", true);
                return;
            }
            
            
        }

    }
}
