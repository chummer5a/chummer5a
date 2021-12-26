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
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Chummer
{
    [CLSCompliant(false)]
    public sealed class CustomActivity : Activity
    {
        [CLSCompliant(false)]
        //public IOperationHolder<DependencyTelemetry> myOperationDependencyHolder { get; set; }
        //public IOperationHolder<RequestTelemetry> myOperationRequestHolder { get; set; }
        public TelemetryClient MyTelemetryClient { get; set; }
        [CLSCompliant(false)]
        public DependencyTelemetry MyDependencyTelemetry { get; private set; }
        [CLSCompliant(false)]
        public RequestTelemetry MyRequestTelemetry { get; private set; }
        public string MyTelemetryTarget { get; private set; }

        public enum OperationType
        {
            DependencyOperation,
            RequestOperation,
            PageViewOperation
        }

        public OperationType MyOperationType { get; set; }

        public CustomActivity(string operationName, CustomActivity parentActivity, OperationType operationType, string target) : base(operationName)
        {
            MyOperationType = operationType;
            MyTelemetryTarget = target;
            if (parentActivity != null)
            {
                SetParent(operationName, parentActivity);
                Start();
                return;
            }
            MyTelemetryClient = new TelemetryClient();
            Start();
            switch (operationType)
            {
                case OperationType.DependencyOperation:
                    MyDependencyTelemetry = new DependencyTelemetry(operationName, MyTelemetryTarget, null, null, DateTimeOffset.UtcNow, TimeSpan.Zero, "not disposed", true);
                    MyDependencyTelemetry.Context.Operation.Id = Id;
                    MyTelemetryClient.Context.Operation.Id = MyDependencyTelemetry.Context.Operation.Id;
                    break;

                case OperationType.RequestOperation:
                    MyRequestTelemetry = new RequestTelemetry(operationName, DateTimeOffset.UtcNow, TimeSpan.Zero, "not disposed", true);
                    MyRequestTelemetry.Context.Operation.Id = Id;
                    MyTelemetryClient.Context.Operation.Id = MyRequestTelemetry.Context.Operation.Id;
                    if (!string.IsNullOrEmpty(MyTelemetryTarget) && Uri.TryCreate(MyTelemetryTarget, UriKind.Absolute, out Uri uriResult))
                        MyRequestTelemetry.Url = uriResult;
                    break;

                default:
                    throw new NotImplementedException("Implement OperationType " + operationType);
            }
        }

        private void SetParent(string operationName, CustomActivity parentActivity)
        {
            if (parentActivity != null)
            {
                MyOperationType = parentActivity.MyOperationType;
                SetParentId(parentActivity.Id ?? string.Empty);
                MyTelemetryClient = parentActivity.MyTelemetryClient;
                MyTelemetryTarget = parentActivity.MyTelemetryTarget;
                switch (MyOperationType)
                {
                    case OperationType.DependencyOperation:
                        MyDependencyTelemetry = new DependencyTelemetry(operationName, null, operationName, null, DateTimeOffset.UtcNow, TimeSpan.Zero, "not disposed", true);
                        MyDependencyTelemetry.Context.Operation.ParentId = ParentId;
                        break;

                    case OperationType.RequestOperation:
                        MyRequestTelemetry = new RequestTelemetry(operationName, DateTimeOffset.UtcNow, TimeSpan.Zero, "not disposed", true);
                        MyRequestTelemetry.Context.Operation.ParentId = ParentId;
                        if (!string.IsNullOrEmpty(MyTelemetryTarget) && Uri.TryCreate(MyTelemetryTarget, UriKind.Absolute, out Uri uriResult))
                            MyRequestTelemetry.Url = uriResult;
                        break;

                    default:
                        throw new NotImplementedException("Implement OperationType " + parentActivity.MyOperationType);
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(parentActivity), "operationType must be supplied in case of parentActivity == null!");
            }
        }

        public CustomActivity(string operationName, CustomActivity parentActivity) : base(operationName)
        {
            SetParent(operationName, parentActivity);
            Start();
        }

        public void SetSuccess(bool success)
        {
            if (MyDependencyTelemetry != null)
                MyDependencyTelemetry.Success = success;
            if (MyRequestTelemetry != null)
                MyRequestTelemetry.Success = success;
        }

        private bool _blnDisposed;

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            if (_blnDisposed)
                return;

            Timekeeper.Finish(OperationName);
            switch (MyOperationType)
            {
                case OperationType.DependencyOperation:
                    MyDependencyTelemetry.Duration = DateTimeOffset.UtcNow - MyDependencyTelemetry.Timestamp;
                    if (MyDependencyTelemetry.ResultCode == "not disposed")
                        MyDependencyTelemetry.ResultCode = "OK";
                    MyTelemetryClient.TrackDependency(MyDependencyTelemetry);
                    break;

                case OperationType.RequestOperation:
                    MyRequestTelemetry.Duration = DateTimeOffset.UtcNow - MyRequestTelemetry.Timestamp;
                    if (MyRequestTelemetry.ResponseCode == "not disposed")
                        MyRequestTelemetry.ResponseCode = "OK";
                    MyTelemetryClient.TrackRequest(MyRequestTelemetry);
                    break;

                default:
                    throw new NotImplementedException("Implement OperationType " + OperationName);
            }

            _blnDisposed = true;
        }
    }
}
