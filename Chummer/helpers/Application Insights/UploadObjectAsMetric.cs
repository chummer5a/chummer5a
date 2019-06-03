using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Metrics;

namespace Chummer
{
    public static class UploadObjectAsMetric
    {
        public static bool UploadObject(TelemetryClient tc, object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));


            List<PropertyInfo> allProperties = null;
            String name = null;
            if (obj is Type)
            {
                allProperties = (obj as Type).GetProperties().ToList();
                name = (obj as Type).Name;
            }
            else
            {
                allProperties = obj.GetType().GetProperties().ToList();
                name = obj.ToString();
            }

            var boolProperties = (from a in allProperties where a.PropertyType == typeof(bool) select a).ToList();

            //var propnames = (from a in boolProperties select a.Name).ToList();
            MetricIdentifier micount = new MetricIdentifier(name, "MetricsReportCount");
            var mcount =  tc.GetMetric(micount);
            mcount.TrackValue(1);

            foreach (var prop in boolProperties)
            {
                MetricIdentifier mi = new MetricIdentifier(name, prop.Name );
                var metric = tc.GetMetric(mi);
                var val = prop.GetValue(obj, null);
                Console.WriteLine("{0}={1}", prop.Name, val);
                if (Boolean.TryParse(val.ToString(), out bool boolval))
                {
                    if(boolval)
                        metric.TrackValue(1);
                    else
                        metric.TrackValue(0);
                }
                
            }

            return true;
        }
    }
}
