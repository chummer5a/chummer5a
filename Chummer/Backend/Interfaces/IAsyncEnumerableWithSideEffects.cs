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
            using (new FetchSafelyFromSafeObjectPool<List<Task>>(Utils.TaskListPool, out List<Task> lstTasks))
            {
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
                    token.ThrowIfCancellationRequested();
                    await Task.WhenAll(lstTasks).ConfigureAwait(false);
                }
                finally
                {
                    if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                        await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                    else
                        objEnumerator.Dispose();
                }
            }
        }

        public static async Task ForEachWithSideEffectsParallelAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task> objFuncToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (new FetchSafelyFromSafeObjectPool<List<Task>>(Utils.TaskListPool, out List<Task> lstTasks))
            {
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
                            lstTasks.Add(objFuncToRun.Invoke(objCurrent));
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
                    token.ThrowIfCancellationRequested();
                    await Task.WhenAll(lstTasks).ConfigureAwait(false);
                }
                finally
                {
                    if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                        await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                    else
                        objEnumerator.Dispose();
                }
            }
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
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                using (new FetchSafelyFromSafeObjectPool<List<Task>>(Utils.TaskListPool, out List<Task> lstTasks))
                using (CancellationTokenSource objBreakSource = new CancellationTokenSource())
                using (CancellationTokenSource objLinkedSource = CancellationTokenSource.CreateLinkedTokenSource(objBreakSource.Token, token))
                {
                    token.ThrowIfCancellationRequested();
                    CancellationToken objLinkedToken = objLinkedSource.Token;
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
                                if (objBreakSource.IsCancellationRequested)
                                    return;
                                bool blnReturn = await Task.Run(() => objFuncToRunWithPossibleTerminate.Invoke(objCurrent), objLinkedToken).ConfigureAwait(false);
                                if (!blnReturn)
                                    // ReSharper disable once AccessToDisposedClosure
                                    objBreakSource.Cancel(false);
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
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
        }

        public static async Task ForEachWithSideEffectsParallelWithBreakAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                using (new FetchSafelyFromSafeObjectPool<List<Task>>(Utils.TaskListPool, out List<Task> lstTasks))
                using (CancellationTokenSource objBreakSource = new CancellationTokenSource())
                using (CancellationTokenSource objLinkedSource = CancellationTokenSource.CreateLinkedTokenSource(objBreakSource.Token, token))
                {
                    token.ThrowIfCancellationRequested();
                    CancellationToken objLinkedToken = objLinkedSource.Token;
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
                                if (objBreakSource.IsCancellationRequested)
                                    return;
                                bool blnReturn = await objFuncToRunWithPossibleTerminate.Invoke(objCurrent).ConfigureAwait(false);
                                if (!blnReturn)
                                    // ReSharper disable once AccessToDisposedClosure
                                    objBreakSource.Cancel(false);
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
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
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
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                using (new FetchSafelyFromSafeObjectPool<List<Task>>(Utils.TaskListPool, out List<Task> lstTasks))
                using (CancellationTokenSource objBreakSource = new CancellationTokenSource())
                using (CancellationTokenSource objLinkedSource = CancellationTokenSource.CreateLinkedTokenSource(objBreakSource.Token, token))
                {
                    token.ThrowIfCancellationRequested();
                    CancellationToken objLinkedToken = objLinkedSource.Token;
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
                                if (objBreakSource.IsCancellationRequested)
                                    return;
                                bool blnReturn = await Task.Run(() => objFuncToRunWithPossibleTerminate.Invoke(objCurrent, objLinkedToken), objLinkedToken).ConfigureAwait(false);
                                if (!blnReturn)
                                    // ReSharper disable once AccessToDisposedClosure
                                    objBreakSource.Cancel(false);
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
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
        }

        public static async Task ForEachWithSideEffectsParallelWithBreakAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, CancellationToken, Task<bool>> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                using (new FetchSafelyFromSafeObjectPool<List<Task>>(Utils.TaskListPool, out List<Task> lstTasks))
                using (CancellationTokenSource objBreakSource = new CancellationTokenSource())
                using (CancellationTokenSource objLinkedSource = CancellationTokenSource.CreateLinkedTokenSource(objBreakSource.Token, token))
                {
                    token.ThrowIfCancellationRequested();
                    CancellationToken objLinkedToken = objLinkedSource.Token;
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
                                if (objBreakSource.IsCancellationRequested)
                                    return;
                                bool blnReturn = await objFuncToRunWithPossibleTerminate.Invoke(objCurrent, objLinkedToken).ConfigureAwait(false);
                                if (!blnReturn)
                                    // ReSharper disable once AccessToDisposedClosure
                                    objBreakSource.Cancel(false);
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
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
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

        public static async Task<int> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    intReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return intReturn;
        }

        public static async Task<int> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    intReturn += await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return intReturn;
        }

        public static async Task<int> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    lngReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return lngReturn;
        }

        public static async Task<long> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    lngReturn += await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return lngReturn;
        }

        public static async Task<long> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    fltReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return fltReturn;
        }

        public static async Task<float> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    fltReturn += await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return fltReturn;
        }

        public static async Task<float> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    dblReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return dblReturn;
        }

        public static async Task<double> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    dblReturn += await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return dblReturn;
        }

        public static async Task<double> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    decReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return decReturn;
        }

        public static async Task<decimal> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    decReturn += await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return decReturn;
        }

        public static async Task<decimal> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
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
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn += await tskLoop.ConfigureAwait(false);
            return intReturn;
        }

        public static async Task<int> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
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
                        lstTasks.Add(funcSelector.Invoke(objCurrent));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn += await tskLoop.ConfigureAwait(false);
            return intReturn;
        }

        public static async Task<int> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
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
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn += await tskLoop.ConfigureAwait(false);
            return lngReturn;
        }

        public static async Task<long> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
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
                        lstTasks.Add(funcSelector.Invoke(objCurrent));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn += await tskLoop.ConfigureAwait(false);
            return lngReturn;
        }

        public static async Task<long> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
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
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn += await tskLoop.ConfigureAwait(false);
            return fltReturn;
        }

        public static async Task<float> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
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
                        lstTasks.Add(funcSelector.Invoke(objCurrent));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn += await tskLoop.ConfigureAwait(false);
            return fltReturn;
        }

        public static async Task<float> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
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
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn += await tskLoop.ConfigureAwait(false);
            return dblReturn;
        }

        public static async Task<double> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
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
                        lstTasks.Add(funcSelector.Invoke(objCurrent));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn += await tskLoop.ConfigureAwait(false);
            return dblReturn;
        }

        public static async Task<double> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
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
                        lstTasks.Add(Task.Run(() => funcSelector.Invoke(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn += await tskLoop.ConfigureAwait(false);
            return decReturn;
        }

        public static async Task<decimal> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
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
                        lstTasks.Add(funcSelector.Invoke(objCurrent));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn += await tskLoop.ConfigureAwait(false);
            return decReturn;
        }

        public static async Task<decimal> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        intReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return intReturn;
        }

        public static async Task<int> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        intReturn += await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return intReturn;
        }

        public static async Task<int> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        lngReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return lngReturn;
        }

        public static async Task<long> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        lngReturn += await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return lngReturn;
        }

        public static async Task<long> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        fltReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return fltReturn;
        }

        public static async Task<float> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        fltReturn += await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return fltReturn;
        }

        public static async Task<float> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        dblReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return dblReturn;
        }

        public static async Task<double> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        dblReturn += await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return dblReturn;
        }

        public static async Task<double> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        decReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return decReturn;
        }

        public static async Task<decimal> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        decReturn += await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return decReturn;
        }

        public static async Task<decimal> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
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
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn += await tskLoop.ConfigureAwait(false);
            return intReturn;
        }

        public static async Task<int> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
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
                        lstTasks.Add(GetValue(objCurrent, token));
                        async Task<int> GetValue(T objInnerCurrent, CancellationToken innerToken)
                        {
                            innerToken.ThrowIfCancellationRequested();
                            if (funcPredicate(objInnerCurrent))
                                return await funcSelector.Invoke(objInnerCurrent).ConfigureAwait(false);
                            return default;
                        }
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn += await tskLoop.ConfigureAwait(false);
            return intReturn;
        }

        public static async Task<int> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
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
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn += await tskLoop.ConfigureAwait(false);
            return lngReturn;
        }

        public static async Task<long> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
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
                        lstTasks.Add(GetValue(objCurrent, token));
                        async Task<long> GetValue(T objInnerCurrent, CancellationToken innerToken)
                        {
                            innerToken.ThrowIfCancellationRequested();
                            if (funcPredicate(objInnerCurrent))
                                return await funcSelector.Invoke(objInnerCurrent).ConfigureAwait(false);
                            return default;
                        }
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn += await tskLoop.ConfigureAwait(false);
            return lngReturn;
        }

        public static async Task<long> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
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
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn += await tskLoop.ConfigureAwait(false);
            return fltReturn;
        }

        public static async Task<float> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
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
                        lstTasks.Add(GetValue(objCurrent, token));
                        async Task<float> GetValue(T objInnerCurrent, CancellationToken innerToken)
                        {
                            innerToken.ThrowIfCancellationRequested();
                            if (funcPredicate(objInnerCurrent))
                                return await funcSelector.Invoke(objInnerCurrent).ConfigureAwait(false);
                            return default;
                        }
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn += await tskLoop.ConfigureAwait(false);
            return fltReturn;
        }

        public static async Task<float> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
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
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn += await tskLoop.ConfigureAwait(false);
            return dblReturn;
        }

        public static async Task<double> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
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
                        lstTasks.Add(GetValue(objCurrent, token));
                        async Task<double> GetValue(T objInnerCurrent, CancellationToken innerToken)
                        {
                            innerToken.ThrowIfCancellationRequested();
                            if (funcPredicate(objInnerCurrent))
                                return await funcSelector.Invoke(objInnerCurrent).ConfigureAwait(false);
                            return default;
                        }
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn += await tskLoop.ConfigureAwait(false);
            return dblReturn;
        }

        public static async Task<double> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
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
                        lstTasks.Add(Task.Run(() => funcPredicate(objCurrent) ? funcSelector.Invoke(objCurrent) : 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn += await tskLoop.ConfigureAwait(false);
            return decReturn;
        }

        public static async Task<decimal> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
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
                        lstTasks.Add(GetValue(objCurrent, token));
                        async Task<decimal> GetValue(T objInnerCurrent, CancellationToken innerToken)
                        {
                            innerToken.ThrowIfCancellationRequested();
                            if (funcPredicate(objInnerCurrent))
                                return await funcSelector.Invoke(objInnerCurrent).ConfigureAwait(false);
                            return default;
                        }
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn += await tskLoop.ConfigureAwait(false);
            return decReturn;
        }

        public static async Task<decimal> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        intReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return intReturn;
        }

        public static async Task<int> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        intReturn += await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return intReturn;
        }

        public static async Task<int> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        lngReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return lngReturn;
        }

        public static async Task<long> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        lngReturn += await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return lngReturn;
        }

        public static async Task<long> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        fltReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return fltReturn;
        }

        public static async Task<float> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        fltReturn += await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return fltReturn;
        }

        public static async Task<float> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        dblReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return dblReturn;
        }

        public static async Task<double> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        dblReturn += await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return dblReturn;
        }

        public static async Task<double> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        decReturn += funcSelector.Invoke(objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return decReturn;
        }

        public static async Task<decimal> SumWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.EnumerateWithSideEffectsAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        decReturn += await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return decReturn;
        }

        public static async Task<decimal> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await SumWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
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
                        lstTasks.Add(GetValue(objCurrent, token));
                        async Task<int> GetValue(T objInnerCurrent, CancellationToken innerToken)
                        {
                            innerToken.ThrowIfCancellationRequested();
                            if (await funcPredicate(objInnerCurrent).ConfigureAwait(false))
                                return funcSelector.Invoke(objInnerCurrent);
                            return default;
                        }
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn += await tskLoop.ConfigureAwait(false);
            return intReturn;
        }

        public static async Task<int> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
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
                        lstTasks.Add(GetValue(objCurrent, token));
                        async Task<int> GetValue(T objInnerCurrent, CancellationToken innerToken)
                        {
                            innerToken.ThrowIfCancellationRequested();
                            if (await funcPredicate(objInnerCurrent).ConfigureAwait(false))
                                return await funcSelector.Invoke(objInnerCurrent).ConfigureAwait(false);
                            return default;
                        }
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<int> tskLoop in lstTasks)
                            intReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<int> tskLoop in lstTasks)
                intReturn += await tskLoop.ConfigureAwait(false);
            return intReturn;
        }

        public static async Task<int> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
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
                        lstTasks.Add(GetValue(objCurrent, token));
                        async Task<long> GetValue(T objInnerCurrent, CancellationToken innerToken)
                        {
                            innerToken.ThrowIfCancellationRequested();
                            if (await funcPredicate(objInnerCurrent).ConfigureAwait(false))
                                return funcSelector.Invoke(objInnerCurrent);
                            return default;
                        }
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn += await tskLoop.ConfigureAwait(false);
            return lngReturn;
        }

        public static async Task<long> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
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
                        lstTasks.Add(GetValue(objCurrent, token));
                        async Task<long> GetValue(T objInnerCurrent, CancellationToken innerToken)
                        {
                            innerToken.ThrowIfCancellationRequested();
                            if (await funcPredicate(objInnerCurrent).ConfigureAwait(false))
                                return await funcSelector.Invoke(objInnerCurrent).ConfigureAwait(false);
                            return default;
                        }
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<long> tskLoop in lstTasks)
                            lngReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<long> tskLoop in lstTasks)
                lngReturn += await tskLoop.ConfigureAwait(false);
            return lngReturn;
        }

        public static async Task<long> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
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
                        lstTasks.Add(GetValue(objCurrent, token));
                        async Task<float> GetValue(T objInnerCurrent, CancellationToken innerToken)
                        {
                            innerToken.ThrowIfCancellationRequested();
                            if (await funcPredicate(objInnerCurrent).ConfigureAwait(false))
                                return funcSelector.Invoke(objInnerCurrent);
                            return default;
                        }
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn += await tskLoop.ConfigureAwait(false);
            return fltReturn;
        }

        public static async Task<float> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
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
                        lstTasks.Add(GetValue(objCurrent, token));
                        async Task<float> GetValue(T objInnerCurrent, CancellationToken innerToken)
                        {
                            innerToken.ThrowIfCancellationRequested();
                            if (await funcPredicate(objInnerCurrent).ConfigureAwait(false))
                                return await funcSelector.Invoke(objInnerCurrent).ConfigureAwait(false);
                            return default;
                        }
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<float> tskLoop in lstTasks)
                            fltReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<float> tskLoop in lstTasks)
                fltReturn += await tskLoop.ConfigureAwait(false);
            return fltReturn;
        }

        public static async Task<float> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
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
                        lstTasks.Add(GetValue(objCurrent, token));
                        async Task<double> GetValue(T objInnerCurrent, CancellationToken innerToken)
                        {
                            innerToken.ThrowIfCancellationRequested();
                            if (await funcPredicate(objInnerCurrent).ConfigureAwait(false))
                                return funcSelector.Invoke(objInnerCurrent);
                            return default;
                        }
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn += await tskLoop.ConfigureAwait(false);
            return dblReturn;
        }

        public static async Task<double> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
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
                        lstTasks.Add(GetValue(objCurrent, token));
                        async Task<double> GetValue(T objInnerCurrent, CancellationToken innerToken)
                        {
                            innerToken.ThrowIfCancellationRequested();
                            if (await funcPredicate(objInnerCurrent).ConfigureAwait(false))
                                return await funcSelector.Invoke(objInnerCurrent).ConfigureAwait(false);
                            return default;
                        }
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<double> tskLoop in lstTasks)
                            dblReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<double> tskLoop in lstTasks)
                dblReturn += await tskLoop.ConfigureAwait(false);
            return dblReturn;
        }

        public static async Task<double> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
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
                        lstTasks.Add(GetValue(objCurrent, token));
                        async Task<decimal> GetValue(T objInnerCurrent, CancellationToken innerToken)
                        {
                            innerToken.ThrowIfCancellationRequested();
                            if (await funcPredicate(objInnerCurrent).ConfigureAwait(false))
                                return funcSelector.Invoke(objInnerCurrent);
                            return default;
                        }
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn += await tskLoop.ConfigureAwait(false);
            return decReturn;
        }

        public static async Task<decimal> SumParallelWithSideEffectsAsync<T>(this IAsyncEnumerableWithSideEffects<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
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
                        lstTasks.Add(GetValue(objCurrent, token));
                        async Task<decimal> GetValue(T objInnerCurrent, CancellationToken innerToken)
                        {
                            innerToken.ThrowIfCancellationRequested();
                            if (await funcPredicate(objInnerCurrent).ConfigureAwait(false))
                                return await funcSelector.Invoke(objInnerCurrent).ConfigureAwait(false);
                            return default;
                        }
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        foreach (Task<decimal> tskLoop in lstTasks)
                            decReturn += await tskLoop.ConfigureAwait(false);
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
            token.ThrowIfCancellationRequested();
            foreach (Task<decimal> tskLoop in lstTasks)
                decReturn += await tskLoop.ConfigureAwait(false);
            return decReturn;
        }

        public static async Task<decimal> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumParallelWithSideEffectsAsync<T>(this Task<IAsyncEnumerableWithSideEffects<T>> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            return await SumParallelWithSideEffectsAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }
    }
}
