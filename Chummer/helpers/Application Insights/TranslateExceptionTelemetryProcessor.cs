using System;
using System.Collections;
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
            var translateCultureInfo = new CultureInfo("en");
            try
            {
                string msg =
                    TranslateExceptionMessage(exceptionTelemetry.Exception, translateCultureInfo);
                if (!exceptionTelemetry.Properties.ContainsKey("Translated"))
                    exceptionTelemetry.Properties.Add("Translated", msg); 
            }
            catch (Exception ex)
            {
                string msg = ex.ToString();
                if (!exceptionTelemetry.Properties.ContainsKey("Message"))
                    exceptionTelemetry.Properties.Add("Message", ex.Message);
                if (!exceptionTelemetry.Properties.ContainsKey("Translated"))
                    exceptionTelemetry.Properties.Add("Translated", msg);
            }
        }

        public static string TranslateExceptionMessage(Exception exception, CultureInfo targetCulture)
        {
            Assembly a = exception.GetType().Assembly;
            ResourceManager rm = new ResourceManager(a.GetName().Name, a);
            ResourceSet rsOriginal = rm.GetResourceSet(Thread.CurrentThread.CurrentUICulture, true, true);
            ResourceSet rsTranslated = rm.GetResourceSet(targetCulture, true, true);

            var result = exception.Message;

            foreach (DictionaryEntry item in rsOriginal)
            {
                if (!(item.Value is string message))
                    continue;

                string translated = rsTranslated.GetString(item.Key.ToString(), false);

                if (!message.Contains("{"))
                {
                    result = result.Replace(message, translated);
                }
                else
                {
                    var pattern = $"{Regex.Escape(message)}";
                    pattern = Regex.Replace(pattern, @"\\{([0-9]+)\}", "(?<group$1>.*)");

                    var regex = new Regex(pattern);

                    var replacePattern = translated;
                    replacePattern = Regex.Replace(replacePattern, @"{([0-9]+)}", @"${group$1}");
                    replacePattern = replacePattern.Replace("\\$", "$");

                    result = regex.Replace(result, replacePattern);
                }
            }

            return result;
        }
    }
}
