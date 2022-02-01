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
using System.Collections;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Chummer
{
    public class DropUserdataTelemetryProcessor : ITelemetryProcessor
    {
        private readonly string _strUserProfilePath;
        private ITelemetryProcessor Next { get; }

        // You can pass values from .config
        public string MyParamFromConfigFile { get; set; }

        [CLSCompliant(false)]

        // Link processors to each other in a chain.
        public DropUserdataTelemetryProcessor(ITelemetryProcessor next, string strUserProfilePath)
        {
            Next = next;
            _strUserProfilePath = strUserProfilePath;
        }

        [CLSCompliant(false)]
        public void Process(ITelemetry item)
        {
            ModifyItem(item);
            if (GlobalSettings.UseLoggingApplicationInsights == UseAILogging.Trace)
            {
                Next.Process(item);
                return;
            }
            if ((item is PageViewTelemetry
                || item is PageViewPerformanceTelemetry)
                && (GlobalSettings.UseLoggingApplicationInsights >= UseAILogging.Info)
                && CustomTelemetryInitializer.IsMilestone == false)
            {
                Next.Process(item);
                return;
            }
            if (GlobalSettings.UseLoggingApplicationInsights >= UseAILogging.Crashes
                && item is ExceptionTelemetry exceptionTelemetry
                && CustomTelemetryInitializer.IsMilestone == false
                && (exceptionTelemetry.Exception.Data.Contains("IsCrash")
                    || exceptionTelemetry.Properties.ContainsKey("IsCrash")))
            {
                Next.Process(item);
                return;
            }
            if (GlobalSettings.UseLoggingApplicationInsights >= UseAILogging.OnlyMetric && item is MetricTelemetry)
            {
                Next.Process(item);
                return;
            }
            if (GlobalSettings.UseLoggingApplicationInsights >= UseAILogging.NotSet
                && item is TraceTelemetry traceTelemetry
                && traceTelemetry.SeverityLevel >= SeverityLevel.Information
                && CustomTelemetryInitializer.IsMilestone == false)
            {
                Next.Process(item);
                return;
            }
            
        }

        // Example: replace with your own modifiers.
        private void ModifyItem(ITelemetry item)
        {
            switch (item)
            {
                case TraceTelemetry trace:
                    trace.Message = trace.Message?.Replace(_strUserProfilePath, "{username}", StringComparison.OrdinalIgnoreCase);
                    return;

                case RequestTelemetry req:
                    {
                        string newurl = req.Url?.ToString().Replace(_strUserProfilePath, "{username}", StringComparison.OrdinalIgnoreCase);
                        if (!string.IsNullOrEmpty(newurl))
                            req.Url = new Uri(newurl);
                        return;
                    }
                case ExceptionTelemetry exception when exception.Exception != null:
                    {
                        foreach (DictionaryEntry de in exception.Exception.Data)
                        {
                            if (!exception.Properties.ContainsKey(de.Key.ToString()))
                                exception.Properties.Add(de.Key.ToString(), de.Value?.ToString());
                        }
                        if (exception.Message == null)
                        {
                            exception.Message = exception.Exception.Message?.Replace(_strUserProfilePath, "{username}", StringComparison.OrdinalIgnoreCase);
                        }

                        break;
                    }
                case ExceptionTelemetry exception:
                    exception.Message = exception.Message?.Replace(_strUserProfilePath, "{username}", StringComparison.OrdinalIgnoreCase);
                    break;
            }
        }
    }
}
