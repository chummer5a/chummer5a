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
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Chummer
{
    

    public class CustomActivity : Activity, IDisposable
    {
        //public IOperationHolder<DependencyTelemetry> myOperationDependencyHolder { get; set; }
        //public IOperationHolder<RequestTelemetry> myOperationRequestHolder { get; set; }
        public TelemetryClient tc { get; set; }
        public DependencyTelemetry MyDependencyTelemetry { get; private set; }
        public RequestTelemetry MyRequestTelemetry { get; private set; }
        public String MyTelemetryTarget { get; private set; }

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
                this.Start();
                return;

            }
            this.tc = new TelemetryClient();
            this.Start();
            switch (operationType)
            {
                case OperationType.DependencyOperation:
                    MyDependencyTelemetry = new DependencyTelemetry(operationName, MyTelemetryTarget, null, null, DateTimeOffset.UtcNow, TimeSpan.Zero, "not disposed", true);
                    MyDependencyTelemetry.Context.Operation.Id = this.Id;
                    tc.Context.Operation.Id = MyDependencyTelemetry.Context.Operation.Id;
                break;
                case OperationType.RequestOperation:
                    MyRequestTelemetry = new RequestTelemetry(operationName, DateTimeOffset.UtcNow, TimeSpan.Zero, "not disposed", true);
                    MyRequestTelemetry.Context.Operation.Id = this.Id;
                    tc.Context.Operation.Id = MyRequestTelemetry.Context.Operation.Id;
                    if (!String.IsNullOrEmpty(MyTelemetryTarget))
                        if (Uri.TryCreate(MyTelemetryTarget, UriKind.Absolute, out Uri Uriresult))
                            MyRequestTelemetry.Url = Uriresult;
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
                this.SetParentId(parentActivity.Id);
                this.tc = parentActivity.tc;
                this.MyTelemetryTarget = parentActivity.MyTelemetryTarget;
                switch (MyOperationType)
                {
                    case OperationType.DependencyOperation:
                        MyDependencyTelemetry = new DependencyTelemetry(operationName, null, operationName, null, DateTimeOffset.UtcNow, TimeSpan.Zero, "not disposed", true);
                        MyDependencyTelemetry.Context.Operation.ParentId = this.ParentId;
                        break;
                    case OperationType.RequestOperation:
                        MyRequestTelemetry = new RequestTelemetry(operationName, DateTimeOffset.UtcNow, TimeSpan.Zero, "not disposed", true);
                        MyRequestTelemetry.Context.Operation.ParentId = this.ParentId;
                        if (!String.IsNullOrEmpty(MyTelemetryTarget))
                            if (Uri.TryCreate(MyTelemetryTarget, UriKind.Absolute, out Uri Uriresult))
                                MyRequestTelemetry.Url = new Uri(MyTelemetryTarget);
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
            this.Start();
        }

        public void SetSuccess(bool success)
        {
            if (MyDependencyTelemetry != null)
                MyDependencyTelemetry.Success = success;
            if (MyRequestTelemetry != null)
                MyRequestTelemetry.Success = success;
        }

        public void Dispose()
        {
            Timekeeper.Finish(this.OperationName);
            this.Stop();
            switch (MyOperationType)
            {
                case OperationType.DependencyOperation:
                    MyDependencyTelemetry.Duration = DateTimeOffset.UtcNow - MyDependencyTelemetry.Timestamp;
                    if (MyDependencyTelemetry.ResultCode == "not disposed")
                        MyDependencyTelemetry.ResultCode = "OK";
                    tc.TrackDependency(MyDependencyTelemetry);
                    break;
                case OperationType.RequestOperation:
                    MyRequestTelemetry.Duration = DateTimeOffset.UtcNow - MyRequestTelemetry.Timestamp;
                    if (MyRequestTelemetry.ResponseCode == "not disposed")
                        MyRequestTelemetry.ResponseCode = "OK";
                    tc.TrackRequest(MyRequestTelemetry);
                    break;
                default:
                    throw new NotImplementedException("Implement OperationType " + OperationName);

            }
        }
    }
}
