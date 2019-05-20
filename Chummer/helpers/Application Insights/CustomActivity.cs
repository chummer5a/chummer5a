using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Chummer
{
    public class CustomActivity : Activity, IDisposable
    {
        public IOperationHolder<DependencyTelemetry> myOperationDependencyHolder { get; set; }
        public IOperationHolder<RequestTelemetry> myOperationRequestHolder { get; set; }

        public CustomActivity(string operationName, CustomActivity parentActivity) : base(operationName)
        {
            if (parentActivity != null)
                this.SetParentId(parentActivity.Id);
            this.Start();
        }

        public void Dispose()
        {
            Timekeeper.Finish(this.OperationName);
            this.Stop();
            if (myOperationDependencyHolder != null)
                Program.ApplicationInsightsTelemetryClient.StopOperation(myOperationDependencyHolder);

            if (myOperationRequestHolder != null)
                Program.ApplicationInsightsTelemetryClient.StopOperation(myOperationRequestHolder);

            Program.ApplicationInsightsTelemetryClient.Flush();
            Thread.Sleep(5);

        }
    }
}
