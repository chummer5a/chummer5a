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

namespace ChummerHub.Client.Backend
{
    public static class TaskCancellationExtension
    {
        /// <summary>
        /// add cancellation functionality to Task with return type
        /// </summary>
        /// <param name="task"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static async Task<T> CancelAfter<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            if (task == null)
                return default;
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cancellationToken);
            return await task;
        }


        /// <summary>
        /// add cancellation functionality to Task with no return type
        /// </summary>
        /// <param name="task"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static async Task CancelAfter(this Task task, CancellationToken cancellationToken)
        {
            if (task == null)
                return;
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cancellationToken);
            await task;
        }


        /// <summary>
        /// add cancellation functionality to Task with return type
        /// </summary>
        /// <param name="task"></param>
        /// <param name="milliseconds"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static async Task<T> CancelAfter<T>(this Task<T> task, int milliseconds)
        {
            if (task == null)
                return default;
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(milliseconds);
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            using (cts.Token.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cts.Token);
            cts.Dispose();
            return await task;
        }


        /// <summary>
        /// add cancellation functionality to Task with no return type
        /// </summary>
        /// <param name="task"></param>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static async Task CancelAfter(this Task task, int milliseconds)
        {
            if (task == null)
                return;
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(milliseconds);
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            using (cts.Token.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cts.Token);
            cts.Dispose();
            await task;
        }
    }
}
