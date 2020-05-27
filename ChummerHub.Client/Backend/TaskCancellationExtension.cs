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
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(true))
                    throw new OperationCanceledException(cancellationToken);
            return await task.ConfigureAwait(true);
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
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(true))
                    throw new OperationCanceledException(cancellationToken);
            await task.ConfigureAwait(true);
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
            var cts = new CancellationTokenSource();
            cts.CancelAfter(milliseconds);
            var tcs = new TaskCompletionSource<bool>();
            using (cts.Token.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(true))
                    throw new OperationCanceledException(cts.Token);
            cts.Dispose();
            return await task.ConfigureAwait(true);
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
            var cts = new CancellationTokenSource();
            cts.CancelAfter(milliseconds);
            var tcs = new TaskCompletionSource<bool>();
            using (cts.Token.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(true))
                    throw new OperationCanceledException(cts.Token);
            cts.Dispose();
            await task.ConfigureAwait(true);
        }
    }
}
