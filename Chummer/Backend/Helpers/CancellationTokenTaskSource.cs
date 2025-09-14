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
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    /// <summary>
    /// Helper that creates a task that gets canceled when a cancellation token is told to cancel.
    /// From:
    /// https://github.com/StephenCleary/AsyncEx/blob/master/src/Nito.AsyncEx.Tasks/CancellationTokenTaskSource.cs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct CancellationTokenTaskSource<T> : IDisposable, IEquatable<CancellationTokenTaskSource<T>>
    {
        /// <summary>
        /// The cancellation token registration, if any. This is <c>null</c> if the registration was not necessary.
        /// </summary>
        private readonly IDisposable _objTokenRegistration;

        /// <summary>
        /// Creates a task for the specified cancellation token, registering with the token if necessary.
        /// </summary>
        /// <param name="token">The cancellation token to observe.</param>
        public CancellationTokenTaskSource(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                Task = System.Threading.Tasks.Task.FromCanceled<T>(token);
                _objTokenRegistration = null;
                return;
            }
            TaskCompletionSource<T> objTaskCompletionSource = new TaskCompletionSource<T>();
            _objTokenRegistration = token.Register(x => ((TaskCompletionSource<T>)x).TrySetCanceled(token), objTaskCompletionSource, false);
            Task = objTaskCompletionSource.Task;
        }

        /// <summary>
        /// Gets the task for the source cancellation token.
        /// </summary>
        public Task<T> Task { get; }

        /// <summary>
        /// Disposes the cancellation token registration, if any. Note that this may cause <see cref="Task"/> to never complete.
        /// </summary>
        public void Dispose()
        {
            _objTokenRegistration?.Dispose();
        }

        public bool Equals(CancellationTokenTaskSource<T> other)
        {
            return _objTokenRegistration == other._objTokenRegistration;
        }

        public override bool Equals(object obj)
        {
            return obj is CancellationTokenTaskSource<T> objCasted && Equals(objCasted);
        }
        public static bool operator ==(CancellationTokenTaskSource<T> left, CancellationTokenTaskSource<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CancellationTokenTaskSource<T> left, CancellationTokenTaskSource<T> right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return _objTokenRegistration?.GetHashCode() ?? 0;
        }
    }
}
