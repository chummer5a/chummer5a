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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chummer.Annotations;

namespace Chummer
{
    public interface IAsyncEnumerableWithSideEffects<T> : IAsyncEnumerable<T>
    {
        IEnumerator<T> EnumerateWithSideEffects();

        Task<IEnumerator<T>> EnumerateWithSideEffectsAsync(CancellationToken token = default);
    }

    public static class AsyncEnumerableWithSideEffectsExtensions
    {
        public static IEnumerable<T> AsEnumerableWithSideEffects<T>(this IAsyncEnumerableWithSideEffects<T> objInput)
        {
            using (IEnumerator<T> objEnumerator = objInput.EnumerateWithSideEffects())
            {
                while (objEnumerator.MoveNext())
                    yield return objEnumerator.Current;
            }
        }

        public static async Task ForEachWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Action<T> objFuncToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    objFuncToRun.Invoke(objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
        }

        public static async Task ForEachWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task> objFuncToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    await objFuncToRun.Invoke(objEnumerator.Current).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
        }

        public static async Task ForEachWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Action<T> objFuncToRun, CancellationToken token = default)
        {
            await ForEachWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), objFuncToRun, token).ConfigureAwait(false);
        }

        public static async Task ForEachWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task> objFuncToRun, CancellationToken token = default)
        {
            await ForEachWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), objFuncToRun, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform a synchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="objEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static void ForEachWithSideEffectsWithBreak<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            foreach (T objItem in objEnumerable.AsEnumerableWithSideEffects())
            {
                token.ThrowIfCancellationRequested();
                if (!objFuncToRunWithPossibleTerminate.Invoke(objItem))
                    return;
            }
        }

        /// <summary>
        /// Perform a synchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="objEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static void ForEachWithSideEffectsWithBreak<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, CancellationToken, bool> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            foreach (T objItem in objEnumerable.AsEnumerableWithSideEffects())
            {
                token.ThrowIfCancellationRequested();
                if (!objFuncToRunWithPossibleTerminate.Invoke(objItem, token))
                    return;
            }
        }

        /// <summary>
        /// Perform a synchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="objEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachWithSideEffectsWithBreakAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (!objFuncToRunWithPossibleTerminate.Invoke(objEnumerator.Current))
                        return;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
        }

        /// <summary>
        /// Perform an asynchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="objEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachWithSideEffectsWithBreakAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (!await objFuncToRunWithPossibleTerminate.Invoke(objEnumerator.Current).ConfigureAwait(false))
                        return;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
        }

        /// <summary>
        /// Perform a synchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="tskEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachWithSideEffectsWithBreakAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            await ForEachWithSideEffectsWithBreakAsync(await tskEnumerable.ConfigureAwait(false), objFuncToRunWithPossibleTerminate, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform an asynchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="tskEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachWithSideEffectsWithBreakAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            await ForEachWithSideEffectsWithBreakAsync(await tskEnumerable.ConfigureAwait(false), objFuncToRunWithPossibleTerminate, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform a synchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="objEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachWithSideEffectsWithBreakAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, CancellationToken, bool> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (!objFuncToRunWithPossibleTerminate.Invoke(objEnumerator.Current, token))
                        return;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
        }

        /// <summary>
        /// Perform an asynchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="objEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachWithSideEffectsWithBreakAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, CancellationToken, Task<bool>> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (!await objFuncToRunWithPossibleTerminate.Invoke(objEnumerator.Current, token).ConfigureAwait(false))
                        return;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
        }

        /// <summary>
        /// Perform a synchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="tskEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachWithSideEffectsWithBreakAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, CancellationToken, bool> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            await ForEachWithSideEffectsWithBreakAsync(await tskEnumerable.ConfigureAwait(false), objFuncToRunWithPossibleTerminate, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform an asynchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="tskEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachWithSideEffectsWithBreakAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, CancellationToken, Task<bool>> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            await ForEachWithSideEffectsWithBreakAsync(await tskEnumerable.ConfigureAwait(false), objFuncToRunWithPossibleTerminate, token).ConfigureAwait(false);
        }

        public static async Task ForEachWithSideEffectsParallelAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Action<T> objFuncToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task>(Utils.MaxParallelBatchSize);
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => objFuncToRun.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        lstTasks.Clear();
                    }
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks).ConfigureAwait(false);
        }

        public static async Task ForEachWithSideEffectsParallelAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task> objFuncToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task>(Utils.MaxParallelBatchSize);
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => objFuncToRun.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        lstTasks.Clear();
                    }
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            token.ThrowIfCancellationRequested();
            await Task.WhenAll(lstTasks).ConfigureAwait(false);
        }

        public static async Task ForEachWithSideEffectsParallelAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Action<T> objFuncToRun, CancellationToken token = default)
        {
            await ForEachWithSideEffectsParallelAsync(await tskEnumerable.ConfigureAwait(false), objFuncToRun, token).ConfigureAwait(false);
        }

        public static async Task ForEachWithSideEffectsParallelAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task> objFuncToRun, CancellationToken token = default)
        {
            await ForEachWithSideEffectsParallelAsync(await tskEnumerable.ConfigureAwait(false), objFuncToRun, token).ConfigureAwait(false);
        }

        public static async Task ForEachWithSideEffectsParallelWithBreakAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (CancellationTokenSource objSource = new CancellationTokenSource())
            // ReSharper disable once AccessToDisposedClosure
            using (token.Register(() => objSource.Cancel(false)))
            {
                CancellationToken objToken = objSource.Token;
                List<Task> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                    ? new List<Task>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(objToken).ConfigureAwait(false)))
                    : new List<Task>(Utils.MaxParallelBatchSize);
                IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
                try
                {
                    bool blnMoveNext = objEnumerator.MoveNext();
                    while (blnMoveNext)
                    {
                        for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                        {
                            token.ThrowIfCancellationRequested();
                            T objCurrent = objEnumerator.Current;
                            lstTasks.Add(DoLoopTask());
                            async Task DoLoopTask()
                            {
                                bool blnReturn = await Task.Run(() => objFuncToRunWithPossibleTerminate.Invoke(objCurrent), objToken).ConfigureAwait(false);
                                if (blnReturn)
                                    // ReSharper disable once AccessToDisposedClosure
                                    objSource.Cancel(false);
                            }
                            blnMoveNext = objEnumerator.MoveNext();
                        }

                        if (blnMoveNext)
                        {
                            token.ThrowIfCancellationRequested();
                            try
                            {
                                await Task.WhenAll(lstTasks).ConfigureAwait(false);
                            }
                            catch (OperationCanceledException)
                            {
                                token.ThrowIfCancellationRequested();
                                return;
                            }
                            token.ThrowIfCancellationRequested();
                            lstTasks.Clear();
                        }
                    }
                }
                finally
                {
                    if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                        await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                    else
                        objEnumerator.Dispose();
                }

                try
                {
                    await Task.WhenAll(lstTasks).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    token.ThrowIfCancellationRequested();
                }
            }
        }

        public static async Task ForEachWithSideEffectsParallelWithBreakAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (CancellationTokenSource objSource = new CancellationTokenSource())
            // ReSharper disable once AccessToDisposedClosure
            using (token.Register(() => objSource.Cancel(false)))
            {
                CancellationToken objToken = objSource.Token;
                List<Task> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                    ? new List<Task>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(objToken).ConfigureAwait(false)))
                    : new List<Task>(Utils.MaxParallelBatchSize);
                IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
                try
                {
                    bool blnMoveNext = objEnumerator.MoveNext();
                    while (blnMoveNext)
                    {
                        for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                        {
                            token.ThrowIfCancellationRequested();
                            T objCurrent = objEnumerator.Current;
                            lstTasks.Add(DoLoopTask());
                            async Task DoLoopTask()
                            {
                                bool blnReturn = await Task.Run(() => objFuncToRunWithPossibleTerminate.Invoke(objCurrent), objToken).ConfigureAwait(false);
                                if (blnReturn)
                                    // ReSharper disable once AccessToDisposedClosure
                                    objSource.Cancel(false);
                            }
                            blnMoveNext = objEnumerator.MoveNext();
                        }

                        if (blnMoveNext)
                        {
                            token.ThrowIfCancellationRequested();
                            try
                            {
                                await Task.WhenAll(lstTasks).ConfigureAwait(false);
                            }
                            catch (OperationCanceledException)
                            {
                                token.ThrowIfCancellationRequested();
                                return;
                            }
                            token.ThrowIfCancellationRequested();
                            lstTasks.Clear();
                        }
                    }
                }
                finally
                {
                    if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                        await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                    else
                        objEnumerator.Dispose();
                }

                try
                {
                    await Task.WhenAll(lstTasks).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    token.ThrowIfCancellationRequested();
                }
            }
        }

        public static async Task ForEachWithSideEffectsParallelWithBreakAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            await ForEachWithSideEffectsParallelWithBreakAsync(await tskEnumerable.ConfigureAwait(false), objFuncToRunWithPossibleTerminate, token).ConfigureAwait(false);
        }

        public static async Task ForEachWithSideEffectsParallelWithBreakAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            await ForEachWithSideEffectsParallelWithBreakAsync(await tskEnumerable.ConfigureAwait(false), objFuncToRunWithPossibleTerminate, token).ConfigureAwait(false);
        }

        public static async Task ForEachWithSideEffectsParallelWithBreakAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, CancellationToken, bool> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (CancellationTokenSource objSource = new CancellationTokenSource())
            // ReSharper disable once AccessToDisposedClosure
            using (token.Register(() => objSource.Cancel(false)))
            {
                CancellationToken objToken = objSource.Token;
                List<Task> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                    ? new List<Task>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(objToken).ConfigureAwait(false)))
                    : new List<Task>(Utils.MaxParallelBatchSize);
                IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
                try
                {
                    bool blnMoveNext = objEnumerator.MoveNext();
                    while (blnMoveNext)
                    {
                        for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                        {
                            token.ThrowIfCancellationRequested();
                            T objCurrent = objEnumerator.Current;
                            lstTasks.Add(DoLoopTask());
                            async Task DoLoopTask()
                            {
                                bool blnReturn = await Task.Run(() => objFuncToRunWithPossibleTerminate.Invoke(objCurrent, objToken), objToken).ConfigureAwait(false);
                                if (blnReturn)
                                    // ReSharper disable once AccessToDisposedClosure
                                    objSource.Cancel(false);
                            }
                            blnMoveNext = objEnumerator.MoveNext();
                        }

                        if (blnMoveNext)
                        {
                            token.ThrowIfCancellationRequested();
                            try
                            {
                                await Task.WhenAll(lstTasks).ConfigureAwait(false);
                            }
                            catch (OperationCanceledException)
                            {
                                token.ThrowIfCancellationRequested();
                                return;
                            }
                            token.ThrowIfCancellationRequested();
                            lstTasks.Clear();
                        }
                    }
                }
                finally
                {
                    if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                        await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                    else
                        objEnumerator.Dispose();
                }

                try
                {
                    await Task.WhenAll(lstTasks).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    token.ThrowIfCancellationRequested();
                }
            }
        }

        public static async Task ForEachWithSideEffectsParallelWithBreakAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, CancellationToken, Task<bool>> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (CancellationTokenSource objSource = new CancellationTokenSource())
            // ReSharper disable once AccessToDisposedClosure
            using (token.Register(() => objSource.Cancel(false)))
            {
                CancellationToken objToken = objSource.Token;
                List<Task> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                    ? new List<Task>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(objToken).ConfigureAwait(false)))
                    : new List<Task>(Utils.MaxParallelBatchSize);
                IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
                try
                {
                    bool blnMoveNext = objEnumerator.MoveNext();
                    while (blnMoveNext)
                    {
                        for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                        {
                            token.ThrowIfCancellationRequested();
                            T objCurrent = objEnumerator.Current;
                            lstTasks.Add(DoLoopTask());
                            async Task DoLoopTask()
                            {
                                bool blnReturn = await Task.Run(() => objFuncToRunWithPossibleTerminate.Invoke(objCurrent, objToken), objToken).ConfigureAwait(false);
                                if (blnReturn)
                                    // ReSharper disable once AccessToDisposedClosure
                                    objSource.Cancel(false);
                            }
                            blnMoveNext = objEnumerator.MoveNext();
                        }

                        if (blnMoveNext)
                        {
                            token.ThrowIfCancellationRequested();
                            try
                            {
                                await Task.WhenAll(lstTasks).ConfigureAwait(false);
                            }
                            catch (OperationCanceledException)
                            {
                                token.ThrowIfCancellationRequested();
                                return;
                            }
                            token.ThrowIfCancellationRequested();
                            lstTasks.Clear();
                        }
                    }
                }
                finally
                {
                    if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                        await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                    else
                        objEnumerator.Dispose();
                }

                try
                {
                    await Task.WhenAll(lstTasks).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    token.ThrowIfCancellationRequested();
                }
            }
        }

        public static async Task ForEachWithSideEffectsParallelWithBreakAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, CancellationToken, bool> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            await ForEachWithSideEffectsParallelWithBreakAsync(await tskEnumerable.ConfigureAwait(false), objFuncToRunWithPossibleTerminate, token).ConfigureAwait(false);
        }

        public static async Task ForEachWithSideEffectsParallelWithBreakAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, CancellationToken, Task<bool>> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            await ForEachWithSideEffectsParallelWithBreakAsync(await tskEnumerable.ConfigureAwait(false), objFuncToRunWithPossibleTerminate, token).ConfigureAwait(false);
        }
    }
}
