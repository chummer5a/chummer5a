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
    public static class WaitHandleExtensions
    {
        /// <summary>
        /// Version of WaitOne with cancellation token support taken from the following blog:
        /// https://thomaslevesque.com/2015/06/04/async-and-cancellation-support-for-wait-handles/
        /// </summary>
        public static bool WaitOne(this WaitHandle objHandle, int intTimeout, CancellationToken token)
        {
            if (token == default)
                return objHandle.WaitOne(intTimeout);
            token.ThrowIfCancellationRequested();
            switch (WaitHandle.WaitAny(new[] { objHandle, token.WaitHandle }, intTimeout))
            {
                case WaitHandle.WaitTimeout:
                    return false;

                case 0:
                    return true;

                default:
                    token.ThrowIfCancellationRequested();
                    return false; // never reached
            }
        }

        /// <summary>
        /// Version of WaitOne with cancellation token support taken from the following blog:
        /// https://thomaslevesque.com/2015/06/04/async-and-cancellation-support-for-wait-handles/
        /// </summary>
        public static bool WaitOne(this WaitHandle objHandle, TimeSpan objTimeout, CancellationToken token)
        {
            if (token == default)
                objHandle.WaitOne(objTimeout);
            token.ThrowIfCancellationRequested();
            return objHandle.WaitOne((int)objTimeout.TotalMilliseconds, token);
        }

        /// <summary>
        /// Version of WaitOne with cancellation token support taken from the following blog:
        /// https://thomaslevesque.com/2015/06/04/async-and-cancellation-support-for-wait-handles/
        /// </summary>
        public static bool WaitOne(this WaitHandle objHandle, CancellationToken token)
        {
            if (token == default)
                objHandle.WaitOne();
            token.ThrowIfCancellationRequested();
            return objHandle.WaitOne(Timeout.Infinite, token);
        }

        /// <summary>
        /// Async version of WaitOne taken from the following blog:
        /// https://thomaslevesque.com/2015/06/04/async-and-cancellation-support-for-wait-handles/
        /// </summary>
        public static async Task<bool> WaitOneAsync(this WaitHandle objHandle, int intTimeout, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            RegisteredWaitHandle objRegisteredHandle = null;
            CancellationTokenRegistration objTokenRegistration = default;
            try
            {
                TaskCompletionSource<bool> objTaskCompletionSource = new TaskCompletionSource<bool>();
                objRegisteredHandle = ThreadPool.RegisterWaitForSingleObject(
                    objHandle,
                    (objState, blnTimedOut) => ((TaskCompletionSource<bool>)objState).TrySetResult(!blnTimedOut),
                    objTaskCompletionSource,
                    intTimeout,
                    true);
                objTokenRegistration = token.Register(
                    objState => ((TaskCompletionSource<bool>)objState).TrySetCanceled(token),
                    objTaskCompletionSource);
                return await objTaskCompletionSource.Task.ConfigureAwait(false);
            }
            finally
            {
                objRegisteredHandle?.Unregister(objHandle);
                if (objTokenRegistration != default)
                    objTokenRegistration.Dispose();
            }
        }

        /// <summary>
        /// Async version of WaitOne taken from the following blog:
        /// https://thomaslevesque.com/2015/06/04/async-and-cancellation-support-for-wait-handles/
        /// </summary>
        public static Task<bool> WaitOneAsync(this WaitHandle objHandle, TimeSpan objTimeout, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return objHandle.WaitOneAsync((int)objTimeout.TotalMilliseconds, token);
        }

        /// <summary>
        /// Async version of WaitOne taken from the following blog:
        /// https://thomaslevesque.com/2015/06/04/async-and-cancellation-support-for-wait-handles/
        /// </summary>
        public static Task<bool> WaitOneAsync(this WaitHandle objHandle, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return objHandle.WaitOneAsync(Timeout.Infinite, token);
        }
    }
}
