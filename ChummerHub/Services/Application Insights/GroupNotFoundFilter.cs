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
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Net;

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
        private static bool OKtoSend(ITelemetry item)
        {
            if (item is RequestTelemetry requestTelemetry && int.Parse(requestTelemetry.ResponseCode) == (int)HttpStatusCode.NotFound)
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
