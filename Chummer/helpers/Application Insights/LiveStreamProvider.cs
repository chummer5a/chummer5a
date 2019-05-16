using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;

namespace Chummer
{
    public class LiveStreamProvider
    {
        private readonly TelemetryConfiguration _configuration;

        public LiveStreamProvider(TelemetryConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public void Enable()
        {
            QuickPulseTelemetryProcessor processor = null;

            _configuration.TelemetryProcessorChainBuilder
                .Use(next =>
                {
                    processor = new QuickPulseTelemetryProcessor(next);
                    return processor;
                })
                .Build();

            var quickPulse = new QuickPulseTelemetryModule();
            quickPulse.Initialize(_configuration);
            quickPulse.RegisterTelemetryProcessor(processor);
        }
    }
}
