using System;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.SnapshotCollector;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChummerHub.Services.Application_Insights
{
    public class SnapshotCollectorTelemetryProcessorFactory : ITelemetryProcessorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public SnapshotCollectorTelemetryProcessorFactory(IServiceProvider serviceProvider) =>
            _serviceProvider = serviceProvider;

        public ITelemetryProcessor Create(ITelemetryProcessor next)
        {
            try
            {
                var snapshotConfigurationOptions = _serviceProvider.GetService<IOptions<SnapshotCollectorConfiguration>>();
                ITelemetryProcessor ret = new SnapshotCollectorTelemetryProcessor(next, configuration: snapshotConfigurationOptions.Value);
                return ret;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.ToString(), e);
                Console.WriteLine(e.ToString());
                return null;
            }

        }
    }
}
