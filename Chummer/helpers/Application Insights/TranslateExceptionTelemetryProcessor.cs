using System;
using System.Globalization;
using System.Threading;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;

namespace Chummer
{
    public class TranslateExceptionTelemetryProcessor : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }

        // You can pass values from .config
        public string MyParamFromConfigFile { get; set; }

        // Link processors to each other in a chain.
        public TranslateExceptionTelemetryProcessor(ITelemetryProcessor next)
        {
            this.Next = next;
        }
        public void Process(ITelemetry item)
        {
            // To filter out an item, just return
            //if (!OKtoSend(item)) { return; }
            // Modify the item if required
            ModifyItem(item);

            this.Next.Process(item);
        }

        // Example: replace with your own criteria.
        private bool OKtoSend(ITelemetry item)
        {
            ExceptionTelemetry exceptionTelemetry = item as ExceptionTelemetry;
            if (exceptionTelemetry == null) return true;

            return false;
        }

        // Example: replace with your own modifiers.
        private void ModifyItem(ITelemetry item)
        {
            ExceptionTelemetry exceptionTelemetry = item as ExceptionTelemetry;
            if (exceptionTelemetry == null) return;
            CultureInfo oldCI = Thread.CurrentThread.CurrentCulture;

            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            try
            {
                throw exceptionTelemetry.Exception;
            }
            catch (Exception ex)
            {
                exceptionTelemetry.Exception = ex;
            }
            Thread.CurrentThread.CurrentCulture = oldCI;
            Thread.CurrentThread.CurrentUICulture = oldCI;
        }
    }
}
