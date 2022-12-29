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

namespace ChummerHub.Services.Application_Insights
{
    /// <summary>
    /// 
    /// </summary>
    public class MyTelemetryInitializer : ITelemetryInitializer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="telemetry"></param>
        public void Initialize(ITelemetry telemetry)
        {
            switch (telemetry)
            {
                case RequestTelemetry request when request.ResponseCode == "404":
                    request.Success = true;
                    break;
            }
            //var requestTelemetry = telemetry as RequestTelemetry;
            //// Is this a TrackRequest() ?
            //if (requestTelemetry == null) return;
            //int code;
            //bool parsed = Int32.TryParse(requestTelemetry.ResponseCode, out code);
            //if (!parsed) return;
            //if (code >= 400 && code < 500)
            //{
            //    // If we set the Success property, the SDK won't change it:
            //    requestTelemetry.Success = true;
            //    // Allow us to filter these requests in the portal:
            //    requestTelemetry.Context.Properties["Overridden400s"] = "true";
            //}

        }
    }
}
