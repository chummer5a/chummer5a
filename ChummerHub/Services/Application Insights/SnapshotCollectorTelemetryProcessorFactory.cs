using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.SnapshotCollector;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace ChummerHub.Services.Application_Insights
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SnapshotCollectorTelemetryProcessorFactory'
    public class SnapshotCollectorTelemetryProcessorFactory : ITelemetryProcessorFactory
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SnapshotCollectorTelemetryProcessorFactory'
    {
        private readonly IServiceProvider _serviceProvider;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SnapshotCollectorTelemetryProcessorFactory.SnapshotCollectorTelemetryProcessorFactory(IServiceProvider)'
        public SnapshotCollectorTelemetryProcessorFactory(IServiceProvider serviceProvider) =>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SnapshotCollectorTelemetryProcessorFactory.SnapshotCollectorTelemetryProcessorFactory(IServiceProvider)'
            _serviceProvider = serviceProvider;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SnapshotCollectorTelemetryProcessorFactory.Create(ITelemetryProcessor)'
        public ITelemetryProcessor Create(ITelemetryProcessor next)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SnapshotCollectorTelemetryProcessorFactory.Create(ITelemetryProcessor)'
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
