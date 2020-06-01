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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Metrics;

namespace Chummer
{
    public static class UploadObjectAsMetric
    {
        public static bool UploadObject(TelemetryClient tc, object obj)
        {
            if (tc == null)
                throw new ArgumentNullException(nameof(tc));
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            List<PropertyInfo> allProperties;
            string name;
            if (obj is Type objAsType)
            {
                allProperties = objAsType.GetProperties().ToList();
                name = objAsType.Name;
            }
            else
            {
                allProperties = obj.GetType().GetProperties().ToList();
                name = obj.ToString();
            }

            //var propnames = (from a in boolProperties select a.Name).ToList();
            MetricIdentifier micount = new MetricIdentifier(name, "MetricsReportCount");
            var mcount =  tc.GetMetric(micount);
            mcount.TrackValue(1);

            foreach (var prop in allProperties.Where(x => x.PropertyType == typeof(bool)))
            {
                MetricIdentifier mi = new MetricIdentifier(name, prop.Name );
                var metric = tc.GetMetric(mi);
                var val = prop.GetValue(obj, null);
                Console.WriteLine("{0}={1}", prop.Name, val);
                if (bool.TryParse(val.ToString(), out bool boolval))
                {
                    metric.TrackValue(boolval ? 1 : 0);
                }
            }

            return true;
        }
    }
}
