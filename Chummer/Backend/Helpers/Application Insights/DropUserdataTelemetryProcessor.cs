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
using System.Text.RegularExpressions;
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
                && GlobalSettings.UseLoggingApplicationInsights >= UseAILogging.Info
                && !Utils.IsMilestoneVersion)
            {
                Next.Process(item);
                return;
            }
            if (GlobalSettings.UseLoggingApplicationInsights >= UseAILogging.Crashes
                && item is ExceptionTelemetry exceptionTelemetry
                && !Utils.IsMilestoneVersion
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
                && !Utils.IsMilestoneVersion)
            {
                Next.Process(item);
            }
        }

        // Example: replace with your own modifiers.
        private void ModifyItem(ITelemetry item)
        {
            switch (item)
            {
                case TraceTelemetry trace:
                    trace.Message = RedactUserData(trace.Message);
                    return;

                case RequestTelemetry req:
                    {
                        string originalUrl = req.Url?.ToString();
                        if (!string.IsNullOrEmpty(originalUrl))
                        {
                            string redactedUrl = RedactUserData(originalUrl);
                            if (redactedUrl != originalUrl)
                            {
                                try
                                {
                                    req.Url = new Uri(redactedUrl);
                                }
                                catch (UriFormatException)
                                {
                                    // If the URL is invalid after replacement, keep the original URL
                                    // This prevents crashes when usernames contain invalid URI characters or can be inserted in a way that breaks the URI format
                                }
                            }
                        }
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
                            exception.Message = RedactUserData(exception.Exception.Message);
                        }

                        break;
                    }
                case ExceptionTelemetry exception:
                    exception.Message = RedactUserData(exception.Message);
                    break;
            }
        }

        /// <summary>
        /// Redacts username from strings.
        /// </summary>
        /// <param name="input">The string to redact</param>
        /// <returns>The redacted string, or original if no redaction needed</returns>
        private string RedactUserData(string input)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(_strUserProfilePath))
                return input;

            // Simple string replacement for file paths - this is the most common case
            // and avoids the complexity and performance overhead of regex patterns
            return input.Replace(_strUserProfilePath, "{username}", StringComparison.OrdinalIgnoreCase);
        }
    }
}
