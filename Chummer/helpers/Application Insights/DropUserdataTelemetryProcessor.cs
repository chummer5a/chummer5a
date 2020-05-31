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
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;

namespace Chummer
{
    public class DropUserdataTelemetryProcessor : ITelemetryProcessor
    {
        private readonly string UserProfilePath;
        private ITelemetryProcessor Next { get; }

        // You can pass values from .config
        public string MyParamFromConfigFile { get; set; }

        // Link processors to each other in a chain.
        public DropUserdataTelemetryProcessor(ITelemetryProcessor next, string UserProfilePath)
        {
            Next = next;
            this.UserProfilePath = UserProfilePath;
        }
        public void Process(ITelemetry item)
        {
            ModifyItem(item);
            if (GlobalOptions.UseLoggingApplicationInsights == UseAILogging.Trace)
            {
                Next.Process(item);
                return;
            }
            if (GlobalOptions.UseLoggingApplicationInsights >= UseAILogging.Crashes)
            {
                if (item is ExceptionTelemetry exceptionTelemetry)
                {
                    if (exceptionTelemetry.Exception.Data.Contains("IsCrash")
                        || exceptionTelemetry.Properties.ContainsKey("IsCrash"))
                    {
                        Next.Process(item);
                        return;
                    }
                }
            }
            if (GlobalOptions.UseLoggingApplicationInsights >= UseAILogging.OnlyMetric)
            {
                if (item is MetricTelemetry)
                {
                    Next.Process(item);
                    return;
                }
            }
            if (GlobalOptions.UseLoggingApplicationInsights >= UseAILogging.Info)
            {
                if (item is PageViewTelemetry
                    || item is PageViewPerformanceTelemetry)
                {
                    Next.Process(item);
                }
                else if (item is TraceTelemetry traceTelemetry)
                {
                    if (traceTelemetry.SeverityLevel >= SeverityLevel.Information)
                    {
                        Next.Process(item);
                    }
                }
            }
        }

        // Example: replace with your own modifiers.
        private void ModifyItem(ITelemetry item)
        {
            if (item is TraceTelemetry trace)
            {
                trace.Message = trace.Message?.Replace(UserProfilePath, @"{username}", StringComparison.OrdinalIgnoreCase);
                return;
            }

            if (item is RequestTelemetry req)
            {
                string newurl = req.Url?.ToString().Replace(UserProfilePath, @"{username}", StringComparison.OrdinalIgnoreCase);
                if (!string.IsNullOrEmpty(newurl))
                    req.Url = new Uri(newurl);
                return;
            }

            if (item is ExceptionTelemetry exception)
            {
                if (exception.Exception != null)
                {
                    foreach (DictionaryEntry de in exception.Exception.Data)
                    {
                        if (!exception.Properties.ContainsKey(de.Key.ToString()))
                            exception.Properties.Add(de.Key.ToString(), de.Value?.ToString());
                    }
                    if (exception.Message == null)
                    {
                        exception.Message = exception.Exception.Message?.Replace(UserProfilePath, @"{username}", StringComparison.OrdinalIgnoreCase);
                    }
                }
                else
                    exception.Message = exception.Message?.Replace(UserProfilePath, @"{username}", StringComparison.OrdinalIgnoreCase);
            }
        }

    }
}
